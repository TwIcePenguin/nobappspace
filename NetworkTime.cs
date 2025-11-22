using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NOBApp
{
    public class NetworkTime
    {
        // 更多樣化的 NTP 伺服器列表
        private static readonly string[] _ntpServers =
        {
            "pool.ntp.org",
            "time.google.com",
            "time.cloudflare.com",
            "time.windows.com",
            "ntp.ntsc.ac.cn",
            "time.tencent.com",
            "time.nist.gov",
            "time.apple.com",
            "time.asia.apple.com"
        };

        // HTTP 備用方案，避免 UDP 123 被擋
        private static readonly string[] _httpUrls =
        {
            "https://www.google.com",
            "https://www.microsoft.com",
            "https://www.cloudflare.com",
            "https://www.baidu.com"
        };

        /// <summary>
        /// 取得網路時間 (同步方法，可能會阻塞 UI，建議改用 GetNowAsync)
        /// </summary>
        public static DateTime GetNetworkTimeAsync()
        {
            // 檢查網路連線狀態，若無網路則直接回傳本地時間，避免無謂的嘗試與錯誤
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return DateTime.Now;
            }

            // 為了不破壞現有同步呼叫的結構，我們在這裡使用 Task.Run 同步等待結果
            try
            {
                return Task.Run(async () => await GetNetworkTimeInternalAsync()).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetNetworkTimeAsync failed: {ex.Message}");
                return DateTime.Now;
            }
        }

        /// <summary>
        /// 非同步取得網路時間 (推薦使用，不會阻塞 UI)
        /// </summary>
        public static async Task<DateTime> GetNowAsync()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return DateTime.Now;
            }

            try
            {
                return await GetNetworkTimeInternalAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetNowAsync failed: {ex.Message}");
                return DateTime.Now;
            }
        }

        private static async Task<DateTime> GetNetworkTimeInternalAsync()
        {
            using var cts = new CancellationTokenSource(4000); // 總體超時 4 秒

            // 1. 並行查詢 NTP
            var ntpTasks = _ntpServers.Select(server => GetNtpTimeAsync(server, cts.Token)).ToList();
            
            try 
            {
                while (ntpTasks.Count > 0)
                {
                    var completedTask = await Task.WhenAny(ntpTasks);
                    ntpTasks.Remove(completedTask);

                    if (completedTask.Status == TaskStatus.RanToCompletion && completedTask.Result > DateTime.MinValue)
                    {
                        // 成功取得時間後，不取消其他任務，避免引發 OperationCanceledException 干擾除錯
                        // cts.Cancel(); 
                        return completedTask.Result;
                    }
                }
            }
            catch
            {
                // 忽略 NTP 階段的錯誤，繼續嘗試 HTTP
            }

            // 2. NTP 全失敗，嘗試 HTTP Header
            var httpTasks = _httpUrls.Select(url => GetHttpTimeAsync(url, cts.Token)).ToList();
            
            try
            {
                while (httpTasks.Count > 0)
                {
                    var completedTask = await Task.WhenAny(httpTasks);
                    httpTasks.Remove(completedTask);

                    if (completedTask.Status == TaskStatus.RanToCompletion && completedTask.Result > DateTime.MinValue)
                    {
                        // 成功後不取消其他任務
                        // cts.Cancel();
                        return completedTask.Result;
                    }
                }
            }
            catch
            {
                // 忽略 HTTP 階段錯誤
            }

            // 3. 全部失敗
            Debug.WriteLine("All network time sources failed.");
            return DateTime.Now;
        }

        private static async Task<DateTime> GetNtpTimeAsync(string server, CancellationToken token)
        {
            using var udpClient = new UdpClient();
            try
            {
                if (token.IsCancellationRequested) return DateTime.MinValue;

                // 使用非同步 DNS 解析
                var addresses = await Dns.GetHostAddressesAsync(server, token);
                if (addresses.Length == 0) return DateTime.MinValue;
                
                var ipEndPoint = new IPEndPoint(addresses[0], 123);
                
                // 設定超時 (UdpClient.Client 是底層 Socket)
                udpClient.Client.SendTimeout = 3000;
                udpClient.Client.ReceiveTimeout = 3000;

                var ntpData = new byte[48];
                ntpData[0] = 0x1B;

                // 連接 (UDP 只是設定預設遠端主機)
                udpClient.Connect(ipEndPoint);
                
                // 發送
                await udpClient.SendAsync(ntpData, ntpData.Length);

                // 接收，使用 Task.WhenAny 實作超時與取消
                var receiveTask = udpClient.ReceiveAsync();
                // 注意: 這裡不傳遞 token 給 Delay，避免引發 TaskCanceledException
                var timeoutTask = Task.Delay(3000); 

                var completedTask = await Task.WhenAny(receiveTask, timeoutTask);
                
                if (completedTask == timeoutTask) return DateTime.MinValue; // 超時

                if (token.IsCancellationRequested) return DateTime.MinValue;

                var result = await receiveTask;
                var receivedData = result.Buffer;

                if (receivedData.Length < 48) return DateTime.MinValue;

                // 解析時間
                const byte serverReplyTime = 40;
                ulong intPart = BitConverter.ToUInt32(receivedData, serverReplyTime);
                ulong fractPart = BitConverter.ToUInt32(receivedData, serverReplyTime + 4);

                intPart = SwapEndianness(intPart);
                fractPart = SwapEndianness(fractPart);

                var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
                var networkDateTime = new DateTime(1900, 1, 1).AddMilliseconds((long)milliseconds);

                return networkDateTime.ToLocalTime();
            }
            catch (OperationCanceledException)
            {
                return DateTime.MinValue;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private static async Task<DateTime> GetHttpTimeAsync(string url, CancellationToken token)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(2);
                var request = new HttpRequestMessage(HttpMethod.Head, url);
                var response = await client.SendAsync(request, token);
                
                if (response.Headers.Date.HasValue)
                {
                    return response.Headers.Date.Value.LocalDateTime;
                }
                return DateTime.MinValue;
            }
            catch (OperationCanceledException)
            {
                return DateTime.MinValue;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000FF) << 24) |
                          ((x & 0x0000FF00) << 8) |
                          ((x & 0x00FF0000) >> 8) |
                          ((x & 0xFF000000) >> 24));
        }
    }
}

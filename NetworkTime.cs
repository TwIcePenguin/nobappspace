using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;

namespace NOBApp
{
    public class NetworkTime
    {
        private static readonly string[] _ntpServers =
        {
            "time.google.com",
            "time.cloudflare.com",
            "time.windows.com",
            "ntp.ntsc.ac.cn",
            "time.tencent.com",
        };

        public static DateTime GetNetworkTimeAsync()
        {
            DateTime onlineTime = DateTime.Now;
            foreach (var server in _ntpServers)
            {
                try
                {
                    // NTP 資料包大小為 48 個字節
                    var ntpData = new byte[48];

                    // 設置 NTP 資料包的起始位元組
                    ntpData[0] = 0x1B;

                    try
                    {
                        // 建立 Socket 連接
                        var addresses = Dns.GetHostEntry(server).AddressList;
                        var ipEndPoint = new IPEndPoint(addresses[0], 123);
                        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                        // 設置 Socket 超時時間
                        socket.ReceiveTimeout = 3000;

                        // 發送 NTP 請求
                        socket.Connect(ipEndPoint);
                        socket.Send(ntpData);

                        // 接收 NTP 回應
                        socket.Receive(ntpData);
                        socket.Close();

                        // 解析時間
                        const byte serverReplyTime = 40;
                        ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
                        ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

                        // 轉換字節順序（由大端到小端）
                        intPart = SwapEndianness(intPart);
                        fractPart = SwapEndianness(fractPart);

                        // 計算毫秒數
                        var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

                        // NTP 時間從 1900 年 1 月 1 日開始
                        var networkDateTime = new DateTime(1900, 1, 1).AddMilliseconds((long)milliseconds);

                        // 返回本地時間
                        return networkDateTime.ToLocalTime();
                    }
                    catch
                    {
                        // 若發生錯誤，返回本地系統時間
                        return DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting time from {server}: {ex.Message}");
                }
            }

            return onlineTime;
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

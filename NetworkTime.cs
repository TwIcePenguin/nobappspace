using System;
using System.Diagnostics;
using NtpClient;

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
                    var timeClient = new NtpConnection(server);
                    onlineTime = timeClient.GetUtc().AddHours(8);
                    return onlineTime;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting time from {server}: {ex.Message}");
                }
            }

            return onlineTime;
        }
    }
}

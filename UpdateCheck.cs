using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace NOBApp
{
    public partial class MainWindow
    {
        private static readonly string GitHubApiUrl = "https://api.github.com/repos/TwIcePenguin/nobapp/releases/latest";
        private static readonly string UpdateUrl = "https://github.com/TwIcePenguin/nobapp/releases/download/{tag}/{filename}.zip";
        private static readonly string UpdateFilePath = "update.zip";

        public static async Task UpdateCheck()
        {
            var release = await GetLatestRelease();
            if (IsUpdateAvailable(release.tag_name))
            {
                Console.WriteLine("有新版本可用，正在下載更新...");
                await UpdateDownloader.DownloadUpdate(release.tag_name);
                Console.WriteLine("更新下載完成，正在應用更新...");
                Process.Start("powershell.exe", "-File ApplyUpdate.ps1");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("當前已是最新版本。");
            }
            // 其他程式邏輯
        }

        private static async Task<GitHubRelease> GetLatestRelease()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
                string json = await client.GetStringAsync(GitHubApiUrl);
                var release = JsonSerializer.Deserialize<GitHubRelease>(json);
                return release;
            }
        }

        private static bool IsUpdateAvailable(string latestVersion)
        {
            return string.Compare(latestVersion, VersionInfo.Version, StringComparison.Ordinal) > 0;
        }

        public class UpdateDownloader
        {
            public static async Task DownloadUpdate(string tag)
            {
                string url = UpdateUrl.Replace("{tag}", tag).Replace("{filename}", "YourReleaseFileName");
                using (HttpClient client = new HttpClient())
                {
                    byte[] updateData = await client.GetByteArrayAsync(url);
                    await File.WriteAllBytesAsync(UpdateFilePath, updateData);
                }
            }
        }

        public class GitHubRelease
        {
            public string tag_name { get; set; } = string.Empty;
        }
    }
}
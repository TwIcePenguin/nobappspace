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
        private static readonly string GitLabApiUrl = "https://gitlab.com/api/v4/projects/{project_id}/releases";
        private static readonly string UpdateUrl = "https://gitlab.com/{owner}/{repo}/-/releases/download/{tag}/{filename}.zip";
        private static readonly string UpdateFilePath = "update.zip";

        public static async Task UpdateCheck()
        {
            if (await UpdateChecker.IsUpdateAvailable())
            {
                Console.WriteLine("有新版本可用，正在下載更新...");
                var release = await GetLatestRelease();
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

        private static async Task<GitLabRelease> GetLatestRelease()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
                string json = await client.GetStringAsync(GitLabApiUrl);
                var releases = JsonSerializer.Deserialize<GitLabRelease[]>(json);
                return releases[0];
            }
        }

        public class UpdateDownloader
        {
            public static async Task DownloadUpdate(string tag)
            {
                string url = UpdateUrl.Replace("{tag}", tag);
                using (HttpClient client = new HttpClient())
                {
                    byte[] updateData = await client.GetByteArrayAsync(url);
                    await File.WriteAllBytesAsync(UpdateFilePath, updateData);
                }
            }
        }

        public class UpdateChecker
        {
            public static async Task<bool> IsUpdateAvailable()
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
                    string json = await client.GetStringAsync(GitLabApiUrl);
                    var releases = JsonSerializer.Deserialize<GitLabRelease[]>(json);
                    var latestRelease = releases[0];
                    return string.Compare(latestRelease.tag_name, VersionInfo.Version, StringComparison.Ordinal) > 0;
                }
            }
        }

        public class GitLabRelease
        {
            public string? tag_name { get; set; }
        }
    }
}
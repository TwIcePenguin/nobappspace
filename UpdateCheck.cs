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
        // 允許從配置文件更新這些 URL
        private static string GitHubApiUrl = "https://api.github.com/repos/TwIcePenguin/nobappspace/releases/latest";
        private static string UpdateUrl = "https://github.com/TwIcePenguin/nobappspace/releases/download/{tag}/{filename}.zip";
        private static readonly string UpdateFilePath = "update.zip";

        private static async Task<GitHubRelease> GetLatestRelease()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
                    client.Timeout = TimeSpan.FromSeconds(10);

                    Debug.WriteLine($"正在檢查 GitHub 更新: {GitHubApiUrl}");
                    string json = await client.GetStringAsync(GitHubApiUrl);
                    var release = JsonSerializer.Deserialize<GitHubRelease>(json);
                    Debug.WriteLine($"獲取到版本: {release?.tag_name}");
                    return release;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"GitHub API 請求失敗: {ex.Message}");
                return new GitHubRelease { tag_name = VersionInfo.Version };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"檢查更新時發生未預期的錯誤: {ex.Message}");
                return new GitHubRelease { tag_name = VersionInfo.Version };
            }
        }

        private static bool IsUpdateAvailable(string latestVersion)
        {
            try
            {
                latestVersion = latestVersion.Replace("v", "");
                Debug.WriteLine($"Version C -> {VersionInfo.Version} S -> {latestVersion}");
                var latest = new Version(latestVersion);
                var current = new Version(VersionInfo.Version);

                Debug.WriteLine($"Version C -> {current} S -> {latest}");
                return latest > current;
            }
            catch
            {
                Debug.WriteLine($"若格式錯誤，保守不升級");
                // 若格式錯誤，保守不升級
                return false;
            }
        }

        public class UpdateDownloader
        {
            public static async Task DownloadUpdate(string tag)
            {
                try
                {
                    // 檢查更新文件是否已存在
                    string updateFilePath = Path.Combine(Environment.CurrentDirectory, "update.zip");
                    if (File.Exists(updateFilePath))
                    {
                        Debug.WriteLine($"更新文件已存在: {updateFilePath}，跳過下載");
                        return; // 如果文件已存在，跳過下載步驟
                    }

                    string url = UpdateUrl.Replace("{tag}", tag).Replace("{filename}", tag);
                    Debug.WriteLine($"開始下載更新: {url}");

                    // 記錄到日誌
                    string logPath = Path.Combine(Environment.CurrentDirectory, "update_log.txt");
                    File.AppendAllText(logPath, $"[{DateTime.Now}] 開始下載更新: {url}\n");

                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromMinutes(5);
                        byte[] updateData = await client.GetByteArrayAsync(url);

                        File.AppendAllText(logPath, $"[{DateTime.Now}] 下載完成，檔案大小: {updateData.Length} 字節\n");
                        await File.WriteAllBytesAsync(updateFilePath, updateData);

                        File.AppendAllText(logPath, $"[{DateTime.Now}] 已保存更新檔到: {updateFilePath}\n");
                    }
                }
                catch (Exception ex)
                {
                    string logPath = Path.Combine(Environment.CurrentDirectory, "update_log.txt");
                    File.AppendAllText(logPath, $"[{DateTime.Now}] 下載更新失敗: {ex.Message}\n{ex.StackTrace}\n");
                    Debug.WriteLine($"下載更新失敗: {ex.Message}");
                    throw;
                }
            }
        }

        public class GitHubRelease
        {
            public string tag_name { get; set; } = "new";
        }

        public class GitHubConfig
        {
            public string RepositoryOwner { get; set; } = "TwIcePenguin";
            public string RepositoryName { get; set; } = "nobappspace";
            public string ReleaseFileName { get; set; } = "release";
        }
    }
}

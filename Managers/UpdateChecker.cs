using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NOBApp.Managers
{
    public class UpdateChecker
    {
        private static string GitHubApiUrl = "https://api.github.com/repos/TwIcePenguin/nobappspace/releases/latest";

        public async Task<GitHubRelease> GetLatestReleaseAsync()
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
                    return release ?? new GitHubRelease { tag_name = VersionInfo.Version };
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

        public bool IsUpdateAvailable(string latestVersion)
        {
            try
            {
                latestVersion = latestVersion.Replace("v", "");
                var latest = new Version(latestVersion);
                var current = new Version(VersionInfo.Version);
                return latest > current;
            }
            catch
            {
                return false;
            }
        }
    }

    public class GitHubRelease
    {
        public string tag_name { get; set; } = "new";
    }
}

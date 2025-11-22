using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NOBApp.Managers
{
    /// <summary>
    /// 更新管理：檢查遠端版本、執行下載與更新腳本。
    /// </summary>
    public class UpdateManager
    {
        private bool _updateAvailable;
        private string _latestVersion = string.Empty;
        public bool UpdateAvailable => _updateAvailable;
        public string LatestVersion => _latestVersion;

        private readonly UpdateChecker _updateChecker = new UpdateChecker();

        public async Task CheckForUpdatesAsync(Action<string>? statusCallback, Action<string>? uiApplyCallback)
        {
            try
            {
                var release = await _updateChecker.GetLatestReleaseAsync();
                _latestVersion = release.tag_name;
                _updateAvailable = _updateChecker.IsUpdateAvailable(_latestVersion);
                statusCallback?.Invoke(_updateAvailable ? $"發現新版本: {_latestVersion}" : "當前已是最新版本");
                if (_updateAvailable) uiApplyCallback?.Invoke(_latestVersion);
            }
            catch (Exception ex)
            {
                statusCallback?.Invoke($"檢查更新失敗: {ex.Message}");
            }
        }

        public async Task<bool> PerformUpdateAsync(string latestVersion, UIElement disableRoot, Button updateButton)
        {
            try
            {
                disableRoot.IsEnabled = false;
                updateButton.IsEnabled = false;
                updateButton.Content = "正在更新...";

                await UpdateDownloader.DownloadUpdate(latestVersion);
                updateButton.Content = "正在安裝更新...";

                string psScriptPath = Path.Combine(Environment.CurrentDirectory, "RunUpdate.ps1");
                File.WriteAllText(psScriptPath, UpdateScriptBuilder.Build(), System.Text.Encoding.UTF8);
                File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "update_pending.txt"), $"更新開始於 {DateTime.Now}，版本: {latestVersion}");

                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoExit -ExecutionPolicy Bypass -NoProfile -File \"{psScriptPath}\"",
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };
                Process.Start(psi);
                await Task.Delay(800);
                Environment.Exit(0);
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText("update_error.log", $"[{DateTime.Now}] 更新失敗: {ex.Message}\n{ex.StackTrace}\n");
                MessageBox.Show($"更新失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                disableRoot.IsEnabled = true;
                updateButton.IsEnabled = true;
                updateButton.Content = $"更新至 {latestVersion}";
                return false;
            }
        }

        public void CheckUpdateSuccessAndNotify()
        {
            string successMarkerPath = Path.Combine(Environment.CurrentDirectory, "update_success.txt");
            if (!File.Exists(successMarkerPath)) return;
            try
            {
                string successMessage = File.ReadAllText(successMarkerPath);
                File.Delete(successMarkerPath);
                string updateFilePath = Path.Combine(Environment.CurrentDirectory, "update.zip");
                if (File.Exists(updateFilePath)) { try { File.Delete(updateFilePath); } catch { } }
                MessageBox.Show($"應用程式已成功更新！\n\n{successMessage}", "更新成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateManager]讀取更新標記失敗: {ex.Message}");
            }
        }
    }
}
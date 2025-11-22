using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace NOBApp.Managers
{
    public class UpdateDownloader
    {
        private static string UpdateUrl = "https://github.com/TwIcePenguin/nobappspace/releases/download/{tag}/{filename}.zip";

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
                throw; // Re-throw to let caller handle it
            }
        }
    }
}

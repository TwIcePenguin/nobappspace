using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace NOBApp
{
    public static class DiscordNotifier
    {
        private static readonly string WebhookUrl = "https://discord.com/api/webhooks/1364050824090882190/21RG1VnK8smD9QG_0L2ED-ng8ogZZecwsCuAgWPxpGAYkanjI-VE5Ku0cMKbZPmR3P-Y";
        private static readonly HttpClient HttpClient = new HttpClient();

        /// <summary>
        /// 發送 Discord 通知訊息
        /// </summary>
        /// <param name="title">訊息標題</param>
        /// <param name="message">訊息內容</param>
        /// <returns>發送成功返回 true</returns>
        public static async Task<bool> SendNotificationAsync(string title, string message)
        {
            try
            {
                // 檢查是否已設定Webhook URL
                if (string.IsNullOrEmpty(WebhookUrl) || WebhookUrl.Contains("YOUR_WEBHOOK_URL_HERE"))
                {
                    Debug.WriteLine("Discord Webhook URL 尚未設定");
                    return false;
                }

                // 使用嵌入格式來讓訊息更美觀
                var payload = new
                {
                    embeds = new[]
                    {
                        new
                        {
                            title = title,
                            description = message,
                            color = 3447003,  // 藍色
                            footer = new
                            {
                                text = $"⏰ {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                            }
                        }
                    }
                };

                // 建立 POST 請求內容
                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                // 發送請求
                var response = await HttpClient.PostAsync(WebhookUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();

                // Discord Webhook 成功時傳回空內容與204狀態碼
                bool success = response.IsSuccessStatusCode;

                if (!success)
                {
                    Debug.WriteLine($"Discord 通知發送失敗: {responseContent}");
                }

                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"發送 Discord 通知時發生錯誤: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 發送帶有檔案的 Discord 通知
        /// </summary>
        /// <param name="title">訊息標題</param>
        /// <param name="message">訊息內容</param>
        /// <param name="filePath">檔案路徑</param>
        /// <returns>發送成功返回 true</returns>
        public static async Task<bool> SendFileAsync(string title, string message, string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(WebhookUrl) || WebhookUrl.Contains("YOUR_WEBHOOK_URL_HERE"))
                {
                    Debug.WriteLine("Discord Webhook URL 尚未設定");
                    return false;
                }

                if (!System.IO.File.Exists(filePath))
                {
                    Debug.WriteLine($"檔案不存在: {filePath}");
                    return false;
                }

                using (var form = new MultipartFormDataContent())
                {
                    // 加入檔案
                    var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(filePath));
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    form.Add(fileContent, "file", System.IO.Path.GetFileName(filePath));

                    // 加入 JSON payload (用於顯示訊息)
                    // 注意: 當發送檔案時，payload_json 欄位必須包含 embed 資訊
                    var payload = new
                    {
                        embeds = new[]
                        {
                            new
                            {
                                title = title,
                                description = message,
                                color = 3447003,
                                footer = new
                                {
                                    text = $"⏰ {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                                }
                            }
                        }
                    };
                    
                    var jsonContent = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                    form.Add(jsonContent, "payload_json");

                    var response = await HttpClient.PostAsync(WebhookUrl, form);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"Discord 檔案發送失敗: {responseContent}");
                    }

                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"發送 Discord 檔案時發生錯誤: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 設定 Discord Webhook URL
        /// </summary>
        /// <param name="webhookUrl">Discord Webhook URL</param>
        public static void SetWebhookUrl(string webhookUrl)
        {
            if (string.IsNullOrEmpty(webhookUrl))
            {
                Debug.WriteLine("Discord Webhook URL 不可為空");
                return;
            }

            var field = typeof(DiscordNotifier).GetField("WebhookUrl",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Static);

            field?.SetValue(null, webhookUrl);
        }
    }
}

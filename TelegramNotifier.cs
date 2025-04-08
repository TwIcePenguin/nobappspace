using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;

namespace NOBApp
{
    public static class TelegramNotifier
    {
        private static readonly string BotToken = "8013845777:AAGzo_9T3Nfry-SHT656OtlsDaKmlCjfpyc"; // 替換為您的 Bot Token
        private static readonly string ChatId = "7822954907";
        private static readonly HttpClient HttpClient = new HttpClient();
        private static readonly string ApiUrl = $"https://api.telegram.org/bot{BotToken}/sendMessage";

        /// <summary>
        /// 發送 Telegram 通知訊息
        /// </summary>
        /// <param name="title">訊息標題</param>
        /// <param name="message">訊息內容</param>
        /// <returns>發送成功返回 true</returns>
        public static async Task<bool> SendNotificationAsync(string title, string message)
        {
            try
            {
                // 組合標題和訊息
                string fullMessage = $"📢 *{title}*\n\n{message}\n\n⏰ {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                // 建立 POST 請求內容
                var content = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        chat_id = ChatId,
                        text = fullMessage,
                        parse_mode = "Markdown",
                        disable_notification = false
                    }),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                // 發送請求
                var response = await HttpClient.PostAsync(ApiUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();

                // 判斷是否成功
                var result = JsonSerializer.Deserialize<TelegramResponse>(responseContent);
                bool success = result != null && result.ok;

                if (!success)
                {
                    Debug.WriteLine($"Telegram 通知發送失敗: {responseContent}");
                }

                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"發送 Telegram 通知時發生錯誤: {ex.Message}");
                return false;
            }
        }

        private class TelegramResponse
        {
            public bool ok { get; set; }
            public JsonElement Result { get; set; }
        }
    }
}

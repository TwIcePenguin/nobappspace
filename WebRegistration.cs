using NOBApp.GoogleData;
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
        public static List<NOBDATA> useNobList = new List<NOBDATA>();

        public static void OnWebReg()
        {
            try
            {
                List<FPDATA> dataList = new();
                Parallel.ForEach(useNobList, nob =>
                {
                    if (nob != null && !string.IsNullOrEmpty(nob.Account) && !string.IsNullOrEmpty(nob.Password))
                    {
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
                                var fdataStr = Encoder.AesEncrypt(fdata);

                                var jdata = JsonSerializer.Serialize(new UData() { KeyStr = fdataStr });

                                var content = new StringContent(jdata, Encoding.UTF8, "application/json");

                                string url = @"https://nobcdkfunctionappt202502241545.azurewebsites.net/api/GetNOBCDK?code=4yEpLUcI6oiYCOj6mwS0x3Rqe7pqdDHiRE_5Co8zyzQiAzFu2iWLlw==";

                                HttpResponseMessage response = await client.PostAsync(url, content);

                                Debug.WriteLine($"url => {url} response -> {response.IsSuccessStatusCode}");
                                if (response.IsSuccessStatusCode)
                                {
                                    string responseContent = await response.Content.ReadAsStringAsync();
                                    Debug.WriteLine($"回傳訊息 -> \n{responseContent}");
                                    Authentication.讀取認證訊息Json(responseContent);
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

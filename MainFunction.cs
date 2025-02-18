using NOBApp.GoogleData;
using NtpClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace NOBApp
{
    public partial class MainWindow
    {
        public static List<NOBDATA> useNobList = new List<NOBDATA>();
        /// <summary>
        /// 註冊訊息
        /// </summary>

        public class UData
        {
            public string KeyStr { get; set; }
        }

        public void OnWebReg()
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
                            dataList.Add(fdata);
                        }
                    }
                });

                if (isGoogleReg)
                {
                    GoogleSheet.Post(dataList);
                }
                else
                {
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

                                    var jdata = JsonSerializer.Serialize(new UData() { KeyStr = fdataStr }); ;

                                    // 將加密後的資料放入 HttpContent
                                    var content = new StringContent(jdata, Encoding.UTF8, "application/json"); // ContentType 設定為 application/json

                                    string url = @"https://ccnobapi20250213162427.azurewebsites.net/"; // URL 不再包含加密後的資料

                                    HttpResponseMessage response = await client.PostAsync(url, content); // 使用 PostAsync 方法

                                    Debug.WriteLine("response -> " + response.IsSuccessStatusCode);
                                    if (response.IsSuccessStatusCode)
                                    {
                                        string responseContent = await response.Content.ReadAsStringAsync();
                                        Debug.WriteLine($"回傳訊息 -> \n{responseContent}");
                                        MainWindow.讀取認證訊息Json(responseContent);
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
            }
            catch (Exception err)
            {
                Debug.WriteLine("Error : " + err.ToString());
            }
        }

        //public void OnWebReg()
        //{
        //    try
        //    {
        //        List<FPDATA> dataList = new();
        //        foreach (var nob in useNobList)
        //        {
        //            if (nob != null && !string.IsNullOrEmpty(nob.Account) && !string.IsNullOrEmpty(nob.Password))
        //            {
        //                FPDATA fdata = new();
        //                fdata.Acc = nob.Account;
        //                fdata.Pww = nob.Password;
        //                fdata.UserName = nob.PlayerName;
        //                fdata.SerialNumber = Tools.GetSerialNumber();
        //                fdata.LoginTimer = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //                dataList.Add(fdata);
        //            }
        //        }

        //        if (isGoogleReg)
        //        {
        //            GoogleSheet.Post(dataList);
        //        }
        //        else
        //        {
        //            try
        //            {
        //                string url = "";
        //                foreach (var data in dataList)
        //                {
        //                    var fdata = JsonSerializer.Serialize(data);
        //                    var fdataStr = Encoder.AesEncrypt(fdata);
        //                    string encodedUrl = Uri.EscapeDataString(fdataStr);
        //                    //Debug.WriteLine("encodedUrl : " + encodedUrl + " | " + fdata);
        //                    url = @$"https://ccnobapi20250213162427.azurewebsites.net/{encodedUrl}";
        //                    Tools.GetWebsiteData(new Uri(url));
        //                }
        //            }
        //            catch (Exception err)
        //            {
        //                Debug.WriteLine("Error : " + err.ToString());
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        Debug.WriteLine("Error : " + err.ToString());
        //    }
        //}

        public static void 儲存認證訊息(PNobUserData nobUseData)
        {
            string jsonString = JsonSerializer.Serialize(nobUseData);
            try
            {
                using (StreamWriter outputFile = new StreamWriter($@"{UseLockNOB!.PlayerName}_CDK.nob"))
                {
                    string dJson = Encoder.AesEncrypt(jsonString);
                    outputFile.WriteLine(dJson);
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine("Error : " + err.ToString());
            }
        }

        public static bool 讀取認證訊息Name(string playerName)
        {
            var name = string.IsNullOrEmpty(playerName) ? UseLockNOB!.PlayerName : playerName;
            if (File.Exists($@"{name}_CDK.nob"))
            {
                using StreamReader reader = new($@"{name}_CDK.nob");
                if (reader == null)
                {
                    Debug.WriteLine("reader == null");
                    return false;
                }
                string jsonString = reader.ReadToEnd();
                CUCDKEY = jsonString;
                if (UseLockNOB != null)
                {
                    讀取認證訊息Json(jsonString);
                    return true;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public static void 讀取認證訊息Json(string json)
        {
            string dJson = Encoder.AesDecrypt(json);
            Debug.WriteLine(dJson);
            PNobUserData nobUseData = JsonSerializer.Deserialize<PNobUserData>(dJson);
            if (nobUseData != null && string.IsNullOrEmpty(nobUseData.Acc) == false &&
                string.IsNullOrEmpty(UseLockNOB!.Account) == false)
            {
                DateTime getOnlineTime = GetNetworkTimeAsync();
                if (DateTime.TryParse(nobUseData.StartTimer, out 到期日) && 到期日 > getOnlineTime)
                {
                    UseLockNOB!.驗證完成 = true;
                    UseLockNOB!.特殊者 = true;
                    儲存認證訊息(nobUseData);
                }
            }
        }


        private static readonly string[] _ntpServers =
        {
            "time.google.com",
            "time.cloudflare.com",
            "time.windows.com",
            "ntp.ntsc.ac.cn",
            "time.tencent.com",
            // 添加其他可靠的 NTP 伺服器
        };


        static public DateTime GetNetworkTimeAsync()
        {
            DateTime onlineTime = DateTime.Now;
            foreach (var server in _ntpServers)
            {
                try
                {
                    var timeClient = new NtpConnection(server);
                    onlineTime = timeClient.GetUtc().AddHours(8);
                    return onlineTime; // 返回 時間
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting time from {server}: {ex.Message}");
                    // 嘗試下一個伺服器
                }
            }

            return onlineTime;
        }

        void RefreshNOBID_Sec()
        {
            FName1.Items.Clear();
            FName2.Items.Clear();
            FName3.Items.Clear();
            SelectFID_1.Items.Clear();
            SelectFID_2.Items.Clear();
            SelectFID_3.Items.Clear();
            SelectFID_4.Items.Clear();
            SelectFID_5.Items.Clear();
            SelectFID_6.Items.Clear();
            SelectFID_7.Items.Clear();

            foreach (var item in nobList)
            {
                FName1.Items.Add(item.PlayerName);
                FName2.Items.Add(item.PlayerName);
                FName3.Items.Add(item.PlayerName);
                SelectFID_1.Items.Add(item.PlayerName);
                SelectFID_2.Items.Add(item.PlayerName);
                SelectFID_3.Items.Add(item.PlayerName);
                SelectFID_4.Items.Add(item.PlayerName);
                SelectFID_5.Items.Add(item.PlayerName);
                SelectFID_6.Items.Add(item.PlayerName);
                SelectFID_7.Items.Add(item.PlayerName);
            }

            FName1.Items.Add("");
            FName2.Items.Add("");
            FName3.Items.Add("");
            SelectFID_1.Items.Add("");
            SelectFID_2.Items.Add("");
            SelectFID_3.Items.Add("");
            SelectFID_4.Items.Add("");
            SelectFID_5.Items.Add("");
            SelectFID_6.Items.Add("");
            SelectFID_7.Items.Add("");
        }

        public static List<long> 目標IDs = new();
        public static List<long> 忽略名單IDs = new();
        static public List<long> 鎖定PCID = new();
        static public Dictionary<long, string> 鎖定PC = new();
        static public void UpdatePCIDs()
        {
            MainWindow.目標IDs.Clear();
            if (UseLockNOB != null)
            {
                var str = AddressData.搜尋身邊NPCID起始;
                long findID, chid;
                List<long> skipIDs = new();
                for (int i = 0; i < 150; i++)
                {
                    findID = MainWindow.dmSoft.ReadInt(UseLockNOB.Hwnd, "<nobolHD.bng> + " + str, 4);
                    chid = MainWindow.dmSoft.ReadInt(UseLockNOB.Hwnd, "<nobolHD.bng> + " + str.AddressAdd(3), 2);
                    if (chid != 1)
                    {
                        skipIDs.Add(findID);
                        str = str.AddressAdd(12); continue;
                    }
                    if (skipIDs.Contains(findID)) { str = str.AddressAdd(12); continue; }

                    skipIDs.Add(findID);
                    if (目標IDs.Contains(findID) == false)
                        目標IDs.Add((int)findID);

                    str = str.AddressAdd(12);
                }
            }
        }

        public static Dictionary<long, long> cacheNPCID = new();
        static public void UpdateNPCIDs(int checkDIS = 65535, bool findFE = false)
        {
            Debug.WriteLine($"探索範圍 -> {checkDIS}");
            MainWindow.目標IDs.Clear();
            cacheNPCID.Clear();
            if (UseLockNOB != null)
            {
                var str = AddressData.搜尋身邊NPCID起始;
                long findID, chid;
                List<long> skipIDs = new();

                for (int i = 0; i < 150; i++)
                {
                    bool isBOX = false;
                    findID = MainWindow.dmSoft.ReadInt(UseLockNOB.Hwnd, "<nobolHD.bng> + " + str, 4);
                    chid = MainWindow.dmSoft.ReadInt(UseLockNOB.Hwnd, "<nobolHD.bng> + " + str.AddressAdd(3), 2);
                    if (chid != 96 && (findFE && chid != 254))
                    {
                        skipIDs.Add(findID);
                        str = str.AddressAdd(12);
                        continue;
                    }
                    if (cacheNPCID.Keys.Contains(findID) || 忽略名單IDs.Contains(Math.Abs(findID)) || 忽略名單IDs.Contains(findID) || skipIDs.Contains(findID))
                    {
                        str = str.AddressAdd(12);
                        continue;
                    }

                    isBOX = chid == 254;
                    long dis = MainWindow.dmSoft.ReadInt(UseLockNOB.Hwnd, "<nobolHD.bng> + " + str.AddressAdd(4), 4);
                    //Debug.WriteLine($"{findID} ch {chid} -DIS  : {checkDIS} - {dis}");
                    if (dis > (isBOX ? 2 : 3) && checkDIS > dis)
                    {
                        //if (目標IDs.Contains(findID) == false)
                        if (cacheNPCID.Keys.Contains(findID) == false)
                        {
                            //Debug.WriteLine($"目標 -> {findID} 距離 : {dis}");
                            cacheNPCID.Add(findID, dis);
                        }
                        skipIDs.Add(findID);
                    }
                    str = str.AddressAdd(12);
                }

                cacheNPCID.OrderBy(Data => Data.Value).ToDictionary(keyvalue => keyvalue.Key, keyvalue => keyvalue.Value);

                foreach (var item in cacheNPCID)
                {
                    目標IDs.Add(item.Key);
                }
            }

        }

        static public List<int> GetNPC2IDs(int checkDIS = 65536, bool findBox = false)
        {
            List<int> list = new();
            Debug.WriteLine($"探索範圍 -> {checkDIS}");
            MainWindow.目標IDs.Clear();
            cacheNPCID.Clear();
            if (UseLockNOB != null)
            {
                var str = AddressData.搜尋身邊NPCID起始;
                long findID, chid;
                List<long> skipIDs = new();

                for (int i = 0; i < 150; i++)
                {
                    bool isBOX = false;
                    findID = MainWindow.dmSoft.ReadInt(UseLockNOB.Hwnd, "<nobolHD.bng> + " + str, 4);
                    chid = MainWindow.dmSoft.ReadInt(UseLockNOB.Hwnd, "<nobolHD.bng> + " + str.AddressAdd(3), 2);
                    isBOX = chid == 254;

                    if (chid != 96 || (findBox == false && isBOX))
                    {
                        skipIDs.Add(findID);
                        str = str.AddressAdd(12);
                        continue;
                    }

                    if (cacheNPCID.Keys.Contains(findID) || 忽略名單IDs.Contains(Math.Abs(findID)) || 忽略名單IDs.Contains(findID) || skipIDs.Contains(findID))
                    {
                        str = str.AddressAdd(12);
                        continue;
                    }
                    long dis = MainWindow.dmSoft.ReadInt(UseLockNOB.Hwnd, "<nobolHD.bng> + " + str.AddressAdd(4), 4);
                    //Debug.WriteLine($"{findID} ch {chid} -DIS  : {checkDIS} - {dis}");
                    if (dis > (isBOX ? 2 : 3) && checkDIS > dis)
                    {
                        //if (目標IDs.Contains(findID) == false)
                        if (cacheNPCID.Keys.Contains(findID) == false)
                        {
                            //Debug.WriteLine($"目標 -> {findID} 距離 : {dis}");
                            cacheNPCID.Add(findID, dis);
                        }
                        skipIDs.Add(findID);
                    }
                    str = str.AddressAdd(12);
                }

                cacheNPCID.OrderBy(Data => Data.Value).ToDictionary(keyvalue => keyvalue.Key, keyvalue => keyvalue.Value);

                foreach (var item in cacheNPCID)
                {
                    目標IDs.Add(item.Key);
                    list.Add((int)item.Key);
                }
            }

            return list;
        }

        static public List<int> GetNPCIDs(int checkDIS = 65536, bool findBox = false)
        {
            List<int> foundNpcIds = new List<int>(); // 更具描述性的變數名稱
            Debug.WriteLine($"探索範圍 -> {checkDIS}");

            目標IDs.Clear(); // 注意: 如果目標IDs 在其他地方使用，清除它可能會產生副作用
            cacheNPCID.Clear(); // 每次搜尋開始時清除快取

            if (UseLockNOB == null)
            {
                Debug.WriteLine("UseLockNOB 為 null，無法搜尋 NPC。");
                return foundNpcIds; // 如果 NOB 未鎖定，返回空列表
            }

            string startAddress = AddressData.搜尋身邊NPCID起始; // 使用已儲存的 AddressData

            // 1. 批次讀取記憶體: 一次讀取所有 NPC 資料
            int npcCountToRead = 150; // 固定迴圈次數，考慮是否能動態化
            int bytesToRead = npcCountToRead * 12; // 根據原始碼，每個 NPC 條目佔 12 位元組

            string dataStr = MainWindow.dmSoft?.ReadData(UseLockNOB.Hwnd, "<nobolHD.bng> + " + startAddress, bytesToRead);

            if (string.IsNullOrEmpty(dataStr))
                return foundNpcIds; // 處理dataStr失敗
            //dataStr = dataStr.Replace(" ", "");
            //Debug.WriteLine($"1 Data => {dataStr}");
            byte[] npcDataBytes = null;
            try
            {
                npcDataBytes = HexStringToByteArray(dataStr); // 使用 HexStringToByteArray 函數
            }
            catch (FormatException ex)
            {
                Debug.WriteLine($"轉換 Hex 字串時發生錯誤: {ex.Message}");
                return foundNpcIds; // 處理 Hex 字串格式錯誤
            }

            //Debug.WriteLine($"2 Length => {npcDataBytes.Length} {bytesToRead}");
            if (npcDataBytes == null || npcDataBytes.Length != bytesToRead)
            {
                Debug.WriteLine($"{npcDataBytes == null} 讀取 NPC 資料區塊時發生錯誤。 ReadData 返回 null 或不正確的大小。");
                return foundNpcIds; // 處理記憶體讀取失敗
            }

            // 2. 在記憶體中處理批次資料
            List<long> skipIDs = new List<long>(); // 使用 List 作為 skipIDs，與原始碼一致

            Parallel.For(0, npcCountToRead, i =>
            {
                int offset = i * 12; // 位元組陣列中每個 NPC 條目的偏移量

                //Debug.WriteLine($"bytes {npcDataBytes.ToString()}");
                long findID = BitConverter.ToInt32(npcDataBytes, offset); // 讀取 Int32 (4 位元組) 作為 findID
                int chid = npcDataBytes[offset + 3];
                long dis = BitConverter.ToUInt16(npcDataBytes, offset + 4); // 讀取 Int32 (4 位元組) 作為 dis
                //Debug.WriteLine($"{findID} - {chid} - {dis}");
                bool isBOX = chid == 254;

                if (skipIDs.Contains(findID))
                {
                    return;
                }

                if (chid != 96 || (!findBox && isBOX)) // 更有效率的合併條件判斷
                {
                    lock (skipIDs)
                    {
                        skipIDs.Add(findID);
                    }
                    return; // 跳到下一次迭代
                }
                if (chid == 1) //忽略玩家
                {
                    lock (skipIDs)
                    {
                        skipIDs.Add(findID);
                    }
                    return; // 跳到下一次迭代
                }

                if (cacheNPCID.ContainsKey(findID) || 忽略名單IDs.Contains(Math.Abs(findID)) || 忽略名單IDs.Contains(findID) || skipIDs.Contains(findID))
                {
                    return; // 如果已經處理過或在忽略名單中，則跳過
                }

                if (dis > (isBOX ? 2 : 3) && checkDIS > dis)
                {
                    if (!cacheNPCID.ContainsKey(findID)) // 在加入快取之前檢查
                    {
                        lock (cacheNPCID)
                        {
                            cacheNPCID[findID] = dis;
                        }
                    }
                    lock (skipIDs)
                    {
                        skipIDs.Add(findID); // 加入 skipIDs 以防止在當前迴圈中重複處理
                    }
                }
            });

            // 3. 優化的排序和列表建立 (如果真的需要排序)
            // 依照距離 (Value) 排序 cacheNPCID - 僅在您的邏輯真的需要排序時才執行
            var orderedCacheNPCID = cacheNPCID.OrderBy(pair => pair.Value); // 依距離排序

            // 從排序後的 (或未排序的) 快取建立結果列表
            foreach (var item in orderedCacheNPCID) // 如果順序不重要，可以直接迭代 cacheNPCID
            {
                目標IDs.Add(item.Key); // 注意修改 目標IDs 的副作用
                foundNpcIds.Add((int)item.Key);
            }

            return foundNpcIds;
        }

        // ... 你的 HexStringToByteArray 函數 (放在 Main 函數外面或其他方便的地方) ...
        public static byte[] HexStringToByteArray(string hex)
        {
            hex = hex.Replace(" ", "");
            if (hex.Length % 2 != 0)
            {
                throw new FormatException("The hex string cannot have an odd number of digits.");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
    }

}

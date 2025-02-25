using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace NOBApp
{
    public class Authentication
    {
        public static void 儲存認證訊息(PNobUserData nobUseData)
        {
            string jsonString = JsonSerializer.Serialize(nobUseData);
            try
            {
                using (StreamWriter outputFile = new StreamWriter($@"{MainWindow.UseLockNOB!.PlayerName}_CDK.nob"))
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
            var name = string.IsNullOrEmpty(playerName) ? MainWindow.UseLockNOB!.PlayerName : playerName;
            if (File.Exists($@"{name}_CDK.nob"))
            {
                using StreamReader reader = new($@"{name}_CDK.nob");
                if (reader == null)
                {
                    Debug.WriteLine("沒有該資料");
                    return false;
                }
                string jsonString = reader.ReadToEnd();
                MainWindow.CUCDKEY = jsonString;
                if (MainWindow.UseLockNOB != null)
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
                string.IsNullOrEmpty(MainWindow.UseLockNOB?.Account) == false)
            {
                DateTime getOnlineTime = NetworkTime.GetNetworkTimeAsync();
                if (DateTime.TryParse(nobUseData.StartTimer, out MainWindow.到期日) && MainWindow.到期日 > getOnlineTime)
                {
                    MainWindow.UseLockNOB!.驗證完成 = true;
                    MainWindow.UseLockNOB!.特殊者 = true;
                    儲存認證訊息(nobUseData);
                }
            }
        }
    }
}

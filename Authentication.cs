using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace NOBApp
{
    public class Authentication
    {
        public static void 儲存認證訊息(PNobUserData nobUseData)
        {
            string jsonString = JsonSerializer.Serialize(nobUseData);
            try
            {
                using (StreamWriter outputFile = new StreamWriter($@"{MainWindow.UseLockNOB!.Account}_CDK.nob"))
                {
                    string dJson = Encoder.AesEncrypt(jsonString, "CHECKNOBPENGUIN", "CHECKNOB");
                    outputFile.WriteLine(dJson);
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine("Error : " + err.ToString());
            }
        }

        public static bool 讀取認證訊息Name(string account)
        {
            var acc = string.IsNullOrEmpty(account) ? MainWindow.UseLockNOB!.Account : account;
            if (File.Exists($@"{acc}_CDK.nob"))
            {
                using StreamReader reader = new($@"{acc}_CDK.nob");
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
            string dJson = Encoder.AesDecrypt(json, "CHECKNOBPENGUIN", "CHECKNOB");
            PNobUserData nobUseData = JsonSerializer.Deserialize<PNobUserData>(dJson);
            if (nobUseData != null && string.IsNullOrEmpty(nobUseData.Acc) == false &&
                string.IsNullOrEmpty(MainWindow.UseLockNOB?.Account) == false)
            {
                DateTime getOnlineTime = NetworkTime.GetNetworkTimeAsync();
                if (DateTime.TryParse(nobUseData.StartTimer, out MainWindow.到期日) && MainWindow.到期日.AddDays(1) > getOnlineTime)
                {
                    MainWindow.UseLockNOB!.驗證完成 = true;
                    MainWindow.UseLockNOB!.特殊者 = true;
                    儲存認證訊息(nobUseData);
                }
                else
                {
                    MessageBox.Show("驗證失敗，時間到期 請找企鵝延長時間");
                }
            }
            else
            {
                MessageBox.Show("驗證失敗，請確認認證碼是否正確");
            }
        }
    }
}

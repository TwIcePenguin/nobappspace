using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace NOBApp
{
    public class Authentication
    {
        public static void 儲存認證訊息(NOBDATA data, PNobUserData nobUseData)
        {
            string jsonString = JsonSerializer.Serialize(nobUseData);
            try
            {
                using (StreamWriter outputFile = new StreamWriter($@"{data!.Account}_CDK.nob"))
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

        public static bool 讀取認證訊息Name(NOBDATA user)
        {
            if (File.Exists($@"{user.Account}_CDK.nob"))
            {
                using StreamReader reader = new($@"{user.Account}_CDK.nob");
                if (reader == null)
                {
                    Debug.WriteLine("沒有該資料");
                    return false;
                }
                string jsonString = reader.ReadToEnd();
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
                return;

            string dJson = Encoder.AesDecrypt(json, "CHECKNOBPENGUIN", "CHECKNOB");
            PNobUserData nobUseData = JsonSerializer.Deserialize<PNobUserData>(dJson);
            if (nobUseData != null && string.IsNullOrEmpty(nobUseData.Acc) == false &&
                string.IsNullOrEmpty(user.Account) == false)
            {
                //DateTime getOnlineTime = NetworkTime.GetNetworkTimeAsync();
                if (DateTime.TryParse(nobUseData.StartTimer, out user.到期日))
                {
                    user.驗證完成 = true;
                    user.特殊者 = true;
                    儲存認證訊息(user, nobUseData);
                }
                //else
                //{
                //    MessageBox.Show("驗證失敗，時間到期 請找企鵝延長時間");
                //}
            }
            else
            {
                MessageBox.Show("驗證失敗，請確認認證碼是否正確");
            }
        }
    }
}

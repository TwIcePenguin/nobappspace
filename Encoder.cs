using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Reflection.Metadata;

namespace NOBApp
{
    public enum EncoderType
    {
        //可逆編碼(對稱金鑰)
        AES,
        DES,
        RC2,
        TripleDES,

        //可逆編碼(非對稱金鑰)
        RSA,

        //不可逆編碼(雜湊值)
        MD5,
        SHA1,
        SHA256,
        SHA384,
        SHA512
    }

    public class Encoder
    {
        public string Key { get; set; } = string.Empty;
        public string IV { get; set; } = string.Empty;

        /// <summary>
        /// 產生新的KEY
        /// </summary>
        /// <param name="type">編碼器種類</param>
        public void GenerateKey(EncoderType type)
        {
            switch (type)
            {
                //可逆編碼(對稱金鑰)
                case EncoderType.AES:
                    using (Aes csp = Aes.Create())
                    {
                        csp.GenerateIV();
                        IV = Convert.ToBase64String(csp.IV);
                        csp.GenerateKey();
                        Key = Convert.ToBase64String(csp.Key);
                    }
                    break;
                case EncoderType.DES:
                    using (DES csp = DES.Create())
                    {
                        csp.GenerateIV();
                        IV = Convert.ToBase64String(csp.IV);
                        csp.GenerateKey();
                        Key = Convert.ToBase64String(csp.Key);
                    }
                    break;
                case EncoderType.RC2:
                    using (RC2 csp = RC2.Create())
                    {
                        csp.GenerateIV();
                        IV = Convert.ToBase64String(csp.IV);
                        csp.GenerateKey();
                        Key = Convert.ToBase64String(csp.Key);
                    }
                    break;
                case EncoderType.TripleDES:
                    using (TripleDES csp = TripleDES.Create())
                    {
                        csp.GenerateIV();
                        IV = Convert.ToBase64String(csp.IV);
                        csp.GenerateKey();
                        Key = Convert.ToBase64String(csp.Key);
                    }
                    break;
                //可逆編碼(非對稱金鑰)
                case EncoderType.RSA:
                    using (RSACryptoServiceProvider csp = new RSACryptoServiceProvider())
                    {
                        IV = "";
                        Key = csp.ToXmlString(true);
                    }
                    break;
            }
        }

        private static string AesKey = "隨便輸入一組字串"; //密鑰
        private static string AesIv = "也是隨便輸入一組字串"; //密鑰向量

        /// <summary>
        /// AES 加密字串
        /// </summary>
        /// <param name="original">原始字串</param>
        /// <param name="key">自訂金鑰</param>
        /// <param name="iv">自訂向量</param>
        /// <returns></returns>
        public static string AesEncrypt(string original, string? key = null, string? iv = null)
        {
            key = string.IsNullOrEmpty(key) ? AesKey : key;
            iv = string.IsNullOrEmpty(iv) ? AesIv : iv;

            string encrypt = "";
            try
            {
                using (Aes aes = Aes.Create())
                using (var md5 = MD5.Create())
                using (var sha256 = SHA256.Create())
                {
                    byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                    byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                    byte[] dataByteArray = Encoding.UTF8.GetBytes(original);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(keyData, ivData), CryptoStreamMode.Write))
                        {
                            cs.Write(dataByteArray, 0, dataByteArray.Length);
                            cs.FlushFinalBlock();
                            encrypt = Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine(ex.Message);
            }

            return encrypt;
        }

        /// <summary>
        /// AES 解密字串
        /// </summary>
        /// <param name="hexString">已加密字串</param>
        /// <param name="key">自訂金鑰</param>
        /// <param name="iv">自訂向量</param>
        /// <returns></returns>
        public static string AesDecrypt(string hexString, string? key = null, string? iv = null)
        {
            key = string.IsNullOrEmpty(key) ? AesKey : key;
            iv = string.IsNullOrEmpty(iv) ? AesIv : iv;

            string decrypt = hexString;
            try
            {
                using (Aes aes = Aes.Create())
                using (var md5 = MD5.Create())
                using (var sha256 = SHA256.Create())
                {
                    byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                    byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                    byte[] dataByteArray = Convert.FromBase64String(hexString);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(keyData, ivData), CryptoStreamMode.Write))
                        {
                            cs.Write(dataByteArray, 0, dataByteArray.Length);
                            cs.FlushFinalBlock();
                            decrypt = Encoding.UTF8.GetString(ms.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine(ex.Message);
            }
            return decrypt;
        }

        public static bool TryAesDecrypt(string hexString, out string original, string? key = null, string? iv = null)
        {
            return hexString != (original = AesDecrypt(hexString, key, iv));
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CMID.Utillity
{
    class JiaMi
    {


        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="text">需要加密的文本</param>
        /// <param name="key">加密密钥</param>
        /// <returns>加密后的文本</returns>
        public static string EncryptText(string text, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32)); // 32位密钥
            byte[] iv = new byte[16]; // 初始化向量（IV），全零

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] textBytes = Encoding.UTF8.GetBytes(text);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(textBytes, 0, textBytes.Length);
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="encryptedText">加密后的文本</param>
        /// <param name="key">加密时使用的密钥</param>
        /// <returns>解密后的文本</returns>
        public static string DecryptText(string encryptedText, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32)); // 32位密钥
            byte[] iv = new byte[16]; // 初始化向量（IV），全零

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;

                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
    }
}

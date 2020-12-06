using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Server
{
    public class Encryption
    {
        static string EncryptionSalt = "66-BC-GC66-BC-GC";
        static string EncryptionKey = "A24-C356A24-C356";
        static bool EncryptionEnabled = true;
        public static byte[] Encrypt(byte[] data)
        {
            if (!EncryptionEnabled)
                return data;
            using (var aes = Aes.Create())
            {
                byte[] key = Utils.StrToBytes(EncryptionKey);
                byte[] salt = Utils.StrToBytes(EncryptionSalt);
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key;
                aes.IV = salt;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, encryptor);
                }
            }
        }

        public static byte[] Decrypt(byte[] data)
        {
            if (!EncryptionEnabled)
                return data;
            using (var aes = Aes.Create())
            {
                byte[] key = Utils.StrToBytes(EncryptionKey);
                byte[] salt = Utils.StrToBytes(EncryptionSalt);
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key;
                aes.IV = salt;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    byte[] data_decrypted = PerformCryptography(data, decryptor);
                    return data_decrypted ;
                }
            }
        }

        private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();

                return ms.ToArray();
            }
        }
    }
}

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
        static Options options;
        private static string EncryptionSalt = "66-BC-GC66-BC-GC";

        public static void Init(Options _options)
        {
            options = _options;
        }

        public static byte[] Encrypt(byte[] data)
        {
            if (!options.EncryptionEnabled)
                return data;
            using var aes = Aes.Create();
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Utils.StrToBytes(options.EncryptionKey);
            aes.IV = Utils.StrToBytes(EncryptionSalt);
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            data = PerformCryptography(data, encryptor);
            if (data == null)
                throw new Exception("Failed encryption");
            return data;
        }

        public static byte[] Decrypt(byte[] data)
        {
            if (!options.EncryptionEnabled)
                return data;
            using var aes = Aes.Create();
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Utils.StrToBytes(options.EncryptionKey);
            aes.IV = Utils.StrToBytes(EncryptionSalt);
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            data = PerformCryptography(data, decryptor);
            if (data == null)
                throw new Exception("Failed decryption");
            return data;
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

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
        static Settings _settings;

        public static void Init(Settings settings)
        {
            _settings = settings;
        }

        public static byte[] Encrypt(byte[] data)
        {
            using var aes = Aes.Create();
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Padding = PaddingMode.Zeros;
            aes.Key = Utils.StrToBytes(_settings.EncryptionKey);
            aes.IV = Utils.StrToBytes(_settings.EncryptionSalt);
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            return PerformCryptography(data, encryptor);
        }

        public static byte[] Decrypt(byte[] data)
        {
            using var aes = Aes.Create();
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Padding = PaddingMode.Zeros;
            aes.Key = Utils.StrToBytes(_settings.EncryptionKey);
            aes.IV = Utils.StrToBytes(_settings.EncryptionSalt);
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            return PerformCryptography(data, decryptor);
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

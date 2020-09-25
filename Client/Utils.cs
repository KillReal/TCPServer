﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class Utils
    {
        public static byte[] AddCommandLength(byte[] commandBytes)
        {
            return ConcatByteArrays(BitConverter.GetBytes(commandBytes.Length), commandBytes);
        }

        public static byte[] ConcatByteArrays(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public static int BytesToInt(byte[] bytesArray)
        {
            return BitConverter.ToInt32(bytesArray, 0);
        }

        public static byte[] GetBytes(string str)
        {
            if (str == null)
                return null;
            byte[] str_bytes = Encoding.Default.GetBytes(str);
            string encoded_str = Encoding.UTF8.GetString(str_bytes);
            var bytes = new byte[encoded_str.Length * sizeof(char)];
            Buffer.BlockCopy(encoded_str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            var chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}

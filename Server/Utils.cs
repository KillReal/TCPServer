using Server.Pockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Server
{
    class Utils
    {
        public static byte[] SplitBytes(ref byte[] bytesArray, int size)
        {
            byte[] ret = new byte[size];
            if (bytesArray.Length <= size)
            {
                ret = bytesArray;
                bytesArray = new byte[0];
                return ret;
            }
            byte[] newArray = new byte[bytesArray.Length - size];
            Buffer.BlockCopy(bytesArray, 0, ret, 0, size);
            Buffer.BlockCopy(bytesArray, size, newArray, 0, bytesArray.Length - size);
            bytesArray = newArray;
            return ret;
        }

        public static byte[] SplitBytes(byte[] bytesArray, int size)
        {
            byte[] ret = new byte[size];
            if (bytesArray.Length <= size)
            {
                ret = bytesArray;
                _ = new byte[0];
                return ret;
            }
            byte[] newArray = new byte[bytesArray.Length - size];
            Buffer.BlockCopy(bytesArray, 0, ret, 0, size);
            Buffer.BlockCopy(bytesArray, size, newArray, 0, bytesArray.Length - size);
            return ret;
        }

        public static byte[] ConcatBytes(byte[] first, byte[] second)
        {
            if (first == null)
                first = new byte[0];
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public static byte[] ConcatBytes(BasePocket first, BasePocket second)
        {
            return ConcatBytes(first.ToBytes(), second.ToBytes());
        }

        public static int BytesToInt(byte[] bytesArray)
        {
            return BitConverter.ToInt32(bytesArray, 0);
        }

        public static byte[] IntToBytes(int val)
        {
            return BitConverter.GetBytes(val);
        }

        public static byte[] StrToBytes(string str)
        {
            if (str == null)
                return null;
            return Encoding.UTF8.GetBytes(str);
        }

        public static string BytesToStr(byte[] bytes)
        {
            if (bytes == null)
                return null;
            return Encoding.UTF8.GetString(bytes);
        }
    }
}

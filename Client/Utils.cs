using Server.Pockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Server
{
    class Utils
    {
        //
        // TODO: increase func's speed and usability
        //

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern short GetAsyncKeyState(int vKey);
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        static public bool IsKeyPressed(char key)
        {
            int result = GetAsyncKeyState(key);
            if (GetConsoleWindow() != GetForegroundWindow())
                return false;
            if (result < 0 && (result & 0x01) == 0x01)
                return true;
            return false;
        }

        public static byte[] AddCommandLength(byte[] commandBytes)
        {
            return ConcatBytes(BitConverter.GetBytes(commandBytes.Length), commandBytes);
        }

        public static byte[] SplitBytes(ref byte[] bytesArray, int size)
        {
            byte[] ret = new byte[size];
            byte[] newArray = new byte[bytesArray.Length - size];
            Buffer.BlockCopy(bytesArray, 0, ret, 0, size);
            Buffer.BlockCopy(bytesArray, size, newArray, 0, bytesArray.Length - size);
            bytesArray = newArray;
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
            byte[] str_bytes = Encoding.Default.GetBytes(str);
            string encoded_str = Encoding.UTF8.GetString(str_bytes);
            var bytes = new byte[encoded_str.Length * 2];
            Buffer.BlockCopy(encoded_str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string BytesToStr(byte[] bytes)
        {
            var chars = new char[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}

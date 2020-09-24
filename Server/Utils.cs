﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
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

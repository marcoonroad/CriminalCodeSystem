using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CriminalCodeSystem.Helpers
{
    public class ConversionUtils
    {
        public static byte[] StringToBytes(string str)
        {
            char[] chars = str.ToArray();
            byte[] bytes = new byte[chars.Length * 2];

            for (int i = 0; i < chars.Length; i += 1)
            {
                Array.Copy(BitConverter.GetBytes(chars[i]), 0, bytes, i * 2, 2);
            }

            return bytes;
        }

        public static string BytesToString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / 2];

            for (int i = 0; i < chars.Length; i += 1)
            {
                chars[i] = BitConverter.ToChar(bytes, i * 2);
            }

            return new string(chars);
        }
    }
}
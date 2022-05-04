using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace CriminalCodeSystem.Helpers
{
    public class HashUtils
    {
        public static byte[] Hash(byte[] input)
        {
            using (SHA256 hash = SHA256.Create())
            {
                byte[] output = hash.ComputeHash(input);
                return output;
            }
        }

        public static string Hash(string input)
        {
            byte[] output = Hash(ConversionUtils.StringToBytes(input));
            return ConversionUtils.BytesToString(output);
        }
    }
}
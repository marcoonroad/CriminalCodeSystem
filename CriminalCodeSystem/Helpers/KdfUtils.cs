using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace CriminalCodeSystem.Helpers;

public class KdfUtils
{
    private static byte[] salt;
    private static int iteration = 100000;
    private static int bytes = 64;

    static KdfUtils() {
        salt = ConversionUtils.StringToBytes(UserConfig.Credentials["MASTER_KDF_SALT"]);
    }

    public static byte[] Derive(byte[] password)
    {
        string input = ConversionUtils.BytesToString(password);
        byte[] output = KeyDerivation.Pbkdf2(input, salt, KeyDerivationPrf.HMACSHA512, iteration, bytes);
        return output;
    }

    public static string Derive(string input)
    {
        byte[] output = KeyDerivation.Pbkdf2(input, salt, KeyDerivationPrf.HMACSHA512, iteration, bytes);
        return ConversionUtils.BytesToString(output);
    }
}
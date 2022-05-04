using System;
using CriminalCodeSystem.Contexts;
using CriminalCodeSystem.Models;

namespace Tests;

public class TestUtils
{
    public static string GenerateToken (string userName, int expireMinutes = 4320)
    {
        return CriminalCodeSystem.Helpers.JwtManager.GenerateToken(userName, expireMinutes);
    }

    public static string GenerateToken(string userName, DateTime notBefore, DateTime expirationPoint)
    {
        return CriminalCodeSystem.Helpers.JwtManager.GenerateToken(userName, notBefore, expirationPoint);
    }

    public static string GetHardPassword (string userName, string password)
    {
        return CriminalCodeSystem.Controllers.SharedMethods.GetHardPassword(userName, password);
    }

    public static DisabledToken NewDisabledTokenRow (string accessToken)
    {
        string byteAccessToken = CriminalCodeSystem.Helpers.HashUtils.Hash(accessToken);
        string hashedAccessToken = Convert.ToBase64String(CriminalCodeSystem.Helpers.ConversionUtils.StringToBytes(byteAccessToken));
        return new DisabledToken {
            AccessTokenHash = hashedAccessToken,
            Expiration = CriminalCodeSystem.Helpers.JwtUtils.GetExpiryTimestampFromJwt(accessToken),
        };
    }

    public static User NewUserRow (string userName, string password)
    {
        return new CriminalCodeSystem.Models.User
        {
            UserName = userName,
            Password = GetHardPassword(userName, password),
        };
    }
}
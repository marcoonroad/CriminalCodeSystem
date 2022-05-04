using System;
using CriminalCodeSystem.Helpers;
using CriminalCodeSystem.Contexts;
using CriminalCodeSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CriminalCodeSystem.Controllers;

public class SharedMethods
{
    public static string CheckAccessTokenError (string accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            return "INVALID ACCESS TOKEN";
        }

        if (DateTime.UtcNow > JwtUtils.GetExpiryTimestampFromJwt(accessToken))
        {
            return "TOKEN IS EXPIRED";
        }

        if (!JwtManager.ValidateToken(accessToken))
        {
            return "TOKEN IS NOT VALID";
        }

        return "";
    }

    public static async Task<bool> TokenIsDisabled (DisabledTokenContext context, string accessToken, bool tokenIsHashed = true)
    {
        // force hashing if raw JWT access token is passed due mistypo
        if (!tokenIsHashed || accessToken.Contains("."))
        {
            byte[] byteAccessToken = ConversionUtils.StringToBytes(accessToken);
            accessToken = Convert.ToBase64String(HashUtils.Hash(byteAccessToken));   
        }
        DisabledToken? found = await context.DisabledTokens
            .Where(token => token.AccessTokenHash == accessToken)
            .FirstOrDefaultAsync();

        return found != null;
    }

    public static async Task<bool> AuthenticationFails (UserContext context, string userName, string hardPassword)
    {
        User? found = await context.Users
            .Where(user => user.UserName == userName && user.Password == hardPassword)
            .FirstOrDefaultAsync();

        return found == null;
    }

    public static string GetHardPassword (string userName, string password)
    {
        byte[] bytePassword = ConversionUtils.StringToBytes(userName + ":" + password);
        string hardPassword = Convert.ToBase64String(KdfUtils.Derive(bytePassword));
        return hardPassword;
    }
}
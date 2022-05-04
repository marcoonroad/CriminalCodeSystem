using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CriminalCodeSystem.Models;
using CriminalCodeSystem.Request.User;
using CriminalCodeSystem.Response.User;
using CriminalCodeSystem.Helpers;
using CriminalCodeSystem.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CriminalCodeSystem.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger)
    {
        _logger = logger;
    }

    [Route("Login")]
    [HttpPost]
    public async Task<LoginResponse> LoginAsync (LoginRequest request) {
        LoginResponse response = new LoginResponse();
        UserContext userContext = new UserContext();
        
        if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
        {
            response.Status = -1;
            response.Message = "INVALID CREDENTIALS";
            return response;
        }

        string hardPassword = SharedMethods.GetHardPassword(request.UserName, request.Password);
        if (await SharedMethods.AuthenticationFails(userContext, request.UserName, hardPassword))
        {
            response.Status = -1;
            response.Message = "FAILED TO AUTHENTICATE USER";
            return response;
        }
        response.AccessToken = JwtManager.GenerateToken(request.UserName);
        response.ExpiresAfter = JwtUtils.GetExpiryTimestampFromJwt(response.AccessToken);

        return response;
    }

    [Route("ChangePassword")]
    [HttpPost]
    public async Task<ChangePasswordResponse> ChangePasswordAsync (ChangePasswordRequest request)
    {
        // NOTE: we need both AccessToken and OldPassword to ensure the user changes her password with an active session
        ChangePasswordResponse response = new ChangePasswordResponse();
        DisabledTokenContext disabledTokenContext = new DisabledTokenContext();
        UserContext userContext = new UserContext();

        string accessTokenError = SharedMethods.CheckAccessTokenError(request.AccessToken);
        if (!string.IsNullOrEmpty(accessTokenError)) {
            response.Status = -1;
            response.Message = accessTokenError;
            return response;
        }

        string userName = JwtUtils.GetUserNameFromJwt(request.AccessToken);
        if (string.IsNullOrEmpty(request.OldPassword) || string.IsNullOrEmpty(request.NewPassword))
        {
            response.Status = -1;
            response.Message = "PASSWORDS MUST BE FILLED";
            return response;
        }

        if (request.OldPassword == request.NewPassword)
        {
            response.Status = -1;
            response.Message = "NEW PASSWORD MUST BE DIFFERENT FROM OLD PASSWORD";
            return response;
        }

        if (request.NewPassword.Length < 8)
        {
            response.Status = -1;
            response.Message = "MINIMUM PASSWORD SIZE MUST BE AT LEAST 8";
            return response;
        }

        byte[] byteAccessToken = ConversionUtils.StringToBytes(request.AccessToken);
        string hashedToken = Convert.ToBase64String(HashUtils.Hash(byteAccessToken));
        
        if (await SharedMethods.TokenIsDisabled(disabledTokenContext, hashedToken)) {
            response.Status = -1;
            response.Message = "SESSION DISPOSED - TOKEN IS DISABLED";
            return response;
        }

        string hardOldPassword = SharedMethods.GetHardPassword(userName, request.OldPassword);
        string hardNewPassword = SharedMethods.GetHardPassword(userName, request.NewPassword);
        User? found = await userContext.Users
            .SingleOrDefaultAsync(user => user.UserName == userName && user.Password == hardOldPassword);
        if (found == null)
        {
            response.Status = -1;
            response.Message = "OLD PASSWORD DOESN'T MATCH";
            return response;
        }

        // NOTE: we update the password mask, disable the used token and generate a new one as well
        found.Password = hardNewPassword;
        await userContext.SetStateAsync(found, EntityState.Modified);
        await disabledTokenContext.AddAsync(new DisabledToken {
            AccessTokenHash = hashedToken,
            Expiration = JwtUtils.GetExpiryTimestampFromJwt(request.AccessToken),
        });
        await disabledTokenContext.SaveChangesAsync();
        response.NewAccessToken = JwtManager.GenerateToken(userName);

        return response;
    }

    [Route("Logoff")]
    [HttpPost]
    public async Task<LogoffResponse> LogoffAsync (LogoffRequest request)
    {
        LogoffResponse response = new LogoffResponse();
        DisabledTokenContext disabledTokenContext = new DisabledTokenContext();

        string accessTokenError = SharedMethods.CheckAccessTokenError(request.AccessToken);
        if (!string.IsNullOrEmpty(accessTokenError)) {
            response.Status = -1;
            response.Message = accessTokenError;
            return response;
        }

        byte[] byteAccessToken = ConversionUtils.StringToBytes(request.AccessToken);
        string hashedToken = Convert.ToBase64String(HashUtils.Hash(byteAccessToken));
        if (await SharedMethods.TokenIsDisabled(disabledTokenContext, hashedToken)) {
            response.Status = -1;
            response.Message = "TOKEN IS ALREADY DISABLED";
            return response;
        }

        await disabledTokenContext.AddAsync(new DisabledToken {
            AccessTokenHash = hashedToken,
            Expiration = JwtUtils.GetExpiryTimestampFromJwt(request.AccessToken),
        });
        await disabledTokenContext.SaveChangesAsync();

        return response;
    }
}
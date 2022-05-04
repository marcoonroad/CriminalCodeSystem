using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CriminalCodeSystem.Request.BackOffice;
using CriminalCodeSystem.Response.BackOffice;
using CriminalCodeSystem.Contexts;
using CriminalCodeSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CriminalCodeSystem.Controllers;

[ApiController]
[Route("[controller]")]
public class BackOfficeController : ControllerBase
{
    private readonly ILogger<BackOfficeController> _logger;

    public BackOfficeController(ILogger<BackOfficeController> logger)
    {
        _logger = logger;
    }

    [Route("RegisterNewUser")]
    [HttpPost]
    public async Task<RegisterNewUserResponse> RegisterNewUserAsync(RegisterNewUserRequest request)
    {
        RegisterNewUserResponse response = new RegisterNewUserResponse();
        UserContext userContext = new UserContext();

        if (string.IsNullOrEmpty(request.MasterToken) || string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
        {
            response.Status = -1;
            response.Message = "INVALID CREDENTIALS";
            return response;
        }

        if (request.MasterToken != UserConfig.Credentials["MASTER_ACCESS_TOKEN"])
        {
            response.Status = -1;
            response.Message = "FAILED TO AUTHENTICATE MASTER TOKEN";
            return response;
        }

        if (request.Password.Length < 8)
        {
            response.Status = -1;
            response.Message = "MINIMUM PASSWORD SIZE MUST BE AT LEAST 8";
            return response;
        }

        User? found = await userContext.Users
            .Where(user => user.UserName == request.UserName)
            .FirstOrDefaultAsync();
        if (found != null) {
            response.Status = -1;
            response.Message = "USER NAME ALREADY EXISTS";
            return response;
        }

        string hardPassword = SharedMethods.GetHardPassword(request.UserName, request.Password);
        User registered = new User {
            UserName = request.UserName,
            Password = hardPassword,
        };
        await userContext.AddAsync(registered);
        await userContext.SaveChangesAsync();

        return response;
    }

    [Route("FlushTokens")]
    [HttpPost]
    public async Task<FlushTokensResponse> FlushTokensAsync (FlushTokensRequest request)
    {
        FlushTokensResponse response = new FlushTokensResponse();
        DisabledTokenContext disabledTokenContext = new DisabledTokenContext();

        if (string.IsNullOrEmpty(request.MasterToken))
        {
            response.Status = -1;
            response.Message = "INVALID CREDENTIALS";
            return response;
        }
        if (request.MasterToken != UserConfig.Credentials["MASTER_ACCESS_TOKEN"])
        {
            response.Status = -1;
            response.Message = "FAILED TO AUTHENTICATE MASTER TOKEN";
            return response;
        }

        var expiredTokens = disabledTokenContext.DisabledTokens
            .Where(token => DateTime.UtcNow >= token.Expiration)
            .ToAsyncEnumerable();

        await foreach (DisabledToken token in expiredTokens)
        {
            response.TotalFlushedExpiredTokens += 1;
            await disabledTokenContext.SetStateAsync(token, EntityState.Deleted);
        }

        return response;
    }
}
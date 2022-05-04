using System;
using CriminalCodeSystem.Controllers;
using CriminalCodeSystem.Contexts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CriminalCodeSystem.Request.User;
using CriminalCodeSystem.Response.User;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Tests.ControllerTests;

[TestClass]
public class UserTests
{
    #region Setup section
    private UserContext userContext = null;
    private CriminalCodeContext criminalCodeContext = null;
    private StatusContext statusContext = null;
    private DisabledTokenContext disabledTokenContext = null;
    private ILoggerFactory factory = null;
    private HttpContext httpContext = null;

    [TestInitialize]
    public void TearUp()
    {
        userContext = new UserContext();
        criminalCodeContext = new CriminalCodeContext();
        statusContext = new StatusContext();
        disabledTokenContext = new DisabledTokenContext();
        factory = LoggerFactory.Create(builder => builder.AddConsole());
        httpContext = new DefaultHttpContext();

        ClearAllRows();
    }

    private void ClearAllRows()
    {
        #region Delete any existing rows
        userContext.Users.RemoveRange(userContext.Users.ToList());
        userContext.SaveChanges();

        statusContext.Statuses.RemoveRange(statusContext.Statuses.ToList());
        statusContext.SaveChanges();

        criminalCodeContext.CriminalCodes.RemoveRange(criminalCodeContext.CriminalCodes.ToList());
        criminalCodeContext.SaveChanges();

        disabledTokenContext.DisabledTokens.RemoveRange(disabledTokenContext.DisabledTokens.ToList());
        disabledTokenContext.SaveChanges();
        #endregion
    }

    [TestCleanup]
    public void DropDown()
    {
        ClearAllRows();

        userContext.Dispose();
        criminalCodeContext.Dispose();
        statusContext.Dispose();
        disabledTokenContext.Dispose();
        factory.Dispose();
    }
    #endregion

    [TestMethod]
    public async Task ChangePasswordTest()
    {
        ILogger<UserController> logger = factory.CreateLogger<UserController>();
        UserController controller = new UserController(logger);
        
        string accessToken = "";
        string newAccessToken = "";

        await userContext.Users.AddAsync(TestUtils.NewUserRow("some-random-dude", "random-dude-password"));
        await userContext.SaveChangesAsync();

        {
            LoginRequest loginRequest = new LoginRequest {
                UserName = "some-random-dude",
                Password = "very-wrong-password",
            };
            LoginResponse loginResponse = await controller.LoginAsync(loginRequest);
            Assert.AreEqual(-1, loginResponse.Status);
            Assert.AreEqual("FAILED TO AUTHENTICATE USER", loginResponse.Message);
            Assert.IsTrue(string.IsNullOrEmpty(loginResponse.AccessToken));
        }

        {
            LoginRequest loginRequest = new LoginRequest {
                UserName = "some-random-dude",
                Password = "random-dude-password",
            };
            LoginResponse loginResponse = await controller.LoginAsync(loginRequest);
            Assert.AreEqual(0, loginResponse.Status);
            Assert.AreEqual("SUCCESS", loginResponse.Message);
            Assert.IsFalse(string.IsNullOrEmpty(loginResponse.AccessToken));
            accessToken = loginResponse.AccessToken;
        }

        {
            ChangePasswordRequest request = new ChangePasswordRequest
            {
                AccessToken = accessToken,
                OldPassword = "wrong-dude-password",
                NewPassword = "new-dude-password",
            };
            ChangePasswordResponse response = await controller.ChangePasswordAsync(request);
            Assert.AreEqual(-1, response.Status);
            Assert.AreEqual("OLD PASSWORD DOESN'T MATCH", response.Message);
            Assert.IsTrue(string.IsNullOrEmpty(response.NewAccessToken));
        }

        {
            ChangePasswordRequest request = new ChangePasswordRequest
            {
                AccessToken = accessToken,
                OldPassword = "random-dude-password",
                NewPassword = "random-dude-password",
            };
            ChangePasswordResponse response = await controller.ChangePasswordAsync(request);
            Assert.AreEqual(-1, response.Status);
            Assert.AreEqual("NEW PASSWORD MUST BE DIFFERENT FROM OLD PASSWORD", response.Message);
            Assert.IsTrue(string.IsNullOrEmpty(response.NewAccessToken));
        }

        {
            ChangePasswordRequest request = new ChangePasswordRequest
            {
                AccessToken = accessToken,
                OldPassword = "random-dude-password",
                NewPassword = "new-dude-password",
            };
            ChangePasswordResponse response = await controller.ChangePasswordAsync(request);
            Assert.AreEqual(0, response.Status);
            Assert.AreEqual("SUCCESS", response.Message);
            Assert.IsFalse(string.IsNullOrEmpty(response.NewAccessToken));
            newAccessToken = response.NewAccessToken;
        }

        {
            ChangePasswordRequest request = new ChangePasswordRequest
            {
                AccessToken = accessToken,
                OldPassword = "new-dude-password",
                NewPassword = "new-dude-password-2",
            };
            ChangePasswordResponse response = await controller.ChangePasswordAsync(request);
            Assert.AreEqual(-1, response.Status);
            Assert.AreEqual("SESSION DISPOSED - TOKEN IS DISABLED", response.Message);
            Assert.IsTrue(string.IsNullOrEmpty(response.NewAccessToken));   
        }

        {
            LogoffRequest logoffRequest = new LogoffRequest
            {
                AccessToken = accessToken,
            };
            LogoffResponse logoffResponse = await controller.LogoffAsync(logoffRequest);
            Assert.AreEqual(-1, logoffResponse.Status);
            Assert.AreEqual("TOKEN IS ALREADY DISABLED", logoffResponse.Message);
        }

        {
            LogoffRequest logoffRequest = new LogoffRequest
            {
                AccessToken = newAccessToken,
            };
            LogoffResponse logoffResponse = await controller.LogoffAsync(logoffRequest);
            Assert.AreEqual(0, logoffResponse.Status);
            Assert.AreEqual("SUCCESS", logoffResponse.Message);
        }
    }
}
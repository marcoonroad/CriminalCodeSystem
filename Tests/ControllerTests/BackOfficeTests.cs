using System;
using CriminalCodeSystem.Controllers;
using CriminalCodeSystem.Contexts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CriminalCodeSystem.Request.BackOffice;
using CriminalCodeSystem.Response.BackOffice;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Tests.ControllerTests;

[TestClass]
public class BackOfficeTests
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
    public async Task FlushTokensTest()
    {
        ILogger<BackOfficeController> logger = factory.CreateLogger<BackOfficeController>();
        BackOfficeController controller = new BackOfficeController(logger);
        FlushTokensRequest request;
        FlushTokensResponse response;

        string expiredToken = TestUtils.GenerateToken("testuser1", DateTime.UtcNow.AddHours(-36), DateTime.UtcNow.AddHours(-16));
        string disabledToken = TestUtils.GenerateToken("testuser2");

        await disabledTokenContext.DisabledTokens.AddAsync(TestUtils.NewDisabledTokenRow(expiredToken));
        await disabledTokenContext.SaveChangesAsync();
        await disabledTokenContext.DisabledTokens.AddAsync(TestUtils.NewDisabledTokenRow(disabledToken));
        await disabledTokenContext.SaveChangesAsync();

        request = new FlushTokensRequest
        {
            MasterToken = "SOME-INVALID-MASTER-TOKEN",
        };
        response = await controller.FlushTokensAsync(request);
        Assert.AreEqual(-1, response.Status);
        Assert.AreEqual("FAILED TO AUTHENTICATE MASTER TOKEN", response.Message);
        Assert.AreEqual(0, response.TotalFlushedExpiredTokens);

        request = new FlushTokensRequest
        {
            MasterToken = CriminalCodeSystem.UserConfig.Credentials["MASTER_ACCESS_TOKEN"],
        };
        response = await controller.FlushTokensAsync(request);
        Assert.AreEqual(0, response.Status);
        Assert.AreEqual("SUCCESS", response.Message);
        Assert.AreEqual(1, response.TotalFlushedExpiredTokens);

        response = await controller.FlushTokensAsync(request);
        Assert.AreEqual(0, response.Status);
        Assert.AreEqual("SUCCESS", response.Message);
        Assert.AreEqual(0, response.TotalFlushedExpiredTokens);
    }

    [TestMethod]
    public async Task RegisetrNewUser()
    {
        ILogger<BackOfficeController> logger = factory.CreateLogger<BackOfficeController>();
        BackOfficeController controller = new BackOfficeController(logger);
        RegisterNewUserRequest request;
        RegisterNewUserResponse response;

        Assert.AreEqual(0, await userContext.Users.CountAsync(user => user.UserName == "testuser"));
        request = new RegisterNewUserRequest {
            MasterToken = CriminalCodeSystem.UserConfig.Credentials["MASTER_ACCESS_TOKEN"],
            UserName = "testuser",
            Password = "",
        };
        response = await controller.RegisterNewUserAsync(request);
        Assert.AreEqual(-1, response.Status);
        Assert.AreEqual("INVALID CREDENTIALS", response.Message);

        request = new RegisterNewUserRequest {
            MasterToken = "SOME-INVALID-MASTER-TOKEN",
            UserName = "testuser",
            Password = "testpassword",
        };
        response = await controller.RegisterNewUserAsync(request);
        Assert.AreEqual(-1, response.Status);
        Assert.AreEqual("FAILED TO AUTHENTICATE MASTER TOKEN", response.Message);

        request = new RegisterNewUserRequest {
            MasterToken = CriminalCodeSystem.UserConfig.Credentials["MASTER_ACCESS_TOKEN"],
            UserName = "testuser",
            Password = "test",
        };
        response = await controller.RegisterNewUserAsync(request);
        Assert.AreEqual(-1, response.Status);
        Assert.AreEqual("MINIMUM PASSWORD SIZE MUST BE AT LEAST 8", response.Message);

        request = new RegisterNewUserRequest {
            MasterToken = CriminalCodeSystem.UserConfig.Credentials["MASTER_ACCESS_TOKEN"],
            UserName = "testuser",
            Password = "testpassword",
        };
        response = await controller.RegisterNewUserAsync(request);
        Assert.AreEqual(0, response.Status);
        Assert.AreEqual("SUCCESS", response.Message);
        Assert.AreEqual(1, await userContext.Users.CountAsync(user => user.UserName == "testuser"));

        response = await controller.RegisterNewUserAsync(request);
        Assert.AreEqual(-1, response.Status);
        Assert.AreEqual("USER NAME ALREADY EXISTS", response.Message);
    }
}
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
using CriminalCodeSystem.Request.CriminalCode;
using CriminalCodeSystem.Response.CriminalCode;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using CriminalCodeSystem.Models;
using System.Collections;
using System.Collections.Generic;

namespace Tests.ControllerTests;

[TestClass]
public class CriminalCodeTests
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
    public async Task FindCriminalCodeTest()
    {
        ILogger<CriminalCodeController> logger = factory.CreateLogger<CriminalCodeController>();
        CriminalCodeController controller = new CriminalCodeController(logger);
        string accessToken = "";
        await userContext.Users.AddAsync(TestUtils.NewUserRow("user-example", "user-password"));
        await userContext.SaveChangesAsync();

        #region Retrieve some access token for API usage
        {
            ILogger<UserController> userLogger = factory.CreateLogger<UserController>();
            UserController userController = new UserController(userLogger);
            LoginRequest loginRequest = new LoginRequest {
                UserName = "user-example",
                Password = "user-password",
            };
            LoginResponse loginResponse = await userController.LoginAsync(loginRequest);
            Assert.AreEqual(0, loginResponse.Status);
            Assert.AreEqual("SUCCESS", loginResponse.Message);
            Assert.IsFalse(string.IsNullOrEmpty(loginResponse.AccessToken));
            accessToken = loginResponse.AccessToken;
        }
        #endregion

        Assert.AreEqual(0, await statusContext.Statuses.CountAsync(status => status.Name == "REVISADO"));

        {        
            AddRequest request = new AddRequest {
                AccessToken = accessToken,
                Name = "Apelação criminal. Crime contra a incolumidade pública",
                Description = "Disparo de arma de fogo em via pública (Lei 10.826/2003, art. 15). Recurso defensivo. Pretendida a absolvição por " +
                "ausência de provas. Insubsistência. Materialidade e autoria devidamente comprovadas. Depoimentos firmes e coerentes das testemunhas " +
                "oculares do ilícito amparados pelos relatos dos policiais militares responsáveis pela apreensão do artefato na residência do réu. Arma " +
                "de fogo encontrada com apenas 04 (quatro) dos 06 (seis) cartuchos. Diferença equivalente aos disparos efetuados na via pública (02 - " +
                "dois). Provas suficientes a embasar o decreto condenatório. Insurgência quanto às penas restritivas de direitos. Prestação de serviços à " +
                "comunidade fixada corretamente, segundo os ditames do CP, art. 46 e CP, art. 55. Sanção de prestação pecuniária que, de igual modo, não merece " +
                "alteração. Quantum arbitrado no mínimo legal. Matérias, outrossim, que poderão ser reexaminadas pelo juízo da execução. Sentença condenatória " +
                "mantida. Recurso conhecido e desprovido.Apelação criminal. Crime contra a incolumidade pública. Disparo de arma de fogo em via pública (Lei " +
                "10.826/2003, art. 15). Recurso defensivo. Pretendida a absolvição por ausência de provas. Insubsistência. Materialidade e autoria devidamente " +
                "comprovadas. Depoimentos firmes e coerentes das testemunhas oculares do ilícito amparados pelos relatos dos policiais militares responsáveis pela " +
                "apreensão do artefato na residência do réu. Arma de fogo encontrada com apenas 04 (quatro) dos 06 (seis) cartuchos. Diferença equivalente aos disparos " +
                "efetuados na via pública (02 - dois). Provas suficientes a embasar o decreto condenatório. Insurgência quanto às penas restritivas de direitos. Prestação " +
                "de serviços à comunidade fixada corretamente, segundo os ditames do CP, art. 46 e CP, art. 55. Sanção de prestação pecuniária que, de igual modo, " +
                "não merece alteração. Quantum arbitrado no mínimo legal. Matérias, outrossim, que poderão ser reexaminadas pelo juízo da execução. Sentença condenatória " +
                "mantida. Recurso conhecido e desprovido.",
                Status = "REVISADO",
            };
            AddResponse response = await controller.PostAsync(request);
            Assert.AreEqual(0, response.Status);
            Assert.AreEqual("SUCCESS", response.Message);
        }

        Assert.AreEqual(1, await statusContext.Statuses.CountAsync(status => status.Name == "REVISADO"));

        CriminalCode? foundCriminalCode = await criminalCodeContext.CriminalCodes
            .SingleOrDefaultAsync(criminalCode => criminalCode.Name == "Apelação criminal. Crime contra a incolumidade pública");
        
        Assert.IsNotNull(foundCriminalCode);
        
        User? foundUser = await userContext.Users.SingleOrDefaultAsync(user => user.Id == foundCriminalCode.CreateUserId);
        Assert.IsNotNull(foundUser);
        Assert.AreEqual("user-example", foundUser.UserName);
    }
}
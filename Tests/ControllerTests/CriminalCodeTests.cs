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
                Name = "Apela????o criminal. Crime contra a incolumidade p??blica",
                Description = "Disparo de arma de fogo em via p??blica (Lei 10.826/2003, art. 15). Recurso defensivo. Pretendida a absolvi????o por " +
                "aus??ncia de provas. Insubsist??ncia. Materialidade e autoria devidamente comprovadas. Depoimentos firmes e coerentes das testemunhas " +
                "oculares do il??cito amparados pelos relatos dos policiais militares respons??veis pela apreens??o do artefato na resid??ncia do r??u. Arma " +
                "de fogo encontrada com apenas 04 (quatro) dos 06 (seis) cartuchos. Diferen??a equivalente aos disparos efetuados na via p??blica (02 - " +
                "dois). Provas suficientes a embasar o decreto condenat??rio. Insurg??ncia quanto ??s penas restritivas de direitos. Presta????o de servi??os ?? " +
                "comunidade fixada corretamente, segundo os ditames do CP, art. 46 e CP, art. 55. San????o de presta????o pecuni??ria que, de igual modo, n??o merece " +
                "altera????o. Quantum arbitrado no m??nimo legal. Mat??rias, outrossim, que poder??o ser reexaminadas pelo ju??zo da execu????o. Senten??a condenat??ria " +
                "mantida. Recurso conhecido e desprovido.Apela????o criminal. Crime contra a incolumidade p??blica. Disparo de arma de fogo em via p??blica (Lei " +
                "10.826/2003, art. 15). Recurso defensivo. Pretendida a absolvi????o por aus??ncia de provas. Insubsist??ncia. Materialidade e autoria devidamente " +
                "comprovadas. Depoimentos firmes e coerentes das testemunhas oculares do il??cito amparados pelos relatos dos policiais militares respons??veis pela " +
                "apreens??o do artefato na resid??ncia do r??u. Arma de fogo encontrada com apenas 04 (quatro) dos 06 (seis) cartuchos. Diferen??a equivalente aos disparos " +
                "efetuados na via p??blica (02 - dois). Provas suficientes a embasar o decreto condenat??rio. Insurg??ncia quanto ??s penas restritivas de direitos. Presta????o " +
                "de servi??os ?? comunidade fixada corretamente, segundo os ditames do CP, art. 46 e CP, art. 55. San????o de presta????o pecuni??ria que, de igual modo, " +
                "n??o merece altera????o. Quantum arbitrado no m??nimo legal. Mat??rias, outrossim, que poder??o ser reexaminadas pelo ju??zo da execu????o. Senten??a condenat??ria " +
                "mantida. Recurso conhecido e desprovido.",
                Status = "REVISADO",
            };
            AddResponse response = await controller.PostAsync(request);
            Assert.AreEqual(0, response.Status);
            Assert.AreEqual("SUCCESS", response.Message);
        }

        Assert.AreEqual(1, await statusContext.Statuses.CountAsync(status => status.Name == "REVISADO"));

        CriminalCode? foundCriminalCode = await criminalCodeContext.CriminalCodes
            .SingleOrDefaultAsync(criminalCode => criminalCode.Name == "Apela????o criminal. Crime contra a incolumidade p??blica");
        
        Assert.IsNotNull(foundCriminalCode);
        
        User? foundUser = await userContext.Users.SingleOrDefaultAsync(user => user.Id == foundCriminalCode.CreateUserId);
        Assert.IsNotNull(foundUser);
        Assert.AreEqual("user-example", foundUser.UserName);
    }
}
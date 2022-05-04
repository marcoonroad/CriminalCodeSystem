using System;

namespace CriminalCodeSystem.Request.BackOffice;

public class RegisterNewUserRequest
{
    public string MasterToken { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}
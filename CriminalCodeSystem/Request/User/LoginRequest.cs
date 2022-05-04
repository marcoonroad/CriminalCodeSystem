using System;

namespace CriminalCodeSystem.Request.User;

public class LoginRequest
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}
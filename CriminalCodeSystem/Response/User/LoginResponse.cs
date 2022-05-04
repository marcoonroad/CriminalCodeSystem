using System;

namespace CriminalCodeSystem.Response.User;

public class LoginResponse
{
    public int Status { get; set; } = 0;
    public string Message { get; set; } = "SUCCESS";
    public string AccessToken { get; set; } = "";
    public DateTime ExpiresAfter { get; set; }
}
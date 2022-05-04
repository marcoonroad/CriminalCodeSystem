using System;

namespace CriminalCodeSystem.Response.User;

public class ChangePasswordResponse
{
    public int Status { get; set; } = 0;
    public string Message { get; set; } = "SUCCESS";
    public string NewAccessToken { get; set; } = "";
}
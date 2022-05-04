using System;

namespace CriminalCodeSystem.Response.BackOffice;

public class RegisterNewUserResponse
{
    public int Status { get; set; } = 0;
    public string Message { get; set; } = "SUCCESS";
}
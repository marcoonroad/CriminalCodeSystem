using System;

namespace CriminalCodeSystem.Response.User;

public class LogoffResponse
{
    public int Status { get; set; } = 0;
    public string Message { get; set; } = "SUCCESS";
}
using System;

namespace CriminalCodeSystem.Response.CriminalCode;

public class AddResponse
{
    public int Status { get; set; } = 0;
    public string Message { get; set; } = "SUCCESS";
}
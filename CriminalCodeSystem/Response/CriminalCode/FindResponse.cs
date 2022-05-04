using System;
using CriminalCodeModel = CriminalCodeSystem.Models.CriminalCode;

namespace CriminalCodeSystem.Response.CriminalCode;

public class FindResponse
{
    public int Status { get; set; } = 0;
    public string Message { get; set; } = "SUCCESS";
    public CriminalCodeModel? FoundCriminalCode { get; set; }
}
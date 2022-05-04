using System;
using System.Collections;
using System.Collections.Generic;
using CriminalCodeModel = CriminalCodeSystem.Models.CriminalCode;

namespace CriminalCodeSystem.Response.CriminalCode;

public class ListResponse
{
    public int Status { get; set; } = 0;
    public string Message { get; set; } = "SUCCESS";

    public int NextId { get; set; }
    public int PageLength { get; set; }
    public List<CriminalCodeModel> CriminalCodes { get; set; } = new();
}
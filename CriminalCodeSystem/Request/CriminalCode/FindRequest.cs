using System;

namespace CriminalCodeSystem.Request.CriminalCode;

public class FindRequest
{
    public int CriminalCodeId { get; set; }
    public string CriminalCodeName { get; set; } = "";
}
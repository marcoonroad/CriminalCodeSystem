using System;

namespace CriminalCodeSystem.Request.CriminalCode;

public class ExcludeRequest
{
    public string AccessToken { get; set; } = "";
    public int CriminalCodeId { get; set; }
    public string CriminalCodeName { get; set; } = "";
}
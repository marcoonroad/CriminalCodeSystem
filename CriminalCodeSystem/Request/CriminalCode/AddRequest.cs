using System;

namespace CriminalCodeSystem.Request.CriminalCode;

public class AddRequest
{
    public string AccessToken { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public Decimal Penalty { get; set; }
    public int PrisonTime { get; set; }
    public string Status { get; set; } = "";
}
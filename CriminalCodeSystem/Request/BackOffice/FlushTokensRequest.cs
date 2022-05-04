using System;

namespace CriminalCodeSystem.Request.BackOffice;

public class FlushTokensRequest
{
    public string MasterToken { get; set; } = "";
}
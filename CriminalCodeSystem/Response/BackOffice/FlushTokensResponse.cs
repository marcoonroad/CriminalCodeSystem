using System;

namespace CriminalCodeSystem.Response.BackOffice;

public class FlushTokensResponse
{
    public int Status { get; set; } = 0;
    public string Message { get; set; } = "SUCCESS";
    public long TotalFlushedExpiredTokens { get; set; } = 0;
}
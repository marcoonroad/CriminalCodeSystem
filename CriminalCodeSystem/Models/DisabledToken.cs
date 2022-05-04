using System;

namespace CriminalCodeSystem.Models;

public class DisabledToken
{
    public int Id { get;set; }
    public string AccessTokenHash { get; set; } = "";
    public DateTime Expiration { get; set; }
}
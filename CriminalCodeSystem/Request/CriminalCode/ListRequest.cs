using System;

namespace CriminalCodeSystem.Request.CriminalCode;

public class ListRequest
{
    public string AccessToken { get; set; } = "";
    public int NextId { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; } = 0;

    public string FilterName { get; set; } = "";
    public string OrderName = "";

    public string FilterDescription { get; set; } = "";
    public string OrderDescription { get; set; } = "";

    public Decimal FilterPenalty { get; set; }
    public string OrderPenalty { get; set; } = "";
    
    public int FilterPrisonTime { get; set; }
    public string OrderPrisonTime { get; set; } = "";

    public DateTime FilterCreateDate { get; set; } = DateTime.MinValue;
    public string OrderCreateDate { get; set; } = "";

    public DateTime FilterUpdateDate { get; set; } = DateTime.MinValue;
    public string OrderUpdateDate { get; set; } = "";

    public string FilterStatus { get; set; } = "";
    public string FilterCreateUserName { get; set; } = "";
    public string FilterUpdateUserName { get; set; } = "";
}
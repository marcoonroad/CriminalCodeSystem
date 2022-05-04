using System;

namespace CriminalCodeSystem.Models;

public class CriminalCode
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public Decimal Penalty { get; set; }
    public int PrisonTime { get; set; }
    public int StatusId { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public int CreateUserId { get; set; }
    public int UpdateUserId { get; set; }
}
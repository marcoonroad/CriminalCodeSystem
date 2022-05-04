using System;
using Microsoft.EntityFrameworkCore;
using CriminalCodeSystem.Models;

namespace CriminalCodeSystem.Contexts;

public class CriminalCodeContext : ContextTemplate
{
    public DbSet<CriminalCode> CriminalCodes { get; set; }
}
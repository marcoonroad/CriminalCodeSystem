using System;
using Microsoft.EntityFrameworkCore;
using CriminalCodeSystem.Models;

namespace CriminalCodeSystem.Contexts;

public class DisabledTokenContext : ContextTemplate
{
    public DbSet<DisabledToken> DisabledTokens { get; set; }
}
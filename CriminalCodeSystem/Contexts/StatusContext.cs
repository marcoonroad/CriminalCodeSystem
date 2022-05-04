using System;
using Microsoft.EntityFrameworkCore;
using CriminalCodeSystem.Models;

namespace CriminalCodeSystem.Contexts;

public class StatusContext : ContextTemplate
{
    public DbSet<Status> Statuses { get; set; }
}
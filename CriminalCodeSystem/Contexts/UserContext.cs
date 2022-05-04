using System;
using Microsoft.EntityFrameworkCore;
using CriminalCodeSystem.Models;

namespace CriminalCodeSystem.Contexts;

public class UserContext : ContextTemplate
{
    public DbSet<User> Users { get; set; }
}
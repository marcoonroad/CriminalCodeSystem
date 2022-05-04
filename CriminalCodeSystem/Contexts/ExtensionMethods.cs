using System;
using CriminalCodeSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CriminalCodeSystem.Contexts;

public static class ExtensionMethods
{
    public static async Task SetStateAsync (this UserContext context, User row, EntityState state)
    {
        context.Entry(row).State = state;
        await context.SaveChangesAsync();
    }

    public static async Task SetStateAsync (this StatusContext context, Status row, EntityState state)
    {
        context.Entry(row).State = state;
        await context.SaveChangesAsync();
    }

    public static async Task SetStateAsync (this DisabledTokenContext context, DisabledToken row, EntityState state)
    {
        context.Entry(row).State = state;
        await context.SaveChangesAsync();
    }

    public static async Task SetStateAsync (this CriminalCodeContext context, CriminalCode row, EntityState state)
    {
        context.Entry(row).State = state;
        await context.SaveChangesAsync();
    }
}
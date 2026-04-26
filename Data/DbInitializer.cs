using FluentValidation.TestHelper;
using MediApp.Identity;
using MediApp.Models;
using MediApp.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.EntityFrameworkCore;

namespace MediApp.Data;

public static class DbInitializer
{
    public async static Task SeedData(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        await IdentitySeeder.SeedIdentities(serviceProvider);
        await MedicationSeeder.SeedMedications(serviceProvider);
    }
}
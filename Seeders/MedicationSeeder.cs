using MediApp.Data;
using MediApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediApp.Seeders;

public static class MedicationSeeder
{
    public async static Task SeedMedications(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await userManager.FindByEmailAsync("test@hotmail.com");

        if(user == null)
        {
            throw new Exception($"user does not exist{user}");
        }
        
        if(!await dbContext.Medications.AnyAsync(i => i.UserId == user.Id))
        {
            var medications = new List<Medication>()
            {
                new Medication
                {
                    Name = "Ibuprofen",
                    Dose = 200,
                    Instructions = "take one a day",
                    StartDate = DateTime.Now,
                    EndDate = new DateTime(2026, 03 , DateTime.DaysInMonth(2026, 03))
                },
                new Medication
                {
                    Name = "Paracetamol",
                    Dose = 300,
                    Instructions = "Take two daily",
                    StartDate = DateTime.Now,
                    EndDate = new DateTime(2026, 03, DateTime.DaysInMonth(2026,03)),
                    UserId = user.Id
                }, 
                new Medication
                {
                    Name = "Viagra",
                    Dose = 200,
                    Instructions = "Take half daily",
                    StartDate = DateTime.Now,
                    EndDate = new DateTime(2026, 03, DateTime.DaysInMonth(2026,03)),
                    UserId = user.Id
                }, 
                new Medication
                {
                    Name = "Morphine",
                    Dose = 1000,
                    Instructions = "Take ten daily",
                    StartDate = DateTime.Now,
                    EndDate = new DateTime(2026, 03, DateTime.DaysInMonth(2026,03)),
                    UserId = user.Id
                }
            };

            await dbContext.Medications.AddRangeAsync(medications);
            await dbContext.SaveChangesAsync();
        }
    }
}
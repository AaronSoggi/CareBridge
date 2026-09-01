using MediApp.Data;
using MediApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediApp.Seeders;
// creating medications for 
public static class MedicationSeeder
{
    public async static Task SeedMedications(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await userManager.FindByEmailAsync("fred@hotmail.com");

        if(user == null)
        {
            throw new Exception($"user does not exist{user}");
        }

        // Get the patient linked to this user
        var patient = await dbContext.Patients
            .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);

        if (patient == null)
            throw new Exception("Patient does not exist for this user");
        
        if(!await dbContext.Medications.AnyAsync(i => i.PatientId == patient.Id))
        {
            var medications = new List<Medication>()
            {
                new Medication
                {
                    Name = "Ibuprofen",
                    Dose = 200,
                    Instructions = "take one a day",
                    StartDate = DateTime.Now,
                    EndDate = new DateTime(2026, 03 , DateTime.DaysInMonth(2026, 03)),
                    PatientId = patient.Id
                },
                new Medication
                {
                    Name = "Paracetamol",
                    Dose = 300,
                    Instructions = "Take two daily",
                    StartDate = DateTime.Now,
                    EndDate = new DateTime(2026, 03, DateTime.DaysInMonth(2026,03)),
                    PatientId = patient.Id
                }, 
                new Medication
                {
                    Name = "Viagra",
                    Dose = 200,
                    Instructions = "Take half daily",
                    StartDate = DateTime.Now,
                    EndDate = new DateTime(2026, 03, DateTime.DaysInMonth(2026,03)),
                    PatientId = patient.Id
                }, 
                new Medication
                {
                    Name = "Morphine",
                    Dose = 1000,
                    Instructions = "Take ten daily",
                    StartDate = DateTime.Now,
                    EndDate = new DateTime(2026, 03, DateTime.DaysInMonth(2026,03)),
                    PatientId = patient.Id
                }
            };

            await dbContext.Medications.AddRangeAsync(medications);
            await dbContext.SaveChangesAsync();
        }
    }
}



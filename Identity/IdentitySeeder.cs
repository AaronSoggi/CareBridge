using System.Runtime.CompilerServices;
using System.Security.Claims;
using MediApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace MediApp.Identity;

public static class IdentitySeeder
{
    public async static Task SeedIdentities(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = 
        {
          Roles.Admin,
          Roles.Doctor,
          Roles.Patient
        };

        foreach(var role in roles)
        {
            if(!await roleManager.RoleExistsAsync(role))
            {
               var result = await roleManager.CreateAsync(new IdentityRole(role));

                if (!result.Succeeded)
                {
                    var errors = string.Join(",", result.Errors.Select(e => e.Description));
                    throw new Exception($"something when wrong when attempting to add a role {errors}");
                }
            }
        }

        var patient = await SeedUser(userManager, "Freddy", "Fredz", "fred@hotmail.com", Roles.Patient);
        var doctor = await SeedUser(userManager, "Monica", "Kumar", "monica@hotmai.com", Roles.Doctor);

        var claims = await userManager.GetClaimsAsync(doctor);

        if(!claims.Any(i => i.Type.Equals(Policies.VerifiedDoctor)))
        {
            var result = await userManager.AddClaimAsync(doctor, new Claim(Policies.VerifiedDoctor, "false"));

            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                throw new Exception($"something went wront when attempting to add a claim to the user {errors}");
            }
        }
    }

    private static async Task<ApplicationUser> SeedUser(UserManager<ApplicationUser> user, 
    string firstName, string lastName, string email, string role)
    {
        var applicationUser = await user.FindByEmailAsync(email);

        if(applicationUser == null)
        {
            applicationUser = new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                Created = DateTime.UtcNow,
                UserName = email,
                Email = email,
                PhoneNumber = "07000000000"
            };

            var result = await user.CreateAsync(applicationUser, "Password123!");
            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e =>e.Description));
                throw new Exception($"Something went wrong {errors}");
            }
        }

        //Checking if user is in a role
        var isInRole = await user.IsInRoleAsync(applicationUser,role);

        if (!isInRole)
        {
            var result = await user.AddToRoleAsync(applicationUser,role);

            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                throw new Exception($"sommething went wrong when assigning user to role {errors}");
            }
        }

        return applicationUser;
        
    }
}
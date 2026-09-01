using MediApp.Data;
using MediApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MediApp.Identity;
using System.Reflection.Metadata.Ecma335;


namespace MediApp.Services;

public class DoctorService : IDoctorService
{

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DoctorService> _logger;
    public DoctorService(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext,
    ILogger<DoctorService> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    //VerifyDoctorAsync method

    public async Task<ServiceResult> VerifyDoctorAsync(string userId)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(i => i.Id == userId);

             if(user == null)
            {
                _logger.LogWarning("User {Userid} could not be found in the system", userId);
                return ServiceResult.Missing("User could not be found");
            }

            var isDoctor = await _userManager.IsInRoleAsync(user, Roles.Doctor);

            if (!isDoctor)
            {
                _logger.LogWarning("The user you are trying to verify is not a doctor");
                return ServiceResult.Fail("User is not a doctor");
            }

            // if user is a doctor then we want to access what claims they currently have

            var claims = await _userManager.GetClaimsAsync(user);
            var existing = claims.FirstOrDefault(i => i.Type == "IsVerified" && i.Value == "false");

            IdentityResult result;

            if(existing != null)
            {
                result =  await _userManager.ReplaceClaimAsync(user, existing, new Claim("IsVerfified", "true"));
            }
            else
            {
                _logger.LogInformation("Doctor has already been verified");
                return ServiceResult.Fail("Doctor has already been verified");
            }

            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(i => i.Description));
                _logger.LogWarning($"something went wrong whilst trying to verify the doctor {userId}: {errors}", userId, errors);
                return ServiceResult.Fail("Doctor could not be verified");
            }

            // Claims live in the auth cookie — bump the stamp so the doctor's
            // existing cookie is rejected and reissued with the new claim.
            await _userManager.UpdateSecurityStampAsync(user);

            return ServiceResult.Ok("Doctor has been verified succesfully");
 
        }
        catch(Exception ex)
        {
            _logger.LogError($"Unfortunately something went wrong whilst trying to verify the doctor, please try again {ex.Message}");
            return ServiceResult.Fail("Something went wrong during the verfication process.");
        }   
    }
}
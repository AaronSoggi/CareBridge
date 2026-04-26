using MediApp.Data;
using MediApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MediApp.Identity;


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

    public async Task<ServiceResult> VerifyDoctorAsync(string userId)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(i => i.Id == userId);

        if(user == null)
        {
            _logger.LogWarning("Unable to fetch user from the database");
            return ServiceResult.Missing("Doctor cannot be found");
        }

        //check if the user is a doctor
        var IsDoctor = await _userManager.IsInRoleAsync(user, Roles.Doctor);

        if (!IsDoctor)
        {
            _logger.LogWarning("Unable to complete request as user is not a doctor");
            return ServiceResult.Fail("User is not a doctor");
        }

        // fetching claims
        var existingClaims = await _userManager.GetClaimsAsync(user);

        // checking if claims contain IsVerified
        var IsVerifiedClaim = existingClaims.FirstOrDefault(i => i.Type == "IsVerified");

        IdentityResult result;

        if(IsVerifiedClaim == null)
        {
            result = await _userManager.AddClaimAsync(user, new Claim("IsVerified", "true"));
        } 
        else if(IsVerifiedClaim.Value == "false")
        {
            result = await _userManager.
            ReplaceClaimAsync(user, IsVerifiedClaim, 
            new Claim("IsVerified", "true"));
        }
        else
        {
            return ServiceResult.Fail("Doctor is already verified");
        }

        if (!result.Succeeded)
        {
            var errors = string.Join(",", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Something went wrong when attempting to verify the doctor: {userId} {errors}", user.Id, errors);
            return ServiceResult.Fail("Something went wrong during the verification process");
        }

        return ServiceResult.Ok("Doctor has been verified succesfully");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Something went wrong when attempting to verify doctor");
            return ServiceResult.Missing("Something went wrong, failed to verify doctor");
        }
    }
}
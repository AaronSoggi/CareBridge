using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using MediApp.Configuration;
using MediApp.DTOs;
using MediApp.Identity;
using MediApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;


namespace MediApp.Controllers;

public class AuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthController> _logger;
    private readonly IOptions<MedicationClientSettings> _apiSettings;

    private readonly IConfiguration _config;
    public AuthController(SignInManager<ApplicationUser> signInManager, 
    UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> rolemanager, IMapper mapper,
    ILogger<AuthController> logger, IOptions<MedicationClientSettings> apiSettings, IConfiguration config)
    {
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = rolemanager ?? throw new ArgumentNullException(nameof(rolemanager));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
        _apiSettings = apiSettings;
        _config = config;
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto)
    {

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("","Login failed");
            return View(dto);
        }

        var result = await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, isPersistent: dto.RememberMe, lockoutOnFailure: true );

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Unable to log user in as they have been locked out for too many failed login attempts");
            ModelState.AddModelError("", $"User has been locked out, please try again later");  
            return View(dto); 
        }

        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(dto);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index","Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Something went wrong please check passwords and try again");
            return View(dto);
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);

        if(user == null)
        {
            return Unauthorized();
        }

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

        if (!result.Succeeded)
        {
            foreach(var error in result.Errors)
            {
                ModelState.AddModelError("", $"Unable to complete request: {error}");
            }
            return View(dto);
        }

        // for now return to user dashboard - but will need to return to user profile page once implemented.
        return RedirectToAction("Index", "Home");
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "An issue occured when the user attempted to change their password");
            ModelState.AddModelError("", "Something happened, please try again");
            return View(dto);
        }

        
    }
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterPatient(CreateUserDto dto)
    {

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Something went wrong, please check values and try again");
            return View(dto);
        }

        try
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);

        // checking if user already exists
        if(existingUser != null)
        {
            ModelState.AddModelError("", "Email already exists, please try again");
            return View(dto);
        }
        
        var user = _mapper.Map<ApplicationUser>(dto);

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            foreach(var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(dto);
        }

        var userRoleResult = await _userManager.AddToRoleAsync(user, Roles.Patient);

        if (!userRoleResult.Succeeded)
        {
            var roleErrors = string.Join(",", userRoleResult.Errors.Select(e => e.Description));
             _logger.LogWarning($"an error occured when assigning a role to the user: {roleErrors}");

            foreach(var error in userRoleResult.Errors)
            {
                ModelState.AddModelError("", $"An error occured when trying to assign a role: {error.Description}");
            }
            return View(dto);
        }

        return RedirectToAction(nameof(Login));

        }
        catch(Exception ex)
        {
            _logger.LogError(ex, $"Something went wrong during registration");
            ModelState.AddModelError("", "Something went wrong during registration");
            return View(dto);
        } 
    }
}
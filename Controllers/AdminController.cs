using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using MediApp.Identity;
using MediApp.Models;
using MediApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MediApp.Controllers;

public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminController> _logger;
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;
    private readonly IAdminService _adminService;
    public AdminController(UserManager<ApplicationUser> userManager, ILogger<AdminController> logger,
     IPatientService patientService, IDoctorService doctorService, IAdminService adminService)
    {
        _userManager = userManager;
        _logger = logger;
        _patientService = patientService;
        _doctorService = doctorService;
        _adminService = adminService;
    }

    public IActionResult Dashboard()
    {
        return View();  
    }

    // dont wanna add authorize for testin purposes
    [HttpGet]
    public async Task<IActionResult> PatientInfo()
    {
        // we want to fetch the medication and user data from the database and then return it to the view
        var userId = _userManager.GetUserId(User);

        if(userId == null)
        {
            _logger.LogWarning("Please make sure user is logged in before carrying out this request");
            return Unauthorized();
        }

        var patientinfo = await _patientService.GetPatientInfo();

        if(patientinfo == null)
        {
            _logger.LogWarning("Unable to retrieve data from the database");
            return NotFound();
        }

       return View(patientinfo); 
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyDoctor(string userId)
    {
        var user = _userManager.GetUserId(User);

        if(user == null)
        {
            _logger.LogWarning("Please ensure user is logged in before making this request");
            return Unauthorized();         
        }

        var result = await _doctorService.VerifyDoctorAsync(userId);

        if (!result.Success)
        {
            if(result.NotFound)
            {
                return NotFound();
            }
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(PendingDoctors));
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(PendingDoctors));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> PendingDoctors()
    {
        var userId = _userManager.GetUserId(User);

        if(userId == null)
        {
            _logger.LogWarning("Please ensure user is logged in before carrying out this request");
            return Unauthorized();
        }

        var doctors = await _adminService.GetPendingDoctors();

        if(!doctors.Any())
        {
            _logger.LogWarning("There doesnt seem to be any pending doctors");
        }

        return View(doctors);
    }
}
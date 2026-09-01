using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using MediApp.DTOs;
using MediApp.Identity;
using MediApp.Models;
using MediApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MediApp.Controllers;

[Route("admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminController> _logger;
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;
    private readonly IAdminService _adminService;

    private readonly IProductClient _productClient;
    public AdminController(UserManager<ApplicationUser> userManager, ILogger<AdminController> logger,
     IPatientService patientService, IDoctorService doctorService, IAdminService adminService,
     IProductClient productClient)
    {
        _userManager = userManager;
        _logger = logger;
        _patientService = patientService;
        _doctorService = doctorService;
        _adminService = adminService;
        _productClient = productClient;
    }

    // public async Task<IActionResult> GetPatientInfo()
    // {
    //     var user = _userManager.GetUserId(User);

    //     if(user == null)
    //     {
    //         _logger.LogWarning("Please can you ensure that user is logged in before carrying out this request");
    //         return Unauthorized();
    //     }

    //     var patientInfo = await _patientService.GetPatientInfoAsync();

    //     return View(patientInfo);
    // }

    public IActionResult Dashboard()
    {
        return View();  
    }


    //VerifyDoctor endpoint
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyDoctor(string userId)
    {
        var user = _userManager.GetUserId(User);

        if(user == null)
        {
            _logger.LogWarning("Please ensure that user is logged in before carrying out the request");
            return Unauthorized();
        }

        var result = await _doctorService.VerifyDoctorAsync(userId);

        if (!result.Success)
        {
            if(result.NotFound)
            {
                _logger.LogWarning("User could not be found");
                return NotFound();
            }

            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(PendingDoctors));
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(PendingDoctors));
    }


    [HttpGet]
    public async Task<IActionResult> PendingDoctors()
    {
        var userId = _userManager.GetUserId(User);

        if(userId == null)
        {
            _logger.LogWarning("unable to carry our this request as the user is not signed in");
            return Unauthorized();
        }

        var pendingDoctors = await _adminService.GetPendingDoctors();

        if (!pendingDoctors.Any())
        {
            _logger.LogWarning("There doesnt seem to be any doctors pending in the system");
        }

        return View(pendingDoctors);
    }


    [HttpGet]
    [AllowAnonymous]
    [Route("get-product/{id}")]
    public async Task<IActionResult> GetProductAsync(int id)
    {
        var result = await _productClient.GetAsync(id);
        
        if(result == null)
        {
            _logger.LogWarning("Unable to fetch the specified product");
            return NotFound();
        }   

        var product = new ProductDto
        {
            Id = result.Id,
            Title = result.Title,
            Description = result.Description,
            Category = result.Category,
            Price = result.Price,
            DiscountPercentage = result.DiscountPercentage,
            Rating = result.Rating,
            Tags = result.Tags         
        };

        return Json(product);   
            
    }

    // here we need to be able to create/update and delete users. these can be admin/doctor or patient.
}
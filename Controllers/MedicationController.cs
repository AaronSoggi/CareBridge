using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediApp.Data;
using MediApp.DTOs;
using MediApp.Identity;
using MediApp.Models;
using MediApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;

namespace MediApp.Controllers;

public class MedicationController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMedicationService _medicationService;
    private readonly ILogger<MedicationController> _logger;
    private readonly ApplicationDbContext _dbcontext;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;

    public MedicationController(UserManager<ApplicationUser> userManager
    , ILogger<MedicationController> logger, ApplicationDbContext dbContext, IMapper mapper, IMedicationService medicationService
    ,IMemoryCache cache)
    {
        _userManager = userManager;
        _logger = logger;
        _dbcontext = dbContext;
        _mapper = mapper;
        _medicationService = medicationService;
        _cache = cache;
    }
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
    {
        var userId = _userManager.GetUserId(User);

        if(userId == null)
        {
            _logger.LogWarning("Please ensure user is logged in before attempting to access the dashboard");
            return Unauthorized();
        }
        
        var medicationListDto = await _medicationService.GetMedicationsAsync(userId, pageNumber, pageSize);

        return View(medicationListDto);
    }
    [HttpGet]
    [Authorize]
    public IActionResult Create()
    {
        return View(new CreateMedicationDto());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMedicationDto dto)
    {
        var userId = _userManager.GetUserId(User);
        
        if(userId == null)
        {
            _logger.LogWarning("Unfortunately this user doesnt exist {userId}", userId);
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Please check fields and try again");
            ModelState.AddModelError("", "Something went wrong, please ensure data meets requirements");
            return View(dto);
        }

        var result = await _medicationService.CreateMedicationAsync(dto, userId);

        if (!result.Success)
        {
            ModelState.AddModelError("", $"something went wrong during the registration process: {result.Message}");
            return View(dto);
        }

        TempData["success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet] 
    [Authorize(Roles = Roles.Patient)]
    public async Task<IActionResult> Update(int id)
    {
        var userId = _userManager.GetUserId(User);

        if(userId == null)
        {
            _logger.LogWarning("User is attempting to edit a medication when not logged in");
            return Unauthorized();
        }

        var medication = await _medicationService.GetUpdateMedicationAsync(userId, id);

        if(medication == null)
        {
            _logger.LogWarning("something went wrong when fetching the medication");
            return NotFound();
        }

        return View(medication);

    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateMedicationDto dto)
    {
        var UserId = _userManager.GetUserId(User);

        if(UserId == null)
        {
            _logger.LogWarning("Please ensure user is logged in before carrying out this request");
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Please can you check all fields anmale sure they meet the requirements");
            ModelState.AddModelError("", "Please ensure that all fields have been filled our correctly");
            return View(dto);
        }

        var result = await _medicationService.UpdateMedicationAsync(dto, UserId);

        if (!result.Success)
        {
            if (result.NotFound)
            {
                return NotFound();
            }
            ModelState.AddModelError("", $"{result.Message}");
            return View(dto);
        }

        TempData["success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Patient)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var patientId = _userManager.GetUserId(User);

        if(patientId == null)
        {
            _logger.LogWarning("Unable to delete medication when user isnt signed in");
            return Forbid();
        }

        var medication = await _dbcontext.Medications
        .FirstOrDefaultAsync(i => i.Id == id && i.PatientId.ToString() == patientId);

        if(medication == null)
        {
            _logger.LogWarning("something went wrong, unable to remove medication");
            return NotFound();
        }

        _dbcontext.Medications.Remove(medication);
        await _dbcontext.SaveChangesAsync();
        TempData["Success"] = "Medication has been removed succesfully";
        return RedirectToAction(nameof(Index));
    }
}
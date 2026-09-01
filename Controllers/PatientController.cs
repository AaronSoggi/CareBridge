using MediApp.Configuration;
using MediApp.Identity;
using MediApp.Models;
using MediApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;


namespace MediApp.Controllers;

public class PatientController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<PatientController> _logger;
    private readonly IPatientService _patientService;

    private readonly IOptions<MedicationClientSettings> _apiSettings;
    public PatientController(UserManager<ApplicationUser> userManager, ILogger<PatientController> logger,
    IPatientService patientService, IOptions<MedicationClientSettings> apiSettings)
    {
        _userManager = userManager;
        _logger = logger;
        _patientService = patientService;
        _apiSettings = apiSettings;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetPatients(int pageNumber = 1, int pageSize = 10)
    {
        var patients = await _patientService.GetPatientsAsync(pageNumber, pageSize);        
        return View(patients);
    }

    // we want to be able to create a patient/ delete a patient and update a patient.
    // we want to list all the patients on the index.cshtml - but we want to also include some other information 
    // we want to include the doctor for each patient and also have a appointments button which will open a new page.
}
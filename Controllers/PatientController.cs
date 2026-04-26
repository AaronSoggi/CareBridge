using MediApp.Configuration;
using MediApp.Models;
using MediApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;


namespace MediApp.Controllers;

public class PatientController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<PatientController> _logger;
    private readonly IPatientService _patientService;

    private readonly IOptions<MedicationApiSettings> _apiSettings;
    public PatientController(UserManager<ApplicationUser> userManager, ILogger<PatientController> logger,
    IPatientService patientService, IOptions<MedicationApiSettings> apiSettings)
    {
        _userManager = userManager;
        _logger = logger;
        _patientService = patientService;
        _apiSettings = apiSettings;
    }
}
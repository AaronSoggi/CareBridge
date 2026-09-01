using System.Security.Claims;
using MediApp.Controllers;
using MediApp.Services;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using MediApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediApp.DTOs;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MediApp.Identity;

namespace MediApp.MediApp.Tests;

public class AdminControllerTests
{
    private readonly Mock<ILogger<AdminController>> _loggerMock;
    private readonly Mock<IPatientService> _mockPatientService;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IDoctorService> _mockDoctorService;
    private readonly Mock<IAdminService> _mockAdminService;

    public AdminControllerTests()
    {
        _mockPatientService = new Mock<IPatientService>();
        _loggerMock = new Mock<ILogger<AdminController>>();

        var userStore = new Mock<IUserStore<ApplicationUser>>();

        _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore.Object);
        _mockAdminService = new Mock<IAdminService>();
        _mockDoctorService = new Mock<IDoctorService>();

    }

    // [Fact]
    // public async Task GetPatientInfo_IsValid_returnsViewWithPatientInfo()
    // {
    //     var userId = Guid.NewGuid().ToString();

    //     var userClaim = new ClaimsPrincipal(new ClaimsIdentity(new[]
    //     {
    //         new Claim(ClaimTypes.NameIdentifier, userId)
    //     }));

    //     var controller = new AdminController(_mockUserManager.Object, _loggerMock.Object, _mockPatientService.Object
    //     , _mockDoctorService.Object, _mockAdminService.Object, null);

    //     var httpContext = new DefaultHttpContext
    //     {
    //         User = userClaim         
    //     };

    //     controller.ControllerContext = new ControllerContext
    //     {
    //         HttpContext = httpContext
    //     };

    //     var patientListDto = new List<PatientInfoDto>
    //     {
    //         new PatientInfoDto()     
    //     };

    //     _mockPatientService.Setup(t => t.GetPatientInfo())
    //     .ReturnsAsync(patientListDto);

    //     // Act
    //     //var result = await controller.PatientInfo();

    //     //Assert

    //     var view  = Assert.IsType<ViewResult>(result);
    //     var model = Assert.IsType<List<PatientInfoDto>>(view.Model);

    //     var count = model.Count();
    //     Assert.Same(patientListDto, model);
    //     Assert.Equal(1 , count);

    //     //_mockPatientService.Verify(c => c.GetPatientInfo(), Times.Once);
        
    // }

    [Fact]
    public async Task VerifyDoctor_IsUserDoctor_Verified()
    {
        var userId = Guid.NewGuid().ToString();

        var userClaim = new ClaimsPrincipal(new ClaimsIdentity(new []
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

        var controller = new AdminController(_mockUserManager.Object, _loggerMock.Object, _mockPatientService.Object, 
        _mockDoctorService.Object, _mockAdminService.Object, null);

        var httpContext = new DefaultHttpContext
        {
            User = userClaim
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        _mockDoctorService.Setup(c => c.VerifyDoctorAsync(userId))
        .ReturnsAsync(ServiceResult.Ok("Doctor has been verified succesfully"));

        //Act
        var result = await controller.VerifyDoctor(userId);

        //Assert

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("PendingDoctors", redirect.ActionName);
        Assert.Equal("Doctor has been verified succesfully", controller.TempData["success"]);

        _mockDoctorService.Verify(c => c.VerifyDoctorAsync(userId), Times.Once);
    }

    [Fact]
    public async Task VerifyDoctor_InvalidId_ReturnNotFound()
    {
        var userId = Guid.NewGuid().ToString();

        var userClaim = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

        var controller = new AdminController(_mockUserManager.Object, _loggerMock.Object, _mockPatientService.Object, 
        _mockDoctorService.Object, _mockAdminService.Object, null);

        var httpContext = new DefaultHttpContext
        {
            User = userClaim
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        _mockDoctorService.Setup(t => t.VerifyDoctorAsync("user12310"))
        .ReturnsAsync(ServiceResult.Missing("Doctor cannot be found"));

        //Act
        var result = await controller.VerifyDoctor("user12310");

        //Assert
        Assert.IsType<NotFoundResult>(result); 

        _mockDoctorService.Verify(t => t.VerifyDoctorAsync("user12310"), Times.Never);
        
    }
    [Fact]
    public async Task VerifyDoctor_NotLoggedIn_ReturnUnauthorized()
    {
        var userclaims = new ClaimsPrincipal(new ClaimsIdentity());

        var controller = new AdminController(_mockUserManager.Object, _loggerMock.Object, _mockPatientService.Object, 
        _mockDoctorService.Object, _mockAdminService.Object, null);

        var httpContext = new DefaultHttpContext
        {
            User = userclaims  
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        //Act
        var result = await controller.VerifyDoctor("user123");

        //Assert
        Assert.IsType<UnauthorizedResult>(result);

        _mockDoctorService.Verify(t => t.VerifyDoctorAsync("user123"), Times.Never);
    }
}
using AutoMapper;
using MediApp.Models;
using Microsoft.AspNetCore.Identity;
using Moq;
using MediApp.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediApp.Configuration;
using MediApp.DTOs;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MediApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MediApp.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MediApp.MediApp.Tests;

public class AuthContollerTests
{
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<IOptions<MedicationClientSettings>> _mockClientSettings;

    public AuthContollerTests()
    {
        var userStore = new Mock<UserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null,
        null,null,null,null,null,null,null);


        var httpContextAccesor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(_mockUserManager.Object, httpContextAccesor.Object, 
        claimsFactory.Object, null, null, null, null);

        _mockRoleManager = new Mock<RoleManager<IdentityRole>>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CreateUserDto, ApplicationUser>();
        }, null);

        _mapper = config.CreateMapper();

        _mockLogger = new Mock<ILogger<AuthController>>();
        _mockConfig = new Mock<IConfiguration>();
        _mockClientSettings = new Mock<IOptions<MedicationClientSettings>>();
    }

    [Fact]
    public async Task Login_Valid_UserLogsIn()
    {

        var dto = new LoginDto
        {
            Email = "test@hotmail.com",
            Password = Guid.NewGuid().ToString(),
            RememberMe = false
        };

        _mockSignInManager.Setup(c => c.PasswordSignInAsync(dto.Email, dto.Password, dto.RememberMe, true))
        .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var controller = new AuthController(_mockSignInManager.Object, _mockUserManager.Object, 
        _mockRoleManager.Object, _mapper, _mockLogger.Object, _mockClientSettings.Object,_mockConfig.Object);

        //Act
        var result  = await controller.Login(dto);

        //Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);

        _mockSignInManager.Verify(t => t.PasswordSignInAsync(dto.Email, dto.Password, dto.RememberMe, true),Times.Once);
        
    }

    [Fact]
    public async Task Login_WhenUsersLockedOut_ReturnsViewWithError()
    {
        var dto = new LoginDto
        {
            Email = "test@hotmail.com",
            Password = Guid.NewGuid().ToString(),
            RememberMe = false
        };

        var resu = _mockSignInManager.Setup(t => t.PasswordSignInAsync(dto.Email, dto.Password, dto.RememberMe, true))
        .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

        var controller = new AuthController(_mockSignInManager.Object, _mockUserManager.Object, 
        _mockRoleManager.Object, _mapper, _mockLogger.Object, _mockClientSettings.Object,_mockConfig.Object);

        var result = await controller.Login(dto);

        //Assert
        var view = Assert.IsType<ViewResult>(result);

        Assert.Same(dto,view.Model);
        Assert.False(controller.ModelState.IsValid);


        _mockSignInManager.Verify(t => t.PasswordSignInAsync(dto.Email, dto.Password, true, true), Times.Once);
    }

    [Fact]
    public async Task ChangePassword_UserValidated_PasswordUpdated()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = "aaron",
            LastName = "soggi",
            UserName = "test@hotmail.com",
            Email = "test@hotmail.com"
        };

        var dto = new ChangePasswordDto
        {
            CurrentPassword = "hello",
            NewPassword = "hello123",
            ConfirmPassord = "hello123"
        };

        var userClaim = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        }));

        _mockUserManager.Setup(t => t.GetUserAsync(userClaim)).ReturnsAsync(user);

        _mockUserManager.Setup(t => t.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword))
        .ReturnsAsync(IdentityResult.Success);

        var controller = new AuthController(_mockSignInManager.Object, _mockUserManager.Object, 
        _mockRoleManager.Object, _mapper, _mockLogger.Object, _mockClientSettings.Object,_mockConfig.Object);

        var httpcontext = new DefaultHttpContext
        {
            User = userClaim
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpcontext
        };

        var result = await controller.ChangePassword(dto);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);

        _mockUserManager.Verify(t => t.GetUserAsync(userClaim), Times.Once);
        _mockUserManager.Verify(t => t.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword), Times.Once);
    }

    [Fact]
    public async Task RegisterPatient_Valid_AccountCreated()
    {
        var dto = new CreateUserDto
        {
            FirstName = "Aaron",
            LastName = "Soggi",
            Email = "test@hotmail.com",
            Password = Guid.NewGuid().ToString(),
            Created = DateTime.UtcNow
        };

        _mockUserManager.Setup(t => t.FindByEmailAsync("test@hotmail.com"))
        .ReturnsAsync((ApplicationUser?)null);
        
        _mockUserManager.Setup(t => t.CreateAsync(It.Is<ApplicationUser>(t => 
        t.FirstName == dto.FirstName &&
        t.LastName == dto.LastName &&
        t.Email == dto.Email
        ), dto.Password))
        .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(t => t.AddToRoleAsync(It.Is<ApplicationUser>(t => t.Email == dto.Email), Roles.Patient))
        .ReturnsAsync(IdentityResult.Success);

        var controller = new AuthController(_mockSignInManager.Object, _mockUserManager.Object, 
        _mockRoleManager.Object, _mapper, _mockLogger.Object, _mockClientSettings.Object,_mockConfig.Object);

        var result = await controller.RegisterPatient(dto);

        //Assert
        var redirect  = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Login", redirect.ActionName);

        _mockUserManager.Verify(t => t.FindByEmailAsync("aaron@hotmail.com"),Times.Once);


        _mockUserManager.Verify(t => t.CreateAsync(It.Is<ApplicationUser>(t => 
        t.FirstName == dto.FirstName &&
        t.LastName == dto.LastName &&
        t.Email == dto.Email
        ), dto.Password), Times.Once);

        _mockUserManager.Verify(t => t.AddToRoleAsync(It.Is<ApplicationUser>(t => t.Email == dto.Email), Roles.Patient), Times.Once);

    }

    [Fact]
    public async Task RegisterPatient_UserRoleIsntAssigned_ReturnsModelStateError()
    {
        var dto = new CreateUserDto
        {
            FirstName = "Aaron",
            LastName = "Soggi",
            Email = "test@hotmail.com",
            Password = Guid.NewGuid().ToString(),
            Created = DateTime.UtcNow
        };

        _mockUserManager.Setup(t => t.FindByEmailAsync(dto.Email))
        .ReturnsAsync((ApplicationUser?)null);

        _mockUserManager.Setup(t => t.CreateAsync(It.Is<ApplicationUser>(t => t.FirstName == dto.FirstName
        && t.LastName == dto.LastName && t.Email == dto.Email), dto.Password))
        .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(t => t.AddToRoleAsync(It.Is<ApplicationUser>(t => t.Email == dto.Email), Roles.Patient))
        .ReturnsAsync(IdentityResult.Failed(new IdentityError
        {
            Description = "something went wrong"
        }));


        var controller = new AuthController(_mockSignInManager.Object, _mockUserManager.Object, 
        _mockRoleManager.Object, _mapper, _mockLogger.Object, _mockClientSettings.Object,_mockConfig.Object);

        var result = await controller.RegisterPatient(dto);

        //Assert
        var view = Assert.IsType<ViewResult>(result);

        Assert.Same(dto, view.Model);
        Assert.False(controller.ModelState.IsValid);

        _mockUserManager.Verify(t => t.FindByEmailAsync(dto.Email), Times.Once);
        _mockUserManager.Verify(t => t.CreateAsync(It.Is<ApplicationUser>(t => t.FirstName == dto.FirstName
        && t.LastName == dto.LastName && t.Email == dto.Email), dto.Password), Times.Once);

        _mockUserManager.Verify(t => t.AddToRoleAsync(It.Is<ApplicationUser>(t => t.Email == dto.Email), Roles.Patient), Times.Once);

    }
}
using MediApp.Models;
using MediApp.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MediApp.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Claims;
using MediApp.Identity;

namespace MediApp.MediApp.Tests;

public class AdminServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ILogger<DoctorService>> _mockLogger;

    public AdminServiceTests()
    {
        _mockLogger = new Mock<ILogger<DoctorService>>();

        var userStore = new Mock<IUserStore<ApplicationUser>>();

         _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore.Object);
    }

    [Fact]
    public async Task VerifyDoctor_IsValid_VerifiesDoctor()
    {
        var userId = Guid.NewGuid().ToString();
        var databaseName = Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: databaseName).Options;

        var context = new ApplicationDbContext(options);

        var user = new ApplicationUser
        {
            Id =  userId,
            UserName = "test@hotmail.com",
            Email = "test@hotmail.com",
            FirstName = "Joe",
            LastName = "burgess"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var claim = new Claim("IsVerified", "false");

         _mockUserManager.Setup(t => t.IsInRoleAsync(user, Roles.Doctor))
        .ReturnsAsync(true);;

        _mockUserManager.Setup(t => t.GetClaimsAsync(user))
        .ReturnsAsync(new List<Claim>()
        {
            claim
        });

        var newClaim = new Claim("IsVerified", "true");

        _mockUserManager.Setup(t => t.ReplaceClaimAsync(
            user,
            claim, 
            newClaim))
        .ReturnsAsync(IdentityResult.Success);

        var service = new DoctorService(_mockUserManager.Object, context, _mockLogger.Object);

        //Act
        var result = await service.VerifyDoctorAsync(userId);

        //Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Doctor has been verified succesfully", result.Message);

        _mockUserManager.Verify(t => t.IsInRoleAsync(user, Roles.Doctor), Times.Once);
        _mockUserManager.Verify(t => t.GetClaimsAsync(user), Times.Once);

        _mockUserManager.Verify(t => t.ReplaceClaimAsync(user, claim, It.Is<Claim>(
            t => t.Type == "IsVerified" && t.Value == "true")), Times.Once);

        _mockUserManager.Verify(t => 
        t.AddClaimAsync(It.IsAny<ApplicationUser>(), 
        It.IsAny<Claim>()), Times.Never);

    }

    [Fact]
    public async Task VerifyDoctor_NotADoctor_ReturnsFail()
    {
        
    }

}




   

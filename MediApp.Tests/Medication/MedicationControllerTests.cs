using MediApp.Controllers;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using MediApp.Models;
using Microsoft.EntityFrameworkCore;
using MediApp.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MediApp.DTOs;
using MediApp.Services;
using System.Security.Claims;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SQLitePCL;
using Castle.Components.DictionaryAdapter.Xml;
using System.Diagnostics.CodeAnalysis;
using System.Data.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;

namespace MediApp.MediApp.Tests;

// create tests for the medication/admin and auth controller
public class MedicationControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<MedicationController>> _loggerMock;
    private readonly Mock<IMedicationService> _medicationServiceMock;
    private readonly IMapper _mapper;
    private readonly MemoryCache _memoryCache;

    public MedicationControllerTests()
    {
        _loggerMock = new Mock<ILogger<MedicationController>>();

        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

        _medicationServiceMock = new Mock<IMedicationService>();

        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        var config = new MapperConfiguration(t =>
        {
            t.CreateMap<CreateMedicationDto, Models.Medication>();
        }, NullLoggerFactory.Instance);

        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task CreateMedicationAsync_Success_CreatesMedication()
    {

        //Arrange
        var dbName = Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: dbName).Options;

        var context = new ApplicationDbContext(options);

        var dto = new CreateMedicationDto
        {
            Name = "test",
            Dose = 200,
            Instructions = "take one a day",
            StartDate = DateTime.UtcNow,
            EndDate = new DateTime(2026, 04, DateTime.DaysInMonth(2026, 04))
        };

        _userManagerMock.Setup(t => t.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user123");

        _medicationServiceMock.Setup(i => i.CreateMedicationAsync(dto, "user123"))
        .ReturnsAsync(ServiceResult.Ok("Medication has succesfully been created"));

        var controller = new MedicationController(_userManagerMock.Object, _loggerMock.Object, context, _mapper, _medicationServiceMock.Object, _memoryCache);


        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        //Act
        var result = await controller.Create(dto);

        //Assert

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Medication has succesfully been created", controller.TempData["success"]);

        _medicationServiceMock.Verify(t => t.CreateMedicationAsync(dto, "user123"), Times.Once);
    }




    //getting medication via ID returns medication in view - update
    //Updating a medication when valid
    [Fact]
    public async Task Update_Valid_ReturnsMedication()
    {
        //Arrange

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

        var context  = new ApplicationDbContext(options);

        var dto = new UpdateMedicationDto
        {
            Id = 1,
            Name = "hello",
            Dose = 12,
            Instructions = "take one"
        };

        _userManagerMock.Setup(t => t.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user123");

        _medicationServiceMock.Setup(t => t.GetUpdateMedicationAsync("user123", dto.Id )).ReturnsAsync(dto);

        var controller = new MedicationController(_userManagerMock.Object, _loggerMock.Object, context, _mapper, _medicationServiceMock.Object, _memoryCache);

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()  
        };

        //Act
        var result = await controller.Update(dto.Id);

        //Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UpdateMedicationDto>(view.Model);

        Assert.Equal(1, model.Id);
        Assert.Equal("hello", model.Name);
        Assert.Equal(12, model.Dose);
        Assert.Equal("take one", model.Instructions);

        _medicationServiceMock.Verify(t => t.GetUpdateMedicationAsync("user123", model.Id), Times.Once);
        
    }

    [Fact]
    public async Task Update_WhenValid_UpdatesMedication()
    {
        //Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

        var context = new ApplicationDbContext(options);

        var dto = new UpdateMedicationDto
        {
            Id = 1,
            Name = "hello",
            Dose = 12,
            Instructions = "take one"
        };

        _userManagerMock.Setup(t => t.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user123");

        _medicationServiceMock.Setup(t => t.UpdateMedicationAsync(dto, "user123")).ReturnsAsync(ServiceResult.Ok("Medication has been updated succesfully"));

        var controller = new MedicationController(_userManagerMock.Object, _loggerMock.Object, context, _mapper, _medicationServiceMock.Object, _memoryCache);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        //Act
        var result = await controller.Update(dto); 

        //Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Medication has been updated succesfully", controller.TempData["success"]);

        _medicationServiceMock.Verify(y => y.UpdateMedicationAsync(dto, "user123"), Times.Once);
    }
}



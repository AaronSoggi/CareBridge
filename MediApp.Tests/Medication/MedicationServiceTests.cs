using AutoMapper;
using MediApp.Services;
using Moq;
using Microsoft.Extensions.Logging;
using MediApp.DTOs;
using Microsoft.EntityFrameworkCore;
using MediApp.Data;
using Humanizer;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using MediApp.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MediApp.MediApp.Tests.Medication;

public class MedicationServiceTests
{
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<MedicationService>> _loggerMock;
    private readonly MemoryCache _cache;
    private readonly Mock<ApplicationDbContext> _applicationDbContextMock;

    public MedicationServiceTests()
    {
        _loggerMock = new Mock<ILogger<MedicationService>>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UpdateMedicationDto, Models.Medication>();
            cfg.CreateMap<CreateMedicationDto, Models.Medication>();
            cfg.CreateMap<Models.Medication, MedicationDto>();
            
        }, null);

        _mapper = config.CreateMapper();

        _cache = new MemoryCache(new MemoryCacheOptions());

        _applicationDbContextMock = new Mock<ApplicationDbContext>();

    }

    [Fact]
    public async Task CreateMedicationAsync_WhenValid_CreatesMedication()
    {
        //Arrange

        //create an in memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: "testDb").Options;

        var context = new ApplicationDbContext(options);

        var dto = new CreateMedicationDto
        {
            Name = "test",
            Dose = 200,
            Instructions = "Take one a day"
        };

        // set up the service method
        var service = new MedicationService(context, _loggerMock.Object, _mapper, _cache);

        //Act

        var result = await service.CreateMedicationAsync(dto,"user123");
        
        //Assert

        Assert.True(result.Success);
        Assert.Equal("Medication has succesfully been created", result.Message);

        var data = await context.Medications.FirstOrDefaultAsync();
        Assert.Equal("test", data.Name);
        Assert.Equal("user123", data.PatientId.ToString());
        
    }

    [Fact]

    public async Task CreateMedicationAsync_Duplicate_ReturnFail()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: "test123").Options;

        await using var context = new ApplicationDbContext(options);

        var medication = new Models.Medication
        {
            Name = "test",
            Dose = 200,
            Instructions = "take one a day",
            PatientId = 123
        };

        var dto = new CreateMedicationDto
        {
            Name = "test",
            Dose = 200,
            Instructions = "take one a day"
        };

        context.Medications.Add(medication);
        await context.SaveChangesAsync();

        var service = new MedicationService(context, _loggerMock.Object, _mapper, _cache);

        var result = await service.CreateMedicationAsync(dto, "user123");

        //Act


        Assert.False(result.Success);
        Assert.NotNull(result);

        Assert.Equal("Error occured, this medication already exists!", result.Message);

    }

    [Fact]
    public async Task UpdateMedicationAsync_Valid_UpdatesMedication()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: "test123").Options;

        var context = new ApplicationDbContext(options);


        var existingMedication = new Models.Medication
        {
            Name = "Name",
            Dose = 300,
            Instructions = "Take two a day",
            PatientId = 123          
        };

        context.Medications.Add(existingMedication);
        await context.SaveChangesAsync();

        var dto = new UpdateMedicationDto
        {
            Name = "derek",
            Dose = 200,
            Instructions = "Take three a day"
        };

        var service = new MedicationService(context, _loggerMock.Object, _mapper, _cache);

        //Act
        var result = await service.UpdateMedicationAsync(dto, "user123");

        //Assert
        Assert.NotNull(result);
        Assert.True(result.Success);

        var updatedMedication = await context.Medications.FirstOrDefaultAsync();

        Assert.NotNull(updatedMedication);
        Assert.Equal("derek", updatedMedication.Name);
        Assert.Equal("Medication was updated succesfully", result.Message);
    }

    [Fact]
    public async Task UpdateMedicationAsync_Invalid_ReturnMissing()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var dto = new UpdateMedicationDto
        {
            Id = 1,
            Name = "name",
            Dose = 200,
            Instructions = "take one a day",
        };

        var service = new MedicationService(
            context,
            _loggerMock.Object,
            _mapper,
            _cache
        );

        // Act
        var result = await service.UpdateMedicationAsync(dto, "user123");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.True(result.NotFound);
        Assert.Equal("something went wrong when trying to Update the medication", result.Message);
    }

    [Fact]
    public async Task GetMedicationsAsync_Cached_ReturnCacheData()
    {

        //Arrange
        var cachedResult = new PagedResult<MedicationDto>
        {
            Items = new List<MedicationDto>
            {
                new MedicationDto
                {
                    Name = "test",
                    Dose = 200,
                    Notes = "Take one a day"
                },
                new MedicationDto
                {
                    Name = "test1",
                    Dose = 20,
                    Notes = "Take two a day"
                }
            },
            PageNumber = 2,
            PageSize = 10,
            TotalItemCount = 2
      
        };

        var cacheKey = "medications:user:1:page:2:size:10";

        _cache.Set(cacheKey, cachedResult);

        var service = new MedicationService(_applicationDbContextMock.Object, _loggerMock.Object, _mapper, _cache);

        //Act
        var result =  await service.GetMedicationsAsync("1", 2 ,10);

        //Assert
        Assert.NotNull(result.Items);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(2, result.TotalItemCount);
        Assert.Equal(10, result.PageSize);
      
    }

    [Fact]
    public async Task GetMedicationAsync_Pagination_ReturningCorrectPage()
    {
        var userId = Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

        var context = new ApplicationDbContext(options);

        var medication = new Models.Medication
        {
            Name = "test",
            Dose = 200,
            Instructions = "take one a day",
            //PatientId = userId
            
        };

        context.Add(medication);
        await context.SaveChangesAsync();

        var service = new MedicationService(context, _loggerMock.Object, _mapper, _cache);

        //Act
        var result = await service.GetMedicationsAsync(userId, 1, 10);

        //Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.TotalPageCount);
        Assert.Equal(1, result.TotalItemCount);
    }

    [Fact]
    public async Task GetMedicationsAsync_IsCached_ReturnCached()
    {
        //Arrange

        var userId = Guid.NewGuid().ToString();

        var cachedResult = new PagedResult<MedicationDto>()
        {
            Items = new List<MedicationDto>()
            {
                new MedicationDto
                {
                    Name = "test",
                    Dose = 20,
                    Notes = "take one a day"
                },
                new MedicationDto
                {
                    Name = "test",
                    Dose = 20,
                    Notes = "take one a day"
                },
                new MedicationDto
                {
                    Name = "test",
                    Dose = 20,
                    Notes = "take one a day"
                }
            },
            PageNumber = 1,
            PageSize = 10,
            TotalItemCount = 3
            
        };

        var cacheKey = $"medications:user:{userId}:page:1:size:10";

        _cache.Set(cacheKey,cachedResult);

        var service = new MedicationService(_applicationDbContextMock.Object, _loggerMock.Object, _mapper, _cache);

        //Act
        var result = await service.GetMedicationsAsync(userId, 1, 10);

        //Assert
        Assert.NotNull(result);

        Assert.Same(cachedResult, result);

        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(3, result.TotalItemCount);
        Assert.Equal(1, result.TotalPageCount);

    }
    
}
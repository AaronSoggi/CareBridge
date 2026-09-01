using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MediApp.Data;
using MediApp.Models;
using MediApp.Mapping;
using MediApp.Identity;
using MediApp.Services;
using MediApp.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediApp.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddHttpClient();

builder.Services.AddHttpClients(builder.Configuration);

builder.Services.AddDefaultIdentity<ApplicationUser>(
    options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
    }
)
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
   options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(5);
   options.Lockout.MaxFailedAccessAttempts = 4; 
   options.Lockout.AllowedForNewUsers = true;
   
});

builder.Services.ConfigureApplicationCookie(options =>
{
   options.SlidingExpiration = true;
   options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
});

builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IMedicationService, MedicationService>();
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

builder.Services.AddControllersWithViews();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<MedicationValidator>();

builder.Services.AddAuthorization(options =>
AuthorizationPolicies.GeneratePolicies(options));

builder.Services.AddMemoryCache();

var app = builder.Build();

using(var scope = app.Services.CreateAsyncScope())
{
   var service = scope.ServiceProvider;
   await DbInitializer.SeedData(service);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();

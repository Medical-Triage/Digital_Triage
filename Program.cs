using System.Security.Claims;
using DigitalTriageApp.Data;
using DigitalTriageApp.Helpers;
using DigitalTriageApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Database
builder.Services.AddDbContext<MedicalTriageDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("MedicalTriageDb")));

// DI for services
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IMedicalDataService, MedicalDataService>();
builder.Services.AddScoped<IPatientIssueService, PatientIssueService>();

// Authentication & Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax; // Changed to Lax for better compatibility
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
            ? CookieSecurePolicy.SameAsRequest 
            : CookieSecurePolicy.Always; // Use SameAsRequest in dev, Always in production
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DoctorOnly", policy => policy.RequireRole("Doctor"));
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

// Auth helper
builder.Services.AddScoped<IAuthHelper, AuthHelper>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<DigitalTriageApp.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();

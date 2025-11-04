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

// Add Razor Pages support for login
builder.Services.AddRazorPages();

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
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
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

// Configure Antiforgery with custom header
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN"; // Custom header name
    options.Cookie.Name = "__RequestVerificationToken";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.SameAsRequest 
        : CookieSecurePolicy.Always;
});

// Add HttpClient for API calls
builder.Services.AddHttpClient();
builder.Services.AddScoped<HttpClient>(serviceProvider =>
{
    var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    var client = factory.CreateClient();
    client.BaseAddress = new Uri(builder.Configuration["BaseUrl"] ?? "https://localhost:7266");
    return client;
});

// Auth helper
builder.Services.AddScoped<IAuthHelper, AuthHelper>();

// Antiforgery helper
builder.Services.AddScoped<AntiforgeryHelper>();

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

// Map Razor Pages (must be before Blazor components)
app.MapRazorPages();

// Map API Controllers (for antiforgery token endpoint)
app.MapControllers();

app.MapRazorComponents<DigitalTriageApp.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();

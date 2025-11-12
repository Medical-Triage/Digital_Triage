using DigitalTriage.Presentation.Common.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using DigitalTriage.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRazorPages();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DoctorOnly", policy => policy.RequireRole("Doctor"));
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "__RequestVerificationToken";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

builder.Services.AddHttpClient();
builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var client = factory.CreateClient();
    client.BaseAddress = new Uri(builder.Configuration["BaseUrl"] ?? "https://localhost:7266");
    return client;
});

builder.Services.AddScoped<IAuthHelper, AuthHelper>();
builder.Services.AddScoped<AntiforgeryHelper>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

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

app.MapRazorPages();
app.MapControllers();
app.MapRazorComponents<DigitalTriage.Presentation.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();

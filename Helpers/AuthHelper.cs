using System.Security.Claims;
using DigitalTriageApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DigitalTriageApp.Helpers
{
 /// <summary>
 /// Provides helper methods to sign users in and out using cookie authentication.
 /// </summary>
 public interface IAuthHelper
 {
 Task SignInAsync(HttpContext httpContext, Patient user);
 Task SignOutAsync(HttpContext httpContext);
 int? GetUserId(ClaimsPrincipal user);
 string? GetUserEmail(ClaimsPrincipal user);
 bool IsDoctor(ClaimsPrincipal user);
 }

 /// <summary>
 /// Implements cookie-based sign-in/out helpers and claims parsing.
 /// </summary>
 public class AuthHelper : IAuthHelper
 {
 public Task SignInAsync(HttpContext httpContext, Patient user)
 {
 // Build claims
 var claims = new List<Claim>
 {
 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
 new Claim(ClaimTypes.Email, user.Email),
 new Claim(ClaimTypes.Role, string.IsNullOrWhiteSpace(user.Role) ? "Patient" : user.Role!)
 };
 var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
 var principal = new ClaimsPrincipal(identity);

 var props = new AuthenticationProperties
 {
 IsPersistent = true,
 ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
 };

 return httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
 }

 public Task SignOutAsync(HttpContext httpContext)
 {
 return httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
 }

 public int? GetUserId(ClaimsPrincipal user)
 {
 var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
 if (int.TryParse(id, out var pid)) return pid;
 return null;
 }

 public string? GetUserEmail(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Email);

 public bool IsDoctor(ClaimsPrincipal user) => user.IsInRole("Doctor");
 }
}

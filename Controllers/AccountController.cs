using DigitalTriageApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTriageApp.Controllers
{
    /// <summary>
    /// API controller for account management operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IPatientService patientService, ILogger<AccountController> logger)
        {
            _patientService = patientService;
            _logger = logger;
        }

        /// <summary>
        /// Deletes the currently authenticated user's account
        /// </summary>
        [HttpDelete("delete-current")]
        public async Task<IActionResult> DeleteCurrentUser()
        {
            try
            {
                // Get the current user's ID from claims
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning("Delete account request failed: No valid user ID found in claims");
                    return BadRequest(new { error = "No logged-in user found." });
                }

                _logger.LogInformation("Delete account requested for user ID: {UserId}", userId);

                // Delete the user account
                await _patientService.DeleteAsync(userId);

                _logger.LogInformation("Account deleted successfully for user ID: {UserId}", userId);

                return Ok(new { message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user account");
                return StatusCode(500, new { error = "Failed to delete user.", details = ex.Message });
            }
        }
    }
}


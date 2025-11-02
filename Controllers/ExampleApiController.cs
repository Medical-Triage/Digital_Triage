using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTriageApp.Controllers
{
    /// <summary>
    /// Example API controller showing how to validate antiforgery tokens
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExampleApiController : ControllerBase
    {
        private readonly IAntiforgery _antiforgery;

        public ExampleApiController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        /// <summary>
        /// Example POST endpoint that validates antiforgery token
        /// </summary>
        [HttpPost("update")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateData([FromBody] UpdateRequest request)
        {
            // Validate antiforgery token is done automatically by [ValidateAntiForgeryToken]
            // If token is invalid, this method won't be reached
            
            // Your business logic here
            return Ok(new { message = "Data updated successfully", data = request });
        }

        /// <summary>
        /// Example POST endpoint with manual token validation
        /// </summary>
        [HttpPost("update-manual")]
        public async Task<IActionResult> UpdateDataManual([FromBody] UpdateRequest request)
        {
            // Manually validate antiforgery token
            try
            {
                await _antiforgery.ValidateRequestAsync(HttpContext);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Invalid antiforgery token", details = ex.Message });
            }

            // Your business logic here
            return Ok(new { message = "Data updated successfully", data = request });
        }
    }

    public class UpdateRequest
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
    }
}


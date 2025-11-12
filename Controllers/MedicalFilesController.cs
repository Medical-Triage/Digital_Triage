using System.IO;
using System.Security.Claims;
using DigitalTriageApp.Data;
using DigitalTriageApp.Models;
using DigitalTriageApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace DigitalTriageApp.Controllers;

/// <summary>
/// Handles upload, download, and deletion of medical file attachments.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MedicalFilesController : ControllerBase
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    private readonly MedicalTriageDbContext _db;
    private readonly IMedicalDataService _medicalDataService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<MedicalFilesController> _logger;

    public MedicalFilesController(
        MedicalTriageDbContext db,
        IMedicalDataService medicalDataService,
        IWebHostEnvironment environment,
        ILogger<MedicalFilesController> logger)
    {
        _db = db;
        _medicalDataService = medicalDataService;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Uploads a new file and associates it with the specified medical record.
    /// </summary>
    [HttpPost("{medicalDataId:int}/upload")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxFileSizeBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSizeBytes)]
    public async Task<IActionResult> UploadAsync([FromRoute] int medicalDataId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "File is required." });
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest(new { error = $"File exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB." });
        }

        var medicalData = await GetUserMedicalDataAsync(medicalDataId);
        if (medicalData == null)
        {
            return Forbid();
        }

        var uploadsRoot = GetUploadsRoot();
        var medicalFolder = Path.Combine(uploadsRoot, medicalDataId.ToString());
        Directory.CreateDirectory(medicalFolder);

        var safeFileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            safeFileName = $"attachment_{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        var uniqueFileName = $"{Guid.NewGuid():N}_{safeFileName}";
        var physicalPath = Path.Combine(medicalFolder, uniqueFileName);

        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = Path.Combine("uploads", medicalDataId.ToString(), uniqueFileName)
            .Replace('\\', '/');
        var savedFile = await _medicalDataService.AddFileAsync(medicalDataId, safeFileName, relativePath);

        _logger.LogInformation("User {UserId} uploaded file {FileName} for medical record {MedicalDataId}", GetCurrentUserId(), safeFileName, medicalDataId);
        return Ok(new
        {
            savedFile.Id,
            savedFile.FileName,
            savedFile.FilePath,
            savedFile.UploadDate
        });
    }

    /// <summary>
    /// Deletes an attachment owned by the current user.
    /// </summary>
    [HttpDelete("{fileId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAsync([FromRoute] int fileId)
    {
        var file = await _medicalDataService.GetFileAsync(fileId);
        if (file?.MedicalData == null)
        {
            return NotFound();
        }

        if (!CanCurrentUserAccess(file.MedicalData))
        {
            return Forbid();
        }

        var physicalPath = GetPhysicalPath(file.FilePath);
        if (!string.IsNullOrWhiteSpace(physicalPath) && System.IO.File.Exists(physicalPath))
        {
            System.IO.File.Delete(physicalPath);
        }

        await _medicalDataService.RemoveFileAsync(fileId);
        _logger.LogInformation("User {UserId} deleted file {FileId} from medical record {MedicalDataId}", GetCurrentUserId(), fileId, file.MedicalDataId);
        return NoContent();
    }

    /// <summary>
    /// Streams an attachment to the browser if the user is authorized to view it.
    /// </summary>
    [HttpGet("download/{fileId:int}")]
    public async Task<IActionResult> DownloadAsync([FromRoute] int fileId)
    {
        var file = await _medicalDataService.GetFileAsync(fileId);
        if (file?.MedicalData == null)
        {
            return NotFound();
        }

        if (!CanCurrentUserAccess(file.MedicalData))
        {
            return Forbid();
        }

        var physicalPath = GetPhysicalPath(file.FilePath);
        if (string.IsNullOrWhiteSpace(physicalPath) || !System.IO.File.Exists(physicalPath))
        {
            return NotFound();
        }

        if (!ContentTypeProvider.TryGetContentType(file.FileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
        return File(fileBytes, contentType, file.FileName);
    }

    private async Task<MedicalData?> GetUserMedicalDataAsync(int medicalDataId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return null;
        }

        return await _db.MedicalDatas
            .AsNoTracking()
            .FirstOrDefaultAsync(md => md.Id == medicalDataId && md.PatientId == userId);
    }

    private bool CanCurrentUserAccess(MedicalData medicalData)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return false;
        }

        if (medicalData.PatientId == userId.Value)
        {
            return true;
        }

        if (!User.IsInRole("Doctor") || medicalData.AuthorizedDoctorId == null)
        {
            return false;
        }

        var doctorProfileId = _db.DoctorProfiles
            .AsNoTracking()
            .Where(dp => dp.UserId == userId.Value)
            .Select(dp => (int?)dp.Id)
            .FirstOrDefault();

        return doctorProfileId.HasValue && doctorProfileId.Value == medicalData.AuthorizedDoctorId;
    }

    private int? GetCurrentUserId()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idValue, out var id) ? id : null;
    }

    private string GetUploadsRoot()
    {
        var root = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(root))
        {
            root = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        Directory.CreateDirectory(root);
        var uploads = Path.Combine(root, "uploads");
        Directory.CreateDirectory(uploads);
        return uploads;
    }

    private string? GetPhysicalPath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(webRoot, normalized);
        return Path.GetFullPath(fullPath);
    }
}


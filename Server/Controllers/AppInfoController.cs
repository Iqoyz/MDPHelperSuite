using fyp_server.Models;
using fyp_server.Services;
using Microsoft.AspNetCore.Mvc;

namespace fyp_server.Controllers;

[ApiController]
[Route("api/appinfo")]
public class AppInfoController : ControllerBase
{
    private readonly AppInfoService _appInfoService;

    public AppInfoController(AppInfoService appInfoService)
    {
        _appInfoService = appInfoService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string version, [FromForm] string author)
    {
        try
        {
            var downloadUrl = await _appInfoService.UploadFileAsync(file, version, author, Request);

            return Ok(new { DownloadUrl = downloadUrl });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Get all AppInfo entries
    [HttpGet]
    public async Task<ActionResult<List<AppInfo>>> GetAll()
    {
        var appInfoList = await _appInfoService.GetAsync();
        return Ok(appInfoList);
    }

    // Get a specific AppInfo entry by version
    [HttpGet("version/{version}")]
    public async Task<ActionResult<AppInfo>> GetByVersion(string version)
    {
        var appInfo = await _appInfoService.GetByVersionAsync(version);
        if (appInfo == null) return NotFound($"No app information found for version {version}.");
        return Ok(appInfo);
    }

    [HttpGet("latest")]
    public async Task<ActionResult<AppInfo>> GetLatest([FromQuery] string? fileType, [FromQuery] string? fileName)
    {
        if (!string.IsNullOrEmpty(fileName))
        {
            var latestByFileNameAndType = await _appInfoService.GetLatestByFileNameAndTypeAsync(fileName, fileType);
            if (latestByFileNameAndType == null) 
                return NotFound($"No app information found for file name '{fileName}' and file type '{fileType}'.");
        
            return Ok(latestByFileNameAndType);
        }

        if (!string.IsNullOrEmpty(fileType))
        {
            var latestByFileType = await _appInfoService.GetLatestByFileTypeAsync(fileType);
            if (latestByFileType == null)
                return NotFound($"No app information found for file type '{fileType}'.");
        
            return Ok(latestByFileType);
        }

        return BadRequest("Either fileType or fileName must be specified.");
    }



    // Delete an AppInfo entry by version
    [HttpDelete("/filetype/{fileType}version/{version}")]
    public async Task<IActionResult> DeleteByVersion(string version, string fileType)
    {
        var existingAppInfo = await _appInfoService.GetByVersionAndFileTypeAsync(version, fileType);
        if (existingAppInfo == null)
            return NotFound($"No app information found for version {version} and file type {fileType}.");

        await _appInfoService.RemoveByVersionAndFileTypeAsync(version, fileType);
        return NoContent();
    }

    // Delete all AppInfo entries
    [HttpDelete("deleteAll")]
    public async Task<IActionResult> DeleteAll()
    {
        await _appInfoService.RemoveAllAsync();
        return NoContent();
    }
}
using fyp_server.Models;
using fyp_server.Services;
using Microsoft.AspNetCore.Mvc;

namespace fyp_server.Controllers;

[ApiController]
[Route("api/firmware")]
public class FirmwareController : ControllerBase
{
    private readonly FirmwareService _firmwareService;

    public FirmwareController(FirmwareService firmwareService)
    {
        _firmwareService = firmwareService;
    }

    // Get all firmwares
    [HttpGet]
    public async Task<ActionResult<List<Firmware>>> GetAll()
    {
        var firmwares = await _firmwareService.GetAsync();
        return Ok(firmwares);
    }

    // Create a new firmware
    [HttpPost]
    public async Task<ActionResult<Firmware>> Create(Firmware firmware)
    {
        await _firmwareService.CreateAsync(firmware);
        return CreatedAtAction(nameof(GetByName), new { firmwareName = firmware.FirmwareName }, firmware);
    }

    // Get a specific firmware by Name
    [HttpGet("name/{firmwareName}")]
    public async Task<ActionResult<Firmware>> GetByName(string firmwareName)
    {
        var firmware = await _firmwareService.GetByNameAsync(firmwareName);
        if (firmware == null) return NotFound();
        return Ok(firmware);
    }

    // Delete a firmware by name
    [HttpDelete("name/{firmwareName}")]
    public async Task<IActionResult> DeleteByName(string firmwareName)
    {
        var existingFirmware = await _firmwareService.GetByNameAsync(firmwareName);
        if (existingFirmware == null) return NotFound();

        await _firmwareService.RemoveByNameAsync(firmwareName);
        return NoContent();
    }

    // Delete all firmwares
    [HttpDelete("deleteAll")]
    public async Task<IActionResult> DeleteAllFirmwares()
    {
        await _firmwareService.RemoveAllAsync();
        return NoContent();
    }
}
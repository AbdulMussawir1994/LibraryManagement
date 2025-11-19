using LibraryManagementSystem.Entities.DTOs;
using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers;

[ApiController]
[Authorize]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class LibraryController : ControllerBase
{
    private readonly ILibraryService _service;

    public LibraryController(ILibraryService service)
    {
        _service = service;
    }

    [HttpGet]
    [Route("GetAllLibraries")]
    public async Task<ActionResult> GetAllLibraries(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllLibrariesAsync(cancellationToken).ConfigureAwait(false);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpGet("GetLibraryById/{id}")]
    public async Task<ActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(MobileResponse<LibraryDto>.Fail("Invalid request.", "Error-400"));

        var result = await _service.GetLibraryByIdAsync(id, cancellationToken).ConfigureAwait(false);

        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Route("CreateLibrary")]
    public async Task<ActionResult> CreateLibrary([FromBody] CreateLibraryDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(MobileResponse<LibraryDto>.Fail("Invalid request.", "Error-400"));

        var result = await _service.CreateLibraryAsync(dto, cancellationToken).ConfigureAwait(false);
        return result.Status ? CreatedAtAction(nameof(CreateLibrary), result) : BadRequest(result);
    }

    [HttpPut("UpdateLibrary/{id}")]
    public async Task<ActionResult> UpdateLibrary(Guid id, [FromBody] UpdateLibraryDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id) return BadRequest(MobileResponse<LibraryDto>.Fail("Mismatched ID.", "ERROR-400"));

        if (!ModelState.IsValid)
            return BadRequest(MobileResponse<LibraryDto>.Fail("Invalid request.", "Error-400"));


        var result = await _service.UpdateLibraryAsync(dto, cancellationToken).ConfigureAwait(false);
        if (!result.Status)
            return NotFound(result);

        return Ok(result);
    }

    [HttpDelete("DeleteId/{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(MobileResponse<bool>.Fail("Invalid request.", "Error-400"));

        var result = await _service.DeleteLibraryAsync(id, cancellationToken).ConfigureAwait(false);
        if (!result.Status)
            return NotFound(result);

        return Ok(result);
    }
}

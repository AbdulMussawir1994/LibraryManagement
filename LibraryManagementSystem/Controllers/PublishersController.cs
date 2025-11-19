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
public class PublishersController : ControllerBase
{
    private readonly IPublisherService _service;

    public PublishersController(IPublisherService service)
    {
        _service = service;
    }

    [HttpGet("GetAllPublishers")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllPublishersAsync(ct);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpGet("GetPublisherById/{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(MobileResponse<PublisherDto>.Fail("Invalid request.", "Error-400"));

        var result = await _service.GetPublisherByIdAsync(id, ct);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpPost("CreatePublisher")]
    public async Task<IActionResult> Create([FromBody] CreatePublisherDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(MobileResponse<PublisherDto>.Fail("Invalid request.", "Error-400"));

        var result = await _service.CreatePublisherAsync(dto, ct);
        if (!result.Status) return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("UpdatePublisher/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePublisherDto dto, CancellationToken ct)
    {
        if (id != dto.Id)
            return BadRequest(MobileResponse<PublisherDto>.Fail("Mismatched ID.", "ERROR-400"));

        if (!ModelState.IsValid)
            return BadRequest(MobileResponse<PublisherDto>.Fail("Invalid request.", "Error-400"));

        var result = await _service.UpdatePublisherAsync(dto, ct);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpDelete("DeletePublisher/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {

        if (!ModelState.IsValid)
            return BadRequest(MobileResponse<bool>.Fail("Invalid request.", "Error-400"));

        var result = await _service.DeletePublisherAsync(id, ct);
        return result.Status ? Ok(result) : NotFound(result);
    }
}

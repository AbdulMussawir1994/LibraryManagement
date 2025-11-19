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
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _service;

    public AuthorsController(IAuthorService service)
    {
        _service = service;
    }

    [HttpGet("GetAllAuthors")]
    public async Task<ActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAuthorsAsync(ct);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpGet("GetAuthorById/{id:int}")]
    public async Task<ActionResult> GetById(int id, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(MobileResponse<AuthorDto>.Fail("Invalid request.", "Error-400"));

        var result = await _service.GetAuthorByIdAsync(id, ct);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpPost("CreateAuthor")]
    public async Task<ActionResult> Create([FromBody] CreateAuthorDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(MobileResponse<AuthorDto>.Fail("Invalid request.", "Error-400"));

        var result = await _service.CreateAuthorAsync(dto, ct);
        if (!result.Status) return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("UpdateAuthor/{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateAuthorDto dto, CancellationToken ct)
    {
        if (id != dto.Id)
            return BadRequest(MobileResponse<AuthorDto>.Fail("Mismatched ID.", "ERROR-400"));

        if (!ModelState.IsValid)
            return BadRequest(MobileResponse<AuthorDto>.Fail("Invalid request.", "Error-400"));

        var result = await _service.UpdateAuthorAsync(dto, ct);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpDelete("DeleteAuthor/{id:int}")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(MobileResponse<bool>.Fail("Invalid request.", "Error-400"));

        var result = await _service.DeleteAuthorAsync(id, ct);
        return result.Status ? Ok(result) : NotFound(result);
    }
}

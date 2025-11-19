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
public class BooksController : ControllerBase
{
    private readonly IBookService _service;

    public BooksController(IBookService service)
    {
        _service = service;
    }

    [HttpGet("GetAllBooks")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllBooksAsync(ct);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpGet("GetBookById/{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetBookByIdAsync(id, ct);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpPost("CreateBook")]
    public async Task<IActionResult> Create([FromBody] CreateBookDto dto, CancellationToken ct)
    {
        var result = await _service.CreateBookAsync(dto, ct);
        if (!result.Status) return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("UpdateBook/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBookDto dto, CancellationToken ct)
    {
        if (id != dto.Id)
            return BadRequest(MobileResponse<BookDto>.Fail("Mismatched ID.", "ERROR-400"));

        var result = await _service.UpdateBookAsync(dto, ct);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpDelete("DeleteBook/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _service.DeleteBookAsync(id, ct);
        return result.Status ? Ok(result) : NotFound(result);
    }
}

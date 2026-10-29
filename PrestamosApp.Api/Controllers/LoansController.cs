using Microsoft.AspNetCore.Mvc;
using PrestamosApp.Application.Loans.DTOs;
using PrestamosApp.Application.Loans.Services;

namespace PrestamosApp.Api.Controllers;

[ApiController]
[Route("api/loans")]
public class LoansController : ControllerBase
{
    private readonly LoanService _service;

    public LoansController(LoanService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<LoanResponseDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LoanResponseDto>> GetById(int id)
    {
        try
        {
            var loan = await _service.GetByIdAsync(id);

            if (loan is null)
            {
                return NotFound(new
                {
                    message = $"No existe un préstamo con el ID {id}."
                });
            }

            return Ok(loan);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("client/{clientId:int}")]
    public async Task<ActionResult<List<LoanResponseDto>>> GetByClientId(
        int clientId)
    {
        try
        {
            var loans = await _service.GetByClientIdAsync(clientId);
            return Ok(loans);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<LoanResponseDto>> Create(
        [FromBody] CreateLoanDto dto)
    {
        try
        {
            var loan = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = loan.Id },
                loan);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}

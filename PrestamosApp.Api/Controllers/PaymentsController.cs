using Microsoft.AspNetCore.Mvc;
using PrestamosApp.Application.Payments.DTOs;
using PrestamosApp.Application.Payments.Services;

namespace PrestamosApp.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentsController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("loan/{loanId:int}")]
    public async Task<IActionResult> GetByLoanId(int loanId)
    {
        try
        {
            var payments = await _paymentService.GetByLoanIdAsync(loanId);
            return Ok(payments);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePaymentDto dto)
    {
        try
        {
            var payment = await _paymentService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetByLoanId),
                new { loanId = payment.LoanId },
                payment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}

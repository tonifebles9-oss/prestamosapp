using Microsoft.AspNetCore.Mvc;
using PrestamosApp.Application.Installments.Repositories;

namespace PrestamosApp.Api.Controllers;

[ApiController]
[Route("api/installments")]
public class InstallmentsController : ControllerBase
{
    private readonly IInstallmentRepository _repository;

    public InstallmentsController(IInstallmentRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("loan/{loanId:int}")]
    public async Task<IActionResult> GetByLoanId(int loanId)
    {
        if (loanId <= 0)
        {
            return BadRequest(new
            {
                message = "El ID del préstamo debe ser mayor que cero."
            });
        }

        var installments = await _repository.GetByLoanIdAsync(loanId);

        return Ok(installments.Select(installment => new
        {
            installment.Id,
            installment.LoanId,
            installment.InstallmentNumber,
            installment.DueDate,
            installment.AmountDue,
            installment.AmountPaid,
            installment.Status,
            installment.PaidAt
        }));
    }
}

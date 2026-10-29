using System.ComponentModel.DataAnnotations;

namespace PrestamosApp.Application.Loans.DTOs;

public class CreateLoanDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente válido.")]
    public int ClientId { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999",
        ErrorMessage = "El monto debe ser mayor que cero.")]
    public decimal Amount { get; set; }

    [Range(typeof(decimal), "0", "100",
        ErrorMessage = "La tasa de interés debe estar entre 0 y 100.")]
    public decimal InterestRate { get; set; }

    [Range(1, 600,
        ErrorMessage = "El plazo debe estar entre 1 y 600 meses.")]
    public int TermInMonths { get; set; }
}

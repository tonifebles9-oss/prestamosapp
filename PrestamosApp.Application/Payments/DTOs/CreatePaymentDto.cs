using System.ComponentModel.DataAnnotations;

namespace PrestamosApp.Application.Payments.DTOs;

public class CreatePaymentDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un préstamo válido.")]
    public int LoanId { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999",
        ErrorMessage = "El monto del pago debe ser mayor que cero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "El método de pago es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El método de pago no puede superar 50 caracteres.")]
    public string PaymentMethod { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "La observación no puede superar 500 caracteres.")]
    public string Notes { get; set; } = string.Empty;
}

namespace PrestamosApp.Application.Payments.DTOs;

public class PaymentResponseDto
{
    public int Id { get; set; }

    public int LoanId { get; set; }

    public int InstallmentNumber { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public decimal TotalPaid { get; set; }

    public decimal OutstandingBalance { get; set; }
}

namespace PrestamosApp.Domain.Entities;

public class Payment
{
    public int Id { get; set; }

    public int LoanId { get; set; }

    public Loan Loan { get; set; } = null!;

    public int InstallmentNumber { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public string PaymentMethod { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;
}

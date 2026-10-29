namespace PrestamosApp.Domain.Entities;

public class Installment
{
    public int Id { get; set; }

    public int LoanId { get; set; }

    public Loan Loan { get; set; } = null!;

    public int InstallmentNumber { get; set; }

    public DateTime DueDate { get; set; }

    public decimal AmountDue { get; set; }

    public decimal AmountPaid { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime? PaidAt { get; set; }
}

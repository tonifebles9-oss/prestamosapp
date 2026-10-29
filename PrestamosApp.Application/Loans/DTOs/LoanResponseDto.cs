namespace PrestamosApp.Application.Loans.DTOs;

public class LoanResponseDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermInMonths { get; set; }
    public DateTime StartDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal MonthlyPayment { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal TotalInterest { get; set; }

    public decimal TotalPaid { get; set; }
    public decimal OutstandingBalance { get; set; }
}

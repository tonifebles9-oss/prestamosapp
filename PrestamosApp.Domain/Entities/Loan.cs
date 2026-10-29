namespace PrestamosApp.Domain.Entities;

public class Loan
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public decimal InterestRate { get; set; }

    public int TermInMonths { get; set; }

    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Pending";

    public int ClientId { get; set; }

    public Client Client { get; set; } = null!;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public ICollection<Installment> Installments { get; set; }
        = new List<Installment>();
}

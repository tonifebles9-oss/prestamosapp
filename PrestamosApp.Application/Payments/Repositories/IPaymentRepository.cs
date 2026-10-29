using PrestamosApp.Domain.Entities;

namespace PrestamosApp.Application.Payments.Repositories;

public interface IPaymentRepository
{
    Task<List<Payment>> GetByLoanIdAsync(int loanId);

    Task<Payment> CreateAsync(Payment payment);
}

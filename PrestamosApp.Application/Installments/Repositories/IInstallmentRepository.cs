using PrestamosApp.Domain.Entities;

namespace PrestamosApp.Application.Installments.Repositories;

public interface IInstallmentRepository
{
    Task CreateRangeAsync(List<Installment> installments);

    Task<List<Installment>> GetByLoanIdAsync(int loanId);

    Task UpdateRangeAsync(List<Installment> installments);
}

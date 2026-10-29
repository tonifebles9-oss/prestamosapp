using PrestamosApp.Domain.Entities;

namespace PrestamosApp.Application.Loans.Repositories;

public interface ILoanRepository
{
    Task<List<Loan>> GetAllAsync();

    Task<Loan?> GetByIdAsync(int id);

    Task<List<Loan>> GetByClientIdAsync(int clientId);

    Task<Loan> CreateAsync(Loan loan);

    Task UpdateStatusAsync(int loanId, string status);
}

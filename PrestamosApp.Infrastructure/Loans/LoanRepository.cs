using Microsoft.EntityFrameworkCore;
using PrestamosApp.Application.Loans.Repositories;
using PrestamosApp.Domain.Entities;
using PrestamosApp.Infrastructure.Data;

namespace PrestamosApp.Infrastructure.Loans;

public class LoanRepository : ILoanRepository
{
    private readonly AppDbContext _context;

    public LoanRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Loan>> GetAllAsync()
    {
        return await _context.Loans
            .AsNoTracking()
            .Include(loan => loan.Client)
            .OrderByDescending(loan => loan.StartDate)
            .ToListAsync();
    }

    public async Task<Loan?> GetByIdAsync(int id)
    {
        return await _context.Loans
            .AsNoTracking()
            .Include(loan => loan.Client)
            .FirstOrDefaultAsync(loan => loan.Id == id);
    }

    public async Task<List<Loan>> GetByClientIdAsync(int clientId)
    {
        return await _context.Loans
            .AsNoTracking()
            .Include(loan => loan.Client)
            .Where(loan => loan.ClientId == clientId)
            .OrderByDescending(loan => loan.StartDate)
            .ToListAsync();
    }

    public async Task<Loan> CreateAsync(Loan loan)
    {
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        return await _context.Loans
            .AsNoTracking()
            .Include(item => item.Client)
            .FirstAsync(item => item.Id == loan.Id);
    }

    public async Task UpdateStatusAsync(int loanId, string status)
    {
        var loan = await _context.Loans
            .FirstOrDefaultAsync(item => item.Id == loanId);

        if (loan is null)
        {
            throw new InvalidOperationException(
                "No existe el préstamo que se intenta actualizar.");
        }

        loan.Status = status;

        await _context.SaveChangesAsync();
    }
}

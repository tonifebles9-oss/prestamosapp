using Microsoft.EntityFrameworkCore;
using PrestamosApp.Application.Installments.Repositories;
using PrestamosApp.Domain.Entities;
using PrestamosApp.Infrastructure.Data;

namespace PrestamosApp.Infrastructure.Installments;

public class InstallmentRepository : IInstallmentRepository
{
    private readonly AppDbContext _context;

    public InstallmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateRangeAsync(List<Installment> installments)
    {
        if (installments.Count == 0)
        {
            return;
        }

        await _context.Installments.AddRangeAsync(installments);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Installment>> GetByLoanIdAsync(int loanId)
    {
        return await _context.Installments
            .Where(installment => installment.LoanId == loanId)
            .OrderBy(installment => installment.InstallmentNumber)
            .ToListAsync();
    }

    public async Task UpdateRangeAsync(List<Installment> installments)
    {
        if (installments.Count == 0)
        {
            return;
        }

        _context.Installments.UpdateRange(installments);
        await _context.SaveChangesAsync();
    }
}

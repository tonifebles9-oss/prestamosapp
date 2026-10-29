using Microsoft.EntityFrameworkCore;
using PrestamosApp.Application.Payments.Repositories;
using PrestamosApp.Domain.Entities;
using PrestamosApp.Infrastructure.Data;

namespace PrestamosApp.Infrastructure.Payments;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Payment>> GetByLoanIdAsync(int loanId)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(payment => payment.LoanId == loanId)
            .OrderBy(payment => payment.InstallmentNumber)
            .ToListAsync();
    }

    public async Task<Payment> CreateAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return payment;
    }
}

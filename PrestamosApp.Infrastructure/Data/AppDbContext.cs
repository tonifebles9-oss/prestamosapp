using Microsoft.EntityFrameworkCore;
using PrestamosApp.Domain.Entities;

namespace PrestamosApp.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Loan> Loans => Set<Loan>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Installment> Installments => Set<Installment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Client>()
            .HasIndex(c => c.IdentificationNumber)
            .IsUnique();

        modelBuilder.Entity<Loan>()
            .Property(l => l.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Loan>()
            .Property(l => l.InterestRate)
            .HasPrecision(5, 2);

        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Client)
            .WithMany(c => c.Loans)
            .HasForeignKey(l => l.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Loan)
            .WithMany(l => l.Payments)
            .HasForeignKey(p => p.LoanId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Installment>()
            .Property(i => i.AmountDue)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Installment>()
            .Property(i => i.AmountPaid)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Installment>()
            .HasIndex(i => new { i.LoanId, i.InstallmentNumber })
            .IsUnique();

        modelBuilder.Entity<Installment>()
            .HasOne(i => i.Loan)
            .WithMany(l => l.Installments)
            .HasForeignKey(i => i.LoanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore.Storage;
using PrestamosApp.Application.Common.Interfaces;

namespace PrestamosApp.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction is not null)
        {
            throw new InvalidOperationException(
                "Ya existe una transacción activa.");
        }

        _transaction =
            await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException(
                "No existe una transacción activa para confirmar.");
        }

        try
        {
            await _transaction.CommitAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction is null)
        {
            return;
        }

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}

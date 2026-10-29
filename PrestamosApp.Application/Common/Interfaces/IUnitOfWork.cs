namespace PrestamosApp.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task BeginTransactionAsync();

    Task CommitTransactionAsync();

    Task RollbackTransactionAsync();
}

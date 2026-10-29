using Microsoft.EntityFrameworkCore;
using PrestamosApp.Application.Clients.Repositories;
using PrestamosApp.Domain.Entities;
using PrestamosApp.Infrastructure.Data;

namespace PrestamosApp.Infrastructure.Clients;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _context;

    public ClientRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Client>> GetAllAsync()
    {
        return await _context.Clients
            .AsNoTracking()
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync();
    }

    public async Task<Client?> GetByIdAsync(int id)
    {
        return await _context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> ExistsByIdentificationAsync(
        string identificationNumber)
    {
        return await _context.Clients.AnyAsync(
            c => c.IdentificationNumber == identificationNumber);
    }

    public async Task<bool> ExistsByIdentificationForOtherClientAsync(
        string identificationNumber, int clientId)
    {
        return await _context.Clients.AnyAsync(
            c => c.IdentificationNumber == identificationNumber
                 && c.Id != clientId);
    }

    public async Task<Client> CreateAsync(Client client)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task<Client> UpdateAsync(Client client)
    {
        _context.Clients.Update(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task<bool> HasLoansAsync(int clientId)
    {
        return await _context.Loans.AnyAsync(
            loan => loan.ClientId == clientId);
    }

    public async Task DeleteAsync(Client client)
    {
        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
    }
}

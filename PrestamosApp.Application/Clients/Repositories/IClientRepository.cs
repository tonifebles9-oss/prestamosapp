using PrestamosApp.Domain.Entities;

namespace PrestamosApp.Application.Clients.Repositories;

public interface IClientRepository
{
    Task<List<Client>> GetAllAsync();

    Task<Client?> GetByIdAsync(int id);

    Task<bool> ExistsByIdentificationAsync(string identificationNumber);

    Task<bool> ExistsByIdentificationForOtherClientAsync(
        string identificationNumber, int clientId);

    Task<Client> CreateAsync(Client client);

    Task<Client> UpdateAsync(Client client);

    Task<bool> HasLoansAsync(int clientId);

    Task DeleteAsync(Client client);
}

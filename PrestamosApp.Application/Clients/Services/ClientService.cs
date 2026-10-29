using PrestamosApp.Application.Clients.DTOs;
using PrestamosApp.Application.Clients.Repositories;
using PrestamosApp.Domain.Entities;

namespace PrestamosApp.Application.Clients.Services;

public class ClientService
{
    private readonly IClientRepository _repository;

    public ClientService(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ClientResponseDto>> GetAllAsync()
    {
        var clients = await _repository.GetAllAsync();
        return clients.Select(ToResponse).ToList();
    }

    public async Task<ClientResponseDto?> GetByIdAsync(int id)
    {
        var client = await _repository.GetByIdAsync(id);
        return client is null ? null : ToResponse(client);
    }

    public async Task<ClientResponseDto> CreateAsync(CreateClientDto dto)
    {
        var identification = dto.IdentificationNumber.Trim();

        if (await _repository.ExistsByIdentificationAsync(identification))
        {
            throw new InvalidOperationException(
                "Ya existe un cliente con esa identificación.");
        }

        var client = new Client
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            IdentificationNumber = identification,
            Phone = dto.Phone.Trim(),
            Address = dto.Address.Trim()
        };

        client = await _repository.CreateAsync(client);
        return ToResponse(client);
    }

    public async Task<ClientResponseDto?> UpdateAsync(
        int id, UpdateClientDto dto)
    {
        var client = await _repository.GetByIdAsync(id);

        if (client is null)
        {
            return null;
        }

        var identification = dto.IdentificationNumber.Trim();

        if (await _repository.ExistsByIdentificationForOtherClientAsync(
            identification, id))
        {
            throw new InvalidOperationException(
                "Ya existe otro cliente con esa identificación.");
        }

        client.FirstName = dto.FirstName.Trim();
        client.LastName = dto.LastName.Trim();
        client.IdentificationNumber = identification;
        client.Phone = dto.Phone.Trim();
        client.Address = dto.Address.Trim();

        client = await _repository.UpdateAsync(client);
        return ToResponse(client);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var client = await _repository.GetByIdAsync(id);

        if (client is null)
        {
            return false;
        }

        if (await _repository.HasLoansAsync(id))
        {
            throw new InvalidOperationException(
                "No se puede eliminar el cliente porque tiene préstamos registrados.");
        }

        await _repository.DeleteAsync(client);
        return true;
    }

    private static ClientResponseDto ToResponse(Client client)
    {
        return new ClientResponseDto
        {
            Id = client.Id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            IdentificationNumber = client.IdentificationNumber,
            Phone = client.Phone,
            Address = client.Address,
            CreatedAt = client.CreatedAt
        };
    }
}

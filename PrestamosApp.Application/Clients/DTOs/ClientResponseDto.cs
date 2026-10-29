namespace PrestamosApp.Application.Clients.DTOs;

public class ClientResponseDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string IdentificationNumber { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

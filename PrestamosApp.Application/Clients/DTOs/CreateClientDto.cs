using System.ComponentModel.DataAnnotations;

namespace PrestamosApp.Application.Clients.DTOs;

public class CreateClientDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La identificación es obligatoria.")]
    [StringLength(30)]
    public string IdentificationNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(250)]
    public string Address { get; set; } = string.Empty;
}

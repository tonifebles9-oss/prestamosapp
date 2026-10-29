using Microsoft.AspNetCore.Mvc;
using PrestamosApp.Application.Clients.DTOs;
using PrestamosApp.Application.Clients.Services;

namespace PrestamosApp.Api.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly ClientService _service;

    public ClientsController(ClientService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClientResponseDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClientResponseDto>> GetById(int id)
    {
        var client = await _service.GetByIdAsync(id);

        if (client is null)
        {
            return NotFound(new
            {
                message = $"No existe un cliente con el ID {id}."
            });
        }

        return Ok(client);
    }

    [HttpPost]
    public async Task<ActionResult<ClientResponseDto>> Create(
        [FromBody] CreateClientDto dto)
    {
        try
        {
            var client = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = client.Id },
                client);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClientResponseDto>> Update(
        int id,
        [FromBody] UpdateClientDto dto)
    {
        try
        {
            var client = await _service.UpdateAsync(id, dto);

            if (client is null)
            {
                return NotFound(new
                {
                    message = $"No existe un cliente con el ID {id}."
                });
            }

            return Ok(client);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = $"No existe un cliente con el ID {id}."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}

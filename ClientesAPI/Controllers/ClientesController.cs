using Clientes.CORE.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace Clientes.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _service;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(IClienteService service, ILogger<ClientesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // GET /api/clientes/{id}
    [Authorize]
    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        _logger.LogInformation("Solicitud GET Cliente por id={Id}", id);

        var cliente = await _service.ObtenerClientePorIdAsync(id, ct);

        if (cliente is null)
        {
            _logger.LogWarning("Cliente no encontrado. id={Id}", id);
            return NotFound();
        }

        return Ok(cliente);
    }
}

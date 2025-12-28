using Clientes.CORE.DTOs;
using Clientes.CORE.Interfaces;

namespace Clientes.BLL.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repo;

    public ClienteService(IClienteRepository repo)
    {
        _repo = repo;
    }

    public async Task<ClienteDto?> ObtenerClientePorIdAsync(int id, CancellationToken ct)
    {
        if (id <= 0) return null;

        var cliente = await _repo.ObtenerPorIdAsync(id, ct);
        if (cliente is null) return null;

        return new ClienteDto
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            FechaCreacion = cliente.FechaCreacion
        };
    }
}

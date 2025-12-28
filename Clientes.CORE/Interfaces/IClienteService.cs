using Clientes.CORE.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clientes.CORE.Interfaces
{
    public interface IClienteService
    {
        Task<ClienteDto?> ObtenerClientePorIdAsync(int id, CancellationToken ct);
    }
}

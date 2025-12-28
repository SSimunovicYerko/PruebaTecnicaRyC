using Clientes.CORE.Entidades;
using Clientes.CORE.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clientes.DAL.Repositories
{
    public class InMemoryClienteRepository : IClienteRepository
    {
        private static readonly List<Cliente> Data = new()
    {
        new Cliente { Id = 1, Nombre = "Ana",  FechaCreacion = new DateTime(2024, 1, 10) },
        new Cliente { Id = 2, Nombre = "Luis", FechaCreacion = new DateTime(2023, 6, 5) },
        new Cliente { Id = 3, Nombre = "Sofia", FechaCreacion = new DateTime(2022, 9, 20) }
    };

        public Task<Cliente?> ObtenerPorIdAsync(int id, CancellationToken ct)
        {
            var cliente = Data.FirstOrDefault(x => x.Id == id);
            return Task.FromResult(cliente);
        }
    }
}

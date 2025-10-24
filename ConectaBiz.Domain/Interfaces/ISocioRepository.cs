using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface ISocioRepository
    {
        Task<List<Socio>> ListarTodosAsync();
        Task<Socio?> ObtenerPorIdAsync(int id);
        Task<Socio?> ObtenerPorNumDocAsync(string numDoc);
        Task<Socio> CrearAsync(Socio socio);
        Task<Socio> ActualizarAsync(Socio socio);
        Task<bool> EliminarAsync(int id);
        Task<bool> ExisteNumDocAsync(string numDoc);
        Task<bool> ExisteNumDocAsync(string numDoc, int idExcluir);
    }
}

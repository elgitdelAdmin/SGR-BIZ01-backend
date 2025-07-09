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
    }
}

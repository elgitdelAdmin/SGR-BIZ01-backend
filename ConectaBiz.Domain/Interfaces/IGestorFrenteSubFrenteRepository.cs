using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface IGestorFrenteSubFrenteRepository
    {
        Task<IEnumerable<GestorFrenteSubFrente>> GetByGestorIdAsync(int IdGestor);
        Task<GestorFrenteSubFrente> CreateAsync(GestorFrenteSubFrente gestorFrenteSubFrente);
        Task UpdateRangeAsync(IEnumerable<GestorFrenteSubFrente> gestorFrenteSubFrente);
        Task DeactivateByGestorIdAsync(int IdGestor);
    }
}

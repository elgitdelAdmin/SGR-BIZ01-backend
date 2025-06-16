using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface IConsultorFrenteSubFrenteRepository
    {
        Task<IEnumerable<ConsultorFrenteSubFrente>> GetByConsultorIdAsync(int consultorId);
        Task<ConsultorFrenteSubFrente> GetByIdAsync(int id);
        Task<ConsultorFrenteSubFrente> CreateAsync(ConsultorFrenteSubFrente consultorFrenteSubFrente);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByConsultorIdAsync(int consultorId);
        Task<bool> ExistsAsync(int consultorId, int frenteId, int subFrenteId);
    }
}

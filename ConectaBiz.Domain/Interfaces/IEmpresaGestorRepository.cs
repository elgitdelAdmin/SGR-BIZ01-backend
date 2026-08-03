using ConectaBiz.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Interfaces
{
    public interface IEmpresaGestorRepository
    {
        Task<IEnumerable<EmpresaGestor>> GetByEmpresaIdAsync(int idEmpresa);
        Task<IEnumerable<EmpresaGestor>> GetActiveByEmpresaIdAsync(int idEmpresa);
        Task<EmpresaGestor?> GetPrincipalByEmpresaIdAsync(int idEmpresa);
        Task<EmpresaGestor> AsociarGestorAsync(int idEmpresa, int idGestor, bool esPrincipal, string usuario);
        Task<bool> DesasociarGestorAsync(int idEmpresa, int idGestor, string usuario);
        Task<bool> EstablecerPrincipalAsync(int idEmpresa, int idGestor, string usuario);
    }
}

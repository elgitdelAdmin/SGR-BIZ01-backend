using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface IEmpresaService
    {
        Task<IEnumerable<EmpresaDto>> GetAllAsync();
        Task<IEnumerable<EmpresaDto>> GetByIdSocio(int idSocio);
        Task<IEnumerable<EmpresaDto>> GetAllActiveAsync();
        Task<EmpresaDto?> GetByIdAsync(int id);
        Task<PersonaConUsuariosEmpresaDto> GetPersonaResponsableByTipoNumDoc(int idTipoDocumento, string numeroDocumento);
        Task<EmpresaDto?> GetByCodigoAsync(string codigo);
        Task<EmpresaDto> GetByIdUserAsync(int iduser);
        Task<IEnumerable<EmpresaDto>> GetBySocioAsync(int idSocio);
        Task<IEnumerable<EmpresaDto>> GetByGestorAsync(int idGestor);
        Task<EmpresaDto> CreateAsync(CreateEmpresaDto createDto);
        Task<EmpresaDto> UpdateAsync(int id, UpdateEmpresaDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByNumDocYPaisAsync(string numDocContribuyente, int? idPais);

    }
}

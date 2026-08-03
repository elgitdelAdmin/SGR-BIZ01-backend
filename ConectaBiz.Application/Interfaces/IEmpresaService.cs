using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Services;
using ConectaBiz.Domain.Interfaces;
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
        Task<IEnumerable<EmpresaDto>> GetByIdUserIdRolAsync(int idUser, string codRol, int? idSocio = null);
        Task<IEnumerable<EmpresaDto>> GetAllActiveAsync();
        Task<EmpresaDto?> GetByIdAsync(int id);
        Task<PersonaConUsuariosEmpresaDto> GetPersonaResponsableByTipoNumDoc(int idTipoDocumento, string numeroDocumento);
        Task<EmpresaDto> GetByIdUserAsync(int iduser);
        Task<EmpresaDto> GetByNumDocContribuyenteAsync(string numDocContribuyente, string numDocSocio);
        Task<EmpresaDto> CreateAsync(CreateEmpresaDto createDto);
        Task<EmpresaDto> UpdateAsync(int id, UpdateEmpresaDto updateDto);
        Task<bool> DeleteAsync(int id);
    }
}

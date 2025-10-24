using ConectaBiz.Application.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketDto>> GetAllAsync();
        Task<TicketDto?> GetByIdAsync(int id);
        Task<TicketDto?> GetByCodTicketAsync(string codTicket);
        Task<IEnumerable<TicketDto>> GetByEmpresaAsync(int idEmpresa);
        Task<IEnumerable<TicketDto>> GetByEstadoAsync(int idEstado);
        Task<TicketDto?> GetByIdSocioNumContribuyenteEmpAsync(int idSocio, string numContribuyenteEmp);
        Task<TicketDto?> GetByNumContribuyenteSocioEmpAsync(string numContribuyenteSocio, string numContribuyenteEmp);
        Task<IEnumerable<TicketDto>> GetByIdUserIdRolAsync(int idUser, string codRol);
        Task<IEnumerable<TicketDto>> GetTicketsWithFiltersAsync(int? idEmpresa = null, int? idEstado = null, bool? urgente = null);
        Task<TicketDto> CreateAsync(TicketInsertDto insertDto);
        //Task<TicketZipFileDto> UploadZipFileAsync(int ticketId, IFormFile zipFile);
        Task<TicketDto> UpdateAsync(int id, TicketUpdateDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<TicketHistorialEstadoDto>> GetHistorialByTicketIdAsync(int idTicket);
        Task<TicketDto?> GetByCodReqSgrCstiAsync(string codReqSgrCsti);
        Task ActualizarEstadoDeAprobadoAEnEjecucion();
    }
}

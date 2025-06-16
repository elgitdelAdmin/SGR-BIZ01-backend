using ConectaBiz.Application.DTOs;
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
        Task<IEnumerable<TicketDto>> GetByGestorAsync(int idGestor);
        Task<IEnumerable<TicketDto>> GetTicketsWithFiltersAsync(int? idEmpresa = null, int? idEstado = null, bool? urgente = null);
        Task<TicketDto> CreateAsync(TicketInsertDto insertDto);
        Task<TicketDto> UpdateAsync(int id, TicketUpdateDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<TicketHistorialEstadoDto>> GetHistorialByTicketIdAsync(int idTicket);
    }
}

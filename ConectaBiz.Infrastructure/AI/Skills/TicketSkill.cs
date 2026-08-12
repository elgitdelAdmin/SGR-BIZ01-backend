using ConectaBiz.Application.Interfaces;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Infrastructure.AI.Skills
{
    public class TicketSkill
    {
        private readonly ITicketService _ticketService;
        public TicketSkill(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }
        [KernelFunction("obtener_tickets_por_id_empresa")]
        [Description("Obtiene la lista de tickets asociados a un ID de empresa específico.")]
        public async Task<string> ObtenerTicketsPorEmpresaAsync(
            [Description("El ID numérico de la empresa")] int idEmpresa)
        {
            var tickets = await _ticketService.GetByEmpresaAsync(idEmpresa);

            if (tickets == null || !tickets.Any()) return $"No hay tickets para la empresa con ID {idEmpresa}.";
            var ticketsLimitados = tickets.Take(15).ToList(); // <--- EL TRUCO: Solo devolvemos los 15 primeros
            
            var resultado = $"Se encontraron {tickets.Count()} tickets en total en la BD. Mostrando los 15 más relevantes:\n";
            foreach (var t in ticketsLimitados)
            {
                resultado += $"- Ticket: {t.CodTicket}, Título: {t.Titulo}\n";
            }
            return resultado;
        }
        [KernelFunction("cambiar_estado_ticket")]
        [Description("Cambia el estado de un ticket. Úsalo cuando el usuario pida cambiar de estado (Cerrar, Aprobar, etc).")]
        public async Task<string> CambiarEstadoTicketAsync(
            [Description("El código del ticket (ej: 'REQ-94858')")] string codigoTicket,

            [Description("El estado destino. Valores permitidos (código o nombre): " +
                 "'CERRADO' (Cerrado), " +
                 "'PENDIENTE_ASIGNACION' (Pendiente de Asignación), " +
                 "'CANCELADO' (Cancelado), " +
                 "'RECHAZADO' (Rechazado), " +
                 "'PENDIENTE_CLIENTE' (Pendiente del Cliente), " +
                 "'PENDIENTE_CONSULTOR' (Pendiente Consultoría), " +
                 "'ANULADO' (Anulado), " +
                 "'PENDIENTE_ATENCION' (Pendiente de Atención), " +
                 "'ASIGNADO' (Asignado), " +
                 "'APROBADO' (Aprobado), " +
                 "'EN_EJECUCION' (En Ejecución), " +
                 "'ATENDIDO' (En Atención).")] string codigoNuevoEstado)
        {
            try
            {
                await _ticketService.CambiarEstadoPorCodigoAsync(codigoTicket, codigoNuevoEstado);
                return $"ÉXITO: El ticket {codigoTicket} cambió correctamente al estado {codigoNuevoEstado}.";
            }
            catch (Exception ex)
            {
                return $"ERROR AL CAMBIAR ESTADO: {ex.Message}";
            }
        }


    }
}

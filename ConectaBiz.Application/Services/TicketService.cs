using AutoMapper;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using FluentValidation.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ITicketConsultorAsignacionRepository _consultorAsignacionRepository;
        private readonly ITicketFrenteSubFrenteRepository _frenteSubFrenteRepository;
        private readonly ITicketHistorialRepository _historialRepository;
        private readonly IParametroRepository _parametroRepository;
        private readonly IEmpresaRepository _empresaRepository;
        private readonly IMapper _mapper;

        public TicketService(
            ITicketRepository ticketRepository,
            ITicketConsultorAsignacionRepository consultorAsignacionRepository,
            ITicketFrenteSubFrenteRepository frenteSubFrenteRepository,
            ITicketHistorialRepository historialRepository,
            IParametroRepository parametroRepository,
            IEmpresaRepository empresaRepository,
            IMapper mapper
            )
        {
            _ticketRepository = ticketRepository;
            _consultorAsignacionRepository = consultorAsignacionRepository;
            _frenteSubFrenteRepository = frenteSubFrenteRepository;
            _historialRepository = historialRepository;
            _parametroRepository = parametroRepository;
            _empresaRepository = empresaRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TicketDto>> GetAllAsync()
        {
            var tickets = await _ticketRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TicketDto>>(tickets);
        }

        public async Task<TicketDto?> GetByIdAsync(int id)
        {
            var ticket = await _ticketRepository.GetByIdWithRelationsAsync(id);
            return ticket != null ? _mapper.Map<TicketDto>(ticket) : null;
        }

        public async Task<TicketDto?> GetByCodTicketAsync(string codTicket)
        {
            var ticket = await _ticketRepository.GetByCodTicketAsync(codTicket);
            return ticket != null ? _mapper.Map<TicketDto>(ticket) : null;
        }

        public async Task<IEnumerable<TicketDto>> GetByEmpresaAsync(int idEmpresa)
        {
            var tickets = await _ticketRepository.GetByEmpresaAsync(idEmpresa);
            return _mapper.Map<IEnumerable<TicketDto>>(tickets);
        }

        public async Task<IEnumerable<TicketDto>> GetByEstadoAsync(int idEstado)
        {
            var tickets = await _ticketRepository.GetByEstadoAsync(idEstado);
            return _mapper.Map<IEnumerable<TicketDto>>(tickets);
        }

        //public async Task<IEnumerable<TicketDto>> GetByGestorAsync(int idGestor)
        //{
        //    var tickets = await _ticketRepository.GetByGestorAsync(idGestor);
        //    return _mapper.Map<IEnumerable<TicketDto>>(tickets);
        //}

        public async Task<IEnumerable<TicketDto>> GetTicketsWithFiltersAsync(int? idEmpresa = null, int? idEstado = null, bool? urgente = null)
        {
            var tickets = await _ticketRepository.GetTicketsWithFiltersAsync(idEmpresa, idEstado, urgente);
            return _mapper.Map<IEnumerable<TicketDto>>(tickets);
        }
        public async Task<string> GenerarCodigoTicketAsync(int idTipoTicket)
        {
            string codigoTipoTicket = (await _parametroRepository.GetByIdAsync(idTipoTicket)).Codigo;
            int nextId = (await _ticketRepository.GetAllAsync()).DefaultIfEmpty().Max(t => t?.Id ?? 0) + 1;
            string fechaHora = DateTime.Now.ToString("yyyyMMddHHmmss");

            return $"{codigoTipoTicket}-{nextId}-{fechaHora}";
        }

        public async Task<TicketDto> CreateAsync(TicketInsertDto insertDto)
        {
            try
            {
                // Crear el ticket principal
                var idGestor = (await _empresaRepository.GetByIdAsync(insertDto.IdEmpresa))?.IdGestor;
                var ticket = _mapper.Map<Ticket>(insertDto);
                ticket.IdGestor = (int)idGestor;
                ticket.FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                ticket.FechaSolicitud = DateTime.SpecifyKind(ticket.FechaSolicitud, DateTimeKind.Local);
                ticket.UsuarioCreacion = insertDto.UsuarioCreacion;
                ticket.Activo = true;
                ticket.CodTicket = await GenerarCodigoTicketAsync(insertDto.IdTipoTicket);
                var createdTicket = await _ticketRepository.CreateAsync(ticket);

                // Crear historial inicial de estado
                await CreateInitialHistorialAsync(createdTicket.Id, insertDto.IdEstadoTicket);

                // Crear asignaciones de consultores
                await CreateConsultorAsignacionesAsync(createdTicket.Id, insertDto.ConsultorAsignaciones);

                // Crear frentes y subfrentes
                await CreateFrenteSubFrentesAsync(createdTicket.Id, insertDto.FrenteSubFrentes);
                // Obtener el ticket completo con relaciones
                var fullTicket = await _ticketRepository.GetByIdWithRelationsAsync(createdTicket.Id);
                return _mapper.Map<TicketDto>(fullTicket);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error al crear el ticket con código: {CodTicket}", insertDto.CodTicket);
                throw;
            }
        }

        public async Task<TicketDto> UpdateAsync(int id, TicketUpdateDto updateDto)
        {
            try
            {
                var existingTicket = await _ticketRepository.GetByIdWithRelationsAsync(id);
                if (existingTicket == null)
                {
                    throw new KeyNotFoundException($"No se encontró el ticket con ID: {id}");
                }

                // Guardar estado anterior para historial
                int? estadoAnterior = existingTicket.IdEstadoTicket;

                // Actualizar los campos del ticket principal
                UpdateTicketFields(existingTicket, updateDto);

                // Si cambió el estado, crear registro en historial
                if (updateDto.IdEstadoTicket != estadoAnterior)
                {
                    await CreateHistorialCambioEstadoAsync(id, estadoAnterior, updateDto.IdEstadoTicket, updateDto.UsuarioActualizacion);
                }

                // Validar y actualizar asignaciones de consultores solo si hay cambios
                bool consultoresChanged = await HasConsultorAsignacionesChanged(id, updateDto.ConsultorAsignaciones);
                if (consultoresChanged)
                {
                    await UpdateConsultorAsignacionesAsync(id , updateDto.ConsultorAsignaciones, updateDto.UsuarioActualizacion);
                }

                // Validar y actualizar frentes y subfrentes solo si hay cambios
                bool frentesChanged = await HasFrenteSubFrentesChanged(id, updateDto.FrenteSubFrentes);
                if (frentesChanged)
                {
                    await UpdateFrenteSubFrentesAsync(id, updateDto.FrenteSubFrentes, updateDto.UsuarioActualizacion);
                }

                // Guardar cambios del ticket principal
                await _ticketRepository.UpdateAsync(existingTicket);

                // Obtener el ticket actualizado con relaciones
                var updatedTicket = await _ticketRepository.GetByIdWithRelationsAsync(id);
                return _mapper.Map<TicketDto>(updatedTicket);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error al actualizar el ticket con ID: {Id}", id);
                throw;
            }
        }
        // Método para validar cambios en ConsultorAsignaciones
        private async Task<bool> HasConsultorAsignacionesChanged(int idTicket, List<TicketConsultorAsignacionUpdateDto> newAsignaciones)
        {
            var currentAsignaciones = (await _ticketRepository.GetConsultorAsignacionesActivasByTicketIdAsync(idTicket)).ToList();

            // Si las cantidades son diferentes, hay cambios
            if (currentAsignaciones.Count != newAsignaciones.Count)
                return true;

            // Ordenar ambas listas por una clave consistente (por ejemplo IdConsultor y FechaAsignacion)
            var currentOrdenadas = currentAsignaciones
                .OrderBy(x => x.IdConsultor)
                .ThenBy(x => x.FechaAsignacion)
                .ToList();

            var nuevasOrdenadas = newAsignaciones
                .OrderBy(x => x.IdConsultor)
                .ThenBy(x => x.FechaAsignacion)
                .ToList();

            // Comparar elemento a elemento
            for (int i = 0; i < currentOrdenadas.Count; i++)
            {
                var actual = currentOrdenadas[i];
                var nuevo = nuevasOrdenadas[i];

                if (actual.IdConsultor != nuevo.IdConsultor ||
                    actual.IdTipoActividad != nuevo.IdTipoActividad ||
                    actual.FechaAsignacion != nuevo.FechaAsignacion ||
                    actual.FechaDesasignacion != nuevo.FechaDesasignacion)
                {
                    return true; // Hay diferencias
                }
            }

            return false; // No hay diferencias
        }


        // Método para validar cambios en FrenteSubFrente
        private async Task<bool> HasFrenteSubFrentesChanged(int idTicket, List<TicketFrenteSubFrenteUpdateDto> newFrenteSubFrentes)
        {
            var currentFrenteSubFrentes = await _ticketRepository.GetFrenteSubFrentesActivosByTicketIdAsync(idTicket);

            // Si las cantidades son diferentes, hay cambios
            if (currentFrenteSubFrentes.Count() != newFrenteSubFrentes.Count)
                return true;

            // Crear listas de comparación con combinación Frente-SubFrente
            var currentCombinations = currentFrenteSubFrentes
                .Select(x => new { x.IdFrente, x.IdSubFrente, x.Cantidad })
                .OrderBy(x => x.IdFrente)
                .ThenBy(x => x.IdSubFrente)
                .ThenBy(x => x.Cantidad)
                .ToList();

            var newCombinations = newFrenteSubFrentes
                .Select(x => new { x.IdFrente, x.IdSubFrente, x.Cantidad })
                .OrderBy(x => x.IdFrente)
                .ThenBy(x => x.IdSubFrente)
                .ThenBy(x => x.Cantidad)
                .ToList();

            // Comparar si las combinaciones son iguales
            return !currentCombinations.SequenceEqual(newCombinations);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var exists = await _ticketRepository.GetByIdAsync(id);
                if (exists == null) return false;

                return await _ticketRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error al eliminar el ticket con ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<TicketHistorialEstadoDto>> GetHistorialByTicketIdAsync(int idTicket)
        {
            var historial = await _historialRepository.GetByTicketIdAsync(idTicket);
            return _mapper.Map<IEnumerable<TicketHistorialEstadoDto>>(historial);
        }

        public async Task<TicketDto?> GetByCodReqSgrCstiAsync(string codReqSgrCsti)
        {
            var ticket = await _ticketRepository.GetByCodReqSgrCstiAsync(codReqSgrCsti);
            return ticket != null ? _mapper.Map<TicketDto>(ticket) : null;
        }

        // ============= MÉTODOS PRIVADOS =============

        private async Task CreateInitialHistorialAsync(int ticketId, int estadoInicial)
        {
            // Crear historial inicial automático
            var historialInicial = new TicketHistorialEstado
            {
                IdTicket = ticketId,
                IdEstadoAnterior = estadoInicial,
                IdEstadoNuevo = estadoInicial,
                FechaCambio = DateTime.Now,
                UsuarioCambio = "SYSTEM"
            };

            await _historialRepository.CreateAsync(historialInicial);

            // Crear registros adicionales del historial si vienen en el DTO
            //foreach (var item in historialDto)
            //{
            //    var historial = _mapper.Map<TicketHistorialEstado>(item);
            //    historial.IdTicket = ticketId;
            //    await _historialRepository.CreateAsync(historial);
            //}
        }

        private async Task CreateConsultorAsignacionesAsync(int ticketId,List<TicketConsultorAsignacionInsertDto> asignacionesDto)
        {
            foreach (var asignacionDto in asignacionesDto)
            {
                var asignacion = _mapper.Map<TicketConsultorAsignacion>(asignacionDto);
                asignacion.IdTicket = ticketId;
                asignacion.Activo = true;
                asignacion.FechaAsignacion = DateTime.SpecifyKind(asignacionDto.FechaAsignacion, DateTimeKind.Utc).ToLocalTime();
                asignacion.FechaDesasignacion = DateTime.SpecifyKind(asignacionDto.FechaDesasignacion, DateTimeKind.Utc).ToLocalTime();
                await _consultorAsignacionRepository.CreateAsync(asignacion);
            }
        }

        private async Task CreateFrenteSubFrentesAsync(int ticketId,List<TicketFrenteSubFrenteInsertDto> frenteSubFrentesDto)
        {
            foreach (var frenteSubFrenteDto in frenteSubFrentesDto)
            {
                var frenteSubFrente = _mapper.Map<TicketFrenteSubFrente>(frenteSubFrenteDto);
                frenteSubFrente.IdTicket = ticketId;
                frenteSubFrente.FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc).ToLocalTime();
                frenteSubFrente.UsuarioCreacion = "ecamarena";
                await _frenteSubFrenteRepository.CreateAsync(frenteSubFrente);
            }
        }

        private void UpdateTicketFields(Ticket existingTicket, TicketUpdateDto updateDto)
        {
            existingTicket.FechaActualizacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

            if (!string.IsNullOrEmpty(updateDto.CodTicketInterno)) existingTicket.CodTicketInterno = updateDto.CodTicketInterno;
            if (!string.IsNullOrEmpty(updateDto.Titulo)) existingTicket.Titulo = updateDto.Titulo;
            if (updateDto.FechaSolicitud != DateTime.MinValue) existingTicket.FechaSolicitud = DateTime.SpecifyKind(updateDto.FechaSolicitud, DateTimeKind.Local);
            if (updateDto.IdTipoTicket > 0) existingTicket.IdTipoTicket = updateDto.IdTipoTicket;
            if (updateDto.IdEstadoTicket > 0) existingTicket.IdEstadoTicket = updateDto.IdEstadoTicket;
            if (updateDto.IdEmpresa > 0) existingTicket.IdEmpresa = updateDto.IdEmpresa;
            if (updateDto.IdUsuarioResponsableCliente > 0) existingTicket.IdUsuarioResponsableCliente = updateDto.IdUsuarioResponsableCliente;
            if (updateDto.IdPrioridad > 0) existingTicket.IdPrioridad = updateDto.IdPrioridad;
            if (!string.IsNullOrEmpty(updateDto.Descripcion)) existingTicket.Descripcion = updateDto.Descripcion;
            if (!string.IsNullOrEmpty(updateDto.UrlArchivos)) existingTicket.UrlArchivos = updateDto.UrlArchivos;
            if (!string.IsNullOrEmpty(updateDto.UsuarioActualizacion)) existingTicket.UsuarioActualizacion = updateDto.UsuarioActualizacion;
        }

        private async Task CreateHistorialCambioEstadoAsync(int ticketId, int? estadoAnterior,int estadoNuevo, string? usuarioCambio)
        {
            var historial = new TicketHistorialEstado
            {
                IdTicket = ticketId,
                IdEstadoAnterior = estadoAnterior,
                IdEstadoNuevo = estadoNuevo,
                FechaCambio = DateTime.Now,
                UsuarioCambio = usuarioCambio ?? "SYSTEM"
            };
            await _historialRepository.CreateAsync(historial);
        }

        private async Task UpdateConsultorAsignacionesAsync(int ticketId, List<TicketConsultorAsignacionUpdateDto> nuevasAsignaciones, string? usuarioModificacion)
        {
            if (nuevasAsignaciones.Any())
            {
                // Desactivar todas las asignaciones anteriores
                await _consultorAsignacionRepository.DeactivateAllByTicketIdAsync(ticketId, usuarioModificacion ?? "SYSTEM");

                // Crear las nuevas asignaciones
                foreach (var asignacionDto in nuevasAsignaciones)
                {
                    var asignacion = new TicketConsultorAsignacion
                    {
                        IdTicket = ticketId, 
                        IdConsultor = asignacionDto.IdConsultor,
                        FechaAsignacion = DateTime.SpecifyKind(asignacionDto.FechaAsignacion, DateTimeKind.Local),
                        FechaDesasignacion = DateTime.SpecifyKind(asignacionDto.FechaDesasignacion, DateTimeKind.Local),
                        IdTipoActividad = asignacionDto.IdTipoActividad,
                        Activo = true,
                    }; 
                    await _consultorAsignacionRepository.CreateAsync(asignacion);
                }
            }
        }

        private async Task UpdateFrenteSubFrentesAsync(int ticketId,List<TicketFrenteSubFrenteUpdateDto> nuevosFrenteSubFrentes, string? usuarioModificacion)
        {
            if (nuevosFrenteSubFrentes.Any())
            {
                // Desactivar todos los frente-subfrentes anteriores
                await _frenteSubFrenteRepository.DeactivateAllByTicketIdAsync(ticketId, usuarioModificacion ?? "SYSTEM");

                // Crear los nuevos frente-subfrentes
                foreach (var frenteSubFrenteDto in nuevosFrenteSubFrentes)
                {
                    var frenteSubFrente = new TicketFrenteSubFrente
                    {
                        IdTicket = ticketId,
                        IdFrente = frenteSubFrenteDto.IdFrente,
                        IdSubFrente = frenteSubFrenteDto.IdSubFrente,
                        Cantidad = frenteSubFrenteDto.Cantidad,
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacion = usuarioModificacion ?? "SYSTEM",
                        Activo = true
                    };
                    await _frenteSubFrenteRepository.CreateAsync(frenteSubFrente);
                }
            }
        }
    }
}

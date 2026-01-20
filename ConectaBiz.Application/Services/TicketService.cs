using AutoMapper;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Constants;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using NPOI.SS.Formula;
using NPOI.SS.Formula.Functions;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static ConectaBiz.Domain.Constants.AppConstants;

namespace ConectaBiz.Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ITicketConsultorAsignacionRepository _consultorAsignacionRepository;
        private readonly ITicketFrenteSubFrenteRepository _frenteSubFrenteRepository;
        private readonly ITicketHistorialRepository _historialRepository;
        private readonly IParametrosCatalogo _parametrosCatalogo;
        private readonly IEmpresaRepository _empresaRepository;
        private readonly IGestorService _gestorService;
        private readonly IConsultorService _consultorService;
        private readonly IEmpresaService _empresaService;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly Lazy<INotificacionTicketService> _notificacionTicketService;
        private readonly string _rutaLog;
        private readonly string _rutaBaseArchivos;

        // 🔹 Variables para cachear los datos que cargamos en ProcesarExcelAsync
        private IEnumerable<Parametro> _listaTipoTicket;
        private IEnumerable<Parametro> _listaSubTipoTicket;
        private IEnumerable<Parametro> _listaEstados;
        private IEnumerable<Parametro> _listaPrioridades;
        private IEnumerable<Parametro> _listaParametros;
        private IEnumerable<Parametro> _listaTipoActividad;

        public TicketService(
            IConfiguration configuration,
            ITicketRepository ticketRepository,
            ITicketConsultorAsignacionRepository consultorAsignacionRepository,
            Lazy<INotificacionTicketService> notificacionTicketService,
            ITicketFrenteSubFrenteRepository frenteSubFrenteRepository,
            ITicketHistorialRepository historialRepository,
            IParametrosCatalogo parametrosCatalogo,
            IEmpresaRepository empresaRepository,
            IGestorService gestorService,
            IConsultorService consultorService,
            IEmpresaService empresaService,
            IAuthService authService,
            IMapper mapper,
            IServiceProvider provider
            )
        {
            _ticketRepository = ticketRepository;
            _consultorAsignacionRepository = consultorAsignacionRepository;
            _notificacionTicketService = notificacionTicketService;
            _frenteSubFrenteRepository = frenteSubFrenteRepository;
            _historialRepository = historialRepository;
            _parametrosCatalogo = parametrosCatalogo;
            _empresaRepository = empresaRepository;
            _gestorService = gestorService;
            _consultorService = consultorService;
            _empresaService = empresaService;
            _authService = authService;
            _mapper = mapper;
            _rutaLog = configuration["Logging:LogFilePath"];
            _rutaBaseArchivos = configuration["RepositorioArchivos:RutaBase"];
        }

        // 🔹 Cargar todos los datos necesarios
        private async Task InicializarDatosAsync()
        {
            await _parametrosCatalogo.EnsureLoadedAsync();

            var snap = _parametrosCatalogo.Current;

            _listaParametros = snap.ListaParametros;
            _listaTipoTicket = snap.ListaTipoTicket;
            _listaSubTipoTicket = snap.ListaSubTipoTicket;
            _listaEstados = snap.ListaEstados;
            _listaPrioridades = snap.ListaPrioridades;
            _listaTipoActividad = snap.ListaTipoActividad;
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
        public async Task<TicketDto?> GetByIdSocioNumContribuyenteEmpAsync(int idSocio, string numContribuyenteEmp)
        {
            var ticket = await _ticketRepository.GetByIdSocioNumContribuyenteEmpAsync(idSocio, numContribuyenteEmp);
            return ticket != null ? _mapper.Map<TicketDto>(ticket) : null;
        }
        public async Task<TicketDto?> GetByNumContribuyenteSocioEmpAsync(string numContribuyenteSocio, string numContribuyenteEmp)
        {
            var ticket = await _ticketRepository.GetByNumContribuyenteSocioEmpAsync(numContribuyenteSocio, numContribuyenteEmp);
            return ticket != null ? _mapper.Map<TicketDto>(ticket) : null;
        }
        public async Task<IEnumerable<TicketDto>> GetByIdUserIdRolAsync(int idUser, string codRol)
        {
            IEnumerable<TicketDto> listadoTickets = Enumerable.Empty<TicketDto>();

            if (codRol == AppConstants.Roles.GestorCuenta)
            {
                GestorDto gestorDto = await _gestorService.GetByIdUserAsync(idUser);
                var tickets = await _ticketRepository.GetByGestorAsync(gestorDto.Id);
                listadoTickets = _mapper.Map<IEnumerable<TicketDto>>(tickets)
                .Select(t =>
                {
                    t.HorasTrabajadas = t.ConsultorAsignaciones
                        .SelectMany(ca => ca.DetalleTareasConsultor)
                        .Sum(dt => (int?)dt.Horas) ?? 0;
                    t.HorasPlanificadas = t.ConsultorAsignaciones
                      .SelectMany(ca => ca.DetallePlanificacionConsultor)
                      .Sum(dt => (int?)dt.Horas) ?? 0;
                    return t;
                })
                .ToList();
            }
            else if (codRol == AppConstants.Roles.GestorConsultoria)
            {
                GestorDto gestorDto = await _gestorService.GetByIdUserAsync(idUser);
                var tickets = await _ticketRepository.GetByGestorConsultoriaAsync(gestorDto.Id);
                listadoTickets = _mapper.Map<IEnumerable<TicketDto>>(tickets)
               .Where(t => t.FrenteSubFrentes != null && t.FrenteSubFrentes.Count > 0)
               .Select(t =>
               {
                   t.HorasTrabajadas = t.ConsultorAsignaciones
                       .SelectMany(ca => ca.DetalleTareasConsultor)
                       .Sum(dt => (int?)dt.Horas) ?? 0;
                   t.HorasPlanificadas = t.ConsultorAsignaciones
                     .SelectMany(ca => ca.DetallePlanificacionConsultor)
                     .Sum(dt => (int?)dt.Horas) ?? 0;
                   return t;
               })
               .ToList();
            }
            else if (codRol == AppConstants.Roles.Consultor)
            {
                ConsultorDto consultorDto = await _consultorService.GetByIdUserAsync(idUser);
                var tickets = await _ticketRepository.GetByConsultorAsync(consultorDto.Id);
                listadoTickets = _mapper.Map<IEnumerable<TicketDto>>(tickets)
                  .Select(t =>
                  {
                      // Sumar solo las horas del consultor específico
                      t.HorasTrabajadas = t.ConsultorAsignaciones
                          .Where(ca => ca.IdConsultor == consultorDto.Id)
                          .SelectMany(ca => ca.DetalleTareasConsultor)
                          .Sum(dt => (int?)dt.Horas ?? 0);

                      t.HorasPlanificadas = t.ConsultorAsignaciones
                        .Where(ca => ca.IdConsultor == consultorDto.Id)
                        .SelectMany(ca => ca.DetallePlanificacionConsultor)
                        .Sum(dt => (int?)dt.Horas ?? 0);
                      return t;
                  })
                  .ToList();
            }
            else if (codRol == AppConstants.Roles.Empresa)
            {
                EmpresaDto empresaDto = await _empresaService.GetByIdUserAsync(idUser);
                var tickets = await _ticketRepository.GetByEmpresaAsync(Convert.ToInt32(empresaDto.Id));
                listadoTickets = _mapper.Map<IEnumerable<TicketDto>>(tickets)
               .Select(t =>
               {
                   t.HorasTrabajadas = t.ConsultorAsignaciones
                       .SelectMany(ca => ca.DetalleTareasConsultor)
                       .Sum(dt => (int?)dt.Horas) ?? 0;
                   t.HorasPlanificadas = t.ConsultorAsignaciones
                      .SelectMany(ca => ca.DetallePlanificacionConsultor)
                      .Sum(dt => (int?)dt.Horas) ?? 0;
                   return t;
               })
               .ToList();
            }
            else if (codRol == AppConstants.Roles.Admin)
            {
                UserDto userDto = await _authService.GetByIdAsync(idUser);
                var tickets = await _ticketRepository.GetBySocioAsync(Convert.ToInt32(userDto.Socio.Id));
                listadoTickets = _mapper.Map<IEnumerable<TicketDto>>(tickets)
               .Select(t =>
               {
                   t.HorasTrabajadas = t.ConsultorAsignaciones
                       .SelectMany(ca => ca.DetalleTareasConsultor)
                       .Sum(dt => (int?)dt.Horas) ?? 0;
                   t.HorasPlanificadas = t.ConsultorAsignaciones
                      .SelectMany(ca => ca.DetallePlanificacionConsultor)
                      .Sum(dt => (int?)dt.Horas) ?? 0;
                   return t;
               })
               .ToList();
            }
            else
            {
                var tickets = await _ticketRepository.GetAllAsync();
                listadoTickets = _mapper.Map<IEnumerable<TicketDto>>(tickets)
               .Select(t =>
               {
                   t.HorasTrabajadas = t.ConsultorAsignaciones
                       .SelectMany(ca => ca.DetalleTareasConsultor)
                       .Sum(dt => (int?)dt.Horas) ?? 0;
                   t.HorasPlanificadas = t.ConsultorAsignaciones
                      .SelectMany(ca => ca.DetallePlanificacionConsultor)
                      .Sum(dt => (int?)dt.Horas) ?? 0;
                   return t;
               })
               .ToList();
            }
            return listadoTickets;
        }
        public async Task<IEnumerable<TicketDto>> GetTicketsWithFiltersAsync(int? idEmpresa = null, int? idEstado = null, bool? urgente = null)
        {
            var tickets = await _ticketRepository.GetTicketsWithFiltersAsync(idEmpresa, idEstado, urgente);
            return _mapper.Map<IEnumerable<TicketDto>>(tickets);
        }
        public async Task<string> GenerarCodigoTicketAsync(int idTipoTicket)
        {
            //string codigoTipoTicket = (await _parametroRepository.GetByIdAsync(idTipoTicket)).Codigo;
            string codigoTipoTicket = _listaTipoTicket.FirstOrDefault(t => t.Id.Equals(idTipoTicket)).Codigo; 
            int nextId = (await _ticketRepository.GetAllAsync()).DefaultIfEmpty().Max(t => t?.Id ?? 0) + 1;
            string fechaHora = DateTime.Now.ToString("yyyyMMdd");
            return $"{codigoTipoTicket}-{fechaHora}-{nextId}";
        }
        public async Task<TicketDto> CreateAsync(TicketInsertDto insertDto)
        {
            await InicializarDatosAsync();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var log = new StringBuilder();
            log.AppendLine("========== INICIO CREACIÓN DE TICKET ==========");
            log.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            try
            {

                // 1. Obtener empresa
                var t0 = sw.ElapsedMilliseconds;
                var empresa = await _empresaRepository.GetByIdAsync(insertDto.IdEmpresa);
                log.AppendLine($"DB Get Empresa ms={sw.ElapsedMilliseconds - t0}");

                if (empresa == null)
                {
                    log.AppendLine("❌ Empresa no encontrada en DB");
                    throw new InvalidOperationException("Empresa no encontrada");
                }
                log.AppendLine($"Empresa encontrada: {empresa.NombreComercial} (IdGestor={empresa.IdGestor})");

                // 2. Crear ticket
                var ticket = _mapper.Map<Ticket>(insertDto);
                ticket.IdGestor = empresa.IdGestor;
                ticket.FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
                ticket.FechaSolicitud = DateTime.SpecifyKind(ticket.FechaSolicitud, DateTimeKind.Local);
                ticket.UsuarioCreacion = insertDto.UsuarioCreacion;
                ticket.Activo = true;
                ticket.UrlArchivos = null;
                ticket.Repositorios = insertDto.Repositorios;
                ticket.CodTicket = await GenerarCodigoTicketAsync(insertDto.IdTipoTicket);

                t0 = sw.ElapsedMilliseconds;
                var createdTicket = await _ticketRepository.CreateAsync(ticket);
                log.AppendLine($"DB Create Ticket ms={sw.ElapsedMilliseconds - t0}");
                log.AppendLine($"Ticket creado Id={createdTicket.Id}, CodTicket={ticket.CodTicket}");

                // 3. Procesar archivo ZIP
                if (insertDto.ZipFile != null && insertDto.ZipFile.Length > 0)
                {
                    try
                    {
                        var uploadsFolder = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            _rutaBaseArchivos,
                            "Empresa",
                            insertDto.IdEmpresa.ToString(),
                            createdTicket.Id.ToString()
                        );
                        Directory.CreateDirectory(uploadsFolder);

                        var fileName = $"{Path.GetFileName(insertDto.ZipFile.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        t0 = sw.ElapsedMilliseconds;

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await insertDto.ZipFile.CopyToAsync(stream);
                            log.AppendLine($"ZIP guardado en {filePath} (ms={sw.ElapsedMilliseconds - t0})");
                        }

                        var zipFileDto = new TicketZipFileDto
                        {
                            Orden = 1,
                            Url = Path.Combine(_rutaBaseArchivos, "Empresa", insertDto.IdEmpresa.ToString(), createdTicket.Id.ToString(), fileName).Replace("\\", "/"),
                            FechaInsert = DateTime.Now
                        };

                        var zipFilesList = new List<TicketZipFileDto> { zipFileDto };
                        createdTicket.UrlArchivos = JsonSerializer.Serialize(zipFilesList);

                        t0 = sw.ElapsedMilliseconds;
                        await _ticketRepository.UpdateAsync(createdTicket);
                        log.AppendLine($"DB Update UrlArchivos ms={sw.ElapsedMilliseconds - t0}");
                    }
                    catch (Exception exZip)
                    {
                        log.AppendLine("❌ Error guardando ZIP: " + exZip.Message);
                    }
                }
                else
                {
                    log.AppendLine("ℹ️ No se recibió archivo ZIP.");
                }

                log.AppendLine("========== FIN EXITOSO ==========");

                // 4. Todas las operaciones secundarias de forma SECUENCIAL
                t0 = sw.ElapsedMilliseconds;
                await CreateInitialHistorialAsync(createdTicket.Id, insertDto.IdEstadoTicket);
                log.AppendLine($"CreateInitialHistorialAsync ms={sw.ElapsedMilliseconds - t0}");

                // 5. Notificaciones (SECUENCIAL - NO en background)
                t0 = sw.ElapsedMilliseconds;
                //int[] idsConsultores = insertDto.ConsultorAsignaciones.Select(c => c.IdConsultor).ToArray();
                await CrearNotificacionesAsignacionTicket(
                    createdTicket.Id,
                    ticket.CodTicket,
                    (int)empresa.IdUser,
                    (int)empresa.IdGestor,
                    (int)insertDto.IdGestorConsultoria,
                    []
                );
                log.AppendLine($"CrearNotificacionesAsignacionTicket ms={sw.ElapsedMilliseconds - t0}");

                log.AppendLine($"✅ FIN EXITOSO (total ms={sw.ElapsedMilliseconds})");

                // IMPORTANTE: Log también secuencial (sin Task.Run)
                //await File.AppendAllTextAsync(_rutaLog, log.ToString());
                return _mapper.Map<TicketDto>(createdTicket);
            }
            catch (Exception ex)
            {
                var logError = new StringBuilder();
                logError.AppendLine("========== ERROR EN CREACIÓN DE TICKET ==========");
                logError.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                logError.AppendLine($"Mensaje: {ex.Message}");
                logError.AppendLine($"InnerException: {ex.InnerException?.Message}");
                logError.AppendLine($"InnerInnerException: {ex.InnerException?.InnerException?.Message}");
                logError.AppendLine($"StackTrace: {ex.StackTrace}");
                logError.AppendLine("================================================");
                logError.AppendLine();

                // Log de error también secuencial
                await File.AppendAllTextAsync(_rutaLog, logError.ToString());
                throw;
            }
        }

        private CrearNotificacionDto CrearNotificacion(int ticketId, int userId, string codTicket, string mensaje)
        {
            return new CrearNotificacionDto
            {
                IdTicket = ticketId,
                IdUser = userId,
                Mensaje = mensaje
            };
        }
        private async Task CrearNotificacionesAsignacionTicket(int ticketId, string codTicket, int idUserEmpresa,int idGestor, int idGestorConsultoria, int[] idsConsultores)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var log = new StringBuilder();
            log.AppendLine("========== INICIO CrearNotificacionesAsignacionTicket ==========");
            log.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            try
            {
                var t0 = sw.ElapsedMilliseconds;
                // Obtener ambos gestores en una sola consulta
                var gestores = (await _gestorService.GetByIdsAsync(new[] { idGestor, idGestorConsultoria })).ToDictionary(g => g.Id);

                log.AppendLine($"GetByIdsAsync({idGestor}, {idGestorConsultoria}) ms={sw.ElapsedMilliseconds - t0}");

                // Recuperar cada uno (si existe en el diccionario)
                gestores.TryGetValue(idGestor, out var gestorDto);
                gestores.TryGetValue(idGestorConsultoria, out var gestorConsultoriaDto);


                var candidatos = new List<int> { idUserEmpresa, (int)gestorDto.IdUser, (int)gestorConsultoriaDto.IdUser };
                candidatos.AddRange(idsConsultores);

                // 2. Traer notificaciones ya existentes
                t0 = sw.ElapsedMilliseconds;
                var existentes = await _notificacionTicketService.Value.GetNotificacionesByIdTicketIdUsersAsync(ticketId, candidatos.ToArray());
                log.AppendLine($"GetNotificacionesByIdTicketIdUsersAsync ms={sw.ElapsedMilliseconds - t0}");

                // 🔑 Crear set único por (IdTicket, IdUser)
                var existentesSet = existentes.Select(n => (n.IdTicket, IdUser: (int)n.IdUser)).Distinct().ToHashSet();

                // 3. Filtrar solo los nuevos usuarios
                t0 = sw.ElapsedMilliseconds;
                var nuevosUsuarios = candidatos.Where(id => !existentesSet.Contains((ticketId, id))).ToList();
                log.AppendLine($"Filtrado nuevosUsuarios ms={sw.ElapsedMilliseconds - t0}");

                // 4. Crear notificaciones
                var lstNotificaciones = new List<CrearNotificacionDto>();
                string mensaje = $"El Ticket: {codTicket} ha sido asignado a usted.";

                t0 = sw.ElapsedMilliseconds;
                foreach (var id in nuevosUsuarios)
                {
                    if (idsConsultores.Contains(id))
                    {
                        var consultorDto = await _consultorService.GetByIdAsync(id);
                        lstNotificaciones.Add(CrearNotificacion(ticketId, consultorDto.IdUser, codTicket, mensaje));
                    }
                    else
                    {
                        lstNotificaciones.Add(CrearNotificacion(ticketId, id, codTicket, mensaje));
                    }
                }
                log.AppendLine($"Construcción lstNotificaciones ms={sw.ElapsedMilliseconds - t0}");

                // 5. Guardar en lote
                if (lstNotificaciones.Any())
                {
                    t0 = sw.ElapsedMilliseconds;
                    await _notificacionTicketService.Value.AddRangeAsync(lstNotificaciones);
                    log.AppendLine($"AddRangeAsync ms={sw.ElapsedMilliseconds - t0}");
                }

                log.AppendLine($"✅ FIN EXITOSO (total ms={sw.ElapsedMilliseconds})");
                //await File.AppendAllTextAsync(_rutaLog, log.ToString());
            }
            catch (Exception ex)
            {
                log.AppendLine("❌ ERROR: " + ex.Message);
                log.AppendLine(ex.StackTrace);
            }
        }


        //public async Task ActualizarEstadoDeAprobadoAEnEjecucion()
        //{
        //    // 1. Obtener estados
        //    var estados = await _parametrosCatalogo.GetByTipoParametroAsync(AppConstants.TiposParametros.EstadoTicket);
        //    var idEstadoAprobado = estados.FirstOrDefault(p => p.Codigo == AppConstants.Estados.APROBADO)?.Id ?? throw new Exception("No se encontró el estado APROBADO");
        //    var idEstadoEnEjecucion = estados.FirstOrDefault(p => p.Codigo == AppConstants.Estados.EN_EJECUCION)?.Id ?? throw new Exception("No se encontró el estado EN_EJECUCION");

        //    // 2. Obtener tickets aprobados
        //    var ticketsAprobados = await _ticketRepository.GetByEstadoAsync(idEstadoAprobado);

        //    if (ticketsAprobados == null || !ticketsAprobados.Any())
        //        return; // No hay nada que actualizar

        //    // 3. Evaluar cada ticket
        //    var ticketsAActualizar = new List<Ticket>();

        //    foreach (var ticket in ticketsAprobados)
        //    {
        //        var fechaMaxAsignacion = ticket.ConsultorAsignaciones.Where(c => c.Activo).Select(c => (DateTime?)c.FechaAsignacion).Max();

        //        // 4. Comparar fechas
        //        if (fechaMaxAsignacion.HasValue && fechaMaxAsignacion.Value >= DateTime.Now)
        //        {
        //            ticket.IdEstadoTicket = idEstadoEnEjecucion;
        //            ticket.FechaActualizacion = DateTime.Now;
        //            ticket.UsuarioActualizacion = "System"; // o el usuario actual

        //            ticketsAActualizar.Add(ticket);
        //        }
        //    }

        //    // 5. Guardar cambios
        //    if (ticketsAActualizar.Any())
        //    {
        //        await _ticketRepository.UpdateRangeAsync(ticketsAActualizar);
        //    }
        //}

        private async Task<int> LogicaActualizarEstadosAutomatico(Ticket ticketActual,TicketUpdateDto updateDto)
        {
            string codigoEstadoActual = _listaEstados.First(x => x.Id == ticketActual.IdEstadoTicket).Codigo;

            var idNuevoEstado = updateDto.IdEstadoTicket;

            bool huboCambiosFrentes = false;
            bool huboCambiosAsignaciones = false;

            if (updateDto.FrenteSubFrentes.Any())
            {
                var (modificados, agregados) =await GetConsulFrenteSubFrentesfAsync(ticketActual.Id,updateDto.FrenteSubFrentes);
                huboCambiosFrentes = modificados.Any() || agregados.Any();
            }

            if (updateDto.ConsultorAsignaciones.Any())
            {
                var (asigMod, asigAg, _, _, _, _) =await GetConsultorAsignacionesDiffAsync(ticketActual.Id,updateDto.ConsultorAsignaciones);
                huboCambiosAsignaciones = asigMod.Any() || asigAg.Any();
            }

            switch (codigoEstadoActual)
            {
                case AppConstants.Estados.ATENDIDO:
                    if (huboCambiosFrentes)
                    {
                        idNuevoEstado = _listaEstados.First(x => x.Codigo == AppConstants.Estados.PENDIENTE_ASIGNACION).Id;
                    }
                    break;

                case AppConstants.Estados.PENDIENTE_ASIGNACION:
                    if (huboCambiosAsignaciones)
                    {
                        idNuevoEstado = _listaEstados.First(x => x.Codigo == AppConstants.Estados.ASIGNADO).Id;
                    }
                    break;
            }
            return idNuevoEstado;
        }


        public async Task<TicketDto> UpdateAsync(int id, TicketUpdateDto updateDto)
        {
            try
            {
                await InicializarDatosAsync();

                var lstNotificaciones = new List<CrearNotificacionDto>();

                var jsonOptions = new JsonSerializerOptions{NumberHandling = JsonNumberHandling.AllowReadingFromString};
                var consultores = JsonSerializer.Deserialize<List<TicketConsultorAsignacionUpdateDto>>(updateDto.consultorAsignaciones, jsonOptions);
                updateDto.ConsultorAsignaciones = consultores;

                var frentesSubfrentes = JsonSerializer.Deserialize<List<TicketFrenteSubFrenteUpdateDto>>(updateDto.frenteSubFrentes);
                updateDto.FrenteSubFrentes = frentesSubfrentes;

                var existingTicket = await _ticketRepository.GetByIdWithRelationsAsync(id);
                if (existingTicket == null)
                if (existingTicket == null)
                {
                    throw new KeyNotFoundException($"No se encontró el ticket con ID: {id}");
                }

                // Guardar estado anterior para historial
                int? estadoAnterior = existingTicket.IdEstadoTicket;

                //// Actualizando el Estado según Logica
                updateDto.IdEstadoTicket = await LogicaActualizarEstadosAutomatico(existingTicket, updateDto);

                // Actualizar los campos del ticket principal
                UpdateTicketFields(existingTicket, updateDto);

                // Si cambió el estado, crear registro en historial
                if (updateDto.IdEstadoTicket != estadoAnterior)
                {
                    await CreateHistorialCambioEstadoAsync(id, estadoAnterior, updateDto);
                }

                // Validar y actualizar asignaciones de consultores solo si hay cambios
                var (modificados, agregados, tareasModificadas, tareasAgregadas, planificacionModificadas , planificacionAgregadas) = await GetConsultorAsignacionesDiffAsync(id, updateDto.ConsultorAsignaciones);

                // 🔹 Procesar asignaciones modificadas (incluye eliminaciones lógicas)
                if (modificados.Count > 0)
                {
                    var listaModificados = _mapper.Map<List<TicketConsultorAsignacion>>(modificados).Select(x => {x.IdTicket = id ;return x;}).ToList();
                    await _consultorAsignacionRepository.UpdateRangeAsync(listaModificados);
                }

                // 🔹 Procesar asignaciones agregadas (nuevas)
                if (agregados.Count > 0)
                {
                    var listaAgregados = _mapper.Map<List<TicketConsultorAsignacion>>(agregados).Select(x => {x.IdTicket = id;x.Id = 0;return x;}).ToList();
                    await _consultorAsignacionRepository.CreateRangeAsync(listaAgregados);

                    // Crear Notificaciones solo para nuevas asignaciones
                    var empresa = await _empresaRepository.GetByIdAsync(existingTicket.IdEmpresa);
                    int[] idsConsultoresNuevos = agregados.Select(c => c.IdConsultor).ToArray();
                    await CrearNotificacionesAsignacionTicket(id,existingTicket.CodTicket,(int)empresa.IdUser,(int)empresa.IdGestor,(int)updateDto.IdGestorConsultoria,idsConsultoresNuevos);
                }

                // 🔹 Procesar tareas modificadas (incluye eliminaciones lógicas)
                if (tareasModificadas.Count > 0)
                {
                    var listaTareasModificadas = _mapper.Map<List<DetalleTareasConsultor>>(tareasModificadas);
                    await _consultorAsignacionRepository.UpdateTareasRangeAsync(listaTareasModificadas);
                }

                // 🔹 Procesar tareas agregadas (nuevas)
                if (tareasAgregadas.Count > 0)
                {
                    var listaTareasAgregadas = _mapper.Map<List<DetalleTareasConsultor>>(tareasAgregadas).Select(x => {x.Id = 0;return x;}).ToList();
                    await _consultorAsignacionRepository.CreateTareasRangeAsync(listaTareasAgregadas);
                }
                // 🔹 Procesar planificaciones modificadas (incluye eliminaciones lógicas)
                if (planificacionModificadas.Count > 0)
                {
                    var listaPlanificacionModificadas = _mapper.Map<List<DetallePlanificacionConsultor>>(planificacionModificadas);
                    await _consultorAsignacionRepository.UpdatePlanificacionRangeAsync(listaPlanificacionModificadas);
                }

                // 🔹 Procesar tareas agregadas (nuevas)
                if (planificacionAgregadas.Count > 0)
                {
                    var listaPlanificacionsAgregadas = _mapper.Map<List<DetallePlanificacionConsultor>>(planificacionAgregadas).Select(x => { x.Id = 0; return x; }).ToList();
                    await _consultorAsignacionRepository.CreatePlanificacionRangeAsync(listaPlanificacionsAgregadas);
                }

                // Validar y actualizar frentes y subfrentes solo si hay cambios
                var (frenteSubFrentesmodificados, frenteSubFrentesagregados) = await GetConsulFrenteSubFrentesfAsync(id, updateDto.FrenteSubFrentes);
                var gestorConsultoria = await _gestorService.GetByIdAsync((int)updateDto.IdGestorConsultoria);

                if (frenteSubFrentesmodificados.Count > 0)
                {
                    lstNotificaciones.Add(CrearNotificacion(id, (int)gestorConsultoria.IdUser, existingTicket.CodTicket, $"Se ha modificado una asignación al ticket {existingTicket.CodTicket}"));
                    var listaModificados = _mapper.Map<List<TicketFrenteSubFrente>>(frenteSubFrentesmodificados).Select(x => { x.IdTicket = id; x.UsuarioModificacion = updateDto.UsuarioActualizacion; x.FechaModificacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local); return x; }).ToList();
                    await _frenteSubFrenteRepository.UpdateRangeAsync(listaModificados);
                }
                if (frenteSubFrentesagregados.Count > 0)
                {
                    lstNotificaciones.Add(CrearNotificacion(id, (int)gestorConsultoria.IdUser, existingTicket.CodTicket, $"Se ha agregado una asignación al ticket {existingTicket.CodTicket}"));
                    var listaAgregados = _mapper.Map<List<TicketFrenteSubFrente>>(frenteSubFrentesagregados).Select(x => { x.IdTicket = id; x.UsuarioCreacion = updateDto.UsuarioActualizacion; x.Id = 0; x.FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local); x.FechaModificacion = null; return x; }).ToList();
                    await _frenteSubFrenteRepository.CreateRangeAsync(listaAgregados);
                }

                if (lstNotificaciones.Any())
                {
                    await _notificacionTicketService.Value.AddRangeAsync(lstNotificaciones);
                }


                // 📌 Manejo del ZIP adicional
                if (updateDto.ZipFile != null && updateDto.ZipFile.Length > 0)
                {
                    // 1. Leer lista existente de archivos desde JSON
                    var listaZips = string.IsNullOrEmpty(existingTicket.UrlArchivos)
                        ? new List<TicketZipFileDto>()
                        : System.Text.Json.JsonSerializer.Deserialize<List<TicketZipFileDto>>(existingTicket.UrlArchivos);

                    // 2. Guardar el nuevo ZIP
                    var uploadsFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        _rutaBaseArchivos,
                        "Empresa",
                        existingTicket.IdEmpresa.ToString(),
                        id.ToString()
                    );
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = $"{Path.GetFileName(updateDto.ZipFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await updateDto.ZipFile.CopyToAsync(stream);
                    }

                    // 3. Agregar a la lista con el siguiente orden
                    var nuevoZip = new TicketZipFileDto
                    {
                        Orden = listaZips.Count + 1,
                        Url = Path.Combine(_rutaBaseArchivos, "Empresa", existingTicket.IdEmpresa.ToString(), id.ToString(), fileName).Replace("\\", "/"),
                        FechaInsert = DateTime.Now
                    };

                    listaZips.Add(nuevoZip);

                    // 4. Serializar de nuevo
                    existingTicket.UrlArchivos = JsonSerializer.Serialize(listaZips);
                }
                AplicarRepositorios(existingTicket, updateDto);

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
        private void AplicarRepositorios(Ticket existingTicket, TicketUpdateDto updateDto)
        {
            // Si el front no envía nada, no tocamos lo existente
            if (string.IsNullOrWhiteSpace(updateDto.Repositorios))
                return;

            // 1️⃣ Repositorios existentes (con Orden y FechaInsert)
            var existentes = string.IsNullOrWhiteSpace(existingTicket.Repositorios)
                ? new List<RepositoriosDto>()
                : JsonSerializer.Deserialize<List<RepositoriosDto>>(existingTicket.Repositorios)
                  ?? new List<RepositoriosDto>();

            // 2️⃣ Repositorios que vienen del frontend (sin FechaInsert)
            var incoming = JsonSerializer.Deserialize<List<JsonElement>>(updateDto.Repositorios)
                           ?? new List<JsonElement>();

            var ahora = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

            // 3️⃣ Determinar el último orden existente
            int ultimoOrden = existentes.Any()
                ? existentes.Max(x => x.Orden)
                : 0;

            var resultado = new List<RepositoriosDto>();

            foreach (var item in incoming)
            {
                var url = item.TryGetProperty("Url", out var urlProp)
                    ? urlProp.GetString()
                    : item.TryGetProperty("Link", out var linkProp)
                        ? linkProp.GetString()
                        : null;

                url = (url ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(url))
                    continue;

                // 🔎 Buscar si ya existe
                var existente = existentes.FirstOrDefault(e =>
                    string.Equals(e.Url?.Trim(), url, StringComparison.OrdinalIgnoreCase));

                if (existente != null)
                {
                    // ✔ Ya existía → conserva Orden y FechaInsert
                    resultado.Add(new RepositoriosDto
                    {
                        Orden = existente.Orden,
                        Url = existente.Url,
                        FechaInsert = existente.FechaInsert
                    });
                }
                else
                {
                    // 🆕 Nuevo → Orden = max + 1, Fecha nueva
                    ultimoOrden++;

                    resultado.Add(new RepositoriosDto
                    {
                        Orden = ultimoOrden,
                        Url = url,
                        FechaInsert = ahora
                    });
                }
            }

            // 4️⃣ Guardar JSON actualizado
            existingTicket.Repositorios = JsonSerializer.Serialize(resultado);
        }

        private async Task<(List<TicketFrenteSubFrenteUpdateDto> modificados  , List<TicketFrenteSubFrenteUpdateDto> agregados)>
        GetConsulFrenteSubFrentesfAsync(int idTicket, List<TicketFrenteSubFrenteUpdateDto> newFrenteSubfrente)
        {
            var modificados = new List<TicketFrenteSubFrenteUpdateDto>();
            var agregados = new List<TicketFrenteSubFrenteUpdateDto>();
       
            // 🔹 Procesar asignaciones basándose únicamente en Id y Activo
            foreach (var frenteSubFrente in newFrenteSubfrente)
            {
                // Ajustar fechas a hora local
                frenteSubFrente.FechaInicio = DateTime.SpecifyKind(frenteSubFrente.FechaInicio, DateTimeKind.Local);
                frenteSubFrente.FechaFin = DateTime.SpecifyKind(frenteSubFrente.FechaFin, DateTimeKind.Local);

                if (frenteSubFrente.Id == 0)
                {
                    // ✅ Nueva asignación (Id = 0)
                    agregados.Add(frenteSubFrente);
                }
                else
                {
                    // ✅ Asignación existente (Id > 0) - puede ser modificación o eliminación lógica
                    // Si Activo = false, es eliminación lógica
                    // Si Activo = true, es modificación/actualización
                    modificados.Add(frenteSubFrente);
                }
            }
            return (modificados, agregados);
        }
        // Método para validar cambios en ConsultorAsignaciones
        private async Task<(
            List<TicketConsultorAsignacionUpdateDto> modificados, 
            List<TicketConsultorAsignacionUpdateDto> agregados, 
            List<DetalleTareasConsultorUpdateDto> tareasModificadas, 
            List<DetalleTareasConsultorUpdateDto> tareasAgregadas,
            List<DetallePlanificacionConsultorUpdateDto> planificacionModificadas,
            List<DetallePlanificacionConsultorUpdateDto> planificacionAgregadas
            )>
          GetConsultorAsignacionesDiffAsync(int idTicket, List<TicketConsultorAsignacionUpdateDto> newAsignaciones)
        {
            var modificados = new List<TicketConsultorAsignacionUpdateDto>();
            var agregados = new List<TicketConsultorAsignacionUpdateDto>();
            var tareasModificadas = new List<DetalleTareasConsultorUpdateDto>();
            var tareasAgregadas = new List<DetalleTareasConsultorUpdateDto>();
            var planificacionModificadas = new List<DetallePlanificacionConsultorUpdateDto>();
            var planificacionAgregadas = new List<DetallePlanificacionConsultorUpdateDto>();

            // 🔹 Procesar asignaciones basándose únicamente en Id y Activo
            foreach (var asignacion in newAsignaciones)
            {
                // Ajustar fechas a hora local
                asignacion.FechaAsignacion = DateTime.SpecifyKind(asignacion.FechaAsignacion, DateTimeKind.Local);
                asignacion.FechaDesasignacion = DateTime.SpecifyKind(asignacion.FechaDesasignacion, DateTimeKind.Local); 

                if (asignacion.Id == 0)
                {
                    // ✅ Nueva asignación (Id = 0)
                    agregados.Add(asignacion);
                }
                else
                {
                    // ✅ Asignación existente (Id > 0) - puede ser modificación o eliminación lógica
                    // Si Activo = false, es eliminación lógica
                    // Si Activo = true, es modificación/actualización
                    modificados.Add(asignacion);
                }
            }

            // 🔹 Procesar tareas basándose únicamente en Id y Activo
            foreach (var asignacion in newAsignaciones)
            {
                foreach (var tarea in asignacion.DetalleTareasConsultor)
                {
                    // Ajustar fechas a hora local
                    tarea.FechaInicio = DateTime.SpecifyKind(tarea.FechaInicio, DateTimeKind.Local);
                    tarea.FechaFin = DateTime.SpecifyKind(tarea.FechaFin, DateTimeKind.Local);

                    if (tarea.Id == 0)
                    {
                        // ✅ Nueva tarea (Id = 0)
                        tareasAgregadas.Add(tarea);
                    }
                    else
                    {
                        // ✅ Tarea existente (Id > 0) - puede ser modificación o eliminación lógica
                        // Si Activo = false, es eliminación lógica
                        // Si Activo = true, es modificación/actualización
                        tareasModificadas.Add(tarea);
                    }
                }
            }

            // 🔹 Procesar planificacion basándose únicamente en Id y Activo
            foreach (var asignacion in newAsignaciones)
            {
                foreach (var planificacion in asignacion.DetallePlanificacionConsultor)
                {
                    // Ajustar fechas a hora local
                    planificacion.FechaInicio = DateTime.SpecifyKind(planificacion.FechaInicio, DateTimeKind.Local);
                    planificacion.FechaFin = DateTime.SpecifyKind(planificacion.FechaFin, DateTimeKind.Local);

                    if (planificacion.Id == 0)
                    {
                        // ✅ Nueva planificacion (Id = 0)
                        planificacionAgregadas.Add(planificacion);
                    }
                    else
                    {
                        // ✅ Planificacion existente (Id > 0) - puede ser modificación o eliminación lógica
                        // Si Activo = false, es eliminación lógica
                        // Si Activo = true, es modificación/actualización
                        planificacionModificadas.Add(planificacion);
                    }
                }
            }
            return (modificados, agregados, tareasModificadas, tareasAgregadas, planificacionModificadas, planificacionAgregadas);
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
        }

        private void UpdateTicketFields(Ticket existingTicket, TicketUpdateDto updateDto)
        {
            existingTicket.FechaActualizacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

            if (!string.IsNullOrEmpty(updateDto.CodTicketInterno)) existingTicket.CodTicketInterno = updateDto.CodTicketInterno;
            if (!string.IsNullOrEmpty(updateDto.Titulo)) existingTicket.Titulo = updateDto.Titulo;
            if (updateDto.FechaSolicitud != DateTime.MinValue) existingTicket.FechaSolicitud = DateTime.SpecifyKind(updateDto.FechaSolicitud, DateTimeKind.Local);
            if (updateDto.IdTipoTicket > 0) existingTicket.IdTipoTicket = updateDto.IdTipoTicket;
            if (updateDto.IdSubTipoTicket > 0) existingTicket.IdSubTipoTicket = updateDto.IdSubTipoTicket;
            if (updateDto.IdEstadoTicket > 0) existingTicket.IdEstadoTicket = updateDto.IdEstadoTicket;
            if (updateDto.IdEmpresa > 0) existingTicket.IdEmpresa = updateDto.IdEmpresa;
            if (updateDto.IdUsuarioResponsableCliente > 0) existingTicket.IdUsuarioResponsableCliente = updateDto.IdUsuarioResponsableCliente;
            if (updateDto.IdPrioridad > 0) existingTicket.IdPrioridad = updateDto.IdPrioridad;
            if (updateDto.IdGestorConsultoria > 0) existingTicket.IdGestorConsultoria = updateDto.IdGestorConsultoria;
            if (!string.IsNullOrEmpty(updateDto.Descripcion)) existingTicket.Descripcion = updateDto.Descripcion;
            if (!string.IsNullOrEmpty(updateDto.UsuarioActualizacion)) existingTicket.UsuarioActualizacion = updateDto.UsuarioActualizacion;
        }
        private async Task CreateHistorialCambioEstadoAsync(int ticketId, int? estadoAnterior, TicketUpdateDto updateDto)
        {
            var historial = new TicketHistorialEstado
            {
                IdTicket = ticketId,
                IdEstadoAnterior = estadoAnterior,
                IdEstadoNuevo = updateDto.IdEstadoTicket,
                FechaCambio = DateTime.Now,
                UsuarioCambio = updateDto.UsuarioActualizacion ?? "SYSTEM"
            };
            await _historialRepository.CreateAsync(historial);
        }

        public async Task<FileStreamResult> DescargarArchivoAsync(int idTicket, int orden)
        {
            var ticket = await _ticketRepository.GetByIdAsync(idTicket)
                ?? throw new FileNotFoundException("Ticket no existe");

            if (string.IsNullOrEmpty(ticket.UrlArchivos))
                throw new FileNotFoundException("El ticket no tiene archivos");

            var lista = JsonSerializer.Deserialize<List<TicketZipFileDto>>(ticket.UrlArchivos)
                        ?? new();

            var archivo = lista.FirstOrDefault(x => x.Orden == orden)
                ?? throw new FileNotFoundException("Archivo no existe");

            var fullPath = Path.GetFullPath(
                Path.Combine(Directory.GetCurrentDirectory(), archivo.Url)
            );

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Archivo no existe en disco");

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fullPath, out var contentType))
                contentType = "application/octet-stream";

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return new FileStreamResult(stream, contentType)
            {
                FileDownloadName = Path.GetFileName(fullPath)
            };
        }

    }
}
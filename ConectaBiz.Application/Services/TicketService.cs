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
using Microsoft.EntityFrameworkCore;

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
            if (ticket == null) return null;
            var dto = _mapper.Map<TicketDto>(ticket);
            await PopulatePlaceholderAssignmentsAsync(new List<TicketDto> { dto });
            return dto;
        }

        public async Task<TicketDto?> GetByCodTicketAsync(string codTicket)
        {
            var ticket = await _ticketRepository.GetByCodTicketAsync(codTicket);
            if (ticket == null) return null;
            var dto = _mapper.Map<TicketDto>(ticket);
            await PopulatePlaceholderAssignmentsAsync(new List<TicketDto> { dto });
            return dto;
        }

        public async Task<IEnumerable<TicketDto>> GetByEmpresaAsync(int idEmpresa)
        {
            var tickets = await _ticketRepository.GetByEmpresaAsync(idEmpresa);
            var dtos = _mapper.Map<IEnumerable<TicketDto>>(tickets).ToList();
            await PopulatePlaceholderAssignmentsAsync(dtos);
            return dtos;
        }

        public async Task<IEnumerable<TicketDto>> GetByEstadoAsync(int idEstado)
        {
            var tickets = await _ticketRepository.GetByEstadoAsync(idEstado);
            var dtos = _mapper.Map<IEnumerable<TicketDto>>(tickets).ToList();
            await PopulatePlaceholderAssignmentsAsync(dtos);
            return dtos;
        }
        public async Task<TicketDto?> GetByIdSocioNumContribuyenteEmpAsync(int idSocio, string numContribuyenteEmp)
        {
            var ticket = await _ticketRepository.GetByIdSocioNumContribuyenteEmpAsync(idSocio, numContribuyenteEmp);
            if (ticket == null) return null;
            var dto = _mapper.Map<TicketDto>(ticket);
            await PopulatePlaceholderAssignmentsAsync(new List<TicketDto> { dto });
            return dto;
        }
        public async Task<TicketDto?> GetByNumContribuyenteSocioEmpAsync(string numContribuyenteSocio, string numContribuyenteEmp)
        {
            var ticket = await _ticketRepository.GetByNumContribuyenteSocioEmpAsync(numContribuyenteSocio, numContribuyenteEmp);
            if (ticket == null) return null;
            var dto = _mapper.Map<TicketDto>(ticket);
            await PopulatePlaceholderAssignmentsAsync(new List<TicketDto> { dto });
            return dto;
        }
        public async Task<IEnumerable<TicketDto>> GetByIdUserIdRolAsync(int idUser, string codRol)
        {
            List<TicketDto> listadoTickets = new List<TicketDto>();

            if (codRol == AppConstants.Roles.GestorCuenta)
            {
                GestorDto gestorDto = await _gestorService.GetByIdUserAsync(idUser);
                var tickets = await _ticketRepository.GetByGestorAsync(gestorDto.Id);
                listadoTickets = _mapper.Map<IEnumerable<TicketDto>>(tickets).ToList();
                await PopulatePlaceholderAssignmentsAsync(listadoTickets);
                listadoTickets = listadoTickets
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
               .ToList();
                await PopulatePlaceholderAssignmentsAsync(listadoTickets);
                listadoTickets = listadoTickets
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
                listadoTickets = _mapper.Map<IEnumerable<TicketDto>>(tickets).ToList();
                await PopulatePlaceholderAssignmentsAsync(listadoTickets);
                listadoTickets = listadoTickets
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
                listadoTickets = _mapper.Map<IEnumerable<TicketDto>>(tickets).ToList();
                await PopulatePlaceholderAssignmentsAsync(listadoTickets);
                listadoTickets = listadoTickets
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
                listadoTickets = _mapper.Map<IEnumerable<TicketDto>>(tickets).ToList();
                await PopulatePlaceholderAssignmentsAsync(listadoTickets);
                listadoTickets = listadoTickets
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
            var dtos = _mapper.Map<IEnumerable<TicketDto>>(tickets).ToList();
            await PopulatePlaceholderAssignmentsAsync(dtos);
            return dtos;
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
                ticket.IdEstadoTicket = _listaEstados.First(x => x.Codigo == AppConstants.Estados.PENDIENTE_ATENCION).Id;
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
                candidatos.AddRange(idsConsultores.Where(id => id > 0));

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
                        if (consultorDto != null)
                        {
                            lstNotificaciones.Add(CrearNotificacion(ticketId, consultorDto.IdUser, codTicket, mensaje));
                        }
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
        //    var idEstadoAprobado = _listaEstados.First(x => x.Codigo == AppConstants.Estados.ASIGNADO).Id;
        //    var idEstadoEnEjecucion = _listaEstados.First(x => x.Codigo == AppConstants.Estados.EN_EJECUCION).Id;

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
            string codigoEstadoActual = ticketActual.IdEstadoTicket > 0
                ? _listaEstados.First(x => x.Id == ticketActual.IdEstadoTicket).Codigo
                : _listaEstados.First(x => x.Id == updateDto.IdEstadoTicket).Codigo;

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
                var (asigMod, asigAg, _, _) =await GetConsultorAsignacionesDiffAsync(ticketActual.Id,updateDto.ConsultorAsignaciones);
                huboCambiosAsignaciones = asigMod.Any() || asigAg.Any();
            }

            bool tienePlanificacion = (updateDto.FrenteSubFrentes != null && updateDto.FrenteSubFrentes.Any(fsf => fsf.DetallePlanificacionConsultor != null && fsf.DetallePlanificacionConsultor.Any(dp => dp.Activo)))
                || (ticketActual.FrenteSubFrentes != null && ticketActual.FrenteSubFrentes.Any(fsf => fsf.DetallePlanificacionConsultor != null && fsf.DetallePlanificacionConsultor.Any(dp => dp.Activo)));

            switch (codigoEstadoActual)
            {
                case AppConstants.Estados.ATENDIDO:
                    if (tienePlanificacion)
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

                case AppConstants.Estados.PENDIENTE_ATENCION:
                    if (huboCambiosFrentes)
                    {
                        idNuevoEstado = _listaEstados.First(x => x.Codigo == AppConstants.Estados.ATENDIDO).Id;
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

                if (consultores != null)
                {
                    foreach (var c in consultores)
                    {
                        if (c.IdConsultor == 0)
                        {
                            c.IdConsultor = null;
                        }
                    }
                }

                var frentesSubfrentes = JsonSerializer.Deserialize<List<TicketFrenteSubFrenteUpdateDto>>(updateDto.frenteSubFrentes, jsonOptions);
                updateDto.FrenteSubFrentes = frentesSubfrentes;

                var existingTicket = await _ticketRepository.GetByIdWithRelationsAsync(id);
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

                // 1️⃣ Validar y actualizar frentes y subfrentes primero (para tener sus IDs de base de datos)
                var (frenteSubFrentesmodificados, frenteSubFrentesagregados) = await GetConsulFrenteSubFrentesfAsync(id, updateDto.FrenteSubFrentes);
                var gestorConsultoria = await _gestorService.GetByIdAsync((int)updateDto.IdGestorConsultoria);

                var listaFrentesModificados = new List<TicketFrenteSubFrente>();
                var listaFrentesAgregados = new List<TicketFrenteSubFrente>();

                if (frenteSubFrentesmodificados.Count > 0)
                {
                    lstNotificaciones.Add(CrearNotificacion(id, (int)gestorConsultoria.IdUser, existingTicket.CodTicket, $"Se ha modificado una asignación al ticket {existingTicket.CodTicket}"));
                    listaFrentesModificados = _mapper.Map<List<TicketFrenteSubFrente>>(frenteSubFrentesmodificados).Select(x => { x.IdTicket = id; x.UsuarioModificacion = updateDto.UsuarioActualizacion; x.FechaModificacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local); return x; }).ToList();
                    await _frenteSubFrenteRepository.UpdateRangeAsync(listaFrentesModificados);
                }
                if (frenteSubFrentesagregados.Count > 0)
                {
                    lstNotificaciones.Add(CrearNotificacion(id, (int)gestorConsultoria.IdUser, existingTicket.CodTicket, $"Se ha agregado una asignación al ticket {existingTicket.CodTicket}"));
                    listaFrentesAgregados = _mapper.Map<List<TicketFrenteSubFrente>>(frenteSubFrentesagregados).Select(x => { x.IdTicket = id; x.UsuarioCreacion = updateDto.UsuarioActualizacion; x.Id = 0; x.FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local); x.FechaModificacion = null; return x; }).ToList();
                    await _frenteSubFrenteRepository.CreateRangeAsync(listaFrentesAgregados);
                }

                // 2️⃣ Obtener un mapa actualizado de todos los frentes activos de este ticket para resolver IdTicketFrenteSubFrente
                var todosFrentesActivos = await _frenteSubFrenteRepository.GetActivosByTicketIdAsync(id);
                // Usar ToLookup en vez de ToDictionary para soportar múltiples frenteSubFrentes con el mismo IdSubFrente
                var frenteIdLookup = todosFrentesActivos.ToLookup(f => f.IdSubFrente, f => f.Id);
                // Mapa directo por Id de TicketFrenteSubFrente para resolución exacta
                var frenteByIdMap = todosFrentesActivos.ToDictionary(f => f.Id, f => f);

                // Validar y actualizar asignaciones de consultores solo si hay cambios
                var asignacionesList = updateDto.ConsultorAsignaciones ?? new List<TicketConsultorAsignacionUpdateDto>();

                var nuevasPlanificacionesAInsertar = new List<DetallePlanificacionConsultor>();
                var planificacionesAModificar = new List<DetallePlanificacionConsultor>();
                var planIdsProcesados = new HashSet<int>();

                if (updateDto.FrenteSubFrentes != null)
                {
                    foreach (var frenteDto in updateDto.FrenteSubFrentes)
                    {
                        if (!frenteDto.Activo) continue;

                        int resolvedFrenteSubFrenteId = 0;
                        if (frenteDto.Id > 0 && frenteByIdMap.ContainsKey(frenteDto.Id))
                        {
                            resolvedFrenteSubFrenteId = frenteDto.Id;
                        }
                        else if (frenteDto.IdSubFrente.HasValue)
                        {
                            resolvedFrenteSubFrenteId = frenteIdLookup[frenteDto.IdSubFrente.Value].FirstOrDefault();
                        }

                        if (resolvedFrenteSubFrenteId == 0) continue;

                        if (frenteDto.DetallePlanificacionConsultor != null)
                        {
                            foreach (var planDto in frenteDto.DetallePlanificacionConsultor)
                            {
                                if (planDto.Id > 0 && !planIdsProcesados.Add(planDto.Id))
                                {
                                    continue;
                                }

                                planDto.FechaInicio = DateTime.SpecifyKind(planDto.FechaInicio, DateTimeKind.Local);
                                planDto.FechaFin = DateTime.SpecifyKind(planDto.FechaFin, DateTimeKind.Local);
                                var planEntity = _mapper.Map<DetallePlanificacionConsultor>(planDto);
                                planEntity.IdTicketFrenteSubFrente = resolvedFrenteSubFrenteId;

                                if (planDto.Id == 0)
                                {
                                    planEntity.Id = 0;
                                    nuevasPlanificacionesAInsertar.Add(planEntity);
                                }
                                else
                                {
                                    planificacionesAModificar.Add(planEntity);
                                }
                            }
                        }
                    }
                }
                
                var nuevasTareasAInsertar = new List<DetalleTareasConsultor>();
                var tareasAModificar = new List<DetalleTareasConsultor>();

                var tareaIdsProcesados = new HashSet<int>();

                var asignacionesAModificar = new List<TicketConsultorAsignacion>();
                var nuevasAsignacionesAInsertar = new List<TicketConsultorAsignacion>();

                var mapaNuevasAsignaciones = new List<(TicketConsultorAsignacionUpdateDto Dto, TicketConsultorAsignacion Entity)>();
                
                // Trackear qué IDs de TicketFrenteSubFrente ya se asignaron (para duplicados)
                var usedFrenteSubFrenteIds = new HashSet<int>();

                foreach (var asigDto in asignacionesList)
                {
                    asigDto.FechaAsignacion = DateTime.SpecifyKind(asigDto.FechaAsignacion, DateTimeKind.Local);
                    asigDto.FechaDesasignacion = DateTime.SpecifyKind(asigDto.FechaDesasignacion, DateTimeKind.Local);

                    bool esPlaceholder = asigDto.IdConsultor == null || asigDto.IdConsultor == 0;
                    
                    // Resolver IdTicketFrenteSubFrente: primero usar el que envía el frontend, luego buscar por IdSubFrente
                    int resolvedFrenteSubFrenteId = 0;
                    if (asigDto.IdTicketFrenteSubFrente.HasValue && asigDto.IdTicketFrenteSubFrente.Value > 0
                        && frenteByIdMap.ContainsKey(asigDto.IdTicketFrenteSubFrente.Value))
                    {
                        resolvedFrenteSubFrenteId = asigDto.IdTicketFrenteSubFrente.Value;
                        usedFrenteSubFrenteIds.Add(resolvedFrenteSubFrenteId);
                    }
                    else if (asigDto.IdSubFrente.HasValue)
                    {
                        // Buscar el primer ID de TicketFrenteSubFrente disponible para este subfrente
                        var candidatos = frenteIdLookup[asigDto.IdSubFrente.Value];
                        foreach (var candidatoId in candidatos)
                        {
                            if (!usedFrenteSubFrenteIds.Contains(candidatoId))
                            {
                                resolvedFrenteSubFrenteId = candidatoId;
                                usedFrenteSubFrenteIds.Add(candidatoId);
                                break;
                            }
                        }
                        // Si todos estaban usados, usar el primero disponible
                        if (resolvedFrenteSubFrenteId == 0 && candidatos.Any())
                        {
                            resolvedFrenteSubFrenteId = candidatos.First();
                        }
                    }

                    if (asigDto.Id == 0)
                    {
                        if (!esPlaceholder)
                        {
                            var entity = _mapper.Map<TicketConsultorAsignacion>(asigDto);
                            entity.IdTicket = id;
                            entity.Id = 0;
                            entity.IdTicketFrenteSubFrente = resolvedFrenteSubFrenteId > 0 ? resolvedFrenteSubFrenteId : (int?)null;
                            nuevasAsignacionesAInsertar.Add(entity);
                            mapaNuevasAsignaciones.Add((asigDto, entity));
                        }
                    }
                    else
                    {
                        if (!esPlaceholder)
                        {
                            var entity = _mapper.Map<TicketConsultorAsignacion>(asigDto);
                            entity.IdTicket = id;
                            entity.IdTicketFrenteSubFrente = resolvedFrenteSubFrenteId > 0 ? resolvedFrenteSubFrenteId : (int?)null;
                            asignacionesAModificar.Add(entity);

                            foreach (var tareaDto in asigDto.DetalleTareasConsultor)
                            {
                                if (tareaDto.Id > 0 && !tareaIdsProcesados.Add(tareaDto.Id))
                                {
                                    continue;
                                }

                                tareaDto.FechaInicio = DateTime.SpecifyKind(tareaDto.FechaInicio, DateTimeKind.Local);
                                tareaDto.FechaFin = DateTime.SpecifyKind(tareaDto.FechaFin, DateTimeKind.Local);
                                var tareaEntity = _mapper.Map<DetalleTareasConsultor>(tareaDto);
                                tareaEntity.IdTicketConsultorAsignacion = asigDto.Id;

                                if (tareaDto.Id == 0)
                                {
                                    tareaEntity.Id = 0;
                                    nuevasTareasAInsertar.Add(tareaEntity);
                                }
                                else
                                {
                                    tareasAModificar.Add(tareaEntity);
                                }
                            }
                        }
                    }
                }

                if (asignacionesAModificar.Count > 0)
                {
                    await _consultorAsignacionRepository.UpdateRangeAsync(asignacionesAModificar);
                }

                if (nuevasAsignacionesAInsertar.Count > 0)
                {
                    await _consultorAsignacionRepository.CreateRangeAsync(nuevasAsignacionesAInsertar);

                    var empresa = await _empresaRepository.GetByIdAsync(existingTicket.IdEmpresa);
                    int[] idsConsultoresNuevos = nuevasAsignacionesAInsertar
                        .Select(c => c.IdConsultor)
                        .Where(idConsultor => idConsultor.HasValue && idConsultor.Value > 0)
                        .Select(idConsultor => idConsultor.Value)
                        .ToArray();
                    await CrearNotificacionesAsignacionTicket(id, existingTicket.CodTicket, (int)empresa.IdUser, (int)empresa.IdGestor, (int)updateDto.IdGestorConsultoria, idsConsultoresNuevos);

                    foreach (var item in mapaNuevasAsignaciones)
                    {
                        int newAsignacionId = item.Entity.Id;

                        foreach (var tareaDto in item.Dto.DetalleTareasConsultor)
                        {
                            if (tareaDto.Id > 0 && !tareaIdsProcesados.Add(tareaDto.Id))
                            {
                                continue;
                            }

                            tareaDto.FechaInicio = DateTime.SpecifyKind(tareaDto.FechaInicio, DateTimeKind.Local);
                            tareaDto.FechaFin = DateTime.SpecifyKind(tareaDto.FechaFin, DateTimeKind.Local);
                            var tareaEntity = _mapper.Map<DetalleTareasConsultor>(tareaDto);
                            tareaEntity.Id = 0;
                            tareaEntity.IdTicketConsultorAsignacion = newAsignacionId;
                            nuevasTareasAInsertar.Add(tareaEntity);
                        }
                    }
                }

                if (tareasAModificar.Count > 0)
                {
                    await _consultorAsignacionRepository.UpdateTareasRangeAsync(tareasAModificar);
                }
                if (nuevasTareasAInsertar.Count > 0)
                {
                    await _consultorAsignacionRepository.CreateTareasRangeAsync(nuevasTareasAInsertar);
                }

                if (planificacionesAModificar.Count > 0)
                {
                    await _consultorAsignacionRepository.UpdatePlanificacionRangeAsync(planificacionesAModificar);
                }
                if (nuevasPlanificacionesAInsertar.Count > 0)
                {
                    await _consultorAsignacionRepository.CreatePlanificacionRangeAsync(nuevasPlanificacionesAInsertar);
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
                var resultDto = _mapper.Map<TicketDto>(updatedTicket);
                await PopulatePlaceholderAssignmentsAsync(new List<TicketDto> { resultDto });
                return resultDto;
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
            List<DetalleTareasConsultorUpdateDto> tareasAgregadas
            )>
          GetConsultorAsignacionesDiffAsync(int idTicket, List<TicketConsultorAsignacionUpdateDto> newAsignaciones)
        {
            var modificados = new List<TicketConsultorAsignacionUpdateDto>();
            var agregados = new List<TicketConsultorAsignacionUpdateDto>();
            var tareasModificadas = new List<DetalleTareasConsultorUpdateDto>();
            var tareasAgregadas = new List<DetalleTareasConsultorUpdateDto>();

            // 🔹 Procesar asignaciones basándose únicamente en Id y Activo
            foreach (var asignacion in newAsignaciones)
            {
                // Ajustar fechas a hora local
                asignacion.FechaAsignacion = DateTime.SpecifyKind(asignacion.FechaAsignacion, DateTimeKind.Local);
                asignacion.FechaDesasignacion = DateTime.SpecifyKind(asignacion.FechaDesasignacion, DateTimeKind.Local); 

                if (asignacion.Id == 0)
                {
                    // ✅ Nueva asignación (Id = 0) - solo si tiene consultor asignado
                    if (asignacion.IdConsultor.HasValue && asignacion.IdConsultor.Value > 0)
                    {
                        agregados.Add(asignacion);
                    }
                }
                else
                {
                    // ✅ Asignación existente (Id > 0) - solo si tiene consultor asignado
                    if (asignacion.IdConsultor.HasValue && asignacion.IdConsultor.Value > 0)
                    {
                        modificados.Add(asignacion);
                    }
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
            return (modificados, agregados, tareasModificadas, tareasAgregadas);
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
            if (updateDto.IdSubTipoTicket.HasValue && updateDto.IdSubTipoTicket > 0) existingTicket.IdSubTipoTicket = updateDto.IdSubTipoTicket;
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

        // ── Paginación server-side ───────────────────────────────────────

        public async Task<PagedResultDto<TicketListItemDto>> GetPagedByUserRolAsync(
            int idUser, string codRol,
            int page, int pageSize,
            List<int>? estadoIds = null,
            string? globalFilter = null,
            string? sortField = null, string? sortOrder = null,
            string? codTicket = null,
            string? codTicketInterno = null,
            string? titulo = null,
            string? empresa = null,
            string? gestor = null,
            string? prioridad = null,
            string? estado = null,
            string? nombreConsultor = null)
        {
            Console.WriteLine($"[DEBUG] GetPagedByUserRolAsync Start - User: {idUser}, Role: {codRol}, Page: {page}, PageSize: {pageSize}");
            Console.WriteLine($"[DEBUG] Params - CodTicket: '{codTicket}', CodTicketInterno: '{codTicketInterno}', GlobalFilter: '{globalFilter}'");
            if (estadoIds != null) Console.WriteLine($"[DEBUG] EstadoIds received: {string.Join(",", estadoIds)}");

            // 1) Obtener el IQueryable base según el rol
            IQueryable<Ticket> query;

            if (codRol == AppConstants.Roles.GestorCuenta)
            {
                Console.WriteLine("[DEBUG] Role detected: GestorCuenta");
                var gestorDto = await _gestorService.GetByIdUserAsync(idUser);
                query = _ticketRepository.GetQueryableByGestor(gestorDto.Id);
            }
            else if (codRol == AppConstants.Roles.GestorConsultoria)
            {
                Console.WriteLine("[DEBUG] Role detected: GestorConsultoria");
                var gestorDto = await _gestorService.GetByIdUserAsync(idUser);
                query = _ticketRepository.GetQueryableByGestorConsultoria(gestorDto.Id);
                // GestorConsultoria filtra por FrenteSubFrentes
                query = query.Where(t => t.FrenteSubFrentes.Any(fsf => fsf.Activo));
            }
            else if (codRol == AppConstants.Roles.Consultor)
            {
                Console.WriteLine("[DEBUG] Role detected: Consultor");
                var consultorDto = await _consultorService.GetByIdUserAsync(idUser);
                query = _ticketRepository.GetQueryableByConsultor(consultorDto.Id);
            }
            else if (codRol == AppConstants.Roles.Empresa)
            {
                Console.WriteLine("[DEBUG] Role detected: Empresa");
                var empresaDto = await _empresaService.GetByIdUserAsync(idUser);
                query = _ticketRepository.GetQueryableByEmpresa(Convert.ToInt32(empresaDto.Id));
            }
            else if (codRol == AppConstants.Roles.Admin)
            {
                Console.WriteLine("[DEBUG] Role detected: ADMIN");
                var userDto = await _authService.GetByIdAsync(idUser);
                Console.WriteLine($"[DEBUG] Admin IdSocio: {userDto?.Socio?.Id}");
                query = _ticketRepository.GetQueryableBySocio(Convert.ToInt32(userDto.Socio.Id));
            }
            else
            {
                Console.WriteLine($"[DEBUG] Role detected: OTHER ({codRol}) - Using GetQueryableAll");
                query = _ticketRepository.GetQueryableAll();
            }

            Console.WriteLine($"[DEBUG] Base Query Count: {await query.CountAsync()}");

            // 2) Filtro por estados (multiselect)
            if (estadoIds != null && estadoIds.Count > 0)
            {
                bool isSpecificSearch = !string.IsNullOrWhiteSpace(codTicket) || !string.IsNullOrWhiteSpace(codTicketInterno);
                
                if (isSpecificSearch)
                {
                    Console.WriteLine("[DEBUG] Specific Code Search detected - Bypassing status filter for all roles.");
                }
                else
                {
                    query = query.Where(t => estadoIds.Contains(t.IdEstadoTicket));
                    Console.WriteLine($"[DEBUG] After estadoIds filter: {await query.CountAsync()}");
                }
            }

            // 3) Filtro global (búsqueda de texto)
            if (!string.IsNullOrWhiteSpace(globalFilter))
            {
                query = query.Where(t =>
                    t.CodTicket.Contains(globalFilter) ||
                    t.CodTicketInterno.Contains(globalFilter) ||
                    t.Titulo.Contains(globalFilter) ||
                    (t.Empresa != null && t.Empresa.RazonSocial.Contains(globalFilter))
                );
                Console.WriteLine($"[DEBUG] After globalFilter '{globalFilter}': {await query.CountAsync()}");
            }

            // ── NUEVOS Filtros por columna ──────────────────────────────────
            if (!string.IsNullOrWhiteSpace(codTicket)) {
                var search = codTicket.Trim().ToLower();
                query = query.Where(t => t.CodTicket.ToLower().Contains(search));
                Console.WriteLine($"[DEBUG] After codTicket filter '{search}': {await query.CountAsync()}");
            }

            if (!string.IsNullOrWhiteSpace(codTicketInterno)) {
                var search = codTicketInterno.Trim().ToLower();
                query = query.Where(t => t.CodTicketInterno.ToLower().Contains(search));
                Console.WriteLine($"[DEBUG] After codTicketInterno filter '{search}': {await query.CountAsync()}");
            }

            if (!string.IsNullOrWhiteSpace(titulo)) {
                var search = titulo.Trim().ToLower();
                query = query.Where(t => t.Titulo.ToLower().Contains(search));
                Console.WriteLine($"[DEBUG] After titulo filter '{search}': {await query.CountAsync()}");
            }

            if (!string.IsNullOrWhiteSpace(empresa))
                query = query.Where(t => t.Empresa != null && t.Empresa.RazonSocial.Contains(empresa));

            if (!string.IsNullOrWhiteSpace(gestor))
            {
                var g = gestor.Trim().ToLower();
                query = query.Where(t =>
                    t.Empresa != null &&
                    t.Empresa.Gestor != null &&
                    t.Empresa.Gestor.Persona != null &&
                    (t.Empresa.Gestor.Persona.Nombres.ToLower().Contains(g) ||
                     t.Empresa.Gestor.Persona.ApellidoPaterno.ToLower().Contains(g)));
            }

            // Para Prioridad y Estado (input texto), buscamos IDs que coincidan con el nombre
            // Necesitamos los parámetros cargados
            if (!string.IsNullOrWhiteSpace(prioridad) || !string.IsNullOrWhiteSpace(estado))
            {
                await InicializarDatosAsync(); // Asegura carga de _listaPrioridades, _listaEstados

                if (!string.IsNullOrWhiteSpace(prioridad))
                {
                    var idsPrioridad = _listaPrioridades?
                        .Where(p => p.Nombre != null && p.Nombre.Contains(prioridad))
                        .Select(p => p.Id)
                        .ToList();

                    if (idsPrioridad != null && idsPrioridad.Count > 0)
                        query = query.Where(t => idsPrioridad.Contains(t.IdPrioridad));
                    else
                        query = query.Where(t => false);
                }

                if (!string.IsNullOrWhiteSpace(estado))
                {
                    var idsEstado = _listaEstados?
                        .Where(e => e.Nombre != null && e.Nombre.Contains(estado))
                        .Select(e => e.Id)
                        .ToList();

                    if (idsEstado != null && idsEstado.Count > 0)
                        query = query.Where(t => idsEstado.Contains(t.IdEstadoTicket));
                    else
                        query = query.Where(t => false);
                }
            }
            // Filtro por nombre de consultor (busca en las asignaciones activas)
            Console.WriteLine($"[DEBUG-SERVICE] nombreConsultor parameter value: '{nombreConsultor}'");
            if (!string.IsNullOrWhiteSpace(nombreConsultor))
            {
                var nc = nombreConsultor.Trim().ToLower();
                Console.WriteLine($"[DEBUG-SERVICE] Applying nombreConsultor filter with query: '{nc}'");
                query = query.Where(t =>
                    t.ConsultorAsignaciones.Any(ca =>
                        ca.Activo &&
                        ca.Consultor != null &&
                        ca.Consultor.Persona != null &&
                        (ca.Consultor.Persona.Nombres.ToLower().Contains(nc) ||
                         ca.Consultor.Persona.ApellidoPaterno.ToLower().Contains(nc) ||
                         ca.Consultor.Persona.ApellidoMaterno.ToLower().Contains(nc))));
            }
            // ────────────────────────────────────────────────────────────────

            // 4) Contar total ANTES de paginar
            var totalRecords = await query.CountAsync();
            Console.WriteLine($"[DEBUG] Total Records Final: {totalRecords}");

            // 5) Ordenamiento
            var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase)
                      || string.Equals(sortOrder, "-1", StringComparison.OrdinalIgnoreCase);

            query = (sortField?.ToLower()) switch
            {
                "codticket" => isDesc ? query.OrderByDescending(t => t.CodTicket) : query.OrderBy(t => t.CodTicket),
                "codticketinterno" => isDesc ? query.OrderByDescending(t => t.CodTicketInterno) : query.OrderBy(t => t.CodTicketInterno),
                "titulo" => isDesc ? query.OrderByDescending(t => t.Titulo) : query.OrderBy(t => t.Titulo),
                "fechasolicitud" => isDesc ? query.OrderByDescending(t => t.FechaSolicitud) : query.OrderBy(t => t.FechaSolicitud),
                "idestadoticket" or "estadonombre" => isDesc ? query.OrderByDescending(t => t.IdEstadoTicket) : query.OrderBy(t => t.IdEstadoTicket),
                "idprioridad" or "prioridadnombre" => isDesc ? query.OrderByDescending(t => t.IdPrioridad) : query.OrderBy(t => t.IdPrioridad),
                "empresa.razonsocial" or "empresarazonsocial" => isDesc
                    ? query.OrderByDescending(t => t.Empresa != null ? t.Empresa.RazonSocial : "")
                    : query.OrderBy(t => t.Empresa != null ? t.Empresa.RazonSocial : ""),
                _ => query.OrderByDescending(t => t.FechaCreacion ?? DateTime.MinValue)
            };

            // 6) Paginación
            var tickets = await query
                .Include(t => t.ConsultorAsignaciones.Where(ca => ca.Activo))
                    .ThenInclude(ca => ca.Consultor)
                        .ThenInclude(c => c.Persona)
                .Include(t => t.FrenteSubFrentes.Where(fsf => fsf.Activo))
                .Skip(page * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            // 7) Cargar parámetros para nombres de estado y prioridad
            await InicializarDatosAsync();
            var estados = _listaEstados?.ToList() ?? new List<Parametro>();
            var prioridades = _listaPrioridades?.ToList() ?? new List<Parametro>();

            // Obtener planificaciones para calcular horas planificadas de forma agregada
            var frenteIds = tickets
                .Where(t => t.FrenteSubFrentes != null)
                .SelectMany(t => t.FrenteSubFrentes)
                .Where(fsf => fsf.Activo)
                .Select(fsf => fsf.Id)
                .Distinct()
                .ToList();

            var planningEntities = frenteIds.Any()
                ? await _consultorAsignacionRepository.GetPlanificacionesByFrenteIdsAsync(frenteIds)
                : new List<DetallePlanificacionConsultor>();

            var frenteToTicketMap = tickets
                .Where(t => t.FrenteSubFrentes != null)
                .SelectMany(t => t.FrenteSubFrentes)
                .Where(fsf => fsf.Activo)
                .ToDictionary(fsf => fsf.Id, fsf => fsf.IdTicket);

            var planHorasByTicket = planningEntities
                .Where(p => p.Activo && frenteToTicketMap.ContainsKey(p.IdTicketFrenteSubFrente))
                .GroupBy(p => frenteToTicketMap[p.IdTicketFrenteSubFrente])
                .ToDictionary(g => g.Key, g => g.Sum(p => (int)p.Horas));

            // 8) Mapear a DTO ligero
            var items = tickets.Select(t =>
            {
                var horasTrabajadas = t.ConsultorAsignaciones
                    .Where(ca => ca.Activo)
                    .SelectMany(ca => ca.DetalleTareasConsultor.Where(dt => dt.Activo))
                    .Sum(dt => (int?)dt.Horas) ?? 0;

                var horasPlanificadas = planHorasByTicket.TryGetValue(t.Id, out int hPlan) ? hPlan : 0;

                return new TicketListItemDto
                {
                    Id = t.Id,
                    CodTicket = t.CodTicket,
                    CodTicketInterno = t.CodTicketInterno,
                    Titulo = t.Titulo,
                    FechaSolicitud = t.FechaSolicitud,
                    IdEstadoTicket = t.IdEstadoTicket,
                    EstadoNombre = estados.FirstOrDefault(e => e.Id == t.IdEstadoTicket)?.Nombre ?? "Sin estado",
                    IdPrioridad = t.IdPrioridad,
                    PrioridadNombre = prioridades.FirstOrDefault(p => p.Id == t.IdPrioridad)?.Nombre ?? "Sin prioridad",
                    IdEmpresa = t.IdEmpresa,
                    EmpresaRazonSocial = t.Empresa?.RazonSocial,
                    NombreGestor = t.Empresa?.Gestor?.Persona != null
                        ? $"{t.Empresa.Gestor.Persona.Nombres} {t.Empresa.Gestor.Persona.ApellidoPaterno}"
                        : null,
                    NombreConsultores = string.Join("/",
                        t.ConsultorAsignaciones
                            .Where(ca => ca.Activo && ca.Consultor?.Persona != null)
                            .Select(ca => $"{ca.Consultor.Persona.Nombres} {ca.Consultor.Persona.ApellidoPaterno} {ca.Consultor.Persona.ApellidoMaterno}".Trim())
                            .Distinct()),
                    HorasTrabajadas = horasTrabajadas,
                    HorasPlanificadas = horasPlanificadas,
                    FechaCreacion = t.FechaCreacion
                };
            }).ToList();

            return new PagedResultDto<TicketListItemDto>
            {
                Items = items,
                TotalRecords = totalRecords,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<string>> GetAllCodTicketInternosAsync()
        {
            return await _ticketRepository.GetAllCodTicketInternosAsync();
        }

        private async Task PopulatePlaceholderAssignmentsAsync(IEnumerable<TicketDto> dtos)
        {
            if (dtos == null || !dtos.Any()) return;

            // 1. Collect all database IDs from active FrenteSubFrentes of all dtos
            var frenteIds = dtos
                .Where(d => d.FrenteSubFrentes != null)
                .SelectMany(d => d.FrenteSubFrentes)
                .Where(fsf => fsf.Activo)
                .Select(fsf => fsf.Id)
                .Distinct()
                .ToList();

            if (!frenteIds.Any()) return;

            // 2. Fetch those DetallePlanificacionConsultor records by Frente IDs
            var planningEntities = await _consultorAsignacionRepository.GetPlanificacionesByFrenteIdsAsync(frenteIds);
            var planningDtos = _mapper.Map<List<DetallePlanificacionConsultorDto>>(planningEntities);
            
            // Group planning records by IdTicketFrenteSubFrente
            var planningByFrenteMap = planningDtos
                .GroupBy(p => p.IdTicketFrenteSubFrente)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 3. For each TicketDto, reconstruct placeholder assignments if needed, and map planning to real assignments
            foreach (var ticketDto in dtos)
            {
                if (ticketDto.FrenteSubFrentes == null) continue;
                if (ticketDto.ConsultorAsignaciones == null)
                {
                    ticketDto.ConsultorAsignaciones = new List<TicketConsultorAsignacionDto>();
                }

                foreach (var frenteSubFrente in ticketDto.FrenteSubFrentes.Where(fsf => fsf.Activo))
                {
                    if (planningByFrenteMap.TryGetValue(frenteSubFrente.Id, out var planningDtosForFrente) && planningDtosForFrente.Any())
                    {
                        // Buscar asignaciones reales para este subfrente
                        var realAsignaciones = ticketDto.ConsultorAsignaciones
                            .Where(ca => ca.Activo && ca.IdSubFrente == frenteSubFrente.IdSubFrente && ca.IdConsultor.HasValue && ca.IdConsultor.Value > 0)
                            .ToList();

                        if (realAsignaciones.Any())
                        {
                            foreach (var realAsig in realAsignaciones)
                            {
                                if (realAsig.DetallePlanificacionConsultor == null)
                                {
                                    realAsig.DetallePlanificacionConsultor = new List<DetallePlanificacionConsultorDto>();
                                }
                                foreach (var planningDto in planningDtosForFrente)
                                {
                                    if (!realAsig.DetallePlanificacionConsultor.Any(dp => dp.Id == planningDto.Id))
                                    {
                                        realAsig.DetallePlanificacionConsultor.Add(planningDto);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Si no hay asignación real, buscar o crear placeholder
                            var placeholderAsig = ticketDto.ConsultorAsignaciones
                                .FirstOrDefault(ca => ca.Activo && ca.IdSubFrente == frenteSubFrente.IdSubFrente && (ca.IdConsultor == null || ca.IdConsultor == 0));

                            if (placeholderAsig == null)
                            {
                                placeholderAsig = new TicketConsultorAsignacionDto
                                {
                                    Id = 0,
                                    IdTicket = ticketDto.Id,
                                    IdSubFrente = frenteSubFrente.IdSubFrente,
                                    IdFrente = frenteSubFrente.IdFrente,
                                    IdTicketFrenteSubFrente = frenteSubFrente.Id,
                                    IdConsultor = 0,
                                    IdTipoActividad = 25,
                                    FechaAsignacion = frenteSubFrente.FechaInicio,
                                    FechaDesasignacion = frenteSubFrente.FechaFin,
                                    Activo = true,
                                    DetalleTareasConsultor = new List<DetalleTareasConsultorDto>(),
                                    DetallePlanificacionConsultor = new List<DetallePlanificacionConsultorDto>(),
                                    EsPlaceholder = true
                                };
                                ticketDto.ConsultorAsignaciones.Add(placeholderAsig);
                            }
                            else if (placeholderAsig.IdTicketFrenteSubFrente == null || placeholderAsig.IdTicketFrenteSubFrente == 0)
                            {
                                placeholderAsig.IdTicketFrenteSubFrente = frenteSubFrente.Id;
                            }

                            if (placeholderAsig.DetallePlanificacionConsultor == null)
                            {
                                placeholderAsig.DetallePlanificacionConsultor = new List<DetallePlanificacionConsultorDto>();
                            }
                            foreach (var planningDto in planningDtosForFrente)
                            {
                                if (!placeholderAsig.DetallePlanificacionConsultor.Any(dp => dp.Id == planningDto.Id))
                                {
                                    placeholderAsig.DetallePlanificacionConsultor.Add(planningDto);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
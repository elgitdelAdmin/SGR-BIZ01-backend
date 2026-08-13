using AutoMapper;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Constants;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
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
        private readonly ICurrentUserService _currentUserService;

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
            IServiceProvider provider,
            ICurrentUserService currentUserService
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
            _currentUserService = currentUserService;
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
            await CargarPlanificacionesEnAsignacionesAsync(new List<TicketDto> { dto });
            return dto;
        }

        public async Task<TicketDto?> GetByCodTicketAsync(string codTicket)
        {
            var ticket = await _ticketRepository.GetByCodTicketAsync(codTicket);
            if (ticket == null) return null;
            var dto = _mapper.Map<TicketDto>(ticket);
            await CargarPlanificacionesEnAsignacionesAsync(new List<TicketDto> { dto });
            return dto;
        }

        public async Task<IEnumerable<TicketDto>> GetByEmpresaAsync(int idEmpresa)
        {
            // 1. Preguntamos de forma invisible quién está llamando (Postman o la IA)
            var rolDelUsuario = _currentUserService.CodRol;
            var sociosDelUsuario = _currentUserService.SociosIds; // Lista de ID de Socios
            // 2. Aplicamos tus Reglas de Negocio Duras
            if (rolDelUsuario == AppConstants.Roles.Consultor)
            {
                return new List<TicketDto>(); 
            }

            var tickets = await _ticketRepository.GetByEmpresaAsync(idEmpresa);
            var dtos = _mapper.Map<IEnumerable<TicketDto>>(tickets).ToList();
            await CargarPlanificacionesEnAsignacionesAsync(dtos);
            return dtos;
        }

        public async Task<IEnumerable<TicketDto>> GetByEstadoAsync(int idEstado)
        {
            var tickets = await _ticketRepository.GetByEstadoAsync(idEstado);
            var dtos = _mapper.Map<IEnumerable<TicketDto>>(tickets).ToList();
            await CargarPlanificacionesEnAsignacionesAsync(dtos);
            return dtos;
        }
        public async Task<TicketDto?> GetByIdSocioNumContribuyenteEmpAsync(int idSocio, string numContribuyenteEmp)
        {
            var ticket = await _ticketRepository.GetByIdSocioNumContribuyenteEmpAsync(idSocio, numContribuyenteEmp);
            if (ticket == null) return null;
            var dto = _mapper.Map<TicketDto>(ticket);
            await CargarPlanificacionesEnAsignacionesAsync(new List<TicketDto> { dto });
            return dto;
        }
        public async Task<TicketDto?> GetByNumContribuyenteSocioEmpAsync(string numContribuyenteSocio, string numContribuyenteEmp)
        {
            var ticket = await _ticketRepository.GetByNumContribuyenteSocioEmpAsync(numContribuyenteSocio, numContribuyenteEmp);
            if (ticket == null) return null;
            var dto = _mapper.Map<TicketDto>(ticket);
            await CargarPlanificacionesEnAsignacionesAsync(new List<TicketDto> { dto });
            return dto;
        }
        public async Task<IEnumerable<TicketDto>> GetByIdUserIdRolAsync(int idUser, string codRol, int? idSocio = null)
        {
            List<TicketDto> listadoTickets = new List<TicketDto>();

            if (codRol == AppConstants.Roles.GestorCuenta)
            {
                GestorDto gestorDto = await _gestorService.GetByIdUserAsync(idUser);
                var tickets = await _ticketRepository.GetByGestorAsync(gestorDto.Id, idSocio);
                listadoTickets = _mapper.Map<IEnumerable<TicketDto>>(tickets).ToList();
                await CargarPlanificacionesEnAsignacionesAsync(listadoTickets);
                listadoTickets = listadoTickets
                .Select(t =>
                {
                    t.HorasTrabajadas = t.ConsultorAsignaciones
                        .SelectMany(ca => ca.DetalleTareasConsultor)
                        .Sum(dt => Math.Round(dt.Horas * 60m)) / 60m;
                    t.HorasPlanificadas = t.FrenteSubFrentes
                      .SelectMany(fsf => fsf.DetallePlanificacionConsultor)
                      .Sum(dt => Math.Round(dt.Horas * 60m)) / 60m;
                    return t;
                })
                .ToList();
            }
            else if (codRol == AppConstants.Roles.GestorConsultoria)
            {
                GestorDto gestorDto = await _gestorService.GetByIdUserAsync(idUser);
                var tickets = await _ticketRepository.GetByGestorConsultoriaAsync(gestorDto.Id, idSocio);
                listadoTickets = _mapper.Map<IEnumerable<TicketDto>>(tickets)
               .Where(t => t.FrenteSubFrentes != null && t.FrenteSubFrentes.Count > 0)
               .ToList();
                await CargarPlanificacionesEnAsignacionesAsync(listadoTickets);
                listadoTickets = listadoTickets
               .Select(t =>
               {
                   t.HorasTrabajadas = t.ConsultorAsignaciones
                       .SelectMany(ca => ca.DetalleTareasConsultor)
                       .Sum(dt => Math.Round(dt.Horas * 60m)) / 60m;
                   t.HorasPlanificadas = t.FrenteSubFrentes
                     .SelectMany(fsf => fsf.DetallePlanificacionConsultor)
                     .Sum(dt => Math.Round(dt.Horas * 60m)) / 60m;
                   return t;
               })
               .ToList();
            }
            else if (codRol == AppConstants.Roles.Consultor)
            {
                ConsultorDto consultorDto = await _consultorService.GetByIdUserAsync(idUser);
                var tickets = await _ticketRepository.GetByConsultorAsync(consultorDto.Id, idSocio);
                listadoTickets = _mapper.Map<IEnumerable<TicketDto>>(tickets).ToList();
                await CargarPlanificacionesEnAsignacionesAsync(listadoTickets);
                listadoTickets = listadoTickets
                  .Select(t =>
                  {
                      // Sumar solo las horas del consultor específico
                      t.HorasTrabajadas = t.ConsultorAsignaciones
                          .Where(ca => ca.IdConsultor == consultorDto.Id)
                          .SelectMany(ca => ca.DetalleTareasConsultor)
                          .Sum(dt => Math.Round(dt.Horas * 60m)) / 60m;

                      t.HorasPlanificadas = t.FrenteSubFrentes
                        .SelectMany(fsf => fsf.DetallePlanificacionConsultor)
                        // Aqui idealmente deberiamos filtrar por consultor, pero DetallePlanificacion no tiene IdConsultor en el DTO sin pasar por la asignación. Como el UI actual asocia la planificacíon al frente entero:
                        .Sum(dt => Math.Round(dt.Horas * 60m)) / 60m;
                      return t;
                  })
                  .ToList();
            }
            else if (codRol == AppConstants.Roles.Empresa)
            {
                EmpresaDto empresaDto = await _empresaService.GetByIdUserAsync(idUser);
                var tickets = await _ticketRepository.GetByEmpresaAsync(Convert.ToInt32(empresaDto.Id));
                listadoTickets = _mapper.Map<IEnumerable<TicketDto>>(tickets).ToList();
                await CargarPlanificacionesEnAsignacionesAsync(listadoTickets);
                listadoTickets = listadoTickets
               .Select(t =>
               {
                   t.HorasTrabajadas = t.ConsultorAsignaciones
                       .SelectMany(ca => ca.DetalleTareasConsultor)
                       .Sum(dt => Math.Round(dt.Horas * 60m)) / 60m;
                   t.HorasPlanificadas = t.FrenteSubFrentes
                      .SelectMany(fsf => fsf.DetallePlanificacionConsultor)
                      .Sum(dt => Math.Round(dt.Horas * 60m)) / 60m;
                   return t;
               })
               .ToList();
            }
            else if (codRol == AppConstants.Roles.Admin)
            {
                int socioIdToUse = idSocio ?? 0;
                var tickets = await _ticketRepository.GetBySocioAsync(socioIdToUse);
                listadoTickets = _mapper.Map<IEnumerable<TicketDto>>(tickets).ToList();
                await CargarPlanificacionesEnAsignacionesAsync(listadoTickets);
                listadoTickets = listadoTickets
               .Select(t =>
               {
                   t.HorasTrabajadas = t.ConsultorAsignaciones
                       .SelectMany(ca => ca.DetalleTareasConsultor)
                       .Sum(dt => Math.Round(dt.Horas * 60m)) / 60m;
                   t.HorasPlanificadas = t.FrenteSubFrentes
                      .SelectMany(fsf => fsf.DetallePlanificacionConsultor)
                      .Sum(dt => Math.Round(dt.Horas * 60m)) / 60m;
                   return t;
               })
               .ToList();
            }
            else
            {
                var tickets = await _ticketRepository.GetAllAsync();
                listadoTickets = _mapper.Map<IEnumerable<TicketDto>>(tickets).ToList();
                await CargarPlanificacionesEnAsignacionesAsync(listadoTickets);
                listadoTickets = listadoTickets
               .Select(t =>
               {
                   t.HorasTrabajadas = t.ConsultorAsignaciones
                       .SelectMany(ca => ca.DetalleTareasConsultor)
                       .Sum(dt => Math.Round(dt.Horas * 60m)) / 60m;
                   t.HorasPlanificadas = t.FrenteSubFrentes
                      .SelectMany(fsf => fsf.DetallePlanificacionConsultor)
                      .Sum(dt => Math.Round(dt.Horas * 60m)) / 60m;
                   return t;
               })
               .ToList();
            }
            return listadoTickets;
        }
        //public async Task<IEnumerable<TicketDto>> GetTicketsWithFiltersAsync(int? idEmpresa = null, int? idEstado = null, bool? urgente = null)
        //{
        //    var tickets = await _ticketRepository.GetTicketsWithFiltersAsync(idEmpresa, idEstado, urgente);
        //    var dtos = _mapper.Map<IEnumerable<TicketDto>>(tickets).ToList();
        //    await CargarPlanificacionesEnAsignacionesAsync(dtos);
        //    return dtos;
        //}
        public async Task<string> GenerarCodigoTicketAsync(int idTipoTicket)
        {
            var tipoTicket = _listaTipoTicket.FirstOrDefault(t => t.Id == idTipoTicket);
            if (tipoTicket == null)
            {
                throw new InvalidOperationException($"No se encontró un tipo de ticket con el Id {idTipoTicket}");
            }

            int maxId = await _ticketRepository.GetMaxIdAsync();
            int nextId = maxId + 1;

            string fechaHora = DateTime.Now.ToString("yyyyMMdd");
            return $"{tipoTicket.Codigo}-{fechaHora}-{nextId}";
        }
        public async Task<TicketDto> CreateAsync(TicketInsertDto insertDto)
        {
            await InicializarDatosAsync();

            try
            {

                // 1. Obtener empresa
                var empresa = await _empresaRepository.GetByIdAsync(insertDto.IdEmpresa);

                if (empresa == null)
                {
                    throw new InvalidOperationException("Empresa no encontrada");
                }

                // 2. Crear ticket
                int idEstadoInicial = _listaEstados.First(x => x.Codigo == AppConstants.Estados.PENDIENTE_ATENCION).Id;
                string codTicket = await GenerarCodigoTicketAsync(insertDto.IdTipoTicket);

                var gestoresEmpresaActivos = empresa.EmpresaGestores != null
                    ? empresa.EmpresaGestores.Where(eg => eg.Activo).Select(eg => eg.IdGestor).ToList()
                    : new List<int>();

                if (empresa.IdGestor.HasValue && !gestoresEmpresaActivos.Contains(empresa.IdGestor.Value))
                {
                    gestoresEmpresaActivos.Add(empresa.IdGestor.Value);
                }

                int idGestorPrincipalEmpresa = empresa.EmpresaGestores?.FirstOrDefault(eg => eg.Activo && eg.EsPrincipal)?.IdGestor 
                                            ?? empresa.IdGestor 
                                            ?? 0;

                var ticket = Ticket.Crear(
                    codTicket: codTicket,
                    titulo: insertDto.Titulo,
                    fechaSolicitud: insertDto.FechaSolicitud,
                    idTipoTicket: insertDto.IdTipoTicket,
                    idSubTipoTicket: insertDto.IdSubTipoTicket,
                    idEstadoInicial: idEstadoInicial,
                    idEmpresa: insertDto.IdEmpresa,
                    idUsuarioResponsableCliente: (int)insertDto.IdUsuarioResponsableCliente,
                    idPrioridad: insertDto.IdPrioridad,
                    idGestorConsultoria: insertDto.IdGestorConsultoria,
                    descripcion: insertDto.Descripcion,
                    repositorios: insertDto.Repositorios,
                    usuarioCreacion: insertDto.UsuarioCreacion,
                    idGestorPrincipalEmpresa: idGestorPrincipalEmpresa,
                    gestoresEmpresaActivos: gestoresEmpresaActivos,
                    idGestoresSecundarios: insertDto.IdGestoresSecundarios ?? new List<int>(),
                    codTicketInterno: insertDto.CodTicketInterno,
                    codReqSgrCsti: insertDto.CodReqSgrCsti,
                    idReqSgrCsti: insertDto.IdReqSgrCsti,
                    esCargaMasiva: insertDto.EsCargaMasiva
                );

                var createdTicket = await _ticketRepository.CreateAsync(ticket);

                // 4. Todas las operaciones secundarias de forma SECUENCIAL
                await CreateInitialHistorialAsync(createdTicket.Id, insertDto.IdEstadoTicket);

                // 5. Notificaciones (SECUENCIAL - NO en background)
                await CrearNotificacionesAsignacionTicket(
                    createdTicket.Id,
                    ticket.CodTicket,
                    (int)empresa.IdUser,
                    (int)empresa.IdGestor,
                    (int)insertDto.IdGestorConsultoria,
                    []
                );

                return _mapper.Map<TicketDto>(createdTicket);
            }
            catch (Exception ex)
            {
                // Log de error también secuencial
                await File.AppendAllTextAsync(_rutaLog, ex.ToString());
                throw;
            }
        }

        public async Task<TicketDto> CreateRapidoAsync(TicketCreacionRapidaDto dto)
        {
            await InicializarDatosAsync();

            try
            {
                // 1. Obtener empresa
                var empresa = await _empresaRepository.GetByIdAsync(dto.IdEmpresa);

                if (empresa == null)
                {
                    throw new InvalidOperationException("Empresa no encontrada");
                }

                // 2. Map Asignaciones and Frentes
                var frentes = _mapper.Map<List<TicketFrenteSubFrente>>(dto.FrenteSubFrentes ?? new List<TicketFrenteSubFrenteInsertDto>());
                var asignaciones = _mapper.Map<List<TicketConsultorAsignacion>>(dto.ConsultorAsignaciones ?? new List<TicketConsultorAsignacionInsertDto>());

                // 3. Crear ticket
                dto.IdTipoTicket = _listaTipoTicket.First(x => x.Codigo == AppConstants.TipoTicket.MesaDeAyuda).Id;

                int idEstadoInicial = _listaEstados.First(x => x.Codigo == AppConstants.Estados.EN_EJECUCION).Id;
                string codTicket = await GenerarCodigoTicketAsync(dto.IdTipoTicket);

                var gestoresEmpresaActivos = empresa.EmpresaGestores != null
                    ? empresa.EmpresaGestores.Where(eg => eg.Activo).Select(eg => eg.IdGestor).ToList()
                    : new List<int>();

                if (empresa.IdGestor.HasValue && !gestoresEmpresaActivos.Contains(empresa.IdGestor.Value))
                {
                    gestoresEmpresaActivos.Add(empresa.IdGestor.Value);
                }

                int idGestorPrincipalEmpresa = empresa.EmpresaGestores?.FirstOrDefault(eg => eg.Activo && eg.EsPrincipal)?.IdGestor 
                                            ?? empresa.IdGestor 
                                            ?? 0;

                var ticket = Ticket.CrearRapido(
                    codTicket: codTicket,
                    titulo: dto.Titulo,
                    fechaSolicitud: dto.FechaSolicitud,
                    idTipoTicket: dto.IdTipoTicket,
                    idSubTipoTicket: dto.IdSubTipoTicket,
                    idEstadoInicial: idEstadoInicial,
                    idEmpresa: dto.IdEmpresa,
                    idUsuarioResponsableCliente: (int)dto.IdUsuarioResponsableCliente,
                    idPrioridad: dto.IdPrioridad,
                    idGestorConsultoria: dto.IdGestorConsultoria,
                    descripcion: dto.Descripcion,
                    repositorios: dto.Repositorios,
                    usuarioCreacion: dto.UsuarioCreacion,
                    idGestorPrincipalEmpresa: idGestorPrincipalEmpresa,
                    gestoresEmpresaActivos: gestoresEmpresaActivos,
                    idGestoresSecundarios: dto.IdGestoresSecundarios ?? new List<int>(),
                    asignaciones: asignaciones,
                    frentesSubFrentes: frentes,
                    codTicketInterno: dto.CodTicketInterno,
                    codReqSgrCsti: dto.CodReqSgrCsti,
                    idReqSgrCsti: dto.IdReqSgrCsti,
                    esCargaMasiva: dto.EsCargaMasiva
                );

                var createdTicket = await _ticketRepository.CreateAsync(ticket);

                // 4. Todas las operaciones secundarias de forma SECUENCIAL
                await CreateInitialHistorialAsync(createdTicket.Id, dto.IdEstadoTicket);

                // 5. Notificaciones (SECUENCIAL - NO en background)
                var consultoresIds = asignaciones.Where(a => a.IdConsultor.HasValue).Select(a => a.IdConsultor.Value).ToArray();
                await CrearNotificacionesAsignacionTicket(
                    createdTicket.Id,
                    ticket.CodTicket,
                    (int)empresa.IdUser,
                    (int)empresa.IdGestor,
                    (int)dto.IdGestorConsultoria,
                    consultoresIds
                );

                return _mapper.Map<TicketDto>(createdTicket);
            }
            catch (Exception ex)
            {
                // Log de error también secuencial
                await File.AppendAllTextAsync(_rutaLog, ex.ToString());
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
            // Obtener ambos gestores en una sola consulta
            var gestores = (await _gestorService.GetByIdsAsync(new[] { idGestor, idGestorConsultoria })).ToDictionary(g => g.Id);

            // Recuperar cada uno (si existe en el diccionario)
            gestores.TryGetValue(idGestor, out var gestorDto);
            gestores.TryGetValue(idGestorConsultoria, out var gestorConsultoriaDto);


            var candidatos = new List<int> { idUserEmpresa, (int)gestorDto.IdUser, (int)gestorConsultoriaDto.IdUser };
            candidatos.AddRange(idsConsultores.Where(id => id > 0));

            // 2. Traer notificaciones ya existentes
            var existentes = await _notificacionTicketService.Value.GetNotificacionesByIdTicketIdUsersAsync(ticketId, candidatos.ToArray());

            // 🔑 Crear set único por (IdTicket, IdUser)
            var existentesSet = existentes.Select(n => (n.IdTicket, IdUser: (int)n.IdUser)).Distinct().ToHashSet();

            // 3. Filtrar solo los nuevos usuarios
            var nuevosUsuarios = candidatos.Where(id => !existentesSet.Contains((ticketId, id))).ToList();

            // 4. Crear notificaciones
            var lstNotificaciones = new List<CrearNotificacionDto>();
            string mensaje = $"El Ticket: {codTicket} ha sido asignado a usted.";

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

            // 5. Guardar en lote
            if (lstNotificaciones.Any())
            {
                await _notificacionTicketService.Value.AddRangeAsync(lstNotificaciones);
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



        public async Task<TicketDto> UpdateAsync(int id, TicketUpdateDto updateDto)
        {
            await InicializarDatosAsync();

                var lstNotificaciones = new List<CrearNotificacionDto>();

                var existingTicket = await _ticketRepository.GetByIdWithRelationsAsync(id);
                if (existingTicket == null)
                {
                    throw new KeyNotFoundException($"No se encontró el ticket con ID: {id}");
                }

                // Actualizar los campos del ticket principal
                UpdateTicketFields(existingTicket, updateDto);

                // Forzar el cambio de estado si viene en el DTO (y es distinto)
                if (updateDto.IdEstadoTicket > 0)
                {
                    existingTicket.CambiarEstado(updateDto.IdEstadoTicket, updateDto.UsuarioActualizacion);
                }

                bool huboCambiosFrentes = false;

                // 1️⃣ Validar y actualizar frentes y subfrentes usando EF Change Tracking
                if (updateDto.FrenteSubFrentes != null)
                {
                    var frentesNuevos = _mapper.Map<List<TicketFrenteSubFrente>>(updateDto.FrenteSubFrentes);
                    var (agregados, modificados) = existingTicket.ActualizarFrentes(frentesNuevos, updateDto.UsuarioActualizacion);
                    huboCambiosFrentes = agregados > 0 || modificados > 0;
                    
                    if (huboCambiosFrentes)
                    {
                        var gestorConsultoria = await _gestorService.GetByIdAsync((int)updateDto.IdGestorConsultoria);
                        
                        if (agregados > 0)
                        {
                            for (int i = 0; i < agregados; i++)
                            {
                                lstNotificaciones.Add(CrearNotificacion(id, (int)gestorConsultoria.IdUser, existingTicket.CodTicket, $"Se ha agregado una asignación al ticket {existingTicket.CodTicket}"));
                            }
                        }
                        if (modificados > 0)
                        {
                            for (int i = 0; i < modificados; i++)
                            {
                                lstNotificaciones.Add(CrearNotificacion(id, (int)gestorConsultoria.IdUser, existingTicket.CodTicket, $"Se ha modificado una asignación al ticket {existingTicket.CodTicket}"));
                            }
                        }
                    }
                }

                // 2️⃣ Validar y actualizar asignaciones de consultores
                var asignacionesNuevas = _mapper.Map<List<TicketConsultorAsignacion>>(updateDto.ConsultorAsignaciones ?? new());
                var nuevosIdsConsultores = existingTicket.ActualizarAsignaciones(asignacionesNuevas);

                // Vincular planificaciones con sus respectivas asignaciones de consultores
                existingTicket.VincularPlanificacionesConAsignaciones();

                // 3️⃣ Evaluar Transiciones Automáticas usando lógica de Dominio
                bool huboCambiosAsignaciones = nuevosIdsConsultores.Any();
                int idEstadoAtendido = _listaEstados.First(x => x.Codigo == AppConstants.Estados.ATENDIDO).Id;
                int idEstadoPendienteAsig = _listaEstados.First(x => x.Codigo == AppConstants.Estados.PENDIENTE_ASIGNACION).Id;
                int idEstadoAsignado = _listaEstados.First(x => x.Codigo == AppConstants.Estados.ASIGNADO).Id;
                int idEstadoPendienteAtencion = _listaEstados.First(x => x.Codigo == AppConstants.Estados.PENDIENTE_ATENCION).Id;

                existingTicket.EvaluarTransicionesAutomaticas(
                    huboCambiosAsignaciones, 
                    huboCambiosFrentes,
                    idEstadoAtendido, 
                    idEstadoPendienteAsig, 
                    idEstadoAsignado, 
                    idEstadoPendienteAtencion,
                    updateDto.UsuarioActualizacion
                );

                existingTicket.ActualizarRepositorios(updateDto.Repositorios);

                // 3.5️⃣ Sincronizar Gestores Secundarios
                if (updateDto.IdGestoresSecundarios != null && existingTicket.Empresa != null)
                {
                    int idGestorPrincipalAccion = existingTicket.Empresa.EmpresaGestores?.FirstOrDefault(eg => eg.Activo && eg.EsPrincipal)?.IdGestor 
                                                ?? existingTicket.Empresa.IdGestor 
                                                ?? 0;

                    var gestoresEmpresaActivos = existingTicket.Empresa.EmpresaGestores != null
                        ? existingTicket.Empresa.EmpresaGestores.Where(eg => eg.Activo).Select(eg => eg.IdGestor).ToList()
                        : new List<int>();

                    if (existingTicket.Empresa.IdGestor.HasValue && !gestoresEmpresaActivos.Contains(existingTicket.Empresa.IdGestor.Value))
                    {
                        gestoresEmpresaActivos.Add(existingTicket.Empresa.IdGestor.Value);
                    }

                    // Filtrar gestores secundarios que no pertenezcan ya a la empresa
                    var gestoresPuntualesPermitidos = updateDto.IdGestoresSecundarios
                        .Where(idG => !gestoresEmpresaActivos.Contains(idG))
                        .ToList();

                    existingTicket.ActualizarGestoresSecundarios(
                        gestoresPuntualesPermitidos,
                        idGestorPrincipalAccion,
                        updateDto.UsuarioActualizacion
                    );
                }

                // Guardar cambios del ticket principal
                await _ticketRepository.UpdateAsync(existingTicket);

                // 4️⃣ Enviar/Guardar notificaciones SOLO SI el ticket se guardó correctamente
                if (nuevosIdsConsultores.Any())
                {
                    await CrearNotificacionesAsignacionTicket(id, existingTicket.CodTicket, (int)existingTicket.Empresa.IdUser, (int)existingTicket.Empresa.IdGestor, (int)updateDto.IdGestorConsultoria, nuevosIdsConsultores.ToArray());
                }

                if (lstNotificaciones.Any())
                {
                    await _notificacionTicketService.Value.AddRangeAsync(lstNotificaciones);
                }

                // Retornar el objeto mapeado directamente desde la memoria (Ahorro de consulta SQL)
                var resultDto = _mapper.Map<TicketDto>(existingTicket);
                await CargarPlanificacionesEnAsignacionesAsync(new List<TicketDto> { resultDto });
                return resultDto;
        }

        public async Task<bool> DeleteAsync(int id)

        {
            var exists = await _ticketRepository.GetByIdAsync(id);
            if (exists == null) return false;

            return await _ticketRepository.DeleteAsync(id);
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
            if (updateDto.IdEmpresa > 0) existingTicket.IdEmpresa = updateDto.IdEmpresa;
            if (updateDto.IdUsuarioResponsableCliente > 0) existingTicket.IdUsuarioResponsableCliente = updateDto.IdUsuarioResponsableCliente;
            if (updateDto.IdPrioridad > 0) existingTicket.IdPrioridad = updateDto.IdPrioridad;
            if (updateDto.IdGestorConsultoria > 0) existingTicket.IdGestorConsultoria = updateDto.IdGestorConsultoria;
            if (!string.IsNullOrEmpty(updateDto.Descripcion)) existingTicket.Descripcion = updateDto.Descripcion;
            if (!string.IsNullOrEmpty(updateDto.UsuarioActualizacion)) existingTicket.UsuarioActualizacion = updateDto.UsuarioActualizacion;
        }


        public async Task<FileDownloadDto> DescargarArchivoAsync(int idTicket, int orden)
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

            var bytes = await File.ReadAllBytesAsync(fullPath);

            return new FileDownloadDto
            {
                Content = bytes,
                ContentType = "application/octet-stream", // El controlador puede refinar esto
                FileName = Path.GetFileName(fullPath)
            };
        }

        // ── Paginación server-side ───────────────────────────────────────

        public async Task<PagedResultDto<TicketListItemDto>> GetPagedByUserRolAsync(
            int idUser, string codRol,
            int? idSocio,
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
            string? nombreConsultor = null,
            string? tipoSubtipo = null)
        {
            Console.WriteLine($"[DEBUG] GetPagedByUserRolAsync Start - User: {idUser}, Role: {codRol}, Socio: {idSocio}, Page: {page}, PageSize: {pageSize}");
            Console.WriteLine($"[DEBUG] Params - CodTicket: '{codTicket}', CodTicketInterno: '{codTicketInterno}', GlobalFilter: '{globalFilter}'");
            if (estadoIds != null) Console.WriteLine($"[DEBUG] EstadoIds received: {string.Join(",", estadoIds)}");

            // 1) Obtener el IQueryable base según el rol
            IQueryable<Ticket> query;

            if (codRol == AppConstants.Roles.GestorCuenta)
            {
                Console.WriteLine("[DEBUG] Role detected: GestorCuenta");
                var gestorDto = await _gestorService.GetByIdUserAsync(idUser);
                query = _ticketRepository.GetQueryableByGestor(gestorDto.Id, idSocio);
            }
            else if (codRol == AppConstants.Roles.GestorConsultoria)
            {
                Console.WriteLine("[DEBUG] Role detected: GestorConsultoria");
                var gestorDto = await _gestorService.GetByIdUserAsync(idUser);
                query = _ticketRepository.GetQueryableByGestorConsultoria(gestorDto.Id, idSocio);
                // GestorConsultoria filtra por FrenteSubFrentes
                query = query.Where(t => t.FrenteSubFrentes.Any(fsf => fsf.Activo));
            }
            else if (codRol == AppConstants.Roles.Consultor)
            {
                Console.WriteLine("[DEBUG] Role detected: Consultor");
                var consultorDto = await _consultorService.GetByIdUserAsync(idUser);
                query = _ticketRepository.GetQueryableByConsultor(consultorDto.Id, idSocio);
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
                int socioIdToUse = idSocio ?? 0;
                Console.WriteLine($"[DEBUG] Admin IdSocio (from param): {socioIdToUse}");
                query = _ticketRepository.GetQueryableBySocio(socioIdToUse);
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
                var gf = globalFilter.Trim().ToLower();
                query = query.Where(t =>
                    (t.CodTicket != null && t.CodTicket.ToLower().Contains(gf)) ||
                    (t.CodTicketInterno != null && t.CodTicketInterno.ToLower().Contains(gf)) ||
                    (t.Titulo != null && t.Titulo.ToLower().Contains(gf)) ||
                    (t.Empresa != null && t.Empresa.RazonSocial != null && t.Empresa.RazonSocial.ToLower().Contains(gf))
                );
                Console.WriteLine($"[DEBUG] After globalFilter '{globalFilter}': {await query.CountAsync()}");
            }

            // ── NUEVOS Filtros por columna ──────────────────────────────────
            if (!string.IsNullOrWhiteSpace(codTicket)) {
                var search = codTicket.Trim().ToLower();
                query = query.Where(t => 
                    (t.CodTicket != null && t.CodTicket.ToLower().Contains(search)) ||
                    (t.CodTicketInterno != null && t.CodTicketInterno.ToLower().Contains(search))
                );
                Console.WriteLine($"[DEBUG] After codTicket filter '{search}': {await query.CountAsync()}");
            }

            if (!string.IsNullOrWhiteSpace(codTicketInterno)) {
                var search = codTicketInterno.Trim().ToLower();
                query = query.Where(t => t.CodTicketInterno != null && t.CodTicketInterno.ToLower().Contains(search));
                Console.WriteLine($"[DEBUG] After codTicketInterno filter '{search}': {await query.CountAsync()}");
            }

            if (!string.IsNullOrWhiteSpace(titulo)) {
                var search = titulo.Trim().ToLower();
                query = query.Where(t => t.Titulo != null && t.Titulo.ToLower().Contains(search));
                Console.WriteLine($"[DEBUG] After titulo filter '{search}': {await query.CountAsync()}");
            }

            if (!string.IsNullOrWhiteSpace(empresa))
            {
                var emp = empresa.Trim().ToLower();
                query = query.Where(t => t.Empresa != null && t.Empresa.RazonSocial != null && t.Empresa.RazonSocial.ToLower().Contains(emp));
            }

            if (!string.IsNullOrWhiteSpace(gestor))
            {
                var g = gestor.Trim().ToLower();
                query = query.Where(t =>
                    t.Empresa != null &&
                    t.Empresa.Gestor != null &&
                    t.Empresa.Gestor.Persona != null &&
                    ((t.Empresa.Gestor.Persona.Nombres != null && t.Empresa.Gestor.Persona.Nombres.ToLower().Contains(g)) ||
                     (t.Empresa.Gestor.Persona.ApellidoPaterno != null && t.Empresa.Gestor.Persona.ApellidoPaterno.ToLower().Contains(g))));
            }

            if (!string.IsNullOrWhiteSpace(tipoSubtipo))
            {
                await InicializarDatosAsync();
                var ts = tipoSubtipo.Trim().ToLower();
                var idsTipo = _listaTipoTicket?
                    .Where(t => t.Nombre != null && t.Nombre.ToLower().Contains(ts))
                    .Select(t => t.Id)
                    .ToList() ?? new List<int>();
                var idsSubTipo = _listaSubTipoTicket?
                    .Where(st => st.Nombre != null && st.Nombre.ToLower().Contains(ts))
                    .Select(st => st.Id)
                    .ToList() ?? new List<int>();

                query = query.Where(t => idsTipo.Contains(t.IdTipoTicket) || (t.IdSubTipoTicket.HasValue && idsSubTipo.Contains(t.IdSubTipoTicket.Value)));
            }

            // Para Prioridad y Estado (input texto), buscamos IDs que coincidan con el nombre
            // Necesitamos los parámetros cargados
            if (!string.IsNullOrWhiteSpace(prioridad) || !string.IsNullOrWhiteSpace(estado))
            {
                await InicializarDatosAsync(); // Asegura carga de _listaPrioridades, _listaEstados

                if (!string.IsNullOrWhiteSpace(prioridad))
                {
                    var prio = prioridad.Trim().ToLower();
                    var idsPrioridad = _listaPrioridades?
                        .Where(p => p.Nombre != null && p.Nombre.ToLower().Contains(prio))
                        .Select(p => p.Id)
                        .ToList();

                    if (idsPrioridad != null && idsPrioridad.Count > 0)
                        query = query.Where(t => idsPrioridad.Contains(t.IdPrioridad));
                    else
                        query = query.Where(t => false);
                }

                if (!string.IsNullOrWhiteSpace(estado))
                {
                    var est = estado.Trim().ToLower();
                    var idsEstado = _listaEstados?
                        .Where(e => e.Nombre != null && e.Nombre.ToLower().Contains(est))
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
                        ((ca.Consultor.Persona.Nombres != null && ca.Consultor.Persona.Nombres.ToLower().Contains(nc)) ||
                         (ca.Consultor.Persona.ApellidoPaterno != null && ca.Consultor.Persona.ApellidoPaterno.ToLower().Contains(nc)) ||
                         (ca.Consultor.Persona.ApellidoMaterno != null && ca.Consultor.Persona.ApellidoMaterno.ToLower().Contains(nc)))));
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

            // 7) Cargar parámetros para nombres de estado, prioridad, tipo y subtipo
            await InicializarDatosAsync();
            var estados = _listaEstados?.ToList() ?? new List<Parametro>();
            var prioridades = _listaPrioridades?.ToList() ?? new List<Parametro>();
            var tipos = _listaTipoTicket?.ToList() ?? new List<Parametro>();
            var subtipos = _listaSubTipoTicket?.ToList() ?? new List<Parametro>();

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
                .ToDictionary(g => g.Key, g => g.Sum(p => Math.Round(p.Horas * 60m)) / 60m);

            // 8) Mapear a DTO ligero
            var items = tickets.Select(t =>
            {
                var horasTrabajadas = t.ConsultorAsignaciones
                    .Where(ca => ca.Activo)
                    .SelectMany(ca => ca.DetalleTareasConsultor.Where(dt => dt.Activo))
                    .Sum(dt => Math.Round(dt.Horas * 60m)) / 60m;

                var horasPlanificadas = planHorasByTicket.TryGetValue(t.Id, out decimal hPlan) ? hPlan : 0m;

                var tipoNombre = tipos.FirstOrDefault(tp => tp.Id == t.IdTipoTicket)?.Nombre;
                var subtipoNombre = t.IdSubTipoTicket.HasValue
                    ? subtipos.FirstOrDefault(st => st.Id == t.IdSubTipoTicket.Value)?.Nombre
                    : null;
                var tipoSubtipoNombre = !string.IsNullOrEmpty(subtipoNombre)
                    ? $"{tipoNombre} / {subtipoNombre}"
                    : (tipoNombre ?? "");

                return new TicketListItemDto
                {
                    Id = t.Id,
                    CodTicket = t.CodTicket,
                    CodTicketInterno = t.CodTicketInterno,
                    Titulo = t.Titulo,
                    FechaSolicitud = t.FechaSolicitud,
                    IdTipoTicket = t.IdTipoTicket,
                    TipoTicketNombre = tipoNombre,
                    IdSubTipoTicket = t.IdSubTipoTicket,
                    SubTipoTicketNombre = subtipoNombre,
                    TipoSubtipoNombre = tipoSubtipoNombre,
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

        private async Task CargarPlanificacionesEnAsignacionesAsync(IEnumerable<TicketDto> dtos)
        {
            if (dtos == null || !dtos.Any()) return;

            // 1. Recolectar todos los IDs de base de datos de los FrenteSubFrentes activos de todos los DTOs
            var frenteIds = dtos
                .Where(d => d.FrenteSubFrentes != null)
                .SelectMany(d => d.FrenteSubFrentes)
                .Where(fsf => fsf.Activo)
                .Select(fsf => fsf.Id)
                .Distinct()
                .ToList();

            if (!frenteIds.Any()) return;

            // 2. Obtener los registros de DetallePlanificacionConsultor usando los IDs recolectados
            var planningEntities = await _consultorAsignacionRepository.GetPlanificacionesByFrenteIdsAsync(frenteIds);
            var planningDtos = _mapper.Map<List<DetallePlanificacionConsultorDto>>(planningEntities);
            
            // Agrupar los registros de planificación por IdTicketFrenteSubFrente
            var planningByFrenteMap = planningDtos
                .GroupBy(p => p.IdTicketFrenteSubFrente)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 3. Para cada TicketDto, reconstruir asignaciones temporales (placeholders) si es necesario, y mapear la planificación a asignaciones reales
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

        public async Task<bool> CambiarEstadoPorCodigoAsync(string codigoTicket, string codigoNuevoEstado)
        {
            // 1. Buscar Ticket
            var ticket = await _ticketRepository.GetByCualquierCodigoAsync(codigoTicket);
            if (ticket == null) throw new Exception($"No se encontró ningún ticket con el código {codigoTicket}");

            // 2. Buscar Catálogos
            var estadoActual = _parametrosCatalogo.Current.ListaEstados.FirstOrDefault(e => e.Id == ticket.IdEstadoTicket);
            // 2b. Buscar el NUEVO estado por CÓDIGO o por NOMBRE
            var estadoNuevo = _parametrosCatalogo.Current.ListaEstados
                .FirstOrDefault(e =>
                    e.Codigo.Equals(codigoNuevoEstado, StringComparison.OrdinalIgnoreCase) ||
                    (e.Nombre != null && e.Nombre.Equals(codigoNuevoEstado, StringComparison.OrdinalIgnoreCase))
                );

            if (estadoNuevo == null)
                throw new Exception($"El estado indicado '{codigoNuevoEstado}' no es válido o no existe en el catálogo.");

            // 3. Dominio hace la magia (valida reglas + genera historial interno)
            ticket.CambiarEstado(estadoActual, estadoNuevo, "AGENTE_IA");

            // 4. Mapear y Guardar
            var updateDto = _mapper.Map<TicketUpdateDto>(ticket);
            updateDto.UsuarioActualizacion = "AGENTE_IA";

            // Rellenamos para pasar validaciones del UpdateAsync
            updateDto.ConsultorAsignaciones ??= new List<TicketConsultorAsignacionUpdateDto>();
            updateDto.FrenteSubFrentes ??= new List<TicketFrenteSubFrenteUpdateDto>();
            updateDto.IdGestoresSecundarios ??= new List<int>();

            await UpdateAsync(ticket.Id, updateDto);
            return true;
        }

    }
}

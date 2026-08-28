using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Constants;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConectaBiz.Domain.Constants.AppConstants;

namespace ConectaBiz.Application.Services
{
    public class SGRCSTIService: ISGRCSTIService
    {
        private readonly ISGRCSTIRepository _sgrcstiRepository;
        private readonly IEmpresaRepository _empresaRepository;
        private readonly IEmpresaService _empresaService;
        private readonly IPersonaService _personaService;
        private readonly ITicketService _ticketService;
        private readonly IPersonaRepository _personaRepository;
        private readonly IParametrosCatalogo _parametrosCatalogo;
        private readonly IConfiguration _configuration;
        private readonly INotificacionSistemaService _notificacionSistemaService;

        // 🔹 Variables para cachear los datos que cargamos en ProcesarExcelAsync
        private IEnumerable<Parametro> _listaTipoTicket = Array.Empty<Parametro>();
        private IEnumerable<Parametro> _listaSubTipoTicket = Array.Empty<Parametro>();
        private IEnumerable<Parametro> _listaEstados = Array.Empty<Parametro>();
        private IEnumerable<Parametro> _listaPrioridades = Array.Empty<Parametro>();
        private IEnumerable<Parametro> _listaParametros = Array.Empty<Parametro>();
        private IEnumerable<Parametro> _listaTipoActividad = Array.Empty<Parametro>();
        private readonly ConectaBiz.Domain.Interfaces.IUserRepository _userRepository;

        public SGRCSTIService(
            ISGRCSTIRepository sGRCSTIRepository, 
            IEmpresaRepository empresaRepository, 
            IEmpresaService empresaService, 
            IPersonaService personaService, 
            ITicketService ticketService,
            IPersonaRepository personaRepository,
            IParametrosCatalogo parametrosCatalogo,
            IConfiguration configuration,
            INotificacionSistemaService notificacionSistemaService,
            ConectaBiz.Domain.Interfaces.IUserRepository userRepository
            )
        {
            _sgrcstiRepository = sGRCSTIRepository;
            _empresaRepository = empresaRepository;
            _empresaService = empresaService;
            _personaService = personaService;
            _ticketService = ticketService;
            _personaRepository = personaRepository;
            _parametrosCatalogo = parametrosCatalogo;
            _configuration = configuration;
            _notificacionSistemaService = notificacionSistemaService;
            _userRepository = userRepository;
        }

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

        public async Task MigracionEmpresa()
        {
            var Clientes =await _empresaRepository.GetAllAsync();
            var ClientesSGRCSTI = await _sgrcstiRepository.ObtenerEmpresasByExcepcion(Clientes.Any(x => x.CodSgrCsti != null) ? Clientes.Select(x => (int)x.CodSgrCsti).ToList() : null);
        }

        public async Task<IEnumerable<dynamic>> MigracionRequerimientos()
        {
            await InicializarDatosAsync();

            var resultados = await _sgrcstiRepository.MigracionRequerimientos();
            var listadoTicketsExistentes = await _ticketService.GetAllCodTicketInternosAsync();
            var hashTicketsExistentes = new HashSet<string>(listadoTicketsExistentes);

            var personaDto = await _personaService.GetByIdAsync(58);

            var errores = new List<string>(); // Para registrar los errores
            var resultadosFinales = new List<dynamic>(); // Para devolver los insertados

            // Filtramos la lista antes para iterar solo sobre los que no existen
            var requerimientosNuevos = resultados
                .Where(req => !hashTicketsExistentes.Contains((string)req.codrequerimiento))
                .ToList();

            foreach (var req in requerimientosNuevos)
            {
                try
                {
                    var createEmpresaDto = new DTOs.CreateEmpresaDto
                    {
                        RazonSocial = req.empresa_razonsocial,
                        NombreComercial = req.empresa_nombrecomercial,
                        NumDocContribuyente = req.empresa_ruc,
                        Direccion = req.empresa_direccion,
                        Telefono = req.empresa_telefono,
                        CodSgrCsti = req.empresa_idempresa,
                        IdSocio = 1,
                        IdPais = 1,
                        IdGestor = 47,
                        UsuarioRegistro = "Migracion SGR",
                        Persona = personaDto == null ? null : new DTOs.CreatePersonaDto
                        {
                            Nombres = personaDto.Nombres,
                            ApellidoMaterno = personaDto.ApellidoMaterno,
                            ApellidoPaterno = personaDto.ApellidoPaterno,
                            NumeroDocumento = personaDto.NumeroDocumento,
                            TipoDocumento = personaDto.TipoDocumento,
                            Telefono = personaDto.Telefono,
                            Telefono2 = personaDto.Telefono2,
                            Correo = personaDto.Correo,
                            Direccion = personaDto.Direccion,
                            FechaNacimiento = personaDto.FechaNacimiento
                        }
                    };

                    int idEmpresa = 0;
                    if (createEmpresaDto.CodSgrCsti != null)
                    {
                        var empresaExistente = await _empresaRepository.GetByCodSgrCstiAsync((int)createEmpresaDto.CodSgrCsti) 
                                            ?? await _empresaRepository.GetByNumDocContribuyenteDatAsync(createEmpresaDto.NumDocContribuyente);

                        idEmpresa = empresaExistente?.Id ?? (await _empresaService.CreateAsync(createEmpresaDto)).Id;
                    }

                    // Lógica para excluir tickets de ciertas empresas (ID del sistema Conecta) obtenido de appsettings
                    var empresasConectaExcluidas = _configuration.GetSection("MigracionSGRSettings:EmpresasExcluidas")
                                                                 .GetChildren()
                                                                 .Select(x => int.Parse(x.Value ?? "0"))
                                                                 .ToList();
                    if (empresasConectaExcluidas.Contains(idEmpresa))
                    {
                        continue; // Omitir la creación del ticket para esta empresa
                    }

                    var subTipoTicket = MapTipoServicioToTipoTicket(req.id_tipo_servicio);

                    var ticketInsertDto = new TicketInsertDto
                    {
                        CodReqSgrCsti = req.codrequerimiento,
                        IdReqSgrCsti = req.idrequerimiento,
                        CodTicketInterno = req.codrequerimiento,
                        Titulo = req.titulo,
                        FechaSolicitud = req.fecharegistro,
                        IdTipoTicket = _listaTipoTicket.FirstOrDefault(t => t.Codigo.Equals(AppConstants.TipoTicket.BolsaDeHoras)).Id,
                        IdSubTipoTicket = subTipoTicket,
                        IdEstadoTicket = _listaEstados.FirstOrDefault(t => t.Codigo.Equals(AppConstants.Estados.PENDIENTE_ATENCION)).Id,
                        IdEmpresa = idEmpresa,
                        IdUsuarioResponsableCliente = personaDto.Id,
                        IdPrioridad = MapPrioridadToId(req.prioridad_descripcion),
                        Descripcion = req.detalle ?? "",
                        UrlArchivos = null, 
                        UsuarioCreacion = "Migracion SGR",
                        EsCargaMasiva = true,
                        IdGestorConsultoria = 100
                    };

                    var ticketGuardado = await _ticketService.CreateAsync(ticketInsertDto);

                    resultadosFinales.Add(req);
                }
                catch (Exception ex)
                {
                    // Registrar el error pero continuar con el siguiente
                    var detalle = $"Error en requerimiento {req?.codrequerimiento}: {ex.Message}";
                    errores.Add(detalle);
                    Console.WriteLine("❌ " + detalle);
                }
            }
            return resultadosFinales;
        }

        private int MapTipoServicioToTipoTicket(int idTipoServicio)
        {
            return idTipoServicio switch
            {
                1 => _listaSubTipoTicket.FirstOrDefault(t => t.Codigo.Equals(AppConstants.SubtipoTicket.BolsaDeHoras.Incidencia)).Id, // Incidente
                2 => _listaSubTipoTicket.FirstOrDefault(t => t.Codigo.Equals(AppConstants.SubtipoTicket.BolsaDeHoras.Requerimiento)).Id, // Requerimiento
                3 => _listaSubTipoTicket.FirstOrDefault(t => t.Codigo.Equals(AppConstants.SubtipoTicket.BolsaDeHoras.Requerimiento)).Id, // Garantía también será Requerimiento
                _ => _listaSubTipoTicket.FirstOrDefault(t => t.Codigo.Equals(AppConstants.SubtipoTicket.BolsaDeHoras.Requerimiento)).Id,  // Valor por defecto
            };
        }

        private int MapPrioridadToId(string prioridad)
        {
            return prioridad.ToUpper() switch
            {
                "BAJA" => _listaPrioridades.FirstOrDefault(t => t.Codigo.Equals(AppConstants.Prioridad.Baja)).Id,
                "MEDIA" => _listaPrioridades.FirstOrDefault(t => t.Codigo.Equals(AppConstants.Prioridad.Media)).Id,
                "ALTA" => _listaPrioridades.FirstOrDefault(t => t.Codigo.Equals(AppConstants.Prioridad.Alta)).Id,
                "CRITICA" => _listaPrioridades.FirstOrDefault(t => t.Codigo.Equals(AppConstants.Prioridad.Critica)).Id,
                _ => _listaPrioridades.FirstOrDefault(t => t.Codigo.Equals(AppConstants.Prioridad.Media)).Id, // Valor por defecto: Media

            };
        }

        public async Task<IEnumerable<dynamic>> MigracionRequerimientoPorCodAsync(string codTicketInterno)
        {
            await InicializarDatosAsync();

            var resultados = await _sgrcstiRepository.MigracionRequerimientoPorCod(codTicketInterno);
            var listadoTicketsExistentes = await _ticketService.GetAllCodTicketInternosAsync();
            var hashTicketsExistentes = new HashSet<string>(listadoTicketsExistentes);

            var personaDto = await _personaService.GetByIdAsync(58);
            var errores = new List<string>();
            var resultadosFinales = new List<dynamic>();

            var requerimientosNuevos = resultados
                .Where(req => !hashTicketsExistentes.Contains((string)req.codrequerimiento))
                .ToList();

            foreach (var req in requerimientosNuevos)
            {
                try
                {
                    var createEmpresaDto = new DTOs.CreateEmpresaDto
                    {
                        RazonSocial = req.empresa_razonsocial,
                        NombreComercial = req.empresa_nombrecomercial,
                        NumDocContribuyente = req.empresa_ruc,
                        Direccion = req.empresa_direccion,
                        Telefono = req.empresa_telefono,
                        CodSgrCsti = req.empresa_idempresa,
                        IdSocio = 1,
                        IdPais = 1,
                        IdGestor = 47,
                        UsuarioRegistro = "Migracion SGR",
                        Persona = personaDto == null ? null : new DTOs.CreatePersonaDto
                        {
                            Nombres = personaDto.Nombres,
                            ApellidoMaterno = personaDto.ApellidoMaterno,
                            ApellidoPaterno = personaDto.ApellidoPaterno,
                            NumeroDocumento = personaDto.NumeroDocumento,
                            TipoDocumento = personaDto.TipoDocumento,
                            Telefono = personaDto.Telefono,
                            Telefono2 = personaDto.Telefono2,
                            Correo = personaDto.Correo,
                            Direccion = personaDto.Direccion,
                            FechaNacimiento = personaDto.FechaNacimiento
                        }
                    };

                    int idEmpresa = 0;
                    if (createEmpresaDto.CodSgrCsti != null)
                    {
                        var empresaExistente = await _empresaRepository.GetByCodSgrCstiAsync((int)createEmpresaDto.CodSgrCsti) 
                                               ?? await _empresaRepository.GetByNumDocContribuyenteDatAsync(createEmpresaDto.NumDocContribuyente);

                        idEmpresa = empresaExistente?.Id ?? (await _empresaService.CreateAsync(createEmpresaDto)).Id;
                    }

                    // Lógica para excluir tickets de ciertas empresas (ID del sistema Conecta)
                    var empresasConectaExcluidas = _configuration.GetSection("MigracionSGRSettings:EmpresasExcluidas")
                                                                 .GetChildren()
                                                                 .Select(x => int.Parse(x.Value ?? "0"))
                                                                 .ToList();
                    if (empresasConectaExcluidas.Contains(idEmpresa))
                    {
                        continue; // Omitir la creación del ticket para esta empresa
                    }

                    var subTipoTicket = MapTipoServicioToTipoTicket(req.id_tipo_servicio);

                    var ticketInsertDto = new TicketInsertDto
                    {
                        CodReqSgrCsti = req.codrequerimiento,
                        IdReqSgrCsti = req.idrequerimiento,
                        CodTicketInterno = req.codrequerimiento,
                        Titulo = req.titulo,
                        FechaSolicitud = req.fecharegistro,
                        IdTipoTicket = _listaTipoTicket.FirstOrDefault(t => t.Codigo.Equals(AppConstants.TipoTicket.BolsaDeHoras)).Id,
                        IdSubTipoTicket = subTipoTicket,
                        IdEstadoTicket = _listaEstados.FirstOrDefault(t => t.Codigo.Equals(AppConstants.Estados.PENDIENTE_ATENCION)).Id,
                        IdEmpresa = idEmpresa,
                        IdUsuarioResponsableCliente = personaDto.Id,
                        IdPrioridad = MapPrioridadToId(req.prioridad_descripcion),
                        Descripcion = req.detalle ?? "",
                        UrlArchivos = null, 
                        UsuarioCreacion = "Migracion Manual SGR",
                        EsCargaMasiva = true,
                        IdGestorConsultoria = 100
                    };

                    var ticketGuardado = await _ticketService.CreateAsync(ticketInsertDto);

                    resultadosFinales.Add(req);
                }
                catch (Exception ex)
                {
                    var detalle = $"Error en requerimiento por cod {req?.codrequerimiento}: {ex.Message}";
                    errores.Add(detalle);
                    Console.WriteLine("❌ " + detalle);
                }
            }
            return resultadosFinales;
        }
    }
}
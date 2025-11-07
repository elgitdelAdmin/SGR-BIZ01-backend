using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private readonly Lazy<INotificacionTicketService> _notificacionTicketService;

        public SGRCSTIService(
            ISGRCSTIRepository sGRCSTIRepository, 
            IEmpresaRepository empresaRepository, 
            IEmpresaService empresaService, 
            IPersonaService personaService, 
            ITicketService ticketService,
            IPersonaRepository personaRepository,
            Lazy<INotificacionTicketService> notificacionTicketService
            )
        {
            _sgrcstiRepository = sGRCSTIRepository;
            _empresaRepository = empresaRepository;
            _empresaService = empresaService;
            _personaService = personaService;
            _ticketService = ticketService;
            _personaRepository = personaRepository;
            _notificacionTicketService = notificacionTicketService;
        }
        public async Task MigracionEmpresa()
        {
            var Clientes=await _empresaRepository.GetAllAsync();

            var ClientesSGRCSTI = await _sgrcstiRepository.ObtenerEmpresasByExcepcion(Clientes.Any(x => x.CodSgrCsti != null) ? Clientes.Select(x => (int)x.CodSgrCsti).ToList() : null);
        }

        public async Task<IEnumerable<dynamic>> MigracionRequerimientos()
        {
            var resultados = await _sgrcstiRepository.MigracionRequerimientos();
            var personaDto = await _personaService.GetByIdAsync(58);

            var errores = new List<string>(); // Para registrar los errores

            foreach (var req in resultados)
            {
                try
                {
                    var ticketExistente = await _ticketService.GetByCodReqSgrCstiAsync(req.codrequerimiento);
                    if (ticketExistente == null)
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
                            UsuarioRegistro = "Migracion",
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
                            var empresaExistente = await _empresaRepository.GetByCodSgrCstiAsync((int)createEmpresaDto.CodSgrCsti);
                            var empresaExisteNroDoc = await _empresaRepository.GetByNumDocContribuyenteDatAsync(createEmpresaDto.NumDocContribuyente);
                            if (empresaExistente == null && empresaExisteNroDoc == null) 
                            {
                                var empresaCreada = await _empresaService.CreateAsync(createEmpresaDto);
                                idEmpresa = empresaCreada.Id;
                            }
                            else
                            {
                                idEmpresa = empresaExistente != null
                                    ? empresaExistente.Id
                                    : empresaExisteNroDoc.Id;
                            }
                        }

                        var tipoTicket = MapTipoServicioToTipoTicket(req.id_tipo_servicio);

                        // Validar si ya existe un ticket con ese CodReqSgrCsti


                        var ticketInsertDto = new TicketInsertDto
                        {
                            CodReqSgrCsti = req.codrequerimiento,
                            IdReqSgrCsti = req.idrequerimiento,
                            CodTicketInterno = req.codrequerimiento,
                            Titulo = req.titulo,
                            FechaSolicitud = req.fecharegistro,
                            IdTipoTicket = tipoTicket,
                            IdEstadoTicket = 59,
                            IdEmpresa = idEmpresa,
                            IdUsuarioResponsableCliente = personaDto.Id,
                            IdPrioridad = MapPrioridadToId(req.prioridad_descripcion),
                            Descripcion = req.detalle ?? "",
                            UrlArchivos = null, 
                            UsuarioCreacion = "Migracion",
                            EsCargaMasiva = true,
                            IdGestorConsultoria = 32
                        };

                        var ticketGuardado = await _ticketService.CreateAsync(ticketInsertDto);

                        await _notificacionTicketService.Value.AddRangeAsync(new[]
                          {
                            new CrearNotificacionDto
                            {
                                IdTicket = ticketGuardado.Id,
                                IdUser = 32,
                                Mensaje = $"El Ticket {ticketGuardado.CodTicket} ha sido asignado a usted"
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Registrar el error pero continuar con el siguiente
                    var detalle = $"Error en requerimiento {req?.codrequerimiento}: {ex.Message}";
                    errores.Add(detalle);
                    Console.WriteLine("❌ " + detalle);
                }
            }
            return resultados;
        }

        private int MapTipoServicioToTipoTicket(int idTipoServicio)
        {
            return idTipoServicio switch
            {
                1 => 12, // Incidente
                2 => 13, // Requerimiento
                3 => 13, // Garantía también será Requerimiento
                _ => 13  // Valor por defecto
            };
        }

        private int MapPrioridadToId(string prioridad)
        {
            return prioridad.ToUpper() switch
            {
                "BAJA" => 18,
                "MEDIA" => 19,
                "ALTA" => 20,
                "CRITICA" => 21,
                _ => 19 // Valor por defecto: Media

            };
        }
    }
}
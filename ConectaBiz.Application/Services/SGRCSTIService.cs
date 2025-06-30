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
        public SGRCSTIService(ISGRCSTIRepository sGRCSTIRepository, IEmpresaRepository empresaRepository, IEmpresaService empresaService, IPersonaService personaService, ITicketService ticketService, IPersonaRepository personaRepository)
        {
            _sgrcstiRepository = sGRCSTIRepository;
            _empresaRepository = empresaRepository;
            _empresaService = empresaService;
            _personaService = personaService;
            _ticketService = ticketService;
            _personaRepository = personaRepository;
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
            foreach (var req in resultados)
            {
                // Buscar si ya existe la persona responsable por tipo y número de documento
                //var personaResponsable = await _personaRepository.GetByTipoNumDocumentoAsync(
                //    req.responsablecliente_idtipodocumento,
                //    req.responsablecliente_documento
                //);

                //int idPersonaResponsable;
                //if (personaResponsable == null)
                //{
                //    // Si no existe, crear la persona responsable
                //    var nuevaPersona = new Persona
                //    {
                //        Nombres = req.responsablecliente_nombres,
                //        ApellidoPaterno = req.responsablecliente_apepaterno,
                //        ApellidoMaterno = req.responsablecliente_apematerno,
                //        NumeroDocumento = req.responsablecliente_documento,
                //        TipoDocumento = req.responsablecliente_tipodocumento,
                //        Telefono = req.responsablecliente_telefonomovil,
                //        Telefono2 = req.responsablecliente_fijo,
                //        Correo = req.responsablecliente_correo,
                //        Direccion = req.responsablecliente_direccion,
                //        FechaNacimiento = req.responsablecliente_fechanacimiento,
                //        FechaCreacion = DateTime.Now,
                //        Activo = true
                //    };
                //    var personaCreada = await _personaRepository.CreateAsync(nuevaPersona);
                //    idPersonaResponsable = personaCreada.Id;
                //}
                //else
                //{
                //    idPersonaResponsable = personaResponsable.Id;
                //}

                var createEmpresaDto = new DTOs.CreateEmpresaDto
                {
                    RazonSocial = req.empresa_razonsocial,
                    NombreComercial = req.empresa_nombrecomercial,
                    NumDocContribuyente = req.empresa_ruc,
                    Direccion = req.empresa_direccion,
                    Telefono = req.empresa_telefono,
                    CodSgrCsti = req.empresa_idempresa,
                    IdSocio= 1,// Pendiente de definicion
                    // Completa los campos obligatorios según tu lógica de negocio:
                    // Email = ...
                    IdPais = 1,
                    IdGestor = 22,
                    UsuarioRegistro = "Migracion",
                    Persona = personaDto == null ? null : new DTOs.CreatePersonaDto {
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
                    if (empresaExistente == null)
                    {
                        var empresaCreada = await _empresaService.CreateAsync(createEmpresaDto);
                        idEmpresa = empresaCreada.Id;
                    }
                    else
                    {
                        idEmpresa = empresaExistente.Id;
                    }
                }

                var tipoTicket = MapTipoServicioToTipoTicket(req.id_tipo_servicio);

                // Validar si ya existe un ticket con ese CodReqSgrCsti
                var ticketExistente = await _ticketService.GetByCodReqSgrCstiAsync(req.codrequerimiento);
                if (ticketExistente == null)
                {
                    var ticketInsertDto = new TicketInsertDto
                    {
                        CodReqSgrCsti = req.codrequerimiento,
                        IdReqSgrCsti = req.idrequerimiento,
                        CodTicketInterno = req.codrequerimiento,
                        Titulo = req.titulo,
                        FechaSolicitud = req.fecharegistro,
                        IdTipoTicket = tipoTicket,
                        IdEstadoTicket = 1,// req.idestadorequerimiento,
                        IdEmpresa = idEmpresa,
                        IdUsuarioResponsableCliente = personaDto.Id,
                        IdPrioridad = MapPrioridadToId(req.prioridad_descripcion),
                        Descripcion = req.detalle ?? "",
                        UrlArchivos = null, // Si tienes archivos, asígnalos aquí
                        UsuarioCreacion = "Migracion",
                        ConsultorAsignaciones = new List<TicketConsultorAsignacionInsertDto>(), // Llena si corresponde
                        FrenteSubFrentes = new List<TicketFrenteSubFrenteInsertDto>() // Llena si corresponde
                    };

                    var ticketCreado = await _ticketService.CreateAsync(ticketInsertDto);
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
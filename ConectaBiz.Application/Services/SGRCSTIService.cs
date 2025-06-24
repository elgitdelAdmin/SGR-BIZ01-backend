using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
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
        public SGRCSTIService(ISGRCSTIRepository sGRCSTIRepository, IEmpresaRepository empresaRepository, IEmpresaService empresaService, IPersonaService personaService, ITicketService ticketService)
        {
            _sgrcstiRepository = sGRCSTIRepository;
            _empresaRepository = empresaRepository;
            _empresaService = empresaService;
            _personaService = personaService;
            _ticketService = ticketService;
        }
        public async Task MigracionEmpresa()
        {
            var Clientes=await _empresaRepository.GetAllAsync();

            var ClientesSGRCSTI = await _sgrcstiRepository.ObtenerEmpresasByExcepcion(Clientes.Any(x=>x.codSGRCSTI!=null)?  Clientes.Select(x =>(int) x.codSGRCSTI).ToList():null);
        }

        public async Task<IEnumerable<dynamic>> MigracionRequerimientos()
        {
            var resultados = await _sgrcstiRepository.MigracionRequerimientos();

            // Obtener la persona con id 58
            var personaDto = await _personaService.GetByIdAsync(58);

            foreach (var req in resultados)
            {
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
                    // IdPais = ...
                    // IdGestor = ...
                    // IdSocio = ...
                    // UsuarioRegistro = "migracion",
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

                var ticketInsertDto = new TicketInsertDto
                {
                    CodTicketInterno = req.codrequerimiento,
                    Titulo = req.titulo,
                    FechaSolicitud = req.fecharegistro,
                    IdTipoTicket = tipoTicket,
                    IdEstadoTicket = req.idestadorequerimiento,
                    IdEmpresa = idEmpresa,
                    IdUsuarioResponsableCliente = req.responsablecliente_idusuario,
                    IdPais = 1, // Asigna el país correspondiente
                    IdPrioridad = MapPrioridadToId(req.prioridad_descripcion),
                    Descripcion = req.detalle ?? "",
                    UrlArchivos = null, // Si tienes archivos, asígnalos aquí
                    IdGestorAsignado = 0, // Asigna el gestor si corresponde
                    ConsultorAsignaciones = new List<TicketConsultorAsignacionInsertDto>(), // Llena si corresponde
                    FrenteSubFrentes = new List<TicketFrenteSubFrenteInsertDto>() // Llena si corresponde
                };

                //var ticketCreado = await _ticketService.CreateAsync(ticketInsertDto);
            }
            return resultados; 
        }

        private string MapTipoServicioToTipoTicket(int idTipoServicio)
        {
            return idTipoServicio switch
            {
                1 => "INC", // Incidente
                2 => "REQ", // Requerimiento
                3 => "REQ", // Garantía también será Requerimiento
                _ => "REQ"  // Valor por defecto
            };
        }

        private int MapPrioridadToId(string prioridad)
        {
            return prioridad.ToUpper() switch
            {
                "BAJA" => 3,
                "MEDIA" => 2,
                "ALTA" => 1,
                "CRITICA" => 4,
                _ => 2 // Valor por defecto: Media
            };
        }
    }
}
using AutoMapper;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Domain.Entities;
using System.Text.Json;

namespace ConectaBiz.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Socio, opt => opt.MapFrom(src => src.Socio))
                .ForMember(dest => dest.Persona, opt => opt.MapFrom(src => src.Persona))
                .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.Rol));

            CreateMap<Rol, RolDto>();

            CreateMap<Socio, SocioDto>();
            CreateMap<RegisterUserDto, User>();

            // Mapeo de Persona
            CreateMap<Persona, PersonaDto>();
            CreateMap<PersonaDto, Persona>();
            CreateMap<Persona, PersonaConUsuariosEmpresaDto>()
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.Users));
            CreateMap<User, UserEmpresaDto>()
                .ForMember(dest => dest.RolCodigo, opt => opt.MapFrom(src => src.Rol.Codigo));

            CreateMap<UpdatePersonaDto, Persona>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<Persona, UpdatePersonaDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());


            // Mapeo de Consultor a ConsultorDto
            CreateMap<Consultor, ConsultorDto>()
                .ForMember(dest => dest.Persona, opt => opt.MapFrom(src => src.Persona))
                .ForMember(dest => dest.Especializaciones, opt => opt.MapFrom(src => src.ConsultorFrenteSubFrente))
                .ForMember(dest => dest.Socio, opt => opt.MapFrom(src => src.Socio));

            // Mapeo de ConsultorDto a Consultor
            CreateMap<ConsultorDto, Consultor>()
                .ForMember(dest => dest.Persona, opt => opt.Ignore()) // La persona se maneja por separado en el servicio
                .ForMember(dest => dest.ConsultorFrenteSubFrente, opt => opt.Ignore());

            // Mapeo de Frente
            CreateMap<Frente, FrenteDto>().ForMember(dest => dest.SubFrente, opt => opt.MapFrom(src => src.SubFrente));
            CreateMap<FrenteDto, Frente>().ForMember(dest => dest.SubFrente, opt => opt.Ignore());

            // Mapeo de SubFrente
            CreateMap<SubFrente, SubFrenteDto>(); 
            CreateMap<SubFrenteDto, SubFrente>().ForMember(dest => dest.Frente, opt => opt.Ignore());

            // Mapeos para ConsultorFrenteSubFrente
            CreateMap<ConsultorFrenteSubFrente, ConsultorFrenteSubFrenteDto>()
                //.ForMember(dest => dest.Consultor, opt => opt.MapFrom(src => src.Consultor))
                .ForMember(dest => dest.Frente, opt => opt.MapFrom(src => src.Frente))
                .ForMember(dest => dest.SubFrente, opt => opt.MapFrom(src => src.SubFrente));

            CreateMap<ConsultorFrenteSubFrenteDto, ConsultorFrenteSubFrente>()
                .ForMember(dest => dest.Consultor, opt => opt.Ignore())
                .ForMember(dest => dest.Frente, opt => opt.Ignore())
                .ForMember(dest => dest.SubFrente, opt => opt.Ignore());

            // mapeos para los DTOs de creación
            CreateMap<CreateConsultorDto, ConsultorDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PersonaId, opt => opt.Ignore())
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
                .ForMember(dest => dest.FechaActualizacion, opt => opt.Ignore())
                .ForMember(dest => dest.Activo, opt => opt.Ignore())
                .ForMember(dest => dest.Especializaciones, opt => opt.MapFrom(src => src.Especializaciones));

                CreateMap<CreatePersonaDto, PersonaDto>()
                  .ForMember(dest => dest.Id, opt => opt.Ignore())
                  .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
                  .ForMember(dest => dest.FechaActualizacion, opt => opt.Ignore())
                  .ForMember(dest => dest.Activo, opt => opt.Ignore());

                CreateMap<CreatePersonaDto, Persona>();
            
                CreateMap<CreateConsultorEspecializacionDto, ConsultorFrenteSubFrenteDto>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.ConsultorId, opt => opt.Ignore())
                    .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
                    .ForMember(dest => dest.FechaActualizacion, opt => opt.Ignore())
                    .ForMember(dest => dest.Activo, opt => opt.Ignore())
                   // .ForMember(dest => dest.Consultor, opt => opt.Ignore())
                    .ForMember(dest => dest.Frente, opt => opt.Ignore())
                    .ForMember(dest => dest.SubFrente, opt => opt.Ignore());

            // mapeos para los DTOs de actualización
            CreateMap<UpdateConsultorDto, ConsultorDto>()
                .ForMember(dest => dest.PersonaId, opt => opt.Ignore())
                .ForMember(dest => dest.FechaActualizacion, opt => opt.Ignore())
                .ForMember(dest => dest.Activo, opt => opt.Ignore())
                .ForMember(dest => dest.Especializaciones, opt => opt.MapFrom(src => src.Especializaciones));
            CreateMap<UpdatePersonaDto, PersonaDto>()
                .ForMember(dest => dest.FechaActualizacion, opt => opt.Ignore())
                .ForMember(dest => dest.Activo, opt => opt.Ignore());


            // NUEVOS MAPEOS PARA LISTADO (sin referencias circulares)
            CreateMap<Consultor, ConsultorListDto>()
                .ForMember(dest => dest.Persona, opt => opt.MapFrom(src => src.Persona))
                .ForMember(dest => dest.Socio, opt => opt.MapFrom(src => src.Socio))
                .ForMember(dest => dest.Especializaciones, opt => opt.MapFrom(src => src.ConsultorFrenteSubFrente));

            CreateMap<ConsultorFrenteSubFrente, ConsultorEspecializacionListDto>()
                .ForMember(dest => dest.Frente, opt => opt.MapFrom(src => src.Frente))
                .ForMember(dest => dest.SubFrente, opt => opt.MapFrom(src => src.SubFrente));

            // MAPEOS PARA DETALLE COMPLETO (para GetById)
            CreateMap<Consultor, ConsultorDetailDto>()
                .ForMember(dest => dest.Persona, opt => opt.MapFrom(src => src.Persona))
                .ForMember(dest => dest.Socio, opt => opt.MapFrom(src => src.Socio))
                .ForMember(dest => dest.Especializaciones, opt => opt.MapFrom(src => src.ConsultorFrenteSubFrente));

            CreateMap<ConsultorFrenteSubFrente, ConsultorEspecializacionDetailDto>()
                .ForMember(dest => dest.Frente, opt => opt.MapFrom(src => src.Frente))
                .ForMember(dest => dest.SubFrente, opt => opt.MapFrom(src => src.SubFrente));

            // MAPEOS ENTRE DTOs (ConsultorDto -> ConsultorListDto)
            CreateMap<ConsultorDto, ConsultorListDto>()
                .ForMember(dest => dest.Especializaciones, opt => opt.MapFrom(src => src.Especializaciones));

            CreateMap<ConsultorFrenteSubFrenteDto, ConsultorEspecializacionListDto>()
                .ForMember(dest => dest.Frente, opt => opt.MapFrom(src => src.Frente))
                .ForMember(dest => dest.SubFrente, opt => opt.MapFrom(src => src.SubFrente));

            // MAPEO ENTRE DTOs (ConsultorDto -> ConsultorDetailDto)
            CreateMap<ConsultorDto, ConsultorDetailDto>()
                .ForMember(dest => dest.Especializaciones, opt => opt.MapFrom(src => src.Especializaciones));

            CreateMap<ConsultorFrenteSubFrenteDto, ConsultorEspecializacionDetailDto>()
                .ForMember(dest => dest.Frente, opt => opt.MapFrom(src => src.Frente))
                .ForMember(dest => dest.SubFrente, opt => opt.MapFrom(src => src.SubFrente));

            // Parametro mappings
            CreateMap<Parametro, ParametroDto>();

            CreateMap<CreateParametroDto, Parametro>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FechaRegistro, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.FechaModificacion, opt => opt.Ignore());

            CreateMap<UpdateParametroDto, Parametro>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FechaRegistro, opt => opt.Ignore())
                .ForMember(dest => dest.FechaModificacion, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.UsuarioRegistro, opt => opt.Ignore());

            // Mapeos principales de Ticket
            CreateMap<Ticket, TicketDto>()
                .ForMember(dest => dest.ConsultorAsignaciones, opt => opt.MapFrom(src => src.ConsultorAsignaciones))
                .ForMember(dest => dest.FrenteSubFrentes, opt => opt.MapFrom(src => src.FrenteSubFrentes))
                .ForMember(dest => dest.Historial, opt => opt.MapFrom(src => src.TicketHistorialEstado));

            CreateMap<TicketDto, Ticket>()
                .ForMember(dest => dest.ConsultorAsignaciones, opt => opt.Ignore())
                .ForMember(dest => dest.FrenteSubFrentes, opt => opt.Ignore())
                .ForMember(dest => dest.TicketHistorialEstado, opt => opt.Ignore());

            CreateMap<TicketInsertDto, Ticket>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FechaActualizacion, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioActualizacion, opt => opt.Ignore())
                .ForMember(dest => dest.ConsultorAsignaciones, opt => opt.Ignore())
                .ForMember(dest => dest.FrenteSubFrentes, opt => opt.Ignore())
                .ForMember(dest => dest.TicketHistorialEstado, opt => opt.Ignore());

            // Mapeos de TicketConsultorAsignacion
            CreateMap<TicketConsultorAsignacion, TicketConsultorAsignacionDto>();
            CreateMap<TicketConsultorAsignacionDto, TicketConsultorAsignacion>()
                .ForMember(dest => dest.Ticket, opt => opt.Ignore());

            CreateMap<TicketConsultorAsignacionInsertDto, TicketConsultorAsignacion>()
                .ForMember(dest => dest.Ticket, opt => opt.Ignore());

            // Mapeos de TicketFrenteSubFrente
            CreateMap<TicketFrenteSubFrente, TicketFrenteSubFrenteDto>();
            CreateMap<TicketFrenteSubFrenteDto, TicketFrenteSubFrente>()
                .ForMember(dest => dest.Ticket, opt => opt.Ignore());

            CreateMap<TicketFrenteSubFrenteInsertDto, TicketFrenteSubFrente>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Ticket, opt => opt.Ignore());

            // Mapeos de TicketHistorialEstado
            CreateMap<TicketHistorialEstado, TicketHistorialEstadoDto>();
            CreateMap<TicketHistorialEstadoDto, TicketHistorialEstado>()
                .ForMember(dest => dest.Ticket, opt => opt.Ignore());


            // Mapeo de País
            CreateMap<Pais, PaisDto>();
            CreateMap<PaisDto, Pais>();
            CreateMap<CreatePaisDto, Pais>()
                .ForMember(dest => dest.FechaRegistro, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => true));
            CreateMap<UpdatePaisDto, Pais>()
                .ForMember(dest => dest.FechaModificacion, opt => opt.MapFrom(src => DateTime.Now));

            // Mapeo de Empresa
            CreateMap<Empresa, EmpresaDto>()
                .ForMember(dest => dest.NombrePais, opt => opt.MapFrom(src => src.Pais != null ? src.Pais.Nombre : null))
                .ForMember(dest => dest.NombreGestor, opt => opt.MapFrom(src => src.Gestor != null && src.Gestor.Persona != null
                    ? $"{src.Gestor.Persona.Nombres} {src.Gestor.Persona.ApellidoPaterno} {src.Gestor.Persona.ApellidoMaterno}".Trim()
                    : null))
              .ForMember(dest => dest.NombrePersonaResponsable, opt => opt.MapFrom(src => src.PersonaResponsable != null
                    ? $"{src.PersonaResponsable.Nombres} {src.PersonaResponsable.ApellidoPaterno} {src.PersonaResponsable.ApellidoMaterno}".Trim()
                    : null))
                 .ForMember(dest => dest.PersonaResponsable, opt => opt.MapFrom(src => src.PersonaResponsable));


            CreateMap<CreateEmpresaDto, Empresa>()
                .ForMember(dest => dest.FechaRegistro, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FechaModificacion, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioModificacion, opt => opt.Ignore());

            CreateMap<UpdateEmpresaDto, Empresa>()
                .ForMember(dest => dest.FechaModificacion, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FechaRegistro, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioRegistro, opt => opt.Ignore());

            // Mapeo de Gestor
            CreateMap<Gestor, GestorDto>()
                .ForMember(dest => dest.Nombres, opt => opt.MapFrom(src => src.Persona.Nombres))
                .ForMember(dest => dest.ApellidoPaterno, opt => opt.MapFrom(src => src.Persona.ApellidoPaterno))
                .ForMember(dest => dest.ApellidoMaterno, opt => opt.MapFrom(src => src.Persona.ApellidoMaterno))
                .ForMember(dest => dest.NumeroDocumento, opt => opt.MapFrom(src => src.Persona.NumeroDocumento))
                .ForMember(dest => dest.TipoDocumento, opt => opt.MapFrom(src => src.Persona.TipoDocumento))
                .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Persona.Telefono))
                .ForMember(dest => dest.Telefono2, opt => opt.MapFrom(src => src.Persona.Telefono2))
                .ForMember(dest => dest.Correo, opt => opt.MapFrom(src => src.Persona.Correo))
                .ForMember(dest => dest.Direccion, opt => opt.MapFrom(src => src.Persona.Direccion))
                .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.Persona.FechaNacimiento))
                .ForMember(dest => dest.FrentesSubFrente, opt => opt.MapFrom(src => src.GestorFrenteSubFrente));

            // Mapeo de GestorFrenteSubFrente
            CreateMap<GestorFrenteSubFrente, GestorFrenteSubFrenteDto>();
            CreateMap<CreateGestorFrenteSubFrenteDto, GestorFrenteSubFrente>()
                .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => true));

            CreateMap<RolPermisoModulo, ModuloPermisoDto>()
          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Modulo.Id))
          .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => src.Modulo.Codigo))
          .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Modulo.Nombre))
          .ForMember(dest => dest.Icono, opt => opt.MapFrom(src => src.Modulo.Icono))
          .ForMember(dest => dest.Ruta, opt => opt.MapFrom(src => src.Modulo.Ruta))
          .ForMember(dest => dest.DivsOcultos, opt => opt.MapFrom(src => ParseJsonArray(src.DivsOcultos)))
          .ForMember(dest => dest.ControlesBloqueados, opt => opt.MapFrom(src => ParseJsonArray(src.ControlesBloqueados)))
          .ForMember(dest => dest.ControlesOcultos, opt => opt.MapFrom(src => ParseJsonArray(src.ControlesOcultos)));
    }
        private static List<string> ParseJsonArray(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new();
            try
            {
                return JsonSerializer.Deserialize<List<string>>(json!) ?? new();
            }
            catch
            {
                return new();
            }
        }
    }
}
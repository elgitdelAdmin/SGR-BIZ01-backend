using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConectaBiz.Application.DTOs
{
    public class ConsultorDto
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public int IdNivelExperiencia { get; set; }
        public int IdModalidadLaboral { get; set; }
        public int IdSocio { get; set; }
        public int IdUser { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string? UsuarioActualizacion { get; set; }
        public string UsuarioCreacion { get; set; }
        public bool Activo { get; set; }
        // Datos de la persona relacionada
        public PersonaDto Persona { get; set; }
        public SocioDto Socio { get; set; }
        public List<ConsultorFrenteSubFrenteDto> Especializaciones { get; set; } = new List<ConsultorFrenteSubFrenteDto>();
    }


    // DTO específico para crear un consultor
    public class CreateConsultorDto
    {
        public int IdNivelExperiencia { get; set; }
        public int IdModalidadLaboral { get; set; }
        public int IdSocio { get; set; }
        public CreatePersonaDto Persona { get; set; }
        public List<CreateConsultorEspecializacionDto> Especializaciones { get; set; } = new List<CreateConsultorEspecializacionDto>();
    }

    // DTO específico para las especializaciones en creación
    public class CreateConsultorEspecializacionDto
    {
        public int IdFrente { get; set; }
        public int IdSubFrente { get; set; }
        public int IdNivelExperiencia { get; set; }
        public bool EsCertificado { get; set; }
    }
    // DTO específico para actualizar un consultor
    public class UpdateConsultorDto
    {
        //public int Id { get; set; }
        public int IdNivelExperiencia { get; set; }
        public int IdModalidadLaboral { get; set; }
        //public int IdSocio { get; set; }
        public string UsuarioActualizacion { get; set; }
        public UpdatePersonaDto Persona { get; set; }
        public List<CreateConsultorEspecializacionDto> Especializaciones { get; set; } = new List<CreateConsultorEspecializacionDto>();
    }
    // DTO para listar consultores (sin referencias circulares)
    public class ConsultorListDto
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public int IdNivelExperiencia { get; set; }
        public int IdSocio { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool Activo { get; set; }
        // Datos de la persona relacionada
        public PersonaDto Persona { get; set; }
        public SocioDto Socio { get; set; }
        public List<ConsultorEspecializacionListDto> Especializaciones { get; set; } = new List<ConsultorEspecializacionListDto>();
    }

    // DTO para especializaciones en listado (sin referencia circular al consultor)
    public class ConsultorEspecializacionListDto
    {
        public int Id { get; set; }
        public int ConsultorId { get; set; }
        public int IdFrente { get; set; }
        public int IdSubFrente { get; set; }
        public int IdNivelExperiencia { get; set; }
        public bool EsCertificado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool Activo { get; set; }

        // Solo información de Frente y SubFrente (sin referencia al Consultor)
        public FrenteDto? Frente { get; set; }
        public SubFrenteDto? SubFrente { get; set; }
    }

    // DTO para detalle completo del consultor (usado en GetById)
    public class ConsultorDetailDto
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public int IdNivelExperiencia { get; set; }
        public int IdModalidadLaboral { get; set; }
        public int IdSocio { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool Activo { get; set; }
        // Datos de la persona relacionada
        public PersonaDto Persona { get; set; }
        public SocioDto Socio { get; set; }
        public List<ConsultorEspecializacionDetailDto> Especializaciones { get; set; } = new List<ConsultorEspecializacionDetailDto>();
    }

    // DTO para especializaciones en detalle (sin referencia circular)
    public class ConsultorEspecializacionDetailDto
    {
        public int Id { get; set; }
        public int ConsultorId { get; set; }
        public int IdFrente { get; set; }
        public int IdSubFrente { get; set; }
        public int IdNivelExperiencia { get; set; }
        public bool EsCertificado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool Activo { get; set; }

        public FrenteDto? Frente { get; set; }
        public SubFrenteDto? SubFrente { get; set; }
    }
}

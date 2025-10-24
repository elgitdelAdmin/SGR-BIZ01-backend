using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.DTOs
{
    public class GestorDto
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public int? IdNivelExperiencia { get; set; }
        public int? IdModalidadLaboral { get; set; }
        public int IdSocio { get; set; }
        public int? IdUser { get; set; }
        public string UsuarioCreacion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioActualizacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool Activo { get; set; }

        // Datos de la persona
        public string Nombres { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string? NumeroDocumento { get; set; }
        public int TipoDocumento { get; set; }
        public string? Telefono { get; set; }
        public string? Telefono2 { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }

        // Lista de frentes y subfrentes
        public List<GestorFrenteSubFrenteDto> FrentesSubFrente { get; set; } = new List<GestorFrenteSubFrenteDto>();
    }

    public class CreateGestorDto
    {
        public int? IdNivelExperiencia { get; set; }
        public int? IdModalidadLaboral { get; set; }
        public int IdSocio { get; set; }
        public string UsuarioCreacion { get; set; }

        // Datos de la persona
        public string Nombres { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string? NumeroDocumento { get; set; }
        public int TipoDocumento { get; set; }
        public string? Telefono { get; set; }
        public string? Telefono2 { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public DateTime FechaNacimiento { get; set; }

        // Lista de frentes y subfrentes
        public List<CreateGestorFrenteSubFrenteDto> FrentesSubFrente { get; set; } = new List<CreateGestorFrenteSubFrenteDto>();
    }

    public class UpdateGestorDto
    {
        //public int Id { get; set; }
        public int? IdNivelExperiencia { get; set; }
        public int? IdModalidadLaboral { get; set; }
        public string? UsuarioActualizacion { get; set; }

        // Datos de la persona
        public string Nombres { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string? NumeroDocumento { get; set; }
        public int TipoDocumento { get; set; }
        public string? Telefono { get; set; }
        public string? Telefono2 { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }

        // Lista de frentes y subfrentes
        public List<CreateGestorFrenteSubFrenteDto> FrentesSubFrente { get; set; } = new List<CreateGestorFrenteSubFrenteDto>();
    }

}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.DTOs
{
    public class FrenteDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioRegistro { get; set; }
        public string? UsuarioModificacion { get; set; }

        // Lista de sub-frentes asociados
        public List<SubFrenteDto> SubFrente { get; set; } = new List<SubFrenteDto>();
    }

    // DTOs/SubFrenteDto.cs
    public class SubFrenteDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int IdFrente { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioRegistro { get; set; }
        public string? UsuarioModificacion { get; set; }
        public string? Nivel { get; set; }
    }
    public class CreateFrenteDto
    {
        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        [StringLength(50, ErrorMessage = "El usuario de registro no puede exceder 50 caracteres")]
        public string? UsuarioRegistro { get; set; }
    }
    public class UpdateFrenteDto
    {
        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; }

        [StringLength(50, ErrorMessage = "El usuario de modificación no puede exceder 50 caracteres")]
        public string? UsuarioModificacion { get; set; }
    }
    public class CreateSubFrenteDto
    {
        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El ID del frente es requerido")]
        public int IdFrente { get; set; }

        public bool Activo { get; set; } = true;

        [StringLength(50, ErrorMessage = "El usuario de registro no puede exceder 50 caracteres")]
        public string? UsuarioRegistro { get; set; }

        [StringLength(20, ErrorMessage = "El nivel no puede exceder 20 caracteres")]
        public string? Nivel { get; set; }
    }
    public class UpdateSubFrenteDto
    {
        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El ID del frente es requerido")]
        public int IdFrente { get; set; }

        public bool Activo { get; set; }

        [StringLength(50, ErrorMessage = "El usuario de modificación no puede exceder 50 caracteres")]
        public string? UsuarioModificacion { get; set; }

        [StringLength(20, ErrorMessage = "El nivel no puede exceder 20 caracteres")]
        public string? Nivel { get; set; }
    }
}

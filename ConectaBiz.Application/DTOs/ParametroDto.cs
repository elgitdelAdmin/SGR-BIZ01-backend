using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.DTOs
{
    public class ParametroDto
    {
        public int Id { get; set; }
        public string TipoParametro { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Color { get; set; }
        public string? Icono { get; set; }
        public short Orden { get; set; }
        public string? Valor1 { get; set; }
        public string? Valor2 { get; set; }
        public string? Valor3 { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioRegistro { get; set; }
        public string? UsuarioModificacion { get; set; }
    }

    public class CreateParametroDto
    {
        [Required(ErrorMessage = "El tipo de parámetro es requerido")]
        [StringLength(30, ErrorMessage = "El tipo de parámetro no puede exceder 30 caracteres")]
        public string TipoParametro { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        public string? Descripcion { get; set; }

        [StringLength(7, ErrorMessage = "El color no puede exceder 7 caracteres")]
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "El color debe tener formato hexadecimal válido")]
        public string? Color { get; set; }

        [StringLength(50, ErrorMessage = "El icono no puede exceder 50 caracteres")]
        public string? Icono { get; set; }

        public short Orden { get; set; } = 0;
        public bool Activo { get; set; } = true;
        public string? UsuarioRegistro { get; set; }
    }

    public class UpdateParametroDto
    {
        [Required(ErrorMessage = "El tipo de parámetro es requerido")]
        [StringLength(30, ErrorMessage = "El tipo de parámetro no puede exceder 30 caracteres")]
        public string TipoParametro { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        public string? Descripcion { get; set; }

        [StringLength(7, ErrorMessage = "El color no puede exceder 7 caracteres")]
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "El color debe tener formato hexadecimal válido")]
        public string? Color { get; set; }

        [StringLength(50, ErrorMessage = "El icono no puede exceder 50 caracteres")]
        public string? Icono { get; set; }

        public short Orden { get; set; }
        public bool Activo { get; set; }
        public string? UsuarioModificacion { get; set; }
    }
}

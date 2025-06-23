using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.DTOs
{
    public class GestorFrenteSubFrenteDto
    {
        public int Id { get; set; }
        public int IdGestor { get; set; }
        public int IdFrente { get; set; }
        public int IdSubFrente { get; set; }
        public int? IdNivelExperiencia { get; set; }
        public bool EsCertificado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UsuarioCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string? UsuarioActualizacion { get; set; }
        public bool Activo { get; set; }
    }

    public class CreateGestorFrenteSubFrenteDto
    {
        public int IdFrente { get; set; }
        public int IdSubFrente { get; set; }
        public int? IdNivelExperiencia { get; set; }
        public bool EsCertificado { get; set; }
        public string UsuarioCreacion { get; set; }
    }
}
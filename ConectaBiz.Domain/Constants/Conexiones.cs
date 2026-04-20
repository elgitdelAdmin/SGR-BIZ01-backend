using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Constants
{
    public     class Conexiones
    {
        public static string ConnectionSGRCSTI { get; set; }

        /// <summary>SQL Server del nuevo Conecta (BizPartner). Vacío = no se replica.</summary>
        public static string? ConnectionConectaNuevo { get; set; }
    }
}

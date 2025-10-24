using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Entities
{
    public class NotificacionTicket
    {
        public int Id { get; set; }
        public int IdTicket { get; set; }
        public int IdUser { get; set; }
        public string Mensaje { get; set; }
        public bool Leido { get; set; } = false;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaLectura { get; set; }
        public bool Activo { get; set; } = true;
        public virtual Ticket Ticket { get; set; }
    }
}

using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.DTOs
{
    public class NotificacionTicketDto
    {
        public int Id { get; set; }
        public int IdTicket { get; set; }
        public int IdUser { get; set; }
        public string Mensaje { get; set; }
        public bool Leido { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaLectura { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class MarcarLeidoDto
    {
        public int IdNotificacion { get; set; }
    }

    public class CrearNotificacionDto
    {
        public int IdTicket { get; set; }
        public int IdUser { get; set; }
        public string Mensaje { get; set; }
    }
}

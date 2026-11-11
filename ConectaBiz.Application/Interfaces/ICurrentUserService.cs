using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        string Username { get; }
        string Email { get; }
        bool IsAuthenticated { get; }

        int IdRol { get; }
        string CodRol { get; }
        bool IsSuperAdmin { get; }

        int? IdSocio { get; }
        int? IdSocioActual { get; }
        List<int> SociosIds { get; }
    }
}

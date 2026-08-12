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
        string CodRol { get; }
        List<int> SociosIds { get; }
    }
}

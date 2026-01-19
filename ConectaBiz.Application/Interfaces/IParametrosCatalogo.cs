using ConectaBiz.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface IParametrosCatalogo
    {
        Task EnsureLoadedAsync(CancellationToken ct = default);
        Task RefreshAsync(CancellationToken ct = default);
        ParametrosSnapshot Current { get; }
    }
}

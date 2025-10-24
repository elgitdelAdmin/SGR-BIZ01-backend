using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface ICargaMasivaTicketsService
    {
        Task<List<Dictionary<string, string>>> ProcesarExcelAsync(Stream stream, string tipo);
    }
}

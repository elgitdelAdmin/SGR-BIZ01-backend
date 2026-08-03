using System.Collections.Generic;
using System.IO;

namespace ConectaBiz.Application.Interfaces
{
    public interface IExcelService
    {
        byte[] GenerateExcel<T>(IEnumerable<T> data, string sheetName = "Sheet1");
        List<T> ReadExcel<T>(Stream stream, string tipoCarga);
        List<Dictionary<string, string>> LeerFilasComoDict(Stream stream, HashSet<string> columnasObligatorias);
    }
}

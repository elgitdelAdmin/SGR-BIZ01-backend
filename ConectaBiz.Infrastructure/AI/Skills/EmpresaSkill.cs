using ConectaBiz.Application.Interfaces;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Infrastructure.AI.Skills
{
    public class EmpresaSkill
    {
        private readonly IEmpresaService _empresaService;
        public EmpresaSkill(IEmpresaService empresaService)
        {
            _empresaService = empresaService;
        }
        [KernelFunction("buscar_empresas_por_nombre")]
        [Description("Busca una empresa por su nombre o razón social para obtener su ID numérico.")]
        public async Task<string> BuscarEmpresasAsync(
            [Description("El nombre de la empresa a buscar (ej. 'Microsoft')")] string nombre)
        {
            var empresas = await _empresaService.GetAllAsync();
            var filtradas = empresas.Where(e => e.RazonSocial != null &&
                                                e.RazonSocial.Contains(nombre, System.StringComparison.OrdinalIgnoreCase)).ToList();
            if (!filtradas.Any()) return $"No se encontró ninguna empresa con la palabra '{nombre}'.";
            var resultado = "Empresas encontradas:\n";
            foreach (var emp in filtradas)
            {
                resultado += $"- ID: {emp.Id}, Nombre: {emp.RazonSocial}\n";
            }
            return resultado;
        }
    }
}

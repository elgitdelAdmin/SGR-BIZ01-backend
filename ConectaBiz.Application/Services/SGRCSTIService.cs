using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Services
{
    public class SGRCSTIService: ISGRCSTIService
    {
        private readonly ISGRCSTIRepository _sgrcstiRepository;
        private readonly IEmpresaRepository _empresaRepository;
        public SGRCSTIService(ISGRCSTIRepository sGRCSTIRepository, IEmpresaRepository empresaRepository)
        {
            _sgrcstiRepository = sGRCSTIRepository;
            _empresaRepository = empresaRepository;
        }
        public async Task MigracionEmpresa()
        {
            var Clientes=await _empresaRepository.GetAllAsync();

            var ClientesSGRCSTI = await _sgrcstiRepository.ObtenerEmpresasByExcepcion(Clientes.Any(x=>x.codSGRCSTI!=null)?  Clientes.Select(x =>(int) x.codSGRCSTI).ToList():null);
        }
    }
}
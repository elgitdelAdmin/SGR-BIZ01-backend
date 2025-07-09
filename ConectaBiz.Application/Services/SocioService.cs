using AutoMapper;
using ConectaBiz.Application.DTOs;
using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Application.Services
{
    public class SocioService : ISocioService
    {
        private readonly ISocioRepository _socioRepository;
        private readonly IMapper _mapper;

        public SocioService(ISocioRepository socioRepository, IMapper mapper)
        {
            _socioRepository = socioRepository;
            _mapper = mapper;
        }

        public async Task<List<SocioDto>> ListarTodosAsync()
        {
            var socios = await _socioRepository.ListarTodosAsync();
            return _mapper.Map<List<SocioDto>>(socios);
        }
    }
}

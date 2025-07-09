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
    public class ModuloService : IModuloService
    {
        private readonly IModuloRepository _repository;
        private readonly IMapper _mapper;

        public ModuloService(IModuloRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<ModuloPermisoDto>> GetAllModulosAsync()
        {
            var modulos = await _repository.GetAllAsync();

            return modulos.Select(m => new ModuloPermisoDto
            {
                Id = m.Id,
                Codigo = m.Codigo,
                Nombre = m.Nombre,
                Icono = m.Icono,
                Ruta = m.Ruta
            }).ToList();
        }
        public async Task<List<ModuloPermisoDto>> GetModulosByRolAsync(int idRol)
        {
            var permisos = await _repository.GetByRolAsync(idRol);
            return _mapper.Map<List<ModuloPermisoDto>>(permisos);
        }
    }

}

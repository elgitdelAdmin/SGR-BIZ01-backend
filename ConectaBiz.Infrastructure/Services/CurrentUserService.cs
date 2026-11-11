using ConectaBiz.Application.Interfaces;
using ConectaBiz.Domain.Constants;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                var idClaim = user?.FindFirst(ClaimTypes.NameIdentifier) 
                           ?? user?.FindFirst("sub") 
                           ?? user?.FindFirst("id");
                
                return idClaim != null ? int.Parse(idClaim.Value) : 0;
            }
        }

        public string Username => 
            _httpContextAccessor.HttpContext?.User?.Identity?.Name 
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("name")?.Value 
            ?? "";

        public string Email => 
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value 
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value 
            ?? "";

        public bool IsAuthenticated => 
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public int IdRol
        {
            get
            {
                var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("idRol");
                return (claim != null && int.TryParse(claim.Value, out var id)) ? id : 0;
            }
        }

        public string CodRol => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ?? "";

        public bool IsSuperAdmin => 
            string.Equals(CodRol, AppConstants.Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase);

        public int? IdSocio
        {
            get
            {
                var socioClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("idSocio");
                return (socioClaim != null && int.TryParse(socioClaim.Value, out var id) && id > 0) ? id : null;
            }
        }

        public int? IdSocioActual => IdSocio;

        public List<int> SociosIds
        {
            get
            {
                var socioClaims = _httpContextAccessor.HttpContext?.User?.FindAll("idSocio");
                if (socioClaims == null || !socioClaims.Any()) return new List<int>();
                return socioClaims.Select(c => int.Parse(c.Value)).ToList();
            }
        }
    }
}

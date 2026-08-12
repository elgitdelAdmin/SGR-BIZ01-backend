using ConectaBiz.Application.Interfaces;
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
                var idClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("id");
                return idClaim != null ? int.Parse(idClaim.Value) : 0;
            }
        }
        public string CodRol => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ?? "";

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

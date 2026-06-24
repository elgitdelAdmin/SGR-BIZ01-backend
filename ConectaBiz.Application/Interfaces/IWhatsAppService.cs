using System.Threading.Tasks;
using ConectaBiz.Application.DTOs;

namespace ConectaBiz.Application.Interfaces
{
    public interface IWhatsAppService
    {
        Task<bool> EnviarWhatsAppAsync(EnviarWhatsAppDto dto);
    }
}

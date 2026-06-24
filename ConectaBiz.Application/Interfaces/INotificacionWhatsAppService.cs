using System.Threading.Tasks;

namespace ConectaBiz.Application.Interfaces
{
    public interface INotificacionWhatsAppService
    {
        Task EnviarNotificacionesWhatsAppAsync();
    }
}

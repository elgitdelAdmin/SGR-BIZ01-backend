using ConectaBiz.Domain.Constants;
using System;

namespace ConectaBiz.Domain.Strategies.CargaMasiva
{
    public class RansaCargaMasivaStrategy : ICargaMasivaEmpresaStrategy
    {
        public bool PuedeResolver(string tipoCarga)
            => tipoCarga == AppConstants.TipoCargaMasiva.TicketsRansa;

        public string NumDocContribuyenteEmpresa
            => AppConstants.Empresas.RansaNumDocContribuyente;

        public string? MapearEstadoACodigo(string estadoExcel)
        {
            return (estadoExcel?.Trim()) switch
            {
                "Queued" => AppConstants.Estados.CERRADO,
                "Closed" => AppConstants.Estados.CERRADO,
                _ => null
            };
        }

        public string? MapearPrioridadANombre(string prioridadExcel)
        {
            return (prioridadExcel?.Trim()) switch
            {
                "Medium"   => "Media",
                "Low"      => "Baja",
                "High"     => "Alta",
                "Critical" => "Critica",
                _ => prioridadExcel?.Trim()
            };
        }

        public string ObtenerCodigoSubTipoTicket(string codigoTicketExcel)
        {
            var codigo = codigoTicketExcel?.Trim() ?? "";
            var codigoCorto = codigo.Length >= 3 ? codigo.Substring(0, 3) : codigo;

            return codigoCorto switch
            {
                AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketRansa.Requerimientos
                    => AppConstants.SubtipoTicket.MesaDeAyuda.Requerimiento,
                AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketRansa.Incidentes
                    => AppConstants.SubtipoTicket.MesaDeAyuda.Incidencia,
                _ => throw new InvalidOperationException($"Subtipo Ransa no reconocido. Código: '{codigo}', Prefijo: '{codigoCorto}'")
            };
        }

        public DateTime ParsearFecha(string fechaTexto, string codTicket)
        {
            return ConectaBiz.Domain.Services.CargaMasivaDataFormatterService.ParsearFechaEstandar(fechaTexto, codTicket);
        }

        public string LimpiarCodTicketInterno(string codTicket)
        {
            return ConectaBiz.Domain.Services.CargaMasivaDataFormatterService.LimpiarCodTicketInternoEstandar(codTicket);
        }
    }
}

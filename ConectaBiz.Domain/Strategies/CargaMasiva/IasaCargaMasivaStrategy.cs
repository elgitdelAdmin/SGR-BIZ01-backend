using ConectaBiz.Domain.Constants;
using System;

namespace ConectaBiz.Domain.Strategies.CargaMasiva
{
    public class IasaCargaMasivaStrategy : ICargaMasivaEmpresaStrategy
    {
        public bool PuedeResolver(string tipoCarga)
            => tipoCarga == AppConstants.TipoCargaMasiva.TicketsIasa;

        public string NumDocContribuyenteEmpresa
            => AppConstants.Empresas.IasaNumDocContribuyente;

        public string? MapearEstadoACodigo(string estadoExcel)
        {
            return (estadoExcel?.Trim()) switch
            {
                "Pending"                        => AppConstants.Estados.CERRADO,
                "En proceso"                     => AppConstants.Estados.CERRADO,
                "Por disponibilidad del usuario" => AppConstants.Estados.CERRADO,
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
            var codigoCorto = codigo.Length >= 9 ? codigo.Substring(0, 9) : codigo;

            return codigoCorto switch
            {
                AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketIasa.Requerimientos
                    => AppConstants.SubtipoTicket.MesaDeAyuda.Requerimiento,
                AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketIasa.Incidentes
                    => AppConstants.SubtipoTicket.MesaDeAyuda.Incidencia,
                _ => throw new InvalidOperationException($"Subtipo IASA no reconocido. Código: '{codigo}', Prefijo: '{codigoCorto}'")
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

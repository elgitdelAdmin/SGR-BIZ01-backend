using ConectaBiz.Domain.Constants;
using System;

namespace ConectaBiz.Domain.Strategies.CargaMasiva
{
    public class ExceliaCargaMasivaStrategy : ICargaMasivaEmpresaStrategy
    {
        public bool PuedeResolver(string tipoCarga)
            => tipoCarga == AppConstants.TipoCargaMasiva.TicketsExcelia;

        public string NumDocContribuyenteEmpresa
            => AppConstants.Empresas.ExceliaNumDocContribuyente;

        public string? MapearEstadoACodigo(string estadoExcel)
        {
            return (estadoExcel?.Trim()) switch
            {
                "Aprobado"  => AppConstants.Estados.CERRADO,
                "Cancelado" => AppConstants.Estados.CERRADO,
                "Cerrado"   => AppConstants.Estados.CERRADO,
                "Pendiente" => AppConstants.Estados.CERRADO,
                "Resuelto"  => AppConstants.Estados.CERRADO,
                _ => null
            };
        }

        public string? MapearPrioridadANombre(string prioridadExcel)
        {
            return (prioridadExcel?.Trim()) switch
            {
                "3" => "Baja",
                "2" => "Media",
                "1" => "Alta",
                _ => prioridadExcel?.Trim()
            };
        }

        public string ObtenerCodigoSubTipoTicket(string codigoTicketExcel)
        {
            var codigo = codigoTicketExcel?.Trim() ?? "";
            var codigoCorto = codigo.Length >= 3 ? codigo.Substring(0, 3) : codigo;

            return codigoCorto switch
            {
                AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketExcelia.Solicitud
                    => AppConstants.SubtipoTicket.MesaDeAyuda.Requerimiento,
                AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketExcelia.Incidentes
                    => AppConstants.SubtipoTicket.MesaDeAyuda.Incidencia,
                _ => throw new InvalidOperationException($"Subtipo Excelia no reconocido. Código: '{codigo}', Prefijo: '{codigoCorto}'")
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

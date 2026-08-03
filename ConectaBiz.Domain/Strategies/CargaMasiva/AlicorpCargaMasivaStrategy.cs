using ConectaBiz.Domain.Constants;
using System;

namespace ConectaBiz.Domain.Strategies.CargaMasiva
{
    public class AlicorpCargaMasivaStrategy : ICargaMasivaEmpresaStrategy
    {
        public bool PuedeResolver(string tipoCarga)
            => tipoCarga == AppConstants.TipoCargaMasiva.TicketsAlicorp;

        public string NumDocContribuyenteEmpresa
            => AppConstants.Empresas.AlicorpNumDocContribuyente;

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
            var partes = prioridadExcel?.Split('-', 2);
            var nombre = partes?.Length == 2 ? partes[1].Trim() : prioridadExcel?.Trim();

            return nombre switch
            {
                "Medio"   => "Media",
                "Baja"    => "Baja",
                "Alta"    => "Alta",
                "Crítica" => "Crítica",
                _ => nombre
            };
        }

        public string ObtenerCodigoSubTipoTicket(string codigoTicketExcel)
        {
            var codigo = codigoTicketExcel?.Trim() ?? "";

            return codigo switch
            {
                var c when c.StartsWith(AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketAlicorp.Requerimientos)
                    => AppConstants.SubtipoTicket.MesaDeAyuda.Requerimiento,
                var c when c.StartsWith(AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketAlicorp.Solicitud)
                    => AppConstants.SubtipoTicket.MesaDeAyuda.Requerimiento,
                var c when c.StartsWith(AppConstants.TipoCargaMasiva.TipoCargaMasivaTicketAlicorp.Incidentes)
                    => AppConstants.SubtipoTicket.MesaDeAyuda.Incidencia,
                _ => throw new InvalidOperationException($"Subtipo Alicorp no reconocido. Código: '{codigo}'")
            };
        }

        public DateTime ParsearFecha(string fechaTexto, string codTicket)
        {
            return Services.CargaMasivaDataFormatterService.ParsearFechaEstandar(fechaTexto, codTicket);
        }

        public string LimpiarCodTicketInterno(string codTicket)
        {
            return Services.CargaMasivaDataFormatterService.LimpiarCodTicketInternoEstandar(codTicket);
        }
    }
}

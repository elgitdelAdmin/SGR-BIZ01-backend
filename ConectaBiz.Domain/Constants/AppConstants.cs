using ConectaBiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Domain.Constants
{
    public static class AppConstants
    {
        public static class TiposParametros
        {
            public const string TipoTicket = "TipoTicket";
            public const string Prioridad = "Prioridad";
            public const string TipoActividad = "TipoActividad";
            public const string TipoDocumento = "TipoDocumento";
            public const string Seniority = "Seniority";
            public const string NivelExperiencia = "NivelExperiencia";
            public const string ModalidadLaboral = "ModalidadLaboral";
            public const string EstadoTicket = "EstadoTicket";
            public const string TipoCargaMasiva = "TipoCargaMasiva";
        }
        public static class TipoTicket
        {
            public const string Incidencia = "INC";
            public const string Requerimiento = "REQ";
            public const string Demanda = "DEM";
            public const string Proyecto = "PROY";
            public const string PreVenta = "PREV";
            public const string Bolsa = "BOL";
        }
        public static class Roles
        {
            public const string SuperAdmin = "SUPERADMIN";
            public const string Admin = "ADMIN";
            public const string GestorCuenta = "GESTORCUENTA";
            public const string GestorConsultoria = "GESTORCONSULTORIA";
            public const string Consultor = "CONSULTOR";
            public const string Empresa = "EMPRESA";
        }
        public static class Estados
        {
            public const string CONSULTORIA_PROCESO = "CONSULTORIA_PROCESO";
            public const string PENDIENTE_APROBACION = "PENDIENTE_APROBACION";
            public const string APROBADO = "APROBADO";
            public const string CERRADO = "CERRADO";
            public const string ANULADO = "ANULADO";
            public const string EN_PRUEBA = "EN_PRUEBA";
            public const string RECHAZADO = "RECHAZADO";
            public const string CANCELADO = "CANCELADO";
            public const string PENDIENTE_SOLUCION = "PENDIENTE_SOLUCION";
            public const string REGISTRADO = "REGISTRADO";
            public const string EN_EJECUCION = "EN_EJECUCION";
            public const string PENDIENTE_ATENCION = "PENDIENTE_ATENCION";
            public const string PENDIENTE_ASIGNACION = "PENDIENTE_ASIGNACION";
            public const string EN_REVISION = "EN_REVISION";
            public const string OBSERVADO = "OBSERVADO";
        }
        public static class TipoCargaMasiva
        {
            public const string IncidentesAlicorp = "INC_ALICORP";
            public const string RequerimientosAlicorp = "REQ_ALICORP";
            public const string TicketsExcelia = "TKT_EXCELIA";
            public const string TicketsRansa = "TKT_RANSA";
            public const string TicketsIasa= "TKT_IASA";

            public static class TipoCargaMasivaTicketExcelia
            {
                public const string Incidentes = "INC";
                public const string Solicitud = "SOL";
            }
            public static class TipoCargaMasivaTicketRansa
            {
                public const string Incidentes = "300";
                public const string Requerimientos = "100";
            }
            public static class TipoCargaMasivaTicketIasa
            {
                public const string Incidentes = "Incidente";
                public const string Requerimientos = "Solicitud";
            }
        }
        public static class TipoActividad
        {
            public const string AnalisisDeRequisitos = "ANREQ";
            public const string Diseno = "DISEN";
            public const string Desarrollo = "DESAR";
            public const string Pruebas = "PRUEB";
            public const string Despliegue = "DESPL";
            public const string Mantenimiento = "MANTE";
            public const string GestionProyecto = "GESTP";
            public const string Documentacion = "DOCUM";
            public const string Calidad = "CALID";
            public const string IncidenciasControles = "INCON";
            public const string RevisionCodigo = "REVCO";
            public const string Planificacion = "PLANF";
            public const string Capacitacion = "CAPAC";
            public const string Monitoreo = "MONIT";
            public const string GestionCambio = "GESCA";
        }

        public static class Empresas
        {
            public const string AlicorpNumDocContribuyente = "20100055237";
            public const string ExceliaNumDocContribuyente = "20100039037";
            public const string RansaNumDocContribuyente = "20100039027";
            public const string IasaNumDocContribuyente = "10232330290";
        }
        public static class Socios
        {
            public const string CstiNumDocContribuyente = "20519339235";
        }
    }
}

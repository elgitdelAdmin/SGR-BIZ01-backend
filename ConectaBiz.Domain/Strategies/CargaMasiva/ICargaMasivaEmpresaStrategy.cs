namespace ConectaBiz.Domain.Strategies.CargaMasiva
{
    public interface ICargaMasivaEmpresaStrategy
    {
        bool PuedeResolver(string tipoCarga);
        string NumDocContribuyenteEmpresa { get; }
        string? MapearEstadoACodigo(string estadoExcel);
        string? MapearPrioridadANombre(string prioridadExcel);
        string ObtenerCodigoSubTipoTicket(string codigoTicketExcel);
        DateTime ParsearFecha(string fechaTexto, string codTicket);
        string LimpiarCodTicketInterno(string codTicket);
    }
}

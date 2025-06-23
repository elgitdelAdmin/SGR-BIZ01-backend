using ConectaBiz.Domain.Constants;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectaBiz.Infrastructure.Persistence.Repositories
{
    public class SGRCSTIRepository : ISGRCSTIRepository
    {
        private readonly string _connectionString;
        public SGRCSTIRepository()
        {
            _connectionString = Conexiones.ConnectionSGRCSTI;
        }
        /*User*/

        public async Task<IEnumerable<Empresa>> ObtenerEmpresasByExcepcion(List<int> excepcion)
        {
            var empresas = new List<Empresa>();

            using (var context = new NpgsqlConnection(_connectionString))
            {


                string query = $@"SELECT razonsocial as RazonSocial,nombrecomercial as NombreComercial,
                                         ruc as NumDocContribuyente ,direccion as Direccion,telefono as Telefono,
                                         estado as Activo,idempresa as CodSgrCsti
                                         FROM Empresas WHERE estado=true {(excepcion!=null && excepcion.Any() ? $" and  ruc not in({string.Join(",", excepcion)} " : "")} ;";
               await context.OpenAsync();
                using (var command = new NpgsqlCommand(query, context))
                {
                    using (var reader = command.ExecuteReader())
                    {

                        while (await reader.ReadAsync())
                        {
                            var empresa = new Empresa
                            {
                                codSGRCSTI = reader.IsDBNull(reader.GetOrdinal("CodSgrCsti")) ? null : reader.GetInt32(reader.GetOrdinal("CodSgrCsti")),
                                RazonSocial = reader.GetString(reader.GetOrdinal("RazonSocial")),
                                NombreComercial = reader.IsDBNull(reader.GetOrdinal("NombreComercial")) ? null : reader.GetString(reader.GetOrdinal("NombreComercial")),
                                NumDocContribuyente = reader.IsDBNull(reader.GetOrdinal("NumDocContribuyente")) ? null : reader.GetString(reader.GetOrdinal("NumDocContribuyente")),
                                Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) ? null : reader.GetString(reader.GetOrdinal("Direccion")),
                                Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString(reader.GetOrdinal("Telefono")),
                                Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),

                            };
                            empresas.Add(empresa);
                        }
                    }
                }

            }
            return empresas;
        }
    }
}

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
                                         FROM Empresa WHERE estado=true {(excepcion!=null && excepcion.Any() ? $" and  ruc not in({string.Join(",", excepcion)} " : "")} ;";
               await context.OpenAsync();
                using (var command = new NpgsqlCommand(query, context))
                {
                    using (var reader = command.ExecuteReader())
                    {

                        while (await reader.ReadAsync())
                        {
                            var empresa = new Empresa
                            {
                                CodSgrCsti = reader.IsDBNull(reader.GetOrdinal("CodSgrCsti")) ? null : reader.GetInt32(reader.GetOrdinal("CodSgrCsti")),
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

        public async Task<IEnumerable<dynamic>> MigracionRequerimientos()
        {
            var resultados = new List<dynamic>();
            using (var context = new NpgsqlConnection(_connectionString))
            {
                string query = @"select idrequerimiento, codrequerimiento, titulo, r.fecharegistro, id_tipo_servicio,
                r.idestadorequerimiento, r.idempresa, r.idusuario, r.idprioridad, detalle, idrequerimiento, codrequerimiento,
                r.fecharegistro, --requerimiento
                ts.id as tipo_servicio_id, ts.codigo as tipo_servicio_codigo, ts.descripcion as tipo_servicio_descripcion, -- tipo servicio
                e.idestadorequerimiento  as estadorequerimiento_idestadorequerimiento, e.descripcion as estadorequerimiento_estadorequerimiento, -- estado
                u.nombres as ResponsableCliente_nombres,
                u.apematerno as ResponsableCliente_apematerno,
                u.apepaterno as ResponsableCliente_apepaterno,
                u.tipodocumento as ResponsableCliente_tipodocumento,
                u.idtipodocumento as ResponsableCliente_idtipodocumento,
                u.ruc as ResponsableCliente_documento,
                u.telefonomovil as ResponsableCliente_telefonomovil,
                u.direccion as ResponsableCliente_direccion,
                u.fechanacimiento as ResponsableCliente_fechanacimiento,
                u.fecharegistro as ResponsableCliente_fecharegistro,
                u.fechamodificacion as ResponsableCliente_fechamodificacion,
                u.correo as ResponsableCliente_correo,
                u.fijo as ResponsableCliente_fijo,
                u.idusuario as ResponsableCliente_idusuario,--usuario cliente
                p.idprioridad as prioridad_idprioridad,
                p.descripcion as prioridad_descripcion,
                e2.razonsocial as Empresa_razonsocial,
                e2.nombrecomercial as Empresa_nombrecomercial,
                e2.ruc as Empresa_ruc,
                e2.direccion as Empresa_direccion,
                e2.telefono as Empresa_telefono,
                e2.idempresa as Empresa_idempresa
                from requerimiento r 
                left join estadorequerimiento e on e.idestadorequerimiento  = r.idestadorequerimiento 
                left join tipo_servicio ts  on r.id_tipo_servicio =  ts.id 
                left join usuario u on r.idusuario = u.idusuario
                left join prioridad p on r.idprioridad = p.idprioridad
                left join  empresa e2 on r.idempresa  = e2.idempresa 
                --where e.idestadorequerimiento in (-3) AND r.fecharegistro >= '2025-06-18'
                where e.idestadorequerimiento in (-3) and r.fecharegistro >= NOW() - INTERVAL '20 minutes'
                --LIMIT 2;
                ";

                await context.OpenAsync();
                using (var command = new NpgsqlCommand(query, context))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            dynamic item = new ExpandoObject();
                            var dict = (IDictionary<string, object>)item;
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                dict[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                            }
                            resultados.Add(item);
                        }
                    }
                }
            }
            return resultados;
        }
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace api.Controllers
{
    public class facturasController : ApiController
    {
        public string getForCombobox()
        {
            /*if (!utilidades.validar_token(Request))
                return Json("incorrecto");*/
            string query = "SELECT id, razon_social from lu_clientes where estado=1 order by razon_social;";
            DataTable tabla = Database.runSelectQuery(query);
            return utilidades.convertDataTableToJson(tabla);
        }

        public string getSearchByPage()
        {
            //Declaración de encabezados
            IEnumerable<string> headerValues = Request.Headers.GetValues("pagina");
            string string_pagina = headerValues.FirstOrDefault().ToString();
            int pagina = int.Parse(string_pagina);

            IEnumerable<string> headerValues_nombre = Request.Headers.GetValues("nombre");
            string nombre = headerValues_nombre.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
            string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

            //if ((id_tipo_de_usuario != "1"))
                //return "Incorrecto";


            string query = string.Format("SELECT  a.id " +
            "    , DATE_FORMAT(a.fecha, '%d/%m/%Y') as fecha_de_factura " + 
            "    , a.folio as folio " +             
            "    , b.razon_social as cliente     " +             
            //"    , a.importe " + 
            //"    , a.iva " + 
            "    , a.total     " + 
            //"    , a.id_estado_de_factura " + 
            "    , e.nombre as estado_de_factura " + 
            "FROM `ft_facturas` a  " + 
            "left join lu_clientes b on a.id_cliente=b.id " +             
            "left join cf_estados_de_factura e on a.id_estado_de_factura=e.id " +
            "where a.estado=1   " +
            "" + //Otras condiciones para el Where
            "group by a.id   " +
            "HAVING cliente like '%{2}%'   " +    
            "OR folio like '%{2}%'" + 
            "order by a.fecha_de_modificacion desc limit {0} offset {1};  "
                , utilidades.elementos_por_pagina
                , ((pagina - 1) * (utilidades.elementos_por_pagina - 1))
                , nombre);

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            return utilidades.convertDataTableToJson(tabla_resultado);
        }


        public string postResumenMensualPorCliente([FromBody]Object value)
        {
            //Declaración de encabezados            

            JObject json = JObject.Parse(value.ToString());

            /*IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
            string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

            //if ((id_tipo_de_usuario != "1"))
                return "Incorrecto";*/
            string [] arreglo = json["mes"].ToString().Replace("'", "''").Split('-');
            int año = int.Parse(arreglo[0]);
            int mes = int.Parse(arreglo[1]); 

            DateTime primer_dia = new DateTime(año, mes, 1);
            DateTime ultimo_dia = primer_dia.AddMonths(1).AddDays(-1);

            string query = string.Format("select  " +
                            "sum(b.total) as suma  " +
                            ", a.coloreado " + 
                            ", a.nombre as estado  " +
                            " from cf_estados_de_factura a  " +
                            "left join ft_facturas b on (a.id=b.id_estado_de_factura  " +
                            "and b.id_cliente='{0}' " +
                            "and b.fecha>= date_format('{1} 00:00:00', '%Y-%m-%d %H:%i:%s')  " +
                            "and b.fecha<=date_format('{2} 23:59:59', '%Y-%m-%d %H:%i:%s')) " +
                            "group by a.id;  " 
                , json["id_cliente"].ToString().Replace("'", "''")
                , primer_dia.ToString("yyyy-MM-dd")
                , ultimo_dia.ToString("yyyy-MM-dd")); 

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);
            if (tabla_resultado == null)
                return "incorrecto";
            for (int x = 0; x < tabla_resultado.Rows.Count; x++ )
            {
                if(tabla_resultado.Rows[x]["suma"].ToString()=="")
                {
                    tabla_resultado.Rows[x]["suma"] = 0; 

                }

            }

                return utilidades.convertDataTableToJson(tabla_resultado);
        }

        public string Post(int id, [FromBody]Object value)
        {
            //Lo que viene en value es lo que nos manda el usuario a través del body de postman. 
            JObject json = JObject.Parse(value.ToString());

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `ft_facturas` " +
            "SET " +
            "fecha=STR_TO_DATE('{0}', '%Y-%m-%d'), " +            
            "`folio` = '{1}', " +
            "`id_cliente` = '{2}', " +
            "`id_producto` = '{3}', " +
            "`importe` = '{4}', " +
            "`iva` = '{5}', " +
            "`total` = '{6}', " +
            "`id_estado_de_factura` = '{7}' " +
            "WHERE `id` = '{8}';"            
            , json["fecha"].ToString().Replace("'", "''")
            , json["folio"].ToString().Replace("'", "''")
            , json["id_cliente"].ToString().Replace("'", "''")
            , json["id_producto"].ToString().Replace("'", "''")
            , json["importe"].ToString().Replace("'", "''")
            , json["iva"].ToString().Replace("'", "''")
            , json["total"].ToString().Replace("'", "''")
            , json["id_estado_de_factura"].ToString().Replace("'", "''")
             , id);

            //Contestamos con el id del nuevo registro.
            if (Database.runQuery(update_query))
                return "correcto";
            else
                return "incorrecto";
        }

        public string PostDelete(int id)
        {
            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `ft_facturas` " +
             "set " +
            "estado = '0' " +
            "where id='{0}'; "
             , id);

            //Contestamos con el id del nuevo registro.
            if (Database.runQuery(update_query))
                return "correcto";
            else
                return "incorrecto";
        }

        public string Post([FromBody]Object value)
        {
            try
            {
                //Aquí se lo dejo joven: http://www.objgen.com/json?demo=true

                DataTable tabla_resultado = new DataTable();
                tabla_resultado.Columns.Add("id");
                tabla_resultado.Rows.Add();
                tabla_resultado.Rows[0]["id"] = "-1";

                JObject json = JObject.Parse(value.ToString());

                //",fecha_de_nacimiento=STR_TO_DATE('{1}', '%Y-%m-%d')" +

                //Actualizamos los datos con un update query. 
                string insert_query = string.Format("INSERT INTO `ft_facturas`" +
                "(`fecha`," +
                "`folio`," +
                "`id_cliente`," +
                "`id_producto`," +
                "`importe`," +
                "`iva`," +
                "`total`," +
                "`id_estado_de_factura`)" +
                "VALUES" +
                "(STR_TO_DATE('{0}', '%Y-%m-%d')," +
                "'{1}'," +
                "'{2}'," +
                "'{3}'," +
                "'{4}'," +
                "'{5}'," +
                "'{6}'," +
                "'{7}');"
                , json["fecha"].ToString().Replace("'", "''")
                , json["folio"].ToString().Replace("'", "''")
                , json["id_cliente"].ToString().Replace("'", "''")
                , json["id_producto"].ToString().Replace("'", "''")
                , json["importe"].ToString().Replace("'", "''")
                , json["iva"].ToString().Replace("'", "''")
                , json["total"].ToString().Replace("'", "''")
                , json["id_estado_de_factura"].ToString().Replace("'", "''"));

                //En caso de error, devolverá incorrecto
                tabla_resultado.Rows[0]["id"] = Database.runInsert(insert_query).ToString();
                if (tabla_resultado.Rows[0]["id"].ToString() == "-1")
                    return "incorrecto";

                //DevolVemos la información de la tabla. 
                return utilidades.convertDataTableToJson(tabla_resultado);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        public string Get(int id)
        {
            //Encabezados
            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
            string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

            //En caso de ser criador un usuario no Administrador, no le regresamos nada.
            //if ((id_tipo_de_usuario != "1"))
                //return "Incorrecto";

            //", DATE_FORMAT(a.fecha_de_nacimiento, '%d/%m/%Y') AS fecha_de_nacimiento " +
            //Utilizaré la variable estatica (global) de la clase de utilidades y el número de la página que me solicitan. 
            string query = string.Format("select " +
            " a.id " +
            ", DATE_FORMAT(a.fecha, '%Y-%m-%d') AS fecha " +
            ", a.folio " +
            ", a.id_cliente " +
            ", a.id_producto " +
            ", a.importe " +
            ", a.iva " +
            ", a.total " +
            ", a.id_estado_de_factura " +
            ", a.estado " + 
            "from ft_facturas a " +
            "where a.id='{0}' "
                , id);

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);


            return utilidades.convertDataTableToJson(tabla_resultado);
        }

        public string GetPagos(int id)
        {
            //Encabezados
            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
            string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

            //En caso de ser criador un usuario no Administrador, no le regresamos nada.
            //if ((id_tipo_de_usuario != "1"))
                //return "Incorrecto";

            //", DATE_FORMAT(a.fecha_de_nacimiento, '%d/%m/%Y') AS fecha_de_nacimiento " +
            //Utilizaré la variable estatica (global) de la clase de utilidades y el número de la página que me solicitan. 
            string query = string.Format("select  " +
                "a.id as id " +
                ", a.fecha as fecha " +
                ", a.importe  " +
                ", b.nombre as banco  " +
                "from ft_pagos a  " +
                "left join cf_bancos b on b.id=a.id_banco " +
                "where a.id_factura='{0}' and a.estado=1;  "
                , id);

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            return utilidades.convertDataTableToJson(tabla_resultado);
        }

        public string GetProductos(int id)
        {
            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
            string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

            //En caso de ser criador un usuario no Administrador, no le regresamos nada.
            //if ((id_tipo_de_usuario != "1"))
                //return "Incorrecto";

            //", DATE_FORMAT(a.fecha_de_nacimiento, '%d/%m/%Y') AS fecha_de_nacimiento " +
            //Utilizaré la variable estatica (global) de la clase de utilidades y el número de la página que me solicitan. 
            string query = string.Format("select  " +
                                            "a.id " +
                                            ", a.id_factura " +
                                            ", a.cantidad " +
                                            ", b.nombre as producto  " +
                                            ", c.nombre as unidad " +
                                            ", concat('$', b.valor_unitario) as valor_unitario " +
                                            "from r_facturas_productos a " +
                                            "left join lu_productos b on a.id_producto=b.id " +
                                            "left join lu_unidades c on c.id=b.id_unidad " +
                                            "where a.estado=1 and a.id_factura='{0}';" 
                                            , id);

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            return utilidades.convertDataTableToJson(tabla_resultado);
        }

        public string precargarEnLote([FromBody]Object value) 
        {
            byte[] bytes = Encoding.Default.GetBytes(value.ToString());
            string cadena = Encoding.UTF8.GetString(bytes);

            DataTable tabla_resultado = new DataTable();
            tabla_resultado.Columns.Add("cliente", typeof(string));
            tabla_resultado.Columns.Add("folio", typeof(string));
            tabla_resultado.Columns.Add("concepto", typeof(string));
            tabla_resultado.Columns.Add("estado_de_registro", typeof(string));
            tabla_resultado.Columns.Add("total", typeof(string));

            DataTable tabla_contable = (DataTable)JsonConvert.DeserializeObject(cadena, (typeof(DataTable)));

            string select_clientes = "SELECT a.id as id_cliente " +
                ", a.razon_social " +
                ", a.RFC " +
                ", '' as total " +
                ", '' as subtotal " +
                ", '' as iva " +
                ", '' as concepto " +
                ", '' as folio " +
                ", '' as fecha_de_emision " +
                ", '' as id_estado_de_factura " +
                ", '' as estado_de_registro " + 
                "FROM lu_clientes a where a.estado=1;";
            DataTable tabla_clientes = Database.runSelectQuery(select_clientes);

            bool encontrado = false; 
            //Iteramos con los valores de la tabla contable. 
            for (int x = 0; x < tabla_contable.Rows.Count; x++ )
            {
                encontrado = false; 
                //Obtenemos el RFC de la tabla.
                for (int y = 0; y < tabla_clientes.Rows.Count; y++)
                {
                    //Hacemos el insert de la factura. 
                    if(tabla_clientes.Rows[y]["RFC"].ToString()==tabla_contable.Rows[x]["RFC Receptor"].ToString())
                    {
                        encontrado = true; 
                        //Copiamos todos los datos de la factura. 
                        //switch (tabla_contable.Rows[x]["Estado SAT"].ToString().ToLower().Trim())
                        switch (tabla_contable.Rows[x][1].ToString().ToLower().Trim())
                        {
                            case "vigente":
                                tabla_clientes.Rows[y]["id_estado_de_factura"] = "1"; 
                                break;
                            case "cancelado":
                                tabla_clientes.Rows[y]["id_estado_de_factura"] = "2"; 
                                break;
                            case "pagado":
                                tabla_clientes.Rows[y]["id_estado_de_factura"] = "3"; 
                                break;
                            default:
                                //El caso esta sin identificar. Se coloca un nuevo estado de carga y se sube la información. 
                                break;
                        }

                        /*tabla_clientes.Rows[y]["total"] = tabla_contable.Rows[x][" Total "];
                        tabla_clientes.Rows[y]["subtotal"] = tabla_contable.Rows[x][" SubTotal "];
                        tabla_clientes.Rows[y]["iva"] = tabla_contable.Rows[x][" IVA 16% "];
                        tabla_clientes.Rows[y]["concepto"] = tabla_contable.Rows[x]["Conceptos"];
                        tabla_clientes.Rows[y]["folio"] = tabla_contable.Rows[x]["Folio"];
                        tabla_clientes.Rows[y]["fecha_de_emision"] = tabla_contable.Rows[x]["Fecha Emision"];
                        tabla_clientes.Rows[y]["estado_sat"] = tabla_contable.Rows[x]["Estado SAT"]; */

                        tabla_clientes.Rows[y]["total"] = tabla_contable.Rows[x][27];
                        tabla_clientes.Rows[y]["subtotal"] = tabla_contable.Rows[x][20];
                        tabla_clientes.Rows[y]["iva"] = tabla_contable.Rows[x][23];
                        tabla_clientes.Rows[y]["concepto"] = tabla_contable.Rows[x][40];
                        tabla_clientes.Rows[y]["folio"] = tabla_contable.Rows[x][9];
                        tabla_clientes.Rows[y]["fecha_de_emision"] = tabla_contable.Rows[x][4];
                        //tabla_clientes.Rows[y]["estado_sat"] = tabla_contable.Rows[x][1];

                        //Agregamos la información a la tabla del resultado. 
                        tabla_resultado.Rows.Add();
                        tabla_resultado.Rows[tabla_resultado.Rows.Count - 1]["total"] = tabla_contable.Rows[x][27];
                        tabla_resultado.Rows[tabla_resultado.Rows.Count - 1]["concepto"] = tabla_clientes.Rows[y]["concepto"];
                        tabla_resultado.Rows[tabla_resultado.Rows.Count - 1]["folio"] = tabla_clientes.Rows[y]["folio"];
                        tabla_resultado.Rows[tabla_resultado.Rows.Count - 1]["cliente"] = tabla_clientes.Rows[y]["razon_social"].ToString(); 

                        try
                        {
                            //Diseñamos el insert query. 
                            string insert_query = string.Format("INSERT INTO ft_facturas ( " +
                                "fecha " +
                                ", folio " +
                                ", id_cliente " +
                                ", concepto " +
                                ", importe " +
                                ", iva " +
                                ", total " +
                                ", id_estado_de_factura) VALUES( " +
                                    "STR_TO_DATE('{0}', '%Y-%m-%d') " +
                                    ", '{1}' " +
                                    ", '{2}' " +
                                    ", '{3}' " +
                                    ", '{4}' " +
                                    ", '{5}' " +
                                    ", '{6}' " +
                                    ", '{7}');"
                                    , DateTime.Parse(tabla_clientes.Rows[y]["fecha_de_emision"].ToString()).ToString("yyyy-MM-dd")
                                    , tabla_clientes.Rows[y]["folio"].ToString().Replace("'", "''").Trim()
                                    , tabla_clientes.Rows[y]["id_cliente"].ToString().Replace("'", "''").Trim()
                                    , tabla_clientes.Rows[y]["concepto"].ToString().Replace("'", "''").Trim()
                                    , tabla_clientes.Rows[y]["subtotal"].ToString().Replace("'", "''").Trim().Replace(",", "")
                                    , tabla_clientes.Rows[y]["iva"].ToString().Replace("'", "''").Trim().Replace(",", "")
                                    , tabla_clientes.Rows[y]["total"].ToString().Replace("'", "''").Trim().Replace(",", "")
                                    , tabla_clientes.Rows[y]["id_estado_de_factura"]);

                            if (Database.runInsert(insert_query) != -1)
                                tabla_resultado.Rows[tabla_resultado.Rows.Count - 1]["estado_de_registro"] = "Cargado"; 
                            else
                                tabla_resultado.Rows[tabla_resultado.Rows.Count - 1]["estado_de_registro"] = "Error"; 

                            //Falta hacer la validación para el folio repetido. 
                        }
                        catch(Exception ex)
                        {
                            tabla_resultado.Rows[tabla_resultado.Rows.Count - 1]["estado_de_registro"] = "Error";
                        }
                        
                    }

                }
                if(!encontrado)
                {
                    tabla_resultado.Rows.Add();
                    tabla_resultado.Rows[tabla_resultado.Rows.Count - 1]["total"] = tabla_contable.Rows[x][27];
                    tabla_resultado.Rows[tabla_resultado.Rows.Count - 1]["concepto"] = tabla_contable.Rows[x][40];
                    tabla_resultado.Rows[tabla_resultado.Rows.Count - 1]["folio"] = tabla_contable.Rows[x][9];
                    tabla_resultado.Rows[tabla_resultado.Rows.Count - 1]["cliente"] = tabla_contable.Rows[x][16].ToString();
                    tabla_resultado.Rows[tabla_resultado.Rows.Count - 1]["estado_de_registro"] = "Error: Cliente no encontrado en la base de datos.";
                }

                //Regresamos la tabla con el repote de carga. 

                //Estado: Cargado, No cargado 
                //Mensaje: El cliente no existe, folio repetido, error en los tipos de dato. 
            }

            return utilidades.convertDataTableToJson(tabla_resultado);
        }
        public string uploadImage(int id, [FromBody]Object value)
        {
            try
            {
                //Tomar en cuenta que las fechas vienen en el formato YYYY-MM-dd
                JObject json = JObject.Parse(value.ToString());

                string filename = string.Format("{0}.jpg", id);
                utilidades.guardar_imagen(json["foto_url"].ToString().Replace("'", "''").ToString(), "clientes", filename);

                string foto_url = "http://" + Request.Headers.Host + "/temp/clientes/" + filename;

                foto_url += "?fecha=" + DateTime.Now.ToString("ddMMyyyy_HHmmss");

                //Actualizamos el campo de foto_url de la mascota.             
                string update_query = string.Format("UPDATE `lu_clientes` " +
               "set " +
               "foto_url='{0}' " +
               "where id='{1}'"
               , foto_url
               , id);

                //Contestamos con el id del nuevo registro.
                if (Database.runQuery(update_query))
                    return "correcto";
                else
                    return "incorrecto";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
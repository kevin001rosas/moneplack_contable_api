using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace api.Controllers
{
    public class pagosController : ApiController

    {
        public string getForCombobox()
        {
            /*if (!utilidades.validar_token(Request))
                return Json("incorrecto");*/
            string query = "SELECT id, razon_social from ft_pagos where estado=1 order by razon_social;";
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


            string query = string.Format("select a.id " +
            ", a.razon_social " +
            ", a.direccion " +
            ", a.email " +
            ", a.foto_url " +
            "from ft_pagos a " +
            "where a.estado=1   " +
            "" + //Otras condiciones para el Where
            "group by a.id   " +
            "HAVING a.email like '%{2}%'   " +
            "OR razon_social like '%{2}%' " +
            "OR direccion like '%{2}%' " +
            "order by a.fecha_de_modificacion desc limit {0} offset {1};  "
                , utilidades.elementos_por_pagina
                , ((pagina - 1) * (utilidades.elementos_por_pagina - 1))
                , nombre);

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            return utilidades.convertDataTableToJson(tabla_resultado);
        }

        public string getSearchByPageOtrosPagos()
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


            string query = string.Format("select " +
            "a.id " +
            ", DATE_FORMAT(a.fecha, '%d/%m/%Y') as fecha " + 
            ", concat('$' , a.importe) as importe " +
            ", b.nombre as banco " +            
            "from ft_pagos a " +
            "LEFT JOIN cf_bancos b on a.id_banco=b.id " +
            "where a.estado=1 and (a.id_factura=-1 or a.id_factura is null) " +
            "" + //Otras condiciones para el Where
            "group by a.id   " +
            "HAVING banco like '%{2}%'   " +
            "OR importe like '%{2}%' " +
            "order by a.fecha_de_modificacion desc limit {0} offset {1};  "
                , utilidades.elementos_por_pagina
                , ((pagina - 1) * (utilidades.elementos_por_pagina - 1))
                , nombre);

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            return utilidades.convertDataTableToJson(tabla_resultado);
        }

        public string Post(int id, [FromBody]Object value)
        {
            //Lo que viene en value es lo que nos manda el usuario a través del body de postman. 
            JObject json = JObject.Parse(value.ToString());

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `ft_pagos` " +
             "set " +
            "fecha = STR_TO_DATE('{0}', '%Y-%m-%d') " +
            ", importe  = '{1}' " +            
            ", id_banco = '{2}' " +
            "where id='{3}' "
            , json["fecha"].ToString().Replace("'", "''")
            , json["importe"].ToString().Replace("'", "''")
            , json["id_banco"].ToString().Replace("'", "''")
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
            string update_query = string.Format("UPDATE `ft_pagos` " +
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

                //Actualizamos los datos con un update query. 
                string insert_query = string.Format("INSERT INTO `ft_pagos` " + 
                "(`fecha`, " + 
                "`importe`, " + 
                "`id_factura`, " +                 
                "`id_banco`) " + 
                "VALUES " + 
                "(STR_TO_DATE('{0}', '%Y-%m-%d'),  " + 
                "'{1}',  " +                                 
                "'{2}',  " + 
                "'{3}');  " 
                    , json["fecha"].ToString().Replace("'", "''")
                    , json["importe"].ToString().Replace("'", "''")
                    , json["id_factura"].ToString().Replace("'", "''")
                    , json["id_banco"].ToString().Replace("'", "''"));

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

            //Utilizaré la variable estatica (global) de la clase de utilidades y el número de la página que me solicitan. 
            string query = string.Format("select " +
            "a.id " +
            ", a.fecha " +
            ", a.id_banco " +
            ", b.nombre as banco " +
            ", a.importe " +            
            "from ft_pagos a " +
            "LEFT JOIN cf_bancos b on a.id_banco=b.id " +
            "where a.id='{0}' "
                , id);

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);


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
                string update_query = string.Format("UPDATE `ft_pagos` " +
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
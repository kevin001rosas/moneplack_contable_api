using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.OleDb;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace api.Controllers
{
    public class documentosController : ApiController
    {
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

            if ((id_tipo_de_usuario != "1"))
                return "Incorrecto";


            string query = string.Format("select " +
            "id " +
            ", a.nombre " +
            ", a.descripcion " +
            ", a.file_url " +
            ", a.fecha_de_registro " +
            ", a.fecha_de_modificacion " +
            "from lu_documentos a where " +
            "(a.nombre like '%{2}%' " +
            "or a.descripcion like '%{2}%') " +
            "and a.estado=1 " +
            "order by a.fecha_de_modificacion desc limit {0} offset {1};  "
                , utilidades.elementos_por_pagina
                , ((pagina - 1) * (utilidades.elementos_por_pagina - 1))
                , nombre);

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            return utilidades.convertDataTableToJson(tabla_resultado);
        }
    }
}

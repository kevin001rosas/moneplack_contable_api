using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace api.Controllers
{
    public class tiposdeusuarioController : ApiController
    {
        public string Get()
        {
            /*if (!utilidades.validar_token(Request))
                return Json("incorrecto");*/
            string query = "SELECT id, nombre from cf_tipos_de_usuario where estado=1 order by nombre;";
            DataTable tabla = Database.runSelectQuery(query);
            return utilidades.convertDataTableToJson(tabla);
        }

        // GET api/ciudades/5
        public string Get(int id)
        {
            if (!utilidades.validar_token(Request))
                return "incorrecto";

            string query = string.Format("SELECT * from cf_tipos_de_usuario where id='{0}'", id);
            DataTable tabla = Database.runSelectQuery(query);
            return utilidades.convertDataTableToJson(tabla);
        }
    }
}

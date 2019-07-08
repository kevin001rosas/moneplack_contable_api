using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace api.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public string Get()
        {
            DataTable tabla = Database.runSelectQuery("SELECT * FROM lu_usuarios where id=1");
            string json = utilidades.convertDataTableToJson(tabla);
            return json;
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        public void Post([FromBody]Object value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
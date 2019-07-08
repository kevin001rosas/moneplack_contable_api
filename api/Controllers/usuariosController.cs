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
    public class usuariosController : ApiController
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
            "a.id   " +
            ", a.nombre_de_usuario " +
            ", a.APATERNO " +
            ", a.AMATERNO " +
            ", a.NOMBRE " +
            ", a.email " + //Conservar 
            ", a.foto_url " +
            ", concat(a.NOMBRE, ' ' , a.APATERNO,  ' ' ,a.AMATERNO) as usuario   " + //Conservar
            ", b.nombre as tipo_de_usuario " +  //Convervar 
            "from lu_usuarios a " +
            "left join cf_tipos_de_usuario b on b.id = a.id_tipo_de_usuario " +
            "where a.estado=1   " +
            "" + //Otras condiciones para el Where
            "group by a.id   " +
            "HAVING usuario like '%{2}%'   " +
            "OR a.email like '%{2}%'   " +
            "OR b.nombre like '%{2}%'   " +
            "OR tipo_de_usuario like '%{2}%' " +
            "order by a.FECHA_MODIFICACION desc limit {0} offset {1};  "
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
            string update_query = string.Format("UPDATE `lu_usuarios` " +
             "set " +
             "nombre_de_usuario='{0}' " +
             ",secret='{1}' " +
             ",email='{2}' " +
             ",id_tipo_de_usuario='{3}' " +
                // ",foto_url='{4}' " +
             "where id='{4}'"
             , json["nombre_de_usuario"].ToString().Replace("'", "''")
             , json["secret"].ToString().Replace("'", "''")
             , json["email"].ToString().Replace("'", "''")
             , json["id_tipo_de_usuario"].ToString().Replace("'", "''")
                // , json["foto_url"].ToString().Replace("'", "''")
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
                string insert_query = string.Format("INSERT INTO `lu_usuarios` " +
                "(`nombre_de_usuario`," +
                    "`APATERNO`," +
                    "`AMATERNO`," +
                    "`NOMBRE`," +
                    "`secret`," +
                    "`email`," +
                    "`id_tipo_de_usuario`) " +
                "VALUES " +
                "('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');"
                    , json["nombre_de_usuario"].ToString().Replace("'", "''")
                    , json["APATERNO"].ToString().Replace("'", "''")
                    , json["AMATERNO"].ToString().Replace("'", "''")
                    , json["NOMBRE"].ToString().Replace("'", "''")
                    , json["secret"].ToString().Replace("'", "''")
                    , json["email"].ToString().Replace("'", "''")
                    , json["id_tipo_de_usuario"].ToString().Replace("'", "''"));

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
            if ((id_tipo_de_usuario != "1"))
                return "Incorrecto";

            //Utilizaré la variable estatica (global) de la clase de utilidades y el número de la página que me solicitan. 
            string query = string.Format("select " +
            "nombre_de_usuario, " +
            "APATERNO, " +
            "AMATERNO, " +
            "NOMBRE, " +
            "secret, " +
            "email, " +
            "id_tipo_de_usuario, " +
            "foto_url, " +
            "FECHA_REGISTRO, " +
            "FECHA_MODIFICACION, " +
            "CLAVE_EMPLEADO, " +
            "NSS, " +
            "RFC, " +
            "CURP, " +
            "AREA, " +
            "DEPARTAMENTO, " +
            "BORRADO, " +
            "FECHA_INGRESO, " +
            "FECHA_BAJA " +
            "from lu_usuarios a " +
            "where a.id='{0}' and a.estado='1' "
                , id);


            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);


            return utilidades.convertDataTableToJson(tabla_resultado);
        }       

        public string getIniciarSesion()
        {
            //Obtenemos el nombre de usuario y contraseña. 
            IEnumerable<string> headerValues_secret = Request.Headers.GetValues("secret");
            string secret = headerValues_secret.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_nombre_de_usuario = Request.Headers.GetValues("nombre_de_usuario");
            string nombre_de_usuario = headerValues_nombre_de_usuario.FirstOrDefault().ToString();

            //
            string query = string.Format("select  " +
                                        "a.* " +
                                        ", a.id as id_usuario " +
                                        ", a.id_tipo_de_usuario as id_tipo_de_usuario" +
                                        ", '' as token " +
                                        ", concat(a.NOMBRE) as nombres_de_usuario " +
                                        ", b.nombre as tipo_de_usuario " +                                                                                                                        
                                        "from lu_usuarios a " +
                                        "LEFT JOIN cf_tipos_de_usuario b on b.id=a.id_tipo_de_usuario " +                                        
                                        "where  " +
                                        "nombre_de_usuario='{0}'  " +
                                        "and binary secret='{1}'; " //Solo traermos a los usuarios activos. 
                                         , nombre_de_usuario
                                         , secret);

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            if (tabla_resultado == null)
                return "vacio";


            //Generamos el Token y lo guardamos.
            string token = utilidades.generar_token(tabla_resultado.Rows[0]["id_usuario"].ToString()
                , nombre_de_usuario);
            tabla_resultado.Rows[0]["token"] = token;

            return utilidades.convertDataTableToJson(tabla_resultado);
        }

        public string getMigrarUsuarios()
        {
            string query_sql_server = "SELECT " +
                            "a.CLAVE_EMPLEADO as CLAVE_EMPLEADO " +
                            ", b.FECHA_MODIFICACION as FECHA_MODIFICACION " +
                            "FROM PERSONAS a  " +
                            "LEFT JOIN EMPLEADOS b on a.CLAVE_EMPLEADO=b.CLAVE_EMPL;";

            string query_mysql = "SELECT CLAVE_EMPLEADO AS CLAVE_EMPLEADO " +
                ", FECHA_MODIFICACION AS FECHA_MODIFICACION " +
                " FROM lu_usuarios; ";

            //Obtenemos las tablas
            DataTable tabla_sql_server = DatabaseChecado.runSelectQuery(query_sql_server);
            DataTable tabla_mysql = Database.runSelectQuery(query_mysql);

            if (tabla_mysql == null)
                tabla_mysql = new DataTable(); 
            
            //Buscamos las claves dentro de la tabla de MySQL 
            for (int x = 0; x < tabla_sql_server.Rows.Count; x++ )
            {   
                bool encontrado = false; 
                for (int y = 0; y < tabla_mysql.Rows.Count; y++ )
                {
                    try
                    {
                        if (tabla_sql_server.Rows[x]["CLAVE_EMPLEADO"].ToString()
                            == tabla_mysql.Rows[y]["CLAVE_EMPLEADO"].ToString())
                        {
                            encontrado = true;
                            string fecha_sql_server = tabla_sql_server.Rows[x]["FECHA_MODIFICACION"].ToString() == "" ? DateTime.Now.ToString("dd/MM/yyyy")
                                : DateTime.Parse(tabla_sql_server.Rows[x]["FECHA_MODIFICACION"].ToString()).ToString("dd/MM/yyyy");
                            string fecha_mysql = tabla_mysql.Rows[y]["FECHA_MODIFICACION"].ToString() == "" ? DateTime.Now.ToString("dd/MM/yyyy")
                                : DateTime.Parse(tabla_mysql.Rows[y]["FECHA_MODIFICACION"].ToString()).ToString("dd/MM/yyyy");

                            if (fecha_sql_server != fecha_mysql)
                            {
                                //Hacemos el update 
                                //Le mandamos la clave de empleado que vamos a insertar. 
                                update_usuario(tabla_sql_server.Rows[x]["CLAVE_EMPLEADO"].ToString());
                            }
                            break;
                        }
                    }
                    catch
                    {
                        continue; 
                    }
                }
                if(!encontrado)                                
                {
                    //Hacemos el insert. 
                        //Le mandamos la clave de empleado que vamos a actualizar. 
                    insert_usuario(tabla_sql_server.Rows[x]["CLAVE_EMPLEADO"].ToString());
                    
                }
                
            }

                return "Actualizado"; 
        }

        private void update_usuario(string clave_empleado)
        {
            if (clave_empleado == "")
                return;

            try
            {

                //Hacemos un select a la base de SQL Server (Incluyendo la fotografía).
                string select_sql_server = string.Format("select  " +
                                        "b.NOMBRE " +
                                        ", b.APATERNO " +
                                        ", b.AMATERNO " +
                                        ", b.FECHA_REGISTRO " +
                                        ", b.FECHA_MODIFICACION " +
                                        ", a.CLAVE_EMPLEADO " +
                                        ", b.NSS " +
                                        ", b.RFC " +
                                        ", b.CURP " +
                                        ", b.AREA " +
                                        ", b.DEPARTAMENTO " +
                                        ", a.BORRADO " +
                                        ", b.FECHA_INGRESO " +
                                        ", b.FECHA_BAJA " +
                                        ", a.FOTO " +
                                        "from PERSONAS a  " +
                                        "left join EMPLEADOS b on a.CLAVE_EMPLEADO=b.CLAVE_EMPL " +
                                        "WHERE a.clave_empleado='{0}'", clave_empleado);

                DataTable tabla_sql_server = DatabaseChecado.runSelectQuery(select_sql_server);


                //Guardamos la fotografía en JPG. 
                string path = "";
                string temp_path = string.Format("~/temp/usuarios/{0}", path);
                string filename = tabla_sql_server.Rows[0]["CLAVE_EMPLEADO"].ToString() + ".jpg";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath(temp_path), filename);

                //Guardamos la fotografía en JPG. 
                try
                {
                    object arreglo = tabla_sql_server.Rows[0]["foto"];
                    using (var ms = new MemoryStream((byte[])tabla_sql_server.Rows[0]["foto"]))
                    {
                        Image imagen = Image.FromStream(ms);
                        imagen.Save(fullPath);
                    }
                }
                catch (Exception ex)
                {


                }



                //Hacemos el Update en la base de MySQL (Inclyendo la nueva ruta) 
                string update_query = string.Format("UPDATE `lu_usuarios` " +
                                    "SET " +
                                    "`APATERNO` = '{0}' " +
                                    ", `AMATERNO` = '{1}' " +
                                    ", `NOMBRE` = '{2}' " +
                                    ", `foto_url` = '{3}' " +
                                    ", `FECHA_REGISTRO` = STR_TO_DATE('{4}', '%Y-%m-%d') " +
                                    ", `FECHA_MODIFICACION` = STR_TO_DATE('{5}', '%Y-%m-%d') " +                                    
                                    ", `NSS` = '{6}' " +
                                    ", `RFC` = '{7}' " +
                                    ", `CURP` = '{8}' " +
                                    ", `AREA` = '{9}' " +
                                    ", `DEPARTAMENTO` = '{10}' " +
                                    ", `BORRADO` = '{11}' " +
                                    ", `FECHA_INGRESO` = STR_TO_DATE('{12}', '%Y-%m-%d') " +
                                    ", `FECHA_BAJA` = STR_TO_DATE('{13}', '%Y-%m-%d') " +
                                    "WHERE `CLAVE_EMPLEADO` = '{14}' "
                                    , tabla_sql_server.Rows[0]["APATERNO"].ToString()
                                    , tabla_sql_server.Rows[0]["AMATERNO"].ToString()
                                    , tabla_sql_server.Rows[0]["NOMBRE"].ToString()
                                    , "http://" + Request.Headers.Host + "/temp/usuarios/" + filename
                                    , tabla_sql_server.Rows[0]["FECHA_REGISTRO"].ToString() == "" ? DateTime.Now.ToString("yyyy-MM-dd") : DateTime.Parse(tabla_sql_server.Rows[0]["FECHA_REGISTRO"].ToString()).ToString("yyyy-MM-dd")
                                    , tabla_sql_server.Rows[0]["FECHA_MODIFICACION"].ToString() == "" ? DateTime.Now.ToString("yyyy-MM-dd") : DateTime.Parse(tabla_sql_server.Rows[0]["FECHA_MODIFICACION"].ToString()).ToString("yyyy-MM-dd")
                                    , tabla_sql_server.Rows[0]["NSS"].ToString()
                                    , tabla_sql_server.Rows[0]["RFC"].ToString()
                                    , tabla_sql_server.Rows[0]["CURP"].ToString()
                                    , tabla_sql_server.Rows[0]["AREA"].ToString()
                                    , tabla_sql_server.Rows[0]["DEPARTAMENTO"].ToString()
                                    , tabla_sql_server.Rows[0]["BORRADO"].ToString()
                                    , tabla_sql_server.Rows[0]["FECHA_INGRESO"].ToString() == "" ? DateTime.Now.ToString("yyyy-MM-dd") : DateTime.Parse(tabla_sql_server.Rows[0]["FECHA_INGRESO"].ToString()).ToString("yyyy-MM-dd")
                                    , tabla_sql_server.Rows[0]["FECHA_BAJA"].ToString() == "" ? DateTime.Now.ToString("yyyy-MM-dd") : DateTime.Parse(tabla_sql_server.Rows[0]["FECHA_BAJA"].ToString()).ToString("yyyy-MM-dd")
                                    , clave_empleado);
                Database.runQuery(update_query); 

                //Guardamos la fotografía en la ruta del servidor. 
                return;
            }
            catch(Exception ex)
            {
                return; 
            }
        }

        public void insert_usuario(string clave_empleado)
        {
            if (clave_empleado == "")
                return; 
            try
            {

                //Hacemos un select a la base de SQL Server (Incluyendo la fotografía).
                string select_sql_server = string.Format("select  " +
                                        "b.NOMBRE " +
                                        ", b.APATERNO " +
                                        ", b.AMATERNO " +
                                        ", b.FECHA_REGISTRO " +
                                        ", b.FECHA_MODIFICACION " +
                                        ", a.CLAVE_EMPLEADO " +
                                        ", b.NSS " +
                                        ", b.RFC " +
                                        ", b.CURP " +
                                        ", b.AREA " +
                                        ", b.DEPARTAMENTO " +
                                        ", a.BORRADO " +
                                        ", b.FECHA_INGRESO " +
                                        ", b.FECHA_BAJA " +
                                        ", a.FOTO " +
                                        "from PERSONAS a  " +
                                        "left join EMPLEADOS b on a.CLAVE_EMPLEADO=b.CLAVE_EMPL " +
                                        "WHERE a.clave_empleado='{0}'", clave_empleado);

                DataTable tabla_sql_server = DatabaseChecado.runSelectQuery(select_sql_server);


                string path = "";
                string temp_path = string.Format("~/temp/usuarios/{0}", path);
                string filename = tabla_sql_server.Rows[0]["CLAVE_EMPLEADO"].ToString() + ".jpg";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath(temp_path), filename);

                //Guardamos la fotografía en JPG. 
                try
                {
                    object arreglo = tabla_sql_server.Rows[0]["foto"];                    
                    using (var ms = new MemoryStream((byte[])tabla_sql_server.Rows[0]["foto"]))
                    {
                        Image imagen = Image.FromStream(ms);
                        imagen.Save(fullPath);
                    }
                }
                catch(Exception ex)
                {

                    
                }

                //Hacemos el Update en la base de MySQL (Inclyendo la nueva ruta) 
                string string_insert_query = string.Format("INSERT INTO `lu_usuarios` " + 
                                                    "(`APATERNO`, " + 
                                                    "`AMATERNO`, " + 
                                                    "`NOMBRE`, " + 
                                                    "`foto_url`, " + 
                                                    "`FECHA_REGISTRO`, " + 
                                                    "`FECHA_MODIFICACION`, " +                                                     
                                                    "`NSS`, " + 
                                                    "`RFC`, " + 
                                                    "`CURP`, " + 
                                                    "`AREA`, " + 
                                                    "`DEPARTAMENTO`, " + 
                                                    "`BORRADO`, " + 
                                                    "`FECHA_INGRESO`, " +                                                     
                                                    "`FECHA_BAJA`, " +
                                                    "`CLAVE_EMPLEADO`) " + 
                                                    "VALUES " + 
                                                    "('{0}', " + 
                                                    "'{1}', " + 
                                                    "'{2}', " + 
                                                    "'{3}', " +
                                                    "STR_TO_DATE('{4}', '%Y-%m-%d'), " +
                                                    "STR_TO_DATE('{5}', '%Y-%m-%d'), " +
                                                    "'{6}', " + 
                                                    "'{7}', " + 
                                                    "'{8}', " + 
                                                    "'{9}', " + 
                                                    "'{10}', " + 
                                                    "'{11}', " +
                                                    "STR_TO_DATE('{12}', '%Y-%m-%d'), " +                                                     
                                                    "STR_TO_DATE('{13}', '%Y-%m-%d'), " +                                                     
                                                    "'{14}'); "
                                    , tabla_sql_server.Rows[0]["APATERNO"].ToString()
                                    , tabla_sql_server.Rows[0]["AMATERNO"].ToString()
                                    , tabla_sql_server.Rows[0]["NOMBRE"].ToString()
                                    , "http://" + Request.Headers.Host + "/temp/usuarios/" + filename
                                    , tabla_sql_server.Rows[0]["FECHA_REGISTRO"].ToString() == "" ? DateTime.Now.ToString("yyyy-MM-dd") : DateTime.Parse(tabla_sql_server.Rows[0]["FECHA_REGISTRO"].ToString()).ToString("yyyy-MM-dd")
                                    , tabla_sql_server.Rows[0]["FECHA_MODIFICACION"].ToString() == "" ? DateTime.Now.ToString("yyyy-MM-dd") : DateTime.Parse(tabla_sql_server.Rows[0]["FECHA_MODIFICACION"].ToString()).ToString("yyyy-MM-dd")
                                    , tabla_sql_server.Rows[0]["NSS"].ToString()
                                    , tabla_sql_server.Rows[0]["RFC"].ToString()
                                    , tabla_sql_server.Rows[0]["CURP"].ToString()
                                    , tabla_sql_server.Rows[0]["AREA"].ToString()
                                    , tabla_sql_server.Rows[0]["DEPARTAMENTO"].ToString()
                                    , tabla_sql_server.Rows[0]["BORRADO"].ToString()
                                    , tabla_sql_server.Rows[0]["FECHA_INGRESO"].ToString() == "" ? DateTime.Now.ToString("yyyy-MM-dd") : DateTime.Parse(tabla_sql_server.Rows[0]["FECHA_INGRESO"].ToString()).ToString("yyyy-MM-dd")
                                    , tabla_sql_server.Rows[0]["FECHA_BAJA"].ToString() == "" ? DateTime.Now.ToString("yyyy-MM-dd") : DateTime.Parse(tabla_sql_server.Rows[0]["FECHA_BAJA"].ToString()).ToString("yyyy-MM-dd")
                                    , clave_empleado);

                Database.runInsert(string_insert_query); 

                //Guardamos la fotografía en la ruta del servidor.
                return;
            }
            catch (Exception ex)
            {
                return;
            }

            return; 
        }
    
    }
}

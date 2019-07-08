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
    public class etiquetasController : ApiController
    {
        public string getPDF()
        //public string getPDF(string id, int cantidad, int idoperador,int idusuario, int partida, int turno, int idmaquina, string comentarios ,int iniciacarrete)
        {
            //Parámetros
            string id = "";
            int cantidad = 0;
            int idoperador = 0;
            int idusuario = 0;
            int partida = 0;
            int turno = 0;
            int idmaquina = 0;
            string comentarios = "";
            int iniciacarrete = 0;


            string clave_de_producto = id;
            string hoja = "Hoja de Especificaciones grales";
            string etiquetavacia = "";
            string footer = "";
            string etiquetas = "";
            string html_completo = "";

            int mazoFinal = 0; //si la variable es '0' y se suman 5 etiquetas, el mazo final sera el número 5. 
            //Para el siguiente será 5 y se le sumaá el número de etiquetas generadas. 



            //declaramos encabezado, clave del producto
            IEnumerable<string> headerValues = Request.Headers.GetValues("clave_de_producto");
            clave_de_producto = headerValues.FirstOrDefault().ToString();

            //cantidad de etiquetas a generar
            IEnumerable<string> headerValues_cantidad = Request.Headers.GetValues("cantidad");
            string numetiquetas = headerValues_cantidad.FirstOrDefault().ToString();
            cantidad = int.Parse(numetiquetas);

            //operador
            IEnumerable<string> headerValues_operador = Request.Headers.GetValues("idoperador");
            string idOperador = headerValues_operador.FirstOrDefault().ToString();
            idoperador = int.Parse(idOperador);

            //usuario
            IEnumerable<string> headerValues_usuario = Request.Headers.GetValues("idusuario");
            string idUsuario = headerValues_usuario.FirstOrDefault().ToString();
            idusuario = int.Parse(idUsuario);

            //partida
            IEnumerable<string> headerValues_partida = Request.Headers.GetValues("partida");
            string Partida = headerValues_partida.FirstOrDefault().ToString();
            partida = int.Parse(Partida);

            //turno
            IEnumerable<string> headerValues_turno = Request.Headers.GetValues("turno");
            string Turno = headerValues_turno.FirstOrDefault().ToString();
            turno = int.Parse(Turno);

            //máquina
            IEnumerable<string> headerValues_maquina = Request.Headers.GetValues("maquina");
            string Maquina = headerValues_maquina.FirstOrDefault().ToString();
            idmaquina = int.Parse(Maquina);

            //comentarios
            IEnumerable<string> headerValues_comentarios = Request.Headers.GetValues("comentarios");
            comentarios = headerValues_comentarios.FirstOrDefault().ToString();

            //cuenta del carrete
            IEnumerable<string> headerValues_cuenta = Request.Headers.GetValues("iniciacarrete");
            string iniciaCarrete = headerValues_cuenta.FirstOrDefault().ToString();
            iniciacarrete = int.Parse(iniciaCarrete);


            //Hay que obtener la información del producto según la clave de producto recibida.
            //string queryEtiqueta = string.Format("SELECT * FROM [{0}$] WHERE [F1]='{1}'", hoja, clave_de_producto);
            string queryEtiqueta = string.Format("SELECT " +
            "[F1] as ClaveProducto, " +
            "[F2] AS Cliente, " +
            "[F3] AS Material, " +
            "[F4] AS Calibre, " +
            "[F5] AS Color, " +
            "[F7] AS Orientacion, " +
            "[F8] AS Perfil, " +
            "[F9] AS Frecuencia, " +
            "[F10] AS Amplitud, " +
            "[F12] AS Lubricante, " +
            "[F13] AS Mazo " +
            "FROM [{0}$] WHERE [F1]='{1}'", hoja, clave_de_producto);
            TablaEtiqueta = DataBaseEspecificaciones.runSelectQuery(queryEtiqueta);

            if (TablaEtiqueta == null)
                return "No se ecuentra el producto";

            //obtenemos el nombre del operador
            string queryOperador = string.Format("SELECT concat(nombres,' ', APATERNO) AS Operador from lu_usuarios where id='{0}'", idoperador);
            TablaOperador = Database.runSelectQuery(queryOperador);

            //obtenemos las iniciales del usuario
            string queryUsuario = string.Format("SELECT substr(nombres,1,1) as InicialNombre, substr(APATERNO, 1,1) as InicialApaterno FROM lu_usuarios where id='{0}'", idusuario);
            TablaUser = Database.runSelectQuery(queryUsuario);

            //generamos la etiqueta
            string header = "<!DOCTYPE html> " +
            "<html lang='es'> " +
            "<head> " +
            "<meta charset='UTF-8'> " +
            "<title>Document</title> " +
            "</head> " +
            "<body> " +
            "<div style='width: 100%; height: 100%; text-align:center; '> ";
            //solo el div que se va a repetir de acuerdo a la cantidad de etiquetas requeridas. 

            for (int x = 0; x < cantidad; x++)
            {

                etiquetas += "<div style='display: inline-block; border: 1px solid black; width: 41%; height: 14%; margin: 1%; font-size: 18px; '> " +
    "            <div style=''> " +
    "                <div style='text-align: center; '> " +
    "                    <span>" + TablaEtiqueta.Rows[0]["ClaveProducto"].ToString() + "</span> " +
    "                    <span>" + TablaEtiqueta.Rows[0]["Cliente"].ToString() + "</span> " +
    "                    <span> " + TablaEtiqueta.Rows[0]["Material"].ToString() + "</span> " +
    "                </div> " +
    "            </div> " +
    "            <div> " +
    "                <div style='text-align: center;'> " +
    "                    <span>" + TablaEtiqueta.Rows[0]["Calibre"].ToString() + "</span> " +
    "                    <span>" + TablaEtiqueta.Rows[0]["Amplitud"].ToString() + "</span> " +
    "                    <span>" + TablaEtiqueta.Rows[0]["Perfil"].ToString() + "</span> " +
    "                    <span>" + turno + "</span> " +

    "                </div> " +
    "            </div> " +
    "            <div style=''> " +
    "                <div style='text-align: center;'> " +
    "                    <span>" + TablaOperador.Rows[0]["Operador"].ToString() + "</span> " +
     "                    <span>" + TablaUser.Rows[0]["InicialNombre"].ToString() + "." + TablaUser.Rows[0]["InicialApaterno"].ToString() + "." + "</span> " +
     "                    <span>" + TablaEtiqueta.Rows[0]["Orientacion"].ToString() + "</span> " +
     "                    <span>" + TablaEtiqueta.Rows[0]["Lubricante"].ToString() + "</span> " +
     "                </div> " +
     "            </div> " +
     "            <div style=''> " +
     "                <div style='text-align: center;'> " +
     "                    <span>" + idmaquina + "</span> " +
     "                    <span>" + partida + "</span> " +
     "                    <span>" + TablaEtiqueta.Rows[0]["Perfil"].ToString() + "</span> " +
     "                    <span>" + TablaEtiqueta.Rows[0]["Mazo"].ToString() + "</span> " +

     "                </div> " +
     "            </div> " +
     "            <div style=''> " +
     "                <div style='text-align: center;'> " +
     "                    <span>" + TablaEtiqueta.Rows[0]["Frecuencia"].ToString() + "</span> " + //vendrá con su respectiva unidad (o/pulg)
     "                    <span>" + TablaEtiqueta.Rows[0]["Color"].ToString() + "</span> " +
     "                    <span>" + comentarios + "</span> " +
     "                    <span>" + Convert.ToDateTime(DateTime.Now).ToString("dd/MM/yyyy") + "</span> " +
     "                </div> " +
     "            </div> " +
     "		</div> ";

            }
            //etiqueta vacia
            etiquetavacia += "<div style='display: inline-block; border: 1px solid black; width: 41%; height: 14%; margin: 1%; font-size: 18px; '> " +
    "            <div style=''> " +
    "                <div style='text-align: center; '> " +
    "                    <span></span> " +
    "                    <span></span> " +
    "                    <span></span> " +
    "                </div> " +
    "            </div> " +
    "            <div> " +
    "                <div style='text-align: center;'> " +
    "                    <span></span> " +
    "                    <span></span> " +
    "                    <span><span> " +
    "                    <span></span> " +

    "                </div> " +
    "            </div> " +
    "            <div style=''> " +
    "                <div style='text-align: center;'> " +
    "                    <span></span> " +
     "                    <span></span> " +
     "                    <span></span> " +
     "                    <span></span> " +
     "                </div> " +
     "            </div> " +
     "            <div style=''> " +
     "                <div style='text-align: center;'> " +
     "                    <span></span> " +
     "                    <span></span> " +
     "                    <span></span> " +
     "                    <span></span> " +

     "                </div> " +
     "            </div> " +
     "            <div style=''> " +
     "                <div style='text-align: center;'> " +
     "                    <span></span> " +
     "                    <span></span> " +
     "                    <span></span> " +
     "                    <span></span> " +
     "                </div> " +
     "            </div> " +
     "		</div> ";
            footer = "</div> " +
               "</body> " +
               "</html> ";
            if ((cantidad % 2) == 0)
                html_completo = header + etiquetas + footer;
            else
                html_completo = header + etiquetas + etiquetavacia + footer;

            //se obtiene el mazo final a obtener, sumando la cantidad de etiquetas con el número con el que se inicia el mazo
            mazoFinal = iniciacarrete + cantidad;

            //se insertan los datos en ft_etiquetas por pdf
            string queryInsert = string.Format("INSERT INTO ft_etiquetas " +
                "(`calibre`, " +
                "`color`, " +
                "`lubricante`, " +
                "`cliente`, " +
                "`claveProducto`, " +
                "`turno`, " +
                "`fechaImpresion`, " +
                "`maquina`, " +
                "`partida`, " +
                "`mazo`, " +
                "`AMP`, " +
                "`frecuencia`, " +
                "`material`, " +
                "`perfil`, " +
                "`comentarios`, " +
                "`orientacion`, " +
                "`id_usuario`, " +
                "`etiquetas`, " +
                "`mazoFinal`, " +
                "`mazoInicial`)" +
                "VALUES" +
                "('{0}', " +
                "'{1}', " +
                "'{2}', " +
                "'{3}', " +
                "'{4}', " +
                "'{5}', " +
                "{6}, " +
                "'{7}', " +
                "'{8}', " +
                "'{9}', " +
                "'{10}', " +
                "'{11}', " +
                "'{12}', " +
                "'{13}', " +
                "'{14}', " +
                "'{15}', " +
                "'{16}', " +
                "'{17}', " +
                "'{18}', " +
                "'{19}') "
                , TablaEtiqueta.Rows[0]["Calibre"].ToString()
                , TablaEtiqueta.Rows[0]["Color"].ToString()
                , TablaEtiqueta.Rows[0]["Lubricante"].ToString()
                , TablaEtiqueta.Rows[0]["Cliente"].ToString()
                , TablaEtiqueta.Rows[0]["ClaveProducto"].ToString()
                , turno
                , "now()"
                , idmaquina
                , partida
                , TablaEtiqueta.Rows[0]["Mazo"].ToString()// mazo 
                , TablaEtiqueta.Rows[0]["Amplitud"].ToString()
                , TablaEtiqueta.Rows[0]["Frecuencia"].ToString()
                , TablaEtiqueta.Rows[0]["Material"].ToString()
                , TablaEtiqueta.Rows[0]["Perfil"].ToString()
                , comentarios
                , TablaEtiqueta.Rows[0]["Orientacion"].ToString()
                , idusuario
                , cantidad
                , mazoFinal
                , iniciacarrete
                );
            Database.runSelectQuery(queryInsert);


            //formato de nombre
            string filename = string.Format("archivo_{0}.pdf", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            //generar PDF
            utilidades.generar_pdf_con_html(html_completo, filename);
            //regresa ruta del servidor
            string baseurl = "http://" + Request.Headers.Host + "/temp/" + filename;
            return baseurl;

        }

        public string getUltimaEtiqueta()
        { //recibe el id del usuario y la última etiqueta impresa

            int idusuario = 0;
            //se recibe el usuario por encabezado
            IEnumerable<string> headerValues_usuario = Request.Headers.GetValues("idusuario");
            string idUsuario = headerValues_usuario.FirstOrDefault().ToString();
            idusuario = int.Parse(idUsuario);

            //se obtienen los datos de a última etiqueta por la fecha de registro que concuerde con el id del usuario.
            string queryReturn = string.Format("SELECT " +
                "calibre" +
                ",color " +
                ",lubricante " +
                ",cliente " +
                ",claveProducto " +
                ",turno " +
                ",maquina " +
                ",partida " +
                ",mazo " +
                ",AMP " +
                ",frecuencia " +
                ",material " +
                ",perfil " +
                ",comentarios " +
                ",orientacion " +
                ",etiquetas " +
                ",mazoFinal " +
                ",mazoInicial " +
                "FROM ft_etiquetas " +
                "WHERE id_usuario = '{0}' " +
                "order by fecha_de_registro desc limit 1"
                , idusuario);
            DataTable tabla = Database.runSelectQuery(queryReturn);
            return utilidades.convertDataTableToJson(tabla);

        }

        public string getSearchByPage()
        {
            IEnumerable<string> headerValues = Request.Headers.GetValues("pagina");
            string string_pagina = headerValues.FirstOrDefault().ToString();
            int pagina = int.Parse(string_pagina);

            IEnumerable<string> headerValues_id_etiqueta = Request.Headers.GetValues("id_etiqueta");
            string id_etiqueta = headerValues_id_etiqueta.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_nombre = Request.Headers.GetValues("nombre");
            string nombre = headerValues_nombre.FirstOrDefault().ToString();



            //En caso de ser criador un usuario no Administrador, no le regresamos nada.
            if ((id_etiqueta != "1"))
                return "Incorrecto";

            //Utilizaré la variable estatica (global) de la clase de utilidades y el número de la página que me solicitan. 
            //Recuerda siempre poner la condicio´n del estado. ¿Ok? 
            string query = string.Format("select * " +

            "from lu_etiquetas " +

            "where estado=1   " +
            "" + //Otras condiciones para el Where
            "group by id_etiqueta   " +
            "HAVING calibre like '%{2}%'   " +
            "OR color like '%{2}%'   " +
            "OR lubricante like '%{2}%'   " +
            "OR cliente like '%{2}%' " +
            "OR claveProducto like '%{2}%' " +
            "OR turno like '%{2}%' " +
            "OR maquina like '%{2}%' " +
            "OR partida like '%{2}%' " +
            "OR mazo like '%{2}%' " +
            "OR AMP like '%{2}%' " +
            "OR frecuencia like '%{2}%' " +
            "OR inicialesUsuario like '%{2}%' " +
            "OR NombreOperador like '%{2}%' " +
            "OR material like '%{2}%' " +
            "OR perfil like '%{2}%' " +
            "OR medidaPulg like '%{2}%' " +
            "OR orientacion like '%{2}%' " +
            "order by id_etiqueta desc limit {0} offset {1};  "
                , utilidades.elementos_por_pagina
                , ((pagina - 1) * (utilidades.elementos_por_pagina - 1))
                , nombre);

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            //Viste como Debuguiee? Cuando te salga algun errror, copia y pega el query que pones aqupara correlo en el Workbench Y listo :D
            //Convertimos a Json y regresamos los datos. 

            return utilidades.convertDataTableToJson(tabla_resultado);

            //Ya terminamos... Ahora a probar. Pondré un punto de ruptura al inicio de la funciójn pra debuggear. 
            //Te sugiero que hagas lo mismo para las funciones que hagas. Creo que es muy útil. 
        }
        public string Post(int id, [FromBody]Object value)
        {
            //Lo que viene en value es lo que nos manda el usuario a través del body de postman. 
            JObject json = JObject.Parse(value.ToString());

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `lu_etiquetas` " +
            "set " +
            "calibre='{0}' " +
            ",color='{1}' " +
            ",lubricante='{2}' " +
            ",cliente='{3}' " +
            ",claveProducto='{4}' " +
            ",turno='{5}' " +
                //",fechaImpresion='{6}' " +
            ",maquina='{6}' " +
            ",partida='{7}' " +
            ",mazo='{8}' " +
            ",AMP='{9}' " +
            ",frecuencia='{10}' " +
            ",inicialesUsuario='{11}' " +
            ",NombreOperador='{12}' " +
            ",material='{13}' " +
            ",perfil='{14}' " +
            ",comentarios='{15}' " +
            ",medidaPulg='{16}' " +
            ",orientacion='{17}' " +
            "where id_etiqueta='{18}'"
            , json["calibre"].ToString().Replace("'", "''")
            , json["color"].ToString().Replace("'", "''")
            , json["lubricante"].ToString().Replace("'", "''")
            , json["cliente"].ToString().Replace("'", "''")
            , json["claveProducto"].ToString().Replace("'", "''")
            , json["turno"].ToString().Replace("'", "''")
            , json["maquina"].ToString().Replace("'", "''")
            , json["partida"].ToString().Replace("'", "''")
            , json["mazo"].ToString().Replace("'", "''")
            , json["AMP"].ToString().Replace("'", "''")
            , json["frecuencia"].ToString().Replace("'", "''")
            , json["inicialesUsuario"].ToString().Replace("'", "''")
            , json["NombreOperador"].ToString().Replace("'", "''")
            , json["material"].ToString().Replace("'", "''")
            , json["perfil"].ToString().Replace("'", "''")
            , json["comentarios"].ToString().Replace("'", "''")
            , json["medidaPulg"].ToString().Replace("'", "''")
            , json["orientacion"].ToString().Replace("'", "''")
            , id);



            //Contestamos con el id del nuevo registro.
            if (Database.runQuery(update_query))
                return "correcto";
            else
                return "incorrecto";
        }



        public string Post([FromBody]Object value)
        {
            //Aquí se lo dejo joven: http://www.objgen.com/json?demo=true

            //Generamos el Datatable para devolver el resultado. 
            //Ya llegó la solicitud. En Postman debes poner que estas enviando un Json
            //En Value vienen los datos que enviaste a través de postman. 

            //Fijate como regreso la palabra "incorrecto" en caso de que la solicitud no se pueda completar correctamente. Esto es muy importante para que cache la excepción en el front cuando haga los formularios con 
            // HTML. ¿De acuerdo? ¿Ana? jaja OK :D 

            DataTable tabla_resultado = new DataTable();
            tabla_resultado.Columns.Add("id");
            tabla_resultado.Rows.Add();
            tabla_resultado.Rows[0]["id"] = "-1";

            JObject json = JObject.Parse(value.ToString());

            //Actualizamos los datos con un update query. 
            string insert_query = string.Format("INSERT INTO `lu_etiquetas` " +
            "( `calibre`," +
                "`color`," +
                "`lubricante`," +
                "`cliente`," +
                "`claveProducto`," +
                "`turno`," +
                "`maquina`," +
                "`partida`," +
                "`mazo`," +
                "`AMP`," +
                "`frecuencia`," +
                "`inicialesUsuario`," +
                "`NombreOperador`," +
                "`material`," +
                "`perfil`," +
                "`comentarios`," +
                "`medidaPulg`," +
                "`orientacion`) " +
            "VALUES " +
                //Verifica las funciones now() (Parametros 17 y 18), envía un post desde postman llenando estos datos y pon un punto de ruptura aquí para que veas el query. Copia y pega el query en Workbench para debuggearlo. 
            "('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}');"
             , json["calibre"].ToString().Replace("'", "''")
            , json["color"].ToString().Replace("'", "''")
            , json["lubricante"].ToString().Replace("'", "''")
            , json["cliente"].ToString().Replace("'", "''")
            , json["claveProducto"].ToString().Replace("'", "''")
            , json["turno"].ToString().Replace("'", "''")
            , json["maquina"].ToString().Replace("'", "''")
            , json["partida"].ToString().Replace("'", "''")
            , json["mazo"].ToString().Replace("'", "''")
            , json["AMP"].ToString().Replace("'", "''")
            , json["frecuencia"].ToString().Replace("'", "''")
            , json["inicialesUsuario"].ToString().Replace("'", "''")
            , json["NombreOperador"].ToString().Replace("'", "''")
            , json["material"].ToString().Replace("'", "''")
            , json["perfil"].ToString().Replace("'", "''")
            , json["comentarios"].ToString().Replace("'", "''")
            , json["medidaPulg"].ToString().Replace("'", "''")
            , json["orientacion"].ToString().Replace("'", "''"));

            //En caso de error, devolverá incorrecto
            tabla_resultado.Rows[0]["id"] = Database.runInsert(insert_query).ToString();

            /*
            string insert_inventario = string.Format("INSERT IGNORE INTO `lu_existencias_de_mascotas` (`id_usuario`, `id_raza`)   " +
                                                        "SELECT a.id as id_usuario, b.id as id_raza  " +
                                                        "from  " +
                                                        "lu_usuarios a  " +
                                                        "LEFT JOIN lu_razas b on 1 " +
                                                        "where a.id='{0}';  ",
                                                        tabla_resultado.Rows[0]["id"]);

            Database.runQuery(insert_inventario); */
            if (tabla_resultado.Rows[0]["id"].ToString() == "-1")
                return "incorrecto";

            //Devolcemos la información de la tabla. 
            return utilidades.convertDataTableToJson(tabla_resultado);

        }
        public string Delete(int id)
        {
            //recibe el id del usuario a eliminar
            string update_query = string.Format("UPDATE `lu_etiquetas` " +
            "set " +
            "estado=0 " +
            "where id_etiqueta='{0}'"
            , id);

            //Contestamos con el id del nuevo registro.
            if (Database.runQuery(update_query))
                return "correcto";
            else
                return "incorrecto";
        }
        public string getInformation(string id)
        {

            //Definición del encabezado para buscar por clave de producto
            //IEnumerable<string> headerValues = Request.Headers.GetValues("clave");
            //string string_clave = headerValues.FirstOrDefault().ToString();
            string hoja = "Hoja de Especificaciones grales";

            string query = string.Format("SELECT " +
             "[F1] as ClaveProducto, " +
            "[F2] AS Cliente, " +
            "[F3] AS Material, " +
            "[F4] AS Calibre, " +
            "[F5] AS Color, " +
            "[F7] AS Orientacion, " +
            "[F8] AS Perfil, " +
            "[F9] AS Frecuencia, " +
            "[F10] AS Amplitud, " +
            "[F12] AS Lubricante, " +
            "[F13] AS Mazo " +
                "FROM [{0}$] WHERE [F1]='{1}'", hoja, id);
            DataTable TablaInfo = new DataTable();
            TablaInfo = DataBaseEspecificaciones.runSelectQuery(query);
            if (TablaInfo == null)
                return "incorrecto";

            return utilidades.convertDataTableToJson(TablaInfo);

        }


        public DataTable TablaEtiqueta { get; set; }

        public DataTable TablaOperador { get; set; }

        public DataTable TablaUser { get; set; }
    }

}
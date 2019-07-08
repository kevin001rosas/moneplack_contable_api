using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;

namespace api
{
    class DataBaseEspecificaciones
    {

        static public OleDbConnection _conexionAPI = null;
        public static void getStringConnectionAPI()
        {
            string temp_path = string.Format("~/conf/");
            string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath(temp_path), "paths.txt");

            string [] lines = System.IO.File.ReadAllLines(fullPath);
            string pathEspecificaciones = lines[0];

            stringConnectionAPI = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + pathEspecificaciones + "; Extended Properties='Excel 8.0;HDR=NO';";
        }   

        internal static void crearConexion()
        {
            DataBaseEspecificaciones.getStringConnectionAPI();
            DataBaseEspecificaciones._conexionAPI = new OleDbConnection(DataBaseEspecificaciones.stringConnectionAPI);
        }

        internal static System.Data.DataTable runSelectQuery(string query)
        {
            try
            {
                //En caso de que la conexión no este inicializada, la creamos. 
                if (DataBaseEspecificaciones._conexionAPI == null)
                {
                    DataBaseEspecificaciones.crearConexion();
                }

                //Creamos el comando 
                OleDbCommand comando = new OleDbCommand();
                comando.Connection = DataBaseEspecificaciones._conexionAPI;

                //Creamos el DataAdapter
                OleDbDataAdapter adaptadorDeDatos = new OleDbDataAdapter();
                adaptadorDeDatos.SelectCommand = comando;


                DataSet ds = new DataSet();
                comando.CommandText = query;
                if (_conexionAPI.State != System.Data.ConnectionState.Open)
                    _conexionAPI.Open();
                DataTable tabla = new DataTable();
                adaptadorDeDatos.Fill(tabla);

                if (_conexionAPI.State != System.Data.ConnectionState.Closed)
                    _conexionAPI.Close();

                if (tabla.Rows.Count == 0)
                {
                    return null;
                }
                return tabla;


            }
            catch (Exception ex)
            {
                ManageException(ex);
                return null;
            }
        }

        private static void ManageException(Exception ex)
        {
            if (_conexionAPI.State == System.Data.ConnectionState.Open)
                _conexionAPI.Close();
        }
        public static string pathBaseDeDatos { get; set; }

        public static string stringConnectionAPI { get; set; }
    }
}
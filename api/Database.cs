using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace api.Controllers
{
    class Database
    {
        public static string tipoUsuario = "";
        //public static MySqlConnection _conn = new MySqlConnection("server=xiksolutions.com;database=db_contabilidad_moneplack;uid=kevin;pwd=conker1385;CHARSET=utf8;convert zero datetime=True");
        //public static MySqlConnection _conn = new MySqlConnection("server=mysql5012.site4now.net;database=db_a47aae_royal;uid=a47aae_royal;pwd=conker1385;CHARSET=utf8;convert zero datetime=True");
        //public static MySqlConnection _conn = new MySqlConnection("server=xik.mx;port=3306;database=db_royal_canin;uid=kevin;pwd=conker1385;CHARSET=utf8;convert zero datetime=True");
        public static MySqlConnection _conn = new MySqlConnection("server=localhost;database=db_contabilidad_moneplack;uid=root;pwd=conker;CHARSET=utf8;convert zero datetime=True");
        //public static MySqlConnection _conn = new MySqlConnection("server=192.168.200.244;port=3309;database=db_royal_canin;uid=kevin;pwd=conker1385;CHARSET=utf8;convert zero datetime=True");
        public static DataTable tablaEquipos;
        public static string connectionStringTablaInsercion;
        public static OleDbConnection __connExcel;
        public static MySqlDataAdapter adapt;
        public static DataTable tablaBusqueda;
        public static string userType;



        public static void killExcelProcesses()
        {
            System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName("Excel");
            foreach (System.Diagnostics.Process p in process)
            {
                if (!string.IsNullOrEmpty(p.ProcessName))
                {
                    try
                    {
                        p.Kill();
                    }
                    catch { }
                }
            }
        }

        public static string[] FixEscapeCharacters(params string[] parameters)
        {
            string[] newParameters = new string[parameters.Length];
            int index = 0;
            foreach (var param in parameters)
            {
                if (param.Contains("'"))
                    newParameters[index] = param.Replace("'", "''");
                else if (param.Contains("\""))
                    newParameters[index] = param.Replace("\"", "\"\"");
                else if (param.Contains("["))
                    newParameters[index] = param.Replace("[", "[[]");
                else
                    newParameters[index] = param;
                index++;
            }
            return newParameters;
        }

        public static DataTable runSelectQuery(string query)
        {
            query = query.Replace("\n", "");


            Console.WriteLine(query);
            DataTable tablaSelect = new DataTable();
            string sql = query;

            try
            {

                var comm = new MySqlCommand(sql, Database._conn);
                Database._conn.Open();
                Database.adapt = new MySqlDataAdapter(comm);
                MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(Database.adapt);
                Database.tablaBusqueda = new DataTable();
                Database.adapt.Fill(tablaSelect);
                Database._conn.Close();

                if (tablaSelect == null)
                {

                    return null;
                }
                else if (tablaSelect.Rows.Count < 1)
                {
                    return null;
                }

                return tablaSelect;

            }
            catch (Exception e)
            {
                Database.ManageException(e);
                Console.WriteLine(sql);
                return null;

            }
        }

        public static string runSelectQueryDebug(string query)
        {
            query = query.Replace("\n", "");


            Console.WriteLine(query);
            DataTable tablaSelect = new DataTable();
            string sql = query;

            try
            {

                var comm = new MySqlCommand(sql, Database._conn);
                Database._conn.Open();
                Database.adapt = new MySqlDataAdapter(comm);
                MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(Database.adapt);
                Database.tablaBusqueda = new DataTable();
                Database.adapt.Fill(tablaSelect);
                Database._conn.Close();

                if (tablaSelect == null)
                {

                    return null;
                }
                else if (tablaSelect.Rows.Count < 1)
                {
                    return null;
                }

                return "correcto";

            }
            catch (Exception e)
            {
                Database.ManageException(e);
                Console.WriteLine(sql);
                return e.ToString();

            }
        }


        public static string runSelectQueryEspecial(string query)
        {
            try
            {
                query = query.Replace("\n", "");


                Console.WriteLine(query);
                DataTable tablaSelect = new DataTable();
                string sql = query;



                var comm = new MySqlCommand(sql, Database._conn);
                Database._conn.Open();
                Database.adapt = new MySqlDataAdapter(comm);
                MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(Database.adapt);
                Database.tablaBusqueda = new DataTable();
                Database.adapt.Fill(tablaSelect);
                Database._conn.Close();

                if (tablaSelect == null)
                {

                    return null;
                }
                else if (tablaSelect.Rows.Count < 1)
                {
                    return null;
                }

                return "Correcto";

            }
            catch (Exception e)
            {
                Database.ManageException(e);
                return e.ToString();

            }
        }

        public static long runInsert(string query)
        {
            query = query.Replace("\n", "");
            query = query.Replace("'", "\'");

            var comm = new MySqlCommand(query, Database._conn);
            try
            {
                Database._conn.Open();
                Console.WriteLine(query);
                comm.ExecuteNonQuery();
                Database._conn.Close();
                return comm.LastInsertedId;
            }
            catch (Exception e)
            {
                Database.ManageException(e);
                return -1;
            }

        }
        public static bool runQuery(string query)
        {
            query = query.Replace("\n", "");
            query = query.Replace("'", "\'");
            var comm = new MySqlCommand(query, Database._conn);
            try
            {
                Database._conn.Open();
                Console.WriteLine(query);
                comm.ExecuteNonQuery();
                Database._conn.Close();
                return true;
            }
            catch (Exception e)
            {
                Database.ManageException(e);
                return false;
            }

        }

        public static bool runDelete(string tabla, string llave, long valorLlave)
        {
            string query = string.Format("UPDATE `{0}`  SET estado=0 WHERE {1}='{2}'", tabla, llave, valorLlave);
            var comm = new MySqlCommand(query, Database._conn);
            try
            {
                Database._conn.Open();
                Console.WriteLine(query);
                comm.ExecuteNonQuery();
                Database._conn.Close();
                return true;
            }
            catch (Exception e)
            {
                Database.ManageException(e);
                return false;
            }

        }

        public static void ManageException(Exception e)
        {
            if (_conn.State == System.Data.ConnectionState.Open)
                _conn.Close();
        }

    }
}
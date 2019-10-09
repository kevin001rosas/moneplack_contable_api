using System;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace api.Controllers
{
    class utilidades
    {
        //Inicializamos un timer para repetirlo cada 30 minutos; 
        public static System.Timers.Timer timer = new System.Timers.Timer(60 * 15 * 1000);


        public static void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Reestablecemos el contador de solicitudes de restablecimiento de contraseñas. 
            solicitudes_de_restablecimiento_de_secret = 0;
        }


        public static DataTable convertJsonStringToDataTable(string jsonString)
        {
            var jsonLinq = JObject.Parse(jsonString);

            // Find the first array using Linq
            var srcArray = jsonLinq.Descendants().Where(d => d is JArray).First();
            var trgArray = new JArray();
            foreach (JObject row in srcArray.Children<JObject>())
            {
                var cleanRow = new JObject();
                foreach (JProperty column in row.Properties())
                {
                    // Only include JValue types
                    if (column.Value is JValue)
                    {
                        cleanRow.Add(column.Name, column.Value);
                    }
                }
                trgArray.Add(cleanRow);
            }

            return JsonConvert.DeserializeObject<DataTable>(trgArray.ToString());
        }
    


        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }


        public static string convertDataTableToJson(DataTable table)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> parentRow = new List<Dictionary<string, object>>();
            Dictionary<string, object> childRow;
            foreach (DataRow row in table.Rows)
            {
                childRow = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    //Hacemos el pase del datetime
                    if (col.DataType == typeof(DateTime))
                    {
                        try
                        { childRow.Add(col.ColumnName, DateTime.Parse(row[col].ToString()).ToString("yyyy-MM-dd")); }
                        catch { childRow.Add(col.ColumnName, DateTime.Now.ToString("yyyy-MM-dd")); }
                    }

                    else
                        childRow.Add(col.ColumnName, row[col]);

                }
                parentRow.Add(childRow);
            }
            return jsSerializer.Serialize(parentRow);
        }

        public static string obtener_tipo_de_usuario(string id_usuario)
        {
            string select_query = string.Format("SELECT id_tipo_de_usuario FROM lu_usuarios where id='{0}'", id_usuario);
            DataTable usuario = Database.runSelectQuery(select_query);
            return (usuario.Rows[0]["id_tipo_de_usuario"].ToString());

        }

        internal static bool validar_token(System.Net.Http.HttpRequestMessage Request)
        {
            return true;
            IEnumerable<string> headerValues = Request.Headers.GetValues("token");
            string token = headerValues.FirstOrDefault().ToString();

            IEnumerable<string> headerValues2 = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues2.FirstOrDefault().ToString();

            if (token == "kevin")
                return true;
            else
                return false;

        }

        //recuerda que en utilidades tendremos todas las funciones y variables que utilizaremos en el esistema. 
        //Aquí pondemos las variables para controlar la paginación. Siempre hay que poner la cantidad deseada + 1 
        static public int elementos_por_pagina = 9;

        internal static bool es_admin(string id_usuario)
        {
            //Corremos el query para verificar si es administrador. 
            string select_query = string.Format("Select id from lu_usuarios where id='{0}' and id_tipo_de_usuario='1'; "
                , id_usuario);
            DataTable tabla = Database.runSelectQuery(select_query);
            return tabla != null ? true : false;
        }

        internal static string generar_token(string id_usuario, string email)
        {
            string text = id_usuario + email;
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        public static void enviar_correo(string mensaje, string direccion, string asunto)
        {
            //Referencia: 
            //http://vandelayweb.com/sending-asp-net-emails-godaddy-gmail-godaddy-hosted/

            //Create the msg object to be sent
            MailMessage msg = new MailMessage();
            //Add your email address to the recipients
            msg.To.Add(direccion);
            //Configure the address we are sending the mail from
            //MailAddress address = new MailAddress("knrosas@xiksolutions.com", "Micrositio Royal Canin");
            MailAddress address = new MailAddress("no-reply@micrositioroyalcanin.com.mx", "Micrositio Royal Canin");

            msg.From = address;
            msg.Subject = asunto;
            msg.Body = mensaje;
            msg.IsBodyHtml = true;


            SmtpClient client = new SmtpClient();
            client.Host = "relay-hosting.secureserver.net";
            client.Port = 25;

            //Send the msg

            /*
            leer_accesos_correo();
            MailMessage mail = new MailMessage(); 
            //SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            SmtpClient SmtpServer = new SmtpClient(utilidades.smtp_server);
            //SmtpServer.Host = "smtpout.secureserver.net"; 
            mail.From = new MailAddress(utilidades.mail_user, "Micrositio Royal Canin");
            mail.To.Add(direccion);
            mail.Subject = asunto;
            mail.Body = mensaje;
            mail.IsBodyHtml = true;
            //SmtpServer.Port = 587;
            SmtpServer.Port = utilidades.mail_port;
            //SmtpServer.Credentials = new System.Net.NetworkCredential("kevin001rosas@gmail.com", "AnilloFlor1");
            SmtpServer.Credentials = new System.Net.NetworkCredential(utilidades.mail_user, utilidades.mail_password);
            SmtpServer.EnableSsl = true;

            SmtpServer.Send(mail);*/
            client.Send(msg);
        }

        public static string generar_pdf_con_html(string html, string filename)
        {   
            IronPdf.HtmlToPdf Renderer = new IronPdf.HtmlToPdf();

            Renderer.PrintOptions.MarginTop = 0;  //millimeters
            Renderer.PrintOptions.MarginLeft = 0;  //millimeters
            Renderer.PrintOptions.MarginRight = 0;  //millimeters
            Renderer.PrintOptions.MarginBottom = 0;  //millimeters
            Renderer.PrintOptions.FitToPaperWidth = true;
            Renderer.PrintOptions.PaperSize = IronPdf.PdfPrintOptions.PdfPaperSize.Letter;
            Renderer.PrintOptions.PaperOrientation = IronPdf.PdfPrintOptions.PdfPaperOrientation.Portrait;
            // Render an HTML document or snippet as a string            
            string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/temp"), filename);

            Renderer.RenderHtmlAsPdf(html).SaveAs(fullPath);
            return filename;
        }

        private static void leer_accesos_correo()
        {
            utilidades.smtp_server = "smtp.gmail.com";
            utilidades.mail_user = "micrositioroyalcanin@gmail.com";
            utilidades.mail_password = "AnilloFlor1";
            utilidades.mail_port = 587;
        }

        //Función para enviar un correo a través del servidor. 

        public static string smtp_server { get; set; }

        public static string mail_user { get; set; }

        public static string mail_password { get; set; }

        public static int mail_port { get; set; }

        public static int solicitudes_de_restablecimiento_de_secret { get; set; }

        internal static void guardar_imagen(string base64_string, string path, string filename)
        {
            try
            {
                //Guardamos la imagen en un archivo. 
                var bytes = Convert.FromBase64String(base64_string.Substring(base64_string.IndexOf(',') + 1));
                string temp_path = string.Format("~/temp/{0}", path);
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath(temp_path), filename);
                using (var imageFile = new FileStream(fullPath, FileMode.Create))
                {
                    imageFile.Write(bytes, 0, bytes.Length);
                    imageFile.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

            }
        }
    }
}

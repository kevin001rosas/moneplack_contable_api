using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace api.Controllers
{
    public class descargasController : ApiController
    {
        // GET api/clientesdescargas        
        public HttpResponseMessage getReporteDeClientes()
        {
            if (!utilidades.validar_token(Request))
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "incorrecto");
                return response;
            }


            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
            string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

            if (id_tipo_de_usuario != "1")
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "incorrecto");
                return response;
            }

            string select_query = string.Format("select  " +
            "a.razon_social as Cliente  " +
            ", (CASE  " +
            "when sum(b.total) is null then 0 " +
            "else sum(b.total) " +
            "end) as Facturado " +
            ", (CASE  " +
            "when sum(c.importe) is null then 0 " +
            "else sum(c.importe)  " +
            "end) as Pagado " +
            "from lu_clientes a  " +
            "LEFT JOIN ft_facturas b on (a.id=b.id_cliente and b.estado=1)  " +
            "LEFT JOIN ft_pagos c on (c.id_factura=b.id and c.estado=1)  " +
            "where a.estado=1  " +
            "group by a.id;  ");
            DataTable dt = Database.runSelectQuery(select_query);
            IWorkbook workbook = GenerateExcelFile(dt);
            
            var fileName = "Reporte_de_clientes" + DateTime.Now.ToString("yyyyMMddHHmm") + ".xls";

            //save the file to server temp folder
            string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/temp"), fileName);

            using (var exportData = new MemoryStream())
            {
                //I don't show the detail how to create the Excel, this is not the point of this article,
                //I just use the NPOI for Excel handler
                FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                workbook.Write(file);
                file.Close();
            }


            //Convirtiendo el archivo 
            var dataBytes = File.ReadAllBytes(fullPath);
            //adding bytes to memory stream   
            var dataStream = new MemoryStream(dataBytes);

            Descarga descarga = new Descarga(dataStream, Request, fileName);
            return descarga.generar_respuesta();
            //Comenzamos la descarga. 

        }

        public HttpResponseMessage getReporteDeFacturasMensual()
        {
            if (!utilidades.validar_token(Request))
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "incorrecto");
                return response;
            }

            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
            string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_mes = Request.Headers.GetValues("mes");
            string mes_string = headerValues_mes.FirstOrDefault().ToString();

            if (id_tipo_de_usuario != "1")
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "incorrecto");
                return response;
            }

            try
            {

                //Hacemos el split del mes. 
                string[] arreglo = mes_string.ToString().Replace("'", "''").Split('-');
                int año = int.Parse(arreglo[0]);
                int mes = int.Parse(arreglo[1]);

                DateTime primer_dia = new DateTime(año, mes, 1);
                DateTime ultimo_dia = primer_dia.AddMonths(1).AddDays(-1);


                string select_query = string.Format("select * from (select  " +
                "a.id " +
                ", a.folio as folio " +
                ", date_format(a.fecha, '%d/%m/%Y') as fecha_de_emision  " +
                ", a.concepto as concepto  " +
                ", a.importe as importe " +
                ", a.iva as iva " +
                ", a.total as total  " +
                ", c.rfc as rfc " +
                ", c.razon_social as cliente " +
                ", b.nombre as estado_de_factura " +
                ", a.estado as estado " +
                "from ft_facturas a  " +
                "left join cf_estados_de_factura b on a.id_estado_de_factura=b.id  " +
                " left join lu_clientes c on a.id_cliente=c.id   " +
                "where a.fecha >= date_format('{0} 00:00:00', '%Y-%m-%d %H:%i:%s') " +
                "and a.fecha <= date_format('{1} 23:59:59', '%Y-%m-%d %H:%i:%s') " +
                "union all  " +
                "select  " +
                "a.id " +
                ", a.folio as folio " +
                ", date_format(a.fecha, '%d/%m/%Y') as fecha_de_emision  " +
                ", a.concepto as concepto  " +
                ", a.importe as importe " +
                ", a.iva as iva " +
                ", a.total as total  " +
                ", c.rfc as rfc " +
                ", c.razon_social as cliente " +
                ", b.nombre as estado_de_factura " +
                ", a.estado as estado " +
                " from ft_facturas a  " +
                " left join cf_estados_de_factura b on a.id_estado_de_factura=b.id  " +
                " left join lu_clientes c on a.id_cliente=c.id   " +
                "where a.id_estado_de_factura=1  " +
                "and a.fecha <= date_format('{1} 23:59:59', '%Y-%m-%d %H:%i:%s')) a " +
                "where a.estado=1 " +
                "group by a.id " +
                "order by a.id; "
                , primer_dia.ToString("yyyy-MM-dd")
                , ultimo_dia.ToString("yyyy-MM-dd"));
                DataTable dt = Database.runSelectQuery(select_query);
                if (dt == null)
                    dt = new DataTable();
                if (dt.Columns.Contains("id"))
                    dt.Columns.Remove("id");
                if (dt.Columns.Contains("estado"))
                    dt.Columns.Remove("estado");




                IWorkbook workbook = GenerateExcelFile(dt);

                var fileName = "Reporte_de_facturas_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".xls";

                //save the file to server temp folder
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/temp"), fileName);

                using (var exportData = new MemoryStream())
                {
                    //I don't show the detail how to create the Excel, this is not the point of this article,
                    //I just use the NPOI for Excel handler
                    FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                    workbook.Write(file);
                    file.Close();
                }


                //Convirtiendo el archivo 
                var dataBytes = File.ReadAllBytes(fullPath);
                //adding bytes to memory stream   
                var dataStream = new MemoryStream(dataBytes);

                Descarga descarga = new Descarga(dataStream, Request, fileName);
                return descarga.generar_respuesta();
                //Comenzamos la descarga. 
            }
            catch (Exception ex)
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, ex.ToString());
                return response;
            }

        }


        public HttpResponseMessage getReporteDePagosMensual()
        {
            if (!utilidades.validar_token(Request))
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "incorrecto");
                return response;
            }

            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
            string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_mes = Request.Headers.GetValues("mes");
            string mes_string = headerValues_mes.FirstOrDefault().ToString();

            if (id_tipo_de_usuario != "1")
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "incorrecto");
                return response;
            }

            //Hacemos el split del mes. 
            string[] arreglo = mes_string.ToString().Replace("'", "''").Split('-');
            int año = int.Parse(arreglo[0]);
            int mes = int.Parse(arreglo[1]);

            DateTime primer_dia = new DateTime(año, mes, 1);
            DateTime ultimo_dia = primer_dia.AddMonths(1).AddDays(-1);


            string select_query = string.Format("select  " +
            "date_format(a.fecha, '%d/%m/%Y') as fecha_de_pago " +
            ", d.nombre as banco  " +
            ", a.importe as pagado  " +
            ", date_format(b.fecha, '%d/%m/%Y') as fecha_de_emision_de_factura " +
            ", b.folio as folio_de_factura " +
            ", c.rfc as rfc  " +
            ", c.razon_social as cliente " +
            ", b.concepto as concepto  " +
            ", b.importe as importe_de_factura " +
            ", b.iva as iva_de_factura " +
            ", b.total as total_de_factura " +
            "from ft_pagos a  " +
            "left join ft_facturas b on a.id_factura=b.id " +
            "left join lu_clientes c on b.id_cliente=c.id " +
            "left join cf_bancos d on d.id=a.id_banco " +
            "where a.fecha>=date_format('{0} 00:00:00', '%Y-%m-%d %H:%i:%s') " +
            "and a.fecha<=date_format('{1} 23:59:59', '%Y-%m-%d %H:%i:%s') " +
            "and a.estado=1 " + 
            "group by a.id  "  + 
            "order by a.fecha asc;  " 
            , primer_dia.ToString("yyyy-MM-dd")
            , ultimo_dia.ToString("yyyy-MM-dd"));
            DataTable dt = Database.runSelectQuery(select_query);
            if (dt == null)
                dt = new DataTable();
            if (dt.Columns.Contains("id"))
                dt.Columns.Remove("id");

            IWorkbook workbook = GenerateExcelFile(dt);

            var fileName = "Reporte_de_pagos_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".xls";

            //save the file to server temp folder
            string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/temp"), fileName);

            using (var exportData = new MemoryStream())
            {
                //I don't show the detail how to create the Excel, this is not the point of this article,
                //I just use the NPOI for Excel handler
                FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                workbook.Write(file);
                file.Close();
            }


            //Convirtiendo el archivo 
            var dataBytes = File.ReadAllBytes(fullPath);
            //adding bytes to memory stream   
            var dataStream = new MemoryStream(dataBytes);

            Descarga descarga = new Descarga(dataStream, Request, fileName);
            return descarga.generar_respuesta();
            //Comenzamos la descarga. 

        }

        public HttpResponseMessage getComplementoDePago()
        {

            try
            {
                if (!utilidades.validar_token(Request))
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "incorrecto");
                    return response;
                }

                IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
                string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

                IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
                string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

                IEnumerable<string> headerValues_id_cliente = Request.Headers.GetValues("id_cliente");
                string id_cliente = headerValues_id_cliente.FirstOrDefault().ToString();

                IEnumerable<string> headerValues_fecha_inicial = Request.Headers.GetValues("fecha_inicial");
                string fecha_inicial_string = headerValues_fecha_inicial.FirstOrDefault().ToString();

                IEnumerable<string> headerValues_fecha_final = Request.Headers.GetValues("fecha_final");
                string fecha_final_string = headerValues_fecha_final.FirstOrDefault().ToString();

                if (id_tipo_de_usuario != "1")
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "incorrecto");
                    return response;
                }

                //Hacemos el split del mes. 
                string[] arreglo = fecha_inicial_string.ToString().Replace("'", "''").Split('-');
                int año = int.Parse(arreglo[0]);
                int mes = int.Parse(arreglo[1]);
                int dia = int.Parse(arreglo[2]);

                string[] arreglo_final = fecha_final_string.ToString().Replace("'", "''").Split('-');
                int año_final = int.Parse(arreglo_final[0]);
                int mes_final = int.Parse(arreglo_final[1]);
                int dia_final = int.Parse(arreglo_final[2]);

                DateTime fecha_inicial = new DateTime(año, mes, dia);
                DateTime fecha_final = new DateTime(año_final, mes_final, dia_final);

                /*
                 "select  " +
    "b.folio as folio " +
    ", b.fecha as fecha_de_factura " +
    ", b.concepto as concepto  " +
    ", b.total as importe_de_factura " +
    ", a.fecha as fecha_de_pago " +
    ", a.importe as importe_de_pago " +
    ", c.nombre as banco " +
    ", round(ceiling((b.total - a.importe)*100)*1.0/100, 2) as saldo  " +
    "from ft_pagos a  " +
    "left join ft_facturas b on b.id=a.id_factura " +
    "left join cf_bancos c on c.id=a.id_banco " +
    "where a.fecha>=date_format('{0} 00:00:00', '%Y-%m-%d %H:%i:%s') " +
    "and a.fecha<=date_format('{1} 23:59:59', '%Y-%m-%d %H:%i:%s') " +
    "and a.estado=1 " +
                 * 
                 */

                string select_query = string.Format("select  a.folio as `Folio`  " +
                ", date_format(a.fecha , '%d/%m/%Y') as `Fecha de emisión de factura` " +
                ", a.concepto as `Concepto` " +
                ", a.total as `Importe de factura`  " +
                ", date_format(b.fecha , '%d/%m/%Y') as `Fecha de pago` " +
                ", b.importe as `Importe de pago` " +
                ", c.nombre as `Banco`  " +
                ", round(ceiling((a.total - b.importe)*100)*1.0/100 , 2) as `Saldo`  " +
                "from ft_facturas a   " +
                "left join ft_pagos b on (a.id=b.id_factura and b.estado=1)" +
                "left join cf_bancos c on c.id=b.id_banco  " +
                "where a.fecha>=date_format('{0} 00:00:00', '%Y-%m-%d %H:%i:%s')  " +
                "and a.fecha<=date_format('{1} 23:59:59', '%Y-%m-%d %H:%i:%s')  " +
                "and a.estado=1  " +                
                "and a.id_cliente='{2}'; "
                , fecha_inicial.ToString("yyyy-MM-dd")
                , fecha_final.ToString("yyyy-MM-dd")
                , id_cliente);
                DataTable dt = Database.runSelectQuery(select_query);
                if (dt == null)
                    dt = new DataTable();
                if (dt.Columns.Contains("id"))
                    dt.Columns.Remove("id");

                //Obtenemos el nombre del cliente 
                string query_cliente = string.Format("Select razon_social from lu_clientes where id='{0}';", id_cliente);
                DataTable cliente = Database.runSelectQuery(query_cliente);
                string titulo_excel = string.Format("Complemento de Pago de {0} del {1} al {2}"
                    , cliente.Rows[0]["razon_social"].ToString()
                    , fecha_inicial.ToString("dd/MM/yyyy")
                    , fecha_final.ToString("dd/MM/yyyy"));

                IWorkbook workbook = GenerateExcelFileWithTitle(dt, titulo_excel);

                var fileName = "Complemento_de_pago" + DateTime.Now.ToString("yyyyMMddHHmm") + ".xls";

                //save the file to server temp folder
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/temp"), fileName);

                using (var exportData = new MemoryStream())
                {
                    //I don't show the detail how to create the Excel, this is not the point of this article,
                    //I just use the NPOI for Excel handler
                    FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                    workbook.Write(file);
                    file.Close();
                }


                //Convirtiendo el archivo 
                var dataBytes = File.ReadAllBytes(fullPath);
                //adding bytes to memory stream   
                var dataStream = new MemoryStream(dataBytes);

                Descarga descarga = new Descarga(dataStream, Request, fileName);
                return descarga.generar_respuesta();
                //Comenzamos la descarga. 
            }
            catch(Exception ex)
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, ex.ToString());
                return response;
            }

        }

        public HttpResponseMessage getEtiquetas()
        {
            if (!utilidades.validar_token(Request))
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "incorrecto");
                return response;
            }


            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
            string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

            if (id_tipo_de_usuario != "1")
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "incorrecto");
                return response;
            }



            string select_query = string.Format("SELECT calibre, " +
                "color_e as Color, " +
                "cliente as Cliente, " +
                "clave_producto as ClaveProducto, " +
                "turno as Turno, " +
                "fecha_de_impresion as FechaImpresion, " +
                "embobinadora as Embobinadora, " +
                "etiquetas as Etiquetas, " +
                "carrete_inicial as CarreteInicial, " +
                "carrete_final as CarreteFinal, " +
                "linea as Linea, " +
                "numero_envio as Envio, " +
                "hilos as Hilos " +
                "FROM ft_etiquetas " +
                "where estado=1 " +
                "order by embobinadora asc;  ");
            DataTable dt = Database.runSelectQuery(select_query);
            IWorkbook workbook = GenerateExcelFile(dt);

            var fileName = "exportacion_etiquetas_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".xls";

            //save the file to server temp folder
            string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/temp"), fileName);

            using (var exportData = new MemoryStream())
            {
                //I don't show the detail how to create the Excel, this is not the point of this article,
                //I just use the NPOI for Excel handler
                FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                workbook.Write(file);
                file.Close();
            }


            //Convirtiendo el archivo 
            var dataBytes = File.ReadAllBytes(fullPath);
            //adding bytes to memory stream   
            var dataStream = new MemoryStream(dataBytes);

            Descarga descarga = new Descarga(dataStream, Request, fileName);
            return descarga.generar_respuesta();
            //Comenzamos la descarga. 
            //return descarga; 

        }
        public IWorkbook GenerateExcelFile(DataTable dt)
        {
            try
            {
                IWorkbook workbook;
                workbook = new HSSFWorkbook();


                ISheet sheet1 = workbook.CreateSheet("Sheet 1");

                //make a header row
                IRow row1 = sheet1.CreateRow(0);

                IFont boldFont = workbook.CreateFont();
                boldFont.Boldweight = (short)FontBoldWeight.Bold;
                boldFont.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
                ICellStyle boldStyle = workbook.CreateCellStyle();
                boldStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.SeaGreen.Index;
                boldStyle.FillPattern = FillPattern.SolidForeground;
                boldStyle.SetFont(boldFont);

                //Creamos el estilo para manejar números. 
                ICellStyle cellNumberStyle = workbook.CreateCellStyle();
                cellNumberStyle.DataFormat = workbook.CreateDataFormat().GetFormat("0.00");


                for (int j = 0; j < dt.Columns.Count; j++)
                {

                    ICell cell = row1.CreateCell(j);
                    String columnName = dt.Columns[j].ToString();
                    cell.CellStyle = boldStyle; //Ponemos el estilo del encabezado.
                    cell.SetCellValue(columnName);
                }

                //loops through data
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    IRow row = sheet1.CreateRow(i + 1);
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {

                        ICell cell = row.CreateCell(j);
                        String columnName = dt.Columns[j].ToString();

                        //Parseamos a float
                        float valor = 0;
                        //En caso de ser número, ponemos la celda como tipo número. 
                        if (float.TryParse(dt.Rows[i][columnName].ToString(), out valor)
                            && !columnName.ToString().ToLower().Contains("folio")) 
                        {
                            cell.SetCellValue(valor);                            
                            cell.CellStyle = cellNumberStyle;

                        }
                        else
                            cell.SetCellValue(dt.Rows[i][columnName].ToString());



                    }
                }

                //Hacemos el autosize 
                foreach (DataColumn column in dt.Columns)
                {
                    sheet1.AutoSizeColumn(column.Ordinal);
                }

                return workbook;

            }
            catch(Exception ex)
            {
                return null; 

            }
            // Declare one MemoryStream variable for write file in stream  
            /*var stream = new MemoryStream();
            workbook.Write(stream);

            string FilePath = "archivo001";

            //Write to file using file stream  
            FileStream file = new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write);
            stream.WriteTo(file);
            file.Close();
            stream.Close();*/
                
        }

        public IWorkbook GenerateExcelFileWithTitle(DataTable dt, string titulo)
        {
            try
            {
                IWorkbook workbook;
                workbook = new HSSFWorkbook();
                ISheet sheet1 = workbook.CreateSheet("Sheet 1");

                int fila_titulo = 0;
                IRow row_titulo = sheet1.CreateRow(fila_titulo);

                //Creamos el estilo para el titutlo
                IFont boldFontTitulo = workbook.CreateFont();
                boldFontTitulo.Boldweight = (short)FontBoldWeight.Bold;                
                boldFontTitulo.Color = NPOI.HSSF.Util.HSSFColor.Black.Index;
                boldFontTitulo.FontHeightInPoints = 15; 
                ICellStyle boldStyle_titulo = workbook.CreateCellStyle();
                //boldStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.SeaGreen.Index;
                //boldStyle.FillPattern = FillPattern.SolidForeground;
                boldStyle_titulo.SetFont(boldFontTitulo);
                boldStyle_titulo.Alignment = HorizontalAlignment.Center; 

                //Ponemos el contenido del título. 
                ICell cell_titulo = row_titulo.CreateCell(0);
                cell_titulo.CellStyle = boldStyle_titulo; 
                cell_titulo.SetCellValue(titulo);

                //Hacemos el merge 
                var merged = new NPOI.SS.Util.CellRangeAddress(fila_titulo, fila_titulo, 0, dt.Columns.Count == 0 ? 8 : dt.Columns.Count -1);
                sheet1.AddMergedRegion(merged); 
                
                


                int fila_inicial = fila_titulo+2;
                


                

                //make a header row
                IRow row1 = sheet1.CreateRow(fila_inicial);                

                IFont boldFont = workbook.CreateFont();
                boldFont.Boldweight = (short)FontBoldWeight.Bold;
                boldFont.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
                ICellStyle boldStyle = workbook.CreateCellStyle();
                boldStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.SeaGreen.Index;
                boldStyle.FillPattern = FillPattern.SolidForeground;
                boldStyle.SetFont(boldFont);

                //Creamos el estilo para manejar números. 
                ICellStyle cellNumberStyle = workbook.CreateCellStyle();
                cellNumberStyle.DataFormat = workbook.CreateDataFormat().GetFormat("0.00");


                for (int j = 0; j < dt.Columns.Count; j++)
                {

                    ICell cell = row1.CreateCell(j);
                    String columnName = dt.Columns[j].ToString();
                    cell.CellStyle = boldStyle; //Ponemos el estilo del encabezado.
                    cell.SetCellValue(columnName);
                }

                //loops through data
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    IRow row = sheet1.CreateRow(i + fila_inicial + 1);
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {

                        ICell cell = row.CreateCell(j);
                        String columnName = dt.Columns[j].ToString();

                        //Parseamos a float
                        float valor = 0;
                        //En caso de ser número, ponemos la celda como tipo número. 
                        if (float.TryParse(dt.Rows[i][columnName].ToString(), out valor)
                            && !columnName.ToString().ToLower().Contains("folio"))
                        {
                            cell.SetCellValue(valor);
                            cell.CellStyle = cellNumberStyle;

                        }
                        else
                            cell.SetCellValue(dt.Rows[i][columnName].ToString());



                    }
                }

                //Hacemos el autosize 
                foreach (DataColumn column in dt.Columns)
                {
                    sheet1.AutoSizeColumn(column.Ordinal);
                }

                return workbook;

            }
            catch (Exception ex)
            {
                return null;

            }
            // Declare one MemoryStream variable for write file in stream  
            /*var stream = new MemoryStream();
            workbook.Write(stream);

            string FilePath = "archivo001";

            //Write to file using file stream  
            FileStream file = new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write);
            stream.WriteTo(file);
            file.Close();
            stream.Close();*/

        }
    }
}
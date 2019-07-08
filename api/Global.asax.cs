using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace api
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            //Aquí agregamos la configuración de los encabezados para los CORS.

            // Las solicitudes Preflight vienen marcadas como Options. 
            // Se termina la respuesta para marcarla como correcta. 
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "*");
            // The following line solves the error message
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            // If any http headers are shown in preflight error in browser console add them below
            //HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, Pragma, Cache-Control, Authorization ");            
            HttpContext.Current.Response.AddHeader("Access-Control-Request-Headers", "*");

            //Aquíse ponen los headers que se admiten para el envío a través de las solicitudes AJAX. 
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, Pragma, Cache-Control, Authorization, id_tipo_de_usuario, id_usuario, token, pagina, nombre, modo, secret, nombre_de_usuario");


            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {

                //HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, Pragma, Cache-Control, Authorization, id_tipo_de_usuario, id_usuario");
                //HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
                HttpContext.Current.Response.StatusCode = 200;
                HttpContext.Current.Response.End();
            }
        }
    }
}
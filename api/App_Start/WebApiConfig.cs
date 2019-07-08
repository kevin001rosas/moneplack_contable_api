using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

            //Inicializamos el timer para evitar ataques de recuperación de contraseñas.
            api.Controllers.utilidades.timer.Elapsed += api.Controllers.utilidades.OnTimedEvent;
            api.Controllers.utilidades.timer.Enabled = true;
            api.Controllers.utilidades.timer.AutoReset = true;

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional, action = RouteParameter.Optional }
            );

            config.Formatters.Add(new BrowserJsonFormatter());

            //EnableCorsAttribute cors = new EnableCorsAttribute("*", "*", "*", "Content-Disposition");

            //config.EnableCors(cors);

            // Web API routes
            //config.MapHttpAttributeRoutes();
            
            
        }
    }
}

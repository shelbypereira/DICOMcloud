using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Cors;
using System.Web.Mvc;
using NLog;


namespace DICOMcloud.Wado
{
   
  
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
                
            // Web API configuration and services
            var cors = new EnableCorsAttribute ("*", "*", "*" );
            config.EnableCors(cors);
             // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new
                {
                    id = RouteParameter.Optional
                }
            );
            var log = LogManager.GetLogger("WebApiConfig");
            log.Info("completed HttpConfigurationRegistration and initialization of WebApiConfig");
        }
    }

  
}

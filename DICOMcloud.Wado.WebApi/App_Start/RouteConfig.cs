using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DICOMcloud.Wado
{
   public class RouteConfig
   {
      public static void RegisterRoutes(RouteCollection routes)
      {
         routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
           
         routes.MapRoute(
             name: "CornerstoneFrameRoute",
             url: "wadors/studies/{StudyInstanceUID}/series/{SeriesInstanceUID}/instances/{SOPInstanceUID}/metadata/frames/1",
             defaults: new { controller = "WadoRS", action = "GetInstance", id = UrlParameter.Optional }
         );

         routes.MapRoute(
             name: "Default",
               url: "{controller}/{action}/{id}",
             defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
         );
         
      }
   }
}

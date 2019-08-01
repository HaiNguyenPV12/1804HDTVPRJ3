using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AirlinesReservationSystem
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "ARS",
                url: "ARS/{action}/{id}",
                defaults: new { controller = "ARS", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ARSAdmin",
                url: "arsadmin/{action}/{id}",
                defaults: new { controller = "ARSAdmin", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "ARS", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

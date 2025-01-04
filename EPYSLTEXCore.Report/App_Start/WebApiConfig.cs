using System.Web.UI.WebControls;

namespace EPYSLTEXCore.Report
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Enable CORS
            //config.EnableCors(new EnableCorsAttribute("*", "*", "*")
            //{
            //    SupportsCredentials = true
            //});
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}

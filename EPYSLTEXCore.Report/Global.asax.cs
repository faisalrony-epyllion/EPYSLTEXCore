using Autofac;
using Autofac.Integration.Mvc;
using EPYSLTEXCore.Report.Repositories;
using EPYSLTEXCore.Report.Service;
using System.Web;
using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
namespace EPYSLTEXCore.Report
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {

            var builder = new ContainerBuilder();

            // Register MVC controllers
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // Register other dependencies (services, repositories, etc.)
            builder.RegisterType<ReportSuiteExternalSetupRepository>().As<IReportSuiteExternalSetupRepository>().InstancePerRequest();
            builder.RegisterType<ReportSuiteRepository>().As<IReportSuiteRepository>().InstancePerRequest();
            builder.RegisterType<ReportSuiteColumnValueRepository>().As<IReportSuiteColumnValueRepository>().InstancePerRequest();
            builder.RegisterType<ReportSuiteColumnValueRepository>().As<IReportSuiteColumnValueRepository>().InstancePerRequest();
            builder.RegisterType<ReportingService>().As<IReportingService>().InstancePerRequest();
            builder.RegisterType<RDLReportDocument>().AsSelf().InstancePerRequest();
            // Build the container
            var container = builder.Build();

            // Set the MVC dependency resolver to Autofac
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*"); // Or specify a specific origin
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization");

            // Handle OPTIONS request (preflight request)
            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                HttpContext.Current.Response.StatusCode = 200;
                HttpContext.Current.Response.End(); // End the request
            }
        }
    }
}

using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Infrastructure.Services;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEX.Web.Services;
using EPYSLTEXCore.Application.DataAccess;
using EPYSLTEXCore.Application.DataAccess.Interfaces;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Services;
using EPYSLTEXCore.Application.Services.Select;
using EPYSLTEXCore.Infrastructure.Data;
namespace EPYSLTEXCore.API.Extension
{
    public static class ServiceExtensions
    {
        public static void AddApplication(this IServiceCollection service)
        {


            service.AddScoped(typeof(IDapperCRUDService<>), typeof(DapperCRUDService<>));
            //service.AddTransient<IMenuService, MenuService>();
            //service.AddTransient<IMenuDAL, MenuDAL>();
            //service.AddTransient<IUserService, UserService>();
            service.AddTransient<ITokenBuilder, TokenBuilder>();
            service.AddTransient<IDeSerializeJwtToken, DeSerializeJwtToken>();
            //service.AddTransient<ICommonInterfaceService, CommonInterfaceService>();
            //service.AddTransient<ISelect2Service, Select2Service>();
            //service.AddTransient<ICommonHelpers, CommonHelpers>();
            //service.AddTransient<IYarnProductSetupService, YarnProductSetupService>();
            //service.AddTransient<IReportAPISetupService, ReportAPISetupService>();
            //service.AddTransient<ISignatureService, SignatureService>();

            foreach (var type in typeof(CommonHelperService).Assembly.GetTypes())
            {
                if (type.Name.EndsWith("Service") && type.IsClass && !type.IsAbstract)
                {
                    // Get all interfaces implemented by the type
                    var interfaces = type.GetInterfaces();
                    foreach (var @interface in interfaces)
                    {
                        // Register the service with its interface
                        service.AddScoped(@interface, type);
                    }
                }
            }

        }
    }


}
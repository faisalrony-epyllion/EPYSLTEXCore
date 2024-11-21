
using EPYSLTEXCore.Application.DataAccess.Interfaces;
using EPYSLTEXCore.Application.DataAccess;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Services;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Infrastructure.Services;
using EPYSLTEX.Web.Services;

namespace EPYSLTEXCore.API.Extension
{
    public static class ServiceExtensions
    {
        public static void AddApplication(this IServiceCollection service)
        {


            service.AddScoped(typeof(IDapperCRUDService<>), typeof(DapperCRUDService<>));
            service.AddTransient<IMenuService, MenuService>();
            service.AddTransient<IMenuDAL, MenuDAL>();
            service.AddTransient<IUserService, UserService>();
            service.AddTransient<ITokenBuilder, TokenBuilder>();
            service.AddTransient<IDeSerializeJwtToken, DeSerializeJwtToken>();
            service.AddTransient<IReportAPISetupService, ReportAPISetupService>();

        }
    }


}
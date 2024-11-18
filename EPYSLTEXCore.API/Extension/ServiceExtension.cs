
using EPYSLTEXCore.Application.DataAccess.Interfaces;
using EPYSLTEXCore.Application.DataAccess;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Services;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.API.Extension
{
    public static class ServiceExtensions
    {
        public static void AddApplication(this IServiceCollection service)
        {


            service.AddScoped(typeof(IDapperCRUDService<>), typeof(DapperCRUDService<>));
            service.AddTransient<IMenuService, MenuService>();
            service.AddTransient<IMenuDAL, MenuDAL>();
            service.AddTransient<IReportAPISetupService, ReportAPISetupService>();

        }
    }


}
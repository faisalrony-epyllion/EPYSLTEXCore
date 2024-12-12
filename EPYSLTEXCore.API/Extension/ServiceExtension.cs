using EPYSLTEX.Infrastructure.Services;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEX.Web.Services;
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
            service.AddScoped(typeof(IItemMasterService<>), typeof(ItemMasterService<>));
            service.AddScoped(typeof(IChildItemMasterService<>), typeof(ChildItemMasterService<>));
            service.AddTransient<ITokenBuilder, TokenBuilder>();
            service.AddTransient<IDeSerializeJwtToken, DeSerializeJwtToken>();
            service.AddTransient<ICommonHelpers, CommonHelpers>();



            string ListOfService = "IDapperCRUDService,IItemMasterService,IChildItemMasterService";
            string[] scopedServices = ListOfService.Split(',');
            foreach (var type in typeof(CommonHelperService).Assembly.GetTypes())
            {
                if (type.Name.EndsWith("Service") && type.IsClass && !type.IsAbstract)
                {
                    // Get all interfaces implemented by the type
                    var interfaces = type.GetInterfaces();
                    foreach (var @interface in interfaces)
                    {
                        // Register the service with its interface
                        if (Array.Exists(scopedServices, element => element == type.Name))
                        {

                        }
                        else
                        {
                            service.AddTransient(@interface, type);
                        }
                    }
                }
            }

            #region manual added services
            //foreach (var type in typeof(CommonHelperService).Assembly.GetTypes())
            //{
            //    if (type.Name.EndsWith("Service") && type.IsClass && !type.IsAbstract)
            //    {
            //        // Get all interfaces implemented by the type
            //        var interfaces = type.GetInterfaces();
            //        foreach (var @interface in interfaces)
            //        {
            //            // Register the service with its interface
            //            service.AddScoped(@interface, type);
            //        }
            //    }
            //}


            //service.AddTransient<ICommonInterfaceService, CommonInterfaceService>();
            //service.AddTransient<ISelect2Service, Select2Service>();
            //service.AddTransient<ICommonHelpers, CommonHelpers>();
            //service.AddTransient<ICommonHelperService, CommonHelperService>();
            //service.AddTransient<IFreeConceptService, FreeConceptService>();
            //service.AddTransient<IYarnProductSetupService, YarnProductSetupService>();
            //service.AddTransient<IReportAPISetupService, ReportAPISetupService>();
            //service.AddTransient<IFreeConceptMRService, FreeConceptMRService>();
            //service.AddTransient<IConceptStatusService, ConceptStatusService>();
            //service.AddTransient<IFBookingAcknowledgeService, FBookingAcknowledgeService>();
            //service.AddTransient<IYarnPRService, YarnPRService>();
            //service.AddTransient<IYarnReceiveService, YarnReceiveService>();
            //service.AddTransient<IYarnRackBinAllocationService, YarnRackBinAllocationService>();
            //service.AddTransient<IItemSetupService, ItemSetupService>();
            //service.AddTransient<IProjectionYarnBookingService, ProjectionYarnPurchaseBookingService>();    
            //service.AddTransient<IBondEntitlementService, BondEntitlementService>();
            //service.AddTransient<IBankLimitService, BankLimitService>();
            ////service.AddTransient<IYarnPOService, YarnPOService>();
            //service.AddTransient<IYarnPIReceiveService, YarnPIReceiveService>();
            //service.AddTransient<IFreeConceptService, FreeConceptService>();
            //service.AddTransient<ICommonHelperService, CommonHelperService>();
            //service.AddTransient<IFabricColorBookSetupService, FabricColorBookSetupService>();
            //service.AddTransient<IYarnQCReqService, YarnQCReqService>();
            //service.AddTransient<IYarnQCRemarksService, YarnQCRemarksService>();
            //service.AddTransient<IYarnQCIssueService, YarnQCIssueService>();
            //service.AddTransient<IYarnMRIRService, YarnMRIRService>();
            #endregion

        }
    }


}
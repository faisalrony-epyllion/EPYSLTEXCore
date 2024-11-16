using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPYSLTEXCore.Application.Entities;

namespace EPYSLTEXCore.Application.Interfaces
{
    public interface IReportAPISetupService : ICommonService<ReportAPISetup>
    {
        //Task<List<ReportAPISetup>> GetAllAPIReport();
        Task<ReportAPISetup> GetAPIReportByReportName(string reportName);
        //Task<ReportAPISetup> AddAPIReport(ReportAPISetup reportAPISetup);
        //Task<string> DeleteAPIReport(string reportName);
        Task<dynamic> GetDynamicReportAPIDataAsync(string username, string token, string reportName, string values);
    }
}

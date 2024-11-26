using EPYSLTEXCore.Report.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Report.Repositories
{
    public interface IReportSuiteColumnValueRepository
    {
        Task<List<ReportSuiteColumnValue>> GetAllAsync(int reportId);
    }

}

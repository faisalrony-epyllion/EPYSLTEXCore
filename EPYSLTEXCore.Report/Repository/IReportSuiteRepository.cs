using EPYSLTEXCore.Report.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Report.Repositories
{
    public interface IReportSuiteRepository
    {
        Task<ReportSuite> GetByIdAsync(int id);
        Task<ReportSuite> GetByNameAsync(string name);
        Task<List<dynamic>> GetDynamicDataDapperAsync(string query);
        DataSet LoadReportSourceDataSet(CommandType cmdType, string strCmdText, IDbDataParameter[] sqlParam);
        Task<DataSet> LoadReportParameterInfoAsync(int reportId);

    }

}

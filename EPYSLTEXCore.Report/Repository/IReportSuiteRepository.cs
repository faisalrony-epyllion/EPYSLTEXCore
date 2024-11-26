using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Report.Repositories
{
    public interface IReportSuiteRepository 
    {
        

        Task<DataSet> LoadReportParameterInfoAsync(int reportId);

        DataSet LoadReportSourceDataSet(CommandType cmdType, string strCmdText, IDbDataParameter[] sqlParam);

        Task<DataSet> LoadReportSourceDataSetAsync(CommandType cmdType, string strCmdText, IDbDataParameter[] sqlParam);
    }
}

using Dapper;
using EPYSLTEXCore.Report.Entities;
using EPYSLTEXCore.Report.Statics;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
namespace EPYSLTEXCore.Report.Repositories
{
    public class ReportSuiteExternalSetupRepository : IReportSuiteExternalSetupRepository
    {
        private readonly SqlConnection _connection;

        public ReportSuiteExternalSetupRepository()
        {
            _connection = new SqlConnection(ConfigurationManager.ConnectionStrings[AppConstants.GMT_CONNECTION].ConnectionString);
        }

        public async Task<ReportSuiteExternalSetup> GetByIdAndBuyerAsync(int Reportid, int ExternalId)
        {
            var sql = "SELECT * FROM ReportSuiteExternalSetup WHERE  REPORTID=@Reportid and ExternalID=@ExternalId";


            _connection.Open();
            var reportSuiteExtSetUp = await _connection.QuerySingleOrDefaultAsync<ReportSuiteExternalSetup>(sql, new { Reportid = Reportid, ExternalId= ExternalId });
            return reportSuiteExtSetUp;
        }


        public ReportSuiteExternalSetup GetByIdAndBuyer(int Reportid, int ExternalId)
        {
            var sql = "SELECT * FROM ReportSuiteExternalSetup WHERE  REPORTID=@Reportid and ExternalID=@ExternalId";


            _connection.Open();
            var reportSuiteExtSetUp =  _connection.QuerySingleOrDefault<ReportSuiteExternalSetup>(sql, new { Reportid = Reportid, ExternalId = ExternalId });
            return reportSuiteExtSetUp;
        }



    }
}
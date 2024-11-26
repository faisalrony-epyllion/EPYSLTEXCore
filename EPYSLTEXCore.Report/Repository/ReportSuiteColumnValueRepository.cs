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
    public class ReportSuiteColumnValueRepository : IReportSuiteColumnValueRepository
    {
        private readonly SqlConnection _connection;

        public ReportSuiteColumnValueRepository(string connectionString)
        {
            _connection = new SqlConnection(ConfigurationManager.ConnectionStrings[AppConstants.GMT_CONNECTION].ConnectionString);
        }

        public async Task<List<ReportSuiteColumnValue>> GetAllAsync(int reportId)
        {
            var sql = "SELECT * FROM ReportSuiteColumnValue WHERE ReportID = @Id";

            try
            {
                await _connection.OpenAsync();

                // Use QueryAsync to fetch a list, even if there's only one result
                var reportSuite = await _connection.QueryAsync<ReportSuiteColumnValue>(sql, new { Id = reportId });


                return reportSuite.ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching report suite data.", ex);
            }
            finally
            {
                _connection.Close();
            }
        }


        

    }
}
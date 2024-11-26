using System;
using System.Data;
using System.Threading.Tasks;

namespace EPYSLTEX.Infrastructure.Data.Repositories
{
    public class ReportSuiteRepository 
    {
        public ReportSuiteRepository()
          
        {
        }

 
        public async Task<DataSet> LoadReportParameterInfoAsync(int reportId)
        {
            var command = _dbContext.Database.Connection.CreateCommand();
            command.CommandText = $@"Declare @SQL As Varchar(8000)
	                                Set @SQL = (Select REPORT_SQL From ReportSuite Where ReportID = {reportId})
	                                IF(@SQL Is Null Or @SQL = '')
		                            Set @SQL = 'Select Null As Dummy Where 1 = 2'
	                                Exec (@SQL)";

            try
            {
                _dbContext.Database.Connection.Open();
                var reader = await command.ExecuteReaderAsync();
                var dataSet = ExtensionMethods.DataReaderToDataSet(reader);
                return dataSet;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                _dbContext.Database.Connection.Close();
            }
        }

        public DataSet LoadReportSourceDataSet(CommandType cmdType, string strCmdText, IDbDataParameter[] sqlParam)
        {
            var dataSet = new DataSet();
            IDataReader reader;
            IDbDataParameter parameter;

            try
            {
                var command = _dbContext.Database.Connection.CreateCommand();
                command.CommandTimeout = 600;
                command.CommandType = cmdType;
                command.CommandText = strCmdText;
                foreach (var param in sqlParam)
                {
                    parameter = command.CreateParameter();
                    parameter.ParameterName = $"{param.ParameterName}";
                    parameter.Value = param.Value;
                    command.Parameters.Add(parameter);
                }

                _dbContext.Database.Connection.Open();
                reader = command.ExecuteReader();
                DataTable table;
                do
                {
                    table = new DataTable();
                    table.Load(reader);
                    dataSet.Tables.Add(table);

                } while (!reader.IsClosed && reader.NextResult());

                reader.Close();

                return dataSet;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dbContext.Database.Connection.Close();
            }
        }

        public async Task<DataSet> LoadReportSourceDataSetAsync(CommandType cmdType, string strCmdText, IDbDataParameter[] sqlParam)
        {
            IDataReader reader;
            IDbDataParameter parameter;

            try
            {
                var command = _dbContext.Database.Connection.CreateCommand();
                command.CommandType = cmdType;
                foreach(var param in sqlParam)
                {
                    parameter = command.CreateParameter();
                    parameter.ParameterName = $"@{param.ParameterName}";
                    parameter.Value = param.Value;
                    command.Parameters.Add(parameter);
                }

                reader = await command.ExecuteReaderAsync();

                var dataSet = ExtensionMethods.DataReaderToDataSet(reader);
                return dataSet;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                _dbContext.Database.Connection.Close();
            }
        }

  
    }
}

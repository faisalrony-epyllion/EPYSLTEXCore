﻿using Dapper;
using EPYSLTEXCore.Report.Entities;
using EPYSLTEXCore.Report.Statics;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
namespace EPYSLTEXCore.Report.Repositories
{
    public class ReportSuiteRepository : IReportSuiteRepository
    {
        
        private readonly SqlConnection _connection;

        public ReportSuiteRepository()
        {
           
            _connection = new SqlConnection(ConfigurationManager.ConnectionStrings[AppConstants.GMT_CONNECTION].ConnectionString);
        }



        public async Task<ReportSuite> GetByIdAsync(int id)
        {
            var sql = "SELECT * FROM ReportSuite WHERE REPORTID = @Id and isapi=1";

       
                _connection.Open();
                var reportSuite = await _connection.QuerySingleOrDefaultAsync<ReportSuite>(sql, new { Id = id });
            _connection.Close();
            return reportSuite;
            
        }
        public  ReportSuite GetById(int id)
        {
            var sql = "SELECT * FROM ReportSuite WHERE REPORTID = @Id and isapi=1";


            _connection.Open();
            var reportSuite =  _connection.QuerySingleOrDefault<ReportSuite>(sql, new { Id = id });
            _connection.Close();
            return reportSuite;

        }
        public async Task<ReportSuite> GetByNameAsync(string name)
        {
            var sql = "SELECT * FROM ReportSuite WHERE REPORT_NAME = @name";


            _connection.Open();
            var reportSuite = await _connection.QuerySingleOrDefaultAsync<ReportSuite>(sql, new { name = name });
            _connection.Close();
            return reportSuite;

        }

        public async Task<List<dynamic>> GetDynamicDataDapperAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryAsync<dynamic>(query); 
                return records.ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error executing query.", ex);
            }
            finally
            {
                _connection.Close();
            }
        }
        public List<dynamic> GetDynamicData(string query, bool IsSP, params SqlParameter[] parameters)
        {
            try
            {
                _connection.Open();

            var dynamicParameters = new DynamicParameters();
                foreach (var param in parameters)
                {
                    dynamicParameters.Add(param.ParameterName, param.Value);
                }

                var result = _connection.Query(
                    query,
                    dynamicParameters,
                    commandType: IsSP ? CommandType.StoredProcedure : CommandType.Text
                ).ToList();
                _connection.Close();
                return result;
            }
            catch (Exception ex)
            {
                _connection.Close();
                throw new InvalidOperationException("Error executing query.", ex);
                
            }
         

        }
        public async Task<DataSet> LoadReportParameterInfoAsync(int reportId)
        {
          
            string query = $@" Declare @SQL As Varchar(8000)
                        Set @SQL = (Select REPORT_SQL From ReportSuite Where ReportID = @ReportId)
                        IF(@SQL Is Null Or @SQL = '')
                            Set @SQL = 'Select Null As Dummy Where 1 = 2'
                        Exec (@SQL)";

            try
            {
              
                var result = await _connection.QueryAsync(query, new { ReportId = reportId });

                
                var dataTable = new DataTable();

                if (result != null && result.Any())
                {
                   
                    var firstRecord = result.First();
                    foreach (var key in firstRecord)
                    {
                        dataTable.Columns.Add(key.Key);
                    }

       
                    foreach (var row in result)
                    {
                        var dataRow = dataTable.NewRow();
                        foreach (var key in row)
                        {
                            dataRow[key.Key] = key.Value;
                        }
                        dataTable.Rows.Add(dataRow);
                    }
                }


                var dataSet = new DataSet();
                dataSet.Tables.Add(dataTable);

                return dataSet;
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                throw new InvalidOperationException("Error executing the report query", ex);
            }
        }



        public DataSet LoadReportSourceDataSet(CommandType cmdType, string strCmdText, IDbDataParameter[] sqlParam)
        {
            try
            {
              
                     _connection.Open();

                    using (var command = _connection.CreateCommand())
                    {
                        command.CommandText = strCmdText;
                        command.CommandType = cmdType;

                        // Add parameters to the command
                        if (sqlParam != null)
                        {
                            foreach (var param in sqlParam)
                            {
                                var parameter = command.CreateParameter();
                                parameter.ParameterName = param.ParameterName;
                                parameter.Value = param.Value ?? DBNull.Value;
                                parameter.DbType = param.DbType;
                                command.Parameters.Add(parameter);
                            }
                        }

                        using (var reader =  command.ExecuteReader())
                        {
                     
                            return ExtensionMethods.DataReaderToDataSet(reader);
                        }
                    }
                }
            
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while loading the report source data.", ex);
            }
        }


    }
}
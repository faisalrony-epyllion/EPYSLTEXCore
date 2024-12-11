using Dapper;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Transactions;
using static Dapper.SqlMapper;

namespace EPYSLTEX.Infrastructure.Services
{
    public class CommonInterfaceService : ICommonInterfaceService
    {
        private readonly IDapperCRUDService<CommonInterfaceMaster> _service;

        private readonly IConfiguration _configuration;
        private readonly SqlConnection _connection = null;

        public CommonInterfaceService(IDapperCRUDService<CommonInterfaceMaster> service, IConfiguration configuration)
        {
            _configuration = configuration;
            _service = service;
            _connection = new SqlConnection(_configuration.GetConnectionString(AppConstants.TEXTILE_CONNECTION)); ;
        }



        public async Task<CommonInterfaceMaster> GetConfigurationAsync(int menuId)
        {
            var query = $@"
            Select * From CommonInterfaceMaster Where MenuId = {menuId}

            Select C.* 
            From CommonInterfaceChild C
            Inner Join CommonInterfaceMaster M On C.ParentID = M.MasterID
            Where M.MenuId = {menuId}

            Select C.* 
            From CommonInterfaceChildGrid C
            Inner Join CommonInterfaceMaster M On C.ParentID = M.MasterID
            Where M.MenuId = {menuId}

            Select C.* 
            From CommonInterfaceChildGridColumn C
            Inner Join CommonInterfaceMaster M On C.ParentID = M.MasterID
            Where M.MenuId = {menuId}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                CommonInterfaceMaster data = await records.ReadFirstOrDefaultAsync<CommonInterfaceMaster>();
                data.Childs = records.Read<CommonInterfaceChild>().ToList();
                data.ChildGrids = records.Read<CommonInterfaceChildGrid>().ToList();
                data.ChildGridColumns = records.Read<CommonInterfaceChildGridColumn>().ToList();

                return data;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<IEnumerable<CommonInterfaceMaster>> GetConfigurationAsyncByApplicationID(int applicationId)
        {
            string query = $@"
                    -- Select MenuId and store in temporary table
                    SELECT MenuId
                    INTO #MenuIds
                    FROM EPYSL..Menu
                    WHERE ApplicationID = {applicationId};

                    -- Select from CommonInterfaceMaster
                    SELECT CM.*
                    FROM CommonInterfaceMaster CM
                    INNER JOIN #MenuIds M ON CM.MenuId = M.MenuId;

                    -- Select from CommonInterfaceChild
                    SELECT C.*
                    FROM CommonInterfaceChild C
                    INNER JOIN CommonInterfaceMaster M ON C.ParentID = M.MasterID
                    INNER JOIN #MenuIds M2 ON M.MenuId = M2.MenuId;

                    -- Select from CommonInterfaceChildGrid
                    SELECT C.*
                    FROM CommonInterfaceChildGrid C
                    INNER JOIN CommonInterfaceMaster M ON C.ParentID = M.MasterID
                    INNER JOIN #MenuIds M2 ON M.MenuId = M2.MenuId;

                    -- Select from CommonInterfaceChildGridColumn
                    SELECT C.*
                    FROM CommonInterfaceChildGridColumn C
                    INNER JOIN CommonInterfaceMaster M ON C.ParentID = M.MasterID
                    INNER JOIN #MenuIds M2 ON M.MenuId = M2.MenuId;

                    -- Drop the temporary table
                    DROP TABLE #MenuIds;";

            try
            {
                await _connection.OpenAsync();

                var records = await _connection.QueryMultipleAsync(query);

                // Read the first result set (CommonInterfaceMaster)
                var commonInterfaceMasters = await records.ReadAsync<CommonInterfaceMaster>();

                // Read the second result set (CommonInterfaceChild)
                var commonInterfaceChildren = await records.ReadAsync<CommonInterfaceChild>();

                // Read the third result set (CommonInterfaceChildGrid)
                var commonInterfaceChildGrids = await records.ReadAsync<CommonInterfaceChildGrid>();

                // Read the fourth result set (CommonInterfaceChildGridColumn)
                var commonInterfaceChildGridColumns = await records.ReadAsync<CommonInterfaceChildGridColumn>();

                // Process the results (Optional - depending on how you want to use them)
                foreach (var master in commonInterfaceMasters)
                {
                    // Optionally, link child data to the parent, if needed
                    master.Childs = commonInterfaceChildren.Where(c => c.ParentId == master.MasterID).ToList();
                    master.ChildGrids = commonInterfaceChildGrids.Where(c => c.ParentId == master.MasterID).ToList();
                    master.ChildGridColumns = commonInterfaceChildGridColumns.Where(c => c.ParentId == master.MasterID).ToList();
                }

                return commonInterfaceMasters;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task<CommonInterfaceMaster> GetCommonInterfaceMasterChildAsync(int menuId)
        {
            var query = $@"
            Select * From CommonInterfaceMaster Where MenuId = {menuId}
            Select C.* 
            From CommonInterfaceChild C
            Inner Join CommonInterfaceMaster M On C.ParentID = M.MasterID
            Where M.MenuId = {menuId}";

            var records = await _connection.QueryMultipleAsync(query);
            CommonInterfaceMaster data = await records.ReadFirstOrDefaultAsync<CommonInterfaceMaster>();
            data.Childs = records.Read<CommonInterfaceChild>().ToList();
            return data;
        }
        public async Task<dynamic> GetFinderData(string sqlQuery, string conKey, string primaryKeyColumn, PaginationInfo paginationInfo)
        {
            var query = sqlQuery;
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? $@"Order By LEN({primaryKeyColumn}), {primaryKeyColumn} ASC" : paginationInfo.OrderBy;
            var isSp = sqlQuery.ToLower().Contains("sp");
            query = isSp ? sqlQuery : $@"
                 {sqlQuery}
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";
            var commandType = isSp ? CommandType.StoredProcedure : CommandType.Text;
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString(conKey));

            // Create a DynamicParameters object
            var parameters = new DynamicParameters();
            parameters.Add("filterBy", paginationInfo.FilterBy); // filterBy parameter
            parameters.Add("orderBy", orderBy); // orderBy parameter
            parameters.Add("@pageBy", paginationInfo.PageBy); // pageBy parameter
            var records = await _service.GetDynamicDataAsync(query, conn, parameters, commandType);

            return records;
        }

        public async Task<dynamic> GetDynamicDataAsync(string sqlQuery, string conKey, object param)
        {
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString(conKey));

            var records = await _service.GetDynamicDataAsync(sqlQuery, conn, param);

            return records;
        }
        public async Task<dynamic> GetDynamicDataAsync(string sqlQuery, string conKey)
        {
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString(conKey));

            var records = await _service.GetDynamicDataAsync(sqlQuery, conn);

            return records;
        }
        public async Task<CommonInterfaceMaster> GetCommonInterfaceChildAsync(int menuId)
        {
            var query = $"Select * From CommonInterfaceChild Where MenuId = {menuId}";
            return await _service.GetFirstOrDefaultAsync(query);
        }



        public async Task<int> ExecuteAsync(string query, object param)
        {
            return await _service.ExecuteAsync(query, param);
        }
        public async Task Save(List<string> tableNames, List<object> objLst, List<string> conKey, List<string> primaryKeyColumns)
        {
            try
            {
                SqlConnection conn = new SqlConnection(_configuration.GetConnectionString(conKey.FirstOrDefault()));
                await conn.OpenAsync(); // Ensure connection is opened

                using (var transaction = conn.BeginTransaction()) // Begin a transaction
                {
                    try
                    {

                        // Ensure the lists are of the same length
                        int listCount = tableNames.Count;
                        if (listCount == objLst.Count && listCount == primaryKeyColumns.Count)
                        {
                            for (int i = 0; i < listCount; i++)
                            {
                                string tableName = tableNames[i];
                                object obj = objLst[i];
                                string primaryKey = primaryKeyColumns[i];
                                int number = await _service.AddUpDateDeleteDynamicObjectAsync(tableName, obj, primaryKeyColumns, conn, transaction);
                                      
                            }
                        }




                        // Commit transaction if all operations succeeded
                        await transaction.CommitAsync();

                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Error during transaction: {ex.Message}");
                        throw; // Re-throw the exception to be handled by the caller
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }


    }
}
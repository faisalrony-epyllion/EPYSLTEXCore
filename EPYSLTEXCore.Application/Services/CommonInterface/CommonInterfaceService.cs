using Dapper;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace EPYSLTEX.Infrastructure.Services
{
    public class CommonInterfaceService : ICommonInterfaceService
    {
        private readonly IDapperCRUDService<CommonInterfaceMaster> _service;
        
        private readonly IConfiguration _configuration;
        private readonly SqlConnection _connection = null;

        public CommonInterfaceService(IDapperCRUDService<CommonInterfaceMaster> service,  IConfiguration configuration)
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
            finally {
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
        public async Task<dynamic> GetFinderData(string sqlQuery,string conKey, string primaryKeyColumn,PaginationInfo paginationInfo)
        {
            var query = sqlQuery;
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? $@"Order By LEN({primaryKeyColumn}), {primaryKeyColumn} ASC" : paginationInfo.OrderBy;
            var isSp = sqlQuery.ToLower().Contains("sp");
            query =isSp? sqlQuery: $@"
                 {sqlQuery}
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";
            var commandType = isSp ? CommandType.StoredProcedure : CommandType.Text;
            SqlConnection conn =  new SqlConnection(_configuration.GetConnectionString(conKey));

          // Create a DynamicParameters object
            var parameters = new DynamicParameters();
            parameters.Add("filterBy", paginationInfo.FilterBy); // filterBy parameter
            parameters.Add("orderBy", orderBy); // orderBy parameter
            parameters.Add("@pageBy", paginationInfo.PageBy); // pageBy parameter
            var records = await _service.GetDynamicDataAsync(query,conn, parameters, commandType);
            
            return records;
        }
 
        public async Task<dynamic> GetDynamicDataAsync(string sqlQuery, string conKey,object param)
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
    }
}
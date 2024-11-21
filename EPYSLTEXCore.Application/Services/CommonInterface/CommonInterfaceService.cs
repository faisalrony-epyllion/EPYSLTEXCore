using Dapper;
using EPYSLTEX.Core.Entities;
using EPYSLTEX.Core.Entities.Tex;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.Application.DTO;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

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
            finally {
                _connection.Close();
            }
        }

        public async Task<CommonInterfaceMaster> GetMasterDetailsAsync(int menuId)
        {
            var query = $"Select * From CommonInterfaceMaster Where MenuId = {menuId}";
            return await _service.GetFirstOrDefaultAsync(query);
        }

        public async Task<List<dynamic>> GetPagedAsync(string query, PaginationInfo paginationInfo)
        {
            var orderBy = string.IsNullOrEmpty(paginationInfo.OrderBy) ? "" : paginationInfo.OrderBy;

            string sql = $@"
                {query}
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.OrderBy}";

            return await _service.GetDynamicDataAsync(sql);
        }

        public async Task<int> ExecuteAsync(string query, object param)
        {
            return await _service.ExecuteAsync(query, param);
        }
    }
}
﻿using EPYSLTEXCore.Infrastructure.Entities;

namespace EPYSLTEX.Core.Interfaces.Services
{
    public interface ICommonInterfaceService
    {

        Task<CommonInterfaceMaster> GetConfigurationAsync(int menuId);

        Task<int> ExecuteAsync(string query, object param);

        Task<CommonInterfaceMaster> GetCommonInterfaceChildAsync(int menuId);
        Task<CommonInterfaceMaster> GetCommonInterfaceMasterChildAsync(int menuId);

        Task<dynamic> GetDynamicDataAsync(string sqlQuery, string conKey, object param);
        Task<dynamic> GetFinderData(string sqlQuery, string conKey, string primaryKeyColumn, PaginationInfo paginationInfo);
        Task<dynamic> GetDynamicDataAsync(string sqlQuery, string conKey);
        Task Save(string tableName, object obj, string conKey, List<string> primaryKeyColumns, string status = "");
        Task<IEnumerable<CommonInterfaceMaster>> GetConfigurationAsyncByApplicationID(int applicationId);


    }
}

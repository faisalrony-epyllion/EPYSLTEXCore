using EPYSLTEXCore.Infrastructure.Entities;

namespace EPYSLTEX.Core.Interfaces.Services
{
    public interface ICommonInterfaceService
    {

        Task<CommonInterfaceMaster> GetConfigurationAsync(int menuId);
        Task<CommonInterfaceMaster> GetMasterDetailsAsync(int menuId);
        Task<int> ExecuteAsync(string query, object param);
        

        Task<dynamic> GetFinderData(string sqlQuery, string conKey, string primaryKeyColumn, PaginationInfo paginationInfo);
    }
}

using EPYSLTEX.Core.DTOs;
using EPYSLTEX.Core.Entities.Gmt;
using EPYSLTEX.Core.Entities.Tex;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPYSLTEX.Core.Interfaces.Services
{
    public interface ICommonInterfaceService
    {
       
        Task<CommonInterfaceMaster> GetConfigurationAsync(int menuId);
        Task<CommonInterfaceMaster> GetMasterDetailsAsync(int menuId);
        Task<int> ExecuteAsync(string query, object param);
        Task<List<dynamic>> GetPagedAsync(string query, PaginationInfo paginationInfo);
    }
}

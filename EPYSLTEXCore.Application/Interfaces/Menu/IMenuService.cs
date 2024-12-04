using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities;

namespace EPYSLTEXCore.Application.Interfaces
{
    public interface IMenuService : ICommonService<Menu>
    {
        Task<List<MenuDTO>> GetMenusAsync(int userId, int applicationId, int companyId);
        Task<List<MenuDTO>> GetAllMenuReport(int userId, int applicationId, int companyId);
    }
}


using EPYSLTEXCore.Application.DTO;
using EPYSLTEXCore.Application.Entities;

namespace EPYSLTEXCore.Application.Interfaces
{
    public interface IMenuService:ICommonService<Menu>
    {
        Task<List<MenuDTO>> GetMenusAsync(int userId, int applicationId, int companyId);
    }
}

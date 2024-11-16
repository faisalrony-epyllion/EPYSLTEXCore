using EPYSLEMSCore.Application.Entities;
using EPYSLEMSCore.Application.Interfaces;
using EPYSLTEXCore.Application.DTO;

namespace EPYSLTEXCore.Application.Interfaces
{
    public interface IMenuService:ICommonService<Menu>
    {
        Task<List<MenuDTO>> GetMenusAsync(int userId, int applicationId, int companyId);
    }
}

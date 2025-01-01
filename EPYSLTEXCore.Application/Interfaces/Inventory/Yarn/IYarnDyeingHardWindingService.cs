using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYarnDyeingHardWindingService
    {
        Task<List<HardWindingMaster>> GetPagedAsync(Status status, string pageName, PaginationInfo paginationInfo);
        Task<HardWindingMaster> GetNewAsync(int YDBookingMasterID, int YDBatchID);
        Task<HardWindingMaster> GetAsync(int id);
        Task<HardWindingMaster> GetAllAsync(int id);
        Task SaveAsync(HardWindingMaster entity);
        Task UpdateEntityAsync(HardWindingMaster entity);
    }
}

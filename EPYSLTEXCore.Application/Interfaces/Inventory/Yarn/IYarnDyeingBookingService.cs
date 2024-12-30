using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYarnDyeingBookingService
    {
        Task<List<YDBookingMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo, string pageName);

        Task<YDBookingMaster> GetNew(string id, string pName);

        Task<YDBookingMaster> GetAsync(int id, string pName);
        Task<YDBookingMaster> GetReviseAsync(int id, string GroupConceptNo, string pName);

        Task<YDBookingMaster> GetAllAsync(int id);

        Task SaveAsync(YDBookingMaster entity);
        Task<string> SaveYDBNoAsync(YDBookingMaster entity);

        Task UpdateEntityAsync(YDBookingMaster entity);
    }
}

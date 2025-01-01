using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface ISendToYDStoreService
    {
        Task<List<SendToYDStoreMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<SendToYDStoreMaster> GetNewAsync(int newId);
        Task<SendToYDStoreMaster> GetAsync(int id);
        Task SaveAsync(SendToYDStoreMaster entity);
        Task<SendToYDStoreMaster> GetAllAsync(int id);
    }
}

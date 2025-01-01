using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYDQCService
    {
        Task<List<YDQCMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<YDQCMaster> GetNewAsync(int newId);
        Task<YDQCMaster> GetAsync(int id);
        Task SaveAsync(YDQCMaster entity);
        Task<YDQCMaster> GetAllAsync(int id);
    }
}

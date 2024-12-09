using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;

namespace EPYSLTEXCore.Application.Interfaces
{
    public interface IItemSetupService
    {
        Task<List<YarnProductSetupChildDTO>> GetProductSetupAsync(int fiberTypeId);
        Task<List<YarnProcessSetupMasterDTO>> GetProcessSetupAsync(int fiberTypeId);
        Task<ItemInformation> GetItemStructureBySubGroup(string subGroupName);
        Task<List<ItemStructureDTO>> GetItemStructureForDisplayBySubGroup(string subGroupName);
        Task SaveAsync(ItemSegmentMaster entity, int userId);
        Task<bool> GetCacheUseSetup();
        Task SaveCacheForYarnSegmentFilterUpdateTimeAsync(string runningMode);
        Task<List<CacheResetSetup>> GetCacheResetSetupsAsync(PaginationInfo paginationInfo);
    }
}

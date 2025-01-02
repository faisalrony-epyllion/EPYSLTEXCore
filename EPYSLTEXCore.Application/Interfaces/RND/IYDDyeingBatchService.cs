using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.RND
{
    public interface IYDDyeingBatchService
    {
        Task<List<YDDyeingBatchMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<YDDyeingBatchMaster> GetNewAsync(int newId);
        Task<List<YDDyeingBatchMaster>> GetNewMultiSelectAsync(string batchIDs);
        Task<YDDyeingBatchMaster> GetAsync(int id);
        Task<List<YDDyeingBatchMaster>> GetBatchListAsync(string batchIds);
        Task<List<YDDyeingBatchMaster>> GetBatchDetails(string batchIds);
        Task<List<YDDyeingBatchChildFinishingProcess>> GetFinishingProcessAsync(int conceptID, int colorID);
        Task<List<YDDyeingBatchChildFinishingProcess>> GetFinishingProcessByYDDyeingBatchAsync(int dBatchID, int colorID);
        Task SaveAsync(YDDyeingBatchMaster entity);
        Task SaveAsyncRecipeCopy(YDDyeingBatchMaster entity);
        Task<YDDyeingBatchMaster> GetAllByIDAsync(int id);
        Task UpdateEntityAsync(YDDyeingBatchMaster entity);
        Task<List<YDDyeingBatchMaster>> GetYDDyeingBatchs(PaginationInfo paginationInfo, string colorName, string conceptNo);
    }
}

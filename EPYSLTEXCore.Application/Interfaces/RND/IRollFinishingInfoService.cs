using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEX.Core.Interfaces.Services
{
    public interface IRollFinishingInfoService
    {
        Task<List<DyeingBatchMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);

        Task<DyeingBatchMaster> GetAsync(int batchId,Status status);

        Task<FinishingProcessMaster> GetMachineParam(int fmsId, int fpChildId);

        Task<DyeingBatchMaster> GetAllByIDAsync(int id);

        Task<DyeingBatchItem> GetAllByBDIIDAsync(int id);

        Task<List<DyeingBatchChildFinishingProcess>> GetExistFinishingProcessList(int dbId);

        Task<List<DyeingBatchChildFinishingProcess>> GetNewFinishingProcessList(int conceptId, int colorId);

        Task SaveAsync(DyeingBatchMaster entity);

        Task SaveBatchItemAsync(DyeingBatchItem entity);

        Task UpdateFinishingProcess(List<DyeingBatchChildFinishingProcess> finishingProcessList);

        Task UpdateBDSTNA_FinishingPlanAsync(int DBatchID);

    }
}
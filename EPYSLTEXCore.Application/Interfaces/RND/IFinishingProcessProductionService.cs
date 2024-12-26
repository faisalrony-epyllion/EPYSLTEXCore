using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.RND
{
    public interface IFinishingProcessProductionService
    {
        Task<List<FinishingProcessMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);

        Task<FinishingProcessMaster> GetAsync(int batchId,Status status);

        Task<FinishingProcessMaster> GetMachineParam(int fmsId, int fpChildId);

        Task<FinishingProcessMaster> GetAllByIDAsync(int id);

        Task SaveAsync(FinishingProcessMaster entity);
    }
}
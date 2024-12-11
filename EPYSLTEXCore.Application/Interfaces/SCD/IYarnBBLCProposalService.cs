using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.SCD
{
    public interface IYarnBBLCProposalService
    {
        Task<List<YarnBBLCProposalMaster>> GetListAsync(Status status, bool isCDAPage, PaginationInfo paginationInfo);
        Task<YarnBBLCProposalMaster> GetNewAsync(int[] piReceiveMasterIdArray);
        Task<List<YarnBBLCProposalMaster>> GetBBLCProposalsForMergeAsync(int companyId, int supplierId, bool isCDAPage);
        Task<YarnBBLCProposalMaster> GetAsync(int id);
        Task<YarnBBLCProposalMaster> GetNatureAsync(int id);
        Task<List<YarnBBLCProposalMaster>> GetProposeContractForPCAsync(int id, PaginationInfo paginationInfo);
        Task<List<YarnBBLCProposalMaster>> GetLCContractNoAsync(int id, PaginationInfo paginationInfo);
        Task<YarnBBLCProposalMaster> GetAllByIDAsync(int id);
        Task<YarnBBLCProposalMaster> GetMergedDataAsync(int proposalId, int[] piReceiveMasterIdArray);
        Task SaveAsync(YarnBBLCProposalMaster entity);
    }
}

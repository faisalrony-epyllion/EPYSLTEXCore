using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEX.Core.Interfaces.Services
{
    public interface IKnittingProductionService
    {
        Task<List<KnittingProduction>> GetPagedAsync(Status status, PaginationInfo paginationInfo);

        Task<KnittingProduction> GetNewAsync(int kJobCardMasterID, int isBDS, int conceptId, string grpConceptNo);
        Task<KnittingProduction> GetNewByKJobCardNo(string kJobCardNo);
        Task<List<KnittingProduction>> GetByJobCardId(int kJobCardMasterID);
        Task<KnittingProduction> GetNewAsyncp();

        Task<KnittingProduction> GetNewAsyncpid(string pid);

        Task<KnittingProduction> GetAsync(int id, int isBDS, int conceptId, string grpConceptNo);

        Task<KnittingProduction> GetjobCardMasterNo(string jobCardMasterId);

        Task<List<KnittingProduction>> GetRollAsync(int id);
        Task<List<KnittingProduction>> GetGRollAsync(int id);
        Task<KnittingProduction> GetDetailsAsync(int ids);

        Task<List<KnittingProduction>> GetByJobCardAsync(int id);

        Task<List<KnittingProduction>> GetDetailsByParentGRollIdAsync(int parentGRollID);

        Task<List<KnittingProduction>> GetDetailsAsync(IEnumerable<int> ids);

        Task SaveAsync(KnittingProduction entity);
        Task SaveAsync(List<KnittingProduction> list, int kJobCardMasterID = 0);
        Task<int> UpdateAsync(KnittingProduction knittingProduction);
        Task UpdateJobCardAsync(int KJobCardMasterID);
        Task UpdateBDSTNA_KnittingPlanAsync(int KJobCardMasterID);
        Task<List<KnittingProduction>> GetKProductionsByConcept(string conceptNo);
        Task<List<KnittingProduction>> GetKProductionsByConceptId(int conceptId);
        Task<DyeingBatchItemRoll> GetDyingBatchItemRoll(int id);
    }
}
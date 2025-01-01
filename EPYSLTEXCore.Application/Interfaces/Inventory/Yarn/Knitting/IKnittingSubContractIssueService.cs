using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn.Knitting;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn.Knitting
{
    public interface IKnittingSubContractIssueService
    {
        Task<List<KnittingSubContractIssueMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);

        Task<KnittingSubContractIssueMaster> GetNewAsync(int yBookingId, string reqType, string programName = null);

        Task<KnittingSubContractIssueMaster> GetAsync(int id, string reqType, string programName = null);

        Task SaveAsync(KnittingSubContractIssueMaster entity, List<YarnReceiveChildRackBin> rackBins = null);
        Task SaveAsyncSC(KnittingSubContractIssueMaster entity, List<YarnReceiveChildRackBin> rackBins);

        Task<KnittingSubContractIssueMaster> GetAllAsync(int id);
    }
}

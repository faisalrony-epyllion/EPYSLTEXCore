using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYDReqIssueService
    {
        Task<List<YDReqIssueMaster>> GetPagedAsync(Status status, string pageName, PaginationInfo paginationInfo);
        Task<List<YDReqMaster>> GetYDRequisitionsAsync(Status status, string pageName, PaginationInfo paginationInfo);
        Task<YDReqIssueMaster> GetNewAsync(int YDReqMasterID);
        Task<YDReqIssueMaster> GetAsync(int id);
        Task<YDReqIssueMaster> GetAllAsync(int id);
        Task SaveAsync(YDReqIssueMaster entity, List<YarnReceiveChildRackBin> rackBins = null);
        Task SaveAsyncYD(YDReqIssueMaster entity, List<YarnReceiveChildRackBin> rackBins);
    }
}

using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces
{
    public interface IYarnReceiveService
    {
        Task<List<YarnReceiveMaster>> GetPagedAsync(Status status, bool isCDAPage, PaginationInfo paginationInfo);
        Task<YarnReceiveMaster> GetNewAsync(int CIID, int POID, bool isCDAPage);
        Task<YarnReceiveMaster> GetNewSampleYarnAsync();
        Task<YarnReceiveMaster> GetAsync(int id, int POID);
        Task<YarnReceiveMaster> GetAllAsync(int id);
        Task SaveAsync(YarnReceiveMaster entity, int userId);
        Task DeleteAsync(YarnReceiveMaster entity, int userId);
        Task<YarnReceiveChild> GetReceiveChild(int childId);
        Task<List<YarnReceiveChild>> GetReceiveChilds(string childIds);
        Task UpdateChildAsync(YarnReceiveChild entityChild);
        Task<List<YarnQCReqChild>> GetPrevReq(PaginationInfo paginationInfo, string LotNo, int ItemMasterID);
    }
}

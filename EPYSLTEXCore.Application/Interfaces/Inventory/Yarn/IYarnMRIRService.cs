using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYarnMRIRService
    {
        //Task<List<YarnMRIRMaster>> GetPagedAsync(Status status, int offset = 0, int limit = 10, string filterBy = null, string orderBy = null);
        Task<List<YarnMRIRChild>> GetPagedAsync(Status status, PaginationInfo paginationInfo, LoginUser AppUser);
        Task<YarnMRIRMaster> GetMRIRDetailsAsync(int MRIRMasterID);
        Task<YarnMRIRMaster> GetNewAsync(int qcReqMasterId);
        Task<YarnMRIRMaster> GetAsync(int id);
        Task<List<YarnReceiveChild>> GetByYarnReceiveChildByChildIds(string reciveChildIds);
        Task<string> SaveAsync(YarnMRIRMaster entity, ReceiveNoteType receiveNoteType);
    }
}

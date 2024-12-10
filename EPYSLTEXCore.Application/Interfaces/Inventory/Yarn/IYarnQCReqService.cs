using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYarnQCReqService
    {
        Task<List<YarnQCReqMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<YarnQCReqMaster> GetNewAsync();
        Task<YarnQCReqMaster> GetAsync(int id, Status status, int itemMasterID);
        Task<YarnQCReqMaster> GetByRemarksChildId(int id, int qcRemarksChildID);
        Task<YarnQCReqMaster> GetReceiveData(int receiveId);
        Task SaveAsync(YarnQCReqMaster entity, YarnQCRemarksMaster entityQCRemarks);
        Task<YarnQCReqMaster> GetAllAsync(int id);
        Task<YarnQCReqMaster> GetByReceiveChildIds(string receiveChildIds);
        Task<YarnQCReqMaster> GetRetest(int id);
    }
}

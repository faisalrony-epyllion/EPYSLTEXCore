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
    public interface IYarnQCRemarksService
    {
        Task<List<YarnQCRemarksMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<YarnQCRemarksMaster> GetNew2Async(int qcReceiveChildID);
        Task<YarnQCRemarksMaster> GetNewAsync(int qcReqMasterId);
        Task<YarnQCRemarksMaster> Get2Async(int qcRemarksChildID);
        Task<YarnQCRemarksMaster> GetAsync(int id);
        Task SaveAsync(YarnQCRemarksMaster entity);
        Task ApproveAsync(YarnQCRemarksMaster entity);
        Task<YarnQCRemarksMaster> GetAllAsync(int id);
    }
}

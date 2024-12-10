using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYarnQCIssueService
    {
        Task<List<YarnQCIssueMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<YarnQCIssueMaster> GetNewAsync(int qcReqMasterId);
        Task<YarnQCIssueMaster> GetAsync(int id);
        Task SaveAsync(YarnQCIssueMaster entity, List<YarnReceiveChildRackBin> rackBins);
        Task<YarnQCIssueMaster> GetAllAsync(int id);
    }
}

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
    public interface IYarnQCReturnService
    {
        Task<List<YarnQCReturnMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<YarnQCReturnMaster> GetNewAsync(int qcIssuerMasterId);
        Task<YarnQCReturnMaster> GetAsync(int id);
        Task<YarnQCReturnMaster> GetDetailsByQCReturnChilds(string qcReceiveChildIds);
        Task SaveAsync(YarnQCReturnMaster entity);
        Task<YarnQCReturnMaster> GetAllAsync(int id);
    }
}

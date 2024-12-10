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
    public interface IYarnQCReceiveService
    {
        Task<List<YarnQCReceiveMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<YarnQCReceiveMaster> GetNewAsync(int qcIssuerMasterId);
        Task<YarnQCReceiveMaster> GetAsync(int id);
        Task SaveAsync(YarnQCReceiveMaster entity);
        Task<YarnQCReceiveMaster> GetAllAsync(int id);
    }
}

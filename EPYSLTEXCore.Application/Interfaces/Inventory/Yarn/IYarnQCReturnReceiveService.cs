using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYarnQCReturnReceiveService
    {
        Task<List<YarnQCReturnReceivedMaster>> GetPagedAsync(Status status, int offset = 0, int limit = 10, string filterBy = null, string orderBy = null);
        Task<YarnQCReturnReceivedMaster> GetNewAsync(int qcIssuerMasterId);
        Task<YarnQCReturnReceivedMaster> GetAsync(int id);
        Task<YarnQCReturnReceivedMaster> GetAllAsync(int id);
        Task SaveAsync(YarnQCReturnReceivedMaster entity, List<YarnReceiveChildRackBin> rackBins);
    }
}
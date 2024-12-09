using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace EPYSLTEX.Core.Interfaces.Services
{
    public interface IYarnRackBinAllocationService
    {
        Task<List<YarnReceiveMaster>> GetPagedAsync(Status status, bool isCDAPage, PaginationInfo paginationInfo);
        Task<YarnReceiveMaster> GetAsync(int id, int locationId);
        Task<List<YarnReceiveChildRackBin>> GetYarnReceiveChildRackBinData(int childId); // List<YarnReceiveChildRackBin>  
        Task<List<YarnReceiveChildRackBin>> GetRackBin(int childId, int locationId, int qcReturnReceivedChildId);
        Task<List<YarnReceiveChildRackBin>> GetRackBinForKnittingReturnRcv(int childId, int locationId, int kReturnReceivedChildId);
        Task<List<YarnReceiveChildRackBin>> GetRackBinForYDReturnRcv(int childId, int locationId, int kReturnReceivedChildId);
        Task<List<YarnReceiveChildRackBin>> GetRackBinForSCReturnRcv(int childId, int locationId, int kReturnReceivedChildId);
        Task<List<YarnReceiveChildRackBin>> GetRackBinForRNDReturnRcv(int childId, int locationId, int kReturnReceivedChildId);
        Task<List<YarnReceiveChildRackBin>> GetRackBinForKnittingReturnRcvUnusable(int childId, int locationId, int kReturnReceivedChildId);
        Task<List<YarnReceiveChildRackBin>> GetRackBinForYDReturnRcvUnusable(int childId, int locationId, int kReturnReceivedChildId);
        Task<List<YarnReceiveChildRackBin>> GetRackBinForSCReturnRcvUnusable(int childId, int locationId, int kReturnReceivedChildId);
        Task<List<YarnReceiveChildRackBin>> GetRackBinForRNDReturnRcvUnusable(int childId, int locationId, int kReturnReceivedChildId);
        Task<List<YarnReceiveChildRackBin>> GetRackBinById(string childRackBinIDs);
        List<YarnReceiveChildRackBin> GetRackBinWithUpdateValue(List<YarnReceiveChildRackBin> rackBins, int childRackBinID, EPYSLTEXCore.Infrastructure.Static.EnumRackBinOperationType rackBinOT, int noOfCone, int noOfCartoon, decimal qty);
        Task<YarnReceiveMaster> GetAllAsync(int id);
        Task<List<YarnReceiveChildRackBin>> GetAllRacks(PaginationInfo paginationInfo);
        Task SaveAsync(YarnReceiveMaster entity);
    }
}

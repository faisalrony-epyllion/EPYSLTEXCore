using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYDStoreRackBinAllocationService
    {
        Task<List<YDStoreReceiveMaster>> GetPagedAsync(Status status, bool isCDAPage, PaginationInfo paginationInfo);
        Task<YDStoreReceiveMaster> GetAsync(int id, int locationId);
        Task<List<YDStoreReceiveChildRackBin>> GetYarnReceiveChildRackBinData(int childId); // List<YarnReceiveChildRackBin>  
        Task<List<YDStoreReceiveChildRackBin>> GetRackBin(int childId, int locationId, int qcReturnReceivedChildId);
        Task<List<YDStoreReceiveChildRackBin>> GetRackBinForKnittingReturnRcv(int childId, int locationId, int kReturnReceivedChildId);
        Task<List<YDStoreReceiveChildRackBin>> GetRackBinForYDReturnRcv(int childId, int locationId, int kReturnReceivedChildId);
        Task<List<YDStoreReceiveChildRackBin>> GetRackBinForSCReturnRcv(int childId, int locationId, int kReturnReceivedChildId);
        Task<List<YDStoreReceiveChildRackBin>> GetRackBinForRNDReturnRcv(int childId, int locationId, int kReturnReceivedChildId);
        Task<List<YDStoreReceiveChildRackBin>> GetRackBinForKnittingReturnRcvUnusable(int childId, int locationId, int kReturnReceivedChildId);
        Task<List<YDStoreReceiveChildRackBin>> GetRackBinForYDReturnRcvUnusable(int childId, int locationId, int kReturnReceivedChildId);
        Task<List<YDStoreReceiveChildRackBin>> GetRackBinForSCReturnRcvUnusable(int childId, int locationId, int kReturnReceivedChildId);
        Task<List<YDStoreReceiveChildRackBin>> GetRackBinForRNDReturnRcvUnusable(int childId, int locationId, int kReturnReceivedChildId);
        Task<List<YDStoreReceiveChildRackBin>> GetRackBinById(string childRackBinIDs);
        List<YDStoreReceiveChildRackBin> GetRackBinWithUpdateValue(List<YDStoreReceiveChildRackBin> rackBins, int childRackBinID, EnumRackBinOperationType rackBinOT, int noOfCone, int noOfCartoon, decimal qty);
        Task<YDStoreReceiveMaster> GetAllAsync(int id);
        Task<List<YDStoreReceiveChildRackBin>> GetAllRacks(PaginationInfo paginationInfo);
        Task SaveAsync(YDStoreReceiveMaster entity);
    }
}

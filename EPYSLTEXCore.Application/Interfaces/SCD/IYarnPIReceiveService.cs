using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Interfaces.Services
{
    public interface IYarnPIReceiveService
    {
        Task<List<YarnPIReceiveMaster>> GetAsync(Status status, PaginationInfo paginationInfo);
        Task<List<YarnPIReceiveMaster>> GetAsync(Status status, PaginationInfo paginationInfo,string LcNo);
        Task<List<YarnPIReceiveMaster>> GetCDAAsync(Status status, PaginationInfo paginationInfo);
        Task<YarnPIReceiveMaster> GetNewAsync(int yarnPoMasterId, int supplierId, int companyId);
        Task<YarnPIReceiveMaster> GetNewCDAAsync(int yarnPoMasterId, int supplierId, int companyId);
        Task<List<AvailablePOForPI>> GetAvailablePOForPIAsync(int[] poMasterIdArray, int supplierId, int companyId, int yPIReceiveMasterID);
        Task<List<AvailablePOForPI>> GetAvailableCDAPOForPIAsync(int[] poMasterIdArray, int supplierId, int companyId, int yPIReceiveMasterID);
        Task<YarnPIReceiveMaster> GetAsync(int id, int supplierId, int companyId, bool isYarnReceivePage);
        Task<YarnPIReceiveMaster> GetReviceAsync(int id, int supplierId, int companyId, bool isYarnReceivePage, string poIds);
        Task<YarnPIReceiveMaster> GetYarnPIReceiveChildItemsAsync(string ypoMasterIds, int yPIReceiveMasterID);
        Task<YarnPIReceiveMaster> GetCDAPIReceiveChildItemsAsync(string ypoMasterIds, int yPIReceiveMasterID);
        Task<YarnPIReceiveMaster> GetAllByIDAsync(int id);
        Task<List<YarnPIReceivePO>> GetReceivePOByIDAsync(int id);
        Task SaveAsync(YarnPIReceiveMaster entity); 
        Task UpdateEntityAsync(YarnPIReceiveMaster entity);
    }
}

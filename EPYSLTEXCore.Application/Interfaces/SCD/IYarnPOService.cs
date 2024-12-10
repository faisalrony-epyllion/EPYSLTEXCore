using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEX.Core.Interfaces.Services
{
    public interface IYarnPOService 
    {
        Task<List<YarnPOMaster>> GetPagedAsync(Status yarnPOStatus, PaginationInfo paginationInfo);
        Task<YarnPOMaster> GetSupplierInfo(int supplierId);
        Task<YarnPOMaster> GetNewAsync();
        Task<YarnPOMaster> GetNewAsync(string purchaseReqId, string yarnPRChildID, int companyId);
        Task<List<YarnPOMaster>> GetPRItems(int companyId, string prChildIds, PaginationInfo paginationInfo);
        Task<YarnPOMaster> GetAsync(int id);
        Task<YarnPOMaster> GetRevisionAsync(int id);
        Task SaveAsync(YarnPOMaster entity, List<YarnPOChild> yarnPoChilds, int userId);
        Task SaveAsyncRevision(YarnPOMaster entity, List<YarnPOChild> yarnPoChilds, string PONo, int userId);
        Task<List<YarnPOChildOrder>> GetOrderListsFromCompany(PaginationInfo paginationInfo);
        Task<List<YarnPOChildOrder>> GetBuyerListsFromCompany(PaginationInfo paginationInfo);
        Task<List<YarnPOMaster>> GetYarnPOListsShowInPIReceive(int SupplierId, PaginationInfo paginationInfo);
        Task<List<YarnPOChildOrder>> GetExportOrderListsFromIdEdit(int yarnPoMasterId);
        Task<List<YarnPOChild>> GetYarnPOItemsIdEdit(int id);
        Task<List<YarnPOChild>> GetPRChildList(Status status, PaginationInfo paginationInfo, string childIDs, int CompanyId);
        Task<YarnPOMaster> GetAllByIDAsync(int id);
        Task UpdateEntityAsync(YarnPOMaster entity);
    }
}

using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.HouseKeeping;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEX.Core.Interfaces.Services
{
    public interface ILabTestRequisitionService
    {
        Task<List<LabTestRequisitionMaster>> GetPagedAsync(int isBDS, Status status, PaginationInfo paginationInfo);
        Task<LabTestRequisitionMaster> GetNewAsync(int newId, int conceptId, int subGroupId, int buyerId);
        Task<LabTestRequisitionMaster> GetAsync(int id, bool isretestflag, int buyerId);
        Task<List<LabTestRequisitionBuyerParameter>> GetBuyerParameterByBuyerId(int id);
        Task<LabTestRequisitionMaster> GetAllByIDAsync(int id);
        Task<LabTestRequisitionMaster> SaveAsync(LabTestRequisitionMaster entity);
        Task<LabTestRequisitionMaster> ReviseAsync(LabTestRequisitionMaster entity);
        Task UpdateEntityAsync(LabTestRequisitionMaster entity);
        Task<List<LabTestRequisitionBuyerParameter>> GetLabTestRequisitionBuyerParameters(PaginationInfo paginationInfo, int buyerID, int testNatureID, int isProduction);
        Task<List<LaundryCareLable_HK>> GetAllLaundryCareLablesAsync(PaginationInfo paginationInfo);
        Task<List<LaundryCareLableBuyerCode>> GetLaundryCareLableCodes(int buyerId, PaginationInfo paginationInfo);
        Task<List<LaundryCareLableBuyerCode>> GetCareLablesByCode(string careLableCode);
        Task<List<LaundryCareLableBuyerCode>> GetCareLebelsByCodes(string careLableCodes, int buyerID);
    }
}
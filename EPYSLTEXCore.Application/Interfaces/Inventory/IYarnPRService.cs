using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces
{
    public interface IYarnPRService
    {
        Task<List<YarnPRMaster>> GetPagedAsync(Status status, string pageName, PaginationInfo paginationInfo);
        Task<YarnPRMaster> GetNewAsync();
        Task<YarnPRMaster> GetNewForMR(string ids, string source, string revisionstatus = "");
        Task<YarnPRMaster> GetAsync(int id, int prFromID, string source, bool isNewForPRAck);
        Task<List<YarnPRMaster>> GetByPRNo(string prNo);
        Task<YarnPRMaster> GetForReviseAsync(int id, int prFromID, string source, string groupConceptNo);
        Task<List<YarnPRChild>> GetChilds(string conceptNos, string itemIds, string bookingNos);
        Task<List<Select2OptionModel>> GetYarnCompositionsAsync(string fiberType, string yarnType);
        Task SaveAsync(YarnPRMaster entity, int userId);
        Task SaveCPRAsync(YarnPRMaster yarnPRMaster, int userId);
        Task SaveFPRAsync(YarnPRMaster entity, int userId);
        Task<List<YarnPRChild>> GetCommercialCompany(int id);
        Task<YarnPRMaster> GetAllByIDAsync(int id);
        Task UpdateEntityAsync(YarnPRMaster entity);
        Task<YarnPRMaster> GetPRByYarnPRFromTable(int yarnPRFromTableId, int yarnPRFromMasterId);
    }
}
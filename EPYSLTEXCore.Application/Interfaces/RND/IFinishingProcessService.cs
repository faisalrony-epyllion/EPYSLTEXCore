using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces
{
    public interface IFinishingProcessService 
    {
        Task<List<FinishingProcessMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);

        Task<FinishingProcessMaster> GetNewAsync(int conceptId, int isBDS, string grpConceptNo);

        Task<FinishingProcessMaster> GetMachineParam(int fmsId);

        Task<FinishingProcessMaster> GetAsync(int id, int conceptId, int isBDS, string grpConceptNo);

        Task SaveAsync(FinishingProcessMaster entity);

        Task<FinishingProcessMaster> GetAllByIDAsync(int id);

        Task<List<Select2OptionModel>> GetChamicalItem(string particularName, int fpChildId);

        Task<List<FinishingProcessChildItem>> GetFinishingProcessChildItems(string particularName, int fpChildId);
        Task<List<FinishingProcessChild>> GetFinishingMachineProcess(PaginationInfo paginationInfo, string setName);
    }
}
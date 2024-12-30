using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPYSLTEXCore.Infrastructure.Entities.Knitting;

namespace EPYSLTEXCore.Application.Interfaces.RND
{
    public interface IKnittingProgramService
    {
        Task<List<KnittingPlanMaster>> GetPagedAsync(KnittingProgramType type, Status status, PaginationInfo paginationInfo, LoginUser AppUser, bool isNew = false);

        Task<List<KnittingPlanBookingChildDTO>> GetBookingChildsAsync(KnittingProgramType type, PaginationInfo paginationInfo);

        Task<List<KnittingPlanBookingChildDetailsDTO>> GetBookingChildsDetailsAsync(Status status, KnittingProgramType type, PaginationInfo paginationInfo);

        Task<KnittingPlanMaster> GetNewAsync(int conceptID, bool isBulkPage, bool withoutOB, string subGroupName);
        Task<KnittingPlanGroup> GetNewGroupAsync(string conceptIds, bool isBulkPage, bool withoutOB, string subGroupName);
        Task<List<KnittingPlanYarn>> GetYarns(string conceptIds, bool isBulkPage, bool withoutOB, string subGroupName);
        Task<List<KnittingPlanYarn>> GetYarnsForFCMRChild(string conceptIds, bool isBulkPage, bool withoutOB, string subGroupName);
        Task<KnittingPlanMaster> GetNewAsync(KnittingProgramType type, int id, int itemMasterId);
        Task<KnittingPlanMaster> GetAsync(KnittingProgramType type, int id, string subgroupName);
        Task<KnittingPlanGroup> GetGroupAsync(string groupConceptNo, int planNo, KnittingProgramType type, string subgroupName);
        Task<KnittingPlanGroup> GetAdditionGroupAsync(string groupConceptNo, int planNo, KnittingProgramType type, string subgroupName);
        Task<KnittingPlanMaster> GetRevisionedListAsync(int id, int conceptID, string subGroupName);
        Task<KnittingPlanGroup> GetGroupRevisionedListAsync(int groupId, string groupConceptNo, string subGroupName);
        Task<IList<KnittingMachine>> GetMachineByGaugeDia(int MachineGauge, int MachineDia);
        Task<List<KnittingPlanMaster>> GetListByMCSubClass(KnittingProgramType type, int mcSubClassId, PaginationInfo paginationInfo);

        Task<List<KnittingPlanChild>> GetChildsAsync(int masterId, int subGroupID, int conceptID);

        Task<int> GetKnittingPlanCompletionStatusAsync(string yBookingNo);

        Task<KnittingPlanMaster> GetDetailsAsync(int id);
        Task<List<KnittingPlanMaster>> GetDetailsByGroupAsync(int groupId);
        Task<KnittingPlanGroup> GetDetailsAsync(string groupConceptNo, int planNo);
        Task<KnittingPlanChild> GetKnittingPlanDetailsAsync(int id);
        Task<KnittingPlanGroup> GetKnittingPlanGroupAsync(int groupId);
        Task SaveAsync(KnittingPlanMaster entity, KnittingProgramType type, decimal oldPlanQty = 0);
        Task SaveGroupWiseAsync(List<KnittingPlanMaster> entities, KnittingProgramType type, int oldPlanQty = 0);
        Task UpdateFreeConceptMasterAsync(int ConceptID, int MCSubClassID);

        //Task UpdateBDSTNA_BatchPreparationPlanAsync(int BatchID);
        Task SaveGroupAsync(KnittingPlanGroup entity, bool isRevision);
        Task SaveRevisionAsync(KnittingPlanMaster revisionEntity, KnittingPlanMaster newEntity, int userId);

        Task ReviseAsync(KnittingPlanMaster entity, KnittingProgramType type, decimal oldPlanQty = 0);
    }
}
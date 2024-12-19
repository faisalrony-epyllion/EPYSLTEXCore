using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data.Entity;

namespace EPYSLTEXCore.Application.Interfaces.Knitting
{
    public interface IMaterialRequirementBDSService
    {
        Task<List<FreeConceptMRMaster>> GetPagedAsync(Status status, int isBDS, PaginationInfo paginationInfo, LoginUser AppUser);
        Task<FreeConceptMRMaster> GetNewAsync(int FBAckID, string menuParam = null);
        Task<FreeConceptMRMaster> GetAsync(string grpConceptNo, string menuParam = null);
        Task<FreeConceptMRMaster> GetRevisionOfCompleteList(string grpConceptNo, string menuParam = null);
        Task<FreeConceptMRMaster> GetPendingAcknowledgeList(string grpConceptNo);
        Task<FreeConceptMRMaster> GetRevision(int FBAckID, string grpConceptNo);
        Task<List<FreeConceptMRChild>> GetCompleteMRChilds(PaginationInfo paginationInfo, string buyerIds, string buyerTeamIDs);
        Task<FreeConceptMRMaster> GetDetailsAsync(int id);
        Task<List<FreeConceptMRMaster>> GetMultiDetailsAsync(string id, int bookingID);
        Task<List<FreeConceptMRMaster>> GetDetailsAsyncForRevise(string id, int bookingID, bool isOwnRevision);
        Task SaveMultipleAsync(List<FreeConceptMRMaster> entities, EntityState entityState, int userId, List<FreeConceptMaster> freeConceptsUpdate = null, FBookingAcknowledge bookingChildEntity = null);
        Task ReviseAsync(List<FreeConceptMRMaster> entities, string grpConceptNo, int userId, string fcmrChildIds, List<YarnPRMaster> prMasters, List<FreeConceptMaster> freeConceptsUpdate = null, FBookingAcknowledge bookingChildEntity = null);
        Task SaveAsync(FreeConceptMRMaster entity);
        Task AcknowledgeEntityAsync(YarnPRMaster yarnPRMaster, int userId);
        Task<FBookingAcknowledge> GetPYBForBulkAsync(string bookingNo);
        Task<List<FreeConceptMRChild>> GetPYBYarnByBookingNo(string bookingNo);
        Task<List<FreeConceptMaster>> GetAllConceptByBookingNo(string bookingNo);
    }
}

using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.CountEntities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Fabric;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.Booking
{
    public interface IBDSAcknowledgeService
    {
        Task<List<FBookingAcknowledge>> GetPagedAsync(Status status, int isBDS, PaginationInfo paginationInfo, LoginUser AppUser);
        Task<List<FBookingAcknowledge>> GetBulkPagedAsync(Status status, PaginationInfo paginationInfo, LoginUser AppUser, int paramTypeId);
        Task<List<FBookingAcknowledge>> GetBulkFabricAckPagedAsync(Status status, PaginationInfo paginationInfo, LoginUser AppUser);
        Task<List<FBookingAcknowledge>> GetLabDipPagedAsync(Status status, PaginationInfo paginationInfo, LoginUser AppUser);
        Task<CountListItem> GetListCountBBKI(EnumBDSAcknowledgeParamType menuType);
        Task<FBookingAcknowledge> GetNewAsync(int bookingId);
        Task<FBookingAcknowledge> GetNewForReviseAsync(int bookingId);
        Task<FBookingAcknowledge> GetNewAsyncLabdip(int bookingId);
        Task<FBookingAcknowledge> GetNewBulkAsync(int bookingId);
        Task<FBookingAcknowledge> GetNewBulkFAsync(int bookingId);
        Task<FBookingAcknowledge> GetNewBulkFabricAsync(int bookingId);
        Task<List<FBookingAcknowledgeChild>> GetDataForAcknowledgColourAsync(int bookingId);
        Task<FBookingAcknowledge> GetDataAsync(int fbAckId);
        Task<FBookingAcknowledge> GetDataLabdipAsync(int bookingId, bool isRnD);
        Task<FBookingAcknowledge> GetDataLabdipAcknowledgedDataAsync(int bookingId, bool isRnD);
        Task<FBookingAcknowledge> GetDataLabdipRevisionAsync(int bookingId);
        Task<FBookingAcknowledge> GetDataByBookingNo(string bookingNo, bool isSample, bool isAddition, string yBookingNo, bool isSavedAddition, bool isAllowYBookingNo, bool isYarnRevisionMenu, bool isFromYBAck);
        Task<FBookingAcknowledge> GetDataByYBookingNo(string bookingNo, bool isSample, bool isAddition, string yBookingNo, bool isSavedAddition, bool isAllowYBookingNo, bool isYarnRevisionMenu, bool isFromYBAck);
        Task<FBookingAcknowledge> GetDataByBookingNoRevise(string bookingNo, bool isSample);
        Task<FBookingAcknowledge> GetFBAcknowledge(int fbAckId);
        Task<FBookingAcknowledge> GetFBAcknowledgeWithChilds(int fbAckId);
        Task<FBookingAcknowledge> GetFBAcknowledgeBulk(string bookingNo, bool isAddition);
        Task<FBookingAcknowledge> GetFBAcknowledgeBulkAddition(string bookingNo, bool isAddition, string yBookingNo = null, bool isUpdateAddition = false);
        Task<FBookingAcknowledge> GetFBAcknowledgeBulkWithRevision(string bookingNo, bool isAddition);
        Task<FBookingAcknowledge> GetYBForBulkAsync(string bookingNo, bool isAddition = false);
        Task<List<FBookingAcknowledge>> GetFBAcknowledgeMasterBulk(string bookingNo, bool isAddition = false);
        Task<List<FBookingAcknowledge>> GetFBAcknowledgeMasterBulkWithRevision(string bookingNo, bool isAddition = false);
        Task<List<FBookingAcknowledge>> GetFBAcknowledgeMasterBulkWithChild(string bookingNo);
        Task<List<FBookingAcknowledge>> GetFBAcknowledgeMasterBulkWithChildWithRevision(string bookingNo);
        Task<List<YarnBookingMaster>> GetYarnBookingsBulk(string bookingNo);
        Task<List<YarnBookingMaster>> GetYarnBookingsBulkWithRevision(string bookingNo);
        Task<FBookingAcknowledge> GetFBAcknowledgeByBookingID(int BookingID);
        Task<SampleBookingMaster> GetAllAsync(int id);
        Task<FreeConceptMaster> GetAllAsyncR(int id);
        Task<List<FBookingAcknowledgeChild>> GetRefSourceItem(PaginationInfo paginationInfo, int bookingID, int consumptionID);
        Task<List<FBookingAcknowledgeChildColor>> GetAllAsyncColorIDs(string colorIDs);
        Task SaveAsync(FBookingAcknowledge entity, List<FBookingAcknowledgeChild> entityChilds, List<FBookingAcknowledgeChildAddProcess> entityChildAddProcess, List<FBookingAcknowledgeChildDetails> entityChildDetails, List<FBookingAcknowledgeChildGarmentPart> entityChildsGpart, List<FBookingAcknowledgeChildProcess> entityChildsProcess, List<FBookingAcknowledgeChildText> entityChildsText, List<FBookingAcknowledgeChildDistribution> entityChildsDistribution, List<FBookingAcknowledgeChildYarnSubBrand> entityChildsYarnSubBrand, List<FBookingAcknowledgeImage> entityChildsImage, List<BDSDependentTNACalander> BDCalander, int isBDS, List<FreeConceptMaster> entityFreeConcepts = null, List<FreeConceptMRMaster> entityFreeMRs = null, List<FBookingAcknowledgementLiabilityDistribution> entityFBookingAcknowledgementLiabilityDistributions = null, List<FabricBookingAcknowledge> entityFBA = null, List<FBookingAcknowledgementYarnLiability> entityFBYL = null, List<YarnBookingMaster> entityYarnBookings = null,
            List<FreeConceptMaster> entityFreeConceptsForRevise = null, List<SampleBookingConsumption> sampleBookingChilds = null, bool isRevised = false, int UserId = 0);
        Task<List<FreeConceptMaster>> GetFreeConcepts(string bookingNo);
        Task UpdateEntityAsync(SampleBookingMaster entity);
        Task UpdateEntityAsyncR(FreeConceptMaster entity);
        Task<BDSDependentTNACalander> GetAllAsyncBDSTNAEvent_HK();
        Task<List<BDSDependentTNACalander>> GetPagedAsyncTNA(PaginationInfo paginationInfo);
        Task<List<BDSDependentTNACalander>> GetPagedAsyncEventlist(PaginationInfo paginationInfo);
        Task<List<BDSDependentTNACalander>> GetBoookingList(PaginationInfo paginationInfo);
        Task<List<BDSDependentTNACalander>> GetbookingWiseList(PaginationInfo paginationInfo, DateTime FromDate, DateTime TotDate);
        Task<List<BDSDependentTNACalander>> GetbookingWiseTNAList(PaginationInfo paginationInfo, String ListData);
        Task<List<BDSDependentTNACalander>> GetEventWiseTNA(PaginationInfo paginationInfo, String EventListData);
        Task<List<BDSDependentTNACalander>> GetEventWiseList(PaginationInfo paginationInfo, int eventID);
        Task<String> GetSampleTypeByBookingID(int bookingID); //SaveAsyncBulk(entity)
        Task<SampleBookingMaster> GetSampleBooking(int id);
        Task<SampleBookingMaster> UpdateSampleBookingAsync(SampleBookingMaster entity);
        Task<FBookingAcknowledge> UpdateFBookingAck(FBookingAcknowledge entity);
        Task<string> SaveAsyncBulk(int userId, FBookingAcknowledge entity, List<FBookingAcknowledge> entities, bool isAddition, List<FreeConceptMRChild> mcChilds);
        Task<string> SaveAsyncBulkAddition(int userId, FBookingAcknowledge entity, List<FBookingAcknowledge> entities, bool isAddition, List<FreeConceptMRChild> mcChilds, bool isUpdateAddition = false, List<YarnBookingMaster_New_RevisionReason> RevisionReasonList = null, bool isRevisedYarn = false);
        Task<string> SaveAsyncBulkWithRevision(int userId, FBookingAcknowledge entity, List<FBookingAcknowledge> entities, bool isAddition, List<FreeConceptMRChild> mcChilds);
        Task<string> SaveAsyncBulkWithFreeConcept(int userId, FBookingAcknowledge entity, List<FBookingAcknowledge> entities, bool isAddition, int isBDS, List<FreeConceptMaster> entityFreeConcepts, List<FreeConceptMRMaster> entityFreeMRs, List<FreeConceptMaster> entityFreeConceptsForRevise, bool isRevised, List<YarnBookingMaster> entitiesYB, bool isYarnRevised, List<YarnBookingMaster_New_RevisionReason> RevisionReasonList = null);
        Task<string> SaveAsyncBulkWithFreeConceptWithYarnRevision(int userId, FBookingAcknowledge entity, List<FBookingAcknowledge> entities, bool isAddition, int isBDS, List<FreeConceptMaster> entityFreeConcepts, List<FreeConceptMRMaster> entityFreeMRs, List<FreeConceptMaster> entityFreeConceptsForRevise, bool isRevised, List<YarnBookingMaster> entitiesYB, bool isYarnRevised, List<YarnBookingMaster_New_RevisionReason> RevisionReasonList = null);
        Task UpdateBulkStatus(List<FBookingAcknowledge> entities, List<YarnBookingChildItem> yarnBookingChildItems, List<YarnBookingMaster> yarnBookings, List<YarnBookingChild> yarnBookingChilds, List<FreeConceptMRChild> mrChilds, bool isYarnRevised, bool pmcApprove = false, bool IsRejectByPMC = false, int UserId = 0);
        Task UpdateBulkStatus2(int userId, List<FBookingAcknowledge> entities, List<YarnBookingChildItem> yarnBookingChildItems, List<YarnBookingMaster> yarnBookings, List<YarnBookingChild> yarnBookingChilds, List<FreeConceptMRChild> mrChilds);
        Task UpdateBulkStatusYarnRevision(int UserId, List<FBookingAcknowledge> entities, List<YarnBookingChildItemRevision> yarnBookingChildItemsRevision, List<YarnBookingMaster> yarnBookings, List<YarnBookingChild> yarnBookingChilds, List<FreeConceptMRChild> mrChilds, bool pmcApprove = false, bool IsRejectByPMC = false);
        Task SaveRevision(List<FBookingAcknowledge> entities, List<YarnBookingChildItemRevision> yarnBookingChildItemsRevision, List<YarnBookingMaster> yarnBookings, List<YarnBookingChild> yarnBookingChilds, List<FreeConceptMRChild> mrChilds, List<YarnBookingMaster_New_RevisionReason> RevisionReasonList = null);
        Task ApproveOrRejectBulkAddition(YarnBookingMaster yarnBooking, List<YarnBookingMaster> yarnBookings);

        Task<List<BulkBookingFinishFabricUtilization>> GetFinishFabricUtilizationByGSMAndCompositionAsync(string GSMId, string GSM, string CompositionId, string ConstructionId, string SubGroupID, PaginationInfo paginationInfo);
        Task<List<BulkBookingFinishFabricUtilization>> GetFinishFabricUtilizationByYBChildID(int YBChildID);

        Task<List<FBookingAcknowledgeChildGFUtilization>> GetGreyFabricUtilizationItem(string GSMId, string GSM, string CompositionId, string ConstructionId, string SubGroupID, PaginationInfo paginationInfo);
        Task<List<BulkBookingDyedYarnUtilization>> GetDyedYarnUtilizationItem(string GSMId, string CompositionId, string ConstructionId, string SubGroupID, PaginationInfo paginationInfo);
        Task<List<BulkBookingGreyYarnUtilization>> GetGreyYarnUtilizationItem(string ItemMasterId, PaginationInfo paginationInfo);
        Task<List<FBookingAcknowledge>> GetItsSampleOrNot(string BookingNo);
        Task<List<YarnBookingMaster_New_RevisionReason>> GetYarnRevisionReason(PaginationInfo paginationInfo);
    }
}

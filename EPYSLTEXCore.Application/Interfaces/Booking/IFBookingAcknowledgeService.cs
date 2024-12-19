using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.CountEntities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Fabric;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.Data.SqlClient;

namespace EPYSLTEXCore.Application.Interfaces.Booking
{
    public interface IFBookingAcknowledgeService
    {
        Task<List<FBookingAcknowledge>> GetPagedAsync(Status status, int isBDS, PaginationInfo paginationInfo, LoginUser AppUser);
        Task<List<FBookingAcknowledge>> GetBulkPagedAsync(Status status, PaginationInfo paginationInfo, LoginUser AppUser);
        Task<List<FBookingAcknowledge>> GetBulkFabricAckPagedAsync(Status status, PaginationInfo paginationInfo, LoginUser AppUser);
        Task<CountListItem> GetListCount();
        Task<FBookingAcknowledge> GetNewAsync(int bookingId);
        Task<FBookingAcknowledge> GetNewBulkAsync(int bookingId);
        Task<FBookingAcknowledge> GetNewBulkFAsync(string bookingNo);
        Task<FBookingAcknowledge> GetNewBulkFabricAsync(int bookingId);
        Task<List<FBookingAcknowledgeChild>> GetDataForAcknowledgColourAsync(int bookingId);
        Task<FBookingAcknowledge> GetDataAsync(int fbAckId);
        Task<FBookingAcknowledge> GetDataByBookingNo(string bookingNo);
        Task<FBookingAcknowledge> GetFBAcknowledge(int fbAckId);
        Task<FBookingAcknowledge> GetFBAcknowledgeByBookingID(int BookingID);
        Task<SampleBookingMaster> GetAllAsync(int id);
        Task<List<SampleBookingMaster>> GetAllSampleBookingByIDAsync(string id);
        Task<BookingMaster> GetAllBookingAsync(int id);
        Task<List<BookingMaster>> GetAllBookingMasterByIDAsync(string id);
        Task<FreeConceptMaster> GetAllAsyncR(int id);
        Task<List<FBookingAcknowledgeChildColor>> GetAllAsyncColorIDs(string colorIDs);
        Task SaveAsync(int userId, FBookingAcknowledge entity, List<FBookingAcknowledgeChild> entityChilds, List<FBookingAcknowledgeChildAddProcess> entityChildAddProcess, List<FBookingAcknowledgeChildDetails> entityChildDetails, List<FBookingAcknowledgeChildGarmentPart> entityChildsGpart, List<FBookingAcknowledgeChildProcess> entityChildsProcess, List<FBookingAcknowledgeChildText> entityChildsText, List<FBookingAcknowledgeChildDistribution> entityChildsDistribution, List<FBookingAcknowledgeChildYarnSubBrand> entityChildsYarnSubBrand, List<FBookingAcknowledgeImage> entityChildsImage, List<BDSDependentTNACalander> BDCalander, int isBDS, List<FreeConceptMaster> entityFreeConcepts = null, List<FreeConceptMRMaster> entityFreeMRs = null, List<FBookingAcknowledgementLiabilityDistribution> entityFBookingAcknowledgementLiabilityDistributions = null, List<FabricBookingAcknowledge> entityFBA = null, List<FBookingAcknowledgementYarnLiability> entityFBYL = null);
        Task SaveAsync(int userId, List<BookingItemAcknowledge> entityBIA, List<FabricBookingAcknowledge> entityFBA, List<FBookingAcknowledge> entityFBookingA, List<BookingMaster> entityBM, String WithoutOB = "0", bool isRevise = false, string SaveType = null);
        Task SaveAsync(int userId, List<BookingItemAcknowledge> entityBIA, List<FabricBookingAcknowledge> entityFBA, List<FBookingAcknowledge> entityFBookingA, List<SampleBookingMaster> entityBM, bool isRevise = false, string SaveType = null);
        Task SaveAsync(int userId, List<FabricBookingAcknowledge> entityFBA);
        Task SaveAsync(int userId, String EditType, String BookingNo, List<FabricBookingAcknowledge> entityFB, List<FBookingAcknowledge> entityFBA, List<FBookingAcknowledgeChild> entityFBC, List<FBookingAcknowledgementLiabilityDistribution> entityLD, List<FBookingAcknowledgementYarnLiability> entityYLD, bool isRevised, string WithoutOB, int styleMasterId, int bomMasterId, int userCode, string SaveType = null);
        //Task SaveAsync(List<FBookingAcknowledge> entityFBA);
        Task SaveAsync(int userId, List<FBookingAcknowledge> entityFBA);
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
        Task<String> GetSampleTypeByBookingID(int bookingID);

        Task<FBookingAcknowledge> GetAllRevisionStatusByExportOrderIDAndSubGroupID(String ExportOrderNo, String SubGroupID);
        void SaveFabricBookingItemAcknowledgeBackup(String BookingNo, bool WithoutOB, ref SqlTransaction transaction);
        Task<List<BookingChild>> GetAllInHouseBookingByBookingNo(String BookingNo);
        Task<List<BookingMaster>> GetBookingMasterByNo(string bookingno);
        Task<List<SampleBookingMaster>> GetBookingMasterByNoSample(string bookingno);
        Task<List<BookingChild>> GetAllInHouseSampleBookingByBookingNo(String BookingNo);
        Task<List<FabricBookingAcknowledge>> GetAllFabricBookingAcknowledgeByBookingNoAndGroupName(String BookingNo);
        Task<List<FabricBookingAcknowledge>> GetAllSampleFabricBookingAcknowledgeByBookingNoAndGroupName(String BookingNo, String SubGroupName);
        Task<List<BookingItemAcknowledge>> GetAllBookingItemAcknowledgeByBookingNo(String BookingNo);
        Task<List<BookingItemAcknowledge>> GetAllBookingItemAcknowledgeByBookingIDAndWithOutOB(String BookingID);
        void RollBackFabricBookingData(String BookingNo, bool WithoutOB, ref SqlTransaction transaction);
        Task<List<FabricBookingAcknowledge>> GetAllBuyerTeamHeadByBOMMasterID(String BOMMasterID);
        Task<List<FabricBookingAcknowledge>> GetAllEmployeeMailSetupByUserCodeAndSetupForName(String UserCode, String SetupForName);
        Task<FBookingAcknowledge> GetSavedBulkFabricAsync(String BookingID);
        Task<FBookingAcknowledge> GetSavedBulkFabricRevisionAsync(String BookingID);
        Task<FabricBookingAcknowledge> GetAllSavedFBAcknowledgeByBookingID(String BookingID, bool isRevised = false);
        Task<List<FBookingAcknowledgementLiabilityDistribution>> GetAllFBookingAckLiabilityByIDAsync(string id);
        Task<List<FBookingAcknowledgementYarnLiability>> GetAllFBookingAckYarnLiabilityByIDAsync(String BookingID);
        Task<List<FBookingAcknowledge>> GetRevMktAckAndRevisionAck(string BookingNo, string ExportOrderID);
        Task UnAckFabricBooking(String Sql);
        Task<List<FBookingAcknowledge>> GetBookingByBookingNo(string bookingNo);
        Task<int> CheckIsBookingApprovedAsync(string bookingNo);
        Task<List<FabricWastageGrid>> GetFabricWastageGridAsync(string wastageFor);
    }
}

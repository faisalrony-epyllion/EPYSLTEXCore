using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data.Entity;

namespace EPYSLTEXCore.Application.Interfaces.Booking
{
    public interface IYarnBookingService
    {
        Task<List<YarnBookingMaster>> GetPagedAsync(Status status, string PageName, PaginationInfo paginationInfo);
        //Task<YarnBookingMaster> GetNewAsync(bool isSample, string bookingNo, string yBookingNo,
        //    string reasonStatus, int yBookingID, Status status);
        Task<YarnBookingMaster> GetListWithoutGroupBy(bool isSample, string bookingNo, Status status);
        Task<YarnBookingMaster> GetPendingYarnList(bool isSample, string bookingNo, Status status);
        Task<YarnBookingMaster> GetSaveYarnList(bool isSample, string bookingNo, string yBookingNo,
            string reasonStatus, int yBookingID, Status status);
        Task<YarnBookingMaster> GetNewAsyncByGroup(bool isSample, string bookingNo, string yBookingNo,
            string reasonStatus, int yBookingID, Status status);
        Task<List<YarnBookingMaster>> GetByYBookingNo(string yBookingNo);
        Task<List<YarnBookingMaster>> GetAllByYBookingNo(string yBookingNo);
        Task<List<YarnBookingMaster>> GetByYBookingNos(List<string> yBookingNos);
        Task<List<YarnBookingMaster>> GetByBookingNo(string bookingNo, bool isAddition = false);
        Task<YarnBookingMaster> GetAsync(string yBookingNo, bool isSample, string reasonStatus, int yBookingID);
        Task<YarnBookingMaster> GetFBARevisionPending(string bookingNo, string exportOrderNo);
        Task<YarnBookingMaster> GetAllByIDAsync(int id);
        Task<List<YarnBookingReason>> GetReason(int yBookingID, string reasonStatus);
        Task<List<YarnBookingChild>> GetYarnChilds(string bookingNo, int subGroupId, int consumptionID, int itemMasterId, string construction);
        Task<List<FBookingAckChildFinishingProcess>> GetFinishingProcesses(string bookingNo, int subGroupId, int consumptionID, int itemMasterId, string construction);
        Task<string> GetYarnRequiredDate(string ExportOrderNo, string CDays);
        Task<List<YarnBookingMaster>> GetAllByNoAsync(string yBookingNo);
        //Task SaveMultipleAsync(List<YarnBookingMaster> entities, EntityState entityState, List<FreeConceptMaster> freeConceptList = null, List<FreeConceptMRMaster> freeConceptMRList = null);
        Task SaveMultipleAsync(List<YarnBookingMaster> entities, EntityState entityState, bool isRevise);
        Task AdditionalSaveMultipleAsync(List<YarnBookingMaster> entities, EntityState entityState);
        Task ReviseSaveMultipleAsync(List<YarnBookingMaster> entities, EntityState entityState);
        Task UpdateEntityAsync(YarnBookingMaster entity);
        //Email
        Task<YarnBookingMaster> GetBookingInformation(string YBookingNo, bool WithoutOB);
        Task<YarnBookingMaster> GetYBForBulkAsync(string bookingNo, bool isSample);
        Task<YarnBookingMaster> GetAsyncforAutoPR(string yBookingNo);
        Task AcknowledgeEntityAsync(YarnPRMaster yarnPRMaster, int userId);
        Task<List<YarnBookingMaster>> GetYarnByNo(string yBookingNo);
        Task UpdateYB(int userId, List<YarnBookingMaster> yarnBookings, bool isRevised = false, List<FBookingAcknowledge> FBAList = null, bool isAdditionalAcknowledge = false);
        Task<List<YarnBookingChildItem>> GetYanBookingChildItems(string sYBChildItemIDs);
        Task<List<YarnBookingChildItem>> GetYanBookingChildItemsWithRevision(string sYBChildItemIDs);
        Task<List<YarnBookingChildItem>> GetYanBookingChildItemsByBookingNo(string bookingNo);
        Task<List<YarnBookingChildItem>> GetYanBookingChildItemsByBookingNoWithRevision(string bookingNo, bool isAddition = false);
        Task<List<YarnBookingChildItem>> GetYanBookingChildItemsByYBookingNoWithRevision(string bookingNo, bool isAddition = false);
        //Task AcknowledgeforAutoPR(int YBookingID);
        Task<string> GetExportDataAsync(string bookingNo, bool isSample);
        Task<List<YarnBookingMaster>> GetAllYarnBookingMasterByBookingNo(string bookingNo);
        Task UpdateDataExportInfo(List<YarnBookingMaster> entities);
        Task<List<FBookingAcknowledge>> GetFBookingAcknowledgeByBookingNo(string BookingNo);

    }
}

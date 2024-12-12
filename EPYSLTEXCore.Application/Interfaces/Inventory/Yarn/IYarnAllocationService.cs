using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYarnAllocationService
    {
        Task<List<YarnAllocationMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<List<YarnAllocationChildItem>> GetAckPagedAsync(Status status, PaginationInfo paginationInfo);

        Task<List<YarnAllocationMaster>> GetPagedAsync2(Status status,
            string buyerIds,
            string buyerTeamIds,
            string countIds,
            string yBookingIds,
            string yItemMasterIds,
            string fabricShadeIds,
            string fabricTypeIds,
            string yarnTypeIds,
            string yarnRequisitionTypes,
            string fabricGSMIds,

            bool yBookingDateAsPerFR_Chk,
            string yBookingDateAsPerFR_From,
            string yBookingDateAsPerFR_To,

            bool actualYarnBookingDate_Chk,
            string actualYarnBookingDate_From,
            string actualYarnBookingDate_To,

            bool yarnInhouseStartDateAsPerFR_Chk,
            string yarnInhouseStartDateAsPerFR_From,
            string yarnInhouseStartDateAsPerFR_To,

            bool fabricDeliveryStartDate_Chk,
            string fabricDeliveryStartDate_From,
            string fabricDeliveryStartDate_To,

            bool fabricDeliveryEndDate_Chk,
            string fabricDeliveryEndDate_From,
            string fabricDeliveryEndDate_To,

            bool tNACalendarDays_Chk,
            string tNACalendarDays_From,
            string tNACalendarDays_To,
            PaginationInfo paginationInfo);

        Task<List<YarnAllocationChildItem>> GetFilterListPending(Status status,
                int searchFieldNameId,
                string selectedBuyerIds,
                string selectedBuyerTeamIds,
                string selectedCountIds,
                string selectedYBookingIds,
                string selectedYItemMasterIds,
                string selectedFabricShadeIds,
                string selectedFabricTypeIds,
                string selectedYarnTypeIds,
                string selectedYarnRequisitionTypes,
                string selectedFabricGSMIds,
                PaginationInfo paginationInfo);

        Task<List<YarnAllocationMaster>> GetPendingBookingAsync(string buyerIds, PaginationInfo paginationInfo);
        Task<YarnAllocationMaster> GetNewAsync(string ybChildItemID);
        Task<List<YarnAllocationMaster>> GetYarnBookingWiseChildAsync(string BookingNos);
        Task<YarnAllocationMaster> GetAsync(int id);
        Task<YarnAllocationMaster> GetAsync2(int allocationId);
        Task<YarnAllocationMaster> GetAsyncRevised(int allocationId);
        Task<YarnAllocationMaster> GetChildItemByStockSetId(int allocationChildId, int yarnStockSetId);
        Task<YarnAllocationChildItem> GetItemAsync(int id);
        Task<YarnAllocationChild> GetChildAsync(int id);
        Task<YarnAllocationMaster> GetAckAsync(int id);
        Task<List<YarnAllocationChildItem>> GetStockAsync(int itemMasterId, int allocationChildID, string operationType, string yarnCount, PaginationInfo paginationInfo);
        //Task<List<YarnAllocationChildItem>> GetStockAsync(int itemMasterId, int allocationChildID, string operationType, string yarnCount);
        Task<List<YarnAllocationChildPipelineItem>> GetPipelineStockAsync(int itemMasterId, int allocationChildID, string operationType, string yarnCount, PaginationInfo paginationInfo);
        Task<List<YarnAllocationChildItem>> GetAllAllocationByYBChildItemId(int ybChildItemID, int allocationChildItemID, PaginationInfo paginationInfo);
        Task<List<YarnAllocationChildItem>> GetAllocationsByBookingNo(string bookingNo, PaginationInfo paginationInfo);
        //Task<List<YarnAllocationChildPipelineItem>> GetPipelineStockAsync(int itemMasterId, int allocationChildID, string operationType, string yarnCount);
        Task<List<YarnAllocationChildItem>> GetAllocatedStockAsync(int id);
        Task<List<YarnAllocationChildItem>> GetAllocatedStockByYBChildItemIDAsync(int yBChildItemID);
        Task<List<YarnAllocationChildPipelineItem>> GetAllocatedPipelineStockAsync(int id);
        Task<List<YarnAllocationChildPipelineItem>> GetAllocatedPipelineStockByYBChildItemIDAsync(int yBChildItemID);
        Task<List<YarnAllocationMaster>> GetByAllocationChildIds(string allocationChildIds);
        Task<string> GetMaxYarnAllocationNoAsync();
        //Task<YarnAllocationMaster> GetAllAsync(int id);

        Task SaveAsync(YarnAllocationMaster entity);
        Task SaveAllocation(YarnAllocationChild entityChild, bool isReAllocation = false, bool isUnAckRevise = false);
        Task UpdateItemAsync(YarnAllocationChildItem entity, List<FBookingAcknowledge> fbaList = null, List<YarnBookingMaster> ybmList = null, bool isAllocationInternalRevise = false);
        Task<List<FBookingAcknowledge>> GetFBookingAcknowledgeByBookingNo(string bookingNo);
        Task<List<YarnBookingMaster>> GetYarnBookingMasterByBookingNo(string ybookingNo);
        Task<List<YarnPRMaster>> GetPRMasterByBookingNoAsync(string bookingNo);
    }
}

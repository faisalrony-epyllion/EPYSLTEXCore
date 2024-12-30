using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.Interfaces.Inventory.Yarn
{
    public interface IYarnStockAdjustmentService
    {
        Task<List<YarnStockAdjustmentMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<YarnStockAdjustmentMaster> GetNewAsync();
        Task<YarnStockAdjustmentMaster> GetAsync(bool isPipelineRecord, int itemMasterId, int supplierId, string shadeCode, int yarnStockSetId);
        Task SaveAsync(YarnStockAdjustmentMaster entity, int UserId);
        Task ItemSaveAsync(List<YarnReceiveChild> items, int UserId);
        Task<List<YarnReceiveChildRackBin>> GetRackBin(int yarnStockSetId, int itemMasterId, int supplierId, int spinnerId, string yarnLotNo, string shadeCode, string physicalCount, string childRackIds);
        Task<YarnStockAdjustmentMaster> GetAsync(int id);
        Task<YarnStockAdjustmentMaster> GetAsync2(int id);
        Task<YarnStockAdjustmentMaster> GetRelatedList();

        #region Common Methods for Stocks
        Task<List<YarnStockAdjustmentMaster>> GetAllStocks(string yarnCategory, string physicalCount, string yarnLotNo, string shadeCode, string supplier, string spinner, string count, string otherQuery, bool isValidItem, PaginationInfo paginationInfo);
        Task<List<YarnStockAdjustmentMaster>> GetAllStocks(string otherQuery);
        Task<List<YarnStockAdjustmentMaster>> GetItemWithStockSetId(List<YarnStockAdjustmentMaster> stocks);
        #endregion

        #region Others
        Task<List<Select2OptionModel>> GetStockTypes();
        #endregion
    }
}
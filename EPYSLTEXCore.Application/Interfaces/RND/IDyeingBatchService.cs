using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.Interfaces.RND
{
    public interface IDyeingBatchService
    {
        Task<List<DyeingBatchMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<DyeingBatchMaster> GetNewAsync(int newId);
        Task<List<DyeingBatchMaster>> GetNewMultiSelectAsync(string batchIDs);
        Task<DyeingBatchMaster> GetAsync(int id);
        Task<List<DyeingBatchMaster>> GetBatchListAsync(string batchIds);
        Task<List<DyeingBatchMaster>> GetBatchDetails(string batchIds);
        Task<List<DyeingBatchChildFinishingProcess>> GetFinishingProcessAsync(int conceptID, int colorID);
        Task<List<DyeingBatchChildFinishingProcess>> GetFinishingProcessByDyeingBatchAsync(int dBatchID, int colorID);
        Task SaveAsync(DyeingBatchMaster entity);
        Task SaveAsyncRecipeCopy(DyeingBatchMaster entity);
        Task<DyeingBatchMaster> GetAllByIDAsync(int id);
        Task UpdateEntityAsync(DyeingBatchMaster entity);
        Task<List<DyeingBatchMaster>> GetDyeingBatchs(PaginationInfo paginationInfo, string colorName, string conceptNo);
    }
}
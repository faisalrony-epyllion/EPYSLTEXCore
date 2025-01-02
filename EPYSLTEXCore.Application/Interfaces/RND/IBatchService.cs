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
    public interface IBatchService
    {
        Task<List<BatchMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);

        Task<BatchMaster> GetNewAsync(int newId, string conceptNo, int bookingId, int isBDS, int colorID);

        Task<BatchMaster> GetAsync(int id, string conceptNo, int bookingId);

        Task<BatchMaster> GetAllAsync(int id);

        Task<BatchItemRequirement> GetBatchItemRequirementAsync(int id);
        Task<List<BatchItemRequirement>> GetOtherItems(PaginationInfo paginationInfo, string conceptIds, int colorId, string groupConceptNo);
        Task SaveAsync(BatchMaster entity);
        Task SaveAsyncRecipeCopy(BatchMaster entity);
        Task UpdateBDSTNA_BatchPreparationPlanAsync(int BatchID);
    }
}
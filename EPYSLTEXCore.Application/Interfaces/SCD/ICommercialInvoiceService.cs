using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.Interfaces.SCD
{
    public interface ICommercialInvoiceService
    {
        Task<List<YarnCIMaster>> GetPagedAsync(Status status, bool isCDAPage, PaginationInfo paginationInfo);

        Task<YarnCIMaster> GetNewAsync(int qcReqMasterId);

        Task<YarnCIMaster> GetAsync(int id);
        Task<List<YarnCIMaster>> GetAllCIInfoByIDAsync(string id, string BankRefNumber);
        Task<YarnCIMaster> GetAllAsync(int id);
        Task SaveAsync(YarnCIMaster entity);
        Task UpdateEntityAsync(YarnCIMaster entity);
        Task UpdateMultiEntityAsync(List<YarnCIMaster> entityList);
        Task<List<YarnCIMaster>> GetItemDetails(string itemIds);
        Task<List<YarnCIMaster>> createBankAcceptance(string nCIIDs, string companyIDs, string supplierIDs, string bankBranchIDs);

    }
}

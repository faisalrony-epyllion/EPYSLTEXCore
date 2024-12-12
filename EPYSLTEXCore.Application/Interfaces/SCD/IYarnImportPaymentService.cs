using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data.Entity;

namespace EPYSLTEXCore.Interfaces.Services
{
    public interface IYarnImportPaymentService
    {
        Task<List<ImportInvoicePaymentMaster>> GetPagedAsync(Status status, bool isCDAPage, PaginationInfo paginationInfo);
        Task<ImportInvoicePaymentMaster> GetNewAsync(string BankRefNumber, int CompanyId, int SupplierId, bool isCDAPage);
        Task SaveAsync(ImportInvoicePaymentMaster entities, EntityState entityState);
        Task<ImportInvoicePaymentMaster> GetMultiDetailsAsync(string IIPMasterID, bool isCDAPage);
        Task<ImportInvoicePaymentMaster> GetEditAsync(int IIPMasterID, bool isCDAPage);
    }
}

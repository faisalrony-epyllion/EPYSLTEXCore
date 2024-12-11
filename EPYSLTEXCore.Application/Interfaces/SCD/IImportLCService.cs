using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces.SCD
{
    public interface IImportLCService
    {
        Task<List<YarnLcMaster>> GetImportLCData(Status status, bool isCDAPage, PaginationInfo paginationInfo);

        Task<YarnLcMaster> GetNewAsync(int newID);

        Task<YarnLcMaster> GetAsync(int id);

        Task<YarnLcMaster> GetAllByIDAsync(int id);

        Task SaveAsync(YarnLcMaster entity);

        //Task<List<DCCodeSetupChild>> GetAllDCCodeSetupChildByCompanyIDAndBankBranchIDAndDCType(String CompanyID, String BankBranchID, String PaymentModeID, String setupForName);

        //Task<List<DCCodeSetupMaster>> GetAllDCCodeSetupIDByCompanyIDAndBankBranchIDAndDCType(String CompanyID, String BankBranchID, String PaymentModeID, String setupForName);
    }
}

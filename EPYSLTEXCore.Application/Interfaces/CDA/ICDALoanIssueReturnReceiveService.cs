using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.CDA;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Services
{
    public interface ICDALoanIssueReturnReceiveService
	{
		#region CDALoanIssueReturnReceiveMaster
		Task<List<CDALoanIssueReturnReceiveMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);

		Task<CDALoanIssueReturnReceiveMaster> GetNewAsync();

		Task<CDALoanIssueReturnReceiveMaster> GetAsync(int Id);

		Task<CDALoanIssueReturnReceiveMaster> GetAllAsync(int id);

		Task SaveAsync (CDALoanIssueReturnReceiveMaster entity);
		Task UpdateEntityAsync(CDALoanIssueReturnReceiveMaster entity);

		#endregion CDALoanIssueReturnReceiveMaster
	}
}

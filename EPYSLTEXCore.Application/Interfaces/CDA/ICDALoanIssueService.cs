using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.CDA;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Services
{
    public interface ICDALoanIssueService
	{
		#region CDALoanIssueService
		Task<List<CDALoanIssueMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo, string Flag);

		Task<CDALoanIssueMaster> GetNewAsync();

		Task<CDALoanIssueMaster> GetAsync(int Id);

		Task<CDALoanIssueMaster> GetAllAsync(int id);

		Task SaveAsync (CDALoanIssueMaster entity);
		Task UpdateEntityAsync(CDALoanIssueMaster entity);

		#endregion CDALoanIssueMaster
	}
}

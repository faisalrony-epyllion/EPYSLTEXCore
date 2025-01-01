using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.CDA;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Services
{
    public interface ICDALoanReturnService
	{
		#region CDALoanReturnMaster
		Task<List<CDALoanReturnMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo, string Flag);

		Task<CDALoanReturnMaster> GetNewAsync();

		Task<CDALoanReturnMaster> GetAsync(int Id);

		Task<CDALoanReturnMaster> GetAllAsync(int id);

		Task SaveAsync (CDALoanReturnMaster entity);
		Task UpdateEntityAsync(CDALoanReturnMaster entity);

		#endregion CDALoanReturnMaster
	}
}

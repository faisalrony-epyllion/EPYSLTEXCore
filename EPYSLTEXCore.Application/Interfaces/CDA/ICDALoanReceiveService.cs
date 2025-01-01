using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.CDA;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEX.Core.Interfaces.Services
{
    public interface ICDALoanReceiveService
	{
		#region CDALoanReceive
		Task<List<CDALoanReceiveMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo); 
		Task<CDALoanReceiveMaster> GetNewAsync();
		Task<CDALoanReceiveMaster> GetAsync(int Id);
		Task<CDALoanReceiveMaster> GetAllAsync(int id);
		Task SaveAsync (CDALoanReceiveMaster entity);
		Task UpdateEntityAsync(CDALoanReceiveMaster entity);
		#endregion CDALoanReceive
	}
}

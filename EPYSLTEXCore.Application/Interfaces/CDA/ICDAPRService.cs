using EPYSLTEXCore.Infrastructure.Entities.CDA;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.Interfaces.CDA
{
    public interface ICDAPRService
    {
        Task<CDAPRMaster> GetDyesChemicalsAsync();
        Task<List<CDAPRMaster>> GetPagedAsync(Status status, string pageName, PaginationInfo paginationInfo);
        Task<List<CDAIndentChild>> GetPendingIndentPagedAsync(Status status, string pageName, PaginationInfo paginationInfo);
        Task<CDAPRMaster> GetNewAsync(string SubGroupName);
        Task<CDAPRMaster> GetIndentNewAsync(string SubGroupName, string IDs);
        Task<CDAPRMaster> GetAsync(int id, string SubGroupName);
        Task<CDAPRMaster> GetAllAsync(int id);
        Task SaveAsync(CDAPRMaster entity);
        Task SaveCPRAsync(CDAPRMaster entity);
        Task SaveFPRAsync(CDAPRMaster entity);
        Task UpdateEntityAsync(CDAPRMaster entity);
    }
}

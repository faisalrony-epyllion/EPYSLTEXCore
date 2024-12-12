using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Admin;
using EPYSLTEXCore.Infrastructure.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.Interfaces.Admin
{
    public interface IBankLimitService
    {
        Task<List<BankLimitMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<BankLimitMaster> GetNewAsync();
        Task<BankLimitMaster> GetDetails(int id);
        Task<BankLimitMaster> GetById(int id);
        Task SaveAsync(BankLimitMaster entity);

        //Task SaveAsync(BankLimitMaster entity, int userId);
    }
}

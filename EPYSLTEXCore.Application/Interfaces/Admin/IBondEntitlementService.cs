using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Admin;
using EPYSLTEXCore.Infrastructure.Entities.Tex;

namespace EPYSLTEXCore.Application.Interfaces.Admin
{
    public interface IBondEntitlementService
    {
        Task<List<BondEntitlementMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo);
        Task<BondEntitlementMaster> GetNewAsync();
        Task<BondEntitlementMaster> GetDetails(int id);
        Task<BondEntitlementMaster> GetById(int id);
        Task SaveAsync(BondEntitlementMaster entity);

        //Task SaveAsync(BondEntitlementMaster entity, int userId);
    }
}

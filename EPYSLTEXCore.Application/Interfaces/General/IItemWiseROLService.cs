using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Interfaces
{
    public interface IItemWiseROLService
    {
        Task<List<ItemMasterReOrderStatus>> GetPagedAsync(PaginationInfo paginationInfo);
        Task<ItemMasterReOrderStatus> GetAsync(int rosid);
        Task SaveAsync(ItemMasterReOrderStatus entitie);
        Task<ItemMasterReOrderStatus> GetMaster();
        Task<List<ItemMasterReOrderStatus>> GetItemMasterDataAsync(PaginationInfo paginationInfo);
    }
}
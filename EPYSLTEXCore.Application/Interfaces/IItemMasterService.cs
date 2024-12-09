using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.General;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item;

namespace EPYSLTEXCore.Application.Interfaces
{
    public interface IItemMasterService<T> where T : BaseItemMaster
    {

        Task<IEnumerable<ItemSegmentValue>> AddSegmentsAsync(IEnumerable<ItemSegmentValue> entities, string tableName);
        void GenerateItem(int subGroupId, ref List<T> itemList);
        IEnumerable<T> GetDataAsync<T>(string query, int db_type);
    }
}

using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.General;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item;

namespace EPYSLTEXCore.Application.Interfaces
{
    public interface IChildItemMasterService<T> where T : BaseChildItemMaster
    {

        void GenerateItemWithYItem(int subGroupId, ref List<ItemMasterBomTemp> itemMasterList, ref List<T> itemList);
        List<ItemMasterBomTemp> GetItemMasterList(int subGroupId);
    }
}

using EPYSLTEXCore.Infrastructure.Entities;

namespace EPYSLTEXCore.Infrastructure.Entities.General
{
    public class ItemMasterUploadBindingModel : BaseItemMaster
    {
    }

    public class ItemMasterListUploadBindingModel
    {
        public int SubGroupID { get; set; }
        public bool CreateNewSegmentValueThatNotFound { get; set; }
        public List<ItemMasterUploadBindingModel> Items { get; set; }

        public ItemMasterListUploadBindingModel()
        {
            Items = new List<ItemMasterUploadBindingModel>();
            CreateNewSegmentValueThatNotFound = true;
        }
    }
}

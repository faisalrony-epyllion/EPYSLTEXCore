namespace EPYSLTEXCore.Infrastructure.Entities.General
{
    public class ItemSegmentValueBindingModel : BaseModel
    {
        public string SegmentValue { get; set; }
        public int SegmentNameId { get; set; }
        public bool IsUsed { get; set; }
    }
}

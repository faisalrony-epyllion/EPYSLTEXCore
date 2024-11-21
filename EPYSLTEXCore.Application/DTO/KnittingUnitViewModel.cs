namespace EPYSLTEX.Web.Models
{
    public class KnittingUnitViewModel : BaseModel
    {
        public string ContactId { get; set; }
        public string UnitName { get; set; }
        public string ShortName { get; set; }
        public bool IsKnitting { get; set; }
    }
}
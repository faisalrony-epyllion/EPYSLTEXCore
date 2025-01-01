using System.Data.Entity;

namespace EPYSLTEX.Core.Entities.Tex
{
    public class CDALCDocument : IBaseEntity
    {
        public int Id { get; set; } 
        public int Lcid { get; set; }
        public int DocId { get; set; }
        public EntityState EntityState { get; set; }
        public virtual CDALCMaster LcMaster { get; set; }  
    }
}

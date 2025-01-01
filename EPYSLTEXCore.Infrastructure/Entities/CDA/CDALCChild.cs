using System.Data.Entity;

namespace EPYSLTEX.Core.Entities.Tex
{
    public class CDALCChild : BaseEntity 
    {
        public int LCID { get; set; }
        public int YPIReceiveMasterID { get; set; }
        public virtual CDALCMaster LcMaster { get; set; } 
    }
}

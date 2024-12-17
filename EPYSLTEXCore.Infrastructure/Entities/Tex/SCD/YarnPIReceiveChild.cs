using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YarnPIReceiveChild)]
    public class YarnPIReceiveChild : YarnItemMaster, IDapperBaseEntity
    {
        public YarnPIReceiveChild()
        {
            PIQty = 0m;
            YarnCategory = "";
            EntityState = EntityState.Added;

            YPOMasterID = 0;
            YPOChildID = 0;

            ShadeCode = "";
        } 
        [ExplicitKey]
        public int YPIReceiveChildID { get; set; }
        public int YPIReceiveMasterID { get; set; }
        public string YarnCategory { get; set; }
        public decimal POQty { get; set; }
        public decimal Rate { get; set; }
        public decimal PIQty { get; set; }
        public decimal PIValue { get; set; }
        public string Remarks { get; set; }
        public string YarnLotNo { get; set; }
        public string HSCode { get; set; }
        public int YarnProgramID { get; set; }
        public string ShadeCode { get; set; }

        public int YPOChildID { get; set; }

        #region Additional
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        [Write(false)]
        public int YpiChildId { get; set; }

        [Write(false)]
        public decimal POValue { get { return POQty * Rate; } }

        [Write(false)]
        public string YarnShade { get; set; }

        [Write(false)]
        public string YarnType { get; set; }

        [Write(false)]
        public string YarnCount { get; set; }

        [Write(false)]
        public string YarnComposition { get; set; }

        [Write(false)]
        public string YarnColor { get; set; }

        [Write(false)]
        public string DisplayUnitDesc { get; set; }

        [Write(false)]
        public string YarnProgram { get; set; }
        [Write(false)]
        public decimal POReceivedQty { get; set; }
        [Write(false)]
        public int YPOMasterID { get; set; }

        [Write(false)]
        public int RevisionNo { get; set; }


        #endregion
    }
}


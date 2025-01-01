using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.HardWindingChild)]
    public class HardWindingChild : YarnItemMaster, IDapperBaseEntity
    {
        #region Table Properties
        [ExplicitKey]
        public int HardWindingChildID { get; set; }
        public int HardWindingMasterID { get; set; }
        public int YDBookingChildID { get; set; } = 0;
        public int YDBItemReqID { get; set; } = 0;
        //public int ItemMasterID { get; set; } = 0;// Inherited FROM YarnItemMaster
        public int ColorID { get; set; } = 0;
        public decimal Qty { get; set; } = 0;
        public int Cone { get; set; } = 0;
        public string Remarks { get; set; } = "";
        public string YarnCategory { get; set; } = "";
        public int YDDryerFinishingChildID { get; set; } = 0;
        public int YDRICRBId { get; set; } = 0;
        #endregion

        #region Additional Properties
        [Write(false)]
        public string ShadeCode { get; set; } = "";
        [Write(false)]
        public string NoOfThread { get; set; } = "";
        [Write(false)]
        public bool IsAdditionalItem { get; set; } = false;
        [Write(false)]
        public string ColorCode { get; set; }
        [Write(false)]
        public string ColorName { get; set; }
        [Write(false)]
        public int BookingFor { get; set; }
        [Write(false)]
        public string BookingForName { get; set; }
        [Write(false)]
        public bool IsTwisting { get; set; }
        [Write(false)]
        public bool IsWaxing { get; set; }
        [Write(false)]
        public int RequiredQty { get; set; }
        [Write(false)]
        public int BookingQty { get; set; }
        [Write(false)]
        public decimal ReceiveQty { get; set; }
        [Write(false)]
        public int ReceiveCone { get; set; }
        [Write(false)]
        public int ReceiveCarton { get; set; }
        [Write(false)]
        public int BacthQty { get; set; }
        //[Write(false)]
        //public int UnitID { get; set; }// Inherited FROM YarnItemMaster
        [Write(false)]
        public string DisplayUnitDesc { get; set; }
        [Write(false)]
        public int BatchCone { get; set; }
        [Write(false)]
        public int DryerFinishQty { get; set; }
        [Write(false)]
        public int DryerFinishCone { get; set; }
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        #endregion

        public HardWindingChild()
        {
            EntityState = EntityState.Added;
        }
    }
}

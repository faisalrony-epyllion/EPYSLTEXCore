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
    [Table(TableNames.YARN_YD_REQ_CHILD)]
    public class YarnYDReqChild : YarnItemMaster, IDapperBaseEntity
    {
        public YarnYDReqChild()
        {
            PhysicalCount = "";
            EntityState = EntityState.Added;
        }

        #region Table Properties
        [ExplicitKey]
        public int YDReqChildID { get; set; }
        public int YDBookingChildID { get; set; }
        public int YDReqMasterID { get; set; }
        public int YarnProgramID { get; set; }
        public string PhysicalCount { get; set; }
        public string Remarks { get; set; }
        public int NoOfThread { get; set; }
        public string DisplayUnitDesc { get; set; }
        public decimal RequiredQty { get; set; }
        public decimal RequsitionQty { get; set; }
        public int NoOfCone { get; set; }
        public int ColorID { get; set; }
        public string ColorCode { get; set; }
        public int BookingFor { get; set; }
        public bool IsTwisting { get; set; }
        public bool IsWaxing { get; set; }
        public string ShadeCode { get; set; }
        public bool IsAdditionalItem { get; set; }
        #endregion

        #region Additional Properties
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        #endregion
    }
}

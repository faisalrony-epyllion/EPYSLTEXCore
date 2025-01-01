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
    [Table(TableNames.YD_REQ_CHILD)]
    public class YDReqChild : YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int YDReqChildID { get; set; }

        public int YDReqMasterID { get; set; } = 0;
        public int YarnProgramID { get; set; } = 0;
        public decimal ReqQty { get; set; } = 0;
        public int ReqCone { get; set; } = 0;
        public decimal BookingQty { get; set; }
        public string Remarks { get; set; }
        public string YarnCategory { get; set; }
        public int NoOfThread { get; set; } = 0;
        public string ShadeCode { get; set; }
        public int SpinnerID { get; set; } = 0;
        public string LotNo { get; set; }
        public string PhysicalCount { get; set; }
        public int YDBookingChildID { get; set; } = 0;
        public int AllocationChildItemID { get; set; } = 0;
        public int StockTypeId { get; set; } = 0;

        #region Additional Fields

        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public string DisplayUnitDesc { get; set; }
        [Write(false)]
        public string SpinnerName { get; set; }
        [Write(false)]
        public decimal AllocatedQty { get; set; }
        [Write(false)]
        public decimal PendingQty { get; set; }
        [Write(false)]
        public int YBChildItemID { get; set; } = 0;
        [Write(false)]
        public decimal NetYarnReqQty { get; set; } = 0;
        [Write(false)]
        public decimal AdvanceStockQty { get; set; } = 0;
        [Write(false)]
        public decimal SampleStockQty { get; set; } = 0;
        [Write(false)]
        public decimal StockQty { get; set; } = 0;
        [Write(false)]
        public bool YDItem { get; set; } = false;
        [Write(false)]
        public string StockTypeName { get; set; } = "";
        [Write(false)]
        public int YarnStockSetId { get; set; } = 0;
        //
        #endregion Additional Fields

        public YDReqChild()
        {
            YarnProgramID = 0;
            ReqQty = 0;
            ReqCone = 0;
            NoOfThread = 0;
            EntityState = EntityState.Added;
        }
    }
}

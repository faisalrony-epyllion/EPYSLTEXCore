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
    [Table(TableNames.YARN_RnD_REQ_CHILD)]
    public class YarnRnDReqChild : YarnItemMaster, IDapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int RnDReqChildID { get; set; } = 0; 
        public int RnDReqMasterID { get; set; }= 0;
        public int ConceptID { get; set; } = 0;
        public int FCMRMasterID { get; set; } = 0;
        public int FCMRChildID { get; set; } = 0;
        public string YarnLotNo { get; set; } = "";
        public string YarnCategory { get; set; } = "";
        public int YarnBrandID { get; set; } = 0;
        public int KPYarnID { get; set; } = 0;
        public decimal ReqCone { get; set; } = 0;
        public string PhysicalCount { get; set; } = "";
        public decimal ReqQty { get; set; } = 0;
        public string Remarks { get; set; } = "";
        public string ShadeCode { get; set; } = "";
        public string BatchNo { get; set; } = "";
        public int PreProcessRevNo { get; set; } = 0;
        public int RevisionNo { get; set; } = 0;
        public int BookingID { get; set; } = 0;
        public bool InActive { get; set; }=false;
        public int StockTypeId { get; set; } = 0;

        #endregion Table Properties

        #region Additinal Properties

        [Write(false)]
        public string ConceptNo { get; set; }

        [Write(false)]
        public string DisplayUnitDesc { get; set; }
        [Write(false)]
        public string YarnBrand { get; set; }
        [Write(false)]
        public bool YD { get; set; }
        [Write(false)]
        public bool YDItem { get; set; }
        [Write(false)]
        public int FloorID { get; set; }
        [Write(false)]
        public string FloorName { get; set; }
        [Write(false)]
        public string GroupConceptNo { get; set; }
        [Write(false)]
        public EntityState EntityState { get; set; }
        [Write(false)]
        public DateTime ConceptDate { get; set; }
        [Write(false)]
        public int IsBDS { get; set; }
        [Write(false)]
        public decimal YarnReqQty { get; set; }
        [Write(false)]
        public decimal UsedQty { get; set; }
        [Write(false)]
        public decimal PendingQty { get; set; }
        [Write(false)]
        public int MaxReqQty { get; set; }
        [Write(false)]
        public int YarnStockSetId { get; set; } = 0;
        [Write(false)]
        public decimal StockQty { get; set; } = 0;
        [Write(false)]
        public decimal SampleStockQty { get; set; } = 0;
        [Write(false)]
        public decimal AdvanceStockQty { get; set; } = 0;
        [Write(false)]
        public string Spinner { get; set; } = "";
        [Write(false)]
        public string StockTypeName { get; set; } = "";
        [Write(false)]
        public int TotalRows { get; set; }
        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        [Write(false)]
        public bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.RnDReqChildID > 0;

        #endregion MyRegion

        //public YarnRnDReqChild()
        //{
        //    Remarks = "";
        //    EntityState = EntityState.Added;
        //    YDItem = false;
        //    KPYarnID = 0;
        //    GroupConceptNo = "";
        //    IsBDS = 0;
        //    PreProcessRevNo = 0;
        //    RevisionNo = 0;
        //    BookingID = 0;
        //    InActive = false;
        //    UsedQty = 0;
        //    PendingQty = 0;
        //}
    }
}

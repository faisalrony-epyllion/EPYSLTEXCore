using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting
{
    [Table(TableNames.Knitting_Plan_Yarn)]
    public class KnittingPlanYarn : DapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int KPYarnID { get; set; } = 0;
        public int KPMasterID { get; set; } = 0;
        public int YarnCountID { get; set; } = 0;
        public int FCMRChildID { get; set; } = 0;
        public int ItemMasterID { get; set; } = 0;
        public string PhysicalCount { get; set; } = "";
        public int YarnTypeID { get; set; } = 0;
        public string YarnLotNo { get; set; } = "";
        public string BatchNo { get; set; } = "";
        public int GroupID { get; set; } = 0;
        public bool YDItem { get; set; } = false;
        public int YarnBrandID { get; set; } = 0;
        public int YarnPly { get; set; } = 0;
        public decimal StitchLength { get; set; } = 0;
        public int YarnStockSetId { get; set; } = 0;
        #endregion Table Properties

        #region Addtional Properties
        [Write(false)]
        public string YarnType { get; set; } = "";
        [Write(false)]
        public string YarnCount { get; set; } = "";
        [Write(false)]
        public string ItemName { get; set; } = "";
        [Write(false)]
        public string LotNo { get; set; } = "";
        [Write(false)]
        public string YarnBrand { get; set; } = "";
        [Write(false)]
        public bool YD { get; set; } = false;
        [Write(false)]
        public int ConceptID { get; set; } = 0;
        [Write(false)]
        public string Composition { get; set; } = "";
        [Write(false)]
        public string YarnCategory { get; set; } = "";
        [Write(false)]
        public decimal TotalQty { get; set; } = 0;
        [Write(false)]
        public decimal ReqQty { get; set; } = 0;
        [Write(false)]
        public int FCMRMasterID { get; set; } = 0;
        [Write(false)]
        public bool IsStockItemFromMR { get; set; } = false;
        [Write(false)]
        public string ItemMasterIDs { get; set; } = "";
        [Write(false)]
        public string YDColorName { get; set; } = "";
        [Write(false)]
        public string YDBookingForName { get; set; } = "";
        [Write(false)]
        public bool IsStockItem => this.YarnStockSetId > 0 ? true : false;
        [Write(false)]
        public YarnStockAdjustmentMaster YarnStockSet { get; set; } = new YarnStockAdjustmentMaster();
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || KPYarnID > 0;
        [Write(false)]
        public List<KnittingPlanYarnChild> Childs { get; set; } = new List<KnittingPlanYarnChild>();
        #endregion Addtional Properties
    }
}

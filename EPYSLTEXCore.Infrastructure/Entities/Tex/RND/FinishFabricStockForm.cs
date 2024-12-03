using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.FinishFabricStockForm)]
    public class FinishFabricStockForm : DapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int FFSFromID { get; set; }
        public int BatchID { get; set; }
        public int ConceptID { get; set; }
        public int BookingID { get; set; }
        public int ItemMasterID { get; set; }
        public int ColorID { get; set; }
        public int FormID { get; set; }
        public int QtyInPcs { get; set; }
        public decimal QtyInKG { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }

        #endregion Table Properties

        #region Additional Properties
        [Write(false)]
        public List<FinishFabricStockFormRoll> KJChilds { get; set; }
        [Write(false)]
        public bool isModified { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.FFSFromID > 0;

        [Write(false)]
        public List<FinishFabricStockFormRoll> Childs { get; set; }

        [Write(false)]
        public int GRollID { get; set; }
        [Write(false)]
        public int DBIRollID { get; set; }
        [Write(false)]
        public int DBatchID { get; set; }
        [Write(false)]
        public decimal RollQty { get; set; }
        [Write(false)]
        public int RollQtyPcs { get; set; }
        [Write(false)]
        public int RollNo { get; set; }
        [Write(false)]
        public string SubGroupName { get; set; }
        [Write(false)]
        public string KnittingType { get; set; }
        [Write(false)]
        public string TechnicalName { get; set; }
        [Write(false)]
        public string Composition { get; set; }
        [Write(false)]
        public string ColorName { get; set; }
        [Write(false)]
        public string Gsm { get; set; }
        [Write(false)]
        public string GSM { get; set; }
        [Write(false)]
        public string ConceptNo { get; set; }
        [Write(false)]
        public DateTime ConceptDate { get; set; }
        [Write(false)]
        public string BatchNo { get; set; }
        [Write(false)]
        public decimal Qty { get; set; }
        [Write(false)]
        public int QtyPcs { get; set; }
        [Write(false)]
        public List<Select2OptionModel> FormList { get; set; }
        [Write(false)]
        public List<LiveProductForm> LiveProductForms { get; set; }
        [Write(false)]
        public List<DyeingBatchItemRoll> DyeingBatchItemRolls { get; set; }
        [Write(false)]
        public List<FinishFabricStockForm> FinishFabricStockForms { get; set; }
        [Write(false)]
        public List<KnittingProduction> knittingProductions { get; set; }
        #endregion Additional Fields
        public FinishFabricStockForm()
        {
            EntityState = System.Data.Entity.EntityState.Added;
            ConceptID = 0;
            Qty = 0;
            QtyPcs = 0;
            Gsm = "";
            ConceptNo = "";
            ColorName = "";
            DateAdded = DateTime.Now;
            UpdatedBy = 0;
            DateUpdated = null;
            isModified = false;
            QtyInPcs = 0;
            QtyInKG = 0;
            Childs = new List<FinishFabricStockFormRoll>();
            LiveProductForms = new List<LiveProductForm>();
            FormList = new List<Select2OptionModel>();
            knittingProductions = new List<KnittingProduction>();
            DyeingBatchItemRolls = new List<DyeingBatchItemRoll>();

            FinishFabricStockForms = new List<FinishFabricStockForm>();
        }
    }
}

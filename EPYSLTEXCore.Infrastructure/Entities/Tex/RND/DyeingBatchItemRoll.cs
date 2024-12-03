using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Production;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table("T_DyeingBatchItemRoll")]
    public class DyeingBatchItemRoll : DapperBaseEntity
    {
        [ExplicitKey]
        public int DBIRollID { get; set; }

        public int DBIID { get; set; }

        public int DBatchID { get; set; }

        public int ItemMasterID { get; set; }

        public int GRollID { get; set; }

        public decimal RollQty { get; set; }

        public decimal RollQtyPcs { get; set; }

        public bool OnlineQCComplete { get; set; }

        public DateTime? OnlineQCDate { get; set; }

        public int OnlineQCBy { get; set; }

        public bool OnlineQCCheck { get; set; }

        public bool OnlineQCPass { get; set; }

        public bool OnlineQCFail { get; set; }

        public bool OnlineQCHold { get; set; }

        public string OnlineQCHoldRemarks { get; set; }

        public bool OnlineQCCommPass { get; set; }

        public string OnlineQCCommRemarks { get; set; }

        public bool SendforQC { get; set; }

        public DateTime? SendQCDate { get; set; }

        public int SendQCBy { get; set; }

        public bool QCComplete { get; set; }

        public DateTime? QCCompleteDate { get; set; }

        public int QCCompleteBy { get; set; }

        public int QCShiftID { get; set; }

        public decimal QCWidth1 { get; set; }

        public decimal QCWidth2 { get; set; }

        public decimal QCWidth3 { get; set; }

        public decimal QCWidth { get; set; }

        public int QCGSM1 { get; set; }

        public int QCGSM2 { get; set; }

        public int QCGSM3 { get; set; }

        public int QCGSM { get; set; }

        public decimal FinishRollQty { get; set; }

        public int FinishRollQtyPcs { get; set; }

        public bool QCPass { get; set; }

        public bool QCFail { get; set; }

        public bool QCHold { get; set; }

        public string QCHoldRemarks { get; set; }

        public bool QCCommPass { get; set; }

        public string QCCommRemarks { get; set; }

        public int Holes { get; set; }

        public int OilMark { get; set; }

        public int DyeStain { get; set; }

        public int RubMark { get; set; }

        public int DirtySpot { get; set; }

        public int ChemicalSpot { get; set; }

        public int BandLine { get; set; }

        public int NeedleBroken { get; set; }

        public int ContaFly { get; set; }

        public int Slub { get; set; }

        public bool Creases { get; set; }

        public bool HairyDC { get; set; }

        public bool TT { get; set; }

        public bool Neps { get; set; }

        public bool BarreMark { get; set; }

        public string QCOthers { get; set; }

        public int BookingID { get; set; }

        public int ExportOrderID { get; set; }

        public int BuyerID { get; set; }

        public int BuyerTeamID { get; set; }

        public int ParentFRollID { get; set; }

        public bool InActive { get; set; }

        public int InActiveBy { get; set; }

        public DateTime? InActiveDate { get; set; }

        public string InActiveReason { get; set; }

        public int OilMark1 { get; set; }

        public int OilMark2 { get; set; }

        public int OilMark3 { get; set; }

        public int DyeStain1 { get; set; }

        public int DyeStain2 { get; set; }

        public int DyeStain3 { get; set; }

        public int RubMark1 { get; set; }

        public int RubMark2 { get; set; }

        public int RubMark3 { get; set; }

        public int DirtySpot1 { get; set; }

        public int DirtySpot2 { get; set; }

        public int DirtySpot3 { get; set; }

        public int ChemicalSpot1 { get; set; }

        public int ChemicalSpot2 { get; set; }

        public int ChemicalSpot3 { get; set; }

        public int BandLine1 { get; set; }

        public int BandLine2 { get; set; }

        public int BandLine3 { get; set; }

        public int NeedleBroken1 { get; set; }

        public int NeedleBroken2 { get; set; }

        public int NeedleBroken3 { get; set; }

        public decimal TotalPoint { get; set; }

        public decimal TotalValue { get; set; }

        public decimal RollLength { get; set; }

        public string Grade { get; set; }

        #region Additional Property

        [Write(false)]
        public string RollNo { get; set; }

        [Write(false)]
        public int BItemReqID { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || DBIRollID > 0;

        [Write(false)]
        public List<DyeingBatchItemRoll> Childs { get; set; }

        [Write(false)]
        public string BookingNo { get; set; }

        [Write(false)]
        public string ColorName { get; set; }

        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public int BatchID { get; set; }

        [Write(false)]
        public string BuyerDepartment { get; set; }

        [Write(false)]
        public string DBatchNo { get; set; }

        [Write(false)]
        public DateTime DBatchDate { get; set; }

        [Write(false)]
        public int ColorID { get; set; }

        [Write(false)]
        public int CCColorID { get; set; }

        [Write(false)]
        public string Shift { get; set; }
        [Write(false)]
        public decimal BatchWeightKG { get; set; }

        [Write(false)]
        public decimal BatchQtyPcs { get; set; }

        [Write(false)]
        public DateTime ProductionDate { get; set; }
        [Write(false)]
        public string ActiveByName { get; set; }
        [Write(false)]
        public string ConceptNo { get; set; }

        [Write(false)]
        public decimal ProductionWidth { get; set; }

        [Write(false)]
        public decimal ProdQty { get; set; }

        [Write(false)]
        public int ProductionGSM { get; set; }

        [Write(false)]
        public int ConceptID { get; set; }
        [Write(false)]
        public int SFDChildID { get; set; }
        [Write(false)]
        public int SFDChildRollID { get; set; }
        [Write(false)]
        public int ParentGRollID { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> QCShiftList { get; set; }

        [Write(false)]
        public List<GreyQCDefect_HK> FinishFabricQCDefect_HKs { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> OnlineQCByList { get; set; }
        [Write(false)]
        public int FFSFRollID { get; set; }
        [Write(false)]
        public decimal ActualRollQty { get; set; }
        [Write(false)]
        public int ActualRollQtyPcs { get; set; }
        [Write(false)]
        public decimal RemainingRollQty => this.ActualRollQty - this.FinishRollQty;
        [Write(false)]
        public int RemainingRollQtyPcs => this.ActualRollQtyPcs - this.FinishRollQtyPcs;
        #endregion Additional Property

        public DyeingBatchItemRoll()
        {
            Childs = new List<DyeingBatchItemRoll>();
            ActualRollQty = 0;
            ActualRollQtyPcs = 0;
        }
    }
}

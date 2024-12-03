using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting;
using FluentValidation;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.RND_KNITTING_PRODUCTION)]
    public class KnittingProduction : DapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int GRollID { get; set; }
        public int KJobCardMasterID { get; set; }
        public int KJobCardChildID { get; set; }
        public DateTime ProductionDate { get; set; }
        public int ConceptID { get; set; }
        public int BookingID { get; set; }
        public int ExportOrderID { get; set; }
        public int BuyerID { get; set; }
        public int BuyerTeamID { get; set; }
        public int OperatorID { get; set; }
        public int ShiftID { get; set; }
        public int RollSeqNo { get; set; }
        public string RollNo { get; set; }
        public decimal RollQty { get; set; }
        public int RollQtyPcs { get; set; }
        public int ProductionGSM { get; set; }
        public decimal ProductionWidth { get; set; }
        public decimal ActualGreyHeight { get; set; }
        public decimal ActualGreyLength { get; set; }
        public bool FirstRollCheck { get; set; }
        public int FirstRollCheckBy { get; set; }
        public DateTime? FirstRollCheckDate { get; set; }
        public bool FirstRollPass { get; set; }
        public bool SendforQC { get; set; }
        public DateTime? SendQCDate { get; set; }
        public int SendQCBy { get; set; }
        public decimal RollLength { get; set; }
        public bool QCComplete { get; set; }
        public DateTime? QCCompleteDate { get; set; }
        public int QCCompleteBy { get; set; }
        public decimal QCWidth { get; set; }
        public int QCGSM { get; set; }
        public bool QCPass { get; set; }
        public bool QCFail { get; set; }
        public decimal QCPassQty { get; set; }
        public decimal QCPassQtyPcs { get; set; }
        public int ParentGRollID { get; set; }
        public bool InActive { get; set; }
        public int InActiveBy { get; set; }
        public DateTime? InActiveDate { get; set; }
        public string InActiveReason { get; set; }
        public bool ProdComplete { get; set; }
        public decimal ProdQty { get; set; }
        public bool RollFinishing { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }

        #region For Gray Fabric QC

        public int Hole { get; set; }
        public int Loop { get; set; }
        public int SetOff { get; set; }
        public int LycraOut { get; set; }
        public int LycraDrop { get; set; }
        public int LycraDrop1 { get; set; }
        public int LycraDrop2 { get; set; }
        public int LycraDrop3 { get; set; }
        public int OilSpot { get; set; }
        public int OilSpot1 { get; set; }
        public int OilSpot2 { get; set; }
        public int OilSpot3 { get; set; }
        public int Slub { get; set; }
        public int FlyingDust { get; set; }
        public int MissingYarn { get; set; }
        public int Knot { get; set; }
        public int DropStitch { get; set; }
        public int DropStitch1 { get; set; }
        public int DropStitch2 { get; set; }
        public int DropStitch3 { get; set; }
        public int YarnContra { get; set; }
        public int NeddleBreakage { get; set; }
        public int Defected { get; set; }
        public bool WrongDesign { get; set; }
        public bool Patta { get; set; }
        public bool ShinkerMark { get; set; }
        public bool NeddleMark { get; set; }
        public bool EdgeMark { get; set; }
        public bool WheelFree { get; set; }
        public bool CountMix { get; set; }
        public bool ThickAndThin { get; set; }
        public bool LineStar { get; set; }
        public string QCOthers { get; set; }
        public string Comment { get; set; }
        public decimal CalculateValue { get; set; }
        public bool CalculateQCStatus { get; set; }
        public string Grade { get; set; }
        public bool Hold { get; set; }
        public int QCBy { get; set; }
        public int QCShiftID { get; set; }
        public int? BatchID { get; set; }
        public bool ITM { get; set; }

        #endregion For Gray Fabric QC

        public decimal RollFinishingWidth { get; set; }
        public decimal RollFinishingGSM { get; set; }
        public int RollFinishingQCStatus { get; set; }
        public string RollFinishingComments { get; set; }

        /// <summary>
        /// If Knitting Production Active or not.
        /// </summary>
        public bool Active { get; set; }

        #endregion Table Properties

        #region Additional Properties

        /// <summary>
        /// There is no Child Table in DB. This is for view purpose only.
        /// </summary>
        [Write(false)]
        public List<KnittingProductionChildBindingModel> Childs { get; set; }

        [Write(false)]
        public List<KnittingProductionQC> KnittingProductionQCs { get; set; }

        [Write(false)]
        public List<FreeConceptChildColor> ChildColors { get; set; }
        [Write(false)]
        public List<KnittingProduction> RollChilds { get; set; }

        [Write(false)]
        public int IsBDS { get; set; }

        [Write(false)]
        public string GroupConceptNo { get; set; }

        [Write(false)]
        public string KJobCardNo { get; set; }

        [Write(false)]
        public DateTime JobCardDate { get; set; }

        [Write(false)]
        public string JobCardStatus { get; set; }
        [Write(false)]
        public bool JobCardProdComplete { get; set; }
        [Write(false)]
        public bool IsSubContact { get; set; }
        [Write(false)]
        public int EWO { get; set; }

        [Write(false)]
        public decimal BookingQty { get; set; }

        [Write(false)]
        public decimal BookingQtyPcs { get; set; }

        [Write(false)]
        public decimal ProducedQty { get; set; }

        [Write(false)]
        public decimal JobCardQty { get; set; }
        [Write(false)]
        public string BatchNo { get; set; }
        [Write(false)]
        public int SubGroupID { get; set; }
        [Write(false)]
        public string Buyer { get; set; }
        [Write(false)]
        public string BuyerTeam { get; set; }
        [Write(false)]
        public int DBIRollID { get; set; }
        [Write(false)]
        public bool IsRollUsed { get; set; }
        [Write(false)]
        public string Message { get; set; }
        [Write(false)]
        public decimal BalanceQty
        {
            get
            {
                if (JobCardQty <= ProducedQty)
                {
                    return "0".ToDecimal();
                }
                else
                {
                    return JobCardQty - ProducedQty;
                }
            }
            //set; 
        }

        [Write(false)]
        public string ConceptNo { get; set; }

        [Write(false)]
        public DateTime ConceptDate { get; set; }

        [Write(false)]
        public string MachineNo { get; set; }

        [Write(false)]
        public string MachineType { get; set; }

        [Write(false)]
        public decimal Dia { get; set; }

        [Write(false)]
        public decimal Gauge { get; set; }

        [Write(false)]
        public string MCBrand { get; set; }

        [Write(false)]
        public string UnitName { get; set; }

        [Write(false)]
        public string WeightURL { get; set; }

        [Write(false)]
        public string PrinterName { get; set; }

        [Write(false)]
        public bool GrayFabricOK { get; set; }

        [Write(false)]
        public string SubClassName { get; set; }

        [Write(false)]
        public string KnittingType { get; set; }

        [Write(false)]
        public string TechnicalName { get; set; }

        [Write(false)]
        public string Composition { get; set; }

        [Write(false)]
        public string GSM { get; set; }
        [Write(false)]
        public string ColorName { get; set; }
        [Write(false)]
        public string Size { get; set; }

        [Write(false)]
        public int PlanNo { get; set; }

        [Write(false)]
        public int PShiftId { get; set; }

        [Write(false)]
        public string SubGroupName { get; set; }

        [Write(false)]
        public string ShiftName { get; set; }

        [Write(false)]
        public string OperatorName { get; set; }

        [Write(false)]
        public int POperatorId { get; set; }
        [Write(false)]
        public string Shift { get; set; }
        [Write(false)]
        public int ColorID { get; set; }
        [Write(false)]
        public bool IsSaveDyeingBatchItemRoll { get; set; }
        [Write(false)]
        public DyeingBatchItemRoll DyeingBatchRollItem { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || GRollID > 0;

        [Write(false)]
        public IEnumerable<Select2OptionModel> QCByList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> QCShiftList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> KnittingTypeList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ConstructionList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> CompositionList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> GSMList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ColorList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ShiftList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> OperatorList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> PShiftList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> POperatorList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ProductionStatusList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> TechnicalNameList { get; set; }

        [Write(false)]
        public List<KJobCardChild> KJobCardChilds { get; set; }

        #endregion Additional Properties

        public KnittingProduction()
        {
            SendQCDate = DateTime.Now;
            DateAdded = DateTime.Now;
            ProductionDate = DateTime.Now;
            RollLength = 0;
            ProdQty = 0;
            KnittingProductionQCs = new List<KnittingProductionQC>();
            Childs = new List<KnittingProductionChildBindingModel>();
            KJobCardChilds = new List<KJobCardChild>();
            JobCardStatus = "";
            BatchNo = "";
            ColorID = 0;
            ITM = false;
            IsSubContact = false;
            SubGroupID = 0;
            KJobCardChildID = 0;
            DBIRollID = 0;
            IsSaveDyeingBatchItemRoll = false;
            DyeingBatchRollItem = new DyeingBatchItemRoll();
            RollChilds = new List<KnittingProduction>();
            QCPassQty = 0;
            QCPassQtyPcs = 0;

            Message = "";
            IsRollUsed = false;
        }
    }

    #region Validator

    public class KnittingProductionBindingModelValidator : AbstractValidator<KnittingProductionBindingModel>
    {
        public KnittingProductionBindingModelValidator()
        {
            RuleFor(x => x.ProdQty).GreaterThan(0);

            When(x => x.List.Any(), () =>
            {
                RuleForEach(x => x.List).SetValidator(new KnittingProductionModelValidator());
            });
            //When(x => x.QCChilds.Any(), () =>
            //{
            //    RuleForEach(x => x.QCChilds).SetValidator(new KnittingProductionQCBindingModelValidator());
            //});
        }
    }

    public class KnittingProductionModelValidator : AbstractValidator<KnittingProduction>
    {
        public KnittingProductionModelValidator()
        {
            //RuleFor(x => x.RollQty).NotEmpty();
            //RuleFor(x => x.OperatorID).NotEmpty();
            //RuleFor(x => x.ShiftID).NotEmpty();
        }
    }

    #endregion Validator

    #region Additonal Classes

    public class KnittingProductionBindingModel
    {
        public int ProdQty { get; set; }
        public DateTime ProductionDate { get; set; }
        public int KJobCardMasterID { get; set; }
        public int KJobCardChildID { get; set; }
        public string JobCardNo { get; set; }
        public int ConceptID { get; set; }
        public bool ProdComplete { get; set; }
        public bool ITM { get; set; }
        public List<KnittingProduction> List { get; set; }

        public KnittingProductionBindingModel()
        {
            List = new List<KnittingProduction>();
            ITM = false;
        }
    }

    public class KnittingProductionChildBindingModel
    {
        public KnittingProductionChildBindingModel()
        {
            RollSeqNo = 0;
        }

        public int GRollID { get; set; }
        public bool FirstRollCheck { get; set; }
        public decimal RollQty { get; set; }
        public int RollQtyPcs { get; set; }
        public int OperatorID { get; set; }
        public int ShiftID { get; set; }
        public decimal RollLength { get; set; }
        public decimal Width { get; set; }
        public decimal Weight { get; set; }
        public DateTime ProductionDate { get; set; }
        public string Operator { get; set; }
        public string Shift { get; set; }
        public int RollSeqNo { get; set; }
        public string RollNo { get; set; }
        public int KJobCardMasterID { get; set; }
        public int KJobCardChildID { get; set; }
        public bool ITM { get; set; }
        public EntityState EntityState { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }

        public string Composition { get; set; }
        public string GSM { get; set; }
        public string ColorName { get; set; }
        public string Size { get; set; }
    }

    #endregion Additonal Classes
}

using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{

    [Table(TableNames.FINISHING_PROCESS_MASTER)]
    public class FinishingProcessMaster : DapperBaseEntity
    {
        public FinishingProcessMaster()
        {
            DateAdded = DateTime.Now;
            PFBatchDate = DateTime.Now;
            PFBatchNo = AppConstants.NEW;
            FinishingProcessChilds = new List<FinishingProcessChild>();
            PreFinishingProcessChilds = new List<FinishingProcessChild>();
            PostFinishingProcessChilds = new List<FinishingProcessChild>();
            PostFinishingProcessChildColors = new List<FinishingProcessChild>();
            PreProcessList = new List<FinishingProcessChild>();
            PostProcessList = new List<FinishingProcessChild>();
            FinishingMachineConfigurationChildList = new List<FinishingMachineConfigurationChild>();
            ProcessMachineList = new List<FinishingProcessChild>();
            PDProductionComplete = false;
            PreFinishingProcessChildItems = new List<FinishingProcessChildItem>();
            ColorList = new List<FinishingProcessChild>();
        }

        [ExplicitKey]
        public int FPMasterID { get; set; }

        public int ConceptID { get; set; }
        public string PFBatchNo { get; set; }
        public DateTime PFBatchDate { get; set; }
        public int BatchQty { get; set; }

        public int BookingID { get; set; }

        public int TrialNo { get; set; }

        public DateTime? TrialDate { get; set; }

        public bool PDProductionComplete { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        #region Additional

        [Write(false)]
        public List<FinishingProcessChild> FinishingProcessChilds { get; set; }

        [Write(false)]
        public List<FinishingProcessChildItem> PreFinishingProcessChildItems { get; set; }

        [Write(false)]
        public List<FinishingProcessChild> ColorList { get; set; }

        [Write(false)]
        public string ConceptNo { get; set; }

        [Write(false)]
        public string SubClassName { get; set; }

        [Write(false)]
        public string TechnicalName { get; set; }

        [Write(false)]
        public int MachineGauge { get; set; }

        [Write(false)]
        public string Composition { get; set; }

        [Write(false)]
        public string Gsm { get; set; }

        [Write(false)]
        public decimal Length { get; set; }

        [Write(false)]
        public decimal Width { get; set; }

        [Write(false)]
        public string SubGroupID { get; set; }

        [Write(false)]
        public string SubGroupName { get; set; }

        [Write(false)]
        public string KnittingTypeName { get; set; }

        [Write(false)]
        public DateTime ConceptDate { get; set; }

        [Write(false)]
        public bool NeedPreFinishingProcess { get; set; }

        [Write(false)]
        public string FUPartName { get; set; }

        [Write(false)]
        public int IsBDS { get; set; }

        [Write(false)]
        public string GroupConceptNo { get; set; }

        [Write(false)]
        public string ColorName { get; set; }

        [Write(false)]
        public List<FinishingProcessChild> PreFinishingProcessChilds { get; set; }

        [Write(false)]
        public List<FinishingProcessChild> PostFinishingProcessChilds { get; set; }

        [Write(false)]
        public List<FinishingProcessChild> PostFinishingProcessChildColors { get; set; }

        [Write(false)]
        public List<FinishingProcessChild> PreProcessList { get; set; }

        [Write(false)]
        public List<FinishingProcessChild> PostProcessList { get; set; }

        [Write(false)]
        public List<FinishingMachineConfigurationChild> FinishingMachineConfigurationChildList { get; set; }

        [Write(false)]
        public List<FinishingProcessChild> ProcessMachineList { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || FPMasterID > 0;

        [Write(false)]
        public IEnumerable<Select2OptionModel> ShiftList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> OperatorList { get; set; }

        #endregion Additional
    }
}
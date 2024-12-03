using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table("T_FBookingAckChildFinishingProcess")]
    public class FBookingAckChildFinishingProcess : DapperBaseEntity
    {
        public FBookingAckChildFinishingProcess()
        {
            BookingChildID = 0;
            ColorID = 0;
            FinishingProcessChildItems = new List<FBookingAckChildFinishingProcessItem>();
            MachineParams = new List<Select2OptionModel>();
        }

        [ExplicitKey]
        public int FPChildID { get; set; }

        public int BookingChildID { get; set; }

        public int ProcessID { get; set; }

        public int ColorID { get; set; }

        public int SeqNo { get; set; }

        public int ProcessTypeID { get; set; }

        public bool IsPreProcess { get; set; }

        public string Remarks { get; set; }

        public int FMSID { get; set; }

        public string Param1Value { get; set; }

        public string Param2Value { get; set; }

        public string Param3Value { get; set; }

        public string Param4Value { get; set; }

        public string Param5Value { get; set; }

        public string Param6Value { get; set; }

        public string Param7Value { get; set; }

        public string Param8Value { get; set; }

        public string Param9Value { get; set; }

        public string Param10Value { get; set; }

        public string Param11Value { get; set; }

        public string Param12Value { get; set; }

        public string Param13Value { get; set; }

        public string Param14Value { get; set; }

        public string Param15Value { get; set; }

        public string Param16Value { get; set; }

        public string Param17Value { get; set; }

        public string Param18Value { get; set; }

        public string Param19Value { get; set; }

        public string Param20Value { get; set; }

        public DateTime? ProductionDate { get; set; }

        public int ShiftID { get; set; }

        public int OperatorID { get; set; }

        public int PFMSID { get; set; }

        public string PParam1Value { get; set; }

        public string PParam2Value { get; set; }

        public string PParam3Value { get; set; }

        public string PParam4Value { get; set; }

        public string PParam5Value { get; set; }

        public string PParam6Value { get; set; }

        public string PParam7Value { get; set; }

        public string PParam8Value { get; set; }

        public string PParam9Value { get; set; }

        public string PParam10Value { get; set; }

        public string PParam11Value { get; set; }

        public string PParam12Value { get; set; }

        public string PParam13Value { get; set; }

        public string PParam14Value { get; set; }

        public string PParam15Value { get; set; }

        public string PParam16Value { get; set; }

        public string PParam17Value { get; set; }

        public string PParam18Value { get; set; }

        public string PParam19Value { get; set; }

        public string PParam20Value { get; set; }

        #region Additional

        [Write(false)]
        public string PFBatchNo { get; set; }

        [Write(false)]
        public string DBatchNo { get; set; }

        [Write(false)]
        public int CCColorID { get; set; }

        [Write(false)]
        public int ConceptID { get; set; }

        [Write(false)]
        public string ProcessName { get; set; }

        [Write(false)]
        public string ProcessType { get; set; }

        [Write(false)]
        public string MachineName { get; set; }

        [Write(false)]
        public string UnitName { get; set; }

        [Write(false)]
        public string BrandName { get; set; }

        [Write(false)]
        public string FMCMasterID { get; set; }

        [Write(false)]
        public string ColorName { get; set; }

        [Write(false)]
        public string MachineNo { get; set; }

        [Write(false)]
        public string PMachineNo { get; set; }
        [Write(false)]
        public string PBrandName { get; set; }
        [Write(false)]
        public string PUnitName { get; set; }

        [Write(false)]
        public int BatchQty { get; set; }

        [Write(false)]
        public string ParamName { get; set; }

        [Write(false)]
        public string ParamValue { get; set; }

        [Write(false)]
        public string DefaultValue { get; set; }

        [Write(false)]
        public string ParamValueEntry { get; set; }

        [Write(false)]
        public string ParamDispalyName { get; set; }

        [Write(false)]
        public string ShiftName { get; set; }

        [Write(false)]
        public string OperatorName { get; set; }

        [Write(false)]
        public bool NeedItem { get; set; }

        [Write(false)]
        public int SerialNo { get; set; }

        [Write(false)]
        public string PlanParamValue { get; set; }

        [Write(false)]
        public string ActulaPlanParamValue { get; set; }

        [Write(false)]
        public string ItemIDs { get; set; }
        [Write(false)]
        public int IsSelected { get; set; } = 0;
        [Write(false)]
        public int MachineDia { get; set; } = 0;
        [Write(false)]
        public int MachineGauge { get; set; } = 0;

        [Write(false)]
        public List<FBookingAckChildFinishingProcessItem> FinishingProcessChildItems { get; set; }

        [Write(false)]
        public List<Select2OptionModel> MachineParams { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || FPChildID > 0;

        #endregion Additional
    }
}

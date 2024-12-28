using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.DTOs;
using System.Collections.Generic;

namespace EPYSLTEX.Core.Entities.Tex
{
    [Table("RollFinishingInfo")]
    public class RollFinishingInfo : DapperBaseEntity
    {
        public RollFinishingInfo()
        {
            MachineID = 0;
            TempIn = 0;
            Speed = 0;
            Feed = 0;
            Stream = 0;
            UnitID = 0;
        }

        [ExplicitKey]
        public int RFinishingID { get; set; }

        public int DBatchID { get; set; }

        public int FPChildID { get; set; }

        public int ProcessID { get; set; }

        public int UnitID { get; set; }

        public int BrandID { get; set; }

        public int MachineID { get; set; }

        public decimal TempIn { get; set; }

        public decimal Speed { get; set; }

        public decimal Feed { get; set; }

        public decimal Stream { get; set; }

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

        #region Additional

        [Write(false)]
        public string DBatchNo { get; set; }

        [Write(false)]
        public string ProcessName { get; set; }

        [Write(false)]
        public string UnitName { get; set; }

        [Write(false)]
        public string BrandName { get; set; }

        [Write(false)]
        public int ProcessTypeID { get; set; }

        [Write(false)]
        public string ProcessType { get; set; }

        [Write(false)]
        public string MachineNo { get; set; }

        [Write(false)]
        public string MachineName { get; set; }

        [Write(false)]
        public int FMCMasterID { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> RollFinishingQCStatusList { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || RFinishingID > 0;

        #endregion Additional
    }
}
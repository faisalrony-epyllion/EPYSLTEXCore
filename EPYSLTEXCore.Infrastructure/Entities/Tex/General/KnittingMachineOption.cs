using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General
{
    [Table("T_KnittingMachineOption")]
    public class KnittingMachineOption : DapperBaseEntity
    {
        [ExplicitKey]
        public int OptionID { get; set; }
        ///<summary>
        /// KnittingMachineID
        ///</summary>
        public int KnittingMachineID { get; set; }

        ///<summary>
        /// MachineGauge
        ///</summary>
        public int MachineGauge { get; set; }

        ///<summary>
        /// CylinderNo
        ///</summary>
        public decimal CylinderNo { get; set; }

        ///<summary>
        /// Needle
        ///</summary>
        public int? Needle { get; set; }

        #region Additional Propeties

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || OptionID > 0;

        #endregion Additional Propeties
        public KnittingMachineOption()
        {
            MachineGauge = 0;
            CylinderNo = 0;
        }
    }
}

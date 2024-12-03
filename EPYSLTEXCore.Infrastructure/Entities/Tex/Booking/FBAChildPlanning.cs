using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table(TableNames.FBOOKING_ACKNOWLEDGE_CHILD_PLANNING)]
    public class FBAChildPlanning : DapperBaseEntity
    {
        [ExplicitKey]
        public int FBAChildPlanningID { get; set; }

        public int BookingChildID { get; set; }

        public int AcknowledgeID { get; set; }

        public int CriteriaID { get; set; }
        public int TotalDays { get; set; }

        #region Additional

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || FBAChildPlanningID > 0;

        [Write(false)]
        public string CriteriaName { get; set; }

        [Write(false)]
        public string OperationName { get; set; }

        [Write(false)]
        public int ProcessTime { get; set; }

        [Write(false)]
        public int GroupNo { get; set; }

        [Write(false)]
        public int ExternalID { get; set; }

        [Write(false)]
        public bool IsSelected { get; set; }

        [Write(false)]
        public string CriteriaIDs { get; set; }

        [Write(false)]
        public int TotalTime { get; set; }

        #endregion Additional
    }
}

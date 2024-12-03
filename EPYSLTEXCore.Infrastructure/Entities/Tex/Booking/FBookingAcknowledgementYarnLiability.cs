using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table("T_FBookingAcknowledgementYarnLiability")]
    public class FBookingAcknowledgementYarnLiability : DapperBaseEntity
    {
        #region Table properties

        [ExplicitKey]
        public int YLChildID { get; set; }
        public int AcknowledgeID { get; set; }
        public int BookingChildID { get; set; } = 0;
        public int ConsumptionID { get; set; }
        public int BookingID { get; set; }
        public int ItemMasterID { get; set; }
        public int UnitID { get; set; }
        public bool YD { get; set; }
        public decimal LiabilityQty { get; set; }
        public decimal Rate { get; set; }
        public int YBChildItemID { get; set; }
        public int AllocationChildID { get; set; }

        #endregion Table properties

        #region Additional Columns
        [Write(false)]
        public bool Blending { get; set; }
        [Write(false)]
        public decimal Distribution { get; set; }
        [Write(false)]
        public decimal BookingQty { get; set; }
        [Write(false)]
        public decimal Allowance { get; set; }
        [Write(false)]
        public decimal RequiredQty { get; set; }
        [Write(false)]
        public string ShadeCode { get; set; }
        [Write(false)]
        public string Remarks { get; set; }
        [Write(false)]
        public decimal ReqQty { get; set; }
        [Write(false)]
        public string POStatus { get; set; }
        [Write(false)]
        public decimal AllocatedQty { get; set; }
        [Write(false)]
        public decimal IssueQty { get; set; }
        [Write(false)]
        public decimal TotalIssueQty { get; set; }
        [Write(false)]
        public decimal YDProdQty { get; set; }
        [Write(false)]
        public decimal TotalValue { get; set; }
        [Write(false)]
        public string Specification { get; set; }
        [Write(false)]
        public string DisplayUnitDesc { get; set; }
        [Write(false)]
        public string _segment1ValueDesc { get; set; }
        [Write(false)]
        public string _segment2ValueDesc { get; set; }
        [Write(false)]
        public string _segment3ValueDesc { get; set; }
        [Write(false)]
        public string _segment4ValueDesc { get; set; }
        [Write(false)]
        public string _segment5ValueDesc { get; set; }
        [Write(false)]
        public string _segment6ValueDesc { get; set; }
        [Write(false)]
        public string _segment7ValueDesc { get; set; }
        [Write(false)]
        public string _segment8ValueDesc { get; set; }
        [Write(false)]
        public int ChildID { get; set; }
        [Write(false)]
        public string YarnCategory { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YLChildID > 0;

        #endregion Additional Columns
        public FBookingAcknowledgementYarnLiability()
        {
            YD = false;
            LiabilityQty = 0;
            Rate = 0;
            IssueQty = 0;
            TotalIssueQty = 0;
            YDProdQty = 0;
            ReqQty = 0;
            POStatus = "";
            AllocatedQty = 0;
            YarnCategory = "";
            YBChildItemID = 0;
            AllocationChildID = 0;
        }
        public FBookingAcknowledgementYarnLiability(FBookingAcknowledgementYarnLiability x)
        {
            if (x != null)
            {
                ChildID = x.ChildID;
                AcknowledgeID = 0;
                YLChildID = 0;
                BookingID = x.BookingID;
                ItemMasterID = x.ItemMasterID;
                ConsumptionID = x.ConsumptionID;
                UnitID = x.UnitID;
                DisplayUnitDesc = x.DisplayUnitDesc;
                Blending = x.Blending;
                YarnCategory = x.YarnCategory;
                BookingQty = x.BookingQty;
                ShadeCode = x.ShadeCode;
                Remarks = x.Remarks;
                Specification = x.Specification;
                YD = x.YD;
                _segment1ValueDesc = x._segment1ValueDesc;
                _segment2ValueDesc = x._segment2ValueDesc;
                _segment3ValueDesc = x._segment3ValueDesc;
                _segment4ValueDesc = x._segment4ValueDesc;
                _segment5ValueDesc = x._segment5ValueDesc;
                _segment6ValueDesc = x._segment6ValueDesc;
                _segment7ValueDesc = x._segment7ValueDesc;
                _segment8ValueDesc = x._segment8ValueDesc;
                LiabilityQty = x.LiabilityQty;
                Rate = x.Rate;
            }
        }

    }
}

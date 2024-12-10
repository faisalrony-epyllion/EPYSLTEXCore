using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILD)]
    public class ProjectionYarnBookingItemChild : YarnItemMaster, IDapperBaseEntity
    {
        public ProjectionYarnBookingItemChild()
        {
            SegmentValueId1 = 0;
            SegmentValueId2 = 0;
            SegmentValueId3 = 0;
            SegmentValueId4 = 0;
            SegmentValueId5 = 0;
            SegmentValueId6 = 0;
            SegmentValueId7 = 0;
            SegmentValueId8 = 0;
            SegmentValueId9 = 0;
            SegmentValueId10 = 0;
            SegmentValueId11 = 0;
            SegmentValueId12 = 0;
            SegmentValueId13 = 0;
            SegmentValueId14 = 0;
            SegmentValueId15 = 0;
            YarnCategory = "";
            Segment1ValueDesc = "";
            Segment2ValueDesc = "";
            Segment3ValueDesc = "";
            Segment4ValueDesc = "";
            Segment5ValueDesc = "";
            Segment6ValueDesc = "";
            Segment7ValueDesc = "";
            Segment8ValueDesc = "";
            Segment9ValueDesc = "";
            Segment10ValueDesc = "";
            Segment11ValueDesc = "";
            Segment12ValueDesc = "";
            Segment13ValueDesc = "";
            Segment14ValueDesc = "";
            Segment15ValueDesc = "";
            EntityState = EntityState.Added;
            PYBItemChildDetails = new List<ProjectionYarnBookingItemChildDetails>();
            IsReceived = false;
        }

        [ExplicitKey]
        public int PYBBookingChildID { get; set; } = 0;
        public int PYBookingID { get; set; } = 0;
        public decimal QTY { get; set; } = 0;
        public decimal ReqCone { get; set; } = 0;
        public string ShadeCode { get; set; } = "";
        public decimal PPrice { get; set; } = 0;
        public string Remarks { get; set; } = "";
        public int DayValidDurationId { get; set; } = 0;
        public int SegmentValueId1 { get; set; } = 0;
        public int SegmentValueId2 { get; set; } = 0;
        public int SegmentValueId3 { get; set; } = 0;
        public int SegmentValueId4 { get; set; } = 0;
        public int SegmentValueId5 { get; set; } = 0;
        public int SegmentValueId6 { get; set; } = 0;
        public int SegmentValueId7 { get; set; } = 0;
        public int SegmentValueId8 { get; set; } = 0;
        public int SegmentValueId9 { get; set; } = 0;
        public int SegmentValueId10 { get; set; } = 0;
        public int SegmentValueId11 { get; set; } = 0;
        public int SegmentValueId12 { get; set; } = 0;
        public int SegmentValueId13 { get; set; } = 0;
        public int SegmentValueId14 { get; set; } = 0;
        public int SegmentValueId15 { get; set; } = 0;
        public string YarnCategory { get; set; } = "";

        //public string RevisionNo { get; set; }

        #region Additional Fields

        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; } = 0;

        [Write(false)]
        public string YarnBrand { get; set; } = "";

        [Write(false)]
        public string DisplayUnitDesc { get; set; } = "";
        [Write(false)]
        public bool IsReceived { get; set; } = false;

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        [Write(false)]
        public List<ProjectionYarnBookingItemChildDetails> PYBItemChildDetails { get; set; }

        #endregion Additional Fields
    }

    #region Validator
    /*
    public class ProjectionYarnBookingItemChildValidator : AbstractValidator<ProjectionYarnBookingItemChild>
    {
        public ProjectionYarnBookingItemChildValidator()
        {
            RuleFor(x => x.PYBItemChildDetails).NotEmpty().WithMessage("Please enter details for each item!");
            //RuleFor(x => x.IssueQty).NotEmpty().WithMessage("Issue quantity is required.");
            //RuleFor(x => x.IssueCone).NotEmpty().WithMessage("Issue cone is required.");
        }
    }
    */
    #endregion Validator
}

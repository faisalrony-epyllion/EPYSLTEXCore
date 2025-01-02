using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_BOOKING_MASTER)]
    public class YDBookingMaster : DapperBaseEntity
    {
        public YDBookingMaster()
        {
            TotalBookingQty = 0;
            PreProcessRevNo = 0;
            RevisionNo = 0;
            RevisionBy = 0;
            YDBookingNo = AppConstants.NEW;
            YDBookingDate = DateTime.Now;
            DateAdded = DateTime.Now;
            YDBookingChilds = new List<YDBookingChild>();
            YDBookingChildTwistings = new List<YDBookingChildTwisting>();
            YDBookingRefs = new List<YDBookingRef>();
            ColorChilds = new List<FreeConceptChildColor>();
            IsUnAcknowledge = false;
            IsAdditional = false;
            ApproveBy = 0;
            AcknowledgeBy = 0;
            UnAcknowledgeBy = 0;
            UpdatedBy = 0;
            ExportOrderID = 0;
            IsYDBNoGenerated = false;
            YDBNo = "";
            MRStatus = "";
            RRStatus = "";
            RStatus = "";
        }

        [ExplicitKey]
        public int YDBookingMasterID { get; set; }

        public string YDBookingNo { get; set; }

        public int YDBookingBy { get; set; }

        public DateTime YDBookingDate { get; set; }

        public string RefBatchNo { get; set; }

        public string SampleRefNo { get; set; }

        public string SwatchFilePath { get; set; }

        public string PreviewTemplate { get; set; }

        public int BuyerID { get; set; }
        public int BuyerTeamID { get; set; }
        public string Remarks { get; set; }

        public bool SendForApproval { get; set; }

        public bool IsApprove { get; set; }

        public DateTime? ApproveDate { get; set; }

        public int ApproveBy { get; set; }

        public bool IsAcknowledge { get; set; }
        public DateTime? AcknowledgeDate { get; set; }

        public int AcknowledgeBy { get; set; }

        public bool IsUnAcknowledge { get; set; }

        public DateTime? UnAcknowledgeDate { get; set; }

        public int UnAcknowledgeBy { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        public int ExportOrderID { get; set; }
        public string GroupConceptNo { get; set; }
        public int ConceptID { get; set; }

        public int YBookingID { get; set; }

        public decimal TotalBookingQty { get; set; }
        public int PreProcessRevNo { get; set; }
        public int RevisionNo { get; set; }
        public DateTime? RevisionDate { get; set; }
        public int RevisionBy { get; set; }
        public string RevisionReason { get; set; }

        public string UnAckReason { get; set; }
        public int ReqTypeID { get; set; } = 0;
        public bool IsYDBNoGenerated { get; set; }
        public string YDBNo { get; set; }

        #region Additional Properties
        [Write(false)]
        public int IsBDS { get; set; }
        [Write(false)]
        public string ConceptNo { get; set; }

        [Write(false)]
        public string YBookingNo { get; set; }

        [Write(false)]
        public string PDate { get; set; }

        [Write(false)]
        public string ProgramName { get; set; }

        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public string RevisionStatus { get; set; }
        [Write(false)]
        public string ImagePath { get; set; }
        [Write(false)]
        public string ImagePath1 { get; set; }
        [Write(false)]
        public bool IsSample { get; set; } = false;
        [Write(false)]
        public List<YDBookingChild> YDBookingChilds { get; set; }

        [Write(false)]
        public List<YDBookingChildTwisting> YDBookingChildTwistings { get; set; }

        [Write(false)]
        public List<YDBookingChildTwistingColor> YDBookingChildTwistingColors { get; set; }

        [Write(false)]
        public List<FreeConceptChildColor> ColorChilds { get; set; }

        [Write(false)]
        public List<YDBookingRef> YDBookingRefs { get; set; }

        [Write(false)]
        public string FabricBookingNo { get; set; }
        [Write(false)]
        public string MRStatus { get; set; }
        [Write(false)]
        public string RRStatus { get; set; }

        [Write(false)]
        public string RStatus { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> BuyerList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> FabricBookingList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnProgramList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnSubProgramList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnTypeList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> UsesInList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnShadeBooks { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnCompositionList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnColorList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ShadeList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnDyeingForList { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDBookingMasterID > 0;

        [Write(false)]
        public IEnumerable<Select2OptionModel> SpinnerList { get; set; }
        [Write(false)]
        public bool IsAdditional { get; set; }
        [Write(false)]
        public string parentYDBookingNo { get; set; }

        #endregion Additional Properties
    }
}

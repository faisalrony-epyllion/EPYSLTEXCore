using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Fabric
{
    [Table(TableNames.FabricBookingAcknowledge)]
    public class FabricBookingAcknowledge : DapperBaseEntity
    {
        #region Table properties

        [ExplicitKey]
        public int AcknowledgeID { get; set; }

        public int PreProcessRevNo { get; set; }

        public int RevisionNo { get; set; }

        public int BookingID { get; set; }

        public int BOMMasterID { get; set; }

        public int ItemGroupID { get; set; }

        public int SubGroupID { get; set; }

        public bool Status { get; set; }

        public DateTime? AcknowledgeDate { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int UpdatedBy { get; set; } = 0;

        public DateTime? DateUpdated { get; set; }

        public bool WithoutOB { get; set; }

        public int UnAcknowledgeBy { get; set; }

        public DateTime? UnAcknowledgeDate { get; set; }

        public bool UnAcknowledge { get; set; }


        #endregion Table properties

        #region Additional Columns
        [Write(false)]
        public int MenuId { get; set; }
        [Write(false)]
        public string UnAcknowledgeReason { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChild> FBookingChild { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChild> FBookingChildCollor { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChild> FBookingChildCuff { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChild> FBookingChildDetailsgroup { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildDetails> FBookingChildDetails { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildDetails> FBookingChildDetailsCollar { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildDetails> FBookingChildDetailsCuff { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildAddProcess> FBookingAcknowledgeChildAddProcess { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildGarmentPart> FBookingAcknowledgeChildGarmentPart { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildProcess> FBookingAcknowledgeChildProcess { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildText> FBookingAcknowledgeChildText { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildDistribution> FBookingAcknowledgeChildDistribution { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildYarnSubBrand> FBookingAcknowledgeChildYarnSubBrand { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeImage> FBookingAcknowledgeImage { get; set; }
        [Write(false)]
        public List<BDSDependentTNACalander> BDSDependentTNACalander { get; set; }
        [Write(false)]
        public List<KnittingMachine> KnittingMachines { get; set; }
        [Write(false)]
        public List<FreeConceptMaster> FreeConcepts { get; set; }
        [Write(false)]
        public List<FreeConceptMaster> FreeConceptsCollar { get; set; }
        [Write(false)]
        public List<FreeConceptMaster> FreeConceptsCuff { get; set; }
        [Write(false)]
        public List<FBAChildPlanning> AllChildPlannings { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> TechnicalNameList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnSourceNameList { get; set; }


        [Write(false)]
        public IEnumerable<Select2OptionModel> MCTypeForFabricList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> MCTypeForOtherList { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildColor> ColorCodeList { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgementLiabilityDistribution> FBookingAckLiabilityDistributionList { get; set; }

        [Write(false)]
        public List<FBookingAcknowledgementYarnLiability> FBookingAcknowledgementYarnLiabilityList { get; set; }
        [Write(false)]
        public List<FBookingAcknowledge> FBookingAcknowledgeList { get; set; }
        [Write(false)]
        public List<FabricBookingAcknowledge> FabricBookingAcknowledgeList { get; set; }
        [Write(false)]
        public bool HasFabric { get; set; }
        [Write(false)]
        public bool HasCollar { get; set; }
        [Write(false)]
        public bool HasCuff { get; set; }
        [Write(false)]
        public string SubGroupName { get; set; }
        [Write(false)]
        public string EmailID { get; set; }
        [Write(false)]
        public string ToMailID { get; set; }
        [Write(false)]
        public string CCMailID { get; set; }
        [Write(false)]
        public string BCCMailID { get; set; }
        [Write(false)]
        public bool IsSample { get; set; }

        [Write(false)]
        public decimal BookingQty { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || AcknowledgeID > 0;

        #endregion Additional Columns

        public FabricBookingAcknowledge()
        {
            FBookingChild = new List<FBookingAcknowledgeChild>();
            FBookingChildDetails = new List<FBookingAcknowledgeChildDetails>();
            FBookingAcknowledgeChildAddProcess = new List<FBookingAcknowledgeChildAddProcess>();
            FBookingAcknowledgeChildGarmentPart = new List<FBookingAcknowledgeChildGarmentPart>();
            FBookingAcknowledgeChildProcess = new List<FBookingAcknowledgeChildProcess>();
            FBookingAcknowledgeChildText = new List<FBookingAcknowledgeChildText>();
            FBookingAcknowledgeChildDistribution = new List<FBookingAcknowledgeChildDistribution>();
            FBookingAcknowledgeChildYarnSubBrand = new List<FBookingAcknowledgeChildYarnSubBrand>();
            FBookingAcknowledgeImage = new List<FBookingAcknowledgeImage>();
            BDSDependentTNACalander = new List<BDSDependentTNACalander>();
            FreeConcepts = new List<FreeConceptMaster>();
            TechnicalNameList = new List<Select2OptionModel>();
            ColorCodeList = new List<FBookingAcknowledgeChildColor>();
            KnittingMachines = new List<KnittingMachine>();
            AllChildPlannings = new List<FBAChildPlanning>();
            FBookingAckLiabilityDistributionList = new List<FBookingAcknowledgementLiabilityDistribution>();
            FBookingAcknowledgementYarnLiabilityList = new List<FBookingAcknowledgementYarnLiability>();
            FabricBookingAcknowledgeList = new List<FabricBookingAcknowledge>();
            FBookingAcknowledgeList = new List<FBookingAcknowledge>();

            IsSample = false;
            UnAcknowledgeReason = "";
            MenuId = 0;
        }
    }
}

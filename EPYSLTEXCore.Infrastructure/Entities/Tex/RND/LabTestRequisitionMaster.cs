using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.LAB_TEST_REQUISITION_MASTER)]
    public class LabTestRequisitionMaster : DapperBaseEntity
    {
        public LabTestRequisitionMaster()
        {
            ReqNo = AppConstants.NEW;
            ReqDate = DateTime.Now;
            DateAdded = DateTime.Now;
            UnAcknowledge = false;
            ExportOrderID = 0;
            BuyerID = 0;
            BuyerTeamID = 0;
            ContactPersonID = 0;
            FabricQty = 0m;
            PerformanceCode = "";
            IsProduction = false;
            LabTestStatus = "";
            WashTemp = 0;
            CareInstruction = "";
            TestNatureID = 0;
            LabTestRequisitionBuyers = new List<LabTestRequisitionBuyer>();
            LabTestRequisitionImages = new List<LabTestRequisitionImage>();
            CareLabels = new List<LabTestRequisitionCareLabel>();
            Countries = new List<LabTestRequisitionExportCountry>();
            EndUses = new List<LabTestRequisitionEndUse>();
            FinishDyeMethods = new List<LabTestRequisitionSpecialFinishDyeMethod>();

            CountryList = new List<LabTestRequisitionExportCountry>();
            EndUsesList = new List<LabTestRequisitionEndUse>();
            FinishDyeMethodsList = new List<LabTestRequisitionSpecialFinishDyeMethod>();
            NewLabTestRequisitionBuyerParameters = new List<LabTestRequisitionBuyerParameter>();
            WashTempList = new List<Select2OptionModel>();
            TestNatureList = new List<Select2OptionModel>();

            DefaultCareInstruction = "";
            TestNatureName = "";
        }

        [ExplicitKey]
        public int LTReqMasterID { get; set; }
        public string ReqNo { get; set; }
        public DateTime ReqDate { get; set; }
        public int DBatchID { get; set; }
        public int ConceptID { get; set; }
        public int BookingID { get; set; }
        public int ExportOrderID { get; set; }
        public int BuyerID { get; set; }
        public int BuyerTeamID { get; set; }
        public int ItemMasterID { get; set; }
        public int ColorID { get; set; }
        public decimal FabricQty { get; set; }
        public int UnitID { get; set; }
        public string Remarks { get; set; }
        public bool IsApproved { get; set; }
        public int ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool IsAcknowledge { get; set; }
        public int AcknowledgeBy { get; set; }
        public int KnittingUnitID { get; set; }
        public int WashTemp { get; set; }
        public DateTime? AcknowledgeDate { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int LabTestServiceTypeID { get; set; }
        public bool IsRetest { get; set; }
        public int ContactPersonID { get; set; }
        public string RetestNo { get; set; }
        public string ImagePath { get; set; }
        public string FileName { get; set; }
        public string PreviewTemplate { get; set; }
        public int SubGroupID { get; set; }
        public string GroupConceptNo { get; set; }
        public string PerformanceCode { get; set; }
        public bool IsProduction { get; set; }
        public string CareInstruction { get; set; }

        public bool UnAcknowledge { get; set; }
        public int UnAcknowledgeBy { get; set; }
        public DateTime? UnAcknowledgeDate { get; set; }
        public string UnAcknowledgeReason { get; set; }

        public int RevisionNo { get; set; }
        public DateTime? RevisionDate { get; set; }
        public int RevisionBy { get; set; }
        public string RevisionReason { get; set; }



        #region Additional Property
        [Write(false)]
        public string ContactPersonName { get; set; }
        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public string BuyerTeamName { get; set; }
        [Write(false)]
        public string LabTestStatus { get; set; }
        [Write(false)]
        public List<LabTestRequisitionBuyer> LabTestRequisitionBuyers { get; set; }
        [Write(false)]
        public List<LabTestRequisitionImage> LabTestRequisitionImages { get; set; }
        [Write(false)]
        public List<LabTestRequisitionBuyerParameter> NewLabTestRequisitionBuyerParameters { get; set; }
        [Write(false)]
        public List<LabTestRequisitionBuyerParameter> LabTestRequisitionBuyerParameters { get; set; }

        [Write(false)]
        public List<LabTestRequisitionBuyerParameter> BuyerParameters { get; set; }

        [Write(false)]
        public List<LabTestRequisitionCareLabel> CareLabels { get; set; }

        [Write(false)]
        public string DBatchNo { get; set; }

        [Write(false)]
        public DateTime DBatchDate { get; set; }

        [Write(false)]
        public string ColorName { get; set; }
        [Write(false)]
        public string SubGroupName { get; set; }
        [Write(false)]
        public string BookingNo { get; set; }
        [Write(false)]
        public int TestNatureID { get; set; }
        [Write(false)]
        public string ConceptNo { get; set; }
        [Write(false)]
        public string DefaultCareInstruction { get; set; }
        [Write(false)]
        public bool IsSend { get; set; }

        [Write(false)]
        public List<Select2OptionModel> UnitList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> WashTempList { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LTReqMasterID > 0;

        [Write(false)]
        public string SubClassName { get; set; }

        [Write(false)]
        public string TechnicalName { get; set; }

        [Write(false)]
        public string Gsm { get; set; }

        [Write(false)]
        public string Composition { get; set; }

        [Write(false)]
        public decimal Length { get; set; }

        [Write(false)]
        public decimal Width { get; set; }

        [Write(false)]
        public string FUPartName { get; set; }
        [Write(false)]
        public string TestNatureName { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> LabTestServiceTypeList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> ContactPersonList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> TestNatureList { get; set; }

        //[Write(false)]
        //public int ContactID { get; set; }
        [Write(false)]
        public string ShortName { get; set; }
        [Write(false)]
        public string Buyer { get; set; }
        [Write(false)]
        public string BuyerTeam { get; set; }
        [Write(false)]
        public List<LabTestRequisitionExportCountry> Countries { get; set; }
        [Write(false)]
        public List<LabTestRequisitionEndUse> EndUses { get; set; }
        [Write(false)]
        public List<LabTestRequisitionSpecialFinishDyeMethod> FinishDyeMethods { get; set; }
        [Write(false)]
        public List<LabTestRequisitionExportCountry> CountryList { get; set; }
        [Write(false)]
        public List<LabTestRequisitionEndUse> EndUsesList { get; set; }
        [Write(false)]
        public List<LabTestRequisitionSpecialFinishDyeMethod> FinishDyeMethodsList { get; set; }
        #endregion Additional Property
    }

    public class LabTestRequisitionMasterValidator : AbstractValidator<LabTestRequisitionMaster>
    {
        public LabTestRequisitionMasterValidator()
        {
            //RuleFor(x => x.UnitID).NotEmpty();
            //RuleFor(x => x.FabricQty).NotEmpty().WithMessage("");
        }
    }
}
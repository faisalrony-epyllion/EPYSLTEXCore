using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAPOMaster")]
    public class CDAPOMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int CDAPOMasterID { get; set; }
        public int SubGroupID { get; set; }
        public string PONo { get; set; }
        public DateTime PODate { get; set; }
        public int RevisionNo { get; set; }
        public DateTime? RevisionDate { get; set; }
        public int CompanyID { get; set; }
        public int SupplierID { get; set; }
        public int POForID { get; set; }
        public int CurrencyID { get; set; }
        public DateTime DeliveryStartDate { get; set; }
        public DateTime DeliveryEndDate { get; set; }
        public string Remarks { get; set; }
        public string InternalNotes { get; set; }
        public int IncoTermsID { get; set; }
        public int PaymentTermsID { get; set; }
        public int? TypeOfLCID { get; set; }
        public int? TenureofLC { get; set; }
        public int? CalculationofTenure { get; set; }
        public int? CreditDays { get; set; }
        public int ReImbursementCurrencyID { get; set; }
        public string Charges { get; set; }
        public int CountryOfOriginID { get; set; }
        public bool TransShipmentAllow { get; set; }
        public decimal ShippingTolerance { get; set; }
        public int? PortofLoadingID { get; set; }
        public int? PortofDischargeID { get; set; }
        public int? ShipmentModeID { get; set; }
        public bool Proposed { get; set; }
        public int ProposedBy { get; set; }
        public DateTime? ProposedDate { get; set; }
        public bool UnApprove { get; set; }
        public int UnApproveBy { get; set; }
        public DateTime? UnApproveDate { get; set; }
        public string UnapproveReason { get; set; }
        public bool Approved { get; set; }
        public int ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public int AddedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int OfferValidity { get; set; }
        public int? QualityApprovalProcedureID { get; set; }
        public bool SignIn { get; set; }
        public DateTime? SignInDate { get; set; }
        public int SignInBy { get; set; }
        public bool SupplierAcknowledge { get; set; }
        public int SupplierAcknowledgeBy { get; set; }
        public DateTime? SupplierAcknowledgeDate { get; set; }
        public bool SupplierReject { get; set; }
        public int SupplierRejectBy { get; set; }
        public string SupplierRejectReason { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string QuotationRefNo { get; set; }
        public int CDAPRMasterID { get; set; }
        public DateTime? QuotationRefDate { get; set; }

        #region Additional Columns
        [Write(false)]
        public DateTime PRRequiredDate { get; set; } 
        [Write(false)]
        public DateTime PRDate { get; set; }
        [Write(false)]
        public string PRNO { get; set; } 
        
        [Write(false)]
        public string POStatus { get; set; } 
        [Write(false)]
        public string CompanyName { get; set; }
        [Write(false)]
        public string SupplierName { get; set; }
        [Write(false)]
        public string POFor { get; set; }
        [Write(false)]
        public decimal TotalQty { get; set; }
        [Write(false)]
        public decimal TotalValue { get; set; }
        [Write(false)]
        public string CurrencyCode { get; set; }
        
        [Write(false)]
        public string ReImbursmentCurrency { get; set; }
        [Write(false)]
        public string CountyOfOrigin { get; set; }
       
        [Write(false)]
        public DateTime? InHouseDate { get; set; }
        [Write(false)]
        public DateTime? SFToPLDate { get; set; }
        [Write(false)]
        public DateTime? PLToPDDate { get; set; }
        [Write(false)]
        public DateTime? PDToCFDate { get; set; }
        [Write(false)]
        public int SFToPLDays { get; set; }
        [Write(false)]
        public int PLToPDDays { get; set; }
        [Write(false)]
        public int PDToCFDays { get; set; }
        [Write(false)]
        public int InHouseDays { get; set; }
        [Write(false)]
        public string UserName { get; set; }
        [Write(false)]
        public string ApproveBy { get; set; }
        [Write(false)]
        public string AcknowledgeBy { get; set; }
        [Write(false)]
        public string PRByUser { get; set; }  
        [Write(false)]
        public int Segment1ValueID { get; set; }
        [Write(false)]
        public string Segment1ValueDesc { get; set; }
        [Write(false)]
        public int Segment2ValueID { get; set; }
        [Write(false)]
        public string Segment2ValueDesc { get; set; }
        [Write(false)]
        public int Segment3ValueID { get; set; }
        [Write(false)]
        public string Segment3ValueDesc { get; set; }
        [Write(false)]
        public int Segment4ValueID { get; set; }
        [Write(false)]
        public string Segment4ValueDesc { get; set; }
        [Write(false)]
        public int ReqQty { get; set; }
        [Write(false)]
        public int CDAPOChildID { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> SupplierList { get; set; }
        
        [Write(false)]
        public IEnumerable<Select2OptionModel> IncoTermsList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> PaymentTermsList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> POForList { get; set; }
         
        [Write(false)]
        public IEnumerable<Select2OptionModel> PortofLoadingList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> PortofDischargeList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> CalculationOfTenureList { get; set; }
        
        [Write(false)]
        public IEnumerable<Select2OptionModel> ShipmentModeList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> CountryOfOriginList { get; set; }
        
        [Write(false)]
        public IEnumerable<Select2OptionModel> OfferValidityList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> QualityApprovalProcedureList { get; set; }
         
        [Write(false)]
        public IEnumerable<Select2OptionModel> CreditDaysList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> TypeOfLCList { get; set; } 
        [Write(false)]
        public List<CDAPOChild> Childs { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.CDAPOMasterID > 0;

        #endregion Additional Columns
        public CDAPOMaster()
        {
            RevisionNo = 0;
            CurrencyID = 0;
            QuotationRefNo = "";
            TypeOfLCID = 0;
            CalculationofTenure = 0;
            CreditDays = 0;
            CountryOfOriginID = 0;
            ShippingTolerance = 0m;
            PortofLoadingID = 0;
            PortofDischargeID = 0;
            ShipmentModeID = 0;
            ProposedBy = 0;
            UnApprove = false;
            UnApproveBy = 0;
            Approved = false;
            ApprovedBy = 0;
            QualityApprovalProcedureID = 0;
            PODate = DateTime.Now;
            DeliveryStartDate = DateTime.Now;
            DeliveryEndDate = DateTime.Now;
            PONo = "**<<New>>**"; 
            Proposed = false; 
            OfferValidity = 0;
            DateAdded = DateTime.Now;
            Childs = new List<CDAPOChild>();
        }
    }
   
}


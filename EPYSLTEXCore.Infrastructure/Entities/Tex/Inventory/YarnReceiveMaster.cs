using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex
{
    [Table(TableNames.YARN_RECEIVE_MASTER)]
    public class YarnReceiveMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int ReceiveID { get; set; } = 0;
        public DateTime ReceiveDate { get; set; } = DateTime.Now;
        public string ReceiveNo { get; set; } = AppConstants.NEW;
        public string MRIRNo { get; set; } = "";
        public string GRNNo { get; set; } = "";
        public DateTime? GRNDate { get; set; }
        public int LocationID { get; set; } = 0;
        public int RCompanyID { get; set; } = 0;
        public int OCompanyID { get; set; } = 0;
        public int SupplierID { get; set; } = 0;
        public int ModeID { get; set; } = 0;
        public bool IsBBLC { get; set; } = false;
        public int DocID { get; set; } = 0;
        public int CurrencyID { get; set; } = 0;
        public decimal CurrencyFactor { get; set; } = 0;
        public string RequisitonNo { get; set; } = "";
        public DateTime? RequisitonDate { get; set; }
        public string PONo { get; set; } = "";
        public DateTime? PODate { get; set; }
        public string LCNo { get; set; } = "";
        public DateTime? LCDate { get; set; }
        public decimal Tolerance { get; set; } = 0;
        public string InvoiceNo { get; set; } = "";
        public DateTime? InvoiceDate { get; set; }
        public string BLNo { get; set; } = "";
        public DateTime? BLDate { get; set; }
        public int BankBranchID { get; set; } = 0;
        public string PLNo { get; set; } = "";
        public DateTime? PLDate { get; set; }
        public string ChallanNo { get; set; } = "";
        public DateTime? ChallanDate { get; set; } = DateTime.Now;
        public string MChallanNo { get; set; } = "";
        public DateTime? MChallanDate { get; set; }
        public string GPNo { get; set; } = "";
        public DateTime? GPDate { get; set; }
        public int TransportMode { get; set; } = 0;
        public int TransportTypeID { get; set; } = 0;
        public int CContractorID { get; set; } = 0;
        public string VehicalNo { get; set; } = "";
        public int ShipmentCarrierID { get; set; } = 0;
        public int ShipmentStatus { get; set; } = 0;
        public string Remarks { get; set; } = "";
        public int RTypeID { get; set; } = 0;
        public int ITStatusID { get; set; } = 0;
        public bool InspSend { get; set; } = false;
        public int InspBy { get; set; } = 0;
        public DateTime? InspDate { get; set; }
        public bool QCSend { get; set; } = false;
        public int QCBy { get; set; } = 0;
        public DateTime? QCDate { get; set; }
        public string QCRemarks { get; set; } = "";
        public int ReasonID { get; set; } = 0;
        public bool HasQuery { get; set; } = false;
        public bool SignIn { get; set; } = false;
        public int SignInBy { get; set; } = 0;
        public DateTime? SignInDate { get; set; }
        public string ACompanyInvoice { get; set; } = "";
        public int PDID { get; set; } = 0;
        public int SpinnerID { get; set; } = 0;
        public bool PartialAllocation { get; set; } = false;
        public bool CompleteAllocation { get; set; } = false;
        public int AddedBy { get; set; } = 0;
        public int UpdatedBy { get; set; } = 0;
        public int POID { get; set; } = 0;
        public int CIID { get; set; } = 0;
        public int LCID { get; set; } = 0;
        public int ReceivedByID { get; set; } = 0;
        public string GPTime { get; set; } = "";
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public DateTime? DateUpdated { get; set; }
        public bool IsCDA { get; set; } = false;
        public bool IsSampleYarn { get; set; } = false;
        public string TruckChallanNo { get; set; } = "";
        public DateTime? TruckChallanDate { get; set; }

        public bool IsSendForApprove { get; set; } = false;
        public bool IsApproved { get; set; } = false;
        public int ApprovedBy { get; set; } = 0;
        public DateTime? ApprovedDate { get; set; }

        public string MushakNo { get; set; } = "";
        public DateTime? MushakDate { get; set; } = null;
        public string BillEntryNo { get; set; } = "";

        #region Additional Columns

        [Write(false)]
        public List<Select2OptionModel> TransportTypeList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> TransportModeList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> TransportAgencyList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> ShipmentStatusList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> LocationList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> SpinnerList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> CContractorList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> RCompanyList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> SupplierList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> BuyerList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModelExtended> SpinnerWisePackingList { get; set; } = new List<Select2OptionModelExtended>();
        [Write(false)]
        public List<int> IgnoreValidationPOIds { get; set; } = new List<int>();
        [Write(false)]
        public string CINo { get; set; } = "";

        [Write(false)]
        public DateTime CIDate { get; set; }

        [Write(false)]
        public decimal CIValue { get; set; } = 0;
        [Write(false)]
        public string YarnReceiveType { get; set; } = "";
        [Write(false)]
        public string SupplierName { get; set; } = "";

        [Write(false)]
        public string BranchName { get; set; } = "";

        [Write(false)]
        public string SpinnerName { get; set; } = "";

        [Write(false)]
        public string CustomerName { get; set; } = "";

        [Write(false)]
        public decimal LCQty { get; set; } = 0;

        [Write(false)]
        public decimal LCValue { get; set; } = 0;

        [Write(false)]
        public string RCompany { get; set; } = "";

        [Write(false)]
        public string OCompany { get; set; } = "";
        [Write(false)]
        public string PINo { get; set; } = "";
        [Write(false)]
        public string BankBranchName { get; set; } = "";
        [Write(false)]
        public string TransportModeName { get; set; } = "";
        [Write(false)]
        public string TransportTypeName { get; set; } = "";
        [Write(false)]
        public string TransportAgencyName { get; set; } = "";
        [Write(false)]
        public string ShipmentStatusName { get; set; } = "";
        [Write(false)]
        public string LocationName { get; set; } = "";
        [Write(false)]
        public string ReceivedBy { get; set; } = "";
        [Write(false)]
        public string QuotationRefNo { get; set; } = "";

        [Write(false)]
        public string ApprovedByName { get; set; } = "";
        [Write(false)]
        public List<Select2OptionModel> RackList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> BinList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> EmployeeList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<YarnReceiveChild> YarnReceiveChilds { get; set; } = new List<YarnReceiveChild>();
        [Write(false)]
        public List<Select2OptionModel> ReceiveForList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || ReceiveID > 0;

        #endregion Additional Columns
    }
}
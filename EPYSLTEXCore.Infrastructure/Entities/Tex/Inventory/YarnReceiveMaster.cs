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
        public int ReceiveID { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string ReceiveNo { get; set; }
        public string MRIRNo { get; set; }
        public string GRNNo { get; set; }
        public DateTime? GRNDate { get; set; }
        public int LocationID { get; set; }
        public int? RCompanyID { get; set; }
        public int? OCompanyID { get; set; }
        public int SupplierID { get; set; }
        public int ModeID { get; set; }
        public bool? IsBBLC { get; set; }
        public int DocID { get; set; }
        public int CurrencyID { get; set; }
        public decimal CurrencyFactor { get; set; }
        public string RequisitonNo { get; set; }
        public DateTime? RequisitonDate { get; set; }
        public string PONo { get; set; }
        public DateTime? PODate { get; set; }
        public string LCNo { get; set; }
        public DateTime? LCDate { get; set; }
        public decimal Tolerance { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string BLNo { get; set; }
        public DateTime? BLDate { get; set; }
        public int BankBranchID { get; set; }
        public string PLNo { get; set; }
        public DateTime? PLDate { get; set; }
        public string ChallanNo { get; set; }
        public DateTime? ChallanDate { get; set; }
        public string MChallanNo { get; set; }
        public DateTime? MChallanDate { get; set; }
        public string GPNo { get; set; }
        public DateTime? GPDate { get; set; }
        public int TransportMode { get; set; }
        public int TransportTypeID { get; set; }
        public int CContractorID { get; set; }
        public string VehicalNo { get; set; }
        public int ShipmentCarrierID { get; set; }
        public int ShipmentStatus { get; set; }
        public string Remarks { get; set; }
        public int RTypeID { get; set; }
        public int ITStatusID { get; set; }
        public bool InspSend { get; set; }
        public int InspBy { get; set; }
        public DateTime? InspDate { get; set; }
        public bool QCSend { get; set; }
        public int QCBy { get; set; }
        public DateTime? QCDate { get; set; }
        public string QCRemarks { get; set; }
        public int ReasonID { get; set; }
        public bool HasQuery { get; set; }
        public bool SignIn { get; set; }
        public int? SignInBy { get; set; }
        public DateTime? SignInDate { get; set; }
        public string ACompanyInvoice { get; set; }
        public int PDID { get; set; }
        public int SpinnerID { get; set; }
        public bool PartialAllocation { get; set; }
        public bool CompleteAllocation { get; set; }
        public int AddedBy { get; set; }
        public int UpdatedBy { get; set; }
        public int POID { get; set; }
        public int CIID { get; set; }
        public int LCID { get; set; }
        public int ReceivedByID { get; set; }
        public string GPTime { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool IsCDA { get; set; }
        public bool IsSampleYarn { get; set; }
        public string TruckChallanNo { get; set; }
        public DateTime? TruckChallanDate { get; set; }

        public bool IsSendForApprove { get; set; }
        public bool IsApproved { get; set; }
        public int ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public string MushakNo { get; set; } = "";
        public DateTime? MushakDate { get; set; } = null;
        public string BillEntryNo { get; set; } = "";

        #region Additional Columns

        [Write(false)]
        public List<Select2OptionModel> TransportTypeList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> TransportModeList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> TransportAgencyList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> ShipmentStatusList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> LocationList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> SpinnerList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> CContractorList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> RCompanyList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> SupplierList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> BuyerList { get; set; }
        [Write(false)]
        public List<Select2OptionModelExtended> SpinnerWisePackingList { get; set; }
        [Write(false)]
        public List<int> IgnoreValidationPOIds { get; set; }
        [Write(false)]
        public string CINo { get; set; }

        [Write(false)]
        public DateTime CIDate { get; set; }

        [Write(false)]
        public decimal CIValue { get; set; }
        [Write(false)]
        public string YarnReceiveType { get; set; }
        [Write(false)]
        public string SupplierName { get; set; }

        [Write(false)]
        public string BranchName { get; set; }

        [Write(false)]
        public string SpinnerName { get; set; }

        [Write(false)]
        public string CustomerName { get; set; }

        [Write(false)]
        public decimal LCQty { get; set; }

        [Write(false)]
        public decimal LCValue { get; set; }

        [Write(false)]
        public string RCompany { get; set; }

        [Write(false)]
        public string OCompany { get; set; }
        [Write(false)]
        public string PINo { get; set; }
        [Write(false)]
        public string BankBranchName { get; set; }
        [Write(false)]
        public string TransportModeName { get; set; }
        [Write(false)]
        public string TransportTypeName { get; set; }
        [Write(false)]
        public string TransportAgencyName { get; set; }
        [Write(false)]
        public string ShipmentStatusName { get; set; }
        [Write(false)]
        public string LocationName { get; set; }
        [Write(false)]
        public string ReceivedBy { get; set; }
        [Write(false)]
        public string QuotationRefNo { get; set; }

        [Write(false)]
        public string ApprovedByName { get; set; }

        [Write(false)]
        public List<Select2OptionModel> RackList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> BinList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> EmployeeList { get; set; }

        [Write(false)]
        public List<YarnReceiveChild> YarnReceiveChilds { get; set; }
        [Write(false)]
        public List<Select2OptionModel> ReceiveForList { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || ReceiveID > 0;

        #endregion Additional Columns

        public YarnReceiveMaster()
        {
            AddedBy = 0;
            DateAdded = DateTime.Now;
            ModeID = 0;
            IsBBLC = false;
            DocID = 0;
            CurrencyID = 0;
            CurrencyFactor = 0m;
            Tolerance = 0m;
            BankBranchID = 0;
            TransportMode = 0;
            TransportTypeID = 0;
            CContractorID = 0;
            ShipmentCarrierID = 0;
            ShipmentStatus = 0;
            RTypeID = 0;
            ITStatusID = 0;
            InspSend = false;
            InspBy = 0;
            QCSend = false;
            QCBy = 0;
            ReasonID = 0;
            UpdatedBy = 0;
            HasQuery = false;
            SignIn = false;
            SignInBy = 0;
            PDID = 0;
            SpinnerID = 0;
            YarnReceiveType = "";
            PINo = "";
            PartialAllocation = false;
            CompleteAllocation = false;
            IsSampleYarn = false;
            TruckChallanNo = "";
            IsSendForApprove = false;
            IsApproved = false;
            ApprovedBy = 0;
            YarnReceiveChilds = new List<YarnReceiveChild>();
            ReceiveDate = DateTime.Now;
            ChallanDate = DateTime.Now;
            ReceiveNo = AppConstants.NEW;
            TransportMode = 0;
            TransportTypeList = new List<Select2OptionModel>();
            TransportModeList = new List<Select2OptionModel>();
            TransportAgencyList = new List<Select2OptionModel>();
            ShipmentStatusList = new List<Select2OptionModel>();
            LocationList = new List<Select2OptionModel>();
            SpinnerList = new List<Select2OptionModel>();
            CContractorList = new List<Select2OptionModel>();
            RackList = new List<Select2OptionModel>();
            BinList = new List<Select2OptionModel>();
            EmployeeList = new List<Select2OptionModel>();
            BuyerList = new List<Select2OptionModel>();
            IgnoreValidationPOIds = new List<int>();
        }
    }
}

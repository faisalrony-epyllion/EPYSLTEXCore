using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDALoanIssueMaster")]
	public class CDALoanIssueMaster : DapperBaseEntity
	{
		#region Table properties

		[ExplicitKey]
		public int CDALIssueMasterID { get; set; }

		public DateTime LIssueDate { get; set; }

		public string LIssueNo { get; set; }

		public int LocationID { get; set; }

		public int? CompanyID { get; set; }

		public int? IssueFromCompanyID { get; set; }

		public int LoanIssueToID { get; set; }

		public int TransportMode { get; set; }

		public int TransportTypeID { get; set; }

		public string ChallanNo { get; set; }

		public DateTime? ChallanDate { get; set; }

		public string GPNo { get; set; }

		public DateTime? GPDate { get; set; }

		public int VehichleID { get; set; }

		public string VehichleNo { get; set; }

		public string DriverName { get; set; }

		public string DriverContactNo { get; set; }

		public string LockNo { get; set; }

		public string Remarks { get; set; }

		public bool DCSendForApproval { get; set; }

		public int DCPrepareBy { get; set; }

		public DateTime? DCPrepareDate { get; set; }

		public bool DCApprove { get; set; }

		public int DCApproveBy { get; set; }

		public DateTime? DCApproveDate { get; set; }

		public bool GPSendForApproval { get; set; }

		public int GPPrepareBy { get; set; }

		public DateTime? GPPrepareDate { get; set; }

		public bool GPApprove { get; set; }

		public int GPApproveBy { get; set; }

		public DateTime? GPApproveDate { get; set; }

		public bool DCCheckOut { get; set; }

		public int DCCheckOutBy { get; set; }

		public DateTime? DCCheckOutDate { get; set; }

		public string EstimatedReleaseTime { get; set; }

		public bool DCRelease { get; set; }

		public int DCReleaseBy { get; set; }

		public DateTime? DCReleaseDate { get; set; }

		public string DCReleaseNo { get; set; }

		public string MushakChallanNo { get; set; }

		public int AddedBy { get; set; }

		public int? UpdatedBy { get; set; }

		public DateTime DateAdded { get; set; }

		public DateTime? DateUpdated { get; set; }

		#endregion Table properties

		#region Additional Columns
		[Write(false)]
		public List<CDALoanIssueChild> Childs { get; set; }

		[Write(false)]
		public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || CDALIssueMasterID > 0;

		[Write(false)]
		public IEnumerable<Select2OptionModel> TransportModeList { get; set; }
		[Write(false)]
		public IEnumerable<Select2OptionModel> TransportTypeList { get; set; }
		[Write(false)]
		public IEnumerable<Select2OptionModel> CompanyList { get; set; }
		[Write(false)]
		public IEnumerable<Select2OptionModel> IssueFromCompanyList { get; set; }
		[Write(false)]
		public IEnumerable<Select2OptionModel> LocationList { get; set; }
		[Write(false)]
		public IEnumerable<Select2OptionModel> LoanProviderList { get; set; }
		[Write(false)]
		public IEnumerable<Select2OptionModel> VehichleList { get; set; }
		[Write(false)]
		public IEnumerable<Select2OptionModel> LoanIssueToList { get; set; }
		 

		[Write(false)]
		public string LocationName { get; set; }
		[Write(false)]
		public string CompanyName { get; set; }
		[Write(false)]
		public bool GPFlag { get; set; }
		#endregion Additional Columns

		public CDALoanIssueMaster()
		{
			LIssueNo = AppConstants.NEW;
			LIssueDate = DateTime.Now;
			LocationID = 0;
			CompanyID = 0;
			IssueFromCompanyID = 0;
			//LoanProviderID = 0;
			TransportMode = 0;
			TransportTypeID = 0;
			ChallanNo = AppConstants.NEW;
			ChallanDate = DateTime.Now;
			GPNo = AppConstants.NEW;
			GPDate = DateTime.Now;
			VehichleID = 0;
			DCSendForApproval = false;
			DCPrepareBy = 0;
			DCApprove = false;
			DCApproveBy = 0;
			GPSendForApproval = false;
			GPPrepareBy = 0;
			GPApprove = false;
			GPApproveBy = 0;
			DCCheckOut = false;
			DCCheckOutBy = 0;
			DCRelease = false;
			DCReleaseBy = 0;
			AddedBy = 0;
			UpdatedBy = 0;
			Childs = new List<CDALoanIssueChild>(); 
		} 
	}

}

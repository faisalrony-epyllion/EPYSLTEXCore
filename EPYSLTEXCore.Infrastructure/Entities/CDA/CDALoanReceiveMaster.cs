using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDALoanReceiveMaster")]
	public class CDALoanReceiveMaster : DapperBaseEntity
	{
		#region Table properties

		[ExplicitKey]
		public int CDALReceiveMasterID { get; set; }

		public DateTime LReceiveDate { get; set; }

		public string LReceiveNo { get; set; }

		public int LocationID { get; set; }

		public int? CompanyID { get; set; }

		public int? RCompanyID { get; set; }

		public int LoanProviderID { get; set; }

		public string ChallanNo { get; set; }

		public DateTime? ChallanDate { get; set; }

		public string MChallanNo { get; set; }

		public DateTime? MChallanDate { get; set; }

		public string GPNo { get; set; }

		public DateTime? GPDate { get; set; }

		public string GPTime { get; set; }

		public int TransportMode { get; set; }

		public int TransportTypeID { get; set; }

		public int CContractorID { get; set; }

		public string VehicleNo { get; set; }

		public string Remarks { get; set; }

		public int RTypeID { get; set; }

		public int ITStatusID { get; set; }

		public bool InspSend { get; set; }

		public DateTime? InspSendDate { get; set; }

		public bool InspAck { get; set; }

		public int InspAckBy { get; set; }

		public DateTime? InspAckDate { get; set; }

		public int InspBy { get; set; }

		public DateTime? InspDate { get; set; }

		public string ACompanyInvoice { get; set; }

		public int PDID { get; set; }

		public int AddedBy { get; set; }

		public int? UpdatedBy { get; set; }

		public DateTime DateAdded { get; set; }

		public DateTime? DateUpdated { get; set; }

		#endregion Table properties

		#region Additional Columns

		[Write(false)]
		public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || CDALReceiveMasterID > 0;
		
		[Write(false)]
		public List<CDALoanReceiveChild> Childs { get; set; }

		[Write(false)]
		public IEnumerable<Select2OptionModel> TransportModeList { get; set; }
		[Write(false)]
		public IEnumerable<Select2OptionModel> TransportTypeList { get; set; }
		[Write(false)]
		public IEnumerable<Select2OptionModel> CompanyList { get; set; }
		[Write(false)]
		public IEnumerable<Select2OptionModel> RCompanyList { get; set; }
		[Write(false)]
		public IEnumerable<Select2OptionModel> LocationList { get; set; }
		[Write(false)]
		public IEnumerable<Select2OptionModel> LoanProviderList { get; set; }
		[Write(false)] 
		public string LocationName { get; set; }
		[Write(false)]
		public string CompanyName { get; set; }

		#endregion Additional Columns

		public CDALoanReceiveMaster()
		{
			TransportMode = 0;
			TransportTypeID = 0;
			CContractorID = 0;
			RTypeID = 0;
			ITStatusID = 0;
			InspSend = false;
			InspBy = 0;
			InspAck = false;
			InspAckBy = 0;
			InspBy = 0; 
			LReceiveDate = DateTime.Now;
			ChallanDate = DateTime.Now;
			MChallanDate = DateTime.Now;
			GPDate = DateTime.Now;
			LReceiveNo = AppConstants.NEW;
			Childs = new List<CDALoanReceiveChild>();
		} 
	}
	
}

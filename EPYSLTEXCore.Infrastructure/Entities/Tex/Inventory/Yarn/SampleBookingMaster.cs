using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.SAMPLE_BOOKING_MASTER)]
    public class SampleBookingMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int BookingID { get; set; }

        public string BookingNo { get; set; }

        public string SLNo { get; set; }

        public DateTime BookingDate { get; set; }

        public bool RevisionNeed { get; set; }

        public int RevisionNo { get; set; }

        public DateTime? RevisionDate { get; set; }

        public int BuyerID { get; set; }
        public int BuyerTeamID { get; set; }
        public int MerchandiserTeamID { get; set; }
        public int ExecutionCompanyID { get; set; }
        public int? SupplierID { get; set; }

        public int SampleID { get; set; }

        public int StyleMasterID { get; set; }

        public string StyleNo { get; set; }

        public int SubGroupID { get; set; }
        public int OrderQty { get; set; }

        public bool OwnerFactory { get; set; }

        public DateTime? FirstInHouseDate { get; set; }
        public DateTime? InHouseDate { get; set; }
        public bool SwatchAttached { get; set; }

        public bool SwatchReceive { get; set; }

        public DateTime? SwatchReceiveDate { get; set; }

        public bool Acknowledge { get; set; }
        public int AcknowledgedBy { get; set; }
        public DateTime? AcknowledgeDate { get; set; }
        public bool UnAcknowledge { get; set; }
        public string UnAcknowledgeReason { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        public bool LabdipAcknowledge { get; set; }
        public int LabdipAcknowledgeBY { get; set; }
        public DateTime? LabdipAcknowledgeDate { get; set; }
        public bool LabdipUnAcknowledge { get; set; }
        public int LabdipUnAcknowledgeBY { get; set; }
        public DateTime? LabdipUnAcknowledgeDate { get; set; }
        public int LabdipRevisionNo { get; set; }
        public string LabdipUnAcknowledgeReason { get; set; }
        public int ExportOrderID { get; set; }
        public bool PricePropose { get; set; } = false;
        public int PriceProposeNo { get; set; } = 0;
        public bool PriceAgree { get; set; } = false;
        public bool PriceSuggest { get; set; } = false;
        public bool PriceReSuggest { get; set; } = false;
        public DateTime? PriceReSuggestDate { get; set; }
        public int PriceReSuggestBy { get; set; } = 0;
        public DateTime? FirstPProposeDate { get; set; }
        public DateTime? PriceProposeDate { get; set; }
        public DateTime? PriceAgreeDate { get; set; }
        public int PriceAgreeBy { get; set; } = 0;
        public DateTime? PriceReAgreeDate { get; set; }
        public int PriceReAgreeBy { get; set; } = 0;
        public DateTime? PriceSuggestDate { get; set; }
        public int PriceSuggestBy { get; set; } = 0;
        public string ReProposeAcceptReason { get; set; } = "";
        public int PriceReProposeReasonID { get; set; } = 0;


        #region Additional Columns        

        [Write(false)]
        public string Remarks { get; set; }
        [Write(false)]
        public string SupplierName { get; set; }
        [Write(false)]
        public string ExportOrderNo { get; set; }
        [Write(false)]
        public int SeasonID { get; set; }
        [Write(false)]
        public String BuyerName { get; set; }
        [Write(false)]
        public int ItemGroupID { get; set; }
        [Write(false)]
        public String BuyerTeamName { get; set; }
        [Write(false)]
        public List<SampleBookingConsumption> Childs { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.BookingID > 0;


        #endregion Additional Columns

        public SampleBookingMaster()
        {
            LabdipAcknowledge = false;
            LabdipAcknowledgeBY = 0;
            LabdipRevisionNo = 0;
            ExportOrderID = 0;
            SeasonID = 0;
            Childs = new List<SampleBookingConsumption>();
            ExportOrderNo = "";
            SupplierName = "";


            //RevisionNo = '0';
            //CompanyID = 0;
            ////BookingByName = UserId;
            //DateAdded = DateTime.Now;
            //PYBookingDate = DateTime.Now;
            //RevisionDate = DateTime.Now;
            //RequiredDate = DateTime.Now;
            //PYBookingNo = AppConstants.NEW;
            //ProjectionYarnBookingItemChilds = new List<ProjectionYarnBookingItemChild>();
            //PYBItemChildDetails = new List<ProjectionYarnBookingItemChildDetails>();
        }
    }

}

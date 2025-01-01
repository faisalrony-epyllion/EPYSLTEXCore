using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using System;
using System.Collections.Generic;

namespace EPYSLTEX.Core.Entities.Tex
{
    [Table("CDAFloorReceiveMaster")]
    public class CDAFloorReceiveMaster : DapperBaseEntity
    {
        public CDAFloorReceiveMaster()
        {
            FloorReceiveDate = DateTime.Now;
            FloorReceiveNo = AppConstants.NEW;
            Childs = new List<CDAFloorReceiveChild>();
        }

        [ExplicitKey]
        public int FloorReceiveMasterID { get; set; }

        public int SupplierID { get; set; }

        public string FloorReceiveNo { get; set; }

        public int FloorReceivedBy { get; set; }

        public System.DateTime FloorReceiveDate { get; set; }

        public int FloorReqMasterID { get; set; }

        public int FloorIssueMasterID { get; set; }

        public int SubGroupID { get; set; }

        public int AddedBy { get; set; }

        public System.DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public System.DateTime? DateUpdated { get; set; }

        public int CompanyID { get; set; }

        public string Remarks { get; set; }

        #region Additional

        [Write(false)]
        public List<CDAFloorReceiveChild> Childs { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || FloorReceiveMasterID > 0;

        [Write(false)]
        public string FloorReceivedByUser { get; set; }

        [Write(false)]
        public string FloorIssueNo { get; set; }

        [Write(false)]
        public DateTime FloorIssueDate { get; set; }

        [Write(false)]
        public string FloorIssueByUser { get; set; }

        [Write(false)]
        public int FloorReqMasterId { get; set; }

        [Write(false)]
        public string FloorReqByUser { get; set; }

        [Write(false)]
        public string FloorReqNo { get; set; }

        [Write(false)]
        public DateTime FloorReqDate { get; set; }

        [Write(false)]
        public string FloorReqFor { get; set; }

        [Write(false)]
        public int ReqQty { get; set; }

        [Write(false)]
        public int IssueQty { get; set; }

        [Write(false)]
        public int ReceiveQty { get; set; }

        #endregion Additional
    }
}
using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn
{
    [Table(TableNames.YARN_QC_RECEIVE_MASTER)]
    public class YarnQCReceiveMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCReceiveMasterID { get; set; }
        ///<summary>
        /// QCReceiveNo (length: 50)
        ///</summary>
        public string QCReceiveNo { get; set; }

        ///<summary>
        /// QCReceivedBy
        ///</summary>
        public int QCReceivedBy { get; set; }

        ///<summary>
        /// QCReceiveDate
        ///</summary>
        public DateTime QCReceiveDate { get; set; }

        ///<summary>
        /// QCReqMasterID
        ///</summary>
        public int QCReqMasterId { get; set; }

        ///<summary>
        /// QCIssueMasterID
        ///</summary>
        public int QCIssueMasterId { get; set; }

        ///<summary>
        /// LocationID
        ///</summary>
        public int LocationId { get; set; }

        /// <summary>
        /// Receive ID
        /// </summary>
        public int ReceiveID { get; set; }

        ///<summary>
        /// CompanyID
        ///</summary>
        public int CompanyId { get; set; }

        ///<summary>
        /// RCompanyID
        ///</summary>
        public int RCompanyId { get; set; }

        ///<summary>
        /// SupplierID
        ///</summary>
        public int SupplierId { get; set; }

        ///<summary>
        /// SpinnerId
        ///</summary>
        public int SpinnerId { get; set; }

        ///<summary>
        /// AddedBy
        ///</summary>
        public int AddedBy { get; set; }

        ///<summary>
        /// DateAdded
        ///</summary>
        public DateTime DateAdded { get; set; }

        ///<summary>
        /// UpdatedBy
        ///</summary>
        public int? UpdatedBy { get; set; }

        ///<summary>
        /// DateUpdated
        ///</summary>
        public DateTime? DateUpdated { get; set; }

        #region AdditionalFields

        [Write(false)]
        public string QCReceivedByUser { get; set; }
        [Write(false)]
        public string QCIssueNo { get; set; }
        [Write(false)]
        public DateTime QCIssueDate { get; set; }
        [Write(false)]
        public string QCIssueByUser { get; set; }
        [Write(false)]
        public string QCReqByUser { get; set; }
        [Write(false)]
        public string QCReqNo { get; set; }
        [Write(false)]
        public DateTime QCReqDate { get; set; }
        [Write(false)]
        public string QCReqFor { get; set; }
        [Write(false)]
        public decimal ReqQtyCone { get; set; }
        [Write(false)]
        public decimal IssueQtyCone { get; set; }
        [Write(false)]
        public int IssueQtyCarton { get; set; }
        [Write(false)]
        public int ReceiveQtyCone { get; set; }
        [Write(false)]
        public int ReceiveQtyCarton { get; set; }
        [Write(false)]
        public string ReceiveNo { get; set; }
        [Write(false)]
        public DateTime? ReceiveDate { get; set; }
        [Write(false)]
        public string Supplier { get; set; }
        [Write(false)]
        public string Spinner { get; set; }
        [Write(false)]
        public string ChallanNo { get; set; }

        [Write(false)]
        public string Status { get; set; }

        [Write(false)]
        public DateTime? ChallanDate { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.QCReceiveMasterID > 0;
        [Write(false)]
        public List<YarnQCReceiveChild> YarnQCReceiveChilds { get; set; }
        [Write(false)]
        public List<Select2OptionModel> TechnicalNameList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> MCTypeForFabricList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> BuyerList { get; set; }

        #endregion Additional Columns

        public YarnQCReceiveMaster()
        {
            DateAdded = DateTime.Now;
            YarnQCReceiveChilds = new List<YarnQCReceiveChild>();
            QCReceiveDate = DateTime.Now;
            QCReceiveNo = AppConstants.NEW;
            TechnicalNameList = new List<Select2OptionModel>();
            MCTypeForFabricList = new List<Select2OptionModel>();
            BuyerList = new List<Select2OptionModel>();
        }
    }
}

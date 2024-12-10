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
    [Table(TableNames.YARN_MRIR_MASTER)]
    public class YarnMRIRMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int MRIRMasterId { get; set; }
        public string MRIRNo { get; set; }
        public int MRIRBy { get; set; }
        public DateTime MRIRDate { get; set; }
        public string GRNNo { get; set; }
        public int GRNBy { get; set; }
        public DateTime GRNDate { get; set; }
        public string MRNNo { get; set; }
        public int MRNBy { get; set; }
        public DateTime MRNDate { get; set; }
        public string ReceiveNo { get; set; }
        public string ChallanNo { get; set; }
        public int CompanyId { get; set; }
        public int RCompanyId { get; set; }
        public int SupplierId { get; set; }
        public int SpinnerId { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int ReceiveNoteType { get; set; }
        public bool Returned { get; set; }
        public int ReturnedBy { get; set; }
        public DateTime ReturnedDate { get; set; }
        public bool ReTest { get; set; }
        public int ReTestBy { get; set; }
        public DateTime ReTestDate { get; set; }
        public string ReTestReason { get; set; }

        #region Additional Columns

        [Write(false)]
        public List<YarnMRIRChild> YarnMRIRChilds { get; set; }

        [Write(false)]
        public string QCRemarksNo { get; set; }
        [Write(false)]
        public DateTime QCRemarksDate { get; set; }
        [Write(false)]
        public string QCRemarksByUser { get; set; }
        [Write(false)]
        public string MRIRByUser { get; set; }
        [Write(false)]
        public string QCReqFor { get; set; }
        [Write(false)]
        public int ReceiveQty { get; set; }
        [Write(false)]
        public int ReceiveQtyCone { get; set; }
        [Write(false)]
        public int ReceiveQtyCarton { get; set; }
        [Write(false)]
        public string Supplier { get; set; }
        [Write(false)]
        public string Spinner { get; set; }
        [Write(false)]
        public string PONo { get; set; }
        [Write(false)]
        public int POQty { get; set; }
        [Write(false)]
        public string InvoiceNo { get; set; }
        [Write(false)]
        public string VehicalNo { get; set; }
        [Write(false)]
        public string POUnit { get; set; }
        [Write(false)]
        public int ReceiveNoteQty { get; set; }
        [Write(false)]
        public List<YarnMRIRChild> Childs { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.MRIRMasterId > 0;

        #endregion Additional Columns

        public YarnMRIRMaster()
        {
            AddedBy = 0;
            DateAdded = DateTime.Now;
            MRIRBy = 0;
            GRNBy = 0;
            MRNBy = 0;
            UpdatedBy = 0;
            DateUpdated = DateTime.Now;
            YarnMRIRChilds = new List<YarnMRIRChild>();
            MRIRDate = DateTime.Now;
            GRNDate = DateTime.Now;
            MRNDate = DateTime.Now;
            QCRemarksDate = DateTime.Now;
            ReturnedDate = DateTime.Now;
            ReTestDate = DateTime.Now;
            //MRIRNo = "";
            //GRNNo = "";
            //MRNNo = "";
            ReceiveNo = "";
            ChallanNo = "";
            CompanyId = 0;
            RCompanyId = 0;
            SupplierId = 0;
            SpinnerId = 0;
            Childs = new List<YarnMRIRChild>();
            ReceiveQty = 0;
            ReceiveQtyCone = 0;
            ReceiveQtyCarton = 0;
        }
    }
}

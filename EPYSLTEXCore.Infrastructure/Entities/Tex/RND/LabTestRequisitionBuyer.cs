using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{

    [Table(TableNames.LAB_TEST_REQUISITION_BUYER)]
    public class LabTestRequisitionBuyer : DapperBaseEntity
    {
        public LabTestRequisitionBuyer()
        { 
            LTResultUpdate = false; 
            LTResultUpdateBy = 0;
            LabTestRequisitionBuyerParameters = new List<LabTestRequisitionBuyerParameter>();
            NewLabTestRequisitionBuyerParameters = new List<LabTestRequisitionBuyerParameter>();
            IsParameterPass = false;
            ParameterStatus = "";
            ParameterRemarks = "";
        }

        [ExplicitKey]
        public int LTReqBuyerID { get; set; }

        public int BuyerID { get; set; }

        public int LTReqMasterID { get; set; }

        public bool LTResultUpdate { get; set; }

        public int LTResultUpdateBy { get; set; }

        public System.DateTime? LTResultUpdateDate { get; set; }

        public bool IsApproved { get; set; }

        public int ApprovedBy { get; set; }

        public System.DateTime? ApprovedDate { get; set; }

        public bool IsSend { get; set; }

        public int SendBy { get; set; }

        public System.DateTime? SendDate { get; set; }

        public bool IsAcknowledge { get; set; }

        public int AcknowledgeBy { get; set; }

        public System.DateTime? AcknowledgeDate { get; set; }

        public bool IsPass { get; set; }

        public string Remarks { get; set; }

        #region Additional Property

        [Write(false)]
        public List<LabTestRequisitionBuyerParameter> LabTestRequisitionBuyerParameters { get; set; }

        [Write(false)]
        public List<LabTestRequisitionBuyerParameter> NewLabTestRequisitionBuyerParameters { get; set; }

        [Write(false)]
        public string BuyerName { get; set; }

        [Write(false)]
        public string Status { get; set; }

        [Write(false)]
        public string ImagePath { get; set; }
        [Write(false)]
        public bool IsParameterPass { get; set; }
        [Write(false)]
        public string ParameterStatus { get; set; }
        [Write(false)]
        public string ParameterRemarks { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LTReqBuyerID > 0;


        #endregion Additional Property
    }
}
using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{

    [Table(TableNames.LAB_TEST_REQUISITION_BUYER_PARAMETER)]
    public class LabTestRequisitionBuyerParameter : DapperBaseEntity
    { 
        public LabTestRequisitionBuyerParameter()
        {
            LTReqBPID = 0;
            LTReqMasterID = 0;
            LTReqBuyerID = 0;
            BuyerID = 0;
            BPID = 0;
            BPSubID = 0;

            RefValueFrom = "";
            RefValueTo = "";
            TestValue = "";
            TestValue1 = "";
            IsPass = false;
            IsParameterPass = false;
            ParameterStatus = "";
            ParameterRemarks = "";
            Addition1 = "";
            Addition2 = "";
            AdditionalInfo = "";
            BuyerName = "";
            ParameterName = "";
            TestMethod = "";
            TestName = "";
            Requirement = "";
            SubIDs = "";
            SubTestName = "";
            SubSubTestName = "";
            Requirement = "";
            Requirement1 = "";
            TestNatureID = 0;
            BuyerParameters = new List<LabTestRequisitionBuyerParameter>();
        }

        [ExplicitKey]
        public int LTReqBPID { get; set; }
        public int LTReqMasterID { get; set; }
        public int LTReqBuyerID { get; set; }
        public int BuyerID { get; set; }
        public int BPID { get; set; }
        public int BPSubID { get; set; }
        public string RefValueFrom { get; set; }
        public string RefValueTo { get; set; }
        public string TestValue { get; set; }
        public string TestValue1 { get; set; }
        public bool IsPass { get; set; }
        public string Remarks { get; set; }
        public bool IsParameterPass { get; set; }
        public string ParameterStatus { get; set; }
        public string ParameterRemarks { get; set; }
        public string Addition1 { get; set; }
        public string Addition2 { get; set; }
        public string AdditionalInfo { get; set; }
        public string Requirement { get; set; }
        public string Requirement1 { get; set; }
        public int TestNatureID { get; set; }

        #region Additional Property

        [Write(false)]
        public string BuyerName { get; set; }

        [Write(false)]
        public string ParameterName { get; set; }
        [Write(false)]
        public string TestMethod { get; set; }
        [Write(false)]
        public string TestName { get; set; }
        [Write(false)]
        public string TestNatureName { get; set; }
        [Write(false)]
        public string SubIDs { get; set; }

        [Write(false)]
        public string SubTestName { get; set; }

        [Write(false)]
        public string SubSubTestName { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LTReqBPID > 0;

        [Write(false)]
        public List<LabTestRequisitionBuyerParameter> BuyerParameters { get; set; }

        #endregion Additional Property
    }
}
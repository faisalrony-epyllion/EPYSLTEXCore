using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table("LabTestRequisitionCareLabel")]
    public class LabTestRequisitionCareLabel : DapperBaseEntity
    {
        #region Table Properties
        [ExplicitKey]
        public int LTReqCareLabelID { get; set; }
        public int LTReqMasterID { get; set; }
        public int LCareLableID { get; set; }
        public int SeqNo { get; set; }
        public string CareLableCode { get; set; }
        public string GroupCode { get; set; }
        #endregion

        #region Additional Properties
        [Write(false)]
        public string CareType { get; set; }

        [Write(false)]
        public string CareName { get; set; }

        [Write(false)]
        public string ImagePath { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LTReqCareLabelID > 0;
        #endregion

        public LabTestRequisitionCareLabel()
        {
            LTReqCareLabelID = 0;
            LTReqMasterID = 0;
            LCareLableID = 0;
            SeqNo = 0;
            CareLableCode = "";
            GroupCode = "";
        }
    }
}

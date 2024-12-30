using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{

    [Table(TableNames.LAB_TEST_REQUISITION_END_USE)]
    public class LabTestRequisitionEndUse : DapperBaseEntity
    {
        public LabTestRequisitionEndUse()
        {
            LTREndUseID = 0;
            LTReqBuyerID = 0;
            LTReqMasterID = 0;
            StyleGenderID = 0;
            SeqNo = 0;

            StyleGenderName = "";
        }

        [ExplicitKey]
        public int LTREndUseID { get; set; }
        public int LTReqBuyerID { get; set; }
        public int LTReqMasterID { get; set; }
        public int StyleGenderID { get; set; }
        public int SeqNo { get; set; }

        #region Additional Props
        [Write(false)]
        public string StyleGenderName { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LTREndUseID > 0;
        #endregion
    }
}

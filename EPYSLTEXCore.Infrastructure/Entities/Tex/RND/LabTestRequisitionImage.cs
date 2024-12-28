using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{

    [Table(TableNames.LABTEST_REQUISITION_IMAGE)]

    public class LabTestRequisitionImage : DapperBaseEntity
    {
        public LabTestRequisitionImage()
        { 
            LTReqImageID = 0;
            LTReqMasterID = 0;
            FileName = "";
            ImagePath = "";
            PreviewTemplate = "";
            ImageGroup = "";
            ImageSubGroup = "";
            BPID = 0;

            IsDelete = false;
        }

        [ExplicitKey]
        public int LTReqImageID { get; set; }
        public int LTReqMasterID { get; set; }
        public string FileName { get; set; }
        public string ImagePath { get; set; }
        public string PreviewTemplate { get; set; }
        public string ImageGroup { get; set; }
        public string ImageSubGroup { get; set; }
        public int BPID { get; set; }
        #region Additional Property
        [Write(false)]
        public bool IsDelete { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LTReqImageID > 0;


        #endregion Additional Property
    }
}
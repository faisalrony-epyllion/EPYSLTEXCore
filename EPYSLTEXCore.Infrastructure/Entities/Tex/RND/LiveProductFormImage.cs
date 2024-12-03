using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.LIVE_PRODUCT_FORM_IMAGE)]
    public class LiveProductFormImage : DapperBaseEntity
    {
        [ExplicitKey]
        public int LPFormImageID { get; set; }
        public int LPFormID { get; set; }
        public int FirmConceptMasterID { get; set; }
        public string ImagePath { get; set; }
        public string PreviewTemplate { get; set; }
        public bool DefaultImage { get; set; }

        #region Addtional Properties 
        [Write(false)]
        public string ImageName { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LPFormImageID > 0;
        #endregion Addtional Properties  
        public LiveProductFormImage()
        {
            DefaultImage = false;
        }
    }
}

using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;


namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn
{
    [Table(TableNames.YarnItemApprovalReason)]
    public class YarnItemApprovalReason : DapperBaseEntity
    {
        [ExplicitKey]
        ///<summary>
        /// ReasonID (Primary key)
        ///</summary>
        public int ReasonID { get; set; }

        ///<summary>
        /// ReasonName (length: 100)
        ///</summary>
        public string ReasonName { get; set; }

        ///<summary>
        /// NotApprove
        ///</summary>
        public bool NotApprove { get; set; }

        #region Additional Columns
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.ReasonID > 0;

        [Write(false)]
        public virtual ICollection<YarnItemPrice> YarnItemPrices { get; set; } // YarnItemPrice.FK_YarnItemPrice_YarnItemApprovalReason
        #endregion

        public YarnItemApprovalReason()
        {
            NotApprove = false;
            YarnItemPrices = new List<YarnItemPrice>();
        }
    }
}

using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YARN_DYEING_RECIPE_MASTER)]
    public class YDRecipeMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDRecipeMasterID { get; set; }

        ///<summary>
        /// YDRecipeNo (length: 50)
        ///</summary>
        public string YDRecipeNo { get; set; }

        ///<summary>
        /// YDRecipeBy
        ///</summary>
        public int YDRecipeBy { get; set; }

        ///<summary>
        /// YDRecipeDate
        ///</summary>
        public System.DateTime YDRecipeDate { get; set; }

        ///<summary>
        /// YDBookingMasterID
        ///</summary>
        public int YDBookingMasterID { get; set; }

        ///<summary>
        /// ExportOrderID
        ///</summary>
        public int ExportOrderID { get; set; }

        ///<summary>
        /// BuyerID
        ///</summary>
        public int BuyerID { get; set; }

        ///<summary>
        /// BuyerTeamID
        ///</summary>
        public int BuyerTeamID { get; set; }

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }

        ///<summary>
        /// ColorID
        ///</summary>
        public int ColorID { get; set; }

        ///<summary>
        /// RecipeFor
        ///</summary>
        public int RecipeFor { get; set; }

        ///<summary>
        /// BatchWeightKG
        ///</summary>
        public decimal BatchWeightKG { get; set; }

        ///<summary>
        /// CCColorID
        ///</summary>
        public int CCColorID { get; set; }

        ///<summary>
        /// Temperature
        ///</summary>
        public decimal Temperature { get; set; }

        ///<summary>
        /// ProcessTime
        ///</summary>
        public decimal ProcessTime { get; set; }

        ///<summary>
        /// DPID
        ///</summary>
        public int DPID { get; set; }

        ///<summary>
        /// DPProcessInfo
        ///</summary>
        public string DPProcessInfo { get; set; }

        ///<summary>
        /// Remarks (length: 500)
        ///</summary>
        public string Remarks { get; set; }

        ///<summary>
        /// IsApprove
        ///</summary>
        public bool IsApprove { get; set; }

        ///<summary>
        /// ApproveDate
        ///</summary>
        public System.DateTime? ApproveDate { get; set; }

        ///<summary>
        /// ApproveBy
        ///</summary>
        public int? ApproveBy { get; set; }

        ///<summary>
        /// IsAcknowledge
        ///</summary>
        public bool IsAcknowledge { get; set; }

        ///<summary>
        /// AcknowledgeDate
        ///</summary>
        public System.DateTime? AcknowledgeDate { get; set; }

        ///<summary>
        /// AcknowledgeBy
        ///</summary>
        public int? AcknowledgeBy { get; set; }

        ///<summary>
        /// AddedBy
        ///</summary>
        public int AddedBy { get; set; }

        ///<summary>
        /// UpdatedBy
        ///</summary>
        public int? UpdatedBy { get; set; }

        ///<summary>
        /// SendForApproval
        ///</summary>
        public bool SendForApproval { get; set; }

        ///<summary>
        /// DateAdded
        ///</summary>
        public System.DateTime DateAdded { get; set; }

        ///<summary>
        /// DateUpdated
        ///</summary>
        public System.DateTime? DateUpdated { get; set; }

        #region Additional Columns

        [Write(false)]
        public int YDBookingChildID { get; set; }

        [Write(false)]
        public string YDBookingNo { get; set; }

        [Write(false)]
        public DateTime YDBookingDate { get; set; }

        [Write(false)]
        public DateTime RecipeRequestAckDate { get; set; }

        [Write(false)]
        public string BuyerName { get; set; }

        [Write(false)]
        public string ProgramName { get; set; }

        [Write(false)]
        public string ItemName { get; set; }

        [Write(false)]
        public int ColorId { get; set; }

        [Write(false)]
        public string ColorName { get; set; }

        [Write(false)]
        public string ColorCode { get; set; }

        [Write(false)]
        public string Composition { get; set; }

        [Write(false)]
        public string YarnType { get; set; }

        [Write(false)]
        public string Process { get; set; }

        [Write(false)]
        public string SubProcess { get; set; }

        [Write(false)]
        public string QualityParameter { get; set; }

        [Write(false)]
        public string ShadeCode { get; set; }

        [Write(false)]
        public string Count { get; set; }

        [Write(false)]
        public string NoofPly { get; set; }

        [Write(false)]
        public decimal BookingQty { get; set; }

        [Write(false)]
        public string DisplayUnitDesc { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> BuyerList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ProcessList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> RecipeForList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ParticularsList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> UOMList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> RawItemList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> DPList { get; set; }

        [Write(false)]
        public List<YDRecipeChild> YDRecipeChilds { get; set; }

        [Write(false)]
        public List<YDRecipeDefinitionItemInfo> YDRecipeDefinitionItemInfoes { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDRecipeMasterID > 0;

        #endregion Additional Columns

        public YDRecipeMaster()
        {
            CCColorID = 0;
            Temperature = 0m;
            ProcessTime = 0m;
            DPID = 0;
            IsApprove = false;
            SendForApproval = false;
            YDRecipeDate = DateTime.Now;
            YDRecipeNo = AppConstants.NEW;
            YDRecipeChilds = new List<YDRecipeChild>();
            YDRecipeDefinitionItemInfoes = new List<YDRecipeDefinitionItemInfo>();
        }
    }
}

using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_PRODUCTION_MASTER)]
    public class YDProductionMaster: DapperBaseEntity
    {
        [ExplicitKey]
        public int YDProductionMasterID { get; set; } = -1;
        public int YDRecipeID { get; set; }= -1;
        public int YDDBatchID { get; set; } = 0;

        ///<summary>
        /// YDProductionNo (length: 50)
        ///</summary>
        public string YDProductionNo { get; set; } = "";

        ///<summary>
        /// YDProductionDate
        ///</summary>
        public DateTime YDProductionDate { get; set; }= DateTime.Now;

        ///<summary>
        /// YDBookingMasterID
        ///</summary>
        public int YDBookingMasterID { get; set; }= 0;

        ///<summary>
        /// BuyerID
        ///</summary>
        public int BuyerID { get; set; } = 0;

        ///<summary>
        /// Remarks (length: 500)
        ///</summary>
        public string Remarks { get; set; } = "";

        public int DMID { get; set; } = 0;

        public int OperatorID { get; set; } = 0;

        public int ShiftID { get; set; } = 0;

        public string BatchNo { get; set; } = "";

        ///<summary>
        /// IsApprove
        ///</summary>
        public bool IsApprove { get; set; } = false;

        ///<summary>
        /// ApproveDate
        ///</summary>
        public DateTime? ApproveDate { get; set; }=DateTime.Now;

        ///<summary>
        /// ApproveBy
        ///</summary>
        public int? ApproveBy { get; set; } = 0;

        ///<summary>
        /// IsAcknowledge
        ///</summary>
        public bool IsAcknowledge { get; set; }= false;

        ///<summary>
        /// AcknowledgeDate
        ///</summary>
        public DateTime? AcknowledgeDate { get; set; } = DateTime.Now;

        ///<summary>
        /// AcknowledgeBy
        ///</summary>
        public int? AcknowledgeBy { get; set; } = 0;

        ///<summary>
        /// AddedBy
        ///</summary>
        public int AddedBy { get; set; } = 0;

        ///<summary>
        /// DateAdded
        ///</summary>
        public DateTime DateAdded { get; set; } = DateTime.Now;

        ///<summary>
        /// UpdatedBy
        ///</summary>
        public int? UpdatedBy { get; set; } = 0;

        ///<summary>
        /// DateUpdated
        ///</summary>
        public DateTime? DateUpdated { get; set; } = DateTime.Now;

        #region Additional Columns
        [Write(false)]
        public string GroupConceptNo { get; set; } = "";

        [Write(false)]
        public string ConceptNo { get; set; } = "";

        [Write(false)]
        public string YDBookingNo { get; set; } = "";

        [Write(false)]
        public string YDBookingDate { get; set; } = "";

        [Write(false)]
        public string Buyer { get; set; } = "";

        [Write(false)]
        public string MCSLNo { get; set; } = "";

        [Write(false)]
        public string Operator { get; set; } = "";

        [Write(false)]
        public string Shift { get; set; } = "";

        [Write(false)]
        public string ColorName { get; set; } = "";

        [Write(false)]
        public int ColorID { get; set; }= 0;

        [Write(false)]
        public string ItemMasterID { get; set; } = "";

        [Write(false)]
        public string Segment1ValueDesc { get; set; } = "";

        [Write(false)]
        public string Segment2ValueDesc { get; set; } = "";

        [Write(false)]
        public string Segment3ValueDesc { get; set; } = "";

        [Write(false)]
        public string Segment4ValueDesc { get; set; } = "";

        [Write(false)]
        public string Segment5ValueDesc { get; set; } = "";

        [Write(false)]
        public string Segment6ValueDesc { get; set; } = "";

        [Write(false)]
        public string Segment7ValueDesc { get; set; } = "";

        [Write(false)]
        public string YDRecipeNo { get; set; } = "";

        [Write(false)]
        public string RecipeReqNo { get; set; } = "";

        [Write(false)]
        public string YDDBatchNo { get; set; } = "";

        [Write(false)]
        public List<YDProductionChild> Childs { get; set; }= new List<YDProductionChild>();

        [Write(false)]
        public List<Select2OptionModel> OperatorList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> ShiftList { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDProductionMasterID > 0;

        #endregion Additional Columns

        
    }

    //public class YDProductionMasterValidator : AbstractValidator<YDProductionMaster>
    //{
    //    public YDProductionMasterValidator()
    //    {
    //        RuleForEach(x => x.Childs).SetValidator(new YDProductionChildValidator());
    //    }
    //}
}

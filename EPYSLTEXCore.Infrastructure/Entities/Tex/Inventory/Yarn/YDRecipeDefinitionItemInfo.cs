using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YARN_DYEING_RECIPE_ITEM)]
    public class YDRecipeDefinitionItemInfo : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDRecipeItemInfoID { get; set; }

        ///<summary>
        /// YDRecipeID
        ///</summary>
        public int YDRecipeID { get; set; }
        ///<summary>
        /// YDRecipeReqChildID
        ///</summary>
        public int YDRecipeReqChildID { get; set; }

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }

        ///<summary>
        /// SubGroupID
        ///</summary>
        public int SubGroupID { get; set; }

        ///<summary>
        /// Pcs
        ///</summary>
        public int? Pcs { get; set; }

        ///<summary>
        /// BookingQty
        ///</summary>
        public decimal BookingQty { get; set; }

        ///<summary>
        /// YarnCategory
        ///</summary>
        public string YarnCategory { get; set; }

        ///<summary>
        /// NoOfThread
        ///</summary>
        public int NoOfThread { get; set; }

        ///<summary>
        /// YarnDyedColorID
        ///</summary>
        public int YarnDyedColorID { get; set; }

        ///<summary>
        /// ProgramName
        ///</summary>
        public string ProgramName { get; set; }

        ///<summary>
        /// UnitID
        ///</summary>
        public int UnitID { get; set; }

        ///<summary>
        /// ColorId
        ///</summary>
        public int ColorId { get; set; }

        ///<summary>
        /// ColorName
        ///</summary>
        public string ColorCode { get; set; }

        ///<summary>
        /// YDBookingChildID
        ///</summary>
        public int YDBookingChildID { get; set; }

        ///<summary>
        /// ShadeCode
        ///</summary>
        public string ShadeCode { get; set; }
        public int BookingID { get; set; }
        public int ConceptID { get; set; }
        public decimal Qty { get; set; }

        #region Additional Columns

        [Write(false)]
        public string YDBookingNo { get; set; }

        [Write(false)]
        public string ItemName { get; set; }

        [Write(false)]
        public string ColorName { get; set; }

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
        public string Count { get; set; }

        [Write(false)]
        public string NoofPly { get; set; }

        [Write(false)]
        public string DisplayUnitDesc { get; set; }

        [Write(false)]
        public decimal FBookingQty { get; set; }

        [Write(false)]
        public decimal SavedQty { get; set; }

        [Write(false)]
        public decimal YDRecipeQty { get; set; }

        [Write(false)]
        public int DPID { get; set; }

        [Write(false)]
        public string DPName { get; set; }

        [Write(false)]
        public string DPProcessInfo { get; set; }
        [Write(false)]
        public int YDRecipeReqMasterID { get; set; }
        [Write(false)]
        public bool RecipeOn { get; set; }
        [Write(false)]
        public string GroupConceptNo { get; set; }
        [Write(false)]
        public string ConceptNo { get; set; }
        [Write(false)]
        public DateTime ConceptDate { get; set; }
        [Write(false)]
        public string KnittingType { get; set; }
        [Write(false)]
        public string FabricComposition { get; set; }
        [Write(false)]
        public int ConstructionID { get; set; }
        [Write(false)]
        public string Construction { get; set; }
        [Write(false)]
        public string FabricGsm { get; set; }
        [Write(false)]
        public string SubGroup { get; set; }
        [Write(false)]
        public int TechnicalNameId { get; set; }
        [Write(false)]
        public string TechnicalName { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDRecipeItemInfoID > 0;

        #endregion Additional Columns
        public YDRecipeDefinitionItemInfo()
        {
            YDRecipeID = 0;
            YDRecipeReqChildID = 0;
            YDBookingChildID = 0;
            ItemMasterID = 0;
            SubGroupID = 0;
            Pcs = 0;
            BookingQty = 0;
            YarnCategory = "";
            NoOfThread = 0;
            YarnDyedColorID = 0;
            ProgramName = "";
            SubGroup = "";
            UnitID = 0;
            ColorId = 0;
            ColorCode = "";
            ShadeCode = "";
            BookingID = 0;
            ConceptID = 0;
            Qty = 0;

            YDRecipeReqMasterID = 0;
            TechnicalNameId = 0;
            YDBookingNo = "";
            GroupConceptNo = "";
            TechnicalName = "";
            ConceptNo = "";
            KnittingType = "";
            FabricComposition = "";
            Construction = "";
            FabricGsm = "";
            ConstructionID = 0;
            ConceptDate = DateTime.Now;
            ItemName = "";
            ColorName = "";
            Composition = "";
            YarnType = "";
            Process = "";
            SubProcess = "";
            QualityParameter = "";
            Count = "";
            NoofPly = "";
            DisplayUnitDesc = "";
            FBookingQty = 0;
            SavedQty = 0;
            YDRecipeQty = 0;
            DPID = 0;
            DPName = "";
            DPProcessInfo = "";
            RecipeOn = false;
        }
    }
}

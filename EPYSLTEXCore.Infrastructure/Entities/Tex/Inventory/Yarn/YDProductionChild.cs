using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_PRODUCTION_CHILD)]
    public class YDProductionChild: YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int YDProductionChildID { get; set; } = 0;

        ///<summary>
        /// YDProductionMasterID
        ///</summary>
        public int YDProductionMasterID { get; set; }= 0;

        ///<summary>
        /// YDBCTwistingID
        ///</summary>
        public int YDBCTwistingID { get; set; } = 0;
        public int YDBookingChildID { get; set; } = 0;
        public int YDDBIID { get; set; } = 0;

        ///<summary>
        /// YarnDyedColorID
        ///</summary>
        public int YarnDyedColorID { get; set; } = 0;

        ///<summary>
        /// BookingQty
        ///</summary>
        public int BookingQty { get; set; } = 0;

        ///<summary>
        /// Remarks (length: 500)
        ///</summary>
        public string Remarks { get; set; } = "";

        ///<summary>
        /// YarnCategory (length: 150)
        ///</summary>
        public string YarnCategory { get; set; } = "";

        ///<summary>
        /// NoOfThread
        ///</summary>
        public int NoOfThread { get; set; } = 0;

        ///<summary>
        /// ProducedQty
        ///</summary>
        public int ProducedQty { get; set; } = 0;

        ///<summary>
        /// ProducedCone
        ///</summary>
        public decimal ProducedCone { get; set; } = 0;

        ///<summary>
        /// TodayProductionQty
        ///</summary>
        public int TodayProductionQty { get; set; } = 0;

        ///<summary>
        /// YarnProgramID
        ///</summary>
        public int YarnProgramID { get; set; } = 0;

        ///<summary>
        /// DPID
        ///</summary>
        public int DPID { get; set; } = 0;

        ///<summary>
        /// ColorId
        ///</summary>
        public int ColorId { get; set; } = 0;

        ///<summary>
        /// ShadeCode
        ///</summary>
        public string ShadeCode { get; set; } = "";

        ///<summary>
        /// ProgramName
        ///</summary>
        public string ProgramName { get; set; } = "";

        ///<summary>
        /// ColorCode
        ///</summary>
        public string ColorCode { get; set; } = "";

        ///<summary>
        /// DPProcessInfo
        ///</summary>
        public string DPProcessInfo { get; set; } = "";
        public int YDRICRBId { get; set; }= 0;

        #region Additional Columns

        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; } = 0;

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        [Write(false)]
        public string Uom { get; set; } = "";

        [Write(false)]
        public string YarnType { get; set; } = "";

        [Write(false)]
        public string YarnCount { get; set; } = "";

        [Write(false)]
        public string Composition { get; set; } = "";

        [Write(false)]
        public string YarnShade { get; set; } = "";

        [Write(false)]
        public string YarnColor { get; set; } = "";

        [Write(false)]
        public string DisplayUnitDesc { get; set; } = "";

        [Write(false)]
        public int BuyerID { get; set; }= 0;

        [Write(false)]
        public string Process { get; set; } = "";

        [Write(false)]
        public string SubProcess { get; set; } = "";

        [Write(false)]
        public string QualityParameter { get; set; } = "";

        [Write(false)]
        public string Count { get; set; } = "";

        [Write(false)]
        public string NoofPly { get; set; } = "";

        [Write(false)]
        public string ColorName { get; set; } = "";

        [Write(false)]
        public string DPName { get; set; } = "";

        [Write(false)]
        public int BookingConeQty { get; set; } = 0;

        #endregion Additional Columns

       
    }

    //public class YDProductionChildValidator : AbstractValidator<YDProductionChild>
    //{
    //    public YDProductionChildValidator()
    //    {
    //        RuleFor(x => x.ProducedQty).NotEmpty().WithMessage("Quantity is required.");
    //    }
    //}
}

using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting
{
    [Table(TableNames.KNITTING_ROLL_Master)]
    public class KRollMaster : DapperBaseEntity
    {

        #region Table Fields

        [ExplicitKey]
        public int KRollMasterID { get; set; }

        ///<summary>
        /// RollNo
        ///</summary>
        public int RollNo { get; set; }

        ///<summary>
        /// BarCodeNo
        ///</summary>
        public string BarCodeNo { get; set; }

        ///<summary>
        /// ProductionDate
        ///</summary>
        public DateTime ProductionDate { get; set; }

        ///<summary>
        /// KJobCardMasterID
        ///</summary>
        public int KJobCardMasterID { get; set; }

        ///<summary>
        /// IsSubContact
        ///</summary>
        public bool IsSubContact { get; set; }

        ///<summary>
        /// ContactID
        ///</summary>
        public int ContactID { get; set; }

        ///<summary>
        /// BookingID
        ///</summary>
        public int BookingID { get; set; }

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
        /// SubGroupID
        ///</summary>
        public int SubGroupID { get; set; }

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }

        ///<summary>
        /// UnitID
        ///</summary>
        public int UnitID { get; set; }

        ///<summary>
        /// KJobCardQty
        ///</summary>
        public decimal KJobCardQty { get; set; }

        ///<summary>
        /// OperatorID
        ///</summary>
        public int OperatorID { get; set; }

        ///<summary>
        /// ShiftID
        ///</summary>
        public int ShiftID { get; set; }

        ///<summary>
        /// Width
        ///</summary>
        public int Width { get; set; }

        ///<summary>
        /// RollWeight
        ///</summary>
        public decimal RollWeight { get; set; }

        ///<summary>
        /// ReceiveQty
        ///</summary>
        public decimal ReceiveQty { get; set; }

        ///<summary>
        /// Remarks (length: 500)
        ///</summary>
        public string Remarks { get; set; }

        ///<summary>
        /// AddedBy
        ///</summary>
        public int AddedBy { get; set; }

        ///<summary>
        /// DateAdded
        ///</summary>
        public DateTime DateAdded { get; set; }

        ///<summary>
        /// UpdatedBy
        ///</summary>
        public int? UpdatedBy { get; set; }

        ///<summary>
        /// DateUpdated
        ///</summary>
        public DateTime? DateUpdated { get; set; }

        ///<summary>
        /// ReceiveBy
        ///</summary>
        public int? ReceiveBy { get; set; }

        ///<summary>
        /// ReceiveDate
        ///</summary>
        public DateTime? ReceiveDate { get; set; }

        #endregion Table Fields

        #region Additional Properties

        [Write(false)]
        public string KJobCardNo { get; set; }

        [Write(false)]
        public DateTime KJobCardDate { get; set; }

        [Write(false)]
        public string Contact { get; set; }

        [Write(false)]
        public string BookingNo { get; set; }

        [Write(false)]
        public DateTime BookingDate { get; set; }

        [Write(false)]
        public string EWONo { get; set; }

        [Write(false)]
        public string Buyer { get; set; }

        [Write(false)]
        public string BuyerTeam { get; set; }

        [Write(false)]
        public string SubGroup { get; set; }

        [Write(false)]
        public string Uom { get; set; }

        [Write(false)]
        public int ConstructionID { get; set; }

        [Write(false)]
        public int CompositionID { get; set; }

        [Write(false)]
        public int FabricColorID { get; set; }

        [Write(false)]
        public int FabricGsm { get; set; }

        [Write(false)]
        public int FabricWidth { get; set; }

        [Write(false)]
        public int KnittingTypeID { get; set; }

        [Write(false)]
        public int DyeingTypeID { get; set; }

        [Write(false)]
        public string FabricConstruction { get; set; }

        [Write(false)]
        public string FabricComposition { get; set; }

        [Write(false)]
        public string ColorName { get; set; }

        [Write(false)]
        public string DyeingType { get; set; }

        [Write(false)]
        public string KnittingType { get; set; }

        [Write(false)]
        public string Operator { get; set; }

        [Write(false)]
        public string Shift { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || KRollMasterID > 0;

        #endregion Additional Properties

        public KRollMaster()
        {
            IsSubContact = false;
            BuyerID = 0;
            BuyerTeamID = 0;
            SubGroupID = 0;
            KJobCardQty = 0m;
            RollWeight = 0m;
            DateAdded = DateTime.Now;
            ProductionDate = DateTime.Now;
        }
    }
    #region Validator

    //public class KRollMasterBindingModel
    //{
    //    public List<KRollMaster> List { get; set; }

    //    public KRollMasterBindingModel()
    //    {
    //        List = new List<KRollMaster>();
    //    }
    //}

    //public class KRollMasterValidator : AbstractValidator<KRollMaster>
    //{
    //    public KRollMasterValidator()
    //    {
    //        //RuleFor(x => x.RollNo).NotEmpty();
    //        RuleFor(x => x.ProductionDate).NotEmpty();
    //        RuleFor(x => x.KJobCardMasterID).NotEmpty();
    //        RuleFor(x => x.ContactID).NotEmpty();
    //        //RuleFor(x => x.BookingID).NotEmpty();
    //        RuleFor(x => x.ExportOrderID).NotEmpty();
    //        RuleFor(x => x.BuyerID).NotEmpty();
    //        RuleFor(x => x.BuyerTeamID).NotEmpty();
    //        RuleFor(x => x.SubGroupID).NotEmpty();
    //        RuleFor(x => x.ItemMasterID).NotEmpty();
    //        RuleFor(x => x.UnitID).NotEmpty();
    //        RuleFor(x => x.KJobCardQty).NotEmpty();
    //        RuleFor(x => x.OperatorID).NotEmpty();
    //        RuleFor(x => x.ShiftID).NotEmpty();
    //        //RuleFor(x => x.Width).NotEmpty();
    //        RuleFor(x => x.RollWeight).NotEmpty();
    //    }
    //}

    #endregion Validator
}

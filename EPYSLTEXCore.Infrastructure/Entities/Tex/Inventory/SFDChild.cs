using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using FluentValidation;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory
{
    [Table(TableNames.SFD_CHILD)]
    public class SFDChild : IDapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int SFDChildID { get; set; }
        public int SFDID { get; set; }
        public int ConceptID { get; set; }
        public int? BookingChildID { get; set; }
        public int BookingID { get; set; }
        public int ConsumptionID { get; set; }
        public int ItemMasterID { get; set; }
        public int SubGroupID { get; set; }
        public int CCColorID { get; set; }
        public int ColorID { get; set; }
        public string ColorCode { get; set; }
        public int TechnicalNameId { get; set; }
        public int KnittingTypeID { get; set; }
        public int ConstructionID { get; set; }
        public int CompositionID { get; set; }
        public int GSMID { get; set; }
        public int FUPartID { get; set; }
        public decimal Width { get; set; }
        public bool IsYD { get; set; }
        public decimal Length { get; set; }
        public int UnitID { get; set; }
        public decimal BookingQty { get; set; }
        public int BookingQtyPcs { get; set; }
        public decimal StockQty { get; set; }
        public int StockQtyPcs { get; set; }
        public decimal DCQty { get; set; }
        public int DCQtyPcs { get; set; }
        public decimal ExcessQty { get; set; }
        public int ExcessQtyPcs { get; set; }
        public decimal ShortQty { get; set; }
        public int ShortQtyPcs { get; set; }
        public decimal AckQty { get; set; }
        public decimal AckQtyPcs { get; set; }
        public int DeliveryToID { get; set; }
        public string Remarks { get; set; }
        public int HangerQtyInPcs { get; set; }
        public int FormID { get; set; }

        #endregion Table Properties

        #region Additional Properties 
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        [Write(false)]
        public List<SFDChildRoll> ChildItems { get; set; }
        [Write(false)]
        public List<SFDChildRoll> AllRolls { get; set; }
        [Write(false)]
        public string GroupConceptNo { get; set; }
        [Write(false)]
        public DateTime ConceptDate { get; set; }

        [Write(false)]
        public string BookingNo { get; set; }
        [Write(false)]
        public string SubGroupName { get; set; }
        [Write(false)]
        public string TechnicalName { get; set; }
        [Write(false)]
        public string KnittingType { get; set; }
        [Write(false)]
        public string ConceptNo { get; set; }
        [Write(false)]
        public string Composition { get; set; }
        [Write(false)]
        public string Construction { get; set; }
        [Write(false)]
        public string Gsm { get; set; }
        [Write(false)]
        public int ConceptTypeID { get; set; }
        [Write(false)]
        public string ConcepTypeName { get; set; }
        [Write(false)]
        public string ConceptForName { get; set; }
        [Write(false)]
        public string ConceptStatus { get; set; }
        [Write(false)]
        public string CompanyName { get; set; }
        [Write(false)]
        public string SubClassName { get; set; }
        [Write(false)]
        public string FabricItemIDs { get; set; }
        [Write(false)]
        public string OthersItemIDs { get; set; }
        [Write(false)]
        public string MachineGauge { get; set; }
        [Write(false)]
        public string ColorName { get; set; }
        [Write(false)]
        public string YarnSubProgram { get; set; }
        [Write(false)]
        public int LFDChildID { get; set; }
        #endregion Additional Properties 
        public SFDChild()
        {
            Width = 0;
            Length = 0;
            BookingQty = 0;
            BookingQtyPcs = 0;
            StockQty = 0;
            StockQtyPcs = 0;
            DCQty = 0;
            DCQtyPcs = 0;
            ExcessQty = 0;
            ExcessQtyPcs = 0;
            ShortQty = 0;
            ShortQtyPcs = 0;
            AckQty = 0;
            AckQtyPcs = 0;
            UnitID = 28;
            SubGroupID = 0;
            LFDChildID = 0;
            EntityState = EntityState.Added;
            ChildItems = new List<SFDChildRoll>();
            AllRolls = new List<SFDChildRoll>();
        }
    }

    #region Validator

    public class SFDChildValidator : AbstractValidator<SFDChild>
    {
        public SFDChildValidator()
        {
            //RuleFor(x => x.ReqQty).GreaterThan(0);
            //RuleFor(x => x.ReqCone).GreaterThan(0);
        }
    }

    #endregion Validator
}

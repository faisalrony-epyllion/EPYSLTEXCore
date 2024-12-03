using Dapper.Contrib.Extensions;
using FluentValidation;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory
{
    [Table("T_SFDChildRoll")]
    public class SFDChildRoll : IDapperBaseEntity
    {
        #region Table Properties  
        [ExplicitKey]
        public int SFDChildRollID { get; set; }
        public int SFDChildID { get; set; }
        public int SFDID { get; set; }
        public int BookingChildID { get; set; }
        public int BookingID { get; set; }
        public int ItemMasterID { get; set; }
        public int ConsumptionID { get; set; }
        public int SubGroupID { get; set; }
        public int RollID { get; set; }
        public string RollNo { get; set; }
        public string Shade { get; set; }
        public string BatchNo { get; set; }
        public string UseBatchNo { get; set; }
        public int RackID { get; set; }
        public decimal RollQtyKg { get; set; }
        public int RollQtyPcs { get; set; }
        public string WeightSheetNo { get; set; }

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
        public string SubGroupName { get; set; }
        [Write(false)]
        public string GroupConceptNo { get; set; }
        [Write(false)]
        public string TechnicalName { get; set; }
        [Write(false)]
        public string RollItemIDs { get; set; }
        [Write(false)]
        public int CCColorID { get; set; }
        [Write(false)]
        public int ColorID { get; set; }
        [Write(false)]
        public string ColorCode { get; set; }
        [Write(false)]
        public string ColorName { get; set; }
        [Write(false)]
        public int ConceptID { get; set; }
        [Write(false)]
        public int BChildID { get; set; }
        [Write(false)]
        public int BItemReqID { get; set; }
        [Write(false)]
        public int BatchID { get; set; }
        [Write(false)]
        public string FabricChildItemIDs { get; set; }
        [Write(false)]
        public string OthersChildItemIDs { get; set; }
        [Write(false)]
        public int LFDMasterID { get; set; }
        [Write(false)]
        public int LFDChildID { get; set; }
        [Write(false)]
        public int LFDRollID { get; set; }
        [Write(false)]
        public bool IsAcknowledge { get; set; }
        #endregion Additional Properties

        public SFDChildRoll()
        {
            EntityState = EntityState.Added;
            SubGroupID = 0;
            ItemMasterID = 0;
            Shade = "";
            IsAcknowledge = false;
        }
    }

    #region Validator

    public class SFDChildRollValidator : AbstractValidator<SFDChildRoll>
    {
        public SFDChildRollValidator()
        {
            //RuleFor(x => x.ReqQty).GreaterThan(0);
            //RuleFor(x => x.ReqCone).GreaterThan(0);
        }
    }

    #endregion Validator
}

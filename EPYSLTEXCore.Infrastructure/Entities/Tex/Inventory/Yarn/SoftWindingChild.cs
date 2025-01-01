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

    [Table(TableNames.SOFT_WINDING_CHILD)]
    public class SoftWindingChild : YarnItemMaster, IDapperBaseEntity
    {
        #region Table Properties
        [ExplicitKey]
        public int SoftWindingChildID { get; set; }=-1;
        public int SoftWindingMasterID { get; set; } = 0;
        public int YDBookingChildID { get; set; } = 0;
        public int YDBItemReqID { get; set; } = 0;
        //public int ItemMasterID { get; set; } = 0;// Inherited FROM YarnItemMaster
        public int ColorID { get; set; } = 0;
        public decimal Qty { get; set; } = 0;
        public int Cone { get; set; } = 0;
        public string Remarks { get; set; } = "";
        public string YarnCategory { get; set; } = "";
        public int YDReceiveChildID { get; set; } = 0;
        public int YDRICRBId { get; set; } = 0;
        #endregion

        #region Additional Properties
        [Write(false)]
        public string ShadeCode { get; set; } = "";
        [Write(false)]
        public string NoOfThread { get; set; } = "";
        [Write(false)]
        public bool IsAdditionalItem { get; set; } = false;
        [Write(false)]
        public string ColorCode { get; set; } = "";
        [Write(false)]
        public string ColorName { get; set; } = "";
        [Write(false)]
        public int BookingFor { get; set; }= 0;
        [Write(false)]
        public string BookingForName { get; set; } = "";
        [Write(false)]
        public bool IsTwisting { get; set; }= false;
        [Write(false)]
        public bool IsWaxing { get; set; }=false;
        [Write(false)]
        public int RequiredQty { get; set; } = 0;
        [Write(false)]
        public int BookingQty { get; set; } = 0;
        [Write(false)]
        public decimal ReceiveQty { get; set; } = 0;
        [Write(false)]
        public int ReceiveCone { get; set; } = 0;
        [Write(false)]
        public int ReceiveCarton { get; set; } = 0;
        //[Write(false)]
        //public int UnitID { get; set; }// Inherited FROM YarnItemMaster
        [Write(false)]
        public string DisplayUnitDesc { get; set; } = "";
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; } = 0;

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        #endregion

        public SoftWindingChild()
        {
            EntityState = EntityState.Added;
        }

    }
    #region Validators

    //public class SoftWindingChildValidator : AbstractValidator<SoftWindingChild>
    //{
    //    public SoftWindingChildValidator()
    //    {
    //        RuleFor(x => x.YDBookingChildID).NotEmpty().WithMessage("YDBookingChildID is required.");
    //        RuleFor(x => x.Qty).NotEmpty().WithMessage("Qty is required.");
    //        RuleFor(x => x.Cone).NotEmpty().WithMessage("Cone is required.");
    //    }
    //}

    #endregion Validators
}

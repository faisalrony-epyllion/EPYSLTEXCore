using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using FluentValidation;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General
{
    [Table(TableNames.KNITTING_MACHINE)]
    public class KnittingMachine : DapperBaseEntity
    {
        [ExplicitKey]
        public int KnittingMachineID { get; set; }

        ///<summary>
        /// KnittingUnitID
        ///</summary>
        public int KnittingUnitID { get; set; }

        ///<summary>
        /// MachineNo (length: 50)
        ///</summary>
        public string MachineNo { get; set; }

        ///<summary>
        /// SerialNo (length: 50)
        ///</summary>
        public string SerialNo { get; set; }

        ///<summary>
        /// MachineNatureID
        ///</summary>
        public int MachineNatureID { get; set; }

        ///<summary>
        /// MachineTypeID
        ///</summary>
        public int MachineTypeID { get; set; }

        ///<summary>
        /// MachineSubClassID
        ///</summary>
        public int MachineSubClassID { get; set; }

        ///<summary>
        /// GG. MG (In Inch)
        ///</summary>
        public int GG { get; set; }

        ///<summary>
        /// Dia. Also Machine Bed for Flat Knit (In Inch)
        ///</summary>
        public int Dia { get; set; }

        ///<summary>
        /// Needle
        ///</summary>
        public int Needle { get; set; }

        ///<summary>
        /// BrandID
        ///</summary>
        public int BrandID { get; set; }

        ///<summary>
        /// OriginID
        ///</summary>
        public int OriginID { get; set; }

        ///<summary>
        /// Capacity
        ///</summary>
        public int Capacity { get; set; }

        ///<summary>
        /// Feeder
        ///</summary>
        public int Feeder { get; set; }

        ///<summary>
        /// MinRPM
        ///</summary>
        public decimal? MinRPM { get; set; }

        ///<summary>
        /// MaxRPM
        ///</summary>
        public decimal? MaxRPM { get; set; }

        ///<summary>
        /// AvgRPM
        ///</summary>
        public decimal? AvgRPM { get; set; }

        ///<summary>
        /// Head. Only for Flat Knit Machine
        ///</summary>
        public int? Head { get; set; }

        ///<summary>
        /// TwoToneCapacity. Only for Flat Knit Machine
        ///</summary>
        public int? TwoToneCapacity { get; set; }

        ///<summary>
        /// SolidCapacity. Only for Flat Knit Machine
        ///</summary>
        public int? SolidCapacity { get; set; }

        ///<summary>
        /// JacquredCapacity. Only for Flat Knit Machine
        ///</summary>
        public int? JacquredCapacity { get; set; }

        ///<summary>
        /// ManufacturerDate (length: 100)
        ///</summary>
        public string ManufacturerDate { get; set; }

        ///<summary>
        /// ErectionDate (length: 100)
        ///</summary>
        public string ErectionDate { get; set; }

        ///<summary>
        /// Remarks (length: 100)
        ///</summary>
        public string Remarks { get; set; }

        ///<summary>
        /// AddedBy
        ///</summary>
        public int AddedBy { get; set; }

        ///<summary>
        /// UpdatedBy
        ///</summary>
        public int? UpdatedBy { get; set; }

        ///<summary>
        /// DateAdded
        ///</summary>
        public System.DateTime DateAdded { get; set; }

        ///<summary>
        /// DateUpdated
        ///</summary>
        public System.DateTime? DateUpdated { get; set; }

        public System.DateTime? NextServicingDate { get; set; }

        public System.DateTime? LastServicingDate { get; set; }

        #region Additional Propeties

        [Write(false)]
        public List<KnittingMachineOption> KnittingMachineOptions { get; set; }

        [Write(false)]
        public string UnitName { get; set; }

        [Write(false)]
        public string Brand { get; set; }

        [Write(false)]
        public string Origin { get; set; }
        [Write(false)]
        public int ContactID { get; set; }

        [Write(false)]
        public string Contact { get; set; }

        [Write(false)]
        public string Nature { get; set; }

        [Write(false)]
        public string NatureName { get; set; }

        [Write(false)]
        public string MachineTypeName { get; set; }

        [Write(false)]
        public string MachineSubClassName { get; set; }

        [Write(false)]
        public int ServicePeriodDays { get; set; }

        [Write(false)]
        public bool IsComplete { get; set; }

        [Write(false)]
        public int UserID { get; set; }
        [Write(false)]
        public decimal NoOfCylinder { get; set; }
        [Write(false)]
        public List<Select2OptionModel> KnittingUnitList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> MachineNatureList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> MachineTypeList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> MachineSubClassList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> BrandList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> OriginList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> MachineGaugeList { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || KnittingMachineID > 0;

        [Write(false)]
        public int IsSubContact { get; set; }

        [Write(false)]
        public string id { get; set; }

        [Write(false)]
        public string text { get; set; }
        [Write(false)]
        public string ValueName { get; set; }
        #endregion Additional Propeties

        public KnittingMachine()
        {
            MachineNatureID = 0;
            MachineTypeID = 0;
            MachineSubClassID = 0;
            BrandID = 0;
            OriginID = 0;
            Head = 0;
            AddedBy = 0;
            ValueName = "";
            KnittingMachineOptions = new List<KnittingMachineOption>();
            KnittingUnitList = new List<Select2OptionModel>();
            MachineNatureList = new List<Select2OptionModel>();
            MachineTypeList = new List<Select2OptionModel>();
            MachineSubClassList = new List<Select2OptionModel>();
            BrandList = new List<Select2OptionModel>();
            OriginList = new List<Select2OptionModel>();
            MachineGaugeList = new List<Select2OptionModel>();
            NoOfCylinder = 0;
        }
    }

    public class KnittingMachineValidator : AbstractValidator<KnittingMachine>
    {
        public KnittingMachineValidator()
        {
            RuleFor(x => x.KnittingUnitID).NotEmpty();
            RuleFor(x => x.MachineNo).NotEmpty().MaximumLength(50);
            RuleFor(x => x.SerialNo).MaximumLength(50);
            RuleFor(x => x.MachineNatureID).NotEmpty();
            RuleFor(x => x.MachineTypeID).NotEmpty();
            RuleFor(x => x.MachineSubClassID).NotEmpty();
            RuleFor(x => x.GG).NotEmpty();
            RuleFor(x => x.Dia).NotEmpty();
            RuleFor(x => x.BrandID).NotEmpty();
            RuleFor(x => x.OriginID).NotEmpty();
            RuleFor(x => x.Remarks).MaximumLength(500);
        }
    }
}

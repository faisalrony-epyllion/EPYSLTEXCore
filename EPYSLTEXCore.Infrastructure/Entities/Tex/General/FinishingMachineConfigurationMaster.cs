using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;

namespace EPYSLTEX.Core.Entities.Tex
{

    [Table(TableNames.FINISHING_MACHINE_CONFIGURATION_MASTER)]
    public class FinishingMachineConfigurationMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int FMCMasterID { get; set; }

        public int ProcessTypeID { get; set; }

        public string ProcessName { get; set; }

        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }

        #region Additional Columns

        [Write(false)]
        public virtual List<FinishingMachineConfigurationChild> FinishingMachineConfigurationChilds { get; set; }

        [Write(false)]
        public virtual List<FinishingMachineSetup> FinishingMachineSetups { get; set; }

        [Write(false)]
        public string ProcessType { get; set; }

        [Write(false)]
        public List<Select2OptionModel> ProcessTypeList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> BrandList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> UnitList { get; set; }

        //[Write(false)]
        //public new List<FinishingMachineConfigurationChildDTO> FinishingMachineConfigurationChilds { get; set; }
        //[Write(false)]
        //public new List<FinishingMachineSetupDTO> FinishingMachineSetups { get; set; }
        [Write(false)]
        public List<Select2OptionModel> DyeingMcBrandList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> DyeProcessList { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.FMCMasterID > 0;

        [Write(false)]
        public string Status { get; set; }

        #endregion Additional Columns

        public FinishingMachineConfigurationMaster()
        {
            DateAdded = DateTime.Now;
            FinishingMachineConfigurationChilds = new List<FinishingMachineConfigurationChild>();
            FinishingMachineSetups = new List<FinishingMachineSetup>();
        }
    }


}
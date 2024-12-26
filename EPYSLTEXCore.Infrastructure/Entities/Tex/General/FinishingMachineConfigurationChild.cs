using Dapper.Contrib.Extensions;
using System.Data.Entity;
using EPYSLTEX.Core.Statics;
namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General
{

    [Table(TableNames.FINISHING_MACHINE_CONFIGURATION_CHILD)]
    public class FinishingMachineConfigurationChild : IDapperBaseEntity
    {
        public FinishingMachineConfigurationChild()
        {
            EntityState = EntityState.Added;
        }

        [ExplicitKey]
        public int FMCChildID { get; set; }

        public int FMCMasterID { get; set; }

        public string ParamName { get; set; }

        public string ParamDisplayName { get; set; }

        public int Sequence { get; set; }
        public int ProcessTypeID { get; set; }
        public bool NeedItem { get; set; }

        public string DefaultValue { get; set; }

        //public virtual FinishingMachineConfigurationMaster FinishingMachineConfigurationMaster { get; set; }

        #region Additional Fields

        [Write(false)]
        public string IsNeedItem => this.NeedItem ? "Yes" : "No";

        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public string ProcessType { get; set; }

        [Write(false)]
        public List<Select2OptionModel> ProcessTypeList { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        #endregion Additional Fields
    }


}
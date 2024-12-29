using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General
{
    [Table(TableNames.DYEING_MACHINE)]
    public class DyeingMachine : DapperBaseEntity
    {
        [ExplicitKey]
        public int DMID { get; set; }
        public int DyeingMcNameId { get; set; }
        public int CompanyId { get; set; }
        public int DyeingMcslNo { get; set; }
        public int DyeingMcStatusId { get; set; }
        public int DyeingMcBrandId { get; set; }
        public int DyeingMcCapacity { get; set; }
        public int DyeingNozzleQty { get; set; }
        public bool IsCC { get; set; }

        //public virtual List<DyeingMachineProcess> DyeingMachineProcesses { get; set; }
        #region Additional Columns

        [Write(false)]
        public virtual List<DyeingMachineProcess> DyeingMachineProcesses { get; set; }

        [Write(false)]
        public string DyeingMcName { get; set; }

        [Write(false)]
        public string Company { get; set; }
        //[Write(false)]
        // public string UserId { get; set; }

        [Write(false)]
        public string DyeingMcStatus { get; set; }
        [Write(false)]
        public string DyeingMcBrand { get; set; }

        [Write(false)]
        public List<Select2OptionModel> DyeingMcNameList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> CompanyList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> DyeingMcStatusList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> DyeingMcBrandList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> DyeProcessList { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.DMID > 0;
        [Write(false)]
        public string Status { get; set; }
        #endregion Additional Columns

        public DyeingMachine()
        {
            DyeingMachineProcesses = new List<DyeingMachineProcess>();
        }

    }
}

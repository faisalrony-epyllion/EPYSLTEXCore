using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EPYSLTEXCore.Infrastructure.Data
{
    public interface IDapperBaseEntity
    {
        EntityState EntityState { get; set; }
        int TotalRows { get; set; }
        bool IsModified { get; }
        bool IsNew { get; }
    }
}

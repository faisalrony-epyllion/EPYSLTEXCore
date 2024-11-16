using System.Data.Entity;


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

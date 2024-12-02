using System.Data.Entity;


namespace EPYSLTEXCore.Infrastructure.Entities
{
    public interface IDapperBaseEntity
    {
        EntityState EntityState { get; set; }
        int TotalRows { get; set; }
        bool IsModified { get; }
        bool IsNew { get; }
    }
}

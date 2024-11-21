using System.Data.Entity;

namespace EPYSLTEX.Web.Models
{
    public abstract class BaseModel
    {
        public BaseModel()
        {
            EntityState = EntityState.Added;
        }

        public int Id { get; set; }
        public int TotalRows { get; set; }
        public EntityState EntityState { get;set;}
        
        public bool IsModified()
        {
            return Id > 0;
        }

        public bool IsNew()
        {
            return Id <= 0;
        }
    }
}
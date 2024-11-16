using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Application.Entities
{
    public class Menu: DapperBaseEntity
    {
        [ExplicitKey]
        public int MenuID { get; set; }
        public string MenuName { get; set; }

        public string ActionLink { get; set; }


        //// Assuming there is another table "User" with "UserId" as its primary key
        //[System.ComponentModel.DataAnnotations.Schema.ForeignKey("User")] // This indicates that UserId is a foreign key referring to the User table
        //public int UserId { get; set; }

        #region Additional Properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || MenuName != "";
        [Write(false)]
        public int TotalReportCall { get; set; }
        [Write(false)]
        public int UserCode { get; set; }
        [Write(false)]
        public int LimitPerday { get; set; }

        #endregion Additional Properties
    }
}

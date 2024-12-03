using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities
{
    [Table("LoginUser")]
    public class LoginUser : DapperBaseEntity
    {
        public LoginUser()
        {
            //EmployeeCode = 0;
            CompanyId = 0;
            UserTypeId = 0;
            DefaultApplicationId = 0;
            IsSuperUser = false;
            IsAdmin = false;
            IsActive = false;
            IsLoggedIn = false;
            AddedBy = 0;
            DateAdded = DateTime.Now;
        }

        [ExplicitKey]
        public int UserCode { get; set; }

        public string UserName { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string EmailPassword { get; set; }
        public bool IsSuperUser { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public bool IsLoggedIn { get; set; }
        public DateTime? LogonTime { get; set; }
        public int EmployeeCode { get; set; }
        public int CompanyId { get; set; }
        public int DefaultApplicationId { get; set; }
        public int? UserTypeId { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }

        #region Additional Fields

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || UserCode > 0;

        //[Write(false)]
        //public List<Select2OptionModel> EmployeeList { get; set; }

        //[Write(false)]
        //public List<Select2OptionModel> CompanyList { get; set; }

        //[Write(false)]
        //public List<Select2OptionModel> DefaultApplicationList { get; set; }

        //[Write(false)]
        //public List<Select2OptionModel> UserTypeList { get; set; }

        //[Write(false)]
        //public List<Select2OptionModel> UserGroupList { get; set; }


        [Write(false)]
        public string CompanyName { get; set; }

        [Write(false)]
        public int EmployeeId { get; set; }


        [Write(false)]
        public string EmployeeName { get; set; }

        [Write(false)]
        public string ApplicationName { get; set; }
        [Write(false)]
        public int DepertmentID { get; set; }

        [Write(false)]
        public string DepertmentDescription { get; set; }
        [Write(false)]
        public int DesignationID { get; set; }

        [Write(false)]
        public string Designation { get; set; }

        [Write(false)]
        public string SectionName { get; set; }

        [Write(false)]
        public string DisplayEmployeeCode { get; set; }

        [Write(false)]
        public string EmployeeStatusName { get; set; }

        [Write(false)]
        public int id { get; set; }

        #endregion Additional Fields
    }
}
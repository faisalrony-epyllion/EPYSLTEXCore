using System;
using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEX.Core.Entities
{
    [Table("LoginHistory")]
    public class LoginHistory : DapperBaseEntity
    {
        public LoginHistory()
        {
            LoginHistoryID = 0;
            UserCode = 0;
            IPAddress = "";
            MachineName = "";
            MachineUserName = "";
            UserHostName = "";
            OpenPortNo = 0;
            LogonUserIdentityName = "";
        }
        [ExplicitKey]
        public int LoginHistoryID { get; set; }
        public int UserCode { get; set; }
        public string IPAddress { get; set; }
        public string MachineName { get; set; }
        public string MachineUserName { get; set; }
        public string LogonUserIdentityName { get; set; }
        public string UserHostName { get; set; }
        public int OpenPortNo { get; set; }
        public DateTime? LogInTime { get; set; }
        public DateTime? LogOutTime { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LoginHistoryID > 0;
    }
}

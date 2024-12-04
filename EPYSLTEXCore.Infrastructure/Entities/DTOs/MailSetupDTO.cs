using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.DTOs
{
    public class EmployeeMailSetupDTO
    {
        public string ToMailId { get; set; }
        public string CcMailId { get; set; }
        public string BccMailId { get; set; }
    }

    public class BuyerTeamWiseEmployeeMailSetupDTO
    {
        public string ToMailId { get; set; }
        public string CcMailId { get; set; }
        public string BccMailId { get; set; }
    }

    public class UserEmailInfo
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string EmailPassword { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string UserName { get; set; } = "";
    }

    public class ItemSubGroupMailSetupDTO
    {
        public string ToMailID { get; set; }
        public string CCMailID { get; set; }
        public string BCCMailID { get; set; }
    }
    public class MailServerConfiguration
    {
        public int ConfigurationID { get; set; }

        public string ConfigurationName { get; set; }

        public string SMTPServerIP { get; set; }

        public int SMTPServerPort { get; set; }

        public string SMTPMailID { get; set; }

        public string SMTPMailDisplayName { get; set; }

        public string SMTPMailPassword { get; set; }

        public bool IsActive { get; set; }

    }
}

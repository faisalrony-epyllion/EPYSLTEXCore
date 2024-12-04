using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities
{
    public class MailBasicProps
    {
        public string FromEmailId { get; set; } = "";
        public string ToEmailIds { get; set; } = "";
        public string CCEmailIds { get; set; } = "";
        public string BCCEmailIds { get; set; } = "";
        public string MailSubject { get; set; } = "";
        public string MailBody { get; set; } = "";
        public string Password { get; set; } = "";

        public string DefaultFromEmailId { get; set; } = "erpnoreply@epylliongroup.com";
        public string DefaultPassword { get; set; } = "Ugr7jT5d";
        public string DefaultToEmailIds { get; set; } = "imrez.ratin@epylliongroup.com;alam.hossain@epylliongroup.com;shifuddin@epylliongroup.com;";
        public string DefaultCCEmailIds { get; set; } = "";
        public string DefaultBCCEmailIds { get; set; } = "";
    }
}

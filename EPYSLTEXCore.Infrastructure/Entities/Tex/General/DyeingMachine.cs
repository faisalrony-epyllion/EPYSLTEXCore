using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General
{
    [Table("TextileProcessMaster")]
    public class TextileProcessMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int TProcessMasterID { get; set; }        
        public string ProcessName { get; set; }
        public string DisplayName { get; set; }
        public string ShortName { get; set; }
        public bool HasSubProcess { get; set; }
        public int SeqNo { get; set; }
        public int? AddedBy { get; set; }
        public System.DateTime? DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public System.DateTime? DateUpdated { get; set; }
        #region Additional Columns
        [Write(false)]
        public string CompletionStatus { get; set; }
        [Write(false)]
        public string ProcessStatus { get; set; }
        [Write(false)]
        public string ActionStatus { get; set; }
        //[Write(false)]
        //public virtual ICollection<BookingAnalysisProcess> BookingAnalysisProcesses { get; set; } // BookingAnalysisProcess.FK_BookingAnalysisProcess_TextileProcessMaster
        //[Write(false)]
        //public virtual ICollection<TextileProcessChild> TextileProcessChilds { get; set; } // TextileProcessChild.FK_TextileProcessChild_TextileProcessMaster
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.TProcessMasterID > 0;
        #endregion Additional Columns
      
        public TextileProcessMaster()
        {
            HasSubProcess = false;
            SeqNo = 0;
            EntityState = EntityState.Added;
            //BookingAnalysisProcesses = new List<BookingAnalysisProcess>();
            //TextileProcessChilds = new List<TextileProcessChild>();
            //TextileProcessUsers = new List<TextileProcessUser>();
            ProcessStatus = "Pending";
            CompletionStatus = "Pending";
        }
    }
}

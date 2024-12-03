using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table("T_BDSDependentTNACalander")]
    public class BDSDependentTNACalander : DapperBaseEntity
    {
        [ExplicitKey]
        public int BDSEventID { get; set; }
        public int BookingID { get; set; }

        public int BookingChildID { get; set; }

        public int EventID { get; set; }

        public int TNADays { get; set; }

        public DateTime? BookingDate { get; set; }

        public DateTime? EventDate { get; set; }

        public DateTime? CompleteDate { get; set; }

        public bool RevisionPending { get; set; }

        public DateTime? RevisionCompleteDate { get; set; }

        public decimal SeqNo { get; set; }

        public bool SystemEvent { get; set; }

        public bool HasDependent { get; set; }

        public bool IsHoliDay { get; set; }

        public bool IsPass { get; set; }


        #region Additional Properties


        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || BDSEventID > 0;

        [Write(false)]
        public string CriteriaName { get; set; }

        [Write(false)]
        public List<BDSTNAEvent_HK> BDSTNAEvent_HKNames { get; set; }

        [Write(false)]
        public List<FBAChildPlanning> FBAChildPlannings { get; set; }

        [Write(false)]
        public List<FBAChildPlanning> FBAChildPlanningsWithIds { get; set; }

        [Write(false)]
        public List<dynamic> Results { get; set; }

        [Write(false)]
        public string BookingNo { get; set; }

        [Write(false)]
        public string EventDescription { get; set; }
        [Write(false)]
        public string ItemName { get; set; }
        [Write(false)]
        public string ColorName { get; set; }

        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public string BuyerTeamName { get; set; }
        [Write(false)]
        public string Construction { get; set; }
        [Write(false)]
        public string Composition { get; set; }
        [Write(false)]
        public string Color { get; set; }
        [Write(false)]
        public string GSM { get; set; }
        [Write(false)]
        public string FabricWidth { get; set; }
        [Write(false)]
        public string TechnicalName { get; set; }
        [Write(false)]
        public string MachineType { get; set; }
        [Write(false)]
        public int LengthYds { get; set; }
        [Write(false)]
        public string LengthInch { get; set; }
        [Write(false)]
        public string DyeingType { get; set; }
        [Write(false)]
        public string Instruction { get; set; }

        [Write(false)]
        public DateTime? AcknowledgeDate { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> BookingList { get; set; }

        [Write(false)]
        public string EventStatus
        {
            get
            {
                if (EventDate.IsNull())
                {
                    return "";
                }
                else
                {
                    if (CompleteDate.IsNull())
                    {
                        if (EventDate == DateTime.Now.Date)
                        {
                            return "Today";
                        }
                        else if (EventDate < DateTime.Now.Date)
                        {
                            return DateTime.Now.Date.Subtract(EventDate.Value.Date).TotalDays.ToString() + " Days over";
                        }
                        else
                        {
                            return EventDate.Value.Date.Subtract(DateTime.Now.Date).TotalDays.ToString() + " Days remaining";
                        }
                    }
                    else
                    {
                        if (EventDate.Value.Date < CompleteDate.Value.Date)
                        {
                            return "Complete after " + CompleteDate.Value.Date.Subtract(EventDate.Value.Date).TotalDays.ToString() + " days";
                        }
                        else if (EventDate.Value.Date.Equals(CompleteDate.Value.Date))
                        {
                            return "Complete";
                        }
                        else
                        {
                            return "Complete before " + EventDate.Value.Date.Subtract(CompleteDate.Value.Date).TotalDays.ToString() + " days";
                        }
                    }
                }
            }
            //set;

        }
        #endregion Additional Properties

        public BDSDependentTNACalander()
        {
            EventDate = DateTime.Now;
            FBAChildPlannings = new List<FBAChildPlanning>();
            BDSTNAEvent_HKNames = new List<BDSTNAEvent_HK>();
            FBAChildPlanningsWithIds = new List<FBAChildPlanning>();
            SystemEvent = false;
            HasDependent = false;
        }
    }
}

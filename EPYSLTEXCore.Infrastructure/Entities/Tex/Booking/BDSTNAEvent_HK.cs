using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table("T_BDSTNAEvent_HK")]
    public class BDSTNAEvent_HK : DapperBaseEntity
    {
        [ExplicitKey]
        public int EventID { get; set; }
        public string EventDescription { get; set; }

        public string EventDisplayName { get; set; }

        public int DepartmentID { get; set; }

        public decimal SeqNo { get; set; }

        public bool SystemEvent { get; set; }

        public bool HasDependent { get; set; }


        #region Additional Properties

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
        public string KnittingType { get; set; }

        [Write(false)]
        public string ConceptTypeID { get; set; }

        [Write(false)]
        public string BookingNo { get; set; }

        [Write(false)]
        public int ConstructionId { get; set; }

        [Write(false)]
        public int CompositionId { get; set; }

        [Write(false)]
        public int KnittingTypeId { get; set; }

        [Write(false)]
        public int KTypeId { get; set; }

        [Write(false)]
        public int GSMId { get; set; }

        [Write(false)]
        public string YarnType { get; set; }

        [Write(false)]
        public string YarnProgram { get; set; }

        [Write(false)]
        public string DyeingType { get; set; }

        [Write(false)]
        public string Instruction { get; set; }

        [Write(false)]
        public string ForBDSStyleNo { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || EventID > 0;

        [Write(false)]
        public string CriteriaName { get; set; }

        [Write(false)]
        public List<FBookingAcknowledgeChild> CriteriaNames { get; set; }

        [Write(false)]
        public List<FBAChildPlanning> FBAChildPlannings { get; set; }

        [Write(false)]
        public List<FBAChildPlanning> FBAChildPlanningsWithIds { get; set; }

        [Write(false)]
        public List<dynamic> Results { get; set; }

        [Write(false)]
        public string CriteriaIDs { get; set; }

        [Write(false)]
        public int CriteriaTime { get; set; }

        [Write(false)]
        public int TechnicalTime { get; set; }
        [Write(false)]
        public int TotalTime { get; set; }
        [Write(false)]
        public int FinishingTime { get; set; }
        [Write(false)]
        public int DyeingTime { get; set; }
        [Write(false)]
        public int MaterialTime { get; set; }
        [Write(false)]
        public int KnittingTime { get; set; }
        [Write(false)]
        public int TestReportTime { get; set; }
        [Write(false)]
        public int PreprocessTime { get; set; }
        [Write(false)]
        public int PreprocessDays { get; set; }
        [Write(false)]
        public int batchPreparationTime { get; set; }
        [Write(false)]
        public string ReferenceSourceName { get; set; }
        [Write(false)]
        public string Height { get; set; }
        [Write(false)]
        public string Description { get; set; }

        #endregion Additional Properties

        public BDSTNAEvent_HK()
        {
            CriteriaIDs = "";
            MachineType = "Empty";
            TechnicalName = "Empty";
            FBAChildPlannings = new List<FBAChildPlanning>();
            CriteriaNames = new List<FBookingAcknowledgeChild>();
            FBAChildPlanningsWithIds = new List<FBAChildPlanning>();

            Height = "";
            Description = "";
        }
    }

}

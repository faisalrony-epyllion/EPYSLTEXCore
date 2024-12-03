namespace EPYSLTEXCore.Infrastructure.Entities.General
{
    public class YarnProcessSetupMasterBindingModel : BaseModel
    {
        public YarnProcessSetupMasterBindingModel()
        {
            ChildYarnCounts = new List<YarnProcessSetupChildYarnCountBindingModel>();
            CountIDs = new List<int>();
            ChildFiberTypes = new List<YarnProcessSetupChildFiberTypeBindingModel>();
            FiberTypeIDs = new List<int>();
        }
        public int ManufacturingLineID { get; set; }
        public int ManufacturingProcessID { get; set; }
        public int ManufacturingSubProcessID { get; set; }
        public int YarnColorID { get; set; }
        public int ColorGradeID { get; set; }
        public List<YarnProcessSetupChildYarnCountBindingModel> ChildYarnCounts { get; set; }
        public List<YarnProcessSetupChildFiberTypeBindingModel> ChildFiberTypes { get; set; }
        public List<int> CountIDs { get; set; }
        public string Counts { get; set; }
        public List<int> FiberTypeIDs { get; set; }
        public string FiberTypes { get; set; }
    }
    public class YarnProcessSetupChildYarnCountBindingModel : BaseModel
    {
        public int SetupMasterID { get; set; }
        public int CountID { get; set; }
        public string Count { get; set; }
    }

    public class YarnProcessSetupChildFiberTypeBindingModel : BaseModel
    {
        public int SetupMasterID { get; set; }
        public int FiberTypeID { get; set; }
    }
}

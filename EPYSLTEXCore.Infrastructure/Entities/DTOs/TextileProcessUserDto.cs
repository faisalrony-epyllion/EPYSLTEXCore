namespace EPYSLTEXCore.Infrastructure.DTOs
{
    public class TextileProcessUserDTO
    {
        public int Id { get; set; }
        public int TProcessMasterID { get; set; }  
        public string ProcessName { get; set; }
        public int UserCode { get; set; }
        public string UserName { get; set; }
        public int TotalRows { get; set; }
        public string EmployeeName { get; set; }
        public string DepertmentDescription { get; set; }
        public string Designation { get; set; }
    }
}

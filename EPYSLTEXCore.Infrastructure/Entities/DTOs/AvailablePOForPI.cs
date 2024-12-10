using System;

namespace EPYSLTEXCore.Infrastructure.DTOs
{
    public class AvailablePOForPI
    {
        public int CDAPOMasterID { get; set; }
        public int YPOMasterID { get; set; }
        public string PONo { get; set; }
        public string CompanyName { get; set; }
        public DateTime PODate { get; set; }
        public int RevisionNo { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalValue { get; set; }
        public bool IsChecked { get; set; }
        public decimal BalancePOQty { get; set; }
        public decimal BalancePOValue { get; set; }
    }
}

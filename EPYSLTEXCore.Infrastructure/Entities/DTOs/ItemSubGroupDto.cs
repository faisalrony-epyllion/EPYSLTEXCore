using System;

namespace EPYSLTEXCore.Infrastructure.DTOs
{
    public class ItemSubGroupDTO
    {
        public int Id { get; set; }
        public string DisplaySubGrupId { get; set; }
        public string SubGroupName { get; set; }
        public string ItemPrefix { get; set; }
        public int ItemGroupId { get; set; }
        public int SeqNo { get; set; }
        public bool IsUsed { get; set; }
        public int MaxDisplayID { get; set; }
        public string GroupName { get; set; }
        public int UnitSetId { get; set; }
        public int DefaultUnitId { get; set; }
    }
}

using EPYSLTEXCore.Infrastructure.Entities;
using System.Collections.Generic;

namespace EPYSLTEXCore.Infrastructure.DTOs
{
    public class ItemStructureDTO
    {
        /// <summary>
        /// Segment display name to show in table.
        /// </summary>
        public string SegmentDisplayName { get; set; }

        /// <summary>
        /// Item master segment value field name. i.e Segment1ValueDesc, Segment2ValueDesc
        /// </summary>
        public string SegmentValueDescName { get; set; }

        /// <summary>
        /// Item master segment value field name. i.e Segment1ValueId, Segment2ValueId
        /// </summary>
        public string SegmentValueIdName { get; set; }

        public int SegmentNameID { get; set; }
        public bool AllowAdd { get; set; }
        public bool IsNumericValue { get; set; }
        public bool HasDefaultValue { get; set; }
        public int SegmentValueID { get; set; }
    }

    public class ItemInformation
    {
        public IEnumerable<ItemStructureDTO> ItemStructures { get; set; }
        public IEnumerable<Select2OptionModel> ItemSegmentValues { get; set; }
    }
}

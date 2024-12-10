using EPYSLTEXCore.Infrastructure.Entities;
using System.Collections.Generic;

namespace EPYSLTEXCore.Infrastructure.DTOs
{
    public class ItemSegmentMaster
    {
        public List<ItemSegmentChild> Compositions = new List<ItemSegmentChild>();
        public List<ItemSegmentChild> YarnTypes = new List<ItemSegmentChild>();
        public List<ItemSegmentChild> Processes = new List<ItemSegmentChild>();
        public List<ItemSegmentChild> SubProcesses = new List<ItemSegmentChild>();
        public List<ItemSegmentChild> QualityParameters = new List<ItemSegmentChild>();
        public List<ItemSegmentChild> Counts = new List<ItemSegmentChild>();
        public List<ItemSegmentChild> Fibers = new List<ItemSegmentChild>();
        public List<ItemSegmentChild> SubPrograms = new List<ItemSegmentChild>();
        public List<ItemSegmentChild> Certifications = new List<ItemSegmentChild>();
    }
}

namespace EPYSLTEX.Core.Entities.Tex
{

    // CDAFloorReFloorhild
    public class CDAFloorReFloorhild : BaseEntity
    {
        ///<summary>
        /// FloorReqMasterID
        ///</summary>
        public int FloorReqMasterID { get; set; }

        ///<summary>
        /// LotNo (length: 50)
        ///</summary>
        public string LotNo { get; set; }

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }

        ///<summary>
        /// UnitId
        ///</summary>
        public int UnitId { get; set; }

        ///<summary>
        /// ReqQty
        ///</summary>
        public int ReqQty { get; set; }

        // Foreign keys

        /// <summary>
        /// Parent CDAFloorReqMaster pointed by [CDAFloorReFloorhild].([FloorReqMasterID]) (FK_dbo.CDAFloorReFloorhild_dbo.CDAFloorReqMaster_FloorReqMasterID)
        /// </summary>
        public virtual CDAFloorReqMaster CDAFloorReqMaster { get; set; } // FK_dbo.CDAFloorReFloorhild_dbo.CDAFloorReqMaster_FloorReqMasterID

        public CDAFloorReFloorhild()
        {
            ReqQty = 0;
        }
    }

}
// </auto-generated>

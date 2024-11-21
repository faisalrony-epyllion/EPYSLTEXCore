namespace EPYSLTEXCore.Application.Entities
{
    public class KnittingUnit : BaseEntity
    {
        ///<summary>
        /// ContactID
        ///</summary>
        public int ContactId { get; set; }

        ///<summary>
        /// ShortName (length: 50)
        ///</summary>
        public string ShortName { get; set; }

        ///<summary>
        /// UnitName (length: 150)
        ///</summary>
        public string UnitName { get; set; }

        ///<summary>
        /// UnitName (length: 150)
        ///</summary>
        public bool IsKnitting { get; set; }

        ///<summary>
        /// WeightURL (length: 300)
        ///</summary>
        public string WeightURL { get; set; }

        ///<summary>
        /// PrinterName (length: 300)
        ///</summary>
        public string PrinterName { get; set; }

    }
}

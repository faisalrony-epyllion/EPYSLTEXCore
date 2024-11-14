using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLEMSCore.Infrastructure.Entities
{
    /// <summary>
    /// Base Item Master follows are generic Item Master structure. 
    /// ItemMasterID and UnitID fields are mapped to DB.
    /// </summary>
    public abstract class BaseItemMaster
    {
        /// <summary>
        /// ItemMasterID
        /// </summary>
        public int ItemMasterID { get; set; }

        /// <summary>
        /// SubGroupId
        /// </summary>
        [Write(false)]
        public int SubGroupId { get; set; }

        /// <summary>
        /// UnitID
        /// </summary>
        public int UnitID { get; set; }

        ///<summary>
        /// Segment1ValueID
        ///</summary>
        [Write(false)]
        public int Segment1ValueId { get; set; }

        /// <summary>
        /// Segment1ValueDesc
        /// </summary>
        private string _segment1ValueDesc;
        [Write(false)]
        public string Segment1ValueDesc
        {
            get
            {
                return _segment1ValueDesc;
            }
            set
            {
                _segment1ValueDesc = string.IsNullOrEmpty(value) || value.Equals("N/A", System.StringComparison.OrdinalIgnoreCase) ? string.Empty : value.Trim();
            }
        }

        ///<summary>
        /// Segment2ValueID
        ///</summary>
        [Write(false)]
        public int Segment2ValueId { get; set; }

        /// <summary>
        /// Segment2ValueDesc
        /// </summary>
        private string _segment2ValueDesc;
        [Write(false)]
        public string Segment2ValueDesc
        {
            get
            {
                return _segment2ValueDesc;
            }
            set
            {
                _segment2ValueDesc = string.IsNullOrEmpty(value) || value.Equals("N/A", System.StringComparison.OrdinalIgnoreCase) ? string.Empty : value.Trim();
            }
        }

        ///<summary>
        /// Segment3ValueID
        ///</summary>
        [Write(false)]
        public int Segment3ValueId { get; set; }

        /// <summary>
        /// Segment3ValueDesc
        /// </summary>
        private string _segment3ValueDesc;
        [Write(false)]
        public string Segment3ValueDesc
        {
            get
            {
                return _segment3ValueDesc;
            }
            set
            {
                _segment3ValueDesc = string.IsNullOrEmpty(value) || value.Equals("N/A", System.StringComparison.OrdinalIgnoreCase) ? string.Empty : value.Trim();
            }
        }

        ///<summary>
        /// Segment4ValueID
        ///</summary>
        [Write(false)]
        public int Segment4ValueId { get; set; }

        /// <summary>
        /// Segment4ValueDesc
        /// </summary>
        private string _segment4ValueDesc;
        [Write(false)]
        public string Segment4ValueDesc
        {
            get
            {
                return _segment4ValueDesc;
            }
            set
            {
                _segment4ValueDesc = string.IsNullOrEmpty(value) || value.Equals("N/A", System.StringComparison.OrdinalIgnoreCase) ? string.Empty : value.Trim();
            }
        }

        ///<summary>
        /// Segment5ValueID
        ///</summary>
        [Write(false)]
        public int Segment5ValueId { get; set; }

        /// <summary>
        /// Segment5ValueDesc
        /// </summary>
        private string _segment5ValueDesc;
        [Write(false)]
        public string Segment5ValueDesc
        {
            get
            {
                return _segment5ValueDesc;
            }
            set
            {
                _segment5ValueDesc = string.IsNullOrEmpty(value) || value.Equals("N/A", System.StringComparison.OrdinalIgnoreCase) ? string.Empty : value.Trim();
            }
        }

        ///<summary>
        /// Segment6ValueID
        ///</summary>
        [Write(false)]
        public int Segment6ValueId { get; set; }

        /// <summary>
        /// Segment6ValueDesc
        /// </summary>
        private string _segment6ValueDesc;
        [Write(false)]
        public string Segment6ValueDesc
        {
            get
            {
                return _segment6ValueDesc;
            }
            set
            {
                _segment6ValueDesc = string.IsNullOrEmpty(value) || value.Equals("N/A", System.StringComparison.OrdinalIgnoreCase) ? string.Empty : value.Trim();
            }
        }

        ///<summary>
        /// Segment7ValueID
        ///</summary>
        [Write(false)]
        public int Segment7ValueId { get; set; }

        /// <summary>
        /// Segment7ValueDesc
        /// </summary>
        private string _segment7ValueDesc;
        [Write(false)]
        public string Segment7ValueDesc
        {
            get
            {
                return _segment7ValueDesc;
            }
            set
            {
                _segment7ValueDesc = string.IsNullOrEmpty(value) || value.Equals("N/A", System.StringComparison.OrdinalIgnoreCase) ? string.Empty : value.Trim();
            }
        }

        ///<summary>
        /// Segment8ValueID
        ///</summary>
        [Write(false)]
        public int Segment8ValueId { get; set; }

        /// <summary>
        /// Segment8ValueDesc
        /// </summary>
        private string _segment8ValueDesc;
        [Write(false)]
        public string Segment8ValueDesc
        {
            get
            {
                return _segment8ValueDesc;
            }
            set
            {
                _segment8ValueDesc = string.IsNullOrEmpty(value) || value.Equals("N/A", System.StringComparison.OrdinalIgnoreCase) ? string.Empty : value.Trim();
            }
        }

        ///<summary>
        /// Segment9ValueID
        ///</summary>
        [Write(false)]
        public int Segment9ValueId { get; set; }

        /// <summary>
        /// Segment4ValueDesc
        /// </summary>
        private string _segment9ValueDesc;
        [Write(false)]
        public string Segment9ValueDesc
        {
            get
            {
                return _segment9ValueDesc;
            }
            set
            {
                _segment9ValueDesc = string.IsNullOrEmpty(value) || value.Equals("N/A", System.StringComparison.OrdinalIgnoreCase) ? string.Empty : value.Trim();
            }
        }

        ///<summary>
        /// Segment10ValueID
        ///</summary>
        [Write(false)]
        public int Segment10ValueId { get; set; }

        /// <summary>
        /// Segment10ValueDesc
        /// </summary>
        private string _segment10ValueDesc;
        [Write(false)]
        public string Segment10ValueDesc
        {
            get
            {
                return _segment10ValueDesc;
            }
            set
            {
                _segment10ValueDesc = string.IsNullOrEmpty(value) || value.Equals("N/A", System.StringComparison.OrdinalIgnoreCase) ? string.Empty : value.Trim();
            }
        }

        ///<summary>
        /// Segment11ValueID
        ///</summary>
        [Write(false)]
        public int Segment11ValueId { get; set; }

        /// <summary>
        /// Segment11ValueDesc
        /// </summary>
        private string _segment11ValueDesc;
        [Write(false)]
        public string Segment11ValueDesc
        {
            get
            {
                return _segment11ValueDesc;
            }
            set
            {
                _segment11ValueDesc = string.IsNullOrEmpty(value) || value.Equals("N/A", System.StringComparison.OrdinalIgnoreCase) ? string.Empty : value.Trim();
            }
        }

        ///<summary>
        /// Segment12ValueID
        ///</summary>
        [Write(false)]
        public int Segment12ValueId { get; set; }

        /// <summary>
        /// Segment12ValueDesc
        /// </summary>
        private string _segment12ValueDesc;
        [Write(false)]
        public string Segment12ValueDesc
        {
            get
            {
                return _segment12ValueDesc;
            }
            set
            {
                _segment12ValueDesc = string.IsNullOrEmpty(value) || value.Equals("N/A", System.StringComparison.OrdinalIgnoreCase) ? string.Empty : value.Trim();
            }
        }

        ///<summary>
        /// Segment13ValueID
        ///</summary>
        [Write(false)]
        public int Segment13ValueId { get; set; }

        /// <summary>
        /// Segment13ValueDesc
        /// </summary>
        private string _segment13ValueDesc;
        [Write(false)]
        public string Segment13ValueDesc
        {
            get
            {
                return _segment13ValueDesc;
            }
            set
            {
                _segment13ValueDesc = string.IsNullOrEmpty(value) || value.Equals("N/A", System.StringComparison.OrdinalIgnoreCase) ? string.Empty : value.Trim();
            }
        }

        ///<summary>
        /// Segment14ValueID
        ///</summary>
        [Write(false)]
        public int Segment14ValueId { get; set; }

        /// <summary>
        /// Segment14ValueDesc
        /// </summary>
        private string _segment14ValueDesc;
        [Write(false)]
        public string Segment14ValueDesc
        {
            get
            {
                return _segment14ValueDesc;
            }
            set
            {
                _segment14ValueDesc = string.IsNullOrEmpty(value) || value.Equals("N/A", System.StringComparison.OrdinalIgnoreCase) ? string.Empty : value.Trim();
            }
        }

        ///<summary>
        /// Segment15ValueID
        ///</summary>
        [Write(false)]
        public int Segment15ValueId { get; set; }

        /// <summary>
        /// Segment15ValueDesc
        /// </summary>
        private string _segment15ValueDesc;
        [Write(false)]
        public string Segment15ValueDesc
        {
            get
            {
                return _segment15ValueDesc;
            }
            set
            {
                _segment15ValueDesc = string.IsNullOrEmpty(value) || value.Equals("N/A", System.StringComparison.OrdinalIgnoreCase) ? string.Empty : value.Trim();
            }
        }

        public string GetItemName(string subGroupPrefix)
        {
            return string.Join(" ", new string[] {
                subGroupPrefix,
                Segment1ValueDesc,
                Segment2ValueDesc,
                Segment3ValueDesc,
                Segment4ValueDesc,
                Segment5ValueDesc,
                Segment6ValueDesc,
                Segment7ValueDesc,
                Segment8ValueDesc,
                Segment9ValueDesc,
                Segment10ValueDesc,
                Segment11ValueDesc,
                Segment12ValueDesc,
                Segment13ValueDesc,
                Segment14ValueDesc,
                Segment15ValueDesc
            }.Where(x => !string.IsNullOrEmpty(x)));
        }
    }
}

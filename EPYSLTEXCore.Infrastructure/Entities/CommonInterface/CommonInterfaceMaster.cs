using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Entities;
using EPYSLTEX.Core.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Data;
using System.Collections.Generic;

namespace EPYSLTEXCore.Infrastructure.Entities
{
    public class CommonInterfaceMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int MasterID { get; set; }

        public int MenuId { get; set; }

        ///<summary>
        /// TableName (length: 200)
        ///</summary>
        public string TableName { get; set; }

        ///<summary>
        /// InterfaceName (length: 500)
        ///</summary>
        public string InterfaceName { get; set; }

        ///<summary>
        /// Api Url
        /// All Insert, Update, Get, And Delete Operation can be performed using this single api
        ///</summary>
        public string ApiUrl { get; set; }

        ///<summary>
        /// ParameterColumns (length: 5000)
        ///</summary>
        public string ParameterColumns { get; set; }

        public int? MasterRowNum { get; set; }

        public int? MasterColNum { get; set; }

        public bool IsInsertAllow { get; set; }
        public bool IsAllowAddNew { get; set; }
        
        public bool IsUpdateAllow { get; set; }

        public bool IsDeleteAllow { get; set; }

        public bool HasGrid { get; set; }

        public string PrimaryKeyColumn { get; set; }

        public string SelectSql { get; set; }

        public string InsertSql { get; set; }

        public string UpdateSql { get; set; }
        public string SaveApiUrl { get; set; }
        public string ConName { get; set; }

        #region Additional Fields

        [Write(false)]
        public List<CommonInterfaceChild> Childs { get; set; }

        [Write(false)]
        public List<CommonInterfaceChildGrid> ChildGrids { get; set; }

        [Write(false)]
        public List<CommonInterfaceChildGridColumn> ChildGridColumns { get; set; }

        [Write(false)]
        public override bool IsModified => MasterID > 0 || EntityState == System.Data.Entity.EntityState.Modified;

        #endregion Additional Fields

        public CommonInterfaceMaster()
        {
            IsInsertAllow = true;
            IsUpdateAllow = true;
            IsDeleteAllow = true;
            HasGrid = false;
            Childs = new List<CommonInterfaceChild>();
            ChildGrids = new List<CommonInterfaceChildGrid>();
            ChildGridColumns = new List<CommonInterfaceChildGridColumn>();
        }
    }
}
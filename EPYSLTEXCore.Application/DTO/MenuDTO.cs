using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.DTO
{

        public class MenuDTO
        {
            public MenuDTO()
            {
                Childs = new List<MenuDTO>();
            }
            public int MenuId { get; set; }
            public int ApplicationId { get; set; }
            public int ParentId { get; set; }
            public string DockPanel { get; set; }
            public string MenuCaption { get; set; }
            public string PageName { get; set; }
            public string TabCaption { get; set; }
            public string NavigateUrl { get; set; }
            public string ImageUrl { get; set; }
            public int TabWidth { get; set; }
            public int PageWidth { get; set; }
            public int PageHeight { get; set; }
            public int SeqNo { get; set; }
            public bool IsVisible { get; set; }
            public int RestrictionLimit { get; set; }
            public bool IsAdminOnly { get; set; }
            public bool SingleUserView { get; set; }
            public bool HasAutoNo { get; set; }
            public bool UseCommonInterface { get; set; }
            public int ModuleSelection { get; set; }
            public bool HasParam { get; set; }
            public int MLevel { get; set; }
            public bool IsAPI { get; set; }
            public bool CanInsert { get; set; }
            public bool CanUpdate { get; set; }
            public bool CanDelete { get; set; }
            public List<MenuDTO> Childs { get; set; }
        }
    }


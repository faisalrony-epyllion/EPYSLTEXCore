namespace EPYSLTEXCore.Infrastructure.DTOs
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

        public int ReportId { get; set; }
        public int? Parent_Key { get; set; }
        public string Node_Text { get; set; }
        public string Report_Name { get; set; }
        public string Report_Path_Name { get; set; }
        public string Report_Sql { get; set; }

        public bool HasDefaultValue { get; set; }
        public bool IsMultipleSelection { get; set; }

        public bool HasExternalReport { get; set; }
        public bool IsSessionUse { get; set; }
        public List<MenuDTO> Childs { get; set; }
        }
    }


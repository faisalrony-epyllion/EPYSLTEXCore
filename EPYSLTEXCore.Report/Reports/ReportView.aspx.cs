﻿using System;

namespace EPYSLTEXCore.Report.Reports
{
    public partial class ReportView : System.Web.UI.Page
    {
        private int reportId;

        private bool HasExternalReport = false;
        private int buyerId;
        
        public ReportView()
        {
            
        }

        #region Page Events

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

 

        #endregion Page Events

        #region Methods



        #endregion Methods
    }
}
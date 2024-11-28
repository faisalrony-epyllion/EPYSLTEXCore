using EPYSLTEXCore.Report.Entities;
using EPYSLTEXCore.Report.ExceptionHandler;
using EPYSLTEXCore.Report.Repositories;
using EPYSLTEXCore.Report.Service;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace EPYSLTEXCore.Report.Reports
{
    public partial class ReportView : System.Web.UI.Page
    {
        private int reportId;
        private bool HasExternalReport = false;
        private int buyerId;
        private readonly RDLReportDocument _reportDocument;
        private readonly IReportSuiteRepository _reportSuiteRepository;
        private readonly IReportSuiteExternalSetupRepository _reportSuiteExternalSetupRepository;

        public ReportView()
        {
            _reportSuiteRepository = DependencyResolver.Current.GetService<IReportSuiteRepository>(); ;
            _reportSuiteExternalSetupRepository = DependencyResolver.Current.GetService<IReportSuiteExternalSetupRepository>(); ;
            _reportDocument = DependencyResolver.Current.GetService<RDLReportDocument>(); ;
        }

        #region Page Events

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                SetReportInfo();

                rpViewer.Drillthrough += RpViewerDrillthrough;
                if (IsPostBack)
                {
                    rpViewer.LocalReport.SubreportProcessing += LocalReport_SubreportProcessing;
                    return;
                }
                rpViewer.Reset();

                rpViewer.LocalReport.SubreportProcessing += LocalReport_SubreportProcessing;
                rpViewer.ProcessingMode = ProcessingMode.Local;
                rpViewer.LocalReport.DataSources.Clear();

                rpViewer.LocalReport.LoadReportDefinition(_reportDocument.GetCustomTextReader(_reportDocument.ReportPath));

                rpViewer.LocalReport.DisplayName = _reportDocument.Name;
                rpViewer.LocalReport.EnableExternalImages = true;

                LoadSubReportDefinition();
                SetDataSource(_reportDocument.DsSource);
                SetParameter();
                rpViewer.LocalReport.Refresh();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        protected void RpViewerDrillthrough(object sender, DrillthroughEventArgs e)
        {
            try
            {
                if (e.Report is ServerReport) return;

                LocalReport localreport = (LocalReport)e.Report;

                RDLReportDocument drillReport = new RDLReportDocument();
                drillReport.SetFields("User.Identity.GetUserId<int>().ToString()", _reportDocument.Name, _reportDocument.ReportPathWithOutName);
                drillReport.Load(string.Format(@"{0}\{1}.{2}", _reportDocument.ReportPathWithOutName, e.ReportPath, "rdl"));

                drillReport.LoadSourceDataSet(localreport.OriginalParametersToDrillthrough);

                localreport.DataSources.Clear();
                localreport.LoadReportDefinition(drillReport.GetCustomTextReader(drillReport.ReportPath));
                localreport.DisplayName = drillReport.Name;
                SetDataSource(drillReport.DsSource, localreport);

                ReportParameter[] Parameters = new ReportParameter[drillReport.Parameters.Count];
                int i = 0;
                foreach (RDLParameter rpParam in drillReport.Parameters)
                {
                    Parameters[i] = new ReportParameter();
                    Parameters[i].Name = rpParam.Name;
                    Parameters[i].Values.Add(rpParam.Value.ToString());
                    i++;
                }
                localreport.Refresh();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        protected void LocalReport_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            try
            {
                RDLReportDocument subReport = _reportDocument.SubReportList.Find(item => item.Name == e.ReportPath);
                if (subReport != null)
                {
                    subReport.LoadSubReportSourceDataSet(e.Parameters);
                    ReportDataSource rdSource = null;
                    foreach (DataTable dtTable in subReport.DsSource.Tables)
                    {
                        rdSource = new ReportDataSource();
                        rdSource.Name = dtTable.TableName;
                        rdSource.Value = dtTable.DefaultView;
                        e.DataSources.Add(rdSource);
                    }
                }
                else
                {
                    foreach (RDLReportDocument subSubReport in _reportDocument.SubReportList)
                    {
                        SubSubreportProcessing(subSubReport, e);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        protected void SubSubreportProcessing(RDLReportDocument subReport, SubreportProcessingEventArgs e)
        {
            try
            {
                RDLReportDocument objSubSubReport = subReport.SubReportList.Find(item => item.Name == e.ReportPath);
                if (objSubSubReport != null)
                {
                    LocalReport_SubSubreportProcessing(subReport, e);
                }
                else
                {
                    foreach (RDLReportDocument subSubReport in subReport.SubReportList)
                    {
                        SubSubreportProcessing(subSubReport, e);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void LocalReport_SubSubreportProcessing(RDLReportDocument subReport, SubreportProcessingEventArgs e)
        {
            try
            {
                RDLReportDocument subSubReport = subReport.SubReportList.Find(item => item.Name == e.ReportPath);
                if (subSubReport != null)
                {
                    subSubReport.LoadSubReportSourceDataSet(e.Parameters);
                    ReportDataSource rdSource = null;
                    foreach (DataTable dtTable in subSubReport.DsSource.Tables)
                    {
                        rdSource = new ReportDataSource();
                        rdSource.Name = dtTable.TableName;
                        rdSource.Value = dtTable.DefaultView;
                        e.DataSources.Add(rdSource);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        #endregion Page Events

        #region Methods


        private void  SetReportInfo()
        {
            int.TryParse(Request.QueryString["ReportId"], out reportId);
            bool.TryParse(Request.QueryString["HasExternalReport"], out HasExternalReport);
            int.TryParse(Request.QueryString["buyerId"], out buyerId);

            var reportEntity =  _reportSuiteRepository.GetById(reportId);
            Guard.Against.NullEntity(reportId, reportEntity);

            string reportPath = string.Format("{0}{1}", reportEntity.REPORT_PATH_NAME, reportEntity.REPORT_NAME);

            if (HasExternalReport)
            {
                if (buyerId <= 0)
                    throw new Exception("You must provide buyer id.");

                var reportSuiteExternalSetup = _reportSuiteExternalSetupRepository.GetByIdAndBuyer( reportId , buyerId);
                if (reportSuiteExternalSetup != null)
                {
                    reportEntity.REPORT_PATH_NAME = reportSuiteExternalSetup.ReportPathName;
                    reportPath = string.Format("{0}{1}", reportSuiteExternalSetup.ReportPathName, reportSuiteExternalSetup.ReportName);
                }

            }

            if (!System.IO.File.Exists(reportPath))
                throw new Exception("Can't load report file.");

            var filterSetList = JsonConvert.DeserializeObject<List<FilterSets>>(Request.QueryString["FilterSetList"]);
            filterSetList.FindAll(x => x.DataType == "DateTime").ForEach(x => x.ColumnValue = x.ColumnValue.FormatToShortDate());

            _reportDocument.SetFields("User.Identity.GetUserId()", reportEntity.NODE_TEXT, reportEntity.REPORT_PATH_NAME);
            _reportDocument.Load(reportPath);
            _reportDocument.FilterSetList = filterSetList;
            _reportDocument.LoadSourceDataSet();
            _reportDocument.SetFilterValue();
        }

        private void LoadSubReportDefinition()
        {
            try
            {
                foreach (RDLReportDocument subReport in _reportDocument.SubReportList)
                {
                    subReport.InitializeReportParameter();
                    rpViewer.LocalReport.LoadSubreportDefinition(subReport.Name, _reportDocument.GetCustomTextReader(subReport.ReportPath));
                    LoadSubSubReportDefinition(subReport);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void LoadSubSubReportDefinition(RDLReportDocument subReport)
        {
            try
            {
                foreach (RDLReportDocument subSubReport in subReport.SubReportList)
                {
                    subSubReport.InitializeReportParameter();
                    rpViewer.LocalReport.LoadSubreportDefinition(subSubReport.Name, _reportDocument.GetCustomTextReader(subSubReport.ReportPath));
                    LoadSubSubReportDefinition(subSubReport);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetDataSource(DataSet dsSource)
        {
            ReportDataSource rdSource = null;
            foreach (DataTable dtTable in dsSource.Tables)
            {
                rdSource = new ReportDataSource();
                rdSource.Name = dtTable.TableName;
                rdSource.Value = dtTable.DefaultView;
                rpViewer.LocalReport.DataSources.Add(rdSource);
            }
        }

        private void SetDataSource(DataSet dsSource, LocalReport report)
        {
            ReportDataSource rdSource = null;
            foreach (DataTable dtTable in dsSource.Tables)
            {
                rdSource = new ReportDataSource
                {
                    Name = dtTable.TableName,
                    Value = dtTable.DefaultView
                };
                report.DataSources.Add(rdSource);
            }
        }

        private void SetParameter()
        {
            ReportParameter[] Parameters = new ReportParameter[_reportDocument.Parameters.Count];
            int i = 0;
            foreach (RDLParameter rpParam in _reportDocument.Parameters)
            {
                Parameters[i] = new ReportParameter
                {
                    Name = rpParam.Name
                };
                Parameters[i].Values.Add(rpParam.Value.ToString());
                i++;
            }
            rpViewer.LocalReport.SetParameters(Parameters);
        }


        #endregion Methods
    }
}
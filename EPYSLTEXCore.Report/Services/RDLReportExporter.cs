using EPYSLTEXCore.Report.Entities;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace EPYSLTEXCore.Report.Service
{
    [Flags]
    public enum ExportType
    {
        PDF = 1,
        Excel = 2
    }
    public class RDLReportExporter
    {
        public static String MimeType = String.Empty;
        private static List<RDLReportDocument> SubReportList;
        public static Byte[] GetExportedByte(ExportType expType, DataSet reportSource, String reportPath, List<RDLParameter> rdlParameters, List<RDLReportDocument> subReportList)
        {
            try
            {
                SubReportList = subReportList;
                MimeType = String.Empty;
                LocalReport localReport = new LocalReport();
                localReport.ReportPath = reportPath;
                localReport.DisplayName = "Exported Report";
                localReport.EnableExternalImages = true;

                localReport.SubreportProcessing += localReport_SubreportProcessing;

                ReportDataSource rdSource = null;
                foreach (DataTable dtTable in reportSource.Tables)
                {
                    rdSource = new ReportDataSource();
                    rdSource.Name = dtTable.TableName;
                    rdSource.Value = dtTable.DefaultView;
                    localReport.DataSources.Add(rdSource);
                }
                SetParameter(localReport, rdlParameters);
                localReport.Refresh();
                return ConvertToByte(localReport, expType);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private static void localReport_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            try
            {
                RDLReportDocument subReport = SubReportList.Find(item => item.Name == e.ReportPath);
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
                    foreach (RDLReportDocument subSubReport in SubReportList)
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
        private static void SubSubreportProcessing(RDLReportDocument subReport, SubreportProcessingEventArgs e)
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
        private static void LocalReport_SubSubreportProcessing(RDLReportDocument subReport, SubreportProcessingEventArgs e)
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

        private static void SetParameter(LocalReport localReport, List<RDLParameter> rdlParameters)
        {
            ReportParameter[] Parameters = new ReportParameter[rdlParameters.Count];
            int i = 0;
            foreach (RDLParameter rpParam in rdlParameters)
            {
                Parameters[i] = new ReportParameter();
                Parameters[i].Name = rpParam.Name;
                Parameters[i].Values.Add(rpParam.Value.ToString());
                i++;

            }
            localReport.SetParameters(Parameters);
        }
        private static Byte[] ConvertToByte(LocalReport localReport, ExportType expType)
        {
            try
            {
                Warning[] warnings;
                String[] streamids;
                String mimeType;
                String encoding;
                String extension;

                Byte[] bytes = localReport.Render(
                   expType.ToString(), null, out mimeType, out encoding,
                   out extension,
                   out streamids, out warnings);

                MimeType = mimeType;
                return bytes;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public static void ExportToPDF(Object reportSource, String sourceName,
            String reportPath, String savePath)
        {
            try
            {
                LocalReport localReport = new LocalReport();
                localReport.ReportPath = reportPath;
                localReport.DisplayName = "Exported Report";

                ReportDataSource rdSource = new ReportDataSource();
                rdSource.Name = sourceName;
                rdSource.Value = reportSource;

                localReport.DataSources.Add(rdSource);
                ExportReportToPDF(localReport, savePath);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private static void ExportReportToPDF(LocalReport localReport, String savePath)
        {
            try
            {

                String fileNameWithPath = savePath;
                Warning[] warnings;
                String[] streamids;
                String mimeType;
                String encoding;
                String extension;

                Byte[] bytes = localReport.Render(
                   "PDF", null, out mimeType, out encoding,
                    out extension,
                   out streamids, out warnings);
                FileStream fs = new FileStream(fileNameWithPath, FileMode.Create);
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
                System.Diagnostics.ProcessStartInfo objNewInstance = new System.Diagnostics.ProcessStartInfo(fileNameWithPath);
                objNewInstance.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
                System.Diagnostics.Process.Start(objNewInstance);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public static String GetExportedReport(ExportType expType, DataSet reportSource, String reportPath, List<RDLParameter> rdlParameters, List<RDLReportDocument> subReportList, String savePath)
        {
            try
            {
                SubReportList = subReportList;
                MimeType = String.Empty;
                LocalReport localReport = new LocalReport();
                localReport.ReportPath = reportPath;
                localReport.DisplayName = "Exported Report";
                localReport.EnableExternalImages = true;

                localReport.SubreportProcessing += localReport_SubreportProcessing;

                ReportDataSource rdSource = null;
                foreach (DataTable dtTable in reportSource.Tables)
                {
                    rdSource = new ReportDataSource();
                    rdSource.Name = dtTable.TableName;
                    rdSource.Value = dtTable.DefaultView;
                    localReport.DataSources.Add(rdSource);
                }
                SetParameter(localReport, rdlParameters);
                localReport.Refresh();
                return ExportReportToPDFOnly(localReport, savePath);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static MemoryStream GetExportedMemoryStreamReport(ExportType expType, DataSet reportSource, String reportPath, List<RDLParameter> rdlParameters, List<RDLReportDocument> subReportList, String savePath)
        {
            try
            {
                SubReportList = subReportList;
                MimeType = String.Empty;
                LocalReport localReport = new LocalReport();
                localReport.ReportPath = reportPath;
                localReport.DisplayName = "Exported Report";
                localReport.EnableExternalImages = true;

                localReport.SubreportProcessing += localReport_SubreportProcessing;

                ReportDataSource rdSource = null;
                foreach (DataTable dtTable in reportSource.Tables)
                {
                    rdSource = new ReportDataSource();
                    rdSource.Name = dtTable.TableName;
                    rdSource.Value = dtTable.DefaultView;
                    localReport.DataSources.Add(rdSource);
                }
                SetParameter(localReport, rdlParameters);
                localReport.Refresh();
                return ExportReportMemoryStreamToPDFOnly(localReport, savePath);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private static String ExportReportToPDFOnly(LocalReport localReport, String savePath)
        {
            try
            {

                String fileNameWithPath = savePath;
                Warning[] warnings;
                String[] streamids;
                String mimeType;
                String encoding;
                String extension;

                Byte[] bytes = localReport.Render(
                   "PDF", null, out mimeType, out encoding,
                    out extension,
                   out streamids, out warnings);
                FileStream fs = new FileStream(fileNameWithPath, FileMode.Create);
                try
                {
                    if (File.Exists(fileNameWithPath))
                        File.Delete(fileNameWithPath);
                }
                catch { }
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
                return fileNameWithPath;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private static MemoryStream ExportReportMemoryStreamToPDFOnly(LocalReport localReport, String savePath)
        {
            try
            {

                String fileNameWithPath = savePath;
                Warning[] warnings;
                String[] streamids;
                String mimeType;
                String encoding;
                String extension;

                Byte[] bytes = localReport.Render(
                   "PDF", null, out mimeType, out encoding,
                    out extension,
                   out streamids, out warnings);
                MemoryStream ms = new MemoryStream(bytes);

                return ms;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}

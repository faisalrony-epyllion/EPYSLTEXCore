using EPYSLTEXCore.Report.Entities;
using EPYSLTEXCore.Report.ExceptionHandler;
using EPYSLTEXCore.Report.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Report.Service
{
    public interface IReportingService
    {
 
        Task<byte[]> GetPdfByte(int reportId, int userId, string poNo);

        Task<byte[]> GetPdfByte(int reportId, int userId, Dictionary<string, string> parameters, bool hasExternalReport = false, int buyerId = 0);
    }

    public class ReportingService : IReportingService
    {
        private readonly IReportSuiteRepository _reportSuiteRepository;
        private readonly IReportSuiteExternalSetupRepository _reportSuiteExternalSetupRepository;
        private readonly RDLReportDocument _reportDocument;

        public ReportingService(RDLReportDocument reportDocument
            , IReportSuiteRepository reportSuiteRepository
            , IReportSuiteExternalSetupRepository reportSuiteExternalSetupRepository
            )
        {
            _reportDocument = reportDocument;
            _reportSuiteRepository = reportSuiteRepository;
            _reportSuiteExternalSetupRepository = reportSuiteExternalSetupRepository;
        }
        
        public async Task<byte[]> GetPdfByte(int reportId, int userId, string poNo)
        {
            var reportEntity = await _reportSuiteRepository.GetByIdAsync(reportId);
            Guard.Against.NullEntity(reportId, reportEntity);

            string reportPath = string.Format("{0}{1}", reportEntity.REPORT_PATH_NAME, reportEntity.REPORT_NAME);
            if (!System.IO.File.Exists(reportPath))
                throw new Exception("Can't load report file.");

            var parameters = new List<CustomeParameter>()
            {
                new CustomeParameter { ParameterName = "PONo", ParameterValue= poNo }
            };

            var parameterValues = await _reportSuiteRepository.LoadReportParameterInfoAsync(reportEntity.REPORTID);
            parameterValues.Tables[0].TableName = "ParameterValues";

            var reportColumnList = new List<ReportSuiteColumnValueViewModel>();
            _reportDocument.SetFields(userId.ToString(), reportEntity.NODE_TEXT, reportEntity.REPORT_PATH_NAME);
            _reportDocument.Load(reportPath);
            _reportDocument.LoadFilterTable(parameterValues.Tables["ParameterValues"].Columns, reportColumnList);
            _reportDocument.SetParameterValue(parameters);
            _reportDocument.LoadSourceDataSet();
            _reportDocument.SetFilterValue();

            return RDLReportExporter.GetExportedByte(ExportType.PDF, _reportDocument.DsSource, _reportDocument.ReportPath, _reportDocument.Parameters, _reportDocument.SubReportList);
        }

        public async Task<byte[]> GetPdfByte(int reportId, int userId, Dictionary<string, string> parameters, bool hasExternalReport = false, int buyerId = 0)
        {
            var reportEntity = await _reportSuiteRepository.GetByIdAsync(reportId);
            Guard.Against.NullEntity(reportId, reportEntity);

            var reportName = reportEntity.REPORT_NAME;
            var reportPathName = reportEntity.REPORT_PATH_NAME;

            if (hasExternalReport && buyerId > 0)
            {
                var reportSuiteExternalSetup = await _reportSuiteExternalSetupRepository.GetByIdAndBuyerAsync(reportId , buyerId);
                if (reportSuiteExternalSetup != null)
                {
                    reportName = reportSuiteExternalSetup.ReportName;
                    reportPathName = reportSuiteExternalSetup.ReportPathName;
                }
            }

            string reportPath = string.Format("{0}{1}", reportPathName, reportName);
            if (!System.IO.File.Exists(reportPath))
                throw new Exception("Can't load report file.");

            var customeParameters = new List<CustomeParameter>();
            foreach (var item in parameters)
            {
                customeParameters.Add(new CustomeParameter { ParameterName = item.Key, ParameterValue = item.Value });
            }

            if (hasExternalReport && buyerId > 0) customeParameters.Add(new CustomeParameter { ParameterName = "BuyerID", ParameterValue = buyerId.ToString() });

            var parameterValues = await _reportSuiteRepository.LoadReportParameterInfoAsync(reportEntity.REPORTID);
            parameterValues.Tables[0].TableName = "ParameterValues";

            var reportColumnList = new List<ReportSuiteColumnValueViewModel>();
            _reportDocument.SetFields(userId.ToString(), reportEntity.NODE_TEXT, reportPathName);
            _reportDocument.Load(reportPath);
            _reportDocument.LoadFilterTable(parameterValues.Tables["ParameterValues"].Columns, reportColumnList);
            _reportDocument.SetParameterValue(customeParameters);
            _reportDocument.LoadSourceDataSet();
            _reportDocument.SetFilterValue();

            return RDLReportExporter.GetExportedByte(ExportType.PDF, _reportDocument.DsSource, _reportDocument.ReportPath, _reportDocument.Parameters, _reportDocument.SubReportList);
        }
    }
}
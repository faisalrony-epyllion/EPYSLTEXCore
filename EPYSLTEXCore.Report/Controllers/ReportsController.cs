using EPYSLTEXCore.Report.Entities;
using EPYSLTEXCore.Report.ExceptionHandler;
using EPYSLTEXCore.Report.Repositories;
using EPYSLTEXCore.Report.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using System.Web.Http.Cors;
using System.Web.Mvc;

namespace EPYSLTEXCore.Report.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*", SupportsCredentials = true)]
    public class ReportsController : Controller
    {
        private readonly IReportSuiteRepository _reportSuiteRepository;
        private readonly IReportSuiteExternalSetupRepository _reportSuiteExternalSetupRepository;
        private readonly IReportSuiteColumnValueRepository _reportSuiteColumnValueRepository;
        private readonly RDLReportDocument _reportDocument;
        private readonly IReportingService _reportingService;

        public ReportsController(IReportSuiteRepository reportSuiteRepository, IReportSuiteExternalSetupRepository reportSuiteExternalSetupRepository,
            RDLReportDocument reportDocument, IReportSuiteColumnValueRepository reportSuiteColumnValueRepository, IReportingService reportingService)
        {
            _reportSuiteRepository = reportSuiteRepository;
            _reportSuiteExternalSetupRepository = reportSuiteExternalSetupRepository;
            _reportSuiteColumnValueRepository = reportSuiteColumnValueRepository;
            _reportDocument = reportDocument;
            _reportingService = reportingService;
        }

        public ActionResult GetReport(string param)
        {
            TempData["paramToken"] = param;
            return RedirectToAction("Index");
        }

        
        public ActionResult Index()
        {        

            ViewBag.ProfilePic = "/images/user.png";
            ViewBag.EmployeeName = "Nishadur Rahman";
            ViewBag.paramToken = TempData["paramToken"];
            return View();
        }

        public async Task<ActionResult> GetReportInformation(int reportId, bool hasExternalReport, int? buyerId)
        {
            var reportEntity = await _reportSuiteRepository.GetByIdAsync(reportId);
            Guard.Against.NullEntity(reportId, reportEntity);

            var columnValueOptions = await _reportSuiteRepository.GetDynamicDataDapperAsync(reportEntity.REPORT_SQL);          
               
            var reportName = reportEntity.REPORT_NAME;
            var reportPathName = reportEntity.REPORT_PATH_NAME;
            if (hasExternalReport)
            {
                if (buyerId <= 0)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You must provide buyer id.");

                var reportSuiteExternalSetup = await _reportSuiteExternalSetupRepository.GetByIdAndBuyerAsync(reportId , buyerId.Value);
                if (reportSuiteExternalSetup != null)
                {
                    reportName = reportSuiteExternalSetup.ReportName;
                    reportPathName = reportSuiteExternalSetup.ReportPathName;
                }
            }

            var reportPhysicalPath = Path.Combine(reportPathName, reportName);
            if (!System.IO.File.Exists(reportPhysicalPath))
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            var records = await _reportSuiteColumnValueRepository.GetAllAsync(reportId);
            List<ReportSuiteColumnValueViewModel> columnValues = new List<ReportSuiteColumnValueViewModel>();
            foreach (var item in records)
            {
                ReportSuiteColumnValueViewModel objVM = new ReportSuiteColumnValueViewModel();
                objVM.ReportId = item.ReportId;
                objVM.ColumnId = item.ColumnId;
                objVM.DefaultValue = item.DefaultValue;
                objVM.Source = item.Source;
                objVM.DaoClass = item.DaoClass;
                objVM.MethodName = item.MethodName;
                objVM.ParameterName = item.ParameterName;
                objVM.IsHidden = item.IsHidden;
                objVM.ParentColumns = item.ParentColumns;
                objVM.ValueColumnId = item.ValueColumnId;
                objVM.DisplayColumnId = item.DisplayColumnId;
                objVM.IsMultipleSelection = item.IsMultipleSelection;
                objVM.DefaultValueDaoClass = item.DefaultValueDaoClass;
                objVM.DefaultValueMethodName = item.DefaultValueMethodName;
                objVM.DefaultValueColumnId = item.DefaultValueColumnId;
                objVM.ShowAdditionalColumn = item.ShowAdditionalColumn;
                objVM.AdditionalColumnId = item.AdditionalColumnId;
                objVM.AdditionalColumnHeader = item.AdditionalColumnHeader;
                objVM.ColumnWidth = item.ColumnWidth;
                columnValues.Add(objVM);
            }
           

            //foreach (var item in columnValues.FindAll(x => !string.IsNullOrEmpty(x.DefaultValueMethodName) && !string.IsNullOrEmpty(x.DefaultValueColumnId)))
            //    item.DefaultValue = await _reportSuiteSqlRepository.GetStringValueAsync(item.DefaultValueMethodName); this processe will be defined 

            var parameterValues = await _reportSuiteRepository.LoadReportParameterInfoAsync(reportId);
            parameterValues.Tables[0].TableName = "ParameterValues";
            _reportDocument.SetFields("UserId.ToString()", reportEntity.NODE_TEXT, reportPathName);
            _reportDocument.Load(reportPhysicalPath);
            _reportDocument.LoadFilterTable(parameterValues.Tables["ParameterValues"].Columns, columnValues);

            if (hasExternalReport)
            {
                var buyerFilterSet = _reportDocument.FilterSetList.Find(x => x.ColumnName.Equals("BuyerID", System.StringComparison.OrdinalIgnoreCase));
                if (buyerFilterSet != null)
                {
                    buyerFilterSet.ColumnValue = buyerId.ToString();
                    buyerFilterSet.DefaultValue = buyerId.ToString();
                }
            }

            foreach (var item in _reportDocument.FilterSetList)
                item.ColumnValue = string.IsNullOrEmpty(item.DefaultValue) ? "" : item.DefaultValue;

            return Json(new { _reportDocument.FilterSetList, ColumnValueOptions = columnValueOptions }, JsonRequestBehavior.AllowGet);
        }


        #region Pdf Report View

        [HttpGet]
        public async Task<ActionResult> DownloadPdf(int reportId, int userId, string filename, string parameters)
        {
            var parameterList = JsonConvert.DeserializeObject<Dictionary<string, string>>(parameters);
            var attachment = await _reportingService.GetPdfByte(reportId, userId, parameterList);
            return File(attachment, System.Net.Mime.MediaTypeNames.Application.Octet, $"{filename}.pdf");
        }

        [HttpGet]
        public async Task<ActionResult> PdfView()
        {
            bool hasExternalReport = false;
            int buyerId;
            int.TryParse(Request.QueryString["ReportId"], out int reportId);
            bool.TryParse(Request.QueryString["HasExternalReport"], out hasExternalReport);
            int.TryParse(Request.QueryString["buyerId"], out buyerId);

            var reportEntity =await  _reportSuiteRepository.GetByIdAsync(reportId);
            Guard.Against.NullEntity(reportId, reportEntity);

            string reportPath = string.Format("{0}{1}", reportEntity.REPORT_PATH_NAME, reportEntity.REPORT_NAME);
            if (hasExternalReport)
            {
                if (buyerId <= 0)
                    throw new Exception("You must provide buyer id.");

                var reportSuiteExternalSetup = await _reportSuiteExternalSetupRepository.GetByIdAndBuyerAsync( reportId ,buyerId);

                if (reportSuiteExternalSetup != null)
                {
                    reportEntity.REPORT_PATH_NAME = reportSuiteExternalSetup.ReportPathName;
                    reportPath = string.Format("{0}{1}", reportSuiteExternalSetup.ReportPathName, reportSuiteExternalSetup.ReportName);
                }
            }
            if (!System.IO.File.Exists(reportPath))
                throw new Exception("Can't load report file.");

            var filterSetList = JsonConvert.DeserializeObject<List<FilterSets>>(Request.QueryString["FilterSetList"]);

            _reportDocument.SetFields("UserId.ToString()", reportEntity.NODE_TEXT, reportEntity.REPORT_PATH_NAME);
            _reportDocument.Load(reportPath);
            _reportDocument.FilterSetList = filterSetList;
            _reportDocument.LoadSourceDataSet();
            _reportDocument.SetFilterValue();

            byte[] pdfByte = RDLReportExporter.GetExportedByte(ExportType.PDF, _reportDocument.DsSource, _reportDocument.ReportPath, _reportDocument.Parameters, _reportDocument.SubReportList);
            Response.Clear();
            Response.ContentType = "Application/pdf";
            if (Request.QueryString["FileName"] != "" && Request.QueryString["FileName"] != null)
            {
                string FileName = Request.QueryString["FileName"].Trim();
                Response.AddHeader("Content-Disposition", "inline; filename=" + FileName + ".pdf;");
            }

            Response.Buffer = false;
            Response.OutputStream.Write(pdfByte, 0, pdfByte.Length);
            Response.Flush();
            Response.End();
            return View();
        }

        public async Task<ActionResult> InlinePdfView()
        {
            int reportId = 0;
            int buyerId = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["ReportId"])) int.TryParse(Request.QueryString["ReportId"], out reportId);
            if (!string.IsNullOrEmpty(Request.QueryString["BuyerId"])) int.TryParse(Request.QueryString["BuyerId"], out buyerId);
            string reportName = Request.QueryString["ReportName"];

            ReportSuite reportEntity = reportId > 0? await _reportSuiteRepository.GetByIdAsync( reportId)
                : await _reportSuiteRepository.GetByNameAsync( reportName );

            Guard.Against.NullObject(reportEntity);

            ReportSuiteExternalSetup rsExternal = new ReportSuiteExternalSetup();
            if (reportEntity.HasExternalReport && buyerId > 0)
            {
                rsExternal = await _reportSuiteExternalSetupRepository.GetByIdAndBuyerAsync(reportEntity.REPORTID, buyerId);
            }

            string reportPath = string.Empty; string reportPathName = string.Empty;
            reportPath = string.Format("{0}{1}", reportEntity.REPORT_PATH_NAME, reportEntity.REPORT_NAME);
            if (rsExternal.IsNotNull())
            {
                if (reportEntity.HasExternalReport && buyerId > 0 && rsExternal.ReportName.IsNotNullOrEmpty() && rsExternal.ReportPathName.IsNotNullOrEmpty())
                {
                    reportEntity.REPORT_PATH_NAME = rsExternal.ReportPathName;
                    reportPath = string.Format("{0}{1}", rsExternal.ReportPathName, rsExternal.ReportName);
                }
            }
            if (!System.IO.File.Exists(reportPath)) throw new Exception("Can't load report file.");

            var parameters = new List<CustomeParameter>();
            foreach (var key in Request.QueryString.AllKeys)
            {
                if (key.Equals("ReportId", StringComparison.OrdinalIgnoreCase) || key.Equals("ReportName", StringComparison.OrdinalIgnoreCase)) continue;

                var param = new CustomeParameter { ParameterName = key, ParameterValue = Request.QueryString[key] };
                parameters.Add(param);
            }

            var parameterValues = await _reportSuiteRepository.LoadReportParameterInfoAsync(reportEntity.REPORTID);
            parameterValues.Tables[0].TableName = "ParameterValues";

            var reportColumnList = new List<ReportSuiteColumnValueViewModel>();
            _reportDocument.SetFields("UserId.ToString()", reportEntity.NODE_TEXT, reportEntity.REPORT_PATH_NAME);
            _reportDocument.Load(reportPath);
            _reportDocument.LoadFilterTable(parameterValues.Tables["ParameterValues"].Columns, reportColumnList);
            _reportDocument.SetParameterValue(parameters);
            _reportDocument.LoadSourceDataSet();
            _reportDocument.SetFilterValue();

            byte[] pdfByte = RDLReportExporter.GetExportedByte(ExportType.PDF, _reportDocument.DsSource, _reportDocument.ReportPath, _reportDocument.Parameters, _reportDocument.SubReportList);
            Response.Clear();
            Response.ContentType = "Application/pdf";
            if (Request.QueryString["FileName"] != "" && Request.QueryString["FileName"] != null)
            {
                string FileName = Request.QueryString["FileName"].Trim();
                Response.AddHeader("Content-Disposition", "inline; filename=" + FileName + ".pdf;");
            }

            Response.Buffer = false;
            Response.OutputStream.Write(pdfByte, 0, pdfByte.Length);
            Response.Flush();
            Response.End();
            return View();
        }

        #endregion Pdf Report View



        [HttpGet]
        public ActionResult GetFilterColumnOptions()
        {
            var parameters = new List<SqlParameter>();
            var isSP = false;
            foreach (var key in Request.QueryString.AllKeys)
            {
                if (key.Equals("IsSP"))
                    isSP = Request.QueryString[key].ToBoolean();
                if (key.Equals("ReportId") || key.Equals("MethodName") || key.Equals("IsSP"))
                    continue;

                var param = new SqlParameter($"@{key}", Request.QueryString[key]);
                parameters.Add(param);
            }

            var data = _reportSuiteRepository.GetDynamicData(Request.QueryString["MethodName"], isSP, parameters.ToArray());

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }

}
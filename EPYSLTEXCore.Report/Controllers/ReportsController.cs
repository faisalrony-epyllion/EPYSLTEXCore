using EPYSLTEXCore.Report.Entities;
using EPYSLTEXCore.Report.ExceptionHandler;
using EPYSLTEXCore.Report.Repositories;
using EPYSLTEXCore.Report.Service;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EPYSLTEXCore.Report.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IReportSuiteRepository _reportSuiteRepository;
        private readonly IReportSuiteExternalSetupRepository _reportSuiteExternalSetupRepository;
        private readonly IReportSuiteColumnValueRepository _reportSuiteColumnValueRepository;
        private readonly RDLReportDocument _reportDocument;

        public ReportsController(IReportSuiteRepository reportSuiteRepository, IReportSuiteExternalSetupRepository reportSuiteExternalSetupRepository,
            RDLReportDocument reportDocument, IReportSuiteColumnValueRepository reportSuiteColumnValueRepository)
        {
            _reportSuiteRepository = reportSuiteRepository;
            _reportSuiteExternalSetupRepository = reportSuiteExternalSetupRepository;
            _reportSuiteColumnValueRepository = reportSuiteColumnValueRepository;
            _reportDocument = reportDocument;

        }
    
        public ActionResult Index()
        {

            ViewBag.ProfilePic = "/images/user.png";
            ViewBag.EmployeeName = "Nishadur Rahman";
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




    }

}
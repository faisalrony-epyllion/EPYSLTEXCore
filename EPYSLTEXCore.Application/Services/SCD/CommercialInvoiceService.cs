using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.SCD;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;

namespace EPYSLTEXCore.Application.Services.SCD
{
    internal class CommercialInvoiceService : ICommercialInvoiceService
    {
        private readonly IDapperCRUDService<YarnCIMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _gmtConnection;

        public CommercialInvoiceService(IDapperCRUDService<YarnCIMaster> service
            //, ISignatureRepository signatureRepository
            )
        {
            _service = service;

            _service.Connection = _service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
            _gmtConnection = service.GetConnection(AppConstants.GMT_CONNECTION);

        }

        public async Task<List<YarnCIMaster>> GetPagedAsync(Status status, bool isCDAPage, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By CIID Desc" : paginationInfo.OrderBy;
            //if (status == Status.ApprovedDone)
            //{
            //    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By BankRefNumber Desc" : paginationInfo.OrderBy;

            //}
            var sql = string.Empty;
            if (status == Status.Pending)
            {
                sql += $@"
                ;WITH YPL AS (
	                SELECT LM.LCID, LM.LCNo, LM.LCDate, LM.LCQty, LM.LCValue, LM.CompanyID, LM.SupplierID, LM.LCExpiryDate,
	                LM.Proposed, LM.Approve, LM.LCFilePath, LM.AttachmentPreviewTemplate, 
	                (LM.LCValue + ((LM.LCValue * LM.Tolerance) / 100))MaxLCValue, SUM(Isnull(YCM.CIValue,0))CIValue, 
                    Count(CIID)TotalCI
	                FROM {TableNames.YarnLCMaster} LM 
	                Left Join {TableNames.YARN_CI_MASTER} YCM On LM.LCID = YCm.LCID 
	                WHERE Proposed = 1 AND Approve = 1 AND LM.IsCDA = '{isCDAPage}' 
	                Group By LM.LCID, LM.LCNo, LM.LCDate, LM.LCQty, LM.LCValue, LM.CompanyID, LM.SupplierID, LM.LCExpiryDate,
	                LM.Proposed, LM.Approve, LM.LCFilePath, LM.AttachmentPreviewTemplate, LM.Tolerance
                ),
                FinalList AS
                (
                    SELECT LCID, LCNo, LCDate, YPL.CompanyID, CE.CompanyName CompanyName, LCQty, LCValue, LCFilePath, AttachmentPreviewTemplate, 
                    LCExpiryDate,YPL.SupplierID, CC.[Name] SupplierName, 
                    CASE 
	                    WHEN Proposed = 1 AND Approve = 0 THEN 'Yet to Approved' 
	                    WHEN Proposed = 1 AND Approve = 1 THEN 'Approved' 
                    END AS LCStatus, TotalCI
                    FROM YPL
                    INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = YPL.SupplierID
                    INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPL.CompanyID 
                    Where LCValue > CIValue 
                )
                SELECT *,Count(*) Over() TotalRows FROM FinalList";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY LCID DESC" : paginationInfo.OrderBy;
            }
            else if (status == Status.PartiallyCompleted)
            {
                sql += $@"With 
                YPL AS(
                    SELECT	CM.CIID, CM.LCID, CM.CINo, CM.CIDate, CM.CIValue, CM.EXPNo, CM.EXPDate, CM.BOENo, CM.BOEDate, CM.ConsigneeID, CM.NotifyPartyID, CM.CIFilePath
                    FROM {TableNames.YARN_CI_MASTER} CM WHERE ISNULL(CM.Acceptance,0) = 0 AND IsCDA = '{isCDAPage}'
                ),
                FinalList AS
                (
                    SELECT	YPL.CIID, YPL.LCID, YPL.CINo, YPL.CIDate, YPL.CIValue, YPL.EXPNo, YPL.EXPDate, YPL.BOENo, YPL.BOEDate, YPL.ConsigneeID, YPL.NotifyPartyID,
		            LC.LCNo, LC.LcDate, LC.CompanyID, LC.LCValue, LC.LCQty, LC.LCExpiryDate, C.CompanyName CustomerName, LC.LCFilePath, YPL.CIFilePath, LC.AttachmentPreviewTemplate
		            FROM YPL
		            INNER JOIN {TableNames.YarnLCMaster} LC ON LC.LCID = YPL.LCID
		            INNER JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = LC.CompanyID
                )
                SELECT *,Count(*) Over() TotalRows FROM FinalList";
            }
            else if (status == Status.Completed)
            {
                sql += $@"With 
                YPL AS(
                    SELECT	CM.CIID, CM.LCID, CM.CINo, CM.CIDate, CM.CIValue, CM.EXPNo, CM.EXPDate, CM.BOENo, CM.BOEDate, CM.ConsigneeID, CM.NotifyPartyID, CM.CIFilePath
                    FROM {TableNames.YARN_CI_MASTER} CM WHERE ISNULL(CM.Acceptance,0) = 1 AND IsCDA = '{isCDAPage}'
                ),
                FinalList AS
                (
                    SELECT	YPL.CIID, YPL.LCID, YPL.CINo, YPL.CIDate, YPL.CIValue, YPL.EXPNo, YPL.EXPDate, YPL.BOENo, YPL.BOEDate, YPL.ConsigneeID, YPL.NotifyPartyID,LC.SupplierID, CC.[Name] SupplierName,
		            LC.LCNo, LC.LcDate, LC.CompanyID, LC.LCValue, LC.LCQty, LC.LCExpiryDate, C.CompanyName CustomerName, LC.LCFilePath, YPL.CIFilePath, LC.AttachmentPreviewTemplate
		            FROM YPL
                    INNER JOIN {TableNames.YarnLCMaster} LC ON LC.LCID = YPL.LCID
                    INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = LC.SupplierID
		            INNER JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = LC.CompanyID
                )
                SELECT *,Count(*) Over() TotalRows FROM FinalList";
            }
            else if (status == Status.Reject)
            {
                sql += $@"With 
                YPL AS(
                    SELECT	CM.CIID, CM.LCID, CM.CINo, CM.CIDate, CM.CIValue, CM.EXPNo, CM.EXPDate, CM.BOENo, CM.BOEDate, CM.ConsigneeID, CM.NotifyPartyID, CM.CIFilePath
                    FROM {TableNames.YARN_CI_MASTER} CM WHERE ISNULL(CM.Reject,0) = 1 AND IsCDA = '{isCDAPage}'
                ),
                FinalList AS
                (
                    SELECT	YPL.CIID, YPL.LCID, YPL.CINo, YPL.CIDate, YPL.CIValue, YPL.EXPNo, YPL.EXPDate, YPL.BOENo, YPL.BOEDate, YPL.ConsigneeID, YPL.NotifyPartyID,
		            LC.LCNo, LC.LcDate, LC.CompanyID, LC.LCValue, LC.LCQty, LC.LCExpiryDate, C.CompanyName CustomerName, LC.LCFilePath, YPL.CIFilePath, LC.AttachmentPreviewTemplate
		            FROM YPL
		            INNER JOIN {TableNames.YarnLCMaster} LC ON LC.LCID = YPL.LCID
		            INNER JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = LC.CompanyID
                )
                SELECT *,Count(*) Over() TotalRows FROM FinalList";
            }
            else if (status == Status.AwaitingPropose)
            {
                sql += $@"With 
                YPL AS(
                    SELECT	CM.CIID, CM.LCID, CM.CINo, CM.CIDate, CM.CIValue, CM.EXPNo, CM.EXPDate, CM.BOENo, CM.BOEDate, CM.ConsigneeID, CM.NotifyPartyID, CM.CIFilePath
                    FROM {TableNames.YARN_CI_MASTER} CM WHERE ISNULL(CM.Acceptance,0) = 1 AND IsCDA = '{isCDAPage}' AND ISNULL(BankAccept,0) = 0
                ),
                FinalList AS
                (
                    SELECT	YPL.CIID, YPL.LCID, YPL.CINo, YPL.CIDate, YPL.CIValue, YPL.EXPNo, YPL.EXPDate, YPL.BOENo, YPL.BOEDate, YPL.ConsigneeID, YPL.NotifyPartyID,SupplierId = LC.SupplierID, CC.[Name] SupplierName,
		            LC.LCNo, LC.LcDate, CompanyId = LC.CompanyID, LC.LCValue, LC.LCQty, LC.LCExpiryDate, C.CompanyName CustomerName, LC.LCFilePath, YPL.CIFilePath, LC.AttachmentPreviewTemplate
                    ,IssueBankId = LC.IssueBankID,IssueBank =BM.BankShortName 
		            FROM YPL
                    INNER JOIN {TableNames.YarnLCMaster} LC ON LC.LCID = YPL.LCID
                    INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = LC.SupplierID
		            INNER JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = LC.CompanyID
                    INNER JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = LC.IssueBankID
					INNER JOIN {DbNames.EPYSL}..BankMaster BM ON BM.BankMasterID = BB.BankMasterID
                )
                SELECT *,Count(*) Over() TotalRows FROM FinalList";
            }
            else if (status == Status.ApprovedDone)
            {
                #region Off For Grouping bankref No
                sql += $@"With 
                  YPL AS(
                      SELECT	CM.CIID, CM.LCID, CM.CINo, CM.CIDate, CM.CIValue, CM.EXPNo, CM.EXPDate, CM.BOENo, CM.BOEDate, CM.ConsigneeID, CM.NotifyPartyID, CM.CIFilePath
                      FROM {TableNames.YARN_CI_MASTER} CM WHERE ISNULL(CM.Acceptance,0) = 1 AND IsCDA = '{isCDAPage}' AND ISNULL(BankAccept,0) = 1
                  ),
                  FinalList AS
                  (
                      SELECT	YPL.CIID, YPL.LCID, YPL.CINo, YPL.CIDate, YPL.CIValue, YPL.EXPNo, YPL.EXPDate, YPL.BOENo, YPL.BOEDate, YPL.ConsigneeID, YPL.NotifyPartyID,LC.SupplierID, CC.[Name] SupplierName,
                LC.LCNo, LC.LcDate, LC.CompanyID, LC.LCValue, LC.LCQty, LC.LCExpiryDate, C.CompanyName CustomerName, LC.LCFilePath, YPL.CIFilePath, LC.AttachmentPreviewTemplate
                FROM YPL
                      INNER JOIN {TableNames.YarnLCMaster} LC ON LC.LCID = YPL.LCID
                      INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = LC.SupplierID
                INNER JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = LC.CompanyID
                  )
                  SELECT *,Count(*) Over() TotalRows FROM FinalList";

                #endregion
                #region Group By Bank Ref No
                //sql += $@"With YPL AS(
                //                SELECT	
                //             CM.BankRefNumber, CM.BankAcceptDate,CM.LCID,CM.CIValue
                //                FROM {TableNames.YARN_CI_MASTER} CM WHERE ISNULL(CM.Acceptance,0) = 1 AND IsCDA = 'False' AND ISNULL(BankAccept,0) = 1 And ISNULL(CM.BankRefNumber,'')!=''
                //            ),
                //            FinalList AS
                //            (
                //                SELECT	YPL.BankRefNumber,YPL.BankAcceptDate,LC.SupplierID, CC.[Name] SupplierName,C.CompanyName CustomerName, LC.CompanyID,
                //             CIValue = SUM(ISNULL(YPL.CIValue,0)),LC.IssueBankID,IssueBank.BranchName IssueBank

                //             FROM YPL
                //                INNER JOIN {TableNames.YarnLCMaster} LC ON LC.LCID = YPL.LCID
                //                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = LC.SupplierID
                //             INNER JOIN {DbNames.EPYSL}..CompanyEntity C ON C.CompanyID = LC.CompanyID
                //             INNER JOIN  {DbNames.EPYSL}..BankBranch CB_IssueBank ON CB_IssueBank.BankBranchID = LC.IssueBankID
                //             INNER JOIN  {DbNames.EPYSL}..BankBranch IssueBank ON IssueBank.BankBranchID = CB_IssueBank.BankBranchID
                //             GROUP BY YPL.BankRefNumber,YPL.BankAcceptDate,LC.SupplierID, CC.[Name],C.CompanyName, LC.CompanyID,LC.IssueBankID,IssueBank.BranchName
                //            )
                //            SELECT *,Count(*) Over() TotalRows FROM FinalList";
                #endregion

            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";
            return await _service.GetDataAsync<YarnCIMaster>(sql);
        }

        public async Task<YarnCIMaster> GetNewAsync(int newID)
        {
            var sql = string.Empty;
            sql += $@"
            ;WITH M AS 
            (
                SELECT	LM.LCID, LM.LCNo, LM.LCDate, LM.LCExpiryDate, LM.ProposalId, LM.RevisionNo, LM.CompanyID,LM.CurrencyID,
                LM.LCValue, LM.LCQty, LM.LCUnit, LM.LCReceiveDate, LM.IssueBankID, LM.PaymentBankID, LM.LienBankID, LM.NotifyPaymentBank, LM.BBReportingNumber, LM.PaymentModeID,
                LM.CalculationOfTenorId, LM.DocPresentationDays, LM.BankAcceptanceFrom, LM.MaturityCalculationID, LM.Tolerance, LM.IncoTermsID, LM.CIDecID, LM.BCDecID, LM.HSCode,
                LM.Proposed, LM.ProposedDate, LM.LCFilePath, LM.AttachmentPreviewTemplate, LM.SupplierId
                FROM {TableNames.YarnLCMaster} LM WHERE LM.LCID = {newID}
            )
            SELECT M.LCID, M.LCNo, M.LCDate, M.LCExpiryDate, M.ProposalId, M.RevisionNo, M.CompanyID CompanyID, M.CurrencyID,
            M.LCValue, M.LCQty, M.LCUnit, M.LCReceiveDate, M.IssueBankID, M.PaymentBankID, M.LienBankID, M.NotifyPaymentBank, 
            M.BBReportingNumber, M.PaymentModeID, M.CalculationOfTenorId CalculationOfTenor, M.DocPresentationDays, 
            M.BankAcceptanceFrom, M.MaturityCalculationID, M.Tolerance, M.IncoTermsID, M.CIDecID, M.BCDecID, M.HSCode,
            M.Proposed, M.ProposedDate, M.LCFilePath, M.AttachmentPreviewTemplate, CE.CompanyName CustomerName, 
            Cr.CurrencyCode, IssueBank.BranchName IssueBank, BB.BranchName PaymentBank, PM.PaymentMethodName, 
            ETV_CD.ValueName TenorOfCalculation, M.SupplierId, (M.LCValue + ((M.LCValue * M.Tolerance) / 100))MaxLCValue,
            ((M.LCValue * M.Tolerance) / 100)MaxTolerance 
            FROM M
            INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID
            LEFT JOIN {DbNames.EPYSL}..Currency Cr ON Cr.CurrencyID = M.CurrencyID
            --INNER JOIN  {DbNames.EPYSL}..CompanyBank CB_IssueBank ON CB_IssueBank.CompanyBankID = M.IssueBankID
            LEFT JOIN  {DbNames.EPYSL}..BankBranch CB_IssueBank ON CB_IssueBank.BankBranchID = M.IssueBankID
			LEFT JOIN  {DbNames.EPYSL}..BankBranch IssueBank ON IssueBank.BankBranchID = CB_IssueBank.BankBranchID
            LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.PaymentBankID
            LEFT JOIN {DbNames.EPYSL}..PaymentMethod PM ON PM.PaymentMethodID = M.PaymentModeID
            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV_CD ON ETV_CD.ValueID = M.CalculationOfTenorId
			GROUP BY M.LCID, M.LCNo, M.LCDate, M.LCExpiryDate, M.ProposalId, M.RevisionNo, M.CompanyID, M.CurrencyID,
            M.LCValue, M.LCQty, M.LCUnit, M.LCReceiveDate, M.IssueBankID, M.PaymentBankID, M.LienBankID, M.NotifyPaymentBank, M.BBReportingNumber, M.PaymentModeID,
            M.CalculationOfTenorId, M.DocPresentationDays, M.BankAcceptanceFrom, M.MaturityCalculationID, M.Tolerance, M.IncoTermsID, M.CIDecID, M.BCDecID, M.HSCode,
            M.Proposed, M.ProposedDate, M.LCFilePath, M.AttachmentPreviewTemplate, CE.CompanyName, Cr.CurrencyCode, IssueBank.BranchName, BB.BranchName,
			PM.PaymentMethodName, ETV_CD.ValueName, M.SupplierId;

            ----Childs
            /*;With M AS 
            (
	            SELECT YPIReceiveMasterID FROM {TableNames.YarnLCChild} WHERE LCID = {newID}
            )
            SELECT ROW_NUMBER() OVER(ORDER BY PIRC.ItemMasterID ASC) AS ChildID, PIRC.ItemMasterID, IM.ItemName ItemDescription, PIRC.UnitID UnitID, U.DisplayUnitDesc UOM, 
            SUM(PIRC.PIQty) PIQty, SUM(PIRC.PIValue) PIValue, SUM(PIRC.PIQty) InvoiceQty, SUM(PIRC.PIValue) PdValue, 
            AVG(PIRC.Rate) Rate, PIRC.YarnProgramId, PIRC.ShadeCode
            FROM M
            INNER JOIN {TableNames.YarnPIReceiveChild} PIRC ON PIRC.YPIReceiveMasterID = M.YPIReceiveMasterID
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = PIRC.ItemMasterID
            INNER JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = PIRC.UnitID
            GROUP BY PIRC.ItemMasterID, IM.ItemName, PIRC.UnitID, U.DisplayUnitDesc, PIRC.YarnProgramId,PIRC.ShadeCode; */

            ;With M AS 
            (
	            SELECT YPIReceiveMasterID FROM {TableNames.YarnLCChild} WHERE LCID = {newID}
            ),
            C As 
            (
	            Select YCC.ItemMasterID, SUM(Isnull(YCC.InvoiceQty,0))BalInvoiceQty, SUM(Isnull(YCC.Rate,0))BalRate, 
	            SUM(Isnull(YCC.PDValue,0))BalPDValue,  SUM(Isnull(YCC.PIQty,0))BalPIQty, SUM(Isnull(YCC.PIValue,0)) BalPIValue
	            From {TableNames.YarnLCMaster} YLM 
	            Left Join {TableNames.YARN_CI_MASTER} YCM On YCM.LCID = YLM.LCID
	            Left Join {TableNames.YARN_CI_CHILD} YCC On YCC.CIID = YCM.CIID 
	            WHERE YLM.LCID = {newID}
	            Group By YCC.ItemMasterID
            ),
            D As 
            (
	            SELECT ROW_NUMBER() OVER(ORDER BY PIRC.ItemMasterID ASC) AS ChildID, PIRC.ItemMasterID, 
                IM.ItemName ItemDescription, PIRC.UnitID UnitID, U.DisplayUnitDesc UOM, 
	            SUM(PIRC.PIQty) PIQty, SUM(PIRC.PIValue) PIValue, (SUM(PIRC.PIQty) - Isnull(BalInvoiceQty,0)) InvoiceQty, 
	            (SUM(PIRC.PIValue) - Isnull(BalPDValue,0)) PdValue, 
	            AVG(PIRC.Rate) Rate, PIRC.YarnProgramId, PIRC.ShadeCode,
	            (SUM(PIRC.PIQty) - Isnull(BalInvoiceQty,0))BalPIQty, (SUM(PIRC.PIValue) - Isnull(BalPDValue,0))BalPIValue
	            FROM M
	            INNER JOIN {TableNames.YarnPIReceiveChild} PIRC ON PIRC.YPIReceiveMasterID = M.YPIReceiveMasterID
	            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = PIRC.ItemMasterID
	            INNER JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = PIRC.UnitID
	            Left Join C On C.ItemMasterID = PIRC.ItemMasterID 
	            GROUP BY PIRC.ItemMasterID, IM.ItemName, PIRC.UnitID, U.DisplayUnitDesc, PIRC.YarnProgramId,PIRC.ShadeCode,
	            BalInvoiceQty, BalPDValue 
            )
            Select * From D Where BalPIQty > 0;

            ----Attached PI
            ;WITH M AS 
            (
	            SELECT YPIReceiveMasterID FROM {TableNames.YarnLCChild} WHERE LCID={newID}
            ),
            C AS 
            (
	            SELECT YPIReceiveMasterID, SUM(PIQty) TotalQty, SUM(PIValue) TotalValue 
                FROM {TableNames.YarnPIReceiveChild}
	            WHERE YPIReceiveMasterID IN (SELECT YPIReceiveMasterID FROM M) GROUP BY YPIReceiveMasterID
            )
            SELECT M.YPIReceiveMasterID YPIMasterID, PRM.YPINo PiNo, PRM.PIDate, PRM.SupplierID, Con.Name SupplierName, C.TotalQty, C.TotalValue
            FROM M
            INNER JOIN {TableNames.YarnPIReceiveMaster} PRM ON PRM.YPIReceiveMasterID = M.YPIReceiveMasterID
            INNER JOIN C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
			INNER JOIN {DbNames.EPYSL}..Contacts Con ON Con.ContactID = PRM.SupplierID;

            ----Customer
            ;SELECT CAST(CompanyID AS VARCHAR) AS id, CompanyName AS text
            FROM {DbNames.EPYSL}..CompanyEntity
			WHERE CompanyID IN(8,6)
			ORDER BY CompanyName;

            ----Payment term
            ;SELECT CAST(PaymentTermsID AS VARCHAR) AS id, PaymentTermsName AS text
            FROM {DbNames.EPYSL}..PaymentTrems
            WHERE PaymentTermsID > 0
            ORDER BY PaymentTermsID;

            ----LC Tenure
            ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
            FROM {DbNames.EPYSL}..EntityTypeValue
            WHERE EntityTypeID = {EntityTypeConstants.LC_TENURE} AND ValueName <> 'Select'
            ORDER BY ValueName;

            ----Currency Type
            ;SELECT CAST(CurrencyID AS VARCHAR) AS id, CurrencyCode AS text
            FROM {DbNames.EPYSL}..Currency
            WHERE CurrencyID = 2
            ORDER BY CurrencyID DESC;

            ----Company
            ;SELECT CAST(CompanyID AS VARCHAR) AS id, CompanyName AS text
            FROM {DbNames.EPYSL}..CompanyEntity
			WHERE CompanyID IN(8,6)
			ORDER BY CompanyName;

            ----lc tYPE
            ;SELECT CAST(PaymentMethodID AS VARCHAR) AS id, PaymentMethodName AS text
            FROM {DbNames.EPYSL}..PaymentMethod
            WHERE PaymentMethodID > 0
            ORDER BY PaymentMethodID;

            ----yARN uNIT
            ;SELECT CAST(PaymentMethodID AS VARCHAR) AS id, PaymentMethodName AS text
            FROM {DbNames.EPYSL}..PaymentMethod
            WHERE PaymentMethodID > 0
            ORDER BY PaymentMethodID;

            ----LCIssuingBank
            ;SELECT CAST(CompanyBankID AS VARCHAR) id, BranchName text FROM {DbNames.EPYSL}..CompanyBank CB
            INNER JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = CB.BankBranchID
            INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = CB.CompanyID;

            ----PaymentBank
            ;SELECT CAST(BB.BankBranchID AS VARCHAR) id, BranchName text FROM {DbNames.EPYSL}..BankBranch BB
            INNER JOIN {DbNames.EPYSL}..BankBranchPaymentType BBPT ON BBPT.BankBranchID = BB.BankBranchID
            INNER JOIN {DbNames.EPYSL}..BankPaymentType_HK BPT ON BPT.PaymentTypeID = BBPT.PaymentTypeID
            WHERE BPT.PaymentTypeID = 5;

            ----ConsigneeList
            ;SELECT CAST(Contacts.ContactID AS VARCHAR) AS id, Contacts.Name AS text
            FROM {DbNames.EPYSL}..Contacts
            INNER JOIN {DbNames.EPYSL}..ContactCategoryChild ON Contacts.ContactID = ContactCategoryChild.ContactID
            INNER JOIN {DbNames.EPYSL}..ContactCategoryHK ON ContactCategoryChild.ContactCategoryID = ContactCategoryHK.ContactCategoryID
            WHERE ContactCategoryHK.ContactCategoryID = {ContactCategoryConstants.CONTACT_CATEGORY_CONSIGNEE}
            ORDER BY Contacts.Name;

            ----NotifyPartyList
            ;SELECT CAST(Contacts.ContactID AS VARCHAR) AS id, Contacts.Name AS text
            FROM {DbNames.EPYSL}..Contacts
            INNER JOIN {DbNames.EPYSL}..ContactCategoryChild ON Contacts.ContactID = ContactCategoryChild.ContactID
            INNER JOIN {DbNames.EPYSL}..ContactCategoryHK ON ContactCategoryChild.ContactCategoryID = ContactCategoryHK.ContactCategoryID
            WHERE ContactCategoryHK.ContactCategoryID = {ContactCategoryConstants.CONTACT_CATEGORY_NOTIFY_PARTY}
            ORDER BY Contacts.Name;

            --All Doc Types
            SELECT DocTypeID = ETV.ValueID, DocTypeName = ETV.ValueName
            FROM {DbNames.EPYSL}..EntityTypeValue ETV
            WHERE ETV.EntityTypeID = 185
            ORDER BY ETV.ValueName

            ";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnCIMaster data = await records.ReadFirstOrDefaultAsync<YarnCIMaster>();
                Guard.Against.NullObject(data);
                data.CIChilds = records.Read<YarnCIChild>().ToList();
                data.CIChildPIs = records.Read<YarnCIChildPI>().ToList();
                data.CustomerList = records.Read<Select2OptionModel>().ToList();
                data.PaymentTermsList = records.Read<Select2OptionModel>().ToList();
                data.LCTenureList = records.Read<Select2OptionModel>().ToList();
                data.CurrencyTypeList = records.Read<Select2OptionModel>().ToList();
                data.CompanyList = records.Read<Select2OptionModel>().ToList();
                data.LCTypeList = records.Read<Select2OptionModel>().ToList();
                data.YarnUnitList = records.Read<Select2OptionModel>().ToList();
                data.LCIssuingBankList = records.Read<Select2OptionModel>().ToList();
                data.PaymentBankList = records.Read<Select2OptionModel>().ToList();
                data.ConsigneeList = records.Read<Select2OptionModel>().ToList();
                data.NotifyPartyList = records.Read<Select2OptionModel>().ToList();
                data.CIValue = data.CIChilds.Sum(x => x.PIValue);
                data.AllDocTypes = records.Read<YarnCIDoc>().ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }

        public async Task<YarnCIMaster> GetAsync(int id)
        {
            var sql = string.Empty;
            sql += $@"
                ;WITH M AS (
	                SELECT CM.IsCDA,CM.CIID, CM.LCID, CM.CINo, CM.CIDate, CM.CIValue, CM.EXPNo, CM.EXPDate, CM.BOENo, 
                    CM.BOEDate, CM.ConsigneeID, CM.NotifyPartyID, CM.SG, CM.CompanyId, CM.SupplierId, 
                    CM.BLNo, CM.ContainerStatus, CM.BLDate, CM.CIFilePath, 
                    CM.AcceptanceDate,CM.BankAcceptDate, CM.MaturityDate, CM.BankRefNumber, CM.AttachmentPreviewTemplate
	                FROM {TableNames.YARN_CI_MASTER} CM WHERE CIID = {id}
                )
                SELECT  M.IsCDA,M.CIID, M.LCID, M.CINo, M.CIDate, M.CIValue, M.EXPNo, M.EXPDate, M.BOENo, M.BOEDate, 
                M.ConsigneeID, M.NotifyPartyID, M.SG, LC.LCNo, LC.LCDate, LC.LCExpiryDate, LC.ProposalId, LC.RevisionNo, 
                LC.CompanyID, LC.CurrencyID, LC.LCValue, LC.LCQty, LC.LCUnit, LC.LCReceiveDate, LC.IssueBankID, 
                LC.PaymentBankID, LC.LienBankID, LC.NotifyPaymentBank, LC.BBReportingNumber, LC.PaymentModeID,
                LC.CalculationOfTenorId CalculationOfTenor, LC.DocPresentationDays, LC.BankAcceptanceFrom, LC.MaturityCalculationID, 
                LC.Tolerance, LC.IncoTermsID, LC.CIDecID, LC.BCDecID, LC.HSCode, LC.Proposed, LC.ProposedDate, LC.LCFilePath, 
                LC.AttachmentPreviewTemplate, CE.CompanyName CustomerName, Cr.CurrencyCode, IssueBank.BranchName IssueBank,
                BB.BranchName PaymentBank, PM.PaymentMethodName, ETV_CD.ValueName TenorOfCalculation, M.CompanyId, M.SupplierId,
                (LC.LCValue + ((LC.LCValue * LC.Tolerance) / 100))MaxLCValue, ((LC.LCValue * LC.Tolerance) / 100)MaxTolerance,
                M.BLNo, M.ContainerStatus, M.BLDate, M.CIFilePath,M.AcceptanceDate, M.BankAcceptDate, M.MaturityDate, M.BankRefNumber, M.AttachmentPreviewTemplate
                FROM M
                INNER JOIN {TableNames.YarnLCMaster} LC ON LC.LCID = M.LCID
                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = LC.CompanyID
                LEFT JOIN {DbNames.EPYSL}..Currency Cr ON Cr.CurrencyID = LC.CurrencyID
                --INNER JOIN  {DbNames.EPYSL}..CompanyBank CB_IssueBank ON CB_IssueBank.CompanyBankID = LC.IssueBankID
                LEFT JOIN  {DbNames.EPYSL}..BankBranch CB_IssueBank ON CB_IssueBank.BankBranchID = LC.IssueBankID
				LEFT JOIN  {DbNames.EPYSL}..BankBranch IssueBank ON IssueBank.BankBranchID = CB_IssueBank.BankBranchID
                LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = LC.PaymentBankID
                LEFT JOIN {DbNames.EPYSL}..PaymentMethod PM ON PM.PaymentMethodID = LC.PaymentModeID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV_CD ON ETV_CD.ValueID = LC.CalculationOfTenorId
                GROUP BY M.IsCDA,M.CIID, M.LCID, M.CINo, M.CIDate, M.CIValue, M.EXPNo, M.EXPDate, M.BOENo, M.BOEDate, M.ConsigneeID, M.NotifyPartyID,
                LC.LCID, LC.LCNo, LC.LCDate, LC.LCExpiryDate, LC.ProposalId, LC.RevisionNo, LC.CompanyID, LC.CurrencyID,
                LC.LCValue, LC.LCQty, LC.LCUnit, LC.LCReceiveDate, LC.IssueBankID, LC.PaymentBankID, LC.LienBankID, LC.NotifyPaymentBank, LC.BBReportingNumber, LC.PaymentModeID,
                LC.CalculationOfTenorId, LC.DocPresentationDays, LC.BankAcceptanceFrom, LC.MaturityCalculationID, LC.Tolerance, LC.IncoTermsID, LC.CIDecID, LC.BCDecID, LC.HSCode,
                LC.Proposed, LC.ProposedDate, LC.LCFilePath, LC.AttachmentPreviewTemplate, CE.CompanyName, Cr.CurrencyCode, IssueBank.BranchName,
                BB.BranchName, PM.PaymentMethodName, ETV_CD.ValueName, M.SG, M.CompanyId, M.SupplierId, M.BLNo, 
                M.ContainerStatus, M.BLDate, M.CIFilePath,M.AcceptanceDate, M.BankAcceptDate, M.MaturityDate, M.BankRefNumber, M.AttachmentPreviewTemplate;

                ----Childs
                ;With
                SP As (
	                Select ChildID, SubProgramID
	                From {TableNames.YARN_CI_CHILD_YARN_SUBPROGRAM} Where CIID = {id}
                )
                , YDBSP As (
	                Select ChildID
		                , STUFF(
		                (
			                SELECT ', ' + EV.ValueName AS [text()]
			                FROM SP SPI
                            Inner Join {DbNames.EPYSL}..EntityTypeValue EV ON SPI.SubProgramID = EV.ValueID
			                WHERE SPI.ChildID = SP.ChildID
			                FOR XML PATH('')
		                ), 1, 1, '') As YarnSubProgramNames
                        , STUFF(
		                (
			                SELECT ', ' + CAST(SPI.SubProgramID As nvarchar) AS [text()]
			                FROM SP SPI
			                WHERE SPI.ChildID = SP.ChildID
			                FOR XML PATH('')
		                ), 1, 1, '') As YarnSubProgramIDs
	                From SP
	                Group By ChildID
                ),
                YPC As (
	                SELECT C.ChildID, C.CIID, C.ItemMasterID, C.ItemDescription, C.UnitID, C.NoOfCarton, C.GrossWeight, C.NetWeight, C.InvoiceQty, C.Rate, C.PDValue,
					C.PIQty, C.PIValue, C.NoOfCone, C.YarnProgramId, C.ShadeCode
	                FROM {TableNames.YARN_CI_CHILD} C WHERE CIID = {id}
                )
				SELECT YPC.ChildID, YPC.CIID, YPC.ItemMasterID, YPC.ItemDescription, YPC.UnitID, YPC.NoOfCarton, YPC.GrossWeight, YPC.NetWeight, YPC.InvoiceQty, YPC.Rate, YPC.PDValue,
                U.DisplayUnitDesc UOM, YPC.PIQty, YPC.PIValue, YPC.NoOfCone, YPC.YarnProgramId, YDBSP.YarnSubProgramIDs, YDBSP.YarnSubProgramNames, YPC.ShadeCode
                FROM YPC
                INNER JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YPC.UnitID
				LEFT JOIN YDBSP ON YDBSP.ChildID = YPC.ChildID
                GROUP BY YPC.ChildID, YPC.CIID, YPC.ItemMasterID, YPC.ItemDescription, YPC.UnitID, YPC.NoOfCarton, YPC.GrossWeight, YPC.NetWeight, YPC.InvoiceQty, YPC.Rate, YPC.PDValue,
                U.DisplayUnitDesc, YPC.PIQty, YPC.PIValue, YPC.NoOfCone, YPC.YarnProgramId, YDBSP.YarnSubProgramIDs, YDBSP.YarnSubProgramNames, YPC.ShadeCode;

                ----Attached PI
                ;WITH M AS (
	                SELECT CC.ChildPIID, CC.CIID, CC.YPIMasterID  FROM {TableNames.YARN_CI_CHILD}PI CC WHERE CIID = {id}
                ),
                C AS (
	                SELECT YPIReceiveMasterID, SUM(PIQty) TotalQty, SUM(PIValue) TotalValue FROM {TableNames.YarnPIReceiveChild}
	                WHERE YPIReceiveMasterID IN (SELECT YPIMasterID FROM M) GROUP BY YPIReceiveMasterID
                )
                SELECT M.YPIMasterID YPIMasterID, PRM.YPINo PiNo, PRM.PIDate, PRM.SupplierID, Con.Name SupplierName, C.TotalQty, C.TotalValue
                FROM M
                INNER JOIN {TableNames.YarnPIReceiveMaster} PRM ON PRM.YPIReceiveMasterID = M.YPIMasterID
                INNER JOIN C ON C.YPIReceiveMasterID = M.YPIMasterID
				INNER JOIN {DbNames.EPYSL}..Contacts Con ON Con.ContactID = PRM.SupplierID;

                ----Customer
                ;SELECT CAST(CompanyID AS VARCHAR) AS id, CompanyName AS text
                FROM {DbNames.EPYSL}..CompanyEntity
			    WHERE CompanyID IN(8,6)
			    ORDER BY CompanyName;

                ----Payment term
                ;SELECT CAST(PaymentTermsID AS VARCHAR) AS id, PaymentTermsName AS text
                FROM {DbNames.EPYSL}..PaymentTrems
                WHERE PaymentTermsID > 0
                ORDER BY PaymentTermsID;

                ----LC Tenure
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.LC_TENURE} AND ValueName <> 'Select'
                ORDER BY ValueName;

                ----Currency Type
                ;SELECT CAST(CurrencyID AS VARCHAR) AS id, CurrencyCode AS text
                FROM {DbNames.EPYSL}..Currency
                WHERE CurrencyID = 2
                ORDER BY CurrencyID DESC;

                ----Company
                ;SELECT CAST(CompanyID AS VARCHAR) AS id, CompanyName AS text
                FROM {DbNames.EPYSL}..CompanyEntity
			    WHERE CompanyID IN(8,6)
			    ORDER BY CompanyName;

                ----lc tYPE
                ;SELECT CAST(PaymentMethodID AS VARCHAR) AS id, PaymentMethodName AS text
                FROM {DbNames.EPYSL}..PaymentMethod
                WHERE PaymentMethodID > 0
                ORDER BY PaymentMethodID;

                ----yARN uNIT
                ;SELECT CAST(PaymentMethodID AS VARCHAR) AS id, PaymentMethodName AS text
                FROM {DbNames.EPYSL}..PaymentMethod
                WHERE PaymentMethodID > 0
                ORDER BY PaymentMethodID;

                ----LCIssuingBank
                ;SELECT CAST(CompanyBankID AS VARCHAR) id, BranchName text FROM {DbNames.EPYSL}..CompanyBank CB
                INNER JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = CB.BankBranchID
                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = CB.CompanyID;

                ----PaymentBank
                ;SELECT CAST(BB.BankBranchID AS VARCHAR) id, BranchName text FROM {DbNames.EPYSL}..BankBranch BB
                INNER JOIN {DbNames.EPYSL}..BankBranchPaymentType BBPT ON BBPT.BankBranchID = BB.BankBranchID
                INNER JOIN {DbNames.EPYSL}..BankPaymentType_HK BPT ON BPT.PaymentTypeID = BBPT.PaymentTypeID
                WHERE BPT.PaymentTypeID = 5;

                ----ConsigneeList
                ;SELECT CAST(Contacts.ContactID AS VARCHAR) AS id, Contacts.Name AS text
                FROM {DbNames.EPYSL}..Contacts
                INNER JOIN {DbNames.EPYSL}..ContactCategoryChild ON Contacts.ContactID = ContactCategoryChild.ContactID
                INNER JOIN {DbNames.EPYSL}..ContactCategoryHK ON ContactCategoryChild.ContactCategoryID = ContactCategoryHK.ContactCategoryID
                WHERE ContactCategoryHK.ContactCategoryID = {ContactCategoryConstants.CONTACT_CATEGORY_CONSIGNEE}
                ORDER BY Contacts.Name;

                ----NotifyPartyList
                ;SELECT CAST(Contacts.ContactID AS VARCHAR) AS id, Contacts.Name AS text
                FROM {DbNames.EPYSL}..Contacts
                INNER JOIN {DbNames.EPYSL}..ContactCategoryChild ON Contacts.ContactID = ContactCategoryChild.ContactID
                INNER JOIN {DbNames.EPYSL}..ContactCategoryHK ON ContactCategoryChild.ContactCategoryID = ContactCategoryHK.ContactCategoryID
                WHERE ContactCategoryHK.ContactCategoryID = {ContactCategoryConstants.CONTACT_CATEGORY_NOTIFY_PARTY}
                ORDER BY Contacts.Name;

                --All Doc Types
                SELECT DocTypeID = ETV.ValueID, DocTypeName = ETV.ValueName
                FROM {DbNames.EPYSL}..EntityTypeValue ETV
                WHERE ETV.EntityTypeID = 185
                ORDER BY ETV.ValueName;

                --CI Docs
                SELECT CII.*, DocTypeName = ETV.ValueName
                FROM {TableNames.YARN_CI_DOC} CII
                INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = CII.DocTypeID
                WHERE CII.CIID = {id}
                ORDER BY ETV.ValueName
                ";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnCIMaster data = await records.ReadFirstOrDefaultAsync<YarnCIMaster>();
                Guard.Against.NullObject(data);
                data.CIChilds = records.Read<YarnCIChild>().ToList();
                data.CIChildPIs = records.Read<YarnCIChildPI>().ToList();
                data.CustomerList = records.Read<Select2OptionModel>().ToList();
                data.PaymentTermsList = records.Read<Select2OptionModel>().ToList();
                data.LCTenureList = records.Read<Select2OptionModel>().ToList();
                data.CurrencyTypeList = records.Read<Select2OptionModel>().ToList();
                data.CompanyList = records.Read<Select2OptionModel>().ToList();
                data.LCTypeList = records.Read<Select2OptionModel>().ToList();
                data.YarnUnitList = records.Read<Select2OptionModel>().ToList();
                data.LCIssuingBankList = records.Read<Select2OptionModel>().ToList();
                data.PaymentBankList = records.Read<Select2OptionModel>().ToList();
                data.ConsigneeList = records.Read<Select2OptionModel>().ToList();
                data.NotifyPartyList = records.Read<Select2OptionModel>().ToList();
                data.AllDocTypes = records.Read<YarnCIDoc>().ToList();
                data.YarnCIDocs = records.Read<YarnCIDoc>().ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }

        public async Task<YarnCIMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * From {TableNames.YARN_CI_MASTER} Where CIID = {id}

            ;Select * From {TableNames.YARN_CI_CHILD} Where CIID = {id}

            ;Select * From {TableNames.YARN_CI_CHILD_PI} Where CIID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnCIMaster data = await records.ReadFirstOrDefaultAsync<YarnCIMaster>();
                Guard.Against.NullObject(data);
                data.CIChilds = records.Read<YarnCIChild>().ToList();
                data.CIChildPIs = records.Read<YarnCIChildPI>().ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }

        public async Task SaveAsync(YarnCIMaster entity)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;

            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _gmtConnection.OpenAsync();
                transactionGmt = _gmtConnection.BeginTransaction();

                int maxChildId = 0;
                int maxPIId = 0;
                int maxSubProgramId = 0;
                int maxDocId = 0;

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.CIID = await _service.GetMaxIdAsync(TableNames.YARN_CI_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);
                        //entity.CINo = await _service.GetMaxNoAsync(TableNames.YARN_CI_MASTER_NO);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_CI_CHILD, entity.CIChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);
                        maxPIId = await _service.GetMaxIdAsync(TableNames.YARN_CI_CHILD_PI, entity.CIChildPIs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);
                        maxSubProgramId = await _service.GetMaxIdAsync(TableNames.YARN_CI_CHILD_YARN_SUBPROGRAM, entity.CIChilds.Sum(x => x.SubPrograms.Count()), RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);

                        maxDocId = await _service.GetMaxIdAsync(TableNames.YARN_CI_DOC, entity.YarnCIDocs.Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);

                        foreach (var item in entity.CIChilds)
                        {
                            item.ChildID = maxChildId++;
                            item.CIID = entity.CIID;
                            foreach (var subProgram in item.SubPrograms)
                            {
                                subProgram.YarnCIChildSubProgramID = maxSubProgramId++;
                                subProgram.CIID = entity.CIID;
                                subProgram.ChildID = item.ChildID;
                            }
                        }

                        foreach (var item in entity.CIChildPIs)
                        {
                            item.ChildPIID = maxPIId++;
                            item.CIID = entity.CIID;
                        }

                        foreach (var item in entity.YarnCIDocs)
                        {
                            item.YarnCIDocID = maxDocId++;
                            item.CIID = entity.CIID;
                        }

                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.CIChilds.FindAll(x => x.EntityState == EntityState.Added);
                        var addedChildPIs = entity.CIChildPIs.FindAll(x => x.EntityState == EntityState.Added);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_CI_CHILD, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);
                        maxPIId = await _service.GetMaxIdAsync(TableNames.YARN_CI_CHILD_PI, entity.CIChildPIs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);
                        maxSubProgramId = await _service.GetMaxIdAsync(TableNames.YARN_CI_CHILD_YARN_SUBPROGRAM, entity.CIChilds.Sum(x => x.SubPrograms.Count()), RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);
                        maxDocId = await _service.GetMaxIdAsync(TableNames.YARN_CI_DOC, entity.YarnCIDocs.FindAll(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);

                        foreach (var item in addedChilds)
                        {
                            item.ChildID = maxChildId++;
                            item.CIID = entity.CIID;
                            foreach (var subProgram in item.SubPrograms)
                            {
                                subProgram.YarnCIChildSubProgramID = maxSubProgramId++;
                                subProgram.CIID = entity.CIID;
                                subProgram.ChildID = item.ChildID;
                            }
                        }

                        foreach (var item in addedChildPIs)
                        {
                            item.ChildPIID = maxPIId++;
                            item.CIID = entity.CIID;
                        }

                        foreach (var item in entity.YarnCIDocs.FindAll(x => x.EntityState == EntityState.Added))
                        {
                            item.YarnCIDocID = maxDocId++;
                            item.CIID = entity.CIID;
                        }

                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.CIChilds.SetDeleted();
                        entity.CIChilds.ForEach(x => x.SubPrograms.SetDeleted());
                        entity.CIChildPIs.SetDeleted();
                        break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.CIChilds, transaction);
                List<YarnCIChildYarnSubProgram> subPrograms = new List<YarnCIChildYarnSubProgram>();
                entity.CIChilds.ForEach(x => subPrograms.AddRange(x.SubPrograms));
                await _service.SaveAsync(subPrograms, transaction);
                await _service.SaveAsync(entity.CIChildPIs, transaction);
                await _service.SaveAsync(entity.YarnCIDocs, transaction);
                transaction.Commit();
                transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (transactionGmt != null) transactionGmt.Rollback();
                throw ex;
            }
            finally
            {
                _connection.Close();
                _gmtConnection.Close();
            }
        }

        public async Task UpdateEntityAsync(YarnCIMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _service.SaveSingleAsync(entity, transaction);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<List<YarnCIMaster>> GetItemDetails(string itemIds)
        {
            var sql = $@"
                 -----for DyeingBatchItem
                ;WITH M AS 
                (
                    SELECT * FROM {TableNames.YarnPIReceiveMaster} BM WHERE BM.YPIReceiveMasterID IN ({itemIds})
                )
                Select M.YPIReceiveMasterID YPIReceiveMasterID, M.RevisionNo,M.SupplierID, M.Remarks, M.ReceivePI, 
                M.ReceivePIBy,M.ReceivePIDate, M.TypeOfLCID 
                From M ;
                -----for DyeingBatchItem
                ;WITH M AS 
                (
	                SELECT YPIReceiveMasterID FROM {TableNames.YarnLCChild} WHERE YPIReceiveMasterID IN ({itemIds})
	             )
                SELECT PIRC.ItemMasterID, IM.ItemName ItemDescription, PIRC.UnitID UnitID, U.DisplayUnitDesc UOM, 
                SUM(PIRC.PIQty) PIQty, SUM(PIRC.PIValue) PIValue, SUM(PIRC.PIQty) InvoiceQty, SUM(PIRC.PIValue) PdValue, 
                AVG(PIRC.Rate) Rate, PIRC.YarnProgramId, PIRC.ShadeCode
                FROM M
                INNER JOIN {TableNames.YarnPIReceiveChild} PIRC ON PIRC.YPIReceiveMasterID = M.YPIReceiveMasterID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = PIRC.ItemMasterID
                INNER JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = PIRC.UnitID
                GROUP BY PIRC.ItemMasterID, IM.ItemName, PIRC.UnitID, U.DisplayUnitDesc, PIRC.YarnProgramId,PIRC.ShadeCode; ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnCIMaster> data = records.Read<YarnCIMaster>().ToList();
                List<YarnCIChild> CIChilds = records.Read<YarnCIChild>().ToList();
                foreach (YarnCIMaster item in data)
                {
                    item.CIChilds = CIChilds.Where(x => x.CIID == item.CIID).ToList();

                }

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task<List<YarnCIMaster>> createBankAcceptance(string nCIIDs, string companyIDs, string supplierIDs, string bankBranchIDs)
        {
            string sql = String.Empty;
            sql += $@"Select CM.CIID, CM.CINo, CM.CIDate,LC.LCNo,LC.LCDate,CIValue,LC.LCValue,CM.AcceptanceDate,
                    CM.BankAccept,CM.BankRefNumber,CM.BankAcceptDate,CM.MaturityDate
                    From {TableNames.YARN_CI_MASTER} CM 
                    INNER JOIN {TableNames.YarnLCMaster} LC ON LC.LCID = CM.LCID
                    Where CM.CIID in({nCIIDs}) And LC.CompanyId in({companyIDs}) And LC.SupplierId in({supplierIDs}) And LC.IssueBankID in({bankBranchIDs})
                    And CM.Acceptance = 1 And CM.BankAccept = 0 ;

                    -- Acceptance Charge
                    Select 
                    a.*,b.CategoryName
                    From {DbNames.EPYSL}..CommercialSourceGroupHead a
                    INNER JOIN  {DbNames.EPYSL}..CommercialTransactionCategory b on b.CTCategoryID = a.CTCategoryID
                    Where b.CategoryName in ('Acceptance Charge')
                    Order By CTCategoryID;

                    Select CSH.SHeadID as id, CSH.SHeadName as text,CSHS.SGHeadID
                    From {DbNames.EPYSL}..CommercialSourceHeadSetup CSHS 
                    INNER JOIN  {DbNames.EPYSL}..CommercialSourceHead CSH on CSHS.SHeadID  = CSH.SHeadID
                    --Where  CSHS.CompanyID in({companyIDs}) and CSHS.BankBranchID in({bankBranchIDs})  --and CSHS.SGHeadID = {2} and CSHS.CTCategoryID ={3} 
                    Group By  CSH.SHeadID,CSH.SHeadName,CSHS.SGHeadID;

                    Select CSH.SHeadID AS id, CSH.SHeadName AS text,CSHS.SGHeadID
                    From {DbNames.EPYSL}..CommercialSourceHeadSetup CSHS 
                    INNER JOIN  {DbNames.EPYSL}..CommercialSourceHead CSH on CSHS.SHeadID  = CSH.SHeadID
                    INNER JOIN  {DbNames.EPYSL}..CommercialTransactionCategory CT on CT.CTCategoryID = CSHS.CTCategoryID
                    Where CT.CategoryName='Sources' --And CSHS.CompanyID in({companyIDs}) and CSHS.BankBranchID in({bankBranchIDs}) -- and CSHS.SGHeadID = {2} 
                    Group By  CSH.SHeadID,CSH.SHeadName,CSHS.SGHeadID;";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnCIMaster> data = records.Read<YarnCIMaster>().ToList();
                List<ImportDocumentAcceptanceChargeDetails> IDACDetails = records.Read<ImportDocumentAcceptanceChargeDetails>().ToList();
                var HeadDescriptionList = records.Read<Select2OptionModel>().ToList();
                var SHeadNameList = records.Read<Select2OptionModel>().ToList();
                foreach (YarnCIMaster item in data)
                {
                    item.IDACDetails = IDACDetails;
                    item.HeadDescriptionList = HeadDescriptionList;
                    item.SHeadNameList = SHeadNameList;

                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task<List<YarnCIMaster>> GetAllCIInfoByIDAsync(string id, string BankRefNumber)
        {
            string sql = String.Empty;
            sql += $@"Select CM.*
                    From {TableNames.YARN_CI_MASTER} CM 
                    INNER JOIN {TableNames.YarnLCMaster} LC ON LC.LCID = CM.LCID
                    Where CM.CIID in({id}) And CM.Acceptance = 1 And CM.BankAccept = 0;

                    Select HH.*
                    From {TableNames.ImportDocumentAcceptanceChargeDetails} HH
                    Where HH.BankRefNo = '{BankRefNumber}';";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnCIMaster> data = records.Read<YarnCIMaster>().ToList();
                var IDACDetails = records.Read<ImportDocumentAcceptanceChargeDetails>().ToList();
                data.ForEach(item => { item.IDACDetails = IDACDetails; });

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }

        }
        public async Task UpdateMultiEntityAsync(List<YarnCIMaster> entityList)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                int maxADetailsID = 0;
                maxADetailsID = _service.GetMaxId(TableNames.ImportDocumentAcceptanceChargeDetails, entityList.FirstOrDefault().IDACDetails.Count(x => x.EntityState == EntityState.Added));

                entityList.FirstOrDefault().
                    IDACDetails.Where(x => x.EntityState == EntityState.Added).ToList().
                    ForEach(oItem => {
                        oItem.ADetailsID = maxADetailsID++;
                        oItem.BankRefNo = entityList.FirstOrDefault().BankRefNumber;
                    });
                await _service.SaveAsync(entityList, transaction);
                await _service.SaveAsync(entityList.FirstOrDefault().IDACDetails.ToList(), transaction);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

    }
}

using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using EPYSLTEXCore.Interfaces.Services;
using Microsoft.Data.SqlClient;

namespace EPYSLTEX.Infrastructure.Services
{
    public class YarnPIReviewService : IYarnPIReviewService
    {
        private readonly IDapperCRUDService<YarnPIReceiveMaster> _service;
        private readonly SqlConnection _connection;

        public YarnPIReviewService(IDapperCRUDService<YarnPIReceiveMaster> service )
        {
            _service = service;
            service.Connection= service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YarnPIReceiveMaster>> GetPagedAsync(Status status, bool isCDAPage, PaginationInfo paginationInfo)
        {
            string orderBy;
            string sql;
            if (status == Status.Pending)
            {
                sql = $@"
                    ;With
                    YPI AS
                    (
	                    SELECT YPRM.YPIReceiveMasterID, YPRM.YPINo, YPRM.PONo, CONVERT(varchar, YPRM.PIDate, 101) PIDate, 
                        CC.Name AS SupplierName, YPRM.PIFilePath, YPRM.AttachmentPreviewTemplate, YPRM.CompanyID, 
                        CE.ShortName As CompanyName, YPRM.SupplierID, SUM(YPRC.POQty) AS POQty, SUM(YPRC.PIQty) PIQty, 
	                    SUM(YPRC.PIValue) AS PIValue, YPRM.NeedsReview, YPRM.Accept, YPRM.Reject, YPRM.RejectReason, POAddedByName = LU.Name
	                    FROM {TableNames.YarnPIReceiveMaster} YPRM
	                    INNER JOIN {TableNames.YarnPIReceiveChild} YPRC ON YPRC.YPIReceiveMasterID = YPRM.YPIReceiveMasterID
	                    INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = YPRM.SupplierID
                        INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPRM.CompanyID 
                        INNER JOIN {TableNames.YarnPIReceivePO} YPIPO ON YPRM.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
						INNER JOIN {TableNames.YarnPOMaster} YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID 
                        Left JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPO.AddedBy
	                    WHERE ISNULL(Acknowledge,0) = 1 And ISNULL(Accept,0) = 0 And ISNULL(Reject,0) = 0 AND IsCDA = '{isCDAPage}'
                        --AND YPO.RevisionNo = YPRM.PreProcessRevNo
                        
	                    GROUP BY YPRM.YPIReceiveMasterID, YPRM.YPINo, YPRM.PONo, YPRM.PIDate, CC.Name, YPRM.PIFilePath, YPRM.AttachmentPreviewTemplate, 
                        YPRM.CompanyID, CE.ShortName, YPRM.SupplierID, YPRM.NeedsReview, YPRM.Accept, YPRM.Reject, YPRM.RejectReason,LU.Name
                    )
                    SELECT YPIReceiveMasterID, YPINo, PONo, PIDate, SupplierName, PIFilePath, AttachmentPreviewTemplate, 
                    POQty, PIQty, PIValue, CompanyID, CompanyName, SupplierID, NeedsReview, Accept, Reject, RejectReason, POAddedByName,
                    Count(*) Over() TotalRows
                    FROM YPI";

                orderBy = "ORDER BY CONVERT(DATETIME,PIDate) DESC";
            }
            else if (status == Status.Completed)
            {
                sql = $@"
                    ;With
                    YPI AS
                    (
	                    SELECT M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, CC.Name AS SupplierName, M.PIFilePath, 
                        M.AttachmentPreviewTemplate, M.CompanyID, M.SupplierID, SUM(C.POQty) AS POQty, SUM(C.PIQty) PIQty, SUM(C.PIValue) AS PIValue, 
                        M.NeedsReview, M.Accept, M.Reject, M.RejectReason
	                    FROM {TableNames.YarnPIReceiveMaster} M
	                    INNER JOIN {TableNames.YarnPIReceiveChild} C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                    INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
						WHERE Accept= 0 AND Acknowledge= 0 AND ISNULL(UnAcknowledge,0) = 0 AND IsCDA = '{isCDAPage}' 
	                    GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, CC.Name, M.PIFilePath, M.AttachmentPreviewTemplate, M.CompanyID, M.SupplierID,
						M.NeedsReview, M.Accept, M.Reject, M.RejectReason
                    )
                    SELECT YPIReceiveMasterID, YPINo, PIDate, SupplierName, PIFilePath, AttachmentPreviewTemplate, POQty, PIQty, PIValue, 
                    CompanyID, SupplierID, NeedsReview, Accept, Reject, RejectReason, Count(*) Over() TotalRows
                    FROM YPI";

                orderBy = "ORDER BY CONVERT(DATETIME,PIDate) DESC";
            }
            else if (status == Status.Approved)
            {
                sql = $@"
                ;With 
                RVSList AS
                (
                SELECT YPI.YPIReceivePOID, YPIReceiveMasterID
                FROM {TableNames.YarnPIReceivePO} YPI
                INNER JOIN {TableNames.YarnPOMaster} YPO ON YPO.YPOMasterID = YPI.YPOMasterID
                WHERE YPI.RevisionNo <> YPO.RevisionNo
                ),  
                YPI AS
                (
	                SELECT M.YPIReceiveMasterID, M.YPINo, M.PONo, CONVERT(varchar, M.PIDate, 101) PIDate, CC.Name AS SupplierName,
                    M.PIFilePath, M.AttachmentPreviewTemplate, M.CompanyID, CE.ShortName As CompanyName, M.SupplierID, SUM(C.POQty) AS POQty, 
                    SUM(C.PIQty) PIQty, SUM(C.PIValue) AS PIValue, M.NeedsReview, M.Accept, M.Reject, M.RejectReason,M.Acknowledge, POAddedByName = LU.Name
	                FROM {TableNames.YarnPIReceiveMaster} M
	                INNER JOIN {TableNames.YarnPIReceiveChild} C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                    INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID
                    INNER JOIN {TableNames.YarnPIReceivePO} YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
                    INNER JOIN {TableNames.YarnPOMaster} YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID
                    LEFT JOIN RVSList RV ON RV.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
                    Left JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPO.AddedBy
					WHERE AcceptDate IS Not Null AND ISNULL(UnAcknowledge,0) = 0 AND IsCDA = 'False'
					--Accept= 1 AND Acknowledge= 1 AND 
                    --AND YPO.RevisionNo = YPIPO.RevisionNo AND RV.YPIReceivePOID IS NULL
                    --AND YPO.RevisionNo = M.PreProcessRevNo
	                GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PONo, M.PIDate, CC.Name, M.PIFilePath, M.AttachmentPreviewTemplate, 
                    M.CompanyID, CE.ShortName, M.SupplierID, M.NeedsReview, M.Accept, M.Reject, M.RejectReason,M.Acknowledge,LU.Name
                )
                SELECT YPIReceiveMasterID, YPINo, PONo, PIDate, SupplierName, PIFilePath, AttachmentPreviewTemplate, POQty, PIQty,POAddedByName, 
                PIValue, CompanyID, CompanyName, SupplierID, NeedsReview, Accept,Acknowledge, Reject, RejectReason, Count(*) Over() TotalRows
                FROM YPI";

                orderBy = "ORDER BY CONVERT(DATETIME,PIDate) DESC";
            }
            else if (status == Status.Acknowledge)
            {
                sql = $@"
                    ;With
                    YPI AS
                    (
	                    SELECT M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, CC.Name AS SupplierName, M.PIFilePath, 
                        M.AttachmentPreviewTemplate, M.CompanyID, M.SupplierID, SUM(C.POQty) AS POQty, SUM(C.PIQty) PIQty, SUM(C.PIValue) AS PIValue, 
                        M.NeedsReview, M.Accept, M.Reject, M.RejectReason
	                    FROM {TableNames.YarnPIReceiveMaster} M
	                    INNER JOIN {TableNames.YarnPIReceiveChild} C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                    INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
						WHERE ISNULL(Acknowledge,0) = 1 AND IsCDA = '{isCDAPage}'
	                    GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, CC.Name, M.PIFilePath, M.AttachmentPreviewTemplate, M.CompanyID, M.SupplierID,
						M.NeedsReview, M.Accept, M.Reject, M.RejectReason
                    )
                    SELECT YPIReceiveMasterID, YPINo, PIDate, SupplierName, PIFilePath, AttachmentPreviewTemplate, POQty, PIQty, PIValue, 
                    CompanyID, SupplierID, NeedsReview, Accept, Reject, RejectReason, Count(*) Over() TotalRows
                    FROM YPI";

                orderBy = "ORDER BY CONVERT(DATETIME,PIDate) DESC";
            }
            else if (status == Status.UnAcknowledge)
            {
                sql = $@"
                    ;With
                    YPI AS(
	                    SELECT M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, CC.Name AS SupplierName, M.PIFilePath, 
                        M.AttachmentPreviewTemplate, M.CompanyID, M.SupplierID, SUM(C.POQty) AS POQty, SUM(C.PIQty) PIQty, SUM(C.PIValue) AS PIValue, 
                        M.NeedsReview, M.Accept, M.Reject, M.RejectReason
	                    FROM {TableNames.YarnPIReceiveMaster} M
	                    INNER JOIN {TableNames.YarnPIReceiveChild} C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                    INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
						WHERE ISNULL(UnAcknowledge,0) = 1 AND IsCDA = '{isCDAPage}'
	                    GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, CC.Name, M.PIFilePath, M.AttachmentPreviewTemplate, M.CompanyID, SupplierID,
						M.NeedsReview, M.Accept, M.Reject, M.RejectReason,LU.Name
                    )
                    SELECT YPIReceiveMasterID, YPINo, PIDate, SupplierName, PIFilePath, AttachmentPreviewTemplate, POQty, PIQty, PIValue, CompanyID, SupplierID,
					NeedsReview, Accept, Reject, RejectReason, Count(*) Over() TotalRows
                    FROM YPI";

                orderBy = "ORDER BY CONVERT(DATETIME,PIDate) DESC";
            }
            else if (status == Status.Revise)
            {
                sql = $@"
                With TmpYR As 
                (
	                SELECT M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, M.CompanyID, CE.ShortName As CompanyName, 
	                M.SupplierID, M.PONo, CC.Name AS SupplierName, M.PIFilePath, M.AttachmentPreviewTemplate,   
	                C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue, POAddedByName = LU.Name
	                FROM {TableNames.YarnPIReceiveMaster} M
	                INNER JOIN {TableNames.YarnPIReceiveChild} C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                INNER JOIN {TableNames.YarnPIReceivePO} YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                INNER JOIN {TableNames.YarnPOMaster} YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID  
	                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID  
                    Left JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPO.AddedBy
	                WHERE M.IsCDA = 0 --AND M.UnAcknowledge = 0 --And M.YPINo = 'az-200422'
                    AND M.Accept=0 
                    --AND YPO.RevisionNo <> YPIPO.RevisionNo
					AND M.RevisionNo>0 AND ReceivePI=1 --AND M.Acknowledge=0
	                GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, M.CompanyID, CE.ShortName, M.SupplierID, M.PONo, CC.Name,POAddedByName,
	                M.PIFilePath, M.AttachmentPreviewTemplate, C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue,YPIPO.RevisionNo,YPO.RevisionNo ,LU.Name
                ),
                TmpYRC As 
                (
	                Select YPIReceiveMasterID, YPINo, PIDate, CompanyID, CompanyName, SupplierID, SupplierName, PONo, PIFilePath, 
	                AttachmentPreviewTemplate,  
	                SUM(POQty)POQty, SUM(Rate)Rate, SUM(PIQty)PIQty, SUM(PIValue)PIValue 
	                From TmpYR
	                Group By YPIReceiveMasterID, YPINo, PIDate, CompanyID, CompanyName, SupplierID, SupplierName, PONo, PIFilePath, 
	                AttachmentPreviewTemplate 
                )
                Select *, Count(*) Over() TotalRows From TmpYRC";

                orderBy = "ORDER BY YPIReceiveMasterID DESC";
            }
            else
            {
                sql = $@"
                ;With YPI AS
                (
	                SELECT M.YPIReceiveMasterID, M.YPINo, M.PONo, CONVERT(varchar, M.PIDate, 101) PIDate, CC.Name AS SupplierName, 
                    M.PIFilePath, M.AttachmentPreviewTemplate, M.CompanyID, CE.ShortName As CompanyName, M.SupplierID, 
                    SUM(C.POQty) AS POQty, SUM(C.PIQty) PIQty, SUM(C.PIValue) AS PIValue, M.NeedsReview, M.Accept, M.Reject, M.RejectReason, POAddedByName = LU.Name
	                FROM {TableNames.YarnPIReceiveMaster} M
	                INNER JOIN {TableNames.YarnPIReceiveChild} C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                    INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID 
                    INNER JOIN {TableNames.YarnPIReceivePO} YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
				    INNER JOIN {TableNames.YarnPOMaster} YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID 
                    Left JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPO.AddedBy
					WHERE ISNULL(M.NeedsReview,0) = 0 AND ISNULL(Reject,0) = 1 AND IsCDA = 'False'
                    --AND YPO.RevisionNo = YPRM.PreProcessRevNo
	                GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PONo, M.PIDate, CC.Name, M.PIFilePath, M.AttachmentPreviewTemplate, 
                    M.CompanyID, CE.ShortName, M.SupplierID, M.NeedsReview, M.Accept, M.Reject, M.RejectReason,LU.Name
                )
                SELECT YPIReceiveMasterID, YPINo, PONo, PIDate, SupplierName, PIFilePath, AttachmentPreviewTemplate, 
                POQty, PIQty, PIValue, CompanyID, CompanyName, SupplierID, NeedsReview, Accept, Reject, RejectReason, POAddedByName,
                Count(*) Over() TotalRows
                FROM YPI";

                orderBy = "ORDER BY CONVERT(DATETIME,PIDate) DESC";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnPIReceiveMaster>(sql);
        }

        public async Task<YarnPIReceiveMaster> GetAsync(int id, int supplierId, int companyId, bool isCDAPage)
        {
            var query = "";
            if (isCDAPage)
            {
                query =
                $@"
                ;WITH M
                AS (
	                SELECT YPIReceiveMasterID, TypeOfLCID, YPINo, PIDate, SupplierID, NetPIValue, ShippingTolerance, 
                    NeedsReview, Accept, Reject, RejectReason, CreditDays,Acknowledge,UnAcknowledge
					FROM {TableNames.YarnPIReceiveMaster} WHERE YPIReceiveMasterID = {id}
                ),
                C AS (
	                SELECT YPIReceiveMasterID, SUM(POQty)POQty, CONVERT(decimal(18,2), SUM(POQty * Rate)) POValue, SUM(PIValue) PIValue, SUM(PIQty) PIQty 
                    FROM {TableNames.YarnPIReceiveChild} WHERE YPIReceiveMasterID = {id}
	                GROUP BY YPIReceiveMasterID
                ),
                A AS (
	                SELECT YarnPIReceiveAdditionalValue.YPIReceiveMasterID, SUM(AdditionalValue) TotalAddValue
                    FROM YarnPIReceiveAdditionalValue WHERE YPIReceiveMasterID = {id}
	                GROUP BY YPIReceiveMasterID
                ),
                D AS (
	                SELECT YPIReceiveMasterID, SUM(DeductionValue) TotalDeductionValue 
                    FROM YarnPIReceiveDeductionValue WHERE YPIReceiveMasterID = {id}
	                GROUP BY YPIReceiveMasterID
                )
                SELECT M.YPIReceiveMasterID, M.YPINo, TypeOfLCID, M.PIDate, M.ShippingTolerance, M.SupplierID, 
                CC.ShortName SupplierName, POQty, POValue, C.PIQty, Cast(Round(C.PIValue,2) as Decimal(10,2))PIValue, 
                M.NetPIValue, PM.PaymentMethodName TypeOfLC, A.TotalAddValue, D.TotalDeductionValue, M.NeedsReview, 
                M.Accept, M.Reject, M.RejectReason, CR.ValueName CreditDays,M.Acknowledge,M.UnAcknowledge
                FROM M
                INNER JOIN C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
                LEFT JOIN A ON A.YPIReceiveMasterID = M.YPIReceiveMasterID
                LEFT JOIN D ON D.YPIReceiveMasterID = M.YPIReceiveMasterID
                LEFT JOIN {DbNames.EPYSL}..PaymentMethod PM ON PM.PaymentMethodID = M.TypeOfLCID
                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue CR ON CR.ValueID = M.CreditDays;

                ;With
                PO As (
	                Select YPIReceivePOID, YPIReceiveMasterID, YPOMasterID
	                From {TableNames.YarnPIReceivePO}
	                Where YPIReceiveMasterID = {id}
                )
                Select PO.YPIReceivePOID, POM.PONo,  PO.YPIReceiveMasterID, PO.YPOMasterID, POM.RevisionNo, 
                POM.QuotationRefNo, CE.ShortName CompanyName, SUM(POC.POQty) TotalQty, 
                Cast(Round(SUM(POC.POQty * POC.Rate),2) as Decimal(10,2)) TotalValue 
                From PO
                Inner Join CDAPOMaster POM On POM.CDAPOMasterID = PO.YPOMasterID
                Inner Join CDAPOChild POC On PO.YPOMasterID = POC.CDAPOMasterID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On POM.CompanyID = CE.CompanyID
                Group By PO.YPIReceivePOID, POM.PONo, PO.YPIReceiveMasterID, PO.YPOMasterID, POM.RevisionNo, 
                POM.QuotationRefNo, CE.ShortName;

                ;WITH M
                AS (
	                SELECT CDAPOMasterID YPOMasterID, TypeOfLCID, ShippingTolerance, CreditDays
	                FROM CDAPOMaster 
	                WHERE CDAPOMasterID IN (Select YPOMasterID From {TableNames.YarnPIReceivePO} Where YPIReceiveMasterID = {id})
                ),
                C AS (
	                SELECT CDAPOMasterID YPOMasterID, SUM((POQty*Rate)) TotalValue, SUM(POQty) TotalQty 
	                FROM CDAPOChild
	                WHERE CDAPOMasterID IN (Select YPOMasterID From {TableNames.YarnPIReceivePO} Where YPIReceiveMasterID = {id})
	                GROUP BY CDAPOMasterID
                ),
                PM As (
	                Select * From {DbNames.EPYSL}..PaymentMethod Where PaymentMethodID <> 0
                ),
                TmpSum As 
                (
                    SELECT Max(M.TypeOfLCID)TypeOfLCID, Max(M.ShippingTolerance)ShippingTolerance, --SUM(C.TotalValue)TotalValue, 
                    Cast(Round(SUM(C.TotalValue),2)as Decimal(10,2))TotalValue,
                    SUM(C.TotalQty)TotalQty, MAx(PM.PaymentMethodName) TypeOfLC, Max(CR.ValueName) CreditDays
                    FROM M
                    INNER JOIN C ON C.YPOMasterID = M.YPOMasterID
                    LEFT JOIN PM ON PM.PaymentMethodID = M.TypeOfLCID
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue CR ON CR.ValueID = M.CreditDays
                )
                Select * From TmpSum; 


                /*;With
                PO As (
	                Select YPIReceivePOID, YPIReceiveMasterID, YPOMasterID
	                From {TableNames.YarnPIReceivePO}
	                Where YPIReceiveMasterID = {id}
                )
                Select PO.YPIReceivePOID, POM.PONo,  PO.YPIReceiveMasterID, PO.YPOMasterID, POM.RevisionNo, POM.QuotationRefNo, CE.ShortName CompanyName
	                , SUM(POC.POQty) TotalQty, SUM(POC.POQty * POC.Rate) TotalValue
                From PO
                Inner Join CDAPOMaster POM On PO.YPOMasterID = POM.CDAPOMasterID
                Inner Join CDAPOChild POC On PO.YPOMasterID = POC.CDAPOMasterID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On POM.CompanyID = CE.CompanyID
                Group By PO.YPIReceivePOID, POM.PONo, PO.YPIReceiveMasterID, PO.YPOMasterID, POM.RevisionNo, POM.QuotationRefNo, CE.ShortName;

                ;WITH M
                AS (
	                SELECT CDAPOMasterID, TypeOfLCID, ShippingTolerance, CreditDays
					FROM CDAPOMaster WHERE CDAPOMasterID IN (Select YPOMasterID From {TableNames.YarnPIReceivePO} Where YPIReceiveMasterID = {id})
                ),
                C AS (
	                SELECT CDAPOMasterID, SUM((POQty*Rate)) TotalValue, SUM(POQty) TotalQty FROM CDAPOChild
					WHERE CDAPOMasterID IN (Select YPOMasterID From {TableNames.YarnPIReceivePO} Where YPIReceiveMasterID = {id})
	                GROUP BY CDAPOMasterID
                )
				, PM As (
					Select * From {DbNames.EPYSL}..PaymentMethod Where PaymentMethodID <> 0
				)
                SELECT M.CDAPOMasterID YPOMasterID, M.TypeOfLCID, M.ShippingTolerance, C.TotalValue, C.TotalQty, PM.PaymentMethodName TypeOfLC, CR.ValueName CreditDays
                FROM M
                INNER JOIN C ON C.CDAPOMasterID = M.CDAPOMasterID
                LEFT JOIN PM ON PM.PaymentMethodID = M.TypeOfLCID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue CR ON CR.ValueID = M.CreditDays; */ ";
            }
            else
            {
                query =
                $@"
                ;WITH M
                AS (
	                SELECT YPIReceiveMasterID, TypeOfLCID, YPINo, PIDate, SupplierID, NetPIValue, ShippingTolerance, 
                    NeedsReview, Accept, Reject, RejectReason, CreditDays,Acknowledge,UnAcknowledge
					FROM {TableNames.YarnPIReceiveMaster} WHERE YPIReceiveMasterID = {id}
                ),
                C AS (
	                SELECT YPIReceiveMasterID, SUM(POQty)POQty, CONVERT(decimal(18,2), SUM(POQty * Rate)) POValue, SUM(PIValue) PIValue, SUM(PIQty) PIQty 
                    FROM {TableNames.YarnPIReceiveChild} WHERE YPIReceiveMasterID = {id}
	                GROUP BY YPIReceiveMasterID
                ),
                A AS (
	                SELECT YarnPIReceiveAdditionalValue.YPIReceiveMasterID, SUM(AdditionalValue) TotalAddValue
                    FROM YarnPIReceiveAdditionalValue WHERE YPIReceiveMasterID = {id}
	                GROUP BY YPIReceiveMasterID
                ),
                D AS (
	                SELECT YPIReceiveMasterID, SUM(DeductionValue) TotalDeductionValue 
                    FROM YarnPIReceiveDeductionValue WHERE YPIReceiveMasterID = {id}
	                GROUP BY YPIReceiveMasterID
                )
                SELECT M.YPIReceiveMasterID, M.YPINo, TypeOfLCID, M.PIDate, M.ShippingTolerance, M.SupplierID, 
                CC.ShortName SupplierName, POQty, POValue, C.PIQty, Cast(Round(C.PIValue,2) as Decimal(10,2))PIValue, 
                M.NetPIValue, PM.PaymentMethodName TypeOfLC, A.TotalAddValue, D.TotalDeductionValue, M.NeedsReview, 
                M.Accept, M.Reject, M.RejectReason, CR.ValueName CreditDays,M.Acknowledge,M.UnAcknowledge
                FROM M
                INNER JOIN C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
                LEFT JOIN A ON A.YPIReceiveMasterID = M.YPIReceiveMasterID
                LEFT JOIN D ON D.YPIReceiveMasterID = M.YPIReceiveMasterID
                LEFT JOIN {DbNames.EPYSL}..PaymentMethod PM ON PM.PaymentMethodID = M.TypeOfLCID
                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue CR ON CR.ValueID = M.CreditDays;

                ;With
                PO As (
	                Select YPIReceivePOID, YPIReceiveMasterID, YPOMasterID
	                From {TableNames.YarnPIReceivePO}
	                Where YPIReceiveMasterID = {id}
                )
                Select PO.YPIReceivePOID, POM.PONo,  PO.YPIReceiveMasterID, PO.YPOMasterID, POM.RevisionNo, 
                POM.QuotationRefNo, CE.ShortName CompanyName, SUM(POC.POQty) TotalQty, 
                Cast(Round(SUM(POC.POQty * POC.Rate),2) as Decimal(10,2)) TotalValue
                --Round(SUM(POC.POQty * POC.Rate),2) TotalValue
                From PO
                Inner Join {TableNames.YarnPOMaster} POM On PO.YPOMasterID = POM.YPOMasterID
                Inner Join {TableNames.YarnPOChild} POC On PO.YPOMasterID = POC.YPOMasterID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On POM.CompanyID = CE.CompanyID
                Group By PO.YPIReceivePOID, POM.PONo, PO.YPIReceiveMasterID, PO.YPOMasterID, POM.RevisionNo, 
                POM.QuotationRefNo, CE.ShortName;

                ;WITH M
                AS (
	                SELECT YPOMasterID, TypeOfLCID, ShippingTolerance, CreditDays
					FROM {TableNames.YarnPOMaster} WHERE YPOMasterID IN (Select YPOMasterID From {TableNames.YarnPIReceivePO} Where YPIReceiveMasterID = {id})
                ),
                C AS (
	                SELECT YPOMasterID, SUM((POQty*Rate)) TotalValue, SUM(POQty) TotalQty FROM {TableNames.YarnPOChild}
					WHERE YPOMasterID IN (Select YPOMasterID From {TableNames.YarnPIReceivePO} Where YPIReceiveMasterID = {id})
	                GROUP BY YPOMasterID
                ),
				PM As (
					Select * From {DbNames.EPYSL}..PaymentMethod Where PaymentMethodID <> 0
				),
                TmpSum As 
                (
                    SELECT Max(M.TypeOfLCID)TypeOfLCID, Max(M.ShippingTolerance)ShippingTolerance, --SUM(C.TotalValue)TotalValue, 
                    Cast(Round(SUM(C.TotalValue),2)as Decimal(10,2))TotalValue,
                    SUM(C.TotalQty)TotalQty, MAx(PM.PaymentMethodName) TypeOfLC, Max(CR.ValueName) CreditDays
                    FROM M
                    INNER JOIN C ON C.YPOMasterID = M.YPOMasterID
                    LEFT JOIN PM ON PM.PaymentMethodID = M.TypeOfLCID
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue CR ON CR.ValueID = M.CreditDays
                )
                Select * From TmpSum; ";
            }
 
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnPIReceiveMaster data = records.Read<YarnPIReceiveMaster>().FirstOrDefault();
                data.YarnPIReceivePOList = records.Read<YarnPIReceivePO>().ToList();
                data.PONo = string.Join(",", data.YarnPIReceivePOList.Select(x => x.PONo).Distinct());
                data.YarnPO = records.Read<YarnPOMaster>().FirstOrDefault();

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

        public async Task SaveAsync(YarnPIReceiveMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _service.SaveSingleAsync(entity, transaction);
                //await _service.SaveAsync(entity.Childs, transaction);
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

        public async Task<YarnPIReceiveMaster> GetDetailsAsync(int id)
        {
            string sql = $@"
            ;Select * From {TableNames.YarnPIReceiveMaster} Where YPIReceiveMasterID = {id};
            SELECT * FROM {TableNames.YarnPIReceiveChild} WHERE YPIReceiveMasterID={id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnPIReceiveMaster data = records.Read<YarnPIReceiveMaster>().FirstOrDefault();
                data.Childs = records.Read<YarnPIReceiveChild>().ToList();
                Guard.Against.NullObject(data);
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

    }
}
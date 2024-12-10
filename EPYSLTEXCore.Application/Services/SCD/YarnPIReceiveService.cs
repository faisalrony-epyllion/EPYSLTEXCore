using Dapper;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;

namespace EPYSLTEX.Infrastructure.Services
{
    public class YarnPIReceiveService : IYarnPIReceiveService
    {
        private readonly IDapperCRUDService<YarnPIReceiveMaster> _service;
        private readonly SqlConnection _connection;

        public YarnPIReceiveService(IDapperCRUDService<YarnPIReceiveMaster> service)
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<List<YarnPIReceiveMaster>> GetAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy;
            string sql;
            if (status == Status.Pending)
            {
                sql = $@"
                     With 
                    YPM As ( 
                        Select 0 as YPIReceiveMasterID, M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID
	                    ,M.QuotationRefNo, M.DeliveryStartDate, M.DeliveryEndDate, M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID
	                    , M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID, M.Charges, 
	                    M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, 
	                    M.ShipmentModeID, M.OfferValidity,
	                    case when M.RevisionNo != YPIPO.RevisionNo AND YPIPO.YPOMasterID IS NOT NULL then 'Revise' else'New' end as Status
	                    , M.QualityApprovalProcedureId, M.AddedBy,M.Approved, POAddedByName = LU.Name
	                    From YarnPOMaster M
	                    Left Join YarnPIReceivePO YPIPO ON M.YPOMasterID = YPIPO.YPOMasterID
                        Left JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.AddedBy
	                    Where M.Approved = 1 AND ISNULL(M.IsCancel,0) = 0 AND YPIPO.YPOMasterID IS NULL
	                    GROUP BY YPIReceiveMasterID,M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID
	                    , M.QuotationRefNo, M.DeliveryStartDate, M.DeliveryEndDate, M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID
	                    , M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID, M.Charges, 
	                    M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, 
	                    M.ShipmentModeID, M.OfferValidity, M.RevisionNo, YPIPO.RevisionNo, YPIPO.YPOMasterID, M.QualityApprovalProcedureId, M.AddedBy,M.Approved,LU.Name

	                    UNION

	                    Select 0 as YPIReceiveMasterID,M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID
	                    , M.QuotationRefNo, M.DeliveryStartDate, M.DeliveryEndDate, M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID
	                    , M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID, M.Charges, 
	                    M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, 
	                    M.ShipmentModeID, M.OfferValidity,
	                    case when M.RevisionNo != YPIPO.RevisionNo AND YPIPO.YPOMasterID IS NOT NULL then 'Revise' else'New' end as Status
	                    , M.QualityApprovalProcedureId, M.AddedBy,M.Approved, POAddedByName = LU.Name
	                    From YarnPOMaster M
	                    Left Join YarnPIReceivePO YPIPO ON M.YPOMasterID = YPIPO.YPOMasterID
                        Left JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.AddedBy
	                    Where M.Approved = 1  AND ISNULL(M.IsCancel,0) = 0 AND YPIPO.YPOMasterID IS NOT NULL 
                        AND  M.RevisionNo != YPIPO.RevisionNo 
	                    GROUP BY YPIReceiveMasterID,M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID
	                    , M.QuotationRefNo, M.DeliveryStartDate, M.DeliveryEndDate, M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID
	                    , M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID, M.Charges, 
	                    M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, 
	                    M.ShipmentModeID, M.OfferValidity,YPIPO.YPOMasterID,M.RevisionNo,YPIPO.RevisionNo,M.QualityApprovalProcedureId, M.AddedBy,M.Approved,LU.Name
                        
                        Union

						Select 0 as YPIReceiveMasterID,M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID
	                    , M.QuotationRefNo, M.DeliveryStartDate, M.DeliveryEndDate, M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID
	                    , M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID, M.Charges, 
	                    M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, 
	                    M.ShipmentModeID, M.OfferValidity,
	                    case when M.RevisionNo = YPIPO.RevisionNo AND YPIPO.YPOMasterID IS NOT NULL then 'Revise' else'New' end as Status
	                    , M.QualityApprovalProcedureId, M.AddedBy,M.Approved, POAddedByName = LU.Name
	                    From YarnPOMaster M
	                    Left Join YarnPIReceivePO YPIPO ON M.YPOMasterID = YPIPO.YPOMasterID
                        Left JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.AddedBy
	                    Where M.Approved = 1 AND ISNULL(M.IsCancel,0) = 0 AND YPIPO.YPOMasterID IS NOT NULL 
                        AND  M.RevisionNo = YPIPO.RevisionNo 
	                    GROUP BY YPIReceiveMasterID,M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID
	                    , M.QuotationRefNo, M.DeliveryStartDate, M.DeliveryEndDate, M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID
	                    , M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID, M.Charges, 
	                    M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, 
	                    M.ShipmentModeID, M.OfferValidity,YPIPO.YPOMasterID,M.RevisionNo,YPIPO.RevisionNo,M.QualityApprovalProcedureId, M.AddedBy,M.Approved,LU.Name
                    ),
                    YPI AS(
                        SELECT YPM.YPIReceiveMasterID,YPM.YPOMasterID, YPM.PONo, YPM.CompanyID, YPM.SupplierID, CONVERT(varchar, YPM.PODate, 101) PODate, 
	                    Contacts.Name AS SupplierName, YPM.QuotationRefNo, YPF.POFor, CONVERT(varchar, YPM.DeliveryStartDate, 101) DeliveryStartDate, 
	                    CONVERT(varchar, YPM.DeliveryEndDate, 101) DeliveryEndDate, CE.ShortName AS CompanyName, 
	                    YPC.POQty,
                        YPC.YPOChildID,
                        SUM(ISNULL(YPR.PIQty,0)) PIQty,
	                    SUM(YPC.PIValue) AS PIValue,YPM.Status,YPM.Approved
	                    FROM YarnPOChild YPC 
	                    INNER JOIN YPM ON YPM.YPOMasterID = YPC.YPOMasterID
                        Left Join YarnPIReceivePO YPR ON YPR.YPOMasterID=YPC.YPOMasterID AND YPR.ItemMasterID=YPC.ItemMasterID And YPR.YPOChildID = YPC.YPOChildID
						--Left Join YarnPIReceiveChild YPRC ON YPRC.ItemMasterID=YPC.ItemMasterID and YPRC.YPIReceiveMasterID=YPR.YPIReceiveMasterID
	                    LEFT JOIN YarnPIFor YPF ON YPF.POForID = YPM.POForID
	                    Inner Join {DbNames.EPYSL}..Contacts ON Contacts.ContactID = YPM.SupplierID
	                    INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPM.CompanyID
	                    GROUP BY YPM.YPIReceiveMasterID,YPM.YPOMasterID, YPM.PONo, YPM.CompanyID, YPM.SupplierID, PODate, Contacts.Name, YPM.QuotationRefNo, 
	                    YPF.POFor,  YPC.POQty, YPC.YPOChildID, YPM.DeliveryStartDate, YPM.DeliveryEndDate, CE.ShortName,YPM.Status,YPM.Approved
                    ),
                    FinalList AS
					(
						SELECT YPIReceiveMasterID,YPOMasterID, PONo, CompanyID, SupplierID, PODate, 
	                    SupplierName, QuotationRefNo, POFor, DeliveryStartDate, Status,Approved,
	                    DeliveryEndDate, CompanyName, POQty = SUM(POQty), PIQty = SUM(PIQty), PIValue = SUM(PIValue)
						FROM YPI
						GROUP BY YPIReceiveMasterID,YPOMasterID, PONo, CompanyID, SupplierID, PODate, 
	                    SupplierName, QuotationRefNo, POFor, DeliveryStartDate, 
	                    DeliveryEndDate, CompanyName, Status,Approved
                        Having SUM(YPI.POQty)>SUM(YPI.PIQty)
					),
                    --SELECT YPIReceiveMasterID,YPOMasterID, PONo, CompanyID, SupplierID, CompanyName, PODate, SupplierName, QuotationRefNo, POFor, DeliveryStartDate, 
                    --DeliveryEndDate, POQty, PIQty,PIValue,
                    --Status=Case When POQty>PIQty then 'New' else 'Revise' end,
                    --Count(*) Over() TotalRows FROM FinalList

				    RYPM as(
						SELECT  YPO.YPIReceiveMasterID,M.YPOMasterID, YPRM.PONo,YPRM.YPINo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID
	                    , M.QuotationRefNo, M.DeliveryStartDate, M.DeliveryEndDate, M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID
	                    , M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID, M.Charges, 
	                    M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, 
	                    M.ShipmentModeID, M.OfferValidity,'Revision' As Status
	                    , M.QualityApprovalProcedureId, M.AddedBy,M.Approved, POAddedByName = LU.Name

						FROM YarnPIReceiveMaster YPRM
						Left Join YarnPIReceivePO YPO ON YPO.YPIReceiveMasterID = YPRM.YPIReceiveMasterID
						Left Join YarnPOMaster M ON M.YPOMasterID = YPO.YPOMasterID
						Inner Join {DbNames.EPYSL}..Contacts ON Contacts.ContactID = M.SupplierID
						INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID
						LEFT JOIN YarnPIFor YPF ON YPF.POForID = M.POForID
						LEFT JOIN YarnPOChild YPC ON YPC.YPOMasterID=M.YPOMasterID
                        Left JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.AddedBy
						--Where YPRM.PreProcessRevNo!=M.RevisionNo --And M.Approved = 1
                        Where YPO.RevisionNo!=M.RevisionNo --And M.Approved = 1

						GROUP BY YPO.YPIReceiveMasterID,M.YPOMasterID, YPRM.PONo,YPRM.YPINo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID
	                    , M.QuotationRefNo, M.DeliveryStartDate, M.DeliveryEndDate, M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID
	                    , M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID, M.Charges, 
	                    M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, M.Approved,
	                    M.ShipmentModeID, M.OfferValidity
	                    , M.QualityApprovalProcedureId, M.AddedBy,LU.Name
						),
                    RYPI AS(
                        SELECT RYPM.YPIReceiveMasterID,RYPM.YPOMasterID, RYPM.PONo,RYPM.YPINo, RYPM.CompanyID, RYPM.SupplierID, CONVERT(varchar, RYPM.PODate, 101) PODate, 
	                    Contacts.Name AS SupplierName, RYPM.QuotationRefNo, YPF.POFor, CONVERT(varchar, RYPM.DeliveryStartDate, 101) DeliveryStartDate, 
	                    CONVERT(varchar, RYPM.DeliveryEndDate, 101) DeliveryEndDate, CE.ShortName AS CompanyName, 
	                    YPC.POQty,
                        YPC.YPOChildID,
                        SUM(ISNULL(YPR.PIQty,0)) PIQty,
	                    SUM(YPC.PIValue) AS PIValue,RYPM.Status,RYPM.Approved,RYPM.POAddedByName
	                    FROM YarnPOChild YPC 
	                    INNER JOIN RYPM ON RYPM.YPOMasterID = YPC.YPOMasterID
                        Left Join YarnPIReceivePO YPR ON YPR.YPOMasterID=YPC.YPOMasterID AND YPR.ItemMasterID=YPC.ItemMasterID And YPR.YPOChildID = YPC.YPOChildID
						--Left Join YarnPIReceiveChild YPRC ON YPRC.ItemMasterID=YPC.ItemMasterID and YPRC.YPIReceiveMasterID=YPR.YPIReceiveMasterID
	                    LEFT JOIN YarnPIFor YPF ON YPF.POForID = RYPM.POForID
	                    Inner Join {DbNames.EPYSL}..Contacts ON Contacts.ContactID = RYPM.SupplierID
	                    INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = RYPM.CompanyID
	                    GROUP BY RYPM.YPIReceiveMasterID,RYPM.YPOMasterID, RYPM.PONo,RYPM.YPINo, RYPM.CompanyID, RYPM.SupplierID, PODate, Contacts.Name, RYPM.QuotationRefNo, 
	                    YPF.POFor,  YPC.POQty, YPC.YPOChildID, RYPM.DeliveryStartDate, RYPM.DeliveryEndDate, CE.ShortName,RYPM.Status,RYPM.Approved,RYPM.POAddedByName
                    ),
                    RFinalList AS
					(
						SELECT YPIReceiveMasterID,YPOMasterID, PONo,YPINo, CompanyID, SupplierID, PODate, 
	                    SupplierName, QuotationRefNo, POFor, DeliveryStartDate, Status,Approved,
	                    DeliveryEndDate, CompanyName, POQty = SUM(POQty), PIQty = SUM(PIQty), PIValue = SUM(PIValue),POAddedByName
						FROM RYPI
						GROUP BY YPIReceiveMasterID,YPOMasterID, PONo,YPINo, CompanyID, SupplierID, PODate, 
	                    SupplierName, QuotationRefNo, POFor, DeliveryStartDate, 
	                    DeliveryEndDate, CompanyName, Status,Approved,POAddedByName
					),

					FullFinal As(
					SELECT YPIReceiveMasterID,YPOMasterID, PONo,'' as YPINo, CompanyID, SupplierID, CompanyName, PODate, SupplierName, QuotationRefNo, POFor, DeliveryStartDate, 
                    DeliveryEndDate, POQty, PIQty,PIValue,
                    Status=Case When POQty>PIQty then 'New' else 'Revise' end,Approved,
                    Count(*) Over() TotalRows FROM FinalList
					Union
					SELECT R.YPIReceiveMasterID,R.YPOMasterID, R.PONo,R.YPINo, R.CompanyID, R.SupplierID, R.CompanyName, R.PODate, R.SupplierName, R.QuotationRefNo, R.POFor, R.DeliveryStartDate, 
                    R.DeliveryEndDate, R.POQty, R.PIQty,R.PIValue,R.Status,R.Approved,
                    Count(*) Over() TotalRows FROM RFinalList R
					
					),
					PoIdUniqueList AS
					(
	                SELECT DISTINCT POM.YPOMasterID, F.YPIReceiveMasterID
	                FROM FullFinal F
	                INNER JOIN YarnPIReceivePO RPO ON RPO.YPIReceiveMasterID = F.YPIReceiveMasterID
	                INNER JOIN YarnPOMaster POM ON POM.YPOMasterID = RPO.YPOMasterID 
					),
					PoIdList AS
					(
	                SELECT POIds = STRING_AGG(A.YPOMasterID, ','), A.YPIReceiveMasterID
	                FROM PoIdUniqueList A
	                GROUP BY A.YPIReceiveMasterID
					)

					Select F.*, PO.POIds
					From FullFinal F
					LEFT JOIN PoIdList PO ON PO.YPIReceiveMasterID = F.YPIReceiveMasterID";

                orderBy = "ORDER BY YPOMasterID DESC";
            }
            else if (status == Status.RejectReview)
            {
                //sql = $@"
                //    ;With 
                //    YPI AS(
                //     SELECT M.Acknowledge, M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, CC.Name AS SupplierName, 
                //        PIFilePath, AttachmentPreviewTemplate, M.CompanyID, M.SupplierID, LU.Name POCreatedBy, SUM(C.POQty) AS POQty, 
                //        SUM(C.PIQty) PIQty, SUM(C.PIValue) AS PIValue
                //     FROM YarnPIReceiveMaster M
                //     INNER JOIN YarnPIReceiveChild C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
                //     INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                //     INNER JOIN YarnPIReceivePO YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
                //     INNER JOIN YarnPOMaster YPO ON YPIPO.YPOMasterID = YPO.YPOMasterID
                //     INNER JOIN {DbNames.{DbNames.EPYSL}}..LoginUser LU ON YPO.AddedBy = LU.UserCode
                //        WHERE M.IsCDA = 0 And Acknowledge=0 And UnAcknowledge=1
                //     GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, CC.Name, PIFilePath, AttachmentPreviewTemplate, M.CompanyID, 
                //        M.SupplierID, LU.Name, M.Acknowledge
                //    )		

                //    SELECT  YPI.Acknowledge,YPIReceiveMasterID, YPINo, PIDate, SupplierName, PIFilePath, AttachmentPreviewTemplate, POQty, PIQty, 
                //    PIValue, CompanyID, SupplierID, POCreatedBy, Count(*) Over() TotalRows
                //    FROM YPI";

                //orderBy = "ORDER BY YPIReceiveMasterID DESC";

                sql = $@"
                With TmpYR As 
                (
	                SELECT M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, M.CompanyID, CE.ShortName As CompanyName, 
	                M.SupplierID, M.PONo, CC.Name AS SupplierName, M.PIFilePath, M.AttachmentPreviewTemplate,   
	                C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue, POAddedByName = LU.Name 
	                FROM YarnPIReceiveMaster M
	                INNER JOIN YarnPIReceiveChild C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                INNER JOIN YarnPIReceivePO YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                INNER JOIN YarnPOMaster YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID  
	                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID
                    Left JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPO.AddedBy
	                WHERE ISNULL(M.NeedsReview,0) = 0 AND ISNULL(Reject,0) = 1 AND IsCDA = 'False'
	                GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, M.CompanyID, CE.ShortName, M.SupplierID, M.PONo, CC.Name,
	                M.PIFilePath, M.AttachmentPreviewTemplate, C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue ,LU.Name
                ),
                TmpYRC As 
                (
	                Select YPIReceiveMasterID, YPINo, PIDate, CompanyID, CompanyName, SupplierID, SupplierName, PONo, PIFilePath, 
	                AttachmentPreviewTemplate,  
	                SUM(POQty)POQty, SUM(Rate)Rate, SUM(PIQty)PIQty, SUM(PIValue)PIValue,POAddedByName
	                From TmpYR
	                Group By YPIReceiveMasterID, YPINo, PIDate, CompanyID, CompanyName, SupplierID, SupplierName, PONo, PIFilePath, 
	                AttachmentPreviewTemplate,POAddedByName
                )
                Select *, Count(*) Over() TotalRows From TmpYRC ";

                orderBy = "ORDER BY YPIReceiveMasterID DESC";
            }
            else if (status == Status.Reject)
            {
                sql = $@"
                With TmpYR As 
                (
	                SELECT M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, M.CompanyID, CE.ShortName As CompanyName, 
	                M.SupplierID, M.PONo, CC.Name AS SupplierName, M.PIFilePath, M.AttachmentPreviewTemplate,   
	                C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue, POAddedByName = LU.Name   
	                FROM YarnPIReceiveMaster M
	                INNER JOIN YarnPIReceiveChild C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                INNER JOIN YarnPIReceivePO YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                INNER JOIN YarnPOMaster YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID  
	                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID  
                    Left JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPO.AddedBy
	                WHERE M.IsCDA = 0 And M.Acknowledge = 0 And M.UnAcknowledge = 1
                    And YPO.RevisionNo = YPIPO.RevisionNo
	                GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, M.CompanyID, CE.ShortName, M.SupplierID, M.PONo, CC.Name,
	                M.PIFilePath, M.AttachmentPreviewTemplate, C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue ,LU.Name
                ),
                TmpYRC As 
                (
	                Select YPIReceiveMasterID, YPINo, PIDate, CompanyID, CompanyName, SupplierID, SupplierName, PONo, PIFilePath, 
	                AttachmentPreviewTemplate,  
	                SUM(POQty)POQty, SUM(Rate)Rate, SUM(PIQty)PIQty, SUM(PIValue)PIValue ,POAddedByName
	                From TmpYR
	                Group By YPIReceiveMasterID, YPINo, PIDate, CompanyID, CompanyName, SupplierID, SupplierName, PONo, PIFilePath, 
	                AttachmentPreviewTemplate ,POAddedByName
                )
                Select *, Count(*) Over() TotalRows From TmpYRC ";

                orderBy = "ORDER BY YPIReceiveMasterID DESC";
            }
            else if (status == Status.AwaitingPropose)
            {
           
                sql = $@"
                With TmpYR As 
                (
	                SELECT M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, M.CompanyID, CE.ShortName As CompanyName, 
	                M.SupplierID, M.PONo, CC.Name AS SupplierName, M.PIFilePath, M.AttachmentPreviewTemplate,   
	                C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue,
                    case when M.RevisionNo>0 then 'Revise' else'New' end as Status, POAddedByName = LU.Name
	                FROM YarnPIReceiveMaster M
	                INNER JOIN YarnPIReceiveChild C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                INNER JOIN YarnPIReceivePO YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                INNER JOIN YarnPOMaster YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID  
	                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID
                    Left JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPO.AddedBy
	                WHERE M.IsCDA = 0 And M.Acknowledge = 0 And M.UnAcknowledge = 0
                    --AND YPO.RevisionNo = YPIPO.RevisionNo
	                GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, M.CompanyID, CE.ShortName, M.SupplierID, M.PONo, CC.Name,M.RevisionNo,
	                M.PIFilePath, M.AttachmentPreviewTemplate, C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue ,LU.Name
                ),
                TmpYRC As 
                (
	                Select YPIReceiveMasterID, YPINo, PIDate, CompanyID, CompanyName, SupplierID, SupplierName, PONo, PIFilePath, 
	                AttachmentPreviewTemplate,TmpYR.Status,   
	                SUM(POQty)POQty, SUM(Rate)Rate, SUM(PIQty)PIQty, SUM(PIValue)PIValue ,POAddedByName
	                From TmpYR
	                Group By YPIReceiveMasterID, YPINo, PIDate, CompanyID, CompanyName, SupplierID, SupplierName, PONo, PIFilePath, 
	                AttachmentPreviewTemplate,TmpYR.Status,POAddedByName
                )
                Select *, Count(*) Over() TotalRows From TmpYRC";

                orderBy = "ORDER BY YPIReceiveMasterID DESC";
            }
            else if (status == Status.Acknowledge)
            {
             
                sql = $@"
                With RVSList AS
                (
                    SELECT YPI.YPIReceivePOID, YPIReceiveMasterID
                    FROM YarnPIReceivePO YPI
                    INNER JOIN YarnPOMaster YPO ON YPO.YPOMasterID = YPI.YPOMasterID
                    WHERE YPI.RevisionNo <> YPO.RevisionNo
                ),
                TmpYR As 
                (
	                SELECT M.YPIReceiveMasterID, M.YPINo,YPO.YPOMasterID, CONVERT(varchar, M.PIDate, 101) PIDate, M.CompanyID, CE.ShortName As CompanyName, 
	                M.SupplierID, M.PONo, CC.Name AS SupplierName, M.PIFilePath, M.AttachmentPreviewTemplate,   
	                C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue, POAddedByName = LU.Name  
	                FROM YarnPIReceiveMaster M
	                INNER JOIN YarnPIReceiveChild C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                INNER JOIN YarnPIReceivePO YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                INNER JOIN YarnPOMaster YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID  
	                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID
                    LEFT JOIN RVSList RV ON RV.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
                    Left JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPO.AddedBy
	                WHERE M.IsCDA = 0 And Acknowledge = 1 And UnAcknowledge = 0
                    AND YPO.RevisionNo = YPIPO.RevisionNo AND RV.YPIReceivePOID IS NULL
	                GROUP BY M.YPIReceiveMasterID, M.YPINo,YPO.YPOMasterID, M.PIDate, M.CompanyID, CE.ShortName, M.SupplierID, M.PONo, CC.Name,
	                M.PIFilePath, M.AttachmentPreviewTemplate, C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue,LU.Name 
                ),
                TmpYRC As 
                (
	                Select YPIReceiveMasterID, YPINo, PIDate,POIds=YPOMasterID, CompanyID, CompanyName, SupplierID, SupplierName, PONo, PIFilePath, 
	                AttachmentPreviewTemplate,  
	                SUM(POQty)POQty, SUM(Rate)Rate, SUM(PIQty)PIQty, SUM(PIValue)PIValue ,POAddedByName
	                From TmpYR
	                Group By YPIReceiveMasterID, YPINo,YPOMasterID, PIDate, CompanyID, CompanyName, SupplierID, SupplierName, PONo, PIFilePath, 
	                AttachmentPreviewTemplate ,POAddedByName
                )
                Select *, Count(*) Over() TotalRows From TmpYRC";

                orderBy = "ORDER BY YPIReceiveMasterID DESC";
            }
            else if(status==Status.Revise)
            {
                sql = $@"
                With TmpYR As 
                (
	                SELECT M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, M.CompanyID, CE.ShortName As CompanyName, 
	                M.SupplierID, M.PONo, CC.Name AS SupplierName, M.PIFilePath, M.AttachmentPreviewTemplate,   
	                C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue, POAddedByName = LU.Name  
	                FROM YarnPIReceiveMaster M
	                INNER JOIN YarnPIReceiveChild C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                INNER JOIN YarnPIReceivePO YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                INNER JOIN YarnPOMaster YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID  
	                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID  
                    Left JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPO.AddedBy
	                WHERE M.IsCDA = 0 AND M.UnAcknowledge = 0 --And M.YPINo = 'az-200422'
                    --AND M.Accept=0
                    AND YPO.RevisionNo <> YPIPO.RevisionNo
                    --AND YPO.RevisionNo <> M.PreProcessRevNo
	                GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, M.CompanyID, CE.ShortName, M.SupplierID, M.PONo, CC.Name,
	                M.PIFilePath, M.AttachmentPreviewTemplate, C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue,YPIPO.RevisionNo,YPO.RevisionNo,LU.Name
                ),
                PoIdUniqueList AS
                (
	                SELECT DISTINCT POM.YPOMasterID, TmpYR.YPIReceiveMasterID
	                FROM TmpYR
	                INNER JOIN YarnPIReceivePO RPO ON RPO.YPIReceiveMasterID = TmpYR.YPIReceiveMasterID
	                INNER JOIN YarnPOMaster POM ON POM.YPOMasterID = RPO.YPOMasterID 
                ),
                PoIdList AS
                (
	                SELECT POIds = STRING_AGG(A.YPOMasterID, ','), A.YPIReceiveMasterID
	                FROM PoIdUniqueList A
	                GROUP BY A.YPIReceiveMasterID
                ),
                TmpYRC As 
                (
	                Select TmpYR.YPIReceiveMasterID, YPINo, PIDate, CompanyID, CompanyName, SupplierID, SupplierName, PONo, PIFilePath, 
	                AttachmentPreviewTemplate,PO.POIds,  
	                SUM(POQty)POQty, SUM(Rate)Rate, SUM(PIQty)PIQty, SUM(PIValue)PIValue ,POAddedByName
	                From TmpYR
                    LEFT JOIN PoIdList PO ON PO.YPIReceiveMasterID = TmpYR.YPIReceiveMasterID
	                Group By TmpYR.YPIReceiveMasterID, YPINo, PIDate, CompanyID, CompanyName, SupplierID, SupplierName, PONo, PIFilePath, 
	                AttachmentPreviewTemplate,PO.POIds,POAddedByName
                )
                Select *, Count(*) Over() TotalRows From TmpYRC";

                orderBy = "ORDER BY YPIReceiveMasterID DESC";
            }
            else
            {
                sql = $@"
                With 
				RVSList AS
				(
					SELECT YPI.YPIReceivePOID, YPIReceiveMasterID
					FROM YarnPIReceivePO YPI
					INNER JOIN YarnPOMaster YPO ON YPO.YPOMasterID = YPI.YPOMasterID
					WHERE YPI.RevisionNo <> YPO.RevisionNo
				),
                TmpYR As 
                (
	                SELECT M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, M.CompanyID, CE.ShortName As CompanyName, 
	                M.SupplierID, M.PONo, CC.Name AS SupplierName, M.PIFilePath, M.AttachmentPreviewTemplate,   
	                C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue,M.RevisionNo,
                    case when M.RevisionNo>0 then 'Revise' else'New' end as Status, POAddedByName = LU.Name
	                --FROM YarnPIReceiveMaster M
	                --INNER JOIN YarnPIReceiveChild C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                --INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                --INNER JOIN YarnPIReceivePO YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                --INNER JOIN YarnPOMaster YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID  
	                --INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID  
	                --WHERE M.IsCDA = 0 AND M.UnAcknowledge = 0 --And M.YPINo = 'az-200422'
                    --AND YPO.RevisionNo=YPIPO.RevisionNo
                    FROM YarnPIReceiveMaster M
	                INNER JOIN YarnPIReceiveChild C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
					--INNER JOIN YarnPOMaster YPO2 ON YPO2.YPOMasterID = M.YPOMasterID
	                INNER JOIN YarnPIReceivePO YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                INNER JOIN YarnPOMaster YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID  
	                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID
					LEFT JOIN RVSList RV ON RV.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
                    Left JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPO.AddedBy
	                WHERE M.IsCDA = 0 AND M.Acknowledge=0 AND M.UnAcknowledge = 0 --And M.YPINo = 'az-200422'
                    AND YPO.RevisionNo = YPIPO.RevisionNo AND RV.YPIReceivePOID IS NULL
					--AND YPO.RevisionNo = M.PreProcessRevNo

                    GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, M.CompanyID, CE.ShortName, M.SupplierID, M.PONo, CC.Name,M.RevisionNo,
	                M.PIFilePath, M.AttachmentPreviewTemplate, C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue,YPIPO.RevisionNo,YPO.RevisionNo,LU.Name
                ),
                TmpYRC As 
                (
	                Select YPIReceiveMasterID, YPINo, PIDate, CompanyID, CompanyName, SupplierID, SupplierName, PONo, PIFilePath, 
	                AttachmentPreviewTemplate,TmpYR.Status,TmpYR.RevisionNo,
	                SUM(POQty)POQty, SUM(Rate)Rate, SUM(PIQty)PIQty, SUM(PIValue)PIValue ,POAddedByName
	                From TmpYR
	                Group By YPIReceiveMasterID, YPINo, PIDate, CompanyID, CompanyName, SupplierID, SupplierName, PONo, PIFilePath, 
	                AttachmentPreviewTemplate,TmpYR.Status,TmpYR.RevisionNo,POAddedByName

                )
                Select *, Count(*) Over() TotalRows From TmpYRC";

                orderBy = "ORDER BY YPIReceiveMasterID DESC";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnPIReceiveMaster>(sql);
        }         
        public async Task<List<YarnPIReceiveMaster>> GetCDAAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy;
            string sql;

            if (status == Status.Pending)
            {
                sql = $@"
                    With YPM As 
                    (
	                    Select CDAPM.CDAPOMasterID, CDAPM.CompanyID, CDAPM.PONo, CDAPM.PODate, CDAPM.SupplierID, 
                        CDAPM.QuotationRefNo, CDAPM.DeliveryStartDate, CDAPM.DeliveryEndDate
	                    From CDAPOMaster CDAPM
	                    Left Join YarnPIReceivePO YPIPO ON CDAPM.CDAPOMasterID = YPIPO.YPOMasterID
	                    Where CDAPM.Approved = 1 AND YPIPO.YPOMasterID IS NULL
                    ),
                    YPI AS
                    (
	                    SELECT YPM.CDAPOMasterID, YPM.CompanyID, CE.ShortName AS CompanyName, YPM.PONo, CONVERT(varchar, YPM.PODate, 101) PODate, 
	                    YPM.SupplierID, C.Name AS SupplierName, YPM.QuotationRefNo, CONVERT(varchar, YPM.DeliveryStartDate, 101) DeliveryStartDate,  
	                    CONVERT(varchar, YPM.DeliveryEndDate, 101) DeliveryEndDate, SUM(YPC.POQty) POQty, SUM(YPC.POValue) AS PIValue 
	                    FROM YPM 
	                    INNER JOIN CDAPOChild YPC ON YPC.CDAPOMasterID = YPM.CDAPOMasterID 
	                    Inner Join {DbNames.EPYSL}..Contacts C ON C.ContactID = YPM.SupplierID
	                    INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPM.CompanyID
	                    --INNER JOIN {DbNames.EPYSL}..ContactCategoryChild CCC ON C.ContactID = CCC.ContactID 
	                    --INNER JOIN {DbNames.EPYSL}..ContactCategoryHK CCH ON CCC.ContactCategoryID = CCH.ContactCategoryID 
	                    --LEFT JOIN YarnPIFor YPF ON YPF.POForID = YPM.POForID 
	                    GROUP BY YPM.CDAPOMasterID, YPM.CompanyID, CE.ShortName, YPM.PONo, YPM.PODate, YPM.SupplierID, C.Name, 
	                    YPM.QuotationRefNo, YPM.DeliveryStartDate, YPM.DeliveryEndDate
                    )
                    SELECT CDAPOMasterID YPOMasterID, CompanyID, CompanyName, PONo, PODate, SupplierID, SupplierName, 
                    QuotationRefNo, DeliveryStartDate, DeliveryEndDate, POQty, 0 As PIQty, PIValue, Count(*) Over() TotalRows
                    FROM YPI";

                orderBy = "ORDER BY CDAPOMasterID DESC";
            }
            else if (status == Status.Reject)
            {
                sql = $@"
                    ;With M As 
                    (
                        Select * From YarnPIReceiveMaster Where Reject = 1
                    )
                    ,YPI AS
                    (
	                    SELECT M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, M.PONo, 
                        CC.Name AS SupplierName, M.PIFilePath, M.AttachmentPreviewTemplate, M.CompanyID, CE.ShortName As CompanyName, 
                        M.SupplierID, SUM(C.POQty) AS POQty, SUM(C.PIQty) PIQty, SUM(C.PIValue) AS PIValue
	                    FROM M
	                    INNER JOIN YarnPIReceiveChild C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                    INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                        Inner Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = M.CompanyID
	                    INNER JOIN YarnPIReceivePO YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
                        WHERE M.IsCDA = 1
	                    GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, M.PONo, CC.Name, M.PIFilePath, 
                        M.AttachmentPreviewTemplate, M.CompanyID,  CE.ShortName, M.SupplierID
                    )
                    SELECT YPIReceiveMasterID, YPINo, PIDate, PONo, CompanyID, CompanyName, SupplierID, SupplierName, 
                    POQty, PIQty, PIValue, Count(*) Over()
                    FROM YPI";

                orderBy = "ORDER BY YPIReceiveMasterID DESC";
            }
            else if (status == Status.Completed)
            {
                sql = $@"
                    ;With YPI AS
                    (
	                    SELECT YPRM.YPIReceiveMasterID, YPRM.YPINo, CONVERT(varchar, YPRM.PIDate, 101) PIDate, 
                        YPRM.PONo, CC.Name AS SupplierName, PIFilePath, YPRM.AttachmentPreviewTemplate, YPRM.CompanyID, 
                        CE.ShortName As CompanyName, YPRM.SupplierID, SUM(YPRC.POQty) AS POQty, SUM(YPRC.PIQty) PIQty, 
	                    SUM(YPRC.PIValue) AS PIValue
	                    FROM YarnPIReceiveMaster YPRM
	                    INNER JOIN YarnPIReceiveChild YPRC ON YPRC.YPIReceiveMasterID = YPRM.YPIReceiveMasterID
	                    INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = YPRM.SupplierID
                        Inner Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = YPRM.CompanyID
	                    INNER JOIN YarnPIReceivePO YPIPO ON YPRM.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                    WHERE YPRM.IsCDA = 1 AND Acknowledge = 0
	                    GROUP BY YPRM.YPIReceiveMasterID, YPRM.YPINo, YPRM.PIDate, YPRM.PONo, CC.Name, PIFilePath, 
                        AttachmentPreviewTemplate, YPRM.CompanyID, CE.ShortName, YPRM.SupplierID
                    )
                    SELECT YPIReceiveMasterID, YPINo, PIDate, PONo, CompanyID, CompanyName, SupplierID, SupplierName, 
                    POQty, PIQty, PIValue 
                    FROM YPI";

                orderBy = "ORDER BY YPIReceiveMasterID DESC";
            }
            else if (status == Status.AwaitingPropose)
            {
                sql = $@"
                    ;With YPI AS
                    (
	                    SELECT YPRM.YPIReceiveMasterID, YPRM.YPINo, YPRM.PONo, CONVERT(varchar, YPRM.PIDate, 101) PIDate, 
                        CC.Name AS SupplierName, PIFilePath, YPRM.AttachmentPreviewTemplate, YPRM.CompanyID,
                        CE.ShortName As CompanyName, YPRM.SupplierID, SUM(YPRC.POQty) AS POQty, SUM(YPRC.PIQty) PIQty, 
	                    SUM(YPRC.PIValue) AS PIValue
	                    FROM YarnPIReceiveMaster YPRM
	                    INNER JOIN YarnPIReceiveChild YPRC ON YPRC.YPIReceiveMasterID = YPRM.YPIReceiveMasterID
	                    INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = YPRM.SupplierID
                        Inner Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = YPRM.CompanyID
	                    INNER JOIN YarnPIReceivePO YPIPO ON YPRM.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                    WHERE YPRM.IsCDA = 1 AND Acknowledge = 0
	                    GROUP BY YPRM.YPIReceiveMasterID, YPRM.YPINo, YPRM.PONo, YPRM.PIDate, CC.Name, 
                        PIFilePath, AttachmentPreviewTemplate, YPRM.CompanyID, CE.ShortName, YPRM.SupplierID
                    )
                    SELECT YPIReceiveMasterID, YPINo, PONo, PIDate, CompanyID, CompanyName, SupplierID, SupplierName, 
                    POQty, PIQty, PIValue 
                    FROM YPI";

                orderBy = "ORDER BY YPIReceiveMasterID DESC";
            }
            else
            {
                sql = $@"
                    ;With YPI AS
                    (
	                    SELECT YPRM.YPIReceiveMasterID, YPRM.YPINo, YPRM.PONo, CONVERT(varchar, YPRM.PIDate, 101) PIDate, 
                        CC.Name AS SupplierName, PIFilePath, YPRM.AttachmentPreviewTemplate, YPRM.CompanyID, 
                        CE.ShortName As CompanyName, YPRM.SupplierID, SUM(YPRC.POQty) AS POQty, SUM(YPRC.PIQty) PIQty, 
	                    SUM(YPRC.PIValue) AS PIValue
	                    FROM YarnPIReceiveMaster YPRM
	                    INNER JOIN YarnPIReceiveChild YPRC ON YPRC.YPIReceiveMasterID = YPRM.YPIReceiveMasterID
	                    INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = YPRM.SupplierID
                        Inner Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = YPRM.CompanyID
	                    INNER JOIN YarnPIReceivePO YPIPO ON YPRM.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                    WHERE YPRM.IsCDA = 1 AND Acknowledge = 1 And Accept = 0
	                    GROUP BY YPRM.YPIReceiveMasterID, YPRM.YPINo, YPRM.PONo, YPRM.PIDate, CC.Name, PIFilePath, 
                        AttachmentPreviewTemplate, YPRM.CompanyID, CE.ShortName, YPRM.SupplierID
                    )
                    SELECT YPIReceiveMasterID, YPINo, PIDate, PONo, CompanyID, CompanyName, SupplierID, SupplierName, 
                    POQty, PIQty, PIValue 
                    FROM YPI";

                orderBy = "ORDER BY YPIReceiveMasterID DESC";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnPIReceiveMaster>(sql);
        }
        public async Task<YarnPIReceiveMaster> GetNewAsync(int yarnPoMasterId, int supplierId, int companyId)
        {
            var sql =
                $@"
                -- Master Info
                ;With YPM AS(
	                SELECT YPM.YPOMasterID,YarnPOMasterRevision= YPM.RevisionNo,YPM.PONo, YPM.PODate, YPM.CompanyID, YPM.SupplierID, YPM.QuotationRefNo, YPM.IncoTermsID, 
                    YPM.PaymentTermsID, YPM.TypeOfLCID, YPM.TenureofLC, YPM.CalculationofTenure, YPM.CreditDays, YPM.ReImbursementCurrencyID, 
                    YPM.Charges, YPM.CountryOfOriginID, YPM.TransShipmentAllow, YPM.ShippingTolerance, YPM.PortofLoadingID, 
                    YPM.PortofDischargeID, YPM.ShipmentModeID, YPM.OfferValidity, YPM.Remarks, 
                    --YPIPO.YPIReceiveMasterID YPINo,
                    YPRM.YPINo,
                    YPIPO.YPIReceiveMasterID
	                FROM YarnPOMaster YPM
	                Left Join YarnPIReceivePO YPIPO ON YPIPO.YPOMasterID = YPM.YPOMasterID
                    Left Join YarnPIReceiveMaster YPRM ON YPRM.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID And YPRM.IsCDA = 0
	                WHERE YPM.YPOMasterID = {yarnPoMasterId}
                ) 
                SELECT YPM.YPOMasterID, YPM.YarnPOMasterRevision,YPM.PONo, CONVERT(varchar, YPM.PODate, 101) PODate, YPM.CompanyID, YPM.SupplierID, 
                YPM.QuotationRefNo, YPM.IncoTermsID, YPM.PaymentTermsID, YPM.TypeOfLCID, YPM.TenureofLC, YPM.CalculationofTenure, 
                YPM.CreditDays, YPM.ReImbursementCurrencyID, YPM.Charges, YPM.CountryOfOriginID, YPM.TransShipmentAllow, 
                YPM.ShippingTolerance, YPM.PortofLoadingID, YPM.PortofDischargeID, YPM.ShipmentModeID, YPM.OfferValidity, 
                YPM.Remarks, C.Name SupplierName, CE.ShortName CompanyName, YPM.YPINo, YPM.YPIReceiveMasterID
                FROM YPM
                LEFT JOIN  {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPM.CompanyID
                LEFT JOIN  {DbNames.EPYSL}..Contacts C ON C.ContactID = YPM.SupplierID
                GROUP BY YPM.YPOMasterID, YPM.YarnPOMasterRevision,YPM.PONo, YPM.PODate, YPM.CompanyID, YPM.SupplierID, YPM.QuotationRefNo, 
                YPM.IncoTermsID, YPM.PaymentTermsID, YPM.TypeOfLCID, YPM.TenureofLC, YPM.CalculationofTenure, YPM.CreditDays, 
                YPM.ReImbursementCurrencyID, YPM.Charges, YPM.CountryOfOriginID, YPM.TransShipmentAllow, YPM.ShippingTolerance,
                YPM.PortofLoadingID, YPM.PortofDischargeID, YPM.ShipmentModeID, YPM.OfferValidity, YPM.Remarks, C.Name, 
                CE.ShortName,YPINo,YPIReceiveMasterID;

                -- Childs
                ;With YPC As (
	                Select * From YarnPOChild Where YPOMasterID = {yarnPoMasterId}
                ),
                Finalist As(
                SELECT YPC.YPOChildID As YPIReceiveChildID,YPC.YPOChildID, YPC.ItemMasterID, YPC.YarnCategory, YPC.UnitID, YPC.Remarks, 
                HSCode, YarnLotNo, U.DisplayUnitDesc, YPC.YarnProgramId, YPC.ShadeCode, YarnProgram.ValueName AS YarnProgram, 
                YPC.POQty, SUM(ISNULL(B.PIQty,0)) As POReceivedQty, YPC.Rate, YPC.PIValue POValue, 
                PIQty=YPC.POQty-SUM(ISNULL(B.PIQty,0)), 
                YPC.Rate PIRate, YPOM.RevisionNo,
                PIValue=(YPC.POQty-SUM(ISNULL(B.PIQty,0)))*YPC.Rate,
                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, 
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, 
                ISV7.SegmentValue Segment7ValueDesc
                FROM YPC 
                Inner Join YarnPOMaster YPOM ON YPC.YPOMasterID = YPOM.YPOMasterID
                LEFT JOIN YarnPIReceivePO B ON YPC.YPOMasterID=B.YPOMasterID  And YPC.YPOChildID = B.YPOChildID --and YPC.ItemMasterID=B.ItemMasterID
                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = YPOM.SupplierID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YPC.ItemMasterID   
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                INNER JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YPC.UnitID   
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue YarnProgram ON YarnProgram.ValueID = YPC.YarnProgramId  
                GROUP BY YPC.YPOChildID, YPC.ItemMasterID, YPC.YarnCategory, YPC.UnitID, YPC.Remarks, HSCode, YarnLotNo, 
                U.DisplayUnitDesc, YPC.YarnProgramId, YarnProgram.ValueName, YPC.POQty, YPC.Rate,YPOM.RevisionNo, YPC.PIValue, YPC.POQty, 
                YPC.Rate, YPC.PIValue, YPC.ShadeCode, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, 
                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue
                )

                Select * From Finalist
				Where POQty > POReceivedQty
                
                -- YarnPIReceivePO
                ;With A As 
                (
	                Select B.YPOMasterID, B.ItemMasterID, Sum(B.PIQty) PIQty
	                From YarnPIReceivePO B
                    Inner Join YarnPIReceiveMaster YPRM ON YPRM.YPIReceiveMasterID = B.YPIReceiveMasterID And YPRM.IsCDA = 0
	                Where B.YPOMasterID in ({yarnPoMasterId})
	                Group By B.YPOMasterID, B.ItemMasterID
                ), B As 
                (
	                Select YPOMasterID, ItemMasterID, SUM(POQty) POQty
	                From YarnPOChild
	                Where YPOMasterID in ({yarnPoMasterId})
	                Group By YPOMasterID, ItemMasterID
                )
                Select B.YPOMasterID, B.ItemMasterID, B.POQty, BalancePOQty = ISNULL(B.POQty,0) - ISNULL(A.PIQty,0)
                From B
                Left Join A On A.YPOMasterID = B.YPOMasterID And A.ItemMasterID = B.ItemMasterID; 

                -- Companies
                ;SELECT CAST(CompanyID AS VARCHAR) AS id, CompanyName AS [text]
                FROM {DbNames.EPYSL}..CompanyEntity
                WHERE CompanyID IN(8,6)
                ORDER BY CompanyName;

                -- Type of LC
                ;SELECT CAST(PaymentMethodID AS VARCHAR) AS id, PaymentMethodName AS [text]
                FROM {DbNames.EPYSL}..PaymentMethod
                WHERE PaymentMethodID > 0
                ORDER BY PaymentMethodID;
                
                -- Inco Terms
                ;SELECT CAST(CIT.IncoTermsID AS VARCHAR) AS id, IT.IncoTermsName AS [text] 
                FROM {DbNames.EPYSL}..ContactIncoTerms CIT
                INNER JOIN {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = CIT.ContactID
                INNER JOIN {DbNames.EPYSL}..IncoTerms IT ON IT.IncoTermsID = CIT.IncoTermsID
                WHERE CAI.ContactID = {supplierId}

                -- Payment Terms
                ;SELECT CAST(CPT.PaymentTermsID AS VARCHAR) AS id, PT.PaymentTermsName AS [text]
                FROM {DbNames.EPYSL}..ContactPaymentTerms CPT
                INNER JOIN {DbNames.EPYSL}..PaymentTrems PT ON PT.PaymentTermsID = CPT.PaymentTermsID 
                WHERE CPT.ContactID = {supplierId};

                -- Countries of Origin
                SELECT CAST(CT.CountryID AS VARCHAR) AS id, C.CountryName AS [text]
                FROM {DbNames.EPYSL}..Contacts CT
                INNER JOIN {DbNames.EPYSL}..Country C ON C.CountryID = CT.CountryID
                WHERE ContactID = {supplierId}

                ;SELECT	CAST(AdditionalValueID AS VARCHAR) AS id, AdditionalValueName text 
                FROM YarnAdditionalValueSetup;

                ;SELECT	CAST(DeductionValueID AS VARCHAR) AS id, DeductionValueName text 
                FROM YarnDeductionValueSetup;

                -- Entity Types
                {CommonQueries.GetEntityTypeValues()}; ";

            var data = new YarnPIReceiveMaster();
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);

                data = records.Read<YarnPIReceiveMaster>().FirstOrDefault();
                data.Childs = records.Read<YarnPIReceiveChild>().ToList();
                data.YarnPIReceivePOList = records.Read<YarnPIReceivePO>().ToList();
                data.CompanyList = records.Read<Select2OptionModel>().ToList();
                data.TypeOfLCList = records.Read<Select2OptionModel>().ToList();
                data.IncoTermsList = records.Read<Select2OptionModel>().ToList();
                data.PaymentTermsList = records.Read<Select2OptionModel>().ToList();
                data.CountryOfOriginList = records.Read<Select2OptionModel>().ToList();

                data.AdditionalValueSetupList = records.Read<Select2OptionModel>().ToList();
                data.DeductionValueSetupList = records.Read<Select2OptionModel>().ToList();

                var entityTypes = await records.ReadAsync<Select2OptionModel>();
                data.ShipmentModeList = entityTypes.Where(x => x.desc == EntityTypeConstants.SHIPMENT_MODE.ToString());
                data.CreditDaysList = entityTypes.Where(x => x.desc == EntityTypeConstants.LC_TENURE.ToString());
                data.PortofLoadingList = entityTypes.Where(x => x.desc == EntityTypeConstants.PORT_OF_LOADING.ToString());
                data.PortofDischargeList = entityTypes.Where(x => x.desc == EntityTypeConstants.PORT_OF_DISCHARGE.ToString());
                data.OfferValidityList = entityTypes.Where(x => x.desc == EntityTypeConstants.OFFER_VALIDITY.ToString()).OrderBy(x => x.desc); 
                data.CalculationofTenureList = entityTypes.Where(x => x.desc == EntityTypeConstants.CALCULATION_OF_TENURE.ToString());

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
        public async Task<YarnPIReceiveMaster> GetNewCDAAsync(int yarnPoMasterId, int supplierId, int companyId)
        {
            var sql =
                $@"
                -- Master Info
                ;With YPM AS
                (
	                SELECT CPM.CDAPOMasterID, CPM.PONo, CPM.PODate, CPM.CompanyID, CPM.SupplierID, CPM.QuotationRefNo, 
                    CPM.IncoTermsID, CPM.PaymentTermsID, CPM.TypeOfLCID, CPM.TenureofLC, CPM.CalculationofTenure, CPM.CreditDays, 
                    CPM.ReImbursementCurrencyID, CPM.Charges, CPM.CountryOfOriginID, CPM.TransShipmentAllow, CPM.ShippingTolerance, 
                    CPM.PortofLoadingID, CPM.PortofDischargeID, CPM.ShipmentModeID, CPM.OfferValidity, CPM.Remarks, CPM.SubGroupID,
                    YPIPO.YPIReceiveMasterID YPINo, YPIPO.YPIReceiveMasterID
	                FROM CDAPOMaster CPM
                    Left Join YarnPIReceivePO YPIPO ON YPIPO.YPOMasterID = CPM.CDAPOMasterID
                    Left Join YarnPIReceiveMaster YPRM ON YPRM.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID And YPRM.IsCDA = 1
                    WHERE CDAPOMasterID = {yarnPoMasterId}
                )
                SELECT YPM.CDAPOMasterID YPOMasterID, YPM.PONo, CONVERT(varchar, YPM.PODate, 101) PODate, YPM.CompanyID, YPM.SupplierID, 
                YPM.QuotationRefNo, YPM.IncoTermsID, YPM.PaymentTermsID, YPM.TypeOfLCID, YPM.TenureofLC, YPM.CalculationofTenure, 
                YPM.CreditDays, YPM.ReImbursementCurrencyID, YPM.Charges, YPM.CountryOfOriginID, YPM.TransShipmentAllow, YPM.SubGroupID, 
                YPM.ShippingTolerance, YPM.PortofLoadingID, YPM.PortofDischargeID, YPM.ShipmentModeID, YPM.OfferValidity, YPM.Remarks, 
                C.Name SupplierName, CE.ShortName CompanyName, YPM.YPINo, YPM.YPIReceiveMasterID
                FROM YPM
                LEFT JOIN  {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPM.CompanyID
                LEFT JOIN  {DbNames.EPYSL}..Contacts C ON C.ContactID = YPM.SupplierID
                GROUP BY YPM.CDAPOMasterID, YPM.PONo, YPM.PODate, YPM.CompanyID, YPM.SupplierID, YPM.QuotationRefNo, YPM.IncoTermsID, 
                YPM.PaymentTermsID, YPM.TypeOfLCID, YPM.TenureofLC, YPM.CalculationofTenure, YPM.CreditDays, YPM.ReImbursementCurrencyID, 
                YPM.Charges, YPM.CountryOfOriginID, YPM.TransShipmentAllow, YPM.SubGroupID, YPM.ShippingTolerance, YPM.PortofLoadingID, 
                YPM.PortofDischargeID, YPM.ShipmentModeID, YPM.OfferValidity, YPM.Remarks, C.Name, CE.ShortName, YPM.YPINo, YPM.YPIReceiveMasterID;

                -- Childs
                ;With YPC As (
	                Select * From CDAPOChild Where CDAPOMasterID = {yarnPoMasterId}
                )
                SELECT YPC.CDAPOChildID As YPIReceiveChildID, YPC.ItemMasterID, YPC.UnitID, YPC.Remarks, HSCode, U.DisplayUnitDesc, YPC.CDAProgramId YarnProgramId, 
                YarnProgram.ValueName AS YarnProgram, YPC.POQty, 0 As POReceivedQty, YPC.Rate, YPC.POValue, YPC.POQty PIQty, YPC.Rate PIRate, YPC.POValue PIValue,
                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, 
                ISV4.SegmentValue Segment4ValueDesc
                FROM YPC 
                Inner Join CDAPOMaster CDAPOM ON YPC.CDAPOMasterID = CDAPOM.CDAPOMasterID
                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = CDAPOM.SupplierID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YPC.ItemMasterID   
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                INNER JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YPC.UnitID   
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue YarnProgram ON YarnProgram.ValueID = YPC.CDAProgramId 
                GROUP BY YPC.CDAPOChildID, YPC.ItemMasterID, YPC.UnitID, YPC.Remarks, HSCode, U.DisplayUnitDesc, 
                YPC.CDAProgramId, YarnProgram.ValueName, YPC.POQty, YPC.Rate, YPC.POValue, YPC.POQty, YPC.Rate, 
                ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue;

                -- YarnPIReceivePO
                ;With A As 
                (
	                Select B.YPOMasterID, B.ItemMasterID, Sum(B.PIQty) PIQty
	                From YarnPIReceivePO B
                    Inner Join YarnPIReceiveMaster YPRM ON YPRM.YPIReceiveMasterID = B.YPIReceiveMasterID And YPRM.IsCDA = 1
	                Where B.YPOMasterID in ({yarnPoMasterId})
	                Group By B.YPOMasterID, B.ItemMasterID
                ), B As 
                (
	                Select CDAPOMasterID As YPOMasterID, ItemMasterID, SUM(POQty) POQty
	                From CDAPOChild
	                Where CDAPOMasterID in ({yarnPoMasterId})
	                Group By CDAPOMasterID, ItemMasterID
                )
                Select B.YPOMasterID, B.ItemMasterID, B.POQty, BalancePOQty = ISNULL(B.POQty,0) - ISNULL(A.PIQty,0)
                From B
                Left Join A On A.YPOMasterID = B.YPOMasterID And A.ItemMasterID = B.ItemMasterID; 

                -- Companies
                ;SELECT CAST(CompanyID AS VARCHAR) AS id, CompanyName AS [text]
                FROM {DbNames.EPYSL}..CompanyEntity
                WHERE CompanyID IN(8,6)
                ORDER BY CompanyName;

                -- Type of LC
                ;SELECT CAST(PaymentMethodID AS VARCHAR) AS id, PaymentMethodName AS [text]
                FROM {DbNames.EPYSL}..PaymentMethod
                WHERE PaymentMethodID > 0
                ORDER BY PaymentMethodID;
                
                -- Inco Terms
                ;SELECT CAST(CIT.IncoTermsID AS VARCHAR) AS id, IT.IncoTermsName AS [text] 
                FROM {DbNames.EPYSL}..ContactIncoTerms CIT
                INNER JOIN {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = CIT.ContactID
                INNER JOIN {DbNames.EPYSL}..IncoTerms IT ON IT.IncoTermsID = CIT.IncoTermsID
                WHERE CAI.ContactID = {supplierId}

                -- Payment Terms
                ;SELECT CAST(CPT.PaymentTermsID AS VARCHAR) AS id, PT.PaymentTermsName AS [text]
                FROM {DbNames.EPYSL}..ContactPaymentTerms CPT
                INNER JOIN {DbNames.EPYSL}..PaymentTrems PT ON PT.PaymentTermsID = CPT.PaymentTermsID 
                WHERE CPT.ContactID = {supplierId};

                -- Countries of Origin
                SELECT CAST(CT.CountryID AS VARCHAR) AS id, C.CountryName AS [text]
                FROM {DbNames.EPYSL}..Contacts CT
                INNER JOIN {DbNames.EPYSL}..Country C ON C.CountryID = CT.CountryID
                WHERE ContactID = {supplierId}

                -- Entity Type List
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName [text], CAST(EntityTypeID AS VARCHAR) [desc]
                FROM {DbNames.EPYSL}..EntityTypeValue 
                WHERE ValueName <> 'Select'
                ORDER BY ValueName;

                ;SELECT	CAST(AdditionalValueID AS VARCHAR) AS id, AdditionalValueName text 
                FROM YarnAdditionalValueSetup;

                ;SELECT	CAST(DeductionValueID AS VARCHAR) AS id, DeductionValueName text 
                FROM YarnDeductionValueSetup";

            var data = new YarnPIReceiveMaster();
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);

                data = records.Read<YarnPIReceiveMaster>().FirstOrDefault();
                data.Childs = records.Read<YarnPIReceiveChild>().ToList();
                data.YarnPIReceivePOList = records.Read<YarnPIReceivePO>().ToList();
                data.CompanyList = records.Read<Select2OptionModel>().ToList();
                data.TypeOfLCList = records.Read<Select2OptionModel>().ToList();
                data.IncoTermsList = records.Read<Select2OptionModel>().ToList();
                data.PaymentTermsList = records.Read<Select2OptionModel>().ToList();
                data.CountryOfOriginList = records.Read<Select2OptionModel>().ToList();

                var entityTypeList = records.Read<Select2OptionModel>().ToList();
                data.CalculationofTenureList = entityTypeList.FindAll(x => x.desc == EntityTypeConstants.CALCULATION_OF_TENURE.ToString());
                data.ShipmentModeList = entityTypeList.FindAll(x => x.desc == EntityTypeConstants.SHIPMENT_MODE.ToString());
                data.LCTenureList = entityTypeList.FindAll(x => x.desc == EntityTypeConstants.LC_TENURE.ToString());
                data.PortofLoadingList = entityTypeList.FindAll(x => x.desc == EntityTypeConstants.PORT_OF_LOADING.ToString());
                data.OfferValidityList = entityTypeList.FindAll(x => x.desc == EntityTypeConstants.OFFER_VALIDITY.ToString());
                data.CreditDaysList = entityTypeList.FindAll(x => x.desc == EntityTypeConstants.LC_TENURE.ToString());
                data.PortofDischargeList = entityTypeList.Where(x => x.desc == EntityTypeConstants.PORT_OF_DISCHARGE.ToString());

                data.AdditionalValueSetupList = records.Read<Select2OptionModel>().ToList();
                data.DeductionValueSetupList = records.Read<Select2OptionModel>().ToList();

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
        public async Task<List<AvailablePOForPI>> GetAvailablePOForPIAsync(int[] poMasterIdArray, int supplierId, int companyId, int yPIReceiveMasterID)
        {
            var param = new { YPOMasterIds = poMasterIdArray };
            var sql = $@"
            With A As
            (
	            Select M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.QuotationRefNo, CE.ShortName As CompanyName,
	            M.CompanyID,SUM(C.POQty) TotalQty, SUM(C.POQty * C.Rate) TotalValue, SUM(Isnull(YPIPO.PIQTY,0))TotalPIQty,
	            (SUM(Isnull(C.POQty, 0)) - SUM(Isnull(YPIPO.PIQTY, 0)))BalancePOQty,
                ((SUM(Isnull(C.POQty, 0))- SUM(Isnull(YPIPO.PIQTY, 0))) * SUM(C.Rate))BalancePOValue
	            From YarnPOMaster M
	            Inner Join YarnPOChild C ON C.YPOMasterID = M.YPOMasterID
	            Inner Join {DbNames.EPYSL}..CompanyEntity CE ON M.CompanyID = CE.CompanyID
	            Left Join YarnPIReceivePO YPIPO ON M.YPOMasterID = YPIPO.YPOMasterID And YPIPO.ItemMasterID = C.ItemMasterID
                    And YPIPO.YPIReceiveMasterID != {yPIReceiveMasterID}
                Left Join YarnPIReceiveMaster YPRM ON YPRM.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID And YPRM.IsCDA = 0
	            Where M.Approved = 1 And M.SupplierID = {supplierId} And M.CompanyID = {companyId}
	            Group By M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.QuotationRefNo, CE.ShortName,
	            M.CompanyID
            )
            Select * From A Where BalancePOQty > 0; ";

            var records = await _service.GetDataAsync<AvailablePOForPI>(sql, param);  
            records.ForEach(x => { x.IsChecked = poMasterIdArray.Contains(x.YPOMasterID); });
            records.OrderBy(x => x.IsChecked);
            return records;
        } 
        public async Task<List<AvailablePOForPI>> GetAvailableCDAPOForPIAsync(int[] poMasterIdArray, int supplierId, int companyId, int yPIReceiveMasterID)
        {
            var param = new { YPOMasterIds = poMasterIdArray };
            var sql = $@"
            With A As
            (
	            Select M.CDAPOMasterID As YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.QuotationRefNo, CE.ShortName As CompanyName,
	            M.CompanyID,SUM(C.POQty) TotalQty, SUM(C.POQty * C.Rate) TotalValue, SUM(Isnull(YPIPO.PIQTY,0))TotalPIQty,
	            (SUM(Isnull(C.POQty, 0)) - SUM(Isnull(YPIPO.PIQTY, 0)))BalancePOQty,
                ((SUM(Isnull(C.POQty, 0))- SUM(Isnull(YPIPO.PIQTY, 0))) * SUM(C.Rate))BalancePOValue
	            From CDAPOMaster M
	            Inner Join CDAPOChild C ON C.CDAPOMasterID = M.CDAPOMasterID
	            Inner Join {DbNames.EPYSL}..CompanyEntity CE ON M.CompanyID = CE.CompanyID
	            Left Join YarnPIReceivePO YPIPO ON M.CDAPOMasterID = YPIPO.YPOMasterID 
                    And YPIPO.ItemMasterID = C.ItemMasterID And YPIPO.YPIReceiveMasterID != {yPIReceiveMasterID}
                Left Join YarnPIReceiveMaster YPRM ON YPRM.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID And YPRM.IsCDA = 0
	            Where M.Approved = 1 And M.SupplierID = {supplierId} And M.CompanyID = {companyId} 
	            Group By M.CDAPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.QuotationRefNo, CE.ShortName, M.CompanyID
            )
            Select * From A Where BalancePOQty > 0; ";

            var records = await _service.GetDataAsync<AvailablePOForPI>(sql, param); //await _sqlQueryRepository.GetDataDapperAsync<AvailablePOForPI>(sql, param);
            records.ForEach(x => { x.IsChecked = poMasterIdArray.Contains(x.YPOMasterID); });
            records.OrderBy(x => x.IsChecked);
            return records;
        }
        public async Task<YarnPIReceiveMaster> GetAsync(int id, int supplierId, int companyId, bool isYarnReceivePage)
        {
            var query = ""; 
            if(isYarnReceivePage)
            {
                query = $@";With M As 
                (
	                Select YPIReceiveMasterID, PIDate, RevisionNo, RevisionDate, SupplierID, CompanyID, Remarks, ReceivePI, ReceivePIBy, 
                    ReceivePIDate, PIFilePath, AttachmentPreviewTemplate, NetPIValue, IncoTermsID, PaymentTermsID, TypeOfLCID, TenureofLC, 
                    CalculationofTenure, CreditDays, OfferValidity, ReImbursementCurrencyID, Charges, CountryOfOriginID, TransShipmentAllow, 
                    ShippingTolerance, PortofLoadingID, PortofDischargeID, ShipmentModeID, YPINo, RejectReason, PONo,Reject 
	                From YarnPIReceiveMaster
	                WHERE YPIReceiveMasterID = {id} AND IsCDA = 0
                )
                Select M.YPIReceiveMasterID, M.PIDate, M.RevisionNo, M.RevisionDate, M.SupplierID, M.CompanyID, M.Remarks, M.ReceivePI, 
                M.ReceivePIBy, M.ReceivePIDate, M.PIFilePath, M.AttachmentPreviewTemplate, M.NetPIValue, M.IncoTermsID, M.PaymentTermsID, 
                M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.OfferValidity, M.ReImbursementCurrencyID, M.Charges, 
                M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, M.ShipmentModeID, 
                M.YPINo, M.RejectReason, M.Reject,M.PONo, CE.ShortName As CompanyName, C.ShortName As SupplierName
                From M
                LEFT JOIN  {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID
                LEFT JOIN  {DbNames.EPYSL}..Contacts C ON C.ContactID = M.SupplierID;
                
                -- Child Items
                ;With 
                YPC As (
	                Select * From YarnPIReceiveChild Where YPIReceiveMasterID = {id}
                )
                SELECT YPC.YPIReceiveChildID,YPC.YPOChildID, YPC.ItemMasterID, YPC.YarnCategory, YPC.UnitID, YPC.Remarks, YPC.HSCode, 
                YPC.YarnLotNo, U.DisplayUnitDesc, YarnProgram.ValueName AS YarnProgram, YPC.POQty, YPC.Rate, YPC.PIValue POValue, 
                --YPC.PIQty,--YPO.YPOChildID,
                PIQty=CASE When YPC.PIQty<0 then 0 else YPC.PIQty end,
                YPC.PIQty As POReceivedQty,YPC.PIValue, YPC.ShadeCode, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
                ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, 
                ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc
                FROM YPC
                --Left Join YarnPIReceivePO YPO ON YPO.YPIReceiveMasterID = YPC.YPIReceiveMasterID And YPO.ItemMasterID = YPC.ItemMasterID 
				--Left Join YarnPOChild YPOC ON YPOC.YPOMasterID = YPO.YPOMasterID And YPOC.ItemMasterID = YPO.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YPC.ItemMasterID
				LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YPC.UnitID   
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue YarnProgram ON YPC.YarnProgramId = YarnProgram.ValueID 
                GROUP BY YPC.YPIReceiveChildID, YPC.ItemMasterID,YPC.YPOChildID, YPC.YarnCategory, YPC.UnitID, YPC.Remarks, YPC.HSCode, YPC.YarnLotNo, 
                U.DisplayUnitDesc, YarnProgram.ValueName, YPC.POQty, YPC.PIQty, YPC.Rate, YPC.PIValue, YPC.ShadeCode, ISV1.SegmentValue, --YPO.YPOChildID,
                ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue;

                -- PIReceivePOList
                /*;With 
                PO As (
	                Select YPIReceivePOID, YPIReceiveMasterID, YPOMasterID, ItemMasterID, PIQty, RevisionNo
	                From YarnPIReceivePO 
	                Where YPIReceiveMasterID = {id}
                )
                Select PO.YPIReceivePOID, POM.PONo,  PO.YPIReceiveMasterID, PO.YPOMasterID, PO.ItemMasterID, 
                POM.RevisionNo, POM.QuotationRefNo, PIQty, CE.ShortName CompanyName, SUM(POC.POQty) TotalQty, 
                SUM(POC.POQty * POC.Rate) TotalValue
                From PO
                Inner Join YarnPOMaster POM On POM.YPOMasterID = PO.YPOMasterID
                Inner Join YarnPOChild POC On POC.YPOMasterID = PO.YPOMasterID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = POM.CompanyID
                Group By PO.YPIReceivePOID, POM.PONo, PO.YPIReceiveMasterID, PO.YPOMasterID, PO.ItemMasterID, 
                POM.RevisionNo, POM.QuotationRefNo, CE.ShortName, PIQty; */

                ;With P As
                (
	                Select YPIReceiveMasterID
	                From YarnPIReceiveMaster
	                Where  YPIReceiveMasterID = {id}
                ), PO As 
                (
	                Select B.YPOMasterID, B.ItemMasterID, Sum(B.PIQty) PIQty
	                From YarnPIReceivePO B
	                Where B.YPOMasterID IN(Select YPOMasterID From YarnPIReceiveMaster Where YPIReceiveMasterID = {id})
                    And YPIReceiveMasterID != {id}
	                Group By B.YPOMasterID, B.ItemMasterID
                ), C As 
                (
	                Select YPOMasterID, ItemMasterID, Rate, SUM(POQty) POQty
	                From YarnPOChild
	                Where YPOMasterID IN(Select YPOMasterID From YarnPIReceiveMaster Where YPIReceiveMasterID = {id})
	                Group By YPOMasterID, Rate, ItemMasterID
                )
                Select B.YPIReceivePOID, B.YPIReceiveMasterID, B.YPOMasterID,YPO.RevisionNo MasterRevisioNo,
                B.ItemMasterID, B.RevisionNo, C.Rate, ISNULL(C.POQty,0)POQty, ISNULL(PO.PIQty,0)PIQty, 
                BalancePOQty = ISNULL(C.POQty,0) - ISNULL(PO.PIQty,0)
                From YarnPIReceivePO B
                Inner Join P On P.YPIReceiveMasterID = B.YPIReceiveMasterID
                Inner Join YarnPOMaster YPO on YPO.YPOMasterID=B.YPOMasterID
                Left Join PO ON PO.YPOMasterID = b.YPOMasterID And PO.ItemMasterID = B.ItemMasterID
                Left Join C ON C.YPOMasterID = b.YPOMasterID And C.ItemMasterID = B.ItemMasterID; ";
            }
            else
            {
                query = $@";With YPRM As 
                (
	                Select YPIReceiveMasterID, PIDate, RevisionNo, RevisionDate, SupplierID, CompanyID, Remarks, ReceivePI, ReceivePIBy, 
                    ReceivePIDate, PIFilePath, AttachmentPreviewTemplate, NetPIValue, IncoTermsID, PaymentTermsID, TypeOfLCID, 
                    TenureofLC, CalculationofTenure, CreditDays, OfferValidity, ReImbursementCurrencyID, Charges, CountryOfOriginID, 
                    TransShipmentAllow, ShippingTolerance, PortofLoadingID, PortofDischargeID, ShipmentModeID, YPINo, RejectReason, Reject,PONo 
	                From YarnPIReceiveMaster
	                WHERE YPIReceiveMasterID = {id} AND IsCDA = 1
                )
                Select YPRM.YPIReceiveMasterID, YPRM.PIDate, YPRM.RevisionNo, YPRM.RevisionDate, YPRM.SupplierID, YPRM.CompanyID, YPRM.Remarks, 
                YPRM.ReceivePI, YPRM.ReceivePIBy, YPRM.ReceivePIDate, YPRM.PIFilePath, YPRM.AttachmentPreviewTemplate, YPRM.NetPIValue, 
                YPRM.IncoTermsID, YPRM.PaymentTermsID, YPRM.TypeOfLCID, YPRM.TenureofLC, YPRM.CalculationofTenure, YPRM.CreditDays, YPRM.OfferValidity, 
                YPRM.ReImbursementCurrencyID, YPRM.Charges, YPRM.CountryOfOriginID, YPRM.TransShipmentAllow, YPRM.ShippingTolerance, YPRM.PortofLoadingID,
                YPRM.PortofDischargeID, YPRM.ShipmentModeID, YPRM.YPINo, YPRM.RejectReason, YPRM.Reject,YPRM.PONo, CE.ShortName CompanyName, C.ShortName SupplierName
                From YPRM
                LEFT JOIN  {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPRM.CompanyID
                LEFT JOIN  {DbNames.EPYSL}..Contacts C ON C.ContactID = YPRM.SupplierID;
                
                -- Child Items
                ;With 
                YPC As 
                (
	                Select * From YarnPIReceiveChild Where YPIReceiveMasterID = {id}
                )
                SELECT YPC.YPIReceiveChildID, YPC.ItemMasterID, YPC.YarnCategory, YPC.UnitID, YPC.Remarks, YPC.HSCode, YPC.YarnLotNo, 
                U.DisplayUnitDesc, YarnProgram.ValueName AS YarnProgram, YPC.POQty, YPC.Rate, YPC.PIValue As POValue, YPC.PIQty, YPC.PIValue, 
                YPC.ShadeCode, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, 
                ISV4.SegmentValue Segment4ValueDesc
                FROM YPC
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YPC.ItemMasterID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YPC.UnitID   
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue YarnProgram ON YarnProgram.ValueID  = YPC.YarnProgramID
                GROUP BY YPC.YPIReceiveChildID, YPC.ItemMasterID, YPC.YarnCategory, YPC.UnitID, YPC.Remarks, YPC.HSCode, YPC.YarnLotNo, 
                U.DisplayUnitDesc, YarnProgram.ValueName, YPC.POQty, YPC.PIQty, YPC.Rate, YPC.PIValue, YPC.ShadeCode,
                ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue;

                -- PIReceivePOList
                ;With PO As 
                (
	                Select YPIReceivePOID, YPIReceiveMasterID, YPOMasterID 
	                From YarnPIReceivePO 
	                Where YPIReceiveMasterID = {id}
                )
                Select PO.YPIReceivePOID, POM.PONo,  PO.YPIReceiveMasterID, PO.YPOMasterID, POM.RevisionNo, POM.QuotationRefNo, 
                CE.ShortName As CompanyName, SUM(POC.POQty) TotalQty, SUM(POC.POQty * POC.Rate) TotalValue
                From PO
                Inner Join CDAPOMaster POM On PO.YPOMasterID = POM.CDAPOMasterID
                Inner Join CDAPOChild POC On PO.YPOMasterID = POC.CDAPOMasterID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On POM.CompanyID = CE.CompanyID
                Group By PO.YPIReceivePOID, POM.PONo, PO.YPIReceiveMasterID, PO.YPOMasterID, POM.RevisionNo, POM.QuotationRefNo, CE.ShortName;";
            }
            var sql = $@"
                -- YarnPIReceiveAdditionalValueList
                ;Select YPIReceiveAdditionalID, YPIReceiveMasterID, AdditionalValueID, AdditionalValue 
                From YarnPIReceiveAdditionalValue
                Where YPIReceiveMasterID = {id};

                -- YarnPIReceiveDeductionValueList
                ;Select YPIReceiveDeductionID, YPIReceiveMasterID, DeductionValueID, DeductionValue
                From YarnPIReceiveDeductionValue
                Where YPIReceiveMasterID = {id};

                -- Companies
                ;SELECT CAST(CompanyID AS VARCHAR) AS id, CompanyName AS [text]
                FROM {DbNames.EPYSL}..CompanyEntity
                WHERE CompanyID IN(8,6)
                ORDER BY CompanyName;

                -- Type of LC
                ;SELECT CAST(PaymentMethodID AS VARCHAR) AS id, PaymentMethodName AS [text]
                FROM {DbNames.EPYSL}..PaymentMethod
                WHERE PaymentMethodID > 0
                ORDER BY PaymentMethodID;
                
                -- Inco Terms
                ;SELECT CAST(CIT.IncoTermsID AS VARCHAR) AS id, IT.IncoTermsName AS [text] 
                FROM {DbNames.EPYSL}..ContactIncoTerms CIT
                INNER JOIN {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = CIT.ContactID
                INNER JOIN {DbNames.EPYSL}..IncoTerms IT ON IT.IncoTermsID = CIT.IncoTermsID
                WHERE CAI.ContactID = {supplierId}

                -- Payment Terms
                ;SELECT CAST(CPT.PaymentTermsID AS VARCHAR) AS id, PT.PaymentTermsName AS [text]
                FROM {DbNames.EPYSL}..ContactPaymentTerms CPT
                INNER JOIN {DbNames.EPYSL}..PaymentTrems PT ON PT.PaymentTermsID = CPT.PaymentTermsID 
                WHERE CPT.ContactID = {supplierId};

                -- Countries of Origin
                SELECT CAST(CT.CountryID AS VARCHAR) AS id, C.CountryName AS [text]
                FROM {DbNames.EPYSL}..Contacts CT
                INNER JOIN {DbNames.EPYSL}..Country C ON C.CountryID = CT.CountryID
                WHERE ContactID = {supplierId}

                -- Entity Type List
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName [text], CAST(EntityTypeID AS VARCHAR) [desc]
                FROM {DbNames.EPYSL}..EntityTypeValue 
                WHERE ValueName <> 'Select'
                ORDER BY ValueName;

                ;SELECT	CAST(AdditionalValueID AS VARCHAR) AS id, AdditionalValueName text 
                FROM YarnAdditionalValueSetup;

                ;SELECT	CAST(DeductionValueID AS VARCHAR) AS id, DeductionValueName text 
                FROM YarnDeductionValueSetup";

            var data = new YarnPIReceiveMaster();
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query + sql);
                data = records.Read<YarnPIReceiveMaster>().FirstOrDefault();
                data.Childs = records.Read<YarnPIReceiveChild>().ToList();
                data.YarnPIReceivePOList = records.Read<YarnPIReceivePO>().ToList();
                data.YarnPIReceiveAdditionalValueList = records.Read<YarnPIReceiveAdditionalValue>().ToList();
                data.YarnPIReceiveDeductionValueList = records.Read<YarnPIReceiveDeductionValue>().ToList();
                data.CompanyList = records.Read<Select2OptionModel>().ToList();
                data.TypeOfLCList = records.Read<Select2OptionModel>().ToList();
                data.IncoTermsList = records.Read<Select2OptionModel>().ToList();
                data.PaymentTermsList = records.Read<Select2OptionModel>().ToList();
                data.CountryOfOriginList = records.Read<Select2OptionModel>().ToList();
                var entityTypeList = records.Read<Select2OptionModel>().ToList();
                data.CalculationofTenureList = entityTypeList.FindAll(x => x.desc == EntityTypeConstants.CALCULATION_OF_TENURE.ToString());
                data.CreditDaysList = entityTypeList.Where(x => x.desc == EntityTypeConstants.LC_TENURE.ToString());
                data.ShipmentModeList = entityTypeList.FindAll(x => x.desc == EntityTypeConstants.SHIPMENT_MODE.ToString());
                data.LCTenureList = entityTypeList.FindAll(x => x.desc == EntityTypeConstants.LC_TENURE.ToString());
                data.PortofLoadingList = entityTypeList.FindAll(x => x.desc == EntityTypeConstants.PORT_OF_LOADING.ToString());
                data.OfferValidityList = entityTypeList.FindAll(x => x.desc == EntityTypeConstants.OFFER_VALIDITY.ToString());
                data.PortofDischargeList = entityTypeList.Where(x => x.desc == EntityTypeConstants.PORT_OF_DISCHARGE.ToString());
                data.AdditionalValueSetupList = records.Read<Select2OptionModel>().ToList();
                data.DeductionValueSetupList = records.Read<Select2OptionModel>().ToList();

                if(data.PONo.NullOrEmpty()) data.PONo = string.Join(",", data.YarnPIReceivePOList.Select(x => x.PONo));

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
        public async Task<YarnPIReceiveMaster> GetReviceAsync(int id, int supplierId, int companyId, bool isYarnReceivePage, string poIds)
        {
            var query = "";
            if (isYarnReceivePage)
            {
                query = $@"
                ;With M As 
                (
	                Select YPIReceiveMasterID, PIDate, RevisionNo, RevisionDate, SupplierID, CompanyID, Remarks, ReceivePI, ReceivePIBy, 
                    ReceivePIDate, PIFilePath, AttachmentPreviewTemplate, NetPIValue, IncoTermsID, PaymentTermsID, TypeOfLCID, TenureofLC, 
                    CalculationofTenure, CreditDays, OfferValidity, ReImbursementCurrencyID, Charges, CountryOfOriginID, TransShipmentAllow, 
                    ShippingTolerance, PortofLoadingID, PortofDischargeID, ShipmentModeID, YPINo, RejectReason, PONo,Reject 
	                From YarnPIReceiveMaster
	                WHERE YPIReceiveMasterID = {id} AND IsCDA = 0
                )
                /*
                Select M.YPIReceiveMasterID, M.PIDate, M.RevisionNo, M.RevisionDate, M.SupplierID, M.CompanyID, M.Remarks, M.ReceivePI, 
                M.ReceivePIBy, M.ReceivePIDate, M.PIFilePath, M.AttachmentPreviewTemplate, M.NetPIValue, M.IncoTermsID, M.PaymentTermsID, 
                M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.OfferValidity, M.ReImbursementCurrencyID, M.Charges, 
                M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, M.ShipmentModeID, 
                M.YPINo, M.RejectReason, M.Reject,M.PONo, CE.ShortName As CompanyName, C.ShortName As SupplierName
                */
                Select M.YPIReceiveMasterID,YarnPOMasterRevision= YPO.RevisionNo, M.PIDate, M.RevisionNo, M.RevisionDate, M.SupplierID, M.CompanyID, M.Remarks, M.ReceivePI, 
                M.ReceivePIBy, M.ReceivePIDate, M.PIFilePath, M.AttachmentPreviewTemplate, M.NetPIValue, YPO.IncoTermsID, YPO.PaymentTermsID, 
                YPO.TypeOfLCID, YPO.TenureofLC, YPO.CalculationofTenure, YPO.CreditDays, YPO.OfferValidity, M.ReImbursementCurrencyID, YPO.Charges, 
                YPO.CountryOfOriginID, YPO.TransShipmentAllow, YPO.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, M.ShipmentModeID, 
                M.YPINo, M.RejectReason, M.Reject,M.PONo, CE.ShortName As CompanyName, C.ShortName As SupplierName,YPO.YPOMasterID
                From M
                LEFT JOIN  {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID
                LEFT JOIN  {DbNames.EPYSL}..Contacts C ON C.ContactID = M.SupplierID
                LEFT JOIN YarnPIReceivePO YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	            LEFT JOIN YarnPOMaster YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID;
                
                -- Child Items
                ;With POC AS
                (
                    SELECT POC.YPOMasterID, POC.ItemMasterID,POC.YPOChildID, POC.YarnCategory, POC.UnitID, POC.POQty, POC.Rate, POC.YarnLotNo, POC.HSCode, POC.ShadeCode, POC.YarnProgramId, POM.RevisionNo,SUM(YPO.PIQty) AS POReceivedQty
	                FROM YarnPOChild POC
	                INNER JOIN YarnPOMaster POM ON POM.YPOMasterID = POC.YPOMasterID
                    LEFT JOIN YarnPIReceivePO YPO ON  YPO.YPOChildID = POC.YPOChildID --AND YPO.ItemMasterID = POC.ItemMasterID
	                Where POM.YPOMasterID In ({poIds})
	                GROUP BY POC.YPOMasterID, POC.ItemMasterID,POC.YPOChildID, POC.YarnCategory, POC.UnitID, POC.Rate, POC.YarnLotNo, POC.HSCode, POC.ShadeCode, POC.YarnProgramId, POM.RevisionNo,POC.POQty
               
                    /*
	                SELECT POC.YPOMasterID, POC.ItemMasterID,POC.YPOChildID, POC.YarnCategory, POC.UnitID, POQty = SUM(POC.POQty), POC.Rate, POC.YarnLotNo, POC.HSCode, POC.ShadeCode, POC.YarnProgramId, POM.RevisionNo
	                FROM YarnPOChild POC
	                INNER JOIN YarnPOMaster POM ON POM.YPOMasterID = POC.YPOMasterID
	                Where POM.YPOMasterID In ({poIds}) 
	                GROUP BY POC.YPOMasterID, POC.ItemMasterID,POC.YPOChildID, POC.YarnCategory, POC.UnitID, POC.Rate, POC.YarnLotNo, POC.HSCode, POC.ShadeCode, POC.YarnProgramId, POM.RevisionNo
                    */
                ),
                YPC As (
	                Select * 
	                From YarnPIReceiveChild 
	                Where YPIReceiveMasterID = {id}
                ), YPIPO As(
	                Select * 
	                From YarnPIReceivePO
	                Where YPIReceiveMasterID = {id}
                ),
				UsedItem AS
				(
					SELECT YPOC.YPOChildID --RC.ItemMasterID
					FROM YarnPIReceiveChild RC
					INNER JOIN YarnPIReceivePO RPO ON RPO.YPIReceiveMasterID = RC.YPIReceiveMasterID AND RPO.ItemMasterID = RC.ItemMasterID
					INNER JOIN YarnPOChild YPOC ON YPOC.YPOMasterID = RPO.YPOMasterID AND YPOC.YPOChildID = RPO.YPOChildID --AND YPOC.ItemMasterID = RPO.ItemMasterID
					INNER JOIN POC ON POC.YPOMasterID = RPO.YPOMasterID And POC.YPOChildID = RPO.YPOChildID
					LEFT JOIN YPC ON YPC.ItemMasterID = RC.ItemMasterID
					WHERE RC.YPIReceiveMasterID <> {id} AND YPC.YPIReceiveChildID IS NULL And POC.POQty = POC.POReceivedQty
					GROUP BY YPOC.YPOChildID
				)
                , FList AS 
                (
	                SELECT YPC.YPIReceiveChildID,YPIPO.YPIReceiveMasterID, POC.ItemMasterID,POC.YPOChildID, POC.YarnCategory, POC.UnitID, YPC.Remarks, HSCode = ISNULL(YPC.HSCode, POC.HSCode), 
	                YPC.YarnLotNo, U.DisplayUnitDesc, YarnProgram.ValueName AS YarnProgram, POC.POQty, POC.Rate, POValue = (POC.POQty * POC.Rate), 
					case When YPC.YPIReceiveChildID is not null Then YPIPO.PIQty Else POC.POQty-POC.POReceivedQty End As PIQty,
	                --PIQty = YPIPO.PIQty,
					--case When YPC.YPIReceiveChildID is not null Then POC.POReceivedQty Else '0' End As POReceivedQty,
					--SUM(YPIPO.PIQty) As POReceivedQty,
                    POC.POReceivedQty As POReceivedQty,
                    


                    PIValue = ISNULL(YPC.PIValue,(POC.POQty * POC.Rate)), ShadeCode = ISNULL(YPC.ShadeCode,POC.ShadeCode), ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
	                ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, 
	                ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc
	                FROM POC 
	                Left JOIN YPIPO ON YPIPO.YPOMasterID = POC.YPOMasterID  And YPIPO.YPOChildID = POC.YPOChildID --And YPIPO.ItemMasterID = POC.ItemMasterID
	                Left Join YPC ON YPC.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID  And YPC.YPOChildID = YPIPO.YPOChildID -- AND YPC.Rate = POC.Rate AND ISNULL(YPC.ShadeCode,'') = ISNULL(POC.ShadeCode,'')
	                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = POC.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YPC.UnitID   
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue YarnProgram ON YPC.YarnProgramId = YarnProgram.ValueID
                    WHERE POC.YPOChildID NOT IN (SELECT YPOChildID FROM UsedItem)
	                GROUP BY YPC.YPIReceiveChildID,YPIPO.YPIReceiveMasterID, POC.ItemMasterID,POC.YPOChildID, POC.YarnCategory, POC.UnitID, YPC.Remarks,YPC.HSCode, POC.HSCode, 
	                YPC.YarnLotNo, U.DisplayUnitDesc, YarnProgram.ValueName, POC.POQty, POC.Rate, 
	                YPC.PIQty,YPC.PIValue,YPC.ShadeCode,POC.ShadeCode, ISV1.SegmentValue, ISV2.SegmentValue, 
	                ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, 
	                ISV6.SegmentValue, ISV7.SegmentValue,YPIPO.PIQty,POC.POReceivedQty
                )
                SELECT * FROM FList ;

                -- PIReceivePOList
                /*;With 
                PO As (
	                Select YPIReceivePOID, YPIReceiveMasterID, YPOMasterID, ItemMasterID, PIQty, RevisionNo
	                From YarnPIReceivePO 
	                Where YPIReceiveMasterID = {id}
                )
                Select PO.YPIReceivePOID, POM.PONo,  PO.YPIReceiveMasterID, PO.YPOMasterID, PO.ItemMasterID, 
                POM.RevisionNo, POM.QuotationRefNo, PIQty, CE.ShortName CompanyName, SUM(POC.POQty) TotalQty, 
                SUM(POC.POQty * POC.Rate) TotalValue
                From PO
                Inner Join YarnPOMaster POM On POM.YPOMasterID = PO.YPOMasterID
                Inner Join YarnPOChild POC On POC.YPOMasterID = PO.YPOMasterID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = POM.CompanyID
                Group By PO.YPIReceivePOID, POM.PONo, PO.YPIReceiveMasterID, PO.YPOMasterID, PO.ItemMasterID, 
                POM.RevisionNo, POM.QuotationRefNo, CE.ShortName, PIQty; */

                ;With P As
                (
	                Select YPIReceiveMasterID
	                From YarnPIReceiveMaster
	                Where  YPIReceiveMasterID = {id}
                ), PO As 
                (
	                Select B.YPOMasterID, B.ItemMasterID, Sum(B.PIQty) PIQty
	                From YarnPIReceivePO B
	                Where B.YPOMasterID IN(Select YPOMasterID From YarnPIReceiveMaster Where YPIReceiveMasterID = {id})
                    And YPIReceiveMasterID != {id}
	                Group By B.YPOMasterID, B.ItemMasterID
                ), C As 
                (
	                Select YPOMasterID, ItemMasterID, Rate, SUM(POQty) POQty
	                From YarnPOChild
	                Where YPOMasterID IN(Select YPOMasterID From YarnPIReceiveMaster Where YPIReceiveMasterID = {id})
	                Group By YPOMasterID, Rate, ItemMasterID
                )
                Select B.YPIReceivePOID, B.YPIReceiveMasterID, B.YPOMasterID,YPO.RevisionNo MasterRevisioNo,
                B.ItemMasterID, B.RevisionNo, C.Rate, ISNULL(C.POQty,0)POQty, ISNULL(PO.PIQty,0)PIQty, 
                BalancePOQty = ISNULL(C.POQty,0) - ISNULL(PO.PIQty,0)
                From YarnPIReceivePO B
                Inner Join P On P.YPIReceiveMasterID = B.YPIReceiveMasterID
                Inner Join YarnPOMaster YPO on YPO.YPOMasterID=B.YPOMasterID
                Left Join PO ON PO.YPOMasterID = b.YPOMasterID And PO.ItemMasterID = B.ItemMasterID
                Left Join C ON C.YPOMasterID = b.YPOMasterID And C.ItemMasterID = B.ItemMasterID; ";
            }
            else
            {
                query = $@";With YPRM As 
                (
	                Select YPIReceiveMasterID, PIDate, RevisionNo, RevisionDate, SupplierID, CompanyID, Remarks, ReceivePI, ReceivePIBy, 
                    ReceivePIDate, PIFilePath, AttachmentPreviewTemplate, NetPIValue, IncoTermsID, PaymentTermsID, TypeOfLCID, 
                    TenureofLC, CalculationofTenure, CreditDays, OfferValidity, ReImbursementCurrencyID, Charges, CountryOfOriginID, 
                    TransShipmentAllow, ShippingTolerance, PortofLoadingID, PortofDischargeID, ShipmentModeID, YPINo, RejectReason, Reject,PONo 
	                From YarnPIReceiveMaster
	                WHERE YPIReceiveMasterID = {id} AND IsCDA = 1
                )
                Select YPRM.YPIReceiveMasterID, YPRM.PIDate, YPRM.RevisionNo, YPRM.RevisionDate, YPRM.SupplierID, YPRM.CompanyID, YPRM.Remarks, 
                YPRM.ReceivePI, YPRM.ReceivePIBy, YPRM.ReceivePIDate, YPRM.PIFilePath, YPRM.AttachmentPreviewTemplate, YPRM.NetPIValue, 
                YPRM.IncoTermsID, YPRM.PaymentTermsID, YPRM.TypeOfLCID, YPRM.TenureofLC, YPRM.CalculationofTenure, YPRM.CreditDays, YPRM.OfferValidity, 
                YPRM.ReImbursementCurrencyID, YPRM.Charges, YPRM.CountryOfOriginID, YPRM.TransShipmentAllow, YPRM.ShippingTolerance, YPRM.PortofLoadingID,
                YPRM.PortofDischargeID, YPRM.ShipmentModeID, YPRM.YPINo, YPRM.RejectReason, YPRM.Reject,YPRM.PONo, CE.ShortName CompanyName, C.ShortName SupplierName
                From YPRM
                LEFT JOIN  {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPRM.CompanyID
                LEFT JOIN  {DbNames.EPYSL}..Contacts C ON C.ContactID = YPRM.SupplierID;
                
                -- Child Items
                ;With 
                YPC As 
                (
	                Select * From YarnPIReceiveChild Where YPIReceiveMasterID = {id}
                )
                SELECT YPC.YPIReceiveChildID, YPC.ItemMasterID, YPC.YarnCategory, YPC.UnitID, YPC.Remarks, YPC.HSCode, YPC.YarnLotNo, 
                U.DisplayUnitDesc, YarnProgram.ValueName AS YarnProgram, YPC.POQty, YPC.Rate, YPC.PIValue As POValue, YPC.PIQty, YPC.PIValue, 
                YPC.ShadeCode, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, 
                ISV4.SegmentValue Segment4ValueDesc
                FROM YPC
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YPC.ItemMasterID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YPC.UnitID   
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue YarnProgram ON YarnProgram.ValueID  = YPC.YarnProgramID
                GROUP BY YPC.YPIReceiveChildID, YPC.ItemMasterID, YPC.YarnCategory, YPC.UnitID, YPC.Remarks, YPC.HSCode, YPC.YarnLotNo, 
                U.DisplayUnitDesc, YarnProgram.ValueName, YPC.POQty, YPC.PIQty, YPC.Rate, YPC.PIValue, YPC.ShadeCode,
                ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue;

                -- PIReceivePOList
                ;With PO As 
                (
	                Select YPIReceivePOID, YPIReceiveMasterID, YPOMasterID 
	                From YarnPIReceivePO 
	                Where YPIReceiveMasterID = {id}
                )
                Select PO.YPIReceivePOID, POM.PONo,  PO.YPIReceiveMasterID, PO.YPOMasterID, POM.RevisionNo, POM.QuotationRefNo, 
                CE.ShortName As CompanyName, SUM(POC.POQty) TotalQty, SUM(POC.POQty * POC.Rate) TotalValue
                From PO
                Inner Join CDAPOMaster POM On PO.YPOMasterID = POM.CDAPOMasterID
                Inner Join CDAPOChild POC On PO.YPOMasterID = POC.CDAPOMasterID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On POM.CompanyID = CE.CompanyID
                Group By PO.YPIReceivePOID, POM.PONo, PO.YPIReceiveMasterID, PO.YPOMasterID, POM.RevisionNo, POM.QuotationRefNo, CE.ShortName;";
            }
            var sql = $@"
                -- YarnPIReceiveAdditionalValueList
                ;Select YPIReceiveAdditionalID, YPIReceiveMasterID, AdditionalValueID, AdditionalValue 
                From YarnPIReceiveAdditionalValue
                Where YPIReceiveMasterID = {id};

                -- YarnPIReceiveDeductionValueList
                ;Select YPIReceiveDeductionID, YPIReceiveMasterID, DeductionValueID, DeductionValue
                From YarnPIReceiveDeductionValue
                Where YPIReceiveMasterID = {id};

                -- Companies
                ;SELECT CAST(CompanyID AS VARCHAR) AS id, CompanyName AS [text]
                FROM {DbNames.EPYSL}..CompanyEntity
                WHERE CompanyID IN(8,6)
                ORDER BY CompanyName;

                -- Type of LC
                ;SELECT CAST(PaymentMethodID AS VARCHAR) AS id, PaymentMethodName AS [text]
                FROM {DbNames.EPYSL}..PaymentMethod
                WHERE PaymentMethodID > 0
                ORDER BY PaymentMethodID;
                
                -- Inco Terms
                ;SELECT CAST(CIT.IncoTermsID AS VARCHAR) AS id, IT.IncoTermsName AS [text] 
                FROM {DbNames.EPYSL}..ContactIncoTerms CIT
                INNER JOIN {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = CIT.ContactID
                INNER JOIN {DbNames.EPYSL}..IncoTerms IT ON IT.IncoTermsID = CIT.IncoTermsID
                WHERE CAI.ContactID = {supplierId}

                -- Payment Terms
                ;SELECT CAST(CPT.PaymentTermsID AS VARCHAR) AS id, PT.PaymentTermsName AS [text]
                FROM {DbNames.EPYSL}..ContactPaymentTerms CPT
                INNER JOIN {DbNames.EPYSL}..PaymentTrems PT ON PT.PaymentTermsID = CPT.PaymentTermsID 
                WHERE CPT.ContactID = {supplierId};

                -- Countries of Origin
                SELECT CAST(CT.CountryID AS VARCHAR) AS id, C.CountryName AS [text]
                FROM {DbNames.EPYSL}..Contacts CT
                INNER JOIN {DbNames.EPYSL}..Country C ON C.CountryID = CT.CountryID
                WHERE ContactID = {supplierId}

                -- Entity Type List
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName [text], CAST(EntityTypeID AS VARCHAR) [desc]
                FROM {DbNames.EPYSL}..EntityTypeValue 
                WHERE ValueName <> 'Select'
                ORDER BY ValueName;

                ;SELECT	CAST(AdditionalValueID AS VARCHAR) AS id, AdditionalValueName text 
                FROM YarnAdditionalValueSetup;

                ;SELECT	CAST(DeductionValueID AS VARCHAR) AS id, DeductionValueName text 
                FROM YarnDeductionValueSetup";

            var data = new YarnPIReceiveMaster();
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query + sql);
                data = records.Read<YarnPIReceiveMaster>().FirstOrDefault();
                data.Childs = records.Read<YarnPIReceiveChild>().ToList();
                data.YarnPIReceivePOList = records.Read<YarnPIReceivePO>().ToList();
                data.YarnPIReceiveAdditionalValueList = records.Read<YarnPIReceiveAdditionalValue>().ToList();
                data.YarnPIReceiveDeductionValueList = records.Read<YarnPIReceiveDeductionValue>().ToList();
                data.CompanyList = records.Read<Select2OptionModel>().ToList();
                data.TypeOfLCList = records.Read<Select2OptionModel>().ToList();
                data.IncoTermsList = records.Read<Select2OptionModel>().ToList();
                data.PaymentTermsList = records.Read<Select2OptionModel>().ToList();
                data.CountryOfOriginList = records.Read<Select2OptionModel>().ToList();

                var entityTypeList = records.Read<Select2OptionModel>().ToList();
                data.CalculationofTenureList = entityTypeList.FindAll(x => x.desc == EntityTypeConstants.CALCULATION_OF_TENURE.ToString());
                data.CreditDaysList = entityTypeList.Where(x => x.desc == EntityTypeConstants.LC_TENURE.ToString());
                data.ShipmentModeList = entityTypeList.FindAll(x => x.desc == EntityTypeConstants.SHIPMENT_MODE.ToString());
                data.LCTenureList = entityTypeList.FindAll(x => x.desc == EntityTypeConstants.LC_TENURE.ToString());
                data.PortofLoadingList = entityTypeList.FindAll(x => x.desc == EntityTypeConstants.PORT_OF_LOADING.ToString());
                data.OfferValidityList = entityTypeList.FindAll(x => x.desc == EntityTypeConstants.OFFER_VALIDITY.ToString());
                data.PortofDischargeList = entityTypeList.Where(x => x.desc == EntityTypeConstants.PORT_OF_DISCHARGE.ToString());
                data.AdditionalValueSetupList = records.Read<Select2OptionModel>().ToList();
                data.DeductionValueSetupList = records.Read<Select2OptionModel>().ToList();

                if (data.PONo.NullOrEmpty()) data.PONo = string.Join(",", data.YarnPIReceivePOList.Select(x => x.PONo));

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
        public async Task<YarnPIReceiveMaster> GetYarnPIReceiveChildItemsAsync(string ypoMasterIds, int yPIReceiveMasterID)
        {
            var sql = $@" 
            SELECT COALESCE(YPRC.YPIReceiveChildID, ROW_NUMBER() OVER(ORDER BY YPC.ItemMasterID ASC))YPIReceiveChildID, 
            YPOM.YPOMasterID, YPC.YPOChildID,YPOM.RevisionNo, YPC.ItemMasterID, YPC.YarnCategory, YPC.UnitID, YPOM.Remarks, YPC.HSCode, YPC.YarnLotNo, U.DisplayUnitDesc, 
            '' As YarnProgram,
            SUM(YPC.POQty) POQty, SUM(Isnull(YRP.PIQty,0))POReceivedQty, YPC.Rate, 0 As POValue, SUM(YPC.POQty - Isnull(YRP.PIQty,0))PIQty,
            SUM((YPC.POQty - Isnull(YRP.PIQty,0)) * YPC.Rate)PIValue, YPC.ShadeCode, ISV1.SegmentValue Segment1ValueDesc,
            ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
            ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc
            FROM YarnPOMaster YPOM
            INNER JOIN YarnPOChild YPC ON YPC.YPOMasterID = YPOM.YPOMasterID
            INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = YPOM.SupplierID
            LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YPC.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID 
            INNER JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YPC.UnitID
            --------------------------------------
            Left Join YarnPIReceivePO YRP On YRP.YPOMasterID = YPOM.YPOMasterID And YRP.ItemMasterID = YPC.ItemMasterID And YRP.YPIReceiveMasterID != {yPIReceiveMasterID}
            Left Join YarnPIReceiveChild YPRC on YPRC.YPIReceiveMasterID = {yPIReceiveMasterID} And YPRC.ItemMasterID = YPC.ItemMasterID And YPRC.Rate = YPC.Rate And ISNULL(YPRC.ShadeCode,'') = ISNULL(YPC.ShadeCode,'')
            Left Join YarnPIReceiveMaster YPRM ON YPRM.YPIReceiveMasterID = YRP.YPIReceiveMasterID And YPRM.IsCDA = 0
            WHERE YPOM.YPOMasterID IN({ypoMasterIds})
            GROUP BY YPRC.YPIReceiveChildID,YPOM.YPOMasterID, YPC.YPOChildID,YPOM.RevisionNo, YPC.ItemMasterID, YPC.YarnCategory, YPC.UnitID, YPOM.Remarks, YPC.HSCode, 
            YPC.YarnLotNo, U.DisplayUnitDesc, YPC.Rate, YPC.ShadeCode, ISV1.SegmentValue, ISV2.SegmentValue, 
            ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue;  

            ;With A As 
            (
	            Select B.YPOMasterID, B.ItemMasterID, Sum(B.PIQty) PIQty
	            From YarnPIReceivePO B
                Inner Join YarnPIReceiveMaster YPRM ON YPRM.YPIReceiveMasterID = B.YPIReceiveMasterID And YPRM.IsCDA = 0
	            Where B.YPOMasterID IN({ypoMasterIds}) And B.YPIReceiveMasterID <> {yPIReceiveMasterID}
	            Group By B.YPOMasterID, B.ItemMasterID
            ), B As 
            (
	            Select YPOMasterID, ItemMasterID, Rate, SUM(POQty) POQty
	            From YarnPOChild
	            Where YPOMasterID IN({ypoMasterIds})
	            Group By YPOMasterID, ItemMasterID, Rate
            ),
            C As 
            (
	            Select B.YPIReceivePOID, B.YPIReceiveMasterID, B.YPOMasterID, B.ItemMasterID, B.RevisionNo 
	            From YarnPIReceivePO B
                Inner Join YarnPIReceiveMaster YPRM ON YPRM.YPIReceiveMasterID = B.YPIReceiveMasterID And YPRM.IsCDA = 0
	            Where B.YPIReceiveMasterID = {yPIReceiveMasterID}  
            )
            Select COALESCE(C.YPIReceivePOID, ROW_NUMBER() OVER(ORDER BY B.ItemMasterID ASC))YPIReceivePOID,
            Isnull(C.YPIReceiveMasterID,0)YPIReceiveMasterID, B.YPOMasterID, B.ItemMasterID, YPO.RevisionNo MasterRevisioNo,Isnull(C.RevisionNo,0)RevisionNo, B.Rate, 
            B.POQty, Isnull(A.PIQty,0)PIQty, BalancePOQty = ISNULL(B.POQty,0) - ISNULL(A.PIQty,0)  
            From B
            Left Join A On A.YPOMasterID = B.YPOMasterID And A.ItemMasterID = B.ItemMasterID
            Inner Join YarnPOMaster YPO on YPO.YPOMasterID=B.YPOMasterID
            Left Join C On C.YPOMasterID = B.YPOMasterID And C.ItemMasterID = B.ItemMasterID; ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnPIReceiveMaster data = new YarnPIReceiveMaster();
                data.Childs = records.Read<YarnPIReceiveChild>().ToList();
                data.YarnPIReceivePOList = records.Read<YarnPIReceivePO>().ToList();
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
        public async Task<YarnPIReceiveMaster> GetCDAPIReceiveChildItemsAsync(string ypoMasterIds, int yPIReceiveMasterID)
        {
            var sql = $@" 
            SELECT COALESCE(YPRC.YPIReceiveChildID, ROW_NUMBER() OVER(ORDER BY YPC.ItemMasterID ASC))YPIReceiveChildID, 
            YPC.ItemMasterID, '' YarnCategory, YPC.UnitID, U.DisplayUnitDesc, '' As YarnProgram, 
            SUM(YPC.POQty) POQty, SUM(Isnull(YRP.PIQty,0))POReceivedQty, YPC.Rate, 0 As POValue, SUM(YPC.POQty - Isnull(YRP.PIQty,0))PIQty,
            SUM((YPC.POQty - Isnull(YRP.PIQty,0)) * YPC.Rate)PIValue, ISV1.SegmentValue Segment1ValueDesc, 
            ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc  
            FROM CDAPOMaster YPOM
            INNER JOIN CDAPOChild YPC ON YPC.CDAPOMasterID = YPOM.CDAPOMasterID
            INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = YPOM.SupplierID
            LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YPC.ItemMasterID
            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            INNER JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YPC.UnitID
            --------------------------------------
            Left Join YarnPIReceivePO YRP On YRP.YPOMasterID = YPOM.CDAPOMasterID And YRP.ItemMasterID = YPC.ItemMasterID And YRP.YPIReceiveMasterID != {yPIReceiveMasterID}
            Left Join YarnPIReceiveChild YPRC On YPRC.YPIReceiveMasterID = {yPIReceiveMasterID} And YPRC.ItemMasterID = YPC.ItemMasterID And YPRC.Rate = YPC.Rate
            Left Join YarnPIReceiveMaster YPRM ON YPRM.YPIReceiveMasterID = YRP.YPIReceiveMasterID And Isnull(YPRM.IsCDA,0) = 1
            WHERE YPOM.CDAPOMasterID IN({ypoMasterIds})
            GROUP BY YPRC.YPIReceiveChildID,YPC.ItemMasterID, YPC.UnitID, 
            U.DisplayUnitDesc, YPC.Rate, ISV1.SegmentValue, ISV2.SegmentValue, 
            ISV3.SegmentValue, ISV4.SegmentValue;   

            -- YarnPIReceivePO
            ;With A As 
            (
	            Select B.YPOMasterID, B.ItemMasterID, Sum(B.PIQty) PIQty
	            From YarnPIReceivePO B
                Inner Join YarnPIReceiveMaster YPRM ON YPRM.YPIReceiveMasterID = B.YPIReceiveMasterID And YPRM.IsCDA = 1
	            Where B.YPOMasterID IN({ypoMasterIds}) And B.YPIReceiveMasterID <> {yPIReceiveMasterID}
	            Group By B.YPOMasterID, B.ItemMasterID
            ), B As 
            (
                Select CDAPOMasterID As YPOMasterID, ItemMasterID, Rate, SUM(POQty) POQty
	            From CDAPOChild
	            Where CDAPOMasterID IN({ypoMasterIds})
	            Group By CDAPOMasterID, ItemMasterID, Rate
            ),
            C As 
            (
	            Select B.YPIReceivePOID, B.YPIReceiveMasterID, B.YPOMasterID, B.ItemMasterID, B.RevisionNo 
	            From YarnPIReceivePO B
                Inner Join YarnPIReceiveMaster YPRM ON YPRM.YPIReceiveMasterID = B.YPIReceiveMasterID And YPRM.IsCDA = 1
	            Where B.YPIReceiveMasterID = {yPIReceiveMasterID}  
            )
            Select COALESCE(C.YPIReceivePOID, ROW_NUMBER() OVER(ORDER BY B.ItemMasterID ASC))YPIReceivePOID,
            Isnull(C.YPIReceiveMasterID,0)YPIReceiveMasterID, B.YPOMasterID, B.ItemMasterID, Isnull(C.RevisionNo,0)RevisionNo, B.Rate, 
            B.POQty, Isnull(A.PIQty,0)PIQty, BalancePOQty = ISNULL(B.POQty,0) - ISNULL(A.PIQty,0)  
            From B
            Left Join A On A.YPOMasterID = B.YPOMasterID And A.ItemMasterID = B.ItemMasterID 
            Left Join C On C.YPOMasterID = B.YPOMasterID And C.ItemMasterID = B.ItemMasterID; ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnPIReceiveMaster data = new YarnPIReceiveMaster();
                data.Childs = records.Read<YarnPIReceiveChild>().ToList();
                data.YarnPIReceivePOList = records.Read<YarnPIReceivePO>().ToList();
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
        public async Task<YarnPIReceiveMaster> GetAllByIDAsync(int id)
        {
            string sql = $@"
            ;Select * From YarnPIReceiveMaster Where YPIReceiveMasterID = {id}

            ;Select * From YarnPIReceiveChild Where YPIReceiveMasterID = {id}

            ;Select * From YarnPIReceiveAdditionalValue Where YPIReceiveMasterID = {id}

            ;Select * From YarnPIReceiveDeductionValue Where YPIReceiveMasterID = {id}

            ;Select * From YarnPIReceivePO Where YPIReceiveMasterID = {id}

            ;select YPM.RevisionNo 
            From YarnPIReceivePO YPRP
            Inner Join YarnPOMaster YPM ON YPM.YPOMasterID=YPRP.YPOMasterID
            Where YPM.YPOMasterID=(Select Top 1 YPOMasterID From YarnPIReceivePO Where YPIReceiveMasterID = {id})

            ;Select YPC.* 
			From YarnPOChild YPC
			Inner Join YarnPIReceivePO YPIPO ON YPIPO.YPOMasterID = YPC.YPOMasterID
			Where YPIPO.YPIReceiveMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnPIReceiveMaster data = records.Read<YarnPIReceiveMaster>().FirstOrDefault();
                data.Childs = records.Read<YarnPIReceiveChild>().ToList();
                data.YarnPIReceiveAdditionalValueList = records.Read<YarnPIReceiveAdditionalValue>().ToList();
                data.YarnPIReceiveDeductionValueList = records.Read<YarnPIReceiveDeductionValue>().ToList();
                data.YarnPIReceivePOList = records.Read<YarnPIReceivePO>().ToList();
                data.YarnPOMasterRevision = records.Read<int>().FirstOrDefault();
                data.YarnPOChildList = records.Read<YarnPOChild>().ToList();

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
        public async Task SaveAsync(YarnPIReceiveMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                if (entity.IsRevise)
                {
                    await _connection.ExecuteAsync("spBackupYarnPIReceiveMaster_Full", new { YPIReceiveMasterID = entity.YPIReceiveMasterID, UserId = entity.AddedBy }, transaction, 30, CommandType.StoredProcedure);
                }
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity = await AddAsync(entity);
                        break;
                    case EntityState.Modified:
                        entity = await UpdateAsync(entity);
                        break;
                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Childs, transaction);
                await _service.SaveAsync(entity.YarnPIReceiveAdditionalValueList, transaction);
                await _service.SaveAsync(entity.YarnPIReceiveDeductionValueList, transaction);
                await _service.SaveAsync(entity.YarnPIReceivePOList, transaction);

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
        private async Task<YarnPIReceiveMaster> AddAsync(YarnPIReceiveMaster entity)
        {
            entity.YPIReceiveMasterID = await _service.GetMaxIdAsync(TableNames.YarnPIReceiveMaster);
            var maxYPIReceiveChildId = await _service.GetMaxIdAsync(TableNames.YarnPIReceiveChild, entity.Childs.Count);
            var maxYPIReceivePOId = await _service.GetMaxIdAsync(TableNames.YarnPIReceivePO, entity.YarnPIReceivePOList.Count);
            var maxYPIReceiveAddtionalId = await _service.GetMaxIdAsync(TableNames.YarnPIReceiveAdditionalValue, entity.YarnPIReceiveAdditionalValueList.Count);
            var maxYPIReceiveDeductionId = await _service.GetMaxIdAsync(TableNames.YarnPIReceiveDeductionValue, entity.YarnPIReceiveDeductionValueList.Count);
            
            foreach (var item in entity.Childs)
            {
                item.YPIReceiveChildID = maxYPIReceiveChildId++;
                item.YPIReceiveMasterID = entity.YPIReceiveMasterID;
                item.EntityState = EntityState.Added;
            }

            foreach (var item in entity.YarnPIReceivePOList)
            {
                if (item.EntityState == EntityState.Added)
                {
                    item.YPIReceivePOID = maxYPIReceivePOId++;
                    item.YPIReceiveMasterID = entity.YPIReceiveMasterID;
                    item.EntityState = EntityState.Added;
                }
            }

            foreach (var item in entity.YarnPIReceiveAdditionalValueList)
            {
                item.YPIReceiveAdditionalID = maxYPIReceiveAddtionalId++;
                item.YPIReceiveMasterID = entity.YPIReceiveMasterID;
                item.EntityState = EntityState.Added;
            }

            foreach (var item in entity.YarnPIReceiveDeductionValueList)
            {
                item.YPIReceiveDeductionID = maxYPIReceiveDeductionId++;
                item.YPIReceiveMasterID = entity.YPIReceiveMasterID;
                item.EntityState = EntityState.Added;
            }

            return entity;
        }
        private async Task<YarnPIReceiveMaster> UpdateAsync(YarnPIReceiveMaster entity)
        {
            var maxYPIReceiveChildId = await _service.GetMaxIdAsync(TableNames.YarnPIReceiveChild, entity.Childs.Where(x => x.EntityState == EntityState.Added).Count());
            var maxYPIReceivePOId = await _service.GetMaxIdAsync(TableNames.YarnPIReceivePO, entity.YarnPIReceivePOList.Where(x => x.EntityState == EntityState.Added).Count());
            var maxYPIReceiveAddtionalId = await _service.GetMaxIdAsync(TableNames.YarnPIReceiveAdditionalValue, entity.YarnPIReceiveAdditionalValueList.Where(x => x.EntityState == EntityState.Added).Count());
            var maxYPIReceiveDeductionId = await _service.GetMaxIdAsync(TableNames.YarnPIReceiveDeductionValue, entity.YarnPIReceiveDeductionValueList.Where(x => x.EntityState == EntityState.Added).Count());
            
            foreach (var item in entity.Childs.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.YPIReceiveChildID = maxYPIReceiveChildId++;
                        item.YPIReceiveMasterID = entity.YPIReceiveMasterID;
                        break;
                    case EntityState.Modified:
                        item.EntityState = EntityState.Modified;
                        break; 
                    default:
                        break;
                }
            }

            foreach (var item in entity.YarnPIReceivePOList.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.YPIReceivePOID = maxYPIReceivePOId++;
                        item.YPIReceiveMasterID = entity.YPIReceiveMasterID;
                        break;
                    case EntityState.Modified:
                        item.YPIReceiveMasterID = entity.YPIReceiveMasterID;
                        item.EntityState = EntityState.Modified;
                        break; 
                    default:
                        break;
                }
            }

            foreach (var item in entity.YarnPIReceiveAdditionalValueList.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.YPIReceiveAdditionalID = maxYPIReceiveAddtionalId++;
                        item.YPIReceiveMasterID = entity.YPIReceiveMasterID;
                        break;
                    case EntityState.Modified:
                        item.EntityState = EntityState.Modified;
                        break; 
                    default:
                        break;
                }
            }

            foreach (var item in entity.YarnPIReceiveDeductionValueList.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.YPIReceiveDeductionID = maxYPIReceiveDeductionId++;
                        item.YPIReceiveMasterID = entity.YPIReceiveMasterID;
                        break;
                    case EntityState.Modified:
                        item.EntityState = EntityState.Modified;
                        break; 
                    default:
                        break;
                }
            }

            return entity;
        }
        public async Task UpdateEntityAsync(YarnPIReceiveMaster entity)
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

        public async Task<List<YarnPIReceiveMaster>> GetAsync(Status status, PaginationInfo paginationInfo, string LcNo)
        {
            string orderBy;
            string sql;
            if (status == Status.Pending)
            {
                sql = $@"
                    With 
                    YPM As ( 
	                    --Select M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID
                        --, M.QuotationRefNo, M.DeliveryStartDate, M.DeliveryEndDate, M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID
                        --, M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID, M.Charges, 
                        --M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, 
                        --M.ShipmentModeID, M.OfferValidity,case when M.RevisionNo != YPIPO.RevisionNo AND YPIPO.YPOMasterID IS NOT NULL then 'Revise' else'New' end as Status
                        --, M.QualityApprovalProcedureId, M.AddedBy
                        --From YarnPOMaster M
                        --Left Join YarnPIReceivePO YPIPO ON M.YPOMasterID = YPIPO.YPOMasterID
                        --Where M.Approved = 1 AND YPIPO.YPOMasterID IS NULL
                        --UNION
                        --Select M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID
                        --, M.QuotationRefNo, M.DeliveryStartDate, M.DeliveryEndDate, M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID
                        --, M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID, M.Charges, 
                        --M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, 
                        --M.ShipmentModeID, M.OfferValidity,case when M.RevisionNo != YPIPO.RevisionNo AND YPIPO.YPOMasterID IS NOT NULL then 'Revise' else'New' end as Status
                        --, M.QualityApprovalProcedureId, M.AddedBy
                        --From YarnPOMaster M
                        --Left Join YarnPIReceivePO YPIPO ON M.YPOMasterID = YPIPO.YPOMasterID
                        --Where M.Approved = 1  AND YPIPO.YPOMasterID IS NOT NULL AND  M.RevisionNo != YPIPO.RevisionNo
                        Select M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID
	                    ,M.QuotationRefNo, M.DeliveryStartDate, M.DeliveryEndDate, M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID
	                    , M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID, M.Charges, 
	                    M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, 
	                    M.ShipmentModeID, M.OfferValidity,
	                    case when M.RevisionNo != YPIPO.RevisionNo AND YPIPO.YPOMasterID IS NOT NULL then 'Revise' else'New' end as Status
	                    , M.QualityApprovalProcedureId, M.AddedBy
	                    From YarnPOMaster M
	                    Left Join YarnPIReceivePO YPIPO ON M.YPOMasterID = YPIPO.YPOMasterID
	                    Where M.Approved = 1 AND YPIPO.YPOMasterID IS NULL
	                    GROUP BY M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID
	                    , M.QuotationRefNo, M.DeliveryStartDate, M.DeliveryEndDate, M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID
	                    , M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID, M.Charges, 
	                    M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, 
	                    M.ShipmentModeID, M.OfferValidity, M.RevisionNo, YPIPO.RevisionNo, YPIPO.YPOMasterID, M.QualityApprovalProcedureId, M.AddedBy

	                    UNION

	                    Select M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID
	                    , M.QuotationRefNo, M.DeliveryStartDate, M.DeliveryEndDate, M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID
	                    , M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID, M.Charges, 
	                    M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, 
	                    M.ShipmentModeID, M.OfferValidity,
	                    case when M.RevisionNo != YPIPO.RevisionNo AND YPIPO.YPOMasterID IS NOT NULL then 'Revise' else'New' end as Status
	                    , M.QualityApprovalProcedureId, M.AddedBy
	                    From YarnPOMaster M
	                    Left Join YarnPIReceivePO YPIPO ON M.YPOMasterID = YPIPO.YPOMasterID
	                    Where M.Approved = 1  AND YPIPO.YPOMasterID IS NOT NULL 
                        AND  M.RevisionNo != YPIPO.RevisionNo 
	                    GROUP BY M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID
	                    , M.QuotationRefNo, M.DeliveryStartDate, M.DeliveryEndDate, M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID
	                    , M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID, M.Charges, 
	                    M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, 
	                    M.ShipmentModeID, M.OfferValidity,YPIPO.YPOMasterID,M.RevisionNo,YPIPO.RevisionNo,M.QualityApprovalProcedureId, M.AddedBy
                        
                        Union

						Select M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID
	                    , M.QuotationRefNo, M.DeliveryStartDate, M.DeliveryEndDate, M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID
	                    , M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID, M.Charges, 
	                    M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, 
	                    M.ShipmentModeID, M.OfferValidity,
	                    case when M.RevisionNo = YPIPO.RevisionNo AND YPIPO.YPOMasterID IS NOT NULL then 'Revise' else'New' end as Status
	                    , M.QualityApprovalProcedureId, M.AddedBy
	                    From YarnPOMaster M
	                    Left Join YarnPIReceivePO YPIPO ON M.YPOMasterID = YPIPO.YPOMasterID
	                    Where M.Approved = 1  AND YPIPO.YPOMasterID IS NOT NULL 
                        AND  M.RevisionNo = YPIPO.RevisionNo 
	                    GROUP BY M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID
	                    , M.QuotationRefNo, M.DeliveryStartDate, M.DeliveryEndDate, M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID
	                    , M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID, M.Charges, 
	                    M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID, M.PortofDischargeID, 
	                    M.ShipmentModeID, M.OfferValidity,YPIPO.YPOMasterID,M.RevisionNo,YPIPO.RevisionNo,M.QualityApprovalProcedureId, M.AddedBy
                    ),
                    YPI AS(
	                    --SELECT YPM.YPOMasterID AS YPOMasterID, YPM.PONo, YPM.CompanyID, YPM.SupplierID, CONVERT(varchar, YPM.PODate, 101) PODate, 
                        --Contacts.Name AS SupplierName, YPM.QuotationRefNo, YPF.POFor, CONVERT(varchar, YPM.DeliveryStartDate, 101) DeliveryStartDate, 
                        --CONVERT(varchar, YPM.DeliveryEndDate, 101) DeliveryEndDate, CE.ShortName AS CompanyName, SUM(YPC.POQty) POQty, 
                        --SUM(YPC.PIValue) AS PIValue,YPM.Status
                        --FROM YPM 
                        --INNER JOIN YarnPOChild YPC ON YPC.YPOMasterID = YPM.YPOMasterID
                        --LEFT JOIN YarnPIFor YPF ON YPF.POForID = YPM.POForID
                        --Inner Join {DbNames.EPYSL}..Contacts ON YPM.SupplierID = Contacts.ContactID
                        --INNER JOIN {DbNames.EPYSL}..ContactCategoryChild ON Contacts.ContactID = ContactCategoryChild.ContactID 
                        --INNER JOIN {DbNames.EPYSL}..ContactCategoryHK ON ContactCategoryChild.ContactCategoryID = ContactCategoryHK.ContactCategoryID 
                        --INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPM.CompanyID
                        --GROUP BY YPM.YPOMasterID, YPM.PONo, YPM.CompanyID, YPM.SupplierID, PODate, Contacts.Name, YPM.QuotationRefNo, 
                        --YPF.POFor, YPM.DeliveryStartDate, YPM.DeliveryEndDate, CE.ShortName,YPM.Status
                        SELECT YPM.YPOMasterID, YPM.PONo, YPM.CompanyID, YPM.SupplierID, CONVERT(varchar, YPM.PODate, 101) PODate, 
	                    Contacts.Name AS SupplierName, YPM.QuotationRefNo, YPF.POFor, CONVERT(varchar, YPM.DeliveryStartDate, 101) DeliveryStartDate, 
	                    CONVERT(varchar, YPM.DeliveryEndDate, 101) DeliveryEndDate, CE.ShortName AS CompanyName, 
	                    YPC.POQty,
                        YPC.YPOChildID,
                        SUM(ISNULL(YPR.PIQty,0)) PIQty,
	                    SUM(YPC.PIValue) AS PIValue,YPM.Status
	                    FROM YarnPOChild YPC 
	                    INNER JOIN YPM ON YPM.YPOMasterID = YPC.YPOMasterID
                        Left Join YarnPIReceivePO YPR ON YPR.YPOMasterID=YPC.YPOMasterID AND YPR.ItemMasterID=YPC.ItemMasterID
						--Left Join YarnPIReceiveChild YPRC ON YPRC.ItemMasterID=YPC.ItemMasterID and YPRC.YPIReceiveMasterID=YPR.YPIReceiveMasterID
	                    LEFT JOIN YarnPIFor YPF ON YPF.POForID = YPM.POForID
	                    Inner Join {DbNames.EPYSL}..Contacts ON Contacts.ContactID = YPM.SupplierID
	                    INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPM.CompanyID
	                    GROUP BY YPM.YPOMasterID, YPM.PONo, YPM.CompanyID, YPM.SupplierID, PODate, Contacts.Name, YPM.QuotationRefNo, 
	                    YPF.POFor,  YPC.POQty, YPC.YPOChildID, YPM.DeliveryStartDate, YPM.DeliveryEndDate, CE.ShortName,YPM.Status
                    ),
                    FinalList AS
					(
						SELECT YPOMasterID, PONo, CompanyID, SupplierID, PODate, 
	                    SupplierName, QuotationRefNo, POFor, DeliveryStartDate, Status,
	                    DeliveryEndDate, CompanyName, POQty = SUM(POQty), PIQty = SUM(PIQty), PIValue = SUM(PIValue)
						FROM YPI
						GROUP BY YPOMasterID, PONo, CompanyID, SupplierID, PODate, 
	                    SupplierName, QuotationRefNo, POFor, DeliveryStartDate, 
	                    DeliveryEndDate, CompanyName, Status
                        Having SUM(YPI.POQty)>SUM(YPI.PIQty)
					)
                    SELECT YPOMasterID, PONo, CompanyID, SupplierID, CompanyName, PODate, SupplierName, QuotationRefNo, POFor, DeliveryStartDate, 
                    DeliveryEndDate, POQty, PIQty,PIValue,
                    Status=Case When POQty>PIQty then 'New' else 'Revise' end,
                    Count(*) Over() TotalRows FROM FinalList";

                orderBy = "ORDER BY YPOMasterID DESC";
            }
            else if (status == Status.RejectReview)
            {
                //sql = $@"
                //    ;With 
                //    YPI AS(
                //     SELECT M.Acknowledge, M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, CC.Name AS SupplierName, 
                //        PIFilePath, AttachmentPreviewTemplate, M.CompanyID, M.SupplierID, LU.Name POCreatedBy, SUM(C.POQty) AS POQty, 
                //        SUM(C.PIQty) PIQty, SUM(C.PIValue) AS PIValue
                //     FROM YarnPIReceiveMaster M
                //     INNER JOIN YarnPIReceiveChild C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
                //     INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                //     INNER JOIN YarnPIReceivePO YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
                //     INNER JOIN YarnPOMaster YPO ON YPIPO.YPOMasterID = YPO.YPOMasterID
                //     INNER JOIN {DbNames.{DbNames.EPYSL}}..LoginUser LU ON YPO.AddedBy = LU.UserCode
                //        WHERE M.IsCDA = 0 And Acknowledge=0 And UnAcknowledge=1
                //     GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, CC.Name, PIFilePath, AttachmentPreviewTemplate, M.CompanyID, 
                //        M.SupplierID, LU.Name, M.Acknowledge
                //    )		

                //    SELECT  YPI.Acknowledge,YPIReceiveMasterID, YPINo, PIDate, SupplierName, PIFilePath, AttachmentPreviewTemplate, POQty, PIQty, 
                //    PIValue, CompanyID, SupplierID, POCreatedBy, Count(*) Over() TotalRows
                //    FROM YPI";

                //orderBy = "ORDER BY YPIReceiveMasterID DESC";

                sql = $@"
                With TmpYR As 
                (
	                SELECT M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, M.CompanyID, CE.ShortName As CompanyName, 
	                M.SupplierID, M.PONo, CC.Name AS SupplierName, M.PIFilePath, M.AttachmentPreviewTemplate,   
	                C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue   
	                FROM YarnPIReceiveMaster M
	                INNER JOIN YarnPIReceiveChild C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                INNER JOIN YarnPIReceivePO YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                INNER JOIN YarnPOMaster YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID  
	                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID  
	                WHERE ISNULL(M.NeedsReview,0) = 0 AND ISNULL(Reject,0) = 1 AND IsCDA = 'False'
	                GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, M.CompanyID, CE.ShortName, M.SupplierID, M.PONo, CC.Name,
	                M.PIFilePath, M.AttachmentPreviewTemplate, C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue 
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
                Select *, Count(*) Over() TotalRows From TmpYRC ";

                orderBy = "ORDER BY YPIReceiveMasterID DESC";
            }
            else if (status == Status.Reject)
            {

                sql = $@"
                With TmpYR As 
                (
	                SELECT M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, M.CompanyID, CE.ShortName As CompanyName, 
	                M.SupplierID, M.PONo, CC.Name AS SupplierName, M.PIFilePath, M.AttachmentPreviewTemplate,   
	                C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue   
	                FROM YarnPIReceiveMaster M
	                INNER JOIN YarnPIReceiveChild C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                INNER JOIN YarnPIReceivePO YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                INNER JOIN YarnPOMaster YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID  
	                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID  
	                WHERE M.IsCDA = 0 And M.Acknowledge = 0 And M.UnAcknowledge = 1
	                GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, M.CompanyID, CE.ShortName, M.SupplierID, M.PONo, CC.Name,
	                M.PIFilePath, M.AttachmentPreviewTemplate, C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue 
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
                Select *, Count(*) Over() TotalRows From TmpYRC ";

                orderBy = "ORDER BY YPIReceiveMasterID DESC";
            }
            else if (status == Status.AwaitingPropose)
            {

                sql = $@"
                With TmpYR As 
                (
	                SELECT M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, M.CompanyID, CE.ShortName As CompanyName, 
	                M.SupplierID, M.PONo, CC.Name AS SupplierName, M.PIFilePath, M.AttachmentPreviewTemplate,   
	                C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue   
	                FROM YarnPIReceiveMaster M
	                INNER JOIN YarnPIReceiveChild C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                INNER JOIN YarnPIReceivePO YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                INNER JOIN YarnPOMaster YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID  
	                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID  
	                WHERE M.IsCDA = 0 And M.Acknowledge = 0 And M.UnAcknowledge = 0
	                GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, M.CompanyID, CE.ShortName, M.SupplierID, M.PONo, CC.Name,
	                M.PIFilePath, M.AttachmentPreviewTemplate, C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue 
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
            else if (status == Status.Acknowledge)
            {

                sql = $@"
                With RVSList AS
                (
                SELECT YPI.YPIReceivePOID, YPIReceiveMasterID
                FROM YarnPIReceivePO YPI
                INNER JOIN YarnPOMaster YPO ON YPO.YPOMasterID = YPI.YPOMasterID
                WHERE YPI.RevisionNo <> YPO.RevisionNo
                ),
                TmpYR As 
                (
	                SELECT M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, M.CompanyID, CE.ShortName As CompanyName, 
	                M.SupplierID, M.PONo, CC.Name AS SupplierName, M.PIFilePath, M.AttachmentPreviewTemplate,   
	                C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue   
	                FROM YarnPIReceiveMaster M
	                INNER JOIN YarnPIReceiveChild C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                INNER JOIN YarnPIReceivePO YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                INNER JOIN YarnPOMaster YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID  
	                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID
                    LEFT JOIN RVSList RV ON RV.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                WHERE M.IsCDA = 0 And Acknowledge = 1 And UnAcknowledge = 0
                    AND YPO.RevisionNo = YPIPO.RevisionNo AND RV.YPIReceivePOID IS NULL
	                GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, M.CompanyID, CE.ShortName, M.SupplierID, M.PONo, CC.Name,
	                M.PIFilePath, M.AttachmentPreviewTemplate, C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue 
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
            else if (status == Status.Revise)
            {
                sql = $@"
                With TmpYR As 
                (
	                SELECT M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, M.CompanyID, CE.ShortName As CompanyName, 
	                M.SupplierID, M.PONo, CC.Name AS SupplierName, M.PIFilePath, M.AttachmentPreviewTemplate,   
	                C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue   
	                FROM YarnPIReceiveMaster M
	                INNER JOIN YarnPIReceiveChild C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                INNER JOIN YarnPIReceivePO YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                INNER JOIN YarnPOMaster YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID  
	                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID  
	                WHERE M.IsCDA = 0 AND M.UnAcknowledge = 0 --And M.YPINo = 'az-200422'
                    --AND M.Accept=0
                    AND YPO.RevisionNo <> YPIPO.RevisionNo
	                GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, M.CompanyID, CE.ShortName, M.SupplierID, M.PONo, CC.Name,
	                M.PIFilePath, M.AttachmentPreviewTemplate, C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue,YPIPO.RevisionNo,YPO.RevisionNo 
                ),
                PoIdUniqueList AS
                (
	                SELECT DISTINCT POM.YPOMasterID, TmpYR.YPIReceiveMasterID
	                FROM TmpYR
	                INNER JOIN YarnPIReceivePO RPO ON RPO.YPIReceiveMasterID = TmpYR.YPIReceiveMasterID
	                INNER JOIN YarnPOMaster POM ON POM.YPOMasterID = RPO.YPOMasterID 
                ),
                PoIdList AS
                (
	                SELECT POIds = STRING_AGG(A.YPOMasterID, ','), A.YPIReceiveMasterID
	                FROM PoIdUniqueList A
	                GROUP BY A.YPIReceiveMasterID
                ),
                TmpYRC As 
                (
	                Select TmpYR.YPIReceiveMasterID, YPINo, PIDate, CompanyID, CompanyName, SupplierID, SupplierName, PONo, PIFilePath, 
	                AttachmentPreviewTemplate,PO.POIds,  
	                SUM(POQty)POQty, SUM(Rate)Rate, SUM(PIQty)PIQty, SUM(PIValue)PIValue 
	                From TmpYR
                    LEFT JOIN PoIdList PO ON PO.YPIReceiveMasterID = TmpYR.YPIReceiveMasterID
	                Group By TmpYR.YPIReceiveMasterID, YPINo, PIDate, CompanyID, CompanyName, SupplierID, SupplierName, PONo, PIFilePath, 
	                AttachmentPreviewTemplate,PO.POIds
                )
                Select *, Count(*) Over() TotalRows From TmpYRC";

                orderBy = "ORDER BY YPIReceiveMasterID DESC";
            }
            else
            {
                sql = $@"
                ;With BBLC AS (
					Select ProposalID 
					From YarnLCMaster
					Where LCNo = '{LcNo}'
				), LCPI AS(
					Select YPI.YPIReceiveMasterID
					From BBLC
					Inner Join YarnBBLCProposalChild YPI ON YPI.ProposalID = BBLC.ProposalID
				),
                TmpYR As 
                (
	                SELECT M.YPIReceiveMasterID, M.YPINo, CONVERT(varchar, M.PIDate, 101) PIDate, M.CompanyID, CE.ShortName As CompanyName, 
	                M.SupplierID, M.PONo, CC.Name AS SupplierName, M.PIFilePath, M.AttachmentPreviewTemplate,   
	                C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue   
                    FROM LCPI
					Inner Join YarnPIReceiveMaster M On M.YPIReceiveMasterID = LCPI.YPIReceiveMasterID
	                INNER JOIN YarnPIReceiveChild C ON C.YPIReceiveMasterID = M.YPIReceiveMasterID
	                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                INNER JOIN YarnPIReceivePO YPIPO ON M.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                INNER JOIN YarnPOMaster YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID  
	                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID
	                WHERE M.IsCDA = 0 AND M.UnAcknowledge = 0
                    AND YPO.RevisionNo = YPIPO.RevisionNo 
	                
                    GROUP BY M.YPIReceiveMasterID, M.YPINo, M.PIDate, M.CompanyID, CE.ShortName, M.SupplierID, M.PONo, CC.Name,
	                M.PIFilePath, M.AttachmentPreviewTemplate, C.YPIReceiveChildID, C.POQty, C.Rate, C.PIQty, C.PIValue
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

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnPIReceiveMaster>(sql);
        }

        public async Task<List<YarnPIReceivePO>> GetReceivePOByIDAsync(int id)
        {
            string sql = $@"
            ;Select * From YarnPIReceivePO Where YPOMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryAsync<YarnPIReceivePO>(sql);
                List<YarnPIReceivePO> data = records.ToList();
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

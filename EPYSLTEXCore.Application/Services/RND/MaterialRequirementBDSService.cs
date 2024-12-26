using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Knitting;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using static Dapper.SqlMapper;

namespace EPYSLTEXCore.Application.Services.RND
{
    public class MaterialRequirementBDSService : IMaterialRequirementBDSService
    {
        private readonly IDapperCRUDService<FreeConceptMRMaster> _service;
        //private readonly ISignatureRepository _signatureRepository;
        private readonly IConceptStatusService _conceptStatusService;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        private SqlTransaction transaction;
        private SqlTransaction transactionGmt;

        public MaterialRequirementBDSService(IDapperCRUDService<FreeConceptMRMaster> service
            //, ISignatureRepository signatureRepository
            , IConceptStatusService conceptStatusService)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;
            //_signatureRepository = signatureRepository;
            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
            _conceptStatusService = conceptStatusService;
        }

        public async Task<List<FreeConceptMRMaster>> GetPagedAsync(Status status, int isBDS, PaginationInfo paginationInfo, LoginUser AppUser)
        {
            string orderBy, groupBy, sql;

            string sqlBuyerPermission = AppUser.IsSuperUser ? "" : $@" BT As (
                        Select CategoryTeamID
                        From {DbNames.EPYSL}..EmployeeAssignContactTeam
                        Where EmployeeCode = {AppUser.EmployeeCode} AND IsActive = 1
                        Group By CategoryTeamID
                    ),
                    B As (
	                    Select C.ContactID
	                    From {DbNames.EPYSL}..ContactAssignTeam C
	                    Inner Join BT On BT.CategoryTeamID = C.CategoryTeamID
	                    Group By C.ContactID
                    ), ";

            string sqlBuyerPerInnerJoin = AppUser.IsSuperUser ? "" : $@" INNER JOIN B ON B.ContactID = FB.BuyerID ";

            if (status == Status.Pending)
            {
                if (isBDS == 3)
                {
                    sql = $@"
                     WITH FBA AS(
		                SELECT DISTINCT FBA.* 
                        FROM {TableNames.FBBOOKING_ACKNOWLEDGE} FBA
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.BookingID = FBA.BookingID
                        WHERE FBA.IsUnAcknowledge = 0 AND FCM.IsBDS = 3
	                ),
                    {sqlBuyerPermission}
                    F As (
	                    Select 'New' [Status], FB.FBAckID, FB.BookingID, FB.BookingNo, FB.BookingDate, FB.BuyerID, C.ShortName BuyerName, FB.BuyerTeamID,
		                CCT.TeamName As BuyerDepartment, FB.ExecutionCompanyID, CE.CompanyName, FB.SupplierID, FB.BookingQty
	                    From FBA FB
	                    Inner join {DbNames.EPYSL}..Contacts C On C.ContactID = FB.BuyerID
                        {sqlBuyerPerInnerJoin}
	                    Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = FB.BuyerTeamID
	                    Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = FB.ExecutionCompanyID
                        Where FB.BookingID Not in(Select BookingID FROM {TableNames.RND_FREE_CONCEPT_MASTER} WHERE ConceptID IN (SELECT ConceptID FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER}))
                        Group By FB.FBAckID, FB.BookingID, FB.BookingNo, FB.BookingDate, FB.BuyerID, C.ShortName, FB.BuyerTeamID,
		                CCT.TeamName, FB.ExecutionCompanyID, CE.CompanyName, FB.SupplierID, FB.BookingQty
                    )
	                SELECT *, Count(*) Over() TotalRows
	                FROM F";
                }
                else
                {
                    sql = $@"
                    WITH FBA AS(
					    SELECT FBA.FBAckID, FBA.BookingID, FBA.BookingNo, FBA.BookingDate, FBA.BuyerID, FBA.BuyerTeamID, FBA.ExecutionCompanyID,
	                    FBA.SupplierID, FBA.BookingQty, AcknowledgeDate = MIN(FBA.DateAdded)
	                    FROM {TableNames.FBBOOKING_ACKNOWLEDGE} FBA
	                    WHERE FBA.IsUnAcknowledge = 0
	                    GROUP BY FBA.FBAckID, FBA.BookingID, FBA.BookingNo, FBA.BookingDate, FBA.BuyerID, FBA.BuyerTeamID, FBA.ExecutionCompanyID,
	                    FBA.SupplierID, FBA.BookingQty
				    ),
                    {sqlBuyerPermission}
                    F As (
	                    Select 'New' [Status], FB.FBAckID, FB.BookingID, FB.BookingNo, FB.BookingDate, FB.BuyerID, C.ShortName BuyerName, FB.BuyerTeamID,
	                    CCT.TeamName As BuyerDepartment, FB.ExecutionCompanyID, CE.CompanyName, FB.SupplierID, FB.BookingQty, FB.AcknowledgeDate
	                    From FBA FB
                        INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.AcknowledgeID = FB.FBAckID
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.BookingID = FB.BookingID
								                        AND FCM.BookingChildID = FBC.BookingChildID
									                    AND FCM.IsBDS = 1
	                    Inner join {DbNames.EPYSL}..Contacts C On C.ContactID = FB.BuyerID
                        {sqlBuyerPerInnerJoin}
	                    Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = FB.BuyerTeamID
	                    Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = FB.ExecutionCompanyID
                        Where FB.BookingID Not in(Select BookingID FROM {TableNames.RND_FREE_CONCEPT_MASTER} WHERE ConceptID IN (SELECT ConceptID FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER}))
                        AND FCM.IsBDS = {isBDS}
                        Group By FB.FBAckID, FB.BookingID, FB.BookingNo, FB.BookingDate, FB.BuyerID, C.ShortName, FB.BuyerTeamID,
                        CCT.TeamName, FB.ExecutionCompanyID, CE.CompanyName, FB.SupplierID, FB.BookingQty, FB.AcknowledgeDate
                    )
				    SELECT *, Count(*) Over() TotalRows
				    FROM F";
                }

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? " Order By FBAckID Desc" : paginationInfo.OrderBy;
                groupBy = "";
            }
            else if (status == Status.PartiallyCompleted)
            {
                sql = $@"
                With 
                {sqlBuyerPermission}
                FCM As
                (
                    Select FB.BookingNo, FB.BookingDate, FB.BuyerID, C.ShortName BuyerName, FB.BuyerTeamID, CCT.TeamName As BuyerDepartment,
                    FB.ExecutionCompanyID, CE.CompanyName, FC.GroupConceptNo ConceptNo,AddDate=MIN(FCM.DateAdded)
                    FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM
                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = FCM.ConceptID AND FC.IsBDS = {isBDS}
                    Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FB On FB.BookingID = FC.BookingID
                    Inner join {DbNames.EPYSL}..Contacts C On C.ContactID = FB.BuyerID
                    {sqlBuyerPerInnerJoin}
	                Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = FB.BuyerTeamID
	                Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = FB.ExecutionCompanyID
	                Where FCM.IsComplete=0 AND FC.RevisionNo = FCM.RevisionNo
	                AND FCM.PreProcessRevNo = FB.RevisionNo
                    GROUP BY FB.BookingNo, FB.BookingDate, FB.BuyerID, C.ShortName, FB.BuyerTeamID, CCT.TeamName,
	                FB.ExecutionCompanyID, CE.CompanyName, FC.GroupConceptNo
                )
                Select *, Count(*) Over() TotalRows from FCM";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By AddDate Desc" : paginationInfo.OrderBy;
                groupBy = "";
            }
            else if (status == Status.Revise)
            {
                sql = $@"
                With 
                {sqlBuyerPermission}
                FCM As
                ( 
                    Select FB.FBAckID, FB.BookingNo, FB.BookingDate, FB.BuyerID, C.ShortName BuyerName, FB.BuyerTeamID, CCT.TeamName As BuyerDepartment,
                    FB.ExecutionCompanyID, CE.CompanyName, FC.GroupConceptNo ConceptNo,AddDate=MIN(FCM.DateAdded)
                    FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM
                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = FCM.ConceptID AND FC.IsBDS = {isBDS}
                    Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FB On FB.BookingID = FC.BookingID
                    Inner join {DbNames.EPYSL}..Contacts C On C.ContactID = FB.BuyerID
                    {sqlBuyerPerInnerJoin}
                    Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = FB.BuyerTeamID
                    Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = FB.ExecutionCompanyID
                    Where FB.RevisionNo <> FCM.PreProcessRevNo
                    GROUP BY FB.FBAckID, FB.BookingNo, FB.BookingDate, FB.BuyerID, C.ShortName, FB.BuyerTeamID, CCT.TeamName,
                    FB.ExecutionCompanyID, CE.CompanyName, FC.GroupConceptNo
                )
                Select *, Count(*) Over() TotalRows from FCM";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By AddDate Desc" : paginationInfo.OrderBy;
                groupBy = "";
            }
            else if (status == Status.Acknowledge)
            {
                sql = $@"
                With 
                {sqlBuyerPermission}
                FCM As
                (
	                Select FB.BookingNo, FB.BookingDate, FB.BuyerID, C.ShortName BuyerName, FB.BuyerTeamID, CCT.TeamName As BuyerDepartment,
					FB.ExecutionCompanyID, CE.CompanyName, FC.GroupConceptNo ConceptNo,AddDate=CONVERT(varchar,FCM.DateAdded,103)
	                FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM
					INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = FCM.ConceptID
	                Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FB On FB.BookingID = FC.BookingID
	                Inner join {DbNames.EPYSL}..Contacts C On C.ContactID = FB.BuyerID
                    
	                Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = FB.BuyerTeamID
	                Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = FB.ExecutionCompanyID
	                Where FC.IsBDS = 3 AND FCM.IsComplete=1 AND ISNULL(FCM.isAcknowledge,0) = 1
                    AND FCM.PreProcessRevNo = FB.RevisionNo
					GROUP BY FB.BookingNo, FB.BookingDate, FB.BuyerID, C.ShortName, FB.BuyerTeamID, CCT.TeamName,
					FB.ExecutionCompanyID, CE.CompanyName, FC.GroupConceptNo,CONVERT(varchar,FCM.DateAdded,103)
                )
                Select *, Count(*) Over() TotalRows from FCM";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By AddDate Desc" : paginationInfo.OrderBy;
                groupBy = "";
            }
            else if (status == Status.ProposedForAcknowledge)
            {
                sql = $@"
                With 
                {sqlBuyerPermission}
                FCM As
                (
	                Select FB.BookingNo, FB.BookingDate, FB.BuyerID, C.ShortName BuyerName, FB.BuyerTeamID, CCT.TeamName As BuyerDepartment,
					FB.ExecutionCompanyID, CE.CompanyName, FC.GroupConceptNo ConceptNo,AddDate=CONVERT(varchar,FCM.DateAdded,103)
	                FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM
					INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = FCM.ConceptID
	                Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FB On FB.BookingID = FC.BookingID
	                Inner join {DbNames.EPYSL}..Contacts C On C.ContactID = FB.BuyerID
                    
	                Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = FB.BuyerTeamID
	                Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = FB.ExecutionCompanyID
	                Where FC.IsBDS = 3 AND FCM.IsComplete=1 AND ISNULL(FCM.isAcknowledge,0) = 0
                    AND FCM.PreProcessRevNo = FB.RevisionNo
					GROUP BY FB.BookingNo, FB.BookingDate, FB.BuyerID, C.ShortName, FB.BuyerTeamID, CCT.TeamName,
					FB.ExecutionCompanyID, CE.CompanyName, FC.GroupConceptNo,CONVERT(varchar,FCM.DateAdded,103)
                )
                Select *, Count(*) Over() TotalRows from FCM";

                //orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By ConceptNo Desc" : paginationInfo.OrderBy;
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By AddDate Desc" : paginationInfo.OrderBy;
                groupBy = "";
            }
            else
            {
                sql = $@"
                With 
                {sqlBuyerPermission}
                FCM As
                (
	                Select FB.BookingNo, FB.BookingDate, FB.BuyerID, C.ShortName BuyerName, FB.BuyerTeamID, CCT.TeamName As BuyerDepartment,
	                FB.ExecutionCompanyID, CE.CompanyName, FC.GroupConceptNo ConceptNo,AddDate=MIN(FCM.DateAdded)
	                FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = FCM.ConceptID AND FC.IsBDS = {isBDS}
	                Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FB On FB.BookingID = FC.BookingID
	                Inner join {DbNames.EPYSL}..Contacts C On C.ContactID = FB.BuyerID
                    {sqlBuyerPerInnerJoin}
                    Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = FB.BuyerTeamID
                    Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = FB.ExecutionCompanyID
                    Where FCM.IsComplete=1 
                    AND FCM.PreProcessRevNo = FB.RevisionNo
                    GROUP BY FB.BookingNo, FB.BookingDate, FB.BuyerID, C.ShortName, FB.BuyerTeamID, CCT.TeamName,
                    FB.ExecutionCompanyID, CE.CompanyName, FC.GroupConceptNo
                )
                Select *, Count(*) Over() TotalRows from FCM";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By AddDate Desc" : paginationInfo.OrderBy;
                groupBy = "";
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {groupBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<FreeConceptMRMaster>(sql, _connection);
        }

        public async Task<FreeConceptMRMaster> GetNewAsync(int FBAckID, string menuParam = null)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.YARN_CERTIFICATIONS,
                    ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW
                }
            };

            var query = $@"
                With FB As
                (
	                Select * FROM {TableNames.FBBOOKING_ACKNOWLEDGE} WHERE FBAckID = {FBAckID}
                )
                SELECT FC.ConceptID As FCMRMasterID, FB.FBAckID, FB.BookingID, FB.BookingNo, FB.BookingDate, FB.BuyerID,FB.BuyerTeamID,
                FB.ExecutionCompanyID, FB.SupplierID,
                FC.TotalQty Qty, FC.ExcessPercentage, FC.ExcessQty, FC.ExcessQtyInKG, FC.TotalQty, FC.TotalQtyInKG,
                FC.ItemMasterID FabricID, Construction.SegmentValue Construction,
                Composition.SegmentValue Composition, DT.SegmentValue DyeingType,
                KT.SegmentValue KnittingType, FC.SubGroupID, ISG.SubGroupName ItemSubGroup, FTN.TechnicalName, FC.ConceptID, FC.ConceptNo, FC.ConceptDate

                ,KMS.SubClassName MachineType, SBC.Segment1Desc Construction,SBC.Segment2Desc Composition,
				
				Color = CASE WHEN FC.SubGroupID=1 THEN Color.SegmentValue ELSE SV5.SegmentValue END, 
				Gsm = CASE WHEN FC.SubGroupID=1 THEN Gsm.SegmentValue ELSE '' END,

				Length= CASE WHEN FC.SubGroupID<>1 THEN CONVERT(decimal(18,2),Color.SegmentValue) ELSE 0 END, 
				Width= CASE WHEN FC.SubGroupID<>1 THEN CONVERT(decimal(18,2),Gsm.SegmentValue) ELSE 0 END,

                TotalQtyInKG = Round(CASE WHEN FC.SubGroupID<>1 THEN CONVERT(decimal(18,2),Color.SegmentValue)*CONVERT(decimal(18,2),Gsm.SegmentValue)*0.045*FC.TotalQty/420 ELSE 0 END,2),
				
				SBC.Segment5Desc FabricWidth, SBC.Segment7Desc KnittingType,
				--,ISV.SegmentValue YarnType, ETV.ValueName YarnProgram, 
				C.ShortName BuyerName

                , (SELECT STUFF((
				SELECT DISTINCT ', '+ ETV1.ValueName
				FROM {DbNames.EPYSL}..SampleBookingConsumptionYarnSubBrand SBCYSB
				LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV1 ON ETV1.ValueID = SBCYSB.YarnSubBrandID
				WHERE SBCYSB.BookingID = FC.BookingID AND SBCYSB.ConsumptionID = FC.ConsumptionID
				FOR XML PATH('')),1,1,'')) YarnSubProgram

				,SBC.Remarks Instruction,SBC.ForBDSStyleNo
				,ReferenceSource = SBC.ReferenceSourceID,ReferenceNo=SBC.ReferenceNo, ColorReferenceNo=SBC.ColorReferenceNo
                ,FBC.DayValidDurationId
                ,FB.BookingDate

                FROM FB
                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.AcknowledgeID = FB.FBAckID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.BookingID = FB.BookingID AND FC.BookingChildID = FBC.BookingChildID
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue SV5 ON SV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue DT ON DT.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue KT ON KT.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FC.SubGroupID
                LEFT JOIN {TableNames.FabricTechnicalName} FTN ON FC.TechnicalNameId = FTN.TechnicalNameId
                LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} KMS ON KMS.SubClassID = FC.MCSubClassID
				LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumption SBC ON SBC.BookingID = FC.BookingID AND SBC.ConsumptionID = FC.ConsumptionID
                LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumptionChild SBCC ON SBCC.BookingID = FC.BookingID AND SBCC.ConsumptionID = FC.ConsumptionID AND SBCC.ItemMasterID = FC.ItemMasterID
				--LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = FC.A1ValueID
				--LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = FC.YarnBrandID
                LEFT JOIN {DbNames.EPYSL}..Contacts C On C.ContactID = FB.BuyerID
				Where SBCC.ConsumptionID is Not Null;

                -- Item Segments
                  {CommonQueries.GetCertifications()};

                -- Fabric Components
                  {CommonQueries.GetFabricComponents(EntityTypeNameConstants.FABRIC_TYPE)};

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()}

                --Item Segments
                {CommonQueries.GetSubPrograms()}; 

                -- DayValidDuration
                {CommonQueries.GetDayValidDurations()};

                --Fiber-SubProgram-Certifications Mapping Setup
                Select * FROM {TableNames.FIBER_SUBPROGRAM_CERTIFICATIONS_FILTER_SETUP}


                --Color Wise Size Collar
                    SELECT distinct id=CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END+' X '+CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END,
                          text=CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END+' X '+CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END
                          FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} BAC
                          INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FCM ON FCM.BookingID = BAC.BookingID
                          INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                          WHERE FCM.FBAckID = {FBAckID} AND BAC.SubGroupID IN (11); 

                --Color Wise Size Cuff
                    SELECT distinct id=CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END+' X '+CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END,
                          text=CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END+' X '+CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END
                          FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} BAC
                          INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FCM ON FCM.BookingID = BAC.BookingID
                          INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                          WHERE FCM.FBAckID = {FBAckID} AND BAC.SubGroupID IN (12); 

                --Color Wise All Size Collar
                    SELECT BAC.*,ConstructionId=ISV1.SegmentValueID, Construction = ISV1.SegmentValue, 
                          CompositionId = ISV2.SegmentValueID,Composition = ISV2.SegmentValue,
                          ColorID = CASE WHEN BAC.SubGroupID = 1 THEN ISV3.SegmentValueID ELSE ISV5.SegmentValueID END, 
                          Color = CASE WHEN BAC.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                          Length = CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END,
                          Width = CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END
                          FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} BAC
                          INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FCM ON FCM.BookingID = BAC.BookingID
                          INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                          WHERE FCM.FBAckID = {FBAckID} AND BAC.SubGroupID IN (11);; 

                --Color Wise All Size Cuff
                    SELECT BAC.*,ConstructionId=ISV1.SegmentValueID, Construction = ISV1.SegmentValue, 
                          CompositionId = ISV2.SegmentValueID,Composition = ISV2.SegmentValue,
                          ColorID = CASE WHEN BAC.SubGroupID = 1 THEN ISV3.SegmentValueID ELSE ISV5.SegmentValueID END, 
                          Color = CASE WHEN BAC.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                          Length = CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END,
                          Width = CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END
                          FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} BAC
                          INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FCM ON FCM.BookingID = BAC.BookingID
                          INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                          WHERE FCM.FBAckID = {FBAckID} AND BAC.SubGroupID IN (12);";

            try
            {//--
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query, segmentNames);
                FreeConceptMRMaster data = new FreeConceptMRMaster();
                data.OtherItems = records.Read<FreeConceptMRMaster>().ToList();
                data.BuyerName = data.OtherItems.Count() > 0 ? data.OtherItems[0].BuyerName : "";
                foreach (FreeConceptMRMaster item in data.OtherItems)
                {
                    if (item.ItemSubGroup == "Fabric") data.HasFabric = true;
                    else if (item.ItemSubGroup == "Collar") data.HasCollar = true;
                    else if (item.ItemSubGroup == "Cuff") data.HasCuff = true;
                }
                var _recvCertifications = await records.ReadAsync<Select2OptionModelExtended>();
                data.Certifications = _recvCertifications.Where(x => x.desc == ItemSegmentNameConstants.YARN_CERTIFICATIONS);
                data.FabricComponentsNew = await records.ReadAsync<Select2OptionModel>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();
                var itemSegments = await records.ReadAsync<Select2OptionModelExtended>();
                data.YarnSubProgramNews = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW);

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.OtherItems.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.IsCheckDVD = true;
                data.FabricComponentMappingSetupList = records.Read<FabricComponentMappingSetup>().ToList();

                data.OtherItems.Where(x => x.DayValidDurationId > 0).ToList().ForEach(x =>
                {
                    var dayObj = data.DayValidDurations.ToList().Find(d => d.id == x.DayValidDurationId.ToString());
                    if (dayObj.IsNotNull())
                    {
                        x.DayValidDurationName = dayObj.text;
                        x.DayDuration = Convert.ToInt32(dayObj.additionalValue);
                    }
                });
                data.CollarSizeList = records.Read<Select2OptionModel>().ToList();
                data.CuffSizeList = records.Read<Select2OptionModel>().ToList();
                data.AllCollarSizeList = records.Read<FBookingAcknowledgeChild>().ToList();
                data.AllCuffSizeList = records.Read<FBookingAcknowledgeChild>().ToList();
                if (menuParam == "MRPB")
                {
                    #region Grouping Collar Cuff
                    //Collar
                    List<FreeConceptMRMaster> tempChildsCollar = CommonFunction.DeepClone(data.OtherItems.Where(x => x.SubGroupID == 11).ToList());
                    var disList = tempChildsCollar.Select(m => new { m.Construction, m.Composition, m.Color, m.DayValidDurationId })
                            .Distinct()
                            .ToList();

                    List<FreeConceptMRMaster> tempChilds = new List<FreeConceptMRMaster>();
                    int conceptID = 1;
                    int fcmrMasterID = 1;
                    disList.ForEach(c =>
                    {
                        var tempChilds1 = tempChildsCollar.Where(x => x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color && x.DayValidDurationId == c.DayValidDurationId).ToList();
                        var firstChild = tempChilds1.First();

                        tempChilds.Add(new FreeConceptMRMaster()
                        {
                            FCMRMasterID = fcmrMasterID++,
                            FBAckID = firstChild.FBAckID,
                            BookingID = firstChild.BookingID,
                            BookingNo = firstChild.BookingNo,
                            BookingDate = firstChild.BookingDate,
                            Qty = tempChilds1.Sum(x => x.Qty),
                            TotalQty = tempChilds1.Sum(x => x.TotalQty),
                            TotalQtyInKG = tempChilds1.Sum(x => x.TotalQtyInKG),
                            FabricID = firstChild.FabricID,
                            ConstructionID = firstChild.ConstructionID,
                            CompositionId = firstChild.CompositionId,
                            Construction = firstChild.Construction,
                            Composition = firstChild.Composition,
                            Color = firstChild.Color,
                            DyeingType = firstChild.DyeingType,
                            KnittingType = firstChild.KnittingType,
                            SubGroupID = firstChild.SubGroupID,
                            ItemSubGroup = firstChild.ItemSubGroup,
                            TechnicalName = firstChild.TechnicalName,
                            ConceptID = conceptID++,
                            ConceptNo = "",
                            MachineType = firstChild.MachineType,
                            GSM = firstChild.GSM,
                            Length = firstChild.Length,
                            Width = firstChild.Width,
                            FabricWidth = firstChild.FabricWidth,
                            BuyerName = firstChild.BuyerName,
                            YarnSubProgram = firstChild.YarnSubProgram,
                            Instruction = firstChild.Instruction,
                            ReferenceSource = firstChild.ReferenceSource,
                            ReferenceNo = firstChild.ReferenceNo,
                            ColorReferenceNo = firstChild.ColorReferenceNo,
                            DayValidDurationId = firstChild.DayValidDurationId,
                            DayValidDurationName = firstChild.DayValidDurationName,
                            DayDuration = firstChild.DayDuration,
                            HasFabric = firstChild.HasFabric,
                            HasCollar = firstChild.HasCollar,
                            HasCuff = firstChild.HasCuff
                        });
                    });

                    //Cuff

                    List<FreeConceptMRMaster> tempChildsCuff = CommonFunction.DeepClone(data.OtherItems.Where(x => x.SubGroupID == 12).ToList());
                    disList = tempChildsCuff.Select(m => new { m.Construction, m.Composition, m.Color, m.DayValidDurationId })
                            .Distinct()
                            .ToList();

                    disList.ForEach(c =>
                    {
                        var tempChilds1 = tempChildsCuff.Where(x => x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color && x.DayValidDurationId == c.DayValidDurationId).ToList();
                        var firstChild = tempChilds1.First();
                        tempChilds.Add(new FreeConceptMRMaster()
                        {
                            FCMRMasterID = fcmrMasterID++,
                            FBAckID = firstChild.FBAckID,
                            BookingID = firstChild.BookingID,
                            BookingNo = firstChild.BookingNo,
                            BookingDate = firstChild.BookingDate,
                            Qty = tempChilds1.Sum(x => x.Qty),
                            TotalQty = tempChilds1.Sum(x => x.TotalQty),
                            TotalQtyInKG = tempChilds1.Sum(x => x.TotalQtyInKG),
                            FabricID = firstChild.FabricID,
                            ConstructionID = firstChild.ConstructionID,
                            CompositionId = firstChild.CompositionId,
                            Construction = firstChild.Construction,
                            Composition = firstChild.Composition,
                            Color = firstChild.Color,
                            DyeingType = firstChild.DyeingType,
                            KnittingType = firstChild.KnittingType,
                            SubGroupID = firstChild.SubGroupID,
                            ItemSubGroup = firstChild.ItemSubGroup,
                            TechnicalName = firstChild.TechnicalName,
                            ConceptID = conceptID++,
                            ConceptNo = "",
                            MachineType = firstChild.MachineType,
                            GSM = firstChild.GSM,
                            Length = firstChild.Length,
                            Width = firstChild.Width,
                            FabricWidth = firstChild.FabricWidth,
                            BuyerName = firstChild.BuyerName,
                            YarnSubProgram = firstChild.YarnSubProgram,
                            Instruction = firstChild.Instruction,
                            ReferenceSource = firstChild.ReferenceSource,
                            ReferenceNo = firstChild.ReferenceNo,
                            ColorReferenceNo = firstChild.ColorReferenceNo,
                            DayValidDurationId = firstChild.DayValidDurationId,
                            DayValidDurationName = firstChild.DayValidDurationName,
                            DayDuration = firstChild.DayDuration,
                            HasFabric = firstChild.HasFabric,
                            HasCollar = firstChild.HasCollar,
                            HasCuff = firstChild.HasCuff
                        });
                    });

                    data.OtherItems = data.OtherItems.Where(x => x.SubGroupID == 1).ToList();
                    data.OtherItems.AddRange(tempChilds);
                    #endregion
                }

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task<FreeConceptMRMaster> GetDetailsAsync(int id)
        {
            var query =
                $@"-- Master Data
                Select * FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} Where FCMRMasterID = {id}

                Select * FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} Where FCMRMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                FreeConceptMRMaster data = await records.ReadFirstOrDefaultAsync<FreeConceptMRMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<FreeConceptMRChild>().ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task<List<FreeConceptMRMaster>> GetMultiDetailsAsync(string id, int bookingID)
        {
            string query =
                $@"-- Master Data
                Select FCM.* FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = FCM.ConceptID
                WHERE CM.GroupConceptNo  = '{id}';

                Select FCC.*,ISV1.SegmentValue Segment1ValueDesc,ISV2.SegmentValue Segment2ValueDesc,ISV3.SegmentValue Segment3ValueDesc,ISV4.SegmentValue Segment4ValueDesc
                ,ISV5.SegmentValue Segment5ValueDesc,ISV6.SegmentValue Segment6ValueDesc
                FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCC
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM ON FCM.FCMRMasterID = FCC.FCMRMasterID
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = FCM.ConceptID
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCC.ItemMasterId
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                WHERE CM.GroupConceptNo = '{id}';

                Select * From BDSDependentTNACalander
                WHERE bookingID = {bookingID};";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                List<FreeConceptMRMaster> datas = records.Read<FreeConceptMRMaster>().ToList();
                List<FreeConceptMRChild> childs = records.Read<FreeConceptMRChild>().ToList();
                List<BDSDependentTNACalander> BDSDependentTNACalanders = records.Read<BDSDependentTNACalander>().ToList();
                datas.ForEach(data =>
                {
                    data.OtherItems = datas.Where(x => x.FCMRMasterID == data.FCMRMasterID).ToList();
                    data.Childs = childs.Where(x => x.FCMRMasterID == data.FCMRMasterID).ToList();
                    data.BDSDependentTNACalanders = BDSDependentTNACalanders.ToList();
                });
                return datas;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<List<FreeConceptMRMaster>> GetDetailsAsyncForRevise(string id, int bookingID, bool isOwnRevise)
        {
            string query = "";
            if (!isOwnRevise) //From Revision List
            {
                query =
                $@"-- Master Data
                Select FCM.FCMRMasterID, FCM.ReqDate,FCM.ConceptID,FCM.TrialNo,FCM.RevisionNo,FCM.RevisionDate,FCM.RevisionBy,
				FCM.HasYD,FCM.Remarks,CM.IsBDS,FCM.FabricID,FCM.IsComplete,
				PreProcessRevNo = FB.RevisionNo, FCM.IsNeedRevision, FCM.ItemRevisionNo
				FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = FCM.ConceptID
				LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FCC ON FCC.BookingChildID=CM.BookingChildID AND FCC.ConsumptionID=CM.ConsumptionID AND FCC.BookingID=CM.BookingID
				LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FB ON FB.FBAckID=FCC.AcknowledgeID
                WHERE CM.GroupConceptNo  = '{id}';

                Select FCC.* FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCC
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM ON FCM.FCMRMasterID = FCC.FCMRMasterID
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = FCM.ConceptID
                WHERE CM.GroupConceptNo = '{id}';

                Select * From BDSDependentTNACalander
                WHERE bookingID = {bookingID}";
            }
            else //From Complete List
            {
                query =
                $@"-- Master Data
                Select FCM.FCMRMasterID, FCM.ReqDate,FCM.ConceptID,FCM.TrialNo,FCM.RevisionNo,FCM.RevisionDate,FCM.RevisionBy,
				FCM.HasYD,FCM.Remarks,FC.IsBDS,FCM.FabricID,FCM.IsComplete,FCM.PreProcessRevNo
				FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID=FCM.ConceptID
                WHERE FC.GroupConceptNo  = '{id}';

                Select FCC.* FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCC
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM ON FCM.FCMRMasterID = FCC.FCMRMasterID
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = FCM.ConceptID
                WHERE CM.GroupConceptNo = '{id}';

                Select * From BDSDependentTNACalander
                WHERE bookingID = {bookingID}";
            }
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                List<FreeConceptMRMaster> datas = records.Read<FreeConceptMRMaster>().ToList();
                List<FreeConceptMRChild> childs = records.Read<FreeConceptMRChild>().ToList();
                List<BDSDependentTNACalander> BDSDependentTNACalanders = records.Read<BDSDependentTNACalander>().ToList();
                datas.ForEach(data =>
                {
                    data.OtherItems = datas.Where(x => x.FCMRMasterID == data.FCMRMasterID).ToList();
                    data.Childs = childs.Where(x => x.FCMRMasterID == data.FCMRMasterID).ToList();
                    data.BDSDependentTNACalanders = BDSDependentTNACalanders.ToList();
                });
                return datas;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<FreeConceptMRMaster> GetAsync(string grpConceptNo, string menuParam = null)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.YARN_CERTIFICATIONS,
                    ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW
                }
            };

            var query =
                $@"
                With FCM As
                (
	                Select MR.FCMRMasterID, MR.ConceptID, MR.FabricID, MR.ReqDate, FCM.ConsumptionID, FCM.GroupConceptNo, FCM.ConceptDate, FCM.BookingID, FCM.BookingChildID, FCM.Qty 
                    ,FBC.DayValidDurationId
                    FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR
					INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MR.ConceptID
                    LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = FCM.BookingChildID AND FBC.BookingID = FCM.BookingID
					WHERE FCM.GroupConceptNo = '{grpConceptNo}'
                )
                Select FCM.FCMRMasterID, FB.FBAckID, FCM.ConceptID, FCM.GroupConceptNo ConceptNo,FCM.GroupConceptNo, FCm.ConceptDate, FB.BookingNo, FB.BookingDate, FB.BuyerID,
				FBC.ExcessPercentage,FBC.ExcessQty,FC.TotalQty,FBC.ExcessQtyInKG,
                FB.BuyerTeamID, FB.ExecutionCompanyID, FB.SupplierID, FCM.Qty, FCM.FabricID, Construction.SegmentValue Construction, Composition.SegmentValue Composition,
				
                Color = CASE WHEN FBC.SubGroupID=1 THEN Color.SegmentValue ELSE SV5.SegmentValue END, 
				Gsm = CASE WHEN FBC.SubGroupID=1 THEN Gsm.SegmentValue ELSE '' END,

				Length= CASE WHEN FBC.SubGroupID<>1 THEN CONVERT(decimal(18,2),Color.SegmentValue) ELSE 0 END, 
				Width= CASE WHEN FBC.SubGroupID<>1 THEN CONVERT(decimal(18,2),Gsm.SegmentValue) ELSE 0 END,
                FC.TotalQtyInKG,
                --TotalQtyInKG = Round(CASE WHEN FBC.SubGroupID<>1 THEN CONVERT(decimal(18,2),Color.SegmentValue)*CONVERT(decimal(18,2),Gsm.SegmentValue)*0.045*FC.TotalQty/420 ELSE 0 END,2),

                DT.SegmentValue DyeingType, KT.SegmentValue KnittingType, FBC.SubGroupID, ISG.SubGroupName ItemSubGroup,
                FTN.TechnicalName,FCM.BookingID, FCM.DayValidDurationId,
                ReqDate = MAX(FCM.ReqDate), FB.CollarSizeID, FB.CollarWeightInGm, FB.CuffSizeID, FB.CuffWeightInGm
                From FCM
                Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FB On FB.BookingID = FCM.BookingID
                Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = FCM.BookingChildID AND FCM.ConsumptionID = FBC.ConsumptionID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.BookingChildID = FBC.BookingChildID AND FC.ConsumptionID = FBC.ConsumptionID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.FabricID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue SV5 ON SV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue DT ON DT.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue KT ON KT.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FBC.SubGroupID --And
                LEFT JOIN {TableNames.FabricTechnicalName} FTN ON FBC.TechnicalNameId = FTN.TechnicalNameId
                Group By FCM.FCMRMasterID, FB.FBAckID, FCM.ConceptID, FCM.GroupConceptNo, FCm.ConceptDate, FB.BookingNo, FB.BookingDate, FB.BuyerID,
				FB.BuyerTeamID, FB.ExecutionCompanyID, FB.SupplierID, FCM.Qty, FCM.FabricID, Construction.SegmentValue, Composition.SegmentValue,
				Color.SegmentValue, Gsm.SegmentValue, DT.SegmentValue, KT.SegmentValue, FBC.SubGroupID, ISG.SubGroupName, FTN.TechnicalName,FCM.BookingID,
	            FBC.ExcessPercentage,FBC.ExcessQty,FC.TotalQty,FBC.ExcessQtyInKG,FC.TotalQtyInKG, SV5.SegmentValue, FCM.DayValidDurationId,
                FB.CollarSizeID, FB.CollarWeightInGm, FB.CuffSizeID, FB.CuffWeightInGm;

                --Childs
				With FCC As
                (
	                Select FCC.FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.YarnCategory, FCC.YD, FCC.YDItem, FCC.IsPR, FCC.UnitID,
					FCC.ReqQty,FCC.ReqCone, FCC.ShadeCode, FCC.Distribution, FCC.BookingQty, FCC.Allowance, FCC.YarnStockSetId, FCC.DayValidDurationId,
                    Isnull(YPM.YDProductionMasterID,0)YDProductionMasterID
                    FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCC
					INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM ON FCM.FCMRMasterID = FCC.FCMRMasterID
					INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = FCM.ConceptID
                    LEFT JOIN {TableNames.YD_BOOKING_MASTER} YBM ON YBM.ConceptID = FCM.ConceptID And YBM.ConceptID = CM.ConceptID
	                LEFT JOIN {TableNames.YD_PRODUCTION_MASTER} YPM ON YPM.YDBookingMasterID = YBM.YDBookingMasterID
					WHERE CM.GroupConceptNo = '{grpConceptNo}'
                )
                Select FCC.FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.YarnCategory, FCC.YD, FCC.YDItem, FCC.IsPR, FCC.UnitID, U.DisplayUnitDesc,
                FCC.ReqQty,FCC.ReqCone, FCC.ShadeCode, FCC.Distribution, FCC.BookingQty, FCC.Allowance,
                IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
                IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                ISV8.SegmentValue Segment8ValueDesc, YDProductionMasterID, FCC.YarnStockSetId, YSM.PhysicalCount, YSM.YarnLotNo, SpinnerName = '', YSM.SampleStockQty, YSM.AdvanceStockQty
                ,FCC.DayValidDurationId
                from FCC
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                LEFT JOIN {TableNames.YarnStockMaster_New} YSM ON YSM.ItemMasterID = FCC.ItemMasterID 
														--AND YSM.SupplierID = YDRC.SupplierID 
														--AND YSM.SpinnerID = YDBC.SpinnerID 
														--AND ISNULL(YSM.YarnLotNo,'') = ISNULL(YDBC.LotNo,'')
														--AND ISNULL(YSM.PhysicalCount,'') = ISNULL(YDBC.PhysicalCount,'')
														--AND ISNULL(YSM.ShadeCode,'') = ISNULL(FCC.ShadeCode,'')
														--AND YSM.BookingID = YDRC.BookingID
														--AND YSM.LocationID = YDRC.LocationID
														--AND YSM.CompanyID = YDRC.CompanyID
                --LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerId
                LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = FCC.UnitID;

                -- Item Segments
                  {CommonQueries.GetCertifications()};

                -- Fabric Components
                  {CommonQueries.GetFabricComponents(EntityTypeNameConstants.FABRIC_TYPE)};

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()};

                --Item Segments
                  {CommonQueries.GetSubPrograms()};

                -- DayValidDuration
                {CommonQueries.GetDayValidDurations()};


                --Color Wise Size Collar
                    SELECT distinct id=CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END+' X '+CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END,
                          text=CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END+' X '+CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END
                          FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} BAC
                          INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FCM ON FCM.BookingID = BAC.BookingID
                          INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                          WHERE FCM.BookingNo = '{grpConceptNo}' AND BAC.SubGroupID IN (11); 

                --Color Wise Size Cuff
                    SELECT distinct id=CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END+' X '+CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END,
                          text=CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END+' X '+CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END
                          FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} BAC
                          INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FCM ON FCM.BookingID = BAC.BookingID
                          INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                          WHERE FCM.BookingNo = '{grpConceptNo}' AND BAC.SubGroupID IN (12); 

                --Color Wise All Size Collar
                    SELECT BAC.*,ConstructionId=ISV1.SegmentValueID, Construction = ISV1.SegmentValue, 
                          CompositionId = ISV2.SegmentValueID,Composition = ISV2.SegmentValue,
                          ColorID = CASE WHEN BAC.SubGroupID = 1 THEN ISV3.SegmentValueID ELSE ISV5.SegmentValueID END, 
                          Color = CASE WHEN BAC.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                          Length = CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END,
                          Width = CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END
                          FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} BAC
                          INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FCM ON FCM.BookingID = BAC.BookingID
                          INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                          WHERE FCM.BookingNo = '{grpConceptNo}' AND BAC.SubGroupID IN (11);; 

                --Color Wise All Size Cuff
                    SELECT BAC.*,ConstructionId=ISV1.SegmentValueID, Construction = ISV1.SegmentValue, 
                          CompositionId = ISV2.SegmentValueID,Composition = ISV2.SegmentValue,
                          ColorID = CASE WHEN BAC.SubGroupID = 1 THEN ISV3.SegmentValueID ELSE ISV5.SegmentValueID END, 
                          Color = CASE WHEN BAC.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                          Length = CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END,
                          Width = CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END
                          FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} BAC
                          INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FCM ON FCM.BookingID = BAC.BookingID
                          INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                          LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                          WHERE FCM.BookingNo = '{grpConceptNo}' AND BAC.SubGroupID IN (12);";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query, segmentNames);
                FreeConceptMRMaster data = new FreeConceptMRMaster();
                data.OtherItems = records.Read<FreeConceptMRMaster>().ToList();
                foreach (FreeConceptMRMaster item in data.OtherItems)
                {
                    if (item.ItemSubGroup == "Fabric") data.HasFabric = true;
                    else if (item.ItemSubGroup == "Collar") data.HasCollar = true;
                    else if (item.ItemSubGroup == "Cuff") data.HasCuff = true;
                }

                List<FreeConceptMRChild> Childs = records.Read<FreeConceptMRChild>().ToList();
                data.OtherItems.ForEach(childDetails =>
                {
                    childDetails.Childs = Childs.Where(c => c.FCMRMasterID == childDetails.FCMRMasterID).ToList();
                });
                var _recvCertifications = await records.ReadAsync<Select2OptionModelExtended>();
                data.Certifications = _recvCertifications.Where(x => x.desc == ItemSegmentNameConstants.YARN_CERTIFICATIONS);
                data.FabricComponentsNew = await records.ReadAsync<Select2OptionModel>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();
                var itemSegments = await records.ReadAsync<Select2OptionModelExtended>();
                data.YarnSubProgramNews = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW);

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.OtherItems.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));

                data.CollarSizeList = records.Read<Select2OptionModel>().ToList();
                data.CuffSizeList = records.Read<Select2OptionModel>().ToList();
                data.AllCollarSizeList = records.Read<FBookingAcknowledgeChild>().ToList();
                data.AllCuffSizeList = records.Read<FBookingAcknowledgeChild>().ToList();

                if (data.OtherItems.Count() > 0)
                {
                    data.IsCheckDVD = data.OtherItems.First().ReqDate < CommonConstent.YarnSourcingModeImplementDate ? false : true;
                }

                data.OtherItems.Where(x => x.DayValidDurationId > 0).ToList().ForEach(x =>
                {
                    var dayObj = data.DayValidDurations.ToList().Find(d => d.id == x.DayValidDurationId.ToString());
                    if (dayObj.IsNotNull())
                    {
                        x.DayValidDurationName = dayObj.text;
                        x.DayDuration = Convert.ToInt32(dayObj.additionalValue);
                    }
                });

                if (menuParam == "MRPB")
                {
                    #region Grouping Collar Cuff
                    //Collar
                    List<FreeConceptMRMaster> tempChildsCollar = CommonFunction.DeepClone(data.OtherItems.Where(x => x.SubGroupID == 11).ToList());
                    var disList = tempChildsCollar.Select(m => new { m.Construction, m.Composition, m.Color, m.DayValidDurationId })
                            .Distinct()
                            .ToList();

                    List<FreeConceptMRMaster> tempChilds = new List<FreeConceptMRMaster>();
                    int conceptID = 1;
                    int fcmrMasterID = 1;
                    int ycChildItemID = 1;

                    List<FreeConceptMRChild> tempYarnChildItems = new List<FreeConceptMRChild>();

                    disList.ForEach(c =>
                    {
                        var tempChilds1 = tempChildsCollar.Where(x => x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color && x.DayValidDurationId == c.DayValidDurationId).ToList();
                        var firstChild = tempChilds1.First();

                        var fbac = new FreeConceptMRMaster()
                        {
                            FCMRMasterID = fcmrMasterID++,
                            FBAckID = firstChild.FBAckID,
                            BookingID = firstChild.BookingID,
                            BookingNo = firstChild.BookingNo,
                            BookingDate = firstChild.BookingDate,
                            Qty = tempChilds1.Sum(x => x.Qty),
                            TotalQty = tempChilds1.Sum(x => x.TotalQty),
                            TotalQtyInKG = tempChilds1.Sum(x => x.TotalQtyInKG),
                            FabricID = firstChild.FabricID,
                            ConstructionID = firstChild.ConstructionID,
                            CompositionId = firstChild.CompositionId,
                            Construction = firstChild.Construction,
                            Composition = firstChild.Composition,
                            Color = firstChild.Color,
                            DyeingType = firstChild.DyeingType,
                            KnittingType = firstChild.KnittingType,
                            SubGroupID = firstChild.SubGroupID,
                            ItemSubGroup = firstChild.ItemSubGroup,
                            TechnicalName = firstChild.TechnicalName,
                            ConceptID = conceptID++,
                            ConceptNo = "",
                            MachineType = firstChild.MachineType,
                            GSM = firstChild.GSM,
                            Length = firstChild.Length,
                            Width = firstChild.Width,
                            FabricWidth = firstChild.FabricWidth,
                            BuyerName = firstChild.BuyerName,
                            YarnSubProgram = firstChild.YarnSubProgram,
                            Instruction = firstChild.Instruction,
                            ReferenceSource = firstChild.ReferenceSource,
                            ReferenceNo = firstChild.ReferenceNo,
                            ColorReferenceNo = firstChild.ColorReferenceNo,
                            DayValidDurationId = firstChild.DayValidDurationId,
                            DayValidDurationName = firstChild.DayValidDurationName,
                            DayDuration = firstChild.DayDuration,
                            HasFabric = firstChild.HasFabric,
                            HasCollar = firstChild.HasCollar,
                            HasCuff = firstChild.HasCuff
                        };
                        #region Yarn Organizing

                        var mainChilds = CommonFunction.DeepClone(data.OtherItems.Where(x => x.SubGroupID == 11 && x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color && x.DayValidDurationId == c.DayValidDurationId).ToList());
                        List<int> itemMasterIds = new List<int>();
                        data.OtherItems.Where(x => x.SubGroupID == 11 && x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color && x.DayValidDurationId == c.DayValidDurationId).ToList().ForEach(x =>
                        {
                            tempYarnChildItems.AddRange(CommonFunction.DeepClone(x.Childs));
                        });
                        var yarnItems = mainChilds.First().Childs;
                        //var yarnItemsRevision = mainChilds.First().ChildItemsRevision;

                        yarnItems.ForEach(im =>
                        {
                            im.FCMRChildID = ycChildItemID++;
                            im.FCMRMasterID = fbac.FCMRMasterID;
                            im.BookingQty = tempYarnChildItems.Where(x => x.ItemMasterID == im.ItemMasterID).Sum(x => x.BookingQty);
                            im.ReqQty = tempYarnChildItems.Where(x => x.ItemMasterID == im.ItemMasterID).Sum(x => x.ReqQty);
                            //im.ReqCone = tempYarnChildItems.Where(x => x.ItemMasterID == im.ItemMasterID).Sum(x => x.ReqCone);

                            fbac.Childs.Add(im);
                        });
                        #endregion

                        tempChilds.Add(fbac);
                    });

                    //Cuff

                    List<FreeConceptMRMaster> tempChildsCuff = CommonFunction.DeepClone(data.OtherItems.Where(x => x.SubGroupID == 12).ToList());
                    disList = tempChildsCuff.Select(m => new { m.Construction, m.Composition, m.Color, m.DayValidDurationId })
                            .Distinct()
                            .ToList();

                    disList.ForEach(c =>
                    {
                        var tempChilds1 = tempChildsCuff.Where(x => x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color && x.DayValidDurationId == c.DayValidDurationId).ToList();
                        var firstChild = tempChilds1.First();
                        var fbac = new FreeConceptMRMaster()
                        {
                            FCMRMasterID = fcmrMasterID++,
                            FBAckID = firstChild.FBAckID,
                            BookingID = firstChild.BookingID,
                            BookingNo = firstChild.BookingNo,
                            BookingDate = firstChild.BookingDate,
                            Qty = tempChilds1.Sum(x => x.Qty),
                            TotalQty = tempChilds1.Sum(x => x.TotalQty),
                            TotalQtyInKG = tempChilds1.Sum(x => x.TotalQtyInKG),
                            FabricID = firstChild.FabricID,
                            ConstructionID = firstChild.ConstructionID,
                            CompositionId = firstChild.CompositionId,
                            Construction = firstChild.Construction,
                            Composition = firstChild.Composition,
                            Color = firstChild.Color,
                            DyeingType = firstChild.DyeingType,
                            KnittingType = firstChild.KnittingType,
                            SubGroupID = firstChild.SubGroupID,
                            ItemSubGroup = firstChild.ItemSubGroup,
                            TechnicalName = firstChild.TechnicalName,
                            ConceptID = conceptID++,
                            ConceptNo = "",
                            MachineType = firstChild.MachineType,
                            GSM = firstChild.GSM,
                            Length = firstChild.Length,
                            Width = firstChild.Width,
                            FabricWidth = firstChild.FabricWidth,
                            BuyerName = firstChild.BuyerName,
                            YarnSubProgram = firstChild.YarnSubProgram,
                            Instruction = firstChild.Instruction,
                            ReferenceSource = firstChild.ReferenceSource,
                            ReferenceNo = firstChild.ReferenceNo,
                            ColorReferenceNo = firstChild.ColorReferenceNo,
                            DayValidDurationId = firstChild.DayValidDurationId,
                            DayValidDurationName = firstChild.DayValidDurationName,
                            DayDuration = firstChild.DayDuration,
                            HasFabric = firstChild.HasFabric,
                            HasCollar = firstChild.HasCollar,
                            HasCuff = firstChild.HasCuff
                        };
                        #region Yarn Organizing

                        var mainChilds = CommonFunction.DeepClone(data.OtherItems.Where(x => x.SubGroupID == 12 && x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color && x.DayValidDurationId == c.DayValidDurationId).ToList());
                        List<int> itemMasterIds = new List<int>();
                        data.OtherItems.Where(x => x.SubGroupID == 12 && x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color && x.DayValidDurationId == c.DayValidDurationId).ToList().ForEach(x =>
                        {
                            tempYarnChildItems.AddRange(CommonFunction.DeepClone(x.Childs));
                        });
                        var yarnItems = mainChilds.First().Childs;
                        //var yarnItemsRevision = mainChilds.First().ChildItemsRevision;

                        yarnItems.ForEach(im =>
                        {
                            im.FCMRChildID = ycChildItemID++;
                            im.FCMRMasterID = fbac.FCMRMasterID;
                            im.BookingQty = tempYarnChildItems.Where(x => x.ItemMasterID == im.ItemMasterID).Sum(x => x.BookingQty);
                            im.ReqQty = tempYarnChildItems.Where(x => x.ItemMasterID == im.ItemMasterID).Sum(x => x.ReqQty);
                            //im.ReqCone = tempYarnChildItems.Where(x => x.ItemMasterID == im.ItemMasterID).Sum(x => x.ReqCone);

                            fbac.Childs.Add(im);
                        });
                        #endregion
                        tempChilds.Add(fbac);
                    });

                    data.OtherItems = data.OtherItems.Where(x => x.SubGroupID == 1).ToList();
                    data.OtherItems.AddRange(tempChilds);
                    #endregion
                }

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<FreeConceptMRMaster> GetRevisionOfCompleteList(string grpConceptNo, string menuParam = null)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.YARN_CERTIFICATIONS,
                    ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW
                }
            };
            var query =
                        $@"
                        With FCM As
                        (
                            Select MR.FCMRMasterID, MR.ConceptID, MR.FabricID, MR.ReqDate,  
                            FCM.SubGroupID, FCM.TechnicalNameId, FCM.ExcessPercentage,
		                    FCM.ExcessQty,FCM.TotalQty,FCM.ExcessQtyInKG, FCM.GroupConceptNo, FCM.ConceptDate, 
		                    FCM.BookingID, FCM.BookingChildID, FCM.Qty,FCM.ConsumptionID,TotalQtyInKG = Round(FCM.TotalQtyInKG,2),
		                    DayValidDurationId = ISNULL(FBC.DayValidDurationId,0)
	                        FROM {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR
	                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MR.ConceptID
		                    LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = FCM.BookingChildID AND FBC.BookingID = FCM.BookingID
                            WHERE FCM.GroupConceptNo = '{grpConceptNo}'
                        )
                        Select FCM.FCMRMasterID, FB.FBAckID, FCM.ConceptID, FCM.GroupConceptNo ConceptNo,FCM.GroupConceptNo, FCm.ConceptDate, FB.BookingNo, FB.BookingDate, FB.BuyerID,
                        C.ShortName BuyerName,FCM.ExcessPercentage,FCM.ExcessQty,FCM.TotalQty,FCM.ExcessQtyInKG,FCM.TotalQtyInKG,
                        FB.BuyerTeamID, FB.ExecutionCompanyID, FB.SupplierID, FCM.Qty, FCM.FabricID, 
                        ISV1.SegmentValue Construction, 
                        ISV2.SegmentValue Composition,
                        Color = CASE WHEN FCM.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                        Gsm = CASE WHEN FCM.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
                        DyeingType = CASE WHEN FCM.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
                        KnittingType = CASE WHEN FCM.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
                        Length = CASE WHEN FCM.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
                        Width = CASE WHEN FCM.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
                        FCM.SubGroupID, ISG.SubGroupName ItemSubGroup,
                        FTN.TechnicalName,FCM.BookingID

                        , (SELECT STUFF((
                        SELECT DISTINCT ', '+ ETV1.ValueName
                        FROM {DbNames.EPYSL}..SampleBookingConsumptionYarnSubBrand SBCYSB
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV1 ON ETV1.ValueID = SBCYSB.YarnSubBrandID
                        WHERE SBCYSB.BookingID=FCM.BookingID AND SBCYSB.ConsumptionID=FCM.ConsumptionID
                        FOR XML PATH('')),1,1,'')) YarnSubProgram,

                        FCM.DayValidDurationId,
                        ReqDate = MAX(FCM.ReqDate), FB.CollarSizeID, FB.CollarWeightInGm, FB.CuffSizeID, FB.CuffWeightInGm

                        From FCM
                        Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FB On FB.BookingID = FCM.BookingID
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.FabricID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FCM.SubGroupID --And
                        LEFT JOIN {TableNames.FabricTechnicalName} FTN ON FTN.TechnicalNameId = FCM.TechnicalNameId
                        LEFT JOIN {DbNames.EPYSL}..Contacts C On C.ContactID = FB.BuyerID
                        Group By FCM.FCMRMasterID, FB.FBAckID, FCM.ConceptID, FCM.GroupConceptNo, FCm.ConceptDate, FB.BookingNo, FB.BookingDate, FB.BuyerID,C.ShortName,
                        FB.BuyerTeamID, FB.ExecutionCompanyID, FB.SupplierID, FCM.Qty, FCM.FabricID, ISV1.SegmentValue, ISV2.SegmentValue,
                        ISV3.SegmentValue, ISV4.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, FCM.SubGroupID, ISG.SubGroupName, FTN.TechnicalName,FCM.BookingID,
                        FCM.ExcessPercentage,FCM.ExcessQty,FCM.TotalQty,FCM.ExcessQtyInKG,FCM.TotalQtyInKG,FCM.ConsumptionID,ISV5.SegmentValue, FCM.DayValidDurationId,
                        FB.CollarSizeID, FB.CollarWeightInGm, FB.CuffSizeID, FB.CuffWeightInGm;

                        --Childs
                        With FCC As
                        (
                            Select FCC.FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.YarnCategory, FCC.YD, FCC.YDItem, FCC.IsPR, FCC.UnitID,
							FCC.ReqQty,FCC.ReqCone, FCC.ShadeCode, FCC.Distribution, FCC.BookingQty, FCC.Allowance, FCC.YarnStockSetId, FCC.DayValidDurationId,
                            CM.TotalQtyInKG
				            FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCC
				            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM ON FCM.FCMRMasterID = FCC.FCMRMasterID
				            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = FCM.ConceptID
				            --Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = CM.BookingChildID And FBC.ItemMasterID = CM.ItemMasterId
                            WHERE CM.GroupConceptNo = '{grpConceptNo}'
                        )
                        select FCC.FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.YarnCategory, FCC.YD, FCC.YDItem, FCC.IsPR, FCC.UnitID, U.DisplayUnitDesc,
                        FCC.ReqQty,FCC.ReqCone, FCC.ShadeCode, FCC.Distribution, FCC.BookingQty, FCC.Allowance, FCC.TotalQtyInKG,
                        IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
                        IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                        ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                        ISV8.SegmentValue Segment8ValueDesc, FCC.YarnStockSetId, YSS.PhysicalCount, YSS.YarnLotNo, SpinnerName = SP.ShortName, YSM.SampleStockQty, YSM.AdvanceStockQty
                        ,FCC.DayValidDurationId
                        from FCC
                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCC.ItemMasterID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                        LEFT JOIN YarnStockSet YSS ON YSS.YarnStockSetId = FCC.YarnStockSetId
                        LEFT JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
                        LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerId
                        LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = FCC.UnitID;

                        --Item Segments
                        {CommonQueries.GetCertifications()};

                        --Fabric Components
                        {CommonQueries.GetFabricComponents(EntityTypeNameConstants.FABRIC_TYPE)};

                        --Shade book
                        {CommonQueries.GetYarnShadeBooks()};

                        --Item Segments
                        {CommonQueries.GetSubPrograms()}; 

                        -- DayValidDuration
                        {CommonQueries.GetDayValidDurations()};

                        --Fiber-SubProgram-Certifications Mapping Setup
                        Select * FROM {TableNames.FIBER_SUBPROGRAM_CERTIFICATIONS_FILTER_SETUP};


                        --Color Wise Size Collar
                            SELECT distinct id=CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END+' X '+CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END,
                                  text=CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END+' X '+CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END
                                  FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} BAC
                                  INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FCM ON FCM.BookingID = BAC.BookingID
                                  INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                                  WHERE FCM.BookingNo = '{grpConceptNo}' AND BAC.SubGroupID IN (11); 

                        --Color Wise Size Cuff
                            SELECT distinct id=CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END+' X '+CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END,
                                  text=CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END+' X '+CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END
                                  FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} BAC
                                  INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FCM ON FCM.BookingID = BAC.BookingID
                                  INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                                  WHERE FCM.BookingNo = '{grpConceptNo}' AND BAC.SubGroupID IN (12); 

                        --Color Wise All Size Collar
                            SELECT BAC.*,ConstructionId=ISV1.SegmentValueID, Construction = ISV1.SegmentValue, 
                                  CompositionId = ISV2.SegmentValueID,Composition = ISV2.SegmentValue,
                                  ColorID = CASE WHEN BAC.SubGroupID = 1 THEN ISV3.SegmentValueID ELSE ISV5.SegmentValueID END, 
                                  Color = CASE WHEN BAC.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                                  Length = CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END,
                                  Width = CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END
                                  FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} BAC
                                  INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FCM ON FCM.BookingID = BAC.BookingID
                                  INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                                  WHERE FCM.BookingNo = '{grpConceptNo}' AND BAC.SubGroupID IN (11);; 

                         --Color Wise All Size Cuff
                            SELECT BAC.*,ConstructionId=ISV1.SegmentValueID, Construction = ISV1.SegmentValue, 
                                  CompositionId = ISV2.SegmentValueID,Composition = ISV2.SegmentValue,
                                  ColorID = CASE WHEN BAC.SubGroupID = 1 THEN ISV3.SegmentValueID ELSE ISV5.SegmentValueID END, 
                                  Color = CASE WHEN BAC.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
                                  Length = CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV3.SegmentValue END,
                                  Width = CASE WHEN BAC.SubGroupID = 1 THEN '0' ELSE ISV4.SegmentValue END
                                  FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} BAC
                                  INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FCM ON FCM.BookingID = BAC.BookingID
                                  INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BAC.ItemMasterID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                                  LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                                  WHERE FCM.BookingNo = '{grpConceptNo}' AND BAC.SubGroupID IN (12);";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query, segmentNames);
                FreeConceptMRMaster data = new FreeConceptMRMaster();
                data.OtherItems = records.Read<FreeConceptMRMaster>().ToList();
                List<FreeConceptMRChild> Childs = records.Read<FreeConceptMRChild>().ToList();
                var _recvCertifications = await records.ReadAsync<Select2OptionModelExtended>();
                data.Certifications = _recvCertifications.Where(x => x.desc == ItemSegmentNameConstants.YARN_CERTIFICATIONS);
                data.FabricComponentsNew = await records.ReadAsync<Select2OptionModel>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();
                var itemSegments = await records.ReadAsync<Select2OptionModelExtended>();
                data.YarnSubProgramNews = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW);

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.OtherItems.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.FabricComponentMappingSetupList = records.Read<FabricComponentMappingSetup>().ToList();

                data.CollarSizeList = records.Read<Select2OptionModel>().ToList();
                data.CuffSizeList = records.Read<Select2OptionModel>().ToList();
                data.AllCollarSizeList = records.Read<FBookingAcknowledgeChild>().ToList();
                data.AllCuffSizeList = records.Read<FBookingAcknowledgeChild>().ToList();

                if (data.OtherItems.Count() > 0)
                {
                    data.IsCheckDVD = data.OtherItems.First().ReqDate < CommonConstent.YarnSourcingModeImplementDate ? false : true;
                }

                foreach (FreeConceptMRMaster item in data.OtherItems)
                {
                    if (item.ItemSubGroup == "Fabric") data.HasFabric = true;
                    else if (item.ItemSubGroup == "Collar") data.HasCollar = true;
                    else if (item.ItemSubGroup == "Cuff") data.HasCuff = true;
                    item.Childs = Childs.Where(c => c.FCMRMasterID == item.FCMRMasterID).ToList();

                    var dayObj = data.DayValidDurations.ToList().Find(d => d.id == item.DayValidDurationId.ToString());
                    if (dayObj.IsNotNull())
                    {
                        item.DayValidDurationName = dayObj.text;
                        item.DayDuration = Convert.ToInt32(dayObj.additionalValue);
                    }
                }

                if (menuParam == "MRPB")
                {
                    #region Grouping Collar Cuff
                    //Collar
                    List<FreeConceptMRMaster> tempChildsCollar = CommonFunction.DeepClone(data.OtherItems.Where(x => x.SubGroupID == 11).ToList());
                    var disList = tempChildsCollar.Select(m => new { m.Construction, m.Composition, m.Color, m.DayValidDurationId })
                            .Distinct()
                            .ToList();

                    List<FreeConceptMRMaster> tempChilds = new List<FreeConceptMRMaster>();
                    int conceptID = 1;
                    int fcmrMasterID = 1;
                    int ycChildItemID = 1;

                    List<FreeConceptMRChild> tempYarnChildItems = new List<FreeConceptMRChild>();

                    disList.ForEach(c =>
                    {
                        var tempChilds1 = tempChildsCollar.Where(x => x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color && x.DayValidDurationId == c.DayValidDurationId).ToList();
                        var firstChild = tempChilds1.First();

                        var fbac = new FreeConceptMRMaster()
                        {
                            FCMRMasterID = fcmrMasterID++,
                            FBAckID = firstChild.FBAckID,
                            BookingID = firstChild.BookingID,
                            BookingNo = firstChild.BookingNo,
                            BookingDate = firstChild.BookingDate,
                            Qty = tempChilds1.Sum(x => x.Qty),
                            TotalQty = tempChilds1.Sum(x => x.TotalQty),
                            TotalQtyInKG = tempChilds1.Sum(x => x.TotalQtyInKG),
                            FabricID = firstChild.FabricID,
                            ConstructionID = firstChild.ConstructionID,
                            CompositionId = firstChild.CompositionId,
                            Construction = firstChild.Construction,
                            Composition = firstChild.Composition,
                            Color = firstChild.Color,
                            DyeingType = firstChild.DyeingType,
                            KnittingType = firstChild.KnittingType,
                            SubGroupID = firstChild.SubGroupID,
                            ItemSubGroup = firstChild.ItemSubGroup,
                            TechnicalName = firstChild.TechnicalName,
                            ConceptID = conceptID++,
                            ConceptNo = "",
                            MachineType = firstChild.MachineType,
                            GSM = firstChild.GSM,
                            Length = firstChild.Length,
                            Width = firstChild.Width,
                            FabricWidth = firstChild.FabricWidth,
                            BuyerName = firstChild.BuyerName,
                            YarnSubProgram = firstChild.YarnSubProgram,
                            Instruction = firstChild.Instruction,
                            ReferenceSource = firstChild.ReferenceSource,
                            ReferenceNo = firstChild.ReferenceNo,
                            ColorReferenceNo = firstChild.ColorReferenceNo,
                            DayValidDurationId = firstChild.DayValidDurationId,
                            DayValidDurationName = firstChild.DayValidDurationName,
                            DayDuration = firstChild.DayDuration,
                            HasFabric = firstChild.HasFabric,
                            HasCollar = firstChild.HasCollar,
                            HasCuff = firstChild.HasCuff
                        };
                        #region Yarn Organizing

                        var mainChilds = CommonFunction.DeepClone(data.OtherItems.Where(x => x.SubGroupID == 11 && x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color && x.DayValidDurationId == c.DayValidDurationId).ToList());
                        List<int> itemMasterIds = new List<int>();
                        data.OtherItems.Where(x => x.SubGroupID == 11 && x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color && x.DayValidDurationId == c.DayValidDurationId).ToList().ForEach(x =>
                        {
                            tempYarnChildItems.AddRange(CommonFunction.DeepClone(x.Childs));
                        });
                        var yarnItems = mainChilds.First().Childs;
                        //var yarnItemsRevision = mainChilds.First().ChildItemsRevision;

                        yarnItems.ForEach(im =>
                        {
                            im.FCMRChildID = ycChildItemID++;
                            im.FCMRMasterID = fbac.FCMRMasterID;
                            im.BookingQty = tempYarnChildItems.Where(x => x.ItemMasterID == im.ItemMasterID).Sum(x => x.BookingQty);
                            im.ReqQty = tempYarnChildItems.Where(x => x.ItemMasterID == im.ItemMasterID).Sum(x => x.ReqQty);
                            //im.ReqCone = tempYarnChildItems.Where(x => x.ItemMasterID == im.ItemMasterID).Sum(x => x.ReqCone);

                            fbac.Childs.Add(im);
                        });
                        #endregion

                        tempChilds.Add(fbac);
                    });

                    //Cuff

                    List<FreeConceptMRMaster> tempChildsCuff = CommonFunction.DeepClone(data.OtherItems.Where(x => x.SubGroupID == 12).ToList());
                    disList = tempChildsCuff.Select(m => new { m.Construction, m.Composition, m.Color, m.DayValidDurationId })
                            .Distinct()
                            .ToList();

                    disList.ForEach(c =>
                    {
                        var tempChilds1 = tempChildsCuff.Where(x => x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color && x.DayValidDurationId == c.DayValidDurationId).ToList();
                        var firstChild = tempChilds1.First();
                        var fbac = new FreeConceptMRMaster()
                        {
                            FCMRMasterID = fcmrMasterID++,
                            FBAckID = firstChild.FBAckID,
                            BookingID = firstChild.BookingID,
                            BookingNo = firstChild.BookingNo,
                            BookingDate = firstChild.BookingDate,
                            Qty = tempChilds1.Sum(x => x.Qty),
                            TotalQty = tempChilds1.Sum(x => x.TotalQty),
                            TotalQtyInKG = tempChilds1.Sum(x => x.TotalQtyInKG),
                            FabricID = firstChild.FabricID,
                            ConstructionID = firstChild.ConstructionID,
                            CompositionId = firstChild.CompositionId,
                            Construction = firstChild.Construction,
                            Composition = firstChild.Composition,
                            Color = firstChild.Color,
                            DyeingType = firstChild.DyeingType,
                            KnittingType = firstChild.KnittingType,
                            SubGroupID = firstChild.SubGroupID,
                            ItemSubGroup = firstChild.ItemSubGroup,
                            TechnicalName = firstChild.TechnicalName,
                            ConceptID = conceptID++,
                            ConceptNo = "",
                            MachineType = firstChild.MachineType,
                            GSM = firstChild.GSM,
                            Length = firstChild.Length,
                            Width = firstChild.Width,
                            FabricWidth = firstChild.FabricWidth,
                            BuyerName = firstChild.BuyerName,
                            YarnSubProgram = firstChild.YarnSubProgram,
                            Instruction = firstChild.Instruction,
                            ReferenceSource = firstChild.ReferenceSource,
                            ReferenceNo = firstChild.ReferenceNo,
                            ColorReferenceNo = firstChild.ColorReferenceNo,
                            DayValidDurationId = firstChild.DayValidDurationId,
                            DayValidDurationName = firstChild.DayValidDurationName,
                            DayDuration = firstChild.DayDuration,
                            HasFabric = firstChild.HasFabric,
                            HasCollar = firstChild.HasCollar,
                            HasCuff = firstChild.HasCuff
                        };
                        #region Yarn Organizing

                        var mainChilds = CommonFunction.DeepClone(data.OtherItems.Where(x => x.SubGroupID == 12 && x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color && x.DayValidDurationId == c.DayValidDurationId).ToList());
                        List<int> itemMasterIds = new List<int>();
                        data.OtherItems.Where(x => x.SubGroupID == 12 && x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color && x.DayValidDurationId == c.DayValidDurationId).ToList().ForEach(x =>
                        {
                            tempYarnChildItems.AddRange(CommonFunction.DeepClone(x.Childs));
                        });
                        var yarnItems = mainChilds.First().Childs;
                        //var yarnItemsRevision = mainChilds.First().ChildItemsRevision;

                        yarnItems.ForEach(im =>
                        {
                            im.FCMRChildID = ycChildItemID++;
                            im.FCMRMasterID = fbac.FCMRMasterID;
                            im.BookingQty = tempYarnChildItems.Where(x => x.ItemMasterID == im.ItemMasterID).Sum(x => x.BookingQty);
                            im.ReqQty = tempYarnChildItems.Where(x => x.ItemMasterID == im.ItemMasterID).Sum(x => x.ReqQty);
                            //im.ReqCone = tempYarnChildItems.Where(x => x.ItemMasterID == im.ItemMasterID).Sum(x => x.ReqCone);

                            fbac.Childs.Add(im);
                        });
                        #endregion
                        tempChilds.Add(fbac);
                    });

                    data.OtherItems = data.OtherItems.Where(x => x.SubGroupID == 1).ToList();
                    data.OtherItems.AddRange(tempChilds);
                    #endregion
                }

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<FreeConceptMRMaster> GetPendingAcknowledgeList(string grpConceptNo)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.YARN_CERTIFICATIONS
                }
            };
            var query =
                        $@"
            With FCM As
            (
                Select MR.YBookingNo,MR.YBookingID, MR.BookingID, MR.SubGroupID, MRC.ItemMasterID, 
	            --FCM.SubGroupID, 
	            FCM.TechnicalNameId, FCM.ExcessPercentage,FCM.ExcessQty,FCM.TotalQty,FCM.ExcessQtyInKG, FCM.GroupConceptNo, FCM.ConceptDate, 
	            --FCM.BookingID, 
	            FCM.BookingChildID, FCM.Qty,FCM.ConsumptionID,TotalQtyInKG = Round(FCM.TotalQtyInKG,2)
	            FROM {TableNames.YarnBookingMaster_New} MR
	            INNER JOIN {TableNames.YarnBookingChild_New} MRC on MR.YBookingID=MRC.YBookingID
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.BookingID = MR.BookingID
	
	            --Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = FCM.BookingChildID And FBC.ItemMasterID = FCM.ItemMasterId
                WHERE FCM.GroupConceptNo = '{grpConceptNo}'
            )
            Select FCM.YBookingNo,FCM.YBookingID, FB.FBAckID, FCM.BookingID, FCM.GroupConceptNo ConceptNo,FCM.GroupConceptNo, FCm.ConceptDate, FB.BookingNo, FB.BookingDate, FB.BuyerID,
            FCM.ExcessPercentage,FCM.ExcessQty,FCM.TotalQty,FCM.ExcessQtyInKG,FCM.TotalQtyInKG,
            FB.BuyerTeamID, FB.ExecutionCompanyID, FB.SupplierID, FCM.Qty, FCM.ItemMasterID, Construction.SegmentValue Construction, Composition.SegmentValue Composition,
            Color.SegmentValue Color, Gsm.SegmentValue Gsm, DT.SegmentValue DyeingType, KT.SegmentValue KnittingType, FCM.SubGroupID, ISG.SubGroupName ItemSubGroup,
            FTN.TechnicalName,FCM.BookingID

            , (SELECT STUFF((
            SELECT DISTINCT ', '+ ETV1.ValueName
            FROM {DbNames.EPYSL}..SampleBookingConsumptionYarnSubBrand SBCYSB
            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV1 ON ETV1.ValueID = SBCYSB.YarnSubBrandID
            WHERE SBCYSB.BookingID=FCM.BookingID AND SBCYSB.ConsumptionID=FCM.ConsumptionID
            FOR XML PATH('')),1,1,'')) YarnSubProgram

            From FCM
            Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FB On FB.BookingID = FCM.BookingID
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Width ON Width.SegmentValueID = IM.Segment5ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue DT ON DT.SegmentValueID = IM.Segment6ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue KT ON KT.SegmentValueID = IM.Segment7ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FCM.SubGroupID --And
            LEFT JOIN {TableNames.FabricTechnicalName} FTN ON FTN.TechnicalNameId = FCM.TechnicalNameId
            Group By FCM.YBookingNo,FCM.YBookingID, FB.FBAckID, FCM.BookingID, FCM.GroupConceptNo, FCm.ConceptDate, FB.BookingNo, FB.BookingDate, FB.BuyerID,
            FB.BuyerTeamID, FB.ExecutionCompanyID, FB.SupplierID, FCM.Qty, FCM.ItemMasterID, Construction.SegmentValue, Composition.SegmentValue,
            Color.SegmentValue, Gsm.SegmentValue, DT.SegmentValue, KT.SegmentValue, FCM.SubGroupID, ISG.SubGroupName, FTN.TechnicalName,FCM.BookingID,
            FCM.ExcessPercentage,FCM.ExcessQty,FCM.TotalQty,FCM.ExcessQtyInKG,FCM.TotalQtyInKG,FCM.ConsumptionID;

            --Childs
            With FCC As
            (
                Select FCC.FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.YarnCategory, FCC.YD, FCC.YDItem, FCC.IsPR, FCC.UnitID,
				FCC.ReqQty,FCC.ReqCone, FCC.ShadeCode, FCC.Distribution, FCC.BookingQty, FCC.Allowance, FCC.DayValidDurationId, CM.TotalQtyInKG
	            FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCC
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM ON FCM.FCMRMasterID = FCC.FCMRMasterID
	            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = FCM.ConceptID
	            --Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = CM.BookingChildID And FBC.ItemMasterID = CM.ItemMasterId
                WHERE CM.GroupConceptNo = '{grpConceptNo}'
            )
            select FCC.FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.YarnCategory, FCC.YD, FCC.YDItem, FCC.IsPR, FCC.UnitID, U.DisplayUnitDesc,
            FCC.ReqQty,FCC.ReqCone, FCC.ShadeCode, FCC.Distribution, FCC.BookingQty, FCC.Allowance, FCC.TotalQtyInKG,
            IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
            IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
            ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
            ISV8.SegmentValue Segment8ValueDesc, FCC.DayValidDurationId

            from FCC
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCC.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
            --LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = FCC.YarnBrandID
            LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = FCC.UnitID;

            -- Item Segments
            {CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()};

            -- Fabric Components
            {CommonQueries.GetEntityTypeValuesOnly(EntityTypeNameConstants.FABRIC_TYPE)}

            -- Shade book
            {CommonQueries.GetYarnShadeBooks()};

            -- DayValidDuration
            {CommonQueries.GetDayValidDurations()};";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query, segmentNames);
                FreeConceptMRMaster data = new FreeConceptMRMaster();
                data.OtherItems = records.Read<FreeConceptMRMaster>().ToList();
                List<FreeConceptMRChild> Childs = records.Read<FreeConceptMRChild>().ToList();
                data.Certifications = await records.ReadAsync<Select2OptionModelExtended>();
                data.FabricComponents = await records.ReadAsync<string>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();
                data.YBookingNo = data.OtherItems.Count() > 0 ? data.OtherItems.First().YBookingNo : "";
                foreach (FreeConceptMRMaster item in data.OtherItems)
                {
                    if (item.ItemSubGroup == "Fabric") data.HasFabric = true;
                    else if (item.ItemSubGroup == "Collar") data.HasCollar = true;
                    else if (item.ItemSubGroup == "Cuff") data.HasCuff = true;
                    item.Childs = Childs.Where(c => c.FCMRMasterID == item.FCMRMasterID).ToList();
                }

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.OtherItems.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.IsCheckDVD = true;

                data.OtherItems.Where(x => x.DayValidDurationId > 0).ToList().ForEach(x =>
                {
                    var dayObj = data.DayValidDurations.ToList().Find(d => d.id == x.DayValidDurationId.ToString());
                    if (dayObj.IsNotNull())
                    {
                        x.DayValidDurationName = dayObj.text;
                        x.DayDuration = Convert.ToInt32(dayObj.additionalValue);
                    }
                });
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<FreeConceptMRMaster> GetRevision(int fbAckId, string grpConceptNo)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.YARN_CERTIFICATIONS,
                    ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW
                }
            };

            var query =
                $@"
                With FB As
                (
	                Select * FROM {TableNames.FBBOOKING_ACKNOWLEDGE} WHERE FBAckID = {fbAckId}
                )
                SELECT FCMRMasterID = (CASE WHEN ISNULL(MR.FCMRMasterID,0) = 0 THEN FC.ConceptID ELSE MR.FCMRMasterID END), FB.FBAckID, FB.BookingID, FB.BookingNo, FB.BookingDate, FB.BuyerID,FB.BuyerTeamID,
                FB.ExecutionCompanyID, FB.SupplierID,
                FC.TotalQty Qty, FC.ExcessPercentage, FC.ExcessQty, FC.ExcessQtyInKG, FC.TotalQty, FC.TotalQtyInKG,
                FC.ItemMasterID FabricID, Construction.SegmentValue Construction,
                Composition.SegmentValue Composition, DT.SegmentValue DyeingType,
                KT.SegmentValue KnittingType, FC.SubGroupID, ISG.SubGroupName ItemSubGroup, FTN.TechnicalName, FC.ConceptID, FC.ConceptNo, FC.ConceptDate

                ,KMS.SubClassName MachineType, SBC.Segment1Desc Construction,SBC.Segment2Desc Composition,
				
				Color = CASE WHEN FC.SubGroupID=1 THEN Color.SegmentValue ELSE SV5.SegmentValue END, 
				Gsm = CASE WHEN FC.SubGroupID=1 THEN Gsm.SegmentValue ELSE '' END,

				Length= CASE WHEN FC.SubGroupID<>1 THEN CONVERT(decimal(18,2),Color.SegmentValue) ELSE 0 END, 
				Width= CASE WHEN FC.SubGroupID<>1 THEN CONVERT(decimal(18,2),Gsm.SegmentValue) ELSE 0 END,

                TotalQtyInKG = Round(CASE WHEN FC.SubGroupID<>1 THEN CONVERT(decimal(18,2),Color.SegmentValue)*CONVERT(decimal(18,2),Gsm.SegmentValue)*0.045*FC.TotalQty/420 ELSE 0 END,2),
				
				SBC.Segment5Desc FabricWidth, SBC.Segment7Desc KnittingType,
				C.ShortName BuyerName

                , (SELECT STUFF((
				SELECT DISTINCT ', '+ ETV1.ValueName
				FROM {DbNames.EPYSL}..SampleBookingConsumptionYarnSubBrand SBCYSB
				LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV1 ON ETV1.ValueID = SBCYSB.YarnSubBrandID
				WHERE SBCYSB.BookingID = FC.BookingID AND SBCYSB.ConsumptionID = FC.ConsumptionID
				FOR XML PATH('')),1,1,'')) YarnSubProgram

				,SBC.Remarks Instruction,SBC.ForBDSStyleNo
				,ReferenceSource = SBC.ReferenceSourceID,ReferenceNo=SBC.ReferenceNo, ColorReferenceNo=SBC.ColorReferenceNo
                ,FBC.DayValidDurationId

                ,FB.BookingDate

                FROM FB
				INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.AcknowledgeID = FB.FBAckID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.BookingChildID = FBC.BookingChildID AND FC.BookingID = FB.BookingID
				LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.ConceptID=FC.ConceptID
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue SV5 ON SV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue DT ON DT.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue KT ON KT.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = FC.SubGroupID
                LEFT JOIN {TableNames.FabricTechnicalName} FTN ON FTN.TechnicalNameId = FC.TechnicalNameId
                LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} KMS ON KMS.SubClassID = FC.MCSubClassID
				LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumption SBC ON SBC.BookingID = FC.BookingID AND SBC.ConsumptionID = FC.ConsumptionID
                LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumptionChild SBCC ON SBCC.BookingID = FC.BookingID AND SBCC.ConsumptionID = FC.ConsumptionID AND SBCC.ItemMasterID = FC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..Contacts C On C.ContactID = FB.BuyerID
				Where FC.ConsumptionID is Not Null;

                --Childs
				With FCC As
                (
	                Select FCC.* 
                    FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCC
		            INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCM ON FCM.FCMRMasterID = FCC.FCMRMasterID
		            INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = FCM.ConceptID
		            WHERE CM.GroupConceptNo = '{grpConceptNo}'
                )
                select FCC.FCMRChildID, FCC.FCMRMasterID, FCC.ItemMasterID, FCC.YarnCategory, FCC.YD, FCC.YDItem, FCC.IsPR, FCC.UnitID, U.DisplayUnitDesc,
                FCC.ReqQty,FCC.ReqCone, FCC.ShadeCode, FCC.Distribution, FCC.BookingQty, FCC.Allowance,
                IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
                IM.Segment8ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                ISV8.SegmentValue Segment8ValueDesc, FCC.YarnStockSetId, YSS.PhysicalCount, YSS.YarnLotNo, SpinnerName = SP.ShortName, YSM.SampleStockQty, YSM.AdvanceStockQty
                ,FCC.DayValidDurationId
                from FCC
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCC.ItemMasterID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
	            LEFT JOIN YarnStockSet YSS ON YSS.YarnStockSetId = FCC.YarnStockSetId
                LEFT JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
	            LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerId
                LEFT JOIN  {DbNames.EPYSL}..Unit U ON U.UnitID = FCC.UnitID;
                
                --Item Segments
                {CommonQueries.GetCertifications()};

                --Fabric Components
                {CommonQueries.GetFabricComponents(EntityTypeNameConstants.FABRIC_TYPE)};

                --Shade book
                {CommonQueries.GetYarnShadeBooks()};

                --Item Segments
                {CommonQueries.GetSubPrograms()}; 

                -- DayValidDuration
                {CommonQueries.GetDayValidDurations()};


                --Fiber-SubProgram-Certifications Mapping Setup
                Select * FROM {TableNames.FIBER_SUBPROGRAM_CERTIFICATIONS_FILTER_SETUP}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query, segmentNames);
                FreeConceptMRMaster data = new FreeConceptMRMaster();
                data.OtherItems = records.Read<FreeConceptMRMaster>().ToList();
                List<FreeConceptMRChild> Childs = records.Read<FreeConceptMRChild>().ToList();
                var _recvCertifications = await records.ReadAsync<Select2OptionModelExtended>();
                data.Certifications = _recvCertifications.Where(x => x.desc == ItemSegmentNameConstants.YARN_CERTIFICATIONS);
                data.FabricComponentsNew = await records.ReadAsync<Select2OptionModel>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();
                var itemSegments = await records.ReadAsync<Select2OptionModelExtended>();
                data.YarnSubProgramNews = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW);

                foreach (FreeConceptMRMaster item in data.OtherItems)
                {
                    if (item.ItemSubGroup == "Fabric") data.HasFabric = true;
                    else if (item.ItemSubGroup == "Collar") data.HasCollar = true;
                    else if (item.ItemSubGroup == "Cuff") data.HasCuff = true;
                    item.Childs = Childs.Where(c => c.FCMRMasterID == item.FCMRMasterID).ToList();
                }
                data.BuyerName = data.OtherItems.Count() > 0 ? data.OtherItems.First().BuyerName : "";

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.OtherItems.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.FabricComponentMappingSetupList = records.Read<FabricComponentMappingSetup>().ToList();

                if (data.OtherItems.Count() > 0)
                {
                    data.IsCheckDVD = data.OtherItems.First().BookingDate < CommonConstent.YarnSourcingModeImplementDate ? false : true;
                }

                data.OtherItems.Where(x => x.DayValidDurationId > 0).ToList().ForEach(x =>
                {
                    var dayObj = data.DayValidDurations.ToList().Find(d => d.id == x.DayValidDurationId.ToString());
                    if (dayObj.IsNotNull())
                    {
                        x.DayValidDurationName = dayObj.text;
                        x.DayDuration = Convert.ToInt32(dayObj.additionalValue);
                    }
                });
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<FBookingAcknowledge> GetPYBForBulkAsync(string bookingNo)
        {

            string sql = $@"  
                --Seq 1
                 Select top 1 * FROM {TableNames.FBBOOKING_ACKNOWLEDGE} Where BookingNo = '{bookingNo}';

                --Seq 2
                WITH 
                FBAC AS
                (
	                Select SBC.BookingID, SBC.BookingChildID, SBCon.ItemGroupID, SBC.SubGroupID, SBC.ItemMasterID, U.RelativeFactor,
	                ISG.SubGroupName, ConsumptionQty = Ceiling(Sum(SBC.ConsumptionQty)),  SBC.ConsumptionID, SBC.MachineTypeId,SBC.TechnicalNameID,
	                ColorID = CASE WHEN SBC.SubGroupID = 1 THEN IM.Segment3ValueID ELSE IM.Segment5ValueID END,
	                BookingUnitID = (CASE WHEN SBC.SubGroupID = 1 THEN SBC.BookingUnitID ELSE 28 END),
	                BookingUOM = CASE WHEN SBC.SubGroupID = 1 THEN  U.DisplayUnitDesc ELSE 'KG' END, 
	                BookingQty = Ceiling(Sum(SBC.BookingQty)),
	                RequisitionQty = Ceiling(Sum(SBC.BookingQty)), SBCon.Remarks,
	                SBCon.LengthYds, SBCon.LengthInch, SBCon.FUPartID, 
	                YarnTypeID = CASE WHEN M.IsSample = 0 THEN A.A1ValueID ELSE SBCon.A1ValueID END, 
	                YarnType = CASE WHEN M.IsSample = 0 THEN ISVA1.SegmentValue ELSE ISVA11.SegmentValue END, 
	                SBCon.YarnBrandID, YarnBrand = ETV.ValueName, FUP.PartName, ForTechPack = Convert(Varchar(50),''), 
	                ISourcing = Convert(bit,1), ISourcingName = 'In-House', ContactName = '',ContactID = 0,
	                LabDipNo = IsNull(SBCon.LabDipNo,''), BlockBookingQty = Convert(decimal,0), AdjustQty = Convert(decimal,0), 
	                AutoAgree = Convert(bit,0),Price = convert(decimal,0), SuggestedPrice = convert(decimal,0), 
	                IsCompleteReceive = Convert(bit,'0'), IsCompleteDelivery = Convert(bit,'0'), M.BuyerID, M.BuyerTeamID, M.ExportOrderID,
                    ConstructionID = ISV1.SegmentValueID,
	                Construction = ISV1.SegmentValue, 
	                Composition = ISV2.SegmentValue,
	                Color = CASE WHEN SBC.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
	                Gsm = CASE WHEN SBC.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
	                DyeingType = CASE WHEN SBC.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
	                KnittingType = CASE WHEN SBC.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
	                Length = CASE WHEN SBC.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
	                Width = CASE WHEN SBC.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
	                FabricWidth = CASE WHEN SBC.SubGroupID = 1 THEN ISV5.SegmentValue ELSE '' END,
	                Instruction = CASE WHEN M.IsSample = 0 THEN A.Remarks ELSE SBCon.Remarks END,
	                YarnProgram = CASE WHEN M.IsSample = 0 THEN ETV.ValueName ELSE ETV1.ValueName END,
                    SBC.DayValidDurationId, FCM.ConceptID, FCMRMasterID = ISNULL(FCMR.FCMRMasterID,0)

	                FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} SBC 
                    LEFT Join {DbNames.EPYSL}..BookingChild A on A.ConsumptionID = SBC.ConsumptionID AND A.ItemMasterID = SBC.ItemMasterID
	                LEFT Join {DbNames.EPYSL}..SampleBookingConsumption SBCon On SBCon.ConsumptionID = SBC.ConsumptionID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} M On M.FBAckID = SBC.AcknowledgeID
					INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.BookingID = SBC.BookingID AND FCM.BookingChildID = SBC.BookingChildID
					LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCMR ON FCMR.ConceptID = FCM.ConceptID
	                Left Join {DbNames.EPYSL}..Unit U On U.UnitID = SBC.BookingUnitID
	                INNER Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = SBC.ItemGroupID
	                INNER Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBC.SubGroupID
	                Left Join {DbNames.EPYSL}..ItemSegmentValue ISVA1 On ISVA1.SegmentValueID = A.A1ValueID
	                Left Join {DbNames.EPYSL}..ItemSegmentValue ISVA11 On ISVA11.SegmentValueID = SBCon.A1ValueID
	                Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = A.YarnBrandID
	                Left Join {DbNames.EPYSL}..EntityTypeValue ETV1 ON ETV1.ValueID = SBCon.YarnBrandID
	                Left Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = SBCon.FUPartID 
	                Left Join {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = SBC.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                Where M.BookingNo = '{bookingNo}' AND SBC.IsDeleted=0                              
	                Group By SBC.BookingID, SBC.BookingChildID, SBCon.ItemGroupID, SBC.ItemMasterID, ISG.SubGroupName,
	                SBC.BookingUnitID,U.DisplayUnitDesc,SBCon.LengthYds,SBCon.LengthInch,SBCon.FUPartID,A.A1ValueID,SBCon.A1ValueID,SBCon.YarnBrandID,ISVA1.SegmentValue,ISVA11.SegmentValue,
	                IsNull(SBCon.LabDipNo,''),SBCon.Remarks,ISVA1.SegmentValue,ETV.ValueName,ETV1.ValueName,FUP.PartName, M.BuyerID, 
	                M.BuyerTeamID, M.ExportOrderID, U.RelativeFactor, SBC.ConsumptionID, SBC.MachineTypeId,SBC.TechnicalNameID, SBCon.Segment3Desc, SBCon.Segment4Desc,
	                SBC.SubGroupID, IM.Segment3ValueID, IM.Segment5ValueID, A.Remarks, SBCon.Remarks, M.IsSample
	                ,ISV1.SegmentValueID,ISV1.SegmentValue,ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, SBC.DayValidDurationId, FCM.ConceptID, ISNULL(FCMR.FCMRMasterID,0)
                ),
                FinalList AS
                (
	                SELECT FBAC.BookingID, FBAC.BookingChildID, FBAC.ItemGroupID, FBAC.SubGroupID, FBAC.ItemMasterID, FBAC.SubGroupName,
	                BookingUnitID,LengthYds,LengthInch,FUPartID,YarnBrandID,
	                LabDipNo,Remarks,FBAC.YarnTypeID,FBAC.YarnType,YarnBrand,PartName, FBAC.BuyerID, FBAC.
	                BuyerTeamID, FBAC.ExportOrderID, FBAC.ConsumptionID, FBAC.MachineTypeId,FBAC.TechnicalNameID, FBAC.ConsumptionQty, FBAC.BookingUOM, FBAC.RequisitionQty,
	                FBAC.ForTechPack, FBAC.ISourcing, FBAC.ISourcingName, FBAC.ContactName, FBAC.ContactID, FBAC.BlockBookingQty, FBAC.AdjustQty,
	                AutoAgree, FBAC.Price, FBAC.SuggestedPrice, FBAC.IsCompleteReceive, FBAC.IsCompleteDelivery,
	                ColorID, FBAC.Construction, FBAC.ConstructionID, FBAC.Composition, FBAC.Color, FBAC.Gsm, FBAC.DyeingType, FBAC.KnittingType, FBAC.Length, FBAC.Width,
	                FBAC.BookingQty,
	                RS.RefSourceID, RS.RefSourceNo, RS.SourceConsumptionID, SourceItemMasterID = RS.ItemMasterID, FBAC.FabricWidth, T.TechnicalName, KMS.SubClassName MachineType,FBAC.Instruction, FBAC.YarnProgram, FBAC.DayValidDurationId, FBAC.ConceptID, FBAC.FCMRMasterID
                    FROM FBAC
                    LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} KMS ON KMS.SubClassID = FBAC.MachineTypeId
                    LEFT JOIN {TableNames.FabricTechnicalName} T ON T.TechnicalNameId = FBAC.TechnicalNameID
	                LEFT JOIN {DbNames.EPYSL}..BookingChildReferenceSource RS ON RS.BookingID = FBAC.BookingID AND RS.ConsumptionID = FBAC.ConsumptionID
	                Group By FBAC.BookingID, FBAC.BookingChildID, FBAC.ItemGroupID, FBAC.SubGroupID, FBAC.ItemMasterID, FBAC.SubGroupName,
	                BookingUnitID,LengthYds,LengthInch,FUPartID,YarnBrandID,
	                LabDipNo,Remarks,FBAC.YarnType,YarnBrand,PartName, FBAC.BuyerID, FBAC.BuyerTeamID, FBAC.ExportOrderID, FBAC.ConsumptionID, FBAC.MachineTypeId,FBAC.TechnicalNameID, FBAC.ConsumptionQty, FBAC.BookingUOM, FBAC.RequisitionQty,
	                FBAC.YarnTypeID, FBAC.ForTechPack, FBAC.ISourcing, FBAC.ISourcingName, FBAC.ContactName, FBAC.ContactID, FBAC.BlockBookingQty, FBAC.AdjustQty,
	                AutoAgree, FBAC.Price, FBAC.SuggestedPrice, FBAC.IsCompleteReceive, FBAC.IsCompleteDelivery,FBAC.BookingQty,
	                ColorID, FBAC.Construction, FBAC.ConstructionID, FBAC.Composition, FBAC.Color, FBAC.Gsm, FBAC.DyeingType, FBAC.KnittingType, FBAC.Length, FBAC.Width, FBAC.RelativeFactor
                    ,RS.RefSourceID, RS.RefSourceNo, RS.SourceConsumptionID, RS.ItemMasterID, FBAC.FabricWidth, T.TechnicalName, KMS.SubClassName,FBAC.Instruction, FBAC.YarnProgram, FBAC.DayValidDurationId, FBAC.ConceptID, FBAC.FCMRMasterID
                )
                SELECT * FROM FinalList;";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                FBookingAcknowledge data = records.Read<FBookingAcknowledge>().FirstOrDefault();
                data.MRMasters = records.Read<FreeConceptMRMaster>().ToList();


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
        public async Task<List<FreeConceptMRChild>> GetPYBYarnByBookingNo(string bookingNo)
        {
            string sql = $@"SELECT FCMC.*, FCM.BookingChildID, SubGroupId = FCM.SubGroupID
                    ,Segment1ValueId = ISV1.SegmentValueID
                    ,Segment1ValueDesc = ISV1.SegmentValue
                    ,Segment2ValueId = ISV2.SegmentValueID
                    ,Segment2ValueDesc = ISV2.SegmentValue
                    ,Segment3ValueId = ISV3.SegmentValueID
                    ,Segment3ValueDesc = ISV3.SegmentValue
                    ,Segment4ValueId = ISV4.SegmentValueID
                    ,Segment4ValueDesc = ISV4.SegmentValue
                    ,Segment5ValueId = ISV5.SegmentValueID
                    ,Segment5ValueDesc = ISV5.SegmentValue
                    ,Segment6ValueId = ISV6.SegmentValueID
                    ,Segment6ValueDesc = ISV6.SegmentValue
                    FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMC
                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} FCMM ON FCMM.FCMRMasterID = FCMC.FCMRMasterID
                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMM.ConceptID
                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBAC ON FBAC.BookingID = FCM.BookingID AND FBAC.BookingChildID = FCM.BookingChildID
					INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.FBAckID = FBAC.AcknowledgeID
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                    WHERE FBA.BookingNo = '{bookingNo}'";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<FreeConceptMRChild> data = records.Read<FreeConceptMRChild>().ToList();
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
        public async Task<List<FreeConceptMaster>> GetAllConceptByBookingNo(string bookingNo)
        {
            string sql = $@"Select FCM.*
                    FROM {TableNames.RND_FREE_CONCEPT_MASTER} FCM 
                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBAC ON FBAC.BookingID = FCM.BookingID AND FBAC.BookingChildID = FCM.BookingChildID
                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.FBAckID = FBAC.AcknowledgeID
                    Where FBA.BookingNo = '{bookingNo}'";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<FreeConceptMaster> data = records.Read<FreeConceptMaster>().ToList();
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
        public async Task SaveAsync(FreeConceptMRMaster entity)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();
                var freeConceptStatusList = await _conceptStatusService.GetByCPSIDs("1,2", entity.ConceptID.ToString());

                bool isPR = false;
                bool isYD = false;

                var objList = entity.Childs.Where(x => x.IsPR == true).ToList();
                if (objList != null && objList.Count() > 0) isPR = true;

                objList = entity.Childs.Where(x => x.YD == true).ToList();
                if (objList != null && objList.Count() > 0) isYD = true;

                int maxChildId = 0;
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.FCMRMasterID = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MR_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MR_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        foreach (var item in entity.Childs)
                        {
                            item.FCMRChildID = maxChildId++;
                            item.FCMRMasterID = entity.FCMRMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MR_CHILD, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in addedChilds)
                        {
                            item.FCMRChildID = maxChildId++;
                            item.FCMRMasterID = entity.FCMRMasterID;
                        }
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.Childs.SetDeleted();
                        break;

                    default:
                        break;
                }

                entity.ConceptStatusList = this.GetConceptStatusList(entity.ConceptID, freeConceptStatusList, isPR, isYD);

                await _service.SaveSingleAsync(entity, _connection, transaction);
                //var childsTemp = entity.Childs.Where(x => x.YarnCategory.Trim() == "").ToList();
                //if (childsTemp.Count() > 0)
                //{
                //    throw new Exception("Yarn Category missing => SaveAsync => MaterialRequirementBDSService");
                //}
                await _service.SaveAsync(entity.Childs, _connection, transaction);
                await _service.SaveAsync(entity.ConceptStatusList, _connection, transaction);
                transaction.Commit();
                transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (transactionGmt != null) transactionGmt.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
                throw ex;
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                _connection.Close();
                _connectionGmt.Close();
            }
        }

        public async Task SaveMultipleAsync(List<FreeConceptMRMaster> entities, EntityState entityState, int userId, List<FreeConceptMaster> freeConceptsUpdate = null, FBookingAcknowledge bookingChildEntity = null)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();
                var conceptIds = string.Join(",", entities.Select(x => x.ConceptID).Distinct());
                var freeConceptStatusList = await _conceptStatusService.GetByCPSIDs("1,2", conceptIds);

                switch (entityState)
                {
                    case EntityState.Added:
                        entities = await AddManyAsync(entities, freeConceptStatusList);
                        break;

                    case EntityState.Modified:
                        entities = await UpdateManyAsync(entities, freeConceptStatusList);
                        break;

                    default:
                        break;
                }

                await _service.SaveAsync(entities, _connection, transaction);

                List<FreeConceptMRChild> childs = new List<FreeConceptMRChild>();
                List<BDSDependentTNACalander> BDSDependentTNACalanders = new List<BDSDependentTNACalander>();
                List<ConceptStatus> statusList = new List<ConceptStatus>();

                entities.ForEach(entity =>
                {
                    childs.AddRange(entity.Childs);
                    statusList.AddRange(entity.ConceptStatusList);
                    BDSDependentTNACalanders.AddRange(entity.BDSDependentTNACalanders);
                });

                BDSDependentTNACalanders = entities[0].BDSDependentTNACalanders.ToList();
                BDSDependentTNACalanders.ForEach(x =>
                {
                    x.EntityState = x.BDSEventID > 0 ? EntityState.Modified : EntityState.Added;
                });


                //var childsTemp = childs.Where(x => x.YarnCategory.Trim() == "").ToList();
                //if (childsTemp.Count() > 0)
                //{
                //    throw new Exception("Yarn Category missing => SaveMultipleAsync => MaterialRequirementBDSService");
                //}

                await _service.SaveAsync(childs, _connection, transaction);
                foreach (FreeConceptMRChild item in childs)
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Validation_FreeConceptMRChild", item.EntityState, userId, item.FCMRChildID);
                    await _connection.ExecuteAsync(SPNames.sp_Validation_FreeConceptMRChild, new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.FCMRChildID }, transaction, 30, CommandType.StoredProcedure);

                }

                await _service.SaveAsync(BDSDependentTNACalanders, _connection, transaction);
                if (entityState == EntityState.Added && statusList.Count() > 0)
                {
                    await _service.SaveAsync(statusList, _connection, transaction);
                }
                if (freeConceptsUpdate != null && freeConceptsUpdate.Count > 0)
                {
                    await _service.SaveAsync(freeConceptsUpdate, _connection, transaction);
                }
                if (bookingChildEntity != null)
                {
                    await _service.SaveSingleAsync(bookingChildEntity, _connection, transaction);
                }

                transaction.Commit();
                transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (transactionGmt != null) transactionGmt.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
                throw ex;
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                _connection.Close();
                _connectionGmt.Close();
            }
        }

        private async Task<List<FreeConceptMRMaster>> AddManyAsync(List<FreeConceptMRMaster> entities, List<ConceptStatus> freeConceptStatusList)
        {
            int fcMRMasterID = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MR_MASTER, entities.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            int maxChildId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MR_CHILD, entities.Sum(x => x.Childs.Count), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            entities.ToList().ForEach(entity =>
            {
                entity.FCMRMasterID = fcMRMasterID++;
                entity.EntityState = EntityState.Added;
                entity.ConceptStatusList = new List<ConceptStatus>();

                bool isPR = false;
                bool isYD = false;

                var objList = entity.Childs.Where(x => x.IsPR == true).ToList();
                if (objList != null && objList.Count() > 0) isPR = true;

                objList = entity.Childs.Where(x => x.YD == true).ToList();
                if (objList != null && objList.Count() > 0) isYD = true;

                foreach (FreeConceptMRChild item in entity.Childs)
                {
                    item.FCMRChildID = maxChildId++;
                    item.FCMRMasterID = entity.FCMRMasterID;
                }
                entity.ConceptStatusList = this.GetConceptStatusList(entity.ConceptID, freeConceptStatusList, isPR, isYD);
            });

            return entities;
        }

        private async Task<List<FreeConceptMRMaster>> UpdateManyAsync(List<FreeConceptMRMaster> entities, List<ConceptStatus> freeConceptStatusList)
        {
            int fcMRMasterID = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MR_MASTER, entities.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            int maxChildId = await _service.GetMaxIdAsync(TableNames.RND_FREE_CONCEPT_MR_CHILD, entities.Sum(x => x.Childs.Where(y => y.EntityState == EntityState.Added).Count()), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            entities.ToList().ForEach(entity =>
            {
                bool isPR = false;
                bool isYD = false;

                var objList = entity.Childs.Where(x => x.IsPR == true).ToList();
                if (objList != null && objList.Count() > 0) isPR = true;

                objList = entity.Childs.Where(x => x.YD == true).ToList();
                if (objList != null && objList.Count() > 0) isYD = true;

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.FCMRMasterID = fcMRMasterID++;
                        foreach (FreeConceptMRChild item in entity.Childs)
                        {
                            item.FCMRChildID = maxChildId++;
                            item.FCMRMasterID = entity.FCMRMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        List<FreeConceptMRChild> addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        foreach (FreeConceptMRChild item in addedChilds)
                        {
                            item.FCMRChildID = maxChildId++;
                            item.FCMRMasterID = entity.FCMRMasterID;
                        }
                        break;

                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.Childs.SetDeleted();
                        break;

                    default:
                        break;
                }
                entity.ConceptStatusList = this.GetConceptStatusList(entity.ConceptID, freeConceptStatusList, isPR, isYD);
            });

            return entities;
        }

        private List<ConceptStatus> GetConceptStatusList(int conceptID, List<ConceptStatus> freeConceptStatusList, bool isPR, bool isYD)
        {
            var conceptWiseFreeConceptStatusList = freeConceptStatusList.Where(x => x.ConceptID == conceptID).ToList();
            var finalList = new List<ConceptStatus>();
            conceptWiseFreeConceptStatusList.ForEach(x =>
            {
                bool isApplicable = x.IsApplicable;
                if (x.CPSID == 1) isApplicable = isPR;
                else if (x.CPSID == 2) isApplicable = isYD;

                finalList.Add(new ConceptStatus
                {
                    FCSID = x.FCSID,
                    ConceptID = conceptID,
                    CPSID = x.CPSID,
                    IsApplicable = isApplicable,
                    SeqNo = x.SeqNo,
                    Status = x.Status,
                    Remarks = x.Remarks,
                    EntityState = EntityState.Modified
                });
            });
            return finalList;
        }

        public async Task ReviseAsync(List<FreeConceptMRMaster> entities, string grpConceptNo, int userId, string fcmrChildIds, List<YarnPRMaster> prMasters, List<FreeConceptMaster> freeConceptsUpdate = null, FBookingAcknowledge bookingChildEntity = null)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();
                await _connection.ExecuteAsync("spBackupFreeConceptMR_Full", new { ConceptNo = grpConceptNo, UserId = userId }, transaction, 30, CommandType.StoredProcedure);

                var conceptIds = string.Join(",", entities.Select(x => x.ConceptID).Distinct());
                var freeConceptStatusList = await _conceptStatusService.GetByCPSIDs("1,2", conceptIds);

                entities = await UpdateManyAsync(entities, freeConceptStatusList);


                List<FreeConceptMRChild> childs = new List<FreeConceptMRChild>();
                List<BDSDependentTNACalander> BDSDependentTNACalanders = new List<BDSDependentTNACalander>();
                List<ConceptStatus> statusList = new List<ConceptStatus>();
                entities.ForEach(entity =>
                {
                    childs.AddRange(entity.Childs);
                    statusList.AddRange(entity.ConceptStatusList);
                    BDSDependentTNACalanders.AddRange(entity.BDSDependentTNACalanders);
                });

                BDSDependentTNACalanders = entities[0].BDSDependentTNACalanders.ToList();
                BDSDependentTNACalanders.ForEach(x =>
                {
                    x.EntityState = x.BDSEventID > 0 ? EntityState.Modified : EntityState.Added;
                });

                List<FreeConceptMRMaster> freeConceptMRs = entities.Where(x => x.EntityState != EntityState.Deleted).ToList();
                List<FreeConceptMRMaster> freeConceptMRDeletes = entities.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<FreeConceptMRChild> freeConceptMRChilds = childs.Where(x => x.EntityState != EntityState.Deleted).ToList();
                List<FreeConceptMRChild> freeConceptMRChildDeletes = childs.Where(x => x.EntityState == EntityState.Deleted).ToList();

                List<BDSDependentTNACalander> calenders = BDSDependentTNACalanders.Where(x => x.EntityState != EntityState.Deleted).ToList();
                List<BDSDependentTNACalander> calenderDeletes = BDSDependentTNACalanders.Where(x => x.EntityState == EntityState.Deleted).ToList();

                await _service.SaveAsync(freeConceptMRs, _connection, transaction);
                foreach (FreeConceptMRMaster item in freeConceptMRs.Where(x => x.IsNeedRevision == false))
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_UpdateKnittingPlanMasterRevisionNo", item.EntityState, userId, item.ConceptID);
                    await _connection.ExecuteAsync("sp_UpdateKnittingPlanMasterRevisionNo", new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.ConceptID }, transaction, 30, CommandType.StoredProcedure);
                }
                foreach (FreeConceptMRMaster item in freeConceptMRs.Where(x => x.IsNeedRevisionTemp == true))
                {
                //await _service.ValidationSingleAsync(item, transaction, "sp_UpdateFreeConceptMRMaster_ItemRevisionNo", item.EntityState, userId, item.ConceptID);
                    await _connection.ExecuteAsync("sp_UpdateFreeConceptMRMaster_ItemRevisionNo", new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.ConceptID }, transaction, 30, CommandType.StoredProcedure);
                }
                //var childsTemp = freeConceptMRChilds.Where(x => x.YarnCategory.Trim() == "").ToList();
                //if (childsTemp.Count() > 0)
                //{
                //    throw new Exception("Yarn Category missing => ReviseAsync => MaterialRequirementBDSService");
                //}
                await _service.SaveAsync(freeConceptMRChilds, _connection, transaction);
                foreach (FreeConceptMRChild item in childs)
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Validation_FreeConceptMRChild", item.EntityState, userId, item.FCMRChildID);
                    await _connection.ExecuteAsync("sp_Validation_FreeConceptMRChild", new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.FCMRChildID }, transaction, 30, CommandType.StoredProcedure);
                }
                await _service.SaveAsync(calenders, _connection, transaction);

                await _service.SaveAsync(freeConceptMRChildDeletes, _connection, transaction);
                await _service.SaveAsync(freeConceptMRDeletes, _connection, transaction);
                await _service.SaveAsync(calenderDeletes, _connection, transaction);

                if (prMasters.Count() > 0)
                {
                    await _service.SaveAsync(prMasters, _connection, transaction);
                }

                if (fcmrChildIds.IsNotNullOrEmpty())
                {
                    var query = $@"UPDATE PR SET PR.NeedRevision = 1
                            FROM YarnPRMaster PR
                            INNER JOIN YarnPRChild PC ON PC.YarnPRMasterID = PR.YarnPRMasterID
                            WHERE PC.FCMRChildID IN ({fcmrChildIds});";
                    await _connection.ExecuteAsync(query, null, transaction);
                }
                if (freeConceptsUpdate != null && freeConceptsUpdate.Count > 0)
                {
                    await _service.SaveAsync(freeConceptsUpdate, _connection, transaction);
                }
                if (bookingChildEntity != null)
                {
                    await _service.SaveSingleAsync(bookingChildEntity, _connection, transaction);
                }

                transaction.Commit();
                transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (transactionGmt != null) transactionGmt.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
                throw ex;
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                _connection.Close();
                _connectionGmt.Close();
            }
        }
        public async Task<List<FreeConceptMRChild>> GetCompleteMRChilds(PaginationInfo paginationInfo, string buyerIds, string buyerTeamIDs)
        {
            if (buyerIds.IsNullOrEmpty()) return new List<FreeConceptMRChild>();
            if (buyerTeamIDs == "-") buyerTeamIDs = "";

            paginationInfo.OrderBy = string.IsNullOrEmpty(paginationInfo.OrderBy) ? " ORDER BY FCMRChildID DESC" : paginationInfo.OrderBy;

            string buyerTeamIdCon = "";
            if (!buyerTeamIDs.IsNullOrEmpty())
            {
                buyerTeamIdCon = $@" AND FCM.BuyerTeamID IN ({buyerTeamIDs})";
            }

            var sql =
                $@"
                    WITH C AS (
                    SELECT C.FCMRChildID, C.ItemMasterId, C.ShadeCode, C.BookingQty, C.ReqCone, C.Remarks,
                    C.UnitID, U.DisplayUnitDesc, BookingNo = FCM.GroupConceptNo,
                    IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID,
                    ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                    ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc

                    FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} C
                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} M ON M.FCMRMasterID=C.FCMRMasterID
                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID=M.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID=C.ItemMasterId
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN  {DbNames.EPYSL}..Unit U ON U.UnitID = C.UnitID
                    WHERE M.IsComplete=1 AND FCM.BuyerID IN ({buyerIds}) 
                    {buyerTeamIdCon})
                    SELECT * FROM C

                    {paginationInfo.FilterBy}
                    {paginationInfo.OrderBy}
                    {paginationInfo.PageBy};";

            return await _service.GetDataAsync<FreeConceptMRChild>(sql, _connection);
        }
        public async Task AcknowledgeEntityAsync(YarnPRMaster yarnPRMaster, int userId)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                yarnPRMaster.YarnPRMasterID = await _service.GetMaxIdAsync(TableNames.YARN_PR_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                yarnPRMaster.YarnPRNo = await _service.GetMaxNoAsync(TableNames.YARN_PRNO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);
                int maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_PR_CHILD, yarnPRMaster.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                foreach (YarnPRChild child in yarnPRMaster.Childs)
                {
                    child.YarnPRChildID = maxChildId++;
                    child.YarnPRMasterID = yarnPRMaster.YarnPRMasterID;
                }
                await _service.SaveSingleAsync(yarnPRMaster, _connection, transaction);
                await _service.SaveAsync(yarnPRMaster.Childs, _connection, transaction);
                foreach (YarnPRChild item in yarnPRMaster.Childs)
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Validation_YarnPRChild", item.EntityState, userId, item.YarnPRChildID);
                    await _connection.ExecuteAsync("sp_Validation_YarnPRChild", new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.YarnPRChildID }, transaction, 30, CommandType.StoredProcedure);
                }
                transaction.Commit();
                transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (transactionGmt != null) transactionGmt.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
                throw ex;
            }
            finally
            {
                _connection.Close();
                _connectionGmt.Close();
            }
        }
    }
}

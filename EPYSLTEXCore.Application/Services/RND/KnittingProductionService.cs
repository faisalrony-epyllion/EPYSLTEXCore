using Dapper;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Static;
using System.Data;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Statics;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting;

namespace EPYSLTEX.Infrastructure.Services
{
    public class KnittingProductionService : IKnittingProductionService
    {
        private readonly IDapperCRUDService<KnittingProduction> _service;
        private readonly IDapperCRUDService<DyeingBatchItemRoll> _serviceDBIR;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        private decimal _currentTime;

        public KnittingProductionService(IDapperCRUDService<KnittingProduction> service
            , IDapperCRUDService<DyeingBatchItemRoll> serviceDBIR)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
            
            _serviceDBIR = serviceDBIR;
            //_serviceDBIR.Connection = serviceDBIR.GetConnection(AppConstants.GMT_CONNECTION);
            //_connectionGmt = serviceDBIR.Connection;

            _serviceDBIR.Connection = serviceDBIR.GetConnection(AppConstants.TEXTILE_CONNECTION);
            //_connection = serviceDBIR.Connection;
        }

        public async Task<List<KnittingProduction>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By GRollID Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;

            if (status == Status.Pending)
            {
                sql += $@"
               With F As (
                    Select J.KJobCardMasterID, J.KJobCardNo, JobCardDate = J.KJobCardDate, EWO = J.ExportOrderID, BookingQty = C.TotalQty, 
                    JobCardQty = J.KJobCardQty, ProducedQty = Case When J.SubGroupID = 1 Then J.ProdQty Else J.ProdQtyPcs End, 
	                J.ConceptId, J.Remarks, C.ConceptNo, C.ConceptDate, ISG.SubGroupName, MachineType = T.TypeName, 
                    KPM.PlanNo, KnittingType = KT.TypeName, Technical.TechnicalName, Composition = COM.SegmentValue, C.IsBDS, 
                    C.GroupConceptNo, GSM = Case When C.SubGroupID = 1 Then Gsm.SegmentValue Else '' End, 
                    Size = Case When C.SubGroupID <> 1 Then Color.SegmentValue + ' X ' + Gsm.SegmentValue Else '' End,
	                --ColorName = Case When C.SubGroupID = 1 Then Color.SegmentValue Else '' End,
                    ColorName = Case When C.SubGroupID = 1 Then ISNULL(Color.SegmentValue,'') Else ISNULL(CollarCuffColor.SegmentValue,'') End,
                    J.IsSubContact, Buyer = Case When IsNull(KPM.BuyerID,0)=0 Then '' Else CTO.ShortName END, CCT.TeamName BuyerTeam
                    FROM {TableNames.KNITTING_JOB_CARD_Master} J
                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} C ON J.ConceptID = C.ConceptID
                    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} T ON J.MachineKnittingTypeID = T.TypeID
                    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID = J.KPChildID
                    LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID = KPC.KPMasterID
	                LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = C.KnittingTypeID
	                LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId=C.TechnicalNameId
                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = KPM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = KPM.BuyerTeamID
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = C.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COM ON COM.SegmentValueID = C.CompositionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = C.SubGroupID   
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue CollarCuffColor ON CollarCuffColor.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {TableNames.RND_KNITTING_PRODUCTION} KP ON KP.KJobCardMasterID = J.KJobCardMasterID
                    WHERE KP.GRollID IS NULL AND KPM.GrayFabricOK = 1 AND KPM.IsConfirm = 1
                ) 
                Select *, COUNT(*) Over() TotalRows
                From F";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By F.KJobCardMasterID Desc" : paginationInfo.OrderBy;
            }
            else if (status == Status.Completed)
            {
                sql += $@"
                With F As (
                    Select J.KJobCardMasterID, J.KJobCardNo, JobCardDate = J.KJobCardDate, EWO = J.ExportOrderID, BookingQty = C.TotalQty, JobCardQty = J.KJobCardQty,  J.ConceptId, J.Remarks,
                    C.ConceptNo, C.ConceptDate,ISG.SubGroupName, MachineType = T.TypeName, KPM.PlanNo, KnittingType = KT.TypeName, Technical.TechnicalName, Composition = COM.SegmentValue,
                    ProducedQty = Case When J.SubGroupID = 1 Then J.ProdQty Else J.ProdQtyPcs End, C.IsBDS, C.GroupConceptNo, J.IsSubContact,
                    GSM = Case When C.SubGroupID = 1 Then Gsm.SegmentValue Else '' End,
                    Buyer = Case When IsNull(KPM.BuyerID,0)=0 Then '' Else CTO.ShortName END, CCT.TeamName BuyerTeam,
	                Size = Case When C.SubGroupID <> 1 Then Color.SegmentValue + ' X ' + Gsm.SegmentValue Else '' End,
                    ColorName = Case When C.SubGroupID = 1 Then ISNULL(Color.SegmentValue,'') Else ISNULL(CollarCuffColor.SegmentValue,'') End,
                    JobCardStatus = JCS.[text], ProductionDate = MAX(KP.ProductionDate)
                    FROM {TableNames.KNITTING_JOB_CARD_Master} J
                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} C ON J.ConceptID = C.ConceptID
                    LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} T ON J.MachineKnittingTypeID = T.TypeID
                    LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID = J.KPChildID
                    LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID = KPC.KPMasterID
	                LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = C.KnittingTypeID
	                LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId=C.TechnicalNameId
                    LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = KPM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = KPM.BuyerTeamID
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = C.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COM ON COM.SegmentValueID = C.CompositionID
                    LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = C.SubGroupID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue CollarCuffColor ON CollarCuffColor.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = IM.Segment4ValueID 
                    LEFT JOIN {TableNames.JobCardStatus} JCS ON JCS.id = J.Status
	                LEFT JOIN {TableNames.RND_KNITTING_PRODUCTION} KP ON KP.KJobCardMasterID = J.KJobCardMasterID
                    where KP.GRollID IS NOT NULL AND KPM.IsConfirm = 1 --18242
	                GROUP BY J.KJobCardMasterID, J.KJobCardNo, J.KJobCardDate, J.ExportOrderID, C.TotalQty, J.KJobCardQty, J.ConceptId, J.Remarks,
                    C.ConceptNo, C.ConceptDate,ISG.SubGroupName,T.TypeName, KPM.PlanNo, KT.TypeName, Technical.TechnicalName, COM.SegmentValue,
                    Case When J.SubGroupID = 1 Then J.ProdQty Else J.ProdQtyPcs End, C.IsBDS, C.GroupConceptNo, J.IsSubContact,
                    Case When C.SubGroupID = 1 Then Gsm.SegmentValue Else '' End, 
                    Case When IsNull(KPM.BuyerID,0)=0 Then '' Else CTO.ShortName END, CCT.TeamName,
	                Case When C.SubGroupID <> 1 Then Color.SegmentValue + ' X ' + Gsm.SegmentValue Else '' End,
                    Case When C.SubGroupID = 1 Then ISNULL(Color.SegmentValue,'') Else ISNULL(CollarCuffColor.SegmentValue,'') End,
                    JCS.[text]
                )

                Select *, COUNT(*) Over() TotalRows
                From F";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By F.KJobCardMasterID Desc" : paginationInfo.OrderBy;
            }
            else if (status == Status.Approved)   //// for Sending QC, QC=0,  Inventory Grey Fabric Receive //By Rana
            {
                sql += $@"
               With F As (
                    Select KP.GRollID, KJ.KJobCardNo,KP.ProductionDate,FCM.ConceptNo,KP.ProdQty,KP.RollNo,KP.RollQty,KP.RollQtyPcs,
                    FCM.ConceptID, FCM.IsBDS, FCM.GroupConceptNo, KJ.IsSubContact, CTO.ShortName Buyer, ISNULL( CCT.TeamName, 'RND') BuyerTeam
                    FROM {TableNames.RND_KNITTING_PRODUCTION} KP
                    INNER JOIN {TableNames.KNITTING_JOB_CARD_Master} KJ ON KJ.KJobCardMasterID=KP.KJobCardMasterID
                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID=KP.ConceptID
					LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.ConceptID=KP.ConceptID
					LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = KPM.BuyerID
					LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = KPM.BuyerTeamID
                    Where QCPass = 1 AND QCComplete = 0 OR QCFail = 1
                )
				Select *, COUNT(*) Over() TotalRows
                From F
				";
            }
            else if (status == Status.Acknowledge)///For Sended QC, QC=1, Inventory Grey Fabric Receive  //By Rana
            {
                sql += $@"
                With F As (
                    Select KP.GRollID, KJ.KJobCardNo,KP.ProductionDate,FCM.ConceptNo,KP.ProdQty,KP.RollNo,KP.RollQty,KP.RollQtyPcs,KP.ProdQty ProducedQty,
                    FCM.ConceptID, FCM.IsBDS, FCM.GroupConceptNo, KJ.IsSubContact
                    FROM {TableNames.RND_KNITTING_PRODUCTION} KP
                    INNER JOIN {TableNames.KNITTING_JOB_CARD_Master} KJ ON KJ.KJobCardMasterID=KP.KJobCardMasterID
                    LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID=KP.ConceptID
                    Where QCPass = 1 AND QCComplete = 1
                )

                Select *, COUNT(*) Over() TotalRows
                From F";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<KnittingProduction>(sql);
        }

        public async Task<KnittingProduction> GetNewAsync(int KJobCardMasterId, int isBDS, int conceptId, string grpConceptNo)
        {
            string colString = isBDS == 1 ? $@"={conceptId}" : $@" In  (Select ConceptID From FreeConceptMaster Where GroupConceptNo = '{grpConceptNo}')";

            var query =
                $@"
                select KJobCardMasterId,KJobCardNo,C.ConceptId,C.ConceptNo,J.KJobCardQty JobCardQty,J.ProdQty ProducedQty, M.MachineNo, 0 FWidth,
                C.KnittingTypeID,C.ConstructionID,C.TechnicalNameId,Technical.TechnicalName,C.CompositionID,C.GSMID,J.KJobCardQty ProdQty,
				KT.TypeName KnittingType, COM.SegmentValue Composition, GSM.SegmentValue GSM, KPM.GrayFabricOK,MSC.SubClassName,M.GG Gauge,
                M.Dia Dia,Brand.ValueName MCBrand,KU.UnitName,C.ConstructionID,Construction.SegmentValue Construction, J.ProdComplete, C.SubGroupID, ISG.SubGroupName,
                J.GrayGSM ProductionGSM, J.GrayWidth ProductionWidth, J.ActualGreyHeight, J.ActualGreyLength, KU.WeightURL,KU.PrinterName, J.IsSubContact
                FROM {TableNames.KNITTING_JOB_CARD_Master} J
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} C ON C.ConceptID = J.ConceptID
                LEFT JOIN {TableNames.KNITTING_MACHINE} M ON M.KnittingMachineID = J.KnittingMachineID
                LEFT JOIN {TableNames.KNITTING_MACHINE}SubClass MSC ON MSC.SubClassID = M.MachineSubClassID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue Brand ON Brand.ValueID=M.BrandID
                LEFT JOIN {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = J.ContactID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = C.ConstructionID
                LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId=C.TechnicalNameId
				LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = C.KnittingTypeID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COM ON COM.SegmentValueID = C.CompositionID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSM ON GSM.SegmentValueID = C.GSMID
				LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID = J.KPChildID
				LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID = KPC.KPMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = C.SubGroupID
                --LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = J.SubGroupID
                WHERE J.KJobCardMasterID = {KJobCardMasterId};

                --KJobCardChild
                SELECT KJC.*, MachineType = ISNULL(KT.TypeName,''), MCSubClassName = MS.SubClassName, KPM.MCSubClassID,
                Composition.SegmentValue Composition, Gsm.SegmentValue Gsm, 
                ColorName = Case When FCM.SubGroupID = 1 Then Color.SegmentValue Else C1.SegmentValue End, Size = Case When FCM.SubGroupID <> 1 Then CONVERT(varchar(100),FCM.Length) + ' X ' + CONVERT(varchar(100),FCM.Width) ELSE '' END
                FROM {TableNames.KNITTING_JOB_CARD_Child} KJC
                LEFT JOIN {TableNames.KNITTING_JOB_CARD_Master} KM ON KM.KJobCardMasterID = KJC.KJobCardMasterID
                LEFT JOIN {TableNames.Knitting_Plan_Group} KPG ON KPG.GroupID = KM.GroupID
                LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID = KJC.KPChildID
                LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID = KPC.KPMasterID
                LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = KM.MachineKnittingTypeID
                LEFT JOIN {TableNames.KNITTING_MACHINE}SubClass MS ON MS.SubClassID = KPC.MCSubClassID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KPM.ConceptID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = FCM.GSMID
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue C1 ON C1.SegmentValueID = IM.Segment5ValueID
                WHERE KM.KJobCardMasterID = {KJobCardMasterId};

                --FreeConceptChildColor
                ;SELECT CCColorID, FC.ConceptID, ColorID, ColorName, ColorCode
                FROM {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FC
                Where ConceptID {colString};

                --Operators
               ;select E.EmployeeCode id, E.DisplayEmployeeCode +' '+EmployeeName text, E.ProximityCardNo additionalValue
                from {DbNames.EPYSL}..Employee E
                INNER JOIN {DbNames.EPYSL}..EmployeeDesignation D ON D.DesigID=E.DesigID
				INNER JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID= E.DepertmentID
                where (D.Designation like '%operator%' or D.Designation = 'Helper') and ED.DepertmentDescription ='Knitting' ;

                --Shifts
               ;SELECT CAST(ShiftId AS VARCHAR) id, ShortName text, CAST(FromHour AS nvarchar(6)) [desc],CAST(ToHour AS nvarchar(6)) additionalValue
                FROM {DbNames.EPYSL}..ShiftInfo
                Where CompanyID=6
                order by SeqNo;";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KnittingProduction data = records.Read<KnittingProduction>().FirstOrDefault();
                data.KJobCardChilds = records.Read<KJobCardChild>().ToList();
                data.ChildColors = records.Read<FreeConceptChildColor>().ToList();
                data.OperatorList = await records.ReadAsync<Select2OptionModel>();
                data.ShiftList = await records.ReadAsync<Select2OptionModel>();
                data.PShiftList = data.ShiftList;
                data.POperatorList = data.OperatorList;

                _currentTime = Convert.ToDecimal($"{DateTime.Now.Hour}.{DateTime.Now.Minute}");
                data.PShiftId = Convert.ToInt32(data.ShiftList.FirstOrDefault(x => IsInTimeSlot(x)).id);


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

        private bool IsInTimeSlot(Select2OptionModel shift)
        {
            if (Convert.ToDecimal(shift.desc) <= Convert.ToDecimal(shift.additionalValue))
            {
                // Time slot does not wrap around to the next day
                return _currentTime >= Convert.ToDecimal(shift.desc) && _currentTime <= Convert.ToDecimal(shift.additionalValue);
            }
            else
            {
                // Time slot wraps around to the next day
                return _currentTime >= Convert.ToDecimal(shift.desc) || _currentTime <= Convert.ToDecimal(shift.additionalValue);
            }
        }

        public async Task<KnittingProduction> GetNewByKJobCardNo(string kJobCardNo)
        {
            var query =
                $@"SELECT TOP(1)JCK.KJobCardMasterID, JCK.ConceptID, FCM.IsBDS, FCM.GroupConceptNo, JobCardProdComplete = JCK.ProdComplete, JobCardStatus = JS.text, JCK.ProdComplete
                FROM {TableNames.KNITTING_JOB_CARD_Master} JCK
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = JCK.ContactID
                LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID = JCK.KPChildID
                LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID = KPC.KPMasterID
                LEFT JOIN {TableNames.JobCardStatus} JS ON JS.id = JCK.Status
                WHERE KPM.IsConfirm = 1 AND JCK.KJobCardNo = '{kJobCardNo}';
                
                --Childs
                select K.GRollID, K.KJobCardMasterID, K.ProductionWidth, K.ProductionDate, K.RollQty,K.RollQtyPcs, K.ShiftID, K.OperatorID, K.FirstRollCheck, K.RollLength,
                ShiftInfo.ShortName [Shift], E.DisplayEmployeeCode +' '+EmployeeName AS Operator, RollSeqNo, RollNo, K.DateAdded, K.DateUpdated, K.ConceptID, 16 EntityState
                FROM {TableNames.RND_KNITTING_PRODUCTION} K
                LEFT JOIN {DbNames.EPYSL}..ShiftInfo ON ShiftInfo.ShiftId = K.ShiftID
                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = K.OperatorID
                LEFT JOIN {TableNames.KNITTING_JOB_CARD_Master} JCM ON JCM.KJobCardMasterID = K.KJobCardMasterID
                WHERE JCM.KJobCardNo = '{kJobCardNo}';";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KnittingProduction data = records.Read<KnittingProduction>().FirstOrDefault();
                if (data != null)
                {
                    data.Childs = records.Read<KnittingProductionChildBindingModel>().ToList();
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
            //return new KnittingProduction();
        }
        public async Task<KnittingProduction> GetNewAsyncp()
        {
            var query =
                $@"
                 --Operators
               ;select E.EmployeeCode id, E.DisplayEmployeeCode +' ' +EmployeeName text,E.ProximityCardNo [desc], E.ProximityCardNo additionalValue
                from {DbNames.EPYSL}..Employee E
                INNER JOIN {DbNames.EPYSL}..EmployeeDesignation D ON D.DesigID=E.DesigID
				INNER JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID= E.DepertmentID
                where (D.Designation like '%operator%' or D.Designation = 'Helper') and ED.DepertmentDescription ='Knitting';
                 ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                KnittingProduction data = new KnittingProduction();
                data.OperatorList = await records.ReadAsync<Select2OptionModel>();
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

        public async Task<KnittingProduction> GetNewAsyncpid(string pid)
        {
            var sql = $@" ;select E.EmployeeCode id, E.DisplayEmployeeCode +' '+EmployeeName text
                from {DbNames.EPYSL}..Employee E
                INNER JOIN {DbNames.EPYSL}..EmployeeDesignation D ON D.DesigID=E.DesigID
				INNER JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID= E.DepertmentID
                where (D.Designation like '%operator%' or D.Designation = 'Helper') and ED.DepertmentDescription ='Knitting'  and E.ProximityCardNo={pid};
                ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                KnittingProduction data = new KnittingProduction();
                data.OperatorList = await records.ReadAsync<Select2OptionModel>();
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
        public async Task<List<KnittingProduction>> GetByJobCardId(int kJobCardMasterID)
        {
            if (kJobCardMasterID == 0) return new List<KnittingProduction>();
            var sql = $@"SELECT * 
                        FROM {TableNames.RND_KNITTING_PRODUCTION} 
                        WHERE KJobCardMasterID = {kJobCardMasterID};";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<KnittingProduction> datas = records.Read<KnittingProduction>().ToList();
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
        public async Task<KnittingProduction> GetAsync(int KJobCardMasterId, int isBDS, int conceptId, string grpConceptNo)
        {
            string colString = isBDS == 1 ? $@"={conceptId}" : $@" In  (Select ConceptID From FreeConceptMaster Where GroupConceptNo = '{grpConceptNo}')";
            var query =
                $@"
                WITH JOB AS (
		            SELECT TOP(1)j.KJobCardMasterId,KJobCardNo,J.KJobCardQty JobCardQty, J.KJobCardQty ProdQty,
						ProducedQty = Case When J.SubGroupID = 1 Then J.ProdQty Else J.ProdQtyPcs End, KPChildID, j.ConceptID,
						KnittingMachineID,J.ContactID, FCM.SubGroupID, J.GrayGSM ProductionGSM, J.GrayWidth ProductionWidth, J.ActualGreyHeight,
						J.ActualGreyLength, JobCardProdComplete = j.ProdComplete, JobCardStatus = JS.text, J.IsSubContact
						FROM {TableNames.KNITTING_JOB_CARD_Master} J 
                        LEFT JOIN {TableNames.JobCardStatus} JS ON JS.id = j.Status
                        LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = J.ConceptID
                        WHERE J.KJobCardMasterID={KJobCardMasterId}
		            ),
	            PROD AS (
		            SELECT K.KJobCardMasterID, K.ProductionDate, K.ProdComplete
		            FROM {TableNames.RND_KNITTING_PRODUCTION} K WHERE K.KJobCardMasterID={KJobCardMasterId}
		            GROUP BY K.KJobCardMasterID, K.ProductionDate, K.ProdComplete
	            )
	            SELECT JOB.KJobCardMasterId, JOB.KJobCardNo, JOB.JobCardQty, JOB.ProdQty,JOB.ProducedQty, C.ConceptId, C.ConceptNo, M.MachineNo, 0 FWidth,
				C.KnittingTypeID, JOB.SubGroupID, ISG.SubGroupName, C.ConstructionID, C.TechnicalNameId, Technical.TechnicalName,
				C.CompositionID, C.GSMID, KT.TypeName KnittingType, COM.SegmentValue Composition, GSM.SegmentValue GSM, KPM.GrayFabricOK,
				PROD.ProductionDate,PROD.ProdComplete,MSC.SubClassName,M.GG Gauge,M.Dia Dia,Brand.ValueName MCBrand,KU.UnitName, C.ConstructionID,
				Construction.SegmentValue Construction, JOB.ProductionGSM, JOB.ProductionWidth, JOB.ActualGreyHeight,
				JOB.ActualGreyLength, JOB.JobCardProdComplete, JOB.JobCardStatus, JOB.IsSubContact
	            FROM JOB
	            LEFT JOIN PROD ON PROD.KJobCardMasterID = JOB.KJobCardMasterID
	            LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} C ON C.ConceptID = JOB.ConceptID
                LEFT JOIN {TableNames.KNITTING_MACHINE} M ON M.KnittingMachineID = JOB.KnittingMachineID
                LEFT JOIN {TableNames.KNITTING_MACHINE}SubClass MSC ON MSC.SubClassID = M.MachineSubClassID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue Brand ON Brand.ValueID=M.BrandID
                LEFT JOIN {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = JOB.ContactID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Construction ON Construction.SegmentValueID = C.ConstructionID
                LEFT JOIN {TableNames.FabricTechnicalName} Technical ON Technical.TechnicalNameId=C.TechnicalNameId
	            LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = C.KnittingTypeID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COM ON COM.SegmentValueID = C.CompositionID
	            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue GSM ON GSM.SegmentValueID = C.GSMID
	            LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID = JOB.KPChildID
	            LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID = KPC.KPMasterID
				LEFT JOIN {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = JOB.SubGroupID;

                --KJobCardChild
                SELECT KJC.*, MachineType = ISNULL(KT.TypeName,''), MCSubClassName = MS.SubClassName, KPM.MCSubClassID,
                Composition.SegmentValue Composition, Gsm.SegmentValue Gsm, 
                ColorName = Case When FCM.SubGroupID = 1 Then Color.SegmentValue Else C1.SegmentValue End, Size = Case When FCM.SubGroupID <> 1 Then CONVERT(varchar(100),FCM.Length) + ' X ' + CONVERT(varchar(100),FCM.Width) ELSE '' END
                FROM {TableNames.KNITTING_JOB_CARD_Child} KJC
                LEFT JOIN {TableNames.KNITTING_JOB_CARD_Master} KM ON KM.KJobCardMasterID = KJC.KJobCardMasterID
                LEFT JOIN {TableNames.Knitting_Plan_Group} KPG ON KPG.GroupID = KM.GroupID
                LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID = KJC.KPChildID
                LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID = KPC.KPMasterID
                LEFT JOIN {TableNames.KNITTING_MACHINE_TYPE} KT ON KT.TypeID = KM.MachineKnittingTypeID
                LEFT JOIN {TableNames.KNITTING_MACHINE}SubClass MS ON MS.SubClassID = KPC.MCSubClassID
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KPM.ConceptID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = FCM.GSMID
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue C1 ON C1.SegmentValueID = IM.Segment5ValueID
                WHERE KM.KJobCardMasterID = {KJobCardMasterId};

                ----Color
                SELECT CCColorID, FC.ConceptID, ColorID, ColorName, ColorCode
                FROM {TableNames.RND_FREE_CONCEPT_CHILD_COLOR} FC
                Where ConceptID {colString};

                ----Child
                select K.GRollID, K.KJobCardMasterID, K.ProductionWidth, K.ProductionDate, K.RollQty,K.RollQtyPcs, K.ShiftID, K.OperatorID, K.FirstRollCheck, K.RollLength,
				ShiftInfo.ShortName [Shift], E.DisplayEmployeeCode +' '+EmployeeName AS Operator, RollSeqNo, RollNo, K.DateAdded, K.DateUpdated, K.ConceptID, 16 EntityState, K.ITM,
				Composition.SegmentValue Composition, Gsm.SegmentValue Gsm, 
                ColorName = Case When FCM.SubGroupID = 1 Then Color.SegmentValue Else C1.SegmentValue End, Size = Case When FCM.SubGroupID <> 1 Then CONVERT(varchar(100),FCM.Length) + ' X ' + CONVERT(varchar(100),FCM.Width) ELSE '' END
				FROM {TableNames.RND_KNITTING_PRODUCTION} K
				LEFT JOIN {DbNames.EPYSL}..ShiftInfo ON ShiftInfo.ShiftId = K.ShiftID
				LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = K.OperatorID
				LEFT JOIN {TableNames.KNITTING_JOB_CARD_Child} KJC ON KJC.KJobCardChildID = K.KJobCardChildID
				LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID = KJC.KPChildID
				LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID = KPC.KPMasterID
				LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KPM.ConceptID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = FCM.GSMID
				LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue C1 ON C1.SegmentValueID = IM.Segment5ValueID
				where K.KJobCardMasterID = {KJobCardMasterId} AND CHARINDEX('_', K.RollNo) = 0;

                --Operators
               ;select E.EmployeeCode id, E.DisplayEmployeeCode +' '+EmployeeName text, E.ProximityCardNo additionalValue
                from {DbNames.EPYSL}..Employee E
                INNER JOIN {DbNames.EPYSL}..EmployeeDesignation D ON D.DesigID=E.DesigID
				INNER JOIN {DbNames.EPYSL}..EmployeeDepartment ED ON ED.DepertmentID= E.DepertmentID
                where (D.Designation like '%operator%' or D.Designation = 'Helper') and ED.DepertmentDescription ='Knitting';

                --Shifts
                 ;SELECT CAST(ShiftId AS VARCHAR) id, ShortName text, CAST(FromHour AS nvarchar(6)) [desc],CAST(ToHour AS nvarchar(6)) additionalValue
                FROM {DbNames.EPYSL}..ShiftInfo
                Where CompanyID=6
                order by SeqNo;";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                KnittingProduction data = records.Read<KnittingProduction>().FirstOrDefault();
                Guard.Against.NullObject(data);

                data.KJobCardChilds = records.Read<KJobCardChild>().ToList();

                data.ChildColors = records.Read<FreeConceptChildColor>().ToList();
                data.Childs = records.Read<KnittingProductionChildBindingModel>().ToList();

                data.OperatorList = await records.ReadAsync<Select2OptionModel>();
                data.ShiftList = await records.ReadAsync<Select2OptionModel>();
                data.PShiftList = data.ShiftList;
                data.POperatorList = data.OperatorList;

                _currentTime = Convert.ToDecimal($"{DateTime.Now.Hour}.{DateTime.Now.Minute}");
                data.PShiftId = Convert.ToInt32(data.ShiftList.FirstOrDefault(x => IsInTimeSlot(x)).id);

                data.Childs.Where(x => x.DateUpdated == null).ToList().ForEach(c => c.DateUpdated = c.DateAdded);

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

        public async Task<KnittingProduction> GetjobCardMasterNo(string jobCardMasterNo)
        {
            var sql = $@"
            ;Select *,C.IsBDS, C.GroupConceptNo 
            FROM {TableNames.KNITTING_JOB_CARD_Master} J
            LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID = J.KPChildID
            LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} C ON J.ConceptID = C.ConceptID
            LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID = KPC.KPMasterID
            WHERE KPM.IsConfirm=1 AND KJobCardNo = '{jobCardMasterNo}' ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                KnittingProduction data = await records.ReadFirstOrDefaultAsync<KnittingProduction>();
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

        public async Task<List<KnittingProduction>> GetRollAsync(int id)
        {
            var query = $@"SELECT GRollID, ConceptID, RollNo, RollQty,RollQtyPcs, ProdQty, ParentGRollID FROM {TableNames.RND_KNITTING_PRODUCTION} WHERE ParentGRollID = {id} AND InActive = 0";
            return await _service.GetDataAsync<KnittingProduction>(query);
        }
        public async Task<List<KnittingProduction>> GetGRollAsync(int id)
        {
            var query = $@"SELECT GRollID, ConceptID, RollNo, RollQty,RollQtyPcs, ProdQty, ParentGRollID FROM {TableNames.RND_KNITTING_PRODUCTION} WHERE GRollID ={id}  AND InActive = 1";
            return await _service.GetDataAsync<KnittingProduction>(query);
        }

        public async Task<KnittingProduction> GetDetailsAsync(int id)
        {
            var query = $@"Select KP.*, ConceptNo = FCM.GroupConceptNo, IsRollUsed = CASE WHEN ISNULL(B.BChildID,0) > 0 THEN 1 ELSE 0 END
                        FROM {TableNames.RND_KNITTING_PRODUCTION} KP
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KP.ConceptID
                        LEFT JOIN {TableNames.BATCH_CHILD} B ON B.GRollID = KP.GRollID
                        Where KP.GRollID = {id};

                        Select KP.*, FCM.GroupConceptNo ConceptNo 
                        FROM {TableNames.RND_KNITTING_PRODUCTION} KP
                        INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KP.ConceptID
                        Where KP.ParentGRollID = {id};";

            var records = await _connection.QueryMultipleAsync(query);
            Guard.Against.NullObject(records);
            KnittingProduction data = records.Read<KnittingProduction>().FirstOrDefault();
            data.RollChilds = records.Read<KnittingProduction>().ToList();
            return data;
        }
        public async Task<DyeingBatchItemRoll> GetDyingBatchItemRoll(int id)
        {
            var query = $@"SELECT * FROM {TableNames.DYEING_BATCH_ITEM_ROLL} WHERE DBIRollID = {id}";
            DyeingBatchItemRoll record = await _serviceDBIR.GetFirstOrDefaultAsync(query);
            Guard.Against.NullObject(record);
            return record;
        }

        public async Task<List<KnittingProduction>> GetByJobCardAsync(int jobCardId)
        {
            var query = $@"select K.GRollID, K.KJobCardMasterID, K.ProductionWidth, K.ProductionDate, K.RollQty,K.RollQtyPcs, K.ShiftID, K.OperatorID, K.FirstRollCheck, K.RollLength,
                ShiftInfo.ShortName [Shift], E.DisplayEmployeeCode +' '+EmployeeName AS Operator, RollSeqNo, RollNo, K.DateAdded, K.DateUpdated, K.ConceptID, 16 EntityState,
                JobCardProdComplete = JCK.ProdComplete, JCK.Status, K.ProdComplete, K.ITM,
                Composition.SegmentValue Composition, Gsm.SegmentValue Gsm, 
                ColorName = Case When FCM.SubGroupID = 1 Then Color.SegmentValue Else C1.SegmentValue End, Size = Case When FCM.SubGroupID <> 1 Then CONVERT(varchar(100),FCM.Length) + ' X ' + CONVERT(varchar(100),FCM.Width) ELSE '' END
                FROM {TableNames.RND_KNITTING_PRODUCTION} K
                LEFT JOIN {DbNames.EPYSL}..ShiftInfo ON ShiftInfo.ShiftId = K.ShiftID
                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = K.OperatorID
                LEFT JOIN {TableNames.KNITTING_JOB_CARD_Master} JCK ON JCK.KJobCardMasterID = K.KJobCardMasterID
                LEFT JOIN {TableNames.JobCardStatus} JS ON JS.id = JCK.Status
                LEFT JOIN {TableNames.KNITTING_JOB_CARD_Child} KJC ON KJC.KJobCardChildID = K.KJobCardChildID
				LEFT JOIN {TableNames.Knitting_Plan_Child} KPC ON KPC.KPChildID = KJC.KPChildID
				LEFT JOIN {TableNames.Knitting_Plan_Master} KPM ON KPM.KPMasterID = KPC.KPMasterID
				LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = KPM.ConceptID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Composition ON Composition.SegmentValueID = FCM.CompositionID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Gsm ON Gsm.SegmentValueID = FCM.GSMID
				LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FCM.ItemMasterID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue C1 ON C1.SegmentValueID = IM.Segment5ValueID
                where K.KJobCardMasterID = {jobCardId} AND K.ParentGRollID = 0;
            ";
            return await _service.GetDataAsync<KnittingProduction>(query);
        }

        public async Task<List<KnittingProduction>> GetDetailsByParentGRollIdAsync(int parentGRollID)
        {
            var query = $@"Select KP.*, BM.BatchNo, BM.ColorID, DBIRollID = ISNULL(DBIR.DBIRollID,0)
                            FROM {TableNames.RND_KNITTING_PRODUCTION} KP
                            LEFT JOIN {TableNames.BATCH_MASTER} BM ON BM.BatchID = KP.BatchID
                            LEFT JOIN {TableNames.DYEING_BATCH_ITEM_ROLL} DBIR ON DBIR.GRollID = KP.GRollID
                        Where KP.ParentGRollID = {parentGRollID}";
            return await _service.GetDataAsync(query);
        }

        public async Task<List<KnittingProduction>> GetDetailsAsync(IEnumerable<int> ids)
        {
            var query = $@"Select * FROM {TableNames.RND_KNITTING_PRODUCTION} WHERE GRollID In @Ids";
            return await _service.GetDataAsync(query, new { Ids = ids });
        }

        public async Task<List<KnittingProduction>> GetKProductionsByConcept(string conceptNo)
        {
            var query = $@";
                    WITH M AS(
                        SELECT KP.* FROM {TableNames.RND_KNITTING_PRODUCTION} KP
	                    INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FC ON FC.ConceptID = KP.ConceptID
	                    WHERE FC.GroupConceptNo = '{conceptNo}' AND KP.InActive = 0
                    )
                    SELECT M.GRollID, M.KJobCardMasterID, M.ProductionDate, M.ConceptID, M.OperatorID, M.ShiftID, M.RollSeqNo, M.RollNo, M.RollQty, M.RollQtyPcs, M.ProductionGSM, M.ProductionWidth,
                    M.FirstRollCheck, M.FirstRollCheckBy, M.FirstRollCheckDate, M.FirstRollPass, M.SendforQC, M.SendQCDate, M.SendQCBy, M.QCComplete, M.QCCompleteDate, M.QCCompleteBy,
                    M.QCWidth, M.QCGSM, M.QCPass, M.QCPassQty, M.AddedBy, M.UpdatedBy, M.ProdComplete, M.ProdQty, M.Hole, M.Loop, M.SetOff, M.LycraOut, M.LycraDrop, M.OilSpot, M.Slub, M.FlyingDust,
                    M.MissingYarn, M.Knot, M.DropStitch, M.YarnContra, M.NeddleBreakage, M.Defected, M.WrongDesign, M.Patta, M.ShinkerMark, M.NeddleMark, M.EdgeMark, M.WheelFree, M.CountMix,
                    M.ThickAndThin, M.LineStar, M.QCOthers, M.Comment, M.CalculateValue, M.Grade, M.RollLength, M.Hold, M.QCBy, M.QCShiftID, M.BookingID, M.DateAdded, M.DateUpdated,
                    M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.ParentGRollID, M.InActive, M.InActiveBy, M.InActiveDate, M.InActiveReason, M.BatchID
                    FROM M
					Left JOIN {TableNames.BATCH_CHILD} BC ON BC.GRollID = M.GRollID
					where BC.GRollID IS NULL;";
            return await _service.GetDataAsync<KnittingProduction>(query);
        }

        public async Task<List<KnittingProduction>> GetKProductionsByConceptId(int conceptId)
        {
            var query = $@";
                    WITH M AS(
                        SELECT KP.* FROM {TableNames.RND_KNITTING_PRODUCTION} KP WHERE KP.ConceptID = {conceptId} AND KP.InActive = 0
                    )
                    SELECT M.GRollID, M.KJobCardMasterID, M.ProductionDate, M.ConceptID, M.OperatorID, M.ShiftID, M.RollSeqNo, M.RollNo, M.RollQty, M.RollQtyPcs, M.ProductionGSM, M.ProductionWidth,
                    M.FirstRollCheck, M.FirstRollCheckBy, M.FirstRollCheckDate, M.FirstRollPass, M.SendforQC, M.SendQCDate, M.SendQCBy, M.QCComplete, M.QCCompleteDate, M.QCCompleteBy,
                    M.QCWidth, M.QCGSM, M.QCPass, M.QCPassQty, M.AddedBy, M.UpdatedBy, M.ProdComplete, M.ProdQty, M.Hole, M.Loop, M.SetOff, M.LycraOut, M.LycraDrop, M.OilSpot, M.Slub, M.FlyingDust,
                    M.MissingYarn, M.Knot, M.DropStitch, M.YarnContra, M.NeddleBreakage, M.Defected, M.WrongDesign, M.Patta, M.ShinkerMark, M.NeddleMark, M.EdgeMark, M.WheelFree, M.CountMix,
                    M.ThickAndThin, M.LineStar, M.QCOthers, M.Comment, M.CalculateValue, M.Grade, M.RollLength, M.Hold, M.QCBy, M.QCShiftID, M.BookingID, M.DateAdded, M.DateUpdated,
                    M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.ParentGRollID, M.InActive, M.InActiveBy, M.InActiveDate, M.InActiveReason, M.BatchID
                    FROM M
					Left JOIN {TableNames.BATCH_CHILD} BC ON BC.GRollID = M.GRollID
					where BC.GRollID IS NULL;";
            return await _service.GetDataAsync<KnittingProduction>(query);
        }

        public async Task SaveAsync(KnittingProduction entity)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                if (entity.EntityState == EntityState.Added) entity.GRollID = await _service.GetMaxIdAsync(TableNames.RND_KNITTING_PRODUCTION, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                entity.QCPass = true; //false for bulk while use for bulk
                entity.QCPassQty = entity.RollQty; //false for bulk while use for bulk
                entity.QCPassQtyPcs = entity.RollQtyPcs; //false for bulk while use for bulk
                await _service.SaveSingleAsync(entity, transaction);

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
                transaction.Dispose();
                transactionGmt.Dispose();
                _connection.Close();
                _connectionGmt.Close();
            }
        }

        public async Task SaveAsync(List<KnittingProduction> list, int kJobCardMasterID = 0)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                KJobCardMaster jobCard = new KJobCardMaster();
                if (list.Count() == 0)
                {
                    jobCard = new KJobCardMaster
                    {
                        KJobCardMasterID = kJobCardMasterID,
                        ProdQty = 0,
                        ProdQtyPcs = 0
                    };
                }
                else
                {
                    jobCard = new KJobCardMaster
                    {
                        KJobCardMasterID = list.FirstOrDefault().KJobCardMasterID,
                        ProdQty = list.Where(c => !c.InActive).Sum(x => x.RollQty),
                        ProdQtyPcs = list.Where(c => !c.InActive).Sum(x => x.RollQtyPcs)
                    };
                }
                //string query = $@"Update KJobCardMaster Set ProdQty = {jobCard.ProdQty}, ProdQtyPcs = {jobCard.ProdQtyPcs} Where KJobCardMasterID = {jobCard.KJobCardMasterID}";
                //await _connection.ExecuteAsync(query, transaction);

                var query = $@"Update {TableNames.KNITTING_JOB_CARD_Master} Set ProdQty = @ProdQty, ProdQtyPcs = @ProdQtyPcs Where KJobCardMasterID = {jobCard.KJobCardMasterID}";
                await _connection.ExecuteAsync(query, new { jobCard.ProdQty, jobCard.ProdQtyPcs }, transaction);

                var addedList = list.FindAll(x => x.EntityState == EntityState.Added);
                int maxId = await _service.GetMaxIdAsync(TableNames.RND_KNITTING_PRODUCTION, addedList.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                int maxDyeingItemRoll = await _service.GetMaxIdAsync(TableNames.DYEING_BATCH_ITEM_ROLL, addedList.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                List<DyeingBatchItemRoll> dbItemRolls = new List<DyeingBatchItemRoll>();
                foreach (var item in addedList)
                {
                    item.GRollID = maxId++;

                    if (item.IsSaveDyeingBatchItemRoll)
                    {
                        DyeingBatchItemRoll dbItemRoll = new DyeingBatchItemRoll();
                        dbItemRoll = CommonFunction.DeepClone(item.DyeingBatchRollItem);
                        dbItemRoll.DBIRollID = maxDyeingItemRoll++;
                        dbItemRoll.GRollID = item.GRollID;
                        dbItemRoll.ParentFRollID = item.DyeingBatchRollItem.DBIRollID;
                        dbItemRoll.InActive = false;
                        dbItemRoll.RollQty = item.RollQty;
                        dbItemRoll.RollQtyPcs = item.RollQtyPcs;
                        dbItemRoll.FinishRollQty = item.RollQty;
                        dbItemRoll.FinishRollQtyPcs = item.RollQtyPcs;
                        dbItemRoll.EntityState = EntityState.Added;
                        dbItemRolls.Add(dbItemRoll);
                    }
                }
                await _service.SaveAsync(list, transaction);
                if (dbItemRolls.Count() > 0)
                {
                    DyeingBatchItemRoll masterItemRoll = addedList.First().DyeingBatchRollItem;
                    masterItemRoll.InActive = true;
                    masterItemRoll.InActiveBy = addedList.First().AddedBy;
                    masterItemRoll.InActiveDate = DateTime.Now;
                    masterItemRoll.EntityState = EntityState.Modified;
                    dbItemRolls.Add(masterItemRoll);
                    await _service.SaveAsync(dbItemRolls, transaction);
                }
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
                transaction.Dispose();
                transactionGmt.Dispose();
                _connection.Close();
                _connectionGmt.Close();
            }
        }

        public async Task<int> UpdateAsync(KnittingProduction knittingProduction)
        {
            var query = $@"Update {TableNames.RND_KNITTING_PRODUCTION} Set QCComplete = @QCComplete, QCCompleteBy = @QCCompleteBy, QCCompleteDate = @QCCompleteDate Where GRollID = {knittingProduction.GRollID}";
            return await _service.ExecuteAsync(query, new { knittingProduction.QCComplete, knittingProduction.QCCompleteBy, knittingProduction.QCCompleteDate });
        }

        public async Task UpdateJobCardAsync(int KJobCardMasterID)
        {
            await _service.ExecuteAsync(SPNames.spUpdateJobCardProductionQty, new { KJobCardMasterID = KJobCardMasterID }, 30, CommandType.StoredProcedure);
        }
        public async Task UpdateBDSTNA_KnittingPlanAsync(int KJobCardMasterID)
        {
            await _service.ExecuteAsync(SPNames.spUpdateBDSTNA_KnittingPlan, new { KJobCardMasterID = KJobCardMasterID }, 30, CommandType.StoredProcedure);
        }
    }
}
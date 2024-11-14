(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, tblMasterId, tblChildId, $modalPlanningEl, $tblStockInfoEl, tblStockInfoId;
    var status;
    var pageIdWithHash;
    var masterData;
    var isYD = false, isKnitting = false;
    var selectedIndex;

    var _childRackBins = [];
    var _selectedKSCReqChildID = 0;
    var _maxKSCICRBId = 999;

    var _paramType = {
        SCYDYarnReq: 0,
        SCYDYarnReqApprove: 1
    }
    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $modalPlanningEl = $("#modalPlanning" + pageId);
        tblStockInfoId = pageConstants.STOCK_INFO_PREFIX + pageId;
        pageIdWithHash = "#" + pageId;
        menuType = localStorage.getItem("SCYDYarnReqPage");
        menuType = parseInt(menuType);
        debugger;
        $divDetailsEl.find('#Req-Complete').prop('checked', true);
        if (menuType == _paramType.SCYDYarnReq) {
            $toolbarEl.find("#btnPending").show();
            $toolbarEl.find("#btnDraft").show();
            $toolbarEl.find("#btnPendingApproval").show();
            $toolbarEl.find("#btnList").show();
            $toolbarEl.find("#btnRejectList").show();
            $toolbarEl.find("#btnPendingGPList").hide();
            $toolbarEl.find("#btnApprovedGPList").show();

            toggleActiveToolbarBtn($(pageIdWithHash).find("#btnPending"), $toolbarEl);
            status = statusConstants.PENDING;
            $divDetailsEl.find("#btnSave").show();
            $divDetailsEl.find("#btnSaveAndSend").show();
            $divDetailsEl.find("#btnApprove").hide();
            $divDetailsEl.find("#btnReject").hide();
            $divDetailsEl.find("#btnGPApprove").hide();
            $divDetailsEl.find(".editField").prop("disabled", false);
            initMasterTable();

            $toolbarEl.find("#btnPending").on("click", function (e) {
                $toolbarEl.find("#btnExcelReport").hide();

                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.PENDING;
                $divDetailsEl.find("#btnSave").show();
                $divDetailsEl.find("#btnSaveAndSend").show();
                $divDetailsEl.find(".editField").prop("disabled", false);
                initMasterTable();
            });
            $toolbarEl.find("#btnDraft").on("click", function (e) {
                $toolbarEl.find("#btnExcelReport").show();

                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.DRAFT;
                $divDetailsEl.find("#btnSave").show();
                $divDetailsEl.find("#btnSaveAndSend").show();
                $divDetailsEl.find(".editField").prop("disabled", false);
                initMasterTable();
            });
            $toolbarEl.find("#btnPendingApproval").on("click", function (e) {
                $toolbarEl.find("#btnExcelReport").hide();

                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.PROPOSED_FOR_APPROVAL;
                $divDetailsEl.find("#btnSave").hide();
                $divDetailsEl.find("#btnSaveAndSend").hide();
                $divDetailsEl.find(".editField").prop("disabled", true);
                initMasterTable();
            });
            $toolbarEl.find("#btnList").on("click", function (e) {
                $toolbarEl.find("#btnExcelReport").hide();

                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.APPROVED;
                $divDetailsEl.find("#btnSave").hide();
                $divDetailsEl.find("#btnSaveAndSend").hide();
                $divDetailsEl.find(".editField").prop("disabled", true);
                initMasterTable();
            });
            $toolbarEl.find("#btnRejectList").on("click", function (e) {
                $toolbarEl.find("#btnExcelReport").hide();

                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.REJECT;
                $divDetailsEl.find("#btnSave").hide();
                $divDetailsEl.find("#btnSaveAndSend").hide();
                $divDetailsEl.find(".editField").prop("disabled", true);
                initMasterTable();
            });
            $toolbarEl.find("#btnPendingGPList").on("click", function (e) {
                $toolbarEl.find("#btnExcelReport").hide();

                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.APPROVED;
                $divDetailsEl.find("#btnSave").hide();
                $divDetailsEl.find("#btnSaveAndSend").hide();
                $divDetailsEl.find(".editField").prop("disabled", true);
                initMasterTable();
            });
            $toolbarEl.find("#btnApprovedGPList").on("click", function (e) {
                $toolbarEl.find("#btnExcelReport").hide();

                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.APPROVED2;
                $divDetailsEl.find("#btnSave").hide();
                $divDetailsEl.find("#btnSaveAndSend").hide();
                $divDetailsEl.find(".editField").prop("disabled", true);
                initMasterTable();
            });
            $formEl.find("#btnSave").click(function (e) {
                e.preventDefault();
                save(false, false, false, '', false);
            });
            $formEl.find("#btnSaveAndSend").click(function (e) {
                e.preventDefault();
                save(true, false, false, '', false);
            });

            $toolbarEl.find("#btnExcelReport").click(function () {
                ch_generateAndExportExcel(pageId, 2, null);
            });
        }
        else if (menuType == _paramType.SCYDYarnReqApprove) {
            $toolbarEl.find("#btnPending").hide();
            $toolbarEl.find("#btnDraft").hide();
            $toolbarEl.find("#btnPendingApproval").show();
            $toolbarEl.find("#btnList").show();
            $toolbarEl.find("#btnRejectList").show();
            $toolbarEl.find("#btnPendingGPList").show();
            $toolbarEl.find("#btnApprovedGPList").show();

            toggleActiveToolbarBtn($(pageIdWithHash).find("#btnPendingApproval"), $toolbarEl);
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            $divDetailsEl.find("#btnSave").hide();
            $divDetailsEl.find("#btnSaveAndSend").hide();
            $divDetailsEl.find("#btnApprove").show();
            $divDetailsEl.find("#btnReject").show();
            $divDetailsEl.find("#btnGPApprove").hide();
            $divDetailsEl.find(".editField").prop("disabled", true);//--

            initMasterTable();

            $toolbarEl.find("#btnPendingApproval").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.PROPOSED_FOR_APPROVAL;
                $divDetailsEl.find("#btnApprove").show();
                $divDetailsEl.find("#btnReject").show();
                $divDetailsEl.find("#btnGPApprove").hide();
                initMasterTable();
            });
            $toolbarEl.find("#btnList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.APPROVED;
                $divDetailsEl.find("#btnApprove").hide();
                $divDetailsEl.find("#btnReject").hide();
                $divDetailsEl.find("#btnGPApprove").hide();
                initMasterTable();
            });
            $toolbarEl.find("#btnRejectList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.REJECT;
                $divDetailsEl.find("#btnApprove").hide();
                $divDetailsEl.find("#btnReject").hide();
                $divDetailsEl.find("#btnGPApprove").hide();
                initMasterTable();
            });
            $toolbarEl.find("#btnPendingGPList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.APPROVED;
                $divDetailsEl.find("#btnApprove").hide();
                $divDetailsEl.find("#btnReject").hide();
                $divDetailsEl.find("#btnGPApprove").show();
                initMasterTable();
            });
            $toolbarEl.find("#btnApprovedGPList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.APPROVED2;
                $divDetailsEl.find("#btnApprove").hide();
                $divDetailsEl.find("#btnReject").hide();
                $divDetailsEl.find("#btnGPApprove").hide();
                initMasterTable();
            });
            $formEl.find("#btnApprove").click(function (e) {
                e.preventDefault();
                save(false, true, false, '', false);
            });
            $formEl.find("#btnReject").click(function (e) {
                e.preventDefault();
                bootbox.prompt("Are you sure you want to reject this?", function (result) {
                    if (!result) {
                        return toastr.error("Reject reason is required.");
                    }

                    save(false, false, true, result, false);
                });

            });
            $formEl.find("#btnGPApprove").click(function (e) {
                e.preventDefault();
                save(false, false, false, '', true);
            });
        }
        $toolbarEl.find("#btnRefreshList").on("click", function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
        });
        $formEl.find("#btnCancel").on("click", backToList);
        $formEl.find("#btnOk").click(function (e) {

            var hasErrorRack = false;
            if (_selectedKSCReqChildID > 0) {
                var rackList = DeepClone($tblStockInfoEl.getCurrentViewRecords());
                var childList = masterData.Childs;//DeepClone($tblChildEl.getCurrentViewRecords());

                var indexF = childList.findIndex(x => x.KSCReqChildID == _selectedKSCReqChildID);
                if (indexF > -1) {
                    var totalIssueQty = 0;
                    var totalIssueQtyCone = 0;
                    var totalIssueQtyCarton = 0;

                    for (var iRack = 0; iRack < rackList.length; iRack++) {
                        var rack = rackList[iRack];
                        /*
                        if (parseFloat(rack.IssueCartoon) > parseFloat(rack.NoOfCartoon)) {
                            hasErrorRack = true;
                            toastr.error("Issue cartoon (" + rack.IssueCartoon + ") cannot be greater then no of cartoon (" + rack.NoOfCartoon + ")");
                            break;
                        }
                        if (parseFloat(rack.IssueQtyCone) > parseFloat(rack.NoOfCone)) {
                            hasErrorRack = true;
                            toastr.error("Issue cone (" + rack.IssueQtyCone + ") cannot be greater then no of cone (" + rack.NoOfCone + ")");
                            break;
                        }
                        if (parseFloat(rack.IssueQtyKg) > parseFloat(rack.ReceiveQty)) {
                            hasErrorRack = true;
                            toastr.error("Issue qty (" + rack.IssueQtyKg + ") cannot be greater than stock qty (" + rack.ReceiveQty + ")");
                            break;
                        }*/

                        totalIssueQty += rack.IssueQtyKg;
                        totalIssueQtyCone += rack.IssueQtyCone;
                        totalIssueQtyCarton += rack.IssueCartoon;
                    }

                    if (hasErrorRack) return false;

                    for (var iRack = 0; iRack < rackList.length; iRack++) {
                        var rack = rackList[iRack];

                        //-------------------------- START Add data to _childRackBins --------------------------
                        var indexFRB = _childRackBins.findIndex(x => x.KSCReqChildID == _selectedKSCReqChildID);
                        if (indexFRB == -1) {
                            _childRackBins.push({
                                KSCReqChildID: _selectedKSCReqChildID,
                                ChildRackBins: []
                            });
                            indexFRB = _childRackBins.findIndex(x => x.KSCReqChildID == _selectedKSCReqChildID);
                            if (indexFRB > -1) {
                                if (rack.KSCICRBId == 0) rack.KSCICRBId = _maxKSCICRBId++;
                                _childRackBins[indexFRB].ChildRackBins.push(rack);
                            }
                        } else {
                            var indexC = _childRackBins[indexFRB].ChildRackBins.findIndex(x => x.ChildRackBinID == rack.ChildRackBinID);
                            if (indexC == -1) {
                                if (rack.KSCICRBId == 0) rack.KSCICRBId = _maxKSCICRBId++;
                                _childRackBins[indexFRB].ChildRackBins.push(rack);
                            } else {
                                _childRackBins[indexFRB].ChildRackBins[indexC] = rack;
                            }
                        }
                        //-------------------------- END Add data to _childRackBins --------------------------

                    }
                    if (!hasErrorRack) {
                        childList[indexF].IssueQty = totalIssueQty;
                        childList[indexF].IssueQtyCone = totalIssueQtyCone;
                        childList[indexF].IssueQtyCarton = totalIssueQtyCarton;

                        //var childObj = DeepClone(childList[indexF]);
                        //masterData.Childs.splice(index, 1);
                        //$tblChildEl.bootstrapTable('load', childList);
                        initChildTable(childList);
                        //$tblChildEl.updateRow(indexF, childObj);
                    }
                }
            }
            if (!hasErrorRack) {
                $modalPlanningEl.modal('hide');
            }


        });

    });

    function initMasterTable() {
        var commandItems = [];
        if (status == statusConstants.PENDING) {
            commandItems = [
                { type: 'Edit', title: 'Edit this row', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } },
                { type: 'MRS Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'MOU, YD Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
            ];
        }
        else if (status == statusConstants.DRAFT) {
            commandItems = [
                { type: 'Edit', title: 'Edit this row', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'MRS Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'MOU, YD Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'MIN Subcontract', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'Delivery Challan', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'Gate Pass', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
            ];
        }
        else {
            commandItems = [
                { type: 'Edit', title: 'Edit this row', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                { type: 'MRS Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'MOU, YD Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'MIN Subcontract', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'Delivery Challan', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'Gate Pass', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
            ];
        }



        var columns = [
            {
                headerText: 'Action', textAlign: 'Center', width: ch_setActionCommandCellWidth(commandItems), commands: commandItems
            },
            {
                field: 'KSCIssueNo', headerText: 'Issue No', visible: status !== statusConstants.PENDING
            },
            {
                field: 'ChallanNo', headerText: 'Challan No', visible: status !== statusConstants.PENDING
            },
            {
                field: 'GPNo', headerText: 'GP No', visible: status !== statusConstants.PENDING
            },
            {
                field: 'KSCIssueDate', headerText: 'Issue Date', type: 'date', format: _ch_date_format_1, visible: status !== statusConstants.PENDING
            },
            {
                field: 'KSCReqNo', headerText: 'Requisition No'
            },
            {
                field: 'ReqType', headerText: 'Req Type'
            },
            {
                field: 'ProgramName', headerText: 'Program'
            },
            {
                field: 'KSCNo', headerText: 'KSC No/YD Booking No'
            },
            {
                field: 'KSCReqDate', headerText: 'Requisition Date', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'ConceptNo', headerText: 'Concept No / Booking No.'
            },
            {
                field: 'ReqByUser', headerText: 'Requisition By'
            },
            {
                field: 'Company', headerText: 'Company'
            },
            {
                field: 'KSCUnit', headerText: 'KSCUnit'
            },
            {
                field: 'ReqQty', headerText: 'Req Qty', visible: status == statusConstants.PENDING || status == statusConstants.DRAFT
            },
            /*{
                field: 'SubGroupName', headerText: 'SubGroup Name'
            },
            {
                field: 'KnittingType', headerText: 'Machine Type'
            },
            {
                field: 'TechnicalName', headerText: 'Technical Name'
            },
            {
                field: 'Composition', headerText: 'Composition'
            },
            {
                field: 'Gsm', headerText: 'Gsm'
            }*/

        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            allowFiltering: true,
            apiEndPoint: `/api/ksc-issue/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }
    function setPopupYarnCategoryAndReqQty(yarnCategory, reqQty) {
        $formEl.find(".spnYarnCateogy").text(yarnCategory + " - Req Qty : ");
        $formEl.find(".spnReqQty").text(reqQty);
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'MRS Report') {
            if (args.rowData.ReqType == "SC") {
                window.open(`/reports/InlinePdfView?ReportName=DailySubContractYarnRequisitionSlip.rdl&RequisitionID=${args.rowData.KSCReqMasterID}`, '_blank');
            } else if (args.rowData.ReqType == "YD") {
                window.open(`/reports/InlinePdfView?ReportName=YDYarnRequisitionSlip.rdl&RequisitionID=${args.rowData.KSCReqMasterID}`, '_blank');
            }
        }
        else if (args.commandColumn.type == 'MOU, YD Booking Report') {
            if (args.rowData.ReqType == "SC") {
                window.open(`/reports/InlinePdfView?ReportName=KnittingSubContract.rdl&KSCNo=${args.rowData.KSCNo}`, '_blank');
            } else if (args.rowData.ReqType == "YD") {
                window.open(`/reports/InlinePdfView?ReportName=YarnDyedBooking.rdl&YDBookingNo=${args.rowData.KSCNo}`, '_blank');
            }
        }
        else if (args.commandColumn.type == 'SC MOU') {
            window.open(`/reports/InlinePdfView?ReportName=KnittingSubContract.rdl&KSCNo=${args.rowData.KSCNo}`, '_blank');
        }
        else if (args.commandColumn.type == 'MIN Subcontract') {
            window.open(`/reports/InlinePdfView?ReportName=MINSubContract.rdl&IssueNo=${args.rowData.KSCIssueNo}`, '_blank');
        }
        else if (args.commandColumn.type == 'Delivery Challan') {
            window.open(`/reports/InlinePdfView?ReportName=SubContractDC.rdl&ChallanNo=${args.rowData.ChallanNo}`, '_blank');
        }
        else if (args.commandColumn.type == 'Gate Pass') {
            window.open(`/reports/InlinePdfView?ReportName=SubContractGP.rdl&GPNo=${args.rowData.GPNo}`, '_blank');
        }
        else if (status == statusConstants.PENDING) {
            //ProgramName
            if (args.rowData.ProgramName == "YD") {
                isYD = true;
            }
            else {
                isKnitting = true;
            }
            getNew(args.rowData.KSCReqMasterID, args.rowData.ReqType, args.rowData.ProgramName);
        }
        else {
            if (args.rowData.ProgramName == "YD") {
                isYD = true;
            }
            else {
                isKnitting = true;
            }
            getDetails(args.rowData.KSCIssueMasterID, args.rowData.ReqType, args.rowData.ProgramName);
        }
    }

    function initChildTable(data) {
        if ($tblChildEl) $tblChildEl.destroy();

        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            allowPaging: false,
            editSettings: {
                allowEditing: true,
                allowAdding: true,
                allowDeleting: true,
                mode: "Normal",
                showDeleteConfirmDialog: true
            },
            columns: [
                {
                    headerText: 'Command', visible: status == statusConstants.PENDING || status == statusConstants.DRAFT, width: 100, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } }//,
                        //{ type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        //{ type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        //{ type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
                { field: 'KSCIssueChildID', visible: false, isPrimaryKey: true },
                { field: 'YBChildItemID', visible: false },
                {
                    field: 'YarnCategory', width: 140, headerText: 'Yarn Details', allowEditing: false
                },
                {
                    field: 'YarnCount', headerText: 'Yarn Count', allowEditing: false
                },
                { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false },
                { field: 'YarnLotNo', headerText: 'Lot No', allowEditing: false },
                {
                    field: 'SpinnerID', headerText: 'Spinner', visible: false
                },
                {
                    field: 'SpinnerName', headerText: 'Spinner', allowEditing: false
                },
                //{
                //    field: 'StitchLength', headerText: 'Stitch Length', allowEditing: false
                //},
                {
                    field: 'ReqQty', headerText: 'Requisition Qty(Kg)', allowEditing: false
                },
                {
                    field: 'ReqCone', headerText: 'Req Cone(PCS)', allowEditing: false
                },
                {
                    field: 'AllocatedQty', headerText: 'Allocated Qty', allowEditing: false
                },
                {
                    field: 'IssueQty', headerText: 'Issue Qty', allowEditing: false, textAlign: 'center', width: 85, valueAccessor: displayYarnAllocation
                },
                {
                    field: 'IssueQtyCarton', headerText: 'Issue Qty(Crtn)', allowEditing: false, textAlign: 'center', width: 85, valueAccessor: displayYarnAllocation
                },
                {
                    field: 'IssueQtyCone', headerText: 'Issue Cone Qty(Cone)', allowEditing: false, textAlign: 'center', width: 85, valueAccessor: displayYarnAllocation
                },
                { field: 'Remarks', headerText: 'Remarks', allowEditing: status == statusConstants.PENDING }
            ],
            recordClick: function (args) {

                setPopupYarnCategoryAndReqQty(args.rowData.YarnCategory, args.rowData.ReqQty);

                if (args.column && args.column.field == "IssueQty" || args.column.field == "IssueQtyCarton" || args.column.field == "IssueQtyCone") {

                    _selectedKSCReqChildID = args.rowData.KSCReqChildID;
                    var childRackBinsExisting = args.rowData.ChildRackBins;
                    //selectedIndex = index;
                    //var ItemMasterID = 0;
                    var LotNo = args.rowData.YarnLotNo;
                    var PhysicalCount = args.rowData.PhysicalCount;
                    var ItemMasterID = getDefaultValueWhenInvalidN(args.rowData.ItemMasterID);
                    var SuppilerId = getDefaultValueWhenInvalidN(args.rowData.SuppilerId);
                    var SpinnerId = getDefaultValueWhenInvalidN(args.rowData.SpinnerID);
                    var YBChildItemID = getDefaultValueWhenInvalidN(args.rowData.YBChildItemID);
                    var menuName = masterData.ReqType //"RnDIssue";

                    LotNo = getDefaultValueForAPICall(replaceInvalidChar(LotNo));
                    PhysicalCount = getDefaultValueForAPICall(replaceInvalidChar(PhysicalCount));
                    var shadeCode = getDefaultValueForAPICall(replaceInvalidChar(args.rowData.ShadeCode));

                    var stockTypeId = 0;
                    var stockFromTableId = 0;
                    var stockFromPKId = 0;
                    if (args.rowData.StockTypeId != null && typeof args.rowData.StockTypeId != "undefined")
                        stockTypeId = getDefaultValueWhenInvalidN(args.rowData.StockTypeId);
                    if (args.rowData.StockFromTableId != null && typeof args.rowData.StockFromTableId != "undefined")
                        stockFromTableId = getDefaultValueWhenInvalidN(args.rowData.StockFromTableId);
                    if (args.rowData.StockFromPKId != null && typeof args.rowData.StockFromPKId != "undefined")
                        stockFromPKId = getDefaultValueWhenInvalidN(args.rowData.StockFromPKId);

                    var isFromDraft = status == statusConstants.COMPLETED || status == statusConstants.AWAITING_PROPOSE;
                    var childRackBinID = args.rowData.ChildRackBins.length > 0 ? args.rowData.ChildRackBins[0].ChildRackBinID : 0;
                    var issuedQtySt = replaceInvalidChar(getDefaultValueWhenInvalidN_Float(args.rowData.IssueQty).toString());

                    if (masterData.ProgramName.toUpperCase() == "BULK") {
                        url = `/api/yarn-rnd-issues/GetAllocatedStockForIssue/${YBChildItemID}/${LotNo}/${PhysicalCount}/${ItemMasterID}/${SuppilerId}/${SpinnerId}/${shadeCode}/${menuName}/${stockTypeId}/${stockFromTableId}/${stockFromPKId}/${isFromDraft}/${childRackBinID}/${issuedQtySt}`;
                    } else if (masterData.ProgramName.toUpperCase() == "RND") {
                        var url = `/api/yarn-rnd-issues/GetStockForIssue/${LotNo}/${PhysicalCount}/${ItemMasterID}/${SpinnerId}/${menuName}/${stockTypeId}/${stockFromTableId}/${stockFromPKId}/${isFromDraft}/${childRackBinID}/${issuedQtySt}`;
                    }
                    //var data = [];

                    axios.get(url)
                        .then(function (response) {
                            var list = response.data;
                            if (list.length == 0) {
                                toastr.error("Item not found.");
                                return false;
                            }

                            //ChildRackBins
                            var issueCartoon = 0;
                            var issueQtyCone = 0;
                            var issueQtyKg = 0;

                            var indexF = _childRackBins.findIndex(x => x.KSCReqChildID == _selectedKSCReqChildID);
                            if (indexF > -1) {
                                var childRackBins = _childRackBins[indexF].ChildRackBins;
                                list.map(x => {

                                    var indexC = childRackBins.findIndex(y => y.ChildRackBinID == x.ChildRackBinID);
                                    if (indexC > -1) {
                                        var crbObj = childRackBins[indexC];

                                        //------------Add Existing Qty-------------------------------
                                        issueCartoon = 0;
                                        issueQtyCone = 0;
                                        issueQtyKg = 0;
                                        if (childRackBinsExisting == null) {
                                            childRackBinsExisting = [];
                                        }
                                        childRackBinsExisting.filter(e => e.ChildRackBinID == x.ChildRackBinID).map(c => {
                                            issueCartoon += isNaN(c.IssueCartoon) ? 0 : c.IssueCartoon;
                                            issueQtyCone += isNaN(c.IssueQtyCone) ? 0 : c.IssueQtyCone;
                                            issueQtyKg += isNaN(c.IssueQtyKg) ? 0 : c.IssueQtyKg;
                                        });

                                        //x.NoOfCartoon = x.NoOfCartoon + issueCartoon;
                                        //x.NoOfCone = x.NoOfCone + issueQtyCone;
                                        //x.ReceiveQty = x.ReceiveQty + issueQtyKg;
                                        //--------------------------------------------------------------

                                        x.KSCICRBId = crbObj.KSCICRBId;
                                        x.IssueCartoon = crbObj.IssueCartoon;
                                        x.IssueQtyCone = crbObj.IssueQtyCone;
                                        x.IssueQtyKg = crbObj.IssueQtyKg;
                                    }
                                });
                            }
                            list.filter(x => x.KSCICRBId == 0).map(y => {
                                y.KSCICRBId = _maxKSCICRBId++;
                            });

                            initStockInfo(list);
                            $modalPlanningEl.modal('show');
                        })
                        .catch(showResponseError);

                }
            },
            actionBegin: function (args) {
                if (args.requestType === "add") {

                }
                else if (args.requestType === "save") {

                }
                else if (args.requestType.toLowerCase() === "delete") {

                    //for (var i = 0; i < masterData.SummaryChilds.length; i++) {
                    //    if (masterData.Childs.filter(y => y.YBookingNo == masterData.SummaryChilds[i].YBookingNo).length <= 1) {
                    //        const indexToDelete = masterData.SummaryChilds.findIndex(item => item.YBookingNo === masterData.SummaryChilds[i].YBookingNo);

                    //        if (indexToDelete !== -1) {
                    //            masterData.SummaryChilds.splice(indexToDelete, 1);
                    //            initSummaryChild(masterData.SummaryChilds);
                    //        }
                    //    }
                    //}

                }
            },
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }
    function reset() {
        _childRackBins = [];
        _selectedKSCReqChildID = 0;
    }
    function displayYarnAllocation(field, data, column) {
        column.disableHtmlEncode = false;
        return `<a class="btn btn-xs btn-default" href="javascript:void(0)" title="Allocation Qty">
                                     ${data[field] ? data[field] : 0}
                                </a>`;
    }
    function initStockInfo(data) {
        if ($tblStockInfoEl) $tblStockInfoEl.destroy();

        $tblStockInfoEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            allowPaging: false,
            editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                //{ field: 'KSCIssueChildID', isPrimaryKey: true, visible: true, width: 10 },
                { field: 'ChildRackBinID', isPrimaryKey: true, visible: false, width: 10 },
                { field: 'YarnStockSetId', headerText: 'YarnStockSetId', visible: false, allowEditing: false, width: 10 },
                { field: 'LocationName', headerText: 'Location', allowEditing: false, width: 80 },
                { field: 'RackNo', headerText: 'Rack', allowEditing: false, width: 80 },
                { field: 'YarnControlNo', headerText: 'Control No', allowEditing: false, width: 80 },
                { field: 'AvgCartoonWeight', headerText: 'Avg. Cartoon Weight', allowEditing: false, width: 80 },
                { field: 'RackQty', headerText: 'Rack Qty', allowEditing: false, width: 80 },
                { field: 'LotNo', headerText: 'Lot No', allowEditing: false, width: 80 },
                { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false, width: 80 },
                { field: 'NoOfCartoon', headerText: 'No of Cartoon', allowEditing: false, width: 80 },
                { field: 'NoOfCone', headerText: 'No of Cone', allowEditing: false, width: 80 },
                { field: 'ReceiveQty', headerText: 'Allocated Qty', allowEditing: false, width: 80 },
                { field: 'IssueQtyKg', headerText: 'Issue Qty (Kg)', allowEditing: true, width: 80, edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } } },
                { field: 'IssueCartoon', headerText: 'Issue Cartoon', allowEditing: true, width: 80, edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } } },
                { field: 'IssueQtyCone', headerText: 'Issue Cone', allowEditing: true, width: 80, edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } } }
            ],
            actionBegin: function (args) {
                if (args.requestType === "save") {

                    //if (args.data.IssueQtyKg > args.data.ReceiveQty) {
                    //    toastr.error("Issue Qty (" + args.data.IssueQtyKg + ") cannot be greater than Stock Qty (" + args.data.ReceiveQty + ")");
                    //    args.data.IssueQtyKg = 0;
                    //    args.data.IssueCartoon = 0;
                    //    args.data.IssueQtyCone = 0;
                    //    return;
                    //}
                    //else
                    if (args.data.IssueQtyKg > args.data.RackQty) {
                        toastr.error("Issue Qty (" + args.data.IssueQtyKg + ") cannot be greater than Rack Qty (" + args.data.RackQty + ")");
                        args.data.IssueQtyKg = 0;
                        args.data.IssueCartoon = 0;
                        args.data.IssueQtyCone = 0;
                        return;
                    }
                    /*var indexF = _childRackBins.findIndex(x => x.KSCReqChildID == _selectedKSCReqChildID);
                    if (indexF == -1) {
                        _childRackBins.push({
                            KSCReqChildID: _selectedKSCReqChildID,
                            ChildRackBins: []
                        });
                        indexF = _childRackBins.findIndex(x => x.KSCReqChildID == _selectedKSCReqChildID);
                        if (indexF > -1) {
                            if (args.data.KSCICRBId == 0) args.data.KSCICRBId = _maxKSCICRBId++;
                            _childRackBins[indexF].ChildRackBins.push(args.data);
                        }
                    } else {
                        var indexC = _childRackBins[indexF].ChildRackBins.findIndex(x => x.ChildRackBinID == args.data.ChildRackBinID);
                        if (indexC == -1) {
                            if (args.data.KSCICRBId == 0) args.data.KSCICRBId = _maxKSCICRBId++;
                            _childRackBins[indexF].ChildRackBins.push(args.data);
                        } else {
                            _childRackBins[indexF].ChildRackBins[indexC] = args.data;
                        }
                    }*/
                }
            },
        });
        $tblStockInfoEl.refreshColumns;
        $tblStockInfoEl.appendTo(tblStockInfoId);
    }
    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#KSCIssueMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getNew(kscReqMasterID, reqType, programName) {
        if (programName == 'R&D') programName = 'RnD';
        resetGlobals();
        axios.get(`/api/ksc-issue/new/${kscReqMasterID}/${reqType}/${programName}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ReqType = reqType;
                masterData.ProgramName = programName;
                masterData.KSCIssueDate = formatDateToDefault(masterData.KSCIssueDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate);
                masterData.GPDate = formatDateToDefault(masterData.GPDate);
                masterData.KSCReqDate = formatDateToDefault(masterData.KSCReqDate);
                if (isTextNewValue(masterData.GPNo)) masterData.GPNo = "";

                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
                $tblChildEl.refresh();
                $divDetailsEl.find('#Req-Complete').prop('checked', true);
                //$formEl.find("#btnSave").show();
            })
            .catch(showResponseError);
    }

    function getDetails(id, reqType, programName) {
        if (programName == 'R&D') {
            programName = 'RnD'
        }
        resetGlobals();
        axios.get(`/api/ksc-issue/${id}/${reqType}/${programName}`)
            .then(function (response) {

                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ReqType = reqType;
                masterData.ProgramName = programName;
                masterData.KSCIssueDate = formatDateToDefault(masterData.KSCIssueDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate);
                masterData.GPDate = formatDateToDefault(masterData.GPDate);
                masterData.KSCReqDate = formatDateToDefault(masterData.KSCReqDate);
                //if (isTextNewValue(masterData.GPNo)) masterData.GPNo = "";
                setFormData($formEl, masterData);

                masterData.Childs.map(x => {
                    _childRackBins.push({
                        KSCReqChildID: x.KSCReqChildID,
                        ChildRackBins: x.ChildRackBins
                    });
                });
                initChildTable(masterData.Childs);
                $tblChildEl.refresh();

                if (masterData.IsCompleted) {
                    $divDetailsEl.find('#Req-Complete').prop('checked', true);
                    $divDetailsEl.find('#Req-Partial').prop('checked', false);
                } else {
                    $divDetailsEl.find('#Req-Complete').prop('checked', false);
                    $divDetailsEl.find('#Req-Partial').prop('checked', true);
                }
            })
            .catch(showResponseError);
    }

    function checkValidation() {
        var childs = $tblChildEl.getCurrentViewRecords();
        var hasError = false;
        for (var iC = 0; iC < childs.length; iC++) {
            var rowIndex = iC + 1;
            var child = childs[iC];
            if (child.IssueQty == 0) {
                toastr.error(`Give issue qty at row ${rowIndex}`);
                hasError = true;
                break;
            }
            if (child.IssueQtyCarton == 0) {
                toastr.error(`Give issue qty carton at row ${rowIndex}`);
                hasError = true;
                break;
            }
            if (child.IssueQtyCone == 0) {
                toastr.error(`Give issue qty cone at row ${rowIndex}`);
                hasError = true;
                break;
            }
        }
        return hasError;
    }

    function save(isSendForApproval, isApprove, isReject, rejectReason, isGPApprove) {
        var data = formDataToJson($formEl.serializeArray());
        debugger;
        if (menuType == _paramType.SCYDYarnReqApprove) {
            data.TransportTypeID = masterData.TransportTypeID;
            data.TransportAgencyID = masterData.TransportAgencyID;
            data.VehicleNo = masterData.VehicleNo;
            data.DriverName = masterData.DriverName;
            data.ContactNo = masterData.ContactNo;
        }
        else if (menuType == _paramType.SCYDYarnReq) {
            if (data.TransportTypeID == '' || typeof data.TransportTypeID == "undefined") {
                toastr.error('Must have Transport Type.');
                return false;
            }
            if (data.TransportAgencyID == '' || typeof data.TransportAgencyID == "undefined") {
                toastr.error('Must have Transport Agency.');
                return false;
            }
            if (data.VehicleNo == '' || typeof data.VehicleNo == "undefined") {
                toastr.error('Must have Vehicle No.');
                return false;
            }
            if (data.DriverName == '' || typeof data.DriverName == "undefined") {
                toastr.error('Must have Driver Name.');
                return false;
            }
            if (data.ContactNo == '' || typeof data.ContactNo == "undefined") {
                toastr.error('Must have Contact No.');
                return false;
            }
        }
        if (checkValidation()) return false;

        data.Childs = $tblChildEl.getCurrentViewRecords();

        data.ReqType = masterData.ReqType;
        data.ProgramName = masterData.ProgramName;

        if (getDefaultValueWhenInvalidS(data.ReqType) == "") {
            toastr.error("Req type missing");
            return false;
        }
        if (getDefaultValueWhenInvalidS(data.ProgramName) == "") {
            toastr.error("Program name missing");
            return false;
        }

        //set completed/partial
        if ($formEl.find('#Req-Complete').is(':checked')) {
            data.IsCompleted = true;
        } else {
            data.IsCompleted = false;
        }

        for (var index = 0; index < data.Childs.length; index++) {
            var indexF = _childRackBins.findIndex(x => x.KSCReqChildID == data.Childs[index].KSCReqChildID);
            if (indexF > -1) {

                _childRackBins[indexF].ChildRackBins.filter(x => x.KSCICRBId == 0).map(x => x.KSCICRBId = _maxKSCICRBId++);
                data.Childs[index].ChildRackBins = _childRackBins[indexF].ChildRackBins.filter(x => x.IssueCartoon > 0 || x.IssueQtyCone > 0 || x.IssueQtyKg > 0);

                totalIssueQty = 0;
                totalIssueQtyCone = 0;
                totalIssueQtyCarton = 0;

                _childRackBins[indexF].ChildRackBins.map(x => {
                    totalIssueQty += x.IssueQtyKg;
                    totalIssueQtyCone += x.IssueQtyCone;
                    totalIssueQtyCarton += x.IssueCartoon;
                });
                data.Childs[index].IssueQty = totalIssueQty;
                data.Childs[index].IssueQtyCone = totalIssueQtyCone;
                data.Childs[index].IssueQtyCarton = totalIssueQtyCarton;
            }
        }

        var path = '/api/ksc-issue/save';
        var msg = 'Saved successfully!';
        if (isSendForApproval) {
            data.IsSendForApprove = true;
        }
        if (isApprove) {
            data.IsApprove = true;
            //path = '/api/ksc-issue/approve';
            msg = 'Approved successfully!';
        }
        if (isReject) {
            data.IsReject = true;
            data.RejectReason = rejectReason;
            path = '/api/ksc-issue/reject';
            msg = 'Rejected successfully!';
        }
        if (isGPApprove) {
            data.IsGPApprove = true;
            path = '/api/ksc-issue/approveGP';
            msg = 'GP Approved successfully!';
        }

        axios.post(path, data)
            .then(function (response) {
                if (isGPApprove) {
                    msg = 'GP Approved successfully! GP No: ' + response.data;
                    showBootboxAlert(msg);
                }
                else {
                    toastr.success(msg);
                }
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
    function resetGlobals() {
        _childRackBins = [];
        _selectedKSCReqChildID = 0;
        _maxKSCICRBId = 999;

        _paramType = {
            SCYDYarnReq: 0,
            SCYDYarnReqApprove: 1
        }
    }
})();
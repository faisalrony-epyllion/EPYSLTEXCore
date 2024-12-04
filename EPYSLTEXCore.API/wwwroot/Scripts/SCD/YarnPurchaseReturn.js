(function () {
    var menuId, pageName, menuParam;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $tblChildEl, tblChildId, $formEl,
        $modalRackItemEl, $tblRackLocationItemEl, tblRackLocationItem;
    var pageId;
    var isCDAPage = false;
    var status;
    var _isPRPage = false;
    var _isPRApprovePage = false;
    var _childRackBins = [];
    //_childRackBins.push({
    //    ReceiveChildID: _selectedReceiveChildId,
    //    ChildRackBins: []
    //});
    var _selectedReceiveChildId = 0;
    var _maxYPRCRBId = 999;
    var masterData;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        if (!menuParam)
            menuParam = localStorage.getItem("menuParam");

        pageId = pageName + "-" + menuId;
        $pageEl = $(`#${pageId}`);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblCreateItemId = `#tblCreateItem-${pageId}`;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $modalRackItemEl = $("#modalRackItem" + pageId);
        tblRackLocationItem = "#tblRackLocationItem" + pageId;

        if (menuParam == "YPR") _isPRPage = true;
        else if (menuParam == "YPRA") _isPRApprovePage = true;

        $toolbarEl.find("#btnReceiveList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            operationalButtonHideShow();
            initMasterTable();
        });

        $toolbarEl.find("#btnDraft").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.DRAFT;
            operationalButtonHideShow();
            initMasterTable();
        });

        $toolbarEl.find("#btnApprovedList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            operationalButtonHideShow();
            initMasterTable();
        });

        $toolbarEl.find("#btnRejectList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REJECT;
            operationalButtonHideShow();
            initMasterTable();
        });

        $toolbarEl.find("#btnProposedForApporval").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            operationalButtonHideShow();
            initMasterTable();
        });

        $toolbarEl.find("#btnPendingForGatePass").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING_GROUP;
            operationalButtonHideShow();
            initMasterTable();
        });

        $toolbarEl.find("#btnGatePassApprovedList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED2;
            operationalButtonHideShow();
            initMasterTable();
        });

        $formEl.find("#btnDraftYR").click(function (e) {
            e.preventDefault();
            save(false, false, false, false, false);
        });
        $formEl.find("#btnSaveYR").click(function (e) {
            e.preventDefault();
            save(true, false, false, false, false);
        });
        $formEl.find("#btnApproveYR").click(function (e) {
            e.preventDefault();
            save(false, true, false, false, false);
        });
        $formEl.find("#btnRejectYR").click(function (e) {
            e.preventDefault();
            $formEl.find(".rejectReason").show();
            $formEl.find("#RejectReason").focus();
            save(false, false, true, false, false);
        });
        $formEl.find("#btnApproveYRGP").click(function (e) {
            save(false, false, false, true, false);
        });
        $formEl.find("#btnRevisionYR").click(function (e) {
            save(false, false, false, false, true);
        });

        $formEl.find("#btnYREditCancel").on("click", backToList);

        $toolbarEl.find(".btnListOperation").hide();
        if (_isPRPage) {
            $toolbarEl.find("#btnReceiveList,#btnDraft,#btnProposedForApporval,#btnApprovedList,#btnRejectList").show();
            $toolbarEl.find("#btnReceiveList").click();
        }
        else if (_isPRApprovePage) {
            $toolbarEl.find("#btnProposedForApporval,#btnPendingForGatePass,#btnGatePassApprovedList,#btnApprovedList,#btnRejectList").show();
            $toolbarEl.find("#btnProposedForApporval").click();
        }

        $formEl.find("#btnOk").click(function () {
            var hasErrorRack = false;
            if (_selectedReceiveChildId > 0) {
                var rackList = $tblRackLocationItemEl.getCurrentViewRecords();
                var childList = $tblChildEl.getCurrentViewRecords();

                var indexF = childList.findIndex(x => x.ReceiveChildID == _selectedReceiveChildId);
                if (indexF > -1) {
                    var totalNoOfCartoonReturn = 0;
                    var totalNoOfConeReturn = 0;
                    var totalReturnQty = 0;

                    for (var iRack = 0; iRack < rackList.length; iRack++) {
                        var rack = rackList[iRack];
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
                            toastr.error("Issue qty (" + rack.IssueQtyKg + ") cannot be greater then stock qty (" + rack.ReceiveQty + ")");
                            break;
                        }

                        totalNoOfCartoonReturn += rack.IssueCartoon;
                        totalNoOfConeReturn += rack.IssueQtyCone;
                        totalReturnQty += rack.IssueQtyKg;
                    }

                    if (!hasErrorRack) {
                        childList[indexF].NoOfCartoonReturn = totalNoOfCartoonReturn;
                        childList[indexF].NoOfConeReturn = totalNoOfConeReturn;
                        childList[indexF].ReturnQty = totalReturnQty;

                        var childObj = DeepClone(childList[indexF]);
                        $tblChildEl.updateRow(indexF, childObj);
                    }
                }
            }
            if (!hasErrorRack) {
                $modalRackItemEl.modal('hide');
            }
        });
    });
    function initMasterTable() {
        var commands = [];
        if (status === statusConstants.PENDING) {
            commands = [
                { type: 'New', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } }
            ]
        } else if (status === statusConstants.DRAFT ||
            status === statusConstants.PROPOSED_FOR_APPROVAL ||
            status === statusConstants.REJECT) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'Challan Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
            ]
        } else if (status === statusConstants.APPROVED ||
            status === statusConstants.APPROVED2) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'Challan Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'GatePass Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
            ]
        } else {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }
            ]
        }

        var columns = [
            {
                headerText: '', commands: commands, textAlign: 'Center', width: 100
            },
            {
                field: 'YarnReceiveType', headerText: 'Type'
            },
            {
                field: 'PONo', headerText: 'PO No'
            },
            {
                field: 'LCNo', headerText: 'LC No'
            },
            {
                field: 'YarnPurchaseReturnNo', headerText: 'Return No', visible: status == statusConstants.COMPLETED
            },
            {
                field: 'ReceiveNo', headerText: 'Receive No'
            },
            {
                field: 'ReceiveDate', headerText: 'Receive Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'ReturnChallanNo', headerText: 'Challan No', visible: status != statusConstants.PENDING
            },
            {
                field: 'ReturnChallanDate', headerText: 'Challan Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'GatePassNo', headerText: 'Gate Pass No', visible: status == statusConstants.APPROVED || status == statusConstants.PENDING_GROUP || status == statusConstants.APPROVED2
            },
            {
                field: 'GatePassDate', headerText: 'Gate Pass Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status == statusConstants.APPROVED
            },
            {
                field: 'GatePassApproveDate', headerText: 'GP Approve Done', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status == statusConstants.APPROVED2
            },
            {
                field: 'SupplierName', headerText: 'Supplier'
            },
            {
                field: 'TransportAgencyName', headerText: 'Agency'
            },
            {
                field: 'LocationName', headerText: 'Store Location'
            },
            {
                field: 'VehicalNo', headerText: 'Vehicle'
            },
            {
                field: 'RCompany', headerText: 'Rcv. Company'
            },
            {
                field: 'RejectReason', headerText: 'Reject Reason', visible: status == statusConstants.REJECT
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: true,
            apiEndPoint: `/api/yarn-purchase-return/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'New') {
            getNew(args.rowData.ReceiveID);
        }
        else if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.YarnPurchaseReturnID);
        }
        else if (args.commandColumn.type == 'Challan Report') {
            if ($.trim(args.rowData.ReturnChallanNo).length == 0) {
                toastr.error("No challan no found.");
                return false;
            }
            window.open(`/reports/InlinePdfView?ReportName=YarnReturnDC.rdl&ChallanNo=${args.rowData.ReturnChallanNo}`, '_blank');
        }
        else if (args.commandColumn.type == 'GatePass Report') {
            if ($.trim(args.rowData.GatePassNo).length == 0) {
                toastr.error("No gate pass found.");
                return false;
            }
            window.open(`/reports/InlinePdfView?ReportName=YarnReturnGP.rdl&GPNo=${args.rowData.GatePassNo}`, '_blank');
        }
    }
    function operationalButtonHideShow() {
        $formEl.find(".btnOperation").hide();
        if (_isPRPage) {
            if (status == statusConstants.PENDING) {
                $formEl.find("#btnDraftYR").show();
                $formEl.find("#btnSaveYR").show();
            }
            else if (status == statusConstants.DRAFT) {
                $formEl.find("#btnDraftYR").show();
                $formEl.find("#btnSaveYR").show();
            }
            else if (status == statusConstants.REJECT) {
                $formEl.find("#btnRevisionYR").show();
            }
        }
        else if (_isPRApprovePage) {
            if (status == statusConstants.PROPOSED_FOR_APPROVAL) {
                $formEl.find("#btnApproveYR").show();
                $formEl.find("#btnRejectYR").show();
            }
            else if (status == statusConstants.PENDING_GROUP) {
                $formEl.find("#btnApproveYRGP").show();
            }
        }
    }
    function reset() {
        _childRackBins = [];
        _selectedReceiveChildId = 0;
        $formEl.find(".rejectReason").hide();
    }
    function getNew(receiveID) {
        axios.get(`/api/yarn-purchase-return/new-return/${receiveID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                reset();

                masterData = response.data;
                masterData.YarnPurchaseReturnNo = '**<< NEW >>**';
                masterData.YarnPurchaseReturnDate = formatDateToDefault(masterData.YarnPurchaseReturnDate);
                masterData.ReceiveDate = formatDateToDefault(masterData.ReceiveDate);
                masterData.PODate = formatDateToDefault(masterData.PODate);
                masterData.LCDate = formatDateToDefault(masterData.LCDate);
                masterData.ReturnChallanNo = '**<< NEW >>**';
                masterData.ReturnChallanDate = new Date();
                masterData.ReturnChallanDate = formatDateToDefault(masterData.ReturnChallanDate);
                setFormData($formEl, masterData);
                var yarnPurchaseReturnChildID = 999;
                masterData.YarnPurchaseReturnChilds.map(x => {
                    x.YarnPurchaseReturnChildID = yarnPurchaseReturnChildID++;
                });
                initChildTable(masterData.YarnPurchaseReturnChilds);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }


    function getDetails(id) {
        axios.get(`/api/yarn-purchase-return/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                reset();

                masterData = response.data;
                masterData.YarnPurchaseReturnDate = formatDateToDefault(masterData.YarnPurchaseReturnDate);
                masterData.ReceiveDate = formatDateToDefault(masterData.ReceiveDate);
                masterData.PODate = formatDateToDefault(masterData.PODate);
                masterData.LCDate = formatDateToDefault(masterData.LCDate);
                masterData.ReturnChallanDate = formatDateToDefault(masterData.ReturnChallanDate);
                masterData.GatePassApproveDate = formatDateToDefault(masterData.GatePassApproveDate);
                setFormData($formEl, masterData);

                $formEl.find(".rejectReason").hide();
                if (status == statusConstants.REJECT) {
                    $formEl.find(".rejectReason").show();
                }

                $formEl.find(".gatePassInfo").hide();
                if (status == statusConstants.PENDING_GROUP || status == statusConstants.APPROVED2) {
                    $formEl.find(".gatePassInfo").show();
                }

                masterData.YarnPurchaseReturnChilds.map(x => {
                    _childRackBins.push({
                        ReceiveChildID: x.ReceiveChildID,
                        ChildRackBins: x.ChildRackBins
                    });
                });

                initChildTable(masterData.YarnPurchaseReturnChilds);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }


    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }
    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#YarnPurchaseReturnID").val(-1111);
        $formEl.find("#ReceiveID").val(-1111);
    }

    async function initChildTable(data) {
        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];

        if (isCDAPage) {
            columns.push(
                { field: 'Segment1ValueDesc', headerText: 'Item Name', allowEditing: false, width: 80 },
                { field: 'Segment2ValueDesc', headerText: 'Agent Name', allowEditing: false, width: 80 }
            )
        }
        else {
            columns = [
                {
                    headerText: '', commands: [
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                    ], width: 60
                }
            ];
            var segColumns = [
                { field: 'Segment1ValueDesc', headerText: 'Composition', allowEditing: false, width: 350 },
                { field: 'Segment2ValueDesc', headerText: 'Yarn Type', allowEditing: false, width: 120 },
                { field: 'Segment3ValueDesc', headerText: 'Process', allowEditing: false, width: 100 },
                { field: 'Segment4ValueDesc', headerText: 'Sub Process', allowEditing: false, width: 100 },
                { field: 'Segment5ValueDesc', headerText: 'Quality Parameter', allowEditing: false, width: 100 },
                { field: 'Segment6ValueDesc', headerText: 'Count', allowEditing: false, width: 80 }
            ];

            columns.push.apply(columns, segColumns);
            columns.push.apply(columns, [{ field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false, width: 100 }]);
        }

        var additionalColumns = [
            { field: 'YarnPurchaseReturnChildID', isPrimaryKey: true, visible: false, width: 10 },
            { field: 'YarnPurchaseReturnID', visible: false, width: 10 },
            { field: 'ReceiveChildID', visible: false, width: 10 },
            { field: 'ChallanCount', headerText: 'Challan Count', minWidth: 250, maxWidth: 350, textAlign: 'center', allowEditing: false },
            { field: 'PhysicalCount', headerText: 'Physical Count', minWidth: 250, maxWidth: 350, textAlign: 'center', allowEditing: false },
            { field: 'BuyerName', headerText: 'Buyer', allowEditing: false, width: 100 },
            { field: 'EWONo', headerText: 'Shade Code', allowEditing: false, width: 100 },
            { field: 'SpinnerName', headerText: 'Spinner', allowEditing: false, width: 100 },
            { field: 'YarnControlNo', headerText: 'Receive ID', allowEditing: false, visible: status != statusConstants.PendingReceivePO && status != statusConstants.PendingReceiveCI, textAlign: 'center', width: 80 },
            { field: 'ChallanLot', headerText: 'Challan Lot', textAlign: 'center', width: 80, allowEditing: false },
            { field: 'LotNo', headerText: 'Physical Lot', textAlign: 'center', width: 80, allowEditing: false },
            { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false, width: 80 },
            { field: 'POQty', headerText: 'PO Qty', allowEditing: false, width: 80 },
            { field: 'InvoiceQty', headerText: 'Invoice Qty', allowEditing: false, width: 80 },
            { field: 'ChallanQty', headerText: 'Challan/PL Qty', allowEditing: false, textAlign: 'right', width: 80 },

            { field: 'NoOfCartoon', headerText: 'No Of Cartoon', textAlign: 'center', width: 80, allowEditing: false },
            { field: 'NoOfCone', headerText: 'No Of Cone', textAlign: 'center', width: 80, allowEditing: false },
            { field: 'ReceiveQty', headerText: 'Receive Qty', textAlign: 'right', width: 80, allowEditing: false },

            { field: 'NoOfCartoonReturn', headerText: 'Return Cartoon', allowEditing: false, textAlign: 'center', width: 80, valueAccessor: diplayNumberButton },
            { field: 'NoOfConeReturn', headerText: 'Return Cone', allowEditing: false, textAlign: 'center', width: 80, valueAccessor: diplayNumberButton },
            { field: 'ReturnQty', headerText: 'Return Qty', allowEditing: false, textAlign: 'center', width: 80, valueAccessor: diplayNumberButton },

            //{ field: 'NoOfCartoonReturn', headerText: 'Return Cartoon', textAlign: 'center', width: 80 },
            //{ field: 'NoOfConeReturn', headerText: 'Return Cone', textAlign: 'center', width: 80 },
            //{ field: 'ReturnQty', headerText: 'Return Qty', textAlign: 'right', width: 80 },

            { field: 'ExcessQty', headerText: 'Excess Qty', textAlign: 'right', allowEditing: false, width: 80 },
            { field: 'ShortQty', headerText: 'Short Qty', textAlign: 'right', allowEditing: false, width: 80 },
            { field: 'Remarks', headerText: 'Remarks', textAlign: 'left', width: 400 }
        ];

        columns.push.apply(columns, additionalColumns);

        $tblChildEl = new ej.grids.Grid({
            //tableId: tblChildId,
            dataSource: data,
            columns: columns,
            allowResizing: true,
            actionBegin: function (args) {
                if (args.requestType === "beginEdit") {

                }
                else if (args.requestType === "save") {

                    var obj = args.data;
                    if (obj.ReturnQty > obj.ReceiveQty) {
                        toastr.error("Return qty cannot be greater than receive qty");
                        args.data.ReturnQty = obj.ReceiveQty;
                    }
                    if (obj.NoOfConeReturn > obj.NoOfCone) {
                        toastr.error("Maximun No of cone is " + obj.NoOfCone);
                        args.data.NoOfConeReturn = obj.NoOfCone;
                    }
                    if (obj.NoOfCartoonReturn > obj.NoOfCartoon) {
                        toastr.error("Maximun No of cartoon is " + obj.NoOfCartoon);
                        args.data.NoOfCartoonReturn = obj.NoOfCartoon;
                    }
                }
                else if (args.requestType === "delete") {
                    //var YRCList = $tblChildEl.getCurrentViewRecords();
                    //masterData.YarnPurchaseReturnChilds[0].YarnPurchaseReturnChildID = YRCList.map(function (el) {
                    //    if (args.data[0].ChildID != el.ChildID) {
                    //        return el.ChildID
                    //    }
                    //}).toString();
                }
            },
            recordClick: function (args) {
                if (args.column && (args.column.field == "NoOfCartoonReturn" || args.column.field == "NoOfConeReturn" || args.column.field == "ReturnQty")) {
                    _selectedReceiveChildId = args.rowData.ReceiveChildID;
                    var yarnPurchaseReturnChildID = args.rowData.YarnPurchaseReturnChildID;

                    axios.get(`/api/yarn-rack-bin-allocation/get-by-receive-child/${_selectedReceiveChildId}/${args.rowData.LocationID}/0`)
                        .then(function (response) {
                            var list = response.data;
                            if (list.length == 0) {
                                toastr.error("Rack bin allocation not completed.");
                                return false;
                            }

                            if ($tblRackLocationItemEl) $tblRackLocationItemEl.destroy();

                            //Set pop up field values
                            var indexF = _childRackBins.findIndex(x => x.ReceiveChildID == _selectedReceiveChildId);
                            if (indexF > -1) {
                                var childRackBins = _childRackBins[indexF].ChildRackBins;
                                list.map(x => {
                                    var indexC = childRackBins.findIndex(y => y.ChildRackBinID == x.ChildRackBinID); // && y.YarnPurchaseReturnChildID == yarnPurchaseReturnChildID
                                    if (indexC > -1) {
                                        var crbObj = childRackBins[indexC];

                                        x.YPRCRBId = crbObj.YPRCRBId;
                                        x.IssueCartoon = crbObj.IssueCartoon;
                                        x.IssueQtyCone = crbObj.IssueQtyCone;
                                        x.IssueQtyKg = crbObj.IssueQtyKg;
                                    }
                                });
                            }
                            list.filter(x => x.YPRCRBId == 0).map(y => {
                                y.YPRCRBId = _maxYPRCRBId++;
                            });

                            ej.base.enableRipple(true);
                            $tblRackLocationItemEl = new ej.grids.Grid({
                                dataSource: list,
                                allowResizing: true,

                                columns: [
                                    { field: 'YPRCRBId', isPrimaryKey: true, visible: false, width: 10 },
                                    { field: 'LocationName', headerText: 'Location', allowEditing: false, width: 80 },
                                    { field: 'RackNo', headerText: 'Rack', allowEditing: false, width: 80 },
                                    { field: 'NoOfCartoon', headerText: 'No of Cartoon', allowEditing: false, width: 80 },
                                    { field: 'NoOfCone', headerText: 'No of Cone', allowEditing: false, width: 80 },
                                    { field: 'ReceiveQty', headerText: 'Stock Qty', allowEditing: false, width: 80 },
                                    { field: 'IssueCartoon', headerText: 'Issue Cartoon', allowEditing: true, width: 80, edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } } },
                                    { field: 'IssueQtyCone', headerText: 'Issue Cone', allowEditing: true, width: 80, edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } } },
                                    { field: 'IssueQtyKg', headerText: 'Issue Qty (Kg)', allowEditing: true, width: 80, edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } } }
                                ],
                                editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
                                recordClick: function (args) {

                                },
                                actionBegin: function (args) {
                                    if (args.requestType === "save") {
                                        var indexF = _childRackBins.findIndex(x => x.ReceiveChildID == _selectedReceiveChildId);
                                        if (indexF == -1) {
                                            _childRackBins.push({
                                                ReceiveChildID: _selectedReceiveChildId,
                                                ChildRackBins: []
                                            });
                                            indexF = _childRackBins.findIndex(x => x.ReceiveChildID == _selectedReceiveChildId);
                                            if (indexF > -1) {
                                                _childRackBins[indexF].ChildRackBins.push(args.data);
                                            }
                                        } else {
                                            var indexC = _childRackBins[indexF].ChildRackBins.findIndex(x => x.ChildRackBinID == args.data.ChildRackBinID);
                                            if (indexC == -1) {
                                                _childRackBins[indexF].ChildRackBins.push(args.data);
                                            } else {
                                                _childRackBins[indexF].ChildRackBins[indexC] = args.data;
                                            }
                                        }
                                    }
                                },
                            });
                            $tblRackLocationItemEl.refreshColumns;
                            $tblRackLocationItemEl.appendTo(tblRackLocationItem);
                            $modalRackItemEl.modal('show');
                        })
                        .catch(function (err) {
                            toastr.error(err.response.data.Message);
                        });
                }
            },
            autofitColumns: true,
            showDefaultToolbar: false,
            allowFiltering: false,
            toolbar: ['ColumnChooser'],
            editSettings: { allowAdding: false, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true }
            //commandClick: childCommandClick,
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }
    async function childCommandClick(e) {

    }
    function diplayNumberButton(field, data, column) {
        column.disableHtmlEncode = false;
        return `<a class="btn btn-xs btn-default" href="javascript:void(0)" title="${column.headerText}">
                                     ${data[field] ? data[field] : 0}
                                </a>`;
    }
    function checkValidation(obj) {
        if (obj.TransportTypeId == null || obj.TransportTypeId == "" || obj.TransportTypeId == 0) {
            toastr.error("Select Transport Type.");
            return false;
        }
        if (obj.TransportAgencyId == null || obj.TransportAgencyId == "" || obj.TransportAgencyId == 0) {
            toastr.error("Select Transport Agency.");
            return false;
        }
        if (obj.VehicleNo == null || $.trim(obj.VehicleNo).length == 0) {
            toastr.error("Give Vehicle No.");
            return false;
        }
        if (obj.LockNo == null || $.trim(obj.LockNo).length == 0) {
            toastr.error("Give Lock No.");
            return false;
        }
        if (obj.DriverName == null || $.trim(obj.DriverName).length == 0) {
            toastr.error("Give Driver Name.");
            return false;
        }
        if (obj.DriverContactNo == null || $.trim(obj.DriverContactNo).length == 0) {
            toastr.error("Give Driver Contact No.");
            return false;
        }
        if (obj.ReturnReason == null || $.trim(obj.ReturnReason).length == 0) {
            toastr.error("Give Return Reason.");
            return false;
        }
        return true;
    }

    function save(isProposed, isApporve, isReject, isGatePassApprove, isRevise) {
        if (isReject) {
            var rejectReason = $formEl.find("#RejectReason").val();
            if ($.trim(rejectReason).length == 0) {
                toastr.error("Give reject reason.");
                return false;
            }
        }

        var data = formDataToJson($formEl.serializeArray());
        if (!checkValidation(data)) return false;

        data.YarnPurchaseReturnChilds = $tblChildEl.getCurrentViewRecords();

        data.IsProposedForApporval = isProposed;
        data.IsApprove = isApporve;
        data.IsReject = isReject;
        data.IsGatePassApprove = isGatePassApprove;
        data.IsRevise = isRevise;

        data.RejectReason = isReject ? $.trim($formEl.find("#RejectReason").val()) : "";

        var totalNoOfCartoonReturn = 0;
        var totalNoOfConeReturn = 0;
        var totalReturnQty = 0;

        var hasError = false;
        for (var i = 0; i < data.YarnPurchaseReturnChilds.length; i++) {
            var indexF = _childRackBins.findIndex(x => x.ReceiveChildID == data.YarnPurchaseReturnChilds[i].ReceiveChildID);
            if (indexF > -1) {
                data.YarnPurchaseReturnChilds[i].ChildRackBins = _childRackBins[indexF].ChildRackBins.filter(x => x.IssueCartoon > 0 || x.IssueQtyCone > 0 || x.IssueQtyKg > 0);

                totalNoOfCartoonReturn = 0;
                totalNoOfConeReturn = 0;
                totalReturnQty = 0;

                _childRackBins[indexF].ChildRackBins.map(x => {
                    totalNoOfCartoonReturn += x.IssueCartoon;
                    totalNoOfConeReturn += x.IssueQtyCone;
                    totalReturnQty += x.IssueQtyKg;
                });
                data.YarnPurchaseReturnChilds[i].NoOfCartoonReturn = totalNoOfCartoonReturn;
                data.YarnPurchaseReturnChilds[i].NoOfConeReturn = totalNoOfConeReturn;
                data.YarnPurchaseReturnChilds[i].ReturnQty = totalReturnQty;
            }

            var child = data.YarnPurchaseReturnChilds[i];
            if (child.ReturnQty == 0 && child.NoOfCartoonReturn == 0 && child.NoOfConeReturn == 0) {
                toastr.error("No return found.");
                hasError = true;
                break;
            }
            if (child.NoOfCartoon < child.NoOfCartoonReturn) {
                toastr.error("Return cartoon cannot be greater than no of cartoon.");
                hasError = true;
                break;
            }
            if (child.NoOfCone < child.NoOfConeReturn) {
                toastr.error("Return cone cannot be greater than no of cone.");
                hasError = true;
                break;
            }
            if (child.ReceiveQty < child.ReturnQty) {
                toastr.error("Return qty cannot be greater than receive qty.");
                hasError = true;
                break;
            }
        }
        if (hasError) return false;

        var successMessage = "Successfully saved.";
        if (isProposed) successMessage = "Successfully sent for approval.";
        else if (isApporve) successMessage = "Successfully approved.";
        else if (isReject) successMessage = "Successfully rejected.";

        axios.post("/api/yarn-purchase-return/save", data)
            .then(function () {
                toastr.success(successMessage);
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
})();
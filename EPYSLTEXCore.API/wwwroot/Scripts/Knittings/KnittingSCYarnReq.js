(function () {
    var menuId, pageName, menuParam;
    var pageId, toolbarId;
    var KnittingSCYarnReqApprovePage;
    var KnittingSCYarnReq;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, tblMasterId, tblChildId;
    var status;

    var masterData = null;
    var isReq = false,
        isApp = false,
        isAck = false;
    var isKnitting = false,
        isYD = false;
    var _actionObj = {
        IsDraft: false,
        IsSendForApproval: false,
        IsApprove: false,
        IsReject: false,
        RejectReason: "",
        IsAcknowledge: false,
        IsUnacknowledge: false,
        UnAcknowledgeReason: ""
    };
    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        if (!menuParam)
            menuParam = localStorage.getItem("menuParam");

        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        if (menuParam == "Req") isReq = true;
        else if (menuParam == "App") isApp = true;
        else if (menuParam == "Ack") isAck = true;

        if (isReq || isApp) {
            isKnitting = true;
        }

        $toolbarEl.find(".btnToolbar").hide();
        if (isReq) {
            $toolbarEl.find(".btnToolbar").show();
        }
        else if (isApp) {
            $toolbarEl.find("#btnSendForApprovalList,#btnApproveList,#btnRejectList").show();
        }
        else if (isAck) {
            $toolbarEl.find("#btnPendingForAcknowledgeList,#btnAcknowledgeList,#btnUnAcknowledgeList").show();
        }

        $toolbarEl.find("#btnPendingList").click(function (e) {
            e.preventDefault();
            actionBtnHideShow();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            initMasterTable();
        });
        $toolbarEl.find("#btnDraftList").click(function (e) {
            e.preventDefault();
            actionBtnHideShow();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.DRAFT;
            initMasterTable();
        });
        $toolbarEl.find("#btnSendForApprovalList").click(function (e) {
            e.preventDefault();
            actionBtnHideShow();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            initMasterTable();
        });
        $toolbarEl.find("#btnApproveList").click(function (e) {
            e.preventDefault();
            actionBtnHideShow();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            initMasterTable();
        });
        $toolbarEl.find("#btnRejectList").click(function (e) {
            e.preventDefault();
            actionBtnHideShow();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REJECT;
            initMasterTable();
        });
        $toolbarEl.find("#btnRevisionList").click(function (e) {
            e.preventDefault();
            actionBtnHideShow();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING_REVISE;
            initMasterTable();
        });
        $toolbarEl.find("#btnPendingForAcknowledgeList").click(function (e) {
            e.preventDefault();
            actionBtnHideShow();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED_FOR_ACKNOWLEDGE;
            initMasterTable();
        });
        $toolbarEl.find("#btnAcknowledgeList").click(function (e) {
            e.preventDefault();
            actionBtnHideShow();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACKNOWLEDGE;
            initMasterTable();
        });
        $toolbarEl.find("#btnUnAcknowledgeList").click(function (e) {
            e.preventDefault();
            actionBtnHideShow();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.UN_ACKNOWLEDGE;
            initMasterTable();
        });
        $toolbarEl.find("#btnRefreshList").click(function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
        });

        //Action Operations

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            _actionObj.IsDraft = true;
            save();
        });

        $formEl.find("#btnSendForApproval").click(function (e) {
            e.preventDefault();
            _actionObj.IsSendForApproval = true;
            save();
        });

        $formEl.find("#btnApproval").click(function (e) {
            e.preventDefault();
            _actionObj.IsApprove = true;
            save();
        });

        $formEl.find("#btnReject").click(function (e) {
            e.preventDefault();
            bootbox.prompt("Enter your Reject reason:", function (result) {
                if (!result) {
                    return toastr.error("Reject reason is required.");
                }
                _actionObj.IsReject = true;
                _actionObj.RejectReason = result;

                save();
            });
        });

        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            _actionObj.IsAcknowledge = true;
            save();
        });

        $formEl.find("#btnUnacknowledge").click(function (e) {
            e.preventDefault();
            bootbox.prompt("Enter your Unacknowledge reason:", function (result) {
                if (!result) {
                    return toastr.error("Unacknowledge reason is required.");
                }
                _actionObj.IsUnacknowledge = true;
                _actionObj.UnAcknowledgeReason = result;

                save();
            });
        });

        $formEl.find("#btnCancel").on("click", backToList);


        if (isReq) {
            $toolbarEl.find("#btnPendingList").click();
        }
        else if (isApp) {
            $toolbarEl.find("#btnSendForApprovalList").click();
        }
        else if (isAck) {
            $toolbarEl.find("#btnPendingForAcknowledgeList").click();
        }
    });

    function initMasterTable() {
        var columns = [
            {
                headerText: 'Command', width: 100, visible: status == statusConstants.PENDING, commands: [
                    { type: 'New', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } },
                    { type: 'MOU, YD Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            },
            {
                headerText: 'Command', width: 100, visible: status !== statusConstants.PENDING && !isAck, commands: [
                    { type: 'Edit', title: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-edit' } },
                    { type: 'MRS Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                    { type: 'SC MOU', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            },
            {
                headerText: 'Command', width: 150, visible: isAck && status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE, commands: [
                    { type: 'Acknowledge', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } },
                    { type: 'Edit', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'MRS Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                    { type: 'MOU, YD Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            },
            {
                headerText: 'Command', width: 120, visible: isAck && status != statusConstants.PROPOSED_FOR_ACKNOWLEDGE, commands: [
                    { type: 'Edit', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'MRS Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                    { type: 'MOU, YD Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            },
            {
                field: 'ReqType', headerText: 'Requisition Type', visible: isAck
            },
            {
                field: 'SubGroupName', headerText: 'Sub Group', width: 90, visible: isAck || isReq
            },
            {
                field: 'ProgramName', headerText: 'Program', width: 90, visible: isAck || isReq
            },
            {
                field: 'KSCReqNo', headerText: 'Requisition No', visible: status !== statusConstants.PENDING
            },
            {
                field: 'KSCReqDate', headerText: 'Requisition Date', type: 'date', format: _ch_date_format_1, visible: status !== statusConstants.PENDING
            },
            {
                field: 'BookingNo', headerText: 'Concept / Booking No'
            },
            {
                field: 'BuyerName', headerText: 'Buyer', visible: isAck
            },
            {
                field: 'ReqQty', headerText: 'Qty(KG)', visible: isAck
            },
            {
                field: 'UnitName', headerText: 'Subcontract Unit', visible: isAck
            },
            {
                field: 'KSCNo', headerText: 'KSC/YDBooking No'
            },
            {
                field: 'KSCDate', headerText: 'KSC/YDBooking Date', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'KSCByUser', headerText: 'KSC/YDBooking By'
            },
            {
                field: 'CompanyName', headerText: 'Company', visible: isAck
            },
            //{
            //    field: 'SubContractor', headerText: 'Sub Contract'
            //}
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/ksc-req/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'MRS Report') {
            if (args.rowData.ReqType == "SC" || isKnitting) {
                window.open(`/reports/InlinePdfView?ReportName=DailySubContractYarnRequisitionSlip.rdl&RequisitionID=${args.rowData.KSCReqMasterID}`, '_blank');
            } else if (args.rowData.ReqType == "YD" || isYD) {
                window.open(`/reports/InlinePdfView?ReportName=YDYarnRequisitionSlip.rdl&RequisitionID=${args.rowData.KSCReqMasterID}`, '_blank');
            }
        }
        else if (args.commandColumn.type == 'MOU, YD Booking Report') {
            if (args.rowData.ReqType == "SC" || isKnitting) {
                window.open(`/reports/InlinePdfView?ReportName=KnittingSubContract.rdl&KSCNo=${args.rowData.KSCNo}`, '_blank');
            } else if (args.rowData.ReqType == "YD" || isYD) {
                window.open(`/reports/InlinePdfView?ReportName=YarnDyedBooking.rdl&YDBookingNo=${args.rowData.KSCNo}`, '_blank');
            }
        }
        else if (args.commandColumn.type == 'SC MOU') {
            window.open(`/reports/InlinePdfView?ReportName=KnittingSubContract.rdl&KSCNo=${args.rowData.KSCNo}`, '_blank');
        }
        else if (args.commandColumn.type == 'Acknowledge' && isAck) {
            if (args.rowData.ReqType == 'SC') {
                isKnitting = true;
                isYD = false;
            }
            else if (args.rowData.ReqType == 'YD') {
                isKnitting = false;
                isYD = true;
            }
            ackMaster(args.rowData.KSCReqMasterID, args.rowData.ReqType);
        }
        else if (args.commandColumn.type == 'New') {
            getNew(args.rowData.KSCMasterID);
        }
        else {
            var reqType = 'SC';
            if (args.rowData.ReqType == 'YD') {
                isYD = true;
                reqType = 'YD';
            }
            else {
                isKnitting = true;
            }

            getDetails(args.rowData.KSCReqMasterID, reqType);
        }
    }

    function initChildTable(records) {
        if ($tblChildEl) $tblChildEl.destroy();
        ej.base.enableRipple(true);

        var childColumns = [
            {
                headerText: 'Command', width: 100, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                ]
            },
            {
                field: 'KSCReqMasterID', visible: false
            },
            {
                field: 'KSCReqChildID', visible: false
            },
            {
                field: 'KSCChildItemID', visible: false, isPrimaryKey: true
            },
            {
                field: 'YBChildItemID', visible: false
            },
            {
                field: 'ReqType', visible: false
            },
            {
                field: 'ItemMasterID', visible: false
            },
            {
                field: 'UnitID', visible: false
            },
            {
                field: 'ConceptNo', headerText: 'Concept No', allowEditing: false
            },
            //{
            //    field: 'YarnComposition', headerText: 'Composition', allowEditing: false
            //},
            //{
            //    field: 'YarnType', headerText: 'Yarn Type', allowEditing: false
            //},
            //{
            //    field: 'YarnProcess', headerText: 'Process', allowEditing: false
            //},
            //{
            //    field: 'YarnSubProcess', headerText: 'Sub Process', allowEditing: false
            //},
            //{
            //    field: 'YarnQualityParameter', headerText: 'Quality Parameter', allowEditing: false
            //},
            //{
            //    field: 'YarnCount', headerText: 'Count', allowEditing: false
            //},
            {
                field: 'YarnCategory', headerText: 'Yarn Detail', allowEditing: false
            },
            {
                field: 'Shade', headerText: 'Shade Code', allowEditing: false
            },
            {
                field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false
            },
            {
                field: 'YarnLotNo', headerText: 'Lot No', allowEditing: false
            },
            {
                field: 'SpinnerID', visible: false
            },
            {
                field: 'SpinnerName', headerText: 'Spinner', allowEditing: false
            },
            {
                field: 'AllocatedQty', headerText: 'Allocated Qty', allowEditing: false
            },
            {
                field: 'YarnReqQty', headerText: 'Yarn Req Qty', allowEditing: false
            },
            {
                field: 'PendingQty', headerText: 'Pending Qty', allowEditing: false, visible: status == statusConstants.PENDING && isReq
            },
            {
                field: 'ReqQty', headerText: 'Req Qty(KG)', allowEditing: (status === statusConstants.PENDING || status === statusConstants.DRAFT),
                editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 0 } }
            },
            {
                field: 'ReqCone', headerText: 'Req Cone(PCS)', allowEditing: (status === statusConstants.PENDING || status === statusConstants.DRAFT),
                editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 0 } }
            },
            { field: 'StockQty', headerText: 'Stock Qty', allowEditing: false, visible: masterData.IsBDS != 2},
            {
                field: 'Remarks', headerText: 'Remarks', allowEditing: (status === statusConstants.PENDING || status === statusConstants.DRAFT)
            }
        ];

        if (typeof masterData.StockTypeList === "undefined" || masterData.StockTypeList == null) {
            masterData.StockTypeList = [];
        }

        var stockTypeCell = "";
        if (!isApp & !isAck & (status == statusConstants.DRAFT || status == statusConstants.PENDING || status == statusConstants.REVISE)) {
            stockTypeCell = {
                field: 'StockTypeId',
                headerText: 'Stock Type',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.StockTypeList,
                displayField: "text",
                allowEditing: true,
                visible: masterData.IsBDS != 2,
                edit: ej2GridDropDownObj({
                })
            };

        }
        else {
            stockTypeCell = {
                field: 'StockTypeName',
                headerText: 'Stock Type',
                allowEditing: false,
                visible: masterData.IsBDS != 2
            }
        }

        var indexS = childColumns.findIndex(x => x.field == 'ReqCone');
        indexS = indexS + 1;
        childColumns.splice(indexS, 0, stockTypeCell);

        childColumns = setMandatoryFieldsCSS(childColumns, "ReqQty, ReqCone");

        $tblChildEl = new ej.grids.Grid({
            editSettings: { showDeleteConfirmDialog: true, allowEditing: true, allowDeleting: true },
            allowResizing: true,
            dataSource: records,
            columns: childColumns,
            actionBegin: function (args) {
                if (args.requestType === "save") {
                    
                    if (args.data.ReqQty > args.data.PendingQty) {
                        toastr.error(`Req Qty ${args.data.ReqQty} cannot be greater than Pending Qty ${args.data.PendingQty}`);
                        args.data.ReqQty = args.data.PendingQty;
                        args.rowData = args.data;
                        return false;
                    }
                    if (masterData.IsBDS != 2) {
                        args.data.StockQty = parseInt(args.data.StockTypeId) == 3 ? parseFloat(args.data.AdvanceStockQty) : parseFloat(args.data.SampleStockQty);
                    }
                }
                else if (args.requestType.toLowerCase() === "delete") {
                    // args.cancel = true;
                }
            },
        });
        $tblChildEl.appendTo(tblChildId);
        $tblChildEl.refresh();
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
        $formEl.find("#KSCReqMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getNew(kscMasterId) {
        actionBtnHideShow();
        axios.get(`/api/ksc-req/new/${kscMasterId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                
                masterData = response.data;
                masterData.KSCReqDate = formatDateToDefault(new Date());
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
            })
            .catch(showResponseError);
    }

    function getDetails(id, reqType) {
        actionBtnHideShow();

        var isEdit = false;
        if (status == statusConstants.DRAFT) isEdit = true;

        axios.get(`/api/ksc-req/${id}/${reqType}/${isEdit}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.KSCReqDate = formatDateToDefault(masterData.KSCReqDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
                
            })
            .catch(showResponseError);
    }

    function ackMaster(kscReqMasterID, reqType) {
        var data = {
            KSCReqMasterID: kscReqMasterID,
            IsAcknowledge: true,
            ReqType: reqType
        };
        axios.post("/api/ksc-req/save", data)
            .then(function () {
                toastr.success("Successfully acknowledged!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function save() {
        
        var data = formDataToJson($formEl.serializeArray());
        data.Childs = $tblChildEl.getCurrentViewRecords();

        data.Childs = data.Childs.filter(x=>x.ReqQty > 0);
        data.IsSendForApproval = _actionObj.IsSendForApproval;
        data.IsApprove = _actionObj.IsApprove;
        data.IsReject = _actionObj.IsReject;
        data.RejectReason = _actionObj.RejectReason;
        data.IsAcknowledge = _actionObj.IsAcknowledge;
        data.IsUnAcknowledge = _actionObj.IsUnacknowledge;
        data.UnAcknowledgeReason = _actionObj.UnAcknowledgeReason;
        if (status == statusConstants.DRAFT || status == statusConstants.PENDING) {
        var hasError = false;
        if (data.IsBDS == 2) {
            for (var i = 0; i < data.Childs.length; i++) {
                if (data.Childs[i].ReqQty > data.Childs[i].AllocatedQty) {
                    toastr.error(`Req Qty ${data.Childs[i].ReqQty} cannot be greater than Allocated Qty ${data.Childs[i].AllocatedQty}`);
                    hasError = true;
                    break;
                }
                if (hasError) break;
            }
            if (hasError) return false;

            for (var i = 0; i < data.Childs.length; i++) {
                var child = data.Childs[i];
                var totalReqQty = getTotalReqQtyOfCurrentItem(data.Childs, child.YBChildItemID);
                var maxReqQty = child.PendingQty; //getMaxAllocatedQty(data.Childs, child.YBChildItemID);
                if (totalReqQty > maxReqQty) {
                    toastr.error(`Total req qty is ${totalReqQty} cannot be greater than Maximum Yarn Req qty is ${maxReqQty}. `);
                    hasError = true;
                    break;
                }
            }
            if (hasError) return false;
        }
        else {
            for (var i = 0; i < data.Childs.length; i++) {
                if (data.Childs[i].ReqQty > data.Childs[i].StockQty && data.Childs[i].YDItem == false) {
                    toastr.error(`Req Qty ${data.Childs[i].ReqQty} cannot be greater than Stock Qty ${data.Childs[i].StockQty}`);
                    hasError = true;
                    break;
                }
                if (hasError) break;
            }
            if (hasError) return false;

            for (var i = 0; i < data.Childs.length; i++) {
                var child = data.Childs[i];
                var totalReqQty = child.ReqQty//getTotalReqQtyOfCurrentItem(data.Childs, child.YBChildItemID);
                var maxReqQty = child.PendingQty; //getMaxAllocatedQty(data.Childs, child.YBChildItemID);
                if (totalReqQty > maxReqQty) {
                    toastr.error(`Total req qty is ${totalReqQty} cannot be greater than maximum req qty is ${maxReqQty}. `);
                    hasError = true;
                    break;
                }
            }
            if (hasError) return false;

            for (var i = 0; i < data.Childs.length; i++) {
                var child = data.Childs[i];
                var totalReqQty = getTotalReqQtyOfCurrentStockSet(data.Childs, child.YarnStockSetId);
                var maxReqQty = child.StockQty; //getMaxAllocatedQty(data.Childs, child.YBChildItemID);
                if (totalReqQty > maxReqQty) {
                    toastr.error(`Total req qty is ${totalReqQty} cannot be greater than stock qty is ${maxReqQty}. `);
                    hasError = true;
                    break;
                }
            }
            if (hasError) return false;

        }
        if (hasError) return false;
        }

        if (isKnitting == true) {
            data.ReqType = 'SC'
        }
        else if (isYD == true) {
            data.ReqType = 'YD'
        }
        //data.ReqType = isKnitting == false ? 'YD' : 'Knitting';

        
        axios.post("/api/ksc-req/save", data)
            .then(function () {
                if (data.IsSendForApproval) toastr.success("Successfully propose for approval!");
                else if (data.IsApprove) toastr.success("Successfully approved!");
                else if (data.IsReject) toastr.success("Successfully rejected!");
                else if (data.IsAcknowledge) toastr.success("Successfully acknowledged!");
                else if (data.IsReject) toastr.success("Successfully unacknowledged!");
                else toastr.success("Successfully saved!");

                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function getTotalReqQtyOfCurrentItem(childs, ybChildItemID) {
        var totalReqQty = 0;
        childs.filter(x => x.YBChildItemID == ybChildItemID).map(x => {
            totalReqQty += getDefaultValueWhenInvalidN_Float(x.ReqQty);
        });
        return totalReqQty;
    }
    function getTotalReqQtyOfCurrentStockSet(childs, yarnStockSetId) {
        var totalReqQty = 0;
        childs.filter(x => x.YarnStockSetId == yarnStockSetId).map(x => {
            totalReqQty += getDefaultValueWhenInvalidN_Float(x.ReqQty);
        });
        return totalReqQty;
    }
    function actionBtnHideShow() {
        resetBasicInfo();
        $formEl.find(".btnAction").hide();
        if (isReq) {
            if (status == statusConstants.PENDING || status == statusConstants.DRAFT) {
                $formEl.find("#btnSave,#btnSendForApproval").show();
            }
        }
        else if (isApp) {
            if (status == statusConstants.PROPOSED_FOR_APPROVAL) {
                $formEl.find("#btnApproval,#btnReject").show();
            }
        }
        else if (isAck) {
            if (status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE) {
                $formEl.find("#btnAcknowledge,#btnUnacknowledge").show();
            }
        }
    }
    function resetBasicInfo() {
        _actionObj = {
            IsDraft: false,
            IsSendForApproval: false,
            IsApprove: false,
            IsReject: false,
            RejectReason: "",
            IsAcknowledge: false,
            IsUnacknowledge: false,
            UnAcknowledgeReason: ""
        };
    }
})();
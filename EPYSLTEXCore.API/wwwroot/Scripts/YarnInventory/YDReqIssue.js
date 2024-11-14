(function () {
    var menuId,
        pageName;
    var toolbarId,
        pageId;
    var status = statusConstants.PROPOSED;
    var $divTblEl,
        $divDetailsEl,
        $toolbarEl,
        $tblMasterEl,
        $tblChildEl,
        $formEl,
        tblMasterId,
        tblChildId;
       
    var masterData; 
    var isIssueReqPage = false,
        isApprovePage = false,
        isAcknowledgePage = false,
        status = statusConstants.PROPOSED;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblChildTwistingId = "#tblChildTwisting" + pageId;
        $tblColorChildEl = $("#tblColorChild" + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        //Get data ViewBag variable wise 
        isIssueReqPage = convertToBoolean($(`#${pageId}`).find("#IssueRequisitionPage").val());
        isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());
        isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val()); 
        
        if (isIssueReqPage) {
            $toolbarEl.find("#btnPendingList,#btnIssueReqList,#btnPendingforApprovalList,#btnApproveList,#btnRejectList").show();
            $toolbarEl.find("#btnPendingApprovalList,#btnYDIssueReqList,#btnAcknowledgeList").hide();

            $formEl.find("#btnSave,#btnSaveAndSend").show();
            $formEl.find("#btnApprove,#btnAcknowledge,#btnReject").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingList"), $toolbarEl);

            status = statusConstants.PROPOSED;
            isEditable = false;
        }
        else if (isApprovePage) {
            $toolbarEl.find("#btnPendingApprovalList,#btnApproveList,#btnRejectList").show();
            $toolbarEl.find("#btnPendingList,#btnIssueReqList,#btnPendingforApprovalList,#btnYDIssueReqList,#btnAcknowledgeList").hide();

            $formEl.find("#btnApprove,#btnReject").show();
            $formEl.find("#btnSave,#btnSaveAndSend,#btnAcknowledge").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingApprovalList"), $toolbarEl);

            status = statusConstants.AWAITING_PROPOSE;
            isEditable = false;
        }
        else if (isAcknowledgePage) {
            $toolbarEl.find("#btnYDIssueReqList,#btnAcknowledgeList").show();
            $toolbarEl.find("#btnPendingList,#btnIssueReqList,#btnPendingforApprovalList,#btnApproveList,#btnPendingApprovalList,#btnRejectList").hide();

            $formEl.find("#btnAcknowledge").show();
            $formEl.find("#btnSave,#btnSaveAndSend,#btnApprove,#btnReject").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnYDIssueReqList"), $toolbarEl);

            status = statusConstants.APPROVED;
            isEditable = false;
        }

        initMasterTable();

        //Button List
        //btnPendingList = PROPOSED
        //btnIssueReqList = PENDING
        //btnPendingForApprovalList = AWAITING_PROPOSE
        //btnApproveList = APPROVED

        $toolbarEl.find("#btnPendingList,#btnIssueReqList,#btnPendingforApprovalList,#btnPendingApprovalList,#btnApproveList,#btnYDIssueReqList,#btnAcknowledgeList,#btnRejectList").on("click", function (e) {
            var ClickBTN = $(this).attr('id');

            if (isIssueReqPage) {
                if (ClickBTN == 'btnPendingList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.PROPOSED;
                    initMasterTable();
                }
                else if (ClickBTN == 'btnIssueReqList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.PENDING;
                    initMasterTable();
                }
                else if (ClickBTN == 'btnPendingforApprovalList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.AWAITING_PROPOSE;
                    initMasterTable();
                }
                else if (ClickBTN == 'btnApproveList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initMasterTable();
                }
                else if (ClickBTN == 'btnRejectList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.REJECT;
                    initMasterTable();
                }
            }
            else if (isApprovePage) {
                if (ClickBTN == 'btnPendingApprovalList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.AWAITING_PROPOSE;
                    initMasterTable();
                }
                else if (ClickBTN == 'btnApproveList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initMasterTable();
                }
                else if (ClickBTN == 'btnRejectList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.REJECT;
                    initMasterTable();
                }
            }
            else if (isAcknowledgePage) {
                if (ClickBTN == 'btnYDIssueReqList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initMasterTable();
                }
                if (ClickBTN == 'btnAcknowledgeList') {
                    e.preventDefault();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.ACKNOWLEDGE;
                    initMasterTable();
                }
            }
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(false);
        });

        $formEl.find("#btnSaveAndSend").click(function (e) {
            e.preventDefault();
            save(true);
        });

        $formEl.find("#btnApprove,#btnAcknowledge,#btnReject").click(function (e) {
            var ClickBTN = $(this).attr('id');

            var IsApprove = false,
                IsAcknowledge = false,
                IsReject = false;

            if (ClickBTN == 'btnApprove') {
                e.preventDefault();
                IsApprove = true;
                SaveProcess(IsApprove, IsAcknowledge, IsReject)
            }
            else if (ClickBTN == 'btnAcknowledge') {
                e.preventDefault();
                IsAcknowledge = true;
                SaveProcess(IsApprove, IsAcknowledge, IsReject)
            }
            else if (ClickBTN == 'btnReject') {
                e.preventDefault();
                IsReject = true;
                SaveProcess(IsApprove, IsAcknowledge, IsReject)
            }
        });

        $formEl.find("#btnCancel").on("click", backToList);
    });

    function initMasterTable() {
        
        var columns = [];
        if (status == statusConstants.PROPOSED) { // Data get to the booking table
            columns = [
                {
                    headerText: 'Commands', width: 80, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Yarn Dyeing Requisition Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }]
                },
                {
                    field: 'YDReqNo', headerText: 'YD Req. No'
                },
                {
                    field: 'YDBookingNo', headerText: 'YD Booking No'
                },
                {
                    field: 'YDReqDate', headerText: 'YD Req. Date', type: 'date', format: _ch_date_format_1
                },
                {
                    field: 'BuyerName', headerText: 'Buyer Name'
                },
                {
                    field: 'Remarks', headerText: 'Remarks'
                },
                {
                    field: 'ReqQty', headerText: 'Req. Qty'
                }
            ];
        }
        else {
            if (isIssueReqPage) {
                if (status == statusConstants.PENDING) {
                    columns = [
                        {
                            headerText: 'Commands', width: 80, commands: [
                                { type: 'Edit', visible: true, buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                                { type: 'Yarn Dyeing Requisition Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                            ]
                        }];
                }
                if (status == statusConstants.AWAITING_PROPOSE || status == statusConstants.APPROVED || status == statusConstants.REJECT) {
                    columns = [
                        {
                            headerText: 'Commands', width: 80, commands: [
                                { type: 'View', visible: true, buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                                { type: 'Yarn Dyeing Requisition Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                            ]
                        }
                    ];
                }
            }
            else if (isApprovePage) {
                if (status == statusConstants.AWAITING_PROPOSE) {
                    columns = [
                        {
                            headerText: 'Commands', width: 80, commands: [
                                { type: 'Edit', visible: true, buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                                { type: 'Yarn Dyeing Requisition Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                            ]
                        }];
                }
                if (status == statusConstants.APPROVED || status == statusConstants.REJECT) {
                    columns = [
                        {
                            headerText: 'Commands', width: 80, commands: [
                                { type: 'View', visible: true, buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                                { type: 'Yarn Dyeing Requisition Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                            ]
                        }];
                }
            }
            else if (isAcknowledgePage) {
                if (status == statusConstants.APPROVED) {
                    columns = [
                        {
                            headerText: 'Commands', width: 80, commands: [
                                { type: 'Edit', visible: true, buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                                { type: 'Yarn Dyeing Requisition Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                            ]
                        }];
                }
                if (status == statusConstants.ACKNOWLEDGE) {
                    columns = [
                        {
                            headerText: 'Commands', width: 80, commands: [
                                { type: 'View', visible: true, buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                                { type: 'Yarn Dyeing Requisition Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                            ]
                        }];
                }
            }

            var additionalColumns = [
                {
                    field: 'YDReqNo', headerText: 'YD Req No', width: 100
                },
                {
                    field: 'YDReqDate', headerText: 'YD Req Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100
                },
                {
                    field: 'ReqByUser', headerText: 'YD Req By', width: 100
                },
                {
                    field: 'ReqQty', headerText: 'Req Qty', width: 100
                },
                {
                    field: 'YDReqIssueNo', headerText: 'YD Req. Issue No', visible: isIssueReqPage||isApprovePage
                },
                {
                    field: 'YDReqIssueDate', headerText: 'YD Req. Issue Date', type: 'date', format: _ch_date_format_1, visible: isIssueReqPage || isApprovePage
                },
                //{
                //    field: 'IssueQty', headerText: 'Issue Qty'
                //},
                //{
                //    field: 'AcknowledgeName', headerText: 'Acknowledge By'
                //},
                {
                    field: 'SendForApproveName', headerText: 'Send For Approve By', visible: isIssueReqPage || isApprovePage
                },
                {
                    field: 'ApproveName', headerText: 'Approved By', visible: isIssueReqPage || isApprovePage
                },
                {
                    field: 'RejectBy', headerText: 'Reject By', visible: isIssueReqPage || isApprovePage
                },
                {
                    field: 'ConceptNo', headerText: 'Concept No', width: 100
                },
                {
                    field: 'YDBookingNo', headerText: 'Booking No', width: 100
                },
                {
                    field: 'YDBookingDate', headerText: 'Booking Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100
                },
                {
                    field: 'BookingByUser', headerText: 'Booking By', width: 100
                },
                {
                    field: 'BuyerName', headerText: 'Buyer', width: 100
                },
                {
                    field: 'Remarks', headerText: 'Remarks'
                }
               
                /*{
                    field: 'YDReqIssueNo', headerText: 'YD Req. Issue No'
                },
                {
                    field: 'YDReqIssueDate', headerText: 'YD Req. Issue Date', type: 'date', format:_ch_date_format_1
                }, 
                {
                    field: 'ReqQty', headerText: 'Req. Qty'
                },
                {
                    field: 'IssueQty', headerText: 'Issue Qty'
                },
                {
                    field: 'SendForApproveName', headerText: 'Send For Approve By'
                },
                {
                    field: 'ApproveName', headerText: 'Approved By'
                },
                {
                    field: 'AcknowledgeBy', headerText: 'Acknowledge By'
                },
                {
                    field: 'RejectBy', headerText: 'Reject By'
                },
                {
                    field: 'Remarks', headerText: 'Remarks'
                }*/

            ];
            columns.push.apply(columns, additionalColumns);
        }

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/yd-req-issue/list/${status}/${pageName}`,
            columns: columns,
            autofitColumns: false,
            commandClick: handleCommands
        });
    } 
    function handleCommands(args) {
        
        
        if (args.commandColumn.type == 'Yarn Dyeing Requisition Report') {
            window.open(`/reports/InlinePdfView?ReportName=YDYarnRequisitionSlip.rdl&RequisitionID=${args.rowData.YDReqMasterID}`, '_blank');
        } else {
            if (status === statusConstants.PROPOSED && isAcknowledgePage == false) {
                getNew(args.rowData.YDReqMasterID);
            }
            else if (isAcknowledgePage) {
                getYDReqDetails(args.rowData.YDReqMasterID);
            }
            else {
                getDetails(args.rowData.YDReqIssueMasterID);
            }
        }
    }

    async function initChildTable(data) {
        
        isEditable = true;
        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];
        if (isIssueReqPage) {
            columns.push(
                {
                    headerText: 'Commands', width: 120, commands: [
                        {
                            type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' }
                        },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
            );
            columns.push.apply(columns, await getYarnItemColumnsForDisplayOnly());
            var additionalColumns = [
                { field: 'YDReqIssueChildID', isPrimaryKey: true, visible: false },
                { field: 'ShadeCode', headerText: 'Shade Code'},
                { field: 'NoOfThread', headerText: 'No of Thread'}, 
                { field: 'ReqQty', headerText: 'Req. Qty', allowEditing: false },
                { field: 'IssueQty', headerText: 'Issue Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
                { field: 'IssueQtyCone', headerText: 'Issue Qty(Cone)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
                { field: 'IssueQtyCarton', headerText: 'IssueQty(Cart)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
                { field: 'Rate', headerText: 'Rate', editType: "numericedit", edit: { params: { showSpinButton: false, showSpinButton: false, decimals: 0, min: 1 } } },
                { field: 'PhysicalCount', headerText: 'Physical Count' },
                { field: 'LotNo', headerText: 'Lot No' }, 
                {
                    field: 'YarnBrandID', headerText: 'Spinner', valueAccessor: ej2GridDisplayFormatter,dataSource: masterData.YarnBrandList,
                        displayField: "YarnBrand", edit: ej2GridDropDownObj({
                        
                    })
                },
                { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false},
                { field: 'Remarks', headerText: 'Remarks' }
            ];
        }
        else if ((isApprovePage) || (isAcknowledgePage)) {
            columns.push.apply(columns, await getYarnItemColumnsForDisplayOnly());
            var additionalColumns = [
                { field: 'YDReqIssueChildID', isPrimaryKey: true, visible: false },
                { field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false},
                { field: 'NoOfThread', headerText: 'No of Thread', allowEditing: false}, 
                { field: 'ReqQty', headerText: 'Req. Qty', allowEditing: false },
                { field: 'IssueQty', headerText: 'Issue Qty', allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
                { field: 'IssueQtyCone', headerText: 'Issue Qty(Cone)', allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
                { field: 'IssueQtyCarton', headerText: 'IssueQty(Cart)', allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
                { field: 'Rate', headerText: 'Rate', editType: "numericedit", allowEditing: false, edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
                { field: 'LotNo', headerText: 'Lot No', allowEditing: false },
                { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false},
                { field: 'Remarks', headerText: 'Remarks', allowEditing: false }
            ];
        }

        columns.push.apply(columns, additionalColumns);

        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.YDReqIssueChildID = getMaxIdForArray(masterData.Childs, "YDReqIssueChildID");
                    args.data.ItemMasterID = getMaxIdForArray(masterData.Childs, "ItemMasterID");
                    args.data.DisplayUnitDesc = "Kg";
                }
                else if (args.requestType === "save") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.YDReqIssueChildID);
                   args.data.BrandName = args.rowData.BrandName;
                    masterData.Childs[index] = args.data;
                }
            },
            //commandClick: childCommandClick,
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false
        };
        tableOptions["editSettings"] = { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true };
        $tblChildEl = new initEJ2Grid(tableOptions);
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
        $formEl.find("#Id").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(YDReqMasterID) {
        
        axios.get(`/api/yd-req-issue/new/${YDReqMasterID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                copiedRecord = null;
                masterData = response.data;
                masterData.YDReqDate = formatDateToDefault(masterData.YDReqDate);
                masterData.YDReqIssueNo = masterData.YDReqIssueNo;
                masterData.YDReqIssueDate = formatDateToDefault(masterData.YDReqIssueDate);
                masterData.YDBookingDate = formatDateToDefault(masterData.YDBookingDate);
                //masterData.YDReqDate = formatDateToDefault(masterData.YDReqDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
                $formEl.find("#btnSave").fadeIn();
                $formEl.find("#btnSaveAndSend").fadeIn();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            }); 
    }

    function getDetails(id) {
        
        axios.get(`/api/yd-req-issue/${id}`)
            .then(function (response) {
                
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                copiedRecord = null;
                masterData = response.data;
                masterData.YDReqDate = formatDateToDefault(masterData.YDReqDate);
                masterData.YDReqIssueNo = masterData.YDReqIssueNo;
                masterData.YDReqIssueDate = formatDateToDefault(masterData.YDReqIssueDate);
                masterData.YDBookingDate = formatDateToDefault(masterData.YDBookingDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);

                if (isIssueReqPage) {
                    if (status == statusConstants.PROPOSED || status == statusConstants.PENDING) {
                        $formEl.find("#btnSave").show();
                        $formEl.find("#btnSaveAndSend").show();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").show();
                        $formEl.find("#pnlRejectReason").hide();
                    }
                    else if (status == statusConstants.AWAITING_PROPOSE || status == statusConstants.APPROVED) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").show();
                        $formEl.find("#pnlRejectReason").hide();
                    }
                    else if (status == statusConstants.REJECT) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").hide();
                        $formEl.find("#pnlRejectReason").show();
                    }
                }
                else if (isApprovePage) {
                    if (status == statusConstants.AWAITING_PROPOSE) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").show();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").show();
                        //Panel
                        $formEl.find("#pnlRemarks").show();
                        $formEl.find("#pnlRejectReason").show();
                    }
                    else if (status == statusConstants.APPROVED) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").show();
                        $formEl.find("#pnlRejectReason").hide();
                    }
                    else if (status == statusConstants.REJECT) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").hide();
                        $formEl.find("#pnlRejectReason").show();
                    }
                }
                else if (isAcknowledgePage) {
                    if (status == statusConstants.APPROVED) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").show();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").show();
                        $formEl.find("#pnlRejectReason").hide();
                    }
                    else if (status == statusConstants.APPROVED) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").show();
                        $formEl.find("#pnlRejectReason").hide();
                    }
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getYDReqDetails(id) {
        
            axios.get(`/api/yd-requisition/${id}`)
                .then(function (response) {
                    
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                copiedRecord = null;
                masterData = response.data;
                masterData.YDReqDate = formatDateToDefault(masterData.YDReqDate);
                masterData.YDBookingDate = formatDateToDefault(masterData.YDBookingDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
                
                if (isIssueReqPage) {
                    if (status == statusConstants.PROPOSED || status == statusConstants.PENDING) {
                        $formEl.find("#btnSave").show();
                        $formEl.find("#btnSaveAndSend").show();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").show();
                        $formEl.find("#pnlRejectReason").hide();
                    }
                    else if (status == statusConstants.AWAITING_PROPOSE || status == statusConstants.APPROVED) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").show();
                        $formEl.find("#pnlRejectReason").hide();
                    }
                    else if (status == statusConstants.REJECT) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").hide();
                        $formEl.find("#pnlRejectReason").show();
                    }
                }
                else if (isApprovePage) {
                    if (status == statusConstants.AWAITING_PROPOSE) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").show();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").show();
                        //Panel
                        $formEl.find("#pnlRemarks").show();
                        $formEl.find("#pnlRejectReason").show();
                    }
                    else if (status == statusConstants.APPROVED) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").show();
                        $formEl.find("#pnlRejectReason").hide();
                    }
                    else if (status == statusConstants.REJECT) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").hide();
                        $formEl.find("#pnlRejectReason").show();
                    }
                }
                else if (isAcknowledgePage) {
                    if (status == statusConstants.APPROVED) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").show();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").show();
                        $formEl.find("#pnlRejectReason").hide();
                    }
                    else if (status == statusConstants.ACKNOWLEDGE) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnApprove").hide();
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnReject").hide();
                        //Panel
                        $formEl.find("#pnlRemarks").show();
                        $formEl.find("#pnlRejectReason").hide();
                    }
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save(IsSendForApprove) {
        
        //Data get for save process
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = $tblChildEl.getCurrentViewRecords();

        ////Validation Set in Master & Child
        //initializeValidation($formEl, validationConstraints);
        //if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        //else hideValidationErrors($formEl);

        if (isValidChildForm(data)) return;

        //Data send to controller
        data.IsSendForApprove = IsSendForApprove;
        data.CompanyID = 0;
        axios.post("/api/yd-req-issue/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function SaveProcess(IsApprove, IsAcknowledge, IsReject) {
        var data = formDataToJson($formEl.serializeArray());
        data.IsApprove = IsApprove;
        data.IsAcknowledge = IsAcknowledge;
        data.IsReject = IsReject;
        if (data.IsAcknowledge == true && data.IsApprove == false && data.IsReject==false) {
            axios.post("/api/yd-req-issue/saveAcknowledge", data)
                .then(function () {
                    toastr.success("Save Process Successfully!");
                    backToList();
                })
                .catch(function (error) {
                    toastr.error(error.response.data.Message);
                });
        }
        else {
            axios.post("/api/yd-req-issue/saveprocess", data)
                .then(function () {
                    toastr.success("Save Process Successfully!");
                    backToList();
                })
                .catch(function (error) {
                    toastr.error(error.response.data.Message);
                });
        }
        
    }
    function isValidChildForm(data) {
        var isValidItemInfo = false;

        $.each(data["Childs"], function (i, el) {
            if (el.IssueQty == "" || el.IssueQty == null || el.IssueQty <= 0) {
                toastr.error("Issue Qty is required.");
                isValidItemInfo = true;
            }
            else if (el.IssueQty > el.ReqQty) {
                toastr.error("Issue Qty must be equal or less than Required Qty");
                isValidItemInfo = true;
            }
            if (el.IssueQtyCone == "" || el.IssueQtyCone == null || el.IssueQtyCone <= 0) {
                toastr.error("Issue Qty(Cone) is required.");
                isValidItemInfo = true;
            }
            if (el.IssueQtyCarton == "" || el.IssueQtyCarton == null || el.IssueQtyCarton <= 0) {
                toastr.error("Issue Qty(Cart) is required.");
                isValidItemInfo = true;
            } 
        });

        return isValidItemInfo;
    }

    var validationConstraints = {
        LocationID: {
            presence: true
        },
        SupplierID: {
            presence: true
        },
        YDReqIssueDate: {
            presence: true
        }
    }
})();
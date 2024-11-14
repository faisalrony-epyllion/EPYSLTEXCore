
(function () {
    var menuId, pageName, menuParam;
    var status = "";
    var $toolbarEl, toolbarId, $pageEl, pageId, $divTblEl, $divDetailsEl, $formEl,
        $tblMasterEl, tblMasterId, $tblChildEl, tblChildId;
    var masterData;
    var maxColDetails = 999;
    var menuType = 0;
    var _paramType = {
        B2BIssueRequestPage: 0,
        B2BIssueRequestPageApprove: 1,
        B2BIssueRequestPageAcknowledge: 2
    };
    var _saveProps = {
        IsDraft: false,
        IsSendForApproval: false,

        IsApp: false,
        IsReject: false,
        RejectReason: "",

        IsAck: false,
        IsUnAck: false,
        UnAckReason: ""
    };

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
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        menuType = localStorage.getItem("B2BIssueRequestPage");
        menuType = parseInt(menuType);
       
        isB2BIssueRequestMenuShowHide();
        $toolbarEl.find("#btnPendingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            CreateToolBarHideShow();
            initMasterTable();
           
        });
        $toolbarEl.find("#btnAddIR").click(function () {
            getNewDataForIssueRequestLoan();
        });
        $toolbarEl.find("#btnDraftList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.DRAFT;
            CreateToolBarHideShow();
            initMasterTable();
        });
        $toolbarEl.find("#btnPendingApprovalList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            CreateToolBarHideShow();
            initMasterTable();
        });
        $toolbarEl.find("#btnApprovedList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            CreateToolBarHideShow();
            initMasterTable();
        });
        $toolbarEl.find("#btnPendingAckList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED_FOR_ACKNOWLEDGE;
            CreateToolBarHideShow();
            initMasterTable();
        });
        $toolbarEl.find("#btnAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACKNOWLEDGE;
            CreateToolBarHideShow();
            initMasterTable();
        });
        $toolbarEl.find("#btnUnAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.UN_ACKNOWLEDGE;
            CreateToolBarHideShow();
            initMasterTable();
        });
        $toolbarEl.find("#btnRejectedList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REJECT;
            CreateToolBarHideShow();
            initMasterTable();
        });


        $toolbarEl.find("#btnRefreshList").on("click", function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
        });

        $formEl.find("#btnCancel").click(function () {
            backToList();
        });
        $formEl.find("#btnSaveAsDraft").click(function (e) {
            resetSavedProps();
            _saveProps.IsDraft = true;
            save();
        });
        $formEl.find("#btnSendForApproval").click(function (e) {
            resetSavedProps();
            _saveProps.IsSendForApproval = true;
            save();
        });
        $formEl.find("#btnApprove").click(function (e) {
            resetSavedProps();
            _saveProps.IsApp = true;
            save();
        });
        $formEl.find("#btnReject").click(function (e) {
            bootbox.prompt("Enter your reject reason:", function (result) {
                if (!result) {
                    return toastr.error("reject reason is required.");
                }
                resetSavedProps();
                _saveProps.IsReject = true;
                _saveProps.RejectReason = result;
                save();
            });
        });
        $formEl.find("#btnAcknowledge").click(function (e) {
            resetSavedProps();
            _saveProps.IsAck = true;
            save();
        });
        $formEl.find("#btnUnAcknowledge").click(function (e) {
            bootbox.prompt("Enter your unacknowledge reason:", function (result) {
                if (!result) {
                    return toastr.error("unacknowledge reason is required.");
                }
                resetSavedProps();
                _saveProps.IsUnAck = true;
                _saveProps.UnAckReason = result;
                save();
            });
        });
    });
    function save() {
        var data = formDataToJson($formEl.serializeArray());

        data.IsModified = data.B2BIssueRequestMasterId > 0 ? true : false;

        data.IsSendForApproval = _saveProps.IsSendForApproval;
        data.IsApproved = _saveProps.IsApp;
        data.IsRejected = _saveProps.IsReject;
        data.RejectReason = _saveProps.RejectReason;
        data.IsAcknowledged = _saveProps.IsAck;
        data.IsUnAcknowledged = _saveProps.IsUnAck;
        data.UnAcknowledgedReason = _saveProps.UnAcknowledgedReason;

        data.Childs = $tblChildEl.getCurrentViewRecords();

        var successMSG = "Save successfully.";
        if (_saveProps.IsSendForApproval)
            successMSG = "Send for approval successfully.";
        else if (_saveProps.IsApp)
            successMSG = "Apporved successfully.";
        else if (_saveProps.IsReject)
            successMSG = "Reject successfully.";
        else if (_saveProps.IsAck)
            successMSG = "Acknowledged successfully.";
        else if (_saveProps.IsUnAck)
            successMSG = "UnAcknowledged successfully.";


        axios.post("/api/b2b-issue-request/save", data)
            .then(function () {
                toastr.success(successMSG);
                successMSG = "";
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }

    function CreateToolBarHideShow() {
        $toolbarEl.find("#btnAddIR").fadeOut();
        if (menuType == _paramType.B2BIssueRequestPage && status == statusConstants.PENDING) 
            $toolbarEl.find("#btnAddIR").fadeIn();

    }
    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#B2BIssueRequestMasterId").val(-1111);
    }

    function resetSavedProps() {
        _saveProps = {
            IsDraft: false,
            IsSendForApproval: false,

            IsApp: false,
            IsReject: false,
            RejectReason: "",

            IsAck: false,
            IsUnAck: false,
            UnAckReason: ""
        };
    }

    function isB2BIssueRequestMenuShowHide() {
        $toolbarEl.find(".btnToolbar").hide();
        $toolbarEl.find("#btnAddIR").fadeOut();
       
        if (menuType == _paramType.B2BIssueRequestPage) {
            status = statusConstants.PENDING;
            $toolbarEl.find("#btnAddIR").fadeIn();
            $toolbarEl.find("#btnPendingList,#btnDraftList,#btnPendingApprovalList,#btnApprovedList,#btnPendingAckList,#btnAcknowledgeList,#btnUnAcknowledgeList,#btnRejectedList").show();
           
        }
        else if (menuType == _paramType.B2BIssueRequestPageApprove) {
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            $toolbarEl.find("#btnPendingApprovalList,#btnApprovedList,#btnRejectedList").show();
        }
        else if (menuType == _paramType.B2BIssueRequestPageAcknowledge) {
            status = statusConstants.APPROVED;
            $toolbarEl.find("#btnPendingAckList,#btnAcknowledgeList,#btnUnAcknowledgeList").show();
        }
        initMasterTable();
    }
    function initMasterTable() {
        var commands = [],
            isVisible = true,
            width = 100;
        //if (status == statusConstants.PENDING) isVisible = false;
        if (status == statusConstants.ACTIVE) {
            width = 200;
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }
            ]
        }
        else {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }
            ]
        }
        var columns = [
            {
                headerText: 'Actions', width: width, textAlign: 'Center', commands: commands, visible: (status != statusConstants.PENDING)
            },
            {
                field: 'YarnDetails', headerText: 'Yarn Details'
            },
            {
                field: 'YarnStockSetId', headerText: 'YarnStockSetId', visible: false
            },
            {
                field: 'YarnCount', headerText: 'Yarn Count', visible: isVisible
            },
            {
                field: 'PhysicalCount', headerText: 'Physical Count', visible: isVisible
            },
            {
                field: 'LotNo', headerText: 'Lot No', visible: isVisible
            },
            {
                field: 'SpinnerID', headerText: 'SpinnerIDo', visible: false
            },
            {
                field: 'SpinnerName', headerText: 'Spinner Name', visible: isVisible
            },
            {
                field: 'StockTypeId', headerText: 'StockTypeId', visible: false
            },
            {
                field: 'StockTypeName', headerText: 'Stock Type Name', visible: isVisible
            },
            {
                field: 'StockQtyKg', headerText: 'Stock Qty Kg', visible: isVisible,textAlign: 'Center', headerTextAlign: 'Center'
            }
            
        ];
        var selectionType = "Single";
        if (status == statusConstants.PENDING) {
            columns.unshift({ type: 'checkbox', width: 20 });
            selectionType = "Multiple";
        }

        if ($tblMasterEl) $tblMasterEl.destroy();

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            allowGrouping: true,
            apiEndPoint: `/api/b2b-issue-request/list?status=${status}`,
            columns: columns,
            //allowSorting: true,
            commandClick: handleCommands,
            allowSelection: status == statusConstants.PENDING,
            selectionSettings: { type: selectionType, checkboxOnly: true, persistSelection: true }
        });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.B2BIssueRequestMasterId, args.rowData.YarnStockSetId);
            $toolbarEl.find("#btnAddIR").fadeOut();
        }

    }
    function getDetails(id, YarnStockSetId) {
        actionBtnHideShow();
        axios.get(`/api/b2b-issue-request/${id}/${YarnStockSetId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                $formEl.find("#divChildInfo").show();
                masterData = response.data;
                masterData.RequestDate = formatDateToDefault(masterData.RequestDate);

                if (masterData.Childs.length > 0) {
                    setFormData($formEl, masterData);
                    initTblB2BIssueRequestChild(masterData.Childs);
                }

               
            })
            .catch(showResponseError);
    }

    function actionBtnHideShow() {
        $formEl.find(".btnAction").hide();
        if (menuType == _paramType.B2BIssueRequestPage) {
            if (status == statusConstants.PENDING || status == statusConstants.DRAFT) {
                $formEl.find("#btnSaveAsDraft,#btnSendForApproval").show();
            }
        }
        else if (menuType == _paramType.B2BIssueRequestPageApprove) {
            if (status == statusConstants.PROPOSED_FOR_APPROVAL) {
                $formEl.find("#btnApprove,#btnReject").show();
            }
        }
        else if (menuType == _paramType.B2BIssueRequestPageAcknowledge) {
            if (status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE) {
                $formEl.find("#btnAcknowledge,#btnUnAcknowledge").show();
            }
        }
    }

    function getNewDataForIssueRequestLoan() {
        var selectedRecords = $tblMasterEl.getSelectedRecords();
        if (selectedRecords.length == 0) {
            toastr.error("Please select row(s)!");
            return;
        }
        var YarnStockSetId = selectedRecords.map(x => x.YarnStockSetId).join(",")
        getNewForIssueRequestLoan(YarnStockSetId);
        actionBtnHideShow();


    }

    function getNewForIssueRequestLoan(YarnStockSetId) {
        axios.get(`/api/b2b-issue-request/new/${YarnStockSetId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                $formEl.find("#divChildInfo").show();
                masterData = response.data;
                masterData.RequestDate = formatDateToDefault(masterData.RequestDate);

                if (masterData.Childs.length > 0) {
                    setFormData($formEl, masterData);
                    initTblB2BIssueRequestChild(masterData.Childs);
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    async function initTblB2BIssueRequestChild(records) {
        if ($tblChildEl) $tblChildEl.destroy();
        ej.base.enableRipple(true);
        var childColumns = [
            {
                headerText: '', width: 80, commands: [
                    {
                        type: 'Edit', title: 'Remove this row',
                        buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' }
                    }]
            },
            { field: 'B2BIssueRequestChildId',  visible: false },
            { field: 'B2BIssueRequestMasterId', visible: false },
            { field: 'YarnStockSetId', isPrimaryKey: true, visible: false },
            { field: 'StockTypeId', visible: false },
            { field: 'YarnDetails', headerText: 'Yarn Details', allowEditing: false},
            { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false },
            { field: 'LotNo', headerText: 'Physical Lot', allowEditing: false },
            { field: 'SpinnerName', headerText: 'Spinner', allowEditing: false },
            { field: 'StockQtyKg', headerText: 'Stock Qty Kg', allowEditing: false },
            { field: 'B2BRequestQtyKg', headerText: 'B2B Request Qty Kg', allowEditing: true }
        ];

        $tblChildEl = new ej.grids.Grid({
            editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            //commandClick: programhandleCommands,
            autofitColumns: true,
            allowResizing: true,
            enableContextMenu: true,
            enableSingleClickEdit: true,
            dataSource: records,
            columns: childColumns
        });
        $tblChildEl.appendTo(tblChildId);
        $tblChildEl.refresh();

    }
})();
(function () {
    var menuId, pageName;
    var status = "";
    var IsYarnReturnApprovedPage = false;
    var $toolbarEl, toolbarId, $pageEl, pageId, $divTblEl, $divDetailsEl, $formEl,
        $tblMasterEl, tblMasterId, $tblChildEl, tblChildId;
    var masterData;
    var _saveProps = {
        IsDraft: false,
        IsSaveAndSendToMCD: false,
        IsApproved: false
    };

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $pageEl = $(`#${pageId}`);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        IsYarnReturnApprovedPage = convertToBoolean($(`#${pageId}`).find("#IsYarnReturnApprovedPage").val());

        if (IsYarnReturnApprovedPage)
            status = statusConstants.PROPOSED;
        else
            status = statusConstants.PENDING;
        ShowHideWithStatus();
        $toolbarEl.find("#btnPendingReturnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            CreateToolBarHideShow();
            initMasterTable();

        });
        $toolbarEl.find("#btnNewCreateReturn").click(function () {
            getNewDataForKYLOReturn();
        });
        $toolbarEl.find("#btnDraftReturnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.DRAFT;
            CreateToolBarHideShow();
            initMasterTable();
        });
        $toolbarEl.find("#btnProposedReturnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED;
            CreateToolBarHideShow();

            initMasterTable();
        });
        $toolbarEl.find("#btnApprovedReturnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            CreateToolBarHideShow();

            initMasterTable();
        });

        $toolbarEl.find("#btnAcknowledgedReturnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.Acknowledge;
            CreateToolBarHideShow();
            initMasterTable();
        });
        $toolbarEl.find("#btnPendingReturnConfirmationList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PendingReturnConfirmation;
            CreateToolBarHideShow();
            initMasterTable();
        });
        $toolbarEl.find("#btnRefreshList").on("click", function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
        });

        $formEl.find("#btnCancel").on("click", backToList);
        $formEl.find("#btnSaveAsDraft").click(function (e) {
            resetSavedProps();
            _saveProps.IsDraft = true;
            save();
        });
        $formEl.find("#btnSaveAndSendToMCD").click(function (e) {
            resetSavedProps();
            _saveProps.IsSaveAndSendToMCD = true;
            save();
        });
        $formEl.find("#btnApproved").click(function (e) {
            resetSavedProps();
            _saveProps.IsApproved = true;
            save();
        });
    });

    function ShowHideWithStatus() {
        $toolbarEl.find(".btnToolbar").hide();
        $toolbarEl.find("#btnNewCreateReturn").fadeOut();
        if (status == statusConstants.PENDING) {
            $toolbarEl.find("#btnNewCreateReturn").fadeIn();
            $toolbarEl.find("#btnPendingReturnList,#btnDraftReturnList,#btnProposedReturnList,#btnApprovedReturnList").show();
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingReturnList"), $toolbarEl);
        }
        else if (status == statusConstants.PROPOSED) {
            $toolbarEl.find("#btnNewCreateReturn").fadeOut();
            $toolbarEl.find("#btnProposedReturnList,#btnApprovedReturnList").show();
            toggleActiveToolbarBtn($toolbarEl.find("#btnProposedReturnList"), $toolbarEl);

        }
        initMasterTable();
    }
    function actionBtnHideShow() {
        $formEl.find(".btnAction").hide();
        if (status == statusConstants.PENDING || status == statusConstants.DRAFT) {
            $formEl.find("#btnSaveAsDraft,#btnSaveAndSendToMCD").show();
            $formEl.find("#btnApproved").hide();
        }
        else if (status == statusConstants.PROPOSED && IsYarnReturnApprovedPage) {
            $formEl.find("#btnSaveAsDraft,#btnSaveAndSendToMCD").hide();
            $formEl.find("#btnApproved").show();
        }
    }
    function initMasterTable() {
        var commands = [],
            isVisible = true,
            width = 200;
        if (status == statusConstants.APPROVED || status == statusConstants.PROPOSED) {
            commands = [
                { type: 'Edit', title: 'View this Return', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }
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
                field: 'KYLOReturnMasterID', headerText: 'KYLOReturnMasterID', visible: false
            },
            {
                field: 'KYIssueChildID', headerText: 'KYIssueChildID', visible: false
            },
            {
                field: 'KYIssueMasterID', headerText: 'KYIssueMasterID', visible: false
            },
            {
                field: 'KYLOReturnNo', headerText: 'Return No', visible: (status != statusConstants.PENDING)
            },
            {
                field: 'KYLOReturnDate', headerText: 'Return Date', type: 'date', format: _ch_date_format_1, visible: (status != statusConstants.PENDING)
            },
            {
                field: 'KYIssueNo', headerText: 'Issue No'
            },
            {
                field: 'BookingNo', headerText: 'Booking No', visible: (status != statusConstants.PENDING)
            },
            {
                field: 'YarnDetails', headerText: 'Yarn Category', width: 80, visible: (status == statusConstants.PENDING)
            },
            {
                field: 'YarnCount', headerText: 'Yarn Count', visible: (status == statusConstants.PENDING)
            },
            {
                field: 'PhysicalCount', headerText: 'Physical Count', visible: (status == statusConstants.PENDING)
            },
            {
                field: 'LotNo', headerText: 'Physical Lot No', visible: (status == statusConstants.PENDING)
            },
            {
                field: 'BatchNo', headerText: 'Batch No', visible: (status == statusConstants.PENDING)
            },
            {
                field: 'BookingNo', headerText: 'Booking No', visible: isVisible
            },
            {
                field: 'Floor', headerText: 'Floor', visible: (status == statusConstants.PENDING)
            },
            {
                field: 'BalanceQuantity', headerText: 'Balance Quantity', visible: (status == statusConstants.PENDING), textAlign: 'Center', headerTextAlign: 'Center'
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
            apiEndPoint: `/api/KYLO-Return/list?status=${status}`,
            columns: columns,
            //allowSorting: true,
            commandClick: handleCommands,
            allowSelection: status == statusConstants.PENDING,
            selectionSettings: { type: selectionType, checkboxOnly: true, persistSelection: true }
        });
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'Edit') {
            //getDetails(args.rowData.KYLOReturnMasterID, args.rowData.KYIssueMasterID);
            getDetails(args.rowData.KYLOReturnMasterID);
            $toolbarEl.find("#btnNewCreateReturn").fadeOut();
        }

    }
    function getDetails(id) {
        actionBtnHideShow();
        axios.get(`/api/KYLO-Return/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                $formEl.find("#divChildInfo").show();
                masterData = response.data;
                masterData.KYLOReturnDate = formatDateToDefault(masterData.KYLOReturnDate);
                //masterData.KYIssueDate = formatDateToDefault(masterData.KYIssueDate);

                if (masterData.Childs.length > 0) {
                    setFormData($formEl, masterData);
                    initTblKYLOReturntChild(masterData.Childs);
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    async function initTblKYLOReturntChild(records) {
        if ($tblChildEl) $tblChildEl.destroy();
        ej.base.enableRipple(true);
        var childColumns = [];
        var IsVisible = (status != statusConstants.PROPOSED && status != statusConstants.APPROVED) || (status == statusConstants.PROPOSED && IsYarnReturnApprovedPage);
        var IsAllowEditing = IsVisible;

        childColumns = [
            {
                headerText: 'Commands', visible: IsVisible, width: 80, commands: [
                    {
                        type: 'Edit', title: 'Edit this row',
                        buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' }
                    }]
            },
            { field: 'KYLOReturnChildID', visible: false },
            { field: 'KYLOReturnMasterID', visible: false },
            { field: 'KYIssueChildID', isPrimaryKey: true, visible: false },
            { field: 'YarnDetails', headerText: 'Yarn Category', allowEditing: false },
            { field: 'YarnCount', headerText: 'Yarn Count', allowEditing: false },
            { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false },
            { field: 'LotNo', headerText: 'Physical Lot', allowEditing: false },
            { field: 'BatchNo', headerText: 'Batch No', allowEditing: false },
            { field: 'ExportOrderNo', headerText: 'EWO', allowEditing: false },
            { field: 'BookingNo', headerText: 'Booking No', allowEditing: false },
            //{ field: 'ProgramNo', headerText: 'Program No', allowEditing: false },
            { field: 'Floor', headerText: 'Floor', allowEditing: false },
            { field: 'BalanceQuantity', headerText: 'Balance Quantity', allowEditing: false },
            { field: 'UseableReturnQtyKG', headerText: 'Useable Return Qty(KG)', allowEditing: IsAllowEditing },
            { field: 'UseableReturnQtyCone', headerText: 'Useable Return Qty(Cone)', allowEditing: IsAllowEditing },
            { field: 'UseableReturnQtyBag', headerText: 'Useable Return Qty(Bag)', allowEditing: IsAllowEditing },
            { field: 'UnuseableReturnQtyKG', headerText: 'Unuseable Return Qty(KG)', allowEditing: IsAllowEditing },
            { field: 'UnuseableReturnQtyCone', headerText: 'Unuseable Return Qty(Cone)', allowEditing: IsAllowEditing },
            { field: 'UnuseableReturnQtyBag', headerText: 'Unuseable Return Qty(Bag)', allowEditing: IsAllowEditing },

        ];

        $tblChildEl = new ej.grids.Grid({
            editSettings: { allowEditing: IsAllowEditing, allowAdding: IsAllowEditing, allowDeleting: IsAllowEditing, mode: "Normal", showDeleteConfirmDialog: IsAllowEditing },
            //commandClick: programhandleCommands,
            autofitColumns: true,
            allowResizing: true,
            enableContextMenu: true,
            enableSingleClickEdit: (status != statusConstants.APPROVED),
            dataSource: records,
            columns: childColumns
        });
        $tblChildEl.appendTo(tblChildId);
        $tblChildEl.refresh();

    }
    function CreateToolBarHideShow() {
        $toolbarEl.find("#btnNewCreateReturn").fadeOut();
        if (status == statusConstants.PENDING)
            $toolbarEl.find("#btnNewCreateReturn").fadeIn();

    }
    function getNewDataForKYLOReturn() {
        var selectedRecords = $tblMasterEl.getSelectedRecords();

        if (selectedRecords.length == 0) {
            toastr.error("Please select row(s)!");
            return;
        }

        var KYIssueChildID = selectedRecords.map(x => x.KYIssueChildID).join(",")
        getNewForKYLOReturn(KYIssueChildID);
        actionBtnHideShow();


    }
    function getNewForKYLOReturn(KYIssueChildID) {

        axios.get(`/api/KYLO-Return/new/${KYIssueChildID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                $formEl.find("#divChildInfo").show();
                masterData = response.data;
                masterData.KYLOReturnDate = formatDateToDefault(masterData.KYLOReturnDate);
                masterData.KYIssueDate = formatDateToDefault(masterData.KYIssueDate);

                if (masterData.Childs.length > 0) {
                    setFormData($formEl, masterData);
                    initTblKYLOReturntChild(masterData.Childs);
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function resetSavedProps() {
        _saveProps = {
            IsDraft: false,
            IsSaveAndSendToMCD: false
        };
    }
    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }
    function resetForm() {
        $formEl.trigger("reset");

        $formEl.find("#KYLOReturnMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data.IsModified = data.KYLOReturnMasterID > 0 ? true : false;
        data.IsSendToMCD = _saveProps.IsSaveAndSendToMCD;
        data.IsApproved = _saveProps.IsApproved;
        data.Childs = $tblChildEl.getCurrentViewRecords();

        data.Childs.map(x => {
            x.BalanceQuantity = getDefaultValueWhenInvalidN(x.BalanceQuantity);
            x.UseableReturnQtyKG = getDefaultValueWhenInvalidN(x.UseableReturnQtyKG);
            x.UseableReturnQtyCone = getDefaultValueWhenInvalidN(x.UseableReturnQtyCone);
            x.UseableReturnQtyBag = getDefaultValueWhenInvalidN(x.UseableReturnQtyBag);
            x.UnuseableReturnQtyKG = getDefaultValueWhenInvalidN(x.UnuseableReturnQtyKG);
            x.UnuseableReturnQtyCone = getDefaultValueWhenInvalidN(x.UnuseableReturnQtyCone);
            x.UnuseableReturnQtyBag = getDefaultValueWhenInvalidN(x.UnuseableReturnQtyBag);
        });

        var successMSG = "Save Successfully.";
        if (_saveProps.IsSaveAndSendToMCD)
            successMSG = "Send to MCD successfully.";
        if (_saveProps.IsApproved)
            successMSG = "Approved successfully.";

        axios.post("/api/KYLO-Return/save", data)
            .then(function () {
                toastr.success(successMSG);
                successMSG = "";
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }


})();

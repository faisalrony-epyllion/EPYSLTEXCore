(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, tblMasterId, tblChildId, $modalPlanningEl;
    var status;
    var pageIdWithHash;
    var masterData;
    var selectedIndex;

    var _childRackBins = [];
    var _selectedKSCReqChildID = 0;
    var _returnChildID = 1000;

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
        pageIdWithHash = "#" + pageId;

        $toolbarEl.find(".btnToolBar").hide();
        $toolbarEl.find("#btnPending,#btnDraft").show();
        initMasterTable();
        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            initMasterTable();
        });
        $toolbarEl.find("#btnDraft").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.DRAFT;
            $toolbarEl.find("#PendingType").fadeOut();
            initMasterTable();
        });
        $formEl.find("#btnCancel").on("click", backToList);
        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });
        $toolbarEl.find("#btnPending").click();
    });
    function initMasterTable() {
        var commandItems = [];
        if (status == statusConstants.PENDING) {
            commandItems = [
                { type: 'Add', title: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } },
            ];
        }
        else if (status == statusConstants.DRAFT) {
            commandItems = [
                { type: 'View', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
            ];
        }
        var columns = [
            {
                headerText: 'Action', textAlign: 'Center', width: ch_setActionCommandCellWidth(commandItems), commands: commandItems
            },
            {
                field: 'YDReturnNo', headerText: 'Return No', textAlign: 'left', visible: status !== statusConstants.PENDING
            },
            {
                field: 'YDReturnDate', headerText: 'Return Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status !== statusConstants.PENDING
            },
            {
                field: 'YDReceiveNo', headerText: 'Receive No', textAlign: 'left', visible: true
            },
            {
                field: 'YDReceiveDate', headerText: 'YD Receive Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: true
            },
            {
                field: 'CompanyName', headerText: 'Company', visible: true, textAlign: 'Center'
            },
            {
                field: 'Remarks', headerText: 'Remarks', visible: true, textAlign: 'left'
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            allowFiltering: true,
            apiEndPoint: `/api/yd-return/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'Add') {
            getNewDetails(args.rowData.YDReceiveMasterID);
        }
        else if (args.commandColumn.type == 'View') {
            getDetails(args.rowData.YDReturnMasterID);
        }
    }
    function getNewDetails(ydReceiveMasterID) {
        axios.get(`/api/yd-return/new/${ydReceiveMasterID}`)
            .then(function (response) {

                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.YDReturnDate = formatDateToDefault(masterData.YDReturnDate);
                masterData.YDReceiveDate = formatDateToDefault(masterData.YDReceiveDate);

                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getDetails(ydReturnMasterID) {
        axios.get(`/api/yd-return/${ydReturnMasterID}`)
            .then(function (response) {

                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.YDReturnDate = formatDateToDefault(masterData.YDReturnDate);
                masterData.YDReceiveDate = formatDateToDefault(masterData.YDReceiveDate);

                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    async function initChildTable(data) {
        data.filter(x => x.YDReturnChildID == 0).map(x => {
            x.YDReturnChildID = _returnChildID++;
        });
        var columns = [
            {
                headerText: 'Commands', textAlign: 'Center', width: 120, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
            },
            {
                field: 'YDReturnChildID', isPrimaryKey: true, visible: false
            },
            {
                field: 'YDReceiveMasterID', headerText: 'YDReceiveMasterID', allowEditing: false, visible: false
            },
            {
                field: 'YDReceiveChildID', headerText: 'YDReceiveChildID', allowEditing: false, visible: false
            },
            {
                field: 'ItemMasterID', headerText: 'ItemMasterID', allowEditing: false, visible: false
            },
            {
                field: 'UnitID', headerText: 'UnitID', allowEditing: false, visible: false
            },
            {
                field: 'YarnCategory', headerText: 'Yarn Category', allowEditing: false, width: 120, textAlign: 'left', visible: true
            },
            {
                field: 'Unit', headerText: 'Unit', allowEditing: false, width: 120, textAlign: 'center', visible: true
            },
            {
                field: 'ReceiveQty', headerText: 'Receive Qty', allowEditing: false, width: 120, textAlign: 'right', visible: true
            },
            {
                field: 'ReceiveCone', headerText: 'Receive Qty Cone', allowEditing: false, width: 120, textAlign: 'right', visible: true
            },
            {
                field: 'ReceiveCarton', headerText: 'Receive Qty Cone', allowEditing: false, width: 120, textAlign: 'right', visible: true
            },
            {
                field: 'ReturnQty', headerText: 'Return Qty', editType: "numericedit", allowEditing: true, width: 120, textAlign: 'right', visible: true,
                edit: { params: { showSpinButton: false, decimals: 2, format: "N2" } }, width: 100
            },
            {
                field: 'ReturnCone', headerText: 'Return Cone', editType: "numericedit", allowEditing: true, width: 120, textAlign: 'right', visible: true,
                edit: { params: { showSpinButton: false, decimals: 2, format: "N2" } }, width: 100
            },
            {
                field: 'ReturnCarton', headerText: 'Return Carton', editType: "numericedit", allowEditing: true, width: 120, textAlign: 'right', visible: true,
                edit: { params: { showSpinButton: false, decimals: 2, format: "N2" } }, width: 100
            },
            {
                field: 'Remarks', headerText: 'Remarks', allowEditing: true, width: 120, textAlign: 'left', visible: true
            },
        ];

        var gridOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "save") {
                    
                }
                else if (args.requestType === "delete") {

                }
            },
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false,
            editSettings: { allowAdding: false, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },

        };
        if ($tblChildEl) $tblChildEl.destroy();
        $tblChildEl = new initEJ2Grid(gridOptions);
    }
    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
    }
    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#YDReturnMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }
    function save() {
        var data = formDataToJson($formEl.serializeArray());
        if (data.CompanyID == 0) {
            toastr.error("Select Company");
            return false;
        }

        var today = new Date();
        today.setHours(0, 0, 0, 0);

        var ydReturnDate = new Date(data.YDReturnDate);
        ydReturnDate.setHours(0, 0, 0, 0);
        if (ydReturnDate > today) {
            return toastr.error("YD Return Date should not greater than today.");
        }

        data.Childs = $tblChildEl.getCurrentViewRecords();
        if (data.Childs.length === 0) {
            return toastr.error("At least 1 item is required.");
        }

        var hasError = false;
        for (var i = 0; i < data.Childs.length; i++) {
            var child = data.Childs[i],
                rowNo = i + 1;

            child.ReturnQty = getDefaultValueWhenInvalidN_Float(child.ReturnQty);
            child.ReturnCone = getDefaultValueWhenInvalidN_Float(child.ReturnCone);
            child.ReturnCarton = getDefaultValueWhenInvalidN_Float(child.ReturnCarton);
            child.Remarks = getDefaultValueWhenInvalidS(child.Remarks);
            child.YarnCategory = getDefaultValueWhenInvalidS(child.YarnCategory);

            if (child.ReturnQty == 0) {
                toastr.error(`Give Return Qty (at row ${rowNo})`);
                hasError = true;
                break;
            }
            if (child.ReturnCone == 0) {
                toastr.error(`Give Return Cone (at row ${rowNo})`);
                hasError = true;
                break;
            }
            if (hasError) break;
        }
        if (hasError) return false;

        axios.post("/api/yd-return/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
                $tblMasterEl.refresh();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    var validationConstraints = {
        YDReturnDate: {
            presence: true
        },
        CompanyID: {
            presence: true
        },
    }
})();
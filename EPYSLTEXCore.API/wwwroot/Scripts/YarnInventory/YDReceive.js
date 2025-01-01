(function () {
    var menuId, pageName, menuParam;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, tblMasterId, tblChildId, $modalPlanningEl;
    var status;
    var pageIdWithHash;
    var masterData;
    var selectedIndex;

    var _childRackBins = [];
    var _selectedKSCReqChildID = 0;
    var _receiveChildID = 1000;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        if (!menuParam)
            menuParam = localStorage.getItem("menuParam");

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
                field: 'YDReceiveNo', headerText: 'Receive No', textAlign: 'left', visible: status !== statusConstants.PENDING
            },
            {
                field: 'YDReceiveDate', headerText: 'YDReceive Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status !== statusConstants.PENDING
            },
            {
                field: 'CompanyName', headerText: 'Company', visible: true, textAlign: 'Center', visible: status !== statusConstants.PENDING
            },
            {
                field: 'YDReqIssueNo', headerText: 'Issue No', visible: true, textAlign: 'left'
            },
            {
                field: 'YDReqIssueDate', headerText: 'Issue Date', type: 'date', textAlign: 'Center', format: _ch_date_format_1, visible: true
            },
            {
                field: 'ChallanNo', headerText: 'Challan No', visible: true, textAlign: 'left',
            },
            {
                field: 'ChallanDate', headerText: 'Challan Date', format: _ch_date_format_1, visible: true, textAlign: 'Center',
            },
            {
                field: 'GPNo', headerText: 'GP No', visible: true, textAlign: 'left',
            },
            {
                field: 'GPDate', headerText: 'GP Date', type: 'date', format: _ch_date_format_1, visible: true, textAlign: 'Center'
            },
            {
                field: 'ReqQty', headerText: 'Req Qty', visible: false
            },
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            allowFiltering: true,
            apiEndPoint: `/api/yd-receive/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'Add') {
            //getNew(args.rowData.YDReceiveMasterID == null ? 0 : args.rowData.YDReceiveMasterID, args.rowData.YDReqIssueMasterID);
            getNewDetails(args.rowData.YDReqIssueMasterID);
        }
        else if (args.commandColumn.type == 'View') {
            getDetails(args.rowData.YDReceiveMasterID);
        }
    }
    function getNewDetails(ydReqIssueMasterID) {
        axios.get(`/api/yd-receive/new/${ydReqIssueMasterID}`)
            .then(function (response) {

                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.YDReqIssueDate = formatDateToDefault(masterData.YDReqIssueDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate);
                masterData.YDReceiveDate = formatDateToDefault(masterData.YDReceiveDate);

                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getDetails(ydReceiveMasterID) {
        axios.get(`/api/yd-receive/${ydReceiveMasterID}`)
            .then(function (response) {

                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.YDReqIssueDate = formatDateToDefault(masterData.YDReqIssueDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate);
                masterData.YDReceiveDate = formatDateToDefault(masterData.YDReceiveDate);

                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    async function initChildTable(data) {
        data.filter(x => x.YDReceiveChildID == 0).map(x => {
            x.YDReceiveChildID = _receiveChildID++;
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
                field: 'YDReceiveChildID', isPrimaryKey: true, visible: false
            },
            {
                field: 'YDRICRBId', headerText: 'YDRICRBId', visible: false
            },
            {
                field: 'YDReqChildID', headerText: 'YDReqChildID', allowEditing: false, visible: false
            },
            {
                field: 'YDReqIssueChildID', headerText: 'YDReqIssueChildID', allowEditing: false, visible: false
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
                field: 'ReqQty', headerText: 'Req Qty', allowEditing: false, width: 120, textAlign: 'right', visible: true
            },
            {
                field: 'IssueQty', headerText: 'Issue Qty', allowEditing: false, width: 120, textAlign: 'right', visible: true
            },
            {
                field: 'IssueQtyCone', headerText: 'Issue Qty Cone', allowEditing: false, width: 120, textAlign: 'right', visible: true
            },
            {
                field: 'IssueQtyCarton', headerText: 'Issue Qty Carton', allowEditing: false, width: 120, textAlign: 'right', visible: true
            },
            //{
            //    field: 'LocationName', headerText: 'Location', allowEditing: false, width: 120, textAlign: 'left', visible: true
            //},
            //{
            //    field: 'RackNo', headerText: 'Rack No', allowEditing: false, width: 80, textAlign: 'left', visible: true
            //},
            {
                field: 'ReceiveQty', headerText: 'Receive Qty', editType: "numericedit", allowEditing: true, width: 120, textAlign: 'right', visible: true,
                edit: { params: { showSpinButton: false, decimals: 2, format: "N2" } }, width: 100
            },
            {
                field: 'ReceiveCone', headerText: 'Receive Cone', editType: "numericedit", allowEditing: true, width: 120, textAlign: 'right', visible: true,
                edit: { params: { showSpinButton: false, decimals: 2, format: "N2" } }, width: 100
            },
            {
                field: 'ReceiveCarton', headerText: 'Receive Carton', editType: "numericedit", allowEditing: true, width: 120, textAlign: 'right', visible: true,
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
        //initMasterTable();
    }
    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#YDReceiveMasterID").val(-1111);
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

        var ydReceiveDate = new Date(data.YDReceiveDate);
        ydReceiveDate.setHours(0, 0, 0, 0);
        if (ydReceiveDate > today) {
            return toastr.error("YD Receive Date should not greater than today.");
        }

        data.Childs = $tblChildEl.getCurrentViewRecords();
        if (data.Childs.length === 0) {
            return toastr.error("At least 1 item is required.");
        }

        var hasError = false;
        for (var i = 0; i < data.Childs.length; i++) {
            var child = data.Childs[i],
                rowNo = i + 1;

            child.ReceiveQty = getDefaultValueWhenInvalidN_Float(child.ReceiveQty);
            child.ReceiveCone = getDefaultValueWhenInvalidN_Float(child.ReceiveCone);
            child.ReceiveCarton = getDefaultValueWhenInvalidN_Float(child.ReceiveCarton);
            child.Remarks = getDefaultValueWhenInvalidS(child.Remarks);
            child.YarnCategory = getDefaultValueWhenInvalidS(child.YarnCategory);

            if (child.ReceiveQty == 0) {
                toastr.error(`Give Receive Qty (at row ${rowNo})`);
                hasError = true;
                break;
            }
            if (child.ReceiveCone == 0) {
                toastr.error(`Give Receive Cone (at row ${rowNo})`);
                hasError = true;
                break;
            }
            if (hasError) break;
        }
        if (hasError) return false;

        axios.post("/api/yd-receive/save", data)
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
        YDReceiveDate: {
            presence: true
        },
        CompanyID: {
            presence: true
        },
    }
})();
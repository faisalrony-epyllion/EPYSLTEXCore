(function () { 
    var menuId,
        pageName;
    var toolbarId,
        pageId;

    var status = statusConstants.PENDING;

    var $divTblEl,
        $divDetailsEl,
        $toolbarEl,
        $tblMasterEl,
        $tblChildEl,
        $formEl,
        tblMasterId,
        tblChildId;

    var masterData;
    var isEditable = false;

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
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId); 

        initMasterTable();

        $toolbarEl.find("#btnPending").on("click", function (e) { 
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl); 
            status = statusConstants.PENDING; 
            initMasterTable(); 
        });

        $toolbarEl.find("#btnList").on("click", function (e) { 
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl); 
            status = statusConstants.COMPLETED; 
            initMasterTable(); 
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnCancel").on("click", backToList);
    });

    function initMasterTable() {
        var columns = [];
        columns = [
            {
                headerText: 'Commands', width: 80, visible: status == statusConstants.PENDING ? true : false,
                commands: [{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
            },
            {
                headerText: 'Commands', width: 80, visible: status == statusConstants.COMPLETED ? true : false, commands: [{
                    type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' }}]
            },
            {
                field: 'SSReturnNo', headerText: 'Return No', visible: status == statusConstants.PENDING
            },
            {
                field: 'SSReturnDate', headerText: 'Return Date', type: 'date', format: _ch_date_format_1, visible: status == statusConstants.PENDING
            },
            {
                field: 'SSReturnReceivedDate', headerText: 'Receive Date', type: 'date', format: _ch_date_format_1, visible: status !== statusConstants.PENDING
            },
            { field: 'ReceiveQty', headerText: 'Rcv Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            { field: 'ReturnQty', headerText: 'Rtn Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            {
                field: 'SSReturnByUser', headerText: 'Return By', visible: status == statusConstants.PENDING
            },
            {
                field: 'ReturnReceivedByUser', headerText: 'Received By', visible: status !== statusConstants.PENDING
            } 
        ]; 
        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/CDA-SS-returnreceive/list?status=${status}`,
            columns: columns,
            autofitColumns: false,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        if (status === statusConstants.PENDING) {
            getNew(args.rowData.SSReturnMasterID);
        }
        else {
            getDetails(args.rowData.SSReturnReceivedMasterID);
        }
    } 
    async function initChildTable(data) { 
        if ($tblChildEl) $tblChildEl.destroy(); 
        var columns = [
            { field: 'SSReturnReceivedChildID', isPrimaryKey: true, visible: false },
            { field: 'ItemName', headerText: 'Item Name', allowEditing: false },
            { field: 'AgentName', headerText: 'Agent Name', allowEditing: false },
            { field: 'Uom', headerText: 'Uom', allowEditing: false },
            { field: 'BatchNo', headerText: 'Batch No', allowEditing: false },
            { field: 'Rate', headerText: 'Rate', allowEditing: isEditable, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            { field: 'ReturnQty', headerText: 'Return Qty', allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            { field: 'ReceiveQty', headerText: 'Rcv Qt', allowEditing: isEditable, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            { field: 'Remarks', headerText: 'Remarks', allowEditing: isEditable },
        ];
        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.SSReturnReceivedChildID = getMaxIdForArray(masterData.Childs, "SSReturnReceivedChildID");
                }
                else if (args.requestType === "save") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.SSReturnReceivedChildID);
                    masterData.Childs[index] = args.data;
                }
            },
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false
        };
        //if (status == statusConstants.PENDING || status == statusConstants.PARTIALLY_COMPLETED) {
        //    tableOptions["toolbar"] = ['Add'];
        //}
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
        $formEl.find("#SSReturnReceivedMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getNew(reqMasterId) {
        axios.get(`/api/CDA-SS-returnreceive/new/${reqMasterId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                copiedRecord = null;
                masterData = response.data;
                isEditable = true;
                $formEl.find("#btnSave").show();
                masterData.SSReturnReceivedDate = formatDateToDefault(masterData.SSReturnReceivedDate);
                masterData.SSReturnDate = formatDateToDefault(masterData.SSReturnDate);
                masterData.SSReqDate = formatDateToDefault(masterData.SSReqDate);
                masterData.SSReceiveDate = formatDateToDefault(masterData.SSReceiveDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    } 
    function getDetails(id) {
        axios.get(`/api/CDA-SS-returnreceive/${id}`)
            .then(function (response) { 
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                copiedRecord = null;
                isEditable = false;
                $formEl.find("#btnSave").hide();
                masterData = response.data;
                masterData.SSReturnReceivedDate = formatDateToDefault(masterData.SSReturnReceivedDate);
                masterData.SSReturnDate = formatDateToDefault(masterData.SSReturnDate);
                masterData.SSReqDate = formatDateToDefault(masterData.SSReqDate);
                masterData.SSReceiveDate = formatDateToDefault(masterData.SSReceiveDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    } 
    function save(approve = false, reject = false) {
        //Data get for save process
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = $tblChildEl.getCurrentViewRecords();

        //Validation Set in Master & Child
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        if (isValidChildForm(data)) return;

        //Data send to controller
      
        axios.post("/api/CDA-SS-returnreceive/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function isValidChildForm(data) {
        var isValidItemInfo = false;

        $.each(data["Childs"], function (i, el) { 
            if (el.ReceiveQty == "" || el.ReceiveQty == null || el.ReceiveQty <= 0) {
                toastr.error("Receive Qty is required.");
                isValidItemInfo = true;
            }
            if (el.ReturnQty == "" || el.ReturnQty == null || el.ReturnQty <= 0) {
                toastr.error("Return Qty is required.");
                isValidItemInfo = true;
            }
            if (el.ReceiveQty > el.ReturnQty) {
                toastr.error("Receive Qty must be less then Return Qty.");
                isValidItemInfo = true;
            }
            if (el.Rate < 0) {
                toastr.error("Rate must be greater than zero(0) or equal.");
                isValidItemInfo = true;
            }
            if (el.Remarks == "" || el.Remarks == null) {
                toastr.error("Remarks is required.");
                isValidItemInfo = true;
            }
        });

        return isValidItemInfo;
    }

    var validationConstraints = {
        CompanyID: {
            presence: true
        }
        //,
        //SSReturnReceivedNo: {
        //    presence: true
        //} 
    }
})();

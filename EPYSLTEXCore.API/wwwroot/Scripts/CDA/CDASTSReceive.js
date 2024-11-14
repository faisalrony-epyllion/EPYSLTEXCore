(function () {
    var menuId,
        pageName;
    var toolbarId,
        pageId;
    
    var $divTblEl,
        $divDetailsEl,
        $toolbarEl,
        $tblMasterEl,
        $tblChildEl,
        $formEl,
        tblMasterId,
        tblChildId; 
    var masterData;
    var status = statusConstants.PENDING; 

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

        $formEl.find("#btnSave").click(save);

        $formEl.find("#btnCancel").on("click", backToList);
    });

     function initMasterTable() {
        var columns = []; 
         columns = [
             {
                 headerText: 'Commands', width: 80, visible: status == statusConstants.PENDING ? true : false, commands: [{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
             }, 
             {
                 headerText: 'Commands', width: 80, visible: status == statusConstants.COMPLETED ? true : false, commands: [{ type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }]
             },
             {
                 field: 'STSReceiveNo', headerText: 'Receive No', visible: status !== statusConstants.PENDING
             },
             {
                 field: 'STSReceiveDate', headerText: 'Received Date', type: 'date', format: _ch_date_format_1, visible: status !== statusConstants.PENDING
             },
             {
                 field: 'STSIssueNo', headerText: 'Issue No'
             },
             {
                 field: 'STSIssueDate', headerText: 'Issue Date', type: 'date', format: _ch_date_format_1
             },
             {
                 field: 'STSIssueByUser', headerText: 'Issue By'
             },
             {
                 field: 'STSReqNo', headerText: 'Req No'
             },
             {
                 field: 'IssueQty', headerText: 'Issue Qty'
             },
             {
                 field: 'ReqQty', headerText: 'Req Qty'
             },
             {
                 field: 'ReqByUser', headerText: 'Req By', visible: status === statusConstants.PENDING
             }
         ]; 

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/CDA-STS-receive/list?status=${status}`,
            columns: columns,
            autofitColumns: false,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        if (status === statusConstants.PENDING) {
            getNew(args.rowData.STSIssueMasterID);
        }
        else {
            getDetails(args.rowData.STSReceiveMasterID);
        }
    } 
     
    async function initChildTable(data) {
        //console.log(data)
        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];
        columns = [
            { field: 'STSReceiveChildID', isPrimaryKey: true, visible: false },
            { field: 'ItemName', headerText: 'Item Name' },
            { field: 'AgentName', headerText: 'Agent Name' },
            { field: 'DisplayUnitDesc', headerText: 'Uom' },
            { field: 'BatchNo', headerText: 'Batch No' },
            { field: 'IssueQty', headerText: 'Issue Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            { field: 'ReqQty', headerText: 'Req. Qty', visible: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            { field: 'ReceiveQty', headerText: 'Rcv Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } }
        ];
       
        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.STSReceiveChildID = getMaxIdForArray(masterData.Childs, "STSReceiveChildID");
                }
                else if (args.requestType === "save") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.STSReceiveChildID);
                    masterData.Childs[index] = args.data;
                }
            },
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false
        };
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
        $formEl.find("#STSReceiveMasterID").val(-1111); 
        $formEl.find("#EntityState").val(4);
    } 
    function getNew(STSIssueMasterId) {
        axios.get(`/api/CDA-STS-receive/new/${STSIssueMasterId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                copiedRecord = null; 
                $formEl.find("#btnSave").fadeIn(); 
                masterData = response.data;
                masterData.STSReceiveDate = formatDateToDefault(masterData.STSReceiveDate);
                masterData.STSReqDate = formatDateToDefault(masterData.STSReqDate);
                masterData.STSIssueDate = formatDateToDefault(masterData.STSIssueDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    } 
    function getDetails(id) {
        axios.get(`/api/CDA-STS-receive/${id}`)
            .then(function (response) { 
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                copiedRecord = null;
                $formEl.find("#btnSave").fadeOut();
                masterData = response.data;
                masterData.STSReqDate = formatDateToDefault(masterData.STSReqDate);
                masterData.STSIssueDate = formatDateToDefault(masterData.STSIssueDate);
                masterData.STSReceiveDate = formatDateToDefault(masterData.STSReceiveDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    } 
    function save() {
        //Data get for save process
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = $tblChildEl.getCurrentViewRecords();

        //Validation Set in Master & Child
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        if (isValidChildForm(data)) return;

        //Data send to controller 
        axios.post("/api/CDA-STS-receive/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function isValidChildForm(data) {
        var isValidItemInfo = false;

        $.each(data["Childs"], function (i, el) {
            if (el.IssueQty == "" || el.IssueQty == null || el.IssueQty <= 0) {
                toastr.error("Issue Qty is required.");
                isValidItemInfo = true;
            }
            if (el.ReceiveQty == "" || el.ReceiveQty == null || el.ReceiveQty <= 0) {
                toastr.error("Receive Qty is required.");
                isValidItemInfo = true;
            }
        });

        return isValidItemInfo;
    }

    var validationConstraints = {
        STSReceiveNo: {
            presence: true
        },
        STSReceiveDate: {
            presence: true
        },
        CompanyID: {
            presence: true
        } 
    } 
})();
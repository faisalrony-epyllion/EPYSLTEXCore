(function () { 
    'use strict'
    var menuId, pageName;
    var toolbarId, pageId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $formEl, $tblChildEl, tblChildId;
    var status;
    var isEditable = false; 
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var masterData; 

    $(function () { 
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $pageEl = $(pageConstants.PAGE_ID_PREFIX + pageId);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId); 

        // Load Event
        initMasterTable(); 

        // Button Click Event
        $toolbarEl.find("#btnReceiveLists").on("click", function (e) {
            e.preventDefault(); 
            //resetTableParams(); 
            initMasterTable(); 
        }); 
        $toolbarEl.find("#btnNew").on("click", getNew);

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnCancel").on("click", backToList);

        // Change Event
        $("#BuyerId").on("select2:select select2:unselect", function (e) {
            if (e.params.type === 'unselect') initSelect2($("#FabricBookingIds"), []);
            else getBookingByBuyer(e.params.data.id);
        }); 
    });

    function initMasterTable() { 
        var columns = []; 
        columns = [{
            headerText: 'Commands', commands: [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
        },
        {
            field: 'LoanIssueNo', headerText: 'Issue No'
        },
        {
            field: 'LoanIssueDate', headerText: 'Issue Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
        },
        {
            field: 'ChallanNo', headerText: 'Challan No'
        },
        //{
        //    field: 'LReceiveDate', headerText: 'Receive Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
        //},
        //{
        //    field: 'ChallanNo', headerText: 'Challan No'
        //},
        {
            field: 'ChallanDate', headerText: 'Challan Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
        },
        {
            field: 'Remarks', headerText: 'Remarks'
        }
        ]; 

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false, 
            apiEndPoint: `/api/yarn-loan-issue/list`,
            columns: columns,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) { 
        getDetails(args.rowData.LIssueMasterID);
        $formEl.find("#btnSave").fadeIn();
    } 

    async function initChildTable(data) {
        isEditable = true;

        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];
        columns = await getYarnItemColumnsAsync(data, isEditable);
        columns.push.apply(columns, [{ field: 'ShadeCode', headerText: 'Shade Code', allowEditing: isEditable }]);

        var additionalColumns = [
            { field: 'LIssueChildID', isPrimaryKey: true, visible: false },
            {
                field: 'SpinnerID', headerText: 'Spinner', valueAccessor: ej2GridDisplayFormatter,dataSource: masterData.SpinnerList,
                    displayField: "SpinnerName", edit: ej2GridDropDownObj({
                    
                })
            },
            { field: 'LotNo', headerText: 'Lot No' },
            { field: 'PhysicalCount', headerText: 'Physical Count' },
            { field: 'Rate', headerText: 'Rate', editType: "numericedit",visible:false, edit: { params: { decimals: 0 } } },
            { field: 'IssueQty', headerText: 'Issue Qty(Kg)', editType: "numericedit", edit: { params: { decimals: 0 } } },
            { field: 'IssueQtyCone', headerText: 'Issue Qty(Cone)', editType: "numericedit", edit: { params: { decimals: 0 } } },
            { field: 'IssueQtyCarton', headerText: 'Issue Qty(Crtn)', editType: "numericedit", edit: { params: { decimals: 0 } } },
            { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks' }
        ];
        columns.push.apply(columns, additionalColumns);
        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            /*showTimeIndicator :false,*/
            actionBegin: function (args) {
                //console.log("requestType");
                //console.log(args.requestType);
                if (args.requestType === "add") {
                    args.data.LIssueChildID = getMaxIdForArray(masterData.Childs, "LIssueChildID");
                    args.data.DisplayUnitDesc = "Kg";
                    args.data.Rate = 0;
                    args.data.IssueQty = 0;
                    args.data.IssueQtyCarton = 0;
                    args.data.IssueQtyCarton = 0;
                    //console.log(args.data);
                }
                else if (args.requestType === "save") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.LIssueChildID);
                    //args.data.CompanyNames = args.rowData.CompanyNames; 

                    //////args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                    ////args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                    ////args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                    ////args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                    ////args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                    ////args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                    ////args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                    ////args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;

                    masterData.Childs[index] = args.data;
                }
            },
            //commandClick: childCommandClick,
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false
        };

        tableOptions["toolbar"] = ['Add'];
        tableOptions["editSettings"] = { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true };
        $tblChildEl = new initEJ2Grid(tableOptions);
    }
    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn(); 
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#LIssueMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    //function resetTableParams() {
    //    tableParams.offset = 0;
    //    tableParams.limit = 10;
    //    tableParams.filter = '';
    //    tableParams.sort = '';
    //    tableParams.order = '';
    //}
   
    function getNew() {
        axios.get("/api/yarn-loan-issue/new")
            .then(function (response) {
                //isEditable = true;
                status = statusConstants.NEW;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();          
                masterData = response.data;
                masterData.LoanIssueDate = formatDateToDefault(masterData.LoanIssueDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate);
                setFormData($formEl, masterData);
                initChildTable([]);
            })
            .catch(showResponseError); 
    }

    function getDetails(id) {
        axios.get(`/api/yarn-loan-issue/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.LoanIssueDate = formatDateToDefault(masterData.LoanIssueDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate);
                setFormData($formEl, masterData); 
                initChildTable(masterData.Childs); 
            })
            .catch(showResponseError);
    } 

    function getBookingByBuyer(buyerId) {
        axios.get(`/api/selectoption/booking-by-buyer/${buyerId}`)
            .then(function (response) {
                initSelect2($("#FabricBookingIds"), response.data);
            })
            .catch(function () {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {       
        //Data get for save process
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = $tblChildEl.getCurrentViewRecords();
        if (data.Childs.length === 0) return toastr.error("At least 1 Yarn items is required.");

        

         //Validation Set in Master & Child
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        if (isValidMasterForm()) return;

        if (isValidChildForm(data)) return;

        //Data send to controller 
        axios.post("/api/yarn-loan-issue/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
                initMasterTable();
            })
            .catch(showResponseError);
    }

    function isValidMasterForm() {
        var isValidItemInfo = false;

        if ($formEl.find("#CompanyID").val() == null || $formEl.find("#CompanyID").val() == '' || $formEl.find("#CompanyID").val() == '0') {
            toastr.error("Loan Provider is required.");
            isValidItemInfo = true;
        }
        else if ($formEl.find("#LocationID").val() == null || $formEl.find("#LocationID").val() == '' || $formEl.find("#LocationID").val() == '0') {
            toastr.error("Location is required.");
            isValidItemInfo = true;
        }

        return isValidItemInfo;
    }

    function isValidChildForm(data) { 
        var isValidItemInfo = false;
        //console.log(data["Childs"]);
        $.each(data["Childs"], function (i, el) {
            if (el.IssueQty == "" || el.IssueQty == null || el.IssueQty <= 0) {
                toastr.error("Issue Qty is required.");
                isValidItemInfo = true;
            }
            else if (el.IssueQtyCarton == "" || el.IssueQtyCarton == null || el.IssueQtyCarton <= 0) {
                toastr.error("Issue Qty(Crtn) is required.");
                isValidItemInfo = true;
            }
            else if (el.IssueQtyCone == "" || el.IssueQtyCone == null || el.IssueQtyCone <= 0) {
                toastr.error("Issue Qty(Cone) is required.");
                isValidItemInfo = true;
            } 
            else if (el.SpinnerID == "" || el.SpinnerID == null) {
                toastr.error("Spinner is required.");
                isvaliditeminfo = true;
            }
        });

        return isValidItemInfo;
    }

    var validationConstraints = {
        ChallanNo: {
            presence: true
        },
        CompanyID: {
            presence: true
        }
    }
})();
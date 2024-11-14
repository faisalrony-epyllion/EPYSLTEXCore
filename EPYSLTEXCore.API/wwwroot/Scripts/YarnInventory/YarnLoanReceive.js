(function () { 
    'use strict'
    var currentChildRowData;
    var menuId, pageName;
    var toolbarId, pageId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $formEl, $tblChildEl, tblChildId;
    var status;
    var isEditable = false;
    var isAcknowledge = false;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }

    var masterData;

    $(function () { 
        if(!menuId)
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

        if (isAcknowledge) {
            $("#btnSave").hide(); 
        }

        //Load Event
        initMasterTable(); 

        //Button Click Event
        $toolbarEl.find("#btnReceiveLists").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;  
            //initMasterTable(); 
        });

        $toolbarEl.find("#btnNew").on("click", getNew);

        $formEl.find("#btnSave").click(function (e) { 
            //Validation Set in Master & Child
            initializeValidation($formEl, validationConstraints);
            if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
            else hideValidationErrors($formEl); 

            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnCancel").on("click", backToList);

         //Change Event
        $("#BuyerId").on("select2:select select2:unselect", function (e) {
            if (e.params.type === 'unselect') initSelect2($("#FabricBookingIds"), []);
            else getBookingByBuyer(e.params.data.LReceiveMasterID);
        }) 
    });
     
    function initMasterTable() { 
        var columns = [
            {
                headerText: 'Commands', commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
            },
            {
                field: 'LReceiveNo', headerText: 'Receive No'
            },
            {
                field: 'LReceiveDate', headerText: 'Receive Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'ChallanNo', headerText: 'Challan No'
            },
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
            apiEndPoint: `/api/yarn-loan-receive/list`,
            columns: columns,
            commandClick: handleCommands
        });

    }
    function handleCommands(args) {  
        getDetails(args.rowData.LReceiveMasterID);
        $formEl.find("#btnSave").fadeIn();
    } 

    async function initChildTable(data) {
        isEditable = true;

        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];
        columns = await getYarnItemColumnsAsync(data, isEditable);
        columns.push.apply(columns, [{ field: 'ShadeCode', headerText: 'Shade Code', allowEditing: isEditable }]);

        var additionalColumns = [
            { field: 'LReceiveChildID', isPrimaryKey: true, visible: false },
            {
                field: 'SpinnerID', headerText: 'Spinner', valueAccessor: ej2GridDisplayFormatter,dataSource: masterData.SpinnerList, displayField: "SpinnerName", edit: ej2GridDropDownObj({
                    
                })
            },
            { field: 'LotNo', headerText: 'Lot No' },
            { field: 'PhysicalCount', headerText: 'Physical Count' },
            { field: 'Rate', headerText: 'Rate', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },
            { field: 'ChallanQty', headerText: 'Challan Qty(kg)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },
            { field: 'ReceiveQty', headerText: 'Receive Qty(Kg)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },
            { field: 'ChallanQtyCone', headerText: 'Challan Qty(Cone)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },
            { field: 'ReceiveQtyCone', headerText: 'Receive Qty(Cone)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },
            { field: 'ChallanQtyCarton', headerText: 'Challan Qty(Crtn)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },
            { field: 'ReceiveQtyCarton', headerText: 'Receive Qty(Crtn)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },
            { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false, columnValues : 'Kg' },
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
                    args.data.LReceiveChildID = getMaxIdForArray(masterData.Childs, "LReceiveChildID"); 
                    args.data.DisplayUnitDesc = "Kg";
                    args.data.Rate = 0;
                    args.data.ChallanQty = 0;
                    args.data.ChallanQtyCarton = 0;
                    args.data.ChallanQtyCone = 0; 
                    args.data.ReceiveQty = 0;
                    args.data.ReceiveQtyCarton = 0;
                    args.data.ReceiveQtyCone = 0; 
                    args.data.DisplayUnitDesc = "Kg";
                    //console.log(args.data);
                }
                else if (args.requestType === "save") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.LReceiveChildID);
                    //args.data.CompanyNames = args.rowData.CompanyNames; 

                    ////args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                    //args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                    //args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                    //args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                    //args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                    //args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                    //args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                    //args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;

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
        toggleActiveToolbarBtn($toolbarEl.find("#btnReceiveLists"), $toolbarEl);
        $divDetailsEl.fadeOut();
        $formEl.find("#LReceiveMasterID").val(-1111);
        resetForm();
        $divTblEl.fadeIn(); 
        initMasterTable(); 
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#LReceiveMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew() { 
        axios.get(`/api/yarn-loan-receive/new`)
            .then(function (response) {
                isEditable = true;
                status = statusConstants.NEW;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut(); 
                masterData = response.data;
                masterData.LReceiveDate = formatDateToDefault(masterData.LReceiveDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate);
                setFormData($formEl, masterData);
                initChildTable([]);
            })
            .catch(showResponseError);
    } 
    function getDetails(id) {  
        var url = `/api/yarn-loan-receive/${id}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.LReceiveDate = formatDateToDefault(masterData.LReceiveDate);
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

    function save(isApprove = false) {
        //Data get for save process
        var data = formElToJson($formEl);
        data["Childs"] = $tblChildEl.getCurrentViewRecords();
        if (data.Childs.length === 0) return toastr.error("At least 1 Yarn items is required.");

        //Child Validation check 
        if (isValidChildForm(data)) return;
        //Data send to controller
        masterData.Approve = isApprove;
        axios.post("/api/yarn-loan-receive/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError); 
    } 
    function isValidChildForm(data) { 
        var isValidItemInfo = false;

        $.each(data["Childs"], function (i, el) { 
            if (el.ChallanQty == "" || el.ChallanQty == null || el.ChallanQty <= 0) {
                toastr.error("Challan Qty is required.");
                isValidItemInfo = true;
            }
            else if (el.ChallanQtyCone == "" || el.ChallanQtyCone == null || el.ChallanQtyCone <= 0) {
                toastr.error("Challan Qty(Cone) is required.");
                isValidItemInfo = true;
            }
            else if (el.ChallanQtyCarton == "" || el.ChallanQtyCarton == null || el.ChallanQtyCarton <= 0) {
                toastr.error("Challan Qty(Crtn) is required.");
                isValidItemInfo = true;
            }
            
            else if (el.ReceiveQtyCarton == "" || el.ReceiveQtyCarton == null || el.ReceiveQtyCarton <= 0) {
                toastr.error("Receive Qty(Crtn) is required.");
                isValidItemInfo = true;
            }
            else if (el.ReceiveQtyCone == "" || el.ReceiveQtyCone == null || el.ReceiveQtyCone <= 0) {
                toastr.error("Receive Qty(Cone) is required.");
                isValidItemInfo = true;
            }
            else if (el.ReceiveQty == "" || el.ReceiveQty == null || el.ReceiveQty <= 0) {
                toastr.error("Receive Qty is required.");
                isValidItemInfo = true;
            }
           
            else if (el.SpinnerID == "" || el.SpinnerID == null) {
                toastr.error("Spinner is required.");
                isValidItemInfo = true;
            }
        });

        return isValidItemInfo;
    }

    var validationConstraints = {
        ChallanNo: {
            presence: true
        },
        LReceiveNo : {
            presence: true
        },
        LReceiveDate: {
            presence: true
        }, 
        TransportTypeID: {
            presence: true
        }, 
        LoanProviderID: {
            presence: true
        },
        TransportMode: {
            presence: true
        } 
    }
})();
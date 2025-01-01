(function () {
    'use strict'
    var currentChildRowData;
    var menuId, pageName;
    var toolbarId, pageId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $formEl, $tblChildEl, tblChildId;
    var status;
    var isEditable = false;
    var isCDALReceivePage = false;
    var isAcknowledgePage = false;

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

        isCDALReceivePage = convertToBoolean($(`#${pageId}`).find("#CDALReceivePage").val());
        isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val());

        $toolbarEl.find("#btnNew,#btnReceiveLists,#btnPendingForAcknowledge,#btnAcknowledgeList").hide();
        $formEl.find("#btnSave,#btnSaveFAck,#btnAcknowledge").hide();

        if (isCDALReceivePage) {
            $toolbarEl.find("#btnNew,#btnReceiveLists,#btnPendingForAcknowledge").show();
            $toolbarEl.find("#btnAcknowledgeList").hide();

            $formEl.find("#btnSave,#btnSaveFAck").show();
            $formEl.find("#btnAcknowledge").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnReceiveLists"), $toolbarEl);
            status = statusConstants.PENDING;
            isEditable = false;
        }
        else if (isAcknowledgePage) {
            $toolbarEl.find("#btnNew,#btnReceiveLists").hide();
            $toolbarEl.find("#btnPendingForAcknowledge,#btnAcknowledgeList").show();

            $formEl.find("#btnSave,#btnSaveFAck").hide();
            $formEl.find("#btnAcknowledge").show();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingForAcknowledge"), $toolbarEl);
            status = statusConstants.PROPOSED_FOR_ACKNOWLEDGE;
            isEditable = false;
        }

        //Load Event
        initMasterTable();

        //Button Click Event
        $toolbarEl.find("#btnReceiveLists").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            initMasterTable();
        });
        $toolbarEl.find("#btnPendingForAcknowledge").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PROPOSED_FOR_ACKNOWLEDGE;
            initMasterTable();
        });
        $toolbarEl.find("#btnAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.ACKNOWLEDGE;
            initMasterTable();
        });

        $toolbarEl.find("#btnNew").on("click", getNew);

        $formEl.find("#btnSave").click(function (e) {
            //Validation Set in Master & Child
            initializeValidation($formEl, validationConstraints);
            if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
            else hideValidationErrors($formEl);

            e.preventDefault();
            save(false);
        });
        $formEl.find("#btnSaveFAck").click(function (e) {
            //Validation Set in Master & Child
            initializeValidation($formEl, validationConstraints);
            if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
            else hideValidationErrors($formEl);

            e.preventDefault();
            save(true);
        });
        
        $formEl.find("#btnAcknowledge").on("click", acknowledge);

        $formEl.find("#btnCancel").on("click", backToList);
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
                field: 'LocationName', headerText: 'Location'
            },
            {
                field: 'CompanyName', headerText: 'Company'
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
            apiEndPoint: `/api/cda-loan-receive/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) { 
        $formEl.find("#btnSave,#btnSaveFAck,#btnAcknowledge").hide();
        
        if (status == statusConstants.PENDING && isCDALReceivePage) {
            $formEl.find("#btnSave,#btnSaveFAck").show();
            $formEl.find("#btnAcknowledge").hide();
        }
        else if (status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE && isAcknowledgePage) {
            $formEl.find("#btnSave,#btnSaveFAck").hide();
            $formEl.find("#btnAcknowledge").show();
        }  

        if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.CDALReceiveMasterID); 
        }
    }

    async function initChildTable(data) {
        isEditable = true;

        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];
        columns = await getYarnItemColumnsAsync(data, isEditable);

        var additionalColumns = [
            { field: 'CDALReceiveChildID', isPrimaryKey: true, visible: false },
            { field: 'BatchNo', headerText: 'Batch No', width: 100 },
            {
                field: 'ExpiryDate', headerText: 'Expiry Date', type: 'date', format: _ch_date_format_1,
                editType: 'datepickeredit', width: 100, textAlign: 'Center'
            },
            { field: 'Rate', headerText: 'Rate', width: 100, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },
            { field: 'ChallanQty', width: 100, headerText: 'Challan Qty(kg)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },
            { field: 'ReceiveQty', width: 100, headerText: 'Receive Qty(Kg)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },
            { field: 'DisplayUnitDesc', width: 100, headerText: 'Unit', allowEditing: false, columnValues: 'Kg' },
            { field: 'Remarks', width: 100, headerText: 'Remarks' }
        ];
        columns.push.apply(columns, additionalColumns);
        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.CDALReceiveChildID = getMaxIdForArray(masterData.Childs, "CDALReceiveChildID");
                    args.data.DisplayUnitDesc = "Kg";
                    args.data.Rate = 0;
                    args.data.ChallanQty = 0;
                    args.data.ReceiveQty = 0;
                    args.data.DisplayUnitDesc = "Kg";
                }
                else if (args.requestType === "save") {
                    ////args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                    //args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                    //args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                    //args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                    //args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                    //args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                    //args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                    //args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;

                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.CDALReceiveChildID);
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
        tableOptions["editSettings"] = {
            allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true
        };
        $tblChildEl = new initEJ2Grid(tableOptions);
    }

    function backToList() { 
        $divDetailsEl.fadeOut();
        $formEl.find("#CDALReceiveMasterID").val(-1111);
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#CDALReceiveMasterID").val(-1111);
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
        axios.get(`/api/cda-loan-receive/new`)
            .then(function (response) {
                isEditable = true;
                //status = statusConstants.NEW;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.LReceiveDate = formatDateToDefault(masterData.LReceiveDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate);
                masterData.MChallanDate = formatDateToDefault(masterData.MChallanDate);
                masterData.GPDate = formatDateToDefault(masterData.GPDate);
                setFormData($formEl, masterData);
                initChildTable([]);
            })
            .catch(showResponseError);
    }
    function getDetails(id) {
        var url = `/api/cda-loan-receive/${id}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.LReceiveDate = formatDateToDefault(masterData.LReceiveDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate);
                masterData.MChallanDate = formatDateToDefault(masterData.MChallanDate);
                masterData.GPDate = formatDateToDefault(masterData.GPDate);
                setFormData($formEl, masterData);

                initChildTable(masterData.Childs);
            })
            .catch(showResponseError);
    }

    function save(isSendForAck) {
        //Data get for save process
        var data = formElToJson($formEl);
        data["Childs"] = $tblChildEl.getCurrentViewRecords();
        if (data.Childs.length === 0) return toastr.error("At least 1 Yarn items is required.");

        //Child Validation check 
        if (isValidChildForm(data)) return;

        //Data send to controller 
        if (isSendForAck) {
            data.InspSend = true;
        } else {
            data.InspSend = false;
        }
       
        axios.post("/api/cda-loan-receive/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function acknowledge() {
        var id = $formEl.find("#CDALReceiveMasterID").val();
        var url = `/api/cda-loan-receive/acknowledge/${id}`;
        axios.post(url)
            .then(function () {
                toastr.success("Acknowledgement successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function isValidChildForm(data) {
        var isValidItemInfo = false;
        console.log(data);

        $.each(data["Childs"], function (i, el) {
            if (el.ChallanQty == "" || el.ChallanQty == null || el.ChallanQty <= 0) {
                toastr.error("Challan Qty is required.");
                isValidItemInfo = true;
            } 
            else if (el.ReceiveQty == "" || el.ReceiveQty == null || el.ReceiveQty <= 0) {
                toastr.error("Receive Qty is required.");
                isValidItemInfo = true;
            }
            else if (el.BatchNo == "" || el.BatchNo == null) {
                toastr.error("Batch No is required.");
                isValidItemInfo = true;
            }
            else if (el.ExpiryDate == "" || el.ExpiryDate == null) {
                toastr.error("Expiry Date is required.");
                isValidItemInfo = true;
            }
        });

        return isValidItemInfo;
    }

    var validationConstraints = {
        ChallanNo: {
            presence: true
        },
        LReceiveNo: {
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
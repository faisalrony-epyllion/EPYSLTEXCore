(function () {
    'use strict'
    var currentChildRowData;
    var menuId, pageName;
    var toolbarId, pageId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $formEl,
        $tblMasterEl, tblMasterId,
        $tblChildEl, tblChildId,
        $tblItemAdjustmentEL, tblItemAdjustmentId;

    var status;
    var isEditable = false;
    var isCDALIssueRRPage = false;
    var isAcknowledgePage = false;
    var childData;
    var selectedIds; 
    var itemAdjustmentList = new Array(); 

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
        tblItemAdjustmentId = "#tblItemAdjustment" + pageId;

        isCDALIssueRRPage = convertToBoolean($(`#${pageId}`).find("#CDALIssueRRPage").val());
        isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val()); 
         
        $toolbarEl.find("#btnNew,#btnIssueRReceiveLists,#btnPendingForAcknowledge,#btnAcknowledgeList").hide();
        $formEl.find("#btnSave,#btnSaveFAck,#btnAcknowledge").hide();

        if (isCDALIssueRRPage) {
            $toolbarEl.find("#btnNew,#btnIssueRReceiveLists,#btnPendingForAcknowledge").show();
            $toolbarEl.find("#btnAcknowledgeList").hide();

            $formEl.find("#btnSave,#btnSaveFAck").show();
            $formEl.find("#btnAcknowledge").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnIssueRReceiveLists"), $toolbarEl);
            status = statusConstants.PENDING;
            isEditable = false;
        }
        else if (isAcknowledgePage) {
            $toolbarEl.find("#btnNew,#btnIssueRReceiveLists").hide();
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
        $toolbarEl.find("#btnIssueRReceiveLists").on("click", function (e) {
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

        $formEl.find("#btnAcknowledge").on("click", Acknowledgment);

        $formEl.find("#btnCancel").on("click", backToList);

        $("#btnAddItemAdjustment").click(addItemAdjustment)
    });

    function initMasterTable() {
        var columns = [
            {
                headerText: 'Commands', commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
            },
            {
                field: 'LIReturnNo', headerText: 'Issue Return No'
            },
            {
                field: 'LIReturnDate', headerText: 'Issue Return Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
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
            apiEndPoint: `/api/cda-loan-issue-rr/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        $formEl.find("#btnSave,#btnSaveFAck,#btnAcknowledge").hide();

        if (status == statusConstants.PENDING && isCDALIssueRRPage) {
            $formEl.find("#btnSave,#btnSaveFAck").show();
            $formEl.find("#btnAcknowledge").hide();
        }
        else if (status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE && isAcknowledgePage) {
            $formEl.find("#btnSave,#btnSaveFAck").hide();
            $formEl.find("#btnAcknowledge").show();
        }

        if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.CDALIssueReturnID);
        }
    }

    async function initChildTable(data) {
        isEditable = true;

        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];
        columns = await getYarnItemColumnsAsync(data, isEditable);

        var additionalColumns = [
            { field: 'CDALIssueReturnChildID', isPrimaryKey: true, visible: false },
            { field: 'CDALIssueReturnID', visible: false }, 
            {
                headerText: '', textAlign: 'Center', width: 40, commands: [
                    { buttonOption: { type: 'CDALReturnAdj', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
                ]
            },
            { field: 'BatchNo', headerText: 'Batch No', width: 100 },
            {
                field: 'ExpiryDate', headerText: 'Expiry Date', type: 'date', format: _ch_date_format_1,
                editType: 'datepickeredit', width: 100, textAlign: 'Center'
            }, 
            { field: 'ChallanQty', width: 100, headerText: 'Challan Qty(Kg)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },
            { field: 'ReceiveQty', width: 100, headerText: 'Receive Qty(Kg)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },           
            { field: 'Rate', headerText: 'Rate', width: 100, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },
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
                    args.data.CDALIssueReturnChildID = getMaxIdForArray(masterData.Childs, "CDALIssueReturnChildID");
                    args.data.DisplayUnitDesc = "Kg";
                    args.data.Rate = 0;
                    args.data.ChallanQty = 0;
                    args.data.ReceiveQty = 0;
                    masterData.Childs.push(args.data);
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

                    var index = masterData.Childs.findIndex(x => x.CDALIssueReturnChildID == args.rowData.CDALIssueReturnChildID);
                    masterData.Childs[index] = args.rowData;
                }
            },
            commandClick: childCommandClick,
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

    async function childCommandClick(e) {  
        if (e.commandColumn.buttonOption.type == 'CDALReturnAdj') {
            initItemAdjustmenTable([]);
            childData = e.rowData; 
            $("#modalItemAdjustment").modal('show');
            childData = e.rowData;
            selectedIds = '';
            $.each(itemAdjustmentList, function (j, obj) {
                if (obj.CDALIssueReturnChildID == childData.CDALIssueReturnChildID) {
                    selectedIds += obj.CDALIRRAdjID + ',';
                }
            });
            selectedIds = selectedIds.substring(0, selectedIds.length - 1); 
            initItemAdjustmenTable(masterData.ChildAdjutment, selectedIds);            
        } 
    } 

    function initItemAdjustmenTable(data, selectedIds) {
        var columns = [
            { field: 'CDALIRRAdjID', isPrimaryKey: true, visible: false },
            { field: 'CDALIssueReturnChildID', visible: false },
            { field: 'CDALIssueReturnID', visible: false },
            { field: 'CDALIssueChildID', visible: false },
            { field: 'ItemMasterID', visible: false },

            { type: 'checkbox', width: 50 },

            { field: 'BatchNo', headerText: 'Batch No', width: 100, allowEditing: false },
            {
                field: 'ExpiryDate', width: 100, headerText: 'Expiry Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false
            }, 
            {
                field: 'AdjustQty', headerText: 'Adjust Qty(Kg)', allowEditing: false, editType: "numericedit",
                textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 2, min: 0, format: "N2" } }
            },
            {
                field: 'Rate', headerText: 'Total Value', editType: "numericedit",
                textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 2, min: 0, format: "N2" } }
            }, 
            { field: 'Segment1ValueDesc', headerText: 'Composition', allowEditing: false, width: '100px' },
            { field: 'Segment2ValueDesc', headerText: 'Yarn Type', allowEditing: false, width: '100px' },
            { field: 'Segment3ValueDesc', headerText: 'Process', allowEditing: false, width: '100px' },
            { field: 'Segment4ValueDesc', headerText: 'Sub Process', allowEditing: false, width: '100px' },
            { field: 'Segment5ValueDesc', headerText: 'Quality Parameter', allowEditing: false, width: '100px' },
            { field: 'Segment6ValueDesc', headerText: 'Count', allowEditing: false, width: '100px' }, 
            { field: 'Segment7ValueDesc', headerText: 'No of Ply', allowEditing: false, width: '100px' }
        ];

        if ($tblItemAdjustmentEL) $tblItemAdjustmentEL.destroy();
        ej.base.enableRipple(true);
        $tblItemAdjustmentEL = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            columns: columns,
            editSettings: {
                allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal",
                showDeleteConfirmDialog: true
            },
            selectionSettings: { type: "Multiple" }, 
            dataBound: function (args) {
                if (selectedIds != undefined) {
                    var ids = selectedIds.split(',');
                    var selIds = [];
                    for (var i = 0; i < ids.length; i++) {
                        selIds.push($tblItemAdjustmentEL.getRowIndexByPrimaryKey(parseInt(ids[i])));
                    }
                    this.selectRows(selIds);
                }
            }
        });
        $tblItemAdjustmentEL.refreshColumns;
        $tblItemAdjustmentEL.appendTo(tblItemAdjustmentId);
    }

    function addItemAdjustment() { 
        var selectedRecords = $tblItemAdjustmentEL.getSelectedRecords(); 
        if (selectedRecords.length == 0) toastr.warning("You didn't select any record.");
        else $("#modalItemAdjustment").modal('hide');
       
        if (selectedRecords.length > 0) {
            itemAdjustmentList = itemAdjustmentList.filter(x => x.CDALIssueReturnChildID != childData.CDALIssueReturnChildID);

            $.each(selectedRecords, function (j, obj) {
                var newChildItem = {
                    CDALIRRAdjID: obj.CDALIRRAdjID,
                    CDALIssueReturnChildID: childData.CDALIssueReturnChildID,
                    CDALIssueReturnID: obj.CDALIssueReturnID,
                    CDALIssueChildID: obj.CDALIssueChildID,
                    ItemMasterID: obj.ItemMasterID,
                    BatchNo: obj.BatchNo,
                    ExpiryDate: obj.ExpiryDate,
                    UnitID: obj.UnitID,
                    AdjustQty: obj.AdjustQty,
                    Rate: obj.Rate 
                };
                itemAdjustmentList.push(newChildItem);
            });  
        }
        console.log(itemAdjustmentList);
    }

    function backToList() { 
        $divDetailsEl.fadeOut();
        $formEl.find("#CDALIssueReturnID").val(-1111);
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
        itemAdjustmentList = [];
        console.log(itemAdjustmentList);
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#CDALIssueReturnID").val(-1111);
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
        axios.get(`/api/cda-loan-issue-rr/new`)
            .then(function (response) {
                isEditable = true;
                //status = statusConstants.NEW;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.LIReturnDate = formatDateToDefault(masterData.LIReturnDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate); 
                masterData.GPDate = formatDateToDefault(masterData.GPDate);
                setFormData($formEl, masterData);
                initChildTable([]); 
                initItemAdjustmenTable(masterData.ChildAdjutment, selectedIds);
                $formEl.find("#divGPNo,#divGPDate").hide();
            })
            .catch(showResponseError);
    }
    function getDetails(id) {
        var url = `/api/cda-loan-issue-rr/${id}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.LIReturnDate = formatDateToDefault(masterData.LIReturnDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate); 
                masterData.GPDate = formatDateToDefault(masterData.GPDate);
                setFormData($formEl, masterData);

                initChildTable(masterData.Childs); 
                itemAdjustmentList = masterData.ChildAdjutment.filter(x => x.CDALIRRAdjID != 0); 
                initItemAdjustmenTable(masterData.ChildAdjutment, selectedIds);
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
            else if (el.ReceiveQty == "" || el.ReceiveQty == null || el.ReceiveQty <= 0) {
                toastr.error("Receive Qty is required.");
                isValidItemInfo = true;
            }
            else if (el.Rate == "" || el.Rate == null || el.Rate <= 0) {
                toastr.error("Rate is required.");
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

        var TotalValue = 0;
        var TotalAdjValue = 0; 
        $.each(data["Childs"], function (i, el) {
            TotalValue = 0;
            TotalAdjValue = 0;
            TotalValue = (parseFloat(el.ReturnQty) * parseFloat(el.Rate));

            $.each(el.ChildAdjutment, function (i, objCAdj) {
                if (el.CDALIssueReturnChildID == objCAdj.CDALIssueReturnChildID) {
                    TotalAdjValue += parseFloat(objCAdj.Rate);
                } 
            });

            if (TotalAdjValue > TotalValue) {
                toastr.error("Total Adjustment Value must be same or less than Return Value.");
                isValidItemInfo = true;
            } 
        });

        return isValidItemInfo;
    }

    var validationConstraints = {  
        LIReturnDate: {
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

    function save(isInspSend) {
        //Data get for save process
        var data = formElToJson($formEl); 
        data.VehichleNo = $formEl.find("#VehichleID").find(":selected").text();  
        data["Childs"] = $tblChildEl.getCurrentViewRecords();
        if (data.Childs.length === 0) return toastr.error("At least 1 Yarn items is required.");

        for (var i = 0; i < data.Childs.length; i++) {
            data.Childs[i].ChildAdjutment = itemAdjustmentList.filter(x => x.CDALIssueReturnChildID == data.Childs[i].CDALIssueReturnChildID);
        }

        //Child Validation check 
        if (isValidChildForm(data)) return;

        //Data send to controller 
        if (isInspSend) {
            data.InspSend = true;
        } else {
            data.InspSend = false;
        } 
         
        axios.post("/api/cda-loan-issue-rr/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function Acknowledgment() {
        var id = $formEl.find("#CDALIssueReturnID").val();
        var url = `/api/cda-loan-issue-rr/acknowledgment/${id}`;
        axios.post(url)
            .then(function () {
                toastr.success("Acknowledgment successfully.");
                backToList();
            })
            .catch(showResponseError);
    }  
})();
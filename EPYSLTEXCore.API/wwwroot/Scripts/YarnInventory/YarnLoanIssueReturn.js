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
    var status = statusConstants.PENDING;

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

        //Load Event
        initMasterTable(); 

        //Button Click Event
        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING; 
            initMasterTable(); 
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED; 
            initMasterTable(); 
        });
        $toolbarEl.find("#btnNew").on("click", getNew);
        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        }); 
        
        $formEl.find("#btnCancel").on("click", backToList);  
    });

    function initMasterTable() {
        var columns = [];
        if (status == statusConstants.PENDING) {
            columns = [{
                headerText: 'Commands', commands: [
                    { type: 'Pending', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
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
            {
                field: 'ChallanDate', headerText: 'Challan Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'Remarks', headerText: 'Remarks'
            }
            ];
        }
        else if (status == statusConstants.COMPLETED) {
            columns = [{
                headerText: 'Commands', commands: [
                    { type: 'Completed', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }]
            },
            {
                field: 'LIssueReturnNo', headerText: 'Return No'
            },
            {
                field: 'LIssueReturnDate', headerText: 'Return Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
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
            {
                field: 'ChallanDate', headerText: 'Challan Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            }, 
            {
                field: 'Remarks', headerText: 'Remarks'
            }
            ];
        }

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/yarn-loan-issue-return/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) { 
        //console.log(args);
        if (args.commandColumn.type == 'Pending') {
            getNew(args.rowData.LIssueMasterID);
        }
        else if (args.commandColumn.type == 'Completed') {
            getDetails(args.rowData.LIssueReturnMasterID);
        }
        
        $formEl.find("#btnSave").fadeIn();
    }
     
    async function initChildTable(data) {
        isEditable = false;
        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];
        columns.push(
            {
                headerText: 'Commands', width: 120, commands: [
                    {
                        type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' }
                    },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                ]
            },
        );
        columns.push.apply(columns, await getYarnItemColumnsForDisplayOnly());
        columns.push.apply(columns, [{ field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false }]);
        var additionalColumns = [
            { field: 'LIssueReturnChildID', isPrimaryKey: true, visible: false },
            {
                field: 'SpinnerID', headerText: 'Spinner', valueAccessor: ej2GridDisplayFormatter,dataSource: masterData.SpinnerList,
                    displayField: "SpinnerName", edit: ej2GridDropDownObj({
                    
                })
            },
            { field: 'LotNo', headerText: 'Lot No', allowEditing: false },
            { field: 'PhysicalCount', headerText: 'Physical Count' },
            { field: 'Rate', headerText: 'Rate', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0} }, allowEditing: false },
            { field: 'IssueQtyCone', headerText: 'Issue Qty(Cone)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0} }, allowEditing: false },
            { field: 'IssueQtyCarton', headerText: 'Issue Qty(Crtn)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } }, allowEditing: false },
            { field: 'ReturnQty', headerText: 'Return Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },
            { field: 'ReturnQtyCone', headerText: 'Return Qty(Cone)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } }},
            { field: 'ReturnQtyCarton', headerText: 'Return Qty(Crtn)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } }},
            { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks' }
        ];
        columns.push.apply(columns, additionalColumns);
        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") { 
                    args.data.LIssueReturnChildID = getMaxIdForArray(masterData.Childs, "LIssueReturnChildID");
                    args.data.Rate = 0;
                    args.data.IssueQtyCone = 0;
                    args.data.IssueQtyCarton = 0;
                    args.data.ReturnQtyCone = 0;
                    args.data.ReturnQtyCarton = 0; 
                    args.data.DisplayUnitDesc = "Kg"; 
                    //console.log(args.data);
                }
                else if (args.requestType === "save") {
                    //console.log(args);
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.LIssueReturnChildID);
                    args.data.SpinnerName = args.rowData.Spinner; 
                    masterData.Childs[index] = args.data;
                }
            },
            //commandClick: childCommandClick,
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false
        };
        
        tableOptions["editSettings"] = { allowAdding: true, allowEditing: true, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true };
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
        $formEl.find("#LIssueReturnMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    } 
     
    function getNew(reqMasterId) {
        var url = `/api/yarn-loan-issue-return/new/?id=${reqMasterId}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.LoanIssueDate = formatDateToDefault(masterData.LoanIssueDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate);
                masterData.LIssueReturnDate = formatDateToDefault(masterData.LIssueReturnDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
            })
            .catch(showResponseError);
    }

    function getDetails(id) {
        var url = `/api/yarn-loan-issue-return/${id}`;
        axios.get(url)
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

    //function getYarnCountByYarnType(yarnTypeId, rowData) {
    //    var url = "/api/selectoption/yarn-count-by-yarn-type/" + yarnTypeId;
    //    axios.get(url)
    //        .then(function (response) {
    //            var yarnCountList = convertToSelectOptions(response.data);
    //            showBootboxSelectPrompt("Select Yarn Count", yarnCountList, "", function (result) {
    //                if (!result)
    //                    return toastr.warning("You didn't selected any Yarn Count.");

    //                var selectedYarnCount = yarnCountList.find(function (el) { return el.value === result })
    //                rowData.Segment2ValueId = result;
    //                rowData.Segment2ValueDesc = selectedYarnCount.text;
    //                rowData.YarnCategory = calculateYarnCategory(rowData);
    //                if ((rowData.Segment1ValueId == 625) || (rowData.Segment1ValueId == 8238)) {
    //                    rowData.NoOfThread = "0";
    //                    isNoOfThread = false;
    //                }
    //                else {
    //                    rowData.NoOfThread = "1";
    //                    isNoOfThread = true;
    //                }
    //                $tblChildEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
    //            })
    //        })
    //        .catch(function (err) {
    //            console.log(err);
    //        });
    //}

    //function getBookingByBuyer(buyerId) {
    //    axios.get(`/api/selectoption/booking-by-buyer/${buyerId}`)
    //        .then(function (response) {
    //            initSelect2($("#FabricBookingIds"), response.data);
    //        })
    //        .catch(function () {
    //            toastr.error(err.response.data.Message);
    //        });
    //}

    function save() { 
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
        //data.Approve = isApprove;
        //data.LoanProviderId = 0;
         
        axios.post("/api/yarn-loan-issue-return/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
                initMasterTable();
            })
            .catch(showResponseError); 
    }

    //function initNewAttachment($el) {
    //    $el.fileinput('destroy');
    //    $el.fileinput({
    //        showUpload: false,
    //        previewFileType: 'any'
    //    });
    //}

    //function initAttachment(path, $el) {
    //    if (!path) {
    //        initNewAttachment($el);
    //        return;
    //    }

    //    var preveiwData = [rootPath + path];
    //    var previewConfig = [{ type: 'image', caption: "Swatch Attachment", key: 1 }];

    //    $el.fileinput('destroy');
    //    $el.fileinput({
    //        showUpload: false,
    //        initialPreview: preveiwData,
    //        initialPreviewAsData: true,
    //        initialPreviewFileType: 'image',
    //        initialPreviewConfig: previewConfig,
    //        purifyHtml: true,
    //        required: true,
    //        maxFileSize: 4096
    //    });
    //}
    function isValidMasterForm() {
        var isValidItemInfo = false;

        if ($formEl.find("#LocationID").val() == null || $formEl.find("#LocationID").val() == '' || $formEl.find("#LocationID").val() == '0') {
            toastr.error("Location is required.");
            isValidItemInfo = true;
        }

        return isValidItemInfo;
    }

    function isValidChildForm(data) {
        var isValidItemInfo = false;

        $.each(data["Childs"], function (i, el) {
            if (el.ReturnQty == "" || el.ReturnQty == null || el.ReturnQty <= 0) {
                toastr.error("Return Qty is required.");
                isValidItemInfo = true;
            }
            else if (el.ReturnQtyCone == "" || el.ReturnQtyCone == null || el.ReturnQtyCone <= 0) {
                toastr.error("Return Qty(Cone) is required.");
                isValidItemInfo = true;
            } 
            else if (el.ReturnQtyCarton == "" || el.ReturnQtyCarton == null || el.ReturnQtyCarton <= 0) {
                toastr.error("Return Qty(Crtn) is required.");
                isValidItemInfo = true;
            } 
            //else if (el.ReceiveQty == "" || el.ReceiveQty == null || el.ReceiveQty <= 0) {
            //    toastr.error("Receive Qty(Crtn) is required.");
            //    isValidItemInfo = true;
            //} 
        });

        return isValidItemInfo;
    }

    var validationConstraints = {
        LIssueReturnNo: {
            presence: true
        },
        ChallanNo: {
            presence: true 
        }
    }
})();
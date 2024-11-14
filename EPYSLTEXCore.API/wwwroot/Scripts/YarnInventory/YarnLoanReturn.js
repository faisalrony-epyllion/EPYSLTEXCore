(function () { 
    'use strict'
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

        isAcknowledge = convertToBoolean($(pageId).find("#Acknowledge").val());
        if (isAcknowledge) {
            $("#btnSave").hide(); 
        }

        initMasterTable(); 

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

        $("#BuyerId").on("select2:select select2:unselect", function (e) {
            if (e.params.type === 'unselect') initSelect2($("#FabricBookingIds"), []);
            else getBookingByBuyer(e.params.data.id);
        });

        //$toolbarEl.find("#btnNew").on("click", getNew); 

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
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
        }
        else if (status == statusConstants.COMPLETED) {
            columns = [{
                headerText: 'Commands', commands: [
                    { type: 'Completed', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }]
            },
            {
                field: 'LReturnNo', headerText: 'Return No'
            },
            {
                field: 'LReturnDate', headerText: 'Return Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
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
        }  
       
        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/yarn-loan-return/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        }); 
    }
    function handleCommands(args) {
        //console.log(args);
        if (args.commandColumn.type == 'Pending') {
            getNew(args.rowData.LReceiveMasterID);
            $formEl.find("#btnSave").fadeIn();
        }
        else if (args.commandColumn.type == 'Completed') {
            getDetails(args.rowData.LReturnMasterID);
            $("#btnSave").hide();
        } 
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
            { field: 'LReturnChildID', isPrimaryKey: true, visible: false },
            {
                field: 'SpinnerID', headerText: 'Spinner', valueAccessor: ej2GridDisplayFormatter, dataSource: masterData.SpinnerList, displayField: "SpinnerName", edit: ej2GridDropDownObj({

                })
            },
            { field: 'LotNo', headerText: 'Lot No', allowEditing: false },
            { field: 'PhysicalCount', headerText: 'Physical Count' },
            { field: 'Rate', headerText: 'Rate', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0} }, allowEditing: false },
            { field: 'LoanQty', headerText: 'Receive Qty(Kg)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } }, allowEditing: false },
            { field: 'ReturnQty', headerText: 'Return Qty(Kg)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },
            { field: 'ReceiveQtyCone', headerText: 'Receive Qty(Cone)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } }, allowEditing: false },
            { field: 'ReturnQtyCone', headerText: 'Return Qty(Cone)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0} } },
            { field: 'ReceiveQtyCarton', headerText: 'Receive Qty(Crtn)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } }, allowEditing: false },
            { field: 'ReturnQtyCarton', headerText: 'Return Qty(Crtn)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } } },
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
                    args.data.LReturnChildID = getMaxIdForArray(masterData.Childs, "LReturnChildID");
                }
                else if (args.requestType === "save") {
                 
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.LReceiveChildID);
                    args.data.Spinner = args.rowData.Spinner;
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
        initMasterTable(); 
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#LReturnMasterID").val(-1111);
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
        var url = `/api/yarn-loan-return/new/?id=${reqMasterId}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.LReceiveDate = formatDateToDefault(masterData.LReceiveDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate);
                masterData.LReturnDate = formatDateToDefault(masterData.LReturnDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
            })
            .catch(showResponseError); 
    }

    function getDetails(id) { 
        var url = `/api/yarn-loan-return/${id}`;       
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.LReceiveDate = formatDateToDefault(masterData.LReceiveDate);
                masterData.LReturnDate = formatDateToDefault(masterData.LReturnDate);
                masterData.ChallanDate = formatDateToDefault(masterData.ChallanDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
            })
            .catch(showResponseError); 
    }

    function getYarnCountByYarnType(yarnTypeId, rowData) {
        var url = "/api/selectoption/yarn-count-by-yarn-type/" + yarnTypeId;
        axios.get(url)
            .then(function (response) {
                var yarnCountList = convertToSelectOptions(response.data);
                showBootboxSelectPrompt("Select Yarn Count", yarnCountList, "", function (result) {
                    if (!result)
                        return toastr.warning("You didn't selected any Yarn Count.");

                    var selectedYarnCount = yarnCountList.find(function (el) { return el.value === result })
                    rowData.Segment2ValueId = result;
                    rowData.Segment2ValueDesc = selectedYarnCount.text;
                    rowData.YarnCategory = calculateYarnCategory(rowData);
                    if ((rowData.Segment1ValueId == 625) || (rowData.Segment1ValueId == 8238)) {
                        rowData.NoOfThread = "0";
                        isNoOfThread = false;
                    }
                    else {
                        rowData.NoOfThread = "1";
                        isNoOfThread = true;
                    }
                    $tblChildEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
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
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = $tblChildEl.getCurrentViewRecords();
      /*  data["Childs"] = masterData.Childs; */
        //console.log(data["Childs"]); 

        //Validation Set in Master & Child
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        if (isValidChildForm(data)) return;

        //Data send to controller
        masterData.Approve = isApprove;
        //console.log(masterData.Childs);
        axios.post("/api/yarn-loan-return/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function initNewAttachment($el) {
        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            previewFileType: 'any'
        });
    }

    function initAttachment(path, $el) {
        if (!path) {
            initNewAttachment($el);
            return;
        }

        var preveiwData = [rootPath + path];
        var previewConfig = [{ type: 'image', caption: "Swatch Attachment", key: 1 }];

        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            initialPreview: preveiwData,
            initialPreviewAsData: true,
            initialPreviewFileType: 'image',
            initialPreviewConfig: previewConfig,
            purifyHtml: true,
            required: true,
            maxFileSize: 4096
        });
    }

    function isValidChildForm(data) {
       
        var isValidItemInfo = false;

        $.each(data["Childs"], function (i, el) {
            if (el.ReturnQtyCarton == "" || el.ReturnQtyCarton == null || el.ReturnQtyCarton <= 0) {
                toastr.error("Return Qty(Crtn) is required.");
                isValidItemInfo = true;
            }
            else if (el.ReturnQtyCone == "" || el.ReturnQtyCone == null || el.ReturnQtyCone <= 0) {
                toastr.error("Return Qty(Cone) is required.");
                isValidItemInfo = true;
            }
            else if (el.ReturnQty == "" || el.ReturnQty == null || el.ReturnQty <= 0) {
                toastr.error("Return Qty(Cone) is required.");
                isValidItemInfo = true;
            } 
        });

        return isValidItemInfo;
    }

    var validationConstraints = {
        LReturnNo: {
            presence: true
        },
        ChallanNo: {
            presence: true
        }
    }
})();
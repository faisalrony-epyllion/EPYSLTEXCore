
(function () {
    var menuId, pageName, menuParam;
    var status = "";
    var toolbarId, pageId, $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, tblMasterId, tblChildId, $tblOtherItemEl, tblOtherItemId, tblCreateCompositionId, $tblCreateCompositionEl, $tblChildSetEl, tblChildSetId, $tblChildSetDetailsEl, tblChildSetDetailsId, $tblYarnPIItemsId, $tblYarnPIItemsEl, tblYarnPIItemsId, $tblPaymentDetailsId, $tblPaymentDetailsEl, tblPaymentDetailsId;
    var masterData, _currentRowForDD = {};
    var maxColDetails = 999;

    var IsCDA = false;
    var validationConstraints = {
    };

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        if (!menuParam)
            menuParam = localStorage.getItem("menuParam");
        if (menuParam == "CDA")
            IsCDA = true;
        else
            IsCDA = false;

        status = statusConstants.PENDING;

        pageId = pageName + "-" + menuId;
        $pageEl = $(`#${pageId}`);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblYarnPIItemsId = "#tblYarnPIItems" + pageId;
        tblPaymentDetailsId = "#tblPaymentDetailsId" + pageId;

        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);


        initMasterTable();
        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            initMasterTable();
        });
        $toolbarEl.find("#btnDraftList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.EDIT;
            initMasterTable();
        });
        $formEl.find("#btnCancel").on("click", backToList);
        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(false);
        });

        $formEl.find("#TotalPaymentedValue").on("keypress", setToAttachedInvoice);


    });
    function setToAttachedInvoice() {
        var PIItemList = $tblYarnPIItemsEl.getCurrentViewRecords();
        var sumPaymentValue = 0;
        var newValue = 0.00;
        var nValue = 0.00;
        nValue = parseFloat($('#TotalPaymentedValue').val());
        for (var i = 0; i < PIItemList.length; i++) {
            var InvoiceValue = 0.00, PaymentedValue = 0.00;
            InvoiceValue = PIItemList[i].InvoiceValue;
            PaymentedValue = PIItemList[i].PaymentedValue;
            newValue = parseFloat(nValue - parseFloat(InvoiceValue - PaymentedValue));
            if (newValue >= 0) {
                PIItemList[i].PaymentValue = parseFloat(InvoiceValue - PaymentedValue);
                sumPaymentValue += parseFloat(InvoiceValue - PaymentedValue);
            }
            else if (nValue > 0) {
                PIItemList[i].PaymentValue = parseFloat(nValue);
                sumPaymentValue += nValue;
            }
            else {
                PIItemList[i].PaymentValue = 0;

            }
            nValue = newValue;
            var PaymentValue = PIItemList[i].PaymentValue;
            var BalanceAmount = parseFloat(parseFloat(InvoiceValue) - (parseFloat(PaymentValue) + parseFloat(PaymentedValue))).toFixed(2);

            BalanceAmount = parseFloat(BalanceAmount);
            PIItemList[i].BalanceAmount = BalanceAmount;


        }

        initAttachedPIChildTable(PIItemList);
        $tblYarnPIItemsEl.refresh();
        $('#TotalPaidValue').val(sumPaymentValue);

    }

    function initMasterTable() {
        var commands = [];
        if (status == statusConstants.PENDING) {
            commands = [
                { type: 'New', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } }
            ]
        }
        else {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-edit' } },
                { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
            ]
        }

        var columns = [
            {
                headerText: 'Actions', commands: commands, width: 20
            },
            { field: 'IIPMasterID', headerText: 'IIPMasterID', visible: false },
            { field: 'CompanyID', headerText: 'CompanyID', visible: false },
            { field: 'SupplierID', headerText: 'SupplierID', visible: false },
            { field: 'IIPMasterNo', headerText: 'Pyment No', width: 30, visible: status === statusConstants.EDIT },
            { field: 'BankRefNumber', headerText: 'Bank Ref No', width: 30 },
            { field: 'CustomerName', headerText: 'Business Unit', width: 30 },
            { field: 'SupplierName', headerText: 'Supplier Name', width: 30 },
            { field: 'PaymentBank', headerText: 'L/C Opening Bank', width: 30 },
            { field: 'CIValue', headerText: 'Invoice Value', width: 30 },
            { field: 'TotalPaymentValue', headerText: 'Paid Value', width: 30 },
            { field: 'BankAcceptDate', headerText: 'Bank Accepted Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 30 },
            { field: 'MaturityDate', headerText: 'Maturity Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 20 }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            ////allowGrouping: true,
            apiEndPoint: `/yiipAPI/list?status=${status}&isCDAPage=${IsCDA}`,
            columns: columns,
            allowSorting: true,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'New') {
            getNew(args.rowData.BankRefNumber, args.rowData.CompanyID, args.rowData.SupplierID);
            internalBtnHideShow(args.commandColumn.type);
        }
        else if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.IIPMasterID);
            internalBtnHideShow(args.commandColumn.type);   
        }
        else if (args.commandColumn.type == 'View') {
            isEditable = false;
            getDetails(args.rowData.IIPMasterID);
            internalBtnHideShow(args.commandColumn.type);
        }

    }
    function getNew(BankRefNumber, CompanyId, SupplierId) {
        axios.get(`/yiipAPI/new/${BankRefNumber}/${CompanyId}/${SupplierId}/${IsCDA}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.PaymentDate = formatDateToDefault(masterData.PaymentDate);
                masterData.BankAcceptDate = formatDateToDefault(masterData.BankAcceptDate);
                masterData.MaturityDate = formatDateToDefault(masterData.MaturityDate);
                setFormData($formEl, masterData);

                if (status == statusConstants.PENDING) {
                    $formEl.find("#btnSave").fadeIn();
                    //$formEl.find("#btnAccept,#btnReject").fadeOut();
                }
                initAttachedPIChildTable(masterData.IPChilds);
                initPaymentDetailsTable(masterData.IPDetails);

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getDetails(IIPMasterID) {
        axios.get(`/yiipAPI/ipEdit/${IIPMasterID}/${IsCDA}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.PaymentDate = formatDateToDefault(masterData.PaymentDate);
                masterData.BankAcceptDate = formatDateToDefault(masterData.BankAcceptDate);
                masterData.MaturityDate = formatDateToDefault(masterData.MaturityDate);
                setFormData($formEl, masterData);

                initAttachedPIChildTable(masterData.IPChilds);
                initPaymentDetailsTable(masterData.IPDetails);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function internalBtnHideShow(buttonType) {
        $formEl.find("#btnSave").fadeOut();
        if (buttonType == "Edit" || buttonType =="New") {
            $formEl.find("#btnSave").fadeIn();
        }
    }

    function initAttachedPIChildTable(data) {
        isEditable = true;
        if ($tblYarnPIItemsEl) {
            $tblYarnPIItemsEl.destroy();
            $(tblYarnPIItemsId).html("");
        }

        ej.base.enableRipple(true);
        $tblYarnPIItemsEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            editSettings: { allowAdding: true, allowDeleting: true, allowEditing: false },
            allowResizing: true,
            allowEditing: false,
            primaryKeyColumn: "InvoiceNo",
            columns: [
                { field: 'IIPChildID', isPrimaryKey: true, visible: false },
                { field: 'InvoiceID', visible: false },
                { field: 'InvoiceNo', headerText: 'Invoice No', width: 20 },
                { field: 'InvoiceDate', headerText: 'Invoice Date', type: 'date', format: _ch_date_format_1, width: 20 },
                { field: 'InvoiceValue', headerText: 'Invoice Value', width: 15, textAlign: 'Right' },
                { field: 'PaymentValue', headerText: 'Payment Value', width: 15, textAlign: 'Right' },
                { field: 'PaymentedValue', headerText: 'Paid Value', width: 15, textAlign: 'Right' },
                { field: 'BalanceAmount', headerText: 'Balance', width: 15, textAlign: 'Right' }
            ]
        });
        $tblYarnPIItemsEl["dataBound"] = function () {
            $tblYarnPIItemsEl.autoFitColumns();
        };
        $tblYarnPIItemsEl.refreshColumns;
        $tblYarnPIItemsEl.appendTo(tblYarnPIItemsId);
    }
    function initPaymentDetailsTable(data) {
        var CalculatOnList = [
            { id: 1, text: "FC" },
            { id: 2, text: "LC" },
            { id: 3, text: "Rate" }
        ];
        isEditable = true;
        if ($tblPaymentDetailsId) $tblPaymentDetailsId.destroy();
        ej.base.enableRipple(true);
        $tblPaymentDetailsId = new ej.grids.Grid({
            dataSource: data,
            editSettings: { allowEditing: false, allowAdding: false, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: false },

            allowResizing: false,
            //autofitColumns: false,
            //showColumnChooser: true,
            //showDefaultToolbar: false,
            actionBegin: function (args) {
            },
            columns: [
                //{ field: 'IIPMasterID', isPrimaryKey: true, visible: false },
                { field: 'SGHeadID', isPrimaryKey: true, visible: false, allowEditing: false },
                { field: 'CTCategoryID', visible: false, allowEditing: false },
                { field: 'DHeadNeed', visible: false, allowEditing: false },
                { field: 'SHeadNeed', width: 20, visible: false, allowEditing: false },
                { field: 'SGHeadName', headerText: 'Head Group', allowEditing: false, width: 100  }

            ],
            childGrid: {
                queryString: 'SGHeadID',
                allowResizing: true,
                autofitColumns: false,
                toolbar: ['Add'],
                //toolbar: [{ text: 'Add Item', tooltipText: 'Add Item', prefixIcon: 'e-icons e-add', id: 'addItem' }],
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: [
                    {
                        headerText: 'Commands', textAlign: 'Center', width: 120, commands: [
                            { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                            { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                            { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                            { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
                    },
                    { field: 'SGHeadID', isPrimaryKey: true, visible: false, allowEditing: false },
                    { field: 'IIPDetailsID', visible: false, allowEditing: false },
                    {
                        field: 'DHeadID', headerText: 'Head Description', allowEditing: isEditable, required: true, width: 350, valueAccessor: ej2GridDisplayFormatter, dataSource: masterData.HeadDescriptionList,
                        displayField: "DHeadName", edit: ej2GridDropDownObj({
                            width: 350
                        })
                    },
                    {
                        field: 'CalculationOn', headerText: 'Calculate On', allowEditing: isEditable, required: true, width: 350, valueAccessor: ej2GridDisplayFormatter, dataSource: CalculatOnList,
                        displayField: "CalculationOnName", edit: ej2GridDropDownObj({
                            width: 350
                        })
                    },
                    { field: 'ValueInFC', headerText: 'Amount In FC', width: 100, editType: "numericedit" },
                    { field: 'ValueInLC', headerText: 'Amount In LC', width: 100, editType: "numericedit" },
                    { field: 'CurConvRate', headerText: 'Conversion Rate', width: 100, editType: "numericedit" },
                    {
                        field: 'SHeadID', headerText: 'Source Head', allowEditing: isEditable, required: true, width: 350, valueAccessor: ej2GridDisplayFormatter, dataSource: masterData.SHeadNameList,
                        displayField: "SHeadName", edit: ej2GridDropDownObj({
                            width: 350
                        })
                    }
                ],
                commandClick: commandClick,
                actionBegin: function (args) {
                    if (args.requestType === 'beginEdit') {

                    }
                    else if (args.requestType === "add") {
                        debugger
                        args.data.SGHeadID = this.parentDetails.parentKeyFieldValue;
                        args.data.IIPDetailsID = maxColDetails++;
                    }
                    else if (args.requestType === "save") {

                    }
                    else if (args.requestType === "delete") {

                    }

                },
                load: loadPaymentDetailsGrid

            }

        });
        $tblPaymentDetailsId["dataBound"] = function () {
            $tblPaymentDetailsId.autoFitColumns();
        };
        $tblPaymentDetailsId.refreshColumns;
        $tblPaymentDetailsId.appendTo(tblPaymentDetailsId);
    }
    function loadPaymentDetailsGrid() {
        this.dataSource = this.parentDetails.parentRowData.IPDetailSub;

    }
    function commandClick(e) {
       
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }
    function resetForm() {
        $formEl.trigger("reset");
    }


    function save(IsComplete) {
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        //// CI Child Information 
        var ciChildInfo = [];
        $tblYarnPIItemsEl.getCurrentViewRecords().map(x => {
            ciChildInfo.push(x);
        });



        /// End Of CI Information

        //// Details Information 
        var yipListMain = [];
        var yipList = [];
        $tblPaymentDetailsId.getCurrentViewRecords().map(x => {
            yipList.push(x);
        });
        if (yipList.length > 0) {
            for (var i = 0; i < yipList.length; i++) {
                yipListMain.push(yipList[i]);
            }
        }

        ///// End Details Information


        var data = formDataToJson($formEl.serializeArray());
        data.IPChilds = ciChildInfo;
        data.IPDetails = yipListMain;
        data.Modify = (status === statusConstants.PENDING) ? false : true;
        data.IsCDA = IsCDA;

        axios.post("/yiipAPI/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });

    }

})();
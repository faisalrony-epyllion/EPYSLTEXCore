(function () {
    var menuId, pageName, menuParam;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $tblChildEl, tblChildId, $formEl;
    var $pageEl;
    var pageId;
    var status;
    var masterData;
    var _bankLimitChildID = 9999;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        if (!menuParam)
            menuParam = localStorage.getItem("menuParam");


        pageId = pageName + "-" + menuId;
        $pageEl = $(`#${pageId}`);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblCreateItemId = `#tblCreateItem-${pageId}`;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);


        $toolbarEl.find("#btnList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ALL;
            initMasterTable();
        });
        $toolbarEl.find("#btnNew").click(function (e) {
            e.preventDefault();
            loadNew();
        });

        $toolbarEl.find("#btnList").click();

        $formEl.find("#btnSave").click(function () {
            save();
        });
        $formEl.find("#btnBackToList").click(function () {
            backToList();
        });
    });

    function loadNew() {
        _isNew = true;
        axios.get(`/api/bank-limit/new/`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                setFormData($formEl, masterData);

                initChildTable(masterData.Childs);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getDetails(id) {
        var url = `/api/bank-limit/${id}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                setFormData($formEl, masterData);

                initChildTable(masterData.Childs);
            })
            .catch(showResponseError);
    }

    function initMasterTable() {
        var commands = [];
        if (status === statusConstants.PendingReceiveCI || status === statusConstants.PendingReceivePO || status === statusConstants.PendingReceiveSF) {
            commands = [
                { type: 'New', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } }
            ]
        } else if (status === statusConstants.DRAFT) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } },
                { type: 'Yarn Control Sheet', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
            ]
        }
        else {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'Yarn Control Sheet', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
            ]
        }

        var columns = [
            {
                headerText: '', commands: commands, textAlign: 'Center', width: ch_setActionCommandCellWidth(commands)
            },
            {
                field: 'CompanyName', headerText: 'Company'
            },
            {
                field: 'BankName', headerText: 'Bank'
            },
            {
                field: 'BankFacilityTypeName', headerText: 'Bank Facility Type'
            },
            {
                field: 'AccumulatedLimit', headerText: 'Accumulated Limit'
            },
            {
                field: 'CurrencyName', headerText: 'Currency'
            },
            {
                field: 'FormBankFacilityName', headerText: 'Form Bank Facility'
            },
            {
                field: 'LiabilityTypeName', headerText: 'Liability Type'
            },
            {
                field: 'FromTenureDay', headerText: 'From Tenure Day'
            },
            {
                field: 'ToTenureDay', headerText: 'To Tenure Day'
            },
            {
                field: 'MaxLimit', headerText: 'Max Limit'
            },
            {
                field: 'LCOpened', headerText: 'LC Opened'
            },
            {
                field: 'LCAcceptenceGiven', headerText: 'LC Acceptence Given'
            },
            {
                field: 'PaymentOnMaturity', headerText: 'Payment On Maturity'
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: true,
            apiEndPoint: `/api/bank-limit/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.BankLimitMasterID);
        }
    }

    async function initChildTable(data) {
        var columns = [
            {
                headerText: 'Action', width: 100, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                ]
            },
            { field: 'BankLimitChildID', isPrimaryKey: true, visible: false },
            {
                field: 'FormBankFacilityID',
                headerText: 'Form Bank Facility',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.FormBankFacilityList,
                displayField: "text",
                width: 300,
                edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'LiabilityTypeID',
                headerText: 'Liability Type',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.LiabilityTypeList,
                displayField: "text",
                width: 300,
                edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'FromTenureDay', headerText: 'From Tenure Day', width: 150,
                edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } }
            },
            {
                field: 'ToTenureDay', headerText: 'To Tenure Day', width: 150,
                edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } }
            },
            {
                field: 'MaxLimit', headerText: 'Max Limit', width: 150,
                edit: { params: { showSpinButton: false, decimals: 2, format: "N2" } }
            },
            {
                field: 'LCOpened', headerText: 'LC Opened', width: 150,
                edit: { params: { showSpinButton: false, decimals: 2, format: "N2" } }
            },
            {
                field: 'LCAcceptenceGiven', headerText: 'LC Acceptence Given', width: 150,
                edit: { params: { showSpinButton: false, decimals: 2, format: "N2" } }
            },
            {
                field: 'PaymentOnMaturity', headerText: 'Payment On Maturity', width: 150,
                edit: { params: { showSpinButton: false, decimals: 2, format: "N2" } }
            }
        ];

        if ($tblChildEl) $tblChildEl.destroy();
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            showColumnChooser: true,
            showDefaultToolbar: false,
            toolbar: ['Add'],
            editSettings: { allowEditing: true, allowAdding: true, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
            actionBegin: function (args) {

            },
            columns: columns,

        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }
    function backToList() {
        $divDetailsEl.fadeOut();
        $divTblEl.fadeIn();
        initMasterTable();
    }
    function save() {
        var hasError = false;

        var data = formDataToJson($formEl.serializeArray());

        data.CompanyID = $formEl.find('#CompanyID').val();
        data.CurrencyID = $formEl.find('#CurrencyID').val();
        data.BankID = $formEl.find('#BankID').val();
        data.BankFacilityTypeID = $formEl.find('#BankFacilityTypeID').val();

        var accumulatedLimit = getDefaultValueWhenInvalidN_Float(data.AccumulatedLimit);

        data.Childs = DeepClone($tblChildEl.getCurrentViewRecords());
        var childs = DeepClone(data.Childs);
        for (var i = 0; i < childs.length; i++) {
            var row = ` at row ${i + 1}`;
            var child = DeepClone(childs[i]);
            var maxLimit = getDefaultValueWhenInvalidN_Float(child.MaxLimit);

            if (maxLimit > accumulatedLimit) {
                toastr.error(`Max limit ${maxLimit} cannot be greater than bank accumulated limit ${accumulatedLimit} ${row}`);
                hasError = true;
                break;
            }
        }
        if (hasError) return false;

        axios.post("/api/bank-limit/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();
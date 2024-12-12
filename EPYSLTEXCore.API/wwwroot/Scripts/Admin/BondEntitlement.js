(function () {
    var menuId, pageName, menuParam;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $tblChildEl, tblChildId, $formEl;
    var $pageEl;
    var pageId;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var status;
    var tblCreateItemId, $tblCreateItemEl;

    var masterData, currentChildRowData;
    var childData;
    var _itemSegmentValues;
    var _isNew = false;
    var copiedRecord = null;
    var _childID = 1000;
    var _isBuyersChange = false;
    var _selectedIndex = -1;
    var _ignoreValidationPOIds = [];

    var isCDAPage = false;
    var isYarnRcv = false;
    var isYarnRcvApp = false;
    var _actionProps = {
        IsSendForApprove: false,
        IsApproved: false
    };

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
        axios.get(`/api/bond-entitlement/new/`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.FromDate = formatDateToDefault(new Date());
                masterData.ToDate = formatDateToDefault(new Date());
                setFormData($formEl, masterData);

                initChildTable(masterData.Childs);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getDetails(id) {
        var url = `/api/bond-entitlement/${id}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;

                masterData.FromDate = formatDateToDefault(new Date());
                masterData.ToDate = formatDateToDefault(new Date());
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
                field: 'BondLicenceNo', headerText: 'Bond Licence No'
            },
            {
                field: 'EBINNo', headerText: 'EBIN No'
            },
            {
                field: 'FromDate', headerText: 'From Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'ToDate', headerText: 'To Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'CurrencyName', headerText: 'Currency'
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: true,
            apiEndPoint: `/api/bond-entitlement/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.BondEntitlementMasterID);
        }
    }

    async function initChildTable(data) {
        if ($tblChildEl) $tblChildEl.destroy();

        var columns = [
            { field: 'BondEntitlementChildID', isPrimaryKey: true, visible: false, width: 10 },
            { field: 'SegmentName', headerText: 'RM Types', width: 120, textAlign: 'left', allowEditing: false },
            { field: 'HSCode', headerText: 'HS Code', width: 120, textAlign: 'left' },
            {
                field: 'UnitID',
                headerText: 'Unit',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.UnitList,
                displayField: "text",
                width: 130,
                edit: ej2GridDropDownObj({
                })
            },
            { field: 'BankFacilityAmount', headerText: 'Bank Facility Amount', textAlign: 'left', width: 120 }
        ];

        var childColumns = [
            { field: 'BondEntitlementChildItemID', isPrimaryKey: true, visible: false, width: 10 },
            { field: 'BondEntitlementChildID', visible: false, width: 10 },
            { field: 'SegmentValue', headerText: 'Item Name', width: 120, textAlign: 'left', allowEditing: false },
            { field: 'HSCode', headerText: 'HS Code', width: 120, textAlign: 'left' },
            { field: 'BankFacilityAmount', headerText: 'Bank Facility Amount', textAlign: 'left', width: 120 }
        ];

        var childItems = [];
        data.map(x => {
            childItems.push(...x.ChildItems);
        });
        // ej.grids.Grid({
        // new initEJ2Grid({

        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            showColumnChooser: true,
            showDefaultToolbar: false,
            editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
            actionBegin: function (args) {

            },
            columns: columns,
            childGrid: {
                queryString: 'BondEntitlementChildID',
                allowResizing: true,
                autofitColumns: false,
                editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns,
                actionBegin: function (args) {

                },
                load: loadChildItems
            },
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }
    function loadChildItems() {
        this.dataSource = this.parentDetails.parentRowData.ChildItems;
    }
    async function childCommandClick(e) {
        childData = e.rowData;
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
        data.CurrencyID = $formEl.find('#CompanyID').val();

        data.Childs = DeepClone($tblChildEl.getCurrentViewRecords());
        var childs = DeepClone(data.Childs);
        for (var i = 0; i < childs.length; i++) {
            var child = DeepClone(childs[i]);
            var bankFacilityAmount = getDefaultValueWhenInvalidN_Float(child.BankFacilityAmount);
            var bankFacilityAmount_CI = 0;

            for (var iC = 0; iC < child.ChildItems.length; iC++) {
                var childItem = DeepClone(child.ChildItems[iC]);
                bankFacilityAmount_CI += getDefaultValueWhenInvalidN_Float(childItem.BankFacilityAmount);
            }
            if (bankFacilityAmount_CI != bankFacilityAmount) {
                toastr.error(`For RM Type ${child.SegmentName} bank facility amount ${bankFacilityAmount} mismatched with items facility amount ${bankFacilityAmount_CI}`);
                hasError = true;
                break;
            }
        }

        data.Childs = data.Childs.filter(x => x.BankFacilityAmount > 0);
        data.Childs.map(c => {
            c.ChildItems = c.ChildItems.filter(x => x.BankFacilityAmount > 0);
        });

        if (hasError) return false;

        axios.post("/api/bond-entitlement/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();
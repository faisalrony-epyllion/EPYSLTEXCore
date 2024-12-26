(function () {
    var menuId, pageName, menuParam;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $tblChildEl, tblChildId, $formEl;
    var $pageEl;
    var pageId;
    var status;
    var masterData;
    var _bondEntitlementChildID = 9999;
    var _bondEntitlementChildItemID = 9999;

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

        $('.ej2-datepicker').each(function (i, el) {
            $(el).datepicker({
                todayHighlight: true,
                format: _ch_date_format_3,
                autoclose: true,
                todayBtn: "linked"
            }).on("show", function (date) {
                if (this.value && !date.date) {
                    console.log(this.value);
                    $(this).datepicker('update', this.value);
                }
            });
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

                masterData.FromDate = formatDateToDefault(masterData.FromDate);
                masterData.ToDate = formatDateToDefault(masterData.ToDate);
                setFormData($formEl, masterData);

                initChildTable(masterData.Childs);
            })
            .catch(showResponseError);
    }

    function initMasterTable() {
        var commands = [
            { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
        ]

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
        data.filter(x => x.BondEntitlementChildID == 0).map(x => {
            x.BondEntitlementChildID = _bondEntitlementChildID++;
        });

        var columns = [
            { field: 'BondEntitlementChildID', isPrimaryKey: true, visible: false, width: 10 },
            { field: 'GetItems', headerText: 'Select Item', allowEditing: false, textAlign: 'center', width: 30, valueAccessor: displayItems },
            { field: 'SubGroupName', headerText: 'RM Types', width: 120, textAlign: 'left', allowEditing: false },
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
            { field: 'HSCode', headerText: 'HS Code', width: 100 },
            {
                field: 'EntitlementQty', headerText: 'Entitlement Qty',
                edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } }, width: 100
            },
        ];

        var childColumns = [
            {
                headerText: '', width: 34, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
            },
            { field: 'BondEntitlementChildItemID', isPrimaryKey: true, visible: false, width: 10 },
            { field: 'BondEntitlementChildID', visible: false, width: 10 },
            { field: 'BondEntitlementMasterID', visible: false, width: 10 },
            { field: 'SegmentValue', headerText: 'Item Name', width: 120, allowEditing: false },
            { field: 'HSCode', headerText: 'HS Code', width: 100 },
            {
                field: 'EntitlementQty', headerText: 'Entitlement Qty',
                edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } }, width: 100
            },
        ];

        if ($tblChildEl) $tblChildEl.destroy();
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            showColumnChooser: true,
            showDefaultToolbar: false,
            editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
            actionBegin: function (args) {

            },
            columns: columns,
            recordClick: function (args) {
                if (args.column && args.column.field == "GetItems") {
                    var parent = args.rowData;
                    var itemList = [];
                    if (parent.SubGroupName.trim() == "Dyes") {
                        itemList = masterData.Dyes;
                    }
                    else if (parent.SubGroupName.trim() == "Chemicals") {
                        itemList = masterData.Chemicals;
                    }
                    if (itemList.length == 0) {
                        return toastr.error("No item found");
                    }

                    var finder = new commonFinder({
                        title: "Yarn Items",
                        pageId: pageId,
                        height: 320,
                        modalSize: "modal-md",
                        data: itemList,
                        headerTexts: "Item Name",
                        fields: "text",
                        primaryKeyColumn: "id",
                        autofitColumns: true,
                        widths: "100",
                        isMultiselect: true,
                        onMultiselect: function (selectedRecords) {
                            if (selectedRecords.length > 0) {
                                var indexF = masterData.Childs.findIndex(x => x.SubGroupID == parent.SubGroupID);
                                selectedRecords.map(x => {
                                    x.BondEntitlementChildID = parent.BondEntitlementChildID;
                                    x.BondEntitlementMasterID = parent.BondEntitlementMasterID;
                                    x.SegmentValueID = x.id;
                                    x.SegmentValue = x.text;

                                    var indexF_CI = masterData.Childs[indexF].ChildItems.findIndex(y => y.SegmentValueID == x.SegmentValueID);
                                    if (indexF_CI == -1) {
                                        x.BondEntitlementChildItemID = _bondEntitlementChildItemID++;
                                        masterData.Childs[indexF].ChildItems.push(x);
                                    }
                                });
                                $tblChildEl.updateRow(args.rowIndex, masterData.Childs[indexF]);
                            }
                        }
                    });
                    finder.showModal();
                }
            },
            childGrid: {
                queryString: 'BondEntitlementChildID',
                allowResizing: true,
                autofitColumns: false,
                editSettings: { allowEditing: true, allowAdding: false, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
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
    function displayItems(field, data, column) {
        column.disableHtmlEncode = false;
        return `<button type="button" class="btn btn-sm" style="background-color: #ffffff; color: black;" title='Select items'><span class="e-icons e-plus"></span></button>`;
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

        data.CompanyID = getDefaultValueWhenInvalidN($formEl.find('#CompanyID').val());

        if (data.CompanyID == 0) {
            toastr.error('Select company');
            return false;
        }

        data.Childs = DeepClone($tblChildEl.getCurrentViewRecords());
        var childs = DeepClone(data.Childs);

        for (var i = 0; i < childs.length; i++) {
            data.Childs[i].UnitID = getDefaultValueWhenInvalidN(data.Childs[i].UnitID);
            //data.Childs[i].ChildItems = data.Childs[i].ChildItems.filter(x => x.EntitlementQty > 0);

            var child = DeepClone(childs[i]);
            var entitlementQty = getDefaultValueWhenInvalidN_Float(child.EntitlementQty);
            var entitlementQty_CI = 0;

            for (var iC = 0; iC < child.ChildItems.length; iC++) {
                var childItem = DeepClone(child.ChildItems[iC]);
                entitlementQty_CI += getDefaultValueWhenInvalidN_Float(childItem.EntitlementQty);
            }
            if (entitlementQty_CI > entitlementQty && child.SubGroupName != 'Yarn') {
                toastr.error(`For RM Type ${child.SubGroupName} Entitlement Qty ${entitlementQty} cannot be less than items Entitlement Qty ${entitlementQty_CI}`);
                hasError = true;
                break;
            }
        }

        //data.Childs = data.Childs.filter(x => x.EntitlementQty > 0);
        //data.Childs.map(c => {
        //    c.ChildItems = c.ChildItems.filter(x => x.EntitlementQty > 0);
        //});

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
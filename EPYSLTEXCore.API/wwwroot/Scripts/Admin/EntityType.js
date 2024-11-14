(function () {
    var menuId, pageName, pageId;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, tblMasterId, $tblMasterEl, $tblChildEl, tblChildId, $formEl;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        filter: '',
        sort: '',
        order: ''
    }
    var status;
    var masterData;
    var subGroupElem;
    var subGroupObj;
    var uomElem;
    var uomObj;

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        status = statusConstants.COMPLETED;

        initMasterTable();
        getMasterTableData();

        $toolbarEl.find("#btnNew").on("click", function (e) {
            e.preventDefault();
            getNew();
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;

            getMasterTableData();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#btnNewItem").on("click", function (e) {
            e.preventDefault();
            if (masterData.BondFinancialYearImportLimits.length == 15) {
                toastr.error("Total Child can not be greater than 15!");
                return;
            }
            var newChildItem = {
                Id: getMaxIdForArray(masterData.BondFinancialYearImportLimits, "Id"),
                masterDataID: masterData.Id,
                SubGroupID: 0,
                Consumption: 0,
                ImportLimit: 0,
                UOMID: 0
            };

            masterData.BondFinancialYearImportLimits.push(newChildItem);
            $tblChildEl.bootstrapTable('load', masterData.BondFinancialYearImportLimits);
        });
    });

    function initMasterTable() {
        ej.base.enableRipple(true);
        $tblMasterEl = new ej.grids.Grid({
            allowExcelExport: true,
            allowPdfExport: true,
            toolbar: ['ExcelExport', 'PdfExport', 'CsvExport'],
            allowResizing: true,
            allowFiltering: true,
            actionComplete: handleGridEvents,
            commandClick: handleCommands,
            allowPaging: true,
            pageSettings: { pageCount: 5, currentPage: 1, pageSize: 10, pageSizes: true },
            columns: [
                {
                    headerText: 'Commands', width: 60, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
                },
                {
                    field: 'Company', isPrimaryKey: true, headerText: 'Company', width: 120
                },
                {
                    field: 'FinancialYear', headerText: 'Financial Year', width: 120
                },
                {
                    field: 'StartDate', headerText: 'Start Date', textAlign: 'Right', width: 120, type: 'date', format: _ch_date_format_1
                },
                {
                    field: 'EndDate', headerText: 'End Date', textAlign: 'Right', width: 120, type: 'date', format: _ch_date_format_1
                }
            ],
        });

        $tblMasterEl.appendTo(tblMasterId);

        $tblMasterEl.toolbarClick = function (args) {
            if (args.item.id.includes('pdfexport')) {
                $tblMasterEl.pdfExport();
            }
            if (args.item.id.includes('excelexport')) {
                $tblMasterEl.excelExport();
            }
            if (args.item.id.includes('csvexport')) {
                $tblMasterEl.csvExport();
            }
        };        
    }

    function getMasterTableData() {
        var url = `/api/bond-financial-year/list?status=${status}`;
        axios.get(url)
            .then(function (response) {
                var rows = response.data.rows;
                for (var i = 0; i < 20; i++) {
                    var obj = _.clone(rows[0]);
                    rows.push(obj);
                }

                var total = rows.length;
                $tblMasterEl.pageSettings.totalRecordsCount = total;
                $tblMasterEl.dataSource = rows;
                $tblMasterEl.refresh();
            })
            .catch(showResponseError);
    }

    function initChildTable(data) {
        if ($tblChildEl) $tblChildEl.destroy();

        ej.base.enableRipple(true);
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            allowFiltering: true,
            allowPaging: true,
            pageSettings: { pageCount: 5 },
            toolbar: ['Add'],
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true },
            actionComplete: childGridActionComplete,
            endEdit: function (e) {
                alert("end");
            },
            columns: [
                {
                    field: 'Id', isPrimaryKey: true, visible: false
                },
                {
                    headerText: 'Commands', width: 60, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-update e-icons' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-cancel-icon e-icons' } }]
                },
                {
                    field: 'SubGroupID', headerText: 'Sub Group', valueAccessor: ej2GridDisplayFormatter,dataSource: masterData.SubGroupList, displayField: "SubGroupName", edit: ej2GridDropDownObj ({
                        onChange: function (selectedData, rowData) {
                            var index = $tblChildEl.getRowIndexByPrimaryKey(rowData.Id);
                            rowData.SubGroupID = selectedData.id;
                            rowData.SubGroupName = selectedData.text;

                            // Call this if you want to update row data
                            $tblChildEl.editModule.updateRow(index, rowData);
                        }
                    })
                },
                {
                    field: 'UOM', headerText: 'Unit', width: 150, edit: {
                        create: function () {
                            uomElem = document.createElement('input');
                            return uomElem;
                        },
                        read: function () {
                            return uomObj.text;
                        },
                        destroy: function () {
                            uomObj.destroy();
                        },
                        write: function () {
                            uomObj = new ej.dropdowns.DropDownList({
                                dataSource: masterData.UOMList,
                                fields: { value: 'id', text: 'text' },
                                //enabled: false,
                                placeholder: 'Select a state',
                                floatLabelType: 'Never'
                            });
                            uomObj.appendTo(uomElem);
                        }
                    }
                },
                {
                    field: 'Consumption', headerText: 'Consumption', width: 120, editType: 'numericedit'
                    //, template: function () { return 'a' }
                        //'<a href="javascript:void(0)" class="editable-link edit">${(Consumption != 0) ? "AA" : "BB"}</a>'
                    , formatter: function (args, rowData) {
                        
                        template = rowData[args.field] ? rowData[args.field] : '<a href="javascript:void(0)" class="editable-link edit">Empty</a>';
                    }
                },
                {
                    field: 'ImportLimit', headerText: 'Import Limit', width: 120, editType: 'numericedit', validationRules: { required: true, number: true }
                }
            ],
            childGrid: {
                queryString: 'Id',
                allowPaging: true,
                toolbar: ['Add', 'Edit', 'Delete', 'Update', 'Cancel'],
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true },
                columns: [
                    { field: 'OrderID', headerText: 'Order ID', textAlign: 'Right', width: 120 },
                    { field: 'ShipCity', headerText: 'Ship City', width: 120 },
                    { field: 'Freight', headerText: 'Freight', width: 120 },
                    { field: 'ShipName', headerText: 'Ship Name', width: 150 }
                ],
                load: loadFirstLevelChildGrid
            }
        });

        $tblChildEl.appendTo(tblChildId);
    }

    function gridTemplate(props) {
        
        
    }

    function loadFirstLevelChildGrid(args) {
        this.parentDetails.parentKeyFieldValue = this.parentDetails.parentRowData['Id'];
        var source = [
            {
                'Id': this.parentDetails.parentKeyFieldValue,
                'OrderID': 10248,
                'CustomerID': 'VINET',
                'OrderDate': '1996-07-04T00:00:00.000Z',
                'ShippedDate': '1996-07-16T00:00:00.000Z',
                'Freight': 32.38,
                'ShipName': 'Vins et alcools Chevalier',
                'ShipAddress': '59 rue de l\'Abbaye',
                'ShipCity': 'Reims',
                'ShipRegion': null,
                'ShipCountry': 'France'
            },
            {
                'Id': this.parentDetails.parentKeyFieldValue,
                'OrderID': 10249,
                'CustomerID': 'TOMSP',
                'OrderDate': '1996-07-05T00:00:00.000Z',
                'ShippedDate': '1996-07-10T00:00:00.000Z',
                'Freight': 11.61,
                'ShipName': 'Toms Spezialitäten',
                'ShipAddress': 'Luisenstr. 48',
                'ShipCity': 'Münster',
                'ShipRegion': null,
                'ShipCountry': 'Germany'
            }
        ];
        this.dataSource = source;
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        getMasterTableData();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#Id").val(-1111);
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
        axios.get(`/api/bond-financial-year/new`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.StartDate = formatDateToDefault(masterData.StartDate);
                masterData.EndDate = formatDateToDefault(masterData.EndDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.BondFinancialYearImportLimits);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/bond-financial-year/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.StartDate = formatDateToDefault(masterData.StartDate);
                masterData.EndDate = formatDateToDefault(masterData.EndDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.BondFinancialYearImportLimits);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data["BondFinancialYearImportLimits"] = masterData.BondFinancialYearImportLimits;
        axios.post("/api/bond-financial-year/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    // #region grid events
    function handleGridEvents(args) {
        //console.log(args);
        switch (args.requestType) {
            case "filtering":
                break;
            case "paging":
                filterBy = {};
            default:
                break;
        }
    }

    function handleCommands(args) {
        //console.log(args);
        getDetails(args.rowData.Id);
    }

    function childGridActionComplete(args) {
        if (args.requestType === "save") {
        }        
    }
    // #endregion

    function getRandom(arr, n) {
        var result = new Array(n),
            len = arr.length,
            taken = new Array(len);
        if (n > len)
            throw new RangeError("getRandom: more elements taken than available");
        while (n--) {
            var x = Math.floor(Math.random() * len);
            result[n] = arr[x in taken ? taken[x] : x];
            taken[x] = --len in taken ? taken[len] : len;
        }
        return result;
    }
})();
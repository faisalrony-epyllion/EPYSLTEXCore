(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        filter: '',
        sort: '',
        order: ''
    }
    var status;
    var BondFinancialYear;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        $tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
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

            initMasterTable();
            getMasterTableData();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#btnNewItem").on("click", function (e) {
            e.preventDefault();
            if (BondFinancialYear.BondFinancialYearImportLimits.length == 15) {
                toastr.error("Total Child can not be greater than 15!");
                return;
            }
            var newChildItem = {
                BondFinancialYearImportLimitID: getMaxIdForArray(BondFinancialYear.BondFinancialYearImportLimits, "BondFinancialYearImportLimitID"),
                BondFinancialYearID: BondFinancialYear.BondFinancialYearID,
                SubGroupID: 0,
                Consumption: 0,
                ImportLimit: 0,
                UOMID: 0
            };

            BondFinancialYear.BondFinancialYearImportLimits.push(newChildItem);
            $tblChildEl.bootstrapTable('load', BondFinancialYear.BondFinancialYearImportLimits);
        });
    });

    function initMasterTable() {
        $tblMasterEl.bootstrapTable('destroy');
        $tblMasterEl.bootstrapTable({
            showRefresh: true,
            showExport: true,
            showColumns: true,
            toolbar: toolbarId,
            exportTypes: "['csv', 'excel']",
            pagination: true,
            filterControl: true,
            searchOnEnterKey: true,
            sidePagination: "server",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            showFooter: true,
            columns: [
                {
                    title: "Actions",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        return `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Bond Financial Year">
                                        <i class="fa fa-edit" aria-hidden="true"></i>
                                    </a>`;
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.BondFinancialYearID);
                        }
                    }
                },
                {
                    field: "Company",
                    title: "Company",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "FinancialYear",
                    title: "Financial Year",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "StartDate",
                    title: "Start Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "EndDate",
                    title: "End Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                }
            ],
            onPageChange: function (number, size) {
                var newOffset = (number - 1) * size;
                var newLimit = size;
                if (tableParams.offset == newOffset && tableParams.limit == newLimit)
                    return;

                tableParams.offset = newOffset;
                tableParams.limit = newLimit;

                getMasterTableData();
            },
            onSort: function (name, order) {
                tableParams.sort = name;
                tableParams.order = order;
                tableParams.offset = 0;

                getMasterTableData();
            },
            onRefresh: function () {
                resetTableParams();
                getMasterTableData();
            },
            onColumnSearch: function (columnName, filterValue) {
                if (columnName in filterBy && !filterValue) {
                    delete filterBy[columnName];
                }
                else
                    filterBy[columnName] = filterValue;

                if (Object.keys(filterBy).length === 0 && filterBy.constructor === Object)
                    tableParams.filter = "";
                else
                    tableParams.filter = JSON.stringify(filterBy);

                getMasterTableData();
            }
        });
    }

    function getMasterTableData() {
        var queryParams = $.param(tableParams);
        $tblMasterEl.bootstrapTable('showLoading');
        var url = `/api/bond-financial-year/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function initChildTable() {
        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            showFooter: true,
            columns: [
                {
                    field: "SubGroupID",
                    title: "Sub Group",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: BondFinancialYear.SubGroupList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    }
                },
                {
                    field: "Consumption",
                    title: "Consumption",
                    align: 'center',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        //validate: function (value) {
                        //    if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                        //        return 'Must be a positive integer.';
                        //    }
                        //}
                    }
                },
                {
                    field: "ImportLimit",
                    title: "ImportLimit",
                    align: 'center',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                        }
                    }
                },
                {
                    field: "UOMID",
                    title: "Unit",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: BondFinancialYear.UOMList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    }
                }
            ]
        });
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        getMasterTableData();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#BondFinancialYearID").val(-1111);
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
                BondFinancialYear = response.data;
                BondFinancialYear.StartDate = formatDateToDefault(BondFinancialYear.StartDate);
                BondFinancialYear.EndDate = formatDateToDefault(BondFinancialYear.EndDate);
                setFormData($formEl, BondFinancialYear);
                initChildTable();
                $tblChildEl.bootstrapTable("load", BondFinancialYear.BondFinancialYearImportLimits);
                $tblChildEl.bootstrapTable('hideLoading');
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
                BondFinancialYear = response.data;
                BondFinancialYear.StartDate = formatDateToDefault(BondFinancialYear.StartDate);
                BondFinancialYear.EndDate = formatDateToDefault(BondFinancialYear.EndDate);
                setFormData($formEl, BondFinancialYear);
                initChildTable();
                $tblChildEl.bootstrapTable("load", BondFinancialYear.BondFinancialYearImportLimits);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data["BondFinancialYearImportLimits"] = BondFinancialYear.BondFinancialYearImportLimits;
        axios.post("/api/bond-financial-year/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }


})();
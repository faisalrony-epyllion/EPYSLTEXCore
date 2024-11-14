(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $formEl;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }

    var supplierProductRelation, countIds, fiberTypeIds;

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
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        countIds = [];
        fiberTypeIds = [];

        initMasterTable();
        getMasterTableData();

        $toolbarEl.find("#btnNew").click(getNew);

        $toolbarEl.find("#btnList").click("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            initMasterTable();
            getMasterTableData();
        });

        $formEl.find("#btnSave").click(save);

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#CountIDs").on("select2:select", addCountID);
        $formEl.find("#CountIDs").on("select2:unselect", removeCountID);

        $formEl.find("#FiberTypeIDs").on("select2:select", addFiberTypeID);
        $formEl.find("#FiberTypeIDs").on("select2:unselect", removeFiberTypeID);
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
                    field: "",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        var template =
                            `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Process Setup">
                                <i class="fa fa-pencil" aria-hidden="true"></i>
                            </a>`;
                        return template;
                    },
                    events: {
                        'click .edit': getDetails
                    }
                },
                {
                    field: "ManufacturingLine",
                    title: "Manufacturing Line",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ManufacturingProcess",
                    title: "Manufacturing Process",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ManufacturingSubProcess",
                    title: "Sub Process",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "YarnColor",
                    title: "Yarn Color",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "ColorGrade",
                    title: "Color Grade",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Counts",
                    title: "Count Name",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-500' } }
                },
                {
                    field: "FiberTypes",
                    title: "Fiber Type",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-300' } }
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
        var url = `/api/yarn-process-setup/list?gridType=bootstrap-table&${queryParams}`;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(showResponseError)
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

    function getNew(e) {
        e.preventDefault();
        axios.get("/api/yarn-process-setup/new")
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                supplierProductRelation = response.data;
                countIds = [];
                fiberTypeIds = [];
                setFormData($formEl, supplierProductRelation);
            })
            .catch(showResponseError);
    }

    function getDetails(e, value, row, index) {
        e.preventDefault();
        axios.get(`/api/yarn-process-setup/${row.Id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                supplierProductRelation = response.data;
                countIds = supplierProductRelation.CountIDs;
                fiberTypeIds = supplierProductRelation.FiberTypeIDs;
                setFormData($formEl, supplierProductRelation);
            })
            .catch(showResponseError);
    }

    function addCountID(e) {
        var countId = e.params.data.id;
        countIds.push(parseInt(countId));
    }

    function removeCountID(e) {
        var countId = e.params.data.id;
        countIds.remove(parseInt(countId));
    }

    Array.prototype.remove = function () {
        var what, a = arguments, L = a.length, ax;
        while (L && this.length) {
            what = a[--L];
            while ((ax = this.indexOf(what)) !== -1) {
                this.splice(ax, 1);
            }
        }
        return this;
    };

    function addFiberTypeID(e) {
        var fabricId = e.params.data.id;
        fiberTypeIds.push(parseInt(fabricId));
    }

    function removeFiberTypeID(e) {
        var fabricId = e.params.data.id;
        fiberTypeIds.remove(parseInt(fabricId));
    }

    function save(e) {
        e.preventDefault();
        var data = formDataToJson($formEl.serializeArray());
        data.CountIDs = countIds;
        data.FiberTypeIDs = fiberTypeIds;
        axios.post("/api/yarn-process-setup/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(showResponseError);
    }
})();
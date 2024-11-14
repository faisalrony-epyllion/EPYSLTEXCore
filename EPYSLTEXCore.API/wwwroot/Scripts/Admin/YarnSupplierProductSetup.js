(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }

    var supplierProductSetup;

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

        $formEl.find("#YarnTypeIds").on('select2:select select2:unselect', getYarnCountByYarnType)

        $formEl.find("#btnSave").click(save);

        $formEl.find("#btnCancel").on("click", backToList);
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
                            `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Supplier Product Setup">
                                <i class="fa fa-pencil" aria-hidden="true"></i>
                            </a>`;
                        return template;
                    },
                    events: {
                        'click .edit': getDetails
                    }
                },
                {
                    field: "Supplier",
                    title: "Supplier",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "YarnPrograms",
                    title: "Yarn Programs",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "YarnSubPrograms",
                    title: "Yarn Sub Programs",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "YarnTypes",
                    title: "Yarn Types",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
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
        var url = `/api/yarn-supplier-product-setup/list?gridType=bootstrap-table&${queryParams}`;
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

    function getYarnCountByYarnType(e) {
        var yarnCountData = $formEl.find("#YarnTypeIds").select2("data");

        if (e.params._type === "unselect" && yarnCountData.length === 0) {
            initSelect2($formEl.find("#YarnCountIds"), []);
            return;
        }

        var yarnCountIds = yarnCountData.map(function (el) { return el.id }).join(",");

        var url = "/api/selectoption/yarn-count-by-yarn-type/" + yarnCountIds;
        axios.get(url)
            .then(function (response) {
                initSelect2($formEl.find("#YarnCountIds"), response.data);
            })
            .catch(showResponseError);
    }

    function getNew(e) {
        e.preventDefault();
        axios.get("/api/yarn-supplier-product-setup/new")
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                supplierProductSetup = response.data;
                setFormData($formEl, supplierProductSetup);
            })
            .catch(showResponseError);
    }

    function getDetails(e, value, row, index) {
        e.preventDefault();
        axios.get(`/api/yarn-supplier-product-setup/${row.Id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                supplierProductSetup = response.data;
                supplierProductSetup.SupplierID = supplierProductSetup.SupplierID.toString();
                $("#SupplierID").prop("disabled", true);
                setFormData($formEl, supplierProductSetup);
            })
            .catch(showResponseError);
    }

    function save(e) {
        e.preventDefault();
        var data = formDataToJson($formEl.serializeArray());
        var subProgramData = $formEl.find("#YarnProgramIds").select2("data");
        data.YarnProgramIds = subProgramData.map(function (el) { return parseInt(el.id) });
        data.YarnPrograms = subProgramData.map(function (el) { return el.text }).join(',');

        var yarnSubProgramData = $formEl.find("#YarnSubProgramIds").select2("data");
        data.YarnSubProgramIds = yarnSubProgramData.map(function (el) { return parseInt(el.id) });
        data.YarnSubPrograms = yarnSubProgramData.map(function (el) { return el.text }).join(',');

        var yarnTypeData = $formEl.find("#YarnTypeIds").select2("data");
        data.YarnTypeIds = yarnTypeData.map(function (el) { return parseInt(el.id) });
        data.YarnTypes = yarnTypeData.map(function (el) { return el.text }).join(',');

        var yarnCountData = $formEl.find("#YarnCountIds").select2("data");
        data.YarnCountIds = yarnCountData.map(function (el) { return parseInt(el.id) });
        data.YarnCounts = yarnCountData.map(function (el) { return el.text }).join(',');

        axios.post("/api/yarn-supplier-product-setup/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
})();
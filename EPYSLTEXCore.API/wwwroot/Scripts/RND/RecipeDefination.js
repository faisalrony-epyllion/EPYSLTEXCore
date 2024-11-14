(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl;
    var filterBy = {};
    var status = statusConstants.PENDING;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var isEditable = true;
    var Recipe;

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

        $formEl.find("#RecipeDate").datepicker({
            todayHighlight: true,
            autoclose: true
        });
       

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            initMasterTable();
            getMasterTableData();
        });
        $toolbarEl.find("#btnCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            initMasterTable();
            getMasterTableData();
        });

        $formEl.find("#btnAddItem").on("click", function (e) {
            e.preventDefault();
            addNewItem();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnCancel").on("click", backToList);
    });

    function addNewItem() {
        var newChildItem = {
            Id: getMaxIdForArray(Recipe.Childs, "Id"),
            EntityState: 4,

        };
        Recipe.Childs.push(newChildItem);
        $tblYarnChildEl.bootstrapTable('load', Recipe.Childs);
        
    }

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
                    visible: status == statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return [
                            '<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="New">',
                            '<i class="fa fa-plus" aria-hidden="true"></i>',
                            '</a>'
                        ].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            debugger;
                            getNew(row.Id);
                        }
                    }
                },
                {
                    field: "view",
                    align: "center",
                    width: 50,
                    visible: status !== statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return [
                            '<a class="btn btn-xs btn-default view" href="javascript:void(0)" title="View">',
                            '<i class="fa fa-eye" aria-hidden="true"></i>',
                            '</a>'
                        ].join(' ');
                    },
                    events: {
                        'click .view': function (e, value, row, index) {
                            e.preventDefault();
                            if (row) {
                                HoldOn.open({
                                    theme: "sk-circle"
                                });
                                getDetails(row.Id);
                            }
                        }
                    }
                },
                {
                    field: "JobCardNo",
                    title: "Job Card No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                }, {
                    field: "JobCardDate",
                    title: "Job Card Date",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    formatter: function (value, row, index, field) {
                        return formatDateToMMDDYYYY(value);
                    }
                },
                
                {
                    field: "ConceptNo",
                    title: "Concept No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ConceptDate",
                    title: "Concept Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToMMDDYYYY(value);
                    },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ColorName",
                    title: "Color Name",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ColorCode",
                    title: "Color Code",
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
        var url = "/api/rnd-recipe-defination/list?status=" + status + "&" + queryParams;
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
            uniqueId: 'Id',
            editable: isEditable,
            columns: [
                {
                    field: "ProcessId",
                    title: "Process",
                    align: 'center',
                    editable: {
                        type: 'select2',
                        title: 'Select Process',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: Recipe.ProcessList,
                        select2: { width: 200, placeholder: 'Select Color Name', alloclear: true }
                    }
                },
                {
                    field: "ColorCode",
                    title: "Color Code",
                    align: 'center',
                    editable: {
                        type: "text",
                        showButtons: false,
                        tpl: '<input type="text" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        
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
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
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

    function getNew(KJobCardMasterId) {
        var url = "/api/rnd-recipe-defination/new/" + KJobCardMasterId;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                Recipe = response.data;
                Recipe.ConceptDate = formatDateToMMDDYYYY(Recipe.ConceptDate);
                Recipe.JobCardDate = formatDateToMMDDYYYY(Recipe.JobCardDate);
                Recipe.RecipeDate = formatDateToMMDDYYYY(Recipe.RecipeDate);
                setFormData($formEl, Recipe);
                initChildTable();
                $tblChildEl.bootstrapTable("load", Recipe.ChildColors);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getDetails(id) {
        axios.get(`/api/rnd-recipe-defination/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                Concept = response.data;
                Recipe.ConceptDate = formatDateToMMDDYYYY(Recipe.ConceptDate);
                Recipe.ReqDate = formatDateToMMDDYYYY(Recipe.ReqDate);
                setFormData($formEl, Concept);
                initChildTable();
                initYarnChildTable();
                $tblChildEl.bootstrapTable("load", Recipe.ChildColors);
                $tblChildEl.bootstrapTable('hideLoading');
                $tblYarnChildEl.bootstrapTable("load", Recipe.Childs);
                $tblYarnChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        if ($formEl.find("#ProdComplete").is(':checked') == true)
            data.ProdComplete = true;
        data["Childs"] = Recipe.Childs;
        if (Recipe.Childs.length == 0) {
            toastr.error("At least one roll item required!", "Error");
        }
        else {
            axios.post("/api/rnd-recipe-defination/save", data)
                .then(function () {
                    toastr.success("Saved successfully.");
                    backToList();
                })
                .catch(function (error) {
                    toastr.error(error.response.data.Message);
                });
        }

    }
})();
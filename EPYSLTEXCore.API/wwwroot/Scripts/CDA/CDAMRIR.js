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

    var CDAMRIR;

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
        initChildTable();
        getMasterTableData();

        $("#ReceiveID").on("select2:select select2:unselect", function (e) {
            if (e.params.data.selected) {
                var receiveId = $(this).val();
                getNewChilds(receiveId);
            }
            else {
                CDAMRIR.Childs = [];
                $tblChildEl.bootstrapTable("load", CDAMRIR.Childs);
            }
        })

        $toolbarEl.find("#btnNew").on("click", getNew);

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnSaveAndSend").click(function (e) {
            e.preventDefault();
            save(true);
        });

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
                        return [
                            '<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit CDA QC Requisition">',
                            '<i class="fa fa-edit" aria-hidden="true"></i>',
                            '</a>'
                        ].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.Id);
                        }
                    }
                },
                {
                    field: "MRIRNo",
                    title: "MRIR No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "MRIRDate",
                    title: "MRIR Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "MRIRByUser",
                    title: "MRIR By User",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
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
        var url = "/api/CDA-mrir/list?gridType=bootstrap-table&" + queryParams;
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
                    field: "ItemName",
                    title: "Item Name",
                    width: 150
                },
                {
                    field: "AgentName",
                    title: "Agent Name",
                    width: 150
                },
                {
                    field: "Uom",
                    title: "Uom"
                },
                {
                    field: "LotNo",
                    title: "Lot No"
                },
                {
                    field: "Rate",
                    title: "Rate"
                },
                {
                    field: "ReceiveQty",
                    title: "Receive Qty",
                    align: 'center',
                    editable: {
                        type: "number",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                        }
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

    function getNew() {
        axios.get("/api/CDA-mrir/new")
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                CDAMRIR = response.data;
                CDAMRIR.MRIRDate = formatDateToDefault(CDAMRIR.MRIRDate);
                $formEl.find("#btnSave").show();
                setFormData($formEl, CDAMRIR);
                $tblChildEl.bootstrapTable("load", []);
                $tblChildEl.bootstrapTable('hideLoading');
                $formEl.find("#ReceiveID").prop('disabled', false);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/CDA-mrir/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                CDAMRIR = response.data;
                CDAMRIR.MRIRDate = formatDateToDefault(CDAMRIR.MRIRDate);
                $formEl.find("#btnSave").hide();
                setFormData($formEl, CDAMRIR);
                $tblChildEl.bootstrapTable("load", CDAMRIR.Childs);
                $formEl.find("#ReceiveID").prop('disabled', true);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getNewChilds(receiveId) {
        $tblChildEl.bootstrapTable('showLoading');
        axios.get(`/api/CDA-mrir/new/childs/${receiveId}`)
            .then(function (response) {
                CDAMRIR.Childs = response.data;
                $formEl.find("#SupplierId").val(response.data[0].SupplierId);
                $formEl.find("#CompanyId").val(response.data[0].CompanyId);
                $formEl.find("#RCompanyId").val(response.data[0].RCompanyId);
                $formEl.find("#QCRemarksMasterId").val(response.data[0].QCRemarksMasterId);
                $tblChildEl.bootstrapTable("load", CDAMRIR.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save(isApprove = false) {
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = CDAMRIR.Childs;
        CDAMRIR.Approve = isApprove;

        axios.post("/api/CDA-mrir/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
})();
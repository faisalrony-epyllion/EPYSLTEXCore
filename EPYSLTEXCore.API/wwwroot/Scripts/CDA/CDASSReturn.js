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

    var CDASSReq;
    var status = statusConstants.PENDING;

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

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;

            initMasterTable();
            getMasterTableData();
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
                        var template;
                        if (status === statusConstants.PENDING) {
                            template =
                                `<a class="btn btn-xs btn-default add" href="javascript:void(0)" title="New CDA SS Retrurn">
                                    <i class="fa fa-plus" aria-hidden="true"></i>
                                </a>`;
                        }
                        else {
                            template =
                                `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit CDA SS Retrurn">
                                    <i class="fa fa-edit" aria-hidden="true"></i>
                                </a>`;
                        }

                        return template;
                    },
                    events: {
                        'click .add': function (e, value, row, index) {
                            e.preventDefault();
                     
                            getNew(row.SSRemarksMasterID);
                        },
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.SSReturnMasterID);
                        }
                    }
                },
                {
                    field: "SSRemarksNo",
                    title: "Remarks No",
                },
                {
                    field: "SSReturnNo",
                    title: "Return No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "SSReturnDate",
                    title: "Return Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "SSReqNo",
                    title: "Req No",
                },
                {
                    field: "SSReqQty",
                    title: "Req Qty",
                },
                {
                    field: "SSReceiveNo",
                    title: "Rcv No",
                    visible: status == statusConstants.PENDING
                },
                {
                    field: "ReceiveQty",
                    title: "Rcv Qty",
                },
                {
                    field: "ReturnQty",
                    title: "Rtn Qty",
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "SSReturnByUser",
                    title: "Return By",
                    visible: status !== statusConstants.PENDING
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
        var url = `/api/CDA-SS-return/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
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
                    field: "BatchNo",
                    title: "Batch No"
                },
                {
                    field: "Rate",
                    title: "Rate"
                },
                {
                    field: "ReceiveQty",
                    title: "Rcv Qty"
                },
                {
                    field: "ReturnQty",
                    title: "Return Qty",
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
                    field: "Remarks",
                    title: "Remarks",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
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
        $formEl.find("#SSReturnMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(reqMasterId) {
        axios.get(`/api/CDA-SS-return/new/${reqMasterId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                CDASSReq = response.data;
                CDASSReq.SSReturnDate = formatDateToDefault(CDASSReq.SSReturnDate);
                CDASSReq.SSReqDate = formatDateToDefault(CDASSReq.SSReqDate);
                CDASSReq.SSReceiveDate = formatDateToDefault(CDASSReq.SSReceiveDate);
                setFormData($formEl, CDASSReq);

                $tblChildEl.bootstrapTable("load", CDASSReq.Childs);
                $tblChildEl.bootstrapTable('hideLoading');

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/CDA-SS-return/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                CDASSReq = response.data;
                CDASSReq.SSReturnDate = formatDateToDefault(CDASSReq.SSReturnDate);
                CDASSReq.SSReqDate = formatDateToDefault(CDASSReq.SSReqDate);
                CDASSReq.SSReceiveDate = formatDateToDefault(CDASSReq.SSReceiveDate);
                setFormData($formEl, CDASSReq);
                $tblChildEl.bootstrapTable("load", CDASSReq.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {

        var data = formDataToJson($formEl.serializeArray());

        data["Childs"] = CDASSReq.Childs;
        axios.post("/api/CDA-SS-return/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();

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
    var status;

    var YDLeftOverReturnReceive;

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

        status = statusConstants.PENDING;

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
                    title: "Actions",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        if (status === statusConstants.PENDING) {
                            return `<a class="btn btn-xs btn-default add" href="javascript:void(0)" title="New Left Return Receive">
                                        <i class="fa fa-plus" aria-hidden="true"></i>
                                    </a>`;
                        }
                        else {
                            return `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Left Return Receive">
                                        <i class="fa fa-edit" aria-hidden="true"></i>
                                    </a>`;
                        }
                    },
                    events: {
                        'click .add': function (e, value, row, index) {
                            e.preventDefault();
                            getNew(row.LOReturnMasterID);
                        },
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.Id);
                        }
                    }
                },
                {
                    field: "LOReturnReceiveNo",
                    title: "LO Return Rcv No",
                    filterControl: "input",
                    visible: status === statusConstants.COMPLETED,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "LOReturnReceiveDate",
                    title: "LO Return Rcv Date",
                    visible: status === statusConstants.COMPLETED,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "LOReturnReceiveByUser",
                    title: "Return Rcv By",
                    filterControl: "input",
                    visible: status === statusConstants.COMPLETED,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "LOReturnNo",
                    title: "LO Return No",
                    filterControl: "input",
                    visible: status === statusConstants.COMPLETED,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "LOReturnDate",
                    title: "LO Return Date",
                    visible: status === statusConstants.COMPLETED,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
               
                {
                    field: "LOReturnNo",
                    title: "LO Return No",
                    filterControl: "input",
                    visible: status === statusConstants.PENDING,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "LOReturnByUser",
                    title: "LO Return By",
                    filterControl: "input",
                    visible: status === statusConstants.PENDING,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "LOReturnDate",
                    title: "LO Return Date",
                    visible: status === statusConstants.PENDING,
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
        var url = `/api/cda-floor-left-over-return-receive/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
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
                    field: "ReturnQty",
                    title: "Return Qty",

                },
                {
                    field: "ReceiveQty",
                    title: "Rcv Qty",
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

    function getNew(YDLOReturnMasterID) {
        axios.get(`/api/cda-floor-left-over-return-receive/new/${YDLOReturnMasterID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                YDLeftOverReturnReceive = response.data;
                YDLeftOverReturnReceive.LOReturnReceiveDate = formatDateToDefault(YDLeftOverReturnReceive.LOReturnReceiveDate);
                YDLeftOverReturnReceive.LOReturnDate = formatDateToDefault(YDLeftOverReturnReceive.LOReturnDate);
                setFormData($formEl, YDLeftOverReturnReceive);
                $tblChildEl.bootstrapTable("load", YDLeftOverReturnReceive.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/cda-floor-left-over-return-receive/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                YDLeftOverReturnReceive = response.data;
                YDLeftOverReturnReceive.LOReturnReceiveDate = formatDateToDefault(YDLeftOverReturnReceive.LOReturnReceiveDate);
                YDLeftOverReturnReceive.LOReturnDate = formatDateToDefault(YDLeftOverReturnReceive.LOReturnDate);

                setFormData($formEl, YDLeftOverReturnReceive);
                $tblChildEl.bootstrapTable("load", YDLeftOverReturnReceive.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save(e) {
        e.preventDefault();
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = YDLeftOverReturnReceive.Childs;
        axios.post("/api/cda-floor-left-over-return-receive/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }


})();
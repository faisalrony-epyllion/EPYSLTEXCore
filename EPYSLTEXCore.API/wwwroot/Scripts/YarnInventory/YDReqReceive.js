(function () {
    var menuId, pageName, status;
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

    var ydReqReceive;

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

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
                        var template = "";
                        if (status === statusConstants.PENDING) {
                            template = `<a class="btn btn-xs btn-default add" href="javascript:void(0)" title="Edit YD Issue">
                            <i class="fa fa-plus" aria-hidden="true"></i>
                            </a>`;
                        }
                        else {
                            template = `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit YD Issue">
                            <i class="fa fa-edit" aria-hidden="true"></i>
                            </a>`;
                        }
                        return template;
                    },
                    events: {
                        'click .add': getNew,
                        'click .edit': getDetails
                    }
                },//YDReqIssueMasterID, , , , YDBookingNo, BuyerName, ReqQty, IssueQty
                {
                    field: "YDReqReceiveNo",
                    title: "Receive No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "YDReqReceiveDate",
                    title: "Receive Date",
                    visible: status !== statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "ReqReceiveByUser",
                    title: "Receive By",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "YDReqIssueNo",
                    title: "Issue No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "YDReqIssueDate",
                    title: "Issue Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "ReqIssueByUser",
                    title: "Issue By",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status === statusConstants.PENDING
                },
                {
                    field: "YDBookingNo",
                    title: "Booking No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BuyerName",
                    title: "Buyer",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status === statusConstants.PENDING
                },
                {
                    field: "ReqQty",
                    title: "Req Qty",
                    filterControl: "input",
                    align: "right",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "IssueQty",
                    title: "Issue Qty",
                    filterControl: "input",
                    align: "right",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status === statusConstants.PENDING
                },
                {
                    field: "ReceiveQty",
                    title: "Receive Qty",
                    filterControl: "input",
                    align: "right",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status !== statusConstants.PENDING
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
        var url = "/api/yd-req-receive/list?gridType=bootstrap-table&status=" + status + "&" + queryParams;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function initChildTable(data) {
        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            showFooter: true,
            columns: [
                {
                    field: "YarnProgramName",
                    title: "Yarn Program"
                },
                {
                    field: "YarnType",
                    title: "Yarn Type"
                },
                {
                    field: "YarnComposition",
                    title: "Yarn Composition"
                },
                {
                    field: "YarnCount",
                    title: "Yarn Count"
                },
                {
                    field: "YarnColor",
                    title: "Yarn Color"
                },
                {
                    field: "Shade",
                    title: "Shade"
                },
                {
                    field: "Uom",
                    title: "Uom"
                },
                {
                    field: "ReqQty",
                    title: "Req Qty"
                },
                {
                    field: "IssueQty",
                    title: "Issue Qty"
                },
                {
                    field: "IssueQtyCone",
                    title: "Issue Qty Cone"
                },
                {
                    field: "IssueQtyCarton",
                    title: "Issue Qty Carton"
                },
                {
                    field: "ReceiveQty",
                    title: "Receive Qty",
                    align: 'right',
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
                    field: "ReceiveQtyCone",
                    title: "Receive Qty Cone",
                    align: 'right',
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
                    field: "ReceiveQtyCarton",
                    title: "Receive Qty Carton",
                    align: 'right',
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
                },
                {
                    field: "YarnSubProgramNames",
                    title: "YarnSubProgramNames"
                },
                {
                    field: "YarnCategory",
                    title: "Yarn Category"
                },
                {
                    field: "NoOfThread",
                    title: "NoOfThread"
                }
            ],
            data: data
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
        $formEl.find("#YDReqReceiveMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(e, value, row, index) {
        e.preventDefault();

        axios.get(`/api/yd-req-receive/new/${row.YDReqIssueMasterId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                ydReqReceive = response.data;
                ydReqReceive.YDReqReceiveDate = formatDateToDefault(ydReqReceive.YDReqReceiveDate);
                ydReqReceive.YDReqIssueDate = formatDateToDefault(ydReqReceive.YDReqIssueDate);
                setFormData($formEl, ydReqReceive);

                initChildTable(ydReqReceive.Childs);
            })
            .catch(showResponseError);
    }

    function getDetails(e, value, row, index) {
        e.preventDefault();
        axios.get(`/api/yd-req-receive/${row.YDReqReceiveMasterID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                ydReqReceive = response.data;
                ydReqReceive.YDReqReceiveDate = formatDateToDefault(ydReqReceive.YDReqReceiveDate);
                ydReqReceive.YDReqDate = formatDateToDefault(ydReqReceive.YDReqDate);
                setFormData($formEl, ydReqReceive);

                initChildTable(ydReqReceive.Childs);
            })
            .catch(showResponseError);
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = ydReqReceive.Childs;

        axios.post("/api/yd-req-receive/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
})();
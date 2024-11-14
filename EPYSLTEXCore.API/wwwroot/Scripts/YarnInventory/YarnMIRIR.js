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

    var yarnQCRemarks;

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

        $formEl.find("#QCReceiveDate").datepicker({
            todayHighlight: true,
            autoclose: true
        });

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
                            return `<a class="btn btn-xs btn-default add" href="javascript:void(0)" title="New Yarn QC Receive">
                                        <i class="fa fa-plus" aria-hidden="true"></i>
                                    </a>`;
                        }
                        else {
                            return `<a class="btn btn-xs btn-default view" href="javascript:void(0)" title="Edit Yarn QC Issue">
                                        <i class="fa fa-eye" aria-hidden="true"></i>
                                    </a>`;
                        }
                    },
                    events: {
                        'click .add': function (e, value, row, index) {
                            e.preventDefault();
                            getNew(row.QCReceiveMasterId);
                        },
                        'click .view': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.Id);
                        }
                    }
                },
                {
                    field: "QCReceiveNo",
                    title: "Receive No",
                    filterControl: "input",
                    visible: status === statusConstants.PENDING,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "QCReceiveDate",
                    title: "Received Date",
                    visible: status === statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToMMDDYYYY(value);
                    }
                },
                {
                    field: "QCReceivedByUser",
                    title: "Received By",
                    filterControl: "input",
                    visible: status === statusConstants.PENDING,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "QCRemarksNo",
                    title: "Remarks No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status !== statusConstants.PENDING,
                },
                {
                    field: "QCRemarksDate",
                    title: "Remarks Date",
                    visible: status !== statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToMMDDYYYY(value);
                    }
                },
                {
                    field: "QCRemarksByUser",
                    title: "Remarks By",
                    filterControl: "input",
                    visible: status !== statusConstants.PENDING,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "QCReqFor",
                    title: "Req For",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ReceiveQty",
                    title: "Receive Qty",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ReceiveQtyCone",
                    title: "Receive Qty (Cone)",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ReceiveQtyCarton",
                    title: "Receive Qty (Carton)",
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
        var url = `/api/yarn-mrir/list?status=${status}&${queryParams}`;
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
                    field: "YarnType",
                    title: "Yarn Type",
                    width: 60
                },
                {
                    field: "YarnComposition",
                    title: "Yarn Composition",
                    width: 100
                },
                {
                    field: "YarnCount",
                    title: "Yarn Count",
                    width: 60
                },
                {
                    field: "YarnColor",
                    title: "Yarn Color",
                    width: 80
                },
                {
                    field: "YarnShade",
                    title: "Shade",
                    width: 60
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
                    field: "ReceiveQty",
                    title: "Receive Qty",
                },
                {
                    field: "ReceiveQtyCarton",
                    title: "Receive Qty(Carton)",
                },
                {
                    field: "ReceiveQtyCone",
                    title: "Receive Qty(Cone)",
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
                    title: "Actions",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        var template = 
                        `<span class="btn-group">
                            <a class="btn btn-xs btn-success approve" href="javascript:void(0)" title="Approve">
                                <i class="fa fa-check" aria-hidden="true"></i> Approve
                            </a>
                            <a class="btn btn-xs btn-danger reject" href="javascript:void(0)" title="Reject">
                                <i class="fa fa-ban" aria-hidden="true"></i> Reject
                            </a>
                            <a class="btn btn-xs btn-primary retest" href="javascript:void(0)" title="Re-Test">
                                <i class="fa fa-hourglass" aria-hidden="true"></i> Re-Test
                            </a>
                        </span>`;

                        return template;
                    },
                    events: {
                        'click .approve': function (e, value, row, index) {
                            e.preventDefault();
                            var data = row;

                            data.Remarks = row.Remarks ? row.Remarks.trim() : "";
                            if (!data.Remarks) return toastr.error("You must enter remarks");

                            data.Approve = true;
                            data.Reject = false;
                            data.ReTest = false;
                            save(data);
                        },
                        'click .reject': function (e, value, row, index) {
                            e.preventDefault();
                            var data = row;

                            data.Remarks = row.Remarks ? row.Remarks.trim() : "";
                            if (!data.Remarks) return toastr.error("You must enter remarks");

                            data.Approve = false;
                            data.Reject = true;
                            data.ReTest = false;
                            save(data);
                        },
                        'click .retest': function (e, value, row, index) {
                            e.preventDefault();
                            var data = row;

                            data.Remarks = row.Remarks ? row.Remarks.trim() : "";
                            if (!data.Remarks) return toastr.error("You must enter remarks");

                            data.Approve = false;
                            data.Reject = false;
                            data.ReTest = true;
                            save(data);
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

    function getNew(qcIssueMasterId) {
        axios.get(`/api/yarn-mrir/new/${qcIssueMasterId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                yarnQCRemarks = response.data;
                yarnQCRemarks.QCRemarksDate = formatDateToMMDDYYYY(yarnQCRemarks.QCRemarksDate);
                yarnQCRemarks.QCReceiveDate = formatDateToMMDDYYYY(yarnQCRemarks.QCReceiveDate);
                yarnQCRemarks.QCReqDate = formatDateToMMDDYYYY(yarnQCRemarks.QCReqDate);
                setFormData($formEl, yarnQCRemarks);
                $tblChildEl.bootstrapTable("load", yarnQCRemarks.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/yarn-mrir/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                yarnQCRemarks = response.data;
                yarnQCRemarks.QCRemarksDate = formatDateToMMDDYYYY(yarnQCRemarks.QCRemarksDate);
                yarnQCRemarks.QCReceiveDate = formatDateToMMDDYYYY(yarnQCRemarks.QCReceiveDate);
                yarnQCRemarks.QCReqDate = formatDateToMMDDYYYY(yarnQCRemarks.QCReqDate);
                setFormData($formEl, yarnQCRemarks);
                $tblChildEl.bootstrapTable("load", yarnQCRemarks.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save(testData) {
        var data = formDataToJson($formEl.serializeArray());
        data.Remarks = testData.Remarks;
        data.Approve = testData.Approve;
        data.Reject = testData.Reject;
        data.ReTest = testData.ReTest;

        axios.post("/api/yarn-mrir/save", data)
            .then(function (response) {
                toastr.success("Saved successfully.");
                $("#Id").val(response.data.Id);
                backToList();
            })
            .catch(showResponseError);
    }
})();
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

    var CDAQCReceive;

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
                            return `<a class="btn btn-xs btn-default add" href="javascript:void(0)" title="New CDA QC Receive">
                                        <i class="fa fa-plus" aria-hidden="true"></i>
                                    </a>`;
                        }
                        else {
                            return `<a class="btn btn-xs btn-default view" href="javascript:void(0)" title="Edit CDA QC Issue">
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
                            getDetails(row.QCRemarksMasterID);
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
                        return formatDateToDefault(value);
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
                    field: "QCIssueNo",
                    title: "Issue No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status === statusConstants.PENDING
                },
                {
                    field: "QCIssueDate",
                    title: "Issue Date",
                    visible: status === statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "QCIssueByUser",
                    title: "Issue By",
                    filterControl: "input",
                    visible: status === statusConstants.PENDING,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "QCReqNo",
                    title: "Req No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                     visible: status === statusConstants.PENDING,
                },
                {
                    field: "QCRemarksNo",
                    title: "Remarks No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status !== statusConstants.PENDING,
                },
                {
                    field: "IssueQty",
                    title: "Issue Qty",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "QCReqFor",
                    title: "Req For",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ReqQty",
                    title: "Req Qty",
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
        var url = `/api/CDA-qc-remarks/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
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
                    field: "ReceiveQty",
                    title: "Receive Qty",
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
        $formEl.find("#QCRemarksMasterID").val(-1111);
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
        axios.get(`/api/CDA-qc-remarks/new/${qcIssueMasterId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                CDAQCReceive = response.data;
                CDAQCReceive.QCReqDate = formatDateToDefault(CDAQCReceive.QCReqDate);
                CDAQCReceive.QCReqDate = formatDateToDefault(CDAQCReceive.QCReqDate);
                CDAQCReceive.QCReceiveDate = formatDateToDefault(CDAQCReceive.QCReceiveDate);
                CDAQCReceive.QCRemarksDate = formatDateToDefault(CDAQCReceive.QCRemarksDate);
                setFormData($formEl, CDAQCReceive);
                $tblChildEl.bootstrapTable("load", CDAQCReceive.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/CDA-qc-remarks/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                CDAQCReceive = response.data;
                CDAQCReceive.QCReqDate = formatDateToDefault(CDAQCReceive.QCReqDate);
                CDAQCReceive.QCReceiveDate = formatDateToDefault(CDAQCReceive.QCReceiveDate);
                CDAQCReceive.QCRemarksDate = formatDateToDefault(CDAQCReceive.QCRemarksDate);
                setFormData($formEl, CDAQCReceive);
                $tblChildEl.bootstrapTable("load", CDAQCReceive.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save(data) {
        data.QCRemarksDate = $("#QCRemarksDate").val();
        data.QCReqMasterId = $("#QCReqMasterId").val();
        data.QCIssueMasterId = $("#QCIssueMasterId").val();
        data.QCReceiveMasterId = $("#QCReceiveMasterId").val();

        data.SupplierId = $("#SupplierId").val();
        data.CompanyId = $("#CompanyId").val();
        data.RCompanyId = $("#RCompanyId").val();
        data.ReceiveID = $("#ReceiveID").val();



        axios.post("/api/CDA-qc-remarks/save", data)
            .then(function (response) {
                toastr.success("Saved successfully.");
                $("#Id").val(response.data.Id);
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();
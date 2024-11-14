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

    var knittingSCReq;

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

        $formEl.find("#KSCReqDate").datepicker({
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

        $toolbarEl.find("#btnApproveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.APPROVED;

            initMasterTable();
            getMasterTableData();
        }); 

        $toolbarEl.find("#btnRejectList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.REJECT;

            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnAllList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.ALL;

            initMasterTable();
            getMasterTableData();
        });

        $formEl.find("#btnSave").click(save);

        $formEl.find("#btnSave").click(function (e) {
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
                        var template = status === statusConstants.PENDING ?
                            `<a class="btn btn-xs btn-default add" href="javascript:void(0)" title="New KSC">
                                <i class="fa fa-plus" aria-hidden="true"></i>
                            </a>`
                            :
                            `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Yarn QC Issue">
                                <i class="fa fa-pencil" aria-hidden="true"></i>
                            </a>`;

                        return template;
                    },
                    events: {
                        'click .add': function (e, value, row, index) {
                            e.preventDefault();
                            getNew(row.KSCMasterID);
                        },
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.Id);
                        }
                    }
                },
                {
                    field: "KSCReqNo",
                    title: "Req No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "KSCReqDate",
                    title: "Req Date",
                    visible: status !== statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToMMDDYYYY(value);
                    }
                },
                {
                    field: "KSCByUser",
                    title: "KSC By",
                    filterControl: "input",
                    visible: status !== statusConstants.PENDING,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "SubContractor",
                    title: "Sub Contractor",
                    filterControl: "input",
                    visible: status !== statusConstants.PENDING,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                }, 
                {
                    field: "BookingNo",
                    title: "Booking No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ExportOrderNo",
                    title: "EWO No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "YBookingNo",
                    title: "Yarn Booking No",
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
        var url = `/api/knitting-sub-contract/list?status=${status}&${queryParams}`;
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
                    field: "YarnProgram",
                    title: "Yarn Program",
                    width: 60
                },
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
                    field: "BookingQty",
                    title: "Booking Qty"
                },
                {
                    field: "SCQty",
                    title: "SC Qty"
                },
                {
                    field: "ReqQty",
                    title: "Req Qty",
                    align: 'center',
                    editable: {
                        type: "text",
                        showButtons: false,
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
                    align: 'center',
                    editable: {
                        type: "text",
                        showButtons: false,
                    }
                }
            ]
        });
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();

        $toolbarEl.find("#btnList").trigger("click");
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

    function getNew(yBookingId) {
        axios.get(`/api/knitting-sub-contract/new/${yBookingId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                knittingSCReq = response.data;
                knittingSCReq.KSCReqDate = formatDateToMMDDYYYY(knittingSCReq.KSCReqDate);
                setFormData($formEl, knittingSCReq);
                $tblChildEl.bootstrapTable("load", knittingSCReq.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/knitting-sub-contract/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                knittingSCReq = response.data;
                knittingSCReq.KSCReqDate = formatDateToMMDDYYYY(knittingSCReq.KSCReqDate);
                setFormData($formEl, knittingSCReq);
                $tblChildEl.bootstrapTable("load", knittingSCReq.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save(sendForApproval = false) {
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = knittingSCReq.Childs;
        data.SendForApproval = sendForApproval;

        axios.post("/api/knitting-sub-contract/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();
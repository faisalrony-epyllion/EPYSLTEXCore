(function () {
    //'use strict'

    // #region variables
    var menuId, pageName;
    var toolbarId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $formEl;
    var isShowingAllPO = false;

    var yarnPICheckIn = {};
    var yarnPICheckInSplit = [];
    var filterBy = {};
    var yarnPICheckInStatus = 1;
    var yarnPOStatus = 3;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var ExportOrderfilterBy = {};
    var exportOrderTableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var startDate = null;
    var endDate = null;
    var pODate = null;
    var validationConstraints = [];
    // #endregion

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $pageEl = $(pageConstants.PAGE_ID_PREFIX + pageId);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        $tblYPOEl = $(pageConstants.MASTER_TBL_ID_PREFIX +'YPO'+ pageId);
        $tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        status = statusConstants.PENDING;

        var today = new Date();
        var datetoday = (today.getMonth() + 1) + '/' + today.getDate() + '/' + today.getFullYear();
        $formEl.find("#PICINDate").val(datetoday);
        initYarnPOMasterTable();
        getYarnPOMasterData();

        // Init file input plugin
        $("#UploadFile").fileinput('destroy');
        $("#UploadFile").fileinput({
            showUpload: false,
            previewFileType: 'any',
            maxFileSize: 4096,
            required: true
        });
        $formEl.find("#btnCancel").on("click", function (e) {
            e.preventDefault();
            backToList();
        });
        $toolbarEl.find("#btnYPONew").on("click", function (e) {
            e.preventDefault();
            $divDetailsEl.fadeIn();
            $divTblEl.fadeOut();

            toggleActiveToolbarBtn(this, $toolbarEl);
        });
        $toolbarEl.find("#btnYPOEdit").on("click", function (e) {
            e.preventDefault();


            toggleActiveToolbarBtn(this, $toolbarEl);
        });
        $toolbarEl.find("#btnYPOApproved").on("click", function (e) {
            e.preventDefault();


            toggleActiveToolbarBtn(this, $toolbarEl);
        });
        $toolbarEl.find("#btnYPOUnApproved").on("click", function (e) {
            e.preventDefault();


            toggleActiveToolbarBtn(this, $toolbarEl);
        });
    });

    function initYarnPOMasterTable() {
        $tblYPOEl.bootstrapTable('destroy');
        $tblYPOEl.bootstrapTable({
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
                    title: 'Actions',
                    align: 'center',
                    width: 100,
                    formatter: function (value, row, index, field) {
                        return getMasterTblRowActions(row);
                    },
                    events: {
                        'click .new': function (e, value, row, index) {
                            e.preventDefault();

                            $formEl.find("#Id").val(row.Id);
                            $formEl.find("#PONoList").val(row.PoNo);
                            $formEl.find("#SupplierName").val(row.SupplierName);
                            $divDetailsEl.fadeIn();
                            $divTblEl.fadeOut();
                        },
                        'click .propose': function (e, value, row, index) {
                            e.preventDefault();

                            showBootboxConfirm("Propose Yarn PO", "Are you sure you want to propose this PO?", function (yes) {
                                if (yes) {
                                    var url = "/api/ypo/propose-ypo/" + row.Id;
                                    axios.post(url)
                                        .then(function () {
                                            toastr.success(constants.PROPOSE_SUCCESSFULLY);
                                            getYarnPOMasterData();
                                        })
                                        .catch(function (error) {
                                            toastr.error(error.response.data.Message);
                                        });
                                }
                            });
                        },
                        'click .approve': function (e, value, row, index) {
                            e.preventDefault();

                            showBootboxConfirm("Approve Yarn PO", "Are you sure you want to approve this PO?", function (yes) {
                                if (yes) {
                                    var url = "/api/ypo/approve-ypo/" + row.Id;
                                    axios.post(url)
                                        .then(function () {
                                            toastr.success(constants.APPROVE_SUCCESSFULLY);
                                            getYarnPOMasterData();
                                        })
                                        .catch(function (error) {
                                            toastr.error(error.response.data.Message);
                                        });
                                }
                            });
                        },
                        'click .reject': function (e, value, row, index) {
                            e.preventDefault();

                            showBootboxPrompt("Reject Yarn PO", "Are you sure you want to Reject this PO?", function (result) {
                                if (result) {
                                    var data = {
                                        Id: row.Id,
                                        UnapproveReason: result
                                    };

                                    axios.post("/api/ypo/reject-ypo", data)
                                        .then(function () {
                                            toastr.success(constants.REJECT_SUCCESSFULLY);
                                            getYarnPOMasterData();
                                        })
                                        .catch(function (error) {
                                            toastr.error(error.response.data.Message);
                                        });
                                }
                            });
                        }
                    }
                },
                {
                    field: "PoNo",
                    title: "PO No",
                    filterControl: "input",
                    width: 100,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    footerFormatter: function () {
                        return [
                            '<span >',
                            '<label title="Total">',
                            '<i style="font-size:15px"></i>',
                            ' Total:',
                            '</label>',
                            '</span>'
                        ].join('');
                    }
                },
                {
                    field: "PoDateStr",
                    title: "PO Date",
                    filterControl: "input",
                    width: 80,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "CompanyName",
                    title: "Company",
                    filterControl: "input",
                    width: 60,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "SupplierName",
                    title: "Supplier",
                    filterControl: "input",
                    width: 180,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "QuotationRefNo",
                    title: "Ref No",
                    filterControl: "input",
                    width: 80,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                //{
                //    field: "POFor",
                //    title: "PO For",
                //    filterControl: "input",
                //    width: 80,
                //    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                //},
                {
                    field: "DeliveryStartDateStr",
                    title: "Delivery Start",
                    filterControl: "input",
                    width: 80,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "DeliveryEndDateStr",
                    title: "Delivery End",
                    filterControl: "input",
                    width: 80,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "TotalQty",
                    title: "Total Qty",
                    filterControl: "input",
                    align: 'right',
                    footerFormatter: calculateTotalYarnQtyAll,
                    width: 60,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "TotalValue",
                    title: "Total Value",
                    filterControl: "input",
                    align: 'right',
                    footerFormatter: calculateTotalYarnValueAll,
                    width: 60,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "UserName",
                    title: "Created By",
                    filterControl: "input",
                    width: 80,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "POStatus",
                    title: "PO Status",
                    filterControl: "input",
                    width: 150,
                    visible: isShowingAllPO,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "InHouseDateStr",
                    title: "In-house Date",
                    filterControl: "input",
                    width: 80,
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

                getYarnPOMasterData();
            },
            onSort: function (name, order) {
                tableParams.sort = name;
                tableParams.order = order;
                tableParams.offset = 0;

                getYarnPOMasterData();
            },
            onRefresh: function () {
                resetTableParams();
                getYarnPOMasterData();
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

                getYarnPOMasterData();
            }
        });
    }

    function getYarnPOMasterData() {
        var queryParams = $.param(tableParams);
        $tblYPOEl.bootstrapTable('showLoading');
        var url = `/api/ypo/list?status=${status}&${queryParams}`;
        axios.get(url)
            .then(function (response) {
                $tblYPOEl.bootstrapTable('load', response.data);
                $tblYPOEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }
    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();

        getYarnPOMasterData();
    }
    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#Id").val(-1111);
        $formEl.find("#EntityState").val(4);
    }
    function calculateTotalYarnQtyAll(data) {
        var yarnPoQtyAll = 0;

        $.each(data, function (i, row) {
            yarnPoQtyAll += isNaN(parseFloat(row.TotalQty)) ? 0 : parseFloat(row.TotalQty);
        });

        return yarnPoQtyAll.toFixed(2);
    }
    function calculateTotalYarnValueAll(data) {
        var yarnPoValueAll = 0;

        $.each(data, function (i, row) {
            yarnPoValueAll += isNaN(parseFloat(row.TotalValue)) ? 0 : parseFloat(row.TotalValue);
        });

        return yarnPoValueAll.toFixed(2);
    }

    function getMasterTblRowActions(row) {
        var rowActions = [];
        rowActions = ['<span class="btn-group">',
            '<a class="btn btn-xs btn-primary new" href="javascript:void(0)" title="New PI">',
            '<i class="fa fa-plus" aria-hidden="true"></i>',
            '</a>',
            '<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportId=990&PONo=' + row.PoNo + '" target="_blank" title="PO Report">',
            '<i class="fa fa-file-pdf-o" aria-hidden="true"></i>',
            '</a>',
            '</span>'];
        return rowActions.join(' ');
    }
})();
(function () {
    //'use strict'

    // #region variables
    var menuId, pageName;
    var toolbarId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $formEl;
    var isShowingAllPO = false;

    var yarnPurchaseOrder = {};
    var yarnPoOrdersSplit = [];
    var filterBy = {};
    var yarnPOStatus = 1;
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
        $tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        if (pageName == "YarnPurchaseOrderApproval") {
            yarnPOStatus = 2; // Peding for approval
            $toolbarEl.find("#btnYPONew").hide();
            $toolbarEl.find("#btnYPOPending").hide();
            toggleActiveToolbarBtn($toolbarEl.find("#btnYPOPendingforApproval"), $toolbarEl);
        }

        initYarnPOMasterTable();
        getYarnPOMasterData();
        initExportOrderListsTable();

        // Init Validation
        validationConstraints = getConstraints();
        initializeValidation($formEl, validationConstraints);

        // #region Toolbar button click events
        $toolbarEl.find("#btnYPONew").on("click", function (e) {
            e.preventDefault();

            toggleActiveToolbarBtn(this, $toolbarEl);
            resetForm();
            $formEl.find("#btnSaveYPO").fadeIn();
            $formEl.find("#btnSaveAndProposeYPO").fadeIn();
            $formEl.find("#btnApproveYPO").fadeOut();
            $formEl.find("#btnRejectYPO").fadeOut();
            $divTblEl.fadeOut();
            $formEl.find("#SupplierTNA").fadeOut();
            $divDetailsEl.fadeIn();

            var today = new Date();
            var datetoday = (today.getMonth() + 1) + '/' + today.getDate() + '/' + today.getFullYear();
            $formEl.find("#PoDate").val(datetoday);

            getYPONewYarnPurchaseData();
            $formEl.find("#RevisionArea").fadeOut();
        });

        $toolbarEl.find("#btnYPOPending").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            yarnPOStatus = 1;
            isShowingAllPO = false;
            initYarnPOMasterTable();
            getYarnPOMasterData();
            toggleActiveToolbarBtn(this, $toolbarEl);
            $formEl.find("#RevisionArea").fadeOut();
        });

        $toolbarEl.find("#btnYPOPendingforApproval").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            yarnPOStatus = 2;
            isShowingAllPO = false;
            initYarnPOMasterTable();
            getYarnPOMasterData();
            toggleActiveToolbarBtn(this, $toolbarEl);
            $formEl.find("#RevisionArea").fadeOut();
        });

        $toolbarEl.find("#btnYPOApproved").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            yarnPOStatus = 3;
            isShowingAllPO = false;
            initYarnPOMasterTable();
            getYarnPOMasterData();
            toggleActiveToolbarBtn(this, $toolbarEl);
            $formEl.find("#RevisionArea").fadeIn();
        });

        $toolbarEl.find("#btnYPOUnApproved").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            yarnPOStatus = 4;
            isShowingAllPO = false;
            initYarnPOMasterTable();
            getYarnPOMasterData();
            toggleActiveToolbarBtn(this, $toolbarEl);
            $formEl.find("#RevisionArea").fadeOut();
        });

        $toolbarEl.find("#btnYPOALL").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            yarnPOStatus = 5;
            isShowingAllPO = true;
            initYarnPOMasterTable();
            getYarnPOMasterData();
            toggleActiveToolbarBtn(this, $toolbarEl);
            $formEl.find("#RevisionArea").fadeOut();
        });
        // #endregion

        if ($formEl.find("#IsRevision").prop("checked", true)) {
            $formEl.find("#IsRevision").prop("checked", true);
            $formEl.find("#IsCancel").prop("checked", false);
        }

        // #region Form Events
        // #region Form Elements Events
        $formEl.find("#PoDate").datepicker({
            endDate: "0d",
            todayHighlight: true,
            autoclose: true
        });

        $formEl.find("#DeliveryStartDate").datepicker({
            todayHighlight: true,
            autoclose: true
        });

        $formEl.find("#DeliveryEndDate").datepicker({
            todayHighlight: true,
            autoclose: true
        });

        $formEl.find("#SupplierId").on("select2:select", function (e) {
            if ($formEl.find("#CompanyId").val())
                getSupplierAdditionalInformation(e.params.data.id, $formEl.find("#CompanyId").val());

            $formEl.find("#SupplierTNA").fadeIn();
        });

        $formEl.find("#CompanyId").on("select2:select", function (e) {
            if ($formEl.find("#SupplierId").val())
                getSupplierAdditionalInformation($formEl.find("#SupplierId").val(), e.params.data.id);
        });

        $formEl.find("#PaymentTermsId").on("select2:select", function (e) {
            if (e.params.data.id == "1") {
                showHideLCSection(false);
            }
            else {
                showHideLCSection(true);
            }
        });

        $formEl.find("#TypeOfLcid").on("select2:select", function (e) {
            if (e.params.data.id == "1") {
                $formEl.find("#formGroupCreditDays").fadeOut();
            }
            else {
                $formEl.find("#formGroupCreditDays").fadeIn();
            }
        });

        $formEl.find('#DeliveryStartDate').datepicker()
            .on('changeDate', function (ev) {
                startDate = new Date(ev.date.getFullYear(), ev.date.getMonth(), ev.date.getDate(), 0, 0, 0);
                if (pODate != null && pODate != 'undefined') {
                    if (startDate < pODate) {
                        bootbox.alert({
                            size: "small",
                            title: "Alert !!!",
                            message: "Start Date can't less than PO Date.",
                            callback: function () {
                                $formEl.find("#DeliveryStartDate").val("");
                            }
                        })
                    }
                }
            });

        $formEl.find("#PoDate").datepicker()
            .on("changeDate", function (ev) {
                pODate = new Date(ev.date.getFullYear(), ev.date.getMonth(), ev.date.getDate(), 0, 0, 0);
                if (startDate != null && startDate != 'undefined') {
                    if (startDate < pODate) {
                        bootbox.alert({
                            size: "small",
                            title: "Alert !!!",
                            message: "Start Date can't less than PO Date.",
                            callback: function () {
                                $formEl.find("#DeliveryEndDate").val("");
                            }
                        })
                    }
                }
            });

        $formEl.find('#DeliveryStartDate').datepicker()
            .on('changeDate', function (ev) {
                startDate = new Date(ev.date.getFullYear(), ev.date.getMonth(), ev.date.getDate(), 0, 0, 0);
                if (endDate != null && endDate != 'undefined') {
                    if (endDate < startDate) {
                        bootbox.alert({
                            size: "small",
                            title: "Alert !!!",
                            message: "End Date can't less than Start Date.",
                            callback: function () {
                                $formEl.find("#DeliveryStartDate").val("");
                            }
                        })
                    }
                }
            });

        $formEl.find("#DeliveryEndDate").datepicker()
            .on("changeDate", function (ev) {
                endDate = new Date(ev.date.getFullYear(), ev.date.getMonth(), ev.date.getDate(), 0, 0, 0);
                if (startDate != null && startDate != 'undefined') {
                    if (endDate < startDate) {
                        bootbox.alert({
                            size: "small",
                            title: "Alert !!!",
                            message: "End Date can't less than Start Date.",
                            callback: function () {
                                $formEl.find("#DeliveryEndDate").val("");
                            }
                        })
                    }
                }
            });
        // #endregion

        // #region Form Action Events
        $formEl.find("#btnAddBuyers").on("click", function (e) {
            e.preventDefault();
            $("#modal-child-Buyer").modal('show');
            getBuyerListsFromBuyerCompanyYarnPO();
        });

        $formEl.find("#btnYPOAddItemOrders").on("click", function (e) {
            e.preventDefault();

            var newYarnPoChildData = {
                Id: getMaxIdForArray(yarnPurchaseOrder.YarnPoChilds, "Id"),
                YpoMasterId: 0,
                YarnSubProgramIds: 0,
                YarnSubProgramNames: '',
                YarnCategory: "",
                NoOfThread: 0,
                YarnLotNo: "",
                PoQty: 0,
                Rate: 0,
                Remarks: "",
                HSCode: "",
                value: 0,
                SubGroupId: 0,
                ItemMasterId: 0,
                UnitId: 28,
                DisplayUnitDesc: "Kg",
                YarnProgramId: 0,
                YarnProgram: "",
                Segment1ValueId: 0,
                Segment1ValueDesc: "",
                Segment2ValueId: 0,
                Segment2ValueDesc: "Empty",
                Segment3ValueId: 0,
                Segment3ValueDesc: "",
                Segment4ValueId: 0,
                Segment4ValueDesc: "",
                Segment5ValueId: 0,
                Segment5ValueDesc: "",
                Segment6ValueId: 0,
                Segment6ValueDesc: "",
                Segment7ValueId: 0,
                Segment7ValueDesc: "",
                Segment8ValueId: 0,
                Segment8ValueDesc: "",
                Segment9ValueId: 0,
                Segment9ValueDesc: "",
                Segment10ValueId: 0,
                Segment10ValueDesc: "",
                Segment11ValueId: 0,
                Segment11ValueDesc: "",
                Segment12ValueId: 0,
                Segment12ValueDesc: "",
                Segment13ValueId: 0,
                Segment13ValueDesc: "",
                Segment14ValueId: 0,
                Segment14ValueDesc: "",
                Segment15ValueId: 0,
                Segment15ValueDesc: "",
                YarnChildPoEWOs: "",
                YarnChildPoBuyers: "",
                EntityState: 4
            };

            yarnPurchaseOrder.YarnPoChilds.push(newYarnPoChildData);
            $formEl.find("#tblYarnPurchaseOrderItems").bootstrapTable('load', yarnPurchaseOrder.YarnPoChilds);
        });

        $formEl.find("#btnViewDetailsTNA").on("click", function (e) {
            e.preventDefault();
            $("#modal-child-Yarn-TNA").modal('show');
        });

        $formEl.find("#btnEditCancelYarnPO").on("click", function (e) {
            e.preventDefault();
            backToList();
            toggleActiveToolbarBtn($formEl.find("#btnYPOPending"), $toolbarEl);
        });

        $formEl.find("#btnSaveYPO").click(function (e) {
            e.preventDefault();
            saveYPO(this);
        });

        $formEl.find("#btnSaveAndProposeYPO").click(function (e) {
            e.preventDefault();
            saveYPO(this, true);
        });

        $formEl.find("#btnApproveYPO").click(function (e) {
            e.preventDefault();
            var l = $(this).ladda();
            l.ladda('start');

            var url = "/scdapi/approve-ypo/" + $formEl.find("#Id").val();
            axios.post(url)
                .then(function () {
                    l.ladda('stop');
                    toastr.success(constants.PROPOSE_SUCCESSFULLY);
                    backToList();
                })
                .catch(function (error) {
                    l.ladda('stop');
                    toastr.error(error.response.data.Message);
                });
        });

        $formEl.find("#btnRejectYPO").click(function (e) {
            e.preventDefault();
            var l = $(this).ladda();
            l.ladda('start');

            showBootboxPrompt("Reject Yarn PO", "Are you sure you want to Reject this PO?", function (result) {
                if (result) {
                    var data = {
                        Id: $formEl.find("#Id").val(),
                        UnapproveReason: result
                    };

                    axios.post("/scdapi/reject-ypo", data)
                        .then(function () {
                            l.ladda('stop');
                            toastr.success(constants.REJECT_SUCCESSFULLY);
                            backToList();
                        })
                        .catch(function (error) {
                            l.ladda('stop');
                            toastr.error(error.response.data.Message);
                        });
                }
            });
        });
        // #endregion

        // #endregion
    });

    function initYarnPOMasterTable() {
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
                    title: 'Actions',
                    align: 'center',
                    width: 100,
                    formatter: function (value, row, index, field) {
                        return getMasterTblRowActions(row);
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            $formEl.find("#Id").val(row.Id);

                            switch (yarnPOStatus) {
                                case 1:
                                    $formEl.find("#btnApproveYPO").fadeOut();
                                    $formEl.find("#btnRejectYPO").fadeOut();
                                    $formEl.find("#btnSaveYPO").fadeIn();
                                    $formEl.find("#btnSaveAndProposeYPO").fadeIn();
                                    break;
                                case 2:
                                    if (pageName == "YarnPurchaseOrderApproval") {
                                        $formEl.find("#btnSaveYPO").fadeOut();
                                        $formEl.find("#btnSaveAndProposeYPO").fadeOut();
                                        $formEl.find("#btnApproveYPO").fadeIn();
                                        $formEl.find("#btnRejectYPO").fadeIn();
                                    }
                                    else {
                                        $formEl.find("#btnApproveYPO").fadeOut();
                                        $formEl.find("#btnRejectYPO").fadeOut();
                                        $formEl.find("#btnSaveYPO").fadeIn();
                                        $formEl.find("#btnSaveAndProposeYPO").fadeIn();
                                    }
                                    break;
                                case 4:
                                    $formEl.find("#btnApproveYPO").fadeOut();
                                    $formEl.find("#btnRejectYPO").fadeOut();
                                    $formEl.find("#btnSaveYPO").fadeIn();
                                    $formEl.find("#btnSaveAndProposeYPO").fadeIn();
                                default:
                                    break;
                            }

                            getYarnPurchaseDataEdit(row.Id);
                        },
                        'click .propose': function (e, value, row, index) {
                            e.preventDefault();

                            showBootboxConfirm("Propose Yarn PO", "Are you sure you want to propose this PO?", function (yes) {
                                if (yes) {
                                    var url = "/scdapi/propose-ypo/" + row.Id;
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
                                    var url = "/scdapi/approve-ypo/" + row.Id;
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

                                    axios.post("/scdapi/reject-ypo", data)
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
                    field: "SupplierRefNo",
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
        $tblMasterEl.bootstrapTable('showLoading');
        var url = "/scdapi/yarnpolists" + "?YarnPOStatus=" + yarnPOStatus + "&" + queryParams;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
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

    function getExportOrdersFromBuyerCompanyYarnPO() {
        var queryParams = $.param(exportOrderTableParams);
        url = "/scdapi/orderlistsfromcompany" + "?" + queryParams;
        axios.get(url)
            .then(function (response) {
                $pageEl.find("#tblYPOExportOrderLists").bootstrapTable('load', response.data);
            })
            .catch(function () {
                toastr.error(err.response.data.Message);
            })
    };

    function initExportOrderListsTable() {
        $pageEl.find("#tblYPOExportOrderLists").bootstrapTable('destroy');
        $pageEl.find("#tblYPOExportOrderLists").bootstrapTable({
            pagination: true,
            filterControl: true,
            searchOnEnterKey: true,
            sidePagination: "server",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            columns: [
                {
                    field: "BuyerName",
                    title: "Buyer Name",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BuyerTeam",
                    title: "Buyer Team",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ExportOrderNo",
                    title: "Export Order No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "StyleNo",
                    title: "Style No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                }
            ],
            onDblClickRow: function (row, $element, field) {
                if (yarnPurchaseOrder.YarnPoForOrders.find(function (el) { return el.ExportOrderId === row.ExportOrderId })) {
                    toastr.warning(row.ExportOrderNo + " is already added.");
                    return;
                }
                else {
                    $pageEl.find("#modal-child-yarnPOExportOrder").modal('hide');
                    yarnPurchaseOrder.YarnPoForOrders.push(row);
                    yarnPoOrdersSplit.push(row);
                    $formEl.find("#tblYarnPOforExportOrders").bootstrapTable('load', yarnPurchaseOrder.YarnPoForOrders);
                }
            },
            onPageChange: function (number, size) {
                var newOffset = (number - 1) * size;
                var newLimit = size;
                if (exportOrderTableParams.offset == newOffset && exportOrderTableParams.limit == newLimit)
                    return;

                exportOrderTableParams.offset = newOffset;
                exportOrderTableParams.limit = newLimit;
                getExportOrdersFromBuyerCompanyYarnPO();
            },
            onSort: function (name, order) {
                exportOrderTableParams.sort = name;
                exportOrderTableParams.order = order;
                exportOrderTableParams.offset = 0;

                getExportOrdersFromBuyerCompanyYarnPO();
            },
            onRefresh: function () {
                resetTableParams();
                getExportOrdersFromBuyerCompanyYarnPO();
            },
            onColumnSearch: function (columnName, filterValue) {
                if (columnName in ExportOrderfilterBy && !filterValue) {
                    delete ExportOrderfilterBy[columnName];
                }
                else
                    ExportOrderfilterBy[columnName] = filterValue;

                if (Object.keys(ExportOrderfilterBy).length === 0 && ExportOrderfilterBy.constructor === Object)
                    exportOrderTableParams.filter = "";
                else
                    exportOrderTableParams.filter = JSON.stringify(ExportOrderfilterBy);

                getExportOrdersFromBuyerCompanyYarnPO();
            }
        });
    }

    function initTblYarnPurchaseOrderItems() {
        $formEl.find("#tblYarnPurchaseOrderItems").bootstrapTable('destroy');
        $formEl.find("#tblYarnPurchaseOrderItems").bootstrapTable({
            uniqueId: 'Id',
            showFooter: true,
            //detailView: true,
            columns: [
                {
                    formatter: function (value, row, index, field) {
                        return ['<span class="btn-group">',
                            '<a class="btn btn-xs btn-danger remove" onclick="javascript:void(0)" title="Remove EWO/Booking">',
                            '<i class="fa fa-remove" aria-hidden="true"></i>',
                            '</a>',
                            '</span>'].join(' ');
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            e.preventDefault();
                            $pageEl.find("#tblYarnPurchaseOrderItems").bootstrapTable('remove', { field: 'Id', values: [row.Id] });
                        },
                    }
                },
                {
                    field: "YarnProgramId",
                    title: "Yarn Program",
                    editable: {
                        type: 'select2',
                        title: 'Select Yarn Type',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: yarnPurchaseOrder.YarnProgramList,
                        select2: { width: 130, placeholder: 'Yarn Type', alloclear: true }
                    },
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
                    field: "Segment1ValueId",
                    title: "Yarn Type",
                    editable: {
                        type: 'select2',
                        title: 'Select Yarn Composition',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: yarnPurchaseOrder.YarnTypeList,
                        select2: { width: 200, placeholder: 'Yarn Composition', alloclear: true }
                    }
                },
                {
                    field: "Segment3ValueId",
                    title: "Yarn Composition",
                    editable: {
                        type: 'select2',
                        title: 'Select Yarn Composition',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: yarnPurchaseOrder.YarnCompositionList,
                        select2: { width: 200, placeholder: 'Yarn Composition', alloclear: true }
                    }
                },
                {
                    field: "Segment2ValueId",
                    title: "Yarn Count",
                    formatter: function (value, row, index, field) {
                        return ['<span class="btn-group">',
                            '<a href="javascript:void(0)" class="editable-link edit">' + row.Segment2ValueDesc + '</a>',
                            '</span>'].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            debugger;
                            if (!row.Segment1ValueId) return toastr.error("Yarn Type is not selected");

                            getYarnCountByYarnType(row.Segment1ValueId, row);
                        },
                    }
                },
                {
                    field: "NoOfThread",
                    title: "No Of Thread",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                //{
                //    field: "YarnSubProgramIds",
                //    title: "Yarn Sub Program",
                //    editable: {
                //        type: 'select2',
                //        inputclass: 'input-sm',
                //        showbuttons: true,
                //        source: yarnPurchaseOrder.YarnSubProgramList,
                //        select2: { width: 250, height: 100, multiple: true, placeholder: 'Select Sub Program' }
                //    }
                //},
                {
                    field: "YarnSubProgramNames",
                    title: "Yarn Sub Program",
                    formatter: function (value, row, index, field) {
                        var text = row.YarnSubProgramNames ? row.YarnSubProgramNames : "Empty";
                        return `<a href="javascript:void(0)" class="editable-link edit">${text}</a>`;
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            showBootboxSelect2MultipleDialog("Select Yarn Sub-Programs", "YarnSubProgramIds", "Select Yarn Sub-Programs", yarnPurchaseOrder.YarnSubProgramList, function (result) {
                                if (result) {
                                    row.YarnSubProgramIds = result.map(function (item) { return item.id }).join(",");
                                    row.YarnSubProgramNames = result.map(function (item) { return item.text }).join(",");
                                    $formEl.find("#tblYarnPurchaseOrderItems").bootstrapTable('updateByUniqueId', { id: row.Id, row: row });
                                    console.log(result);
                                }
                            });
                        },
                    }
                },
                {
                    field: "YarnCategory",
                    title: "Yarn Category"
                },
                {
                    field: "Segment5ValueId",
                    title: "Yarn Color",
                    editable: {
                        type: 'select2',
                        title: 'Select Yarn Color',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: yarnPurchaseOrder.YarnColorList,
                        select2: { width: 130, placeholder: 'Yarn Color', alloclear: true }
                    }
                },
                {
                    field: "Segment4ValueDesc",
                    title: "Shade",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "YarnLotNo",
                    title: "Lot No/Reference",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "PoQty",
                    title: "PO Qty",
                    align: 'right',
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm m-w-50',
                        showbuttons: false
                    },
                    footerFormatter: calculateTotalYarnPIQty,
                    cellStyle: function () { return { classes: 'm-w-50' } }
                },
                {
                    field: "UnitId",
                    title: "Unit",
                    visible: false
                },
                {
                    field: "DisplayUnitDesc",
                    title: "Unit"
                },
                {
                    field: "Rate",
                    align: 'right',
                    title: "Rate",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    },
                    cellStyle: function () { return { classes: 'm-w-50' } }
                },
                {
                    field: "PIValue",
                    title: "Total Value",
                    footerFormatter: calculateTotalYarnPIValue
                },
                {
                    field: "HSCode",
                    title: "H.S Code",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "PoForId",
                    title: "PO For",
                    editable: {
                        type: 'select2',
                        title: 'Select Yarn Color',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: yarnPurchaseOrder.PIForList,
                        select2: { width: 100, placeholder: 'PO For', alloclear: true }
                    }
                },
                {
                    field: "YarnChildPoBuyers",
                    title: "Buyer",
                    formatter: function (value, row, index, field) {
                        var text = row.YarnChildPoBuyers ? row.YarnChildPoBuyers : "Empty";
                        return `<a href="javascript:void(0)" class="editable-link edit">${text}</a>`;
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            showBootboxSelect2MultipleDialog("Select Buyers", "YarnChildPoBuyerIds", "Select Buyers", yarnPurchaseOrder.BuyerList, function (result) {
                                if (result) {
                                    row.YarnChildPoBuyerIds = result.map(function (item) { return item.id }).join(",");
                                    row.YarnChildPoBuyers = result.map(function (item) { return item.text }).join(",");
                                    $formEl.find("#tblYarnPurchaseOrderItems").bootstrapTable('updateByUniqueId', { id: row.Id, row: row });
                                    console.log(result);
                                }
                            });
                        },
                    }
                },
                {
                    field: "YarnChildPoEWOs",
                    title: "EWOs",
                    formatter: function (value, row, index, field) {
                        var text = row.YarnChildPoEWOs ? row.YarnChildPoEWOs : "Empty";
                        return `<a href="javascript:void(0)" class="editable-link edit">${text}</a>`;
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            showBootboxSelect2MultipleDialog("Select EWO", "YarnChildPoExportIds", "Select EWO", yarnPurchaseOrder.ExportOrderList, function (result) {
                                if (result) {
                                    row.YarnChildPoExportIds = result.map(function (item) { return item.id }).join(",");
                                    row.YarnChildPoEWOs = result.map(function (item) { return item.text }).join(",");
                                    $formEl.find("#tblYarnPurchaseOrderItems").bootstrapTable('updateByUniqueId', { id: row.Id, row: row });
                                    console.log(result);
                                }
                            });
                        },
                    }
                },
                //{
                //    field: "YarnChildPoBuyerIds",
                //    title: "Buyer",
                //    editable: {
                //        type: 'select2',
                //        inputclass: 'input-sm',
                //        showbuttons: true,
                //        source: yarnPurchaseOrder.BuyerList,
                //        select2: { width: 250, height: 100, multiple: true, placeholder: 'Select Buyer' }
                //    }
                //},
                //{
                //    field: "YarnChildPoExportIds",
                //    title: "Export Order",
                //    editable: {
                //        type: 'select2',
                //        inputclass: 'input-sm',
                //        showbuttons: true,
                //        source: yarnPurchaseOrder.ExportOrderList,
                //        select2: { width: 250, height: 100, multiple: true, placeholder: 'Select Export Order' }
                //    }
                //},
                {
                    field: "Remarks",
                    title: "Special Specifications",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                }
            ],
            onEditableSave: function (field, row, oldValue, $el) {
                debugger;
                var selectedValue = { id: "", text: "" };
                switch (field) {
                    //case "YarnSubProgramIds":
                    //    if (row.YarnSubProgramIds) {
                    //        selectedValue = setMultiSelectValueInBootstrapTableEditable(yarnPurchaseOrder.YarnSubProgramList, row.YarnSubProgramIds);
                    //        //row.YarnSubProgramIds = selectedValue.id;
                    //        row.YarnSubProgramNames = selectedValue.text;
                    //    }
                    //    break;
                    case "YarnChildPoBuyerIds":
                        if (row.YarnChildPoBuyerIds) {
                            selectedValue = setMultiSelectValueInBootstrapTableEditable(yarnPurchaseOrder.BuyerList, row.YarnChildPoBuyerIds);
                            row.YarnChildPoBuyerIds = selectedValue.id;
                        }
                        break;
                    case "YarnChildPoExportIds":
                        if (row.YarnChildPoExportIds) {
                            selectedValue = setMultiSelectValueInBootstrapTableEditable(yarnPurchaseOrder.ExportOrderList, row.YarnChildPoExportIds);
                            row.YarnChildPoExportIds = selectedValue.id;
                        }
                        break;
                    case "Segment1ValueId":
                        if (row.Segment1ValueId) {
                            selectedValue = yarnPurchaseOrder.YarnTypeList.find(function (el) { return el.id == row.Segment1ValueId });
                            row.Segment1ValueDesc = selectedValue.text;
                        }
                        break;
                    case "Segment3ValueId":
                        if (row.Segment3ValueId) {
                            selectedValue = yarnPurchaseOrder.YarnCompositionList.find(function (el) { return el.id == row.Segment3ValueId });
                            row.Segment3ValueDesc = selectedValue.text;
                        }
                        break;
                    case "YarnProgramId":
                        if (row.YarnProgramId) {
                            selectedValue = yarnPurchaseOrder.YarnProgramList.find(function (el) { return el.id == row.YarnProgramId });
                            row.YarnProgram = selectedValue.text;
                        }
                        break;
                    case "Segment5ValueId":
                        if (row.Segment5ValueId) {
                            selectedValue = yarnPurchaseOrder.YarnColorList.find(function (el) { return el.id == row.Segment5ValueId });
                            row.Segment5ValueDesc = selectedValue.text;
                        }
                        break;
                    default:
                        break;
                }

                row.PIValue = (row.PoQty * row.Rate).toFixed(2);
                row.YarnCategory = calculateYarnCategory(row);
                if ((row.Segment1ValueId == 625) || (row.Segment1ValueId == 8238)) {
                    row.NoOfThread = "0";
                }
                $formEl.find("#tblYarnPurchaseOrderItems").bootstrapTable('load', yarnPurchaseOrder.YarnPoChilds);
            },
            onExpandRow: function (index, row, $detail) {
                if ($formEl.find("#Id").val() > 0) {
                    $formEl.find("#GFPChildId").val(row.GFPChildId);
                    var url = "/GFPApi/greyFabricProductionChildRollEdit/" + $("#GFPChildId").val();
                    axios.get(url)
                        .then(function (response) {
                            $childTableEl = $("#tblYarnPOItemsSplitData-" + row.GFPChildId);
                            var data = $childTableEl.bootstrapTable("getData");
                            data.push(response.data);
                            $childTableEl.bootstrapTable("load", data);

                            initYarnPurchaseOrderPOItemsSplitDataTable(index, data);
                        })
                        .catch(function () {
                            toastr.error(constants.LOAD_ERROR_MESSAGE);
                        })
                }
                else {
                    $childTableEl = $detail.html('<table id="tblYarnPOItemsSplitData-' + index + '"></table>').find('table');
                    initYarnPurchaseOrderPOItemsSplitDataTable(index, yarnPoOrdersSplit);
                }
            }
        });
    }

    function initYarnPurchaseOrderPOItemsSplitDataTable(rowId, data) {
        $childTableEl.bootstrapTable({
            showFooter: true,
            data: data,
            rowStyle: function (row, index) {
                if (row.EntityState == 8)
                    return { classes: 'deleted-row' };

                return "";
            },
            columns: [
                //{
                //    title: 'Actions',
                //    align: 'center',
                //    width: 120,
                //    formatter: function () {
                //        return [
                //            '<span class="btn-group">',
                //            '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete Item">',
                //            '<i class="fa fa-remove" style="font-size:15px"></i>',
                //            '</a>',
                //            '</span>'
                //        ].join('');
                //    },
                //    footerFormatter: function () {
                //        return [
                //            '<span class="btn-group">',
                //            '<button class="btn btn-success btn-xs edit" onclick="return addYPOChildSplitDataRow(event, ' + rowId + ')" title="Add">',
                //            '<i class="fa fa-plus" style="font-size:15px"></i>',
                //            ' Add',
                //            '</button>',
                //            '</span>'
                //        ].join('');
                //    },
                //    events: {
                //        'click .remove': function (e, value, row, index) {
                //            this.data[index].EntityState = 8;
                //            var $target = $(e.target);
                //            $target.closest("tr").addClass('deleted-row');
                //        }
                //    }
                //},
                {
                    field: "BuyerName",
                    title: "Buyer Name",
                    filterControl: "input"
                },
                {
                    field: "ExportOrderNo",
                    title: "Export Order No",
                    filterControl: "input"
                },
                {
                    field: "BuyerTeam",
                    title: "Buyer Team",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "POSplitQty",
                    title: "Split Qty (KG)",
                    filterControl: "input",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                }
            ]
        });
    }

    function calculateTotalYarnPIQty(data) {
        var yarnPoQty = 0;

        $.each(data, function (i, row) {
            yarnPoQty += isNaN(parseFloat(row.PoQty)) ? 0 : parseFloat(row.PoQty);
        });

        return yarnPoQty.toFixed(2);
    }

    function calculateTotalYarnPIValue(data) {
        var yarnPoValue = 0;

        $.each(data, function (i, row) {
            yarnPoValue += isNaN(parseFloat(row.PIValue)) ? 0 : parseFloat(row.PIValue);
        });

        return yarnPoValue.toFixed(2);
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

    function getSupplierAdditionalInformation(supplierId, companyId) {
        url = "/SCDApi/SupplierAdditionalInformation/" + supplierId + "/" + companyId;

        axios.get(url)
            .then(function (response) {
                var data = response.data;
                initSelect2($formEl.find("#IncoTermsId"), data.IncoTermsList);
                initSelect2($formEl.find("#PaymentTermsId"), data.PaymentTermsList);
                initSelect2($formEl.find("#ShipmentModeId"), data.ShipmentModeList);
                initSelect2($formEl.find("#CountryOfOriginId"), data.CountryOfOriginList);
                initSelect2($formEl.find("#CreditDays"), data.LCTenureList);
                initSelect2($formEl.find("#CalculationofTenure"), data.CalculationOfTenureList);
                initSelect2($formEl.find("#PortOfLoading"), data.PortOfLoadingList);
                initSelect2($formEl.find("#PortofDischarge"), data.PortOfDischargeList);

                setSelect2Data($formEl.find("#IncoTermsId"), data.IncoTermsId);
                setSelect2Data($formEl.find("#PaymentTermsId"), data.PaymentTermsId);
                setSelect2Data($formEl.find("#ShipmentModeId"), data.ShipmentModeId);
                setSelect2Data($formEl.find("#CountryOfOriginId"), data.CountryOfOriginId);
                setSelect2Data($formEl.find("#CalculationofTenure"), data.CalculationofTenure);
                setSelect2Data($formEl.find("#CreditDays"), data.CreditDays);
                setSelect2Data($formEl.find("#PortOfLoading"), data.PortofLoading);
                setSelect2Data($formEl.find("#PortofDischarge"), data.PortofDischarge);

                if (data.PaymentTermsId === 2)
                    showHideLCSection(true);
                else
                    showHideLCSection(false);

                if (data.PortofLoading === 105)
                    showHideSupplierRegionSection(false);
                else
                    showHideSupplierRegionSection(true);

                if (data.TypeOfLcid === 2)
                    $formEl.find("#formGroupCreditDays").fadeIn();
                else
                    $formEl.find("#formGroupCreditDays").fadeOut();

                $formEl.find("#ShippingTolerance").val(data.ShippingTolerance);
                $formEl.find("#InHouseDate").val(data.InHouseDateStr);
                $pageEl.find("#SFToPLDate").text(data.SFToPLDateStr);
                $pageEl.find("#PLToPDDate").text(data.PLToPDDateStr);
                $pageEl.find("#PDToCFDate").text(data.PDToCFDateStr);
                $pageEl.find("#SFToPLDays").text(data.SFToPLDays);
                $pageEl.find("#PLToPDDays").text(data.PLToPDDays);
                $pageEl.find("#PDToCFDays").text(data.PDToCFDays);
                $pageEl.find("#InHouseDays").text(data.InHouseDays);

                var m_names = ['Jan', 'Feb', 'Mar',
                    'Apr', 'May', 'Jun', 'Jul',
                    'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
                var today = new Date();
                var datetoday = (today.getDate() + '-' + m_names[today.getMonth()] + '-' + today.getFullYear());
                $("#PODateCurrent").text(datetoday);
            })
            .catch(function (err) {
                console.log(err.response.data);
            })
    }

    function setSupplierAdditionalInformations(data) {
        initSelect2($formEl.find("#IncoTermsId"), data.IncoTermsList);
        initSelect2($formEl.find("#PaymentTermsId"), data.PaymentTermsList);
        initSelect2($formEl.find("#ShipmentModeId"), data.ShipmentModeList);
        initSelect2($formEl.find("#CountryOfOriginId"), data.CountryOfOriginList);
        initSelect2($formEl.find("#CreditDays"), data.LCTenureList);
        initSelect2($formEl.find("#CalculationofTenure"), data.CalculationOfTenureList);
        initSelect2($formEl.find("#PortOfLoading"), data.PortOfLoadingList);
        initSelect2($formEl.find("#PortofDischarge"), data.PortOfDischargeList);
        initSelect2($formEl.find("#TypeOfLcid"), data.LCTypeList);

        setSelect2Data($formEl.find("#IncoTermsId"), data.IncoTermsId);
        setSelect2Data($formEl.find("#PaymentTermsId"), data.PaymentTermsId);
        setSelect2Data($formEl.find("#ShipmentModeId"), data.ShipmentModeId);
        setSelect2Data($formEl.find("#CountryOfOriginId"), data.CountryOfOriginId);
        setSelect2Data($formEl.find("#CreditDays"), data.CreditDays);
        setSelect2Data($formEl.find("#CalculationofTenure"), data.CalculationofTenure);
        setSelect2Data($formEl.find("#PortOfLoading"), data.PortofLoading);
        setSelect2Data($formEl.find("#PortofDischarge"), data.PortofDischarge);
        setSelect2Data($formEl.find("#TypeOfLcid"), data.TypeOfLcid);

        if (data.PaymentTermsId === 1)
            showHideLCSection(false);
        else
            showHideLCSection(true);

        if (data.PortofLoading === 105)
            showHideSupplierRegionSection(false);
        else
            showHideSupplierRegionSection(true);

        if (data.TypeOfLcid === 2) {
            $formEl.find("#formGroupCreditDays").fadeIn();
        }
        else
            $formEl.find("#formGroupCreditDays").fadeOut();

        $formEl.find("#ShippingTolerance").val(data.ShippingTolerance);

        $formEl.find("#SupplierTNA").fadeIn();
        $formEl.find("#InHouseDate").val(data.InHouseDateStr);
        $pageEl.find("#SFToPLDate").text(data.SFToPLDateStr);
        $pageEl.find("#PLToPDDate").text(data.PLToPDDateStr);
        $pageEl.find("#PDToCFDate").text(data.PDToCFDateStr);
        $pageEl.find("#SFToPLDays").text(data.SFToPLDays);
        $pageEl.find("#PLToPDDays").text(data.PLToPDDays);
        $pageEl.find("#PDToCFDays").text(data.PDToCFDays);
        $pageEl.find("#InHouseDays").text(data.InHouseDays);
        $pageEl.find("#PODateCurrent").text(data.ApprovedDateStr);
    }

    function getYPONewYarnPurchaseData() {
        url = "/scdapi/newyarnpurchaseorder/";
        axios.get(url)
            .then(function (response) {
                yarnPurchaseOrder = response.data;
                initSelect2($formEl.find("#CompanyId"), response.data.CompanyList);
                initSelect2($formEl.find("#SupplierId"), response.data.SupplierListYarn);
                initSelect2($formEl.find("#TypeOfLcid"), response.data.LCTypeList);
                initSelect2($formEl.find("#PoForId"), response.data.PIForList);
                initSelect2($formEl.find("#ExportIds"), response.data.ExportOrderList);

                if ($formEl.find("#TypeOfLcid").val() == "1") {
                    $formEl.find("#formGroupCreditDays").fadeOut();
                }
                else if ($formEl.find("#TypeOfLcid").val() == "2") {
                    $formEl.find("#formGroupCreditDays").fadeIn();
                }
                else {
                    $formEl.find("#formGroupCreditDays").fadeOut();
                }

                initSelect2($formEl.find("#OfferValidity"), yarnPurchaseOrder.OfferValidityList);
                $formEl.find("#OfferValidity").val(yarnPurchaseOrder.OfferValidity).trigger("change");
                initSelect2($formEl.find("#QualityApprovalProcedureId"), yarnPurchaseOrder.QualityApprovalProcedureList);
                $formEl.find("#QualityApprovalProcedureId").val(yarnPurchaseOrder.QualityApprovalProcedureId).trigger("change");

                initTblYarnPurchaseOrderItems();
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
            })
            .catch(function (err) {
                console.log(err.response.data);
            })
    }

    function getYarnPurchaseDataEdit(id) {
        url = "/scdapi/yarnpurchaseorder/" + id;
        axios.get(url)
            .then(function (response) {
                $divTblEl.fadeOut();
                $divDetailsEl.fadeIn();
                yarnPurchaseOrder = response.data;
                $formEl.find("#Id").val(yarnPurchaseOrder.Id);
                $formEl.find("#lblPONo").text(yarnPurchaseOrder.PoNo);
                $formEl.find("#PoNo").val(yarnPurchaseOrder.PoNo).fadeIn();
                $formEl.find("#PoDate").val(yarnPurchaseOrder.PoDateStr);
                $formEl.find("#SupplierRefNo").val(yarnPurchaseOrder.SupplierRefNo);
                $formEl.find("#DeliveryStartDate").val(yarnPurchaseOrder.DeliveryStartDateStr);
                $formEl.find("#DeliveryEndDate").val(yarnPurchaseOrder.DeliveryEndDateStr);
                $formEl.find("#Remarks").val(yarnPurchaseOrder.Remarks);
                $formEl.find("#InternalNotes").val(yarnPurchaseOrder.InternalNotes);
                $formEl.find("#Charges").val(yarnPurchaseOrder.Charges);
                $formEl.find("#ShippingTolerance").val(yarnPurchaseOrder.ShippingTolerance);
                debugger;
                setCheckBox($formEl.find("#TransShipmentAllow"), yarnPurchaseOrder.TransShipmentAllow);

                initSelect2($formEl.find("#CompanyId"), yarnPurchaseOrder.CompanyList);
                $formEl.find("#CompanyId").val(yarnPurchaseOrder.CompanyId).trigger("change");

                initSelect2($formEl.find("#SupplierId"), yarnPurchaseOrder.SupplierListYarn);
                $formEl.find("#SupplierId").val(yarnPurchaseOrder.SupplierId).trigger("change");

                setSupplierAdditionalInformations(response.data);

                initSelect2($formEl.find("#OfferValidity"), yarnPurchaseOrder.OfferValidityList);
                $formEl.find("#OfferValidity").val(yarnPurchaseOrder.OfferValidity).trigger("change");
                initSelect2($formEl.find("#QualityApprovalProcedureId"), yarnPurchaseOrder.QualityApprovalProcedureList);
                $formEl.find("#QualityApprovalProcedureId").val(yarnPurchaseOrder.QualityApprovalProcedureId).trigger("change");
                $formEl.find("#Charges").val(yarnPurchaseOrder.Charges);

                initTblYarnPurchaseOrderItems();
                $formEl.find("#tblYarnPurchaseOrderItems").bootstrapTable('load', yarnPurchaseOrder.YarnPoChilds);
            })
            .catch(function (err) {
                console.log(err.response.data);
            })
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#Id").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function saveYPO(caller, isPropose) {
        var l = $(caller).ladda();
        l.ladda('start');

        $formEl.find(':checkbox').each(function () {
            this.value = this.checked;
        });

        var data = formDataToJson($formEl.serializeArray());

        if (data.YarnChildPoBuyerIds == null) {
            data.YarnChildPoBuyerIds = "0";
        }

        if (data.YarnChildPoExportIds == null) {
            data.YarnChildPoExportIds = "0";
        }

        data.Proposed = isPropose ? true : false;

        data["YarnPoChilds"] = yarnPurchaseOrder.YarnPoChilds;
        data["YarnPoForOrders"] = yarnPurchaseOrder.YarnPoForOrders;

        if (!validateSave()) {
            l.ladda('stop');
            return toastr.error("Please correct all validation errors.");
        }

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/scdapi/yarnpurchaseorder", data, config)
            .then(function (response) {
                l.ladda('stop');
                showBootboxAlert("Your PO saved successfully." + "<br> PO No: <b>" + response.data + "</b>");
                backToList();
            })
            .catch(function (error) {
                l.ladda('stop');
                toastr.error(error.response.data.Message);
            });
    }

    function getMasterTblRowActions(row) {
        var rowActions = [];
        switch (yarnPOStatus) {
            case 1:
                rowActions = ['<span class="btn-group">',
                    '<a class="btn btn-xs btn-primary edit" href="javascript:void(0)" title="Edit PO">',
                    '<i class="fa fa-edit" aria-hidden="true"></i>',
                    '</a>',
                    '<a class="btn btn-xs btn-primary propose" href="javascript:void(0)" target="_blank" title="Propose PO">',
                    '<i class="fa fa-sticky-note-o" aria-hidden="true"></i>',
                    '</a>',
                    '<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportId=990&PONo=' + row.PoNo + '" target="_blank" title="PO Report">',
                    '<i class="fa fa-file-pdf-o" aria-hidden="true"></i>',
                    '</a>',
                    '</span>'];
                break;
            case 2:
                if (pageName == "YarnPurchaseOrderApproval") {
                    rowActions = ['<span class="btn-group">',
                        '<a class="btn btn-xs btn-primary edit" href="javascript:void(0)" title="View Details">',
                        '<i class="fa fa-eye" aria-hidden="true"></i>',
                        '</a>',
                        '<a class="btn btn-xs btn-success approve" href="javascript:void(0)" title="Approve PO">',
                        '<i class="fa fa-check" aria-hidden="true"></i>',
                        '</a>',
                        '<a class="btn btn-xs btn-danger reject" href="javascript:void(0)" title="Reject PO">',
                        '<i class="fa fa-ban" aria-hidden="true"></i>',
                        '</a>',
                        '<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportId=990&PONo=' + row.PoNo + '" target="_blank" title="PO Report">',
                        '<i class="fa fa-file-pdf-o" aria-hidden="true"></i>',
                        '</a>',
                        '</span>'];
                }
                else {
                    rowActions = ['<span class="btn-group">',
                        '<a class="btn btn-xs btn-primary edit" href="javascript:void(0)" title="Edit PO">',
                        '<i class="fa fa-edit" aria-hidden="true"></i>',
                        '</a>',
                        '<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportId=990&PONo=' + row.PoNo + '" target="_blank" title="PO Report">',
                        '<i class="fa fa-file-pdf-o" aria-hidden="true"></i>',
                        '</a>',
                        '</span>'];
                }
                break;
            case 3:
                rowActions = ['<span class="btn-group">',
                    '<a class="btn btn-xs btn-primary edit" href="javascript:void(0)" title="Revise PO">',
                    '<i class="fa fa-edit" aria-hidden="true"></i>',
                    '</a>',
                    '<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportId=990&PONo=' + row.PoNo + '" target="_blank" title="PO Report">',
                    '<i class="fa fa-file-pdf-o" aria-hidden="true"></i>',
                    '</a>',
                    '</span>'];
                break;
            case 5:
                rowActions = ['<span class="btn-group">',
                    '<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportId=990&PONo=' + row.PoNo + '" target="_blank" title="PO Report">',
                    '<i class="fa fa-file-pdf-o" aria-hidden="true"></i>',
                    '</a>',
                    '</span>'];
                break;
            case 4:
                if (pageName == "YarnPurchaseOrderApproval") {
                    rowActions = ['<span class="btn-group">',
                        '<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportId=990&PONo=' + row.PoNo + '" target="_blank" title="PO Report">',
                        '<i class="fa fa-file-pdf-o" aria-hidden="true"></i>',
                        '</a>',
                        '</span>'];
                }
                else {
                    rowActions = ['<span class="btn-group">',
                        '<a class="btn btn-xs btn-primary edit" href="javascript:void(0)" title="Edit PO">',
                        '<i class="fa fa-edit" aria-hidden="true"></i>',
                        '</a>',
                        '<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportId=990&PONo=' + row.PoNo + '" target="_blank" title="PO Report">',
                        '<i class="fa fa-file-pdf-o" aria-hidden="true"></i>',
                        '</a>',
                        '</span>'];
                }
                break;
            default:
                break;
        }

        return rowActions.join(' ');
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();

        showHideLCSection(false);
        showHideSupplierRegionSection(false);
        $formEl.find("#tblYarnPurchaseOrderItems").bootstrapTable('destroy');
        $formEl.find("#tblYarnPOforExportOrders").bootstrapTable('load', yarnPurchaseOrder.YarnPoForOrders);
        $formEl.find("#formGroupPoForDetails").remove();

        $divTblEl.fadeIn();

        getYarnPOMasterData();
    }

    function getYarnCountByYarnType(yarnTypeId, rowData) {
        var url = "/api/selectoption/yarn-count-by-yarn-type/" + yarnTypeId;
        axios.get(url)
            .then(function (response) {
                var yarnCountList = convertToSelectOptions(response.data);
                showBootboxSelectPrompt("Select Yarn Count", yarnCountList, "", function (result) {
                    if (!result)
                        return toastr.warning("You didn't selected any Yarn Count.");

                    var selectedYarnCount = yarnCountList.find(function (el) { return el.value === result })
                    rowData.Segment2ValueId = result;
                    rowData.Segment2ValueDesc = selectedYarnCount.text;
                    rowData.YarnCategory = calculateYarnCategory(rowData);
                    if ((rowData.Segment1ValueId == 625) || (rowData.Segment1ValueId == 8238)) {
                        rowData.NoOfThread = "0";
                        isNoOfThread = false;
                    }
                    else {
                        rowData.NoOfThread = "1";
                        isNoOfThread = true;
                    }
                    $formEl.find("#tblYarnPurchaseOrderItems").bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
                })
            })
            .catch(function (err) {
                console.log(err);
            });
    }

    function getConstraints() {
        return {
            PoDate: {
                presence: true,
            },
            CompanyId: {
                presence: true
            },
            SupplierId: {
                presence: true
            },
            //PoForId: {
            //    presence: true
            //},
            CurrencyId: {
                presence: true
            },
            SupplierRefNo: {
                length: {
                    maximum: 100
                }
            },
            DeliveryStartDate: {
                presence: true
            },
            DeliveryStartDate: {
                presence: true
            },
            Remarks: {
                length: {
                    maximum: 500
                }
            },
            InternalNotes: {
                length: {
                    maximum: 500
                }
            },
            IncoTermsId: {
                presence: true
            },
            PaymentTermsId: {
                presence: true
            },
            ReImbursementCurrencyId: {
                presence: true
            },
            Charges: {
                length: {
                    maximum: 500
                }
            },
            CountryOfOriginId: {
                presence: true
            },
            UnapproveReason: {
                length: {
                    maximum: 500
                }
            },
            ShippingTolerance: {
                numericality: {
                    onlyInteger: true,
                    greaterThanOrEqualTo: 0,
                    lessThanOrEqualTo: 10
                }
            }
        };
    }

    function validateSave() {
        var isValid = false;
        if (!isValidForm($formEl, validationConstraints))
            return isValid;
        else
            hideValidationErrors($formEl);

        isValid = true;
        $.each(yarnPurchaseOrder.YarnPoChilds, function (i, child) {
            if (!child.YarnProgramId) {
                toastr.error("Yarn program is required.");
                isValid = false;
            }

            if (!child.Segment1ValueId) {
                toastr.error("Yarn type is required.");
                isValid = false;
            }

            if (!child.Segment2ValueId) {
                toastr.error("Yarn count is required.");
                isValid = false;
            }

            if (!child.Segment3ValueId) {
                toastr.error("Yarn composition is required.");
                isValid = false;
            }

            //if (!child.Segment5ValueId) {
            //    toastr.error("Yarn color is required.");
            //    isValid = false;
            //}

            if (child.PoQty <= 0) {
                toastr.error("POQty must be greater than 0");
                isValid = false;
            }

            if (child.Rate <= 0) {
                toastr.error("Rate must be greater than 0");
                isValid = false;
            }

            return false;
        });

        return isValid;
    }

    function showHideLCSection(show) {
        if (show) {
            $formEl.find("#formGroupTypeOfLcid").show();
            $formEl.find("#formGroupCalculationofTenure").show();
        }
        else {
            $formEl.find("#formGroupTypeOfLcid").hide();
            $formEl.find("#formGroupCalculationofTenure").hide();
        }
    }

    function showHideSupplierRegionSection(show) {  // Supplier was local or foreign
        if (show) {
            $formEl.find("#formGroupPortOfLoading").show();
            $formEl.find("#formGroupPortOfDischarge").show();
            $formEl.find("#formGroupQuantityApprovalProcedure").show();
        }
        else {
            $formEl.find("#formGroupPortOfLoading").hide();
            $formEl.find("#formGroupPortOfDischarge").hide();
            $formEl.find("#formGroupQuantityApprovalProcedure").hide();
        }
    }
})();
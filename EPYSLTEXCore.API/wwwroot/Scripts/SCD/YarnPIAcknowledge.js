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
    var isCDAPage = false;
    var status;

    var YarnPIReview;

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
        isCDAPage = convertToBoolean($(`#${pageId}`).find("#CDAPage").val());

        status = statusConstants.COMPLETED;
        initMasterTable();
        getMasterTableData();

        $toolbarEl.find("#btnAproved").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;

            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnAcKnowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.ACKNOWLEDGE;

            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnUnAcKnowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.UN_ACKNOWLEDGE;

            initMasterTable();
            getMasterTableData();
        });

        $formEl.find("#btnAcknowlege").click(function (e) {
            e.preventDefault();
            save(this, true);
        });
        $formEl.find("#btnUnAcknowlege").click(function (e) {
            e.preventDefault();
            bootbox.prompt("Enter your unacknowledge reason:", function (result) {
                if (!result) {
                    return toastr.error("Unacknowledge reason is required.");
                }
                var id = $formEl.find("#YPIReceiveMasterID").val();
                var reason = result;
                axios.post(`/api/yarn-pi-acknowledge/reject/${id}/${reason}`)
                    .then(function () {
                        toastr.success("Unacknowledged successfully.");
                        backToList();
                    })
                    .catch(showResponseError);
            });
            //save(this);
        });

        $formEl.find("#btnCancel").on("click", backToList);

    });

    function initMasterTable() {
        $tblMasterEl.bootstrapTable('destroy');
        $tblMasterEl.bootstrapTable({
            showRefresh: true,
            showExport: true,
            showColumns: true,
            toolbar: $toolbarEl,
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
                    field: "Actions",
                    title: "Actions",
                    align: "center",
                    width: 50,
                    formatter: function (value, row, index, field) {
                        if (status === statusConstants.COMPLETED) {
                            return `<span class="btn-group">
                                        <a class="btn btn-xs btn-primary m-w-30 add" href="javascript:void(0)" title="Review PI">
                                            <i class="fa fa-plus" aria-hidden="true"></i>
                                        </a>
                                        <a class="btn btn-xs btn-primary m-w-30" href="${row.PIFilePath}" target="_blank" title="View PI">
                                            <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                        </a>
                                    </span>`;
                        }
                        else {
                            return `<span class="btn-group">
                                        <a class="btn btn-xs btn-primary m-w-30 edit" href="javascript:void(0)" title="Review PI">
                                            <i class="fa fa-edit" aria-hidden="true"></i>
                                        </a>
                                        <a class="btn btn-xs btn-primary m-w-30" href="${row.PIFilePath}" target="_blank" title="View PI">
                                            <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                        </a>
                                    </span>`;
                        }
                    },
                    events: {
                        'click .add': function (e, value, row, index) {
                            e.preventDefault();
                            resetForm();
                            getData(row.YPIReceiveMasterID, row.SupplierID, row.CompanyID);
                        },
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            resetForm();
                            getData(row.YPIReceiveMasterID, row.SupplierID, row.CompanyID);
                        }
                    }
                },
                {
                    field: "YPINo",
                    title: "PI No",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "PIDate",
                    title: "PI Date",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "SupplierName",
                    title: "Supplier",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "POQty",
                    title: "PO Qty",
                    align: 'right',
                    cellStyle: function () { return { classes: 'm-w-50' } }
                },
                {
                    field: "PIQty",
                    title: "PI Qty",
                    filterControl: "input",
                    align: 'right',
                    cellStyle: function () { return { classes: 'm-w-50' } }
                },
                {
                    field: "PIValue",
                    title: "PI Value",
                    filterControl: "input",
                    align: 'right',
                    cellStyle: function () { return { classes: 'm-w-50' } }
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
        var url = `/api/yarn-pi-acknowledge/list?gridType=bootstrap-table&status=${status}&isCDAPage=${isCDAPage}&${queryParams}`;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        getMasterTableData();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#YPIReceiveMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
        $formEl.find("#divReject,#btnAcknowlege,#btnUnAcknowlege,#btnReject,#divCreditDays,#divCreditDaysPO,#divCreditDaysMsg").fadeOut();
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getData(id, supplierId, companyId) {
        axios.get(`/api/yarn-pi-acknowledge/getData/${id}/${supplierId}/${companyId}/${isCDAPage}`)
            .then(function (response) {
                
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                YarnPIReview = response.data;
                YarnPIReview.PIDate = formatDateToDefault(YarnPIReview.PIDate);
                setFormData($formEl, YarnPIReview);
                $formEl.find("#TotalQty").val(YarnPIReview.YarnPO.TotalQty);
                $formEl.find("#TotalValue").val(YarnPIReview.YarnPO.TotalValue);
                $formEl.find("#TypeOfLcPO").val(YarnPIReview.YarnPO.TypeOfLC);
                $formEl.find("#ShippingTolerancePO").val(YarnPIReview.YarnPO.ShippingTolerance);
                $formEl.find("#CreditDaysPO").val(YarnPIReview.YarnPO.CreditDays);
                $formEl.find("#IsCDA").val(isCDAPage);
                if (YarnPIReview.TypeOfLcId != LcType.Usance) {
                    if (YarnPIReview.PIQty == YarnPIReview.YarnPO.TotalQty && YarnPIReview.PIValue == YarnPIReview.YarnPO.TotalValue && YarnPIReview.TypeOfLCID == YarnPIReview.YarnPO.TypeOfLcId
                        && YarnPIReview.ShippingTolerance == YarnPIReview.YarnPO.ShippingTolerance) {
                        $formEl.find("#lblQty,#lblValue,#lblLcType,#lblTolerance").text("Matched!");


                        //if (!YarnPIReview.Acknowledge)
                        //    $formEl.find("#btnAcknowlege").fadeIn();
                        //if (!YarnPIReview.UnAcknowledge)
                        //    $formEl.find("#btnUnAcknowlege").fadeIn();
                        if (status == statusConstants.COMPLETED) {
                            $formEl.find("#btnUnAcknowlege").fadeIn();
                            $formEl.find("#btnAcknowlege").fadeIn();
                        }
                        if (status == statusConstants.UN_ACKNOWLEDGE) {
                            $formEl.find("#btnUnAcknowlege").fadeOut();
                            $formEl.find("#btnAcknowlege").fadeOut();
                        }
                        if (status == statusConstants.ACKNOWLEDGE) {
                            $formEl.find("#btnUnAcknowlege").fadeOut();
                           /* $formEl.find("#btnAcknowlege").fadeOut();*/
                        }
                    } else {
                      

                        $formEl.find("#btnUnAcknowlege").fadeIn();
                       /* $formEl.find("#btnAcknowlege").fadeIn();*/
                      
                        //$formEl.find("#divReject").fadeIn();
                        if (YarnPIReview.PIQty != YarnPIReview.YarnPO.TotalQty) $formEl.find("#lblQty").text("Not Matched!");
                        else $formEl.find("#lblQty").text("Matched!");
                        if (YarnPIReview.PIValue != YarnPIReview.YarnPO.TotalValue) $formEl.find("#lblValue").text("Not Matched!");
                        else $formEl.find("#lblValue").text("Matched!");
                        if (YarnPIReview.TypeOfLcId != YarnPIReview.YarnPO.TypeOfLcId) $formEl.find("#lblLcType").text("Not Matched!");
                        else $formEl.find("#lblLcType").text("Matched!");
                        if (YarnPIReview.ShippingTolerance != YarnPIReview.YarnPO.ShippingTolerance) $formEl.find("#lblTolerance").text("Not Matched!");
                        else $formEl.find("#lblTolerance").text("Matched!");
                        //if (!YarnPIReview.Reject)
                        //    $formEl.find("#btnReject").fadeIn();
                    }

                }
                else {
                    $formEl.find("#divCreditDays,#divCreditDaysPO,#divCreditDaysMsg").fadeIn();
                    if (YarnPIReview.PIQty == YarnPIReview.YarnPO.TotalQty && YarnPIReview.PIValue == YarnPIReview.YarnPO.TotalValue && YarnPIReview.TypeOfLcId == YarnPIReview.YarnPO.TypeOfLcId
                        && YarnPIReview.ShippingTolerance == YarnPIReview.YarnPO.ShippingTolerance && YarnPIReview.CreditDays == YarnPIReview.YarnPO.CreditDays) {
                        $formEl.find("#lblQty,#lblValue,#lblLcType,#lblTolerance,#lblCreditDays").text("Matched!");
                        //if (!YarnPIReview.Acknowledge)
                        //    $formEl.find("#btnAcknowlege").fadeIn();
                        //   if (!YarnPIReview.UnAcknowledge)
                        //    $formEl.find("#btnUnAcknowlege").fadeIn();
                        if (status == statusConstants.COMPLETED) {
                            $formEl.find("#btnUnAcknowlege").fadeIn();
                            $formEl.find("#btnAcknowlege").fadeIn();
                        }
                        if (status == statusConstants.UN_ACKNOWLEDGE) {
                            $formEl.find("#btnUnAcknowlege").fadeOut();
                            $formEl.find("#btnAcknowlege").fadeOut();
                        }
                        if (status == statusConstants.ACKNOWLEDGE) {
                            $formEl.find("#btnUnAcknowlege").fadeOut();
                            $formEl.find("#btnAcknowlege").fadeOut();
                        }
                    } else {
                       
                        //$formEl.find("#divReject").fadeIn();
                        if (YarnPIReview.PIQty != YarnPIReview.YarnPO.TotalQty) $formEl.find("#lblQty").text("Not Matched!");
                        else $formEl.find("#lblQty").text("Matched!");
                        if (YarnPIReview.PIValue != YarnPIReview.YarnPO.TotalValue) $formEl.find("#lblValue").text("Not Matched!");
                        else $formEl.find("#lblValue").text("Matched!");
                        if (YarnPIReview.TypeOfLcId != YarnPIReview.YarnPO.TypeOfLcId) $formEl.find("#lblLcType").text("Not Matched!");
                        else $formEl.find("#lblLcType").text("Matched!");
                        if (YarnPIReview.ShippingTolerance != YarnPIReview.YarnPO.ShippingTolerance) $formEl.find("#lblTolerance").text("Not Matched!");
                        else $formEl.find("#lblTolerance").text("Matched!");
                        if (YarnPIReview.CreditDays != YarnPIReview.YarnPO.CreditDays) $formEl.find("#lblCreditDays").text("Not Matched!");
                        else $formEl.find("#lblCreditDays").text("Matched!");
                        //if (!YarnPIReview.Reject)
                        //    $formEl.find("#btnReject").fadeIn();
                    }
                }

                
                $tblChildEl.bootstrapTable("load", YarnPIReview.KRolls);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save(invokedBy, isAcknowledge = false) {
        var data = YarnPIReview;// formDataToJson($formEl.serializeArray()); //data["KRolls"] = YarnPIReview.KRolls;
        if (isAcknowledge)
            data.Acknowledge = true;
        else {
          
            data.UnAcknowledge = true;
            data.UnAcknowledgeReason = $formEl.find("#RejectReason").val();
            alert(data.UnAcknowledgeReason);
        }

        axios.post("/api/yarn-pi-acknowledge/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }


})();
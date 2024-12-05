(function () {
    'use strict'

    // #region variables
    var masterData = {};

    var menuId, pageName;
    var toolbarId;
    var pageId, $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $formEl, $tblChildEl;
    var showAllPO = false;
    var filterBy = {};
    var status;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var validationConstraints = [];
    var supplierId, supplierName, prMasterId, companyId;
    var isApprovePage = false;

    // #endregion

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $pageEl = $(pageConstants.PAGE_ID_PREFIX + pageId);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        $tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        isApprovePage = convertToBoolean($pageEl.find("#IsApprovePage").val());
        if (isApprovePage) {
            status = statusConstants.PROPOSED; // Pending for approval
            $toolbarEl.find("#btnYPONew,#btnAwaitingProposeList").hide();
            $formEl.find("#btnSave").hide();
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingApprovalList"), $toolbarEl);
        }
        else {
            status = statusConstants.AWAITING_PROPOSE;
            $toolbarEl.find("#btnYPONew,#btnAwaitingProposeList").show();
            $formEl.find("#btnSave").show();
        }

        initMasterTable();
        getMasterTableData();

        // Init Validation
        validationConstraints = getConstraints();
        initializeValidation($formEl, validationConstraints);

        // #region Toolbar button click events
        $toolbarEl.find("#btnYPONew").click(btnNewClick);

        $toolbarEl.find("#btnAwaitingProposeList").click(btnAwaitingProposeListClick);

        $toolbarEl.find("#btnPendingApprovalList").click(btnPendingApprovalListClick);

        $toolbarEl.find("#btnApprovedList").click(btnApprovedListClick);

        $toolbarEl.find("#btnRejectList").click(btnRejectListClick);

        $toolbarEl.find("#btnAllList").click(btnAllListClick);
        // #endregion

        $formEl.find('#IsRevision').click(function () {
            if ($(this).is(':checked')) {
                controlReset(true);
            } else {
                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnPropose").fadeOut();
            }
        });
        $formEl.find('#IsRevision').change(function () {
            if ($(this).is(':checked')) {
                controlReset(true);
            }
        });
        $formEl.find('#IsCancel').click(function () {
            if ($(this).is(':checked')) {
                controlReset(false);
            } else {
                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnPropose").fadeOut();
            }
        });
        $formEl.find('#IsCancel').change(function () {
            if ($(this).is(':checked')) {
                controlReset(false);
            }
        });
        // #region Form Events
        // #region Form Elements Events
        //$formEl.find("#CompanyId").on("select2:select", getPortofDischarge);

        $formEl.find("#PaymentTermsId").on("select2:select", function (e) {
            if (e.params.data.id == "1") {
                showHideLCSection(false); $formEl.find("#CurrencyId").val(2); $formEl.find("#CurrencyCode").val('USD'); $formEl.find("#ReImbursementCurrencyId").val(2); $formEl.find("#ReImbursmentCurrency").val('USD')
            }
            else if (e.params.data.text == "A/C Payee Cheque") {
                showHideLCSection(false); $formEl.find("#CurrencyId").val(1); $formEl.find("#CurrencyCode").val('BDT'); $formEl.find("#ReImbursementCurrencyId").val(1); $formEl.find("#ReImbursmentCurrency").val('BDT')
            }
            else {
                showHideLCSection(true);
                $formEl.find("#CurrencyId").val(2); $formEl.find("#CurrencyCode").val('USD');$formEl.find("#ReImbursementCurrencyId").val(2); $formEl.find("#ReImbursmentCurrency").val('USD')
            }
        });

        $formEl.find("#TypeOfLcId").on("select2:select", function (e) {
            if (e.params.data.id == "1") $formEl.find("#formGroupCreditDays").fadeOut();
            else $formEl.find("#formGroupCreditDays").fadeIn();
        });

        $formEl.find('#DeliveryStartDate').on('changeDate', function (ev) {
            var deliveryStartDate = moment(ev.date);
            if (deliveryStartDate.isBefore($formEl.find("#PoDate").val())) return showBootboxAlert("Delivery start date can not be before PO date.");

            $pageEl.find("#PODateCurrent").text($formEl.find('#DeliveryStartDate').val());
            $pageEl.find("#SFToPLDate").text(moment(deliveryStartDate).add(masterData.SFToPLDays, 'd').format(dateFormats.DEFAULT));
            $pageEl.find("#PLToPDDate").text(moment(deliveryStartDate).add(masterData.SFToPLDays + masterData.PLToPDDays, 'd').format(dateFormats.DEFAULT));
            $pageEl.find("#PDToCFDate").text(moment(deliveryStartDate).add(masterData.InHouseDays, 'd').format(dateFormats.DEFAULT));
            $pageEl.find("#InHouseDate").val(moment(deliveryStartDate).add(masterData.InHouseDays, 'd').format(dateFormats.DEFAULT));
        });

        $formEl.find("#PoDate").on("changeDate", function (ev) {
            var pODate = moment(ev.date);
            if (pODate.isAfter($formEl.find("#DeliveryStartDate").val())) return showBootboxAlert("Delivery start date can not be before PO date.");
        });

        $formEl.find('#DeliveryEndDate').on('changeDate', function (ev) {
            var endDate = moment(ev.date);
            if (endDate.isBefore($formEl.find("#DeliveryStartDate").val())) return showBootboxAlert("Delivery end date can not be before Delivery start date.");
        });

        // #endregion

        // #region Form Action Events
        $formEl.find("#btnChangeSupplier").click(changeSupplier);

        $formEl.find("#btnAddBuyers").on("click", function (e) {
            e.preventDefault();
            $("#modal-child-Buyer").modal('show');
            getBuyerListsFromBuyerCompanyYarnPO();
        });

        $formEl.find("#btnYPOAddItemOrders").click(addNewItem);

        $formEl.find("#btnViewDetailsTNA").click(function (e) {
            e.preventDefault();
            $("#modal-child-Yarn-TNA").modal('show');
        });

        $formEl.find("#btnBack").click(function (e) {
            e.preventDefault();
            backToList();
            toggleActiveToolbarBtn($formEl.find("#btnAwaitingProposeList"), $toolbarEl);
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            saveYPO();
        });

        $formEl.find("#btnPropose").click(function (e) {
            e.preventDefault();
            saveYPO(true);
        });

        $formEl.find("#btnApprove").click(function (e) {
            e.preventDefault();

            var url = "/api/ypo/approve-ypo/" + $formEl.find("#YPOMasterID").val();
            axios.post(url)
                .then(function () {
                    toastr.success(constants.PROPOSE_SUCCESSFULLY);
                    backToList();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnReject").click(rejectPO);
    });

    function controlReset(revision) {
        if (revision) {
            $formEl.find("#IsCancel").prop("checked", false);
            $formEl.find("#RevisionReason").fadeIn();
            $formEl.find("#lblRevisionReason").fadeIn();
            $formEl.find("#CancelReason").fadeOut();
            $formEl.find("#lblCancelReason").fadeOut();

        } else {
            $formEl.find("#IsRevision").prop("checked", false);
            $formEl.find("#RevisionReason").fadeOut();
            $formEl.find("#lblRevisionReason").fadeOut();
            $formEl.find("#CancelReason").fadeIn();
            $formEl.find("#lblCancelReason").fadeIn();

        }
        $formEl.find("#btnSave").fadeIn();
        $formEl.find("#btnPropose").fadeIn();
    }

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
                    title: '',
                    align: 'center',
                    formatter: function (value, row, index, field) {
                        return getMasterTblRowActions(row);
                    },
                    events: {
                        'click .add': showSupplierSelectionForPR,
                        'click .edit': getDetails,
                        'click .propose': proposePO,
                        'click .approve': approvePO,
                        'click .reject': rejectPO
                    }
                },
                {
                    field: "PRNO",
                    title: "PR No",
                    filterControl: "input",
                    visible: status === statusConstants.PENDING,
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
                    field: "PRDate",
                    title: "PR Date",
                    visible: status === statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "PRByUser",
                    title: "PR By",
                    visible: status === statusConstants.PENDING
                },
                {
                    field: "PoNo",
                    title: "PO No",
                    filterControl: "input",
                    visible: status !== statusConstants.PENDING,
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
                    field: "PoDate",
                    title: "PO Date",
                    visible: status !== statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "RevisionNo",
                    title: "Revision No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "RevisionDate",
                    title: "Revision Date",
                    visible: status !== statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "CompanyName",
                    title: "Company",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "SupplierName",
                    title: "Supplier",
                    filterControl: "input",
                    visible: status !== statusConstants.PENDING,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "QuotationRefNo",
                    title: "Ref No",
                    filterControl: "input",
                    visible: status !== statusConstants.PENDING,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "DeliveryStartDate",
                    title: "Delivery Start",
                    visible: status !== statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "DeliveryEndDate",
                    title: "Delivery End",
                    visible: status !== statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "TotalQty",
                    title: "Total Qty",
                    filterControl: "input",
                    align: 'right',
                    footerFormatter: calculateTotalYarnQtyAll,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "TotalValue",
                    title: "Total Value",
                    filterControl: "input",
                    align: 'right',
                    footerFormatter: calculateTotalYarnValueAll,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "POStatus",
                    title: "PO Status",
                    filterControl: "input",
                    width: 150,
                    visible: showAllPO,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "InHouseDate",
                    title: "In-house Date",
                    visible: status !== statusConstants.PENDING,
                    width: 80,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "UserName",
                    title: "Created By",
                    filterControl: "input",
                    visible: status !== statusConstants.PENDING,
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
        var url = `/api/ypo/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(showResponseError)
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function initChildTable() {
        $tblChildEl.bootstrapTable('destroy');
        $tblChildEl.bootstrapTable({
            uniqueId: 'YPOChildID',
            showFooter: true,
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
                            $tblChildEl.bootstrapTable('remove', { field: 'YPOChildID', values: [row.YPOChildID] });
                        },
                    }
                },
                {
                    field: "YarnProgramId",
                    title: "Yarn Program",
                    editable: {
                        type: 'select2',
                        title: 'Select Yarn Program',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: masterData.YarnProgramList,
                        select2: { width: 130, placeholder: 'Yarn Program', allowClear: true }
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
                        source: masterData.YarnTypeList,
                        select2: { width: 200, placeholder: 'Yarn Composition', allowClear: true }
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
                        source: masterData.YarnCompositionList,
                        select2: { width: 200, placeholder: 'Yarn Composition', allowClear: true }
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
                            var subProgramIds;
                            if (row.YarnSubProgramIds) subProgramIds = row.YarnSubProgramIds.split(',');

                            showBootboxSelect2MultipleDialog("Select Yarn Sub-Programs", "YarnSubProgramIds", "Select Yarn Sub-Programs", masterData.YarnSubProgramList, function (result) {
                                if (result) {
                                    row.YarnSubProgramIds = result.map(function (item) { return item.id }).join(",");
                                    row.YarnSubProgramNames = result.map(function (item) { return item.text }).join(",");
                                    $tblChildEl.bootstrapTable('updateByUniqueId', { id: row.YPOChildID, row: row });
                                }
                            }, subProgramIds);
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
                        source: masterData.YarnColorList,
                        select2: { width: 130, placeholder: 'Yarn Color', allowClear: true }
                    }
                },
                {
                    field: "Segment4ValueDesc",
                    title: "Shade",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        source: masterData.ShadeList,
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
                    field: "UnitID",
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
                    title: "Purchase For",
                    editable: {
                        type: 'select2',
                        title: 'Select Yarn Color',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: masterData.PIForList,
                        select2: { width: 100, placeholder: 'PO For', allowClear: true }
                    }
                },
                {
                    field: "BuyerNames",
                    title: "Buyer",
                    formatter: function (value, row, index, field) {
                        var text = row.BuyerNames ? row.BuyerNames : "Empty";
                        return `<a href="javascript:void(0)" class="editable-link edit">${text}</a>`;
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            //var selectedValues = row.YarnChildPoBuyerIds.split(",");
                            showBootboxSelect2MultipleDialog("Select Buyers", "YarnChildPoBuyerIds", "Select Buyers", masterData.BuyerList, function (result) {
                                if (result) {
                                    row.YarnChildPoBuyerIds = result.map(function (item) { return item.id }).join(",");
                                    row.BuyerNames = result.map(function (item) { return item.text }).join(",");
                                    $tblChildEl.bootstrapTable('updateByUniqueId', { id: row.YPOChildID, row: row });
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
                            if (!row.YarnChildPoBuyerIds) {
                                toastr.info("You must select buyer first.");
                                return;
                            }

                            var finder = new commonFinder({
                                title: "Select EWO",
                                pageId: pageId,
                                height: 320,
                                apiEndPoint: `/api/ypo/ewo-list/${row.YarnChildPoBuyerIds}`,
                                fields: "EWONo,IsSample,BuyerName,BuyerTeam",
                                headerTexts: "EWO,Is Sample?,Buyer,Buyer Team",
                                isMultiselect: true,
                                autofitColumns: true,
                                primaryKeyColumn: "EWONo",
                                seperateSelection: false,
                                onMultiselect: function (selectedRecords) {
                                    finder.hideModal();

                                    row["YarnPOChildOrders"] = [];
                                    var selectedEWOArray = [];
                                    for (var i = 0; i < selectedRecords.length; i++) {
                                        var selectedValue = selectedRecords[i];
                                        selectedEWOArray.push(selectedValue.EWONo);

                                        var yarnPOChildOrder = {
                                            YPOChildID: row.YPOChildID,
                                            ExportOrderId: selectedValue.ExportOrderId,
                                            EWONo: selectedValue.EWONo,
                                            IsSample: selectedValue.IsSample,
                                            BuyerID: selectedValue.BuyerID,
                                            BuyerTeamID: selectedValue.BuyerTeamID,
                                            BuyerName: selectedValue.BuyerName
                                        }

                                        row.YarnPOChildOrders.push(yarnPOChildOrder);
                                    }

                                    row.YarnChildPoEWOs = selectedEWOArray.join(",");
                                    $tblChildEl.bootstrapTable('updateByUniqueId', { id: row.YPOChildID, row: row });
                                }
                            });
                            finder.showModal();
                        },
                    }
                },
                {
                    field: "EWOOthers",
                    title: "EWO Others",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
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
                var selectedValue = { id: "", text: "" };
                switch (field) {
                    case "PoForId":
                        if (row.PoForId) {
                            selectedValue = masterData.PIForList.find(function (el) { return el.id == row.PoForId });
                            row.POFor = selectedValue.text;
                        }
                        break;
                    case "Segment1ValueId":
                        if (row.Segment1ValueId) {
                            selectedValue = masterData.YarnTypeList.find(function (el) { return el.id == row.Segment1ValueId });
                            row.Segment1ValueDesc = selectedValue.text;
                        }
                        break;
                    case "Segment3ValueId":
                        if (row.Segment3ValueId) {
                            selectedValue = masterData.YarnCompositionList.find(function (el) { return el.id == row.Segment3ValueId });
                            row.Segment3ValueDesc = selectedValue.text;
                        }
                        break;
                    case "YarnProgramId":
                        if (row.YarnProgramId) {
                            selectedValue = masterData.YarnProgramList.find(function (el) { return el.id == row.YarnProgramId });
                            row.YarnProgram = selectedValue.text;
                        }
                        break;
                    case "Segment5ValueId":
                        if (row.Segment5ValueId) {
                            selectedValue = masterData.YarnColorList.find(function (el) { return el.id == row.Segment5ValueId });
                            row.Segment5ValueDesc = selectedValue.text;
                        }
                        break;
                    default:
                        break;
                }

                row.PIValue = (row.PoQty * row.Rate).toFixed(2);
                row.YarnCategory = calculateYarnCategory(row);

                //if ((row.Segment1ValueId == 625) || (row.Segment1ValueId == 8238)) row.NoOfThread = "0";
                //else row.NoOfThread = "1";

                $tblChildEl.bootstrapTable('load', masterData.YarnPOChilds);
            }
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

    function changeSupplier(e) {
        e.preventDefault;
        showBootboxConfirm("Change Supplier", "Are you sure you want to change supplier?", function (yes) {
            if (yes) showSupplierSelection();
        })
    }
     
    function getNewData(supplierId) {
        var url = "/api/ypo/new/" + supplierId;
        axios.get(url)
            .then(function (response) {
                masterData = response.data;

                masterData.PoDate = formatDateToDefault(masterData.PoDate);
                masterData.DeliveryStartDate = formatDateToDefault(masterData.DeliveryStartDate);
                masterData.DeliveryEndDate = formatDateToDefault(masterData.DeliveryEndDate);
                masterData.InHouseDate = formatDateToDefault(masterData.InHouseDate);
                masterData.QuotationRefDate = formatDateToDefault(masterData.QuotationRefDate);
                setFormData($formEl, masterData);
                $formEl.find("#SupplierName").val(supplierName);

                initChildTable();

                if (masterData.PaymentTermsId === 2) showHideLCSection(true);
                else showHideLCSection(false);

                if (masterData.PortofLoadingID === 105) showHideSupplierRegionSection(false);
                else showHideSupplierRegionSection(true);

                if (masterData.TypeOfLcId === 2) $formEl.find("#formGroupCreditDays").fadeIn();
                else $formEl.find("#formGroupCreditDays").fadeOut();

                $("#PODateCurrent").text(masterData.DeliveryStartDate);
                $pageEl.find("#SFToPLDate").text(formatDateToDefault(masterData.SFToPLDate));
                $pageEl.find("#SFToPLDays").text(masterData.SFToPLDays);
                $pageEl.find("#PLToPDDate").text(formatDateToDefault(masterData.PLToPDDate));
                $pageEl.find("#PLToPDDays").text(masterData.PLToPDDays);
                $pageEl.find("#PDToCFDate").text(formatDateToDefault(masterData.PDToCFDate));
                $pageEl.find("#PDToCFDays").text(masterData.PDToCFDays);
                $pageEl.find("#InHouseDays").text(masterData.InHouseDays);

                HoldOn.close();
            })
            .catch(showResponseError)
    }

    function addPOFromPR() {
        HoldOn.open({
            theme: "sk-circle"
        });

        var url = `/api/ypo/new/${supplierId}/${prMasterId}`;
        axios.get(url)
            .then(function (response) {
                masterData = response.data;
                masterData.PoDate = formatDateToDefault(masterData.PoDate);
                masterData.DeliveryStartDate = formatDateToDefault(masterData.DeliveryStartDate);
                masterData.DeliveryEndDate = formatDateToDefault(masterData.DeliveryEndDate);
                masterData.QuotationRefDate = formatDateToDefault(masterData.QuotationRefDate);
                setFormData($formEl, masterData);
                $formEl.find("#SupplierName").val(supplierName);

                companyId = masterData.CompanyId;

                if (masterData.PaymentTermsId === 2) showHideLCSection(true);
                else showHideLCSection(false);

                if (masterData.PortofLoadingID === 105) showHideSupplierRegionSection(false);
                else showHideSupplierRegionSection(true);

                if (masterData.TypeOfLcId === 2) $formEl.find("#formGroupCreditDays").fadeIn();
                else $formEl.find("#formGroupCreditDays").fadeOut();

                $("#PODateCurrent").text(masterData.DeliveryStartDate);
                $pageEl.find("#SFToPLDate").text(formatDateToDefault(masterData.SFToPLDate));
                $pageEl.find("#SFToPLDays").text(masterData.SFToPLDays);
                $pageEl.find("#PLToPDDate").text(formatDateToDefault(masterData.PLToPDDate));
                $pageEl.find("#PLToPDDays").text(masterData.PLToPDDays);
                $pageEl.find("#PDToCFDate").text(formatDateToDefault(masterData.PDToCFDate));
                $pageEl.find("#PDToCFDays").text(masterData.PDToCFDays);
                $pageEl.find("#InHouseDays").text(masterData.InHouseDays);

                initChildTable();
                $tblChildEl.bootstrapTable('load', masterData.YarnPOChilds);

                HoldOn.close();
            })
            .catch(showResponseError)
    }

    function getDetails(e, value, row, index) {
        e.preventDefault();

        HoldOn.open({
            theme: "sk-circle"
        });

        switch (status) {
            case statusConstants.AWAITING_PROPOSE:
                $formEl.find("#btnPropose,#btnSave").show();
                $formEl.find("#btnApprove,#btnReject").hide();
                break;
            case statusConstants.PROPOSED:
                if (isApprovePage) {
                    $formEl.find("#btnPropose,#btnSave").hide();
                    $formEl.find("#btnApprove,#btnReject").show();
                }
                else {
                    $formEl.find("#btnPropose,#btnSave").show();
                    $formEl.find("#btnApprove,#btnReject").hide();
                }
                break;
            case statusConstants.PROPOSED:
                if (isApprovePage) {
                    $formEl.find("#btnPropose").show();
                    $formEl.find("#btnApprove,#btnReject,#btnSave").hide();
                }
                break;
            case statusConstants.AWAITING_PROPOSE:
                $formEl.find("#btnPropose,#btnSave").show();
                $formEl.find("#btnApprove,#btnReject").hide();
                break;
            default:
                break;
        }

        var url = `/api/ypo/${row.YPOMasterID}`;
        axios.get(url)
            .then(function (response) {
                $divTblEl.fadeOut();
                $divDetailsEl.fadeIn();
                $formEl.find("#SupplierTNA").fadeIn();

                masterData = response.data;
                masterData.PoDate = formatDateToDefault(masterData.PoDate);
                masterData.DeliveryStartDate = formatDateToDefault(masterData.DeliveryStartDate);
                masterData.DeliveryEndDate = formatDateToDefault(masterData.DeliveryEndDate);
                masterData.InHouseDate = formatDateToDefault(masterData.InHouseDate);
                masterData.QuotationRefDate = formatDateToDefault(masterData.QuotationRefDate);
                setFormData($formEl, masterData);

                if (masterData.PaymentTermsId === 2) showHideLCSection(true);
                else showHideLCSection(false);

                if (masterData.PortofLoadingID === 105) showHideSupplierRegionSection(false);
                else showHideSupplierRegionSection(true);

                if (masterData.TypeOfLcId === 2) $formEl.find("#formGroupCreditDays").fadeIn();
                else $formEl.find("#formGroupCreditDays").fadeOut();

                $("#PODateCurrent").text(masterData.DeliveryStartDate);
                $pageEl.find("#SFToPLDate").text(formatDateToDefault(masterData.SFToPLDate));
                $pageEl.find("#SFToPLDays").text(masterData.SFToPLDays);
                $pageEl.find("#PLToPDDate").text(formatDateToDefault(masterData.PLToPDDate));
                $pageEl.find("#PLToPDDays").text(masterData.PLToPDDays);
                $pageEl.find("#PDToCFDate").text(formatDateToDefault(masterData.PDToCFDate));
                $pageEl.find("#PDToCFDays").text(masterData.PDToCFDays);
                $pageEl.find("#InHouseDays").text(masterData.InHouseDays);

                $formEl.find("#IsRevision").prop("disabled", false);
                $formEl.find("#IsCancel").prop("disabled", false);
                $formEl.find("#RevisionReason").prop("readonly", false);
                $formEl.find("#RevisionArea").fadeOut();
                if (status == statusConstants.PROPOSED) {
                    if (masterData.RevisionNo > 0) {
                        $formEl.find("#RevisionArea").fadeIn();
                        $formEl.find("#IsRevision").fadeIn()
                        $formEl.find("#RevisionReason").fadeIn();
                        $formEl.find("#lblRevisionReason").fadeIn();
                        $formEl.find("#IsCancel").fadeOut()
                        $formEl.find("#CancelReason").fadeOut();
                        $formEl.find("#lblCancelReason").fadeOut();

                        $formEl.find("#IsRevision").prop("disabled", true);
                        $formEl.find("#IsCancel").prop("disabled", true);
                        $formEl.find("#RevisionReason").prop("readonly", true);
                    }
                } else if (status == statusConstants.APPROVED){
                    $formEl.find("#RevisionArea").fadeIn();
                }
                initChildTable();
                $tblChildEl.bootstrapTable('load', masterData.YarnPOChilds);

                HoldOn.close();
            })
            .catch(showResponseError)
    }

    function proposePO(e, value, row, index) {
        e.preventDefault();

        showBootboxConfirm("Propose Yarn PO", "Are you sure you want to propose this PO?", function (yes) {
            if (yes) {
                var url = "/api/ypo/propose-ypo/" + row.YPOMasterID;
                axios.post(url)
                    .then(function () {
                        toastr.success(constants.PROPOSE_SUCCESSFULLY);
                        backToList();
                    })
                    .catch(showResponseError);
            }
        });
    }

    function approvePO(e, value, row, index) {
        e.preventDefault();

        showBootboxConfirm("Approve Yarn PO", "Are you sure you want to approve this PO?", function (yes) {
            if (yes) {
                var url = "/api/ypo/approve-ypo/" + row.YPOMasterID;
                axios.post(url)
                    .then(function () {
                        toastr.success(constants.APPROVE_SUCCESSFULLY);
                        backToList();
                    })
                    .catch(showResponseError);
            }
        });
    }

    function rejectPO(e, value, row, index) {
        e.preventDefault();
        var masterID = row ? row.YPOMasterID : $formEl.find("#YPOMasterID").val();

        showBootboxPrompt("Reject Yarn PO", "Are you sure you want to Reject this PO?", function (reason) {
            if (reason) {
                axios.post(`/api/ypo/unapprove-ypo?id=${masterID}&reason=${reason}`)
                    .then(function () {
                        toastr.success(constants.REJECT_SUCCESSFULLY);
                        backToList();
                    })
                    .catch(showResponseError);
            }
        });
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#YPOMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function saveYPO(isPropose) {
        if (!validateSave()) {
            return toastr.error("Please correct all validation errors.");
        }

        var data = formElToJson($formEl);
        if (!data.CompanyId) data.CompanyId = companyId;
        data.Proposed = isPropose;
        data["YarnPOChilds"] = masterData.YarnPOChilds;
        data["YarnPOForOrders"] = masterData.YarnPOForOrders;
        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/api/ypo/save", data, config)
            .then(function (response) {
                showBootboxAlert("Yarn PO No: <b>" + response.data + "</b> saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function getMasterTblRowActions(row) {
        var rowActions = [];
        switch (status) {
            case statusConstants.PENDING:
                rowActions = ['<span class="btn-group">',
                    '<a class="btn btn-xs btn-primary add" href="javascript:void(0)" title="Add PO">',
                    '<i class="fa fa-edit" aria-hidden="true"></i>',
                    '</a>',
                    '</span>'];
                break;
            case statusConstants.AWAITING_PROPOSE:
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
            case statusConstants.PROPOSED:
                if (isApprovePage) {
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
            case statusConstants.APPROVED:
                if (isApprovePage) {
                    rowActions = ['<span class="btn-group">',
                        '<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportId=990&PONo=' + row.PoNo + '" target="_blank" title="PO Report">',
                        '<i class="fa fa-file-pdf-o" aria-hidden="true"></i>',
                        '</a>',
                        '</span>'];
                }
                else {
                    rowActions = ['<span class="btn-group">',
                        '<a class="btn btn-xs btn-primary edit" href="javascript:void(0)" title="Revise PO">',
                        '<i class="fa fa-edit" aria-hidden="true"></i>',
                        '</a>',
                        '<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportId=990&PONo=' + row.PoNo + '" target="_blank" title="PO Report">',
                        '<i class="fa fa-file-pdf-o" aria-hidden="true"></i>',
                        '</a>',
                        '</span>'];
                }
                break;
            case statusConstants.ALL:
                rowActions = ['<span class="btn-group">',
                    '<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportId=990&PONo=' + row.PoNo + '" target="_blank" title="PO Report">',
                    '<i class="fa fa-file-pdf-o" aria-hidden="true"></i>',
                    '</a>',
                    '</span>'];
                break;
            case statusConstants.UN_APPROVE:
                if (isApprovePage) {
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
        $tblChildEl.bootstrapTable('destroy');
        $formEl.find("#tblYarnPOforExportOrders").bootstrapTable('load', masterData.YarnPOForOrders);
        $formEl.find("#formGroupPoForDetails").remove();

        $divTblEl.fadeIn();
        getMasterTableData();
    }

    function getYarnCountByYarnType(yarnTypeId, rowData) {
        var url = "/api/selectoption/yarn-count-by-yarn-type/" + yarnTypeId;
        axios.get(url)
            .then(function (response) {
                showBootboxSelect2Dialog("Select Yarn Count", "YarnCountId", "Select Yarn Count", response.data,
                    function (data) {
                        if (!data) return toastr.warning("You didn't selected any Yarn Count.");

                        rowData.Segment2ValueId = data.id;
                        rowData.Segment2ValueDesc = data.text;
                        rowData.YarnCategory = calculateYarnCategory(rowData);

                        $tblChildEl.bootstrapTable('updateByUniqueId', { id: rowData.YPOChildID, row: rowData });
                    }
                    , rowData.Segment2ValueId
                )
            })
            .catch(function (error) {
                if (error.response && error.response.data && error.response.data.Message) toastr.error(error.response.data.Message);
                else toastr.error(error);
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
            CurrencyId: {
                presence: true
            },
            QuotationRefNo: {
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
        if (!isValidForm($formEl, validationConstraints)) return isValid;
        else hideValidationErrors($formEl);

        isValid = true;
        for (var i = 0; i < masterData.YarnPOChilds.length; i++) {
            var child = masterData.YarnPOChilds[i];
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

            if (child.PoQty <= 0) {
                toastr.error("POQty must be greater than 0");
                isValid = false;
            }

            if (child.Rate <= 0) {
                toastr.error("Rate must be greater than 0");
                isValid = false;
            }

            if (!child.POFor) {
                toastr.error(`You must select 'Purchase For'.`);
                isValid = false;
            }

            if (child.POFor === poForNames.SPECIFIC_ORDER && child.POFor != poForNames.OTHERS && !child.YarnChildPoEWOs) {
                toastr.error(`For '${poForNames.SPECIFIC_ORDER}', you must select EWO.`);
                isValid = false;
            }

            if (child.POFor != poForNames.RE_ORDER_LEVEL && child.POFor != poForNames.OTHERS && (!child.YarnChildPoBuyerIds || child.YarnChildPoBuyerIds == "0")) {
                toastr.error(`For '${child.POFor}', you must select Buyer.`);
                isValid = false;
            }

            if (child.POFor === poForNames.OTHERS && !child.EWOOthers) {
                toastr.error(`For '${child.POFor}', you must select Other EWO.`);
                isValid = false;
            }
        }

        return isValid;
    }

    function showHideLCSection(show) {
        if (show) {
            $formEl.find("#formGroupTypeOfLcId").show();
            $formEl.find("#formGroupCalculationofTenure").show();
        }
        else {
            $formEl.find("#formGroupTypeOfLcId").hide();
            $formEl.find("#formGroupCalculationofTenure").hide();
        }
    }

    function showHideSupplierRegionSection(show) {  // Supplier was local or foreign
        if (show) {
            $formEl.find("#formGroupPortofLoading").show();
            $formEl.find("#formGroupPortofDischarge").show();
            $formEl.find("#formGroupQuantityApprovalProcedure").show();
        }
        else {
            $formEl.find("#formGroupPortofLoading").hide();
            $formEl.find("#formGroupPortofDischarge").hide();
            $formEl.find("#formGroupQuantityApprovalProcedure").hide();
        }
    }

    function addNewItem(e) {
        e.preventDefault();

        var newYarnPOChildData = {
            YPOChildID: getMaxIdForArray(masterData.YarnPOChilds, "YPOChildID"),
            YPOMasterID: 0,
            YarnSubProgramIds: '',
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
            ItemMasterID: 0,
            UnitID: 28,
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
            EWOOthers: "",
            BuyerNames: "",
            EntityState: 4
        };

        masterData.YarnPOChilds.push(newYarnPOChildData);
        $tblChildEl.bootstrapTable('load', masterData.YarnPOChilds);
    }

    function btnNewClick(e) {
        e.preventDefault();
        toggleActiveToolbarBtn(this, $toolbarEl);
        showSupplierSelection();
    }

    function btnAwaitingProposeListClick(e) {
        e.preventDefault();
        resetTableParams();
        status = statusConstants.AWAITING_PROPOSE;
        showAllPO = false;
        initMasterTable();
        getMasterTableData();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $formEl.find("#RevisionArea").fadeOut();
    }

    function btnPendingApprovalListClick(e) {
        e.preventDefault();
        resetTableParams();
        status = statusConstants.PROPOSED;
        showAllPO = false;
        initMasterTable();
        getMasterTableData();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $formEl.find("#RevisionArea").fadeOut();
    }

    function btnApprovedListClick(e) {
        e.preventDefault();
        resetTableParams();
        status = statusConstants.APPROVED;
        showAllPO = false;
        initMasterTable();
        getMasterTableData();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $formEl.find("#RevisionArea").fadeIn();
        $formEl.find("#btnSave").fadeOut();
        $formEl.find("#btnPropose").fadeOut();
    }

    function btnRejectListClick(e) {
        e.preventDefault();
        resetTableParams();
        status = statusConstants.UN_APPROVE;
        showAllPO = false;
        initMasterTable();
        getMasterTableData();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $formEl.find("#RevisionArea").fadeOut();
    }

    function btnAllListClick(e) {
        e.preventDefault();
        resetTableParams();
        status = statusConstants.ALL;
        showAllPO = true;
        initMasterTable();
        getMasterTableData();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $formEl.find("#RevisionArea").fadeOut();
    }

    function showSupplierSelection() {
        axios.get("/api/selectoption/yarn-suppliers")
            .then(function (response) {
                showBootboxSelect2Dialog("Select supplier", "Supplier", "Choose supplier", response.data, function (data) {
                    if (data) {
                        HoldOn.open({
                            theme: "sk-circle"
                        });

                        resetForm();
                        $divTblEl.fadeOut();
                        $divDetailsEl.fadeIn();
                        $formEl.find("#btnSave").fadeIn();
                        $formEl.find("#btnPropose").fadeIn();
                        $formEl.find("#btnApprove").fadeOut();
                        $formEl.find("#btnReject").fadeOut();
                        $formEl.find("#SupplierTNA").fadeIn();
                        $formEl.find("#RevisionArea").fadeOut();
                        $("#SupplierId").val(data.id);
                        supplierId = data.id;
                        supplierName = data.text;
                        $("#CompanyId").prop("disabled", false);
                        getNewData(supplierId);
                    }
                    else toastr.warning("You must select a supplier.");
                })
            })
            .catch(showResponseError);
    }

    function showSupplierSelectionForPR(e, value, row, index) {
        e.preventDefault();

        $("#PRMasterID").val(row.PRMasterID);
        prMasterId = row.PRMasterID;

        axios.get("/api/selectoption/yarn-suppliers")
            .then(function (response) {
                showBootboxSelect2Dialog("Select supplier", "SupplierId", "Choose supplier", response.data, function (data) {
                    if (data) {
                        resetForm();
                        $divTblEl.fadeOut();
                        $divDetailsEl.fadeIn();
                        $formEl.find("#btnSave").fadeIn();
                        $formEl.find("#btnPropose").fadeIn();
                        $formEl.find("#btnApprove").fadeOut();
                        $formEl.find("#btnReject").fadeOut();
                        $formEl.find("#SupplierTNA").fadeIn();
                        $formEl.find("#RevisionArea").fadeOut();
                        $("#CompanyId").prop("disabled", true);
                        $("#SupplierId").val(data.id);
                        supplierId = data.id;
                        supplierName = data.text;
                        addPOFromPR();
                    }
                    else toastr.warning("You must select a supplier.");
                })
            })
            .catch(showResponseError);
    }
})();
(function () {
    //'use strict'

    // #region variables
    var bblcProposal;
    var menuId, pageName;
    var toolbarId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblMergeExistingEl, $formEl;
    var status = statusConstants.PENDING;

    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }

    var validationConstraints = {
        CompanyID: {
            presence: true
        },
        SupplierID: {
            presence: true
        },
        TExportLCID: {
            presence: true
        }
    };

    var selectedPIReceiveList = [];

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
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $tblMergeExistingEl = $('#tblMergeExisting' + pageId);

        $tblMasterSaveEl = $(pageConstants.MASTER_TBL_ID_PREFIX + 'Save' + pageId);

        initializeValidation($formEl, validationConstraints);

        initMasterTable();
        getMasterTableData();
        initChildTable();

        initMergeExistingTable();

        $formEl.find("#btnBackNew").on("click", function (e) {
            e.preventDefault();
            backToList();
        });

        $formEl.find("#btnBackEdit").on("click", function (e) {
            e.preventDefault();
            backToList();
        });

        $toolbarEl.find("#btnPendingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            $("#div-create-merge-btns").show();
            status = statusConstants.PENDING;
            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            $("#div-create-merge-btns").hide();
            status = statusConstants.COMPLETED;
            initMasterTable();
            getMasterTableData();
        });

        $divTblEl.find("#btnCreate").click(getNew);

        $divTblEl.find("#btnMergeExisting").click(btnMergeExistingClick);

        $formEl.find("#btCancel").click(backToList);

        $formEl.find("#btnSave").click(handleSaveClick);
    });

    function getNew(e) {
        e.preventDefault();
        selectedPIReceiveList = $tblMasterEl.bootstrapTable('getSelections');

        if (!selectedPIReceiveList || selectedPIReceiveList.length === 0) return toastr.error("You must select one PI");

        var piReceiveMasterIds = selectedPIReceiveList.map(function (el) { return el.YPIReceiveMasterID; }).join();

        axios.get(`/api/cdalcproposal/new?piReceiveMasterIds=${piReceiveMasterIds}`)
            .then(function (response) {
                bblcProposal = response.data;
                bblcProposal.ProposalDate = formatDateToDefault(bblcProposal.ProposalDate);
                goToDetails(bblcProposal);
            })
            .catch(showResponseError);
    }

    function getDetails(e, value, row, index) {
        e.preventDefault();
        $formEl.find("#btnSave").hide();
        $formEl.find("#TExportLCID").prop('disabled', 'disabled');
        $formEl.find("#Remarks").prop('disabled', 'disabled');
        axios.get(`/api/cdalcproposal/${row.Id}`)
            .then(function (response) {
                bblcProposal = response.data;
                bblcProposal.ProposalDate = formatDateToDefault(bblcProposal.ProposalDate);
                goToDetails(bblcProposal);
            })
            .catch(showResponseError);
    }

    function handleSaveClick(e) {
        e.preventDefault();

        var errors = validateForm($formEl, validationConstraints);
        if (errors) {
            showValidationErrorToast(errors)
            return;
        }
        else hideValidationErrors($formEl);

        var data = formDataToJson($formEl.serializeArray());

        if (bblcProposal.EntityState === entityState.MODIFIED) {
            data.TExportLCID = bblcProposal.TExportLCID;
            data.TExportLCNo = bblcProposal.TExportLCNo;
        }
        else data.TExportLCNo = $formEl.find("#TExportLCID").select2("data")[0].text;

        data["Childs"] = bblcProposal.Childs;

        axios.post("/api/cdalcproposal/save", data)
            .then(function () {
                toastr.success("BBLC Proposal has been saved.");
                backToList();
            })
            .catch(showResponseError);
    }

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
            checkboxHeader: false,
            clickToSelect: true,
            idField: "Id",
            columns: [
                {
                    checkbox: true,
                    visible: status == statusConstants.PENDING
                },
                {
                    field: "Actions",
                    title: "Actions",
                    align: "center",
                    width: 50,
                    visible: status == statusConstants.COMPLETED,
                    formatter: function (value, row, index, field) {
                        template =
                            `<span class="btn-group">
                                <a class="btn btn-xs btn-primary m-w-30 edit" href="javascript:void(0)" title="Edit Proposal">
                                    <i class="fa fa-pencil-square-o" aria-hidden="true"></i>
                                </a>
                            </span>`;
                        return template;
                    },
                    events: {
                        'click .edit': getDetails
                    }
                },
                {
                    field: "ProposalNo",
                    title: "Proposal No",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status == statusConstants.COMPLETED
                },
                {
                    field: "ProposalDate",
                    title: "Proposal Date",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                    visible: status == statusConstants.COMPLETED
                },
                {
                    field: "TExportLCNo",
                    title: "Export LC No",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status == statusConstants.COMPLETED
                },
                {
                    field: "YPINo",
                    title: "PI No",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "PIDate",
                    title: "PI Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "CompanyName",
                    title: "Company",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "SupplierName",
                    title: "Supplier",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "PONo",
                    title: "PO No",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status == statusConstants.PENDING
                },
                {
                    field: "TotalQty",
                    title: "Total Qty",
                    filterControl: "input",
                    align: 'right',
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    footerFormatter: calculateTotalYarnQtyAll,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "TotalValue",
                    title: "Total Value",
                    filterControl: "input",
                    align: 'right',
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    footerFormatter: calculateTotalYarnValueAll,
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
            onCheck: function (row, $element) {

                var rowsList = $tblMasterEl.bootstrapTable('getData');
                var filterData = $.grep(rowsList, function (h, i) {
                    return h.CheckAll == true && h.Id != row.Id;
                });
                if (filterData.length > 0) {
                    row.CheckAll = true;


                    var filterSupplierData = $.grep(filterData, function (h, i) {
                        return h.CheckAll == true && h.SupplierName == row.SupplierName;
                    });
                    if (filterSupplierData.length > 0) {
                        row.CheckAll = true;
                        //$element[0].checked = true;
                    }
                    else {
                        row.CheckAll = false;
                        toastr.warning("Must have same supplier");
                    }
                }

                $tblMasterEl.bootstrapTable("updateByUniqueId", row.Id, row);
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

    function initChildTable() {
        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            showFooter: true,
            columns: [
                {
                    field: "PiFilePath",
                    title: "View PI",
                    formatter: function (value, row, index, field) {
                        return `<a href="${row.PiFilePath}" target="_blank"><i class="fa fa-eye"></i> View PI</a>`;
                    }
                },
                {
                    field: "YPINo",
                    title: "PI No",
                    footerFormatter: function () {
                        return "<label>Total</label>";
                    }
                },
                {
                    field: "PIDate",
                    title: "PI Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                },
                {
                    field: "Unit",
                    title: "Unit",
                },
                {
                    field: "TotalQty",
                    title: "PI Qty",
                    align: 'right',
                    footerFormatter: calculateTotalQty
                },
                {
                    field: "TotalValue",
                    title: "PI Value",
                    align: 'right',
                    footerFormatter: calculateTotalValue
                }
            ]
        });
    }

    function getMasterTableData() {
        var queryParams = $.param(tableParams);
        $tblMasterEl.bootstrapTable('showLoading');
        var url = "/api/cdalcproposal/list?status=" + status + "&" + queryParams;
        axios.get(url)
            .then(function (response) {
                listData = response.data;
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function LoadMergeData(id) {
        var rowsList = $tblMasterEl.bootstrapTable('getData');
        var piReceiveMasterIds = rowsList.map(function (el) { return el.YPIReceiveMasterID; }).join();
        
        axios.get(`/api/cdalcproposal/load-merge-data/${id}/${piReceiveMasterIds}`)
            .then(function (response) {
                bblcProposal = response.data;
                
                bblcProposal.ProposalDate = formatDateToDefault(bblcProposal.ProposalDate);
                goToDetails(bblcProposal);
            })
            .catch(showResponseError);
    }

    function initMergeExistingTable() {
        $tblMergeExistingEl.bootstrapTable('destroy');
        $tblMergeExistingEl.bootstrapTable({
            showRefresh: true,
            filterControl: true,
            searchOnEnterKey: true,
            checkboxHeader: false,
            clickToSelect: true,
            idField: "Id",
            columns: [
                {
                    field: "ProposalNo",
                    title: "Proposal No",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ProposalDate",
                    title: "Proposal Date",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "TExportLCNo",
                    title: "Export LC No",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER

                },
                {
                    field: "CompanyName",
                    title: "Company",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "SupplierName",
                    title: "Supplier",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "TotalQty",
                    title: "Total Qty",
                    filterControl: "input",
                    align: 'right',
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "TotalValue",
                    title: "Total Value",
                    filterControl: "input",
                    align: 'right',
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                }
            ],
            onDblClickRow: function (row, $element, field) {
                $("#modal-merge-existing").modal('hide');
                LoadMergeData(row.Id);
            }
        });
    }

    function btnMergeExistingClick(e) {
        e.preventDefault();

        selectedPIReceiveList = $tblMasterEl.bootstrapTable('getSelections');

        if (!selectedPIReceiveList || selectedPIReceiveList.length === 0) return toastr.error("You must select one PI");

        var selectedRecords = selectedPIReceiveList.map(function (item) { return { CompanyID: item.CompanyID, SupplierID: item.SupplierID } });

        var isEqual = true;
        for (var i = 1; i < selectedRecords.length; i++) {
            isEqual = _.isEqual(selectedRecords[i - 1], selectedRecords[i]);
            if (!isEqual) break;
        }

        if (!isEqual) {
            toastr.error("Company and Supplier must be same.");
            return;
        }

        axios.get(`/api/cdalcproposal/list-for-merge/${selectedPIReceiveList[0].CompanyID}/${selectedPIReceiveList[0].SupplierID}`)
            .then(function (response) {
                
                $tblMergeExistingEl.bootstrapTable("load", response.data);
                $("#modal-merge-existing").modal("show");
            })
            .catch(showResponseError);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function goToDetails(data) {
        $divDetailsEl.fadeIn();
        $divTblEl.fadeOut();
        resetForm();
        setFormData($formEl, data);
        $tblChildEl.bootstrapTable("load", data.Childs);
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
        $formEl.find("#Id").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function calculateTotalQty(data) {
        var totalQty = 0;

        $.each(data, function (i, row) {
            totalQty += isNaN(parseFloat(row.TotalQty)) ? 0 : parseFloat(row.TotalQty);
        });

        return totalQty.toFixed(2);
    }

    function calculateTotalValue(data) {
        var totalValue = 0;

        $.each(data, function (i, row) {
            totalValue += isNaN(parseFloat(row.TotalValue)) ? 0 : parseFloat(row.TotalValue);
        });

        return totalValue.toFixed(2);
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
})();
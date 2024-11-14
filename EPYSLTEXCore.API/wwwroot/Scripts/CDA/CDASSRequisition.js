(function () {
    var menuId, pageName;
    var toolbarId;
    var isAcknowledgePage = false;
    var isEditable = false;
    var isApprovePage = false;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl;
    var filterBy = {};
    var status = statusConstants.PENDING;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }

    var CDASSRequisition;
    var SubGroupId;

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
        $formEl.find("#divRejectReason").hide();
        isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val());
        isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());

        if (isAcknowledgePage) {
            $toolbarEl.find("#btnNew").hide();
            $toolbarEl.find("#btnYPOPending").hide();
            $toolbarEl.find("#btnApproveList").hide();
            $toolbarEl.find("#btnRejectList").hide();
            $toolbarEl.find("#btnPendingAkgList").show();
            $toolbarEl.find("#btnAcknowledgementList").show();
            status = statusConstants.APPROVED;
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingAkgList"), $toolbarEl);
        }
        else if (isApprovePage) {
            $toolbarEl.find("#btnNew").hide();
            $toolbarEl.find("#btnYPOPending").show();
            $toolbarEl.find("#btnApproveList").show();
            $toolbarEl.find("#btnRejectList").show();
            $toolbarEl.find("#btnPendingAkgList").hide();
            $toolbarEl.find("#btnAcknowledgementList").hide();
            status = statusConstants.PENDING;
            toggleActiveToolbarBtn($toolbarEl.find("#btnYPOPending"), $toolbarEl);
        }
        else {
            toggleActiveToolbarBtn($toolbarEl.find("#btnYPOPending"), $toolbarEl);
        }

        initMasterTable();
        getMasterTableData();

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
        $toolbarEl.find("#btnAcknowledgementList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.ACKNOWLEDGE;

            initMasterTable();
            getMasterTableData();
        });
        $toolbarEl.find("#btnPendingAkgList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PARTIALLY_COMPLETED;

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
        $toolbarEl.find("#btnYPOPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;

            initMasterTable();
            getMasterTableData();
        });
        $toolbarEl.find("#btnNew").on("click", showSubGroupSelection);

        $formEl.find("#btnNewItem").on("click", function (e) {
            e.preventDefault();
            var newChildItem = {
                SSReqChildID: getMaxIdForArray(CDASSRequisition.Childs, "SSReqChildID"),
                CDAReqMasterId: 0,
                ReqQty: 0,
                Remarks: "",
                SubGroupId: 0,
                ItemMasterID: 0,
                UnitID: 28,
                DisplayUnitDesc: "Kg",
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
                EntityState: 4
            };

            CDASSRequisition.Childs.push(newChildItem);
            $tblChildEl.bootstrapTable('load', CDASSRequisition.Childs);
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });
        $formEl.find("#btnApproveYPO").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#SSReqMasterID").val();
            axios.post(`/api/CDA-SS-requisition/approve/${id}`)
                .then(function () {
                    toastr.success("Requisition approved successfully.");
                    backToList();
                })
                .catch(showResponseError);
        });
        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#SSReqMasterID").val();
            axios.post(`/api/CDA-SS-requisition/acknowledge/${id}`)
                .then(function () {
                    toastr.success("Requisition acknowledged successfully.");
                    backToList();
                })
                .catch(showResponseError);
        });
        $formEl.find("#btnRejectYPO").click(function (e) {
            e.preventDefault();

            bootbox.prompt("Are you sure you want to reject this?", function (result) {
                if (!result) {
                    return toastr.error("Reject reason is required.");
                }
                var id = $formEl.find("#SSReqMasterID").val();
                var reason = result;
                axios.post(`/api/CDA-SS-requisition/reject/${id}/${reason}`)
                    .then(function () {
                        toastr.success("Requisition rejected successfully.");
                        backToList();
                    })
                    .catch(showResponseError);
            });
        });

        $formEl.find("#btnSaveAndSend").click(function (e) {
            e.preventDefault();
            save(this, true);
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
                        return [
                            '<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Requisition">',
                            '<i class="fa fa-edit" aria-hidden="true"></i>',
                            '</a>'
                        ].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.SSReqMasterID);
                        }
                    }
                },
                {
                    field: "SSReqNo",
                    title: "Requisition No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "SSReqDate",
                    title: "Requisition Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                }
                ,
                {
                    field: "TotalQty",
                    title: "Total Qty"
                },

                {
                    field: "RequisitionByUser",
                    title: "Requisition By",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },

                {
                    field: "Remarks",
                    title: "Remarks",
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
        var url = "/api/CDA-SS-requisition/list?gridType=bootstrap-table&status=" + status + "&" + queryParams;
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
        $tblChildEl.bootstrapTable('destroy');
        $tblChildEl.bootstrapTable({
            uniqueId: 'SSReqChildID',
            editable: isEditable,
            columns: [
                {
                    width: 20,
                    visible: isEditable,
                    formatter: function (value, row, index, field) {
                        return ['<span class="btn-group">',
                            '<a class="btn btn-xs btn-danger remove" onclick="javascript:void(0)" title="Remove">',
                            '<i class="fa fa-remove" aria-hidden="true"></i>',
                            '</a>',
                            '</span>'].join(' ');
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            e.preventDefault();
                            $tblChildEl.bootstrapTable('remove', { field: 'SSReqChildID', values: [row.SSReqChildID] });
                        },
                    }
                },
                {
                    field: "Segment1ValueId",
                    title: "Item Name",
                    visible: isEditable,
                    editable: {
                        type: 'select2',
                        title: 'Select Item Name',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: CDASSRequisition.ItemList,
                        select2: { width: 200, placeholder: 'Item Name', allowClear: true }
                    }
                },
                {
                    field: "Segment2ValueId",
                    title: "Agent Name",
                    visible: isEditable,
                    editable: {
                        type: 'select2',
                        title: 'Select Agent Name',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: CDASSRequisition.AgentList,
                        select2: { width: 200, placeholder: 'Agent Name', allowClear: true }
                    }
                },
                {
                    field: "Segment1ValueDesc",
                    title: "Item Name",
                    visible: !isEditable,
                },
                {
                    field: "Segment2ValueDesc",
                    title: "Agent Name",
                    visible: !isEditable,
                },
                {
                    field: "ReqQty",
                    title: "Req Qty(Kg)",
                    align: 'right',
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm m-w-50',
                        showbuttons: false
                    }
                },

                {
                    field: "DisplayUnitDesc",
                    title: "Unit"
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
            ],
            onEditableSave: function (field, row, oldValue, $el) {
                var selectedValue = { id: "", text: "" };
                switch (field) {
                    case "Segment1ValueId":
                        if (row.Segment1ValueId) {
                            selectedValue = CDASSRequisition.ItemList.find(function (el) { return el.id == row.Segment1ValueId });
                            row.Segment1ValueDesc = selectedValue.text;
                        }
                        break;
                    case "Segment2ValueId":
                        if (row.Segment2ValueId) {
                            selectedValue = CDASSRequisition.AgentList.find(function (el) { return el.id == row.Segment2ValueId });
                            row.Segment2ValueDesc = selectedValue.text;
                        }
                        break;

                    default:
                        break;
                }

                row.POValue = (row.BookingQty * row.Rate).toFixed(2);
                $tblChildEl.bootstrapTable('load', CDASSRequisition.Childs);
            }
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
        $formEl.find("#SSReqMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function showSubGroupSelection() {
        axios.get("/api/selectoption/cda-dyes-chemical")
            .then(function (response) {
                showBootboxSelect2Dialog("Select SubGroup", "SubGroupID", "Choose SubGroup", response.data, function (data) {
                    if (data) {
                        HoldOn.open({
                            theme: "sk-circle"
                        });

                        resetForm();
                        $divTblEl.fadeOut();
                        $divDetailsEl.fadeIn();
                        $formEl.find("#btnSaveCDAPO").fadeIn();
                        $formEl.find("#btnApproveYPO").fadeOut();
                        $formEl.find("#btnRejectYPO").fadeOut();
                        $formEl.find("#SupplierTNA").fadeIn();
                        $formEl.find("#RevisionArea").fadeOut();
                        SubGroupId = data.id;
                        getNew(SubGroupId);
                    }
                    else toastr.warning("You must select a supplier.");
                })
            })
            .catch(showResponseError);
    }

    function getNew(subgroupId) {
        var url = "/api/CDA-SS-requisition/new/" + subgroupId;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                CDASSRequisition = response.data;
                CDASSRequisition.SSReqDate = formatDateToDefault(CDASSRequisition.SSReqDate);
                isEditable = true;
                setFormData($formEl, CDASSRequisition);
                initChildTable();
                $formEl.find("#SubGroupId").val(SubGroupId);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/CDA-SS-requisition/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                CDASSRequisition = response.data;
                CDASSRequisition.SSReqDate = formatDateToDefault(CDASSRequisition.SSReqDate);

                if (!CDASSRequisition.IsApprove && !CDASSRequisition.Reject) {
                    isEditable = false;
                    $formEl.find("#btnSave").hide();
                    $formEl.find("#btnNewItem").hide();
                    $formEl.find("#CompanyID").prop("disabled", true);
                    $formEl.find("#CDARequisitionBy").prop("disabled", true);
                    $formEl.find("#Remarks").prop("readonly", true);
                    if (isApprovePage && !CDASSRequisition.IsApprove && !CDASSRequisition.Reject) {
                        $formEl.find("#btnApproveYPO").show();
                        $formEl.find("#btnRejectYPO").show();
                    }
                    else {
                        $formEl.find("#btnApproveYPO").hide();
                        $formEl.find("#btnRejectYPO").hide();
                        if (CDASSRequisition.Reject) {
                            $formEl.find("#divRejectReason").show();
                        }
                        else {
                            $formEl.find("#divRejectReason").hide();
                        }
                    }

                    if (isAcknowledgePage && CDASSRequisition.IsApprove && !CDASSRequisition.IsAcknowledge) {
                        $formEl.find("#btnAcknowledge").show();
                    }
                    else {
                        $formEl.find("#btnAcknowledge").hide();
                    }
                }
                else if (CDASSRequisition.IsApprove || CDASSRequisition.Reject) {
                    $formEl.find("#btnSave").hide();
                    $formEl.find("#btnNewItem").hide();
                    $formEl.find("#CompanyID").prop("disabled", true);
                    $formEl.find("#CDARequisitionBy").prop("disabled", true);
                    $formEl.find("#Remarks").prop("readonly", true);
                    if (isApprovePage && !CDASSRequisition.IsApprove && !CDASSRequisition.Reject) {
                        $formEl.find("#btnApproveYPO").show();
                        $formEl.find("#btnRejectYPO").show();
                    }
                    else {
                        $formEl.find("#btnApproveYPO").hide();
                        $formEl.find("#btnRejectYPO").hide();
                        if (CDASSRequisition.Reject) {
                            $formEl.find("#divRejectReason").show();
                        }
                        else {
                            $formEl.find("#divRejectReason").hide();
                        }
                    }

                    if (isAcknowledgePage && CDASSRequisition.IsApprove && !CDASSRequisition.IsAcknowledge) {
                        $formEl.find("#btnAcknowledge").show();
                    }
                    else {
                        $formEl.find("#btnAcknowledge").hide();
                    }
                }
                else {
                    isEditable = true;
                    $formEl.find("#btnSave").show();
                    $formEl.find("#CompanyID").prop("disabled", false);
                    $formEl.find("#CDARequisitionBy").prop("disabled", false);
                    $formEl.find("#Remarks").prop("readonly", false);
                }
                setFormData($formEl, CDASSRequisition);
                initChildTable();
                $formEl.find("#SubGroupId").val(CDASSRequisition.SubGroupID);
                $tblChildEl.bootstrapTable("load", CDASSRequisition.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = CDASSRequisition.Childs;
        axios.post("/api/CDA-SS-requisition/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
})();
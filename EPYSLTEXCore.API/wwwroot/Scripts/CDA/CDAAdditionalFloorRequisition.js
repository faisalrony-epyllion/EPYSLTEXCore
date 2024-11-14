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

    var CDAFloorRequisition;
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
        isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());

        if (isApprovePage) {
            $toolbarEl.find("#btnNew").hide();
            $toolbarEl.find("#btnYPOPending").show();
            $toolbarEl.find("#btnApproveList").show();
            $toolbarEl.find("#btnAvailable").hide();
            status = statusConstants.PARTIALLY_COMPLETED;
            toggleActiveToolbarBtn($toolbarEl.find("#btnYPOPending"), $toolbarEl);
        }
        else {
            toggleActiveToolbarBtn($toolbarEl.find("#btnAvailable"), $toolbarEl);
        }

        initMasterTable();
        getMasterTableData();

        $toolbarEl.find("#btnAvailable").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
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


        $toolbarEl.find("#btnYPOPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PARTIALLY_COMPLETED;

            initMasterTable();
            getMasterTableData();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });
        $formEl.find("#btnApproveYPO").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#Id").val();
            axios.post(`/api/cda-additional-floor-requisition/approve/${id}`)
                .then(function () {
                    toastr.success("Requisition approved successfully.");
                    backToList();
                })
                .catch(showResponseError);
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
                    field: "new",
                    align: "center",
                    visible: status == statusConstants.PENDING,
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
                            getNew(row.Id);
                        }
                    }
                },
                {
                    field: "",
                    align: "center",
                    visible: status !== statusConstants.PENDING,
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
                            getDetails(row.Id);
                        }
                    }
                },
                {
                    field: "FloorReqNo",
                    title: "Requisition No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status !== statusConstants.PENDING_BATCH
                },
                {
                    field: "FloorReqDate",
                    title: "Requisition Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                    visible: status !== statusConstants.PENDING_BATCH
                }
                ,
                {
                    field: "TotalQty",
                    title: "Total Qty",
                    visible: status !== statusConstants.PENDING_BATCH
                },

                {
                    field: "RequisitionByUser",
                    title: "Requisition By",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status !== statusConstants.PENDING_BATCH
                },

                {
                    field: "Remarks",
                    title: "Remarks",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status !== statusConstants.PENDING_BATCH
                },
                {
                    field: "BatchNo",
                    title: "Batch No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status == statusConstants.PENDING_BATCH
                },
                {
                    field: "BatchDate",
                    title: "Batch Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                    visible: status == statusConstants.PENDING_BATCH
                },
                {
                    field: "ConceptNo",
                    title: "Concept No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status == statusConstants.PENDING_BATCH
                },
                {
                    field: "ConceptDate",
                    title: "Concept Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                    visible: status == statusConstants.PENDING_BATCH
                },
                {
                    field: "ColorName",
                    title: "Color",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status == statusConstants.PENDING_BATCH
                },
               
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
        var url = "/api/cda-additional-floor-requisition/list?gridType=bootstrap-table&status=" + status + "&" + queryParams;
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
            uniqueId: 'Id',
            editable: isEditable,
            columns: [
                
                {
                    field: "Segment1ValueDesc",
                    title: "Item Name"
                },
                {
                    field: "Segment2ValueDesc",
                    title: "Agent Name"
                },
                {
                    field: "ReqQty",
                    title: "Req Qty(Kg)",
                    align: 'right'
                },


                {
                    field: "DisplayUnitDesc",
                    title: "Unit"
                },
                {
                    field: "AdditionalReqQty",
                    title: "Additional Qty",
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
                            selectedValue = CDAFloorRequisition.ItemList.find(function (el) { return el.id == row.Segment1ValueId });
                            row.Segment1ValueDesc = selectedValue.text;
                        }
                        break;
                    case "Segment2ValueId":
                        if (row.Segment2ValueId) {
                            selectedValue = CDAFloorRequisition.AgentList.find(function (el) { return el.id == row.Segment2ValueId });
                            row.Segment2ValueDesc = selectedValue.text;
                        }
                        break;

                    default:
                        break;
                }

                row.POValue = (row.BookingQty * row.Rate).toFixed(2);
                $tblChildEl.bootstrapTable('load', CDAFloorRequisition.Childs);
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

    function getNew(subgroupId) {
        var url = "/api/cda-additional-floor-requisition/new/" + subgroupId;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                CDAFloorRequisition = response.data;
                CDAFloorRequisition.FloorReqDate = formatDateToDefault(CDAFloorRequisition.FloorReqDate);
                isEditable = true;
                $formEl.find("#btnSave").show();
                $formEl.find("#btnNewItem").hide();
                $formEl.find("#CompanyId").prop("disabled", true);
                $formEl.find("#FloorReqBy").prop("disabled", true);
                setFormData($formEl, CDAFloorRequisition);
                initChildTable();
                $formEl.find("#SubGroupId").val(CDAFloorRequisition.SubGroupID);
                $tblChildEl.bootstrapTable("load", CDAFloorRequisition.Childs);
                $tblChildEl.bootstrapTable('hideLoading');

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getDetails(id) {
        axios.get(`/api/cda-additional-floor-requisition/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                CDAFloorRequisition = response.data;
                CDAFloorRequisition.FloorReqDate = formatDateToDefault(CDAFloorRequisition.FloorReqDate);
                isEditable = false;
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnNewItem").hide();
                $formEl.find("#CompanyId").prop("disabled", true); 
                $formEl.find("#FloorReqBy").prop("disabled", true);
                $formEl.find("#AdditionalReason").prop("disabled", true);
                if (isApprovePage && !CDAFloorRequisition.AdditionalApprove) {
                    $formEl.find("#btnApproveYPO").show();
                }
                else {
                    $formEl.find("#btnApproveYPO").hide();
                }
                setFormData($formEl, CDAFloorRequisition);
                initChildTable();
                $formEl.find("#SubGroupId").val(CDAFloorRequisition.SubGroupID);
                $tblChildEl.bootstrapTable("load", CDAFloorRequisition.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = CDAFloorRequisition.Childs;
        axios.post("/api/cda-additional-floor-requisition/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
})();
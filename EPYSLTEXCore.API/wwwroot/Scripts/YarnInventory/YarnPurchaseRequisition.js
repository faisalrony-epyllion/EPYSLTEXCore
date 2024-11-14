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

    var YPRequisition;

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

        isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val());
        isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());
        if (isAcknowledgePage) {
            $toolbarEl.find("#btnNew").hide();
            $toolbarEl.find("#btnNewItem").hide();
            $toolbarEl.find("#btnPending").hide();
            $toolbarEl.find("#btnYPOPending").hide();
            $toolbarEl.find("#btnYPOApproved").hide();
            $toolbarEl.find("#btnYPOUnApproved").hide();
            $toolbarEl.find("#btnYPOAkg").show();
            $toolbarEl.find("#btnYPOUnAkg").show();
            $toolbarEl.find("#btnYPOAkgPending").show();
            status = statusConstants.PARTIALLY_COMPLETED;
            toggleActiveToolbarBtn($toolbarEl.find("#btnYPOAkgPending"), $toolbarEl);
        }
        else if (isApprovePage) {
            $toolbarEl.find("#btnNew").hide();
            $toolbarEl.find("#btnNewItem").hide();
            $toolbarEl.find("#btnPending").hide();
            $toolbarEl.find("#btnYPOPending").show();
            $toolbarEl.find("#btnYPOApproved").show();
            $toolbarEl.find("#btnYPOUnApproved").show();
            $toolbarEl.find("#btnYPOAkg").hide();
            $toolbarEl.find("#btnYPOUnAkg").hide();
            $toolbarEl.find("#btnYPOAkgPending").hide();
            status = statusConstants.PROPOSED;
            toggleActiveToolbarBtn($toolbarEl.find("#btnYPOPending"), $toolbarEl);
        }
        else {
            toggleActiveToolbarBtn($toolbarEl.find("#btnPending"), $toolbarEl);
        }




      
        initMasterTable();
        getMasterTableData();

        $formEl.find("#YpRequiredDate").datepicker({
            todayHighlight: true,
            autoclose: true
        });
        $formEl.find("#YpReqDate").datepicker({
            todayHighlight: true,
            autoclose: true
        });

        $formEl.find("#CompanyId").prop("disabled", true);
        $formEl.find("#YpRequisitionBy").prop("disabled", true);
        $formEl.find("#Remarks").prop("readonly", true);


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


        $formEl.find("#btnRejectYPO").click(function (e) {
            e.preventDefault();
            
            var l = $(this).ladda();
            l.ladda('start');
            bootbox.prompt("Are you sure you want to reject this?", function (result) {
                if (!result) {
                    l.ladda('stop');
                    return toastr.error("Reject reason is required.");
                }
                var id = $formEl.find("#Id").val();
                var reason = result;
                axios.post(`/api/yarn-purchase-requisition/reject/${id}/${reason}`)
                    .then(function () {
                        toastr.success("Requisition rejected successfully.");
                        l.ladda('stop');
                        backToList();
                    })
                    .catch(showResponseError);

            });
        });

        $toolbarEl.find("#btnYPOPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PROPOSED;

            initMasterTable();
            getMasterTableData();
        });
        $toolbarEl.find("#btnYPOApproved").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.APPROVED;
            initMasterTable();
            getMasterTableData();
        });
        $toolbarEl.find("#btnYPOAkgPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PARTIALLY_COMPLETED;
            initMasterTable();
            getMasterTableData();
        });
        $toolbarEl.find("#btnYPOUnApproved").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.REJECT;

            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnYPOAkg").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.ACKNOWLEDGE;

            initMasterTable();
            getMasterTableData();
        });
        $toolbarEl.find("#btnYPOUnAkg").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.UN_ACKNOWLEDGE;

            initMasterTable();
            getMasterTableData();
        });



        $("#BuyerId").on("select2:select select2:unselect", function (e) {
            if (e.params.type === 'unselect') initSelect2($("#FabricBookingIds"), []);
            else getBookingByBuyer(e.params.data.id);
        })

        $toolbarEl.find("#btnNew").on("click", getNew);

        $formEl.find("#btnNewItem").on("click", function (e) {
            e.preventDefault();
            var newChildItem = {
                Id: getMaxIdForArray(YPRequisition.Childs, "Id"),
                YPRequisitionMasterId: 0,
                YarnSubProgramIds: 0,
                YarnSubProgramNames: '',
                YarnCategory: "",
                NoOfThread: 0,
                ReqQty: 0,
                Remarks: "",
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

            YPRequisition.Childs.push(newChildItem);
            $tblChildEl.bootstrapTable('load', YPRequisition.Childs);
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnApproveYPO").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#Id").val();
            axios.post(`/api/yarn-purchase-requisition/approve/${id}`)
                .then(function () {
                    toastr.success("Requisition approved successfully.");
                    backToList();
                })
                .catch(showResponseError);
        });


        $formEl.find("#btnAkgYPO").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#Id").val();
            axios.post(`/api/yarn-purchase-requisition/acknowledge/${id}`)
                .then(function () {
                    toastr.success("Requisition acknowledged successfully.");
                    backToList();
                })
                .catch(showResponseError);
        });
        $formEl.find("#btnUAkgYPO").click(function (e) {
            e.preventDefault();
            var l = $(this).ladda();
            l.ladda('start');
            bootbox.prompt("Are you sure you want to reject this?", function (result) {
                if (!result) {
                    l.ladda('stop');
                    return toastr.error("Reject reason is required.");
                }
                var id = $formEl.find("#Id").val();
                var reason = result;
                axios.post(`/api/yarn-purchase-requisition/unacknowledge/${id}/${reason}`)
                    .then(function () {
                        toastr.success("Requisition rejected successfully.");
                        l.ladda('stop');
                        backToList();
                    })
                    .catch(showResponseError);

            });
        });


        $formEl.find("#btnSaveAndProposeYPO").click(function (e) {
            e.preventDefault();
            saveForApproval(this);
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
                            getDetails(row.Id);
                        }
                    }
                },
                {
                    field: "YpReqNo",
                    title: "Requisition No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "YpReqDate",
                    title: "Requisition Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToMMDDYYYY(value);
                    }
                },
                {
                    field: "YpRequiredDate",
                    title: "Required Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToMMDDYYYY(value);
                    }
                }
                ,

                {
                    field: "YpRequisitionByUser",
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
        var url = "/api/yarn-purchase-requisition/list?status=" + status + "&" + queryParams;
        axios.get(url)
            .then(function (response) {
                console.log(response);
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
                            $tblChildEl.bootstrapTable('remove', { field: 'Id', values: [row.Id] });
                        },
                    },
                    visible: isEditable
                },
                {
                    field: "YarnProgramId",
                    title: "Yarn Program",
                    visible: isEditable,
                    editable: {
                        type: 'select2',
                        title: 'Select Yarn Type',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: YPRequisition.YarnProgramList,
                        select2: { width: 130, placeholder: 'Yarn Type', alloclear: true }
                    }
                },
                {
                    field: "Segment1ValueId",
                    title: "Yarn Type",
                    visible: isEditable,
                    editable: {
                        type: 'select2',
                        title: 'Select Yarn Composition',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: YPRequisition.YarnTypeList,
                        select2: { width: 200, placeholder: 'Yarn Composition', alloclear: true }
                    }
                },
                {
                    field: "Segment1ValueDesc",
                    title: "Yarn Type",
                    visible: !isEditable
                },
                {
                    field: "Segment3ValueId",
                    title: "Yarn Composition",
                    visible: isEditable,
                    editable: {
                        type: 'select2',
                        title: 'Select Yarn Composition',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: YPRequisition.YarnCompositionList,
                        select2: { width: 200, placeholder: 'Yarn Composition', alloclear: true }
                    }
                },
                {
                    field: "Segment3ValueDesc",
                    title: "Yarn Composition",
                    visible: !isEditable
                },
                {
                    field: "Segment2ValueId",
                    title: "Yarn Count",
                    formatter: function (value, row, index, field) {
                        return isEditable ? ['<span class="btn-group">',
                            '<a href="javascript:void(0)" class="editable-link edit">' + row.Segment2ValueDesc + '</a>',
                            '</span>'].join(' ') : row.Segment2ValueDesc;
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
                        return isEditable ? `<a href="javascript:void(0)" class="editable-link edit">${text}</a>` : text;
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            showBootboxSelect2MultipleDialog("Select Yarn Sub-Programs", "YarnSubProgramIds", "Select Yarn Sub-Programs", YPRequisition.YarnSubProgramList, function (result) {
                                if (result) {
                                    row.YarnSubProgramIds = result.map(function (item) { return item.id }).join(",");
                                    row.YarnSubProgramNames = result.map(function (item) { return item.text }).join(",");
                                    $tblChildEl.bootstrapTable('updateByUniqueId', { id: row.Id, row: row });
                                    console.log(result);
                                }
                            });
                        },
                    }
                },
                {
                    field: "YarnCategory",
                    title: "Yarn Category"
                }
               ,
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
                    case "YarnChildPoBuyerIds":
                        if (row.YarnChildPoBuyerIds) {
                            selectedValue = setMultiSelectValueInBootstrapTableEditable(YPRequisition.BuyerList, row.YarnChildPoBuyerIds);
                            row.YarnChildPoBuyerIds = selectedValue.id;
                        }
                        break;
                    case "YarnChildPoExportIds":
                        if (row.YarnChildPoExportIds) {
                            selectedValue = setMultiSelectValueInBootstrapTableEditable(YPRequisition.ExportOrderList, row.YarnChildPoExportIds);
                            row.YarnChildPoExportIds = selectedValue.id;
                        }
                        break;
                    case "Segment1ValueId":
                        if (row.Segment1ValueId) {
                            selectedValue = YPRequisition.YarnTypeList.find(function (el) { return el.id == row.Segment1ValueId });
                            row.Segment1ValueDesc = selectedValue.text;
                        }
                        break;
                    case "Segment3ValueId":
                        if (row.Segment3ValueId) {
                            selectedValue = YPRequisition.YarnCompositionList.find(function (el) { return el.id == row.Segment3ValueId });
                            row.Segment3ValueDesc = selectedValue.text;
                        }
                        break;
                    case "YarnProgramId":
                        if (row.YarnProgramId) {
                            selectedValue = YPRequisition.YarnProgramList.find(function (el) { return el.id == row.YarnProgramId });
                            row.YarnProgram = selectedValue.text;
                        }
                        break;
                    case "Segment5ValueId":
                        if (row.Segment5ValueId) {
                            selectedValue = YPRequisition.YarnColorList.find(function (el) { return el.id == row.Segment5ValueId });
                            row.Segment5ValueDesc = selectedValue.text;
                        }
                        break;
                    default:
                        break;
                }

                row.PIValue = (row.BookingQty * row.Rate).toFixed(2);
                row.YarnCategory = calculateYarnCategory(row);
                if ((row.Segment1ValueId == 625) || (row.Segment1ValueId == 8238)) {
                    row.NoOfThread = "0";
                }
                $tblChildEl.bootstrapTable('load', YPRequisition.Childs);
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

    function getNew() {
        axios.get(`/api/yarn-purchase-requisition/new`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                $formEl.find("#CompanyId").prop("disabled", false);
                $formEl.find("#YpRequisitionBy").prop("disabled", false);
                $formEl.find("#Remarks").prop("readonly", false);
                $formEl.find("#divRejectReason").hide();
                $formEl.find("#divUnAcknowledgeReason").hide();
                isEditable = true;
                YPRequisition = response.data;
                YPRequisition.YpRequiredDate = formatDateToMMDDYYYY(YPRequisition.YpRequiredDate);
                YPRequisition.YpReqDate = formatDateToMMDDYYYY(YPRequisition.YpReqDate);
                setFormData($formEl, YPRequisition);
                initChildTable();
                $("#btnSave").show();
                $toolbarEl.find("#btnNewItem").show();
                $("#btnSaveAndProposeYPO").show();
                $("#btnApproveYPO").hide();
                $("#btnRejectYPO").hide();
                $("#btnAkgYPO").hide();
                $("#btnUAkgYPO").hide();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/yarn-purchase-requisition/${id}`)
            .then(function (response) {
               
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                YPRequisition = response.data;
                YPRequisition.YpRequiredDate = formatDateToMMDDYYYY(YPRequisition.YpRequiredDate);
                YPRequisition.YpReqDate = formatDateToMMDDYYYY(YPRequisition.YpReqDate);
                $formEl.find("#CompanyId").prop("disabled", true);
                $formEl.find("#YpRequisitionBy").prop("disabled", true);
                $formEl.find("#Remarks").prop("readonly", true);
                isEditable = false;

                setFormData($formEl, YPRequisition);

                if (YPRequisition.SendForApproval && !YPRequisition.Approve) { //proposed
                    $formEl.find("#btnSave").hide();
                    $toolbarEl.find("#btnNewItem").hide();
                    $formEl.find("#btnSaveAndProposeYPO").hide();
                    $formEl.find("#btnAkgYPO").hide();
                    $formEl.find("#btnUAkgYPO").hide();
                    if (isApprovePage) {
                        if (!YPRequisition.Reject) {
                            $formEl.find("#btnApproveYPO").show();
                            $formEl.find("#btnRejectYPO").show();
                        }
                        else {
                            //$formEl.find("#btnSave").show();
                            $formEl.find("#btnSaveAndProposeYPO").show();
                            $formEl.find("#btnApproveYPO").hide();
                            $formEl.find("#btnRejectYPO").hide();

                        }
                    }
                    else {
                        $formEl.find("#btnApproveYPO").hide();
                        $formEl.find("#btnRejectYPO").hide();
                    }
                }
                else if (YPRequisition.SendForApproval && YPRequisition.Approve && !YPRequisition.Acknowlege) { //approved
                    $formEl.find("#btnSave").hide();
                    $toolbarEl.find("#btnNewItem").hide();
                    $formEl.find("#btnSaveAndProposeYPO").hide();
                    $formEl.find("#btnApproveYPO").hide();
                    $formEl.find("#btnRejectYPO").hide();
                    if (isAcknowledgePage) {
                        if (!YPRequisition.UnAcknowlege) {
                            $formEl.find("#btnAkgYPO").show();
                            $formEl.find("#btnUAkgYPO").show();
                        }
                        else {
                            $formEl.find("#btnAkgYPO").hide();
                            $formEl.find("#btnUAkgYPO").hide();
                        }
                    }
                    else {
                        $formEl.find("#btnAkgYPO").hide();
                        $formEl.find("#btnUAkgYPO").hide();
                    }

                }
                else if (YPRequisition.SendForApproval && YPRequisition.Approve && YPRequisition.Acknowlege) { //ackg
                    $formEl.find("#btnSave").hide();
                    $toolbarEl.find("#btnNewItem").hide();
                    $formEl.find("#btnSaveAndProposeYPO").hide();
                    $formEl.find("#btnApproveYPO").hide();
                    $formEl.find("#btnRejectYPO").hide();
                    $formEl.find("#btnAkgYPO").hide();
                    $formEl.find("#btnUAkgYPO").hide();
                }
                else {
                    $formEl.find("#CompanyId").prop("disabled", false);
                    $formEl.find("#YpRequisitionBy").prop("disabled", false);
                    $formEl.find("#Remarks").prop("readonly", false);
                    isEditable = true;
                    $formEl.find("#btnSave").show();
                    $formEl.find("#btnSaveAndProposeYPO").show();
                    $formEl.find("#btnApproveYPO").hide();
                    $formEl.find("#btnRejectYPO").hide();
                    $formEl.find("#btnAkgYPO").hide();
                    $formEl.find("#btnUAkgYPO").hide();
                }
                if (YPRequisition.Reject) {
                    $formEl.find("#divRejectReason").show();
                }
                else {
                    $formEl.find("#divRejectReason").hide();
                }
                if (YPRequisition.UnAcknowlege) {
                    $formEl.find("#divUnAcknowledgeReason").show();
                }
                else {
                    $formEl.find("#divUnAcknowledgeReason").hide();
                }


                initChildTable();
                $tblChildEl.bootstrapTable("load", YPRequisition.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
                
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
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
                    $tblChildEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
                })
            })
            .catch(function (err) {
                console.log(err);
            });
    }

    //function getBookingByBuyer(buyerId) {
    //    axios.get(`/api/selectoption/booking-by-buyer/${buyerId}`)
    //        .then(function (response) {
    //            initSelect2($("#FabricBookingIds"), response.data);
    //        })
    //        .catch(function () {
    //            toastr.error(err.response.data.Message);
    //        });
    //}

    function save(isApprove = false) {
        //var data = formDataToJson($formEl.serializeArray());
        //data.append("Childs", JSON.stringify(YPRequisition.Childs));
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = YPRequisition.Childs;
        YPRequisition.Approve = isApprove;
        console.log(YPRequisition.Childs);

        axios.post("/api/yarn-purchase-requisition/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    
    function saveForApproval(isApprove = false) {
        //var data = formDataToJson($formEl.serializeArray());
        //data.append("Childs", JSON.stringify(YPRequisition.Childs));
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = YPRequisition.Childs;
        console.log(YPRequisition.Childs);
        axios.post("/api/yarn-purchase-requisition/saveForApproval", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    //function initNewAttachment($el) {
    //    $el.fileinput('destroy');
    //    $el.fileinput({
    //        showUpload: false,
    //        previewFileType: 'any'
    //    });
    //}

    //function initAttachment(path, $el) {
    //    if (!path) {
    //        initNewAttachment($el);
    //        return;
    //    }

    //    var preveiwData = [rootPath + path];
    //    var previewConfig = [{ type: 'image', caption: "Swatch Attachment", key: 1 }];

    //    $el.fileinput('destroy');
    //    $el.fileinput({
    //        showUpload: false,
    //        initialPreview: preveiwData,
    //        initialPreviewAsData: true,
    //        initialPreviewFileType: 'image',
    //        initialPreviewConfig: previewConfig,
    //        purifyHtml: true,
    //        required: true,
    //        maxFileSize: 4096
    //    });
    //}
})();
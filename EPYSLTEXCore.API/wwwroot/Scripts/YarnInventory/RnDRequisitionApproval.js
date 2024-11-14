(function () {
    var menuId, pageName;
    var toolbarId;
    var isAcknowledge = false;
    var sendForApproval = false;
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

        isAcknowledge = convertToBoolean($(pageId).find("#Acknowledge").val());

        if (isAcknowledge) {
            $("#btnSave").hide();
            $("#btnSaveAndSend").hide();
            $("#btnAcknowledge").show();
        }

        initMasterTable();
        getMasterTableData();

        $toolbarEl.find("#btnApprove").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.APPROVED;

            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;

            initMasterTable();
            getMasterTableData();
        });

        $("#btnRejectYPO").click(function (e) {
            e.preventDefault();

            bootbox.prompt("Are you sure you want to reject this?", function (result) {
                if (!result) {
                    return toastr.error("Reject reason is required.");
                }

                var data = formDataToJson($formEl.serializeArray());
                data.RejectReason = result;
                data["Childs"] = YPRequisition.Childs;
                //YPRequisition.Approve = isApprove;
                //console.log(YPRequisition.Childs);

                axios.post("/api/yarn-rnd-requisition/Reject", data)
                    .then(function () {
                        toastr.warning(constants.REJECT_SUCCESSFULLY);
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
                Rate: 0,
                Remarks: "",
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
                BuyerNames: "",
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
            Approve(this);
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
                            '<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="View Requisition">',
                            '<i class="fa fa-eye" aria-hidden="true"></i>',
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
                    field: "RnDReqNo",
                    title: "Requisition No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "RnDReqDate",
                    title: "Requisition Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "Supplier",
                    title: "Supplier",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Spinner",
                    title: "Spinner",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Location",
                    title: "Location",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "RnDApproveBy",
                    title: "Approve By",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status == statusConstants.APPROVED
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
        var url = "/api/yarn-rnd-requisition/list?gridType=bootstrap-table&status=" + status + "&" + queryParams;
        axios.get(url)
            .then(function (response) {
                //console.log(response);
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
                        source: YPRequisition.YarnProgramList,
                        select2: { width: 130, placeholder: 'Yarn Type', allowClear: true }
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
                        source: YPRequisition.YarnTypeList,
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
                        source: YPRequisition.YarnCompositionList,
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
                            showBootboxSelect2MultipleDialog("Select Yarn Sub-Programs", "YarnSubProgramIds", "Select Yarn Sub-Programs", YPRequisition.YarnSubProgramList, function (result) {
                                if (result) {
                                    row.YarnSubProgramIds = result.map(function (item) { return item.id }).join(",");
                                    row.YarnSubProgramNames = result.map(function (item) { return item.text }).join(",");
                                    $tblChildEl.bootstrapTable('updateByUniqueId', { id: row.Id, row: row });
                                    //console.log(result);
                                }
                            });
                        },
                    }
                },
                {
                    field: "YarnCategory",
                    title: "Yarn Category"
                }
                //,
                //{
                //    field: "Segment5ValueId",
                //    title: "Yarn Color",
                //    editable: {
                //        type: 'select2',
                //        title: 'Select Yarn Color',
                //        inputclass: 'input-sm',
                //        showbuttons: false,
                //        source: YPRequisition.YarnColorList,
                //        select2: { width: 130, placeholder: 'Yarn Color', allowClear: true }
                //    }
                //},
                //{
                //    field: "Segment4ValueDesc",
                //    title: "Shade",
                //    editable: {
                //        type: 'text',
                //        inputclass: 'input-sm',
                //        showbuttons: false
                //    }
                //}
                ,
                {
                    field: "DisplayUnitDesc",
                    title: "Unit"
                },
                {
                    field: "Rate",
                    title: "Rate",
                    align: 'right',
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm m-w-50',
                        showbuttons: false
                    }
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
        initMasterTable();
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
        axios.get(`/api/yarn-rnd-requisition/new`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                YPRequisition = response.data;
                YPRequisition.RnDReqDate = formatDateToDefault(YPRequisition.RnDReqDate);
                setFormData($formEl, YPRequisition);
                initChildTable();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/yarn-rnd-requisition/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                YPRequisition = response.data;
                //console.log(YPRequisition);
                YPRequisition.YarnPRRequiredDate = formatDateToDefault(YPRequisition.RnDRequiredDate);
                YPRequisition.YarnPRDate = formatDateToDefault(YPRequisition.RnDReqDate);
                sendForApproval = YPRequisition.SendForApproval;
                setFormData($formEl, YPRequisition);

                initChildTable();
                $tblChildEl.bootstrapTable("load", YPRequisition.Childs);
                $tblChildEl.bootstrapTable('hideLoading');

                if (sendForApproval) {
                    $("#btnSave").hide();
                    $("#btnSaveAndProposeYPO").hide();
                    $("#btnApproveYPO").show();
                    $("#btnRejectYPO").show();
                }
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
                toastr.error(err.response.data.Message);
            });
    }

    function getBookingByBuyer(buyerId) {
        axios.get(`/api/selectoption/booking-by-buyer/${buyerId}`)
            .then(function (response) {
                initSelect2($("#FabricBookingIds"), response.data);
            })
            .catch(function () {
                toastr.error(err.response.data.Message);
            });
    }

    function save(isApprove = false) {
        //var data = formDataToJson($formEl.serializeArray());
        //data.append("Childs", JSON.stringify(YPRequisition.Childs));
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = YPRequisition.Childs;
        YPRequisition.Approve = isApprove;
        //console.log(YPRequisition.Childs);

        axios.post("/api/yarn-rnd-requisition/save", data)
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
        //console.log(YPRequisition.Childs);
        axios.post("/api/yarn-rnd-requisition/saveForApproval", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function Approve() {
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = YPRequisition.Childs;
        //console.log(YPRequisition.Childs);
        axios.post("/api/yarn-rnd-requisition/Approve", data)
            .then(function () {
                toastr.success("Approve successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function initNewAttachment($el) {
        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            previewFileType: 'any'
        });
    }

    function initAttachment(path, $el) {
        if (!path) {
            initNewAttachment($el);
            return;
        }

        var preveiwData = [rootPath + path];
        var previewConfig = [{ type: 'image', caption: "Swatch Attachment", key: 1 }];

        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            initialPreview: preveiwData,
            initialPreviewAsData: true,
            initialPreviewFileType: 'image',
            initialPreviewConfig: previewConfig,
            purifyHtml: true,
            required: true,
            maxFileSize: 4096
        });
    }
})();
(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        filter: '',
        sort: '',
        order: ''
    }
    var status;

    var YarnAllocationProposal;

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

        status = statusConstants.COMPLETED;
        $("#btnList").css('background', '#5cb85c');
        $("#btnList").css('color', '#FFFFFF');
        initMasterTable();
        initChildTable();
        getMasterTableData();

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;

            initMasterTable();
            getMasterTableData();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
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
                    title: "",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        return `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="View Issue">
                                        <i class="fa fa-edit" aria-hidden="true"></i>
                                    </a>`;
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.YAPMasterID);
                        }
                    }
                },
                {
                    field: "ProposalNo",
                    title: "Proposal No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ProposalDate",
                    title: "Proposal Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "AllocationNo",
                    title: "Allocation No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BookingNo",
                    title: "Booking No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Buyer",
                    title: "Buyer",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "EWONo",
                    title: "Export No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Location",
                    title: "Location",
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
        var url = `/api/yarn-allocation-proposal/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
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
                    field: "Segment1ValueDesc",
                    title: "Fiber Type",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "Segment2ValueDesc",
                    title: "Blend Type",
                    cellStyle: function () { return { classes: 'm-w-150' } }
                },
                {
                    field: "Segment3ValueDesc",
                    title: "Yarn Type"
                },
                {
                    field: "Segment4ValueDesc",
                    title: "Yarn Program"
                },
                {
                    field: "Segment5ValueDesc",
                    title: "Yarn Sub Program"
                },
                {
                    field: "Segment6ValueDesc",
                    title: "Manufacturing Line"
                },
                {
                    field: "Segment7ValueDesc",
                    title: "Manufacturing Process"
                },
                {
                    field: "Segment8ValueDesc",
                    title: "Manufacturing Sub Process"
                },
                {
                    field: "Segment9ValueDesc",
                    title: "Yarn Composition"
                },
                {
                    field: "Segment10ValueDesc",
                    title: "Yarn Count"
                },
                {
                    field: "Segment11ValueDesc",
                    title: "Yarn Color"
                },
                {
                    field: "Segment12ValueDesc",
                    title: "Color Grade"
                },
                {
                    field: "Segment13ValueDesc",
                    title: "Shade"
                },
                {
                    field: "Segment14ValueDesc",
                    title: "Yarn Color"
                },
                {
                    field: "Segment15ValueDesc",
                    title: "Color Grade"
                },
                {
                    field: "Uom",
                    title: "Unit",
                    cellStyle: function () { return { classes: 'm-w-60' } }
                },
                {
                    field: "DiagnosticStockQty",
                    title: "Diagnostic Stock Qty",
                    align: 'center',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                        }
                    }
                },
                {
                    field: "Approve",
                    title: "Is Approve?",
                    width: 100,
                    editable: {
                        type: 'select2',
                        title: 'Select Type',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: [{ id: 0, text: 'Reject' }, { id: 1, text: 'Approve' }],
                        select2: { width: 100, placeholder: 'Select Type', allowClear: true }
                    }
                },
                {
                    field: "Remarks",
                    title: "Remarks",
                    width: 120,
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" min="0" style="padding-right: 24px;">'
                    }
                }
            ]
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
        $formEl.find("#YAPMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(newId) {
        axios.get(`/api/yarn-allocation-proposal/new/${newId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                YarnAllocationProposal = response.data;
                YarnAllocationProposal.ProposalDate = formatDateToDefault(YarnAllocationProposal.ProposalDate);
                YarnAllocationProposal.FabricDeliveryDate = formatDateToDefault(YarnAllocationProposal.FabricDeliveryDate);

                setFormData($formEl, YarnAllocationProposal);
                $tblChildEl.bootstrapTable("load", YarnAllocationProposal.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/yarn-allocation-proposal/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                YarnAllocationProposal = response.data;
                YarnAllocationProposal.ProposalDate = formatDateToDefault(YarnAllocationProposal.ProposalDate);
                YarnAllocationProposal.FabricDeliveryDate = formatDateToDefault(YarnAllocationProposal.FabricDeliveryDate);

                setFormData($formEl, YarnAllocationProposal);
                $tblChildEl.bootstrapTable("load", YarnAllocationProposal.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        YarnAllocationProposal.Childs.forEach(function (entry) {
            if (entry.Approve == 1) {
                entry.Approve = true;
                entry.Reject = false;
            } else {
                entry.Approve = false;
                entry.Reject = true;
            }
        });
        data["Childs"] = YarnAllocationProposal.Childs;

        axios.post("/api/yarn-allocation-proposal/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

})();
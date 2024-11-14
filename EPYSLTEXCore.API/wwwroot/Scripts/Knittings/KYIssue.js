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
    var status;

    var KYIssue;

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

        status = statusConstants.PENDING;
        initMasterTable();
        //initChildTable();
        getMasterTableData();

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

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnSaveAndAapprove").click(function (e) {
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
                    title: "",
                    width: 75,
                    minWidth: 75,
                    maxWidth: 75,
                    align: "center",
                    formatter: function (value, row, index, field) {
                        if (status === statusConstants.PENDING) {
                            return `<a class="btn btn-xs btn-default add" href="javascript:void(0)" title="New Issue">
                                        <i class="fa fa-plus" aria-hidden="true"></i>
                                    </a>`;
                        }
                        else {
                            return `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Issue">
                                        <i class="fa fa-edit" aria-hidden="true"></i>
                                    </a>`;
                        }
                    },
                    events: {
                        'click .add': function (e, value, row, index) { 
                            e.preventDefault();
                            getNew(row.KYReqMasterID);
                        },
                        'click .edit': function (e, value, row, index) { 
                            e.preventDefault();
                            getDetails(row.KYIssueMasterID);
                        }
                    }
                },
                {
                    field: "KYIssueNo",
                    title: "Issue No",
                    filterControl: "input",
                    visible: status === statusConstants.COMPLETED,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "KYIssueDate",
                    title: "Issue Date",
                    visible: status === statusConstants.COMPLETED,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "KYIssueByUser",
                    title: "Issue By",
                    filterControl: "input",
                    visible: status === statusConstants.COMPLETED,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "KYReqNo",
                    title: "Req. No",
                    filterControl: "input",
                    visible: status === statusConstants.COMPLETED,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "KYReqDate",
                    title: "Req. Date",
                    visible: status === statusConstants.COMPLETED,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "BAnalysisNo",
                    title: "Analysis No",
                    filterControl: "input",
                    visible: status === statusConstants.COMPLETED,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BAnalysisDate",
                    title: "Analysis Date",
                    visible: status === statusConstants.COMPLETED,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "BookingNo",
                    title: "Booking No",
                    filterControl: "input",
                    visible: status === statusConstants.COMPLETED,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BookingDate",
                    title: "Booking Date",
                    visible: status === statusConstants.COMPLETED,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "Supplier",
                    title: "Supplier",
                    filterControl: "input",
                    visible: status === statusConstants.COMPLETED,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Spinner",
                    title: "Spinner",
                    filterControl: "input",
                    visible: status === statusConstants.COMPLETED,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                
                //Complete

                //Pending
                {
                    field: "KYReqNo",
                    title: "Req. No",
                    filterControl: "input",
                    visible: status === statusConstants.PENDING,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "KYReqDate",
                    title: "Req. Date",
                    visible: status === statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "BAnalysisNo",
                    title: "Analysis No",
                    filterControl: "input",
                    visible: status === statusConstants.PENDING,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BAnalysisDate",
                    title: "Analysis Date",
                    visible: status === statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "BookingNo",
                    title: "Booking No",
                    filterControl: "input",
                    visible: status === statusConstants.PENDING,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BookingDate",
                    title: "Booking Date",
                    visible: status === statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "Remarks",
                    title: "Remarks",
                    filterControl: "input",
                    visible: status === statusConstants.PENDING,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                //Pending

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
        var url = `/api/KY-Issue/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
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
            data: KYIssue.KYIssueChilds,
            columns: [
                {
                    field: "YarnProgramName",
                    title: "Yarn Program",
                    width: 100
                },
                {
                    field: "YarnType",
                    title: "Yarn Type",
                    width: 60
                },
                {
                    field: "YarnComposition",
                    title: "Yarn Composition",
                    width: 100
                },
                {
                    field: "YarnCount",
                    title: "Yarn Count",
                    width: 60
                },
                {
                    field: "YarnColor",
                    title: "Yarn Color",
                    width: 80
                },
                {
                    field: "YarnShade",
                    title: "Shade",
                    width: 60
                },
                {
                    field: "YarnSubProgramNames",
                    title: "Yarn Sub Program",
                    width: 100
                },
                //{
                //    field: "LotNo",
                //    title: "Lot No",
                //    width: 100
                //},
                //{
                //    field: "PhysicalCount",
                //    title: "Physical Count"
                //},
                {
                    field: "PhysicalCount",
                    title: "Physical Count",
                    align: 'center',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" min="0" style="padding-right: 24px;">'  
                    }
                },
                {
                    field: "LotNo",
                    title: "Lot No",
                    align: 'center',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" min="0" style="padding-right: 24px;">'
                    }
                },
                {
                    field: "YarnBrandID",
                    title: "Spinner",
                    //visible: status == statusConstants.PENDING || status == statusConstants.COMPLETED,
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: KYIssue.YarnBrandList,
                        select2: { width: 200, placeholder: 'Select yarn brand.', allowClear: true }
                    }
                },
                {
                    field: "Uom",
                    title: "Unit",
                    width: 80
                },
                {
                    field: "ReqQty",
                    title: "Req. Qty",
                    width: 80
                },
                {
                    field: "IssueQty",
                    title: "Issue Qty",
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
                    field: "IssueQtyCone",
                    title: "Issue Qty (Cone)",
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
                    field: "IssueQtyCarton",
                    title: "Issue Qty (Crtn)",
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
        $formEl.find("#KYIssueMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(KYReqMasterID) {
        axios.get(`/api/KY-Issue/new/${KYReqMasterID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                KYIssue = response.data;
                KYIssue.KYIssueDate = formatDateToDefault(KYIssue.KYIssueDate);
                KYIssue.KYReqDate = formatDateToDefault(KYIssue.KYReqDate);
                KYIssue.BAnalysisDate = formatDateToDefault(KYIssue.BAnalysisDate);
                KYIssue.BookingDate = formatDateToDefault(KYIssue.BookingDate);

                setFormData($formEl, KYIssue);
                //console.log(KYIssue.KYIssueChilds);
                initChildTable();
                $tblChildEl.bootstrapTable("load", KYIssue.KYIssueChilds);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/KY-Issue/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                KYIssue = response.data;
                KYIssue.KYIssueDate = formatDateToDefault(KYIssue.KYIssueDate);
                KYIssue.KYReqDate = formatDateToDefault(KYIssue.KYReqDate);
                KYIssue.BAnalysisDate = formatDateToDefault(KYIssue.BAnalysisDate);
                KYIssue.BookingDate = formatDateToDefault(KYIssue.BookingDate);

                setFormData($formEl, KYIssue);
                initChildTable();
                $tblChildEl.bootstrapTable("load", KYIssue.KYIssueChilds);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save(invokedBy, isApprove = false) {
        var data = formDataToJson($formEl.serializeArray());
        data["KYIssueChilds"] = KYIssue.KYIssueChilds;
        data.Approve = isApprove;

        axios.post("/api/KY-Issue/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }


})();
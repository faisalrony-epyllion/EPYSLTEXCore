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

    var masterData, currentChildRowData;
    var status = statusConstants.PENDING;

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

        initMasterTable();
        initChildTable();
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
            save();
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
                        var template;
                        if (status === statusConstants.PENDING) {
                            template =
                                `<a class="btn btn-xs btn-default add" href="javascript:void(0)" title="Pending">
                                    <i class="fa fa-plus" aria-hidden="true"></i>
                                </a>`;
                        }
                        else {
                            template =
                                `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="List">
                                    <i class="fa fa-edit" aria-hidden="true"></i>
                                </a>`;
                        }

                        return template;
                    },
                    events: {
                        'click .add': function (e, value, row, index) {
                            e.preventDefault();
                            getNew(row.RnDIssueMasterID);
                            $formEl.find("#btnSave").fadeIn();
                        },
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.RnDReceiveMasterID);
                            $formEl.find("#btnSave").fadeOut();
                        }
                    }
                },
                {
                    field: "RnDReceiveNo",
                    title: "Receive No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "RnDReceiveDate",
                    title: "Receive Date",
                    visible: status !== statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "ReceiveBy",
                    title: "Receive By",
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "RnDIssueNo",
                    title: "Issue No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                },
                {
                    field: "RnDReqNo",
                    title: "Requisition No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                },
                {
                    field: "RnDIssueDate",
                    title: "Issue Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                    visible: status == statusConstants.PENDING
                },
                //{
                //    field: "RnDIssueByUser",
                //    title: "Issued By",
                //},
                //{
                //    field: "Supplier",
                //    title: "Supplier",
                //    filterControl: "input",
                //    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                //},
                //{
                //    field: "Spinner",
                //    title: "Spinner",
                //    filterControl: "input",
                //    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                //},
                //{
                //    field: "Location",
                //    title: "Location",
                //    filterControl: "input",
                //    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                //},
                {
                    field: "ReqQty",
                    title: "Req Qty",
                },
                {
                    field: "IssueQty",
                    title: "Issue Qty",
                },
                {
                    field: "ReceiveQty",
                    title: "Receive Qty",
                    visible: status !== statusConstants.PENDING
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
        var url = `/api/yarn-rnd-req-receive/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
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
                    title: "Composition",
                    cellStyle: function () { return { classes: 'm-w-130' } }
                },
                {
                    field: "Segment2ValueDesc",
                    title: "Yarn Type",
                    cellStyle: function () { return { classes: 'm-w-130' } }
                },
                {
                    field: "Segment3ValueDesc",
                    title: "Process"
                },
                {
                    field: "Segment4ValueDesc",
                    title: "Sub Process"
                },
                {
                    field: "Segment5ValueDesc",
                    title: "Quality Parameter",
                    cellStyle: function () { return { classes: 'm-w-130' } }
                },
                {
                    field: "Segment6ValueDesc",
                    title: "Count",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "Segment7ValueDesc",
                    title: "No of Ply"
                },
                {
                    field: "ShadeCode",
                    title: "Shade Code",
                    cellStyle: function () { return { classes: 'm-w-80' } }
                },
                //{
                //    field: "Uom",
                //    title: "Uom"
                //},
                {
                    field: "LotNo",
                    title: "Lot No",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "ReqQty",
                    title: "Req Qty",

                },
                {
                    field: "RnDIssueQty",
                    title: "Issue Qty",

                },
                {
                    field: "IssueQtyCarton",
                    title: "Issue Qty Crtn",

                },
                {
                    field: "IssueQtyCone",
                    title: "Issue Qty Cone",

                },
                {
                    field: "ReceiveQty",
                    title: "Receive Qty",
                    align: 'center',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" step="0.01" pattern="^\d+(?:\.\d{1,2})?$" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                        }
                    }
                },
                {
                    field: "ReceiveQtyCarton",
                    title: "Receive Qty Crtn",
                    align: 'center',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" step="0.01" pattern="^\d+(?:\.\d{1,2})?$" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                        }
                    }
                },
                {
                    field: "ReceiveQtyCone",
                    title: "Receive Qty Cone",
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
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#RnDReceiveMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(reqMasterId) {
        axios.get(`/api/yarn-rnd-req-receive/new/${reqMasterId}`)
            .then(function (response) {
                //console.log(response);
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.RnDReceiveDate = formatDateToDefault(masterData.RnDReceiveDate);
                masterData.RnDIssueDate = formatDateToDefault(masterData.RnDIssueDate);
                setFormData($formEl, masterData);

                $tblChildEl.bootstrapTable("load", masterData.Childs);
                $tblChildEl.bootstrapTable('hideLoading');

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/yarn-rnd-req-receive/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.RnDReceiveDate = formatDateToDefault(masterData.RnDReceiveDate);
                masterData.RnDIssueDate = formatDateToDefault(masterData.RnDIssueDate);
                setFormData($formEl, masterData);
                $tblChildEl.bootstrapTable("load", masterData.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data.Childs = masterData.Childs;

        var hasError = false;
        for (var i = 0; i < data.Childs.length; i++) {
            var child = data.Childs[i];
            if (parseFloat(child.ReceiveQty) > parseFloat(child.RnDIssueQty)) {
                toastr.error("Receive Qty (" + child.ReceiveQty + ") cannot be greater then Issue Qty (" + child.RnDIssueQty+")");
                hasError = true;
                break;
            }
            if (parseFloat(child.ReceiveQtyCarton) > parseFloat(child.IssueQtyCarton)) {
                toastr.error("Receive Qty Crtn (" + child.ReceiveQtyCarton + ") cannot be greater then Issue Qty Crtn (" + child.IssueQtyCarton + ")");
                hasError = true;
                break;
            }
            if (parseFloat(child.ReceiveQtyCone) > parseFloat(child.IssueQtyCone)) {
                toastr.error("Receive Qty Cone (" + child.ReceiveQtyCone + ") cannot be greater then Issue Qty Cone (" + child.IssueQtyCone + ")");
                hasError = true;
                break;
            }
        }

        if (hasError) return false;

        axios.post("/api/yarn-rnd-req-receive/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();

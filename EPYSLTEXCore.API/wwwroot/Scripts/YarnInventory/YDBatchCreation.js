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

    var yarnQCReq;

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

        $("#ReceiveID").on("select2:select select2:unselect", function (e) {
            if (e.params.data.selected) {
                var receiveId = $(this).val();
                getReceiveData(receiveId);
            }
            else {
                yarnQCReq.Childs = [];
                $tblChildEl.bootstrapTable("load", yarnQCReq.Childs);
            }
        })

        $("#QCForId").on("select2:select select2:unselect", function (e) {
            if (e.params.data.selected) {
                yarnQCReq.QCForId = $(this).val();
            }
            else {
                yarnQCReq.QCForId = 0;
            }
        })

        $toolbarEl.find("#btnNew").on("click", getNew);

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnSaveAndSend").click(function (e) {
            e.preventDefault();
            save(true);
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            //status = statusConstants.COMPLETED;

            initMasterTable();
            getMasterTableData();
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
                            '<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Yarn QC Requisition">',
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
                    field: "QCReqNo",
                    title: "Req No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "QCReqDate",
                    title: "Req Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                    //filterControl: "input",
                    //filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "QCReqByUser",
                    title: "Req By",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "QCReqFor",
                    title: "Req For",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ReceiveNo",
                    title: "Receive No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ReceiveDate",
                    title: "Receive Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "IsApprove",
                    title: "Sent for Req",
                    formatter: function (value, row, index, field) {
                        return value ? "Yes" : "No";
                    }
                },
                {
                    field: "IsAcknowledge",
                    title: "Acknowledged",
                    formatter: function (value, row, index, field) {
                        return value ? "Yes" : "No";
                    }
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
        var url = "/api/yarn-qc-requisition/list?gridType=bootstrap-table&" + queryParams;
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
                    field: "Uom",
                    title: "Uom"
                },
                {
                    field: "LotNo",
                    title: "Lot No"
                },
                {
                    field: "ReceiveQty",
                    title: "Receive Qty"
                },
                {
                    field: "ReqQty",
                    title: "Req Qty",
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
                    field: "ReqQtyCone",
                    title: "Req Qty (Cone)",
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
                    field: "ReqQtyCarton",
                    title: "Req Qty (Crtn)",
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
        axios.get("/api/yarn-qc-requisition/new")
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                yarnQCReq = response.data;
                yarnQCReq.QCReqDate = formatDateToDefault(yarnQCReq.QCReqDate);
                yarnQCReq.ReceiveDate = formatDateToDefault(yarnQCReq.ReceiveDate);
                setFormData($formEl, yarnQCReq);
                $formEl.find("#ReceiveID").prop('disabled', false);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/yarn-qc-requisition/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                yarnQCReq = response.data;
                yarnQCReq.QCReqDate = formatDateToDefault(yarnQCReq.QCReqDate);
                yarnQCReq.ReceiveDate = formatDateToDefault(yarnQCReq.ReceiveDate);
                setFormData($formEl, yarnQCReq);
                $tblChildEl.bootstrapTable("load", yarnQCReq.Childs);
                $formEl.find("#ReceiveID").prop('disabled', true);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getReceiveData(receiveId) {
        $tblChildEl.bootstrapTable('showLoading');
        axios.get(`/api/yarn-qc-requisition/new/receiveData?receiveId=${receiveId}`)
            .then(function (response) {
                yarnQCReq.ReceiveID = response.data.ReceiveID;
                yarnQCReq.ReceiveNo = response.data.ReceiveNo;
                yarnQCReq.ReceiveDate = response.data.ReceiveDate;
                yarnQCReq.Supplier = response.data.Supplier;
                yarnQCReq.Spinner = response.data.Spinner;
                yarnQCReq.SpinnerId = response.data.SpinnerId;
                yarnQCReq.SupplierId = response.data.SupplierId;
                yarnQCReq.LocationId = response.data.LocationId;
                yarnQCReq.CompanyId = response.data.CompanyId;
                yarnQCReq.RCompanyId = response.data.RCompanyId;
                yarnQCReq.ReceiveDate = formatDateToDefault(yarnQCReq.ReceiveDate);

                yarnQCReq.Childs = response.data.Childs;
                setFormData($formEl, yarnQCReq);
                $tblChildEl.bootstrapTable("load", yarnQCReq.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save(isApprove = false) {
        var data = yarnQCReq;
        yarnQCReq.Approve = isApprove;

        axios.post("/api/yarn-qc-requisition/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
})();
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
                            getNew(row.RNDReturnMasterID);
                        },
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.RNDReturnReceiveMasterID);
                        }
                    }
                },
                {
                    field: "RNDReturnReceiveNo",
                    title: "Return Receive No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "RNDReturnReceiveDate",
                    title: "Return Receive Date",
                    visible: status !== statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                },
               
                {
                    field: "RNDReturnNo",
                    title: "Return No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "RNDReturnDate",
                    title: "Return Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                    visible: status == statusConstants.PENDING
                },

                {
                    field: "ReturnByUser",
                    title: "Return by",
                    visible: status == statusConstants.PENDING
                },
                {
                    field: "RNDReturnReceiveByUser",
                    title: "Return Receive by",
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "Supplier",
                    title: "Supplier",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-150' } }
                },
                {
                    field: "Spinner",
                    title: "Spinner",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-150' } }
                },
                {
                    field: "Location",
                    title: "Location",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-150' } }
                },
                {
                    field: "ReturnQty",
                    title: "Return Qty(Kg)"
                },
                {
                    field: "ReturnQtyCarton",
                    title: "Return Qty(Crtn)"
                }
                ,
                {
                    field: "ReturnQtyCone",
                    title: "Return Qty(Cone)"
                },
                {
                    field: "ReturnReceiveQty",
                    title: "Return Receive Qty(Kg)",
                    visible: status !== statusConstants.PENDING
                },
                {
                    field: "ReturnReceiveQtyCarton",
                    title: "Return Receive Qty(Crtn)",
                    visible: status !== statusConstants.PENDING
                }
                ,
                {
                    field: "ReturnReceiveQtyCone",
                    title: "Return Receive Qty(Cone)",
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
        var url = `/api/yarn-rnd-returnreceive/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
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
                    field: "FiberType",
                    title: "Fiber Type",
                    cellStyle: function () { return { classes: 'm-w-200' } }
                },
                {
                    field: "BlendType",
                    title: "Blend Type",
                    cellStyle: function () { return { classes: 'm-w-200' } }
                },
                {
                    field: "YarnType",
                    title: "Yarn Type"
                },
                {
                    field: "Program",
                    title: "Yarn Program"
                },
                {
                    field: "SubProgram",
                    title: "Yarn Sub Program",
                    cellStyle: function () { return { classes: 'm-w-200' } }
                },
                {
                    field: "Certifications",
                    title: "Certifications"
                },
                {
                    field: "TechnicalParameter",
                    title: "Technical Parameter"
                },
                {
                    field: "Compositions",
                    title: "Compositions",
                    cellStyle: function () { return { classes: 'm-w-200' } }
                },
                {
                    field: "ManufacturingLine",
                    title: "Manufacturing Line"
                },
                {
                    field: "ManufacturingProcess",
                    title: "Manufacturing Process"
                },
                {
                    field: "Manufacturing Sub Process",
                    title: "Manufacturing Sub Process"
                },
                {
                    field: "Count",
                    title: "Count"
                },
                {
                    field: "Shade",
                    title: "Shade"
                },
                {
                    field: "YarnColor",
                    title: "Yarn Color"
                },
                {
                    field: "ColorGrade",
                    title: "Color Grade"
                },

                {
                    field: "ReturnQty",
                    title: "Return Qty(Kg)",

                },
                {
                    field: "ReturnQtyCarton",
                    title: "Return Qty(Crtn)",

                },
                {
                    field: "ReturnQtyCone",
                    title: "Return Qty(Cone)",

                },
                
                {
                    field: "ReturnReceiveQty",
                    title: "Receive Qty(Kg)",
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
                    field: "ReturnReceiveQtyCarton",
                    title: "Receive Qty(Crtn)",
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
                    field: "ReturnReceiveQtyCone",
                    title: "Receive Qty(Cone)",
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
        $formEl.find("#RNDReturnReceiveMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(rndReturnMasterID) {
        axios.get(`/api/yarn-rnd-returnreceive/new/${rndReturnMasterID}`)
            .then(function (response) {
                //console.log(response);
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                yarnQCReq = response.data;
                yarnQCReq.RNDReturnReceiveDate = formatDateToDefault(yarnQCReq.RNDReturnReceiveDate);
                yarnQCReq.RNDReturnDate = formatDateToDefault(yarnQCReq.RNDReturnDate);
                setFormData($formEl, yarnQCReq);
                $tblChildEl.bootstrapTable("load", yarnQCReq.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/yarn-rnd-returnreceive/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                yarnQCReq = response.data;
                yarnQCReq.RNDReturnReceiveDate = formatDateToDefault(yarnQCReq.RNDReturnReceiveDate);
                yarnQCReq.RNDReturnDate = formatDateToDefault(yarnQCReq.RNDReturnDate);

                setFormData($formEl, yarnQCReq);
                $tblChildEl.bootstrapTable("load", yarnQCReq.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = yarnQCReq.Childs;
        axios.post("/api/yarn-rnd-returnreceive/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();

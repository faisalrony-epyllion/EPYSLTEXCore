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
    var isSelectionPage = false;

    var FirmConceptMaster;

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
        isSelectionPage = convertToBoolean($(`#${pageId}`).find("#SelectionPage").val());

        if (isSelectionPage) {
            status = statusConstants.COMPLETED;
            $toolbarEl.find("#btnList").fadeIn();
            $toolbarEl.find("#btnPending").fadeOut();
            toggleActiveToolbarBtn($toolbarEl.find("#btnList"), $toolbarEl);
        } else {
            status = statusConstants.PENDING;
            $toolbarEl.find("btnPending,#btnList").fadeIn();
            toggleActiveToolbarBtn($toolbarEl.find("#btnPending"), $toolbarEl);
        }
        initMasterTable();
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
                    title: "Actions",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        if (status === statusConstants.PENDING) {
                            return `<a class="btn btn-xs btn-default add" href="javascript:void(0)" title="New Live Product Submission">
                                        <i class="fa fa-plus" aria-hidden="true"></i>
                                    </a>`;
                        } else {
                            return `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Live Product Submission">
                                        <i class="fa fa-edit" aria-hidden="true"></i>
                                    </a>`;
                        }
                    },
                    events: {
                        'click .add': function (e, value, row, index) {
                            e.preventDefault();
                            getNew(row.FCID);
                        },
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.FCID);
                        }
                    }
                },
                {
                    field: "ConceptNo",
                    title: "Concept No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ConceptDate",
                    title: "Concept Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "TrialNo",
                    title: "Trial No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ColorName",
                    title: "Color",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-40' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "SubGroupName",
                    title: "End Use",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-40' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "KnittingType",
                    title: "Machine Type",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-40' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "TechnicalName",
                    title: "Technical Name",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Construction",
                    title: "Construction",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Composition",
                    title: "Composition",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-200' } }
                },
                {
                    field: "Length",
                    title: "Length(CM)"
                },
                 {
                     field: "Width",
                     title: "Width(CM)"
                },
                 {
                    field: "GSM",
                    title: "Gsm"
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
        var url = `/api/live-product-submission/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
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
            checkboxHeader: false,
            uniqueId: 'LPSubmissionID',
            columns: [
                {
                    field: "SubmissionDate",
                    title: "Submission Date",
                    width: 100,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="date" class="form-control input-sm" style="padding-right: 24px;">'
                    }
                },
                {
                    field: "SubmitedTo",
                    title: "Submited To",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: 'select2',
                        title: 'Select Knitting Type',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: FirmConceptMaster.SubmitedToList,
                        select2: { width: 200, placeholder: 'Select User', allowClear: true }
                    }
                },
                {
                    field: "ReferenceName",
                    title: "Reference Name",
                    width: 120,
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" min="0" style="padding-right: 24px;">'
                    }
                },
                {
                    field: "FormID",
                    title: "Form",
                    width: 100,
                    editable: {
                        type: 'select2',
                        title: 'Select Knitting Type',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: FirmConceptMaster.FormList,
                        select2: { width: 200, placeholder: 'Select Form', allowClear: true }
                    }
                },
                {
                    field: "QtyInPcs",
                    title: "Qty Pcs",
                    width: 80
                },
                {
                    field: "QtyinKG",
                    title: "Qty KG",
                    width: 80
                },
                {
                    field: "SubmitedQtyInPcs",
                    title: "Submited Qty Pcs",
                    width: 80,
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer!';
                            }
                        }
                    }
                },
                {
                    field: "SubmitedQtyinKG",
                    title: "Submited Qty KG",
                    width: 80,
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer!';
                            }
                        }
                    }
                },
                {
                    field: "SelectionStatus",
                    title: "Selection Status",
                    showSelectTitle: true,
                    visible: isSelectionPage,
                    formatter: function (value, row, index, field) {
                        var checked = value ? "checked" : "";
                        return `<input class="form-check-input selectionStatus" type="checkbox" ${checked} />`;
                    },
                    events: {
                        'click .selectionStatus': function (e, value, row, index) {
                            row.SelectionStatus = e.currentTarget.checked;
                            $tblChildEl.bootstrapTable('updateByUniqueId', { id: row.Id, row: row });
                        }
                    }
                },
                {
                    field: "SelectionDate",
                    title: "Selection Date",
                    width: 100,
                    visible: isSelectionPage,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="date" class="form-control input-sm" style="padding-right: 24px;">'
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
        $formEl.find("#FCID").val(-1111);
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
        axios.get(`/api/live-product-submission/new/${newId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                FirmConceptMaster = response.data;
                FirmConceptMaster.TrialDate = formatDateToDefault(FirmConceptMaster.TrialDate);
                FirmConceptMaster.ConceptDate = formatDateToDefault(FirmConceptMaster.ConceptDate);
                initChildTable();
                setFormData($formEl, FirmConceptMaster);
                $tblChildEl.bootstrapTable("load", FirmConceptMaster.LiveProductSubmissions);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/live-product-submission/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                FirmConceptMaster = response.data;
                FirmConceptMaster.TrialDate = formatDateToDefault(FirmConceptMaster.TrialDate);
                FirmConceptMaster.ConceptDate = formatDateToDefault(FirmConceptMaster.ConceptDate);
                initChildTable();
                setFormData($formEl, FirmConceptMaster);
                $tblChildEl.bootstrapTable("load", FirmConceptMaster.LiveProductSubmissions);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {

        var data = FirmConceptMaster;
        axios.post("/api/live-product-submission/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

})();
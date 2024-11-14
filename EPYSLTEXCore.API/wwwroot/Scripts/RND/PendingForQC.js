(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $tblYarnChildEl, $formEl;
    var filterBy = {};
    var status = statusConstants.PENDING;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var isEditable = true;
    var JobCard;

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
        $tblYarnChildEl = $("#tblYarnChild" + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        initMasterTable();
        getMasterTableData();

        $formEl.find("#ProductionDate").datepicker({
            todayHighlight: true,
            autoclose: true
        });
        $formEl.find("#PRollQty").keyup(function (event) {
            if (event.keyCode === 13) {
                $formEl.find("#btnAddItem").click();
            }
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            initMasterTable();
            getMasterTableData();
        });
        $toolbarEl.find("#btnCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            initMasterTable();
            getMasterTableData();
        });

        $formEl.find("#btnAddItem").on("click", function (e) {
            e.preventDefault();
            addNewItem();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnCancel").on("click", backToList);
    });

    function addNewItem() {
        if ($formEl.find("#PShiftId").val() == null) {
            toastr.warning("Select Shift", "Required");
        }
        else if ($formEl.find("#POperatorId").val() == null) {
            toastr.warning("Select Operator", "Required");
        }
        else if ($formEl.find("#PRollQty").val() == '' || $formEl.find("#PRollQty").val() <= 0) {
            toastr.warning("Input valid quantity", "Required");
        }
        else {
            var newChildItem = {
                Id: getMaxIdForArray(JobCard.Childs, "Id"),
                Width: $formEl.find("#FWidth").val(),
                RollQty: $formEl.find("#PRollQty").val(),
                ShiftId: $formEl.find("#PShiftId").val(),
                OperatorId: $formEl.find("#POperatorId").val(),
                EntityState: 4
            };
            JobCard.Childs.push(newChildItem);
            $tblYarnChildEl.bootstrapTable('load', JobCard.Childs);
            $formEl.find("#PRollQty").val('');
            $formEl.find("#PRollQty").focus();
        }
        
    }

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
                    visible: status == statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return [
                            '<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="New">',
                            '<i class="fa fa-plus" aria-hidden="true"></i>',
                            '</a>'
                        ].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            debugger;
                            getNew(row.KJobCardMasterId);
                        }
                    }
                },
                {
                    field: "view",
                    align: "center",
                    width: 50,
                    visible: status !== statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return [
                            '<a class="btn btn-xs btn-default view" href="javascript:void(0)" title="View">',
                            '<i class="fa fa-eye" aria-hidden="true"></i>',
                            '</a>'
                        ].join(' ');
                    },
                    events: {
                        'click .view': function (e, value, row, index) {
                            e.preventDefault();
                            if (row) {
                                HoldOn.open({
                                    theme: "sk-circle"
                                });
                                getDetails(row.Id);
                            }
                        }
                    }
                },
                {
                    field: "JobCardNo",
                    title: "Job Card No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                }, {
                    field: "JobCardDate",
                    title: "Job Card Date",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    formatter: function (value, row, index, field) {
                        return formatDateToMMDDYYYY(value);
                    }
                }, {
                    field: "EWO",
                    title: "EWO",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
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
                        return formatDateToMMDDYYYY(value);
                    }
                },
                {
                    field: "BookingQty",
                    title: "Booking Qty",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "JobCardQty",
                    title: "Job Card Qty",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "MachineType",
                    title: "Machine Type",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
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
        var url = "/api/rnd-pending-for-qc/list?status=" + status + "&" + queryParams;
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
            uniqueId: 'Id',
            editable: isEditable,
            columns: [
                {
                    field: "ColorId",
                    title: "Color Name",
                    align: 'center',
                    editable: {
                        type: 'select2',
                        title: 'Select Color Name',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: JobCard.ColorList,
                        select2: { width: 200, placeholder: 'Select Color Name', alloclear: true }
                    }
                },
                {
                    field: "ColorCode",
                    title: "Color Code",
                    align: 'center',
                    editable: {
                        type: "text",
                        showButtons: false,
                        tpl: '<input type="text" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        
                    }
                }
            ]
        });
    }
    function initYarnChildTable() {
        $tblYarnChildEl.bootstrapTable("destroy");
        $tblYarnChildEl.bootstrapTable({
            uniqueId: 'Id',
            editable: isEditable,
            checkboxHeader: false,
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
                            $tblYarnChildEl.bootstrapTable('remove', { field: 'Id', values: [row.Id] });
                        },
                    }
                },
                {
                    field: "Width",
                    title: "Width",
                   
                },
                
                {
                    field: "RollQty",
                    title: "Roll Qty",
                    align: 'center',
                    editable: {
                        type: "text",
                        showButtons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                        }
                    }
                },
                {
                    field: "ShiftId",
                    title: "Shift",
                    align: 'center',
                    editable: {
                        type: 'select2',
                        title: 'Select Shift',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: JobCard.ShiftList,
                        select2: { width: 200, placeholder: 'Select Shift', allowclear: true }
                    }
                },
                {
                    field: "OperatorId",
                    title: "Operator",
                    align: 'center',
                    editable: {
                        type: 'select2',
                        title: 'Select Operator',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: JobCard.OperatorList,
                        select2: { width: 200, placeholder: 'Select Operator', allowclear: true }
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

    function getNew(KJobCardMasterId) {
        var url = "/api/rnd-pending-for-qc/new/" + KJobCardMasterId;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                JobCard = response.data;
                JobCard.ProductionDate = formatDateToMMDDYYYY(JobCard.ProductionDate);
                $formEl.find("#KnittingTypeId").prop("disabled", true);
                $formEl.find("#ConstructionId").prop("disabled", true);
                $formEl.find("#CompositionId").prop("disabled", true);
                $formEl.find("#GSMId").prop("disabled", true);
                setFormData($formEl, JobCard);
                initChildTable();
                initYarnChildTable();
                
                $tblChildEl.bootstrapTable("load", JobCard.ChildColors);
                $tblChildEl.bootstrapTable('hideLoading');
                $formEl.find("#ConceptId").val(JobCard.ConceptId);
                $formEl.find("#KJobCardMasterId").val(JobCard.KJobCardMasterId);
                $formEl.find("#FWidth").val(JobCard.FWidth);
                $("#PRollQty").focus();

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getDetails(id) {
        axios.get(`/api/rnd-pending-for-qc/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                Concept = response.data;
                JobCard.ConceptDate = formatDateToMMDDYYYY(JobCard.ConceptDate);
                JobCard.ReqDate = formatDateToMMDDYYYY(JobCard.ReqDate);
                setFormData($formEl, Concept);
                initChildTable();
                initYarnChildTable();
                $tblChildEl.bootstrapTable("load", JobCard.ChildColors);
                $tblChildEl.bootstrapTable('hideLoading');
                $tblYarnChildEl.bootstrapTable("load", JobCard.Childs);
                $tblYarnChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        if ($formEl.find("#ProdComplete").is(':checked') == true)
            data.ProdComplete = true;
        data["Childs"] = JobCard.Childs;
        if (JobCard.Childs.length == 0) {
            toastr.error("At least one roll item required!", "Error");
        }
        else {
            axios.post("/api/rnd-pending-for-qc/save", data)
                .then(function () {
                    toastr.success("Saved successfully.");
                    backToList();
                })
                .catch(function (error) {
                    toastr.error(error.response.data.Message);
                });
        }

    }
})();
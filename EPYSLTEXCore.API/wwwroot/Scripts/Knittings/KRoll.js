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

    var KJobCard;

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

        $formEl.find("#btnNewItem").on("click", function (e) {
            e.preventDefault();
            var operator = $formEl.find('#OperatorID').select2('data')[0];
            var shift = $formEl.find('#ShiftID').select2('data')[0];
            var newChildItem = {
                Id: getMaxIdForArray(KJobCard.KRolls, "Id"),
                RollNo: 0,
                ProductionDate: formatDateToDefault(new Date()),
                KJobCardMasterID: KJobCard.Id,
                IsSubContact: KJobCard.IsSubContact,
                ContactID: KJobCard.ContactID,
                BookingID: KJobCard.BookingID,
                ExportOrderID: KJobCard.ExportOrderID,
                BuyerID: KJobCard.BuyerID,
                BuyerTeamID: KJobCard.BuyerTeamID,
                SubGroupID: KJobCard.SubGroupID,
                ItemMasterID: KJobCard.ItemMasterID,
                UnitID: KJobCard.UnitID,
                KJobCardQty: KJobCard.KJobCardQty,
                OperatorID: operator.id,
                Operator: operator.text,
                ShiftID: shift.id,
                Shift: shift.text,
                Width: 0,
                RollWeight: $formEl.find('#RollWeight').val(),
                Remarks: ''
            };

            KJobCard.KRolls.push(newChildItem);
            $tblChildEl.bootstrapTable('load', KJobCard.KRolls);
        });
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
                            return `<a class="btn btn-xs btn-default add" href="javascript:void(0)" title="New Job Card">
                                        <i class="fa fa-plus" aria-hidden="true"></i>
                                    </a>`;
                        }
                        else {
                            return `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Job Card">
                                        <i class="fa fa-edit" aria-hidden="true"></i>
                                    </a>`;
                        }
                    },
                    events: {
                        'click .add': function (e, value, row, index) {
                            e.preventDefault();
                            getNew(row.Id);
                        },
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.Id);
                        }
                    }
                },
                {
                    field: "KJobCardNo",
                    title: "Job Card No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "KJobCardDate",
                    title: "Job Card Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "BookingNo",
                    title: "Booking No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BookingDate",
                    title: "Booking Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "SubGroup",
                    title: "Sub Group",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "MachineGauge",
                    title: "Machine Gauge"
                },
                {
                    field: "MachineDia",
                    title: "Machine Dia"
                },
                {
                    field: "EWONo",
                    title: "EWO No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Buyer",
                    title: "Buyer",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-180' } }
                },
                {
                    field: "TechnicalName",
                    title: "Technical Name",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "FabricComposition",
                    title: "Composition",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-200' } }
                },
                {
                    field: "ColorName",
                    title: "Color",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "FabricGsm",
                    title: "Gsm"
                },
                {
                    field: "FabricWidth",
                    title: "Width"
                },
                {
                    field: "DyeingType",
                    title: "Dyeing Type",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "KnittingType",
                    title: "Knitting Type",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BookingQty",
                    title: "Booking Qty"
                },
                {
                    field: "KJobCardQty",
                    title: "Total Job Qty"
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
        var url = `/api/knitting-roll/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
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
            columns: [
                {
                    field: "RollNo",
                    title: "Roll No",
                    width: 100,
                    formatter: function (value, row, index, field) {
                        return (value == 0) ? "**<< NEW >>**" : value;
                    }
                },
                {
                    field: "ProductionDate",
                    title: "Production Date",
                    width: 100,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "Shift",
                    title: "Shift",
                    width: 100
                },
                {
                    field: "Operator",
                    title: "Operator",
                    width: 100
                },
                {
                    field: "Width",
                    title: "Width",
                    width: 100
                },
                {
                    field: "RollWeight",
                    title: "Roll Weight",
                    width: 100
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

    function getNew(KJobCardID) {
        axios.get(`/api/knitting-roll/new/${KJobCardID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                KJobCard = response.data;
                KJobCard.KJobCardDate = formatDateToDefault(KJobCard.KJobCardDate);
                KJobCard.BookingDate = formatDateToDefault(KJobCard.BookingDate);
                initChildTable();
                setFormData($formEl, KJobCard);
                $tblChildEl.bootstrapTable("load", KJobCard.KRolls);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/knitting-roll/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                KJobCard = response.data;
                KJobCard.KJobCardDate = formatDateToDefault(KJobCard.KJobCardDate);
                KJobCard.BookingDate = formatDateToDefault(KJobCard.BookingDate);
                initChildTable();
                setFormData($formEl, KJobCard);
                $tblChildEl.bootstrapTable("load", KJobCard.KRolls);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save(invokedBy, isApprove = false) {
        var data = KJobCard;// formDataToJson($formEl.serializeArray());
        //data["KRolls"] = KJobCard.KRolls;
        //data.Approve = isApprove;

        axios.post("/api/knitting-roll/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }


})();
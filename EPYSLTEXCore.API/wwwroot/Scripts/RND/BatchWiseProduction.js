(function () {
    var menuId, pageName, menuParam;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $tblYarnChildEl, $formEl, $tblItemChildE1, tblItemChildId;
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
    var Batch;
    var ConceptID = 0;
    var isDP = false, isDC = false;
    var _isRework = false;

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");
        if (!menuParam) menuParam = localStorage.getItem("menuParam");
        if (menuParam == "DP") isDP = true;
        else if (menuParam == "DC") isDC = true;

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        $tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        tblItemChildId = "#tblItemChild" + pageId;
        //initMasterTable();
        //getMasterTableData();


        //$formEl.find('#datetimepicker1').datetimepicker();

        $toolbarEl.find(".btnToolbar").hide();
        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            checkBoxBSOperations(null, true);
            initMasterTable();
            getMasterTableData();
            //$formEl.find(".divMachineNo").hide();
            //$formEl.find(".divProductionUnit").hide();
        });
        $toolbarEl.find("#btnUnloadingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.AWAITING_PROPOSE;
            checkBoxBSOperations(null, true);
            initMasterTable();
            getMasterTableData();
            //$formEl.find(".divMachineNo").show();
            //$formEl.find(".divProductionUnit").show();
        });
        $toolbarEl.find("#btnCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            checkBoxBSOperations(null, true);
            initMasterTable();
            getMasterTableData();
            //$formEl.find(".divMachineNo").show();
            //$formEl.find(".divProductionUnit").show();
        });
        $toolbarEl.find("#btnPendingConfirmation").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING_CONFIRMATION;
            checkBoxBSOperations(null, false);
            $formEl.find("#btnSave").show();
            initMasterTable();
            getMasterTableData();
            //$formEl.find(".divMachineNo").show();
            //$formEl.find(".divProductionUnit").show();
        });
        $toolbarEl.find("#btnFailList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.FAIL;
            checkBoxBSOperations(null, true);
            $formEl.find("#btnSave").hide();
            initMasterTable();
            getMasterTableData();
            //$formEl.find(".divMachineNo").show();
            //$formEl.find(".divProductionUnit").show();
        });
        $toolbarEl.find("#btnConfirmList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.CONFIRM;
            checkBoxBSOperations(null, true);
            $formEl.find("#btnSave").hide();
            initMasterTable();
            getMasterTableData();
            //$formEl.find(".divMachineNo").show();
            //$formEl.find(".divProductionUnit").show();
        });
        $toolbarEl.find("#btnReworkList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.REWORK;
            checkBoxBSOperations(null, true);
            $formEl.find("#btnSave").hide();
            initMasterTable();
            getMasterTableData();
            //$formEl.find(".divMachineNo").show();
            //$formEl.find(".divProductionUnit").show();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find(".divBatchStatus").hide();
        $formEl.find(".chkBatchStatus").change(function () {
            $formEl.find(".chkBatchStatus").prop('checked', false);
            $(this).prop('checked', true);
            hideShowdivForNewBatch();
            hideShowIsNewRecipe();
        });
        $formEl.find(".chkIsNewBatch").change(function () {
            $formEl.find(".chkIsNewBatch").prop('checked', false);
            $(this).prop('checked', true);
            hideShowIsNewRecipe();
        });
        $formEl.find(".chkIsNewRecipe").change(function () {
            $formEl.find(".chkIsNewRecipe").prop('checked', false);
            $(this).prop('checked', true);
        });

        if (isDP) {
            $toolbarEl.find("#btnList,#btnUnloadingList,#btnCompleteList").show();
            $toolbarEl.find("#btnList").click();
        } else if (isDC) {
            $toolbarEl.find("#btnPendingConfirmation,#btnConfirmList,#btnFailList,#btnReworkList").show();
            $toolbarEl.find("#btnPendingConfirmation").click();
        }
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
                        if (status == statusConstants.PENDING) {
                            return [
                                '<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="New">',
                                '<i class="fa fa-plus" aria-hidden="true"></i>',
                                '</a>'
                            ].join(' ');
                        } else {
                            return [
                                '<a class="btn btn-xs btn-default view" href="javascript:void(0)" title="View">',
                                '<i class="fa fa-eye" aria-hidden="true"></i>',
                                '</a>'
                            ].join(' ');
                        }

                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            if (row.Status == 'Deactive') {
                                toastr.error("Select active dyeing production");
                                return false;
                            }
                            ConceptID = row.ConceptID;
                            _isRework = row.Status == "Rework" ? true : false;
                            getNew(row.DBatchID);
                        },
                        'click .view': function (e, value, row, index) {
                            e.preventDefault();
                            if (row) {
                                if (row.Status == 'Deactive') {
                                    toastr.error("Select active dyeing production");
                                    return false;
                                }
                                HoldOn.open({
                                    theme: "sk-circle"
                                });
                                _isRework = false;
                                getDetails(row.DBatchID);
                            }
                        }
                    }
                },
                //{
                //    field: "DBatchID",
                //    title: "DBatchID",
                //    filterControl: "input"
                //},
                {
                    field: "Status",
                    title: "Status",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: isDP == true
                },
                {
                    field: "DBatchNo",
                    title: "D. Batch No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "DBatchDate",
                    title: "D. Batch Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "ConceptNo",
                    title: "Concept No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BuyerName",
                    title: "Buyer",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BuyerTeamName",
                    title: "Buyer Team",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ProductionDate",
                    title: "Production Date",
                    visible: status !== statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "ShiftName",
                    title: "Loading Shift",
                    visible: status == statusConstants.AWAITING_PROPOSE || status == statusConstants.COMPLETED,
                },
                {
                    field: "UnloadShiftName",
                    title: "Unloading Shift",
                    visible: status == statusConstants.APPROVED || status == statusConstants.COMPLETED,
                },
                {
                    field: "OperatorName",
                    title: "Loading Operator",
                    visible: status == statusConstants.AWAITING_PROPOSE || status == statusConstants.COMPLETED,
                },
                {
                    field: "UnloadOperatorName",
                    title: "Unloading Operator",
                    visible: status == statusConstants.APPROVED || status == statusConstants.COMPLETED,
                },
                {
                    field: "BatchStartTime",
                    title: "Loading",
                    visible: status == statusConstants.AWAITING_PROPOSE || status == statusConstants.COMPLETED,
                    formatter: function (value, row, index, field) {
                        return formatDateToHHMMADMMMYYYY(value);
                    }
                },
                {
                    field: "BatchEndTime",
                    title: "Unloading",
                    visible: status == statusConstants.APPROVED || status == statusConstants.COMPLETED,
                    formatter: function (value, row, index, field) {
                        return formatDateToHHMMADMMMYYYY(value);
                    }
                },
                {
                    field: "RecipeNo",
                    title: "Recipe No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "RecipeForName",
                    title: "Recipe For",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ColorName",
                    title: "Color",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BatchWeightKG",
                    title: "Batch Weight(Kg)",
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
        var url = "/api/batch-wise-production/list?gridType=bootstrap-table&status=" + status + "&" + queryParams;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function initChildTable(ItemStatus1) {
        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            uniqueId: 'DBBMID',
            editable: isEditable,
            columns: [
                {
                    field: "BatchNo",
                    title: "Batch No"
                },
                {
                    field: "BatchUseQtyKG",
                    title: "Batch Qty(Kg)"
                },
                {
                    field: "BatchUseQtyPcs",
                    visible: ItemStatus1 != 1,
                    title: "Batch Qty (Pcs)"
                }
            ]
        });
    }
    function initItemChildTable(data) {
        if ($tblItemChildE1) {
            $tblItemChildE1.destroy();
            $(tblItemChildId).html("");
        }

        ej.base.enableRipple(true);
        $tblItemChildE1 = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            allowDeleting: false,
            editSettings: { allowAdding: false, allowEditing: false, allowDeleting: false },
            columns: [
                { headerText: '', width: 8 },
                { field: 'BatchNo', headerText: 'Batch No', width: 25, allowEditing: false },
                { field: 'KnittingType', headerText: 'Machine Type', width: 25, allowEditing: false },
                { field: 'TechnicalName', headerText: 'Technical Name', width: 30, allowEditing: false },
                { field: 'FabricComposition', headerText: 'Composition', width: 45, allowEditing: false },
                { field: 'FabricGsm', headerText: 'Fabric Gsm', width: 15, allowEditing: false },
                { field: 'ItemSubGroup', headerText: 'Sub Group', width: 25, allowEditing: false },
                { field: 'Qty', headerText: 'Qty (Kg)', width: 15 },
                { field: 'QtyPcs', headerText: 'Qty (Pcs)', width: 15 }
            ],
            childGrid: {
                queryString: 'DBIID',
                allowResizing: true,
                //toolbar: ['Add', 'Edit', 'Delete', 'Update', 'Cancel'],
                editSettings: { allowEditing: false, allowAdding: false, allowDeleting: false },
                columns: [
                    { headerText: 'Action', width: 10 },
                    { field: 'RollNo', headerText: 'Roll No', textAlign: 'Center', width: 20, allowEditing: false },
                    { field: 'RollQty', headerText: 'Roll Qty (Kg)', width: 20 },
                    { field: 'RollQtyPcs', headerText: 'Roll Qty (Pcs)', width: 20 }
                ],
                //actionComplete: function (e) {
                //    alert($tblItemChildE1.getSelectedRowIndexes()[0]);
                //},
                load: loadFirstLevelChildGrid
            }
        });
        $tblItemChildE1.refreshColumns;
        $tblItemChildE1.appendTo(tblItemChildId);
    }
    function loadFirstLevelChildGrid() {
        this.dataSource = this.parentDetails.parentRowData.DyeingBatchItemRolls;
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
        $formEl.find("#DBatchID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(batchId) {
        var url = "/api/batch-wise-production/new/" + batchId;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                Batch = response.data;
                Batch.DBatchDate = formatDateToDefault(Batch.DBatchDate);
                Batch.ProductionDate = formatDateToDefault(new Date());
                Batch.BatchStartTime = new Date(new Date().setUTCHours(new Date().getUTCHours() + 6)).toJSON().slice(0, 16);
                Batch.PlanBatchStartTime = formatDateToDefault(Batch.PlanBatchStartTime);
                Batch.PlanBatchEndTime = formatDateToDefault(Batch.PlanBatchEndTime);
                setFormData($formEl, Batch);

                initChildTable(Batch.ItemStatus1);
                initItemChildTable(Batch.DyeingBatchItems);
                $tblChildEl.bootstrapTable("load", Batch.DyeingBatchWithBatchMasters);
                $tblChildEl.bootstrapTable('hideLoading');
                IsElementHide();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getDetails(id) {
        axios.get(`/api/batch-wise-production/${id}/${status}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                Batch = response.data;
                Batch.DBatchDate = formatDateToDefault(Batch.DBatchDate);
                Batch.ProductionDate = formatDateToDefault(Batch.ProductionDate);
                if (!Batch.BatchEndTime) {
                Batch.BatchEndTime = new Date(new Date().setUTCHours(new Date().getUTCHours() + 6)).toJSON().slice(0, 16);
                }
                Batch.PlanBatchStartTime = formatDateToDefault(Batch.PlanBatchStartTime);
                Batch.PlanBatchEndTime = formatDateToDefault(Batch.PlanBatchEndTime);
                setFormData($formEl, Batch);


                var isDisable = status == statusConstants.PENDING_CONFIRMATION ? false : true;
                checkBoxBSOperations(Batch, isDisable);

                initChildTable(Batch.ItemStatus1);
                initItemChildTable(Batch.DyeingBatchItems);
                $tblChildEl.bootstrapTable("load", Batch.DyeingBatchWithBatchMasters);
                $tblChildEl.bootstrapTable('hideLoading');

                IsElementHide();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());

        data.BatchStatus = $formEl.find('input[name="chkBatchStatus"]:checked').val();
        data.BatchStatus = typeof data.BatchStatus === "undefined" ? 0 : data.BatchStatus;
        if (status == statusConstants.PENDING_CONFIRMATION && data.BatchStatus == 0) {
            toastr.error("Select batch status.");
            return false;
        }

        data.IsNewBatch = $formEl.find('input[name="chkIsNewBatch"]:checked').val();
        data.IsNewBatch = typeof data.IsNewBatch === "undefined" ? -1 : data.IsNewBatch;
        data.IsNewBatch = data.IsNewBatch == "1" ? true : false;

        data.IsNewRecipe = $formEl.find('input[name="chkIsNewRecipe"]:checked').val();
        data.IsNewRecipe = typeof data.IsNewRecipe === "undefined" ? -1 : data.IsNewRecipe;
        data.IsNewRecipe = data.IsNewRecipe == "1" ? true : false;

        data.IsRework = _isRework;

        if (status == statusConstants.PENDING_CONFIRMATION && data.BatchStatus == 3 && data.IsNewBatch == -1) {
            toastr.error("Select new or existing batch.");
            return false;
        }
        if (status == statusConstants.PENDING_CONFIRMATION && data.BatchStatus == 3 && data.IsNewBatch == 1 && data.IsNewRecipe == -1) {
            toastr.error("Select new or existing recipe.");
            return false;
        }

        //data["ChildColors"] = Batch.ChildColors;

        if (status == statusConstants.PENDING) {
            initializeValidation($formEl, validationConstraints_Loading);
            if (!isValidForm($formEl, validationConstraints_Loading)) return toastr.error("Please correct all validation errors!");
            else hideValidationErrors($formEl);
        } else {
            initializeValidation($formEl, validationConstraints_Unloading);
            if (!isValidForm($formEl, validationConstraints_Unloading)) return toastr.error("Please correct all validation errors!");
            else hideValidationErrors($formEl);
        }
        data["ConceptID"] = ConceptID;

        var saveAPI = "/api/batch-wise-production/save";
        if (isDC) saveAPI = "/api/batch-wise-production/save/rework";
        axios.post(saveAPI, data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    var validationConstraints_Loading = {
        ProductionDate: {
            presence: true
        },
        BatchStartTime: {
            presence: true
        },
        ShiftID: {
            presence: true
        },
        OperatorID: {
            presence: true
        }
    }

    var validationConstraints_Unloading = {
        ProductionDate: {
            presence: true
        },
        BatchEndTime: {
            presence: true
        },
        UnloadShiftID: {
            presence: true
        },
        UnloadOperatorID: {
            presence: true
        }
    }
    function IsElementHide() {
        if (status == statusConstants.PENDING) {
            $formEl.find("#divBatchStartTime").fadeIn();
            $formEl.find("#divBatchEndTime").fadeOut();
            $formEl.find("#divShiftID").fadeIn();
            $formEl.find("#divUnloadShiftID").fadeOut();
            $formEl.find("#divOperatorID").fadeIn();
            $formEl.find("#divUnloadOperatorID").fadeOut();
            $formEl.find("#btnSave").fadeIn();
        }
        else if (status == statusConstants.AWAITING_PROPOSE) {
            $formEl.find("#divBatchStartTime").fadeOut();
            $formEl.find("#divBatchEndTime").fadeIn();
            $formEl.find("#divShiftID").fadeOut();
            $formEl.find("#divUnloadShiftID").fadeIn();
            $formEl.find("#divOperatorID").fadeOut();
            $formEl.find("#divUnloadOperatorID").fadeIn();
            $formEl.find("#btnSave").fadeIn();
        }
        else {
            $formEl.find("#divBatchStartTime").fadeIn();
            $formEl.find("#divBatchEndTime").fadeIn();
            $formEl.find("#divShiftID").fadeIn();
            $formEl.find("#divUnloadShiftID").fadeIn();
            $formEl.find("#divOperatorID").fadeIn();
            $formEl.find("#divUnloadOperatorID").fadeIn();
            /*$formEl.find("#btnSave").fadeOut();*/
        }
    }
    function hideShowdivForNewBatch() {
        $formEl.find(".divForNewBatch").hide();

        var batchStatus = $formEl.find('input[name="chkBatchStatus"]:checked').val();
        batchStatus = typeof batchStatus === "undefined" ? 0 : batchStatus;

        if (batchStatus == 3) { //Rework
            $formEl.find(".divForNewBatch").show();
            $formEl.find(".divForNewRecipe").hide();
            $formEl.find(".chkIsNewBatch").prop('disabled', false);
            $formEl.find(".chkIsNewRecipe").prop('disabled', false);
        } else {
            $formEl.find(".chkIsNewBatch").prop('disabled', true);
            $formEl.find(".chkIsNewRecipe").prop('disabled', true);
        }
        $formEl.find(".chkIsNewBatch").prop('checked', false);
        $formEl.find(".chkIsNewRecipe").prop('checked', false);
    }
    function hideShowIsNewRecipe() {

        var isNewBatch = $formEl.find('input[name="chkIsNewBatch"]:checked').val();
        isNewBatch = typeof isNewBatch === "undefined" ? -1 : isNewBatch;
        $formEl.find(".chkIsNewRecipe").prop('checked', false);

        if (isNewBatch == -1) {
            $formEl.find(".divForNewRecipe").hide();
            $formEl.find(".chkIsNewRecipe").prop('disabled', true);
        }
        else if (isNewBatch == 1) { //Rework
            $formEl.find(".divForNewRecipe").show();
            $formEl.find(".spnExistingRecipe").show();
            $formEl.find(".spnNewRecipe").show();
            $formEl.find(".chkIsNewRecipe").prop('disabled', false);
        } else {
            $formEl.find(".divForNewRecipe").show();
            $formEl.find(".spnExistingRecipe").hide();
            $formEl.find(".spnNewRecipe").show();

            $formEl.find(".chkExistingRecipe").prop('disabled', true);
            $formEl.find(".chkNewRecipe").prop('checked', true);
        }
    }

    function checkBoxBSOperations(batch, isDisable) {
        checkBoxBSAllHideShow(batch, isDisable);
        checkBoxBSBatchOperation(batch, isDisable);
        checkBoxBSRecipeOperation(batch, isDisable);
    }

    function checkBoxBSAllHideShow(batch, isDisable) {
        $formEl.find(".divBatchStatus").hide();
        $formEl.find(".divForNewBatch").hide();
        $formEl.find(".divForNewRecipe").hide();
        if (isDC) {
            $formEl.find(".divBatchStatus").show();
        }
        $formEl.find(".chkBatchStatus").prop('checked', false);
        if (batch != null) {
            $formEl.find(".chkBatchStatus[value=" + batch.BatchStatus + "]").prop("checked", true);
        }
        $formEl.find(".chkBatchStatus").prop('disabled', isDisable);
    }
    function checkBoxBSBatchOperation(batch, isDisable) {
        var batchStatus = $formEl.find('input[name="chkBatchStatus"]:checked').val();
        batchStatus = typeof batchStatus === "undefined" ? -1 : batchStatus;
        if (batchStatus == 3 && isDC) {
            $formEl.find(".divForNewBatch").show();
        } else {
            $formEl.find(".divForNewBatch").hide();
        }
        $formEl.find(".chkIsNewBatch").prop('checked', false);
        if (batch != null) {
            var isNewBatch = batch.IsNewBatch ? 1 : 0;
            $formEl.find(".chkIsNewBatch[value=" + isNewBatch + "]").prop("checked", true);
        }
        $formEl.find(".chkIsNewBatch").prop('disabled', isDisable);
    }
    function checkBoxBSRecipeOperation(batch, isDisable) {
        var isNewBatch = $formEl.find('input[name="chkIsNewBatch"]:checked').val();
        isNewBatch = typeof isNewBatch === "undefined" ? -1 : isNewBatch;
        isNewBatch = isNewBatch == "1" ? true : false;
        if (isNewBatch && isDC) {
            $formEl.find(".divForNewRecipe").show();
        }
        else {
            $formEl.find(".divForNewRecipe").hide();
        }
        $formEl.find(".chkIsNewRecipe").prop('checked', false);
        if (batch != null) {

            if (!isNewBatch && batch.IsNewRecipe && isDC) {
                $formEl.find(".divForNewRecipe").show();
                $formEl.find(".spnNewRecipe").show();
                $formEl.find(".spnExistingRecipe").hide();
            } else if (isNewBatch && isDC) {
                $formEl.find(".divForNewRecipe").show();
                $formEl.find(".spnNewRecipe").show();
                $formEl.find(".spnExistingRecipe").show();
            }

            var isNewRecipe = batch.IsNewRecipe ? 1 : 0;
            $formEl.find(".chkIsNewRecipe[value=" + isNewRecipe + "]").prop("checked", true);
        }
        $formEl.find(".chkIsNewRecipe").prop('disabled', isDisable);
    }
})();
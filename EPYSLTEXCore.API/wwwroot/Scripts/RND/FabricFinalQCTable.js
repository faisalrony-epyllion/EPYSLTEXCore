(function () {
    var menuId, pageName, pageId;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, tblMasterId, $tblMasterEl, $tblChildEl, $tblItemChildE1, tblItemChildId, $formEl;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        filter: '',
        sort: '',
        order: ''
    }
    var status;
    var masterData;
    var validationConstraints = {
        DBatchDate: {
            presence: true
        },
        BatchWeightKG: {
            presence: true,
            numericality: {
                onlyInteger: true,
                greaterThan: 0,
                //lessThanOrEqualTo: 50
            }
        },
        MachineLoading: {
            presence: true,
            numericality: {
                greaterThan: 0
            }
        },
        DyeingNozzleQty: {
            presence: true,
            numericality: {
                onlyInteger: true,
                greaterThan: 0
            }
        },
        DMNo: {
            presence: true
        }
    }

    $(function () {

        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblItemChildId = "#tblItemChild" + pageId;
        tblPostChildId = "#tblPostChild" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        status = statusConstants.PENDING;
        initMasterTable();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            initMasterTable();
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            initMasterTable();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnCancel").on("click", backToList);

        //$formEl.find("#btnAddBatch").on("click", function (e) {
        //    e.preventDefault();
        //    var batchIds = "";
        //    for (var i = 0; i < $tblChildEl.getCurrentViewRecords().length; i++) batchIds += $tblChildEl.getCurrentViewRecords()[i].BatchID + ","
        //    batchIds = batchIds.substr(0, batchIds.length - 1);

        //    var finder = new commonFinder({
        //        title: "Select Batch",
        //        pageId: pageId,
        //        apiEndPoint: `/api/dyeing-batch/batch-list/${batchIds}`,
        //        fields: "BatchNo,BatchDate",
        //        headerTexts: "Batch No, Batch Date",
        //        isMultiselect: true,
        //        primaryKeyColumn: "BatchID",
        //        onMultiselect: function (selectedRecords) {
        //            batchIds = "";
        //            for (var i = 0; i < selectedRecords.length; i++) batchIds += selectedRecords[i].BatchID + ","
        //            batchIds = batchIds.substr(0, batchIds.length - 1);
        //            getSelectedBatchDetails(batchIds);
        //        }
        //    });
        //    finder.showModal();
        //});

        //$formEl.find("#btnAddNozzle").on("click", function (e) {
        //    e.preventDefault();
        //    var finder = new commonFinder({
        //        title: "Select Nozzle",
        //        pageId: pageId,
        //        isMultiselect: false,
        //        modalSize: "modal-md",
        //        primaryKeyColumn: "DMID",
        //        fields: "DyeingMcStatus,DyeingNozzleQty,DyeingMcCapacity,DyeingMcBrand",
        //        headerTexts: "Status,Nozzle,Capacity,Brand",
        //        apiEndPoint: `/api/dyeing-machine/nozzle-list`,
        //        onSelect: function (res) {
        //            finder.hideModal();
        //            $formEl.find("#DyeingNozzleQty").val(res.rowData.DyeingNozzleQty);
        //            $formEl.find("#DyeingMcCapacity").val(res.rowData.DyeingMcCapacity);
        //            $formEl.find("#DMNo").val("");
        //            $formEl.find("#DMID").val(0);
        //        },
        //    });
        //    finder.showModal();
        //});

        //$formEl.find("#btnAddDM").on("click", function (e) {
        //    e.preventDefault();
        //    var finder = new commonFinder({
        //        title: "Select Machine",
        //        pageId: pageId,
        //        apiEndPoint: `/api/dyeing-machine/dyeing-machine-by-nozzle/${$formEl.find("#DyeingNozzleQty").val()}`,
        //        fields: "DyeingMcslNo,Company,DyeingMcStatus",
        //        headerTexts: "Machine No, Unit, Status",
        //        isMultiselect: false,
        //        primaryKeyColumn: "DMID",
        //        onSelect: function (res) {
        //            finder.hideModal();
        //            $formEl.find("#DMNo").val(res.rowData.DyeingMcslNo);
        //            $formEl.find("#DMID").val(res.rowData.DMID);
        //        }
        //    });
        //    finder.showModal();
        // });

        $formEl.find("#btnAddDMWithNozzle").on("click", function (e) {
            e.preventDefault();
            var finder = new commonFinder({
                title: "Select Machine",
                pageId: pageId,
                isMultiselect: false,
                modalSize: "modal-md",
                primaryKeyColumn: "DMID",
                fields: "DyeingMcslNo,DyeingMcStatus,DyeingNozzleQty,DyeingMcCapacity,DyeingMcBrand,Company",
                headerTexts: "M/C SL No,Status,Nozzle,Capacity,Brand,Unit",
                apiEndPoint: `/api/dyeing-machine/nozzle-list`,
                onSelect: function (res) {
                    finder.hideModal();
                    $formEl.find("#DyeingNozzleQty").val(res.rowData.DyeingNozzleQty);
                    $formEl.find("#DyeingMcCapacity").val(res.rowData.DyeingMcCapacity);
                    $formEl.find("#DMNo").val(res.rowData.DyeingMcslNo);
                    $formEl.find("#DMID").val(res.rowData.DMID);
                },
            });
            finder.showModal();
        });
    });

    //function getSelectedBatchDetails(batchIds) {
    //    axios.get(`/api/dyeing-batch/get-batch-details/${batchIds}`)
    //        .then(function (response) {
    //            for (var i = 0; i < response.data.length; i++) {
    //                var batch = { BatchID: response.data[i].BatchID, BatchNo: response.data[i].BatchNo, BatchUseQtyKG: response.data[i].BatchWeightKG, BatchQtyPcs: response.data[i].BatchQtyPcs };
    //                $tblChildEl.getCurrentViewRecords().push(batch);
    //                for (var j = 0; j < response.data[i].DyeingBatchItems.length; j++) {
    //                    $tblItemChildE1.getCurrentViewRecords().push(response.data[i].DyeingBatchItems[j]);
    //                }
    //            }
    //            initItemChildTable($tblItemChildE1.getCurrentViewRecords());
    //            $tblChildEl.refresh();
    //            $tblItemChildE1.refresh();

    //        })
    //        .catch(function (err) {
    //            toastr.error(err.response.data.Message);
    //        });
    //}

    function initMasterTable() {
        var commands = status == statusConstants.PENDING
            ? [{ type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } }]
            : [
                { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
            ];

        var columns = [
            { headerText: 'Actions', commands: commands, width: 15, textAlign: 'Center' },
            { field: 'BatchNo', headerText: 'Batch No', width: 20 },
            //{ field: 'ConceptNo', headerText: 'Concept No', width: 20 },
            //{ field: 'RecipeNo', headerText: 'Recipe No', width: 20 },
            { field: 'KnittingType', headerText: 'Machine Type', width: 25 },
            //{ field: 'TechnicalName', headerText: 'Technical Name', width: 30 },
            //{ field: 'FabricComposition', headerText: 'Composition', width: 45 },
            { field: 'ColorName', headerText: 'Color', width: 15 },
            { field: 'FabricGsm', headerText: 'Fabric GSM', width: 15 },
            { field: 'Qty', headerText: 'Qty (Kg)', width: 15 },
            { field: 'QtyPcs', headerText: 'Qty (Pcs)', width: 15 }
            //{ field: 'RecipeDate', headerText: 'Recipe Date', textAlign: 'Center', width: 20, type: 'date', format: _ch_date_format_1 },
            //{ field: 'BatchWeightKG', headerText: 'Batch Qty (Kg)', width: 10 },
            //{ field: 'BatchQtyPcs', headerText: 'Batch Qty (Pcs)', width: 10 }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/batch-item-req/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        //if (args.commandColumn.type == 'Report') {
        //    var a = document.createElement('a');
        //    a.href = "/reports/InlinePdfView?ReportId=1254&DBatchNo=" + args.rowData.DBatchNo;
        //    a.setAttribute('target', '_blank');
        //    a.click();
        //}
        //else 
        if (args.commandColumn.type == 'Add') {
            getNew(args.rowData.BatchID);
        }
        else if (args.commandColumn.type == 'View') {
            getDetails(args.rowData.BatchID);
        }
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
            editSettings: { allowAdding: false, allowEditing: false, allowDeleting: false },
            columns: [
                // { headerText: '', width: 8, commands: [{ type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }] },
                { field: 'BatchNo', headerText: 'Batch No', width: 25 },
                { field: 'KnittingType', headerText: 'Machine Type', width: 25 },
                { field: 'TechnicalName', headerText: 'Technical Name', width: 30 },
                { field: 'FabricComposition', headerText: 'Composition', width: 45 },
                { field: 'FabricGsm', headerText: 'Fabric Gsm', width: 15 },
                { field: 'Qty', headerText: 'Qty (Kg)', width: 15 },
                { field: 'QtyPcs', headerText: 'Qty (Pcs)', width: 15 }
            ],
            childGrid: {
                queryString: 'DBIID',
                allowResizing: true,
                editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false },
                columns: [
                    { field: 'DBIRollID', isPrimaryKey: true, visible: false },
                    { field: 'RollNo', headerText: 'Roll No', textAlign: 'Center', width: 20, allowEditing: false },
                    { field: 'QCWidth', headerText: 'Width', width: 20 },
                    { field: 'QCGSM', headerText: 'GSM', width: 20 },
                    { field: 'ActualRollQty', allowEditing: false, headerText: 'Actual Roll (Kg)', width: 20 },
                    { field: 'FinishRollQty', headerText: 'Roll Qty (Kg)', width: 20 },
                    { field: 'RemainingRollQty', allowEditing: false, headerText: 'Remaining Roll (Kg)', width: 20 },
                    { field: 'ActualRollQtyPcs', allowEditing: false, headerText: 'Actual Roll (Pcs)', width: 20 },
                    { field: 'FinishRollQtyPcs', headerText: 'Roll Qty (Pcs)', width: 20 },
                    { field: 'RemainingRollQtyPcs', allowEditing: false, headerText: 'Remaining Roll (Pcs)', width: 20 },
                    { field: 'QCPass', headerText: 'QC Pass', displayAsCheckBox: true, editType: "booleanedit", width: 20, textAlign: 'Center' },
                    { field: 'QCFail', headerText: 'QC Fail', displayAsCheckBox: true, editType: "booleanedit", width: 20, textAlign: 'Center' },
                    { field: 'QCHold', headerText: 'QC Hold', displayAsCheckBox: true, editType: "booleanedit", width: 20, textAlign: 'Center' },
                    { field: 'ActiveByName', headerText: 'Action By', width: 30, visible: status == statusConstants.COMPLETED },
                    { field: 'InActiveDate', headerText: 'Action Date', type: 'date', format: _ch_date_format_1, editType: 'datepickeredit', width: 30, textAlign: 'Center', visible: status == statusConstants.COMPLETED }
                ],
                actionBegin: function (args) {
                    if (args.requestType === 'beginEdit') {

                    }
                    else if (args.requestType === "save") {
                        var countChecked = 0,
                            isDisplayMessage = true;
                        if (args.data.QCPass) {
                            countChecked++;
                        }
                        if (args.data.QCFail) {
                            if (countChecked > 0) {
                                args.data.QCFail = false;
                                isDisplayMessage = false;
                                toastr.error("Select Pass or Fail or Hold.");
                            } else {
                                countChecked++;
                            }
                        }
                        if (args.data.QCHold) {
                            if (countChecked > 0) {
                                if (isDisplayMessage) toastr.error("Select Pass or Fail or Hold.");
                                args.data.QCHold = false;
                            } else {
                                countChecked++;
                            }
                        }
                        args.rowData = args.data;
                        args.rowData.RemainingRollQty = (args.rowData.ActualRollQty - args.rowData.FinishRollQty).toFixed(2);
                        args.rowData.RemainingRollQtyPcs = args.rowData.ActualRollQtyPcs - args.rowData.FinishRollQtyPcs;

                        $tblItemChildE1.updateRow(args.rowIndex, args.rowData);
                    }
                },
                load: loadFirstLevelChildGrid
            }
        });
        $tblItemChildE1.refreshColumns;
        $tblItemChildE1.appendTo(tblItemChildId);
    }
    function loadFirstLevelChildGrid() {
        if (masterData.DyeingBatchItemRolls) {
            this.dataSource = masterData.DyeingBatchItemRolls.filter(x => x.ConceptID == this.parentDetails.parentRowData.ConceptID);// this.parentDetails.parentRowData.DyeingBatchItemRolls;
        } else {
            this.dataSource = [];
        }
    }
    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
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

    function getNew(batchID) {
        axios.get(`/api/batch-item-req/new/${batchID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.DBatchDate = formatDateToDefault(masterData.DBatchDate);
                masterData.RecipeDate = formatDateToDefault(masterData.RecipeDate);
                masterData.PlanBatchStartTime = formatDateToDefault(masterData.PlanBatchStartTime);
                masterData.PlanBatchEndTime = formatDateToDefault(masterData.PlanBatchEndTime);
                setFormData($formEl, masterData);
                initItemChildTable(masterData.DyeingBatchItems);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/batch-item-req/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.DBatchDate = formatDateToDefault(masterData.DBatchDate);
                masterData.RecipeDate = formatDateToDefault(masterData.RecipeDate);
                masterData.PlanBatchStartTime = formatDateToDefault(masterData.PlanBatchStartTime);
                masterData.PlanBatchEndTime = formatDateToDefault(masterData.PlanBatchEndTime);
                setFormData($formEl, masterData);
                initItemChildTable(masterData.DyeingBatchItems);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        var dyeingBatchItems = $tblItemChildE1.getCurrentViewRecords();
        var dyeingBatchItemRolls = [];
        dyeingBatchItems.map(x => {
            var rolls = masterData.DyeingBatchItemRolls.filter(y => y.ConceptID == x.ConceptID);
            if (rolls) {
                dyeingBatchItemRolls.push(...rolls);
            }
        });
        axios.post("/api/batch-item-req/save", dyeingBatchItemRolls)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();
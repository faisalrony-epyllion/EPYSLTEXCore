(function () {
    var menuId, pageName, pageId;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, tblMasterId, $tblRollEl, $tblKProductionE1, $tblMasterEl, $tblChildEl, tblChildId, $tblItemChildE1, tblItemChildId, $tblRecipeChildE1, tblRecipeChildId, $formEl;
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
    var isRework = false;
    var selectedMasterItems = [];

    var validationConstraints = {
        YDDBatchDate: {
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
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblItemChildId = "#tblItemChild" + pageId;
        tblRecipeChildId = "#tblRecipeChild" + pageId;
        tblPostChildId = "#tblPostChild" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        tblMachineParamId = $("#tblMachineParam" + pageId);
        $tblRollEl = $("#tblRoll" + pageId);
        $tblKProductionE1 = $("#tblKProduction" + pageId);

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
        $toolbarEl.find("#btnCreate").on("click", createNew);

        $formEl.find("#btnAddBatch").on("click", function (e) {
            e.preventDefault();


            var childs = $tblChildEl.getCurrentViewRecords();
            var batchIds = childs.map(x => x.YDBatchID).join(",");

            var finder = new commonFinder({
                title: "Select Batch",
                pageId: pageId,
                apiEndPoint: `/api/yd-dyeing-batch/batch-list/${batchIds}`,
                fields: "YDBatchNo,YDBatchDate",
                headerTexts: "Batch No,Batch Date",
                widths: "100,80",
                isMultiselect: true,
                primaryKeyColumn: "YDBatchID",
                onMultiselect: function (selectedRecords) {
                    batchIds = "";
                    for (var i = 0; i < selectedRecords.length; i++) batchIds += selectedRecords[i].YDBatchID + ","
                    batchIds = batchIds.substr(0, batchIds.length - 1);
                    getSelectedBatchDetails(batchIds);
                }
            });
            finder.showModal();
        });

        $formEl.find("#btnAddNozzle").on("click", function (e) {
            e.preventDefault();
            var finder = new commonFinder({
                title: "Select Nozzle",
                pageId: pageId,
                isMultiselect: false,
                modalSize: "modal-md",
                primaryKeyColumn: "DMID",
                fields: "DyeingMcStatus,DyeingNozzleQty,DyeingMcCapacity,DyeingMcBrand",
                headerTexts: "Status,Nozzle,Capacity,Brand",
                apiEndPoint: `/api/dyeing-machine/nozzle-list`,
                onSelect: function (res) {
                    finder.hideModal();
                    $formEl.find("#DyeingNozzleQty").val(res.rowData.DyeingNozzleQty);
                    $formEl.find("#DyeingMcCapacity").val(res.rowData.DyeingMcCapacity);
                    $formEl.find("#DMNo").val("");
                    $formEl.find("#DMID").val(0);
                },
            });
            finder.showModal();
        });

        $formEl.find("#btnAddDM").on("click", function (e) {
            e.preventDefault();
            var finder = new commonFinder({
                title: "Select Machine",
                pageId: pageId,
                apiEndPoint: `/api/dyeing-machine/dyeing-machine-by-nozzle/${$formEl.find("#DyeingNozzleQty").val()}`,
                fields: "DyeingMcslNo,Company,DyeingMcStatus",
                headerTexts: "Machine No, Unit, Status",
                isMultiselect: false,
                primaryKeyColumn: "DMID",
                onSelect: function (res) {
                    finder.hideModal();
                    $formEl.find("#DMNo").val(res.rowData.DyeingMcslNo);
                    $formEl.find("#DMID").val(res.rowData.DMID);
                }
            });
            finder.showModal();
        });

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
        $formEl.find("#btnSplit").click(function (e) {
            e.preventDefault();
            split();
        });
        $formEl.find("#btnSaveKProd").click(function (e) {
            e.preventDefault();
            saveKProd();
        });
    });

    function getSelectedBatchDetails(batchIds) {
        axios.get(`/api/yd-dyeing-batch/get-batch-details/${batchIds}`)
            .then(function (response) {
                for (var i = 0; i < response.data.length; i++) {
                    var batch = { YDBatchID: response.data[i].YDBatchID, YDBatchNo: response.data[i].YDBatchNo, BatchUseQtyKG: response.data[i].BatchWeightKG, BatchQtyPcs: response.data[i].BatchQtyPcs };
                    $tblChildEl.getCurrentViewRecords().push(batch);

                    for (var j = 0; j < response.data[i].YDDyeingBatchItems.length; j++) {
                        $tblItemChildE1.getCurrentViewRecords().push(response.data[i].YDDyeingBatchItems[j]);
                    }

                    for (var j = 0; j < response.data[i].YDDyeingBatchRecipes.length; j++) {
                        $tblRecipeChildE1.getCurrentViewRecords().push(response.data[i].YDDyeingBatchRecipes[j]);
                    }
                }
                initChildTable($tblChildEl.getCurrentViewRecords());
                initItemChildTable($tblItemChildE1.getCurrentViewRecords());
                initRecipeChildTable($tblRecipeChildE1.getCurrentViewRecords());
                $tblChildEl.refresh();
                $tblItemChildE1.refresh();
                $tblRecipeChildE1.refresh();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'report') {
            var a = document.createElement('a');
            a.href = "/reports/InlinePdfView?ReportId=1254&YDDBatchNo=" + args.rowData.YDDBatchNo;
            a.setAttribute('target', '_blank');
            a.click();
        }
        else {
            if (status === statusConstants.PENDING)
                getNew(args.rowData.YDBatchID);
            else
                getDetails(args.rowData.YDDBatchID);
        }
    }

    // #endregion

    function initMasterTable() {
        //var commands = status == statusConstants.PENDING
        //    ? [{ type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }]
        //    : [
        //        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
        //        { type: 'report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
        //    ];

        var commands = [
            { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
            { type: 'report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
        ];

        var columnVisible = false;
        if (status == statusConstants.PENDING) columnVisible = true;
        else if (status == statusConstants.COMPLETED) columnVisible = true;
        var columns = [
            { headerText: 'Actions', visible: status != statusConstants.PENDING, commands: commands, width: 20, textAlign: 'Center' },
            { field: 'Status', headerText: 'Status', width: 20, visible: status == statusConstants.PENDING },

            { field: 'YDDBatchNo', headerText: 'D. Batch No', width: 20, visible: status == statusConstants.COMPLETED },
            { field: 'YDDBatchDate', headerText: 'D. Batch Date', textAlign: 'Center', width: 20, type: 'date', format: _ch_date_format_1, visible: status == statusConstants.COMPLETED },

            { field: 'ConceptNo', headerText: 'Concept No', width: 20 },
            { field: 'YDBatchNo', headerText: 'Batch No', width: 20 },
            { field: 'YDRecipeNo', headerText: 'Recipe No', width: 20 },
            { field: 'YDRecipeDate', headerText: 'Recipe Date', visible: false, textAlign: 'Center', width: 20, type: 'date', format: _ch_date_format_1 },
            { field: 'BuyerName', headerText: 'Buyer', width: 20 },
            { field: 'BuyerTeamName', headerText: 'Buyer Team', width: 20 },
            { field: 'ColorName', headerText: 'Color Name', width: 20 },
            { field: 'RecipeForName', headerText: 'Recipe For', width: 20, visible: false },
            { field: 'BatchWeightKG', headerText: 'Batch Qty (Kg)', width: 20, visible: columnVisible },
            { field: 'BatchQtyPcs', headerText: 'Batch Qty (Pcs)', width: 20, visible: columnVisible },
            { field: 'MachineLoading', headerText: 'Machine Loading', width: 20, visible: status == statusConstants.COMPLETED },
            { field: 'DyeingNozzleQty', headerText: 'Nozzle Qty', width: 20, visible: false },
            { field: 'ExistingYDDBatchNo', headerText: 'Existing D.Batch No', width: 40, visible: status == statusConstants.PENDING },
        ];

        var selectionType = "Single";
        if (status == statusConstants.PENDING) {
            columns.unshift({ type: 'checkbox', width: 20 });
            selectionType = "Multiple";
        }

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/yd-dyeing-batch/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands,
            allowSelection: status == statusConstants.PENDING,
            selectionSettings: { type: selectionType, checkboxOnly: true, persistSelection: true }
        });
    }

    function initChildTable(data) {
        if ($tblChildEl) {
            $tblChildEl.destroy();
            $(tblChildId).html("");
        }
        ej.base.enableRipple(true);
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true },
            columns: [
                /*
                {
                    headerText: '', width: 8, commands: [
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }
                    ]
                },
                */
                { field: 'YDBatchID', isPrimaryKey: true, visible: false },
                { field: 'YDBatchNo', headerText: 'Batch No', width: 40, allowEditing: false },
                { field: 'BatchUseQtyKG', headerText: 'Batch Qty (KG)', width: 30, allowEditing: false },
                { field: 'BatchUseQtyPcs', headerText: 'Batch Qty (Pcs)', width: 30, allowEditing: false }
            ],
            actionBegin: function (args) {
                if (args.requestType === "save") {
                    args.rowData = args.data;
                    var list = $tblChildEl.getCurrentViewRecords();
                    var totalBatchWeightKG = 0,
                        totalBatchQtyPcs = 0;
                    list.map(x => {
                        if (x.YDBatchID == args.rowData.YDBatchID) {
                            totalBatchWeightKG += args.rowData.BatchUseQtyKG;
                            totalBatchQtyPcs += args.rowData.BatchUseQtyPcs;
                        }
                        else {
                            totalBatchWeightKG += x.BatchUseQtyKG;
                            totalBatchQtyPcs += x.BatchUseQtyPcs;
                        }
                    });
                    $formEl.find("#BatchWeightKG").val(totalBatchWeightKG);
                    $formEl.find("#BatchQtyPcs").val(totalBatchQtyPcs);
                }
            },
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
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
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true },
            columns: [
                /*
                 {
                     headerText: '', width: 8, commands: [
                         { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }
                     ]
                 },
                 */
                { field: 'YDBItemReqID', isPrimaryKey: true, visible: false },
                { field: 'YDBatchNo', headerText: 'Batch No', width: 25, allowEditing: false },
                { field: 'YarnCategory', headerText: 'Yarn Description', width: 45, allowEditing: false },
                { field: 'KnittingType', headerText: 'Machine Type', width: 25, allowEditing: false },
                { field: 'TechnicalName', headerText: 'Technical Name', width: 30, allowEditing: false },
                { field: 'FabricComposition', headerText: 'Composition', width: 45, allowEditing: false },
                { field: 'FabricGsm', headerText: 'Fabric Gsm', width: 15, allowEditing: false },
                { field: 'ItemSubGroup', headerText: 'Sub Group', width: 25, allowEditing: false },
                { field: 'Qty', headerText: 'Qty (Kg)', width: 15, allowEditing: true },
                { field: 'QtyPcs', headerText: 'Qty (Pcs)', width: 15, allowEditing: true },
                { field: 'SoftWindingChildID', headerText: 'SoftWindingChildID', width: 15, allowEditing: false, visible: false },
                { field: 'YDRICRBId', headerText: 'YDRICRBId', width: 15, allowEditing: false, visible: false }
            ],
            /*childGrid: {
                queryString: 'BItemReqID',
                allowResizing: true,
                //toolbar: ['Add', 'Edit', 'Delete', 'Update', 'Cancel'],
                toolbar: [
                    { text: 'Add Roll', tooltipText: 'Add Roll', prefixIcon: 'e-icons e-add', id: 'addItem', visible: status == statusConstants.PENDING },
                    { text: 'Split Roll', tooltipText: 'Split Roll', prefixIcon: 'e-icons e-copy', id: 'splitRoll', visible: status == statusConstants.PENDING }
                ],
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true },
                columns: [

                    {
                        headerText: '', width: 8, commands: [
                            { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }
                        ]
                    },

                    { field: 'YDDBIRollID', headerText: 'YDDBIRollID', textAlign: 'Center', width: 20, allowEditing: false, visible: false },
                    { field: 'RollNo', headerText: 'Roll No', textAlign: 'Center', width: 20, allowEditing: false },
                    { field: 'RollQty', headerText: 'Roll Qty (Kg)', width: 20, allowEditing: false },
                    { field: 'RollQtyPcs', headerText: 'Roll Qty (Pcs)', width: 20, allowEditing: false }
                ],
                toolbarClick: function (args) {
                    var iDCQty = 0;
                    var data = this.parentDetails.parentRowData;
                    selectedData = data;

                    if (args.item.id === "addItem") {

                        setSubGroupValue(masterData.YDDyeingBatchItemRolls);
                        var ChildItemsFabric = masterData.YDDyeingBatchItemRolls.filter(function (el) {
                            return el.BItemReqID == selectedData.BItemReqID;
                        });
                        if (ChildItemsFabric.length == 0) {
                            toastr.error("No Roll Found");
                            return false;
                        }

                        var ChildsFabric = masterData.YDDyeingBatchItems;

                        var FabricItemList = new Array();

                        if (ChildItemsFabric.length > 0) {
                            //var rollList = ChildItemsFabric.filter(x => x.TechnicalName == selectedData.TechnicalName);

                            var SFDChildRollIDs = "";
                            //var childs = $tblChildEl.getCurrentViewRecords();
                            //var childObj = childs[0];
                            var childObj = ChildsFabric.find(x => x.BItemReqID == data.BItemReqID);
                            if (typeof childObj.YDDyeingBatchItemRolls !== "undefined" && childObj.YDDyeingBatchItemRolls != null) {
                                SFDChildRollIDs = childObj.YDDyeingBatchItemRolls.map(x => x.SFDChildRollID).join(",");
                            }

                            var rollList = ChildItemsFabric;
                            var finder = new commonFinder({
                                title: "Select Items",
                                pageId: pageId,
                                data: rollList, //masterData.Childs,
                                fields: "RollNo,RollQty,RollQtyPcs,GroupConceptNo",
                                headerTexts: "Roll No,Roll Qty(kg),Roll Qty(Pcs),Group Concept No",
                                isMultiselect: true,
                                selectedIds: SFDChildRollIDs,
                                allowPaging: false,
                                primaryKeyColumn: "SFDChildRollID",
                                onMultiselect: function (selectedRecords) {
                                    for (var i = 0; i < selectedRecords.length; i++) {
                                        var oPreProcess = {
                                            SFDChildRollID: selectedRecords[i].SFDChildRollID,
                                            SFDChildID: selectedRecords[i].SFDChildID,
                                            GroupConceptNo: selectedRecords[i].GroupConceptNo,
                                            ConceptID: selectedRecords[i].ConceptID,
                                            CCColorID: selectedRecords[i].CCColorID,
                                            ColorID: selectedRecords[i].ColorID,
                                            BChildID: selectedRecords[i].BChildID,
                                            BItemReqID: selectedRecords[i].BItemReqID,
                                            BatchID: selectedRecords[i].YDBatchID,
                                            RollQty: selectedRecords[i].RollQty,
                                            RollQtyPcs: selectedRecords[i].RollQtyPcs,
                                            GRollID: selectedRecords[i].GRollID,
                                            RollNo: selectedRecords[i].RollNo,
                                            SubGroupID: selectedRecords[i].SubGroupID,
                                            SubGroupName: selectedRecords[i].SubGroupName,
                                            BatchNo: selectedRecords[i].YDBatchNo,
                                            BItemReqID: selectedRecords[i].BItemReqID,
                                            ItemMasterID: selectedRecords[i].ItemMasterID
                                        }
                                        FabricItemList.push(oPreProcess);
                                        iDCQty += parseFloat(selectedRecords[i].RollQty);
                                    }

                                    var indexFind = ChildsFabric.findIndex(x => x.BItemReqID == data.BItemReqID);
                                    ChildsFabric[indexFind].YDDyeingBatchItemRolls = FabricItemList;
                                    //ChildsFabric[indexFind].ChildItems = FabricItemList;
                                    ChildsFabric[indexFind].Qty = iDCQty;
                                    ////var indexFindMD = masterData.YDDyeingBatchItems.findIndex(x => x.BItemReqID == data.BItemReqID);
                                    ////masterData.YDDyeingBatchItems[indexFindMD].YDDyeingBatchItemRolls = FabricItemList;
                                    ////masterData.YDDyeingBatchItems[indexFindMD].Qty = iDCQty;
                                    var index = $tblItemChildE1.getRowIndexByPrimaryKey(data.BItemReqID);
                                    $tblItemChildE1.updateRow(index, ChildsFabric[indexFind]);
                                    $tblItemChildE1.refreshColumns;
                                }
                            });
                            finder.showModal();
                        }
                    }
                    else if (args.item.id === "splitRoll") {

                        setSubGroupValue(masterData.YDDyeingBatchItemRolls);
                        var ChildItemsFabric = masterData.YDDyeingBatchItemRolls.filter(function (el) {
                            return el.BItemReqID == selectedData.BItemReqID;
                        });
                        initMachineParamTable(ChildItemsFabric);
                        $("#modal-machine").modal('show');

                    }
                },
                actionBegin: function (args) {
                    if (args.requestType === "add") {

                    }
                    //else if (args.requestType === "save") {
                    //    var data = this.parentDetails.parentRowData;
                    //}
                },
                load: loadFirstLevelChildGrid
            }*/
        });
        $tblItemChildE1.refreshColumns;
        $tblItemChildE1.appendTo(tblItemChildId);
    }
    function setSubGroupValue(childItems) {
        _subGroup = 0;
        if (childItems != null && childItems.length > 0) {
            if (childItems[0].SubGroupName == "Fabric") _subGroup = 1;
            else if (childItems[0].SubGroupName == "Collar") _subGroup = 11;
            else if (childItems[0].SubGroupName == "Cuff") _subGroup = 12;
            else _subGroup = 1;
        }
    }
    function initMachineParamTable(list) {

        tblMachineParamId.bootstrapTable("destroy");
        tblMachineParamId.bootstrapTable({
            editable: true,
            columns: [
                {
                    field: "GRollID",
                    title: "Roll ID",
                    align: 'left',
                    visible: false
                },
                {
                    field: "RollNo",
                    title: "Roll No",
                    align: 'left'
                },
                {
                    field: "RollQty",
                    title: "Roll Qty (KG)",
                    align: 'right'
                },
                //{
                //    field: "RollQtyPcs",
                //    title: "Roll Qty (PCS)",
                //    visible: _subGroup != 1,
                //    align: 'right'
                //},
                {
                    field: "BatchNo",
                    title: "Batch No",
                    align: 'left'
                },
                //{
                //    field: "GroupConceptNo",
                //    title: "Group Concept No",
                //    align: 'left',
                //},
                {
                    field: "",
                    title: "Split",
                    filterControl: "input",
                    align: 'center',
                    formatter: function (value, row, index, field) {
                        return '<button type="button" id="btnSplitRoll" class="split">Split</button >';
                    },
                    events: {
                        'click .split': function (e, value, row, index) {

                            e.preventDefault();
                            initRollTable();
                            $tblKProductionE1.data("roll", row);
                            $tblKProductionE1.data("index", index);
                            showRoll(row.GRollID);

                            totalQty = row.RollQtyKg;
                        },
                    }
                }
            ],
            data: list,
            onEditableSave: function (field, row, oldValue, $el) {
                //row.ParamValue
            },
        });
    }
    function loadFirstLevelChildGrid() {
        this.dataSource = this.parentDetails.parentRowData.YDDyeingBatchItemRolls;
    }

    function initRecipeChildTable(data) {
        $formEl.find("#tblRecipeChildId").bootstrapTable('destroy');
        $formEl.find("#tblRecipeChildId").bootstrapTable({
            detailView: true,
            allowResizing: true,
            uniqueId: 'RecipeChildID',

            columns: [
                { field: 'FiberPart', title: 'Fiber Part', width: 25, allowEditing: false },
                { field: 'ColorName', title: 'Color Name', width: 40, allowEditing: false },
                { field: 'TempIn', title: 'Temperature', width: 20, allowEditing: false },
                { field: 'ProcessTime', title: 'Process Time', width: 20, allowEditing: false }
            ],
            data: data,
            onExpandRow: function (index, row, $detail) {
                populateChildex(row.YDRecipeDInfoID, $detail);
            }
        });
    }
    function initRollTable() {

        $tblRollEl.bootstrapTable("destroy");
        $tblRollEl.bootstrapTable({
            showFooter: true,
            columns: [
                {
                    field: "BatchID",
                    title: "BatchID",
                    visible: false,
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "RollNo",
                    title: "Roll No",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "RollQty",
                    title: "Roll Qty (Kg)",
                    visible: _subGroup == 1,
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" step="0.01" pattern="^\d+(?:\.\d{1,2})?$" style="padding-right: 24px;">',
                        validate: function (value) {
                            //if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                            //    return 'Must be a positive integer.';
                            //}
                        }
                    }
                },
                {
                    field: "RollQtyPcs",
                    title: "Roll Qty (Pcs)",
                    visible: _subGroup != 1,
                    cellStyle: function () { return { classes: 'm-w-80' } },
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
            ],
            //onEditableSave: function (value, row, index, field) {
            //    var remainQty = parseFloat($tblKProductionE1.data("roll").RollQty) - parseFloat(row.RollQty);
            //    var rows = $tblRollEl.bootstrapTable('getData');
            //    for (var i = 0; i < rows.length; i++) {
            //        if (row.RollNo != rows[i].RollNo) {
            //            rows[i].RollQty = (parseFloat(remainQty) / parseInt(rows.length - 1)).toFixed(2);
            //        }
            //    }
            //    $tblRollEl.bootstrapTable("load", rows);
            //    $tblRollEl.bootstrapTable('hideLoading');
            //}
        });
    }
    function split() {

        var splitList = [];
        var totalRoll = parseInt($formEl.find("#NoOfSplit").val());
        var roll = $tblKProductionE1.data("roll");
        for (var i = 0; i < totalRoll; i++) {
            var obj = {
                RollID: roll.GRollID,
                RollNo: roll.RollNo + '_' + (i + 1),
                BatchID: roll.YDBatchID,
                BatchNo: roll.YDBatchNo,
                InActive: 0,
                IsSave: false
            }
            if (_subGroup == 1) {
                obj.RollQty = (parseFloat(roll.RollQty) / totalRoll).toFixed(2);
                obj.RollQtyPcs = 0;
            } else {
                obj.RollQtyPcs = (parseInt(roll.RollQtyPcs) / totalRoll);
                obj.RollQty = (parseFloat(roll.RollQty) / totalRoll).toFixed(2);
            }
            splitList.push(obj);
        }
        $tblRollEl.bootstrapTable("load", splitList);
        $tblRollEl.bootstrapTable('hideLoading');
        $formEl.find("#btnSaveKProd").fadeIn();
    }
    function showRoll(rollId) {

        if (rollId > 0) {
            var ChildItemsFabric = masterData.YDDyeingBatchItemRolls.filter(x => x.GRollID == rollId);
            if (ChildItemsFabric.length > 0) {
                $tblRollEl.bootstrapTable("load", ChildItemsFabric);
                $tblRollEl.bootstrapTable('hideLoading');
                $formEl.find("#btnSaveKProd").fadeIn();
            }
            $formEl.find("#NoOfSplit").val(0);
            $("#modal-child").modal('show');

            var gRollID = $tblKProductionE1.data("roll").GRollID;
            axios.get(`/api/batch/get-roll/${gRollID}`)
                .then(function (response) {

                    if (response.data.length > 0) {
                        response.data.map(x => {
                            x.RollQtyKg = x.RollQty;
                        });
                        $tblRollEl.bootstrapTable("load", response.data);
                        $tblRollEl.bootstrapTable('hideLoading');
                        $formEl.find("#btnSaveKProd").fadeOut();
                    } else {
                        $tblRollEl.bootstrapTable("load", [$tblKProductionE1.data("roll")]);
                        $tblRollEl.bootstrapTable('hideLoading');
                        $formEl.find("#btnSaveKProd").fadeIn();
                    }
                    $formEl.find("#NoOfSplit").val(0);
                    $("#modal-child").modal('show');
                })
                .catch(function (err) {
                    toastr.error(err.response.data.Message);
                });
        }
    }
    function saveKProd() {

        var rows = $tblRollEl.bootstrapTable('getData');
        rows.map(x => {
            x.GRollID = x.RollID;
            x.RollQty = x.RollQty;
        });

        var splitTotalQty = rows.reduce((total, row) => total + parseFloat(row.RollQty), 0);
        if (splitTotalQty > totalQty || splitTotalQty < totalQty) {
            toastr.error(`Total Split Qty of Roll must be equal to ${totalQty}`);
            return false;
        }


        axios.post("/api/batch/save-kProd", rows)
            .then(function (response) {

                $tblRollEl.bootstrapTable("load", response.data.newSplitedList);
                $tblRollEl.bootstrapTable('hideLoading');
                toastr.success("Saved successfully!");
                $('#btnCloseRollSplitted').click();
                var aaa = response.data.totalUpdatedList;
                var indexList = [];
                response.data.newSplitedList.map(x => {

                    var index = masterData.YDDyeingBatchItemRolls.findIndex(y => y.GRollID == x.GRollID);
                    if (index < 0) {
                        index = masterData.YDDyeingBatchItemRolls.findIndex(y => y.GRollID == x.ParentGRollID);
                        var hasList = masterData.YDDyeingBatchItemRolls != null && masterData.YDDyeingBatchItemRolls.length > 0 ? true : false;
                        x.SFDChildRollID = x.GRollID;
                        x.SFDChildID = hasList ? masterData.YDDyeingBatchItemRolls[index].BItemReqID : 0;
                        x.RollQty = x.RollQty;
                        x.GRollID = x.GRollID;
                        x.BChildID = hasList ? masterData.YDDyeingBatchItemRolls[index].BChildID : 0;
                        x.BItemReqID = hasList ? masterData.YDDyeingBatchItemRolls[index].BItemReqID : 0;
                        x.YDBatchID = hasList ? masterData.YDDyeingBatchItemRolls[index].YDBatchID : 0;
                        x.YDBatchNo = hasList ? masterData.YDDyeingBatchItemRolls[index].YDBatchNo : 0;
                        x.RollNo = hasList ? masterData.YDDyeingBatchItemRolls[index].RollNo : "";
                        x.SubGroupID = hasList ? masterData.YDDyeingBatchItemRolls[index].SubGroupID : _subGroup;
                        x.SubGroupName = hasList ? masterData.YDDyeingBatchItemRolls[index].SubGroupName : getSubGroupName(x.SubGroupID);
                        x.GroupConceptNo = hasList ? masterData.YDDyeingBatchItemRolls[index].GroupConceptNo : "";
                        x.ConceptID = hasList ? masterData.YDDyeingBatchItemRolls[index].ConceptID : 0;
                        x.CCColorID = hasList ? masterData.YDDyeingBatchItemRolls[index].CCColorID : 0;
                        x.ColorID = hasList ? masterData.YDDyeingBatchItemRolls[index].ColorID : 0;
                        masterData.YDDyeingBatchItemRolls.push(x);
                        //masterData.YDDyeingBatchItems.find(y => y.BItemReqID == x.BItemReqID).YDDyeingBatchItemRolls.push(x);
                        if (index > -1) {
                            if (indexList.includes(index) === false) {
                                indexList.push(index);
                            }
                            //masterData.YDDyeingBatchItemRolls.splice(index, 1);
                        }
                    }
                });
                indexList.map(index => {
                    masterData.YDDyeingBatchItemRolls.splice(index, 1);
                });
                var ChildItemsFabric = masterData.YDDyeingBatchItemRolls.filter(function (el) {
                    return el.BItemReqID == selectedData.BItemReqID;
                });
                initMachineParamTable(ChildItemsFabric);
                //masterData.KnittingProductions = response.data.totalUpdatedList;
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function populateChildex(childId, $detail) {
        //
        $tblDefChildEl = $detail.html('<table id="TblChildDef-' + childId + '"></table>').find('table');
        var childIndex = getIndexFromArray(masterData.YDDyeingBatchRecipes, "YDRecipeDInfoID", childId)
        initDefChildTable(childId, $detail);
        var cnt = (masterData.YDDyeingBatchRecipes[childIndex].DefChilds == null) ? 0 : masterData.YDDyeingBatchRecipes[childIndex].DefChilds.length;
        if (cnt > 0)
            $tblDefChildEl.bootstrapTable('load', masterData.YDDyeingBatchRecipes[childIndex].DefChilds.filter(function (item) {
                return item.EntityState != 8
            }));
    }
    function initDefChildTable(childId, $detail) {
        $tblDefChildEl.bootstrapTable("destroy");
        $tblDefChildEl.bootstrapTable({
            showFooter: true,
            checkboxHeader: false,
            columns: [

                {
                    field: "ParticularsName",
                    title: "Particulars",
                    align: 'center'
                },
                {
                    field: "RawItemName",
                    title: "Item",
                    align: 'center'
                },
                {
                    field: "IsPercentageText",
                    title: "%"
                },
                {
                    field: "Qty",
                    title: "Qty (gm/ltr)/%",
                    align: 'center'
                }

            ]
        });
    }
    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#YDDBatchID").val(-1111);
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
        axios.get(`/api/yd-dyeing-batch/new/${batchId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.YDDBatchDate = formatDateToDefault(masterData.YDDBatchDate);
                masterData.RecipeDate = formatDateToDefault(masterData.RecipeDate);
                masterData.PlanBatchStartTime = formatDateToDefault(masterData.PlanBatchStartTime);
                masterData.PlanBatchEndTime = formatDateToDefault(masterData.PlanBatchEndTime);
                setFormData($formEl, masterData);

                var childs = [
                    {
                        YDBatchID: masterData.YDBatchID,
                        YDBatchNo: masterData.YDBatchNo,
                        BatchUseQtyKG: masterData.BatchWeightKG,
                        BatchUseQtyPcs: masterData.BatchQtyPcs
                    }
                ];

                //masterData.YDDyeingBatchItems = setQtyRollWise(masterData.YDDyeingBatchItems);
                childs = setQtyBatchWise(childs, masterData.YDDyeingBatchItems);

                initChildTable(childs);
                initItemChildTable(masterData.YDDyeingBatchItems);
                initRecipeChildTable(masterData.YDDyeingBatchRecipes);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/yd-dyeing-batch/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.YDDBatchDate = formatDateToDefault(masterData.YDDBatchDate);
                masterData.RecipeDate = formatDateToDefault(masterData.RecipeDate);
                masterData.PlanBatchStartTime = formatDateToDefault(masterData.PlanBatchStartTime);
                masterData.PlanBatchEndTime = formatDateToDefault(masterData.PlanBatchEndTime);
                setFormData($formEl, masterData);

                initChildTable(masterData.YDDyeingBatchWithBatchMasters);
                initItemChildTable(masterData.YDDyeingBatchItems);
                initRecipeChildTable(masterData.YDDyeingBatchRecipes);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        var totalBatchQty = 0;
        $tblChildEl.getCurrentViewRecords().forEach(element => totalBatchQty += parseFloat(element.BatchUseQtyKG));

        //if (parseFloat(data.BatchWeightKG) != totalBatchQty) {
        //    toastr.error("Total Batch Qty and Dyeing Batch Weight should be equal!!");
        //    return;
        //}
        var totalYarnBatchQty = 0;
        var dt = $tblItemChildE1.getCurrentViewRecords();
        $tblItemChildE1.getCurrentViewRecords().forEach(element => totalYarnBatchQty += parseFloat(element.Qty));

        data.BatchWeightKG = totalYarnBatchQty;
        data.YDDyeingBatchWithBatchMasters = $tblChildEl.getCurrentViewRecords();//masterData.FinishingProcessChilds;
        data.YDDyeingBatchItems = $tblItemChildE1.getCurrentViewRecords();
        data.YDDyeingBatches = selectedMasterItems;
        if (data.YDDyeingBatchItems.length == 0) {
            toastr.error("No Item Found.");
            return;
        }
        var childs = [];
        masterData.YDDyeingBatchRecipes.forEach(function (child) {
            child.DefChilds.forEach(function (defChild) {
                defChild.YDRecipeDInfoID = child.YDRecipeDInfoID;
                defChild.Temperature = child.Temperature;
                defChild.TempIn = child.TempIn;
                defChild.ProcessTime = child.ProcessTime;
                childs.push(defChild);
            });
        });
        data.YDDyeingBatchRecipes = childs;
        data.SLNo = masterData.SLNo;
        data.IsRework = isRework;

        axios.post("/api/yd-dyeing-batch/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function createNew() {
        var selectedRecords = $tblMasterEl.getSelectedRecords();
        selectedMasterItems = selectedRecords;
        if (selectedRecords.length == 0) {
            toastr.error("Please select row(s)!");
            return;
        }

        var statusType = [],
            conceptList = [],
            recipeList = [];

        selectedRecords.map(x => {
            statusType.push(x.Status);
            conceptList.push(x.ConceptNo);
            recipeList.push(x.YDRecipeNo);
        });
        var isNew = true;
        var isMultiSelect = false;

        var unique_array = [...new Set(statusType)];
        if (unique_array.length > 1) {
            toastr.error("Status must be same.");
            return;
        }

        isRework = unique_array[0] == "Rework" ? true : false;
        if (isRework) isNew = false;

        unique_array = [...new Set(recipeList)];
        if (unique_array.length > 1) {
            toastr.error("Concept & Recipe must be same.");
            return;
        }
        if (conceptList.length > 1 && unique_array[0] == "New") {
            toastr.error("Multiple select allow for rework only.");
            return;
        }
        unique_array = [...new Set(conceptList)];
        if (unique_array.length > 1) {
            toastr.error("Concept must be same.");
            return;
        }

        var batchIds = selectedRecords.map(x => x.YDBatchID).join(",");
        batchIds = batchIds.split(',');
        if (batchIds.length > 0) {
            batchIds = [...new Set(batchIds)];
            batchIds = batchIds.join(',');

            if (selectedRecords.length > 0) isMultiSelect = true;
        }
        if (isNew && !isMultiSelect) {
            getNew(batchIds);
        } else {
            axios.get(`/api/yd-dyeing-batch/new/multiSelect/${batchIds}`)
                .then(function (response) {
                    $divDetailsEl.fadeIn();
                    $divTblEl.fadeOut();
                    var result = response.data;
                    masterData = result[0];

                    masterData.YDDBatchDate = formatDateToDefault(masterData.YDDBatchDate);
                    masterData.YDRecipeDate = formatDateToDefault(masterData.YDRecipeDate);
                    masterData.PlanBatchStartTime = formatDateToDefault(masterData.PlanBatchStartTime);
                    masterData.PlanBatchEndTime = formatDateToDefault(masterData.PlanBatchEndTime);

                    var childs = [],
                        YDDyeingBatchItems = [],
                        YDDyeingBatchRecipes = [];
                    result.map(x => {
                        childs.push({
                            YDBatchID: x.YDBatchID,
                            YDBatchNo: x.YDBatchNo,
                            BatchUseQtyKG: x.BatchWeightKG,
                            BatchUseQtyPcs: x.BatchQtyPcs
                        });
                        YDDyeingBatchItems.push(...x.YDDyeingBatchItems);
                        YDDyeingBatchRecipes.push(...x.YDDyeingBatchRecipes);
                    });

                    //YDDyeingBatchItems = setQtyRollWise(YDDyeingBatchItems);
                    childs = setQtyBatchWise(childs, YDDyeingBatchItems);

                    initChildTable(childs);
                    initItemChildTable(YDDyeingBatchItems);
                    initRecipeChildTable(YDDyeingBatchRecipes);

                    setFormData($formEl, masterData);
                })
                .catch(function (err) {
                    toastr.error(err.response.data.Message);
                });
        }
    }

    function setQtyBatchWise(listItems, YDDyeingBatchListItems) {
        var batchWeightKG = 0,
            batchQtyPcs = 0;

        listItems.map(x => {
            x.BatchUseQtyKG = 0;
            x.BatchUseQtyPcs = 0;

            YDDyeingBatchListItems.filter(y => y.YDBatchID == x.YDBatchID).map(y => {
                x.BatchUseQtyKG += parseFloat(y.Qty);
                x.BatchUseQtyPcs += parseInt(y.QtyPcs);

                batchWeightKG += parseFloat(y.Qty);
                batchQtyPcs += parseInt(y.QtyPcs);
            });

            x.BatchUseQtyKG = parseFloat(x.BatchUseQtyKG.toFixed(2));
            batchWeightKG = parseFloat(batchWeightKG.toFixed(2));
        });

        masterData.BatchWeightKG = batchWeightKG;
        masterData.BatchQtyPcs = batchQtyPcs;

        return listItems;
    }

    function setQtyRollWise(listItems) {
        listItems.map(x => {
            x.Qty = 0;
            x.QtyPcs = 0;

            x.YDDyeingBatchItemRolls.map(y => {
                x.Qty += parseFloat(y.RollQty);
                x.QtyPcs += parseInt(y.RollQtyPcs);
            });

            x.Qty = parseFloat(x.Qty.toFixed(2));
        });
        return listItems;
    }
})();
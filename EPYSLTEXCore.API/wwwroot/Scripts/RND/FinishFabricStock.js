(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $tblRollEl, tblChildId, $formEl, tblMasterId, $tblKProductionE1;
    var status;
    var masterData = null;
    var fFSFromID = 1000, fFSFRollID = 1000;
    var _tempfFSFromID = 0;
    var pageId;
    var _subGroup = 1;
    var _selectedRoll = null;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        tblMachineParamId = $("#tblMachineParam" + pageId);
        $tblRollEl = $("#tblRoll" + pageId);
        $divRollEl = $("#divRoll" + pageId);
        $tblKProductionE1 = $("#tblKProduction" + pageId);

        status = statusConstants.PENDING;

        initMasterTable();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            status = statusConstants.PENDING;
            toggleActiveToolbarBtn(this, $toolbarEl);
            initMasterTable();
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            status = statusConstants.COMPLETED;
            toggleActiveToolbarBtn(this, $toolbarEl);
            initMasterTable();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });
        $formEl.find("#btnSplit").click(function (e) {
            e.preventDefault();
            split();
        });
        $formEl.find("#btnSaveKProd").click(function (e) {
            e.preventDefault();
            saveKProd();
        });

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#btnNewItem").on("click", function (e) {
            e.preventDefault();
        });
    });
    function initMasterTable() {
        var commands = [];
        if (status == statusConstants.PENDING) {
            commands = [
                { type: 'Add', title: 'Add requisition', visible: status == statusConstants.PENDING, buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } },
            ];
        } else {
            commands = [
                { type: 'Edit', title: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-edit' } }
            ];
        }

        columns = [
            {
                headerText: 'Command', textAlign: 'Center', commands: commands,
                textAlign: 'Center', width: 20
            },

            { field: 'FFSFromID', visible: false },
            { field: 'ConceptID', visible: false },
            { field: 'DBatchID', visible: false },
            { field: 'BatchNo', headerText: 'Batch No', width: 25 },
            //{ field: 'KnittingType', headerText: 'Machine Type', width: 25 },
            { field: 'ColorName', headerText: 'Color', width: 15 },
            { field: 'SubGroupName', headerText: 'End Use', visible: false },
            { field: 'Gsm', headerText: 'Gsm', width: 15 },
            { field: 'Qty', headerText: 'Qty (Kg)', width: 15 },
            { field: 'QtyPcs', headerText: 'Qty (Pcs)', width: 15 }
        ];
        if ($tblMasterEl) $tblMasterEl.destroy();

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            allowFiltering: true,
            apiEndPoint: `/api/finish-fabric-stock/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'Add') {
            getNew(args.rowData.DBatchID);
        }
        else if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.BatchID, args.rowData.ConceptID, args.rowData.DBatchID);
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
        $formEl.find("#FFSFormID").val(-1111);
    }
    function getNew(id) {
        axios.get(`/api/finish-fabric-stock/New/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
                setFormData($formEl, masterData);
                initChildTable([]);

                if (masterData.SubGroupName == "Fabric") {
                    $formEl.find("#divSubGroupName").fadeOut();
                    $formEl.find("#divComposition").fadeIn();
                    $formEl.find("#divGsm").fadeIn();
                    $formEl.find("#divLength").fadeOut();
                    $formEl.find("#divWidth").fadeOut();
                } else {
                    $formEl.find("#divSubGroupName").fadeIn();
                    $formEl.find("#divComposition").fadeOut();
                    $formEl.find("#divGsm").fadeOut();
                    $formEl.find("#divLength").fadeIn();
                    $formEl.find("#divWidth").fadeIn();
                }
            })
            .catch(function (err) {
                // toastr.error(err.response.data.Message);
            });
    }
    function getDetails(batchId, conceptId, dBatchID) {
        axios.get(`/api/finish-fabric-stock/${batchId}/${conceptId}/${dBatchID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.FinishFabricStockForms);

                if (masterData.SubGroupName == "Fabric") {
                    $formEl.find("#divSubGroupName").fadeOut();
                    $formEl.find("#divComposition").fadeIn();
                    $formEl.find("#divGsm").fadeIn();
                    $formEl.find("#divLength").fadeOut();
                    $formEl.find("#divWidth").fadeOut();
                } else {
                    $formEl.find("#divSubGroupName").fadeIn();
                    $formEl.find("#divComposition").fadeOut();
                    $formEl.find("#divGsm").fadeOut();
                    $formEl.find("#divLength").fadeIn();
                    $formEl.find("#divWidth").fadeIn();
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function initChildTable(data) {
        if ($tblChildEl) $tblChildEl.destroy();
        var parentColumns = [
            {
                headerText: 'Commands', width: 40, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                ]
            },
            { field: 'ConceptID', visible: false },
            { field: 'BatchID', visible: false },
            {
                field: 'FormID', width: 200, headerText: 'Form', valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.FormList,
                displayField: "Form",
                edit: ej2GridDropDownObj({})
            },
            {
                field: 'QtyInKG', width: 40, headerText: 'Qty(KG)', allowEditing: false, editType: "numericedit",
                editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 0 } }
            },
            {
                field: 'QtyInPcs', width: 40, headerText: 'Qty(Pcs)', allowEditing: false, editType: "numericedit",
                editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } }
            },

        ];
        var childColumns = [
            {
                headerText: 'Action', textAlign: 'Center', width: 100, commands: [
                    //{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    //{ type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                    //{ type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                ]
            },
            { field: 'FFSFRollID', isPrimaryKey: true, visible: false },
            { field: 'FFSFromID', visible: false },
            { field: 'DBIRollID', headerText: 'DBIRollID', visible: false },
            { field: 'GRollID', headerText: 'GRollID', visible: false },
            { field: 'RollNo', headerText: 'Roll No', width: 35 },
            {
                field: 'RollQty', width: 40, headerText: 'Roll Qty(KG)',
                editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 0 } }
            },
            {
                field: 'RollQtyPcs', width: 40, headerText: 'Roll Qty(Pcs)',
                editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } }
            }
        ];

        ej.base.enableRipple(true);
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            toolbar: ['Add'],
            allowResizing: true,
            showColumnChooser: true,
            showDefaultToolbar: false,
            commandClick: childCommandClick,
            editSettings: {
                allowAdding: true,
                allowEditing: true,
                allowDeleting: true,
                mode: "Normal",
                showDeleteConfirmDialog: true
            },
            columns: parentColumns,
            actionBegin: function (args) {
                if (args.requestType === "add") {

                    if (status == statusConstants.PENDING) {
                        var indexF = masterData.FinishFabricStockForms.findIndex(x => x.FFSFromID == args.data.FFSFromID);
                        if (indexF < 0) {
                            args.data.Childs = [];
                            args.data.FFSFromID = fFSFromID++;
                            args.data.ConceptID = masterData.ConceptID;
                            args.data.BatchID = masterData.BatchID;

                            _tempfFSFromID = args.data.FFSFromID;
                            masterData.FinishFabricStockForms.push(args.data);
                        }
                    }
                    var childs = [];

                    var totalRollQtyinKG = 0;
                    var totalRollQtyinPcs = 0;
                    childs.map(x => {
                        totalRollQtyinKG += x.RollQty;
                        totalRollQtyinPcs += x.RollQtyPcs;
                    });
                    args.data.QtyInKG = totalRollQtyinKG;
                    args.data.QtyInPcs = totalRollQtyinPcs;

                }
                if (args.requestType === "save") {
                    var indexF = masterData.FinishFabricStockForms.findIndex(x => x.FFSFromID == args.data.FFSFromID);
                    if (indexF < 0) {
                        args.data.FFSFromID = fFSFromID++;
                        _tempfFSFromID = args.data.FFSFromID;
                        masterData.FinishFabricStockForms.push(args.data);

                        initChildTable(masterData.FinishFabricStockForms);
                    } else {
                        masterData.FinishFabricStockForms[indexF] = args.data;
                    }
                    if (args.data.FormID == null || typeof args.data.FormID === "undefined" || args.data.FormID == 0) {
                        toastr.error("Select Form");
                        args.data.editable = false;
                        return;
                    }
                    if (args.data.FormID == 'Empty') {
                        //    args.rowData = args.data;
                        //    args.rowData.HangerQtyInPcs = 0;
                        //    $tblChildEl.updateRow(args.rowIndex, args.rowData);
                    }


                }
                else if (args.requestType === "delete") {

                }
            },
            childGrid: {
                queryString: 'FFSFromID',
                allowResizing: true,
                autofitColumns: false,
                toolbar: [{ text: 'Add Item', tooltipText: 'Add Item', prefixIcon: 'e-icons e-add', id: 'addItem' },
                { text: 'Split Roll', tooltipText: 'Split Roll', prefixIcon: 'e-icons e-copy', id: 'splitRoll' }
                ],
                editSettings: {
                    allowAdding: true,
                    allowEditing: false,
                    allowDeleting: true,
                    mode: "Normal",
                    showDeleteConfirmDialog: true
                },
                columns: childColumns,
                toolbarClick: function (args) {
                    var evt = args;
                    _tempfFSFromID = this.parentDetails.parentRowData.FFSFromID;
                    if (args.item.id === "addItem") {
                        var rolls = getRolls();
                        if (rolls.length == 0) {
                            toastr.error("No remaining roll found.");
                            return false;
                        }
                        var finder = new commonFinder({
                            title: "Select Item(s)",
                            pageId: pageId,
                            isMultiselect: true,
                            modalSize: "modal-lg",
                            primaryKeyColumn: "GRollID",
                            fields: "RollNo,RollQty,RollQtyPcs",
                            headerTexts: "Roll No,Roll Qty(KG),Roll Qty(Pcs)",
                            widths: "100,80,80",
                            autofitColumns: false,
                            data: rolls,
                            //apiEndPoint: "/api/dyeing-batch/get-rolls/",
                            onMultiselect: function (selectedRecords) {
                                if (selectedRecords) {
                                    var indexF = masterData.FinishFabricStockForms.findIndex(x => x.FFSFromID == _tempfFSFromID);
                                    if (indexF > -1) {
                                        for (var i = 0; i < selectedRecords.length; i++) {
                                            var obj = DeepClone(selectedRecords[i]);
                                            obj.FFSRollID = fFSFRollID++;
                                            obj.FFSFromID = _tempfFSFromID;
                                            if (masterData.FinishFabricStockForms.find(x => x.FFSFromID == obj.FFSFromID).Childs.find(c => c.GRollID == obj.GRollID) == null) {
                                                masterData.FinishFabricStockForms.find(x => x.FFSFromID == obj.FFSFromID).Childs.push(obj);
                                            }
                                        }
                                        masterData.FinishFabricStockForms.map(x => {
                                            var totalRollQtyinKG = 0;
                                            var totalRollQtyinPcs = 0;
                                            x.Childs.map(c => {
                                                totalRollQtyinKG += c.RollQty;
                                                totalRollQtyinPcs += c.RollQtyPcs;
                                            });
                                            x.QtyInKG = totalRollQtyinKG;
                                            x.QtyInPcs = totalRollQtyinPcs;
                                        });
                                        //masterData.FormList
                                        if (masterData.FinishFabricStockForms[indexF].Form == null || masterData.FinishFabricStockForms[indexF].Form.trim() == "") {
                                            if (masterData.FormList.length > 0) {
                                                if (typeof masterData.FinishFabricStockForms[indexF].FormID != "undefined" && masterData.FinishFabricStockForms[indexF].FormID != null) {
                                                    masterData.FinishFabricStockForms[indexF].Form = masterData.FormList.find(x => x.id == masterData.FinishFabricStockForms[indexF].FormID).text;
                                                }
                                            }
                                        }
                                        $tblChildEl.updateRow(indexF, masterData.FinishFabricStockForms[indexF]);
                                        $tblChildEl.refreshColumns;
                                    }
                                }
                            }
                        });
                        finder.showModal();
                    }
                    else if (args.item.id === "splitRoll") {
                        initMachineParamTable(getRolls());
                        $formEl.find("#modal-machine").modal('show');
                    }
                },

                actionBegin: function (args) {
                    if (args.requestType === 'beginEdit') {
                    }
                    else if (args.requestType === "add") {

                    }
                    else if (args.requestType === "save") {
                        var indexF = masterData.FinishFabricStockForms.findIndex(x => x.FFSFromID == _tempfFSFromID);
                        if (indexF >= 0) {
                            args.data.FFSFRollID = fFSFRollID++;
                            args.data.FFSFromID = _tempfFSFromID;

                            masterData.FinishFabricStockForms[indexF].Childs.push(args.data);
                            //newChildList.push(args.data);
                        }
                    }
                    else if (args.requestType === "delete") {
                        var indexF = masterData.FinishFabricStockForms.findIndex(x => x.FFSFromID == this.parentDetails.parentRowData.FFSFromID);
                        if (indexF > -1) {
                            masterData.FinishFabricStockForms[indexF].Childs = masterData.FinishFabricStockForms[indexF].Childs.filter(x => x.GRollID != args.data[0].GRollID);
                            masterData.FinishFabricStockForms.map(x => {
                                var totalRollQtyinKG = 0;
                                var totalRollQtyinPcs = 0;
                                x.Childs.map(c => {
                                    totalRollQtyinKG += c.RollQty;
                                    totalRollQtyinPcs += c.RollQtyPcs;
                                });
                                x.QtyInKG = totalRollQtyinKG;
                                x.QtyInPcs = totalRollQtyinPcs
                            });

                            //initChildTable(masterData.FinishFabricStockForms);
                            $tblChildEl.updateRow(indexF, masterData.FinishFabricStockForms[indexF]);
                            $tblChildEl.refreshColumns;
                        }

                    }
                },
                load: loadChilds
            },
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }
    function loadChilds() {
        this.dataSource = [];
        var index = masterData.FinishFabricStockForms.findIndex(c => c.FFSFromID == this.parentDetails.parentRowData.FFSFromID);
        if (index > -1) {
            if (typeof masterData.FinishFabricStockForms[index].Childs !== "undefined") {
                this.dataSource = masterData.FinishFabricStockForms[index].Childs;
            }
        }
    }
    async function childCommandClick(e) {
        var childData = e.rowData;

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
                    align: 'left'
                },
                //{
                //    field: "RollQtyPcs",
                //    title: "Roll Qty (PCS)",
                //    align: 'left'
                //},

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
                            _selectedRoll = row;
                            initRollTable();
                            $tblKProductionE1.data("roll", row);
                            $tblKProductionE1.data("index", index);
                            showRoll(row.GRollID);
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
                BatchID: roll.BatchID,
                InActive: 0,
                IsSave: false
            }
            if (_subGroup == 1) {
                obj.RollQty = (parseFloat(roll.RollQty) / totalRoll).toFixed(2);
                obj.RollQtyPcs = 0;

            } else {
                obj.RollQtyPcs = (parseInt(roll.RollQtyPcs) / totalRoll);
                obj.RollQtyPcs = (parseFloat(roll.RollQtyPcs) / totalRoll).toFixed(2);
            }
            splitList.push(obj);
        }
        $tblRollEl.bootstrapTable("load", splitList);
        $tblRollEl.bootstrapTable('hideLoading');
    }
    function showRoll(rollId) {
        if (rollId > 0) {

            var ChildItemsFabric = masterData.knittingProductions.filter(x => x.GRollID == rollId);
            if (ChildItemsFabric.length > 0) {
                $tblRollEl.bootstrapTable("load", ChildItemsFabric);
                $tblRollEl.bootstrapTable('hideLoading');
                $formEl.find("#btnSaveKProd").fadeIn();
            }
            $formEl.find("#NoOfSplit").val(0);
            $("#modal-child").modal('show');

            var gRollID = $tblKProductionE1.data("roll").GRollID;
            axios.get(`/api/batch/get-groll/${gRollID}`)
                .then(function (response) {
                    if (response.data.length > 0) {
                        response.data.map(x => {
                            x.RollQty = x.RollQtyKg;

                        });
                        $tblRollEl.bootstrapTable("load", response.data);
                        $tblRollEl.bootstrapTable('hideLoading');
                        //$formEl.find("#btnSaveKProd").fadeOut();
                    } else {
                        $tblRollEl.bootstrapTable("load", [$tblKProductionE1.data("roll")]);
                        $tblRollEl.bootstrapTable('hideLoading');
                        //$formEl.find("#btnSaveKProd").fadeIn();
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
        var selectedDyeingRoll = masterData.knittingProductions.find(x => x.GRollID == _selectedRoll.GRollID);
        if (selectedDyeingRoll == null || typeof selectedDyeingRoll === "undefined") return false;

        var rows = $tblRollEl.bootstrapTable('getData');
        rows.map(x => {
            x.GRollID = x.RollID;
            x.IsSaveDyeingBatchItemRoll = true;
            x.DBIRollID = selectedDyeingRoll.DBIRollID; //Depends on IsSaveDyeingBatchItemRoll
        });
        axios.post("/api/batch/save-kProd", rows)
            .then(function (response) {
                $tblRollEl.bootstrapTable("load", response.data.newSplitedList);
                $tblRollEl.bootstrapTable('hideLoading');
                toastr.success("Saved successfully!");
                $formEl.find('#btnCloseRollSplitted').click();
                var aaa = response.data.totalUpdatedList;
                response.data.newSplitedList.map(x => {
                    var index = masterData.knittingProductions.findIndex(y => y.RollID == x.GRollID);
                    if (index < 0) {
                        var hasList = masterData.knittingProductions != null && masterData.knittingProductions.length > 0 ? true : false;
                        x.RollQtyKg = x.RollQty;
                        x.RollID = x.GRollID;
                        x.BatchID = hasList ? masterData.knittingProductions[0].BatchID : 0;
                        x.BatchNo = hasList ? masterData.knittingProductions[0].BatchNo : 0;
                        x.SubGroupID = hasList ? masterData.knittingProductions[0].SubGroupID : _subGroup;
                        x.SubGroupName = hasList ? masterData.knittingProductions[0].SubGroupName : getSubGroupName(x.SubGroupID);
                        x.GroupConceptNo = hasList ? masterData.knittingProductions[0].GroupConceptNo : "";
                        masterData.knittingProductions.push(x);
                        index = masterData.knittingProductions.findIndex(y => y.GRollID == x.ParentGRollID);
                        if (index > -1) {
                            masterData.knittingProductions.splice(index, 1);
                        }
                    }
                });
                initMachineParamTable(masterData.knittingProductions);

            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function save() {
        var data = formElToJson($formEl);
        data.FinishFabricStockForms = masterData.FinishFabricStockForms;
        if (status == statusConstants.COMPLETED) {
            data.isModified = true;
        }
        var DataList = [];
        var DataListMaster = new Array();
        var fabrics = $tblChildEl.getCurrentViewRecords();
        for (var i = 0; i < fabrics.length; i++) {
            if (fabrics[i].knittingProductions) {
                fabrics[i].knittingProductions.map(x => {
                    var roll = masterData.knittingProductions.find(y => y.RollNo == x.RollNo);
                    if (roll) {
                        x.GRollID = roll.GRollID;
                    }
                });
            }
            DataList.push(fabrics[i]);
        }
        DataListMaster.push(data);
        axios.post("/api/finish-fabric-stock/Save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function getRolls() {
        var allRolls = DeepClone(masterData.knittingProductions);
        masterData.FinishFabricStockForms.map(m => {
            m.Childs.map(c => {
                var indexF = allRolls.findIndex(x => x.GRollID == c.GRollID);
                if (indexF > -1) {
                    allRolls = allRolls.filter(x => x.GRollID != c.GRollID);
                }
            });
        });
        return allRolls;
    }
    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
})();
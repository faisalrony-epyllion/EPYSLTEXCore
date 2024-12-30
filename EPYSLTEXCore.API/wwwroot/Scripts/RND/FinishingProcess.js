(function () {
    var menuId, pageName, pageId;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, tblMasterId, $tblMasterEl, $tblChildEl, tblChildId, $tblPostChildEl, $tblColorChildEl, tblPostChildId, tblColorChildId, tblMachineId, $formEl;
    var childGridcolumnList = [];
    var saveChildGridcolumnList = [];
    var tableParams = {
        offset: 0,
        limit: 10,
        filter: '',
        sort: '',
        order: ''
    }
    var status;
    var masterData;
    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblProcessId = "#tblProcess" + pageId;
        tblPostProcessId = "#tblPostProcess" + pageId;
        tblPostChildId = "#tblPostChild" + pageId;
        tblColorChildId = "#tblColorChild" + pageId;
        tblMachineId = $("#tblMachine" + pageId);
        tblSaveMachineId = $("#tblSaveMachine" + pageId);
        tblMachineParamId = $("#tblMachineParam" + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        toggleActiveToolbarBtn($toolbarEl.find("#btnPrePending"), $toolbarEl);
        status = statusConstants.PRE_PENDING;
        initMasterTable();

        $toolbarEl.find("#btnPrePending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PRE_PENDING;

            initMasterTable();
        });

        $toolbarEl.find("#btnPostPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.POST_PENDING;
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

        $formEl.find("#btnSelectMachine").on("click", function (e) {
            e.preventDefault();
            SetMachine();
        });

        $formEl.find("#btnAddPreProcess").on("click", function (e) {
            e.preventDefault();
            var finder = new commonFinder({
                title: "Select Process",
                pageId: pageId,
                data: masterData.PreProcessList,
                fields: "ProcessName,ProcessType,MachineName",
                headerTexts: "Process Name, Process Type,Machine Name",
                isMultiselect: true,
                allowPaging: false,
                primaryKeyColumn: "ProcessID",
                onMultiselect: function (selectedRecords) {
                    for (var i = 0; i < selectedRecords.length; i++) {
                        var oPreProcess = {
                            FPChildID: getMaxIdForArray(masterData.PreFinishingProcessChilds, "FPChildID"),
                            FPMasterID: 0,
                            ProcessID: selectedRecords[i].ProcessID,
                            ProcessTypeID: selectedRecords[i].ProcessTypeID,
                            ProcessName: selectedRecords[i].ProcessName,
                            ProcessType: selectedRecords[i].ProcessType,
                            MachineName: selectedRecords[i].MachineName,
                            FMCMasterID: selectedRecords[i].FMCMasterID,
                            MachineNo: "",
                            UnitName: "",
                            BrandName: "",
                            Remarks: "",
                            PreFinishingProcessChildItems: []
                        }
                        masterData.PreFinishingProcessChilds.push(oPreProcess);
                    }

                    initChildTable(masterData.PreFinishingProcessChilds);
                }
            });
            finder.showModal();
        });

        $formEl.find("#btnAddPostProcessColor").on("click", function (e) {
            var data = masterData.ColorList;
            if (data.length == 0) {
                toastr.error('Color not found for this concept!');
            }
            else {
                var colorList = [];
                data.forEach(function (value) {
                    //
                    var exists = $tblColorChildEl.getCurrentViewRecords().find(function (el) {
                        return el.ColorID == value.ColorID
                    });
                    if (!exists) {
                        colorList.push(value);
                    }
                })
                if (colorList.length == 0) {
                    toastr.error('All Color already used!');
                }
                e.preventDefault();
                var finder = new commonFinder({
                    title: "Select Color",
                    pageId: pageId,
                    data: colorList,
                    fields: "ColorName",
                    headerTexts: "Color Name",
                    widths: "100",
                    isMultiselect: true,
                    allowPaging: false,
                    primaryKeyColumn: "ColorID",
                    onMultiselect: function (selectedRecords) {
                        //
                        var listCol = $tblColorChildEl.getCurrentViewRecords();
                        selectedRecords.forEach(function (value) {
                            listCol.unshift({
                                CCColorID: value.CCColorID,
                                ConceptID: value.ConceptID,
                                ColorID: value.ColorID,
                                ColorName: value.ColorName
                            });
                        });
                        initChildTableColor(listCol);
                    }
                });
                finder.showModal();
            }
        });

        $formEl.find("#btnAddPostProcess").on("click", function (e) {
            e.preventDefault();
            var finder = new commonFinder({
                title: "Select Process",
                pageId: pageId,
                data: masterData.PostProcessList,
                fields: "ProcessName,ProcessType,MachineName",
                headerTexts: "Process Name, Process Type,Machine Name",
                isMultiselect: true,
                allowPaging: false,
                primaryKeyColumn: "ProcessID",
                onMultiselect: function (selectedRecords) {
                    for (var i = 0; i < selectedRecords.length; i++) {
                        var oPreProcess = {
                            FPChildID: getMaxIdForArray(masterData.PostFinishingProcessChilds, "FPChildID"),
                            FPMasterID: 0,
                            ProcessID: selectedRecords[i].ProcessID,
                            ProcessTypeID: selectedRecords[i].ProcessTypeID,
                            ProcessName: selectedRecords[i].ProcessName,
                            ProcessType: selectedRecords[i].ProcessType,
                            MachineName: selectedRecords[i].MachineName,
                            FMCMasterID: selectedRecords[i].FMCMasterID,
                            MachineNo: "",
                            UnitName: "",
                            BrandName: "",
                            Remarks: "",
                            FMSID: null,
                            Param1Value: null,
                            Param2Value: null,
                            Param3Value: null,
                            Param4Value: null,
                            Param5Value: null,
                            Param6Value: null,
                            Param7Value: null,
                            Param8Value: null,
                            Param9Value: null,
                            Param10Value: null,
                            Param11Value: null,
                            Param12Value: null,
                            Param13Value: null,
                            Param14Value: null,
                            Param15Value: null,
                            Param16Value: null,
                            Param17Value: null,
                            Param18Value: null,
                            Param19Value: null,
                            Param20Value: null,
                            PreFinishingProcessChildItems: []
                        }
                        masterData.PostFinishingProcessChilds.push(oPreProcess);
                    }
                    initChildTableColor(masterData.PostFinishingProcessChilds);
                }
            });
            finder.showModal();
        });
    });
    var cGrid;
    async function initChildTableColor(data) {
        if ($tblColorChildEl) $tblColorChildEl.destroy();

        var childColumns = [
            {
                headerText: '', width: 30, commands: [
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }
                ]
            }, 
            { field: 'FPChildID', isPrimaryKey: true, visible: false },
            { field: 'ColorID', headerText: 'ColorID', width: 100, allowEditing: false, visible: false },
            { field: 'ProcessName', headerText: 'Process Name', width: 100, allowEditing: false },
            { field: 'ProcessType', headerText: 'Process Type.', width: 100, allowEditing: false },
            { field: 'MachineName', headerText: 'Machine Name', width: 100, allowEditing: false },
            { field: 'MachineNo', headerText: 'Machine No', width: 100, allowEditing: false },
            {
                headerText: '...', width: 30, commands: [
                    { buttonOption: { type: 'machine', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
                ]
            },
            { field: 'UnitName', headerText: 'Unit', width: 80, allowEditing: false },
            { field: 'BrandName', headerText: 'Brand', width: 80, allowEditing: false },
            { field: 'Param1Value', headerText: 'Param 1 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param2Value', headerText: 'Param 2 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param3Value', headerText: 'Param 3 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param4Value', headerText: 'Param 4 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param5Value', headerText: 'Param 5 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param6Value', headerText: 'Param 6 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param7Value', headerText: 'Param 7 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param8Value', headerText: 'Param 8 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param9Value', headerText: 'Param 9 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param10Value', headerText: 'Param 10 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param11Value', headerText: 'Param 11 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param12Value', headerText: 'Param 12 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param13Value', headerText: 'Param 13 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param14Value', headerText: 'Param 14 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param15Value', headerText: 'Param 15 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param16Value', headerText: 'Param 16 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param17Value', headerText: 'Param 17 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param18Value', headerText: 'Param 18 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param19Value', headerText: 'Param 19 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Param20Value', headerText: 'Param 20 Value', width: 100, allowEditing: false, visible: false },
            { field: 'Remarks', headerText: 'Remarks', width: 100, allowEditing: true }
        ]

        ej.base.enableRipple(true);
        $tblColorChildEl = new ej.grids.Grid({
            dataSource: data,
            allowRowDragAndDrop: true,
            selectionSettings: { type: 'Multiple' },
            editSettings: { allowDeleting: true, allowEditing: true, mode: "Normal", showDeleteConfirmDialog: true },
            allowResizing: true,
            columns: [
                //{
                //    headerText: 'Action', width: 30, commands: [{ type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }]
                //},
                { field: 'ColorID', headerText: 'ColorID', visible: false, allowEditing: false, isPrimaryKey: true },
                { field: 'ColorName', headerText: 'Color Name', allowEditing: false, width: 100 },
                //{
                //    field: 'ColorID', headerText: '', displayvalue: 'Add Item', width: 180, valueAccessor: diplayPlanningCriteria
                //}
            ],
            childGrid: {
                queryString: 'ColorID',
                allowRowDragAndDrop: true,
                selectionSettings: { type: 'Multiple' },
                allowResizing: true,
                autofitColumns: false,
                toolbar: [{ text: 'Add Item', tooltipText: 'Add Item', prefixIcon: 'e-icons e-add', id: 'addItem' }],
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns,
                commandClick: commandClick,
                load: loadFirstLevelPostChildGrid,
                toolbarClick: function (args) {
                    var evt = args;
                    var postProcessList = this.parentDetails.parentRowData.PostFinishingProcessChilds;
                    postProcessList = postProcessList == undefined ? [] : postProcessList
                    var colorId = this.parentDetails.parentKeyFieldValue;
                    var dataS = this;
                    if (args.item.id === "addItem") {
                        var finder = new commonFinder({
                            title: "Select Process",
                            pageId: pageId,
                            data: masterData.PostProcessList,
                            fields: "ProcessName,ProcessType,MachineName",
                            headerTexts: "Process Name, Process Type,Machine Name",
                            isMultiselect: true,
                            allowPaging: false,
                            primaryKeyColumn: "ProcessID",
                            onMultiselect: function (selectedRecords) {
                                for (var i = 0; i < selectedRecords.length; i++) {
                                    var FPChildID = getMaxIdForArray(postProcessList, "FPChildID");
                                    var oPreProcess = {
                                        FPChildID: FPChildID,
                                        FPMasterID: 0,
                                        ProcessID: selectedRecords[i].ProcessID,
                                        ProcessTypeID: selectedRecords[i].ProcessTypeID,
                                        ProcessName: selectedRecords[i].ProcessName,
                                        ProcessType: selectedRecords[i].ProcessType,
                                        MachineName: selectedRecords[i].MachineName,
                                        FMCMasterID: selectedRecords[i].FMCMasterID,
                                        MachineNo: "",
                                        UnitName: "",
                                        BrandName: "",
                                        Remarks: "",
                                        ColorID: colorId,
                                        FMSID: null,
                                        Param1Value: null,
                                        Param2Value: null,
                                        Param3Value: null,
                                        Param4Value: null,
                                        Param5Value: null,
                                        Param6Value: null,
                                        Param7Value: null,
                                        Param8Value: null,
                                        Param9Value: null,
                                        Param10Value: null,
                                        Param11Value: null,
                                        Param12Value: null,
                                        Param13Value: null,
                                        Param14Value: null,
                                        Param15Value: null,
                                        Param16Value: null,
                                        Param17Value: null,
                                        Param18Value: null,
                                        Param19Value: null,
                                        Param20Value: null,
                                        PreFinishingProcessChildItems: []
                                    }
                                    masterData.PostFinishingProcessChilds.push(oPreProcess);
                                    postProcessList.push(oPreProcess);
                                }
                                dataS.dataSource = postProcessList;
                                dataS.refresh();
                                //console.log(postProcessList);
                            }
                        });
                        finder.showModal();
                    }
                },
                actionBegin: function (args) {
                    if (args.requestType === "add") {
                        //args.data.FCMRChildID = maxCol++; //getMaxIdForArray($tblChildEl.getCurrentViewRecords(), "FCMRChildID");
                        //args.data.ConceptID = this.parentDetails.parentKeyFieldValue;
                    }
                    else if (args.requestType === "delete") {
                        var selectedData = args.data[0];
                        masterData.PostFinishingProcessChilds.find(x => x.FPChildID == selectedData.FPChildID).EntityState = 8;

                        //args.data.FCMRChildID = maxCol++; //getMaxIdForArray($tblChildEl.getCurrentViewRecords(), "FCMRChildID");
                        //args.data.ConceptID = this.parentDetails.parentKeyFieldValue;
                    }
                },
                actionComplete: function (args) {
                    if (args.requestType === "add") {
                        //args.data.FCMRChildID = maxCol++; //getMaxIdForArray($tblChildEl.getCurrentViewRecords(), "FCMRChildID");
                        //args.data.ConceptID = this.parentDetails.parentKeyFieldValue;
                    }
                }
            }
        });
        $tblColorChildEl.refreshColumns;
        $tblColorChildEl.appendTo(tblColorChildId);
    }

    function loadFirstLevelPostChildGrid() {
        //this.parentDetails.parentKeyFieldValue = this.parentDetails.parentRowData['DBIID'];
        this.dataSource = this.parentDetails.parentRowData.PostFinishingProcessChilds;
    }

    function initMasterTable() {
        //var commands = status == statusConstants.PRE_PENDING || statusConstants.POST_PENDING
        var commands = status == statusConstants.PRE_PENDING
            ? [{ type: 'Add', title: 'Create New', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }]
            : [
                //{ type: 'Add', title: 'Another Batch Create', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus-circle' } },
                { type: 'View', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }
            ];

        var columns = [
            {
                headerText: '', commands: commands, width: 100, maxWidth: 100,
            },
            {
                field: 'ConceptNo', headerText: 'Concept No'
            },
            {
                field: 'ColorName', headerText: 'Color Name'
            },
            {
                field: 'SubClassName', headerText: 'Machine Type'
            },
            {
                field: 'SubGroupName', headerText: 'Sub Group Name'
            },
            {
                field: 'TechnicalName', headerText: 'Technical Name'
            },
            //{
            //    field: 'MachineGauge', headerText: 'Machine Gauge'
            //},
            {
                field: 'Gsm', headerText: 'Gsm'
            },
            {
                field: 'Composition', headerText: 'Composition'
            },
            {
                field: 'Width', headerText: 'Width(CM)', textAlign: 'Center'
            },
            {
                field: 'Length', headerText: 'Length(CM)', textAlign: 'Center'
            },
            {
                field: 'FUPartName', headerText: 'End Use'
            },
            {
                field: 'ConceptDate', headerText: 'Concept Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'PFBatchNo', headerText: 'Batch No', visible: status !== (statusConstants.PRE_PENDING || statusConstants.POST_PENDING)
            },
            {
                field: 'PFBatchDate', headerText: 'Batch Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status !== (statusConstants.PRE_PENDING || statusConstants.POST_PENDING)
            },
            //{
            //    field: 'BatchQty', headerText: 'Batch Qty', visible: status !== statusConstants.PENDING
            //}
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/finishing-process/list?status=${status}`,
            autofitColumns: false,
            columns: columns,
            commandClick: handleCommands,
            queryCellInfo: cellModifyForFP
        });
    }
    function cellModifyForFP(args) {
        //if (args.data.ConceptNo.toLowerCase() == 'bds-2302177-efl_1') {
        //    debugger;
        //}
        if (!args.data.NeedPreFinishingProcess) {
            if (args.cell.childNodes.length > 0) {
                for (var i = 0; i < args.cell.childNodes[0].childNodes.length; i++) {
                    if (args.cell.childNodes[0].childNodes[i].title === 'View Report') {
                        args.cell.childNodes[0].childNodes[i].style.display = "none";
                    }
                }
            }
        }
    }
    function handleCommands(args) {
        if (args.commandColumn.type == "View") {
            window.open(`/reports/InlinePdfView?ReportName=PreFinishProcessBatchCard.rdl&PFBatchNo=${args.rowData.PFBatchNo}`, '_blank');
        }
        else {
            if (args.commandColumn.type == "Add") {
                if (args.rowData.FPMasterID > 0) {
                    getDetails(args.rowData.FPMasterID, args.rowData.ConceptID, args.rowData.IsBDS, args.rowData.GroupConceptNo);
                }
                else {
                    getNew(args.rowData.ConceptID, args.rowData.IsBDS, args.rowData.GroupConceptNo);
                }
            }
            else {
                if (args.rowData.FPMasterID == 0) {
                    getNew(args.rowData.ConceptID, args.rowData.IsBDS, args.rowData.GroupConceptNo);
                }
                else {
                    getDetails(args.rowData.FPMasterID, args.rowData.ConceptID, args.rowData.IsBDS, args.rowData.GroupConceptNo);
                }
            }
        }
    }

    function initMachineTable() {
        tblMachineId.bootstrapTable("destroy");
        tblMachineId.bootstrapTable({
            showFooter: true,
            columns: childGridcolumnList
        });

        tblMachineId.on('dbl-click-row.bs.table', function ($element, row, field) {
            if (machinetype == 'machine') {
                for (var i = 0; i < masterData.PostFinishingProcessChilds.length; i++) {
                    if (masterData.PostFinishingProcessChilds[i].FPChildID == selectedId &&
                        masterData.PostFinishingProcessChilds[i].ColorID == colorId) {
                        masterData.PostFinishingProcessChilds[i].FMSID = row.FMSID;
                        masterData.PostFinishingProcessChilds[i].MachineNo = row.MachineNo;
                        masterData.PostFinishingProcessChilds[i].BrandName = row.BrandName;
                        masterData.PostFinishingProcessChilds[i].UnitName = row.UnitName;
                        //masterData.PostFinishingProcessChilds[i].Param1Value = row.Param1Value;
                        //masterData.PostFinishingProcessChilds[i].Param2Value = row.Param2Value;
                        //masterData.PostFinishingProcessChilds[i].Param3Value = row.Param3Value;
                        //masterData.PostFinishingProcessChilds[i].Param4Value = row.Param4Value;
                        //masterData.PostFinishingProcessChilds[i].Param5Value = row.Param5Value;
                        //masterData.PostFinishingProcessChilds[i].Param6Value = row.Param6Value;
                        //masterData.PostFinishingProcessChilds[i].Param7Value = row.Param7Value;
                        //masterData.PostFinishingProcessChilds[i].Param8Value = row.Param8Value;
                        //masterData.PostFinishingProcessChilds[i].Param9Value = row.Param9Value;
                        //masterData.PostFinishingProcessChilds[i].Param10Value = row.Param10Value;
                        //masterData.PostFinishingProcessChilds[i].Param11Value = row.Param11Value;
                        //masterData.PostFinishingProcessChilds[i].Param12Value = row.Param12Value;
                        //masterData.PostFinishingProcessChilds[i].Param13Value = row.Param13Value;
                        //masterData.PostFinishingProcessChilds[i].Param14Value = row.Param14Value;
                        //masterData.PostFinishingProcessChilds[i].Param15Value = row.Param15Value;
                        //cGrid.updateRow(selectedId, masterData.PostFinishingProcessChilds[i]);
                        cGrid.updateRow(colorId, masterData.PostFinishingProcessChilds[i]);
                        cGrid.refreshColumns;
                    }
                }
            }
            else {
                for (var i = 0; i < masterData.PreFinishingProcessChilds.length; i++) {
                    if (masterData.PreFinishingProcessChilds[i].FPChildID == selectedId) {
                        masterData.PreFinishingProcessChilds[i].FMSID = row.FMSID;
                        masterData.PreFinishingProcessChilds[i].MachineNo = row.MachineNo;
                        masterData.PreFinishingProcessChilds[i].BrandName = row.BrandName;
                        masterData.PreFinishingProcessChilds[i].UnitName = row.UnitName;
                        //masterData.PreFinishingProcessChilds[i].Param1Value = row.Param1Value;
                        //masterData.PreFinishingProcessChilds[i].Param2Value = row.Param2Value;
                        //masterData.PreFinishingProcessChilds[i].Param3Value = row.Param3Value;
                        //masterData.PreFinishingProcessChilds[i].Param4Value = row.Param4Value;
                        //masterData.PreFinishingProcessChilds[i].Param5Value = row.Param5Value;
                        //masterData.PreFinishingProcessChilds[i].Param6Value = row.Param6Value;
                        //masterData.PreFinishingProcessChilds[i].Param7Value = row.Param7Value;
                        //masterData.PreFinishingProcessChilds[i].Param8Value = row.Param8Value;
                        //masterData.PreFinishingProcessChilds[i].Param9Value = row.Param9Value;
                        //masterData.PreFinishingProcessChilds[i].Param10Value = row.Param10Value;
                        //masterData.PreFinishingProcessChilds[i].Param11Value = row.Param11Value;
                        //masterData.PreFinishingProcessChilds[i].Param12Value = row.Param12Value;
                        //masterData.PreFinishingProcessChilds[i].Param13Value = row.Param13Value;
                        //masterData.PreFinishingProcessChilds[i].Param14Value = row.Param14Value;
                        //masterData.PreFinishingProcessChilds[i].Param15Value = row.Param15Value;
                        //$tblChildEl.refresh();
                        var index = $tblChildEl.getRowIndexByPrimaryKey(row.FMSID);
                        $tblChildEl.editModule.updateRow(index, masterData.PreFinishingProcessChilds[i]);
                    }
                }
            }

            $("#modal-child").modal('hide');
            generateSaveChildGrid(selectedData, row.FMSID);
            $("#modal-save-child").modal('hide');
            $("#modal-machine").modal('show');
        });
    }

    function generateChildGrid(row) {
        childGridcolumnList = [];
        var column = {
            field: "Id",
            isPrimaryKey: true,
            visible: false
        }
        childGridcolumnList.push(column);

        column = {
            field: "MachineNo",
            title: "Machine No",
            align: 'center',
            editable: false
        }
        childGridcolumnList.push(column);
        column = {
            field: "UnitName",
            title: "Unit",
            align: 'center',
            editable: false
        }
        childGridcolumnList.push(column);
        column = {
            field: "BrandName",
            title: "Brand",
            align: 'center',
            editable: false
        }

        childGridcolumnList.push(column);
        column = {
            field: "Capacity",
            title: "Capacity",
            align: 'center',
            editable: false
        }
        childGridcolumnList.push(column);
        var filterData = $.grep(masterData.FinishingMachineConfigurationChildList, function (h) {
            return h.FMCMasterID == row.FMCMasterID
        });
        for (var i = 0; i < filterData.length; i++) {
            column = {
                field: `Param${i + 1}Value`,
                title: filterData[i].ParamDisplayName,
                align: "center",
                editable: false
            }
            childGridcolumnList.push(column);
        }

        initMachineTable();

        getMachines(row.ProcessID, row.ProcessTypeID)
    }

    function getMachineParam(fmsId) {
        axios.get(`/api/finishing-process/machine/${fmsId}`)
            .then(function (response) {
                masterData.ProcessMachineList = response.data.ProcessMachineList;

                var filterData;
                if (machinetype == 'machine') {
                    filterData = $.grep(masterData.PostFinishingProcessChilds, function (h) {
                        return h.FPChildID == selectedId && h.ColorID == colorId
                    })

                    $formEl.find("#lblMachineNo").text(filterData[0].MachineNo);
                    $formEl.find("#lblUnit").text(filterData[0].UnitName);
                    $formEl.find("#lblBrand").text(filterData[0].BrandName);

                    if (filterData[0].Param1Value != undefined) {
                        masterData.ProcessMachineList[0].ParamValueEntry = String(filterData[0].Param1Value);
                    }
                    if (filterData[0].Param2Value != undefined) {
                        masterData.ProcessMachineList[1].ParamValueEntry = String(filterData[0].Param2Value);
                    }
                    if (filterData[0].Param3Value != undefined) {
                        masterData.ProcessMachineList[2].ParamValueEntry = String(filterData[0].Param3Value);
                    }
                    if (filterData[0].Param4Value != undefined) {
                        masterData.ProcessMachineList[3].ParamValueEntry = String(filterData[0].Param4Value);
                    }
                    if (filterData[0].Param5Value != undefined) {
                        masterData.ProcessMachineList[4].ParamValueEntry = String(filterData[0].Param5Value);
                    }
                    if (filterData[0].Param6Value != undefined) {
                        masterData.ProcessMachineList[5].ParamValueEntry = String(filterData[0].Param6Value);
                    }
                    if (filterData[0].Param7Value != undefined) {
                        masterData.ProcessMachineList[6].ParamValueEntry = String(filterData[0].Param7Value);
                    }
                    if (filterData[0].Param8Value != undefined) {
                        masterData.ProcessMachineList[7].ParamValueEntry = String(filterData[0].Param8Value);
                    }
                    if (filterData[0].Param9Value != undefined) {
                        masterData.ProcessMachineList[8].ParamValueEntry = String(filterData[0].Param9Value);
                    }
                    if (filterData[0].Param10Value != undefined) {
                        masterData.ProcessMachineList[9].ParamValueEntry = String(filterData[0].Param10Value);
                    }
                    if (filterData[0].Param11Value != undefined) {
                        masterData.ProcessMachineList[10].ParamValueEntry = String(filterData[0].Param11Value);
                    }
                    if (filterData[0].Param12Value != undefined) {
                        masterData.ProcessMachineList[11].ParamValueEntry = String(filterData[0].Param12Value);
                    }
                    if (filterData[0].Param13Value != undefined) {
                        masterData.ProcessMachineList[12].ParamValueEntry = String(filterData[0].Param13Value);
                    }
                    if (filterData[0].Param14Value != undefined) {
                        masterData.ProcessMachineList[13].ParamValueEntry = String(filterData[0].Param14Value);
                    }
                    if (filterData[0].Param15Value != undefined) {
                        masterData.ProcessMachineList[14].ParamValueEntry = String(filterData[0].Param15Value);
                    }
                    if (filterData[0].Param16Value != undefined) {
                        masterData.ProcessMachineList[15].ParamValueEntry = String(filterData[0].Param15Value);
                    }
                    if (filterData[0].Param17Value != undefined) {
                        masterData.ProcessMachineList[16].ParamValueEntry = String(filterData[0].Param15Value);
                    }
                    if (filterData[0].Param18Value != undefined) {
                        masterData.ProcessMachineList[17].ParamValueEntry = String(filterData[0].Param15Value);
                    }
                    if (filterData[0].Param19Value != undefined) {
                        masterData.ProcessMachineList[18].ParamValueEntry = String(filterData[0].Param15Value);
                    }
                    if (filterData[0].Param20Value != undefined) {
                        masterData.ProcessMachineList[19].ParamValueEntry = String(filterData[0].Param15Value);
                    }
                }
                else {
                    filterData = $.grep(masterData.PreFinishingProcessChilds, function (h) {
                        return h.FPChildID == selectedId
                    })

                    $formEl.find("#lblMachineNo").text(filterData[0].MachineNo);
                    $formEl.find("#lblUnit").text(filterData[0].UnitName);
                    $formEl.find("#lblBrand").text(filterData[0].BrandName);

                    if (filterData[0].Param1Value != undefined) {
                        masterData.ProcessMachineList[0].ParamValueEntry = String(filterData[0].Param1Value);
                    }
                    if (filterData[0].Param2Value != undefined) {
                        masterData.ProcessMachineList[1].ParamValueEntry = String(filterData[0].Param2Value);
                    }
                    if (filterData[0].Param3Value != undefined) {
                        masterData.ProcessMachineList[2].ParamValueEntry = String(filterData[0].Param3Value);
                    }
                    if (filterData[0].Param4Value != undefined) {
                        masterData.ProcessMachineList[3].ParamValueEntry = String(filterData[0].Param4Value);
                    }
                    if (filterData[0].Param5Value != undefined) {
                        masterData.ProcessMachineList[4].ParamValueEntry = String(filterData[0].Param5Value);
                    }
                    if (filterData[0].Param6Value != undefined) {
                        masterData.ProcessMachineList[5].ParamValueEntry = String(filterData[0].Param6Value);
                    }
                    if (filterData[0].Param7Value != undefined) {
                        masterData.ProcessMachineList[6].ParamValueEntry = String(filterData[0].Param7Value);
                    }
                    if (filterData[0].Param8Value != undefined) {
                        masterData.ProcessMachineList[7].ParamValueEntry = String(filterData[0].Param8Value);
                    }
                    if (filterData[0].Param9Value != undefined) {
                        masterData.ProcessMachineList[8].ParamValueEntry = String(filterData[0].Param9Value);
                    }
                    if (filterData[0].Param10Value != undefined) {
                        masterData.ProcessMachineList[9].ParamValueEntry = String(filterData[0].Param10Value);
                    }
                    if (filterData[0].Param11Value != undefined) {
                        masterData.ProcessMachineList[10].ParamValueEntry = String(filterData[0].Param11Value);
                    }
                    if (filterData[0].Param12Value != undefined) {
                        masterData.ProcessMachineList[11].ParamValueEntry = String(filterData[0].Param12Value);
                    }
                    if (filterData[0].Param13Value != undefined) {
                        masterData.ProcessMachineList[12].ParamValueEntry = String(filterData[0].Param13Value);
                    }
                    if (filterData[0].Param14Value != undefined) {
                        masterData.ProcessMachineList[13].ParamValueEntry = String(filterData[0].Param14Value);
                    }
                    if (filterData[0].Param15Value != undefined) {
                        masterData.ProcessMachineList[14].ParamValueEntry = String(filterData[0].Param15Value);
                    }
                    if (filterData[0].Param16Value != undefined) {
                        masterData.ProcessMachineList[15].ParamValueEntry = String(filterData[0].Param15Value);
                    }
                    if (filterData[0].Param17Value != undefined) {
                        masterData.ProcessMachineList[16].ParamValueEntry = String(filterData[0].Param15Value);
                    }
                    if (filterData[0].Param18Value != undefined) {
                        masterData.ProcessMachineList[17].ParamValueEntry = String(filterData[0].Param15Value);
                    }
                    if (filterData[0].Param19Value != undefined) {
                        masterData.ProcessMachineList[18].ParamValueEntry = String(filterData[0].Param15Value);
                    }
                    if (filterData[0].Param20Value != undefined) {
                        masterData.ProcessMachineList[19].ParamValueEntry = String(filterData[0].Param15Value);
                    }
                }

                initMachineParamTable();

                if (machinetype == 'machine') {
                    for (var i = 0; i < masterData.ProcessMachineList.length; i++) {
                        if (masterData.ProcessMachineList[i].ProcessType == "Pre Set") {
                            tblMachineParamId.bootstrapTable('hideRow', { index: i });
                        }
                    }
                }
                else {
                    for (var i = 0; i < masterData.ProcessMachineList.length; i++) {
                        if (masterData.ProcessMachineList[i].ProcessType == "Post Set") {
                            tblMachineParamId.bootstrapTable('hideRow', { index: i });
                        }
                    }
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function setChildData(row) {
        //debugger
        var item;
        $.each(row, function (obj, key) {
            var paramName = key.ParamName;

            if (key.ParamValueEntry == null) {
                paramValue = key.DefaultValue;
            }
            else {
                paramValue = key.ParamValueEntry;
            }
            if (machinetype == 'machine') {
                var item = masterData.PostFinishingProcessChilds.filter(function (el) {
                    return el.FPChildID == selectedId && el.ColorID == colorId
                });
                item[0].FMSID = key.FMSID;
                item[0].MachineNo = key.MachineNo;
                item[0][paramName] = paramValue;
                cGrid.updateRow(selectedId, item[0]);
                //cGrid.updateRow(colorId, item[0]);
                cGrid.refreshColumns;
            }
            else {
                var item = masterData.PreFinishingProcessChilds.filter(function (el) {
                    return el.FPChildID == selectedId
                });
                item[0].FMSID = key.FMSID;
                item[0].MachineNo = key.MachineNo;
                item[0][paramName] = paramValue;
                var index = $tblChildEl.getRowIndexByPrimaryKey(key.FMSID);
                $tblChildEl.editModule.updateRow(index, item[0]);
            }
        });

        //var paramName = row.ParamName;
        //var paramValue = row.ParamValueEntry;
        //if (machinetype == 'machine') {
        //    var item = masterData.PostFinishingProcessChilds.filter(function (el) { return el.FPChildID == selectedId });
        //    item[0].FMSID = row.FMSID;
        //    item[0].MachineNo = row.MachineNo;
        //    item[0][paramName] = paramValue;
        //    var index = $tblPostChildEl.getRowIndexByPrimaryKey(row.FMSID);
        //    $tblPostChildEl.editModule.updateRow(index, item[0]);
        //}
        //else {
        //    var item = masterData.PreFinishingProcessChilds.filter(function (el) { return el.FPChildID == selectedId });
        //    item[0].FMSID = row.FMSID;
        //    item[0].MachineNo = row.MachineNo;
        //    item[0][paramName] = paramValue;
        //    var index = $tblChildEl.getRowIndexByPrimaryKey(row.FMSID);
        //    $tblChildEl.editModule.updateRow(index, item[0]);
        //}
    }

    function initMachineParamTable() {
        tblMachineParamId.bootstrapTable("destroy");
        tblMachineParamId.bootstrapTable({
            editable: true,
            columns: [
                {
                    field: "MachineNo",
                    title: "Machine No",
                    align: 'center',
                    visible: false
                },
                {
                    field: "ParamName",
                    title: "Param Name",
                    align: 'center',
                    visible: false
                },
                {
                    field: "ProcessType",
                    title: "Process Type",
                    align: 'center'
                },
                {
                    field: "NeedItem",
                    title: "Need Item",
                    align: 'center',
                    visible: false
                },
                {
                    field: "ParamDispalyName",
                    title: "Param Name",
                    align: 'center',
                },
                {
                    field: "ParamValueEntry",
                    title: "Input Value",
                    filterControl: "input",
                    align: 'center',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" style="padding-right: 10px;">'
                    },
                    formatter: function (value, row, index, field) {
                        if (value != null) {
                            return value;
                        }
                        else if (row.ParamValue === 'N/A') {
                            return 'N/A';
                        }
                        else if (value == 'undefined' && row.DefaultValue != 'undefined' || row.DefaultValue != 'Empty') {
                            return row.DefaultValue;
                        }
                    }
                },
                {
                    field: "ParamValue",
                    title: "Range Value",
                    align: 'center'
                },
                {
                    field: "DefaultValue",
                    title: "Default Value",
                    align: 'center',
                    visible: false
                },
                {
                    field: "",
                    title: "Add Item",
                    filterControl: "input",
                    align: 'center',
                    formatter: function (value, row, index, field) {
                        return row.NeedItem == true ? '<button type="button" id="btnAddItem" class="edit">Add Item</button >' : "";
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();

                            getChemicalItem(row, index);
                        },
                    }
                }
            ],
            data: masterData.ProcessMachineList,
            onEditableSave: function (field, row, oldValue, $el) {
                if (row.ParamValue == "Yes/No") {
                    if (row.ParamValueEntry !== "Yes" && row.ParamValueEntry !== "No") {
                        showBootboxAlert('Please enter Yes or No');
                        row.ParamValueEntry = '';
                        tblMachineParamId.bootstrapTable('load', masterData.ProcessMachineList);
                    }
                    else {
                        //setChildData(row);
                    }
                }
                else if (row.ParamValue == "On/Off") {
                    if (row.ParamValueEntry !== "On" && row.ParamValueEntry !== "Off") {
                        showBootboxAlert('Please enter On or Off');
                        row.ParamValueEntry = '';
                        tblMachineParamId.bootstrapTable('load', masterData.ProcessMachineList);
                    }
                    else {
                        //setChildData(row);
                    }
                }
                else if (row.ParamValue == "N/A") {
                    if (row.ParamValueEntry != "N/A") {
                        showBootboxAlert('Please enter N/A');
                        row.ParamValueEntry = '';
                        tblMachineParamId.bootstrapTable('load', masterData.ProcessMachineList);
                    }
                    else {
                        //setChildData(row);
                    }
                }
                else if (row.ParamValue.includes("to")) {
                    var min = row.ParamValue.split('to').shift();
                    var max = row.ParamValue.substring(row.ParamValue.indexOf('o') + 1, row.ParamValue.indexOf('o') + 6);
                    if (isNaN(parseInt(row.ParamValueEntry)) || parseInt(row.ParamValueEntry) < parseInt(min) || parseInt(row.ParamValueEntry) > parseInt(max)) {
                        showBootboxAlert('Enter the value between ' + min + ' and ' + max);
                        row.ParamValueEntry = '';
                        tblMachineParamId.bootstrapTable('load', masterData.ProcessMachineList);
                    }
                    else {
                        //setChildData(row);
                    }
                }
                else if (row.ParamValue.split('-').shift() >= 0 && row.ParamValue != '1200-1700') {
                    var min = row.ParamValue.split('-').shift();
                    var max = row.ParamValue.substring(row.ParamValue.indexOf('-') + 1, row.ParamValue.indexOf('-') + 4);
                    if (isNaN(parseInt(row.ParamValueEntry)) || parseInt(row.ParamValueEntry) < parseInt(min) || parseInt(row.ParamValueEntry) > parseInt(max)) {
                        showBootboxAlert('Enter the value between ' + min + ' and ' + max);
                        row.ParamValueEntry = '';
                        tblMachineParamId.bootstrapTable('load', masterData.ProcessMachineList);
                    }
                    else {
                        //setChildData(row);
                    }
                }
                else if (row.ParamValue == '1200-1700') {
                    var min = row.ParamValue.split('-').shift();
                    var max = row.ParamValue.substring(row.ParamValue.indexOf('-') + 1, row.ParamValue.indexOf('-') + 5);
                    if (isNaN(parseInt(row.ParamValueEntry)) || parseInt(row.ParamValueEntry) < parseInt(min) || parseInt(row.ParamValueEntry) > parseInt(max)) {
                        showBootboxAlert('Enter the value between ' + min + ' and ' + max);
                        row.ParamValueEntry = '';
                        tblMachineParamId.bootstrapTable('load', masterData.ProcessMachineList);
                    }
                    else {
                        //setChildData(row);
                    }
                }
                else if (row.ParamValue == '1 Time/ 2 Times') {
                    if (row.ParamValueEntry !== "1 Time" && row.ParamValueEntry !== "2 Times") {
                        showBootboxAlert("Please enter '1 Time' or '2 Times'");
                        row.ParamValueEntry = '';
                        tblMachineParamId.bootstrapTable('load', masterData.ProcessMachineList);
                    }
                    else {
                        //setChildData(row);
                    }
                }
                else if (row.ParamValue == 'Tube Open/ Bag Sewing Open/ Plaiting') {
                    if (row.ParamValueEntry !== "Tube Open" && row.ParamValueEntry !== "Bag Sewing Open" && row.ParamValueEntry !== "Plaiting") {
                        showBootboxAlert("Please enter 'Tube Open' or 'Bag Sewing Open' or 'Plaiting'");
                        row.ParamValueEntry = '';
                        tblMachineParamId.bootstrapTable('load', masterData.ProcessMachineList);
                    }
                    else {
                        //setChildData(row);
                    }
                }
                else {
                    //setChildData(row);
                }
                setChildData(masterData.ProcessMachineList);
            },
        });
    }
    function getChemicalItem(rowData, index) {
        var item;
        if (machinetype == 'machine') {
            item = masterData.PostFinishingProcessChilds.filter(function (el) { return el.FPChildID == selectedId && el.ColorID == colorId });
        }
        else {
            item = masterData.PreFinishingProcessChilds.filter(function (el) { return el.FPChildID == selectedId });
        }
        if (!item[0].MachineParams || item[0].MachineParams.length == 0) {
            item[0]['MachineParams'] = [];
            var url;
            if (status != (statusConstants.PRE_PENDING || statusConstants.POST_PENDING)) {
                url = `/api/finishing-process/raw-item-by-type/'Chemical'/${item[0].FPChildID}`;
            } else {
                url = `/api/finishing-process/raw-item-by-type/'Chemical'/0`;
            }
            axios.get(url)
                .then(function (response) {
                    item[0].MachineParams = response.data.Items;

                    var finder = new commonFinder({
                        title: "Select Item",
                        pageId: pageId,
                        fields: "text,desc",
                        headerTexts: "Item Name,Item Qty(gm/l)",
                        widths: "70,30",
                        editTypes: ",numericedit",
                        allowEditing: true,
                        //autofitColumns: true,
                        //apiEndPoint: url,
                        data: item[0].MachineParams,
                        isMultiselect: true,
                        selectedIds: item[0].ItemIDs,
                        allowPaging: true,
                        primaryKeyColumn: "id",
                        onMultiselect: function (selectedRecords) {
                            if (machinetype == 'machine') {
                                for (var i = 0; i < selectedRecords.length; i++) {
                                    item[0].PreFinishingProcessChildItems[i] = {};
                                    item[0].PreFinishingProcessChildItems[i]["FPChildID"] = item[0].FPChildID;
                                    item[0].PreFinishingProcessChildItems[i]["ItemMasterID"] = selectedRecords[i].id;
                                    item[0].PreFinishingProcessChildItems[i]["text"] = selectedRecords[i].text;
                                    item[0].PreFinishingProcessChildItems[i]["Qty"] = (!selectedRecords[i].desc) ? 0 : parseInt(selectedRecords[i].desc);

                                    var obj = item[0].MachineParams.find(function (el) { return el.id == selectedRecords[i].id; });
                                    obj.desc = selectedRecords[i].desc;
                                }
                                item[0].ItemIDs = selectedRecords.map(function (el) { return el.id }).toString();
                                //var index = $tblPostChildEl.getRowIndexByPrimaryKey(rowData.FMSID);
                                //$tblPostChildEl.editModule.updateRow(index, item[0]);
                                //$tblPostChildEl.refreshColumns;
                            }
                            else {
                                for (var i = 0; i < selectedRecords.length; i++) {
                                    item[0].PreFinishingProcessChildItems[i] = {};
                                    item[0].PreFinishingProcessChildItems[i]["FPChildID"] = item[0].FPChildID;
                                    item[0].PreFinishingProcessChildItems[i]["ItemMasterID"] = selectedRecords[i].id;
                                    item[0].PreFinishingProcessChildItems[i]["text"] = selectedRecords[i].text;
                                    item[0].PreFinishingProcessChildItems[i]["Qty"] = (!selectedRecords[i].desc) ? 0 : parseInt(selectedRecords[i].desc);

                                    var obj = item[0].MachineParams.find(function (el) { return el.id == selectedRecords[i].id; });
                                    obj.desc = selectedRecords[i].desc;
                                }
                                item[0].ItemIDs = selectedRecords.map(function (el) { return el.id }).toString();
                                var index = $tblChildEl.getRowIndexByPrimaryKey(rowData.FMSID);
                                $tblChildEl.editModule.updateRow(index, item[0]);
                                $tblChildEl.refreshColumns;
                            }
                        },
                        onFinish: function () {
                            finder.hideModal();
                        }
                    });
                    finder.showModal();
                })
                .catch(function (err) {
                    toastr.error(err.response.data.Message);
                });
        }
        else {
            var finder = new commonFinder({
                title: "Select Item",
                pageId: pageId,
                fields: "text,desc",
                headerTexts: "Item Name,Item Qty",
                widths: "70,30",
                editTypes: ",numericedit",
                allowEditing: true,
                //autofitColumns: true,
                //apiEndPoint: url,
                data: item[0].MachineParams,
                isMultiselect: true,
                selectedIds: item[0].ItemIDs,
                allowPaging: true,
                primaryKeyColumn: "id",
                onMultiselect: function (selectedRecords) {
                    if (machinetype == 'machine') {
                        for (var i = 0; i < selectedRecords.length; i++) {
                            item[0].PreFinishingProcessChildItems[i] = {};
                            item[0].PreFinishingProcessChildItems[i]["FPChildID"] = item[0].FPChildID;
                            item[0].PreFinishingProcessChildItems[i]["ItemMasterID"] = selectedRecords[i].id;
                            item[0].PreFinishingProcessChildItems[i]["text"] = selectedRecords[i].text;
                            item[0].PreFinishingProcessChildItems[i]["Qty"] = (!selectedRecords[i].desc) ? 0 : parseInt(selectedRecords[i].desc);

                            var obj = item[0].MachineParams.find(function (el) { return el.id == selectedRecords[i].id; });
                            obj.desc = selectedRecords[i].desc;
                        }
                        item[0].ItemIDs = selectedRecords.map(function (el) { return el.id }).toString();
                        //var index = $tblPostChildEl.getRowIndexByPrimaryKey(rowData.FMSID);
                        //$tblPostChildEl.editModule.updateRow(index, item[0]);
                        //$tblPostChildEl.refreshColumns;
                    }
                    else {
                        for (var i = 0; i < selectedRecords.length; i++) {
                            item[0].PreFinishingProcessChildItems[i] = {};
                            item[0].PreFinishingProcessChildItems[i]["FPChildID"] = item[0].FPChildID;
                            item[0].PreFinishingProcessChildItems[i]["ItemMasterID"] = selectedRecords[i].id;
                            item[0].PreFinishingProcessChildItems[i]["text"] = selectedRecords[i].text;
                            item[0].PreFinishingProcessChildItems[i]["Qty"] = (!selectedRecords[i].desc) ? 0 : parseInt(selectedRecords[i].desc);

                            var obj = item[0].MachineParams.find(function (el) { return el.id == selectedRecords[i].id; });
                            obj.desc = selectedRecords[i].desc;
                        }
                        var index = $tblChildEl.getRowIndexByPrimaryKey(rowData.FMSID);
                        $tblChildEl.editModule.updateRow(index, item[0]);
                        $tblChildEl.refreshColumns;
                    }
                },
                onFinish: function () {
                    finder.hideModal();
                }
            });
            finder.showModal();
        }
    }

    function initSaveMachineTable() {
        tblSaveMachineId.bootstrapTable("destroy");
        tblSaveMachineId.bootstrapTable({
            columns: saveChildGridcolumnList,
            onDblClickRow: function (row, $el, field) {
                getMachineParam(row.FMSID);
                $("#modal-machine").modal('show');
                $formEl.find("#lblMachineNo").text(row.MachineNo);
                $formEl.find("#lblUnit").text(row.UnitName);
                $formEl.find("#lblBrand").text(row.BrandName);
                $("#modal-save-child").modal('hide');
            }
        });
    }

    function generateSaveChildGrid(row, FMSID) {
        saveChildGridcolumnList = [];
        saveChildGridcolumnList.push(
            {
                field: "Id",
                isPrimaryKey: true,
                visible: false
            },
            {
                field: "MachineNo",
                title: "Machine No",
                align: 'center',
                editable: false
            },
            {
                field: "UnitName",
                title: "Unit",
                align: 'center',
                editable: false
            },
            {
                field: "BrandName",
                title: "Brand",
                align: 'center',
                editable: false
            }
        );

        var filterData = $.grep(masterData.FinishingMachineConfigurationChildList, function (h) {
            return h.FMCMasterID == row.FMCMasterID
        });
        for (var i = 0; i < filterData.length; i++) {
            var column = {
                field: `Param${i + 1}Value`,
                title: filterData[i].ParamDisplayName,
                align: "center",
                editable: {
                    type: "text",
                    showbuttons: false,
                    tpl: '<input type="text" class="form-control input-sm" style="padding-right: 10px;">'
                }
            }
            saveChildGridcolumnList.push(column);
            $formEl.find("#lbl" + (i + 1)).text(filterData[i].ParamDisplayName);
        }
        initSaveMachineTable();
        getSaveMachines(FMSID);
        $("#modal-machine").modal('show');
    }

    function initChildTable(data) {
        if ($tblChildEl) {
            $tblChildEl.destroy();
            $(tblChildId).html("");
        }
        ej.base.enableRipple(true);
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowRowDragAndDrop: true,
            selectionSettings: { type: 'Multiple' },
            editSettings: { allowAdding: true, allowDeleting: true, allowEditing: true },  //allowAdding: true, allowEditing: true,
            commandClick: commandClick,
            columns: [
                {
                    headerText: '', width: 30, commands: [
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }
                    ]
                },
                { field: 'FPChildID', isPrimaryKey: true, visible: false },
                { field: 'ProcessID', visible: false },
                { field: 'ProcessTypeID', visible: false },
                { field: 'ProcessName', headerText: 'Process Name', width: 100, allowEditing: false },
                { field: 'ProcessType', headerText: 'Process Type..', width: 100, allowEditing: false },
                { field: 'MachineName', headerText: 'Machine Name', width: 100, allowEditing: false },
                { field: 'MachineNo', headerText: 'Machine No', width: 80, allowEditing: false },
                {
                    headerText: '...', width: 50, commands: [
                        { buttonOption: { type: 'dmachine', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
                    ]
                },
                { field: 'UnitName', headerText: 'Unit', width: 80, allowEditing: false },
                { field: 'BrandName', headerText: 'Brand', width: 80, allowEditing: false },
                { field: 'Param1Value', headerText: 'Param 1 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param2Value', headerText: 'Param 2 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param3Value', headerText: 'Param 3 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param4Value', headerText: 'Param 4 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param5Value', headerText: 'Param 5 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param6Value', headerText: 'Param 6 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param7Value', headerText: 'Param 7 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param8Value', headerText: 'Param 8 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param9Value', headerText: 'Param 9 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param10Value', headerText: 'Param 10 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param11Value', headerText: 'Param 11 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param12Value', headerText: 'Param 12 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param13Value', headerText: 'Param 13 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param14Value', headerText: 'Param 14 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param15Value', headerText: 'Param 15 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param16Value', headerText: 'Param 16 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param17Value', headerText: 'Param 17 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param18Value', headerText: 'Param 18 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param19Value', headerText: 'Param 19 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param20Value', headerText: 'Param 20 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Remarks', headerText: 'Remarks', width: 100, allowEditing: true }
            ],
            childGrid: {
                queryString: 'FPChildID',
                allowResizing: true,
                editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: [
                    {
                        headerText: 'Action', width: 60, visible: status == (statusConstants.PRE_PENDING || statusConstants.POST_PENDING), commands: [
                            { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                            { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                            { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                            { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                        ]
                    },
                    { field: 'ItemMasterID', isPrimaryKey: true, visible: false },
                    { field: 'text', headerText: 'Item Name', textAlign: 'Center', allowEditing: false },
                    { field: 'Qty', headerText: 'Qty(gm/l)' }
                ],
                load: loadFirstLevelChildGrid
            }
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }

    function loadFirstLevelChildGrid() {
        this.dataSource = this.parentDetails.parentRowData.PreFinishingProcessChildItems;
    }

    var selectedId = 0;
    var colorId = 0;
    var machinetype = '';
    var selectedData = [];

    function commandClick(e) {
        cGrid = null;
        masterData.ProcessMachineList = [];
        $formEl.find("#lblMachineNo").text('');
        $formEl.find("#lblUnit").text('');
        $formEl.find("#lblBrand").text('');
        var data = e.rowData;
        selectedData = data;
        machinetype = e.commandColumn.buttonOption.type;
        if (e.commandColumn.buttonOption.type == 'machine') {
            //debugger
            cGrid = this;
            selectedId = data.FPChildID;
            colorId = data.ColorID;
            generateSaveChildGrid(data, 0);
            $("#modal-save-child").modal('hide');
            $("#modal-machine").modal('show');
        }
        else if (e.commandColumn.buttonOption.type == 'dmachine') {
            cGrid = this;
            selectedId = data.FPChildID;
            colorId = data.ColorID;
            generateSaveChildGrid(data, 0);
            $("#modal-save-child").modal('hide');
            $("#modal-machine").modal('show');
        }
        else if (e.commandColumn.buttonOption.type == 'colormachine') {
            //var color = e.commandColumn.rowData.ColorID;
            //alert(color);
            // e.preventDefault();
            var finder = new commonFinder({
                title: "Select Process",
                pageId: pageId,
                data: masterData.PostProcessList,
                fields: "ProcessName,ProcessType,MachineName",
                headerTexts: "Process Name, Process Type,Machine Name",
                isMultiselect: true,
                allowPaging: false,
                primaryKeyColumn: "ProcessID",
                onMultiselect: function (selectedRecords) {
                    for (var i = 0; i < selectedRecords.length; i++) {
                        var oPreProcess = {
                            FPChildID: getMaxIdForArray(masterData.PostFinishingProcessChilds, "FPChildID"),
                            FPMasterID: 0,
                            ProcessID: selectedRecords[i].ProcessID,
                            ProcessTypeID: selectedRecords[i].ProcessTypeID,
                            ProcessName: selectedRecords[i].ProcessName,
                            ProcessType: selectedRecords[i].ProcessType,
                            MachineName: selectedRecords[i].MachineName,
                            FMCMasterID: selectedRecords[i].FMCMasterID,
                            MachineNo: "",
                            UnitName: "",
                            BrandName: "",
                            Remarks: "",
                            ColorID: 0,
                            FMSID: null,
                            Param1Value: null,
                            Param2Value: null,
                            Param3Value: null,
                            Param4Value: null,
                            Param5Value: null,
                            Param6Value: null,
                            Param7Value: null,
                            Param8Value: null,
                            Param9Value: null,
                            Param10Value: null,
                            Param11Value: null,
                            Param12Value: null,
                            Param13Value: null,
                            Param14Value: null,
                            Param15Value: null,
                            Param16Value: null,
                            Param17Value: null,
                            Param18Value: null,
                            Param19Value: null,
                            Param20Value: null,
                            PreFinishingProcessChildItems: []
                        }
                        masterData.PostFinishingProcessChilds.push(oPreProcess);
                    }
                    initChildTableColor(masterData.PostFinishingProcessChilds);
                }
            });
            finder.showModal();
        }
    }

    function SetMachine() {
        $("#modal-save-child").modal('hide');
        $("#modal-machine").modal('hide');
        generateChildGrid(selectedData);
        $("#modal-child").modal('show');
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#FPMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(conceptId, isBDS, grpConceptNo) {
        axios.get(`/api/finishing-process/new/${conceptId}/${isBDS}/${grpConceptNo}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
                masterData.PFBatchDate = formatDateToDefault(masterData.PFBatchDate);
                setFormData($formEl, masterData);
                initChildTable([]);
                //initChildTableColor([]);

                initChildTableColor(masterData.ColorList);

                copiedRecord = null;
                if (!masterData.NeedPreFinishingProcess) {
                    $formEl.find("#btnAddPreProcess").fadeOut();
                    $formEl.find("#divNotPreDying").fadeIn();
                } else {
                    $formEl.find("#btnAddPreProcess").fadeIn();
                    $formEl.find("#divNotPreDying").fadeOut();
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id, conceptId, isBDS, grpConceptNo) {
        axios.get(`/api/finishing-process/${id}/${conceptId}/${isBDS}/${grpConceptNo}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
                masterData.PFBatchDate = formatDateToDefault(masterData.PFBatchDate);
                setFormData($formEl, masterData);
                copiedRecord = null;
                if (!masterData.NeedPreFinishingProcess) {
                    $formEl.find("#btnAddPreProcess").fadeOut();
                    $formEl.find("#divNotPreDying").fadeIn();
                    initChildTable([]);
                    initChildTableColor(masterData.PostFinishingProcessChildColors);
                } else {
                    $formEl.find("#btnAddPreProcess").fadeIn();
                    $formEl.find("#divNotPreDying").fadeOut();
                    initChildTable(masterData.PreFinishingProcessChilds);
                    initChildTableColor(masterData.PostFinishingProcessChildColors);
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getMachines(ProcessID, ProcessTypeID) {
        axios.get(`/api/finishing-machine-setup/machinelist?processId=${ProcessID}&processTypeId=${ProcessTypeID}`)
            .then(function (response) {
                var machineData = response.data;
                tblMachineId.bootstrapTable("load", machineData);
                tblMachineId.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getSaveMachines(FMSID) {
        if (machinetype == 'machine') {
            var filterData = $.grep(masterData.PostFinishingProcessChilds, function (h) {
                return h.FPChildID == selectedId && h.ColorID == colorId
            })
            if (filterData.length > 0) {
                if (filterData[0].FMSID > 0) {
                    getMachineParam(filterData[0].FMSID);
                }
            }
            else {
                getMachineParam(FMSID);
            }
            initMachineParamTable();
        }
        else {
            var filterData = $.grep(masterData.PreFinishingProcessChilds, function (h) {
                return h.FPChildID == selectedId && h.ColorID == colorId
            })
            if (filterData.length > 0) {
                if (filterData[0].FMSID > 0) {
                    getMachineParam(filterData[0].FMSID);
                }
            }
            initMachineParamTable();
        }
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data["PreFinishingProcessChilds"] = $tblChildEl.getCurrentViewRecords();
        data["PostFinishingProcessChilds"] = masterData.PostFinishingProcessChilds;

        if (data.PostFinishingProcessChilds != null && data.PostFinishingProcessChilds.length > 0) {
            data.PostFinishingProcessChilds.map(x => {
                if (x.FMSID == null) x.FMSID = 0;
            });
        }
        //data["PostFinishingProcessChilds"] = $tblColorChildEl.getDataRows();
        if (data.PreFinishingProcessChilds.length < 1) {
            data.PDProductionComplete = true;
        }
        axios.post("/api/finishing-process/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();


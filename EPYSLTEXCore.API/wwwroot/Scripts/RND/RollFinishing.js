(function () {
    var menuId, pageName;
    var toolbarId;
    var pageId, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $tblChildEl, tblChildId, tblChildItemsId,
        $tblRollEl, $formEl, tblSearchModalId, $tblSearchModalEl, $tblMachineInfoEl, tblMachineInfoId,
        tblMachineId, tblOtherItemId, $tblOtherItemEl, $tblChildItemsRF;

    var status = statusConstants.PENDING;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var dBCFPID = 0, segmentNo = 0, fpMasterID = 0, colorId = 0, machineId = 0;
    var selectedChildItemsRF = [];
    var masterData, pendingData;
    var childGridcolumnList = [];

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
        $tblRollEl = $("#tblRoll" + pageId);
        tblMachineId = $("#tblMachine" + pageId);
        tblChildItemsId = "#tblChildItems" + pageId;
        tblSearchModalId = "#tblSearchModalId";
        tblMachineInfoId = "#tblMachineInfo" + pageId;
        tblOtherItemId = "#tblOtherItem" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        initMasterTable();

        $toolbarEl.find("#btnPendingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            initMasterTable();
        });
        $toolbarEl.find("#btnCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            initMasterTable();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#btnSelectMachine").on("click", function (e) {
            e.preventDefault();
            ShowMachine();
        });

        $formEl.find("#btnProductionComplete").on("click", function (e) {
            e.preventDefault();
            ProductionComplete();
        });
        $formEl.find("#btnUpdateProcess").on("click", function (e) {
            e.preventDefault();
            UpdateFinishingProcess();
        });
        $formEl.find("#btnLoadChildItemsRollFinishing").on("click", function (e) {
            getChemicalItemRF();
        });
    });

    function initMasterTable() {
        var commands = status == statusConstants.PENDING
            ? [{ type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }]
            : [{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }];

        var columns = [
            {
                headerText: 'Actions', commands: commands
            },
            {
                field: 'SubGroupName', headerText: 'End Use', visible: false
            },
            {
                field: 'ConceptNo', headerText: 'Concept No'
            },
            {
                field: 'ColorName', headerText: 'Color'
            },
            {
                field: 'KnittingType', headerText: 'Machine Type'
            },
            {
                field: 'Composition', headerText: 'Composition'
            },
            {
                field: 'Gsm', headerText: 'Gsm'
            },
            {
                field: 'Length', headerText: 'Length'
            },
            {
                field: 'Width', headerText: 'Height'
            },
            {
                field: 'TechnicalName', headerText: 'Technical Name'
            },
            {
                field: 'DBatchNo', headerText: 'D. Batch No'
            },
            {
                field: 'DBatchDate', headerText: 'D. Batch Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'ProductionDate', headerText: 'Production Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'BatchStartTime', headerText: 'Batch Start Time', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'BatchEndTime', headerText: 'Batch End Time', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'BatchWeightKG', headerText: 'Batch Weight (KG)'
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/roll-finishing/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        getDetails(args.rowData.DBIID);
    }

    //function initOtherItemTable(records, SubGroupName) {
    //    console.log(records);
    //    if ($tblOtherItemEl) $tblOtherItemEl.destroy();

    //    $tblOtherItemEl = new initEJ2Grid({
    //        tableId: tblOtherItemId,
    //        data: records,
    //        autofitColumns: false,
    //        //allowSorting: true,
    //        allowPaging: false,
    //        allowFiltering: false,
    //        //showDefaultToolbar: false,
    //        //enableSingleClickEdit: true,
    //        columns: [
    //            { field: 'GroupConceptNo', headerText: 'Group Concept No', visible:false },
    //            { field: 'FUPartName', headerText: 'End Use', visible: SubGroupName != 'Fabric' },
    //            { field: 'KnittingType', headerText: 'Machine Type' },
    //            { field: 'TechnicalName', headerText: 'Technical Name' },
    //            { field: 'Composition', headerText: 'Composition', visible: SubGroupName == 'Fabric' },
    //            { field: 'Gsm', headerText: 'Gsm', visible: SubGroupName == 'Fabric' },
    //            { field: 'Length', headerText: 'Length (CM)', visible: SubGroupName != 'Fabric' },
    //            { field: 'Width', headerText: 'Height (CM)', visible: SubGroupName != 'Fabric' }
    //        ]
    //    });
    //}

    function initChildTable(data) {
        if ($tblChildEl) {
            $tblChildEl.destroy();
            $(tblChildId).html("");
        }
        ej.base.enableRipple(true);
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            editSettings: { allowEditing: true },
            recordClick: function (args) {
                if (args.column.field == "Status") { //args.rowData.DBCFPID == pendingData.DBCFPID  //ProductionDate = null
                    
                    pendingData = args.rowData;

                    $formEl.find("#divProcessProduction").fadeIn();
                    if (pendingData.ProductionDate == null)
                        $formEl.find("#btnProductionComplete").fadeIn();
                    else
                        $formEl.find("#btnProductionComplete").fadeOut();

                    $formEl.find("#ProcessName").val(pendingData.ProcessName);
                    $formEl.find("#ProdDate").val(formatDateToDefault(pendingData.ProductionDate == null ? new Date() : pendingData.ProductionDate));
                    $formEl.find("#ShiftID").val(pendingData.ShiftID).trigger("change");
                    $formEl.find("#OperatorID").val(pendingData.OperatorID).trigger("change");
                    $formEl.find("#MachineNo").val(pendingData.MachineNo);
                    $formEl.find("#PMachineNo").val(pendingData.PMachineNo == null ? pendingData.MachineNo : pendingData.PMachineNo);
                    $formEl.find("#BrandName").val(pendingData.BrandName);
                    $formEl.find("#UnitName").val(pendingData.UnitName);
                    dBCFPID = pendingData.DBCFPID;

                    var nFMSID = pendingData.PFMSID == 0 ? pendingData.FMSID : pendingData.PFMSID;
                    getFinishingMCSetupInfo(nFMSID, pendingData.DBCFPID);
                } else {
                    $formEl.find("#divProcessProduction").fadeOut();
                }
            },
            columns: [
                { field: 'DBCFPID', isPrimaryKey: true, visible: false, allowEditing: false },
                { field: 'ProcessName', headerText: 'Process Name', width: 20, allowEditing: false },
                { field: 'ProcessType', headerText: 'Process Type', width: 20, allowEditing: false },
                { field: 'MachineName', headerText: 'Machine Name', width: 20, allowEditing: false },
                { field: 'MachineNo', headerText: 'Machine No', width: 20, allowEditing: false },
                { field: 'Status', headerText: 'Status', width: 20, allowEditing: false, valueAccessor: statusFormatter }
            ]
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }

    function statusFormatter(field, data, column) {
        column.disableHtmlEncode = false;
        if (data.ProductionDate == null) {
            return `<a class="btn btn-xs btn-default" onclick="return href="javascript:void(0)" title="Status">
                                    <i class="fa fa-tasks" aria-hidden="true"></i> Pending
                                </a>`;
        } else {
            return `<a class="btn btn-xs btn-default" onclick="return href="javascript:void(0)" title="Status">
                                    <i class="fa fa-tasks" aria-hidden="true"></i> Complete
                                </a>`;
        }
    }

    function getFinishingMCSetupInfo(fmsId, dbcFpId) {
        axios.get(`/api/roll-finishing/machine/${fmsId}/${dbcFpId}`)
            .then(function (response) {
                //for (var i = 0; i < response.data.ProcessMachineList.length; i++) {
                //    response.data.ProcessMachineList[i].ActulaPlanParamValue = pendingData[response.data.ProcessMachineList[i].ParamName];
                //}
                inittblMachineInfoTable(response.data.ProcessMachineList);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function inittblMachineInfoTable(data) {
        if ($tblMachineInfoEl) {
            $tblMachineInfoEl.destroy();
        }
        var columns = [
            {
                field: 'SerialNo',
                isPrimaryKey: true,
                visible: false
            },
            {
                field: "ParamDispalyName",
                headerText: "Param Name",
                allowEditing: false
            },
            {
                field: "ParamValue",
                headerText: "Default Value",
                allowEditing: false
            },
            {
                field: "PlanParamValue",
                headerText: "Plan Value",
                allowEditing: false
            },
            {
                field: "ActulaPlanParamValue",
                headerText: "Entry Value",
                allowEditing: true
            },
            {
                field: 'AddRecipe',
                headerText: 'Add Recipe',
                //width: 20,
                allowEditing: false,
                valueAccessor: AddRecipe
            }
        ]

        //if ($tblMachineInfoEl) $tblMachineInfoEl.destroy();
        //$tblMachineInfoEl = new initEJ2Grid({
        //    tableId: tblMachineInfoId,
        //    autofitColumns: false,
        //    allowSorting: false,
        //    allowPaging: false,
        //    allowFiltering: false,
        //    data: data,
        //    columns: columns,
        //    editSettings: { allowEditing: true, mode: "Normal" },
        //    recordClick: function (args) {
        //        if (args.column.field == "AddRecipe") {
        //            getChildItems(args.rowData, args.rowIndex);
        //        }
        //    },
        //});

        if ($tblMachineInfoEl) $tblMachineInfoEl.destroy();
        $tblMachineInfoEl = new ej.grids.Grid({
            dataSource: data,
            editSettings: { allowAdding: true, allowDeleting: true, allowEditing: true },  //allowAdding: true, allowEditing: true,
            recordClick: function (args) {
                if (args.column.field == "AddRecipe") {
                    getChildItemsRF(args.rowData, args.rowIndex);
                }
            },
            columns: columns
        });
        $tblMachineInfoEl.refreshColumns;
        $tblMachineInfoEl.appendTo(tblMachineInfoId);
    }

    function AddRecipe(field, data, column) {
        column.disableHtmlEncode = false;
        if (data.NeedItem) {
            return '<a class="btn btn-xs btn-default" data-toggle="modal" data-target="#dyeing-process-child-items" href="javascript:void(0)" title="Add Recipe"><i class="fa fa-tasks" aria-hidden="true"></i> Add Recipe</a>';
        }
    }

    function getChildItemsRF(rowData, index) {
        segmentNo = rowData.SerialNo;
        fpMasterID = rowData.FPMasterID;
        colorId = rowData.ColorID;
        machineId = rowData.FMSID;

        var url = `/api/dyeing-batch-child-finishing-process-child/get-dyeing-batch-child-items-color-machine-wise/${fpMasterID}/${colorId}/${machineId}/${segmentNo}/${dBCFPID}`;
        axios.get(url)
            .then(function (response) {
                var objList = response.data.Items;
                selectedChildItemsRF = [];
                loadChildItems(objList);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getSelectedChildItemsRF(objList) {
        var finalList = [];
        var childItems = [...selectedChildItemsRF, ...objList];
        if (childItems.length > 0) {
            var itemMasterIds = childItems.map(x => parseInt(x.ItemMasterID));
            itemMasterIds = itemMasterIds.filter((item, i, ar) => ar.indexOf(item) === i);
            itemMasterIds.map(itemMasterId => {
                var itemWiseItem = childItems.find(x => parseInt(x.ItemMasterID) == itemMasterId && x.ActualQty > 0);
                if (typeof itemWiseItem === "undefined" || itemWiseItem == null) {
                    itemWiseItem = childItems.find(x => parseInt(x.ItemMasterID) == itemMasterId);
                }
                finalList.push(itemWiseItem);
            });
        }
        return finalList;
    }

    function loadChildItems(objList) {
        selectedChildItemsRF = getSelectedChildItemsRF(objList);
        var columns = [
            {
                headerText: 'Action', width: 30, commands: [
                    { type: 'Delete1', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }
                ]
            },
            {
                field: 'ItemMasterID',
                isPrimaryKey: true,
                visible: false
            },
            {
                field: "ItemName",
                headerText: "Item Name",
                allowEditing: false,
                width: 100
            },
            {
                field: "Qty",
                headerText: "Recipe Qty(gm/l)",
                allowEditing: false,
                width: 40
            },
            {
                field: "ActualQty",
                headerText: "Actual Qty(gm/l)",
                allowEditing: true,
                width: 40
            }
        ]

        if ($tblChildItemsRF) $tblChildItemsRF.destroy();
        $tblChildItemsRF = new initEJ2Grid({
            tableId: tblChildItemsId,
            autofitColumns: false,
            columns: columns,
            data: selectedChildItemsRF,
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            commandClick: handleCommands1,
            actionComplete: function (e) {
                if (e.requestType == "save") {
                    var obj = e.data;
                    if (typeof obj.DBCFPID === "undefined") obj.DBCFPID = dBCFPID;
                    if (typeof obj.DBatchID === "undefined") obj.DBatchID = parseInt($formEl.find("#DBatchID").val());
                    if (typeof obj.SegmentNo === "undefined") obj.SegmentNo = segmentNo;
                    axios.post("/api/dyeing-batch-child-finishing-process-child/save", obj)
                        .then(function (response) {
                            var index = selectedChildItemsRF.findIndex(x => x.ItemMasterID === response.data.ItemMasterID);
                            if (index > -1) {
                                selectedChildItemsRF[index] = response.data;
                            }
                        })
                        .catch(function (error) {
                            toastr.error(error.response.data.Message);
                        });
                }
            }
        });
    }

    function handleCommands1(args) {
        var childItem = args.rowData;
        if (args.commandColumn.type == "Delete1") {
            var dBCFPCID = 0;
            var selectedChildItem = selectedChildItemsRF.find(x => x.ItemMasterID === childItem.ItemMasterID);
            if (typeof selectedChildItem !== "undefined" && selectedChildItem != null) {
                dBCFPCID = selectedChildItem.DBCFPCID;
            }
            if (typeof dBCFPCID !== "undefined" && dBCFPCID > 0) {
                axios.delete(`/api/dyeing-batch-child-finishing-process-child/delete-child/${dBCFPCID}`)
                    .then(function (response) {
                        if (response.data == "Deleted") {
                            var index = selectedChildItemsRF.findIndex(x => x.DBCFPCID === dBCFPCID);
                            if (index > -1) {
                                selectedChildItemsRF.splice(index, 1);
                                loadChildItems([]);
                            }
                        } else {
                            toastr.error(response.data);
                        }
                    })
                    .catch(showResponseError)
            } else {
                var index = selectedChildItemsRF.findIndex(x => x.ItemMasterID === childItem.ItemMasterID);
                if (index > -1) {
                    selectedChildItemsRF.splice(index, 1);
                    loadChildItems([]);
                }
            }
        }
    }

    function getChemicalItemRF() {
        var fpChildID = 0;
        var url = `/api/finishing-process/raw-item-by-type/'Chemical'/${fpChildID}`;
        axios.get(url)
            .then(function (response) {
                var list = response.data.Items;

                var finder = new commonFinder({
                    title: "Select Item",
                    pageId: pageId,
                    fields: "text,desc",
                    headerTexts: "Item Name,Item Qty(gm/l)",
                    widths: "70,30",
                    editTypes: ",",
                    allowEditing: true,
                    data: list,
                    isMultiselect: true,
                    //selectedIds: item[0].ItemIDs,
                    allowPaging: true,
                    primaryKeyColumn: "id",
                    onMultiselect: function (selectedRecords) {
                        var objList = [];
                        selectedRecords.map(x => {
                            objList.push({
                                ItemMasterID: parseInt(x.id),
                                ItemName: x.text,
                                Qty: 0,
                                ActualQty: 0
                            });
                        });
                        loadChildItems(objList);
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

    function ShowMachine() {
        generateChildGrid();
        $("#modal-machine-production").modal('show');
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
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

    function getDetails(dbiID) {
        axios.get(`/api/roll-finishing/${dbiID}/${status}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.DBatchDate = formatDateToDefault(masterData.DBatchDate);
                masterData.ProductionDate = formatDateToDefault(masterData.ProductionDate);
                masterData.BatchStartTime = formatDateToDefault(masterData.BatchStartTime);
                masterData.BatchEndTime = formatDateToDefault(masterData.BatchEndTime);
                setFormData($formEl, masterData);

                initChildTable(masterData.DyeingBatchChildFinishingProcesses);
                //console.log(masterData.OtherItems);

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
                //initOtherItemTable(masterData.OtherItems, masterData.SubGroupName);

                $formEl.find("#divProcessProduction").fadeOut();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function ShowMachine() {
        generateChildGrid();
        $("#modal-machine-finishing-production").modal('show');
    }

    function generateChildGrid() {
        childGridcolumnList = [
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
            },
            {
                field: "Capacity",
                title: "Capacity",
                align: 'center',
                editable: false
            }
        ];
        var column;
        var filterData = $.grep(masterData.FinishingMachineConfigurationChildList, function (h) { return h.FMCMasterID == pendingData.FMCMasterID });
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
        getMachines(pendingData.ProcessID, pendingData.ProcessTypeID)
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

    function initMachineTable() {
        tblMachineId.bootstrapTable("destroy");
        tblMachineId.bootstrapTable({
            showFooter: true,
            columns: childGridcolumnList
        });

        tblMachineId.on('dbl-click-row.bs.table', function ($element, row, field) {
            
            pendingData.PFMSID = row.FMSID;
            pendingData.PMachineNo = row.MachineNo;
            //pendingData.BrandName = row.BrandName;
            //pendingData.UnitName = row.UnitName;

            getFinishingMCSetupInfo(pendingData.PFMSID, pendingData.DBCFPID);
            $("#modal-machine-finishing-production").modal('hide');

            $formEl.find("#ProcessName").val(pendingData.ProcessName);
            $formEl.find("#MachineNo").val(pendingData.MachineNo);
            $formEl.find("#PMachineNo").val(pendingData.PMachineNo);
            //$formEl.find("#UnitName").val(pendingData.UnitName);
            //$formEl.find("#BrandName").val(pendingData.BrandName);
        });
    }

    function ProductionComplete() {
        var index = $tblChildEl.getRowIndexByPrimaryKey(pendingData.DBCFPID);
        var list = $tblChildEl.getCurrentViewRecords();
        if (index > 0 && list[index - 1].ProductionDate == null) {
            toastr.error("Previous process not completed yet!");
            return;
        }
        if ($formEl.find("#ProdDate").val() == '') {
            toastr.error("Please select Production Date!");
            return;
        }
        if ($formEl.find("#ShiftID").val() == null) {
            toastr.error("Please select Shift!");
            return;
        }
        if ($formEl.find("#OperatorID").val() == null) {
            toastr.error("Please select Operator!");
            return;
        }

        var machinePros = $tblMachineInfoEl.getCurrentViewRecords();
        var fmID = pendingData.PFMSID > 0 ? pendingData.PFMSID : pendingData.FMSID;
        if (fmID == 0 || machinePros.length == 0) {
            toastr.error("Select machine");
            pendingData.PFMSID = 0;
            pendingData.FMSID = 0;
            return;
        }

        pendingData.PFMSID = fmID;
        pendingData.FMSID = fmID;


        pendingData.ProductionDate = $formEl.find("#ProdDate").val();
        pendingData.ShiftID = $formEl.find("#ShiftID").val();
        pendingData.OperatorID = $formEl.find("#OperatorID").val();
        var machinePros = $tblMachineInfoEl.getCurrentViewRecords();
        //pendingData
        machinePros.forEach(function (mc) {
            pendingData[mc.ParamName] = mc.PlanParamValue;
            pendingData['P' + mc.ParamName] = mc.ActulaPlanParamValue;
        });
        axios.post("/api/roll-finishing/save", pendingData)
            .then(function (response) {
                toastr.success("Saved successfully!");
                $tblChildEl.updateRow(index, pendingData);
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function UpdateFinishingProcess() {
        //axios.get(`/api/roll-finishing/update-finishing-process?dbId=${masterData.DBatchID}&conceptId=${masterData.ConceptID}&colorId=${masterData.ColorID}`)
        axios.post("/api/roll-finishing/update-finishing-process", masterData)
            .then(function (response) {
                initChildTable(response.data);
                toastr.success("Update successfully!");
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();
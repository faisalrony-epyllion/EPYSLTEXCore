(function () {
    var menuId, pageName, pageId;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, tblMasterId, tblChildItemsId, $tblMasterEl, $tblChildEl, tblChildId, $tblMachineInfoEl, tblMachineInfoId, $formEl, tblMachineId, $tblChildItemsFPP;
    var tableParams = {
        offset: 0,
        limit: 10,
        filter: '',
        sort: '',
        order: ''
    }
    var selectedChildItemsFPP = [];
    var fpChildID = 0, segmentNo = 0;
    var status, pendingData, childGridcolumnList = [];
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
        tblMachineInfoId = "#tblMachineInfo" + pageId;
        tblMachineId = $("#tblMachine" + pageId);
        tblChildItemsId = "#tblChildItems" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $formEl.find("#divProcessProduction").fadeOut();
        pageIdWithHash = "#" + pageId;
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

        $formEl.find("#btnCancel").on("click", backToListWithoutFilter);

        $formEl.find("#btnSelectMachine").on("click", function (e) {
            e.preventDefault();
            ShowMachine();
        });

        $formEl.find("#btnProductionComplete").on("click", function (e) {
            e.preventDefault();
            ProductionComplete();
        });
        $formEl.find("#btnLoadChildItemsFPP").on("click", function (e) {
            getChemicalItemFPP();
        });
    });

    function initMasterTable() {
        var commands = status == statusConstants.PENDING
            ? [{ type: 'Add', title: 'Create', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }]
            : [{ type: 'View', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }];

        var columns = [

            {
                headerText: 'Actions', commands: commands, width: 100
            },
            {
                field: 'ConceptNo', headerText: 'Concept No'
            },
            {
                field: 'ConceptDate', headerText: 'Concept Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'PFBatchNo', headerText: 'Batch No'
            },
            {
                field: 'PFBatchDate', headerText: 'Batch Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'BatchQty', headerText: 'Batch Qty'
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/finishing-process-production/list?status=${status}`,
            autofitColumns: false,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        getDetails(args.rowData.FPMasterID);
    }

    function initChildTable(data) {
        if ($tblChildEl) {
            $tblChildEl.destroy();
            $(tblChildId).html("");
        }
        ej.base.enableRipple(true);
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            editSettings: { allowAdding: true, allowDeleting: true, allowEditing: true },  //allowAdding: true, allowEditing: true,
            recordClick: function (args) {
                if (args.column.field == "Status") { //args.column.field == "ProductionDate"  //ProductionDate = null
                    pendingData = args.rowData;
                    $formEl.find("#divProcessProduction").fadeIn();

                    productionCompleteBtnHideShow();

                    $formEl.find("#ProcessName").val(pendingData.ProcessName);
                    $formEl.find("#ProductionDate").val(formatDateToDefault(pendingData.ProductionDate == null ? new Date() : pendingData.ProductionDate));
                    $formEl.find("#ShiftID").val(pendingData.ShiftID).trigger("change");
                    $formEl.find("#OperatorID").val(pendingData.OperatorID).trigger("change");
                    $formEl.find("#MachineNo").val(pendingData.MachineNo);
                    $formEl.find("#PMachineNo").val(pendingData.PMachineNo == null ? pendingData.MachineNo : pendingData.PMachineNo);
                    $formEl.find("#PBrandName").val(typeof pendingData.PBrandName === "undefined" || pendingData.PBrandName == null ? pendingData.BrandName : pendingData.PBrandName);
                    $formEl.find("#PUnitName").val(typeof pendingData.PUnitName === "undefined" || pendingData.PUnitName == null ? pendingData.UnitName : pendingData.PUnitName);
                    $formEl.find("#BrandName").val(pendingData.BrandName);
                    $formEl.find("#UnitName").val(pendingData.UnitName);

                    fpChildID = pendingData.FPChildID;

                    var nFMSID = pendingData.PFMSID == 0 ? pendingData.FMSID : pendingData.PFMSID;
                    getFinishingMCSetupInfo(nFMSID, pendingData.FPChildID);
                } else {
                    $formEl.find("#divProcessProduction").fadeOut();
                }
            },
            columns: [
                { field: 'FPChildID', isPrimaryKey: true, visible: false },
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
            return `<a class="btn btn-xs btn-default" href="javascript:void(0)" title="Status">
                                    <i class="fa fa-tasks" aria-hidden="true"></i> Pending
                                </a>`;
        } else {
            return `<a class="btn btn-xs btn-default" href="javascript:void(0)" title="Status">
                                    <i class="fa fa-tasks" aria-hidden="true"></i> Complete
                                </a>`;
        }
    }


    function getFinishingMCSetupInfo(fmsId, fpChildId) {
        axios.get(`/api/finishing-process-production/machine/${fmsId}/${fpChildId}`)
            .then(function (response) {
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
                headerText: "Actual Value",
                allowEditing: true
            },
            {
                field: 'AddRecipe',
                headerText: 'Add Recipe',
                //width: 20,
                allowEditing: false,
                valueAccessor: AddRecipe

            },
        ]

        if ($tblMachineInfoEl) $tblMachineInfoEl.destroy();
        $tblMachineInfoEl = new ej.grids.Grid({
            dataSource: data,
            editSettings: { allowAdding: true, allowDeleting: true, allowEditing: true },  //allowAdding: true, allowEditing: true,
            recordClick: function (args) {
                if (args.column.field == "AddRecipe") {
                    getSavedChemicalItem(args.rowData, args.rowIndex);
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
            return '<a class="btn btn-xs btn-default" data-toggle="modal" data-target="#finishing-process-child-items" href="javascript:void(0)" title="Add Recipe"><i class="fa fa-tasks" aria-hidden="true"></i> Add Recipe</a>';
        }
    }

    function getSavedChemicalItem(rowData, index) {
        segmentNo = rowData.SerialNo;
        var url = `/api/finishing-process-child-item/get-finishing-process-child-items/${fpChildID}/${segmentNo}`;
        axios.get(url)
            .then(function (response) {
                var objList = response.data.Items;
                selectedChildItemsFPP = [];
                loadChildItems(objList);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getSelectedChildItemsFPP(objList) {
        var finalList = [];
        var childItems = [...selectedChildItemsFPP, ...objList];
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
        selectedChildItemsFPP = getSelectedChildItemsFPP(objList);
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

        if ($tblChildItemsFPP) $tblChildItemsFPP.destroy();
        $tblChildItemsFPP = new initEJ2Grid({
            tableId: tblChildItemsId,
            autofitColumns: false,
            columns: columns,
            data: selectedChildItemsFPP,
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            commandClick: handleCommands1,
            actionComplete: function (e) {
                if (e.requestType == "save") {
                    var obj = e.data;
                    if (typeof obj.FPChildID === "undefined") obj.FPChildID = fpChildID;
                    if (typeof obj.FPMasterID === "undefined") obj.FPMasterID = parseInt($formEl.find("#FPMasterID").val());
                    if (typeof obj.SegmentNo === "undefined") obj.SegmentNo = segmentNo;
                    if (typeof obj.IsPreProcess === "undefined") obj.IsPreProcess = 0;
                    axios.post("/api/finishing-process-child-item/save", obj)
                        .then(function (response) {
                            var index = selectedChildItemsFPP.findIndex(x => x.ItemMasterID === response.data.ItemMasterID);
                            if (index > -1) {
                                selectedChildItemsFPP[index] = response.data;
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
            if (typeof childItem.IsPreProcess === "undefined" || childItem.IsPreProcess == 0) {
                var fpChildItemID = 0;
                var selectedChildItem = selectedChildItemsFPP.find(x => x.ItemMasterID === childItem.ItemMasterID);
                if (typeof selectedChildItem !== "undefined" && selectedChildItem != null) {
                    fpChildItemID = selectedChildItem.FPChildItemID;
                }
                if (typeof fpChildItemID !== "undefined" && fpChildItemID > 0) {
                    axios.delete(`/api/finishing-process-child-item/delete-child/${fpChildItemID}`)
                        .then(function (response) {
                            if (response.data == "Deleted") {
                                var index = selectedChildItemsFPP.findIndex(x => x.FPChildItemID === fpChildItemID);
                                if (index > -1) {
                                    selectedChildItemsFPP.splice(index, 1);
                                    loadChildItems([]);
                                }
                            } else {
                                toastr.error(response.data);
                            }
                        })
                        .catch(showResponseError)
                } else {
                    var index = selectedChildItemsFPP.findIndex(x => x.ItemMasterID === childItem.ItemMasterID);
                    if (index > -1) {
                        selectedChildItemsFPP.splice(index, 1);
                        loadChildItems([]);
                    }
                }
            }
        }
    }
    function getChemicalItemFPP() {
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
            debugger;
            pendingData.PFMSID = row.FMSID;
            //pendingData.PMachineNo = row.MachineNo;
            //pendingData.BrandName = row.BrandName;
            //pendingData.UnitName = row.UnitName;
            getFinishingMCSetupInfo(pendingData.PFMSID, pendingData.FPChildID);
            $("#modal-machine-production").modal('hide');

            $formEl.find("#ProcessName").val(pendingData.ProcessName);
            $formEl.find("#MachineNo").val(pendingData.MachineNo);
            $formEl.find("#PMachineNo").val(row.MachineNo);

            $formEl.find("#PBrandName").val(row.BrandName);
            $formEl.find("#PUnitName").val(row.UnitName);
        });
    }
    
    function backToListWithoutFilter() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
    }
    function backToList() {
        backToListWithoutFilter();
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

    function getDetails(id) {
        axios.get(`/api/finishing-process-production/${id}/${status}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;

                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
                masterData.PFBatchDate = formatDateToDefault(masterData.PFBatchDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.PreFinishingProcessChilds);
                $formEl.find("#divProcessProduction").fadeOut();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function productionCompleteBtnHideShow() {
        if (pendingData.ProductionDate == null)
            $formEl.find("#btnProductionComplete").fadeIn();
        else
            $formEl.find("#btnProductionComplete").fadeOut();
    }
    function ProductionComplete() {
        var index = $tblChildEl.getRowIndexByPrimaryKey(pendingData.FPChildID);
        var list = $tblChildEl.getCurrentViewRecords();
        if (index > 0 && list[index - 1].ProductionDate == null) {
            toastr.error("Previous process not completed yet!");
            return;
        }

        if ($formEl.find("#ProductionDate").val() == '') {
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
        if (pendingData.PFMSID == 0)
            pendingData.PFMSID = pendingData.FMSID;
        pendingData.ProductionDate = $formEl.find("#ProductionDate").val();
        pendingData.ShiftID = $formEl.find("#ShiftID").val();
        pendingData.OperatorID = $formEl.find("#OperatorID").val();
        var machinePros = $tblMachineInfoEl.getCurrentViewRecords();
        //pendingData
        machinePros.forEach(function (mc) {
            pendingData[mc.ParamName] = mc.PlanParamValue;
            pendingData['P' + mc.ParamName] = mc.ActulaPlanParamValue;
        });
   
        pendingData.BatchQty = $formEl.find("#BatchQty").val();
        pendingData.PMachineNo = $formEl.find("#PMachineNo").val();
        pendingData.PBrandName = $formEl.find("#PBrandName").val();
        pendingData.PUnitName = $formEl.find("#PUnitName").val();

        axios.post("/api/finishing-process-production/save", pendingData)
            .then(function (response) {
                toastr.success("Saved successfully!");
                var index = $tblChildEl.getRowIndexByPrimaryKey(pendingData.FPChildID);
                $tblChildEl.updateRow(index, pendingData);

                productionCompleteBtnHideShow();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();
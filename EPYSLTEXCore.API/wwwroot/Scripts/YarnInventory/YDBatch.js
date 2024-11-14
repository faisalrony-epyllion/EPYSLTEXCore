(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $tblRequirementEl, $tblOthersRequirementEl, $tblRecipeE1, $tblKProductionE1, $tblRequirementsChildE1, $formEl, $tblRollEl, $divRollEl;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        filter: '',
        sort: '',
        order: ''
    }
    var status, childIndex = 0, _subGroup;
    var masterData;
    var removingRollNo;
    var totalQty;
    var _rowIndex;

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
        $tblRequirementEl = $("#tblRequirement" + pageId);
        $tblOthersRequirementEl = $("#tblOthersRequirement" + pageId);
        $tblRecipeE1 = $("#tblRecipe" + pageId);
        $tblKProductionE1 = $("#tblKProduction" + pageId);
        $tblRequirementsChildE1 = $("#tblRequirementsChild" + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $tblRollEl = $("#tblRoll" + pageId);
        $divRollEl = $("#divRoll" + pageId);

        status = statusConstants.PENDING;
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

        //$toolbarEl.find("#btnApproveList").on("click", function (e) {
        //    e.preventDefault();
        //    toggleActiveToolbarBtn(this, $toolbarEl);
        //    resetTableParams();
        //    status = statusConstants.APPROVED;

        //    initMasterTable();
        //    getMasterTableData();
        //});

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            //save();
            save(true);
        });

        //$formEl.find("#btnApprove").click(function (e) {
        //    e.preventDefault();
        //    save(true);
        //});

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#btnSplit").click(function (e) {
            e.preventDefault();
            split();
        });

        $formEl.find("#btnSaveKProd").click(function (e) {
            e.preventDefault();
            saveKProd();
        });

        $formEl.find("#btnSelectMachine").on("click", function (e) {
            e.preventDefault();
            var finder = new commonFinder({
                title: "Select Machine",
                pageId: pageId,
                isMultiselect: false,
                modalSize: "modal-lg",
                primaryKeyColumn: "DMID",
                fields: "DyeingMcslNo,DyeingMcStatus,DyeingNozzleQty,DyeingMcCapacity,DyeingMcBrand,Company",
                headerTexts: "M/C SL No,Status,Nozzle,Capacity,Brand,Unit",
                apiEndPoint: `/api/dyeing-machine/nozzle-list`,
                autofitColumns: false,
                onSelect: function (res) {
                    finder.hideModal();
                    $formEl.find("#DMID").val(res.rowData.DMID);
                    $formEl.find("#DMNo").val(res.rowData.DyeingMcslNo);
                    $formEl.find("#DyeingNozzleQty").val(res.rowData.DyeingNozzleQty);
                    $formEl.find("#DyeingMcCapacity").val(res.rowData.DyeingMcCapacity);
                    calculateMachineLoading();
                },
            });
            finder.showModal();
        });

        $formEl.find("#btnAddItem").on("click", function (e) {
            
            var list = masterData.YDBatchItemRequirements;
            var colorId = masterData.ColorID;
            var yDBookingChildIds = "-";
            var yDBookingMasterID = masterData.YDBookingMasterID;
            if (list != null && list.length > 0) {
                yDBookingChildIds = masterData.YDBatchItemRequirements.map(x => x.YDBookingChildID);
                yDBookingChildIds = yDBookingChildIds.join(",");
            }
            if (colorId > 0 && yDBookingMasterID > 0) {
                e.preventDefault();
                var finder = new commonFinder({
                    title: "Select Item(s)",
                    pageId: pageId,
                    isMultiselect: true,
                    modalSize: "modal-lg",
                    primaryKeyColumn: "ConceptID",
                    fields: "SubGroup,TechnicalName,FabricComposition,FabricGsm,Length,Width,ConceptOrSampleQty,Qty,ProdQty,ProdQtyPcs",
                    headerTexts: "Item,Technical Name,Fabric Composition,Fabric Gsm,Length,Width,Concept Qty,Qty,Production Qty,Pcs",
                    widths: "100,80,150,80,80,80,80,80,80,80",
                    apiEndPoint: `/api/yd-batch/get-other-items/${yDBookingChildIds}/${colorId}/${yDBookingMasterID}`,
                    autofitColumns: false,
                    onMultiselect: function (selectedRecords) {
                        selectedRecords.forEach(function (obj) {
                            
                            if (obj.YDBookingChildID > 0) {
                                var cObj = masterData.YDBatchItemRequirements.find(x => x.YDBookingChildID == obj.YDBookingChildID);
                                if (cObj == null || typeof cObj === "undefined") {
                                    masterData.YDBatchItemRequirements.push(obj);
                                }
                            }
                        });
                        $tblRequirementEl.bootstrapTable("load", masterData.YDBatchItemRequirements);
                    }
                });
                finder.showModal();
            }
        });

        $formEl.find("#btnOk").click(function (e) {
            e.preventDefault();
            if (_subGroup == 1) {
                var sumRollQty = 0;
                if (masterData.YDBatchItemRequirements[childIndex].YDBatchChilds == null) {
                    masterData.YDBatchItemRequirements[childIndex].YDBatchChilds = [];
                }
                $tblRequirementsChildE1.bootstrapTable('getSelections').forEach(function (item) {
                    var isExist = false;
                    sumRollQty = sumRollQty + item.RollQty
                    masterData.YDBatchItemRequirements[childIndex].YDBatchChilds.forEach(function (child) {
                        if (child.RollNo == item.RollNo) {
                            isExist = true;
                        }
                    });
                    if (!isExist) {
                        var childObject = {
                            GRollID: item.GRollID,
                            RollNo: item.RollNo,
                            RollQty: item.RollQty,
                            RollQtyPcs: 0,
                            ItemMasterID: 0
                        }

                        //masterData.YDBatchItemRequirements[ind].YDBatchChilds.splice(dIndex, 1);
                        masterData.YDBatchItemRequirements[childIndex].YDBatchChilds.push(childObject);
                        var dIndex = masterData.YDBatchItemRequirements[childIndex].YDBatchChilds.findIndex(x => x.GRollID == item.ParentGRollID);
                        if (dIndex >= 0) {
                            masterData.YDBatchItemRequirements[childIndex].YDBatchChilds.splice(dIndex, 1);
                        }

                    }
                });
                masterData.YDBatchItemRequirements[childIndex].Qty = sumRollQty;
                $tblRequirementEl.bootstrapTable('updateCellById', { id: masterData.YDBatchItemRequirements[childIndex].RecipeItemInfoID, field: 'Qty', value: sumRollQty });
                calculateMachineLoading();
                $tblChildEl.bootstrapTable('load', masterData.YDBatchItemRequirements[childIndex].YDBatchChilds);
                $('#modal-item-req-child').modal('toggle');
                $tblRequirementEl.bootstrapTable('expandRow', childIndex);

                var totalQty = 0;
                for (var i = 0; i < $tblRequirementEl.bootstrapTable('getData').length; i++) {
                    totalQty += parseFloat($tblRequirementEl.bootstrapTable('getData')[i].Qty) || 0;

                }
                for (var i = 0; i < $tblOthersRequirementEl.bootstrapTable('getData').length; i++) {
                    totalQty += parseFloat($tblOthersRequirementEl.bootstrapTable('getData')[i].Qty) || 0;
                }
                $formEl.find("#BatchWeightKG").val(totalQty);

                var totalQtyFabricKG = 0;
                masterData.YDBatchItemRequirements[childIndex].YDBatchChilds.map(x => {
                    totalQtyFabricKG += parseFloat(x.RollQty.toFixed(2));
                });
                totalQtyFabricKG = parseFloat(totalQtyFabricKG.toFixed(2));

                masterData.YDBatchItemRequirements[childIndex].Qty = totalQtyFabricKG;
                $tblRequirementEl.bootstrapTable('updateCellById', { id: masterData.YDBatchItemRequirements[childIndex].RecipeItemInfoID, field: 'Qty', value: totalQtyFabricKG });

            }
            else {
                var sumRollQty = 0, sumRollQtyPcs = 0;
                if (masterData.YDBatchOtherItemRequirements[childIndex].YDBatchChilds == null) {
                    masterData.YDBatchOtherItemRequirements[childIndex].YDBatchChilds = [];
                }
                $tblRequirementsChildE1.bootstrapTable('getSelections').forEach(function (item) {
                    var isExist = false;
                    sumRollQty += item.RollQty;
                    sumRollQtyPcs += item.RollQtyPcs;
                    masterData.YDBatchOtherItemRequirements[childIndex].YDBatchChilds.forEach(function (child) {
                        if (child.RollNo == item.RollNo) isExist = true;
                    });
                    if (!isExist) {
                        var childObject = {
                            GRollID: item.GRollID,
                            RollNo: item.RollNo,
                            RollQty: item.RollQty,
                            RollQtyPcs: item.RollQtyPcs,
                            ItemMasterID: 0
                        }
                        masterData.YDBatchOtherItemRequirements[childIndex].YDBatchChilds.push(childObject);
                        var dIndex = masterData.YDBatchOtherItemRequirements[childIndex].YDBatchChilds.findIndex(x => x.GRollID == item.ParentGRollID);
                        if (dIndex >= 0) {
                            masterData.YDBatchOtherItemRequirements[childIndex].YDBatchChilds.splice(dIndex, 1);
                        }
                    }
                })
                masterData.YDBatchOtherItemRequirements[childIndex].Qty = sumRollQty;
                masterData.YDBatchOtherItemRequirements[childIndex].Pcs = sumRollQtyPcs;
                $tblOthersRequirementEl.bootstrapTable('updateCellById', { id: masterData.YDBatchOtherItemRequirements[childIndex].RecipeItemInfoID, field: 'Qty', value: sumRollQty });
                calculateMachineLoading();
                $tblChildEl.bootstrapTable('load', masterData.YDBatchOtherItemRequirements[childIndex].YDBatchChilds);
                $('#modal-item-req-child').modal('toggle');
                $tblOthersRequirementEl.bootstrapTable('expandRow', childIndex);

                var totalQty = 0, totalQtyPcs = 0;
                for (var i = 0; i < $tblRequirementEl.bootstrapTable('getData').length; i++) {
                    totalQty += parseFloat($tblRequirementEl.bootstrapTable('getData')[i].Qty) || 0;
                }

                for (var i = 0; i < $tblOthersRequirementEl.bootstrapTable('getData').length; i++) {
                    totalQty += parseFloat($tblOthersRequirementEl.bootstrapTable('getData')[i].Qty) || 0;
                    totalQtyPcs += parseFloat($tblOthersRequirementEl.bootstrapTable('getData')[i].Pcs) || 0;
                }

                $formEl.find("#BatchWeightKG").val(totalQty);
                $formEl.find("#BatchWeightPcs").val(totalQtyPcs);

                var totalQtyCollarCuffKG = 0;
                var totalQtyCollarCuffPcs = 0;

                masterData.YDBatchOtherItemRequirements[childIndex].YDBatchChilds.map(x => {
                    totalQtyCollarCuffKG += parseFloat(x.RollQty.toFixed(2));
                    totalQtyCollarCuffPcs += x.RollQtyPcs;
                });
                totalQtyCollarCuffKG = parseFloat(totalQtyCollarCuffKG.toFixed(2));

                masterData.YDBatchOtherItemRequirements[childIndex].Qty = totalQtyCollarCuffKG;
                masterData.YDBatchOtherItemRequirements[childIndex].Pcs = totalQtyCollarCuffPcs;

                $tblOthersRequirementEl.bootstrapTable('updateCellById', { id: masterData.YDBatchOtherItemRequirements[childIndex].RecipeItemInfoID, field: 'Qty', value: totalQtyCollarCuffKG });
                $tblOthersRequirementEl.bootstrapTable('updateCellById', { id: masterData.YDBatchOtherItemRequirements[childIndex].RecipeItemInfoID, field: 'Pcs', value: totalQtyCollarCuffPcs });
            }
            calculateBatchQty(masterData);
        });
    });

    function calculateMachineLoading() {
        var totalQty = 0;
        for (var i = 0; i < $tblRequirementEl.bootstrapTable('getData').length; i++) {
            totalQty += parseFloat($tblRequirementEl.bootstrapTable('getData')[i].Qty) || 0;
        }
        for (var i = 0; i < $tblOthersRequirementEl.bootstrapTable('getData').length; i++) {
            totalQty += parseFloat($tblOthersRequirementEl.bootstrapTable('getData')[i].Qty) || 0;
        }

        var machineLoading = ((totalQty * 100) / parseFloat($formEl.find("#DyeingMcCapacity").val())).toFixed(2);
        if (machineLoading == 'Infinity') {
            machineLoading = 0;
        }
        $formEl.find("#MachineLoading").val(machineLoading);
    }
    function calculateBatchWeight(field) {
        if (field == "Qty") {
            var totalQty = 0;

            for (var i = 0; i < $tblRequirementEl.bootstrapTable('getData').length; i++) {
                totalQty += parseFloat($tblRequirementEl.bootstrapTable('getData')[i].Qty) || 0;
            }

            for (var i = 0; i < $tblOthersRequirementEl.bootstrapTable('getData').length; i++) {
                totalQty += parseFloat($tblOthersRequirementEl.bootstrapTable('getData')[i].Qty) || 0;
            }
            $formEl.find("#BatchWeightKG").val(totalQty);
        }
        if (field == "Pcs") {
            var totalPcs = 0;

            for (var i = 0; i < $tblRequirementEl.bootstrapTable('getData').length; i++) {
                totalPcs += parseFloat($tblRequirementEl.bootstrapTable('getData')[i].Pcs) || 0;
            }

            for (var i = 0; i < $tblOthersRequirementEl.bootstrapTable('getData').length; i++) {
                totalPcs += parseFloat($tblOthersRequirementEl.bootstrapTable('getData')[i].Pcs) || 0;
            }

            $formEl.find("#BatchWeightPcs").val(totalPcs);
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
                    title: "Actions",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        if (status === statusConstants.PENDING) {
                            return `<a class="btn btn-xs btn-default add" href="javascript:void(0)" title="New masterData">
                                        <i class="fa fa-plus" aria-hidden="true"></i>
                                    </a>`;
                        } else if (status === statusConstants.COMPLETED) {
                            return `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit masterData">
                                        <i class="fa fa-edit" aria-hidden="true"></i>
                                    </a>`;
                        }
                        else {
                            return `<a class="btn btn-xs btn-default view" href="javascript:void(0)" title="View masterData">
                                        <i class="fa fa-eye" aria-hidden="true"></i>
                                    </a>`;
                        }
                    },
                    events: {
                        'click .add': function (e, value, row, index) {
                            e.preventDefault();
                            
                            getNew(row.YDBookingMasterID, row.YDRecipeID, row.ConceptNo, row.BookingID, row.IsBDS, row.ColorID);
                            $formEl.find("#btnSave").fadeIn();
                            $formEl.find("#btnApprove").fadeOut();
                        },
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            
                            getDetails(row.YDBatchID, row.ConceptNo);
                            $formEl.find("#btnSave").fadeIn();
                        },
                        'click .view': function (e, value, row, index) {
                            
                            e.preventDefault();
                            getDetails(row.YDBatchID, row.ConceptNo);
                            $formEl.find("#btnSave").fadeOut();
                        }
                    }
                },
                {
                    field: "YDBookingMasterID",
                    title: "YDBookingMasterID",
                    visible: false,
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "YDBatchNo",
                    title: "Batch No",
                    visible: status != statusConstants.PENDING,
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "YDBatchDate",
                    title: "Batch Date",
                    visible: status != statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "YDBookingNo",
                    title: "YD Booking No",
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
                    field: "YDRecipeNo",
                    title: "Recipe No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "RecipeDate",
                    title: "Recipe Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "ColorName",
                    title: "Color Name",
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
        var url = `/api/yd-batch/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function initRequirementTable() {
        $tblRequirementEl.bootstrapTable("destroy");
        $tblRequirementEl.bootstrapTable({
            showFooter: true,
            detailView: true,
            uniqueId: "RecipeItemInfoID",
            columns: [
                {
                    title: 'Actions',
                    align: 'center',
                    width: 100,
                    formatter: function () {
                        return [
                            '<span class="btn-group">',
                            '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete Item">',
                            '<i class="fa fa-remove"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            showBootboxConfirm("Delete Record.", "Are you sure want to delete this?", function (yes) {
                                if (yes) {
                                    masterData.YDBatchItemRequirements.splice(index, 1);
                                    $tblRequirementEl.bootstrapTable('load', masterData.YDBatchItemRequirements);
                                    calculateBatchQty(masterData);
                                }
                            });
                        }
                    }
                },
                {
                    field: "SubGroup",
                    title: "Item",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "TechnicalName",
                    title: "Technical Name"
                },
                {
                    field: "YarnCategory",
                    title: "Yarn Category",
                    align: 'left',
                },
                {
                    field: "FabricComposition",
                    title: "Fabric Composition",
                    align: 'left',
                },
                {
                    field: "FabricGsm",
                    title: "Fabric Gsm",
                    align: 'right',
                },
                {
                    field: "Length",
                    title: "Length",
                    align: 'right',
                },
                {
                    field: "Width",
                    title: "Width",
                    align: 'right',
                },
                {
                    field: "ConceptOrSampleQtyKg",
                    title: "Booking Qty",
                    align: 'right',
                },
                {
                    field: "Qty",
                    title: "Batch Qty (Kg)",
                    align: 'right',
                    cellStyle: function () { return { classes: 'm-w-80' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" step=".01" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseFloat(value)) || parseInt(value) == 0) {
                                return 'Must be a positive integer.';
                            }
                        }
                    }
                },
                {
                    field: "PlannedBatchQtyKg",
                    title: "Planned Batch Qty (Kg)",
                    align: 'right',
                }
            ],
            onExpandRow: function (index, row, $detail) {
                populateChild(row.RecipeItemInfoID, row.ConceptID, $detail, 1, index);
            },
            onAll: function (name, args) {
                if (name === "editable-save.bs.table") {
                    calculateMachineLoading();
                }
            },
            onEditableSave: function (field, row, oldValue, $el) {
                if (field == "Qty") {
                    calculateBatchWeight("Qty");
                }
            }
        });
    }

    function initOthersRequirementTable() {
        $tblOthersRequirementEl.bootstrapTable("destroy");
        $tblOthersRequirementEl.bootstrapTable({
            showFooter: true,
            detailView: true,
            uniqueId: "RecipeItemInfoID",
            columns: [
                {
                    title: 'Actions',
                    align: 'center',
                    width: 100,
                    formatter: function () {
                        return [
                            '<span class="btn-group">',
                            '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete Item">',
                            '<i class="fa fa-remove"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            showBootboxConfirm("Delete Record.", "Are you sure want to delete this?", function (yes) {
                                if (yes) {
                                    masterData.YDBatchOtherItemRequirements.splice(index, 1);
                                    $tblOthersRequirementEl.bootstrapTable('load', masterData.YDBatchOtherItemRequirements);
                                    calculateBatchQty(masterData);
                                }
                            });
                        }
                    }
                },
                {
                    field: "FUPartName",
                    title: "End Use",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "TechnicalName",
                    title: "Technical Name"
                },
                {
                    field: "ColorName",
                    title: "Color Name"
                },
                {
                    field: "Length",
                    title: "Length",
                    align: 'right',
                },
                {
                    field: "Width",
                    title: "Width",
                    align: 'right',
                },
                {
                    field: "ConceptOrSampleQtyKg",
                    title: "Concept Qty (Kg)",
                    align: 'right',
                },
                {
                    field: "ConceptOrSampleQtyPcs",
                    title: "Concept Qty (Pcs)",
                    align: 'right',
                },
                {
                    field: "Qty",
                    title: "Batch Qty (Kg)",
                    align: 'right',
                    cellStyle: function () { return { classes: 'm-w-80' } },
                    //editable: {
                    //    type: "text",
                    //    showbuttons: false,
                    //    tpl: '<input type="number" step=".01" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                    //    validate: function (value) {
                    //        if (!value || !value.trim() || isNaN(parseFloat(value)) || parseInt(value) == 0) {
                    //            return 'Must be a positive integer.';
                    //        }
                    //    }
                    //}
                },
                {
                    field: "Pcs",
                    title: "Batch Qty (Pcs)",
                    align: 'right',
                    cellStyle: function () { return { classes: 'm-w-80' } },
                    //editable: {
                    //    type: "text",
                    //    showbuttons: false,
                    //    tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                    //    validate: function (value) {
                    //        if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) == 0) {
                    //            return 'Must be a positive integer.';
                    //        }
                    //    }
                    //}
                },
                {
                    field: "PlannedBatchQtyKg",
                    title: "Planned Batch Qty (Kg)",
                    align: 'right',
                },
                {
                    field: "PlannedBatchQtyPcs",
                    title: "Planned Batch Qty (Pcs)",
                    align: 'right',
                }
            ],
            onExpandRow: function (index, row, $detail) {
                populateChild(row.RecipeItemInfoID, row.ConceptID, $detail, 11, index);
            },
            onAll: function (name, args) {
                if (name === "editable-save.bs.table") {
                    calculateMachineLoading();
                }
            },
            onEditableSave: function (field, row, oldValue, $el) {
                if (field == "Qty") {
                    calculateBatchWeight("Qty");
                    //var totalQty = 0;

                    //for (var i = 0; i < $tblRequirementEl.bootstrapTable('getData').length; i++) {
                    //    totalQty += parseFloat($tblRequirementEl.bootstrapTable('getData')[i].Qty) || 0;
                    //}

                    //for (var i = 0; i < $tblOthersRequirementEl.bootstrapTable('getData').length; i++) {
                    //    totalQty += parseFloat($tblOthersRequirementEl.bootstrapTable('getData')[i].Qty) || 0;
                    //}

                    //$formEl.find("#BatchWeightKG").val(totalQty);
                }
                else if (field == "Pcs") {
                    calculateBatchWeight("Pcs");
                    //var totalPcs = 0;

                    //for (var i = 0; i < $tblRequirementEl.bootstrapTable('getData').length; i++) {
                    //    totalPcs += parseFloat($tblRequirementEl.bootstrapTable('getData')[i].Pcs) || 0;
                    //}

                    //for (var i = 0; i < $tblOthersRequirementEl.bootstrapTable('getData').length; i++) {
                    //    totalPcs += parseFloat($tblOthersRequirementEl.bootstrapTable('getData')[i].Pcs) || 0;
                    //}

                    //$formEl.find("#BatchWeightPcs").val(totalPcs);
                }
            }
        });
    }

    function populateChild(childId, conceptID, $detail, subGroup, ind) {
        $tblChildEl = $detail.html('<table id="TblBatchChild-' + childId + '"></table>').find('table');
        initChildTable(conceptID, subGroup, ind);

        var cnt = 0;

        if (subGroup == 1) {
            //ind = getIndexFromArray(masterData.YDBatchItemRequirements, "RecipeItemInfoID", childId);
            cnt = (masterData.YDBatchItemRequirements[ind].YDBatchChilds == null) ? 0 : masterData.YDBatchItemRequirements[ind].YDBatchChilds.length;
            if (cnt > 0)
                $tblChildEl.bootstrapTable('load', masterData.YDBatchItemRequirements[ind].YDBatchChilds.filter(function (item) {
                    return item.EntityState != 8
                }));
        } else {
            //ind = getIndexFromArray(masterData.YDBatchOtherItemRequirements, "RecipeItemInfoID", childId);
            cnt = (masterData.YDBatchOtherItemRequirements[ind].YDBatchChilds == null) ? 0 : masterData.YDBatchOtherItemRequirements[ind].YDBatchChilds.length;
            if (cnt > 0)
                $tblChildEl.bootstrapTable('load', masterData.YDBatchOtherItemRequirements[ind].YDBatchChilds.filter(function (item) {
                    return item.EntityState != 8
                }));
        }
        _subGroup = subGroup;
        childIndex = ind;
    }

    function initChildTable(conceptID, subGroup, ind) {
        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            showFooter: true,
            columns: [
                {
                    title: 'Actions',
                    align: 'center',
                    width: 100,
                    formatter: function () {
                        return [
                            '<span class="btn-group">',
                            '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete Item">',
                            '<i class="fa fa-remove"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    footerFormatter: function () {
                        return [
                            '<span class="btn-group">',
                            '<button class="btn btn-success" onclick="showTblRequirementChild(' + conceptID + ',' + ind + ',' + subGroup + ')" title="Add Roll">',
                            '<i class="fa fa-plus"></i>',
                            '</button>',
                            '<button class="btn btn-success" onclick="showProductionRoll(' + conceptID + ',' + ind + ',' + subGroup + ')" title="Split Roll">',
                            '<i class="fa fa-edit"></i>',
                            '</button>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            var sumRollQty = 0;
                            if (subGroup == 1) {
                                masterData.YDBatchItemRequirements[ind].YDBatchChilds.splice(index, 1);
                                $tblChildEl.bootstrapTable('load', masterData.YDBatchItemRequirements[ind].YDBatchChilds);

                                var totalQtyFabricKG = 0;
                                masterData.YDBatchItemRequirements[ind].YDBatchChilds.map(x => {
                                    totalQtyFabricKG += parseFloat(x.RollQty.toFixed(2));
                                });
                                totalQtyFabricKG = parseFloat(totalQtyFabricKG.toFixed(2));

                                masterData.YDBatchItemRequirements[ind].Qty = totalQtyFabricKG;
                                $tblRequirementEl.bootstrapTable('updateCellById', { id: masterData.YDBatchItemRequirements[ind].RecipeItemInfoID, field: 'Qty', value: totalQtyFabricKG });
                            }
                            else {
                                masterData.YDBatchOtherItemRequirements[ind].YDBatchChilds.splice(index, 1);
                                $tblChildEl.bootstrapTable('load', masterData.YDBatchOtherItemRequirements[ind].YDBatchChilds);

                                var totalQtyCollarCuffKG = 0, totalQtyCollarCuffPcs = 0;
                                masterData.YDBatchOtherItemRequirements[ind].YDBatchChilds.map(x => {
                                    totalQtyCollarCuffKG += parseFloat(x.RollQty.toFixed(2));
                                    totalQtyCollarCuffPcs += x.RollQtyPcs;
                                });
                                totalQtyCollarCuffKG = parseFloat(totalQtyCollarCuffKG.toFixed(2));

                                masterData.YDBatchOtherItemRequirements[ind].Qty = totalQtyCollarCuffKG;
                                masterData.YDBatchOtherItemRequirements[ind].Pcs = totalQtyCollarCuffPcs;

                                $tblOthersRequirementEl.bootstrapTable('updateCellById', { id: masterData.YDBatchOtherItemRequirements[ind].RecipeItemInfoID, field: 'Qty', value: totalQtyCollarCuffKG });
                                $tblOthersRequirementEl.bootstrapTable('updateCellById', { id: masterData.YDBatchOtherItemRequirements[ind].RecipeItemInfoID, field: 'Pcs', value: totalQtyCollarCuffPcs });
                            }
                            calculateBatchQty(masterData);
                        }
                    }
                },
                {
                    field: "RollNo",
                    title: "Roll No",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "RollQty",
                    title: "Roll Qty (Kg)",
                    cellStyle: function () { return { classes: 'm-w-80' } }
                },
                {
                    field: "RollQtyPcs",
                    title: "Roll Qty (Pcs)",
                    visible: subGroup != 1,
                    cellStyle: function () { return { classes: 'm-w-80' } }
                }
            ]
        });
    }

    function initRequirementsChildTable(subGroup) {
        $tblRequirementsChildE1.bootstrapTable("destroy");
        $tblRequirementsChildE1.bootstrapTable({
            showFooter: true,
            columns: [
                {
                    field: "",
                    title: "",
                    checkbox: true
                },
                {
                    field: "RollNo",
                    title: "Roll No",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "RollQty",
                    title: "Roll Qty (Kg)",
                    cellStyle: function () { return { classes: 'm-w-80' } }
                },
                {
                    field: "RollQtyPcs",
                    title: "Roll Qty (Pcs)",
                    visible: subGroup != 1,
                    cellStyle: function () { return { classes: 'm-w-80' } }
                }
            ]
        });
    }

    function initRecipeTable() {
        $tblRecipeE1.bootstrapTable("destroy");
        $tblRecipeE1.bootstrapTable({
            uniqueId: 'RecipeChildID',
            //editable: isEditable,
            detailView: true,
            checkboxHeader: false,
            showFooter: true,
            columns: [
                {
                    field: "FiberPart",
                    title: "Fiber Part",
                    align: 'center'
                },
                {
                    field: "ColorName",
                    title: "Color Name",
                    align: 'center'
                },
                {
                    field: "TempIn",
                    title: "Temperature",
                    align: 'center'
                },
                {
                    field: "ProcessTime",
                    title: "Process Time",
                    align: 'center',
                }
            ],
            onExpandRow: function (index, row, $detail) {
                populateChildex(row.RecipeDInfoID, $detail);
            }
        });
    }

    function populateChildex(childId, $detail) {
        //console.log($detail);
        $tblDefChildEl = $detail.html('<table id="TblChildDef-' + childId + '"></table>').find('table');
        var childIndex = getIndexFromArray(masterData.YDBatchWiseRecipeChilds, "RecipeDInfoID", childId)
        initDefChildTable(childId, $detail);
        var cnt = (masterData.YDBatchWiseRecipeChilds[childIndex].DefChilds == null) ? 0 : masterData.YDBatchWiseRecipeChilds[childIndex].DefChilds.length;
        if (cnt > 0)
            $tblDefChildEl.bootstrapTable('load', masterData.YDBatchWiseRecipeChilds[childIndex].DefChilds.filter(function (item) {
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
        getMasterTableData();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#YDBatchID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
        filterBy = {};
    }

    function getNew(yDBookingMasterID, recipeID, conceptNo, bookingID, isBDS, colorID) {
        if (typeof bookingID == "undefined") {
            bookingID = 0;
        }
        axios.get(`/api/yd-batch/new/${yDBookingMasterID}/${recipeID}/${conceptNo}/${bookingID}/${isBDS}/${colorID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                
                var qtyInKg = 0, qtyInPcs = 0;
                //if (masterData.YDBatchItemRequirements.length > 0) {
                masterData.YDBatchItemRequirements.forEach(function (el) {
                    qtyInKg += parseFloat(el.Qty);
                });
                //}
                //if (masterData.YDBatchOtherItemRequirements.length > 0) {
                masterData.YDBatchOtherItemRequirements.forEach(function (el) {
                    qtyInKg += parseFloat(el.Qty);
                    qtyInPcs += parseInt(el.Pcs);
                });
                //}

                masterData.YDBatchDate = formatDateToDefault(masterData.YDBatchDate);
                masterData.RecipeDate = formatDateToDefault(masterData.RecipeDate);
                setFormData($formEl, masterData);

                $formEl.find("#FabricQty").val(qtyInKg);
                $formEl.find("#BatchWeightKG").val(qtyInKg);
                $formEl.find("#BatchWeightPcs").val(qtyInPcs);

                $formEl.find("#divRequirement").fadeIn();
                initRequirementTable();
                $tblRequirementEl.bootstrapTable("load", masterData.YDBatchItemRequirements);
                $tblRequirementEl.bootstrapTable('hideLoading');

                if (masterData.YDBatchOtherItemRequirements.length > 0) {
                    initOthersRequirementTable();
                    $tblOthersRequirementEl.bootstrapTable("load", masterData.YDBatchOtherItemRequirements);
                    $tblOthersRequirementEl.bootstrapTable('hideLoading');
                    $formEl.find("#divOtherRequirement").fadeIn();
                } else {
                    $formEl.find("#divOtherRequirement").fadeOut();
                }

                initRecipeTable();
                $tblRecipeE1.bootstrapTable("load", masterData.YDBatchWiseRecipeChilds);
                $tblRecipeE1.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id, conceptNo) {
        
        axios.get(`/api/yd-batch/${id}/${conceptNo}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                
                masterData.YDBatchDate = formatDateToDefault(masterData.YDBatchDate);
                masterData.RecipeDate = formatDateToDefault(masterData.RecipeDate);
                setFormData($formEl, masterData);
                if (masterData.IsApproved) $formEl.find("#btnApprove").fadeOut();
                else $formEl.find("#btnApprove").fadeIn();

                $formEl.find("#divRequirement").fadeIn();
                initRequirementTable();
                $tblRequirementEl.bootstrapTable("load", masterData.YDBatchItemRequirements);
                $tblRequirementEl.bootstrapTable('hideLoading');

                if (masterData.YDBatchOtherItemRequirements.length > 0) {
                    initOthersRequirementTable();
                    $tblOthersRequirementEl.bootstrapTable("load", masterData.YDBatchOtherItemRequirements);
                    $tblOthersRequirementEl.bootstrapTable('hideLoading');
                    $formEl.find("#divOtherRequirement").fadeIn();
                } else {
                    $formEl.find("#divOtherRequirement").fadeOut();
                }

                initRecipeTable();
                $tblRecipeE1.bootstrapTable("load", masterData.YDBatchWiseRecipeChilds);
                $tblRecipeE1.bootstrapTable('hideLoading');

                var fabricQty = 0, flatKnitQty = 0, qtyInPcs = 0;

                //if (masterData.YDBatchItemRequirements.length > 0) {
                masterData.YDBatchItemRequirements.forEach(function (el) {
                    fabricQty += parseFloat(el.Qty);
                });
                //}
                //if (masterData.YDBatchOtherItemRequirements.length > 0) {
                masterData.YDBatchOtherItemRequirements.forEach(function (el) {
                    //flatKnitQty += parseFloat(el.Qty);
                    fabricQty += parseFloat(el.Qty);
                    qtyInPcs += parseInt(el.Pcs);
                });
                //}

                $formEl.find("#FabricQty").val(fabricQty);
                $formEl.find("#BatchWeightKG").val(fabricQty);
                $formEl.find("#BatchWeightPcs").val(qtyInPcs);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    var validationConstraints = {
        YDBatchDate: {
            presence: true
        }
    }
    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
    function getValidList(listItems) {
        var resultList = listItems;
        if (typeof resultList === "undefined") resultList = [];
        return DeepClone(resultList);
    }
    function calculateBatchQty(obj) {
        var batchItems = getValidList(obj.YDBatchItemRequirements);
        var collarCuffs = getValidList(obj.YDBatchOtherItemRequirements);
        batchItems.push(...collarCuffs);

        var batchWeightKG = 0,
            batchWeightPcs = 0;

        batchItems.map(x => {
            batchWeightKG += x.Qty;
            batchWeightPcs += x.Pcs;
        });
        batchWeightKG = isNaN(batchWeightKG) ? 0 : batchWeightKG;
        batchWeightKG = parseFloat(batchWeightKG.toFixed(2));

        batchWeightPcs = isNaN(batchWeightPcs) ? 0 : batchWeightPcs;

        $formEl.find("#BatchWeightKG").val(batchWeightKG);
        $formEl.find("#BatchWeightPcs").val(batchWeightPcs);
    }

    function save(isApprove = false) {
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);
        var data = formDataToJson($formEl.serializeArray());
        data.YDBatchItemRequirements = masterData.YDBatchItemRequirements;
        data.YDBatchOtherItemRequirements = masterData.YDBatchOtherItemRequirements;
        data.YDBatchWiseRecipeChilds = masterData.YDBatchWiseRecipeChilds;
        data.SLNo = masterData.SLNo;
        data.IsApproved = isApprove;
        debugger;
        //Roll Validation
        var hasError = false;

        var batchWeightKG = 0,
            batchWeightPcs = 0;

        data.YDBatchItemRequirements = getValidList(data.YDBatchItemRequirements);
        if (data.YDBatchItemRequirements.length > 0) {
            for (var i = 0; i < data.YDBatchItemRequirements.length; i++) {
                var batchItemRequirement = data.YDBatchItemRequirements[i];

                //if (batchItemRequirement.YDBatchChilds == null || batchItemRequirement.YDBatchChilds.length == 0) {
                //    toastr.error("Select roll for every item");
                //    hasError = true;
                //    break;
                //}

                var rollQtyKg = 0;
                batchItemRequirement.YDBatchChilds.map(b => {
                    rollQtyKg += b.RollQty;
                });
                rollQtyKg = parseFloat(rollQtyKg.toFixed(2));
                //data.YDBatchItemRequirements[i].Qty = rollQtyKg;

                batchWeightKG += data.YDBatchItemRequirements[i].Qty;
            }
        }

        data.YDBatchOtherItemRequirements = getValidList(data.YDBatchOtherItemRequirements);
        if (!hasError && data.YDBatchOtherItemRequirements.length > 0) {
            for (var i = 0; i < data.YDBatchOtherItemRequirements.length; i++) {
                var batchOtherItemRequirement = data.YDBatchOtherItemRequirements[i];

                if (batchOtherItemRequirement.YDBatchChilds == null || batchOtherItemRequirement.YDBatchChilds.length == 0) {
                    toastr.error("Select roll for every item");
                    hasError = true;
                    break;
                }

                var rollQtyKg = 0,
                    rollQtyPcs = 0;
                batchOtherItemRequirement.YDBatchChilds.map(b => {
                    rollQtyKg += b.RollQty;
                    rollQtyPcs += b.RollQtyPcs;
                });
                debugger;
                rollQtyKg = parseFloat(rollQtyKg.toFixed(2));
                //data.YDBatchOtherItemRequirements[i].Qty = rollQtyKg;
                //data.YDBatchOtherItemRequirements[i].Pcs = rollQtyPcs;

                batchWeightKG += data.YDBatchOtherItemRequirements[i].Qty;
                batchWeightPcs += data.YDBatchOtherItemRequirements[i].Pcs;
            }
        }

        if (hasError) return false;
        //Roll Validation

        data.YDBatchWeightKG = batchWeightKG;
        data.YDBatchWeightPcs = batchWeightPcs;

        var childs = [];
        masterData.YDBatchWiseRecipeChilds.forEach(function (child) {
            child.DefChilds.forEach(function (defChild) {
                defChild.RecipeDInfoID = child.RecipeDInfoID;
                defChild.Temperature = child.Temperature;
                defChild.TempIn = child.TempIn;
                defChild.ProcessTime = child.ProcessTime;
                childs.push(defChild);
            });
        });

        data.YDBatchWiseRecipeChilds = childs;
        data.GroupConceptNo = data.ConceptNo;

        data.ExportOrderID = masterData.ExportOrderID;
        data.BuyerID = masterData.BuyerID;
        data.BuyerTeamID = masterData.BuyerTeamID;

        axios.post("/api/yd-batch/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function initRollTable() {
        //
        $tblRollEl.bootstrapTable("destroy");
        $tblRollEl.bootstrapTable({
            showFooter: true,
            columns: [
                {
                    field: "RollNo",
                    title: "Roll No",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "RollQty",
                    title: "Roll Qty",
                    visible: _subGroup == 1,
                    cellStyle: function () { return { classes: 'm-w-80' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseFloat(value)) || parseInt(value) == 0) {
                                return 'Must be a positive integer.';
                            }
                        }
                    }
                },
                {
                    field: "RollQty",
                    title: "Roll Qty (Kg)",
                    visible: _subGroup != 1,
                    cellStyle: function () { return { classes: 'm-w-100' } }
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
            onEditableSave: function (value, row, index, field) {
                var remainQty = parseFloat($tblKProductionE1.data("roll").RollQty) - parseFloat(row.RollQty);
                var rows = $tblRollEl.bootstrapTable('getData');
                for (var i = 0; i < rows.length; i++) {
                    if (row.RollNo != rows[i].RollNo) {
                        rows[i].RollQty = (parseFloat(remainQty) / parseInt(rows.length - 1)).toFixed(2);
                    }
                }
                $tblRollEl.bootstrapTable("load", rows);
                $tblRollEl.bootstrapTable('hideLoading');
            }
        });
    }

    function split() {
        var splitList = [];
        removingRollNo = $tblKProductionE1.data("roll").RollNo;
        //alert(removingRollNo);
        for (var i = 0; i < parseInt($formEl.find("#NoOfSplit").val()); i++) {
            var obj = {
                GRollID: $tblKProductionE1.data("roll").GRollID,
                RollNo: $tblKProductionE1.data("roll").RollNo + '_' + (i + 1),
                //RollQty: (parseFloat($tblKProductionE1.data("roll").RollQty) / parseInt($formEl.find("#NoOfSplit").val())).toFixed(2),
                ProdQty: parseFloat($tblKProductionE1.data("roll").ProdQty),
                InActive: 0,
                IsSave: false   //just for check save item or not
            }
            if (_subGroup == 1) {
                obj.RollQty = (parseFloat($tblKProductionE1.data("roll").RollQty) / parseInt($formEl.find("#NoOfSplit").val())).toFixed(2);
                obj.RollQtyPcs = 0;
            } else {
                obj.RollQtyPcs = (parseInt($tblKProductionE1.data("roll").RollQtyPcs) / parseInt($formEl.find("#NoOfSplit").val()));
                obj.RollQty = (parseFloat($tblKProductionE1.data("roll").RollQty) / parseInt($formEl.find("#NoOfSplit").val())).toFixed(2);;
            }
            splitList.push(obj);
        }
        $tblRollEl.bootstrapTable("load", splitList);
        $tblRollEl.bootstrapTable('hideLoading');
    }

    function showRoll() {
        var gRollID = $tblKProductionE1.data("roll").GRollID;
        axios.get(`/api/yd-batch/get-roll/${gRollID}`)
            .then(function (response) {
                if (response.data.length > 0) {
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

    function saveKProd() {
        var rows = $tblRollEl.bootstrapTable('getData');
        axios.post("/api/yd-batch/save-kProd", rows)
            .then(function (response) {
                $tblRollEl.bootstrapTable("load", response.data.newSplitedList);
                $tblRollEl.bootstrapTable('hideLoading');
                toastr.success("Saved successfully!");
                $formEl.find("#btnSaveKProd").fadeOut();
                $('#modal-child').modal('hide');
                masterData.KnittingProductions = response.data.totalUpdatedList;
                var list = [];
                if (_subGroup == 1) {
                    list = masterData.KnittingProductions.filter(el => el.ConceptID == masterData.YDBatchItemRequirements[_rowIndex].ConceptID);
                    masterData.YDBatchItemRequirements[_rowIndex].YDBatchChilds = [];
                    $tblRequirementEl.bootstrapTable('updateCellById', { id: masterData.YDBatchItemRequirements[_rowIndex].RecipeItemInfoID, field: 'Qty', value: 0 });

                } else {
                    list = masterData.KnittingProductions.filter(el => el.ConceptID == masterData.YDBatchOtherItemRequirements[_rowIndex].ConceptID);
                    masterData.YDBatchOtherItemRequirements[_rowIndex].YDBatchChilds = [];
                    $tblOthersRequirementEl.bootstrapTable('updateCellById', { id: masterData.YDBatchOtherItemRequirements[_rowIndex].RecipeItemInfoID, field: 'Qty', value: 0 });
                    $tblOthersRequirementEl.bootstrapTable('updateCellById', { id: masterData.YDBatchOtherItemRequirements[_rowIndex].RecipeItemInfoID, field: 'Pcs', value: 0 });
                }
                $tblKProductionE1.bootstrapTable("load", list);
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    window.showTblRequirementChild = function (conceptID, ind, subGroup) {
        childIndex = ind;
        _subGroup = subGroup;
        initRequirementsChildTable(subGroup);
        var list = masterData.KnittingProductions.filter(function (el) { return el.ConceptID == conceptID; });
        $tblRequirementsChildE1.bootstrapTable("load", list);
        $tblRequirementsChildE1.bootstrapTable('hideLoading');
        $("#modal-item-req-child").modal('show');
    }

    window.showProductionRoll = function (conceptID, ind, subGroup) {
        childIndex = ind;
        _subGroup = subGroup;
        var list = masterData.KnittingProductions.filter(function (el) { return el.ConceptID == conceptID; });
        initKProductionTable(subGroup);
        $tblKProductionE1.bootstrapTable("load", list);
        $tblKProductionE1.bootstrapTable('hideLoading');
        $("#modal-splitted-roll").modal('show');

        _rowIndex = ind;

    }

    function initKProductionTable(subGroup) {
        $tblKProductionE1.bootstrapTable("destroy");
        $tblKProductionE1.bootstrapTable({
            showFooter: true,
            columns: [
                {
                    field: "RollNo",
                    title: "Roll No",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "RollQty",
                    title: "Roll Qty (Kg)",
                    cellStyle: function () { return { classes: 'm-w-80' } }
                },
                {
                    field: "RollQtyPcs",
                    title: "Roll Qty (Pcs)",
                    visible: subGroup != 1,
                    cellStyle: function () { return { classes: 'm-w-80' } }
                },
                {
                    title: "",
                    width: 20,
                    formatter: function (value, row, index, field) {
                        return `<a class="btn btn-xs btn-default plan" href="javascript:void(0)" title="Split">
                                    <i class="fa fa-tasks" aria-hidden="true"></i> Splits
                                </a>`;
                    },
                    events: {
                        'click .plan': function (e, value, row, index) {
                            e.preventDefault();
                            initRollTable();
                            $tblKProductionE1.data("roll", row);
                            $tblKProductionE1.data("index", index);
                            showRoll();
                            totalQty = row.RollQty;
                        }
                    }
                }
            ]
        });
    }
})();
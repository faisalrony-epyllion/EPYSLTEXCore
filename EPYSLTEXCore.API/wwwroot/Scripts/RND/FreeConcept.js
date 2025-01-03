﻿(function () {
    var menuId, pageName;
    var status = freeConceptStatus.All;
    var toolbarId, pageId, $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, tblMasterId, tblChildId, $tblOtherItemEl, tblOtherItemId, $tblFabricItemEl, tblFabricItemId, tblCreateCompositionId, $tblCreateCompositionEl, $tblChildSetEl, tblChildSetId, $tblChildSetDetailsEl, tblChildSetDetailsId;
    var masterData, _currentRowForDD = {};
    var compositionComponents = [];
    var _conceptId = 9999;

    $(function () {

        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $pageEl = $(`#${pageId}`);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblOtherItemId = "#tblOtherItem" + pageId;
        tblFabricItemId = "#tblFabricItem" + pageId;
        tblChildSetId = "#tblChildSet" + pageId;
        tblChildSetDetailsId = "#tblChildSetDetails" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        tblCreateCompositionId = `#tblCreateComposition-${pageId}`;

        initMasterTable();

        $toolbarEl.find("#btnNew").on("click", getNew);

        $toolbarEl.find("#btnAllList").on("click", { selectedStatus: freeConceptStatus.All }, getList);
        $toolbarEl.find("#btnLiveList").on("click", { selectedStatus: freeConceptStatus.Live }, getList);
        $toolbarEl.find("#btnPendingList").on("click", openPendingBtns);
        $toolbarEl.find("#btnPreservedList").on("click", { selectedStatus: freeConceptStatus.Preserved }, getList);
        $toolbarEl.find("#btnDroppedList").on("click", { selectedStatus: freeConceptStatus.Dropped }, getList);

        $toolbarEl.find("#btnSourcingPendingList").on("click", { selectedStatus: freeConceptStatus.SourcingPending }, getPendingList);
        $toolbarEl.find("#btnYDPendingList").on("click", { selectedStatus: freeConceptStatus.YDPending }, getPendingList);
        $toolbarEl.find("#btnKnitPendingList").on("click", { selectedStatus: freeConceptStatus.KnitPending }, getPendingList);
        $toolbarEl.find("#btnBatchPendingList").on("click", { selectedStatus: freeConceptStatus.BatchPending }, getPendingList);
        $toolbarEl.find("#btnDyeingPendingList").on("click", { selectedStatus: freeConceptStatus.DyeingPending }, getPendingList);
        $toolbarEl.find("#btnFinishPendingList").on("click", { selectedStatus: freeConceptStatus.FinishPending }, getPendingList);
        $toolbarEl.find("#btnTestPendingList").on("click", { selectedStatus: freeConceptStatus.TestPending }, getPendingList);
        $toolbarEl.find("#btnWaitingForLiveList").on("click", { selectedStatus: freeConceptStatus.WaitingForLivePending }, getPendingList);

        $formEl.find("#btn-add-composition").on("click", showAddComposition);

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#btnSave").click(save);
        $formEl.find("#btnRevise").click(revise);
        $formEl.find("#btnUpdate").click(function () {
            update();
        });
        $formEl.find("#btnAddColor").on("click", addColor);

        $formEl.find('#MCSubClassID').on('select2:select', function (e) {
            var subClassId = e.params.data.id;
            var url = `/api/selectoption/get-machine-by-subclass/${subClassId}`;
            axios.get(url)
                .then(function (response) {
                    masterData.KnittingTypeID = response.data[0].id;
                    $formEl.find("#KnittingType").val(response.data[0].text);
                    if (response.data[0].text == 'Flat Knit' || response.data[0].text == 'Flat Bed') {
                        $formEl.find("#lblQty").text("Quantity(Pcs)");
                        $formEl.find("#SubGroupID").find('option[value="1"]').prop("disabled", true);
                        $formEl.find("#SubGroupID").select2();
                        $formEl.find('#SubGroupID').val('0').trigger("change");
                        $formEl.find('#divSubGroup').fadeIn();
                    }
                    else {
                        $formEl.find("#lblQty").text("Quantity(Kg)");
                        $formEl.find("#SubGroupID").find('option[value="1"]').prop("disabled", false);
                        $formEl.find("#SubGroupID").select2();
                        $formEl.find('#SubGroupID').val('1').trigger("change"); //fabric
                        $formEl.find('#divSubGroup').fadeOut();
                    }
                })

            axios.get(`/api/rnd-free-concept/technical-names/${subClassId}`)
                .then(function (response) {
                    masterData.TechnicalNameList = response.data.TechnicalNameList;
                    initSelect2($formEl.find("#TechnicalNameId"), response.data.TechnicalNameList);
                })
                .catch(showResponseError);
        });

        $pageEl.find("#btnAddComposition").click(saveComposition);

        $formEl.find('.chkConceptType[type="checkbox"]').change(function () {
            $formEl.find('.chkConceptType[type="checkbox"]').not(this).prop('checked', false);
            if ($formEl.find("#onlyFabric").is(':checked')) {
                $formEl.find("#divFabricItem").fadeIn();
                $formEl.find("#divOtherItem").fadeOut();
            } else if ($formEl.find("#fabricOtherItem").is(':checked')) {
                $formEl.find("#divFabricItem, #divOtherItem").fadeIn();
            } else {
                $formEl.find("#divOtherItem").fadeIn();
                $formEl.find("#divFabricItem").fadeOut();
            }
        });
    });

    function initMasterTable() {
        var columns = [
            {
                headerText: '', commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }], width: 15, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'GroupConceptNo', headerText: 'Concept No', width: 30
            },
            {
                field: 'ConcepTypeName', headerText: 'Concept Type', width: 50
            },
            {
                field: 'ConceptDate', headerText: 'Concept Date', textAlign: 'Right', type: 'date', format: _ch_date_format_1, width: 40, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'TechnicalName', headerText: 'Technical Name', width: 70
            },
            //{
            //    field: 'Composition', headerText: 'Composition', width: 100
            //},
            //{
            //    field: 'GSM', headerText: 'GSM', width: 25, textAlign: 'Center', headerTextAlign: 'Center'
            //},
            //{
            //    field: 'Qty', headerText: 'QTY (kg)', width: 25, textAlign: 'Center', headerTextAlign: 'Center'
            //},
            {
                field: 'UserName', headerText: 'Concept By', width: 60
            },
            {
                field: 'LiveStatus', headerText: 'Live Status', width: 60
            },
            {
                field: 'StatusRemarks', headerText: 'Status Remarks', width: 140
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/rnd-free-concept/list?status=${status}`,
            columns: columns,
            showColumnChooser: true,
            allowExcelExport: true,
            allowPdfExport: true,
            autofitColumns: true,
            allowSorting: true,
            showDefaultToolbar: false,
            toolbar: ['ColumnChooser', 'ExcelExport'],
            handleToolbarClick: toolbarClickExcelEvent,
            commandClick: handleCommands
        });
    }

    function toolbarClickExcelEvent(args) {
        if (args['item'].id.indexOf('_excelexport') >= 0) {
            $tblMasterEl.excelExport();
        } else if (args['item'].id.indexOf('_pdfexport') >= 0) {
            var exportProperties = {
                bAllowHorizontalOverflow: false
            };
            $tblMasterEl.pdfExport(exportProperties);
        }
    }

    function handleCommands(args) {
        //if (!args.rowData.RevisionPending) getDetails(args.rowData.ConceptID, args.rowData.MCSubClassID);
        //else getRevisionedDetails(args.rowData.ConceptID, args.rowData.MCSubClassID);
        if (args.commandColumn.type == 'Edit')
            getDetailsByGroupConcept(args.rowData.GroupConceptNo, args.rowData.ConceptTypeID)
    }

    function initChildTable(records) {
        if ($tblChildEl) $tblChildEl.destroy();
        records.map(x => {
            if (typeof x.IsLive === "undefined") x.IsLive = false;
        });
        $tblChildEl = new initEJ2Grid({
            tableId: tblChildId,
            data: records,
            autofitColumns: false,
            allowSorting: true,
            allowPaging: false,
            allowFiltering: false,
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                { field: 'CCColorID', isPrimaryKey: true, visible: false },
                { field: 'ConceptID', headerText: 'ConceptID', visible: false },
                {
                    headerText: 'Action', commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
                { field: 'ColorCode', headerText: 'Code', allowEditing: false },
                { field: 'ColorName', headerText: 'Name', allowEditing: false },
                { field: 'Color', headerText: 'Visual', uid: "RGBOrHex", allowEditing: false, valueAccessor: ej2GridColorFormatter },
                { field: 'Remarks', headerText: 'Remarks' },
                { field: 'IsRecipeDone', headerText: 'IsRecipeDone', visible: false },
                { field: 'IsLive', headerText: 'Live?', allowEditing: false, displayAsCheckBox: true, textAlign: 'Center' }
            ],
            actionBegin: function (args) {
                if (args.requestType === "delete") {
                    if (args.data[0].IsRecipeDone) {
                        toastr.error("Already used in recipe.");
                        var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.CCColorID);
                        $tblChildEl.updateRow(index, args.data);
                        args.cancel = true;
                    }
                }
            },
        });
    }

    function initOtherItemTable(records) {
        if ($tblOtherItemEl) $tblOtherItemEl.destroy();

        $tblOtherItemEl = new initEJ2Grid({
            tableId: tblOtherItemId,
            data: records,
            autofitColumns: false,
            allowSorting: true,
            allowPaging: false,
            allowFiltering: false,
            showDefaultToolbar: false,
            enableSingleClickEdit: true,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.ConceptID = _conceptId++; //getMaxIdForArray($tblOtherItemEl.getCurrentViewRecords(), "ConceptID");
                } else if (args.requestType === "save" && args.action == "edit") {
                    args.data.FUPartName = _currentRowForDD.FUPartName;
                    args.data.MCSubClassName = _currentRowForDD.MCSubClassName;
                    args.data.TechnicalName = _currentRowForDD.TechnicalName;
                    args.data.SubGroupID = _currentRowForDD.SubGroupID;
                    args.data.KnittingTypeID = _currentRowForDD.KnittingTypeID;
                    var index = $tblOtherItemEl.getRowIndexByPrimaryKey(args.rowData.ConceptID);
                    $tblOtherItemEl.updateRow(index, args.data);
                } else if (args.requestType === "delete") {
                    var colors = masterData.ChildColors.filter(x => x.ConceptID == args.data[0].ConceptID);
                    if (colors.length > 0) {
                        toastr.error('Cannot delete, because has color with this concept.');
                        return false;
                    }
                }
            },
            toolbar: ['Add'],
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Action', width: 14, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
                { field: 'ConceptID', isPrimaryKey: true, visible: false },
                { field: 'SubGroupID', visible: false },
                {
                    field: 'FUPartID', headerText: 'End Use', width: 20, valueAccessor: ej2GridDisplayFormatterV2, edit: ej2GridDropDownObjV2({
                        displayField: "FUPartName",
                        dataSource: masterData.FabricUsedPartList,
                        onChange: function (selectedData, currentRowData) {
                            currentRowData.SubGroupID = selectedData.desc;
                            _currentRowForDD.SubGroupID = selectedData.desc;
                            _currentRowForDD.FUPartName = currentRowData.FUPartName;
                        }
                    })
                },
                {
                    field: 'MCSubClassID', headerText: 'Machine Type', width: 20, valueAccessor: ej2GridDisplayFormatterV2, edit: ej2GridDropDownObjV2({
                        displayField: "MCSubClassName",
                        dataSource: masterData.OtherMCSubClassList,
                        onChange: function (selectedData, currentRowData) {
                            currentRowData.KnittingTypeID = selectedData.desc;
                            _currentRowForDD.KnittingTypeID = selectedData.desc;
                            _currentRowForDD.MCSubClassName = currentRowData.MCSubClassName;
                        }
                    })
                },
                {
                    field: 'TechnicalNameId', headerText: 'Technical Name', width: 20, valueAccessor: ej2GridDisplayFormatterV2, edit: ej2GridDropDownObjV2({
                        displayField: "TechnicalName",
                        //filterBy: "MCSubClassID",
                        //apiEndPoint: `/api/rnd-free-concept/technicalName-by-mc`, //${mcSubClassId}
                        dataSource: masterData.OtherTechnicalNameList,
                        onChange: function (selectedData, currentRowData) {
                            _currentRowForDD.TechnicalName = currentRowData.TechnicalName;
                        }
                    })
                },
                {
                    field: 'MachineGauge', headerText: 'Machine Gauge', width: 20, valueAccessor: ej2GridDisplayFormatterV2, edit: ej2GridDropDownObjV2({
                        displayField: "MachineGauge",
                        dataSource: masterData.MachineGaugeList,
                        onChange: function (selectedData, currentRowData) {
                            _currentRowForDD.TechnicalName = currentRowData.TechnicalName;
                        }
                    })
                },
                { field: 'Length', headerText: 'Length (CM)', editType: "numericedit", width: 20, edit: { params: { showSpinButton: false, decimals: 2, min: 1 } } },
                { field: 'Width', headerText: 'Height (CM)', editType: "numericedit", width: 20, edit: { params: { showSpinButton: false, decimals: 2, min: 1 } } },
                { field: 'Qty', headerText: 'Qty (Pcs)', editType: "numericedit", width: 20, edit: { params: { showSpinButton: false, decimals: 2, min: 1 } } }
            ]
        });
    }

    var machineTypeElem;
    var machineTypeObj;
    var technicalNameElem;
    var technicalNameObj;

    function initFabricItemTable(records) {

        if ($tblFabricItemEl) $tblFabricItemEl.destroy();
        $tblFabricItemEl = new initEJ2Grid({
            tableId: tblFabricItemId,
            data: records,
            autofitColumns: false,
            allowPaging: false,
            allowFiltering: false,
            showDefaultToolbar: false,
            enableSingleClickEdit: true,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.ConceptID = _conceptId++; // getMaxIdForArray($tblFabricItemEl.getCurrentViewRecords(), "ConceptID");
                }
                else if (args.requestType == "save") {
                    args.data.SubGroupID = 1;
                    if (args.data.MCSubClassID > 0 && args.data.MCSubClassName == null) {
                        var obj = masterData.MCSubClassList.find(x => parseInt(x.id) == parseInt(args.data.MCSubClassID));
                        if (obj) {
                            args.data.MCSubClassName = obj.text;
                        }
                    }
                    if (args.data.TechnicalNameId > 0 && args.data.TechnicalName == null) {
                        var obj = masterData.TechnicalNameList.find(x => parseInt(x.id) == parseInt(args.data.TechnicalNameId));
                        if (obj) {
                            args.data.TechnicalName = obj.text;
                        }
                    }
                }
                else if (args.action == "edit") {
                    alert(1);
                }
                else if (args.requestType === "delete") {
                    var colors = masterData.ChildColors.filter(x => x.ConceptID == args.data[0].ConceptID);
                    if (colors.length > 0) {
                        toastr.error('Cannot delete, because has color with this concept.');
                        args.cancel = true;
                    }
                }
            },
            toolbar: ['Add'],
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Action', commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
                { field: 'ConceptID', isPrimaryKey: true, visible: false },
                { field: 'SubGroupID', visible: false },

                {
                    field: 'MCSubClassName', headerText: 'Machine Type',
                    valueAccessor: ej2GridDisplayFormatterV2, edit: {
                        create: function () {
                            machineTypeElem = document.createElement('input');
                            return machineTypeElem;
                        },
                        read: function () {
                            return machineTypeObj.text;
                        },
                        destroy: function () {
                            machineTypeObj.destroy();
                        },
                        write: function (e) {
                            machineTypeObj = new ej.dropdowns.DropDownList({
                                dataSource: masterData.MCSubClassList,
                                fields: { value: 'id', text: 'text' },
                                change: function (f) {
                                    technicalNameObj.enabled = true;

                                    var tempQuery = new ej.data.Query().where('additionalValue', 'equal', machineTypeObj.value);
                                    technicalNameObj.query = tempQuery;
                                    technicalNameObj.text = null;
                                    technicalNameObj.dataBind();

                                    e.rowData.MCSubClassID = f.itemData.id;
                                    e.rowData.MCSubClassName = f.itemData.text;
                                    e.rowData.KnittingTypeID = f.itemData.desc;
                                },
                                placeholder: 'Select M/C Type',
                                floatLabelType: 'Never'
                            });
                            machineTypeObj.appendTo(machineTypeElem);
                        }
                    }
                },
                {
                    field: 'TechnicalName', headerText: 'Technical Name', displayField: "TechnicalName", valueAccessor: ej2GridDisplayFormatterV2, edit: {
                        create: function () {
                            technicalNameElem = document.createElement('input');
                            return technicalNameElem;
                        },
                        read: function () {
                            return technicalNameObj.text;
                        },
                        destroy: function () {
                            technicalNameObj.destroy();
                        },
                        write: function (e) {
                            technicalNameObj = new ej.dropdowns.DropDownList({
                                dataSource: masterData.TechnicalNameList, //.TechnicalNameList,//.filter(x => x.id == _machineTypeId),
                                fields: { value: 'id', text: 'text' },
                                //enabled: false,
                                placeholder: 'Select Technical Name',
                                floatLabelType: 'Never',
                                change: function (f) {
                                    if (!f.isInteracted || !f.itemData) return false;

                                    e.rowData.TechnicalNameId = f.itemData.id;
                                    e.rowData.TechnicalName = f.itemData.text;

                                    //$tblFabricItemEl.updateRow(e.row.rowIndex, e.rowData);
                                }
                            });
                            technicalNameObj.appendTo(technicalNameElem);
                        }
                    }
                },
                {
                    field: 'CompositionId', headerText: 'Composition', valueAccessor: ej2GridDisplayFormatter,
                    dataSource: masterData.CompositionList,
                    displayField: "Composition",
                    edit: ej2GridDropDownObj({
                        width: 250
                    })
                },
                //{
                //    field: 'CompositionId', headerText: 'Composition', valueAccessor: ej2GridDisplayFormatterV2, edit: ej2GridDropDownObjV2({
                //        displayField: "Composition",
                //        dataSource: masterData.CompositionList,
                //        width: 350,
                //        onChange: function (selectedData, currentRowData) {
                //        }
                //    })
                //},
                {
                    field: 'GSMId', headerText: 'GSM', valueAccessor: ej2GridDisplayFormatterV2, edit: ej2GridDropDownObjV2({
                        displayField: "GSM",
                        dataSource: masterData.GSMList,
                        onChange: function (selectedData, currentRowData) {
                        }
                    })
                },
                {
                    field: 'Qty', headerText: 'Quantity(pcs/kg)', editType: "numericedit",
                    edit: { params: { showSpinButton: false, decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } }
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
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#ConceptID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getNew(e) {
        //$divDetailsEl.removeClass('d-none');
        e.preventDefault();
        axios.get("/api/rnd-free-concept/new")
            .then(function (response) {
                $("#divRetrial").fadeOut();
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
                masterData.TrialDate = formatDateToDefault(masterData.TrialDate);
                //masterData.SubGroupList.unshift({ id: 0, text: 'Set' });
                setFormData($formEl, masterData);
                //$("#structure-based").prop("checked", true);
                //$formEl.find("#divChild").fadeOut();
                initChildTable([]);
                initOtherItemTable([]);
                initFabricItemTable([]);
                //initChildSetTable([{ ReqQty: 100, Remarks: 'Test'}]);
                $formEl.find("#onlyFabric").prop('checked', true);
                $formEl.find("#divFabricItem").fadeIn();
                $formEl.find("#divOtherItem").fadeOut();
                $formEl.find("#btnRevise").fadeOut();
                $formEl.find("#btnSave").fadeIn();
                $formEl.find("#btnUpdate").fadeOut();
            })
            .catch(showResponseError);
    }

    function getRevisionedDetails(id, subClassId) {
        axios.get(`/api/rnd-free-concept/revision/${id}/${subClassId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
                masterData.TrialDate = formatDateToDefault(masterData.TrialDate);
                //masterData.SubGroupList.unshift({ id: 0, text: 'Set' });
                setFormData($formEl, masterData);
                //if (masterData.ConceptFor == '1092')
                //    $formEl.find("#divChild").fadeOut();
                //else
                //    $formEl.find("#divChild").fadeIn();

                if (masterData.KnittingType == "Single Jersey" || masterData.KnittingType == "Double Jersey") {
                    $formEl.find('#divSubGroup').fadeOut();
                } else {
                    $formEl.find('#divSubGroup').fadeIn();
                }

                if (masterData.TrialNo == 0) {
                    $("#divRetrial").fadeOut();
                } else {
                    $("#divRetrial").fadeIn();
                }

                initChildTable(masterData.ChildColors);
            })
            .catch(showResponseError);
    }
    function openPendingBtns(e) {
        $toolbarEl.find("#divPendingButtons").fadeIn();
        changeBtnClassLeft(e.currentTarget.id);
        changeBtnClassRight("btnSourcingPendingList");
        status = "sourcingPending";
        initMasterTable();
    }
    function getList(e) {
        $toolbarEl.find("#divPendingButtons").fadeOut();
        changeBtnClassLeft(e.currentTarget.id);
        status = e.data.selectedStatus;
        initMasterTable();
    }
    function getPendingList(e) {
        changeBtnClassRight(e.currentTarget.id);
        status = e.data.selectedStatus;
        initMasterTable();
    }
    function changeBtnClassLeft(btnId) {
        $toolbarEl.find(".btn-list-left").removeClass("btn-success").addClass("btn-default");
        $toolbarEl.find("#" + btnId).addClass("btn-success").removeClass("btn-default");
    }
    function changeBtnClassRight(btnId) {
        $toolbarEl.find(".btn-list-right").removeClass("btn-success").addClass("btn-default");
        $toolbarEl.find("#" + btnId).addClass("btn-success").removeClass("btn-default");
    }
    function getDetails(id, subClassId) {

        axios.get(`/api/rnd-free-concept/${id}/${subClassId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
                masterData.TrialDate = formatDateToDefault(masterData.TrialDate);
                //masterData.SubGroupList.unshift({ id: 0, text: 'Set' });
                setFormData($formEl, masterData);
                //if (masterData.ConceptFor == '1092')
                //    $formEl.find("#divChild").fadeOut();
                //else
                //    $formEl.find("#divChild").fadeIn();

                if (masterData.KnittingType == "Single Jersey" || masterData.KnittingType == "Double Jersey") {
                    $formEl.find('#divSubGroup').fadeOut();
                } else {
                    $formEl.find('#divSubGroup').fadeIn();
                }

                if (masterData.TrialNo == 0) {
                    $("#divRetrial").fadeOut();
                } else {
                    $("#divRetrial").fadeIn();
                }

                initChildTable(masterData.ChildColors);
            })
            .catch(showResponseError);
    }

    function getDetailsByGroupConcept(grpConceptNo, conceptTypeID) {

        axios.get(`/api/rnd-free-concept/by-group-concept/${grpConceptNo}/${conceptTypeID}`)
            .then(function (response) {
                $formEl.find("#btnUpdate").fadeIn();
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                //console.log(masterData);
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
                masterData.TrialDate = formatDateToDefault(masterData.TrialDate);
                setFormData($formEl, masterData);

                if (masterData.KnittingType == "Single Jersey" || masterData.KnittingType == "Double Jersey") {
                    $formEl.find('#divSubGroup').fadeOut();
                } else {
                    $formEl.find('#divSubGroup').fadeIn();
                }

                if (masterData.TrialNo == 0) {
                    $("#divRetrial").fadeOut();
                } else {
                    $("#divRetrial").fadeIn();
                }

                initChildTable(masterData.ChildColors);
                initFabricItemTable(masterData.FabricItems);
                initOtherItemTable(masterData.OtherItems);

                if (masterData.NeedRevision) {
                    $formEl.find("#btnRevise").fadeIn();
                    $formEl.find("#btnSave").fadeOut();
                    if (status == freeConceptStatus.YDPending || status == freeConceptStatus.KnitPending || status == freeConceptStatus.BatchPending || status == freeConceptStatus.DyeingPending || status == freeConceptStatus.FinishPending || status == freeConceptStatus.TestPending) {
                        $formEl.find("#btnRevise").fadeOut();
                    }
                } else {
                    $formEl.find("#btnRevise").fadeOut();
                    if (status == freeConceptStatus.YDPending || status == freeConceptStatus.KnitPending || status == freeConceptStatus.BatchPending || status == freeConceptStatus.DyeingPending || status == freeConceptStatus.FinishPending || status == freeConceptStatus.TestPending) {
                        $formEl.find("#btnSave").fadeOut();
                    }
                    //else {
                    //    $formEl.find("#btnSave").fadeIn();
                    //}
                    if (status == freeConceptStatus.All) {
                        $formEl.find("#btnSave").fadeIn();
                    }
                }
                if (conceptTypeID == 1) {   //Only Fabric
                    $formEl.find("#divFabricItem").fadeIn();
                    $formEl.find("#divOtherItem").fadeOut();
                    $formEl.find("#onlyFabric").prop('checked', true);
                } else if (conceptTypeID == 2) {    //Fabric & Other Item
                    $formEl.find("#divFabricItem, #divOtherItem").fadeIn();
                    $formEl.find("#fabricOtherItem").prop('checked', true);
                } else {    //Other Item
                    $formEl.find("#divOtherItem").fadeIn();
                    $formEl.find("#divFabricItem").fadeOut();
                    $formEl.find("#onlyOtherItem").prop('checked', true);
                }
            })
            .catch(showResponseError);
    }

    function save(e) {
        e.preventDefault();
        var concepts = [];
        var colorList = $tblChildEl.getCurrentViewRecords();

        var fabricList = [];
        var collarCuffList = [];

        var isFabric = $formEl.find("#onlyFabric").is(':checked');
        var isFabricAndFlatKnit = $formEl.find("#fabricOtherItem").is(':checked');
        var isFlatKnit = $formEl.find("#onlyOtherItem").is(':checked');

        if (isFabric) {
            conceptType = 1;
            fabricList = $tblFabricItemEl.getCurrentViewRecords();
        }
        else if (isFabricAndFlatKnit) {
            conceptType = 2;
            fabricList = $tblFabricItemEl.getCurrentViewRecords();
            collarCuffList = $tblOtherItemEl.getCurrentViewRecords();
        }
        else if (isFlatKnit) {
            conceptType = 3;
            collarCuffList = $tblOtherItemEl.getCurrentViewRecords();
        }

        var hasError = false;
        for (var i = 0; i < fabricList.length; i++) {
            var str = ' at row (Fabric Information) ' + (i + 1);
            var obj = fabricList[i];

            var objTemp = masterData.MCSubClassList.find(x => x.text == obj.MCSubClassName);
            if (objTemp) obj.MCSubClassID = objTemp.id;

            objTemp = masterData.TechnicalNameList.find(x => x.text == obj.TechnicalName);
            if (objTemp) obj.TechnicalNameId = objTemp.id;

            if (obj.MCSubClassID == '0' || obj.MCSubClassID == undefined) {
                toastr.error("Machine Type can't be blank" + str);
                hasError = true;
                break;
            }
            else if (obj.TechnicalNameId == '0' || obj.TechnicalNameId == undefined) {
                toastr.error("Technical name can't be blank" + str);
                hasError = true;
                break;
            }
            else if (obj.CompositionId == '0' || obj.CompositionId == undefined) {
                toastr.error("Composition can't be blank" + str);
                hasError = true;
                break;
            }
            else if (obj.GSMId == '0' || obj.GSMId == undefined) {
                toastr.error("GSM can't be blank" + str);
                hasError = true;
                break;
            }
            else if (parseInt(obj.Qty) == 0) {
                toastr.error("Qty must be greater than 0" + str);
                hasError = true;
                break;
            }
            else if (parseInt(obj.Qty) > 50) {
                toastr.error("Qty not more than 50" + str);
                hasError = true;
                break;
            }

            obj.SubGroupID = 1;
            obj.ConceptTypeID = conceptType;
            obj.GroupConceptNo = $formEl.find('#GroupConceptNo').val();
            obj.ChildColors = i == 0 ? colorList : [];
            obj.Remarks = $.trim($formEl.find("#Remarks").val());

            concepts.push(obj);
        }
        if (hasError) return;

        for (var i = 0; i < collarCuffList.length; i++) {
            var str = ' at row (Flat Knit Information) ' + (i + 1);
            var obj = collarCuffList[i];

            var objTemp = masterData.MCSubClassList.find(x => x.text == obj.MCSubClassName);
            if (objTemp) obj.MCSubClassID = objTemp.id;

            objTemp = masterData.TechnicalNameList.find(x => x.text == obj.TechnicalName);
            if (objTemp) obj.TechnicalNameId = objTemp.id;

            obj.ConceptTypeID = conceptType;
            obj.GroupConceptNo = $formEl.find('#GroupConceptNo').val();
            obj.ChildColors = fabricList.length == 0 && i == 0 ? colorList : [];

            concepts.push(obj);
        }
        if (hasError) return;
        var config = {
            headers: {
                'Content-Type': 'application/json'
            }
        };
        debugger;
        axios.post("/api/rnd-free-concept/save", concepts, config)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function update() {

        var conceptID = 0;
        var childColors = $tblChildEl.getCurrentViewRecords();
        var conceptObj = childColors.find(x => x.ConceptID > 0);
        if (conceptObj) {
            conceptID = conceptObj.ConceptID;
        }

        if (conceptID == 0) {

            var isFabric = $formEl.find("#onlyFabric").is(':checked');
            var isFabricAndFlatKnit = $formEl.find("#fabricOtherItem").is(':checked');
            var isFlatKnit = $formEl.find("#onlyOtherItem").is(':checked');

            if (isFabric) {
                conceptType = 1;
                fabricList = $tblFabricItemEl.getCurrentViewRecords();
            }
            else if (isFabricAndFlatKnit) {
                conceptType = 2;
                fabricList = $tblFabricItemEl.getCurrentViewRecords();
                collarCuffList = $tblOtherItemEl.getCurrentViewRecords();
            }
            else if (isFlatKnit) {
                conceptType = 3;
                collarCuffList = $tblOtherItemEl.getCurrentViewRecords();
            }

            conceptObj = fabricList.find(x => x.ConceptID > 0);
            if (conceptObj) {
                conceptID = conceptObj.ConceptID;
            }
            if (conceptID == 0) {
                conceptObj = collarCuffList.find(x => x.ConceptID > 0);
                if (conceptObj) {
                    conceptID = conceptObj.ConceptID;
                }
            }
        }
        childColors.map(x => {
            x.ConceptID = conceptID;
        });
        axios.post(`/api/rnd-free-concept/updateColor`, childColors)
            .then(function () {
                toastr.success("Updated successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function revise(e) {
        e.preventDefault();

        var concepts = [];
        var colorList = $tblChildEl.getCurrentViewRecords();

        var fabricList = [];
        var collarCuffList = [];

        var isFabric = $formEl.find("#onlyFabric").is(':checked');
        var isFabricAndFlatKnit = $formEl.find("#fabricOtherItem").is(':checked');
        var isFlatKnit = $formEl.find("#onlyOtherItem").is(':checked');

        if (isFabric) {
            conceptType = 1;
            fabricList = $tblFabricItemEl.getCurrentViewRecords();
        }
        else if (isFabricAndFlatKnit) {
            conceptType = 2;
            fabricList = $tblFabricItemEl.getCurrentViewRecords();
            collarCuffList = $tblOtherItemEl.getCurrentViewRecords();
        }
        else if (isFlatKnit) {
            conceptType = 3;
            collarCuffList = $tblOtherItemEl.getCurrentViewRecords();
        }

        var hasError = false;
        for (var i = 0; i < fabricList.length; i++) {
            var str = ' at row (Fabric Information) ' + (i + 1);
            var obj = fabricList[i];

            var objTemp = masterData.MCSubClassList.find(x => x.text == obj.MCSubClassName);
            if (objTemp) obj.MCSubClassID = objTemp.id;

            objTemp = masterData.TechnicalNameList.find(x => x.text == obj.TechnicalName);
            if (objTemp) obj.TechnicalNameId = objTemp.id;

            if (obj.TechnicalNameId == '0' || obj.TechnicalNameId == undefined) {
                toastr.error("Technical name id can't be blank" + str);
                hasError = true;
                break;
            }
            else if (obj.MCSubClassID == '0' || obj.MCSubClassID == undefined) {
                toastr.error("Machine Type can't be blank" + str);
                hasError = true;
                break;
            }
            else if (parseInt(obj.Qty) == 0) {
                toastr.error("Qty must be greater than 0" + str);
                hasError = true;
                break;
            }
            else if (parseInt(obj.Qty) > 50) {
                toastr.error("Qty not more than 50" + str);
                hasError = true;
                break;
            }

            obj.SubGroupID = 1;
            obj.ConceptTypeID = conceptType;
            obj.GroupConceptNo = $formEl.find('#GroupConceptNo').val();
            obj.ChildColors = colorList;
            obj.Remarks = $.trim($formEl.find("#Remarks").val());

            concepts.push(obj);
        }
        if (hasError) return;

        for (var i = 0; i < collarCuffList.length; i++) {
            var str = ' at row (Flat Knit Information) ' + (i + 1);
            var obj = collarCuffList[i];

            var objTemp = masterData.MCSubClassList.find(x => x.text == obj.MCSubClassName);
            if (objTemp) obj.MCSubClassID = objTemp.id;

            objTemp = masterData.TechnicalNameList.find(x => x.text == obj.TechnicalName);
            if (objTemp) obj.TechnicalNameId = objTemp.id;

            obj.ConceptTypeID = conceptType;
            obj.GroupConceptNo = $formEl.find('#GroupConceptNo').val();
            obj.ChildColors = colorList;

            concepts.push(obj);
        }
        if (hasError) return;

        axios.post("/api/rnd-free-concept/revise", concepts)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function showAddComposition(e) {
        e.preventDefault();
        initTblCreateComposition();
        $pageEl.find(`#modal-new-composition-${pageId}`).modal("show");
        $pageEl.find(`#modal-new-composition-${pageId}`).removeAttr('tabindex');
    }

    function initTblCreateComposition() {

        var YarnSubProgramNewsFilteredList = [];//masterData.YarnSubProgramNews;
        var CertificationsFilteredList = [];//masterData.Certifications;
        compositionComponents = [];
        var columns = [
            {
                field: 'Id', isPrimaryKey: true, visible: false
            },
            {
                headerText: '', width: 70, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
            },
            {
                field: 'Percent', headerText: 'Percent(%)', width: 120, editType: "numericedit", params: { decimals: 0, format: "N", min: 1, validateDecimalOnType: true }, allowEditing: isBlended
            },
            //{
            //    field: 'Fiber', headerText: 'Component', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.FabricComponents, field: "Fiber" })
            //}
            {
                field: 'Fiber', headerText: 'Yarn Type', valueAccessor: ej2GridDisplayFormatterV2, edit: {
                    create: function () {
                        fiberElem = document.createElement('input');
                        return fiberElem;
                    },
                    read: function () {
                        return fiberObj.text;
                    },
                    destroy: function () {
                        fiberObj.destroy();
                    },
                    write: function (e) {
                        fiberObj = new ej.dropdowns.DropDownList({
                            dataSource: masterData.FabricComponentsNew,
                            fields: { value: 'id', text: 'text' },
                            //enabled: false,
                            placeholder: 'Select Yarn Type',
                            floatLabelType: 'Never',
                            change: async function (f) {

                                if (!f.isInteracted || !f.itemData) return false;
                                e.rowData.Fiber = f.itemData.id;
                                e.rowData.Fiber = f.itemData.text;

                                //YarnSubProgramNewsFilteredList = masterData.YarnSubProgramNews.filter(y => y.additionalValue == f.itemData.id);

                                var list = await axios.get(`/api/rnd-free-concept-mr/yarn-sub-progran-new/${f.itemData.id}`);
                                var YarnSubProgramNewsFilteredList = list.data;

                                subProgramObj.dataSource = YarnSubProgramNewsFilteredList;
                                subProgramObj.dataBind();

                                certificationObj.dataSource = [];
                                certificationObj.dataBind();

                                $tblChildEl.updateRow(e.row.rowIndex, e.rowData);
                            }
                        });
                        fiberObj.appendTo(fiberElem);

                    }
                }
            },
            //{
            //    field: 'YarnSubProgramNew', headerText: 'Yarn Sub Program New', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.YarnSubProgramNews, field: "YarnSubProgramNew" })
            //},
            {
                field: 'YarnSubProgramNew', headerText: 'Yarn Sub Program New', valueAccessor: ej2GridDisplayFormatterV2, edit: {
                    create: function () {
                        subProgramElem = document.createElement('input');
                        return subProgramElem;
                    },
                    read: function () {
                        return subProgramObj.text;
                    },
                    destroy: function () {
                        subProgramObj.destroy();
                    },
                    write: function (e) {
                        subProgramObj = new ej.dropdowns.DropDownList({
                            dataSource: YarnSubProgramNewsFilteredList,
                            fields: { value: 'id', text: 'text' },
                            //enabled: false,
                            placeholder: 'Select Yarn Sub Program',
                            floatLabelType: 'Never',
                            change: async function (f) {

                                if (!f.isInteracted || !f.itemData) return false;
                                e.rowData.YarnSubProgramNew = f.itemData.id;
                                e.rowData.YarnSubProgramNew = f.itemData.text;

                                //CertificationsFilteredList = masterData.Certifications.filter(y => y.additionalValue == f.itemData.id);
                                //CertificationsFilteredList = masterData.Certifications.filter(y => y.additionalValue == f.itemData.id && y.additionalValue2 == f.itemData.additionalValue);

                                var list = await axios.get(`/api/rnd-free-concept-mr/certification/${f.itemData.additionalValue}/${f.itemData.id}`);
                                var CertificationsFilteredList = list.data;


                                certificationObj.dataSource = CertificationsFilteredList;
                                certificationObj.dataBind();

                                $tblChildEl.updateRow(e.row.rowIndex, e.rowData);
                            }
                        });
                        subProgramObj.appendTo(subProgramElem);
                    }
                }
            },
            //{
            //    field: 'Certification', headerText: 'Certification', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.Certifications, field: "Certification" })
            //},
            {
                field: 'Certification', headerText: 'Certification', valueAccessor: ej2GridDisplayFormatterV2, edit: {
                    create: function () {
                        certificationElem = document.createElement('input');
                        return certificationElem;
                    },
                    read: function () {
                        return certificationObj.text;
                    },
                    destroy: function () {
                        certificationObj.destroy();
                    },
                    write: function (e) {
                        certificationObj = new ej.dropdowns.DropDownList({
                            dataSource: CertificationsFilteredList,
                            fields: { value: 'id', text: 'text' },
                            //enabled: false,
                            placeholder: 'Select Certification',
                            floatLabelType: 'Never',
                            change: function (f) {

                                if (!f.isInteracted || !f.itemData) return false;
                                e.rowData.Certification = f.itemData.id;
                                e.rowData.Certification = f.itemData.text;

                                $tblChildEl.updateRow(e.row.rowIndex, e.rowData);
                            }
                        });
                        certificationObj.appendTo(certificationElem);
                    }
                }
            },
            {
                field: 'FiberTypeName', headerText: 'Fiber Type', width: 150, allowEditing: false //, visible: !$formEl.find('#blended').is(':checked')
            },
            {
                field: 'ProgramTypeName', headerText: 'Program', width: 150, allowEditing: false //, visible: !$formEl.find('#blended').is(':checked')
            },
            {
                field: 'ManufacturingLine', headerText: 'Manufacturing Line', width: 150, allowEditing: false //, visible: !$formEl.find('#blended').is(':checked')
            },
        ];

        var gridOptions = {
            tableId: tblCreateCompositionId,
            data: compositionComponents,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    if (isBlended) {
                        if (compositionComponents.length === 5) {
                            toastr.info("You can only add 5 components.");
                            args.cancel = true;
                            return;
                        }
                    }
                    else {
                        if (compositionComponents.length === 1) {
                            toastr.info("You can only add 1 component.");
                            args.cancel = true;
                            return;
                        }
                        else args.data.Percent = 100;
                    }

                    args.data.Id = getMaxIdForArray(compositionComponents, "Id");
                }
                else if (args.requestType === "save") {
                    var fiberID = 0;
                    var subProgramID = 0;
                    var certificationsID = 0;
                    if (typeof args.rowData.Fiber != 'undefined') {
                        fiberID = masterData.FabricComponentsNew.find(y => y.text == args.rowData.Fiber).id;
                    }
                    if (typeof args.rowData.YarnSubProgramNew != 'undefined') {
                        subProgramID = masterData.YarnSubProgramNews.find(y => y.text == args.rowData.YarnSubProgramNew).id;
                    }
                    if (typeof args.rowData.Certification != 'undefined') {
                        certificationsID = masterData.Certifications.find(y => y.text == args.rowData.Certification).id;
                    }

                    var cnt = masterData.FabricComponentMappingSetupList.filter(y => y.FiberID == fiberID && y.SubProgramID == subProgramID && y.CertificationsID == certificationsID);
                    if (cnt == 0) {
                        if (fiberID == 0) {
                            toastr.warning("Fiber is required.");
                            args.cancel = true;
                            return;
                        }
                        if (subProgramID == 0) {
                            toastr.warning("Sub Program is required.");
                            args.cancel = true;
                            return;
                        }
                        if (certificationsID == 0) {
                            toastr.warning("certifications is required.");
                            args.cancel = true;
                            return;
                        }
                    }


                    //fiberTypeName, programTypeName
                    var fiberTypeName = "";
                    var programTypeName = "";
                    var manufacturingLine = "";

                    var obj = masterData.FabricComponentMappingSetupList.find(x => x.FiberID == fiberID);
                    if (typeof obj !== "undefined") {
                        fiberTypeName = obj.FiberTypeName;
                    }
                    obj = masterData.FabricComponentMappingSetupList.find(x => x.CertificationsID == certificationsID);
                    if (typeof obj !== "undefined") {
                        programTypeName = obj.ProgramTypeName;
                    }
                    debugger;
                    obj = masterData.FabricComponentsNew.find(x => x.id == fiberID);
                    if (typeof obj !== "undefined") {
                        manufacturingLine = obj.desc;
                    }


                    args.rowData.FiberTypeName = fiberTypeName;
                    args.data.FiberTypeName = fiberTypeName;
                    args.data.ManufacturingLine = manufacturingLine;

                    args.rowData.ProgramTypeName = programTypeName;
                    args.data.ProgramTypeName = programTypeName;
                    args.data.ManufacturingLine = manufacturingLine;

                    //fiberTypeName, programTypeName




                    //masterData.FabricComponentsNew

                    if (args.action === "edit") {
                        if (!args.data.Fiber) {
                            toastr.warning("Fabric component is required.");
                            args.cancel = true;
                            return;
                        }
                        else if (!args.data.Percent || args.data.Percent <= 0 || args.data.Percent > 100) {
                            toastr.warning("Composition percent must be greater than 0 and less than or equal 100.");
                            args.cancel = true;
                            return;
                        }
                    }
                }
            },
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false,
            toolbar: ['Add'],
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true }
        };

        if ($tblCreateCompositionEl) $tblCreateCompositionEl.destroy();
        $tblCreateCompositionEl = new initEJ2Grid(gridOptions);
    }

    function saveComposition() {

        var totalPercent = sumOfArrayItem(compositionComponents, "Percent");
        if (totalPercent != 100) return toastr.error("Sum of compostion percent must be 100");
        compositionComponents.reverse();

        var composition = "";
        var blendTypeNames = [];
        var programTypeNames = [];
        //compositionComponents = _.sortBy(compositionComponents, "Percent").reverse();
        compositionComponents = compositionComponents.sort((a, b) => b.Percent - a.Percent);

        var manufacturingLines = [];
        var yarnTypes = []; //Fibers

        compositionComponents.forEach(function (component) {
            composition += composition ? ` ${component.Percent}%` : `${component.Percent}%`;

            yarnTypes.push(component.Fiber);
            var indexF = masterData.FabricComponentsNew.findIndex(x => x.text == component.Fiber);
            if (indexF > -1) {
                var manufacturingLine = masterData.FabricComponentsNew[indexF].desc;

                var indexG = manufacturingLines.findIndex(x => x == manufacturingLine);
                if (indexG == -1) {
                    manufacturingLines.push(manufacturingLine);
                }
            }

            if (component.YarnSubProgramNew) {
                if (component.YarnSubProgramNew != 'N/A') {
                    composition += ` ${component.YarnSubProgramNew}`;
                }
            }
            //if (component.Certification) composition += ` ${component.Certification}`;
            if (component.Certification) {
                if (component.Certification != 'N/A') {
                    composition += ` ${component.Certification}`;
                }
            }
            composition += ` ${component.Fiber}`;

            console.log(compositionComponents);
            component.FiberTypeName = getDefaultValueWhenInvalidS(component.FiberTypeName);
            if (component.FiberTypeName.length > 0) {
                blendTypeNames.push(component.FiberTypeName);
            }
            component.ProgramTypeName = getDefaultValueWhenInvalidS(component.ProgramTypeName);
            if (component.ProgramTypeName.length > 0) {
                programTypeNames.push(component.ProgramTypeName);
            }

        });
        yarnTypes = yarnTypes.join(",");
        manufacturingLines = manufacturingLines.join(",");

        blendTypeNames = [...new Set(blendTypeNames)];
        var blendTypeName = blendTypeNames.join(" + ");

        programTypeNames = [...new Set(programTypeNames)];
        var programTypeName = "Conventional";
        var indexF = programTypeNames.findIndex(x => x == "Sustainable");
        if (indexF > -1) {
            programTypeName = "Sustainable";
        }

        //var data = {
        //    SegmentValue: composition
        //};
        var data = {
            SegmentValue: composition,
            BlendTypeName: blendTypeName,
            ProgramTypeName: programTypeName,
            ManufacturingLines: manufacturingLines,
            YarnTypes: yarnTypes
        }

        axios.post("/api/rnd-free-concept-mr/save-yarn-composition", data)
            .then(function () {
                $pageEl.find(`#modal-new-composition-${pageId}`).modal("hide");
                toastr.success("Composition added successfully.");
                //masterData.CompositionList.unshift({ id: response.data.Id, text: response.data.SegmentValue });
                // initChildTable(masterData.Childs);
            })
            .catch(error => {
                if (error.response) {
                    toastr.error(error.response.data);
                } else {
                    toastr.error('Error message:', error.response.data.Message);
                }
                args.cancel = true;
            });
        //.catch(showResponseError)
    }

    function addColor(e) {
        e.preventDefault();

        var finder = new commonFinder({
            title: "Select Color for free concept",
            pageId: pageId,
            height: 350,
            apiEndPoint: "/api/fabric-color-book-setups/allcolor",
            fields: "ColorSource,ColorCode,ColorName,RGBOrHex",
            headerTexts: "Source,Code,Name,Visual",
            customFormats: ",,,ej2GridColorFormatter",
            
            //widths: "50,80,150,100",
            isMultiselect: true,
            primaryKeyColumn: "PTNID",
            onMultiselect: function (selectedRecords) {
                masterData.ChildColors = $tblChildEl.getCurrentViewRecords();
                selectedRecords.forEach(function (value) {
                    var exists = masterData.ChildColors.find(function (el) { return el.ColorId == value.ColorID });
                    if (!exists) masterData.ChildColors.unshift({
                        CCColorID: getMaxIdForArray($tblChildEl.getCurrentViewRecords(), "CCColorID"),
                        ColorId: value.ColorID,
                        ConceptID: masterData.ConceptID,
                        ColorName: value.ColorName,
                        ColorCode: value.ColorCode,
                        RGBOrHex: value.RGBOrHex
                    });
                });
                initChildTable(masterData.ChildColors);
            }
        });
        finder.showModal();
    }
})();
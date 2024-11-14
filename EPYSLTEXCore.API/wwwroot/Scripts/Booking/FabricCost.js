
(function () {
    var menuId, pageName, menuParam;
    var status = "";
    var $toolbarEl, toolbarId, $pageEl, pageId, $divTblEl, $divDetailsEl, $formEl,
        $tblMasterEl, tblMasterId, $tblFabricEl, tblFabricId, $tblCollarEl, tblCollarId, $tblCuffEl, tblCuffId,
        $tblYarnCostDetailsEl, tblYarnCostDetailsId, $tblAdditionalProcessCostDetailsEL, tblAdditionalProcessCostDetailsId;
    var masterData;
    var sampleData;


    var _saveProps = {
        SaveType: 'S'
    };
    var menuType = 0;
    var _paramType = {
        ApprovedPage: 1,
    };

    var _BookingID = 0;
    var _YBookingID = 0;
    var _YBookingNo = 0;
    var _YBookingNo = 0;
    var _BookingNo = "";
    var _EditType = "";
    var _WithoutOB = 0;
    var _CompositionMake = "";
    var _AdditioinalProcessCostModalTitle = "";
    var _IsYD = false;

    var GridColumnInfoYarn;
    var YarnDetailsListFromDB;

    var eRowDataFabricCollarCuffForYarnCostingGrid;


    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        if (!menuParam)
            menuParam = localStorage.getItem("menuParam");

        pageId = pageName + "-" + menuId;
        $pageEl = $(`#${pageId}`);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblFabricId = "#tblFabric" + pageId;
        tblCollarId = "#tblCollar" + pageId;
        tblCuffId = "#tblCuff" + pageId;
        tblYarnCostDetailsId = "#tblYarnCostDetailsId" + pageId;
        tblAdditionalProcessCostDetailsId = "#tblAdditionalProcessCostDetailsId" + pageId;

        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);

        menuType = localStorage.getItem("ApprovedPage");
        menuType = parseInt(menuType);
        pageMenuShowHide();

        initMasterTable();

        $toolbarEl.find("#btnPendingFabricCostList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;

            initMasterTable();

        });
        $toolbarEl.find("#btnPendingApprovalList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING2;
            initMasterTable();
        });
        $toolbarEl.find("#btnApprovedFabricCostList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            initMasterTable();

        });
        $toolbarEl.find("#btnRejectedFabricCostList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REJECT;
            initMasterTable();

        });


        $toolbarEl.find("#btnRefreshList").on("click", function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
        });





        $formEl.find("#btnCancel").click(function () {
            backToList();
        });
        $formEl.find("#btnSaveFabricCost").click(function (e) {
            resetSavedProps();
            _saveProps.SaveType = 'S';
            save();
        });
        $formEl.find("#btnSaveAndSendForApproval").click(function (e) {
            resetSavedProps();
            _saveProps.SaveType = 'SA';
            save();
        });
        $formEl.find("#btnApproved").click(function (e) {
            resetSavedProps();
            _saveProps.SaveType = 'A';
            save();
        });


        $formEl.find("#btnYarnCost").on("click", function (e) {
            e.preventDefault();

            yarnDetailsGridInfo();

        });
        $formEl.find("#btnAddProcessCost").on("click", function (e) {
            e.preventDefault();
            var AddProcessCostList = eRowDataFabricCollarCuffForYarnCostingGrid.rowData.AdditionalProcessCostDetails;
            AddProcessCostDetailsGridInfo(AddProcessCostList);

        });
        $formEl.find("#btnAdditionalProcessCostAdd").on("click", function (e) {
            e.preventDefault();

            AdditionalProcessCostAddPopUP();

        });
        $formEl.find("#btnModalYarnSave").on("click", function (e) {
            e.preventDefault();

            ModalYarnSave();

        });
        $formEl.find("#btnModalYarnClose").on("click", function (e) {
            e.preventDefault();

            ModalYarnClose();

        });

        $formEl.find("#btnModalSaveFabricCost").on("click", function (e) {
            e.preventDefault();

            ModalSaveFabricCost();

        });

        $formEl.find("#btnModalCloseFabricCost").on("click", function (e) {
            e.preventDefault();

            ModalCloseFabricCost();

        });
        $formEl.find("#btnModalAdditionalProcessSave").on("click", function (e) {
            e.preventDefault();

            ModalAdditionalProcessSave();

        });
        $formEl.find("#btnModalAdditionalProcessClose").on("click", function (e) {
            e.preventDefault();
            $formEl.find("#divAdditionalProcessCostDetailsInfo").hide();
            $('#divModalAdditionalProcessCost').modal('hide');


        });


        $formEl.find(".EnterFabricCostCal").on('keypress', function (event) {
           
            EnterEventCalForFabricCost(event);

        });

      
    });


    function resetSavedProps() {
        _saveProps = {
            SaveType: 'S'
        };
    }
    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
        if ($tblFabricEl) $tblFabricEl.destroy();
        if ($tblCollarEl) $tblCollarEl.destroy();
        if ($tblCuffEl) $tblCuffEl.destroy();
        $formEl.find("#divFebricInfo,#divCollarInfo,#divCuffInfo,#divYarnCostDetailsInfo,#divModalFabricCostYarnDetails,#divModalFabricCost,#divAdditionalProcessCostDetailsInfo,#divModalAdditionalProcessCost").hide();

    }
    function resetForm() {
        $formEl.trigger("reset");
    }
    function initMasterTable() {
        var commands = [],
            isVisible = true,
            width = 100;
        //if (status == statusConstants.PENDING) isVisible = false;
        if (status == statusConstants.PENDING) {
            width = 200;
            commands = [
                { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } },
            ]
        }
        else {
            commands = [
                { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
            ]
        }
        var columns = [
            {
                headerText: 'Actions', width: width, textAlign: 'Center', commands: commands, visible: true
            },

            { field: 'YBookingID', headerText: 'YBookingID', visible: false, isPrimaryKey: true },
            { field: 'BookingID', headerText: 'BookingID', visible: false },
            { field: 'SubGroupID', headerText: 'SubGroupID', visible: false },
            { field: 'StyleMasterID', headerText: 'StyleMasterID', visible: false },
            { field: 'FCStatus', headerText: 'Cost Status', visible: isVisible },
            { field: 'ExportOrderNo', headerText: 'Export Order No', visible: isVisible },
            { field: 'BookingNo', headerText: 'Booking No', visible: isVisible },
            { field: 'StyleNo', headerText: 'Style No', visible: isVisible },
            { field: 'BuyerName', headerText: 'Buye', visible: isVisible },
            { field: 'BuyerTeam', headerText: 'Buyer Team', visible: isVisible },
            { field: 'BusinessUnitShortName', headerText: 'Business Unit', visible: isVisible },
            { field: 'RevisionNo', headerText: 'Revision No', visible: isVisible },
            { field: 'IsAgreed', headerText: 'Agreed', visible: isVisible, displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            { field: 'WithoutOB', headerText: 'Is Sample?', visible: isVisible, displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            { field: 'AcknowledgeDate', headerText: 'Available Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100, visible: isVisible },
            { field: 'TNADate', headerText: 'TNA Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100, visible: isVisible },
            { field: 'FabricDeliveryStartDate', headerText: 'Delivery Start', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100, visible: isVisible },

        ];


        if ($tblMasterEl) $tblMasterEl.destroy();

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            allowGrouping: true,
            apiEndPoint: `/api/fabric-cost-fpyms/list?status=${status}`,
            columns: columns,
            allowSorting: true,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {

        if (args.commandColumn.type == 'View') {

            _BookingID = args.rowData.BookingID;
            _BookingNo = args.rowData.BookingNo;
            _YBookingNo = args.rowData.YBookingNo;
            _YBookingID = args.rowData.YBookingID;
            _WithoutOB = args.rowData.WithoutOB;
            _EditType = args.rowData.FCStatus;

            $formEl.find("#ExportOrderNo").val(args.rowData.ExportOrderNo);
            $formEl.find("#StyleNo").val(args.rowData.StyleNo);
            $formEl.find("#RevisionNo").val(args.rowData.RevisionNo);
            $formEl.find("#BookingNo").val(args.rowData.BookingNo);
            $formEl.find("#BuyerTeam").val(args.rowData.BuyerTeam);
            $formEl.find("#SeasonName").val(args.rowData.SeasonName);
            $formEl.find("#ItemName").val(args.rowData.SubGroupName);


            //ShowHide();

            getDetails();
        }

    }
    function getDetails() {

        axios.get(`/api/fabric-cost-fpyms/new/${_BookingID}/${_WithoutOB}/${_YBookingNo}/${_EditType}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;



                $('#FabricBookingDate').val(formatDateToDefault(masterData.BookingDate));


                if (masterData.BookingDetailsInfoList.length > 0) {
                    $formEl.find("#divFebricInfo").show();
                    initFabricChild(masterData.BookingDetailsInfoList, masterData.oGridColumnInfoFabricList);
                }
                if (masterData.CollarBookingDetailsInfoList.length > 0) {
                    $formEl.find("#divCollarInfo").show();
                    initCollarChild(masterData.CollarBookingDetailsInfoList, masterData.oGridColumnInfoCollarList);
                }
                if (masterData.CuffBookingDetailsInfoList.length > 0) {

                    $formEl.find("#divCuffInfo").show();
                    initCuffChild(masterData.CuffBookingDetailsInfoList, masterData.oGridColumnInfoCuffList);
                }

                GridColumnInfoYarn = masterData.oGridColumnInfoYarnCountsList;
                //YarnDetailsListFromDB = masterData.YarnDetailsListFromDB;
                ShowHide();

            })
            .catch(showResponseError);
    }

    function getColumnsForDisplayBySubGroup(data) {
        var columns = [];
        for (var i = 0; i < data.length; i++) {
            var fieldItem = data[i];

            if ((i - 1) >= 0) {
                if (data[i - 1].field == 'BookingUOM') {

                    columns.push({
                        field: '', headerText: 'FabricCost', valueAccessor: ej2GridEmptyFormatter, allowEditing: true, visible: true, commands: [
                            { buttonOption: { type: 'findFabricCost', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "FCost" } }
                        ]
                    });

                }
            }
            //
            if (fieldItem.editType == "numericedit") {
                columns.push({
                    field: fieldItem.field,
                    headerText: fieldItem.headerText,
                    allowEditing: fieldItem.allowEditing,
                    visible: fieldItem.visible,
                    //width: fieldItem.width,
                    editType: fieldItem.editType,
                    params: { decimals: 0, format: "N", min: 1, validateDecimalOnType: true }
                    //textAlign: fieldItem.textAlign
                });
            }
            else {
                columns.push({
                    field: fieldItem.field,
                    headerText: fieldItem.headerText,
                    valueAccessor: ej2GridEmptyFormatter,
                    allowEditing: fieldItem.allowEditing,
                    visible: fieldItem.visible
                });
            }
        }
        return columns;
    }
    function getYarnCostDetailsColumnsForDisplay(data) {
        var columns = [];
        for (var i = 0; i < data.length; i++) {
            var fieldItem = data[i];
            if (fieldItem.editType == "numericedit") {
               
                columns.push({
                    field: fieldItem.field,
                    headerText: fieldItem.headerText,
                    allowEditing: fieldItem.allowEditing,
                    visible: fieldItem.visible,
                    width: fieldItem.width,
                    editType: fieldItem.editType,
                    //params: { decimals: 4, format: "N4", min: 1, validateDecimalOnType: true },
                    //edit: { params: { showSpinButton: false, decimals: 4, format: "N2" } },
                    edit: { params: { showSpinButton: false, decimals: 4, format: "N4" } },
                    textAlign: fieldItem.textAlign
                });
            }
            else {
                columns.push({
                    field: fieldItem.field,
                    headerText: fieldItem.headerText,
                    valueAccessor: ej2GridEmptyFormatter,
                    allowEditing: fieldItem.allowEditing,
                    visible: fieldItem.visible,
                    width: fieldItem.width,
                    displayAsCheckBox: fieldItem.displayAsCheckBox,
                    editType: fieldItem.editType,
                    textAlign: fieldItem.textAlign
                });
            }

        }
        return columns;
    }

    async function initFabricChild(records, oGridColumnInfoList) {
        if ($tblFabricEl) $tblFabricEl.destroy();
        var FabricColumns = [], additionalColumns = [];

        FabricColumns = [
            {
                headerText: 'Commands', visible: false, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } }
                ]
            }
        ];
        FabricColumns.push.apply(FabricColumns, getColumnsForDisplayBySubGroup(oGridColumnInfoList));

        additionalColumns = [
            //{ field: 'BookingChildID', isPrimaryKey: true, visible: false },

        ];
        FabricColumns.push.apply(FabricColumns, additionalColumns);


        ej.base.enableRipple(true);
        $tblFabricEl = new ej.grids.Grid({
            editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
            autofitColumns: false,
            allowResizing: true,
            allowScrolling: true,
            enableContextMenu: true,
            enableSingleClickEdit: true,
            dataSource: records,
            columns: FabricColumns,
            commandClick: childFabricCommandClick
        });
        $tblFabricEl.appendTo(tblFabricId);
        $tblFabricEl.refresh();

    }
    async function initCollarChild(records, oGridColumnInfoList) {
        if ($tblCollarEl) $tblCollarEl.destroy();
        var CollarColumns = [], additionalColumns = [];

        CollarColumns = [
            {
                headerText: 'Commands', visible: false, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } }
                ]
            }
        ];
        CollarColumns.push.apply(CollarColumns, getColumnsForDisplayBySubGroup(oGridColumnInfoList));

        additionalColumns = [
            //{ field: 'BookingChildID', isPrimaryKey: true, visible: false },
        ];
        CollarColumns.push.apply(CollarColumns, additionalColumns);


        ej.base.enableRipple(true);
        $tblCollarEl = new ej.grids.Grid({
            editSettings: { allowEditing: true, allowAdding: false, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            autofitColumns: true,
            allowResizing: true,
            enableContextMenu: true,
            enableSingleClickEdit: true,
            dataSource: records,
            columns: CollarColumns
        });
        $tblCollarEl.appendTo(tblCollarId);
        $tblCollarEl.refresh();

    }
    async function initCuffChild(records, oGridColumnInfoList) {
        if ($tblCuffEl) $tblCuffEl.destroy();
        var CuffColumns = [], additionalColumns = [];

        CuffColumns = [
            {
                headerText: 'Commands', visible: false, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } }
                ]
            }
        ];
        CuffColumns.push.apply(CuffColumns, getColumnsForDisplayBySubGroup(oGridColumnInfoList));

        additionalColumns = [
            //{ field: 'BookingChildID', isPrimaryKey: true, visible: false }
        ];
        CuffColumns.push.apply(CuffColumns, additionalColumns);


        ej.base.enableRipple(true);
        $tblCuffEl = new ej.grids.Grid({
            editSettings: { allowEditing: true, allowAdding: false, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            autofitColumns: true,
            allowResizing: true,
            enableContextMenu: true,
            enableSingleClickEdit: true,
            dataSource: records,
            columns: CuffColumns
        });
        $tblCuffEl.appendTo(tblCuffId);
        $tblCuffEl.refresh();

    }

    async function initYarnCostDetails(records, oGridColumnInfoList) {
        if ($tblYarnCostDetailsEl) $tblYarnCostDetailsEl.destroy();
        var YarnCostDetailsColumns = [], additionalColumns = [];

        YarnCostDetailsColumns = [
            {
                headerText: 'Commands', visible: false, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } }
                ]
            }
        ];
        YarnCostDetailsColumns.push.apply(YarnCostDetailsColumns, getYarnCostDetailsColumnsForDisplay(oGridColumnInfoList));

        additionalColumns = [
            //{ field: 'BookingChildID', isPrimaryKey: true, visible: false },

        ];
        YarnCostDetailsColumns.push.apply(YarnCostDetailsColumns, additionalColumns);


        ej.base.enableRipple(true);
        $tblYarnCostDetailsEl = new ej.grids.Grid({
            editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
            autofitColumns: false,
            allowResizing: true,
            allowScrolling: true,
            enableContextMenu: true,
            enableSingleClickEdit: true,
            dataSource: records,
            columns: YarnCostDetailsColumns,
            //commandClick: yarnCostDetailCommandClick
            actionBegin: function (args) {

                if (args.requestType === "save") {
                    var nYarnLandedCost = 0;
                    var nYarnTotalCost = 0;
                    var nRequiredQtyFM = 0;

                    nYarnLandedCost = (getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.SourcingRate) * 1.3) / 100;
                    nYarnTotalCost = nYarnLandedCost + getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.SourcingRate);
                    nRequiredQtyFM = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.BookingQty) + ((getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.BookingQty) * getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.AllowanceFM)) / 100);

                    args.data.SourcingLandedCost = getDefaultValueWhenInvalidN_FloatWithFourDigit(nYarnLandedCost);
                    args.data.TotalSourcingRate = getDefaultValueWhenInvalidN_FloatWithFourDigit(nYarnTotalCost);
                    args.data.RequiredQtyFM = getDefaultValueWhenInvalidN_FloatWithFourDigit(nRequiredQtyFM);
                    args.rowData = args.data;

                    setTimeout(function () {

                        yarnFootarPartCalc();

                    }, 1000);





                }

            }

        });
        $tblYarnCostDetailsEl.appendTo(tblYarnCostDetailsId);
        $tblYarnCostDetailsEl.refresh();

    }

    async function initAdditionalProcessCostDetails(records) {
        if ($tblAdditionalProcessCostDetailsEL) $tblAdditionalProcessCostDetailsEL.destroy();
        var AdditionalProcessCostColumns = [];
        AdditionalProcessCostColumns = [
            {
                headerText: 'Commands', visible: true, width: 50, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                ]
            },
            { field: 'BDChildID', headerText: 'BDChildID', isPrimaryKey: true, visible: false, allowEditing: false },
            { field: 'YBookingID', headerText: 'YBookingID', visible: false, allowEditing: false },
            { field: 'YBChildID', headerText: 'YBChildID', visible: false, allowEditing: false },
            { field: 'ConsumptionID', headerText: 'ConsumptionID', visible: false, allowEditing: false },
            { field: 'ItemMasterID', headerText: 'ItemMasterID', visible: false, allowEditing: false },
            { field: 'BDID', headerText: 'BDID', visible: false, allowEditing: false },
            { field: 'CostID', headerText: 'CostID', visible: false, allowEditing: false },
            { field: 'CostName', headerText: 'Cost Type', visible: true, allowEditing: false },
            { field: 'FixedValue', headerText: 'Cost', visible: true, allowEditing: true, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } } }

        ];


        ej.base.enableRipple(true);
        $tblAdditionalProcessCostDetailsEL = new ej.grids.Grid({
            editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
            autofitColumns: false,
            allowResizing: true,
            allowScrolling: true,
            enableContextMenu: true,
            enableSingleClickEdit: true,
            dataSource: records,
            columns: AdditionalProcessCostColumns
            //commandClick: childFabricCommandClick
        });
        $tblAdditionalProcessCostDetailsEL.appendTo(tblAdditionalProcessCostDetailsId);
        $tblAdditionalProcessCostDetailsEL.refresh();

    }

    async function childFabricCommandClick(e) {

        if (e.commandColumn.buttonOption.type == 'findFabricCost') {
            
            eRowDataFabricCollarCuffForYarnCostingGrid = e;
            _IsYD = e.rowData.YD;
  
            _AdditioinalProcessCostModalTitle = GetSegmentValue('Construction', e.rowData);

            _CompositionMake = GetSegmentValue('Fabric Composition', e.rowData) + ' ' + _AdditioinalProcessCostModalTitle;
            $formEl.find("#fabricCostModalTitle").text(_CompositionMake);

            if (_IsYD) {
                $formEl.find("#divYDCost").show();
                $formEl.find("#divDyeingCost").hide();
            }
            else {
                $formEl.find("#divYDCost").hide();
                $formEl.find("#divDyeingCost").show();
            }
            var fabricCostPopUpValues = fabricCostPopupCalculation();
            setAndDisplayFabricCostFieldValues(fabricCostPopUpValues);
            $('#divModalFabricCost').modal('show');

        }
    }

    function yarnDetailsGridInfo() {
        YarnDetailsListFromDB = eRowDataFabricCollarCuffForYarnCostingGrid.rowData.YarnCostDetailsList;

        initYarnCostDetails(YarnDetailsListFromDB, GridColumnInfoYarn);
   
        yarnCostPopupCalculation(YarnDetailsListFromDB[0]);


        $formEl.find("#yarnCostModalTitle").text(_CompositionMake);
        $formEl.find("#divYarnCostDetailsInfo").show();
        $('#divModalFabricCostYarnDetails').modal('show');
        //yarnFootarPartCalc();
    }
    function AddProcessCostDetailsGridInfo(AddProcessDataList) {
        //initYarnCostDetails(YarnDetailsListFromDB, GridColumnInfoYarn);

        $formEl.find("#additioinalProcessCostModalTitle").text(_CompositionMake);

        $formEl.find("#divAdditionalProcessCostDetailsInfo").show();


        initAdditionalProcessCostDetails(AddProcessDataList);

        $('#divModalAdditionalProcessCost').modal('show');
    }
    function AdditionalProcessCostAddPopUP() {

        var entiryTypeName = "Fabric Additional Process";
        var finder = new commonFinder({
            title: "Select Process",
            pageId: pageId,
            height: 350,
            apiEndPoint: `/api/fabric-cost-fpyms/addProcess/${entiryTypeName}`,
            fields: "ValueID,ValueName",
            headerTexts: "ValueID,Process",
            widths: "0,100",
            hiddenFields: "ValueID",
            isMultiselect: true,
            autofitColumns: true,
            primaryKeyColumn: "ValueID",
            onMultiselect: function (selectedRecords) {
                var tempAdditionalProcessCostDetailList = [];
                selectedRecords.forEach(function (value) {
                    tempAdditionalProcessCostDetailList = $tblAdditionalProcessCostDetailsEL.getCurrentViewRecords();

                    var indexF = -1;
                    if (tempAdditionalProcessCostDetailList != null) {
                        indexF = tempAdditionalProcessCostDetailList.findIndex(x => x.CostID == value.ValueID);
                    }
                    else {
                        tempAdditionalProcessCostDetailList = [];
                    }

                    if (indexF == -1) {
                        //BDChildID, YBookingID, ConsumptionID, ItemMasterID, BDID, CostID, CostName, FixedValue
                       
                        var YBookingID =  eRowDataFabricCollarCuffForYarnCostingGrid.rowData.YBookingID;
                        var ConsumptionID = eRowDataFabricCollarCuffForYarnCostingGrid.rowData.ConsumptionID;
                        var ItemMasterID = eRowDataFabricCollarCuffForYarnCostingGrid.rowData.ItemMasterID;
                        var YBChildID = eRowDataFabricCollarCuffForYarnCostingGrid.rowData.YBChildID;
                        
                        var newData = { BDChildID: 0, YBookingID: YBookingID, YBChildID: YBChildID , ConsumptionID: ConsumptionID, ItemMasterID: ItemMasterID, BDID: 0, CostID: value.ValueID, CostName: value.ValueName, FixedValue: 0 };

                        tempAdditionalProcessCostDetailList.push(DeepClone(newData));
                    }

                });
                initAdditionalProcessCostDetails(tempAdditionalProcessCostDetailList);
            }
        });
        finder.showModal();
    }

    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }

    function GetSegmentValue(sheaderText, dataObj) {
        var columnModelList = $tblFabricEl.columnModel;
        var sField = "";
        for (var i = 0; i < columnModelList.length; i++) {
            if (columnModelList[i].headerText == sheaderText) {
                sField = columnModelList[i].field;

                return dataObj[sField];
            }
        }
    }

    function ShowHide() {
        if (status == statusConstants.PENDING) {
            $formEl.find("#btnSaveFabricCost,#btnSaveAndSendForApproval").show();
        }
        else {
            $formEl.find("#btnSaveFabricCost,#btnSaveAndSendForApproval").hide();
        }
        if (menuType == _paramType.ApprovedPage && status == statusConstants.PENDING2) {

            $formEl.find("#btnApproved").show();

        }
        else {
            $formEl.find("#btnApproved").hide();
        }

    }

    function save() {
        var sub = ""; var msg = "";
        if (_saveProps.SaveType == "S") {
            sub = "Save Fabric Cost";
            msg = "Do you want to save this cost information";
        }
        else if (_saveProps.SaveType == "SA") {
            sub = "Save and send  fabric cost";
            msg = "Do you want to save and send for approval this fabric cost Information";
        }
        else if (_saveProps.SaveType == "A") {
            sub = "Approved fabric cost";
            msg = "Do you want to approved this fabric cost Information";
        }

      
        var model = formDataToJson($formEl.serializeArray());
        model.WithoutOB = _WithoutOB;
        model.SaveType = _saveProps.SaveType;
        model.BookingID = _BookingID;
        model.YBookingID = _YBookingID;
        model.BookingNo = _BookingNo;
        model.YBookingNo = _YBookingNo;
        model.FCStatus = _EditType;

        if ($tblFabricEl != undefined) {
            model.BookingDetailsInfoList = $tblFabricEl.getCurrentViewRecords();
        }
        if ($tblCollarEl != undefined) {
            model.CollarBookingDetailsInfoList = $tblCollarEl.getCurrentViewRecords();
        }
        if ($tblCuffEl != undefined) {
            model.CuffBookingDetailsInfoList = $tblCuffEl.getCurrentViewRecords();
        }

        showBootboxConfirm(sub, msg, function (yes) {
            if (yes) {
                axios.post("/api/fabric-cost-fpyms/saveFCost", model)
                    .then(function () {
                        toastr.success("Saved successfully.");
                        $toolbarEl.find("#btnPendingFabricCostList").click();
                        backToList();
                    })
                    .catch(function (error) {
                        toastr.error(error.response.data.Message);
                    });
            }
        });




    }


    //Fabric Costing Popup operation
    function setAndDisplayFabricCostFieldValues(fabricCostObj) {

        $formEl.find("#YarnCostBeforePercent").val(fabricCostObj.YarnCost.YarnCostBeforePercent);
        $formEl.find("#YarnCostPercent").val(fabricCostObj.YarnCost.YarnCostPercent);
        $formEl.find("#YarnCost").val(fabricCostObj.YarnCost.YarnCost);

        $formEl.find("#TotalProcessCostBeforePercent").val(fabricCostObj.ProcessCost.TotalProcessCostBeforePercent);
        $formEl.find("#TotalProcessCostPercent").val(fabricCostObj.ProcessCost.TotalProcessCostPercent);
        $formEl.find("#TotalProcessCost").val(fabricCostObj.ProcessCost.TotalProcessCost);

        $formEl.find("#KnittingCost").val(fabricCostObj.ProcessCost.KnittingCost.KnittingCost);
        $formEl.find("#KnittingCostPercent").val(fabricCostObj.ProcessCost.KnittingCost.KnittingCostPercent);
        $formEl.find("#KnittingCostAfterPercent").val(fabricCostObj.ProcessCost.KnittingCost.KnittingCostAfterPercent);

        $formEl.find("#DyeingCost").val(fabricCostObj.ProcessCost.DyeingCost.DyeingCost);
        $formEl.find("#DyeingCostPercent").val(fabricCostObj.ProcessCost.DyeingCost.DyeingCostPercent);
        $formEl.find("#DyeingCostAfterPercent").val(fabricCostObj.ProcessCost.DyeingCost.DyeingCostAfterPercent);
      
        $formEl.find("#YDCost").val(fabricCostObj.ProcessCost.DyeingCostYD.YDCost);
        $formEl.find("#YDCostPercent").val(fabricCostObj.ProcessCost.DyeingCostYD.YDCostPercent);
        $formEl.find("#YDCostAfterPercent").val(fabricCostObj.ProcessCost.DyeingCostYD.YDCostAfterPercent);

        $formEl.find("#FinishingCost").val(fabricCostObj.ProcessCost.FinishingAndCompactingCost.FinishingCost);
        $formEl.find("#FinishingCostPercent").val(fabricCostObj.ProcessCost.FinishingAndCompactingCost.FinishingCostPercent);
        $formEl.find("#FinishingCostAfterPercent").val(fabricCostObj.ProcessCost.FinishingAndCompactingCost.FinishingCostAfterPercent);

        $formEl.find("#AddProcessCost").val(fabricCostObj.ProcessCost.AdditionalProcessCost.AddProcessCost);
        $formEl.find("#AddProcessCostPercent").val(fabricCostObj.ProcessCost.AdditionalProcessCost.AddProcessCostPercent);
        $formEl.find("#AddProcessCostAfterPercent").val(fabricCostObj.ProcessCost.AdditionalProcessCost.AddProcessCostAfterPercent);

        $formEl.find("#TVariableCost").val(fabricCostObj.TotalVariableCost.TVariableCost);
        $formEl.find("#FixedCost").val(fabricCostObj.FixedCost.FixedCost);
        $formEl.find("#TotalCost").val(fabricCostObj.TotalCost.TotalCost);

        $formEl.find("#TotalMarkupPercent").val(fabricCostObj.Markup.TotalMarkupPercent);
        $formEl.find("#TotalMarkup").val(fabricCostObj.Markup.TotalMarkup);

        $formEl.find("#QFabricPrice").val(fabricCostObj.TotalFabricCost.QFabricPrice);
    }
    function getFabricCostCurrentValue() {
        var fromValueObj = {
            YarnCost: {
                YarnCostBeforePercent: getDefaultValueWhenInvalidN_Float($formEl.find("#YarnCostBeforePercent").val()),
                YarnCostPercent: getDefaultValueWhenInvalidN_Float($formEl.find("#YarnCostPercent").val()),
                YarnCost: getDefaultValueWhenInvalidN_Float($formEl.find("#YarnCost").val())
            },
            ProcessCost: {
                TotalProcessCostBeforePercent: getDefaultValueWhenInvalidN_Float($formEl.find("#TotalProcessCostBeforePercent").val()),
                TotalProcessCostPercent: getDefaultValueWhenInvalidN_Float($formEl.find("#TotalProcessCostPercent").val()),

                KnittingCost: {
                    KnittingCost: getDefaultValueWhenInvalidN_Float($formEl.find("#KnittingCost").val()),
                    KnittingCostPercent: getDefaultValueWhenInvalidN_Float($formEl.find("#KnittingCostPercent").val()),
                    KnittingCostAfterPercent: getDefaultValueWhenInvalidN_Float($formEl.find("#KnittingCostAfterPercent").val()),
                },

                DyeingCost: {
                    DyeingCost: getDefaultValueWhenInvalidN_Float($formEl.find("#DyeingCost ").val()),
                    DyeingCostPercent: getDefaultValueWhenInvalidN_Float($formEl.find("#DyeingCostPercent").val()),
                    DyeingCostAfterPercent: getDefaultValueWhenInvalidN_Float($formEl.find("#DyeingCostAfterPercent").val())
                },
                DyeingCostYD: {
                    YDCost: getDefaultValueWhenInvalidN_Float($formEl.find("#YDCost").val()),
                    YDCostPercent: getDefaultValueWhenInvalidN_Float($formEl.find("#YDCostPercent").val()),
                    YDCostAfterPercent: getDefaultValueWhenInvalidN_Float($formEl.find("#YDCostAfterPercent").val())
                },
                FinishingAndCompactingCost: {
                    FinishingCost: getDefaultValueWhenInvalidN_Float($formEl.find("#FinishingCost").val()),
                    FinishingCostPercent: getDefaultValueWhenInvalidN_Float($formEl.find("#FinishingCostPercent").val()),
                    FinishingCostAfterPercent: getDefaultValueWhenInvalidN_Float($formEl.find("#FinishingCostAfterPercent").val())
                },
                AdditionalProcessCost: {
                    AddProcessCost: getDefaultValueWhenInvalidN_Float($formEl.find("#AddProcessCost").val()),
                    AddProcessCostPercent: getDefaultValueWhenInvalidN_Float($formEl.find("#AddProcessCostPercent").val()),
                    AddProcessCostAfterPercent: getDefaultValueWhenInvalidN_Float($formEl.find("#AddProcessCostAfterPercent").val())
                }
            },
            TotalVariableCost: {
                TVariableCost: getDefaultValueWhenInvalidN_Float($formEl.find("#TVariableCost").val())
            },
            FixedCost: {
                FixedCost: getDefaultValueWhenInvalidN_Float($formEl.find("#FixedCost").val())
            },
            TotalCost: {
                TotalCost: getDefaultValueWhenInvalidN_Float($formEl.find("#TotalCost").val())
            },
            Markup: {
                TotalMarkupPercent: getDefaultValueWhenInvalidN_Float($formEl.find("#TotalMarkupPercent").val()),
                TotalMarkup: getDefaultValueWhenInvalidN_Float($formEl.find("#TotalMarkup").val())
            },
            TotalFabricCost: {
                QFabricPrice: getDefaultValueWhenInvalidN_Float($formEl.find("#QFabricPrice").val())
            }
        };
        return fromValueObj;
    }
    function fabricCostPopupCalculation() {

        var args = eRowDataFabricCollarCuffForYarnCostingGrid;

        bookingPrice = args.rowData.PORate;

        var fCostPopupCalObj = args.rowData.FabricCostDetailsList;
   
        //Ratin
        var calculatedObj = {
            YarnCost: {
                YarnCostBeforePercent: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.YarnCostBeforePercent),
                YarnCostPercent: getDefaultValueWhenInvalidN_Float(fCostPopupCalObj.YarnCostPercent),
                YarnCost: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.YarnCost),
            },
            ProcessCost: {
                TotalProcessCostBeforePercent: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.TotalProcessCostBeforePercent),
                TotalProcessCostPercent: getDefaultValueWhenInvalidN_Float(fCostPopupCalObj.TotalProcessCostPercent),
                TotalProcessCost: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.TotalProcessCost),

                KnittingCost: {
                    KnittingCost: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.KnittingCost),
                    KnittingCostPercent: getDefaultValueWhenInvalidN_Float(fCostPopupCalObj.KnittingCostPercent),
                    KnittingCostAfterPercent: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.KnittingCostAfterPercent),
                },

                DyeingCost: {
                    DyeingCost: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.DyeingCost),
                    DyeingCostPercent: getDefaultValueWhenInvalidN_Float(fCostPopupCalObj.DyeingCostPercent),
                    DyeingCostAfterPercent: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.DyeingCostAfterPercent),
                },
                DyeingCostYD: {
                    YDCost: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.YDCost),
                    YDCostPercent: getDefaultValueWhenInvalidN_Float(fCostPopupCalObj.YDCostPercent),
                    YDCostAfterPercent: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.YDCostAfterPercent),
                },
                FinishingAndCompactingCost: {
                    FinishingCost: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.FinishingCost),
                    FinishingCostPercent: getDefaultValueWhenInvalidN_Float(fCostPopupCalObj.FinishingCostPercent),
                    FinishingCostAfterPercent: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.FinishingCostAfterPercent),
                },
                AdditionalProcessCost: {
                    AddProcessCost: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.AddProcessCost),
                    AddProcessCostPercent: getDefaultValueWhenInvalidN_Float(fCostPopupCalObj.AddProcessCostPercent),
                    AddProcessCostAfterPercent: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.AddProcessCostAfterPercent),
                }
            },
            TotalVariableCost: {
                TVariableCost: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.TVariableCost)
            },
            FixedCost: {
                FixedCost: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.FixedCost)
            },
            TotalCost: {
                TotalCost: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.TotalCost)
            },
            Markup: {
                TotalMarkupPercent: getDefaultValueWhenInvalidN_Float(fCostPopupCalObj.TotalMarkupPercent),
                TotalMarkup: getDefaultValueWhenInvalidN_FloatWithFourDigit(fCostPopupCalObj.TotalMarkup),
            },
            TotalFabricCost: {
                QFabricPrice: getDefaultValueWhenInvalidN_FloatWithFourDigit(bookingPrice)
            }
        };
        
        calculatedObj.YarnCost.YarnCost = getDefaultValueWhenInvalidN_FloatWithFourDigit(calculatedObj.YarnCost.YarnCostBeforePercent + (calculatedObj.YarnCost.YarnCostBeforePercent * calculatedObj.YarnCost.YarnCostPercent) / 100);


        calculatedObj.ProcessCost.KnittingCost.KnittingCostAfterPercent = getDefaultValueWhenInvalidN_FloatWithFourDigit(calculatedObj.ProcessCost.KnittingCost.KnittingCost + (calculatedObj.ProcessCost.KnittingCost.KnittingCost * calculatedObj.ProcessCost.KnittingCost.KnittingCostPercent) / 100);

        calculatedObj.ProcessCost.DyeingCost.DyeingCostAfterPercent = getDefaultValueWhenInvalidN_FloatWithFourDigit(calculatedObj.ProcessCost.DyeingCost.DyeingCost + (calculatedObj.ProcessCost.DyeingCost.DyeingCost * calculatedObj.ProcessCost.DyeingCost.DyeingCostPercent) / 100);

        calculatedObj.ProcessCost.FinishingAndCompactingCost.FinishingCostAfterPercent = getDefaultValueWhenInvalidN_FloatWithFourDigit(calculatedObj.ProcessCost.FinishingAndCompactingCost.FinishingCost+(calculatedObj.ProcessCost.FinishingAndCompactingCost.FinishingCost * calculatedObj.ProcessCost.FinishingAndCompactingCost.FinishingCostPercent) / 100);

        calculatedObj.ProcessCost.AdditionalProcessCost.AddProcessCostAfterPercent = getDefaultValueWhenInvalidN_FloatWithFourDigit(calculatedObj.ProcessCost.AdditionalProcessCost.AddProcessCost + (calculatedObj.ProcessCost.AdditionalProcessCost.AddProcessCost * calculatedObj.ProcessCost.AdditionalProcessCost.AddProcessCostPercent) / 100);


        calculatedObj.ProcessCost.TotalProcessCostBeforePercent = getDefaultValueWhenInvalidN_FloatWithFourDigit(
            calculatedObj.ProcessCost.KnittingCost.KnittingCost +
            calculatedObj.ProcessCost.DyeingCost.DyeingCost +
            calculatedObj.ProcessCost.FinishingAndCompactingCost.FinishingCost +
            calculatedObj.ProcessCost.AdditionalProcessCost.AddProcessCost);


        calculatedObj.ProcessCost.TotalProcessCost = getDefaultValueWhenInvalidN_FloatWithFourDigit(
            calculatedObj.ProcessCost.KnittingCost.KnittingCostAfterPercent + calculatedObj.ProcessCost.DyeingCost.DyeingCostAfterPercent +
            calculatedObj.ProcessCost.FinishingAndCompactingCost.FinishingCostAfterPercent + calculatedObj.ProcessCost.AdditionalProcessCost.AddProcessCostAfterPercent);


        //calculatedObj.ProcessCost.TotalProcessCostPercent = getDefaultValueWhenInvalidN_Float((calculatedObj.ProcessCost.TotalProcessCost / calculatedObj.ProcessCost.TotalProcessCostBeforePercent) * 100);
        //calculatedObj.ProcessCost.TotalProcessCostPercent = getDefaultValueWhenInvalidN_Float((calculatedObj.ProcessCost.TotalProcessCost / calculatedObj.ProcessCost.TotalProcessCostBeforePercent) -1);

        calculatedObj.ProcessCost.TotalProcessCostPercent = getDefaultValueWhenInvalidN_Float(((calculatedObj.ProcessCost.TotalProcessCost - calculatedObj.ProcessCost.TotalProcessCostBeforePercent) / calculatedObj.ProcessCost.TotalProcessCostBeforePercent) * 100);

        calculatedObj.TotalVariableCost.TVariableCost = getDefaultValueWhenInvalidN_FloatWithFourDigit(calculatedObj.YarnCost.YarnCost + calculatedObj.ProcessCost.TotalProcessCost);

        calculatedObj.TotalCost.TotalCost = getDefaultValueWhenInvalidN_FloatWithFourDigit(calculatedObj.TotalVariableCost.TVariableCost + calculatedObj.FixedCost.FixedCost);

        calculatedObj.Markup.TotalMarkup = getDefaultValueWhenInvalidN_FloatWithFourDigit(calculatedObj.TotalFabricCost.QFabricPrice - calculatedObj.TotalCost.TotalCost);

        calculatedObj.Markup.TotalMarkupPercent = getDefaultValueWhenInvalidN_Float((calculatedObj.Markup.TotalMarkup / calculatedObj.TotalFabricCost.QFabricPrice) * 100)
        //calculatedObj.TotalFabricCost.QFabricPrice = getDefaultValueWhenInvalidN_Float(getDefaultValueWhenInvalidN_Float(calculatedObj.TotalCost.TotalCost) + getDefaultValueWhenInvalidN_Float(calculatedObj.Markup.TotalMarkup));

        var TotalYDValue = 0; var TotalYDValueB4Allowance = 0; var TotalFabric = 0; var YDCost = 0; var YDCostB4Allowance = 0; var totalBookingValue = 0;

        for (var i = 0; i < args.rowData.YarnCostDetailsList.length; i++) {
            if (_IsYD) {
                TotalYDValue += getDefaultValueWhenInvalidN_FloatWithFourDigit(args.rowData.YarnCostDetailsList[i].RequiredQtyFM) *
                    getDefaultValueWhenInvalidN_FloatWithFourDigit(args.rowData.YarnCostDetailsList[i].DyeingCostFM);

                TotalYDValueB4Allowance += getDefaultValueWhenInvalidN_FloatWithFourDigit(args.rowData.YarnCostDetailsList[i].BookingQty) *
                    getDefaultValueWhenInvalidN_FloatWithFourDigit(args.rowData.YarnCostDetailsList[i].DyeingCostFM);

                totalBookingValue = parseFloat(totalBookingValue) +
                    (parseFloat(args.rowData.YarnCostDetailsList[i].BookingQty) *
                    parseFloat(args.rowData.YarnCostDetailsList[i].TotalSourcingRate));
             
            }
        }
        

        if (_IsYD) {
            TotalFabric = getDefaultValueWhenInvalidN_FloatWithFourDigit($formEl.find("#FBKInKg").val());

            YDCost = getDefaultValueWhenInvalidN_FloatWithFourDigit(TotalYDValue) / getDefaultValueWhenInvalidN_FloatWithFourDigit(TotalFabric);
            YDCostB4Allowance = getDefaultValueWhenInvalidN_FloatWithFourDigit(getDefaultValueWhenInvalidN_FloatWithFourDigit(TotalYDValueB4Allowance) / getDefaultValueWhenInvalidN_FloatWithFourDigit(TotalFabric));

            var BookingCost = parseFloat(totalBookingValue) > parseFloat(0) && parseFloat(TotalFabric) > parseFloat(0) ? parseFloat(totalBookingValue) / parseFloat(TotalFabric) : 0;
            var AllowancePercent = parseFloat(calculatedObj.YarnCost.YarnCost) > parseFloat(0) && parseFloat(BookingCost) > parseFloat(0) ? ((parseFloat(calculatedObj.YarnCost.YarnCost) * parseFloat(100)) / parseFloat(BookingCost)) - parseFloat(100) : parseFloat(0);

            var YDCostPercent = $formEl.find("#YDCostPercent").val();
            YDCostPercent = YDCostPercent == '' ? 0 : YDCostPercent;
            YDCostPercent = (YDCostPercent == 0 || parseFloat(YDCostPercent) != parseFloat(AllowancePercent)) ? AllowancePercent : YDCostPercent;

            calculatedObj.ProcessCost.DyeingCostYD.YDCost = getDefaultValueWhenInvalidN_FloatWithFourDigit(YDCostB4Allowance);
            calculatedObj.ProcessCost.DyeingCostYD.YDCostPercent = getDefaultValueWhenInvalidN_Float(YDCostPercent);
            calculatedObj.ProcessCost.DyeingCostYD.YDCostAfterPercent = getDefaultValueWhenInvalidN_FloatWithFourDigit(YDCost);
        }

      
        
        return calculatedObj;
    }

    function pageMenuShowHide() {
        //$toolbarEl.find(".btnToolbar").hide();
    
        if (menuType == _paramType.ApprovedPage) {
            status = statusConstants.PENDING2;
            $toolbarEl.find("#btnPendingFabricCostList").hide();
            $toolbarEl.find("#btnPendingApprovalList").click();
        }
        else {
            status = statusConstants.PENDING;
            $toolbarEl.find("#btnPendingFabricCostList,#btnPendingApprovalList,#btnApprovedFabricCostList,#btnRejectedFabricCostList,#btnRefreshList").show();
        }
    }


    // alamin
    function ModalSaveFabricCost() {

        var fabricCostDetailsList = eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList;

        var fCostDetail = SetFabricCostDetail(fabricCostDetailsList);
        eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList = fCostDetail;

        
        eRowDataFabricCollarCuffForYarnCostingGrid.rowData.Price = parseFloat($formEl.find("#QFabricPrice").val());

        //var index = $tblFabricEl.getRowIndexByPrimaryKey(eRowDataFabricCollarCuffForYarnCostingGrid.rowData.YBChildID);
        //eRowDataFabricCollarCuffForYarnCostingGrid.rowIndex
        //var index  = eRowDataFabricCollarCuffForYarnCostingGrid.rowInfo.rowIndex

        var RowIndex = $tblFabricEl.currentViewData.findIndex(x => x.YBChildID == eRowDataFabricCollarCuffForYarnCostingGrid.rowData.YBChildID && x.ConsumptionID == eRowDataFabricCollarCuffForYarnCostingGrid.rowData.ConsumptionID && x.ItemMasterID == eRowDataFabricCollarCuffForYarnCostingGrid.rowData.ItemMasterID);
        
        $tblFabricEl.updateRow(RowIndex, eRowDataFabricCollarCuffForYarnCostingGrid.rowData);

        $formEl.find('#divModalFabricCost').modal('hide');

    }
    function SetFabricCostDetail(fabricCostDetailsList) {
        fabricCostDetailsList.YarnCostBeforePercent = $formEl.find("#YarnCostBeforePercent").val();
        fabricCostDetailsList.YarnCostPercent = $formEl.find("#YarnCostPercent").val();
        fabricCostDetailsList.YarnCost = $formEl.find("#YarnCost").val();

        fabricCostDetailsList.TotalProcessCostBeforePercent = $formEl.find("#TotalProcessCostBeforePercent").val();
        fabricCostDetailsList.TotalProcessCostPercent = $formEl.find("#TotalProcessCostPercent").val();
        fabricCostDetailsList.TotalProcessCost = $formEl.find("#TotalProcessCost").val();

        fabricCostDetailsList.KnittingCost = $formEl.find("#KnittingCost").val();
        fabricCostDetailsList.KnittingCostPercent = $formEl.find("#KnittingCostPercent").val();
        fabricCostDetailsList.KnittingCostAfterPercent = $formEl.find("#KnittingCostAfterPercent").val();

        fabricCostDetailsList.DyeingCost = $formEl.find("#DyeingCost").val();
        fabricCostDetailsList.DyeingCostPercent = $formEl.find("#DyeingCostPercent").val();
        fabricCostDetailsList.DyeingCostAfterPercent = $formEl.find("#DyeingCostAfterPercent").val();

        fabricCostDetailsList.YDCost = $formEl.find("#YDCost").val();
        fabricCostDetailsList.YDCostPercent = $formEl.find("#YDCostPercent").val();
        fabricCostDetailsList.YDCostAfterPercent = $formEl.find("#YDCostAfterPercent").val();


        fabricCostDetailsList.FinishingCost = $formEl.find("#FinishingCost").val();
        fabricCostDetailsList.FinishingCostPercent = $formEl.find("#FinishingCostPercent").val();
        fabricCostDetailsList.FinishingCostAfterPercent = $formEl.find("#FinishingCostAfterPercent").val();

        fabricCostDetailsList.AddProcessCost = $formEl.find("#AddProcessCost").val();
        fabricCostDetailsList.AddProcessCostPercent = $formEl.find("#AddProcessCostPercent").val();
        fabricCostDetailsList.AddProcessCostAfterPercent = $formEl.find("#AddProcessCostAfterPercent").val();

        fabricCostDetailsList.TVariableCost = $formEl.find("#TVariableCost").val();
        fabricCostDetailsList.FixedCost = $formEl.find("#FixedCost").val();
        fabricCostDetailsList.TotalCost = $formEl.find("#TotalCost").val();

        fabricCostDetailsList.TotalMarkupPercent = $formEl.find("#TotalMarkupPercent").val();
        fabricCostDetailsList.TotalMarkup = $formEl.find("#TotalMarkup").val();

        fabricCostDetailsList.QFabricPrice = $formEl.find("#QFabricPrice").val();

        return fabricCostDetailsList;
    }
    function ModalCloseFabricCost() {
        $('#divModalFabricCost').modal('hide');

    }

    function ModalYarnSave() {
        // save operation need to work 
        eRowDataFabricCollarCuffForYarnCostingGrid.rowData.YarnCostDetailsList = $tblYarnCostDetailsEl.getCurrentViewRecords();

        if (eRowDataFabricCollarCuffForYarnCostingGrid.rowData.YarnCostDetailsList.length > 0) {
            eRowDataFabricCollarCuffForYarnCostingGrid.rowData.YarnCostDetailsList[0].YarnInKg = $formEl.find("#YarnInKg").val();
            eRowDataFabricCollarCuffForYarnCostingGrid.rowData.YarnCostDetailsList[0].YarnValue = $formEl.find("#YarnValue").val();
            eRowDataFabricCollarCuffForYarnCostingGrid.rowData.YarnCostDetailsList[0].FBKInKg = $formEl.find("#FBKInKg").val();
            eRowDataFabricCollarCuffForYarnCostingGrid.rowData.YarnCostDetailsList[0].YarnRate = $formEl.find("#YarnRate").val();
            eRowDataFabricCollarCuffForYarnCostingGrid.rowData.YarnCostDetailsList[0].YarnAllowance = $formEl.find("#YarnAllowance").val();

        }

        eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList.YarnCostBeforePercent = getDefaultValueWhenInvalidN_FloatWithFourDigit($formEl.find("#yarnCostPrice").val());
        eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList.YarnCostPercent = getDefaultValueWhenInvalidN_Float($formEl.find("#YarnAllowance").val());

        eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList.KnittingCostPercent = getDefaultValueWhenInvalidN_Float($formEl.find("#YarnAllowance").val());
        eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList.DyeingCostPercent  = getDefaultValueWhenInvalidN_Float($formEl.find("#YarnAllowance").val());
        eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList.FinishingCostPercent  = getDefaultValueWhenInvalidN_Float($formEl.find("#YarnAllowance").val());
        eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList.AddProcessCostPercent  = getDefaultValueWhenInvalidN_Float($formEl.find("#YarnAllowance").val());

        $tblFabricEl.updateRow(eRowDataFabricCollarCuffForYarnCostingGrid.rowIndex, eRowDataFabricCollarCuffForYarnCostingGrid.data);

        var fabricCostPopUpValues = fabricCostPopupCalculation();
        setAndDisplayFabricCostFieldValues(fabricCostPopUpValues);

        $formEl.find("#divYarnCostDetailsInfo").hide();
        $('#divModalFabricCostYarnDetails').modal('hide');

    }
    function ModalYarnClose() {

        $formEl.find("#divYarnCostDetailsInfo").hide();
        $('#divModalFabricCostYarnDetails').modal('hide');

    }

    function yarnCostPopupCalculation(rowDataList) {
        var yarnCalculatedObj = {
            YarnInKg: getDefaultValueWhenInvalidN_Float(rowDataList.YarnInKg),
            YarnValue: getDefaultValueWhenInvalidN_Float(rowDataList.YarnValue),
            FBKInKg: getDefaultValueWhenInvalidN_Float(rowDataList.FBKInKg),
            YarnRate: getDefaultValueWhenInvalidN_Float(rowDataList.YarnRate),
            YarnAllowance: getDefaultValueWhenInvalidN_Float(rowDataList.YarnAllowance)
        };

        setAndDisplayYarnCostFieldValues(yarnCalculatedObj);

    }

    function setAndDisplayYarnCostFieldValues(yarnCostObj) {
        $formEl.find("#YarnInKg").val(yarnCostObj.YarnInKg);
        $formEl.find("#YarnValue").val(yarnCostObj.YarnValue);
        $formEl.find("#FBKInKg").val(yarnCostObj.FBKInKg);
        $formEl.find("#YarnRate").val(yarnCostObj.YarnRate);
        $formEl.find("#YarnAllowance").val(yarnCostObj.YarnAllowance);
    }


    function yarnFootarPartCalc() {
        //var yarnCostDetailsList = eRowDataFabricCollarCuffForYarnCostingGrid.rowData.YarnCostDetailsList;

        var yarnFootarObj = {
            YarnInKg: 0,
            YarnValue: 0,
            YarnRate: 0,
            YarnAllowance: 0,
            yarnCostPrice: 0,
            RequiredQty: 0,
            TempBQTYIntoTotalCost: 0

        };
        var yarnCostDetailsList = $tblYarnCostDetailsEl.getCurrentViewRecords();
        for (var i = 0; i < yarnCostDetailsList.length; i++) {

            yarnFootarObj.YarnInKg += yarnCostDetailsList[i].RequiredQtyFM;
            yarnFootarObj.YarnValue += (yarnCostDetailsList[i].RequiredQtyFM * yarnCostDetailsList[i].TotalSourcingRate);
            yarnFootarObj.TempBQTYIntoTotalCost += (yarnCostDetailsList[i].BookingQty * yarnCostDetailsList[i].TotalSourcingRate);

            yarnFootarObj.yarnCostPrice += (yarnCostDetailsList[i].RequiredQty * yarnCostDetailsList[i].TotalSourcingRate);
            yarnFootarObj.RequiredQty += (yarnCostDetailsList[i].RequiredQty);
        }
        yarnFootarObj.YarnRate = getDefaultValueWhenInvalidN_FloatWithFourDigit(getDefaultValueWhenInvalidN_FloatWithFourDigit(yarnFootarObj.YarnValue) / getDefaultValueWhenInvalidN_FloatWithFourDigit($formEl.find("#FBKInKg").val()));
        yarnFootarObj.YarnAllowance = getDefaultValueWhenInvalidN_Float((
            (
                getDefaultValueWhenInvalidN_FloatWithFourDigit(yarnFootarObj.YarnValue) / getDefaultValueWhenInvalidN_FloatWithFourDigit(yarnFootarObj.TempBQTYIntoTotalCost)
            ) * 100
        ) - 100);


        yarnFootarObj.yarnCostPrice = (getDefaultValueWhenInvalidN_FloatWithFourDigit(yarnFootarObj.yarnCostPrice) / getDefaultValueWhenInvalidN_FloatWithFourDigit(yarnFootarObj.RequiredQty));

        $formEl.find("#YarnInKg").val(getDefaultValueWhenInvalidN_FloatWithFourDigit(yarnFootarObj.YarnInKg));
        $formEl.find("#YarnValue").val(getDefaultValueWhenInvalidN_FloatWithFourDigit(yarnFootarObj.YarnValue));
        $formEl.find("#YarnRate").val(getDefaultValueWhenInvalidN_FloatWithFourDigit(yarnFootarObj.YarnRate));
        $formEl.find("#YarnAllowance").val(getDefaultValueWhenInvalidN_FloatWithFourDigit(yarnFootarObj.YarnAllowance));

        $formEl.find("#yarnCostPrice").val(getDefaultValueWhenInvalidN_FloatWithFourDigit(yarnFootarObj.yarnCostPrice));

    }

    function EnterEventCalForFabricCost(event) {

        if (event.key === 'Enter' || event.keyCode === 13) {

            eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList.KnittingCost = getDefaultValueWhenInvalidN_FloatWithFourDigit($formEl.find("#KnittingCost").val());
            eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList.KnittingCostPercent = getDefaultValueWhenInvalidN_Float($formEl.find("#KnittingCostPercent").val());

            eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList.DyeingCost = getDefaultValueWhenInvalidN_FloatWithFourDigit($formEl.find("#DyeingCost").val());
            eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList.DyeingCostPercent = getDefaultValueWhenInvalidN_Float($formEl.find("#DyeingCostPercent").val());

            eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList.YDCost = getDefaultValueWhenInvalidN_FloatWithFourDigit($formEl.find("#YDCost").val());
            eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList.YDCostPercent = getDefaultValueWhenInvalidN_Float($formEl.find("#YDCostPercent").val());

            eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList.FinishingCost = getDefaultValueWhenInvalidN_FloatWithFourDigit($formEl.find("#FinishingCost").val());
            eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList.FinishingCostPercent = getDefaultValueWhenInvalidN_Float($formEl.find("#FinishingCostPercent").val());


            eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList.AddProcessCostPercent = getDefaultValueWhenInvalidN_Float($formEl.find("#AddProcessCostPercent").val());
            eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList.AddProcessCostAfterPercent = getDefaultValueWhenInvalidN_FloatWithFourDigit($formEl.find("#AddProcessCostAfterPercent").val());

            $tblFabricEl.updateRow(eRowDataFabricCollarCuffForYarnCostingGrid.rowIndex, eRowDataFabricCollarCuffForYarnCostingGrid.data);



            var fabricCostPopUpValues = fabricCostPopupCalculation();
            setAndDisplayFabricCostFieldValues(fabricCostPopUpValues);
        }
    }

    function ModalAdditionalProcessSave() {
        var additionalProcessList = $tblAdditionalProcessCostDetailsEL.getCurrentViewRecords();
        eRowDataFabricCollarCuffForYarnCostingGrid.rowData.AdditionalProcessCostDetails = additionalProcessList;

        var tempCost = 0;
        for (var i = 0; i < additionalProcessList.length; i++) {

            tempCost += getDefaultValueWhenInvalidN_FloatWithFourDigit(additionalProcessList[i].FixedValue);
        }

        $formEl.find("#AddProcessCost").val(tempCost);

        $tblFabricEl.updateRow(eRowDataFabricCollarCuffForYarnCostingGrid.rowIndex, eRowDataFabricCollarCuffForYarnCostingGrid.data);



        eRowDataFabricCollarCuffForYarnCostingGrid.rowData.FabricCostDetailsList.AddProcessCost = tempCost;
        var fabricCostPopUpValues = fabricCostPopupCalculation();
        setAndDisplayFabricCostFieldValues(fabricCostPopUpValues);


        $formEl.find("#divAdditionalProcessCostDetailsInfo").hide();
        $('#divModalAdditionalProcessCost').modal('hide');

    }


})();
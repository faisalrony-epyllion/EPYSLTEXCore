(function () {
    var menuId, pageName, menuParam;
    var status = "";
    var $toolbarEl, toolbarId, $pageEl, pageId, $divTblEl, $divDetailsEl, $formEl,
        $tblMasterEl, tblMasterId, $tblFabricEl, tblFabricId, $tblCollarEl, tblCollarId, $tblCuffEl, tblCuffId;
    var masterData;


    var _saveProps = {
        IsAcceptPrice: false,
        SendSuggestPrice: false,
        AcceptForPriceRePropose: false,
        PriceStatus: 'Propose Price',
        SaveType: 'A'
    };

    var _BookingID = 0;
    var _BookingNo = "";
    var _WithoutOB = 0;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

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

        status = statusConstants.PENDING;
        initMasterTable();

        $toolbarEl.find("#btnProposeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            //CreateToolBarHideShow();
            _saveProps.PriceStatus = 'Propose Price';

            initMasterTable();

        });
        $toolbarEl.find("#btnAcceptList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            _saveProps.PriceStatus = 'Accept Price';

            initMasterTable();

        });
        $toolbarEl.find("#btnSuggestedList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED2;
            _saveProps.PriceStatus = 'Suggest Price';
            initMasterTable();

        });


        $toolbarEl.find("#btnRefreshList").on("click", function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
        });





        $formEl.find("#btnCancel").click(function () {
            backToList();
        });
        $formEl.find("#btnAcceptPrice").click(function (e) {
            resetSavedProps();
            _saveProps.IsAcceptPrice = true;
            _saveProps.SaveType = 'A';
            save();
        });
        $formEl.find("#btnSendSuggestPrice").click(function (e) {
            resetSavedProps();
            _saveProps.SendSuggestPrice = true;
            _saveProps.SaveType = 'S';
            save();
        });
        $formEl.find("#btnAcceptForPriceRePropose").click(function (e) {
            resetSavedProps();
            _saveProps.SendSuggestPrice = true;
            _saveProps.SaveType = 'RP';
            save();
        });
    });


    function resetSavedProps() {
        _saveProps = {
            IsAcceptPrice: false,
            SendSuggestPrice: false,
            AcceptForPriceRePropose: false,
            PriceStatus: 'Propose Price',
            SaveType: 'A'
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
        $formEl.find("#divFebricInfo,#divCollarInfo,#divCuffInfo").hide();

    }
    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#BookingID").val(-1111);
        $formEl.find("#ReProposeReasonID").val(-1111);
    }
    function initMasterTable() {
        var commands = [],
            isVisible = true,
            width = 100;
        //if (status == statusConstants.PENDING) isVisible = false;
        if (status == statusConstants.ACTIVE) {
            width = 200;
            commands = [
                { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
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

            { field: 'BookingID', headerText: 'BookingID', visible: false, isPrimaryKey: true },
            { field: 'SubGroupID', headerText: 'SubGroupID', visible: false },
            { field: 'SubGroupName', headerText: 'SubGroupName', visible: false },
            { field: 'WithoutOB', headerText: 'WithoutOB', visible: false },
            { field: 'PriceStatus', headerText: 'Status', visible: isVisible },
            { field: 'BookingNo', headerText: 'Booking No', visible: isVisible },
            { field: 'BookingDate', headerText: 'Booking Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100, visible: isVisible },
            { field: 'PriceProposeNo', headerText: 'No of Propose', visible: isVisible },
            { field: 'PriceProposeDateTime', headerText: 'Propose Date', visible: isVisible },
            { field: 'PriceSuggestDateTime', headerText: 'Suggest Date', visible: isVisible },
            { field: 'PriceAgreeDateTime', headerText: 'Accept Date', visible: isVisible },
            { field: 'StyleNo', headerText: 'Style No', visible: isVisible },
            { field: 'ExportOrderNo', headerText: 'EWO No', visible: isVisible },
            { field: 'BuyerName', headerText: 'Buyer', visible: isVisible },
            { field: 'BuyerTeamName', headerText: 'Buyer Team', visible: isVisible },
            { field: 'SupplierName', headerText: 'Supplier', visible: isVisible },
            { field: 'BookingType', headerText: 'Booking Type', visible: isVisible }

        ];


        if ($tblMasterEl) $tblMasterEl.destroy();

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            allowGrouping: true,
            apiEndPoint: `/api/fabric-price-acceptance/list?status=${status}`,
            columns: columns,
            allowSorting: true,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {

        if (args.commandColumn.type == 'View') {
            _BookingID = args.rowData.BookingID;
            _BookingNo = args.rowData.BookingNo;
            _WithoutOB = args.rowData.WithoutOB;


            $formEl.find("#BookingNo").val(args.rowData.BookingNo);
            $formEl.find("#BookingID").val(args.rowData.BookingID);
            $formEl.find("#BookingDate").val(formatDateToDefault(args.rowData.BookingDate));
            $formEl.find("#ExportOrderNo").val(args.rowData.ExportOrderNo);
            $formEl.find("#PriceProposeNo").val(args.rowData.PriceProposeNo);
            $formEl.find("#Buyer").val(args.rowData.BuyerName);
            $formEl.find("#BuyerTeam").val(args.rowData.BuyerTeamName);
            $formEl.find("#SupplierName").val(args.rowData.SupplierName);
            $formEl.find("#Remarks").val(args.rowData.Remarks);
            $formEl.find("#ReferenceNo").val(args.rowData.ReferenceNo);


            ShowHide();

            getDetails();
        }

    }
    function getDetails() {

        axios.get(`/api/fabric-price-acceptance/new/${_BookingNo}/${_WithoutOB}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                //masterData.RequestDate = formatDateToDefault(masterData.RequestDate);
                //setFormData($formEl, masterData);
                
                if (masterData.BookingChildDetailsFabric.length > 0) {
                    $formEl.find("#divFebricInfo").show();
                    initFabricChild(masterData.BookingChildDetailsFabric);
                }
                if (masterData.BookingChildDetailsCollar.length > 0) {
                    $formEl.find("#divCollarInfo").show();
                    initCollarChild(masterData.BookingChildDetailsCollar);
                }
                if (masterData.BookingChildDetailsCuff.length > 0) {

                    $formEl.find("#divCuffInfo").show();
                    initCuffChild(masterData.BookingChildDetailsCuff);
                }



            })
            .catch(showResponseError);
    }

    async function initFabricChild(records) {
        if ($tblFabricEl) $tblFabricEl.destroy();
        var FabricColumns = [], additionalColumns = [];

        FabricColumns = [
            {
                headerText: 'Commands', visible: true, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } }
                ]
            }
        ];

        FabricColumns.push.apply(FabricColumns, await getItemColumnsForDisplayBySubGroupAsync('Fabric'));

        additionalColumns = [
            { field: 'BookingChildID', isPrimaryKey: true, visible: false },
            { field: 'BookingID', visible: false },
            { field: 'ItemMasterID', visible: false },
            { field: 'ConsumptionID', visible: false },
            { field: 'ItemGroupID', visible: false },
            { field: 'SubGroupID', visible: false },
            { field: 'AutoAgree', visible: false },

            { field: 'LengthYds', headerText: 'Length (Yds)', visible: false, allowEditing: false },
            { field: 'LengthInch', headerText: 'Length (Inch)', visible: false, allowEditing: false },
            { field: 'Remarks', headerText: 'Instruction', visible: true, allowEditing: false },
            { field: 'A1Desc', headerText: 'Yarn Type', visible: true, allowEditing: false },
            { field: 'YarnBrandName', headerText: 'Yarn Program', visible: true, allowEditing: false },

            // for Fabric only 
            { field: 'PartName', headerText: 'Uses In', visible: true, allowEditing: false },
            { field: 'LabDipNo', headerText: 'Lab Dip No', visible: true, allowEditing: false },

            { field: 'BookingQty', headerText: 'Booking Qty', visible: true, allowEditing: false },
            { field: 'BookingUOM', headerText: 'Booking UOM', visible: true, allowEditing: false },
            { field: 'Price', headerText: 'Price', visible: true, allowEditing: false },
            { field: 'SuggestedPrice', headerText: 'Suggest Price', visible: true, allowEditing: true },
            { field: 'FabricCost', headerText: 'Fabric Cost', visible: false, allowEditing: false }
        ];
        FabricColumns.push.apply(FabricColumns, additionalColumns);


        ej.base.enableRipple(true);
        $tblFabricEl = new ej.grids.Grid({
            editSettings: { allowEditing: true, allowAdding: true, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
            autofitColumns: false,
            allowResizing: true,
            allowScrolling: true,
            enableContextMenu: true,
            enableSingleClickEdit: true,
            dataSource: records,
            columns: FabricColumns
        });
        $tblFabricEl.appendTo(tblFabricId);
        $tblFabricEl.refresh();

    }
    async function initCollarChild(records) {
        if ($tblCollarEl) $tblCollarEl.destroy();
        var CollarColumns = [], additionalColumns = [];

        CollarColumns = [
            {
                headerText: 'Commands', visible: true, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } }
                ]
            }
        ];
        CollarColumns.push.apply(CollarColumns, await getItemColumnsForDisplayBySubGroupAsync('Collar'));

        additionalColumns = [
            { field: 'BookingChildID', isPrimaryKey: true, visible: false },
            { field: 'BookingID', visible: false },
            { field: 'ItemMasterID', visible: false },
            { field: 'ConsumptionID', visible: false },
            { field: 'ItemGroupID', visible: false },
            { field: 'SubGroupID', visible: false },
            { field: 'AutoAgree', visible: false },

            { field: 'LengthYds', headerText: 'Length (Yds)', visible: false, allowEditing: false },
            { field: 'LengthInch', headerText: 'Length (Inch)', visible: false, allowEditing: false },
            { field: 'Remarks', headerText: 'Instruction', visible: true, allowEditing: false },
            { field: 'A1Desc', headerText: 'Yarn Type', visible: true, allowEditing: false },
            { field: 'YarnBrandName', headerText: 'Yarn Program', visible: true, allowEditing: false },

            { field: 'BookingQty', headerText: 'Booking Qty', visible: true, allowEditing: false },
            { field: 'BookingUOM', headerText: 'Booking UOM', visible: true, allowEditing: false },
            { field: 'Price', headerText: 'Price', visible: true, allowEditing: false },
            { field: 'SuggestedPrice', headerText: 'Suggest Price', visible: true, allowEditing: true },
            { field: 'FabricCost', headerText: 'Fabric Cost', visible: false, allowEditing: false }
        ];
        CollarColumns.push.apply(CollarColumns, additionalColumns);


        ej.base.enableRipple(true);
        $tblCollarEl = new ej.grids.Grid({
            editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
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
    async function initCuffChild(records) {
        if ($tblCuffEl) $tblCuffEl.destroy();
        var CuffColumns = [], additionalColumns = [];

        CuffColumns = [
            {
                headerText: 'Commands', visible: true, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } }
                ]
            }
        ];
        CuffColumns.push.apply(CuffColumns, await getItemColumnsForDisplayBySubGroupAsync('Cuff'));

        additionalColumns = [
            { field: 'BookingChildID', isPrimaryKey: true, visible: false },
            { field: 'BookingID', visible: false },
            { field: 'ItemMasterID', visible: false },
            { field: 'ConsumptionID', visible: false },
            { field: 'ItemGroupID', visible: false },
            { field: 'SubGroupID', visible: false },
            { field: 'AutoAgree', visible: false },

            { field: 'LengthYds', headerText: 'Length (Yds)', visible: false, allowEditing: false },
            { field: 'LengthInch', headerText: 'Length (Inch)', visible: false, allowEditing: false },
            { field: 'Remarks', headerText: 'Instruction', visible: true, allowEditing: false },
            { field: 'A1Desc', headerText: 'Yarn Type', visible: true, allowEditing: false },
            { field: 'YarnBrandName', headerText: 'Yarn Program', visible: true, allowEditing: false },

            { field: 'BookingQty', headerText: 'Booking Qty', visible: true, allowEditing: false },
            { field: 'BookingUOM', headerText: 'Booking UOM', visible: true, allowEditing: false },
            { field: 'Price', headerText: 'Price', visible: true, allowEditing: false },
            { field: 'SuggestedPrice', headerText: 'Suggest Price', visible: true, allowEditing: true },
            { field: 'FabricCost', headerText: 'Fabric Cost', visible: false, allowEditing: false }
        ];
        CuffColumns.push.apply(CuffColumns, additionalColumns);


        ej.base.enableRipple(true);
        $tblCuffEl = new ej.grids.Grid({
            editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
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

    function save() {
        var sub = ""; var msg = "";
        if (_saveProps.SaveType == "A") {
            sub = "Accept Price";
            msg = "Do you want to Accept this Price Information";
        }
        else if (_saveProps.SaveType == "S") {
            sub = "Suggest Price";
            msg = "Do you want to Send this Suggest Price Information";
        }
        else if (_saveProps.SaveType == "RP") {
            sub = "Repropose Price";
            msg = "Do you want to Re-Propose Price Information";
        }

        var model = formDataToJson($formEl.serializeArray());
        model.IsAcceptPrice = _saveProps.IsAcceptPrice;
        model.SendSuggestPrice = _saveProps.SendSuggestPrice;
        model.WithoutOB = _WithoutOB;
        model.PriceStatus = _saveProps.PriceStatus;
        model.SaveType = _saveProps.SaveType;
        model.BookingID = _BookingID;
        model.BookingNo = _BookingNo;
        model.IsAllowRePropose = "N";
        //model.ReProposeAcceptReason = ReProposeReason;

        if ($tblFabricEl != undefined) {
            model.BookingChildDetailsFabric = $tblFabricEl.getCurrentViewRecords();
        }
        if ($tblCollarEl != undefined) {
            model.BookingChildDetailsCollar = $tblCollarEl.getCurrentViewRecords();
        }
        if ($tblCuffEl != undefined) {
            model.BookingChildDetailsCuff = $tblCuffEl.getCurrentViewRecords();
        }

        if (CheckGridDetails(model)) {
            showBootboxConfirm(sub, msg, function (yes) {
                if (yes) {
                    axios.post("/api/fabric-price-acceptance/save", model)
                        .then(function () {
                            toastr.success("Saved successfully.");
                            $toolbarEl.find("#btnProposeList").click();
                            backToList();
                        })
                        .catch(function (error) {
                            toastr.error(error.response.data.Message);
                        });
                }
            });


          
        }
    }

    function CheckGridDetails(model) {
        var isFabricCheck = false;
        var isCollarCheck = false;
        var isCuffCheck = false;

        if ($tblFabricEl != undefined) {
            for (var idx = 0; idx < model.BookingChildDetailsFabric.length; idx++) {
                if (_saveProps.SaveType == 'S' && (parseFloat(model.BookingChildDetailsFabric[idx].SuggestedPrice) <= 0 || model.BookingChildDetailsFabric[idx].SuggestedPrice == '')) {
                    isFabricCheck = true;
                    break;
                }
            }

        }
        if ($tblCollarEl != undefined) {
            for (var idx = 0; idx < model.BookingChildDetailsCollar.length; idx++) {
                if (_saveProps.SaveType == 'S' && (parseFloat(model.BookingChildDetailsCollar[idx].SuggestedPrice) <= 0 || model.BookingChildDetailsCollar[idx].SuggestedPrice == '')) {
                    isCollarCheck = true;
                    break;
                }
            }
        }
        if ($tblCuffEl != undefined) {
            for (var idx = 0; idx < model.BookingChildDetailsCuff.length; idx++) {
                if (_saveProps.SaveType == 'S' && (parseFloat(model.BookingChildDetailsCuff[idx].SuggestedPrice) <= 0 || model.BookingChildDetailsCuff[idx].SuggestedPrice == '')) {
                    isCuffCheck = true;
                    break;
                }
            }

        }

        if (isFabricCheck == true && isCollarCheck == true && isCuffCheck == true) {
            toastr.error("Suggest Price");
            return false;
        }

        return true;
    }

    function ShowHide() {
        if (_saveProps.PriceStatus == 'Propose Price') {
            $formEl.find("#btnAcceptForPriceRePropose,#pnlPriceReProposeReason").hide();
        }
        else if (_saveProps.PriceStatus == 'Accept Price') {
            $formEl.find("#btnAcceptForPriceRePropose,#pnlPriceReProposeReason").show();
            //$formEl.find("#btnAcceptPrice,#btnSendSuggestPrice").hide();

        }
        else if (_saveProps.PriceStatus == 'Suggest Price') {
            //$formEl.find("#btnAcceptPrice,#btnSendSuggestPrice,#btnAcceptForPriceRePropose,#pnlPriceReProposeReason").hide();
        }
       
    }
})();
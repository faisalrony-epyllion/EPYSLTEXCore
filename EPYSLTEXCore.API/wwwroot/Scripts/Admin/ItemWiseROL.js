(function () {
    var menuId, pageName;
    var $tblMasterEl, $formEl, tblMasterId, $toolbarEl;
    var itemMasterList, itemMasterLists = []
    var _maxROSID = -999;
    var _isEdit = false;
    var pageId;
    var $dialogtemplate;
    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        $dialogtemplate = $(".dialogtemplate" + pageId);

        $formEl.find("#btnAddItemMaster").on("click", addItemMaster);
        getInitData();
    });
    function initMasterTable() {
        var columns = [
            {
                field: 'ROSID',
                headerText: 'ROSID',
                textAlign: 'Right',
                width: 100,
                isPrimaryKey: true,
                visible: false
            },
            {
                field: 'ItemMasterID',
                headerText: 'ItemMasterID',
                width: 150,
                visible: false
            },
            {
                field: 'CompanyID',
                headerText: 'CompanyID',
                width: 150,
                visible: false
            },
            {
                field: 'SubGroupID',
                headerText: 'SubGroupID',
                width: 150,
                visible: false
            },
            {
                field: 'SubGroupName',
                headerText: 'Item Category',
                width: 150
            },
            {
                field: 'ItemName',
                headerText: 'Item Name',
                width: 150
            },
            {
                field: 'CompanyName',
                headerText: 'Company Name',
                width: 150
            },
            {
                field: 'MonthlyAvgConsumptionLP',
                headerText: 'Monthly Avg Consumption LP',
            },
            {
                field: 'MonthlyAvgConsumptionFP',
                headerText: 'Monthly Avg Consumption FP',
                width: 120,
            },
            {
                field: 'ROLLocalPurchase',
                headerText: 'ROL Local Purchase',
                width: 120,
            },
            {
                field: 'ROLForeignPurchase',
                headerText: 'ROL Foreign Purchase',
                width: 120,
            },
            {
                field: 'ReOrderQty',
                headerText: 'Re-Order Qty',
                width: 120,
            },
            {
                field: 'MaximumPRQtyLP',
                headerText: 'Maximum PR Qty LP',
                width: 120,
            },
            {
                field: 'MaximumPRQtyFP',
                headerText: 'Maximum PR Qty FP',
                width: 120,
            },
            {
                field: 'MOQ',
                headerText: 'MOQ',
                width: 120,
            },
            {
                field: 'ValidDate',
                headerText: 'Valid Date',
                width: 120,
                type: 'date',
                format: _ch_date_format_1,
                textAlign: 'Center'
            },
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: true,
            apiEndPoint: `/api/item-wise-rol/list`,
            columns: columns,
            allowExcelExport: false,
            allowPdfExport: false,
            toolbar: ['Add', 'Edit'],
            editSettings: {
                allowEditing: true,
                allowAdding: true,
                allowDeleting: false,
                mode: 'Dialog',
                template: '#dialogtemplateROL'
            },
            actionBegin: function (args) {
                if (args.requestType === 'save') {
                    args.data.ROSID = getDefaultValueWhenInvalidN(args.data.ROSID);
                    args.rowData = setValidPropsValue(args.data, args.rowData);
                    //args.data = setDropDownValues(masterData, args.data, args.rowData);
                    args.rowData = args.data;

                    if (args.data.ROSID == 0) {
                        args.data.ROSID = _maxROSID--;
                    }
                    var allData = $tblMasterEl.dataSource;

                    //var isExist = false;
                    //var list = allData.filter(item => item.ItemName === args.data.ItemName);

                    //if (list.length > 0 && _isEdit) {
                    //    list = list.filter(x => x.ROSID != args.data.ROSID);
                    //}

                    //if (list.length > 0) isExist = true;

                    //if (isExist) {
                    //    toastr.error("Duplicate Item found!!!");
                    //    args.cancel = true;
                    //    return;
                    //}

                    var dataObj = {
                        ROSID: args.data.ROSID,
                        ItemMasterID: getDefaultValueWhenInvalidN($formEl.find("#ItemMasterID").val()),
                        SubGroupID: getDefaultValueWhenInvalidN($formEl.find("#SubGroupID").val()),
                        CompanyID: getDefaultValueWhenInvalidN($formEl.find("#CompanyID").val()),
                        MonthlyAvgConsumptionLP: args.data.MonthlyAvgConsumptionLP,
                        MonthlyAvgConsumptionFP: args.data.MonthlyAvgConsumptionFP,
                        ROLLocalPurchase: args.data.ROLLocalPurchase,
                        ROLForeignPurchase: args.data.ROLForeignPurchase,
                        MaximumPRQtyLP: args.data.MaximumPRQtyLP,
                        MaximumPRQtyFP: args.data.MaximumPRQtyFP,
                        MOQ: args.data.MOQ,
                        ValidDate: formatDateToDefault(args.data.ValidDate)
                    };

                    args.rowData = DeepClone(args.data);
                    save(dataObj, args);
                    //if (!save(dataObj)) {
                    //    args.cancel = true;
                    //    return;
                    //};
                }
                if (args.requestType === 'delete') {
                }
            },
            actionComplete: function (args) {
                _isEdit = false;
                if (args.requestType === 'add' || args.requestType === 'beginEdit') {
                    setTimeout(function () {
                        var dialog = args.form.closest('.e-dialog'); // Get the dialog element
                        dialog.style.width = '60%'; // Set the width
                        dialog.style.height = '75%'; // Set the height
                        //dialog.style.top = 'auto'; // Optional: Set dialog position (vertically centered)
                        dialog.style.left = '40%'; // Optional: Center dialog horizontally
                        dialog.style.transform = 'translateX(-55%)'; // Center horizontally by adjusting position

                        var ejDialogInstance = dialog.ej2_instances[0]; // Access the EJ2 Dialog instance
                        ejDialogInstance.dragging = false;
                        // Set focus on a specific input field after dialog is opened
                        $('#MonthlyAvgConsumptionLP').focus();
                    }, 100);
                }
                if (args.requestType === 'add') {
                    var itemNameID = getDefaultValueWhenInvalidN($formEl.find("#ItemMasterID").val());
                    var companyID = getDefaultValueWhenInvalidN($formEl.find("#CompanyID").val());
                    //var tdaysDate = formatGridPopupDate(new Date());
                    $('.ValidDate').val(formatGridPopupDate(new Date()));
                    //$("#ValidDate").val(formatGridPopupDate(new Date()));
                    if (itemNameID == 0) {
                        toastr.error("Selecet Item!!!");
                        args.cancel = true;
                        var closeButton = document.querySelector('.e-dialog .e-dlg-closeicon-btn');

                        if (closeButton) {
                            closeButton.click();
                        }
                        return;
                    }
                    if (companyID == 0) {
                        toastr.error("Selecet Company!!!");
                        args.cancel = true;
                        var closeButton = document.querySelector('.e-dialog .e-dlg-closeicon-btn');

                        if (closeButton) {
                            closeButton.click();
                        }
                        return;
                    }
                    var itemName = $formEl.find("#ItemName").val();
                    $('.ItemMaster_Finder').val(itemName);
                    //$("#ItemMaster_Finder").val(itemName);
                    //$formEl.find('#ItemMaster_Finder').val(itemName);
                    args.dialog.header = 'Add Details';
                }
                if (args.requestType === 'beginEdit') {
                    _isEdit = true;
                    args.dialog.header = 'Edit Details';

                    //$formEl.find("#ROSID").val(args.rowData.ROSID);
                    //$formEl.find("#ItemMasterID").val(args.rowData.ItemMasterID);
                    //$formEl.find("#SubGroupID").val(args.rowData.SubGroupID);
                    //$formEl.find("#CompanyID").val(args.rowData.CompanyID);
                    //$formEl.find("#ItemName").val(args.rowData.ItemName);

                    debugger;

                    $('.ItemMaster_Finder').val(args.rowData.ItemName);
                    $('.MonthlyAvgConsumptionLP').val(args.rowData.MonthlyAvgConsumptionLP);
                    $('.MonthlyAvgConsumptionFP').val(args.rowData.MonthlyAvgConsumptionFP);
                    $('.ROLLocalPurchase').val(args.rowData.ROLLocalPurchase);
                    $('.ROLForeignPurchase').val(args.rowData.ROLForeignPurchase);
                    $('.MaximumPRQtyLP').val(args.rowData.MaximumPRQtyLP);
                    $('.MaximumPRQtyFP').val(args.rowData.MaximumPRQtyFP);
                    $('.MOQ').val(args.rowData.MOQ);
                    $('.ValidDate').val(formatGridPopupDate(args.rowData.ValidDate));

                    /*

                    $formEl.find("#ROSID").val(args.rowData.ROSID);
                    $formEl.find("#ItemMasterID").val(args.rowData.ItemMasterID);
                    $formEl.find("#SubGroupID").val(args.rowData.SubGroupID);
                    $formEl.find("#CompanyID").val(args.rowData.CompanyID);
                    $formEl.find("#ItemName").val(args.rowData.ItemName);

                    var itemName = $formEl.find("#ItemName").val();
                    //$("#ItemMaster_Finder").val(itemName);
                    $formEl.find('#ItemMaster_Finder').val(itemName);
                    //$("#MonthlyAvgConsumptionLP").val(args.rowData.MonthlyAvgConsumptionLP);

                    alert(args.rowData.MonthlyAvgConsumptionLP);

                    $formEl.find('#MonthlyAvgConsumptionLP').val(args.rowData.MonthlyAvgConsumptionLP);
                    //$("#MonthlyAvgConsumptionFP").val(args.rowData.MonthlyAvgConsumptionFP);
                    $formEl.find('#MonthlyAvgConsumptionFP').val(args.rowData.MonthlyAvgConsumptionFP);
                    //$("#ROLLocalPurchase").val(args.rowData.ROLLocalPurchase);
                    $formEl.find('#ROLLocalPurchase').val(args.rowData.ROLLocalPurchase);
                    //$("#ROLForeignPurchase").val(args.rowData.ROLForeignPurchase);
                    $formEl.find('#ROLForeignPurchase').val(args.rowData.ROLForeignPurchase);
                    //$("#MaximumPRQtyLP").val(args.rowData.MaximumPRQtyLP);
                    $formEl.find('#MaximumPRQtyLP').val(args.rowData.MaximumPRQtyLP);
                    //$("#MaximumPRQtyFP").val(args.rowData.MaximumPRQtyFP);
                    $formEl.find('#MaximumPRQtyFP').val(args.rowData.MaximumPRQtyFP);
                    //$("#MOQ").val(args.rowData.MOQ);
                    $formEl.find('#MOQ').val(args.rowData.MOQ);
                    //$("#ValidDate").val(formatGridPopupDate(args.rowData.ValidDate));
                    $formEl.find('#ValidDate').val(formatGridPopupDate(args.rowData.ValidDate));
                    */
                }
            }
            //commandClick: handleCommands
        });
    }
    function initMasterTable1() {
        var url = "/api/item-wise-rol/list";
        axios.get(url)
            .then(function (response) {
                if ($tblMasterEl) {
                    $tblMasterEl.destroy();
                }
                ej.grids.Grid.Inject(ej.grids.Edit, ej.grids.Toolbar);
                $tblMasterEl = new ej.grids.Grid({
                    dataSource: response.data.Items,
                    allowPaging: true,
                    pageSettings: { pageSize: 10 },
                    allowFiltering: true,
                    filterSettings: { type: 'FilterBar' },
                    toolbar: ['Add', 'Edit', 'Delete'],
                    editSettings: {
                        allowEditing: true,
                        allowAdding: true,
                        allowDeleting: false,
                        mode: 'Dialog',
                        template: '#dialogtemplateROL'
                    },
                    columns: [
                        {
                            field: 'ROSID',
                            headerText: 'ROSID',
                            textAlign: 'Right',
                            width: 100,
                            isPrimaryKey: true,
                            visible: false
                        },
                        {
                            field: 'ItemMasterID',
                            headerText: 'ItemMasterID',
                            width: 150,
                            visible: false
                        },
                        {
                            field: 'CompanyID',
                            headerText: 'CompanyID',
                            width: 150,
                            visible: false
                        },
                        {
                            field: 'SubGroupID',
                            headerText: 'SubGroupID',
                            width: 150,
                            visible: false
                        },
                        {
                            field: 'SubGroupName',
                            headerText: 'Item Category',
                            width: 150
                        },
                        {
                            field: 'ItemName',
                            headerText: 'Item Name',
                            width: 150
                        },
                        {
                            field: 'CompanyName',
                            headerText: 'Company Name',
                            width: 150
                        },
                        {
                            field: 'MonthlyAvgConsumptionLP',
                            headerText: 'Monthly Avg Consumption LP',
                        },
                        {
                            field: 'MonthlyAvgConsumptionFP',
                            headerText: 'Monthly Avg Consumption FP',
                            width: 120,
                        },
                        {
                            field: 'ROLLocalPurchase',
                            headerText: 'ROL Local Purchase',
                            width: 120,
                        },
                        {
                            field: 'ROLForeignPurchase',
                            headerText: 'ROL Foreign Purchase',
                            width: 120,
                        },
                        {
                            field: 'ReOrderQty',
                            headerText: 'Re-Order Qty',
                            width: 120,
                        },
                        {
                            field: 'MaximumPRQtyLP',
                            headerText: 'Maximum PR Qty LP',
                            width: 120,
                        },
                        {
                            field: 'MaximumPRQtyFP',
                            headerText: 'Maximum PR Qty FP',
                            width: 120,
                        },
                        {
                            field: 'MOQ',
                            headerText: 'MOQ',
                            width: 120,
                        },
                        {
                            field: 'ValidDate',
                            headerText: 'Valid Date',
                            width: 120,
                        },

                    ],
                    pageSettings: {
                        pageSize: 5,
                        pageCount: 3,
                        //pageSizes: [5, 10, 20] // Allows the user to select page size dynamically
                    },
                    height: 500,
                    actionBegin: function (args) {
                        if (args.requestType === 'save') {

                            //    // cast string to integer value.
                            //    args.data['NetWeight'] =
                            //        parseFloat(args.form.querySelector("#NetWeight").value);

                            args.data.ROSID = getDefaultValueWhenInvalidN(args.data.ROSID);
                            args.rowData = setValidPropsValue(args.data, args.rowData);
                            //args.data = setDropDownValues(masterData, args.data, args.rowData);
                            args.rowData = args.data;

                            if (args.data.ROSID == 0) {
                                args.data.ROSID = _maxROSID--;
                            }
                            var allData = $tblMasterEl.dataSource;

                            var isExist = false;
                            var list = allData.filter(item => item.ItemName === args.data.ItemName);

                            if (list.length > 0 && _isEdit) {
                                list = list.filter(x => x.ROSID != args.data.ROSID);
                            }

                            if (list.length > 0) isExist = true;

                            if (isExist) {
                                toastr.error("Duplicate Item found!!!");
                                args.cancel = true;
                                return;
                            }

                            var dataObj = {
                                ROSID: args.data.ROSID,
                                ItemMasterID: getDefaultValueWhenInvalidN($formEl.find("#ItemMasterID").val()),
                                SubGroupID: getDefaultValueWhenInvalidN($formEl.find("#SubGroupID").val()),
                                CompanyID: getDefaultValueWhenInvalidN($formEl.find("#CompanyID").val()),
                                MonthlyAvgConsumptionLP: args.data.MonthlyAvgConsumptionLP,
                                MonthlyAvgConsumptionFP: args.data.MonthlyAvgConsumptionFP,
                                ROLLocalPurchase: args.data.ROLLocalPurchase,
                                ROLForeignPurchase: args.data.ROLForeignPurchase,
                                MaximumPRQtyLP: args.data.MaximumPRQtyLP,
                                MaximumPRQtyFP: args.data.MaximumPRQtyFP,
                                MOQ: args.data.MOQ,
                                ValidDate: formatDateToDefault(args.data.ValidDate)
                            };

                            args.rowData = DeepClone(args.data);

                            if (!save(dataObj)) {
                                args.cancel = true;
                                return;
                            };
                        }
                        if (args.requestType === 'delete') {


                        }
                    },
                    actionComplete: function (args) {

                        _isEdit = false;
                        if (args.requestType === 'add') {
                            var itemNameID = getDefaultValueWhenInvalidN($formEl.find("#ItemMasterID").val());
                            var companyID = getDefaultValueWhenInvalidN($formEl.find("#CompanyID").val());
                            //$("#ValidDate").val(moment().format("mm/dd/yyyy"));
                            $formEl.find('#ValidDate').val(moment().format("mm/dd/yyyy"));
                            if (itemNameID == 0) {
                                toastr.error("Selecet Item!!!");
                                args.cancel = true;
                                var closeButton = document.querySelector('.e-dialog .e-dlg-closeicon-btn');

                                if (closeButton) {
                                    closeButton.click();
                                }
                                return;
                            }
                            if (companyID == 0) {
                                toastr.error("Selecet Company!!!");
                                args.cancel = true;
                                var closeButton = document.querySelector('.e-dialog .e-dlg-closeicon-btn');

                                if (closeButton) {
                                    closeButton.click();
                                }
                                return;
                            }
                            var itemName = $formEl.find("#ItemName").val();
                            $("#ItemMaster_Finder").val(itemName);
                            args.dialog.header = 'Add Details';

                        }
                        if (args.requestType === 'beginEdit') {
                            _isEdit = true;
                            args.dialog.header = 'Edit Details';
                            $formEl.find("#ROSID").val(args.rowData.ROSID);
                            $formEl.find("#ItemMasterID").val(args.rowData.ItemMasterID);
                            $formEl.find("#SubGroupID").val(args.rowData.SubGroupID);
                            $formEl.find("#CompanyID").val(args.rowData.CompanyID);
                            $formEl.find("#ItemName").val(args.rowData.ItemName);

                            var itemName = $formEl.find("#ItemName").val();
                            $("#ItemMaster_Finder").val(itemName);
                            $("#MonthlyAvgConsumptionLP").val(args.rowData.MonthlyAvgConsumptionLP);
                            $("#MonthlyAvgConsumptionFP").val(args.rowData.MonthlyAvgConsumptionFP);
                            $("#ROLLocalPurchase").val(args.rowData.ROLLocalPurchase);
                            $("#ROLForeignPurchase").val(args.rowData.ROLForeignPurchase);
                            $("#MaximumPRQtyLP").val(args.rowData.MaximumPRQtyLP);
                            $("#MaximumPRQtyFP").val(args.rowData.MaximumPRQtyFP);
                            $("#MOQ").val(args.rowData.MOQ);
                            $("#ValidDate").val(formatDateToDefault(args.rowData.ValidDate));
                        }
                    }
                });
                $tblMasterEl.appendTo('#Grid');
            })
            .catch(showResponseError)
    }
    function getInitData() {

        var url = "/api/item-wise-rol/GetMaster";
        axios.get(url)
            .then(function (response) {
                masterData = response.data;

                setFormData($formEl, masterData);
                initMasterTable();
            })
            .catch(showResponseError)
    }
    function getItemMastedData(subGroupId) {

        var url = "/api/item-wise-rol/getitemmaster/" + subGroupId;
        axios.get(url)
            .then(function (response) {

                masterData.ItemMasterList = response.data.ItemMasterList;
            })
            .catch(showResponseError)
    }
    function save(dataObj, args) {
        //var returnFlag = false;

        //e.preventDefault();
        var data = formElToJson($formEl);
        data.SetupID = 0;
        data.ROSID = getDefaultValueWhenInvalidN(dataObj.ROSID);
        data.ItemMasterID = getDefaultValueWhenInvalidN(dataObj.ItemMasterID);
        data.SubGroupID = getDefaultValueWhenInvalidN(dataObj.SubGroupID);
        data.CompanyID = getDefaultValueWhenInvalidN(dataObj.CompanyID);
        data.MonthlyAvgConsumptionLP = getDefaultValueWhenInvalidN(dataObj.MonthlyAvgConsumptionLP);
        data.MonthlyAvgConsumptionFP = getDefaultValueWhenInvalidN(dataObj.MonthlyAvgConsumptionFP);
        data.ROLLocalPurchase = getDefaultValueWhenInvalidN(dataObj.ROLLocalPurchase);
        data.ROLForeignPurchase = getDefaultValueWhenInvalidN(dataObj.ROLForeignPurchase);
        data.MaximumPRQtyLP = getDefaultValueWhenInvalidN(dataObj.MaximumPRQtyLP);
        data.MaximumPRQtyFP = getDefaultValueWhenInvalidN(dataObj.MaximumPRQtyFP);
        data.ValidDate = formatDateToDefault(dataObj.ValidDate);
        axios.post("/api/item-wise-rol/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                $tblMasterEl.refresh();
                reset();
            })
            .catch(error => {
                if (error.response.data.Message === undefined) {
                    toastr.error(error.response.data);
                } else {
                    toastr.error('Error message:', error.response.data.Message);
                }
                args.cancel = true;
            });
        //.catch(showResponseError);

        //returnFlag = false;
    }
    function addItemMaster(e) {
        e.preventDefault();

        var finder = new commonFinder({
            title: "Select Item Master for Re-Order Status",
            pageId: pageId,
            height: 350,
            width: 500,
            apiEndPoint: "/api/item-wise-rol/get-item-master/",
            fields: "SubGroupName,ItemName",
            headerTexts: "Item Category,Item Name",
            //customFormats: ",,,ej2GridColorFormatter",
            modalSize: "modal-lg",
            widths: "50,100",
            isMultiselect: false,
            primaryKeyColumn: "ItemMasterID",
            onSelect: function (selectedRecords) {
                $formEl.find("#ItemName").val(selectedRecords.rowData.ItemName);
                $formEl.find("#ItemMasterID").val(selectedRecords.rowData.ItemMasterID);
                $formEl.find("#SubGroupID").val(selectedRecords.rowData.SubGroupID);
                document.querySelector('.modal.show .close').click();
            }
        });
        finder.showModal();
    }
    function reset() {
        $formEl.find("#ItemName").val("");
        $formEl.find("#ItemMasterID").val(0);
        $formEl.find("#SubGroupID").val(0);
        $formEl.find("#ItemMaster_Finder").val("");
        $formEl.find("#MonthlyAvgConsumptionLP").val(0);
        $formEl.find("#MonthlyAvgConsumptionFP").val(0);
        $formEl.find("#ROLLocalPurchase").val(0);
        $formEl.find("#ROLForeignPurchase").val(0);
        $formEl.find("#MaximumPRQtyLP").val(0);
        $formEl.find("#MaximumPRQtyFP").val(0);
        $formEl.find("#ValidDate").val(formatGridPopupDate(new Date()));
        //$("#ItemMaster_Finder").val("");
        //$("#MonthlyAvgConsumptionLP").val(0);
        //$("#MonthlyAvgConsumptionFP").val(0);
        //$("#ROLLocalPurchase").val(0);
        //$("#ROLForeignPurchase").val(0);
        //$("#MaximumPRQtyLP").val(0);
        //$("#MaximumPRQtyFP").val(0);
        //$("#ValidDate").val(formatGridPopupDate(new Date()));
    }
})();
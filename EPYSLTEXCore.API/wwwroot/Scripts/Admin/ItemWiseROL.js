(function () {
    var menuId, pageName;
    var $tblMasterEl, $formEl, tblMasterId;
    var itemMasterList, itemMasterLists = []
    var _maxROSID = -999;
    var _isEdit = false;
    var pageId;
    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        //$tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        $formEl.find("#btnAddItemMaster").on("click", addItemMaster);
        getInitData();
    });
    function initMasterTable() {
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
                        template: '#dialogtemplate'
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
                            field: 'MonthlyAvgConsumption',
                            headerText: 'Monthly Avg Consumption',
                        },
                        {
                            field: 'LeadTimeDays',
                            headerText: 'Lead Time Days',
                            width: 120,
                        },
                        {
                            field: 'SafetyStockDays',
                            headerText: 'Safety Stock Days',
                            width: 120,
                        },
                        {
                            field: 'MonthlyWorkingDays',
                            headerText: 'Monthly Working Days',
                            width: 120,
                        },
                        {
                            field: 'PackSize',
                            headerText: 'PackSize',
                            width: 120,
                        },
                        {
                            field: 'MOQ',
                            headerText: 'MOQ',
                            width: 120,
                        },
                        {
                            field: 'ReOrderQty',
                            headerText: 'Re Order Qty',
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
                            debugger;
                            //    // cast string to integer value.
                            //    args.data['NetWeight'] =
                            //        parseFloat(args.form.querySelector("#NetWeight").value);
                            console.log(args.data);

                            args.data.ROSID = getDefaultValueWhenInvalidN(args.data.ROSID);
                            args.rowData = setValidPropsValue(args.data, args.rowData);
                            //args.data = setDropDownValues(masterData, args.data, args.rowData);
                            args.rowData = args.data;

                            if (args.data.ROSID == 0) {
                                args.data.ROSID = _maxROSID--;
                            }
                            var allData = $tblMasterEl.dataSource;
                            console.log(allData);

                            debugger;
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
                                MonthlyAvgConsumption: args.data.MonthlyAvgConsumption,
                                LeadTimeDays: args.data.LeadTimeDays,
                                SafetyStockDays: args.data.SafetyStockDays,
                                MonthlyWorkingDays: args.data.MonthlyWorkingDays,
                                PackSize: args.data.PackSize,
                                MOQ: args.data.MOQ,
                                ReOrderQty: args.data.ReOrderQty
                            };

                            args.rowData = DeepClone(args.data);

                            if (!save(dataObj)) {
                                args.cancel = true;
                                return;
                            };
                        }
                        if (args.requestType === 'delete') {
                            debugger;

                        }
                    },
                    actionComplete: function (args) {
                        debugger;
                        _isEdit = false;
                        if (args.requestType === 'add') {
                            let itemNameID = getDefaultValueWhenInvalidN($formEl.find("#ItemMasterID").val());
                            if (itemNameID == 0) {
                                toastr.error("Please Selecet a Item Master!!!");
                                args.cancel = true;
                                var closeButton = document.querySelector('.e-dialog .e-dlg-closeicon-btn');

                                // If the button is found, click it
                                if (closeButton) {
                                    closeButton.click();
                                }
                                return;
                            }
                            let itemName = $formEl.find("#ItemName").val();
                            $("#ItemMaster_Finder").val(itemName);
                            //getItemMastedData(subGroupID);
                            console.log(args);
                            //ejDropDownLoad(ej, args, masterData.ItemMasterList, "ItemMaster", "text", "id", "ItemName");
                            //function ejDropDownLoad(ej, args, listP, gridFieldName, textFieldName, valueFieldName, placeHolder)
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

                            let itemName = $formEl.find("#ItemName").val();
                            $("#ItemMaster_Finder").val(itemName);
                            $("#MonthlyAvgConsumption").val(args.rowData.MonthlyAvgConsumption);
                            $("#LeadTimeDays").val(args.rowData.LeadTimeDays);
                            $("#SafetyStockDays").val(args.rowData.SafetyStockDays);
                            $("#MonthlyWorkingDays").val(args.rowData.MonthlyWorkingDays);
                            $("#PackSize").val(args.rowData.PackSize);
                            $("#MOQ").val(args.rowData.MOQ);
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
                debugger;
                masterData.ItemMasterList = response.data.ItemMasterList;
            })
            .catch(showResponseError)
    }
    function save(dataObj) {
        let returnFlag = false;
        debugger;
        //e.preventDefault();
        var data = formElToJson($formEl);
        data.SetupID = 0;
        data.ROSID = dataObj.ROSID;
        data.ItemMasterID = dataObj.ItemMasterID;
        data.SubGroupID = dataObj.SubGroupID;
        data.CompanyID = dataObj.CompanyID;
        data.MonthlyAvgConsumption = dataObj.MonthlyAvgConsumption;
        data.LeadTimeDays = dataObj.LeadTimeDays;
        data.SafetyStockDays = dataObj.SafetyStockDays;
        data.MonthlyWorkingDays = dataObj.MonthlyWorkingDays;
        data.PackSize = dataObj.PackSize;
        data.MOQ = dataObj.MOQ;
        console.log(data);
        axios.post("/api/item-wise-rol/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                reset();
                initMasterTable();
            })
            .catch(showResponseError);
        debugger;
        returnFlag = false;
    }
    function addItemMaster(e) {
        e.preventDefault();
        debugger;
        var finder = new commonFinder({
            title: "Select Item Master for Re-Order Status",
            pageId: pageId,
            height: 350,
            width: 500,
            apiEndPoint: "/api/item-wise-rol/getitemmaster/",
            fields: "SubGroupName,ItemName",
            headerTexts: "Item Category,Item Name",
            //customFormats: ",,,ej2GridColorFormatter",

            //widths: "50,80,150,100",
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
        $("#ItemMaster_Finder").val("");
        $("#MonthlyAvgConsumption").val(0);
        $("#LeadTimeDays").val(0);
        $("#SafetyStockDays").val(0);
        $("#MonthlyWorkingDays").val(0);
        $("#PackSize").val(0);
        $("#MOQ").val(0);
    }
})();
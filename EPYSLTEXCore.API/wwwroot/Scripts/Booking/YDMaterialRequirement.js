(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, tblMasterId,
        tblFabricChildId, tblCollarChildId, tblCuffChildId, TblReasonId, $TblReasonEl, $tblChildFabricEl, $tblChildCollarEl, $tblChildCuffEl;

    var status;
    var masterData;
    var ChildItems = new Array();
    var maxCol = 999, vAdditionalBooking = 0;
    var isMRequirementPage = false, isAcknowledgePage = false; 

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblFabricChildId = "#tblFabric-" + pageId;
        tblCollarChildId = "#tblCollar-" + pageId;
        tblCuffChildId = "#tblCuff-" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);  

        isMRequirementPage = convertToBoolean($(`#${pageId}`).find("#MRequirementPage").val());
        isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val());
       
        if (isMRequirementPage) { 
            $toolbarEl.find("#btnPendingList,#btnCompleteList").show();
            $toolbarEl.find("#btnPendingAckList,#btnAckList").hide();
            status = statusConstants.PENDING;
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingList"), $toolbarEl);
            $formEl.find("#btnSave,#btnSaveAndSend").show();
            $formEl.find("#btnAcknowledge").hide();
            initMasterTable();
        }
        else if (isAcknowledgePage) { 
            $toolbarEl.find("#btnPendingList,#btnCompleteList").hide();
            $toolbarEl.find("#btnPendingAckList,#btnAckList").show(); 
            status = statusConstants.AWAITING_PROPOSE;
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingAckList"), $toolbarEl);
            $formEl.find("#btnSave,#btnSaveAndSend").hide();
            $formEl.find("#btnAcknowledge").show();
            initMasterTable();
        }  
        $toolbarEl.find("#btnPendingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            initMasterTable();
        }); 
        $toolbarEl.find("#btnCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.COMPLETED;
            reasonStatus = "BookingList";
            initMasterTable();
        }); 
        $toolbarEl.find("#btnPendingAckList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.AWAITING_PROPOSE;
            initMasterTable();
        }); 
        $toolbarEl.find("#btnAckList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACKNOWLEDGE;
            initMasterTable();
        }); 
        //Button List
        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault(); 
            save(false);
        });

        $formEl.find("#btnSaveAndSend").click(function (e) {
            e.preventDefault();
            save(true);
        });  
        
        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#YDMaterialRequirementMasterID").val();
            axios.post(`/api/yd-material-req/acknowledge/${id}`)
                .then(function () {
                    toastr.success("Acknowledged operation successfull.");
                    backToList();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnCancel").on("click", backToList);  
    });

    function initMasterTable() {
      
        var commands;
        if (status === statusConstants.PENDING) {
            commands = [{ type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }]
        }
        else {
            commands = [{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
        }
        var columns = [];

        if (status === statusConstants.PENDING) {
            columns = [
                {
                    headerText: 'Actions', commands: commands
                }, 
                {
                    field: 'YBookingNo', headerText: 'YD Booking No'
                },
                {
                    field: 'YBookingDate', headerText: 'YD Booking Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
                },
                {
                    field: 'BuyerName', headerText: 'Buyer'
                },
                {
                    field: 'BuyerDepartment', headerText: 'Buyer Department'
                },
                {
                    field: 'CompanyName', headerText: 'Company'
                },
                {
                    field: 'Remarks', headerText: 'Remarks'
                }
            ];
        }
        else {
            columns = [
                {
                    headerText: 'Actions', commands: commands
                }, 
                {
                    field: 'YDMaterialRequirementNo', headerText: 'Material Requirement No'
                },
                {
                    field: 'MaterialRequirementDate', headerText: 'Material Requirement Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
                },
                {
                    field: 'BuyerName', headerText: 'Buyer'
                },
                {
                    field: 'BuyerDepartment', headerText: 'Buyer Department'
                },
                {
                    field: 'CompanyName', headerText: 'Company'
                },
                {
                    field: 'Remarks', headerText: 'Remarks'
                }
            ];
        }

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/yd-material-req/list?status=${status}&PageName=${pageName}`,
            columns: columns,
            commandClick: handleCommands
        }); 
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'Add') {
            getNew(args.rowData);
            $formEl.find("#btnSave,#btnSaveAndSend").fadeIn();
            $formEl.find("#btnAcknowledge").fadeOut();
        }
        else if (args.commandColumn.type == 'Edit') {
            if ((status == statusConstants.COMPLETED)) {
                $formEl.find("#btnSave,#btnSaveAndSend").show();
                $formEl.find("#btnAcknowledge").hide();
            }
            if ((status == statusConstants.AWAITING_PROPOSE)) {
                $formEl.find("#btnSave,#btnSaveAndSend").hide();
                $formEl.find("#btnAcknowledge").show();
            }
            else if ((status == statusConstants.ACKNOWLEDGE)) {
                $formEl.find("#btnSave,#btnSaveAndSend,#btnAcknowledge").hide(); 
            } 

            getDetails(args.rowData); 
        }
    } 

    async function initFabricChildTableAsync(data, subGroupName) {
      
        var isEditable = true;
        if (status == statusConstants.PENDING && status == statusConstants.COMPLETED) {
            isEditable = true;
        } else {
            isEditable = false;
        } 

        if ($tblChildFabricEl) $tblChildFabricEl.destroy();

        var columns = [], additionalColumns = [], childColumns = [];
      
        //YarnBookingChild grid load
      /*  var data = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupName });  */
         
        columns = await getItemColumnsForDisplayBySubGroupAsync(subGroupName);  

        additionalColumns = [
            { field: 'YDMaterialRequirementChildID', isPrimaryKey: true, visible: false },
            { field: 'YBChildID', visible: false },
            { field: 'YBookingID', visible: false },
            { field: 'YarnTypeID', headerText: 'YarnTypeID', visible: false }, 
            { field: 'YarnBrandID', headerText: 'YarnBrandID', visible: false },
            { field: 'UnitID', headerText: 'BookingUnitID', visible: false }, 
            { field: 'Remarks', headerText: 'Instructions', allowEditing: false },
            { field: 'YarnBrand', headerText: 'Yarn Program', allowEditing: false }, 
            { field: 'YarnType', headerText: 'Yarn Type', allowEditing: false }, 
            { field: 'FTechnicalName', headerText: 'Technical Name', allowEditing: false},
            { field: 'BookingQty', headerText: 'Booking Qty', allowEditing: false },
            { field: 'DisplayUnitDesc', headerText: 'UOM', allowEditing: false }
        ];
        columns.push.apply(columns, additionalColumns);

        //YarnBookingChildItem grid load
        /* childColumns = await getYarnItemColumnsAsync(data, false);*/
        childColumns = await getYarnItemColumnsForDisplayOnly();
        childColumns.unshift({ field: 'YBChildItemID', isPrimaryKey: true, visible: false });
        additionalColumns = [
            { field: 'SubGroupId', visible: false },
            { field: 'ShadeCode', headerText: 'ShadeCode', allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks', allowEditing: false },
            { field: 'YD', headerText: 'YD', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', allowEditing: false },
            { field: 'IsPR', headerText: 'Go for PR?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            //{ field: 'YD', headerText: 'YD', editType: "booleanedit", textAlign: 'Center', allowEditing: false },
            { field: 'Distribution', allowEditing: false, headerText: 'Yarn Distribution (%)', editType: "numericedit", params: { decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } },
            { field: 'BookingQty', allowEditing: false, headerText: 'Net Consumption', allowEditing: false, params: { decimals: 0, format: "N2" } },
            { field: 'Allowance', allowEditing: false, headerText: 'Allowance (%)', editType: "numericedit", params: { decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } },
            { field: 'RequiredQty', allowEditing: false, headerText: 'RequiredQty', allowEditing: false, params: { decimals: 0, format: "N2" } },
            { field: 'MRQty', headerText: 'MR. Qty', params: { decimals: 0, format: "N2", validateDecimalOnType: true} },
            { field: 'DisplayUnitDesc', allowEditing: false, headerText: 'UOM', allowEditing: isEditable }
        ];
        childColumns.push.apply(childColumns, additionalColumns); 

        ej.base.enableRipple(true);
        $tblChildFabricEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true, 
            columns: columns,
            //editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            childGrid: {
                queryString: "YBChildID",
                additionalQueryParams: "BookingID",
                allowResizing: true,
                autofitColumns: false, 
                /*toolbar: ['Add'],*/
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns, 
                actionBegin: function (args) {
                     
                },
                load: loadFLChildGridFabric
            }
        });
        $tblChildFabricEl.refreshColumns;
        $tblChildFabricEl.appendTo(tblFabricChildId);
       
    } 

    function loadFLChildGridFabric() {
        this.dataSource = this.parentDetails.parentRowData.ChildItems;
    }
    async function initCollarChildTableAsync(data, subGroupName) {

        var isEditable = true;
        if (status == statusConstants.PENDING && status == statusConstants.COMPLETED) {
            isEditable = true;
        } else {
            isEditable = false;
        }

        if ($tblChildCollarEl) $tblChildCollarEl.destroy();

        var columns = [], additionalColumns = [], childColumns = [];
        
        //YarnBookingChild grid load
       /* var data = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupName });*/

        columns = await getItemColumnsForDisplayBySubGroupAsync(subGroupName);

        additionalColumns = [
            { field: 'YDMaterialRequirementChildID', isPrimaryKey: true, visible: false },
            { field: 'YBChildID', visible: false },
            { field: 'YBookingID', visible: false },
            { field: 'YarnTypeID', headerText: 'YarnTypeID', visible: false },
            { field: 'YarnBrandID', headerText: 'YarnBrandID', visible: false },
            { field: 'UnitID', headerText: 'BookingUnitID', visible: false },
            { field: 'Remarks', headerText: 'Instructions', allowEditing: false },
            { field: 'YarnBrand', headerText: 'Yarn Program', allowEditing: false },
            { field: 'YarnType', headerText: 'Yarn Type', allowEditing: false },
            { field: 'FTechnicalName', headerText: 'Technical Name', allowEditing: false },
            { field: 'BookingQty', headerText: 'Booking Qty', allowEditing: false },
            { field: 'DisplayUnitDesc', headerText: 'UOM', allowEditing: false }
        ];
        columns.push.apply(columns, additionalColumns);

        //YarnBookingChildItem grid load
        /* childColumns = await getYarnItemColumnsAsync(data, false);*/
        childColumns = await getYarnItemColumnsForDisplayOnly();
        childColumns.unshift({ field: 'YBChildItemID', isPrimaryKey: true, visible: false });
        additionalColumns = [
            { field: 'SubGroupId', visible: false },
            { field: 'ShadeCode', headerText: 'ShadeCode', allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks', allowEditing: false },
            { field: 'YD', headerText: 'YD', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', allowEditing: false },
            { field: 'IsPR', headerText: 'Go for PR?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            //{ field: 'YD', headerText: 'YD', editType: "booleanedit", textAlign: 'Center', allowEditing: false },
            { field: 'Distribution', allowEditing: false, headerText: 'Yarn Distribution (%)', editType: "numericedit", params: { decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } },
            { field: 'BookingQty', allowEditing: false, headerText: 'Net Consumption', allowEditing: false, params: { decimals: 0, format: "N2" } },
            { field: 'Allowance', allowEditing: false, headerText: 'Allowance (%)', editType: "numericedit", params: { decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } },
            { field: 'RequiredQty', allowEditing: false, headerText: 'RequiredQty', allowEditing: false, params: { decimals: 0, format: "N2" } },
            { field: 'MRQty', headerText: 'MR. Qty', params: { decimals: 0, format: "N2", validateDecimalOnType: true } },
            { field: 'DisplayUnitDesc', allowEditing: false, headerText: 'UOM', allowEditing: isEditable }
        ];
        childColumns.push.apply(childColumns, additionalColumns);

        ej.base.enableRipple(true);
        $tblChildCollarEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            columns: columns,
            //editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            childGrid: {
                queryString: "YBChildID",
                additionalQueryParams: "BookingID",
                allowResizing: true,
                autofitColumns: false,
                /*toolbar: ['Add'],*/
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns,
                actionBegin: function (args) {

                },
                load: loadFLChildGridCollar
            }
        });
        $tblChildCollarEl.refreshColumns;
        $tblChildCollarEl.appendTo(tblCollarChildId);
       
      
    }

    function loadFLChildGridCollar() {
        this.dataSource = this.parentDetails.parentRowData.ChildItems;
    }
    async function initCuffChildTableAsync(data, subGroupName) {

        var isEditable = true;
        if (status == statusConstants.PENDING && status == statusConstants.COMPLETED) {
            isEditable = true;
        } else {
            isEditable = false;
        }

        if ($tblChildCuffEl) $tblChildCuffEl.destroy();

        var columns = [], additionalColumns = [], childColumns = [];

        //YarnBookingChild grid load
        //var data = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupName });

        columns = await getItemColumnsForDisplayBySubGroupAsync(subGroupName);

        additionalColumns = [
            { field: 'YDMaterialRequirementChildID', isPrimaryKey: true, visible: false },
            { field: 'YBChildID', visible: false },
            { field: 'YBookingID', visible: false },
            { field: 'YarnTypeID', headerText: 'YarnTypeID', visible: false },
            { field: 'YarnBrandID', headerText: 'YarnBrandID', visible: false },
            { field: 'UnitID', headerText: 'BookingUnitID', visible: false },
            { field: 'Remarks', headerText: 'Instructions', allowEditing: false },
            { field: 'YarnBrand', headerText: 'Yarn Program', allowEditing: false },
            { field: 'YarnType', headerText: 'Yarn Type', allowEditing: false },
            { field: 'FTechnicalName', headerText: 'Technical Name', allowEditing: false },
            { field: 'BookingQty', headerText: 'Booking Qty', allowEditing: false },
            { field: 'DisplayUnitDesc', headerText: 'UOM', allowEditing: false }
        ];
        columns.push.apply(columns, additionalColumns);

        //YarnBookingChildItem grid load
        /* childColumns = await getYarnItemColumnsAsync(data, false);*/
        childColumns = await getYarnItemColumnsForDisplayOnly();
        childColumns.unshift({ field: 'YBChildItemID', isPrimaryKey: true, visible: false });
        additionalColumns = [
            { field: 'SubGroupId', visible: false },
            { field: 'ShadeCode', headerText: 'ShadeCode', allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks', allowEditing: false },
            { field: 'YD', headerText: 'YD', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', allowEditing: false },
            { field: 'IsPR', headerText: 'Go for PR?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            //{ field: 'YD', headerText: 'YD', editType: "booleanedit", textAlign: 'Center', allowEditing: false },
            { field: 'Distribution', allowEditing: false, headerText: 'Yarn Distribution (%)', editType: "numericedit", params: { decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } },
            { field: 'BookingQty', allowEditing: false, headerText: 'Net Consumption', allowEditing: false, params: { decimals: 0, format: "N2" } },
            { field: 'Allowance', allowEditing: false, headerText: 'Allowance (%)', editType: "numericedit", params: { decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } },
            { field: 'RequiredQty', allowEditing: false, headerText: 'RequiredQty', allowEditing: false, params: { decimals: 0, format: "N2" } },
            { field: 'MRQty', headerText: 'MR. Qty', params: { decimals: 0, format: "N2", validateDecimalOnType: true } },
            { field: 'DisplayUnitDesc', allowEditing: false, headerText: 'UOM', allowEditing: isEditable }
        ];
        childColumns.push.apply(childColumns, additionalColumns);

        ej.base.enableRipple(true);
        $tblChildCuffEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            columns: columns,
            //editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            childGrid: {
                queryString: "YBChildID",
                additionalQueryParams: "BookingID",
                allowResizing: true,
                autofitColumns: false,
                /*toolbar: ['Add'],*/
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns,
                actionBegin: function (args) {

                },
                load: loadFLChildGridCuff
            }
        });
        $tblChildCuffEl.refreshColumns;
        $tblChildCuffEl.appendTo(tblCuffChildId);
        
    }

    function loadFLChildGridCuff() {
        this.dataSource = this.parentDetails.parentRowData.ChildItems;
    }

    function getNew(rowData) {
        var url = `/api/yd-material-req/new/${rowData.YBookingNo}/${rowData.WithoutOB}`;
        axios.get(url)
            .then(function (response) {
                masterData = response.data;
                //console.log(masterData);
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                //masterData.HasFabric = response.data.HasFabric;
                //masterData.HasCollar = response.data.HasCollar;
                //masterData.HasCuff = response.data.HasCuff;
               /* masterData.Childs = response.data.Childs;*/

                masterData.MaterialRequirementDate = formatDateToDefault(masterData.MaterialRequirementDate);
                masterData.YBookingDate = formatDateToDefault(masterData.YBookingDate);


                
                setFormData($formEl, masterData);

                masterData.HasFabric = response.data.HasFabric;
                masterData.HasCollar = response.data.HasCollar;
                masterData.HasCuff = response.data.HasCuff;

                if (masterData.HasFabric) {
                    var FabricData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.FABRIC });
                    initFabricChildTableAsync(FabricData, subGroupNames.FABRIC);
                    $formEl.find("#divFabricInfo").show();
                }
                else {
                    $formEl.find("#divFabricInfo").hide();
                }

                if (masterData.HasCollar) {
                    var CollarData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.COLLAR });
                    initCollarChildTableAsync(CollarData, subGroupNames.COLLAR);
                    $formEl.find("#divCollarInfo").show();
                }
                else {
                    $formEl.find("#divCollarInfo").hide();
                }

                if (masterData.HasCuff) {
                    var CuffData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.CUFF });
                    initCuffChildTableAsync(CuffData, subGroupNames.CUFF)
                    $formEl.find("#divCufInfo").show();
                }
                else {
                    $formEl.find("#divCufInfo").hide();
                }
            })
            .catch(showResponseError);
    }

    function getDetails(rowData) {
        axios.get(`/api/yd-material-req/${rowData.YDMaterialRequirementNo}/${rowData.YBookingID}/${rowData.WithoutOB}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.MaterialRequirementDate = formatDateToDefault(masterData.MaterialRequirementDate);
                masterData.YBookingDate = formatDateToDefault(masterData.YBookingDate); 
                setFormData($formEl, masterData);

                masterData.HasFabric = response.data.HasFabric;
                masterData.HasCollar = response.data.HasCollar;
                masterData.HasCuff = response.data.HasCuff;
                vAdditionalBooking = masterData.AdditionalBooking;
           
                if (masterData.HasFabric) {
                    var FabricData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.FABRIC });
                    initFabricChildTableAsync(FabricData, subGroupNames.FABRIC);
                    $formEl.find("#divFabricInfo").show();
                }
                else {
                    $formEl.find("#divFabricInfo").hide();
                }

                if (masterData.HasCollar) {
                    var CollarData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.COLLAR });
                    initCollarChildTableAsync(CollarData, subGroupNames.COLLAR);
                    $formEl.find("#divCollarInfo").show();
                }
                else {
                    $formEl.find("#divCollarInfo").hide();
                }

                if (masterData.HasCuff) {
                    var CuffData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.CUFF });
                    initCuffChildTableAsync(CuffData, subGroupNames.CUFF)
                    $formEl.find("#divCufInfo").show();
                }
                else {
                    $formEl.find("#divCufInfo").hide();
                }
            })
            .catch(showResponseError);
    }

    function backToList() {
      
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        $tblMasterEl.refresh();
        ChildItems.length = 0;
        /*$tblChildEl.refresh();*/
        $formEl.find(tblFabricChildId).html("");
        $formEl.find(tblCollarChildId).html("");
        $formEl.find(tblCuffChildId).html("");
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#YDMaterialRequirementMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function save(flag) {
        var DataList = [];
        var DataListMaster = new Array();

        var data = formDataToJson($formEl.serializeArray());

        if (masterData.HasFabric) {
            let fabrics = $tblChildFabricEl.getCurrentViewRecords();

            for (let i = 0; i < fabrics.length; i++) {
                for (let j = 0; j < fabrics[i].ChildItems.length; j++) {
                    if (fabrics[i].ChildItems[j].Segment1ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment1ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment2ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment2ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment3ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment3ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment4ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment4ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment5ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment5ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment6ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment6ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment7ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment7ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment8ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment8ValueId = 0;
                    }
                }
            }

            for (let i = 0; i < fabrics.length; i++) {
                DataList.push(fabrics[i]);
            }

            DataListMaster.push(data);
        }

        if (masterData.HasCollar) {
            let collars = $tblChildCollarEl.getCurrentViewRecords();

            for (let i = 0; i < collars.length; i++) {
                for (let j = 0; j < collars[i].ChildItems.length; j++) {
                    if (collars[i].ChildItems[j].Segment1ValueId == undefined) {
                        collars[i].ChildItems[j].Segment1ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment2ValueId == undefined) {
                        collars[i].ChildItems[j].Segment2ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment3ValueId == undefined) {
                        collars[i].ChildItems[j].Segment3ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment4ValueId == undefined) {
                        collars[i].ChildItems[j].Segment4ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment5ValueId == undefined) {
                        collars[i].ChildItems[j].Segment5ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment6ValueId == undefined) {
                        collars[i].ChildItems[j].Segment6ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment7ValueId == undefined) {
                        collars[i].ChildItems[j].Segment7ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment8ValueId == undefined) {
                        collars[i].ChildItems[j].Segment8ValueId = 0;
                    }
                }
            }

            for (let i = 0; i < collars.length; i++) {
                DataList.push(collars[i]);
            }
            DataListMaster.push(data);
        }

        if (masterData.HasCuff) {
            let cuffs = $tblChildCuffEl.getCurrentViewRecords();

            for (let i = 0; i < cuffs.length; i++) {
                for (let j = 0; j < cuffs[i].ChildItems.length; j++) {
                    if (cuffs[i].ChildItems[j].Segment1ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment1ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment2ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment2ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment3ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment3ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment4ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment4ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment5ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment5ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment6ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment6ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment7ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment7ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment8ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment8ValueId = 0;
                    }
                }
            }

            for (let i = 0; i < cuffs.length; i++) {
                DataList.push(cuffs[i]);
            }

            DataListMaster.push(data);
        }

        data.Propose = flag;
        data["HasFabric"] = masterData.HasFabric;
        data["HasCollar"] = masterData.HasCollar;
        data["HasCuff"] = masterData.HasCuff;

        data["Childs"] = DataList;
        if (data.Childs.length === 0) return toastr.error("At least 1 Yarn items is required.");

        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        //if (isValidChildForm(data)) return;   

        model = DataListMaster;
        model.IsModified = (status == statusConstants.PENDING) ? false : true;

        axios.post("/api/yd-material-req/save", model)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

     
    function isValidChildForm(data) {
        var isValidItemInfo = false;

        var Distribution = 0; 
        $.each(data["Childs"], function (i, el) {
            Distribution = 0;
            //debugger
            $.each(data["ChildItems"], function (j, cd) {
                if (el.YBChildID == cd.YBChildID) {
                    Distribution += parseFloat(cd.Distribution);
                    if (parseInt(Distribution) > 100) { 
                        toastr.error("Yarn Distribution must be 100% or less than 100% ( " + el.SubGroupName + ", Row No = " + i + ") ");
                        isValidItemInfo = true;
                        Distribution = 0;
                    }
                }
            });
        });

        return isValidItemInfo;
    }

    var validationConstraints = {
        //ChallanNo: {
        //    presence: true
        //}, 
    }
})();
(function () {
    var menuId, pageName;
    var status = statusConstants.PENDING;

    var toolbarId, pageId, $pageEl, $formEl, $divTblEl, $divDetailsEl, $toolbarEl,
        $tblMasterEl, tblMasterId,
        $tblChildEl, tblChildId;

    var masterData;  
    var vIsBDS = 0; 
    var RollListChilds = new Array();

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
       
        tblChildSetId = "#tblChildSet" + pageId;
        tblChildSetDetailsId = "#tblChildSetDetails" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);  

        $toolbarEl.find("#btnPendingList,#btnQCPassList,#btnQCFailList,#btnQCHoldList").show();  
        toggleActiveToolbarBtn("#btnPendingList", $toolbarEl); 
        $toolbarEl.find("#btnAddDC").fadeOut();
        status = statusConstants.PENDING;

        initMasterTable();

        $toolbarEl.find("#btnPendingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            isEditable = false; 
            initMasterTable();
        });
        $toolbarEl.find("#btnQCPassList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.COMPLETED;
            isEditable = false;
            $toolbarEl.find("#btnAddDC").fadeOut();
            initMasterTable();
        });
        $toolbarEl.find("#btnQCFailList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.AWAITING_PROPOSE;
            isEditable = false;
            $toolbarEl.find("#btnAddDC").fadeOut();
            initMasterTable();
        });
        $toolbarEl.find("#btnQCHoldList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PARTIALLY_COMPLETED;
            isEditable = false;
            $toolbarEl.find("#btnAddDC").fadeOut();
            initMasterTable();
        });  

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#btnAddRoll").click(function (e) {
            e.preventDefault();
            
            var RollList = masterData.Childs.find(function (el) {
                return el.RollNo == $formEl.find("#RollNo").val()
            });
            var oRollListProcess = {
                DBIRollID: RollList.DBIRollID,
                DBIID: RollList.DBIID,
                DBatchID: RollList.DBatchID,
                GRollID: RollList.GRollID,
                RollNo: RollList.RollNo,
                RollQty: RollList.RollQty,
                RollQtyPcs: RollList.RollQtyPcs 
            }
            RollListChilds.push(oRollListProcess);
            initChildTable(RollListChilds); 
        }); 

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(false);
        });
      

        $formEl.find("#btnApprove").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#SFDID").val();
            axios.post(`/api/sample-delivery-challan/approve/${id}`)
                .then(function () {
                    toastr.success(constants.APPROVE_SUCCESSFULLY);
                    backToList();
                })
                .catch(showResponseError);
        }); 

        $formEl.find("#QCPass,#QCFail,#Hold").change(function () { 
            if ($(this).attr('id') == 'QCPass') {
                $formEl.find("#QCPass").prop("checked", true);
                $formEl.find("#Hold").prop("checked", false);
                $formEl.find("#QCFail").prop("checked", false);
                $formEl.find("#divOnlineQCHoldRemarks").fadeOut();
            }
            else if ($(this).attr('id') == 'QCFail') {
                $formEl.find("#QCFail").prop("checked", true);
                $formEl.find("#Hold").prop("checked", false);
                $formEl.find("#QCPass").prop("checked", false);
                $formEl.find("#divOnlineQCHoldRemarks").fadeOut();
            }
            else {
                $formEl.find("#Hold").prop("checked", true);
                $formEl.find("#QCPass").prop("checked", false);
                $formEl.find("#QCFail").prop("checked", false);
                $formEl.find("#divOnlineQCHoldRemarks").fadeIn();
            } 
        }); 
    });

    function initMasterTable() {
        //console.log($tblMasterEl);

        var commands = [];
        var columns = []; 
        commands = [
            {
                type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' },
            }
        ] 

        if (status == statusConstants.PENDING) {
            columns = [
                {
                    headerText: 'Action', commands: commands, //visible: status == statusConstants.PENDING ? false : true,
                    textAlign: 'Center', width: 100, minWidth: 100, maxWidth: 100
                },
                {
                    field: 'BookingNo', headerText: 'Booking No'//, width: 25
                },
                {
                    field: 'DBatchNo', headerText: 'D.Batch No' 
                },
                {
                    field: 'ColorName', headerText: 'Color Name' 
                }, 
                {
                    field: 'BatchWeightKG', headerText: 'Batch Qty(Kg)'
                },
                {
                    field: 'BatchQtyPcs', headerText: 'Batch Qty(Pcs)'
                },
                {
                    field: 'BuyerName', headerText: 'Buyer'
                },
                {
                    field: 'BuyerDepartment', headerText: 'BuyerTeam'
                }  
            ];
        }
        else {
            columns = [
                {
                    headerText: 'Action', commands: commands, //visible: status == statusConstants.PENDING ? false : true,
                    textAlign: 'Center', width: 100, minWidth: 100, maxWidth: 100
                },
                //{
                //    field: 'BookingNo', headerText: 'Booking No'//, width: 25
                //},
                {
                    field: 'DBatchNo', headerText: 'D.Batch No'
                },
                {
                    field: 'DBatchDate', headerText: 'D.Batch Date', type: 'date', format: _ch_date_format_1
                },
                {
                    field: 'ColorName', headerText: 'Color Name'
                },
                {
                    field: 'BatchWeightKG', headerText: 'Batch Qty(Kg)'
                },
                {
                    field: 'BatchQtyPcs', headerText: 'Batch Qty(Pcs)'
                },
                {
                    field: 'RollQty', headerText: 'RollQty(Kg)'
                },
                {
                    field: 'RollQtyPcs', headerText: 'RollQty(Pcs)'
                },
                {
                    field: 'Buyer', headerText: 'Buyer'
                },
                {
                    field: 'BuyerTeam', headerText: 'BuyerTeam'
                }
            ];
        }  

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/onlinefabric-qc/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        }); 
        //console.log($tblMasterEl);
    }       

    function handleCommands(args) {
        if (status == statusConstants.PENDING) {
            getNew(args.rowData.DBatchID);
        }
        else {
            getDetails(args.rowData.DBatchID);
        }
    }

    function getNew(DBatchID) {
        axios.get(`/api/onlinefabric-qc/new/${DBatchID}`)
            .then(function (response) {

                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.DBatchDate = formatDateToDefault(masterData.DBatchDate);
                masterData.BookingNo = masterData.BookingNo;

                setFormData($formEl, masterData); 
                $formEl.find("#btnSave").fadeIn();
                initChildTable([]);

                //console.log(masterData);
                 
            })
            .catch(showResponseError);
    }  
   
    function initChildTable(data) { 
        if ($tblChildEl) {
            $tblChildEl.destroy();
            $(tblChildId).html("");
        }
        ej.base.enableRipple(true);
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowRowDragAndDrop: false,
            allowResizing: true,
            autofitColumns: false,
            selectionSettings: { type: 'Multiple' },
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },  
            columns: [ 
                {
                    headerText: 'Commands', width: 80, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                }, 
                { field: 'DBIRollID', isPrimaryKey: true, visible: false },
                { field: 'DBIID', visible: false },
                { field: 'DBatchID', visible: false },
                { field: 'GRollID', visible: false },
                { field: 'RollQty', headerText: 'RollQty', allowEditing: false, visible: false },
                { field: 'RollQtyPcs', headerText: 'RollQty(Pcs)', allowEditing: false, visible: false },
                { field: 'RollNo', headerText: 'Roll No', allowEditing: true  }
            ],
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.DBIRollID = getMaxIdForArray(masterData.Childs, "DBIRollID");
                }
                else if (args.requestType === "delete") { 
                    var index = $tblChildEl.getRowIndexByPrimaryKey(masterData.Childs, "DBIRollID"); 
                }
                else if (args.requestType === "save") { 
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.DBIRollID);
                }
            },  
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }  

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
        RollListChilds = [];
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#ConceptID").val(-1111);
        $formEl.find("#EntityState").val(4);
    } 
   
    function getDetails(id) {
        axios.get(`/api/onlinefabric-qc/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.DBatchDate = formatDateToDefault(masterData.DBatchDate);

                if (masterData.OnlineQCPass) {
                    $formEl.find("#QCPass").prop("checked", true);
                    $formEl.find("#divOnlineQCHoldRemarks").fadeOut();
                }
                else if (masterData.OnlineQCFail) {
                    $formEl.find("#QCFail").prop("checked", true);
                    $formEl.find("#divOnlineQCHoldRemarks").fadeOut();
                }
                else {
                    $formEl.find("#Hold").prop("checked", true);
                    $formEl.find("#divOnlineQCHoldRemarks").fadeIn();
                } 

                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
            })
            .catch(showResponseError);
    } 

    var validationConstraints = {
        OnlineQCBy: {
            presence: true
        },
        QCShiftID: {
            presence: true
        } 
    }
    
    function isValidChildForm(data) { 
        var isValidItemInfo = false;  
        
        $.each(data, function (i, obj) { 
            if (obj.SubGroupID == 1) { 
                if (obj.DCQty == null || obj.DCQty == undefined || obj.DCQty <= 0) {
                    toastr.error("Challan Qty is required.");
                    isValidItemInfo = true;
                }
            }
            if (obj.SubGroupID != 1) {
                if (obj.DCQtyPcs == null || obj.DCQtyPcs == undefined || obj.DCQtyPcs <= 0) {
                    toastr.error("Challan Qty(Pcs) is required.");
                    isValidItemInfo = true;
                }
            }
        });
        
        $.each(data, function (i, obj) {
            $.each(obj.ChildItems, function (j, objChild) {
                if (objChild.Shade == null || objChild.Shade == "") {
                    toastr.error("Shade is required.");
                    isValidItemInfo = true;
                }
            });
        });

        return isValidItemInfo;
    }

    function save(flag) { 
        var data = formDataToJson($formEl.serializeArray()); 

        data["Childs"] = $tblChildEl.getCurrentViewRecords(); 
        if (data.Childs.length === 0) return toastr.error("At least 1 items is required.");   

        //Get Radio Button value 
        if ($formEl.find("#QCPass").is(':checked')) {
            data.OnlineQCPass = 1;
            data.OnlineQCFail = 0;
            data.OnlineQCHold = 0;
        }
        else if ($formEl.find("#QCFail").is(':checked')) {
            data.OnlineQCPass = 0;
            data.OnlineQCFail = 1;
            data.OnlineQCHold = 0;
        }
        else {
            data.OnlineQCPass = 0;
            data.OnlineQCFail = 0;
            data.OnlineQCHold = 1;
        } 

        //Master Data Validate
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        //Child Item Validation
        //if (isValidChildForm(data["Childs"])) return; 

        axios.post("/api/onlinefabric-qc/save", data)
            .then(function (response) {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();
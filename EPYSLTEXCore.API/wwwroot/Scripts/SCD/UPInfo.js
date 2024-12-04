(function () { 
    'use strict'
    var currentChildRowData;
    var menuId, pageName;
    var toolbarId, pageId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl,
        $tblMasterEl, tblMasterId, $formEl,
        $tblChildEl, tblChildId,
        $tblUPAmendmentEl, tblUPAmendmentId;
    var status;
    var isEditable = false;
    var isNewSave = false;
    var isAcknowledge = false;
    var ChildsLCList = new Array();
    var ImagesContentList = new Array();
    var ImagesList = new Array();
    var selectedId = 0; 

    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }

    var masterData;

    $(function () { 
        if(!menuId)
        menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $pageEl = $(pageConstants.PAGE_ID_PREFIX + pageId);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblUPAmendmentId = "#tblUPAmendment" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId); 
      
        //Load Event
        status = statusConstants.PENDING;
        initMasterTable(); 

        //Button Click Event
        $toolbarEl.find("#btnReceiveLists").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;  
            initMasterTable(); 
        });

        $toolbarEl.find("#btnNew").on("click", getNew);

        $formEl.find("#btnSave").click(function (e) { 
            //Validation Set in Master & Child
            //initializeValidation($formEl, validationConstraints);
            //if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
            //else hideValidationErrors($formEl); 

            e.preventDefault();
            save(isNewSave);
        });

        $formEl.find("#btnCancel").on("click", backToList);

         //Change Event
        $("#ContractID").on("select2:select select2:unselect", function (e) { 
            getLCInfoByContractID($formEl.find("#ContractID").val());
        });

        $formEl.find("#btnAddLC").on("click", function (e) {
            e.preventDefault();
            //var ChildsLC = masterData.Childs; 
            var finder = new commonFinder({
                title: "Select Items",
                pageId: pageId,
                data: masterData.Childs,
                fields: "ContractNo,LCNo,LCDate,SupplierName,BBLCValue,CurrencyCode,Tolerance",
                headerTexts: "ContractNo,LCNo,LCDate,SupplierName,BBLCValue,CurrencyCode,Tolerance",
                isMultiselect: true,
                //selectedIds: masterData.Childs[0].UPCItemIDs,
                allowPaging: false,
                primaryKeyColumn: "BBLCID",
                onMultiselect: function (selectedRecords) {
                    ChildsLCList = $tblChildEl.getCurrentViewRecords();  
                     
                    for (var i = 0; i < selectedRecords.length; i++) {
                        var exists = ChildsLCList.find(function (el) {
                            return el.BBLCID == selectedRecords[i].BBLCID  
                        });

                        if (!exists) {
                            var oPreProcess = {
                                UPChildID: selectedRecords[i].UPChildID,
                                UPMasterID: 0,
                                BBLCID: selectedRecords[i].BBLCID,
                                ContractNo: selectedRecords[i].ContractNo,
                                LCNo: selectedRecords[i].LCNo,
                                LCDate: selectedRecords[i].LCDate,
                                SupplierID: selectedRecords[i].SupplierID,
                                SupplierName: selectedRecords[i].SupplierName,
                                BBLCValue: selectedRecords[i].BBLCValue,
                                CurrencyID: selectedRecords[i].CurrencyID,
                                CurrencyCode: selectedRecords[i].CurrencyCode,
                                Tolerance: selectedRecords[i].Tolerance,
                                ProposeContractID: selectedRecords[i].ProposeContractID 
                            }
                            //masterData.Childs[0].UPCItemIDs = selectedRecords.map(function (el) {
                            //    return el.UPChildID
                            //}).toString();
                            
                            ChildsLCList.push(oPreProcess);
                        }
                    } 
                    initChildTable(ChildsLCList);
                }
            });
            finder.showModal();
        });

        $("#btnAddImages").click(function (e) {
            e.preventDefault();
            var totalfiles = document.getElementById('ProductImage').files.length;

            for (var i = 0; i < totalfiles; i++) {
                //Image Content File
                var objList = new Object;
                //objList.UPAmendID = i + 1;
                objList.UPAmendID = selectedId;
                objList.ImageURL = "/Uploads/UP/" + document.getElementById('ProductImage').files[i].name;
                //objList.Image64Byte = "";
                objList.ImageName = document.getElementById('ProductImage').files[i].name;
                //objList.PreviewTemplate = "PDF";
                //objList.DefaultImage = 0;
                ImagesContentList.push(objList);

                //Image File
                var obj = document.getElementById('ProductImage').files[i];
                ImagesList.push(obj);
            }
            $("#modal-child-Live-Product").modal('hide');

            $("#ProductImage").val('');
            document.getElementById("ProductImage").innerHTML = "";
            document.getElementById("ProductImagediv").innerHTML = "";
            $(".file-drop-zone").text("");
            //console.log(ImagesContentList);
        });
    });
     
    function initMasterTable() {
        var columns = [
            {
                headerText: 'Commands', commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
            },
            {
                field: 'UPNo', headerText: 'UP No'
            },
            {
                field: 'UPNoDate', headerText: 'UP Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            //{
            //    field: 'AmendmentNo', headerText: 'Amendment No'
            //},
            //{
            //    field: 'AmendmentDate', headerText: 'Amendment Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            //},
            {
                field: 'ApplicationNo', headerText: 'Application No'
            },
            {
                field: 'ApplicationDate', headerText: 'Application Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'ContractNo', headerText: 'Contract No'
            },
            {
                field: 'CompanyName', headerText: 'Business Unit'
            }
            //,
            //{
            //    field: 'LC', headerText: 'BBLC/Import LC No'
            //}
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            /*apiEndPoint: `/api/up-info/list`,*/
            apiEndPoint: `/api/up-info/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.UPMasterID); 
        }
    }
    function initAttachment(path, type, htmlEl) {
        if (!path) {
            initNewAttachment(htmlEl);
            return;
        }

        var preveiwData = [rootPath + path];
        var previewConfig = [{ type: type, caption: "UP File", key: 1 }];

        $(htmlEl).fileinput('destroy');
        $(htmlEl).fileinput({
            showUpload: false,
            initialPreview: preveiwData,
            initialPreviewAsData: true,
            initialPreviewFileType: 'image',
            initialPreviewConfig: previewConfig,
            purifyHtml: true,
            required: true,
            maxFileSize: 4096
        }); 
    }

    function initNewAttachment($el) {
        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            previewFileType: 'any'
        });
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
            editSettings: {
                allowAdding: false, allowEditing: false, allowDeleting: true,
                mode: "Normal", showDeleteConfirmDialog: true
            },
            columns: [
                {
                    headerText: 'Commands', width: 80, commands: [ 
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
                { field: 'UPChildID', isPrimaryKey: true, visible: false },
                { field: 'BBLCID', visible: false },
                { field: 'ContractNo', headerText: 'Contract No', allowEditing: false, width: 80 },
                { field: 'LCNo', headerText: 'LCNo', allowEditing: false, width: 80 },
                { field: 'LCDate', headerText: 'LCDate', allowEditing: false, width: 80, type: 'date', format: _ch_date_format_1 },
                { field: 'SupplierName', headerText: 'Supplier', allowEditing: false, width: 80 },
                { field: 'BBLCValue', headerText: 'LCValue', allowEditing: false, width: 80 },
                { field: 'CurrencyCode', headerText: 'Currency', allowEditing: false, width: 80 },
                { field: 'Tolerance', headerText: 'Tolerance', allowEditing: false, width: 80 }
            ],
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.UPChildID = getMaxIdForArray(masterData.Childs, "UPChildID");
                }
                else if (args.requestType === "delete") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(masterData.Childs, "UPChildID");
                    var ChildsList = $tblChildEl.getCurrentViewRecords();
                    //masterData.Childs[0].UPCItemIDs = ChildsList.map(function (el) {
                    //    if (args.data[0].UPChildID != el.UPChildID) {
                    //        return el.UPChildID
                    //    }
                    //}).toString();
                }
                else if (args.requestType === "save") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.UPChildID);
                }
            }  
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }

    async function initUPAmendmentTable(data) {
        if ($tblUPAmendmentEl) $tblUPAmendmentEl.destroy();

        var columns = [];
        var additionalColumns = [];

        //columns = [
        //    {
        //        headerText: 'Commands', width: 100,
        //        commands: [
        //            { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } }, 
        //            { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
        //            { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
        //        ]
        //    }
        //];
        columns = [
            { field: 'UPAmendID', isPrimaryKey: true, visible: false },
            {
                headerText: '...', width: 30, commands: [
                    { buttonOption: { type: 'AmendmentImg', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-edit' } }
                ]
            },
            { field: 'AmendmentNo', headerText: 'Amendment No', width: 100 },
            {
                field: 'AmendmentDate', headerText: 'Amendment Date', type: 'date', format: _ch_date_format_1,
                editType: 'datepickeredit', width: 70, textAlign: 'Center'
            },
            { field: 'Remarks', headerText: 'Remarks', width: 200  }//,
            //{
            //    headerText: '...', width: 30, commands: [
            //        { buttonOption: { type: 'AmendmentImg', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
            //    ]
            //},
        ]; 
        columns.push.apply(columns, additionalColumns);

        var tableOptions = {
            tableId: tblUPAmendmentId,
            data: data,
            columns: columns, 
            actionBegin: function (args) { 
                if (args.requestType === "add") { 
                    args.data.UPAmendID = getMaxIdForArray(masterData.UPAmendments, "UPAmendID");
                } 
                else if (args.requestType === "save") {
                    var index = $tblUPAmendmentEl.getRowIndexByPrimaryKey(args.rowData.UPAmendID);
                    masterData.UPAmendments[index] = args.data;
                } 
            },
            commandClick: commandClickAmendment,
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false 
        };
         
       /* tableOptions["toolbar"] = ['Add']; */
        tableOptions["editSettings"] = {
            allowAdding: true, allowEditing: true, allowDeleting:
                true, mode: "Normal", showDeleteConfirmDialog: true
        };
        $tblUPAmendmentEl = new initEJ2Grid(tableOptions);
    } 
    
    function commandClickAmendment(e) {
        //debugger
        var data = e.rowData; 
        if (e.commandColumn.buttonOption.type == 'AmendmentImg') { 
            $formEl.find("#AmendmentNo").val(data.AmendmentNo);
            $formEl.find("#AmendmentDate").val(data.AmendmentDate);
            $formEl.find("#Remarks").val(data.Remarks); 
            initNewAttachment($("#UploadFileAMD"));
            initAttachment(data.ImageURL, "pdf", $formEl.find("#UploadFileAMD"));
        }
    }
     
    function backToList() {
        toggleActiveToolbarBtn($toolbarEl.find("#btnReceiveLists"), $toolbarEl);
        $divDetailsEl.fadeOut();
        $formEl.find("#LReceiveMasterID").val(-1111);
        resetForm();
        $divTblEl.fadeIn();
        status = statusConstants.PENDING;
        initMasterTable(); 
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#LReceiveMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew() { 
        axios.get(`/api/up-info/new`)
            .then(function (response) {
                isNewSave = false;
                status = statusConstants.PENDING;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut(); 
                masterData = response.data;
                masterData.UPNoDate = formatDateToDefault(masterData.UPNoDate);
                masterData.ApplicationDate = formatDateToDefault(masterData.ApplicationDate);
                masterData.AmendmentDate = formatDateToDefault(masterData.AmendmentDate);

                setFormData($formEl, masterData);
                initChildTable([]);
                initUPAmendmentTable([]);
                $formEl.find("#divAmendment").fadeOut();
                initNewAttachment($formEl.find("#UploadFile"));
            })
            .catch(showResponseError);
    } 

    function getDetails(id) {
        var url = `/api/up-info/${id}`;
        axios.get(url)
            .then(function (response) {
                isNewSave = true;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.UPNoDate = formatDateToDefault(masterData.UPNoDate);
                masterData.ApplicationDate = formatDateToDefault(masterData.ApplicationDate);
                masterData.AmendmentDate = formatDateToDefault(masterData.AmendmentDate);  
                //debugger
                initAttachment(masterData.ImageURL, "pdf", $formEl.find("#UploadFile"));
                setFormData($formEl, masterData);

                
                initChildTable(masterData.Childs);
                initUPAmendmentTable(masterData.UPAmendments);
                $formEl.find("#divAmendment").fadeIn(); 
            })
            .catch(showResponseError);
    } 

    function getLCInfoByContractID(proposeContractID) {
        axios.get(`/api/up-info/new/${proposeContractID}`)
            .then(function (response) {
                masterData = response.data;
                //console.log(masterData);
                $formEl.find("#CompanyName").val(masterData.Childs[0].CompanyName);
                $formEl.find("#CompanyID").val(masterData.Childs[0].CompanyID);
            })
            .catch(function () {
                toastr.error(err.response.data.Message);
            });
    }

    function save(isApprove = false) {
        var formData = getFormData($formEl);

        //Master PDF file upload
        var files = $formEl.find("#UploadFile")[0].files;
        formData.append("UploadFile", files[0]); 

        //Amendment PDF file upload
        if (isApprove) { 
            var files = $formEl.find("#UploadFileAMD")[0].files;

            if (files.length > 0) { 
                formData.append("UploadFileAMD", files[0]);
                var obj = new Object;

                obj.UPAmendID = 1;
                obj.UPMasterID = 1;
                obj.AmendmentNo = $formEl.find("#AmendmentNo").val();
                obj.AmendmentDate = $formEl.find("#AmendmentDate").val();
                obj.Remarks = $formEl.find("#Remarks").val();
                obj.ImageURL = "/Uploads/UP/" + document.getElementById('UploadFileAMD').files[0].name;
                masterData.UPAmendments.push(obj);
            }
        }

        formData.append("Childs", JSON.stringify(masterData.Childs));
        formData.append("UPAmendments", JSON.stringify(masterData.UPAmendments)); 
        formData.append("ContractNo", $formEl.find("#ContractID option:selected").text());

        const config = {
            headers: {
                'content-type': 'multipart/form-data',
                'Authorization': "Bearer " + localStorage.getItem("token")
            }
        } 
      
        //Master Value check
        //initializeValidation($formEl, validationConstraints);
        //if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        //else hideValidationErrors($formEl);

        //if (isValidMasterForm($formEl)) return;

        axios.post("/api/up-info/save", formData, config)
            .then(function (response) {
                toastr.success("Save successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    var validationConstraints = {
        ChallanNo: {
            presence: true
        },
        LReceiveNo : {
            presence: true
        },
        LReceiveDate: {
            presence: true
        }, 
        TransportTypeID: {
            presence: true
        }, 
        LoanProviderID: {
            presence: true
        },
        TransportMode: {
            presence: true
        } 
    }
})();
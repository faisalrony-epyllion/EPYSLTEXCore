(function () {
    var menuId, pageName, menuParam;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $tblChildEl, tblChildId, $formEl;
    var $pageEl;
    var pageId;
    var status;
    var masterData;
    var _yrmpChildID = 9999;
    var _isEdit = false;

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
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        $toolbarEl.find("#btnList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ALL;
            initMasterTable();
        });
        $toolbarEl.find("#btnNew").click(function (e) {
            e.preventDefault();
            _isEdit = false;
            status = statusConstants.NEW;
            $formEl.find('#FiberTypeID').prop('disabled', false);
            $formEl.find('#BlendTypeID').prop('disabled', false);
            $formEl.find('#YarnTypeID').prop('disabled', false);
            $formEl.find('#ProgramID').prop('disabled', false);
            $formEl.find('#SubProgramID').prop('disabled', false);
            $formEl.find('#CertificationID').prop('disabled', false);
            $formEl.find('#TechnicalParameterID').prop('disabled', false);
            $formEl.find('#YarnCompositionID').prop('disabled', false);
            $formEl.find('#ShadeReferenceID').prop('disabled', false);
            $formEl.find('#ManufacturingLineID').prop('disabled', false);
            $formEl.find('#ManufacturingProcessID').prop('disabled', false);
            $formEl.find('#ManufacturingSubProcessID').prop('disabled', false);
            $formEl.find('#ColorID').prop('disabled', false);
            $formEl.find('#ColorGradeID').prop('disabled', false);
            $formEl.find('#YarnCountID').prop('disabled', false);
            loadNew();
        });

        $toolbarEl.find("#btnList").click();

        $formEl.find("#btnSave").click(function () {
            save();
        });
        $formEl.find("#btnBackToList").click(function () {
            backToList();
        });
    });

    function loadNew() {
        _isNew = true;
        axios.get(`/api/yarn-rm-properties/new/`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                setFormData($formEl, masterData);

                initChild(masterData.Childs);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getDetails(id) {
        var url = `/api/yarn-rm-properties/${id}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                setFormData($formEl, masterData);

                initChild(masterData.Childs);
            })
            .catch(showResponseError);
    }

    function initMasterTable() {
        var commands = [
            { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
        ]

        var columns = [
            {
                headerText: '', commands: commands, textAlign: 'Center', width: ch_setActionCommandCellWidth(commands)
            },
            { field: 'YRMPID', headerText: 'YRMPID', visible: false },
            { field: 'YRMPChildID', headerText: 'YRMPChildID', visible: false },
            { field: 'SupplierID', headerText: 'SupplierID', visible: false },
            { field: 'SpinnerID', headerText: 'SpinnerID', visible: false },
            {
                field: 'FiberType', headerText: 'Fiber Type'
            },
            {
                field: 'BlendType', headerText: 'Blend Type'
            },
            {
                field: 'YarnType', headerText: 'Yarn Type'
            },
            {
                field: 'Program', headerText: 'Program'
            },
            {
                field: 'SubProgram', headerText: 'Sub-Program'
            },
            {
                field: 'Certification', headerText: 'Certification'
            },
            {
                field: 'TechnicalParameter', headerText: 'Technical Parameter'
            },
            {
                field: 'YarnComposition', headerText: 'Yarn Composition'
            },
            {
                field: 'ShadeReference', headerText: 'Shade Reference'
            },
            {
                field: 'ManufacturingLine', headerText: 'Manufacturing Line'
            },
            {
                field: 'ManufacturingProcess', headerText: 'Manufacturing Process'
            },
            {
                field: 'ManufacturingSubProcess', headerText: 'Manufacturing Sub-Process'
            },
            {
                field: 'Color', headerText: 'Color'
            },
            {
                field: 'ColorGrade', headerText: 'Color Grade'
            },
            {
                field: 'YarnCount', headerText: 'Yarn Count'
            },
            {
                field: 'Supplier', headerText: 'Supplier'
            },
            {
                field: 'Spinner', headerText: 'Spinner'
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: true,
            apiEndPoint: `/api/yarn-rm-properties/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'Edit') {
            _isEdit = true;
            $formEl.find('#FiberTypeID').prop('disabled', true);
            $formEl.find('#BlendTypeID').prop('disabled', true);
            $formEl.find('#YarnTypeID').prop('disabled', true);
            $formEl.find('#ProgramID').prop('disabled', true);
            $formEl.find('#SubProgramID').prop('disabled', true);
            $formEl.find('#CertificationID').prop('disabled', true);
            $formEl.find('#TechnicalParameterID').prop('disabled', true);
            $formEl.find('#YarnCompositionID').prop('disabled', true);
            $formEl.find('#ShadeReferenceID').prop('disabled', true);
            $formEl.find('#ManufacturingLineID').prop('disabled', true);
            $formEl.find('#ManufacturingProcessID').prop('disabled', true);
            $formEl.find('#ManufacturingSubProcessID').prop('disabled', true);
            $formEl.find('#ColorID').prop('disabled', true);
            $formEl.find('#ColorGradeID').prop('disabled', true);
            $formEl.find('#YarnCountID').prop('disabled', true);
            getDetails(args.rowData.YRMPID);
        }
    }

    async function initChild(data) {
        var commands = [];
        if (status != statusConstants.ALL) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
            ]
        }
        else {
            commands = [
                { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
            ]
        }
        var columns = [
            {
                headerText: 'Action', width: 100, commands: commands
            },
            { field: 'YRMPChildID', isPrimaryKey: true, visible: false },
            { field: 'YRMPID', headerText: 'YRMPID', visible: false },
            {
                field: 'SupplierID',
                headerText: 'Supplier',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.SupplierList,
                displayField: "text",
                width: 300,
                edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'SpinnerID',
                headerText: 'Spinner',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.SpinnerList,
                displayField: "text",
                width: 300,
                edit: ej2GridDropDownObj({
                })
            }
        ];

        if ($tblChildEl) $tblChildEl.destroy();
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            showColumnChooser: true,
            showDefaultToolbar: false,
            toolbar: ['Add'],
            editSettings: { allowEditing: !_isEdit, allowAdding: true, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
            actionBegin: function (args) {
                if (args.requestType === 'add') {
                    args.data.YRMPChildID = _yrmpChildID++;
                    args.rowData.YRMPChildID = args.data.YRMPChildID;
                }
            },
            columns: columns,

        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }
    function backToList() {
        $divDetailsEl.fadeOut();
        $divTblEl.fadeIn();
        initMasterTable();
    }
    function save() {
        var hasError = false;

        var data = formDataToJson($formEl.serializeArray());

        data.FiberTypeID = getDefaultValueWhenInvalidN($formEl.find('#FiberTypeID').val());
        data.BlendTypeID = getDefaultValueWhenInvalidN($formEl.find('#BlendTypeID').val());
        data.YarnTypeID = getDefaultValueWhenInvalidN($formEl.find('#YarnTypeID').val());
        data.ProgramID = getDefaultValueWhenInvalidN($formEl.find('#ProgramID').val());
        data.SubProgramID = getDefaultValueWhenInvalidN($formEl.find('#SubProgramID').val());
        data.CertificationID = getDefaultValueWhenInvalidN($formEl.find('#CertificationID').val());
        data.TechnicalParameterID = getDefaultValueWhenInvalidN($formEl.find('#TechnicalParameterID').val());
        data.YarnCompositionID = getDefaultValueWhenInvalidN($formEl.find('#YarnCompositionID').val());
        data.ShadeReferenceID = getDefaultValueWhenInvalidN($formEl.find('#ShadeReferenceID').val());
        data.ManufacturingLineID = getDefaultValueWhenInvalidN($formEl.find('#ManufacturingLineID').val());
        data.ManufacturingProcessID = getDefaultValueWhenInvalidN($formEl.find('#ManufacturingProcessID').val());
        data.ManufacturingSubProcessID = getDefaultValueWhenInvalidN($formEl.find('#ManufacturingSubProcessID').val());
        data.ColorID = getDefaultValueWhenInvalidN($formEl.find('#ColorID').val());
        data.ColorGradeID = getDefaultValueWhenInvalidN($formEl.find('#ColorGradeID').val());
        data.YarnCountID = getDefaultValueWhenInvalidN($formEl.find('#YarnCountID').val());
        data.Childs = DeepClone($tblChildEl.getCurrentViewRecords());
        var childs = DeepClone(data.Childs);
        if (data.YarnCountID == 0) {
            toastr.error('Select Yarn Count !!!');
            return false;
        }
        axios.post("/api/yarn-rm-properties/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();



//(function () {
//    var menuId, pageName, pageId;
//    var $tblMasterEl, $formEl, tblMasterId, $toolbarEl, $tblChildEl, tblChildId;
//    //var spinnerList, spinnerLists = []
//    var grid;
//    var _maxYRMPID = -999;
//    var _isEdit = false;
//    var $dialogtemplate;
//    var masterData;

//    $(function () {

//        if (!menuId) menuId = localStorage.getItem("menuId");
//        if (!pageName) pageName = localStorage.getItem("pageName");

//        pageId = pageName + "-" + menuId;
//        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
//        $toolbarEl = $(toolbarId);
//        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
//        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
//        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
//        $dialogtemplate = $(".dialogtemplate" + pageId);
//        //$toolbarEl.find("#dialogtemplateRMP").fadeIn();
//        getInitData();
//    });

//    function initMasterTable() {
//        var columns = [
//            {
//                field: 'YRMPID',
//                headerText: 'YRMPID',
//                textAlign: 'Right',
//                width: 100,
//                isPrimaryKey: true,
//                visible: false
//            },
//            { field: 'FiberTypeID', headerText: 'FiberTypeID', width: 150, visible: false },
//            { field: 'BlendTypeID', headerText: 'BlendTypeID', width: 150, visible: false },
//            { field: 'YarnTypeID', headerText: 'YarnTypeID', width: 150, visible: false },
//            { field: 'ProgramID', headerText: 'ProgramID', width: 150, visible: false },
//            { field: 'SubProgramID', headerText: 'SubProgramID', width: 150, visible: false },
//            { field: 'CertificationID', headerText: 'CertificationID', width: 150, visible: false },
//            { field: 'TechnicalParameterID', headerText: 'TechnicalParameterID', width: 150, visible: false },
//            { field: 'YarnCompositionID', headerText: 'YarnCompositionID', width: 150, visible: false },
//            { field: 'ShadeReferenceID', headerText: 'ShadeReferenceID', width: 150, visible: false },
//            { field: 'ManufacturingLineID', headerText: 'ManufacturingLineID', width: 150, visible: false },
//            { field: 'ManufacturingProcessID', headerText: 'ManufacturingProcessID', width: 150, visible: false },
//            { field: 'ManufacturingSubProcessID', headerText: 'ManufacturingSubProcessID', width: 150, visible: false },
//            { field: 'ColorID', headerText: 'ColorID', width: 150, visible: false },
//            { field: 'ColorGradeID', headerText: 'ColorGradeID', width: 150, visible: false },
//            { field: 'YarnCountID', headerText: 'YarnCountID', width: 150, visible: false },
//            {
//                field: 'FiberType',
//                headerText: 'Fiber Type',
//                width: 150
//            },
//            {
//                field: 'BlendType',
//                headerText: 'Blend Type',
//                width: 120,
//            },
//            {
//                field: 'YarnType',
//                headerText: 'Yarn Type',
//                width: 120,
//            },
//            {
//                field: 'Program',
//                headerText: 'Program',
//                width: 120,
//            },
//            {
//                field: 'SubProgram',
//                headerText: 'Sub-Program',
//                width: 120,
//            },
//            {
//                field: 'Certification',
//                headerText: 'Certification',
//                width: 120,
//            },
//            {
//                field: 'TechnicalParameter',
//                headerText: 'Technical Parameter',
//                width: 120,
//            },
//            {
//                field: 'YarnComposition',
//                headerText: 'Yarn Composition',
//                width: 120,
//            },
//            {
//                field: 'ShadeReference',
//                headerText: 'Shade Reference',
//                width: 120,
//            },
//            {
//                field: 'ManufacturingLine',
//                headerText: 'Manufacturing Line',
//                width: 120,
//            },
//            {
//                field: 'ManufacturingProcess',
//                headerText: 'Manufacturing Process',
//                width: 120,
//            },
//            {
//                field: 'ManufacturingSubProcess',
//                headerText: 'Manufacturing SubProcess',
//                width: 120,
//            },
//            {
//                field: 'Color',
//                headerText: 'Color',
//                width: 120,
//            },
//            {
//                field: 'ColorGrade',
//                headerText: 'Color Grade',
//                width: 120,
//            },
//            {
//                field: 'YarnCount',
//                headerText: 'Yarn Count',
//                width: 120,
//            },

//        ];

//        if ($tblMasterEl) $tblMasterEl.destroy();
//        $tblMasterEl = new initEJ2Grid({
//            tableId: tblMasterId,
//            autofitColumns: true,
//            apiEndPoint: `/api/yarn-rm-properties/list`,
//            columns: columns,
//            allowExcelExport: false,
//            allowPdfExport: false,
//            toolbar: ['Add'],
//            editSettings: {
//                allowEditing: false,
//                allowAdding: true,
//                allowDeleting: false,
//                mode: 'Dialog',
//                template: '#dialogtemplateRMP'
//            },
//            actionBegin: function (args) {
//                if (args.requestType === 'save') {
//                    args.data.YRMPID = getDefaultValueWhenInvalidN(args.data.YRMPID);
//                    args.data.YarnCount = getDefaultValueWhenInvalidN(args.data.YarnCount);
//                    if (args.data.YarnCount == 0) {
//                        toastr.error('Select Yarn Count!!!');
//                        args.cancel = true;
//                        return;
//                    }

//                    args.rowData = setValidPropsValue(args.data, args.rowData);
//                    args.data = setDropDownValues(masterData, args.data, args.rowData);
//                    args.rowData = args.data;

//                    if (args.data.YRMPID == 0) {
//                        args.data.YRMPID = _maxYRMPID--;
//                    }

//                    var dataObj = {
//                        YRMPID: args.data.YRMPID,
//                        FiberTypeID: args.data.FiberTypeID,
//                        BlendTypeID: args.data.BlendTypeID,
//                        YarnTypeID: args.data.YarnTypeID,
//                        ProgramID: args.data.ProgramID,
//                        SubProgramID: args.data.SubProgramID,
//                        CertificationID: args.data.CertificationID,
//                        TechnicalParameterID: args.data.TechnicalParameterID,
//                        YarnCompositionID: args.data.YarnCompositionID,
//                        ShadeReferenceID: args.data.ShadeReferenceID,
//                        ManufacturingLineID: args.data.ManufacturingLineID,
//                        ManufacturingProcessID: args.data.ManufacturingProcessID,
//                        ManufacturingSubProcessID: args.data.ManufacturingSubProcessID,
//                        ColorID: args.data.ColorID,
//                        ColorGradeID: args.data.ColorGradeID,
//                        YarnCountID: args.data.YarnCountID,
//                    };

//                    args.rowData = DeepClone(args.data);

//                    save(dataObj, args);
//                }
//                if (args.requestType === 'delete') {
//                }
//            },
//            actionComplete: function (args) {
//                _isEdit = false;
//                if ((args.requestType === 'beginEdit' || args.requestType === 'add')) {

//                    setTimeout(function () {
//                        var dialog = args.form.closest('.e-dialog'); // Get the dialog element
//                        dialog.style.width = '80%'; // Set the width
//                        dialog.style.height = '90%'; // Set the height
//                        //dialog.style.top = 'auto'; // Optional: Set dialog position (vertically centered)
//                        dialog.style.left = '43%'; // Optional: Center dialog horizontally
//                        dialog.style.transform = 'translateX(-45%)'; // Center horizontally by adjusting position

//                        var ejDialogInstance = dialog.ej2_instances[0]; // Access the EJ2 Dialog instance
//                        ejDialogInstance.dragging = false;
//                        // Set focus on a specific input field after dialog is opened
//                        $('#FiberType').focus();
//                    }, 100);

//                    //ejDropDownLoad(ej, args, masterData.FiberTypeList, "FiberType", "text", "id", "Fiber Type");
//                    ejDropDownLoad(ej, args, masterData.FiberTypeList, "FiberType", "text", "id", "");
//                    //ejDropDownLoad(ej, args, masterData.BlendTypeList, "BlendType", "text", "id", "Blend Type");
//                    ejDropDownLoad(ej, args, masterData.BlendTypeList, "BlendType", "text", "id", "");
//                    //ejDropDownLoad(ej, args, masterData.YarnTypeList, "YarnType", "text", "id", "Yarn Type");
//                    ejDropDownLoad(ej, args, masterData.YarnTypeList, "YarnType", "text", "id", "");
//                    //ejDropDownLoad(ej, args, masterData.ProgramList, "Program", "text", "id", "Program");
//                    ejDropDownLoad(ej, args, masterData.ProgramList, "Program", "text", "id", "");
//                    //ejDropDownLoad(ej, args, masterData.SubProgramList, "SubProgram", "text", "id", "Sub-Program");
//                    ejDropDownLoad(ej, args, masterData.SubProgramList, "SubProgram", "text", "id", "");
//                    //ejDropDownLoad(ej, args, masterData.CertificationList, "Certification", "text", "id", "Certification");
//                    ejDropDownLoad(ej, args, masterData.CertificationList, "Certification", "text", "id", "");
//                    //ejDropDownLoad(ej, args, masterData.TechnicalParameterList, "TechnicalParameter", "text", "id", "Technical Parameter");
//                    ejDropDownLoad(ej, args, masterData.TechnicalParameterList, "TechnicalParameter", "text", "id", "");
//                    //ejDropDownLoad(ej, args, masterData.YarnCompositionList, "YarnComposition", "text", "id", "Yarn Composition");
//                    ejDropDownLoad(ej, args, masterData.YarnCompositionList, "YarnComposition", "text", "id", "");
//                    //ejDropDownLoad(ej, args, masterData.ShadeReferenceList, "ShadeReference", "text", "id", "Shade Reference");
//                    ejDropDownLoad(ej, args, masterData.ShadeReferenceList, "ShadeReference", "text", "id", "");
//                    //ejDropDownLoad(ej, args, masterData.ManufacturingLineList, "ManufacturingLine", "text", "id", "Manufacturing Line");
//                    ejDropDownLoad(ej, args, masterData.ManufacturingLineList, "ManufacturingLine", "text", "id", "");
//                    //ejDropDownLoad(ej, args, masterData.ManufacturingProcessList, "ManufacturingProcess", "text", "id", "Manufacturing Process");
//                    ejDropDownLoad(ej, args, masterData.ManufacturingProcessList, "ManufacturingProcess", "text", "id", "");
//                    //ejDropDownLoad(ej, args, masterData.ManufacturingSubProcessList, "ManufacturingSubProcess", "text", "id", "Manufacturing Sub-Process");
//                    ejDropDownLoad(ej, args, masterData.ManufacturingSubProcessList, "ManufacturingSubProcess", "text", "id", "");
//                    //ejDropDownLoad(ej, args, masterData.ColorList, "Color", "text", "id", "Color");
//                    ejDropDownLoad(ej, args, masterData.ColorList, "Color", "text", "id", "");
//                    //ejDropDownLoad(ej, args, masterData.ColorGradeList, "ColorGrade", "text", "id", "Color Grade");
//                    ejDropDownLoad(ej, args, masterData.ColorGradeList, "ColorGrade", "text", "id", "");
//                    //ejDropDownLoad(ej, args, masterData.YarnCountList, "YarnCount", "text", "id", "Yarn Count");
//                    ejDropDownLoad(ej, args, masterData.YarnCountList, "YarnCount", "text", "id", "");
//                    //args.dialog.width = "70%";
//                }
//                if (args.requestType === 'add') {
//                    args.dialog.header = 'Add Yarn RM Properties';
//                    //initChild(masterData.Childs);
//                }
//                if (args.requestType === 'beginEdit') {
//                    _isEdit = true;
//                    args.dialog.header = 'Edit Yarn RM Properties';
//                }
//                //args.dialog.width = "60%";
//            },

//            //commandClick: handleCommands
//        });
//    }
//    function initChild(data) {

//        var columns = [
//            {
//                headerText: 'Action', width: 100, commands: [
//                    //{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
//                    //{ type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
//                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
//                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
//                ]
//            },
//            { field: 'YRMPChildID', isPrimaryKey: true, visible: false },
//            { field: 'YRMPID', headerText: 'YRMPID', visible: false },
//            {
//                field: 'SupplierID',
//                headerText: 'Supplier',
//                valueAccessor: ej2GridDisplayFormatter,
//                dataSource: masterData.SupplierList,
//                displayField: "text",
//                width: 300,
//                edit: ej2GridDropDownObj({
//                })
//            },
//            {
//                field: 'SpinnerID',
//                headerText: 'Spinner',
//                valueAccessor: ej2GridDisplayFormatter,
//                dataSource: masterData.SpinnerList,
//                displayField: "text",
//                width: 300,
//                edit: ej2GridDropDownObj({
//                })
//            },
//        ];

//        if ($tblChildEl) $tblChildEl.destroy();
//        $tblChildEl = new ej.grids.Grid({
//            dataSource: data,
//            allowResizing: true,
//            showColumnChooser: true,
//            showDefaultToolbar: false,
//            toolbar: ['Add'],
//            editSettings: { allowEditing: false, allowAdding: true, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
//            actionBegin: function (args) {

//            },
//            columns: columns,

//        });
//        $tblChildEl.refreshColumns;
//        $tblChildEl.appendTo(tblChildId);
//    }
//    function initMasterTable1() {
//        var url = "/api/yarn-rm-properties/list";
//        axios.get(url)
//            .then(function (response) {

//                if ($tblMasterEl) {
//                    $tblMasterEl.destroy();
//                }
//                ej.grids.Grid.Inject(ej.grids.Edit, ej.grids.Toolbar);
//                $tblMasterEl = new ej.grids.Grid({
//                    dataSource: response.data.Items,
//                    allowPaging: true,
//                    pageSettings: { pageSize: 10 },
//                    allowFiltering: true,
//                    filterSettings: { type: 'FilterBar' },
//                    //toolbar: ['Add', 'Edit', 'Delete'],
//                    toolbar: ['Add'],
//                    editSettings: {
//                        allowEditing: false,
//                        allowAdding: true,
//                        allowDeleting: false,
//                        mode: 'Dialog',
//                        template: '#dialogtemplateRMP'
//                    },
//                    columns: [
//                        {
//                            field: 'YRMPID',
//                            headerText: 'YRMPID',
//                            textAlign: 'Right',
//                            width: 100,
//                            isPrimaryKey: true,
//                            visible: false
//                        },
//                        { field: 'FiberTypeID', headerText: 'FiberTypeID', width: 150, visible: false },
//                        { field: 'BlendTypeID', headerText: 'BlendTypeID', width: 150, visible: false },
//                        { field: 'YarnTypeID', headerText: 'YarnTypeID', width: 150, visible: false },
//                        { field: 'ProgramID', headerText: 'ProgramID', width: 150, visible: false },
//                        { field: 'SubProgramID', headerText: 'SubProgramID', width: 150, visible: false },
//                        { field: 'CertificationID', headerText: 'CertificationID', width: 150, visible: false },
//                        { field: 'TechnicalParameterID', headerText: 'TechnicalParameterID', width: 150, visible: false },
//                        { field: 'YarnCompositionID', headerText: 'YarnCompositionID', width: 150, visible: false },
//                        { field: 'ShadeReferenceID', headerText: 'ShadeReferenceID', width: 150, visible: false },
//                        { field: 'ManufacturingLineID', headerText: 'ManufacturingLineID', width: 150, visible: false },
//                        { field: 'ManufacturingProcessID', headerText: 'ManufacturingProcessID', width: 150, visible: false },
//                        { field: 'ManufacturingSubProcessID', headerText: 'ManufacturingSubProcessID', width: 150, visible: false },
//                        { field: 'ColorID', headerText: 'ColorID', width: 150, visible: false },
//                        { field: 'ColorGradeID', headerText: 'ColorGradeID', width: 150, visible: false },
//                        { field: 'YarnCountID', headerText: 'YarnCountID', width: 150, visible: false },
//                        {
//                            field: 'FiberType',
//                            headerText: 'Fiber Type',
//                            width: 150
//                        },
//                        {
//                            field: 'BlendType',
//                            headerText: 'Blend Type',
//                            width: 120,
//                        },
//                        {
//                            field: 'YarnType',
//                            headerText: 'Yarn Type',
//                            width: 120,
//                        },
//                        {
//                            field: 'Program',
//                            headerText: 'Program',
//                            width: 120,
//                        },
//                        {
//                            field: 'SubProgram',
//                            headerText: 'Sub-Program',
//                            width: 120,
//                        },
//                        {
//                            field: 'Certification',
//                            headerText: 'Certification',
//                            width: 120,
//                        },
//                        {
//                            field: 'TechnicalParameter',
//                            headerText: 'Technical Parameter',
//                            width: 120,
//                        },
//                        {
//                            field: 'YarnComposition',
//                            headerText: 'Yarn Composition',
//                            width: 120,
//                        },
//                        {
//                            field: 'ShadeReference',
//                            headerText: 'Shade Reference',
//                            width: 120,
//                        },
//                        {
//                            field: 'ManufacturingLine',
//                            headerText: 'Manufacturing Line',
//                            width: 120,
//                        },
//                        {
//                            field: 'ManufacturingProcess',
//                            headerText: 'Manufacturing Process',
//                            width: 120,
//                        },
//                        {
//                            field: 'ManufacturingSubProcess',
//                            headerText: 'Manufacturing SubProcess',
//                            width: 120,
//                        },
//                        {
//                            field: 'Color',
//                            headerText: 'Color',
//                            width: 120,
//                        },
//                        {
//                            field: 'ColorGrade',
//                            headerText: 'Color Grade',
//                            width: 120,
//                        },
//                        {
//                            field: 'YarnCount',
//                            headerText: 'Yarn Count',
//                            width: 120,
//                        },

//                    ],
//                    height: 500,
//                    actionBegin: function (args) {
//                        if (args.requestType === 'save') {

//                            args.data.YRMPID = getDefaultValueWhenInvalidN(args.data.YRMPID);
//                            args.rowData = setValidPropsValue(args.data, args.rowData);
//                            args.data = setDropDownValues(masterData, args.data, args.rowData);
//                            args.rowData = args.data;

//                            if (args.data.YRMPID == 0) {
//                                args.data.YRMPID = _maxYRMPID--;
//                            }
//                            var allData = $tblMasterEl.dataSource;

//                            var isExist = false;
//                            var list = allData.filter(item =>
//                                item.FiberTypeID === args.data.FiberTypeID
//                                && item.BlendTypeID === args.data.BlendTypeID
//                                && item.YarnTypeID === args.data.YarnTypeID
//                                && item.ProgramID === args.data.ProgramID
//                                && item.SubProgramID === args.data.SubProgramID
//                                && item.CertificationID === args.data.CertificationID
//                                && item.TechnicalParameterID === args.data.TechnicalParameterID
//                                && item.YarnCompositionID === args.data.YarnCompositionID
//                                && item.ShadeReferenceID === args.data.ShadeReferenceID
//                                && item.ManufacturingLineID === args.data.ManufacturingLineID
//                                && item.ManufacturingProcessID === args.data.ManufacturingProcessID
//                                && item.ManufacturingSubProcessID === args.data.ManufacturingSubProcessID
//                                && item.ColorID === args.data.ColorID
//                                && item.ColorGradeID === args.data.ColorGradeID
//                                && item.YarnCountID === args.data.YarnCountID
//                            );

//                            if (list.length > 0 && _isEdit) {
//                                list = list.filter(x => x.YRMPID != args.data.YRMPID);
//                            }
//                            if (list.length > 0) isExist = true;

//                            if (isExist) {
//                                toastr.error("Duplicate data found!!!");
//                                args.cancel = true;
//                                return;
//                            }
//                            //if (getDefaultValueWhenInvalidN(args.data.YarnCountID) == 0) {
//                            //    toastr.error("Please Select Yarn Count!!!");
//                            //    args.cancel = true;
//                            //    return;
//                            //}

//                            var dataObj = {
//                                YRMPID: args.data.YRMPID,
//                                FiberTypeID: args.data.FiberTypeID,
//                                BlendTypeID: args.data.BlendTypeID,
//                                YarnTypeID: args.data.YarnTypeID,
//                                ProgramID: args.data.ProgramID,
//                                SubProgramID: args.data.SubProgramID,
//                                CertificationID: args.data.CertificationID,
//                                TechnicalParameterID: args.data.TechnicalParameterID,
//                                YarnCompositionID: args.data.YarnCompositionID,
//                                ShadeReferenceID: args.data.ShadeReferenceID,
//                                ManufacturingLineID: args.data.ManufacturingLineID,
//                                ManufacturingProcessID: args.data.ManufacturingProcessID,
//                                ManufacturingSubProcessID: args.data.ManufacturingSubProcessID,
//                                ColorID: args.data.ColorID,
//                                ColorGradeID: args.data.ColorGradeID,
//                                YarnCountID: args.data.YarnCountID,
//                            };

//                            args.rowData = DeepClone(args.data);

//                            if (!save(dataObj)) {
//                                args.cancel = true;
//                                return;
//                            };
//                        }
//                        if (args.requestType === 'delete') {


//                        }
//                    },
//                    actionComplete: function (args) {

//                        _isEdit = false;
//                        if ((args.requestType === 'beginEdit' || args.requestType === 'add')) {
//                            //(args.form.elements.namedItem('FiberType')).focus();
//                            ejDropDownLoad(ej, args, masterData.FiberTypeList, "FiberType", "text", "id", "Fiber Type");
//                            ejDropDownLoad(ej, args, masterData.BlendTypeList, "BlendType", "text", "id", "Blend Type");
//                            ejDropDownLoad(ej, args, masterData.YarnTypeList, "YarnType", "text", "id", "Yarn Type");
//                            ejDropDownLoad(ej, args, masterData.ProgramList, "Program", "text", "id", "Program");
//                            ejDropDownLoad(ej, args, masterData.SubProgramList, "SubProgram", "text", "id", "Sub-Program");
//                            ejDropDownLoad(ej, args, masterData.CertificationList, "Certification", "text", "id", "Certification");
//                            ejDropDownLoad(ej, args, masterData.TechnicalParameterList, "TechnicalParameter", "text", "id", "Technical Parameter");
//                            ejDropDownLoad(ej, args, masterData.YarnCompositionList, "YarnComposition", "text", "id", "Yarn Composition");
//                            ejDropDownLoad(ej, args, masterData.ShadeReferenceList, "ShadeReference", "text", "id", "Shade Reference");
//                            ejDropDownLoad(ej, args, masterData.ManufacturingLineList, "ManufacturingLine", "text", "id", "Manufacturing Line");
//                            ejDropDownLoad(ej, args, masterData.ManufacturingProcessList, "ManufacturingProcess", "text", "id", "Manufacturing Process");
//                            ejDropDownLoad(ej, args, masterData.ManufacturingSubProcessList, "ManufacturingSubProcess", "text", "id", "Manufacturing Sub-Process");
//                            ejDropDownLoad(ej, args, masterData.ColorList, "Color", "text", "id", "Color");
//                            ejDropDownLoad(ej, args, masterData.ColorGradeList, "ColorGrade", "text", "id", "Color Grade");
//                            ejDropDownLoad(ej, args, masterData.YarnCountList, "YarnCount", "text", "id", "Yarn Count");
//                            args.dialog.header = 'Add Yarn RM Properties';
//                        }
//                        if (args.requestType === 'beginEdit') {
//                            _isEdit = true;
//                            args.dialog.header = 'Edit Yarn RM Properties';
//                        }
//                        //var select2Element = args.form.elements.namedItem('FiberType');
//                        //select2Element.innerHTML = '<option value="" disabled selected>Select Fiber Type</option>';

//                        //$(select2Element).select2({
//                        //    placeholder: "Select Fiber Type",
//                        //    width: '100%'
//                        //});
//                    }
//                });
//                $tblMasterEl.appendTo('#Grid');
//            })
//            .catch(showResponseError)
//    }
//    function getInitData() {

//        var url = "/api/yarn-rm-properties/GetMaster";
//        axios.get(url)
//            .then(function (response) {
//                masterData = response.data;

//                setFormData($formEl, masterData);
//                initMasterTable();
//            })
//            .catch(showResponseError)
//    }
//    function save(dataObj, args) {
//        var data = formElToJson($formEl);
//        data.YRMPID = getDefaultValueWhenInvalidN(dataObj.YRMPID);
//        data.FiberTypeID = getDefaultValueWhenInvalidN(dataObj.FiberTypeID);
//        data.BlendTypeID = getDefaultValueWhenInvalidN(dataObj.BlendTypeID);
//        data.YarnTypeID = getDefaultValueWhenInvalidN(dataObj.YarnTypeID);
//        data.ProgramID = getDefaultValueWhenInvalidN(dataObj.ProgramID);
//        data.SubProgramID = getDefaultValueWhenInvalidN(dataObj.SubProgramID);
//        data.CertificationID = getDefaultValueWhenInvalidN(dataObj.CertificationID);
//        data.TechnicalParameterID = getDefaultValueWhenInvalidN(dataObj.TechnicalParameterID);
//        data.YarnCompositionID = getDefaultValueWhenInvalidN(dataObj.YarnCompositionID);
//        data.ShadeReferenceID = getDefaultValueWhenInvalidN(dataObj.ShadeReferenceID);
//        data.ManufacturingLineID = getDefaultValueWhenInvalidN(dataObj.ManufacturingLineID);
//        data.ManufacturingProcessID = getDefaultValueWhenInvalidN(dataObj.ManufacturingProcessID);
//        data.ManufacturingSubProcessID = getDefaultValueWhenInvalidN(dataObj.ManufacturingSubProcessID);
//        data.ColorID = getDefaultValueWhenInvalidN(dataObj.ColorID);
//        data.ColorGradeID = getDefaultValueWhenInvalidN(dataObj.ColorGradeID);
//        data.YarnCountID = getDefaultValueWhenInvalidN(dataObj.YarnCountID);

//        axios.post("/api/yarn-rm-properties/save", data)
//            .then(function () {
//                toastr.success("Saved successfully.");
//                $tblMasterEl.refresh();
//            })
//            .catch(error => {
//                if (error.response.data.Message === undefined) {
//                    toastr.error(error.response.data);
//                } else {
//                    toastr.error('Error message:', error.response.data.Message);
//                }
//                args.cancel = true;
//            });
//    }

//})();
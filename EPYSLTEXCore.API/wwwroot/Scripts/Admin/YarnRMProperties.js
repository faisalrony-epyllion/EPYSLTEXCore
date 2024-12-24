(function () {
    var menuId, pageName;
    var $tblMasterEl, $formEl, tblMasterId;
    //var spinnerList, spinnerLists = []
    var grid;
    var _maxYRMPID = -999;
    var _isEdit = false;

    $(function () {

        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;

        getInitData();
    });

    function initMasterTable() {
        var columns = [
            {
                field: 'YRMPID',
                headerText: 'YRMPID',
                textAlign: 'Right',
                width: 100,
                isPrimaryKey: true,
                visible: false
            },
            { field: 'FiberTypeID', headerText: 'FiberTypeID', width: 150, visible: false },
            { field: 'BlendTypeID', headerText: 'BlendTypeID', width: 150, visible: false },
            { field: 'YarnTypeID', headerText: 'YarnTypeID', width: 150, visible: false },
            { field: 'ProgramID', headerText: 'ProgramID', width: 150, visible: false },
            { field: 'SubProgramID', headerText: 'SubProgramID', width: 150, visible: false },
            { field: 'CertificationID', headerText: 'CertificationID', width: 150, visible: false },
            { field: 'TechnicalParameterID', headerText: 'TechnicalParameterID', width: 150, visible: false },
            { field: 'YarnCompositionID', headerText: 'YarnCompositionID', width: 150, visible: false },
            { field: 'ShadeReferenceID', headerText: 'ShadeReferenceID', width: 150, visible: false },
            { field: 'ManufacturingLineID', headerText: 'ManufacturingLineID', width: 150, visible: false },
            { field: 'ManufacturingProcessID', headerText: 'ManufacturingProcessID', width: 150, visible: false },
            { field: 'ManufacturingSubProcessID', headerText: 'ManufacturingSubProcessID', width: 150, visible: false },
            { field: 'ColorID', headerText: 'ColorID', width: 150, visible: false },
            { field: 'ColorGradeID', headerText: 'ColorGradeID', width: 150, visible: false },
            { field: 'YarnCountID', headerText: 'YarnCountID', width: 150, visible: false },
            {
                field: 'FiberType',
                headerText: 'Fiber Type',
                width: 150
            },
            {
                field: 'BlendType',
                headerText: 'Blend Type',
                width: 120,
            },
            {
                field: 'YarnType',
                headerText: 'Yarn Type',
                width: 120,
            },
            {
                field: 'Program',
                headerText: 'Program',
                width: 120,
            },
            {
                field: 'SubProgram',
                headerText: 'Sub-Program',
                width: 120,
            },
            {
                field: 'Certification',
                headerText: 'Certification',
                width: 120,
            },
            {
                field: 'TechnicalParameter',
                headerText: 'Technical Parameter',
                width: 120,
            },
            {
                field: 'YarnComposition',
                headerText: 'Yarn Composition',
                width: 120,
            },
            {
                field: 'ShadeReference',
                headerText: 'Shade Reference',
                width: 120,
            },
            {
                field: 'ManufacturingLine',
                headerText: 'Manufacturing Line',
                width: 120,
            },
            {
                field: 'ManufacturingProcess',
                headerText: 'Manufacturing Process',
                width: 120,
            },
            {
                field: 'ManufacturingSubProcess',
                headerText: 'Manufacturing SubProcess',
                width: 120,
            },
            {
                field: 'Color',
                headerText: 'Color',
                width: 120,
            },
            {
                field: 'ColorGrade',
                headerText: 'Color Grade',
                width: 120,
            },
            {
                field: 'YarnCount',
                headerText: 'Yarn Count',
                width: 120,
            },

        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: true,
            apiEndPoint: `/api/yarn-rm-properties/list`,
            columns: columns,
            allowExcelExport: false,
            allowPdfExport: false,
            toolbar: ['Add'],
            editSettings: {
                allowEditing: false,
                allowAdding: true,
                allowDeleting: false,
                mode: 'Dialog',
                template: '#dialogtemplate'
            },
            actionBegin: function (args) {
                if (args.requestType === 'save') {
                    args.data.YRMPID = getDefaultValueWhenInvalidN(args.data.YRMPID);
                    args.data.YarnCount = getDefaultValueWhenInvalidN(args.data.YarnCount);
                    if (args.data.YarnCount == 0) {
                        toastr.error('Select Yarn Count!!!');
                        args.cancel = true;
                        return;
                    }

                    args.rowData = setValidPropsValue(args.data, args.rowData);
                    args.data = setDropDownValues(masterData, args.data, args.rowData);
                    args.rowData = args.data;

                    if (args.data.YRMPID == 0) {
                        args.data.YRMPID = _maxYRMPID--;
                    }

                    var dataObj = {
                        YRMPID: args.data.YRMPID,
                        FiberTypeID: args.data.FiberTypeID,
                        BlendTypeID: args.data.BlendTypeID,
                        YarnTypeID: args.data.YarnTypeID,
                        ProgramID: args.data.ProgramID,
                        SubProgramID: args.data.SubProgramID,
                        CertificationID: args.data.CertificationID,
                        TechnicalParameterID: args.data.TechnicalParameterID,
                        YarnCompositionID: args.data.YarnCompositionID,
                        ShadeReferenceID: args.data.ShadeReferenceID,
                        ManufacturingLineID: args.data.ManufacturingLineID,
                        ManufacturingProcessID: args.data.ManufacturingProcessID,
                        ManufacturingSubProcessID: args.data.ManufacturingSubProcessID,
                        ColorID: args.data.ColorID,
                        ColorGradeID: args.data.ColorGradeID,
                        YarnCountID: args.data.YarnCountID,
                    };

                    args.rowData = DeepClone(args.data);

                    save(dataObj, args);
                }
                if (args.requestType === 'delete') {
                }
            },
            actionComplete: function (args) {
                _isEdit = false;
                if ((args.requestType === 'beginEdit' || args.requestType === 'add')) {

                    setTimeout(function () {
                        var dialog = args.form.closest('.e-dialog'); // Get the dialog element
                        dialog.style.width = '70%'; // Set the width
                        dialog.style.height = '85%'; // Set the height
                        //dialog.style.top = 'auto'; // Optional: Set dialog position (vertically centered)
                        dialog.style.left = '40%'; // Optional: Center dialog horizontally
                        dialog.style.transform = 'translateX(-45%)'; // Center horizontally by adjusting position

                        var ejDialogInstance = dialog.ej2_instances[0]; // Access the EJ2 Dialog instance
                        ejDialogInstance.dragging = false;
                        // Set focus on a specific input field after dialog is opened
                        document.getElementById('FiberType').focus();
                    }, 100);

                    ejDropDownLoad(ej, args, masterData.FiberTypeList, "FiberType", "text", "id", "Fiber Type");
                    ejDropDownLoad(ej, args, masterData.BlendTypeList, "BlendType", "text", "id", "Blend Type");
                    ejDropDownLoad(ej, args, masterData.YarnTypeList, "YarnType", "text", "id", "Yarn Type");
                    ejDropDownLoad(ej, args, masterData.ProgramList, "Program", "text", "id", "Program");
                    ejDropDownLoad(ej, args, masterData.SubProgramList, "SubProgram", "text", "id", "Sub-Program");
                    ejDropDownLoad(ej, args, masterData.CertificationList, "Certification", "text", "id", "Certification");
                    ejDropDownLoad(ej, args, masterData.TechnicalParameterList, "TechnicalParameter", "text", "id", "Technical Parameter");
                    ejDropDownLoad(ej, args, masterData.YarnCompositionList, "YarnComposition", "text", "id", "Yarn Composition");
                    ejDropDownLoad(ej, args, masterData.ShadeReferenceList, "ShadeReference", "text", "id", "Shade Reference");
                    ejDropDownLoad(ej, args, masterData.ManufacturingLineList, "ManufacturingLine", "text", "id", "Manufacturing Line");
                    ejDropDownLoad(ej, args, masterData.ManufacturingProcessList, "ManufacturingProcess", "text", "id", "Manufacturing Process");
                    ejDropDownLoad(ej, args, masterData.ManufacturingSubProcessList, "ManufacturingSubProcess", "text", "id", "Manufacturing Sub-Process");
                    ejDropDownLoad(ej, args, masterData.ColorList, "Color", "text", "id", "Color");
                    ejDropDownLoad(ej, args, masterData.ColorGradeList, "ColorGrade", "text", "id", "Color Grade");
                    ejDropDownLoad(ej, args, masterData.YarnCountList, "YarnCount", "text", "id", "Yarn Count");
                    args.dialog.header = 'Add Yarn RM Properties';
                    //args.dialog.width = "70%";
                }
                if (args.requestType === 'beginEdit') {
                    _isEdit = true;
                    args.dialog.header = 'Edit Yarn RM Properties';
                }
                //args.dialog.width = "60%";
            },
            
            //commandClick: handleCommands
        });
    }

    function initMasterTable1() {
        var url = "/api/yarn-rm-properties/list";
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
                    //toolbar: ['Add', 'Edit', 'Delete'],
                    toolbar: ['Add'],
                    editSettings: {
                        allowEditing: false,
                        allowAdding: true,
                        allowDeleting: false,
                        mode: 'Dialog',
                        template: '#dialogtemplate'
                    },
                    columns: [
                        {
                            field: 'YRMPID',
                            headerText: 'YRMPID',
                            textAlign: 'Right',
                            width: 100,
                            isPrimaryKey: true,
                            visible: false
                        },
                        { field: 'FiberTypeID', headerText: 'FiberTypeID', width: 150, visible: false },
                        { field: 'BlendTypeID', headerText: 'BlendTypeID', width: 150, visible: false },
                        { field: 'YarnTypeID', headerText: 'YarnTypeID', width: 150, visible: false },
                        { field: 'ProgramID', headerText: 'ProgramID', width: 150, visible: false },
                        { field: 'SubProgramID', headerText: 'SubProgramID', width: 150, visible: false },
                        { field: 'CertificationID', headerText: 'CertificationID', width: 150, visible: false },
                        { field: 'TechnicalParameterID', headerText: 'TechnicalParameterID', width: 150, visible: false },
                        { field: 'YarnCompositionID', headerText: 'YarnCompositionID', width: 150, visible: false },
                        { field: 'ShadeReferenceID', headerText: 'ShadeReferenceID', width: 150, visible: false },
                        { field: 'ManufacturingLineID', headerText: 'ManufacturingLineID', width: 150, visible: false },
                        { field: 'ManufacturingProcessID', headerText: 'ManufacturingProcessID', width: 150, visible: false },
                        { field: 'ManufacturingSubProcessID', headerText: 'ManufacturingSubProcessID', width: 150, visible: false },
                        { field: 'ColorID', headerText: 'ColorID', width: 150, visible: false },
                        { field: 'ColorGradeID', headerText: 'ColorGradeID', width: 150, visible: false },
                        { field: 'YarnCountID', headerText: 'YarnCountID', width: 150, visible: false },
                        {
                            field: 'FiberType',
                            headerText: 'Fiber Type',
                            width: 150
                        },
                        {
                            field: 'BlendType',
                            headerText: 'Blend Type',
                            width: 120,
                        },
                        {
                            field: 'YarnType',
                            headerText: 'Yarn Type',
                            width: 120,
                        },
                        {
                            field: 'Program',
                            headerText: 'Program',
                            width: 120,
                        },
                        {
                            field: 'SubProgram',
                            headerText: 'Sub-Program',
                            width: 120,
                        },
                        {
                            field: 'Certification',
                            headerText: 'Certification',
                            width: 120,
                        },
                        {
                            field: 'TechnicalParameter',
                            headerText: 'Technical Parameter',
                            width: 120,
                        },
                        {
                            field: 'YarnComposition',
                            headerText: 'Yarn Composition',
                            width: 120,
                        },
                        {
                            field: 'ShadeReference',
                            headerText: 'Shade Reference',
                            width: 120,
                        },
                        {
                            field: 'ManufacturingLine',
                            headerText: 'Manufacturing Line',
                            width: 120,
                        },
                        {
                            field: 'ManufacturingProcess',
                            headerText: 'Manufacturing Process',
                            width: 120,
                        },
                        {
                            field: 'ManufacturingSubProcess',
                            headerText: 'Manufacturing SubProcess',
                            width: 120,
                        },
                        {
                            field: 'Color',
                            headerText: 'Color',
                            width: 120,
                        },
                        {
                            field: 'ColorGrade',
                            headerText: 'Color Grade',
                            width: 120,
                        },
                        {
                            field: 'YarnCount',
                            headerText: 'Yarn Count',
                            width: 120,
                        },

                    ],
                    height: 500,
                    actionBegin: function (args) {
                        if (args.requestType === 'save') {

                            args.data.YRMPID = getDefaultValueWhenInvalidN(args.data.YRMPID);
                            args.rowData = setValidPropsValue(args.data, args.rowData);
                            args.data = setDropDownValues(masterData, args.data, args.rowData);
                            args.rowData = args.data;

                            if (args.data.YRMPID == 0) {
                                args.data.YRMPID = _maxYRMPID--;
                            }
                            var allData = $tblMasterEl.dataSource;

                            var isExist = false;
                            var list = allData.filter(item =>
                                item.FiberTypeID === args.data.FiberTypeID
                                && item.BlendTypeID === args.data.BlendTypeID
                                && item.YarnTypeID === args.data.YarnTypeID
                                && item.ProgramID === args.data.ProgramID
                                && item.SubProgramID === args.data.SubProgramID
                                && item.CertificationID === args.data.CertificationID
                                && item.TechnicalParameterID === args.data.TechnicalParameterID
                                && item.YarnCompositionID === args.data.YarnCompositionID
                                && item.ShadeReferenceID === args.data.ShadeReferenceID
                                && item.ManufacturingLineID === args.data.ManufacturingLineID
                                && item.ManufacturingProcessID === args.data.ManufacturingProcessID
                                && item.ManufacturingSubProcessID === args.data.ManufacturingSubProcessID
                                && item.ColorID === args.data.ColorID
                                && item.ColorGradeID === args.data.ColorGradeID
                                && item.YarnCountID === args.data.YarnCountID
                            );

                            if (list.length > 0 && _isEdit) {
                                list = list.filter(x => x.YRMPID != args.data.YRMPID);
                            }
                            if (list.length > 0) isExist = true;

                            if (isExist) {
                                toastr.error("Duplicate data found!!!");
                                args.cancel = true;
                                return;
                            }
                            //if (getDefaultValueWhenInvalidN(args.data.YarnCountID) == 0) {
                            //    toastr.error("Please Select Yarn Count!!!");
                            //    args.cancel = true;
                            //    return;
                            //}

                            var dataObj = {
                                YRMPID: args.data.YRMPID,
                                FiberTypeID: args.data.FiberTypeID,
                                BlendTypeID: args.data.BlendTypeID,
                                YarnTypeID: args.data.YarnTypeID,
                                ProgramID: args.data.ProgramID,
                                SubProgramID: args.data.SubProgramID,
                                CertificationID: args.data.CertificationID,
                                TechnicalParameterID: args.data.TechnicalParameterID,
                                YarnCompositionID: args.data.YarnCompositionID,
                                ShadeReferenceID: args.data.ShadeReferenceID,
                                ManufacturingLineID: args.data.ManufacturingLineID,
                                ManufacturingProcessID: args.data.ManufacturingProcessID,
                                ManufacturingSubProcessID: args.data.ManufacturingSubProcessID,
                                ColorID: args.data.ColorID,
                                ColorGradeID: args.data.ColorGradeID,
                                YarnCountID: args.data.YarnCountID,
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
                        if ((args.requestType === 'beginEdit' || args.requestType === 'add')) {
                            //(args.form.elements.namedItem('FiberType')).focus();
                            ejDropDownLoad(ej, args, masterData.FiberTypeList, "FiberType", "text", "id", "Fiber Type");
                            ejDropDownLoad(ej, args, masterData.BlendTypeList, "BlendType", "text", "id", "Blend Type");
                            ejDropDownLoad(ej, args, masterData.YarnTypeList, "YarnType", "text", "id", "Yarn Type");
                            ejDropDownLoad(ej, args, masterData.ProgramList, "Program", "text", "id", "Program");
                            ejDropDownLoad(ej, args, masterData.SubProgramList, "SubProgram", "text", "id", "Sub-Program");
                            ejDropDownLoad(ej, args, masterData.CertificationList, "Certification", "text", "id", "Certification");
                            ejDropDownLoad(ej, args, masterData.TechnicalParameterList, "TechnicalParameter", "text", "id", "Technical Parameter");
                            ejDropDownLoad(ej, args, masterData.YarnCompositionList, "YarnComposition", "text", "id", "Yarn Composition");
                            ejDropDownLoad(ej, args, masterData.ShadeReferenceList, "ShadeReference", "text", "id", "Shade Reference");
                            ejDropDownLoad(ej, args, masterData.ManufacturingLineList, "ManufacturingLine", "text", "id", "Manufacturing Line");
                            ejDropDownLoad(ej, args, masterData.ManufacturingProcessList, "ManufacturingProcess", "text", "id", "Manufacturing Process");
                            ejDropDownLoad(ej, args, masterData.ManufacturingSubProcessList, "ManufacturingSubProcess", "text", "id", "Manufacturing Sub-Process");
                            ejDropDownLoad(ej, args, masterData.ColorList, "Color", "text", "id", "Color");
                            ejDropDownLoad(ej, args, masterData.ColorGradeList, "ColorGrade", "text", "id", "Color Grade");
                            ejDropDownLoad(ej, args, masterData.YarnCountList, "YarnCount", "text", "id", "Yarn Count");
                            args.dialog.header = 'Add Yarn RM Properties';
                        }
                        if (args.requestType === 'beginEdit') {
                            _isEdit = true;
                            args.dialog.header = 'Edit Yarn RM Properties';
                        }
                        //var select2Element = args.form.elements.namedItem('FiberType');
                        //select2Element.innerHTML = '<option value="" disabled selected>Select Fiber Type</option>';

                        //$(select2Element).select2({
                        //    placeholder: "Select Fiber Type",
                        //    width: '100%'
                        //});
                    }
                });
                $tblMasterEl.appendTo('#Grid');
            })
            .catch(showResponseError)
    }
    function getInitData() {

        var url = "/api/yarn-rm-properties/GetMaster";
        axios.get(url)
            .then(function (response) {
                masterData = response.data;

                setFormData($formEl, masterData);
                initMasterTable();
            })
            .catch(showResponseError)
    }
    function save(dataObj, args) {
        var data = formElToJson($formEl);
        data.YRMPID = getDefaultValueWhenInvalidN(dataObj.YRMPID);
        data.FiberTypeID = getDefaultValueWhenInvalidN(dataObj.FiberTypeID);
        data.BlendTypeID = getDefaultValueWhenInvalidN(dataObj.BlendTypeID);
        data.YarnTypeID = getDefaultValueWhenInvalidN(dataObj.YarnTypeID);
        data.ProgramID = getDefaultValueWhenInvalidN(dataObj.ProgramID);
        data.SubProgramID = getDefaultValueWhenInvalidN(dataObj.SubProgramID);
        data.CertificationID = getDefaultValueWhenInvalidN(dataObj.CertificationID);
        data.TechnicalParameterID = getDefaultValueWhenInvalidN(dataObj.TechnicalParameterID);
        data.YarnCompositionID = getDefaultValueWhenInvalidN(dataObj.YarnCompositionID);
        data.ShadeReferenceID = getDefaultValueWhenInvalidN(dataObj.ShadeReferenceID);
        data.ManufacturingLineID = getDefaultValueWhenInvalidN(dataObj.ManufacturingLineID);
        data.ManufacturingProcessID = getDefaultValueWhenInvalidN(dataObj.ManufacturingProcessID);
        data.ManufacturingSubProcessID = getDefaultValueWhenInvalidN(dataObj.ManufacturingSubProcessID);
        data.ColorID = getDefaultValueWhenInvalidN(dataObj.ColorID);
        data.ColorGradeID = getDefaultValueWhenInvalidN(dataObj.ColorGradeID);
        data.YarnCountID = getDefaultValueWhenInvalidN(dataObj.YarnCountID);

        axios.post("/api/yarn-rm-properties/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                $tblMasterEl.refresh();
            })
            .catch(error => {
                if (error.response) {
                    toastr.error(error.response.data.Message);
                } else {
                    toastr.error('Error message:', error.response.data.Message);
                }
                args.cancel = true;
            });
    }

})();
(function () {
    var menuId, pageName;
    var $tblMasterEl, $formEl, tblMasterId;
    //var spinnerList, spinnerLists = []
    var grid;
    var _maxYarnPropertiesMappingID = -999;
    var _isEdit = false;

    $(function () {
        debugger;
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        //$tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;

        getInitData();
    });
    function initMasterTable() {
        var url = "/api/yarn-properties-mapping/list";
        axios.get(url)
            .then(function (response) {
                debugger;
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
                            field: 'YarnPropertiesMappingID',
                            headerText: 'YarnPropertiesMappingID',
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
                            debugger;
                            console.log(args.data);

                            args.data.YarnPropertiesMappingID = getDefaultValueWhenInvalidN(args.data.YarnPropertiesMappingID);
                            args.rowData = setValidPropsValue(args.data, args.rowData);
                            args.data = setDropDownValues(masterData, args.data, args.rowData);
                            args.rowData = args.data;

                            if (args.data.YarnPropertiesMappingID == 0) {
                                args.data.YarnPropertiesMappingID = _maxYarnPropertiesMappingID--;
                            }
                            var allData = $tblMasterEl.dataSource;
                            console.log(allData);

                            debugger;
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
                                list = list.filter(x => x.YarnPropertiesMappingID != args.data.YarnPropertiesMappingID);
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
                                YarnPropertiesMappingID: args.data.YarnPropertiesMappingID,
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
                            debugger;

                        }
                    },
                    actionComplete: function (args) {
                        debugger;
                        _isEdit = false;
                        if ((args.requestType === 'beginEdit' || args.requestType === 'add')) {
                            console.log(args);
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

        var url = "/api/yarn-properties-mapping/GetMaster";
        axios.get(url)
            .then(function (response) {
                masterData = response.data;

                setFormData($formEl, masterData);
                initMasterTable();
            })
            .catch(showResponseError)
    }
    function save(dataObj) {
        let returnFlag = false;
        debugger;
        var data = formElToJson($formEl);
        data.YarnPropertiesMappingID = dataObj.YarnPropertiesMappingID;
        data.FiberTypeID = dataObj.FiberTypeID;
        data.BlendTypeID = dataObj.BlendTypeID;
        data.YarnTypeID = dataObj.YarnTypeID;
        data.ProgramID = dataObj.ProgramID;
        data.SubProgramID = dataObj.SubProgramID;
        data.CertificationID = dataObj.CertificationID;
        data.TechnicalParameterID = dataObj.TechnicalParameterID;
        data.YarnCompositionID = dataObj.YarnCompositionID;
        data.ShadeReferenceID = dataObj.ShadeReferenceID;
        data.ManufacturingLineID = dataObj.ManufacturingLineID;
        data.ManufacturingProcessID = dataObj.ManufacturingProcessID;
        data.ManufacturingSubProcessID = dataObj.ManufacturingSubProcessID;
        data.ColorID = dataObj.ColorID;
        data.ColorGradeID = dataObj.ColorGradeID;
        data.YarnCountID = dataObj.YarnCountID;
        console.log(data);
        axios.post("/api/yarn-properties-mapping/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                initMasterTable();
                //$tblMasterEl.refresh();
            })
            .catch(showResponseError);
        debugger;
        returnFlag = false;
    }

})();
(function () {
    var menuId, pageName;
    var $tblMasterEl, $tblYPSProgramEl, $tblYPSTechnicalParameterEl, $formEl;
    var $fiberTypeEl;
    var fiberTypeId, fiberType, isBlendedOrColorMelange, isNotBlendedOrColorMelange;

    var productSetupMaster;

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $fiberTypeEl = $formEl.find("#FiberTypeID");
        $tblYPSProgramEl = $formEl.find("#tblYPSProgram");
        $tblYPSTechnicalParameterEl = $formEl.find("#tblYPSTechnicalParameter");

        getInitData();

        $formEl.find("#btnSave").click(save);

        $formEl.find("#btnAddItem").click(addNewItem);
        $formEl.find("#btnAddProgramInformation").click(addNewProgramInformation);
        $formEl.find("#btnAddYPSTechnicalParameter").click(addNewTechnicalParameter);
    });

    function addNewItem(e) {
        e.preventDefault();
        var newChildData = {
            Id: getMaxIdForArray(productSetupMaster.Childs, "Id"),
            BlendTypeID: 0,
            BlendType: "Empty",
            YarnTypeID: 0,
            YarnType: "",
            ProgramID: 0,
            Program: "",
            SubProgramID: 0,
            SubProgram: "",
            CertificationsID: 0,
            Certifications: "",
            TechnicalParameterID: 0,
            TechnicalParameter: "",
            CompositionsID: 0,
            Compositions: "",
            ShadeID: 0,
            Shade: "",
            ManufacturingLineID: 0,
            ManufacturingLine: "",
            ManufacturingProcessID: 0,
            ManufacturingProcess: "",
            ManufacturingSubProcessID: 0,
            ManufacturingSubProcess: "",
            YarnColorID: 0,
            YarnColor: "",
            ColorGradeID: 0,
            ColorGrade: "",
            CountNames: "",
            CountIDs: "",
            EntityState: 4
        };
        productSetupMaster.Childs.push(newChildData);
        $tblMasterEl.bootstrapTable('load', productSetupMaster.Childs);
    }

    function addNewProgramInformation(e) {
        e.preventDefault();
        var newChildData = {
            Id: getMaxIdForArray(productSetupMaster.ChildPrograms, "Id"),
            ProgramID: 0,
            Program: "",
            SubProgramID: 0,
            SubProgram: "",
            CertificationsID: 0,
            Certifications: "",
            EntityState: 4
        };
        productSetupMaster.ChildPrograms.push(newChildData);
        $tblYPSProgramEl.bootstrapTable('load', productSetupMaster.ChildPrograms);
    }

    function addNewTechnicalParameter(e) {
        e.preventDefault();
        var newChildData = {
            Id: getMaxIdForArray(productSetupMaster.ChildTechnicalParameters, "Id"),
            TechnicalParameterID: 0,
            TechnicalParameter: "",
            EntityState: 4
        };
        productSetupMaster.ChildTechnicalParameters.push(newChildData);
        $tblYPSTechnicalParameterEl.bootstrapTable('load', productSetupMaster.ChildTechnicalParameters);
    }    

    function initMasterTable() {
        window.blendTypeFilterObject = convertSelect2ArrayToSelectOptionForFilter(productSetupMaster.BlendTypeList);
        window.yarnTypeFilterObject = convertSelect2ArrayToSelectOptionForFilter(productSetupMaster.YarnTypeList);
        window.programFilterObject = convertSelect2ArrayToSelectOptionForFilter(productSetupMaster.YarnProgramList);
        window.subProgramFilterObject = convertSelect2ArrayToSelectOptionForFilter(productSetupMaster.YarnSubProgramList);
        window.certificationsSelectOptionList = convertSelect2ArrayToSelectOptionForFilter(productSetupMaster.CertificationsList);

        $tblMasterEl.bootstrapTable('destroy');
        $tblMasterEl.bootstrapTable({
            uniqueId: 'Id',
            pagination: true,
            sidePagination: "client",
            pageList: "[10, 25, 50, 100, 500, all]",
            cache: false,
            filterControl: true,
            columns: [
                {
                    title: "Actions",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        return `<a class="btn btn-xs btn-default remove" href="javascript:void(0)" title="Remove">
                                        <i class="fa fa-remove" aria-hidden="true"></i>
                                    </a>`;
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            e.preventDefault();
                            row.EntityState = 8;
                            $tblMasterEl.bootstrapTable('updateByUniqueId', { id: row.Id, row: row });
                            $tblMasterEl.bootstrapTable('hideRow', { index: index });
                        }
                    },
                    width: 50
                },
                {
                    field: "BlendTypeID",
                    title: "Blend Type",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: productSetupMaster.BlendTypeList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    },
                    filterControl: "select",
                    filterData: "var:blendTypeFilterObject",
                    width: 100
                },
                {
                    field: "YarnTypeID",
                    title: "Yarn Type",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: productSetupMaster.YarnTypeList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    },
                    filterControl: "select",
                    filterData: "var:yarnTypeFilterObject",
                    width: 100
                },
                {
                    field: "ProgramID",
                    title: "Yarn Program",
                    visible: isNotBlendedOrColorMelange,
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: productSetupMaster.YarnProgramList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    },
                    filterControl: "select",
                    filterData: "var:programFilterObject",
                    width: 100
                },
                {
                    field: "SubProgramID",
                    title: "Yarn Sub Program",
                    visible: isNotBlendedOrColorMelange,
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: productSetupMaster.YarnSubProgramList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    },
                    filterControl: "select",
                    filterData: "var:subProgramFilterObject",
                    width: 100
                },
                {
                    field: "CertificationsID",
                    title: "Certification",
                    visible: isNotBlendedOrColorMelange,
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: productSetupMaster.CertificationsList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    },
                    filterControl: "select",
                    filterData: "var:certificationsSelectOptionList",
                    width: 100
                },
                {
                    field: "TechnicalParameterID",
                    title: "Technical Parameter",
                    visible: isNotBlendedOrColorMelange,
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: productSetupMaster.TechnicalParameterList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    },
                    width: 100
                },
                {
                    field: "CompositionsID",
                    title: "Compositions",
                    visible: isNotBlendedOrColorMelange,
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: productSetupMaster.CompositionsList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    },
                    width: 100
                },
                {
                    field: "ShadeID",
                    title: "Shade",
                    visible: isNotBlendedOrColorMelange,
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: productSetupMaster.ShadeList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    },
                    width: 100
                },
                {
                    field: "ManufacturingLineID",
                    title: "Manufacturing Line",
                    visible: isNotBlendedOrColorMelange,
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: productSetupMaster.ManufacturingLineList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    },
                    width: 100
                },
                {
                    field: "ManufacturingProcessID",
                    title: "Manufacturing Process",
                    visible: isNotBlendedOrColorMelange,
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: productSetupMaster.ManufacturingProcessList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    },
                    width: 100
                },
                {
                    field: "ManufacturingSubProcessID",
                    title: "Manufacturing Sub Process",
                    visible: isNotBlendedOrColorMelange,
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: productSetupMaster.ManufacturingSubProcessList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    },
                    width: 100
                },
                {
                    field: "YarnColorID",
                    title: "Yarn Color",
                    visible: isNotBlendedOrColorMelange,
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: productSetupMaster.YarnColorList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    },
                    width: 50
                },
                {
                    field: "ColorGradeID",
                    title: "Color Grade",
                    visible: isNotBlendedOrColorMelange,
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: productSetupMaster.ColorGradeList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    },
                    width: 50
                },
                {
                    field: "CountNames",
                    title: "Count Name",
                    visible: isNotBlendedOrColorMelange,
                    formatter: function (value, row, index, field) {
                        var text = row.CountNames ? row.CountNames : "Empty";
                        return `<a href="javascript:void(0)" class="editable-link edit">${text}</a>`;
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            var countIds;
                            if (row.CountIDs) countIds = row.CountIDs.split(',');

                            showBootboxSelect2MultipleDialog("Select Count", "CountIDs", "Select Count", productSetupMaster.CountList, function (result) {
                                if (result) {
                                    row.CountIDs = result.map(function (item) { return item.id }).join(",");
                                    row.CountNames = result.map(function (item) { return item.text }).join(",");
                                    $tblMasterEl.bootstrapTable('updateByUniqueId', { id: row.Id, row: row });
                                }
                            }, countIds);
                        },
                    },
                    cellStyle: function () { return { classes: 'm-w-500' } }
                }

            ]
        });
    }

    function initYPSProgramTable() {
        $tblYPSProgramEl.bootstrapTable('destroy');
        $tblYPSProgramEl.bootstrapTable({
            uniqueId: 'Id',
            pagination: true,
            filterControl: true,
            sidePagination: "client",
            pageList: "[10, 25, 50, 100, 500]",
            columns: [
                {
                    title: "Actions",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        return `<a class="btn btn-xs btn-default remove" href="javascript:void(0)" title="Remove">
                                        <i class="fa fa-remove" aria-hidden="true"></i>
                                    </a>`;
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            e.preventDefault();
                            row.EntityState = 8;
                            $tblYPSProgramEl.bootstrapTable('updateByUniqueId', { id: row.Id, row: row });
                            $tblYPSProgramEl.bootstrapTable('hideRow', { index: index });
                        }
                    },
                    cellStyle: function () { return { classes: 'm-w-10' } }
                },
                {
                    field: "ProgramID",
                    title: "Yarn Program",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: productSetupMaster.YarnProgramList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    },
                    cellStyle: function () { return { classes: 'm-w-30' } }
                },
                {
                    field: "SubProgramID",
                    title: "Yarn Sub Program",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: productSetupMaster.YarnSubProgramList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    },
                    cellStyle: function () { return { classes: 'm-w-30' } }
                },
                {
                    field: "CertificationsID",
                    title: "Certification",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: productSetupMaster.CertificationsList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    },
                    cellStyle: function () { return { classes: 'm-w-30' } }
                }
            ]
        });
    }

    function initYPSTechnicalParameterTable() {
        $tblYPSTechnicalParameterEl.bootstrapTable('destroy');
        $tblYPSTechnicalParameterEl.bootstrapTable({
            uniqueId: 'Id',
            pagination: true,
            filterControl: true,
            sidePagination: "client",
            pageList: "[10, 25, 50, 100, 500]",
            columns: [
                {
                    title: "Actions",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        return `<a class="btn btn-xs btn-default remove" href="javascript:void(0)" title="Remove">
                                        <i class="fa fa-remove" aria-hidden="true"></i>
                                    </a>`;
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            e.preventDefault();
                            row.EntityState = 8;
                            $tblYPSTechnicalParameterEl.bootstrapTable('updateByUniqueId', { id: row.Id, row: row });
                            $tblYPSTechnicalParameterEl.bootstrapTable('hideRow', { index: index });
                        }
                    },
                    cellStyle: function () { return { classes: 'm-w-20' } }
                },
                {
                    field: "TechnicalParameterID",
                    title: "Technical Parameter",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: productSetupMaster.TechnicalParameterList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    },
                    cellStyle: function () { return { classes: 'm-w-80' } }
                }
            ]
        });
    }

    function getInitData() {
        var url = "/api/yarn-product-setup/new";
        axios.get(url)
            .then(function (response) {
                productSetupMaster = response.data;
                initSelect2($fiberTypeEl, productSetupMaster.FiberTypeList, true, "Select fiber type.");
                $fiberTypeEl.on("select2:select", function (e) {
                    fiberTypeId = e.params.data.id;
                    if (!fiberTypeId) return;
                    fiberType = e.params.data.text;
                    isBlendedOrColorMelange = fiberType == fiberTypeConstants.BLENDED || fiberType == fiberTypeConstants.COLOR_MELANGE;
                    isNotBlendedOrColorMelange = !isBlendedOrColorMelange;
                    
                    getDataByFiberType();
                    initMasterTable();
                    
                    if (isBlendedOrColorMelange) {
                        initYPSProgramTable();
                        initYPSTechnicalParameterTable();
                        $("#divYPSProgram").removeClass("d-none");
                        $("#divYPSTechnicalParameter").removeClass("d-none");
                    }
                    else {
                        $("#divYPSProgram").addClass("d-none");
                        $("#divYPSTechnicalParameter").addClass("d-none");
                    }
                });
            })
            .catch(showResponseError)
    }

    function getDataByFiberType() {
        var url = `/api/yarn-product-setup/list?fiberTypeId=${fiberTypeId}`;
        axios.get(url)
            .then(function (response) {
                productSetupMaster.Childs = response.data.Childs;
                productSetupMaster.ChildPrograms = response.data.ChildPrograms;
                productSetupMaster.ChildTechnicalParameters = response.data.ChildTechnicalParameters;
                $tblMasterEl.bootstrapTable("load", productSetupMaster.Childs);
                $tblYPSProgramEl.bootstrapTable('load', productSetupMaster.ChildPrograms);
                $tblYPSTechnicalParameterEl.bootstrapTable('load', productSetupMaster.ChildTechnicalParameters);
            })
            .catch(showResponseError);
    }

    function save(e) {
        e.preventDefault();
        var data = {
            FiberTypeId: fiberTypeId,
            Childs: productSetupMaster.Childs,
            ChildPrograms: productSetupMaster.ChildPrograms,
            ChildTechnicalParameters: productSetupMaster.ChildTechnicalParameters
        };

        axios.post("/api/yarn-product-setup/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
            })
            .catch(showResponseError);
    }
})();
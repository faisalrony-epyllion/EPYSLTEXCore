(function () {
    var menuId, pageName;
    var $tblMasterEl, $formEl;
    var $fiberTypeEl;
    var fiberTypeId, fiberType, isBlendedOrColorMelange;

    var segmentFilterSetup, segmentFilterSetups = [], productSetupChilds = [], productSetupChildPrograms = [], productSetupChildTechnicalParameters = [], ProcessSetupList = [];

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $fiberTypeEl = $formEl.find("#FiberTypeID");

        getInitData();

        $formEl.find("#btnSave").click(save);

        $formEl.find("#btnAddItem").on("click", function (e) {
            e.preventDefault();
            showYarnProductSetup();
        });

        $formEl.find("#FiberTypeID").on("select2:select", function (e) {
            fiberTypeId = e.params.data.id;
            if (!fiberTypeId) return;
            fiberType = e.params.data.text;
            isBlendedOrColorMelange = fiberType == fiberTypeConstants.BLENDED || fiberType == fiberTypeConstants.COLOR_MELANGE;

            getDataBySupplierAndFiberType();
        });
    });

    function initMasterTable() {
        $tblMasterEl.bootstrapTable('destroy');
        $tblMasterEl.bootstrapTable({
            uniqueId: 'SegmentValueMappingID',
            pagination: true,
            sidePagination: "client",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            editable: true,
            data: segmentFilterSetup.SegmentFilterMappingList,
            columns: [
                {
                    field: "YarnType",
                    title: "Yarn Type"
                },
                {
                    field: "ManufacturingProcess",
                    title: "Manufacturing Process"
                },
                {
                    field: "SubProcess",
                    title: "Sub Process"
                },
                {
                    field: "QualityParameter",
                    title: "Quality Parameter"
                },
                {
                    field: "Count",
                    title: "Count"
                }
            ]
        });
    }

    function getInitData() {

        var url = "/api/items/yarn/item-segments-admin";
        axios.get(url)
            .then(function (response) {
                segmentFilterSetup = response.data;
                segmentFilterSetup.YarnCountMasterList = segmentFilterSetup.YarnCountMaster;

        var textList3 = [];
                var obj3 = {};

        var FilteredSegment3ValueList = [];
                for (var i = 0; i < segmentFilterSetup.Segment3ValueList.length; i++) {
            if (textList3.includes(segmentFilterSetup.Segment3ValueList[i].text) == false) {
                obj3 = {};
                obj3.id = segmentFilterSetup.Segment3ValueList[i].id;
                obj3.text = segmentFilterSetup.Segment3ValueList[i].text;
                obj3.desc = segmentFilterSetup.Segment3ValueList[i].desc;
                FilteredSegment3ValueList.push(obj3);
                textList3.push(segmentFilterSetup.Segment3ValueList[i].text);
            }
        }

        var textList4 = [];
        var obj4 = {};
        var FilteredSegment4ValueList = [];
                for (var i = 0; i < segmentFilterSetup.Segment4ValueList.length; i++) {
            if (textList4.includes(segmentFilterSetup.Segment4ValueList[i].text) == false) {
                obj4 = {};
                obj4.id = segmentFilterSetup.Segment4ValueList[i].id;
                obj4.text = segmentFilterSetup.Segment4ValueList[i].text;
                obj4.desc = segmentFilterSetup.Segment4ValueList[i].desc;
                FilteredSegment4ValueList.push(obj4);
                textList4.push(segmentFilterSetup.Segment4ValueList[i].text);
            }
        }

        var textList5 = [];
        var obj5 = {};
        var FilteredSegment5ValueList = [];
                for (var i = 0; i < segmentFilterSetup.Segment5ValueList.length; i++) {
            if (textList5.includes(segmentFilterSetup.Segment5ValueList[i].text) == false) {
                obj5 = {};
                obj5.id = segmentFilterSetup.Segment5ValueList[i].id;
                obj5.text = segmentFilterSetup.Segment5ValueList[i].text;
                obj5.desc = segmentFilterSetup.Segment5ValueList[i].desc;
                FilteredSegment5ValueList.push(obj5);
                textList5.push(segmentFilterSetup.Segment5ValueList[i].text);
            }
        }

        var textList6 = [];
        var obj6 = {};
        var FilteredSegment6ValueList = [];
                for (var i = 0; i < segmentFilterSetup.YarnCountMasterList.length; i++) {
                    if (textList6.includes(segmentFilterSetup.YarnCountMasterList[i].text) == false) {
                obj6 = {};
                        obj6.id = segmentFilterSetup.YarnCountMasterList[i].text;
                        obj6.text = segmentFilterSetup.YarnCountMasterList[i].text;
                        obj6.desc = segmentFilterSetup.YarnCountMasterList[i].desc;
                FilteredSegment6ValueList.push(obj6);
                        textList6.push(segmentFilterSetup.YarnCountMasterList[i].text);
            }
        }


                segmentFilterSetup.Segment3ValueList = FilteredSegment3ValueList;
                segmentFilterSetup.Segment4ValueList = FilteredSegment4ValueList;
                segmentFilterSetup.Segment5ValueList = FilteredSegment5ValueList;
                segmentFilterSetup.YarnCountMasterList = FilteredSegment6ValueList;

                setFormData($formEl, segmentFilterSetup);
                initMasterTable();
            })
            .catch(showResponseError)
    }

    function getDataBySupplierAndFiberType() {
        var url = `/api/yarn-product-setup-supplier/list?supplierId=${$formEl.find("#SupplierID").val()}&fiberTypeId=${fiberTypeId}`;
        axios.get(url)
            .then(function (response) {
                segmentFilterSetups = response.data;
                initMasterTable();
                $tblMasterEl.bootstrapTable("load", segmentFilterSetups);
            })
            .catch(showResponseError);
    }

    function showYarnProductSetup() {
        getDataByFiberType();
    }

    function getDataByFiberType() {
        var url = `/api/yarn-product-setup-supplier/list-product-setup?fiberTypeId=${fiberTypeId}`;
        axios.get(url)
            .then(function (response) {
                productSetupChilds = response.data.Childs;
                productSetupChildPrograms = response.data.ChildPrograms;
                productSetupChildTechnicalParameters = response.data.TechnicalParameterList;
                ProcessSetupList = response.data.ProcessSetupList;

                initTblYarnProductSetupV1();
                $("#tblYarnProductSetupV1").bootstrapTable("load", productSetupChilds);
                $("#modal-YarnProductSetupV1").modal("show");
            })
            .catch(showResponseError);
    }

    function showYarnCompositionSelction(rowData) {
        axios.get(`/api/yarn-pr/compositions?fiberType=${fiberType}&yarnType=${rowData.YarnType}`)
            .then(function (response) {
                compositionList = response.data;
                showBootboxSelect2Dialog("Select Composition", "Composition", "Choose Composition", compositionList, function (result) {
                    if (!result) {
                        rowData.CompositionsID = 0;
                        rowData.Compositions = "";
                        $tblMasterEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
                        toastr.warning("You didn't selected any Composition.");
                        return;
                    }

                    rowData.CompositionsID = (result.id == "") ? 0 : result.id;
                    rowData.Compositions = result.text;
                    $tblMasterEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
                }, rowData.CompositionsID)
            })
            .catch(showResponseError);
    }

    function initTblYarnProductSetupV1() {
        $("#tblYarnProductSetupV1").bootstrapTable('destroy');
        $("#tblYarnProductSetupV1").bootstrapTable({
            //toolbar: "#toolbarTblAddNewComposition",
            showFooter: true,
            editable: true,
            pagination: true,
            filterControl: true,
            sidePagination: "client",
            searchOnEnterKey: true,
            pageList: "[10, 25, 50, 100, 500]",
            columns: [
                {
                    field: "BlendType",
                    title: "Blend Type",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-200' } }
                },
                {
                    field: "YarnType",
                    title: "Yarn Type",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Program",
                    title: "Program",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: !isBlendedOrColorMelange
                },
                {
                    field: "SubProgram",
                    title: "Sub Program",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-180' } },
                    visible: !isBlendedOrColorMelange
                },
                {
                    field: "Certifications",
                    title: "Certifications",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: !isBlendedOrColorMelange
                },
                {
                    field: "TechnicalParameter",
                    title: "Technical Parameter",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: !isBlendedOrColorMelange
                },
                {
                    field: "Compositions",
                    title: "Compositions",
                    visible: !isBlendedOrColorMelange,
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Shade",
                    title: "Shade",
                    visible: !isBlendedOrColorMelange,
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ManufacturingLine",
                    title: "Manufacturing Line",
                    visible: !isBlendedOrColorMelange,
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ManufacturingProcess",
                    title: "Manufacturing Process",
                    visible: !isBlendedOrColorMelange,
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ManufacturingSubProcess",
                    title: "Manufacturing Sub Process",
                    visible: !isBlendedOrColorMelange,
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "YarnColor",
                    title: "Yarn Color",
                    visible: !isBlendedOrColorMelange,
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ColorGrade",
                    title: "Color Grade",
                    visible: !isBlendedOrColorMelange,
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                }
            ],
            onDblClickRow: function (row, $element, field) {
                if (!isBlendedOrColorMelange) {
                    var newChildItem = {
                        Id: getMaxIdForArray(segmentFilterSetups, "Id"),
                        //FiberType: row.FiberType,
                        BlendType: row.BlendType,
                        YarnType: row.YarnType,
                        Program: row.Program,
                        SubProgram: row.SubProgram,
                        Certifications: row.Certifications,
                        TechnicalParameter: row.TechnicalParameter,
                        Compositions: row.Compositions,
                        ManufacturingLine: row.ManufacturingLine,
                        ManufacturingProcess: row.ManufacturingProcess,
                        ManufacturingSubProcess: row.ManufacturingSubProcess,
                        Shade: row.Shade,
                        YarnColor: row.YarnColor,
                        ColorGrade: row.ColorGrade,
                        SupplierID: $formEl.find("#SupplierID").val(),
                        FiberTypeID: $formEl.find("#FiberTypeID").val(),
                        BlendTypeID: row.BlendTypeID,
                        YarnTypeID: row.YarnTypeID,
                        ProgramID: row.ProgramID,
                        SubProgramID: row.SubProgramID,
                        CertificationsID: row.CertificationsID,
                        TechnicalParameterID: row.TechnicalParameterID,
                        CompositionsID: row.CompositionsID,
                        ManufacturingLineID: row.ManufacturingLineID,
                        ManufacturingProcessID: row.ManufacturingProcessID,
                        ManufacturingSubProcessID: row.ManufacturingSubProcessID,
                        ShadeID: row.ShadeID,
                        YarnColorID: row.YarnColorID,
                        ColorGradeID: row.ColorGradeID,
                        CountIDs: "",
                        CountNames: "",
                        EntityState: 4
                    };
                    segmentFilterSetups.push(newChildItem);
                    $tblMasterEl.bootstrapTable('load', segmentFilterSetups);
                    $('#modal-YarnProductSetupV1').modal('toggle');

                    var url = `/api/selectoption/get-count-from-product-setup-by-childId/${row.SetupChildID}`;
                    axios.get(url)
                        .then(function (response) {
                            segmentFilterSetup.CountList = response.data;
                        }).catch(showResponseError);

                } else {
                    var newChildItem = {
                        Id: getMaxIdForArray(segmentFilterSetups, "Id"),
                        BlendType: row.BlendType,
                        YarnType: row.YarnType,
                        Program: "",
                        SubProgram: "",
                        Certifications: "",
                        TechnicalParameter: "",
                        Compositions: "",
                        ManufacturingLine: "",
                        ManufacturingProcess: "",
                        ManufacturingSubProcess: "",
                        Shade: "",
                        YarnColor: "",
                        ColorGrade: "",
                        SupplierID: $formEl.find("#SupplierID").val(),
                        FiberTypeID: $formEl.find("#FiberTypeID").val(),
                        BlendTypeID: row.BlendTypeID,
                        YarnTypeID: row.YarnTypeID,
                        ProgramID: 0,
                        SubProgramID: 0,
                        CertificationsID: 0,
                        TechnicalParameterID: 0,
                        CompositionsID: 0,
                        ManufacturingLineID: 0,
                        ManufacturingProcessID: 0,
                        ManufacturingSubProcessID: 0,
                        ShadeID: 0,
                        YarnColorID: 0,
                        ColorGradeID: 0,
                        CountIDs: "",
                        CountNames: "",
                        EntityState: 4
                    };
                    segmentFilterSetups.push(newChildItem);
                    $tblMasterEl.bootstrapTable('load', segmentFilterSetups);
                    $('#modal-YarnProductSetupV1').modal('toggle');

                    initTblYarnProductSetupV2(newChildItem);
                    $("#modal-YarnProductSetupV2").modal("show");
                }

            }
        });
    }

    function initTblYarnProductSetupV2(rowData) {
        $("#tblYarnProductSetupV2").bootstrapTable('destroy');
        $("#tblYarnProductSetupV2").bootstrapTable({
            //toolbar: "#toolbarTblAddNewComposition",
            showFooter: true,
            editable: true,
            pagination: true,
            filterControl: true,
            sidePagination: "client",
            searchOnEnterKey: true,
            pageList: "[10, 25, 50, 100, 500]",
            data: productSetupChildPrograms,
            columns: [
                {
                    field: "Program",
                    title: "Program",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "SubProgram",
                    title: "Sub Program",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-180' } }
                },
                {
                    field: "Certifications",
                    title: "Certifications",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                }
            ],
            onDblClickRow: function (row, $element, field) {
                rowData.Program = row.Program;
                rowData.ProgramID = row.ProgramID;
                rowData.SubProgram = row.SubProgram;
                rowData.SubProgramID = row.SubProgramID;
                rowData.Certifications = row.Certifications;
                rowData.CertificationsID = row.CertificationsID;
                $tblMasterEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
                $('#modal-YarnProductSetupV2').modal('toggle');

            }
        });
    }

    function showManufacturingModal(rowData, masterIndex) {
        initTblManufacturing(rowData, masterIndex);
        $("#modal-manufacturing").modal("show");
    }

    function initTblManufacturing(rowData, masterIndex) {
        $("#tblManufacturing").bootstrapTable('destroy');
        $("#tblManufacturing").bootstrapTable({
            //toolbar: "#toolbarTblAddNewComposition",
            showFooter: true,
            editable: true,
            pagination: true,
            filterControl: true,
            sidePagination: "client",
            searchOnEnterKey: true,
            pageList: "[10, 25, 50, 100, 500]",
            data: ProcessSetupList,
            columns: [
                {
                    field: "ManufacturingLine",
                    title: "Manufacturing Line",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ManufacturingProcess",
                    title: "Manufacturing Process",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ManufacturingSubProcess",
                    title: "Manufacturing Sub Process",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Counts",
                    title: "Counts",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-200' } }
                },
                {
                    field: "YarnColor",
                    title: "Yarn Color",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ColorGrade",
                    title: "Color Grade",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                }
            ],
            onDblClickRow: function (row, $element, field) {
                rowData.ManufacturingLineID = row.ManufacturingLineID;
                rowData.ManufacturingProcessID = row.ManufacturingProcessID;
                rowData.ManufacturingSubProcessID = row.ManufacturingSubProcessID;
                rowData.YarnColorID = row.YarnColorID;
                rowData.ColorGradeID = row.ColorGradeID;
                rowData.Segment12ValueId = 0;
                rowData.Segment12ValueDesc = "";
                rowData.ManufacturingLine = row.ManufacturingLine;
                rowData.ManufacturingProcess = row.ManufacturingProcess;
                rowData.ManufacturingSubProcess = row.ManufacturingSubProcess;
                rowData.YarnColor = row.YarnColor;
                rowData.ColorGrade = row.ColorGrade;
                $tblMasterEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });

                $('#modal-manufacturing').modal('toggle');

                var url = `/api/selectoption/get-count-from-process-setup/${row.Id}`;
                axios.get(url)
                    .then(function (response) {
                        segmentFilterSetup.CountList = response.data;
                    }).catch(showResponseError);
            }

        });
    }

    function save(e) {
        e.preventDefault();
        var data = formElToJson($formEl);
        data.SegmentValueMappingID = 0;
        data.YarnTypeSVID = data.Segment2ValueID;
        data.ManufacturingProcessSVID  = data.Segment3ValueID;
        data.SubProcessSVID  = data.Segment4ValueID;
        data.QualityParameterSVID  = data.Segment5ValueID;
        data.CountUnit = data.YarnCountMasterID;
        debugger;
        axios.post("/api/segment-filter-setup/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                getInitData();
            })
            .catch(showResponseError);
    }

})();
(function () {
    var menuId, pageName;
    var $tblMasterEl, $formEl;
    var $fiberTypeEl;
    var fiberTypeId, fiberType, isBlendedOrColorMelange;

    var yarnProductSetupSupplier, yarnProductSetupSuppliers = [], productSetupChilds = [], productSetupChildPrograms = [], productSetupChildTechnicalParameters = [], ProcessSetupList = [];

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
            //initTblYarnProductSetupV1();
            //$("#modal-YarnProductSetupV1").modal("show");
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
            uniqueId: 'Id',
            pagination: true,
            sidePagination: "client",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            editable: true,
            columns: [
                {
                    field: "BlendType",
                    title: "Blend Type"
                },
                {
                    field: "YarnType",
                    title: "Yarn Type"
                },
                {
                    field: "Program",
                    title: "Yarn Program"
                },
                {
                    field: "SubProgram",
                    title: "Yarn Sub Program"
                },
                {
                    field: "Certifications",
                    title: "Certification"
                },
                {
                    field: "TechnicalParameter",
                    title: "Technical Parameter",
                    formatter: function (value, row, index, field) {
                        var value = row.TechnicalParameter ? row.TechnicalParameter : "Empty";
                        return isBlendedOrColorMelange ? `<span class="btn-group">
                                <a href="javascript:void(0)" class="editable-link edit">${value}</a>
                                </span>` : "Empty";
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            showBootboxSelect2Dialog("Technical Parameter", "TechnicalParameter", "Choose TechnicalParameter", productSetupChildTechnicalParameters, function (result) {
                                if (!result) {
                                    row.TechnicalParameterID = 0;
                                    row.TechnicalParameter = "";
                                    $tblMasterEl.bootstrapTable('updateByUniqueId', { id: row.Id, row: row });
                                    toastr.warning("You didn't selected any Composition.");
                                    return;
                                }
                                row.TechnicalParameterID = (result.id == "") ? 0 :result.id;
                                row.TechnicalParameter = result.text;
                                $tblMasterEl.bootstrapTable('updateByUniqueId', { id: row.Id, row: row });
                            }, row.TechnicalParameterID)
                        }
                    }
                },
                {
                    field: "Compositions",
                    title: "Compositions",
                    formatter: function (value, row, index, field) {
                        var value = row.Compositions ? row.Compositions : "Empty";
                        return isBlendedOrColorMelange ? `<span class="btn-group">
                                <a href="javascript:void(0)" class="editable-link edit">${value}</a>
                                </span>` : "Empty";
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            if (!row.YarnTypeID) return toastr.error("Please select yarn!");

                            showYarnCompositionSelction(row);
                        }
                    }
                },
                {
                    field: "ShadeID",
                    title: "Shade",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: yarnProductSetupSupplier.ShadeList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true },
                        //noEditFormatter: function(value, row, index) {
                        //    return !isBlendedOrColorMelange
                        //}
                    }
                },
                {
                    field: "ManufacturingLine",
                    title: "Manufacturing Line",
                    formatter: function (value, row, index, field) {
                        return isBlendedOrColorMelange ? `<span class="btn-group">
                            <a href="javascript:void(0)" class="editable-link edit">${row.ManufacturingLine ? row.ManufacturingLine : "Empty"}</a>
                            </span>` : "Empty";
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            showManufacturingModal(row, index);
                        }
                    }
                },
                {
                    field: "ManufacturingProcess",
                    title: "Manufacturing Process"
                },
                {
                    field: "ManufacturingSubProcess",
                    title: "Manufacturing Sub Process"
                },
                {
                    field: "YarnColor",
                    title: "Yarn Color"
                },
                {
                    field: "ColorGrade",
                    title: "Color Grade"
                },
                {
                    field: "CountNames",
                    title: "Count Name",
                    formatter: function (value, row, index, field) {
                        var text = row.CountNames ? row.CountNames : "Empty";
                        return `<a href="javascript:void(0)" class="editable-link edit">${text}</a>`;
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            var countIds;
                            if (row.CountIDs) countIds = row.CountIDs.split(',');

                            showBootboxSelect2MultipleDialog("Select Count", "CountIDs", "Select Count", yarnProductSetupSupplier.CountList, function (result) {
                                if (result) {
                                    row.CountIDs = result.map(function (item) { return item.id }).join(",");
                                    row.CountNames = result.map(function (item) { return item.text }).join(",");
                                    $tblMasterEl.bootstrapTable('updateByUniqueId', { id: row.Id, row: row });
                                }
                            }, countIds);
                        },
                    }
                }
            ]
        });
    }

    function getInitData() {
        var url = "/api/yarn-product-setup-supplier/new";
        axios.get(url)
            .then(function (response) {
                yarnProductSetupSupplier = response.data;
                setFormData($formEl, yarnProductSetupSupplier);
            })
            .catch(showResponseError)
    }

    function getDataBySupplierAndFiberType() {
        var url = `/api/yarn-product-setup-supplier/list?supplierId=${$formEl.find("#SupplierID").val()}&fiberTypeId=${fiberTypeId}`;
        axios.get(url)
            .then(function (response) {
                yarnProductSetupSuppliers = response.data;
                initMasterTable();
                $tblMasterEl.bootstrapTable("load", yarnProductSetupSuppliers);
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
                        Id: getMaxIdForArray(yarnProductSetupSuppliers, "Id"),
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
                    yarnProductSetupSuppliers.push(newChildItem);
                    $tblMasterEl.bootstrapTable('load', yarnProductSetupSuppliers);
                    $('#modal-YarnProductSetupV1').modal('toggle');

                    var url = `/api/selectoption/get-count-from-product-setup-by-childId/${row.SetupChildID}`;
                    axios.get(url)
                        .then(function (response) {
                            yarnProductSetupSupplier.CountList = response.data;
                        }).catch(showResponseError);

                } else {
                    var newChildItem = {
                        Id: getMaxIdForArray(yarnProductSetupSuppliers, "Id"),
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
                    yarnProductSetupSuppliers.push(newChildItem);
                    $tblMasterEl.bootstrapTable('load', yarnProductSetupSuppliers);
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
                        yarnProductSetupSupplier.CountList = response.data;
                    }).catch(showResponseError);
            }

        });
    }

    function save(e) {
        e.preventDefault();

        axios.post("/api/yarn-product-setup-supplier/save", yarnProductSetupSuppliers)
            .then(function () {
                toastr.success("Saved successfully!");
                getDataBySupplierAndFiberType();
            })
            .catch(showResponseError);
    }

})();
(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }

    var supplierProductRelation;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        $tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        initMasterTable();
        getMasterTableData();

        $toolbarEl.find("#btnNew").click(getNew);

        $toolbarEl.find("#btnList").click("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            initMasterTable();
            getMasterTableData();
        });

        $formEl.find("#btnSave").click(save);

        $formEl.find("#btnAddItem").click(addNewItem);

        $formEl.find("#btnCancel").on("click", backToList);
    });

    function initMasterTable() {
        $tblMasterEl.bootstrapTable('destroy');
        $tblMasterEl.bootstrapTable({
            showRefresh: true,
            showExport: true,
            showColumns: true,
            toolbar: toolbarId,
            exportTypes: "['csv', 'excel']",
            pagination: true,
            filterControl: true,
            searchOnEnterKey: true,
            sidePagination: "server",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            showFooter: true,
            columns: [
                {
                    field: "",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        var template =
                            `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Supplier Product Setup">
                                <i class="fa fa-pencil" aria-hidden="true"></i>
                            </a>`;
                        return template;
                    },
                    events: {
                        'click .edit': getDetails
                    }
                },
                {
                    field: "SupplierName",
                    title: "Company",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-200' } }
                },
                {
                    field: "FiberType",
                    title: "Fiber Type",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-120' } }
                },
                {
                    field: "BlendType",
                    title: "Blend Type",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-120' } }
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
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "SubProgram",
                    title: "Sub Program",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Certifications",
                    title: "Certifications",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "TechnicalParameter",
                    title: "Technical Parameter",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Compositions",
                    title: "Compositions",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Shade",
                    title: "Shade",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
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
                    title: "Sub Process",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "YarnColor",
                    title: "Yarn Color",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "ColorGrade",
                    title: "Color Grade",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "CountNames",
                    title: "Count Name",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function () { return { classes: 'm-w-500' } }
                }
            ],
            onPageChange: function (number, size) {
                var newOffset = (number - 1) * size;
                var newLimit = size;
                if (tableParams.offset == newOffset && tableParams.limit == newLimit)
                    return;

                tableParams.offset = newOffset;
                tableParams.limit = newLimit;

                getMasterTableData();
            },
            onSort: function (name, order) {
                tableParams.sort = name;
                tableParams.order = order;
                tableParams.offset = 0;

                getMasterTableData();
            },
            onRefresh: function () {
                resetTableParams();
                getMasterTableData();
            },
            onColumnSearch: function (columnName, filterValue) {
                if (columnName in filterBy && !filterValue) {
                    delete filterBy[columnName];
                }
                else
                    filterBy[columnName] = filterValue;

                if (Object.keys(filterBy).length === 0 && filterBy.constructor === Object)
                    tableParams.filter = "";
                else
                    tableParams.filter = JSON.stringify(filterBy);

                getMasterTableData();
            }
        });
    }

    function addNewItem(e) {
        e.preventDefault();
        var newChildData = {
            Id: getMaxIdForArray(supplierProductRelation.Childs, "Id"),
            SetupMasterID: 0,
            FiberTypeID: 0,
            FiberType: "",
            BlendTypeID: 0,
            BlendType: "Empty",
            YarnTypeID: 0,
            YarnType: "",
            ProgramID: 0,
            Program: "",
            SubProgramID: 0,
            SubProgram: "",
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
            CertificationsID: 0,
            Certifications: "",
            TechnicalParameterID: 0,
            TechnicalParameter: "",
            CountIDs: "",
            CountNames: "",
            EntityState: 4
        };
        supplierProductRelation.Childs.push(newChildData);
        $tblChildEl.bootstrapTable('load', supplierProductRelation.Childs);
    }

    function initChildTable() {
        $tblChildEl.bootstrapTable('destroy');
        $tblChildEl.bootstrapTable({
            uniqueId: 'Id',
            showFooter: true,
            columns: [
                //{
                //    formatter: function (value, row, index, field) {
                //        return ['<span class="btn-group">',
                //            '<a class="btn btn-xs btn-danger remove" onclick="javascript:void(0)" title="Remove">',
                //            '<i class="fa fa-remove" aria-hidden="true"></i>',
                //            '</a>',
                //            '</span>'].join(' ');
                //    },
                //    events: {
                //        'click .remove': function (e, value, row, index) {
                //            e.preventDefault();
                //            $tblChildEl.bootstrapTable('remove', { field: 'Id', values: [row.Id] });
                //        },
                //    }
                //},
                {
                    field: "FiberTypeID",
                    title: "Fiber Type",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: supplierProductRelation.FiberTypeList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    }
                },
                {
                    field: "BlendTypeID",
                    title: "Blend Type",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: supplierProductRelation.BlendTypeList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    }
                },
                {
                    field: "YarnTypeID",
                    title: "Yarn Type",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: supplierProductRelation.YarnTypeList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    }
                },
                {
                    field: "ProgramID",
                    title: "Yarn Program",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: supplierProductRelation.YarnProgramList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    }
                },
                {
                    field: "SubProgramID",
                    title: "Yarn Sub Program",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: supplierProductRelation.YarnSubProgramList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    }
                },
                {
                    field: "CertificationsID",
                    title: "Certification",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: supplierProductRelation.CertificationsList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    }
                },
                {
                    field: "TechnicalParameterID",
                    title: "Technical Parameter",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: supplierProductRelation.TechnicalParameterList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    }
                },
                {
                    field: "ManufacturingLineID",
                    title: "Manufacturing Line",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: supplierProductRelation.ManufacturingLineList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    }
                },
                {
                    field: "ManufacturingProcessID",
                    title: "Manufacturing Process",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: supplierProductRelation.ManufacturingProcessList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    }
                },
                {
                    field: "ManufacturingSubProcessID",
                    title: "Manufacturing Sub Process",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: supplierProductRelation.ManufacturingSubProcessList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    }
                },
                {
                    field: "CompositionsID",
                    title: "Yarn Composition",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: supplierProductRelation.YarnCompositionList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    }
                },
                {
                    field: "CountNames",
                    title: "Count",
                    formatter: function (value, row, index, field) {
                        var text = row.CountNames ? row.CountNames : "Empty";
                        return `<a href="javascript:void(0)" class="editable-link edit">${text}</a>`;
                    },
                    cellStyle: function () { return { classes: 'm-w-200' } },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            if (!row.YarnTypeID) return toastr.error("Yarn Type is not selected!");
                            getYarnCountByYarnType(row.YarnTypeID, row);

                        },
                    }
                },
                {
                    field: "YarnColorID",
                    title: "Yarn Color",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: supplierProductRelation.YarnColorList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    }
                },
                {
                    field: "ColorGradeID",
                    title: "Color Grade",
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: supplierProductRelation.ColorGradeList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
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
                        source: supplierProductRelation.ShadeList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    }
                }
            ]
        });
    }

    function getMasterTableData() {
        var queryParams = $.param(tableParams);
        $tblMasterEl.bootstrapTable('showLoading');
        var url = `/api/yarn-supplier-product-relation/list?gridType=bootstrap-table&${queryParams}`;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(showResponseError)
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        getMasterTableData();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#Id").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getYarnCountByYarnType(yarnTypeId, rowData) {
        var url = "/api/selectoption/yarn-count-by-yarn-type/" + yarnTypeId;
        axios.get(url)
            .then(function (response) {
                supplierProductRelation.CountList = response.data;
                showBootboxSelect2MultipleDialog("Select Count", "CountIDs", "Select Count", supplierProductRelation.CountList, function (result) {
                    if (result) {
                        rowData.CountIDs = result.map(function (item) { return item.id }).join(",");
                        rowData.CountNames = result.map(function (item) { return item.text }).join(",");
                        $tblChildEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
                    }
                });
            })
            .catch(showResponseError);
    }

    function getNew(e) {
        e.preventDefault();
        axios.get("/api/yarn-supplier-product-relation/new")
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                supplierProductRelation = response.data;
                initChildTable();
                setFormData($formEl, supplierProductRelation);
            })
            .catch(showResponseError);
    }

    function getDetails(e, value, row, index) {
        e.preventDefault();
        axios.get(`/api/yarn-supplier-product-relation/${row.SetupMasterID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                supplierProductRelation = response.data;
                setFormData($formEl, supplierProductRelation);
                initChildTable();
                $tblChildEl.bootstrapTable("load", supplierProductRelation.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(showResponseError);
    }

    function save(e) {
        e.preventDefault();
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = supplierProductRelation.Childs;
        axios.post("/api/yarn-supplier-product-relation/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(showResponseError);
    }
})();
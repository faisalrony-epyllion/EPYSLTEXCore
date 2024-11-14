(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl;
    var isFlatKnit = false;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }

    var masterData = {};

    /** use to filter data for select options */
    var filterData = {};

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");
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

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnNew").on("click", getNew);

        $formEl.find("#btnNewItem").on("click", function (e) {
            e.preventDefault();
            var newChildItem = {
                OptionID: getMaxIdForArray(masterData.KnittingMachineOptions, "OptionID"),
                MachineGauge: 0,
                CylinderNo: 0,
                Needle: 0,
                EntityState: 4
            };

            masterData.KnittingMachineOptions.push(newChildItem);
            $tblChildEl.bootstrapTable('load', masterData.KnittingMachineOptions);
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#MachineTypeID").on("select2:select select2:unselect", function (e) {
            var machineNatureId;
            var subClassList = [];
            if (e.params._type === "unselect") {
                machineNatureId = "";
            }
            else {
                var subClassList = filterData.MachineSubClassList.filter(function (el) { return el.desc == e.params.data.id });
                machineNatureId = e.params.data.desc;
            }

            $formEl.find("#MachineNatureID").val(machineNatureId);
            toggleFlatKnitControls(machineNatureId);
            initSelect2($formEl.find("#MachineSubClassID"), subClassList, true, "Select machine sub class.");
        });

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
                        return [
                            '<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Machine">',
                            '<i class="fa fa-edit" aria-hidden="true"></i>',
                            '</a>'
                        ].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            //console.log(row);
                            e.preventDefault();
                            getDetails(row.KnittingMachineID);
                        }
                    }
                },
                {
                    field: "MachineNo",
                    title: "Machine No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "SerialNo",
                    title: "Serial No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "UnitName",
                    title: "Unit",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Nature",
                    title: "Machine Nature",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "MachineTypeName",
                    title: "Machine Type",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "MachineSubClassName",
                    title: "Machine SubClass",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Brand",
                    title: "Brand",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Origin",
                    title: "Origin",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Dia",
                    title: "Dia",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "GG",
                    title: "GG",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Capacity",
                    title: "Capacity/Day (Kg)",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Feeder",
                    title: "Feeder",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "MinRPM",
                    title: "Min RPM",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "AvgRPM",
                    title: "Avg RPM",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Head",
                    title: "Head",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "TwoToneCapacity",
                    title: "Two Tone Capacity",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },

                {
                    field: "SolidCapacity",
                    title: "Solid Capacity",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "JacquredCapacity",
                    title: "Jacqured Capacity",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ManufacturerDate",
                    title: "Manufacturer Date",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "ErectionDate",
                    title: "Erection Date",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "Remarks",
                    title: "Remarks",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
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

    function getMasterTableData() {
        var queryParams = $.param(tableParams);
        $tblMasterEl.bootstrapTable('showLoading');
        var url = "/api/knitting-machine_list?gridType=bootstrap-table&" + queryParams;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function initChildTable() {
        $tblChildEl.bootstrapTable('destroy');
        $tblChildEl.bootstrapTable({
            uniqueId: 'OptionID',
            columns: [
                {
                    width: 40,
                    formatter: function (value, row, index, field) {
                        return ['<span class="btn-group">',
                            '<a class="btn btn-xs btn-danger remove" onclick="javascript:void(0)" title="Remove">',
                            '<i class="fa fa-remove" aria-hidden="true"></i>',
                            '</a>',
                            '</span>'].join(' ');
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            e.preventDefault();
                            $tblChildEl.bootstrapTable('remove', { field: 'OptionID', values: [row.OptionID] });
                        },
                    }
                },
                {
                    field: "MachineGauge",
                    title: "Machine Gauge",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm m-w-50',
                        showbuttons: false
                    }
                },
                {
                    field: "CylinderNo",
                    title: "No of Cylinder",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm m-w-50',
                        showbuttons: false
                    }
                },
                {
                    field: "Needle",
                    title: "Needle",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm m-w-50',
                        showbuttons: false
                    }
                },

            ]
        });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data["KnittingMachineOptions"] = masterData.KnittingMachineOptions;
        /*    var a = data["Childs"]*/
        /* console.log(a);*/
        /* data["Childs"] = $tblChildEl.getCurrentViewRecords();*/

        axios.post("/api/knitting-machine", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        getMasterTableData();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#KnittingMachineID").val(-1111);
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
        axios.get(`/api/knitting-machine/new`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                filterData = _.clone(masterData);
                masterData.YarnPRRequiredDate = formatDateToDefault(masterData.YarnPRRequiredDate);
                masterData.YarnPRDate = formatDateToDefault(masterData.YarnPRDate);
                masterData.MachineSubClassList = [];


                setFormData($formEl, masterData);
                initChildTable();
            })
            .catch(showResponseError);
    }

    function getDetails(id) {
        axios.get(`/api/knitting-machine/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                filterData = _.clone(masterData);
                masterData.YarnPRRequiredDate = formatDateToDefault(masterData.ManufacturerDate);
                masterData.YarnPRRequiredDate = formatDateToDefault(masterData.ErectionDate);

                setFormData($formEl, masterData);
                toggleFlatKnitControls(masterData.MachineNatureID);
                initChildTable();
                $tblChildEl.bootstrapTable("load", masterData.KnittingMachineOptions);
            })
            .catch(showResponseError);
    }

    function toggleFlatKnitControls(machineNatureId) {
        if (!machineNatureId) return false;
        var machineNature = filterData.MachineNatureList.find(function (el) { return el.id == machineNatureId });
        var isFlatKnit = machineNature && machineNature.text == "Flat";
        if (isFlatKnit) {
            $("#div-flat-knit-only").show();
            $("#div-child-tbl").hide();
        }
        else {
            $("#div-flat-knit-only").show();
            $("#div-child-tbl").hide();
        }
    }

})();
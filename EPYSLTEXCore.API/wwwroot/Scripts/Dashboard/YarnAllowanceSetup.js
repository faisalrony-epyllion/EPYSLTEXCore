var formYarnQtySetupMaster;
var formFabricConstructionSetupMaster;
var formFabricDyeingSetupMaster;
var formFabricFinishingSetupMaster;
var formFabricGSMSetupMaster;
var formYarnProgramSetupMaster;
var formCollarCuffSetupMaster;
var formYarnCategorySetupMaster;
var buyerLists = [];
var filterBy = {};
var yarnqtysetupmaster;
var fabricconstructionsetupmaster;
var fabricdyeingsetupmaster;
var fabricfinishingsetupmaster;
var fabricgsmsetupmaster;
var yarnprogramsetupmaster;
var collarcuffsetupmaster;
var yarncategorysetupmaster;
var tableParams = {
    offset: 0,
    limit: 10,
    sort: '',
    order: '',
    filter: ''
}

$(function () {
    formYarnQtySetupMaster = $("#formYarnQtySetupMaster");
    formFabricConstructionSetupMaster = $("#formFabricConstructionSetupMaster");
    formFabricDyeingSetupMaster = $("#formFabricDyeingSetupMaster");
    formFabricFinishingSetupMaster = $("#formFabricFinishingSetupMaster");
    formFabricGSMSetupMaster = $("#formFabricGSMSetupMaster");
    formYarnProgramSetupMaster = $("#formYarnProgramSetupMaster");
    formCollarCuffSetupMaster = $("#formCollarCuffSetupMaster");
    formYarnCategorySetupMaster = $("#formYarnCategorySetupMaster");

    getYarnQtySetupData();
    getFabricConstructionSetupData();
    getFabricDyeingSetupData();
    getFabricFinishingSetupData();
    getFabricGSMSetupData();
    getYarnProgramSetupData();
    getCollarCuffSetupData();
    getYarnCategorySetupData();

    initYarnQtySetupMasterTable();
    initFabricConstructionSetupMasterTable();
    initFabricDyeingSetupMasterTable();
    initFabricFinishingSetupMasterTable();
    initFabricGSMSetupMasterTable();
    initYarnProgramSetupMasterTable();
    initCollarCuffSetupMasterTable();
    initYarnCategorySetupMasterTable();

    formYarnQtySetupMaster.find("#BuyerId").on("select2:select", function (e) {
        var id = e.params.data.id;
        var buyerName = e.params.data.text;
        yarnPOQtySetupChildItem = {
            Id: id,
            BuyerId: id,
            BuyerName: buyerName
        };

        yarnqtysetupmaster.YaYarnQtyChildSetups.push(yarnPOQtySetupChildItem);
        $("#tblYarnQtySetupChild").bootstrapTable('load', yarnqtysetupmaster.YaYarnQtyChildSetups);
    });

    formFabricConstructionSetupMaster.find("#BuyerId").on("select2:select", function (e) {
        var id = e.params.data.id;
        var buyerName = e.params.data.text;
        fabricConstructionSetupChildItem = {
            Id: id,
            BuyerId: id,
            BuyerName: buyerName
        };

        fabricconstructionsetupmaster.YaFabricConstructionSetupChilds.push(fabricConstructionSetupChildItem);
        $("#tblFabricConstructionSetupChild").bootstrapTable('load', fabricconstructionsetupmaster.YaFabricConstructionSetupChilds);
    });

    formFabricDyeingSetupMaster.find("#BuyerId").on("select2:select", function (e) {
        var id = e.params.data.id;
        var buyerName = e.params.data.text;
        fabricDyeingSetupChildItem = {
            Id: id,
            BuyerId: id,
            BuyerName: buyerName
        };

        fabricdyeingsetupmaster.YaDyeingTypeSetupChilds.push(fabricDyeingSetupChildItem);
        $("#tblFabricDyeingSetupChild").bootstrapTable('load', fabricdyeingsetupmaster.YaDyeingTypeSetupChilds);
    });

    formFabricFinishingSetupMaster.find("#BuyerId").on("select2:select", function (e) {
        var id = e.params.data.id;
        var buyerName = e.params.data.text;
        fabricFinishingSetupChildItem = {
            Id: id,
            BuyerId: id,
            BuyerName: buyerName
        };

        fabricfinishingsetupmaster.YaFinishingTypeSetupChilds.push(fabricFinishingSetupChildItem);
        $("#tblFabricFinishingSetupChild").bootstrapTable('load', fabricfinishingsetupmaster.YaFinishingTypeSetupChilds);
    });

    formFabricGSMSetupMaster.find("#BuyerId").on("select2:select", function (e) {
        var id = e.params.data.id;
        var buyerName = e.params.data.text;
        fabricGSMSetupChildItem = {
            Id: id,
            BuyerId: id,
            BuyerName: buyerName
        };

        fabricgsmsetupmaster.YaFabricGsmSetupChilds.push(fabricGSMSetupChildItem);
        $("#tblFabricGSMSetupChild").bootstrapTable('load', fabricgsmsetupmaster.YaFabricGsmSetupChilds);
    });

    formYarnProgramSetupMaster.find("#BuyerId").on("select2:select", function (e) {
        var id = e.params.data.id;
        var buyerName = e.params.data.text;
        yarnProgramSetupChildItem = {
            Id: id,
            BuyerId: id,
            BuyerName: buyerName
        };

        yarnprogramsetupmaster.YaYarnProgramSetupChilds.push(yarnProgramSetupChildItem);
        $("#tblYarnProgramSetupChild").bootstrapTable('load', yarnprogramsetupmaster.YaYarnProgramSetupChilds);
    });

    formCollarCuffSetupMaster.find("#BuyerId").on("select2:select", function (e) {
        var id = e.params.data.id;
        var buyerName = e.params.data.text;
        collarCuffSetupChildItem = {
            Id: id,
            BuyerId: id,
            BuyerName: buyerName
        };

        collarcuffsetupmaster.YaCollarCuffSetupChilds.push(collarCuffSetupChildItem);
        $("#tblCollarCuffSetupChild").bootstrapTable('load', collarcuffsetupmaster.YaCollarCuffSetupChilds);
    });

    formYarnCategorySetupMaster.find("#BuyerId").on("select2:select", function (e) {
        var id = e.params.data.id;
        var buyerName = e.params.data.text;
        yarnCategorySetupChildItem = {
            Id: id,
            BuyerId: id,
            BuyerName: buyerName
        };

        yarncategorysetupmaster.YaYarnCategorySetupChilds.push(yarnCategorySetupChildItem);
        $("#tblYarnCategorySetupChild").bootstrapTable('load', yarncategorysetupmaster.YaYarnCategorySetupChilds);
    });

    $("#btnNewYarnQty").on("click", function (e) {
        e.preventDefault();

        resetFormYarnQtySetup();
        $("#divtblYarnQtySetup").fadeOut();
        $("#divEditYarnQtySetup").fadeIn();
        $("#divButtonExecutionsYarnQtySetup").fadeIn();

        getNewYarnQtySetup();
    });

    $("#btnNewFabricConstruction").on("click", function (e) {
        e.preventDefault();

        resetFormFabricConstructionSetup();
        $("#divtblFabricConstructionSetup").fadeOut();
        $("#divEditFabricConstructionSetup").fadeIn();
        $("#divButtonExecutionsFabricConstructionSetup").fadeIn();

        getNewFabricConstructionSetup();
    });

    $("#btnNewFabricDyeing").on("click", function (e) {
        e.preventDefault();

        resetFormFabricDyeingSetup();
        $("#divtblFabricDyeingSetup").fadeOut();
        $("#divEditFabricDyeingSetup").fadeIn();
        $("#divButtonExecutionsFabricDyeingSetup").fadeIn();

        getNewFabricDyeingSetup();
    });

    $("#btnNewFabricFinishing").on("click", function (e) {
        e.preventDefault();

        resetFormFabricFinishingSetup();
        $("#divtblFabricFinishingSetup").fadeOut();
        $("#divEditFabricFinishingSetup").fadeIn();
        $("#divButtonExecutionsFabricFinishingSetup").fadeIn();

        getNewFabricFinishingSetup();
    });

    $("#btnNewFabricGSM").on("click", function (e) {
        e.preventDefault();

        resetFormFabricGSMSetup();
        $("#divtblFabricGSMSetup").fadeOut();
        $("#divEditFabricGSMSetup").fadeIn();
        $("#divButtonExecutionsFabricGSMSetup").fadeIn();

        getNewFabricGSMSetup();
    });

    $("#btnNewYarnProgram").on("click", function (e) {
        e.preventDefault();

        resetFormYarnProgramSetup();
        $("#divtblYarnProgramSetup").fadeOut();
        $("#divEditYarnProgramSetup").fadeIn();
        $("#divButtonExecutionsYarnProgramSetup").fadeIn();

        getNewYarnProgramSetup();
    });

    $("#btnNewCollarCuff").on("click", function (e) {
        e.preventDefault();

        resetFormCollarCuffSetup();
        $("#divtblCollarCuffSetup").fadeOut();
        $("#divEditCollarCuffSetup").fadeIn();
        $("#divButtonExecutionsCollarCuffSetup").fadeIn();

        getNewCollarCuffSetup();
    });

    $("#btnNewYarnCategory").on("click", function (e) {
        e.preventDefault();

        resetFormYarnCategorySetup();
        $("#divtblYarnCategorySetup").fadeOut();
        $("#divEditYarnCategorySetup").fadeIn();
        $("#divButtonExecutionsYarnCategorySetup").fadeIn();

        getNewYarnCategorySetup();
    });

    $("#btnSaveYarnQtySetup").click(function (e) {
        e.preventDefault();
        var data = formDataToJson(formYarnQtySetupMaster.serializeArray());
        data["YaYarnQtyChildSetups"] = yarnqtysetupmaster.YaYarnQtyChildSetups;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/YarnAllowanceApi/yarnQtySetup", data, config)
            .then(function () {
                toastr.success("Your processes saved successfully.");
                backToListYarnQtySetup();
            })
            .catch(showResponseError);
    });

    $("#btnSaveFabricConstructionSetup").click(function (e) {
        e.preventDefault();
        var data = formDataToJson(formFabricConstructionSetupMaster.serializeArray());
        data["YaFabricConstructionSetupChilds"] = fabricconstructionsetupmaster.YaFabricConstructionSetupChilds;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/YarnAllowanceApi/fabricConstructionSetup", data, config)
            .then(function () {
                toastr.success("Your processes saved successfully.");
                backToListFabricConstructionSetup();
            })
            .catch(showResponseError);
    });

    $("#btnSaveFabricDyeingSetup").click(function (e) {
        e.preventDefault();
        var data = formDataToJson(formFabricDyeingSetupMaster.serializeArray());
        data["YaDyeingTypeSetupChilds"] = fabricdyeingsetupmaster.YaDyeingTypeSetupChilds;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/YarnAllowanceApi/fabricDyeingSetup", data, config)
            .then(function () {
                toastr.success("Your processes saved successfully.");
                backToListFabricDyeingSetup();
            })
            .catch(showResponseError);
    });

    $("#btnSaveFabricFinishingSetup").click(function (e) {
        e.preventDefault();
        var data = formDataToJson(formFabricFinishingSetupMaster.serializeArray());
        data["YaFinishingTypeSetupChilds"] = fabricfinishingsetupmaster.YaFinishingTypeSetupChilds;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/YarnAllowanceApi/fabricFinishingSetup", data, config)
            .then(function () {
                toastr.success("Your processes saved successfully.");
                backToListFabricFinishingSetup();
            })
            .catch(showResponseError);
    });

    $("#btnSaveFabricGSMSetup").click(function (e) {
        e.preventDefault();
        var data = formDataToJson(formFabricGSMSetupMaster.serializeArray());
        data["YaFabricGsmSetupChilds"] = fabricgsmsetupmaster.YaFabricGsmSetupChilds;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/YarnAllowanceApi/fabricGSMSetup", data, config)
            .then(function () {
                toastr.success("Your processes saved successfully.");
                backToListFabricGSMSetup();
            })
            .catch(showResponseError);
    });

    $("#btnSaveYarnProgramSetup").click(function (e) {
        e.preventDefault();
        var data = formDataToJson(formYarnProgramSetupMaster.serializeArray());
        data["YaYarnProgramSetupChilds"] = yarnprogramsetupmaster.YaYarnProgramSetupChilds;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/YarnAllowanceApi/yarnProgramSetup", data, config)
            .then(function () {
                toastr.success("Your processes saved successfully.");
                backToListYarnProgramSetup();
            })
            .catch(showResponseError);
    });

    $("#btnSaveCollarCuffSetup").click(function (e) {
        e.preventDefault();
        var data = formDataToJson(formCollarCuffSetupMaster.serializeArray());
        data["YaCollarCuffSetupChilds"] = collarcuffsetupmaster.YaCollarCuffSetupChilds;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/YarnAllowanceApi/collarCuffSetup", data, config)
            .then(function () {
                toastr.success("Your processes saved successfully.");
                backToListCollarCuffSetup();
            })
            .catch(showResponseError);
    });

    $("#btnSaveYarnCategorySetup").click(function (e) {
        e.preventDefault();
        var data = formDataToJson(formYarnCategorySetupMaster.serializeArray());
        data["YaYarnCategorySetupChilds"] = yarncategorysetupmaster.YaYarnCategorySetupChilds;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/YarnAllowanceApi/yarnCategorySetup", data, config)
            .then(function () {
                toastr.success("Your processes saved successfully.");
                backToListYarnCategorySetup();
            })
            .catch(showResponseError);
    });

    $("#btnEditCancelYarnQtySetup").on("click", function (e) {
        e.preventDefault();
        backToListYarnQtySetup();
    });

    $("#btnEditCancelFabricConstructionSetup").on("click", function (e) {
        e.preventDefault();
        backToListFabricConstructionSetup();
    });

    $("#btnEditCancelFabricDyeingSetup").on("click", function (e) {
        e.preventDefault();
        backToListFabricDyeingSetup();
    });

    $("#btnEditCancelFabricFinishingSetup").on("click", function (e) {
        e.preventDefault();
        backToListFabricFinishingSetup();
    });

    $("#btnEditCancelFabricGSMSetup").on("click", function (e) {
        e.preventDefault();
        backToListFabricGSMSetup();
    });

    $("#btnEditCancelYarnProgramSetup").on("click", function (e) {
        e.preventDefault();
        backToListYarnProgramSetup();
    });

    $("#btnEditCancelCollarCuffSetup").on("click", function (e) {
        e.preventDefault();
        backToListCollarCuffSetup();
    });

    $("#btnEditCancelYarnCategorySetup").on("click", function (e) {
        e.preventDefault();
        backToListYarnCategorySetup();
    });
});

function getYarnQtySetupData() {
    var queryParams = $.param(tableParams);
    $('#tblYarnQtySetupMaster').bootstrapTable('showLoading');
    var url = "/YarnAllowanceApi/yarnQtySetup" + "?" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblYarnQtySetupMaster").bootstrapTable('load', response.data);
            $('#tblYarnQtySetupMaster').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getFabricConstructionSetupData() {
    var queryParams = $.param(tableParams);
    $('#tblFabricConstructionSetupMaster').bootstrapTable('showLoading');
    var url = "/YarnAllowanceApi/fabricConstructionSetup" + "?" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblFabricConstructionSetupMaster").bootstrapTable('load', response.data);
            $('#tblFabricConstructionSetupMaster').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getFabricDyeingSetupData() {
    var queryParams = $.param(tableParams);
    $('#tblFabricDyeingSetupMaster').bootstrapTable('showLoading');
    var url = "/YarnAllowanceApi/fabricDyeingSetup" + "?" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblFabricDyeingSetupMaster").bootstrapTable('load', response.data);
            $('#tblFabricDyeingSetupMaster').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getFabricFinishingSetupData() {
    var queryParams = $.param(tableParams);
    $('#tblFabricFinishingSetupMaster').bootstrapTable('showLoading');
    var url = "/YarnAllowanceApi/fabricFinishingSetup" + "?" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblFabricFinishingSetupMaster").bootstrapTable('load', response.data);
            $('#tblFabricFinishingSetupMaster').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getFabricGSMSetupData() {
    var queryParams = $.param(tableParams);
    $('#tblFabricGSMSetupMaster').bootstrapTable('showLoading');
    var url = "/YarnAllowanceApi/fabricGSMSetup" + "?" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblFabricGSMSetupMaster").bootstrapTable('load', response.data);
            $('#tblFabricGSMSetupMaster').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getYarnProgramSetupData() {
    var queryParams = $.param(tableParams);
    $('#tblYarnProgramSetupMaster').bootstrapTable('showLoading');
    var url = "/YarnAllowanceApi/yarnProgramSetup" + "?" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblYarnProgramSetupMaster").bootstrapTable('load', response.data);
            $('#tblYarnProgramSetupMaster').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getCollarCuffSetupData() {
    var queryParams = $.param(tableParams);
    $('#tblCollarCuffSetupMaster').bootstrapTable('showLoading');
    var url = "/YarnAllowanceApi/collarCuffSetup" + "?" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblCollarCuffSetupMaster").bootstrapTable('load', response.data);
            $('#tblCollarCuffSetupMaster').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getYarnCategorySetupData() {
    var queryParams = $.param(tableParams);
    $('#tblYarnCategorySetupMaster').bootstrapTable('showLoading');
    var url = "/YarnAllowanceApi/yarnCategorySetup" + "?" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblYarnCategorySetupMaster").bootstrapTable('load', response.data);
            $('#tblYarnCategorySetupMaster').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function initYarnQtySetupMasterTable() {
    $("#tblYarnQtySetupMaster").bootstrapTable('destroy');
    $("#tblYarnQtySetupMaster").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#YarnQtySetupToolbar",
        exportTypes: "['csv', 'excel']",
        pagination: true,
        filterControl: true,
        searchOnEnterKey: true,
        sidePagination: "server",
        pageList: "[10, 25, 50, 100, 500]",
        cache: false,
        columns: [
            {
                title: 'Actions',
                align: 'center',
                width: 50,
                formatter: function () {
                    return [
                        '<span class="btn-group">',
                        '<a class="btn btn-default btn-xs edit" href="javascript:void(0)" title="View Setup">',
                        '<i class="fa fa-edit"></i>',
                        '</a>',
                        '</span>'
                    ].join('');
                },
                events: {
                    'click .edit': function (e, value, row, index) {
                        $("#YQSID").val(row.YQSID);
                        getYarnQtySetupDataEdit(row.YQSID);
                        initYarnQtySetupChildTable();
                    }
                }
            },
            {
                field: "YqFrom",
                title: "YQ From (Kg)",
                align: 'center',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "YqTo",
                title: "YQ To (Kg)",
                align: 'center',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "AddYq",
                title: "Add. Yarn Qty (Kg)",
                align: 'center',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "YAllowance",
                title: "Yarn Allowance (%)",
                align: 'center',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "AddYAllowance",
                title: "Add. Allowance (%)",
                align: 'center',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerNames",
                title: "Buyer Names",
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

            getYarnQtySetupData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getYarnQtySetupData();
        },
        onRefresh: function () {
            resetTableParams();
            getYarnQtySetupData();
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

            getYarnQtySetupData();
        }
    });
}

function initFabricConstructionSetupMasterTable() {
    $("#tblFabricConstructionSetupMaster").bootstrapTable('destroy');
    $("#tblFabricConstructionSetupMaster").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#FabricConstructionSetupToolbar",
        exportTypes: "['csv', 'excel']",
        pagination: true,
        filterControl: true,
        searchOnEnterKey: true,
        sidePagination: "server",
        pageList: "[10, 25, 50, 100, 500]",
        cache: false,
        columns: [
            {
                title: 'Actions',
                align: 'center',
                width: 50,
                formatter: function () {
                    return [
                        '<span class="btn-group">',
                        '<a class="btn btn-default btn-xs edit" href="javascript:void(0)" title="View Setup">',
                        '<i class="fa fa-edit"></i>',
                        '</a>',
                        '</span>'
                    ].join('');
                },
                events: {
                    'click .edit': function (e, value, row, index) {
                        $("#FCSID").val(row.FCSID);
                        getFabricConstructionSetupDataEdit(row.FCSID);
                        initFabricConstructionSetupChildTable();
                    }
                }
            },
            {
                field: "TechnicalName",
                title: "Technical Name",
                align: 'left',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "YAllowance",
                title: "Yarn Allowance (%)",
                align: 'center',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerNames",
                title: "Buyer Names",
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

            getFabricConstructionSetupData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getFabricConstructionSetupData();
        },
        onRefresh: function () {
            resetTableParams();
            getFabricConstructionSetupData();
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

            getFabricConstructionSetupData();
        }
    });
}

function initFabricDyeingSetupMasterTable() {
    $("#tblFabricDyeingSetupMaster").bootstrapTable('destroy');
    $("#tblFabricDyeingSetupMaster").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#FabricDyeingSetupToolbar",
        exportTypes: "['csv', 'excel']",
        pagination: true,
        filterControl: true,
        searchOnEnterKey: true,
        sidePagination: "server",
        pageList: "[10, 25, 50, 100, 500]",
        cache: false,
        columns: [
            {
                title: 'Actions',
                align: 'center',
                width: 50,
                formatter: function () {
                    return [
                        '<span class="btn-group">',
                        '<a class="btn btn-default btn-xs edit" href="javascript:void(0)" title="View Setup">',
                        '<i class="fa fa-edit"></i>',
                        '</a>',
                        '</span>'
                    ].join('');
                },
                events: {
                    'click .edit': function (e, value, row, index) {
                        $("#DTSID").val(row.DTSID);
                        getFabricDyeingSetupDataEdit(row.DTSID);
                        initFabricDyeingSetupChildTable();
                    }
                }
            },
            {
                field: "DyeingType",
                title: "Dyeing Type",
                align: 'left',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "YAllowance",
                title: "Yarn Allowance (%)",
                align: 'center',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerNames",
                title: "Buyer Names",
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

            getFabricDyeingSetupData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getFabricDyeingSetupData();
        },
        onRefresh: function () {
            resetTableParams();
            getFabricDyeingSetupData();
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

            getFabricDyeingSetupData();
        }
    });
}

function initFabricFinishingSetupMasterTable() {
    $("#tblFabricFinishingSetupMaster").bootstrapTable('destroy');
    $("#tblFabricFinishingSetupMaster").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#FabricFinishingSetupToolbar",
        exportTypes: "['csv', 'excel']",
        pagination: true,
        filterControl: true,
        searchOnEnterKey: true,
        sidePagination: "server",
        pageList: "[10, 25, 50, 100, 500]",
        cache: false,
        columns: [
            {
                title: 'Actions',
                align: 'center',
                width: 50,
                formatter: function () {
                    return [
                        '<span class="btn-group">',
                        '<a class="btn btn-default btn-xs edit" href="javascript:void(0)" title="View Setup">',
                        '<i class="fa fa-edit"></i>',
                        '</a>',
                        '</span>'
                    ].join('');
                },
                events: {
                    'click .edit': function (e, value, row, index) {
                        $("#FTSID").val(row.FTSID);
                        getFabricFinishingSetupDataEdit(row.FTSID);
                        initFabricFinishingSetupChildTable();
                    }
                }
            },
            {
                field: "FinishingTypeName",
                title: "Finishing Process",
                align: 'left',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "YAllowance",
                title: "Yarn Allowance (%)",
                align: 'center',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerNames",
                title: "Buyer Names",
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

            getFabricFinishingSetupData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getFabricFinishingSetupData();
        },
        onRefresh: function () {
            resetTableParams();
            getFabricFinishingSetupData();
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

            getFabricFinishingSetupData();
        }
    });
}

function initFabricGSMSetupMasterTable() {
    $("#tblFabricGSMSetupMaster").bootstrapTable('destroy');
    $("#tblFabricGSMSetupMaster").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#FabricGSMSetupToolbar",
        exportTypes: "['csv', 'excel']",
        pagination: true,
        filterControl: true,
        searchOnEnterKey: true,
        sidePagination: "server",
        pageList: "[10, 25, 50, 100, 500]",
        cache: false,
        columns: [
            {
                title: 'Actions',
                align: 'center',
                width: 50,
                formatter: function () {
                    return [
                        '<span class="btn-group">',
                        '<a class="btn btn-default btn-xs edit" href="javascript:void(0)" title="View Setup">',
                        '<i class="fa fa-edit"></i>',
                        '</a>',
                        '</span>'
                    ].join('');
                },
                events: {
                    'click .edit': function (e, value, row, index) {
                        $("#FGSMSID").val(row.FGSMSID);
                        getFabricGSMSetupDataEdit(row.FGSMSID);
                        initFabricGSMSetupChildTable();
                    }
                }
            },
            {
                field: "GsmFrom",
                title: "GSM From",
                align: 'left',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "GsmTo",
                title: "GSM To",
                align: 'left',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "AddYAllowance",
                title: "Yarn Allowance (%)",
                align: 'center',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerNames",
                title: "Buyer Names",
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

            getFabricGSMSetupData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getFabricGSMSetupData();
        },
        onRefresh: function () {
            resetTableParams();
            getFabricGSMSetupData();
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

            getFabricGSMSetupData();
        }
    });
}

function initYarnProgramSetupMasterTable() {
    $("#tblYarnProgramSetupMaster").bootstrapTable('destroy');
    $("#tblYarnProgramSetupMaster").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#YarnProgramSetupToolbar",
        exportTypes: "['csv', 'excel']",
        pagination: true,
        filterControl: true,
        searchOnEnterKey: true,
        sidePagination: "server",
        pageList: "[10, 25, 50, 100, 500]",
        cache: false,
        columns: [
            {
                title: 'Actions',
                align: 'center',
                width: 50,
                formatter: function () {
                    return [
                        '<span class="btn-group">',
                        '<a class="btn btn-default btn-xs edit" href="javascript:void(0)" title="View Setup">',
                        '<i class="fa fa-edit"></i>',
                        '</a>',
                        '</span>'
                    ].join('');
                },
                events: {
                    'click .edit': function (e, value, row, index) {
                        $("#YPSID").val(row.YPSID);
                        getYarnProgramSetupDataEdit(row.YPSID);
                        initYarnProgramSetupChildTable();
                    }
                }
            },
            {
                field: "YarnProgram",
                title: "Yarn Program",
                align: 'left',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "ShadeName",
                title: "Shade",
                align: 'left',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "AddYAllowance",
                title: "Yarn Allowance (%)",
                align: 'center',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerNames",
                title: "Buyer Names",
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

            getYarnProgramSetupData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getYarnProgramSetupData();
        },
        onRefresh: function () {
            resetTableParams();
            getYarnProgramSetupData();
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

            getYarnProgramSetupData();
        }
    });
}

function initCollarCuffSetupMasterTable() {
    $("#tblCollarCuffSetupMaster").bootstrapTable('destroy');
    $("#tblCollarCuffSetupMaster").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#CollarCuffSetupToolbar",
        exportTypes: "['csv', 'excel']",
        pagination: true,
        filterControl: true,
        searchOnEnterKey: true,
        sidePagination: "server",
        pageList: "[10, 25, 50, 100, 500]",
        cache: false,
        columns: [
            {
                title: 'Actions',
                align: 'center',
                width: 50,
                formatter: function () {
                    return [
                        '<span class="btn-group">',
                        '<a class="btn btn-default btn-xs edit" href="javascript:void(0)" title="View Setup">',
                        '<i class="fa fa-edit"></i>',
                        '</a>',
                        '</span>'
                    ].join('');
                },
                events: {
                    'click .edit': function (e, value, row, index) {
                        $("#CCSID").val(row.CCSID);
                        getCollarCuffSetupDataEdit(row.CCSID);
                        initCollarCuffSetupChildTable();
                    }
                }
            },
            {
                field: "ItemName",
                title: "Item Name",
                align: 'left',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "SolidAllowance",
                title: "Solid Allowance (%)",
                align: 'left',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "YdAllowance",
                title: "Y/D Allowance (%)",
                align: 'center',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerNames",
                title: "Buyer Names",
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

            getCollarCuffSetupData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getCollarCuffSetupData();
        },
        onRefresh: function () {
            resetTableParams();
            getCollarCuffSetupData();
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

            getCollarCuffSetupData();
        }
    });
}

function initYarnCategorySetupMasterTable() {
    $("#tblYarnCategorySetupMaster").bootstrapTable('destroy');
    $("#tblYarnCategorySetupMaster").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#YarnCategorySetupToolbar",
        exportTypes: "['csv', 'excel']",
        pagination: true,
        filterControl: true,
        searchOnEnterKey: true,
        sidePagination: "server",
        pageList: "[10, 25, 50, 100, 500]",
        cache: false,
        columns: [
            {
                title: 'Actions',
                align: 'center',
                width: 50,
                formatter: function () {
                    return [
                        '<span class="btn-group">',
                        '<a class="btn btn-default btn-xs edit" href="javascript:void(0)" title="View Setup">',
                        '<i class="fa fa-edit"></i>',
                        '</a>',
                        '</span>'
                    ].join('');
                },
                events: {
                    'click .edit': function (e, value, row, index) {
                        $("#YCSID").val(row.YCSID);
                        getYarnCategorySetupDataEdit(row.YCSID);
                        initYarnCategorySetupChildTable();
                    }
                }
            },
            {
                field: "YarnCategory",
                title: "Yarn Category",
                align: 'left',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "YAllowance",
                title: "Allowance (%)",
                align: 'left',
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerNames",
                title: "Buyer Names",
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

            getYarnCategorySetupData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getYarnCategorySetupData();
        },
        onRefresh: function () {
            resetTableParams();
            getYarnCategorySetupData();
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

            getYarnCategorySetupData();
        }
    });
}

function getNewYarnQtySetup() {
    url = "/YarnAllowanceApi/newYarnQtySetup/";

    axios.get(url)
        .then(function (response) {
            yarnqtysetupmaster = response.data;
            initSelect2(formYarnQtySetupMaster.find("#BuyerId"), response.data.BuyerList);

            initYarnQtySetupChildTable();
            $("#tblYarnQtySetupChild").bootstrapTable('load', response.data.YaYarnQtyChildSetups);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getNewFabricConstructionSetup() {
    url = "/YarnAllowanceApi/newFabricConstructionSetup/";

    axios.get(url)
        .then(function (response) {
            fabricconstructionsetupmaster = response.data;
            initSelect2(formFabricConstructionSetupMaster.find("#BuyerId"), response.data.BuyerList);
            initSelect2(formFabricConstructionSetupMaster.find("#TechnicalNameId"), response.data.FabricTechnicalNameList);

            initFabricConstructionSetupChildTable();
            $("#tblFabricConstructionSetupChild").bootstrapTable('load', response.data.YaFabricConstructionSetupChilds);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getNewFabricDyeingSetup() {
    url = "/YarnAllowanceApi/newFabricDyeingSetup/";

    axios.get(url)
        .then(function (response) {
            fabricdyeingsetupmaster = response.data;
            initSelect2(formFabricDyeingSetupMaster.find("#BuyerId"), response.data.BuyerList);
            initSelect2(formFabricDyeingSetupMaster.find("#DyeingTypeId"), response.data.DyeingTypeList);

            initFabricDyeingSetupChildTable();
            $("#tblFabricDyeingSetupChild").bootstrapTable('load', response.data.YaDyeingTypeSetupChilds);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getNewFabricFinishingSetup() {
    url = "/YarnAllowanceApi/newFabricFinishingSetup/";

    axios.get(url)
        .then(function (response) {
            fabricfinishingsetupmaster = response.data;
            initSelect2(formFabricFinishingSetupMaster.find("#BuyerId"), response.data.BuyerList);
            initSelect2(formFabricFinishingSetupMaster.find("#FinishingType"), response.data.FinishingTypeList);

            initFabricFinishingSetupChildTable();
            $("#tblFabricFinishingSetupChild").bootstrapTable('load', response.data.YaFinishingTypeSetupChilds);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getNewFabricGSMSetup() {
    url = "/YarnAllowanceApi/newFabricGSMSetup/";

    axios.get(url)
        .then(function (response) {
            fabricgsmsetupmaster = response.data;
            initSelect2(formFabricGSMSetupMaster.find("#BuyerId"), response.data.BuyerList);

            initFabricGSMSetupChildTable();
            $("#tblFabricGSMSetupChild").bootstrapTable('load', response.data.YaFabricGsmSetupChilds);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getNewYarnProgramSetup() {
    url = "/YarnAllowanceApi/newYarnProgramSetup/";

    axios.get(url)
        .then(function (response) {
            yarnprogramsetupmaster = response.data;
            initSelect2(formYarnProgramSetupMaster.find("#BuyerId"), response.data.BuyerList);
            initSelect2(formYarnProgramSetupMaster.find("#YarnProgramId"), response.data.YarnProgramList);
            initSelect2(formYarnProgramSetupMaster.find("#ShadeId"), response.data.ShadeList);

            initYarnProgramSetupChildTable();
            $("#tblYarnProgramSetupChild").bootstrapTable('load', response.data.YaYarnProgramSetupChilds);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getNewCollarCuffSetup() {
    url = "/YarnAllowanceApi/newCollarCuffSetup/";

    axios.get(url)
        .then(function (response) {
            collarcuffsetupmaster = response.data;
            initSelect2(formCollarCuffSetupMaster.find("#BuyerId"), response.data.BuyerList);
            initSelect2(formCollarCuffSetupMaster.find("#ItemId"), response.data.CollarCuffList);

            initCollarCuffSetupChildTable();
            $("#tblCollarCuffSetupChild").bootstrapTable('load', response.data.YaCollarCuffSetupChilds);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getNewYarnCategorySetup() {
    url = "/YarnAllowanceApi/newYarnCategorySetup/";

    axios.get(url)
        .then(function (response) {
            yarncategorysetupmaster = response.data;
            initSelect2(formYarnCategorySetupMaster.find("#BuyerId"), response.data.BuyerList);

            initYarnCategorySetupChildTable();
            $("#tblYarnCategorySetupChild").bootstrapTable('load', response.data.YaYarnCategorySetupChilds);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getYarnQtySetupDataEdit(YQSID) {
    var url = "";
    url = "/YarnAllowanceApi/yarnQtySetupEdit/" + YQSID;
    axios.get(url)
        .then(function (response) {
            yarnqtysetupmaster = response.data;
            formYarnQtySetupMaster.find("#Id").val(response.data.Id);
            formYarnQtySetupMaster.find("#YQFrom").val(response.data.YqFrom);
            formYarnQtySetupMaster.find("#YQTo").val(response.data.YqTo);
            formYarnQtySetupMaster.find("#AddYQ").val(response.data.AddYq);
            formYarnQtySetupMaster.find("#YAllowance").val(response.data.YAllowance);
            formYarnQtySetupMaster.find("#AddYAllowance").val(response.data.AddYAllowance);

            initSelect2(formYarnQtySetupMaster.find("#BuyerId"), response.data.BuyerList);

            $("#divEditYarnQtySetup").fadeIn();
            $("#divtblYarnQtySetup").fadeOut();
            $("#divButtonExecutionsYarnQtySetup").fadeIn();

            $("#tblYarnQtySetupChild").bootstrapTable('load', response.data.YaYarnQtyChildSetups);
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function getFabricConstructionSetupDataEdit(FCSID) {
    var url = "";
    url = "/YarnAllowanceApi/fabricConstructionSetupEdit/" + FCSID;
    axios.get(url)
        .then(function (response) {
            fabricconstructionsetupmaster = response.data;
            formFabricConstructionSetupMaster.find("#Id").val(response.data.Id);
            formFabricConstructionSetupMaster.find("#TechnicalNameId").val(response.data.TechnicalNameId);
            formFabricConstructionSetupMaster.find("#YAllowance").val(response.data.YAllowance);

            initSelect2(formFabricConstructionSetupMaster.find("#TechnicalNameId"), response.data.FabricTechnicalNameList);
            formFabricConstructionSetupMaster.find("#TechnicalNameId").val(response.data.TechnicalNameId).trigger("change");
            initSelect2(formFabricConstructionSetupMaster.find("#BuyerId"), response.data.BuyerList);

            $("#divEditFabricConstructionSetup").fadeIn();
            $("#divtblFabricConstructionSetup").fadeOut();
            $("#divButtonExecutionsFabricConstructionSetup").fadeIn();

            $("#tblFabricConstructionSetupChild").bootstrapTable('load', response.data.YaFabricConstructionSetupChilds);
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function getFabricDyeingSetupDataEdit(FCSID) {
    var url = "";
    url = "/YarnAllowanceApi/fabricDyeingSetupEdit/" + FCSID;
    axios.get(url)
        .then(function (response) {
            fabricdyeingsetupmaster = response.data;
            formFabricDyeingSetupMaster.find("#Id").val(response.data.Id);
            formFabricDyeingSetupMaster.find("#DyeingTypeId").val(response.data.DyeingTypeId);
            formFabricDyeingSetupMaster.find("#YAllowance").val(response.data.YAllowance);

            initSelect2(formFabricDyeingSetupMaster.find("#DyeingTypeId"), response.data.DyeingTypeList);
            formFabricDyeingSetupMaster.find("#DyeingTypeId").val(response.data.DyeingTypeId).trigger("change");
            initSelect2(formFabricDyeingSetupMaster.find("#BuyerId"), response.data.BuyerList);

            $("#divEditFabricDyeingSetup").fadeIn();
            $("#divtblFabricDyeingSetup").fadeOut();
            $("#divButtonExecutionsFabricDyeingSetup").fadeIn();

            $("#tblFabricDyeingSetupChild").bootstrapTable('load', response.data.YaDyeingTypeSetupChilds);
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function getFabricFinishingSetupDataEdit(FCSID) {
    var url = "";
    url = "/YarnAllowanceApi/fabricFinishingSetupEdit/" + FCSID;
    axios.get(url)
        .then(function (response) {
            fabricfinishingsetupmaster = response.data;
            formFabricFinishingSetupMaster.find("#Id").val(response.data.Id);
            formFabricFinishingSetupMaster.find("#FinishingType").val(response.data.FinishingType);
            formFabricFinishingSetupMaster.find("#YAllowance").val(response.data.YAllowance);

            initSelect2(formFabricFinishingSetupMaster.find("#FinishingType"), response.data.FinishingTypeList);
            formFabricFinishingSetupMaster.find("#FinishingType").val(response.data.FinishingType).trigger("change");
            initSelect2(formFabricFinishingSetupMaster.find("#BuyerId"), response.data.BuyerList);

            $("#divEditFabricFinishingSetup").fadeIn();
            $("#divtblFabricFinishingSetup").fadeOut();
            $("#divButtonExecutionsFabricFinishingSetup").fadeIn();

            $("#tblFabricFinishingSetupChild").bootstrapTable('load', response.data.YaFinishingTypeSetupChilds);
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function getFabricGSMSetupDataEdit(FCSID) {
    var url = "";
    url = "/YarnAllowanceApi/fabricGSMSetupEdit/" + FCSID;
    axios.get(url)
        .then(function (response) {
            fabricgsmsetupmaster = response.data;
            formFabricGSMSetupMaster.find("#Id").val(response.data.Id);
            formFabricGSMSetupMaster.find("#GSMFrom").val(response.data.GsmFrom);
            formFabricGSMSetupMaster.find("#GSMTo").val(response.data.GsmTo);
            formFabricGSMSetupMaster.find("#AddYAllowance").val(response.data.AddYAllowance);

            initSelect2(formFabricGSMSetupMaster.find("#BuyerId"), response.data.BuyerList);

            $("#divEditFabricGSMSetup").fadeIn();
            $("#divtblFabricGSMSetup").fadeOut();
            $("#divButtonExecutionsFabricGSMSetup").fadeIn();

            $("#tblFabricGSMSetupChild").bootstrapTable('load', response.data.YaFabricGsmSetupChilds);
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function getYarnProgramSetupDataEdit(FCSID) {
    var url = "";
    url = "/YarnAllowanceApi/yarnProgramSetupEdit/" + FCSID;
    axios.get(url)
        .then(function (response) {
            yarnprogramsetupmaster = response.data;
            formYarnProgramSetupMaster.find("#Id").val(response.data.Id);
            formYarnProgramSetupMaster.find("#AddYAllowance").val(response.data.AddYAllowance);

            initSelect2(formYarnProgramSetupMaster.find("#YarnProgramId"), response.data.YarnProgramList);
            formYarnProgramSetupMaster.find("#YarnProgramId").val(response.data.YarnProgramId).trigger("change");

            initSelect2(formYarnProgramSetupMaster.find("#ShadeId"), response.data.ShadeList);
            formYarnProgramSetupMaster.find("#ShadeId").val(response.data.ShadeId).trigger("change");

            initSelect2(formYarnProgramSetupMaster.find("#BuyerId"), response.data.BuyerList);

            $("#divEditYarnProgramSetup").fadeIn();
            $("#divtblYarnProgramSetup").fadeOut();
            $("#divButtonExecutionsYarnProgramSetup").fadeIn();

            $("#tblYarnProgramSetupChild").bootstrapTable('load', response.data.YaYarnProgramSetupChilds);
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function getCollarCuffSetupDataEdit(FCSID) {
    var url = "";
    url = "/YarnAllowanceApi/collarCuffSetupEdit/" + FCSID;
    axios.get(url)
        .then(function (response) {
            collarcuffsetupmaster = response.data;
            formCollarCuffSetupMaster.find("#Id").val(response.data.Id);
            formCollarCuffSetupMaster.find("#SolidAllowance").val(response.data.SolidAllowance);
            formCollarCuffSetupMaster.find("#YDAllowance").val(response.data.YdAllowance);

            initSelect2(formCollarCuffSetupMaster.find("#ItemId"), response.data.CollarCuffList);
            formCollarCuffSetupMaster.find("#ItemId").val(response.data.ItemId).trigger("change");

            initSelect2(formCollarCuffSetupMaster.find("#BuyerId"), response.data.BuyerList);

            $("#divEditCollarCuffSetup").fadeIn();
            $("#divtblCollarCuffSetup").fadeOut();
            $("#divButtonExecutionsCollarCuffSetup").fadeIn();

            $("#tblCollarCuffSetupChild").bootstrapTable('load', response.data.YaCollarCuffSetupChilds);
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function getYarnCategorySetupDataEdit(FCSID) {
    var url = "";
    url = "/YarnAllowanceApi/yarnCategorySetupEdit/" + FCSID;
    axios.get(url)
        .then(function (response) {
            yarncategorysetupmaster = response.data;
            formYarnCategorySetupMaster.find("#Id").val(response.data.Id);
            formYarnCategorySetupMaster.find("#YarnCategory").val(response.data.YarnCategory);
            formYarnCategorySetupMaster.find("#YAllowance").val(response.data.YAllowance);

            initSelect2(formYarnCategorySetupMaster.find("#BuyerId"), response.data.BuyerList);

            $("#divEditYarnCategorySetup").fadeIn();
            $("#divtblYarnCategorySetup").fadeOut();
            $("#divButtonExecutionsYarnCategorySetup").fadeIn();

            $("#tblYarnCategorySetupChild").bootstrapTable('load', response.data.YaYarnCategorySetupChilds);
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function initYarnQtySetupChildTable() {
    $("#tblYarnQtySetupChild").bootstrapTable('destroy');
    $("#tblYarnQtySetupChild").bootstrapTable({
        scrolling: true,
        columns: [
            {
                field: "BuyerName",
                title: "Buyer Name",
                filterControl: "input"
            }
        ]
    });
};

function initFabricConstructionSetupChildTable() {
    $("#tblFabricConstructionSetupChild").bootstrapTable('destroy');
    $("#tblFabricConstructionSetupChild").bootstrapTable({
        scrolling: true,
        columns: [
            {
                field: "BuyerName",
                title: "Buyer Name",
                filterControl: "input"
            }
        ]
    });
};

function initFabricDyeingSetupChildTable() {
    $("#tblFabricDyeingSetupChild").bootstrapTable('destroy');
    $("#tblFabricDyeingSetupChild").bootstrapTable({
        scrolling: true,
        columns: [
            {
                field: "BuyerName",
                title: "Buyer Name",
                filterControl: "input"
            }
        ]
    });
};

function initFabricFinishingSetupChildTable() {
    $("#tblFabricFinishingSetupChild").bootstrapTable('destroy');
    $("#tblFabricFinishingSetupChild").bootstrapTable({
        scrolling: true,
        columns: [
            {
                field: "BuyerName",
                title: "Buyer Name",
                filterControl: "input"
            }
        ]
    });
};

function initFabricGSMSetupChildTable() {
    $("#tblFabricGSMSetupChild").bootstrapTable('destroy');
    $("#tblFabricGSMSetupChild").bootstrapTable({
        scrolling: true,
        columns: [
            {
                field: "BuyerName",
                title: "Buyer Name",
                filterControl: "input"
            }
        ]
    });
};

function initYarnProgramSetupChildTable() {
    $("#tblYarnProgramSetupChild").bootstrapTable('destroy');
    $("#tblYarnProgramSetupChild").bootstrapTable({
        scrolling: true,
        columns: [
            {
                field: "BuyerName",
                title: "Buyer Name",
                filterControl: "input"
            }
        ]
    });
};

function initCollarCuffSetupChildTable() {
    $("#tblCollarCuffSetupChild").bootstrapTable('destroy');
    $("#tblCollarCuffSetupChild").bootstrapTable({
        scrolling: true,
        columns: [
            {
                field: "BuyerName",
                title: "Buyer Name",
                filterControl: "input"
            }
        ]
    });
};

function initYarnCategorySetupChildTable() {
    $("#tblYarnCategorySetupChild").bootstrapTable('destroy');
    $("#tblYarnCategorySetupChild").bootstrapTable({
        scrolling: true,
        columns: [
            {
                field: "BuyerName",
                title: "Buyer Name",
                filterControl: "input"
            }
        ]
    });
};

function resetFormYarnQtySetup() {
    formYarnQtySetupMaster.trigger("reset");
    formYarnQtySetupMaster.find("#Id").val(-1111);
    formYarnQtySetupMaster.find("#EntityState").val(4);
}

function resetFormFabricConstructionSetup() {
    formFabricConstructionSetupMaster.trigger("reset");
    formFabricConstructionSetupMaster.find("#Id").val(-1111);
    formFabricConstructionSetupMaster.find("#EntityState").val(4);
}

function resetFormFabricDyeingSetup() {
    formFabricDyeingSetupMaster.trigger("reset");
    formFabricDyeingSetupMaster.find("#Id").val(-1111);
    formFabricDyeingSetupMaster.find("#EntityState").val(4);
}

function resetFormFabricFinishingSetup() {
    formFabricFinishingSetupMaster.trigger("reset");
    formFabricFinishingSetupMaster.find("#Id").val(-1111);
    formFabricFinishingSetupMaster.find("#EntityState").val(4);
}

function resetFormFabricGSMSetup() {
    formFabricGSMSetupMaster.trigger("reset");
    formFabricGSMSetupMaster.find("#Id").val(-1111);
    formFabricGSMSetupMaster.find("#EntityState").val(4);
}

function resetFormYarnProgramSetup() {
    formYarnProgramSetupMaster.trigger("reset");
    formYarnProgramSetupMaster.find("#Id").val(-1111);
    formYarnProgramSetupMaster.find("#EntityState").val(4);
}

function resetFormCollarCuffSetup() {
    formCollarCuffSetupMaster.trigger("reset");
    formCollarCuffSetupMaster.find("#Id").val(-1111);
    formCollarCuffSetupMaster.find("#EntityState").val(4);
}

function resetFormYarnCategorySetup() {
    formYarnCategorySetupMaster.trigger("reset");
    formYarnCategorySetupMaster.find("#Id").val(-1111);
    formYarnCategorySetupMaster.find("#EntityState").val(4);
}

function backToListYarnQtySetup() {
    $("#divEditYarnQtySetup").fadeOut();
    $("#divtblYarnQtySetup").fadeIn();
    $("#divButtonExecutionsYarnQtySetup").fadeOut();
    getYarnQtySetupData();
}

function backToListFabricConstructionSetup() {
    $("#divEditFabricConstructionSetup").fadeOut();
    $("#divtblFabricConstructionSetup").fadeIn();
    $("#divButtonExecutionsFabricConstructionSetup").fadeOut();
    getFabricConstructionSetupData();
}

function backToListFabricDyeingSetup() {
    $("#divEditFabricDyeingSetup").fadeOut();
    $("#divtblFabricDyeingSetup").fadeIn();
    $("#divButtonExecutionsFabricDyeingSetup").fadeOut();
    getFabricDyeingSetupData();
}

function backToListFabricFinishingSetup() {
    $("#divEditFabricFinishingSetup").fadeOut();
    $("#divtblFabricFinishingSetup").fadeIn();
    $("#divButtonExecutionsFabricFinishingSetup").fadeOut();
    getFabricFinishingSetupData();
}

function backToListFabricGSMSetup() {
    $("#divEditFabricGSMSetup").fadeOut();
    $("#divtblFabricGSMSetup").fadeIn();
    $("#divButtonExecutionsFabricGSMSetup").fadeOut();
    getFabricGSMSetupData();
}

function backToListYarnProgramSetup() {
    $("#divEditYarnProgramSetup").fadeOut();
    $("#divtblYarnProgramSetup").fadeIn();
    $("#divButtonExecutionsYarnProgramSetup").fadeOut();
    getYarnProgramSetupData();
}

function backToListCollarCuffSetup() {
    $("#divEditCollarCuffSetup").fadeOut();
    $("#divtblCollarCuffSetup").fadeIn();
    $("#divButtonExecutionsCollarCuffSetup").fadeOut();
    getCollarCuffSetupData();
}

function backToListYarnCategorySetup() {
    $("#divEditYarnCategorySetup").fadeOut();
    $("#divtblYarnCategorySetup").fadeIn();
    $("#divButtonExecutionsYarnCategorySetup").fadeOut();
    getYarnCategorySetupData();
}
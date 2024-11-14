var formSampleBookingStatus;
var formSampleBookingItemModal;
var SampleBookingStatus;
var filterBy = {};
var SampleBookingChilds = [];
var Id;
var sbStatus = 1;
var isAllChecked = false;
var fabricTechnicalNameList = [];
var yarnSupplierLists = [];
var yarnStatusLists = [];
var tableParams = {
    offset: 0,
    limit: 10,
    sort: '',
    order: '',
    filter: ''
}
var ExportOrderfilterBy = {};
var ExportOrderTableParams = {
    offset: 0,
    limit: 10,
    sort: '',
    order: '',
    filter: ''
}

var RequisitionByfilterBy = {};
var RequisitionByTableParams = {
    offset: 0,
    limit: 10,
    sort: '',
    order: '',
    filter: ''
}
var sampleBookingChildList = [];
var houseKeepingDropDownsInitialized = false;

$(function () {
    formSampleBookingStatus = $("#formSampleBookingStatus");
    formSampleBookingItemModal = $("#formSampleBookingItemModal");
    initPendingSampleBookingStatusMasterTable();
    getPendingSampleBookingStatusMasterData();
    initExportOrderListsTable();
    initSampleBookingByUsersTable();
    getFabricTechnicalNames();
    getYarnSupplierNames();
    getYarnStatus();
    houseKeepingDropDownsInitialized = false;

    $("#btnSampleBookingAddItem").on("click", function (e) {
        e.preventDefault();

        if (!SampleBookingStatus) {
            toastr.warning("Loading problem.");
            return;
        }

        if (!houseKeepingDropDownsInitialized) {
            initSelect2($("#YarnTypeId"), SampleBookingStatus.YarnTypeList);
            initSelect2($("#YarnConstructionId"), SampleBookingStatus.FabricConstructionList);
            initSelect2($("#YarnCompositionId"), SampleBookingStatus.FabricCompositionList);
            initSelect2($("#YarnColor"), SampleBookingStatus.FabricColorList);
            initSelect2($("#FabricTechnicalNameId"), SampleBookingStatus.FabricTechnicalNameList);
            initSelect2($("#FabricGsm"), SampleBookingStatus.FabricGSMList);
            initSelect2($("#FabricWidth"), SampleBookingStatus.FabricWidthList);
            initSelect2($("#KnittingType"), SampleBookingStatus.FinishingTypeList);
            initSelect2($("#DyeingType"), SampleBookingStatus.DyeingTypeList);
            initSelect2($("#YarnProgramId"), SampleBookingStatus.YarnProgramList);
            initSelect2($("#YarnStatus"), SampleBookingStatus.YarnStatusList);
            initSelect2($("#YarnSupplier"), SampleBookingStatus.SupplierListYarn);
        }

        $("#modal-sample-booking-child-items-information").modal('show');
    });

    $("#btnSampleBookingNew").on("click", function (e) {
        e.preventDefault();

        var today = new Date();
        var datetoday = (today.getMonth() + 1) + '/' + today.getDate() + '/' + today.getFullYear();
        formSampleBookingStatus.find("#SPBookingDate").val(datetoday);
        formSampleBookingStatus.find("#DeliveryDate").val(datetoday);
        $("#KnittingDate").val(datetoday);

        $("#divNewSampleBookingStatus").fadeIn();
        $("#divtblSampleBookingStatus").fadeOut();
        $("#divSampleBookingStatusButtonExecutions").fadeIn();
        SampleBookingStatusresetForm();
        getNewSampleBookingStatusData();
    });

    $("#btnSampleBookingPending").on("click", function (e) {
        e.preventDefault();
        resetTableParams();
        sbStatus = 1;
        initPendingSampleBookingStatusMasterTable();
        getPendingSampleBookingStatusMasterData();
    });

    $("#btnSampleBookingCompleted").on("click", function (e) {
        e.preventDefault();
        resetTableParams();
        sbStatus = 2;
        initPendingSampleBookingStatusMasterTable();
        getPendingSampleBookingStatusMasterData();
    });

    $("#btnSampleBookingRevise").on("click", function (e) {
        e.preventDefault();
        resetTableParams();
        sbStatus = 3;
        initPendingSampleBookingStatusMasterTable();
        getPendingSampleBookingStatusMasterData();
    });

    $("#btnItemUpdate").on("click", function (e) {
        e.preventDefault();
        var id = 72156;
        var sampleBookingChildItems = SampleBookingStatus.SampleBookingChilds.find(function (el) {
            return el.Id == id;
        });
        //id = getMaxIdForArray(SampleBookingStatus.SampleBookingChilds, "Id");
        sampleBookingChildItems = {
            Id: 72156,
            BookingQtyKg: formSampleBookingStatus.find("#BookingQty").val()
        };

        //SampleBookingStatus.SampleBookingChilds.push(sampleBookingChildItems);
        formSampleBookingStatus.find("#tblSampleBookingStatusChilds").bootstrapTable('updateByUniqueId', Id, sampleBookingChildItems);
        formSampleBookingStatus.find("#tblSampleBookingStatusChilds").bootstrapTable('load', SampleBookingStatus.SampleBookingChilds);
    });

    $("#btnAddTeamLeader").on("click", function (e) {
        e.preventDefault();
        getExportOrdersFromBuyerCompany();
        $("#modal-child").modal('show');
    });

    $("#btnAddBookedBy").on("click", function (e) {
        e.preventDefault();
        $("#modal-child-").modal('show');
        getSampleBookingBookedByUsers();
    });

    $("#btnSaveSampleBooking").click(function (e) {
        e.preventDefault();
        var data = formDataToJson(formSampleBookingStatus.serializeArray());
        data["SampleBookingChilds"] = SampleBookingStatus.SampleBookingChilds;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/SBSApi/SampleBookingSave", data, config)
            .then(function () {
                toastr.success("Your Sample Booking saved successfully.");
                SampleBookingStatusbackToList();
            })
            .catch(showResponseError);
    });

    $("#btnSampleBookingEditCancel").on("click", function (e) {
        e.preventDefault();
        SampleBookingStatusbackToList();
    });

    $("#btnSampleBookingAddYarnItemData").click(function (e) {
        e.preventDefault;
        var yarnType = formSampleBookingItemModal.find("#YarnTypeId").select2('data')[0];
        var fabricConstruction = formSampleBookingItemModal.find("#YarnConstructionId").select2('data')[0];
        var yarnComposition = formSampleBookingItemModal.find("#YarnCompositionId").select2('data')[0];
        var yarnColor = formSampleBookingItemModal.find("#YarnColor").select2('data')[0];
        var technicalName = formSampleBookingItemModal.find("#FabricTechnicalNameId").select2('data')[0];
        var fabricGSM = formSampleBookingItemModal.find("#FabricGsm").select2('data')[0];
        var fabricWidth = formSampleBookingItemModal.find("#FabricWidth").select2('data')[0];
        var knittingType = formSampleBookingItemModal.find("#KnittingType").select2('data')[0];
        var dyeingType = formSampleBookingItemModal.find("#DyeingType").select2('data')[0];
        var yarnProgram = formSampleBookingItemModal.find("#YarnProgramId").select2('data')[0];
        var yarnStatus = formSampleBookingItemModal.find("#YarnStatus").select2('data')[0];
        var yarnSupplier = formSampleBookingItemModal.find("#YarnSupplier").select2('data')[0];

        var sampleBookingYarnPOChildItem = {
            YarnType: yarnType.text,
            ConstructionID: fabricConstruction.id,
            FabricConstruction: fabricConstruction.text,
            CompositionID: yarnComposition.id,
            FabricComposition: yarnComposition.text,
            FabricColorID: yarnColor.id,
            ColorName: yarnColor.text,
            TechnicalNameId: technicalName.id,
            FabricGSM: fabricGSM.id,
            FabricGsm: fabricGSM.text,
            FabricWidth: fabricWidth.id,
            FabricWidth: fabricWidth.text,
            KnittingType: knittingType.text,
            DyeingTypeID: dyeingType.id,
            DyeingType: dyeingType.text,
            YarnProgram: yarnProgram.text,
            YarnStatusId: yarnStatus.id,
            YarnSupplierId: yarnSupplier.id,

            RequiredQty: formSampleBookingItemModal.find("#RequiredQty").val(),
            BookingQtyKg: formSampleBookingItemModal.find("#BookingQtyKg").val(),
            BookingQtyPcs: formSampleBookingItemModal.find("#BookingQtyPcs").val(),
            YarnCount: formSampleBookingItemModal.find("#YarnCount").val(),
            MachineGauge: formSampleBookingItemModal.find("#MachineGauge").val(),
            MachineDia: formSampleBookingItemModal.find("#MachineDia").val(),
            StitchLength: formSampleBookingItemModal.find("#StitchLength").val(),
            YarnLotNo: formSampleBookingItemModal.find("#YarnLotNo").val(),
            SDR: formSampleBookingItemModal.find("#SDR").val(),
            KnittingDate: formSampleBookingItemModal.find("#KnittingDate").val(),
            ReqKnittingQty: formSampleBookingItemModal.find("#ReqKnittingQty").val(),
            FlatKnitQtyPCs: formSampleBookingItemModal.find("#FlatKnitQty").val(),
            MachineNo: formSampleBookingItemModal.find("#MachineNo").val(),
            MachineBrand: formSampleBookingItemModal.find("#MachineBrand").val(),
            KnittedQty: formSampleBookingItemModal.find("#KnittedQty").val(),
            KnitBalance: 0
        };

        SampleBookingStatus.SampleBookingChilds.push(sampleBookingYarnPOChildItem);
        formSampleBookingStatus.find("#tblSampleBookingStatusChilds").bootstrapTable('load', SampleBookingStatus.SampleBookingChilds);
    });
});

function getNewSampleBookingStatusData() {
    $('.readonly-applicable').removeProp('readonly', false);
    url = "/SBSApi/NewSampleBooking/";

    axios.get(url)
        .then(function (response) {
            SampleBookingStatus = response.data;

            //initSelect2(formSampleBookingStatus.find("#BuyerId"), response.data.BuyerList);
            //initSelect2(formSampleBookingStatus.find("#BuyerTeamId"), response.data.BuyerTeamList);
            initSelect2(formSampleBookingStatus.find("#ReviseInfoId"), response.data.ReviseInformationList);
            initSelect2(formSampleBookingStatus.find("#SPTypeId"), response.data.SPTypeList);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getSampleBookingBookedByUsers() {
    var queryParams = $.param(RequisitionByTableParams);
    url = "/yrApi/getUsers" + "?" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblSampleBookingByUserLists").bootstrapTable('load', response.data);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
};

function getYarnChildItems(exportOrderId) {
    axios.get("/yrApi/yarnChildItems/" + exportOrderId)
        .then(function (response) {
            RequisitionChilds = response.data;
            formSampleBookingStatus.find("#Id").val(response.data.Id);

            initTblSampleBookingStatusChildItems();

            formSampleBookingStatus.find("#tblSampleBookingStatusChilds").bootstrapTable('load', response.data);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function initPendingSampleBookingStatusMasterTable() {
    $("#tblSampleBookingStatus").bootstrapTable('destroy');
    $("#tblSampleBookingStatus").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#SampleBookingStatusToolbar",
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
                title: 'Actions',
                align: 'center',
                width: 50,
                visible: !isAllChecked,
                formatter: function () {
                    return [
                        '<span class="btn-group">',
                        '<a class="btn btn-default btn-xs edit" href="javascript:void(0)" title="View Booking">',
                        '<i class="fa fa-eye"></i>',
                        '</a>',
                        '</span>'
                    ].join('');
                },
                events: {
                    'click .edit': function (e, value, row, index) {
                        e.preventDefault();
                        SampleBookingStatusresetForm();
                        formSampleBookingStatus.find("#BookingID").val(row.BookingId);
                        if (row.Id) {
                            getSampleBookingStatusEdit(row.Id);
                        }
                        else {
                            getBookingAnalysisMasterSampleBooking(row.BookingId);
                        }

                        $("#divNewSampleBookingStatus").fadeIn();
                        $("#divtblSampleBookingStatus").fadeOut();
                        $("#divSampleBookingStatusButtonExecutions").fadeIn();
                    }
                }
            },
            {
                field: "BookingNo",
                title: "Booking No",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerName",
                title: "Buyer",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerTeamName",
                title: "Buyer Team",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "MerchandiserName",
                title: "Merchandiser",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BookingQty",
                title: "Booking Qty",
                filterControl: "input",
                align: 'right',
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                //footerFormatter: calculateSampleBookingStatusMasterBookingQty
            },
            {
                field: "SpBookingDateStr",
                title: "Booking Date"
                //filterControl: "input"
                //filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "RevisionNo",
                title: "Revision No"
                //filterControl: "input"
                //filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            }
        ],
        onPageChange: function (number, size) {
            var newOffset = (number - 1) * size;
            var newLimit = size;
            if (tableParams.offset == newOffset && tableParams.limit == newLimit)
                return;

            tableParams.offset = newOffset;
            tableParams.limit = newLimit;

            getPendingSampleBookingStatusMasterData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getPendingSampleBookingStatusMasterData();
        },
        onRefresh: function () {
            resetBookingAnalysisTableParams();
            getPendingSampleBookingStatusMasterData();
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

            getPendingSampleBookingStatusMasterData();
        }
    });
}

function getPendingSampleBookingStatusMasterData() {
    var queryParams = $.param(tableParams);
    $('#tblSampleBookingStatus').bootstrapTable('showLoading');
    var url = "/SBSApi/SampleBookingLists" + "?sbStatus=" + sbStatus + "&" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblSampleBookingStatus").bootstrapTable('load', response.data);
            $('#tblSampleBookingStatus').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getBookingAnalysisMasterSampleBooking(bookingId) {
    $('.readonly-applicable').prop('readonly', true);
    var url = "";
    url = "/SBSApi/newbookinganalysisSample/" + bookingId;
    axios.get(url)
        .then(function (response) {
            SampleBookingStatus = response.data;
            formSampleBookingStatus.find("#Id").val(response.data.Id);
            formSampleBookingStatus.find("#PreProcessRevNo").val(response.data.PreProcessRevNo);
            formSampleBookingStatus.find("#RevisionNo").val(response.data.RevisionNo);
            formSampleBookingStatus.find("#BuyerId").val(response.data.BuyerId);
            formSampleBookingStatus.find("#BuyerTeamId").val(response.data.BuyerTeamId);
            formSampleBookingStatus.find("#BookingId").val(response.data.BookingId);
            formSampleBookingStatus.find("#Remarks").val(response.data.Remarks);

            formSampleBookingStatus.find("#SPNo").val(response.data.SpNo);
            formSampleBookingStatus.find("#lblBuyerName").val(response.data.BuyerName);
            formSampleBookingStatus.find("#lblBuyerTeam").val(response.data.BuyerTeamName);
            formSampleBookingStatus.find("#lblMerchandiserName").val(response.data.MerchandiserName);
            formSampleBookingStatus.find("#SPBookingDate").val(response.data.SPBookingDateStr);

            formSampleBookingStatus.find("#OrderSession").val(response.data.SessionName);
            formSampleBookingStatus.find("#MerchandisingTeam").val(response.data.MerchandisingTeam);
            formSampleBookingStatus.find("#SeasonId").val(response.data.SeasonId);
            formSampleBookingStatus.find("#MerchandiserTeamId").val(response.data.MerchandiserTeamId);
            formSampleBookingStatus.find("#StyleNo").val(response.data.StyleNo);

            initSelect2(formSampleBookingStatus.find("#SPTypeId"), response.data.SPTypeList);
            formSampleBookingStatus.find("#SPTypeId").val(response.data.SpTypeId).trigger("change");

            initSelect2(formSampleBookingStatus.find("#ReviseInfoId"), response.data.ReviseInformationList);

            initSelect2($("#YarnTypeId"), response.data.YarnTypeList);
            initSelect2($("#YarnConstructionId"), response.data.FabricConstructionList);
            initSelect2($("#YarnCompositionId"), response.data.FabricCompositionList);
            initSelect2($("#YarnColor"), response.data.FabricColorList);
            initSelect2($("#FabricTechnicalNameId"), response.data.FabricTechnicalNameList);
            initSelect2($("#FabricGsm"), response.data.FabricGSMList);
            initSelect2($("#FabricWidth"), response.data.FabricWidthList);
            initSelect2($("#KnittingType"), response.data.FinishingTypeList);
            initSelect2($("#DyeingType"), response.data.DyeingTypeList);
            initSelect2($("#YarnProgramId"), response.data.YarnProgramList);
            initSelect2($("#YarnStatus"), response.data.YarnStatusList);
            initSelect2($("#YarnSupplier"), response.data.SupplierListYarn);

            initTblSampleBookingStatusChildItems();
            formSampleBookingStatus.find("#tblSampleBookingStatusChilds").bootstrapTable('load', response.data.SampleBookingChilds);
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function getSampleBookingStatusEdit(id) {
    url = "/SBSApi/SampleBookingEdit/" + id;

    axios.get(url)
        .then(function (response) {
            SampleBookingStatus = response.data;
            formSampleBookingStatus.find("#Id").val(response.data.Id);
            formSampleBookingStatus.find("#SPNo").val(response.data.SpNo);
            formSampleBookingStatus.find("#BuyerId").val(response.data.BuyerId);
            formSampleBookingStatus.find("#BuyerTeamId").val(response.data.BuyerTeamId);
            formSampleBookingStatus.find("#lblBuyerName").val(response.data.BuyerName);
            formSampleBookingStatus.find("#lblBuyerTeam").val(response.data.BuyerTeamName);
            formSampleBookingStatus.find("#lblMerchandiserName").val(response.data.MerchandiserName);
            formSampleBookingStatus.find("#SPBookingDate").val(response.data.SPBookingDateStr);
            formSampleBookingStatus.find("#TeamLeader").val(response.data.TeamLeader);
            formSampleBookingStatus.find("#BookedBy").val(response.data.BookedBy);
            formSampleBookingStatus.find("#TeamLeaderName").val(response.data.TeamLeaderName);
            formSampleBookingStatus.find("#BookedByName").val(response.data.BookedByName);
            formSampleBookingStatus.find("#SPBookingDate").val(response.data.SpBookingDateStr);
            formSampleBookingStatus.find("#SampleReference").val(response.data.SampleReference);
            formSampleBookingStatus.find("#EWPNo").val(response.data.EwpNo);
            formSampleBookingStatus.find("#LotNo").val(response.data.LotNo);
            formSampleBookingStatus.find("#SDR").val(response.data.Sdr);
            formSampleBookingStatus.find("#DeliveryDate").val(response.data.DeliveryDate);
            formSampleBookingStatus.find("#Status").val(response.data.Status);
            formSampleBookingStatus.find("#Remarks").val(response.data.Remarks);

            formSampleBookingStatus.find("#OrderSession").val(response.data.SessionName);
            formSampleBookingStatus.find("#MerchandisingTeam").val(response.data.MerchandisingTeam);
            formSampleBookingStatus.find("#SeasonId").val(response.data.SeasonId);
            formSampleBookingStatus.find("#MerchandiserTeamId").val(response.data.MerchandiserTeamId);
            formSampleBookingStatus.find("#StyleNo").val(response.data.StyleNo);

            initSelect2(formSampleBookingStatus.find("#ReviseInfoId"), response.data.ReviseInformationList);
            formSampleBookingStatus.find("#ReviseInfoId").val(response.data.ReviseInfoId).trigger("change");

            initSelect2(formSampleBookingStatus.find("#SPTypeId"), response.data.SPTypeList);
            formSampleBookingStatus.find("#SPTypeId").val(response.data.SpTypeId).trigger("change");

            initTblSampleBookingStatusChildItems();
            formSampleBookingStatus.find("#tblSampleBookingStatusChilds").bootstrapTable('load', response.data.SampleBookingChilds);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function initTblSampleBookingStatusChildItems() {
    formSampleBookingStatus.find("#tblSampleBookingStatusChilds").bootstrapTable({
        showFooter: true,
        uniqueId: 'Id',
        columns: [
            //{
            //    field: "Id",
            //    title: "Id",
            //    align: "left",
            //    filterControl: "input"
            //},
            {
                field: "YarnType",
                title: "Yarn Type",
                align: "left",
                filterControl: "input"
            },
            {
                field: "FabricConstruction",
                title: "Construction",
                filterControl: "input",
            },
            {
                field: "FabricComposition",
                title: "Composition",
                filterControl: "input"
            },
            {
                field: "ColorName",
                title: "Fabric Color",
                filterControl: "input"
            },
            {
                field: "TechnicalNameId",
                title: "Technical Name",
                editable: {
                    source: fabricTechnicalNameList,
                    type: 'select2',
                    showbuttons: false,
                    select2: { width: 100, placeholder: 'Technical Name' }
                }
            },
            {
                field: "FabricGsm",
                title: "GSM",
                align: "center",
                filterControl: "input"
            },
            {
                field: "FabricWidth",
                title: "Width",
                align: "center",
                filterControl: "input",
                width: 50
            },
            {
                field: "KnittingType",
                title: "Knitting Type",
                align: "left",
                filterControl: "input"
            },
            {
                field: "DyeingType",
                title: "YD/Solid",
                align: "left",
                filterControl: "input"
            },
            {
                field: "YarnProgram",
                title: "Yarn Program",
                align: "left",
                filterControl: "input"
            },
            {
                field: "LapDipNo",
                title: "Lab Dip No",
                align: "left",
                filterControl: "input"
            },
            {
                field: "Remarks",
                title: "Instruction",
                align: "left",
                filterControl: "input"
            },
            {
                field: "RequiredQty",
                title: "Booking Qty",
                align: "right",
                filterControl: "input"
            },
            {
                field: "BookingQtyKg",
                title: "Booking Qty (Kg)",
                width: 20,
                align: "right",
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "BookingQtyPCs",
                title: "Booking Qty (PCs)",
                width: 20,
                align: "right",
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "YarnCount",
                title: "Yarn Count",
                width: 20,
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "MachineGauge",
                title: "Machine Gauge",
                width: 20,
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "MachineDia",
                title: "Machine Dia",
                width: 20,
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "StitchLength",
                title: "Stitch Length",
                width: 20,
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "YarnStatusId",
                title: "Yarn Status",
                editable: {
                    source: yarnStatusLists,
                    type: 'select2',
                    showbuttons: false,
                    select2: { width: 100, placeholder: 'Yarn Status' }
                }
            },
            {
                field: "YarnLotNo",
                title: "Yarn Lot No",
                width: 20,
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "SDR",
                title: "SDR No",
                width: 20,
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "KnittingDate",
                title: "Knitting Date",
                width: 200,
                editable: {
                    type: 'combodate',
                    format: 'YYYY-MM-DD',
                    template: 'DD/MM/YYYY',
                    inputclass: 'input-sm'
                }
            },
            {
                field: "YarnSupplierId",
                title: "Yarn Supplier",
                editable: {
                    source: yarnSupplierLists,
                    type: 'select2',
                    showbuttons: false,
                    select2: { width: 100, placeholder: 'Yarn Supplier' }
                }
            },
            {
                field: "ReqKnittingQty",
                title: "Req. Knitting Qty (Kg)",
                width: 20,
                align: "right",
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "FlatKnitQtyPCs",
                title: "Flat Knit Qty (PCs)",
                width: 20,
                align: "right",
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "MachineNo",
                title: "Machine No",
                width: 20,
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "MachineBrand",
                title: "Machine Brand",
                width: 20,
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "KnittedQty",
                title: "Knitted Qty",
                width: 20,
                align: "right",
                editable: {
                    type: 'text',
                    inputclass: 'input-sm',
                    showbuttons: false
                }
            },
            {
                field: "KnitBalance",
                title: "Knit Balance",
                width: 20
            }
        ]
    });
}

function SampleBookingStatusbackToList() {
    formSampleBookingStatus.find("#divNewSampleBookingStatus").fadeOut();
    $("#divtblSampleBookingStatus").fadeIn();
    $("#divSampleBookingStatusButtonExecutions").fadeOut();
}

function SampleBookingStatusresetForm() {
    formSampleBookingStatus.trigger("reset");
    formSampleBookingStatus.find("#Id").val(-1111);
    formSampleBookingStatus.find("#EntityState").val(4);
}

function resetTableParams() {
    tableParams.offset = 0;
    tableParams.limit = 10;
    tableParams.filter = '';
    tableParams.sort = '';
    tableParams.order = '';
}

function getExportOrdersFromBuyerCompany() {
    var queryParams = $.param(ExportOrderTableParams);
    url = "/yrApi/getALLUsers" + "?" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblSampleBookingByALLUserLists").bootstrapTable('load', response.data);
        })
        .catch(function () {
            toastr.error(err.response.data.Message);
        })
};

function initExportOrderListsTable() {
    $("#tblSampleBookingByALLUserLists").bootstrapTable('destroy');
    $("#tblSampleBookingByALLUserLists").bootstrapTable({
        pagination: true,
        filterControl: true,
        searchOnEnterKey: true,
        sidePagination: "server",
        pageList: "[10, 25, 50, 100, 500]",
        cache: false,
        columns: [
            {
                field: "UserCode",
                title: "User Code",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "UserName",
                title: "User Name",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "EmployeeName",
                title: "Employee Name",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "DepertmentDescription",
                title: "Description",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "Designation",
                title: "Designation",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            }
        ],
        onDblClickRow: function (row, $element, field) {
            $("#modal-child").modal('hide');
            $("#TeamLeader").val(row.UserCode);
            $("#TeamLeaderName").val(row.EmployeeName);
        },
        onPageChange: function (number, size) {
            var newOffset = (number - 1) * size;
            var newLimit = size;
            if (ExportOrderTableParams.offset == newOffset && ExportOrderTableParams.limit == newLimit)
                return;

            ExportOrderTableParams.offset = newOffset;
            ExportOrderTableParams.limit = newLimit;
            getExportOrdersFromBuyerCompany();
        },
        onSort: function (name, order) {
            ExportOrderTableParams.sort = name;
            ExportOrderTableParams.order = order;
            ExportOrderTableParams.offset = 0;

            getExportOrdersFromBuyerCompany();
        },
        onRefresh: function () {
            resetTableParams();
            getExportOrdersFromBuyerCompany();
        },
        onColumnSearch: function (columnName, filterValue) {
            if (columnName in ExportOrderfilterBy && !filterValue) {
                delete ExportOrderfilterBy[columnName];
            }
            else
                ExportOrderfilterBy[columnName] = filterValue;

            if (Object.keys(ExportOrderfilterBy).length === 0 && ExportOrderfilterBy.constructor === Object)
                ExportOrderTableParams.filter = "";
            else
                ExportOrderTableParams.filter = JSON.stringify(ExportOrderfilterBy);

            getExportOrdersFromBuyerCompany();
        }
    });
}

function initSampleBookingByUsersTable() {
    $("#tblSampleBookingByUserLists").bootstrapTable('destroy');
    $("#tblSampleBookingByUserLists").bootstrapTable({
        pagination: true,
        filterControl: true,
        searchOnEnterKey: true,
        sidePagination: "server",
        pageList: "[10, 25, 50, 100, 500]",
        cache: false,
        columns: [
            {
                field: "UserCode",
                title: "User Code",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "UserName",
                title: "User Name",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "EmployeeName",
                title: "Employee Name",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "DepertmentDescription",
                title: "Description",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "Designation",
                title: "Designation",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            }
        ],
        onDblClickRow: function (row, $element, field) {
            $("#modal-child-").modal('hide');
            $("#BookedBy").val(row.UserCode);
            $("#BookedByName").val(row.EmployeeName);
        },
        onPageChange: function (number, size) {
            var newOffset = (number - 1) * size;
            var newLimit = size;
            if (RequisitionByTableParams.offset == newOffset && RequisitionByTableParams.limit == newLimit)
                return;

            RequisitionByTableParams.offset = newOffset;
            RequisitionByTableParams.limit = newLimit;
            getSampleBookingBookedByUsers();
        },
        onSort: function (name, order) {
            RequisitionByTableParams.sort = name;
            RequisitionByTableParams.order = order;
            RequisitionByTableParams.offset = 0;

            getSampleBookingBookedByUsers();
        },
        onRefresh: function () {
            resetTableParams();
            getSampleBookingBookedByUsers();
        },
        onColumnSearch: function (columnName, filterValue) {
            if (columnName in RequisitionByfilterBy && !filterValue) {
                delete RequisitionByfilterBy[columnName];
            }
            else
                RequisitionByfilterBy[columnName] = filterValue;

            if (Object.keys(RequisitionByfilterBy).length === 0 && RequisitionByfilterBy.constructor === Object)
                RequisitionByTableParams.filter = "";
            else
                RequisitionByTableParams.filter = JSON.stringify(RequisitionByfilterBy);

            getSampleBookingBookedByUsers();
        }
    });
}

function calculateSampleBookingStatusMasterBookingQty(data) {
    var bookingQty = 0;

    $.each(data, function (i, row) {
        bookingQty += isNaN(parseInt(row.BookingQty)) ? 0 : parseInt(row.BookingQty);
    });
    return bookingQty.toFixed(2);
}

function getFabricTechnicalNames() {
    axios.get("/api/selectoption/fabrictechnicalname")
        .then(function (response) {
            fabricTechnicalNameList = response.data;
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getYarnSupplierNames() {
    axios.get("/api/selectoption/yarn-suppliers")
        .then(function (response) {
            yarnSupplierLists = response.data;
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getYarnStatus() {
    axios.get("/api/selectoption/SampleBookingYarnStatusLists")
        .then(function (response) {
            yarnStatusLists = response.data;
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}
var formWashBatchCreation;
var WashBatchCreation;
var swoStatus = 6;
var filterBy = {};
var tableParams = {
    offset: 0,
    limit: 10,
    sort: '',
    order: '',
    filter: ''
}
var WashBatchCreationChilds = [];

$(function () {
    formWashBatchCreation = $("#formWashBatchCreation");
    formWashBatchCreationItemModal = $("#formWashBatchCreationItemModal");

    initWashBatchCreationMasterTable();
    getWashBatchCreationMasterData();
    iniTblWashBatchCreationChilds();
    iniTblWashBatchCreationChildEWOs();

    $("#btnEditCancelWashBatchCreation").on("click", function (e) {
        e.preventDefault();
        backToListWashBatchCreation();
    });

    $("#btnWashBatchCreationNew").on("click", function (e) {
        e.preventDefault();
        WashBatchCreationResetForm();

        var today = new Date();
        var datetoday = (today.getMonth() + 1) + '/' + today.getDate() + '/' + today.getFullYear();
        formWashBatchCreation.find("#BatchCreationDate").val(datetoday);
        formWashBatchCreation.find("#PISubmissionDate").val(datetoday);
        formWashBatchCreation.find("#DeliveryDate").val(datetoday);

        $("#divEditWashBatchCreation").fadeIn();
        $("#divtblWashBatchCreation").fadeOut();
        $("#divButtonExecutionsWashBatchCreation").fadeIn();
        getNewWashBatchCreationData();
    });

    $("#btnAcknowledgeWashBatchCreation").click(function (e) {
        e.preventDefault();
        var data = formDataToJson(formWashBatchCreation.serializeArray());

        data["SwoDate"] = formWashBatchCreation.find("#SWODate").val();
        data["DeliveryDate"] = formWashBatchCreation.find("#DeliveryDate").val();

        data["BuyerId"] = formWashBatchCreation.find("#BuyerId").val();
        data["BuyerTeamId"] = formWashBatchCreation.find("#BuyerTeamId").val();
        data["IncoTermsId"] = formWashBatchCreation.find("#IncoTermsId").val();
        data["PaymentTermsId"] = formWashBatchCreation.find("#PaymentTermsId").val();
        data["TypeOfLCId"] = formWashBatchCreation.find("#TypeOfLCId").val();
        data["CurrencyId"] = formWashBatchCreation.find("#CurrencyId").val();
        data["CreditDays"] = formWashBatchCreation.find("#CreditDays").val();
        data["OfferValidity"] = formWashBatchCreation.find("#OfferValidity").val();
        data["PaymentInstrumentId"] = formWashBatchCreation.find("#PaymentInstrumentId").val();
        data["PartialShipmentStatusId"] = formWashBatchCreation.find("#PartialShipmentStatusId").val();

        data["WashBatchCreationChilds"] = WashBatchCreation.WashBatchCreationChilds;
        data["WashBatchCreationProcesses"] = WashBatchCreation.WashBatchCreationProcesses;
        data["WashBatchCreationSubProcessChilds"] = WashBatchCreation.WashBatchCreationSubProcessChilds;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/texSWOApi/WashBatchCreationSave", data, config)
            .then(function () {
                toastr.success("Your Service WO Acknowledged successfully.");
                backToListWashBatchCreation();
            })
            .catch(showResponseError);
    });

    formWashBatchCreation.find("#btnWashBatchCreationAddValueItem").on("click", function (e) {
        e.preventDefault();
        formWashBatchCreationItemModal.trigger("reset");
        getExportOrderLists(formWashBatchCreation.find("#BuyerId").val(), formWashBatchCreation.find("#BuyerTeamId").val());
        $("#modal-textile-service-wo-items-information").modal('show');
    });

    $("#btnWashBatchCreationPending").on("click", function (e) {
        e.preventDefault();
        swoStatus = 1;
        initWashBatchCreationMasterTable();
        getWashBatchCreationMasterData();
    });

    $("#btnWashBatchCreationApproved").on("click", function (e) {
        e.preventDefault();
        swoStatus = 2;
        initWashBatchCreationMasterTable();
        getWashBatchCreationMasterData();
    });

    $("#btnWashBatchCreationAddItemData").click(function (e) {
        e.preventDefault;
        var exportOrderId = formWashBatchCreationItemModal.find("#ExportOrderIdServiceWO").select2('data')[0];
        var rateApplyId = formWashBatchCreationItemModal.find("#RateApplyId").select2('data')[0];
        var woUnitId = formWashBatchCreationItemModal.find("#WOUnitId").select2('data')[0];
        formWashBatchCreation.find("#ExportOrderNo").val(exportOrderId.text);

        var forPO = formWashBatchCreationItemModal.find("#ForPO").val();
        if (forPO == "on") {
            var forPoMsg = "All";
        }

        var forColor = formWashBatchCreationItemModal.find("#ForColor").val();
        if (forColor == "on") {
            var forColorMsg = "All";
        }

        var WashBatchCreationChildItem = {
            ExportOrderID: exportOrderId.id,
            ExportOrderNo: exportOrderId.text,
            RateApplyID: rateApplyId.id,
            RateApply: rateApplyId.text,
            WOUnitID: woUnitId.id,
            UnitDesc: woUnitId.text,

            WoQty: formWashBatchCreationItemModal.find("#WOQty").val(),
            Rate: formWashBatchCreationItemModal.find("#Rate").val(),
            TotalValue: formWashBatchCreationItemModal.find("#WOQty").val() * formWashBatchCreationItemModal.find("#Rate").val(),
            Wastage: formWashBatchCreationItemModal.find("#Wastage").val(),
            ForOb: forPoMsg,
            ForColor: forColorMsg
        };

        WashBatchCreation.WashBatchCreationChilds.push(WashBatchCreationChildItem);
        formWashBatchCreation.find("#tblWashBatchCreationChilds").bootstrapTable('load', WashBatchCreation.WashBatchCreationChilds);
        $("#modal-textile-service-wo-items-information").modal('hide');
    });

    formWashBatchCreation.find("#BuyerId").on("select2:select", function (e) {
        getBuyerTeamFromBuyer(e.params.data.id);
    });
});
function getBuyerTeamFromBuyer(buyerId) {
    url = "/texSWOApi/GetBuyerTeamFromBuyerServiceWO/" + buyerId;
    axios.get(url)
        .then(function (response) {
            if (formWashBatchCreation.find("#BuyerTeamId").hasClass("select2-hidden-accessible")) {
                formWashBatchCreation.find("#BuyerTeamId").empty();
            };

            initSelect2(formWashBatchCreation.find("#BuyerTeamId"), response.data.BuyerTeamList);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getExportOrderLists(buyerId, buyerTeamId) {
    url = "/texSWOApi/GetExportOrderListsServiceWO/" + buyerId + "/" + buyerTeamId;
    axios.get(url)
        .then(function (response) {
            initSelect2($("#ExportOrderIdServiceWO"), response.data.ExportOrderList);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function getNewWashBatchCreationData() {
    $('.readonly-applicable').removeAttr('readonly', false);
    $('.disabled-applicable').removeAttr('disabled');

    url = "/texSWOApi/NewWashBatchCreationHouseKeeping/";

    axios.get(url)
        .then(function (response) {
            WashBatchCreation = response.data;
            var WashBatchCreationChilds = [];
            formWashBatchCreation.find("#btnWashBatchCreationAddValueItem").fadeIn();
            initSelect2(formWashBatchCreation.find("#BuyerId"), response.data.BuyerList);
            initSelect2(formWashBatchCreation.find("#BuyerTeamId"), response.data.BuyerTeamList);
            initSelect2(formWashBatchCreation.find("#IncoTermsId"), response.data.IncoTermsList);
            initSelect2(formWashBatchCreation.find("#PaymentTermsId"), response.data.PaymentTermsList);
            initSelect2(formWashBatchCreation.find("#TypeOfLCId"), response.data.LCTypeList);
            initSelect2(formWashBatchCreation.find("#CurrencyId"), response.data.CurrencyTypeList);
            initSelect2(formWashBatchCreation.find("#CreditDays"), response.data.LCTenureList);
            initSelect2(formWashBatchCreation.find("#OfferValidity"), response.data.OfferValidityList);
            initSelect2(formWashBatchCreation.find("#PaymentInstrumentId"), response.data.PaymentInstrumentList);
            initSelect2(formWashBatchCreation.find("#PartialShipmentStatusId"), response.data.PartialShipmentAllowList);

            initSelect2(formWashBatchCreationItemModal.find("#RateApplyId"), response.data.ServiceWORateApplyList);
            initSelect2(formWashBatchCreationItemModal.find("#WOUnitId"), response.data.ServiceWOUnitList);

            $("#btnRejectWashBatchCreation").fadeOut();
            $("#btnAcknowledgeWashBatchCreation").fadeOut();
            $("#btnSaveWashBatchCreation").fadeIn();
            formWashBatchCreation.find("#tblWashBatchCreationChilds").bootstrapTable('destroy');
            iniTblWashBatchCreationChilds();
            iniTblWashBatchCreationChildEWOs();
            formWashBatchCreation.find("#tblWashBatchCreationChilds").bootstrapTable('load', response.data.WashBatchCreationChilds);
            formWashBatchCreation.find("#tblWashBatchCreationChildEWOs").bootstrapTable('load', response.data.WashBatchCreationChilds);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function backToListWashBatchCreation() {
    $("#divtblWashBatchCreation").fadeIn();
    $("#divEditWashBatchCreation").fadeOut();
    $("#divButtonExecutionsWashBatchCreation").fadeOut();

    getWashBatchCreationMasterData();
}

function initWashBatchCreationMasterTable() {
    $("#tblWashBatchCreation").bootstrapTable('destroy');
    $("#tblWashBatchCreation").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#WashBatchCreationToolbar",
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
                        '<a class="btn btn-default btn-xs edit" href="javascript:void(0)" title="View Booking">',
                        '<i class="fa fa-edit"></i>',
                        '</a>',
                        '</span>'
                    ].join('');
                },
                events: {
                    'click .edit': function (e, value, row, index) {
                        e.preventDefault();
                        WashBatchCreationResetForm();
                        $("#divtblWashBatchCreation").fadeOut();
                        $("#divEditWashBatchCreation").fadeIn();
                        $("#divButtonExecutionsWashBatchCreation").fadeIn();
                        formWashBatchCreation.find("#SWOMasterId").val(row.SWOMasterId);
                        formWashBatchCreation.find("#Id").val(row.Id);
                        if (row.Id == "0") {
                            getNewWashBatchCreationMasterData(row.SWOMasterId);
                        }
                        else {
                            getWashBatchCreationMasterDataEdit(row.Id);
                        }
                    }
                }
            },
            {
                field: "SwoNo",
                title: "Batch No",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "SwoDateStr",
                title: "Batch Date",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "ExportOrderNo",
                title: "Export Order No",
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
            }
        ],
        onPageChange: function (number, size) {
            var newOffset = (number - 1) * size;
            var newLimit = size;
            if (tableParams.offset == newOffset && tableParams.limit == newLimit)
                return;

            tableParams.offset = newOffset;
            tableParams.limit = newLimit;

            getWashBatchCreationMasterData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getWashBatchCreationMasterData();
        },
        onRefresh: function () {
            resetWashBatchCreationTableParams();
            getWashBatchCreationMasterData();
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

            getWashBatchCreationMasterData();
        }
    });
}

function getWashBatchCreationMasterData() {
    var queryParams = $.param(tableParams);
    $('#tblWashBatchCreation').bootstrapTable('showLoading');
    var url = "/texSWOApi/TextileWashServiceWOLists" + "?swoStatus=" + swoStatus + "&" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblWashBatchCreation").bootstrapTable('load', response.data);
            $('#tblWashBatchCreation').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function resetWashBatchCreationTableParams() {
    tableParams.offset = 0;
    tableParams.limit = 10;
    tableParams.filter = '';
    tableParams.sort = '';
    tableParams.order = '';
}

function getNewWashBatchCreationMasterData(serviceWOId) {
    $('.disabled-applicable').prop("disabled", true);
    $('.readonly-applicable').prop('readonly', true);

    var url = "";
    url = "/texSWOApi/NewWashBatchCreationData/" + serviceWOId;
    axios.get(url)
        .then(function (response) {
            WashBatchCreation = response.data;
            formWashBatchCreation.find("#SWONo").val(response.data.SwoNo);

            formWashBatchCreation.find("#Id").val(response.data.Id);
            formWashBatchCreation.find("#PreSWOMasterId").val(response.data.PreSWOMasterId);
            formWashBatchCreation.find("#PreProcessRevNo").val(response.data.PreProcessRevNo);
            formWashBatchCreation.find("#RevisionNo").val(response.data.RevisionNo);
            formWashBatchCreation.find("#DeliveryPlaceId").val(response.data.DeliveryPlaceId);
            formWashBatchCreation.find("#SupplierId").val(response.data.SupplierId);
            formWashBatchCreation.find("#ContactPersonId").val(response.data.ContactPersonId);
            formWashBatchCreation.find("#BankBranchId").val(response.data.BankBranchId);
            formWashBatchCreation.find("#CalculationOfTenor").val(response.data.CalculationOfTenor);
            formWashBatchCreation.find("#CashIncentive").val(response.data.CashIncentive);
            formWashBatchCreation.find("#Amount").val(response.data.Amount);
            formWashBatchCreation.find("#DiscountPercentage").val(response.data.DiscountPercentage);
            formWashBatchCreation.find("#DiscountAmount").val(response.data.DiscountAmount);
            formWashBatchCreation.find("#LcIssuanceDate").val(response.data.LcIssuanceDate);
            formWashBatchCreation.find("#UdSubmissionDate").val(response.data.UdSubmissionDate);
            formWashBatchCreation.find("#Charges").val(response.data.Charges);
            formWashBatchCreation.find("#CountryOfOrigin").val(response.data.CountryOfOrigin);
            formWashBatchCreation.find("#WastageAllowance").val(response.data.WastageAllowance);
            formWashBatchCreation.find("#ShippingTolerance").val(response.data.ShippingTolerance);
            formWashBatchCreation.find("#OrderStatus").val(response.data.OrderStatus);

            formWashBatchCreation.find("#RevisionNeed").val(response.data.RevisionNeed);
            formWashBatchCreation.find("#ProcessId").val(response.data.ProcessId);
            formWashBatchCreation.find("#swoStatus").val(response.data.swoStatus);
            formWashBatchCreation.find("#ProcessName").val(response.data.ProcessName);
            formWashBatchCreation.find("#WithoutOb").val(response.data.WithoutOb);

            initSelect2(formWashBatchCreation.find("#BuyerId"), response.data.BuyerList);
            formWashBatchCreation.find("#BuyerId").val(response.data.BuyerId).trigger("change");

            initSelect2(formWashBatchCreation.find("#BuyerTeamId"), response.data.BuyerTeamList);
            formWashBatchCreation.find("#BuyerTeamId").val(response.data.BuyerTeamId).trigger("change");

            formWashBatchCreation.find("#CompanyId").val(response.data.CompanyId);
            formWashBatchCreation.find("#ExportOrderId").val(response.data.ExportOrderId);

            formWashBatchCreation.find("#ExportOrderNo").val(response.data.ExportOrderNo);
            formWashBatchCreation.find("#ReferenceNo").val(response.data.ReferenceNo);
            formWashBatchCreation.find("#Remarks").val(response.data.Remarks);

            if (response.data.SwoDate)
                formWashBatchCreation.find("#SWODate").val(moment(response.data.SwoDate).format('MM/DD/YYYY'));

            if (response.data.DeliveryDate)
                formWashBatchCreation.find("#DeliveryDate").val(moment(response.data.DeliveryDate).format('MM/DD/YYYY'));

            if (response.data.PiSubmissionDate)
                formWashBatchCreation.find("#PISubmissionDate").val(moment(response.data.PiSubmissionDate).format('MM/DD/YYYY'));

            initSelect2(formWashBatchCreation.find("#IncoTermsId"), response.data.IncoTermsList);
            formWashBatchCreation.find("#IncoTermsId").val(response.data.IncoTermsId).trigger("change");

            initSelect2(formWashBatchCreation.find("#PaymentTermsId"), response.data.PaymentTermsList);
            formWashBatchCreation.find("#PaymentTermsId").val(response.data.PaymentTermsId).trigger("change");

            initSelect2(formWashBatchCreation.find("#TypeOfLCId"), response.data.LCTypeList);
            formWashBatchCreation.find("#TypeOfLCId").val(response.data.TypeOfLcid).trigger("change");

            initSelect2(formWashBatchCreation.find("#CurrencyId"), response.data.CurrencyTypeList);
            formWashBatchCreation.find("#CurrencyId").val(response.data.CurrencyId).trigger("change");

            initSelect2(formWashBatchCreation.find("#CreditDays"), response.data.LCTenureList);
            formWashBatchCreation.find("#CreditDays").val(response.data.CreditDays).trigger("change");

            initSelect2(formWashBatchCreation.find("#OfferValidity"), response.data.OfferValidityList);
            formWashBatchCreation.find("#OfferValidity").val(response.data.OfferValidity).trigger("change");

            initSelect2(formWashBatchCreation.find("#PaymentInstrumentId"), response.data.PaymentInstrumentList);
            formWashBatchCreation.find("#PaymentInstrumentId").val(response.data.PaymentInstrumentId).trigger("change");

            initSelect2(formWashBatchCreation.find("#PartialShipmentStatusId"), response.data.PartialShipmentAllowList);
            formWashBatchCreation.find("#PartialShipmentStatusId").val(response.data.PartialShipmentStatusId).trigger("change");

            iniTblWashBatchCreationChilds();
            formWashBatchCreation.find("#tblWashBatchCreationChilds").bootstrapTable('load', response.data.WashBatchCreationChilds);

            formWashBatchCreation.find("#btnWashBatchCreationAddValueItem").fadeOut();
            $("#btnRejectWashBatchCreation").fadeIn();
            $("#btnAcknowledgeWashBatchCreation").fadeIn();
            $("#btnSaveWashBatchCreation").fadeOut();
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function getWashBatchCreationMasterDataEdit(serviceWOId) {
    $('.disabled-applicable').prop("disabled", true);
    $('.readonly-applicable').prop('readonly', true);

    var url = "";
    url = "/texSWOApi/WashBatchCreationDataEdit/" + serviceWOId;
    axios.get(url)
        .then(function (response) {
            WashBatchCreation = response.data;
            formWashBatchCreation.find("#SWONo").val(response.data.SwoNo);

            formWashBatchCreation.find("#Id").val(response.data.Id);
            formWashBatchCreation.find("#PreSWOMasterId").val(response.data.PreSWOMasterId);
            formWashBatchCreation.find("#PreProcessRevNo").val(response.data.PreProcessRevNo);
            formWashBatchCreation.find("#RevisionNo").val(response.data.RevisionNo);
            formWashBatchCreation.find("#DeliveryPlaceId").val(response.data.DeliveryPlaceId);
            formWashBatchCreation.find("#SupplierId").val(response.data.SupplierId);
            formWashBatchCreation.find("#ContactPersonId").val(response.data.ContactPersonId);
            formWashBatchCreation.find("#BankBranchId").val(response.data.BankBranchId);
            formWashBatchCreation.find("#CalculationOfTenor").val(response.data.CalculationOfTenor);
            formWashBatchCreation.find("#CashIncentive").val(response.data.CashIncentive);
            formWashBatchCreation.find("#Amount").val(response.data.Amount);
            formWashBatchCreation.find("#DiscountPercentage").val(response.data.DiscountPercentage);
            formWashBatchCreation.find("#DiscountAmount").val(response.data.DiscountAmount);
            formWashBatchCreation.find("#LcIssuanceDate").val(response.data.LcIssuanceDate);
            formWashBatchCreation.find("#UdSubmissionDate").val(response.data.UdSubmissionDate);
            formWashBatchCreation.find("#Charges").val(response.data.Charges);
            formWashBatchCreation.find("#CountryOfOrigin").val(response.data.CountryOfOrigin);
            formWashBatchCreation.find("#WastageAllowance").val(response.data.WastageAllowance);
            formWashBatchCreation.find("#ShippingTolerance").val(response.data.ShippingTolerance);
            formWashBatchCreation.find("#OrderStatus").val(response.data.OrderStatus);

            formWashBatchCreation.find("#RevisionNeed").val(response.data.RevisionNeed);
            formWashBatchCreation.find("#ProcessId").val(response.data.ProcessId);
            formWashBatchCreation.find("#swoStatus").val(response.data.swoStatus);
            formWashBatchCreation.find("#ProcessName").val(response.data.ProcessName);
            formWashBatchCreation.find("#WithoutOb").val(response.data.WithoutOb);

            initSelect2(formWashBatchCreation.find("#BuyerId"), response.data.BuyerList);
            formWashBatchCreation.find("#BuyerId").val(response.data.BuyerId).trigger("change");

            initSelect2(formWashBatchCreation.find("#BuyerTeamId"), response.data.BuyerTeamList);
            formWashBatchCreation.find("#BuyerTeamId").val(response.data.BuyerTeamId).trigger("change");

            formWashBatchCreation.find("#CompanyId").val(response.data.CompanyId);
            formWashBatchCreation.find("#ExportOrderId").val(response.data.ExportOrderId);

            formWashBatchCreation.find("#ExportOrderNo").val(response.data.ExportOrderNo);
            formWashBatchCreation.find("#ReferenceNo").val(response.data.ReferenceNo);
            formWashBatchCreation.find("#Remarks").val(response.data.Remarks);

            if (response.data.SwoDate)
                formWashBatchCreation.find("#SWODate").val(moment(response.data.SwoDate).format('MM/DD/YYYY'));

            if (response.data.DeliveryDate)
                formWashBatchCreation.find("#DeliveryDate").val(moment(response.data.DeliveryDate).format('MM/DD/YYYY'));

            if (response.data.PiSubmissionDate)
                formWashBatchCreation.find("#PISubmissionDate").val(moment(response.data.PiSubmissionDate).format('MM/DD/YYYY'));

            initSelect2(formWashBatchCreation.find("#IncoTermsId"), response.data.IncoTermsList);
            formWashBatchCreation.find("#IncoTermsId").val(response.data.IncoTermsId).trigger("change");

            initSelect2(formWashBatchCreation.find("#PaymentTermsId"), response.data.PaymentTermsList);
            formWashBatchCreation.find("#PaymentTermsId").val(response.data.PaymentTermsId).trigger("change");

            initSelect2(formWashBatchCreation.find("#TypeOfLCId"), response.data.LCTypeList);
            formWashBatchCreation.find("#TypeOfLCId").val(response.data.TypeOfLcid).trigger("change");

            initSelect2(formWashBatchCreation.find("#CurrencyId"), response.data.CurrencyTypeList);
            formWashBatchCreation.find("#CurrencyId").val(response.data.CurrencyId).trigger("change");

            initSelect2(formWashBatchCreation.find("#CreditDays"), response.data.LCTenureList);
            formWashBatchCreation.find("#CreditDays").val(response.data.CreditDays).trigger("change");

            initSelect2(formWashBatchCreation.find("#OfferValidity"), response.data.OfferValidityList);
            formWashBatchCreation.find("#OfferValidity").val(response.data.OfferValidity).trigger("change");

            initSelect2(formWashBatchCreation.find("#PaymentInstrumentId"), response.data.PaymentInstrumentList);
            formWashBatchCreation.find("#PaymentInstrumentId").val(response.data.PaymentInstrumentId).trigger("change");

            initSelect2(formWashBatchCreation.find("#PartialShipmentStatusId"), response.data.PartialShipmentAllowList);
            formWashBatchCreation.find("#PartialShipmentStatusId").val(response.data.PartialShipmentStatusId).trigger("change");

            iniTblWashBatchCreationChilds();
            formWashBatchCreation.find("#tblWashBatchCreationChilds").bootstrapTable('load', response.data.WashBatchCreationChilds);

            if (swoStatus == "4") {
                formWashBatchCreation.find("#btnWashBatchCreationAddValueItem").fadeIn();
                $("#btnRejectWashBatchCreation").fadeOut();
                $("#btnAcknowledgeWashBatchCreation").fadeOut();
                $("#btnSaveWashBatchCreation").fadeIn();
            }

            if (swoStatus == "2" || swoStatus == "3") {
                formWashBatchCreation.find("#btnWashBatchCreationAddValueItem").fadeOut();
                $("#btnRejectWashBatchCreation").fadeOut();
                $("#btnAcknowledgeWashBatchCreation").fadeOut();
                $("#btnSaveWashBatchCreation").fadeOut();
            }
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function iniTblWashBatchCreationChilds() {
    formWashBatchCreation.find("#tblWashBatchCreationChilds").bootstrapTable({
        showFooter: true,
        columns: [
            {
                field: "SwoNo",
                title: "SWO No",
                filterControl: "input"
            },
            {
                field: "SwoDateStr",
                title: "SWO Date",
                filterControl: "input"
            },
            {
                field: "ExportOrderNo",
                title: "Export Order No",
                filterControl: "input"
            },
            {
                field: "BuyerName",
                title: "Buyer",
                filterControl: "input"
            },
            {
                field: "BuyerTeamName",
                title: "Buyer Team",
                filterControl: "input"
            }

        ]
    });
}

function iniTblWashBatchCreationChildEWOs() {
    formWashBatchCreation.find("#tblWashBatchCreationChildEWOs").bootstrapTable({
        showFooter: true,
        columns: [
            {
                field: "ExportOrderNo",
                title: "Export Order No"
            },
            {
                field: "RateApply",
                title: "Rate Apply"
            },
            {
                field: "StyleNo",
                title: "Style No"
            },
            {
                field: "WashType",
                title: "Process Type"
            },
            {
                field: "WoQty",
                title: "WO Qty",
                align: 'right',
                footerFormatter: calculateTotalServiceWOQty
            },
            {
                field: "Rate",
                title: "Rate",
                align: 'center',
            },
            {
                field: "UnitDesc",
                title: "Unit"
            },
            {
                field: "TotalValue",
                title: "Total Value ($)",
                align: 'right',
                footerFormatter: calculateTotalServiceWOTotalValue
            },
            {
                field: "Wastage",
                title: "Wastage (%)",
                align: 'center'
            },
            {
                field: "ForOb",
                title: "PO"
            },
            {
                field: "ForColor",
                title: "Color"
            }

        ]
        //onEditableSave: function (field, row, oldValue, $el) {
        //    row.PIValueN = row.PIQtyN * row.PIRateN;

        //    if (row.PIQtyN > row.PoQty) {
        //        bootbox.alert({
        //            size: "small",
        //            title: "Alert !!!",
        //            message: "PI Qty can't more than PO Qty !!!",
        //            callback: function () {
        //                row.PIQtyN = "0";
        //            }
        //        })
        //    }

        //    $("#tblyarnPIItems").bootstrapTable('load', yarnPiMaster.YarnPOChilds);
        //}
    });
}

function WashBatchCreationResetForm() {
    formWashBatchCreation.trigger("reset");
    formWashBatchCreation.find("#Id").val(-1111);
    formWashBatchCreation.find("#EntityState").val(4);
}

function calculateTotalServiceWOQty(data) {
    var WoQty = 0;

    $.each(data, function (i, row) {
        WoQty += isNaN(parseFloat(row.WoQty)) ? 0 : parseFloat(row.WoQty);
    });

    return WoQty.toFixed(2);
}

function calculateTotalServiceWOTotalValue(data) {
    var TotalValue = 0;

    $.each(data, function (i, row) {
        TotalValue += isNaN(parseFloat(row.TotalValue)) ? 0 : parseFloat(row.TotalValue);
    });

    return TotalValue.toFixed(2);
}
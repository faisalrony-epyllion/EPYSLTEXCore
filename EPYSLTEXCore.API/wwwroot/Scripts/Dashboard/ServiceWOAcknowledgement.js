var formTextileServiceWO;
var textileServiceWO;
var swoStatus = 1;
var filterBy = {};
var tableParams = {
    offset: 0,
    limit: 10,
    sort: '',
    order: '',
    filter: ''
}
var TextileServiceWoChilds = [];

$(function () {
    formTextileServiceWO = $("#formTextileServiceWO");
    formTextileServiceWOItemModal = $("#formTextileServiceWOItemModal");

    initTextileServiceWOMasterTable();
    getTextileServiceWOMasterData();
    iniTblTextileServiceWOChilds();

    $("#btnEditCancelTextileServiceWO").on("click", function (e) {
        e.preventDefault();
        backToListTextileServiceWO();
    });

    $("#btnTextileServiceWONew").on("click", function (e) {
        e.preventDefault();
        TextileServiceWOResetForm();

        var today = new Date();
        var datetoday = (today.getMonth() + 1) + '/' + today.getDate() + '/' + today.getFullYear();
        formTextileServiceWO.find("#SWODate").val(datetoday);
        formTextileServiceWO.find("#PISubmissionDate").val(datetoday);
        formTextileServiceWO.find("#DeliveryDate").val(datetoday);

        $("#divEditTextileServiceWO").fadeIn();
        $("#divtblTextileServiceWO").fadeOut();
        $("#divButtonExecutionsTextileServiceWO").fadeIn();
        getNewTextileServiceWOData();
    });

    $("#btnAcknowledgeTextileServiceWO").click(function (e) {
        e.preventDefault();
        var data = formDataToJson(formTextileServiceWO.serializeArray());

        data["SwoDate"] = formTextileServiceWO.find("#SWODate").val();
        data["DeliveryDate"] = formTextileServiceWO.find("#DeliveryDate").val();

        data["BuyerId"] = formTextileServiceWO.find("#BuyerId").val();
        data["BuyerTeamId"] = formTextileServiceWO.find("#BuyerTeamId").val();
        data["IncoTermsId"] = formTextileServiceWO.find("#IncoTermsId").val();
        data["PaymentTermsId"] = formTextileServiceWO.find("#PaymentTermsId").val();
        data["TypeOfLCId"] = formTextileServiceWO.find("#TypeOfLCId").val();
        data["CurrencyId"] = formTextileServiceWO.find("#CurrencyId").val();
        data["CreditDays"] = formTextileServiceWO.find("#CreditDays").val();
        data["OfferValidity"] = formTextileServiceWO.find("#OfferValidity").val();
        data["PaymentInstrumentId"] = formTextileServiceWO.find("#PaymentInstrumentId").val();
        data["PartialShipmentStatusId"] = formTextileServiceWO.find("#PartialShipmentStatusId").val();

        data["TextileServiceWoChilds"] = textileServiceWO.TextileServiceWoChilds;
        data["TextileServiceWoProcesses"] = textileServiceWO.TextileServiceWoProcesses;
        data["TextileServiceWoSubProcessChilds"] = textileServiceWO.TextileServiceWoSubProcessChilds;

        var config = { headers: { 'Content-Type': 'application/json' } };
        axios.post("/texSWOApi/TextileServiceWOSave", data, config)
            .then(function () {
                toastr.success("Your Service WO Acknowledged successfully.");
                backToListTextileServiceWO();
            })
            .catch(showResponseError);
    });

    $("#btnRejectTextileServiceWO").click(function (e) {
        e.preventDefault();

        bootbox.prompt("Are you sure you want to reject this?", function (result) {
            if (!result) {
                return toastr.error("Reject reason is required.");
            }

            var data = formDataToJson(formTextileServiceWO.serializeArray());

            data["SwoDate"] = formTextileServiceWO.find("#SWODate").val();
            data["DeliveryDate"] = formTextileServiceWO.find("#DeliveryDate").val();

            data["BuyerId"] = formTextileServiceWO.find("#BuyerId").val();
            data["BuyerTeamId"] = formTextileServiceWO.find("#BuyerTeamId").val();
            data["IncoTermsId"] = formTextileServiceWO.find("#IncoTermsId").val();
            data["PaymentTermsId"] = formTextileServiceWO.find("#PaymentTermsId").val();
            data["TypeOfLCId"] = formTextileServiceWO.find("#TypeOfLCId").val();
            data["CurrencyId"] = formTextileServiceWO.find("#CurrencyId").val();
            data["CreditDays"] = formTextileServiceWO.find("#CreditDays").val();
            data["OfferValidity"] = formTextileServiceWO.find("#OfferValidity").val();
            data["PaymentInstrumentId"] = formTextileServiceWO.find("#PaymentInstrumentId").val();
            data["PartialShipmentStatusId"] = formTextileServiceWO.find("#PartialShipmentStatusId").val();

            data["TextileServiceWoChilds"] = textileServiceWO.TextileServiceWoChilds;
            data["TextileServiceWoProcesses"] = textileServiceWO.TextileServiceWoProcesses;
            data["TextileServiceWoSubProcessChilds"] = textileServiceWO.TextileServiceWoSubProcessChilds;

            data.PreSWOMasterId = formTextileServiceWO.find("#PreSWOMasterId").val();
            data.RejectReason = result;

            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/texSWOApi/TextileServiceWOReject", data, config)
                .then(function () {
                    toastr.warning(constants.REJECT_SUCCESSFULLY);
                    backToListTextileServiceWO();
                })
                .catch(showResponseError);
        });
    });

    formTextileServiceWO.find("#btnTextileServiceWOAddValueItem").on("click", function (e) {
        e.preventDefault();
        formTextileServiceWOItemModal.trigger("reset");
        getExportOrderLists(formTextileServiceWO.find("#BuyerId").val(), formTextileServiceWO.find("#BuyerTeamId").val());
        $("#modal-textile-service-wo-items-information").modal('show');
    });

    $("#btnTextileServiceWOPending").on("click", function (e) {
        e.preventDefault();
        swoStatus = 1;
        initTextileServiceWOMasterTable();
        getTextileServiceWOMasterData();
    });

    $("#btnTextileServiceWOApproved").on("click", function (e) {
        e.preventDefault();
        swoStatus = 2;
        initTextileServiceWOMasterTable();
        getTextileServiceWOMasterData();
    });

    $("#btnTextileServiceWOReject").on("click", function (e) {
        e.preventDefault();
        swoStatus = 3;
        initTextileServiceWOMasterTable();
        getTextileServiceWOMasterData();
    });

    $("#btnTextileServiceWORevise").on("click", function (e) {
        e.preventDefault();
        swoStatus = 4;
        initTextileServiceWOMasterTable();
        getTextileServiceWOMasterData();
    });

    $("#btnTextileServiceWOAddItemData").click(function (e) {
        e.preventDefault;
        var exportOrderId = formTextileServiceWOItemModal.find("#ExportOrderIdServiceWO").select2('data')[0];
        var rateApplyId = formTextileServiceWOItemModal.find("#RateApplyId").select2('data')[0];
        var woUnitId = formTextileServiceWOItemModal.find("#WOUnitId").select2('data')[0];
        formTextileServiceWO.find("#ExportOrderNo").val(exportOrderId.text);

        var forPO = formTextileServiceWOItemModal.find("#ForPO").val();
        if (forPO == "on") {
            var forPoMsg = "All";
        }

        var forColor = formTextileServiceWOItemModal.find("#ForColor").val();
        if (forColor == "on") {
            var forColorMsg = "All";
        }

        var textileServiceWOChildItem = {
            ExportOrderID: exportOrderId.id,
            ExportOrderNo: exportOrderId.text,
            RateApplyID: rateApplyId.id,
            RateApply: rateApplyId.text,
            WOUnitID: woUnitId.id,
            UnitDesc: woUnitId.text,

            WoQty: formTextileServiceWOItemModal.find("#WOQty").val(),
            Rate: formTextileServiceWOItemModal.find("#Rate").val(),
            TotalValue: formTextileServiceWOItemModal.find("#WOQty").val() * formTextileServiceWOItemModal.find("#Rate").val(),
            Wastage: formTextileServiceWOItemModal.find("#Wastage").val(),
            ForOb: forPoMsg,
            ForColor: forColorMsg
        };

        textileServiceWO.TextileServiceWoChilds.push(textileServiceWOChildItem);
        formTextileServiceWO.find("#tblTextileServiceWOChilds").bootstrapTable('load', textileServiceWO.TextileServiceWoChilds);
        $("#modal-textile-service-wo-items-information").modal('hide');
    });

    formTextileServiceWO.find("#BuyerId").on("select2:select", function (e) {
        getBuyerTeamFromBuyer(e.params.data.id);
    });
});
function getBuyerTeamFromBuyer(buyerId) {
    url = "/texSWOApi/GetBuyerTeamFromBuyerServiceWO/" + buyerId;
    axios.get(url)
        .then(function (response) {
            if (formTextileServiceWO.find("#BuyerTeamId").hasClass("select2-hidden-accessible")) {
                formTextileServiceWO.find("#BuyerTeamId").empty();
            };

            initSelect2(formTextileServiceWO.find("#BuyerTeamId"), response.data.BuyerTeamList);
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

function getNewTextileServiceWOData() {
    $('.readonly-applicable').removeAttr('readonly', false);
    $('.disabled-applicable').removeAttr('disabled');

    url = "/texSWOApi/NewTextileServiceWOHouseKeeping/";

    axios.get(url)
        .then(function (response) {
            textileServiceWO = response.data;
            var TextileServiceWoChilds = [];
            formTextileServiceWO.find("#btnTextileServiceWOAddValueItem").fadeIn();
            initSelect2(formTextileServiceWO.find("#BuyerId"), response.data.BuyerList);
            initSelect2(formTextileServiceWO.find("#BuyerTeamId"), response.data.BuyerTeamList);
            initSelect2(formTextileServiceWO.find("#IncoTermsId"), response.data.IncoTermsList);
            initSelect2(formTextileServiceWO.find("#PaymentTermsId"), response.data.PaymentTermsList);
            initSelect2(formTextileServiceWO.find("#TypeOfLCId"), response.data.LCTypeList);
            initSelect2(formTextileServiceWO.find("#CurrencyId"), response.data.CurrencyTypeList);
            initSelect2(formTextileServiceWO.find("#CreditDays"), response.data.LCTenureList);
            initSelect2(formTextileServiceWO.find("#OfferValidity"), response.data.OfferValidityList);
            initSelect2(formTextileServiceWO.find("#PaymentInstrumentId"), response.data.PaymentInstrumentList);
            initSelect2(formTextileServiceWO.find("#PartialShipmentStatusId"), response.data.PartialShipmentAllowList);

            initSelect2(formTextileServiceWOItemModal.find("#RateApplyId"), response.data.ServiceWORateApplyList);
            initSelect2(formTextileServiceWOItemModal.find("#WOUnitId"), response.data.ServiceWOUnitList);

            $("#btnRejectTextileServiceWO").fadeOut();
            $("#btnAcknowledgeTextileServiceWO").fadeOut();
            $("#btnSaveTextileServiceWO").fadeIn();
            formTextileServiceWO.find("#tblTextileServiceWOChilds").bootstrapTable('destroy');
            iniTblTextileServiceWOChilds();
            formTextileServiceWO.find("#tblTextileServiceWOChilds").bootstrapTable('load', response.data.TextileServiceWoChilds);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function backToListTextileServiceWO() {
    $("#divtblTextileServiceWO").fadeIn();
    $("#divEditTextileServiceWO").fadeOut();
    $("#divButtonExecutionsTextileServiceWO").fadeOut();

    getTextileServiceWOMasterData();
}

function initTextileServiceWOMasterTable() {
    $("#tblTextileServiceWO").bootstrapTable('destroy');
    $("#tblTextileServiceWO").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#TextileServiceWOToolbar",
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
                        TextileServiceWOResetForm();
                        $("#divtblTextileServiceWO").fadeOut();
                        $("#divEditTextileServiceWO").fadeIn();
                        $("#divButtonExecutionsTextileServiceWO").fadeIn();
                        formTextileServiceWO.find("#SWOMasterId").val(row.SWOMasterId);
                        formTextileServiceWO.find("#SWOMasterID").val(row.SWOMasterID);
                        if (row.SWOMasterID == "0") {
                            getNewTextileServiceWOMasterData(row.SWOMasterId);
                        }
                        else {
                            getTextileServiceWOMasterDataEdit(row.SWOMasterID);
                        }
                    }
                }
            },
            {
                field: "SwoNo",
                title: "SWO No",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "SwoDateStr",
                title: "SWO Date",
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

            getTextileServiceWOMasterData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getTextileServiceWOMasterData();
        },
        onRefresh: function () {
            resetTextileServiceWOTableParams();
            getTextileServiceWOMasterData();
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

            getTextileServiceWOMasterData();
        }
    });
}

function getTextileServiceWOMasterData() {
    var queryParams = $.param(tableParams);
    $('#tblTextileServiceWO').bootstrapTable('showLoading');
    var url = "/texSWOApi/TextileWashServiceWOLists" + "?swoStatus=" + swoStatus + "&" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblTextileServiceWO").bootstrapTable('load', response.data);
            $('#tblTextileServiceWO').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function resetTextileServiceWOTableParams() {
    tableParams.offset = 0;
    tableParams.limit = 10;
    tableParams.filter = '';
    tableParams.sort = '';
    tableParams.order = '';
}

function getNewTextileServiceWOMasterData(serviceWOId) {
    $('.disabled-applicable').prop("disabled", true);
    $('.readonly-applicable').prop('readonly', true);

    var url = "";
    url = "/texSWOApi/NewTextileServiceWOData/" + serviceWOId;
    axios.get(url)
        .then(function (response) {
            textileServiceWO = response.data;
            formTextileServiceWO.find("#SWONo").val(response.data.SwoNo);

            formTextileServiceWO.find("#SWOMasterID").val(response.data.SWOMasterID);
            formTextileServiceWO.find("#PreSWOMasterId").val(response.data.PreSWOMasterId);
            formTextileServiceWO.find("#PreProcessRevNo").val(response.data.PreProcessRevNo);
            formTextileServiceWO.find("#RevisionNo").val(response.data.RevisionNo);
            formTextileServiceWO.find("#DeliveryPlaceId").val(response.data.DeliveryPlaceId);
            formTextileServiceWO.find("#SupplierId").val(response.data.SupplierId);
            formTextileServiceWO.find("#ContactPersonId").val(response.data.ContactPersonId);
            formTextileServiceWO.find("#BankBranchId").val(response.data.BankBranchId);
            formTextileServiceWO.find("#CalculationOfTenor").val(response.data.CalculationOfTenor);
            formTextileServiceWO.find("#CashIncentive").val(response.data.CashIncentive);
            formTextileServiceWO.find("#Amount").val(response.data.Amount);
            formTextileServiceWO.find("#DiscountPercentage").val(response.data.DiscountPercentage);
            formTextileServiceWO.find("#DiscountAmount").val(response.data.DiscountAmount);
            formTextileServiceWO.find("#LcIssuanceDate").val(response.data.LcIssuanceDate);
            formTextileServiceWO.find("#UdSubmissionDate").val(response.data.UdSubmissionDate);
            formTextileServiceWO.find("#Charges").val(response.data.Charges);
            formTextileServiceWO.find("#CountryOfOrigin").val(response.data.CountryOfOrigin);
            formTextileServiceWO.find("#WastageAllowance").val(response.data.WastageAllowance);
            formTextileServiceWO.find("#ShippingTolerance").val(response.data.ShippingTolerance);
            formTextileServiceWO.find("#OrderStatus").val(response.data.OrderStatus);

            formTextileServiceWO.find("#RevisionNeed").val(response.data.RevisionNeed);
            formTextileServiceWO.find("#ProcessId").val(response.data.ProcessId);
            formTextileServiceWO.find("#SwoStatus").val(response.data.SwoStatus);
            formTextileServiceWO.find("#ProcessName").val(response.data.ProcessName);
            formTextileServiceWO.find("#WithoutOb").val(response.data.WithoutOb);

            initSelect2(formTextileServiceWO.find("#BuyerId"), response.data.BuyerList);
            formTextileServiceWO.find("#BuyerId").val(response.data.BuyerId).trigger("change");

            initSelect2(formTextileServiceWO.find("#BuyerTeamId"), response.data.BuyerTeamList);
            formTextileServiceWO.find("#BuyerTeamId").val(response.data.BuyerTeamId).trigger("change");

            formTextileServiceWO.find("#CompanyId").val(response.data.CompanyId);
            formTextileServiceWO.find("#ExportOrderId").val(response.data.ExportOrderId);

            formTextileServiceWO.find("#ExportOrderNo").val(response.data.ExportOrderNo);
            formTextileServiceWO.find("#ReferenceNo").val(response.data.ReferenceNo);
            formTextileServiceWO.find("#Remarks").val(response.data.Remarks);

            if (response.data.SwoDate)
                formTextileServiceWO.find("#SWODate").val(moment(response.data.SwoDate).format('MM/DD/YYYY'));

            if (response.data.DeliveryDate)
                formTextileServiceWO.find("#DeliveryDate").val(moment(response.data.DeliveryDate).format('MM/DD/YYYY'));

            if (response.data.PiSubmissionDate)
                formTextileServiceWO.find("#PISubmissionDate").val(moment(response.data.PiSubmissionDate).format('MM/DD/YYYY'));

            initSelect2(formTextileServiceWO.find("#IncoTermsId"), response.data.IncoTermsList);
            formTextileServiceWO.find("#IncoTermsId").val(response.data.IncoTermsId).trigger("change");

            initSelect2(formTextileServiceWO.find("#PaymentTermsId"), response.data.PaymentTermsList);
            formTextileServiceWO.find("#PaymentTermsId").val(response.data.PaymentTermsId).trigger("change");

            initSelect2(formTextileServiceWO.find("#TypeOfLCId"), response.data.LCTypeList);
            formTextileServiceWO.find("#TypeOfLCId").val(response.data.TypeOfLcid).trigger("change");

            initSelect2(formTextileServiceWO.find("#CurrencyId"), response.data.CurrencyTypeList);
            formTextileServiceWO.find("#CurrencyId").val(response.data.CurrencyId).trigger("change");

            initSelect2(formTextileServiceWO.find("#CreditDays"), response.data.LCTenureList);
            formTextileServiceWO.find("#CreditDays").val(response.data.CreditDays).trigger("change");

            initSelect2(formTextileServiceWO.find("#OfferValidity"), response.data.OfferValidityList);
            formTextileServiceWO.find("#OfferValidity").val(response.data.OfferValidity).trigger("change");

            initSelect2(formTextileServiceWO.find("#PaymentInstrumentId"), response.data.PaymentInstrumentList);
            formTextileServiceWO.find("#PaymentInstrumentId").val(response.data.PaymentInstrumentId).trigger("change");

            initSelect2(formTextileServiceWO.find("#PartialShipmentStatusId"), response.data.PartialShipmentAllowList);
            formTextileServiceWO.find("#PartialShipmentStatusId").val(response.data.PartialShipmentStatusId).trigger("change");

            iniTblTextileServiceWOChilds();
            formTextileServiceWO.find("#tblTextileServiceWOChilds").bootstrapTable('load', response.data.TextileServiceWoChilds);

            formTextileServiceWO.find("#btnTextileServiceWOAddValueItem").fadeOut();
            $("#btnRejectTextileServiceWO").fadeIn();
            $("#btnAcknowledgeTextileServiceWO").fadeIn();
            $("#btnSaveTextileServiceWO").fadeOut();
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function getTextileServiceWOMasterDataEdit(serviceWOId) {
    $('.disabled-applicable').prop("disabled", true);
    $('.readonly-applicable').prop('readonly', true);

    var url = "";
    url = "/texSWOApi/TextileServiceWODataEdit/" + serviceWOId;
    axios.get(url)
        .then(function (response) {
            textileServiceWO = response.data;
            formTextileServiceWO.find("#SWONo").val(response.data.SwoNo);

            formTextileServiceWO.find("#SWOMasterID").val(response.data.SWOMasterID);
            formTextileServiceWO.find("#PreSWOMasterId").val(response.data.PreSWOMasterId);
            formTextileServiceWO.find("#PreProcessRevNo").val(response.data.PreProcessRevNo);
            formTextileServiceWO.find("#RevisionNo").val(response.data.RevisionNo);
            formTextileServiceWO.find("#DeliveryPlaceId").val(response.data.DeliveryPlaceId);
            formTextileServiceWO.find("#SupplierId").val(response.data.SupplierId);
            formTextileServiceWO.find("#ContactPersonId").val(response.data.ContactPersonId);
            formTextileServiceWO.find("#BankBranchId").val(response.data.BankBranchId);
            formTextileServiceWO.find("#CalculationOfTenor").val(response.data.CalculationOfTenor);
            formTextileServiceWO.find("#CashIncentive").val(response.data.CashIncentive);
            formTextileServiceWO.find("#Amount").val(response.data.Amount);
            formTextileServiceWO.find("#DiscountPercentage").val(response.data.DiscountPercentage);
            formTextileServiceWO.find("#DiscountAmount").val(response.data.DiscountAmount);
            formTextileServiceWO.find("#LcIssuanceDate").val(response.data.LcIssuanceDate);
            formTextileServiceWO.find("#UdSubmissionDate").val(response.data.UdSubmissionDate);
            formTextileServiceWO.find("#Charges").val(response.data.Charges);
            formTextileServiceWO.find("#CountryOfOrigin").val(response.data.CountryOfOrigin);
            formTextileServiceWO.find("#WastageAllowance").val(response.data.WastageAllowance);
            formTextileServiceWO.find("#ShippingTolerance").val(response.data.ShippingTolerance);
            formTextileServiceWO.find("#OrderStatus").val(response.data.OrderStatus);

            formTextileServiceWO.find("#RevisionNeed").val(response.data.RevisionNeed);
            formTextileServiceWO.find("#ProcessId").val(response.data.ProcessId);
            formTextileServiceWO.find("#SwoStatus").val(response.data.SwoStatus);
            formTextileServiceWO.find("#ProcessName").val(response.data.ProcessName);
            formTextileServiceWO.find("#WithoutOb").val(response.data.WithoutOb);

            initSelect2(formTextileServiceWO.find("#BuyerId"), response.data.BuyerList);
            formTextileServiceWO.find("#BuyerId").val(response.data.BuyerId).trigger("change");

            initSelect2(formTextileServiceWO.find("#BuyerTeamId"), response.data.BuyerTeamList);
            formTextileServiceWO.find("#BuyerTeamId").val(response.data.BuyerTeamId).trigger("change");

            formTextileServiceWO.find("#CompanyId").val(response.data.CompanyId);
            formTextileServiceWO.find("#ExportOrderId").val(response.data.ExportOrderId);

            formTextileServiceWO.find("#ExportOrderNo").val(response.data.ExportOrderNo);
            formTextileServiceWO.find("#ReferenceNo").val(response.data.ReferenceNo);
            formTextileServiceWO.find("#Remarks").val(response.data.Remarks);

            if (response.data.SwoDate)
                formTextileServiceWO.find("#SWODate").val(moment(response.data.SwoDate).format('MM/DD/YYYY'));

            if (response.data.DeliveryDate)
                formTextileServiceWO.find("#DeliveryDate").val(moment(response.data.DeliveryDate).format('MM/DD/YYYY'));

            if (response.data.PiSubmissionDate)
                formTextileServiceWO.find("#PISubmissionDate").val(moment(response.data.PiSubmissionDate).format('MM/DD/YYYY'));

            initSelect2(formTextileServiceWO.find("#IncoTermsId"), response.data.IncoTermsList);
            formTextileServiceWO.find("#IncoTermsId").val(response.data.IncoTermsId).trigger("change");

            initSelect2(formTextileServiceWO.find("#PaymentTermsId"), response.data.PaymentTermsList);
            formTextileServiceWO.find("#PaymentTermsId").val(response.data.PaymentTermsId).trigger("change");

            initSelect2(formTextileServiceWO.find("#TypeOfLCId"), response.data.LCTypeList);
            formTextileServiceWO.find("#TypeOfLCId").val(response.data.TypeOfLcid).trigger("change");

            initSelect2(formTextileServiceWO.find("#CurrencyId"), response.data.CurrencyTypeList);
            formTextileServiceWO.find("#CurrencyId").val(response.data.CurrencyId).trigger("change");

            initSelect2(formTextileServiceWO.find("#CreditDays"), response.data.LCTenureList);
            formTextileServiceWO.find("#CreditDays").val(response.data.CreditDays).trigger("change");

            initSelect2(formTextileServiceWO.find("#OfferValidity"), response.data.OfferValidityList);
            formTextileServiceWO.find("#OfferValidity").val(response.data.OfferValidity).trigger("change");

            initSelect2(formTextileServiceWO.find("#PaymentInstrumentId"), response.data.PaymentInstrumentList);
            formTextileServiceWO.find("#PaymentInstrumentId").val(response.data.PaymentInstrumentId).trigger("change");

            initSelect2(formTextileServiceWO.find("#PartialShipmentStatusId"), response.data.PartialShipmentAllowList);
            formTextileServiceWO.find("#PartialShipmentStatusId").val(response.data.PartialShipmentStatusId).trigger("change");

            iniTblTextileServiceWOChilds();
            formTextileServiceWO.find("#tblTextileServiceWOChilds").bootstrapTable('load', response.data.TextileServiceWoChilds);

            if (swoStatus == "4") {
                formTextileServiceWO.find("#btnTextileServiceWOAddValueItem").fadeIn();
                $("#btnRejectTextileServiceWO").fadeOut();
                $("#btnAcknowledgeTextileServiceWO").fadeOut();
                $("#btnSaveTextileServiceWO").fadeIn();
            }

            if (swoStatus == "2" || swoStatus == "3") {
                formTextileServiceWO.find("#btnTextileServiceWOAddValueItem").fadeOut();
                $("#btnRejectTextileServiceWO").fadeOut();
                $("#btnAcknowledgeTextileServiceWO").fadeOut();
                $("#btnSaveTextileServiceWO").fadeOut();
            }
        })
        .catch(function () {
            toastr.error(constants.LOAD_ERROR_MESSAGE);
        })
}

function iniTblTextileServiceWOChilds() {
    formTextileServiceWO.find("#tblTextileServiceWOChilds").bootstrapTable({
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
    });
}

function TextileServiceWOResetForm() {
    formTextileServiceWO.trigger("reset");
    formTextileServiceWO.find("#SWOMasterID").val(-1111);
    formTextileServiceWO.find("#EntityState").val(4);
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
(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $formEl, tblMasterId, $tblChildEl, tblChildId, $tblBuyerEl, tblBuyerId;
    var status;
    var masterData;
    var ImagesList = Array();
    var ImagesContentList = Array();

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblBuyerId = "#tblBuyer" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        status = statusConstants.PENDING;

        initMasterTable();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            initMasterTable();
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.COMPLETED;
            initMasterTable();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnCancel").on("click", backToList);
    });

    function initMasterTable() {
        var commands = [];
        if (status == statusConstants.PENDING) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }
            ];
        } else {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'ReportCHanger', title: 'Conuter Hanger', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'ReportMHanger', title: 'Marketing Hanger', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
            ];
        }

        columns = [
            {
                headerText: '', textAlign: 'Center', commands: commands,
                textAlign: 'Center', width: 60, minWidth: 60, maxWidth: 80
            },
            {
                field: 'LPPRID', headerText: 'LPPRID', visible: false
            },
            {
                field: 'LPFormID', headerText: 'LPFormID', visible: false
            },
            {
                field: 'ConceptNo', headerText: 'Concept No', width: 120
            },
            {
                field: 'ConceptDate', headerText: 'Concept Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100
            },
            {
                field: 'RequestedUser', headerText: 'Requested By', width: 100
            },
            {
                field: 'RequestDate', headerText: 'Request Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100
            },
            {
                field: 'RequiredDate', headerText: 'Required Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 120
            },
            {
                field: 'IsBuyerSpecific', headerText: 'Buyer Specific', width: 120
            },
            {
                field: 'PRRemarks', headerText: 'PRRemarks', width: 100
            },
            {
                field: 'Price', headerText: 'Price', width: 100, visible: status == statusConstants.COMPLETED
            },
            {
                field: 'ValidUpToDate', headerText: 'Validity', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 120, visible: status == statusConstants.COMPLETED
            },
            {
                field: 'PRRemarks', headerText: 'PRRemarks', width: 100, visible: status == statusConstants.COMPLETED
            },
            {
                field: 'TechnicalName', headerText: 'Fabric', width: 100
            },
            {
                field: 'ColorName', headerText: 'Color', width: 100
            },
            {
                field: 'Composition', headerText: 'Composition', width: 150
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/live-product/live-product-price-request-list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.LPPRID);
        }
        else if (args.commandColumn.type == "ReportCHanger") {
            //window.open(`/reports/InlinePdfView?ReportName=ConuterHangerFormat.rdl&FirmConceptMasterID=${args.rowData.FCID}`, '_blank');
        }
        else if (args.commandColumn.type == "ReportMHanger") {
            //window.open(`/reports/InlinePdfView?ReportName=MarketingHangerFormat.rdl&FirmConceptMasterID=${args.rowData.FCID}`, '_blank');
        }
    }
    function getDetails(id) {
        axios.get(`/api/live-product/price-request/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.RequestDate = formatDateToDefault(masterData.RequestDate);
                masterData.RequiredDate = formatDateToDefault(masterData.RequiredDate);
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
                masterData.ValidUpToDate = formatDateToDefault(masterData.ValidUpToDate);

                setFormData($formEl, masterData);
                setDefault();

                if (masterData.IsBuyerSpecific) {
                    $formEl.find(".divPrice").hide();
                } else {
                    $formEl.find(".divPrice").show();
                }

                if (masterData.LiveProductFormImages.length > 0) {
                    $formEl.find("#divImage").show();
                    if (masterData.IsBuyerSpecific) {
                        $formEl.find("#divImage").removeClass("col-sm-12 col-sm-6").addClass("col-sm-6");
                    } else {
                        $formEl.find("#divImage").removeClass("col-sm-12 col-sm-6").addClass("col-sm-12");
                    }

                    masterData.LiveProductFormImages.map(x => {
                        var sTemp = "<div class='text-center col-sm-4' style='float:left;'>";
                        sTemp += "<img src='" + x.ImagePath + "' style='height:150px;width:150px;' class='zoom1' />";
                        sTemp += "</div>";
                        $formEl.find("#divParentImage").append(sTemp);
                    });
                }
                if (masterData.IsBuyerSpecific) {
                    $formEl.find("#divBuyer").show();

                    if (masterData.LiveProductFormImages.length > 0) {
                        $formEl.find("#divBuyer").removeClass("col-sm-12 col-sm-6").addClass("col-sm-6");
                    } else {
                        $formEl.find("#divBuyer").removeClass("col-sm-12 col-sm-6").addClass("col-sm-12");
                    }

                    var nSL = 1;
                    masterData.PriceRequestBuyers.map(x => {
                        x["SL"] = nSL++
                    });
                    loadBuyers(masterData.PriceRequestBuyers);
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function setDefault() {
        $formEl.find("#divImage").hide();
        $formEl.find("#divImage").removeClass("col-sm-12 col-sm-6").addClass("col-sm-6");
        $formEl.find("#divBuyer").hide();
        $formEl.find("#divBuyer").removeClass("col-sm-12 col-sm-6").addClass("col-sm-6");
        $formEl.find("#divParentImage div").remove();
    }
    function isValid(data) {
        if (!data.IsBuyerSpecific) {
            if (data.Price == null || data.Price == 0) {
                toastr.error("Give price.");
                $formEl.find("#Price").focus();
                return;
            }
        }
        if (data.ValidUpToDate == null || $.trim(data.ValidUpToDate).length == 0) {
            toastr.error("Give validity date.");
            $formEl.find("#ValidUpToDate").focus();
            return;
        }
        if (data.IsBuyerSpecific) {
            var buyers = $tblBuyerEl.getCurrentViewRecords();
            for (var i = 0; i < buyers.length; i++) {
                if (buyers[i].Price == 0) {
                    toastr.error("Give " + buyers[i].BuyerName + " buyer price.");
                    return;
                }
            }
        }
        return true;
    }
    function save() {
        var data = formElToJson($formEl);
        if (data.IsBuyerSpecific) {
            data.PriceRequestBuyers = $tblBuyerEl.getCurrentViewRecords();
        } 
        if (isValid(data)) {
            axios.post("/api/live-product/save-price", data)
                .then(function () {
                    toastr.success("Successfully Saved!");
                    backToList();
                })
                .catch(showResponseError);
        }
    }
    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
    }
    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#LPPRID").val(-1111);
    }
    function loadBuyers(data) {
        if ($tblBuyerEl) $tblBuyerEl.destroy();
        var columns = [
            { field: 'SL', textAlign: 'center', headerText: 'SL', width: 50, allowEditing: false },
            { field: 'PRBID', isPrimaryKey: true, visible: false },
            { field: 'BuyerName', textAlign: 'center', width: 200, headerText: 'Buyer', allowEditing: false },
            { field: 'Price', headerText: 'Price', width: 50, allowEditing: true, editType: "numericedit", textAlign: 'center', edit: { params: { showSpinButton: false, decimals: 2, min: 0 } } },
            { field: 'Remarks', width: 200, headerText: 'Remarks', allowEditing: true }
        ];
        var tableOptions = {
            tableId: tblBuyerId,
            data: data,
            columns: columns,

            dataSource: data,
            allowResizing: true,
            showColumnChooser: true,
            showDefaultToolbar: false,

            editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            actionBegin: function (args) {

            },
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false,
        };
        $tblBuyerEl = new initEJ2Grid(tableOptions);
    }
})();
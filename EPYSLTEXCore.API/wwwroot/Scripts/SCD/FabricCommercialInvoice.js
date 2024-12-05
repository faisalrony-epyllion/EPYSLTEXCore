
(function () {
    var menuId, pageName, pageId;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl,
        tblMasterId, $tblChildEl, tblChildId, $formEl, $tblFabricPIItemsEl,
        tblFabricPIItemsId, tblDocsId, tblReportId;
    var filterBy = {};
    var isAcceptancePage = false, isFabricCommercialPage = false; 
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var status;
    var masterData;
    var data;
    var invoiceNo;
    var reportData = [];
    var FabricCI;
    var uploadDocs = [];
    var _isHideRemoveBtn = false;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblFabricPIItemsId = "#tblFabricPIItems" + pageId;
        tblDocsId = "#tblDocs" + pageId;
        tblReportId = "#tblReport" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        isAcceptancePage = convertToBoolean($(`#${pageId}`).find("#AcceptancePage").val());
        isFabricCommercialPage = convertToBoolean($(`#${pageId}`).find("#FabricCommercialPage").val());
        isBankAcceptancePage = convertToBoolean($(`#${pageId}`).find("#BankAcceptancePage").val());

        $formEl.find("#btnAddItem").on("click", addItem);

        $("#" + pageId).find("#btnViewAllReport").click(function () {
            var data = reportData;
            for (var i = 0; i < data.length; i++) {
                var reportName = data[i].additionalValue;
                window.open(`/reports/InlinePdfView?ReportName=${reportName}&InvoiceNo=${invoiceNo}`, '_blank');
            }
        });
        
        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;

            initMasterTable();
        });

        $toolbarEl.find("#btnPartialCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PARTIALLY_COMPLETED;
            initMasterTable();
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            initMasterTable();
        });

        $toolbarEl.find("#btnRejectList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.REJECT;
            initMasterTable();
        });
        $toolbarEl.find("#btnAwaitingForBankAcceptance").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.AWAITING_PROPOSE;
            initMasterTable();
        });
        $toolbarEl.find("#btnAcceptedInvoice").on("click", function (e) {
            e.preventDefault();
            $formEl.find("#btnBankAccept,#btnBankReject").hide();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.APPROVED_DONE;
            initMasterTable();
        });

        $formEl.find(".cia").hide();
        if (isFabricCommercialPage == true) {
            status = statusConstants.PENDING;

            $toolbarEl.find("#btnPending").show();
            $toolbarEl.find("#btnPartialCompleteList").show();
            $toolbarEl.find("#btnList,#btnRejectList").hide();
            $formEl.find("#btnBankAccept,#btnBankReject").hide();
            $formEl.find("#OwnAcceptance").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPending"), $toolbarEl);
            $toolbarEl.find("#btnPending").click();

        } else if (isAcceptancePage == true) {
            status = statusConstants.PARTIALLY_COMPLETED;

            $toolbarEl.find("#btnList,#btnRejectList").show();
            $toolbarEl.find("#btnPartialCompleteList").show();
            $toolbarEl.find("#btnPending").hide();
            $formEl.find("#btnBankAccept,#btnBankReject").hide();
            $formEl.find("#OwnAcceptance").show();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPartialCompleteList"), $toolbarEl);
            $toolbarEl.find("#btnPartialCompleteList").click();
        }
        else if (isBankAcceptancePage == true) {
            status = statusConstants.AWAITING_PROPOSE;

            $toolbarEl.find("#btnList,#btnRejectList").show();
            $toolbarEl.find("#btnPartialCompleteList").hide();
            $toolbarEl.find("#btnPending").hide();
            $formEl.find("#btnBankAccept,#btnBankReject").show();
            $formEl.find(".cia").show();
            $formEl.find("#OwnAcceptance").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPartialCompleteList"), $toolbarEl);
            $toolbarEl.find("#btnAwaitingForBankAcceptance").click();
        }


        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnAccept").click(function (e) {
            if ($formEl.find("#OwnAcceptDate").val() == "") {
                toastr.error("Please Give Acceptance Date");
                return false;
            }
            e.preventDefault();
            approve(false);
        });
        $formEl.find("#btnBankAccept").click(function (e) {
            e.preventDefault();
            if ($formEl.find("#BankAcceptDate").val() == "") {
                toastr.error("Please Give Bank Acceptance Date");
                return false;
            }
            else if ($formEl.find("#MaturityDate").val() == "") {
                toastr.error("Please Give Bank Maturity Date");
                return false;
            }
            else if ($formEl.find("#BankRefNumber").val() == "") {
                toastr.error("Please Give Bank Maturity Number");
                return false;
            }
            approve(true);
        });

        $formEl.find("#btnReject").click(function (e) {
            e.preventDefault();
            reject(false);
        });
        $formEl.find("#btnBankReject").click(function (e) {
            e.preventDefault();
            reject(true);
        });

        $formEl.find("#btnCancel").on("click", backToList);

        $(document).on('click', 'input[type="checkbox"]', function () {
            $('input[type="checkbox"]').not(this).prop('checked', false);
        });
        operationDocumentUploads();
    });

    function initMasterTable() {
        var commands = [];
        if (status === statusConstants.PENDING) {
            commands = [
                { type: 'New', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } }
            ]
        }
        else if (status === statusConstants.PARTIALLY_COMPLETED) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file' } },
                //{ type: 'ViewAttachedFile', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file' } },
            ]
        }
        else {
            commands = [
                { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }
                //{ type: 'ViewAttachedFile', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file' } }
            ]
        }

        var columns = [
            {
                headerText: 'Actions', commands: commands
            },
            {
                field: 'CINo', headerText: 'Invoice No', visible: status != statusConstants.PENDING
            },
            {
                field: 'CIDate', headerText: 'Invoice Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'CIValue', headerText: 'Invoice Value', textAlign: 'Right', editType: "numericedit", params: { decimals: 6, format: "N", min: 1, validateDecimalOnType: true }, visible: status != statusConstants.PENDING
            },
            {
                field: 'LcNo', headerText: 'L/C No'
            },
            {
                field: 'LcDate', headerText: 'L/C Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            //{
            //    field: 'CustomerName', headerText: 'Customer'
            //},
            {
                field: "SupplierID",
                title: "Supplier ID",
                visible: false
            },
            {
                field: 'SupplierName', headerText: 'Supplier', visible: true
            },
            {
                field: "ProposeContractID",
                title: "Propose Contract",
                visible: false
            },
            {
                field: "ProposeContract",
                title: "Propose Contract No",
                visible: false
            },
            {
                field: "ProposeBankID",
                title: "Propose Bank",
                visible:false
            },
            {
                field: "BankName",
                title: "Propose Bank",
            },
            {
                field: 'CompanyName', headerText: 'Company', visible: false
            },
            //{
            //    field: 'LcQty', headerText: 'Total Qty', textAlign: 'Right'
            //},
            {
                field: 'LcValue', headerText: 'L/C Value', textAlign: 'Right'
            },
            {
                field: 'LcExpiryDate', headerText: 'L/C Expiry Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            //{
            //    field: 'LcValue', headerText: 'L/C Value', textAlign: 'Right', visible: status != statusConstants.PENDING
            //},
            //{
            //    field: 'LcQty', headerText: 'L/C Qty', textAlign: 'Right', visible: status != statusConstants.PENDING
            //},
            //{
            //    field: 'TotalCI', headerText: 'Total CI', textAlign: 'Right'
            //},
            {
                field: 'LcExpiryDate', headerText: 'L/C Expiry Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                headerText: 'LC', textAlign: 'Center', commands: [
                    { buttonOption: { type: 'Attachments', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-paperclip', tooltip: 'Attachments' } }
                ]
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/fabricCI/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        /* */
        if (args.commandColumn.type == 'New') {
            getNew(args.rowData.LCID);
        } else if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.CIID);
        } else if (args.commandColumn.type == 'View') {
            
            getDetails(args.rowData.CIID);
        } else if (args.commandColumn.type == 'Report') {
            
            getReports(args.rowData.CIID);
            //var data = reportData;
            //showReportList();
            //$('#divModalReport').modal('show');


            
            //window.open(`/reports/InlinePdfView?ReportName=CommercialInvoice.rdl&InvoiceNo=${args.rowData.CINo}`, '_blank');
        } else if (args.commandColumn.type == 'ViewAttachedFile') {
            if (args.rowData.CIFilePath == null || args.rowData.CIFilePath.length == 0) {
                toastr.error("No Attchment Found.");
                return false;
            }
            if (args.rowData.CIFilePath != null && args.rowData.CIFilePath.length > 0) {
                window.open(args.rowData.CIFilePath, '_blank')
            }
        } else if (args.commandColumn.buttonOption.type == 'Attachments') {
            //var a = document.createElement('a');
            //a.href = args.rowData.LcFilePath;
            //a.setAttribute('target', '_blank');
            //a.click();

            window.open(args.rowData.ServerPath + args.rowData.LcFilePath, '_blank')
        }
    }

    function initChildTable(data) {
        if ($tblChildEl) {
            $tblChildEl.destroy();
            $(tblChildId).html("");
        }

        ej.base.enableRipple(true);
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            editSettings: { allowAdding: true, allowDeleting: true, allowEditing: true },
            allowResizing: true,
            primaryKeyColumn: "Id",
            columns: [
                {
                    headerText: '', width: 50, commands: [
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }
                    ]
                },
                { field: 'ChildID', isPrimaryKey: true, visible: false },
                { field: 'ItemDescription', headerText: 'Item Description', textAlign: 'Left', allowEditing: false },
                { field: 'UOM', headerText: 'Unit', allowEditing: false },
                { field: 'PIQty', headerText: 'PI Qty', allowEditing: false, textAlign: 'Right' },
                { field: 'PIValue', headerText: 'PI Value', allowEditing: false, textAlign: 'Right' },
                {
                    field: 'BalPIQty', headerText: 'Balance PI Qty', textAlign: 'Right',
                    allowEditing: false, visible: status === statusConstants.PENDING
                },
                {
                    field: 'BalPIValue', headerText: 'Balance PI Value', textAlign: 'Right',
                    allowEditing: false, visible: status === statusConstants.PENDING
                },
                { field: 'Rate', headerText: 'Rate', allowEditing: false, textAlign: 'Right' },
                { field: 'InvoiceQty', headerText: 'Invoice Qty', textAlign: 'Right' },
                { field: 'PdValue', headerText: 'Invoice Value', editType: "numericedit", params: { decimals: 6, format: "N", min: 1, validateDecimalOnType: true }, textAlign: 'Right', allowEditing: false },
                { field: 'NoOfCarton', headerText: 'No Of Carton', textAlign: 'Right' },
                { field: 'NoOfCone', headerText: 'No Of Cone', textAlign: 'Right' },
                { field: 'GrossWeight', headerText: 'Gross Weight', textAlign: 'Right' },
                { field: 'NetWeight', headerText: 'Net Weight', textAlign: 'Right' }
            ],
            actionBegin: function (args) {
                var TPDValue = 0;
                var CTDValue = 0;
                if (args.requestType === "save") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.ChildID);
                    var ChildDetails = $tblChildEl.getCurrentViewRecords();
                    //CTDValue = args.data.PdValue;
                    CTDValue = args.data.InvoiceQty * args.data.Rate;
                    ChildDetails[index].PdValue = CTDValue;
                    $tblChildEl.editModule.updateRow(index, ChildDetails[index]);
                    $tblChildEl.refreshColumns;
                    $.each(ChildDetails, function (j, obj) {
                        TPDValue += obj.PdValue;
                    });
                    
                    $formEl.find("#CIValue").val(TPDValue);

                    //Modify Invoice Value (PD value)
                    var TChildPDValue = 0;
                    TChildPDValue = (args.data.Rate * args.data.InvoiceQty);
                    args.data.PdValue = TChildPDValue;

                }
                else if (args.requestType === "delete") {
                    CTDValue = args.data[0].PdValue;
                    TPDValue = $formEl.find("#CIValue").val();
                    //$formEl.find("#CIValue").val(TPDValue - CTDValue);
                }
            },
        });
        $tblChildEl["dataBound"] = function () {
            $tblChildEl.autoFitColumns();
        };
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }

    function initAttachedPIChildTable(data) {
        isEditable = true;
        if ($tblFabricPIItemsEl) {
            $tblFabricPIItemsEl.destroy();
            $(tblFabricPIItemsId).html("");
        }

        ej.base.enableRipple(true);
        $tblFabricPIItemsEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            editSettings: { allowAdding: true, allowDeleting: true, allowEditing: false },
            allowResizing: true,
            allowEditing: false,
            primaryKeyColumn: "ChildPIID",
            columns: [
                //{
                //    headerText: '', width: 50, commands: [
                //        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                //        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                //        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } }
                //    ]
                //},
                { field: 'PiNo', headerText: 'PI No', width: 20 },
                { field: 'PIDate', headerText: 'PI Date', type: 'date', format: _ch_date_format_1, width: 20 },
                { field: 'SupplierName', headerText: 'Supplier', width: 30 },
                /*{ field: 'TotalQty', headerText: 'Total Qty', width: 15, textAlign: 'Right' },*/
                { field: 'TotalValue', headerText: 'Total Value', width: 15, textAlign: 'Right' }
            ]
        });
        $tblFabricPIItemsEl["dataBound"] = function () {
            $tblFabricPIItemsEl.autoFitColumns();
        };
        $tblFabricPIItemsEl.refreshColumns;
        $tblFabricPIItemsEl.appendTo(tblFabricPIItemsId);
    }

    function initAttachment(path, type, $el) {
        if (!path) {
            initNewAttachment($el);
            return;
        }

        if (!type) type = "any";

        var preveiwData = [rootPath + path];
        var previewConfig = [{ type: type, caption: "CI Attachment", key: 1, width: "80px", frameClass: "preview-frame" }];

        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            initialPreview: preveiwData,
            initialPreviewAsData: true,
            initialPreviewFileType: 'image',
            initialPreviewConfig: previewConfig,
            purifyHtml: true,
            required: true,
            maxFileSize: 4096
        });
    }

    function initNewAttachment($el) {
        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            previewFileType: 'any'
        });
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#CIID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(Lcid) {
        axios.get(`/api/fabricCI/new/${Lcid}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                _isHideRemoveBtn = false;
                resetDocTypeCbo();
                resetDocTable();
                uploadDocs = [];

                FabricCI = response.data;
                FabricCI.LcDate = formatDateToDefault(FabricCI.LcDate);
                FabricCI.LcReceiveDate = formatDateToDefault(FabricCI.LcReceiveDate);
                FabricCI.LcExpiryDate = formatDateToDefault(FabricCI.LcExpiryDate);
                FabricCI.CIDate = formatDateToDefault(FabricCI.CIDate);
                FabricCI.ExpDate = formatDateToDefault(FabricCI.ExpDate);
                FabricCI.BOEDate = formatDateToDefault(FabricCI.BOEDate);
                loadDocTypes();

                setFormData($formEl, FabricCI);
                initChildTable(FabricCI.CIChilds);
                initAttachedPIChildTable(FabricCI.CIChildPIs);

                if (status == statusConstants.PENDING) {
                    $formEl.find("#btnSave").fadeIn();
                    $formEl.find("#btnAccept,#btnReject,#btnReport").fadeOut();
                } else if (status == statusConstants.PARTIALLY_COMPLETED) {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnAccept,#btnReject").fadeOut();
                    $formEl.find("#btnReport").fadeIn();
                    if (isAcceptancePage)
                        $formEl.find("#btnAccept,#btnReject").fadeIn();
                } else {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnAccept,#btnReject").fadeOut();
                }

                if (FabricCI.SG) {
                    $formEl.find("#SG").prop('checked', true);
                    $formEl.find("#OD").prop('checked', false);
                }
                else {
                    $formEl.find("#SG").prop('checked', false);
                    $formEl.find("#OD").prop('checked', true);
                }
                initAttachment(FabricCI.CIFilePath, FabricCI.AttachmentPreviewTemplate, $formEl.find("#UploadFile"));
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getReports(id) {
        axios.get(`/api/fabricCI/${id}`)
            .then(function (response) {
                FabricCI = response.data;
                reportData = FabricCI.ReportList;
                invoiceNo = FabricCI.CINo;
                showReportList();
                $('#divModalReport').modal('show');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getDetails(id) {
        axios.get(`/api/fabricCI/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                _isHideRemoveBtn = true;
                resetDocTypeCbo();
                resetDocTable();
                uploadDocs = [];

                FabricCI = response.data;
                FabricCI.LcDate = formatDateToDefault(FabricCI.LcDate);
                FabricCI.LcReceiveDate = formatDateToDefault(FabricCI.LcReceiveDate);
                FabricCI.LcExpiryDate = formatDateToDefault(FabricCI.LcExpiryDate);
                FabricCI.CIDate = formatDateToDefault(FabricCI.CIDate);
                FabricCI.ExpDate = formatDateToDefault(FabricCI.ExpDate);
                FabricCI.BOEDate = formatDateToDefault(FabricCI.BOEDate);
                FabricCI.BLDate = formatDateToDefault(FabricCI.BLDate);
                FabricCI.BankAcceptDate = formatDateToDefault(FabricCI.BankAcceptDate);
                FabricCI.MaturityDate = formatDateToDefault(FabricCI.MaturityDate);
                loadDocTypes();
                loadDocTables();

                invoiceNo = FabricCI.CINo;

                setFormData($formEl, FabricCI);
                initChildTable(FabricCI.CIChilds);
                initAttachedPIChildTable(FabricCI.CIChildPIs);

                if (status == statusConstants.PENDING) {
                    $formEl.find("#btnSave").fadeIn();
                    $formEl.find("#btnAccept,#btnReject,#btnReport").fadeOut();
                } else if (status == statusConstants.PARTIALLY_COMPLETED) {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnAccept,#btnReject").fadeOut();
                    $formEl.find("#btnReport").fadeIn();
                    if (isAcceptancePage)
                        $formEl.find("#btnAccept,#btnReject").fadeIn();
                } else {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnAccept,#btnReject").fadeOut();
                }
                if (FabricCI.SG) {
                    $formEl.find("#SG").prop('checked', true);
                    $formEl.find("#OD").prop('checked', false);
                }
                else {
                    $formEl.find("#SG").prop('checked', false);
                    $formEl.find("#OD").prop('checked', true);
                }
                initAttachment(FabricCI.CIFilePath, FabricCI.AttachmentPreviewTemplate, $formEl.find("#UploadFile"));

                reportData = FabricCI.ReportList;
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function loadDocTables() {
        FabricCI.FabricCIDocs.map(x => {
            generateSingleRow(x);
        });
    }

    function resetDocTable() {
        var docTbl = $(tblDocsId);
        docTbl.find("tbody tr").remove();
    }
    function resetDocTypeCbo() {
        $("#" + pageId).find("#cboDocType").find("option").remove();
    }
    function loadDocTypes() {
        resetDocTypeCbo();
        FabricCI.AllDocTypes.map(x => {
            $("#" + pageId).find("#cboDocType").append(`<option value=${x.DocTypeID}>${x.DocTypeName}</option>`);
        });
    }
    function operationDocumentUploads() {
        var thisPage = $("#" + pageId);

        thisPage.find("#btnAddDoc").click(function () { 
            thisPage.find('#divModalDoc').modal('show');
        });

        thisPage.find("#btnReport").click(function () {
            thisPage.find('#divModalReport').modal('show');
            showReportList();

        });

        thisPage.find("#modalAddDoc").click(function () {
            thisPage.find('#divModalDoc').modal('hide');

            var fabricCIDoc = {
                DocTypeID: thisPage.find("#cboDocType").val(),
                DocTypeName: thisPage.find("#cboDocType option:selected").text()
            };

            var totalfiles = document.getElementById('UploadFileDoc').files.length;
            var fileName = "";
            if (totalfiles > 0) {
                var indexF = uploadDocs.findIndex(x => x.FileName == fileName);
                if (indexF > -1) {
                    uploadDocs[indexF].File = document.getElementById('UploadFileDoc').files[0];
                }
                else {
                   
                    fileName = generateFileName(document.getElementById('UploadFileDoc').files[0].name);
                    uploadDocs.push({
                        File: document.getElementById('UploadFileDoc').files[0],
                        FileName: fileName,
                        DocTypeID: fabricCIDoc.DocTypeID
                    });
                }
            }
            fabricCIDoc.FileName = removeInvalidChar(fileName);
            generateSingleRow(fabricCIDoc);
        });
    }
    function generateSingleRow(fabricCIDoc) {
        fabricCIDoc.FabricCIDocID = typeof fabricCIDoc.FabricCIDocID === "undefined" ? 0 : fabricCIDoc.FabricCIDocID;
        fabricCIDoc.CIID = typeof fabricCIDoc.CIID === "undefined" ? 0 : fabricCIDoc.CIID;
        fabricCIDoc.ImagePath = typeof fabricCIDoc.ImagePath === "undefined" || fabricCIDoc.ImagePath == "" ? "#" : fabricCIDoc.ImagePath;

        var splitValue = fabricCIDoc.FileName.split(".");
        if (fabricCIDoc.ImagePath != "#" && splitValue.length == 1) {
            var extension = getFileExtension(fabricCIDoc.ImagePath);
            fabricCIDoc.FileName = fabricCIDoc.FileName + extension;
        }

        var docTbl = $(tblDocsId);
        var slNo = docTbl.find("tbody tr").length + 1;

        if (typeof fabricCIDoc.ImagePath === "undefined" || fabricCIDoc.ImagePath == "") {
            anchorText = `${fabricCIDoc.FileName}`;
        } else {
            anchorText = `<a href='${fabricCIDoc.ImagePath}' target="_blank" title='Show Doc'>${fabricCIDoc.FileName}</a>`;
        }

        var css = _isHideRemoveBtn ? "style='display:none'" : "";
        if (_isHideRemoveBtn) {
            docTbl.find("thead").find(".cellActionDoc").hide();
        }

        docTbl.find("tbody").append(`
                <tr rowIndex=${slNo}>
                    <td class='tdStatic tdRemoveIcon cellActionDoc' FabricCIDocID=${fabricCIDoc.FabricCIDocID} CIID=${fabricCIDoc.CIID} ${css}>
                        <button id='btnRemoveImg${slNo}' type='button' class="btn btn-danger btn-xs add btnRemoveImg" title="Remove"><i class="fa fa-trash"></i></button>
                    </td>
                    <td>${slNo}</td>
                    <td>${fabricCIDoc.DocTypeName}</td>
                    <td>
                        ${anchorText}
                    </td>
                </tr>
            `);

        docTbl.find("tbody tr[rowIndex=" + slNo + "]").find("#btnRemoveImg" + slNo).click(function () {
            if (confirm("Confirm delete ?")) {
                var fabricCIDocID = docTbl.find("tbody tr[rowIndex=" + slNo + "] .tdRemoveIcon").attr("FabricCIDocID");
                if (fabricCIDocID > 0) {
                    var indexF = FabricCI.FabricCIDocs.findIndex(x => x.FabricCIDocID == fabricCIDocID);
                    if (indexF > -1) {
                        FabricCI.FabricCIDocs[indexF].IsDelete = true;
                    }
                }
                docTbl.find("tbody tr[rowIndex=" + slNo + "]").remove();
            }
        });
    }
    function generateFileName(pFName) {
        var fName = getFileNameWithoutExtension(pFName);
        var ex = getFileExtension(pFName);
        fName = fName + "_" + Math.random().toString(16).slice(2) + ex;
        fName = removeInvalidChar(fName);
        return fName;
    }
    function removeInvalidChar(fName) {
        fName = fName.replaceAll("#", "").replace(/[^a-z0-9 ,._]/ig, '');
        return fName;
    }
    function getFileNameWithoutExtension(name) {
        var splitList = name.split('.');
        var fileName = "";
        if (splitList.length > 0) {
            extension = splitList[splitList.length - 1];
            extension = "." + extension;
            fileName = name.replace(extension, "");
        }
        return fileName;
    }
    function getFileExtension(name) {
        var splitList = name.split('.');
        var extension = "";
        if (splitList.length > 0) {
            extension = splitList[splitList.length - 1];
            extension = "." + extension;
        }
        return extension;
    }
    function addItem(e) {
        var lcNumber = $(`#${pageId}`).find("#LcNo").val();
        e.preventDefault();
        var itemIds = "";
        var finder = new commonFinder({
            title: "Select Proposal",
            pageId: pageId,
            height: 350,
            //apiEndPoint: "/api/ybblcproposal/proposallist?gridType=ej2&status=COMPLETED&isCDAPage=false",
            // url = "/api/ypi-receive/list?gridType=bootstrap-table&status=" + status + "&" + queryParams;
            apiEndPoint: `/api/fabricCI/proposallist?gridType=ej2&status=COMPLETED&lcNumber=${lcNumber}`,
            fields: "YPINo,PIDate,CompanyName,SupplierName,YPINo,PIQty,PIValue",
            headerTexts: "PI No,PI Date,Company,Supplier,PO No,Total Qty,Total Value",
            customFormats: ",dd/MM/yyyy,,,,,ej2GridColorFormatter",
            isMultiselect: true,
            primaryKeyColumn: "YPIReceiveMasterID",
            onMultiselect: function (selectedRecords) {
                FabricCI.CiChildPis = $tblFabricPIItemsEl.getCurrentViewRecords();
                FabricCI.CiChilds = $tblChildEl.getCurrentViewRecords();
                selectedRecords.forEach(function (value) {
                    
                    itemIds = "";
                    for (var i = 0; i < selectedRecords.length; i++) itemIds += selectedRecords[i].YPIReceiveMasterID + ","
                    itemIds = itemIds.substr(0, itemIds.length - 1);
                    // getSelectedBatchDetails(itemIds);
                    var exists = FabricCI.CiChildPis.find(function (el) { return el.YPINo == value.YPINo });
                    /*   alert(exists);*/
                    //if (!exists) $tblChildEl.getCurrentViewRecords().unshift(value);
                    if (!exists) FabricCI.CiChildPis.unshift({
                        YPIMasterID: value.YPIReceiveMasterID,
                        /* Id: getMaxIdForArray($tblYarnPIItemsEl.getCurrentViewRecords(), "YPIReceiveMasterID"),*/
                        PiNo: value.YPINo,
                       
                        PIDate: value.PIDate,
                        SupplierName: value.SupplierName,
                        TotalQty: value.PIQty,
                        TotalValue: value.PIValue
                    });
                });
                initAttachedPIChildTable(FabricCI.CiChildPis);
                getSelectedItemDetails(itemIds);
            }
        });

        finder.showModal();
    }

    function getSelectedItemDetails(itemIds) {
        //alert(itemIds);
        axios.get(`/api/fabricCI/get-item-details/${itemIds}`)
            .then(function (response) {
                for (var i = 0; i < response.data.length; i++) {
                    for (var j = 0; j < response.data[i].CIChilds.length; j++) {
                        $tblChildEl.getCurrentViewRecords().push(response.data[i].CIChilds[j]);
                    }
                }
                initChildTable($tblChildEl.getCurrentViewRecords());
                $tblChildEl.refresh();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    var validationConstraints = {
        CINo: {
            presence: true
        }
    }

    function isValidMasterForm(mData) {
        var isValid = false;
        if (parseFloat(mData.CIValue) > parseFloat(mData.LCValue)) {
            toastr.error("Invoice Value must be less than or equal to L/C Value With Tolerance.");
            isValid = true;
        }
        return isValid;
    }

    function save() {
        //var files = $formEl.find("#UploadFile")[0].files;
        //if (!files || files.length == 0) return toastr.error("You must upload CI document.");
        const records = $tblChildEl.getCurrentViewRecords();
        if (records != null) {
            const sum = records.reduce((total, record) => total + record.PdValue, 0);
            var InvoiceValue = $formEl.find("#CIValue").val();
            if (InvoiceValue != sum) {
                return toastr.error("Invoice Value And Total Item Invoice Value Must Be same");
            }
        }
        


        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors.");
        else hideValidationErrors($formEl);
        var formData = getFormData($formEl);
        //formData.append("UploadFile", files[0]);
        
        formData.append("CiChilds", JSON.stringify($tblChildEl.getCurrentViewRecords()));
        formData.append("CiChildPis", JSON.stringify($tblFabricPIItemsEl.getCurrentViewRecords()));
        //formData.append("IsCDA", isFabricCommercialPage);

        uploadDocs.map(x => {
            var file = x.File;
            var blob = file.slice(0, file.size, file.type);
            x.FileName = removeInvalidChar(x.FileName) + "~" + x.DocTypeID;
            newFile = new File([blob], x.FileName, { type: file.type });
            formData.append("UploadFileDoc", newFile);
        });

        if ($formEl.find("#OD").is(':checked') == true)
            formData.append("SG", false);
        else if ($formEl.find("#SG").is(':checked') == true)
            formData.append("SG", true);

        //Child Validation check 
        if (isValidMasterForm(FabricCI)) return;

        const config = {
            headers: {
                'content-type': 'multipart/form-data',
                'Authorization': "Bearer " + localStorage.getItem("token")
            }
        }

        axios.post("/api/fabricCI/save", formData, config)
            .then(function (response) {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(showResponseError);
    }

    //function save() {
    //    var data = formDataToJson($formEl.serializeArray());
    //    masterData = data; 
    //    data.IsCDA = isCDAPage;
    //    data["CiChilds"] = $tblChildEl.getCurrentViewRecords();
    //    data["CiChildPis"] = $tblYarnPIItemsEl.getCurrentViewRecords();

    //    if ($formEl.find("#OD").is(':checked') == true)
    //        data.SG = false;
    //    else if ($formEl.find("#SG").is(':checked') == true)
    //        data.SG = true;

    //    //Validation
    //    initializeValidation($formEl, validationConstraints);
    //    if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
    //    else hideValidationErrors($formEl);

    //    isValidMasterForm(masterData); 

    //    axios.post("/ciAPI/save", data)
    //        .then(function () {
    //            toastr.success("Saved successfully!");
    //            backToList();
    //        })
    //        .catch(function (error) {
    //            toastr.error(error.response.data.Message);
    //        });
    //}

    function approve(bankAccept) {
        var data = formDataToJson($formEl.serializeArray());
        data.BankAccept = bankAccept;

        axios.post("/api/fabricCI/approve", data)
            .then(function (response) {
                toastr.success("Successfully Approved!");
                backToList();
            })
            .catch(showResponseError);

        /*
        var data = formDataToJson($formEl.serializeArray());
        axios.post(`/ciAPI/approve/${data.CIID}`)
            .then(function () {
                toastr.success("Successfully Approved!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
        */
    }

    function reject(bankReject) {
        var data = formDataToJson($formEl.serializeArray());
        data.BankAccept = bankReject;
        axios.post("/api/fabricCI/reject", data)
            .then(function (response) {
                toastr.success("Successfully Rejected!");
                backToList();
            })
            .catch(showResponseError);

        /*
        axios.post(`/ciAPI/reject/${data.CIID}`)
            .then(function () {
                toastr.success("Successfully Rejected!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
        */
    }

    async function reportCommandClick(e) {
        if (e.commandColumn.buttonOption.type == 'Preview') {
            window.open(`/reports/InlinePdfView?ReportName=${e.rowData.additionalValue}&InvoiceNo=${invoiceNo}`, '_blank');
        }
    }


    function showReportList() {
        if (reportData != null) {
            $(tblReportId).html("");
            var grid = new ej.grids.Grid({
                dataSource: reportData,
                columns: [
                    { field: 'id', headerText: 'ID', visible: false },
                    { field: 'additionalValue', visible: false },
                    { field: 'text', headerText: '',  },
                    {
                        headerText: '', textAlign: 'Center', width: 80, commands: [
                            {
                                buttonOption: {
                                    type: 'Preview', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-eye'
                                }

                            }
                        ]
                    }
                ],
                commandClick: reportCommandClick,
                autofitColumns: false,
                showDefaultToolbar: false,
                allowFiltering: false,
                allowPaging: false,
                editSettings: {
                    allowAdding: false,
                    allowEditing: false,
                    allowDeleting: false,
                    mode: "Normal",
                    showDeleteConfirmDialog: true
                },
            });

            grid.appendTo(tblReportId);
        }
    }
})();
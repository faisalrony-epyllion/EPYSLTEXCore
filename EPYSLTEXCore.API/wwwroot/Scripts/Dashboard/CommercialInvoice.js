
(function () {
    var menuId, pageName, pageId;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl,
        tblMasterId, $tblChildEl, tblChildId, $formEl, $tblYarnPIItemsEl,
        tblYarnPIItemsId, tblDocsId, tblAcceptanceChargeDetailsId, $tblAcceptanceChargeDetailsEL,
        tblAcceptanceCIInfoId, $tblAcceptanceCIInfoEL;
    var filterBy = {};
    var isAcceptancePage = false, isCDAPage = false;
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
    var YarnCI;
    var uploadDocs = [];
    var _isHideRemoveBtn = false;

    var _OwnAcceptance = false;
    var _BankAcceptance = false;
    var maxColDetails = 999;
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
        tblYarnPIItemsId = "#tblYarnPIItems" + pageId;
        tblDocsId = "#tblDocs" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        tblAcceptanceCIInfoId = "#tblAcceptanceCIInfoId" + pageId;
        tblAcceptanceChargeDetailsId = "#tblAcceptanceChargeDetailsId" + pageId;

        isAcceptancePage = convertToBoolean($(`#${pageId}`).find("#AcceptancePage").val());
        isCDAPage = convertToBoolean($(`#${pageId}`).find("#CDAPage").val());

        $formEl.find("#btnAddItem").on("click", addItem);
        $formEl.find("#divAddKPForGrouping").hide();
        $toolbarEl.find("#btnCreateBankAcceptance").hide();
        if (!isAcceptancePage) {
            status = statusConstants.PENDING;

            $toolbarEl.find("#btnPending").show();
            $toolbarEl.find("#btnPartialCompleteList").show();
            $toolbarEl.find("#btnList,#btnRejectList").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPending"), $toolbarEl);
        }
        else {
            status = statusConstants.PARTIALLY_COMPLETED;

            $toolbarEl.find("#btnList,#btnRejectList").show();
            $toolbarEl.find("#btnPartialCompleteList").show();
            $toolbarEl.find("#btnPending").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPartialCompleteList"), $toolbarEl);
        }

        initMasterTable();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;

            initMasterTable();
            $formEl.find("#divAddKPForGrouping").hide();
            $toolbarEl.find("#btnCreateBankAcceptance").hide();
        });

        $toolbarEl.find("#btnPartialCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PARTIALLY_COMPLETED;
            initMasterTable();
            $formEl.find("#divAddKPForGrouping").hide();
            $toolbarEl.find("#btnCreateBankAcceptance").hide();
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            initMasterTable();
            $formEl.find("#divAddKPForGrouping").hide();
            $toolbarEl.find("#btnCreateBankAcceptance").hide();
        });

        $toolbarEl.find("#btnRejectList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.REJECT;
            initMasterTable();
            $formEl.find("#divAddKPForGrouping").hide();
            $toolbarEl.find("#btnCreateBankAcceptance").hide();
        });
        $toolbarEl.find("#btnAwaitingForBankAcceptance").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.AWAITING_PROPOSE;
            initMasterTable();
            $formEl.find("#divAddKPForGrouping").show();
            $toolbarEl.find("#btnCreateBankAcceptance").show();
        });
        $toolbarEl.find("#btnAcceptedInvoice").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.APPROVED_DONE;
            /*  initMasterTable();*/
            initAcceptanceTable();
            $formEl.find("#divAddKPForGrouping").hide();
            $toolbarEl.find("#btnCreateBankAcceptance").hide();
        });


        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnAccept").click(function (e) {
            e.preventDefault();
            approve();
        });

        $formEl.find("#btnReject").click(function (e) {
            e.preventDefault();
            reject();
        });

        $formEl.find("#btnCancel").on("click", backToList);

        $(document).on('click', 'input[type="checkbox"]', function () {
            $('input[type="checkbox"]').not(this).prop('checked', false);
        });

        $toolbarEl.find("#btnCreateBankAcceptance").click(function () {
            getNewDataForBankAcceptance();
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
                { type: 'ViewAttachedFile', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file' } }
            ]
        }
        else {
            commands = [
                { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                { type: 'ViewAttachedFile', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file' } }
            ]
        }

        var columns = [
            {
                headerText: 'Actions', commands: commands, width: 100
            },
            { field: 'IssueBankId', headerText: 'IssueBankId', visible: false },
            { field: 'CompanyId', headerText: 'CompanyId', visible: false },
            { field: 'SupplierId', headerText: 'SupplierId', visible: false },
            { field: 'CIID', headerText: 'CIID', visible: false },
            { field: 'CINo', headerText: 'Invoice No', visible: status != statusConstants.PENDING },
            {
                field: 'SupplierName', headerText: 'Supplier', visible: status != statusConstants.PARTIALLY_COMPLETED
            },
            {
                field: 'CustomerName', headerText: 'Customer', visible: status != statusConstants.PARTIALLY_COMPLETED
            },
            {
                field: 'IssueBank', headerText: 'Issue Bank', visible: status != statusConstants.PARTIALLY_COMPLETED
            },
            {
                field: 'CIDate', headerText: 'Invoice Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'CIValue', headerText: 'Invoice Value', textAlign: 'Right', visible: status != statusConstants.PENDING
            },
            {
                field: 'LcNo', headerText: 'L/C No'
            },
            {
                field: 'LcDate', headerText: 'L/C Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'LcQty', headerText: 'Total Qty', textAlign: 'Right'
            },
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
            {
                field: 'TotalCI', headerText: 'Total CI', textAlign: 'Right'
            },
            {
                field: 'LcExpiryDate', headerText: 'L/C Expiry Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                headerText: 'LC', textAlign: 'Center', commands: [
                    { buttonOption: { type: 'Image', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-image' } }
                ]
            }
        ];
        var selectionType = "Single";
        if (status == statusConstants.AWAITING_PROPOSE) {
            columns.unshift({ type: 'checkbox', width: 30 });
            selectionType = "Multiple";
        }
        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/ciAPI/list?status=${status}&isCDAPage=${isCDAPage}`,
            columns: columns,
            commandClick: handleCommands,
            allowSelection: status == statusConstants.AWAITING_PROPOSE,
            selectionSettings: { type: selectionType, checkboxOnly: true, persistSelection: true }
        });
    }

    function handleCommands(args) {
        /* */
        BankAccShowHide("N");
        if (args.commandColumn.type == 'New') {
            getNew(args.rowData.LCID);
            internalBtnHideShow(args.commandColumn.type);
        } else if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.CIID);
            internalBtnHideShow(args.commandColumn.type);
        } else if (args.commandColumn.type == 'View') {
            getDetails(args.rowData.CIID);
            internalBtnHideShow(args.commandColumn.type);
        } else if (args.commandColumn.type == 'ViewAttachedFile') {
            if (args.rowData.CIFilePath == null || args.rowData.CIFilePath.length == 0) {
                toastr.error("No Attchment Found.");
                return false;
            }
            if (args.rowData.CIFilePath != null && args.rowData.CIFilePath.length > 0) {
                window.open(args.rowData.CIFilePath, '_blank')
            }
        } else if (args.commandColumn.buttonOption.type == 'Image') {
            var a = document.createElement('a');
            a.href = args.rowData.LcFilePath;
            a.setAttribute('target', '_blank');
            a.click();
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
                { field: 'ItemDescription', headerText: 'Item Description', textAlign: 'Left' },
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
                { field: 'PdValue', headerText: 'Invoice Value', textAlign: 'Right', allowEditing: false },
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
                    CTDValue = args.data.PdValue;
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
                    $formEl.find("#CIValue").val(TPDValue - CTDValue);
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
        if ($tblYarnPIItemsEl) {
            $tblYarnPIItemsEl.destroy();
            $(tblYarnPIItemsId).html("");
        }

        ej.base.enableRipple(true);
        $tblYarnPIItemsEl = new ej.grids.Grid({
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
                { field: 'TotalQty', headerText: 'Total Qty', width: 15, textAlign: 'Right' },
                { field: 'TotalValue', headerText: 'Total Value', width: 15, textAlign: 'Right' }
            ]
        });
        $tblYarnPIItemsEl["dataBound"] = function () {
            $tblYarnPIItemsEl.autoFitColumns();
        };
        $tblYarnPIItemsEl.refreshColumns;
        $tblYarnPIItemsEl.appendTo(tblYarnPIItemsId);
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

        axios.get(`/ciAPI/new/${Lcid}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                _isHideRemoveBtn = false;
                resetDocTypeCbo();
                resetDocTable();
                uploadDocs = [];

                //if (isAcceptancePage) {
                //    $formEl.find(".cia").show();
                //    $formEl.find(".owncia").show();
                //} else {
                //    $formEl.find(".cia").hide();
                //    $formEl.find(".owncia").hide();
                //}
                YarnCI = response.data;
                YarnCI.LcDate = formatDateToDefault(YarnCI.LcDate);
                YarnCI.LcReceiveDate = formatDateToDefault(YarnCI.LcReceiveDate);
                YarnCI.LcExpiryDate = formatDateToDefault(YarnCI.LcExpiryDate);
                YarnCI.CIDate = formatDateToDefault(YarnCI.CIDate);
                YarnCI.ExpDate = formatDateToDefault(YarnCI.ExpDate);
                YarnCI.BOEDate = formatDateToDefault(YarnCI.BOEDate);
                loadDocTypes();

                setFormData($formEl, YarnCI);
                initChildTable(YarnCI.CIChilds);
                initAttachedPIChildTable(YarnCI.CIChildPIs);

                if (status == statusConstants.PENDING) {
                    $formEl.find("#btnSave").fadeIn();
                    $formEl.find("#btnAccept,#btnReject").fadeOut();
                } else if (status == statusConstants.PARTIALLY_COMPLETED) {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnAccept,#btnReject").fadeOut();
                    if (isAcceptancePage)
                        $formEl.find("#btnAccept,#btnReject").fadeIn();
                } else {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnAccept,#btnReject").fadeOut();
                }

                if (YarnCI.SG) {
                    $formEl.find("#SG").prop('checked', true);
                    $formEl.find("#OD").prop('checked', false);
                }
                else {
                    $formEl.find("#SG").prop('checked', false);
                    $formEl.find("#OD").prop('checked', true);
                }
                initAttachment(YarnCI.CIFilePath, YarnCI.AttachmentPreviewTemplate, $formEl.find("#UploadFile"));
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/ciAPI/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                _isHideRemoveBtn = true;
                resetDocTypeCbo();
                resetDocTable();
                uploadDocs = [];

                //if (isAcceptancePage) {
                //    $formEl.find(".cia").show();
                //    $formEl.find(".owncia").show();
                //} else {
                //    $formEl.find(".cia").hide();
                //    $formEl.find(".owncia").hide();
                //}

                YarnCI = response.data;
                YarnCI.LcDate = formatDateToDefault(YarnCI.LcDate);
                YarnCI.LcReceiveDate = formatDateToDefault(YarnCI.LcReceiveDate);
                YarnCI.LcExpiryDate = formatDateToDefault(YarnCI.LcExpiryDate);
                YarnCI.CIDate = formatDateToDefault(YarnCI.CIDate);
                YarnCI.ExpDate = formatDateToDefault(YarnCI.ExpDate);
                YarnCI.BOEDate = formatDateToDefault(YarnCI.BOEDate);
                YarnCI.BLDate = formatDateToDefault(YarnCI.BLDate);
                YarnCI.AcceptanceDate = formatDateToDefault(YarnCI.AcceptanceDate);
                YarnCI.BankAcceptDate = formatDateToDefault(YarnCI.BankAcceptDate);
                YarnCI.MaturityDate = formatDateToDefault(YarnCI.MaturityDate);
                loadDocTypes();
                loadDocTables();

                setFormData($formEl, YarnCI);
                initChildTable(YarnCI.CIChilds);
                initAttachedPIChildTable(YarnCI.CIChildPIs);

                if (status == statusConstants.PENDING) {
                    $formEl.find("#btnSave").fadeIn();
                    $formEl.find("#btnAccept,#btnReject").fadeOut();
                } else if (status == statusConstants.PARTIALLY_COMPLETED) {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnAccept,#btnReject").fadeOut();
                    if (isAcceptancePage)
                        $formEl.find("#btnAccept,#btnReject").fadeIn();
                } else {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnAccept,#btnReject").fadeOut();
                }
                if (YarnCI.SG) {
                    $formEl.find("#SG").prop('checked', true);
                    $formEl.find("#OD").prop('checked', false);
                }
                else {
                    $formEl.find("#SG").prop('checked', false);
                    $formEl.find("#OD").prop('checked', true);
                }
                initAttachment(YarnCI.CIFilePath, YarnCI.AttachmentPreviewTemplate, $formEl.find("#UploadFile"));
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function loadDocTables() {
        YarnCI.YarnCIDocs.map(x => {
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
        YarnCI.AllDocTypes.map(x => {
            $("#" + pageId).find("#cboDocType").append(`<option value=${x.DocTypeID}>${x.DocTypeName}</option>`);
        });
    }
    function operationDocumentUploads() {
        var thisPage = $("#" + pageId);

        thisPage.find("#btnAddDoc").click(function () {
            thisPage.find('#divModalDoc').modal('show');
        });

        thisPage.find("#modalAddDoc").click(function () {
            thisPage.find('#divModalDoc').modal('hide');

            var yarnCIDoc = {
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
                        DocTypeID: yarnCIDoc.DocTypeID
                    });
                }
            }
            yarnCIDoc.FileName = removeInvalidChar(fileName);
            generateSingleRow(yarnCIDoc);
        });
    }
    function generateSingleRow(yarnCIDoc) {
        yarnCIDoc.YarnCIDocID = typeof yarnCIDoc.YarnCIDocID === "undefined" ? 0 : yarnCIDoc.YarnCIDocID;
        yarnCIDoc.CIID = typeof yarnCIDoc.CIID === "undefined" ? 0 : yarnCIDoc.CIID;
        yarnCIDoc.ImagePath = typeof yarnCIDoc.ImagePath === "undefined" || yarnCIDoc.ImagePath == "" ? "#" : yarnCIDoc.ImagePath;

        var splitValue = yarnCIDoc.FileName.split(".");
        if (yarnCIDoc.ImagePath != "#" && splitValue.length == 1) {
            var extension = getFileExtension(yarnCIDoc.ImagePath);
            yarnCIDoc.FileName = yarnCIDoc.FileName + extension;
        }

        var docTbl = $(tblDocsId);
        var slNo = docTbl.find("tbody tr").length + 1;

        if (typeof yarnCIDoc.ImagePath === "undefined" || yarnCIDoc.ImagePath == "") {
            anchorText = `${yarnCIDoc.FileName}`;
        } else {
            anchorText = `<a href='${yarnCIDoc.ImagePath}' target="_blank" title='Show Doc'>${yarnCIDoc.FileName}</a>`;
        }

        var css = _isHideRemoveBtn ? "style='display:none'" : "";
        if (_isHideRemoveBtn) {
            docTbl.find("thead").find(".cellActionDoc").hide();
        }

        docTbl.find("tbody").append(`
                <tr rowIndex=${slNo}>
                    <td class='tdStatic tdRemoveIcon cellActionDoc' YarnCIDocID=${yarnCIDoc.YarnCIDocID} CIID=${yarnCIDoc.CIID} ${css}>
                        <button id='btnRemoveImg${slNo}' type='button' class="btn btn-danger btn-xs add btnRemoveImg" title="Remove"><i class="fa fa-trash"></i></button>
                    </td>
                    <td>${slNo}</td>
                    <td>${yarnCIDoc.DocTypeName}</td>
                    <td>
                        ${anchorText}
                    </td>
                </tr>
            `);

        docTbl.find("tbody tr[rowIndex=" + slNo + "]").find("#btnRemoveImg" + slNo).click(function () {
            if (confirm("Confirm delete ?")) {
                var yarnCIDocID = docTbl.find("tbody tr[rowIndex=" + slNo + "] .tdRemoveIcon").attr("YarnCIDocID");
                if (yarnCIDocID > 0) {
                    var indexF = YarnCI.YarnCIDocs.findIndex(x => x.YarnCIDocID == yarnCIDocID);
                    if (indexF > -1) {
                        YarnCI.YarnCIDocs[indexF].IsDelete = true;
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
        var LcNo = $(`#${pageId}`).find("#LcNo").val();
        e.preventDefault();
        var itemIds = "";
        var finder = new commonFinder({
            title: "Select Proposal",
            pageId: pageId,
            height: 350,
            //apiEndPoint: "/api/ybblcproposal/proposallist?gridType=ej2&status=COMPLETED&isCDAPage=false",
            // url = "/api/ypi-receive/list?gridType=bootstrap-table&status=" + status + "&" + queryParams;
            apiEndPoint: `/api/ypi-receive/proposallist?gridType=ej2&status=COMPLETED&lcNumber=${LcNo}`,
            fields: "YPINo,PIDate,CompanyName,SupplierName,YPINo,PIQty,PIValue",
            headerTexts: "PI No,PI Date,Company,Supplier,PO No,Total Qty,Total Value",
            customFormats: ",dd/MM/yyyy,,,,,ej2GridColorFormatter",
            isMultiselect: true,
            primaryKeyColumn: "YPIReceiveMasterID",
            onMultiselect: function (selectedRecords) {
                YarnCI.CiChildPis = $tblYarnPIItemsEl.getCurrentViewRecords();
                YarnCI.CiChilds = $tblChildEl.getCurrentViewRecords();
                selectedRecords.forEach(function (value) {
                    itemIds = "";
                    for (var i = 0; i < selectedRecords.length; i++) itemIds += selectedRecords[i].YPIReceiveMasterID + ","
                    itemIds = itemIds.substr(0, itemIds.length - 1);
                    // getSelectedBatchDetails(itemIds);
                    var exists = YarnCI.CiChildPis.find(function (el) { return el.YPINo == value.YPINo });
                    /*   alert(exists);*/
                    //if (!exists) $tblChildEl.getCurrentViewRecords().unshift(value);
                    if (!exists) YarnCI.CiChildPis.unshift({
                        YPIMasterID: value.YPIReceiveMasterID,
                        /* Id: getMaxIdForArray($tblYarnPIItemsEl.getCurrentViewRecords(), "YPIReceiveMasterID"),*/
                        PiNo: value.YPINo,
                        PIDate: value.PIDate,
                        SupplierName: value.SupplierName,
                        TotalQty: value.PIQty,
                        TotalValue: value.PIValue
                    });
                });
                initAttachedPIChildTable(YarnCI.CiChildPis);
                getSelectedItemDetails(itemIds);
            }
        });

        finder.showModal();
    }

    function getSelectedItemDetails(itemIds) {
        //alert(itemIds);
        axios.get(`/ciAPI/get-item-details/${itemIds}`)
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
        var files = $formEl.find("#UploadFile")[0].files;
        if (!files || files.length == 0) return toastr.error("You must upload CI document.");

        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors.");
        else hideValidationErrors($formEl);

        var formData = getFormData($formEl);
        formData.append("UploadFile", files[0]);
        formData.append("CiChilds", JSON.stringify(YarnCI.CIChilds));
        formData.append("CiChildPis", JSON.stringify(YarnCI.CIChildPIs));
        formData.append("IsCDA", isCDAPage);

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
        if (isValidMasterForm(YarnCI)) return;

        const config = {
            headers: {
                'content-type': 'multipart/form-data',
                'Authorization': "Bearer " + localStorage.getItem("token")
            }
        }

        axios.post("/ciAPI/save", formData, config)
            .then(function (response) {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(showResponseError);
    }

    function internalBtnHideShow(buttonType) {
        if (status == statusConstants.PARTIALLY_COMPLETED && isAcceptancePage == true && buttonType == "Edit") {
            $formEl.find(".owncia").show();
            $formEl.find(".cia").hide();
            _OwnAcceptance = true;
            _BankAcceptance = false;

        }
        else if (status == statusConstants.AWAITING_PROPOSE && isAcceptancePage == true && (buttonType == "Edit" || buttonType == "View")) {
            $formEl.find(".cia").show();
            $formEl.find(".owncia").show();
            _OwnAcceptance = false;
            _BankAcceptance = true;
        }
        else {
            $formEl.find(".cia").hide();
            $formEl.find(".owncia").hide();
            _OwnAcceptance = false;
            _BankAcceptance = false;
        }
        //$formEl.find("#btnSave").fadeOut();
        //if (buttonType == "Edit" || buttonType == "New") {
        //    $formEl.find("#btnSave").fadeIn();
        //}
    }
    function getNewDataForBankAcceptance() {
        var selectedRecords = $tblMasterEl.getSelectedRecords();
        if (selectedRecords.length == 0) {
            toastr.error("Please select row(s)!");
            return;
        }
        var companyList = [],
            bankList = [],
            supplierList = [];
        selectedRecords.map(x => {
            companyList.push(x.CompanyId);
            bankList.push(x.IssueBankId);
            supplierList.push(x.SupplierId);
        });
        var unique_array = [...new Set(companyList)];
        if (unique_array.length > 1) {
            toastr.error("Customer name should same.");
            return;
        }
        var unique_array = [...new Set(bankList)];
        if (unique_array.length > 1) {
            toastr.error("Bank name should same.");
            return;
        }

        var unique_array = [...new Set(supplierList)];
        if (unique_array.length > 1) {
            toastr.error("Supplier name should same.");
            return;
        }

        var nCIIDs = selectedRecords.map(x => x.CIID).join(",");
        var companyIDs = selectedRecords.map(x => x.CompanyId).join(",");
        var supplierIDs = selectedRecords.map(x => x.SupplierId).join(",");
        var bankBranchIDs = selectedRecords.map(x => x.IssueBankId).join(",");

        getNewForGroupBankAcceptance(nCIIDs, companyIDs, supplierIDs, bankBranchIDs);

    }
    function getNewForGroupBankAcceptance(nCIIDs, companyIDs, supplierIDs, bankBranchIDs) {
        axios.get(`/ciAPI/createBankAcceptance/${nCIIDs}/${companyIDs}/${supplierIDs}/${bankBranchIDs}`)
            .then(function (response) {
                resetForm();
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                BankAccShowHide("Y");
                initAcceptCIChild(masterData);
                initChargeDetailsTable(masterData[0].IDACDetails);
                _BankAcceptance = true;
                _OwnAcceptance = false;

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function BankAccShowHide(type) {
        if (type == "Y") {
            $formEl.find(".ciAForBankAccept").hide();
            $formEl.find(".acceptInfo").show();
            $formEl.find(".AcceptanceChargeShowHide").show();
            $formEl.find("#btnAccept").fadeIn();
        }
        else {
            $formEl.find(".ciAForBankAccept").show();
            $formEl.find(".AcceptanceChargeShowHide").hide();
            $formEl.find(".acceptInfo").hide();
        }

    }

    function approve() {
        var data = formDataToJson($formEl.serializeArray());
        data.Acceptance = _OwnAcceptance;
        data.BankAccept = _BankAcceptance;
        var ciChildInfo = [];
        if (_BankAcceptance == true) {
            var sBankRef = "", sBankAcceptDate = "";
            sBankRef = $('#BankRefNumber').val();
            sBankAcceptDate = $('#BankAcceptDate').val();
            if (sBankRef == "" || sBankRef == undefined || sBankRef == null || sBankRef == "0") {
                return toastr.error("Please Enter Valid Bank Ref No!");
            }
            else if (sBankAcceptDate == "" || sBankAcceptDate == undefined || sBankAcceptDate == null) {
                return toastr.error("Please Select Valid Bank Ref Date!");
            }

            $tblAcceptanceCIInfoEL.getCurrentViewRecords().map(x => {
                ciChildInfo.push(x);
            });
            data.YarnCIList = ciChildInfo;


            //// Details Information 
            var yipListMain = [];
            var yipList = [];
            $tblAcceptanceChargeDetailsEL.getCurrentViewRecords().map(x => {
                yipList.push(x);
            });
            if (yipList.length > 0) {
                for (var i = 0; i < yipList.length; i++) {
                    yipListMain.push(yipList[i]);
                }
            }

            data.IDACDetails = yipListMain;
            ///// End Details Information
            var SGHeadID = 0, DHeadID = 0, CalculationOn = 0, ValueInFC = 0,
                ValueInLC = 0, CurConvRate = 0, SHeadID = 0;
            var sFlagToastar = "N";
            for (var i = 0; i < data.IDACDetails.length; i++) {
                if (data.IDACDetails[i].IDACDetailSub.length == 0) {
                    return toastr.error("Please Add One Charge!");
                    sFlagToastar = "Y";
                    break;
                }
                for (var j = 0; j < data.IDACDetails[i].IDACDetailSub.length; j++) {

                    SGHeadID = data.IDACDetails[i].IDACDetailSub[j].SGHeadID;
                    DHeadID = data.IDACDetails[i].IDACDetailSub[j].DHeadID;
                    CalculationOn = data.IDACDetails[i].IDACDetailSub[j].CalculationOn;
                    ValueInFC = data.IDACDetails[i].IDACDetailSub[j].ValueInFC;
                    ValueInLC = data.IDACDetails[i].IDACDetailSub[j].ValueInLC;
                    CurConvRate = data.IDACDetails[i].IDACDetailSub[j].CurConvRate;
                    SHeadID = data.IDACDetails[i].IDACDetailSub[j].SHeadID;

                    if (parseInt(SGHeadID) == 0 || SGHeadID == undefined || SGHeadID == null) {
                        return toastr.error("At Least One Source Group Head!");
                        sFlagToastar = "Y";
                        break;
                    }
                    else if (parseInt(DHeadID) == 0 || DHeadID == undefined || DHeadID == null) {
                        return toastr.error("Please Select Valid Description!");
                        sFlagToastar = "Y";
                        break;
                    }
                    else if (parseInt(CalculationOn) == 0 || CalculationOn == undefined || CalculationOn == null) {
                        return toastr.error("Please Select Valid Calculate On!");
                        sFlagToastar = "Y";
                        break;
                    }

                    else if (parseInt(ValueInFC) == 0 || ValueInFC == undefined || ValueInFC == null) {
                        return toastr.error("Please Enter Valid Amount In FC!");
                        sFlagToastar = "Y";
                        break;
                    }
                    else if (parseInt(ValueInLC) == 0 || ValueInLC == undefined || ValueInLC == null) {
                        return toastr.error("Please Enter Valid Amount In LC!");
                        sFlagToastar = "Y";
                        break;
                    }
                    else if (parseInt(CurConvRate) == 0 || CurConvRate == undefined || CurConvRate == null) {
                        return toastr.error("Please Enter Valid Rate!");
                        sFlagToastar = "Y";
                        break;
                    }

                }
                if (sFlagToastar == "Y")
                    break;


            }



        }
        axios.post("/ciAPI/approve", data)
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

    function reject() {
        var data = formDataToJson($formEl.serializeArray());

        axios.post("/ciAPI/reject", data)
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

    function initAcceptCIChild(data) {
        if ($tblAcceptanceCIInfoEL) $tblAcceptanceCIInfoEL.destroy();
        ej.base.enableRipple(true);
        $tblAcceptanceCIInfoEL = new ej.grids.Grid({
            dataSource: data,
            editSettings: { allowEditing: false, allowAdding: false, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: false },
            allowResizing: false,
            actionBegin: function (args) { },
            columns: [
                { field: 'CIID', isPrimaryKey: true, visible: false, allowEditing: false },
                { field: 'LCNo', headerText: 'L/C No', allowEditing: false, width: 100 },
                { field: 'LCDate', headerText: 'L/C Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1 },
                { field: 'CINo', headerText: 'Invoice No', allowEditing: false, width: 100 },
                { field: 'CIDate', headerText: 'Invoice Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1 },
                { field: 'CIValue', headerText: 'Invoice Value', allowEditing: false, width: 100 },
                { field: 'LCValue', headerText: 'L/C Value', allowEditing: false, width: 100 },
                { field: 'AcceptanceDate', headerText: 'Acceptance Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1 }

            ]
        });
        $tblAcceptanceCIInfoEL["dataBound"] = function () {
            $tblAcceptanceCIInfoEL.autoFitColumns();
        };
        $tblAcceptanceCIInfoEL.refreshColumns;
        $tblAcceptanceCIInfoEL.appendTo(tblAcceptanceCIInfoId);
    }
    var DHeadNameElem;
    var DHeadNameObj;
    function initChargeDetailsTable(data) {
        var CalculatOnList = [
            { id: 1, text: "FC" },
            { id: 2, text: "LC" },
            { id: 3, text: "Rate" }
        ];
        isEditable = true;
        if ($tblAcceptanceChargeDetailsEL) $tblAcceptanceChargeDetailsEL.destroy();
        ej.base.enableRipple(true);
        $tblAcceptanceChargeDetailsEL = new ej.grids.Grid({
            dataSource: data,
            editSettings: { allowEditing: false, allowAdding: false, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: false },
            allowResizing: false,
            actionBegin: function (args) {
            },
            columns: [
                { field: 'SGHeadID', isPrimaryKey: true, visible: false, allowEditing: false },
                { field: 'CTCategoryID', visible: false, allowEditing: false },
                { field: 'DHeadNeed', visible: false, allowEditing: false },
                { field: 'SHeadNeed', width: 20, visible: false, allowEditing: false },
                { field: 'SGHeadName', headerText: 'Head Group', allowEditing: false, width: 100 }

            ],
            childGrid: {
                queryString: 'SGHeadID',
                allowResizing: true,
                autofitColumns: false,
                toolbar: ['Add'],
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: [
                    {
                        headerText: 'Commands', textAlign: 'Center', width: 120, commands: [
                            { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                            { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                            { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                            { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
                    },
                    { field: 'SGHeadID', isPrimaryKey: true, visible: false, allowEditing: false },
                    { field: 'ADetailsID', visible: false, allowEditing: false },
                    //{
                    //    field: 'DHeadID', headerText: 'Head Description', allowEditing: isEditable, required: true, width: 350, valueAccessor: ej2GridDisplayFormatter, dataSource: masterData[0].HeadDescriptionList,
                    //    displayField: "DHeadName", edit: ej2GridDropDownObj({
                    //        width: 350
                    //    })
                    //},
                    {
                        field: 'DHeadName', headerText: 'Head Description', visible: true,
                        valueAccessor: ej2GridDisplayFormatterV2, edit: {
                            create: function () {
                                DHeadNameElem = document.createElement('input');
                                return DHeadNameElem;
                            },
                            read: function () {
                                return DHeadNameObj.text;
                            },
                            destroy: function () {
                                DHeadNameObj.destroy();
                            },
                            write: function (e) {
                                DHeadNameObj = new ej.dropdowns.DropDownList({
                                    dataSource: masterData[0].HeadDescriptionList,//masterData[0].HeadDescriptionList.filter(x => x.SGHeadID == this.parentDetails.parentRowData.SGHeadID),
                                    fields: { value: 'id', text: 'text' },
                                    change: function (f) {
                                        //technicalNameObj.enabled = true;
                                        //var tempQuery = new ej.data.Query().where('additionalValue', 'equal', machineTypeObj.value);
                                        //technicalNameObj.query = tempQuery;
                                        //technicalNameObj.text = null;
                                        //technicalNameObj.dataBind();

                                        //e.rowData.MachineTypeId = f.itemData.id;
                                        //e.rowData.MachineType = f.itemData.text;
                                        //e.rowData.KTypeId = f.itemData.desc;
                                        //e.rowData = setTotalDaysAndDeliveryDate(e.rowData, e.rowData.CriteriaNames);
                                    },
                                    placeholder: 'Select Head Description',
                                    floatLabelType: 'Never'
                                });
                                DHeadNameObj.appendTo(DHeadNameElem);
                            }
                        }
                    },
                    {
                        field: 'CalculationOn', headerText: 'Calculate On', allowEditing: isEditable, required: true, width: 350, valueAccessor: ej2GridDisplayFormatter, dataSource: CalculatOnList,
                        displayField: "CalculationOnName", edit: ej2GridDropDownObj({
                            width: 350
                        })
                    },
                    {
                        field: 'ValueInFC', headerText: 'Amount In FC', editType: "numericedit",
                        edit: { params: { showSpinButton: false, decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } }, width: 100
                    },
                    {
                        field: 'ValueInLC', headerText: 'Amount In LC', editType: "numericedit",
                        edit: { params: { showSpinButton: false, decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } }, width: 100
                    },
                    {
                        field: 'CurConvRate', headerText: 'Conversion Rate', editType: "numericedit",
                        edit: { params: { showSpinButton: false, decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } }, width: 100
                    }, {
                        field: 'SHeadID', headerText: 'Source Head', allowEditing: isEditable, required: true, width: 350, valueAccessor: ej2GridDisplayFormatter, dataSource: masterData[0].SHeadNameList,
                        displayField: "SHeadName", edit: ej2GridDropDownObj({
                            width: 350
                        })
                    }
                ],
                //commandClick: commandClick,
                actionBegin: function (args) {
                    if (args.requestType === 'beginEdit') {

                    }
                    else if (args.requestType === "add") {
                        debugger;
                        args.data.SGHeadID = this.parentDetails.parentKeyFieldValue;
                        args.data.ADetailsID = maxColDetails++;
                        //var ff = masterData[0].HeadDescriptionList.filter(x => x.SGHeadID == args.data.SGHeadID);

                        //DHeadNameObj.dataSource = masterData[0].HeadDescriptionList.filter(x => x.SGHeadID == args.data.SGHeadID);
                    }
                    else if (args.requestType === "save") {

                    }
                    else if (args.requestType === "delete") {

                    }

                },
                load: loadChargeDetailsGrid

            }

        });
        $tblAcceptanceChargeDetailsEL["dataBound"] = function () {
            $tblAcceptanceChargeDetailsEL.autoFitColumns();
        };
        $tblAcceptanceChargeDetailsEL.refreshColumns;
        $tblAcceptanceChargeDetailsEL.appendTo(tblAcceptanceChargeDetailsId);
    }
    function loadChargeDetailsGrid() {
        this.dataSource = this.parentDetails.parentRowData.IDACDetailSub;

    }

    function initAcceptanceTable() {
        var commands = [];
        commands = [
            { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
            { type: 'ViewAttachedFile', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file' } }
        ]

        var columns = [
            {
                headerText: 'Actions', commands: commands, width: 100
            },
            { field: 'IssueBankId', headerText: 'IssueBankId', visible: false },
            { field: 'CompanyId', headerText: 'CompanyId', visible: false },
            { field: 'SupplierId', headerText: 'SupplierId', visible: false },
            { field: 'BankRefNumber', headerText: 'Bank Ref Number', visible: status != statusConstants.PENDING },
            { field: 'BankAcceptDate', headerText: 'Bank Accept Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING },
            {
                field: 'SupplierName', headerText: 'Supplier', visible: status != statusConstants.PARTIALLY_COMPLETED
            },
            {
                field: 'CustomerName', headerText: 'Customer', visible: status != statusConstants.PARTIALLY_COMPLETED
            },
            {
                field: 'IssueBank', headerText: 'Issue Bank', visible: status != statusConstants.PARTIALLY_COMPLETED
            },
            {
                field: 'CIValue', headerText: 'Accepted Value', textAlign: 'Right', visible: status != statusConstants.PENDING
            }
        ];
        var selectionType = "Single";

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/ciAPI/list?status=${status}&isCDAPage=${isCDAPage}`,
            columns: columns,
            commandClick: handleCommands,
            allowSelection: status == statusConstants.AWAITING_PROPOSE,
            selectionSettings: { type: selectionType, checkboxOnly: true, persistSelection: true }
        });
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



    //function calculateTotalLCQtyAll(data) {
    //    var lcQtyAll = 0;

    //    $.each(data, function (i, row) {
    //        lcQtyAll += isNaN(parseFloat(row.LCQty)) ? 0 : parseFloat(row.LCQty);
    //    });

    //    return lcQtyAll.toFixed(2);
    //}

    //function calculateTotalLCValueAll(data) {
    //    var lcValueAll = 0;

    //    $.each(data, function (i, row) {
    //        lcValueAll += isNaN(parseFloat(row.LCValue)) ? 0 : parseFloat(row.LCValue);
    //    });

    //    return lcValueAll.toFixed(2);
    //}

    //function calculateTotalYarnQtyAll(data) {
    //    var yarnPoQtyAll = 0;

    //    $.each(data, function (i, row) {
    //        yarnPoQtyAll += isNaN(parseFloat(row.TotalQty)) ? 0 : parseFloat(row.TotalQty);
    //    });

    //    return yarnPoQtyAll.toFixed(2);
    //}

    //function calculateTotalYarnValueAll(data) {
    //    var yarnPoValueAll = 0;

    //    $.each(data, function (i, row) {
    //        yarnPoValueAll += isNaN(parseFloat(row.TotalValue)) ? 0 : parseFloat(row.TotalValue);
    //    });

    //    return yarnPoValueAll.toFixed(2);
    //}

    //function calculateCITotalPIQty(data) {
    //    var ciPIQty = 0;

    //    $.each(data, function (i, row) {
    //        ciPIQty += isNaN(parseFloat(row.InvoiceQty)) ? 0 : parseFloat(row.InvoiceQty);
    //    });

    //    return ciPIQty.toFixed(2);
    //}

    //function calculateCITotalPIValue(data) {
    //    var ciPIValue = 0;

    //    $.each(data, function (i, row) {
    //        ciPIValue += isNaN(parseFloat(row.PdValue)) ? 0 : parseFloat(row.PdValue);
    //    });

    //    return ciPIValue.toFixed(2);
    //}

    //function calculateCITotalNoOfCarton(data) {
    //    var ciNoOfCarton = 0;

    //    $.each(data, function (i, row) {
    //        ciNoOfCarton += isNaN(parseFloat(row.NoOfCarton)) ? 0 : parseFloat(row.NoOfCarton);
    //    });

    //    return ciNoOfCarton.toFixed();
    //}

    //function calculateCITotalGrossWeight(data) {
    //    var ciGrossWeight = 0;

    //    $.each(data, function (i, row) {
    //        ciGrossWeight += isNaN(parseFloat(row.GrossWeight)) ? 0 : parseFloat(row.GrossWeight);
    //    });

    //    return ciGrossWeight.toFixed(2);
    //}

    //function calculateCITotalNetWeight(data) {
    //    var ciNetWeight = 0;

    //    $.each(data, function (i, row) {
    //        ciNetWeight += isNaN(parseFloat(row.NetWeight)) ? 0 : parseFloat(row.NetWeight);
    //    });

    //    return ciNetWeight.toFixed(2);
    //}

    //function calculateCITotalInvoiceValue(data) {
    //    var ciLCValue = 0;

    //    $.each(data, function (i, row) {
    //        ciLCValue += isNaN(parseFloat(row.CiValue)) ? 0 : parseFloat(row.CiValue);
    //    });

    //    return ciLCValue.toFixed(2);
    //}  
})();
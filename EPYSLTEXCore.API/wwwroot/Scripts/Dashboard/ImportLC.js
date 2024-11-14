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
    var isCDAPage = false;
    var status;
    var docAry = [];
    var isAmendentValue = false;
    var isRevision = false;

    var YarnLcMaster;

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
        isCDAPage = convertToBoolean($(`#${pageId}`).find("#CDAPage").val());
        
        $formEl.find("#pnlLCNO").hide();
        $formEl.find("#pnlLCDate").hide();

        //$formEl.find('#LCReceiveDate').datepicker('setStartDate', new Date());
        //$formEl.find('#LCExpiryDate').datepicker('setStartDate', new Date());

        status = statusConstants.PENDING;
        initMasterTable();
        initChildTable();
        getMasterTableData();

        $formEl.find('#PaymentModeID').on('select2:select', function (e) {
            EntityShowBySelect();
        });

        $formEl.find('#CompanyID').on('select2:select', function (e) { 
            if (isCDAPage) {
                //getBankBranch(); 
            }  
        });

        $toolbarEl.find("#btnPendingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;

            initMasterTable();
            getMasterTableData();

            $formEl.find("#pnlLCNO").hide();
            $formEl.find("#pnlLCDate").hide();
        });

        $toolbarEl.find("#btnLCPendingforApproval").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PARTIALLY_COMPLETED;

            initMasterTable();
            getMasterTableData();

            $formEl.find("#pnlLCNO").show();
            $formEl.find("#pnlLCDate").show();
        });

        $toolbarEl.find("#btnLCApproved").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;

            initMasterTable();
            getMasterTableData();

            $formEl.find("#pnlLCNO").show();
            $formEl.find("#pnlLCDate").show();
        });
        $toolbarEl.find("#btnLCAmendent").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.ADDITIONAL;

            initMasterTable();
            getMasterTableData();

            $formEl.find("#pnlLCNO").show();
            $formEl.find("#pnlLCDate").show();
        });
        $toolbarEl.find("#btnLCALL").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.ALL;

            initMasterTable();
            getMasterTableData();

            $formEl.find("#pnlLCNO").show();
            $formEl.find("#pnlLCDate").show();
        });

        $formEl.find("#btnSaveLC").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnApprovedLC").click(function (e) {
            e.preventDefault();
            save(this, true);
        });

        $formEl.find("#btnLCEditCancel").on("click", backToList); 

        $formEl.find("#btnViewAllPI").click(function (e) {
            e.preventDefault();
          
            $.each(YarnLcMaster.LcChilds, function (j, obj) { 
                window.open(obj.PIFilePath, "_blank");  
            });
        });
        $formEl.find("#btnViewLCImg").click(function (e) {
            e.preventDefault();
           
            if (YarnLcMaster.LCFilePath != null && YarnLcMaster.LCFilePath != "") {
                window.open(YarnLcMaster.LCFilePath, "_blank");
            } 
        });

    }); 

    function getBankBranch() {
        if (isCDAPage) { 
            axios.get(`/api/selectoption/company_bank/${$formEl.find('#CompanyID').val()}`)
                .then(function (response) {
                    initSelect2($formEl.find("#IssueBankID"), response.data);
                })
                .catch(showResponseError);
        }
    }

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
                    title: "Action",
                    align: "center",
                    visible: (status == statusConstants.PENDING || status == statusConstants.PARTIALLY_COMPLETED || status == statusConstants.ADDITIONAL || status == statusConstants.COMPLETED) ? true : false,
                    formatter: function (value, row, index, field) {
                        if (status == statusConstants.PENDING) {
                            return `<a class="btn btn-xs btn-default add" href="javascript:void(0)" title="New LC">
                                        <i class="fa fa-plus" aria-hidden="true"></i>
                                    </a>`;
                        } else if (status == statusConstants.PARTIALLY_COMPLETED) {
                            return [
                                '<span class="btn-group">',
                                '<a class="btn btn-xs btn-default edit-attachment" href="javascript:void(0)" title="Edit LC">',
                                '<i class="fa fa-edit" aria-hidden="true"></i>',
                                
                                '</span>',
                            ].join(' ');
                        }
                        else if (status == statusConstants.ADDITIONAL) {
                            return [
                                '<span class="btn-group">',
                                '<a class="btn btn-xs btn-default edit-attachment" href="javascript:void(0)" title="Edit LC">',
                                '<i class="fa fa-edit" aria-hidden="true"></i>',
                                '<a class="btn btn-xs btn-success" href="' + row.LCFilePath + '" target="_blank" title="View LC Image">',
                                '<i class="fa fa-image" aria-hidden="true"></i>',
                                '</span>',
                            ].join(' ');
                        }
                        else {
                            return [
                                '<span class="btn-group">',
                                '<a class="btn btn-xs btn-default view-attachment" href="javascript:void(0)" title="View LC">',
                                '<i class="fa fa-eye" aria-hidden="true"></i>',
                                '<a class="btn btn-xs btn-success" href="' + row.LCFilePath + '" target="_blank" title="View LC Image">',
                                '<i class="fa fa-image" aria-hidden="true"></i>',
                                '</span>',
                            ].join(' ');
                        }
                    },
                    events: {
                        'click .edit-attachment': function (e, value, row, index) {

                            //if (row.PIStatus == "Waiting For PI Review") {
                            //    toastr.error("Need to Accept PI First");
                            //    return false;
                            //}
                            

                            e.preventDefault(); 

                            $("LCPILists").fadeIn();
                            if (status == statusConstants.PARTIALLY_COMPLETED) {
                                $formEl.find("#btnProposeLC,#btnApprovedLC").fadeIn();
                                $formEl.find("#btnSaveLC").fadeOut();
                            }
                            else if (status == statusConstants.ADDITIONAL) {
                                $formEl.find("#btnProposeLC,#btnApprovedLC").fadeIn();
                                $formEl.find("#btnSaveLC").fadeOut();
                            }
                            else if (status == statusConstants.COMPLETED) {
                                $formEl.find("#btnProposeLC,#btnApprovedLC").fadeOut();
                                $formEl.find("#btnSaveLC").fadeOut();
                            }
                            else {
                                $("#btnSaveLC,#btnProposeLC,#btnApprovedLC").fadeOut();
                            }
                            initAttachment(row.LCFilePath, row.AttachmentPreviewTemplate, "#UploadFile");
                            //$formEl.find("#UploadFile,#BbReportingNumber,#BankAcceptanceFrom").prop("disabled", false);
                            $formEl.find("#UploadFile,#BbReportingNumber").prop("disabled", false);
                            getDetails(row.LCID);
                        },
                        'click .view-attachment': function (e, value, row, index) {
                            e.preventDefault(); 
                            $("LCPILists").fadeIn();
                            $formEl.find("#btnProposeLC,#btnApprovedLC").fadeOut();
                            $formEl.find("#btnSaveLC").fadeOut();
                            initAttachment(row.LCFilePath, row.AttachmentPreviewTemplate, "#UploadFile");
                            /*$formEl.find("#UploadFile,#BbReportingNumber,#BankAcceptanceFrom").prop("disabled", false);*/
                            $formEl.find("#UploadFile,#BbReportingNumber").prop("disabled", false);
                            getDetails(row.LCID);
                        },
                        'click .add': function (e, value, row, index) {
                            e.preventDefault(); 
                            initAttachment(row.LCFilePath, row.AttachmentPreviewTemplate, "#UploadFile");
                            /*$formEl.find("#UploadFile,#BbReportingNumber,#BankAcceptanceFrom").prop("disabled", true);*/
                            $formEl.find("#UploadFile,#BbReportingNumber").prop("disabled", true);
                            $formEl.find("#btnSaveLC").fadeIn();
                            $formEl.find("#btnProposeLC,#btnApprovedLC").fadeOut();
                            getNew(row.ProposalID);

                            if (row.CashStatus) {
                                $formEl.find("#divProposeContractNo").fadeOut();
                            } else {
                                $formEl.find("#divProposeContractNo").fadeIn();
                            }
                        }
                    }
                },
                {
                    field: "ProposalNo",
                    title: "Proposal No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    footerFormatter: function () {
                        return [
                            '<span >',
                            '<label title="Total">',
                            '<i style="font-size:15px"></i>',
                            ' Total:',
                            '</label>',
                            '</span>'
                        ].join('');
                    }
                },
                {
                    field: "ProposalDate",
                    title: "Proposal Date",
                    visible: status == statusConstants.PENDING ? true : false,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "LCNo",
                    title: "L/C No",
                    filterControl: "input",
                    visible: (status == statusConstants.PENDING) ? false : true,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "LCDate",
                    title: "L/C Date",
                    filterControl: "input",
                    visible: (status == statusConstants.PENDING) ? false : true,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                //{
                //    field: "CustomerName",
                //    title: "Customer",
                //    filterControl: "input",
                //    visible: status == statusConstants.PENDING ? false : true,
                //    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                //},
                //{
                //    field: "CompanyName",
                //    title: "Company",
                //    filterControl: "input",
                //    //visible: status == statusConstants.PENDING ? false : true,
                //    visible: (status == statusConstants.ALL || status == statusConstants.COMPLETED) ? true : false,
                //    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                //},
                {
                    field: "PiNo",
                    title: "PI No",
                    filterControl: "input",
                    visible: status == statusConstants.PENDING ? true : false,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                //{
                //    field: "PIStatus",
                //    title: "PI Status",
                //    filterControl: "input",
                //    visible: status == statusConstants.ADDITIONAL ? true : false,
                //    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                //},
                {
                    field: "SupplierName",
                    title: "Supplier",
                    filterControl: "input",
                    //visible: status == statusConstants.PENDING ? true : false,
                    //visible: (status == statusConstants.ALL || status == statusConstants.COMPLETED) ? false : true,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "CompanyName",
                    title: "Company",
                    filterControl: "input",
                    //visible: status == statusConstants.PENDING ? true : false,
                    //visible: (status == statusConstants.ALL || status == statusConstants.COMPLETED) ? false : true,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ProposeContractNo",
                    title: "Contract/Export LC No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "LCQty",
                    title: "Total Qty",
                    filterControl: "input",
                    align: 'right',
                    footerFormatter: calculateTotalLCQtyAll,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "LCValue",
                    title: "L/C Value",
                    filterControl: "input",
                    align: 'right',
                    footerFormatter: calculateTotalLCValueAll,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "CashStatus",
                    title: "Cash L/C?",
                    checkbox: true,
                    showSelectTitle: false,
                    checkboxEnabled: false,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    //visible: status === statusConstants.PENDING ? true : false,
                }, 
                {
                    field: "ProposeBankID",
                    title: "Propose Bank",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: false
                },
                {
                    field: "BranchName",
                    title: "Propose Bank",
                    visible: status === statusConstants.PENDING ? true : false,
                },
                {
                    field: "RetirementMode",
                    title: "Retirement Mode",
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

    function calculateTotalYarnQtyAll(data) {
        var yarnPoQtyAll = 0;

        $.each(data, function (i, row) {
            yarnPoQtyAll += isNaN(parseFloat(row.TotalQty)) ? 0 : parseFloat(row.TotalQty);
        });

        return yarnPoQtyAll.toFixed(2);
    }

    function calculateTotalYarnValueAll(data) {
        var yarnPoValueAll = 0;

        $.each(data, function (i, row) {
            yarnPoValueAll += isNaN(parseFloat(row.TotalValue)) ? 0 : parseFloat(row.TotalValue);
        });

        return yarnPoValueAll.toFixed(2);
    }

    function calculateTotalLCQtyAll(data) {
        var lcQtyAll = 0;

        $.each(data, function (i, row) {
            lcQtyAll += isNaN(parseFloat(row.LCQty)) ? 0 : parseFloat(row.LCQty);
        });

        return lcQtyAll.toFixed(2);
    }

    function calculateTotalLCValueAll(data) {
        var lcValueAll = 0;

        $.each(data, function (i, row) {
            lcValueAll += isNaN(parseFloat(row.LCValue)) ? 0 : parseFloat(row.LCValue);
        });

        return lcValueAll.toFixed(2);
    }

    function getMasterTableData() {
        var queryParams = $.param(tableParams);
        $tblMasterEl.bootstrapTable('showLoading');
        var url = `/ImportLCApi/importLCMasterData?gridType=bootstrap-table&status=${status}&isCDAPage=${isCDAPage}&${queryParams}`;
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
        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            showFooter: true,
            cache: false,
            columns: [
                {
                    title: "",
                    align: "center",
                    cellStyle: function () { return { classes: 'm-w-10' } },
                    formatter: function (value, row, index, field) { 
                        return [
                            '<span class="btn-group">', 
                            '<a class="btn btn-xs btn-primary" href="' + row.PIFilePath + '" target="_blank" title="PI Report">',
                            '<i class="fa fa-file-pdf-o" aria-hidden="true"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    } 
                },
                {
                    field: "PINo",
                    title: "PI No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    footerFormatter: function () {
                        return [
                            '<span >',
                            '<label title="Total">',
                            '<i style="font-size:15px"></i>',
                            ' Total:',
                            '</label>',
                            '</span>'
                        ].join('');
                    }
                },
                {
                    field: "PIDate",
                    title: "PI Date",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "SupplierName",
                    title: "Supplier",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "TotalQty",
                    title: "Total Qty",
                    filterControl: "input",
                    align: 'right',
                    footerFormatter: calculateTotalYarnQtyAll,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "TotalValue",
                    title: "Total Value",
                    filterControl: "input",
                    align: 'right',
                    footerFormatter: calculateTotalYarnValueAll,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                }
            ]
        });
    }

    function initAttachment(path, type, htmlEl) {
        if (!path) {
            initNewAttachment(htmlEl);
            return;
        }

        var preveiwData = [rootPath + path];
        var previewConfig = [{ type: type, caption: "L/C Attachment", key: 1 }];

        $(htmlEl).fileinput('destroy');
        $(htmlEl).fileinput({
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

    function initNewAttachment(htmlEl) {
        $(htmlEl).fileinput('destroy');
        $(htmlEl).fileinput({
            showUpload: false,
            previewFileType: 'any'
        });
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        getMasterTableData();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#LCID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(ProposalID) {
        axios.get(`/ImportLCApi/getProposal/${ProposalID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                
                YarnLcMaster = response.data;
                YarnLcMaster.LCDate = formatDateToDefault(YarnLcMaster.LCDate);
                YarnLcMaster.LCReceiveDate = formatDateToDefault(YarnLcMaster.LCReceiveDate);
                YarnLcMaster.LCExpiryDate = formatDateToDefault(YarnLcMaster.LCExpiryDate);
                YarnLcMaster.DocPresentationDays = YarnLcMaster.DocPresentationDays;
                YarnLcMaster.IsConInsWith = true;
                setFormData($formEl, YarnLcMaster);
                $formEl.find("#IsCDA").val(isCDAPage);
                if (YarnLcMaster.IsConInsWith) $('#IsConInsWith').attr('checked', 'checked');
                else $('#IsConInsWithout').attr('checked', 'checked'); 
               
                $tblChildEl.bootstrapTable("load", YarnLcMaster.LcChilds);
                $tblChildEl.bootstrapTable('hideLoading'); 

                docAry = [];
                var template = '';
                $.each(YarnLcMaster.CommercialAttachmentList, function (i, v) {
                    docAry.push({ value: 'DocumentsRequired-' + v.id, id: v.id });
                    template += ''
                        + '<input type="checkbox" style="margin:5px;" id="DocumentsRequired-' + v.id + '" name="DocumentsRequired-' + v.id + '" value="' + v.id + '" />'
                        + '<label for="DocumentsRequired-' + v.id + '" style="margin-right:15px;">' + v.text + '</label>'
                });
                $formEl.find("#divCommercialAttachmentDocs").empty();
                $formEl.find("#divCommercialAttachmentDocs").append(template);
                EntityShowBySelect();

                if (isCDAPage) {
                    //getBankBranch();
                    $formEl.find("#CompanyID").prop("disabled", true);
                    $formEl.find("#IssueBankID").prop("disabled", true);
                } else {
                    $formEl.find("#CompanyID").prop("disabled", true);
                    $formEl.find("#IssueBankID").prop("disabled", true);
                }

                $formEl.find("#IssueBankID").val(YarnLcMaster.IssueBankID).trigger('change');
                isAmendentValue = false;
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        
        axios.get(`/ImportLCApi/${id}`)
            .then(function (response) {
                
                YarnLcMaster = response.data;
                
                if (YarnLcMaster.LcChilds.some(child => child.BBLCStatus === "BBLC Not Revised")) {
                    toastr.error("Waiting For BBLC Revision");
                }
                else {
                    $divDetailsEl.fadeIn();
                    $divTblEl.fadeOut();

                    YarnLcMaster.LCDate = formatDateToDefault(YarnLcMaster.LCDate);
                    YarnLcMaster.LCReceiveDate = formatDateToDefault(YarnLcMaster.LCReceiveDate);
                    YarnLcMaster.LCExpiryDate = formatDateToDefault(YarnLcMaster.LCExpiryDate);
                    YarnLcMaster.DocPresentationDays = YarnLcMaster.DocPresentationDays;

                    setFormData($formEl, YarnLcMaster);
                    $formEl.find("#IsCDA").val(isCDAPage);

                    $tblChildEl.bootstrapTable("load", YarnLcMaster.LcChilds);
                    $tblChildEl.bootstrapTable('hideLoading');
                    if (YarnLcMaster.IsConInsWith) $('#IsConInsWith').attr('checked', 'checked');
                    else $('#IsConInsWithout').attr('checked', 'checked');
                    docAry = [];
                    var template = '';
                    $.each(YarnLcMaster.CommercialAttachmentList, function (i, v) {
                        docAry.push({ value: 'DocumentsRequired-' + v.id, id: v.id });
                        template += ''
                            + '<input type="checkbox" style="margin:5px;" id="DocumentsRequired-' + v.id + '" name="DocumentsRequired-' + v.id + '" value="' + v.id + '" />'
                            + '<label for="DocumentsRequired-' + v.id + '" style="margin-right:15px;">' + v.text + '</label>'
                    });
                    $formEl.find("#divCommercialAttachmentDocs").empty();
                    $formEl.find("#divCommercialAttachmentDocs").append(template);
                    $.each(YarnLcMaster.LcDocuments, function (i, v) {
                        $formEl.find("#DocumentsRequired-" + v.DocId).prop("checked", "true");
                    });
                    EntityShowBySelect();


                    if (isCDAPage) {
                        //getBankBranch();
                        $formEl.find("#CompanyID").prop("disabled", true);
                        $formEl.find("#IssueBankID").prop("disabled", true);
                    } else {
                        $formEl.find("#CompanyID").prop("disabled", true);
                        $formEl.find("#IssueBankID").prop("disabled", true);
                    }
                    //$formEl.find("#divCommercialAttachmentDocs").val(YarnLcMaster.IssueBankID).trigger('change');

                    $formEl.find("#IssueBankID").val(YarnLcMaster.IssueBankID).trigger('change');
                    //Pending Amendent List
                    if (status == statusConstants.ADDITIONAL) {
                        isAmendentValue = true;
                    } else {
                        isAmendentValue = false;
                    }
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function EntityShowBySelect() {
        if ($formEl.find("#PaymentModeID").val() == "1") {
            $formEl.find("#pnlTenureofLCId").hide();
        }
        else {
            $formEl.find("#pnlTenureofLCId").show();
        }
    }

    var validationConstraints = {
        LCReceiveDate: {
            presence: true
        },
        LCExpiryDate: {
            presence: true
        },
        LCValue: {
            presence: true
        },
        LCQty: {
            presence: true
        },
        CompanyID: {
            presence: true
        }
    }
    function isValidMasterForm($formEl) {
        var isValidItemInfo = false;

        if ($formEl.find("#PaymentModeID").val() == "2") {
            if ($formEl.find("#TenureofLCID").val() == "" || $formEl.find("#TenureofLCID").val() == null) {
                toastr.error("Tenure of L/C is required.");
                isValidItemInfo = true;
            }
            if ($formEl.find("#IssueBankID").val() == "" || $formEl.find("#IssueBankID").val() == null) {
                toastr.error("D.C. Opening Bank");
                isValidItemInfo = true;
            }
            if ($formEl.find("#CurrencyID").val() == "" || $formEl.find("#CurrencyID").val() == null) {
                toastr.error("Select currency.");
                isValidItemInfo = true;
            }
        }
        var LCReceiveDate = new Date($formEl.find("#LCReceiveDate").val());
        var LCExpiryDate = new Date($formEl.find("#LCExpiryDate").val());
        if (LCReceiveDate > LCExpiryDate) {
            toastr.error("Last Shipment Date must be less then L/C Expiry Date");
            isValidItemInfo = true;
        }

        return isValidItemInfo;
    }

    function save(invokedBy, isApprove = false) {
   

        var formData = getFormData($formEl);
        if (isApprove) {
            var files = $formEl.find("#UploadFile")[0].files;
            //if (!files || files.length == 0) return toastr.error("You must upload an image!");

            var fileExists = YarnLcMaster.LCFilePath;
            if (!fileExists) {
                if (!files || files.length == 0) return toastr.error("You must upload an image!");
            }
            else {
                formData.append("LCFilePath", YarnLcMaster.LCFilePath);
                formData.append("AttachmentPreviewTemplate", YarnLcMaster.AttachmentPreviewTemplate);
            }

            //const file = this.files[0];
            //const fileType = files[0]['type'];
            //const validImageTypes = ['image/gif', 'image/jpeg', 'image/jpg', 'image/png'];
            //if (!validImageTypes.contains(fileType)) {
            //    return toastr.error("You must upload an image!");
            //}

            formData.append("UploadFile", files[0]);

            if ($formEl.find("#LCNo").val() == "") {
                return toastr.error("LC No is required.");
            }
            if ($formEl.find("#LCDate").val() == "") {
                return toastr.error("LC Date is required.");
            }
        }
        formData.append("Approve", isApprove); 
        var dList = [];
        for (var i = 0; i < docAry.length; i++) {
            if ($formEl.find("#" + docAry[i].value).is(':checked'))
                dList.push({ DocID: docAry[i].id });
        }
        YarnLcMaster.LcDocuments = dList;

        //
        formData.append("LcChilds", JSON.stringify(YarnLcMaster.LcChilds));
        formData.append("LcDocuments", JSON.stringify(YarnLcMaster.LcDocuments));
        formData.append("IsConInsWith", $('input[name="IsConInsWithRadio"]:checked').val());

        formData.append("CompanyID", YarnLcMaster.CompanyID);
        formData.append("IssueBankID", YarnLcMaster.IssueBankID);
        formData.append("ProposalNo", YarnLcMaster.ProposalNo);
        formData.append("SupplierID", YarnLcMaster.SupplierID);
        formData.append("isAmendentValue", isAmendentValue);
        
        if (status == statusConstants.ADDITIONAL) {
            isRevision = true;
            formData.append("RevisionNo", YarnLcMaster.BBLCRevisionNo);
        }
        else {
            formData.append("RevisionNo", YarnLcMaster.RevisionNo);
        }
        formData.append("isRevision", isRevision);
        
        if ($formEl.find("#CashStatus").is(':checked')) {
            formData.append("CashStatus", $('input[name="CashStatus"]:checked').val());
        }
        else {
            formData.append("CashStatus", false);
        }  

        const config = {
            headers: {
                'content-type': 'multipart/form-data',
                'Authorization': "Bearer " + localStorage.getItem("token")
            }
        }

        //Master Value check
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        if (isValidMasterForm($formEl)) return;

        axios.post("/ImportLCApi/save", formData, config)
            .then(function (response) {
                toastr.success("Save successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
})();
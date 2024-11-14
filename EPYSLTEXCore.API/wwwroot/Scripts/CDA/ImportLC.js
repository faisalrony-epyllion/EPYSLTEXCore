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
    var status;
    var docAry = [];

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

        status = statusConstants.PENDING;
        $("#btnLCNew").css('background', '#5cb85c');
        $("#btnLCNew").css('color', '#FFFFFF');
        
        initMasterTable();
        initChildTable();
        getMasterTableData();

        $toolbarEl.find("#btnLCNew").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;

            $("#btnLCNew").css('background', '#5cb85c');
            $("#btnLCNew").css('color', '#FFFFFF');

            $("#btnLCPendingforApproval").css('background', '#FFFFFF');
            $("#btnLCPendingforApproval").css('color', '#000000');

            $("#btnLCApproved").css('background', '#FFFFFF');
            $("#btnLCApproved").css('color', '#000000');

            $("#btnLCALL").css('background', '#FFFFFF');
            $("#btnLCALL").css('color', '#000000');

            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnLCPendingforApproval").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PARTIALLY_COMPLETED;

            $("#btnLCPendingforApproval").css('background', '#5cb85c');
            $("#btnLCPendingforApproval").css('color', '#FFFFFF');

            $("#btnLCNew").css('background', '#FFFFFF');
            $("#btnLCNew").css('color', '#000000');

            $("#btnLCApproved").css('background', '#FFFFFF');
            $("#btnLCApproved").css('color', '#000000');

            $("#btnLCALL").css('background', '#FFFFFF');
            $("#btnLCALL").css('color', '#000000');

            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnLCApproved").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;

            $("#btnLCApproved").css('background', '#5cb85c');
            $("#btnLCApproved").css('color', '#FFFFFF');

            $("#btnLCPendingforApproval").css('background', '#FFFFFF');
            $("#btnLCPendingforApproval").css('color', '#000000');

            $("#btnLCNew").css('background', '#FFFFFF');
            $("#btnLCNew").css('color', '#000000');

            $("#btnLCALL").css('background', '#FFFFFF');
            $("#btnLCALL").css('color', '#000000');

            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnLCALL").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.ALL;

            $("#btnLCALL").css('background', '#5cb85c');
            $("#btnLCALL").css('color', '#FFFFFF');

            $("#btnLCNew").css('background', '#FFFFFF');
            $("#btnLCNew").css('color', '#000000');

            $("#btnLCPendingforApproval").css('background', '#FFFFFF');
            $("#btnLCPendingforApproval").css('color', '#000000');

            $("#btnLCApproved").css('background', '#FFFFFF');
            $("#btnLCApproved").css('color', '#000000');

            initMasterTable();
            getMasterTableData();
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
                    title: "Action",
                    align: "center",
                    visible: (status == statusConstants.PENDING || status == statusConstants.PARTIALLY_COMPLETED || status == statusConstants.COMPLETED) ? true : false,
                    formatter: function (value, row, index, field) {
                        if (status == statusConstants.PENDING) {
                            return `<a class="btn btn-xs btn-default add" href="javascript:void(0)" title="New LC">
                                        <i class="fa fa-plus" aria-hidden="true"></i>
                                    </a>`;
                        } else {
                            return [
                                '<span class="btn-group">',
                                '<a class="btn btn-xs btn-default edit-attachment" href="javascript:void(0)" title="Edit LC">',
                                '<i class="fa fa-edit" aria-hidden="true"></i>',
                                '<a class="btn btn-xs btn-default" href="' + row.LcFilePath + '" target="_blank" title="View LC">',
                                '<i class="fa fa-eye" aria-hidden="true"></i>',
                                '</span>',
                            ].join(' ');
                        }

                    },
                    events: {
                        'click .edit-attachment': function (e, value, row, index) {
                            e.preventDefault();
                            $("LCPILists").fadeIn();
                            if (status == statusConstants.PARTIALLY_COMPLETED) {
                                $("#btnProposeLC").fadeIn();
                                $("#btnApprovedLC").fadeIn();
                                $("#btnSaveLC").fadeOut();
                            }
                            else if (status == statusConstants.COMPLETED) {
                                $("#btnProposeLC").fadeOut();
                                $("#btnApprovedLC").fadeOut();
                                $("#btnSaveLC").fadeOut();
                            }
                            else {
                                $("#btnProposeLC").fadeOut();
                                $("#btnApprovedLC").fadeOut();
                                $("#btnSaveLC").fadeOut();
                            }
                            initAttachment(row.LcFilePath, row.AttachmentPreviewTemplate, "#UploadFile");
                            $formEl.find("#LcNo,#LcDate,#UploadFile").prop("disabled", false);
                            getDetails(row.Id);
                        },
                        'click .add': function (e, value, row, index) {
                            e.preventDefault();
                            initAttachment(row.LcFilePath, row.AttachmentPreviewTemplate, "#UploadFile");
                            $formEl.find("#LcNo,#LcDate,#UploadFile").prop("disabled", true);
                            getNew(row.ProposalId);
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
                    field: "LcNo",
                    title: "L/C No",
                    filterControl: "input",
                    visible: (status == statusConstants.PENDING) ? false : true,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "LcDate",
                    title: "L/C Date",
                    filterControl: "input",
                    visible: (status == statusConstants.PENDING) ? false : true,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "CustomerName",
                    title: "Customer",
                    filterControl: "input",
                    visible: status == statusConstants.PENDING ? false : true,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "PiNo",
                    title: "PI No",
                    filterControl: "input",
                    visible: status == statusConstants.PENDING ? true : false,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "SupplierName",
                    title: "Supplier",
                    filterControl: "input",
                    visible: status == statusConstants.PENDING ? true : false,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "TExportLCNo",
                    title: "Export L/C No",
                    filterControl: "input",
                    //visible: status == statusConstants.PENDING ? true : false,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "LcQty",
                    title: "Total Qty",
                    filterControl: "input",
                    align: 'right',
                    footerFormatter: calculateTotalLCQtyAll,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "LcValue",
                    title: "L/C Value",
                    filterControl: "input",
                    align: 'right',
                    footerFormatter: calculateTotalLCValueAll,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                //{
                //    field: "LCStatus",
                //    title: "LC Status",
                //    filterControl: "input",
                //    visible: isAllChecked,
                //    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                //    visible: status == statusConstants.PENDING ? false : true
                //},


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
            lcQtyAll += isNaN(parseFloat(row.LcQty)) ? 0 : parseFloat(row.LcQty);
        });

        return lcQtyAll.toFixed(2);
    }
    function calculateTotalLCValueAll(data) {
        var lcValueAll = 0;

        $.each(data, function (i, row) {
            lcValueAll += isNaN(parseFloat(row.LcValue)) ? 0 : parseFloat(row.LcValue);
        });

        return lcValueAll.toFixed(2);
    }

    function getMasterTableData() {
        
        var queryParams = $.param(tableParams);
        $tblMasterEl.bootstrapTable('showLoading');
        var url = `/cdaImportLC/importLCMasterData?status=${status}&${queryParams}`;
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

    function getNew(ProposalId) {
        axios.get(`/cdaImportLC/getProposal/${ProposalId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                YarnLcMaster = response.data;
                YarnLcMaster.LcDate = formatDateToDefault(YarnLcMaster.LcDate);
                YarnLcMaster.LcReceiveDate = formatDateToDefault(YarnLcMaster.LcReceiveDate);
                YarnLcMaster.LcExpiryDate = formatDateToDefault(YarnLcMaster.LcExpiryDate);
                YarnLcMaster.DocPresentationDate = formatDateToDefault(YarnLcMaster.DocPresentationDate);

                setFormData($formEl, YarnLcMaster);
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

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/cdaImportLC/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                YarnLcMaster = response.data;
                
                YarnLcMaster.LcDate = formatDateToDefault(YarnLcMaster.LcDate);
                YarnLcMaster.LcReceiveDate = formatDateToDefault(YarnLcMaster.LcReceiveDate);
                YarnLcMaster.LcExpiryDate = formatDateToDefault(YarnLcMaster.LcExpiryDate);
                YarnLcMaster.DocPresentationDate = formatDateToDefault(YarnLcMaster.DocPresentationDate);

                setFormData($formEl, YarnLcMaster);
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
                $.each(YarnLcMaster.LcDocuments, function (i, v) {
                    $formEl.find("#DocumentsRequired-" + v.DocId).prop("checked", "true");
                });
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    var validationConstraints = {
        LcNo: {
            presence: true
        },
        LcDate: {
            presence: true
        }
    }

    function save(invokedBy, isApprove = false) {
        
        var formData = getFormData($formEl);
        if (isApprove) {
            var files = $formEl.find("#UploadFile")[0].files;
            if (!files || files.length == 0) return toastr.error("You must upload an image!");

            //const file = this.files[0];
            //const fileType = files[0]['type'];
            //const validImageTypes = ['image/gif', 'image/jpeg', 'image/jpg', 'image/png'];
            //if (!validImageTypes.contains(fileType)) {
            //    return toastr.error("You must upload an image!");
            //}

            formData.append("UploadFile", files[0]);

            initializeValidation($formEl, validationConstraints);
            if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
            else hideValidationErrors($formEl);
        }
        formData.append("Approve", isApprove);
        var dList = [];
        for (var i = 0; i < docAry.length; i++) {
            if ($formEl.find("#" + docAry[i].value).is(':checked'))
                dList.push({ DocID: docAry[i].id });
        }
        YarnLcMaster.LcDocuments = dList;

        formData.append("LcChilds", JSON.stringify(YarnLcMaster.LcChilds));
        formData.append("LcDocuments", JSON.stringify(YarnLcMaster.LcDocuments));

        const config = {
            headers: {
                'content-type': 'multipart/form-data',
                'Authorization': "Bearer " + localStorage.getItem("token")
            }
        }

        axios.post("/cdaImportLC/save", formData, config)
            .then(function (response) {
                showBootboxAlert('Save successfully!');
                backToList();
            })
            .catch(showResponseError);

    }


})();
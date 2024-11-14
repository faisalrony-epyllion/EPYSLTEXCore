(function () {
    var menuId, pageName;
    var toolbarId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $formEl, $tblBookingAnalysisChildFabric;
    var yComposition, yType, shadeName, yProgram, ySubProgram, yarnCount, FinalYarnCategory, status;
    var bookingAnalysisMaster, ShadeID2, yarnTypeName, yarnColorName, com1V, com2V, Com1V, Com2V;
    var filterBy = {};
    var yearnCompositionfilterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    };
    var yarnCompositionTableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    };

    var yarnCountList = [];
    var yarnCountALLList = [];
    var processMasterList = [];
    var bookingHistoryMasterList = [];
    var bookingAnalysisChildRevisionHistoryList = [];
    var constructionChildList = [];
    var yarnAdditionalBookingLists = [];

    var childEl;
    var childSaveData = [];
    var isAllChecked = false;
    var isCustomChecked = false;
    var isCustomCheckedYarnColor = false;
    var isCustomCheckedYarnCount = false;
    var isFMTDDateChecked = false;
    var isAdditionalBooking = false;
    var isAdditionBookingButton = false;
    var isBAnalysisNoChecked = false;
    var isTextileEntry = false;
    var isTextileApprovalEntry = false;
    var bookingQty;
    var bookingChildId;
    var constructionId;
    var fabricGsm;
    var fabricWidth;
    var shadeId;

    var bookingAnalysisChildDyeingList = [];
    var bookingAnalysisChildFinishingList = [];

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $pageEl = $(pageConstants.PAGE_ID_PREFIX + pageId);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        $tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        $tblBookingAnalysisChildFabric = $formEl.find("#tblFabricBookingAnalysisChildFabric");
        $tblBookingAnalysisChildCollar = $formEl.find("#tblFabricBookingAnalysisChildCollar");
        $tblBookingAnalysisChildCuff = $formEl.find("#tblFabricBookingAnalysisChildCuff");

        status = statusConstants.PENDING;
        initMasterTable();
        getMasterData();

        isFMTDDateChecked = false;

        initTblYarnComposition();
        initTblYarnCompositionCollar();
        initTblYarnCompositionCuff();

        // #region Toolbar Events
        $toolbarEl.find("#btnPartiallyCompleted").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetBookingAnalysisTableParams();
            status = 7;
            isAllChecked = false;
            isFMTDDateChecked = false;
            isLaterCustomChecked = true;
            isAdditionalBooking = false;
            isAdditionBookingButton = false;
            isBAnalysisNoChecked = true;
            isTextileEntry = true;
            isTextileApprovalEntry = false;
            initMasterTable();
            getMasterData();
        });

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetBookingAnalysisTableParams();
            status = 2;
            isAllChecked = false;
            isFMTDDateChecked = false;
            isLaterCustomChecked = true;
            isAdditionalBooking = false;
            isAdditionBookingButton = false;
            isBAnalysisNoChecked = false;
            isTextileEntry = false;
            isTextileApprovalEntry = false;
            initMasterTable();
            getMasterData();
        });

        $toolbarEl.find("#btnProposed").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetBookingAnalysisTableParams();
            status = 4;
            isAllChecked = false;
            isFMTDDateChecked = false;
            isAdditionalBooking = false;
            isAdditionBookingButton = false;
            isBAnalysisNoChecked = true;
            isTextileEntry = true;
            isTextileApprovalEntry = false;
            initMasterTable();
            getMasterData();
        });

        $toolbarEl.find("#btnApproved").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetBookingAnalysisTableParams();
            status = 5;
            isAllChecked = false;
            isFMTDDateChecked = false;
            isAdditionalBooking = false;
            isAdditionBookingButton = true;
            isBAnalysisNoChecked = true;
            isTextileEntry = true;
            isTextileApprovalEntry = true;
            initMasterTable();
            getMasterData();
        });

        $toolbarEl.find("#btnAcknowledge").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetBookingAnalysisTableParams();
            status = 6;
            isFMTDDateChecked = true;
            isAllChecked = false;
            isAdditionalBooking = false;
            isAdditionBookingButton = true;
            isBAnalysisNoChecked = true;
            isTextileEntry = true;
            isTextileApprovalEntry = true;
            initMasterTable();
            getMasterData();
        });

        $toolbarEl.find("#btnAll").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetBookingAnalysisTableParams();
            status = 8;
            isFMTDDateChecked = false;
            isAllChecked = true;
            isAdditionalBooking = false;
            isBAnalysisNoChecked = true;
            initMasterTable();
            getMasterData();
        });

        $toolbarEl.find("#btnReject").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetBookingAnalysisTableParams();
            status = 9;
            isFMTDDateChecked = false;
            isAdditionalBooking = false;
            isTextileEntry = true;
            initMasterTable();
            getMasterData();
        });
        // #endregion

        // #region Action Btn Events
        $formEl.find("#btnSaveBookingAnalysis").click(save);

        $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").click(function (e) {
            e.preventDefault();

            var data = formDataToJson($formEl.serializeArray());
            data.AdditionalBooking = $formEl.find("#AdditionalBookingNew").val();

            var reasonIdArray = $formEl.find("#ReasonIds").val();
            data.ReasonIds = reasonIdArray;
            data.Id = 0;

            var hasChildYarnCount = 0;
            $.each(bookingAnalysisMaster.Childs, function (i, child) {
                if (child.ChildYarns.length)
                    hasChildYarnCount++;
            });

            data["Childs"] = bookingAnalysisMaster.Childs;

            if (hasChildYarnCount > 0) {
                processMasterList[0].CompletionStatus = "Partially Completed";
                if (hasChildYarnCount == bookingAnalysisMaster.Childs.length)
                    processMasterList[0].CompletionStatus = "Completed";
            }

            data["ProcessChilds"] = processMasterList;

            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/api/bookinganalysisAdditionalBooking", data, config)
                .then(function () {
                    toastr.success("Your Additional Booking saved successfully.");
                    backToListBookingBI();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnSaveBookingAnalysisDyeing").click(function (e) {
            e.preventDefault();
            var data = formDataToJson($formEl.serializeArray());
            data.Id = 0;
            data["FabricBookingAnalysisDyeingChilds"] = bookingAnalysisChildDyeingList;

            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/api/bookinganalysisDyeing", data, config)
                .then(function () {
                    toastr.success("Your dyeing processes saved successfully.");
                    var data = {};
                    data.BAnalysisId = $("#PreBAnalysisId").val();
                    data.TProcessMasterId = processMasterList[2].TProcessMasterId;

                    var config = { headers: { 'Content-Type': 'application/json' } };
                    axios.post("/api/bookinganalysisprocessupdatebydyeing", data, config)
                        .then(function () {
                            backToListBookingBI();
                        })
                        .catch(showResponseError);
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnSaveBookingAnalysisFinishing").click(function (e) {
            e.preventDefault();
            var data = formDataToJson($formEl.serializeArray());
            data.Id = 0;
            data["FabricBookingAnalysisFinishingChilds"] = bookingAnalysisChildFinishingList;

            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/api/bookinganalysisFinishing", data, config)
                .then(function () {
                    toastr.success("Your finishing processes saved successfully.");
                    var data = {};
                    data.BAnalysisId = $("#PreBAnalysisId").val();
                    data.TProcessMasterId = processMasterList[3].TProcessMasterId;

                    var config = { headers: { 'Content-Type': 'application/json' } };
                    axios.post("/api/bookinganalysisprocessupdatebydyeing", data, config)
                        .then(function () {
                            backToListBookingBI();
                        })
                        .catch(showResponseError);
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnProposeBookingAnalysis").click(function (e) {
            e.preventDefault();
            var data = {};
            data.BAnalysisId = $formEl.find("#Id").val();
            data.TProcessMasterId = processMasterList[0].TProcessMasterId;

            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/api/proposebookinganalysisprocess", data, config)
                .then(function () {
                    toastr.success(constants.PROPOSE_SUCCESSFULLY);
                    backToListBookingBI();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnApprovedBookingAnalysis").click(function (e) {
            e.preventDefault();
            var data = {};
            data.BAnalysisId = $formEl.find("#Id").val();
            data.TProcessMasterId = processMasterList[0].TProcessMasterId;
            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/api/approvebookinganalysisprocess", data, config)
                .then(function () {
                    toastr.success(constants.APPROVE_SUCCESSFULLY);
                    backToListBookingBI();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnRejectBookingAnalysis").click(function (e) {
            e.preventDefault();

            bootbox.prompt("Are you sure you want to reject this?", function (result) {
                if (!result) {
                    return toastr.error("Reject reason is required.");
                }

                var data = {};
                data.BAnalysisId = $formEl.find("#Id").val();
                data.TProcessMasterId = processMasterList[0].TProcessMasterId;
                data.RejectReason = result;

                var config = { headers: { 'Content-Type': 'application/json' } };
                axios.post("/api/rejectbookinganalysisprocess", data, config)
                    .then(function () {
                        toastr.success(constants.REJECT_SUCCESSFULLY);
                        backToListBookingBI();
                    })
                    .catch(showResponseError);
            });
        });

        $formEl.find("#btnAcknowledgeBookingAnalysis").click(function (e) {
            e.preventDefault();

            var data = {};
            data.BAnalysisId = $formEl.find("#Id").val();
            data.TProcessMasterId = processMasterList[0].TProcessMasterId;

            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/api/acknowledgebookinganalysisprocess", data, config)
                .then(function () {
                    toastr.success(constants.SUCCESS_MESSAGE);
                    backToListBookingBI();
                    $("#btnBookingAnalysisAcknowledgeBookingAnalysis").hasClass('active');
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnResetForm").click(function (e) {
            e.preventDefault();
            resetForm();
        });

        $formEl.find("#btnEditCancelBookingAnalysis").on("click", function (e) {
            e.preventDefault();
            backToListBookingBI();
        });

        $formEl.find("#btnEditCancelLab").on("click", function (e) {
            e.preventDefault();
            backToListBookingBI();
        });
        $formEl.find("#btnEditCancelDyeing").on("click", function (e) {
            e.preventDefault();
            backToListBookingBI();
        });
        $formEl.find("#btnEditCancelFinishing").on("click", function (e) {
            e.preventDefault();
            backToListBookingBI();
        });
        // #endregion
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
            rowStyle: function (row, index) {
                return getRowStyle(row.BookingType);
            },
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
                            resetForm();
                            $formEl.find("#divEditBookingAnalysisMaster1").fadeOut();
                            $formEl.find("#divEditBookingAnalysisLab").fadeOut();
                            $formEl.find("#divEditBookingAnalysisDyeing").fadeOut();
                            $formEl.find("#divEditBookingAnalysisFinishing").fadeOut();
                            $formEl.find("#BookingNo").val(row.BookingNo);

                            switch (row.BookingType) {
                                case bookingTypeConstants.SAMPLE:
                                    $formEl.find("#RevisionHistory").fadeOut();
                                    //getBookingAnalysisMasterSampleBooking(row.Id, row.BookingNo);
                                    getBookingAnalysisMaster(row.Id, row.BookingNo, row.BookingType);
                                    $formEl.find("#divButtonExecutionsBookingBI").fadeIn();
                                    break;
                                case bookingTypeConstants.BULK:
                                    $formEl.find("#RevisionHistory").fadeOut();
                                    getBookingAnalysisMaster(row.Id, row.BookingNo, row.BookingType);
                                    break;
                                case bookingTypeConstants.REVISED:
                                    $formEl.find("#RevisionHistory").fadeIn();
                                    getBookingAnalysisMaster(row.Id, row.BookingNo, row.BookingType);
                                    getBookingRevisionHistoryMasterData(row.BookingNo);
                                    initYarnBookingRevisionHistoryMaster();
                                    break;
                                default:
                                    break;
                            }

                            if (row.id) getBookingAnalysisProcess(row.Id);
                            else getTextileProcess();
                        }
                    }
                },
                {
                    field: "BookingNo",
                    title: "Booking No",
                    filterControl: "input",
                    visible: !isTextileEntry,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BAnalysisNo",
                    title: "Analysis No",
                    filterControl: "input",
                    visible: isBAnalysisNoChecked,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ExportOrderNo",
                    title: "EWO No",
                    filterControl: "input",
                    visible: !isTextileEntry,
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
                    field: "StyleNo",
                    title: "Style No",
                    filterControl: "input",
                    visible: !isAllChecked,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "MerchandiserName",
                    title: "Merchandiser",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BookingDate",
                    title: "Booking Date",
                    filterControl: "input",
                    visible: !isAllChecked,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "AcknowledgeDate",
                    title: "Acknowledged Date",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "EntryBy",
                    title: "Analysis By",
                    filterControl: "input",
                    visible: isTextileEntry,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "DateAdded",
                    title: "Analysis Date",
                    filterControl: "input",
                    visible: isTextileEntry,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "ApprovedBy",
                    title: "Approved By",
                    filterControl: "input",
                    visible: isTextileApprovalEntry,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ApprovedDate",
                    title: "Approved Date",
                    filterControl: "input",
                    visible: isTextileApprovalEntry,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },

                {
                    field: "TNADays",
                    title: "TNA",
                    filterControl: "input",
                    visible: !isTextileEntry,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BookingType",
                    title: "BookingType",
                    filterControl: "input",
                    visible: false,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Knitting",
                    title: "Knitting",
                    filterControl: "input",
                    visible: isAllChecked,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Lab",
                    title: "Lab",
                    filterControl: "input",
                    visible: isAllChecked,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Dyeing",
                    title: "Dyeing",
                    filterControl: "input",
                    visible: isAllChecked,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "Wash",
                    title: "Wash",
                    filterControl: "input",
                    visible: isAllChecked,
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

                getMasterData();
            },
            onSort: function (name, order) {
                tableParams.sort = name;
                tableParams.order = order;
                tableParams.offset = 0;

                getMasterData();
            },
            onRefresh: function () {
                resetBookingAnalysisTableParams();
                getMasterData();
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

                getMasterData();
            }
        });
    }

    function initTblTextileProcess() {
        $formEl.find("#tblTextileProcessMaster").bootstrapTable('destroy').bootstrapTable({
            columns: [
                {
                    title: 'Actions',
                    align: 'center',
                    width: 50,
                    formatter: function (value, row, index, field) {
                        var action = row.ProcessStatus == "Pending" ? "Create" : row.ProcessStatus == "Acknowledged" ? "View" : row.ProcessStatus == "Approved" ? "View" : "Edit";
                        return [
                            '<span class="btn-group">',
                            '<a class="btn btn-success btn-xs m-w-50 create" href="javascript:void(0)" title="' + row.ActionStatus + '">',
                            action,
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .create': function (e, value, row, index) {
                            e.preventDefault();
                            $formEl.find("#divEditBookingAnalysisMasterCollar").fadeOut();
                            $formEl.find("#divEditBookingAnalysisMasterCuff").fadeOut();

                            $formEl.find("#TProcessMasterID").val(row.TProcessMasterId);
                            if (status === statusConstants.PENDING) {
                                $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                $formEl.find("#btnSaveBookingAnalysis").fadeOut();
                                $formEl.find("#btnApprovedBookingAnalysis").fadeOut();
                                $formEl.find("#btnRejectBookingAnalysis").fadeOut();
                                $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                                $formEl.find("#btnProposeBookingAnalysis").fadeOut();
                                $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeOut();
                                $formEl.find("#divAdditionalBooking").fadeOut();
                            }
                            else if (status === statusConstants.PROPOSED) {
                                $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                $formEl.find("#btnSaveBookingAnalysis").fadeIn();
                                $formEl.find("#btnApprovedBookingAnalysis").fadeIn();
                                $formEl.find("#btnRejectBookingAnalysis").fadeIn();
                                $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                                $formEl.find("#btnProposeBookingAnalysis").fadeOut();
                                $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeOut();
                                $formEl.find("#divAdditionalBooking").fadeOut();
                            }
                            else if (status === statusConstants.APPROVED) {
                                $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                $formEl.find("#btnSaveBookingAnalysis").fadeOut();
                                $formEl.find("#btnApprovedBookingAnalysis").fadeOut();
                                $formEl.find("#btnRejectBookingAnalysis").fadeOut();
                                $formEl.find("#btnAcknowledgeBookingAnalysis").fadeIn();
                                $formEl.find("#btnProposeBookingAnalysis").fadeOut();
                                $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeOut();
                                $formEl.find("#divAdditionalBooking").fadeOut();
                            }
                            else if (status == statusConstants.ACKNOWLEDGE) {
                                $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                $formEl.find("#btnSaveBookingAnalysis").fadeOut();
                                $formEl.find("#btnApprovedBookingAnalysis").fadeOut();
                                $formEl.find("#btnRejectBookingAnalysis").fadeOut();
                                $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                                $formEl.find("#btnProposeBookingAnalysis").fadeOut();
                                $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeOut();
                                $formEl.find("#divAdditionalBooking").fadeOut();
                            }
                            else if (status === statusConstants.PARTIALLY_COMPLETED) {
                                $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                $formEl.find("#btnSaveBookingAnalysis").fadeIn();
                                $formEl.find("#btnApprovedBookingAnalysis").fadeOut();
                                $formEl.find("#btnRejectBookingAnalysis").fadeOut();
                                $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                                $formEl.find("#btnProposeBookingAnalysis").fadeIn();
                                $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeOut();
                                $formEl.find("#divAdditionalBooking").fadeOut();
                            }
                            else if (status === statusConstants.ADDITIONAL) {
                                $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                $formEl.find("#btnSaveBookingAnalysis").fadeOut();
                                $formEl.find("#btnApprovedBookingAnalysis").fadeOut();
                                $formEl.find("#btnRejectBookingAnalysis").fadeOut();
                                $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                                $formEl.find("#btnProposeBookingAnalysis").fadeOut();
                                $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeIn();
                                $formEl.find("#divAdditionalBooking").fadeIn();
                            }

                            if (row.TProcessMasterId === 1) {
                                $formEl.find("#divAdditionalBooking").fadeOut();
                                isAdditionalBooking = false;
                                $formEl.find("#divEditBookingAnalysisMaster1").fadeIn();
                                $formEl.find("#divEditBookingAnalysisLab").fadeOut();
                                $formEl.find("#divEditBookingAnalysisDyeing").fadeOut();
                                $formEl.find("#divEditBookingAnalysisFinishing").fadeOut();
                                $formEl.find("#divAdditionalBooking").fadeOut();

                                $formEl.find("#btnSaveBookingAnalysis").fadeIn();
                                //$formEl.find("#btnProposeBookingAnalysis").fadeOut();
                                $formEl.find("#btnBookingAnalysisApprovedBookingAnalysis").fadeOut();
                                $formEl.find("#btnBookingAnalysisAcknowledgeBookingAnalysis").fadeIn();
                                $formEl.find("#btnBookingAnalysisRejectBookingAnalysis").fadeOut();
                                $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();

                                if (bookingAnalysisMaster.BookingType = bookingTypeConstants.SAMPLE) getChildDataSample(row.ProcessStatus);
                                else getChildData();
                            }
                            else if (row.TProcessMasterId === 2) {
                                $formEl.find("#divEditBookingAnalysisMaster1").fadeOut();
                                $formEl.find("#divEditBookingAnalysisLab").fadeIn();
                                $formEl.find("#divEditBookingAnalysisDyeing").fadeOut();
                                $formEl.find("#divEditBookingAnalysisFinishing").fadeOut();
                                $formEl.find("#divButtonExecutionsBookingBI").fadeOut();
                            }
                            else if (row.TProcessMasterId === 3) {
                                $formEl.find("#divEditBookingAnalysisMaster1").fadeOut();
                                $formEl.find("#divEditBookingAnalysisLab").fadeOut();
                                $formEl.find("#divEditBookingAnalysisDyeing").fadeIn();
                                $formEl.find("#divEditBookingAnalysisFinishing").fadeOut();
                                $formEl.find("#divButtonExecutionsBookingBI").fadeOut();
                                initTblFabricBookingAnalysisChildDyeing();
                                getChildDataDyeing(row.ProcessStatus);
                            }
                            else if (row.TProcessMasterId === 4) {
                                $formEl.find("#divEditBookingAnalysisMaster1").fadeOut();
                                $formEl.find("#divEditBookingAnalysisLab").fadeOut();
                                $formEl.find("#divEditBookingAnalysisDyeing").fadeOut();
                                $formEl.find("#divEditBookingAnalysisFinishing").fadeIn();
                                $formEl.find("#divButtonExecutionsBookingBI").fadeOut();
                                initTblFabricBookingAnalysisChildFinishing();
                                getChildDataFinishing(row.ProcessStatus);
                            }
                        }
                    }
                },
                {
                    title: 'Revise',
                    align: 'center',
                    width: 50,
                    visible: isAdditionBookingButton,
                    formatter: function (value, row, index, field) {
                        return [
                            '<span class="btn-group">',
                            '</span>',
                            '<a class="btn btn-xs btn-primary btn-xs m-w-50 revise-booking" href="javascript:void(0)" title="Addiotional">',
                            '<i aria-hidden="true"></i> Revise',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .revise-booking': function (e, value, row, index) {
                            e.preventDefault();

                            showBootboxConfirm("Revise Booking Analysis", "Are you sure you want to revise this?", function (yes) {
                                if (yes) {
                                    $formEl.find("#TProcessMasterID").val(row.TProcessMasterId);

                                    if (status === statusConstants.PENDING) {
                                        $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                        $formEl.find("#btnSaveBookingAnalysis").fadeOut();
                                        $formEl.find("#btnApprovedBookingAnalysis").fadeOut();
                                        $formEl.find("#btnRejectBookingAnalysis").fadeOut();
                                        $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                                        $formEl.find("#btnProposeBookingAnalysis").fadeOut();
                                        $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeOut();
                                        $formEl.find("#divAdditionalBooking").fadeOut();
                                    }
                                    else if (status === statusConstants.PROPOSED) {
                                        $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                        $formEl.find("#btnSaveBookingAnalysis").fadeIn();
                                        $formEl.find("#btnApprovedBookingAnalysis").fadeIn();
                                        $formEl.find("#btnRejectBookingAnalysis").fadeIn();
                                        $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                                        $formEl.find("#btnProposeBookingAnalysis").fadeOut();
                                        $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeOut();
                                        $formEl.find("#divAdditionalBooking").fadeOut();
                                    }
                                    else if (status === statusConstants.APPROVED) {
                                        $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                        $formEl.find("#btnSaveBookingAnalysis").fadeOut();
                                        $formEl.find("#btnApprovedBookingAnalysis").fadeOut();
                                        $formEl.find("#btnRejectBookingAnalysis").fadeOut();
                                        $formEl.find("#btnAcknowledgeBookingAnalysis").fadeIn();
                                        $formEl.find("#btnProposeBookingAnalysis").fadeOut();
                                        $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeOut();
                                        $formEl.find("#divAdditionalBooking").fadeOut();
                                    }
                                    else if (status === statusConstants.ACKNOWLEDGE) {
                                        $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                        $formEl.find("#btnSaveBookingAnalysis").fadeOut();
                                        $formEl.find("#btnApprovedBookingAnalysis").fadeOut();
                                        $formEl.find("#btnRejectBookingAnalysis").fadeOut();
                                        $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                                        $formEl.find("#btnProposeBookingAnalysis").fadeOut();
                                        $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeOut();
                                        $formEl.find("#divAdditionalBooking").fadeOut();
                                    }
                                    else if (status === statusConstants.PARTIALLY_COMPLETED) {
                                        $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                        $formEl.find("#btnSaveBookingAnalysis").fadeIn();
                                        $formEl.find("#btnApprovedBookingAnalysis").fadeOut();
                                        $formEl.find("#btnRejectBookingAnalysis").fadeOut();
                                        $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                                        $formEl.find("#btnProposeBookingAnalysis").fadeIn();
                                        $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeOut();
                                        $formEl.find("#divAdditionalBooking").fadeOut();
                                    }
                                    else if (status === statusConstants.ADDITIONAL) {
                                        $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                        $formEl.find("#btnSaveBookingAnalysis").fadeOut();
                                        $formEl.find("#btnApprovedBookingAnalysis").fadeOut();
                                        $formEl.find("#btnRejectBookingAnalysis").fadeOut();
                                        $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                                        $formEl.find("#btnProposeBookingAnalysis").fadeOut();
                                        $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeIn();
                                        $formEl.find("#divAdditionalBooking").fadeIn();
                                    }

                                    if (row.TProcessMasterId === 1) {
                                        $formEl.find("#divEditBookingAnalysisMaster1").fadeIn();
                                        $formEl.find("#divEditBookingAnalysisLab").fadeOut();
                                        $formEl.find("#divEditBookingAnalysisDyeing").fadeOut();
                                        $formEl.find("#divEditBookingAnalysisFinishing").fadeOut();
                                        //isAdditionalBooking = true;
                                        //getYarnAdditionalBookingReason();

                                        $formEl.find("#BAnalysisNo").val($("#BookingNo").val() + '-BA-Rev-' + $("#AdditionalBookingNew").val());
                                        $formEl.find("BAnalysisNoAdd").val($("#BAnalysisNo").val());

                                        //$formEl.find("#divAdditionalBooking").fadeIn();
                                        $formEl.find("#btnSaveBookingAnalysis").fadeOut();
                                        $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeIn();
                                        $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                                        initTblFabricBookingAnalysisChild();
                                        if (bookingAnalysisMaster.BookingType = bookingTypeConstants.BULK) // Bulk Booking
                                        {
                                            getChildData();
                                            if ($formEl.find("#HasCollar").val() == "Yes") {
                                                $formEl.find("#divEditBookingAnalysisMasterCollar").fadeIn();
                                                getChildCollarData();
                                                initTblFabricBookingAnalysisChildCollar();
                                            }
                                            else $formEl.find("#divEditBookingAnalysisMasterCollar").fadeOut();

                                            if ($formEl.find("#HasCuff").val() == "Yes") {
                                                $formEl.find("#divEditBookingAnalysisMasterCuff").fadeIn();
                                                getChildCuffData();
                                                initTblFabricBookingAnalysisChildCuff();
                                            }
                                            else $formEl.find("#divEditBookingAnalysisMasterCuff").fadeOut();
                                        }
                                        if (bookingAnalysisMaster.BookingType = bookingTypeConstants.SAMPLE) // Sample Booking
                                        {
                                            $("#btnSaveBookingAnalysis").fadeIn();
                                            $("#btnProposeBookingAnalysis").fadeOut();
                                            $("#btnBookingAnalysisApprovedBookingAnalysis").fadeOut();
                                            $("#btnBookingAnalysisAcknowledgeBookingAnalysis").fadeOut();
                                            $("#btnAcknowledgeBookingAnalysis").fadeOut();

                                            getChildDataSample(row.ProcessStatus);
                                        }
                                        if (bookingAnalysisMaster.BookingType = bookingTypeConstants.REVISED) // Revised Booking
                                        {
                                            getChildData(row.ProcessStatus);
                                        }
                                    }
                                    else if (row.TProcessMasterId === 2) {
                                        $formEl.find("#divEditBookingAnalysisMaster1").fadeOut();
                                        $formEl.find("#divEditBookingAnalysisLab").fadeIn();
                                        $formEl.find("#divEditBookingAnalysisDyeing").fadeOut();
                                        $formEl.find("#divEditBookingAnalysisFinishing").fadeOut();
                                        $formEl.find("#divButtonExecutionsBookingBI").fadeOut();
                                        $formEl.find("#divAdditionalBooking").fadeOut();
                                    }
                                    else if (row.TProcessMasterId === 3) {
                                        $formEl.find("#divEditBookingAnalysisMaster1").fadeOut();
                                        $formEl.find("#divEditBookingAnalysisLab").fadeOut();
                                        $formEl.find("#divEditBookingAnalysisDyeing").fadeIn();
                                        $formEl.find("#divEditBookingAnalysisFinishing").fadeOut();
                                        $formEl.find("#divButtonExecutionsBookingBI").fadeOut();
                                        $formEl.find("#divAdditionalBooking").fadeOut();
                                        initTblFabricBookingAnalysisChildDyeing();
                                        getChildDataDyeing(row.ProcessStatus);
                                    }
                                    else if (row.TProcessMasterId === 4) {
                                        $formEl.find("#divEditBookingAnalysisMaster1").fadeOut();
                                        $formEl.find("#divEditBookingAnalysisLab").fadeOut();
                                        $formEl.find("#divEditBookingAnalysisDyeing").fadeOut();
                                        $formEl.find("#divEditBookingAnalysisFinishing").fadeIn();
                                        $formEl.find("#divButtonExecutionsBookingBI").fadeOut();
                                        $formEl.find("#divAdditionalBooking").fadeOut();
                                        initTblFabricBookingAnalysisChildFinishing();
                                        getChildDataFinishing(row.ProcessStatus);
                                    }
                                }
                            });
                        }
                    }
                },
                {
                    title: 'Additional',
                    align: 'center',
                    width: 50,
                    visible: isAdditionBookingButton,
                    formatter: function (value, row, index, field) {
                        return [
                            '<span class="btn-group">',
                            '</span>',
                            '<a class="btn btn-xs btn-success btn-xs m-w-50 add-booking" href="javascript:void(0)" title="Addiotional Booking">',
                            '<i class="fa fa-plus" aria-hidden="true"></i> New',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .add-booking': function (e, value, row, index) {
                            e.preventDefault();
                            $formEl.find("#TProcessMasterID").val(row.TProcessMasterId);

                            if (status === statusConstants.PENDING) {
                                $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                $formEl.find("#btnSaveBookingAnalysis").fadeOut();
                                $formEl.find("#btnApprovedBookingAnalysis").fadeOut();
                                $formEl.find("#btnRejectBookingAnalysis").fadeOut();
                                $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                                $formEl.find("#btnProposeBookingAnalysis").fadeOut();
                                $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeOut();
                                $formEl.find("#divAdditionalBooking").fadeOut();
                            }
                            if (status === statusConstants.PROPOSED) {
                                $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                $formEl.find("#btnSaveBookingAnalysis").fadeIn();
                                $formEl.find("#btnApprovedBookingAnalysis").fadeIn();
                                $formEl.find("#btnRejectBookingAnalysis").fadeIn();
                                $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                                $formEl.find("#btnProposeBookingAnalysis").fadeOut();
                                $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeOut();
                                $formEl.find("#divAdditionalBooking").fadeOut();
                            }
                            if (status === statusConstants.APPROVED) {
                                $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                $formEl.find("#btnSaveBookingAnalysis").fadeOut();
                                $formEl.find("#btnApprovedBookingAnalysis").fadeOut();
                                $formEl.find("#btnRejectBookingAnalysis").fadeOut();
                                $formEl.find("#btnAcknowledgeBookingAnalysis").fadeIn();
                                $formEl.find("#btnProposeBookingAnalysis").fadeOut();
                                $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeOut();
                                $formEl.find("#divAdditionalBooking").fadeOut();
                            }
                            if (status === statusConstants.ACKNOWLEDGE) {
                                $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                $formEl.find("#btnSaveBookingAnalysis").fadeOut();
                                $formEl.find("#btnApprovedBookingAnalysis").fadeOut();
                                $formEl.find("#btnRejectBookingAnalysis").fadeOut();
                                $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                                $formEl.find("#btnProposeBookingAnalysis").fadeOut();
                                $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeOut();
                                $formEl.find("#divAdditionalBooking").fadeOut();
                            }
                            if (status === statusConstants.PARTIALLY_COMPLETED) {
                                $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                $formEl.find("#btnSaveBookingAnalysis").fadeIn();
                                $formEl.find("#btnApprovedBookingAnalysis").fadeOut();
                                $formEl.find("#btnRejectBookingAnalysis").fadeOut();
                                $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                                $formEl.find("#btnProposeBookingAnalysis").fadeIn();
                                $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeOut();
                                $formEl.find("#divAdditionalBooking").fadeOut();
                            }
                            if (status === statusConstants.ADDITIONAL) {
                                $formEl.find("#btnEditCancelBookingAnalysis").fadeIn();
                                $formEl.find("#btnSaveBookingAnalysis").fadeOut();
                                $formEl.find("#btnApprovedBookingAnalysis").fadeOut();
                                $formEl.find("#btnRejectBookingAnalysis").fadeOut();
                                $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                                $formEl.find("#btnProposeBookingAnalysis").fadeOut();
                                $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeIn();
                                $formEl.find("#divAdditionalBooking").fadeIn();
                            }

                            if (row.TProcessMasterId === 1) {
                                $formEl.find("#divEditBookingAnalysisMaster1").fadeIn();
                                $formEl.find("#divEditBookingAnalysisLab").fadeOut();
                                $formEl.find("#divEditBookingAnalysisDyeing").fadeOut();
                                $formEl.find("#divEditBookingAnalysisFinishing").fadeOut();
                                isAdditionalBooking = true;
                                getYarnAdditionalBookingReason();

                                $formEl.find("#BAnalysisNo").val($("#BookingNo").val() + '-BA-Add-' + $("#AdditionalBookingNew").val());
                                $formEl.find("BAnalysisNoAdd").val($("#BAnalysisNo").val());

                                $formEl.find("#divAdditionalBooking").fadeIn();
                                $formEl.find("#btnSaveBookingAnalysis").fadeOut();
                                $formEl.find("#btnSaveBookingAnalysisAdditionalBooking").fadeIn();
                                $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();

                                if (bookingAnalysisMaster.BookingType = bookingTypeConstants.BULK) // Bulk Booking
                                {
                                    getChildData();
                                }
                                else if (bookingAnalysisMaster.BookingType = bookingTypeConstants.SAMPLE) // Sample Booking
                                {
                                    $("#btnSaveBookingAnalysis").fadeIn();
                                    $("#btnProposeBookingAnalysis").fadeOut();
                                    $("#btnBookingAnalysisApprovedBookingAnalysis").fadeOut();
                                    $("#btnBookingAnalysisAcknowledgeBookingAnalysis").fadeOut();
                                    $("#btnAcknowledgeBookingAnalysis").fadeOut();

                                    getChildDataSample(row.ProcessStatus);
                                }
                                else if (bookingAnalysisMaster.BookingType = bookingTypeConstants.REVISED) // Revised Booking
                                {
                                    getChildData(row.ProcessStatus);
                                }
                            }
                            else if (row.TProcessMasterId === 2) {
                                $formEl.find("#divEditBookingAnalysisMaster1").fadeOut();
                                $formEl.find("#divEditBookingAnalysisLab").fadeIn();
                                $formEl.find("#divEditBookingAnalysisDyeing").fadeOut();
                                $formEl.find("#divEditBookingAnalysisFinishing").fadeOut();
                                $formEl.find("#divButtonExecutionsBookingBI").fadeOut();
                                $formEl.find("#divAdditionalBooking").fadeOut();
                            }
                            else if (row.TProcessMasterId === 3) {
                                $formEl.find("#divEditBookingAnalysisMaster1").fadeOut();
                                $formEl.find("#divEditBookingAnalysisLab").fadeOut();
                                $formEl.find("#divEditBookingAnalysisDyeing").fadeIn();
                                $formEl.find("#divEditBookingAnalysisFinishing").fadeOut();
                                $formEl.find("#divButtonExecutionsBookingBI").fadeOut();
                                $formEl.find("#divAdditionalBooking").fadeOut();
                                initTblFabricBookingAnalysisChildDyeing();
                                getChildDataDyeing(row.ProcessStatus);
                            }
                            else if (row.TProcessMasterId === 4) {
                                $formEl.find("#divEditBookingAnalysisMaster1").fadeOut();
                                $formEl.find("#divEditBookingAnalysisLab").fadeOut();
                                $formEl.find("#divEditBookingAnalysisDyeing").fadeOut();
                                $formEl.find("#divEditBookingAnalysisFinishing").fadeIn();
                                $formEl.find("#divButtonExecutionsBookingBI").fadeOut();
                                $formEl.find("#divAdditionalBooking").fadeOut();
                                initTblFabricBookingAnalysisChildFinishing();
                                getChildDataFinishing(row.ProcessStatus);
                            }
                        }
                    }
                },
                {
                    field: "DisplayName",
                    title: "Process Name",
                    filterControl: "input",
                },
                {
                    field: "ProcessStatus",
                    title: "Process Status",
                    filterControl: "input"
                }
            ]
        });
    }

    // #region Child Tables
    function initTblFabricBookingAnalysisChild() {
        $tblBookingAnalysisChildFabric.bootstrapTable('destroy').bootstrapTable({
            detailView: true,
            uniqueId: 'Id',
            //scrolling: true,
            //height: 300,
            columns: [
                {
                    title: 'Actions',
                    align: 'center',
                    width: 100,
                    visible: isAdditionalBooking,
                    formatter: function () {
                        return [
                            '<span class="btn-group">',
                            '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete Item">',
                            '<i class="fa fa-remove"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            this.data[index].EntityState = 8;
                            var $target = $(e.target);
                            $target.closest("tr").addClass('deleted-row');
                        }
                    }
                },
                {
                    field: "Segment1ValueDesc",
                    title: "Construction",
                    filterControl: "input",
                },
                {
                    field: "Segment2ValueDesc",
                    title: "Composition",
                    filterControl: "input"
                },
                {
                    field: "Segment3ValueDesc",
                    title: "Fabric Color",
                    filterControl: "input"
                },
                {
                    field: "TechnicalNameId",
                    title: "Technical Name",
                    editable: {
                        source: bookingAnalysisMaster.FabricTechnicalNameList,
                        type: 'select2',
                        showbuttons: false,
                        select2: { width: 100, placeholder: 'Technical Name' }
                    },
                    visible: !isCustomChecked
                },
                {
                    field: "ShadeId",
                    title: "Shade Name",
                    editable: {
                        type: 'select',
                        title: 'Select a Shade',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: bookingAnalysisMaster.ShadeList
                    },
                    visible: !isCustomChecked
                },
                {
                    field: "Segment4ValueDesc",
                    title: "GSM",
                    align: "center",
                    filterControl: "input"
                },
                {
                    field: "Segment5ValueDesc",
                    title: "Width",
                    align: "center",
                    filterControl: "input",
                    width: 50
                },
                {
                    field: "Segment6ValueDesc",
                    title: "Knitting Type",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "Segment7ValueDesc",
                    title: "Dyeing Type",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "Finishing",
                    title: "Finishing",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "Washing",
                    title: "Washing",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "YarnType",
                    title: "Yarn Type",
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
                    field: "Remarks",
                    title: "Instruction",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "AOP",
                    title: "AOP",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "UsesIn",
                    title: "Uses In",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "BookingQty",
                    title: "Booking Qty",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "DisplayUnitDesc",
                    title: "Booking UOM",
                    align: "left",
                    filterControl: "input"
                }
            ],
            onExpandRow: function (index, row, $detail) {
                bookingChildId = row.BookingChildId;
                constructionId = row.Segment1ValueId;
                yComposition = row.Segment2ValueId;
                fabricGsm = row.Segment4ValueId;
                fabricWidth = row.Segment5ValueId;
                dyeingtype = row.Segment6ValueId;
                knittingtype = row.Segment7ValueId;
                shadeId = row.ShadeId;
                shadeName = row.ShadeName;
                TechnicalNameId = row.TechnicalNameId;
                yarnType = row.YarnTypeId;
                buyerId = $formEl.find("#BuyerId").val();
                yProgram = row.YarnProgram;
                bookingQty = row.BookingQty;
                populateBookingAnalysisChildYarn(row.Id, row.TechnicalNameId, row.FabricComposition, $detail);
            },
            onEditableSave: function (field, row, oldValue, $el) {
                if (!row.ShadeId && row.TechnicalNameId) {
                    var matchingChilds = bookingAnalysisMaster.Childs.filter(function (el) {
                        return el.Segment1ValueId === row.Segment1ValueId && el.Segment2ValueId === row.Segment2ValueId;
                    });

                    bootbox.confirm({
                        size: "small",
                        message: "Do you want to apply this Technical Name for similar items?",
                        buttons: {
                            confirm: {
                                label: 'Yes',
                                className: 'btn-success'
                            },
                            cancel: {
                                label: 'No',
                                className: 'btn-danger'
                            }
                        },
                        callback: function (yes) {
                            if (yes) {
                                $.each(matchingChilds, function (i, item) {
                                    item.TechnicalNameId = row.TechnicalNameId;
                                    item.TechnicalName = bookingAnalysisMaster.FabricTechnicalNameList.find(function (el) {
                                        return el.value == row.TechnicalNameId;
                                    }).text;
                                    TechnicalName = item.TechnicalName;
                                    $tblBookingAnalysisChildFabric.bootstrapTable('updateByUniqueId', item.Id, item);
                                });
                            }
                            else {
                                $.each(matchingChilds, function (i, item) {
                                    if (!item.shadeId)
                                        return;

                                    item.technicalNameId = row.technicalNameId;
                                    item.TechnicalName = bookingAnalysisMaster.FabricTechnicalNameList.find(function (el) {
                                        return el.value == row.ShadeId;
                                    }).text;
                                    TechnicalName = item.TechnicalName;
                                    $tblBookingAnalysisChildFabric.bootstrapTable('updateByUniqueId', item.Id, item);
                                });
                            }
                        }
                    });
                }
                if (row.ShadeId && row.TechnicalNameId) {
                    var matchingChilds = bookingAnalysisMaster.Childs.filter(function (el) {
                        return el.Segment3ValueId === row.Segment3ValueId;
                    });

                    bootbox.confirm({
                        size: "small",
                        message: "Do you want to apply this shade for similar items?",
                        buttons: {
                            confirm: {
                                label: 'Yes',
                                className: 'btn-success'
                            },
                            cancel: {
                                label: 'No',
                                className: 'btn-danger'
                            }
                        },
                        callback: function (yes) {
                            var selectedShade = bookingAnalysisMaster.ShadeList.find(function (el) {
                                return el.value == row.ShadeId;
                            });

                            if (yes) {
                                $.each(matchingChilds, function (i, item) {
                                    item.ShadeId = row.ShadeId;
                                    item.ShadeName = bookingAnalysisMaster.ShadeList.find(function (el) {
                                        return el.value == row.ShadeId;
                                    }).text;
                                    shadeName = item.ShadeName;
                                    $tblBookingAnalysisChildFabric.bootstrapTable('updateByUniqueId', item.Id, item);
                                });
                            }
                            else {
                                $.each(matchingChilds, function (i, item) {
                                    if (!item.shadeId)
                                        return;

                                    item.ShadeId = row.ShadeId;
                                    item.ShadeName = bookingAnalysisMaster.ShadeList.find(function (el) {
                                        return el.value == row.ShadeId;
                                    }).text;
                                    shadeName = item.ShadeName;
                                    $tblBookingAnalysisChildFabric.bootstrapTable('updateByUniqueId', item.Id, item);
                                });
                            }
                        }
                    });
                }
            }
        });
    }

    function initTblFabricBookingAnalysisChildCollar() {
        $tblBookingAnalysisChildCollar.bootstrapTable('destroy').bootstrapTable({
            detailView: true,
            uniqueId: 'Id',
            columns: [
                {
                    title: 'Actions',
                    align: 'center',
                    width: 100,
                    visible: isAdditionalBooking,
                    formatter: function () {
                        return [
                            '<span class="btn-group">',
                            '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete Item">',
                            '<i class="fa fa-remove"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            this.data[index].EntityState = 8;
                            var $target = $(e.target);
                            $target.closest("tr").addClass('deleted-row');
                        }
                    }
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
                    field: "BodyColor",
                    title: "Body Color",
                    filterControl: "input"
                },
                {
                    field: "BodyMeasurement",
                    title: "Collar Measurement",
                    align: "center",
                    filterControl: "input"
                },
                {
                    field: "YarnType",
                    title: "Yarn Type",
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
                    field: "Remarks",
                    title: "Instruction",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "BookingQty",
                    title: "Booking Qty",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "DisplayUnitDesc",
                    title: "Booking UOM",
                    align: "left",
                    filterControl: "input"
                }
            ],
            onExpandRow: function (index, row, $detail) {
                bookingChildId = row.BookingChildId;
                constructionId = row.ConstructionId;
                fabricGsm = row.FabricGsm;
                fabricWidth = row.FabricWidth;
                shadeId = row.ShadeId;
                shadeName = row.ShadeName;
                TechnicalNameId = row.TechnicalNameId;
                dyeingtype = row.DyeingTypeId;
                knittingtype = row.KnittingTypeId;
                yarnType = row.YarnTypeId;
                buyerId = $formEl.find("#BuyerId").val();
                yComposition = row.FabricComposition;
                yProgram = row.YarnProgram;
                bookingQty = row.BookingQty;

                populateBookingAnalysisChildYarnCollar(row.Id, row.FabricComposition, $detail);
            }
        });
    }

    function initTblFabricBookingAnalysisChildCuff() {
        $tblBookingAnalysisChildCuff.bootstrapTable('destroy').bootstrapTable({
            detailView: true,
            uniqueId: 'Id',
            columns: [
                {
                    title: 'Actions',
                    align: 'center',
                    width: 100,
                    visible: isAdditionalBooking,
                    formatter: function () {
                        return [
                            '<span class="btn-group">',
                            '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete Item">',
                            '<i class="fa fa-remove"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            this.data[index].EntityState = 8;
                            var $target = $(e.target);
                            $target.closest("tr").addClass('deleted-row');
                        }
                    }
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
                    field: "BodyColor",
                    title: "Body Color",
                    filterControl: "input"
                },
                {
                    field: "BodyMeasurement",
                    title: "Cuff Measurement",
                    align: "center",
                    filterControl: "input"
                },
                {
                    field: "YarnType",
                    title: "Yarn Type",
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
                    field: "Remarks",
                    title: "Instruction",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "BookingQty",
                    title: "Booking Qty",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "DisplayUnitDesc",
                    title: "Booking UOM",
                    align: "left",
                    filterControl: "input"
                }
            ],
            onExpandRow: function (index, row, $detail) {
                bookingChildId = row.BookingChildId;
                constructionId = row.ConstructionId;
                fabricGsm = row.FabricGsm;
                fabricWidth = row.FabricWidth;
                shadeId = row.ShadeId;
                shadeName = row.ShadeName;
                TechnicalNameId = row.TechnicalNameId;
                dyeingtype = row.DyeingTypeId;
                knittingtype = row.KnittingTypeId;
                yarnType = row.YarnTypeId;
                buyerId = $formEl.find("#BuyerId").val();
                yComposition = row.FabricComposition;
                yProgram = row.YarnProgram;
                bookingQty = row.BookingQty;

                populateBookingAnalysisChildYarnCuff(row.Id, row.TechnicalNameId, row.FabricComposition, $detail);
            }
        });
    }
    // #endregion

    function initTblFabricBookingAnalysisChildDyeing() {
        $formEl.find("#tblFabricBookingAnalysisChildDyeing").bootstrapTable('destroy').bootstrapTable({
            uniqueId: 'Id',
            columns: [
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
                    field: "DyeingType",
                    title: "Dyeing Type",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "DyeingMcNameId",
                    title: "Dyeing MC Type",
                    editable: {
                        source: bookingAnalysisMaster.DyeingMCNameList,
                        type: 'select2',
                        showbuttons: false,
                        select2: { width: 100, placeholder: 'Dyeing MC Name' }
                    }
                },
                {
                    field: "Remarks",
                    title: "Instruction",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "AOP",
                    title: "AOP",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "UsesIn",
                    title: "Uses In",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "BookingQty",
                    title: "Booking Qty",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "DisplayUnitDesc",
                    title: "Booking UOM",
                    align: "left",
                    filterControl: "input"
                }
            ]
        });
    }

    function initTblFabricBookingAnalysisChildFinishing() {
        $formEl.find("#tblFabricBookingAnalysisChildFinishing").bootstrapTable('destroy').bootstrapTable({
            columns: [
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
                    field: "Finishing",
                    title: "Finishing",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "FinishingMcNameId",
                    title: "Finishing MC Type",
                    editable: {
                        source: bookingAnalysisMaster.FinishingMCNameList,
                        type: 'select2',
                        showbuttons: false,
                        select2: { width: 100, placeholder: 'Finishing MC Name', allowclear: true }
                    }
                },
                {
                    field: "Remarks",
                    title: "Instruction",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "AOP",
                    title: "AOP",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "UsesIn",
                    title: "Uses In",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "BookingQty",
                    title: "Booking Qty",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "DisplayUnitDesc",
                    title: "Booking UOM",
                    align: "left",
                    filterControl: "input"
                }
            ]
        });
    }

    function initTblYarnComposition() {
        $("#tblYarnComposition").bootstrapTable({
            pagination: true,
            filterControl: true,
            sidePagination: "server",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            columns: [
                {
                    field: "YarnCompositionId",
                    title: "Yarn Composition Id",
                    filterControl: "input",
                    visible: false
                },
                {
                    field: "YarnCompositionName",
                    title: "Yarn Composition",
                    filterControl: "input"
                }
            ],
            onDblClickRow: function (row, $element, field) {
                yComposition = row.YarnCompositionName;
                if (bookingAnalysisMaster.BookingType = bookingTypeConstants.BULK) // 1 for Bulk Fabric
                {
                    var url = "/api/newbookinganalysischildyarnPopUp?bookingChildId=" + bookingChildId + "&constructionId=" + constructionId
                        + "&fabricGsm=" + fabricGsm + "&fabricWidth=" + fabricWidth + "&shadeId=" + shadeId + "&yarnCompositionName=" + row.YarnCompositionName
                        + "&fabTechnicalName=" + TechnicalNameId + "&dyeingtype=" + dyeingtype + "&buyerId=" + buyerId + "&knittingtype=" + knittingtype + "&yarnProgram=" + yProgram;
                }
                if (bookingAnalysisMaster.BookingType = bookingTypeConstants.SAMPLE) // 0 for Sample Fabric
                {
                    var url = "/api/newbookinganalysischildyarnSample?bookingChildId=" + bookingChildId + "&constructionId=" + constructionId
                        + "&fabricGsm=" + fabricGsm + "&fabricWidth=" + fabricWidth + "&shadeId=" + shadeId + "&yarnCompositionName=" + row.YarnCompositionName
                        + "&fabTechnicalName=" + TechnicalNameId + "&dyeingtype=" + dyeingtype + "&buyerId=" + buyerId + "&knittingtype=" + knittingtype + "&yarnProgram=" + yProgram;
                }

                var bookingAnalysisChild = bookingAnalysisMaster.Childs.find(function (el) {
                    return el.BookingChildId == bookingChildId;
                });
                axios.get(url)
                    .then(function (response) {
                        response.data[0].Id = getMaxIdForArray(bookingAnalysisMaster.Childs, "Id");
                        bookingAnalysisChild.ChildYarns.push(response.data[0]);
                        $(childEl).bootstrapTable('load', bookingAnalysisChild.ChildYarns);

                        $("#modal-child").modal('hide');
                    })
                    .catch(function () {
                        toastr.error(constants.LOAD_ERROR_MESSAGE);
                    })
            },
            onPageChange: function (number, size) {
                var newOffset = (number - 1) * size;
                var newLimit = size;
                if (yarnCompositionTableParams.offset == newOffset && yarnCompositionTableParams.limit == newLimit)
                    return;

                yarnCompositionTableParams.offset = newOffset;
                yarnCompositionTableParams.limit = newLimit;

                getFabricCompositionLists();
            },
            onSort: function (name, order) {
                yarnCompositionTableParams.sort = name;
                yarnCompositionTableParams.order = order;
                yarnCompositionTableParams.offset = 0;

                getFabricCompositionLists();
            },
            onRefresh: function () {
                yarnCompositionTableParams.offset = 0;
                yarnCompositionTableParams.limit = 10;
                yarnCompositionTableParams.sort = '';
                yarnCompositionTableParams.order = '';
                yarnCompositionTableParams.filter = '';

                getFabricCompositionLists();
            },
            onColumnSearch: function (columnName, filterValue) {
                if (columnName in yearnCompositionfilterBy && !filterValue) {
                    delete yearnCompositionfilterBy[columnName];
                }
                else
                    yearnCompositionfilterBy[columnName] = filterValue;

                if (Object.keys(yearnCompositionfilterBy).length === 0 && yearnCompositionfilterBy.constructor === Object)
                    yarnCompositionTableParams.filter = "";
                else
                    yarnCompositionTableParams.filter = JSON.stringify(yearnCompositionfilterBy);

                getFabricCompositionLists();
            }
        });
    }

    function initTblYarnCompositionCollar() {
        $("#tblYarnCompositionCollar").bootstrapTable({
            pagination: true,
            filterControl: true,
            sidePagination: "server",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            columns: [
                {
                    field: "YarnCompositionId",
                    title: "Yarn Composition Id",
                    filterControl: "input",
                    visible: false
                },
                {
                    field: "YarnCompositionName",
                    title: "Yarn Composition",
                    filterControl: "input"
                }
            ],
            onDblClickRow: function (row, $element, field) {
                yComposition = row.YarnCompositionName;

                url = "/api/newbookinganalysischildyarnCollarCuff?bookingChildId=" + bookingChildId + "&constructionId=" + constructionId + "&yarnCompositionName=" + yComposition + "&buyerId=" + buyerId + "&yarnProgram=" + yProgram;

                var bookingAnalysisChildCollar = bookingAnalysisChildCollarList.find(function (el) {
                    return el.BookingChildId == bookingChildId;
                });
                axios.get(url)
                    .then(function (response) {
                        response.data[0].Id = getMaxIdForArray(bookingAnalysisChildCollarList, "Id");
                        bookingAnalysisChildCollar.ChildYarns.push(response.data[0]);
                        $(childEl).bootstrapTable('load', bookingAnalysisChildCollar.ChildYarns);

                        $("#modal-child-Collar").modal('hide');
                    })
                    .catch(function () {
                        toastr.error(constants.LOAD_ERROR_MESSAGE);
                    })
            },
            onPageChange: function (number, size) {
                var newOffset = (number - 1) * size;
                var newLimit = size;
                if (yarnCompositionTableParams.offset == newOffset && yarnCompositionTableParams.limit == newLimit)
                    return;

                yarnCompositionTableParams.offset = newOffset;
                yarnCompositionTableParams.limit = newLimit;

                getFabricCompositionLists();
            },
            onSort: function (name, order) {
                yarnCompositionTableParams.sort = name;
                yarnCompositionTableParams.order = order;
                yarnCompositionTableParams.offset = 0;

                getFabricCompositionLists();
            },
            onRefresh: function () {
                yarnCompositionTableParams.offset = 0;
                yarnCompositionTableParams.limit = 10;
                yarnCompositionTableParams.sort = '';
                yarnCompositionTableParams.order = '';
                yarnCompositionTableParams.filter = '';

                getFabricCompositionLists();
            },
            onColumnSearch: function (columnName, filterValue) {
                if (columnName in yearnCompositionfilterBy && !filterValue) {
                    delete yearnCompositionfilterBy[columnName];
                }
                else
                    yearnCompositionfilterBy[columnName] = filterValue;

                if (Object.keys(yearnCompositionfilterBy).length === 0 && yearnCompositionfilterBy.constructor === Object)
                    yarnCompositionTableParams.filter = "";
                else
                    yarnCompositionTableParams.filter = JSON.stringify(yearnCompositionfilterBy);

                getFabricCompositionLists();
            }
        });
    }

    function initTblYarnCompositionCuff() {
        $("#tblYarnCompositionCuff").bootstrapTable({
            pagination: true,
            filterControl: true,
            sidePagination: "server",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            columns: [
                {
                    field: "YarnCompositionId",
                    title: "Yarn Composition Id",
                    filterControl: "input",
                    visible: false
                },
                {
                    field: "YarnCompositionName",
                    title: "Yarn Composition",
                    filterControl: "input"
                }
            ],
            onDblClickRow: function (row, $element, field) {
                yComposition = row.YarnCompositionName;

                url = "/api/newbookinganalysischildyarnCollarCuff?bookingChildId=" + bookingChildId + "&constructionId=" + constructionId + "&yarnCompositionName=" + yComposition + "&buyerId=" + buyerId + "&yarnProgram=" + yProgram;

                var bookingAnalysisChildCuff = bookingAnalysisChildCuffList.find(function (el) {
                    return el.BookingChildId == bookingChildId;
                });
                axios.get(url)
                    .then(function (response) {
                        response.data[0].Id = getMaxIdForArray(bookingAnalysisChildCuffList, "Id");
                        bookingAnalysisChildCuff.ChildYarns.push(response.data[0]);
                        $(childEl).bootstrapTable('load', bookingAnalysisChildCuff.ChildYarns);

                        $("#modal-child-Cuff").modal('hide');
                    })
                    .catch(function () {
                        toastr.error(constants.LOAD_ERROR_MESSAGE);
                    })
            },
            onPageChange: function (number, size) {
                var newOffset = (number - 1) * size;
                var newLimit = size;
                if (yarnCompositionTableParams.offset == newOffset && yarnCompositionTableParams.limit == newLimit)
                    return;

                yarnCompositionTableParams.offset = newOffset;
                yarnCompositionTableParams.limit = newLimit;

                getFabricCompositionLists();
            },
            onSort: function (name, order) {
                yarnCompositionTableParams.sort = name;
                yarnCompositionTableParams.order = order;
                yarnCompositionTableParams.offset = 0;

                getFabricCompositionLists();
            },
            onRefresh: function () {
                yarnCompositionTableParams.offset = 0;
                yarnCompositionTableParams.limit = 10;
                yarnCompositionTableParams.sort = '';
                yarnCompositionTableParams.order = '';
                yarnCompositionTableParams.filter = '';

                getFabricCompositionLists();
            },
            onColumnSearch: function (columnName, filterValue) {
                if (columnName in yearnCompositionfilterBy && !filterValue) {
                    delete yearnCompositionfilterBy[columnName];
                }
                else
                    yearnCompositionfilterBy[columnName] = filterValue;

                if (Object.keys(yearnCompositionfilterBy).length === 0 && yearnCompositionfilterBy.constructor === Object)
                    yarnCompositionTableParams.filter = "";
                else
                    yarnCompositionTableParams.filter = JSON.stringify(yearnCompositionfilterBy);

                getFabricCompositionLists();
            }
        });
    }

    function initBookingAnalysisChildYarn($tableEl, bookingChildId, data) {
        $tableEl.bootstrapTable({
            showFooter: true,
            data: data,
            uniqueId: 'Id',
            rowStyle: function (row, index) {
                if (row.EntityState == 8)
                    return { classes: 'deleted-row' };

                return "";
            },
            columns: [
                {
                    title: 'Actions',
                    align: 'center',
                    width: 100,
                    formatter: function () {
                        return [
                            '<span class="btn-group">',
                            '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete Item">',
                            '<i class="fa fa-remove"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    footerFormatter: function () {
                        return [
                            '<span class="btn-group">',
                            '<button class="btn btn-success btn-xs edit" onclick="return addNewChildRowBA(event, ' + bookingChildId + ')" title="Add">',
                            '<i class="fa fa-plus"></i>',
                            ' Add',
                            '</button>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            this.data[index].EntityState = 8;
                            var $target = $(e.target);
                            $target.closest("tr").addClass('deleted-row');
                        }
                    }
                    //events: {
                    //    'click .remove': function (e, value, row, index) {
                    //        e.preventDefault();
                    //        $tableEl.bootstrapTable('remove', { field: 'Id', values: [row.Id] });
                    //    },
                    //}
                },
                {
                    field: "YarnTypeId",
                    title: "Yarn Type",
                    align: "left",
                    editable: {
                        source: bookingAnalysisMaster.YarnTypeList,
                        type: 'select2',
                        showbuttons: false,
                        select2: { width: 100, placeholder: 'Yarn Type' }
                    }
                },
                {
                    field: "YarnCompositionId",
                    title: "Yarn Composition",
                    width: 100,
                    editable: {
                        type: 'select2',
                        title: 'Select a Yarn Composition',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: bookingAnalysisMaster.YarnCompositionList,
                        select2: { width: 200, placeholder: 'Yarn Composition' }
                    }
                },
                {
                    field: "YarnColorId",
                    title: "Yarn Color",
                    width: 80,
                    editable: {
                        type: 'select2',
                        title: 'Select a Color',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: bookingAnalysisMaster.YarnColorList,
                        select2: { width: 100, placeholder: 'Yarn Color' }
                    }
                },
                {
                    field: "YarnSubProgramIds",
                    title: "Yarn Sub Program",
                    editable: {
                        type: 'select2',
                        showbuttons: true,
                        source: bookingAnalysisMaster.YarnSubProgramList,
                        select2: { width: 200, height: 300, multiple: true, placeholder: 'Select Sub Program' }
                    }
                },
                {
                    field: "YarnCategory",
                    title: "Yarn Category",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "YarnSpecification",
                    title: "Yarn Specification",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "YarnShadeCode",
                    title: "Shade Code",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "IsYD",
                    title: "YD",
                    placeholder: 'YD',
                    formatter: function (value, row, index, field) {
                        return row.IsYD ? '<input type="checkbox" class="check" checked >' : '<input type="checkbox" class="check">';
                    },
                    events: {
                        'click .check': function (e, value, row, index) {
                            if (e.target.checked == true) {
                                FinalYarnCategory = row.YarnCategory;
                                row.YarnCategory = row.YarnCategory + "-YD";
                                row.IsYD = e.target.checked;
                            }
                            else {
                                row.YarnCategory = FinalYarnCategory;
                                row.IsYD = e.target.checked;
                            }

                            $tableEl.bootstrapTable('updateByUniqueId', row.Id, row);
                        }
                    }
                },
                {
                    field: "CalcYarnCount",
                    title: "Suggested YC",
                    width: 20,
                    visible: !isCustomCheckedYarnCount
                },
                {
                    field: "FinalYarnCount",
                    title: "Production YC",
                    width: 20,
                    formatter: function (value, row, index, field) {
                        return ['<span class="btn-group">',
                            '<a href="javascript:void(0)" class="editable-link edit">' + row.Segment2ValueDesc + '</a>',
                            '</span>'].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            if (!row.YarnTypeId) return toastr.error("Yarn Type is not selected");
                            var url = "/api/selectoption/constructionWiseYarnCountLists/" + row.YarnTypeId + "/" + constructionId + "/" + fabricGsm;
                            axios.get(url)
                                .then(function (response) {
                                    var yarnCountList = convertToSelectOptions(response.data);
                                    showBootboxSelectPrompt("Select Yarn Count", yarnCountList, "", function (result) {
                                        if (!result)
                                            return toastr.warning("You didn't selected any Yarn Count.");

                                        var selectedYarnCount = yarnCountList.find(function (el) { return el.value === result })
                                        row.FinalYarnCount = result;
                                        row.Segment2ValueDesc = selectedYarnCount.text;

                                        //yComposition = row.YarnComposition;
                                        if (row.YarnCompositionId) {
                                            yComposition = bookingAnalysisMaster.YarnCompositionList.find(function (el) { return el.id == row.YarnCompositionId }).text;
                                        }
                                        if (row.YarnSubProgramIds) {
                                            var selectedValue = setMultiSelectValueByValueInBootstrapTableEditable(bookingAnalysisMaster.YarnSubProgramList, row.YarnSubProgramIds);
                                            row.YarnSubProgramIds = selectedValue.id;

                                            yarnCount = row.Segment2ValueDesc;
                                            row.YarnCategory = (yarnCount ? yarnCount : "") + "-" + selectedValue.text;
                                            if (row.YarnTypeId) {
                                                yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                                            }
                                            if (row.YarnColorId) {
                                                yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                                            }

                                            yProgram = yProgram;
                                            row.Segment1ValueDesc = yType;
                                            row.Segment2ValueDesc = yarnCount;
                                            row.Segment3ValueDesc = yComposition;
                                            row.Segment4ValueDesc = shadeName;
                                            row.Segment5ValueDesc = yColor;

                                            row.Segment1ValueId = row.YarnTypeId;
                                            row.Segment3ValueId = row.YarnCompositionId;
                                            row.Segment5ValueId = row.YarnColorId;

                                            var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yColor, yProgram, yType, ySubProgram);
                                            YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                                            YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                                            YCategory = YCategory.replaceAll('--', '-');
                                            row.YarnCategory = YCategory + "-" + selectedValue.text;
                                            row.ProductionYarnCategory = row.YarnCategory;
                                        }
                                        else {
                                            if (row.FinalYarnCount) {
                                                yarnCount = row.Segment2ValueDesc;
                                                if (row.YarnTypeId) {
                                                    yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                                                }
                                                if (row.YarnColorId) {
                                                    yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                                                }

                                                yProgram = yProgram;
                                                row.Segment1ValueDesc = yType;
                                                row.Segment2ValueDesc = yarnCount;
                                                row.Segment3ValueDesc = yComposition;
                                                row.Segment4ValueDesc = shadeName;
                                                row.Segment5ValueDesc = yColor;

                                                row.Segment1ValueId = row.YarnTypeId;
                                                row.Segment3ValueId = row.YarnCompositionId;
                                                row.Segment5ValueId = row.YarnColorId;

                                                var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yColor, yProgram, yType);
                                                YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                                                YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                                                YCategory = YCategory.replaceAll('--', '-');
                                                row.YarnCategory = YCategory;
                                                row.ProductionYarnCategory = YCategory;
                                            }
                                            else {
                                                yarnCount = row.CalcYarnCount;
                                                if (row.YarnTypeId) {
                                                    yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                                                }
                                                if (row.YarnColorId) {
                                                    yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                                                }

                                                yProgram = yProgram;
                                                row.Segment1ValueDesc = yType;
                                                row.Segment2ValueDesc = yarnCount;
                                                row.Segment3ValueDesc = yComposition;
                                                row.Segment4ValueDesc = shadeName;
                                                row.Segment5ValueDesc = yColor;

                                                row.Segment1ValueId = row.YarnTypeId;
                                                row.Segment3ValueId = row.YarnCompositionId;
                                                row.Segment5ValueId = row.YarnColorId;

                                                var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yColor, yProgram, yType);
                                                YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                                                YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                                                YCategory = YCategory.replaceAll('--', '-');
                                                row.YarnCategory = YCategory;
                                                row.ProductionYarnCategory = YCategory;
                                            }
                                        }

                                        row.NetConsumption = ((row.BookingQty * row.YarnDistribution) / 100).toFixed(4);
                                        if (row.FinalAllowancePer == "0") {
                                            row.RequiredYarnQty = row.NetConsumption;
                                        }
                                        else {
                                            row.RequiredYarnQty = (((row.BookingQty * row.YarnDistribution) / 100) + ((row.FinalAllowancePer * row.NetConsumption) / 100)).toFixed(4);
                                        }

                                        var url = "";
                                        if (row.FinalYarnCount) {
                                            row.YarnCountId = row.FinalYarnCount;
                                            row.Segment2ValueId = row.FinalYarnCount;
                                        }
                                        else {
                                            url = "/api/getYarnCountId/" + row.CalcYarnCount;
                                            axios.get(url)
                                                .then(function (response) {
                                                    var data = response.data;
                                                    row.YarnCountId = data.YarnCountId;
                                                    row.Segment2ValueId = data.YarnCountId;
                                                })
                                                .catch(function () {
                                                    toastr.error(constants.LOAD_ERROR_MESSAGE);
                                                })
                                        }
                                        row.Segment5ValueId = row.YarnColorId;
                                        $tableEl.bootstrapTable('updateByUniqueId', row.Id, row);
                                    })
                                })
                                .catch(function (err) {
                                    //console.log(err);
                                });
                        },
                    }
                },
                {
                    field: "MachineGauge",
                    title: "Suggested MG"
                },
                {
                    field: "FinalMachineGauge",
                    title: "Production MG",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "MachineDia",
                    title: "Suggested MD"
                },
                {
                    field: "FinalMachineDia",
                    title: "Production MD",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "StitchLength",
                    title: "Suggested SL"
                },
                {
                    field: "FinalStitchLength",
                    title: "Production SL",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "YarnDistribution",
                    title: "Yarn Distribution (%)",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "NetConsumption",
                    title: "Net Consumption"
                },
                {
                    field: "AllowancePer",
                    title: "Allowance (%)",
                    width: 20
                },
                {
                    field: "FinalAllowancePer",
                    title: "Production Allowance (%)",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "RequiredYarnQty",
                    title: "Required Qty"
                },
                {
                    field: "DisplayUnitDesc",
                    title: "UOM"
                },
                {
                    field: "RemarksOnYarn",
                    title: "Remarks",
                    width: 100,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
            ],
            onEditableSave: function (field, row, oldValue, $el) {
                yProgram = yProgram;
                yComposition = bookingAnalysisMaster.YarnCompositionList.find(function (el) { return el.id == row.YarnCompositionId }).text;
                if (row.YarnSubProgramIds) {
                    var selectedValue = setMultiSelectValueByValueInBootstrapTableEditable(bookingAnalysisMaster.YarnSubProgramList, row.YarnSubProgramIds);
                    row.YarnSubProgramIds = selectedValue.id;

                    yarnCount = row.Segment2ValueDesc;
                    row.YarnCategory = (yarnCount ? yarnCount : "") + "-" + selectedValue.text;
                    if (row.YarnTypeId) {
                        yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                    }
                    if (row.YarnColorId) {
                        yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                    }

                    yProgram = yProgram;
                    row.Segment1ValueDesc = yType;
                    row.Segment2ValueDesc = yarnCount;
                    row.Segment3ValueDesc = yComposition;
                    row.Segment4ValueDesc = shadeName;
                    row.Segment5ValueDesc = yColor;

                    row.Segment1ValueId = row.YarnTypeId;
                    row.Segment3ValueId = row.YarnCompositionId;
                    row.Segment5ValueId = row.YarnColorId;

                    var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yColor, yProgram, yType, ySubProgram);
                    YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                    YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                    YCategory = YCategory.replaceAll('--', '-');
                    row.YarnCategory = YCategory + "-" + selectedValue.text;
                    row.ProductionYarnCategory = row.YarnCategory;
                }
                else {
                    if (row.FinalYarnCount && row.YarnTypeId && row.YarnColorId) {
                        yarnCount = row.Segment2ValueDesc;
                        if (row.YarnTypeId) {
                            yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                        }
                        if (row.YarnColorId) {
                            yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                        }
                        yProgram = yProgram;
                        row.Segment1ValueDesc = yType;
                        row.Segment2ValueDesc = yarnCount;
                        row.Segment3ValueDesc = yComposition;
                        row.Segment4ValueDesc = shadeName;
                        row.Segment5ValueDesc = yColor;

                        row.Segment1ValueId = row.YarnTypeId;
                        row.Segment3ValueId = row.YarnCompositionId;
                        row.Segment5ValueId = row.YarnColorId;

                        var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yColor, yProgram, yType);
                        YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                        YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                        YCategory = YCategory.replaceAll('--', '-');
                        row.YarnCategory = YCategory;
                        row.ProductionYarnCategory = YCategory;
                    }
                    else {
                        if (row.YarnTypeId && row.YarnColorId) {
                            yarnCount = row.CalcYarnCount;
                            if (row.YarnTypeId) {
                                yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                            }
                            if (row.YarnColorId) {
                                yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                            }
                            yProgram = yProgram;
                            row.Segment1ValueDesc = yType;
                            row.Segment2ValueDesc = yarnCount;
                            row.Segment3ValueDesc = yComposition;
                            row.Segment4ValueDesc = shadeName;
                            row.Segment5ValueDesc = yColor;

                            row.Segment1ValueId = row.YarnTypeId;
                            row.Segment3ValueId = row.YarnCompositionId;
                            row.Segment5ValueId = row.YarnColorId;

                            var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yColor, yProgram, yType);
                            YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                            YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                            YCategory = YCategory.replaceAll('--', '-');
                            row.YarnCategory = YCategory;
                            row.ProductionYarnCategory = YCategory;
                        }
                    }
                }

                row.NetConsumption = ((row.BookingQty * row.YarnDistribution) / 100).toFixed(4);
                if (row.FinalAllowancePer == "0") {
                    row.RequiredYarnQty = row.NetConsumption;
                }
                else {
                    row.RequiredYarnQty = (((row.BookingQty * row.YarnDistribution) / 100) + ((row.FinalAllowancePer * row.NetConsumption) / 100)).toFixed(4);
                }

                var url = "";
                if (row.FinalYarnCount) {
                    row.YarnCountId = row.FinalYarnCount;
                    row.Segment2ValueId = row.FinalYarnCount;
                }
                else {
                    if (row.CalcYarnCount > 2) {
                        url = "/api/getYarnCountId/" + row.CalcYarnCount;
                        axios.get(url)
                            .then(function (response) {
                                var data = response.data;
                                row.YarnCountId = data.YarnCountId;
                                row.Segment2ValueId = data.YarnCountId;
                            })
                            .catch(function () {
                                toastr.error(constants.LOAD_ERROR_MESSAGE);
                            })
                    }
                }
                row.Segment5ValueId = row.YarnColorId;
                $tableEl.bootstrapTable('load', $el);
            }
        });
    }

    function GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yColor, yProgram, yType, ySubProgram) {
        //var yProgram = '';  //Comment by 15-01-2020

        if (yComposition.match(/BCI/g)) {
            yProgram = 'BCI';
            yComposition = yComposition.replaceAll('BCI ', '')
        }
        else if (yComposition.match(/Conventional/g)) {
            yProgram = 'Conventional';
            yComposition = yComposition.replaceAll('Conventional ', '')
        }
        else if (yComposition.match(/Melange/g)) {
            yProgram = 'Melange';
            yComposition = yComposition.replaceAll('Melange ', '')
        }
        else if (yComposition.match(/Organic/g)) {
            yProgram = 'Organic';
            yComposition = yComposition.replaceAll('Organic ', '')
        }
        else if (yComposition.match(/GOTS/g)) {
            yProgram = 'Gots';
            yComposition = yComposition.replaceAll('Gots ', '')
        }
        else if (yComposition.match(/Prima/g)) {
            yProgram = 'Prima';
            yComposition = yComposition.replaceAll('Prima ', '')
        }
        else if (yComposition.match(/Suprima/g)) {
            yProgram = 'Suprima';
            yComposition = yComposition.replaceAll('Suprima ', '')
        }
        else if (yComposition.match(/Inject/g)) {
            yProgram = 'Inject';
            yComposition = yComposition.replaceAll('Inject ', '')
        }
        else if (yComposition.match(/Siro/g)) {
            yProgram = 'Siro';
            yComposition = yComposition.replaceAll('Siro ', '')
        }

        var YCategory = '';
        var YCount = yarnCount.split(' ')[0];

        //if (ySubProgram.match(/Inject Melange/g))
        //    YCount = YCount + "Inject-Melange-";
        //else if (ySubProgram.match(/Inject Poly/g))
        //    YCount = YCount + "Inject-Poly-";
        //else if (ySubProgram.match(/Naps/g))
        //    YCount = YCount + "Naps-";

        var Com1 = GetFabricCompSegment(yComposition, 1);
        if (Com1 != undefined && Com1.substr(0, 5) == 'Bio-C') {
            Com1 = 'C';
        }
        Com1 = Com1 == undefined ? '' : Com1 == 'Lyocell' ? 'Lyocell' : Com1 == 'Tencel' ? 'Tencel' : Com1.substr(0, 1);
        var Com1v = GetFabricCompSegment(yComposition, 0);
        Com1v = Com1v == undefined ? parseInt(0) : parseInt(Com1v.replaceAll('%', ''));

        var Com2 = GetFabricCompSegment(yComposition, 3);
        if (Com2 != undefined && Com2.substr(0, 5) == 'Bio-C') {
            Com2 = 'C';
        }
        Com2 = Com2 == undefined ? '' : Com2 == 'Lyocell' ? 'Lyocell' : Com2 == 'Tencel' ? 'Tencel' : Com2.substr(0, 1);
        var Com2v = GetFabricCompSegment(yComposition, 2);
        Com2v = Com2v == undefined ? parseInt(0) : parseInt(Com2v.replaceAll('%', ''));

        var Com3 = GetFabricCompSegment(yComposition, 5);
        if (Com3 != undefined && Com3.substr(0, 5) == 'Bio-C') {
            Com3 = 'C';
        }
        Com3 = Com3 == undefined ? '' : Com3 == 'Lyocell' ? 'Lyocell' : Com3 == 'Tencel' ? 'Tencel' : Com3.substr(0, 1);
        var Com3v = GetFabricCompSegment(yComposition, 4);
        Com3v = Com3v == undefined ? parseInt(0) : parseInt(Com3v.replaceAll('%', ''));

        if (Com3 == '') {
            if (Com1 == 'C' && Com2 == 'P') {
                if (YCount.match(/Inject-Melange/g) != null || YCount.match(/Inject-Poly/g) != null || YCount.match(/Naps/g) != null) {
                    YCount = YCount.length > 0 ? YCount.charAt(YCount.length - 1) == '-' ? YCount.slice(0, -1) + '' : YCount : YCount;
                    //YCategory = YCount + '' + yProgram + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                    YCategory = YCount + '' + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                }
                else {
                    if (yProgram == 'Organic') {
                        //YCategory = YCount + '' + yProgram + 'OCVC(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                        YCategory = YCount + '' + 'OCVC(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                    }
                    else if (yProgram == 'GOTS') {
                        //YCategory = YCount + '' + yProgram + 'OCVC(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                        YCategory = YCount + '' + 'OCVC(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                    }
                    else {
                        //YCategory = YCount + '' + yProgram + 'CVC(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                        YCategory = YCount + '' + 'CVC(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                    }
                }
            }
            else if (Com1 == 'P' && Com2 == 'C') {
                if (YCount.match(/Inject-Melange/g) != null || YCount.match(/Inject-Poly/g) != null || YCount.match(/Naps/g) != null) {
                    YCount = YCount.length > 0 ? YCount.charAt(YCount.length - 1) == '-' ? YCount.slice(0, -1) + '' : YCount : YCount;
                    //YCategory = YCount + '' + yProgram + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')'; // Comment on 15-01-2020
                    YCategory = YCount + '' + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                }
                else
                    //YCategory = YCount + '' + yProgram + 'PC(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                    YCategory = YCount + '' + 'PC(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
            }
            else if (Com1 == 'C' && Com2 == 'M') {
                if (YCount.match(/Inject-Melange/g) != null || YCount.match(/Inject-Poly/g) != null || YCount.match(/Naps/g) != null) {
                    YCount = YCount.length > 0 ? YCount.charAt(YCount.length - 1) == '-' ? YCount.slice(0, -1) + '' : YCount : YCount;
                    //YCategory = YCount + '' + yProgram + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                    YCategory = YCount + '' + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                }
                else
                    //YCategory = YCount + '' + yProgram + 'CM(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                    YCategory = YCount + '' + 'CM(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
            }
            else if (Com1 == 'P' && Com2 == 'V') {
                if (YCount.match(/Inject-Melange/g) != null || YCount.match(/Inject-Poly/g) != null || YCount.match(/Naps/g) != null) {
                    YCount = YCount.length > 0 ? YCount.charAt(YCount.length - 1) == '-' ? YCount.slice(0, -1) + '' : YCount : YCount;
                    //YCategory = YCount + '' + yProgram + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                    YCategory = YCount + '' + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                }
                else
                    //YCategory = YCount + '' + yProgram + 'PV(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                    YCategory = YCount + '' + 'PV(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
            }

            else if (Com2 == 'V') {
                //if (yColor != 'Black') {
                if (YCount.match(/Inject-Melange/g) != null || YCount.match(/Inject-Poly/g) != null || YCount.match(/Naps/g) != null) {
                    YCount = YCount.length > 0 ? YCount.charAt(YCount.length - 1) == '-' ? YCount.slice(0, -1) + '' : YCount : YCount;
                    YCategory = YCount + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                }
                else
                    //YCategory = YCount + '' + yProgram + '' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                    YCategory = YCount + '' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                //}
                //else {
                if (ySubProgram) {
                    if (ySubProgram.match(/Color Melange/g) == null) {
                        if (Com2v > 0 && Com2v < 5) {
                            if (yProgram == 'Organic') {
                                if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                    //YCategory = YCount + '' + yProgram + 'OGM-' + Com2v + 'V';
                                    YCategory = YCount + '' + 'OGM-' + Com2v + 'V';
                            } else if (yProgram == 'GOTS') {
                                if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                    //YCategory = YCount + '' + yProgram + 'OGM-' + Com2v + 'V';
                                    YCategory = YCount + '' + 'OGM-' + Com2v + 'V';
                            }
                            else {
                                if (yColor != 'Raw Color' && yColor != 'Raw White') {
                                    if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                        //YCategory = YCount + '' + yProgram + 'GM-' + Com2v + 'V';
                                        YCategory = YCount + '' + 'GM-' + Com2v + 'V';
                                }
                            }
                        }
                        //else if (Com2v > 4 && Com2v < 31) {
                        else if (Com2v > 4 && Com2v < 41) {
                            if (yProgram == 'Organic') {
                                if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                    //YCategory = YCount + '' + yProgram + 'OGM-' + Com2v + 'V';
                                    YCategory = YCount + '' + 'OGM-' + Com2v + 'V';
                            } else if (yProgram == 'GOTS') {
                                if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                    //YCategory = YCount + '' + yProgram + 'OGM-' + Com2v + 'V';
                                    YCategory = YCount + '' + 'OGM-' + Com2v + 'V';
                            }
                            else {
                                if (yColor != 'Raw Color' && yColor != 'Raw White') {
                                    if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                        //YCategory = YCount + '' + yProgram + 'GM-' + Com2v + 'V';
                                        YCategory = YCount + '' + 'GM-' + Com2v + 'V';
                                }
                            }
                        }
                        //else if (Com2v > 30) {
                        else if (Com2v > 40) {
                            if (yProgram == 'Organic') {
                                if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                    //YCategory = YCount + '' + yProgram + 'O' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                                    YCategory = YCount + '' + 'O' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                            } else if (yProgram == 'GOTS') {
                                if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                    //YCategory = YCount + '' + yProgram + 'O' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                                    YCategory = YCount + '' + 'O' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                            }
                            else {
                                if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                    //YCategory = YCount + '' + yProgram + '' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                                    YCategory = YCount + '' + '' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                            }
                        }
                    }
                }
                else {
                    if (Com2v > 0 && Com2v < 5) {
                        if (yProgram == 'Organic') {
                            if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                //YCategory = YCount + '' + yProgram + 'OGM-' + Com2v + 'V';
                                YCategory = YCount + '' + 'OGM-' + Com2v + 'V';
                        } else if (yProgram == 'GOTS') {
                            if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                //YCategory = YCount + '' + yProgram + 'OGM-' + Com2v + 'V';
                                YCategory = YCount + '' + 'OGM-' + Com2v + 'V';
                        }
                        else {
                            if (yColor != 'Raw Color' && yColor != 'Raw White') {
                                if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                    //YCategory = YCount + '' + yProgram + 'GM-' + Com2v + 'V';
                                    YCategory = YCount + '' + 'GM-' + Com2v + 'V';
                            }
                        }
                    }
                    //else if (Com2v > 4 && Com2v < 31) {
                    else if (Com2v > 4 && Com2v < 41) {
                        if (yProgram == 'Organic') {
                            if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                //YCategory = YCount + '' + yProgram + 'OGM-' + Com2v + 'V';
                                YCategory = YCount + '' + 'OGM-' + Com2v + 'V';
                        } else if (yProgram == 'GOTS') {
                            if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                //YCategory = YCount + '' + yProgram + 'OGM-' + Com2v + 'V';
                                YCategory = YCount + '' + 'OGM-' + Com2v + 'V';
                        }
                        else {
                            if (yColor != 'Raw Color' && yColor != 'Raw White') {
                                if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                    //YCategory = YCount + '' + yProgram + 'GM-' + Com2v + 'V';
                                    YCategory = YCount + '' + 'GM-' + Com2v + 'V';
                            }
                        }
                    }
                    //else if (Com2v > 30) {
                    else if (Com2v > 40) {
                        if (yProgram == 'Organic') {
                            if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                //YCategory = YCount + '' + yProgram + 'O' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                                YCategory = YCount + '' + 'O' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                        } else if (yProgram == 'GOTS') {
                            if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                //YCategory = YCount + '' + yProgram + 'O' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                                YCategory = YCount + '' + 'O' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                        }
                        else {
                            if (YCount.match(/Inject-Melange/g) == null && YCount.match(/Inject-Poly/g) == null && YCount.match(/Naps/g) == null)
                                //YCategory = YCount + '' + yProgram + '' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                                YCategory = YCount + '' + '' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                        }
                    }
                }

                //}
            }
            else if (Com2 == '') {
                if (Com1 == 'P') {
                    if (yType == 'Filament') {
                        if (YCount.match(/DSP/g) == 'DSP')//add on 03-Apr-2018 request by Mr. Alim
                        {
                            //YCategory = YCount + '' + yProgram;
                            YCategory = YCount;
                        }
                        else {
                            //YCategory = YCount + '' + yProgram + 'Poly';
                            YCategory = YCount + '' + 'Poly';
                        }
                    }
                    else {
                        if (YCount == '20/2' || YCount == '50/2') {
                            //YCategory = YCount + '' + yProgram + 'Poly';
                            YCategory = YCount + '' + 'Poly';
                        }
                        else if (YCount.match(/F/g) == 'F' || YCount.match(/D/g) == 'D')//add on 13-03-18 requested by Mr. Rafiq SCD
                        {
                            //YCategory = YCount + '' + yProgram;
                            YCategory = YCount;
                        }
                        else {
                            //YCategory = YCount + '' + yProgram + 'Spun-Poly';
                            YCategory = YCount + '' + 'Spun-Poly';
                        }
                    }
                }
                else if (Com1 == 'C') {
                    //if ((yType == 'Compact' || yType == 'Combed' || yType == 'Carded') && yProgram == 'Organic') {
                    //    if (YCount.match(/Inject-Melange/g) != null || YCount.match(/Inject-Poly/g) != null || YCount.match(/Naps/g) != null)
                    //    YCategory = YCount + '' + 'OC';
                    //}
                    //else if (yType == 'Compact' || yType == 'Combed') {
                    //    if (YCount.match(/Inject-Melange/g) != null || YCount.match(/Inject-Poly/g) != null || YCount.match(/Naps/g) != null)
                    //    YCategory = YCount + '' + 'CC';
                    //}
                    //else if (yType == 'Carded') {
                    //    if (YCount.match(/Inject-Melange/g)!=null || YCount.match(/Inject-Poly/g)!=null || YCount.match(/Naps/g)!=null)
                    //    YCategory = YCount + '' + 'KC';
                    //}

                    //YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'Cotton';
                    if (ySubProgram) {
                        if (ySubProgram.match(/Open End/g)) {
                            YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'OE';
                            ySubProgram = ySubProgram.replaceAll(', Open End', '').replaceAll('Open End', '');
                        }
                    }
                    else {
                        if ((yType == 'Compact' || yType == 'Combed' || yType == 'Carded') && yProgram == 'Organic') {
                            //YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'OC'; // Comment on 15-01-2020
                            YCategory = YCount + '' + yarnCount.split(' ')[1] + 'OC';
                        } else if ((yType == 'Compact' || yType == 'Combed' || yType == 'Carded') && yProgram == 'GOTS') {
                            //YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'OC';
                            YCategory = YCount + '' + yarnCount.split(' ')[1] + 'OC';
                        }
                        else if ((yType == 'Combed') && yProgram == 'BCI') {
                            YCategory = YCount + '' + yarnCount.split(' ')[1] + 'CC';
                        }
                        else if ((yType == 'Compact') && yProgram == 'BCI') {
                            //YCategory = YCount + '' + yarnCount.split(' ')[1] + 'CC-Compact' + '-' + yType;
                            YCategory = YCount + '' + yarnCount.split(' ')[1] + 'CC-Compact';
                        }
                        else if ((yType == 'Combed') && yProgram != 'BCI' && yProgram != 'GOTS') {
                            //YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'CC';
                            YCategory = YCount + '' + yarnCount.split(' ')[1] + 'CC';
                        }
                        else if ((yType == 'Compact') && yProgram != 'BCI' && yProgram != 'GOTS') {
                            //YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'CC-Compact';
                            YCategory = YCount + '' + yarnCount.split(' ')[1] + 'CC-Compact';
                        }
                        //else if (yType == 'Compact' || yType == 'Combed') {
                        //    YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'CC';
                        //}
                        else if ((yType == 'Carded') && yProgram == 'BCI') {
                            //YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'KC';
                            YCategory = YCount + '' + yarnCount.split(' ')[1] + 'KC';
                        }
                        else if (yType == 'Open End') {
                            //YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'OE';
                            YCategory = YCount + '' + yarnCount.split(' ')[1] + 'OE';
                        }
                    }
                }
                else if (Com1 == 'V') {
                    //YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'Viscose';
                    YCategory = YCount + '' + '' + yarnCount.split(' ')[1] + 'Viscose';
                }
                else if (Com1 == 'M') {
                    //YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'Modal';
                    YCategory = YCount + '' + '' + yarnCount.split(' ')[1] + 'Modal';
                }
                else if (Com1 == 'L') {
                    //YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'Linen';
                    YCategory = YCount + '' + yarnCount.split(' ')[1] + 'Linen';
                }
                else if (Com1 == 'R') {
                    //YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'Rayon';
                    YCategory = YCount + '' + yarnCount.split(' ')[1] + 'Rayon';
                }
                else if (Com1 == 'E') {         // Added New Line on 15-01-2020
                    YCategory = YCount + '' + yarnCount.split(' ')[1];
                }
                else if (Com1 == 'Lyocell') {
                    //YCategory = YCount + '' + yProgram + '' + 'Lyocell';
                    YCategory = YCount + '' + 'Lyocell';
                }
                else if (Com1 == 'Tencel') {
                    //YCategory = YCount + '' + yProgram + '' + 'Tencel';
                    YCategory = YCount + '' + 'Tencel';
                }
                else {
                    //YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1];
                    YCategory = YCount + '' + yarnCount.split(' ')[1];
                }
            }
            else {
                //YCategory = YCount + '' + yProgram + '' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                YCategory = YCount + '' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
            }
        }
        else {
            if (Com2 != '' && Com3 == '') {
                //YCategory = YCount + '' + yProgram + '' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                YCategory = YCount + '' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
            }
            else if (Com2 != '' && Com3 != '') {
                //YCategory = YCount + '' + yProgram + '' + Com1 + Com2 + Com3 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + '/' + Com3 + Com3v + ')';
                YCategory = YCount + '' + Com1 + Com2 + Com3 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + '/' + Com3 + Com3v + ')';
            }
        }

        //if (yComposition.match(/100% Cotton/g) && ySubProgram.match(/Melange/g)) {
        // //   YCategory = YCount + '' + yProgram + 'CM';
        //}

        YCategory = YCategory.replaceAll(' undefined', '').replaceAll('undefined', '').replaceAll('undefined', '');

        if (ySubProgram) {
            if (ySubProgram.match(/Inject Melange/g))
                YCount = YCategory + "Inject-Melange-";
            else if (ySubProgram.match(/Inject Poly/g))
                YCount = YCategory + "Inject-Poly-";
            else if (ySubProgram.match(/Naps/g))
                YCount = YCategory + "Naps-";

            ///----------add yarn brand into yarn category------------//
            if (yProgram.match(/Inject/g)) {
                YCategory = YCategory + 'Inject';
            }
            else if (yProgram.match(/Siro/g)) {
                YCategory = YCategory + 'Siro';
            }
        }

        ///----------add yarn type into yarn category------------//
        if (yType.match(/Ring/g)) {
            YCategory = YCategory + 'Ring';
        }
        else if (yType.match(/Vortex/g)) {
            YCategory = YCategory + 'Vortex';
        }
        YCategory = YCategory + GetYarnSubProgramString(ySubProgram);
        if (yColor == 'White') {
            if (yComposition.match(/Viscose/g) && yColor == 'White') {
                YCategory = YCategory.replaceAll('-W', '');
            }
            else {
                YCategory = YCategory.replaceAll('-W', '') + '-W';
            }
        }

        if (yColor == 'Raw White') {
            YCategory = YCategory.replaceAll('-W', '') + '-W';
        }
        if (ySubProgram) {
            if (ySubProgram.match(/Color Melange/g)) {
                if (yColor == 'Raw Color') {
                    YCategory = YCategory.replaceAll('Raw', '') + 'Raw';
                }
            }
        }
        return YCategory;
    }

    function GetYarnSubProgramString(ySubProgram) {
        var ysp = '';
        if (ySubProgram) {
            var ySubProgram = ySubProgram.split(',');
            for (var i = 0; i < ySubProgram.length; i++) {
                if (i == 0) {
                    if (ysp == '')
                        ysp = ySubProgram[i].trim().replaceAll(' ', '-');
                    else
                        ysp = ySubProgram[i].trim().replaceAll(' ', '-');
                }
                else {
                    if (ysp == '')
                        ysp = ySubProgram[i].trim().replaceAll(' ', '-');
                    else
                        ysp = ysp + "-" + ySubProgram[i].trim().replaceAll(' ', '-');
                }
            }
        }

        if (ysp != '') {
            ysp = '-' + ysp + '-';
        }
        return ysp;
    }

    function GetYarnCategoryCollar(yComposition, yarnCount, yProgram, yType, yColor) {
        var yProgram = '';

        if (yComposition.match(/BCI/g)) {
            yProgram = 'BCI';
            yComposition = yComposition.replaceAll('BCI ', '')
        }
        else if (yComposition.match(/Conventional/g)) {
            yProgram = 'Conventional';
            yComposition = yComposition.replaceAll('Conventional ', '')
        }
        else if (yComposition.match(/Melange/g)) {
            yProgram = 'Melange';
            yComposition = yComposition.replaceAll('Melange ', '')
        }
        else if (yComposition.match(/Organic/g)) {
            yProgram = 'Organic';
            yComposition = yComposition.replaceAll('Organic ', '')
        }
        else if (yComposition.match(/GOTS/g)) {
            yProgram = 'Gots';
            yComposition = yComposition.replaceAll('Gots ', '')
        }
        else if (yComposition.match(/Prima/g)) {
            yProgram = 'Prima';
            yComposition = yComposition.replaceAll('Prima ', '')
        }
        else if (yComposition.match(/Suprima/g)) {
            yProgram = 'Suprima';
            yComposition = yComposition.replaceAll('Suprima ', '')
        }
        else if (yComposition.match(/Inject/g)) {
            yProgram = 'Inject';
            yComposition = yComposition.replaceAll('Inject ', '')
        }
        else if (yComposition.match(/Siro/g)) {
            yProgram = 'Siro';
            yComposition = yComposition.replaceAll('Siro ', '')
        }

        var YCategory = '';
        var YCount = yarnCount.split(' ')[0];

        var Com1 = GetFabricCompSegmentCollar(yComposition, 1);
        if (Com1 != undefined && Com1.substr(0, 5) == 'Bio-C') {
            Com1 = 'C';
        }
        Com1 = Com1 == undefined ? '' : Com1 == 'Lyocell' ? 'Lyocell' : Com1 == 'Tencel' ? 'Tencel' : Com1.substr(0, 1);
        var Com1v = GetFabricCompSegmentCollar(yComposition, 0);
        Com1v = Com1v == undefined ? parseInt(0) : parseInt(Com1v.replaceAll('%', ''));

        var Com2 = GetFabricCompSegmentCollar(yComposition, 3);
        if (Com2 != undefined && Com2.substr(0, 5) == 'Bio-C') {
            Com2 = 'C';
        }
        Com2 = Com2 == undefined ? '' : Com2 == 'Lyocell' ? 'Lyocell' : Com2 == 'Tencel' ? 'Tencel' : Com2.substr(0, 1);
        var Com2v = GetFabricCompSegmentCollar(yComposition, 2);
        Com2v = Com2v == undefined ? parseInt(0) : parseInt(Com2v.replaceAll('%', ''));

        var Com3 = GetFabricCompSegmentCollar(yComposition, 5);
        if (Com3 != undefined && Com3.substr(0, 5) == 'Bio-C') {
            Com3 = 'C';
        }
        Com3 = Com3 == undefined ? '' : Com3 == 'Lyocell' ? 'Lyocell' : Com3 == 'Tencel' ? 'Tencel' : Com3.substr(0, 1);
        var Com3v = GetFabricCompSegmentCollar(yComposition, 4);
        Com3v = Com3v == undefined ? parseInt(0) : parseInt(Com3v.replaceAll('%', ''));

        if (Com3 == '') {
            if (Com1 == 'C' && Com2 == 'P') {
                if (YCount.match(/Inject-Melange/g) != null || YCount.match(/Inject-Poly/g) != null || YCount.match(/Naps/g) != null) {
                    YCount = YCount.length > 0 ? YCount.charAt(YCount.length - 1) == '-' ? YCount.slice(0, -1) + '' : YCount : YCount;
                    YCategory = YCount + '' + yProgram + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                }
                else
                    if (yProgram == 'Organic') {
                        YCategory = YCount + '' + yProgram + 'OCVC(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                    } else if (yProgram == 'GOTS') {
                        YCategory = YCount + '' + yProgram + 'OCVC(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                    }
                    else {
                        YCategory = YCount + '' + yProgram + 'CVC(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                    }
            }
            else if (Com1 == 'P' && Com2 == 'C') {
                if (YCount.match(/Inject-Melange/g) != null || YCount.match(/Inject-Poly/g) != null || YCount.match(/Naps/g) != null) {
                    YCount = YCount.length > 0 ? YCount.charAt(YCount.length - 1) == '-' ? YCount.slice(0, -1) + '' : YCount : YCount;
                    YCategory = YCount + '' + yProgram + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                }
                else
                    YCategory = YCount + '' + yProgram + 'PC(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
            }
            else if (Com1 == 'C' && Com2 == 'M') {
                if (YCount.match(/Inject-Melange/g) != null || YCount.match(/Inject-Poly/g) != null || YCount.match(/Naps/g) != null) {
                    YCount = YCount.length > 0 ? YCount.charAt(YCount.length - 1) == '-' ? YCount.slice(0, -1) + '' : YCount : YCount;
                    YCategory = YCount + '' + yProgram + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                }
                else
                    YCategory = YCount + '' + yProgram + 'CM(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
            }
            else if (Com1 == 'P' && Com2 == 'V') {
                if (YCount.match(/Inject-Melange/g) != null || YCount.match(/Inject-Poly/g) != null || YCount.match(/Naps/g) != null) {
                    YCount = YCount.length > 0 ? YCount.charAt(YCount.length - 1) == '-' ? YCount.slice(0, -1) + '' : YCount : YCount;
                    YCategory = YCount + '' + yProgram + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                }
                else
                    YCategory = YCount + '' + yProgram + 'PV(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
            }

            else if (Com2 == 'V') {
                //if (yColor != 'Black') {
                if (YCount.match(/Inject-Melange/g) != null || YCount.match(/Inject-Poly/g) != null || YCount.match(/Naps/g) != null) {
                    YCount = YCount.length > 0 ? YCount.charAt(YCount.length - 1) == '-' ? YCount.slice(0, -1) + '' : YCount : YCount;
                    YCategory = YCount + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
                }
                else
                    YCategory = YCount + '' + yProgram + '' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
            }
            else if (Com2 == '') {
                if (Com1 == 'P') {
                    if (yType == 'Filament') {
                        if (YCount.match(/DSP/g) == 'DSP')//add on 03-Apr-2018 request by Mr. Alim
                        {
                            YCategory = YCount + '' + yProgram;
                        }
                        else {
                            YCategory = YCount + '' + yProgram + 'Poly';
                        }
                    }
                    else {
                        //YCategory = YCount + '' + yProgram + 'Spun-Poly';
                        if (YCount == '20/2' || YCount == '50/2') {
                            YCategory = YCount + '' + yProgram + 'Poly';
                        }
                        else if (YCount.match(/F/g) == 'F' || YCount.match(/D/g) == 'D')//add on 13-03-18 requested by Mr. Rafiq SCD
                        {
                            YCategory = YCount + '' + yProgram;
                        }
                        else {
                            YCategory = YCount + '' + yProgram + 'Spun-Poly';
                        }
                    }
                }
                else if (Com1 == 'C') {
                    if ((yType == 'Compact' || yType == 'Combed' || yType == 'Carded') && yProgram == 'Organic') {
                        YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'OC';
                    } else if ((yType == 'Compact' || yType == 'Combed' || yType == 'Carded') && yProgram == 'GOTS') {
                        YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'OC';
                    }
                    else if ((yType == 'Combed' || yType == 'Carded') && yProgram == 'BCI') {
                        YCategory = YCount + '' + yarnCount.split(' ')[1] + 'CC';
                    }
                    else if ((yType == 'Compact') && yProgram == 'BCI') {
                        YCategory = YCount + '' + yarnCount.split(' ')[1] + 'CC-Compact' + '-' + yType;
                    }
                    else if ((yType == 'Combed') && yProgram != 'BCI') {
                        YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'CC';
                    }
                    else if ((yType == 'Compact') && yProgram != 'BCI') {
                        YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'CC-Compact';
                    }
                    //else if (yType == 'Compact' || yType == 'Combed') {
                    //    YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'CC';
                    //}
                    else if (yType == 'Carded') {
                        YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'KC';
                    }
                    else if (yType == 'Open End') {
                        YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'OE';
                    }
                }
                else if (Com1 == 'V') {
                    YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'Viscose';
                }
                else if (Com1 == 'M') {
                    YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'Modal';
                }
                else if (Com1 == 'L') {
                    YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'Linen';
                }
                else if (Com1 == 'R') {
                    YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1] + 'Rayon';
                }
                else if (Com1 == 'Lyocell') {
                    YCategory = YCount + '' + yProgram + '' + 'Lyocell';
                }
                else if (Com1 == 'Tencel') {
                    YCategory = YCount + '' + yProgram + '' + 'Tencel';
                }
                else {
                    YCategory = YCount + '' + yProgram + '' + yarnCount.split(' ')[1];
                }
            }
            else {
                YCategory = YCount + '' + yProgram + '' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
            }
        }
        else {
            if (Com2 != '' && Com3 == '') {
                YCategory = YCount + '' + yProgram + '' + Com1 + Com2 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + ')';
            }
            else if (Com2 != '' && Com3 != '') {
                YCategory = YCount + '' + yProgram + '' + Com1 + Com2 + Com3 + '(' + Com1 + Com1v + '/' + Com2 + Com2v + '/' + Com3 + Com3v + ')';
            }
        }

        YCategory = YCategory.replaceAll(' undefined', '').replaceAll('undefined', '').replaceAll('undefined', '');

        ///----------add yarn brand into yarn category------------//
        if (yProgram.match(/Inject/g)) {
            YCategory = YCategory + 'Inject';
        }
        else if (yProgram.match(/Siro/g)) {
            YCategory = YCategory + 'Siro';
        }

        ///----------add yarn type into yarn category------------//
        if (yType.match(/Ring/g)) {
            YCategory = YCategory + 'Ring';
        }
        else if (yType.match(/Vortex/g)) {
            YCategory = YCategory + 'Vortex';
        }

        if (yColor == 'Raw White') {
            YCategory = YCategory.replaceAll('-W', '') + '-W';
        }

        return YCategory;
    }

    function GetFabricCompSegment(fbcomp, seq) {
        var ycSegment = fbcomp.split(' ');
        if (ycSegment[seq] != undefined) {
            return ycSegment[seq].trim();
        }
    }

    function GetFabricCompSegmentCollar(fbcomp, seq) {
        var ycSegment = fbcomp.split(' ');
        if (ycSegment[seq] != undefined) {
            return ycSegment[seq].trim();
        }
    }

    function populateBookingAnalysisChildYarn(analysisChildId, technicalNameId, fabricComposition, $detail) {
        if (!technicalNameId) {
            return bootbox.alert({
                message: '<span class="text-danger">Your must select a technical name before expand.<span>',
                size: 'small'
            });
        }
        if (!shadeId) {
            return bootbox.alert({
                message: '<span class="text-danger">Your must select a shade before expand.<span>',
                size: 'small'
            });
        }
        var $el = $detail.html('<table id="tblFabricBookingAnalysisChildYarn-' + bookingChildId + '"></table>').find('table');

        var bookingAnalysisChild = {};
        if (analysisChildId > 0) {
            bookingAnalysisChild = bookingAnalysisMaster.Childs.find(function (el) {
                return el.Id == analysisChildId;
            });
        }
        else {
            bookingAnalysisChild = bookingAnalysisMaster.Childs.find(function (el) {
                return el.BookingChildId == bookingChildId;
            });
        }

        if (bookingAnalysisChild.ChildYarns.length > 0) {
            initBookingAnalysisChildYarn($el, bookingChildId, bookingAnalysisChild.ChildYarns);
        }
        else {
            var url = "";
            if (analysisChildId > 0) {
                url = "/api/bookinganalysischildyarn/" + analysisChildId + "/" + bookingChildId;
            }
            else {
                if (bookingAnalysisMaster.BookingType = bookingTypeConstants.BULK) // 1 for Bulk Fabric
                {
                    url = "/api/newbookinganalysischildyarn?bookingChildId=" + bookingChildId + "&constructionId=" + constructionId + "&fabricGsm=" + fabricGsm + "&fabricWidth=" + fabricWidth + "&shadeId=" + shadeId + "&yarnCompositionName=" + fabricComposition + "&fabTechnicalName=" + technicalNameId + "&dyeingtype=" + dyeingtype + "&buyerId=" + buyerId + "&knittingtype=" + knittingtype + "&yarnProgram=" + yProgram;
                }
                if (bookingAnalysisMaster.BookingType = bookingTypeConstants.SAMPLE) // 0 for Sample Fabric
                {
                    url = "/api/newbookinganalysischildyarnSample?bookingChildId=" + bookingChildId + "&constructionId=" + constructionId + "&fabricGsm=" + fabricGsm + "&fabricWidth=" + fabricWidth + "&shadeId=" + shadeId + "&yarnCompositionName=" + fabricComposition + "&fabTechnicalName=" + technicalNameId + "&dyeingtype=" + dyeingtype + "&buyerId=" + buyerId + "&knittingtype=" + knittingtype + "&yarnProgram=" + yProgram;
                }
            }

            axios.get(url)
                .then(function (response) {
                    $.each(response.data, function (i, v) {
                        bookingAnalysisChild.ChildYarns.push(v);
                    });
                    initBookingAnalysisChildYarn($el, bookingChildId, response.data);
                })
                .catch(function (err) {
                    toastr.error(err.response.data.Message);
                });
        }
    }

    function populateBookingAnalysisChildYarnCollar(analysisChildId, fabricComposition, $detail) {
        var $el = $detail.html('<table id="tblFabricBookingAnalysisChildYarnCollar-' + bookingChildId + '"></table>').find('table');

        var bookingAnalysisChildCollar = {};
        if (analysisChildId > 0) {
            bookingAnalysisChildCollar = bookingAnalysisChildCollarList.find(function (el) {
                return el.Id == analysisChildId;
            });
        }
        else {
            bookingAnalysisChildCollar = bookingAnalysisChildCollarList.find(function (el) {
                return el.BookingChildId == bookingChildId;
            });
        }

        var url = "";
        if (analysisChildId > 0) {
            url = "/api/bookinganalysischildyarn/" + analysisChildId + "/" + bookingChildId;
        }
        else {
            url = "/api/newbookinganalysischildyarnCollarCuff?bookingChildId=" + bookingChildId + "&constructionId=" + constructionId + "&yarnCompositionName=" + yComposition + "&buyerId=" + buyerId + "&yarnProgram=" + yProgram;
        }

        axios.get(url)
            .then(function (response) {
                $.each(response.data, function (i, v) {
                    bookingAnalysisChildCollar.ChildYarns.push(v);
                });
                initBookingAnalysisChildYarnCollar($el, bookingChildId, response.data);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function populateBookingAnalysisChildYarnCuff(analysisChildId, technicalNameId, fabricComposition, $detail) {
        var $el = $detail.html('<table id="tblFabricBookingAnalysisChildYarnCuff-' + bookingChildId + '"></table>').find('table');

        var bookingAnalysisChildCuff = {};
        if (analysisChildId > 0) {
            bookingAnalysisChildCuff = bookingAnalysisChildCuffList.find(function (el) {
                return el.Id == analysisChildId;
            });
        }
        else {
            bookingAnalysisChildCuff = bookingAnalysisChildCuffList.find(function (el) {
                return el.BookingChildId == bookingChildId;
            });
        }

        var url = "";
        if (analysisChildId > 0) {
            url = "/api/bookinganalysischildyarn/" + analysisChildId + "/" + bookingChildId;
        }
        else {
            url = "/api/newbookinganalysischildyarnCollarCuff?bookingChildId=" + bookingChildId + "&constructionId=" + constructionId + "&yarnCompositionName=" + yComposition + "&buyerId=" + buyerId + "&yarnProgram=" + yProgram;
        }

        axios.get(url)
            .then(function (response) {
                $.each(response.data, function (i, v) {
                    bookingAnalysisChildCuff.ChildYarns.push(v);
                });
                initBookingAnalysisChildYarnCuff($el, bookingChildId, response.data);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getMasterData() {
        $tblMasterEl.bootstrapTable('showLoading');
        var queryParams = $.param(tableParams);
        var url = "/api/bookinganalysis?gridType=bootstrap-table&status=" + status + "&processMasterId=" + 1 + "&" + queryParams;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(showResponseError)
    }

    function getBookingAnalysisMaster(id, bookingNo, bookingType) {
        var url = id ? `/api/booking-analysis/${id}/${bookingType}` : `/api/booking-analysis/new/${bookingNo}/${bookingType}`;

        axios.get(url)
            .then(function (response) {
                bookingAnalysisMaster = response.data;
                bookingAnalysisMaster.PreBAnalysisId = bookingAnalysisMaster.Id;
                bookingAnalysisMaster.FabricLDD = formatDateToDefault(bookingAnalysisMaster.FabricLDD);
                bookingAnalysisMaster.RevisionDate = formatDateToDefault(bookingAnalysisMaster.RevisionDate);
                bookingAnalysisMaster.AcknowledgeDate = formatDateToDefault(bookingAnalysisMaster.AcknowledgeDate);
                bookingAnalysisMaster.BookingDate = formatDateToDefault(bookingAnalysisMaster.BookingDate);
                bookingAnalysisMaster.BAnalysisDate = formatDateToDefault(bookingAnalysisMaster.BAnalysisDate);
                bookingAnalysisMaster.HasFabric = bookingAnalysisMaster.HasFabric ? "Yes" : "No";
                bookingAnalysisMaster.HasCollar = bookingAnalysisMaster.HasCollar ? "Yes" : "No";
                bookingAnalysisMaster.HasCuff = bookingAnalysisMaster.HasCuff ? "Yes" : "No";
                setFormData($formEl, bookingAnalysisMaster);

                $formEl.find("#divEditBookingAnalysisMaster").fadeIn();
                $formEl.find("#tblTextileProcessMaster").fadeIn();
                $divTblEl.fadeOut();
            }).catch(showResponseError)
    }

    function getBookingAnalysisMasterSampleBooking(id, bookingNo) {
        var url = `/api/booking-analysis/sample/${id}/${bookingNo}`;
        axios.get(url)
            .then(function (response) {
                bookingAnalysisMaster = response.data;
                setFormData($formEl, bookingAnalysisMaster);
                $formEl.find("#PreBAnalysisId").val(bookingAnalysisMaster.Id);
                $formEl.find("#BookingNo").val(bookingAnalysisMaster.BookingNo);
                $formEl.find("#ExportOrderNo").val(bookingAnalysisMaster.ExportOrderNo);
                $formEl.find("#BuyerName").val(bookingAnalysisMaster.BuyerName);
                $formEl.find("#BuyerName").val(bookingAnalysisMaster.BuyerTeamName);
                $formEl.find("#MerchandiserName").val(bookingAnalysisMaster.MerchandiserName);
                if (bookingAnalysisMaster.AcknowledgeDate) $formEl.find("#AcknowledgeDate").val(moment(bookingAnalysisMaster.AcknowledgeDate).format('MM/DD/YYYY'));
                if (bookingAnalysisMaster.BookingDate) $formEl.find("#BookingDate").val(moment(bookingAnalysisMaster.BookingDate).format('MM/DD/YYYY'));
                $formEl.find("#StyleNo").val(bookingAnalysisMaster.StyleNo);
                $formEl.find("#Constructions").text(bookingAnalysisMaster.Constructions);
                $formEl.find("#Compositions").text(bookingAnalysisMaster.Compositions);
                $formEl.find("#StyleTypes").text(bookingAnalysisMaster.StyleTypes);
                $formEl.find("#divEditBookingAnalysisMaster").fadeIn();
                $formEl.find("#tblTextileProcessMaster").fadeIn();
                $divTblEl.fadeOut();
                $formEl.find("#divButtonExecutionsBookingBI").fadeIn();
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }

    function getChildData() {
        var id = $formEl.find("#Id").val();
        var bookingNo = $formEl.find("#BookingNo").val();
        var url = id ? `/api/booking-analysis/new/childs/${bookingNo}` : "";
        axios.get(url)
            .then(function (response) {
                bookingAnalysisMaster.Childs = response.data;

                var fabricList = bookingAnalysisMaster.Childs.filter(function (el) { return el.SubGroupID == 1 });
                initTblFabricBookingAnalysisChild();
                $tblBookingAnalysisChildFabric.bootstrapTable('load', fabricList);

                if (bookingAnalysisMaster.HasCollar == "Yes") {
                    var collarList = bookingAnalysisMaster.Childs.filter(function (el) { return el.SubGroupID == 11 });
                    initTblFabricBookingAnalysisChildCollar();
                    $tblBookingAnalysisChildCollar.bootstrapTable('load', collarList);
                    $formEl.find("#divEditBookingAnalysisMasterCollar").fadeIn();
                }
                if (bookingAnalysisMaster.HasCuff == "Yes") {
                    var cuffList = bookingAnalysisMaster.Childs.filter(function (el) { return el.SubGroupID == 12 });
                    initTblFabricBookingAnalysisChildCuff();
                    $tblBookingAnalysisChildCuff.bootstrapTable('load', cuffList);
                    $formEl.find("#divEditBookingAnalysisMasterCuff").fadeIn();
                }
            }).catch(showResponseError)
    }

    function initBookingAnalysisChildYarnCollar($tableEl, bookingChildId, data) {
        $tableEl.bootstrapTable({
            showFooter: true,
            data: data,
            uniqueId: 'Id',
            rowStyle: function (row, index) {
                if (row.EntityState == 8)
                    return { classes: 'deleted-row' };

                return "";
            },
            columns: [
                {
                    title: 'Actions',
                    align: 'center',
                    width: 100,
                    formatter: function () {
                        return [
                            '<span class="btn-group">',
                            '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete Item">',
                            '<i class="fa fa-remove"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    footerFormatter: function () {
                        return [
                            '<span class="btn-group">',
                            '<button class="btn btn-success btn-xs edit" onclick="return addNewChildRowCollarBA(event, ' + bookingChildId + ')" title="Add">',
                            '<i class="fa fa-plus"></i>',
                            ' Add',
                            '</button>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            this.data[index].EntityState = 8;
                            var $target = $(e.target);
                            $target.closest("tr").addClass('deleted-row');
                        }
                    }
                },
                {
                    field: "YarnTypeId",
                    title: "Yarn Type",
                    align: "left",
                    editable: {
                        source: bookingAnalysisMaster.YarnTypeList,
                        type: 'select2',
                        showbuttons: false,
                        select2: { width: 100, placeholder: 'Yarn Type' }
                    }
                },
                {
                    field: "YarnComposition",
                    title: "Yarn Composition",
                    width: 100,
                },
                {
                    field: "YarnColorId",
                    title: "Yarn Color",
                    width: 80,
                    editable: {
                        type: 'select2',
                        title: 'Select a Color',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: bookingAnalysisMaster.YarnColorList,
                        select2: { width: 100, placeholder: 'Yarn Color' }
                    }
                },

                {
                    field: "YarnSubProgramIds",
                    title: "Yarn Sub Program",
                    editable: {
                        type: 'select2',
                        showbuttons: true,
                        source: bookingAnalysisMaster.YarnSubProgramList,
                        select2: { width: 200, height: 300, multiple: true, placeholder: 'Select Sub Program' }
                    }
                },
                {
                    field: "YarnCategory",
                    title: "Yarn Category",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "YarnSpecification",
                    title: "Yarn Specification",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "YarnShadeCode",
                    title: "Shade Code",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "IsYD",
                    title: "YD",
                    placeholder: 'YD',
                    checkbox: true
                },
                {
                    field: "CalcYarnCount",
                    title: "Suggested YC",
                    width: 20,
                    visible: !isCustomCheckedYarnCount
                },
                {
                    field: "FinalYarnCount",
                    title: "Production YC",
                    width: 20,
                    formatter: function (value, row, index, field) {
                        return ['<span class="btn-group">',
                            '<a href="javascript:void(0)" class="editable-link edit">' + row.Segment2ValueDesc + '</a>',
                            '</span>'].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            if (!row.YarnTypeId) return toastr.error("Yarn Type is not selected");
                            var url = "/api/selectoption/constructionAndYarnTypeWiseYarnCountLists/" + row.YarnTypeId;
                            axios.get(url)
                                .then(function (response) {
                                    var yarnCountList = convertToSelectOptions(response.data);
                                    showBootboxSelectPrompt("Select Yarn Count", yarnCountList, "", function (result) {
                                        if (!result)
                                            return toastr.warning("You didn't selected any Yarn Count.");

                                        var selectedYarnCount = yarnCountList.find(function (el) { return el.value === result })
                                        row.FinalYarnCount = result;
                                        row.Segment2ValueDesc = selectedYarnCount.text;

                                        yComposition = row.YarnComposition;
                                        if (row.YarnSubProgramIds) {
                                            var selectedValue = setMultiSelectValueByValueInBootstrapTableEditable(bookingAnalysisMaster.YarnSubProgramList, row.YarnSubProgramIds);
                                            row.YarnSubProgramIds = selectedValue.id;

                                            yarnCount = row.Segment2ValueDesc;
                                            row.YarnCategory = (yarnCount ? yarnCount : "") + "-" + selectedValue.text;
                                            if (row.YarnTypeId) {
                                                yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                                            }
                                            if (row.YarnColorId) {
                                                yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                                            }
                                            yProgram = yProgram;
                                            row.Segment1ValueDesc = yType;
                                            row.Segment2ValueDesc = yarnCount;
                                            row.Segment3ValueDesc = yComposition;
                                            row.Segment4ValueDesc = shadeName;
                                            row.Segment5ValueDesc = yColor;

                                            row.Segment1ValueId = row.YarnTypeId;
                                            row.Segment5ValueId = row.YarnColorId;

                                            var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yColor, yProgram, yType, ySubProgram);
                                            YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                                            YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                                            YCategory = YCategory.replaceAll('--', '-');
                                            row.YarnCategory = YCategory + "-" + selectedValue.text;
                                            row.ProductionYarnCategory = row.YarnCategory;
                                        }
                                        else {
                                            if (row.FinalYarnCount) {
                                                yarnCount = row.Segment2ValueDesc;
                                                if (row.YarnTypeId) {
                                                    yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                                                }
                                                if (row.YarnColorId) {
                                                    yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                                                }
                                                yProgram = yProgram;
                                                row.Segment1ValueDesc = yType;
                                                row.Segment2ValueDesc = yarnCount;
                                                row.Segment3ValueDesc = yComposition;
                                                row.Segment4ValueDesc = shadeName;
                                                row.Segment5ValueDesc = yColor;

                                                row.Segment1ValueId = row.YarnTypeId;
                                                row.Segment5ValueId = row.YarnColorId;

                                                var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yColor, yProgram, yType);
                                                YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                                                YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                                                YCategory = YCategory.replaceAll('--', '-');
                                                row.YarnCategory = YCategory;
                                                row.ProductionYarnCategory = YCategory;
                                            }
                                            else {
                                                yarnCount = row.CalcYarnCount;
                                                if (row.YarnTypeId) {
                                                    yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                                                }
                                                if (row.YarnColorId) {
                                                    yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                                                }
                                                yProgram = yProgram;
                                                row.Segment1ValueDesc = yType;
                                                row.Segment2ValueDesc = yarnCount;
                                                row.Segment3ValueDesc = yComposition;
                                                row.Segment4ValueDesc = shadeName;
                                                row.Segment5ValueDesc = yColor;

                                                row.Segment1ValueId = row.YarnTypeId;
                                                row.Segment5ValueId = row.YarnColorId;

                                                var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yColor, yProgram, yType);
                                                YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                                                YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                                                YCategory = YCategory.replaceAll('--', '-');
                                                row.YarnCategory = YCategory;
                                                row.ProductionYarnCategory = YCategory;
                                            }
                                        }

                                        row.NetConsumption = ((row.BookingQty * row.YarnDistribution) / 100).toFixed(4);
                                        row.FinalNetConsumption = (row.NetConsumption * row.ConsumptionPerPcs).toFixed(4);
                                        if (row.FinalAllowancePer == "0") {
                                            row.RequiredYarnQty = row.NetConsumption;
                                        }
                                        else {
                                            if (row.FinalNetConsumption > 0) {
                                                row.RequiredYarnQty = (((row.NetConsumption * row.ConsumptionPerPcs)) + ((row.FinalAllowancePer * row.FinalNetConsumption) / 100)).toFixed(4);
                                            }
                                            else {
                                                row.RequiredYarnQty = (((row.BookingQty * row.YarnDistribution) / 100) + ((row.FinalAllowancePer * row.NetConsumption) / 100)).toFixed(4);
                                            }
                                        }

                                        var url = "";
                                        if (row.FinalYarnCount) {
                                            row.YarnCountId = row.FinalYarnCount;
                                            row.Segment2ValueId = row.FinalYarnCount;
                                        }
                                        else {
                                            url = "/api/getYarnCountId/" + row.CalcYarnCount;
                                            axios.get(url)
                                                .then(function (response) {
                                                    var data = response.data;
                                                    row.YarnCountId = data.YarnCountId;
                                                    row.Segment2ValueId = data.YarnCountId;
                                                })
                                                .catch(function () {
                                                    toastr.error(constants.LOAD_ERROR_MESSAGE);
                                                })
                                        }
                                        row.Segment5ValueId = row.YarnColorId;
                                        $tableEl.bootstrapTable('updateByUniqueId', row.Id, row);
                                    })
                                })
                                .catch(function (err) {
                                    //console.log(err);
                                });
                        },
                    }
                },
                {
                    field: "MachineGauge",
                    title: "Suggested MG"
                },
                {
                    field: "FinalMachineGauge",
                    title: "Production MG",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "MachineDia",
                    title: "Suggested MD"
                },
                {
                    field: "FinalMachineDia",
                    title: "Production MD",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "StitchLength",
                    title: "Suggested SL"
                },
                {
                    field: "FinalStitchLength",
                    title: "Production SL",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "YarnDistribution",
                    title: "Yarn Distribution (%)",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "NetConsumption",
                    title: "Net Consumption"
                },
                {
                    field: "DisplayUnitDesc",
                    title: "UOM"
                },
                {
                    field: "ConsumptionPerPcs",
                    title: "Consumption/Pcs",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "FinalNetConsumption",
                    title: "Total Net Consumption"
                },
                {
                    field: "DisplayUnitDescKG",
                    title: "UOM"
                },
                {
                    field: "AllowancePer",
                    title: "Allowance (%)",
                    width: 20
                },
                {
                    field: "FinalAllowancePer",
                    title: "Production Allowance (%)",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "RequiredYarnQty",
                    title: "Required Qty"
                },
                {
                    field: "DisplayUnitDescKG",
                    title: "UOM"
                }
            ],
            onEditableSave: function (field, row, oldValue, $el) {
                yProgram = yProgram;
                yComposition = row.YarnComposition;
                if (row.YarnSubProgramIds) {
                    var selectedValue = setMultiSelectValueByValueInBootstrapTableEditable(bookingAnalysisMaster.YarnSubProgramList, row.YarnSubProgramIds);
                    row.YarnSubProgramIds = selectedValue.id;

                    yarnCount = row.Segment2ValueDesc;
                    row.YarnCategory = (yarnCount ? yarnCount : "") + "-" + selectedValue.text;
                    if (row.YarnTypeId) {
                        yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                    }
                    if (row.YarnColorId) {
                        yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                    }
                    yProgram = yProgram;
                    row.Segment1ValueDesc = yType;
                    row.Segment2ValueDesc = yarnCount;
                    row.Segment3ValueDesc = yComposition;
                    row.Segment4ValueDesc = shadeName;
                    row.Segment5ValueDesc = yColor;

                    row.Segment1ValueId = row.YarnTypeId;
                    row.Segment5ValueId = row.YarnColorId;

                    var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yProgram, yType, yColor);
                    YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                    YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                    YCategory = YCategory.replaceAll('--', '-');
                    row.YarnCategory = YCategory + "-" + selectedValue.text;
                    row.ProductionYarnCategory = row.YarnCategory;
                }
                else {
                    if (row.FinalYarnCount && row.YarnColorId) {
                        yarnCount = row.Segment2ValueDesc;
                        if (row.YarnTypeId) {
                            yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                        }
                        if (row.YarnColorId) {
                            yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                        }
                        yProgram = yProgram;
                        row.Segment1ValueDesc = yType;
                        row.Segment2ValueDesc = yarnCount;
                        row.Segment3ValueDesc = yComposition;
                        row.Segment4ValueDesc = shadeName;
                        row.Segment5ValueDesc = yColor;

                        row.Segment1ValueId = row.YarnTypeId;
                        row.Segment5ValueId = row.YarnColorId;

                        var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yProgram, yType, yColor);
                        YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                        YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                        YCategory = YCategory.replaceAll('--', '-');
                        row.YarnCategory = YCategory;
                        row.ProductionYarnCategory = YCategory;
                    }
                    else {
                        if (row.YarnTypeId && row.YarnColorId) {
                            yarnCount = row.CalcYarnCount;
                            if (row.YarnTypeId) {
                                yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                            }
                            if (row.YarnColorId) {
                                yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                            }
                            yProgram = yProgram;
                            row.Segment1ValueDesc = yType;
                            row.Segment2ValueDesc = yarnCount;
                            row.Segment3ValueDesc = yComposition;
                            row.Segment4ValueDesc = shadeName;
                            row.Segment5ValueDesc = yColor;

                            row.Segment1ValueId = row.YarnTypeId;
                            row.Segment5ValueId = row.YarnColorId;

                            var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yProgram, yType, yColor);
                            YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                            YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                            YCategory = YCategory.replaceAll('--', '-');
                            row.YarnCategory = YCategory;
                            row.ProductionYarnCategory = YCategory;
                        }
                    }
                }

                row.NetConsumption = ((row.BookingQty * row.YarnDistribution) / 100).toFixed(4);
                row.FinalNetConsumption = (row.NetConsumption * row.ConsumptionPerPcs).toFixed(4);
                if (row.FinalAllowancePer == "0") {
                    row.RequiredYarnQty = row.NetConsumption;
                }
                else {
                    if (row.FinalNetConsumption > 0) {
                        row.RequiredYarnQty = (((row.NetConsumption * row.ConsumptionPerPcs)) + ((row.FinalAllowancePer * row.FinalNetConsumption) / 100)).toFixed(4);
                    }
                    else {
                        row.RequiredYarnQty = (((row.BookingQty * row.YarnDistribution) / 100) + ((row.FinalAllowancePer * row.NetConsumption) / 100)).toFixed(4);
                    }
                }

                var url = "";
                if (row.FinalYarnCount) {
                    row.YarnCountId = row.FinalYarnCount;
                    row.Segment2ValueId = row.FinalYarnCount;
                }
                else {
                    if (row.CalcYarnCount > 2) {
                        url = "/api/getYarnCountId/" + row.CalcYarnCount;
                        axios.get(url)
                            .then(function (response) {
                                var data = response.data;
                                row.YarnCountId = data.YarnCountId;
                                row.Segment2ValueId = data.YarnCountId;
                            })
                            .catch(function () {
                                toastr.error(constants.LOAD_ERROR_MESSAGE);
                            })
                    }
                }
                row.Segment5ValueId = row.YarnColorId;
                $tableEl.bootstrapTable('load', $el);
            }
        });
    }

    function getChildCollarData() {
        var url = "";
        //var id = $formEl.find("#Id").val();
        //if (id > 0)
        //    url = "/api/bookinganalysischildCollar/" + id;
        //else {
        //    url = "/api/newbookinganalysischildCollar/" + $formEl.find("#CollarBookingID").val();
        //}

        url = "/api/newbookinganalysischildCollar/" + $formEl.find("#CollarBookingID").val();

        axios.get(url)
            .then(function (response) {
                bookingAnalysisChildCollarList = response.data;
                $tblBookingAnalysisChildCollar.bootstrapTable('load', response.data);
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }

    function initBookingAnalysisChildYarnCuff($tableEl, bookingChildId, data) {
        $tableEl.bootstrapTable({
            showFooter: true,
            data: data,
            uniqueId: 'Id',
            rowStyle: function (row, index) {
                if (row.EntityState == 8)
                    return { classes: 'deleted-row' };

                return "";
            },
            columns: [
                {
                    title: 'Actions',
                    align: 'center',
                    width: 100,
                    formatter: function () {
                        return [
                            '<span class="btn-group">',
                            '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete Item">',
                            '<i class="fa fa-remove"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    footerFormatter: function () {
                        return [
                            '<span class="btn-group">',
                            '<button class="btn btn-success btn-xs edit" onclick="return addNewChildRowCuffBA(event, ' + bookingChildId + ')" title="Add">',
                            '<i class="fa fa-plus"></i>',
                            ' Add',
                            '</button>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            this.data[index].EntityState = 8;
                            var $target = $(e.target);
                            $target.closest("tr").addClass('deleted-row');
                        }
                    }
                },
                {
                    field: "YarnTypeId",
                    title: "Yarn Type",
                    align: "left",
                    editable: {
                        source: bookingAnalysisMaster.YarnTypeList,
                        type: 'select2',
                        showbuttons: false,
                        select2: { width: 100, placeholder: 'Yarn Type' }
                    }
                },
                {
                    field: "YarnComposition",
                    title: "Yarn Composition",
                    width: 100,
                },
                {
                    field: "YarnColorId",
                    title: "Yarn Color",
                    width: 80,
                    editable: {
                        type: 'select2',
                        title: 'Select a Color',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: bookingAnalysisMaster.YarnColorList,
                        select2: { width: 100, placeholder: 'Yarn Color' }
                    }
                },

                {
                    field: "YarnSubProgramIds",
                    title: "Yarn Sub Program",
                    editable: {
                        type: 'select2',
                        showbuttons: true,
                        source: bookingAnalysisMaster.YarnSubProgramList,
                        select2: { width: 200, height: 300, multiple: true, placeholder: 'Select Sub Program' }
                    }
                },
                {
                    field: "YarnCategory",
                    title: "Yarn Category",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "YarnSpecification",
                    title: "Yarn Specification",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "YarnShadeCode",
                    title: "Shade Code",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "IsYD",
                    title: "YD",
                    placeholder: 'YD',
                    checkbox: true
                },
                {
                    field: "CalcYarnCount",
                    title: "Suggested YC",
                    width: 20,
                    visible: !isCustomCheckedYarnCount
                },
                {
                    field: "FinalYarnCount",
                    title: "Production YC",
                    width: 20,
                    formatter: function (value, row, index, field) {
                        return ['<span class="btn-group">',
                            '<a href="javascript:void(0)" class="editable-link edit">' + row.Segment2ValueDesc + '</a>',
                            '</span>'].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            if (!row.YarnTypeId) return toastr.error("Yarn Type is not selected");
                            var url = "/api/selectoption/constructionAndYarnTypeWiseYarnCountLists/" + row.YarnTypeId;
                            axios.get(url)
                                .then(function (response) {
                                    var yarnCountList = convertToSelectOptions(response.data);
                                    showBootboxSelectPrompt("Select Yarn Count", yarnCountList, "", function (result) {
                                        if (!result)
                                            return toastr.warning("You didn't selected any Yarn Count.");

                                        var selectedYarnCount = yarnCountList.find(function (el) { return el.value === result })
                                        row.FinalYarnCount = result;
                                        row.Segment2ValueDesc = selectedYarnCount.text;

                                        yComposition = row.YarnComposition;
                                        if (row.YarnSubProgramIds) {
                                            var selectedValue = setMultiSelectValueByValueInBootstrapTableEditable(bookingAnalysisMaster.YarnSubProgramList, row.YarnSubProgramIds);
                                            row.YarnSubProgramIds = selectedValue.id;

                                            yarnCount = row.Segment2ValueDesc;
                                            row.YarnCategory = (yarnCount ? yarnCount : "") + "-" + selectedValue.text;
                                            if (row.YarnTypeId) {
                                                yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                                            }
                                            if (row.YarnColorId) {
                                                yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                                            }
                                            yProgram = yProgram;
                                            row.Segment1ValueDesc = yType;
                                            row.Segment2ValueDesc = yarnCount;
                                            row.Segment3ValueDesc = yComposition;
                                            row.Segment4ValueDesc = shadeName;
                                            row.Segment5ValueDesc = yColor;

                                            var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yColor, yProgram, yType, ySubProgram);
                                            YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                                            YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                                            YCategory = YCategory.replaceAll('--', '-');
                                            row.YarnCategory = YCategory + "-" + selectedValue.text;
                                            row.ProductionYarnCategory = row.YarnCategory;
                                        }
                                        else {
                                            if (row.FinalYarnCount) {
                                                yarnCount = row.Segment2ValueDesc;
                                                if (row.YarnTypeId) {
                                                    yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                                                }
                                                if (row.YarnColorId) {
                                                    yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                                                }
                                                yProgram = yProgram;
                                                row.Segment1ValueDesc = yType;
                                                row.Segment2ValueDesc = yarnCount;
                                                row.Segment3ValueDesc = yComposition;
                                                row.Segment4ValueDesc = shadeName;
                                                row.Segment5ValueDesc = yColor;

                                                var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yColor, yProgram, yType);
                                                YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                                                YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                                                YCategory = YCategory.replaceAll('--', '-');
                                                row.YarnCategory = YCategory;
                                                row.ProductionYarnCategory = YCategory;
                                            }
                                            else {
                                                yarnCount = row.CalcYarnCount;
                                                if (row.YarnTypeId) {
                                                    yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                                                }
                                                if (row.YarnColorId) {
                                                    yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                                                }
                                                yProgram = yProgram;
                                                row.Segment1ValueDesc = yType;
                                                row.Segment2ValueDesc = yarnCount;
                                                row.Segment3ValueDesc = yComposition;
                                                row.Segment4ValueDesc = shadeName;
                                                row.Segment5ValueDesc = yColor;

                                                var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yColor, yProgram, yType);
                                                YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                                                YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                                                YCategory = YCategory.replaceAll('--', '-');
                                                row.YarnCategory = YCategory;
                                                row.ProductionYarnCategory = YCategory;
                                            }
                                        }

                                        row.NetConsumption = ((row.BookingQty * row.YarnDistribution) / 100).toFixed(4);
                                        row.FinalNetConsumption = (row.NetConsumption * row.ConsumptionPerPcs).toFixed(4);
                                        if (row.FinalAllowancePer == "0") {
                                            row.RequiredYarnQty = row.NetConsumption;
                                        }
                                        else {
                                            if (row.FinalNetConsumption > 0) {
                                                row.RequiredYarnQty = (((row.NetConsumption * row.ConsumptionPerPcs)) + ((row.FinalAllowancePer * row.FinalNetConsumption) / 100)).toFixed(4);
                                            }
                                            else {
                                                row.RequiredYarnQty = (((row.BookingQty * row.YarnDistribution) / 100) + ((row.FinalAllowancePer * row.NetConsumption) / 100)).toFixed(4);
                                            }
                                        }

                                        var url = "";
                                        if (row.FinalYarnCount) {
                                            row.YarnCountId = row.FinalYarnCount;
                                            row.Segment2ValueId = row.FinalYarnCount;
                                        }
                                        else {
                                            url = "/api/getYarnCountId/" + row.CalcYarnCount;
                                            axios.get(url)
                                                .then(function (response) {
                                                    var data = response.data;
                                                    row.YarnCountId = data.YarnCountId;
                                                    row.Segment2ValueId = data.YarnCountId;
                                                })
                                                .catch(function () {
                                                    toastr.error(constants.LOAD_ERROR_MESSAGE);
                                                })
                                        }
                                        row.Segment5ValueId = row.YarnColorId;
                                        $tableEl.bootstrapTable('updateByUniqueId', row.Id, row);
                                    })
                                })
                                .catch(function (err) {
                                    //console.log(err);
                                });
                        },
                    }
                },
                {
                    field: "MachineGauge",
                    title: "Suggested MG"
                },
                {
                    field: "FinalMachineGauge",
                    title: "Production MG",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "MachineDia",
                    title: "Suggested MD"
                },
                {
                    field: "FinalMachineDia",
                    title: "Production MD",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "StitchLength",
                    title: "Suggested SL"
                },
                {
                    field: "FinalStitchLength",
                    title: "Production SL",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "YarnDistribution",
                    title: "Yarn Distribution (%)",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "NetConsumption",
                    title: "Net Consumption"
                },
                {
                    field: "DisplayUnitDesc",
                    title: "UOM"
                },
                {
                    field: "ConsumptionPerPcs",
                    title: "Consumption/Pcs",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "FinalNetConsumption",
                    title: "Total Net Consumption"
                },
                {
                    field: "DisplayUnitDescKG",
                    title: "UOM"
                },
                {
                    field: "AllowancePer",
                    title: "Allowance (%)",
                    width: 20
                },
                {
                    field: "FinalAllowancePer",
                    title: "Production Allowance (%)",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "RequiredYarnQty",
                    title: "Required Qty"
                },
                {
                    field: "DisplayUnitDescKG",
                    title: "UOM"
                }
            ],
            onEditableSave: function (field, row, oldValue, $el) {
                yProgram = yProgram;
                yComposition = row.YarnComposition;
                if (row.YarnSubProgramIds) {
                    var selectedValue = setMultiSelectValueByValueInBootstrapTableEditable(bookingAnalysisMaster.YarnSubProgramList, row.YarnSubProgramIds);
                    row.YarnSubProgramIds = selectedValue.id;

                    yarnCount = row.Segment2ValueDesc;
                    row.YarnCategory = (yarnCount ? yarnCount : "") + "-" + selectedValue.text;
                    if (row.YarnTypeId) {
                        yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                    }
                    if (row.YarnColorId) {
                        yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                    }
                    yProgram = yProgram;
                    row.Segment1ValueDesc = yType;
                    row.Segment2ValueDesc = yarnCount;
                    row.Segment3ValueDesc = yComposition;
                    row.Segment4ValueDesc = shadeName;
                    row.Segment5ValueDesc = yColor;

                    var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yColor, yProgram, yType, ySubProgram);
                    YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                    YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                    YCategory = YCategory.replaceAll('--', '-');
                    row.YarnCategory = YCategory + "-" + selectedValue.text;
                    row.ProductionYarnCategory = row.YarnCategory;
                }
                else {
                    if (row.FinalYarnCount && row.YarnColorId) {
                        yarnCount = row.Segment2ValueDesc;
                        if (row.YarnTypeId) {
                            yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                        }
                        if (row.YarnColorId) {
                            yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                        }
                        yProgram = yProgram;
                        row.Segment1ValueDesc = yType;
                        row.Segment2ValueDesc = yarnCount;
                        row.Segment3ValueDesc = yComposition;
                        row.Segment4ValueDesc = shadeName;
                        row.Segment5ValueDesc = yColor;

                        var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yColor, yProgram, yType);
                        YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                        YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                        YCategory = YCategory.replaceAll('--', '-');
                        row.YarnCategory = YCategory;
                        row.ProductionYarnCategory = YCategory;
                    }
                    else {
                        if (row.YarnTypeId && row.YarnColorId) {
                            yarnCount = row.CalcYarnCount;
                            if (row.YarnTypeId) {
                                yType = bookingAnalysisMaster.YarnTypeList.find(function (el) { return el.id == row.YarnTypeId }).text;
                            }
                            if (row.YarnColorId) {
                                yColor = bookingAnalysisMaster.YarnColorList.find(function (el) { return el.id == row.YarnColorId }).text;
                            }
                            yProgram = yProgram;
                            row.Segment1ValueDesc = yType;
                            row.Segment2ValueDesc = yarnCount;
                            row.Segment3ValueDesc = yComposition;
                            row.Segment4ValueDesc = shadeName;
                            row.Segment5ValueDesc = yColor;

                            var YCategory = GetYarnCategoryBookingAnalysis(yComposition, yarnCount, yColor, yProgram, yType);
                            YCategory = YCategory != '' ? YCategory.replaceAll('  ', ' ') : YCategory;
                            YCategory = YCategory.length > 0 ? YCategory.charAt(YCategory.length - 1) == '-' ? YCategory.slice(0, -1) + '' : YCategory : YCategory;
                            YCategory = YCategory.replaceAll('--', '-');
                            row.YarnCategory = YCategory;
                            row.ProductionYarnCategory = YCategory;
                        }
                    }
                }

                row.NetConsumption = ((row.BookingQty * row.YarnDistribution) / 100).toFixed(4);
                row.FinalNetConsumption = (row.NetConsumption * row.ConsumptionPerPcs).toFixed(4);
                if (row.FinalAllowancePer == "0") {
                    row.RequiredYarnQty = row.NetConsumption;
                }
                else {
                    if (row.FinalNetConsumption > 0) {
                        row.RequiredYarnQty = (((row.NetConsumption * row.ConsumptionPerPcs)) + ((row.FinalAllowancePer * row.FinalNetConsumption) / 100)).toFixed(4);
                    }
                    else {
                        row.RequiredYarnQty = (((row.BookingQty * row.YarnDistribution) / 100) + ((row.FinalAllowancePer * row.NetConsumption) / 100)).toFixed(4);
                    }
                }

                var url = "";
                if (row.FinalYarnCount) {
                    row.YarnCountId = row.FinalYarnCount;
                    row.Segment2ValueId = row.FinalYarnCount;
                }
                else {
                    if (row.CalcYarnCount > 2) {
                        url = "/api/getYarnCountId/" + row.CalcYarnCount;
                        axios.get(url)
                            .then(function (response) {
                                var data = response.data;
                                row.YarnCountId = data.YarnCountId;
                                row.Segment2ValueId = data.YarnCountId;
                            })
                            .catch(function () {
                                toastr.error(constants.LOAD_ERROR_MESSAGE);
                            })
                    }
                }
                row.Segment5ValueId = row.YarnColorId;
                $tableEl.bootstrapTable('load', $el);
            }
        });
    }

    function getChildCuffData() {
        var url = "";
        //var id = $formEl.find("#Id").val();
        //if (id > 0)
        //    url = "/api/bookinganalysischildCuff/" + id;
        //else
        //{
        //    url = "/api/newbookinganalysischildCuff/" + $formEl.find("#CuffBookingID").val();
        //}

        url = "/api/newbookinganalysischildCuff/" + $formEl.find("#CuffBookingID").val();

        axios.get(url)
            .then(function (response) {
                bookingAnalysisChildCuffList = response.data;
                $tblBookingAnalysisChildCuff.bootstrapTable('load', response.data);
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }

    function getChildDataSample(processStatus) {
        var url = "";
        var id = $formEl.find("#Id").val();
        if (id && processStatus != "Pending") {
            url = "/api/bookinganalysischildSample/" + id;
        }
        else {
            url = "/api/newbookinganalysischildSample/" + $formEl.find("#BookingId").val();
        }

        axios.get(url)
            .then(function (response) {
                bookingAnalysisMaster.Childs = response.data;
                $tblBookingAnalysisChildFabric.bootstrapTable('load', response.data);
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }

    function getChildDataDyeing(processStatus) {
        var url = "";
        var id = $formEl.find("#Id").val();
        if (id && processStatus != "Pending")
            url = "/api/bookinganalysischildDyeing/" + id;
        else
            url = "/api/newbookinganalysischild/" + $formEl.find("#BookingId").val();

        axios.get(url)
            .then(function (response) {
                bookingAnalysisChildDyeingList = response.data;
                $formEl.find("#tblFabricBookingAnalysisChildDyeing").bootstrapTable('load', response.data);
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }

    function getChildDataFinishing(processStatus) {
        var url = "";
        var id = $formEl.find("#Id").val();
        if (id && processStatus != "Pending")
            url = "/api/bookinganalysischildFinishing/" + id;
        else
            url = "/api/newbookinganalysischild/" + $formEl.find("#BookingId").val();

        axios.get(url)
            .then(function (response) {
                bookingAnalysisChildFinishingList = response.data;
                $formEl.find("#tblFabricBookingAnalysisChildFinishing").bootstrapTable('load', response.data);
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }

    function getFabricCompositionLists() {
        var queryParams = $.param(yarnCompositionTableParams);
        var url = "/api/YarnCompositionlists" + "?" + queryParams;
        axios.get(url)
            .then(function (response) {
                $("#tblYarnComposition").bootstrapTable('load', response.data);
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }

    function getFabricCompositionListsCollar() {
        var queryParams = $.param(yarnCompositionTableParams);
        var url = "/api/YarnCompositionlists" + "?" + queryParams;
        axios.get(url)
            .then(function (response) {
                $("#tblYarnCompositionCollar").bootstrapTable('load', response.data);
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }

    function getFabricCompositionListsCuff() {
        var queryParams = $.param(yarnCompositionTableParams);
        var url = "/api/YarnCompositionlists" + "?" + queryParams;
        axios.get(url)
            .then(function (response) {
                $("#tblYarnCompositionCuff").bootstrapTable('load', response.data);
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#Id").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    window.addNewChildRowBA = function (e, bookingChildId) {
        childEl = $("#tblFabricBookingAnalysisChildYarn-" + bookingChildId);
        getFabricCompositionLists();
        $("#modal-child").modal('show');
    }

    window.addNewChildRowCollarBA = function (e, bookingChildId) {
        childEl = $("#tblFabricBookingAnalysisChildYarnCollar-" + bookingChildId);
        getFabricCompositionListsCollar();
        $("#modal-child-Collar").modal('show');
    }

    window.addNewChildRowCuffBA = function (e, bookingChildId) {
        childEl = $("#tblFabricBookingAnalysisChildYarnCuff-" + bookingChildId);
        getFabricCompositionListsCuff();
        $("#modal-child-Cuff").modal('show');
    }

    function getYarnAdditionalBookingReason() {
        axios.get("/api/selectoption/additionalBookingReason")
            .then(function (response) {
                yarnAdditionalBookingLists = response.data;
                initSelect2($formEl.find("#ReasonIds"), response.data);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function resetBookingAnalysisTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function backToListBookingBI() {
        resetForm();
        $divTblEl.fadeIn();
        $("#divEditBookingAnalysisMaster").fadeOut();
        $("#divEditBookingAnalysisMaster1").fadeOut();

        $formEl.find("#btnSaveBookingAnalysis").fadeOut();
        $formEl.find("#btnApprovedBookingAnalysis").fadeOut();
        $formEl.find("#btnRejectBookingAnalysis").fadeOut();
        $formEl.find("#btnAcknowledgeBookingAnalysis").fadeOut();
        $formEl.find("#btnProposeBookingAnalysis").fadeOut();
        $formEl.find("#divEditBookingAnalysisMasterCollar").fadeOut();
        $formEl.find("#divEditBookingAnalysisMasterCuff").fadeOut();
        $formEl.find("#divAdditionalBooking").fadeOut();

        getMasterData();
    }

    function getTextileProcess() {
        initTblTextileProcess(false);
        axios.get("/api/textileprocess")
            .then(function (response) {
                processMasterList = response.data;
                $formEl.find("#tblTextileProcessMaster").bootstrapTable('load', response.data);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function getBookingAnalysisProcess(analysisId) {
        initTblTextileProcess(true);
        axios.get("/api/bookinganalysisprocess/" + analysisId)
            .then(function (response) {
                processMasterList = response.data;
                if (processMasterList[0].ProcessStatus == "Completed")
                    $("#btnProposeBookingAnalysis").prop("disabled", false);
                $("#tblTextileProcessMaster").bootstrapTable('load', response.data);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function getRowStyle(bookingType) {
        var st;
        switch (bookingType) {
            case bookingTypeConstants.SAMPLE:
                st = { classes: "bg-primary" };
                break;
            case bookingTypeConstants.REVISED:
                st = { classes: "bg-info" };
                break;
            default:
                st = {};
                break;
        }

        return st;
    }

    function initYarnBookingRevisionHistoryMaster() {
        $("#tblBookingRevisionHistoryMaster").bootstrapTable('destroy');
        $("#tblBookingRevisionHistoryMaster").bootstrapTable({
            columns: [
                {
                    title: 'Actions',
                    align: 'center',
                    width: 50,
                    formatter: function () {
                        return [
                            '<span class="btn-group">',
                            '<a class="btn btn-default btn-xs edit" href="javascript:void(0)" title="View Yarn Composition Items">',
                            '<i class="fa fa-eye"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            initTblFabricBookingAnalysisChildRevisionHistory();
                            getChildDataBookingHistory(row.BookingId, row.RevisionNo);
                        }
                    }
                },
                {
                    field: "RevisionNo",
                    title: "Revision No",
                    filterControl: "input"
                },
                {
                    field: "RevisionDate",
                    title: "Revision Date",
                    filterControl: "input"
                },
                {
                    field: "RevisionReasons",
                    title: "Revision Reason",
                    filterControl: "input"
                }
            ]
        });
    }

    function getBookingRevisionHistoryMasterData(bookingId) {
        var url = "";
        url = "/api/bookingRevisionHistoryMasterData/" + bookingId;
        axios.get(url)
            .then(function (response) {
                bookingHistoryMasterList = response.data;
                $formEl.find("#tblBookingRevisionHistoryMaster").bootstrapTable('load', response.data);
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }

    function initTblFabricBookingAnalysisChildRevisionHistory() {
        $formEl.find("#tblFabricBookingAnalysisChildRevisionHistory").bootstrapTable('destroy').bootstrapTable({
            uniqueId: 'Id',
            columns: [
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
                    title: "Dyeing Type",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "Finishing",
                    title: "Finishing",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "Washing",
                    title: "Washing",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "YarnType",
                    title: "Yarn Type",
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
                    field: "Remarks",
                    title: "Instruction",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "AOP",
                    title: "AOP",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "UsesIn",
                    title: "Uses In",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "BookingQty",
                    title: "Booking Qty",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "DisplayUnitDesc",
                    title: "Booking UOM",
                    align: "left",
                    filterControl: "input"
                }
            ]
        });
    }

    function getChildDataBookingHistory(bookingId, revisionNo) {
        var url = "";
        url = "/api/newbookinganalysischildRevisionHistory/" + bookingId + "/" + revisionNo;
        axios.get(url)
            .then(function (response) {
                bookingAnalysisChildRevisionHistoryList = response.data;
                $formEl.find("#tblFabricBookingAnalysisChildRevisionHistory").bootstrapTable('load', response.data);
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }

    function save(e) {
        e.preventDefault();
        var data = formDataToJson($formEl.serializeArray());
        var hasChildYarnCount = 0;
        $.each(bookingAnalysisMaster.Childs, function (i, child) {
            if (child.ChildYarns.length)
                hasChildYarnCount++;
        });
        data["Childs"] = bookingAnalysisMaster.Childs;

        if (hasChildYarnCount > 0) {
            processMasterList[0].CompletionStatus = "Partially Completed";
            if (hasChildYarnCount == bookingAnalysisMaster.Childs.length)
                processMasterList[0].CompletionStatus = "Completed";
        }
        data["ProcessChilds"] = processMasterList;

        axios.post("/api/bookinganalysis", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToListBookingBI();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();
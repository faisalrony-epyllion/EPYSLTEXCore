(function () {
    var menuId, pageName;
    var toolbarId, pageId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, $modalPlanningEl, $tblStockInfoEl, tblStockInfoId;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var status,
        isIssuePage = false,
        isApprovePage = false;

    var _childRackBins = [];
    var _selectedRnDReqChildID = 0;
    var _maxYRICRBId = 999;
    var _maxKYIssueChildID = 999;
    var _maxIssueChildID = 999;

    var masterData;
    var addItem = [];
    var reqType;
    var selectedIndex;

    $(function () {


        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        $tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $modalPlanningEl = $("#modalPlanning" + pageId);
        tblStockInfoId = pageConstants.STOCK_INFO_PREFIX + pageId;
        //status = statusConstants.ACKNOWLEDGE; // Our default status here is awaiting for acknowledge
        //initMasterTable();
        //getMasterTableData();


        isIssuePage = convertToBoolean($(`#${pageId}`).find("#IssuePage").val());
        isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());

        if (isIssuePage) {
            //$toolbarEl.find("#btnPendingAcknowledgeList").show();
            $toolbarEl.find("#btnPenidngList,#btnCompletedList").show();
            $toolbarEl.find("#btnPendingApproveList,#btnApprovedList,#btnRejectList").show();

            $formEl.find("#btnSave,#btnAcknowledge,#btnUnAcknowledge").show();
            $formEl.find("#btnApprove,#btnReject").hide();

            //toggleActiveToolbarBtn($toolbarEl.find("#btnPendingAcknowledgeList"), $toolbarEl);

            //status = statusConstants.ACKNOWLEDGE;
            status = statusConstants.PENDING;
            isEditable = false;

            initMasterTable();
            getMasterTableData();
        }
        else if (isApprovePage) {
            $toolbarEl.find("#btnPendingApproveList,#btnApprovedList,#btnRejectList").show();
            //$toolbarEl.find("#btnPendingAcknowledgeList").hide();
            $toolbarEl.find("#btnPenidngList,#btnCompletedList").hide();

            $formEl.find("#btnApprove,#btnReject").show();
            $formEl.find("#btnSave,#btnAcknowledge,#btnUnAcknowledge").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingApproveList"), $toolbarEl);

            status = statusConstants.AWAITING_PROPOSE;
            isEditable = true;

            initMasterTable();
            getMasterTableData();
        }
        //Issue Part List
        //$toolbarEl.find("#btnPendingAcknowledgeList").on("click", function (e) {
        //    e.preventDefault();
        //    toggleActiveToolbarBtn(this, $toolbarEl);
        //    resetTableParams();
        //    status = statusConstants.ACKNOWLEDGE;
        //    initMasterTable();
        //    getMasterTableData();
        //});

        $toolbarEl.find("#btnPenidngList").on("click", function (e) {
            $toolbarEl.find("#btnExcelReport").hide();

            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnCompletedList").on("click", function (e) {
            $toolbarEl.find("#btnExcelReport").show();

            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            initMasterTable();
            getMasterTableData();
        });

        //Approve Part List
        $toolbarEl.find("#btnPendingApproveList").on("click", function (e) {
            $toolbarEl.find("#btnExcelReport").hide();

            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.AWAITING_PROPOSE;
            initMasterTable();
            getMasterTableData();
        });
        $toolbarEl.find("#btnApprovedList").on("click", function (e) {
            $toolbarEl.find("#btnExcelReport").hide();

            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.APPROVED;
            initMasterTable();
            getMasterTableData();
        });
        $toolbarEl.find("#btnRejectList").on("click", function (e) {
            $toolbarEl.find("#btnExcelReport").hide();

            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.REJECT;
            initMasterTable();
            getMasterTableData();
        });

        //$toolbarEl.find("#btnRefreshList").on("click", function (e) {
        //    e.preventDefault();
        //    $tblMasterEl.refresh();
        //});

        $formEl.find("#btnSelectMachine").on("click", function (e) {
            e.preventDefault();
            var reqMasterID = $formEl.find("#RnDReqMasterID").val();
            //var api = "/api/yarn-rnd-issues/pending/"+reqMasterID;
            var finder = new commonFinder({
                title: "Item List",
                pageId: pageId,
                //data: masterData.Childs,
                height: 220,
                modalSize: "modal-lg",
                apiEndPoint: "/api/yarn-rnd-issues/pending/" + reqMasterID,
                fields: "Segment1ValueDesc,Segment2ValueDesc,Segment3ValueDesc,Segment4ValueDesc,Segment5ValueDesc,Segment6ValueDesc,Segment7ValueDesc,ShadeCode,UOM,ReqQty,ReqCone,PhysicalCount,LotNo,YarnBrandID,IssueQty,IssueCone,YarnBrand",
                widths: "20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20",
                headerTexts: "Composition,Yarn Type,Process,Sub Process,Quality Parameter,Count,No of Ply,Shade,UOM,Req Qty,Req Cone,Physical Count,Lot No, Spinner, Issue Qty, Issue Cone, Yarn Brand",
                isMultiselect: true,
                primaryKeyColumn: "RnDReqChildID",
                allowPaging: false,
                autofitColumns: true,
                onMultiselect: function (selectedRecords) {
                    finder.hideModal();
                    selectedRecords.map(x => {
                        var index = masterData.Childs.findIndex(y => y.RnDReqChildID == x.RnDReqChildID);
                        if (index < 0) {
                            masterData.Childs.push(x);
                        }
                    });
                    initChildTable(masterData.Childs);
                }
            });
            finder.showModal();
        });

        $formEl.find("#btnSave").click(save);
        $formEl.find("#btnOk").click(function (e) {
            
            var hasErrorRack = false;
            if (_selectedRnDReqChildID > 0) {
                var rackList = DeepClone($tblStockInfoEl.getCurrentViewRecords());
                var childList = masterData.Childs;//DeepClone($tblChildEl.getCurrentViewRecords());

                var totalReqQty = parseFloat($formEl.find(".spnStockInfoBasicInfo").attr("TotalReqQty"));

                var maxQty = 0;
                if (reqType == "Bulk") {
                    maxQty = parseFloat($formEl.find(".spnStockInfoBasicInfo").attr("TotalAllocatedQty"));
                } else if (reqType == "RnD") {
                    maxQty = parseFloat($formEl.find(".spnStockInfoBasicInfo").attr("TotalSampleQty"));
                }

                var indexF = childList.findIndex(x => x.RnDReqChildID == _selectedRnDReqChildID);
                if (indexF > -1) {
                    var totalIssueQty = 0;
                    var totalIssueQtyCone = 0;
                    var totalIssueQtyCarton = 0;


                    //This loop only for validation
                    for (var iRack = 0; iRack < rackList.length; iRack++) {
                        var rack = rackList[iRack];

                        /*if (parseFloat(rack.IssueCartoon) > parseFloat(rack.NoOfCartoon)) {
                            hasErrorRack = true;
                            toastr.error("Issue cartoon (" + rack.IssueCartoon + ") cannot be greater then no of cartoon (" + rack.NoOfCartoon + ")");
                            break;
                        }
                        if (parseFloat(rack.IssueQtyCone) > parseFloat(rack.NoOfCone)) {
                            hasErrorRack = true;
                            toastr.error("Issue cone (" + rack.IssueQtyCone + ") cannot be greater then no of cone (" + rack.NoOfCone + ")");
                            break;
                        }
                        if (parseFloat(rack.IssueQtyKg) > parseFloat(rack.ReceiveQty)) {
                            hasErrorRack = true;
                            toastr.error("Issue qty (" + rack.IssueQtyKg + ") cannot be greater then allocated qty (" + rack.ReceiveQty + ")");
                            break;
                        }
                        */
                        totalIssueQty += rack.IssueQtyKg;
                        totalIssueQtyCone += rack.IssueQtyCone;
                        totalIssueQtyCarton += rack.IssueCartoon;
                    }

                    //if (totalIssueQty > totalReqQty) {
                    //    hasErrorRack = true;
                    //    toastr.error(`Total issue qty ${totalIssueQty} cannot be greater than requisition qty ${totalReqQty}`);
                    //    return false;
                    //}
                    //if (totalIssueQty > maxQty) {
                    //    hasErrorRack = true;
                    //    var qtyType = reqType == "Bulk" ? "allocation" : "sample";
                    //    toastr.error(`Total issue qty ${totalIssueQty} cannot be greater than ${qtyType} qty ${maxQty}`);
                    //    return false;
                    //}

                    if (hasErrorRack) return false;
                    //End = This loop only for validation

                    //Set value into global variables
                    for (var iRack = 0; iRack < rackList.length; iRack++) {
                        var rack = rackList[iRack];

                        //-------------------------- START Add data to _childRackBins --------------------------

                        var indexFRB = _childRackBins.findIndex(x => x.RnDReqChildID == _selectedRnDReqChildID);
                        if (indexFRB == -1) {
                            _childRackBins.push({
                                RnDReqChildID: _selectedRnDReqChildID,
                                ChildRackBins: []
                            });
                            indexFRB = _childRackBins.findIndex(x => x.RnDReqChildID == _selectedRnDReqChildID);
                            if (indexFRB > -1) {
                                if (rack.YRICRBId == 0) rack.YRICRBId = _maxYRICRBId++;
                                _childRackBins[indexFRB].ChildRackBins.push(rack);
                            }
                        } else {
                            var indexC = _childRackBins[indexFRB].ChildRackBins.findIndex(x => x.ChildRackBinID == rack.ChildRackBinID);
                            if (indexC == -1) {
                                if (rack.YRICRBId == 0) rack.YRICRBId = _maxYRICRBId++;
                                _childRackBins[indexFRB].ChildRackBins.push(rack);
                            } else {
                                _childRackBins[indexFRB].ChildRackBins[indexC] = rack;
                            }
                        }
                        //-------------------------- END Add data to _childRackBins --------------------------
                    }
                    //End = Set value into global variables

                    if (!hasErrorRack) {
                        childList[indexF].IssueQty = totalIssueQty;
                        childList[indexF].IssueCone = totalIssueQtyCone;
                        childList[indexF].IssueQtyCarton = totalIssueQtyCarton;

                        masterData.Childs = childList;
                        $tblChildEl.bootstrapTable('load', childList);
                    }
                }
            }

            if (!hasErrorRack) $modalPlanningEl.modal('hide');
            //--------------------------------------------
        });

        $formEl.find("#btnAcknowledge").click(acknowledgeMRS);
        $formEl.find("#btnUnAcknowledge").click(function (e) {
            e.preventDefault();
            bootbox.prompt("Enter your UnAcknowledge reason:", function (result) {
                if (!result) {
                    return toastr.error("UnAcknowledge reason is required.");
                }
                var id = $formEl.find("#RnDReqMasterID").val();
                var unAcknowledgeReason = result;
                axios.post(`/api/yarn-rnd-requisition/unacknowledge/${id}/${unAcknowledgeReason}`)
                    .then(function () {
                        toastr.success("Rejected successfully.");
                        backToList();
                    })
                    .catch(showResponseError);
            });

        });

        $formEl.find("#btnApprove").click(Approve);
        $formEl.find("#btnReject").click(Reject);
        $formEl.find("#btnCancel").on("click", backToList);
        $toolbarEl.find("#btnPenidngList").click();

        $toolbarEl.find("#btnExcelReport").click(function () {
            ch_generateAndExportExcel(pageId, 1, null);
        });
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
                    title: "",
                    width: 100,
                    minWidth: 100,
                    maxWidth: 100,
                    field: "",
                    align: "center",
                    cellStyle: function () { return { classes: 'w-150' } },
                    formatter: function (value, row, index, field) {
                        var template = "";
                        if (status === statusConstants.PENDING) {
                            template = `<span class="btn-group">
                                            <a class="btn btn-xs btn-default add" href="javascript:void(0)" title="Yarn R&D Issue">
                                                <i class="fa fa-plus" aria-hidden="true"></i>
                                            </a>
                                            <a class="btn btn-xs btn-default viewreport" href="javascript:void(0)" title="View Report">
                                                <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                            </a>
                                        </span>`;
                        }
                        else if (status === statusConstants.COMPLETED) {
                            template = `<span class="btn-group">
                                            <a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Yarn R&D Issue">
                                                <i class="fa fa-edit" aria-hidden="true"></i>
                                            </a>
                                            <a class="btn btn-xs btn-default viewreport" href="javascript:void(0)" title="View Report">
                                                <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                            </a>
                                            <a class="btn btn-xs btn-default minReport" href="javascript:void(0)" title="MIN Report">
                                                <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                            </a>
                                    </span>`;
                        }
                        else if (status === statusConstants.ACKNOWLEDGE) {
                            template =
                                `<span class="btn-group">
                                <a class="btn btn-xs btn-default acknowledge" href="javascript:void(0)" title="Acknowledge MRS">
                                    <i class="fa fa-check" aria-hidden="true"></i>
                                </a>
                                <a class="btn btn-xs btn-default unacknowledge" href="javascript:void(0)" title="UnAcknowledge MRS">
                                    <i class="fa fa-times" aria-hidden="true"></i>
                                </a>
                                <a class="btn btn-xs btn-default mrsDetails" href="javascript:void(0)" title="View Details">
                                    <i class="fa fa-edit" aria-hidden="true"></i>
                                </a>
                                <a class="btn btn-xs btn-default viewreport" href="javascript:void(0)" title="View Report">
                                    <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                 </a>
                                <a class="btn btn-xs btn-default minReport" href="javascript:void(0)" title="MIN Report">
                                        <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                    </a>
                            </span>`;
                        }
                        else if (status === statusConstants.AWAITING_PROPOSE) {
                            template =
                                `<span class="btn-group"> 
                                <a class="btn btn-xs btn-default PendingApprove" href="javascript:void(0)" title="View Details">
                                    <i class="fa fa-edit" aria-hidden="true"></i>
                                </a>
                                <a class="btn btn-xs btn-default viewreport" href="javascript:void(0)" title="View Report">
                                    <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                </a>
                                <a class="btn btn-xs btn-default minReport" href="javascript:void(0)" title="MIN Report">
                                        <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                    </a>
                            </span>`;
                        }
                        else if (status === statusConstants.APPROVED) {
                            template =
                                `<span class="btn-group"> 
                                    <a class="btn btn-xs btn-default Approved" href="javascript:void(0)" title="View Details">
                                        <i class="fa fa-eye" aria-hidden="true"></i>
                                    </a>
                                    <a class="btn btn-xs btn-default viewreport" href="javascript:void(0)" title="View Report">
                                        <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                    </a>
                                    <a class="btn btn-xs btn-default minReport" href="javascript:void(0)" title="MIN Report">
                                        <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                    </a>
                                </span>`;
                        }
                        else {
                            template =
                                `<span class="btn-group"> 
                                    <a class="btn btn-xs btn-default Reject" href="javascript:void(0)" title="View Details">
                                        <i class="fa fa-eye" aria-hidden="true"></i>
                                    </a>
                                    <a class="btn btn-xs btn-default viewreport" href="javascript:void(0)" title="View Report">
                                        <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                    </a>
                                    <a class="btn btn-xs btn-default minReport" href="javascript:void(0)" title="MIN Report">
                                        <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                    </a>
                                </span>`;
                        }
                        return template;
                    },
                    events: {
                        'click .add': function (e, value, row, index) {
                            e.preventDefault();

                            reqType = row.RequisitionType;
                            reqType = CheckReqType(reqType);

                            if (row.IsRevised == true) {
                                toastr.error("Pending Requisition Revision.");
                                return;
                            }
                            getNew(row.RnDReqMasterID);
                            $formEl.find("#btnSave").fadeIn();
                            $formEl.find("#btnApprove,#btnReject,#btnUnAcknowledge,#btnAcknowledge,#divRejectReason,#UnAcknowledgeReason").fadeOut();
                        },
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();

                            reqType = row.RequisitionType;
                            reqType = CheckReqType(reqType);
                            $formEl.find("#btnSave").fadeIn();
                            $formEl.find("#btnApprove,#btnReject,#btnUnAcknowledge,#btnAcknowledge,#divRejectReason,#UnAcknowledgeReason").fadeOut();
                            getDetails(row.IssueMasterID);
                        },
                        'click .acknowledge': function (e, value, row, index) {
                            e.preventDefault();

                            reqType = row.RequisitionType;
                            reqType = CheckReqType(reqType);
                            showBootboxConfirm("Acknowledge MRS", `Are you sure you want to acknowledge this ${row.RnDReqNo} MRS.`, function (yes) {
                                if (yes) acknowledgeMRS(e, row.RnDReqMasterID, row.RnDReqNo);
                            })
                        },
                        'click .mrsDetails': function (e, value, row, index) {
                            e.preventDefault();

                            reqType = row.RequisitionType;
                            reqType = CheckReqType(reqType);
                            $formEl.find("#btnSave,#btnApprove,#btnReject,#divRejectReason,#UnAcknowledgeReason").fadeOut();
                            $formEl.find("#btnAcknowledge,#btnUnAcknowledge,#UnAcknowledgeReason").fadeIn();

                            getNew(row.RnDReqMasterID);
                        },
                        'click .PendingApprove': function (e, value, row, index) {
                            e.preventDefault();

                            reqType = row.RequisitionType;
                            reqType = CheckReqType(reqType);
                            if (isIssuePage) {
                                $formEl.find("#btnSave,#btnAcknowledge,#btnUnAcknowledge,#UnAcknowledgeReason").fadeOut();
                                $formEl.find("#btnApprove,#btnReject,#divRejectReason").fadeOut();
                                $formEl.find("#divRejectReason").prop("disabled", false);
                            } else {
                                $formEl.find("#btnSave,#btnAcknowledge,#btnUnAcknowledge,#UnAcknowledgeReason").fadeOut();
                                $formEl.find("#btnApprove,#btnReject,#divRejectReason").fadeIn();
                                $formEl.find("#divRejectReason").prop("disabled", false);
                            }
                            getDetails(row.IssueMasterID);
                        },
                        'click .Approved': function (e, value, row, index) {
                            e.preventDefault();

                            reqType = row.RequisitionType;
                            reqType = CheckReqType(reqType);
                            $formEl.find("#divRejectReason").prop("disabled", true);
                            $formEl.find("#divRejectReason").fadeIn();
                            $formEl.find("#btnSave,#btnAcknowledge,#btnApprove,#btnReject,#btnUnAcknowledge,#UnAcknowledgeReason").fadeOut();
                            getDetails(row.IssueMasterID);
                        },
                        'click .viewreport': function (e, value, row, index) {
                            e.preventDefault();

                            reqType = row.RequisitionType;
                            viewReport(row.RnDReqMasterID, reqType);
                        },
                        'click .minReport': function (e, value, row, index) {
                            e.preventDefault();
                            window.open(`/reports/InlinePdfView?ReportName=MINRnDBulk.rdl&IssueNo=${row.IssueNo}`, '_blank');
                        },
                        'click .Reject': function (e, value, row, index) {
                            e.preventDefault();

                            reqType = row.RequisitionType;
                            reqType = CheckReqType(reqType);
                            $formEl.find("#divRejectReason").fadeIn();
                            $formEl.find("#divRejectReason").prop("disabled", true);
                            $formEl.find("#btnSave,#btnAcknowledge,#btnApprove,#btnReject,#btnUnAcknowledge,#UnAcknowledgeReason").fadeOut();
                            getNew(row.RnDReqMasterID);
                        },
                        'click .unacknowledge': function (e, value, row, index) {
                            e.preventDefault();

                            reqType = row.RequisitionType;
                            reqType = CheckReqType(reqType);
                            $formEl.find("#UnAcknowledgeReason").fadeIn();
                            $formEl.find("#UnAcknowledgeReason").prop("disabled", false);
                            $formEl.find("#btnSave,#btnAcknowledge,#btnApprove,#btnReject,#divRejectReason,#divRejectReason").fadeOut();
                            getNew(row.RnDReqMasterID);
                        }
                    }
                },
                {
                    field: "IssueNo",
                    title: "Issue No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status != statusConstants.PENDING

                },
                {
                    field: "IssueDate",
                    title: "Issue Date",
                    visible: status != statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "IssueByName",
                    title: "Issue By",
                    filterControl: "input",
                    visible: status === statusConstants.COMPLETED ||
                        status === statusConstants.AWAITING_PROPOSE ||
                        status === statusConstants.APPROVED,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "RnDReqNo",
                    title: "Requisition No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "RequisitionType",
                    title: "Requisition Type",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "RnDReqDate",
                    title: "Requisition Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "ConceptNo",
                    title: "Concept/Booking No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                },
                {
                    field: "IsReqForYD",
                    title: "Req. For YD?",
                    formatter: function (value, row, index, field) {
                        return value ? "Yes" : "No";
                    },
                },
                {
                    field: "CompanyName",
                    title: "Company",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                },
                {
                    field: "TotalReqQty",
                    title: "Req Qty",
                },
                {
                    field: "TotalReqCone",
                    title: "Req Cone",
                },
                {
                    field: "TotalIssueQty",
                    title: "Issued Qty",
                    //visible: status === statusConstants.COMPLETED
                    //    || status === statusConstants.AWAITING_PROPOSE
                    //    || status === statusConstants.APPROVED
                },
                {
                    field: "TotalIssueCone",
                    title: "Issue Cone",
                    visible: status === statusConstants.COMPLETED &&
                        status === statusConstants.AWAITING_PROPOSE &&
                        status === statusConstants.APPROVED
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
    function viewReport(rnDReqMasterID, reqType) {
        if (reqType == "Bulk") {
            window.open(`/reports/InlinePdfView?ReportName=DailyKnittingYarnRequisitionSlip.rdl&RequisitionID=${rnDReqMasterID}`, '_blank');
        } else {
            window.open(`/reports/InlinePdfView?ReportName=DailyYarnRequisitionSlip.rdl&RequisitionID=${rnDReqMasterID}`, '_blank');
        }
    }

    function getMasterTableData() {
        var queryParams = $.param(tableParams);
        $tblMasterEl.bootstrapTable('showLoading');
        var url = `/api/yarn-rnd-issues/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(showResponseError)
    }

    function initChildTable(datas) {
        datas.filter(x => x.KYIssueChildID == 0).map(x => {
            x.KYIssueChildID = _maxKYIssueChildID++;
        });
        datas.filter(x => x.IssueChildID == 0).map(x => {
            x.IssueChildID = _maxIssueChildID++;
        });

        datas.map(x => {
            if (x.IsItemValid) x.IsItemValidSt = "Valid Stock";
            else  if (!x.IsItemValid) x.IsItemValidSt = "Invalid Stock";
        });


        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            showFooter: true,
            data: datas,
            columns: [
                {
                    title: "Actions",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        return [
                            //'<span class="btn-group">',
                            //'<a class="btn btn-success btn-xs add" href="javascript:void(0)" title="Add a row like this">',
                            //'<i class="fa fa-plus"></i>',
                            //'</a>',
                            '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Remove this row">',
                            '<i class="fa fa-remove"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        //'click .add': function (e, value, row, index) {
                        //    addNewYarnItem(row);
                        //},
                        'click .remove': function (e, value, row, index) {
                            masterData.Childs.splice(index, 1);
                            $tblChildEl.bootstrapTable('load', masterData.Childs);
                        }
                    }
                },
                {
                    field: "YarnCategory", title: "Yarn Details"
                },
                {
                    field: "YarnPly", title: "No of Ply"
                },
                {
                    field: "ShadeCode", title: "Shade"
                },
                {
                    field: "StockTypeId", title: "StockTypeId", visible: false
                },
                {
                    field: "StockFromTableId", title: "StockFromTableId", visible: false
                },
                {
                    field: "StockFromPKId", title: "StockFromPKId", visible: false
                },
                {
                    field: "UOM",
                    title: "UOM"
                },
                {
                    field: "ReqQty",
                    title: "Req Qty"
                },
                {
                    field: "ReqCone",
                    title: "Req Cone"
                },
                {
                    field: "AllocatedQty",
                    title: "Allocated Qty",
                    visible: reqType == "Bulk"
                },
                {
                    field: "SampleStockQty",
                    title: "Sample Stock Qty",
                    visible: reqType == "RnD"
                },
                {
                    field: "PhysicalCount",
                    title: "Physical Count"
                },
                {
                    field: "LotNo",
                    title: "Lot No"
                },
                {
                    field: "YarnBrand",
                    title: "Spinner"
                },
                {
                    field: "IssueQty", title: "Issue Qty", visible: status == statusConstants.AWAITING_PROPOSE
                        || status == statusConstants.APPROVED || status == statusConstants.REJECT
                },
                {
                    field: "IssueQty",
                    title: "Issue Qty",
                    visible: status == statusConstants.PENDING || status == statusConstants.COMPLETED,
                    align: 'center',
                    formatter: function (value, row, index, field) {
                        // Add a class to the cell to enable the click event
                        return '<div class="issueQtyCell">' + value + '</div>';
                    }
                },
                {
                    field: "IssueCone", title: "Issue Cone", visible: status == statusConstants.AWAITING_PROPOSE
                        || status == statusConstants.APPROVED || status == statusConstants.REJECT
                },
                {
                    field: "IssueCone",
                    title: "Issue Cone",
                    visible: status == statusConstants.PENDING || status == statusConstants.COMPLETED,
                    align: 'center',
                    formatter: function (value, row, index, field) {
                        // Add a class to the cell to enable the click event
                        return '<div class="issueQtyCell">' + value + '</div>';
                    }
                },
                {
                    field: "IssueQtyCarton", title: "Issue Qty Carton", visible: status == statusConstants.AWAITING_PROPOSE
                        || status == statusConstants.APPROVED || status == statusConstants.REJECT
                },
                {
                    field: "IssueQtyCarton",
                    title: "Issue Qty Cartoon",
                    visible: status == statusConstants.PENDING || status == statusConstants.COMPLETED,
                    align: 'center',
                    formatter: function (value, row, index, field) {
                        // Add a class to the cell to enable the click event
                        return '<div class="issueQtyCell">' + value + '</div>';
                    }

                },
                {
                    field: "YarnBrand", title: "Yarn Brand", visible: status == statusConstants.AWAITING_PROPOSE
                        || status == statusConstants.APPROVED || status == statusConstants.REJECT
                },
                {
                    field: "IsItemValidSt", title: "Is Valid Stock ?", visible: status == statusConstants.AWAITING_PROPOSE
                }
            ]
        });
        // Add event delegation for "Issue Cone" cell
        $tblChildEl.on('click', '.issueQtyCell', function (e) {
            var $cell = $(this);
            var index = $cell.closest('tr').index();
            var row = $tblChildEl.bootstrapTable('getData')[index];

            $formEl.find(".spnStockInfoBasicInfo").text(row.YarnCategory);
            $formEl.find(".spnStockInfoBasicInfo").attr("TotalReqQty", row.ReqQty);
            $formEl.find(".spnStockInfoBasicInfo").attr("TotalAllocatedQty", row.AllocatedQty);
            $formEl.find(".spnStockInfoBasicInfo").attr("TotalSampleQty", row.SampleStockQty);

            selectedIndex = index;
            _selectedRnDReqChildID = row.RnDReqChildID;
            var childRackBinsExisting = row.ChildRackBins;
            var lotNo = getDefaultValueForAPICall(replaceInvalidChar(row.LotNo));
            var physicalCount = getDefaultValueForAPICall(replaceInvalidChar(row.PhysicalCount));
            var shadeCode = getDefaultValueForAPICall(replaceInvalidChar(row.ShadeCode));

            var itemMasterID = getDefaultValueWhenInvalidN(row.ItemMasterID);
            var supplierId = getDefaultValueWhenInvalidN(row.SupplierId);
            var spinnerId = getDefaultValueWhenInvalidN(row.SpinnerId);
            var YBChildItemID = getDefaultValueWhenInvalidN(row.YBChildItemID);
            var stockTypeId = getDefaultValueWhenInvalidN(row.StockTypeId);
            var stockFromTableId = getDefaultValueWhenInvalidN(row.StockFromTableId);
            var stockFromPKId = getDefaultValueWhenInvalidN(row.StockFromPKId);

            var isFromDraft = status == statusConstants.COMPLETED || status == statusConstants.AWAITING_PROPOSE;
            var childRackBinID = row.ChildRackBins.length > 0 ? row.ChildRackBins[0].ChildRackBinID : 0;
            var issuedQtySt = replaceInvalidChar(getDefaultValueWhenInvalidN_Float(row.IssueQty).toString());
            var menuName = "RnDIssue";
            var url = `/api/yarn-rnd-issues/GetStockForIssue/${lotNo}/${physicalCount}/${itemMasterID}/${spinnerId}/${menuName}/${stockTypeId}/${stockFromTableId}/${stockFromPKId}/${isFromDraft}/${childRackBinID}/${issuedQtySt}`;

            if (row.ReqType == 'Bulk') {
                url = `/api/yarn-rnd-issues/GetAllocatedStockForIssue/${YBChildItemID}/${lotNo}/${physicalCount}/${itemMasterID}/${supplierId}/${spinnerId}/${shadeCode}/${menuName}/${stockTypeId}/${stockFromTableId}/${stockFromPKId}/${isFromDraft}/${childRackBinID}/${issuedQtySt}`;
            }

            axios.get(url)
                .then(function (response) {
                    var list = response.data;
                    if (list.length == 0) {
                        //toastr.error("Rack bin allocation not completed.");
                        toastr.error("No data found.");
                        return false;
                    }
                    //ChildRackBins
                    var issueCartoon = 0;
                    var issueQtyCone = 0;
                    var issueQtyKg = 0;

                    var indexF = _childRackBins.findIndex(x => x.RnDReqChildID == _selectedRnDReqChildID);
                    if (indexF > -1) {
                        var childRackBins = _childRackBins[indexF].ChildRackBins;
                        list.map(x => {

                            var indexC = childRackBins.findIndex(y => y.ChildRackBinID == x.ChildRackBinID);
                            if (indexC > -1) {
                                var crbObj = childRackBins[indexC];

                                //------------Add Existing Qty-------------------------------
                                issueCartoon = 0;
                                issueQtyCone = 0;
                                issueQtyKg = 0;
                                if (childRackBinsExisting == null) {
                                    childRackBinsExisting = [];
                                }
                                childRackBinsExisting.filter(e => e.ChildRackBinID == x.ChildRackBinID).map(c => {
                                    issueCartoon += isNaN(c.IssueCartoon) ? 0 : c.IssueCartoon;
                                    issueQtyCone += isNaN(c.IssueQtyCone) ? 0 : c.IssueQtyCone;
                                    issueQtyKg += isNaN(c.IssueQtyKg) ? 0 : c.IssueQtyKg;
                                });

                                x.NoOfCartoon = x.NoOfCartoon + issueCartoon;
                                x.NoOfCone = x.NoOfCone + issueQtyCone;
                                x.ReceiveQty = x.ReceiveQty + issueQtyKg;
                                //--------------------------------------------------------------

                                x.YRICRBId = crbObj.YRICRBId;
                                x.IssueCartoon = crbObj.IssueCartoon;
                                x.IssueQtyCone = crbObj.IssueQtyCone;
                                x.IssueQtyKg = crbObj.IssueQtyKg;
                            }
                        });
                    }
                    list.filter(x => x.YRICRBId == 0).map(y => {
                        y.YRICRBId = _maxYRICRBId++;
                    });

                    initStockInfo(list, row.YarnCategory, row.ReqQty);
                    $modalPlanningEl.modal('show');
                })
                .catch(showResponseError);
        });


    }
    function reset() {
        _childRackBins = [];
        _selectedRnDReqChildID = 0;
    }
    function initStockInfo(data, yarnCategory, qeqQty) {
        if ($tblStockInfoEl) $tblStockInfoEl.destroy();

        $formEl.find(".spnStockInfoBasicInfo").text("(" + yarnCategory + ") Req Qty - " + qeqQty);

        data.map(x => {
            x.NoOfCartoon = getDefaultValueWhenInvalidN(x.NoOfCartoon);
            x.NoOfCone = getDefaultValueWhenInvalidN(x.NoOfCone);
            x.ReceiveQty = getDefaultValueWhenInvalidN(x.ReceiveQty);

            x.IssueCartoon = getDefaultValueWhenInvalidN(x.IssueCartoon);
            x.IssueQtyCone = getDefaultValueWhenInvalidN(x.IssueQtyCone);
            x.IssueQtyKg = getDefaultValueWhenInvalidN(x.IssueQtyKg);
        });

        $tblStockInfoEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            allowPaging: false,
            columns: [
                { field: 'ChildRackBinID', isPrimaryKey: true, visible: false, width: 10 },
                { field: 'IssueChildID', visible: false, width: 10 },
                { field: 'LocationName', headerText: 'Location', allowEditing: false, width: 80 },
                { field: 'RackNo', headerText: 'Rack', allowEditing: false, width: 80 },
                { field: 'RackQty', headerText: 'Rack Qty', allowEditing: false, width: 80 },
                { field: 'YarnControlNo', headerText: 'Control no', allowEditing: false, width: 80 },
                { field: 'AvgCartoonWeight', headerText: 'Avg. Cartoon Weight', allowEditing: false, width: 80 },
                { field: 'LotNo', headerText: 'Lot No', allowEditing: false, width: 80 },
                { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false, width: 80 },
                { field: 'SpinnerName', headerText: 'Spinner', allowEditing: false, width: 80 },
                { field: 'NoOfCartoon', headerText: 'No of Cartoon', allowEditing: false, width: 80 },
                { field: 'NoOfCone', headerText: 'No of Cone', allowEditing: false, width: 80 },
                //{ field: 'ReceiveQty', headerText: 'Stock Qty', allowEditing: false, width: 80 },
                { field: 'ReceiveQty', headerText: 'Stock Qty (Allocated/Sample)', allowEditing: false, width: 100 },

                { field: 'IssueQtyKg', headerText: 'Issue Qty (Kg)', allowEditing: true, width: 80, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 0 } } },
                { field: 'IssueQtyCone', headerText: 'Issue Cone', allowEditing: true, width: 80, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
                { field: 'IssueCartoon', headerText: 'Issue Cartoon', allowEditing: true, width: 80, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } }

            ],
            editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
            recordClick: function (args) {

            },
            actionBegin: function (args) {
                if (args.requestType === "save") {

                    if (args.data.IssueQtyKg > args.data.RackQty) {
                        toastr.error("Issue Qty (" + args.data.IssueQtyKg + ") cannot be greater than Rack Qty (" + args.data.RackQty + ")");
                        args.data.IssueQtyKg = 0;
                        args.data.IssueCartoon = 0;
                        args.data.IssueQtyCone = 0;
                        return;
                    }
                    if (args.data.IssueQtyKg > args.data.ReceiveQty) {
                        toastr.error("Issue Qty (" + args.data.IssueQtyKg + ") cannot be greater than Stock Qty (" + args.data.ReceiveQty + ")");
                        args.data.IssueQtyKg = 0;
                        args.data.IssueCartoon = 0;
                        args.data.IssueQtyCone = 0;
                        return;
                    }
                    /*var indexF = _childRackBins.findIndex(x => x.RnDReqChildID == _selectedRnDReqChildID);
                    if (indexF == -1) {
                        _childRackBins.push({
                            RnDReqChildID: _selectedRnDReqChildID,
                            ChildRackBins: []
                        });
                        indexF = _childRackBins.findIndex(x => x.RnDReqChildID == _selectedRnDReqChildID);
                        if (indexF > -1) {
                            if (args.data.YRICRBId == 0) args.data.YRICRBId = _maxYRICRBId++;
                            _childRackBins[indexF].ChildRackBins.push(args.data);
                        }
                    } else {
                        var indexC = _childRackBins[indexF].ChildRackBins.findIndex(x => x.ChildRackBinID == args.data.ChildRackBinID);
                        if (indexC == -1) {
                            if (args.data.YRICRBId == 0) args.data.YRICRBId = _maxYRICRBId++;
                            _childRackBins[indexF].ChildRackBins.push(args.data);
                        } else {
                            _childRackBins[indexF].ChildRackBins[indexC] = args.data;
                        }
                    }*/
                }
            },
        });
        $tblStockInfoEl.refreshColumns;
        $tblStockInfoEl.appendTo(tblStockInfoId);
    }
    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        getMasterTableData();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#IssueMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(reqMasterID) {
        resetGlobals();
        axios.get(`/api/yarn-rnd-issues/new/${reqMasterID}/${reqType}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                $formEl.find("#IsReqForYD").prop("disabled", true);
                masterData.IssueDate = formatDateToDefault(masterData.IssueDate);
                masterData.RnDReqDate = formatDateToDefault(masterData.RnDReqDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
            })
            .catch(showResponseError);
    }

    function getDetails(id) {
        resetGlobals();
        axios.get(`/api/yarn-rnd-issues/${id}/${reqType}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                $formEl.find("#IsReqForYD").prop("disabled", true);
                masterData.IssueDate = formatDateToDefault(masterData.IssueDate);
                masterData.RnDReqDate = formatDateToDefault(masterData.RnDReqDate);
                setFormData($formEl, masterData);

                masterData.Childs.map(x => {
                    _childRackBins.push({
                        RnDReqChildID: x.RnDReqChildID,
                        ChildRackBins: x.ChildRackBins
                    });
                });

                initChildTable(masterData.Childs);
            })
            .catch(showResponseError);
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data.RequisitionType = reqType;
        data.Childs = masterData.Childs;

        if (checkValidation(data.Childs)) return false;
        for (var index = 0; index < data.Childs.length; index++) {
            var indexF = _childRackBins.findIndex(x => x.RnDReqChildID == data.Childs[index].RnDReqChildID);
            if (indexF > -1) {

                _childRackBins[indexF].ChildRackBins.filter(x => x.YRICRBId == 0).map(x => x.YRICRBId = _maxYRICRBId++);
                data.Childs[index].ChildRackBins = _childRackBins[indexF].ChildRackBins.filter(x => x.IssueCartoon > 0 || x.IssueQtyCone > 0 || x.IssueQtyKg > 0);

                totalIssueQty = 0;
                totalIssueQtyCone = 0;
                totalIssueQtyCarton = 0;

                _childRackBins[indexF].ChildRackBins.map(x => {
                    totalIssueQty += x.IssueQtyKg;
                    totalIssueQtyCone += x.IssueQtyCone;
                    totalIssueQtyCarton += x.IssueCartoon;
                });
                data.Childs[index].IssueQty = getDefaultValueWhenInvalidN(totalIssueQty);
                data.Childs[index].IssueQtyCone = getDefaultValueWhenInvalidN(totalIssueQtyCone);
                data.Childs[index].IssueQtyCarton = getDefaultValueWhenInvalidN(totalIssueQtyCarton);
            }
        }

        axios.post("/api/yarn-rnd-issues/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function checkValidation(childs) {
        var hasError = false;
        for (var iC = 0; iC < childs.length; iC++) {
            var rowIndex = iC + 1;
            var child = childs[iC];
            if (child.IssueQty == 0) {
                toastr.error(`Give issue qty at row ${rowIndex}`);
                hasError = true;
                break;
            }
            //if (child.IssueQtyCarton == 0) {
            //    toastr.error(`Give issue qty carton at row ${rowIndex}`);
            //    hasError = true;
            //    break;
            //}
            //if (child.IssueQtyCone == 0) {
            //    toastr.error(`Give issue qty cone at row ${rowIndex}`);
            //    hasError = true;
            //    break;
            //}
        }
        return hasError;
    }
    function acknowledgeMRS(e, reqMasterID, reqNo) {
        e.preventDefault();
        if (!reqMasterID) reqMasterID = $formEl.find("#RnDReqMasterID").val();
        if (!reqMasterID) return toastr.error("Requisition ID is required.");
        if (!reqNo) reqNo = $formEl.find("#RnDReqNo").val();

        axios.post(`/api/yarn-rnd-requisition/acknowledge/${reqMasterID}`)
            .then(function () {
                toastr.success(`MRS no ${reqNo} acknowledged successfully.`);
                backToList();
            })
            .catch(showResponseError);
    }
    function unacknowledgeMRS(e, reqMasterID, reqNo, unAcknowledgeReason) {
        var data = formDataToJson($formEl.serializeArray());
        e.preventDefault();
        if (!reqMasterID) reqMasterID = $formEl.find("#RnDReqMasterID").val();
        if (!reqMasterID) return toastr.error("Requisition ID is required.");
        if (!reqNo) reqNo = $formEl.find("#RnDReqNo").val();
        if (!unAcknowledgeReason) unAcknowledgeReason = $formEl.find("#UnAcknowledgeReason").val();
        //alert(UnAcknowledgeReason);
        axios.post(`/api/yarn-rnd-requisition/unacknowledge/${reqMasterID}/${unAcknowledgeReason}`)
            .then(function () {
                toastr.success(`MRS no ${reqNo} Unacknowledged successfully.`);
                backToList();
            })
            .catch(showResponseError);
    }

    function Approve() {
        var data = formDataToJson($formEl.serializeArray());
        data.Childs = masterData.Childs;

        var hasError = false;
        for (var i = 0; i < data.Childs.length; i++) {
            if (!data.Childs[i].IsItemValid) {
                toastr.error(`Invalid stock item at row ${i + 1}`);
                hasError = true;
                break;
            }
        }
        if (hasError) return false;

        data.Approve = true;
        data.RequisitionType = reqType;

        //axios.post("/api/yarn-rnd-issues/saveprocess", data)
        axios.post("/api/yarn-rnd-issues/approveOrReject", data)
            .then(function () {
                toastr.success(`Approve operation successfully.`);
                backToList();
            })
            .catch(showResponseError);
    }
    function Reject() {
        var data = formDataToJson($formEl.serializeArray());
        data.Childs = masterData.Childs;
        data.Approve = false;
        data.Reject = true;
        data.RequisitionType = reqType;

        //axios.post("/api/yarn-rnd-issues/saveprocess", data)
        axios.post("/api/yarn-rnd-issues/approveOrReject", data)
            .then(function () {
                toastr.success(`Approve operation successfully.`);
                backToList();
            })
            .catch(showResponseError);
    }
    function CheckReqType(reqType) {
        if (reqType == 'R&D') {
            reqType = 'RnD';
        }
        else {
            reqType = 'Bulk';
        }
        return reqType;
    }

    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
    function resetGlobals() {
        _childRackBins = [];
        _selectedRnDReqChildID = 0;
        _maxYRICRBId = 999;
        _maxKYIssueChildID = 999;
        _maxIssueChildID = 999;

        addItem = [];
        var reqType = null;
    }
})();
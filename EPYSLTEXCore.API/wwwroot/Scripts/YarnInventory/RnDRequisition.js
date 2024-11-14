(function () {
    var menuId, pageName;
    var toolbarId;
    var isAcknowledgePage = false;
    var isApprovePage = false;
    var $pageEl, pageId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, tblChildId, $formEl, $tblFreeConceptMREl, tblMasterId, tblFreeConceptMRId;
    var status = statusConstants.PROPOSED;
    var isEditable = false;
    var addAdditionalReq = false;
    var masterData, tempData;
    var _isNewWithoutKnittingInfo = false;
    var _itemSegmentValues;
    var _rnDReqChildID = 1000;
    var tblCreateItemId, $tblCreateItemEl;
    var _isReqForYDShow = true;
    var validationConstraints = {
        RnDReqDate: {
            presence: true
        },
        RCompanyID: {
            presence: true
        }
    }
    var reqType = "";
    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        $pageEl = $(`#${pageId}`);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblFreeConceptMRId = "#tblFreeConceptMRId" + pageId;
        tblCreateItemId = `#tblCreateItem-${pageId}`;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val());
        isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());

        if (isAcknowledgePage) _isReqForYDShow = false;

        if (isAcknowledgePage) {
            status = statusConstants.PROPOSED_FOR_ACKNOWLEDGE;
            $toolbarEl.find("#btnNewItem").hide();
            $toolbarEl.find("#btnApproved").hide();
            $toolbarEl.find("#btnList").hide();
            $toolbarEl.find("#btnWaitForApprove").hide();
            $toolbarEl.find("#btnAcknowledge").show();
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingAcknowledgeList"), $toolbarEl);
            $toolbarEl.find(".btnExtra").hide();
            $toolbarEl.find("#btnPending").hide();
            $formEl.find("#btnAddItem").hide();
            $toolbarEl.find("#btnPendingRevision").hide();
        }
        else if (isApprovePage) {
            status = statusConstants.PENDING;
            toggleActiveToolbarBtn($toolbarEl.find("#btnWaitForApprove"), $toolbarEl);
            $toolbarEl.find("#btnList").hide();
            $toolbarEl.find("#btnNew").hide();
            $toolbarEl.find("#btnNewItem").hide();
            $toolbarEl.find("#btnApproved").show();
            $toolbarEl.find("#btnAcknowledge").hide();
            $toolbarEl.find("#btnPendingAcknowledgeList").hide();
            $toolbarEl.find(".btnExtra").hide();
            $toolbarEl.find("#btnPending").hide();
            $formEl.find("#btnAddItem").hide();
            $formEl.find("#btnSave").hide();
            $formEl.find("#btnApproveYPR").show();
            $toolbarEl.find("#btnPendingRevision").hide();
        }
        else {
            status = statusConstants.PROPOSED;
            $toolbarEl.find("#btnPendingAcknowledgeList").hide();
            $toolbarEl.find("#btnWaitForApprove").hide();
        }

        initMasterTable();

        $formEl.find("#btnRejectYPR").click(function (e) {
            e.preventDefault();

            bootbox.prompt("Are you sure you want to reject this?", function (result) {
                if (!result) {
                    return toastr.error("Reject reason is required.");
                }
                var id = $formEl.find("#RnDReqMasterID").val();
                var reason = result;
                axios.post(`/api/yarn-rnd-requisition/reject/${id}/${reason}`)
                    .then(function () {
                        toastr.success("Requisition rejected successfully.");
                        backToList();
                    })
                    .catch(showResponseError);
            });
        });

        $toolbarEl.find("#btnRefreshList").on("click", function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
        });

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            status = statusConstants.PROPOSED;
            toggleActiveToolbarBtn(this, $toolbarEl);
            initMasterTable();
            $toolbarEl.find(".btnExtra").show();
            $formEl.find("#btnAddItem").show();
            isEditable = true;
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            status = statusConstants.PENDING;
            toggleActiveToolbarBtn(this, $toolbarEl);
            initMasterTable();
            $toolbarEl.find(".btnExtra").hide();
            $formEl.find("#btnAddItem").show();
            isEditable = true;
        });

        $toolbarEl.find("#btnWaitForApprove").on("click", function (e) {
            e.preventDefault();
            status = statusConstants.PENDING;
            $formEl.find("#btnAkgYPR, #btnAkgYPRUN").hide();
            toggleActiveToolbarBtn(this, $toolbarEl);
            initMasterTable();
            $toolbarEl.find(".btnExtra").hide();
            $formEl.find("#btnAddItem").show();
            isEditable = false;
        });

        $toolbarEl.find("#btnApproved").on("click", function (e) {
            e.preventDefault();
            status = statusConstants.APPROVED;
            $formEl.find("#btnAkgYPR, #btnAkgYPRUN").hide();
            toggleActiveToolbarBtn(this, $toolbarEl);
            initMasterTable();
            $toolbarEl.find(".btnExtra").hide();
            $formEl.find("#btnAddItem").hide();
            $formEl.find("#btnRevise").hide();
            isEditable = false;
        });

        $toolbarEl.find("#btnPendingAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            status = statusConstants.PROPOSED_FOR_ACKNOWLEDGE;
            toggleActiveToolbarBtn(this, $toolbarEl);
            initMasterTable();
            $toolbarEl.find(".btnExtra").hide();
            $formEl.find("#btnAddItem").hide();
            isEditable = false;
        });

        $toolbarEl.find("#btnAcknowledge").on("click", function (e) {
            e.preventDefault();
            status = statusConstants.ACKNOWLEDGE;
            toggleActiveToolbarBtn(this, $toolbarEl);
            initMasterTable();
            $toolbarEl.find(".btnExtra").hide();
            $formEl.find("#btnAddItem").hide();
            $formEl.find("#btnRevise").hide();
            isEditable = false;
        });

        $toolbarEl.find("#btnPendingRevision").on("click", function (e) {
            e.preventDefault();
            status = statusConstants.REVISE;
            toggleActiveToolbarBtn(this, $toolbarEl);
            initMasterTable();
            $toolbarEl.find(".btnExtra").hide();
            $formEl.find("#btnAddItem").hide();
            isEditable = false;
        });
        $formEl.find("#btnSave").click(save);

        $formEl.find("#btnRevise").click(revise);

        $toolbarEl.find("#btnCreate").on("click", function (e) {
            _isNewWithoutKnittingInfo = false;
            operationKnittingInfoFields();

            addAdditionalReq = false;
            if ($tblMasterEl.getSelectedRecords().length == 0) {
                toastr.error("You must select concept(s)!");
                return;
            }
            $formEl.find("#btnRevise").hide();
            var uniqueAry = distinctArrayByProperty($tblMasterEl.getSelectedRecords(), "Source");
            //console.log($tblMasterEl.getSelectedRecords());
            if (uniqueAry.length != 1) {
                toastr.error("Selected row(s) source should be same!");
                return;
            } else {
                var uniqueAry1 = distinctArrayByProperty($tblMasterEl.getSelectedRecords(), "Status");
                if (uniqueAry1.length != 1) {
                    toastr.error("Selected row(s) Status should be same!");
                    return;
                }
            }
            var fcIds = $tblMasterEl.getSelectedRecords().map(function (el) { return el.FCMRMasterID }).toString();
            var Status = $tblMasterEl.getSelectedRecords().map(function (el) { return el.Status }).toString();
            var isYDItem = false, isGrey = false;

            var groupIDs = new Array();
            var RndSelectedRecord = $tblMasterEl.getSelectedRecords();

            for (var i = 0; i < RndSelectedRecord.length; i++) {
                var newMasterItem = {
                    GroupID: RndSelectedRecord[i].GroupID,
                    Status: uniqueAry[0].Status
                };
                groupIDs.push(newMasterItem);
            }
            axios.post("/api/yarn-rnd-requisition/new2/", groupIDs)
                //axios.get(`/api/yarn-rnd-requisition/new/${arrFcId}/${uniqueAry[0].Status}`)
                .then(function (response) {

                    response.data.Childs.forEach(function (value) {
                        if (value.YDItem) isYDItem = true;
                        else isGrey = true;

                        if (isYDItem && !isGrey) {
                            $('input[name=ReqFor]').attr("disabled", true);
                            $("#Req-for-YD").prop("checked", true);
                        } else if (!isYDItem && isGrey) {
                            $('input[name=ReqFor]').attr("disabled", true);
                            $("#Req-for-Grey").prop("checked", true);
                        } else {
                            $('input[name=ReqFor]').attr("disabled", false);
                            $("#Req-for-YD").prop("checked", true);
                        }
                        isEditable = true;

                        tempData = response.data;
                        $("#modal-ReqFor").modal('show');
                    });
                })
                .catch(showResponseError);
        });

        $toolbarEl.find("#btnNewWithoutKnittingInfo").click(function () {
            axios.post("/api/yarn-rnd-requisition/new-without-knitting-info")
                .then(function (response) {
                    _isNewWithoutKnittingInfo = true;
                    $formEl.find("#btnRevise,#btnAkgYPR,#btnAkgYPRUN").hide();

                    $divDetailsEl.fadeIn();
                    $divTblEl.fadeOut();

                    masterData = response.data;
                    masterData.RnDReqDate = formatDateToDefault(masterData.RnDReqDate);
                    setFormData($formEl, masterData);
                    operationKnittingInfoFields();

                    initFCMRTable([]);
                    initChildTable([]);
                })
                .catch(showResponseError);
        });

        //$formEl.find("#btnApproveYPR").click(function (e) {
        //    e.preventDefault();
        //    var id = $formEl.find("#RnDReqMasterID").val();
        //    axios.post(`/api/yarn-rnd-requisition/approve/${id}`)
        //        .then(function () {
        //            toastr.success("Requisition approved successfully.");
        //            backToList();
        //        })
        //        .catch(showResponseError);
        //});

        $formEl.find("#btnApproveYPR").click(function (e) {
            e.preventDefault();

            var data = {
                RnDReqMasterID: $formEl.find("#RnDReqMasterID").val(),
                Childs: $tblChildEl.getCurrentViewRecords()
            };

            axios.post("/api/yarn-rnd-requisition/approve", data)
                .then(function () {
                    toastr.success("Requisition approved successfully.");
                    backToList();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnAkgYPR").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#RnDReqMasterID").val();
            var requisitionType = $formEl.find("#RequisitionType").val();
            ackRndReq(id, requisitionType);
            //ackRndReq(args.rowData.RnDReqMasterID, args.rowData.RequisitionType);
        });

        $formEl.find("#btnAkgYPRUN").click(function (e) {
            e.preventDefault();
            bootbox.prompt("Are you sure you want to unacknowledge this?", function (result) {
                if (!result) {
                    return toastr.error("Unacknowledge reason is required.");
                }
                var id = $formEl.find("#RnDReqMasterID").val();
                var reason = result;
                axios.post(`/api/yarn-rnd-requisition/unacknowledge/${id}/${reason}/${reqType}`)
                    .then(function () {
                        toastr.success("Requisition Unacknowledged successfully.");
                        backToList();
                    })
                    .catch(showResponseError);
            });
        });

        $formEl.find("#btnSaveAndProposeYPR").click(function (e) {
            e.preventDefault();
            saveForApproval(this);
        });

        $formEl.find("#btnCancel").on("click", backToListWithoutFilter);

        $formEl.find("#btnAddItem").on("click", function (e) {
            e.preventDefault();
            /*
            var list = $tblFreeConceptMREl.getCurrentViewRecords();
            var fcIds = list.map(x => x.GroupID).join(",");// $tblFreeConceptMREl.getCurrentViewRecords().map(function (el) { return el.FCMRMasterID }).toString();
            var finder = new commonFinder({
                title: "Select Concept",
                pageId: pageId,
                height: 320,
                modalSize: "modal-lg",
                apiEndPoint: `/api/yarn-rnd-requisition/MRs/${fcIds}`,
                fields: "ConceptNo,ConceptForName",
                headerTexts: "Concept No,Concept For",
                isMultiselect: true,
                primaryKeyColumn: "GroupID",
                onMultiselect: function (selectedRecords) {
                    selectedRecords.forEach(function (value) {
                        var indexF = list.findIndex(el => el.GroupID == value.GroupID);
                        if (indexF > -1) {
                            list.splice(index, 1);
                        }
                    });

                    fcIds = list.map(x => x.GroupID).join(",");
                    getFCData(fcIds);

                    //selectedRecords.forEach(function (value) {
                    //    var exists = $tblFreeConceptMREl.getCurrentViewRecords().find(function (el) { return el.FCMRMasterID == value.FCMRMasterID });
                    //    if (!exists) $tblFreeConceptMREl.getCurrentViewRecords().unshift(value);
                    //});

                    //fcIds = $tblFreeConceptMREl.getCurrentViewRecords().map(function (el) { return el.FCMRMasterID }).toString();
                    //getFCData(fcIds);
                }
            });

            finder.showModal();
            */
        });

        $("#btnOkReqFor").click(function (e) {
            e.preventDefault();
            if (tempData) {
                tempData.IsReqForYD = $('input[name="ReqFor"]')[0].checked;
                $("#modal-ReqFor").modal('hide');
                getFCDataForSave(tempData);
            }
        });

        $formEl.find("#btnCompanyName").on("click", function (e) {
            e.preventDefault();
            var finder = new commonFinder({
                title: "Company List",
                pageId: pageId,
                data: masterData.RCompanyList,
                isMultiselect: false,
                modalSize: "modal-md",
                top: "2px",
                primaryKeyColumn: "id",
                fields: "text",
                headerTexts: "Company Name",
                widths: "30",
                onSelect: function (res) {
                    finder.hideModal();
                    masterData.RCompanyID = res.rowData.id;
                    $formEl.find("#RCompanyID").val(res.rowData.id);
                    $formEl.find("#RCompanyName").val(res.rowData.text);
                },
            });
            finder.showModal();
        });

        $formEl.find("#btnAddItemPopup").on("click", function (e) {
            showAddItem();
        });
        $pageEl.find("#btnAddItem1").click(setItems);

        axios.get(getYarnItemsApiUrl([])).then(res => {
            _itemSegmentValues = res.data;
        });
    });
    function ackRndReq(rnDReqMasterID, RequisitionType = null) {
        //if (RequisitionType == 'R&D') {
        //    reqType = 'RnD';
        //}
        //else {
        //    reqType = 'Bulk';
        //}
        RequisitionType = replaceInvalidChar(RequisitionType);
        axios.post(`/api/yarn-rnd-requisition/acknowledge/${rnDReqMasterID}/${RequisitionType}`)
            .then(function () {
                toastr.success("Requisition acknowledged successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function resizeColumns(columnList) {
        var cAry = ["BookingNo", "Segment1ValueId", "Segment2ValueId", "Segment3ValueId", "Segment4ValueId", "Segment5ValueId", "Segment6ValueId"];
        cAry.map(c => {
            var indexF = columnList.findIndex(x => x.field == c);
            var widthValue = 62;
            if (c == "Segment1ValueId") widthValue = 180;
            if (indexF > -1) columnList[indexF].width = widthValue;
        });
        return columnList;
    }
    function showAddItem() {
        initTblCreateItem();
        $pageEl.find(`#modal-new-item-${pageId}`).modal("show");
    }
    async function setItems() {
        var itemList = [];
        var yarnReceiveChilds = $tblChildEl.getCurrentViewRecords();
        if (typeof yarnReceiveChilds !== "undefined" && yarnReceiveChilds.length > 0) {
            itemList = yarnReceiveChilds;
        }
        for (var i = 0; i < compositionItems.length; i++) {
            compositionItems[i].RnDReqChildID = _rnDReqChildID++;
            if (compositionItems[i].id) {
                compositionItems[i].BookingID = compositionItems[i].id;
                compositionItems[i].BookingNo = masterData.BookingList.find(x => x.id == compositionItems[i].BookingID).text;
                compositionItems[i].ConceptNo = compositionItems[i].BookingNo;
            }
            if (compositionItems[i].Segment1ValueId) {
                compositionItems[i].Segment1ValueDesc = _itemSegmentValues.Segment1ValueList.find(x => x.id == compositionItems[i].Segment1ValueId).text;
            }
            if (compositionItems[i].Segment2ValueId) {
                compositionItems[i].Segment2ValueDesc = _itemSegmentValues.Segment2ValueList.find(x => x.id == compositionItems[i].Segment2ValueId).text;
            }
            if (compositionItems[i].Segment3ValueId) {
                compositionItems[i].Segment3ValueDesc = _itemSegmentValues.Segment3ValueList.find(x => x.id == compositionItems[i].Segment3ValueId).text;
            }
            if (compositionItems[i].Segment4ValueId) {
                compositionItems[i].Segment4ValueDesc = _itemSegmentValues.Segment4ValueList.find(x => x.id == compositionItems[i].Segment4ValueId).text;
            }
            if (compositionItems[i].Segment5ValueId) {
                compositionItems[i].Segment5ValueDesc = _itemSegmentValues.Segment5ValueList.find(x => x.id == compositionItems[i].Segment5ValueId).text;
            }
            if (compositionItems[i].Segment6ValueId) {
                compositionItems[i].Segment6ValueDesc = _itemSegmentValues.Segment6ValueList.find(x => x.id == compositionItems[i].Segment6ValueId).text;
            }
            itemList.push(DeepClone(compositionItems[i]));
        }
        compositionItems = DeepClone(itemList);
        initChildTable(compositionItems);
        $pageEl.find(`#modal-new-item-${pageId}`).modal("hide");
    }
    async function initTblCreateItem() {
        compositionItems = [];
        var bookingColumn = {
            field: 'id', headerText: 'Booking No', valueAccessor: ej2GridDisplayFormatter,
            dataSource: masterData.BookingList, displayField: "text",
            edit: ej2GridDropDownObj({
            })
        };
        var itemcolumns = await getYarnItemColumnsAsync([], true);
        itemcolumns.splice(1, 0, bookingColumn);
        itemcolumns = resizeColumns(itemcolumns);
        var columns = [
            {
                field: 'Id', isPrimaryKey: true, visible: false
            },
            {
                headerText: '', width: 20, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
            },
        ];
        var gridOptions = {
            tableId: tblCreateItemId,
            data: compositionItems,
            columns: itemcolumns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.Id = getMaxIdForArray(compositionItems, "Id");
                }
                else if (args.requestType === "save" && args.action === "edit") {
                    ////args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                    //args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                    //args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                    //args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                    //args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                    //args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                    //args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                    //args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;
                }
            },
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false,
            toolbar: ['Add'],
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true }
        };

        if ($tblCreateItemEl) $tblCreateItemEl.destroy();
        $tblCreateItemEl = new initEJ2Grid(gridOptions);
    }
    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
    function operationKnittingInfoFields() {
        if (_isNewWithoutKnittingInfo) {
            $formEl.find("#btnCompanyName").show();
            $formEl.find(".divSummaryDiv").hide();
            $formEl.find("#btnAddItemPopup").show();
            $formEl.find("#RCompanyName").css({
                "width": "90%",
                "float": "left"
            });

        } else {
            $formEl.find("#btnCompanyName").hide();
            $formEl.find(".divSummaryDiv").show();
            $formEl.find("#btnAddItemPopup").hide();
            $formEl.find("#RCompanyName").css({
                "width": "100%"
            });
        }
    }

    function initMasterTable() {
        var columnsBtn = {};
        if (isApprovePage) {
            columnsBtn = {
                headerText: 'Command', width: 100, commands: [
                    { type: 'Edit', title: 'View this requisition', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Add', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
        }
        else if (isAcknowledgePage && status != statusConstants.ACKNOWLEDGE) {
            columnsBtn = {
                headerText: 'Command', width: 100, commands: [
                    { type: 'AcknowledgeMRS', title: 'Acknowledge MRS', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } },
                    { type: 'Edit', title: 'View this requisition', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Add', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
        }
        else if (isAcknowledgePage && status == statusConstants.ACKNOWLEDGE) {
            columnsBtn = {
                headerText: 'Command', width: 100, commands: [
                    { type: 'Edit', title: 'View this requisition', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Add', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            }
        }
        else if (status != statusConstants.PROPOSED) {
            var commandsList = [];
            if (status == statusConstants.APPROVED || status == statusConstants.ACKNOWLEDGE) {
                commandsList.push({ type: 'View', visible: status == statusConstants.APPROVED || status == statusConstants.ACKNOWLEDGE, title: 'Create Another requisition', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } });
            }
            commandsList.push({ type: 'Edit', title: 'View this requisition', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } });
            commandsList.push({ type: 'Add', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } });

            columnsBtn = {
                headerText: 'Command', width: 100, commands: commandsList
            }
        }
        var columns = [
            {
                field: 'GroupID', headerText: 'GroupID', visible: false
            },
            {
                field: 'RnDReqMasterID', headerText: 'RnDReqMasterID', visible: false
            },
            {
                field: 'RequisitionType', headerText: 'Requisition Type'
            },
            {
                field: 'RnDReqNo', headerText: 'Requisition No', visible: status != statusConstants.PROPOSED
            },
            {
                field: 'ConceptNo', headerText: 'Booking No'
            },
            {
                field: 'Buyer', headerText: 'Buyer', visible: status == statusConstants.PROPOSED
            },
            {
                field: 'ItemSubGroup', headerText: 'Item Sub Group', visible: status == statusConstants.PROPOSED
            },
            {
                field: 'KnittingType', headerText: 'Knitting Type', visible: status == statusConstants.PROPOSED, visible: false
            },
            {
                field: 'TechnicalName', headerText: 'Technical Name', visible: status == statusConstants.PROPOSED
            },
            {
                field: 'Composition', headerText: 'Composition', visible: status == statusConstants.PROPOSED
            },
            {
                field: 'GSM', headerText: 'GSM', visible: status == statusConstants.PROPOSED
            },
            {
                field: 'Color', headerText: 'Color', visible: status == statusConstants.PROPOSED
            },
            {
                field: 'Status', headerText: 'Status', visible: status == statusConstants.PROPOSED
            },
            {
                field: 'Qty', headerText: 'Qty(Kg)', visible: status != statusConstants.PROPOSED
            },
            {
                field: 'YDStatus', headerText: 'YD Requisition?'
            },
            {
                field: 'RnDReqDate', headerText: 'Requisition Date', visible: status != statusConstants.PROPOSED, type: 'date', format: _ch_date_format_1
            },
            {
                field: 'RnDReqByName', headerText: 'Requisition By', visible: status != statusConstants.PROPOSED
            },
            {
                field: 'ApproveDate', headerText: 'Approve Date', visible: status === statusConstants.APPROVED, type: 'date', format: _ch_date_format_1
            },
            {
                field: 'ApproveDate', headerText: 'Approve Time', visible: status === statusConstants.APPROVED, type: 'date', format: 'hh:mm:ss'
            },
            {
                field: 'RnDApproveBy', headerText: 'Approve By', visible: status === statusConstants.APPROVED
            },
            {
                field: 'CompanyName', headerText: 'Company', visible: status != statusConstants.PROPOSED
            },
            {
                field: 'ConceptDate', headerText: 'Concept Date', type: 'date', format: _ch_date_format_1, visible: false
            },
            {
                field: 'ConceptForName', headerText: 'Concept For', visible: false
            },
            {
                field: 'TrialNo', headerText: 'Re-Trial No', visible: false
            },
            {
                field: 'IsIssue', headerText: 'IsIssue', visible: status == statusConstants.ACKNOWLEDGE
            },
            {
                field: 'YarnReqStatus', headerText: 'Status', visible: status != statusConstants.PROPOSED
            }
        ];

        if (status != statusConstants.PROPOSED) {
            columns.unshift(columnsBtn);
        }

        if (status == statusConstants.PROPOSED) {
            columns.splice(1, 0, {
                type: 'checkbox', width: 100, headerText: 'Select', visible: status == statusConstants.PROPOSED
            });
        }

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            allowFiltering: true,
            apiEndPoint: `/api/yarn-rnd-requisition/list?status=${status}&isReqForYDShow=${_isReqForYDShow}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        addAdditionalReq = false;
        resetGlobals();
        var reqType = args.rowData.RequisitionType;
        if (typeof reqType === "undefined") reqType = "";

        if (args.commandColumn.type == "Add") {
            if (reqType == "Bulk") {
                window.open(`/reports/InlinePdfView?ReportName=DailyKnittingYarnRequisitionSlip.rdl&RequisitionID=${args.rowData.RnDReqMasterID}`, '_blank');
            } else {
                window.open(`/reports/InlinePdfView?ReportName=DailyYarnRequisitionSlip.rdl&RequisitionID=${args.rowData.RnDReqMasterID}`, '_blank');
            }
            return false;
        }
        if (args.commandColumn.type == "View") {
            isEditable = true;
            addAdditionalReq = true;
            addPOFromPR(args.rowData.RnDReqMasterID, 1, args.rowData.RequisitionType);
        }
        else if (args.commandColumn.type == "Edit") {

            //if (args.rowData.RnDReqNo.includes("Add") == true) {
            addAdditionalReq = args.rowData.IsAdditional;

            if (status != statusConstants.REVISE)
                getDetails(args.rowData.RnDReqMasterID, 2, args.rowData.RequisitionType);
            else {
                isEditable = true;
                getReviseDetails(args.rowData.RnDReqMasterID, 2, args.rowData.FCMRMasterIDs);
            }
        }
        else if (args.commandColumn.type == "AcknowledgeMRS" && isAcknowledgePage) {
            showBootboxConfirm("Acknowledge MRS", `Are you sure you want to acknowledge this ${args.rowData.RnDReqNo} MRS.`, function (yes) {
                if (yes) ackRndReq(args.rowData.RnDReqMasterID, args.rowData.RequisitionType); //acknowledgeMRS(e, row.RnDReqMasterID, row.RnDReqNo);
            })
        }
        else {
            //if (isApprovePage) {
            //    approvePR(args.rowData.RnDReqMasterID);
            //}
            if (isAcknowledgePage) {
                acknowledgePR(args.rowData.RnDReqMasterID);
            }
            else {
                // getDetails(args.rowData.RnDReqMasterID, 2);
            }
        }
    }
    function addPOFromPR(id, flag, RequisitionType = null) {
        if (RequisitionType == 'R&D') {
            reqType = 'RnD';
        }
        else {
            reqType = 'Bulk';
        }

        _isNewWithoutKnittingInfo = false;
        operationKnittingInfoFields();
        axios.get(`/api/yarn-rnd-requisition/${id}/${flag}/${reqType}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.RnDReqDate = formatDateToDefault(masterData.RnDReqDate);
                //isEditable = true;
                $formEl.find("#IsReqForYD").prop("disabled", true);
                setFormData($formEl, masterData);

                initChildTable(masterData.Childs);

                initFCMRTable(masterData.FreeConceptMR);

                if (addAdditionalReq) {
                    $formEl.find("#btnSave").show();
                    $formEl.find("#btnApproveYPR, #btnAkgYPRUN").hide();
                }
                else if (isApprovePage && status == statusConstants.PENDING) {
                    $formEl.find("#btnSave").show();
                    $formEl.find("#btnApproveYPR").show();
                }
                else if (status == statusConstants.PROPOSED || status == statusConstants.PENDING) {
                    $formEl.find("#btnSave").show();
                    $formEl.find("#btnApproveYPR").hide();
                }
                else if (status == statusConstants.ACKNOWLEDGE) {
                    $formEl.find("#btnSave").show();
                    $formEl.find("#btnApproveYPR, #btnAkgYPRUN").show();
                }
                else {
                    $formEl.find("#btnSave").show();
                    $formEl.find("#btnApproveYPR").hide();
                }

                _isNewWithoutKnittingInfo = masterData.IsWOKnittingInfo;
                operationKnittingInfoFields();
            })
            .catch(showResponseError);
    }

    function initFCMRTable(records) {
        if ($tblFreeConceptMREl) $tblFreeConceptMREl.destroy();
        ej.base.enableRipple(true);
        $tblFreeConceptMREl = new ej.grids.Grid({
            editSettings: { allowDeleting: true },
            commandClick: fcHandleCommands,
            allowResizing: true,
            dataSource: records,
            columns: [
                {
                    headerText: '', width: 80, commands: [
                        { type: 'Edit', title: 'Remove this row', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }]
                },
                {
                    field: 'GroupID', width: 140, visible: false
                },
                {
                    field: 'ConceptNo', width: 140, headerText: 'Concept No'
                },
                {
                    field: 'ConceptDate', width: 120, headerText: 'Concept Date', type: 'date', format: _ch_date_format_1
                }
                //{
                //    field: 'ConceptForName', width: 140, headerText: 'Concept For'
                //}
            ]
        });
        $tblFreeConceptMREl.appendTo(tblFreeConceptMRId);
    }

    function fcHandleCommands(args) {
        var i = 0;
        while (i < masterData.Childs.length) {
            if (masterData.Childs[i].FCMRMasterID === args.rowData.FCMRMasterID) {
                masterData.Childs.splice(i, 1);
            } else {
                ++i;
            }
        }
        initChildTable(masterData.Childs);

        var j = 0;
        while (j < $tblFreeConceptMREl.dataSource.length) {
            if ($tblFreeConceptMREl.dataSource[j].FCMRMasterID === args.rowData.FCMRMasterID) {
                $tblFreeConceptMREl.dataSource.splice(j, 1);
            } else {
                ++j;
            }
        }
        initFCMRTable($tblFreeConceptMREl.dataSource);
    }

    async function initChildTable(data) {
        var conceptNoText = _isNewWithoutKnittingInfo ? "Concept No / Booking No" : "Concept No";
        data.map(x => {
            x.RnDReqChildID = x.RnDReqChildID == 0 ? _rnDReqChildID++ : x.RnDReqChildID;

            if (typeof x.StockQty === "undefined" || x.StockQty == null) {
                x.StockQty = 0;
            }
        });

        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];
        if (isAcknowledgePage == true || isApprovePage == true) {
            columns = [{ field: 'ConceptNo', headerText: conceptNoText, allowEditing: false }];
        }
        else {
            if (status === statusConstants.PROPOSED || status === statusConstants.PENDING) {
                columns = [
                    {
                        headerText: 'Commands', visible: (!isApprovePage), width: 120, commands: [
                            { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                            { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                            { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                            { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
                    },
                    { field: 'ConceptNo', headerText: conceptNoText, allowEditing: false }
                ];
            }
            else if (status === statusConstants.REVISE) {
                columns = [
                    {
                        headerText: 'Commands', width: 120, commands: [
                            { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }]
                    },
                    { field: 'ConceptNo', headerText: conceptNoText, allowEditing: false }
                ];
            }
            else if (addAdditionalReq) {
                columns = [
                    {
                        headerText: 'Commands', visible: (!isApprovePage), width: 120, commands: [
                            { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                            { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                            { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                            { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
                    },
                    { field: 'ConceptNo', headerText: conceptNoText, allowEditing: false }
                ];
            }
        }

        //columns.push.apply(columns, await getYarnItemColumnsForDisplayOnly());

        var isRemarksEditable = true;
        if (status == statusConstants.PENDING && isApprovePage) {
            isRemarksEditable = true;
        }
        else if (!_isNewWithoutKnittingInfo) {
            isRemarksEditable = isEditable;
        }

        if (typeof masterData.StockTypeList === "undefined" || masterData.StockTypeList == null) {
            masterData.StockTypeList = [];
        }

        var stockTypeCell = "";
        if (!isApprovePage & !isAcknowledgePage & (status == statusConstants.PROPOSED || status == statusConstants.PENDING || status == statusConstants.REVISE)) {
            stockTypeCell = {
                field: 'StockTypeId',
                headerText: 'Stock Type',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.StockTypeList,
                displayField: "text",
                allowEditing: true,
                visible: !masterData.IsReqForYD && masterData.RequisitionType != "Bulk",
                edit: ej2GridDropDownObj({
                })
            };

        }
        else {
            stockTypeCell = {
                field: 'StockTypeName',
                headerText: 'Stock Type',
                allowEditing: false,
                visible: !masterData.IsReqForYD && masterData.RequisitionType != "Bulk"
            }
        }
        var additionalColumns = [
            { field: 'RnDReqChildID', isPrimaryKey: false, visible: false },
            { field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false },
            { field: 'PreProcessRevNo', headerText: 'KPM RevisionNo', visible: false },
            { field: 'GroupConceptNo', headerText: 'Group Concept No', visible: false },
            { field: 'YarnCategory', headerText: 'Yarn Detail', allowEditing: false },
            { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false },
            { field: 'YarnLotNo', headerText: 'Yarn Lot', allowEditing: false },
            { field: 'Spinner', headerText: 'Spinner', allowEditing: false },
            //{
            //    field: 'YarnBrandID', headerText: 'Yarn Brand', allowEditing: false, valueAccessor: ej2GridDisplayFormatter,
            //    dataSource: masterData.YarnBrandList, displayField: "YarnBrand",
            //    edit: ej2GridDropDownObj({
            //    })
            //},
            { field: 'BatchNo', headerText: 'Batch No', visible: masterData.IsReqForYD },
            { field: 'StockQty', headerText: 'Stock Qty', allowEditing: false, visible: !masterData.IsReqForYD && masterData.RequisitionType != "Bulk"},
            { field: 'YarnReqQty', headerText: 'Yarn Req Qty(Kg)', visible: addAdditionalReq == false ? true : false, allowEditing: false, textAlign: 'Right' },
            { field: 'PendingQty', headerText: 'Pending Qty(Kg)', visible: addAdditionalReq == false ? true : false, allowEditing: false, textAlign: 'Right' },
            { field: 'ReqQty', headerText: 'Req Qty(Kg)', allowEditing: _isNewWithoutKnittingInfo == false ? isEditable : true, editType: "numericedit", textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 2, min: 0 } } },
            { field: 'ReqCone', headerText: 'Req Cone(PCS)', allowEditing: _isNewWithoutKnittingInfo == false ? isEditable : true, editType: "numericedit", textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 0, min: 0, format: "N" } } },
            { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false, visible: false },
            { field: 'Remarks', headerText: 'Remarks', allowEditing: isRemarksEditable }
        ];
        columns.push.apply(columns, additionalColumns);

        var indexS = columns.findIndex(x => x.field == 'BatchNo');
        indexS = indexS + 1;
        columns.splice(indexS, 0, stockTypeCell);

        $tblChildEl = new initEJ2Grid({
            tableId: tblChildId,
            data: data,
            columns: columns,
            actionBegin: function (args) {
         
                if (args.requestType === "save") {
                    if (args.data.ReqQty > args.data.MaxReqQty && addAdditionalReq == false && isApprovePage == false) {
                        toastr.error(`Maximum Remaining Qty is ${args.data.MaxReqQty} !!!`);
                        args.data.ReqQty = args.data.MaxReqQty;
                    }
                    if (!masterData.IsReqForYD) {
                        args.data.StockQty = parseInt(args.data.StockTypeId) == 3 ? parseFloat(args.data.AdvanceStockQty) : parseFloat(args.data.SampleStockQty);
                    }
                    args.rowData = args.data;
                }
                else if (args.requestType === "delete") {
                    data.Childs = $tblChildEl.getCurrentViewRecords();
                    data.Childs = data.Childs.filter(x => x.RnDReqChildID != args.data[0].RnDReqChildID);

                    initChildTable(data.Childs);
                }
            },
            autofitColumns: true,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false,
            //toolbar: ['Add'],
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true }
        });
    }

    function backToListWithoutFilter() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
    }

    function backToList() {
        backToListWithoutFilter();
        initMasterTable();
        //getPoupMRData();
        initChildTable([]);
        initFCMRTable([]);
        if ($tblFreeConceptMREl) $tblFreeConceptMREl.destroy();
        $tblChildEl.destroy();
        PopUpMRList = [];
    }

    function resetForm() {
        filterBy = {};
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#RnDReqMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getDetails(id, flag, RequisitionType = null) {

        _isNewWithoutKnittingInfo = false;
        operationKnittingInfoFields();
        var path = '';
        if (RequisitionType == 'R&D') {
            RequisitionType = 'RnD';
            reqType = 'RnD';
        }
        else {
            reqType = 'Bulk';
        }
        if (isAcknowledgePage == true && status == statusConstants.ACKNOWLEDGE) {
            path = 'yarn-rnd-requisition/groupBy';
        } else {
            path = 'yarn-rnd-requisition';
        }
        axios.get(`/api/${path}/${id}/${flag}/${RequisitionType}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;

                masterData.RnDReqDate = formatDateToDefault(masterData.RnDReqDate);
                //isEditable = true;
                $formEl.find("#IsReqForYD").prop("disabled", true);
                setFormData($formEl, masterData);

                if (masterData.IsApprove || masterData.IsAcknowledge) {
                    $formEl.find("#btnSave").fadeOut();
                } else {
                    $formEl.find("#btnSave").fadeIn();
                }

                if (status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE) {
                    $formEl.find("#btnAkgYPR, #btnAkgYPRUN").show();
                } else {
                    $formEl.find("#btnAkgYPR, #btnAkgYPRUN").hide();
                }

                //console.log(masterData.YarnBrandList);
                initChildTable(masterData.Childs);


                initFCMRTable(masterData.FreeConceptMR);
                if (isApprovePage && status == statusConstants.PENDING) {
                    $formEl.find("#btnSave").hide();
                    $formEl.find("#btnApproveYPR").show();
                }
                else if (status == statusConstants.PROPOSED || status == statusConstants.PENDING) {
                    $formEl.find("#btnSave").show();
                    $formEl.find("#btnApproveYPR").hide();
                }
                else {
                    $formEl.find("#btnSave").hide();
                    $formEl.find("#btnApproveYPR").hide();
                }
                $formEl.find("#btnRevise").fadeOut();

                _isNewWithoutKnittingInfo = masterData.IsWOKnittingInfo;
                operationKnittingInfoFields();
            })
            .catch(showResponseError);
    }

    function getReviseDetails(id, flag, mrId) {
        _isNewWithoutKnittingInfo = false;
        operationKnittingInfoFields();
        if (RequisitionType == 'R&D') {
            reqType = 'RnD';
        }
        else {
            reqType = 'Bulk';
        }
        axios.get(`/api/yarn-rnd-requisition/revise/${id}/${flag}/${mrId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.RnDReqDate = formatDateToDefault(masterData.RnDReqDate);
                $formEl.find("#IsReqForYD").prop("disabled", true);
                setFormData($formEl, masterData);

                initChildTable(masterData.Childs);

                initFCMRTable(masterData.FreeConceptMR);
                $formEl.find("#btnRevise").fadeIn();
                $formEl.find("#btnSave,#btnApproveYPR").fadeOut();
                $formEl.find("#btnAkgYPR, #btnAkgYPRUN").hide();

                _isNewWithoutKnittingInfo = masterData.IsWOKnittingInfo;
                operationKnittingInfoFields();
            })
            .catch(showResponseError);
    }

    function getFCData(fcIds) {
        axios.get(`/api/yarn-rnd-requisition/${fcIds}`)
            .then(function (response) {

                masterData = response.data;
                masterData.RnDReqDate = formatDateToDefault(masterData.RnDReqDate);
                $formEl.find("#IsReqForYD").prop("disabled", true);
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
                initFCMRTable(masterData.FreeConceptMR);
                $tblFreeConceptMREl.refresh();

                _isNewWithoutKnittingInfo = masterData.IsWOKnittingInfo;
                operationKnittingInfoFields();
            })
            .catch(showResponseError);
    }

    function getFCDataForSave(data) {
        $divDetailsEl.fadeIn();
        $divTblEl.fadeOut();
        $formEl.find("#btnSave").fadeIn();
        initFCMRTable([]);

        masterData = data;
        masterData.RnDReqDate = formatDateToDefault(masterData.RnDReqDate);
        $formEl.find("#IsReqForYD").prop("disabled", true);
        setFormData($formEl, masterData);
        $formEl.find("#btnAkgYPR, #btnAkgYPRUN").hide();
        masterData.Childs = masterData.Childs.filter(function (el) {
            return el.YDItem == masterData.IsReqForYD;
        });
        initChildTable(masterData.Childs);

        initFCMRTable(masterData.FreeConceptMR);
        $tblFreeConceptMREl.refresh();
    }

    function save(sendForApproval = false) {
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);
        if ($tblChildEl.getCurrentViewRecords().length == 0) return toastr.error("No Item found!");
        var data = formElToJson($formEl);
        data.IsReqForYD = convertToBoolean(data.IsReqForYD);
        data.Childs = $tblChildEl.getCurrentViewRecords();
        
        var hasError = false;
        //var hasError = false;
        for (var i = 0; i < data.Childs.length; i++) {
            var child = data.Childs[i];
            if (!child.YDItem) {
                var totalReqQty = getTotalReqQtyOfCurrentItem(data.Childs, child.YarnStockSetId);
                var maxReqQty = child.StockQty; //getMaxAllocatedQty(data.Childs, child.YBChildItemID);
                if (totalReqQty > maxReqQty) {
                    toastr.error(`Total req qty (${totalReqQty}) cannot be greater than maximum stock qty (${maxReqQty}). `);
                    hasError = true;
                    break;
                }
            }
        }
        if (hasError) return false;

        for (var i = 0; i < data.Childs.length; i++) {
            var child = data.Childs[i];
            if (!masterData.IsReqForYD) {
                if (child.StockTypeId == 0) {
                    toastr.error(`Select stock type at row ${i + 1}`);
                    hasError = true;
                    break;
                }
                if (child.StockQty == 0) {
                    toastr.error(`Stock quantity unavailable at row ${i + 1}`);
                    hasError = true;
                    break;
                }
            }
            if (child.ReqQty <= 0) {
                toastr.error(`Give Req qty at row ${i + 1}`);
                hasError = true;
                break;
            }
        }
        if (hasError) return false;

        masterData.SendForApproval = sendForApproval;
        data.IsAdditional = addAdditionalReq;
        data.ParentRnDReqNo = masterData.ParentRnDReqNo;

        if (_isNewWithoutKnittingInfo) {
            if (data.RCompanyID == null || data.RCompanyID == 0) {
                return toastr.error("Select company");
            }
            data.IsWOKnittingInfo = _isNewWithoutKnittingInfo;
        }

        axios.post("/api/yarn-rnd-requisition/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function getTotalReqQtyOfCurrentItem(childs, yarnStockSetID) {
        var totalReqQty = 0;
        childs.filter(x => x.YarnStockSetId == yarnStockSetID).map(x => {
            totalReqQty += getDefaultValueWhenInvalidN_Float(x.ReqQty);
        });
        return totalReqQty.toFixed(2);
    }
    function saveForApproval() {
        var data = formElToJson($formEl);
        data.Childs = masterData.Childs;
        axios.post("/api/yarn-rnd-requisition/saveForApproval", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function approvePR(id) {
        showBootboxConfirm("Approve Record.", "Are you sure want to approve?", function (yes) {
            if (yes) {
                var url = `/api/yarn-rnd-requisition/approve/${id}`;
                axios.post(url)
                    .then(function () {
                        toastr.success("Successfully approved!");
                        backToList();
                    })
                    .catch(showResponseError);
            }
        });
    }

    function acknowledgePR(id, reason) {
        showBootboxConfirm("Acknowledge Record.", "Are you sure want to Acknowledge?", function (yes) {
            if (yes) {
                var url = `/api/yarn-rnd-requisition/acknowledge/${id}/${reqType}`;
                axios.post(url)
                    .then(function () {
                        toastr.success("Successfully Acknowledged!");
                        backToList();
                    })
                    .catch(showResponseError);
            }
        });
    }

    function revise() {
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        if ($tblChildEl.getCurrentViewRecords().length == 0) return toastr.error("No Item found!");

        var data = formElToJson($formEl);
        data.IsReqForYD = convertToBoolean(data.IsReqForYD);
        data.Childs = $tblChildEl.getCurrentViewRecords();

        var hasError = false;
        for (var i = 0; i < data.Childs.length; i++) {
            var child = data.Childs[i];
            if (!masterData.IsReqForYD) {
                if (child.StockTypeId == 0) {
                    toastr.error(`Select stock type at row ${i + 1}`);
                    hasError = true;
                    break;
                }
                if (child.StockQty == 0) {
                    toastr.error(`Stock quantity unavailable at row ${i + 1}`);
                    hasError = true;
                    break;
                }
            }
            if (child.ReqQty <= 0) {
                toastr.error(`Give Req qty at row ${i + 1}`);
                hasError = true;
                break;
            }
        }
        if (hasError) return false;


        axios.post("/api/yarn-rnd-requisition/revise", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function resetGlobals() {
        reqType = ""
    }
})();
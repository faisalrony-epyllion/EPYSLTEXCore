(function () {
    var menuId, pageName, menuParam;
    var toolbarId, pageId;
    var status = statusConstants.PENDING;
    var isBookingPage = false;
    var isAcknowledgePage = false;
    var isApprovePage = false;
    var isYDBBPage = false;
    var addAdditionalReq = false;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $formEl, tblMasterId, $tblColorChildEl, tblChildId, tblPrvChildId,
        $tblChildEl, $tblPrvChildEl, $tblChildTwistingEl, tblChildTwistingId, tblTwistingColorId, $tblTwistingColorEL,
        $tblChildDyedCalculationEl, tblChildDyedCalculationId;
    var masterData = null;
    var masterDataPrv = null;
    var isCopyFromPreviousData = false;

    var twistedChilds = [],
        twistedColorChilds = [],
        twistedColorDetailsChilds = []; //finalTwistedColors

    var copiedRecord = null;
    var copiedTwistingRecord = null;
    var maxFCMRChildId = 99999;
    var maxYDBCTwistingID = 99999;
    var maxColorIDForOtherBookingFor = 999999;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        if (!menuParam)
            menuParam = localStorage.getItem("menuParam");
        debugger;
        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblPrvChildId = pageConstants.CHILD_TBL_ID_PREFIX + "Prv" + pageId;
        tblChildTwistingId = "#tblChildTwisting" + pageId;
        $tblColorChildEl = $("#tblColorChild" + pageId);
        tblTwistingColorId = "#tblPrintingColor" + pageId;
        tblChildDyedCalculationId = "#tblChildDyedCalculation" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        //isBookingPage = false;
        //isAcknowledgePage = false;
        //isApprovePage = false;
        //isYDBBPage = false;
        debugger;
        //isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val());
        //isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());
        //isYDBBPage = convertToBoolean($(`#${pageId}`).find("#YDBBPage").val());
        isAcknowledgePage = menuParam === "Ack";
        isApprovePage = menuParam === "A";
        isYDBBPage = menuParam === "YDBB";

        $formEl.find("#divDyeingDetailsCalculation,#btnBackToTwisted").hide();
        $formEl.find("#btnNextForDyedCalculation").show();

        if (isAcknowledgePage) {
            $formEl.find("#div_YDBNo").hide();
            status = statusConstants.APPROVED;
            $toolbarEl.find("#btnNew").hide();
            $toolbarEl.find("#btnList").hide();
            $toolbarEl.find("#btnPendingList").hide();
            $toolbarEl.find("#btnProposeList").hide();
            $toolbarEl.find("#btnApproveList").hide();
            $toolbarEl.find("#btnRevisionList").hide();
            $toolbarEl.find("#btnPendingAcknowlegeList").show();
            $toolbarEl.find("#btnAcknowledgeList").show();
            $toolbarEl.find("#btnUnAcnowledgeList").show();
            $toolbarEl.find("#btnCompleteList").show();

            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingAcknowlegeList"), $toolbarEl);
            $formEl.find("#btnAcknowledge").show();
            $formEl.find("#btnUnAcknowledge").show();
            $formEl.find("#divUnAckReason").show();
            $formEl.find("#btnRevise").hide();
            $formEl.find("#btnSave").hide();
            $formEl.find("#btnSaveAndSend").hide();
            $formEl.find("#btnApprove").hide();
            $formEl.find("#btnApproveMailResend").hide();
            $formEl.find("#divRevisionReason").hide();
            $formEl.find("#btnYDBNoSave").hide();
        }
        else if (isApprovePage) {
            $formEl.find("#div_YDBNo").hide();
            status = statusConstants.PROPOSED;
            $toolbarEl.find("#btnNew").hide();
            $toolbarEl.find("#btnList").hide();
            $toolbarEl.find("#btnPendingList").hide();
            $toolbarEl.find("#btnProposeList").show();
            $toolbarEl.find("#btnApproveList").show();
            $toolbarEl.find("#btnRevisionList").hide();
            $toolbarEl.find("#btnAcknowledgeList").hide();
            $toolbarEl.find("#btnUnAcnowledgeList").hide();
            $toolbarEl.find("#btnPendingAcknowlegeList").hide();
            $toolbarEl.find("#btnCompleteList").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnProposeList"), $toolbarEl);

            $formEl.find("#btnSave").hide();
            $formEl.find("#btnApprove").show();
            $formEl.find("#btnApproveMailResend").show();
            $formEl.find("#btnSaveAndSend").hide();
            $formEl.find("#btnAcknowledge").hide();
            $formEl.find("#btnUnAcknowledge").hide();
            $formEl.find("#divUnAckReason").hide();
            $formEl.find("#btnRevise").hide();
            $formEl.find("#divRevisionReason").hide();
            $formEl.find("#btnYDBNoSave").hide();
        }
        else if (isYDBBPage) {
            $formEl.find("#div_YDBNo").show();
            status = statusConstants.PENDING;
            $toolbarEl.find("#btnNew").hide();
            $toolbarEl.find("#btnList").hide();
            $toolbarEl.find("#btnPendingList").show();
            $toolbarEl.find("#btnProposeList").hide();
            $toolbarEl.find("#btnApproveList").hide();
            $toolbarEl.find("#btnRevisionList").hide();
            $toolbarEl.find("#btnAcknowledgeList").hide();
            $toolbarEl.find("#btnUnAcnowledgeList").hide();
            $toolbarEl.find("#btnPendingAcknowlegeList").hide();
            $toolbarEl.find("#btnCompleteList").show();

            //toggleActiveToolbarBtn($toolbarEl.find("#btnPendingList"), $toolbarEl);

            $formEl.find("#btnSave").hide();
            $formEl.find("#btnApprove").hide();
            $formEl.find("#btnApproveMailResend").hide();
            $formEl.find("#btnSaveAndSend").hide();
            $formEl.find("#btnAcknowledge").hide();
            $formEl.find("#btnUnAcknowledge").hide();
            $formEl.find("#divUnAckReason").hide();
            $formEl.find("#btnRevise").hide();
            $formEl.find("#divRevisionReason").hide();
            $formEl.find("#btnYDBNoSave").show();
            $formEl.find("#btnAddTwistedItems").hide();

        }
        else {
            $formEl.find("#div_YDBNo").hide();
            isBookingPage = true;
            $toolbarEl.find("#btnNew").show();
            $toolbarEl.find("#btnList").show();
            $toolbarEl.find("#btnPendingList").show();
            $toolbarEl.find("#btnProposeList").show();
            $toolbarEl.find("#btnApproveList").show();
            $toolbarEl.find("#btnAcknowledgeList").show();
            $toolbarEl.find("#btnUnAcnowledgeList").show();
            $toolbarEl.find("#btnRevisionList").show();
            $toolbarEl.find("#btnCompleteList").hide();
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingList"), $toolbarEl);
            $formEl.find("#btnSave").show();
            $formEl.find("#btnSaveAndSend").show();
            $formEl.find("#btnApprove").hide();
            $formEl.find("#btnApproveMailResend").hide();
            $formEl.find("#btnAcknowledge").hide();
            $formEl.find("#btnUnAcknowledge").hide();
            $formEl.find("#divUnAckReason").hide();
            $formEl.find("#btnRevise").show();
            $formEl.find("#divRevisionReason").hide();
            $toolbarEl.find("#btnPendingAcknowlegeList").hide();
            $toolbarEl.find("#btnYDBNoSave").hide();
        }

        //initMasterTable();

        $toolbarEl.find("#btnPendingList").click(function (e) {
            e.preventDefault();

            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            initMasterTable();
        });

        $toolbarEl.find("#btnList").click(function (e) {
            e.preventDefault();

            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PARTIALLY_COMPLETED;
            initMasterTable();
        });

        $toolbarEl.find("#btnProposeList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED;
            initMasterTable();
        });

        $toolbarEl.find("#btnApproveList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            initMasterTable();
        });

        $toolbarEl.find("#btnPendingAcknowlegeList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            initMasterTable();
        });

        $toolbarEl.find("#btnAcknowledgeList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACKNOWLEDGE;
            initMasterTable();
        });

        $toolbarEl.find("#btnUnAcnowledgeList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.UN_ACKNOWLEDGE;
            initMasterTable();
        });

        $toolbarEl.find("#btnRevisionList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REVISE;
            initMasterTable();
        });

        $toolbarEl.find("#btnCompleteList").click(function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.COMPLETED;
            initMasterTable();
        });

        $toolbarEl.find("#btnRefreshList").click(function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
        });

        $("#btnAddTwisting").click(addTwistingColorsFromAddTwisting);
        //$("#btnAddTwisting").click(addTwistingColors);

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(false);
        });

        $formEl.find("#btnSaveAndSend").click(function (e) {
            e.preventDefault();
            save(true);
        });
        $formEl.find("#btnYDBNoSave").click(function (e) {
            debugger;
            let ydBNo = $formEl.find("#YDBNo").val()
            if (ydBNo == null || ydBNo == "" || ydBNo == undefined) {
                toastr.error("Please insert YDB No!!!");
                return;
            }
            e.preventDefault();
            saveYDBNo();
        });

        $formEl.find("#btnRevise").click(function (e) {
            e.preventDefault();
            saveRevision();
        });

        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#YDBookingMasterID").val();
            axios.post(`/api/yd-booking/acknowledge/${id}`)
                .then(function () {
                    toastr.success("Booking acknowledged successfully.");
                    backToList();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnUnAcknowledge").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#YDBookingMasterID").val();
            var unackreason = $formEl.find("#UnAckReason").val();
            if (!unackreason) {
                toastr.error("Give Unacknowledge Reason");
                $formEl.find("#UnAckReason").focus();
                return false;
            }
            axios.post(`/api/yd-booking/unacknowledge/${id}/${unackreason}`)
                .then(function () {
                    toastr.success("Booking Unacknowledged successfully.");
                    backToList();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnApprove").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#YDBookingMasterID").val();
            axios.post(`/api/yd-booking/approve/${id}`)
                .then(function () {
                    toastr.success("Booking approved successfully.");
                    backToList();
                })
                .catch(showResponseError);
        });
        $formEl.find("#btnApproveMailResend").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#YDBookingMasterID").val();
            axios.post(`/api/yd-booking/approvemailresend/${id}`)
                .then(function () {
                    toastr.success("Mail Sent successfully.");
                    backToList();
                })
                .catch(showResponseError);
        });
        $formEl.find("#btnCancel").click(backToList);
        $formEl.find("#btnAddTwistedItems").click(displayColors);

        $formEl.find("#btnNextForTwist").click(function (e) {
            e.preventDefault();
            var YDChilds = $tblChildEl.getCurrentViewRecords();
            YDChilds = JSON.parse(JSON.stringify(YDChilds));
            YDChilds = YDChilds.filter(x => x.IsTwisting == true);

            var bookingForRequired = _.some(YDChilds, function (el) { return el.BookingFor == 0 });
            if (bookingForRequired) return toastr.error("'Booking For' is required.");

            var requireColor = _.some(YDChilds, function (el) { return el.ColorID == 0 && el.BookingForName == "Yarn Dyeing" });
            if (requireColor) return toastr.error("Please select color for items where Booking for: Yarn Dyeing");

            var printColorsRequired = _.some(YDChilds, function (el) { return el.PrintColors.length == 0 && el.BookingForName === "Yarn Printing" });
            if (printColorsRequired) return toastr.error("You must select print colors where 'Booking For' is 'Yarn Printing'.");

            var physicalCount = _.some(YDChilds, function (el) { return el.PhysicalCount == null });
            if (physicalCount) return toastr.error("Physical Count is required.");

            if (masterData.IsBDS != 2) {
                var lotNo = _.some(YDChilds, function (el) { return el.LotNo == null });
                if (lotNo) return toastr.error("Lot No is required.");
            } else {
                YDChilds.map(x => {
                    x.LotNo = "";
                    x.SpinnerID = 0;
                });
            }

            showBootboxConfirm("Confirmation", "Are you sure?", function (yes) {
                if (yes) {
                    $formEl.find("#divTwistedDetails,#btnBackToDyeing").show();
                    $formEl.find("#btnNextForTwist").hide();

                    setAndLoadTwistedChilds(YDChilds);
                    setTwistingColorsForModal(YDChilds);
                }
            });
        });

        $formEl.find("#btnBackToDyeing").click(function (e) {
            e.preventDefault();
            $formEl.find("#divTwistedDetails,#btnBackToDyeing").hide();
            $formEl.find("#btnNextForTwist").show();
        });

        $formEl.find("#btnNextForDyedCalculation").click(function (e) {

            e.preventDefault();
            var YDChilds = $tblChildEl.getCurrentViewRecords();
            YDChilds = JSON.parse(JSON.stringify(YDChilds));
            var twistedCalculatedYDChilds = [];
            //YDChilds.map(ydc => {
            //    var newYdc = JSON.parse(JSON.stringify(ydc));
            //    newYdc.BookingQty = setRemainingQty(newYdc);
            //    if (newYdc.BookingQty > 0) {
            //        twistedCalculatedYDChilds.push(newYdc);
            //    }
            //});
            for (var i = 0; i < YDChilds.length; i++) {
                var ydc = YDChilds[i];
                var newYdc = JSON.parse(JSON.stringify(ydc));
                newYdc.BookingQty = setRemainingQty(newYdc);
                if (newYdc.BookingQty > 0) {
                    twistedCalculatedYDChilds.push(newYdc);
                }
            }

            $formEl.find("#divDyeingDetailsCalculation,#btnBackToTwisted").show();
            $formEl.find("#btnNextForDyedCalculation").hide();
            initChildDyedCalculation(twistedCalculatedYDChilds);
        });

        $formEl.find("#btnBackToTwisted").click(function (e) {
            e.preventDefault();
            initChildDyedCalculation([]);
            $formEl.find("#divDyeingDetailsCalculation,#btnBackToTwisted").hide();
            $formEl.find("#btnNextForDyedCalculation").show();
        });
    });
    function setRemainingQty(ydChild) {

        var totalUsedQty = 0;
        twistedColorDetailsChilds.filter(cc => cc.ItemMasterID == ydChild.ItemMasterID &&
            cc.ColorID == ydChild.ColorID).map(cd => {
                totalUsedQty += cd.AssignQty;
            });
        var remainingQty = parseFloat(ydChild.BookingQty) - parseFloat(totalUsedQty);
        return parseFloat(remainingQty.toFixed(2));
    }
    function setAndLoadTwistedChilds(ydChilds) {
        twistedChilds = [];
        var maxYDBCTwistID = getMaxIdForArray(ydChilds, "YDBookingChildID");
        ydChilds.map(ydc => {
            twistedChilds.push({
                YDBCTwistingID: maxYDBCTwistID++,
                YDBookingMasterID: ydc.YDBookingMasterID,
                ItemMasterID: ydc.ItemMasterID,
                Remarks: ydc.Remarks,
                YarnCategory: ydc.YarnCategory,
                NoOfThread: ydc.NoOfThread,
                YarnDyedColorID: ydc.YarnDyedColorID,
                UnitID: ydc.UnitID,
                BookingQty: 0,//ydc.BookingQty,
                YarnProgramID: ydc.YarnProgramID,
                FCMRChildID: ydc.FCMRChildID,
                YBChildItemID: ydc.YBChildItemID,
                ProgramName: ydc.ProgramName,
                ColorID: ydc.ColorID,
                ColorCode: ydc.ColorCode,
                ColorName: ydc.ColorName,
                TwistedColors: ydc.TwistedColors,
                RequestRecipe: ydc.RequestRecipe,
                RequestBy: ydc.RequestBy,
                RequestDate: ydc.RequestDate,
                RequestAck: ydc.RequestAck,
                RequestAckBy: ydc.RequestAckBy,
                RequestAckDate: ydc.RequestAckDate,
                DPID: ydc.DPID,
                DPProcessInfo: ydc.DPProcessInfo,
                NoOfCone: ydc.NoOfCone,
                IsTwisting: ydc.IsTwisting,
                IsWaxing: ydc.IsWaxing,
                UsesIn: ydc.UsesIn,
                ShadeCode: ydc.ShadeCode,
                PhysicalCount: ydc.PhysicalCount,
                PrintedDensity: ydc.PrintedDensity,
                TPI: ydc.TPI,
                DisplayUnitDesc: ydc.DisplayUnitDesc,
                Segment1ValueId: ydc.Segment1ValueId,
                Segment2ValueId: ydc.Segment2ValueId,
                Segment3ValueId: ydc.Segment3ValueId,
                Segment4ValueId: ydc.Segment4ValueId,
                Segment5ValueId: ydc.Segment5ValueId,
                Segment6ValueId: ydc.Segment6ValueId,
                Segment7ValueId: ydc.Segment7ValueId,
                Segment1ValueDesc: ydc.Segment1ValueDesc,
                Segment2ValueDesc: ydc.Segment2ValueDesc,
                Segment3ValueDesc: ydc.Segment3ValueDesc,
                Segment4ValueDesc: ydc.Segment4ValueDesc,
                Segment5ValueDesc: ydc.Segment5ValueDesc,
                Segment6ValueDesc: ydc.Segment6ValueDesc,
                Segment7ValueDesc: ydc.Segment7ValueDesc
            });
        });
        twistedChilds.map(x => {
            var list = twistedColorDetailsChilds.filter(y => y.FCMRChildID == x.FCMRChildID);
            x = setChildDataPropValues(x, list, "PrimaryTwistingColorID");
        });
        initTwistedChildTable(twistedChilds, false);
    }
    function setTwistingColorsForModal(ydChilds) {
        twistedColorChilds = [];
        var maxId = 1;

        ydChilds.filter(el => el.IsTwisting == true).map(x => {
            var bookingForName = "";
            if (x.BookingFor == 1) bookingForName = "Yarn Dyeing";
            else if (x.BookingFor == 2) bookingForName = "Yarn Printing";
            else if (x.BookingFor == 3) bookingForName = "Yarn Twist";

            twistedColorChilds.push({
                PrimaryTwistingColorID: x.YDBookingChildID,
                YDBCTwistingColorID: maxId++,
                YDBCTwistingID: x.YDBCTwistingID,
                YDBookingChildID: x.YDBookingChildID,
                YDBookingMasterID: x.YDBookingMasterID,
                ItemMasterID: x.ItemMasterID,
                BookingForName: bookingForName,
                ColorID: x.ColorID,
                ColorName: x.ColorName,
                ColorCode: x.ColorCode,
                PhysicalCount: x.PhysicalCount,
                LotNo: x.LotNo,
                TwistingColorQty: x.BookingQty,
                AssignQty: 0,
                PreviousAssignQty: 0,
                BalanceQty: x.BookingQty,
                Segment1ValueDesc: x.Segment1ValueDesc,
                Segment2ValueDesc: x.Segment2ValueDesc,
                Segment3ValueDesc: x.Segment3ValueDesc,
                Segment4ValueDesc: x.Segment4ValueDesc,
                Segment5ValueDesc: x.Segment5ValueDesc,
                Segment6ValueDesc: x.Segment6ValueDesc,
                Segment7ValueDesc: x.Segment7ValueDesc
            });
        });
    }

    function initMasterTable() {
        var commands = [];
        //if (status == statusConstants.PENDING) {
        //    commands = [
        //        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }
        //    ]
        //}
        //else if (status == statusConstants.APPROVED || status == statusConstants.ACKNOWLEDGE || status == statusConstants.UN_ACKNOWLEDGE) {
        //    commands = [
        //        { type: 'Addition', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } },
        //        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
        //        { type: 'YD Booking', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } },
        //        { type: 'Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } },
        //        { type: 'View Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image' } }
        //    ]
        //}
        //else {
        //    commands = [
        //        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
        //        { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } }
        //    ]
        //}
        if (isBookingPage && status == statusConstants.PENDING) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }
            ]
        }
        else if ((isBookingPage && (status == statusConstants.PARTIALLY_COMPLETED || status == statusConstants.PROPOSED || status == statusConstants.REVISE)) || (isApprovePage && status == statusConstants.PROPOSED)) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } }
            ]
        }
        else if (isBookingPage && status == statusConstants.APPROVED) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'Addition', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } },
                { type: 'YD Booking', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } },
            ]
        }
        else if ((isBookingPage && (status == statusConstants.ACKNOWLEDGE || status == statusConstants.UN_ACKNOWLEDGE)) || (isAcknowledgePage && (status == statusConstants.APPROVED || status == statusConstants.ACKNOWLEDGE || status == statusConstants.UN_ACKNOWLEDGE))) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'YD Booking', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } },
                { type: 'Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } },
                { type: 'View Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image' } },
                { type: 'Booking Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image' } }
            ]
        }
        else if (isApprovePage && status == statusConstants.APPROVED) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'YD Booking', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } }
            ]
        }
        else if (isYDBBPage && status == statusConstants.PENDING) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
            ]
        }
        else if (status == statusConstants.COMPLETED) {
            commands = [
                { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'e-btn-icon fa fa-eye' } },
            ]
        }
        var columns = [];

        columns = [
            {
                headerText: 'Action', width: ch_setActionCommandCellWidth(commands), textAlign: 'Center', commands: commands
            },
            {
                field: 'PreProcessRevNo', headerText: 'PreProcessRevNo', visible: false
            },
            {
                field: 'YDBNo', headerText: 'YDB No', visible: status == statusConstants.COMPLETED
            },
            {
                field: 'YDBookingNo', headerText: 'YD Booking No', visible: status != statusConstants.PENDING
            },
            {
                field: 'YDBookingDate', headerText: 'YD Booking Date', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            //{
            //    field: 'ConceptNo', headerText: 'Concept No'
            //},
            {
                field: 'GroupConceptNo', headerText: 'Group Concept No'
            },
            {
                field: 'YBookingNo', headerText: 'Yarn Booking No'
            },
            {
                field: 'FabricBookingNo', headerText: 'Fabric Booking No', visible: status != statusConstants.PENDING
            },
            {
                field: 'Remarks', headerText: 'Remarks', visible: status != statusConstants.PENDING
            },
            {
                field: 'PDate', headerText: 'Concept/Yarn Booking Date', visible: status == statusConstants.PENDING, type: 'date', format: _ch_date_format_1,
            },
            {
                field: 'ProgramName', headerText: 'Program Name'//, visible: status == statusConstants.PENDING
            },
            {
                field: 'BuyerName', headerText: 'Buyer Name'
            },
            {
                field: 'UnAckReason', headerText: 'Unacknowledge Reason', visible: status == statusConstants.UN_ACKNOWLEDGE
            },
            {
                field: 'MRStatus', headerText: 'MRS Status', visible: status == statusConstants.COMPLETED
            },
            {
                field: 'RRStatus', headerText: 'Recipe Req. Status', visible: status == statusConstants.COMPLETED
            },
            {
                field: 'RStatus', headerText: 'Receive Status', visible: status == statusConstants.COMPLETED
            }
        ];
        var paramPageName = isYDBBPage ? pageNameConstants.YDBB : pageName;
        if ($tblMasterEl) $tblMasterEl.destroy();
        debugger;
        $tblMasterEl = initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/yd-booking/list?status=${status}&pageName=${paramPageName}`,
            columns: columns,
            autofitColumns: false,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (status === statusConstants.PENDING) {
            if (isYDBBPage) {
                if (args.rowData.ProgramName == 'Concept' || args.rowData.ProgramName == 'BDS') {
                    getDetails(args.rowData.YDBookingMasterID, "Concept", false);
                }
                else {
                    getDetails(args.rowData.YDBookingMasterID, "BulkOrBDS", false);
                }
            }
            else {
                if (args.rowData.ProgramName == 'Concept') {
                    getNew(args.rowData.GroupConceptNo, args.rowData.ProgramName);
                }
                else if (args.rowData.ProgramName == 'BDS') {
                    getNew(args.rowData.GroupConceptNo, args.rowData.ProgramName);
                }
                else {
                    getNew(args.rowData.GroupConceptNo, args.rowData.ProgramName);
                }
            }

            addAdditionalReq = false;
        }
        else if (status === statusConstants.REVISE) {
            if (args.commandColumn.type == "Report") {
                window.open(`/reports/InlinePdfView?ReportName=YarnDyedBooking.rdl&YDBookingNo=${args.rowData.YDBookingNo}`, '_blank');
            }
            else if (args.rowData.ProgramName == 'Concept' || args.rowData.ProgramName == 'BDS') {
                getReviseDetails(args.rowData.YDBookingMasterID, args.rowData.GroupConceptNo, "Concept");
            }
            else {
                getReviseDetails(args.rowData.YDBookingMasterID, args.rowData.GroupConceptNo, "BulkOrBDS");
            }
            addAdditionalReq = false;
        }
        else if (status === statusConstants.APPROVED || status === statusConstants.ACKNOWLEDGE || status === statusConstants.UN_ACKNOWLEDGE) {
            if (args.commandColumn.type == "YD Booking") {
                window.open(`/reports/InlinePdfView?ReportName=YarnDyedBooking.rdl&YDBookingNo=${args.rowData.YDBookingNo}`, '_blank');
            }
            else if (args.commandColumn.type == 'Booking Report' && args.rowData.ProgramName == "Bulk") {
                if (args.rowData.IsSample == 1) {
                    window.open(`/reports/InlinePdfView?ReportName=SampleFabric.rdl&BookingNo=${args.rowData.GroupConceptNo}`, '_blank');
                } else {
                    window.open(`/reports/InlinePdfView?ReportName=BookingInformationFabricMainForPMCN.rdl&BookingNo=${args.rowData.GroupConceptNo}`, '_blank');
                }
            }
            else if (args.commandColumn.type == 'Booking Report' && args.rowData.ProgramName == "BDS") {
                window.open(`/reports/InlinePdfView?ReportName=SampleFabric.rdl&BookingNo=${args.rowData.GroupConceptNo}`, '_blank');
            }
            else if ((args.commandColumn.type == 'View Attachment' || args.commandColumn.type == 'Booking Attachment') && args.rowData.ProgramName == "BDS") {
                if (typeof args.rowData.ImagePath == "undefined" || args.rowData.ImagePath == '') {
                    toastr.error("No attachment found!!");
                } else {
                    var imagePath = constants.GMT_ERP_BASE_PATH + args.rowData.ImagePath;
                    window.open(imagePath, "_blank");
                }
            }
            else if (args.commandColumn.type == 'View Attachment' && args.rowData.ProgramName == "Bulk") {
                if (typeof args.rowData.ImagePath == "undefined" || args.rowData.ImagePath == '') {
                    toastr.error("No attachment found!!");
                } else {
                    var imagePath = constants.GMT_ERP_BASE_PATH + args.rowData.ImagePath;
                    window.open(imagePath, "_blank");
                }
            }
            else if (args.commandColumn.type == 'Booking Attachment' && args.rowData.ProgramName == "Bulk") {
                if (typeof args.rowData.ImagePath1 == "undefined" || args.rowData.ImagePath1 == '') {
                    toastr.error("No attachment found!!");
                } else {
                    var imagePath1 = constants.GMT_ERP_BASE_PATH + args.rowData.ImagePath1;
                    window.open(imagePath1, "_blank");
                }
            }
            else if (args.commandColumn.type == "Addition") {
                addAdditionalReq = true;
                if (args.rowData.ProgramName == 'Concept' || args.rowData.ProgramName == 'BDS') {
                    getDetails(args.rowData.YDBookingMasterID, "Concept", false);
                }
                else {
                    getDetails(args.rowData.YDBookingMasterID, "BulkOrBDS", false);
                }
            }
            else if (args.commandColumn.type == "Edit") {
                addAdditionalReq = false;
                if (args.rowData.ProgramName == 'Concept' || args.rowData.ProgramName == 'BDS') {
                    getDetails(args.rowData.YDBookingMasterID, "Concept", false);
                }
                else {
                    getDetails(args.rowData.YDBookingMasterID, "BulkOrBDS", false);
                }
            }
            else if (args.rowData.ProgramName == 'Concept' || args.rowData.ProgramName == 'BDS') {
                getReviseDetails(args.rowData.YDBookingMasterID, args.rowData.GroupConceptNo, "Concept");
                addAdditionalReq = false;
            }
            else {
                getReviseDetails(args.rowData.YDBookingMasterID, args.rowData.GroupConceptNo, "BulkOrBDS");
                addAdditionalReq = false;
            }
        }
        else {
            if (args.commandColumn.type == "Report") {
                window.open(`/reports/InlinePdfView?ReportName=YarnDyedBooking.rdl&YDBookingNo=${args.rowData.YDBookingNo}`, '_blank');
            }
            else {
                if (args.rowData.ProgramName == 'Concept' || args.rowData.ProgramName == 'BDS') {
                    getDetails(args.rowData.YDBookingMasterID, "Concept", false);
                }
                else {
                    getDetails(args.rowData.YDBookingMasterID, "BulkOrBDS", false);
                }
            }
            addAdditionalReq = false;
        }
    }

    //Previous Dyeing Item Information
    async function initChildTablePrv() {
        if ($tblPrvChildEl) $tblPrvChildEl.destroy();
        var columns = await getYarnItemColumnsForDisplayOnly();

        var additionalColumns = [
            { field: 'YDBookingChildID', isPrimaryKey: true, visible: false },
            { field: 'FCMRChildID', visible: false },
            { field: 'YD', visible: false },
            { field: 'YDItem', visible: false },
            {
                field: 'ShadeCode', headerText: 'Shade Code'
                , valueAccessor: ej2GridDisplayFormatter, dataSource: masterDataPrv.YarnShadeBooks, displayField: "ShadeCode", edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'BookingFor', headerText: 'Booking For'
                , valueAccessor: ej2GridDisplayFormatter, displayField: "BookingForName", dataSource: masterDataPrv.YarnDyeingForList, edit: ej2GridDropDownObj({
                })
            },
            { field: 'ColorName', headerText: 'Color', allowEditing: false },
            { field: 'ColorCode', headerText: 'Color Code' },
            { field: 'IsTwisting', headerText: 'Twisting?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            { field: 'IsWaxing', headerText: 'Waxing?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            { field: 'UsesIns', headerText: 'Uses In', allowEditing: false },
            {
                field: 'SpinnerID', headerText: 'Spinner', valueAccessor: ej2GridDisplayFormatter, dataSource: masterDataPrv.SpinnerList, displayField: "SpinnerName", edit: ej2GridDropDownObj({
                })
            },
            { field: 'LotNo', headerText: 'Lot No' },
            { field: 'PhysicalCount', headerText: 'Count / Physical Count' },
            {
                field: 'BookingQty', headerText: 'Booking Qty(Kg)', editType: "numericedit",
                textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 2, min: 0, format: "N2" } }
            },
            {
                field: 'NoOfCone', headerText: 'No Of Cone (Pcs)',
                editType: "numericedit", edit: { params: { validateDecimalOnType: true, showSpinButton: false, decimals: 2, min: 0, format: "N2" } }
            },
            { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks' }
        ];
        columns.push.apply(columns, additionalColumns);

        $tblPrvChildEl = new initEJ2Grid({
            tableId: tblPrvChildId,
            data: masterDataPrv.YDBookingChilds,
            columns: columns,
            actionBegin: function (args) {

            },
            autofitColumns: true,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false,
            enableSingleClickEdit: true,
            editSettings: { allowAdding: false, allowEditing: false, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: false },
            enableContextMenu: true,
            contextMenuItems: [
                { text: 'Copy', target: '.e-content', id: 'copy' }
            ],
            contextMenuClick: function (args) {
                if (args.item.id === 'copy') {
                    copiedRecord = objectCopy(args.rowInfo.rowData);
                    copiedRecord.YDBookingChildID = 0;
                    isCopyFromPreviousData = true;
                }
            }
        });
    }
    async function getYarnColumns(displayColumns) {
        if (typeof displayColumns === "undefined" || displayColumns == null) displayColumns = [];
        var yarnList = await getYarnItemColumnsForDisplayOnly();
        yarnList.map(x => {
            if (x.headerText) {
                var indexF = displayColumns.findIndex(y => y.toLowerCase() == x.headerText.toLowerCase());
                if (indexF == -1) x.visible = false;
            }
        });
        return yarnList;
    }
    //Dyeing Item Information
    async function initChildTable(data) {
        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];

        if (!isAcknowledgePage && !isApprovePage && !isYDBBPage) {
            columns.push(
                {
                    headerText: 'Commands', width: 120, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },

                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
                }
            );
        }


        columns.push.apply(columns, await getYarnColumns(["Composition", "Count"]));

        var additionalColumns = [
            { field: 'YDBookingChildID', isPrimaryKey: true, visible: false },
            { field: 'FCMRChildID', visible: false },

            {
                field: 'BookingFor', headerText: 'Booking For'
                , valueAccessor: ej2GridDisplayFormatter, displayField: "BookingForName", dataSource: masterData.YarnDyeingForList, edit: ej2GridDropDownObj({
                })
            },
            //{
            //    headerText: '', textAlign: 'Center', width: 40, commands: [
            //        { buttonOption: { type: 'printColors', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select Print Colors for 'Yarn Printing'" } }
            //    ]
            //},
            { field: 'ColorName', headerText: 'Color', allowEditing: false },
            {
                headerText: '', textAlign: 'Center', width: 40, commands: [
                    { buttonOption: { type: 'mainColor', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select Color for 'Yarn Dyeing'" } }
                ]
            },
            { field: 'ColorCode', headerText: 'Color Code' },
            { field: 'IsTwisting', headerText: 'Twisting?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            { field: 'UsesIns', headerText: 'Uses In', allowEditing: false },
            {
                headerText: '', textAlign: 'Center', width: 40, commands: [
                    { buttonOption: { type: 'UsesIn', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
                ]
            },
            { field: 'PhysicalCount', headerText: 'Count / Physical Count' },
            { field: 'LotNo', headerText: 'Lot No' },
            {
                field: 'SpinnerID',
                headerText: 'Spinner',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.SpinnerList,
                displayField: "SpinnerName",
                edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'BookingQty', headerText: 'Booking Qty(Kg)', editType: "numericedit",
                textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 2, min: 0, format: "N2" } }
            },
            {
                field: 'NoOfCone', headerText: 'No Of Cone (Pcs)',
                editType: "numericedit", edit: { params: { validateDecimalOnType: true, showSpinButton: false, decimals: 2, min: 0, format: "N2" } }
            },
            { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks' }

            /*
            { field: 'YD', visible: false },
            { field: 'YDItem', visible: false },
            {
                field: 'ShadeCode', headerText: 'Shade Code'
                , valueAccessor: ej2GridDisplayFormatter, dataSource: masterData.YarnShadeBooks, displayField: "ShadeCode", edit: ej2GridDropDownObj({
                })
            },
            { field: 'IsWaxing', headerText: 'Waxing?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
           
            {
                field: 'SpinnerID', headerText: 'Spinner', valueAccessor: ej2GridDisplayFormatter, dataSource: masterData.SpinnerList, displayField: "SpinnerName", edit: ej2GridDropDownObj({
                })
            },
            */
        ];
        columns.push.apply(columns, additionalColumns);

        $tblChildEl = new initEJ2Grid({
            tableId: tblChildId,
            data: data,
            columns: columns,
            commandClick: childCommandClick,
            actionBegin: function (args) {
                if (args.requestType === 'beginEdit') {
                    if (args.rowData.YDProductionMasterID > 0) {
                        toastr.error("Yarn Dyeing found, You cannot modify anything.");
                        args.cancel = true;
                    }
                }
                else if (args.requestType === "save" && args.action == "edit") {
                    var hasError = false;
                    var childTwistings = $tblChildTwistingEl.getCurrentViewRecords();
                    for (var i = 0; i < childTwistings.length; i++) {
                        var twistingColors = childTwistings[i].YDBookingChildTwistingColors.filter(x => x.YDBookingChildID == args.rowData.YDBookingChildID);
                        if (twistingColors.length > 0) {
                            toastr.error(`Already has Final / Twisted Item Information, can't change`);
                            hasError = true;
                            break;
                        }
                    }

                    if (hasError) {
                        args.rowData = args.previousData;
                        args.data = args.previousData;

                        $tblChildEl.updateRow(args.rowIndex, args.previousData);
                        return false;
                    }

                    if (args.data.BookingFor == 3) {
                        args.data.IsTwisting = true;
                        args.rowData.IsTwisting = true;
                    }

                    childTwistings.map(x => {
                        x.YDBookingChildTwistingColors.filter(y => y.BookingForName == "Yarn Twist" || y.BookingFor == 3).map(y => {
                            y.ColorID = 0;
                        });
                        x.ColorIDs = x.YDBookingChildTwistingColors.map(t => t.ColorID).join(",");
                    });

                    if (args.rowData.IsTwisting && !args.data.IsTwisting) {
                        var twistingList = $tblChildTwistingEl.getCurrentViewRecords();
                        if (twistingList.length > 0) {
                            for (var i = 0; i < twistingList.length; i++) {
                                var item = twistingList[i];
                                var colorIDs = item.ColorIDs.split(',');
                                var indexF = colorIDs.findIndex(x => parseInt(x) == args.rowData.ColorID);
                                if (indexF > -1) {
                                    toastr.error(`Twisting cannot be unchecked because Color ${args.rowData.ColorName} already used in twisted item information.`);
                                    args.data.IsTwisting = true;
                                    break;
                                }
                            }
                        }
                    }

                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.YDBookingChildID);
                    if (args.data.SpinnerID == null) {
                        args.data.SpinnerID = 0;
                    }
                    $tblChildEl.updateRow(index, args.data);

                    if (args.data.IsTwisting) {

                        var indexF = twistedColorChilds.findIndex(c => c.ColorID == args.data.ColorID);
                        if (indexF == -1) {
                            var x = args.data;
                            var bookingForName = "";
                            if (x.BookingFor == 1) bookingForName = "Yarn Dyeing";
                            else if (x.BookingFor == 2) bookingForName = "Yarn Printing";
                            else if (x.BookingFor == 3) bookingForName = "Yarn Twist";

                            twistedColorChilds.push({
                                PrimaryTwistingColorID: x.YDBookingChildID,
                                YDBCTwistingColorID: twistedColorChilds.length + 1,
                                YDBCTwistingID: x.YDBCTwistingID,
                                YDBookingChildID: x.YDBookingChildID,
                                YDBookingMasterID: x.YDBookingMasterID,
                                BookingForName: bookingForName,
                                ItemMasterID: x.ItemMasterID,
                                ColorID: x.ColorID,
                                ColorName: x.ColorName,
                                ColorCode: x.ColorCode,
                                PhysicalCount: x.PhysicalCount,
                                LotNo: x.LotNo,
                                TwistingColorQty: x.BookingQty,
                                AssignQty: 0,
                                PreviousAssignQty: 0,
                                BalanceQty: x.BookingQty,
                                UsedQty: 0,
                                Segment1ValueDesc: x.Segment1ValueDesc,
                                Segment2ValueDesc: x.Segment2ValueDesc,
                                Segment3ValueDesc: x.Segment3ValueDesc,
                                Segment4ValueDesc: x.Segment4ValueDesc,
                                Segment5ValueDesc: x.Segment5ValueDesc,
                                Segment6ValueDesc: x.Segment6ValueDesc,
                                Segment7ValueDesc: x.Segment7ValueDesc
                            });
                        }
                        else {
                            twistedColorChilds.splice(indexF, 1);
                        }
                    }
                }
                else if (args.requestType === "add") {
                    args.data.YDBookingChildID = getMaxIdForArray(masterData.YDBookingChilds, "YDBookingChildID");
                }
                else if (args.requestType === "delete") {
                    if (args.data[0].YDProductionMasterID > 0) {
                        toastr.error("Yarn Dyeing found, You cannot modify anything.");
                        args.cancel = true;
                    }
                }
            },
            autofitColumns: true,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false,
            enableSingleClickEdit: true,
            editSettings: { allowAdding: !isYDBBPage, allowEditing: !isYDBBPage, allowDeleting: !isYDBBPage, mode: "Normal", showDeleteConfirmDialog: true },
            enableContextMenu: true,
            contextMenuItems: [
                { text: 'Copy', target: '.e-content', id: 'copy' },
                { text: 'Paste', target: '.e-content', id: 'paste' },
                { text: 'Copy And Paste', target: '.e-content', id: 'copyAndPaste' },
            ],
            contextMenuClick: function (args) {
                if (args.item.id === 'copy') {
                    isCopyFromPreviousData = false;
                    copiedRecord = objectCopy(args.rowInfo.rowData);
                    copiedRecord.ColorID = 0;
                    copiedRecord.ColorName = "";
                    copiedRecord.ColorCode = "";
                    copiedRecord.PrintColors = [];
                }
                else if (args.item.id === 'paste') {
                    if (copiedRecord == null) {
                        toastr.error("Please copy first!!");
                        return;
                    }
                    if (!isCopyFromPreviousData) {
                        var cr = objectCopy(copiedRecord);
                        cr.YDBookingChildID = getMaxIdForArray($tblChildEl.getCurrentViewRecords(), "YDBookingChildID");
                        cr.YDBookingChild_DemoID = getMaxIdForArray($tblChildEl.getCurrentViewRecords(), "YDBookingChildID");
                        cr.FCMRChildID = maxFCMRChildId--;
                        $tblChildEl.addRecord(cr);
                    }
                    else {
                        var cr = objectCopy(copiedRecord);
                        args.rowInfo.rowData.ColorID = cr.ColorID;
                        args.rowInfo.rowData.ColorName = cr.ColorName;
                        args.rowInfo.rowData.ColorCode = cr.ColorCode;
                        args.rowInfo.rowData.PrintColors = cr.PrintColors;
                        args.rowInfo.rowData.YarnProgramID = cr.YarnProgramID;
                        args.rowInfo.rowData.YarnDyedColorID = cr.YarnDyedColorID;
                        args.rowInfo.rowData.Remarks = cr.Remarks;
                        args.rowInfo.rowData.YarnCategory = cr.YarnCategory;
                        args.rowInfo.rowData.NoOfThread = cr.NoOfThread;
                        args.rowInfo.rowData.ShadeCode = cr.ShadeCode;
                        args.rowInfo.rowData.BookingFor = cr.BookingFor;
                        args.rowInfo.rowData.IsTwisting = cr.IsTwisting;
                        args.rowInfo.rowData.IsWaxing = cr.IsWaxing;

                        args.rowInfo.rowData.UsesIns = cr.UsesIns;
                        args.rowInfo.rowData.UsesInIDs = cr.UsesInIDs;
                        var usesInIDs = cr.UsesInIDs != null ? cr.UsesInIDs.split(',') : [];
                        var usesList = [];
                        for (var i = 0; i < usesInIDs.length; i++) {
                            var useObj = {
                                UsesIn: usesInIDs[i]
                            }
                            usesList.push(useObj);
                        }
                        args.rowInfo.rowData.YDBookingChildUsesIns = usesList;

                        args.rowInfo.rowData.IsAdditionalItem = cr.IsAdditionalItem;
                        args.rowInfo.rowData.SpinnerID = cr.SpinnerID;
                        args.rowInfo.rowData.LotNo = cr.LotNo;
                        args.rowInfo.rowData.PhysicalCount = cr.PhysicalCount;
                        $tblChildEl.updateRow(args.rowInfo.rowIndex, args.rowInfo.rowData);
                    }
                }
                else if (args.item.id === 'copyAndPaste') {
                    isCopyFromPreviousData = false;
                    copiedRecord = objectCopy(args.rowInfo.rowData);
                    copiedRecord.ColorID = 0;
                    copiedRecord.ColorName = "";
                    copiedRecord.ColorCode = "";
                    copiedRecord.PrintColors = [];
                    copiedRecord.YDBookingChildID = getMaxIdForArray($tblChildEl.getCurrentViewRecords(), "YDBookingChildID");
                    copiedRecord.YDBookingChild_DemoID = getMaxIdForArray($tblChildEl.getCurrentViewRecords(), "YDBookingChildID");
                    copiedRecord.FCMRChildID = maxFCMRChildId--;
                    $tblChildEl.addRecord(copiedRecord);
                }
            }
        });
    }
    async function childCommandClick(e) {
        childData = e.rowData;
        machinetype = e.commandColumn.buttonOption.type;
        if (e.commandColumn.buttonOption.type == 'mainColor') {
            if (childData.BookingForName == "Yarn Dyeing") {
                var response = await axios.get("/api/fabric-color-book-setups/allcolor");

                response.data.Items = response.data.Items.sort(function (a, b) {
                    return a.ColorName.length - b.ColorName.length || 0;
                });

                var finder = new commonFinder({
                    title: "Select Color",
                    pageId: pageId,
                    height: 320,
                    //apiEndPoint: "/api/fabric-color-book-setups/allcolor",
                    data: response.data.Items,
                    fields: "ColorName,ColorSource,ColorCode,RGBOrHex",
                    headerTexts: "Color Name,Source,Code,Color",
                    customFormats: ",,,ej2GridColorFormatter",
                    isMultiselect: false,
                    primaryKeyColumn: "ColorID",
                    onSelect: function (selectedRecord) {
                        finder.hideModal();
                        childData.ColorID = selectedRecord.rowData.ColorID;
                        childData.ColorName = selectedRecord.rowData.ColorName;
                        childData.ColorCode = selectedRecord.rowData.ColorCode;

                        var index = $tblChildEl.getRowIndexByPrimaryKey(childData.YDBookingChildID);
                        $tblChildEl.updateRow(index, childData);
                    }
                });
                finder.showModal();
            }
            if (childData.BookingForName == "Yarn Printing") {
                var hasMajorColor = false;
                var response = await axios.get("/api/fabric-color-book-setups/allcolor");
                var finder = new commonFinder({
                    title: "Select Printing Colors",
                    pageId: pageId,
                    height: 320,
                    data: response.data.Items,
                    fields: "ColorName,IsMajor,ColorSource,ColorCode,RGBOrHex",
                    headerTexts: "Color Name,Major Color?,Source,Code,Color",
                    customFormats: ",,,,ej2GridColorFormatter",
                    editTypes: ",booleanedit,,,",
                    isMultiselect: true,
                    selectedIds: childData.PrintColorIDs,
                    allowEditing: true,
                    autofitColumns: true,
                    primaryKeyColumn: "ColorID",
                    onMultiselect: function (selectedRecords) {
                        hasMajorColor = _.some(selectedRecords, function (el) { return el.IsMajor });
                        if (!hasMajorColor) return toastr.error("You must select a major color.");

                        var majorColorIndex = selectedRecords.findIndex(function (el) { return el.IsMajor });
                        for (var i = 0; i < selectedRecords.length; i++) {
                            selectedRecords[i].IsMajor = false;
                            selectedRecords[i].YDBookingChildID = childData.YDBookingChildID;
                        }

                        selectedRecords[majorColorIndex].IsMajor = true;

                        childData.PrintColors = selectedRecords;
                        childData.ColorID = selectedRecords[majorColorIndex].ColorID;
                        childData.ColorName = selectedRecords[majorColorIndex].ColorName;
                        childData.ColorCode = selectedRecords[majorColorIndex].ColorCode;
                        childData.PrintColorIDs = selectedRecords.map(function (el) { return el.ColorID }).toString();
                        var index = $tblChildEl.getRowIndexByPrimaryKey(childData.YDBookingChildID);
                        $tblChildEl.updateRow(index, DeepClone(childData));
                    },
                    onFinish: function () {
                        if (hasMajorColor) finder.hideModal();
                    }
                });
                finder.showModal();
            }
        }
        /*
        else if (e.commandColumn.buttonOption.type == 'printColors' && childData.BookingForName == "Yarn Printing") {
            var hasMajorColor = false;
            var response = await axios.get("/api/fabric-color-book-setups/allcolor");
            var finder = new commonFinder({
                title: "Select Printing Colors",
                pageId: pageId,
                height: 320,
                data: response.data.Items,
                fields: "ColorName,IsMajor,ColorSource,ColorCode,RGBOrHex",
                headerTexts: "Color Name,Major Color?,Source,Code,Color",
                customFormats: ",,,,ej2GridColorFormatter",
                editTypes: ",booleanedit,,,",
                isMultiselect: true,
                selectedIds: childData.PrintColorIDs,
                allowEditing: true,
                autofitColumns: true,
                primaryKeyColumn: "ColorID",
                onMultiselect: function (selectedRecords) {
                    hasMajorColor = _.some(selectedRecords, function (el) { return el.IsMajor });
                    if (!hasMajorColor) return toastr.error("You must select a major color.");

                    var majorColorIndex = selectedRecords.findIndex(function (el) { return el.IsMajor });
                    for (var i = 0; i < selectedRecords.length; i++) {
                        selectedRecords[i].IsMajor = false;
                        selectedRecords[i].YDBookingChildID = childData.YDBookingChildID;
                    }

                    selectedRecords[majorColorIndex].IsMajor = true;

                    childData.PrintColors = selectedRecords;
                    childData.ColorID = selectedRecords[majorColorIndex].ColorID;
                    childData.ColorName = selectedRecords[majorColorIndex].ColorName;
                    childData.ColorCode = selectedRecords[majorColorIndex].ColorCode;
                    childData.PrintColorIDs = selectedRecords.map(function (el) { return el.ColorID }).toString();
                    var index = $tblChildEl.getRowIndexByPrimaryKey(childData.YDBookingChildID);
                    $tblChildEl.updateRow(index, childData);
                },
                onFinish: function () {
                    if (hasMajorColor) finder.hideModal();
                }
            });
            finder.showModal();
        }
        */
        else if (e.commandColumn.buttonOption.type == 'UsesIn') {
            var finder = new commonFinder({
                title: "Select Uses In",
                pageId: pageId,
                height: 320,
                data: masterData.UsesInList,
                fields: "text",
                headerTexts: "Name",
                isMultiselect: true,
                selectedIds: childData.UsesInIDs,
                primaryKeyColumn: "id",
                onMultiselect: function (selectedRecords) {
                    childData.UsesIns = selectedRecords.map(function (el) { return el.text }).toString();

                    var usesList = [];
                    for (var i = 0; i < selectedRecords.length; i++) {
                        var useObj = {
                            UsesIn: selectedRecords[i].id
                        }
                        usesList.push(useObj);
                    }
                    childData.YDBookingChildUsesIns = usesList;
                    childData.UsesInIDs = selectedRecords.map(function (el) { return el.id }).toString();
                    var index = $tblChildEl.getRowIndexByPrimaryKey(childData.YDBookingChildID);
                    $tblChildEl.updateRow(index, childData);
                }
            });
            finder.showModal();
        }
    }
    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
    //Final / Twisted(if any) Item Information
    async function initTwistedChildTable(data, isLoadList) {
        if (!isLoadList) data = [];
        if ($tblChildTwistingEl) $tblChildTwistingEl.destroy();
        var columns = [];

        columns.push(
            {
                headerText: 'Commands', width: 120, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', visible: false, buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
            }
        );

        columns.push.apply(columns, await getYarnColumns());

        var additionalColumns = [
            { field: 'YDBCTwistingID', isPrimaryKey: true, visible: false },
            { field: 'FCMRChildID', visible: false },
            { field: 'PhysicalCount', headerText: 'Count / Physical Count', allowEditing: false },
            { field: 'ColorName', headerText: 'Major Color', allowEditing: false },
            { field: 'TwistedColors', headerText: 'Twisted Colors', allowEditing: false },
            {
                headerText: '', textAlign: 'Center', width: 40, commands: [
                    { buttonOption: { type: 'twistedColor', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
                ]
            },
            { field: 'TPI', headerText: 'Twist/Inch', editType: "numericedit", edit: { params: { validateDecimalOnType: true, showSpinButton: false, decimals: 2, min: 0, format: "N" } } },
            { field: 'BookingQty', headerText: 'Booking Qty(Kg)', allowEditing: false },
            { field: 'NoOfCone', headerText: 'No Of Cone (Pcs)', allowEditing: true },
            { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks', allowEditing: true }



            /*
            { field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false },
            //{ field: 'ColorID', headerText: 'ColorID', allowEditing: false, visible: false },
            { field: 'ColorCode', headerText: 'Color Code', allowEditing: false },
           
            { field: 'IsTwisting', headerText: 'Twisting?', displayAsCheckBox: true, textAlign: 'Center', allowEditing: false },
            { field: 'IsWaxing', headerText: 'Waxing?', displayAsCheckBox: true, textAlign: 'Center', allowEditing: false },
            */
        ];
        columns.push.apply(columns, additionalColumns);

        $tblChildTwistingEl = new initEJ2Grid({
            tableId: tblChildTwistingId,
            data: data,
            columns: columns,
            commandClick: twistingChildCommandClick,
            autofitColumns: true,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false,
            enableSingleClickEdit: true,
            editSettings: {
                allowAdding: !isYDBBPage, allowEditing: !isYDBBPage, allowDeleting: !isYDBBPage,
                mode: "Normal", showDeleteConfirmDialog: true
            },
            enableContextMenu: true,
            contextMenuItems: [
                { text: 'Copy', target: '.e-content', id: 'copy' },
                { text: 'Paste', target: '.e-content', id: 'paste' },
                { text: 'Copy And Paste', target: '.e-content', id: 'copyAndPaste' },
            ],
            contextMenuClick: function (args) {
                if (args.item.id === 'copy') {
                    copiedTwistingRecord = objectCopy(args.rowInfo.rowData);
                }
                else if (args.item.id === 'paste') {
                    if (copiedTwistingRecord == null) {
                        toastr.error("Please copy first!!");
                        return;
                    }
                    var cr = objectCopy(copiedTwistingRecord);
                    cr.YDBCTwistingID = getMaxIdForArray($tblChildTwistingEl.getCurrentViewRecords(), "YDBCTwistingID");
                    $tblChildTwistingEl.addRecord(cr);
                }
                else if (args.item.id === 'copyAndPaste') {
                    copiedTwistingRecord = objectCopy(args.rowInfo.rowData);
                    copiedTwistingRecord.YDBCTwistingID = getMaxIdForArray($tblChildTwistingEl.getCurrentViewRecords(), "YDBCTwistingID");
                    $tblChildTwistingEl.addRecord(copiedTwistingRecord);
                }
            }
        });
    }

    function twistingChildCommandClick(e) {
        if (e.commandColumn.type === 'Edit' || e.commandColumn.type === 'Delete') {
            if (e.rowData.YDProductionMasterID > 0) {
                toastr.error("Yarn Dyeing found, You cannot modify anything.");
                e.cancel = true;
            } else {
                var cData = e.rowData;
                twistedChilds = twistedChilds.filter(x => x.YDBCTwistingID != cData.YDBCTwistingID);
                twistedColorDetailsChilds = twistedColorDetailsChilds.filter(x => x.YDBCTwistingID != cData.YDBCTwistingID);
            }
        }
        else {
            childData = e.rowData;

            var assignQty = 0;
            twistedColorChilds.map(x => {
                x.YDBCTwistingID = childData.YDBCTwistingID;
                x.FCMRChildID = childData.FCMRChildID;

                var tempList = twistedColorDetailsChilds.filter(y => y.YDBookingChildID == x.YDBookingChildID &&
                    y.YDBCTwistingID == x.YDBCTwistingID);

                tempList.map(y => assignQty += y.AssignQty);
                x.AssignQty = assignQty;
                assignQty = 0;
            });

            //twistedColorDetailsChilds
            if (e.commandColumn.buttonOption.type == 'twistedColor' && childData.IsTwisting) {
                //var aaa = twistedColorDetailsChilds;
                if (twistedColorDetailsChilds.length > 0) {
                    twistedColorChilds.map(x => {
                        var obj = twistedColorDetailsChilds.find(y => y.YDBookingChildID == x.YDBookingChildID &&
                            y.FCMRChildID == x.FCMRChildID);
                        if (obj) {
                            x.AssignQty = obj.AssignQty;
                        } else {
                            x.AssignQty = 0;
                        }
                    });
                }
                twistedColorChilds = getTwistedColorChildsWithQtys(twistedColorChilds);
                $("#btnAddTwisting").hide();
                initTwistingColorTable(twistedColorChilds, childData.TwistedSelectedColorIDs);
                $("#modalTwistingColorSelection").modal('show');
            }
        }
    }
    function getTwistedColorChildsWithQtys(tColorChilds) {
        tColorChilds.map(tcc => {
            var usedQty = getUsedQty(tcc);
            tcc.BalanceQty = parseFloat(tcc.TwistingColorQty) - usedQty;
            tcc.BalanceQty = tcc.BalanceQty.toFixed(2);
            tcc.UsedQty = usedQty;
        });
        return tColorChilds;
    }
    function getUsedQty(twistedColorObj) {
        var usedQty = 0;
        var singleTwistedColors = twistedColorDetailsChilds.filter(cc => cc.ColorID == twistedColorObj.ColorID &&
            cc.ItemMasterID == twistedColorObj.ItemMasterID &&
            cc.FCMRChildID != twistedColorObj.FCMRChildID)
        //cc.YDBCTwistingID != twistedColorObj.YDBCTwistingID);
        singleTwistedColors.map(cc => usedQty += cc.AssignQty);
        return parseFloat(usedQty.toFixed(2));
    }
    //Select Twisted Printing Color in the modal
    function initTwistingColorTable(data, selectedIds) {
        var columns = [
            { field: 'PrimaryTwistingColorID', isPrimaryKey: true, visible: false },
            { field: 'YDBCTwistingColorID', visible: false },
            { field: 'ItemMasterID', visible: false },
            { field: 'FCMRChildID', visible: false },
            { type: 'checkbox', width: 50 },
            { field: 'ColorName', headerText: 'Color', allowEditing: false, width: '100px' },
            { field: 'ColorCode', headerText: 'Color Code', allowEditing: false, width: '100px' },
            //{ field: 'TwistingColorQty', headerText: 'Qty', editType: "numericedit", width: '100px' },// edit: { params: { decimals: 0 } } },
            {
                field: 'TwistingColorQty', headerText: 'Qty', editType: "numericedit", allowEditing: false, width: '100px',
                textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 2, min: 0, format: "N2" } }
            },
            {
                field: 'AssignQty', headerText: 'Assign Qty', editType: "numericedit", width: '100px',
                textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 2, min: 0, format: "N2" } }
            },
            {
                field: 'UsedQty', headerText: 'Used Qty', editType: "numericedit", allowEditing: false, width: '100px',
                textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 2, min: 0, format: "N2" } }
            },
            {
                field: 'BalanceQty', headerText: 'Balance Qty', editType: "numericedit", allowEditing: false, width: '100px',
                textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 2, min: 0, format: "N2" } }
            },
            { field: 'Segment1ValueDesc', headerText: 'Composition', allowEditing: false, width: '100px' },
            { field: 'Segment2ValueDesc', headerText: 'Yarn Type', allowEditing: false, width: '100px' },
            { field: 'Segment3ValueDesc', headerText: 'Process', allowEditing: false, width: '100px' },
            { field: 'Segment4ValueDesc', headerText: 'Sub Process', allowEditing: false, width: '100px' },
            { field: 'Segment5ValueDesc', headerText: 'Quality Parameter', allowEditing: false, width: '100px' },
            { field: 'Segment6ValueDesc', headerText: 'Count', allowEditing: false, width: '100px' },
            //{ field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false, width: '100px' },
            { field: 'Segment7ValueDesc', headerText: 'No of Ply', allowEditing: false, width: '100px' }
        ];

        if ($tblTwistingColorEL) $tblTwistingColorEL.destroy();
        ej.base.enableRipple(true);
        $tblTwistingColorEL = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            columns: columns,
            enableSingleClickEdit: true,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                }
                else if (args.requestType === "save") {
                    var usedQty = getUsedQty(args.data);
                    args.data.BalanceQty = parseFloat(args.data.TwistingColorQty) - usedQty;
                    args.data.BalanceQty = args.data.BalanceQty.toFixed(2);
                }
            },
            editSettings: { allowEditing: !isYDBBPage, allowAdding: !isYDBBPage, allowDeleting: !isYDBBPage, mode: "Normal", showDeleteConfirmDialog: true },
            selectionSettings: { type: "Multiple" },
            dataBound: function (args) {
                if (selectedIds != undefined) {
                    var ids = selectedIds.split(',');
                    var selIds = [];
                    for (var i = 0; i < ids.length; i++) {
                        selIds.push($tblTwistingColorEL.getRowIndexByPrimaryKey(parseInt(ids[i])));
                    }
                    this.selectRows(selIds);
                }
            }
        });
        $tblTwistingColorEL.refreshColumns;
        $tblTwistingColorEL.appendTo(tblTwistingColorId);
    }
    function removeItemFromTwistedColorDetailsChilds(selectedItems, fcMRChildID) {
        var deletedIndexs = [];
        if (selectedItems.length > 0) {
            for (var iCD = 0; iCD < twistedColorDetailsChilds.length; iCD++) {
                if (twistedColorDetailsChilds[iCD].FCMRChildID == fcMRChildID) {
                    var index = selectedItems.findIndex(x => x.FCMRChildID == twistedColorDetailsChilds[iCD].FCMRChildID &&
                        x.YDBookingChildID == twistedColorDetailsChilds[iCD].YDBookingChildID);
                    if (index < 0) {
                        deletedIndexs.push(iCD);
                    }
                }
            }
        } else {
            for (var iCD = 0; iCD < twistedColorDetailsChilds.length; iCD++) {
                if (twistedColorDetailsChilds[iCD].FCMRChildID == fcMRChildID) {
                    deletedIndexs.push(iCD);
                }
            }
        }

        if (deletedIndexs.length > 0) {
            for (var iCD = twistedColorDetailsChilds.length - 1; iCD >= 0; iCD--) {
                if ($.inArray(iCD, deletedIndexs) > -1) {
                    twistedColorDetailsChilds.splice(iCD, 1);
                }
            }
        }
    }

    function addTwistingColors() {
        var allRecords = $tblTwistingColorEL.getCurrentViewRecords();
        var selectedRecords = $tblTwistingColorEL.getSelectedRecords();

        var zeroQtyItems = selectedRecords.filter(x => x.AssignQty == 0);
        if (zeroQtyItems.length > 0) return toastr.error("Selected items quantity can not be zero.");

        var fcMRChildID = 0;
        if (selectedRecords.length > 0) fcMRChildID = selectedRecords[0].FCMRChildID;
        else if (allRecords.length > 0) {
            fcMRChildID = allRecords[0].FCMRChildID;
        }


        if (selectedRecords.length == 0) toastr.warning("You didn't select any record.");
        else if (selectedRecords.length === 1) {
            showBootboxConfirm("Twist Color Selections", "Do you want to twist with this same item?", function (yes) {
                if (yes) {
                    var hasError = setTwistedColors(childData, selectedRecords);
                    if (!hasError) {
                        $("#modalTwistingColorSelection").modal('hide');
                        removeItemFromTwistedColorDetailsChilds(selectedRecords, fcMRChildID);
                    }
                }
                else {
                    toastr.info("Can not twist a single Item.")
                }
            })
        }
        else {
            var hasError = setTwistedColors(childData, selectedRecords);
            if (!hasError) {
                $("#modalTwistingColorSelection").modal('hide');
                removeItemFromTwistedColorDetailsChilds(selectedRecords, fcMRChildID);
            }
        }
    }
    function setTwistedColors(childData, selectedRecords) {
        var totalBookingQty = 0;
        var hasError = false;
        for (var iSR = 0; iSR < selectedRecords.length; iSR++) {
            selectedRecords[iSR].YDBCTwistingID = childData.YDBCTwistingID;
            totalBookingQty += parseFloat(selectedRecords[iSR].AssignQty) || 0;

            var usedQty = getUsedQty(selectedRecords[iSR]);
            var balanceQty = selectedRecords[iSR].TwistingColorQty - usedQty;

            if (selectedRecords[iSR].AssignQty > balanceQty) {
                toastr.error("Maximum Remaining Qty " + balanceQty.toFixed(2) + " (Color : " + selectedRecords[iSR].ColorName + " & Color Code : " + selectedRecords[iSR].ColorCode + ")");
                hasError = true;
                return hasError;
            }

            var findObj = twistedColorDetailsChilds.find(y => y.YDBookingChildID == selectedRecords[iSR].YDBookingChildID &&
                y.YDBCTwistingID == selectedRecords[iSR].YDBCTwistingID);
            if (findObj) {
                var findIndex = twistedColorDetailsChilds.findIndex(y => y.YDBookingChildID == selectedRecords[iSR].YDBookingChildID &&
                    y.YDBCTwistingID == selectedRecords[iSR].YDBCTwistingID);

                if (findIndex > -1) {
                    twistedColorDetailsChilds[findIndex].AssignQty = selectedRecords[iSR].AssignQty;
                    twistedColorDetailsChilds[findIndex].ItemMasterID = selectedRecords[iSR].ItemMasterID;
                    twistedColorDetailsChilds[findIndex].UsedQty = usedQty;
                    twistedColorDetailsChilds[findIndex].BalanceQty = (balanceQty + selectedRecords[iSR].AssignQty).toFixed(2);
                }
            } else {
                var tcChild = {
                    PrimaryTwistingColorID: selectedRecords[iSR].YDBookingChildID,
                    YDBCTwistingColorID: selectedRecords[iSR].YDBCTwistingColorID,
                    YDBCTwistingID: selectedRecords[iSR].YDBCTwistingID,
                    YDBookingChildID: selectedRecords[iSR].YDBookingChildID,
                    ItemMasterID: selectedRecords[iSR].ItemMasterID,
                    ColorID: selectedRecords[iSR].ColorID,
                    ColorName: selectedRecords[iSR].ColorName,
                    ColorCode: selectedRecords[iSR].ColorCode,
                    FCMRChildID: selectedRecords[iSR].FCMRChildID,
                    TwistingColorQty: selectedRecords[iSR].TwistingColorQty,
                    AssignQty: selectedRecords[iSR].AssignQty,
                    UsedQty: usedQty,
                    BalanceQty: (balanceQty + selectedRecords[iSR].AssignQty).toFixed(2),
                    Segment1ValueDesc: selectedRecords[iSR].Segment1ValueDesc,
                    Segment2ValueDesc: selectedRecords[iSR].Segment2ValueDesc,
                    Segment3ValueDesc: selectedRecords[iSR].Segment3ValueDesc,
                    Segment4ValueDesc: selectedRecords[iSR].Segment4ValueDesc,
                    Segment5ValueDesc: selectedRecords[iSR].Segment5ValueDesc,
                    Segment6ValueDesc: selectedRecords[iSR].Segment6ValueDesc,
                    Segment7ValueDesc: selectedRecords[iSR].Segment7ValueDesc,
                    PhysicalCount: selectedRecords[iSR].PhysicalCount
                }

                twistedColorDetailsChilds.push(tcChild);
            }
            if (!hasError) {
                childData.BookingQty = totalBookingQty;
                childData = setChildDataPropValues(childData, selectedRecords, "YDBookingChildID");
                var index = $tblChildTwistingEl.getRowIndexByPrimaryKey(childData.YDBCTwistingID);
                $tblChildTwistingEl.updateRow(index, childData);
            }
        }
        return hasError;
    }
    function setChildDataPropValues(cData, listRecords, twistedColorPropName) {
        cData.TwistedColors = listRecords.map(x => x.ColorName).toString();
        if (twistedColorPropName == "PrimaryTwistingColorID" && listRecords.length > 0) {
            cData.PhysicalCount = listRecords[0].PhysicalCount;
            cData.BookingQty = listRecords[0].BookingQty;
            cData.TPI = listRecords[0].TPI;
        } else {
            cData.PhysicalCount = listRecords.map(x => x.PhysicalCount).join(' + ');
        }

        if (cData.BookingFor == '1' && !cData.IsTwisting) {
            cData.PhysicalCount = cData.PhysicalCount + "YD";
        } else if (cData.BookingFor == '1' && cData.IsTwisting) {
            cData.PhysicalCount = cData.PhysicalCount + "YD Twist";
        } else if (cData.BookingFor == '2') {
            cData.PhysicalCount = cData.PhysicalCount + "YP";
        }

        if (twistedColorPropName == "YDBookingChildID") {
            cData.TwistedSelectedColorIDs = listRecords.map(x => x.YDBookingChildID).join(",");
        } else {
            cData.TwistedSelectedColorIDs = listRecords.map(x => x.PrimaryTwistingColorID).join(",");
        }
        return cData;
    }
    function setTwistedColorInfo(childData, selectedRecords) {
        childData.TwistedColors = selectedRecords.map(x => x.ColorName).toString();
        childData.PhysicalCount = selectedRecords.map(x => x.PhysicalCount).join(' + ');
        if (childData.BookingFor == '1' && !childData.IsTwisting) {
            childData.PhysicalCount = childData.PhysicalCount + "YD";
        }
        if (childData.BookingFor == '2') {
            childData.PhysicalCount = childData.PhysicalCount + "YP";
        }
        if (childData.BookingFor == '1' && childData.IsTwisting) {
            childData.PhysicalCount = childData.PhysicalCount + "YD Twist";
        }

        for (var i = 0; i < selectedRecords.length; i++) {
            selectedRecords[i].YDBCTwistingID = childData.YDBCTwistingID;
        }
        //Update Booking Qty in the child list
        var BookingQty = 0;
        selectedRecords.forEach(function (obj) {
            BookingQty += parseFloat(obj.AssignQty) || 0;
        });
        childData.BookingQty = BookingQty;
        childData.TwistedSelectedColorIDs = selectedRecords.map(x => x.YDBookingChildID).join(",");

        //Add Details Record 
        var PreviousAssignQty = 0;
        for (var i = 0; i < selectedRecords.length; i++) {
            var exists = twistedColorDetailsChilds.find(x => x.YDBookingChildID == selectedRecords[i].YDBookingChildID &&
                x.YDBCTwistingID == selectedRecords[i].YDBCTwistingID);

            if (!exists) {
                var TCChild = {
                    PrimaryTwistingColorID: selectedRecords[i].YDBookingChildID,
                    YDBCTwistingColorID: selectedRecords[i].YDBCTwistingColorID,
                    YDBCTwistingID: selectedRecords[i].YDBCTwistingID,
                    YDBookingChildID: selectedRecords[i].YDBookingChildID,
                    ItemMasterID: selectedRecords[i].ItemMasterID,
                    ColorID: selectedRecords[i].ColorID,
                    ColorName: selectedRecords[i].ColorName,
                    ColorCode: selectedRecords[i].ColorCode,
                    FCMRChildID: selectedRecords[i].FCMRChildID,
                    TwistingColorQty: selectedRecords[i].TwistingColorQty,
                    AssignQty: selectedRecords[i].AssignQty,
                    PreviousAssignQty: PreviousAssignQty,
                    BalanceQty: (selectedRecords[i].TwistingColorQty) - (selectedRecords[i].AssignQty + PreviousAssignQty),  //selectedRecords[i].BalanceQty,
                    Segment1ValueDesc: selectedRecords[i].Segment1ValueDesc,
                    Segment2ValueDesc: selectedRecords[i].Segment2ValueDesc,
                    Segment3ValueDesc: selectedRecords[i].Segment3ValueDesc,
                    Segment4ValueDesc: selectedRecords[i].Segment4ValueDesc,
                    Segment5ValueDesc: selectedRecords[i].Segment5ValueDesc,
                    Segment6ValueDesc: selectedRecords[i].Segment6ValueDesc,
                    Segment7ValueDesc: selectedRecords[i].Segment7ValueDesc,
                    PhysicalCount: selectedRecords[i].PhysicalCount
                }

                twistedColorDetailsChilds.push(TCChild);
            }
            else {
                var findIndex = twistedColorDetailsChilds.findIndex(x => x.YDBookingChildID == selectedRecords[i].YDBookingChildID &&
                    x.YDBCTwistingID == selectedRecords[i].YDBCTwistingID);
                if (findIndex > -1) {
                    twistedColorDetailsChilds[findIndex].AssignQty = selectedRecords[i].AssignQty;
                    twistedColorDetailsChilds[findIndex].ItemMasterID = selectedRecords[i].ItemMasterID;
                    twistedColorDetailsChilds[findIndex].PreviousAssignQty = PreviousAssignQty;
                    twistedColorDetailsChilds[findIndex].BalanceQty = (selectedRecords[i].TwistingColorQty) - (selectedRecords[i].AssignQty + PreviousAssignQty);
                }
            }
        }

        //childData.YDBookingChildTwistingColors = twistedColorDetailsChilds;

        var index = $tblChildTwistingEl.getRowIndexByPrimaryKey(childData.YDBCTwistingID);
        $tblChildTwistingEl.updateRow(index, childData);
    }

    //Dyed Yarn Balance Qty
    async function initChildDyedCalculation(data) {
        var childTwistings = typeof $tblChildTwistingEl !== "undefined" ? $tblChildTwistingEl.getCurrentViewRecords() : [];
        data.map(d => {
            d.BalanceQty = d.BookingQty;
            childTwistings.map(x => {
                x.YDBookingChildTwistingColors.filter(y => y.ColorID == d.ColorID && y.BookingFor == d.BookingFor).map(y => {
                    d.BalanceQty = d.BalanceQty - y.AssignQty;
                });
            });
        });

        if ($tblChildDyedCalculationEl) $tblChildDyedCalculationEl.destroy();
        var columns = [];
        columns.push.apply(columns, await getYarnItemColumnsForDisplayOnly());
        var additionalColumns = [
            { field: 'YDBookingChildID', isPrimaryKey: true, visible: false },
            { field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false },
            { field: 'BookingForName', headerText: 'Booking For', allowEditing: false },
            { field: 'ColorName', headerText: 'Color', allowEditing: false },
            { field: 'ColorCode', headerText: 'Color Code', allowEditing: false },
            { field: 'IsTwisting', headerText: 'Twisting?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', allowEditing: false },
            { field: 'IsWaxing', headerText: 'Waxing?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', allowEditing: false },
            { field: 'UsesIns', headerText: 'Uses In', allowEditing: false },
            { field: 'SpinnerName', headerText: 'Spinner', allowEditing: false },
            { field: 'LotNo', headerText: 'Lot No', allowEditing: false },
            { field: 'PhysicalCount', headerText: 'Count / Physical Count', allowEditing: false },
            {
                field: 'BookingQty', headerText: 'Booking Qty(Kg)', editType: "numericedit", allowEditing: false,
                edit: { params: { validateDecimalOnType: true, showSpinButton: false, decimals: 0, format: "N", min: 1 } }
            },
            {
                field: 'BalanceQty', headerText: 'Balance Qty(Kg)', editType: "numericedit", allowEditing: false,
                edit: { params: { validateDecimalOnType: true, showSpinButton: false, decimals: 0, format: "N", min: 0 } }
            },
            { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false }
        ];
        columns.push.apply(columns, additionalColumns);

        data.map(x => {
            x.SpinnerName = x.SpinnerName == "Select" ? "" : x.SpinnerName;
            x.BookingQty = parseFloat(x.BookingQty.toFixed(2));
            x.BalanceQty = parseFloat(x.BalanceQty.toFixed(2));
        });

        $tblChildDyedCalculationEl = new initEJ2Grid({
            tableId: tblChildDyedCalculationId,
            data: data,
            columns: columns,
            autofitColumns: true,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false,
            enableContextMenu: true
        });
    }

    function getValidValue(value) {
        if (typeof value === "undefined" || value == null) return 0;
        return parseFloat(value).toFixed(2);
    }

    function displayColors() {
        var YDChilds = DeepClone($tblChildEl.getCurrentViewRecords());
        YDChilds = YDChilds.filter(x => x.IsTwisting == true);


        YDChilds.filter(x => x.BookingForName == "Yarn Twist").map(x => {
            if (x.ColorID == 0) {
                x.ColorID = maxColorIDForOtherBookingFor++;

                var mainList = DeepClone($tblChildEl.getCurrentViewRecords());

                var indexF = mainList.findIndex(y => y.BookingForName == x.BookingForName &&
                    y.PhysicalCount == x.PhysicalCount &&
                    y.LotNo == x.LotNo);

                if (indexF > -1) {
                    var obj = mainList.find(y => y.BookingForName == x.BookingForName &&
                        y.PhysicalCount == x.PhysicalCount &&
                        y.LotNo == x.LotNo);

                    obj.ColorID = x.ColorID;

                    $tblChildEl.updateRow(indexF, obj);
                }
            }
        });

        YDChilds = setTwistingCalculatedValues(YDChilds, true);

        $("#btnAddTwisting").show();
        initTwistingColorTableFromAddTwisting(YDChilds, "");
        $("#modalTwistingColorSelection").modal('show');
    }
    function getQtyFields(ydChilds) {
        ydChilds.map(x => {
            var usedQty = 0; //getUsedQty(tcc);
            x.TwistingColorQty = x.BookingQty.toFixed(2);
            x.AssignQty = 0;
            x.UsedQty = usedQty.toFixed(2);
            x.BalanceQty = x.TwistingColorQty - x.UsedQty;
            x.BalanceQty = x.BalanceQty.toFixed(2);
        });
        return ydChilds;
    }
    async function initTwistedChildTableFromAddTwisting(data, isLoadList) {
        if (!isLoadList) data = [];

        if ($tblChildTwistingEl) $tblChildTwistingEl.destroy();
        var columns = [];

        columns.push(
            {
                headerText: 'Commands', width: 120, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
            }
        );

        columns.push.apply(columns, await getYarnColumns());

        var additionalColumns = [
            { field: 'YDBCTwistingID', isPrimaryKey: true, visible: false },
            { field: 'FCMRChildID', visible: false },
            { field: 'PhysicalCount', headerText: 'Count / Physical Count', allowEditing: false },
            { field: 'ColorName', headerText: 'Major Color', allowEditing: false },
            { field: 'ColorCode', headerText: 'Color Code', allowEditing: false },
            { field: 'TwistedColors', headerText: 'Twisted Colors', allowEditing: false },
            {
                headerText: '', textAlign: 'Center', width: 40, commands: [
                    { buttonOption: { type: 'twistedColor1', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
                ]
            },
            { field: 'UsesIns', headerText: 'Uses In', allowEditing: false },
            {
                headerText: '', textAlign: 'Center', width: 40, commands: [
                    { buttonOption: { type: 'UsesIn', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
                ]
            },
            { field: 'TPI', headerText: 'Twist/Inch', editType: "numericedit", edit: { params: { validateDecimalOnType: true, showSpinButton: false, decimals: 2, min: 0, format: "N" } } },
            { field: 'BookingQty', headerText: 'Booking Qty(Kg)', allowEditing: false },
            { field: 'NoOfCone', headerText: 'No Of Cone (Pcs)', allowEditing: true },
            { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks', allowEditing: true },

            /*
            { field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false },
            //{ field: 'ColorID', headerText: 'ColorID', allowEditing: false, visible: false },
            { field: 'IsTwisting', headerText: 'Twisting?', displayAsCheckBox: true, textAlign: 'Center', allowEditing: false },
            { field: 'IsWaxing', headerText: 'Waxing?', displayAsCheckBox: true, textAlign: 'Center', allowEditing: false },
            */
        ];
        columns.push.apply(columns, additionalColumns);

        $tblChildTwistingEl = new initEJ2Grid({
            tableId: tblChildTwistingId,
            data: data,
            columns: columns,
            commandClick: twistingChildCommandClick2,
            autofitColumns: true,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false,
            enableSingleClickEdit: true,
            editSettings: {
                allowAdding: !isYDBBPage,
                allowEditing: !isYDBBPage,
                allowDeleting: !isYDBBPage,
                mode: "Normal",
                showDeleteConfirmDialog: true
            },
            actionBegin: function (args) {
                if (args.requestType === 'beginEdit') {

                }
                else if (args.requestType === "save" && args.action == "edit") {

                }
                else if (args.requestType === "add") {
                }
                else if (args.requestType === "delete") {

                    if (args.data.length > 0) {
                        var selectedRow = args.data[0];
                        var colorIDs = selectedRow.ColorIDs;

                        var indexF = twistedColorChilds.findIndex(c => c.ColorID == args.data[0].ColorID);
                        if (indexF > -1) {
                            twistedColorChilds[indexF].UsedQty = twistedColorChilds[indexF].UsedQty - selectedRow.AssignQty;
                            twistedColorChilds[indexF].BalanceQty = twistedColorChilds[indexF].BalanceQty + selectedRow.AssignQty;
                        }
                    }
                }
            },
        });
    }
    function twistingChildCommandClick2(e) {
        if (e.commandColumn.buttonOption.type == 'twistedColor1') {
            var twistingList = e.rowData.YDBookingChildTwistingColors;
            twistedColorChilds = $tblChildEl.getCurrentViewRecords().filter(x => x.IsTwisting == true);
            twistedColorChilds.map(x => {
                if (x.ColorID == 0 && x.BookingForName == "Yarn Twist") {
                    var objC = twistingList.find(c => c.BookingForName == x.BookingForName && c.PhysicalCount == x.PhysicalCount && c.LotNo == x.LotNo);
                    if (objC) x.ColorID = objC.ColorID;
                }
                var obj = twistingList.find(y => y.ColorID == x.ColorID && y.BookingForName == x.BookingForName);
                if (obj) {
                    x.AssignQty = obj.AssignQty;
                } else {
                    x.AssignQty = 0;
                }
            });
            var colorIDs = twistingList.map(x => x.ColorID).join(",");
            twistedColorChilds = setTwistingCalculatedValues(twistedColorChilds, false);
            $("#btnAddTwisting").hide();
            initTwistingColorTableFromAddTwisting(twistedColorChilds, colorIDs);
            $("#modalTwistingColorSelection").modal('show');
        }
        else if (e.commandColumn.buttonOption.type == 'UsesIn') {
            var tChildData = e.rowData;
            var finder = new commonFinder({
                title: "Select Uses In",
                pageId: pageId,
                height: 320,
                data: masterData.UsesInList,
                fields: "text",
                headerTexts: "Name",
                isMultiselect: true,
                selectedIds: tChildData.UsesInIDs,
                primaryKeyColumn: "id",
                onMultiselect: function (selectedRecords) {
                    tChildData.UsesIns = selectedRecords.map(function (el) { return el.text }).toString();
                    var usesList = [];
                    for (var i = 0; i < selectedRecords.length; i++) {
                        var useObj = {
                            UsesIn: selectedRecords[i].id
                        }
                        usesList.push(useObj);
                    }
                    tChildData.YDBCTwistingUsesIns = usesList;
                    tChildData.UsesInIDs = selectedRecords.map(function (el) { return el.id }).toString();
                    var index = $tblChildTwistingEl.getRowIndexByPrimaryKey(tChildData.YDBCTwistingID);
                    $tblChildTwistingEl.updateRow(index, tChildData);
                }
            });
            finder.showModal();
        }
    }
    function setPhysicalCount(obj) {
        var bookingForName = "";
        if (obj.BookingForName.toLowerCase() == "yarn dyeing") bookingForName = " YD ";
        else if (obj.BookingForName.toLowerCase() == "yarn printing") bookingForName = " YP ";
        obj.PhysicalCount = obj.PhysicalCount.replace(" YD ", "");
        obj.PhysicalCount = obj.PhysicalCount.replace(" YP ", "");
        return obj.PhysicalCount + bookingForName;
    }
    function addTwistingColorsFromAddTwisting() {
        var selectedRecords = $tblTwistingColorEL.getSelectedRecords();
        if (selectedRecords.length == 0) return toastr.error("No item selected.");

        var zeroQtyItems = selectedRecords.filter(x => x.AssignQty == 0);
        if (zeroQtyItems.length > 0) return toastr.error("Selected items assign qty can not be zero.");

        var hasError = false;
        for (var i = 0; i < selectedRecords.length; i++) {
            var selectItem = DeepClone(selectedRecords[i]);
            if (selectItem.AssignQty > selectItem.BalanceQty) {
                toastr.error(`Maximum Remaining qty of color ${selectItem.ColorName} is ${selectItem.BalanceQty}`);
                hasError = true;
                break;
            }

            //var indexF = twistedColorChilds.findIndex(c => c.ColorID == selectItem.ColorID);
            //if (indexF > -1) {
            //    if (selectItem.AssignQty > twistedColorChilds[indexF].BalanceQty) {
            //        toastr.error(`Maximum remaining qty of color ${selectItem.ColorName} is ${twistedColorChilds[indexF].BalanceQty}`);
            //        hasError = true;
            //        break;
            //    }
            //}
        }
        if (hasError) return false;

        var childData = DeepClone(selectedRecords[0]);

        if (childData != null) {
            var twistedColors = [],
                physicalCounts = "",
                colorIDs = [],
                bookingQty = 0,
                majorColors = [];

            selectedRecords.map(x => {
                if (x.ColorName) {
                    twistedColors.push(x.ColorName);
                }
                if (x.PhysicalCount) {
                    var hasYP = x.PhysicalCount.includes("YP");
                    if (hasYP) {
                        majorColors.push(x.ColorName);
                    }
                    physicalCounts += x.PhysicalCount + "+";
                }
                colorIDs.push(x.ColorID);
                bookingQty += parseFloat(x.AssignQty);
            });
            twistedColors = twistedColors.join(' + ');

            majorColors = [...new Set(majorColors)];
            majorColors = majorColors.join(',');

            if (selectedRecords.length == 1) {
                selectedRecords[0].PhysicalCount = selectedRecords[0].PhysicalCount.replace("+", "");
                physicalCounts = selectedRecords[0].PhysicalCount + " + " + selectedRecords[0].PhysicalCount + " Twist";
                if (typeof selectedRecords[0].ColorName === "undefined" || selectedRecords[0].ColorName == null) {
                    twistedColors = "";
                } else {
                    if (selectedRecords[0].ColorName == null || selectedRecords[0].ColorName == "") {
                        twistedColors = "";
                    } else {
                        twistedColors = selectedRecords[0].ColorName + " + " + selectedRecords[0].ColorName;
                    }
                }
            } else {
                physicalCounts = physicalCounts.slice(0, -1);
                physicalCounts = physicalCounts.replace("+", " + ") + " Twist";
            }

            var twistedRecords = $tblChildTwistingEl.getCurrentViewRecords();

            colorIDs = colorIDs.sort(function (a, b) {
                return a - b;
            });
            colorIDs = colorIDs.map(x => x).join(",");
            childData.YDBCTwistingID = maxYDBCTwistingID++;
            childData.ColorID = selectedRecords[0].ColorID;
            childData.ColorIDs = colorIDs;
            childData.ColorName = majorColors;
            childData.ColorCode = selectedRecords[0].ColorCode;
            childData.TwistedColors = twistedColors;
            childData.PhysicalCount = physicalCounts;
            childData.BookingQty = bookingQty;

            var indexF = twistedRecords.findIndex(x => x.ColorIDs == childData.ColorIDs && x.BookingFor == childData.BookingFor);
            if (indexF > -1) {
                toastr.error("Record with color(s) " + twistedColors + " already in list");
                return false;
            }

            selectedRecords.map(x => {
                var indexF = twistedColorChilds.findIndex(c => c.ColorID == x.ColorID);
                if (indexF > -1) {
                    if (typeof twistedColorChilds[indexF].UsedQty === "undefined") twistedColorChilds[indexF].UsedQty = 0;
                    //twistedColorChilds[indexF].TwistingColorQty
                    twistedColorChilds[indexF].AssignQty += x.AssignQty;
                    twistedColorChilds[indexF].UsedQty += x.AssignQty;
                    twistedColorChilds[indexF].BalanceQty = twistedColorChilds[indexF].TwistingColorQty - twistedColorChilds[indexF].AssignQty;
                }
            });


            childData.YDBookingChildTwistingColors = selectedRecords;

            var data = $tblChildTwistingEl.getCurrentViewRecords();
            data.push(childData);


            initTwistedChildTableFromAddTwisting(data, true);
            $("#modalTwistingColorSelection").modal('hide');
        }
    }
    function initTwistingColorTableFromAddTwisting(data, selectedIds) {
        var primaryTwistingColorID = 1;
        data.map(x => {
            x.PrimaryTwistingColorID = primaryTwistingColorID++;
        });
        var columns = [
            { field: 'PrimaryTwistingColorID', isPrimaryKey: true, visible: false },
            { field: 'YDBCTwistingColorID', visible: false },
            { field: 'ItemMasterID', visible: false },
            { field: 'ColorID', visible: false },
            { field: 'FCMRChildID', visible: false },
            { type: 'checkbox', width: 50 },

            { field: 'PhysicalCount', headerText: 'Count / Physical Count', allowEditing: false, width: '100px' },
            { field: 'ColorName', headerText: 'Color', allowEditing: false, width: '100px' },
            { field: 'ColorCode', headerText: 'Color Code', allowEditing: false, width: '100px' },
            {
                field: 'TwistingColorQty', headerText: 'Qty', editType: "numericedit", allowEditing: false, width: '100px',
                textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 2, min: 0, format: "N2" } }
            },
            {
                field: 'AssignQty', headerText: 'Assign Qty', editType: "numericedit", width: '100px',
                textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 2, min: 0, format: "N2" } }
            },
            {
                field: 'UsedQty', headerText: 'Used Qty', editType: "numericedit", allowEditing: false, width: '100px',
                textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 2, min: 0, format: "N2" } }
            },
            {
                field: 'BalanceQty', headerText: 'Balance Qty', editType: "numericedit", allowEditing: false, width: '100px',
                textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 2, min: 0, format: "N2" } }
            },
            { field: 'Segment1ValueDesc', headerText: 'Composition', allowEditing: false, width: '100px' },
            { field: 'Segment2ValueDesc', headerText: 'Yarn Type', allowEditing: false, width: '100px' },
            { field: 'Segment3ValueDesc', headerText: 'Process', allowEditing: false, width: '100px' },
            { field: 'Segment4ValueDesc', headerText: 'Sub Process', allowEditing: false, width: '100px' },
            { field: 'Segment5ValueDesc', headerText: 'Quality Parameter', allowEditing: false, width: '100px' },
            { field: 'Segment6ValueDesc', headerText: 'Count', allowEditing: false, width: '100px' },
            { field: 'Segment7ValueDesc', headerText: 'No of Ply', allowEditing: false, width: '100px' }, //BookingFor
            { field: 'BookingForName', headerText: 'Booking For', allowEditing: false, width: '100px', visible: false }
        ];

        if ($tblTwistingColorEL) $tblTwistingColorEL.destroy();
        ej.base.enableRipple(true);
        $tblTwistingColorEL = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            columns: columns,
            //enableSingleClickEdit: true,
            actionBegin: function (args) {
                if (args.requestType === "beginEdit") {
                    //var allRecords = $tblTwistingColorEL.getCurrentViewRecords();
                    //args.rowData = allRecords[args.rowIndex];
                }
                else if (args.requestType === "add") {
                }
                else if (args.requestType === "save") {

                }
            },
            editSettings: {
                allowEditing: !isYDBBPage,
                allowAdding: !isYDBBPage,
                allowDeleting: !isYDBBPage,
                mode: "Normal",
                showDeleteConfirmDialog: true
            },
            selectionSettings: { type: "Multiple" },
            dataBound: function (args) {
                if (selectedIds != undefined) {
                    var ids = selectedIds.split(',');
                    var selIds = [];
                    var allRecords = $tblTwistingColorEL.getCurrentViewRecords();
                    for (var i = 0; i < allRecords.length; i++) {
                        if (allRecords[i].AssignQty > 0) {
                            var indexF = ids.findIndex(x => x == allRecords[i].ColorID);
                            if (indexF > -1) {
                                selIds.push(i);
                            }
                        }
                    }
                    this.selectRows(selIds);
                }
            }
        });
        $tblTwistingColorEL.refreshColumns;
        $tblTwistingColorEL.appendTo(tblTwistingColorId);
    }

    function backToList() {
        $divDetailsEl.hide();
        resetForm();
        $divTblEl.show();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#YDBookingMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetPrvIssue() {
        masterDataPrv = null;
        isCopyFromPreviousData = false;
        $formEl.find("#divPrvDyeingDetails").hide();
    }

    function getNew(id, pName) {
        resetPrvIssue();

        axios.get(`/api/yd-booking/yarn/${id}/${pName}`)
            .then(function (response) {
                $divDetailsEl.show();
                $divTblEl.hide();
                $formEl.find("#divTwistedDetails,#btnBackToDyeing").hide();
                $formEl.find("#btnNextForTwist").show();
                copiedRecord = null;
                masterData = response.data;
                masterData.YDBookingDate = formatDateToDefault(masterData.YDBookingDate);
                setFormData($formEl, masterData);
                initNewAttachment($formEl.find("#SwatchFile"));

                initChildTable(masterData.YDBookingChilds);

                initChildDyedCalculation([]);
                $formEl.find("#divDyeingDetailsCalculation,#btnBackToTwisted").hide();
                $formEl.find("#btnNextForDyedCalculation").show();

                $formEl.find("#btnSave").show();
                $formEl.find("#btnSaveAndSend").show();
                $formEl.find("#btnRevise").hide();
                $formEl.find("#divRevisionReason").hide();
                $formEl.find("#divUnAckReason").hide();
                //initColorChildTable();

                var vColorChilds = masterData.ColorChilds.find(function (el) {
                    return el.ColorName != '';
                });

                $tblColorChildEl.bootstrapTable("load", vColorChilds);
                //initTwistedChildTable([], false);

                initTwistedChildTableFromAddTwisting([], false);

                twistedChilds = masterData.YDBookingChildTwistings;

                twistedColorChilds = masterData.YDBookingChildTwistings;

                twistedColorDetailsChilds = masterData.YDBookingChildTwistingColors;
            })
            .catch(showResponseError);
    }

    function getDetails(id, pName, isCallForRevision) {
        axios.get(`/api/yd-booking/${id}/${pName}`)
            .then(function (response) {
                resetPrvIssue();
                if (!isCallForRevision) {
                    $divDetailsEl.show();
                    $divTblEl.hide();
                    $formEl.find("#divTwistedDetails,#btnBackToDyeing").show();
                    $formEl.find("#btnNextForTwist").hide();
                    copiedRecord = null;
                    masterData = response.data;

                    masterData.YDBookingDate = formatDateToDefault(masterData.YDBookingDate);
                    if (addAdditionalReq) {
                        $formEl.find("#btnRevise").hide();
                        $formEl.find("#btnSave").show();
                        $formEl.find("#btnSaveAndSend").show();
                        masterData.parentYDBookingNo = masterData.YDBookingNo;
                        masterData.YDBookingMasterID = 0;
                        masterData.YDBookingNo = "<<--New-->>";
                        //masterData.IsModified = false;
                        masterData.IsAdditional = true;
                        masterData.YDBookingDate = formatDateToDefault(new Date());
                        if (masterData.YDBookingChilds.length > 0) {
                            masterData.YDBookingChilds.forEach(function (value) {
                                value.BookingQty = 0;
                                value.NoOfCone = 0;
                                return value;
                            });
                        }

                        if (masterData.YDBookingChildTwistings.length > 0) {
                            masterData.YDBookingChildTwistings.forEach(function (value) {
                                value.BookingQty = 0;
                                value.NoOfCone = 0;
                                return value;
                            });
                        }
                    }
                    setFormData($formEl, masterData);
                    initAttachment($formEl.find("#SwatchFile"), masterData.SwatchFilePath);

                    initChildTable(masterData.YDBookingChilds);
                    initChildDyedCalculation([]);
                    $formEl.find("#divDyeingDetailsCalculation,#btnBackToTwisted").hide();
                    $formEl.find("#btnNextForDyedCalculation").show();

                    //initTwistedChildTable(masterData.YDBookingChildTwistings, true);

                    initTwistedChildTableFromAddTwisting(masterData.YDBookingChildTwistings, true);
                    //initColorChildTable();

                    var vColorChilds = masterData.ColorChilds.find(function (el) {
                        return el.ColorName != '';
                    });
                    $tblColorChildEl.bootstrapTable("load", vColorChilds);

                    twistedChilds = masterData.YDBookingChildTwistings;

                    var maxId = 1;


                    //masterData.YDBookingChilds.filter(x => x.ColorID == 0 && x.BookingForName == "Yarn Twist").map(x => {
                    //    x.ColorID = maxColorIDForOtherBookingFor++;
                    //});

                    twistedColorChilds = masterData.YDBookingChilds.filter(x => x.IsTwisting == true).map(function (el) {
                        return {
                            PrimaryTwistingColorID: el.YDBookingChildID,
                            YDBCTwistingColorID: maxId++,
                            YDBCTwistingID: el.YDBCTwistingID,
                            YDBookingChildID: el.YDBookingChildID,
                            YDBookingMasterID: el.YDBookingMasterID,
                            ItemMasterID: el.ItemMasterID,
                            ColorID: el.ColorID,
                            ColorName: el.ColorName,
                            ColorCode: el.ColorCode,
                            PhysicalCount: el.PhysicalCount,
                            LotNo: el.LotNo,
                            TwistingColorQty: el.BookingQty,
                            AssignQty: 0,
                            PreviousAssignQty: 0,
                            BalanceQty: el.BookingQty,
                            Segment1ValueDesc: el.Segment1ValueDesc,
                            Segment2ValueDesc: el.Segment2ValueDesc,
                            Segment3ValueDesc: el.Segment3ValueDesc,
                            Segment4ValueDesc: el.Segment4ValueDesc,
                            Segment5ValueDesc: el.Segment5ValueDesc,
                            Segment6ValueDesc: el.Segment6ValueDesc,
                            Segment7ValueDesc: el.Segment7ValueDesc
                        }
                    });

                    if ((status == statusConstants.PROPOSED || status == statusConstants.APPROVED || status == statusConstants.ACKNOWLEDGE) && !isAcknowledgePage && !isApprovePage) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        if (addAdditionalReq) {
                            $formEl.find("#btnSave").show();
                            $formEl.find("#btnSaveAndSend").show();
                            $formEl.find("#btnRevise").hide();
                        }
                    }
                    if ((status == statusConstants.REVISE) && !isAcknowledgePage && !isApprovePage) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnRevise").show();
                        $formEl.find("#divRevisionReason").show();
                    }
                    if (status == statusConstants.PENDING || status == statusConstants.PARTIALLY_COMPLETED) {
                        $formEl.find("#btnSave").show();
                        $formEl.find("#btnSaveAndSend").show();
                        $formEl.find("#btnRevise").hide();
                        $formEl.find("#divRevisionReason").show();
                        $formEl.find("#btnYDBNoSave").hide();
                    }
                    $formEl.find("#btnApproveMailResend").hide();
                    if (isApprovePage && status == statusConstants.APPROVED) {
                        $formEl.find("#btnApproveMailResend").show();
                    }
                    if (isApprovePage && status == statusConstants.PROPOSED) {
                        $formEl.find("#btnApprove").show();
                    }
                    else {
                        $formEl.find("#btnApprove").hide();
                    }

                    if (isAcknowledgePage && status == statusConstants.APPROVED) {
                        $formEl.find("#btnAcknowledge").show();
                        $formEl.find("#btnUnAcknowledge").show();
                        $formEl.find("#divUnAckReason").show();
                    }
                    else {
                        $formEl.find("#btnAcknowledge").hide();
                        $formEl.find("#btnUnAcknowledge").hide();
                        $formEl.find("#divUnAckReason").hide();
                    }

                    if (status == statusConstants.UN_ACKNOWLEDGE) {
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                        $formEl.find("#btnRevise").hide();
                        $formEl.find("#divRevisionReason").hide();
                    }
                    if (status == statusConstants.COMPLETED) {
                        $formEl.find("#btnYDBNoSave").hide();
                    }
                    if (isYDBBPage && status == statusConstants.PENDING) {
                        $formEl.find("#btnYDBNoSave").show();
                        $formEl.find("#btnSave").hide();
                        $formEl.find("#btnSaveAndSend").hide();
                    }
                } else {
                    masterDataPrv = response.data;
                    $formEl.find("#divPrvDyeingDetails").show();
                    initChildTablePrv();
                }
            })
            .catch(showResponseError);
    }

    function getReviseDetails(id, groupConceptNo, pName) {
        axios.get(`/api/yd-booking/revise/${id}/${groupConceptNo}/${pName}`)
            .then(function (response) {
                $divDetailsEl.show();
                $divTblEl.hide();
                $formEl.find("#divTwistedDetails,#btnBackToDyeing").show();
                $formEl.find("#btnNextForTwist").hide();
                copiedRecord = null;
                masterData = response.data;

                masterData.YDBookingDate = formatDateToDefault(masterData.YDBookingDate);
                setFormData($formEl, masterData);
                initAttachment($formEl.find("#SwatchFile"), masterData.SwatchFilePath);

                initChildTable(masterData.YDBookingChilds);
                initChildDyedCalculation([]);
                $formEl.find("#divDyeingDetailsCalculation,#btnBackToTwisted").hide();
                $formEl.find("#btnNextForDyedCalculation").show();

                initTwistedChildTable(masterData.YDBookingChildTwistings, false);
                //initColorChildTable();

                var vColorChilds = masterData.ColorChilds.find(function (el) {
                    return el.ColorName != '';
                });
                $tblColorChildEl.bootstrapTable("load", vColorChilds);

                if ((status == statusConstants.PROPOSED || status == statusConstants.APPROVED || status == statusConstants.ACKNOWLEDGE) && !isAcknowledgePage && !isApprovePage) {
                    $formEl.find("#btnSave").hide();
                    $formEl.find("#btnSaveAndSend").hide();
                }
                if ((status == statusConstants.REVISE) && !isAcknowledgePage && !isApprovePage) {
                    $formEl.find("#btnSave").hide();
                    $formEl.find("#btnSaveAndSend").hide();
                    $formEl.find("#btnRevise").show();
                    $formEl.find("#divRevisionReason").show();
                }
                if (status == statusConstants.PENDING || status == statusConstants.PARTIALLY_COMPLETED) {
                    $formEl.find("#btnSave").show();
                    $formEl.find("#btnSaveAndSend").show();
                    $formEl.find("#btnRevise").show();
                    $formEl.find("#divRevisionReason").show();
                }

                $formEl.find("#btnApproveMailResend").hide();
                if (isApprovePage && status == statusConstants.APPROVED) {
                    $formEl.find("#btnApproveMailResend").show();
                }
                if (isApprovePage && status == statusConstants.PROPOSED) {
                    $formEl.find("#btnApprove").show();
                }
                else {
                    $formEl.find("#btnApprove").hide();
                }
                if (isAcknowledgePage && status == statusConstants.APPROVED) {
                    $formEl.find("#btnAcknowledge").show();
                    $formEl.find("#btnUnAcknowledge").show
                    $formEl.find("#divUnAckReason").show();
                }
                else {
                    $formEl.find("#btnAcknowledge").hide();
                    $formEl.find("#btnUnAcknowledge").hide();
                    $formEl.find("#divUnAckReason").hide();
                }
                getDetails(id, pName, true);
            })
            .catch(showResponseError);
    }

    function initNewAttachment($el) {
        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            previewFileType: 'any'
        });
    }

    function initAttachment($el, path) {
        if (!path) {
            initNewAttachment($el);
            return;
        }

        var preveiwData = [rootPath + path];
        var previewConfig = [
            {
                type: 'image',
                caption: "Swatch Attachment",
                key: 1,
                frameClass: 'my-custom-frame-css',
                frameAttr: {
                    style: 'height:120px',
                },
            }];

        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            initialPreview: preveiwData,
            initialPreviewAsData: true,
            initialPreviewFileType: 'image',
            initialPreviewConfig: previewConfig,
            previewSettings: {
                image: { width: "auto", height: "100px", 'max-width': "100%", 'max-height': "100px" },
                pdf: { width: "auto", height: "100px", 'max-width': "100%", 'max-height': "100px" },
            },
            purifyHtml: true,
            required: true,
            maxFileSize: 4096
        });
    }

    function isValidChildForm(cData, tData) {
        var isValidItemInfo = false;
        for (var i = 0; i < cData.length; i++) {
            var obj = cData[i];
            if (obj.BookingFor == "" || obj.BookingFor == null) {
                toastr.error("'Booking For' is required.");
                isValidItemInfo = true;
                break;
            }
            if ((obj.ColorID == "" || obj.ColorID == null) && obj.BookingForName == "Yarn Dyeing") {
                toastr.error("Please select color for items where Booking for: Yarn Dyeing");
                isValidItemInfo = true;
                break;
            }
            if ((obj.PrintColors == "" || obj.PrintColors == null) && obj.BookingForName == "Yarn Printing") {
                toastr.error("You must select print colors where 'Booking For' is 'Yarn Printing'.");
                isValidItemInfo = true;
                break;
            }
            if (obj.PhysicalCount == "" || obj.PhysicalCount == null) {
                toastr.error("Physical Count is required.");
                isValidItemInfo = true;
                break;
            }
            if (masterData.IsBDS != 2) {
                //if (obj.LotNo == "" || obj.LotNo == null) {
                //    toastr.error("Lot No is required.");
                //    isValidItemInfo = true;
                //    break;
                //}
            } else {
                obj.LotNo == "";
                obj.SpinnerID = 0;
            }
            if (obj.UsesInIDs == null) {
                toastr.error("Uses in is required.");
                isValidItemInfo = true;
                break;
            }
        }

        if (!isValidItemInfo) {
            for (var i = 0; i < tData.length; i++) {
                var obj = tData[i];
                if (obj.TPI == "" || obj.TPI == null) {
                    toastr.error("Twist/Inch is required.");
                    isValidItemInfo = true;
                    break;
                }
            }
        }
        return isValidItemInfo;
    }

    function save(sendForApproval = false) {
        var formData = getFormData($formEl);
        var childs = $tblChildEl.getCurrentViewRecords();
        if (childs.length == 0) {
            toastr.error("No item found.");
            return false;
        }
        childs.filter(y => y.BookingForName == "Yarn Twist" || y.BookingFor == 3).map(x => {
            x.ColorID = 0;
        });
        formData.append("YDBookingChilds", JSON.stringify(childs));



        var childTwistings = $tblChildTwistingEl.getCurrentViewRecords();

        childTwistings.map(x => {
            x.YDBookingChildTwistingColors.filter(y => y.BookingForName == "Yarn Twist" || y.BookingFor == 3).map(y => {
                y.ColorID = 0;
            });
            x.ColorIDs = x.YDBookingChildTwistingColors.map(t => t.ColorID).join(",");
        });

        formData.append("YDBookingChildTwistings", JSON.stringify(childTwistings));
        formData.append("SendForApproval", sendForApproval);

        var files = $formEl.find("#SwatchFile")[0].files;
        if (files && files.length > 0) formData.append("SwatchFile", files[0]);

        const config = {
            headers: {
                'content-type': 'multipart/form-data',
                'Authorization': "Bearer " + localStorage.getItem("token")
            }
        }

        if (isValidChildForm($tblChildEl.getCurrentViewRecords(), $tblChildTwistingEl.getCurrentViewRecords()))
            return;

        axios.post("/api/yd-booking/save", formData, config)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function saveRevision() {
        var formData = getFormData($formEl);
        //var formObj = formDataToJson(formData);
        var childs = $tblChildEl.getCurrentViewRecords();
        if (childs.length == 0) {
            toastr.error("No item found.");
            return false;
        }
        childs.filter(y => y.BookingForName == "Yarn Twist" || y.BookingFor == 3).map(x => {
            x.ColorID = 0;
        });
        formData.append("YDBookingChilds", JSON.stringify(childs));

        /*
        var childTwistingsDetails;
        for (var i = 0; i < childTwistings.length; i++) {
            childTwistingsDetails = twistedColorDetailsChilds.filter(x => x.YDBCTwistingID == childTwistings[i].YDBCTwistingID);
            childTwistings[i].YDBookingChildTwistingColors = childTwistingsDetails;
            childTwistingsDetails = "";
        }
        */

        var childTwistings = $tblChildTwistingEl.getCurrentViewRecords();

        childTwistings.map(x => {
            x.YDBookingChildTwistingColors.filter(y => y.BookingForName == "Yarn Twist" || y.BookingFor == 3).map(y => {
                y.ColorID = 0;
            });
            x.ColorIDs = x.YDBookingChildTwistingColors.map(t => t.ColorID).join(",");
        });

        formData.append("YDBookingChildTwistings", JSON.stringify(childTwistings));

        formData.append("SendForApproval", false);
        formData.append("RevisionStatus", "Revision");

        var files = $formEl.find("#SwatchFile")[0].files;
        if (files && files.length > 0) formData.append("SwatchFile", files[0]);

        const config = {
            headers: {
                'content-type': 'multipart/form-data',
                'Authorization': "Bearer " + localStorage.getItem("token")
            }
        }
        axios.post("/api/yd-booking/save", formData, config)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function saveYDBNo() {
        var data = formDataToJson($formEl.serializeArray());
        axios.post("/api/yd-booking/save-ydbno", data)
            .then(function (response) {
                console.log(response.data);
                if (response.data.status) {
                    toastr.success("Saved successfully.");
                    backToList();
                }
                else {
                    toastr.error(response.data.message);
                }
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
        //var formData = getFormData($formEl);

        //const config = {
        //    headers: {
        //        'content-type': 'multipart/form-data',
        //        'Authorization': "Bearer " + localStorage.getItem("token")
        //    }
        //}
        //axios.post("/api/yd-booking/save", formData, config)
        //    .then(function () {
        //        toastr.success("Saved successfully.");
        //        backToList();
        //    })
        //    .catch(showResponseError);
    }
    function setTwistingCalculatedValues(popupItemList, isDefaultAssignQty) {
        var mainList = $tblChildEl.getCurrentViewRecords();
        var twistedList = $tblChildTwistingEl.getCurrentViewRecords();

        popupItemList.map(pp => {
            var mainObj = DeepClone(mainList.find(ml => ml.ColorID == pp.ColorID && ml.BookingForName == pp.BookingForName));
            if (mainObj) {
                pp.TwistingColorQty = parseFloat(mainObj.BookingQty);
                pp.BookingForName = mainObj.BookingForName;
            }
            pp.TwistingColorQty = getNaNValue(pp.TwistingColorQty);
            if (isDefaultAssignQty) pp.AssignQty = parseFloat(0);

            var usedQty = 0;
            twistedList.map(tc => {
                //usedQty += parseFloat(tc.BookingQty);
                tc.YDBookingChildTwistingColors.filter(tcc => tcc.ColorID == pp.ColorID && tcc.BookingForName == pp.BookingForName).map(tcc => {
                    usedQty += parseFloat(tcc.AssignQty);
                });
            });
            pp.UsedQty = parseFloat(usedQty);
            pp.UsedQty = getNaNValue(pp.UsedQty);
            if (!isDefaultAssignQty) pp.UsedQty = pp.UsedQty - pp.AssignQty;

            pp.BalanceQty = parseFloat(pp.TwistingColorQty - pp.UsedQty);
            pp.BalanceQty = getNaNValue(pp.BalanceQty);

            pp.PhysicalCount = setPhysicalCount(pp);
        });
        return popupItemList;
    }

    function getNaNValue(value) {
        if (isNaN(value)) return parseFloat(0);
        return value;
    }
})();
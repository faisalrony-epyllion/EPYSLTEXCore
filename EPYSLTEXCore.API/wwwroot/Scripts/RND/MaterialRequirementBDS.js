(function () {
    var menuId, pageName, menuParam;
    var toolbarId;
    var isBlended = false;
    var $divTblEl, pageId, pageIdWithHash, $pageEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $tblChildEl, tblChildId,
        $formEl, $tblOtherItemEl, tblOtherItemId, $tblCuffItemEl, tblCuffItemId, tblCreateCompositionId, $tblCreateCompositionEl;
    var status = statusConstants.PENDING;
    var masterData, currentChildRowData = true, maxColYarn = 999, maxColCollar = 999, maxColCuff = 999;
    var isFabric = 0, isCollar = 0, isCuff = 0;
    var isBDS = 1;
    var _segments = [];
    var validationConstraints = {
    };
    var fabYarnItem = null;
    var collarYarnItem = null;
    //var cuffYarnItem = null;
    var copyYarnItem = null;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        if (!menuParam)
            menuParam = localStorage.getItem("menuParam");

        if (menuParam == "MRBDS") isBDS = 1;
        else if (menuParam == "MRPB" || menuParam == "MRPBAck") isBDS = 3;

        pageId = pageName + "-" + menuId;
        $pageEl = $(`#${pageId}`);
        pageIdWithHash = "#" + pageId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblColorChildId = "#tblColorChild" + pageId;
        tblOtherItemId = "#tblOtherItem" + pageId;
        tblCuffItemId = "#tblCuffItem" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        tblCreateCompositionId = `#tblCreateComposition-${pageId}`;

        $toolbarEl.find("#btnAckList").fadeOut();

        $formEl.find(".divForWeight").hide();
        $formEl.find(".SizeWithConsumption").prop("disabled", true);
        if (menuParam == "MRPBAck") {
            $toolbarEl.find("#btnList").fadeOut();
            $toolbarEl.find("#btnDraftList").fadeOut();
            $toolbarEl.find("#btnRevisionList").fadeOut();
            $toolbarEl.find("#btnAckList").fadeIn();
            $formEl.find("#btnAcknowledgeAutoPR").fadeIn();
        }

        initMasterTable();
        $formEl.find("#addYarnComposition").on("click", function (e) {
            showAddComposition();
        });
        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            isEnableSizeConsumption(true);
            initMasterTable();
        });
        $toolbarEl.find("#btnList").click();

        $toolbarEl.find("#btnDraftList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PARTIALLY_COMPLETED;
            initMasterTable();
        });

        $toolbarEl.find("#btnCompleteList").on("click", function (e) {
            e.preventDefault();

            toggleActiveToolbarBtn(this, $toolbarEl);
            if (menuParam == "MRPBAck") {
                status = statusConstants.PROPOSED_FOR_ACKNOWLEDGE;
            }
            else {
                status = statusConstants.COMPLETED;
            }
            //status = statusConstants.COMPLETED;
            initMasterTable();
        });

        $toolbarEl.find("#btnAckList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACKNOWLEDGE;
            $formEl.find("#btnAcknowledgeAutoPR").fadeOut();
            initMasterTable();
        });

        //$toolbarEl.find("#btnRevisionAckList").on("click", function (e) {
        //    e.preventDefault();
        //    toggleActiveToolbarBtn(this, $toolbarEl);
        //    status = statusConstants.REVISE_FOR_ACKNOWLEDGE;
        //    $formEl.find("#btnAcknowledgeAutoPR").fadeOut();
        //    initMasterTable();
        //});

        $toolbarEl.find("#btnRevisionList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REVISE;
            initMasterTable();
        });
        $pageEl.find('input[type=radio][name=Blended]').change(function (e) {
            e.preventDefault();
            isBlended = convertToBoolean(this.value);
            initTblCreateComposition();
            return false;
        });
        $pageEl.find("#btnAddComposition").click(saveComposition);
        $toolbarEl.find("#btnRefreshList").on("click", function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(false);
        });

        $formEl.find("#btnSaveComplete").click(function (e) {
            e.preventDefault();
            save(true);
        });

        $formEl.find("#btnRevise").click(function (e) {
            e.preventDefault();
            revise(true);
        });

        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            approve(this);
        });

        $formEl.find("#btnAcknowledgeAutoPR").click(function (e) {
            e.preventDefault();
            acknowledgeforautopr(this);
        });

        $formEl.find("#btnCancel").on("click", backToListWithoutFilter);

        if (menuParam == "MRPBAck") {
            $toolbarEl.find("#btnCompleteList").click();
            $toolbarEl.find("#btnCompleteList span").text("Pending for Acknowledge");
            $toolbarEl.find("#btnCompleteList i").removeClass('fa-list').addClass('fa-hourglass');
        }


        $formEl.find("#btnCollarApplyKG").click(function (e) {
            var size = $(pageIdWithHash).find("#CollarSizeID").val();
            if (size == null || size.length == 0) {
                toastr.error("Select size");
                return false;
            }
            var weight = $(pageIdWithHash).find("#CollarWeightInGm").val();
            if (weight == null || weight == 0) {
                toastr.error("Give consumption(gm)");
                return false;
            }
            var data = masterData.OtherItems.filter(y => y.SubGroupID == 11);
            btnCollarApplyKGClick(data);
            $tblOtherItemEl.refresh();
        });
        $formEl.find("#btnCuffApplyKG").click(function (e) {
            var size = $(pageIdWithHash).find("#CuffSizeID").val();
            if (size == null || size.length == 0) {
                toastr.error("Select size");
                return false;
            }
            var weight = $(pageIdWithHash).find("#CuffWeightInGm").val();
            if (weight == null || weight == 0) {
                toastr.error("Give consumption(gm)");
                return false;
            }
            var data = masterData.OtherItems.filter(y => y.SubGroupID == 12);
            btnCuffApplyKGClick(data);
            $tblCuffItemEl.refresh();
        });

        getSegments();
    });
    function btnCollarApplyKGClick(data) {
        if (menuParam == "MRPB") {

            SetCollarBookingWeightKG(data);
        }

        if (menuParam == "MRPB") {
            GetCalculatedFBookingChildCollor(data);
        }
        //initOtherItemTable(masterData.OtherItems.filter(y => y.SubGroupID == 11));
    }
    function btnCuffApplyKGClick(data) {
        if (menuParam == "MRPB") {
            SetCuffBookingWeightKG(data);
        }
        if (menuParam == "MRPB") {
            GetCalculatedFBookingChildCuff(data);
        }
    }
    function SetCollarBookingWeightKG(data) {
        data.forEach(x => {

            var Sizelist = masterData.AllCollarSizeList.filter(y => y.Construction == x.Construction && y.Composition == x.Composition && y.Color == x.Color && y.DayValidDurationId == x.DayValidDurationId);
            var BookingWeightGM = 0;
            var BookingWeightGMPCS = 0;

            Sizelist.forEach(z => {

                BookingWeightGM += getBookingQtyKG(z.Length, z.Width, z.BookingQty, 11);
                z.BookingQtyKG = getBookingQtyKG(z.Length, z.Width, z.BookingQty, 11);
                z = setBookingQtyKGRelatedFieldsValue(z, 11);
            });

            x.TotalQtyInKG = getDefaultValueWhenInvalidN_Float(BookingWeightGM);
            x = setBookingQtyKGRelatedFieldsValue(x, 11);
            Sizelist.forEach(z => {

                BookingWeightGMPCS += getBookingQtyKGFromPcs(z.Length, z.Width, z.GreyProdQty, 11);
            });

            //var ff = BookingWeightGMPCS;
        });
    }
    function SetCuffBookingWeightKG(data) {
        data.forEach(x => {
            var Sizelist = masterData.AllCuffSizeList.filter(y => y.Construction == x.Construction && y.Composition == x.Composition && y.Color == x.Color && y.DayValidDurationId == x.DayValidDurationId);
            var BookingWeightGM = 0;
            Sizelist.forEach(z => {
                BookingWeightGM += getBookingQtyKG(z.Length, z.Width, z.BookingQty, 12);
            });
            x.TotalQtyInKG = getDefaultValueWhenInvalidN_Float(BookingWeightGM);
            x = setBookingQtyKGRelatedFieldsValue(x, 12);
        });
    }
    function SetCollarBookingWeightKGAfterSave() {

        masterData.FBookingChildCollor.forEach(x => {

            var Sizelist = masterData.AllCollarSizeList.filter(y => y.Construction == x.Construction && y.Composition == x.Composition && y.Color == x.Color);
            var BookingWeightGM = 0;
            Sizelist.forEach(z => {
                BookingWeightGM += getBookingQtyKG(z.Length, z.Width, z.BookingQty, 11);
            });
            x.BookingQtyKG = getDefaultValueWhenInvalidN_Float(BookingWeightGM);
            x = setBookingQtyKGRelatedFieldsValue(x, 11);
        });
    }
    function SetCuffBookingWeightKGAfterSave() {
        masterData.FBookingChildCuff.forEach(x => {
            var Sizelist = masterData.AllCuffSizeList.filter(y => y.Construction == x.Construction && y.Composition == x.Composition && y.Color == x.Color);
            var BookingWeightGM = 0;
            Sizelist.forEach(z => {
                BookingWeightGM += getBookingQtyKG(z.Length, z.Width, z.BookingQty, 12);
            });
            x.BookingQtyKG = getDefaultValueWhenInvalidN_Float(BookingWeightGM);
            x = setBookingQtyKGRelatedFieldsValue(x, 12);
        });
    }
    function GetCalculatedFBookingChildCollor(FBookingChildCollor, isDoCalculateFields) {
        FBookingChildCollor.forEach(x => {

            //x.ReqFinishFabricQty = x.TotalQtyInKG - x.FinishFabricUtilizationQty;
            x = setBookingQtyKGRelatedFieldsValue(x, 11);
            x.Childs.forEach(y => {
                if (typeof x.TotalQtyInKG != 'undefined' && x.TotalQtyInKG != null && typeof y.Distribution != 'undefined' && y.Distribution != null && typeof y.Allowance != 'undefined' && y.Allowance != null) {
                    y = getYarnRelatedProps(y, x, false, false);

                }
            });
        });
        //FBookingChildCollor = setCalculatedValues(FBookingChildCollor);
    }
    function GetCalculatedFBookingChildCuff(FBookingChildCuff, isDoCalculateFields) {
        FBookingChildCuff.forEach(x => {
            //x.ReqFinishFabricQty = x.TotalQtyInKG - x.FinishFabricUtilizationQty;
            x = setBookingQtyKGRelatedFieldsValue(x, 12);
            x.Childs.forEach(y => {
                if (typeof x.TotalQtyInKG != 'undefined' && x.TotalQtyInKG != null && typeof y.Distribution != 'undefined' && y.Distribution != null && typeof y.Allowance != 'undefined' && y.Allowance != null) {

                    y = getYarnRelatedProps(y, x, false, false);

                }

            });
        });

        //FBookingChildCuff = setCalculatedValues(FBookingChildCuff);
    }
    function setBookingQtyKGRelatedFieldsValue(item, subGroupId) {

        item.TotalQtyInKG = getDefaultValueWhenInvalidN_Float(item.TotalQtyInKG);

        return item;
    }
    function getBookingQtyKG(length, width, bookingQtyPcs, subGroupId) {

        var size = "";
        var gm = 0;
        if (subGroupId == 11) {
            gm = parseFloat($(pageIdWithHash).find("#CollarWeightInGm").val());
            size = $(pageIdWithHash).find("#CollarSizeID").val();
        }
        else if (subGroupId == 12) {
            gm = parseFloat($(pageIdWithHash).find("#CuffWeightInGm").val());
            size = $(pageIdWithHash).find("#CuffSizeID").val();
        }

        var selectedLength = 0;
        var selectedWidth = 0;
        var bookingQtyKG = 0;
        var result = 0;
        if (size != null) {
            selectedLength = parseFloat(size.split(' X ')[0]);
            selectedWidth = parseFloat(size.split(' X ')[1]);
            bookingQtyKG = getDefaultValueWhenInvalidN_Float(bookingQtyPcs * ((gm * length * width) / (selectedLength * selectedWidth)));
            result = getDefaultValueWhenInvalidN_Float(bookingQtyKG / 1000);
        }
        //var perWeight = parseFloat(gm) / (parseFloat(selectedLength) * parseFloat(selectedWidth));


        return result;
    }
    function getBookingQtyKGFromPcs(length, width, bookingQtyKG, subGroupId) {
        var size = "";
        var gm = 0;
        if (subGroupId == 11) {
            gm = parseFloat($(pageIdWithHash).find("#CollarWeightInGm").val());
            size = $(pageIdWithHash).find("#CollarSizeID").val();
        }
        else if (subGroupId == 12) {
            gm = parseFloat($(pageIdWithHash).find("#CuffWeightInGm").val());
            size = $(pageIdWithHash).find("#CuffSizeID").val();
        }

        var selectedLength = 0;
        var selectedWidth = 0;
        var finalResultForPCS = 0;
        if (size != null) {
            var selectedLength = parseFloat(size.split(' X ')[0]);
            var selectedWidth = parseFloat(size.split(' X ')[1]);

            var finalResultForPCS = (bookingQtyKG * 1000) / ((gm * length * width) / (selectedLength * selectedWidth));
        }
        return finalResultForPCS;
    }

    function setCalculatedValues(dataList) {
        if (dataList != null && dataList.length > 0 && (status == statusConstants.PENDING || status == statusConstants.REJECT)) {
            dataList.map(x => {
                x.TechnicalTime = setTechnicalTime(x);
                x = setArgDataValues(x);
            });
        }
        return dataList;
    }

    function setTechnicalTime(data) {

        var obj = masterData.TechnicalNameList.find(x => x.additionalValue == data.MachineTypeId
            && x.id == data.TechnicalNameId);
        if (obj != null) return obj.desc;
        return 0;
    }

    function setArgDataValues(argsData, argsRowData = null) {
        if (argsRowData == null) argsRowData = argsData;
        var totalCriteriaDays = 0, subcontactDays = 0;
        var materialDays = 0, preProcessDays = 0, batchPreparationDays = 0, dyeingDays = 0, finishDays = 0, testingDays = 0, qcDays = 0;
        for (var i = 0; i < argsRowData.CriteriaNames.length; i++) {
            totalCriteriaDays += argsRowData.CriteriaNames[i].TotalTime;
            if (argsRowData.CriteriaNames[i].CriteriaName === "Material") materialDays = argsRowData.CriteriaNames[i].TotalTime;
            else if (argsRowData.CriteriaNames[i].CriteriaName === "Preprocess") preProcessDays = argsRowData.CriteriaNames[i].TotalTime;
            else if (argsRowData.CriteriaNames[i].CriteriaName === "Batch Preparation") batchPreparationDays = argsRowData.CriteriaNames[i].TotalTime;
            else if (argsRowData.CriteriaNames[i].CriteriaName === "Finishing") finishDays = argsRowData.CriteriaNames[i].TotalTime;
            else if (argsRowData.CriteriaNames[i].CriteriaName === "Quality Check") qcDays = argsRowData.CriteriaNames[i].TotalTime;
            else if (argsRowData.CriteriaNames[i].CriteriaName === "Testing") testingDays = argsRowData.CriteriaNames[i].TotalTime;
            else if (argsRowData.CriteriaNames[i].CriteriaName === "Dyeing") dyeingDays = argsRowData.CriteriaNames[i].TotalTime;
        }
        if (argsData.IsSubContact) subcontactDays = 14;

        argsData.StructureDays = parseInt(argsRowData.TechnicalTime);
        argsData.MaterialDays = materialDays;
        argsData.KnittingDays = argsData.StructureDays + materialDays;
        argsData.BatchPreparationDays = argsData.StructureDays + materialDays + preProcessDays + batchPreparationDays;
        argsData.DyeingDays = argsData.StructureDays + materialDays + preProcessDays + batchPreparationDays + dyeingDays;
        argsData.FinishingDays = argsData.StructureDays + materialDays + preProcessDays + batchPreparationDays + dyeingDays + finishDays;
        argsData.TestReportDays = argsData.StructureDays + materialDays + preProcessDays + batchPreparationDays + dyeingDays + finishDays + testingDays;
        argsData.TotalDays = argsData.StructureDays + subcontactDays + totalCriteriaDays;

        var dt = new Date();
        dt.setDate(dt.getDate() + argsData.TotalDays);
        argsData.DeliveryDate = dt;

        argsData.MachineTypeId = argsRowData.MachineTypeId;
        argsData.MachineType = argsRowData.MachineType;
        argsData.KTypeId = argsRowData.KTypeId;
        argsData.TechnicalNameId = argsRowData.TechnicalNameId;
        argsData.TechnicalName = argsRowData.TechnicalName;
        argsData.TechnicalTime = argsRowData.TechnicalTime;
        argsData.YarnSourceID = argsRowData.YarnSourceID;
        argsData.BrandID = argsRowData.BrandID;
        argsData.Brand = argsRowData.Brand;

        return argsData;
    }

    function getYarnRelatedProps(obj, parent, isdistributionChenged = false, isDoCalculateFields = false) {

        obj.ReqQty = getDefaultValueWhenInvalidN(obj.ReqQty);
        obj.Allowance = getDefaultValueWhenInvalidN(obj.Allowance);
        obj.Distribution = getDefaultValueWhenInvalidN(obj.Distribution);

        parent.TotalQtyInKG = getDefaultValueWhenInvalidN(parent.TotalQtyInKG);
        //obj.BookingQty = (parent.TotalQtyInKG * (obj.Distribution / 100)) / (1 + (x.YarnAllowance / 100) - (0.5 / 100));
        obj.BookingQty = parent.TotalQtyInKG * (obj.Distribution / 100);
        //if (isDoCalculateFields) {

        var netYRQ = getNetYarnReqQty(obj.Distribution, 0, 0, 0, obj.Allowance, 0, 0, parent.TotalQtyInKG);

        if (parent.TotalQtyInKG > 0 || isdistributionChenged == true) {
            obj.ReqQty = netYRQ;
        }
        else {
            obj.ReqQty = 0;
        }

        //}

        obj.ReqQty = parseFloat(obj.ReqQty).toFixed(2);
        obj.Allowance = parseFloat(obj.Allowance).toFixed(2);

        return obj;
    }
    function getNetYarnReqQty(yarnDistribution, finishFabricUtilizationQty, greyUtilizationQty, dyedYarnUtilizationQty, totalAllowance, yDAllowance, greyYarnUtilizationQty, reqFinishFabricQty) {

        var yarnFFU = parseFloat(yarnDistribution) * (parseFloat(finishFabricUtilizationQty) / 100);
        yarnFFU = yarnFFU + (yarnFFU * parseFloat(totalAllowance)) / 100;

        var yarnGU = parseFloat(yarnDistribution) * (parseFloat(greyUtilizationQty) / 100);
        yarnGU = yarnGU + (yarnGU * (parseFloat(yDAllowance) + parseFloat(0.5))) / 100;

        //var yarnDYU = yarnDistribution * (dyedYarnUtilizationQty / 100);
        //yarnDYU = yarnDYU + (yarnDYU * (yDAllowance)) / 100;

        //var yarnGYU = greyYarnUtilizationQty;
        reqFinishFabricQty = (reqFinishFabricQty / 100) * yarnDistribution;
        var netReqQty = parseFloat(reqFinishFabricQty) + ((parseFloat(reqFinishFabricQty) * parseFloat(totalAllowance)) / 100) - yarnGU;
        //var netReqQty = parseFloat(reqFinishFabricQty) + ((parseFloat(reqFinishFabricQty) * parseFloat(totalAllowance)) / 100) - yarnFFU - yarnGU;

        return netReqQty;
    }
    function acknowledgeforautopr() {
        //var YBookingNo = $formEl.find("#YBookingNo").val();
        //var url = `/api/mr-bds/remove-from-reject/${masterData.FCMRMasterID}`;
        //masterData.OtherItems[0].BuyerName
        axios.post(`/api/mr-bds/acknowledgeAutoPR/${masterData.OtherItems[0].ConceptNo}`)
            .then(function () {
                toastr.success("Acknowledge operation successful.");
                backToList();
            })
            .catch(showResponseError);
    }

    async function getSegments() {
        _segments = await axios.get(getYarnItemsApiUrl([]));
        _segments = _segments.data;
    }

    function showAddComposition() {
        initTblCreateComposition();
        $pageEl.find(`#modal-new-composition-${pageId}`).modal("show");
    }
    var subProgramElem, certificationElem, fiberElem;
    var subProgramObj, certificationObj, fiberObj;
    //var YarnSubProgramNewsFilteredList, CertificationsFilteredList;
    async function initTblCreateComposition() {
        var YarnSubProgramNewsFilteredList = [];//masterData.YarnSubProgramNews;
        var CertificationsFilteredList = [];//masterData.Certifications;

        compositionComponents = [];

        var columns = [
            {
                field: 'Id', isPrimaryKey: true, visible: false
            },
            {
                headerText: '', width: 70, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
            },
            {
                field: 'Percent', headerText: 'Percent(%)', width: 120, editType: "numericedit", params: { decimals: 0, format: "N", min: 1, validateDecimalOnType: true }, allowEditing: isBlended
            },
            //{
            //    field: 'Fiber', headerText: 'Component', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.FabricComponentsNew, field: "Fiber" })
            //}
            {
                field: 'Fiber', headerText: 'Fiber', valueAccessor: ej2GridDisplayFormatterV2, edit: {
                    create: function () {
                        fiberElem = document.createElement('input');
                        return fiberElem;
                    },
                    read: function () {
                        return fiberObj.text;
                    },
                    destroy: function () {
                        fiberObj.destroy();
                    },
                    write: function (e) {
                        fiberObj = new ej.dropdowns.DropDownList({
                            dataSource: masterData.FabricComponentsNew,
                            fields: { value: 'id', text: 'text' },
                            //enabled: false,
                            placeholder: 'Select Component',
                            floatLabelType: 'Never',
                            change: function (f) {

                                if (!f.isInteracted || !f.itemData) return false;
                                e.rowData.Fiber = f.itemData.id;
                                e.rowData.Fiber = f.itemData.text;

                                YarnSubProgramNewsFilteredList = masterData.YarnSubProgramNews.filter(y => y.additionalValue == f.itemData.id);
                                subProgramObj.dataSource = YarnSubProgramNewsFilteredList;
                                subProgramObj.dataBind();

                                certificationObj.dataSource = [];
                                certificationObj.dataBind();

                                $tblChildEl.updateRow(e.row.rowIndex, e.rowData);
                            }
                        });
                        fiberObj.appendTo(fiberElem);

                    }
                }
            },
            //{
            //    field: 'YarnSubProgramNew', headerText: 'Yarn Sub Program New', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.YarnSubProgramNews, field: "YarnSubProgramNew" })
            //},

            {
                field: 'YarnSubProgramNew', headerText: 'Yarn Sub Program New', valueAccessor: ej2GridDisplayFormatterV2, edit: {
                    create: function () {
                        subProgramElem = document.createElement('input');
                        return subProgramElem;
                    },
                    read: function () {
                        return subProgramObj.text;
                    },
                    destroy: function () {
                        subProgramObj.destroy();
                    },
                    write: function (e) {
                        subProgramObj = new ej.dropdowns.DropDownList({
                            dataSource: YarnSubProgramNewsFilteredList,
                            fields: { value: 'id', text: 'text' },
                            //enabled: false,
                            placeholder: 'Select Yarn Sub Program',
                            floatLabelType: 'Never',
                            change: function (f) {
                                if (!f.isInteracted || !f.itemData) return false;
                                e.rowData.YarnSubProgramNew = f.itemData.id;
                                e.rowData.YarnSubProgramNew = f.itemData.text;

                                //CertificationsFilteredList = masterData.Certifications.filter(y => y.additionalValue == f.itemData.id);
                                CertificationsFilteredList = masterData.Certifications.filter(y => y.additionalValue == f.itemData.id && y.additionalValue2 == f.itemData.additionalValue);
                                certificationObj.dataSource = CertificationsFilteredList;
                                certificationObj.dataBind();

                                $tblChildEl.updateRow(e.row.rowIndex, e.rowData);
                            }
                        });
                        subProgramObj.appendTo(subProgramElem);
                    }
                }
            },
            //{
            //    field: 'Certification', headerText: 'Certification', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.Certifications, field: "Certification" })
            //},
            {
                field: 'Certification', headerText: 'Certification', valueAccessor: ej2GridDisplayFormatterV2, edit: {
                    create: function () {
                        certificationElem = document.createElement('input');
                        return certificationElem;
                    },
                    read: function () {
                        return certificationObj.text;
                    },
                    destroy: function () {
                        certificationObj.destroy();
                    },
                    write: function (e) {
                        certificationObj = new ej.dropdowns.DropDownList({
                            dataSource: CertificationsFilteredList,
                            fields: { value: 'id', text: 'text' },
                            //enabled: false,
                            placeholder: 'Select Certification',
                            floatLabelType: 'Never',
                            change: function (f) {

                                if (!f.isInteracted || !f.itemData) return false;
                                e.rowData.Certification = f.itemData.id;
                                e.rowData.Certification = f.itemData.text;

                                $tblChildEl.updateRow(e.row.rowIndex, e.rowData);
                            }
                        });
                        certificationObj.appendTo(certificationElem);
                    }
                }
            }

        ];

        var gridOptions = {
            tableId: tblCreateCompositionId,
            data: compositionComponents,
            columns: columns,
            actionBegin: function (args) {

                if (args.requestType === "add") {
                    if (isBlended) {
                        if (compositionComponents.length === 5) {
                            toastr.info("You can only add 5 components.");
                            args.cancel = true;
                            return;
                        }
                    }
                    else {
                        if (compositionComponents.length === 1) {
                            toastr.info("You can only add 1 component.");
                            args.cancel = true;
                            return;
                        }
                        else args.data.Percent = 100;
                    }

                    args.data.Id = getMaxIdForArray(compositionComponents, "Id");
                }
                else if (args.requestType === "save") {
                    var fiberID = 0;
                    var subProgramID = 0;
                    var certificationsID = 0;
                    if (typeof args.rowData.Fiber != 'undefined') {
                        fiberID = masterData.FabricComponentsNew.find(y => y.text == args.rowData.Fiber).id;
                    }
                    if (typeof args.rowData.YarnSubProgramNew != 'undefined') {
                        subProgramID = masterData.YarnSubProgramNews.find(y => y.text == args.rowData.YarnSubProgramNew).id;
                    }
                    if (typeof args.rowData.Certification != 'undefined') {
                        certificationsID = masterData.Certifications.find(y => y.text == args.rowData.Certification).id;
                    }

                    var cnt = masterData.FabricComponentMappingSetupList.filter(y => y.FiberID == fiberID && y.SubProgramID == subProgramID && y.CertificationsID == certificationsID);
                    if (cnt == 0) {
                        if (fiberID == 0) {
                            toastr.warning("Fiber is required.");
                            args.cancel = true;
                            return;
                        }
                        if (subProgramID == 0) {
                            toastr.warning("Sub Program is required.");
                            args.cancel = true;
                            return;
                        }
                        if (certificationsID == 0) {
                            toastr.warning("certifications is required.");
                            args.cancel = true;
                            return;
                        }
                    }

                    if (args.action === "edit") {
                        if (!args.data.Fiber) {
                            toastr.warning("Fabric component is required.");
                            args.cancel = true;
                            return;
                        }
                        else if (!args.data.Percent || args.data.Percent <= 0 || args.data.Percent > 100) {
                            toastr.warning("Composition percent must be greater than 0 and less than or equal 100.");
                            args.cancel = true;
                            return;
                        }
                    }

                }
            },

            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false,
            toolbar: ['Add'],
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true }
        };

        if ($tblCreateCompositionEl) $tblCreateCompositionEl.destroy();
        $tblCreateCompositionEl = new initEJ2Grid(gridOptions);
    }

    function initMasterTable() {
        var commands = [];
        if (status == statusConstants.PENDING) {
            commands = [
                { type: 'New', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } }
            ]
        } else {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-edit' } }
            ]
        }

        var columns = [
            {
                headerText: 'Actions', commands: commands, width: 10
            },
            //{
            //    field: 'Status', headerText: 'Status', width: 10, visible: status == statusConstants.PENDING
            //},
            {
                field: 'BookingNo', headerText: 'Booking No', width: 20
            },
            {
                field: 'YBookingNo', headerText: 'YBooking No', width: 20, visible: false
            },
            {
                field: 'BookingDate', headerText: 'Booking Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 20
            },
            {
                field: 'AcknowledgeDate', headerText: 'Acknowledge Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 20, visible: status == statusConstants.PENDING
            },
            {
                field: 'BuyerName', headerText: 'Buyer', width: 20
            },
            {
                field: 'BuyerDepartment', headerText: 'Buyer Department', width: 20
            },
            {
                field: 'CompanyName', headerText: 'Company', width: 20
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            //allowGrouping: true,
            apiEndPoint: `/api/mr-bds/list?status=${status}&isBDS=${isBDS}`,
            columns: columns,
            allowSorting: true,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'New') {
            if (args.rowData.Status == 'New') {
                getNew(args.rowData.FBAckID);
                $formEl.find("#btnRevise").fadeOut();
                $formEl.find("#btnSave,#btnSaveComplete").fadeIn();
            }
            else {
                getRevision(args.rowData.FBAckID, args.rowData.ConceptNo);
            }
            $formEl.find("#btnAcknowledge,#btnAcknowledgeAutoPR").fadeOut();
        } else if (args.commandColumn.type == 'Edit' && status == statusConstants.REVISE) {
            if (args.rowData) {
                getRevision(args.rowData.FBAckID, args.rowData.ConceptNo);
                $formEl.find("#btnSave,#btnSaveComplete,#btnAcknowledge,#btnAcknowledgeAutoPR").fadeOut();
                $formEl.find("#btnRevise").fadeIn();
            }
        } else if (args.commandColumn.type == 'Edit' && status == statusConstants.COMPLETED) {
            if (args.rowData) {
                getRevisionForCompleteList(args.rowData.ConceptNo);
                $formEl.find("#btnSave,#btnSaveComplete,#btnAcknowledge,#btnRevise,#btnAcknowledgeAutoPR").fadeOut();
                if (menuParam == "MRPBAck") {
                    $formEl.find("#btnAcknowledgeAutoPR").fadeIn();
                }
                else {
                    $formEl.find("#btnRevise").fadeIn();
                }

            }
        }
        else if (args.commandColumn.type == 'Edit' && status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE) {
            if (args.rowData) {
                getRevisionForCompleteList(args.rowData.ConceptNo);
                //getAcknowledgeForList(args.rowData.YBookingNo);
                $formEl.find("#btnSave,#btnSaveComplete,#btnAcknowledge").fadeOut();
                $formEl.find("#btnRevise").fadeOut();
                $formEl.find("#btnAcknowledgeAutoPR").fadeIn();
            }
        }
        else if (args.commandColumn.type == 'Edit') {
            if (args.rowData) {
                getDetails(args.rowData.ConceptNo);
                $formEl.find("#btnSave,#btnSaveComplete,#btnAcknowledge,#btnAcknowledgeAutoPR").fadeOut();
                $formEl.find("#btnRevise").fadeOut();
                if (status == statusConstants.PARTIALLY_COMPLETED) $formEl.find("#btnSave,#btnSaveComplete").fadeIn();
            }
        }
    }

    async function initYarnChildTable(data) {
        if ($tblChildEl) $tblChildEl.destroy();

        var childColumns = await getYarnItemColumnsWithSearchDDLAsync(ch_getCountRelatedList(data, 1), currentChildRowData);
        childColumns.unshift({ field: 'FCMRChildID', isPrimaryKey: true, visible: false });

        var additionalColumns = [
            {
                field: 'ShadeCode',
                headerText: 'Shade Code',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: data[0].YarnShadeBooks,
                displayField: "ShadeCode",
                edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'DayValidDurationId', headerText: 'Yarn Sourcing Mode', width: 120,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.DayValidDurations,
                displayField: "text",
                edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'YDProductionMasterID', headerText: 'YDProductionMasterID', visible: false
            },
            { field: 'YDItem', headerText: 'YD Item?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            { field: 'YD', headerText: 'Go for YD?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            {
                field: 'Distribution', headerText: 'Yarn Distribution (%)', editType: "numericedit",
                edit: { params: { showSpinButton: false, decimals: 2, min: 0 } }, width: 100
                /*edit: { params: { showSpinButton: false, decimals: 0, format: "N2", min: 1, validateDecimalOnType: true } }, width: 100*/
            },
            {
                field: 'BookingQty', headerText: 'Net Consumption', allowEditing: false,
                edit: { params: { showSpinButton: false, decimals: 2, format: "N2" } }, width: 100
            },
            {
                field: 'Allowance', headerText: 'Allowance (%)', editType: "numericedit",
                edit: { params: { showSpinButton: false, decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } }, width: 100
            },
            {
                field: 'ReqQty', headerText: 'Req Qty(KG )', editType: "numericedit", allowEditing: false,
                edit: { params: { showSpinButton: false, decimals: 2, format: "N2" } }, width: 100
            },
            { field: 'ReqCone', headerText: 'Req Cone(PCS)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } }, width: 100 },
            { field: 'IsPR', headerText: 'Go for PR?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            { field: 'PhysicalCount', headerText: 'Physical Count', width: 100, allowEditing: false },
            { field: 'YarnLotNo', headerText: 'Lot No', width: 60, allowEditing: false },
            { field: 'SpinnerName', headerText: 'Spinner', width: 100, allowEditing: false },
            { field: 'SampleStockQty', headerText: 'Sample Stock Qty', edit: { params: { showSpinButton: false, decimals: 2, format: "N2" } }, width: 100, allowEditing: false, visible: menuParam == "MRBDS" },
            { field: 'AdvanceStockQty', headerText: 'Advance Stock Qty', edit: { params: { showSpinButton: false, decimals: 2, format: "N2" } }, width: 100, allowEditing: false, visible: menuParam == "MRBDS" },
            { field: 'YarnStockSetId', headerText: 'YarnStockSetId', width: 10, allowEditing: false, visible: false }
        ];
        childColumns.push.apply(childColumns, additionalColumns);
        childColumns = setMandatoryFieldsCSS(childColumns, "Segment1ValueId, Segment8ValueId, Distribution, Allowance, ReqQty, ReqCone");

        data.map(x => {
            x.GetYarnFromStock = 0;
        });

        ej.base.enableRipple(true);
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            showColumnChooser: true,
            showDefaultToolbar: false,
            toolbar: ['ColumnChooser'],
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    //var maxId = Math..apply(Math, data.map(function (el) { return el["Distribution"]; }));
                    var totalDis = 0, remainDis = 0;
                    this.dataSource.forEach(l => {
                        totalDis += l.Distribution;
                    })
                    if (totalDis < 100) remainDis = 100 - totalDis;
                    else {
                        toastr.error("Distribution can not more then 100!!");
                        args.cancel = true;
                        return;
                    }
                    var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQty) * parseFloat(remainDis) / 100);
                    var reqQty = netConsumption;
                    args.data.Distribution = remainDis;
                    args.data.BookingQty = netConsumption.toFixed(4);
                    args.data.Allowance = 0.00;
                    args.data.ReqQty = reqQty.toFixed(2);

                    args.data.FCMRChildID = maxColYarn++;
                    args.data.FCMRMasterID = this.parentDetails.parentKeyFieldValue;
                }
                else if (args.requestType === "save") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.FCMRChildID);
                    var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQty) * parseFloat(args.data.Distribution) / 100);
                    var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(args.data.Allowance)) / 100);
                    args.data.BookingQty = netConsumption.toFixed(4);
                    args.data.ReqQty = reqQty.toFixed(2);
                }
            },
            columns: [
                //Master Grid Record
                //if(menuParam == "MRPBAck") {
                //    this.dataSource = this.parentDetails.parentRowData.ChildItems;
                //}
                { field: 'GetYarnFromStock', headerText: 'From Stock', allowEditing: false, visible: menuParam == "MRBDS", textAlign: 'center', width: 30, valueAccessor: displayStockIcon },
                { field: 'FCMRMasterID', isPrimaryKey: true, visible: false },
                { field: 'SubGroupID', visible: false },
                { field: 'Construction', headerText: 'Construction', width: 100, allowEditing: false, visible: false },
                { field: 'Composition', headerText: 'Composition', width: 100, allowEditing: false },
                { field: 'Color', headerText: 'Color', width: 100, allowEditing: false },
                { field: 'GSM', headerText: 'GSM', width: 40, allowEditing: false },
                { field: 'DyeingType', headerText: 'Dyeing Type', width: 70, allowEditing: false },
                { field: 'DayValidDurationName', headerText: 'Yarn Sourcing Mode', width: 120, allowEditing: false, visible: isBDS == 3 },
                { field: 'KnittingType', headerText: 'Knitting Type', width: 100, allowEditing: false, visible: false },

                { field: 'YarnProgram', headerText: 'Yarn Program', width: 80, allowEditing: false },
                { field: 'YarnSubProgram', headerText: 'Yarn Sub Program', width: 100, allowEditing: false },

                { field: 'MachineType', headerText: 'Machine Type', width: 100, allowEditing: false, visible: false },
                { field: 'TechnicalName', headerText: 'Technical Name', width: 100, allowEditing: false },
                { field: 'IsSubContact', headerText: 'Sub-Contact?', textAlign: 'Center', width: 70, allowEditing: false, editType: "booleanedit", displayAsCheckBox: true, visible: false },
                { field: 'DeliveryDate', headerText: 'Delivery Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false, width: 100, visible: false },
                { field: 'FabricWidth', headerText: 'Fabric Width', width: 100, allowEditing: false, visible: false },
                { field: 'YarnType', headerText: 'Yarn Type', width: 70, allowEditing: false },
                { field: 'ReferenceNo', headerText: 'Reference No', width: 100, allowEditing: false, visible: false },
                { field: 'ColorReferenceNo', headerText: 'ColorReference No', width: 100, allowEditing: false, visible: false },
                { field: 'LengthYds', headerText: 'Length (Yds)', width: 100, allowEditing: false, visible: false },
                { field: 'LengthInch', headerText: 'Length (Inch)', width: 100, allowEditing: false, visible: false },
                { field: 'Instruction', headerText: 'Instruction', width: 100, allowEditing: false, visible: false },
                { field: 'LabDipNo', headerText: 'Lab Dip No', width: 100, allowEditing: false, visible: false },
                { field: 'ForBDSStyleNo', headerText: 'Style No', width: 100, allowEditing: false, visible: false },
                { field: 'Qty', headerText: 'Booking Qty', width: 70, allowEditing: false, visible: false },
                { field: 'TotalQty', headerText: 'Req. Knitting Qty', width: 70, allowEditing: false }
                //{ field: 'ConsumptionQty', headerText: 'Consumption Qty', width: 100, allowEditing: false }
            ],
            childGrid: {
                //queryString: menuParam == "MRPBAck" ? 'YBChildID' : 'FCMRMasterID',
                queryString: 'FCMRMasterID',
                allowResizing: true,
                autofitColumns: false,
                toolbar: ['Add'],
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns,
                actionBegin: function (args) {
                    if (args.requestType === 'beginEdit') {
                        if (args.rowData.YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                    }
                    else if (args.requestType === "add") {

                        if (status === statusConstants.COMPLETED || status === statusConstants.REVISE) {
                            //var isEqualData = isDeepEqual(args.data, args.rowData);
                            this.parentDetails.parentRowData.IsNeedRevisionTemp = true;
                        }

                        var totalDis = 0, remainDis = 0;
                        this.dataSource.forEach(l => {
                            totalDis += l.Distribution;
                        })
                        if (totalDis < 100) remainDis = 100 - totalDis;
                        else {
                            toastr.error("Distribution can not more then 100!!");
                            args.cancel = true;
                            return;
                        }
                        var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQty) * parseFloat(remainDis) / 100);
                        var reqQty = netConsumption;
                        args.data.Distribution = remainDis;
                        args.data.BookingQty = netConsumption.toFixed(4);
                        args.data.Allowance = 0.00;
                        args.data.ReqQty = reqQty.toFixed(2);

                        args.data.FCMRChildID = maxColYarn++;
                        args.data.FCMRMasterID = this.parentDetails.parentKeyFieldValue;
                        args.data.ReqCone = 1;

                        var subGroupId = this.parentDetails.parentRowData.subGroupId;
                        var parentObj = this.parentDetails.parentRowData;
                        args.data = getDayValidDurationInfos(args.data, parentObj, subGroupId);
                    }
                    else if (args.requestType === "save") {
                        if (status === statusConstants.COMPLETED || status === statusConstants.REVISE) {
                            var isEqualData = isDeepEqual(args.data, args.rowData);
                            this.parentDetails.parentRowData.IsNeedRevisionTemp = !isEqualData;
                        }

                        args.data.IsInvalidItem = typeof args.data.IsInvalidItem === "undefined" ? 1 : args.data.IsInvalidItem;
                        args.data.StockItemNote = getDefaultValueWhenInvalidS(args.data.StockItemNote);
                        if (args.data.YarnStockSetId > 0 && args.data.IsInvalidItem && args.data.IsPR && isBDS == 1) {
                            toastr.error(`'Go For PR' is only for stock valid item`);
                            args.data.IsPR = false;
                            $tblChildEl.updateRow(args.rowIndex, args.data);
                            return false;
                        }

                        //Check YDItem when YD field check
                        if (args.data.YD) {
                            args.data.YDItem = true;
                        }

                        var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQty) * parseFloat(args.data.Distribution) / 100);
                        var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(args.data.Allowance)) / 100);
                        args.data.BookingQty = netConsumption.toFixed(4);
                        args.data.ReqQty = reqQty.toFixed(2);
                        args.data = getFreeConceptMRChild(args.data);

                        //args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                        args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                        args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                        args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                        args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                        args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                        args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                        args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;

                        if (menuParam == "MRBDS") {
                            args.data = getYarnStockRelatedValues(args.data, args.rowData, args.previousData);
                        }


                        var subGroupId = this.parentDetails.parentRowData.subGroupId;
                        var parentObj = this.parentDetails.parentRowData;
                        args.data = getDayValidDurationInfos(args.data, parentObj, subGroupId);

                        $tblChildEl.updateRow(args.rowIndex, args.data);
                    }
                    else if (args.requestType === "delete") {

                        if (status === statusConstants.COMPLETED || status === statusConstants.REVISE) {
                            var isEqualData = isDeepEqual(args.data, args.rowData);
                            this.parentDetails.parentRowData.IsNeedRevisionTemp = !isEqualData;
                        }

                        if (args.data[0].YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }


                        //var index = $tblChildEl.getRowIndexByPrimaryKey(args.data[0].FCMRChildID);
                        //if (index > -1) {
                        //    args.data.EntityState = 8;
                        //    masterData.Childs[index] = args.data;
                        //}
                    }
                },
                load: loadFirstLChildYarnGrid
            },
            enableContextMenu: true,
            contextMenuItems: [
                { text: 'Copy Yarn Information', target: '.e-content', id: 'copy' },
                { text: 'Paste Yarn Information', target: '.e-content', id: 'paste' }
            ],
            contextMenuClick: function (args) {
                if (args.item.id === 'copy') {
                    fabYarnItem = objectCopy(args.rowInfo.rowData.Childs);
                    if (fabYarnItem.length == 0) {
                        toastr.error("No Yarn information found to copy!!");
                        return;
                    }
                }
                else if (args.item.id === 'paste') {
                    var rowIndex = args.rowInfo.rowIndex;
                    if (fabYarnItem == null || fabYarnItem.length == 0) {
                        toastr.error("Please copy first!!");
                        return;
                    } else {
                        for (var i = 0; i < fabYarnItem.length; i++) {
                            //var parentRowData = $tblChildEl.getRowByIndex($tblChildEl.getRowIndexByPrimaryKey(args.rowInfo.rowData.FCMRMasterID));
                            var copiedItem = objectCopy(fabYarnItem[i]);
                            copiedItem.FCMRChildID = maxColYarn++;
                            copiedItem.FCMRMasterID = args.rowInfo.rowData.FCMRMasterID;
                            var netConsumption = (parseFloat(args.rowInfo.rowData.TotalQty) * parseFloat(copiedItem.Distribution) / 100);
                            var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(copiedItem.Allowance)) / 100);
                            copiedItem.BookingQty = netConsumption.toFixed(4);
                            copiedItem.ReqQty = reqQty.toFixed(2);
                            //$tblChildEl.addRecord(copiedItem, 'Child', args.rowInfo.rowIndex);
                            args.rowInfo.rowData.DayValidDurationName = getDefaultValueWhenInvalidS(args.rowInfo.rowData.DayValidDurationName);
                            if (isDayValidDurationLocal(args.rowInfo.rowData.DayValidDurationName)) {
                                copiedItem.DayValidDurationId = args.rowInfo.rowData.DayValidDurationId;
                            }
                            args.rowInfo.rowData.Childs.push(copiedItem);
                        }
                        $tblChildEl.refresh();
                    }
                }
            },
            recordClick: function (args) {
                if (args.column && args.column.field == "GetYarnFromStock") {
                    var otherQuery = " AND (SampleStockQty > 0 OR AdvanceStockQty > 0) ";
                    otherQuery = replaceInvalidChar(otherQuery);
                    var finder = new commonFinder({
                        title: "Yarn Stock",
                        pageId: pageId,
                        height: 320,
                        modalSize: "modal-lg",
                        apiEndPoint: `/api/yarn-stock-adjustment/get-all-stocks-with-custom-query/${otherQuery}`,
                        headerTexts: "Yarn Detail,Count,Physical Count,Lot No,Shade Code,Supplier,Spinner,Sample Stock Qty,Advance Stock Qty,Block Sample Stock Qty,Block Advance Stock Qty,Issued Qty,Item Type,Note",
                        fields: "YarnCategory,Count,PhysicalCount,YarnLotNo,ShadeCode,SupplierName,SpinnerName,SampleStockQty,AdvanceStockQty,BlockSampleStockQty,BlockAdvanceStockQty,TotalIssueQty,InvalidItem_St,Note",
                        primaryKeyColumn: "YarnStockSetId",
                        autofitColumns: true,
                        onSelect: function (res) {
                            finder.hideModal();
                            loadChilds(1, args.rowData.FCMRMasterID, res.rowData, maxColYarn++);
                        }
                    });
                    finder.showModal();
                }
            },
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }

    function displayStockIcon(field, data, column) {
        column.disableHtmlEncode = false;
        return `<button type="button" class="btn btn-sm" style="background-color: #ffffff; color: black;" title='Get item from yarn stock'><span class="fa fa-dropbox"></span></button>`;
    }
    function getYarnStockRelatedValues(obj, rowData, previousData) {
        obj.YarnStockSetId = getDefaultValueWhenInvalidN(rowData.YarnStockSetId);
        obj.IsInvalidItem = typeof rowData.IsInvalidItem === "undefined" ? false : rowData.IsInvalidItem;

        if (obj.IsInvalidItem) {
            obj.Segment1ValueId = previousData.Segment1ValueId;
            obj.Segment2ValueId = previousData.Segment2ValueId;
            obj.Segment3ValueId = previousData.Segment3ValueId;
            obj.Segment4ValueId = previousData.Segment4ValueId;
            obj.Segment5ValueId = previousData.Segment5ValueId;
            obj.Segment6ValueId = previousData.Segment6ValueId;
            obj.Segment1ValueDesc = previousData.Segment1ValueDesc;
            obj.Segment2ValueDesc = previousData.Segment2ValueDesc;
            obj.Segment3ValueDesc = previousData.Segment3ValueDesc;
            obj.Segment4ValueDesc = previousData.Segment4ValueDesc;
            obj.Segment5ValueDesc = previousData.Segment5ValueDesc;
            obj.Segment6ValueDesc = previousData.Segment6ValueDesc;
            obj.ShadeCode = previousData.ShadeCode;
        }

        if (obj.YarnStockSetId > 0) {
            if (obj.ShadeCode == null) obj.ShadeCode = "";

            if (obj.Segment1ValueId != previousData.Segment1ValueId
                || obj.Segment2ValueId != previousData.Segment2ValueId
                || obj.Segment3ValueId != previousData.Segment3ValueId
                || obj.Segment4ValueId != previousData.Segment4ValueId
                || obj.Segment5ValueId != previousData.Segment5ValueId
                || obj.Segment6ValueId != previousData.Segment6ValueId
                || obj.Segment1ValueDesc != previousData.Segment1ValueDesc
                || obj.Segment2ValueDesc != previousData.Segment2ValueDesc
                || obj.Segment3ValueDesc != previousData.Segment3ValueDesc
                || obj.Segment4ValueDesc != previousData.Segment4ValueDesc
                || obj.Segment5ValueDesc != previousData.Segment5ValueDesc
                || obj.Segment6ValueDesc != previousData.Segment6ValueDesc
                || obj.ShadeCode != previousData.ShadeCode) {
                obj.YarnStockSetId = 0;
            }
        }

        if (obj.YarnStockSetId == 0) {
            obj.PhysicalCount = "";
            obj.YarnLotNo = "";
            obj.SpinnerName = "";
            obj.SampleStockQty = 0;
            obj.AdvanceStockQty = 0;
        } else {
            obj.PhysicalCount = rowData.PhysicalCount;
            obj.YarnLotNo = rowData.YarnLotNo;
            obj.SpinnerName = rowData.SpinnerName;
            obj.SampleStockQty = rowData.SampleStockQty;
            obj.AdvanceStockQty = rowData.AdvanceStockQty;
        }
        return obj;
    }
    function loadChilds(subGroupId, nFCMRMasterID, selectedDate, nextFCMRChildID) {
        var masterList = [];
        if (subGroupId == 1) masterList = $tblChildEl.getCurrentViewRecords();
        else if (subGroupId == 11) masterList = $tblOtherItemEl.getCurrentViewRecords();
        else if (subGroupId == 12) masterList = $tblCuffItemEl.getCurrentViewRecords();

        var indexF = masterList.findIndex(x => x.FCMRMasterID == nFCMRMasterID);
        if (indexF > -1) {
            if (typeof masterList[indexF].Childs === "undefined") masterList[indexF].Childs = [];
            if (status === statusConstants.COMPLETED || status === statusConstants.REVISE) {
                //var isEqualData = isDeepEqual(args.data, args.rowData);
                masterList[indexF].IsNeedRevisionTemp = true;
            }

            masterList[indexF].Childs.push({
                FCMRChildID: nextFCMRChildID,
                FCMRMasterID: nFCMRMasterID,
                ItemMasterId: selectedDate.ItemMasterId,
                YarnCategory: selectedDate.YarnCategory,
                YD: false,
                ReqQty: 0,
                SetupChildID: 0,
                UnitID: 28,
                Remarks: "",
                ReqCone: 0,
                IsPR: false,
                ShadeCode: selectedDate.ShadeCode,
                Distribution: 0,
                BookingQty: 0,
                Allowance: 0,
                YDItem: false,
                DayValidDurationId: getDefaultValueWhenInvalidN(selectedDate.DayValidDurationId),

                YarnStockSetId: selectedDate.YarnStockSetId,
                IsInvalidItem: selectedDate.IsInvalidItem,
                StockItemNote: selectedDate.Note,

                Segment1ValueId: selectedDate.Segment1ValueId,
                Segment1ValueDesc: selectedDate.Segment1ValueDesc,
                Segment2ValueId: selectedDate.Segment2ValueId,
                Segment2ValueDesc: selectedDate.Segment2ValueDesc,
                Segment3ValueId: selectedDate.Segment3ValueId,
                Segment3ValueDesc: selectedDate.Segment3ValueDesc,
                Segment4ValueId: selectedDate.Segment4ValueId,
                Segment4ValueDesc: selectedDate.Segment4ValueDesc,
                Segment5ValueId: selectedDate.Segment5ValueId,
                Segment5ValueDesc: selectedDate.Segment5ValueDesc,
                Segment6ValueId: selectedDate.Segment6ValueId,
                Segment6ValueDesc: selectedDate.Segment6ValueDesc,

                Composition: selectedDate.Composition,
                YarnType: selectedDate.YarnType,
                ManufacturingProcess: selectedDate.ManufacturingProcess,
                SubProcess: selectedDate.SubProcess,
                QualityParameter: selectedDate.QualityParameter,
                Count: selectedDate.Count,

                PhysicalCount: selectedDate.PhysicalCount,
                YarnLotNo: selectedDate.YarnLotNo,
                SpinnerName: selectedDate.SpinnerName,
                SampleStockQty: selectedDate.SampleStockQty,
                AdvanceStockQty: selectedDate.AdvanceStockQty
            });

            if (subGroupId == 1) {
                initYarnChildTable(masterList);
            } else if (subGroupId == 11) {
                initOtherItemTable(masterList);
            } else if (subGroupId == 12) {
                initCuffItemTable(masterList);
            }
        }
    }
    function isEnableSizeConsumption(isEnable) {

        if (menuParam == "MRPB") {
            $formEl.find(".divForWeight").show();
            $formEl.find(".SizeWithConsumption").prop("disabled", !isEnable);
        }
        else {
            $formEl.find(".divForWeight").hide();
            $formEl.find(".SizeWithConsumption").prop("disabled", true);
        }
    }
    function getSegmentValueDesc(segNo, argsObj) {
        var propValue = argsObj["Segment" + segNo + "ValueId"];
        if (typeof propValue === "undefined" || propValue == null) return "";
        var segValueList = _segments["Segment" + segNo + "ValueList"];
        if (typeof segValueList !== "undefined" && segValueList != null && parseInt(propValue) > 0) {
            var seg = segValueList.find(x => x.id == propValue);
            if (typeof seg !== "undefined" && seg != null) return seg.text;
        }
        return "";
    }

    function getFreeConceptMRChild(argsObj) {

        var oFreeConceptMRChild = {
            FCMRChildID: argsObj.FCMRChildID,
            FCMRMasterID: argsObj.FCMRMasterID,
            YarnCategory: argsObj.YarnCategory,
            YDItem: argsObj.YDItem,
            YDItem: argsObj.YDItem,
            YD: argsObj.YD,
            ReqQty: parseFloat(argsObj.ReqQty).toFixed(2),
            ReqCone: argsObj.ReqCone,
            Remarks: argsObj.Remarks,
            //IsPR: true,
            IsPR: argsObj.IsPR,
            ShadeCode: argsObj.ShadeCode,
            DayValidDurationId: getDefaultValueWhenInvalidN(argsObj.DayValidDurationId),
            Distribution: argsObj.Distribution,
            BookingQty: argsObj.BookingQty,
            Allowance: argsObj.Allowance,
            SampleStockQty: argsObj.SampleStockQty,
            AdvanceStockQty: argsObj.AdvanceStockQty,

            Segment1ValueId: argsObj.Segment1ValueId,
            Segment2ValueId: argsObj.Segment2ValueId,
            Segment3ValueId: argsObj.Segment3ValueId,
            Segment4ValueId: argsObj.Segment4ValueId,
            Segment5ValueId: argsObj.Segment5ValueId,
            Segment6ValueId: argsObj.Segment6ValueId,
            Segment7ValueId: argsObj.Segment7ValueId
        };
        if (menuParam == "MRBDS") {
            oFreeConceptMRChild.YarnStockSetId = argsObj.YarnStockSetId;
        }
        if (_segments != null && typeof _segments !== "undefined") {
            for (var i = 1; i <= 7; i++) {
                oFreeConceptMRChild["Segment" + i + "ValueDesc"] = getSegmentValueDesc(i, argsObj);
            }
        }
        return oFreeConceptMRChild;
    }

    function loadFirstLChildYarnGrid() {
        this.dataSource = this.parentDetails.parentRowData.Childs;
    }

    async function initOtherItemTable(data) {

        if (menuParam == "MRPB") {
            btnCollarApplyKGClick(data);
        }
        if ($tblOtherItemEl) $tblOtherItemEl.destroy();

        //Child Grid Record
        var childColumns = await getYarnItemColumnsWithSearchDDLAsync(ch_getCountRelatedList(data, 1), currentChildRowData);
        childColumns.unshift({ field: 'FCMRChildID', isPrimaryKey: true, visible: false });
        //if (menuParam == "MRPBAck") {
        //    childColumns.unshift({ field: 'YBChildID', isPrimaryKey: true, visible: false });
        //}
        //else {
        //    childColumns.unshift({ field: 'FCMRChildID', isPrimaryKey: true, visible: false });
        //}
        var additionalColumns = [
            {
                field: 'ShadeCode',
                headerText: 'Shade Code',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: data[0].YarnShadeBooks,
                displayField: "ShadeCode",
                edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'DayValidDurationId', headerText: 'Yarn Sourcing Mode', width: 120,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.DayValidDurations,
                displayField: "text",
                edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'YDProductionMasterID', headerText: 'YDProductionMasterID', width: 20, visible: false
            },
            { field: 'YDItem', headerText: 'YD Item?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            { field: 'YD', headerText: 'Go for YD?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            {
                field: 'Distribution', headerText: 'Yarn Distribution (%)', editType: "numericedit",
                /*edit: { params: { showSpinButton: false, decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } }, width: 100*/
                edit: { params: { showSpinButton: false, decimals: 2, min: 0 } }, width: 100
            },
            {
                field: 'BookingQty', headerText: 'Net Consumption', allowEditing: false,
                edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } }, width: 100
            },
            {
                field: 'Allowance', headerText: 'Allowance (%)', editType: "numericedit",
                edit: { params: { showSpinButton: false, decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } }, width: 100
            },
            {
                field: 'ReqQty', headerText: 'Req Qty(KG )', editType: "numericedit", allowEditing: false,
                edit: { params: { showSpinButton: false, decimals: 0, min: 1 } }, width: 100
            },
            {
                field: 'ReqCone', headerText: 'Req Cone(PCS)', editType: "numericedit",
                edit: { params: { showSpinButton: false, decimals: 0, min: 1 } }, width: 100
            },
            { field: 'IsPR', headerText: 'Go for PR?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            { field: 'PhysicalCount', headerText: 'Physical Count', width: 120, allowEditing: false },
            { field: 'YarnLotNo', headerText: 'Lot No', width: 100, allowEditing: false },
            { field: 'SpinnerName', headerText: 'Spinner', width: 160, allowEditing: false },
            { field: 'SampleStockQty', headerText: 'Sample Stock Qty', width: 100, allowEditing: false, visible: menuParam == "MRBDS" },
            { field: 'AdvanceStockQty', headerText: 'Advance Stock Qty', width: 100, allowEditing: false, visible: menuParam == "MRBDS" },
            { field: 'YarnStockSetId', headerText: 'YarnStockSetId', width: 10, allowEditing: false, visible: false }
        ];
        childColumns.push.apply(childColumns, additionalColumns);
        childColumns = setMandatoryFieldsCSS(childColumns, "Segment1ValueId, Segment8ValueId, Distribution, Allowance, ReqQty, ReqCone");

        data.map(x => {
            x.GetYarnFromStock = 0;
        });

        ej.base.enableRipple(true);
        $tblOtherItemEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            showColumnChooser: true,
            showDefaultToolbar: false,
            toolbar: ['ColumnChooser'],
            columns: [
                //Master Grid Record
                { field: 'GetYarnFromStock', headerText: 'From Stock', allowEditing: false, visible: menuParam == "MRBDS", textAlign: 'center', width: 30, valueAccessor: displayStockIcon },
                { field: 'FCMRMasterID', isPrimaryKey: true, visible: false },
                { field: 'SubGroupID', visible: false },
                { field: 'Construction', headerText: 'Collar Description', width: 20, allowEditing: false },
                { field: 'Composition', headerText: 'Collar Type', width: 20, allowEditing: false },
                { field: 'Color', headerText: 'Body Color', width: 20, allowEditing: false },
                { field: 'Length', headerText: 'Length', width: 20, allowEditing: false },
                { field: 'Width', headerText: 'Width', width: 20, allowEditing: false },
                { field: 'DayValidDurationName', headerText: 'Yarn Sourcing Mode', width: 120, allowEditing: false, visible: isBDS == 3 },
                { field: 'TechnicalName', headerText: 'Technical Name', width: 20, allowEditing: false },
                { field: 'Qty', headerText: 'Booking Qty (Pcs)', width: 30, allowEditing: false, visible: false },
                { field: 'QtyInKG', headerText: 'Booking Qty (Kg)', width: 30, allowEditing: false, visible: false },
                { field: 'TotalQty', headerText: 'Req. Knitting Qty (Pcs)', width: 30, allowEditing: false },
                { field: 'TotalQtyInKG', headerText: 'Req. Knitting Qty (Kg)', width: 30, allowEditing: false }
            ],
            childGrid: {
                queryString: 'FCMRMasterID',
                //queryString: menuParam == "MRPBAck" ? 'YBChildID' : 'FCMRMasterID',
                allowResizing: true,
                autofitColumns: false,
                toolbar: ['Add'],
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns,
                actionBegin: function (args) {
                    if (args.requestType === 'beginEdit') {
                        if (args.rowData.YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                    }
                    else if (args.requestType === "add") {

                        if (status === statusConstants.COMPLETED || status === statusConstants.REVISE) {
                            //var isEqualData = isDeepEqual(args.data, args.rowData);
                            this.parentDetails.parentRowData.IsNeedRevisionTemp = true;
                        }

                        var totalDis = 0, remainDis = 0;
                        this.dataSource.forEach(l => {
                            totalDis += l.Distribution;
                        })
                        if (totalDis < 100) remainDis = 100 - totalDis;
                        else {
                            toastr.error("Distribution can not more then 100!!");
                            args.cancel = true;
                            return;
                        }
                        var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQtyInKG) * parseFloat(remainDis) / 100);
                        var reqQty = netConsumption;
                        args.data.Distribution = remainDis;
                        args.data.BookingQty = netConsumption.toFixed(4);
                        args.data.Allowance = 0.00;
                        args.data.ReqQty = reqQty.toFixed(2);
                        args.data.FCMRChildID = maxColCollar++; //getMaxIdForArray($tblChildEl.getCurrentViewRecords(), "FCMRChildID");

                        args.data.FCMRMasterID = this.parentDetails.parentKeyFieldValue;
                        args.data.ReqCone = 1;
                    }
                    else if (args.requestType === "save") {
                        if (status === statusConstants.COMPLETED || status === statusConstants.REVISE) {
                            var isEqualData = isDeepEqual(args.data, args.rowData);
                            this.parentDetails.parentRowData.IsNeedRevisionTemp = !isEqualData;
                        }

                        args.data.IsInvalidItem = typeof args.data.IsInvalidItem === "undefined" ? 1 : args.data.IsInvalidItem;
                        args.data.StockItemNote = getDefaultValueWhenInvalidS(args.data.StockItemNote);
                        if (args.data.YarnStockSetId > 0 && args.data.IsInvalidItem && args.data.IsPR && isBDS == 1) {
                            toastr.error(`'Go For PR' is only for stock valid item`);
                            args.data.IsPR = false;
                            $tblOtherItemEl.updateRow(args.rowIndex, args.data);
                            return false;
                        }

                        //Check YDItem when YD field check
                        if (args.data.YD) {
                            args.data.YDItem = true;
                        }

                        var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQtyInKG) * parseFloat(args.data.Distribution) / 100);
                        var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(args.data.Allowance)) / 100);
                        if (netConsumption != 0) {
                            args.data.BookingQty = netConsumption.toFixed(4);
                        }
                        if (reqQty != 0) {
                            args.data.ReqQty = reqQty.toFixed(2);
                        }
                        args.data = getFreeConceptMRChild(args.data);


                        //args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                        args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                        args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                        args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                        args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                        args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                        args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                        args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;

                        if (menuParam == "MRBDS") {
                            args.data = getYarnStockRelatedValues(args.data, args.rowData, args.previousData);
                        }

                        var subGroupId = this.parentDetails.parentRowData.subGroupId;
                        var parentObj = this.parentDetails.parentRowData;
                        args.data = getDayValidDurationInfos(args.data, parentObj, subGroupId);

                        $tblOtherItemEl.updateRow(args.rowIndex, args.data);
                    }
                    else if (args.requestType === "delete") {

                        if (status === statusConstants.COMPLETED || status === statusConstants.REVISE) {
                            var isEqualData = isDeepEqual(args.data, args.rowData);
                            this.parentDetails.parentRowData.IsNeedRevisionTemp = !isEqualData;
                        }

                        if (args.data[0].YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                        //var index = $tblOtherItemEl.getRowIndexByPrimaryKey(args.data[0].FCMRChildID);
                        //if (index > -1) {
                        //    args.data.EntityState = 8;
                        //    masterData.Childs[index] = args.data;
                        //}
                    }
                },
                load: loadFirstLChildCollarGrid
            },
            enableContextMenu: true,
            contextMenuItems: [
                { text: 'Copy Yarn Information', target: '.e-content', id: 'copy' },
                { text: 'Paste Yarn Information', target: '.e-content', id: 'paste' }
            ],
            contextMenuClick: function (args) {
                if (args.item.id === 'copy') {
                    collarYarnItem = objectCopy(args.rowInfo.rowData.Childs);
                    if (collarYarnItem.length == 0) {
                        toastr.error("No Yarn information found to copy!!");
                        return;
                    }
                }
                else if (args.item.id === 'paste') {
                    var rowIndex = args.rowInfo.rowIndex;
                    if (collarYarnItem == null || collarYarnItem.length == 0) {
                        toastr.error("Please copy first!!");
                        return;
                    } else {
                        for (var i = 0; i < collarYarnItem.length; i++) {
                            var copiedItem = objectCopy(collarYarnItem[i]);
                            copiedItem.FCMRChildID = maxColYarn++;
                            copiedItem.FCMRMasterID = args.rowInfo.rowData.FCMRMasterID;
                            var netConsumption = (parseFloat(args.rowInfo.rowData.TotalQtyInKG) * parseFloat(copiedItem.Distribution) / 100);
                            var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(copiedItem.Allowance)) / 100);
                            copiedItem.BookingQty = netConsumption.toFixed(4);
                            copiedItem.ReqQty = reqQty.toFixed(2);
                            args.rowInfo.rowData.DayValidDurationName = getDefaultValueWhenInvalidS(args.rowInfo.rowData.DayValidDurationName);
                            if (isDayValidDurationLocal(args.rowInfo.rowData.DayValidDurationName)) {
                                copiedItem.DayValidDurationId = args.rowInfo.rowData.DayValidDurationId;
                            }
                            args.rowInfo.rowData.Childs.push(copiedItem);
                        }
                        $tblOtherItemEl.refresh();
                    }
                }
            },
            recordClick: function (args) {
                if (args.column && args.column.field == "GetYarnFromStock") {
                    var otherQuery = " AND (SampleStockQty > 0 OR AdvanceStockQty > 0) ";
                    otherQuery = replaceInvalidChar(otherQuery);
                    var finder = new commonFinder({
                        title: "Yarn Stock",
                        pageId: pageId,
                        height: 320,
                        modalSize: "modal-lg",
                        apiEndPoint: `/api/yarn-stock-adjustment/get-all-stocks-with-custom-query/${otherQuery}`,
                        headerTexts: "Yarn Detail,Count,Physical Count,Lot No,Shade Code,Supplier,Spinner,Sample Stock Qty,Advance Stock Qty,Block Sample Stock Qty,Block Advance Stock Qty,Issued Qty,Item Type,Note",
                        fields: "YarnCategory,Count,PhysicalCount,YarnLotNo,ShadeCode,SupplierName,SpinnerName,SampleStockQty,AdvanceStockQty,BlockSampleStockQty,BlockAdvanceStockQty,TotalIssueQty,InvalidItem_St,Note",
                        primaryKeyColumn: "YarnStockSetId",
                        autofitColumns: true,
                        onSelect: function (res) {
                            finder.hideModal();
                            loadChilds(11, args.rowData.FCMRMasterID, res.rowData, maxColCollar++);
                        }
                    });
                    finder.showModal();
                }
            },
        });
        $tblOtherItemEl.refreshColumns;
        $tblOtherItemEl.appendTo(tblOtherItemId);
    }

    function loadFirstLChildCollarGrid() {
        this.dataSource = this.parentDetails.parentRowData.Childs;
    }

    async function initCuffItemTable(data) {
        if (menuParam == "MRPB") {
            btnCuffApplyKGClick(data);
        }
        if ($tblCuffItemEl) $tblCuffItemEl.destroy();

        //Child Grid Record
        var childColumns = await getYarnItemColumnsWithSearchDDLAsync(ch_getCountRelatedList(data, 1), currentChildRowData);
        childColumns.unshift({ field: 'FCMRChildID', isPrimaryKey: true, visible: false });
        //if (menuParam == "MRPBAck") {
        //    childColumns.unshift({ field: 'YBChildID', isPrimaryKey: true, visible: false });
        //}
        //else {
        //    childColumns.unshift({ field: 'FCMRChildID', isPrimaryKey: true, visible: false });
        //}
        var additionalColumns = [
            {
                field: 'ShadeCode',
                headerText: 'Shade Code',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: data[0].YarnShadeBooks,
                displayField: "ShadeCode",
                edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'DayValidDurationId', headerText: 'Yarn Sourcing Mode', width: 120,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.DayValidDurations,
                displayField: "text",
                edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'YDProductionMasterID', headerText: 'YDProductionMasterID', visible: false
            },
            { field: 'YDItem', headerText: 'YD Item?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            { field: 'YD', headerText: 'Go for YD?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            {
                field: 'Distribution', headerText: 'Yarn Distribution (%)', editType: "numericedit",
                /*edit: { params: { showSpinButton: false, decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } }, width: 100*/
                edit: { params: { showSpinButton: false, decimals: 2, min: 0 } }, width: 100
            },
            {
                field: 'BookingQty', headerText: 'Net Consumption', allowEditing: false,
                edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } }, width: 100
            },
            {
                field: 'Allowance', headerText: 'Allowance (%)', editType: "numericedit",
                edit: { params: { showSpinButton: false, decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } }, width: 100
            },
            {
                field: 'ReqQty', headerText: 'Req Qty(KG)', editType: "numericedit", allowEditing: false,
                edit: { params: { showSpinButton: false, decimals: 0, min: 1 } }, width: 100
            },
            {
                field: 'ReqCone', headerText: 'Req Cone(PCS)', editType: "numericedit",
                edit: { params: { showSpinButton: false, decimals: 0, min: 1 } }, width: 100
            },
            { field: 'IsPR', headerText: 'Go for PR?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            { field: 'PhysicalCount', headerText: 'Physical Count', width: 100, allowEditing: false },
            { field: 'YarnLotNo', headerText: 'Lot No', width: 60, allowEditing: false },
            { field: 'SpinnerName', headerText: 'Spinner', width: 100, allowEditing: false },
            { field: 'SampleStockQty', headerText: 'Sample Stock Qty', width: 100, allowEditing: false, visible: menuParam == "MRBDS" },
            { field: 'AdvanceStockQty', headerText: 'Advance Stock Qty', width: 100, allowEditing: false, visible: menuParam == "MRBDS" },
            { field: 'YarnStockSetId', headerText: 'YarnStockSetId', width: 10, allowEditing: false, visible: false }
        ];
        childColumns.push.apply(childColumns, additionalColumns);
        childColumns = setMandatoryFieldsCSS(childColumns, "Segment1ValueId, Segment8ValueId, Distribution, Allowance, ReqQty, ReqCone");

        data.map(x => {
            x.GetYarnFromStock = 0;
        });

        ej.base.enableRipple(true);
        $tblCuffItemEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            showColumnChooser: true,
            showDefaultToolbar: false,
            toolbar: ['ColumnChooser'],
            columns: [
                //Master Grid Record
                { field: 'GetYarnFromStock', headerText: 'From Stock', allowEditing: false, visible: menuParam == "MRBDS", textAlign: 'center', width: 30, valueAccessor: displayStockIcon },
                { field: 'FCMRMasterID', isPrimaryKey: true, visible: false },
                { field: 'SubGroupID', visible: false },
                { field: 'Construction', headerText: 'Construction', width: 20, allowEditing: false },
                { field: 'Composition', headerText: 'Composition', width: 20, allowEditing: false },
                { field: 'Color', headerText: 'Body Color', width: 20, allowEditing: false },
                { field: 'Length', headerText: 'Length', width: 20, allowEditing: false },
                { field: 'Width', headerText: 'Width', width: 20, allowEditing: false },
                { field: 'DayValidDurationName', headerText: 'Yarn Sourcing Mode', width: 120, allowEditing: false, visible: isBDS == 3 },
                { field: 'TechnicalName', headerText: 'Technical Name', width: 20, allowEditing: false },
                { field: 'Qty', headerText: 'Booking Qty (Pcs)', width: 30, allowEditing: false, visible: false },
                { field: 'QtyInKG', headerText: 'Booking Qty (Kg)', width: 30, allowEditing: false, visible: false },
                { field: 'TotalQty', headerText: 'Req. Knitting Qty (Pcs)', width: 30, allowEditing: false },
                { field: 'TotalQtyInKG', headerText: 'Req. Knitting Qty (Kg)', width: 30, allowEditing: false }
            ],
            childGrid: {
                queryString: 'FCMRMasterID',
                //queryString: menuParam == "MRPBAck" ? 'YBChildID' : 'FCMRMasterID',
                allowResizing: true,
                autofitColumns: false,
                toolbar: ['Add'],
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns,
                actionBegin: function (args) {

                    if (args.requestType === 'beginEdit') {
                        if (args.rowData.YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                    }
                    else if (args.requestType === "add") {

                        if (status === statusConstants.COMPLETED || status === statusConstants.REVISE) {
                            //var isEqualData = isDeepEqual(args.data, args.rowData);
                            this.parentDetails.parentRowData.IsNeedRevisionTemp = true;
                        }

                        var totalDis = 0, remainDis = 0;
                        this.dataSource.forEach(l => {
                            totalDis += l.Distribution;
                        })
                        if (totalDis < 100) remainDis = 100 - totalDis;
                        else {
                            toastr.error("Distribution can not more then 100!!");
                            args.cancel = true;
                            return;
                        }
                        var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQtyInKG) * parseFloat(remainDis) / 100);
                        var reqQty = netConsumption;
                        args.data.Distribution = remainDis;
                        args.data.BookingQty = netConsumption.toFixed(4);
                        args.data.Allowance = 0.00;
                        args.data.ReqQty = reqQty.toFixed(2);


                        args.data.FCMRChildID = maxColCuff++; //getMaxIdForArray($tblChildEl.getCurrentViewRecords(), "FCMRChildID");
                        args.data.FCMRMasterID = this.parentDetails.parentKeyFieldValue;

                        //args.data.Distribution = 100.00;
                        //args.data.BookingQty = (parseFloat(this.parentDetails.parentRowData.TotalQtyInKG)).toFixed(2);
                        //args.data.Allowance = 0.00;
                        //args.data.ReqQty = (parseFloat(this.parentDetails.parentRowData.TotalQtyInKG)).toFixed(2);
                        args.data.ReqCone = 1;
                    }
                    else if (args.requestType === "save") {
                        if (status === statusConstants.COMPLETED || status === statusConstants.REVISE) {
                            var isEqualData = isDeepEqual(args.data, args.rowData);
                            this.parentDetails.parentRowData.IsNeedRevisionTemp = !isEqualData;
                        }

                        args.data.IsInvalidItem = typeof args.data.IsInvalidItem === "undefined" ? 1 : args.data.IsInvalidItem;
                        args.data.StockItemNote = getDefaultValueWhenInvalidS(args.data.StockItemNote);
                        if (args.data.YarnStockSetId > 0 && args.data.IsInvalidItem && args.data.IsPR && isBDS == 1) {
                            toastr.error(`'Go For PR' is only for stock valid item`);
                            args.data.IsPR = false;
                            $tblCuffItemEl.updateRow(args.rowIndex, args.data);
                            return false;
                        }

                        //Check YDItem when YD field check
                        if (args.data.YD) {
                            args.data.YDItem = true;
                        }

                        var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQtyInKG) * parseFloat(args.data.Distribution) / 100);
                        var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(args.data.Allowance)) / 100);
                        if (netConsumption != 0) {
                            args.data.BookingQty = netConsumption.toFixed(4);
                        }
                        if (reqQty != 0) {
                            args.data.ReqQty = reqQty.toFixed(2);
                        }
                        args.data = getFreeConceptMRChild(args.data);

                        //args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                        args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                        args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                        args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                        args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                        args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                        args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                        args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;

                        if (menuParam == "MRBDS") {
                            args.data = getYarnStockRelatedValues(args.data, args.rowData, args.previousData);
                        }

                        var subGroupId = this.parentDetails.parentRowData.subGroupId;
                        var parentObj = this.parentDetails.parentRowData;
                        args.data = getDayValidDurationInfos(args.data, parentObj, subGroupId);

                        $tblCuffItemEl.updateRow(args.rowIndex, args.data);
                    }
                    else if (args.requestType === "delete") {

                        if (status === statusConstants.COMPLETED || status === statusConstants.REVISE) {
                            var isEqualData = isDeepEqual(args.data, args.rowData);
                            this.parentDetails.parentRowData.IsNeedRevisionTemp = !isEqualData;
                        }

                        if (args.data[0].YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                        //var index = $tblCuffItemEl.getRowIndexByPrimaryKey(args.data[0].FCMRChildID);
                        //if (index > -1) {
                        //    args.data.EntityState = 8;
                        //    masterData.Childs[index] = args.data;
                        //}
                    }
                },
                load: loadFirstLChildCuffGrid
            },
            enableContextMenu: true,
            contextMenuItems: [
                { text: 'Copy Yarn Information', target: '.e-content', id: 'copy' },
                { text: 'Paste Yarn Information', target: '.e-content', id: 'paste' }
            ],
            contextMenuClick: function (args) {
                if (args.item.id === 'copy') {
                    //cuffYarnItem = objectCopy(args.rowInfo.rowData.Childs);
                    collarYarnItem = objectCopy(args.rowInfo.rowData.Childs);
                    if (collarYarnItem.length == 0) {
                        toastr.error("No Yarn information found to copy!!");
                        return;
                    }
                }
                else if (args.item.id === 'paste') {
                    var rowIndex = args.rowInfo.rowIndex;
                    if (collarYarnItem == null || collarYarnItem.length == 0) {
                        toastr.error("Please copy first!!");
                        return;
                    } else {
                        for (var i = 0; i < collarYarnItem.length; i++) {
                            var copiedItem = objectCopy(collarYarnItem[i]);
                            copiedItem.FCMRChildID = maxColYarn++;
                            copiedItem.FCMRMasterID = args.rowInfo.rowData.FCMRMasterID;
                            var netConsumption = (parseFloat(args.rowInfo.rowData.TotalQtyInKG) * parseFloat(copiedItem.Distribution) / 100);
                            var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(copiedItem.Allowance)) / 100);
                            copiedItem.BookingQty = netConsumption.toFixed(4);
                            copiedItem.ReqQty = reqQty.toFixed(2);
                            args.rowInfo.rowData.DayValidDurationName = getDefaultValueWhenInvalidS(args.rowInfo.rowData.DayValidDurationName);
                            if (isDayValidDurationLocal(args.rowInfo.rowData.DayValidDurationName)) {
                                copiedItem.DayValidDurationId = args.rowInfo.rowData.DayValidDurationId;
                            }
                            args.rowInfo.rowData.Childs.push(copiedItem);
                        }
                        $tblCuffItemEl.refresh();
                    }
                }
            },
            recordClick: function (args) {
                if (args.column && args.column.field == "GetYarnFromStock") {
                    var otherQuery = " AND (SampleStockQty > 0 OR AdvanceStockQty > 0) ";
                    otherQuery = replaceInvalidChar(otherQuery);
                    var finder = new commonFinder({
                        title: "Yarn Stock",
                        pageId: pageId,
                        height: 320,
                        modalSize: "modal-lg",
                        apiEndPoint: `/api/yarn-stock-adjustment/get-all-stocks-with-custom-query/${otherQuery}`,
                        headerTexts: "Yarn Detail,Count,Physical Count,Lot No,Shade Code,Supplier,Spinner,Sample Stock Qty,Advance Stock Qty,Block Sample Stock Qty,Block Advance Stock Qty,Issued Qty,Item Type,Note",
                        fields: "YarnCategory,Count,PhysicalCount,YarnLotNo,ShadeCode,SupplierName,SpinnerName,SampleStockQty,AdvanceStockQty,BlockSampleStockQty,BlockAdvanceStockQty,TotalIssueQty,InvalidItem_St,Note",
                        primaryKeyColumn: "YarnStockSetId",
                        autofitColumns: true,
                        onSelect: function (res) {
                            finder.hideModal();
                            loadChilds(12, args.rowData.FCMRMasterID, res.rowData, maxColCuff++);
                        }
                    });
                    finder.showModal();
                }
            },
        });
        $tblCuffItemEl.refreshColumns;
        $tblCuffItemEl.appendTo(tblCuffItemId);
    }

    function loadFirstLChildCuffGrid() {
        this.dataSource = this.parentDetails.parentRowData.Childs
    }
    function backToListWithoutFilter() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
    }
    function backToList() {
        backToListWithoutFilter();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#FCMRMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getNew(FBAckID) {

        resetGlobals();
        var url = `/api/mr-bds/new/${FBAckID}/${menuParam}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.BookingNo = masterData.OtherItems.length > 0 ? masterData.OtherItems[0].BookingNo : "";
                masterData.BookingDate = masterData.OtherItems.length > 0 ? formatDateToDefault(masterData.OtherItems[0].BookingDate) : "";
                setFormData($formEl, masterData);
                isFabric = 0, isCollar = 0, isCuff = 0;
                if (masterData.HasFabric) {
                    var FabricData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.FABRIC });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        FabricData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initYarnChildTable(FabricData);
                    $formEl.find("#divFabricInfo").show();
                    $formEl.find("#onlyFabric").prop('checked', true);
                    isFabric = 1;
                }
                else {
                    $formEl.find("#divFabricInfo").hide();
                    $formEl.find("#onlyFabric").prop('checked', false);
                    isFabric = 0;
                }
                if (masterData.HasCollar) {
                    var CollarData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.COLLAR });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CollarData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initOtherItemTable(CollarData);
                    $formEl.find("#divOtherItem").show();
                    $formEl.find("#fabricOtherItem").prop('checked', true);
                    isCollar = 1;
                }
                else {
                    $formEl.find("#divOtherItem").hide();
                    $formEl.find("#fabricOtherItem").prop('checked', false);
                    isCollar = 0;
                }
                if (masterData.HasCuff) {
                    var CuffData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.CUFF });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CuffData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initCuffItemTable(CuffData);
                    $formEl.find("#divCuffItem").show();
                    $formEl.find("#onlyOtherItem").prop('checked', true);
                    isCuff = 1;
                }
                else {
                    $formEl.find("#divCuffItem").hide();
                    $formEl.find("#onlyOtherItem").prop('checked', false);
                    isCuff = 0;
                }

                $formEl.find("#BookingID").val(masterData.BookingID);
                if (masterData.TrialNo == 0) {
                    $("#divRetrial").fadeOut();
                } else {
                    $("#divRetrial").fadeIn();
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(grpConceptNo) {

        resetGlobals();
        axios.get(`/api/mr-bds/${grpConceptNo}/${menuParam}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                if (masterData.OtherItems.length > 0) {
                    masterData.BookingNo = masterData.OtherItems[0].BookingNo;
                    masterData.BookingDate = formatDateToDefault(masterData.OtherItems[0].BookingDate);
                    masterData.BookingID = masterData.OtherItems[0].BookingID;
                    masterData.CollarSizeID = masterData.OtherItems[0].CollarSizeID;
                    masterData.CollarWeightInGm = masterData.OtherItems[0].CollarWeightInGm;
                    masterData.CuffSizeID = masterData.OtherItems[0].CuffSizeID;
                    masterData.CuffWeightInGm = masterData.OtherItems[0].CuffWeightInGm;
                }
                setFormData($formEl, masterData);
                isFabric = 0, isCollar = 0, isCuff = 0;
                if (masterData.HasFabric) {
                    var FabricData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.FABRIC });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        FabricData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initYarnChildTable(FabricData);
                    $formEl.find("#divFabricInfo").show();
                    $formEl.find("#onlyFabric").prop('checked', true);
                    isFabric = 1;
                }
                else {
                    $formEl.find("#divFabricInfo").hide();
                    $formEl.find("#onlyFabric").prop('checked', false);
                    isFabric = 0;
                }
                if (masterData.HasCollar) {
                    var CollarData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.COLLAR });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CollarData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initOtherItemTable(CollarData);
                    $formEl.find("#divOtherItem").show();
                    $formEl.find("#fabricOtherItem").prop('checked', true);
                    isCollar = 1;
                }
                else {
                    $formEl.find("#divOtherItem").hide();
                    $formEl.find("#fabricOtherItem").prop('checked', false);
                    isCollar = 0;
                }
                if (masterData.HasCuff) {
                    var CuffData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.CUFF });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CuffData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initCuffItemTable(CuffData);
                    $formEl.find("#divCuffItem").show();
                    $formEl.find("#onlyOtherItem").prop('checked', true);
                    isCuff = 1;
                }
                else {
                    $formEl.find("#divCuffItem").hide();
                    $formEl.find("#onlyOtherItem").prop('checked', false);
                    isCuff = 0;
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getRevision(fbAckId, grpConceptNo) {
        resetGlobals();
        axios.get(`/api/mr-bds/revision/${fbAckId}/${grpConceptNo}/${menuParam}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                if (masterData.OtherItems.length > 0) {
                    masterData.BookingNo = masterData.OtherItems[0].BookingNo;
                    masterData.BookingDate = formatDateToDefault(masterData.OtherItems[0].BookingDate);
                    masterData.BookingID = masterData.OtherItems[0].BookingID;
                    masterData.CollarSizeID = masterData.OtherItems[0].CollarSizeID;
                    masterData.CollarWeightInGm = masterData.OtherItems[0].CollarWeightInGm;
                    masterData.CuffSizeID = masterData.OtherItems[0].CuffSizeID;
                    masterData.CuffWeightInGm = masterData.OtherItems[0].CuffWeightInGm;
                }
                setFormData($formEl, masterData);
                isFabric = 0, isCollar = 0, isCuff = 0;
                if (masterData.HasFabric) {
                    var FabricData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.FABRIC });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        FabricData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initYarnChildTable(FabricData);
                    $formEl.find("#divFabricInfo").show();
                    $formEl.find("#onlyFabric").prop('checked', true);
                    isFabric = 1;
                }
                else {
                    $formEl.find("#divFabricInfo").hide();
                    $formEl.find("#onlyFabric").prop('checked', false);
                    isFabric = 0;
                }
                if (masterData.HasCollar) {
                    var CollarData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.COLLAR });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CollarData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initOtherItemTable(CollarData);
                    $formEl.find("#divOtherItem").show();
                    $formEl.find("#fabricOtherItem").prop('checked', true);
                    isCollar = 1;
                }
                else {
                    $formEl.find("#divOtherItem").hide();
                    $formEl.find("#fabricOtherItem").prop('checked', false);
                    isCollar = 0;
                }
                if (masterData.HasCuff) {
                    var CuffData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.CUFF });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CuffData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initCuffItemTable(CuffData);
                    $formEl.find("#divCuffItem").show();
                    $formEl.find("#onlyOtherItem").prop('checked', true);
                    isCuff = 1;
                }
                else {
                    $formEl.find("#divCuffItem").hide();
                    $formEl.find("#onlyOtherItem").prop('checked', false);
                    isCuff = 0;
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getRevisionForCompleteList(grpConceptNo) {
        axios.get(`/api/mr-bds/revision/${grpConceptNo}/${menuParam}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                if (masterData.OtherItems.length > 0) {
                    masterData.BookingNo = masterData.OtherItems[0].BookingNo;
                    masterData.BookingDate = formatDateToDefault(masterData.OtherItems[0].BookingDate);
                    masterData.BookingID = masterData.OtherItems[0].BookingID;
                    masterData.BuyerName = masterData.OtherItems[0].BuyerName;
                    masterData.CollarSizeID = masterData.OtherItems[0].CollarSizeID;
                    masterData.CollarWeightInGm = masterData.OtherItems[0].CollarWeightInGm;
                    masterData.CuffSizeID = masterData.OtherItems[0].CuffSizeID;
                    masterData.CuffWeightInGm = masterData.OtherItems[0].CuffWeightInGm;
                }
                setFormData($formEl, masterData);
                isFabric = 0, isCollar = 0, isCuff = 0;
                if (masterData.HasFabric) {
                    var FabricData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.FABRIC });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        FabricData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initYarnChildTable(FabricData);
                    $formEl.find("#divFabricInfo").show();
                    $formEl.find("#onlyFabric").prop('checked', true);
                    isFabric = 1;
                }
                else {
                    $formEl.find("#divFabricInfo").hide();
                    $formEl.find("#onlyFabric").prop('checked', false);
                    isFabric = 0;
                }
                if (masterData.HasCollar) {
                    var CollarData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.COLLAR });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CollarData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initOtherItemTable(CollarData);
                    $formEl.find("#divOtherItem").show();
                    $formEl.find("#fabricOtherItem").prop('checked', true);
                    isCollar = 1;
                }
                else {
                    $formEl.find("#divOtherItem").hide();
                    $formEl.find("#fabricOtherItem").prop('checked', false);
                    isCollar = 0;
                }
                if (masterData.HasCuff) {
                    var CuffData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.CUFF });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CuffData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initCuffItemTable(CuffData);
                    $formEl.find("#divCuffItem").show();
                    $formEl.find("#onlyOtherItem").prop('checked', true);
                    isCuff = 1;
                }
                else {
                    $formEl.find("#divCuffItem").hide();
                    $formEl.find("#onlyOtherItem").prop('checked', false);
                    isCuff = 0;
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getAcknowledgeForList(YBookingNo) {
        //var YBookingNo = 122052200531;
        //axios.get(`/api/mr-bds/pendingacknowledgement/${grpConceptNo}`)
        //axios.get(`/api/yarn-booking/GetAsync/${YBookingNo}`)

        axios.get(`/api/yarn-booking/GetAsyncforAutoPR/${YBookingNo}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                //if (masterData.OtherItems.length > 0) {
                //    masterData.BookingNo = masterData.OtherItems[0].BookingNo;
                //    masterData.BookingDate = formatDateToDefault(masterData.OtherItems[0].BookingDate);
                //    masterData.BookingID = masterData.OtherItems[0].BookingID;
                //}
                masterData.BookingDate = formatDateToDefault(masterData.YBookingDate);
                setFormData($formEl, masterData);
                isFabric = 0, isCollar = 0, isCuff = 0;
                if (masterData.HasFabric) {
                    var FabricData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.FABRIC });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        FabricData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initYarnChildTable(FabricData);
                    $formEl.find("#divFabricInfo").show();
                    $formEl.find("#onlyFabric").prop('checked', true);
                    isFabric = 1;
                }
                else {
                    $formEl.find("#divFabricInfo").hide();
                    $formEl.find("#onlyFabric").prop('checked', false);
                    isFabric = 0;
                }
                if (masterData.HasCollar) {
                    var CollarData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.COLLAR });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CollarData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initOtherItemTable(CollarData);
                    $formEl.find("#divOtherItem").show();
                    $formEl.find("#fabricOtherItem").prop('checked', true);
                    isCollar = 1;
                }
                else {
                    $formEl.find("#divOtherItem").hide();
                    $formEl.find("#fabricOtherItem").prop('checked', false);
                    isCollar = 0;
                }
                if (masterData.HasCuff) {
                    var CuffData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.CUFF });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CuffData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initCuffItemTable(CuffData);
                    $formEl.find("#divCuffItem").show();
                    $formEl.find("#onlyOtherItem").prop('checked', true);
                    isCuff = 1;
                }
                else {
                    $formEl.find("#divCuffItem").hide();
                    $formEl.find("#onlyOtherItem").prop('checked', false);
                    isCuff = 0;
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }


    function isValidChildForm(data) {
        var isValidItemInfo = false;

        //Distribution check
        var dis = 0, ciFlag = 0;
        $.each(data, function (i, obj) {
            $.each(obj.Childs, function (j, objChild) {
                if (obj.FCMRMasterID == objChild.FCMRMasterID) {
                    dis = parseInt(dis) + parseInt(objChild.Distribution);
                    ciFlag = 1;
                }
            });

            if (parseInt(dis) != 100 && parseInt(ciFlag) == 1) {
                //toastr.error("Yarn Distribution must be 100%");
                //isValidItemInfo = true;
                dis = 0;
                ciFlag = 0;
            }
            dis = 0;
            ciFlag = 0;
        });

        //Child Items Validation Check

        for (var i = 0; i < data.length; i++) {
            if (data[i].Childs.length === 0) {
                toastr.error("At least 1 Yarn item is required.");
                isValidItemInfo = true;
                break;
            }
        }
        return isValidItemInfo;
    }
    function checkAndGetValueNumber(value) {
        if (value === "undefined" || value == null) {
            return 0;
        }
        return value;
    }
    function checkAndGetValueText(value) {
        if (value === "undefined" || value == null) {
            return "";
        }
        return value;
    }
    function isInvalidSegment(segId) {
        if (typeof segId === "undefined" || segId == null || segId == 0) return true;
        return false;
    }
    function isValidYarnItems(fBookingChild) {
        var hasError = false;
        for (var i = 0; i < fBookingChild.length; i++) {
            var pChild = fBookingChild[i];
            for (var j = 0; j < pChild.Childs.length; j++) {
                var child = pChild.Childs[j];


                if (isInvalidSegment(child.Segment1ValueId)) child.Segment1ValueId = 0;
                if (isInvalidSegment(child.Segment2ValueId)) child.Segment2ValueId = 0;
                if (isInvalidSegment(child.Segment3ValueId)) child.Segment3ValueId = 0;
                if (isInvalidSegment(child.Segment4ValueId)) child.Segment4ValueId = 0;
                if (isInvalidSegment(child.Segment5ValueId)) {
                    child.Segment5ValueId = 0;
                    child.Segment5ValueDesc = "";
                }
                if (isInvalidSegment(child.Segment6ValueId)) child.Segment6ValueId = 0;

                if (child.Segment1ValueId == 0) {
                    toastr.error("Select composition");
                    hasError = true;
                    break;
                }

                if (child.Segment2ValueId == 0 && child.IsPR) {
                    toastr.error("Select yarn type for 'Go for PR'");
                    hasError = true;
                    break;
                }

                if (child.Segment3ValueId == 0 && child.IsPR) {
                    toastr.error("Select manufacturing process for 'Go for PR'");
                    hasError = true;
                    break;
                }

                if (child.Segment4ValueId == 0 && child.IsPR) {
                    toastr.error("Select sub process for 'Go for PR'");
                    hasError = true;
                    break;
                }

                if (child.Segment5ValueId == 0 && child.IsPR) {
                    toastr.error("Select quality parameter for 'Go for PR'");
                    hasError = true;
                    break;
                }

                if (child.Segment6ValueId == 0) {
                    toastr.error("Select count");
                    hasError = true;
                    break;
                }
                if ((child.Segment5ValueDesc.toLowerCase() == "melange" || child.Segment5ValueDesc.toLowerCase() == "color melange") && (child.ShadeCode == null || child.ShadeCode == "")) {
                    toastr.error("Select shade code for color melange");
                    hasError = true;
                    break;
                }
            }
            if (hasError) break;
        }
        if (hasError) return false;
        return true;
    }
    function save(IsComplete) {
        //Validation
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        var mainData = formElToJson($formEl);
        var mrListMain = [];
        var hasError = false;
        if (isFabric == 1) {
            var mrList = $tblChildEl.getCurrentViewRecords();
            if (mrList.length > 0) {
                if (IsComplete) {
                    if (!isValidYarnItems(mrList)) return false;
                }
                for (var i = 0; i < mrList.length; i++) {
                    if (menuParam == "MRPB") {
                        mrList[i].Childs.map(y => { y.IsPR = true; });
                    }
                    var childs = mrList[i].Childs;
                    if (IsComplete) {
                        if (childs.length == 0) {
                            toastr.error("At least 1 Yarn item is required (Yarn Information).");
                            hasError = true;
                            break;
                        }
                        else {
                            var totalYarnDis = 0;
                            for (var j = 0; j < childs.length; j++) {
                                if (childs[j].Segment5ValueDesc) {
                                    if ((childs[j].Segment5ValueDesc.toLowerCase() == "melange" || childs[j].Segment5ValueDesc.toLowerCase() == "color melange") && (childs[j].ShadeCode == null || childs[j].ShadeCode == "")) {
                                        toastr.error("Select shade code for color melange");
                                        hasError = true;
                                        break;
                                    }
                                }
                                totalYarnDis += GetNumberValue(childs[j].Distribution);

                                if (menuParam == "MRBDS") {
                                    if (isBDS == 1 && childs[j].YarnStockSetId == 0 && !childs[j].YDItem && !childs[j].IsPR) {
                                        toastr.error(`Select YD Item or Go for PR for non-stock item at row ${j + 1}.`);
                                        hasError = true;
                                        break;
                                    }
                                }
                            }
                            if (hasError) break;
                            if (totalYarnDis != 100) {
                                toastr.error("Sum of Yarn Distribution must be 100 at Yarn Information. Current sum of Yarn Distribution is " + totalYarnDis);
                                hasError = true;
                                break;
                            }
                        }
                    }
                    mrList[i].IsComplete = IsComplete;
                    mrList[i].MenuParam = menuParam;
                    mrListMain.push(mrList[i]);
                }
            }
        }
        if (isCollar == 1 && !hasError) {
            var mrList = $tblOtherItemEl.getCurrentViewRecords();
            if (mrList.length > 0 && !hasError) {
                if (IsComplete) {
                    if (!isValidYarnItems(mrList)) return false;
                }
                for (var i = 0; i < mrList.length; i++) {
                    if (menuParam == "MRPB") {
                        mrList[i].Childs.map(y => { y.IsPR = true; });
                    }
                    var childs = mrList[i].Childs;
                    if (IsComplete) {
                        if (childs.length == 0) {
                            toastr.error("At least 1 Yarn item is required (Collar Information).");
                            hasError = true;
                            break;
                        }
                        if (childs.length > 0) {
                            var totalYarnDis = 0;
                            for (var j = 0; j < childs.length; j++) {
                                if (childs[j].Segment5ValueDesc) {
                                    if ((childs[j].Segment5ValueDesc.toLowerCase() == "melange" || childs[j].Segment5ValueDesc.toLowerCase() == "color melange") && (childs[j].ShadeCode == null || childs[j].ShadeCode == "")) {
                                        toastr.error("Select shade code for color melange");
                                        hasError = true;
                                        break;
                                    }
                                }
                                totalYarnDis += GetNumberValue(childs[j].Distribution);
                            }
                            if (hasError) break;
                            if (totalYarnDis != 100) {
                                toastr.error("Sum of Yarn Distribution must be 100 at Collar Information. Current sum of Yarn Distribution is " + totalYarnDis);
                                hasError = true;
                                break;
                            }
                        }
                    }
                    mrList[i].IsComplete = IsComplete;
                    if (menuParam == "MRPB") {
                        mrList[i].CollarSizeID = $(pageIdWithHash).find("#CollarSizeID").val();
                        mrList[i].CuffSizeID = $(pageIdWithHash).find("#CuffSizeID").val();
                        mrList[i].CollarWeightInGm = $(pageIdWithHash).find("#CollarWeightInGm").val();
                        mrList[i].CuffWeightInGm = $(pageIdWithHash).find("#CuffWeightInGm").val();
                        mrList[i].AllCollarSizeList = masterData.AllCollarSizeList;
                        mrList[i].AllCuffSizeList = masterData.AllCuffSizeList;
                        mrList[i].CollarSizeList = masterData.CollarSizeList;
                        mrList[i].CuffSizeList = masterData.CuffSizeList;
                        mrList[i].MenuParam = menuParam;
                    }
                    mrListMain.push(mrList[i]);
                }
            }
        }
        if (isCuff == 1 && !hasError) {
            var mrList = $tblCuffItemEl.getCurrentViewRecords();
            if (mrList.length > 0 && !hasError) {
                if (IsComplete) {
                    if (!isValidYarnItems(mrList)) return false;
                }
                for (var i = 0; i < mrList.length; i++) {
                    if (menuParam == "MRPB") {
                        mrList[i].Childs.map(y => { y.IsPR = true; });
                    }
                    var childs = mrList[i].Childs;
                    if (IsComplete) {
                        if (childs.length == 0) {
                            toastr.error("At least 1 Yarn item is required (Cuff Information).");
                            hasError = true;
                            break;
                        }
                        if (childs.length > 0) {
                            var totalYarnDis = 0;
                            for (var j = 0; j < childs.length; j++) {
                                if (childs[j].Segment5ValueDesc) {
                                    if ((childs[j].Segment5ValueDesc.toLowerCase() == "melange" || childs[j].Segment5ValueDesc.toLowerCase() == "color melange") && (childs[j].ShadeCode == null || childs[j].ShadeCode == "")) {
                                        toastr.error("Select shade code for color melange");
                                        hasError = true;
                                        break;
                                    }
                                }
                                totalYarnDis += GetNumberValue(childs[j].Distribution);
                            }
                            if (hasError) break;
                            if (totalYarnDis != 100) {
                                toastr.error("Sum of Yarn Distribution must be 100 at Cuff Information. Current sum of Yarn Distribution is " + totalYarnDis);
                                hasError = true;
                                break;
                            }
                        }
                    }
                    mrList[i].IsComplete = IsComplete;
                    if (menuParam == "MRPB") {
                        mrList[i].CollarSizeID = $(pageIdWithHash).find("#CollarSizeID").val();
                        mrList[i].CuffSizeID = $(pageIdWithHash).find("#CuffSizeID").val();
                        mrList[i].CollarWeightInGm = $(pageIdWithHash).find("#CollarWeightInGm").val();
                        mrList[i].CuffWeightInGm = $(pageIdWithHash).find("#CuffWeightInGm").val();
                        mrList[i].AllCollarSizeList = masterData.AllCollarSizeList;
                        mrList[i].AllCuffSizeList = masterData.AllCuffSizeList;
                        mrList[i].CollarSizeList = masterData.CollarSizeList;
                        mrList[i].CuffSizeList = masterData.CuffSizeList;
                        mrList[i].MenuParam = menuParam;
                    }
                    mrListMain.push(mrList[i]);
                }
            }
        }
        if (hasError) return false;

        var hasIssue = false;
        var dyeingType = "";
        var prChildList = [];
        for (var i = 0; i < mrListMain.length; i++) {
            dyeingType = mrListMain[i].DyeingType;

            mrListMain[i].Childs.map(x => {
                x.DayValidDurationId = getDefaultValueWhenInvalidN(x.DayValidDurationId);
                if (x.IsPR && x.DayValidDurationId == 0) {
                    prChildList.push(x);
                }
                if (!x.IsPR) {
                    x.DayValidDurationId = 0;
                    x.DayDuration = 0;
                    x.DayValidDurationName = "Empty";
                }
            });

            if (isYarn(dyeingType)) {
                var childs = mrListMain[i].Childs;
                if (childs.length > 0) {
                    var childYD = childs.find(x => x.YD == true);
                    if (typeof childYD === "undefined" || childYD == null) {
                        hasIssue = true;
                        break;
                    }
                }
            }
        }

        if (IsComplete && prChildList.length > 0 && isBDS == 1 && masterData.IsCheckDVD) {
            var itemStr = prChildList.length > 1 ? "items" : "item";
            toastr.error(`Select yarn sourcing mode for PR item (${prChildList.length} ${itemStr} found).`);
            return false;
        }

        //if (IsComplete) {
        //    if (isValidChildForm(mrListMain)) return;
        //} 

        var data = mrListMain;
        $.each(data, function (i, obj) {
            obj.GroupConceptNo = $formEl.find('#BookingNo').val();
            obj.BookingID = $formEl.find('#BookingID').val();
            obj.IsComplete = IsComplete;
            obj.IsBDS = isBDS;
            obj.Modify = (status === statusConstants.PENDING) ? false : true
        });
        if (menuParam == "MRPB") {
            if (typeof mainData.CollarSizeID === "undefined" || mainData.CollarSizeID == null) mainData.CollarSizeID = "";
            if (typeof mainData.CollarWeightInGm === "undefined" || mainData.CollarWeightInGm == null || mainData.CollarWeightInGm == "") mainData.CollarWeightInGm = 0;

            if (masterData.HasCollar && (mainData.CollarSizeID.length == 0 || mainData.CollarWeightInGm == 0)) {
                toastr.error("Select collar size & give consumption (gm)");
                return;
            }

            if (typeof mainData.CuffSizeID === "undefined" || mainData.CuffSizeID == null) mainData.CuffSizeID = "";
            if (typeof mainData.CuffWeightInGm === "undefined" || mainData.CuffWeightInGm == null || mainData.CuffWeightInGm == "") mainData.CuffWeightInGm = 0;

            if (masterData.HasCuff && (mainData.CuffSizeID.length == 0 || mainData.CuffWeightInGm == 0)) {
                toastr.error("Select cuff size & give consumption (gm)");
                return;
            }

            //var list = data.FBookingChild.filter(x => x.SubGroupId == 1 && x.BookingQty == 0);
            //if (list.length > 0) {
            //    toastr.error("Fabric booking qty missing.");
            //    return;
            //}
            var list = masterData.OtherItems.filter(x => x.SubGroupID == 11 && x.TotalQtyInKG == 0);
            if (list.length > 0) {
                toastr.error("Give Req. knitting qty (KG) in collar.");
                return;
            }
            list = masterData.OtherItems.filter(x => x.SubGroupID == 12 && x.TotalQtyInKG == 0);
            if (list.length > 0) {
                toastr.error("Give Req. knitting qty (KG) in cuff.");
                return;
            }
        }
        if (hasIssue) {
            showBootboxConfirm("Save Record.", "Do you want to continue without selecting any YD of " + dyeingType + " ?", function (yes) {
                if (yes) {
                    axios.post("/api/mr-bds/save", data)
                        .then(function () {
                            toastr.success("Saved successfully.");
                            backToList();
                        })
                        .catch(function (error) {
                            toastr.error(error.response.data.Message);
                        });
                }
            });
        } else {
            axios.post("/api/mr-bds/save", data)
                .then(function () {
                    toastr.success("Saved successfully.");
                    backToList();
                })
                .catch(function (error) {
                    toastr.error(error.response.data.Message);
                });
        }
    }

    function isYarn(dyeingType) {
        if (dyeingType) {
            if (dyeingType.toUpperCase().indexOf('YARN') >= 0 && dyeingType.toUpperCase().indexOf('DYED') >= 0) return true;
        }
        return false;
    }

    function approve() {
        var url = `/api/mr-bds/remove-from-reject/${masterData.FCMRMasterID}`;
        axios.post(url)
            .then(function () {
                toastr.success(constants.ACKNOWLEDGE_SUCCESSFULLY);
                backToList();
            })
            .catch(showResponseError);
    }

    function saveComposition() {
        var totalPercent = sumOfArrayItem(compositionComponents, "Percent");
        if (totalPercent != 100) return toastr.error("Sum of compostion percent must be 100");
        compositionComponents.reverse();
        
        var composition = "";
        var blendTypeNames = [];
        var programTypeNames = [];

        compositionComponents = compositionComponents.sort((a, b) => b.Percent - a.Percent);
        compositionComponents.forEach(function (component) {
            composition += composition ? ` ${component.Percent}%` : `${component.Percent}%`;
            if (component.YarnSubProgramNew) {
                if (component.YarnSubProgramNew != 'N/A') {
                    composition += ` ${component.YarnSubProgramNew}`;
                }
            }
            //if (component.Certification) composition += ` ${component.Certification}`;
            if (component.Certification) {
                if (component.Certification != 'N/A') {
                    composition += ` ${component.Certification}`;
                }
            }
            composition += ` ${component.Fiber}`;

            component.FiberTypeName = getDefaultValueWhenInvalidS(component.FiberTypeName);
            if (component.FiberTypeName.length > 0) {
                blendTypeNames.push(component.FiberTypeName);
            }
            component.ProgramTypeName = getDefaultValueWhenInvalidS(component.ProgramTypeName);
            if (component.ProgramTypeName.length > 0) {
                programTypeNames.push(component.ProgramTypeName);
            }
        });

        blendTypeNames = [...new Set(blendTypeNames)];
        var blendTypeName = blendTypeNames.join(" + ");

        programTypeNames = [...new Set(programTypeNames)];
        var programTypeName = "Conventional";
        var indexF = programTypeNames.findIndex(x => x == "Sustainable");
        if (indexF > -1) {
            programTypeName = "Sustainable";
        }

        //var data = {
        //    SegmentValue: composition
        //};
        var data = {
            ItemSegmentValue: {
                SegmentValue: composition
            },
            BlendTypeName: blendTypeName,
            ProgramTypeName: programTypeName
        }

        axios.post("/api/rnd-free-concept-mr/save-yarn-composition", data)
            .then(function () {
                $pageEl.find(`#modal-new-composition-${pageId}`).modal("hide");
                toastr.success("Composition added successfully.");
                //masterData.CompositionList.unshift({ id: response.data.Id, text: response.data.SegmentValue });
                // initChildTable(masterData.Childs);
            })
            .catch(error => {
                if (error.response) {
                    toastr.error(error.response.data);
                } else {
                    toastr.error('Error message:', error.response.data.Message);
                }
                args.cancel = true;
            });

            //.catch(showResponseError)
    }
    /*
    function saveComposition() {
        
        var totalPercent = sumOfArrayItem(compositionComponents, "Percent");
        if (totalPercent != 100) return toastr.error("Sum of compostion percent must be 100");
        compositionComponents.reverse();

        var composition = "";
        //compositionComponents = _.sortBy(compositionComponents, "Percent").reverse();
        compositionComponents = compositionComponents.sort((a, b) => b.Percent - a.Percent);
        compositionComponents.forEach(function (component) {
            composition += composition ? ` ${component.Percent}%` : `${component.Percent}%`;
            if (component.YarnSubProgramNew) {
                if (component.YarnSubProgramNew != 'N/A') {
                    composition += ` ${component.YarnSubProgramNew}`;
                }
            }
            //if (component.Certification) composition += ` ${component.Certification}`;
            if (component.Certification) {
                if (component.Certification != 'N/A') {
                    composition += ` ${component.Certification}`;
                }
            }
            composition += ` ${component.Fiber}`;
        });

        var data = {
            SegmentValue: composition
        };

        axios.post("/api/rnd-free-concept-mr/save-yarn-composition", data)
            .then(function () {
                $pageEl.find(`#modal-new-composition-${pageId}`).modal("hide");
                toastr.success("Composition added successfully.");
                //masterData.CompositionList.unshift({ id: response.data.Id, text: response.data.SegmentValue });
                // initChildTable(masterData.Childs);
            })
            .catch(showResponseError)
    }
    */
    function GetNumberValue(value) {
        if (isNaN(value) || typeof value === "undefined" || value == null) return 0;
        return value;
    }

    function revise(IsComplete) {

        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);
        //var data = formDataToJson($formEl.serializeArray());
        var mrListMain = [];

        var hasError = false;
        if (isFabric == 1) {
            var mrList = [];
            $tblChildEl.getCurrentViewRecords().map(x => {
                mrList.push(x);
            });
            if (mrList.length > 0) {
                if (IsComplete) {
                    if (!isValidYarnItems(mrList)) return false;
                }
                for (var i = 0; i < mrList.length; i++) {
                    var childs = mrList[i].Childs;
                    if (IsComplete) {
                        if (childs.length == 0) {
                            toastr.error("At least 1 Yarn item is required (Yarn Information).");
                            hasError = true;
                            break;
                        }
                        if (childs.length > 0) {
                            var totalYarnDis = 0;
                            for (var j = 0; j < childs.length; j++) {
                                if (childs[j].Segment5ValueDesc) {
                                    if ((childs[j].Segment5ValueDesc.toLowerCase() == "melange" || childs[j].Segment5ValueDesc.toLowerCase() == "color melange") && (childs[j].ShadeCode == null || childs[j].ShadeCode == "")) {
                                        toastr.error("Select shade code for color melange");
                                        hasError = true;
                                        break;
                                    }
                                }
                                totalYarnDis += GetNumberValue(childs[j].Distribution);

                                if (menuParam == "MRBDS") {
                                    if (isBDS == 1 && childs[j].YarnStockSetId == 0 && !childs[j].YDItem && !childs[j].IsPR) {
                                        toastr.error(`Select YD Item or Go for PR for non-stock item at row ${j + 1}.`);
                                        hasError = true;
                                        break;
                                    }
                                }
                            }
                            if (hasError) break;
                            if (totalYarnDis != 100) {
                                toastr.error("Sum of Yarn Distribution must be 100 at Yarn Information. Current sum of Yarn Distribution is " + totalYarnDis);
                                hasError = true;
                                break;
                            }
                        }
                    }
                    mrList[i].IsComplete = IsComplete;
                    mrListMain.push(mrList[i]);
                }
            }
        }
        if (isCollar == 1 && !hasError) {
            var mrList = [];
            $tblOtherItemEl.getCurrentViewRecords().map(x => {
                mrList.push(x);
            });
            if (mrList.length > 0 && !hasError) {
                if (IsComplete) {
                    if (!isValidYarnItems(mrList)) return false;
                }
                for (var i = 0; i < mrList.length; i++) {
                    var childs = mrList[i].Childs;
                    if (IsComplete) {
                        if (childs.length == 0) {
                            toastr.error("At least 1 Yarn item is required (Collar Information).");
                            hasError = true;
                            break;
                        }
                        if (childs.length > 0) {
                            var totalYarnDis = 0;
                            for (var j = 0; j < childs.length; j++) {
                                if (childs[j].Segment5ValueDesc) {
                                    if ((childs[j].Segment5ValueDesc.toLowerCase() == "melange" || childs[j].Segment5ValueDesc.toLowerCase() == "color melange") && (childs[j].ShadeCode == null || childs[j].ShadeCode == "")) {
                                        toastr.error("Select shade code for color melange");
                                        hasError = true;
                                        break;
                                    }
                                }
                                totalYarnDis += GetNumberValue(childs[j].Distribution);
                            }
                            if (hasError) break;
                            if (totalYarnDis != 100) {
                                toastr.error("Sum of Yarn Distribution must be 100 at Collar Information. Current sum of Yarn Distribution is " + totalYarnDis);
                                hasError = true;
                                break;
                            }
                        }
                    }
                    mrList[i].IsComplete = IsComplete;
                    if (menuParam == "MRPB") {

                        mrList[i].CollarSizeID = $(pageIdWithHash).find("#CollarSizeID").val();
                        mrList[i].CuffSizeID = $(pageIdWithHash).find("#CuffSizeID").val();
                        mrList[i].CollarWeightInGm = $(pageIdWithHash).find("#CollarWeightInGm").val();
                        mrList[i].CuffWeightInGm = $(pageIdWithHash).find("#CuffWeightInGm").val();
                        mrList[i].AllCollarSizeList = masterData.AllCollarSizeList;
                        mrList[i].AllCuffSizeList = masterData.AllCuffSizeList;
                        mrList[i].CollarSizeList = masterData.CollarSizeList;
                        mrList[i].CuffSizeList = masterData.CuffSizeList;
                        mrList[i].MenuParam = menuParam;
                    }
                    mrListMain.push(mrList[i]);
                }
            }
        }
        if (isCuff == 1 && !hasError) {
            var mrList = [];
            $tblCuffItemEl.getCurrentViewRecords().map(x => {
                mrList.push(x);
            });
            if (IsComplete) {
                if (!isValidYarnItems(mrList)) return false;
            }
            if (mrList.length > 0 && !hasError) {
                for (var i = 0; i < mrList.length; i++) {
                    var childs = mrList[i].Childs;
                    if (IsComplete) {
                        if (childs.length == 0) {
                            toastr.error("At least 1 Yarn item is required (Cuff Information).");
                            hasError = true;
                            break;
                        }
                        if (childs.length > 0) {
                            var totalYarnDis = 0;
                            for (var j = 0; j < childs.length; j++) {
                                if (childs[j].Segment5ValueDesc) {
                                    if ((childs[j].Segment5ValueDesc.toLowerCase() == "melange" || childs[j].Segment5ValueDesc.toLowerCase() == "color melange") && (childs[j].ShadeCode == null || childs[j].ShadeCode == "")) {
                                        toastr.error("Select shade code for color melange");
                                        hasError = true;
                                        break;
                                    }
                                }
                                totalYarnDis += GetNumberValue(childs[j].Distribution);
                            }
                            if (hasError) break;
                            if (totalYarnDis != 100) {
                                toastr.error("Sum of Yarn Distribution must be 100 at Cuff Information. Current sum of Yarn Distribution is " + totalYarnDis);
                                hasError = true;
                                break;
                            }
                        }
                    }
                    mrList[i].IsComplete = IsComplete;
                    if (menuParam == "MRPB") {
                        mrList[i].CollarSizeID = $(pageIdWithHash).find("#CollarSizeID").val();
                        mrList[i].CuffSizeID = $(pageIdWithHash).find("#CuffSizeID").val();
                        mrList[i].CollarWeightInGm = $(pageIdWithHash).find("#CollarWeightInGm").val();
                        mrList[i].CuffWeightInGm = $(pageIdWithHash).find("#CuffWeightInGm").val();
                        mrList[i].AllCollarSizeList = masterData.AllCollarSizeList;
                        mrList[i].AllCuffSizeList = masterData.AllCuffSizeList;
                        mrList[i].CollarSizeList = masterData.CollarSizeList;
                        mrList[i].CuffSizeList = masterData.CuffSizeList;
                        mrList[i].MenuParam = menuParam;
                    }
                    mrListMain.push(mrList[i]);
                }
            }
        }
        if (hasError) return false;

        var hasIssue = false;
        var dyeingType = "";
        var prChildList = [];
        for (var i = 0; i < mrListMain.length; i++) {
            dyeingType = mrListMain[i].DyeingType;

            mrListMain[i].Childs.map(x => {
                x.DayValidDurationId = getDefaultValueWhenInvalidN(x.DayValidDurationId);
                if (x.IsPR && x.DayValidDurationId == 0) {
                    prChildList.push(x);
                }
            });

            if (isYarn(dyeingType)) {
                var childs = mrListMain[i].Childs;
                if (childs.length > 0) {
                    var childYD = childs.find(x => x.YD == true);
                    if (typeof childYD === "undefined" || childYD == null) {
                        hasIssue = true;
                        break;
                    }
                }
            }
        }

        if (IsComplete && prChildList.length > 0 && isBDS == 1 && masterData.IsCheckDVD) {
            var itemStr = prChildList.length > 1 ? "items" : "item";
            toastr.error(`Select yarn sourcing mode for PR item (${prChildList.length} ${itemStr} found).`);
            return false;
        }

        //Load all data
        var data = mrListMain;


        if (hasIssue) {
            showBootboxConfirm("Save Record.", "Do you want to continue without selecting any YD of " + dyeingType + " ?", function (yes) {
                if (yes) {

                    $.each(data, function (i, obj) {
                        obj.GroupConceptNo = $formEl.find('#BookingNo').val();
                        obj.BookingID = $formEl.find('#BookingID').val();
                        obj.IsComplete = IsComplete;
                        obj.Modify = (status === statusConstants.PENDING) ? false : true;
                    });

                    axios.post("/api/mr-bds/revise", data)
                        .then(function () {
                            toastr.success("Revised successfully.");
                            backToList();
                        })
                        .catch(function (error) {
                            toastr.error(error.response.data.Message);
                        });
                }
            });
        } else {

            var isOwnRevise = false;
            if (status == statusConstants.COMPLETED) {
                isOwnRevise = true;
            }

            $.each(data, function (i, obj) {
                obj.GroupConceptNo = $formEl.find('#BookingNo').val();
                obj.BookingID = $formEl.find('#BookingID').val();
                obj.IsComplete = IsComplete;
                obj.Modify = (status === statusConstants.PENDING) ? false : true;
                obj.IsOwnRevise = isOwnRevise;
            });

            axios.post("/api/mr-bds/revise", data)
                .then(function () {
                    toastr.success("Revised successfully.");
                    backToList();
                })
                .catch(function (error) {
                    toastr.error(error.response.data.Message);
                });
        }
    }

    function resetGlobals() {
    }
    function isDeepEqual(obj1, obj2) {
        if (obj1 === obj2) {
            return true;
        }

        if (typeof obj1 !== 'object' || obj1 === null || typeof obj2 !== 'object' || obj2 === null) {
            return false;
        }

        const keys1 = Object.keys(obj1);
        const keys2 = Object.keys(obj2);

        if (keys1.length !== keys2.length) {
            return false;
        }

        for (let key of keys1) {
            if (!keys2.includes(key) || !isDeepEqual(obj1[key], obj2[key])) {
                return false;
            }
        }

        return true;
    }
    function getDayValidDurationInfos(obj, parentObj, subGroupId) {
        parentObj.DayValidDurationId = getDefaultValueWhenInvalidN(parentObj.DayValidDurationId);
        parentObj.DayDuration = getDefaultValueWhenInvalidN(parentObj.DayDuration);
        parentObj.DayValidDurationName = getDefaultValueWhenInvalidS(parentObj.DayValidDurationName);

        obj.DayValidDurationId = getDefaultValueWhenInvalidN(obj.DayValidDurationId);

        if (obj.DayValidDurationId == 0) {
            obj.DayValidDurationId = parentObj.DayValidDurationId;
            obj.DayDuration = parentObj.DayDuration;
            obj.DayValidDurationName = parentObj.DayValidDurationName;
        }

        if (isBDS == 3 && parentObj.DayValidDurationName.includes("Local") && obj.DayValidDurationId > 0 && obj.DayValidDurationId != parentObj.DayValidDurationId) {
            obj.DayValidDurationId = parentObj.DayValidDurationId;
            obj.DayDuration = parentObj.DayDuration;
            obj.DayValidDurationName = parentObj.DayValidDurationName;

            var subGroupName = "fabric";
            if (subGroupId == 11) subGroupName = "collar";
            else if (subGroupId == 12) subGroupName = "cuff";
            toastr.error(`You can not select import for ${subGroupName} sourcing mode local`);
        }

        if (!obj.IsPR && isBDS == 1) {
            obj.DayValidDurationId = 0;
            obj.DayDuration = 0;
            obj.DayValidDurationName = "Empty";
        }

        return obj;
    }
    function isDayValidDurationLocal(dayValidDurationName) {
        return dayValidDurationName.toLowerCase().includes("local");
    }
})();
(function () {
    var menuId, pageName;
    var pageId;
    var toolbarId, _oRow, _index, _modalFrom, _oRowCollar, _indexCollar, _oRowCuff, _indexCuff;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $tblChildElDI, $tblChildYarnEl, $formEl, tblMasterId, tblChildId, tblChildIdDI, tblChildYarnId, tblChildCollarId, tblChildCollarIdDI,
        $tblChildCollarIdEl, $tblChildCollarIdElDI
        , tblChildCuffId, tblChildCuffIdDI, $tblChildCuffIdEl, $tblChildCuffIdElDI, tblFabricChildId, tblCollarChildId, tblCuffChildId, tblPlanningId, $tblPlanningEl, $modalPlanningEl
        , tblCriteriaId, $tblCriteriaIdEl, $modalCriteriaEl, _indexc, _oRowc, ids;
    var menuType = 0;
    var idsList = [];
    var status = statusConstants.NEW;
    var CriteriaName;
    var _machineTypeId = 0;
    var _isBDS = 1;
    var masterData;
    var bmtArray = [];
    var itemTNAInfo = null, itemTNAInfoCollar = null; //itemTNAInfoCuff = null;
    var _yarnLiabilitiesItem = null;
    var _yarnLiabilitiesChildID = 99999;
    var _previousBUChilds = [];
    var _selectedSubGroupId = -1;
    var _liabilitiesType = {
        DyedYarn: "Dyed Yarn",
        FinishedQty: "Finished Qty",
        GreyQty: "Grey Qty",
        YarnQty: "Yarn Qty"
    };

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
        tblChildIdDI = "#tblChildDI" + pageId;

        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        tblChildCollarId = "#tblChildCollarId" + pageId;
        tblChildCollarIdDI = "#tblChildCollarIdDI" + pageId;

        tblChildCuffId = "#tblChildCuffId" + pageId;
        tblChildCuffIdDI = "#tblChildCuffIdDI" + pageId;

        tblYarnChildId = "#tblYarnChild" + pageId;
        tblPlanningId = "#tblPlanning" + pageId;
        $modalPlanningEl = $("#modalPlanning" + pageId);
        tblCriteriaId = "#tblCriteria" + pageId;
        $modalCriteriaEl = $("#modalCriteria" + pageId);
        itemDetailsId = $("#itemDetailsId" + pageId);
        menuType = localStorage.getItem("bulkBookingAckPage");


        if (menuType == 2) {
            status = statusConstants.PENDING;
        }
        //var isBulkBookingAckPage = bulkBookingAckPage == 1 ? true : false;

        if (menuType == 1) {
            $formEl.find("#lblTableTitle").text("Booking Consumption");

            _isBDS = 2;
            $toolbarEl.find("#btnRcvList,#btnList,#btnCancelList,#btnAcknowledgedList,#btnDeliveredList,#btnUnAcknowledgedList").hide();
            $toolbarEl.find("#btnReceive,#btnUnAcknowledge,#btnReceived").hide();
            $toolbarEl.find("#btnReceive,#btnUnAcknowledge,#btnReceived").hide();

            $toolbarEl.find("#btnPendingList").click(function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.PENDING;
                initBulkAckList();
            });
            $toolbarEl.find("#btnBookingList").click(function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.ACTIVE;
                initBulkAckList();
            });

            $formEl.find("#btnCancel").on("click", backToListBulk);

            status = statusConstants.PENDING;
            initBulkAckList();
            $toolbarEl.find("#btnList").click();
        } else if (menuType == 0) {

            $formEl.find("#lblTableTitle").text("Fabric Booking Consumption");
            _isBDS = 1;
            $toolbarEl.find("#btnPendingList,#btnBookingList").hide();

            $toolbarEl.find("#btnList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.NEW;
                initMasterTable();
                $formEl.find("#divYarnInfo").hide();
            });
            $toolbarEl.find("#btnRevisionAckList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.REVISE;
                initMasterTable();
                $formEl.find("#divYarnInfo").show();
            });
            $toolbarEl.find("#btnMktAckList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.PROPOSED_FOR_ACKNOWLEDGE;
                initMasterTable();
                $formEl.find("#divYarnInfo").show();
            });
            $toolbarEl.find("#btnUnAcknowledgedList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.UN_ACKNOWLEDGE;
                initMasterTable();
                $formEl.find("#divYarnInfo").show();
            });
            $toolbarEl.find("#btnCancelList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.REJECT;
                initMasterTable();
            });
            $toolbarEl.find("#btnAllList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.ALL;
                initMasterTable();
            });
            $toolbarEl.find("#btnRcvList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.ACTIVE;
                initMasterTable();
            });
            $toolbarEl.find("#btnAcknowledgedList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.COMPLETED;
                initMasterTable();
            });

            $toolbarEl.find("#btnDeliveredList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.APPROVED;
                initMasterTable();
            });

            $formEl.find("#btnCancel").on("click", backToList);

            $formEl.find("#btnReceive").click(function (e) {
                e.preventDefault();
                Receive(this);
            });
            $formEl.find("#btnReceived").click(function (e) {
                e.preventDefault();
                Received(this);
            });
            $formEl.find("#btnUnAcknowledge").click(function (e) {
                bootbox.prompt("Enter your UnAcknowledge reason:", function (result) {
                    if (!result) {
                        return toastr.error("UnAcknowledge reason is required.");
                    }
                    saveWithConfirm(result, true);
                });
            });

            $formEl.find("#btnCancelAcknowledge").click(function (e) {
                e.preventDefault();
                cancelSave();
            });

            $formEl.find("#btnCancelUnAcknowledge").click(function (e) {
                bootbox.prompt("Enter your UnAcknowledge reason:", function (result) {
                    if (!result) {
                        return toastr.error("UnAcknowledge reason is required.");
                    }
                    cancelSave(result);
                });
            });
            $toolbarEl.find("#btnList").click();
        } else if (menuType == 2) {
            $toolbarEl.find("#btnRcvList").fadeOut();
            status = statusConstants.PENDING;

            $formEl.find("#lblTableTitle").text("Sample Booking Consumption");
            _isBDS = 3;
            $toolbarEl.find("#btnPendingList,#btnBookingList").hide();

            $toolbarEl.find("#btnList").on("click", function (e) {

                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.PENDING;
                initMasterTable();
            });
            $toolbarEl.find("#btnRevisionAckList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.REVISE;
                initMasterTable();
            });
            $toolbarEl.find("#btnCancelList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.REJECT;
                initMasterTable();
            });
            $toolbarEl.find("#btnAcknowledgedList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.COMPLETED;
                initMasterTable();
            });
            $toolbarEl.find("#btnUnAcknowledgedList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.UN_ACKNOWLEDGE;
                initMasterTable();
            });

            $toolbarEl.find("#btnDeliveredList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.APPROVED;
                initMasterTable();
            });

            $formEl.find("#btnCancel").on("click", backToList);

            $formEl.find("#btnReceive").click(function (e) {
                e.preventDefault();
                Receive(this);
            });
            $formEl.find("#btnReceived").click(function (e) {
                e.preventDefault();
                Received(this);
            });
            $formEl.find("#btnUnAcknowledge").click(function (e) {
                bootbox.prompt("Enter your UnAcknowledge reason:", function (result) {
                    if (!result) {
                        return toastr.error("UnAcknowledge reason is required.");
                    }
                    saveWithConfirm(result, true);
                });
            });

            $formEl.find("#btnCancelAcknowledge").click(function (e) {
                e.preventDefault();
                cancelSave();
            });

            $formEl.find("#btnCancelUnAcknowledge").click(function (e) {
                bootbox.prompt("Enter your UnAcknowledge reason:", function (result) {
                    if (!result) {
                        return toastr.error("UnAcknowledge reason is required.");
                    }
                    cancelSave(result);
                });
            });

            $toolbarEl.find("#btnList").click();
        }

        $toolbarEl.find("#btnRefreshList").on("click", function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            saveWithConfirm("", false);
        });

        $formEl.find("#btnOk").click(function (e) {
            e.preventDefault();
            setLiabilitiesData(_oRow, $tblPlanningEl, _indexc);
        });

        $formEl.find("#btnOkk").click(function (e) {
            e.preventDefault();

            var selectedRows = $tblCriteriaIdEl.getCurrentViewRecords();
            for (var i = 0; i < selectedRows.length; i++) {
                if ((selectedRows[i].CriteriaName == "Material" || selectedRows[i].CriteriaName == "Dyeing" || selectedRows[i].CriteriaName == "Finishing" || selectedRows[i].CriteriaName == "Testing") && selectedRows[i].TotalTime == 0) {
                    toastr.error("Please enter criteria for Material, Dyeing, Finishing, Testing!");
                    return;
                }
            }

            if (_modalFrom == subGroupNames.FABRIC) {
                setPlanningCriteriaData(_oRow, $tblChildEl, _index);
            } else if (_modalFrom == subGroupNames.COLLAR) {
                setPlanningCriteriaData(_oRowCollar, $tblChildCollarIdEl, _indexCollar);
            } else if (_modalFrom == subGroupNames.CUFF) {
                setPlanningCriteriaData(_oRowCuff, $tblChildCuffIdEl, _indexCuff);
            }
        });

        //GreyQCDefectHKs
        axios.get(`/api/fabric-con-sub-class-tech-name/list`)
            .then(function (response) {
                bmtArray = response.data;
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });

        loadListCountMethod({
            ToolbarId: toolbarId,
            URL: `/api/fab-acknowledge/bulk/fabric-booking-acknowledge/get-list-count`,
            CountTagProps: `NewCount,Revision,Pending2,Acknowledged,UnAcknowledged,Cancel,AllCount`,
            IsDefaultAllCount: false
        });
    });

    function getValidFinishQty(selectedRow) {
        var subGroupId = parseInt(itemDetailsId.attr("SubGroupId"));
        var maxFinishQtyLiability = getDefaultValueWhenInvalidN_Float(selectedRow.MaxFinishQtyLiability);

        var ffsqLId = "lbl12Value";
        if (subGroupId != 1) ffsqLId = "lbl11Value";

        var liabilityQty = selectedRow.LiabilityQty;
        var consumedQty = selectedRow.ConsumedQty;

        if (liabilityQty > maxFinishQtyLiability) {
            toastr.error(`Finish Qty ${liabilityQty} cannot be greater than Max Qty ${maxFinishQtyLiability}`);
            return {
                IsValid: false,
                LiabilityQty: liabilityQty,
                ConsumedQty: consumedQty,
                TillLiabilityQty: liabilityQty - consumedQty
            };
        }

        /*
        var diffQty = getDefaultValueWhenInvalidN_Float($("#lbl9Value").text());
        if (diffQty < 0) diffQty = diffQty * (-1);
        var finishFabricStockQty = getDefaultValueWhenInvalidN_Float($("#" + ffsqLId).text());

        var maxQty = Math.min(diffQty, finishFabricStockQty);
        if (liabilityQty > maxQty) {
            if (maxQty == finishFabricStockQty) {
                toastr.error(`Finish Qty ${liabilityQty} cannot be greater than Finish Fabric Stock Qty ${finishFabricStockQty}`);
            } else if (maxQty == diffQty) {
                toastr.error(`Finish Qty ${liabilityQty} cannot be greater than Different Qty ${diffQty}`);
            } else {
                toastr.error(`Invalid Finish Qty ${liabilityQty}`);
            }
            liabilityQty = maxQty;
            return {
                IsValid: false,
                LiabilityQty: liabilityQty,
                ConsumedQty: consumedQty,
                TillLiabilityQty: liabilityQty - consumedQty
            };
        }
        */
        return {
            IsValid: true,
            LiabilityQty: liabilityQty,
            ConsumedQty: consumedQty,
            TillLiabilityQty: liabilityQty - consumedQty
        };
    }

    function setLiabilitiesData(oRowc, $tblElq, ind) {
        var selectedRows = $tblPlanningEl.getCurrentViewRecords();
        var selectedYarnRows = $tblChildYarnEl.getCurrentViewRecords();

        if (oRowc.SubGroupID == 1) {
            var indexF = masterData.FBookingChild.findIndex(x => x.BookingID == oRowc.BookingID && x.ConsumptionID == oRowc.ConsumptionID && x.SubGroupID == oRowc.SubGroupID)
            if (indexF > -1) {
                masterData.FBookingChild[indexF].ChildAckLiabilityDetails = selectedRows;
                masterData.FBookingChild[indexF].ChildAckYarnLiabilityDetails = selectedYarnRows;
            }
        }
        else if (oRowc.SubGroupID == 11) {
            var indexF = masterData.FBookingChildCollor.findIndex(x => x.BookingID == oRowc.BookingID && x.ConsumptionID == oRowc.ConsumptionID && x.SubGroupID == oRowc.SubGroupID)
            if (indexF > -1) {
                masterData.FBookingChildCollor[indexF].ChildAckLiabilityDetails = selectedRows;
                masterData.FBookingChildCollor[indexF].ChildAckYarnLiabilityDetails = selectedYarnRows;
            }
        }
        else if (oRowc.SubGroupID == 12) {
            var indexF = masterData.FBookingChildCuff.findIndex(x => x.BookingID == oRowc.BookingID && x.ConsumptionID == oRowc.ConsumptionID && x.SubGroupID == oRowc.SubGroupID)
            if (indexF > -1) {
                masterData.FBookingChildCuff[indexF].ChildAckLiabilityDetails = selectedRows;
                masterData.FBookingChildCuff[indexF].ChildAckYarnLiabilityDetails = selectedYarnRows;
            }
        }
        if (selectedRows.length > 0) {
            var indexF = selectedRows.findIndex(x => x.LiabilitiesProcessID == 1715);

            var objValidation = getValidFinishQty(selectedRows[indexF]);
            if (!objValidation.IsValid) {

                var indexF = selectedRows.findIndex(x => x.LiabilitiesProcessID == 1715);
                selectedRows[indexF].LiabilityQty = objValidation.LiabilityQty;
                selectedRows[indexF].ConsumedQty = objValidation.ConsumedQty;
                selectedRows[indexF].TillLiabilityQty = objValidation.TillLiabilityQty;
                $tblPlanningEl.updateRow(indexF, selectedRows[indexF]);
                return false;
            }
        }

        if (selectedRows.length == 0) {
            toastr.warning("Please entry liabilities qty(s)!");
            return;
        }
        var totalLiabilitiesQty = 0;
        selectedRows.map(x => {
            totalLiabilitiesQty += x.LiabilityQty;
        });
        //selectedYarnRows.map(x => {
        //    totalLiabilitiesQty += x.LiabilityQty;
        //});
        totalLiabilitiesQty = totalLiabilitiesQty.toFixed(2);
        totalLiabilitiesQty = parseFloat(totalLiabilitiesQty);


        oRowc.LiabilitiesBookingQty = totalLiabilitiesQty;
        oRowc.ActualBookingQty = oRowc.BookingQty + oRowc.LiabilitiesBookingQty;
        oRowc.ChildAckYarnLiabilityDetails = selectedYarnRows;

        if ($tblChildEl != undefined && _modalFrom == subGroupNames.FABRIC) {
            $tblChildEl.refresh();
        }
        else if ($tblChildCollarIdEl != undefined && _modalFrom == subGroupNames.COLLAR) {
            $tblChildCollarIdEl.refresh();
        }
        else if ($tblChildCuffIdEl != undefined && _modalFrom == subGroupNames.CUFF) {
            $tblChildCuffIdEl.refresh();
        }

        if ($tblChildElDI != undefined && _modalFrom == subGroupNames.FABRIC) {
            $tblChildElDI.refresh();
        }
        else if ($tblChildCollarIdElDI != undefined && _modalFrom == subGroupNames.COLLAR) {
            $tblChildCollarIdElDI.refresh();
        }
        else if ($tblChildCuffIdElDI != undefined && _modalFrom == subGroupNames.CUFF) {
            $tblChildCuffIdElDI.refresh();
        }

        $modalPlanningEl.modal('toggle');
    }
    function setPlanningData(oRowc, $tblElq, ind) {
        //console.log(oRowc);
        var selectedRows = $tblPlanningEl.getSelectedRecords();

        if (selectedRows.length == 0 && (oRowc.CriteriaName == "Material" || oRowc.CriteriaName == "Dyeing" || oRowc.CriteriaName == "Finishing" || oRowc.CriteriaName == "Testing")) {
            toastr.warning("Please select item(s)!");
            return;
        }
        ids = oRowc.CriteriaIDs = selectedRows.map(function (el) { return el.CriteriaID }).toString();
        idsList.push(oRowc.CriteriaIDs);
        let TotalTime = oRowc.TechnicalTime + (oRowc.IsSubContact ? 14 : 0);
        let FinishingTime = oRowc.TechnicalTime + (oRowc.IsSubContact ? 14 : 0);
        let DyeingTime = oRowc.TechnicalTime + (oRowc.IsSubContact ? 14 : 0);
        let KnittingTime = oRowc.TechnicalTime + (oRowc.IsSubContact ? 14 : 0);
        let MaterialTime = 0;
        let TestReportDaysTime = oRowc.TechnicalTime + (oRowc.IsSubContact ? 14 : 0);
        let PreprocessTime = oRowc.TechnicalTime + (oRowc.IsSubContact ? 14 : 0);
        let batchPreparationTime = oRowc.TechnicalTime + (oRowc.IsSubContact ? 14 : 0);
        let TestingTime = oRowc.TechnicalTime + (oRowc.IsSubContact ? 14 : 0);
        selectedRows.forEach(function (row) {
            //console.log(row);
            if (row.CriteriaName == "Dyeing") {
                DyeingTime += row.ProcessTime;
            }
            else if (row.CriteriaName == "Finishing") {
                FinishingTime += row.ProcessTime;
            }
            else if (row.CriteriaName == "Material") {
                MaterialTime += row.ProcessTime;
            }
            else if (row.CriteriaName == "Preprocess") {
                PreprocessTime += row.ProcessTime;
            }
            else if (row.CriteriaName == "Testing") {
                TestingTime += row.ProcessTime;
            }
            TotalTime += row.ProcessTime;
        });
        //selectedRows.CriteriaName(function (row) {
        //    console.log(row);
        //    TotalTime += row.ProcessTime;
        //});
        oRowc.TotalTime = TotalTime;
        oRowc.FinishingTime = parseInt(FinishingTime + MaterialTime + oRowc.StructureDays + PreprocessTime + DyeingTime);
        oRowc.DyeingTime = parseInt(DyeingTime + MaterialTime + oRowc.StructureDays + PreprocessTime);
        oRowc.batchPreparationTime = parseInt(MaterialTime + oRowc.StructureDays + PreprocessTime);
        //oRowc.KnittingDays = TotalTime;
        oRowc.MaterialTime = MaterialTime;
        oRowc.PreprocessTime = PreprocessTime;
        oRowc.TestingTime = TestingTime;
        oRowc.KnittingTime = parseInt(MaterialTime + oRowc.StructureDays);
        oRowc.TestReportTime = parseInt(oRowc.StructureDays + MaterialTime + PreprocessTime + DyeingTime + FinishingTime + TestingTime);
        $tblElq.updateRow(ind, oRowc);
        $modalPlanningEl.modal('toggle');
    }

    function setPlanningCriteriaData(oRow, $tblEl, ind) {
        ids = idsList.join(",");
        var selectedRows = $tblCriteriaIdEl.getCurrentViewRecords();
        oRow.CriteriaIDs = ids;

        oRow.TechnicalTime = setTechnicalTime(oRow);
        oRow = setArgDataValues(oRow);

        /*
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
        //if (argsRowData.IsSubContact) subcontactDays = 14;
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
         */

        $tblEl.updateRow(ind, oRow);
        $modalCriteriaEl.modal('toggle');
    }
    /*
     function setPlanningCriteriaData(oRow, $tblEl, ind) {
        ids = idsList.join(",");
        var selectedRows = $tblCriteriaIdEl.getCurrentViewRecords();
        oRow.CriteriaIDs = ids;

        let totalDays = 0;//oRow.TechnicalTime + (oRow.IsSubContact ? 14 : 0);
        let finishingDays = oRow.TechnicalTime + (oRow.IsSubContact ? 14 : 0);
        let dyeingDays = oRow.TechnicalTime + (oRow.IsSubContact ? 14 : 0);
        let batchPreparationDays = oRow.TechnicalTime + (oRow.IsSubContact ? 14 : 0);
        //let knittingDays = oRow.TechnicalTime + (oRow.IsSubContact ? 14 : 0);
        let materialDays = 0;
        let KnittingDays = oRow.TechnicalTime + (oRow.IsSubContact ? 14 : 0);
        let testReportDays = oRow.TechnicalTime + (oRow.IsSubContact ? 14 : 0);
        // let testReportDays = oRow.TechnicalTime + (oRow.IsSubContact ? 14 : 0);
        let QualityDays = oRow.TechnicalTime + (oRow.IsSubContact ? 14 : 0);
        selectedRows.forEach(function (row) {
            totalDays += row.TotalTime;
            finishingDays += row.FinishingTime;
            dyeingDays += row.DyeingTime;
            batchPreparationDays += row.batchPreparationTime;
            //batchPreparationDays += row.batchPreparationTime;
            KnittingDays += row.KnittingTime;
            materialDays += row.MaterialTime;
            testReportDays += row.TestReportTime;
            // QualityDays += 1;
        });
        var currentData = setTotalDaysAndDeliveryDate(oRow, []);

        oRow.TotalDays = totalDays + currentData.TotalDays;
        oRow.FinishingDays = finishingDays + 1;
        //alert(oRow.FinishingDays);
        oRow.DyeingDays = dyeingDays + 1
        oRow.BatchPreparationDays = batchPreparationDays + 1;
        oRow.KnittingDays = KnittingDays;
        oRow.TestReportDays = testReportDays + 1;
        //alert(oRow.KnittingDays);
        //alert(oRow.TestReportDays);
        //oRow.KnittingDays = KnittingDays;
        oRow.MaterialDays = materialDays;
        oRow.QualityDays = 1;
        // alert(oRow.MaterialDays);
        var dt = new Date();
        dt.setDate(dt.getDate() + oRow.TotalDays);
        oRow.DeliveryDate = dt;

        $tblEl.updateRow(ind, oRow);
        $modalCriteriaEl.modal('toggle');
    }
     */
    function initBulkAckList() {
        var columns = [
            {
                headerText: 'Command', width: 100, textAlign: 'Left', visible: (status == statusConstants.PENDING || status == statusConstants.REJECT), commands: [
                    { type: 'AddBulk', title: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }
                ]
            },
            {
                headerText: 'Command', width: 100, textAlign: 'Left', visible: status == statusConstants.ACTIVE || status == statusConstants.UN_ACKNOWLEDGE || status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE, commands: [
                    { type: 'View', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }
                ]
            },
            {
                field: 'BookingNo', headerText: 'Booking No'
            },
            {
                field: 'SLNo', headerText: 'SL No', visible: false
            },
            {
                field: 'BookingDate', headerText: 'Booking Date', type: 'date', format: _ch_date_format_7
            },
            {
                field: 'BuyerName', headerText: 'Buyer'
            },
            {
                field: 'BuyerTeamName', headerText: 'Buyer Team'
            },
            {
                field: 'CompanyName', headerText: 'Company'
            },
            {
                field: 'StyleNo', headerText: 'Style No', visible: status == statusConstants.ACTIVE
            },
            {
                field: 'SupplierName', headerText: 'Supplier Name', visible: status == statusConstants.ACTIVE
            },
            {
                field: 'SeasonName', headerText: 'Season Name', visible: status == statusConstants.ACTIVE
            },
            {
                field: 'Remarks', headerText: 'Remarks'
            }
        ];
        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/bds-acknowledge/bulk/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands,
            queryCellInfo: cellModifyForBDSAck
        });
    }
    function initMasterTable() {
        var columns = [
            {
                headerText: 'Command', width: 150, textAlign: 'Left', visible: status == statusConstants.NEW, commands: [
                    { type: 'Add', title: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                    { type: 'Tech Pack Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } },
                    { type: 'Booking Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } }
                ]
            },
            {
                headerText: 'Command', width: 150, textAlign: 'Left', visible: (status == statusConstants.REVISE || status == statusConstants.PENDING || status == statusConstants.REJECT), commands: [
                    { type: 'Edit', title: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                    { type: 'Tech Pack Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } },
                    { type: 'Booking Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } }
                ]
            },
            {
                headerText: 'Command', width: 180, textAlign: 'Left', visible: (status == statusConstants.COMPLETED || status == statusConstants.UN_ACKNOWLEDGE || status == statusConstants.ALL), commands: [
                    { type: 'View', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                    { type: 'Tech Pack Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } },
                    { type: 'Booking Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } },
                    { type: 'Email', title: 'Send Email', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-envelope-o' } }
                ]
            },
            {
                headerText: 'Command', width: 180, textAlign: 'Left', visible: status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE, commands: [
                    { type: 'View', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                    { type: 'Tech Pack Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } },
                    { type: 'Booking Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } },
                    { type: 'Email', title: 'Send Email', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-envelope-o' } }
                ]
            },
            {
                headerText: 'Command', width: 150, textAlign: 'Left', visible: (status == statusConstants.APPROVED), commands: [
                    { type: 'ViewRecive', title: 'ViewRecive', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                    { type: 'Tech Pack Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } },
                    { type: 'Booking Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } }
                ]
            },
            {
                field: 'PendingRevision', headerText: 'Status', visible: status == 2
            },
            {
                field: 'BookingNo', headerText: 'Booking No'
            },
            {
                field: 'ExportOrderNo', headerText: 'Export Order No'
            },
            {
                field: 'BuyerName', headerText: 'Buyer'
            },
            {
                field: 'BuyerTeamName', headerText: 'Buyer Team'
            },
            {
                field: 'RevNoValue', headerText: 'Revision No', visible: status != statusConstants.NEW && status != statusConstants.ALL
            },
            {
                field: 'EmployeeName', headerText: 'Merchant Name'
            },
            {
                field: 'BKAcknowledgeDate', headerText: 'Booking Date', format: _ch_date_format_7
            },
            {
                field: 'SubGroupID', headerText: 'Sub Group ID', visible: false
            },
            {
                field: 'AcknowledgeByName', headerText: 'Ack By', visible: status == statusConstants.COMPLETED
            },
            {
                field: 'AcknowledgeDate', headerText: 'Ack Date', visible: status == statusConstants.COMPLETED, type: 'date', format: _ch_date_format_7
            },
            {
                field: 'UnAcknowledgeByName', headerText: 'UnAck By', visible: status == statusConstants.UN_ACKNOWLEDGE
            },
            {
                field: 'UnAcknowledgeDate', headerText: 'UnAck Date', visible: status == statusConstants.UN_ACKNOWLEDGE, type: 'date', format: _ch_date_format_7
            },
            {
                field: 'UnAcknowledgeReason', headerText: 'UnAck Reason', visible: status == statusConstants.UN_ACKNOWLEDGE
            },
            {
                field: 'UnAcknowledgeReason', headerText: 'Remarks', visible: status == statusConstants.REVISE
            },
            {
                field: 'StatusText', headerText: 'Status', visible: status == statusConstants.ALL
            },
        ];
        if ($tblMasterEl) $tblMasterEl.destroy();

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            allowGrouping: true,
            autofitColumns: false,
            allowSorting: true,
            apiEndPoint: `/api/fab-acknowledge/bulkfabric/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands,
            queryCellInfo: cellModifyForBDSAck
        });
    }
    var actionName = '';
    function handleCommands(args) {
        $formEl.find('.deletedItemTable').hide();
        actionName = args.commandColumn.type;

        if (args.commandColumn.type == 'Add') {
            checkRevisionStatus(args.rowData.BookingNo, args.rowData.ExportOrderNo, args.rowData.SubGroupID);
        }
        else if (args.commandColumn.type == 'Edit') {
            //if (args.rowData.BookingNo == "250399-FBR") {

            //}

            getView(args.rowData.BookingNo, args.rowData.WithoutOB);
            $formEl.find("#btnReceive,#btnReceived,#btnCancelAcknowledge,#btnCancelUnAcknowledge").fadeOut();
            $formEl.find("#btnSave,#btnUnAcknowledge,#btnOkk").fadeIn();
            if (status == statusConstants.REJECT) {
                $formEl.find("#btnCancelAcknowledge,#btnCancelUnAcknowledge").fadeIn();
                $formEl.find("#btnSave,#btnUnAcknowledge,#btnOkk").fadeOut();
            }
        }

        else if (args.commandColumn.type == 'Email') {
            showBootboxConfirm("Send Mail.", "Are you sure to re-send mail?", function (yes) {
                if (yes) {
                    sendMail(args.rowData.BookingNo, args.rowData.WithoutOB);
                }
            });
            //checkRevisionStatusView(args.rowData.BookingNo, args.rowData.ExportOrderNo, args.rowData.SubGroupID, args.rowData.WithoutOB)
            //getView(args.rowData.BookingNo, args.rowData.WithoutOB);
            //$formEl.find("#btnReceive,#btnReceived,#btnCancelAcknowledge,#btnCancelUnAcknowledge").fadeOut();
            //$formEl.find("#btnSave,#btnUnAcknowledge,#btnOkk").fadeIn();
            //if (status == statusConstants.REJECT) {
            //    $formEl.find("#btnCancelAcknowledge,#btnCancelUnAcknowledge").fadeIn();
            //    $formEl.find("#btnSave,#btnUnAcknowledge,#btnOkk").fadeOut();
            //}
        }

        else if (args.commandColumn.type == 'View') {
            if (status != statusConstants.REVISE && status != statusConstants.PROPOSED_FOR_ACKNOWLEDGE && status != statusConstants.UN_ACKNOWLEDGE && status != statusConstants.COMPLETED && status != statusConstants.ALL) {
                if (!args.rowData.IsRevisionValid) {
                    toastr.error("Revision acknowledge pending!!");
                    return false;
                }
            }
            checkRevisionStatusView(args.rowData.BookingNo, args.rowData.ExportOrderNo, args.rowData.SubGroupID, args.rowData.WithoutOB)
            //getView(args.rowData.BookingID);
            //$formEl.find("#btnSave").fadeOut();
            //$formEl.find("#btnSave,#btnUnAcknowledge,#btnReceived,#btnOkk,#btnCancelAcknowledge,#btnCancelUnAcknowledge").fadeOut();
        }
        else if (args.commandColumn.type == 'ViewRecive') {
            getView(args.rowData.BookingID);
            $formEl.find("#btnSave").fadeOut();
            $formEl.find("#btnSave,#btnUnAcknowledge,#btnOkk,#btnCancelAcknowledge,#btnCancelUnAcknowledge").fadeOut();
            $formEl.find("#btnReceived").fadeIn();

            $formEl.find("#btnSave").fadeIn();
            $formEl.find("#btnReceived").fadeOut();
        }
        else if (args.commandColumn.type == 'Booking Report') {
            if (status == statusConstants.COMPLETED) {
                window.open(`/reports/InlinePdfView?ReportName=BookingInformationFabricMainAck.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
            }
            //else if (status == statusConstants.ALL && args.rowData.Status == true) {
            else if (status == statusConstants.ALL && args.rowData.StatusText == "Acknowledged") {
                window.open(`/reports/InlinePdfView?ReportName=BookingInformationFabricMainAck.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
            }
            else {
                if (args.rowData.WithoutOB == true)
                    window.open(`/reports/InlinePdfView?ReportName=SampleFabric.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
                else
                    window.open(`/reports/InlinePdfView?ReportName=BookingInformationFabricMainForPMCN.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
            }
        }
        else if (args.commandColumn.type == 'Tech Pack Attachment') {
            if (args.rowData.ImagePath == '' || args.rowData.ImagePath == null) {
                toastr.error("No attachment found!!");
            } else {
                var url = window.location.href;
                var path = "";
                if (url.includes("8060") || url.includes("local")) {
                    path = constants.GMT_ERP_LOCAL_PATH;
                } else {
                    path = constants.GMT_ERP_BASE_PATH;
                }
                var imagePath = path + args.rowData.ImagePath;
                window.open(imagePath, "_blank");
            }
        }
        else if (args.commandColumn.type == 'Booking Attachment') {
            if (args.rowData.ImagePath1 == '' || args.rowData.ImagePath1 == null) {
                toastr.error("No attachment found!!");
            } else {
                var url = window.location.href;
                var path = "";
                if (url.includes("8060") || url.includes("local")) {
                    path = constants.GMT_ERP_LOCAL_PATH;
                } else {
                    path = constants.GMT_ERP_BASE_PATH;
                }
                var imagePath1 = path + args.rowData.ImagePath1;
                window.open(imagePath1, "_blank");
            }
        }
        else if (args.commandColumn.type == 'AddBulk') {
            getNewBulk(args.rowData.BookingNo);
            $formEl.find("#btnSave").fadeIn();
            $formEl.find("#btnReceive,#btnUnAcknowledge,#btnReceived,#btnCancelAcknowledge,#btnCancelUnAcknowledge").fadeOut();
        }

        //$formEl.find("#btnSave").fadeIn();
    }

    function cellModifyForBDSAck(args) {
        if (args.data.ImagePath == '') {
            if (args.cell.childNodes.length > 0) {
                for (var i = 0; i < args.cell.childNodes[0].childNodes.length; i++) {
                    if (args.cell.childNodes[0].childNodes[i].title === 'Tech Pack Attachment') {
                        args.cell.childNodes[0].childNodes[i].style.display = "none";
                    }
                }
            }
        }
        if (args.data.ImagePath1 == '') {
            if (args.cell.childNodes.length > 0) {
                for (var i = 0; i < args.cell.childNodes[0].childNodes.length; i++) {
                    if (args.cell.childNodes[0].childNodes[i].title === 'Booking Attachment') {
                        args.cell.childNodes[0].childNodes[i].style.display = "none";
                    }
                }
            }
        }
    }

    var YarnSourceNameElem;
    var YarnSourceNameobj;
    var machineTypeElem;
    var machineTypeObj;
    var technicalNameElem;
    var technicalNameObj;
    var brandElem;
    var brandObj;
    async function initChild(data) {
        
        data = data.filter(x => x.BookingQty > 0);
        var totalBookingQty = 0;
        data.map(x => {
            x.DiffPreAndBookingQty = x.BookingQty - x.PreviousBookingQty;
            x.FabricWidth = getIntegerFromdecimal(x.FabricWidth);
            totalBookingQty += parseFloat(x.BookingQty);
        });
        totalBookingQty = getDefaultValueWhenInvalidN_Float(totalBookingQty);
        $formEl.find("#BookingQty").val(totalBookingQty);

        data = setCalculatedValues(data);
        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [
            { field: 'ConsumptionID', isPrimaryKey: true, visible: false },
            { field: 'BookingChildID', visible: false },

            {
                field: 'Construction', headerText: 'Construction', width: 60, allowEditing: false
            },
            {
                field: 'Composition', headerText: 'Composition', width: 120, allowEditing: false
            },
            {
                field: 'Color', headerText: 'Color', width: 70, allowEditing: false
            },
            {
                field: 'GSM', headerText: 'GSM', width: 50, allowEditing: false
            },
            {
                field: 'FabricWidth', headerText: 'Fabric Width', width: 60, allowEditing: false
            },
            {
                field: 'KnittingType', headerText: 'Knitting Type', width: 60, allowEditing: false
            },
            {
                field: 'PreviousBookingQty', headerText: 'Previous Booking Qty', visible: actionName == 'Add' ? false : true, width: 60, textAlign: 'right', allowEditing: false
            },
            {
                field: 'BookingQty', headerText: 'Booking Qty', width: 60, textAlign: 'right', allowEditing: false, valueAccessor: displayBookingQty
            },
            {
                field: 'DiffPreAndBookingQty', headerText: 'Qty Different', visible: actionName == 'Add' ? false : true, textAlign: 'center', width: 85
            },
            {
                field: 'LiabilitiesBookingQty', headerText: 'Liabilities Booking Qty', visible: actionName == 'Add' ? false : true, textAlign: 'center', width: 85, valueAccessor: diplayPlanningCriteria
            },
            {
                field: 'ActualBookingQty', headerText: 'Actual Booking Qty', visible: false, width: 85, allowEditing: false
            },
            {
                field: 'BookingUOM', headerText: 'Booking UOM', width: 60, textAlign: 'left', allowEditing: false
            },
            {
                field: 'RefSourceNo', headerText: 'Ref No', visible: true, width: 85, allowEditing: false
            },
            {
                field: 'PartName', headerText: 'Use In', visible: true, width: 85, allowEditing: false
            },
            {
                headerText: '', textAlign: 'Center', width: 40, visible: true, commands: [
                    { buttonOption: { type: 'findRefSourceNo', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Ref Detail" } }
                ]
            },
            {
                field: 'Instruction', headerText: 'Instruction', allowEditing: false, width: 450
            },
            {
                field: 'BookingChildID', headerText: 'GMT BookingChildID', visible: false
            },
        ];
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            columns: columns,
            recordClick: function (args) {
                if (args.column && args.column.field == "LiabilitiesBookingQty") {
                    if (args.rowData.BookingQty < args.rowData.PreviousBookingQty) {
                        _oRow = args.rowData;
                        _index = args.rowIndex;
                        _modalFrom = subGroupNames.FABRIC;

                        setItemDetails({
                            lbl1: "Construction",
                            lbl1Value: args.rowData.Construction,
                            lbl2: "Composition",
                            lbl2Value: args.rowData.Composition,
                            lbl3: "Color",
                            lbl3Value: args.rowData.Color,
                            lbl4: "GSM",
                            lbl4Value: args.rowData.GSM,
                            lbl5: "Fabric Width",
                            lbl5Value: args.rowData.FabricWidth,
                            lbl6: "Knitting Type",
                            lbl6Value: args.rowData.KnittingType,
                            lbl7: "Pre Booking Qty",
                            lbl7Value: args.rowData.PreviousBookingQty,
                            lbl8: "Current Booking Qty",
                            lbl8Value: args.rowData.BookingQty,

                            lbl9: "Different Qty",
                            lbl9Value: getDefaultValueWhenInvalidN_Float(args.rowData.BookingQty) - getDefaultValueWhenInvalidN_Float(args.rowData.PreviousBookingQty),

                            lbl10: "Knitting Prod Qty",
                            lbl10Value: 0,
                            lbl11: "Dyeing Prod Qty",
                            lbl11Value: 0,
                            lbl12: "Finish Fabric Stock Qty",
                            lbl12Value: args.rowData.TotalFinishFabricStockQty,
                            lbl13: "Total YD Prod.",
                            lbl13Value: 0,
                            lbl14: "Delivered Qty",
                            lbl14Value: args.rowData.DeliveredQtyForLiability
                        }, 1);

                        initPlanningTable(args.rowData.ChildAckLiabilityDetails);
                        initYarnChildTableAsync(args.rowData.ChildAckYarnLiabilityDetails);
                        $modalPlanningEl.modal('show');
                    }
                }
            },
            actionBegin: function (args) {

                //if (args.requestType === "save") {
                //    args.data = setArgDataValues(args.data, args.rowData);
                //}
            },
            commandClick: childCommandClick2,
            rowDataBound: rowDataBound
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }
    async function childCommandClick2(e) {
        if (e.commandColumn.buttonOption.type == 'findRefSourceNo') {
            var response = await axios.get(`/api/bds-acknowledge/getRefSourceItem/${e.rowData.BookingID}/${e.rowData.ConsumptionID}`);
            var list = response.data.Items;

            if (list.length == 0) {
                return toastr.error("No Reference Found.");
            }

            var finder = new commonFinder({
                title: "Select Ref Source",
                pageId: pageId,
                height: 320,
                data: list,
                fields: "BookingNo,RefSourceNo,Composition,Construction,Color,GSM,RefSourceName",
                headerTexts: "BookingNo,RefSourceNo,Composition,Construction,Color,GSM,RefSourceName",
                widths: "80,60,60,60,40,40,40",
                editTypes: "text,text,text,text,text,text,text",
                isMultiselect: false,
                autofitColumns: true,
                primaryKeyColumn: "BRefDetailsID",
                modalSize: "modal-md",
                top: "2px",
                onSelect: function (record) {
                    finder.hideModal();

                    var childData = e.rowData;
                    childData.RefSourceID = record.rowData.RefSourceID;
                    childData.RefSourceNo = record.rowData.RefSourceNo;

                    var bookingNo = record.rowData.RefSourceNo;
                    var subGroupId = record.rowData.SubGroupID;
                    var consumptionID = record.rowData.ConsumptionID;

                    getYarnInfos(childData, bookingNo, subGroupId, consumptionID);
                }
            });
            finder.showModal();
        }

    }
    function setItemDetails(obj, subGroupID) {
        var maxCount = 15;
        itemDetailsId.find(".lblLibMasterInfo").text("");
        itemDetailsId.attr("SubGroupId", subGroupID);
        for (var pp = 1; pp <= maxCount; pp++) {
            var propValue = getDefaultValueWhenInvalidS(obj["lbl" + pp]);
            if (propValue.length > 0) propValue += " : ";

            itemDetailsId.find("#lbl" + pp).text(propValue);
            itemDetailsId.find("#lbl" + pp + "Value").text(obj["lbl" + pp + "Value"]);
        }
    }
    function rowDataBound(args) {
        if (args.data.Status == 'New Child') {
            args.row.style.backgroundColor = "#9FD3F7";
            args.row.style.color = "#000000";
        }
    }

    async function initChildCollar(data) {
        data = data.filter(x => x.BookingQty > 0);
        data.map(x => {
            x.DiffPreAndBookingQty = x.BookingQty - x.PreviousBookingQty;
        });
        data = setCalculatedValues(data);
        if ($tblChildCollarIdEl) $tblChildCollarIdEl.destroy();
        var columns = [
            { field: 'ConsumptionID', isPrimaryKey: true, visible: false },
            { field: 'BookingChildID', visible: false },
            {
                field: 'Segment1Desc', headerText: 'Collar Description', width: 60, allowEditing: false
            },
            {
                field: 'Segment2Desc', headerText: 'Collar Type', width: 120, allowEditing: false
            },
            {
                field: 'Segment3Desc', headerText: 'Length(cm)', width: 70, allowEditing: false
            },
            {
                field: 'Segment4Desc', headerText: 'Height(cm)', width: 50, allowEditing: false
            },
            {
                field: 'Segment5Desc', headerText: 'Body Color', width: 60, allowEditing: false
            },
            {
                field: 'YarnType', headerText: 'Yarn Type', width: 85, allowEditing: false
            },
            {
                field: 'YarnProgram', headerText: 'Yarn Program', width: 85, allowEditing: false
            },
            {
                field: 'PreviousBookingQty', headerText: 'Previous Booking Qty', visible: actionName == 'Add' ? false : true, width: 60, textAlign: 'right', allowEditing: false
            },
            {
                field: 'BookingQty', headerText: 'Booking Qty', width: 60, textAlign: 'right', allowEditing: false, valueAccessor: displayBookingQty
            },
            {
                field: 'DiffPreAndBookingQty', headerText: 'Qty Different', visible: actionName == 'Add' ? false : true, textAlign: 'center', width: 85
            },
            {
                field: 'LiabilitiesBookingQty', headerText: 'Liabilities Booking Qty', visible: actionName == 'Add' ? false : true, textAlign: 'center', width: 85, valueAccessor: diplayPlanningCriteria
            },
            {
                field: 'ActualBookingQty', headerText: 'Actual Booking Qty', visible: false, width: 85, allowEditing: false
            },
            {
                field: 'BookingUOM', headerText: 'Booking UOM', width: 60, textAlign: 'left', allowEditing: false
            },
            {
                field: 'Instruction', headerText: 'Instruction', allowEditing: false
            },
            {
                field: 'BookingChildID', headerText: 'GMT BookingChildID', visible: false
            }
        ];
        $tblChildCollarIdEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            columns: columns,
            recordClick: function (args) {
                if (args.column && args.column.field == "LiabilitiesBookingQty") {
                    if (args.rowData.BookingQty < args.rowData.PreviousBookingQty) {
                        _oRow = args.rowData;
                        _index = args.rowIndex;
                        _modalFrom = subGroupNames.COLLAR;

                        setItemDetails({
                            lbl1: "Collar Description",
                            lbl1Value: args.rowData.Segment1Desc,
                            lbl2: "Collar Type",
                            lbl2Value: args.rowData.Segment2Desc,
                            lbl3: "Length(cm)",
                            lbl3Value: args.rowData.Segment3Desc,
                            lbl4: "Height(cm)",
                            lbl4Value: args.rowData.Segment4Desc,
                            lbl5: "Body Color",
                            lbl5Value: args.rowData.Segment5Desc,
                            lbl6: "Yarn Type",
                            lbl6Value: args.rowData.KnittingType,
                            lbl7: "Pre Booking Qty",
                            lbl7Value: args.rowData.PreviousBookingQty,
                            lbl8: "Current Booking Qty",
                            lbl8Value: args.rowData.BookingQty,
                            lbl9: "Different Qty",
                            lbl9Value: getDefaultValueWhenInvalidN_Float(args.rowData.BookingQty) - getDefaultValueWhenInvalidN_Float(args.rowData.PreviousBookingQty),
                            lbl10: "Total YD Prod.",
                            lbl10Value: 0,
                            lbl11: "Finish Fabric Stock Qty",
                            lbl11Value: args.rowData.TotalFinishFabricStockQty,
                            lbl12: "Delivered Qty",
                            lbl12Value: args.rowData.DeliveredQtyForLiability
                        }, 11);

                        initPlanningTable(args.rowData.ChildAckLiabilityDetails);
                        initYarnChildTableAsync(args.rowData.ChildAckYarnLiabilityDetails);
                        //initCriteriaIDTable(_oRow.CriteriaNames, _oRow.FBAChildPlannings, _oRow.FBAChildPlanningsWithIds, _oRow.BookingChildID);
                        $modalPlanningEl.modal('show');
                    }
                }
            },
            actionBegin: function (args) {
                //if (args.requestType === "save") {
                //    args.data = setArgDataValues(args.data, args.rowData);
                //}
            },
            rowDataBound: rowDataBound
        });
        $tblChildCollarIdEl.refreshColumns;
        $tblChildCollarIdEl.appendTo(tblChildCollarId);
    }

    async function initChildCuff(data) {
        data = data.filter(x => x.BookingQty > 0);
        data.map(x => {
            x.DiffPreAndBookingQty = x.BookingQty - x.PreviousBookingQty;
        });
        data = setCalculatedValues(data);
        if ($tblChildCuffIdEl) $tblChildCuffIdEl.destroy();
        var columns = [
            { field: 'ConsumptionID', isPrimaryKey: true, visible: false },
            { field: 'BookingChildID', visible: false },
            { field: 'ItemMasterID', visible: false },
            {
                field: 'Segment1Desc', headerText: 'Cuff Description', width: 60, allowEditing: false
            },
            {
                field: 'Segment2Desc', headerText: 'Cuff Type', width: 120, allowEditing: false
            },
            {
                field: 'Segment3Desc', headerText: 'Length(cm)', width: 70, allowEditing: false
            },
            {
                field: 'Segment4Desc', headerText: 'Height(cm)', width: 50, allowEditing: false
            },
            {
                field: 'Segment5Desc', headerText: 'Body Color', width: 60, allowEditing: false
            },
            {
                field: 'YarnType', headerText: 'Yarn Type', width: 85, allowEditing: false
            },
            {
                field: 'YarnProgram', headerText: 'Yarn Program', width: 85, allowEditing: false
            },
            {
                field: 'PreviousBookingQty', headerText: 'Previous Booking Qty', visible: actionName == 'Add' ? false : true, width: 60, textAlign: 'right', allowEditing: false
            },
            {
                field: 'BookingQty', headerText: 'Booking Qty', width: 60, textAlign: 'right', allowEditing: false, valueAccessor: displayBookingQty
            },
            {
                field: 'DiffPreAndBookingQty', headerText: 'Qty Different', visible: actionName == 'Add' ? false : true, textAlign: 'center', width: 85
            },
            {
                field: 'LiabilitiesBookingQty', headerText: 'Liabilities Booking Qty', visible: actionName == 'Add' ? false : true, textAlign: 'center', width: 85, valueAccessor: diplayPlanningCriteria
            },
            {
                field: 'ActualBookingQty', headerText: 'Actual Booking Qty', visible: false, width: 85, allowEditing: false
            },
            {
                field: 'BookingUOM', headerText: 'Booking UOM', width: 60, textAlign: 'left', allowEditing: false
            },
            {
                field: 'Instruction', headerText: 'Instruction', allowEditing: false
            },
            {
                field: 'BookingChildID', headerText: 'GMT BookingChildID', visible: false
            }
        ];
        $tblChildCuffIdEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            columns: columns,
            recordClick: function (args) {
                if (args.column && args.column.field == "LiabilitiesBookingQty") {
                    if (args.rowData.BookingQty < args.rowData.PreviousBookingQty) {
                        _oRow = args.rowData;

                        _index = args.rowIndex;
                        _modalFrom = subGroupNames.CUFF;


                        setItemDetails({
                            lbl1: "Cuff Description",
                            lbl1Value: args.rowData.Segment1Desc,
                            lbl2: "Cuff Type",
                            lbl2Value: args.rowData.Segment2Desc,
                            lbl3: "Length(cm)",
                            lbl3Value: args.rowData.Segment3Desc,
                            lbl4: "Height(cm)",
                            lbl4Value: args.rowData.Segment4Desc,
                            lbl5: "Body Color",
                            lbl5Value: args.rowData.Segment5Desc,
                            lbl6: "Yarn Type",
                            lbl6Value: args.rowData.KnittingType,
                            lbl7: "Pre Booking Qty",
                            lbl7Value: args.rowData.PreviousBookingQty,
                            lbl8: "Current Booking Qty",
                            lbl8Value: args.rowData.BookingQty,
                            lbl9: "Different Qty",
                            lbl9Value: getDefaultValueWhenInvalidN_Float(args.rowData.BookingQty) - getDefaultValueWhenInvalidN_Float(args.rowData.PreviousBookingQty),
                            lbl10: "Total YD Prod.",
                            lbl10Value: 0,
                            lbl11: "Finish Fabric Stock Qty",
                            lbl11Value: args.rowData.TotalFinishFabricStockQty,
                            lbl12: "Delivered Qty",
                            lbl12Value: args.rowData.DeliveredQtyForLiability
                        }, 12);

                        initPlanningTable(args.rowData.ChildAckLiabilityDetails);
                        initYarnChildTableAsync(args.rowData.ChildAckYarnLiabilityDetails);
                        //initCriteriaIDTable(_oRow.CriteriaNames, _oRow.FBAChildPlannings, _oRow.FBAChildPlanningsWithIds, _oRow.BookingChildID);
                        $modalPlanningEl.modal('show');
                    }
                }
            },
            actionBegin: function (args) {
                //if (args.requestType === "save") {
                //    args.data = setArgDataValues(args.data, args.rowData);
                //}
            },
            rowDataBound: rowDataBound
        });
        $tblChildCuffIdEl.refreshColumns;
        $tblChildCuffIdEl.appendTo(tblChildCuffId);
    }

    async function initYarnChildTableAsync(data) {
        data.map(x => {
            x.TotalValue = x.LiabilityQty * x.Rate;
            x.TotalValue = getDefaultValueWhenInvalidN_Float(x.TotalValue);
        });

        if ($tblChildYarnEl) $tblChildYarnEl.destroy();
        var columns = [], addYarnColumns = [], additionalColumns = [], childColumns = [];

        columns.shift();
        addYarnColumns = [
            { field: 'ChildID', isPrimaryKey: true, visible: false },
            { field: 'ConsumptionID', visible: false },
            { field: 'ItemMasterID', visible: false },
            { field: 'BookingID', visible: false },
            { field: 'UnitID', visible: false },
            { field: '_segment1ValueDesc', headerText: 'Composition', width: 80, allowEditing: false },
            { field: '_segment2ValueDesc', headerText: 'Yarn Type', width: 80, allowEditing: false },
            { field: '_segment3ValueDesc', headerText: 'Manufacturing Process', width: 80, allowEditing: false },
            { field: '_segment4ValueDesc', headerText: 'Sub Process', width: 80, allowEditing: false },
            { field: '_segment5ValueDesc', headerText: 'Quality Parameter', width: 80, allowEditing: false },
            { field: '_segment6ValueDesc', headerText: 'Count', width: 80, allowEditing: false },
            { field: 'ShadeCode', headerText: 'Shade Code', width: 80, allowEditing: false },
            { field: 'Specification', headerText: 'Yarn Specification', width: 100, allowEditing: false },
            { field: 'DisplayUnitDesc', headerText: 'UOM', width: 40, allowEditing: false },
            { field: 'YD', headerText: 'YD Item?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100, allowEditing: false },

            { field: 'ReqQty', headerText: 'Req Qty', width: 100, allowEditing: false },
            { field: 'POStatus', headerText: 'PO Status', width: 100, allowEditing: false },
            { field: 'AllocatedQty', headerText: 'Allocation Qty', width: 100, allowEditing: false },
            { field: 'TotalIssueQty', headerText: 'Issue Qty', width: 100, allowEditing: false },

            //{ field: 'IssueQty', headerText: 'Issue Qty', width: 100, allowEditing: false },
            { field: 'YDProdQty', headerText: 'YD Prod Qty', width: 100, allowEditing: false },
            { field: 'LiabilityQty', headerText: 'Liability Qty', width: 100, allowEditing: statusConstants.REVISE == status },
            { field: 'Rate', headerText: 'Rate', allowEditing: statusConstants.REVISE == status },
            { field: 'TotalValue', headerText: 'Value', allowEditing: false }
        ];
        columns.push.apply(columns, addYarnColumns);
        columns = resizeColumns(columns);
        ej.base.enableRipple(true);
        $tblChildYarnEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            columns: columns,
            editSettings: {
                allowEditing: true,
                allowAdding: true,
                allowDeleting: false,
                mode: "Normal",
                showDeleteConfirmDialog: true
            },
            enableContextMenu: true,
            contextMenuItems: [
                { text: 'Copy', target: '.e-content', id: 'copy' },
                { text: 'Paste', target: '.e-content', id: 'paste' }
            ],
            contextMenuClick: function (args) {
                if (args.item.id === 'copy') {
                    _yarnLiabilitiesItem = objectCopy(args.rowInfo.rowData);
                    if (_yarnLiabilitiesItem == null) {
                        toastr.error("No item found to copy!!");
                        return false;
                    }
                }
                else if (args.item.id === 'paste') {
                    if (_yarnLiabilitiesItem == null) {
                        //toastr.error("Please copy first!!");
                        return false;
                    }
                    var childs = $tblChildYarnEl.getCurrentViewRecords();

                    //Check Item wise
                    var ydItems = childs.filter(x => x.ItemMasterID == _yarnLiabilitiesItem.ItemMasterID && x.YD == true);
                    var nonYdItems = childs.filter(x => x.ItemMasterID == _yarnLiabilitiesItem.ItemMasterID && x.YD == false);
                    if (ydItems.length == 1 && nonYdItems.length == 1) {
                        //toastr.error("Already has YD and Non-YD item");
                        return false;
                    }

                    var copiedItem = objectCopy(_yarnLiabilitiesItem);
                    copiedItem.ChildID = _yarnLiabilitiesChildID++;
                    copiedItem.YD = copiedItem.YD ? false : true;
                    copiedItem.LiabilityQty = 0;

                    childs.push(DeepClone(copiedItem));
                    $tblChildYarnEl.dataSource = childs;
                }
            },
            actionBegin: function (args) {
                if (args.requestType === "save") {
                    if (!args.data.YD && args.data.AllocatedQty == 0) {
                        args.data.LiabilityQty = 0;
                        toastr.error(`No allocation qty`);
                        return false;
                    }

                    if (!args.data.YD && args.data.LiabilityQty > args.data.AllocatedQty) {
                        args.data.LiabilityQty = args.data.AllocatedQty;
                        toastr.error(`Maximum allocation Qty is ${args.data.AllocatedQty}`);
                        //return false;
                    }

                    var libalities = $tblPlanningEl.getCurrentViewRecords();
                    var libalityIndex = -1;
                    if (typeof args.data.YD === "undefined" || args.data.YD == null) args.data.YD = false;
                    if (args.data.YD) {
                        libalityIndex = libalities.length > 0 ? libalities.findIndex(x => isDyedYarnType(x.LiabilitiesName) == true) : null;
                    }
                    else {
                        libalityIndex = libalities.length > 0 ? libalities.findIndex(x => isYarnType(x.LiabilitiesName) == true) : null;
                    }
                    args.data.TotalValue = (args.data.LiabilityQty * args.data.Rate).toFixed(2);
                    if (libalityIndex > -1) {
                        var selectedRow = args.data;
                        var obj = libalities[libalityIndex];
                        obj.LiabilityQty = args.data.LiabilityQty;
                        obj = getLiabilityQtys(obj, null, selectedRow);
                        $tblPlanningEl.updateRow(libalityIndex, obj);
                    }
                }
            },
        });
        $tblChildYarnEl.refreshColumns;
        $tblChildYarnEl.appendTo(tblYarnChildId);
    }
    function isYarnType(liabilitiesName) {
        if (!liabilitiesName.toLowerCase().includes("dyed") && liabilitiesName.toLowerCase().includes("yarn")) return true;
        return false;
    }
    function isDyedYarnType(liabilitiesName) {
        if (liabilitiesName.toLowerCase().includes("dyed") && liabilitiesName.toLowerCase().includes("yarn")) return true;
        return false;
    }
    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
    function resizeColumns(childColumns) {
        var cAry = ["Segment1ValueId", "ShadeCode", "Specification", "DisplayUnitDesc"];
        cAry.map(c => {
            var indexF = childColumns.findIndex(x => x.field == c);
            var widthValue = 80;
            if (c == "Segment1ValueId") widthValue = 180;
            if (indexF > -1) childColumns[indexF].width = widthValue;
        });
        return childColumns;
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

    async function updateCriteriaIDTable(_oRow, sourceData) {
        for (var i = 0; i < _oRow.CriteriaNames.length; i++) {
            var obj = sourceData.CriteriaNames.find(function (el) { return el.CriteriaName == _oRow.CriteriaNames[i].CriteriaName });
            if (obj) {
                _oRow.CriteriaNames[i].TotalTime = obj.TotalTime;
                _oRow.CriteriaNames[i].CriteriaIDs = obj.CriteriaIDs;
            }
            var objCPs = sourceData.FBAChildPlannings.filter(function (el) { return el.CriteriaName == _oRow.CriteriaNames[i].CriteriaName });
            if (objCPs) {
                for (var j = 0; j < objCPs.length; j++) {
                    var objCP = _oRow.FBAChildPlannings.find(function (el) { return el.CriteriaName == objCPs[j].CriteriaName && el.OperationName == objCPs[j].OperationName });
                    if (objCP) {
                        objCPs[j].ProcessTime = objCP.ProcessTime;
                    }
                }
            }
        }
    }

    function loadYarnBookingChildItems() {
        this.dataSource = (status == statusConstants.PENDING || status == statusConstants.REJECT) ? this.parentDetails.parentRowData.ChildItems : this.parentDetails.parentRowData.ChildDetails;
    }

    function diplayPlanningCriteria(field, data, column) {
        column.disableHtmlEncode = false;
        return `<a class="btn btn-xs btn-default" href="javascript:void(0)" title="Liabilities Qty">
                                     ${data[field] ? data[field] : 0}
                                </a>`;
    }
    function displayBookingQty(field, data, column) {
        column.disableHtmlEncode = false;
        var css = data.Status == 'Qty Changed' ? 'background-color: #ECA338' : '';
        return `<a class="btn btn-xs btn-default" style='${css}' href="javascript:void(0)" title="Previous Booking Qty : ${data['PreviousBookingQty'] ? data['PreviousBookingQty'] : 0}">
                                     ${data[field] ? data[field] : 0}
                                </a>`;
    }
    function diplayPlanningCriteriaTime(field, data, column) {
        column.disableHtmlEncode = false;
        return `<a class="btn btn-xs btn-default" href="javascript:void(0)" title="Total Time">
                                     ${data[field] ? data[field] : 0}
                                </a>`;
    }

    async function initChildCollar1(data) {
        data = setCalculatedValues(data);
        if ($tblChildCollarIdEl) $tblChildCollarIdEl.destroy();
        var columns = [
            { field: 'ConsumptionID', isPrimaryKey: true, visible: false },
            { field: 'BookingID', visible: false },
            { field: 'ItemMasterID', visible: false },
            { field: 'SubGroupID', visible: false },
            { field: 'ConceptTypeID', visible: false },
            {
                field: 'MachineType', headerText: 'Machine Type ', visible: status != statusConstants.ACTIVE || _isBDS == 2, edit: {
                    create: function () {
                        machineTypeElem = document.createElement('input');
                        return machineTypeElem;
                    },
                    read: function () {
                        return machineTypeObj.text;
                    },
                    destroy: function () {
                        machineTypeObj.destroy();
                    },
                    write: function (e) {
                        machineTypeObj = new ej.dropdowns.DropDownList({
                            dataSource: masterData.MCTypeForOtherList,
                            fields: { value: 'id', text: 'text' },
                            change: function (f) {
                                //
                                technicalNameObj.enabled = true;
                                var tempQuery = new ej.data.Query().where('additionalValue', 'equal', machineTypeObj.value);
                                technicalNameObj.query = tempQuery;
                                technicalNameObj.text = null;
                                technicalNameObj.dataBind();

                                e.rowData.MachineTypeId = f.itemData.id;
                                e.rowData.MachineType = f.itemData.text;
                                e.rowData.KTypeId = f.itemData.desc;
                                e.rowData = setTotalDaysAndDeliveryDate(e.rowData, e.rowData.CriteriaNames);
                            },
                            placeholder: 'Select M/C Type',
                            floatLabelType: 'Never'
                        });
                        machineTypeObj.appendTo(machineTypeElem);
                    }
                }
            },
            {
                field: 'TechnicalName', headerText: 'Technical Name', visible: status != statusConstants.ACTIVE || _isBDS == 2, edit: {
                    create: function () {
                        technicalNameElem = document.createElement('input');
                        return technicalNameElem;
                    },
                    read: function () {
                        return technicalNameObj.text;
                    },
                    destroy: function () {
                        technicalNameObj.destroy();
                    },
                    write: function (e) {
                        technicalNameObj = new ej.dropdowns.DropDownList({
                            dataSource: masterData.TechnicalNameList,
                            fields: { value: 'id', text: 'text' },
                            enabled: false,
                            placeholder: 'Select Technical Name',
                            floatLabelType: 'Never',
                            change: function (f) {
                                if (!f.isInteracted || !f.itemData) return false;
                                e.rowData.TechnicalTime = parseInt(f.itemData.desc);
                                e.rowData.TechnicalNameId = f.itemData.id;
                                e.rowData.TechnicalName = f.itemData.text;
                                e.rowData = setTotalDaysAndDeliveryDate(e.rowData, e.rowData.CriteriaNames);
                                //$tblChildCollarIdEl.updateRow(e.row.rowIndex, e.rowData);
                            }
                        });
                        technicalNameObj.appendTo(technicalNameElem);
                    }
                }
            },
            {
                field: 'MachineGauge', headerText: 'Gauge', visible: _isBDS == 2, width: 80, allowEditing: true, editType: "numericedit", params: { decimals: 0, format: "N", min: 0, validateDecimalOnType: true }
            },
            {
                field: 'Brand', headerText: 'Brand', visible: _isBDS == 2, edit: {
                    create: function () {
                        brandElem = document.createElement('input');
                        return brandElem;
                    },
                    read: function () {
                        return brandObj.text;
                    },
                    destroy: function () {
                        brandObj.destroy();
                    },
                    write: function (e) {
                        brandObj = new ej.dropdowns.DropDownList({
                            dataSource: masterData.KnittingMachines,
                            fields: { value: 'id', text: 'text' },
                            change: function (f) {
                                e.rowData.BrandID = f.itemData.id;
                                e.rowData.Brand = f.itemData.text;
                            },
                            placeholder: 'Select Brand',
                            floatLabelType: 'Never'
                        });
                        brandObj.appendTo(brandElem);
                    }
                }
            },
            {
                field: 'IsSubContact', headerText: 'Sub-Contact?', visible: status != statusConstants.ACTIVE && _isBDS == 1, displayAsCheckBox: true, editType: "booleanedit", width: 85, textAlign: 'Center'
            },
            {
                field: 'TotalDays', headerText: 'Total Days', visible: status != statusConstants.ACTIVE && _isBDS == 1, allowEditing: false, textAlign: 'center', width: 85, valueAccessor: diplayPlanningCriteria
            },
            {
                field: 'StructureDays', headerText: 'Structure Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
            },
            {
                field: 'FinishingDays', headerText: 'Finishing Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
            },
            {
                field: 'DyeingDays', headerText: 'Dyeing Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
            },
            {
                field: 'BatchPreparationDays', headerText: 'Batch Preparation Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
            },
            {
                field: 'KnittingDays', headerText: 'Knitting Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
            },
            {
                field: 'TestReportDays', headerText: 'Test Report Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
            },
            {
                field: 'MaterialDays', headerText: 'Material Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
            },
            {
                field: 'QualityDays', headerText: 'Quality Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
            },
            {
                field: 'DeliveryDate', headerText: 'Delivery Date', visible: status != statusConstants.ACTIVE && _isBDS == 1, textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false
            },
            {
                field: 'Color', headerText: 'Color', width: 85, allowEditing: false
            },
            {
                field: 'YarnType', headerText: 'Yarn Type', width: 85, allowEditing: false
            },
            {
                field: 'YarnProgram', headerText: 'Yarn Program', width: 85, allowEditing: false
            },
            {
                field: 'ReferenceSourceName', headerText: 'Reference Source', visible: _isBDS == 1 ? true : false, width: 85, allowEditing: false
            },
            {
                field: 'ReferenceNo', headerText: 'Reference No', visible: _isBDS == 1 ? true : false, width: 85, allowEditing: false
            },
            {
                field: 'ColorReferenceNo', headerText: 'ColorReference No', visible: _isBDS == 1 ? true : false, allowEditing: false
            },
            {
                field: 'ValueName', headerText: 'Yarn Source', visible: false/* status != statusConstants.ACTIVE*/, edit: {
                    create: function () {
                        YarnSourceNameElem = document.createElement('input');
                        return YarnSourceNameElem;
                    },
                    read: function () {
                        return YarnSourceNameobj.text;
                    },
                    destroy: function () {
                        YarnSourceNameobj.destroy();
                    },
                    write: function (e) {
                        YarnSourceNameobj = new ej.dropdowns.DropDownList({
                            dataSource: masterData.YarnSourceNameList,
                            fields: { value: 'id', text: 'text' },
                            change: function (f) {
                                //
                                technicalNameObj.enabled = true;
                                //var tempQuery = new ej.data.Query().where('additionalValue', 'equal', machineTypeObj.value);
                                //technicalNameObj.query = tempQuery;
                                technicalNameObj.text = null;
                                technicalNameObj.dataBind();

                                e.rowData.YarnSourceID = f.itemData.id;
                                e.rowData.ValueName = f.itemData.text;
                            },
                            placeholder: 'Select one',
                            floatLabelType: 'Never'
                        });
                        YarnSourceNameobj.appendTo(YarnSourceNameElem);
                    }
                }
            },
            //{
            //    field: 'LengthYds', headerText: 'Length (Yds)', width: 85, allowEditing: false
            //},
            //{
            //    field: 'LengthInch', headerText: 'Length (Inch)', width: 85, allowEditing: false
            //},
            {
                field: 'DyeingType', headerText: 'Dyeing Type', width: 85, allowEditing: false
            },
            {
                field: 'Instruction', headerText: 'Instruction', allowEditing: false
            },
            {
                field: 'LabDipNo', headerText: 'Lab Dip No', allowEditing: false
            },
            //{
            //    field: 'ForBDSStyleNo', headerText: 'Style No', allowEditing: false
            //},
            {
                field: 'BookingQty', headerText: 'Booking Qty', width: 85, allowEditing: false
            },
            {
                field: 'TotalQty', headerText: 'Total Qty', width: 85, allowEditing: false, visible: status == statusConstants.COMPLETED
            }
            //{
            //    field: 'ConsumptionQty', headerText: 'Consumption Qty', width: 85, allowEditing: false
            //},
            //{
            //    field: 'Height', headerText: 'Height', width: 85, allowEditing: false
            //},
            //{
            //    field: 'Description', headerText: 'Description', width: 85, allowEditing: false
            //},
        ];
        var additionalColumns = [
            {
                field: 'DeliveredQty', headerText: 'Delivered Qty(kg/pcs)', width: 85, allowEditing: false, visible: status == statusConstants.APPROVED
            },
            {
                field: 'DelivereyComplete', headerText: 'Is Delivered?', displayAsCheckBox: true, textAlign: 'Center', visible: status == statusConstants.APPROVED
            }
        ]
        columns.push.apply(columns, additionalColumns);
        var childColumns = [
            { field: 'YBChildItemID', isPrimaryKey: true, visible: false },
            { field: 'ShadeCode', headerText: 'Shade Code', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'Distribution', headerText: 'Distribution', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'BookingQty', headerText: 'Booking Qty', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'Allowance', headerText: 'Allowance', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'StitchLength', headerText: 'Stitch Length', width: 40, allowEditing: true, editType: "numericedit", params: { decimals: 0, format: "N", min: 0, validateDecimalOnType: true } },
            { field: 'Specification', headerText: 'Specification', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks', textAlign: 'Center', width: 40, allowEditing: false },
        ];

        ej.base.enableRipple(true);
        if (_isBDS == 2) {
            $tblChildCollarIdEl = new ej.grids.Grid({
                dataSource: data,
                allowResizing: true,
                columns: columns,
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                recordClick: function (args) {
                    if (args.column && args.column.field == "TotalDays") {
                        _oRowCollar = args.rowData;
                        _indexCollar = args.rowIndex;
                        _modalFrom = subGroupNames.COLLAR;
                        // initPlanningTable(_oRowCollar.FBAChildPlannings, _oRowCollar.CriteriaIDs);
                        initCriteriaIDTable(_oRowCollar.CriteriaNames, _oRowCollar.FBAChildPlannings, _oRowCollar.FBAChildPlanningsWithIds, _oRowCollar.BookingChildID);
                        $modalCriteriaEl.modal('show');
                    }
                },
                actionBegin: function (args) {
                    if (args.requestType === "save") {
                        args.data = setArgDataValues(args.data, args.rowData);
                    }
                },
                childGrid: {
                    queryString: (status == statusConstants.PENDING || status == statusConstants.REJECT) ? 'YBChildID' : 'BookingChildID',
                    allowResizing: true,
                    autofitColumns: false,
                    editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: false },
                    columns: childColumns,
                    load: loadYarnBookingChildItems
                }
            });
        } else {
            $tblChildCollarIdEl = new ej.grids.Grid({
                dataSource: data,
                allowResizing: true,
                columns: columns,
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                recordClick: function (args) {
                    if (args.column && args.column.field == "TotalDays") {
                        _oRowCollar = args.rowData;
                        _indexCollar = args.rowIndex;
                        _modalFrom = subGroupNames.COLLAR;
                        // initPlanningTable(_oRowCollar.FBAChildPlannings, _oRowCollar.CriteriaIDs);
                        initCriteriaIDTable(_oRowCollar.CriteriaNames, _oRowCollar.FBAChildPlannings, _oRowCollar.FBAChildPlanningsWithIds, _oRowCollar.BookingChildID);
                        $modalCriteriaEl.modal('show');
                    }
                },
                actionBegin: function (args) {
                    if (args.requestType === "save") {
                        args.data = setArgDataValues(args.data, args.rowData);
                    }
                },
                enableContextMenu: true,
                contextMenuItems: [
                    { text: 'Copy TNA', target: '.e-content', id: 'copy' },
                    { text: 'Paste TNA', target: '.e-content', id: 'paste' },
                    { text: 'Paste TNA To All', target: '.e-content', id: 'pasteAll' }
                ],
                contextMenuClick: function (args) {
                    if (args.item.id === 'copy') {
                        itemTNAInfoCollar = objectCopy(args.rowInfo.rowData);
                        if (itemTNAInfoCollar.length == 0) {
                            toastr.error("No TNA information found to copy!!");
                            return;
                        }
                    }
                    else if (args.item.id === 'paste') {
                        var rowIndex = args.rowInfo.rowIndex;
                        if (itemTNAInfoCollar == null || itemTNAInfoCollar.length == 0) {
                            toastr.error("Please copy first!!");
                            return;
                        } else {
                            var pasteObject = objectCopy(itemTNAInfoCollar);
                            var preSubContactDays = 0,
                                subContactDays = 0;
                            //if (pasteObject.IsSubContact) preSubContactDays = 14;
                            //if (args.rowInfo.rowData.IsSubContact) subContactDays = 14;
                            //args.rowInfo.rowData.TotalDays = pasteObject.TotalDays - pasteObject.StructureDays + args.rowInfo.rowData.StructureDays - preSubContactDays + subContactDays;

                            args.rowInfo.rowData.MachineTypeId = pasteObject.MachineTypeId;
                            args.rowInfo.rowData.MachineType = pasteObject.MachineType;
                            args.rowInfo.rowData.KTypeId = pasteObject.KTypeId;
                            args.rowInfo.rowData.TechnicalNameId = pasteObject.TechnicalNameId;
                            args.rowInfo.rowData.TechnicalName = pasteObject.TechnicalName;
                            args.rowInfo.rowData.TechnicalTime = pasteObject.TechnicalTime;
                            args.rowInfo.rowData.IsSubContact = pasteObject.IsSubContact;

                            //args.rowInfo.rowData = setTotalDaysAndDeliveryDate(args.rowInfo.rowData, args.rowInfo.rowData.CriteriaNames);
                            //var techTypeDesc = 0;
                            //var techType = masterData.TechnicalNameList.find(y => y.id == pasteObject.TechnicalNameId);
                            //if (typeof techType !== "undefined" && techType != null) techTypeDesc = parseInt(techType.desc) + parseInt(pasteObject.IsSubContact ? 14 : 0);
                            //args.rowInfo.rowData.TotalDays += parseInt(techTypeDesc);

                            args.rowInfo.rowData.TotalDays = pasteObject.TotalDays;

                            var dt = new Date();
                            dt.setDate(dt.getDate() + args.rowInfo.rowData.TotalDays);
                            args.rowInfo.rowData.DeliveryDate = dt;
                            args.rowInfo.rowData.CriteriaIDs = pasteObject.CriteriaIDs;

                            _oRow = args.rowInfo.rowData;
                            updateCriteriaIDTable(_oRow, pasteObject);
                            //$tblChildCollarIdEl.refresh();
                            $tblChildCollarIdEl.updateRow(args.rowInfo.rowIndex, _oRow);
                        }
                    }
                    else if (args.item.id === 'pasteAll') {
                        var rowIndex = args.rowInfo.rowIndex;
                        if (itemTNAInfoCollar == null || itemTNAInfoCollar.length == 0) {
                            toastr.error("Please copy first!!");
                            return;
                        } else {
                            var rows = $tblChildCollarIdEl.getCurrentViewRecords();
                            for (var i = 0; i < rows.length; i++) {

                                var pasteObject = objectCopy(itemTNAInfoCollar);
                                //var preSubContactDays = 0,
                                //    subContactDays = 0;
                                //if (pasteObject.IsSubContact) preSubContactDays = 14;
                                //if (rows[i].IsSubContact) subContactDays = 14;
                                //rows[i].TotalDays = pasteObject.TotalDays - pasteObject.StructureDays + rows[i].StructureDays - preSubContactDays + subContactDays;

                                rows[i].MachineTypeId = pasteObject.MachineTypeId;
                                rows[i].MachineType = pasteObject.MachineType;
                                rows[i].KTypeId = pasteObject.KTypeId;
                                rows[i].TechnicalNameId = pasteObject.TechnicalNameId;
                                rows[i].TechnicalName = pasteObject.TechnicalName;
                                rows[i].TechnicalTime = pasteObject.TechnicalTime;
                                rows[i].IsSubContact = pasteObject.IsSubContact;

                                //rows[i] = setTotalDaysAndDeliveryDate(rows[i], rows[i].CriteriaNames);
                                //var techTypeDesc = 0;
                                //var techType = masterData.TechnicalNameList.find(y => y.id == pasteObject.TechnicalNameId);
                                //if (typeof techType !== "undefined" && techType != null) techTypeDesc = parseInt(techType.desc) + parseInt(pasteObject.IsSubContact ? 14 : 0);
                                //rows[i].TotalDays += parseInt(techTypeDesc);

                                rows[i].TotalDays = pasteObject.TotalDays;

                                var dt = new Date();
                                dt.setDate(dt.getDate() + rows[i].TotalDays);
                                rows[i].DeliveryDate = dt;
                                rows[i].CriteriaIDs = pasteObject.CriteriaIDs;

                                updateCriteriaIDTable(rows[i], pasteObject);


                            }
                            initChildCollar(rows);

                        }
                    }
                }
            });
        }
        $tblChildCollarIdEl.refreshColumns;
        $tblChildCollarIdEl.appendTo(tblChildCollarId);
        //$tblChildCollarIdEl.autoFitColumns();
    }

    function addDays(date, days) {
        var result = new Date(date);
        result.setDate(result.getDate() + days);
        return result;
    }

    function setTotalDaysAndDeliveryDate(currentData, criteriaNames) {
        var techTypeDesc = 0;
        var totalDays = 0;
        criteriaNames.map(y => {
            totalDays += parseInt(y.TotalTime);
        });
        var techType = masterData.TechnicalNameList.find(y => y.id == currentData.TechnicalNameId);
        if (typeof techType !== "undefined" && techType != null) techTypeDesc = parseInt(techType.desc) + parseInt(currentData.IsSubContact ? 14 : 0);
        currentData.TotalDays = parseInt(techTypeDesc) + totalDays;
        currentData.DeliveryDate = currentData.TotalDays > 0 ? addDays(new Date(), currentData.TotalDays) : "";
        return currentData;
    }

    async function initChildCuff1(data) {
        data = setCalculatedValues(data);
        if ($tblChildCuffIdEl) $tblChildCuffIdEl.destroy();
        var columns = [
            { field: 'ConsumptionID', isPrimaryKey: true, visible: false },
            { field: 'BookingID', visible: false },
            { field: 'ItemMasterID', visible: false },
            { field: 'SubGroupID', visible: false },
            { field: 'ConceptTypeID', visible: false },
            {
                field: 'MachineType', headerText: 'Machine Type ', visible: status != statusConstants.ACTIVE || _isBDS == 2, edit: {
                    create: function () {
                        machineTypeElem = document.createElement('input');
                        return machineTypeElem;
                    },
                    read: function () {
                        return machineTypeObj.text;
                    },
                    destroy: function () {
                        machineTypeObj.destroy();
                    },
                    write: function (e) {
                        machineTypeObj = new ej.dropdowns.DropDownList({
                            dataSource: masterData.MCTypeForOtherList,
                            fields: { value: 'id', text: 'text' },
                            change: function (f) {
                                technicalNameObj.enabled = true;
                                var tempQuery = new ej.data.Query().where('additionalValue', 'equal', machineTypeObj.value);
                                technicalNameObj.query = tempQuery;
                                technicalNameObj.text = null;
                                technicalNameObj.dataBind();

                                e.rowData.MachineTypeId = f.itemData.id;
                                e.rowData.MachineType = f.itemData.text;
                                e.rowData.KTypeId = f.itemData.desc;
                                e.rowData = setTotalDaysAndDeliveryDate(e.rowData, e.rowData.CriteriaNames);
                            },
                            placeholder: 'Select M/C Type',
                            floatLabelType: 'Never'
                        });
                        machineTypeObj.appendTo(machineTypeElem);
                    }
                }
            },
            {
                field: 'TechnicalName', headerText: 'Technical Name', visible: status != statusConstants.ACTIVE || _isBDS == 2, edit: {
                    create: function () {
                        technicalNameElem = document.createElement('input');
                        return technicalNameElem;
                    },
                    read: function () {
                        return technicalNameObj.text;
                    },
                    destroy: function () {
                        technicalNameObj.destroy();
                    },
                    write: function (e) {
                        technicalNameObj = new ej.dropdowns.DropDownList({
                            dataSource: masterData.TechnicalNameList,
                            fields: { value: 'id', text: 'text' },
                            enabled: false,
                            placeholder: 'Select Technical Name',
                            floatLabelType: 'Never',
                            change: function (f) {
                                if (!f.isInteracted || !f.itemData) return false;
                                e.rowData.TechnicalTime = parseInt(f.itemData.desc);
                                e.rowData.TechnicalNameId = f.itemData.id;
                                e.rowData.TechnicalName = f.itemData.text;
                                e.rowData = setTotalDaysAndDeliveryDate(e.rowData, e.rowData.CriteriaNames);
                                //$tblChildCuffIdEl.updateRow(e.row.rowIndex, e.rowData);
                            }
                        });
                        technicalNameObj.appendTo(technicalNameElem);
                    }
                }
            },
            {
                field: 'MachineGauge', headerText: 'Gauge', visible: _isBDS == 2, width: 80, allowEditing: true, editType: "numericedit", params: { decimals: 0, format: "N", min: 0, validateDecimalOnType: true }
            },
            {
                field: 'Brand', headerText: 'Brand', visible: _isBDS == 2, edit: {
                    create: function () {
                        brandElem = document.createElement('input');
                        return brandElem;
                    },
                    read: function () {
                        return brandObj.text;
                    },
                    destroy: function () {
                        brandObj.destroy();
                    },
                    write: function (e) {
                        brandObj = new ej.dropdowns.DropDownList({
                            dataSource: masterData.KnittingMachines,
                            fields: { value: 'id', text: 'text' },
                            change: function (f) {
                                e.rowData.BrandID = f.itemData.id;
                                e.rowData.Brand = f.itemData.text;
                            },
                            placeholder: 'Select Brand',
                            floatLabelType: 'Never'
                        });
                        brandObj.appendTo(brandElem);
                    }
                }
            },
            {
                field: 'IsSubContact', headerText: 'Sub-Contact?', visible: status != statusConstants.ACTIVE && _isBDS == 1, displayAsCheckBox: true, editType: "booleanedit", width: 85, textAlign: 'Center'
            },
            {
                field: 'TotalDays', headerText: 'Total Days', visible: status != statusConstants.ACTIVE && _isBDS == 1, allowEditing: false, textAlign: 'center', width: 85, valueAccessor: diplayPlanningCriteria
            },
            {
                field: 'StructureDays', headerText: 'Structure Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
            },
            {
                field: 'FinishingDays', headerText: 'Finishing Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
            },
            {
                field: 'DyeingDays', headerText: 'Dyeing Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
            },
            {
                field: 'BatchPreparationDays', headerText: 'Batch Preparation Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
            },
            {
                field: 'KnittingDays', headerText: 'Knitting Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
            },
            {
                field: 'TestReportDays', headerText: 'Test Report Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
            },
            {
                field: 'MaterialDays', headerText: 'Material Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
            },
            {
                field: 'QualityDays', headerText: 'Quality Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
            },
            {
                field: 'DeliveryDate', headerText: 'Delivery Date', visible: status != statusConstants.ACTIVE && _isBDS == 1, textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false
            },
            {
                field: 'Color', headerText: 'Color', width: 85, allowEditing: false
            },
            {
                field: 'YarnType', headerText: 'Yarn Type', width: 85, allowEditing: false
            },
            {
                field: 'YarnProgram', headerText: 'Yarn Program', width: 85, allowEditing: false
            },
            {
                field: 'ReferenceSourceName', headerText: 'Reference Source', visible: _isBDS == 1 ? true : false, width: 85, allowEditing: false
            },
            {
                field: 'ReferenceNo', headerText: 'Reference No', visible: _isBDS == 1 ? true : false, width: 85, allowEditing: false
            },
            {
                field: 'ColorReferenceNo', headerText: 'ColorReference No', visible: _isBDS == 1 ? true : false, width: 85, allowEditing: false
            },
            {
                field: 'ValueName', headerText: 'Yarn Source', visible: false/* status != statusConstants.ACTIVE*/, edit: {
                    create: function () {
                        YarnSourceNameElem = document.createElement('input');
                        return YarnSourceNameElem;
                    },
                    read: function () {
                        return YarnSourceNameobj.text;
                    },
                    destroy: function () {
                        YarnSourceNameobj.destroy();
                    },
                    write: function (e) {
                        YarnSourceNameobj = new ej.dropdowns.DropDownList({
                            dataSource: masterData.YarnSourceNameList,
                            fields: { value: 'id', text: 'text' },
                            change: function (f) {
                                //
                                technicalNameObj.enabled = true;
                                //var tempQuery = new ej.data.Query().where('additionalValue', 'equal', machineTypeObj.value);
                                //technicalNameObj.query = tempQuery;
                                technicalNameObj.text = null;
                                technicalNameObj.dataBind();

                                e.rowData.YarnSourceID = f.itemData.id;
                                e.rowData.ValueName = f.itemData.text;
                            },
                            placeholder: 'Select one',
                            floatLabelType: 'Never'
                        });
                        YarnSourceNameobj.appendTo(YarnSourceNameElem);
                    }
                }
            },
            //{
            //    field: 'LengthYds', headerText: 'Length (Yds)', width: 85, allowEditing: false
            //},
            //{
            //    field: 'LengthInch', headerText: 'Length (Inch)', width: 85, allowEditing: false
            //},
            {
                field: 'DyeingType', headerText: 'Dyeing Type', width: 85, allowEditing: false
            },
            {
                field: 'Instruction', headerText: 'Instruction', allowEditing: false
            },
            {
                field: 'LabDipNo', headerText: 'Lab Dip No', allowEditing: false
            },
            //{
            //    field: 'ForBDSStyleNo', headerText: 'Style No', allowEditing: false
            //},
            {
                field: 'BookingQty', headerText: 'Booking Qty', width: 85, allowEditing: false
            },
            {
                field: 'TotalQty', headerText: 'Total Qty', width: 85, allowEditing: false, visible: status == statusConstants.COMPLETED
            }
            //{
            //    field: 'ConsumptionQty', headerText: 'Consumption Qty', width: 85, allowEditing: false
            //},
            //{
            //    field: 'Height', headerText: 'Height', width: 80, allowEditing: false
            //},
            //{
            //    field: 'Description', headerText: 'Description', width: 85, allowEditing: false
            //},
        ];
        var additionalColumns = [
            {
                field: 'DeliveredQty', headerText: 'Delivered Qty(kg/pcs)', width: 85, allowEditing: false, visible: status == statusConstants.APPROVED
            },
            {
                field: 'DelivereyComplete', headerText: 'Is Delivered?', displayAsCheckBox: true, textAlign: 'Center', visible: status == statusConstants.APPROVED
            }
        ]
        columns.push.apply(columns, additionalColumns);
        var childColumns = [
            { field: 'YBChildItemID', isPrimaryKey: true, visible: false },
            { field: 'ShadeCode', headerText: 'Shade Code', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'Distribution', headerText: 'Distribution', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'BookingQty', headerText: 'Booking Qty', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'Allowance', headerText: 'Allowance', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'StitchLength', headerText: 'Stitch Length', width: 40, allowEditing: true, editType: "numericedit", params: { decimals: 0, format: "N", min: 0, validateDecimalOnType: true } },
            { field: 'Specification', headerText: 'Specification', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks', textAlign: 'Center', width: 40, allowEditing: false },
        ];
        ej.base.enableRipple(true);
        if (_isBDS == 2) {
            $tblChildCuffIdEl = new ej.grids.Grid({
                dataSource: data,
                allowResizing: true,
                columns: columns,
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                recordClick: function (args) {
                    if (args.column && args.column.field == "TotalDays") {
                        _oRowCuff = args.rowData;
                        _indexCuff = args.rowIndex;
                        _modalFrom = subGroupNames.CUFF;
                        // initPlanningTable(_oRowCuff.FBAChildPlannings, _oRowCuff.CriteriaIDs);
                        initCriteriaIDTable(_oRowCuff.CriteriaNames, _oRowCuff.FBAChildPlannings, _oRowCuff.FBAChildPlanningsWithIds, _oRowCuff.BookingChildID);
                        $modalCriteriaEl.modal('show');
                    }
                },
                actionBegin: function (args) {
                    if (args.requestType === "save") {
                        args.data = setArgDataValues(args.data, args.rowData);
                    }
                },
                childGrid: {
                    queryString: (status == statusConstants.PENDING || status == statusConstants.REJECT) ? 'YBChildID' : 'BookingChildID',
                    allowResizing: true,
                    autofitColumns: false,
                    editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: false },
                    columns: childColumns,
                    load: loadYarnBookingChildItems
                }
            });
        } else {
            $tblChildCuffIdEl = new ej.grids.Grid({
                dataSource: data,
                allowResizing: true,
                columns: columns,
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                recordClick: function (args) {
                    if (args.column && args.column.field == "TotalDays") {
                        _oRowCuff = args.rowData;
                        _indexCuff = args.rowIndex;
                        _modalFrom = subGroupNames.CUFF;
                        // initPlanningTable(_oRowCuff.FBAChildPlannings, _oRowCuff.CriteriaIDs);
                        initCriteriaIDTable(_oRowCuff.CriteriaNames, _oRowCuff.FBAChildPlannings, _oRowCuff.FBAChildPlanningsWithIds, _oRowCuff.BookingChildID);
                        $modalCriteriaEl.modal('show');
                    }
                },
                actionBegin: function (args) {
                    if (args.requestType === "save") {
                        args.data = setArgDataValues(args.data, args.rowData);
                    }
                },
                enableContextMenu: true,
                contextMenuItems: [
                    { text: 'Copy TNA', target: '.e-content', id: 'copy' },
                    { text: 'Paste TNA', target: '.e-content', id: 'paste' },
                    { text: 'Paste TNA To All', target: '.e-content', id: 'pasteAll' }
                ],
                contextMenuClick: function (args) {
                    if (args.item.id === 'copy') {
                        itemTNAInfoCollar = objectCopy(args.rowInfo.rowData);
                        if (itemTNAInfoCollar.length == 0) {
                            toastr.error("No TNA information found to copy!!");
                            return;
                        }
                    }
                    else if (args.item.id === 'paste') {
                        var rowIndex = args.rowInfo.rowIndex;
                        if (itemTNAInfoCollar == null || itemTNAInfoCollar.length == 0) {
                            toastr.error("Please copy first!!");
                            return;
                        } else {
                            var pasteObject = objectCopy(itemTNAInfoCollar);
                            var preSubContactDays = 0,
                                subContactDays = 0;
                            //if (pasteObject.IsSubContact) preSubContactDays = 14;
                            //if (args.rowInfo.rowData.IsSubContact) subContactDays = 14;
                            //args.rowInfo.rowData.TotalDays = pasteObject.TotalDays - pasteObject.StructureDays + args.rowInfo.rowData.StructureDays - preSubContactDays + subContactDays;

                            args.rowInfo.rowData.MachineTypeId = pasteObject.MachineTypeId;
                            args.rowInfo.rowData.MachineType = pasteObject.MachineType;
                            args.rowInfo.rowData.KTypeId = pasteObject.KTypeId;
                            args.rowInfo.rowData.TechnicalNameId = pasteObject.TechnicalNameId;
                            args.rowInfo.rowData.TechnicalName = pasteObject.TechnicalName;
                            args.rowInfo.rowData.TechnicalTime = pasteObject.TechnicalTime;
                            args.rowInfo.rowData.IsSubContact = pasteObject.IsSubContact;

                            //args.rowInfo.rowData = setTotalDaysAndDeliveryDate(args.rowInfo.rowData, args.rowInfo.rowData.CriteriaNames);
                            //var techTypeDesc = 0;
                            //var techType = masterData.TechnicalNameList.find(y => y.id == pasteObject.TechnicalNameId);
                            //if (typeof techType !== "undefined" && techType != null) techTypeDesc = parseInt(techType.desc) + parseInt(pasteObject.IsSubContact ? 14 : 0);
                            //args.rowInfo.rowData.TotalDays += parseInt(techTypeDesc);

                            args.rowInfo.rowData.TotalDays = pasteObject.TotalDays;

                            var dt = new Date();
                            dt.setDate(dt.getDate() + args.rowInfo.rowData.TotalDays);
                            args.rowInfo.rowData.DeliveryDate = dt;
                            args.rowInfo.rowData.CriteriaIDs = pasteObject.CriteriaIDs;

                            _oRow = args.rowInfo.rowData;
                            updateCriteriaIDTable(_oRow, pasteObject);
                            //$tblChildCuffIdEl.refresh();
                            $tblChildCuffIdEl.updateRow(args.rowInfo.rowIndex, _oRow);
                        }
                    }
                    else if (args.item.id === 'pasteAll') {
                        var rowIndex = args.rowInfo.rowIndex;
                        if (itemTNAInfoCollar == null || itemTNAInfoCollar.length == 0) {
                            toastr.error("Please copy first!!");
                            return;
                        } else {

                            var rows = $tblChildCuffIdEl.getCurrentViewRecords();
                            for (var i = 0; i < rows.length; i++) {

                                var pasteObject = objectCopy(itemTNAInfoCollar);
                                //var preSubContactDays = 0,
                                //    subContactDays = 0;
                                //if (pasteObject.IsSubContact) preSubContactDays = 14;
                                //if (rows[i].IsSubContact) subContactDays = 14;
                                //rows[i].TotalDays = pasteObject.TotalDays - pasteObject.StructureDays + rows[i].StructureDays - preSubContactDays + subContactDays;

                                rows[i].MachineTypeId = pasteObject.MachineTypeId;
                                rows[i].MachineType = pasteObject.MachineType;
                                rows[i].KTypeId = pasteObject.KTypeId;
                                rows[i].TechnicalNameId = pasteObject.TechnicalNameId;
                                rows[i].TechnicalName = pasteObject.TechnicalName;
                                rows[i].TechnicalTime = pasteObject.TechnicalTime;
                                rows[i].IsSubContact = pasteObject.IsSubContact;

                                //rows[i] = setTotalDaysAndDeliveryDate(rows[i], rows[i].CriteriaNames);
                                //var techTypeDesc = 0;
                                //var techType = masterData.TechnicalNameList.find(y => y.id == pasteObject.TechnicalNameId);
                                //if (typeof techType !== "undefined" && techType != null) techTypeDesc = parseInt(techType.desc) + parseInt(pasteObject.IsSubContact ? 14 : 0);
                                //rows[i].TotalDays += parseInt(techTypeDesc);

                                rows[i].TotalDays = pasteObject.TotalDays;

                                var dt = new Date();
                                dt.setDate(dt.getDate() + rows[i].TotalDays);
                                rows[i].DeliveryDate = dt;
                                rows[i].CriteriaIDs = pasteObject.CriteriaIDs;

                                updateCriteriaIDTable(rows[i], pasteObject);

                            }
                            initChildCuff(rows);

                        }
                    }
                }
            });
        }
        $tblChildCuffIdEl.refreshColumns;
        $tblChildCuffIdEl.appendTo(tblChildCuffId);
        //$tblChildCuffIdEl.autoFitColumns();
    }

    async function initPlanningTable(data) {
        var unitNameKG = "kg";
        var unitNamePCS = "Pcs";

        data = DeepClone(data);
        data.map(x => {
            x.TotalValue = x.TillLiabilityQty * x.Rate;
            x.TotalValue = getDefaultValueWhenInvalidN_Float(x.TotalValue);

            if (_modalFrom == subGroupNames.FABRIC) x.UnitName = unitNameKG;

            else if (_modalFrom == subGroupNames.COLLAR && x.LiabilitiesProcessID == 2250) x.UnitName = unitNameKG; //Dyed Yarn
            else if (_modalFrom == subGroupNames.COLLAR && x.LiabilitiesProcessID == 1715) x.UnitName = unitNamePCS; //Finished Qty
            else if (_modalFrom == subGroupNames.COLLAR && x.LiabilitiesProcessID == 1714) x.UnitName = unitNamePCS; //Grey Qty
            else if (_modalFrom == subGroupNames.COLLAR && x.LiabilitiesProcessID == 2122) x.UnitName = unitNameKG; //Yarn Qty

            else if (_modalFrom == subGroupNames.CUFF && x.LiabilitiesProcessID == 2250) x.UnitName = unitNameKG; //Dyed Yarn
            else if (_modalFrom == subGroupNames.CUFF && x.LiabilitiesProcessID == 1715) x.UnitName = unitNamePCS; //Finished Qty
            else if (_modalFrom == subGroupNames.CUFF && x.LiabilitiesProcessID == 1714) x.UnitName = unitNamePCS; //Grey Qty
            else if (_modalFrom == subGroupNames.CUFF && x.LiabilitiesProcessID == 2122) x.UnitName = unitNameKG; //Yarn Qty

        });
        if ($tblPlanningEl) $tblPlanningEl.destroy();
        ej.base.enableRipple(true);
        var columns = [
            { field: 'LiabilitiesProcessID', visible: false, isPrimaryKey: true },
            { field: 'BookingChildID', visible: false },
            { field: 'AcknowledgeID', visible: false },
            { field: 'BookingID', visible: false },
            { field: 'UnitID', visible: false },
            { field: 'LiabilitiesName', headerText: 'Process Name', allowEditing: false },
            { field: 'UnitName', headerText: 'Unit', allowEditing: false, textAlign: 'Center' },
            { field: 'LiabilityQty', headerText: 'Liability Qty', allowEditing: statusConstants.REVISE == status },
            { field: 'ConsumedQty', headerText: 'Consumed Qty', allowEditing: statusConstants.REVISE == status },
            { field: 'TillLiabilityQty', headerText: 'Till Liability', allowEditing: false },
            { field: 'Rate', headerText: 'Rate', allowEditing: statusConstants.REVISE == status },
            { field: 'TotalValue', headerText: 'Value', allowEditing: false }
        ];

        var tableOptions = {
            dataSource: data,
            allowResizing: true,
            editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            actionBegin: function (args) {
                if (args.requestType === "save") {
                    var obj = DeepClone(getLiabilityQtys(args.data, args.rowData, null));
                    args.data = DeepClone(obj);
                }
            },
        };
        tableOptions["columns"] = columns;
        $tblPlanningEl = new ej.grids.Grid(
            tableOptions
        );
        $tblPlanningEl.refreshColumns;
        $tblPlanningEl.appendTo(tblPlanningId);
    }

    function getLiabilityQtys(obj, previousData, selectedYarnRow) {

        if (isYarnType(obj.LiabilitiesName)) {
            var totalYarns = 0;
            var totalValue = 0;

            var yarns = $tblChildYarnEl.getCurrentViewRecords();
            yarns = yarns.filter(x => x.YD == false);
            if (selectedYarnRow != null) {
                var indexF = yarns.findIndex(x => x.ChildID == selectedYarnRow.ChildID);
                yarns[indexF] = selectedYarnRow;
            }
            yarns.map(x => {
                totalYarns += parseFloat(x.LiabilityQty);
                x.Rate = x.Rate == null || typeof x.Rate === "undefined" ? 0 : x.Rate;
                totalValue += parseFloat(x.LiabilityQty * x.Rate);
            });
            obj.LiabilityQty = totalYarns;
            obj.TotalValue = totalValue.toFixed(2);
            //obj.Rate = 0;
        }
        else if (isDyedYarnType(obj.LiabilitiesName)) {
            var totalYarns = 0;
            var totalValue = 0;

            var yarns = $tblChildYarnEl.getCurrentViewRecords();
            yarns = yarns.filter(x => x.YD == true);
            if (selectedYarnRow != null) {
                var indexF = yarns.findIndex(x => x.ChildID == selectedYarnRow.ChildID);
                yarns[indexF] = selectedYarnRow;
            }
            yarns.map(x => {
                totalYarns += parseFloat(x.LiabilityQty);
                x.Rate = x.Rate == null || typeof x.Rate === "undefined" ? 0 : x.Rate;
                totalValue += parseFloat(x.LiabilityQty * x.Rate);
            });
            obj.LiabilityQty = totalYarns;
            obj.TotalValue = totalValue.toFixed(2);
            //obj.Rate = 0;
        }
        else if (obj.LiabilitiesName == _liabilitiesType.FinishedQty || obj.LiabilitiesName == _liabilitiesType.GreyQty) {
            var totalValue = 0;
            var processList = $tblPlanningEl.getCurrentViewRecords();

            processList.filter(x => x.LiabilitiesName == obj.LiabilitiesName).map(x => {
                if (obj.LiabilitiesName == _liabilitiesType.FinishedQty) {
                    var objFQ = getValidFinishQty(obj);
                    if (!objFQ.IsValid) {
                        obj.LiabilityQty = previousData.LiabilityQty;
                        obj.ConsumedQty = previousData.ConsumedQty;
                        obj.TillLiabilityQty = previousData.TillLiabilityQty;
                    }
                    else {
                        obj.LiabilityQty = objFQ.LiabilityQty;
                        obj.ConsumedQty = objFQ.ConsumedQty;
                        obj.TillLiabilityQty = objFQ.TillLiabilityQty;
                    }
                }
                totalValue += parseFloat(obj.LiabilityQty * obj.Rate);
            });
            obj.TotalValue = totalValue.toFixed(2);
        }

        obj.TillLiabilityQty = parseFloat(obj.LiabilityQty) - parseFloat(obj.ConsumedQty);
        obj.TillLiabilityQty = getDefaultValueWhenInvalidN_Float(obj.TillLiabilityQty);
        if (isYarnType(obj.LiabilitiesName) || isDyedYarnType(obj.LiabilitiesName)) {
            obj.TotalValue = obj.Rate * obj.TillLiabilityQty;
        }
        obj.TotalValue = getDefaultValueWhenInvalidN_Float(obj.TotalValue);

        return obj;
    }

    async function initCriteriaIDTable(data, criteriaData, savedData, childId) {
        if (childId) {
            data.forEach(function (d) {
                var obj = savedData.find(function (el) { return d.CriteriaName == el.CriteriaName });
                if (obj) {
                    d.TotalTime = obj.TotalTime;
                    d.CriteriaIDs = obj.CriteriaIDs;
                }
            });
        }

        if ($tblCriteriaIdEl) $tblCriteriaIdEl.destroy();

        ej.base.enableRipple(true);
        $tblCriteriaIdEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,

            columns: [
                { field: 'CriteriaName', headerText: 'Criteria Name', allowEditing: false },
                { field: 'TotalTime', headerText: 'Process Time', allowEditing: false, valueAccessor: diplayPlanningCriteria }
            ],
            editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            recordClick: function (args) {
                CriteriaName = (args.rowData.CriteriaName);
                if (args.column && args.column.field == "TotalTime" && CriteriaName != 'Batch Preparation' && CriteriaName != 'Quality Check') {
                    _oRowc = args.rowData;
                    _indexc = args.rowIndex;

                    var pChild = $.grep(criteriaData, function (h) { return h.CriteriaName == CriteriaName });
                    initPlanningTable(pChild, _oRowc.CriteriaIDs);
                    $modalPlanningEl.modal('show');
                }
            }
        });
        $tblCriteriaIdEl.refreshColumns;
        $tblCriteriaIdEl.appendTo(tblCriteriaId);
    }

    function backToListBulk() {
        //initMasterTable();
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
    }

    function backToList() {
        //initMasterTable();
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#FBAckID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getNew(bookingNo) {
        $formEl.find(".divUnAcknowledgeReason").hide();
        var url = `/api/fab-acknowledge/bulk/new/${bookingNo}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;

                masterData.BookingDate = formatDateToDefault(masterData.BookingDate);
                setFormData($formEl, masterData);

                // initChild(masterData.FBookingChild)
                if (masterData.HasFabric) {
                    //initChildTableAsync($tblFabricChildEl, tblFabricChildId, subGroupNames.FABRIC);
                    //bmtArray

                    if (bmtArray.length > 0) {
                        var newFBookingChild = [];
                        masterData.FBookingChild.map(x => {
                            var constructionId = x.ConstructionId;
                            if (constructionId != null && constructionId > 0) {
                                var bmtObj = bmtArray.find(x => x.ConstructionID == constructionId);
                                if (isValidValue(bmtObj)) {
                                    var technicalNameId = x.Composition.toUpperCase().indexOf("ELASTANE") > -1 ? bmtObj.TechnicalNameID_Elastane : bmtObj.TechnicalNameID;

                                    x.MachineTypeId = bmtObj.SubClassID;
                                    if (bmtObj.SubClassID > 0) {
                                        x.MachineType = masterData.MCTypeForFabricList.find(x => x.id == bmtObj.SubClassID).text;
                                        x.KTypeId = masterData.MCTypeForFabricList.find(x => x.id == bmtObj.SubClassID).desc;
                                    }

                                    if (technicalNameId != '0') {
                                        x.TechnicalNameId = technicalNameId
                                        x.TechnicalName = masterData.TechnicalNameList.find(x => x.id == technicalNameId).text;
                                    }
                                }
                            }
                            newFBookingChild.push(x);
                        });
                        masterData.FBookingChild = newFBookingChild;
                    }
                    initChild(masterData.FBookingChild);
                    initYarnChildTableAsync(masterData.FBookingAcknowledgementYarnLiabilityList);
                    $formEl.find("#divFabricInfo").show();
                }
                else $formEl.find("#divFabricInfo").hide();

                if (masterData.HasCollar) {
                    //initChildTableAsync($tblCollarChildEl, tblCollarChildId, subGroupNames.COLLAR);
                    initChildCollar(masterData.FBookingChildCollor);
                    $formEl.find("#divCollarInfo").show();
                }
                else $formEl.find("#divCollarInfo").hide();

                if (masterData.HasCuff) {
                    //initChildTableAsync($tblCuffChildEl, tblCuffChildId, subGroupNames.CUFF);
                    initChildCuff(masterData.FBookingChildCuff);
                    $formEl.find("#divCufInfo").show();
                }
                else $formEl.find("#divCufInfo").hide();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getNewBulk(bookingNo) {
        $formEl.find(".divUnAcknowledgeReason").hide();
        axios.get(`/api/yarn-booking/forBulk/${bookingNo}`)
            .then(function (response) {
                masterData = response.data;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData.YBookingDate = formatDateToDefault(masterData.YBookingDate);
                masterData.BookingDate = formatDateToDefault(masterData.BookingDate);
                setFormData($formEl, masterData);

                if (masterData.HasFabric) {
                    initChild(masterData.Fabrics);
                    $formEl.find("#divFabricInfo").show();
                }
                else $formEl.find("#divFabricInfo").hide();

                if (masterData.HasCollar) {
                    initChildCollar(masterData.Collars);
                    $formEl.find("#divCollarInfo").show();
                }
                else $formEl.find("#divCollarInfo").hide();

                if (masterData.HasCuff) {
                    initChildCuff(masterData.Cuffs);
                    $formEl.find("#divCufInfo").show();
                }
                else $formEl.find("#divCufInfo").hide();
            })
            .catch(showResponseError);
    }

    function isValidValue(value) {
        if (typeof value === "undefined" || value == null) return false;
        return true;
    }
    function setQtyChangeStatus(list, subGroupID, isRevised) {
        if (isRevised) {
            var tempList = _previousBUChilds.filter(x => x.SubGroupID == subGroupID && x.Status != 'Child Deleted'); //Qty Changed & New Child
            tempList.map(x => {
                var indexF = list.findIndex(y => y.BookingChildID == x.BookingChildID);
                if (indexF > -1) {
                    list[indexF].Status = x.Status;
                }
            });
        }
        return list;
    }

    function getView(bookingNo, withoutOB) {
        $formEl.find(".divUnAcknowledgeReason").hide();
        _previousBUChilds = [];
        var isRevised = false;
        if (status == statusConstants.REVISE) isRevised = true;
        withoutOB = withoutOB ? "1" : "0";
        var url = "/api/fab-acknowledge/bulk/slist/" + bookingNo + "/" + withoutOB + "/" + isRevised;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.BookingDate = formatDateToDefault(masterData.BookingDate);

                setFormData($formEl, masterData);
                
                if (isRevised) _previousBUChilds = masterData.ChangesChilds;

                if (masterData.HasFabric) {
                    masterData.FBookingChild = setQtyChangeStatus(masterData.FBookingChild, 1, isRevised);
                    initChild(masterData.FBookingChild);
                    $formEl.find("#divFabricInfo").show();
                }
                else $formEl.find("#divFabricInfo").hide();

                if (masterData.HasCollar) {
                    masterData.FBookingChildCollor = setQtyChangeStatus(masterData.FBookingChildCollor, 11, isRevised);
                    initChildCollar(masterData.FBookingChildCollor);
                    $formEl.find("#divCollarInfo").show();
                }
                else $formEl.find("#divCollarInfo").hide();

                if (masterData.HasCuff) {
                    masterData.FBookingChildCuff = setQtyChangeStatus(masterData.FBookingChildCuff, 12, isRevised);
                    initChildCuff(masterData.FBookingChildCuff);
                    $formEl.find("#divCufInfo").show();
                }
                else $formEl.find("#divCufInfo").hide();

                if (status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE || status == statusConstants.COMPLETED) {
                    _previousBUChilds = [];
                    masterData.FBookingChild.filter(x => x.BookingQty == 0).map(x => {
                        x.Status = "Child Deleted";
                        x.SubGroupID = 1;
                        x.Segment1Desc = getDefaultValueWhenInvalidS(x.Segment1Desc) == "" ? x.Construction : x.Segment1Desc;
                        x.Segment2Desc = getDefaultValueWhenInvalidS(x.Segment2Desc) == "" ? x.Composition : x.Segment2Desc;
                        x.Segment3Desc = getDefaultValueWhenInvalidS(x.Segment3Desc) == "" ? x.Color : x.Segment3Desc;
                        x.Segment4Desc = getDefaultValueWhenInvalidS(x.Segment4Desc) == "" ? x.GSM : x.Segment4Desc;
                        x.Segment5Desc = getDefaultValueWhenInvalidS(x.Segment5Desc) == "" ? x.FabricWidth : x.Segment5Desc;
                        x.Segment6Desc = "";
                        x.Segment7Desc = x.KnittingType;
                        _previousBUChilds.push(x);
                    });
                    masterData.FBookingChildCollor.filter(x => x.BookingQty == 0).map(x => {
                        x.Status = "Child Deleted";
                        x.SubGroupID = 11;
                        x.Segment1Desc = getDefaultValueWhenInvalidS(x.Segment1Desc) == "" ? x.Description : x.Segment1Desc;
                        x.Segment2Desc = getDefaultValueWhenInvalidS(x.Segment2Desc) == "" ? x.CollarType : x.Segment2Desc;
                        x.Segment3Desc = getDefaultValueWhenInvalidS(x.Segment3Desc) == "" ? x.Length : x.Segment3Desc;
                        x.Segment4Desc = getDefaultValueWhenInvalidS(x.Segment4Desc) == "" ? x.Height : x.Segment4Desc;
                        x.Segment5Desc = getDefaultValueWhenInvalidS(x.Segment5Desc) == "" ? x.BodyColor : x.Segment5Desc;
                        x.Segment6Desc = "";
                        x.Segment7Desc = "";
                        _previousBUChilds.push(x);
                    });
                    masterData.FBookingChildCuff.filter(x => x.BookingQty == 0).map(x => {
                        x.Status = "Child Deleted";
                        x.SubGroupID = 12;
                        x.Segment1Desc = getDefaultValueWhenInvalidS(x.Segment1Desc) == "" ? x.Description : x.Segment1Desc;
                        x.Segment2Desc = getDefaultValueWhenInvalidS(x.Segment2Desc) == "" ? x.CollarType : x.Segment2Desc;
                        x.Segment3Desc = getDefaultValueWhenInvalidS(x.Segment3Desc) == "" ? x.Length : x.Segment3Desc;
                        x.Segment4Desc = getDefaultValueWhenInvalidS(x.Segment4Desc) == "" ? x.Height : x.Segment4Desc;
                        x.Segment5Desc = getDefaultValueWhenInvalidS(x.Segment5Desc) == "" ? x.BodyColor : x.Segment5Desc;
                        x.Segment6Desc = "";
                        x.Segment7Desc = "";
                        _previousBUChilds.push(x);
                    });
                }

                displayDeletedItems();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function displayDeletedItems() {
        $formEl.find(".deletedItemTable").hide();
        if (status == statusConstants.REVISE) {
            $formEl.find('.divColorDescription').show();
        }
        if (_previousBUChilds.length > 0) {
            var subGroupIds = [1, 11, 12];

            $formEl.find(".btnDeletedItemShow").off('click');

            subGroupIds.map(sg => {
                var list = _previousBUChilds.filter(pc => pc.SubGroupID == sg && pc.Status == "Child Deleted");
                if (list.length > 0) {
                    if (sg == 1) {
                        initChildDI(list);
                    }
                    else if (sg == 11) {
                        initChildCollarDI(list);
                    }
                    else if (sg == 12) {
                        initChildCuffDI(list);

                    }
                    $formEl.find(".btnDeletedItemShow[group=" + sg + "]").show();
                    $formEl.find(".btnDeletedItemShow[group=" + sg + "]").click(function () {
                        if ($formEl.find(".dTable[group=" + sg + "]").is(':visible')) {
                            $formEl.find(".dTable[group=" + sg + "]").hide();
                        } else {
                            $formEl.find(".dTable[group=" + sg + "]").show();
                        }
                    });
                }
            });
        }
    }
    async function initChildDI(data) {
        data = data.filter(x => x.Status == "Child Deleted");
        data.map(x => {
            x.DiffPreAndBookingQty = x.BookingQty - x.PreviousBookingQty;
        });
        data = setCalculatedValues(data);
        if ($tblChildElDI) $tblChildElDI.destroy();
        var columns = [
            { field: 'ConsumptionID', isPrimaryKey: true, visible: false },
            { field: 'BookingChildID', visible: false },

            {
                field: 'Segment1Desc', headerText: 'Construction', width: 60, allowEditing: false
            },
            {
                field: 'Segment2Desc', headerText: 'Composition', width: 120, allowEditing: false
            },
            {
                field: 'Segment3Desc', headerText: 'Color', width: 70, allowEditing: false
            },
            {
                field: 'Segment4Desc', headerText: 'GSM', width: 50, allowEditing: false
            },
            {
                field: 'Segment5Desc', headerText: 'Fabric Width', width: 60, allowEditing: false
            },
            {
                field: 'Segment7Desc', headerText: 'Knitting Type', width: 60, allowEditing: false
            },
            {
                field: 'PreviousBookingQty', headerText: 'Previous Booking Qty', visible: actionName == 'Add' ? false : true, width: 60, textAlign: 'right', allowEditing: false
            },
            {
                field: 'BookingQty', headerText: 'Booking Qty', width: 60, textAlign: 'right', allowEditing: false, valueAccessor: displayBookingQty
            },
            {
                field: 'DiffPreAndBookingQty', headerText: 'Qty Different', visible: actionName == 'Add' ? false : true, textAlign: 'center', width: 85
            },
            {
                field: 'LiabilitiesBookingQty', headerText: 'Liabilities Booking Qty', visible: actionName == 'Add' ? false : true, textAlign: 'center', width: 85, valueAccessor: diplayPlanningCriteria, allowEditing: false
            },
            {
                field: 'ActualBookingQty', headerText: 'Actual Booking Qty', visible: false, width: 85, allowEditing: false
            },
            {
                field: 'BookingUOM', headerText: 'Booking UOM', width: 60, textAlign: 'left', allowEditing: false
            },
            {
                field: 'RefSourceNo', headerText: 'Ref No', visible: true, width: 85, allowEditing: false
            },
            {
                headerText: '', textAlign: 'Center', width: 40, visible: true, commands: [
                    { buttonOption: { type: 'findRefSourceNo', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Ref Detail" } }
                ]
            },
            {
                field: 'Instruction', headerText: 'Instruction', allowEditing: false, width: 450
            },
            {
                field: 'BookingChildID', headerText: 'GMT BookingChildID', visible: false
            }
        ];
        $tblChildElDI = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            columns: columns,
            rowDataBound: rowDataBoundDI,
            recordClick: function (args) {
                if (args.column && args.column.field == "LiabilitiesBookingQty") {
                    if (args.rowData.DiffPreAndBookingQty < 0) {
                        _oRow = args.rowData;
                        _index = args.rowIndex;
                        _modalFrom = subGroupNames.FABRIC;

                        setItemDetails({
                            lbl1: "Construction",
                            lbl1Value: args.rowData.Segment1Desc,
                            lbl2: "Composition",
                            lbl2Value: args.rowData.Segment2Desc,
                            lbl3: "Color",
                            lbl3Value: args.rowData.Segment3Desc,
                            lbl4: "GSM",
                            lbl4Value: args.rowData.Segment4Desc,
                            lbl5: "Fabric Width",
                            lbl5Value: args.rowData.Segment5Desc,
                            lbl6: "Knitting Type",
                            lbl6Value: args.rowData.Segment7Desc,
                            lbl7: "Pre Booking Qty",
                            lbl7Value: args.rowData.PreviousBookingQty,
                            lbl8: "Current Booking Qty",
                            lbl8Value: args.rowData.BookingQty,
                            lbl9: "Different Qty",
                            lbl9Value: getDefaultValueWhenInvalidN_Float(args.rowData.BookingQty) - getDefaultValueWhenInvalidN_Float(args.rowData.PreviousBookingQty),
                            lbl10: "Knitting Prod Qty",
                            lbl10Value: 0,
                            lbl11: "Dyeing Prod Qty",
                            lbl11Value: 0,
                            lbl12: "Finish Fabric Stock Qty",
                            lbl12Value: args.rowData.TotalFinishFabricStockQty,
                            lbl13: "Total YD Prod.",
                            lbl13Value: 0,
                            lbl14: "Delivered Qty",
                            lbl14Value: args.rowData.DeliveredQtyForLiability
                        }, 1);

                        initPlanningTable(args.rowData.ChildAckLiabilityDetails);
                        initYarnChildTableAsync(args.rowData.ChildAckYarnLiabilityDetails);
                        //initCriteriaIDTable(_oRow.CriteriaNames, _oRow.FBAChildPlannings, _oRow.FBAChildPlanningsWithIds, _oRow.BookingChildID);
                        $modalPlanningEl.modal('show');
                    }
                }
            },
        });
        $tblChildElDI.refreshColumns;
        $tblChildElDI.appendTo(tblChildIdDI);
    }
    function rowDataBoundDI(args) {
        if (args.data.Status == 'Child Deleted') {
            args.row.style.backgroundColor = "#F9A695";
            args.row.style.color = "#000000";
        }
    }

    async function initChildCollarDI(data) {
        data = data.filter(x => x.Status == "Child Deleted");
        data.map(x => {
            x.DiffPreAndBookingQty = x.BookingQty - x.PreviousBookingQty;
        });
        data = setCalculatedValues(data);
        if ($tblChildCollarIdElDI) $tblChildCollarIdElDI.destroy();
        var columns = [
            { field: 'ConsumptionID', isPrimaryKey: true, visible: false },
            { field: 'BookingChildID', visible: false },
            {
                field: 'Segment1Desc', headerText: 'Collar Description', width: 60, allowEditing: false
            },
            {
                field: 'Segment2Desc', headerText: 'Collar Type', width: 120, allowEditing: false
            },
            {
                field: 'Segment3Desc', headerText: 'Length(cm)', width: 70, allowEditing: false
            },
            {
                field: 'Segment4Desc', headerText: 'Height(cm)', width: 50, allowEditing: false
            },
            {
                field: 'Segment5Desc', headerText: 'Body Color', width: 60, allowEditing: false
            },
            {
                field: 'YarnType', headerText: 'Yarn Type', width: 85, allowEditing: false
            },
            {
                field: 'YarnProgram', headerText: 'Yarn Program', width: 85, allowEditing: false
            },
            {
                field: 'PreviousBookingQty', headerText: 'Previous Booking Qty', visible: actionName == 'Add' ? false : true, width: 60, textAlign: 'right', allowEditing: false
            },
            {
                field: 'BookingQty', headerText: 'Booking Qty', width: 60, textAlign: 'right', allowEditing: false, valueAccessor: displayBookingQty
            },
            {
                field: 'DiffPreAndBookingQty', headerText: 'Qty Different', visible: actionName == 'Add' ? false : true, textAlign: 'center', width: 85
            },
            {
                field: 'LiabilitiesBookingQty', headerText: 'Liabilities Booking Qty', visible: actionName == 'Add' ? false : true, textAlign: 'center', width: 85, valueAccessor: diplayPlanningCriteria, allowEditing: false
            },
            {
                field: 'ActualBookingQty', headerText: 'Actual Booking Qty', visible: false, width: 85, allowEditing: false
            },
            {
                field: 'BookingUOM', headerText: 'Booking UOM', width: 60, textAlign: 'left', allowEditing: false
            },
            {
                field: 'Instruction', headerText: 'Instruction', allowEditing: false
            },
            {
                field: 'BookingChildID', headerText: 'GMT BookingChildID', visible: false
            }
        ];
        $tblChildCollarIdElDI = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            columns: columns,
            recordClick: function (args) {
                if (args.column && args.column.field == "LiabilitiesBookingQty") {
                    if (args.rowData.DiffPreAndBookingQty < 0) {
                        _oRow = args.rowData;
                        _index = args.rowIndex;
                        _modalFrom = subGroupNames.COLLAR;

                        setItemDetails({
                            lbl1: "Collar Description",
                            lbl1Value: args.rowData.Segment1Desc,
                            lbl2: "Collar Type",
                            lbl2Value: args.rowData.Segment2Desc,
                            lbl3: "Length(cm)",
                            lbl3Value: args.rowData.Segment3Desc,
                            lbl4: "Height(cm)",
                            lbl4Value: args.rowData.Segment4Desc,
                            lbl5: "Body Color",
                            lbl5Value: args.rowData.Segment5Desc,
                            lbl6: "Yarn Type",
                            lbl6Value: args.rowData.KnittingType,
                            lbl7: "Pre Booking Qty",
                            lbl7Value: args.rowData.PreviousBookingQty,
                            lbl8: "Current Booking Qty",
                            lbl8Value: args.rowData.BookingQty,
                            lbl9: "Different Qty",
                            lbl9Value: getDefaultValueWhenInvalidN_Float(args.rowData.BookingQty) - getDefaultValueWhenInvalidN_Float(args.rowData.PreviousBookingQty),
                            lbl10: "Total YD Prod.",
                            lbl10Value: 0,
                            lbl11: "Finish Fabric Stock Qty",
                            lbl11Value: args.rowData.TotalFinishFabricStockQty,
                            lbl12: "Delivered Qty",
                            lbl12Value: args.rowData.DeliveredQtyForLiability
                        }, 11);

                        args.rowData.ChildAckLiabilityDetails.forEach(x => {
                            if (x.LiabilitiesName == 'Yarn') {
                                var childAckYarnLiabilityDetails = args.rowData.ChildAckYarnLiabilityDetails;
                                var sum = 0;

                                for (var i = 0; i < childAckYarnLiabilityDetails.length; i++) {
                                    var item = childAckYarnLiabilityDetails[i];
                                    sum += item.LiabilityQty;
                                }
                                x.LiabilityQty = sum;
                            }
                        });

                        initPlanningTable(args.rowData.ChildAckLiabilityDetails);
                        initYarnChildTableAsync(args.rowData.ChildAckYarnLiabilityDetails);
                        //initCriteriaIDTable(_oRow.CriteriaNames, _oRow.FBAChildPlannings, _oRow.FBAChildPlanningsWithIds, _oRow.BookingChildID);
                        $modalPlanningEl.modal('show');
                    }
                }
            },
            rowDataBound: rowDataBoundDI
        });
        $tblChildCollarIdElDI.refreshColumns;
        $tblChildCollarIdElDI.appendTo(tblChildCollarIdDI);
    }

    async function initChildCuffDI(data) {
        data = data.filter(x => x.Status == "Child Deleted");
        data.map(x => {
            x.DiffPreAndBookingQty = x.BookingQty - x.PreviousBookingQty;
        });
        data = setCalculatedValues(data);
        if ($tblChildCuffIdElDI) $tblChildCuffIdElDI.destroy();
        var columns = [
            { field: 'ConsumptionID', isPrimaryKey: true, visible: false },
            { field: 'BookingChildID', visible: false },
            { field: 'ItemMasterID', visible: false },
            {
                field: 'Segment1Desc', headerText: 'Cuff Description', width: 60, allowEditing: false
            },
            {
                field: 'Segment2Desc', headerText: 'Cuff Type', width: 120, allowEditing: false
            },
            {
                field: 'Segment3Desc', headerText: 'Length(cm)', width: 70, allowEditing: false
            },
            {
                field: 'Segment4Desc', headerText: 'Height(cm)', width: 50, allowEditing: false
            },
            {
                field: 'Segment5Desc', headerText: 'Body Color', width: 60, allowEditing: false
            },
            {
                field: 'YarnType', headerText: 'Yarn Type', width: 85, allowEditing: false
            },
            {
                field: 'YarnProgram', headerText: 'Yarn Program', width: 85, allowEditing: false
            },
            {
                field: 'PreviousBookingQty', headerText: 'Previous Booking Qty', visible: actionName == 'Add' ? false : true, width: 60, textAlign: 'right', allowEditing: false
            },
            {
                field: 'BookingQty', headerText: 'Booking Qty', width: 60, textAlign: 'right', allowEditing: false, valueAccessor: displayBookingQty
            },
            {
                field: 'DiffPreAndBookingQty', headerText: 'Qty Different', visible: actionName == 'Add' ? false : true, textAlign: 'center', width: 85
            },
            {
                field: 'LiabilitiesBookingQty', headerText: 'Liabilities Booking Qty', visible: actionName == 'Add' ? false : true, textAlign: 'center', width: 85, valueAccessor: diplayPlanningCriteria, allowEditing: false
            },
            {
                field: 'ActualBookingQty', headerText: 'Actual Booking Qty', visible: false, width: 85, allowEditing: false
            },
            {
                field: 'BookingUOM', headerText: 'Booking UOM', width: 60, textAlign: 'left', allowEditing: false
            },
            {
                field: 'Instruction', headerText: 'Instruction', allowEditing: false
            },
            {
                field: 'BookingChildID', headerText: 'GMT BookingChildID', visible: false
            }
        ];
        $tblChildCuffIdElDI = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            columns: columns,
            recordClick: function (args) {
                if (args.column && args.column.field == "LiabilitiesBookingQty") {
                    if (args.rowData.DiffPreAndBookingQty < 0) {
                        _oRow = args.rowData;
                        _index = args.rowIndex;
                        _modalFrom = subGroupNames.CUFF;

                        setItemDetails({
                            lbl1: "Cuff Description",
                            lbl1Value: args.rowData.Segment1Desc,
                            lbl2: "Cuff Type",
                            lbl2Value: args.rowData.Segment2Desc,
                            lbl3: "Length(cm)",
                            lbl3Value: args.rowData.Segment3Desc,
                            lbl4: "Height(cm)",
                            lbl4Value: args.rowData.Segment4Desc,
                            lbl5: "Body Color",
                            lbl5Value: args.rowData.Segment5Desc,
                            lbl6: "Yarn Type",
                            lbl6Value: args.rowData.KnittingType,
                            lbl7: "Pre Booking Qty",
                            lbl7Value: args.rowData.PreviousBookingQty,
                            lbl8: "Current Booking Qty",
                            lbl8Value: args.rowData.BookingQty,
                            lbl9: "Different Qty",
                            lbl9Value: getDefaultValueWhenInvalidN_Float(args.rowData.BookingQty) - getDefaultValueWhenInvalidN_Float(args.rowData.PreviousBookingQty),
                            lbl10: "Total YD Prod.",
                            lbl10Value: 0,
                            lbl11: "Finish Fabric Stock Qty",
                            lbl11Value: args.rowData.TotalFinishFabricStockQty,
                            lbl12: "Delivered Qty",
                            lbl12Value: args.rowData.DeliveredQtyForLiability
                        }, 12);

                        initPlanningTable(args.rowData.ChildAckLiabilityDetails);
                        initYarnChildTableAsync(args.rowData.ChildAckYarnLiabilityDetails);
                        $modalPlanningEl.modal('show');
                    }
                }
            },
            rowDataBound: rowDataBoundDI
        });
        $tblChildCuffIdElDI.refreshColumns;
        $tblChildCuffIdElDI.appendTo(tblChildCuffIdDI);
    }

    function sendMail(bookingNo, withoutOB) {
        var SaveType = 'S';
        if (status == statusConstants.UN_ACKNOWLEDGE)
            SaveType = 'UA'
        withoutOB = withoutOB ? "1" : "0";

        var listTypeMasterGrid = getListTypeMasterGrid();

        var url = "/api/fab-acknowledge/bulk/smail/" + bookingNo + "/" + withoutOB + "/" + SaveType + "/" + listTypeMasterGrid;
        axios.get(url)
            .then(function (response) {

                if (response.data) {
                    toastr.success('Mail has been sent');
                }
                else {
                    toastr.error('Mail not sent properly!!!');
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function checkRevisionStatus(bookingNo, ExportOrderNo, SubGroupID) {
        var url = "/api/fab-acknowledge/bulk/list/" + ExportOrderNo + "/" + SubGroupID;
        axios.get(url)
            .then(function (response) {
                var rData = response.data;
                if (rData.BOMStatus == 'BookingRevisionPending') {
                    toastr.error('Awaiting for Booking revision of the Export Work Order: ' + ExportOrderNo);
                }
                else {
                    getNew(bookingNo)
                    $formEl.find("#btnSave,#btnOkk,#btnUnAcknowledge").fadeIn();
                    $formEl.find("#btnReceived,#btnCancelAcknowledge,#btnCancelUnAcknowledge").fadeOut();
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function checkRevisionStatusView(bookingNo, ExportOrderNo, SubGroupID, WithoutOB) {
        $formEl.find(".divUnAcknowledgeReason").hide();
        var url = "/api/fab-acknowledge/bulk/list/" + ExportOrderNo + "/" + SubGroupID;
        axios.get(url)
            .then(function (response) {
                var rData = response.data;
                if (rData.BOMStatus == 'BookingRevisionPending') {
                    toastr.error('Awaiting for Booking revision of the Export Work Order: ' + ExportOrderNo);
                }
                else {
                    getView(bookingNo, WithoutOB);
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnSave,#btnUnAcknowledge,#btnReceived,#btnOkk,#btnCancelAcknowledge,#btnCancelUnAcknowledge").fadeOut();
                }

                $formEl.find(".divRevisionNo").show();
                if (status == statusConstants.UN_ACKNOWLEDGE) {
                    $formEl.find(".divUnAcknowledgeReason").show();
                    $formEl.find(".divRevisionNo").hide();
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function checkPropValues(obj) {
        if (isNaN(obj.BrandID)) obj.BrandID = 0;
        //if (obj.BrandID == 0 && masterData.KnittingMachines.length > 0) obj.BrandID = (masterData.KnittingMachines.find(x => x.text == obj.Brand)).id;
        if (isNaN(obj.BatchPreparationDays)) obj.BatchPreparationDays = 0;
        if (isNaN(obj.DyeingDays)) obj.DyeingDays = 0;
        if (isNaN(obj.FinishingDays)) obj.FinishingDays = 0;
        if (isNaN(obj.KnittingDays)) obj.KnittingDays = 0;
        if (isNaN(obj.MaterialDays)) obj.MaterialDays = 0;
        if (isNaN(obj.StructureDays)) obj.StructureDays = 0;
        if (isNaN(obj.TestReportDays)) obj.TestReportDays = 0;
        if (isNaN(obj.TotalDays)) obj.TotalDays = 0;
        return obj;
    }


    function saveWithConfirm(result = "", isUnAcknowledge) {
        if (!isUnAcknowledge) {
            showBootboxConfirm("Confirm Acknowledge", "Are you sure you want to acknowledge?", function (yes) {
                if (yes) {
                    save(result, isUnAcknowledge);
                }
            });
        } else {
            save(result, isUnAcknowledge);
        }
    }

    function save(result = "", isUnAcknowledge) {
        
        var acknowledgeList = [];
        var liabilityDistributionList = [];
        var yarnLiabilityList = [];
        var data = formElToJson($formEl);
        data.SaveType = 'S';
        if (isUnAcknowledge) {
            data.SaveType = 'UA';
            data.UnAcknowledgeReason = result;
            data.IsUnAcknowledge = true;
        }


        if (masterData.HasFabric && result == "") {
            var fabrics = $tblChildEl.getCurrentViewRecords();
            acknowledgeList.push(...fabrics);
        }
        if (masterData.HasCollar && result == "") {
            var collars = $tblChildCollarIdEl.getCurrentViewRecords();
            acknowledgeList.push(...collars);
        }
        if (masterData.HasCuff && result == "") {
            var cuffs = $tblChildCuffIdEl.getCurrentViewRecords();
            acknowledgeList.push(...cuffs);
        }


        if (typeof $tblChildElDI !== "undefined" && result == "") {
            var fabrics = $tblChildElDI.getCurrentViewRecords();
            fabrics.map(x => {
                x.BookingQty = 0;
            });
            acknowledgeList.push(...fabrics);
        }
        if (typeof $tblChildCollarIdElDI !== "undefined" && result == "") {
            var collars = $tblChildCollarIdElDI.getCurrentViewRecords();
            collars.map(x => {
                x.BookingQty = 0;
            });
            acknowledgeList.push(...collars);
        }
        if (typeof $tblChildCuffIdElDI !== "undefined" && result == "") {
            var cuffs = $tblChildCuffIdElDI.getCurrentViewRecords();
            cuffs.map(x => {
                x.BookingQty = 0;
            });
            acknowledgeList.push(...cuffs);
        }


        //========= Get Liabilities=============

        if (typeof masterData != "undefined" && result == "" && status != 30) {
            var fabricList = masterData.FBookingChild;
            fabricList.map(i => {
                var filterData = i.ChildAckYarnLiabilityDetails.filter(x => x.AllocatedQty > 0 && x.LiabilityQty > 0);

                filterData.map(j => {
                    j.BookingChildID = i.BookingChildID;
                    j.ConsumptionID = i.ConsumptionID;
                    j.BookingID = i.BookingID;
                    j.AcknowledgeID = i.AcknowledgeID;
                    yarnLiabilityList.push(j);
                });

                var filterData1 = i.ChildAckLiabilityDetails.filter(x => x.LiabilityQty > 0);
                filterData1.map(j => {
                    j.BookingChildID = i.BookingChildID;
                    j.ConsumptionID = i.ConsumptionID;
                    j.BookingID = i.BookingID;
                    j.AcknowledgeID = i.AcknowledgeID;
                    j.UnitID = i.BookingUnitID;
                    liabilityDistributionList.push(j);
                });
                
            });

            var collarList = masterData.FBookingChildCollor;
            collarList.map(i => {
                var filterData = i.ChildAckYarnLiabilityDetails.filter(x => x.AllocatedQty > 0 && x.LiabilityQty > 0);

                filterData.map(j => {
                    j.BookingChildID = i.BookingChildID;
                    j.ConsumptionID = i.ConsumptionID;
                    j.BookingID = i.BookingID;
                    j.AcknowledgeID = i.AcknowledgeID;
                    yarnLiabilityList.push(j);
                });

                var filterData1 = i.ChildAckLiabilityDetails.filter(x => x.LiabilityQty > 0);
                filterData1.map(j => {
                    j.BookingChildID = i.BookingChildID;
                    j.ConsumptionID = i.ConsumptionID;
                    j.BookingID = i.BookingID;
                    j.AcknowledgeID = i.AcknowledgeID;
                    j.UnitID = i.BookingUnitID;
                    liabilityDistributionList.push(j);
                });

            });

            var cuffList = masterData.FBookingChildCuff;
            cuffList.map(i => {
                var filterData = i.ChildAckYarnLiabilityDetails.filter(x => x.AllocatedQty > 0 && x.LiabilityQty > 0);

                filterData.map(j => {
                    j.BookingChildID = i.BookingChildID;
                    j.ConsumptionID = i.ConsumptionID;
                    j.BookingID = i.BookingID;
                    j.AcknowledgeID = i.AcknowledgeID;
                    yarnLiabilityList.push(j);
                });

                var filterData1 = i.ChildAckLiabilityDetails.filter(x => x.LiabilityQty > 0);
                filterData1.map(j => {
                    j.BookingChildID = i.BookingChildID;
                    j.ConsumptionID = i.ConsumptionID;
                    j.BookingID = i.BookingID;
                    j.AcknowledgeID = i.AcknowledgeID;
                    j.UnitID = i.BookingUnitID;
                    liabilityDistributionList.push(j);
                });

            });

            var deletedChilds = masterData.ChangesChilds.filter(x => x.Status == 'Child Deleted');
            deletedChilds.map(i => {
                var filterData = i.ChildAckYarnLiabilityDetails.filter(x => x.AllocatedQty > 0 && x.LiabilityQty > 0);

                filterData.map(j => {
                    j.BookingChildID = i.BookingChildID;
                    j.ConsumptionID = i.ConsumptionID;
                    j.BookingID = i.BookingID;
                    j.AcknowledgeID = i.AcknowledgeID;
                    yarnLiabilityList.push(j);
                });

                var filterData1 = i.ChildAckLiabilityDetails.filter(x => x.LiabilityQty > 0);
                filterData1.map(j => {
                    j.BookingChildID = i.BookingChildID;
                    j.ConsumptionID = i.ConsumptionID;
                    j.BookingID = i.BookingID;
                    j.AcknowledgeID = i.AcknowledgeID;
                    j.UnitID = i.BookingUnitID;
                    liabilityDistributionList.push(j);
                });

            });
        }
        //========= End Get Liabilities=============

        data.WithoutOB = masterData.WithoutOB;
        data.SubGroupID = masterData.SubGroupID;
        data.ItemGroupId = masterData.ItemGroupId;
        data.FBAckID = getDefaultValueWhenInvalidN(data.FBAckID);
        data.BookingQty = getDefaultValueWhenInvalidN_Float(data.BookingQty);
        data.BookingBy = getDefaultValueWhenInvalidN(data.BookingBy);
        data.PreRevisionNo = getDefaultValueWhenInvalidN(data.PreRevisionNo);
        acknowledgeList.map(x => {
            x.YarnSubBrandID = getDefaultValueWhenInvalidN(x.YarnSubBrandID);
            x.LabdipUpdateDate = null;
        });
        data.FBookingAcknowledgeList = masterData.FBookingAcknowledgeList;
        data.FabricBookingAcknowledgeList = masterData.FabricBookingAcknowledgeList;
        data.FBookingChild = acknowledgeList;
        data.FBookingAckLiabilityDistributionList = liabilityDistributionList;
        data.FBookingAcknowledgementYarnLiabilityList = yarnLiabilityList.filter(x => x.AllocatedQty > 0);
        data.grpConceptNo = $formEl.find('#GroupConceptNo').val();
        data.IsBDS = _isBDS;

        data.IsRevised = status == statusConstants.REVISE ? true : false;
        data.PreRevisionNo = masterData.PreRevisionNo;

        data.PageName = pageName;
        data.ActionStatus = status;

        data.IsSample = masterData.IsSample;
        data.MenuId = menuId;

        data.ListTypeMasterGrid = getListTypeMasterGrid();

        
        axios.post("/api/fab-acknowledge/acknowledge", data)
            .then(function (response) {
                if (response.data) {
                    toastr.success("Acknowledged successfully.");
                }
                else {
                    toastr.error('Acknowledged but mail not sent properly!!!');
                }
                if (status == statusConstants.PENDING || status == statusConstants.NEW) {
                    $toolbarEl.find("#btnList").click();
                } else if (status == statusConstants.REVISE) {
                    $toolbarEl.find("#btnRevisionAckList").click();
                }
                $toolbarEl.find("#btnRefreshNotificationCount").click();
                backToListBulk();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function getListTypeMasterGrid() {
        if (status == statusConstants.NEW) {
            return "NAL"; //New Acknowledgement List
        } else if (status == statusConstants.REVISE) {
            return "RAL"; //Revision Acknowledgement List
        }
        return "A";
    }
    function Receive() {

        var data = formElToJson($formEl);
        
        axios.post("/api/bds-acknowledge/Receive", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToListBulk();
                //backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function Received() {
        var data = formElToJson($formEl);
        
        axios.post("/api/bds-acknowledge/Received", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToListBulk();
                //backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function CheckNull(value) {
        if (value == null || value == "") return 0;
        return value;
    }

    function setTechnicalTime(data) {
        var obj = masterData.TechnicalNameList.find(x => x.additionalValue == data.MachineTypeId
            && x.id == data.TechnicalNameId);
        if (obj != null) return obj.desc;
        return 0;
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

    function cancelSave(result = "") {
        var acknowledgeList = [];
        var data = formElToJson($formEl);
        if (result) {
            data["UnAcknowledgeReason"] = result;
            data["IsUnAcknowledge"] = true;
        }

        if (masterData.HasFabric && result == "") {
            let fabrics = $tblChildEl.getCurrentViewRecords();
            for (let i = 0; i < fabrics.length; i++) {
                if (_isBDS != 3) {
                    if (!fabrics[i].MachineTypeId) {
                        toastr.warning("Please enter machine type for each fabric!");
                        return;
                    }
                    if (!fabrics[i].TechnicalNameId) {
                        toastr.warning("Please enter technical name for each fabric!");
                        return;
                    }
                    if (_isBDS == 1) {
                        if (!fabrics[i].CriteriaIDs) {
                            toastr.warning("Please select criteria properly for each fabric!");
                            return;
                        }
                    }
                    if (_isBDS == 2) {
                        fabrics[i] = checkPropValues(fabrics[i]);
                        if (!fabrics[i].BrandID) {
                            toastr.warning("Select brand for each fabric!");
                            return;
                        }
                    }
                }
                acknowledgeList.push(fabrics[i]);
            }
        }
        if (masterData.HasCollar && result == "") {
            let collars = $tblChildCollarIdEl.getCurrentViewRecords();
            for (let i = 0; i < collars.length; i++) {
                if (_isBDS != 3) {
                    if (!collars[i].MachineTypeId) {
                        toastr.warning("Please enter machine type for each collar!");
                        return;
                    }
                    if (!collars[i].TechnicalNameId) {
                        toastr.warning("Please enter technical name for each collar!");
                        return;
                    }
                    if (_isBDS == 1) {
                        if (!collars[i].CriteriaIDs) {
                            toastr.warning("Please select criteria properly for each collar!");
                            return;
                        }
                    }
                    if (_isBDS == 2) {
                        collars[i] = checkPropValues(collars[i]);
                        if (!collars[i].BrandID) {
                            toastr.warning("Select brand for each collar!");
                            return;
                        }
                    }
                }
                acknowledgeList.push(collars[i]);
            }
        }
        if (masterData.HasCuff && result == "") {
            let cuffs = $tblChildCuffIdEl.getCurrentViewRecords();
            for (let i = 0; i < cuffs.length; i++) {
                if (_isBDS != 3) {
                    if (!cuffs[i].MachineTypeId) {
                        toastr.warning("Please enter machine type for each cuff!");
                        return;
                    }
                    if (!cuffs[i].TechnicalNameId) {
                        toastr.warning("Please enter technical name for each cuff!");
                        return;
                    }
                    if (_isBDS == 1) {
                        if (!cuffs[i].CriteriaIDs) {
                            toastr.warning("Please select criteria properly for each cuff!");
                            isValid = false;
                            break;
                        }
                    }
                    if (_isBDS == 2) {
                        cuffs[i] = checkPropValues(cuffs[i]);
                        if (!cuffs[i].BrandID) {
                            toastr.warning("Select brand for each cuff!");
                            return;
                        }
                    }
                }
                acknowledgeList.push(cuffs[i]);
            }
        }
        data.WithoutOB = masterData.WithoutOB;
        if (_isBDS == 2) {
            data.FBAckID = getDefaultValueWhenInvalidN(data.FBAckID);
            data.BookingQty = getDefaultValueWhenInvalidN_Float(data.BookingQty);
            data.BookingBy = getDefaultValueWhenInvalidN(data.BookingBy);
            data.PreRevisionNo = getDefaultValueWhenInvalidN(data.PreRevisionNo);
            acknowledgeList.map(x => {
                x.YarnSubBrandID = getDefaultValueWhenInvalidN(x.YarnSubBrandID),
                    x.LabdipUpdateDate = null;
            });
        }

        data.FBookingChild = acknowledgeList;
        data.grpConceptNo = $formEl.find('#GroupConceptNo').val();
        data.IsBDS = _isBDS;

        
        axios.post("/api/bds-acknowledge/cancel-save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToListBulk();
                //if (_isBDS == 2) backToListBulk();
                //else backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function getIntegerFromdecimal(value) {
        // Check if the value is a number
        if (!isNaN(value)) {
            // Convert to integer and format without decimal places
            return parseInt(value).toFixed(0);
        } else {
            // Return the original value if it's not a number
            return value.toFixed(0);
        }
    }
    function getFinishedQtyLiability(stockQty, deliveryQty, currentBookingQty, previousBookingQty) {

        stockQty = getDefaultValueWhenInvalidN_Float(stockQty);
        deliveryQty = getDefaultValueWhenInvalidN_Float(deliveryQty);
        currentBookingQty = getDefaultValueWhenInvalidN_Float(currentBookingQty);
        previousBookingQty = getDefaultValueWhenInvalidN_Float(previousBookingQty);

        var finishedQty = 0;

        if (currentBookingQty > previousBookingQty) finishedQty = 0;
        if (stockQty + deliveredQty > previousBookingQty) finishedQty = previousBookingQty - currentBookingQty;
        if (stockQty + deliveredQty > currentBookingQty) finishedQty = (stockQty + deliveredQty) - currentBookingQty;

        return getDefaultValueWhenInvalidN_Float(finishedQty);
    }
})();
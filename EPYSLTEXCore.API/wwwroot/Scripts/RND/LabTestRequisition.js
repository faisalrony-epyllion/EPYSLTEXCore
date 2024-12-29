(function () {
    var menuId, pageName, pageId, pageIdWithHash;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl,
        tblTestingRequirementId, $divItemInfo, $divTestingRequirement, $tblTestingRequirementEl,
        $divLabelCare, $tblLabelCare, tblLabelCareId, tblMasterId,
        $divOtherInformations, $tblExportCountries, $tblEndUses, $tblSpecialMethods,
        tblExportCountriesId, tblEndUsesId, tblSpecialMethodsId;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var status;

    var isRequisitionPage = false;
    var isApprovePage = false;
    var isAcknowledgePage = false;
    var isReqBulkPage = false;
    var isReqApproveBulkPage = false;
    var isReqAckBulkPage = false;
    var setTestNameList;
    var testNamelist=null;

    var masterData;
    var testingRequirementList = [];
    var maxLTReqCareLabelID = 99999;
    var isRetest = false;
    var testNatureObjForRetest = {
        TestNatureID: 0,
        TestNatureName: ""
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
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        tblTestingRequirementId = "#tblTestingRequirement" + pageId;
        tblLabelCareId = "#tblLabelCare" + pageId;
        tblExportCountriesId = "#tblExportCountries" + pageId;
        tblEndUsesId = "#tblEndUses" + pageId;
        tblSpecialMethodsId = "#tblSpecialMethods" + pageId;
        pageIdWithHash = "#" + pageId;

        tblTestingRequirementmodal = "#tblTestingRequirementmodal" + pageId;
        $divItemInfo = $(`#divItemInfo${pageId}`);
        $divTestingRequirement = $(`#divTestingRequirement${pageId}`);
        $divLabelCare = $(`#divLabelCare${pageId}`);
        $divOtherInformations = $(`#divOtherInformations${pageId}`);
        $modaltestrequirment = $(`#modaltestrequirment${pageId}`);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());
        isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val());
        isReqBulkPage = convertToBoolean($(`#${pageId}`).find("#ReqBulkPage").val());
        isReqApproveBulkPage = convertToBoolean($(`#${pageId}`).find("#ReqApproveBulkPage").val());
        isReqAckBulkPage = convertToBoolean($(`#${pageId}`).find("#ReqAckBulkPage").val());

        $formEl.find("#btnAddItem").fadeIn();

        if ((isApprovePage) || (isReqApproveBulkPage)) {
            status = statusConstants.COMPLETED;

            $toolbarEl.find("#btnCompleteList,#btnApproveList").fadeIn();
            $toolbarEl.find("#btnPending,#btnAcknowledgeList").fadeOut();
            $formEl.find("#ContactPersonID").prop("disabled", true);
            toggleActiveToolbarBtn($toolbarEl.find("#btnCompleteList"), $toolbarEl);
            $formEl.find("#btnAddItem").fadeOut();
        }
        else if ((isAcknowledgePage) || (isReqAckBulkPage)) {
            status = statusConstants.APPROVED;

            $toolbarEl.find("#btnApproveList,#btnAcknowledgeList").fadeIn();
            $toolbarEl.find("#btnPending,#btnCompleteList").fadeOut();
            $formEl.find("#ContactPersonID").prop("disabled", true);

            toggleActiveToolbarBtn($toolbarEl.find("#btnApproveList"), $toolbarEl);
            $formEl.find("#btnAddItem").fadeOut();
        } else {
            status = statusConstants.PENDING;

            $toolbarEl.find("#btnPending,#btnCompleteList").fadeIn();
            $toolbarEl.find("#btnApproveList,#btnAcknowledgeList").fadeIn();
            $formEl.find("#ContactPersonID").prop("disabled", false);

            toggleActiveToolbarBtn($toolbarEl.find("#btnPending"), $toolbarEl);
        }

        initMasterTable();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            $formEl.find("#btnUnAcknowledge").fadeOut();
            initMasterTable();
        });

        $toolbarEl.find("#btnCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;

            initMasterTable();
        });

        $toolbarEl.find("#btnApproveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.APPROVED;
            $formEl.find("#btnSave").fadeOut();
            $formEl.find("#btnUnAcknowledge").fadeIn();
            initMasterTable();
        });

        $toolbarEl.find("#btnAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.ACKNOWLEDGE;
            $formEl.find("#btnSave").fadeOut();
            $formEl.find("#btnUnAcknowledge").fadeOut();
            initMasterTable();
        });
        $toolbarEl.find("#btnUnAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.UN_ACKNOWLEDGE;
            $formEl.find("#btnSave").fadeOut();
            $formEl.find("#btnUnAcknowledge").fadeOut();
            
            initMasterTable();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnRevise").click(function (e) {
            e.preventDefault();
            revise();
        });

        $formEl.find("#btnApprove").click(function (e) {
            e.preventDefault();
            approve(true, false,false, null);
        });

        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            approve(true, true,false, null);
        });

        $formEl.find("#btnUnAcknowledge").click(function (e) {
            e.preventDefault();
            bootbox.prompt("Enter your UnAcknowledge reason:", function (result) {
                if (!result) {
                    return toastr.error("UnAcknowledge reason is required.");
                }
                approve(false, false,true,result);
            });
            
        });

        $formEl.find("#btnBack").on("click", backToListWithoutFilter);

        $formEl.find("#btnAddBuyer").click(function (e) {
            e.preventDefault();
            getBuyer();
        });

        $formEl.find("#IsProduction").change(function () {
            $formEl.find("#WashTemp").val(0).trigger('change');
            washTempVisibility(false);
            initTestingRequirementTable([]);
        });
        $formEl.find("#TestNatureID").change(function () {
            masterData.TestNatureID = $(this).val();
            masterData.TestNatureName = getTestNatureName();
        });

        function selectMatchingItems(selectedItems, allItems) {
            var dimensionalStabilityWashObj = {
                LineDry: 650,
                TumbleDry: 652,
                FlatDry: 653
            };

            var spiralityObj = {
                LineDry: 651,
                TumbleDry: 654,
                FlatDry: 655
            };
            var itemDLineDry = selectedItems.find(x => x.BPID == dimensionalStabilityWashObj.LineDry);
            var itemDTumbleDry = selectedItems.find(x => x.BPID == dimensionalStabilityWashObj.TumbleDry);
            var itemDFlatDry = selectedItems.find(x => x.BPID == dimensionalStabilityWashObj.FlatDry);
            var itemSLineDry = selectedItems.find(x => x.BPID == spiralityObj.LineDry);
            var itemSTumbleDry = selectedItems.find(x => x.BPID == spiralityObj.TumbleDry);
            var itemSFlatDry = selectedItems.find(x => x.BPID == spiralityObj.FlatDry);

            var item = null;
            if (itemDLineDry && !itemSLineDry) {
                item = allItems.find(x => x.BPID == spiralityObj.LineDry);
                selectedItems.push(item)
            }
            if (!itemDLineDry && itemSLineDry) {
                item = allItems.find(x => x.BPID == dimensionalStabilityWashObj.LineDry);
                selectedItems.push(item)
            }

            if (itemDTumbleDry && !itemSTumbleDry) {
                item = allItems.find(x => x.BPID == spiralityObj.TumbleDry);
                selectedItems.push(item)
            }
            if (!itemDTumbleDry && itemSTumbleDry) {
                item = allItems.find(x => x.BPID == dimensionalStabilityWashObj.TumbleDry);
                selectedItems.push(item)
            }

            if (itemDFlatDry && !itemSFlatDry) {
                item = allItems.find(x => x.BPID == spiralityObj.FlatDry);
                selectedItems.push(item)
            }
            if (!itemDFlatDry && itemSFlatDry) {
                item = allItems.find(x => x.BPID == dimensionalStabilityWashObj.FlatDry);
                selectedItems.push(item)
            }

            return selectedItems;
        }

        $formEl.find("#btnAddItem").on("click", function (e) {
            //API Calling
            e.preventDefault();

            var isProduction = $formEl.find('#IsProduction').is(":checked");
            var testNatureID = $formEl.find("#TestNatureID").val();
            var buyerID = masterData.BuyerID;
            if (isProduction) buyerID = 0;
            isProduction = isProduction ? 1 : 0;

            var currentList = $tblTestingRequirementEl.dataSource;
            var BPIDs = [];
            currentList.map(x => {
                BPIDs.push(x.BPID);
            });
            BPIDs = BPIDs.join(",");

            var finder = new commonFinder({
                title: "Select Testing Requirment",
                pageId: pageId,
                //data: list, //masterData.LabTestRequisitionBuyers[0].NewLabTestRequisitionBuyerParameters,
                apiEndPoint: `/api/lab-test-requisition/get-LabTestRequisition-BuyerParameters/${buyerID}/${testNatureID}/${isProduction}`,
                fields: "TestName,TestMethod,Requirement",
                headerTexts: "Testing Requirement,Test Method, Requirement",
                isMultiselect: true,
                primaryKeyColumn: "BPID",
                allowPaging: false,
                selectedIds: BPIDs,
                onMultiselect: async function (selectedRecords) {
                    selectedRecords.forEach(function (value) {
                        var BuyerName = masterData.Buyer;
                        findMatchedTestReqName(BuyerName,value);
                    })

                    await getSetList(buyerID, testNatureID, isProduction);
                    if (setTestNameList != null) {
                        selectedRecords.push(setTestNameList[0]);
                    }
                    if (isProduction) {
                        selectedRecords = selectMatchingItems(selectedRecords, currentList);
                    }
                    selectedRecords.sort(function (a, b) {
                        return b.TestName.localeCompare(a.TestName);
                    })
                    selectedRecords.forEach(function (item) {
                        var exists = $tblTestingRequirementEl.dataSource.find(function (el) { return el.BPID == item.BPID });
                        if (!exists) $tblTestingRequirementEl.dataSource.unshift(item);
                    });
                    $tblTestingRequirementEl.dataSource.map(x => {
                        x.TestNatureID = masterData.TestNatureID;
                        x.TestNatureName = getTestNatureName();
                    });
                    initTestingRequirementTable($tblTestingRequirementEl.dataSource);
                    $tblTestingRequirementEl.refresh();

                    setTestNatureDisplayType();
                }
            });
            finder.showModal();
        });
        //for care label
        $formEl.find("#btnCareLabel").on("click", function (e) {
            e.preventDefault();

            var finder = new commonFinder({
                title: "Add Care Label",
                pageId: pageId,
                apiEndPoint: "/api/lab-test-requisition/carelables/",
                fields: "CareType,CareName,ImagePath",
                headerTexts: "Care Type, Care Name, Icon",
                widths: "100,100,100",
                customFormats: ",,ej2GridImageFormatter",
                isMultiselect: true,
                primaryKeyColumn: "LCareLableID",
                allowPaging: false,
                onMultiselect: function (selectedRecords) {
                    var list = selectedRecords;// $tblLabelCare.dataSource;
                    //selectedRecords.forEach(function (item) {
                    //    var findIndex = list.findIndex(x => x.LCareLableID == item.LCareLableID);
                    //    if (findIndex == -1) {
                    //        item.LTReqCareLabelID = maxLTReqCareLabelID++;
                    //        list.push(item);
                    //    }
                    //});
                    $tblLabelCare.dataSource = list;
                    masterData.CareLabels = $tblLabelCare.dataSource;
                    var seqNo = 1;
                    masterData.CareLabels.map(x => {
                        x.SeqNo = seqNo++;
                    });
                    initLabelCareTable(masterData.CareLabels);
                    $tblLabelCare.refresh();
                }
            });
            finder.showModal();
        });
        //end for care label

        //for CareLabelCode
        $formEl.find("#btnCareLabelCode").on("click", function (e) {
            e.preventDefault();
            var buyerId = masterData.BuyerID;
            var finder = new commonFinder({
                title: "Add Care Label By Code",
                pageId: pageId,
                apiEndPoint: `/api/lab-test-requisition/careLableCodes/${buyerId}`,
                fields: "CareLableCode,GroupCode",
                headerTexts: "Care Lable Code, Code",
                widths: "100,100",
                isMultiselect: true,
                primaryKeyColumn: "LCLBuyerID",
                allowPaging: false,
                onMultiselect: function (selectedRecords) {
                    if (selectedRecords.length > 0) {
                        finder.hideModal();
                        var careLabelCodes = selectedRecords.map(x => x.CareLableCode).join(',');
                        if (careLabelCodes != null && careLabelCodes.length > 0) {
                            axios.get(`/api/lab-test-requisition/GetCareLebelsByCodes/multiple/${careLabelCodes}/${buyerId}`)
                                .then(function (response) {
                                    $tblLabelCare.dataSource = response.data;
                                    masterData.CareLabels = $tblLabelCare.dataSource;
                                    var seqNo = 1;
                                    masterData.CareLabels.map(x => {
                                        x.SeqNo = seqNo++;
                                    });
                                    initLabelCareTable(masterData.CareLabels);
                                    $tblLabelCare.refresh();
                                })
                                .catch(function (err) {
                                    toastr.error(err.response.data.Message);
                                });
                        }
                    }
                },
                /*
                onSelect: function (selectedRecord) {
                    finder.hideModal();
                    var careLabelCode = selectedRecord.rowData.CareLableCode;
                    if (careLabelCode != null && careLabelCode.trim().length > 0) {
                        axios.get(`/api/lab-test-requisition/GetCareLebelsByCode/${careLabelCode}`)
                            .then(function (response) {
                                $tblLabelCare.dataSource = response.data;
                                masterData.CareLabels = $tblLabelCare.dataSource;
                                var seqNo = 1;
                                masterData.CareLabels.map(x => {
                                    x.SeqNo = seqNo++;
                                });
                                initLabelCareTable(masterData.CareLabels);
                                $tblLabelCare.refresh();
                            })
                            .catch(function (err) {
                                toastr.error(err.response.data.Message);
                            });
                    }
                }
                */
            });
            finder.showModal();
        });
        //end CareLabelCode

        //for countries
        $formEl.find("#btnExportCountries").on("click", function (e) {
            e.preventDefault();
            if (masterData.CountryList.length == 0) {
                return toastr.error("No List Found");
            }
            var finder = new commonFinder({
                title: "Add Countries",
                pageId: pageId,
                data: masterData.CountryList,
                fields: "RegionName",
                headerTexts: "Region",
                widths: "100",
                isMultiselect: true,
                primaryKeyColumn: "CountryRegionID",
                allowPaging: false,
                onMultiselect: function (selectedRecords) {
                    var list = selectedRecords;
                    masterData.Countries = list;
                    initExportCountryTable(list);
                }
            });
            finder.showModal();
        });
        //end for countries

        //for EndUses
        $formEl.find("#btnEndUses").on("click", function (e) {
            e.preventDefault();
            if (masterData.EndUsesList.length == 0) {
                return toastr.error("No List Found");
            }
            var finder = new commonFinder({
                title: "Add End Uses",
                pageId: pageId,
                data: masterData.EndUsesList,
                fields: "StyleGenderName",
                headerTexts: "End Use",
                widths: "100",
                isMultiselect: true,
                primaryKeyColumn: "StyleGenderID",
                allowPaging: false,
                onMultiselect: function (selectedRecords) {
                    var list = selectedRecords;
                    masterData.EndUses = list;
                    initEndUsesTable(list);
                }
            });
            finder.showModal();
        });
        //end for EndUses

        //for FinishDyeMethods
        $formEl.find("#btnSpecialMethods").on("click", function (e) {
            e.preventDefault();
            if (masterData.FinishDyeMethodsList.length == 0) {
                return toastr.error("No List Found");
            }
            var finder = new commonFinder({
                title: "Add Finish / Dye",
                pageId: pageId,
                data: masterData.FinishDyeMethodsList,
                fields: "FinishDyeName,MethodType",
                headerTexts: "Finish Dye Name, Method Type",
                widths: "100,100",
                isMultiselect: true,
                primaryKeyColumn: "FinishDyeMethodID",
                allowPaging: false,
                onMultiselect: function (selectedRecords) {
                    var list = selectedRecords;
                    masterData.FinishDyeMethods = list;
                    initFinishDyeMethodTable(list);
                }
            });
            finder.showModal();
        });
        //end for FinishDyeMethods

        $formEl.find("#btnCareInstruction").click(function () {
            $(pageIdWithHash).find("#txtCareInstruction").val("");
            if ($.trim($(pageIdWithHash).find("#txtCareInstruction").val()) == "" && masterData.CareInstruction.length == 0) {
                $(pageIdWithHash).find("#txtCareInstruction").val(masterData.DefaultCareInstruction);
            } else {
                $(pageIdWithHash).find("#txtCareInstruction").val(masterData.CareInstruction);
            }
            $(pageIdWithHash).find("#modalCareInstruction").modal('show');
        });
        $(pageIdWithHash).find("#btnSaveCareInstruction").click(function () {
            masterData.CareInstruction = $(pageIdWithHash).find("#txtCareInstruction").val();
        });
    });
    function washTempVisibility(isDisabled) {
        if ($formEl.find("#IsProduction").is(':checked')) {
            $formEl.find(".divWashTemp").show();
            masterData.IsProduction = true;
        } else {
            masterData.IsProduction = false;
            $formEl.find(".divWashTemp").hide();
        }
        $formEl.find("#WashTemp").prop('disabled', isDisabled);
        setTestNatureDisplayType();
    }
    function testNatureHideShow() {
        if (masterData.BuyerID > 0) {
            $formEl.find(".divTestNature").show();
        } else {
            $formEl.find(".divTestNature").hide();
        }
    }
    function setTestNatureDisplayType() {
        if (masterData.IsProduction || masterData.BuyerID == 0) {
            masterData.TestNatureID = 1;
            $formEl.find("#TestNatureID").prop('disabled', true);
        } else {
            var list = $tblTestingRequirementEl.dataSource;
            if (list != null && list.length > 0) {
                masterData.TestNatureID = isRetest ? masterData.TestNatureID : list[0].TestNatureID;
                $formEl.find("#TestNatureID").prop('disabled', true);
            } else {
                if (isRetest) {
                    $formEl.find("#TestNatureID").prop('disabled', true);

                }
                else $formEl.find("#TestNatureID").prop('disabled', false);
            }
        }
        $formEl.find("#TestNatureID").select2().val(masterData.TestNatureID).trigger("change");
        testNatureHideShow();
    }
    function getTestNatureName() {
        return $formEl.find("#TestNatureID").select2('data')[0].text;
    }
    function initMasterTable() {
        if ((isApprovePage) || (isReqApproveBulkPage)) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'e-pdf e-icons' } }
            ];

        } else if ((isAcknowledgePage) || (isReqAckBulkPage)) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'e-pdf e-icons' } }
            ];
        } else {
            if (status == statusConstants.PENDING) {
                commands = [
                    { type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }
                ];
            }
            else if (status == statusConstants.ACKNOWLEDGE) {
                commands = [
                    { type: 'Retest', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } },
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'e-pdf e-icons' } }
                ];
            }
            else {
                commands = [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'e-pdf e-icons' } }
                ];
            }
        }
        var columns = [
            {
                headerText: 'Actions', commands: commands, width: ch_setActionCommandCellWidth(commands), textAlign: 'Center'
            },
            {
                field: 'LabTestStatus', headerText: 'Lab Test Status', width: 100, visible: status != statusConstants.PENDING
            },
            {
                field: 'ReqNo', headerText: 'Req. No', visible: status != statusConstants.PENDING, width: 100
            },
            {
                field: 'ReqDate', headerText: 'Req. Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING, width: 100
            },
            {
                field: 'DBatchDate', headerText: 'D.Batch Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100
            },
            //{
            //    field: 'DBatchID', headerText: 'DBatchID', width: 100
            //},
            {
                field: 'BuyerID', headerText: 'BuyerID', width: 100, visible: false
            },
            {
                field: 'TestNatureName', headerText: 'Test Nature', width: 100, visible: status != statusConstants.PENDING
            },
            {
                field: 'Buyer', headerText: 'Buyer', width: 80
            },
            {
                field: 'BuyerTeam', headerText: 'Buyer Team', width: 100
            },
            {
                field: 'SubGroupID', headerText: 'SubGroupID', width: 100, visible: false
            },
            {
                field: 'SubGroupName', headerText: 'Sub Group', width: 100
            },
            {
                field: 'ConceptNo', headerText: 'Concept No', width: 100
            },
            {
                field: 'ColorName', headerText: 'Color Name', width: 100
            },
            {
                field: 'FabricQty', headerText: 'Fabric Qty', width: 100
            },
            {
                field: 'SubClassName', headerText: 'Machine Type', width: 100
            },
            {
                field: 'TechnicalName', headerText: 'Technical Name', width: 120
            },
            {
                field: 'Gsm', headerText: 'Gsm', width: 80
            },
            {
                field: 'Composition', headerText: 'Composition', width: 100
            },
            {
                field: 'Width', headerText: 'Width(CM)', textAlign: 'Center', width: 100
            },
            {
                field: 'Length', headerText: 'Length(CM)', textAlign: 'Center', width: 100
            },
            {
                field: 'FUPartName', headerText: 'End Use', width: 100
            },
            {
                field: "IsRetest",
                headerText: "Retest ?",
                cellStyle: function () { return { classes: 'm-w-50' } },
                checkbox: true,
                showSelectTitle: true,
                checkboxEnabled: false,
                displayAsCheckBox: true,
                width: 80,
                visible: status == statusConstants.PENDING ? false : true &&
                    status == statusConstants.ACKNOWLEDGE ? false : true
            }
        ];

        if (pageName == "LabTestRequisition_Bulk" || pageName == "LabTestRequisitionApprove_Bulk" || pageName == "LabTestRequisitionAcknowledge_Bulk") {
            isBDS = 2;
        } else {
            isBDS = 0;
        }

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/lab-test-requisition/list?isBDS=${isBDS}&status=${status}`,
            autofitColumns: false,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        $formEl.find("#btnSave,#btnApprove,#btnAcknowledge").fadeOut();
        $formEl.find("#RetestNo").fadeOut();
        $formEl.find("#IsRetest").prop("disabled", false);
        $formEl.find("#IsRetest").prop("checked", false);
        if (status == statusConstants.PENDING) {
            getNew(args.rowData.DBatchID, args.rowData.ConceptID, args.rowData.SubGroupID, args.rowData.BuyerID);
            $formEl.find("#btnSave").fadeIn();
            $formEl.find("#btnApprove,#btnAcknowledge,#btnUnAcknowledge,#btnRevise,#UnAckReasonAreaId").fadeOut();
        }
        else {
            if (args.commandColumn.type == "ViewReport") {
                //window.open(`/reports/InlinePdfView?ReportId=1252&ReqID=${args.rowData.LTReqMasterID}`, '_blank');
                if (args.rowData.LabTestStatus == "Production") {
                    window.open(`/reports/InlinePdfView?ReportName=LabTestTestingRequirementFormProduction.rdl&ReqID=${args.rowData.LTReqMasterID}&BuyerId=${args.rowData.BuyerID}`, '_blank');
                }
                else {
                    if (args.rowData.TestNatureName == 'China') {
                        window.open(`/reports/InlinePdfView?ReportName=LabTestTestingRequirementFormChina.rdl&ReqID=${args.rowData.LTReqMasterID}&BuyerId=${args.rowData.BuyerID}`, '_blank');
                    } else {
                        window.open(`/reports/InlinePdfView?ReportName=LabTestTestingRequirementForm.rdl&ReqID=${args.rowData.LTReqMasterID}&BuyerId=${args.rowData.BuyerID}`, '_blank');
                    }
                }
            }

            if ((isApprovePage) || (isReqApproveBulkPage)) {
                $toolbarEl.find("#btnAddBuyer").fadeOut();
                if (status == statusConstants.COMPLETED) {
                    $formEl.find("#btnApprove").fadeIn();
                    $formEl.find("#btnAcknowledge,#btnSave,#btnUnAcknowledge,#btnRevise").fadeOut();
                } else {
                    $formEl.find("#btnSave,#btnApprove,#btnAcknowledge,#btnUnAcknowledge,#btnRevise,#UnAckReasonAreaId").fadeOut();
                }
            }
            else if ((isAcknowledgePage) || (isReqAckBulkPage)) {
                $toolbarEl.find("#btnAddBuyer").fadeOut();
                if (status == statusConstants.APPROVED) {
                    $formEl.find("#btnAcknowledge").fadeIn();
                    $formEl.find("#btnApprove,#btnSave,#btnRevise,#UnAckReasonAreaId").fadeOut();
                } else if (status == statusConstants.UN_ACKNOWLEDGE) {
                    $formEl.find("#UnAckReasonAreaId").fadeIn();
                    $formEl.find("#btnRevise").fadeOut();
                }
                else {
                    $formEl.find("#btnSave,#btnApprove,#btnAcknowledge,#btnRevise,#UnAckReasonAreaId").fadeOut();
                }
            }
            else {
                $toolbarEl.find("#btnAddBuyer").fadeIn();

                if (args.commandColumn.type == 'Retest') {
                    if (status == statusConstants.ACKNOWLEDGE) {
                        $formEl.find("#btnSave").fadeIn();
                        $formEl.find("#btnAcknowledge,#btnApprove").fadeOut();
                        $formEl.find("#RetestNo").fadeIn();
                        $formEl.find("#IsRetest").prop("disabled", true);
                        $formEl.find("#IsRetest").prop("checked", true);
                    }
                }
                else if (args.commandColumn.type == 'Edit') {
                    
                    if (status == statusConstants.ACKNOWLEDGE) {
                        $formEl.find("#btnSave").fadeOut();
                        $formEl.find("#btnAcknowledge,#btnApprove,#btnRevise,#UnAckReasonAreaId").fadeOut();
                    }
                    else if (status == statusConstants.COMPLETED) {
                        $formEl.find("#btnSave").fadeIn();
                        $formEl.find("#btnAcknowledge,#btnApprove,#UnAckReasonAreaId,#btnRevise").fadeOut();
                    }
                    else if (status == statusConstants.UN_ACKNOWLEDGE) {
                        $formEl.find("#btnRevise,#UnAckReasonAreaId").fadeIn();
                        $formEl.find("#btnAcknowledge,#btnApprove,#btnSave").fadeOut();
                    }
                    else {
                        $formEl.find("#btnRevise,#UnAckReasonAreaId").fadeOut();
                    }
                }
            }

            if (args.commandColumn.type == 'Retest') {
                getDetails(args.rowData.LTReqMasterID, true, true, args.rowData.BuyerID);
            } else if (args.commandColumn.type != "ViewReport") {
                getDetails(args.rowData.LTReqMasterID, args.rowData.IsRetest, false, args.rowData.BuyerID);
            }
        }
    }

    function initChildTable() {
        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            showFooter: true,
            detailView: true,
            columns: [
                {
                    title: 'Actions',
                    align: 'center',
                    width: 10,
                    visible: (!isApprovePage && !isAcknowledgePage &&
                        !isReqApproveBulkPage && !isReqAckBulkPage &&
                        (status == statusConstants.PENDING || status == statusConstants.COMPLETED)),
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
                            masterData.LabTestRequisitionBuyers.splice(index, 1);
                            //$tblChildEl.bootstrapTable('hideRow', { index: index });
                            $tblChildEl.bootstrapTable("load", masterData.LabTestRequisitionBuyers);
                            $tblChildEl.bootstrapTable('hideLoading');
                        }
                    }
                },
                {
                    field: "BuyerName",
                    title: "Buyer Name",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                }
            ],
            onExpandRow: function (index, row, $detail) {
                populateLabTestRequisitionBuyer(row.LTReqBuyerID, $detail);
            },
        });
    }

    function initLabelCareTable(records) {
        if ($tblLabelCare) $tblLabelCare.destroy();
        ej.base.enableRipple(true);
        $tblLabelCare = new ej.grids.Grid({
            allowPaging: false,
            allowFiltering: true,
            pageSettings: { pageCount: 5, currentPage: 1, pageSize: 10, pageSizes: true },
            editSettings: { allowEditing: true, allowDeleting: true },
            dataSource: records,
            commandClick: chandleCommandsCareLabel,
            actionBegin: function (args) {
                if (args.requestType === "save") {
                    var findIndex = masterData.CareLabels.findIndex(x => x.LTReqCareLabelID == args.data.LTReqCareLabelID);
                    if (findIndex > -1) {
                        masterData.CareLabels[findIndex].SeqNo = args.data.SeqNo;
                        args.rowData = args.data;
                        $tblLabelCare.updateRow(findIndex, args.rowData);
                    }
                }
            },
            columns: [
                {
                    headerText: 'Action', textAlign: 'Center', visible: (status != statusConstants.APPROVED && status != statusConstants.ACKNOWLEDGE) || isRetest, width: 120, commands: [
                        { type: 'remove', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }
                    ]
                },
                { field: 'LTReqCareLabelID', allowEditing: false, visible: false },
                { field: 'CareType', allowEditing: false, headerText: 'Care Type' },
                { field: 'CareName', allowEditing: false, headerText: 'Care Name' },
                { field: 'ImagePath', allowEditing: false, headerText: 'Icon', valueAccessor: ej2GridImageFormatter },
                { field: 'GroupCode', allowEditing: false, headerText: 'Code' },
                { field: 'SeqNo', headerText: 'SeqNo' }
            ]
        });
        $tblLabelCare.appendTo(tblLabelCareId);
    }

    function initExportCountryTable(records) {
        if ($tblExportCountries) $tblExportCountries.destroy();
        ej.base.enableRipple(true);
        $tblExportCountries = new ej.grids.Grid({
            allowPaging: false,
            allowFiltering: true,
            pageSettings: { pageCount: 5, currentPage: 1, pageSize: 10, pageSizes: true },
            editSettings: { allowEditing: true, allowDeleting: true },
            dataSource: records,
            commandClick: handleCommandExportCountries,
            actionBegin: function (args) {
                if (args.requestType === "save") {

                }
            },
            columns: [
                {
                    headerText: 'Action', textAlign: 'Center', width: 40, visible: (status != statusConstants.APPROVED && status != statusConstants.ACKNOWLEDGE) || isRetest, commands: [
                        { type: 'remove', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }
                    ]
                },
                { field: 'LTRECountryID', width: 80, allowEditing: false, visible: false },
                { field: 'RegionName', width: 80, allowEditing: false, headerText: 'Region' }
            ]
        });
        $tblExportCountries.appendTo(tblExportCountriesId);
    }
    function handleCommandExportCountries(args) {
        if (args.commandColumn.type == "remove") {
            showBootboxConfirm("Delete Record.", "Are you sure want to delete this?", function (yes) {
                if (yes) {
                    masterData.Countries = masterData.Countries.filter(x => x.CountryRegionID != args.rowData.CountryRegionID);
                    initExportCountryTable(masterData.Countries);
                }
            });
        }
    }

    function initEndUsesTable(records) {
        if ($tblEndUses) $tblEndUses.destroy();
        ej.base.enableRipple(true);
        $tblEndUses = new ej.grids.Grid({
            allowPaging: false,
            allowFiltering: true,
            pageSettings: { pageCount: 5, currentPage: 1, pageSize: 10, pageSizes: true },
            editSettings: { allowEditing: true, allowDeleting: true },
            dataSource: records,
            commandClick: handleCommandEndUses,
            actionBegin: function (args) {
                if (args.requestType === "save") {

                }
            },
            columns: [
                {
                    headerText: 'Action', textAlign: 'Center', width: 40, visible: (status != statusConstants.APPROVED && status != statusConstants.ACKNOWLEDGE) || isRetest, commands: [
                        { type: 'remove', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }
                    ]
                },
                { field: 'LTREndUseID', width: 80, allowEditing: false, visible: false },
                { field: 'StyleGenderName', width: 80, allowEditing: false, headerText: 'End Use' }
            ]
        });
        $tblEndUses.appendTo(tblEndUsesId);
    }
    function handleCommandEndUses(args) {
        if (args.commandColumn.type == "remove") {
            showBootboxConfirm("Delete Record.", "Are you sure want to delete this?", function (yes) {
                if (yes) {
                    masterData.EndUses = masterData.EndUses.filter(x => x.StyleGenderID != args.rowData.StyleGenderID);
                    initEndUsesTable(masterData.EndUses);
                }
            });
        }
    }

    function initFinishDyeMethodTable(records) {
        if ($tblSpecialMethods) $tblSpecialMethods.destroy();
        ej.base.enableRipple(true);
        $tblSpecialMethods = new ej.grids.Grid({
            allowPaging: false,
            allowFiltering: true,
            pageSettings: { pageCount: 5, currentPage: 1, pageSize: 10, pageSizes: true },
            editSettings: { allowEditing: true, allowDeleting: true },
            dataSource: records,
            commandClick: handleCommandFinishDyeMethods,
            actionBegin: function (args) {
                if (args.requestType === "save") {

                }
            },
            columns: [
                {
                    headerText: 'Action', textAlign: 'Center', width: 30, visible: (status != statusConstants.APPROVED && status != statusConstants.ACKNOWLEDGE) || isRetest, commands: [
                        { type: 'remove', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }
                    ]
                },
                { field: 'LTRFinishDyeMethodID', width: 80, allowEditing: false, visible: false },
                { field: 'FinishDyeName', width: 80, allowEditing: false, headerText: 'Finish / Dye' },
                { field: 'MethodType', width: 80, allowEditing: false, headerText: 'Method Type' }
            ]
        });
        $tblSpecialMethods.appendTo(tblSpecialMethodsId);
    }
    function handleCommandFinishDyeMethods(args) {
        if (args.commandColumn.type == "remove") {
            showBootboxConfirm("Delete Record.", "Are you sure want to delete this?", function (yes) {
                if (yes) {
                    masterData.FinishDyeMethods = masterData.FinishDyeMethods.filter(x => x.FinishDyeMethodID != args.rowData.FinishDyeMethodID);
                    initFinishDyeMethodTable(masterData.FinishDyeMethods);
                }
            });
        }
    }

    function initTestingRequirementTable(records) {
        if ($tblTestingRequirementEl) $tblTestingRequirementEl.destroy();
        ej.base.enableRipple(true);
        $tblTestingRequirementEl = new ej.grids.Grid({
            allowPaging: true,
            allowFiltering: true,
            pageSettings: { pageCount: 5, currentPage: 1, pageSize: 10, pageSizes: true },
            editSettings: { allowDeleting: true },
            dataSource: records,
            commandClick: chandleCommands,
            columns: [
                {
                    headerText: 'Action', textAlign: 'Center', visible: (status != statusConstants.APPROVED && status != statusConstants.ACKNOWLEDGE) || isRetest, width: 120, commands: [
                        { type: 'View Parameter', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                        { type: 'remove', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }
                    ]
                },
                {
                    headerText: 'Action', textAlign: 'Center', visible: (status == statusConstants.APPROVED || status == statusConstants.ACKNOWLEDGE) && !isRetest, width: 120, commands: [
                        { type: 'View Parameter', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }
                    ]
                },
                { field: 'TestName', headerText: 'Testing Requirement' },
                { field: 'TestMethod', headerText: 'Test Method' },
                { field: 'Requirement', headerText: 'Requirement' },
                { field: 'TestNatureName', headerText: 'Test Nature', visible: masterData.BuyerID > 0 },
            ]
        });
        $tblTestingRequirementEl.appendTo(tblTestingRequirementId);
    }
    function chandleCommandsCareLabel(args) {
        if (args.commandColumn.type == "remove") {
            showBootboxConfirm("Delete Record.", "Are you sure want to delete this?", function (yes) {
                if (yes) {
                    var i = 0;
                    while (i < $tblLabelCare.dataSource.length) {
                        if ($tblLabelCare.dataSource[i].LCareLableID === args.rowData.LCareLableID) {
                            $tblLabelCare.dataSource.splice(i, 1);
                        } else {
                            ++i;
                        }
                    }
                    initLabelCareTable($tblLabelCare.dataSource);
                }
            });
        }
    }

    function getRelatedItem(bpId) {
        var dimensionalStabilityWashObj = {
            LineDry: 650,
            TumbleDry: 652,
            FlatDry: 653
        };

        var spiralityObj = {
            LineDry: 651,
            TumbleDry: 654,
            FlatDry: 655
        };

        if (bpId == dimensionalStabilityWashObj.LineDry) return spiralityObj.LineDry;
        else if (bpId == dimensionalStabilityWashObj.TumbleDry) return spiralityObj.TumbleDry;
        else if (bpId == dimensionalStabilityWashObj.FlatDry) return spiralityObj.FlatDry;
        else if (bpId == spiralityObj.LineDry) return dimensionalStabilityWashObj.LineDry;
        else if (bpId == spiralityObj.TumbleDry) return dimensionalStabilityWashObj.TumbleDry;
        else if (bpId == spiralityObj.FlatDry) return dimensionalStabilityWashObj.FlatDry;
        return 0;
    }

    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }

    function chandleCommands(args) {
        if (args.commandColumn.type == "remove") {
            showBootboxConfirm("Delete Record.", "Are you sure want to delete this?", function (yes) {
                if (yes) {

                    var removeBPIDs = [];
                    var list = DeepClone($tblTestingRequirementEl.dataSource);
                    var indexF = list.findIndex(x => x.BPID === args.rowData.BPID);
                    if (indexF > -1) {
                        removeBPIDs.push(args.rowData.BPID);
                    }
                    if (masterData.IsProduction || $formEl.find('#IsProduction').is(':checked')) {
                        var bpID = getRelatedItem(args.rowData.BPID);
                        indexF = list.findIndex(x => x.BPID === bpID);
                        if (indexF > -1) {
                            removeBPIDs.push(bpID);
                            //list.splice(indexF, 1);
                        }
                    }
                    for (var i = removeBPIDs.length; i--;) {
                        var findIndex = list.findIndex(x => x.BPID == removeBPIDs[i]);
                        list.splice(findIndex, 1);
                    }

                    $tblTestingRequirementEl.dataSource = list;
                    $tblTestingRequirementEl.refresh();

                    if (isRetest) {
                        masterData.TestNatureID = testNatureObjForRetest.TestNatureID;
                        masterData.TestNatureName = testNatureObjForRetest.TestNatureName;
                    }

                    setTestNatureDisplayType();
                }
            });
        }

        if (args.commandColumn.type == "View Parameter") {

            var filterBuyerParametres = masterData.BuyerParameters.filter(e => e.BPID == args.rowData.BPID);

            var finder = new commonFinder({
                title: "Parameter :: " + args.rowData.TestName,
                pageId: pageId,
                data: filterBuyerParametres,
                fields: "SubTestName,SubSubTestName,RefValueFrom,TestValue",
                headerTexts: "Sub Test Name,Sub Sub Test Name,Range Value,Test Value",
                isMultiselect: false,
                primaryKeyColumn: "LTReqBPID",
                allowPaging: false,
            });
            finder.showModal();
        }
    }

    function getBuyer() {
        var url = "/api/selectoption/buyername";
        axios.get(url)
            .then(function (response) {
                showBootboxSelect2MultipleDialog("Select Buyer", "Ids", "Select Buyer", response.data, function (result) {
                    if (result) {
                        var ids = "";
                        for (var i = 0; i < result.length; i++) ids += result[i].id + ",";
                        getBuyerParameter(ids.substring(0, ids.length - 1));
                    }
                });
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getBuyerParameter(ids) {
        var url = "/api/lab-test-requisition/buyer-parameter/" + ids;
        axios.get(url)
            .then(function (response) {
                for (var i = 0; i < response.data.length; i++) {
                    response.data[i].LTReqBuyerID = (masterData.LabTestRequisitionBuyers.length == 0) ? 1 : masterData.LabTestRequisitionBuyers[masterData.LabTestRequisitionBuyers.length - 1].LTReqBuyerID + 1;
                    masterData.LabTestRequisitionBuyers.push(response.data[i]);
                }

                $tblChildEl.bootstrapTable("load", masterData.LabTestRequisitionBuyers);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function populateLabTestRequisitionBuyer(childId, $detail) {
        $el = $detail.html('<table id="TblYarnReceiveChildRackBin-' + childId + '"></table>').find('table');
        initLabTestRequisitionBuyer($el, childId);
        var ind = getIndexFromArray(masterData.LabTestRequisitionBuyers, "LTReqBuyerID", childId)
        var cnt = masterData.LabTestRequisitionBuyers[ind].LabTestRequisitionBuyerParameters.length;
        if (cnt > 0)
            $el.bootstrapTable('load', masterData.LabTestRequisitionBuyers[ind].LabTestRequisitionBuyerParameters);
    }

    function initLabTestRequisitionBuyer($el, childId) {
        $el.bootstrapTable({
            showFooter: true,
            uniqueId: 'LTReqBuyerID',
            columns: [
                {
                    field: "LTReqBuyerID",
                    title: "LTReqBuyerID",
                    align: "left",
                    width: 100,
                    visible: false
                },
                {
                    field: "ParameterName",
                    title: "Parameter Name",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "RefValueFrom",
                    title: "Ref Value From",
                    cellStyle: function () { return { classes: 'm-w-80' } }
                },
                {
                    field: "RefValueTo",
                    title: "Ref Value To",
                    cellStyle: function () { return { classes: 'm-w-80' } }
                }
            ]
        });
    }

    function backToListWithoutFilter() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        testingRequirementList = [];
    }
    function backToList() {
        backToListWithoutFilter();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#LTReqMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(newId, conceptId, subGroupId, buyerId) {
        axios.get(`/api/lab-test-requisition/new/${newId}/${conceptId}/${subGroupId}/${buyerId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ReqDate = formatDateToDefault(masterData.ReqDate);
                masterData.DBatchDate = formatDateToDefault(masterData.DBatchDate);
                masterData.LabTestServiceTypeID = 1;
                masterData.TestNatureID = 1;
                setFormData($formEl, masterData);
                $tblChildEl.bootstrapTable("load", masterData.LabTestRequisitionBuyers);
                $tblChildEl.bootstrapTable('hideLoading');

                initLabelCareTable(masterData.CareLabels);
                initExportCountryTable([]);
                initEndUsesTable([]);
                initFinishDyeMethodTable([]);

                initTestingRequirementTable([]);

                if (masterData.ConceptNo != '') {
                    $divItemInfo.fadeOut();
                    $divTestingRequirement.fadeIn();
                }
                else {
                    $divItemInfo.fadeIn();
                    $divTestingRequirement.fadeOut();
                }
                $formEl.find("#IsProduction").prop("disabled", false);
                washTempVisibility(false);
                setTestNatureDisplayType();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id, isretest, isretestflag, buyerId) {
        isRetest = isretest;
        axios.get(`/api/lab-test-requisition/${id}/${isretestflag}/${buyerId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ReqDate = formatDateToDefault(masterData.ReqDate);
                masterData.DBatchDate = formatDateToDefault(masterData.DBatchDate);
                setFormData($formEl, masterData);

                if (isRetest) {
                    testNatureObjForRetest = {
                        TestNatureID : masterData.TestNatureID,
                        TestNatureName: masterData.TestNatureName
                    };
                }

                $tblChildEl.bootstrapTable("load", masterData.LabTestRequisitionBuyers);
                $tblChildEl.bootstrapTable('hideLoading');
                testingRequirementList = masterData.LabTestRequisitionBuyers[0].LabTestRequisitionBuyerParameters;
                initTestingRequirementTable(testingRequirementList);
                initLabelCareTable(masterData.CareLabels);

                initExportCountryTable(masterData.Countries);
                initEndUsesTable(masterData.EndUses);
                initFinishDyeMethodTable(masterData.FinishDyeMethods);

                var isDisabled = status == statusConstants.COMPLETED ? false : true;
                washTempVisibility(isDisabled);

                if (masterData.ConceptNo != '') {
                    $divItemInfo.fadeOut();
                    $divTestingRequirement.fadeIn();
                }
                else {
                    $divItemInfo.fadeIn();
                    $divTestingRequirement.fadeOut();
                }
                $formEl.find("#IsProduction").prop("disabled", true);
                setTestNatureDisplayType();

                
                if (isAcknowledgePage != true && isApprovePage != true && isReqAckBulkPage != true && isReqApproveBulkPage != true) {
                    if (isretest == true && status == statusConstants.ACKNOWLEDGE) {
                        $formEl.find("#RetestNo").fadeIn();
                        $formEl.find("#IsRetest").prop("disabled", true);
                        $formEl.find("#IsRetest").prop("checked", true);
                    } else {
                        $formEl.find("#RetestNo").fadeOut();
                        $formEl.find("#IsRetest").prop("disabled", false);
                        $formEl.find("#btnUnAcknowledge").fadeOut();
                    }
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    var validationConstraints = {
        UnitID: {
            presence: true
        },
        FabricQty: {
            presence: true
        },
        ContactPersonID: {
            presence: true
        }
    }

    function save() {
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);
        masterData.LabTestRequisitionBuyers[0].LabTestRequisitionBuyerParameters = $tblTestingRequirementEl.dataSource;
        var data = formDataToJson($formEl.serializeArray());
        data["LabTestRequisitionBuyers"] = masterData.LabTestRequisitionBuyers;

        data.CareLabels = masterData.CareLabels;

        data.Countries = masterData.Countries;
        data.EndUses = masterData.EndUses;
        data.FinishDyeMethods = masterData.FinishDyeMethods;

        data.SubGroupID = masterData.SubGroupID;
        data.GroupConceptNo = masterData.ConceptNo;
        data.CareInstruction = masterData.CareInstruction;

        data.TestNatureID = masterData.TestNatureID;

        masterData.LabTestRequisitionBuyers[0].LabTestRequisitionBuyerParameters.map(x => {
            x.TestNatureID = data.TestNatureID;
        });

        var ChekBox;
        if ($formEl.find("#IsRetest").is(':checked'))
            ChekBox = 1;
        else
            ChekBox = 0;
        data["IsRetest"] = ChekBox;

        var ChekBoxProd;
        if ($formEl.find("#IsProduction").is(':checked'))
            ChekBoxProd = 1;
        else
            ChekBoxProd = 0;
        data["IsProduction"] = ChekBoxProd;

        if (data.LabTestServiceTypeID == '0' || typeof data.LabTestServiceTypeID == "undefined") {
            toastr.error("Service Type can't be blank");
            return;
        }
        

        if (data.TestNatureID == null) {
            data.TestNatureID = $formEl.find("#TestNatureID").val();
            data.TestNatureName = getTestNatureName();
        }

        if (data.TestNatureID == '0' || typeof data.TestNatureID == "undefined") {
            toastr.error("Test Nature can't be blank");
            return;
        }
        if (data.LabTestRequisitionBuyers.length == 0) {
            toastr.error("At least 1 Testing Requirement Information is required.");
            return;
        }
        var hasError = false;
        for (var i = 0; i < data.LabTestRequisitionBuyers.length; i++) {
            if (data.LabTestRequisitionBuyers[i].LabTestRequisitionBuyerParameters.length == 0) {
                hasError = true;
                break;
            }
        }
        if (hasError) {
            toastr.error("At least 1 Testing Requirement Information is required.");
            return;
        }

        axios.post("/api/lab-test-requisition/save", data)
            .then(function (response) {
                toastr.success("Saved successfully.");
                var obj = response.data;
                //displayReport(obj.LabTestStatus, obj.LTReqMasterID, obj.BuyerID);
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function revise() {
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);
        masterData.LabTestRequisitionBuyers[0].LabTestRequisitionBuyerParameters = $tblTestingRequirementEl.dataSource;
        var data = formDataToJson($formEl.serializeArray());
        data["LabTestRequisitionBuyers"] = masterData.LabTestRequisitionBuyers;

        data.CareLabels = masterData.CareLabels;

        data.Countries = masterData.Countries;
        data.EndUses = masterData.EndUses;
        data.FinishDyeMethods = masterData.FinishDyeMethods;

        data.SubGroupID = masterData.SubGroupID;
        data.GroupConceptNo = masterData.ConceptNo;
        data.CareInstruction = masterData.CareInstruction;

        data.TestNatureID = masterData.TestNatureID;

        masterData.LabTestRequisitionBuyers[0].LabTestRequisitionBuyerParameters.map(x => {
            x.TestNatureID = data.TestNatureID;
        });

        var ChekBox;
        if ($formEl.find("#IsRetest").is(':checked'))
            ChekBox = 1;
        else
            ChekBox = 0;
        data["IsRetest"] = ChekBox;

        var ChekBoxProd;
        if ($formEl.find("#IsProduction").is(':checked'))
            ChekBoxProd = 1;
        else
            ChekBoxProd = 0;
        data["IsProduction"] = ChekBoxProd;

        if (data.LabTestServiceTypeID == '0' || typeof data.LabTestServiceTypeID == "undefined") {
            toastr.error("Service Type can't be blank");
            return;
        }


        if (data.TestNatureID == null) {
            data.TestNatureID = $formEl.find("#TestNatureID").val();
            data.TestNatureName = getTestNatureName();
        }

        if (data.TestNatureID == '0' || typeof data.TestNatureID == "undefined") {
            toastr.error("Test Nature can't be blank");
            return;
        }
        if (data.LabTestRequisitionBuyers.length == 0) {
            toastr.error("At least 1 Testing Requirement Information is required.");
            return;
        }
        var hasError = false;
        for (var i = 0; i < data.LabTestRequisitionBuyers.length; i++) {
            if (data.LabTestRequisitionBuyers[i].LabTestRequisitionBuyerParameters.length == 0) {
                hasError = true;
                break;
            }
        }
        if (hasError) {
            toastr.error("At least 1 Testing Requirement Information is required.");
            return;
        }

        axios.post("/api/lab-test-requisition/revise", data)
            .then(function (response) {
                toastr.success("Saved successfully.");
                var obj = response.data;
                displayReport(obj.LabTestStatus, obj.LTReqMasterID, obj.BuyerID);
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function displayReport(LabTestStatus, LTReqMasterID, BuyerID) {
        if (LabTestStatus == "Production") {
            window.open(`/reports/InlinePdfView?ReportName=LabTestTestingRequirementFormProduction.rdl&ReqID=${LTReqMasterID}&BuyerId=${BuyerID}`, '_blank');
        }
        else {
            window.open(`/reports/InlinePdfView?ReportName=LabTestTestingRequirementForm.rdl&ReqID=${LTReqMasterID}&BuyerId=${BuyerID}`, '_blank');
        }
    }

    function approve(isApprove = false, isAcknowledge = false,unAcknowledge, unAckReason) {
        var data = formDataToJson($formEl.serializeArray());
        data.IsApproved = isApprove;
        data.IsAcknowledge = isAcknowledge;
        data.UnAcknowledge = unAcknowledge;
        data.UnAcknowledgeReason = unAckReason;
        axios.post("/api/lab-test-requisition/approve", data)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function getCarelabels() {
        var url = "/api/lab-test-requisition/carelables/";
        axios.get(url)
            .then(function (response) {
                for (var i = 0; i < response.data.length; i++) {
                    masterData.CareLabels.push(response.data[i]);
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    async function getSetList(buyerID, testNatureID, isProduction) {

        await axios.get(`/api/lab-test-requisition/get-LabTestRequisition-BuyerParametersSet/${buyerID}/${testNatureID}/${isProduction}`)
            .then(function (response) {
                //debugger;
                if (testNamelist != null) {
                    setTestNameList = response.data.filter(item => testNamelist.includes(item.TestName));
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);

            });
    }

    function findMatchedTestReqName(buyerName,value) {
        if (buyerName == "Ralph Lauren") {
            if (value.TestName == "DIMENSIONAL STABILITY") {
                testNamelist = "SKEWING";
            }
            else if (value.TestName == "SKEWING") {
                testNamelist = "DIMENSIONAL STABILITY";
            }
        }
        else if (buyerName == "M&S") {
            if (value.TestName == "Wascator Stability Wash") {
                testNamelist = "Measurement Of Spirality";
            }
            else if (value.TestName == "Measurement Of Spirality") {
                testNamelist = "Wascator Stability Wash";
            }
        }
        else if (buyerName == "Target Australia") {
            if (value.TestName == "Dimensional stability to washing (Fabric Portion)") {
                testNamelist = "Spirality";
            }
            else if (value.TestName == "Spirality") {
                testNamelist = "Dimensional stability to washing (Fabric Portion)";
            }
        }
        else if (buyerName == "Calvin Klein Jeans" || buyerName == "Tommy Hilfiger" || buyerName == "Tommy Jeans") {
            if (value.TestName == "Dimensional Change To Washing (Fabric)") {
                testNamelist = "Skewness After Washing(Fabric)";
            }
            else if (value.TestName == "Skewness After Washing(Fabric)") {
                testNamelist = "Dimensional Change To Washing (Fabric)";
            }
        }
        else if (buyerName == "CHAMPION EUROPE" || buyerName == "CHAMPION USA" || buyerName == "CHAMPION AUSTRALIA") {
            if (value.TestName == "Dimensional stability to washing") {
                testNamelist = "Spirality after wash";
            }
            else if (value.TestName == "Spirality after wash") {
                testNamelist = "Dimensional stability to washing";
            }
        }
        else if (buyerName == "S.Oliver") {
            if (value.TestName == "DIMENSIONAL STABILITY TO WASHING") {
                testNamelist = "TWISTING";
            }
            else if (value.TestName == "TWISTING") {
                testNamelist = "DIMENSIONAL STABILITY TO WASHING";
            }
        }
        else if (buyerName == "Varner") {
            if (value.TestName == "DIMENSIONAL STABILITY TO WASHING") {
                testNamelist = "TWISTING";
            }
            else if (value.TestName == "TWISTING") {
                testNamelist = "DIMENSIONAL STABILITY TO WASHING";
            }
        }
        else if (buyerName == "Puma") {
            if (value.TestName == "Dimentional Stability to Washing (Fabric)") {
                testNamelist = "Spirality After Washing (Fabric)";
            }
            else if (value.TestName == "Spirality After Washing (Fabric)") {
                testNamelist = "Dimentional Stability to Washing (Fabric)";
            }
        }
        else if (buyerName == "Country Road") {
            if (value.TestName == "Dimensional Change after Washing") {
                testNamelist = "Spirality after Laundering";
            }
            else if (value.TestName == "Spirality after Laundering") {
                testNamelist = "Dimensional Change after Washing";
            }
        }
        else if (buyerName == "Stanley Stella") {
            if (value.TestName == "Dimensional stability to washing: (Fabric Portion)") {
                testNamelist = "Spirality after wash";
            }
            else if (value.TestName == "Spirality after wash") {
                testNamelist = "Dimensional stability to washing: (Fabric Portion)";
            }
        }
        else if (buyerName == "Carhartt") {
            if (value.TestName == "DIMENSIONAL CHANGE AFTER HOME LAUNDERING(3RD WASH): (Fabric Portion)") {
                testNamelist = "SPIRALITY AFTER WASH (3RD WASH)";
            }
            else if (value.TestName == "SPIRALITY AFTER WASH (3RD WASH)") {
                testNamelist = "DIMENSIONAL CHANGE AFTER HOME LAUNDERING(3RD WASH): (Fabric Portion)";
            }
        }
        else if (buyerName == "Original Marines") {
            if (value.TestName == "Dimensional stability to washing") {
                testNamelist = "Spirality after Laundering";
            }
            else if (value.TestName == "Spirality after Laundering") {
                testNamelist = "Dimensional stability to washing";
            }
        }
    }
})();
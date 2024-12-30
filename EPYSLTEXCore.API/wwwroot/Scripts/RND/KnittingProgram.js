(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, $tblYarnEl, tblMasterId;
    var status;
    var masterData;
    var isConceptPage = false, isBDSPage = false, isBulkPage = false;
    var isConcept = false, isBDS = false;
    var _yarnPrevInfoList = [];
    var pageId = "";

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $tblYarnEl = $("#tblKnittingPlanYarn" + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        var menuParam = $("#" + pageId).find("#txtMenuParam").val();
        if (menuParam == "A") isConcept = true;
        else if (menuParam == "BDS") isBDS = true;

        isConceptPage = convertToBoolean($(`#${pageId}`).find("#ConceptPage").val());
        isBDSPage = convertToBoolean($(`#${pageId}`).find("#BDSPage").val());
        isBulkPage = convertToBoolean($(`#${pageId}`).find("#BulkPage").val());

        status = statusConstants.PENDING;
        initMasterTable();

        $formEl.find(".clsKPBulk").hide();
        if (isBulkPage) {
            $formEl.find(".clsKPBulk").show();
            $formEl.find(".lblConceptQty").text("Knitting Prod Qty");
            $formEl.find(".lblConceptDate").text("Booking Date");
            $formEl.find(".lblConceptNo").text("Booking No");
            $formEl.find(".lblPlanQty").text("Total Planned Qty");


            //$toolbarEl.find("#btnInActiveList,#btnAllList").hide();
            $toolbarEl.find("#btnAllList").hide();
        }

        //$("#UploadFile").on('change', function (e) {

        //    $(".fileinput-remove-button").on("click", function (event) {
        //        var confirmed = confirm('Are you sure you want to remove the selected file(s)?');
        //        if (!confirmed) {
        //            event.preventDefault(); // Prevent the default behavior of the remove button
        //        }
        //    });

        //});

        //$divTblEl.find(".select2-search__field").on('click', function (e) {
        $(document).on('click', '.select2-search__field', function (e) {
            return false;
            //e.preventDefault();
        });

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            $formEl.find("#btnSelectMachine").html('Select Machine');
            initMasterTable();
        });

        $toolbarEl.find("#btnActiveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACTIVE;
            $formEl.find("#btnSelectMachine").html('Select Machine');
            initMasterTable();
        });

        $toolbarEl.find("#btnInActiveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.IN_ACTIVE;
            $formEl.find("#btnSelectMachine").html('Select Machine');
            initMasterTable();
            $formEl.find("#btnSave").fadeOut();
        });

        $toolbarEl.find("#btnAllList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ALL;
            $formEl.find("#btnSelectMachine").html('Select Machine');
            initMasterTable();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this, false);
        });

        $formEl.find("#btnRevise").click(function (e) {
            e.preventDefault();
            revise(this);
        });

        $formEl.find("#btnReviseAndComplete").click(function (e) {
            e.preventDefault();
            save(this, false);
        });

        $formEl.find("#btnSaveAndAapprove").click(function (e) {
            e.preventDefault();
            save(this, true);
        });

        $formEl.find("#IsSubContact").click(function (e) {
            if ($formEl.find("#IsSubContact").is(':checked')) {
                $formEl.find("#IsSubContact").prop("checked", true);
                $formEl.find("#btnSelectMachine").html('Select SubContact');

                masterData.Childs[0].IsSubContact = true;
                masterData.Childs[0].ContactID = 0;
                masterData.Childs[0].Contact = "Empty";
                masterData.Childs[0].BrandID = 0;
                masterData.Childs[0].Brand = "Empty";
                masterData.Childs[0].KnittingMachineID = 0;
                masterData.Childs[0].KnittingMachineNo = "Empty";

                $formEl.find("#tblProgramInformation").bootstrapTable('load', masterData.Childs);
            } else {
                $formEl.find("#IsSubContact").prop("checked", false);
                $formEl.find("#btnSelectMachine").html('Select Machine');

                masterData.Childs[0].IsSubContact = false;
                masterData.Childs[0].ContactID = 0;
                masterData.Childs[0].Contact = "Empty";
                masterData.Childs[0].BrandID = 0;
                masterData.Childs[0].Brand = "Empty";
                masterData.Childs[0].KnittingMachineID = 0;
                masterData.Childs[0].KnittingMachineNo = "Empty";

                $formEl.find("#tblProgramInformation").bootstrapTable('load', masterData.Childs);
            }
        });

        $formEl.find("#btnMachineType").on("click", function (e) {
            e.preventDefault();
            //var MachineTypeList = masterData.MachineTypeList;
            var MachineTypeList;
            if (status === statusConstants.PENDING) {
                MachineTypeList = masterData.MachineTypeList;
            }
            else {
                if (isConcept) {
                    MachineTypeList = masterData.MachineTypeList;
                }
                else {
                    MachineTypeList = masterData.MCSubClassList;
                }
            }

            var finder = new commonFinder({
                title: "Machine Type",
                pageId: pageId,
                data: MachineTypeList,
                isMultiselect: false,
                modalSize: "modal-md",
                top: "2px",
                primaryKeyColumn: "id",
                fields: "text",
                headerTexts: "Machine Type",
                widths: "30",
                onSelect: function (res) {
                    finder.hideModal();

                    masterData.MCSubClassID = res.rowData.id;
                    $formEl.find("#MCSubClassID").val(masterData.MCSubClassID);
                    $formEl.find("#MCSubClass").val(res.rowData.text);
                    masterData.Childs.map(c => {
                        c.MCSubClassID = masterData.MCSubClassID;
                    });

                    //
                    //for select machine
                    //masterData.Childs[0].ContactID = 0;
                    //masterData.Childs[0].Contact = "Empty";
                    //if (status != statusConstants.PENDING) {
                    masterData.Childs[0].MachineGauge = 0;
                    masterData.Childs[0].MachineDia = 0;
                    masterData.Childs[0].BrandID = 0;
                    masterData.Childs[0].Brand = "Empty";
                    masterData.Childs[0].KnittingMachineID = 0;
                    masterData.Childs[0].KnittingMachineNo = "Empty";


                    $formEl.find("#tblProgramInformation").bootstrapTable('load', masterData.Childs);
                    //}

                },
            });
            finder.showModal();
        });

        $formEl.find("#btnSelectMachine").on("click", function (e) {
            e.preventDefault();
            var MachineSubContractList;
            if ($formEl.find("#IsSubContact").is(':checked')) {
                MachineSubContractList = masterData.KnittingSubContracts;
            }
            else {
                if (masterData.SubGroupName === subGroupNames.FABRIC) {
                    MachineSubContractList = masterData.KnittingMachines.filter(function (el) {
                        return el.MachineSubClassID == masterData.MCSubClassID;
                    });
                } else {
                    MachineSubContractList = masterData.KnittingMachines.filter(function (el) {
                        return el.MachineSubClassID == masterData.MCSubClassID; //&& el.GG == masterData.MachineGauge;
                    });
                }
            }

            var finder = new commonFinder({
                title: "Select Machine",
                pageId: pageId,
                data: MachineSubContractList,
                isMultiselect: false,
                modalSize: "modal-md",
                top: "2px",
                primaryKeyColumn: "KnittingMachineID",
                fields: "GG,Dia,Brand,Contact,MachineNo,Capacity,IsSubContact",
                headerTexts: "GG,Dia,Brand,Unit,MachineNo,Capacity,SubContact?",
                widths: "30,30,100,70,50,50,50",
                onSelect: function (res) {
                    finder.hideModal();
                    masterData.Childs[0].KnittingMachineID = res.rowData.KnittingMachineID;
                    masterData.Childs[0].MachineGauge = res.rowData.GG;
                    masterData.Childs[0].MachineDia = res.rowData.Dia;
                    if ($formEl.find("#IsSubContact").is(':checked')) {
                        masterData.Childs[0].ContactID = res.rowData.ContactID;
                    } else {
                        masterData.Childs[0].ContactID = res.rowData.KnittingUnitID;
                    }

                    masterData.Childs[0].Contact = res.rowData.Contact;
                    masterData.Childs[0].BrandID = res.rowData.BrandID;
                    masterData.Childs[0].Brand = res.rowData.Brand;
                    masterData.Childs[0].KnittingMachineNo = res.rowData.MachineNo;
                    masterData.Childs[0].IsSubContact = res.rowData.IsSubContact;
                    $formEl.find("#tblProgramInformation").bootstrapTable('updateRow', { index: 0, row: masterData.Childs[0] });
                },
            });
            finder.showModal();
        });

        $formEl.find("#btnCopyProgram").on("click", function (e) {
            e.preventDefault();
            var pType;
            if (isConcept) {
                pType = knittingProgramType.CONCEPT;
            }
            else if (isBDS) {
                pType = knittingProgramType.BDS;
            }
            else {
                pType = knittingProgramType.BULK;
            }
            var finder = new commonFinder({
                title: "Select Knitting Program",
                pageId: pageId,
                //data: MachineSubContractList,
                apiEndPoint: `/api/knitting-program/list-by-mcsubclass?type=${pType}&mcSubClassId=${masterData.MCSubClassID}`,
                isMultiselect: false,
                modalSize: "modal-md",
                top: "2px",
                primaryKeyColumn: "KPMasterID",
                fields: "ConceptNo,Buyer,TechnicalName,GSM,ColorName,Composition",
                headerTexts: "Concept No,Buyer,Technical Name,GSM,Color,Composition",
                widths: "50,40,50,30,40,50",
                onSelect: function (res) {

                    finder.hideModal();
                    axios.get(`/api/knitting-program/${pType}/${res.rowData.KPMasterID}/${res.rowData.SubGroupName}`)
                        .then(function (response) {
                            if (response.data.Childs.length > 0) {
                                masterData.Childs[0].KnittingMachineID = response.data.Childs[0].KnittingMachineID;
                                masterData.Childs[0].MachineGauge = response.data.Childs[0].MachineGauge;
                                masterData.Childs[0].MachineDia = response.data.Childs[0].MachineDia;
                                masterData.Childs[0].ContactID = response.data.Childs[0].ContactID;
                                masterData.Childs[0].Contact = response.data.Childs[0].Contact;

                                masterData.Childs[0].KnittingMachineNo = response.data.Childs[0].KnittingMachineNo;
                                masterData.Childs[0].IsSubContact = response.data.Childs[0].IsSubContact;
                                masterData.Childs[0].FUPartID = response.data.Childs[0].FUPartID;
                                masterData.Childs[0].FUPartName = response.data.Childs[0].FUPartName;
                                masterData.Childs[0].KnittingTypeID = response.data.Childs[0].KnittingTypeID;
                                masterData.Childs[0].Needle = response.data.Childs[0].Needle;
                                masterData.Childs[0].CPI = response.data.Childs[0].CPI;
                                masterData.Childs[0].TotalNeedle = response.data.Childs[0].TotalNeedle;
                                masterData.Childs[0].TotalCourse = response.data.Childs[0].TotalCourse;
                                masterData.Childs[0].StartDate = response.data.Childs[0].StartDate;
                                masterData.Childs[0].EndDate = response.data.Childs[0].EndDate;
                                masterData.Childs[0].Remarks = response.data.Childs[0].Remarks;

                                masterData.Childs[0].BrandID = response.data.Childs[0].BrandID;
                                masterData.Childs[0].Brand = response.data.Childs[0].Brand;

                                $formEl.find("#tblProgramInformation").bootstrapTable('updateRow', { index: 0, row: masterData.Childs[0] });
                            }
                            if (response.data.Yarns.length > 0) {
                                masterData.Yarns.forEach(function (row) {
                                    var yarn = response.data.Yarns.find(function (el) {
                                        return el.YarnCount == row.YarnCount && el.YarnType == row.YarnType
                                    });
                                    if (yarn) {
                                        if (canChangeItemInfo(yarn)) {
                                            row.PhysicalCount = yarn.PhysicalCount;
                                            row.YarnLotNo = yarn.YarnLotNo;
                                            row.YarnBrandID = yarn.YarnBrandID;
                                        }
                                        row.StitchLength = yarn.StitchLength;
                                        row.YarnPly = yarn.YarnPly;
                                    }
                                });
                                initTblYarn(masterData.Yarns);
                            }
                        })
                        .catch(function (err) {
                            toastr.error(err.response.data.Message);
                        });
                },
            });
            finder.showModal();
        });

        $formEl.find("#btnCancel").on("click", backToList);

    });

    function initMasterTable() {
      

        var commands = status == statusConstants.PENDING
            ? [{ type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }]
            : [{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }];

        var columns = [
            {
                headerText: '', commands: commands, width: 40, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'ReqDeliveryDate', headerText: 'Req. DeliveryDate', textAlign: 'Right', type: 'date', format: _ch_date_format_1, visible: status === statusConstants.COMPLETED,
            },
            {
                field: 'RevisionPendingStatus', headerText: 'Revision Status', visible: status == statusConstants.PENDING, width: 60
            },
            {
                field: 'PlanNo', headerText: 'Program No', width: 100, textAlign: 'Center', headerTextAlign: 'Center', visible: false //visible: (status != statusConstants.PENDING),
            },
            {
                field: 'ConceptID', isPrimaryKey: true, headerText: 'Concept ID', width: 100, visible: false
            },
            {
                field: 'ConceptNo', headerText: 'Concept No', width: 100
            },
            {
                field: 'DateAdded', headerText: 'Program Date', textAlign: 'Right', type: 'date', format: _ch_date_format_1, width: 65, textAlign: 'Center', headerTextAlign: 'Center', visible: (status == statusConstants.ACTIVE), width: 80
            },
            {
                field: 'ConceptDate', headerText: 'Concept Date', textAlign: 'Right', type: 'date', format: _ch_date_format_1, width: 100, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'SubGroupName', headerText: 'Sub Group', width: 60, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'BookingQty', headerText: 'Booking Qty', width: 100
            },
            {
                field: 'ProduceKnittingQty', headerText: 'Knitting Prod Qty', width: 100, headerTextAlign: 'Center', visible: isBulkPage
            },
            {
                field: 'PlanQty', headerText: 'Planned Qty', width: 100, visible: isBulkPage && status != statusConstants.PENDING
            },
            {
                field: 'RemainingPlanQty', headerText: 'Remaining Plan Qty', width: 100, visible: isBulkPage
            },
            {
                field: 'Uom', headerText: 'UOM', width: 100, visible: isBulkPage
            },
            {
                field: 'Contact', headerText: 'Floor/Sub-Contractor', width: 100, visible: isBulkPage & status != statusConstants.PENDING
            },
            {
                field: 'UsesIn', headerText: 'Uses In', width: 100, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'Buyer', headerText: 'Buyer', width: 100, headerTextAlign: 'Center', visible: (isBDS || isBulkPage)
            },
            {
                field: 'BuyerTeam', headerText: 'Buyer Team', width: 100, headerTextAlign: 'Center', visible: (isBDS || isBulkPage)
            },
            {
                field: 'KnittingType', headerText: 'Machine Type', width: 70, visible: false
            },
            {
                field: 'TechnicalName', headerText: 'Technical Name', width: 70
            },
            {
                field: 'Composition', headerText: 'Composition', width: 120
            },
            {
                field: 'ColorName', headerText: 'Color', width: 70
            },
            {
                field: 'GSM', headerText: 'GSM', width: 40, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'RevisionNo', headerText: 'Revision No', visible: (status != statusConstants.PENDING), width: 65
            },
            {
                field: 'RevisionDate', headerText: 'Revision Date', textAlign: 'Right', type: 'date', format: _ch_date_format_1, textAlign: 'Center', headerTextAlign: 'Center', visible: (status != statusConstants.PENDING), width: 80
            },
            {
                field: 'Size', headerText: 'Size', width: 40, visible: !isConcept
            },
            {
                field: 'Qty', headerText: 'QTY', width: 40, textAlign: 'Center', headerTextAlign: 'Center', visible: isConcept
            },
            {
                field: 'TotalQty', headerText: 'Plan Qty', width: 40, textAlign: 'Center', headerTextAlign: 'Center', visible: isConcept
            },
            {
                field: 'RemainingQty', headerText: 'Remaining Qty', width: 52, textAlign: 'Center', headerTextAlign: 'Center', visible: isConcept
            },
            {
                field: 'Active', headerText: 'Active?', visible: status == statusConstants.ALL, width: 44, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'UserName', headerText: 'Concept By', width: 60, visible: isConcept
            }
        ];

        var programType;
        if (isConcept) {
            programType = knittingProgramType.CONCEPT;
        }
        else if (isBDS) {
            programType = knittingProgramType.BDS;
        }
        else {
            programType = knittingProgramType.BULK;
        }
        //var programType = isBDSPage ? knittingProgramType.BDS : knittingProgramType.CONCEPT;

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/knitting-program/list?type=${programType}&status=${status}`,
            columns: columns,
            autofitColumns: false,
            allowSorting: true,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {

        if (status === statusConstants.PENDING) {

            if (!args.rowData.RevisionPending) getNew(args.rowData.ConceptID, args.rowData.WithoutOB, args.rowData.SubGroupName);
            else getRevisionedDetails(args.rowData.KPMasterID, args.rowData.ConceptID, args.rowData.SubGroupName);
            $formEl.find("#btnCopyProgram").fadeIn();
            $formEl.find("#btnReviseAndComplete").fadeOut();
        }
        else {
            getDetails(args.rowData.KPMasterID, args.rowData.SubGroupName);
            $formEl.find("#btnCopyProgram").fadeOut();
        }
    }
    function is100Elastane(composition) {
        if (!composition) return false;
        if (~composition.indexOf("100") && ~composition.toLowerCase().indexOf("elastane")) {
            return true;
        } else {
            return false;
        }
    }
    function isDisplayStitchLength(records) {
        var displayStitchLength = false;
        if (masterData.SubGroupName === subGroupNames.FABRIC) {
            displayStitchLength = true;
            for (var iR = 0; iR < records.length; iR++) {
                var composition = records[iR].Composition.toLowerCase();
                if (is100Elastane(composition)) {
                    //displayStitchLength = false;
                } else {
                    displayStitchLength = true;
                    break;
                }
            }
        }
        return displayStitchLength;
    }
    function initTblYarn(records) {
        $tblYarnEl.bootstrapTable("destroy");
        $tblYarnEl.bootstrapTable({
            uniqueId: 'KPYarnID',
            editable: true,
            columns: [
                {
                    title: "Actions",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        //if (!row.IsStockItemFromMR && !row.YDItem) {
                        if (!row.YDItem && !isBulkPage) {
                            return [
                                '<span class="btn-group">',
                                '<a class="btn btn-success btn-xs add" href="javascript:void(0)" title="Add a row like this">',
                                '<i class="fa fa-plus"></i>',
                                '</a>',
                                '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Remove this row">',
                                '<i class="fas fa-times"></i>',
                                '</a>',
                                '<a class="btn btn-warning btn-xs getFromStock" href="javascript:void(0)" title="Get from stock">',
                                '<i class="fab fa-dropbox"></i>',
                                '</a>',
                                '</span>'
                            ].join('');
                        }
                        else {
                            return [
                                '<span class="btn-group">',
                                '<a class="btn btn-success btn-xs add" href="javascript:void(0)" title="Add a row like this">',
                                '<i class="fa fa-plus"></i>',
                                '</a>',
                                '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Remove this row">',
                                '<i class="fas fa-times"></i>',
                                '</a>',
                                '</span>'
                            ].join('');
                        }
                    },
                    events: {
                        'click .add': function (e, value, row, index) {
                            addNewYarnItem(row);
                        },
                        'click .remove': function (e, value, row, index) {
                            masterData.Yarns.splice(index, 1);
                            $tblYarnEl.bootstrapTable('load', masterData.Yarns);
                        },
                        'click .getFromStock': function (e, value, row, index) {
                            var otherQuery = ` AND YarnCategory = '${row.YarnCategory}' AND (SampleStockQty > 0 OR AdvanceStockQty > 0) `;
                            otherQuery = replaceInvalidChar(otherQuery);
                            var finder = new commonFinder({
                                title: "Yarn Stock",
                                pageId: pageId,
                                height: 320,
                                modalSize: "modal-lg",
                                apiEndPoint: `/api/yarn-stock-adjustment/get-all-stocks-with-custom-query/valid-item/${otherQuery}`,
                                headerTexts: "Yarn Detail,Count,Physical Count,Lot No,Shade Code,Supplier,Spinner,Sample Stock Qty,Advance Stock Qty,Block Sample Stock Qty,Block Advance Stock Qty,Issued Qty,Item Type,Note",
                                fields: "YarnCategory,Count,PhysicalCount,YarnLotNo,ShadeCode,SupplierName,SpinnerName,SampleStockQty,AdvanceStockQty,BlockSampleStockQty,BlockAdvanceStockQty,TotalIssueQty,InvalidItem_St,Note",
                                primaryKeyColumn: "YarnStockSetId",
                                autofitColumns: true,
                                onSelect: function (res) {
                                    finder.hideModal();
                                    masterData.Yarns[index].YarnStockSet = res.rowData;

                                    masterData.Yarns[index].PhysicalCount = res.rowData.PhysicalCount;
                                    masterData.Yarns[index].YarnLotNo = res.rowData.YarnLotNo;
                                    masterData.Yarns[index].YarnBrandID = res.rowData.SpinnerId;
                                    masterData.Yarns[index].Spinner = res.rowData.SpinnerName;
                                    masterData.Yarns[index].IsStockItem = "Yes";
                                    masterData.Yarns[index].YarnStockSetId = res.rowData.YarnStockSetId;

                                    masterData.Yarns[index].IsInvalidItem = res.rowData.IsInvalidItem;
                                    masterData.Yarns[index].StockItemNote = res.rowData.Note;

                                    $tblYarnEl.bootstrapTable('load', masterData.Yarns);
                                }
                            });
                            finder.showModal();
                        }
                    }
                },
                {
                    field: "FCMRChildID",
                    title: "FCMRChildID",
                    filterControl: "input",
                    align: 'center',
                    visible: false
                },
                {
                    field: "Composition",
                    title: "Composition",
                    filterControl: "input",
                    align: 'center',
                    visible: false
                },
                {
                    field: "YarnCount",
                    title: "Yarn Count",
                    filterControl: "input",
                    align: 'center',
                    visible: true
                },
                {
                    field: "YarnType",
                    title: "Yarn Type",
                    filterControl: "input",
                    align: 'center',
                    visible: true
                },
                {
                    field: "YarnCategory",
                    title: "Yarn Details",
                    filterControl: "input",
                    align: 'center'
                },
                {
                    field: "PhysicalCount",
                    title: "Physical Count",
                    filterControl: "input",
                    align: 'center',
                    visible: !isBulkPage,
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" style="padding-right: 10px;">'
                    },
                },
                {
                    field: "YarnLotNo",
                    title: "Yarn Lot",
                    filterControl: "input",
                    align: 'center',
                    visible: !isBulkPage,
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" style="padding-right: 10px;">'
                    }
                },
                //{
                //    field: "YD",
                //    title: "YD?",
                //    formatter: function (value, row, index, field) {
                //        return value ? "Yes" : "No";
                //    }
                //},
                {
                    field: "YDItem",
                    title: "YD Item?",
                    formatter: function (value, row, index, field) {
                        return value ? "Yes" : "No";
                    }
                },
                {
                    field: "BatchNo",
                    title: "Batch No",
                    filterControl: "input",
                    align: 'center',
                    visible: !isBulkPage,
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" style="padding-right: 10px;">'
                    }
                },
                //{
                //    field: "BatchNo",
                //    title: "Batch No",
                //    filterControl: "input",
                //    align: 'center',
                //    //editable: {
                //    //    type: "text",
                //    //    showbuttons: false,
                //    //    tpl: '<input type="text" class="form-control input-sm" style="padding-right: 10px;">'
                //    //}
                //},
                {
                    field: "YarnBrandID",
                    title: "Spinner",
                    filterControl: "input",
                    align: 'center',
                    visible: !isBulkPage,
                    editable: {
                        type: 'select2',
                        title: 'Select',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: masterData.YarnBrandList,
                        select2: { width: 200, placeholder: 'Select', allowClear: true }
                    }
                },
                {
                    field: "YarnPly",
                    title: "Yarn Ply",
                    filterControl: "input",
                    align: 'center',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        //tpl: '<input type="number" step=".001" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseFloat(value)) || parseFloat(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                        }
                    }
                },
                {
                    field: "StitchLength",
                    title: "Stitch Length",
                    //filterControl: "input",
                    align: 'center',
                    visible: isDisplayStitchLength(records),
                    formatter: function (value, row, index, field) {
                        if (masterData.SubGroupName === subGroupNames.FABRIC) {
                            var composition = row.Composition;
                            if (is100Elastane(composition)) {
                                return 0;
                            } else {
                                return value;
                            }
                        } else {
                            return value;
                        }
                    },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        //tpl: '<input type="number" step=".001" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseFloat(value)) || parseFloat(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                        }
                    }
                },
                {
                    field: "IsStockItem",
                    title: "Is Stock Item?",
                    visible: !isBulkPage,
                    formatter: function (value, row, index, field) {
                        return value ? "Yes" : "No";
                    }
                }
                //,
                //{
                //    field: "YDColorName",
                //    title: "YD Color",
                //    filterControl: "input",
                //    align: 'center'
                //},
                //{
                //    field: "YDBookingForName",
                //    title: "YD Booking For",
                //    filterControl: "input",
                //    align: 'center'
                //}
            ],
            data: records
        });
    }

    function addNewYarnItem(row) {
        var obj = {
            KPYarnID: getMaxIdForArray(masterData.Yarns, "KPYarnID"),
            KPMasterID: row.KPMasterID,
            FCMRChildID: row.FCMRChildID,
            YarnCountID: row.YarnCountID,
            YarnTypeID: row.YarnTypeID,
            YarnCount: row.YarnCount,
            YarnType: row.YarnType,
            YarnLotNo: row.YarnLotNo,
            YarnBrandID: row.YarnBrandID,
            ItemMasterID: row.ItemMasterID,
            YarnPly: 0,
            StitchLength: 0,
            BatchNo: row.BatchNo,
            PhysicalCount: row.PhysicalCount,
            YarnCategory: row.YarnCategory,
            //YD: row.YD
            YDItem: row.YDItem,
            YarnStockSetId: row.YarnStockSetId,
            IsStockItem: row.IsStockItem

        }
      
        //YarnStockSet: row.YarnStockSet
        if (typeof row.YarnStockSet !== "undefined" && row.YarnStockSet != null) {
            obj.YarnStockSet = row.YarnStockSet;
        }
        masterData.Yarns.push(obj);
        $tblYarnEl.bootstrapTable('load', masterData.Yarns);
    }

    function getNewItem() {
        return {
            KPChildID: getMaxIdForArray(masterData.Childs, "KPChildID"),
            ConceptID: masterData.ConceptID,
            SubGroupID: masterData.SubGroupID,
            FabricConstruction: "",//masterData.Construction,
            FabricComposition: "",//masterData.Composition,
            TechnicalName: "",
            CCColorID: 0,
            ColorName: "",
            FabricGsm: 0,
            FabricWidth: 0,
            DyeingType: "",
            KnittingType: "", //masterData.KnittingType
            KnittingTypeID: 0,
            FUPartName: masterData.FUPartName,
            Needle: 0,
            CPI: 0,
            TotalNeedle: 0,
            TotalCourse: 0,
            MachineDia: isBulkPage ? masterData.MachineDia : 0,
            MachineGauge: isBulkPage ? masterData.MachineGauge : masterData.SubGroupName === subGroupNames.FABRIC ? 0 : masterData.MachineGauge,
            StartDate: new Date(),
            EndDate: new Date(),
            Uom: (masterData.SubGroupName === subGroupNames.FABRIC) ? "Kg" : "Pcs",
            BookingQty: 0,
            KJobCardQty: 0,
            MCSubClassID: masterData.MCSubClassID,
            MCSubClassName: "",
            KJobCardNo: '**<< NEW >>**',
            Remarks: " ",
            KJobCardMasters: [],
            Contact: "",
            BrandID: isBulkPage ? masterData.BrandID : 0,
            Brand: isBulkPage ? masterData.Brand : "",
            KnittingMachineNo: "",
            Length: masterData.Length,
            Width: masterData.Width,
            IsSubContact: masterData.IsSubContact
        }
    }

    function initTblFabricChild() {
        $formEl.find("#tblProgramInformation")
            .bootstrapTable('destroy')
            .bootstrapTable({
                uniqueId: 'ItemMasterID',
                //detailView: true,
                checkboxHeader: false,
                columns: [
                    {
                        title: "Actions",
                        align: "center",
                        visible: masterData.KPMasterID,
                        formatter: function (value, row, index, field) {
                            if (masterData.SubGroupID == 1) {
                                return [
                                    '<span class="btn-group">',
                                    `<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportName=KnittingJobCard.rdl&JobCardNo=${row.KJobCardNo}" target="_blank" title="Job Card Report For Fabric">
                                <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                </a>`,
                                    '</span>'
                                ].join('');
                            }
                            else {
                                return [
                                    '<span class="btn-group">',
                                    `<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportName=KnittingJobCardCC.rdl&JobCardNo=${row.KJobCardNo}" target="_blank" title="Job Card Report For Other Item">
                                <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                </a>`,
                                    '</span>'
                                ].join('');
                            }
                        }
                    },
                    {
                        field: "MachineGauge",
                        title: "Machine Gauge",
                        width: 100
                    },
                    {
                        field: "MachineDia",
                        title: "Machine Dia",
                        width: 100
                    },
                    {
                        field: "IsSubContact",
                        title: "Sub Contact?",
                        checkboxEnabled: false,
                        width: 80,
                        checkbox: true,
                        showSelectTitle: true,
                        visible: true
                    },
                    {
                        field: "Brand", //BrandID
                        title: "Brand",
                        width: 100
                    },
                    {
                        field: "Contact", //ContactID
                        title: "Floor/Sub-Contractor",
                        width: 100
                        //visible: false
                    },
                    {
                        field: "KnittingMachineNo", //KnittingMachineID
                        title: "Machine",
                        width: 120,
                        visible: !isBulkPage
                    },
                    {
                        field: "FUPartName",
                        title: "End Use",
                        visible: (masterData.SubGroupID != 1), //1 = fabric
                        width: 100
                    },
                    {
                        field: "KnittingTypeID",
                        title: "Knitting Type",
                        visible: (masterData.SubGroupID == 1), //1 = fabric
                        align: 'center',
                        editable: {
                            type: 'select2',
                            title: 'Select Knitting Type',
                            inputclass: 'input-sm',
                            showbuttons: false,
                            source: masterData.KnittingTypeList,
                            select2: { width: 130, placeholder: 'Knitting Type', allowClear: true }
                        },
                    },
                    {
                        field: "Needle",
                        title: "Needle/cm",
                        visible: (masterData.SubGroupID != 1), //1 = fabric
                        align: 'center',
                        editable: {
                            type: "text",
                            showbuttons: false,
                            tpl: '<input type="number" step=".001" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                            validate: function (value) {
                                if (!value || !value.trim() || isNaN(parseFloat(value)) || parseFloat(value) <= 0) {
                                    return 'Must be a positive value.';
                                }
                            }
                        }
                    },
                    {
                        field: "CPI",
                        title: "CPI",
                        visible: (masterData.SubGroupID != 1), //1 = fabric
                        align: 'center',
                        editable: {
                            type: "text",
                            showbuttons: false,
                            tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                            validate: function (value) {
                                if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                    return 'Must be a positive integer.';
                                }
                            }
                        }
                    },
                    {
                        field: "TotalNeedle",
                        title: "Total Needle",
                        visible: (masterData.SubGroupID != 1), //1 = fabric
                        align: 'center'
                    },
                    {
                        field: "TotalCourse",
                        title: "Total Course",
                        visible: (masterData.SubGroupID != 1), //1 = fabric
                        align: 'center'
                    },
                    {
                        field: "StartDate",
                        title: "Start Date",
                        filterControl: "input",
                        formatter: function (value, row, index, field) {
                            return formatDateToDefault(value);
                        },
                        editable: {
                            type: "text",
                            showbuttons: false,
                            tpl: '<input type="date" class="form-control input-sm" style="padding-right: 24px;">'
                        }
                    },
                    {
                        field: "EndDate",
                        title: "End Date",
                        filterControl: "input",
                        formatter: function (value, row, index, field) {
                            return formatDateToDefault(value);
                        },
                        editable: {
                            type: "text",
                            showbuttons: false,
                            tpl: '<input type="date" class="form-control input-sm" style="padding-right: 24px;">'
                        }
                    },
                    {
                        field: "Uom",
                        title: "Unit",
                        filterControl: "input"
                    },
                    {
                        field: "MaxQty",
                        title: "MaxQty",
                        filterControl: "input",
                        visible: false
                    },
                    {
                        field: "BookingQty",
                        title: "Qty",
                        align: 'center',
                        editable: {
                            type: "text",
                            showbuttons: false,
                            //tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                            validate: function (value) {
                                if (!value || !value.trim() || isNaN(parseFloat(value)) || parseFloat(value) <= 0) {
                                    return 'Must be a positive value.';
                                }
                            }
                        }
                    },
                    {
                        field: "ItemMasterID",
                        title: "ItemMasterID",
                        width: 100,
                        visible: false
                    },
                    {
                        field: "KJobCardMasterID",
                        title: "KJobCardMasterID",
                        width: 100,
                        visible: false
                    },
                    {
                        field: "KJobCardNo",
                        title: "Job Card No",
                        cellStyle: function () { return { classes: 'm-w-100' } },
                        visible: !isBulkPage
                    },
                    {
                        field: "Remarks",
                        title: "Remarks",
                        filterControl: "input",
                        align: 'center',
                        editable: {
                            type: "text",
                            showbuttons: false,
                            tpl: '<input type="text" class="form-control input-sm" style="padding-right: 10px;">'
                        }
                    }
                ],
                onCheck: function (row, $element) {
                    if (row.IsSubContact) {
                        ResetNextColoum(row);
                    }
                },
                onEditableSave: function (field, row, oldValue, $el) {
                    //if (field == "BookingQty") {
                    //    var remainingQty = $formEl.find("#RemainingQty").val();
                    //    var rowPlanQty = $formEl.find("#RowPlanedQty").val();
                    //    if (status == statusConstants.PENDING) {
                    //        if (row.BookingQty > parseFloat(remainingQty)) {
                    //            showBootboxAlert('Quantity can not more than ' + parseFloat(remainingQty));
                    //            row.BookingQty = oldValue;
                    //        }
                    //    }
                    //    else {
                    //        if (row.BookingQty > parseFloat(remainingQty) + parseFloat(rowPlanQty)) {
                    //            showBootboxAlert('Quantity can not more than ' + (parseFloat(remainingQty) + parseFloat(rowPlanQty)));
                    //            row.BookingQty = oldValue;
                    //        }
                    //    }
                    //}
                    //else 
                    if (field == "Needle") {
                        row.TotalNeedle = Math.ceil((row.Length + (row.Length * (6 / 100))) * row.Needle);
                    }
                    else if (field == "CPI") {
                        row.TotalCourse = Math.ceil((row.Width + (row.Width * (6 / 100))) * (row.CPI / 2.54));
                    }
                    $formEl.find("#tblProgramInformation").bootstrapTable('load', masterData.Childs);
                }
            });
    }

    function getChildBrandByGaugeAndSubclass(subClassId, machineDia, contactId, rowData) {
        var url = `/api/selectoption/get-brand-by-machine-gauge-dia-and-subclass/${subClassId}/${machineDia}/${contactId}`;

        axios.get(url)
            .then(function (response) {
                if (response.data.length == 0)
                    return toastr.warning("No data Found!");
                var list = convertToSelectOptions(response.data);
                showBootboxSelectPrompt("Select One", list, "", function (result) {
                    if (!result) return toastr.warning("You didn't selected!");

                    if (rowData.BrandID != result) {
                        rowData.KnittingMachineID = 0;
                        rowData.KnittingMachineNo = "Empty";
                        //for (var i = 0; i < rowData.KJobCardMasters.length; i++) {
                        //    $el = $("#TblJobCardTable-" + rowData.KJobCardMasters[i].KJobCardMasterID);
                        //    $el.bootstrapTable('updateByUniqueId', { id: rowData.KJobCardMasters[i].KJobCardMasterID, row: rowData.KJobCardMasters[i] });
                        //}
                    }

                    var selectedRes = list.find(function (el) { return el.value === result })
                    rowData.BrandID = (result == "") ? 0 : result;
                    rowData.Brand = selectedRes.text;
                    $formEl.find("#tblProgramInformation").bootstrapTable('updateByUniqueId', { id: rowData.KPChildID, row: rowData });
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getChildMachine(rowData) {
        var url = `/api/selectoption/get-machine-by-contact-knitting-type-gauge-dia/${rowData.MCSubClassID}/${rowData.BrandID}/${rowData.ContactID}/${masterData.KnittingTypeID}/${rowData.MachineDia}`;
        axios.get(url)
            .then(function (response) {
                if (response.data.length == 0) return toastr.warning("No Machine Found!");

                var mcList = convertToSelectOptions(response.data);
                showBootboxSelectPrompt("Select Machine", mcList, "", function (result) {
                    if (!result) return toastr.warning("You didn't selected any Machine!");
                    //if (rowData.KnittingMachineID != result) {
                    //    for (var i = 0; i < rowData.KJobCardMasters.length; i++) {
                    //        $el = $("#TblJobCardTable-" + rowData.KJobCardMasters[i].KJobCardMasterID);
                    //        $el.bootstrapTable('updateByUniqueId', { id: rowData.KJobCardMasters[i].KJobCardMasterID, row: rowData.KJobCardMasters[i] });
                    //    }
                    //}
                    var selectedMC = mcList.find(function (el) { return el.value === result })
                    rowData.KnittingMachineID = (result == "") ? 0 : result;
                    rowData.KnittingMachineNo = selectedMC.text;
                    $formEl.find("#tblProgramInformation").bootstrapTable('updateByUniqueId', { id: rowData.KPChildID, row: rowData });
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getMachineGauge(subClassId, rowData) {
        var url = `/api/selectoption/get-machine-gauge-by-subclass/${subClassId}`;

        axios.get(url)
            .then(function (response) {
                if (response.data.length == 0)
                    return toastr.warning("No Gauge Found!");
                var list = convertToSelectOptions(response.data);
                showBootboxSelectPrompt("Select One", list, "", function (result) {
                    if (!result) return toastr.warning("You didn't selected!");

                    if (rowData.MachineGauge != result) {
                        rowData.MachineDia = 0;
                        rowData.IsSubContact = false;
                        rowData.ContactID = 0;
                        rowData.Contact = "Empty";
                        rowData.BrandID = 0;
                        rowData.Brand = "Empty";
                        rowData.KnittingMachineID = 0;
                        rowData.KnittingMachineNo = "Empty";
                        //for (var i = 0; i < rowData.KJobCardMasters.length; i++) {
                        //    $el = $("#TblJobCardTable-" + rowData.KJobCardMasters[i].KJobCardMasterID);
                        //    $el.bootstrapTable('updateByUniqueId', { id: rowData.KJobCardMasters[i].KJobCardMasterID, row: rowData.KJobCardMasters[i] });
                        //}
                    }

                    rowData.MachineGauge = (result == "") ? 0 : result;
                    $formEl.find("#tblProgramInformation").bootstrapTable('updateByUniqueId', { id: rowData.KPChildID, row: rowData });
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getChildMachineDia(subClassId, rowData) {
        var url = `/api/selectoption/get-machine-dia-by-subclass-guage/${subClassId}`;

        axios.get(url)
            .then(function (response) {
                if (response.data.length == 0)
                    return toastr.warning("No Dia Found!");
                var list = convertToSelectOptions(response.data);
                showBootboxSelectPrompt("Select One", list, "", function (result) {
                    if (!result) return toastr.warning("You didn't selected!");
                    if (rowData.MachineDia != result) {
                        rowData.IsSubContact = false;
                        rowData.ContactID = 0;
                        rowData.Contact = "Empty";
                        rowData.BrandID = 0;
                        rowData.Brand = "Empty";
                        rowData.KnittingMachineID = 0;
                        rowData.KnittingMachineNo = "Empty";
                        //for (var i = 0; i < rowData.KJobCardMasters.length; i++) {
                        //    $el = $("#TblJobCardTable-" + rowData.KJobCardMasters[i].KJobCardMasterID);
                        //    $el.bootstrapTable('updateByUniqueId', { id: rowData.KJobCardMasters[i].KJobCardMasterID, row: rowData.KJobCardMasters[i] });
                        //}
                    }

                    rowData.MachineDia = (result == "") ? 0 : result;
                    $formEl.find("#tblProgramInformation").bootstrapTable('updateByUniqueId', { id: rowData.KPChildID, row: rowData });
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function ResetNextColoum(rowData) {
        rowData.ContactID = 0;
        rowData.Contact = "Empty";
        rowData.BrandID = 0;
        rowData.Brand = "Empty";
        rowData.KnittingMachineID = 0;
        rowData.KnittingMachineNo = "Empty";
        $formEl.find("#tblProgramInformation").bootstrapTable('updateByUniqueId', { id: rowData.KPChildID, row: rowData });
    }

    function getChildContact(subClassId, machineDia, isSubContact, rowData) {
        var url = '';
        if (isSubContact)
            url = `/api/selectoption/get-contacts-by-contact-category/${contactCategoryConstants.SUB_CONTUCT}`;
        else
            url = `/api/selectoption/get-sub-contract-by-subclass-gg-dia/${subClassId}/${machineDia}`;

        axios.get(url)
            .then(function (response) {
                if (response.data.length == 0)
                    return toastr.warning("No Contact Found!");
                var contactList = convertToSelectOptions(response.data);
                showBootboxSelectPrompt("Select Contact", contactList, "", function (result) {
                    if (!result)
                        return toastr.warning("You didn't selected any Contact!");
                    rowData.BrandID = 0;
                    rowData.Brand = "Empty";
                    rowData.KnittingMachineID = 0;
                    rowData.KnittingMachineNo = "Empty";
                    //for (var i = 0; i < rowData.KJobCardMasters.length; i++) {
                    //    $el = $("#TblJobCardTable-" + rowData.KJobCardMasters[i].KJobCardMasterID);
                    //    $el.bootstrapTable('updateByUniqueId', { id: rowData.KJobCardMasters[i].KJobCardMasterID, row: rowData.KJobCardMasters[i] });
                    //}
                    var selectedContact = contactList.find(function (el) { return el.value === result })
                    rowData.ContactID = (result == "") ? 0 : result;
                    rowData.Contact = selectedContact.text;
                    $formEl.find("#tblProgramInformation").bootstrapTable('updateByUniqueId', { id: rowData.KPChildID, row: rowData });
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        $tblMasterEl.refresh();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#KPMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getNew(conceptID, withoutOB, subGroupName) {

        axios.get(`/api/knitting-program/new-kp/${conceptID}/${isBulkPage}/${withoutOB}/${subGroupName}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ReqDeliveryDate = formatDateToDefault(new Date());
                masterData.StartDate = formatDateToDefault(masterData.StartDate);
                masterData.EndDate = formatDateToDefault(masterData.EndDate);
                masterData.ActualStartDate = formatDateToDefault(masterData.ActualStartDate);
                masterData.ActualEndDate = formatDateToDefault(masterData.ActualEndDate);
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);

                if (isBulkPage) {
                    masterData.RemainingQty = (masterData.Qty - masterData.TotalQty).toFixed(2);
                } else {
                    masterData.RemainingQty = (masterData.TotalQty - masterData.PlanQty).toFixed(2);
                }


                //masterData.NeedPreFinishingProcess = true;
                setFormData($formEl, masterData);

                $tblChildEl.bootstrapTable("load", masterData.Childs);
                $tblChildEl.bootstrapTable('hideLoading');

                masterData.Yarns.map(x => {
                    x.YarnPly = masterData.SubGroupID == 1 ? 1 : 0;
                });

                initTblYarn(masterData.Yarns);

                //NeedPreFinishingProcess
                if (masterData.ProcessTime > 0) {
                    $formEl.find("#need-yes").prop("checked", true);
                    $formEl.find("#need-no").prop("checked", false);
                }
                else {
                    $formEl.find("#need-yes").prop("checked", false);
                    $formEl.find("#need-no").prop("checked", true);
                }
                if (status == statusConstants.ALL) {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnRevise").fadeOut();
                }
                else {
                    $formEl.find("#btnSave").fadeIn();
                    $formEl.find("#btnRevise").fadeOut();
                }
                //if (masterData.SubGroupName === subGroupNames.FABRIC)
                initTblFabricChild();

                if (isConcept) {
                    $formEl.find("#need-yes").prop("checked", masterData.NeedPreFinishingProcess);
                    $formEl.find("#need-no").prop("checked", !masterData.NeedPreFinishingProcess);
                }

                var childObject = getNewItem();

                if (isBulkPage) {
                    childObject.BookingQty = parseFloat(masterData.RemainingQty);
                } else {
                    childObject.BookingQty = parseFloat(masterData.TotalQty);
                }
                childObject.MaxQty = parseFloat(masterData.MaxQty);
                masterData.Childs.push(childObject);

                $formEl.find("#tblProgramInformation").bootstrapTable('load', masterData.Childs);

                if (masterData.SubGroupName === subGroupNames.FABRIC) {
                    $formEl.find("#divGSM,#divComposition").fadeIn();
                    $formEl.find("#divLength,#divWidth").fadeOut();
                } else {
                    $formEl.find("#divGSM,#divComposition").fadeOut();
                    $formEl.find("#divLength,#divWidth").fadeIn();
                }

                if (isBDS || isBulkPage) {
                    $formEl.find(".bds").fadeIn();
                } else {
                    $formEl.find(".bds").fadeOut();
                }

                initNewAttachment($formEl.find("#UploadFile"));
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getRevisionedDetails(id, ConceptID, subGroupName) {

        axios.get(`/api/knitting-program/revision/${id}/${ConceptID}/${subGroupName}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.ReqDeliveryDate = formatDateToDefault(masterData.ReqDeliveryDate);
                masterData.StartDate = formatDateToDefault(masterData.StartDate);
                masterData.EndDate = formatDateToDefault(masterData.EndDate);
                masterData.ActualStartDate = formatDateToDefault(masterData.ActualStartDate);
                masterData.ActualEndDate = formatDateToDefault(masterData.ActualEndDate);
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);


                if (isBulkPage) {
                    masterData.RemainingQty = masterData.Qty - masterData.TotalQty;
                } else {
                    masterData.RemainingQty = masterData.TotalQty - masterData.PlanQty;
                }

                setFormData($formEl, masterData);

                //NeedPreFinishingProcess
                if (masterData.NeedPreFinishingProcess > 0) {
                    $formEl.find("#need-yes").prop("checked", true);
                    $formEl.find("#need-no").prop("checked", false);
                }
                else {
                    $formEl.find("#need-yes").prop("checked", false);
                    $formEl.find("#need-no").prop("checked", true);
                }

                initTblYarn(masterData.Yarns);
                if (status == statusConstants.ALL) {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnRevise").fadeOut();
                } else {
                    $formEl.find("#btnRevise").fadeIn();
                    $formEl.find("#btnSave").fadeOut();
                }
                //if (masterData.SubGroupName === subGroupNames.FABRIC)
                initTblFabricChild();

                $formEl.find("#tblProgramInformation").bootstrapTable("load", masterData.Childs);
                $formEl.find("#tblProgramInformation").bootstrapTable('hideLoading');

                if (masterData.SubGroupName === subGroupNames.FABRIC) {
                    $formEl.find("#divGSM,#divComposition").fadeIn();
                    $formEl.find("#divLength,#divWidth").fadeOut();
                } else {
                    $formEl.find("#divGSM,#divComposition").fadeOut();
                    $formEl.find("#divLength,#divWidth").fadeIn();
                }

                if (isBDS || isBulkPage) {
                    $formEl.find(".bds").fadeIn();
                } else {
                    $formEl.find(".bds").fadeOut();
                }

                if (isConcept) {
                    $formEl.find("#need-yes").prop("checked", masterData.NeedPreFinishingProcess);
                    $formEl.find("#need-no").prop("checked", !masterData.NeedPreFinishingProcess);
                }

                //IsSubContact
                if (masterData.IsSubContact == 1) {
                    $formEl.find("#IsSubContact").prop("checked", true);
                } else {
                    $formEl.find("#IsSubContact").prop("checked", false);
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id, subgroupName) {

        var type = knittingProgramType.CONCEPT;
        if (isBDS) type = knittingProgramType.BDS;
        else if (isBulkPage) type = knittingProgramType.BULK;
        axios.get(`/api/knitting-program/${type}/${id}/${subgroupName}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.BAnalysisDate = formatDateToDefault(masterData.BAnalysisDate);
                masterData.ReqDeliveryDate = formatDateToDefault(masterData.ReqDeliveryDate);
                masterData.StartDate = formatDateToDefault(masterData.StartDate);
                masterData.EndDate = formatDateToDefault(masterData.EndDate);
                masterData.ActualStartDate = formatDateToDefault(masterData.ActualStartDate);
                masterData.ActualEndDate = formatDateToDefault(masterData.ActualEndDate);
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);


                if (isBulkPage) {
                    masterData.RemainingQty = masterData.Qty - masterData.TotalQty;
                } else {
                    masterData.RemainingQty = masterData.TotalQty - masterData.PlanQty;
                }

                masterData.RowPlanedQty = masterData.PlanQty;
                masterData.PlanQty = masterData.TotalPlanedQty;
                setFormData($formEl, masterData);

                _yarnPrevInfoList = DeepClone(masterData.Yarns);

                initTblYarn(masterData.Yarns);
                if (status == statusConstants.ALL) {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnRevise").fadeOut();
                    $formEl.find("#btnReviseAndComplete").fadeOut();
                } else if (status == statusConstants.ACTIVE) {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnRevise").fadeOut();
                    $formEl.find("#btnReviseAndComplete").fadeIn();
                }
                else if (isBulkPage && status == statusConstants.IN_ACTIVE) {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnRevise").fadeOut();
                    $formEl.find("#btnReviseAndComplete").fadeOut();
                }
                else {
                    $formEl.find("#btnSave").fadeIn();
                    $formEl.find("#btnRevise").fadeOut();
                    $formEl.find("#btnReviseAndComplete").fadeOut();
                }
                initTblFabricChild();

                $formEl.find("#tblProgramInformation").bootstrapTable("load", masterData.Childs);
                $formEl.find("#tblProgramInformation").bootstrapTable('hideLoading');

                if (masterData.SubGroupName === subGroupNames.FABRIC) {
                    $formEl.find("#divGSM,#divComposition").fadeIn();
                    $formEl.find("#divLength,#divWidth").fadeOut();
                } else {
                    $formEl.find("#divGSM,#divComposition").fadeOut();
                    $formEl.find("#divLength,#divWidth").fadeIn();
                }

                if (isBDS || isBulkPage) {
                    $formEl.find(".bds").fadeIn();
                } else {
                    $formEl.find(".bds").fadeOut();
                }

                //IsSubContact
                if (masterData.IsSubContact == 1) {
                    $formEl.find("#IsSubContact").prop("checked", true);
                } else {
                    $formEl.find("#IsSubContact").prop("checked", false);
                }
                if (masterData.NeedPreFinishingProcess) {
                    $formEl.find("#need-yes").prop("checked", true);
                    $formEl.find("#need-no").prop("checked", false);
                }
                else {
                    $formEl.find("#need-yes").prop("checked", false);
                    $formEl.find("#need-no").prop("checked", true);
                }
                initAttachment(masterData.FilePath, masterData.AttachmentPreviewTemplate, $formEl.find("#UploadFile"));
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function initNewAttachment($el) {
        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            previewFileType: 'any'
        });
    }

    function initAttachment(path, type, $el) {
        if (!path) {
            initNewAttachment($el);
            return;
        }

        if (!type) type = "any";

        var preveiwData = [rootPath + path];
        var previewConfig = [{ type: type, caption: "PI Attachment", key: 1, width: "80px", frameClass: "preview-frame" }];

        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            initialPreview: preveiwData,
            initialPreviewAsData: true,
            initialPreviewFileType: 'image',
            initialPreviewConfig: previewConfig,
            maxFileSize: 4096
        });
        setDownloadBtn(path);
    }
    function setDownloadBtn(filePath) {
        $formEl.find(".file-caption-main").find(".kv-fileinput-caption").hide();
        $formEl.find(".file-caption-main").find(".input-group-append").css("float", "left");
        $formEl.find(".file-caption-main").prepend(`<a class="btn btn-xs btn-primary" href="${filePath}" title="View attached file" target="_blank" style='height: 34px; width: 34px; float:left;'>
                                                        <i class="glyphicon glyphicon-download-alt" aria-hidden="true" style='margin-top: 8px;'></i>
                                                    </a>`);
    }
    function isValidChildForm(data) {
        var isValidItemInfo = false;
        if (!isBulkPage) {
            $.each(data, function (j, obj) {
                $.each(obj.KJobCardMasters, function (j, objChild) {
                    if (objChild.IsSubContact == false || objChild.IsSubContact == undefined) {
                        if (objChild.KnittingMachineID == undefined || objChild.KnittingMachineID == 0) {
                            toastr.error("Machine is required.");
                            isValidItemInfo = true;
                        }
                    }
                });
            });
        }
        return isValidItemInfo;
    }

    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }

    function needPreFinishingProcess() {
        var preProcessYes = $formEl.find("#need-yes")[0];
        var preProcessNo = $formEl.find("#need-no")[0];
        if (preProcessYes.checked) {
            masterData.NeedPreFinishingProcess = true;
        } else if (preProcessNo.checked) {
            masterData.NeedPreFinishingProcess = false;
        }
    }

    function save(e, isApprove) {
        var isRevise = false;
        var hasError = false;
        var kJobCardMaster = [];

        needPreFinishingProcess();

        if (_yarnPrevInfoList.length === masterData.Yarns.length) {
            isRevise = _yarnPrevInfoList.some((prevInfo, i) =>
                prevInfo.PhysicalCount !== masterData.Yarns[i].PhysicalCount ||
                prevInfo.YarnLotNo !== masterData.Yarns[i].YarnLotNo ||
                prevInfo.BatchNo !== masterData.Yarns[i].BatchNo ||
                prevInfo.YarnBrandID !== masterData.Yarns[i].YarnBrandID
            );
        }

        for (var i = 0; i < masterData.Childs.length; i++) {
            var maxQty = parseFloat(masterData.Childs[i].MaxQty) + 5;
            masterData.Childs[i].BookingQty = parseFloat(masterData.Childs[i].BookingQty);

            if (!masterData.Childs[i].KnittingMachineID) {
                toastr.warning('Select machine!');
                masterData.Childs[i]["KJobCardMasters"] = [];
                return false;
            }

            if (!isBulkPage) {
                if (parseFloat(masterData.Childs[i].BookingQty) > maxQty) {
                    toastr.error(`Given Qty ${masterData.Childs[i].BookingQty} cannot be greater then maximum qty ${maxQty} at Program Information Row ${i + 1}`);
                    return false;
                }
            }

            if (masterData.Childs[i].KJobCardMasters.length == 0) {
                kJobCardMaster = {
                    ConceptID: masterData.ConceptID,
                    KJobCardMasterID: masterData.Childs[i].KJobCardMasterID,
                    KJobCardNo: masterData.Childs[i].KJobCardNo,
                    KJobCardDate: masterData.Childs[i].StartDate,
                    ItemMasterID: masterData.Childs[i].ItemMasterID,
                    BrandID: masterData.Childs[i].BrandID,
                    IsSubContact: masterData.Childs[i].IsSubContact,
                    ContactID: masterData.Childs[i].ContactID,
                    MachineDia: masterData.Childs[i].MachineDia,
                    KnittingMachineID: masterData.Childs[i].KnittingMachineID,
                    KJobCardQty: masterData.Childs[i].BookingQty,
                    MachineKnittingTypeID: masterData.KnittingTypeID, //masterData.Childs[i].KnittingTypeID
                    MachineGauge: masterData.Childs[i].MachineGauge,
                    MCSubClassID: masterData.Childs[i].MCSubClassID,
                    UnitID: 28,
                    SubGroupID: masterData.SubGroupID,
                    BookingQty: masterData.Childs[i].BookingQty,
                    Remarks: masterData.Childs[i].Remarks,
                    BuyerID: masterData.BuyerID,
                    BuyerTeamID: masterData.BuyerTeamID,
                    ExportOrderID: masterData.ExportOrderID,
                    BookingID: masterData.BookingID
                }

                if (kJobCardMaster.BookingQty == 0) {
                    hasError = true;
                    toastr.error("Qty cannot be zero.");
                }
                //else if (kJobCardMaster.MachineKnittingTypeID == 0) {
                //    hasError = true;
                //    toastr.error("Select knitting type.");
                //}
                if (hasError) {
                    masterData.Childs[i]["KJobCardMasters"] = [];
                    return false;
                }

                masterData.Childs[i]["KJobCardMasters"].push(kJobCardMaster);
            }
            else {
                for (var j = 0; j < masterData.Childs[i].KJobCardMasters.length; j++) {
                    masterData.Childs[i].KJobCardMasters[j].ConceptID = masterData.ConceptID;
                    masterData.Childs[i].KJobCardMasters[j].KJobCardNo = isBulkPage ? "" : masterData.Childs[i].KJobCardNo;
                    masterData.Childs[i].KJobCardMasters[j].KJobCardDate = isBulkPage ? new Date() : masterData.Childs[i].StartDate;
                    masterData.Childs[i].KJobCardMasters[j].BrandID = masterData.Childs[i].BrandID;
                    masterData.Childs[i].KJobCardMasters[j].IsSubContact = masterData.Childs[i].IsSubContact;
                    masterData.Childs[i].KJobCardMasters[j].ContactID = masterData.Childs[i].ContactID;
                    masterData.Childs[i].KJobCardMasters[j].MachineDia = masterData.Childs[i].MachineDia;
                    masterData.Childs[i].KJobCardMasters[j].KnittingMachineID = isBulkPage ? 0 : masterData.Childs[i].KnittingMachineID;
                    masterData.Childs[i].KJobCardMasters[j].KJobCardQty = isBulkPage ? 0 : masterData.Childs[i].BookingQty;
                    masterData.Childs[i].KJobCardMasters[j].MachineKnittingTypeID = masterData.KnittingTypeID;
                    masterData.Childs[i].KJobCardMasters[j].MachineGauge = masterData.Childs[i].MachineGauge;
                    masterData.Childs[i].KJobCardMasters[j].MCSubClassID = masterData.Childs[i].MCSubClassID;
                    masterData.Childs[i].KJobCardMasters[j].UnitID = 28;
                    masterData.Childs[i].KJobCardMasters[j].SubGroupID = masterData.SubGroupID;
                    masterData.Childs[i].KJobCardMasters[j].BookingQty = masterData.Childs[i].BookingQty;
                    masterData.Childs[i].KJobCardMasters[j].Remarks = masterData.Childs[i].Remarks;
                }
            }
        }

        var tempArray = masterData.Childs;
        for (var i = 0; i < tempArray.length; i++) {
            var filterData = $.grep(masterData.Childs, function (child) {
                return child.KPChildID != tempArray[i].KPChildID && child.MCSubClassID == tempArray[i].MCSubClassID && child.MachineDia == tempArray[i].MachineDia &&
                    child.MachineGauge == tempArray[i].MachineGauge && child.ContactID == tempArray[i].ContactID && child.BrandID == tempArray[i].BrandID &&
                    child.KnittingMachineID == tempArray[i].KnittingMachineID
            });
            if (filterData.length > 0) {
                toastr.warning('Same row exists in Program Information!!!');
                masterData.Childs[i]["KJobCardMasters"] = [];
                return false;
            }
        }

        var tempYarnArray = masterData.Yarns;
        var hasStockTypeError = false;
        for (var i = 0; i < tempYarnArray.length; i++) {
            var rowStr = `at row ${i + 1}`;
            if (is100Elastane(tempYarnArray[i].Composition)) {
                tempYarnArray[i].StitchLength = 0;
            }

            //if (tempYarnArray[i].YDItem && !tempYarnArray[i].BatchNo && !isBulkPage) {
            //    toastr.warning('Please enter Batch No where YDItem is true in Yarn Information!');
            //    masterData.Childs[i]["KJobCardMasters"] = [];
            //    return false;
            //}
            if (!tempYarnArray[i].PhysicalCount && !isBulkPage) {
                toastr.warning(`Please enter Physical Count in Yarn Information ${rowStr}!`);
                masterData.Childs[i]["KJobCardMasters"] = [];
                return false;
            }
            if (!tempYarnArray[i].YarnLotNo && !isBulkPage) {
                toastr.warning(`Please enter Yarn Lot No in Yarn Information ${rowStr}!`);
                masterData.Childs[i]["KJobCardMasters"] = [];
                return false;
            }


            tempYarnArray[i].YarnBrandID = getDefaultValueWhenInvalidN(tempYarnArray[i].YarnBrandID);
            if (!tempYarnArray[i].YarnBrandID && !isBulkPage) {
                toastr.warning(`Please enter Spinner in Yarn Information ${rowStr}!`);
                masterData.Childs[i]["KJobCardMasters"] = [];
                return false;
            }
            if (masterData.SubGroupID == 1 && tempYarnArray[i].StitchLength <= 0 && !is100Elastane(tempYarnArray[i].Composition)) {
                toastr.warning(`Please enter stitch Length in yarn Information ${rowStr}!`);
                masterData.Childs[i]["KJobCardMasters"] = [];
                return false;
            }

            if (!isBulkPage) {
                if (!isValueTrue(tempYarnArray[i].YDItem) && !isValueTrue(tempYarnArray[i].IsStockItem)) {
                    toastr.warning(`Please select item from stock where both YD Item and is stock item is "No" ${rowStr}`);
                    return false;
                }

                //if (tempYarnArray[i].IsStockItem) {
                if (!canChangeItemInfo(tempYarnArray[i])) {
                    if (tempYarnArray[i].YarnStockSet == null) {
                        toastr.warning(`Yarn Stock Set information missing ${rowStr}.`);
                        return false;
                    }
                    if (tempYarnArray[i].YarnStockSet.PhysicalCount != tempYarnArray[i].PhysicalCount) {
                        toastr.warning(`Physical Count Cannot be changed of stock item ${rowStr}`);
                        masterData.Yarns[i].PhysicalCount = tempYarnArray[i].YarnStockSet.PhysicalCount;
                        hasStockTypeError = true;
                    }
                    if (tempYarnArray[i].YarnStockSet.YarnLotNo != tempYarnArray[i].YarnLotNo) {
                        toastr.warning(`Yarn Lot No Cannot be changed of stock item ${rowStr}`);
                        masterData.Yarns[i].YarnLotNo = tempYarnArray[i].YarnStockSet.YarnLotNo;
                        hasStockTypeError = true;
                    }

                    if (tempYarnArray[i].YarnStockSet.SpinnerId != tempYarnArray[i].YarnBrandID) {
                        toastr.warning(`Spinner Cannot be changed of stock item ${rowStr}`);
                        masterData.Yarns[i].YarnBrandID = tempYarnArray[i].YarnStockSet.SpinnerId;
                        masterData.Yarns[i].Spinner = tempYarnArray[i].YarnStockSet.SpinnerName;
                        hasStockTypeError = true;
                    }
                }
            }
        }
        if (hasStockTypeError) {
            initTblYarn(masterData.Yarns);
            return false;
        }

        //var sumBookingQty = 0;
        //if (parseInt(sumBookingQty) > (parseInt(masterData.RemainingQty) + parseInt(masterData.PlanQty))) {
        //    toastr.warning("Fabric quantity is not more than remaining quantity!!!");
        //    return false;
        //} 

        var formData = getFormData($formEl);
        formData.append("SubGroupCount", masterData.Childs.length);
        formData.append("Approve", isApprove);
        formData.append("IsRevise", isRevise);
        formData.append("PlanQty", masterData.Childs[0].BookingQty);

        formData.append("Childs", JSON.stringify(masterData.Childs));

        masterData.Yarns.map(y => {
            if (is100Elastane(y.Composition)) {
                y.StitchLength = 0;
            }
            if (isBulkPage) y.IsStockItem = false;
            else y.IsStockItem = getIsStockItem(y.IsStockItem);
        });

        formData.append("Yarns", JSON.stringify(masterData.Yarns));

        var files = $formEl.find("#UploadFile")[0].files;
        formData.append("UploadFile", files[0]);

        //Machine Check 
        if ($formEl.find("#IsSubContact").is(':checked')) {

        } else {
            if (isValidChildForm(masterData.Childs)) return;
        }

        //IsSubContact save 
        if ($formEl.find("#IsSubContact").is(':checked')) {
            masterData.IsSubContact = 1;
            formData.append("SubGroupCount", masterData.IsSubContact);
        }
        else {
            masterData.IsSubContact = 0;
            formData.append("SubGroupCount", masterData.IsSubContact);
        }

        const config = {
            headers: {
                'content-type': 'multipart/form-data',
                'Authorization': "Bearer " + localStorage.getItem("token")
            }
        }
        //var abc = Object.fromEntries(formData);

        axios.post("/api/knitting-program/save", formData, config)
            .then(function (response) {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(showResponseError);
    }

    function revise(e) {

        var hasError = false;
        var kJobCardMaster = [];
        needPreFinishingProcess();
        for (var i = 0; i < masterData.Childs.length; i++) {
            var maxQty = parseFloat(masterData.Childs[i].MaxQty) + 5;
            masterData.Childs[i].BookingQty = parseFloat(masterData.Childs[i].BookingQty);
            if (!isBulkPage) {
                if (parseFloat(masterData.Childs[i].BookingQty) > maxQty) {
                    toastr.error(`Given Qty ${masterData.Childs[i].BookingQty} cannot be greater then maximum qty ${maxQty} at Program Information Row ${i + 1}`);
                    return false;
                }
            }
            if (masterData.Childs[i].KJobCardMasters.length == 0) {
                kJobCardMaster = {
                    ConceptID: masterData.ConceptID,
                    KJobCardMasterID: masterData.Childs[i].KJobCardMasterID,
                    KJobCardNo: masterData.Childs[i].KJobCardNo,
                    KJobCardDate: masterData.Childs[i].StartDate,
                    BrandID: masterData.Childs[i].BrandID,
                    IsSubContact: masterData.Childs[i].IsSubContact,
                    ItemMasterID: masterData.Childs[i].ItemMasterID,
                    ContactID: masterData.Childs[i].ContactID,
                    MachineDia: masterData.Childs[i].MachineDia,
                    KnittingMachineID: masterData.Childs[i].KnittingMachineID,
                    KJobCardQty: masterData.Childs[i].BookingQty,
                    MachineKnittingTypeID: masterData.KnittingTypeID, //masterData.Childs[i].KnittingTypeID
                    MachineGauge: masterData.Childs[i].MachineGauge,
                    MCSubClassID: masterData.Childs[i].MCSubClassID,
                    UnitID: 28,
                    SubGroupID: masterData.SubGroupID,
                    BookingQty: masterData.Childs[i].BookingQty,
                    Remarks: masterData.Childs[i].Remarks,
                    BuyerID: masterData.BuyerID,
                    BuyerTeamID: masterData.BuyerTeamID,
                    ExportOrderID: masterData.ExportOrderID,
                    BookingID: masterData.BookingID
                }

                if (kJobCardMaster.BookingQty == 0) {
                    hasError = true;
                    toastr.error("Qty cannot be zero.");
                }
                //else if (kJobCardMaster.MachineKnittingTypeID == 0) {
                //    hasError = true;
                //    toastr.error("Select knitting type.");
                //}
                if (hasError) {
                    masterData.Childs[i]["KJobCardMasters"] = [];
                    return false;
                }
                masterData.Childs[i]["KJobCardMasters"].push(kJobCardMaster);
            }
            else {
                for (var j = 0; j < masterData.Childs[i].KJobCardMasters.length; j++) {
                    masterData.Childs[i].KJobCardMasters[j].ConceptID = masterData.ConceptID;
                    masterData.Childs[i].KJobCardMasters[j].KJobCardNo = isBulkPage ? "" : masterData.Childs[i].KJobCardNo;
                    masterData.Childs[i].KJobCardMasters[j].KJobCardDate = isBulkPage ? new Date() : masterData.Childs[i].StartDate;
                    masterData.Childs[i].KJobCardMasters[j].BrandID = masterData.Childs[i].BrandID;
                    masterData.Childs[i].KJobCardMasters[j].IsSubContact = masterData.Childs[i].IsSubContact;
                    masterData.Childs[i].KJobCardMasters[j].ContactID = masterData.Childs[i].ContactID;
                    masterData.Childs[i].KJobCardMasters[j].MachineDia = masterData.Childs[i].MachineDia;
                    masterData.Childs[i].KJobCardMasters[j].KnittingMachineID = isBulkPage ? 0 : masterData.Childs[i].KnittingMachineID;
                    masterData.Childs[i].KJobCardMasters[j].KJobCardQty = isBulkPage ? 0 : masterData.Childs[i].BookingQty;
                    masterData.Childs[i].KJobCardMasters[j].MachineKnittingTypeID = masterData.KnittingTypeID;
                    masterData.Childs[i].KJobCardMasters[j].MachineGauge = masterData.Childs[i].MachineGauge;
                    masterData.Childs[i].KJobCardMasters[j].MCSubClassID = masterData.Childs[i].MCSubClassID;
                    masterData.Childs[i].KJobCardMasters[j].UnitID = 28;
                    masterData.Childs[i].KJobCardMasters[j].SubGroupID = masterData.SubGroupID;
                    masterData.Childs[i].KJobCardMasters[j].BookingQty = masterData.Childs[i].BookingQty;
                    masterData.Childs[i].KJobCardMasters[j].Remarks = masterData.Childs[i].Remarks;
                }
            }
        }

        if (hasError) return false;

        var tempArray = masterData.Childs;
        for (var i = 0; i < tempArray.length; i++) {
            var filterData = $.grep(masterData.Childs, function (child) {
                return child.KPChildID != tempArray[i].KPChildID && child.MCSubClassID == tempArray[i].MCSubClassID && child.MachineDia == tempArray[i].MachineDia &&
                    child.MachineGauge == tempArray[i].MachineGauge && child.ContactID == tempArray[i].ContactID && child.BrandID == tempArray[i].BrandID &&
                    child.KnittingMachineID == tempArray[i].KnittingMachineID
            });
            if (filterData.length > 0) {
                toastr.warning('Same row exists in Program Information!!!');
                return false;
            }
        }

        var tempYarnArray = masterData.Yarns;
        var hasStockTypeError = false;
        for (var i = 0; i < tempYarnArray.length; i++) {
            var rowStr = `at row ${i + 1}`;
            if (tempYarnArray[i].YDItem && !tempYarnArray[i].BatchNo) {
                toastr.warning(`Please enter Batch No where YDItem is true in Yarn Information ${rowStr}!`);
                masterData.Childs[i]["KJobCardMasters"] = [];
                return false;
            }
            if (!tempYarnArray[i].PhysicalCount && !isBulkPage) {
                toastr.warning(`Please enter Physical Count in Yarn Information ${rowStr}!`);
                masterData.Childs[i]["KJobCardMasters"] = [];
                return false;
            }
            if (!tempYarnArray[i].YarnLotNo && !isBulkPage) {
                toastr.warning(`Please enter Yarn Lot No in Yarn Information ${rowStr}!`);
                masterData.Childs[i]["KJobCardMasters"] = [];
                return false;
            }

            tempYarnArray[i].YarnBrandID = getDefaultValueWhenInvalidN(tempYarnArray[i].YarnBrandID);
            if (!tempYarnArray[i].YarnBrandID && !isBulkPage) {
                toastr.warning(`Please enter Spinner in Yarn Information ${rowStr}!`);
                masterData.Childs[i]["KJobCardMasters"] = [];
                return false;
            }
            if (masterData.SubGroupID == 1 && tempYarnArray[i].StitchLength <= 0 && !is100Elastane(tempYarnArray[i].Composition)) {
                toastr.warning(`Please enter stitch Length in yarn Information ${rowStr}!`);
                masterData.Childs[i]["KJobCardMasters"] = [];
                return false;
            }

            if (!isBulkPage) {
                if (!isValueTrue(tempYarnArray[i].YDItem) && !isValueTrue(tempYarnArray[i].IsStockItem)) {
                    toastr.warning(`Please select item from stock where both YD Item and is stock item is "No" ${rowStr}`);
                    return false;
                }

                //if (tempYarnArray[i].IsStockItem) {
                if (!canChangeItemInfo(tempYarnArray[i])) {
                    if (tempYarnArray[i].YarnStockSet == null) {
                        toastr.warning('Yarn Stock Set information missing ${rowStr}.');
                        return false;
                    }
                    if (tempYarnArray[i].YarnStockSet.PhysicalCount != tempYarnArray[i].PhysicalCount) {
                        toastr.warning(`Physical Count Cannot be changed of stock item ${rowStr}`);
                        masterData.Yarns[i].PhysicalCount = tempYarnArray[i].YarnStockSet.PhysicalCount;
                        hasStockTypeError = true;
                    }
                    if (tempYarnArray[i].YarnStockSet.YarnLotNo != tempYarnArray[i].YarnLotNo) {
                        toastr.warning(`Yarn Lot No Cannot be changed of stock item ${rowStr}`);
                        masterData.Yarns[i].YarnLotNo = tempYarnArray[i].YarnStockSet.YarnLotNo;
                        hasStockTypeError = true;
                    }

                    if (tempYarnArray[i].YarnStockSet.SpinnerId != tempYarnArray[i].YarnBrandID) {
                        toastr.warning(`Spinner Cannot be changed of stock item ${rowStr}`);
                        masterData.Yarns[i].YarnBrandID = tempYarnArray[i].YarnStockSet.SpinnerId;
                        masterData.Yarns[i].Spinner = tempYarnArray[i].YarnStockSet.SpinnerName;
                        hasStockTypeError = true;
                    }
                }
            }
        }
        if (hasStockTypeError) {
            initTblYarn(masterData.Yarns);
            return false;
        }
        //var sumBookingQty = 0;
        //if (parseInt(sumBookingQty) > (parseInt(masterData.RemainingQty) + parseInt(masterData.PlanQty))) {
        //    toastr.warning("Fabric quantity is not more than remaining quantity!!!");
        //    return false;
        //}  

        var formData = getFormData($formEl);
        var dt = JSON.stringify(formData);
        formData.append("SubGroupCount", masterData.Childs.length);
        formData.append("PlanQty", masterData.Childs[0].BookingQty);

        formData.append("Childs", JSON.stringify(masterData.Childs));

        masterData.Yarns.map(y => {
            if (is100Elastane(y.Composition)) {
                y.StitchLength = 0;
            }
            if (isBulkPage) y.IsStockItem = false;
            else y.IsStockItem = getIsStockItem(y.IsStockItem);
        });
        formData.append("Yarns", JSON.stringify(masterData.Yarns));

        var files = $formEl.find("#UploadFile")[0].files;
        formData.append("UploadFile", files[0]);


        //Machine Check 
        if ($formEl.find("#IsSubContact").is(':checked')) {

        } else {
            if (isValidChildForm(masterData.Childs)) return;
        }

        //IsSubContact save 
        if ($formEl.find("#IsSubContact").is(':checked')) {
            masterData.IsSubContact = 1;
            formData.append("SubGroupCount", masterData.IsSubContact);
        }
        else {
            masterData.IsSubContact = 0;
            formData.append("SubGroupCount", masterData.IsSubContact);
        }
        const config = {
            headers: {
                'content-type': 'multipart/form-data',
                'Authorization': "Bearer " + localStorage.getItem("token")
            }
        }

        axios.post("/api/knitting-program/revise", formData, config)
            .then(function (response) {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(showResponseError);
    }

    function canChangeItemInfo(yarn) {

        if (!yarn.YDItem && !yarn.IsStockItem) return false;
        else if (!yarn.YDItem && yarn.IsStockItem) return false;
        else if (yarn.YDItem && yarn.IsStockItem) return true;
        else if (yarn.YDItem && !yarn.IsStockItem) return true;


        else if (yarn.YDItem == "No" && yarn.IsStockItem == "No") return false;
        else if (yarn.YDItem == "No" && yarn.IsStockItem == "Yes") return false;
        else if (yarn.YDItem == "Yes" && yarn.IsStockItem == "Yes") return true;
        else if (yarn.YDItem == "Yes" && yarn.IsStockItem == "No") return true;

        return true;
    }
    function getIsStockItem(isStockItem) {
        if (typeof isStockItem == "undefined" || isStockItem == null) return false;
        else if (isStockItem == "Yes") return true;
        else if (isStockItem == "No") return false;
        return false;
    }
    function isValueTrue(propValue) {
        if (propValue || propValue == "Yes") return true;
        return false;
    }
})();
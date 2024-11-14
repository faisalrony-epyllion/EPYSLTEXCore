(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, $tblYarnEl, tblMasterId;
    var status;
    var masterData;
    var subGroupID = 1;
    var isBulkPage = false;
    var isAdditional = false;

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $tblYarnEl = $("#tblKnittingPlanYarn" + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        status = statusConstants.PENDING_GROUP;
        initMasterTable();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING_GROUP;
            $formEl.find("#btnSelectMachine").html('Select Machine');
            $formEl.find("#btnRevise").fadeOut();
            $formEl.find("#btnSave").fadeIn();
            $toolbarEl.find("#btnAddKP").fadeIn();
            initMasterTable();
        });

        $toolbarEl.find("#btnActiveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACTIVE;
            $formEl.find("#btnSelectMachine").html('Select Machine');
            $formEl.find("#btnRevise").fadeOut();
            $formEl.find("#btnSave").fadeIn();
            $toolbarEl.find("#btnAddKP").fadeOut();
            initMasterTable();
        });

        $toolbarEl.find("#btnInActiveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.IN_ACTIVE;
            $formEl.find("#btnSelectMachine").html('Select Machine');
            $formEl.find("#btnRevise").fadeOut();
            $formEl.find("#btnSave").fadeIn();
            $toolbarEl.find("#btnAddKP").fadeOut();
            initMasterTable();
        });

        $toolbarEl.find("#btnRevisionList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REVISE;
            $formEl.find("#btnSelectMachine").html('Select Machine');
            $formEl.find("#btnRevise").fadeOut();
            $formEl.find("#btnSave").hide();
            $toolbarEl.find("#btnAddKP").fadeOut();
            initMasterTable();
        });

        $toolbarEl.find("#btnAllList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ALL;
            $formEl.find("#btnSelectMachine").html('Select Machine');
            $formEl.find("#btnRevise").hide();
            $formEl.find("#btnSave").hide();
            $toolbarEl.find("#btnAddKP").fadeOut();
            initMasterTable();
        });

        $toolbarEl.find("#btnAddKP").click(function () {
            isContactEventCall();
            getNewDataForKP();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnRevise").click(function (e) {
            e.preventDefault();
            revise(this);
        });

        $formEl.find("#btnSaveAndAapprove").click(function (e) {
            e.preventDefault();
            save(this, true);
        });

        $formEl.find("#IsSubContact").click(function (e) {
            isContactEventCall();
        });

        $formEl.find("#btnSelectMachine").on("click", function (e) {
            e.preventDefault();
            var MachineSubContractList = [];
            if ($formEl.find("#IsSubContact").is(':checked')) {
                MachineSubContractList = masterData.KnittingSubContracts;
            }
            else {
                if (subGroupID == 1) {
                    MachineSubContractList = masterData.KnittingMachines.filter(x => x.MachineSubClassID == masterData.MCSubClassID);

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
                //data: (subGroupID == 1)
                //    ? masterData.KnittingMachines.filter(function (e) {
                //        return e.MachineSubClassID == masterData.MCSubClassID;
                //    })
                //    : masterData.KnittingMachines.filter(function (e) {
                //        return e.MachineSubClassID == masterData.MCSubClassID && e.GG == masterData.MachineGauge;
                //    }),
                isMultiselect: false,
                modalSize: "modal-md",
                top: "2px",
                primaryKeyColumn: "KnittingMachineID",
                fields: "GG,Dia,Brand,Contact,MachineNo,Capacity,IsSubContact",
                headerTexts: "GG,Dia,Brand,Unit,Machine No,Capacity,Sub Contact ?",
                widths: "30,30,100,70,50,50,50",
                onSelect: function (res) {
                    finder.hideModal();
                    $formEl.find("#MachineGauge").val(res.rowData.GG);
                    $formEl.find("#MachineDia").val(res.rowData.Dia);

                    //var unit = $formEl.find("#Needle").val();
                    //var rows = $formEl.find("#tblProgramInformation").bootstrapTable('getData');
                    //if (rows) {
                    //    for (var i = 0; i < rows.length; i++) {
                    //        rows[i].Uom = needle;
                    //    }
                    //    $formEl.find("#tblProgramInformation").bootstrapTable("load", rows);
                    //    $formEl.find("#tblProgramInformation").bootstrapTable('hideLoading');
                    //}
                },
            });
            finder.showModal();
        });

        $formEl.find("#btnCopyProgram").on("click", function (e) {
            e.preventDefault();
            //var pType = knittingProgramType.BDS;
            var pType = knittingProgramType.BULK;
            if (masterData.IsBDS == 1) pType = knittingProgramType.BDS;
            else if (masterData.IsBDS == 0) pType = knittingProgramType.CONCEPT;

            var finder = new commonFinder({
                title: "Select Knitting Program",
                pageId: pageId,
                //data: MachineSubContractList,
                apiEndPoint: `/api/knitting-program/list-by-mcsubclass?type=${pType}&mcSubClassId=${masterData.MCSubClassID}`,
                isMultiselect: false,
                modalSize: "modal-md",
                top: "2px",
                primaryKeyColumn: "KPMasterID",
                fields: "ConceptNo,TechnicalName,GSM,ColorName,Composition",
                headerTexts: "Concept No,Technical Name,GSM,Color,Composition",
                widths: "50,50,30,40,50",
                onSelect: function (res) {
                    finder.hideModal();
                    axios.get(`/api/knitting-program/${pType}/${res.rowData.KPMasterID}/${res.rowData.SubGroupName}`)
                        .then(function (response) {
                            if (response.data.Childs.length > 0) {
                                var index = -1;
                                masterData.KnittingPlans.forEach(function (row) {

                                    index++;
                                    if (index == 0) {
                                        $formEl.find("#Needle").val(response.data.Childs[0].Needle);
                                        $formEl.find("#CPI").val(response.data.Childs[0].CPI);
                                        $formEl.find("#MachineGauge").val(response.data.Childs[0].MachineGauge);
                                        $formEl.find("#MachineDia").val(response.data.Childs[0].MachineDia);
                                        $formEl.find("#KnittingTypeID").val(response.data.Childs[0].KnittingTypeID).trigger("change");

                                        $formEl.find("#BAnalysisID").val(response.data.Childs[0].BAnalysisID);
                                        $formEl.find("#BuyerID").val(response.data.Childs[0].BuyerID);
                                        $formEl.find("#BuyerTeamID").val(response.data.Childs[0].BuyerTeamID);
                                        $formEl.find("#ExportOrderID").val(response.data.Childs[0].ExportOrderID);
                                        $formEl.find("#BrandID").val(response.data.Childs[0].BrandID);
                                        $formEl.find("#SeasonID").val(response.data.Childs[0].SeasonID);
                                        $formEl.find("#CompanyID").val(response.data.Childs[0].CompanyID);
                                        $formEl.find("#MerchandiserTeamID").val(response.data.Childs[0].MerchandiserTeamID);
                                    }

                                    row.KnittingMachineID = response.data.Childs[0].KnittingMachineID;
                                    row.KnittingMachineNo = response.data.Childs[0].KnittingMachineNo;
                                    row.MachineGauge = response.data.Childs[0].MachineGauge;
                                    row.MachineDia = response.data.Childs[0].MachineDia;
                                    row.BrandID = response.data.Childs[0].BrandID;
                                    row.Brand = response.data.Childs[0].Brand;
                                    row.KnittingTypeID = response.data.Childs[0].KnittingTypeID;

                                    masterData.MachineGauge = response.data.Childs[0].MachineGauge;
                                    masterData.MachineDia = response.data.Childs[0].MachineDia;
                                    masterData.MCSubClassID = response.data.Childs[0].MCSubClassID;
                                    masterData.BrandID = response.data.Childs[0].BrandID;
                                    masterData.KnittingTypeID = response.data.Childs[0].KnittingTypeID;

                                    row.Composition = response.data.Composition;
                                    row.GSM = response.data.GSM;
                                    row.ColorName = response.data.ColorName;
                                    //row.Size = response.data.Childs[0].Size;
                                    row.FUPartName = response.data.Childs[0].FUPartName;
                                    row.TotalNeedle = response.data.Childs[0].TotalNeedle;
                                    row.TotalCourse = response.data.Childs[0].TotalCourse;
                                    row.Uom = response.data.Childs[0].Uom;
                                    //row.BookingQty = response.data.Childs[0].BookingQty;
                                    row.Remarks = response.data.Childs[0].Remarks;

                                    /*
                                    masterData.Childs[0].KnittingMachineID = response.data.Childs[0].KnittingMachineID;
                                    masterData.Childs[0].MachineGauge = response.data.Childs[0].MachineGauge;
                                    masterData.Childs[0].MachineDia = response.data.Childs[0].MachineDia;
                                    masterData.Childs[0].ContactID = response.data.Childs[0].ContactID;
                                    masterData.Childs[0].Contact = response.data.Childs[0].Contact;
                                    masterData.Childs[0].BrandID = response.data.Childs[0].BrandID;
                                    masterData.Childs[0].Brand = response.data.Childs[0].Brand;
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
                                    */

                                });
                                $formEl.find("#tblProgramInformation").bootstrapTable('load', masterData.KnittingPlans);
                                $tblChildEl.bootstrapTable("load", masterData.KnittingPlans);

                                calculateNeedle();
                                calculateCPI();
                                //$formEl.find("#tblProgramInformation").bootstrapTable('updateRow', { index: 0, row: masterData.Childs[0] });
                            }
                            if (response.data.Yarns.length > 0) {
                                masterData.Yarns.forEach(function (row) {
                                    var yarn = response.data.Yarns.find(function (el) { return el.YarnCount == row.YarnCount && el.YarnType == row.YarnType });
                                    if (yarn) {
                                        row.PhysicalCount = yarn.PhysicalCount;
                                        row.YarnLotNo = yarn.YarnLotNo;
                                        row.YarnBrandID = yarn.YarnBrandID;
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

        $formEl.find("#btnMachineType").on("click", function (e) {
            e.preventDefault();
            var MachineTypeList = masterData.MachineTypeList;

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

                    masterData.Childs[0].MachineGauge = 0;
                    masterData.Childs[0].MachineDia = 0;
                    masterData.Childs[0].BrandID = 0;
                    masterData.Childs[0].Brand = "Empty";
                    masterData.Childs[0].KnittingMachineID = 0;
                    masterData.Childs[0].KnittingMachineNo = "Empty";
                    $formEl.find("#tblProgramInformation").bootstrapTable('load', getChilds(masterData));
                },
            });
            finder.showModal();
        });

        isContactEventCall();

        /*
        $formEl.find("#ActualNeedle").keyup(function (e) {
            e.preventDefault();
            var ActualNeedle = $formEl.find("#ActualNeedle").val();
            var rows = $formEl.find("#tblProgramInformation").bootstrapTable('getData');
            for (var i = 0; i < rows.length; i++) {
                    row[i].Needle = ActualNeedle;
                    row[i].TotalNeedle = Math.ceil((row[i].Length + (row[i].Length * (6 / 100))) * row.Needle);
            }
            $formEl.find("#tblProgramInformation").bootstrapTable("load", rows);
            $formEl.find("#tblProgramInformation").bootstrapTable('hideLoading');

        });
        $formEl.find("#ActualCPI").keyup(function (e) {
            e.preventDefault();
            var ActualCPI = $formEl.find("#ActualCPI").val();
            var rows = $formEl.find("#tblProgramInformation").bootstrapTable('getData');
            for (var i = 0; i < rows.length; i++) {
                    row[i].CPI = ActualCPI;
                    row[i].TotalCourse = Math.ceil((row[i].Width + (row[i].Width * (6 / 100))) * (row[i].CPI / 2.54));
            }
            $formEl.find("#tblProgramInformation").bootstrapTable("load", rows);
            $formEl.find("#tblProgramInformation").bootstrapTable('hideLoading');
        });
        */

        $formEl.find("#Needle").keyup(function (e) {
            e.preventDefault();
            calculateNeedle();
        });
        $formEl.find("#CPI").keyup(function (e) {
            e.preventDefault();
            calculateCPI();
        });
    });

    function calculateNeedle() {
        var needle = $formEl.find("#Needle").val();
        var rows = $formEl.find("#tblProgramInformation").bootstrapTable('getData');
        if (rows) {
            for (var i = 0; i < rows.length; i++) {
                rows[i].Needle = needle;
                rows[i].TotalNeedle = Math.ceil((rows[i].Length + (rows[i].Length * (6 / 100))) * rows[i].Needle);
            }
            $formEl.find("#tblProgramInformation").bootstrapTable("load", rows);
            $formEl.find("#tblProgramInformation").bootstrapTable('hideLoading');
        }
    }

    function calculateCPI() {
        var cpi = $formEl.find("#CPI").val();
        var rows = $formEl.find("#tblProgramInformation").bootstrapTable('getData');
        if (rows) {
            for (var i = 0; i < rows.length; i++) {
                rows[i].CPI = cpi;
                rows[i].TotalCourse = Math.ceil((rows[i].Width + (rows[i].Width * (6 / 100))) * parseFloat(rows[i].CPI / 2.54));
            }
            $formEl.find("#tblProgramInformation").bootstrapTable("load", rows);
            $formEl.find("#tblProgramInformation").bootstrapTable('hideLoading');
        }
    }

    function isContactEventCall() {
        $formEl.find("#MachineGauge").val(0);
        $formEl.find("#MachineDia").val(0);
        if ($formEl.find("#IsSubContact").is(':checked')) {
            $formEl.find("#IsSubContact").prop("checked", true);
            $formEl.find("#btnSelectMachine").html('Select SubContact');
        } else {
            $formEl.find("#IsSubContact").prop("checked", false);
            $formEl.find("#btnSelectMachine").html('Select Machine');
        }
    }

    function initMasterTable() {
        var commands = [],
            isVisible = true,
            width = 60;
        if (status == statusConstants.PENDING_GROUP) isVisible = false;
        if (status == statusConstants.ACTIVE) {
            width = 100;
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'Addition', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } },
                { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
            ]
        } else {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
            ]
        }
        var columns = [
            {
                headerText: 'Actions', width: width, textAlign: 'Center', commands: commands, visible: isVisible
            },
            {
                field: 'ReqDeliveryDate', headerText: 'Req. DeliveryDate', textAlign: 'Right', type: 'date', format: _ch_date_format_1, visible: status === statusConstants.COMPLETED,
            },
            {
                field: 'ConceptID', isPrimaryKey: true, headerText: 'Concept ID', width: 40, visible: false
            },
            {
                field: 'ConceptNo', headerText: 'Concept No', width: 100
            },
            {
                field: 'DateAdded', headerText: 'Program Date', textAlign: 'Right', type: 'date', format: _ch_date_format_1, visible: status === statusConstants.ACTIVE,width:80
            },
            {
                field: 'PlanNo', headerText: 'Plan No', width: 50, textAlign: 'Center', headerTextAlign: 'Center', visible: (status != statusConstants.PENDING_GROUP),
            },
            {
                field: 'TotalPlanedQty', headerText: 'Plan Qty', textAlign: 'Center', width: 50, visible: false
            },
            //{
            //    field: 'ConceptDate', headerText: 'Concept Date', textAlign: 'Right', type: 'date', format:_ch_date_format_1, width: 50, textAlign: 'Center', headerTextAlign: 'Center'
            //},
            {
                field: 'SubGroupName', headerText: 'Sub Group', width: 50, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'Buyer', headerText: 'Buyer', width: 50, headerTextAlign: 'Center'
            },
            {
                field: 'BuyerTeam', headerText: 'Buyer Team', width: 50, headerTextAlign: 'Center'
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
                field: 'Size', headerText: 'Size', width: 40
            },
            {
                field: 'Qty', headerText: 'QTY', width: 40, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'TotalQty', headerText: 'Plan Qty', width: 40, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'RemainingQty', headerText: 'Remaining Qty', width: 50, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'Active', headerText: 'Active?', visible: status == statusConstants.ALL, width: 25, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'UserName', headerText: 'Concept By', width: 60
            }
        ];

        var programType = knittingProgramType.BDS;

        var selectionType = "Single";
        if (status == statusConstants.PENDING_GROUP) {
            columns.unshift({ type: 'checkbox', width: 20 });
            selectionType = "Multiple";
        }

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/knitting-program/list/group?type=${programType}&status=${status}`,
            columns: columns,
            autofitColumns: false,
            commandClick: handleCommands,
            allowSelection: status == statusConstants.PENDING_GROUP,
            selectionSettings: { type: selectionType, checkboxOnly: true, persistSelection: true }
        });
    }

    function handleCommands(args) {
        $formEl.find("#btnCopyProgram").fadeOut();
        isAdditional = false;
        if (args.commandColumn.type == 'Report') {
            window.open(`/reports/InlinePdfView?ReportName=PlanWiseJobCardDetails.rdl&PlanNo=${args.rowData.PlanNo}`, '_blank');
        }
        else if (status === statusConstants.PENDING_GROUP) {
            $formEl.find("#btnCopyProgram").fadeIn();
            getNew(args.rowData.ConceptID, args.rowData.WithoutOB, args.rowData.SubGroupName);
        }
        else if (status === statusConstants.REVISE) {
            getRevisionedDetails(args.rowData.PlanNo, args.rowData.GroupConceptNo, args.rowData.SubGroupName);
        }
        else if (args.commandColumn.type == 'Addition') {
            isAdditional = true;
            getDetails(args.rowData.GroupConceptNo, args.rowData.PlanNo, args.rowData.IsBDS, args.rowData.SubGroupName);
            //getRevisionedDetails(args.rowData.PlanNo, args.rowData.GroupConceptNo, args.rowData.SubGroupName);
        }
        else {
            if (status == statusConstants.ACTIVE) {
                $formEl.find("#btnCopyProgram").fadeIn();
            }
            getDetails(args.rowData.GroupConceptNo, args.rowData.PlanNo, args.rowData.IsBDS, args.rowData.SubGroupName);
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
        if (subGroupID == 1) {
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
                        return [
                            '<span class="btn-group">',
                            '<a class="btn btn-success btn-xs add" href="javascript:void(0)" title="Add a row like this">',
                            '<i class="fa fa-plus"></i>',
                            '</a>',
                            '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Remove this row">',
                            '<i class="fa fa-remove"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .add': function (e, value, row, index) {
                            addNewYarnItem(row);
                        },
                        'click .remove': function (e, value, row, index) {
                            masterData.Yarns.splice(index, 1);
                            $tblYarnEl.bootstrapTable('load', masterData.Yarns);
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
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" style="padding-right: 10px;">'
                    }
                },
                {
                    field: "YarnLotNo",
                    title: "Yarn Lot",
                    filterControl: "input",
                    align: 'center',
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
                    filterControl: "input",
                    align: 'center',
                    visible: isDisplayStitchLength(records),
                    formatter: function (value, row, index, field) {
                        //records
                        if (masterData.SubGroupName.toLowerCase() === subGroupNames.FABRIC.toLowerCase()) {
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
                }
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
            YarnPly: 1,
            StitchLength: 0,
            BatchNo: row.BatchNo,
            PhysicalCount: row.PhysicalCount,
            YarnCategory: row.YarnCategory,
            //YD: row.YD
            YDItem: row.YDItem
        }
        masterData.Yarns.push(obj);
        $tblYarnEl.bootstrapTable('load', masterData.Yarns);
    }

    function getNewItem(obj) {
        return {
            KPChildID: getMaxIdForArray(obj.Childs, "KPChildID"),
            ConceptID: obj.ConceptID,
            SubGroupID: obj.SubGroupID,
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
            FUPartName: obj.FUPartName,
            Needle: 0,
            CPI: 0,
            TotalNeedle: 0,
            TotalCourse: 0,
            MachineDia: 0,
            MachineGauge: (obj.SubGroupName === subGroupNames.FABRIC) ? 0 : obj.MachineGauge,
            StartDate: new Date(),
            EndDate: new Date(),
            Uom: (obj.SubGroupName === subGroupNames.FABRIC) ? "Kg" : "Pcs",
            BookingQty: 0,
            KJobCardQty: 0,
            MCSubClassID: obj.MCSubClassID,
            MCSubClassName: "",
            KJobCardNo: '**<< NEW >>**',
            Remarks: " ",
            KJobCardMasters: [],
            Contact: "",
            Brand: "",
            KnittingMachineNo: "",
            Length: obj.Length,
            Width: obj.Width,
            IsSubContact: obj.IsSubContact
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
                        visible: isAdditional,
                        //formatter: function (value, row, index, field) {
                        //    if (subGroupID == 1) {
                        //        return [
                        //            '<span class="btn-group">',
                        //            `<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportName=KnittingJobCard.rdl&JobCardNo=${row.KJobCardNo}" target="_blank" title="Job Card Report For Fabric">
                        //        <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                        //        </a>`,
                        //            '</span>'
                        //        ].join('');
                        //    }
                        //    else {
                        //        return [
                        //            '<span class="btn-group">',
                        //            `<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportName=KnittingJobCardCC.rdl&JobCardNo=${row.KJobCardNo}" target="_blank" title="Job Card Report For Other Item">
                        //        <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                        //        </a>`,
                        //            '</span>'
                        //        ].join('');
                        //    }
                        //},
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
                                showBootboxConfirm("Delete Record.", "Are you sure want to delete this?", function (yes) {
                                    if (yes) {
                                        var indexF = masterData.KnittingPlans.findIndex(x => x.KPMasterID == row.KPMasterID);
                                        masterData.KnittingPlans.splice(indexF, 1);
                                        $formEl.find("#tblProgramInformation").bootstrapTable("load", masterData.KnittingPlans);
                                    }
                                });
                            }
                        }
                    },
                    {
                        field: "Composition",
                        title: "Composition",
                        width: 100,
                        visible: (subGroupID == 1) //1 = fabric
                    },
                    {
                        field: "GSM",
                        title: "GSM",
                        width: 100,
                        visible: (subGroupID == 1) //1 = fabric
                    },
                    {
                        field: "ColorName",
                        title: "Color",
                        width: 100
                    },
                    /*
                    {
                        field: "KnittingTypeID",
                        title: "Knitting Type",
                        visible: (subGroupID == 1), //1 = fabric
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
                    */
                    {
                        field: "Size",
                        title: "Size",
                        width: 100,
                        visible: (subGroupID != 1) //1 = fabric
                    },
                    {
                        field: "Contact", //ContactID
                        title: "Floor/Sub-Contractor",
                        width: 100,
                        visible: false
                    },
                    {
                        field: "FUPartName",
                        title: "End Use",
                        visible: (subGroupID != 1), //1 = fabric
                        width: 100
                    },
                    /*
                    {
                        field: "Needle",
                        title: "Needle/cm",
                        visible: (subGroupID != 1), //1 = fabric
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
                        visible: (subGroupID != 1), //1 = fabric
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
                    */
                    {
                        field: "TotalNeedle",
                        title: "Total Needle",
                        visible: (subGroupID != 1), //1 = fabric
                        align: 'center'
                    },
                    {
                        field: "TotalCourse",
                        title: "Total Course",
                        visible: (subGroupID != 1), //1 = fabric
                        align: 'center'
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
                            tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                            validate: function (value) {
                                if (!value || !value.trim() || isNaN(parseFloat(value)) || parseFloat(value) <= 0) {
                                    return 'Must be a positive value.';
                                }
                            }
                        }
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
                    if (field == "BookingQty") {
                        var rows = $formEl.find("#tblProgramInformation").bootstrapTable('getData');
                        if (rows && rows.length > 0) {
                            for (var i = 0; i < rows.length; i++) {
                                if (parseInt(rows[i].BookingQty) > parseInt(rows[i].MaxQty)) {
                                    rows[i].BookingQty = rows[i].MaxQty;
                                    toastr.error(`Given Qty ${rows[i].BookingQty} cannot be greater then maximum qty ${rows[i].MaxQty} at Program Information Row ${i + 1}`);
                                    break;
                                }
                            }
                            $formEl.find("#tblProgramInformation").bootstrapTable("load", rows);
                            $formEl.find("#tblProgramInformation").bootstrapTable('hideLoading');

                            rows = $formEl.find("#tblProgramInformation").bootstrapTable('getData');
                            var conceptQty = parseInt($formEl.find("#TotalQty").val()),
                                planQty = 0,
                                remainingQty = 0;

                            rows.map(x => {
                                planQty += parseInt(x.BookingQty);
                            });
                            remainingQty = conceptQty - planQty;

                            $formEl.find("#PlanQty").val(planQty);
                            $formEl.find("#BalanceQty").val(remainingQty);
                        }
                    }
                    else if (field == "Needle") {
                        row.TotalNeedle = Math.ceil((row.Length + (row.Length * (6 / 100))) * row.Needle);
                    }
                    else if (field == "CPI") {
                        row.TotalCourse = Math.ceil((row.Width + (row.Width * (6 / 100))) * (row.CPI / 2.54));
                    }
                    $formEl.find("#tblProgramInformation").bootstrapTable('load', masterData.KnittingPlans);
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
                var hasFirstItem = false;
                if (masterData.KnittingPlans.length > 0) {
                    hasFirstItem = true;
                    subGroupID = masterData.KnittingPlans[0].SubGroupID;
                    if (subGroupID == 1) {
                        $formEl.find(".divForFabric").fadeIn();
                        $formEl.find(".divForCollarCuff").fadeOut();
                    } else {
                        $formEl.find(".divForFabric").fadeOut();
                        $formEl.find(".divForCollarCuff").fadeIn();
                    }
                    masterData.KnittingPlans[0].ReqDeliveryDate = formatDateToDefault(masterData.KnittingPlans[0].ReqDeliveryDate);
                    masterData.KnittingPlans[0].StartDate = formatDateToDefault(masterData.KnittingPlans[0].StartDate);
                    masterData.KnittingPlans[0].EndDate = formatDateToDefault(masterData.KnittingPlans[0].EndDate);
                    masterData.KnittingPlans[0].ActualStartDate = formatDateToDefault(masterData.KnittingPlans[0].ActualStartDate);
                    masterData.KnittingPlans[0].ActualEndDate = formatDateToDefault(masterData.KnittingPlans[0].ActualEndDate);
                    masterData.KnittingPlans[0].ConceptDate = formatDateToDefault(masterData.KnittingPlans[0].ConceptDate);
                    masterData.KnittingPlans[0].RemainingQty = masterData.KnittingPlans[0].TotalQty - masterData.KnittingPlans[0].PlanQty;
                    masterData.KnittingPlans[0].ConceptDate = formatDateToDefault(masterData.KnittingPlans[0].ConceptDate);
                    setFormData($formEl, masterData.KnittingPlans[0]);
                }
                masterData.Yarns.map(x => x.YarnPly = 1);
                initTblYarn(masterData.Yarns);

                if (status == statusConstants.ALL) {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnRevise").fadeOut();
                } else {
                    $formEl.find("#btnSave").fadeIn();
                    $formEl.find("#btnRevise").fadeOut();
                }
                initTblFabricChild();

                if (hasFirstItem) {
                    var childObject = getNewItem(masterData.KnittingPlans[0]);
                    childObject.BookingQty = parseInt(masterData.TotalQty);
                    childObject.MaxQty = parseInt(masterData.MaxQty);
                    masterData.KnittingPlans[0].Childs.push(childObject);
                    $formEl.find("#tblProgramInformation").bootstrapTable('load', masterData.KnittingPlans[0].Childs);
                }

                if (subGroupID == 1) {
                    $formEl.find("#divGSM,#divComposition").fadeIn();
                    $formEl.find("#divLength,#divWidth").fadeOut();
                    //$formEl.find("#divActualNeedle,#divActualCPI").fadeOut();

                    $formEl.find("#divForFabric").fadeIn();
                    $formEl.find("#divForCollarCuff").fadeOut();
                } else {
                    $formEl.find("#divGSM,#divComposition").fadeOut();
                    $formEl.find("#divLength,#divWidth").fadeIn();
                    //$formEl.find("#divActualNeedle,#divActualCPI").fadeIn();

                    $formEl.find("#divForFabric").fadeOut();
                    $formEl.find("#divForCollarCuff").fadeIn();
                }

                initNewAttachment($formEl.find("#UploadFile"));
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getRevisionedDetails(planNo, groupConceptNo, subGroupName) {
        axios.get(`/api/knitting-program/group/revision/${planNo}/${groupConceptNo}/${subGroupName}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.StartDate = formatDateToDefault(masterData.StartDate);
                masterData.EndDate = formatDateToDefault(masterData.EndDate);
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);

                if (masterData.KnittingPlans.length > 0) {
                    subGroupID = masterData.KnittingPlans[0].SubGroupID;
                    if (subGroupID == 1) {
                        $formEl.find(".divForFabric").fadeIn();
                        $formEl.find(".divForCollarCuff").fadeOut();
                    } else {
                        $formEl.find(".divForFabric").fadeOut();
                        $formEl.find(".divForCollarCuff").fadeIn();
                    }
                    masterData.KnittingPlans[0].ReqDeliveryDate = formatDateToDefault(masterData.KnittingPlans[0].ReqDeliveryDate);
                    masterData.KnittingPlans[0].StartDate = formatDateToDefault(masterData.KnittingPlans[0].StartDate);
                    masterData.KnittingPlans[0].EndDate = formatDateToDefault(masterData.KnittingPlans[0].EndDate);
                    masterData.KnittingPlans[0].ActualStartDate = formatDateToDefault(masterData.KnittingPlans[0].ActualStartDate);
                    masterData.KnittingPlans[0].ActualEndDate = formatDateToDefault(masterData.KnittingPlans[0].ActualEndDate);
                    masterData.KnittingPlans[0].ConceptDate = formatDateToDefault(masterData.KnittingPlans[0].ConceptDate);
                    masterData.KnittingPlans[0].RemainingQty = masterData.KnittingPlans[0].TotalQty - masterData.KnittingPlans[0].PlanQty;
                    masterData.KnittingPlans[0].ConceptDate = formatDateToDefault(masterData.KnittingPlans[0].ConceptDate);

                    //masterData.KnittingPlans[0].KnittingTypeList = masterData.KnittingTypeList;
                    //setFormData($formEl, masterData.KnittingPlans[0]);
                    setFormData($formEl, masterData);
                    if (masterData.NeedPreFinishingProcess) {
                        $formEl.find("#need-yes").prop("checked", true);
                        $formEl.find("#need-no").prop("checked", false);
                    }
                    else {
                        $formEl.find("#need-yes").prop("checked", false);
                        $formEl.find("#need-no").prop("checked", true);
                    }
                    //$formEl.find("#KnittingTypeID").val(masterData.KnittingTypeID);
                }

                initTblYarn(masterData.Yarns);

                $formEl.find("#btnSave").hide();
                $formEl.find("#btnRevise").show();

                //if (masterData.SubGroupName === subGroupNames.FABRIC)
                initTblFabricChild();
                $formEl.find("#tblProgramInformation").bootstrapTable("load", masterData.KnittingPlans);
                $formEl.find("#tblProgramInformation").bootstrapTable('hideLoading');

                if (subGroupID == 1) {
                    $formEl.find("#divGSM,#divComposition").fadeIn();
                    $formEl.find("#divLength,#divWidth").fadeOut();
                    $formEl.find("#divActualNeedle,#divActualCPI").fadeOut();

                    $formEl.find("#divForFabric").fadeIn();
                    $formEl.find("#divForCollarCuff").fadeOut();

                } else {
                    $formEl.find("#divGSM,#divComposition").fadeOut();
                    $formEl.find("#divLength,#divWidth").fadeIn();
                    //$formEl.find("#divActualNeedle,#divActualCPI").fadeIn();

                    $formEl.find("#divForFabric").fadeOut();
                    $formEl.find("#divForCollarCuff").fadeIn();
                }
                calculateNeedle();
                calculateCPI();

                //IsSubContact
                if (masterData.IsSubContact == 1) {
                    $formEl.find("#IsSubContact").prop("checked", true);
                } else {
                    $formEl.find("#IsSubContact").prop("checked", false);
                }

                initAttachment(masterData.FilePath, masterData.AttachmentPreviewTemplate, $formEl.find("#UploadFile"));
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(groupConceptNo, planNo, isBDS, subgroupName) {
        var type = knittingProgramType.CONCEPT;
        if (isBDS == 1) type = knittingProgramType.BDS;
        else if (isBDS == 2) type = knittingProgramType.BULK;

        var url = `/api/knitting-program/group/${groupConceptNo}/${planNo}/${type}/${subgroupName}`;
        if (isAdditional) {
            url = `/api/knitting-program/group/addition/${groupConceptNo}/${planNo}/${type}/${subgroupName}`;
        }

        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.StartDate = formatDateToDefault(masterData.StartDate);
                masterData.EndDate = formatDateToDefault(masterData.EndDate);
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);

                if (masterData.KnittingPlans.length > 0) {
                    subGroupID = masterData.KnittingPlans[0].SubGroupID;
                    if (subGroupID == 1) {
                        $formEl.find(".divForFabric").fadeIn();
                        $formEl.find(".divForCollarCuff").fadeOut();
                    } else {
                        $formEl.find(".divForFabric").fadeOut();
                        $formEl.find(".divForCollarCuff").fadeIn();
                    }
                    masterData.KnittingPlans[0].ReqDeliveryDate = formatDateToDefault(masterData.KnittingPlans[0].ReqDeliveryDate);
                    masterData.KnittingPlans[0].StartDate = formatDateToDefault(masterData.KnittingPlans[0].StartDate);
                    masterData.KnittingPlans[0].EndDate = formatDateToDefault(masterData.KnittingPlans[0].EndDate);
                    masterData.KnittingPlans[0].ActualStartDate = formatDateToDefault(masterData.KnittingPlans[0].ActualStartDate);
                    masterData.KnittingPlans[0].ActualEndDate = formatDateToDefault(masterData.KnittingPlans[0].ActualEndDate);
                    masterData.KnittingPlans[0].ConceptDate = formatDateToDefault(masterData.KnittingPlans[0].ConceptDate);
                    masterData.KnittingPlans[0].RemainingQty = masterData.KnittingPlans[0].TotalQty - masterData.KnittingPlans[0].PlanQty;
                    masterData.KnittingPlans[0].ConceptDate = formatDateToDefault(masterData.KnittingPlans[0].ConceptDate);

                    //masterData.KnittingPlans[0].KnittingTypeList = masterData.KnittingTypeList;
                    //setFormData($formEl, masterData.KnittingPlans[0]);
                    setFormData($formEl, masterData);
                    if (masterData.NeedPreFinishingProcess) {
                        $formEl.find("#need-yes").prop("checked", true);
                        $formEl.find("#need-no").prop("checked", false);
                    }
                    else {
                        $formEl.find("#need-yes").prop("checked", false);
                        $formEl.find("#need-no").prop("checked", true);
                    }
                    //$formEl.find("#KnittingTypeID").val(masterData.KnittingTypeID);
                }

                initTblYarn(masterData.Yarns);

                if (status == statusConstants.ALL) {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnRevise").fadeOut();
                } else {
                    $formEl.find("#btnSave").fadeIn();
                    $formEl.find("#btnRevise").fadeOut();
                }
                initTblFabricChild();
                $formEl.find("#tblProgramInformation").bootstrapTable("load", masterData.KnittingPlans);
                $formEl.find("#tblProgramInformation").bootstrapTable('hideLoading');

                if (subGroupID == 1) {
                    $formEl.find("#divGSM,#divComposition").fadeIn();
                    $formEl.find("#divLength,#divWidth").fadeOut();
                    $formEl.find("#divActualNeedle,#divActualCPI").fadeOut();

                    $formEl.find("#divForFabric").fadeIn();
                    $formEl.find("#divForCollarCuff").fadeOut();

                } else {
                    $formEl.find("#divGSM,#divComposition").fadeOut();
                    $formEl.find("#divLength,#divWidth").fadeIn();
                    //$formEl.find("#divActualNeedle,#divActualCPI").fadeIn();

                    $formEl.find("#divForFabric").fadeOut();
                    $formEl.find("#divForCollarCuff").fadeIn();
                }

                //IsSubContact
                if (masterData.IsSubContact == 1) {
                    $formEl.find("#IsSubContact").prop("checked", true);
                } else {
                    $formEl.find("#IsSubContact").prop("checked", false);
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
        return isValidItemInfo;
    }
    function formDataToJson(f) {
        return Object.fromEntries(Array.from(f.keys(), k =>
            k.endsWith('[]') ? [k.slice(0, -2), f.getAll(k)] : [k, f.get(k)]));
    }
    function getChilds(conceptID, kpMasterID) {
        //KnittingPlanChild
        var kPlanChilds = masterData.KnittingPlans.filter(y => y.ConceptID == conceptID);
        if (kPlanChilds) {
            kPlanChilds.map(x => {
                x.KPMasterID = kpMasterID;
                x.ActualStartDate = x.StartDate;
                x.ActualEndDate = x.EndDate;
            });
        }
        return kPlanChilds == null ? [] : kPlanChilds;
    }
    function getYarns(kpMasterID) {
        //KnittingPlanYarn
        var yarns = [];
        masterData.Yarns.map(x => {
            var yarn = JSON.parse(JSON.stringify(x));
            yarn.KPMasterID = kpMasterID;
            yarns.push(yarn);
        });
        return yarns;
    }
    function getMasters(formObj) {
        var kPlans = [];
        var kpMasterID = 0;
        masterData.KnittingPlans.map(x => {
            if (status == statusConstants.PENDING_GROUP) {
                kpMasterID++;
                x.KPMasterID = kpMasterID;
            }
            x.Childs = getChilds(x.ConceptID, x.KPMasterID);
            x.Yarns = getYarns(x.KPMasterID);
            kPlans.push(x);
        });
        return kPlans;
    }
    function getChild(kpMaster) {
        var child = {
            KPMasterID: kpMaster.KPMasterID,
            MachineGauge: $formEl.find("#MachineGauge").val(),
            MachineDia: $formEl.find("#MachineDia").val(),
            SubGroupID: subGroupID,
            StartDate: $formEl.find("#StartDate").val(),
            EndDate: $formEl.find("#EndDate").val(),
            ActualStartDate: $formEl.find("#StartDate").val(),
            ActualEndDate: $formEl.find("#EndDate").val(),
            UnitID: kpMaster.KPMasterID,
            KPMasterID: kpMaster.KPMasterID,
            KPMasterID: kpMaster.KPMasterID,
            KPMasterID: kpMaster.KPMasterID,
            KPMasterID: kpMaster.KPMasterID,
            KPMasterID: kpMaster.KPMasterID,
        };
        return child;
    }
    function save(e, isApprove = false) {
        var formData = getFormData($formEl);
        var formObj = formDataToJson(formData);

        formObj.KnittingTypeID = $formEl.find("#KnittingTypeID").val();

        if (formObj.SubGroupID == 1) {
            if (typeof formObj.KnittingTypeID === "undefined" || formObj.KnittingTypeID == 0 || formObj.KnittingTypeID == null) {
                return toastr.error("Select Knitting Type.");
            }
        }
        else if (formObj.SubGroupID == 11 || formObj.SubGroupID == 12) {
            if (typeof formObj.Needle === "undefined" || formObj.Needle == 0 || formObj.Needle == null) {
                return toastr.error("Give Needle.");
            }
            if (typeof formObj.CPI === "undefined" || formObj.CPI == 0 || formObj.CPI == null) {
                return toastr.error("Give CPI.");
            }
        }

        if (formObj.MachineDia == null || parseInt(formObj.MachineDia) == 0 || formObj.MachineGauge == null || parseInt(formObj.MachineGauge) == 0) {
            return toastr.error("Select Machine.");
        }
        var kPlanGroup = {
            KnittingPlans: getMasters(formObj)
        };
        formData.append("GroupConceptNo", masterData.GroupConceptNo);
        formData.append("GroupID", masterData.GroupID);
        formData.append("IsAdditional", isAdditional);

        if (isAdditional && masterData.IsAdditional) {
            formData.append("ParentGroupID", masterData.ParentGroupID);
        }
        else if (isAdditional) {
            formData.append("ParentGroupID", masterData.GroupID);
        }

        for (var index = 0; index < kPlanGroup.KnittingPlans.length; index++) {
            if (parseFloat(kPlanGroup.KnittingPlans[index].BookingQty) > parseFloat(kPlanGroup.KnittingPlans[index].MaxQty)) {
                toastr.error(`Given Qty ${kPlanGroup.KnittingPlans[index].BookingQty} cannot be greater then maximum qty ${kPlanGroup.KnittingPlans[index].MaxQty} at Program Information Row ${index + 1}`);
                return false;
            }
        }

        var knittingPlans = [],
            knittingPlanChilds = [],
            knittingPlanYarns = [];

        kPlanGroup.KnittingPlans.map(x => {
            knittingPlanChilds.push(...x.Childs);
            x.Yarns.map(y => {
                if (is100Elastane(y.Composition)) {
                    y.StitchLength = 0;
                }
                knittingPlanYarns.push(y);
            });
            x.Childs = [];
            x.Yarns = [];
            knittingPlans.push(x);
        });

        kPlanGroup = formObj;

        formData.append("KPlanGroup", JSON.stringify(kPlanGroup));
        formData.append("KPlans", JSON.stringify(knittingPlans));
        formData.append("Childs", JSON.stringify(knittingPlanChilds));
        formData.append("Yarns", JSON.stringify(knittingPlanYarns));

        //formData.append("SubGroupID", subGroupID);

        var files = $formEl.find("#UploadFile")[0].files;
        formData.append("UploadFile", files[0]);

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

        axios.post("/api/knitting-program/save/group", formData, config)
            .then(function (response) {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(showResponseError);
    }

    function revise(e) {
        var formData = getFormData($formEl);
        var formObj = formDataToJson(formData);

        formObj.KnittingTypeID = $formEl.find("#KnittingTypeID").val();

        if (formObj.SubGroupID == 1) {
            if (typeof formObj.KnittingTypeID === "undefined" || formObj.KnittingTypeID == 0 || formObj.KnittingTypeID == null) {
                return toastr.error("Select Knitting Type.");
            }
        }
        else if (formObj.SubGroupID == 11 || formObj.SubGroupID == 12) {
            if (typeof formObj.Needle === "undefined" || formObj.Needle == 0 || formObj.Needle == null) {
                return toastr.error("Give Needle.");
            }
            if (typeof formObj.CPI === "undefined" || formObj.CPI == 0 || formObj.CPI == null) {
                return toastr.error("Give CPI.");
            }
        }

        if (formObj.MachineDia == null || parseInt(formObj.MachineDia) == 0 || formObj.MachineGauge == null || parseInt(formObj.MachineGauge) == 0) {
            return toastr.error("Select Machine.");
        }

        var kPlanGroup = {
            KnittingPlans: getMasters(formObj)
        };
        formData.append("GroupConceptNo", masterData.GroupConceptNo);
        formData.append("GroupID", masterData.GroupID);

        for (var index = 0; index < kPlanGroup.KnittingPlans.length; index++) {
            if (parseFloat(kPlanGroup.KnittingPlans[index].BookingQty) > parseFloat(kPlanGroup.KnittingPlans[index].MaxQty)) {
                toastr.error(`Given Qty ${kPlanGroup.KnittingPlans[index].BookingQty} cannot be greater then maximum qty ${kPlanGroup.KnittingPlans[index].MaxQty} at Program Information Row ${index + 1}`);
                return false;
            }
        }

        var knittingPlans = [],
            knittingPlanChilds = [],
            knittingPlanYarns = [];

        kPlanGroup.KnittingPlans.map(x => {
            knittingPlanChilds.push(...x.Childs);
            x.Yarns.map(y => {
                if (is100Elastane(y.Composition)) {
                    y.StitchLength = 0;
                }
                knittingPlanYarns.push(y);
            });
            x.Childs = [];
            x.Yarns = [];
            knittingPlans.push(x);
        });

        kPlanGroup = formObj;

        formData.append("KPlanGroup", JSON.stringify(kPlanGroup));
        formData.append("KPlans", JSON.stringify(knittingPlans));
        formData.append("Childs", JSON.stringify(knittingPlanChilds));
        formData.append("Yarns", JSON.stringify(knittingPlanYarns));

        //formData.append("SubGroupID", subGroupID);

        var files = $formEl.find("#UploadFile")[0].files;
        formData.append("UploadFile", files[0]);

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

        axios.post("/api/knitting-program/revise/group", formData, config)
            .then(function (response) {
                toastr.success("Revised successfully!");
                backToList();
            })
            .catch(showResponseError);
    }

    function getNewDataForKP() {
        var selectedRecords = $tblMasterEl.getSelectedRecords();
        if (selectedRecords.length == 0) {
            toastr.error("Please select row(s)!");
            return;
        }
        var conceptList = [],
            technicalNames = [],
            subGroupNames = [],
            compositions = [],
            gsmList = [];
        selectedRecords.map(x => {
            conceptList.push(x.GroupConceptNo);
            technicalNames.push(x.TechnicalName);
            subGroupNames.push(x.SubGroupName);
            compositions.push(x.Composition);
            gsmList.push(x.GSM);
        });
        var unique_array = [...new Set(conceptList)];
        if (unique_array.length > 1) {
            toastr.error("Concept number should same.");
            return;
        }
        unique_array = [...new Set(technicalNames)];
        if (unique_array.length > 1) {
            toastr.error("Technical name should same.");
            return;
        }
        var subGroupName = "";
        unique_array = [...new Set(subGroupNames)];
        if (unique_array.length > 1) {
            toastr.error("Sub group should same.");
            return;
        } else {
            subGroupName = subGroupNames[0];
        }
        if (subGroupName.toLowerCase() == "fabric") {
            unique_array = [...new Set(compositions)];
            if (unique_array.length > 1) {
                toastr.error("Composition should same.");
                return;
            }
            unique_array = [...new Set(gsmList)];
            if (unique_array.length > 1) {
                toastr.error("GSM should same.");
                return;
            }
        }

        var conceptIDs = selectedRecords.map(x => x.ConceptID).join(","),
            withoutOB = false,
            subGroupName = "F",
            isBulkPage = false;

        var hasFabric = false,
            hasCollar = false,
            hasCuff = false;

        var obj = selectedRecords.find(x => x.SubGroupID == 1); //Fabric
        if (obj) {
            hasFabric = true;
            subGroupName = "Fabric";
        }
        obj = selectedRecords.find(x => x.SubGroupID == 11); //Collar
        if (obj) {
            hasCollar = true;
            subGroupName = "Collar";
        }
        obj = selectedRecords.find(x => x.SubGroupID == 12); //Cuff
        if (obj) {
            hasCuff = true;
            subGroupName = "Cuff";
        }
        if (hasFabric && (hasCollar || hasCuff)) {
            toastr.error("Select only fabric item(s) or Collar and Cuff item(s)");
            return;
        }
        if (hasCollar && hasCuff) {
            subGroupName = "CollarCuff";
        }

        getNewForGroup(conceptIDs, subGroupName, isBulkPage, withoutOB);
        //$formEl.find("#btnCopyProgram").fadeIn();
    }
    function getNewForGroup(conceptIds, subGroupName, isBulkPage, withoutOB) {
        axios.get(`/api/knitting-program/new-kp/group/${conceptIds}/${isBulkPage}/${withoutOB}/${subGroupName}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.StartDate = formatDateToDefault(masterData.StartDate);
                masterData.EndDate = formatDateToDefault(masterData.EndDate);
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);

                var hasFirstItem = false;
                if (masterData.KnittingPlans.length > 0) {
                    hasFirstItem = true;
                    subGroupID = masterData.KnittingPlans[0].SubGroupID;
                    if (subGroupID == 1) {
                        $formEl.find(".divForFabric").fadeIn();
                        $formEl.find(".divForCollarCuff").fadeOut();
                    } else {
                        $formEl.find(".divForFabric").fadeOut();
                        $formEl.find(".divForCollarCuff").fadeIn();
                    }
                    masterData.KnittingPlans[0].ReqDeliveryDate = formatDateToDefault(masterData.KnittingPlans[0].ReqDeliveryDate);
                    masterData.KnittingPlans[0].ConceptDate = formatDateToDefault(masterData.KnittingPlans[0].ConceptDate);

                    masterData.KnittingPlans[0].StartDate = masterData.StartDate;
                    masterData.KnittingPlans[0].EndDate = masterData.EndDate;

                    masterData.KnittingPlans[0].KnittingTypeList = masterData.KnittingTypeList;
                    setFormData($formEl, masterData);

                    if (masterData.KnittingPlans[0].ProcessTime > 0) {
                        $formEl.find("#need-yes").prop("checked", true);
                        $formEl.find("#need-no").prop("checked", false);
                    }
                    else {
                        $formEl.find("#need-yes").prop("checked", false);
                        $formEl.find("#need-no").prop("checked", true);
                    }
                }

                masterData.Yarns.map(x => x.YarnPly = 1);
                initTblYarn(masterData.Yarns);

                initTblFabricChild();

                if (hasFirstItem) {
                    $formEl.find("#tblProgramInformation").bootstrapTable('load', masterData.KnittingPlans);
                    $tblChildEl.bootstrapTable("load", masterData.KnittingPlans);
                    $tblChildEl.bootstrapTable('hideLoading');
                }

                if (subGroupID == 1) {
                    $formEl.find("#divGSM,#divComposition").fadeIn();
                    $formEl.find("#divLength,#divWidth").fadeOut();
                    //$formEl.find("#divActualNeedle,#divActualCPI").fadeOut();

                    $formEl.find("#divForFabric").fadeIn();
                    $formEl.find("#divForCollarCuff").fadeOut();

                } else {
                    $formEl.find("#divGSM,#divComposition").fadeOut();
                    $formEl.find("#divLength,#divWidth").fadeIn();
                    //$formEl.find("#divActualNeedle,#divActualCPI").fadeIn();

                    $formEl.find("#divForFabric").fadeOut();
                    $formEl.find("#divForCollarCuff").fadeIn();
                }

                initNewAttachment($formEl.find("#UploadFile"));
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
})();
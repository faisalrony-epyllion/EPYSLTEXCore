(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $formEl, tblMasterId;
    var status = statusConstants.PENDING;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var masterData;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $formEl.find("#lblfabric,#lblother").hide();
        initMasterTable();


        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            initMasterTable();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnCancel").on("click", backToListWithoutFilter);

        $formEl.find('#Status').on('select2:select', function (e) {
            var statusid = e.params.data.id;
            if (statusid == 3 || statusid == 4) //3=Hold, 4=Cancel
            {
                $formEl.find(".reason").fadeIn();
                $formEl.find("#Reason").val(masterData.Reason);
            }
            else {
                $formEl.find(".reason").fadeOut();
                $formEl.find("#Reason").val("");
            }

        });

        $formEl.find("#btnSelectMachine").on("click", function (e) {
            e.preventDefault();

            var dataList = [];
            if (masterData.SubGroupName === subGroupNames.FABRIC) {
                dataList = masterData.KnittingMachines.filter(x => x.MachineSubClassID == masterData.MCSubClassID &&
                    x.IsSubContact == 0 &&
                    x.GG == masterData.MachineGauge);
                    //x.Dia == masterData.MachineDia); //New
            } else {
                dataList = masterData.KnittingMachines.filter(x => x.MachineSubClassID == masterData.MCSubClassID &&
                    x.GG == masterData.MachineGauge &&
                    x.IsSubContact == 0)
                    //x.Dia == masterData.MachineDia); //New
            }
            if (dataList.length == 0) {
                toastr.error("No machine found for gauge " + masterData.MachineGauge + " and dia " + masterData.MachineDia);
            } else {
                var finder = new commonFinder({
                    title: "Select Machine",
                    pageId: pageId,
                    data: dataList,
                    isMultiselect: false,
                    modalSize: "modal-md",
                    top: "2px",
                    primaryKeyColumn: "KnittingMachineID",
                    fields: "GG,Dia,Brand,Contact,MachineNo,Capacity,IsSubContact",
                    headerTexts: "GG,Dia,Brand,Unit,MachineNo,Capacity,SubContact?",
                    widths: "30,30,100,70,50,50,50",
                    onSelect: function (res) {
                        finder.hideModal();
                        $formEl.find("#ContactID").val(res.rowData.KnittingUnitID);
                        $formEl.find("#Uom").val(res.rowData.Contact);
                        $formEl.find("#KnittingMachineID").val(res.rowData.KnittingMachineID);
                        $formEl.find("#MachineGauge").val(res.rowData.GG);
                        $formEl.find("#MachineDia").val(res.rowData.Dia);
                        $formEl.find("#ContactID").val(res.rowData.KnittingUnitID);
                        $formEl.find("#Contact").val(res.rowData.Contact);
                        $formEl.find("#BrandID").val(res.rowData.BrandID);
                        $formEl.find("#Brand").val(res.rowData.Brand);
                        $formEl.find("#KnittingMachineNo").val(res.rowData.MachineNo);

                        masterData.KnittingMachineID = res.rowData.KnittingMachineID;
                        masterData.MachineGauge = res.rowData.GG;
                        masterData.MachineDia = res.rowData.Dia;
                        masterData.ContactID = res.rowData.KnittingUnitID;
                        masterData.Contact = res.rowData.Contact;
                        masterData.BrandID = res.rowData.BrandID;
                        masterData.Brand = res.rowData.Brand;
                        masterData.KnittingMachineNo = res.rowData.MachineNo;
                    },
                });
                finder.showModal();
            }
        });

    });

    function initMasterTable() {
        var commands = [{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
        { type: 'Report', title: 'Job Card Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
        ];

        var columns = [
            {
                headerText: '', commands: commands, textAlign: 'Center', width: ch_setActionCommandCellWidth(commands)
            },
            {
                field: 'StatusInText', headerText: 'Status', width: 60
            },
            {
                field: 'IsConfirm', headerText: 'Knitting Confirm?', allowEditing: false, displayAsCheckBox: true, editType: "booleanedit", width: 85, textAlign: 'Center'
            },
            {
                field: 'PlanNo', headerText: 'Program No', visible: false, width: 70
            },
            {
                field: 'KJobCardNo', headerText: 'Job Card No', width: 90
            },
            {
                field: 'KJobCardDate', headerText: 'Job Card Date', textAlign: 'center', type: 'date', format: _ch_date_format_1, visible: false, width: 90
            },
            {
                field: 'ConceptNo', headerText: 'Concept No', width: 100
            },
            {
                field: 'ConceptDate', headerText: 'Concept Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: false, width: 90
            },
            {
                field: 'KnittingMachineNo', headerText: 'M/C No', textAlign: 'Center', width: 40
            },
            {
                field: 'MachineGauge', headerText: 'M/C Gauge', textAlign: 'Center', width: 40
            },
            {
                field: 'MachineDia', headerText: 'M/C Dia', textAlign: 'Center', width: 40
            },
            {
                field: 'Brand', headerText: 'Brand', textAlign: 'Center', width: 70
            },
            {
                field: 'Uom', headerText: 'Floor', textAlign: 'Center', width: 50
            },
            {
                field: 'SubGroupName', headerText: 'SubGroup Name', visible: false, width: 60
            },
            {
                field: 'BookingQty', headerText: 'Booking / Concept Qty', visible: false, textAlign: 'Center', width: 60
            },
            {
                field: 'KJobCardQty', headerText: 'Job Card Qty', textAlign: 'Center', width: 60
            },
            {
                field: 'ProdQty', headerText: 'Production Qty(kg)', textAlign: 'Center', width: 60
            },
            {
                field: 'ProdQtyPcs', headerText: 'Production Qty(Pcs)', textAlign: 'Center', width: 60
            },
            {
                field: 'MachineType', headerText: 'Machine Type', textAlign: 'Center', width: 70
            },
            {
                field: 'Buyer', headerText: 'Buyer', textAlign: 'Center', width: 80
            },
            {
                field: 'KnittingType', headerText: 'Knitting Type', textAlign: 'Center', visible: false, width: 70
            },
            {
                field: 'TechnicalName', headerText: 'Technical Name', textAlign: 'Center', width: 80
            },
            {
                field: 'ColorName', headerText: 'Color Name', textAlign: 'Center', width: 80
            },
            {
                field: 'GSM', headerText: 'GSM', textAlign: 'Center', width: 80
            },
            {
                field: 'FabricComposition', headerText: 'Composition', textAlign: 'Center', width: 120
            },
            {
                field: 'BuyerTeam', headerText: 'Buyer Team', textAlign: 'Center', visible: false, width: 80
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/job-card-status/list?status=${status}`,
            columns: columns,
            autofitColumns: false,
            commandClick: handleCommands,
            allowGrouping: true,
            allowFiltering: getfilterdata2,
            showColumnChooser: true,
            allowExcelExport: true,
            allowPdfExport: true,
            showDefaultToolbar: false,
            //dataStateChange=dataStateChange,
            toolbar: ['ColumnChooser', 'ExcelExport'],
            handleToolbarClick: toolbarClickExcelMaster,
            keyPressed: getfilterdata
        });
    }
    function toolbarClickExcelMaster(args) {
        if (args['item'].id.indexOf('_excelexport') >= 0) {
            //var exportProperties = {
            //    hierarchyExportMode: "Expanded"
            //};
            //$tblChildEl.excelExport(exportProperties);
            $tblMasterEl.excelExport();
        } else if (args['item'].id.indexOf('_pdfexport') >= 0) {
            var exportProperties = {
                bAllowHorizontalOverflow: false
            };
            $tblMasterEl.pdfExport(exportProperties);
        }
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.KJobCardMasterID);
        }
        else if (args.commandColumn.type == "Report") {
            if (args.rowData.SubGroupName === 'Fabric') {
                window.open(`/reports/InlinePdfView?ReportName=KnittingJobCard.rdl&JobCardNo=${args.rowData.KJobCardNo}`, '_blank');
            } else {
                window.open(`/reports/InlinePdfView?ReportName=KnittingJobCardCC.rdl&JobCardNo=${args.rowData.KJobCardNo}`, '_blank');
            }
        }
    }

    function getfilterdata(args) {
        if (args.key === "Enter") {
            alert("Filtering");
        }
    }

    function getfilterdata2(args) {
        if (args.key === "Enter") {
            alert("Filtering");
        }
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
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getDetails(kJobCardMasterId) {
        axios.get(`/api/job-card-status/${kJobCardMasterId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ProductionDate = formatDateToDefault(masterData.ProductionDate);
                setFormData($formEl, masterData);
                if (masterData.Status == 3 || masterData.Status == 4) //3=Hold, 4=Cancel
                    $formEl.find(".reason").fadeIn();
                else $formEl.find(".reason").fadeOut();

                if (masterData.SubGroupName === subGroupNames.FABRIC) {
                    $formEl.find("#divComposition,#divGSM,#divFabric,#lblfabric").fadeIn();
                    $formEl.find("#divOtherItem,#PcsRollQty,#PcsRollQtyLeb,#lblother").fadeOut();
                } else {
                    $formEl.find("#divComposition,#divGSM,#divFabric,#lblfabric").fadeOut();
                    $formEl.find("#PcsRollQty,#PcsRollQtyLeb,#divOtherItem,#lblother").fadeIn();
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        axios.post("/api/job-card-status/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

})();
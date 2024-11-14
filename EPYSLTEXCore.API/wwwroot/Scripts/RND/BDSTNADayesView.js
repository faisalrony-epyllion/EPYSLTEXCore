(function () {
    var menuId, pageName;
    var toolbarId, _oRow, _index, _modalFrom, _oRowCollar, _indexCollar, _oRowCuff, _indexCuff;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, tblMasterId, tblChildId, tblChildBookingId, $tblChildBookingrIdEl, $tblChildEventIdEl
        , tblChildEventId, pageId;
    var idsList = [];
    var status = statusConstants.ACTIVE;
    var CriteriaName;
    var masterData;
    var bmtArray = [];
   
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
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        tblChildBookingId = "#tblChildBookingId" + pageId;
        tblChildEventId = "#tblChildEventId" + pageId;
        $formEl.find("#divProductionId").hide();
        $formEl.find("#divEventDateRangeId").hide();
        $formEl.find("#divStructureId").hide();
        $formEl.find("#btnProceedf").hide();
        $("#btnAddBookingNo").hide();
        $("#btnAddEvent").hide();
        initMasterTable();

        tblPlanningId = "#tblPlanning" + pageId;
        $modalPlanningEl = $("#modalPlanning" + pageId);
        tblCriteriaId = "#tblCriteria" + pageId;
        $modalCriteriaEl = $("#modalCriteria" + pageId);
        tblMasterEventId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        $toolbarEl.find("#btnList").on("click", function (e) {
            getProduction();
        });

        $toolbarEl.find("#btnRcvList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACTIVE;
            initMasterTable();
         });

        $toolbarEl.find("#btnDateList").on("click", function (e) {
            getDateList();
            $formEl.find("#divProductionId").hide();
            $formEl.find("#divEventDateRangeId").show();
        });

        $toolbarEl.find("#btnEventList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACTIVE;
            getEventList();
            $("#divEventInfo").show();
        });

        $formEl.find("#btnAddEvent").on("click", AddEvent);

        $toolbarEl.find("#btnRefreshList").on("click", function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
            //$tblMasterEventEl.refresh();
        });

        $formEl.find("#btnProceed").click(function (e) {
            e.preventDefault();
            var FromDate = $formEl.find("#EventDate").val();
            var TotDate = $formEl.find("#ToDate").val();
            $("#divFabricInfo").show();
            initChild(FromDate, TotDate);
        });
        $formEl.find("#btnAddBookingNo").click(function (e) {
            e.preventDefault();
            var FromDate = $formEl.find("#EventDate").val();
            var TotDate = $formEl.find("#ToDate").val();
            $("#divBookingInfo").show();
            AddBookingNo();
        });
        $formEl.find("#btnReceive").click(function (e) {
            e.preventDefault();
            Receive(this);
        });

        $formEl.find("#btnUnAcknowledge").click(function (e) {

            bootbox.prompt("Enter your UnAcknowledge reason:", function (result) {
                if (!result) {
                    return toastr.error("UnAcknowledge reason is required.");
                }
                save(result);
            });
        });

        $formEl.find("#btnCancel").on("click", backToList);

      

        //GreyQCDefectHKs
        axios.get(`/api/fabric-con-sub-class-tech-name/list`)
            .then(function (response) {
                bmtArray = response.data;
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    });

    function getProduction() {
        var url = "/api/bds-acknowledge/boookingIdList/";
        axios.get(url)
            .then(function (response) {
                $formEl.find("#divProductionId").show();
                $formEl.find("#divEventDateRangeId").hide();
                $formEl.find("#divStructureId").hide();
                $formEl.find("#general").hide();
                $formEl.find("#btnCancel").show();
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnProceed").hide();
                $("#btnAddBookingNo").show();
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                setFormData($formEl, masterData);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getDateList() {
        var url = "/api/bds-acknowledge/boookingIdList/";
        axios.get(url)
            .then(function (response) {
                $formEl.find("#divProductionId").hide();
                $formEl.find("#divEventDateRangeId").show();
                $formEl.find("#divStructureId").hide();
                $formEl.find("#general").hide();
                $formEl.find("#btnCancel").show();
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnProceed").show();
               // $("#divFabricInfo").show();
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.EventDate = formatDateToDefault(masterData.EventDate);
                setFormData($formEl, masterData);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getEventList() {
        var url = "/api/bds-acknowledge/boookingIdList/";
        axios.get(url)
            .then(function (response) {
                $formEl.find("#divProductionId").hide();
                $formEl.find("#divEventDateRangeId").hide();
                $formEl.find("#divStructureId").show();
                $formEl.find("#general").hide();
                $formEl.find("#btnCancel").show();
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnProceed").hide();
                $("#btnAddEvent").show();
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.EventDate = formatDateToDefault(masterData.EventDate);
                setFormData($formEl, masterData);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function tableColumns() {
        var columns = [
            {
                field: 'EventDescription', headerText: 'Event Description'
            },
            {
                field: 'EventStatus', headerText: 'Event Status'
            },
            {
                field: 'BookingNo', headerText: 'Booking No'
            },
            {
                field: 'BuyerName', headerText: 'Buyer Name'
            },
            {
                field: 'Color', headerText: 'Color'
            },
            {
                field: 'Construction', headerText: 'Construction'
            },
            {
                field: 'TechnicalName', headerText: 'Technical Name'
            },
            {
                field: 'Composition', headerText: 'Composition'
            },
            {
                field: 'GSM', headerText: 'GSM'
            },
            {
                field: 'BookingDate', headerText: 'Booking Date', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'AcknowledgeDate', headerText: 'Acknowledge Date', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'EventDate', headerText: 'Event Date', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'CompleteDate', headerText: 'Complete Date', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'SeqNo', headerText: 'Seq No', visible: false
            },
            {
                field: 'SystemEvent', headerText: 'System Event?', displayAsCheckBox: true, textAlign: 'Center', visible: false
            },
            {
                field: 'HasDependent', headerText: 'Depent Event?', displayAsCheckBox: true, textAlign: 'Center', visible: false
            },
            {
                headerText: '', width: 100, commands: [
                    { type: 'Add', title: 'View Depend Event', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }
                ], visible: false
            },
            {
                field: 'MachineType', headerText: 'Machine Type', visible: false
            },
            {
                field: 'LengthYds', headerText: 'Length(Yds) ', visible: false
            },
            {
                field: 'LengthInch', headerText: 'Length(Inch) ', visible: false
            },
            {
                field: 'ColorName', headerText: 'Color Name', visible: false
            },
            {
                field: 'BuyerTeamName', headerText: 'Buyer Team Name', visible: false
            }

        ];

        return columns;
    }

    function initMasterTable() {
        var columns = tableColumns();

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/bds-acknowledge/TNAlist`,
            columns: columns,
            showColumnChooser: true,
            allowExcelExport: true,
            allowPdfExport: true,
            showDefaultToolbar: false,
            toolbar: ['ColumnChooser', 'ExcelExport'],
            //toolbar: ['ColumnChooser', 'ExcelExport', 'PdfExport'],
            rowDataBound: rowBoundForColor,
            commandClick: handleCommands,
            handleToolbarClick: toolbarClickExcelMaster
        });
    }
    function toolbarClickExcelMaster(args) {
        //debugger;
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

    function AddEvent(e) {
        e.preventDefault();
        var finder = new commonFinder({
            title: "Select Event",
            pageId: pageId,
            height: 350,
            apiEndPoint: "/api/bds-acknowledge/Eventlist/",
            fields: "EventDescription",
            headerTexts: "Event Description",
           // customFormats: ",ej2GridColorFormatter",
            widths: "100",
            isMultiselect: true,
            primaryKeyColumn: "EventID",
            onMultiselect: function (selectedRecords) {
                var EventID;
                var EID = Array();
                selectedRecords.forEach(function (value) {
                    EventID = value.EventID;
                    EID.push(EventID);
                });

                var id = EID.map(function (el) { return el }).toString();
                //(id);
                initEvent(id);
            }
        });

        finder.showModal();
    }

    function AddBookingNo(e) {
        //e.preventDefault();
        var finder = new commonFinder({
            title: "Select Bookig No",
            pageId: pageId,
            height: 350,
            apiEndPoint: "/api/bds-acknowledge/boookingIdList/",
            fields: "BookingNo",
            headerTexts: "Booking No",
            // customFormats: ",ej2GridColorFormatter",
            widths: "100",
            isMultiselect: true,
            primaryKeyColumn: "BookingID",
            onMultiselect: function (selectedRecords) {
                var BookingID;
                var bID = Array(); 
                selectedRecords.forEach(function (value) {
                    BookingID = value.BookingID;
                    bID.push(BookingID);
                });

                var id = bID.map(function (el) { return el }).toString();
                //(id);
                initBooking(id);
            }
        });

        finder.showModal();
    }

    function handleCommands(args) {
        //console.log(args);
        var eventID = args.rowData.EventID;
        if (args.commandColumn.type == 'Add' && args.rowData.HasDependent== true) {
            var finder = new commonFinder({
                title: "Depend Event List",
                pageId: pageId,
                height: 350,
                apiEndPoint: `/api/bds-acknowledge/EventDescriptionList?EventID=${eventID}`,
                fields: "EventDescription",
                headerTexts: "Event Description",
                // customFormats: ",ej2GridColorFormatter",
                widths: "100",
                primaryKeyColumn: "DepenEventID"
            });

            finder.showModal();
        }
     }

    function initChild(FromDate, TotDate) {
        var columns = tableColumns();

        if ($tblChildEl) $tblChildEl.destroy();
        $tblChildEl = new initEJ2Grid({
            tableId: tblChildId,
            apiEndPoint: `/api/bds-acknowledge/bookingList?FromDate=${FromDate}&TotDate=${TotDate}`,
            columns: columns,
            showColumnChooser: true,
            allowExcelExport: true,
            allowPdfExport: true,
            showDefaultToolbar: false,
            toolbar: ['ColumnChooser', 'ExcelExport'],
            //toolbar: ['ColumnChooser', 'ExcelExport', 'PdfExport'],
            rowDataBound: rowBoundForColor,
            commandClick: handleCommands,
            handleToolbarClick: toolbarClickExcel
        });
    }

    function toolbarClickExcel(args) {
        //debugger;
        if (args['item'].id.indexOf('_excelexport') >= 0) {
            //var exportProperties = {
            //    hierarchyExportMode: "Expanded"
            //};
            //$tblChildEl.excelExport(exportProperties);
            $tblChildEl.excelExport();
        } else if (args['item'].id.indexOf('_pdfexport') >= 0) {
            var exportProperties = {
                bAllowHorizontalOverflow: false
            };
            $tblChildEl.pdfExport(exportProperties);
        }
    }
    function rowBoundForColor(args) {
        //debugger;
        var data = args.data['EventStatus'];
        //if (data === "Complete" || data.indexOf('Complete before ') == 0) {
        //    args.row.classList.add('success-TNA');
        //} else if (data.indexOf('Complete after ') == 0) {
        //    args.row.classList.add('info_TNA');
        //} else if (data.indexOf('Days over') != -1) {
        //    args.row.classList.add('danger-TNA');
        //} else {
        //    args.row.classList.add('warning-TNA');
        //}

        if (data.indexOf('Days over') != -1) {
            args.row.classList.add('danger-TNA');
        } else if (data === "1 Days remaining" || data === "2 Days remaining" || data === "3 Days remaining") {
            args.row.classList.add('warning-TNA');
        }
    }

    async function initBooking(data) {
        var columns = tableColumns();

        if ($tblChildBookingrIdEl) $tblChildBookingIdEl.destroy();
        $tblChildBookingIdEl = new initEJ2Grid({
            tableId: tblChildBookingId,
            apiEndPoint: `/api/bds-acknowledge/bookingWiseList?ListData=${data}`,
            columns: columns,
            showColumnChooser: true,
            allowExcelExport: true,
            allowPdfExport: true,
            showDefaultToolbar: false,
            toolbar: ['ColumnChooser', 'ExcelExport'],
            //toolbar: ['ColumnChooser', 'ExcelExport', 'PdfExport'],
            rowDataBound: rowBoundForColor,
            commandClick: handleCommands,
            handleToolbarClick: toolbarClickExcelBooking
        });
    }
    function toolbarClickExcelBooking(args) {
        //debugger;
        if (args['item'].id.indexOf('_excelexport') >= 0) {
            //var exportProperties = {
            //    hierarchyExportMode: "Expanded"
            //};
            //$tblChildEl.excelExport(exportProperties);
            $tblChildBookingIdEl.excelExport();
        } else if (args['item'].id.indexOf('_pdfexport') >= 0) {
            var exportProperties = {
                bAllowHorizontalOverflow: false
            };
            $tblChildBookingIdEl.pdfExport(exportProperties);
        }
    }

    async function initEvent(data) {
        var columns = tableColumns();

        if ($tblChildEventIdEl) $tblChildEventIdEl.destroy();
        $tblChildEventIdEl = new initEJ2Grid({
            tableId: tblChildEventId,
            apiEndPoint: `/api/bds-acknowledge/EventWiseList?EventListData=${data}`,
            columns: columns,
            showColumnChooser: true,
            allowExcelExport: true,
            allowPdfExport: true,
            showDefaultToolbar: false,
            toolbar: ['ColumnChooser', 'ExcelExport'],
            //toolbar: ['ColumnChooser', 'ExcelExport', 'PdfExport'],
            rowDataBound: rowBoundForColor,
            commandClick: handleCommands,
            handleToolbarClick: toolbarClickExcelEvent
        });
    }
    function toolbarClickExcelEvent(args) {
        //debugger;
        if (args['item'].id.indexOf('_excelexport') >= 0) {
            //var exportProperties = {
            //    hierarchyExportMode: "Expanded"
            //};
            //$tblChildEl.excelExport(exportProperties);
            $tblChildEventIdEl.excelExport();
        } else if (args['item'].id.indexOf('_pdfexport') >= 0) {
            var exportProperties = {
                bAllowHorizontalOverflow: false
            };
            $tblChildEventIdEl.pdfExport(exportProperties);
        }
    }

    function backToList() {
        initMasterTable();
        $divDetailsEl.fadeOut();
        $("#divEventInfo").hide();
        $("#divFabricInfo").hide();
        $("#divBookingInfo").hide();
        $("#btnAddBookingNo").hide();
        $("#btnAddEvent").hide();
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
})();
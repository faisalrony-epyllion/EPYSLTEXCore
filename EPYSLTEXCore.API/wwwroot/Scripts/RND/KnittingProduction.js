(function () {
    var menuId, pageName;
    var toolbarId;
    var $pageEl, $divTblEl, $divDetailsEl, $divMasterTblEl, $toolbarEl,
        $tblMasterEl, $tblChildEl, $tblYarnChildEl, $formEl, tblMasterId, $tblChildElJobCardChild,
        tblKJChildId;
    var filterBy = {};
    var status = statusConstants.PENDING;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var isEditable = true;
    var masterData;
    var selectedkJobChild = null;
    var _keybuffer = "", _jobCardNo = "", _punchCardNo = "", _kjcSubGroupName = "";
    var WeightURL, PrinterName;
    var _shifts = [];
    var scaleWeight = "";
    var scaleOperator = "";
    var weightID = "";
    var scaleID = "";
    var clientIP = "";
    var _printerName = "";

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $pageEl = $("#" + pageId);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $tblYarnChildEl = $("#tblYarnChild" + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $divMasterTblEl = $("#divMasterTbl" + pageId);
        tblKJChildId = "#tblKJChildId" + pageId;

        initMasterTable();

        $formEl.find("#lblfabric,#lblother").hide();
        $formEl.find("#PcsRollQty").keyup(function (event) {
            if (event.keyCode === 13) {
                $formEl.find("#btnAddItem").click();
            }
        });

        $formEl.find("#PRollQty").keyup(function (event) {
            if (event.keyCode === 13) {
                $formEl.find("#btnAddItem").click();
            }
        });

        $toolbarEl.find("#btnPendingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            //  getIP(false);
            initMasterTable();
            $divTblEl.fadeIn();
            $divDetailsEl.fadeOut();
            $toolbarEl.fadeIn();
            $divMasterTblEl.fadeIn();
            scaleWeight = 0;
            scaleOperator = '';
            scaleAddress = '';
            $formEl.find("#PRollQty").val(0);
            $formEl.find("#PcsRollQty").val(0);
            $formEl.find("#PRollQty").focus();
            $formEl.find("#POperatorId").val('').trigger('change');
            $formEl.find(".divProximityCardNo").hide();
        });

        $toolbarEl.find("#btnActiveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            //   getIP(false);
            initMasterTable();
            $divTblEl.fadeIn();
            $divDetailsEl.fadeOut();
            $toolbarEl.fadeIn();
            $divMasterTblEl.fadeIn();
            scaleWeight = 0;
            scaleOperator = '';
            scaleAddress = '';
            $formEl.find("#PRollQty").val(0);
            $formEl.find("#PcsRollQty").val(0);
            $formEl.find("#PRollQty").focus();
            $formEl.find("#POperatorId").val('').trigger('change');
            $formEl.find(".divProximityCardNo").hide();
        });

        $toolbarEl.find("#btnProductionList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED;
            //  getIP(false);
            getProduction();
            _jobCardNo = "";
            _punchCardNo = "";
            _keybuffer = "";
            $formEl.find(".divProximityCardNo").hide();
        });

        $toolbarEl.find("#btnProductionListV2").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            //   getIP(false);
            getProduction();
            _jobCardNo = "";
            _punchCardNo = "";
            _keybuffer = "";
            $formEl.find(".divProximityCardNo").hide();
        });

        $toolbarEl.find("#btnProductionListV3").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACKNOWLEDGE;
            //  getIP(true);
            getProduction();
            _jobCardNo = "";
            _punchCardNo = "";
            _keybuffer = "";
            $formEl.find("#PRollQty").val(0);
            $formEl.find("#PcsRollQty").val(0);
            $formEl.find("#POperatorId").val('').trigger('change');
            $formEl.find(".divProximityCardNo").show();
        });

        $formEl.find("#btnAddItem").on("click", function (e) {
            e.preventDefault();
            addNewItem();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnSaveQC").click(function (e) {
            e.preventDefault();
            saveQC(this);
        });

        $formEl.find("#btnProceed").click(function (e) {
            e.preventDefault();
            var jobcardNo = $formEl.find("#JobCardNop").val();
            getDetailsp(jobcardNo);
        });

        $formEl.find("#btnCancel").on("click", backToListWithoutFilter);

        $formEl.find("#JobCardNop").keyup(function (event) {
            var jobCardNop = $formEl.find("#JobCardNop").val();

            if (jobCardNop.length > 11) {
                getJobCard(jobCardNop);
            }
        });

        $formEl.find("#ProximityCardNo").keyup(function (event) {
            var pCardNo = $formEl.find("#ProximityCardNo").val();
            if (pCardNo.length == 8) {
                getOperator(pCardNo);
            }
        });
        $formEl.find("#ITM").change(function () {
            itmChange();
        });

        //setInterval(function () {
        //    GetRollWeightByURL();
        //}, 5000);

        $formEl.find("#ProximityCardNov3").keyup(function (e) {
            var proximityCardNo = $.trim($formEl.find("#ProximityCardNov3").val());
            if (!proximityCardNo) {
                $formEl.find("#ProximityCardNov3").focus();
                return false;
            }
            if (proximityCardNo.length >= 10) {
                setOperator(proximityCardNo);
            }
        });
    });
    $(document).on("keyup", function (e) {
        if (status == statusConstants.PROPOSED_FOR_APPROVAL) {
            var code = e.keyCode || e.which;
            _keybuffer += String.fromCharCode(code).trim();

            if (e.keyCode == 13) {
                if (_keybuffer.length == 11) {
                    _jobCardNo = _keybuffer;
                    _keybuffer = "";
                    if (_punchCardNo == _jobCardNo.substr(0, 8)) {
                        _punchCardNo = "";
                        $formEl.find("#ProximityCardNov2").val(_punchCardNo);
                    }
                }
                else if (_keybuffer.length == 8) {
                    _punchCardNo = _keybuffer;
                    _keybuffer = "";
                }
                else if (_keybuffer.length > 15) {
                    if (_keybuffer.substr(8, 19).length == 11) {
                        _jobCardNo = _keybuffer.substr(8, 19);
                        _keybuffer = "";
                    }
                }
            }
            else if (_keybuffer.length == 8) {
                _punchCardNo = _keybuffer;
                $formEl.find("#ProximityCardNov2").val(_punchCardNo);
            }

            if (_punchCardNo.length == 8 && _jobCardNo.length > 11) {
                $formEl.find("#ProximityCardNov2").val(_punchCardNo);
                $formEl.find("#JobCardNopv2").val(_jobCardNo);

                getJobCard(_jobCardNo);
                getOperator(_punchCardNo);
            } else if (_jobCardNo.length > 11) {
                $formEl.find("#JobCardNopv2").val(_jobCardNo);
            }
        }
        else if (status == statusConstants.ACKNOWLEDGE) {
            var jobCardNo = $.trim($formEl.find("#JobCardNopv3").val());

            if (!jobCardNo || jobCardNo.length < 12) {
                $formEl.find("#JobCardNopv3").focus();
                return false;
            }
            proceedKP(jobCardNo);
        }
    });

    function itmChange() {
        if ($formEl.find("#ITM").is(':checked')) {
            $formEl.find(".notReqField").hide();
        } else {
            $formEl.find(".notReqField").show();
        }
        $formEl.find(".itm").show();
        //initYarnChildTable();
    }
    function GetWeightByURL(url, scID, wID) {

        $.ajax({
            type: 'POST',
            url: 'http://127.0.0.1:17080/?op=scaleweight',
            crossDomain: true,
            data: url,
            dataType: 'text'
        }).done(function (responseData) {
            console.log(responseData);
            if (responseData != 'N') {
                responseData = JSON.parse(responseData);
                scaleWeight = responseData.weight;
                scaleOperator = responseData.rfid;
                weightID = responseData.weight_id;
                scaleID = responseData.scale_id;
                if (scID == scaleID && wID != weightID) {
                    if (scaleWeight != 0 && (scaleOperator != '' || scaleOperator != 'null')) {
                        if (scaleWeight) {
                            masterData.PRollQty = scaleWeight;
                            toastr.info("weight updated")
                        }
                        if (scaleOperator) {
                            if (masterData.OperatorList.length > 0) {
                                var opr = masterData.OperatorList.find(x => x.additionalValue == scaleOperator);
                                if (opr) {
                                    masterData.POperatorId = opr.id;
                                    toastr.info("Operator updated")
                                }
                            }
                        }
                        setFormData($formEl, masterData);
                        weight_timer = 0;
                        getRealTimeWeightStop();
                        if (masterData.POperatorId == 0) {
                            toastr.warning("Must select operator")
                        }
                        if (status == statusConstants.ACKNOWLEDGE && masterData.POperatorId > 0 && parseFloat(masterData.PRollQty) > 0) {
                            $formEl.find("#btnAddItem").click();
                        }
                    }
                }
            }
        }).fail(function (responseData) {
            console.log(responseData);
        });
    }
    function GetRollWeightByURL() {
        $formEl.find("#PRollQty").val(0);
        $formEl.find("#PcsRollQty").val(0);

        $.ajax({
            type: 'POST',
            url: 'http://127.0.0.1:17080/?op=scaleweight',
            crossDomain: true,
            data: masterData.PRollQty,
            dataType: 'text'
        }).done(function (responseData) {
            if (responseData != 'N') {
                scaleWeight = parseFloat(responseData);

                if (scaleWeight) {
                    masterData.PRollQty = scaleWeight;
                    // toastr.info("weight updated")
                }
                setFormData($formEl, masterData);
                //weight_timer = 0;
                //getRealTimeWeightStop();
                if (masterData.POperatorId == 0) {
                    // toastr.warning("Must select operator")
                }
                if (status == statusConstants.ACKNOWLEDGE && masterData.POperatorId > 0 && parseFloat(masterData.PRollQty) > 0) {
                    $formEl.find("#btnAddItem").click();
                }
            }
        }).fail(function (responseData) {
            console.log(responseData);
        });
    }
    function SetWeightIDByURL(url) {
        axios.get(url)
            .then(function (response) {
                if (response) {

                } else {
                    return toastr.error("Invalid weight & operator");
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function SendToPrinter(printerName, parameters) {
        //const printerName = printerName;
        window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
        var data = window.location.origin + "/reports/InlinePdfView?reportid=1255&RollNo=" + parameters;
        var url = 'http://127.0.0.1:17080/?op=print&printtype=url&printerName=' + printerName;
        $.ajax({
            type: 'POST',
            url: url,
            crossDomain: true,
            data: data,
            dataType: 'text'
        }).done(function (responseData) {
            console.log(responseData);
        }).fail(function (responseData) {
            console.log(responseData);
        });
    }

    function getJobCard(jobCardNop) {

        var url = `/api/rnd-knitting-production/newjob/${jobCardNop}`;
        axios.get(url)
            .then(function (response) {
                if (response.data) {
                    $formEl.find("#ProximityCardNo").focus();
                } else {
                    return toastr.error("Job Card not found!");
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function addNewItem() {
        
        var PcsRollQty;
        if ($formEl.find("#PcsRollQty").val() == '' || $formEl.find("#PRollQty").val() <= 0) {
            PcsRollQty = 0;
        } else {
            PcsRollQty = $formEl.find("#PcsRollQty").val();
        }
        if (masterData.JobCardStatus == "Complete") {
            return toastr.error("This production is already completed.");
        }
        if (masterData.JobCardStatus == "Running") {
            var rollKG = $formEl.find("#PRollQty").val();
            if (rollKG == 0) {
                return toastr.error("Give roll qty.");
            }
            //else if (rollKG > 1) {
            //    $formEl.find("#PRollQty").val(1);
            //    return toastr.error("Roll qty can not be greater than 1.");
            //}
        }
        else if (masterData.JobCardQty <= masterData.ProducedQty) {
            toastr.error("This production is already completed.");
            $formEl.find("#ProdComplete").prop('checked', true);
            return false;
        }

        if (masterData.SubGroupName != subGroupNames.FABRIC) {
            if ($formEl.find("#PcsRollQty").val() == '' || $formEl.find("#PcsRollQty").val() <= 0) {
                return toastr.error("Please Select Roll in Pcs");
            }
        }

        var isValidationNeed = true;
        if (masterData.IsSubContact) {
            isValidationNeed = false;
        } else {
            if ($formEl.find("#ITM").is(':checked')) {
                isValidationNeed = false;
            } else {
                isValidationNeed = true;
            }
        }


        if ($formEl.find("#PShiftId").val() == null && isValidationNeed) {
            toastr.warning("Select Shift", "Required");
        }
        else if ($formEl.find("#POperatorId").val() == null && isValidationNeed) {
            toastr.warning("Select Operator", "Required");
        }
        else if ($formEl.find("#PRollQty").val() == '' || $formEl.find("#PRollQty").val() <= 0 || $formEl.find("#PRollQty").val() > 50) {
            toastr.warning("Input quantity should be greater than 0 and less than or equal 50!", "Required");
        }
        else if (masterData.Childs.length == 1 && !masterData.GrayFabricOK) {
            toastr.warning("You can not add more than one roll because gray fabric isn't ok!!", "Required");
        }
        //else if (masterData.SubGroupName != subGroupNames.FABRIC) {
        //    if (($formEl.find("#PcsRollQty").val() == '' || $formEl.find("#PcsRollQty").val() <= 0) && isValidationNeed) {
        //        toastr.warning("Please Select Roll in Pcs", "Required");
        //    }
        //}
        else {
            var shiftName = "";
            if (isValidationNeed) {
                var currentShift = getCurrentShift();
                if (currentShift != null) {
                    shiftName = currentShift.text;
                    $formEl.find("#PShiftId").val(currentShift.id).trigger('change');
                }
            }

            var newChildItem = {
                GRollID: getMaxIdForArray(masterData.Childs, "GRollID"),
                KJobCardMasterID: masterData.KJobCardMasterID,
                KJobCardNo: masterData.KJobCardNo,
                ConceptID: masterData.ConceptID,
                RollSeqNo: getMaxIdForArray(masterData.Childs, "RollSeqNo"),
                Width: $formEl.find("#FWidth").val(),
                RollQty: $formEl.find("#PRollQty").val(),
                RollQtyPcs: PcsRollQty,
                RollLength: 0,
                ShiftID: !isValidationNeed ? 0 : $formEl.find("#PShiftId").val(),
                Shift: shiftName,
                OperatorID: !isValidationNeed ? 0 : $formEl.find("#POperatorId").val(),
                ProductionDate: new Date(),
                ProductionGSM: masterData.ProductionGSM,
                ProductionWidth: masterData.ProductionWidth,
                ActualGreyHeight: masterData.ActualGreyHeight,
                ActualGreyLength: masterData.ActualGreyLength,
                JobCardQty: masterData.JobCardQty,
                ProducedQty: masterData.ProducedQty + parseInt($formEl.find("#PRollQty").val()),
                FirstRollCheck: 0,
                ITM: masterData.IsSubContact ? false : $formEl.find("#ITM").is(":checked"),
                DateUpdated: new Date(),
                EntityState: 4
            };

            if (checkKJobChild()) {
                masterData.Childs.push(newChildItem);
                $tblYarnChildEl.bootstrapTable('load', masterData.Childs);
                $formEl.find("#PRollQty").val(0);
                $formEl.find("#PcsRollQty").val(0);
                $formEl.find("#PRollQty").focus();
                $formEl.find("#POperatorId").val('').trigger('change');
                $formEl.find("#ITM").prop('checked', false);
                if (!masterData.IsSubContact) {
                    itmChange();
                }
                //This comments must be remove
                saveSingleRoll(newChildItem);
            }
        }
    }

    function initMasterTable() {
        var commands = status == statusConstants.PENDING
            ? [{ type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }]
            : [{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }];

        var columns = [
            {
                headerText: 'Actions', commands: commands, textAlign: 'Center'
            },
            {
                field: 'PlanNo', headerText: 'Program No', visible: false
            },
            {
                field: 'KJobCardNo', headerText: 'Job Card No'
            },
            {
                field: 'JobCardDate', headerText: 'Job Card Date', textAlign: 'center', type: 'date', format: _ch_date_format_1, visible: status === statusConstants.COMPLETED
            },
            {
                field: 'ProductionDate', headerText: 'Production Date', textAlign: 'center', type: 'date', format: _ch_date_format_1, visible: status === statusConstants.COMPLETED
            },
            //{
            //    field: 'EWO', headerText: 'EWO'
            //},
            {
                field: 'ConceptNo', headerText: 'Concept No'
            },
            {
                field: 'ConceptDate', headerText: 'Concept Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'Buyer', headerText: 'Buyer', textAlign: 'Center', width: 80
            },
            {
                field: 'SubGroupName', headerText: 'Sub Group Name'
            },
            {
                field: 'BookingQty', headerText: 'Booking / Concept Qty'
            },
            {
                field: 'JobCardQty', headerText: 'Job Card Qty'
            },
            {
                field: 'ProducedQty', headerText: 'Produced Qty', visible: status === statusConstants.COMPLETED
            },
            {
                field: 'BalanceQty', headerText: 'Balance Qty', visible: status === statusConstants.COMPLETED
            },
            {
                field: 'MachineType', headerText: 'Machine Type', textAlign: 'Left'
            },
            //{
            //    field: 'KnittingType', headerText: 'Knitting Type', textAlign: 'Center'
            //},
            {
                field: 'TechnicalName', headerText: 'Technical Name', textAlign: 'Left'
            },
            {
                field: 'Composition', headerText: 'Composition', textAlign: 'Left'
            },
            {
                field: 'ColorName', headerText: 'Color', width: 100
            },
            {
                field: 'GSM', headerText: 'GSM', width: 100, textAlign: 'Left', headerTextAlign: 'Center'
            },
            {
                field: 'Size', headerText: 'Size', width: 100
            },
            {
                field: 'JobCardStatus', headerText: 'Job Card Status', visible: status === statusConstants.COMPLETED
            },
            {
                field: 'ProdComplete', headerText: 'Prod Complete?', allowEditing: false, displayAsCheckBox: true, editType: "booleanedit", width: 85, textAlign: 'Center', visible: status === statusConstants.COMPLETED
            },
            {
                field: 'IsSubContact', headerText: 'Sub Contact?', allowEditing: false, displayAsCheckBox: true, editType: "booleanedit", width: 85, textAlign: 'Center'
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/rnd-knitting-production/list?status=${status}`,
            columns: columns,
            autofitColumns: (status == statusConstants.COMPLETED),
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (status === statusConstants.PENDING) {
            getNew(args.rowData.KJobCardMasterID, args.rowData.IsBDS, args.rowData.ConceptID, args.rowData.GroupConceptNo);
        }
        else {
            getDetails(args.rowData.KJobCardMasterID, args.rowData.IsBDS, args.rowData.ConceptID, args.rowData.GroupConceptNo);
        }
    }

    function initChildTable() {
        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            uniqueId: 'CCColorID',
            editable: isEditable,
            columns: [
                {
                    field: "ColorName",
                    title: "Color Name",
                },
                {
                    field: "ColorCode",
                    title: "Color Code",
                }
            ]
        });
    }

    function initYarnChildTable() {
        $tblYarnChildEl.bootstrapTable("destroy");

        var isVisible = true,
            isVisibleITM = false;
        if (masterData.IsSubContact) {
            isVisible = false;
            isVisibleITM = false;
        } else {
            isVisibleITM = true;
            if ($formEl.find("#ITM").is(':checked')) {
                isVisible = false;
            } else {
                isVisible = true;
            }
        }

        $tblYarnChildEl.bootstrapTable({
            uniqueId: 'GRollID',
            editable: isEditable,
            checkboxHeader: false,
            columns: [
                {
                    width: 20,
                    visible: status == statusConstants.COMPLETED,
                    formatter: function (value, row, index, field) {
                        return status != statusConstants.COMPLETED ? '' : [
                            '<span class="btn-group">',
                            '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete">',
                            '<i class="fa fa-remove"></i>',
                            '</a>',
                            `<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportId=1255&RollNo=${row.RollNo}" target="_blank" title="Roll Wise Bar Code">
                                <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                </a>`,
                            `<a class="btn btn-xs btn-danger printRoll" href="javascript:void(0)" title="Roll Print">
                                <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                </a>`,
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            showBootboxConfirm("Delete Record.", "Are you sure want to delete this?", function (yes) {
                                if (yes) {
                                    masterData.Childs.splice(index, 1);
                                    $tblYarnChildEl.bootstrapTable('load', masterData.Childs);
                                    if (row.EntityState == 16) {  //modify
                                        row.EntityState = 8; //Delete
                                        if (checkKJobChild()) {
                                            saveSingleRoll(row);
                                        }
                                    }
                                }
                            });
                        },
                        'click .printRoll': function (e, value, row, index) {
                            _printerName = "Microsoft Print to PDF";
                            if (row.RollNo) {
                                SendToPrinter(_printerName, row.RollNo);
                            }
                        }
                    }
                },
                {
                    field: "RollSeqNo",
                    title: "Roll Seq No"
                },
                {
                    field: "ProductionDate",
                    title: "Production Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "DateUpdated",
                    title: "Added Date",
                    formatter: function (value, row, index, field) {
                        return formatDateTimeToDefault(value);
                    }
                },
                {
                    field: 'Composition',
                    title: 'Composition',
                    visible: masterData.SubGroupID == 1
                },
                {
                    field: 'GSM',
                    title: 'GSM',
                    visible: masterData.SubGroupID == 1
                },
                {
                    field: 'ColorName',
                    title: 'Color',
                    visible: masterData.SubGroupID == 1
                },
                {
                    field: 'Size',
                    title: 'Size',
                    visible: masterData.SubGroupID != 1
                },
                {
                    field: "Shift",
                    title: "Shift",
                    visible: isVisible
                },
                //{
                //    field: "ShiftID",
                //    title: "Shift",
                //    align: 'center',
                //    editable: {
                //        type: 'select2',
                //        title: 'Select Shift',
                //        inputclass: 'input-sm',
                //        showbuttons: false,
                //        disableElement:true,
                //        source: masterData.ShiftList,
                //        select2: { width: 200, placeholder: 'Select Shift', allowclear: true }
                //    }
                //},
                {
                    field: "OperatorID",
                    title: "Operator",
                    align: 'center',
                    visible: isVisible,
                    editable: {
                        type: 'select2',
                        title: 'Select Operator',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: masterData.OperatorList,
                        select2: { width: 200, placeholder: 'Select Operator', allowclear: true }
                    }
                },
                {
                    field: "RollQty",
                    title: "Roll Qty (Kg)",
                    align: 'center',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" max="40" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseFloat(value)) || parseFloat(value) <= 0) {
                                return 'Must be a positive value!';
                            }
                        }
                    }
                },
                {
                    field: "RollQtyPcs",
                    title: "Roll Qty (Pcs)",
                    align: 'center',
                    visible: $formEl.find("#SubGroupName").val() != 'Fabric',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value))) {
                                return 'Must be a positive integer.';
                            }
                        }
                    }
                },
                {
                    field: "ITM",
                    title: "ITM",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    checkbox: true,
                    showSelectTitle: true,
                    checkboxEnabled: false,
                    visible: isVisibleITM
                }
            ],
            onEditableSave: function (field, row, oldValue, $el) {
                if (row.RollQty == '' || row.RollQty <= 0 || row.RollQty >= 40)
                    if ($formEl.find("#PRollQty").val() == '' || $formEl.find("#PRollQty").val() <= 0 || $formEl.find("#PRollQty").val() > 40) {
                        row.RollQty = oldValue;
                        return toastr.warning("Input quantity should be greater than 0 and less than or equal 40!", "Required");
                    }

                var a = new Date().getTime();
                var b = new Date(row.DateUpdated).getTime();
                var milliseconds = a > b ? a % b : b % a;
                var minutes = (milliseconds / 1000) / 60;
                if (minutes > 30)
                    return toastr.warning("30 mins passed. You can not edit Roll: " + row.RollSeqNo + "!");

                row.DateUpdated = new Date();
                $tblYarnChildEl.bootstrapTable('updateByUniqueId', { id: row.GRollID, row: row });

                if (checkKJobChild()) {
                    saveSingleRoll(row);
                }
            }
        });
    }

    function backToListWithoutFilter() {
        if (status != statusConstants.PROPOSED && status != statusConstants.PROPOSED_FOR_APPROVAL) {
            if (status == statusConstants.ACKNOWLEDGE) {
                $toolbarEl.find("#btnProductionListV3").click();
            }
            $divDetailsEl.fadeOut();
            resetForm();
            $divTblEl.fadeIn();
        }
        else {
            getProduction();
        }
        _jobCardNo = "";
        _punchCardNo = "";
        _keybuffer = "";
    }
    function backToList() {
        backToListWithoutFilter();
        if (status != statusConstants.PROPOSED && status != statusConstants.PROPOSED_FOR_APPROVAL) {
            initMasterTable();
        }
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#GRollID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function notReqFieldsConditions() {
        if (masterData.IsSubContact) {
            $formEl.find(".notReqField").hide();
        } else {
            $formEl.find(".notReqField").show();
        }
    }

    function getOperator(pCardNo) {
        var url = "/api/rnd-knitting-production/GetOperator/" + pCardNo;
        axios.get(url)
            .then(function (response) {
                masterData = response.data;

                notReqFieldsConditions();

                var opName = response.data.OperatorList.map(function (el) { return el.text.toString(); });
                $formEl.find("#OperatorName").val(opName);
                $formEl.find("#OperatorNamev2").val(opName);

                var jobCardNo = $formEl.find("#JobCardNop").val();
                var jobCardNoV2 = $formEl.find("#JobCardNopv2").val();

                if (!jobCardNo && !jobCardNoV2) {
                    return toastr.error("Please enter Job Card No!");
                }
                if (response.data.OperatorList.length == 0) {
                    _punchCardNo = "";
                    _keybuffer = "";
                    return toastr.error("No Operator Found!");
                }

                jobCardNo = jobCardNo ? jobCardNo : jobCardNoV2;
                getDetailsp(jobCardNo, response.data.OperatorList);
                _jobCardNo = "";
                _punchCardNo = "";
                _keybuffer = "";
                // startCount();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function hideShowDivProdForAck() {
        $formEl.find("#divProductionId").hide();
        $formEl.find("#divProductionIdV2").hide();
        $formEl.find("#divProductionIdV3").show();
    }
    function getProduction() {
        var url = "/api/rnd-knitting-production/newp/";
        axios.get(url)
            .then(function (response) {
                if (status == statusConstants.PROPOSED) {
                    $formEl.find("#divProductionId").show();
                    $formEl.find("#divProductionIdV2").hide();
                    $formEl.find("#divProductionIdV3").hide();
                }
                else if (status == statusConstants.PROPOSED_FOR_APPROVAL) {
                    $formEl.find("#divProductionId").hide();
                    $formEl.find("#divProductionIdV2").show();
                    $formEl.find("#divProductionIdV3").hide();
                }
                else if (status == statusConstants.ACKNOWLEDGE) {
                    hideShowDivProdForAck();
                }
                $formEl.find("#general").hide();
                $formEl.find("#btnCancel").show();
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnProceed").show();
                $divTblEl.fadeIn();
                $divDetailsEl.fadeIn();
                $toolbarEl.fadeIn();
                $divMasterTblEl.fadeOut();
                masterData = response.data;

                notReqFieldsConditions();

                setFormData($formEl, masterData);
                $formEl.find("#JobCardNop").focus();
                $formEl.find("#JobCardNopv2").focus();
                $formEl.find("#JobCardNopv3").focus();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    var myInterval;

    function getRealTimeWeight() {
        if (weight_timer != 0) {
            GetRollWeightByURL();
        }
    }

    function getRealTimeWeightStop() {
        clearInterval(myInterval);
    }
    function getNew(kJobCardMasterID, isBDS, conceptId, grpConceptNo) {
        var url = `/api/rnd-knitting-production/new/${kJobCardMasterID}/${isBDS}/${conceptId}/${grpConceptNo}/`;
        axios.get(url)
            .then(function (response) {
                $formEl.find("#JobCardNopv3").val("");

                $divDetailsEl.fadeIn();
                $formEl.find("#general").show();
                $formEl.find("#divProductionId").hide();
                $formEl.find("#divProductionIdV2").hide();
                $formEl.find("#divProductionIdV3").hide();
                $formEl.find("#btnCancel").show();
                $formEl.find("#btnSave").show();
                $formEl.find("#btnProceed").hide();
                $divTblEl.fadeOut();
                masterData = response.data;

                notReqFieldsConditions();

                masterData.ProductionDate = formatDateToDefault(masterData.ProductionDate);

                WeightURL = masterData.WeightURL;
                PrinterName = masterData.PrinterName;

                $formEl.find("#KnittingTypeId").prop("disabled", true);
                $formEl.find("#CompositionId").prop("disabled", true);
                $formEl.find("#GSMId").prop("disabled", true);

                _shifts = masterData.ShiftList;

                if (scaleWeight) {
                    masterData.PRollQty = scaleWeight;
                }
                if (scaleOperator) {
                    if (masterData.OperatorList.length > 0) {
                        var opr = masterData.OperatorList.find(x => x.additionalValue == scaleOperator);
                        if (opr) {
                            masterData.POperatorId = opr.id;
                        }
                    }
                }
                //setOperator(proximityCardNo);
                setFormData($formEl, masterData);
                initChildTable();
                initYarnChildTable();

                //$formEl.find("#ProdComplete").prop("disabled", true);
                $tblChildEl.bootstrapTable("load", masterData.ChildColors);
                $tblChildEl.bootstrapTable('hideLoading');

                $formEl.find("#ConceptID").val(masterData.ConceptID);
                $formEl.find("#KJobCardMasterID").val(masterData.KJobCardMasterID);
                $formEl.find("#FWidth").val(masterData.FWidth);
                $formEl.find("#PRollQty").focus();

                if (status == statusConstants.ACKNOWLEDGE) {
                    $formEl.find("#ProximityCardNov3").val("");
                    $formEl.find("#ProximityCardNov3").focus();
                }
                _kjcSubGroupName = masterData.SubGroupName;
                if (masterData.SubGroupName === subGroupNames.FABRIC) {
                    $formEl.find("#divComposition,#divGSM,#divFabric,#lblfabric,#lblfabricbq,#lblfabricpq").fadeIn();
                    $formEl.find("#divOtherItem,#PcsRollQty,#PcsRollQtyLeb,#lblother,#lblotherbq,#lblotherpq").fadeOut();
                } else {
                    $formEl.find("#divComposition,#divGSM,#divFabric,#lblfabric,#lblfabricbq,#lblfabricpq").fadeOut();
                    $formEl.find("#PcsRollQty,#PcsRollQtyLeb,#divOtherItem,#lblother,#lblotherbq,#lblotherpq").fadeIn();
                }

                if (status == statusConstants.ACKNOWLEDGE && masterData.POperatorId > 0 && parseFloat(masterData.PRollQty) > 0) {
                    $formEl.find("#btnAddItem").click();
                }

                initJobCardChilds(masterData.KJobCardChilds);


                //  startCount();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function setOperator(proximityCardNo) {
        if (proximityCardNo) {
            var operator = masterData.OperatorList.find(x => x.additionalValue == proximityCardNo);
            if (operator) {
                $formEl.find("#POperatorId").val(operator.id).trigger('change');
                masterData.POperatorId = operator.id;
                GetRollWeightByURL();
            }
        }
        $formEl.find("#ProximityCardNov3").val("");
        $formEl.find("#PRollQty").focus();
    }

    var weight_timer = 0;
    function getDetails(kJobCardMasterId, isBDS, conceptId, grpConceptNo, operatorList = [], proximityCardNo) {
        axios.get(`/api/rnd-knitting-production/${kJobCardMasterId}/${isBDS}/${conceptId}/${grpConceptNo}`)
            .then(function (response) {
                $formEl.find("#JobCardNopv3").val("");

                $divDetailsEl.fadeIn();
                $formEl.find("#general").show();
                $formEl.find("#divProductionId").hide();
                $formEl.find("#divProductionIdV2").hide();
                $formEl.find("#divProductionIdV3").hide();
                $formEl.find("#btnCancel").show();
                $formEl.find("#btnSave").show();
                $formEl.find("#btnProceed").hide();
                $divTblEl.fadeOut();

                masterData = response.data;
                notReqFieldsConditions();

                masterData.ProductionDate = formatDateToDefault(masterData.ProductionDate);
                $formEl.find("#KnittingTypeId").prop("disabled", true);
                $formEl.find("#CompositionId").prop("disabled", true);
                $formEl.find("#GSMId").prop("disabled", true);

                _shifts = masterData.ShiftList;

                if (operatorList.length > 0) {
                    masterData.OperatorList = operatorList;
                    masterData.POperatorList = operatorList;
                    masterData.POperatorId = operatorList[0].id;
                }
                if (scaleWeight) {
                    masterData.PRollQty = scaleWeight;
                }
                if (scaleOperator) {
                    if (masterData.OperatorList.length > 0) {
                        var opr = masterData.OperatorList.find(x => x.additionalValue == scaleOperator);
                        if (opr) {
                            masterData.POperatorId = opr.id;
                        }
                    }
                }
                //setOperator(proximityCardNo);
                setFormData($formEl, masterData);

                var BalQty = 0;
                if (masterData.Childs.length > 0) {
                    //firstRollCheck = masterData.Childs[0].FirstRollCheck;// true;
                    var SumforollQty = 0;
                    var JobCardQty = masterData.JobCardQty;
                    $.each(masterData.Childs, function (index, value) {
                        SumforollQty += value.RollQty;
                    });
                    BalQty = JobCardQty - SumforollQty;
                    if (BalQty < 0) {
                        BalQty = 0;
                    }
                }

                //$formEl.find("#BalanceQty").val(BalQty);
                initChildTable();
                initYarnChildTable();

                $formEl.find("#ProdComplete").prop("disabled", false);
                $tblChildEl.bootstrapTable("load", masterData.ChildColors);
                $tblChildEl.bootstrapTable('hideLoading');
                $tblYarnChildEl.bootstrapTable("load", masterData.Childs);
                $tblYarnChildEl.bootstrapTable('hideLoading');
                $formEl.find("#ConceptID").val(masterData.ConceptID);
                $formEl.find("#KJobCardMasterID").val(masterData.KJobCardMasterID);
                $formEl.find("#FWidth").val(masterData.FWidth);
                $formEl.find("#PRollQty").focus();
                if (status == statusConstants.ACKNOWLEDGE) {
                    $formEl.find("#ProximityCardNov3").val("");
                    $formEl.find("#ProximityCardNov3").focus();
                }
                _kjcSubGroupName = masterData.SubGroupName;
                if (masterData.SubGroupName === subGroupNames.FABRIC) {
                    $formEl.find("#divComposition,#divGSM,#divFabric,#lblfabric,#lblfabricbq,#lblfabricpq").fadeIn();
                    $formEl.find("#divOtherItem,#PcsRollQty,#PcsRollQtyLeb,#lblother,#lblotherbq,#lblotherpq").fadeOut();
                } else {
                    $formEl.find("#divComposition,#divGSM,#divFabric,#lblfabric,#lblfabricbq,#lblfabricpq").fadeOut();
                    $formEl.find("#PcsRollQty,#PcsRollQtyLeb,#divOtherItem,#lblother,#lblotherbq,#lblotherpq").fadeIn();
                }

                if (status == statusConstants.ACKNOWLEDGE && masterData.POperatorId > 0 && parseFloat(masterData.PRollQty) > 0) {
                    $formEl.find("#btnAddItem").click();
                }
                initJobCardChilds(masterData.KJobCardChilds);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetailsp(jobCardMasterNob, operatorList) {
        var url = `/api/rnd-knitting-production/newjob/${jobCardMasterNob}`;
        axios.get(url)
            .then(function (response) {
                masterData = response.data;
                getDetails(masterData.KJobCardMasterID, masterData.IsBDS, masterData.ConceptID, masterData.GroupConceptNo, operatorList);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    var count = 1;
    var timeout;
    var timer_on = 0;
    function startCount() {
        if (!timer_on) {
            timer_on = 1;
            SetWeight();
        }
    }

    function stopCount() {
        clearTimeout(timeout);
        timer_on = 0;
    }
    function SetWeight() {
        getWeight()
        timeout = setTimeout(SetWeight, 600);
    }
    function getWeight() {

        if ($formEl.find("#POperatorId").val() != '' && WeightURL != '') {
            var url = WeightURL;
            axios.get(url)
                .then(function (response) {
                    if (response != '') {
                        var res = response;//JSON.parse(response);
                        if (count == 1)
                            count = res['data'].data.id;

                        if (parseFloat(count) == 3 && $formEl.find("#PRollQty").val() != '') {

                            stopCount();
                            $formEl.find("#btnAddItem").click();
                            //count = 1;
                            //startCount();
                        } else if (parseFloat(count) >= 3 && $formEl.find("#PRollQty").val() == '') {
                            $formEl.find("#PRollQty").val(count++);
                        }


                        //stopCount();
                    }
                    //$formEl.find("#PRollQty").val(count++);
                })
                .catch(function (err) {
                    //
                    //clearTimeout(timeout);
                    $formEl.find("#PRollQty").val(count++);
                    toastr.error(err.response.data.Message);
                    stopCount();

                });
        }
    }
    function save() {
        var sumRollQty = 0;
        for (var i = 0; i < masterData.Childs.length; i++) {
            sumRollQty = parseInt(sumRollQty) + parseInt(masterData.Childs[i].RollQty);
        }
        if (parseInt(sumRollQty) > (parseInt(masterData.JobCardQty) + 8)) {
            toastr.warning("Sum of Roll Quantity can't more than 8Kg of Job Card Quantity!!!");
            return;
        }
        if (masterData.Childs.length == 0) {
            toastr.error("At least one roll item required!", "Error");
            return;
        }
        var data = formDataToJson($formEl.serializeArray());
        if ($formEl.find("#ProdComplete").is(':checked') == true)
            data.ProdComplete = true;
        data["List"] = masterData.Childs;

        axios.post("/api/rnd-knitting-production/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                //backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function saveQC() {
        var data = formDataToJson($formEl.serializeArray());
        if ($formEl.find("#ProdComplete").is(':checked') == true)
            data.ProdComplete = true;

        data.SendforQC = true;
        data["Childs"] = masterData.Childs;
        if (masterData.Childs.length == 0) {
            toastr.error("At least one roll item required!", "Error");
        }
        else {
            axios.post("/api/rnd-knitting-production/saveQC", data)
                .then(function () {
                    toastr.success("Saved successfully.");
                    backToList();
                })
                .catch(function (error) {
                    toastr.error(error.response.data.Message);
                });
        }
    }
    function IsInTimeSlot(shift, currentTime) {
        if (parseFloat(shift.desc) <= parseFloat(shift.additionalValue)) {
            return currentTime >= parseFloat(shift.desc) && currentTime <= parseFloat(shift.additionalValue);
        }
        else {
            return currentTime >= parseFloat(shift.desc) || currentTime <= parseFloat(shift.additionalValue);
        }
    }
    function getCurrentShift() {
        var hour = new Date().getHours();
        var min = new Date().getMinutes();
        var currentTime = parseFloat(hour + "." + min); //"02:30";
        var newShiftList = JSON.parse(JSON.stringify(_shifts));

        /*
        var shift = newShiftList.find(x => currentTime > parseFloat(x.desc) && currentTime < parseFloat(x.additionalValue));
        shift.desc = shift.desc.replace(".", ":");
        shift.additionalValue = shift.additionalValue.replace(".", ":");
        */

        var shift = newShiftList.find(x => IsInTimeSlot(x, currentTime));
        return shift;
    }
    function checkKJobChild() {
        if (masterData.KJobCardChilds && masterData.KJobCardChilds.length == 1) {
            selectedkJobChild = masterData.KJobCardChilds[0];
        }
        if (masterData.KJobCardChilds.length > 0 && selectedkJobChild == null) {
            if (masterData.SubGroupID == 1) {
                toastr.error("Select Composition, GSM & Color.");
                return false;
            } else {
                toastr.error("Select Size.");
                return false;
            }
        }
        return true;
    }
    function saveSingleRoll(roll) {
        //var currentShift = getCurrentShift();
        //if (currentShift != null) {
        //    roll.ShiftID = parseInt(currentShift.id);
        //    $formEl.find("#PShiftId").val(roll.ShiftID).trigger('change');
        //}
        if (selectedkJobChild) {
            roll.KJobCardChildID = selectedkJobChild.KJobCardChildID;
        }
        var isValueSave = true;
        if (roll.ITM && (roll.OperatorID > 0)) {
            roll.OperatorID = 0;
            isValueSave = false;
            toastr.error("Operator is not required for ITM.");
        }

        axios.post("/api/rnd-knitting-production/save-roll", roll)
            .then(function (response) {
                masterData.Childs = response.data;

                $tblYarnChildEl.bootstrapTable('load', response.data);
                $tblYarnChildEl.bootstrapTable('hideLoading');

                if (masterData.Childs.length > 0 && $.trim(masterData.Childs[0].Message).length > 0) {
                    isValueSave = false;
                    toastr.error(masterData.Childs[0].Message);
                }

                masterData.JobCardStatus = masterData.Childs[0].JobCardStatus;
                masterData.ProdComplete = masterData.Childs[0].ProdComplete;

                var BalQty = 0;
                if (response.data.length > 0) {
                    var SumforollQty = 0;
                    var JobCardQty = $formEl.find("#JobCardQty").val();
                    if (_kjcSubGroupName === subGroupNames.FABRIC) {
                        $.each(response.data, function (index, value) {
                            SumforollQty += value.RollQty;
                        });
                    } else {
                        $.each(response.data, function (index, value) {
                            SumforollQty += value.RollQtyPcs;
                        });
                    }

                    BalQty = JobCardQty - SumforollQty;
                    if (BalQty < 0) {
                        BalQty = 0;
                    }
                }
                $formEl.find("#ProducedQty").val(SumforollQty);
                $formEl.find("#BalanceQty").val(BalQty);

                masterData.ProducedQty = SumforollQty;
                masterData.BalanceQty = BalQty;

                if (masterData.JobCardQty <= masterData.ProducedQty) {
                    $formEl.find("#ProdComplete").attr("disabled", true);
                    $formEl.find("#ProdComplete").prop("checked", true);
                } else {
                    $formEl.find("#ProdComplete").prop("checked", false);
                    $formEl.find("#ProdComplete").attr("disabled", false);
                }

                if (status == statusConstants.ACKNOWLEDGE && masterData.Childs.length > 0) {
                    var obj = $(masterData.Childs).get(-1);
                    _printerName = "Microsoft Print to PDF";
                    if (_printerName && obj.RollNo) {
                        //var updateURL = `/api/rnd-knitting-production/update-weight-scale/${clientIP}/${scaleID}/${weightID}/`;
                        //SetWeightIDByURL(updateURL);
                        SendToPrinter(_printerName, obj.RollNo);
                    }
                }
                if (isValueSave) toastr.success("Saved successfully.");
                if (status == statusConstants.ACKNOWLEDGE) {
                    scaleWeight = 0;
                    scaleOperator = '';
                    scaleAddress = '';
                    backToList();
                }

            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function loadReportInformation() {
        var hasExternalReport = false;
        if (!hasExternalReport) {
            buyerId = 0;
            $("#liBuyer").remove();
        }
        var url = rootPath + "/reports/GetReportInformation?reportId=" + reportId + "&hasExternalReport=" + hasExternalReport + "&buyerId=" + buyerId;
        axios.get(url)
            .then(function (response) {
                columnValues = response.data.FilterSetList;
                columnValueOptions = response.data.ColumnValueOptions;
                shownColumnValues = columnValues.filter(function (el) {
                    return !el.IsSystemParameter && !el.IsHidden && el.ColumnName != "Expression";
                });
                initTable(shownColumnValues);
            })
            .catch(showResponseError);
    }
    function proceedKP(jobCardNo) {
        var url = `/api/rnd-knitting-production/v3/${jobCardNo}`;
        axios.get(url)
            .then(function (response) {
                if (response.data) {

                    var jobCard = response.data;
                    if (jobCard.Childs.length == 0) {
                        getNew(jobCard.KJobCardMasterID, jobCard.IsBDS, jobCard.ConceptID, jobCard.GroupConceptNo);
                    } else {
                        getDetails(jobCard.KJobCardMasterID, jobCard.IsBDS, jobCard.ConceptID, jobCard.GroupConceptNo, []);
                    }
                    masterData.PRollQty = 0;

                    //GetRollWeightByURL();
                    //getRealTimeWeight();
                    //weight_timer = 1;
                    //myInterval = setInterval(getRealTimeWeight, 2500);

                } else {
                    return toastr.error("Knitting is not confirmed or Invalid job card ");
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getIP(isSetScale) {
        //
        //getClientIP();
        if (!isSetScale) {
            scaleWeight = "";
            scaleOperator = "";
            weightID = "";
            scaleID = "";
            _printerName = "";
        } else {
            $.ajax({
                type: 'GET',
                url: 'http://127.0.0.1:17080/?op=getip',
                crossDomain: true,
                data: '',
                dataType: 'text'
            }).done(function (responseData) {
                //
                console.log(responseData);
                if (responseData != 'N') {
                    var ip = responseData;
                    if (ip != '' && ip != null) {
                        var c = ip.replaceAll('.', '-');
                        clientIP = c;
                        ip = c + "_" + weightID + "_" + scaleID;
                    }
                    var url = `/api/rnd-knitting-production/get-knitting-scale/${ip}`;
                    axios.get(url)
                        .then(function (response) {
                            if (response.data) {
                                if (response.data == "Configuration not found") {
                                    return toastr.error(response.data);
                                } else {
                                    //
                                    response.data = JSON.parse(response.data);
                                    if (response.data.ScaleAddress != '') {
                                        var scaleAddress = response.data.ScaleAddress;
                                        var scID = response.data.ScaleMappingID;
                                        var wID = response.data.WeightID;
                                        _printerName = response.data.PrinterName;
                                        GetWeightByURL(scaleAddress, scID, wID);
                                    }
                                }
                            } else {
                                return toastr.error("Invalid weight & operator");
                            }
                        })
                        .catch(function (err) {
                            toastr.error(err.response.data.Message);
                        });
                }
            }).fail(function (responseData) {
                console.log(responseData);
            });

        }
    }
    function getClientIP() {
        var url = `/api/rnd-knitting-production/get-ip`;
        axios.get(url)
            .then(function (response) {
                if (response.data) {
                    alert('Client Ip :' + response.data);
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });

    }
    function pdfprint() {
        reportId = 1255;
        loadReportInformation();
        var requiredFilterSets = columnValues.filter(function (el) { return el.Caption == "***" });
        var isValid = true;
        $.each(requiredFilterSets, function (i, cv) {
            if (!cv.ColumnValue) {
                isValid = false;
                toastr.error(cv.ColumnName + " is required.");
            }
        });

        if (!isValid) return;

        var params = $.param({ ReportId: reportId, FilterSetList: JSON.stringify(columnValues) });
        var pdf = rootPath + "/reports/pdfview?" + params;
        // Create an IFrame.
        var iframe = document.createElement('iframe');
        // Hide the IFrame.  
        iframe.style.visibility = "hidden";
        // Define the source.  
        iframe.src = pdf;
        // Add the IFrame to the web page.
        document.body.appendChild(iframe);
        iframe.contentWindow.focus();
        iframe.contentWindow.print(); // Print.
    }
    function initJobCardChilds(records) {
        if ($tblChildElJobCardChild) $tblChildElJobCardChild.destroy();
        ej.base.enableRipple(true);
        $tblChildElJobCardChild = new ej.grids.Grid({
            dataSource: records,
            recordClick: function (args) {

                selectedkJobChild = args.rowData;
            },
            columns: [
                { field: 'KJobCardChildID', headerText: 'KJobCardChildID', visible: false, width: 15 },
                { field: 'Composition', headerText: 'Composition', width: 20, visible: masterData.SubGroupID == 1 },
                { field: 'GSM', headerText: 'GSM', width: 20, visible: masterData.SubGroupID == 1 },
                { field: 'ColorName', headerText: 'Color', width: 20 },
                { field: 'Size', headerText: 'Size', width: 20, visible: masterData.SubGroupID != 1 },
                { field: 'ProdQty', headerText: 'Qty (Kg)', width: 20 },
                { field: 'ProdQtyPcs', headerText: 'Qty (Pcs)', width: 20, visible: masterData.SubGroupID != 1 }
            ]
        });
        $tblChildElJobCardChild.refreshColumns;
        $tblChildElJobCardChild.appendTo(tblKJChildId);

        if (records.length == 0) {
            $formEl.find(tblKJChildId).fadeOut();
        } else {
            $formEl.find(tblKJChildId).fadeIn();
        }
    }

})();
(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $formEl, tblMasterId;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        filter: '',
        sort: '',
        order: ''
    }
    var status;

    var GrayFabricQC;
    var GreyQCDefectHKs = [];
    var validSpinnerFieldIds = ["#Hole", "#Loop", "#SetOff", "#LycraOut", "#LycraDrop",
        "#OilSpot", "#Slub", "#FlyingDust", "#MissingYarn", "#Knot", "#DropStitch3",
        "#YarnContra", "#NeddleBreakage", "#Defected"];

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
        status = statusConstants.PENDING;
        initMasterTable();

        $formEl.find(".inputSpinner").inputSpinner({
            decrementButtonColorClass: "btn-success",
            incrementButtonColorClass: "btn-danger",
        });

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            initMasterTable();
        });

        $toolbarEl.find("#btnQCPass").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            initMasterTable();
        });

        $toolbarEl.find("#btnQCFail").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.REVISE;
            initMasterTable();
        });

        $toolbarEl.find("#btnQCHold").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.REJECT;
            initMasterTable();
        });

        $formEl.find("#btnPass").on("click", function (e) {
            showBootboxConfirm("Edit Record.", "Are you sure want to pass?", function (yes) {
                if (yes) {
                    GrayFabricQC.QCPass = 1;
                    GrayFabricQC.Hold = 0;
                    GrayFabricQC.QCFail = 0;
                    $formEl.find("#QCPass").prop('checked', true);
                    $formEl.find("#Hold").prop('checked', false);
                    $formEl.find("#QCFail").prop('checked', false);
                    $formEl.find("#PassHold").fadeIn();
                }
            });
        });

        $formEl.find("#btnFail").on("click", function (e) {
            showBootboxConfirm("Edit Record.", "Are you sure want to fail?", function (yes) {
                if (yes) {
                    GrayFabricQC.QCPass = 0;
                    GrayFabricQC.QCFail = 1;
                    GrayFabricQC.Hold = 0;
                    $formEl.find("#QCPass").prop('checked', false);
                    $formEl.find("#Hold").prop('checked', false);
                    $formEl.find("#QCFail").prop('checked', true);
                    $formEl.find("#PassHold").fadeIn();
                }
            });
        });

        $formEl.find("#btnHold").on("click", function (e) {
            showBootboxConfirm("Edit Record.", "Are you sure want to hold?", function (yes) {
                if (yes) {
                    GrayFabricQC.QCPass = 0;
                    GrayFabricQC.QCFail = 0;
                    GrayFabricQC.Hold = 1;
                    $formEl.find("#QCPass").prop('checked', false);
                    $formEl.find("#Hold").prop('checked', true);
                    $formEl.find("#QCFail").prop('checked', false);
                    $formEl.find("#PassHold").fadeIn();
                }
            });
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnCancel").on("click", backToList);

        //$formEl.find("#Hole,#Loop,#SetOff,#LycraOut,#LycraDrop,#OilSpot,#Slub,#FlyingDust,#MissingYarn,#Knot,#DropStitch,#YarnContra,#NeddleBreakage,#Defected").keyup(function () {
        //    CalculateTotalPoint();
        //});

        $formEl.find(".inputSpinner").keyup(function () {
            var inputFieldId = $(this).parent().find("input").attr("id");
            if (IskeyUpValidFieldIds(inputFieldId)) {
                CalculateTotalPoint();
            }
        });

        $formEl.find("#WrongDesign,#Patta,#ShinkerMark,#NeddleMark,#EdgeMark,#WheelFree,#CountMix,#ThickAndThin,#LineStar,#CalculateQCStatus").click(function () {
            CalculateTotalPoint();
        });

        //GreyQCDefectHKs
        axios.get(`/api/grey-qc-defect-hk/list`)
            .then(function (response) {
                GreyQCDefectHKs = response.data;
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });

    });
    function IskeyUpValidFieldIds(fieldId) {
        if (typeof fieldId === "undefined" || fieldId == null) return false;
        fieldId = "#" + fieldId.split("_")[0]; //spinner creates it's own id with formatter ourGivenId_Spinner
        return validSpinnerFieldIds.includes(fieldId);
    }
    function GetValue(inputFieldId) {
        var value = $formEl.find("#" + inputFieldId).val();
        value = (isNaN(parseFloat(value)) ? 0 : parseFloat(value));
        value = value * GetPointPerDefect(inputFieldId);
        if (value > 0) {
            $formEl.find("#spn" + inputFieldId).show();
        } else {
            $formEl.find("#spn" + inputFieldId).hide();
        }
        $formEl.find("#spn" + inputFieldId).text("(" + value + ")");
        return value;
    }
    function GetPointPerDefect(inputFieldId) {
        var qcDefectID = 0;
        switch (inputFieldId) {
            case "Hole": qcDefectID = 1; break;
            case "Loop": qcDefectID = 2; break;
            case "SetOff": qcDefectID = 3; break;
            case "LycraOut": qcDefectID = 4; break;
            case "LycraDrop": qcDefectID = 5; break;
            case "OilSpot": qcDefectID = 9; break;
            case "Slub": qcDefectID = 13; break;
            case "FlyingDust": qcDefectID = 14; break;
            case "MissingYarn": qcDefectID = 15; break;
            case "Knot": qcDefectID = 16; break;
            case "DropStitch3": qcDefectID = 20; break;
            case "YarnContra": qcDefectID = 21; break;
            case "NeddleBreakage": qcDefectID = 22; break;
            default: qcDefectID = 0; break;
        }
        var pointPerDefect = qcDefectID > 0 ? GreyQCDefectHKs.find(x => x.QCDefectID == qcDefectID).PointPerDefect : 0;
        return pointPerDefect;
    }
    function CalculateTotalPoint() {
        $formEl.find(".spnSpinnerCount").hide();
        var totalPoint = GetValue("Hole") + GetValue("Loop") + GetValue("SetOff") + GetValue("LycraOut") + GetValue("LycraDrop")
            + GetValue("OilSpot") + GetValue("Slub") + GetValue("FlyingDust") + GetValue("MissingYarn") + GetValue("Knot")
            + GetValue("DropStitch3") + GetValue("YarnContra") + GetValue("NeddleBreakage") + GetValue("Defected");

        $formEl.find("#TotalPoint").val(totalPoint);

        GrayFabricQC.CalculateValue = getValueForGrade(totalPoint, parseFloat(GrayFabricQC.RollLength), GrayFabricQC.ProductionWidth);
        GrayFabricQC.CalculateValue = GrayFabricQC.CalculateValue.toFixed(2);
        $formEl.find("#CalculateValue").val(GrayFabricQC.CalculateValue);
        GrayFabricQC.Grade = getGradeFromValue(GrayFabricQC.CalculateValue)
        $formEl.find("#Grade").val(GrayFabricQC.Grade);
        $formEl.find("#msgLabel").text(GrayFabricQC.Grade);
        $formEl.find("#btnPass").fadeIn();
        $formEl.find("#btnFail").fadeIn();
        $formEl.find("#btnHold").fadeIn();
        if (GrayFabricQC.Grade == 'C (Fail)' || GrayFabricQC.Grade == 'No Grade') {
            document.getElementById('msgLabel').style.color = "red";
        }
        else {
            document.getElementById('msgLabel').style.color = "black";
        }

        if ((document.getElementById('WrongDesign').checked) || (document.getElementById('Patta').checked) || (document.getElementById('ShinkerMark').checked) ||
            (document.getElementById('NeddleMark').checked) || (document.getElementById('EdgeMark').checked) || (document.getElementById('WheelFree').checked) ||
            (document.getElementById('CountMix').checked) || (document.getElementById('ThickAndThin').checked) || (document.getElementById('LineStar').checked) ||
            (document.getElementById('CalculateQCStatus').checked)) {
            GrayFabricQC.Grade = 'C (Fail)';
            $formEl.find("#Grade").val(GrayFabricQC.Grade);
            $formEl.find("#msgLabel").text(GrayFabricQC.Grade);
            if (GrayFabricQC.Grade == 'C (Fail)' || GrayFabricQC.Grade == 'No Grade') {
                document.getElementById('msgLabel').style.color = "red";
            }
            else {
                document.getElementById('msgLabel').style.color = "black";
            }
            $formEl.find("#QCPass").prop('checked', false);
            $formEl.find("#QCFail").prop('checked', false);
            $formEl.find("#Hold").prop('checked', false);
            $formEl.find("#PassHold").fadeIn();
            $formEl.find("#btnPass").fadeIn();
            $formEl.find("#btnFail").fadeIn();
            $formEl.find("#btnHold").fadeIn();
        }
    }

    function initMasterTable() {
        var commands = status != statusConstants.COMPLETED
            ? [{ type: 'Add', title: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }]
            : [{ type: 'Edit', title: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }];

        var columns = [
            {
                headerText: '', commands: commands, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'ProductionDate', headerText: 'Production Date', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'RollSeqNo', headerText: 'Roll Seq. No'
            },
            {
                field: 'JobCardNo', headerText: 'Job Card No'
            },
            {
                field: 'JobCardDate', headerText: 'Job Card Date', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'ConceptNo', headerText: 'Concept No'
            },
            {
                field: 'MachineNo', headerText: 'Machine No'
            },
            {
                field: 'Dia', headerText: 'Dia'
            },
            {
                field: 'Gauge', headerText: 'Gauge'
            },
            {
                field: 'TechnicalName', headerText: 'Technical Name'
            },
            {
                field: 'ShiftName', headerText: 'Shift'
            },
            {
                field: 'OperatorName', headerText: 'Operator'
            },
            {
                field: 'ProdQty', headerText: 'Weight(Kg)'
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/gray-fabric-qc/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'Add')
            getDetails(args.rowData.GRollID);
        else {
            getDetails(args.rowData.GRollID);
        }
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
        $formEl.find("#msgLabel").text('');
        $formEl.find("#btnPass").fadeOut();
        $formEl.find("#btnFail").fadeOut();
        $formEl.find("#btnHold").fadeOut();
        $formEl.find("#PassHold").fadeOut();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#GRollId").val(-1111);
        $formEl.find("#EntityState").val(4);
        $formEl.find("#msgLabel").val('');
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getDetails(id) {
        axios.get(`/api/gray-fabric-qc/${id}`)
            .then(function (response) {

                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                GrayFabricQC = response.data;
                GrayFabricQC.ProductionDate = formatDateToDefault(GrayFabricQC.ProductionDate);
                GrayFabricQC.SendQCDate = formatDateToDefault(GrayFabricQC.SendQCDate);
                GrayFabricQC.RollLength = GrayFabricQC.ProductionWidth == 0 || GrayFabricQC.ProductionGSM == 0 ? 0 : ((GrayFabricQC.ProdQty * 39370) / (GrayFabricQC.ProductionWidth * GrayFabricQC.ProductionGSM)) * 1.094;
                GrayFabricQC.RollLength = isNaN(GrayFabricQC.RollLength) ? 0 : GrayFabricQC.RollLength.toFixed(2);
                GrayFabricQC.QCShiftID = GrayFabricQC.QCShiftList.find(x => x.text == GrayFabricQC.ShiftName).id;
                setFormData($formEl, GrayFabricQC);
                displaySpinnerValues(GrayFabricQC);
                CalculateTotalPoint();

                if (GrayFabricQC.QCPass) $formEl.find("#btnSave").fadeOut();
                else $formEl.find("#btnSave").fadeIn();

                if (status != statusConstants.PENDING) {
                    $formEl.find("#btnPass").fadeIn();
                    $formEl.find("#btnFail").fadeIn();
                    $formEl.find("#btnHold").fadeIn();
                    $formEl.find("#PassHold").fadeIn();
                    if (GrayFabricQC.QCPass) {
                        $formEl.find("#QCPass").prop('checked', true);
                        $formEl.find("#Hold").prop('checked', false);
                        $formEl.find("#QCFail").prop('checked', false);
                    } else if (GrayFabricQC.QCFail) {
                        $formEl.find("#QCPass").prop('checked', false);
                        $formEl.find("#Hold").prop('checked', false);
                        $formEl.find("#QCFail").prop('checked', true);
                    } else if (GrayFabricQC.Hold) {
                        $formEl.find("#QCPass").prop('checked', false);
                        $formEl.find("#Hold").prop('checked', true);
                        $formEl.find("#QCFail").prop('checked', false);
                    }

                    $formEl.find("#msgLabel").text(GrayFabricQC.Grade);
                    if (GrayFabricQC.Grade == 'C (Fail)' || GrayFabricQC.Grade == 'No Grade') {
                        document.getElementById('msgLabel').style.color = "red";
                    }
                    else {
                        document.getElementById('msgLabel').style.color = "black";
                    }
                }
                else if (status == statusConstants.PENDING) {
                    if (!GrayFabricQC.QCPass && !GrayFabricQC.Hold && !GrayFabricQC.QCFail) {
                        GrayFabricQC.Grade = "A (Pass)";
                        $formEl.find("#msgLabel").fadeIn();
                        $formEl.find("#msgLabel").text(GrayFabricQC.Grade);
                        $formEl.find("#btnPass").fadeIn();
                        $formEl.find("#btnFail").fadeIn();
                        $formEl.find("#btnHold").fadeIn();
                        document.getElementById('msgLabel').style.color = "black";
                        $formEl.find("#PassHold").fadeIn();
                        $formEl.find("#QCPass").prop('checked', true);
                        $formEl.find("#Hold").prop('checked', false);
                        $formEl.find("#QCFail").prop('checked', false);
                    }
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function displaySpinnerValues(grayFabricQC) {
        //_Spinner
        validSpinnerFieldIds.map(x => {
            $formEl.find(x + "_Spinner").val(grayFabricQC[x.substring(1)]);
        });
    }
    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data.WrongDesign = $formEl.find("#WrongDesign").is(':checked');
        data.Patta = $formEl.find("#Patta").is(':checked');
        data.ShinkerMark = $formEl.find("#ShinkerMark").is(':checked');
        data.NeddleMark = $formEl.find("#NeddleMark").is(':checked');
        data.EdgeMark = $formEl.find("#EdgeMark").is(':checked');
        data.WheelFree = $formEl.find("#WheelFree").is(':checked');
        data.CountMix = $formEl.find("#CountMix").is(':checked');
        data.ThickAndThin = $formEl.find("#ThickAndThin").is(':checked');
        data.LineStar = $formEl.find("#LineStar").is(':checked');
        data.CalculateQCStatus = $formEl.find("#CalculateQCStatus").is(':checked');
        data.QCPass = $formEl.find("#QCPass").is(':checked');
        data.QCFail = $formEl.find("#QCFail").is(':checked');
        data.Hold = $formEl.find("#Hold").is(':checked');
        //console.log(data);
        axios.post("/api/gray-fabric-qc/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();
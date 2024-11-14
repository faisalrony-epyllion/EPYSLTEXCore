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

    var masterData;
    var validSpinnerFieldIds = ["#Holes", "#OilMark", "#DyeStain", "#RubMark", "#DirtySpot",
        "#ChemicalSpot", "#BandLine", "#NeedleBroken", "#ContaFly", "#Slub"];

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
                    masterData.QCPass = 1;
                    masterData.QCHold = 0;
                    masterData.QCFail = 0;
                    $formEl.find("#QCPass").prop('checked', true);
                    $formEl.find("#QCHold").prop('checked', false);
                    $formEl.find("#QCFail").prop('checked', false);
                    $formEl.find("#PassHold").fadeIn();
                }
            });
        });

        $formEl.find("#btnFail").on("click", function (e) {
            showBootboxConfirm("Edit Record.", "Are you sure want to fail?", function (yes) {
                if (yes) {
                    masterData.QCPass = 0;
                    masterData.QCFail = 1;
                    masterData.QCHold = 0;
                    $formEl.find("#QCPass").prop('checked', false);
                    $formEl.find("#QCHold").prop('checked', false);
                    $formEl.find("#QCFail").prop('checked', true);
                    $formEl.find("#PassHold").fadeIn();
                }
            });
        });

        $formEl.find("#btnHold").on("click", function (e) {
            showBootboxConfirm("Edit Record.", "Are you sure want to hold?", function (yes) {
                if (yes) {
                    masterData.QCPass = 0;
                    masterData.QCFail = 0;
                    masterData.QCHold = 1;
                    $formEl.find("#QCPass").prop('checked', false);
                    $formEl.find("#QCHold").prop('checked', true);
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

        $formEl.find(".inputSpinner").keyup(function () {
            var inputFieldId = $(this).parent().find("input").attr("id");
            if (IskeyUpValidFieldIds(inputFieldId)) {
                CalculateTotalPoint();
            }
        });

        $formEl.find("#Creases,#HairyDC,#TT,#Neps,#BarreMark").click(function () {
            CalculateTotalPoint();
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
            case "Holes": qcDefectID = 1; break;
            case "OilMark": qcDefectID = 2; break;
            case "DyeStain": qcDefectID = 6; break;
            case "RubMark": qcDefectID = 10; break;
            case "DirtySpot": qcDefectID = 14; break;
            case "ChemicalSpot": qcDefectID = 18; break;
            case "BandLine": qcDefectID = 22; break;
            case "NeedleBroken": qcDefectID = 26; break;
            case "ContaFly": qcDefectID = 30; break;
            case "Slub": qcDefectID = 31; break;
            default: qcDefectID = 0; break;
        }
        var pointPerDefect = qcDefectID > 0 ? masterData.FinishFabricQCDefect_HKs.find(x => x.QCDefectID == qcDefectID).PointPerDefect : 0;
        return pointPerDefect;
    }
    function CalculateTotalPoint() {
        $formEl.find(".spnSpinnerCount").hide();
        var totalPoint = GetValue("Holes") + GetValue("OilMark") + GetValue("DyeStain") + GetValue("RubMark") + GetValue("DirtySpot")
            + GetValue("ChemicalSpot") + GetValue("BandLine") + GetValue("NeedleBroken") + GetValue("ContaFly") + GetValue("Slub");

        $formEl.find("#TotalPoint").val(totalPoint);

        masterData.TotalValue = getValueForGrade(totalPoint, parseFloat(masterData.RollLength), masterData.ProductionWidth);//********************//
        masterData.TotalValue = masterData.TotalValue.toFixed(2);
        $formEl.find("#TotalValue").val(masterData.TotalValue);
        masterData.Grade = getGradeFromValue(masterData.TotalValue)
        $formEl.find("#Grade").val(masterData.Grade);
        $formEl.find("#msgLabel").text(masterData.Grade);
        $formEl.find("#btnPass").fadeIn();
        $formEl.find("#btnFail").fadeIn();
        $formEl.find("#btnHold").fadeIn();
        if (masterData.Grade == 'C (Fail)' || masterData.Grade == 'No Grade') {
            document.getElementById('msgLabel').style.color = "red";
        }
        else {
            document.getElementById('msgLabel').style.color = "black";
        }

        if ((document.getElementById('Creases').checked) || (document.getElementById('HairyDC').checked) || (document.getElementById('TT').checked) ||
            (document.getElementById('Neps').checked) || (document.getElementById('BarreMark').checked)) {
            masterData.Grade = 'C (Fail)';
            $formEl.find("#Grade").val(masterData.Grade);
            $formEl.find("#msgLabel").text(masterData.Grade);
            if (masterData.Grade == 'C (Fail)' || masterData.Grade == 'No Grade') {
                document.getElementById('msgLabel').style.color = "red";
            }
            else {
                document.getElementById('msgLabel').style.color = "black";
            }
            $formEl.find("#QCPass").prop('checked', false);
            $formEl.find("#QCFail").prop('checked', false);
            $formEl.find("#QCHold").prop('checked', false);
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
                headerText: '', commands: commands, textAlign: 'Center', headerTextAlign: 'Center', width: 10
            },
            {
                field: 'DBatchNo', headerText: 'D. Batch No', width: 20
            },
            {
                field: 'DBatchDate', headerText: 'D. Batch Date', type: 'date', format: _ch_date_format_1, width: 20
            },
            {
                field: 'RollNo', headerText: 'Roll No', width: 20
            },
            {
                field: 'ProductionDate', headerText: 'Production Date', type: 'date', format: _ch_date_format_1, width: 20
            },
            {
                field: 'ConceptNo', headerText: 'Concept No', width: 20
            },
            {
                field: 'ColorName', headerText: 'Color', width: 20
            },
            {
                field: 'RollQty', headerText: 'Roll Qty (Kg)', width: 15
            },
            {
                field: 'RollQtyPcs', headerText: 'Roll Qty (Pcs)', width: 15
            },

        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/fabric-final-qc/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'Add')
            getDetails(args.rowData.DBIRollID);
        else {
            getDetails(args.rowData.DBIRollID);
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
        axios.get(`/api/fabric-final-qc/${id}`)
            .then(function (response) {

                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;

                masterData.RollLength = masterData.ProductionWidth == 0 || masterData.ProductionGSM == 0 ? 0 : ((masterData.ProdQty * 39370) / (masterData.ProductionWidth * masterData.ProductionGSM)) * 1.094;
                masterData.RollLength = isNaN(masterData.RollLength) ? 0 : masterData.RollLength.toFixed(2);

                setFormData($formEl, masterData);
                displaySpinnerValues(masterData);
                CalculateTotalPoint();

                if (masterData.QCPass) $formEl.find("#btnSave").fadeOut();
                else $formEl.find("#btnSave").fadeIn();

                if (status != statusConstants.PENDING) {
                    $formEl.find("#btnPass").fadeIn();
                    $formEl.find("#btnFail").fadeIn();
                    $formEl.find("#btnHold").fadeIn();
                    $formEl.find("#PassHold").fadeIn();
                    if (masterData.QCPass) {
                        $formEl.find("#QCPass").prop('checked', true);
                        $formEl.find("#QCHold").prop('checked', false);
                        $formEl.find("#QCFail").prop('checked', false);
                    } else if (masterData.QCFail) {
                        $formEl.find("#QCPass").prop('checked', false);
                        $formEl.find("#QCHold").prop('checked', false);
                        $formEl.find("#QCFail").prop('checked', true);
                    } else if (masterData.QCHold) {
                        $formEl.find("#QCPass").prop('checked', false);
                        $formEl.find("#QCHold").prop('checked', true);
                        $formEl.find("#QCFail").prop('checked', false);
                    }

                    $formEl.find("#msgLabel").text(masterData.Grade);
                    if (masterData.Grade == 'C (Fail)' || masterData.Grade == 'No Grade') {
                        document.getElementById('msgLabel').style.color = "red";
                    }
                    else {
                        document.getElementById('msgLabel').style.color = "black";
                    }
                }
                else if (status == statusConstants.PENDING) {
                    if (!masterData.QCPass && !masterData.QCHold && !masterData.QCFail) {
                        masterData.Grade = "A (Pass)";
                        $formEl.find("#msgLabel").fadeIn();
                        $formEl.find("#msgLabel").text(masterData.Grade);
                        $formEl.find("#btnPass").fadeIn();
                        $formEl.find("#btnFail").fadeIn();
                        $formEl.find("#btnHold").fadeIn();
                        document.getElementById('msgLabel').style.color = "black";
                        $formEl.find("#PassHold").fadeIn();
                        $formEl.find("#QCPass").prop('checked', true);
                        $formEl.find("#QCHold").prop('checked', false);
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
        //debugger;
        var data = formDataToJson($formEl.serializeArray());

        data.Creases = $formEl.find("#Creases").is(':checked');
        data.HairyDC = $formEl.find("#HairyDC").is(':checked');
        data.TT = $formEl.find("#TT").is(':checked');
        data.Neps = $formEl.find("#Neps").is(':checked');
        data.BarreMark = $formEl.find("#BarreMark").is(':checked');

        data.QCPass = $formEl.find("#QCPass").is(':checked');
        data.QCFail = $formEl.find("#QCFail").is(':checked');
        data.QCHold = $formEl.find("#QCHold").is(':checked');
        
        axios.post("/api/fabric-final-qc/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();
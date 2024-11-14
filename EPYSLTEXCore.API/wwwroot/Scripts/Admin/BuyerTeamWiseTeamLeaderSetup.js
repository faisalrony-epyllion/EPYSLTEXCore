(function () {
    var menuId, pageName;
    var $tblMasterEl, $formEl, tblMasterId;
    var $fiberTypeEl;
    var fiberTypeId, fiberType, isBlendedOrColorMelange;
    var tempBuyerTeamList = [];
    var teamleadersetup, teamleadersetups = [], productSetupChilds = [], productSetupChildPrograms = [], productSetupChildTechnicalParameters = [], ProcessSetupList = [];

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        //$tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        $fiberTypeEl = $formEl.find("#FiberTypeID");
        $formEl.find("#BuyerTeamID").empty().trigger('change')
        getInitData();

        $formEl.find("#btnSave").click(save);
        $formEl.find('#BuyerID').change(function (e) {
            var BuyerID = $(this).val();
            if (BuyerID != null) {
                teamleadersetup.BuyerID = BuyerID;
                teamleadersetup.BuyerTeamList = tempBuyerTeamList;
                teamleadersetup.BuyerTeamList = teamleadersetup.BuyerTeamList.filter(y => y.additionalValue == BuyerID);
                $formEl.find("#BuyerTeamID").empty().trigger('change')
                $formEl.find("#BuyerTeamID").select2({ data: teamleadersetup.BuyerTeamList });

                //$("#ArbiterId").select2('destroy').empty().select2({ data: data.arbiters });


                //$formEl.find("#BuyerTeamID").select2({ 'data': teamleadersetup.BuyerTeamList });
                //$formEl.find("#BuyerTeamID").val(teamleadersetup.BuyerTeamList[0].id).trigger("change");
             
                //$formEl.find("#BuyerTeamID").select2({ 'data': teamleadersetup.BuyerTeamList });
            }
          });
    });

    function loadBuyerTeams(buyerId) {
        setFormData($formEl, teamleadersetup);
    }

    /*function initMasterTable() {
        $tblMasterEl.bootstrapTable('destroy');
        $tblMasterEl.bootstrapTable({
            uniqueId: 'SegmentValueMappingID',
            pagination: true,
            sidePagination: "client",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            editable: true,
            data: segmentFilterSetup.SegmentFilterMappingList,
            columns: [
                {
                    field: "YarnType",
                    title: "Yarn Type"
                },
                {
                    field: "ManufacturingProcess",
                    title: "Manufacturing Process"
                },
                {
                    field: "SubProcess",
                    title: "Sub Process"
                },
                {
                    field: "QualityParameter",
                    title: "Quality Parameter"
                },
                {
                    field: "Count",
                    title: "Count"
                }
            ]
        });
    }*/
    function getInitData() {
        var url = "/api/team-leader-setup/new";
        axios.get(url)
            .then(function (response) {
                teamleadersetup = response.data;
                tempBuyerTeamList = teamleadersetup.BuyerTeamList;
                setFormData($formEl, teamleadersetup);
                initMasterTable();
            })
            .catch(showResponseError)
    }
    function initMasterTable() {
        var commands = [
            { type: 'Edit', title: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-edit' } },
            { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
        ];

        columns = [
            {
                headerText: 'Command', textAlign: 'Center', commands: commands,
                textAlign: 'Center', width: 50
            },

            { field: 'TLAssignID', isPrimaryKey: true, visible: false },
            { field: 'BuyerID', isPrimaryKey: true, visible: false },
            { field: 'BuyerName', headerText: 'Buyer', allowEditing: false, width: 100 },
            //{
            //    field: 'BuyerID', headerText: 'Buyer', allowEditing: false, width: 100, valueAccessor: ej2GridDisplayFormatter, dataSource: teamleadersetup.BuyerList,
            //    displayField: "BuyerName", edit: ej2GridDropDownObj({
            //    })
            //},
            { field: 'BuyerTeamID', isPrimaryKey: true, visible: false },
            { field: 'TeamName', headerText: 'Team Name', allowEditing: false, width: 100 },
            //{
            //    field: 'BuyerTeamID', headerText: 'Buyer Team', allowEditing: false, width: 100, valueAccessor: ej2GridDisplayFormatter, dataSource: teamleadersetup.BuyerTeamList,
            //    displayField: "TeamName", edit: ej2GridDropDownObj({
            //    })
            //},
            {
                field: 'TeamLeaderEmployeeCode', headerText: 'Team Leader', allowEditing: true, width: 100, valueAccessor: ej2GridDisplayFormatter, dataSource: teamleadersetup.TeamLeaderEmployeeCodeList,
                displayField: "TeamLeaderName", edit: ej2GridDropDownObj({
                })
            },
        ];
        if ($tblMasterEl) $tblMasterEl.destroy();

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            allowFiltering: true,
            editSettings: {
                allowEditing: true,
                allowAdding: false,
                allowDeleting: true,
                mode: "Normal",
                showDeleteConfirmDialog: true
            },
            apiEndPoint: `/api/team-leader-setup/list`,
            columns: columns,
            commandClick: handleCommands,

            actionBegin: function (args) {
                debugger;
                if (args.requestType === "beginEdit") {
                  }
                else if (args.requestType === "save") {
                    /*var data = {
                        SubClassID:args.data.SubClassID,
                        SubClassName:args.data.SubClassName,
                        TypeID:args.data.TypeID,
                        ShortCode:args.data.ShortCode,
                        isModified:true
                    }

                    axios.post("/api/kmachine-subclass/save", data)
                        .then(function () {
                            toastr.success("Saved successfully!");
                            getInitData();
                        })
                        .catch(showResponseError);*/
                }
            },
            actionComplete: function (args) {
            }
        });
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'Add') {
            //getNew(args.rowData.SalesInvoiceMasterID);
        }
        else if (args.commandColumn.type == 'Edit') {
            //getDetails(args.rowData.SalesInvoiceMasterID);
        }
        else if (args.commandColumn.type == 'Delete') {
            //getDetails(args.rowData.SalesInvoiceMasterID);
        }
    }
    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        if (status === statusConstants.NEW) {
            status = statusConstants.EDIT;
            toggleActiveToolbarBtn("#btnEdit", $toolbarEl);
        }
        initMasterTable();
    }
    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#SalesInvoiceMasterID").val(-1111);
    }
    function save(e) {
        debugger;
        e.preventDefault();
        var data = formElToJson($formEl);
        //data.TeamLeaderName = $formEl.find('#TeamLeaderEmployeeCode option:selected').text();
        axios.post("/api/team-leader-setup/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                getInitData();
                resetForm();
            })
            .catch(showResponseError);
    }

})();
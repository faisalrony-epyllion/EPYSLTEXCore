(function () {
    var menuId, pageName;
    var $tblMasterEl, $formEl, tblMasterId;
    var $fiberTypeEl;
    var fiberTypeId, fiberType, isBlendedOrColorMelange;

    var fabricTechnicalName, fabricTechnicalNames = [], productSetupChilds = [], productSetupChildPrograms = [], productSetupChildTechnicalParameters = [], ProcessSetupList = [];

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        //$tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;

        getInitData();

        $formEl.find("#btnSave").click(save);
    });

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
        var url = "/api/fabric-technical-name/new";
        axios.get(url)
            .then(function (response) {
                fabricTechnicalName = response.data;
                setFormData($formEl, fabricTechnicalName);
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
                textAlign: 'Center', width: 30
            },

            { field: 'TechnicalNameId', isPrimaryKey: true, visible: false },
            { field: 'TechnicalName', headerText: 'Technical Name', width: 35},
            {
                field: 'ConstructionId',
                headerText: 'Construction',
                width:35,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: fabricTechnicalName.ConstructionList,
                displayField: "ConstructionName",
                edit: ej2GridDropDownObj({
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
            apiEndPoint: `/api/fabric-technical-name/list`,
            columns: columns,
            commandClick: handleCommands,

            actionBegin: function (args) {
                if (args.requestType === "save") {
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
        data.ConstructionName = fabricTechnicalName.ConstructionList.find(y => y.id == data.ConstructionId).text
        axios.post("/api/fabric-technical-name/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                getInitData();
            })
            .catch(showResponseError);
    }

})();
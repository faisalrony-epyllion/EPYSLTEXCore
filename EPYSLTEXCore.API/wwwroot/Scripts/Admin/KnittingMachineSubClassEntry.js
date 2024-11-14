(function () {
    var menuId, pageName;
    var $tblMasterEl, $formEl, tblMasterId;
    var $fiberTypeEl;
    var fiberTypeId, fiberType, isBlendedOrColorMelange;

    var kMachineSubClass, kMachineSubClasses = [], productSetupChilds = [], productSetupChildPrograms = [], productSetupChildTechnicalParameters = [], ProcessSetupList = [];

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        //$tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        $fiberTypeEl = $formEl.find("#FiberTypeID");

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
        var url = "/api/kmachine-subclass/new";
        axios.get(url)
            .then(function (response) {
                kMachineSubClass = response.data;
                setFormData($formEl, kMachineSubClass);
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
                textAlign: 'Center', width: 10
            },

            { field: 'SubClassID', isPrimaryKey: true, visible: false },
            { field: 'TypeID', headerText: 'TypeID', width: 20 , visible: false },
            { field: 'SubClassName', headerText: 'Sub Class', width: 30 },
            { field: 'ShortCode', headerText: 'ShortCode', width: 30 },
            { field: 'TypeName', headerText: 'Type Name', width: 30, allowEditing: false },
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
            apiEndPoint: `/api/kmachine-subclass/list`,
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
        e.preventDefault();
        var data = formElToJson($formEl);
        
        axios.post("/api/kmachine-subclass/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                getInitData();
            })
            .catch(showResponseError);
    }

})();
(function () {
    var menuId, pageName;
    var $tblMasterEl, $formEl, tblMasterId;
    var filterSetup, filterSetups = []

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
    /*
    function initMasterTable() {
        $tblMasterEl.bootstrapTable('destroy');
        $tblMasterEl.bootstrapTable({
            uniqueId: 'SetupID',
            pagination: true,
            sidePagination: "client",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            editable: true,
            data: filterSetup.SegmentFilterMappingList,
            columns: [
                {
                    field: "Fiber",
                    title: "Fiber"
                },
                {
                    field: "SubProgram",
                    title: "Sub Program"
                },
                {
                    field: "Certifications",
                    title: "Certifications"
                }
            ]
        });
    }*/
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

            { field: 'SetupID', isPrimaryKey: true, visible: false },
            //{ field: 'Fiber', headerText: 'Fiber', width: 35 },
            //{ field: 'SubProgram', headerText: 'SubProgram', width: 35 },
            //{ field: 'Certifications', headerText: 'Certifications', width: 35 },
            {
                field: 'FiberID',
                headerText: 'Fiber',
                width: 35,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: filterSetup.FiberList,
                displayField: "Fiber",
                edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'SubProgramID',
                headerText: 'SubProgram',
                width: 35,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: filterSetup.SubProgramList,
                displayField: "SubProgram",
                edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'CertificationsID',
                headerText: 'Certifications',
                width: 35,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: filterSetup.CertificationsList,
                displayField: "Certifications",
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
            apiEndPoint: `/api/fiber-subProgram-certifications-filter-setup/list`,
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
    function getInitData() {

        var url = "/api/items/yarn/fiber-subprogram-certifications";
        axios.get(url)
            .then(function (response) {
                filterSetup = response.data; 
               
                setFormData($formEl, filterSetup);
                initMasterTable();
            })
            .catch(showResponseError)
    }


    function save(e) {
        e.preventDefault();
        var data = formElToJson($formEl);
        data.SetupID = 0;
        
        axios.post("/api/fiber-subProgram-certifications-filter-setup/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                getInitData();
            })
            .catch(showResponseError);
    }

})();
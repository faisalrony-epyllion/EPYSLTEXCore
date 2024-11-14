(function () {
    var menuId, pageName;
    var toolbarId;
    //var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId,
        $formEl, $tblChildEl, tblChildId;
    var isFlatKnit = false;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }

    var masterData = {};

    /** use to filter data for select options */
    var filterData = {};

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");
        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        //$tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        initMasterTable();

        //getMasterTableData();

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            initMasterTable();
            //getMasterTableData();
        });

        $formEl.find("#btnViewEmployeeName").on("click", function (e) {
            e.preventDefault();
            var groupList = [];
            var empCode = $.trim($formEl.find("#EmployeeCode").val());
            //var api = "/api/userinfo/getEmployeeList/" + empCode + "";
            var api = "/api/userinfo/getEmployeeList2/";
            var finder = new commonFinder({
                title: "Employee List",
                pageId: pageId,
                height: 220,
                modalSize: "modal-lg",
                apiEndPoint: api,
                fields: "DisplayEmployeeCode,EmployeeName,Designation,Email,DepertmentDescription,SectionName,CompanyName,EmployeeStatusName",
                widths: "20,20,20,20,20,20,20,20",
                headerTexts: "Employee Code,Employee Name,Designation,Email,Department,Section,Company,Employee Status",
                isMultiselect: false,
                primaryKeyColumn: "id",
                allowPaging: false,
                //selectedIds: "1",
                //editTypes: ",string",
                //allowEditing: true,
                autofitColumns: true,
                onSelect: function (selectedRecord) {
                    setFormData($formEl, masterData);
                    $formEl.find("#EmployeeCode").val(selectedRecord.rowData.EmployeeName);
                    $formEl.find("#EmployeeId").val(selectedRecord.rowData.id);
                    finder.hideModal();
                    //if (selectedRecord.rowData.id > 0) {
                    //    axios.get(`/api/userinfo/getGroupList/${selectedRecord.rowData.id}`)
                    //        .then(function (response) {
                    //            initChildTable(response.data.Items);
                    //        })
                    //        .catch(function (err) {
                    //            toastr.error(err.response.data.Message);
                    //        });
                    //}
                    
                }
            });
            finder.showModal();
            
            
        }

        );
        $formEl.find("#btnViewUserName").on("click", function (e) {
            e.preventDefault();
            var groupList = [];
            //var empCode = $.trim($formEl.find("#EmployeeCode").val());
            var api = "/api/userinfo/GetAllLoginUser2";
            var finder = new commonFinder({
                title: "User List",
                pageId: pageId,
                height: 220,
                modalSize: "modal-lg",
                apiEndPoint: api,
                fields: "Name,UserName,EmployeeName,Designation,Email,DepertmentDescription,SectionName,DisplayEmployeeCode",
                widths: "20,20,20,20,20,20,20,20",
                headerTexts: "Display Name,User Name,Employee Name,Designation,Email,Department,Section Name,Employee Code",
                isMultiselect: false,
                primaryKeyColumn: "id",
                allowPaging: false,
                //selectedIds: "1",
                //editTypes: ",string",
                //allowEditing: true,
                autofitColumns: true,
                onSelect: function (selectedRecord) {
                    $formEl.find("#UserName").val(selectedRecord.rowData.UserName);
                    //console.log(selectedRecord);
                    //$formEl.find("#EmployeeId").val(selectedRecord.rowData.id);
                    finder.hideModal();
                    if (selectedRecord.rowData.id > 0) {
                        axios.get(`/api/userinfo/getGroupList/${selectedRecord.rowData.id}`)
                            .then(function (response) {
                                initChildTable(response.data.Items);
                            })
                            .catch(function (err) {
                                toastr.error(err.response.data.Message);
                            });

                            axios.get(`/api/userinfo/${selectedRecord.rowData.id}`)
                                .then(function (response) {

                                    $divDetailsEl.fadeIn();
                                    $divTblEl.fadeOut();
                                    masterData = response.data;
                                    filterData = _.clone(masterData);
                                    
                                    setFormData($formEl, masterData);
                                    $formEl.find("#EmployeeCode").val(selectedRecord.rowData.EmployeeName);
                                    //$formEl.find("#EmployeeId").val(masterData.EmployeeCode);
                                    //$tagId.attr("disabled", "disabled");
                                    //$formEl.find("#UserName").attr("disabled", "disabled");
                                    //var name = $formEl.find("#Name").val();
                                    //$formEl.find("#EmployeeCode").val(masterData.Name);
                                    $formEl.find("#Password2").val(masterData.Password);
                                    $formEl.find("#EmployeeCode").attr("disabled", "disabled");
                                    //initChildTable();
                                    //$tblChildEl.bootstrapTable("load", masterData.KnittingMachineOptions);
                                })
                                .catch(showResponseError);
                        


                    }

                }
            });
            finder.showModal();


        }

        );
        //$formEl.find("#EmployeeCode").on("click", function (e) {
        //    e.preventDefault();
        //    alert("Search");
        //});
        //EmployeeCode
        //$formEl.find("#UserName").click(function (e) {
        //    e.preventDefault();
        //    if (e.keyCode=13) {
        //        var aaa = $formEl.find("#UserName").val();
        //        if (aaa) {
        //            axios.get(`/api/userinfo/getloginUserbyName/${aaa}`)
        //                .then(function (response) {
        //                    debugger;
        //                    masterData = response.data;
        //                    setFormData($formEl, masterData);
        //                    $formEl.find("#UserName").val(aaa);
        //                    console.log(response.data.UserCode);
        //                    if (response.data.UserCode > 0) {
        //                        axios.get(`/api/userinfo/${response.data.UserCode}`)
        //                            .then(function (response) {

        //                                $divDetailsEl.fadeIn();
        //                                $divTblEl.fadeOut();
        //                                masterData = response.data;
        //                                filterData = _.clone(masterData);
        //                                setFormData($formEl, masterData);
        //                                $formEl.find("#EmployeeCode").val(masterData.EmployeeName);
        //                                $formEl.find("#Password2").val(masterData.Password);
        //                                $formEl.find("#EmployeeCode").attr("disabled", "disabled");
        //                            })
        //                            .catch(showResponseError);

        //                        axios.get(`/api/userinfo/getGroupList/${response.data.UserCode}`)
        //                            .then(function (response) {
        //                                initChildTable(response.data.Items);
        //                            })
        //                            .catch(function (err) {
        //                                toastr.error(err.response.data.Message);
        //                            });
        //                    }
        //                    else {
        //                        initChildTable([]);
        //                    }
        //                })
        //                .catch(function (err) {
        //                    toastr.error(err.response.data.Message);
        //                });
        //        }
        //    }
            
        //});
        $formEl.find("#UserName").mouseleave(function (e) {
        //$formEl.find("#UserName").blur(function (e) {
            e.preventDefault();
            var aaa = $formEl.find("#UserName").val();
            debugger;
            if (aaa) {
                axios.get(`/api/userinfo/getloginUserbyName/${aaa}`)
                    .then(function (response) {
                        debugger;
                        masterData = response.data;
                        setFormData($formEl, masterData);
                        $formEl.find("#UserName").val(aaa);
                        if (response.data.UserCode > 0) {
                            axios.get(`/api/userinfo/${response.data.UserCode}`)
                                .then(function (response) {

                                    $divDetailsEl.fadeIn();
                                    $divTblEl.fadeOut();
                                    masterData = response.data;
                                    filterData = _.clone(masterData);
                                    setFormData($formEl, masterData);
                                    //$formEl.find("#UserName").attr("disabled", "disabled");
                                    //$formEl.find("#EmployeeCode").val(masterData.Name);
                                    $formEl.find("#EmployeeCode").val(masterData.EmployeeName);
                                    $formEl.find("#Password2").val(masterData.Password);
                                    $formEl.find("#EmployeeCode").attr("disabled", "disabled");
                                })
                                .catch(showResponseError);

                            axios.get(`/api/userinfo/getGroupList/${response.data.UserCode}`)
                                .then(function (response) {
                                    initChildTable(response.data.Items);
                                })
                                .catch(function (err) {
                                    toastr.error(err.response.data.Message);
                                });
                        }
                        else {
                            initChildTable([]);
                        }

                        //if (response.data.UserCode > 0) {
                        //    axios.get(`/api/userinfo/getGroupList/${response.data.UserCode}`)
                        //        .then(function (response) {
                        //            initChildTable(response.data.Items);
                        //        })
                        //        .catch(function (err) {
                        //            toastr.error(err.response.data.Message);
                        //        });
                        //}
                    })
                    .catch(function (err) {
                        toastr.error(err.response.data.Message);
                    });
            }

            //var UserCode = $formEl.find("#UserCode").val();
            //if (UserCode > 0) {
            //    axios.get(`/api/userinfo/${UserCode}`)
            //        .then(function (response) {

            //            $divDetailsEl.fadeIn();
            //            $divTblEl.fadeOut();
            //            masterData = response.data;
            //            filterData = _.clone(masterData);
            //            setFormData($formEl, masterData);
            //            $formEl.find("#UserName").attr("disabled", "disabled");
            //            $formEl.find("#EmployeeCode").val(masterData.Name);
            //            $formEl.find("#Password2").val(masterData.Password);
            //            $formEl.find("#EmployeeCode").attr("disabled", "disabled");
            //        })
            //        .catch(showResponseError);
            //}
        });

        $toolbarEl.find("#btnNew").on("click", getNew);

        $formEl.find("#btnNewItem").on("click", function (e) {
            e.preventDefault();
            var newChildItem = {
                OptionID: getMaxIdForArray(masterData.KnittingMachineOptions, "OptionID"),
                MachineGauge: 0,
                CylinderNo: 0,
                Needle: 0,
                EntityState: 4
            };

            //masterData.KnittingMachineOptions.push(newChildItem);
            //$tblChildEl.bootstrapTable('load', masterData.KnittingMachineOptions);
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnCancel").on("click", backToList);

        //$formEl.find("#UserName").on("blur", getEmployee);
        
        //$formEl.find("#MachineTypeID").on("select2:select select2:unselect", function (e) {
        //    var machineNatureId;
        //    var subClassList = [];
        //    if (e.params._type === "unselect") {
        //        machineNatureId = "";
        //    }
        //    else {
        //        var subClassList = filterData.MachineSubClassList.filter(function (el) { return el.desc == e.params.data.id });
        //        machineNatureId = e.params.data.desc;
        //    }

        //    $formEl.find("#MachineNatureID").val(machineNatureId);
        //    toggleFlatKnitControls(machineNatureId);
        //    initSelect2($formEl.find("#MachineSubClassID"), subClassList, true, "Select machine sub class.");
        //});

    });
    function getEmployee() {
        var aaa = $formEl.find("#UserName").val();
        if (aaa) {
            axios.get(`/api/userinfo/getloginUserbyName/${aaa}`)
                .then(function (response) {
                    masterData = response.data;
                    setFormData($formEl, masterData);
                })
                .catch(function (err) {
                    toastr.error(err.response.data.Message);
                });                    
        }
        //var UserCode = $formEl.find("#UserCode").val();
        //if (UserCode > 0) {
        //    axios.get(`/api/userinfo/${UserCode}`)
        //        .then(function (response) {

        //            $divDetailsEl.fadeIn();
        //            $divTblEl.fadeOut();
        //            masterData = response.data;
        //            filterData = _.clone(masterData);
        //            setFormData($formEl, masterData);
        //            $formEl.find("#UserName").attr("disabled", "disabled");
        //            $formEl.find("#EmployeeCode").val(masterData.Name);
        //            $formEl.find("#Password2").val(masterData.Password);
        //            $formEl.find("#EmployeeCode").attr("disabled", "disabled");
        //        })
        //        .catch(showResponseError);
        //}
    }
    function initMasterTable() {
        
        var columns = [];
        columns = [{
            headerText: 'Commands', commands: [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
        },
        {
            field: 'UserName', headerText: 'User Name'
        },
        {
            field: 'Name', headerText: 'Display Name'
        },
        {
            field: 'Email', headerText: 'Email'
        }
        ];
        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/userinfo/list`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        getDetails(args.rowData.UserCode);
        $formEl.find("#btnSave").fadeIn();
    }

    async function initChildTable(data) {
        if ($tblChildEl) $tblChildEl.destroy();
        var commands =
            [
               /* { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },*/
                { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } }
            ];

        var columns = [
            {
                headerText: '', commands: commands, width: "100px", textAlign: 'Left'
            },
            { field: 'id', headerText: 'Code',isPrimaryKey: true},
            { field: 'text', headerText: 'GroupName'},
        ];

        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.OrderChildID = getMaxIdForArray(masterData.Childs, "OrderChildID");
                }
                else if (args.requestType === "save") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.OrderChildID);
                    masterData.Childs[index] = args.data;
                }
            },
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false
        };

        //tableOptions["toolbar"] = ['Add'];
        tableOptions["editSettings"] = { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true };
        $tblChildEl = new initEJ2Grid(tableOptions);
    }

    //function getMasterTableData() {
    //    var queryParams = $.param(tableParams);
    //    $tblMasterEl.bootstrapTable('showLoading');
    //    var url = "/api/user-info_list?gridType=bootstrap-table&" + queryParams;
    //    axios.get(url)
    //        .then(function (response) {
    //            $tblMasterEl.bootstrapTable('load', response.data);
    //            $tblMasterEl.bootstrapTable('hideLoading');
    //        })
    //        .catch(function (err) {
    //            toastr.error(err.response.data.Message);
    //        })
    //}

    //function initChildTable() {
    //    $tblChildEl.bootstrapTable('destroy');
    //    $tblChildEl.bootstrapTable({
    //        uniqueId: 'OptionID',
    //        columns: [
    //            {
    //                width: 40,
    //                formatter: function (value, row, index, field) {
    //                    return ['<span class="btn-group">',
    //                        '<a class="btn btn-xs btn-danger remove" onclick="javascript:void(0)" title="Remove">',
    //                        '<i class="fa fa-remove" aria-hidden="true"></i>',
    //                        '</a>',
    //                        '</span>'].join(' ');
    //                },
    //                events: {
    //                    'click .remove': function (e, value, row, index) {
    //                        e.preventDefault();
    //                        $tblChildEl.bootstrapTable('remove', { field: 'OptionID', values: [row.OptionID] });
    //                    },
    //                }
    //            },
    //            {
    //                field: "MachineGauge",
    //                title: "Machine Gauge",
    //                editable: {
    //                    type: 'text',
    //                    inputclass: 'input-sm m-w-50',
    //                    showbuttons: false
    //                }
    //            },
    //            {
    //                field: "CylinderNo",
    //                title: "No of Cylinder",
    //                editable: {
    //                    type: 'text',
    //                    inputclass: 'input-sm m-w-50',
    //                    showbuttons: false
    //                }
    //            },
    //            {
    //                field: "Needle",
    //                title: "Needle",
    //                editable: {
    //                    type: 'text',
    //                    inputclass: 'input-sm m-w-50',
    //                    showbuttons: false
    //                }
    //            },

    //        ]
    //    });
    //}

    function save() {
        var formData = getFormData($formEl);
        var formObj = formDataToJson(formData);
        var data = formDataToJson($formEl.serializeArray());
        data.UserName = $formEl.find("#UserName").val();
        data.EmployeeCode = $formEl.find("#EmployeeId").val();
        var pw1 = $formEl.find("#Password").val();
        var pw2 = $formEl.find("#Password2").val();
        if (pw1 != pw2) {
            return toastr.error("Passwords did not match");
            //alert("Passwords did not match");
        }
        //Email validation
        var validRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/;

        if ($formEl.find("#Email").val().match(validRegex)) {

        } else {
            return toastr.error("Invalid email address!");
        }
        //Email validation
        data.IsAdmin;
        if ($formEl.find("#IsAdmin").is(':checked'))
            data.IsAdmin = 1;
        else {
            data.IsAdmin = 0;
        }

        data.IsActive;
        if ($formEl.find("#IsActive").is(':checked'))
            data.IsActive = 1;
        else {
            data.IsActive = 0;
        }

        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints))
            return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);



        axios.post("/api/userinfo/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
        //getMasterTableData();
        //$formEl.find("#EmployeeId").fadeIn();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#UserID").val(-1111);
        $formEl.find("#EntityState").val(4);

        //$formEl.find("#UserName").attr("disabled", "false");
        //$formEl.find("#EmployeeId").attr("disabled", "false");
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew() {
        
        axios.get(`/api/userinfo/new`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                filterData = _.clone(masterData);
                //$formEl.find("#EmployeeCode").removeAttr("disabled");
                $formEl.find("#EmployeeCode").attr("disabled", "disabled");
                $formEl.find("#UserName").removeAttr("disabled");
                //$formEl.find("#EmployeeId").hide();
                //masterData.YarnPRRequiredDate = formatDateToDefault(masterData.YarnPRRequiredDate);
                //masterData.YarnPRDate = formatDateToDefault(masterData.YarnPRDate);
                //.MachineSubClassList = [];

                setFormData($formEl, masterData);
                //initChildTable();
            })
            .catch(showResponseError);
    }

    function getDetails(id) {

        axios.get(`/api/userinfo/${id}`)
            .then(function (response) {
                
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                filterData = _.clone(masterData);
                setFormData($formEl, masterData);

                //$formEl.find("#EmployeeId").val(masterData.EmployeeCode);
                //$tagId.attr("disabled", "disabled");
                $formEl.find("#UserName").attr("disabled", "disabled");
                //var name = $formEl.find("#Name").val();
                $formEl.find("#EmployeeCode").val(masterData.EmployeeName);
                $formEl.find("#Password2").val(masterData.Password);
                $formEl.find("#EmployeeCode").attr("disabled", "disabled");
                //initChildTable();
                //$tblChildEl.bootstrapTable("load", masterData.KnittingMachineOptions);
            })
            .catch(showResponseError);
    }



    var validationConstraints = {
        Password: {
            presence: true
        },
        Email: {
            presence: true
        }
    }

    function toggleFlatKnitControls(machineNatureId) {
        if (!machineNatureId) return false;
        var machineNature = filterData.MachineNatureList.find(function (el) { return el.id == machineNatureId });
        var isFlatKnit = machineNature && machineNature.text == "Flat";
        if (isFlatKnit) {
            $("#div-flat-knit-only").show();
            $("#div-child-tbl").hide();
        }
        else {
            $("#div-flat-knit-only").show();
            $("#div-child-tbl").hide();
        }
    }

})();
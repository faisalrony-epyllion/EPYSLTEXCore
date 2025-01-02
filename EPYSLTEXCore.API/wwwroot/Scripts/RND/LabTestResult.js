(function () {
    var menuId, pageName, menuParam;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var status;
    var isApprovePage = false;
    var isAcknowledgePage = false;
    var tblImageId;

    var LabTestRequisition;
    var ImagesList = Array();
    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        if (!menuParam) menuParam = localStorage.getItem("menuParam");

        if (menuParam == "Ack") isAcknowledgePage = true;
        else if (menuParam == "A") isApprovePage = true;

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        $tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        tblImageId = "#tblImage" + pageId



        if (isApprovePage) {
            status = statusConstants.PROPOSED;
            $toolbarEl.find("#btnSendCompleteList,#btnApproveList").fadeIn();
            $toolbarEl.find("#btnAcknowledgeList,#btnCompleteList").fadeOut();
            toggleActiveToolbarBtn($toolbarEl.find("#btnSendCompleteList"), $toolbarEl);
        } else if (isAcknowledgePage) {
            status = statusConstants.APPROVED;
            $toolbarEl.find("#btnApproveList,#btnAcknowledgeList").fadeIn();
            $toolbarEl.find("#btnCompleteList,#btnSendCompleteList").fadeOut();
            toggleActiveToolbarBtn($toolbarEl.find("#btnApproveList"), $toolbarEl);
        } else {
            status = statusConstants.COMPLETED;
            $toolbarEl.find("#btnCompleteList,#btnSendCompleteList,#btnApproveList").fadeIn();
            $toolbarEl.find("#btnAcknowledgeList").fadeIn();
            toggleActiveToolbarBtn($toolbarEl.find("#btnCompleteList"), $toolbarEl);
        }

        initMasterTable();
        initChildTable();
        getMasterTableData();

        $toolbarEl.find("#btnCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnSendCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PROPOSED;
            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnApproveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.APPROVED;
            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.ACKNOWLEDGE;
            initMasterTable();
            getMasterTableData();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnSaveAndSend").click(function (e) {
            e.preventDefault();
            save(true);
        });
        $formEl.find("#btnApprove").click(function (e) {
            e.preventDefault();
            save(true);
        });
        $formEl.find("#btnAcknowledage").click(function (e) {
            e.preventDefault();
            save(true);
        });
        $formEl.find("#btnBack").on("click", backToList);

        $formEl.find("#btnAddBuyer").click(function (e) {
            e.preventDefault();
            getBuyer();
        });

        $formEl.find("#imageShow").on("click", function (e) {

            e.preventDefault();
            $("#modal-child-Live-Product").modal('show');
            initMultipleImageAttachment(LabTestRequisition.LabTestRequisitionImages, $formEl.find("#UploadFile"));
            initNewAttachment($("#UploadFile"));
        });
        $formEl.find("#btnAddImages").click(function (e) {
            e.preventDefault();
            //ImagesList = [];
            var totalfiles = document.getElementById('UploadFile').files.length;

            
            for (var i = 0; i < totalfiles; i++) {
                //Image Content File

                //var uploadedFileName = generateFileName(document.getElementById('UploadFile').files[i].name);

                var objList = new Object;
                objList.LTReqImageID = i + 1;
                //objList.LPFormID = selectedId;
                objList.ImagePath = "/Uploads/RND/LabTest/" + document.getElementById('UploadFile').files[i].name;
                objList.Image64Byte = "";
                objList.FileName = document.getElementById('UploadFile').files[i].name;
                objList.PreviewTemplate = "image";
                objList.DefaultImage = 0;
                //LabTestRequisition.LabTestRequisitionImages.push(objList);
                //Image File
                var obj = document.getElementById('UploadFile').files[i];

                objList.FileName = generateFileName(objList.FileName);
                var fileName = getFileNameWithoutExtension(objList.FileName);

                LabTestRequisition.LabTestRequisitionImages.push({
                    FileName: fileName,
                    BPID: 0,
                    ImageGroup: "",
                    ImageSubGroup: ""
                });

                generateSingleImgRow(obj, null, objList.FileName);

                //ImagesList.push(obj);
            }
            $("#modal-child-Live-Product").modal('hide');

            $("#UploadFile").val('');
            document.getElementById("UploadFile").innerHTML = "";
            document.getElementById("ProductImagediv").innerHTML = "";
            $(".file-drop-zone").text("");
            //console.log(LabTestRequisition.LabTestRequisitionImages);
        });

        //$(".file-caption-main .input-group-append .btn-file").css("width", "100px");

    });

    function initNewAttachment($el) {
        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            previewFileType: 'any'
        });

        $(".file-drop-zone").mCustomScrollbar();
    }

    function initMultipleImageAttachment(firmConceptImages, $el) {
        var previewData = [];
        var previewConfig = [];
        $.each(firmConceptImages, function (i, obj) {
            var objImagesList = new Object;
            objImagesList.LTReqImageID = obj.LTReqImageID;
            objImagesList.DefaultImage = obj.DefaultImage;
            //if (obj.ImagePath == "") {
            //    objImagesList.path = obj.Image64Byte;
            //} else {
            objImagesList.path = rootPath + obj.ImagePath;
            //}
            previewData.push(objImagesList);

            var type = !obj.ImageType ? "any" : obj.ImageType;
            previewConfig.push({ type: type, caption: "Attachment", key: obj.LTReqMasterID, width: "80px", frameClass: "preview-frame" })
        });

        document.getElementById("ProductImagediv").innerHTML = "";

        for (i = 0; previewData.length > i; i++) {
            var img = new Image(150, 150);
            img.src = previewData[i].path;
            img.width = "150";
            img.height = "150";
            img.style.left = "0";
            img.style.top = "0";
            img.style.position = "absolute";
            //img.className = "zoomA";

            // to created checkbox
            //var checkbox = document.createElement('input');
            //checkbox.type = "checkbox";
            //checkbox.id = "chkDImage_" + previewData[i].LTReqMasterID;
            //checkbox.className = "chkDImage";
            //checkbox.style.width = "30px";
            //checkbox.style.height = "30px";
            //checkbox.checked = previewData[i].DefaultImage;

            ////Div Create for Checkbox
            //var divChk = document.createElement("div");
            //divChk.className = "btnDefaultImagesWrap";
            //divChk.style.right = "0";
            //divChk.style.top = "0";
            //divChk.style.zIndex = "9999999";
            //divChk.style.position = "absolute";
            //divChk.appendChild(checkbox);

            //Div Create
            var div = document.createElement("div");
            div.className = "btnImagesRemove";
            div.id = "btnImagesRemove_" + previewData[i].LTReqImageID;
            div.style.width = "40px";
            div.style.height = "40px";
            div.style.color = "red";
            div.style.textAlign = "left";
            div.style.cursor = "pointer";
            div.style.fontWeight = "700";
            div.style.fontSize = "30px";
            div.innerHTML = "X";
            div.style.position = "absolute";

            //2nd Div
            var div2 = document.createElement("div");
            div2.id = "btnMasterRemove_" + previewData[i].LTReqImageID;
            div2.style.width = "150px";
            div2.style.height = "150px";
            div2.style.position = "relative";
            div2.style.textAlign = "center";
            div2.style.float = "left";
            div2.style.marginRight = "10px";
            div2.style.marginBottom = "10px";
            div2.appendChild(img);
            div2.appendChild(div);
            //div2.appendChild(divChk);
            var src = document.getElementById("ProductImagediv");
            src.appendChild(div2);
        }
        document.getElementById("ProductImagediv").hidden = false;
    }


    $(document).on('click', '.btnImagesRemove', function () {

        var rID = $(this).attr("id").split('_')[1];
        $("#btnMasterRemove_" + rID).remove();
        LabTestRequisition.LabTestRequisitionImages.splice($.inArray($("#btnMasterRemove_" + rID), LabTestRequisition.LabTestRequisitionImages), 1);
    });
    $(document).on('click', '.chkDImage', function () {
        $(".chkDImage").prop("checked", false);
        var rID = $(this).attr("id").split('_')[1];
        $("#" + $(this).attr("id")).prop("checked", true);

        $.each(LabTestRequisition.LabTestRequisitionImages, function (j, obj) {
            if (obj.LTReqMasterID == rID) {
                obj.DefaultImage = 1;
            } else {
                obj.DefaultImage = 0;
            }
        });
    });
    function getBase641(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.readAsDataURL(file);
            reader.onload = () => resolve(reader.result);
            reader.onerror = error => reject(error);
        });
    }

    function getBase64(file) {
        var strm, i;
        var reader = new FileReader();
        reader.readAsDataURL(file);
        //console.log(file.name);
        reader.onload = function (e) {
            var objList = new Object;
            objList.LTReqMasterID = i + 1;
            objList.LPFormID = selectedId;
            objList.ImagePath = "";// "/Uploads/RND/" + file.name;
            objList.Image64Byte = reader.result;
            objList.ImageName = file.name;
            objList.PreviewTemplate = "image";
            LabTestRequisition.LabTestRequisitionImages.push(objList);

            //$.each(LabTestRequisition.LabTestRequisitionImages, function (j, obj) {
            //    obj.Image64Byte = reader.result;
            //});
            //strm = reader.result;
        };
        reader.onerror = function (error) {
            //console.log('Error: ', error);
        };
    }

    function initMasterTable() {
        $tblMasterEl.bootstrapTable('destroy');
        $tblMasterEl.bootstrapTable({
            showRefresh: true,
            showExport: true,
            showColumns: true,
            toolbar: toolbarId,
            exportTypes: "['csv', 'excel']",
            pagination: true,
            filterControl: true,
            searchOnEnterKey: true,
            sidePagination: "server",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            showFooter: true,
            columns: [
                {
                    title: "Actions",
                    align: "Left",
                    formatter: function (value, row, index, field) {
                        var btnBdfView = "",
                            btnFormReport = "",
                            btnResultReport = "";
                        if (row.LabTestStatus == 'Production') {
                            btnFormReport = `<a class="btn btn-xs btn-danger" href="/reports/InlinePdfView?ReportName=LabTestTestingRequirementFormProduction.rdl&ReqID=${row.LTReqMasterID}&BuyerId=${row.BuyerID}" target="_blank" title="Lab Test Form Report" style='margin:2px;'>
                                                <i class="fas fa-file-pdf" aria-hidden="true"></i>
                                            </a>`;
                        } else {
                            if (row.TestNatureName == 'China') {
                                btnFormReport = `<a class="btn btn-xs btn-danger" href="/reports/InlinePdfView?ReportName=LabTestTestingRequirementFormChina.rdl&ReqID=${row.LTReqMasterID}&BuyerId=${row.BuyerID}" target="_blank" title="Lab Test Form Report" style='margin:2px;'>
                                                <i class="fas fa-file-pdf" aria-hidden="true"></i>
                                            </a>`;
                            }
                            else {
                                btnFormReport = `<a class="btn btn-xs btn-danger" href="/reports/InlinePdfView?ReportName=LabTestTestingRequirementForm.rdl&ReqID=${row.LTReqMasterID}&BuyerId=${row.BuyerID}" target="_blank" title="Lab Test Form Report" style='margin:2px;'>
                                                <i class="fas fa-file-pdf" aria-hidden="true"></i>
                                            </a>`;
                            }
                        }
                        if (status != statusConstants.COMPLETED) {
                            if (row.LabTestStatus == 'Production') {
                                btnResultReport = `<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportName=LabTestReportProduction.rdl&LTReqMasterID=${row.LTReqMasterID}&BuyerId=${row.BuyerID}" target="_blank" title="Lab Test Result Report" style='margin:2px;'>
                                    <i class="fas fa-file-pdf" aria-hidden="true"></i>
                                    </a>`;
                            }
                            else {
                                btnResultReport = `<a class="btn btn-xs btn-primary" href="${getReportName(row.LTReqMasterID, row.BuyerID, row.TestNatureName)}" target="_blank" title="Lab Test Result Report" style='margin:2px;'>
                                    <i class="fas fa-file-pdf" aria-hidden="true"></i>
                                    </a>`;
                            }
                            if (row.ImagePath != null && row.ImagePath.length > 0) {
                                btnBdfView = `<a class="btn btn-xs btn-warning" href="${row.ImagePath}" title="View attached file" target="_blank" style='margin:2px;'>
                                            <i class="fa fa-solid fa-file" aria-hidden="true"></i>
                                        </a>`;
                            }
                        }
                        return [
                            '<span class="btn-group">',
                            `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Lab Test" style='margin:2px;'>
                                    <i class="fa fa-edit" aria-hidden="true"></i>
                                    </a>`,
                            `${btnResultReport}
                            ${btnFormReport}
                            ${btnBdfView}`,
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.LTReqMasterID);

                            if (isApprovePage) {
                                if (status == statusConstants.PROPOSED) {
                                    $formEl.find("#btnApprove").fadeIn();
                                    $formEl.find("#btnSave,#btnSaveAndSend,#btnAcknowledage").fadeOut();
                                } else {
                                    $formEl.find("#btnSave,#btnSaveAndSend,#btnApprove,#btnAcknowledage").fadeOut();
                                }
                            } else if (isAcknowledgePage) {
                                if (status == statusConstants.APPROVED) {
                                    $formEl.find("#btnAcknowledage").fadeIn();
                                    $formEl.find("#btnSave,#btnSaveAndSend,#btnApprove").fadeOut();
                                } else {
                                    $formEl.find("#btnSave,#btnSaveAndSend,#btnApprove,#btnAcknowledage").fadeOut();
                                }
                            } else {
                                if (status == statusConstants.COMPLETED || status == statusConstants.PROPOSED) {
                                    $formEl.find("#btnSave,#btnSaveAndSend").fadeIn();
                                    $formEl.find("#btnApprove,#btnAcknowledage").fadeOut();
                                } else {
                                    $formEl.find("#btnSave,#btnSaveAndSend,#btnApprove,#btnAcknowledage").fadeOut();
                                }
                            }
                        }
                    }
                },
                {
                    field: "LabTestStatus",
                    title: "Lab Test Status",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ReqNo",
                    title: "Req. No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ReqDate",
                    title: "Req. Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "TestNatureName",
                    title: "Test Nature",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                    //visible: status != statusConstants.COMPLETED
                },
                {
                    field: "DBatchNo",
                    title: "D. Batch No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "DBatchDate",
                    title: "D. Batch Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "ConceptNo",
                    title: "Concept No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BuyerName",
                    title: "Buyer",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "BuyerTeamName",
                    title: "Buyer Team",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                //{
                //    field: "BookingNo",
                //    title: "Booking No",
                //    filterControl: "input",
                //    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                //},
                {
                    field: "ColorName",
                    title: "Color Name",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "FabricQty",
                    title: "Fabric Qty"
                }

            ],
            onPageChange: function (number, size) {
                var newOffset = (number - 1) * size;
                var newLimit = size;
                if (tableParams.offset == newOffset && tableParams.limit == newLimit)
                    return;

                tableParams.offset = newOffset;
                tableParams.limit = newLimit;

                getMasterTableData();
            },
            onSort: function (name, order) {
                tableParams.sort = name;
                tableParams.order = order;
                tableParams.offset = 0;

                getMasterTableData();
            },
            onRefresh: function () {
                resetTableParams();
                getMasterTableData();
            },
            onColumnSearch: function (columnName, filterValue) {
                if (columnName in filterBy && !filterValue) {
                    delete filterBy[columnName];
                }
                else
                    filterBy[columnName] = filterValue;

                if (Object.keys(filterBy).length === 0 && filterBy.constructor === Object)
                    tableParams.filter = "";
                else
                    tableParams.filter = JSON.stringify(filterBy);

                getMasterTableData();
            }
        });
    }

    function getReportName(LTReqMasterID, buyerID, testNatureName) {
        var rdlName = "LabTestReport.rdl";
        if (testNatureName == "China") rdlName = "LabTestReportChina.rdl";
        if (buyerID == 4) rdlName = "LabTestReportMnsMain.rdl"; //M&S
        return `/reports/InlinePdfView?ReportName=${rdlName}&LTReqMasterID=${LTReqMasterID}&BuyerId=${buyerID}`;
    }

    function getMasterTableData() {
        var queryParams = $.param(tableParams);
        $tblMasterEl.bootstrapTable('showLoading');
        var url = `/api/lab-test-result/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function initChildTable() {
        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            showFooter: true,
            detailView: true,
            checkboxHeader: false,
            uniqueId: 'LTReqBuyerID',
            columns: [
                {
                    field: "BuyerName",
                    title: "Buyer Name",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "IsPass",
                    title: "Pass?",
                    showSelectTitle: true,
                    formatter: function (value, row, index, field) {
                        if (isApprovePage || isAcknowledgePage) {
                            return value ? "Pass" : "Fail";
                        } else {
                            var checked = value ? "checked" : "";
                            return `<input class="form-check-input pass" type="checkbox" ${checked} />`;
                        }
                    },
                    events: {
                        'click .pass': function (e, value, row, index) {
                            row.IsPass = e.currentTarget.checked;
                            LabTestRequisition.LabTestRequisitionBuyers.map(x => {
                                x.IsPass = row.IsPass;
                                x.LabTestRequisitionBuyerParameters.map(y => {
                                    y.IsPass = row.IsPass;
                                    //y.TestValue = "";
                                    //y.TestValue1 = "";
                                    y.BuyerParameters.map(z => {
                                        z.IsPass = row.IsPass;
                                        //z.TestValue = "";
                                        //z.TestValue1 = "";
                                    });
                                });
                            });
                            $tblChildEl.bootstrapTable('updateByUniqueId', { id: row.LTReqBuyerID, row: row });
                        }
                    }
                },
                {
                    field: "Remarks",
                    title: "Remarks",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" min="0" style="padding-right: 24px;">'
                    }
                },
                {
                    field: "IsApproved",
                    title: "Approved?",
                    showSelectTitle: true,
                    visible: (isApprovePage || isAcknowledgePage),
                    formatter: function (value, row, index, field) {
                        if (isAcknowledgePage) {
                            return value ? "Approve" : "Not Approve";
                        } else {
                            var checked = value ? "checked" : "";
                            return `<input class="form-check-input approve" type="checkbox" ${checked} />`;
                        }
                    },
                    events: {
                        'click .approve': function (e, value, row, index) {
                            row.IsApproved = e.currentTarget.checked;
                            $tblChildEl.bootstrapTable('updateByUniqueId', { id: row.LTReqBuyerID, row: row });
                        }
                    }
                },
                {
                    field: "IsAcknowledge",
                    title: "Acknowledge?",
                    showSelectTitle: true,
                    visible: isAcknowledgePage,
                    formatter: function (value, row, index, field) {
                        var checked = value ? "checked" : "";
                        return `<input class="form-check-input acknowledge" type="checkbox" ${checked} />`;
                    },
                    events: {
                        'click .acknowledge': function (e, value, row, index) {
                            row.IsAcknowledge = e.currentTarget.checked;
                            $tblChildEl.bootstrapTable('updateByUniqueId', { id: row.LTReqBuyerID, row: row });
                        }
                    }
                }
            ],
            onExpandRow: function (index, row, $detail) {
                populateLabTestRequisitionBuyer(row.LTReqBuyerID, $detail, index);
            },
        });
    }

    function getBuyer() {
        var url = "/api/selectoption/buyername";
        axios.get(url)
            .then(function (response) {
                showBootboxSelect2MultipleDialog("Select Buyer", "Ids", "Select Buyer", response.data, function (result) {
                    if (result) {
                        var ids = "";
                        for (var i = 0; i < result.length; i++) ids += result[i].id + ",";
                        getBuyerParameter(ids.substring(0, ids.length - 1));
                    }
                });
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getBuyerParameter(ids) {
        var url = "/api/lab-test-result/buyer-parameter/" + ids;
        axios.get(url)
            .then(function (response) {
                for (var i = 0; i < response.data.length; i++) {
                    response.data[i].LTReqBuyerID = (LabTestRequisition.LabTestRequisitionBuyers.length == 0) ? 1 : LabTestRequisition.LabTestRequisitionBuyers[LabTestRequisition.LabTestRequisitionBuyers.length - 1].LTReqBuyerID + 1;
                    LabTestRequisition.LabTestRequisitionBuyers.push(response.data[i]);
                }

                $tblChildEl.bootstrapTable("load", LabTestRequisition.LabTestRequisitionBuyers);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function populateLabTestRequisitionBuyer(childId, $detail, index) {
        $el = $detail.html('<table id="TblLabTestRequisitionBuyer-' + childId + '"></table>').find('table');

        initLabTestRequisitionBuyer($el, childId, index);

        LabTestRequisition.LabTestRequisitionBuyers[index].LabTestRequisitionBuyerParameters.forEach(x => {
            if (x.BuyerParameters != null && x.BuyerParameters.length > 0) {
                x.IsPass = x.BuyerParameters[0].IsPass;
                x.ParameterStatus = x.BuyerParameters[0].ParameterStatus;
                x.ParameterRemarks = x.BuyerParameters[0].ParameterRemarks;
                x.AdditionalInfo = x.BuyerParameters[0].AdditionalInfo;
            }
        });

        $el.bootstrapTable('load', LabTestRequisition.LabTestRequisitionBuyers[index].LabTestRequisitionBuyerParameters);
    }

    function initLabTestRequisitionBuyer($ChildBuyerParameter, childId, parentIndex) {
        $ChildBuyerParameter.bootstrapTable({
            showFooter: true,
            uniqueId: 'BPID',
            checkboxHeader: false,
            detailView: true,
            columns: [
                {
                    field: "TestName",
                    title: "Parameter Name",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "TestMethod",
                    title: "Test Method",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "IsPass",
                    title: "Pass?",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    checkbox: true,
                    showSelectTitle: true
                },
                {
                    field: "AdditionalInfo",
                    title: "Additional Info",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "ParameterStatus",
                    title: "Status",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "ParameterRemarks",
                    title: "Remarks",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                }
            ],
            onEditableSave: function (field, row, oldValue, $el) {
                if (isNaN(row.TestValue) || row.TestValue < 0) {
                    bootbox.alert({
                        size: "small",
                        title: "Alert !!!",
                        message: "Invalid Test Value 1!!!",
                        callback: function () {
                            row.TestValue = oldValue;
                        }
                    })
                    row.TestValue = oldValue;
                }
                if (isNaN(row.TestValue1) || row.TestValue1 < 0) {
                    bootbox.alert({
                        size: "small",
                        title: "Alert !!!",
                        message: "Invalid Test Value 2!!!",
                        callback: function () {
                            row.TestValue1 = oldValue;
                        }
                    })
                    row.TestValue1 = oldValue;
                }
                if (isNaN(row.Requirement) || row.Requirement < 0) {
                    bootbox.alert({
                        size: "small",
                        title: "Alert !!!",
                        message: "Invalid Requirement 1!!!",
                        callback: function () {
                            row.Requirement = oldValue;
                        }
                    })
                    row.Requirement = oldValue;
                }
                if (isNaN(row.Requirement1) || row.Requirement1 < 0) {
                    bootbox.alert({
                        size: "small",
                        title: "Alert !!!",
                        message: "Invalid Requirement 2!!!",
                        callback: function () {
                            row.Requirement1 = oldValue;
                        }
                    })
                    row.Requirement1 = oldValue;
                }
                //row.IsPass = (row.TestValue >= row.RefValueFrom && row.TestValue <= row.RefValueTo);
                $ChildBuyerParameter.bootstrapTable('updateByUniqueId', { id: row.LTReqBPID, row: row });
            },
            onExpandRow: function (index, row, $detail) {
                var aaa = LabTestRequisition.LabTestRequisitionBuyers[parentIndex].LabTestRequisitionBuyerParameters[index];
                populateLabTestRequisitionBuyerParam(row.BPID, $detail, parentIndex, index);
            }
        });
    }

    function populateLabTestRequisitionBuyerParam(bpId, $detail, parentIndex, index) {

        $el = $detail.html('<table id="TblLabTestRequisitionBuyerParameter-' + bpId + '"></table>').find('table');
        initLabTestRequisitionBuyerParameter($el, bpId);
        $el.bootstrapTable('load', LabTestRequisition.LabTestRequisitionBuyers[parentIndex].LabTestRequisitionBuyerParameters[index].BuyerParameters);
    }

    function initLabTestRequisitionBuyerParameter($ChildBuyerParameter, childId) {
        $ChildBuyerParameter.bootstrapTable({
            showFooter: true,
            uniqueId: 'LTReqBPID',
            checkboxHeader: false,
            columns: [
                {
                    field: "SubTestName",
                    title: "Test Name",
                    cellStyle: function () { return { classes: 'm-w-80' } }
                },
                {
                    field: "SubSubTestName",
                    title: "Sub-Test Name",
                    cellStyle: function () { return { classes: 'm-w-80' } }
                },
                {
                    field: "RefValueFrom",
                    title: "Range Value",
                    cellStyle: function () { return { classes: 'm-w-80' } }
                },
                {
                    field: "TestValue",
                    title: "Test Value 1",
                    cellStyle: function () { return { classes: 'm-w-60' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        //tpl: '<input type="number" class="form-control input-sm" min="0" step="0.01" pattern="^\d+(?:\.\d{1,2})?$" style="padding-right: 24px;">',
                        //validate: function (value) {
                        //    if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                        //        //return 'Must be a positive integer.';
                        //    }
                        //}
                    }
                },
                {
                    field: "TestValue1",
                    title: "Test Value 2",
                    cellStyle: function () { return { classes: 'm-w-60' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        //tpl: '<input type="number" class="form-control input-sm" min="0" step="0.01" pattern="^\d+(?:\.\d{1,2})?$" style="padding-right: 24px;">',
                        //validate: function (value) {
                        //    if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                        //        //return 'Must be a positive integer.';
                        //    }
                        //}
                    }
                },
                {
                    field: "Requirement",
                    title: "Requirement 1",
                    cellStyle: function () { return { classes: 'm-w-60' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        //tpl: '<input type="number" class="form-control input-sm" min="0" step="0.01" pattern="^\d+(?:\.\d{1,2})?$" style="padding-right: 24px;">',
                        //validate: function (value) {
                        //    if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                        //        //return 'Must be a positive integer.';
                        //    }
                        //}
                    }
                },
                {
                    field: "Requirement1",
                    title: "Requirement 2",
                    cellStyle: function () { return { classes: 'm-w-60' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        //tpl: '<input type="number" class="form-control input-sm" min="0" step="0.01" pattern="^\d+(?:\.\d{1,2})?$" style="padding-right: 24px;">',
                        //validate: function (value) {
                        //    if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                        //        //return 'Must be a positive integer.';
                        //    }
                        //}
                    }
                },
                {
                    field: "IsPass",
                    title: "Pass?",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    checkbox: true,
                    showSelectTitle: true
                },
                {
                    field: "Addition1",
                    title: "Addition 1",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "Addition2",
                    title: "Addition 2",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "Remarks",
                    title: "Remarks",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                }
            ]
        });
    }


    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        getMasterTableData();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#LTReqMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getDetails(id) {
        resetImageTable();
        axios.get(`/api/lab-test-result/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                ImagesList = [];

                LabTestRequisition = response.data;
                LabTestRequisition.ReqDate = formatDateToDefault(LabTestRequisition.ReqDate);
                LabTestRequisition.DBatchDate = formatDateToDefault(LabTestRequisition.DBatchDate);

                if (LabTestRequisition.LabTestRequisitionImages.length > 0) {
                    for (var i = 0; i < LabTestRequisition.LabTestRequisitionImages.length; i++) {
                        initAttachment(LabTestRequisition.LabTestRequisitionImages[i].ImagePath, LabTestRequisition.LabTestRequisitionImages[i].PreviewTemplate, $formEl.find("#UploadFile"));
                    }
                }
                else {
                    initAttachment("", "", $formEl.find("#UploadFile"));
                }
                //formData.append("UploadFile", LabTestRequisition.LabTestRequisitionImages);
                setFormData($formEl, LabTestRequisition);
                $tblChildEl.bootstrapTable("load", LabTestRequisition.LabTestRequisitionBuyers);
                $tblChildEl.bootstrapTable('hideLoading');

                LabTestRequisition.LabTestRequisitionImages.map(x => {
                    generateSingleImgRow(null, x, x.FileName);
                });

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function generateFileName(pFName) {
        var fName = getFileNameWithoutExtension(pFName);
        var ex = getFileExtension(pFName);
        fName = fName + "_" + Math.random().toString(16).slice(2) + ex;
        fName = removeInvalidChar(fName);
        return fName;
    }
    function removeInvalidChar(fName) {
        fName = fName.replaceAll("#", "").replace(/[^a-z0-9 ,._]/ig, '');
        return fName;
    }
    function generateSingleImgRow(file, imgObj, fileName1) {
        var rowIndex = $(tblImageId).find("tbody tr").length + 1;

        if (imgObj == null) {
            file.RowIndex = rowIndex;

            var blob = file.slice(0, file.size, file.type);
            fileName1 = removeInvalidChar(fileName1);
            var newFile = new File([blob], fileName1, { type: file.type });
            newFile.RowIndex = rowIndex;
            ImagesList.push(newFile);
        }

        var src = "#",
            fileName = "",
            imageGroup = "",
            imageSubGroup = "",
            bpID = 0;

        var fileNameWithoutEx = "";

        if (imgObj != null) {
            src = imgObj.ImagePath;
            fileName = imgObj.FileName;
            imageGroup = imgObj.ImageGroup;
            imageSubGroup = imgObj.ImageSubGroup;
            bpID = imgObj.BPID;

            fileNameWithoutEx = imgObj.FileName;

        } else {
            fileName = fileName1; //typeof file.FileName === "undefined" ? file.name : file.FileName;
            fileNameWithoutEx = getFileNameWithoutExtension(fileName);
        }


        $(tblImageId).find("tbody").append(
            `<tr class='trImg' rowIndex = ${rowIndex}>
                <td class='tdStatic'>
                   <button id='btnRemoveImg${rowIndex}' type='button' class="btn btn-danger btn-xs add btnRemoveImg" title="Remove"><i class="fa fa-trash"></i></button>
                </td>
                <td class='tdStatic'>
                    <span>${rowIndex}</span>
                </td>
                <td class='cellSelect'>
                    <select class='cboTestMethod cboTestMethod${rowIndex} txtImg' value="${imageGroup}"></select>
                </td>
                <td>
                    <input type='text' class='form-control txtImageGroup txtImageGroup${rowIndex} txtImg' placeholder = 'Image Group' value="${imageGroup}" />
                </td>
                <td>
                    <input type='text' class='form-control txtImageSubGroup txtImageSubGroup${rowIndex} txtImg' placeholder = 'Image Sub Group' value="${imageSubGroup}" />
                </td>
                <td class="cellImg">
                    <img id="img${rowIndex}" src="${src}" alt="${fileName}" class='zoom' style='height:60px;weight:60px;' />
                </td>
                <td style='display:none;'>
                    <input type='text' class='txtFileName txtFileName${rowIndex} txtImg' value="${fileNameWithoutEx}" />
                </td>
            </tr>`
        );

        $(tblImageId).find("#btnRemoveImg" + rowIndex).click(function () {
            var indexF = ImagesList.findIndex(x => x.RowIndex == rowIndex);
            if (indexF > -1) {
                ImagesList.splice(indexF, 1);
                var fileName = typeof ImagesList[indexF].FileName === "undefined" ? ImagesList[indexF].name : ImagesList[indexF].FileName;
                var extension = getFileExtension(fileName);
                indexF = LabTestRequisition.LabTestRequisitionImages.findIndex(x => x.FileName + extension == fileName);
                if (indexF > -1) {
                    LabTestRequisition.LabTestRequisitionImages[indexF].IsDelete = true;
                }
            } else {
                var fileName = $(tblImageId).find(".txtFileName" + rowIndex).val();
                indexF = LabTestRequisition.LabTestRequisitionImages.findIndex(x => x.FileName == fileName);
                if (indexF > -1) {
                    LabTestRequisition.LabTestRequisitionImages[indexF].IsDelete = true;
                }
            }
            $(tblImageId).find("tbody tr[rowIndex=" + rowIndex + "]").remove();
        });
        $(tblImageId).find(".txtImageGroup" + rowIndex).keyup(function (e) {
            var imageGroup = $.trim($(this).val());
            $(this).attr("title", imageGroup.length > 30 ? imageGroup : "");

            var indexF = ImagesList.findIndex(x => x.RowIndex == rowIndex);
            if (indexF > -1) {
                ImagesList[indexF].ImageGroup = imageGroup;
            }
            var fileName = $(tblImageId).find(".txtFileName" + rowIndex).val();
            indexF = LabTestRequisition.LabTestRequisitionImages.findIndex(x => x.FileName == fileName);
            if (indexF > -1) {
                
                LabTestRequisition.LabTestRequisitionImages[indexF].ImageGroup = imageGroup;
            } 
        });
        $(tblImageId).find(".txtImageSubGroup" + rowIndex).keyup(function (e) {
            var imageSubGroup = $.trim($(this).val());
            $(this).attr("title", imageSubGroup.length > 30 ? imageSubGroup : "");

            var indexF = ImagesList.findIndex(x => x.RowIndex == rowIndex);
            if (indexF > -1) {
                ImagesList[indexF].ImageSubGroup = imageSubGroup;
            }
            var fileName = $(tblImageId).find(".txtFileName" + rowIndex).val();
            indexF = LabTestRequisition.LabTestRequisitionImages.findIndex(x => x.FileName == fileName);
            if (indexF > -1) {
                
                LabTestRequisition.LabTestRequisitionImages[indexF].ImageSubGroup = imageSubGroup;
            } 
        });
        loadTestMethod(rowIndex, bpID);
        $(tblImageId).find(".cboTestMethod" + rowIndex).change(function (e) {
            
            var bpID = $.trim($(this).val());
            var fileName = $(tblImageId).find(".txtFileName" + rowIndex).val();
            var indexF = LabTestRequisition.LabTestRequisitionImages.findIndex(x => x.FileName == fileName);
            if (indexF > -1) {
                
                LabTestRequisition.LabTestRequisitionImages[indexF].BPID = bpID;
            } 
        });
        if (imgObj == null) {
            displayImage(rowIndex, file);
        }
    }
    function getSuggestedGroup(bpID, rowIndex) {
        var cboText = $(tblImageId).find(".cboTestMethod" + rowIndex + " option:selected").text();
        if (cboText == "Appearance after Domestic Wash (Garments)" || bpID == 416) {
            return "Assessment Appearance After 1st care cycle Washing & Ironing";
        }
        else if (cboText == "Appearance after One Domestic Wash (Garments)" || bpID == 155) {
            return "Assessment Appearance After 2nd care cycle Washing & Ironing";
        }
        else if (cboText == "Appearance after three Domestic Wash (Garments)" || bpID == 156) {
            return "Assessment Appearance After 3rd care cycle Washing & Ironing";
        }
        else if (cboText == "Appearance after Five Domestic Wash (Garments)" || bpID == 157) {
            return "Assessment Appearance After 5th care cycle Washing & Ironing";
        }
        return "";
    }
    function loadTestMethod(rowIndex, bpID) {
        var testMethods = [];
        testMethods.push({
            BPID: 0,
            TestName: "--Select Test Method--"
        });
        LabTestRequisition.LabTestRequisitionBuyers.map(b => {
            b.LabTestRequisitionBuyerParameters.map(p => {
                var indexF = testMethods.findIndex(x => x.BPID == p.BPID);
                if (indexF < 0) {
                    testMethods.push({
                        BPID: p.BPID,
                        TestName: p.TestName
                    });
                }
            });
        });
        testMethods.map(x => {
            $(tblImageId).find(".cboTestMethod" + rowIndex).append(`<option value=${x.BPID}>${x.TestName}</option>`);
        });

        if (bpID > 0) {
            $(tblImageId).find(".cboTestMethod" + rowIndex).val(bpID);
        }

        //var maxRowIndex = getMaxRowIndex();
        //for (var i = 1; i <= maxRowIndex; i++) {
        //    testMethods.map(x => {
        //        $(tblImageId).find(".cboTestMethod" + rowIndex).append(`<option value=${x.TestMethodID}>${x.TestMethodName}</option>`);
        //    });
        //}
    }
    function getMaxRowIndex() {
        var maxRowIndex = 0,
            rowIndexs = [];
        $(tblImageId).find("tbody trImg").each(function () {
            rowIndexs.push(parseInt($(this).attr("rowIndex")));
        });
        if (rowIndexs.length > 0) maxRowIndex = Math.max(...rowIndexs);
        return maxRowIndex;
    }
    function displayImage(rowIndex, file) {
        var FR = new FileReader();
        FR.onload = function (e) {
            $(tblImageId).find("tbody tr[rowIndex=" + rowIndex + "]").find("#img" + rowIndex).attr("src", e.target.result);
        };
        FR.readAsDataURL(file);
    }
    function resetImageTable() {
        $(tblImageId).find("tbody tr").remove();
    }
    function getFileNameWithoutExtension(name) {
        var splitList = name.split('.');
        var fileName = "";
        if (splitList.length > 0) {
            extension = splitList[splitList.length - 1];
            extension = "." + extension;
            fileName = name.replace(extension,"");
        }
        return fileName;
    }
    function getFileExtension(name) {
        var splitList = name.split('.');
        var extension = "";
        if (splitList.length > 0) {
            extension = splitList[splitList.length - 1];
            extension = "." + extension;
        }
        return extension;
    }
    function initAttachment(path, type, $el) {
        if (!path) {
            initNewAttachment($el);
            return;
        }

        if (!type) type = "any";

        var preveiwData = [rootPath + path];
        //var previewConfig = [{ type: type, caption: "PI Attachment", key: 1, width: "80px", frameClass: "preview-frame" }];
        var previewConfig = [{ type: type, caption: "PI Attachment", key: 1, width: "80px", frameClass: "preview-frame" }];

        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            showRemove: false,
            initialPreview: preveiwData,
            initialPreviewAsData: true,
            initialPreviewFileType: 'image',
            initialPreviewConfig: previewConfig,
            reversePreviewOrder: true,
            purifyHtml: true,
            required: true,
            hideThumbnailContent: false,
            showPreview: true,
            maxFileSize: 4096
        });
    }

    function initNewAttachment($el) {
        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            previewFileType: 'any'
        });
    }
    

    function save(isSend = false) {
        var UploadFile = $formEl.find("#UploadFile")[0].files;
        //document.getElementById('UploadFile').files.length;
        //if (!UploadFile || UploadFile.length == 0) {
        //    if (LabTestRequisition.ImagePath == null || LabTestRequisition.ImagePath.length == 0) {
        //        return toastr.error("You must upload a lab test result attachment !");
        //    }
        //}

        var data = formDataToJson($formEl.serializeArray());
        LabTestRequisition.LabTestRequisitionBuyers.forEach(function (buyer) {
            var childs = [];
            buyer.LabTestRequisitionBuyerParameters.forEach(function (bp) {
                bp.BuyerParameters.forEach(function (child) {
                    child.IsParameterPass = bp.IsParameterPass;
                    child.ParameterStatus = bp.ParameterStatus;
                    child.ParameterRemarks = bp.ParameterRemarks;
                    child.AdditionalInfo = bp.AdditionalInfo;
                    childs.push(child);
                });
            });
            buyer.LabTestRequisitionBuyerParameters = childs;
        });
        data.IsSend = isSend;

        data.ImagePath = "";
        if (LabTestRequisition.ImagePath != null && LabTestRequisition.ImagePath.length > 0) {
            data.ImagePath = LabTestRequisition.ImagePath;
        }

        var formData = getFormData($formEl);

        for (var i = 0; i < ImagesList.length; i++) {
            formData.append("UploadFile", ImagesList[i]);
            var extension = getFileExtension(ImagesList[i].name);
            
            var indexF = LabTestRequisition.LabTestRequisitionImages.findIndex(x => x.FileName + extension == ImagesList[i].name);
            if (indexF > -1) {
                LabTestRequisition.LabTestRequisitionImages[indexF].RowIndex = ImagesList[i].RowIndex;
                LabTestRequisition.LabTestRequisitionImages[indexF].ImageGroup = ImagesList[i].ImageGroup;
                LabTestRequisition.LabTestRequisitionImages[indexF].ImageSubGroup = ImagesList[i].ImageSubGroup;
                //LabTestRequisition.LabTestRequisitionImages[indexF].BPID = ImagesList[i].BPID;
            }
            else {
                LabTestRequisition.LabTestRequisitionImages.push({
                    FileName: getFileNameWithoutExtension(ImagesList[i].name),
                    RowIndex: ImagesList[i].RowIndex,
                    ImageGroup: ImagesList[i].ImageGroup,
                    ImageSubGroup: ImagesList[i].ImageSubGroup,
                    BPID: ImagesList[i].BPID,
                });
            }
        }

        formData.append("data", JSON.stringify(data));
        formData.append("LabTestRequisitionBuyers", JSON.stringify(LabTestRequisition.LabTestRequisitionBuyers));

        if (!LabTestRequisition.LabTestRequisitionImages) {
            LabTestRequisition.LabTestRequisitionImages = [];
        }
        formData.append("LabTestRequisitionImages", JSON.stringify(LabTestRequisition.LabTestRequisitionImages));

        const config = {
            headers: {
                'content-type': 'multipart/form-data',
                'Authorization': "Bearer " + localStorage.getItem("token")
            }
        }

        axios.post("/api/lab-test-result/save", formData, config)
            .then(function (response) {
                toastr.success("Saved successfully.");
                //backToList();
                if (isSend) {
                    backToList();
                } else {
                    getDetails(response.data.LTReqMasterID);
                }
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();
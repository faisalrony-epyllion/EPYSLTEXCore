(function () {
    var pageId = null;
    var menuId, pageName;
    var masterData;
    var container = [];
    var current_page = 1;
    var records_per_page = 12;
    var toalRecords = 0;

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);

        InitializeFilter();
        setBasicInfoOnPopup();

        var filterItem = {};
        filterItem['NeedSearch'] = false;
        getDetails(filterItem, current_page);

        $("#Filter").on("click", GridFilter);

        $("#btn_prev").on("click", prevPage);

        $("#btn_next").on("click", nextPage);

        $(document).on("click", ".zoom", function (e) {
            var cid = $(this).parent().attr("cid");
            if (typeof cid === "undefined") cid = 0;
            viewImagePopup(e, cid);
        });

        $(document).on("keypress", ".inputTxtClassLPL", function (e) {
            if (e.which == 13) {
                GridFilter();
            }
        });
    });
    function sendMailRemainder(lppid, fid) {
        if (lppid > 0 && fid > 0) {
            axios.post(`/api/live-product/send-mail-remainder/${lppid}/${fid}`)
                .then(function () {
                    toastr.success("Mail Sent");
                })
                .catch(showResponseError);
        }
    }
    function openModalPriceRequestPopup(nLPPRID, isHidePrice) {
        $(".tblBuyer").hide();
        $(".tblBuyer tbody tr").remove();
        $("#IsBuyerSpecific").prop('checked', false);
        $("#RequestDate").val("");
        $("#RequiredDate").val("");
        $(".divRequiredDate").show();
        $("#PRRemarks").val("");

        if (nLPPRID > 0) {
            axios.get(`/api/live-product/get-price-request/${nLPPRID}`)
                .then(function (response) {
                    var oLPPR = response.data;
                    $("#RequestDate").val(new Date(oLPPR.RequestDate).toISOString().split('T')[0]);

                    var requireDate = new Date(oLPPR.RequiredDate).toISOString().split('T')[0];
                    if (requireDate == "1970-01-01") {
                        $(".divRequiredDate").hide();
                    } else {
                        $(".divRequiredDate").show();
                    }

                    requireDate = requireDate == "1970-01-01" ? "-" : requireDate;
                    $("#RequiredDate").val(requireDate);

                    $("#IsBuyerSpecific").prop('checked', oLPPR.IsBuyerSpecific);
                    $("#PRRemarks").val(oLPPR.PRRemarks);
                    if (oLPPR.IsBuyerSpecific && oLPPR.PriceRequestBuyers.length > 0) {
                        $(".tblBuyer").show();
                        var nSL = 0;
                        oLPPR.PriceRequestBuyers.map(x => {
                            nSL++;
                            var sTemp = "<tr><td>" + nSL + "</td><td>" + x.BuyerName + "</td><td class='clsPriceRemarks'>" + x.Price + "</td><td class='clsPriceRemarks'>" + x.Remarks + "</td></tr>";
                            $(".tblBuyer tbody").append(sTemp);
                        });
                    }
                    if (isHidePrice) {
                        $(".clsPriceRemarks").hide();
                    } else {
                        $(".clsPriceRemarks").show();
                    }
                })
                .catch(function (err) {
                });
        }
    }
    function openModalPriceRequest(nLPFormId) {
        var finder = new commonFinder({
            title: "Request Price",
            pageId: pageId,
            height: 220,
            modalSize: "modal-lg",
            apiEndPoint: `/api/live-product/get-buyers`,
            fields: "text,desc",
            widths: "100,100",
            headerTexts: "Buyer Name,Remarks",
            isMultiselect: true,
            primaryKeyColumn: "id",
            allowPaging: false,
            selectedIds: "1",

            editTypes: ",string",
            allowEditing: true,
            autofitColumns: true,

            onMultiselect: function (selectedRecords) {
                if (nLPFormId == null || nLPFormId == 0) {
                    return toastr.error("Select Form.");
                }
                var oLPPR = {
                    LPFormID: nLPFormId,
                    RequiredDate: $("#RequiredDate").val(),
                    IsBuyerSpecific: $('#IsBuyerSpecific').is(":checked"),
                    PRRemarks: $("#PRRemarks").val(),
                    PriceRequestBuyers: []
                };
                if (oLPPR.IsBuyerSpecific && selectedRecords.length > 0) {
                    selectedRecords.map(x => {
                        oLPPR.PriceRequestBuyers.push({
                            BuyerID: x.id,
                            PRRemarks: x.desc
                        });
                    });
                }
                sendPriceRequest(oLPPR);
            }
        });
        finder.showModal();
        setBasicInfoOnPopup();
    }
    function setBasicInfoOnPopup() {
        var modal = $("#modal-common-finder-" + pageId);
        modal.find(".modal-dialog").css("width", "400px");
        modal.find("#btn-selection-" + pageId).text('Request Price');

        var buyerTable = modal.find("#tbl-common-finder-" + pageId);
        buyerTable.css({
            'width': '270px !important'
        });
        buyerTable.wrap('<div class="divBuyerParent col-sm-12"></div>');
        modal.find(".modal-body").prepend(
            `<div class='basic-info-div col-sm-12'>
                <div class="col-sm-12">
                    <div class="row">
                        <label class="control-label col-sm-4" style="line-height: 24px;">Required Date</label>
                        <div class="col-sm-8">
                            <input id="RequiredDate" name="RequiredDate" type='date' class="form-control input-sm ej2-datepicker" />
                        </div>
                    </div>
                    <div class="row" style='margin-bottom:2px;'>
                        <label class="control-label col-sm-4" style="line-height: 24px;">Remarks</label>
                        <div class="col-sm-8">
                            <textarea id="PRRemarks" name="PRRemarks" class="form-control input-sm"></textarea>
                        </div>
                    </div>
                    <div class="row">
                         <label class="control-label col-sm-4" style="line-height: 24px;"></label>
                        <div class="col-sm-8">
                            <input type="checkbox" id="IsBuyerSpecific" name="IsBuyerSpecific" style="margin: 0 4px 0 0; float: left;" />
                            <label style="float:left;">Buyer Specific ?</label>
                        </div>
                    </div>
                </div>
            </div>`
        );

        buyerTable.hide();

        $("#IsBuyerSpecific").change(function () {
            if ($(this).is(":checked")) {
                buyerTable.show();
            } else {
                buyerTable.hide();
            }
        });
    }
    function sendPriceRequest(oLPPR) {
        axios.post("/api/live-product/send-price-request", oLPPR)
            .then(function (response) {
                changeBtnRequestPrice(oLPPR.LPFormID, true);

                $(".btnViewPriceRequest[fid=" + oLPPR.LPFormID + "]").show();
                $(".btnSendMailRemainder[fid=" + oLPPR.LPFormID + "]").show();
                $(".btnViewPriceRequest[fid=" + oLPPR.LPFormID + "]").attr("lppid", response.data.LPPRID);
                $(".btnSendMailRemainder[fid=" + oLPPR.LPFormID + "]").attr("lppid", response.data.LPPRID);

                //btnViewPriceSuggest
                $(".btnViewPriceSuggest[fid=" + oLPPR.LPFormID + "]").hide();

                var requestDate = getDate(response.data.RequestDate);
                $(".lblRequestDateParent[fid=" + oLPPR.LPFormID + "]").show();
                $(".lblRequestDateParent[fid=" + oLPPR.LPFormID + "] .lblRequestDate").text(requestDate);
                //$(".divBtnParent[fid=" + oLPPR.LPFormID + "]").css("margin-top", "0");

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getImageList() {
        // Loop over gallery items and push it to the array
        $('#ParentDiv').find('figure').each(function () {
            var $link = $(this).find('a'),
                item = {
                    src: $link.attr('href'),
                    w: $link.data('width'),
                    h: $link.data('height'),
                    title: $link.data('caption'),
                    cid: $link.attr('cid')
                };
            container.push(item);
        });
    }

    function viewImagePopup(event, cid) {
        // Prevent location change
        event.preventDefault();
        // Define object and gallery options
        var $pswp = $('.pswp')[0],
            options = {
                index: $(this).parent('figure').index(),
                bgOpacity: 0.85,
                showHideOpacity: true
            };

        // Initialize PhotoSwipe
        // var cidWiseList = container.filter(x => parseInt(x.cid) == parseInt(cid));

        var images = [];
        if (typeof masterData.Items !== "undefined" && masterData.Items.length > 0) {
            var cidWiseList = masterData.Items.filter(x => parseInt(x.FirmConceptMasterID) == parseInt(cid));
            cidWiseList.map(m => {
                m.LiveProductFormImages.map(ci => {
                    var item = {
                        src: ci.ImagePath,
                        w: "2500",
                        h: "1667",
                        title: m.HangerRemarks,
                        cid: m.FirmConceptMasterID,
                        caption: 'BB'
                    };
                    images.push(item);
                });
            });
        }

        if (images.length > 0) {
            var gallery = new PhotoSwipe($pswp, PhotoSwipeUI_Default, images, options);
            gallery.init();
        } else {
            toastr.error("No image found.");
        }
    }
    function changeBtnRequestPrice(fid, isSent) {
        var btn = $(".btnPriceRequestPopUp[fid=" + fid + "]");
        if (isSent) {
            btn.prop("disabled", true);
            btn.removeClass("btn-danger").addClass("btn-warning");
            btn.find("span").text("Request Sent");
        }
        else {
            btn.prop("disabled", false);
            btn.removeClass("btn-warning").addClass("btn-danger");
            btn.find("span").text("Request Price");
        }
    }
    function getDetails(filterItem, pageNo) {
        if (filterItem == undefined) filterItem['NeedSearch'] = false;
        if (pageNo == undefined) pageNo = 1;

        filterItem['pageNo'] = pageNo;
        filterItem['perPageRecord'] = records_per_page;

        axios.post("/api/live-product/live-portal", filterItem)
            .then(function (response) {
                masterData = response.data;
                $("#ParentDiv .childDiv").not(".mainChild").remove();
                for (var i = 0; i < masterData.Items.length; i++) {
                    $("#ParentDiv").append($(".mainChild").clone());
                    var lastChildDiv = $("#ParentDiv .childDiv:last");
                    lastChildDiv.removeClass("mainChild");
                    lastChildDiv.addClass("childDiv" + i);
                    lastChildDiv.css({
                        "display": "block"
                    });

                    var imgPath = "../Uploads/No_Image_Available.jpg";
                    if (masterData.Items[i].ImagePath) imgPath = ".." + masterData.Items[i].ImagePath;

                    lastChildDiv.find("figure").attr("itemprop", "associatedMedia");
                    lastChildDiv.find("img").attr("src", imgPath);
                    lastChildDiv.find("img").attr("alt", "");
                    lastChildDiv.find("a").attr("href", imgPath);
                    lastChildDiv.find("a").attr("cid", String(masterData.Items[i].FirmConceptMasterID));
                    lastChildDiv.find("a").attr("data-width", "2500");
                    lastChildDiv.find("a").attr("data-height", "1667");
                    lastChildDiv.find("a").attr("target", "_blank");
                    lastChildDiv.find("a").attr("itemprop", "contentUrl");
                    lastChildDiv.find("a").attr("data-caption", masterData.Items[i].HangerRemarks);

                    lastChildDiv.find("#ref").text('Ref: ' + masterData.Items[i].ReferenceNo);
                    lastChildDiv.find("#qty").text(masterData.Items[i].QtyInPcs + ' Pcs ');

                    lastChildDiv.find("#concept").text('Concept No: ' + masterData.Items[i].ConceptNo);
                    var fabrication = (masterData.Items[i].CommercialName == "") ? masterData.Items[i].TechnicalName : masterData.Items[i].CommercialName;
                    lastChildDiv.find("#fabric").text('Fabric: ' + fabrication);

                    var colorName = masterData.Items[i].ColorName ? masterData.Items[i].ColorName : "";
                    lastChildDiv.find("#colorName").text('Color: ' + colorName);

                    var composition = (masterData.Items[i].FinalComposition) ? masterData.Items[i].FinalComposition : masterData.Items[i].Composition;
                    setDotFormat(composition, "composition", 30, lastChildDiv);

                    var gsm = (masterData.Items[i].FinalGSM == 0) ? masterData.Items[i].Gsm : masterData.Items[i].FinalGSM;
                    lastChildDiv.find("#weight").text('Weight: ' + gsm);

     
                    setDotFormat(masterData.Items[i].HangerRemarks, "comments", 30, lastChildDiv);


                    lastChildDiv.find("#btnPriceRequestPopUp").attr("fid", masterData.Items[i].LPFormID);
                    lastChildDiv.find("#btnViewPriceRequest").attr("fid", masterData.Items[i].LPFormID);
                    lastChildDiv.find("#btnViewPriceSuggest").attr("fid", masterData.Items[i].LPFormID);
                    lastChildDiv.find("#btnSendMailRemainder").attr("fid", masterData.Items[i].LPFormID);
                    lastChildDiv.find("#lblRequestDateParent").attr("fid", masterData.Items[i].LPFormID);
                    lastChildDiv.find("#divBtnParent").attr("fid", masterData.Items[i].LPFormID);

                    if (masterData.Items[i].ConceptNo == "22020016") {
                        //debugger;
                    }

                    var totalPrice = 0;

                    if (masterData.Items[i].IsBuyerSpecific) {
                        masterData.Items[i].PriceRequestBuyers.map(x => {
                            totalPrice += x.Price;
                        });
                    } else {
                        totalPrice = masterData.Items[i].Price;
                    }

                    if (masterData.Items[i].LPPRID1 == 0) {
                        lastChildDiv.find("#btnSendMailRemainder").hide();
                    } else {
                        lastChildDiv.find("#lblRequestDateParent .lblRequestDate").text(getDate(masterData.Items[i].RequestDate));
                        var validityDate = getDate(masterData.Items[i].ValidUpToDate);
                        if (validityDate != "" && totalPrice > 0) {
                            lastChildDiv.find("#validUpToDate").text('Validity: ' + validityDate);
                        }
                        var buyers = masterData.Items[i].PriceRequestBuyers.map(x => x.BuyerName).join(", ");
                        if (buyers.length > 0) setDotFormat(buyers, "buyer", 30, lastChildDiv);
                    }
                    var isSend = masterData.Items[i].LPPRID > 0 ? true : false;

                    if (masterData.Items[i].LPPRID == 0) {
                        lastChildDiv.find("#btnViewPriceRequest").hide();
                    }
                    if (masterData.Items[i].IsBuyerSpecific) {
                        lastChildDiv.find("#price").replaceWith("<label id='price' style='cursor:pointer;text-decoration:underline;color:blue;' lppid=" + masterData.Items[i].LPPRID1 + " data-toggle='modal' data-target='#priceReqViewModal'>Last Price</label>");
                        lastChildDiv.find("#price").click(function () {
                            openModalPriceRequestPopup($(this).attr("lppid"), false);
                        });
                    } else if (!masterData.Items[i].IsBuyerSpecific) {
                        if (masterData.Items[i].Price > 0) {
                            lastChildDiv.find("#price").text('Last Price: ' + masterData.Items[i].Price);
                        }
                    }

                    if (isSend) {
                        lastChildDiv.find("#price").text("");
                        lastChildDiv.find("#lblRequestDateParent").show();
                    } else {
                        lastChildDiv.find("#lblRequestDateParent").hide();
                    }

                    lastChildDiv.find("#btnViewPriceRequest").attr("lppid", masterData.Items[i].LPPRID);
                    lastChildDiv.find("#btnViewPriceSuggest").attr("lppid", masterData.Items[i].LPPRID1);
                    lastChildDiv.find("#btnSendMailRemainder").attr("lppid", masterData.Items[i].LPPRID);

                    if (isSend || masterData.Items[i].LPPRID1 == 0) {
                        lastChildDiv.find("#btnViewPriceSuggest").hide();
                    } else {
                        lastChildDiv.find("#btnViewPriceSuggest").show();
                    }

                    lastChildDiv.find("#btnPriceRequestPopUp").click(function () {
                        openModalPriceRequest($(this).attr("fid"));
                    });
                    lastChildDiv.find("#btnViewPriceRequest").click(function () {
                        openModalPriceRequestPopup($(this).attr("lppid"), true);
                    });
                    lastChildDiv.find("#btnViewPriceSuggest").click(function () {
                        openModalPriceRequestPopup($(this).attr("lppid"), false);
                    });
                    lastChildDiv.find("#btnSendMailRemainder").click(function () {
                        sendMailRemainder($(this).attr("lppid"), $(this).attr("fid"));
                    });
                    changeBtnRequestPrice(masterData.Items[i].LPFormID, isSend);
                }
                getImageList();

                if (masterData.Items.length > 0)
                    toalRecords = masterData.Items[0].TotalRows;
                changePage(pageNo);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function setDotFormat(propValue, tagName, length, selector) {
        var rem = propValue,
            lblText = tagName.charAt(0).toUpperCase() + tagName.slice(1).toLowerCase();
        if (propValue.length > length) {
            rem = propValue.substring(0, length) + "...";
            selector.find("#" + tagName).attr("title", lblText + ': ' + propValue);
        }
        selector.find("#" + tagName).text(lblText + ': ' + rem);
    }
    function InitializeFilter() {
        var filterCompbo = $("#filterCombo").select2({
            tags: true,
            tokenSeparators: [",", " "]
        });
        var filterHeaderID = "ReferenceNo,ConceptNo,CommercialName,Composition,Gsm,Remarks";
        filterHeaderID = filterHeaderID.split(',');
        var filterHeaderName = "Reference No,Concept No,Fabric,Composition,GSM,Remarks";
        filterHeaderName = filterHeaderName.split(',');
        for (var i = 0; i < filterHeaderID.length; i++) {
            if (filterCompbo.find("option[value='" + filterHeaderID[i] + "']").length) {
            } else {
                var newOption = new Option(filterHeaderName[i], filterHeaderID[i], true, true);
                filterCompbo.append(newOption).val(null).trigger('change');
            }
        }

        filterCompbo.change(function () {
            var comboData = $("#filterCombo").select2('data');
            if (comboData.length) $("#Filter").fadeIn();
            else $("#Filter").fadeOut();

            for (var i = 0; i < comboData.length; i++) {
                if ($('#txtFilterGridID' + comboData[i].id).length == 0) {
                    var filterTemplate = "<div id='filterParent" + comboData[i].id + "' class='filterTxtClass' style='float:left;width:150px'><label>" + comboData[i].text + "</label><input id='txtFilterGridID" + comboData[i].id + "' type='text' class='inputTxtClassLPL' placeholder='Type and Enter'/></div>";
                    $("#filterArea").append(filterTemplate);
                }
            }
            //remove unselect data
            $(".filterTxtClass").each(function () {
                var fid = $(this).attr('id');
                var d = $.grep(comboData, function (h) {
                    return 'filterParent' + h.id == fid;
                });
                if (d.length == 0) {
                    $(this).remove();
                }
            });
        })
    }

    function getFilter() {
        var fd = $("#filterCombo").select2('data');

        var filterItem = {};
        for (var i = 0; i < fd.length; i++) {
            var colName = fd[i].id;
            var colValue = $('#txtFilterGridID' + fd[i].id).val();
            if (colName != undefined && colValue != '') {
                filterItem[colName] = colValue;
            }
        }
        filterItem['NeedSearch'] = true;
        return filterItem;
    }

    function GridFilter() {
        var filterItem = getFilter();
        current_page = 1;
        getDetails(filterItem, current_page);
    }

    function prevPage() {
        if (current_page > 1) {
            current_page--;
            var filterItem = getFilter();
            getDetails(filterItem, current_page);
        }
    }

    function nextPage() {
        if (current_page < numPages()) {
            current_page++;
            var filterItem = getFilter();
            getDetails(filterItem, current_page);
        }
    }

    function changePage(page) {
        var btn_next = document.getElementById("btn_next");
        var btn_prev = document.getElementById("btn_prev");
        var page_span = document.getElementById("page");
        var pageRecord_span = document.getElementById("pageRecord");

        var totalPage = numPages();
        if (page < 1) page = 1;
        if (page > totalPage) page = totalPage;
        page_span.innerHTML = page + " of " + totalPage;
        var currentRecordFrom = page * records_per_page;
        if (page == 1)
            currentRecordFrom = 1;
        else {
            currentRecordFrom = ((page - 1) * records_per_page) + 1;
        }
        var currentRecordTo = page * records_per_page;
        if (currentRecordTo > toalRecords) {
            pageRecord_span.innerHTML = currentRecordFrom + "-" + toalRecords + " of " + toalRecords;
        } else if (currentRecordTo == toalRecords) {
            pageRecord_span.innerHTML = currentRecordFrom + " of " + toalRecords;
        }
        else {
            pageRecord_span.innerHTML = currentRecordFrom + "-" + currentRecordTo + " of " + toalRecords;
        }

        if (page == 1) {
            //btn_prev.style.visibility = "hidden";
            btn_next.style.disable = true;
        } else {
            //btn_prev.style.visibility = "visible";
            btn_next.style.disable = false;
        }

        if (page == totalPage) {
            //btn_next.style.visibility = "hidden";
            btn_next.style.disable = true;
        } else {
            //btn_next.style.visibility = "visible";
            btn_next.style.disable = false;
        }
    }

    function numPages() {
        return Math.ceil(toalRecords / records_per_page);
    }
    function getDate(date) {
        if (date != null) {
            date = new Date(date);
            const formattedDate = date.toLocaleDateString('en-GB', {
                day: 'numeric', month: 'short', year: 'numeric'
            }).replace(/ /g, '-');
            return formattedDate;
        }
        return "";
    }
})();
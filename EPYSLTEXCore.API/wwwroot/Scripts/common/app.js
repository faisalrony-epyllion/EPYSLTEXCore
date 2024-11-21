// Please follow these rules to update this page
// 
// Thanks

'use strict'

var rootPath;
var HasAutoNumber = false;
var CanInsert = false;
var CanUpdate = false;
var CanDelete = false;
var ImageFileLength, PICINID, SupplierID, BuyerIds;
var currentTab;
var template = '';
var activeMenu = false;
const FILTERCONTROLPLACEHOLDER = "Type & Enter for Search";
var $mainTab;
var $mainTabContent;
 
var globalControllerName = 'home';
var globalActionName = 'index';

$(document).ready(function () {
  

    rootPath = window.location.protocol + '//' + window.location.host;

    toastr.options.escapeHtml = true;

    $.fn.editable.defaults.mode = 'inline';
    $.fn.editable.defaults.onblur = "submit";

    axios.defaults.headers.post['Content-Type'] = 'application/x-www-form-urlencoded';
    if (localStorage.getItem("token")) {
        axios.defaults.headers.common['Authorization'] = "Bearer " + localStorage.getItem("token");

    }

    //getBookingAnalysisBookingAcknowledgementCountALL();
    loadProgressBar();
 
    axios.interceptors.request.use(function (config) {
        if (config.method === "post") {
            HoldOn.open({
                theme: "sk-circle",
                message: "Please wait while we process your request.",
            });
        }
        return config;
    }, function (error) {
        HoldOn.close();
        return Promise.reject(error);
    });

    axios.interceptors.response.use(function (config) {
        HoldOn.close();
        return config;
    }, function (error) {
        HoldOn.close();
        return Promise.reject(error);
    });
 
    GetMenus(constants.APPLICATION_ID);

    $('[data-toggle="tooltip"]').tooltip({ html: true });

    $mainTab = $("#mainTab");
    $mainTabContent = $("#mainTabContent");
    //when ever any tab is clicked this method will be call
    $mainTab.on("click", "a", function (e) {
        e.preventDefault();
        $(this).tab('show');
        currentTab = $(this);

        var menuLink = e.target.hash.split('#')[1];
        var el = $("[data-action-name='" + menuLink + "']");
        $('.treeview li').removeClass('active');
        el.parent().addClass("active");
    });

    registerCloseEvent();
});

function logout() {
    axios.post('/api/Account/Logout')
        .then(function () {
            window.location.href = "/account/logoff";
        })
        .catch(function () {
            window.location.href = "/account/logoff";
        });
}

// #region Table Formatter
function priceFormatter(value, row, index) {
    var p = value.toFixed(2).split(".");
    var formatedPrice = p[0].split("").reduceRight(function (acc, value, i, orig) {
        var pos = orig.length - i - 1;
        return value + (pos && !(pos % 3) ? "," : "") + acc;
    }, "") + (p[1] ? "." + p[1] : "");

    if (row.CurrencyCode === "USD")
        formatedPrice = "$" + formatedPrice;
    return formatedPrice;
}

function responseHandler(res) {
    return JSON.parse(res);
}
// #endregion

// #region Table button click events
var actionEventsPO = {
    'click .view-po': function (e, value, row, inded) {
        var poNo = row.SPONo ? row.SPONo : row.PONo;
        viewPO(poNo, row.SubGroupName, row.IsSwo);
    }
}

function refreshTable(tableId, url) {
    $(tableId).bootstrapTable('refresh', { "url": url });
}
function reloadTableData(type, tableId) {
    $.get("/PI/GetSPOMasterForNewPI?supplierId=" + SupplierID + "&editType=" + type, function (response) {
        $(tableId).bootstrapTable('load', JSON.parse(response));
    });
}
// #endregion

// #region Tree Plugin Initialization
/* Tree()
 * ======
 * Converts a nested list into a multilevel
 * tree view menu.
 *
 * @Usage: $('.my-menu').tree(options)
 *         or add [data-widget="tree"] to the ul element
 *         Pass any option as data-option="value"
 */
+function ($) {
    'use strict';

    var DataKey = 'lte.tree';

    var Default = {
        animationSpeed: 500,
        accordion: true,
        followLink: false,
        trigger: '.nav-item a'
    };

    var Selector = {
        tree: '.tree',
        treeview: '.treeview',
        treeviewMenu: '.treeview-menu',
        open: '.menu-open, .active',
        li: 'li',
        data: '[data-widget="treeview"]',
        active: '.active'
    };

    var ClassName = {
        open: 'menu-open',
        tree: 'tree'
    };

    var Event = {
        collapsed: 'collapsed.tree',
        expanded: 'expanded.tree'
    };

    // Tree Class Definition
    // =====================
    var Tree = function (element, options) {
        this.element = element;
        this.options = options;

        $(this.element).addClass(ClassName.tree);

        $(Selector.treeview + Selector.active, this.element).addClass(ClassName.open);

        this._setUpListeners();
    };

    Tree.prototype.toggle = function (link, event) {
        var treeviewMenu = link.next(Selector.treeviewMenu);
        var parentLi = link.parent();
        var isOpen = parentLi.hasClass(ClassName.open);

        if (!parentLi.is(Selector.treeview)) {
            return;
        }

        if (!this.options.followLink || link.attr('href') === '#') {
            event.preventDefault();
        }

        if (isOpen) {
            this.collapse(treeviewMenu, parentLi);
        } else {
            this.expand(treeviewMenu, parentLi);
        }
    };

    Tree.prototype.expand = function (tree, parent) {
        var expandedEvent = $.Event(Event.expanded);

        if (this.options.accordion) {
            var openMenuLi = parent.siblings(Selector.open);
            var openTree = openMenuLi.children(Selector.treeviewMenu);
            this.collapse(openTree, openMenuLi);
        }

        parent.addClass(ClassName.open);
        tree.slideDown(this.options.animationSpeed, function () {
            $(this.element).trigger(expandedEvent);
        }.bind(this));
    };

    Tree.prototype.collapse = function (tree, parentLi) {
        var collapsedEvent = $.Event(Event.collapsed);

        //tree.find(Selector.open).removeClass(ClassName.open);
        parentLi.removeClass(ClassName.open);
        tree.slideUp(this.options.animationSpeed, function () {
            //tree.find(Selector.open + ' > ' + Selector.treeview).slideUp();
            $(this.element).trigger(collapsedEvent);
        }.bind(this));
    };

    // Private

    Tree.prototype._setUpListeners = function () {
        var that = this;

        $(this.element).on('click', this.options.trigger, function (event) {
            that.toggle($(this), event);
          
            if ($(this).parent().hasClass("nav-item")) {            
                return;
            }
                

            $('.treeview li').removeClass('active');
            $(this).parent().addClass("active");
            $('.treeview').removeClass('menu-open');
            $(this).closest('li.treeview').addClass("menu-open");

            var actionName = $(this).data("action-name");
            var controllerName = $(this).data("controller-name");
            var navUrlName = $(this).data("navurl-name");
            var pageType = $(this).data("page-type");
            var pageName = $(this).data("page-name");
            if (pageName) pageName = pageName.split(' ').join('');
            else pageName = actionName;

            var menuId = $(this).data("menu-id");
            var tabCaption = $(this).text().trim();
            var isExists = $('#mainTab a[href="#' + pageName + '"]');
            if (pageType == 'Report')
                return;
            else if (pageType == 'NF') {
                if (isExists.length === 0) GetNotFoundViewMarkup(pageName, tabCaption);
                else showTab(pageName);
            }
            else if (pageType == 'CI') {
                if (isExists.length === 0) getCommonInterfaceMarkup(controllerName, actionName, menuId, pageName, tabCaption, navUrlName);
                else showTab(pageName);
            }
            else {
                if (isExists.length === 0) GetViewMarkup(controllerName, actionName, menuId, pageName, tabCaption, navUrlName);
                else {
                    showTab(pageName);
                    var tableId = $(this).data("table-id");
                    var url = $(this).data("refresh-url");
                    if (tableId && url)
                        refreshTable(tableId, url);
                }
            }
        });
    };

    // Plugin Definition
    // =================
    function Plugin(option) {
        return this.each(function () {
            var $this = $(this);
            var data = $this.data(DataKey);

            if (!data) {
                var options = $.extend({}, Default, $this.data(), typeof option === 'object' && option);
                $this.data(DataKey, new Tree($this, options));
            }
        });
    }

    var old = $.fn.tree;

    $.fn.tree = Plugin;
    $.fn.tree.Constructor = Tree;

    // No Conflict Mode
    // ================
    $.fn.tree.noConflict = function () {
        $.fn.tree = old;
        return this;
    };

    //// Tree Data API
    //// =============
    //$(window).on('load', function () {
    //    $(Selector.data).each(function () {
    //        Plugin.call($(this));
    //    });
    //});

}(jQuery);
// #endregion 

// #region Implelmentation of Tab View
//shows the tab with passed content div id..paramter tabid indicates the div where the content resides
function showTab(tabId) {
    $('#mainTab a[href="#' + tabId + '"]').tab('show');
}
//return current active tab
function getCurrentTab() {
    return currentTab;
}

//This function will create a new tab here and it will load the url content in tab content div.
function craeteNewTabAndLoadUrl(parms, url, loadDivSelector) {

    $("" + loadDivSelector).load(url, function (response, status, xhr) {
        if (status === "error") {
            var msg = "Sorry but there was an error getting details ! ";
            $("" + loadDivSelector).html(msg + xhr.status + " " + xhr.statusText);
            $("" + loadDivSelector).html("Load Ajax Content Here...");
        }
    });

}

function getElement(selector) {
    var tabContentId = $currentTab.attr("href");
    return $("" + tabContentId).find("" + selector);

}

function removeCurrentTab() {
    var tabContentId = $currentTab.attr("href");
    $currentTab.parent().remove(); //remove li of tab
    $('#mainTab a:last').tab('show'); // Select first tab
    $(tabContentId).remove(); //remove respective tab content
}

function registerCloseEvent() {
    $(".closeTab").click(function () {
        var tabContentId = $(this).parent().attr("href");
        $(this).parent().parent().remove(); //remove li of tab
        $('#mainTab a:last').tab('show'); // Select first tab
        $(tabContentId).remove(); //remove respective tab content
    });
}

function GetViewMarkup(controllerName, actionName, menuId, pageName, tabCaption, navUrlName) {

    var url = "/" + controllerName + "/" + actionName + "?menuId=" + menuId + "&pageName=" + pageName + "&navUrlName=" + navUrlName;
    axios.get(url).then(function (response) {
        $mainTab.append('<li class="p-2 active bg-info" style=" border: 1px solid #ccc;box-shadow: 0px 4px 6px rgba(0, 0, 0, 0.1);"><a href="#' + pageName + '">' + tabCaption + '&nbsp;<span class="close closeTab fa fa-times fa-lg pt-1"  type="button"><i class="icon-remove fa-lg"></i></span></a></li>');
        var markup = '<div class="tab-pane" id="' + pageName + '">' + response.data + '</div>';
        $mainTabContent.append(markup);
        showTab(pageName);
        registerCloseEvent();

        var scriptPath = '/Scripts/' + controllerName + '/' + actionName + '.js?menuId=' + menuId + '&version=' + $("#versionNumer").val();
        if ($('script[src="' + scriptPath + '"]').length === 0) {
            var s = document.createElement('script');
            s.setAttribute('src', scriptPath);
            $("#" + pageName).append(s);
        }

        var $formEl = $(pageConstants.FORM_ID_PREFIX + pageName + "-" + menuId);
        initCommonControls($formEl);
    }).catch(showResponseError);
}

function clickAccountNavigation(event) {
    var target = event.currentTarget;
    var actionName = target.dataset.actionName;
    var isExists = $('#mainTab a[href="#' + actionName + '"]');
    if (isExists.length === 0) {
        var tabCaption = target.innerText.trim();
        $mainTab.append('<li><a href="#' + actionName + '">' + tabCaption + '<span class="close closeTab fa fa-times" type="button"><i class="icon-remove"></i></span></a></li>');

        var controllerName = target.dataset.controllerName;
        getAccountViewMarkup(controllerName, actionName);
    }
    else {
        showTab(actionName);
    }
}

function getAccountViewMarkup(controller, actionName) {

    $.get("/" + controller + "/" + actionName, function (htmlResponse) {
        var markup = '<div class="tab-pane" id="' + actionName + '">' + htmlResponse + '</div>';
        $mainTabContent.append(markup);
        showTab(actionName);
        registerCloseEvent();

        var scriptPath = '/Scripts/' + controller + '/' + actionName + '.js' + "?version=" + $("#versionNumer").val();
        if ($('script[src="' + scriptPath + '"]').length === 0) {
            var s = document.createElement('script');
            s.setAttribute('src', scriptPath);
            $("#" + actionName).append(s);
        }
    });
}

function GetNotFoundViewMarkup(tabid, tabCaption) {
    axios.get("/home/notfoundpartial")
        .then(function (response) {
            $mainTab.append('<li><a href="#' + pageName + '">' + tabCaption + '<span class="close closeTab fa fa-times" type="button"><i class="icon-remove"></i></span></a></li>');
            var markup = '<div class="tab-pane" id="' + tabid + '">' + response.data + '</div>';
            $mainTabContent.append(markup);
            showTab(tabid);
            registerCloseEvent();
        })
        .catch(showResponseError);
}

function getCommonInterfaceMarkup(controllerName, actionName, menuId, pageName, tabCaption, navUrlName) {
    localStorage.setItem("current_common_interface_menuid", menuId);
    var url = "/" + controllerName + "/" + actionName + "?menuId=" + menuId + "&pageName=" + pageName + "&navUrlName=" + navUrlName;
   // var url = `/admin/${actionName}?menuId=${menuId}`;
    axios.get(url).then(function (response) {
        $mainTab.append('<li><a href="#' + pageName + '">' + tabCaption + '<span class="close closeTab fa fa-times" type="button"><i class="icon-remove"></i></span></a></li>');
        var markup = '<div class="tab-pane" id="' + pageName + '">' + response.data + '</div>';

        $mainTabContent.append(markup);
        showTab(pageName);
        registerCloseEvent();
        const result = navUrlName.split("_");
        var folderName = result[0];
        var jsFileName = result[1];
        var scriptPath = '/Scripts/' + folderName + '/' + jsFileName + '.js?menuId=' + menuId + '&version=' + $("#versionNumer").val();
      
        if ($('script[src="' + scriptPath + '"]').length === 0) {
            var s = document.createElement('script');
            s.setAttribute('src', scriptPath);
            $("#" + pageName).append(s);
        }
    }).catch(showResponseError);
}

function GetMenus(applicationId) {
    axios.get("/api/MenuAPI/GetAllMenu/" + applicationId)
        .then(function (response) {

            generateMenu(response.data);
            $(".sidebar-menu").append(template);

            $(".sidebar-menu").tree();
        })
        .catch(showResponseError);
}

function generateMenu(menuList) {


  
    $.each(menuList, function (i, item) {
        if (!item.childs.length) {
            
            if (!item.navigateUrl) return true;
            var navProperties = item.navigateUrl.split('/');

            // Replace all occurrences of '/' with '_'
            var updatednavigateUrl = item.navigateUrl.replace(/\//g, '_');
         
            if (navProperties[1] == 'notfoundpartial') {
                template += '<li><a href="#!" class="nav-link" data-navurl-name="' + updatednavigateUrl + '" data-controller-name="' + globalControllerName + '" data-action-name="' + globalActionName + '" data-page-name="' + item.pageName + '" data-menu-id="' + item.menuId + '" data-page-type="NF"><i class="nav-icon fa fa-circle-o"></i> <p>' + item.menuCaption + '</p></a></li>';
            }
            else if (item.useCommonInterface) {
                template += '<li><a href="#!" class="nav-link" data-navurl-name="' + updatednavigateUrl + '" data-controller-name="' + globalControllerName + '" data-action-name="' + globalActionName + '" data-page-name="' + item.pageName + '" data-menu-id="' + item.menuId + '" data-page-type="CI"><i class="nav-icon fa fa-circle-o"></i> <p>' + item.menuCaption + '</p></a></li>';
            }
            else if (item.pageName == 'ReportViewer') {
                var path = rootPath + '/reports/index';
                template += '<li><a class="nav-link" href="' + path + '" target="_blank" data-page-type="Report"><i class="nav-icon fa fa-circle-o"></i> <p>' + item.menuCaption + '</p></a></li>';
            }
            else {
                template += '<li><a class="nav-link" href="#!" data-navurl-name="' + updatednavigateUrl + '"  data-controller-name="' + globalControllerName + '" data-action-name="' + globalActionName + '" data-table-id="' + navProperties[2] + '" data-page-name="' + item.pageName + '" data-menu-id="' + item.menuId + '"><i class="nav-icon fa fa-circle-o"></i> <p>' + item.menuCaption + '</p></a></li>';
            }
       
            activeMenu = false;
            return true;
        }
        else {
  
            activeMenu = item.menuId == 509 ? true : false;
            var active = activeMenu ? "active" : "";
            template += '<li class="nav-item' + active + '">';
            template += '<a href="#" class="nav-link">'
                + '<i class="nav-icon fa fa-circle"></i><p>' + item.menuCaption + '<i class="right fa fa-angle-left"></i></p>'
            //    + '<p class="pull-right-container">'
           
              //  + '</p>'
                + '</a>';
            template += '<ul class="nav nav-treeview treeview-menu">';
            activeMenu = false;
        }

        generateMenu(item.childs);
    
        template += '</ul></li>';

        
    });
}

// #region Constants
var constants = Object.freeze({
    APPLICATION_ID:11,
    LOAD_ERROR_MESSAGE: "An error occured in fetching your data",
    SUCCESS_MESSAGE: "Your record saved successfully!",
    PROPOSE_SUCCESSFULLY: "Your record has been sent for approval!",
    APPROVE_SUCCESSFULLY: "Your record approved successfylly!",
    ACCEPTED_SUCCESFULLY: "Your record accepted successfully!",
    REJECT_SUCCESSFULLY: "Your record rejected successfully!",
    UNAPPROVE_SUCCESSFULLY: "Your record unapproved successfully",
    REVISE_BOOKING: "Your booking is under Revised Stage!!!",
    GMT_ERP_BASE_PATH: "https://gmterp.epylliongroup.com",
    GMT_ERP_LOCAL_PATH: "http://192.168.10.231:8080",
    MASTER_GRID_LOADING_TIME: 300000 //5 Min
});

var pageConstants = Object.freeze({
    PAGE_ID_PREFIX: "#",
    DIV_TBL_ID_PREFIX: "#divtbl",
    TOOLBAR_ID_PREFIX: "#toolbar",
    MASTER_TBL_ID_PREFIX: "#tbl",
    CHILD_TBL_ID_PREFIX: "#tblChild",
    REVISED_CHILD_TBL_ID_PREFIX: "#tblRevisedChild",
    FORM_ID_PREFIX: "#form",
    DIV_DETAILS_ID_PREFIX: "#divDetails",
    CHILD_BOOKING_TBL_ID_PREFIX: "#tblChildBooking",
    STOCK_INFO_PREFIX: "#tblStockInfo",
    STOCK_SUMMARY_PREFIX: "#tblStockSummary"
});

var statusConstants = Object.freeze({
    NONE: 0,
    ALL: 1,
    PENDING: 2,
    COMPLETED: 3,
    PROPOSED: 4,
    APPROVED: 5,
    ACKNOWLEDGE: 6,
    PARTIALLY_COMPLETED: 7,
    ALL_STATUS: 8,
    REJECT: 9,
    REVISE: 10,
    ADDITIONAL: 11,
    RETURN_PROPOSE_PRICE: 12,
    ReTest: 13,
    UN_ACKNOWLEDGE: 14,
    PendingReceiveCI: 15,
    PendingReceivePO: 16,
    UN_APPROVE: 17,
    AWAITING_PROPOSE: 18,
    PENDING_BATCH: 19,
    ACTIVE: 22,
    IN_ACTIVE: 23,
    EXECUTED: 24,
    YDP_PENDING: 25,
    YDP_COMPLETE: 26,
    YDQC_PENDING: 27,
    YDQC_COMPLETE: 28,
    YDQC_FAIL: 29,
    NEW: 30,
    REPORT: 31,
    EDIT: 32,
    CHECK: 33,
    PROPOSED_FOR_APPROVAL: 34,
    CHECK_REJECT: 35,
    PROPOSED_FOR_ACKNOWLEDGE: 36,
    PROPOSED_FOR_ACKNOWLEDGE_ACCEPTENCE: 37,
    ACKNOWLEDGE_ACCEPTENCE: 38,
    INDENT_PENDING: 39,
    REJECT_REVIEW: 40,
    PRE_PENDING: 41,
    POST_PENDING: 42,
    REVISE_FOR_ACKNOWLEDGE: 43,
    PASS: 44,
    FAIL: 45,
    CONFIRM: 46,
    REWORK: 47,
    HOLD: 48,
    PENDING_CONFIRMATION: 49,
    PENDING_GROUP: 50,
    PendingReceiveSF: 51,
    DRAFT: 52,
    APPROVED_DONE: 53,
    APPROVED_PMC: 54,
    REJECT_PMC: 55,
    APPROVED_Allowance: 56,
    REJECT_Allowance: 57,
    OTHERS: 58,
    APPROVED2: 59,
    INTERNAL_REJECTION: 60,
    REVISE2: 61,
    PENDING2: 62,
    PENDING3: 63,
    COMPLETED2: 64,
    COMPLETED3: 65,
    RETURN: 66,
    PENDING_REVISE: 67,
    PENDING_RETURN_CONFIRMATION: 68,
    PENDING_EXPORT_DATA: 69,
    EXPORT_DATA: 70,
    ADDITIONAL_INTERNAL_REJECTION: 71,
    ADDITIONAL_APPROVED_OPERATION_HEAD: 72,
});

var freeConceptStatus = Object.freeze({
    All: 1,
    Live: 2,
    Preserved: 3,
    Dropped: 4,
    SourcingPending: 5,
    YDPending: 6,
    KnitPending: 7,
    BatchPending: 8,
    DyeingPending: 9,
    FinishPending: 10,
    TestPending: 11,
    WaitingForLivePending: 12
});

var pageNameConstants = Object.freeze({
    CPR: "CPR",
    FPR: "FPR"
});

var contactCategoryConstants = Object.freeze({
    KNITTING_UNIT: 'Knitting Unit',
    SUB_CONTUCT: 'Knitting Sub Contractor',
});

var LcType = Object.freeze({
    Select: 0,
    At_Sight: 1,
    Usance: 2
});

var entityState = Object.freeze({
    ADDED: 4,
    MODIFIED: 16
});

var bookingTypeConstants = Object.freeze({
    SAMPLE: 0,
    BULK: 1,
    REVISED: 2
});

var fiberTypeConstants = Object.freeze({
    BLENDED: "Blended",
    COLOR_MELANGE: "Color Mellange [CM]"
});

var firmConceptImageTypes = Object.freeze({
    PRODUCT: 1,
    NEEDLE: 2,
    CAM: 3
});

var subGroupNames = Object.freeze({
    FABRIC: "Fabric",
    COLLAR: "Collar",
    CUFF: "Cuff",
    DYES: "Dyes",
    CHEMICALS: "Chemicals",
    YARN_NEW: "Yarn New"
});

var knittingProgramType = Object.freeze({
    CONCEPT: 1,
    BDS: 2,
    BULK: 3
});

var conceptType = Object.freeze({
    ColorBased: "Color Based",
    StructureBased: "Structure Based"
});

var gridTypes = Object.freeze({
    EJ2: "ej2",
    BOOTSTRAP: "bootstrap-table"
})

var poForNames = Object.freeze({
    TEXTILE_TILE_ADVANCE: "Textile Advance",
    SPECIFIC_ORDER: "Specific Order",
    RE_ORDER_LEVEL: "Re-Order Level",
    SCD_ADVANCE: "SCD Advance",
    MNM_PROJECTION: "MNM Projection",
    INPUT_EWO_NO: "Input EWO No",
    OTHERS: "Others"
})

var prFrom = Object.freeze({
    CONCEPT: "Concept",
    BDS: "BDS",
    BULK_BOOKING: "Bulk Booking",
    PROJECTION_YARN_BOOKING: "Projection Yarn Booking",
    FABRIC_PROJECTION_YARN_BOOKING: "Fabric Projection Yarn Booking"
});

var ReceiveNoteType = Object.freeze({
    MRIR: 1,
    GRN: 2,
    MRN: 3
});
// #endregion
// Please follow these rules to update this page
// 
// Thanks

'use strict'

var rootPath;

var reportRootPath;
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

    reportRootPath = "https://localhost:44311/";
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
        $(this).closest("li").siblings().removeClass('active');
        $(this).closest("li").siblings().removeClass('bg-info');
        $(this).closest("li").addClass("active");
        $(this).closest("li").addClass("bg-info");
    });

    registerCloseEvent();

    $("#btnRightMenu").click(function () {
        setAnimate(false);
    });

    $("#btnLeftMenu").click(function () {
        setAnimate(true);
    });
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

            var menuParam = $(this).data("menu-param");

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
                if (isExists.length === 0) getCommonInterfaceMarkup(controllerName, actionName, menuId, pageName, menuParam, tabCaption, navUrlName);
                else showTab(pageName);
            }
            else {
                if (isExists.length === 0) GetViewMarkup(controllerName, actionName, menuId, pageName, menuParam, tabCaption, navUrlName);
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
    $('#mainTab').find('a:last').tab('show'); // Select first tab
    $(tabContentId).remove(); //remove respective tab content
}

function registerCloseEvent(menuId) {
    $(".closeTab" + menuId).click(function () {

        var tabIndex = $(this).closest('li').attr("tabIndex");
        var preTabIndex = tabIndex - 1;
        var isActive = $(this).closest('li').hasClass("active");
        var tabContentId = $(this).parent().find("a").attr("href");
        //$(this).parent().parent().remove(); //remove li of tab
        $('#mainTab').find('a:last').tab('show'); // Select first tab
        $(tabContentId).remove(); //remove respective tab content
        $("#mainTab").find("li[tabIndex=" + tabIndex + "]").remove();
        if (isActive) {
            $("#mainTab").find("li").removeClass("bg-info").removeClass("active");
            $("#mainTab").find("li[tabIndex=" + preTabIndex + "]").addClass("bg-info").addClass("active");
        }
        var pageName = $("#mainTab").find("li.active").attr("pageName");
        if (typeof pageName === "undefined") pageName = "dashboard";
        $('#mainTab a[href="#' + pageName + '"]').tab('show');
        resetTabIndex();

        setAnimate(true);
    });

    //$(".closeTab").click(function () {
    //    
    //    var tabIndex = $(this).closest('li').attr("tabIndex");
    //    var preTabIndex = tabIndex - 1;
    //    var isActive = $(this).closest('li').hasClass("active");

    //     var tabContentId = $(this).parent().find("a").attr("href");
    //    //$(this).parent().parent().remove(); //remove li of tab
    //    $('#mainTab a:last').tab('show'); // Select first tab
    //    $(tabContentId).remove(); //remove respective tab content
    //    $("#mainTab").find("li[tabIndex=" + tabIndex + "]").remove();
    //    if (isActive) {
    //        $("#mainTab").find("li").removeClass("bg-info").removeClass("active");
    //        $("#mainTab").find("li[tabIndex=" + preTabIndex + "]").addClass("bg-info").addClass("active");
    //    }
    //    resetTabIndex();
    //});
}


function setAnimate(isRightBtnClick) {
    var leftC = 100;
    var maxLeftC = 650;
    var leftValue = 0;

    if (!isRightBtnClick) {
        leftValue = parseInt($(".menu-box").css("left"));
        leftValue += leftC;
        if (leftValue > 0) {
            leftValue = 0;
        }
    }
    else {
        var tabLi = $("#mainTab").find(".tabLi").length;
        if (tabLi * leftC > maxLeftC) {
            leftValue = parseInt($(".menu-box").css("left"));
            if (isRightBtnClick) {
                leftValue += -leftC;
            }
        }
    }
    leftValue = leftValue + "px";
    $(".menu-box").animate({ left: leftValue });
}
function resetTabIndex() {
    var tabIndex = 0;
    $("#mainTab").find(".tabLi").each(function () {
        $(this).attr("tabIndex", tabIndex);
        tabIndex++;
    });
}

function GetViewMarkup(controllerName, actionName, menuId, pageName, menuParam, tabCaption, navUrlName) {
    $($mainTab[0]).children().removeClass('active');
    $($mainTab[0]).children().removeClass('bg-info');
    var url = "/" + controllerName + "/" + actionName + "?menuId=" + menuId + "&pageName=" + pageName + "&navUrlName=" + navUrlName + "&menuParam=" + menuParam;

    axios.get(url).then(function (response) {
        var len = $("#mainTab").find("li").length;

        $mainTab.append(`<li class="p-1 tabLi bg-info active" tabIndex=` + len + ` menu-id=` + menuId + ` style="box-shadow: 0px 4px 6px rgba(0, 0, 0, 0.1);">
                         
                                <a href="#` + pageName + `">` + tabCaption + `&nbsp;</a>
                         
                            <div>
                                 <span class="close closeTab closeTab` + menuId + ` fa fa-times fa-lg pt-1" type="button" title='Close this tab'>
                                    <i class="icon-remove fa-lg"></i>
                                </span>
                            </div>
                         </li>`);
        var markup = '<div class="tab-pane" id="' + pageName + '">' + response.data + '</div>';
        $mainTabContent.append(markup);
        showTab(pageName);
        registerCloseEvent(menuId);

        const result = navUrlName.split("_");
        var folderName = result[0];
        var jsFileName = result[1];
        var scriptPath = '/Scripts/' + folderName + '/' + jsFileName + '.js?menuId=' + menuId + '&version=' + $("#versionNumer").val();

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
    $($mainTab[0]).children().removeClass('active');
    $($mainTab[0]).children().removeClass('bg-info');
    var isExists = $('#mainTab a[href="#' + actionName + '"]');
    if (isExists.length === 0) {
        var len = $("#mainTab").find("li").length;

        var tabCaption = target.innerText.trim();
        $mainTab.append(`<li class="p-1 tabLi bg-info active" tabIndex=` + len + ` style="box-shadow: 0px 4px 6px rgba(0, 0, 0, 0.1);">
                      
                                <a href="#` + actionName + `">` + tabCaption + `</a>
                       
                            <div>
                                <span class="close closeTab closeTab` + menuId + ` fa fa-times" type="button">
                                    <i class="icon-remove"></i>
                                </span>
                            </div>
                          </li>`);

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
        registerCloseEvent(0);


        const result = navUrlName.split("_");
        var folderName = result[0];
        var jsFileName = result[1];
        var scriptPath = '/Scripts/' + folderName + '/' + jsFileName + '.js?menuId=' + menuId + '&version=' + $("#versionNumer").val();

        if ($('script[src="' + scriptPath + '"]').length === 0) {
            var s = document.createElement('script');
            s.setAttribute('src', scriptPath);
            $("#" + actionName).append(s);
        }
    });
}

function GetNotFoundViewMarkup(tabid, tabCaption) {
    $($mainTab[0]).children().removeClass('active');
    $($mainTab[0]).children().removeClass('bg-info');
    axios.get("/home/notfoundpartial")
        .then(function (response) {
            var len = $("#mainTab").find("li").length;

            $mainTab.append(`<li class="p-1 tabLi bg-info active" tabIndex=` + len + ` style="box-shadow: 0px 4px 6px rgba(0, 0, 0, 0.1);">
                           
                                    <a href="#` + pageName + `">` + tabCaption + `</a>
                         
                                <div>
                                    <span class="close closeTab closeTab` + menuId + ` fa fa-times fa-lg pt-1" type="button">
                                        <i class="icon-remove"></i>
                                    </span>
                                </div>
                             </li>`);


            var markup = '<div class="tab-pane" id="' + tabid + '">' + response.data + '</div>';
            $mainTabContent.append(markup);
            showTab(tabid);
            registerCloseEvent(menuId);
        })
        .catch(showResponseError);
}

function getCommonInterfaceMarkup(controllerName, actionName, menuId, pageName, menuParam, tabCaption, navUrlName) {
    $($mainTab[0]).children().removeClass('active');
    $($mainTab[0]).children().removeClass('bg-info');
    localStorage.setItem("current_common_interface_menuid", menuId);
    var url = "/" + controllerName + "/" + actionName + "?menuId=" + menuId + "&pageName=" + pageName + "&navUrlName=" + navUrlName;
    // var url = `/admin/${actionName}?menuId=${menuId}`;

    axios.get(url).then(function (response) {
        var len = $("#mainTab").find("li").length;

        $mainTab.append(`<li class="p-1 tabLi bg-info active" tabIndex=` + len + ` menu-id=` + menuId + ` pageName = ` + pageName + ` style="box-shadow: 0px 4px 6px rgba(0, 0, 0, 0.1);">
                   
                                <a href="#` + pageName + `">` + tabCaption + `</a>
                 
                            <div>
                                <span class="close closeTab closeTab` + menuId + ` fa fa-times fa-lg p-1" type="button">
                                        <i class="icon-remove fa-lg pt-1"></i>
                                </span>
                            </div>
                         </li>`);

        var markup = '<div class="tab-pane" id="' + pageName + '">' + response.data + '</div>';

        $mainTabContent.append(markup);
        showTab(pageName);
        registerCloseEvent(menuId);
        resetTabIndex();
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

function gotoReport() {  

    const url = reportRootPath + '/reports/GetReport?param=' + localStorage.getItem("token");
    window.open(url, "_blank"); 
}

function GetMenus(applicationId) {

    axios.get("/api/MenuAPI/GetAllMenu/" + applicationId)
        .then(function (response) {

            generateMenu(response.data);
            $(".sidebar-menu").append(template);
            $(".sidebar-menu").tree();
            //$(".sidebar-menu").css({
            //    "font-size": "12px !important"
            //});
            $(".menuLI").each(function () {
                var menuId = $(this).attr("menu-id");
                $(".menuLI[menu-id=" + menuId + "]").click(function () {
                    $("#mainTab").find("li").removeClass("bg-info").removeClass("active");
                    $("#mainTab").find("li[menu-id=" + menuId + "]").addClass("bg-info").addClass("active");
                });
            });
        })
        .catch(showResponseError);
}

function generateMenu(menuList) {

    $.each(menuList, function (i, item) {
        if (!item.Childs.length) {
            if (!item.NavigateUrl) return true;
            var navProperties = item.NavigateUrl.split('/');

            // Replace all occurrences of '/' with '_'
            var updatednavigateUrl = item.NavigateUrl.replace(/\//g, '_');

            if (navProperties[1] == 'notfoundpartial') {
                template += '<li menu-id=' + item.MenuId + ' class="menuLI"><a href="#!" class="nav-link" data-navurl-name="' + updatednavigateUrl + '" data-controller-name="' + globalControllerName + '" data-action-name="' + globalActionName + '" data-page-name="' + item.PageName + '" data-menu-id="' + item.MenuId + '" data-menu-param = "' + item.MenuParam + '" data-page-type="NF"><i class="nav-icon far fa-dot-circle"></i> <p>' + item.MenuCaption + '</p></a></li>';
            }
            else if (item.UseCommonInterface) {
                template += '<li menu-id=' + item.MenuId + ' class="menuLI"><a href="#!" class="nav-link" data-navurl-name="' + updatednavigateUrl + '" data-controller-name="' + globalControllerName + '" data-action-name="' + globalActionName + '" data-page-name="' + item.PageName + '" data-menu-id="' + item.MenuId + '" data-menu-param = "' + item.MenuParam + '" data-page-type="CI"><i class="nav-icon far fa-dot-circle"></i> <p>' + item.MenuCaption + '</p></a></li>';
            }
            else if (item.PageName == 'ReportViewer') {
              
                template += '<li menu-id=' + item.MenuId + ' class="menuLI"><a class="nav-link" target="_blank" onclick="gotoReport()" data-page-type="Report"><i class="nav-icon far fa-dot-circle"></i> <p>' + item.MenuCaption + '</p></a></li>';
            }
            else {
                template += '<li menu-id=' + item.MenuId + ' class="menuLI"><a class="nav-link" href="#!" data-navurl-name="' + updatednavigateUrl + '"  data-controller-name="' + globalControllerName + '" data-action-name="' + globalActionName + '" data-table-id="' + navProperties[2] + '" data-page-name="' + item.PageName + '" data-menu-id="' + item.MenuId + '" data-menu-param = "' + item.MenuParam + '"><i class="nav-icon far fa-dot-circle"></i> <p>' + item.MenuCaption + '</p></a></li>';
            }

            activeMenu = false;
            return true;
        }
        else {

            activeMenu = item.MenuId == 509 ? true : false;
            var active = activeMenu ? "active" : "";
            template += '<li menu-id=' + item.MenuId + ' class="nav-item' + active + '">';
            template += '<a href="#" class="nav-link">'
                + '<i class="nav-icon fa fa-circle"></i><p>' + item.MenuCaption + '<i class="right fa fa-angle-left"></i></p>'
                //    + '<p class="pull-right-container">'

                //  + '</p>'
                + '</a>';
            template += '<ul class="nav nav-treeview treeview-menu">';
            activeMenu = false;
        }

        generateMenu(item.Childs);

        template += '</ul></li>';


    });
}

// #region Constants
var constants = Object.freeze({
    APPLICATION_ID: 11,
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
    ROL_BASE_PENDING: 73,
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
    YARN_NEW: "Yarn Live"
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
    FABRIC_PROJECTION_YARN_BOOKING: "Fabric Projection Yarn Booking",
    ROL_BASE_BOOKING: "ROL Base Booking"
});

var ReceiveNoteType = Object.freeze({
    MRIR: 1,
    GRN: 2,
    MRN: 3
});
// #endregion
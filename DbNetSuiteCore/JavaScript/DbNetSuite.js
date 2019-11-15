/// <reference types="jquery" />
/// <reference types="notify" />
var Ajax = /** @class */ (function () {
    function Ajax() {
        /*
        $(document).on({
            ajaxStart: function () { $("body").addClass("loading"); },
            ajaxStop: function () { $("body").removeClass("loading"); }
        });
        $.ajaxSetup({
            complete: (xhr, status) => {
                if (xhr.getResponseHeader("X-Responded-JSON") !== null && JSON.parse(xhr.getResponseHeader("X-Responded-JSON")).status === "401") {
                    this.timeout();
                }
            },
            error: (jqxhr, settings, thrownError) => {
                if (jqxhr.status.toString() === "401") { // Login has expired so tell the user and reload the page which will cause a redirect to the login page
                    this.timeout();
                } else {
                    this.ajaxError(jqxhr);
                }
            }
        });
        */
    }
    Ajax.prototype.timeout = function () {
        var _this_1 = this;
        var options = {};
        options.message = "Login has timed out";
        options.callback = function () { return _this_1.reload(); };
        bootbox.alert(options);
    };
    Ajax.prototype.ajaxError = function (xhr) {
        console.log(JSON.stringify(xhr));
        alert(xhr.responseText);
    };
    Ajax.prototype.callServer = function (handler, data, callback) {
        var xhr = new XMLHttpRequest();
        var _this = this;
        xhr.open('POST', "handler.dbnetgrid?handler=" + handler);
        xhr.setRequestHeader('Content-Type', 'application/json');
        xhr.setRequestHeader("RequestVerificationToken", document.querySelector("body").getAttribute("xsrf-token"));
        xhr.onload = function () {
            if (xhr.status === 200) {
                callback(JSON.parse(xhr.responseText));
            }
            else if (xhr.status !== 200) {
                _this.ajaxError(xhr);
            }
        };
        xhr.send(JSON.stringify(data));
    };
    Ajax.prototype.reload = function () {
        window.location.reload();
    };
    Ajax.prototype.notifyInfo = function (message) {
        alert(message);
    };
    Ajax.prototype.notifyError = function (message) {
        alert(message);
    };
    Ajax.prototype.setAntiForgeryToken = function () {
        $("input[name='__RequestVerificationToken']").val($("body").attr("xsrf-token"));
    };
    return Ajax;
}());
/// <reference path="Ajax.ts" />
/// <reference path="Interfaces.ts" />
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var DbNetGrid = /** @class */ (function (_super) {
    __extends(DbNetGrid, _super);
    function DbNetGrid(configuration) {
        var _this = _super.call(this) || this;
        _this.configuration = configuration;
        _this.init();
        return _this;
    }
    DbNetGrid.prototype.init = function () {
        var _this = this;
        this.addCss('handler.dbnetsuite?handler=css');
        this.callServer("Init", this.configuration, function (response) { _this.initCallback(response); });
    };
    DbNetGrid.prototype.addCss = function (fileName) {
        var head = document.head;
        var link = document.createElement("link");
        link.type = "text/css";
        link.rel = "stylesheet";
        link.href = "" + this.baseUrl() + fileName;
        head.appendChild(link);
    };
    DbNetGrid.prototype.baseUrl = function () {
        var getUrl = window.location;
        return getUrl.protocol + "//" + getUrl.host + "/" + getUrl.pathname.split('/')[1];
    };
    DbNetGrid.prototype.initCallback = function (response) {
        var _this = this;
        this.$container = document.querySelector("#" + this.configuration.id);
        this.$container.innerHTML = response.html.toolbar;
        this.$toolbar = this.$container.querySelector(".toolbar");
        this.$prevBtn = this.$container.querySelector("button.prev");
        this.$nextBtn = this.$container.querySelector("button.next");
        this.$searchBtn = this.$container.querySelector("button.search");
        this.$searchToken = this.$container.querySelector("input.search-token");
        this.$prevBtn.onclick = function () { return _this.prevPage(); };
        this.$nextBtn.onclick = function () { return _this.nextPage(); };
        this.$searchBtn.onclick = function () { return _this.applySearch(_this.$searchToken.value); };
        this.quickSearchTimeout = null;
        this.$searchToken.onkeyup = function (e) { return _this.checkSearchBox(e); };
        this.$searchToken.addEventListener("search", function (e) { return function (e) { this.applySearch(""); }; });
        this.pageCallback(response);
    };
    DbNetGrid.prototype.checkSearchBox = function (event) {
        var _this = this;
        var token = event.target;
        clearTimeout(this.quickSearchTimeout);
        this.quickSearchTimeout = setTimeout(function () {
            if (token.value.length > 3) {
                _this.applySearch(token.value);
            }
            else if (token.getAttribute("applied-search-token")) {
                _this.applySearch("");
            }
        }, 1000);
    };
    DbNetGrid.prototype.nextPage = function () {
        var _this = this;
        if (this.configuration.currentPage >= this.configuration.totalPages) {
            return;
        }
        this.configuration.currentPage++;
        this.callServer("Page", this.configuration, function (response) { _this.pageCallback(response); });
    };
    DbNetGrid.prototype.prevPage = function () {
        var _this = this;
        if (this.configuration.currentPage <= 1) {
            return;
        }
        this.configuration.currentPage--;
        this.callServer("Page", this.configuration, function (response) { _this.pageCallback(response); });
    };
    DbNetGrid.prototype.applySearch = function (token) {
        var _this = this;
        this.$searchToken.setAttribute("applied-search-token", token);
        this.configuration.searchToken = token.toString();
        this.configuration.currentPage = 1;
        this.callServer("Page", this.configuration, function (response) { _this.pageCallback(response); });
    };
    DbNetGrid.prototype.pageCallback = function (response) {
        this.configuration = response;
        this.$container.querySelector(".grid").innerHTML = response.html.page;
        this.$container.querySelector(".current-page").innerText = response.currentPage.toString();
        this.$container.querySelector(".total-pages").innerText = response.totalPages.toString();
        this.$prevBtn.disabled = (response.currentPage === 1);
        this.$nextBtn.disabled = (response.currentPage === response.totalPages);
    };
    return DbNetGrid;
}(Ajax));
//# sourceMappingURL=DbNetSuite.js.map
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
        this.$searchBtn.onclick = function () { return _this.applySearch(); };
        this.quickSearchTimeout = null;
        this.$searchToken.onkeyup = function (e) { return _this.checkSearchBox(e); };
        this.pageCallback(response);
    };
    DbNetGrid.prototype.checkSearchBox = function (event) {
        var _this = this;
        var token = event.target;
        clearTimeout(this.quickSearchTimeout);
        this.quickSearchTimeout = setTimeout(function () {
            if (token.value.length > 3) {
                _this.applySearch();
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
    DbNetGrid.prototype.applySearch = function () {
        var _this = this;
        this.configuration.searchToken = this.$searchToken.value.toString();
        this.configuration.currentPage = 1;
        this.callServer("Page", this.configuration, function (response) { _this.pageCallback(response); });
    };
    DbNetGrid.prototype.pageCallback = function (response) {
        this.configuration = response;
        this.$container.querySelector(".grid").innerHTML = response.html.page;
        this.$container.querySelector(".current-page").innerText = response.currentPage.toString();
        this.$container.querySelector(".total-pages").innerText = response.totalPages.toString();
        this.$prevBtn.disabled = (response.currentPage == 1);
        this.$nextBtn.disabled = (response.currentPage == response.totalPages);
    };
    return DbNetGrid;
}(Ajax));
//# sourceMappingURL=DbNetGrid.js.map
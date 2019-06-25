/// <reference types="jquery" />
/// <reference types="notify" />
var Ajax = /** @class */ (function () {
    function Ajax() {
        var _this = this;
        $(document).on({
            ajaxStart: function () { $("body").addClass("loading"); },
            ajaxStop: function () { $("body").removeClass("loading"); }
        });
        $.ajaxSetup({
            complete: function (xhr, status) {
                if (xhr.getResponseHeader("X-Responded-JSON") !== null && JSON.parse(xhr.getResponseHeader("X-Responded-JSON")).status === "401") {
                    _this.timeout();
                }
            },
            error: function (jqxhr, settings, thrownError) {
                if (jqxhr.status.toString() === "401") { // Login has expired so tell the user and reload the page which will cause a redirect to the login page
                    _this.timeout();
                }
                else {
                    _this.ajaxError(jqxhr);
                }
            }
        });
    }
    Ajax.prototype.timeout = function () {
        var _this = this;
        var options = {};
        options.message = "Login has timed out";
        options.callback = function () { return _this.reload(); };
        bootbox.alert(options);
    };
    Ajax.prototype.ajaxError = function (xhr) {
        console.log(JSON.stringify(xhr));
        var options = {};
        this.notifyError("Oops! Something went wrong.");
    };
    Ajax.prototype.callServer = function (handler, data, callback) {
        var _this = this;
        var ajaxSettings = {};
        ajaxSettings.type = "POST";
        ajaxSettings.dataType = "json";
        ajaxSettings.url = "handler.dbnetgrid?handler=" + handler;
        ajaxSettings.data = data;
        ajaxSettings.success = function (response) { callback(response); };
        ajaxSettings.error = function (xhr) {
            _this.ajaxError(xhr);
        };
        ajaxSettings.headers = { "RequestVerificationToken": $("body").attr("xsrf-token") };
        $.ajax(ajaxSettings);
    };
    Ajax.prototype.reload = function () {
        window.location.reload();
    };
    Ajax.prototype.notifyInfo = function (message) {
        this.notify(message, "info");
    };
    Ajax.prototype.notifyError = function (message) {
        this.notify(message, "danger");
    };
    Ajax.prototype.setAntiForgeryToken = function () {
        $("input[name='__RequestVerificationToken']").val($("body").attr("xsrf-token"));
    };
    Ajax.prototype.notify = function (message, className) {
        var options = {};
        options.globalPosition = "top right";
        options.autoHide = true;
        options.className = className;
        $.notify(message, options);
    };
    return Ajax;
}());
/// <reference types="jquery" />
/// <reference types="bootstrap" />
/// <reference types="bootbox" />
/// <reference path="Ajax.ts" />
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
    function DbNetGrid() {
        var _this = _super.call(this) || this;
        _this.init();
        return _this;
    }
    DbNetGrid.prototype.init = function () {
        var _this = this;
        var params = {};
        this.callServer("Init", params, function (response) { _this.initCallback(response); });
    };
    DbNetGrid.prototype.initCallback = function (response) {
        alert(response);
    };
    return DbNetGrid;
}(Ajax));
//# sourceMappingURL=DbNetSuite.js.map
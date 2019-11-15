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
//# sourceMappingURL=Ajax.js.map
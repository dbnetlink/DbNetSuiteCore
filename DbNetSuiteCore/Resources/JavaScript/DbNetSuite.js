"use strict";
class DbNetSuite {
    constructor() {
        this.datePickerOptions = {};
        this.eventHandlers = {};
    }
    bind(event, handler) {
        if (!this.eventHandlers[event])
            this.eventHandlers[event] = [];
        this.eventHandlers[event].push(handler);
    }
    unbind(event, handler) {
        if (this.eventHandlers[event] == null)
            return;
        this.eventHandlers[event].forEach((f) => {
            if (f == handler)
                f = function () { };
        });
    }
    checkStyleSheetLoaded() {
        var _a;
        let found = false;
        for (const sheet of document.styleSheets) {
            if (sheet.href) {
                if (((_a = sheet === null || sheet === void 0 ? void 0 : sheet.href) === null || _a === void 0 ? void 0 : _a.indexOf("resource.dbnetsuite?action=css")) > 0) {
                    found = true;
                }
            }
        }
        if (!found) {
            alert("DbNetSuite stylesheet not found. Add @DbNetSuiteCore.StyleSheet() to yoiur Razor page. See console for details.");
            console.error("DbNetSuite stylesheet not found. See https://dbnetsuitecore.z35.web.core.windows.net/index.htm?context=20#DbNetSuiteCoreStyleSheet");
        }
    }
    fireEvent(event, params = undefined) {
        if (!this.eventHandlers[event])
            return false;
        const events = this.eventHandlers[event];
        events.forEach((method) => {
            let args = [this];
            if (params)
                args = args.concat(Array.prototype.slice.call(arguments, 1));
            method.apply(window, args);
        });
    }
}
document.addEventListener("DOMContentLoaded", function () {
    if ($.fn.button && $.fn.button.noConflict !== undefined) {
        $.fn.bootstrapBtn = $.fn.button.noConflict();
    }
});

"use strict";
class DbNetSuite {
    constructor(id) {
        this.datePickerOptions = {};
        this.element = undefined;
        this.eventHandlers = {};
        this.id = "";
        this.connectionString = "";
        this.connectionType = "SqlServer";
        this.initialised = false;
        this.id = id;
        this.element = $(`#${this.id}`);
        this.element.addClass("dbnetsuite").addClass("cleanslate");
        this.checkStyleSheetLoaded();
        if (this.element.length == 0) {
            this.error(`${this.constructor.name} container element '${this.id}' not found`);
            return;
        }
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
            alert("DbNetSuite stylesheet not found. Add @DbNetSuiteCore.StyleSheet() to your Razor page. See console for details.");
            console.error("DbNetSuite stylesheet not found. See https://dbnetsuitecore.z35.web.core.windows.net/index.htm?context=20#DbNetSuiteCoreStyleSheet");
        }
    }
    fireEvent(event, params = undefined) {
        if (!this.eventHandlers[event])
            return false;
        const events = this.eventHandlers[event];
        events.forEach((method) => {
            let args = [this];
            if (params) {
                args = args.concat([params]);
            }
            method.apply(window, args);
        });
    }
    addPanel(panelId, parent = undefined) {
        const id = `${this.id}_${panelId}Panel`;
        if (parent == null) {
            parent = this.element;
        }
        jQuery('<div>', {
            id: id
        }).appendTo(parent);
        return $(`#${id}`);
    }
    addLoadingPanel() {
        this.loadingPanel = this.addPanel("loading");
        this.addPanel("loadingIcon", this.loadingPanel);
        this.loadingPanel.addClass("dbnetgrid-loading");
        this.loadingPanel.children().first().addClass("icon");
    }
    error(text) {
        var _a;
        if (((_a = this.element) === null || _a === void 0 ? void 0 : _a.length) == 0) {
            alert(text);
            return;
        }
        const errorPanel = this.addPanel("error");
        errorPanel.addClass("dbnetsuite-error");
        errorPanel.html(text);
    }
    showLoader() {
        var _a;
        (_a = this.loadingPanel) === null || _a === void 0 ? void 0 : _a.addClass("display");
    }
    hideLoader() {
        var _a;
        (_a = this.loadingPanel) === null || _a === void 0 ? void 0 : _a.removeClass("display");
    }
}
DbNetSuite.DBNull = "DBNull";
document.addEventListener("DOMContentLoaded", function () {
    if ($.fn.button && $.fn.button.noConflict !== undefined) {
        $.fn.bootstrapBtn = $.fn.button.noConflict();
    }
});

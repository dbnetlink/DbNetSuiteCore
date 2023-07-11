"use strict";
var MessageBoxType;
(function (MessageBoxType) {
    MessageBoxType[MessageBoxType["Error"] = 0] = "Error";
    MessageBoxType[MessageBoxType["Warning"] = 1] = "Warning";
    MessageBoxType[MessageBoxType["Info"] = 2] = "Info";
    MessageBoxType[MessageBoxType["Question"] = 3] = "Question";
})(MessageBoxType || (MessageBoxType = {}));
class DbNetSuite {
    constructor(id) {
        this.datePickerOptions = {};
        this.element = undefined;
        this.eventHandlers = {};
        this.id = "";
        this.connectionString = "";
        this.connectionType = "SqlServer";
        this.culture = "";
        this.linkedControls = [];
        this.initialised = false;
        this.id = id;
        this.element = $(`#${this.id}`);
        this.element.addClass("dbnetsuite").addClass("cleanslate").addClass("empty");
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
                f = function () { return; };
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
    addLinkedControl(control) {
        this.linkedControls.push(control);
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
        }).addClass(`${this.constructor.name.toLowerCase()}-${panelId}`).appendTo(parent);
        return $(`#${id}`);
    }
    addLoadingPanel() {
        this.loadingPanel = this.addPanel("loading");
        this.addPanel("loadingIcon", this.loadingPanel);
        this.loadingPanel.addClass("dbnetsuite-loading");
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
        var _a, _b;
        (_a = this.element) === null || _a === void 0 ? void 0 : _a.removeClass("empty");
        (_b = this.loadingPanel) === null || _b === void 0 ? void 0 : _b.removeClass("display");
    }
    post(action, request, blob = false) {
        this.showLoader();
        const options = {
            method: "POST",
            headers: {
                Accept: "application/json",
                "Content-Type": "application/json;charset=UTF-8",
            },
            body: JSON.stringify(request)
        };
        return fetch(`~/${this.constructor.name.toLowerCase()}.dbnetsuite?action=${action}`, options)
            .then(response => {
            this.hideLoader();
            if (!response.ok) {
                throw response;
            }
            if (blob) {
                return response.blob();
            }
            return response.json();
        })
            .catch(err => {
            err.text().then((errorMessage) => {
                console.error(errorMessage);
                this.error(errorMessage.split("\n").shift());
            });
            return Promise.reject();
        });
    }
    controlElement(name) {
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        return $(`#${this.controlElementId(name)}`);
    }
    controlElementId(name) {
        return `${this.id}_${name}`;
    }
    disable(id, disabled) {
        this.controlElement(id).prop("disabled", disabled);
    }
    setInputElement(name, value) {
        const el = this.controlElement(name);
        el.val(value.toString());
        el.width(`${value.toString().length}em`);
    }
    configureLinkedControls(fk) {
        this.linkedControls.forEach((control) => {
            control.configureLinkedControl(control, fk);
        });
    }
    showMessageBox(message, type, callback) {
        if (this.messageBox == undefined) {
            this.post("message-box", {})
                .then((response) => {
                var _a;
                (_a = this.element) === null || _a === void 0 ? void 0 : _a.append(response.dialog);
                this.messageBox = new MessageBox(`${this.id}_message_box`);
            });
        }
        this.messageBox.show(message, type);
    }
    configureLinkedControl(control, fk) {
        if (control instanceof DbNetGrid) {
            const grid = control;
            grid.configureLinkedGrid(grid, fk);
        }
        if (control instanceof DbNetCombo) {
            const combo = control;
            combo.configureLinkedCombo(combo, fk);
        }
    }
}
DbNetSuite.DBNull = "DBNull";
document.addEventListener("DOMContentLoaded", function () {
    if ($.fn.button && $.fn.button.noConflict !== undefined) {
        $.fn.bootstrapBtn = $.fn.button.noConflict();
    }
});

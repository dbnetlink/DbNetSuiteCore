"use strict";
class DbNetSuite {
    constructor(id) {
        this.datePickerOptions = {};
        this.element = undefined;
        this.eventHandlers = {};
        this.internalEventHandlers = {};
        this.id = "";
        this.connectionString = "";
        this.connectionType = "SqlServer";
        this.culture = "";
        this.linkedControls = [];
        this.parentControlType = "";
        this.initialised = false;
        if (id == null) {
            return;
        }
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
    internalBind(event, callback) {
        if (!this.internalEventHandlers[event])
            this.internalEventHandlers[event] = [];
        this.internalEventHandlers[event].push({ sender: this, method: callback });
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
        control.parentControlType = this.constructor.name;
        this.linkedControls.push(control);
    }
    fireEvent(event, params = undefined) {
        if (this.eventHandlers[event]) {
            const events = this.eventHandlers[event];
            events.forEach((handler) => {
                let args = [this];
                if (params) {
                    args = args.concat([params]);
                }
                handler.apply(window, args);
            });
        }
        if (this.internalEventHandlers[event]) {
            const events = this.internalEventHandlers[event];
            events.forEach((handler) => {
                let args = [this];
                if (params) {
                    args = args.concat([params]);
                }
                handler.method.apply(handler.context, args);
            });
        }
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
    showLoader() {
        var _a;
        (_a = this.loadingPanel) === null || _a === void 0 ? void 0 : _a.addClass("display");
    }
    hideLoader() {
        var _a, _b;
        (_a = this.element) === null || _a === void 0 ? void 0 : _a.removeClass("empty");
        (_b = this.loadingPanel) === null || _b === void 0 ? void 0 : _b.removeClass("display");
    }
    post(action, request, blob = false, page = null) {
        this.showLoader();
        const options = {
            method: "POST",
            headers: {
                Accept: "application/json",
                "Content-Type": "application/json;charset=UTF-8",
            },
            body: JSON.stringify(request)
        };
        if (page == null) {
            page = this.constructor.name.toLowerCase();
        }
        return fetch(`~/${page}.dbnetsuite?action=${action}`, options)
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
    configureLinkedControls(id, pk = null) {
        this.linkedControls.forEach((control) => {
            this._configureLinkedControl(control, id, pk);
        });
    }
    info(text, element) {
        this.showMessageBox(MessageBoxType.Info, text, element);
    }
    confirm(text, element, callback) {
        this.showMessageBox(MessageBoxType.Confirm, text, element, callback);
    }
    error(text, element = null) {
        this.showMessageBox(MessageBoxType.Error, text, element);
    }
    showMessageBox(messageBoxType, text, element = null, callback = undefined) {
        var _a;
        if (this.messageBox == undefined) {
            this.post("message-box", this._getRequest(), false, "dbnetsuite")
                .then((response) => {
                var _a, _b;
                (_a = this.element) === null || _a === void 0 ? void 0 : _a.append(response.html);
                this.messageBox = new MessageBox(`${this.id}_message_box`);
                (_b = this.messageBox) === null || _b === void 0 ? void 0 : _b.show(messageBoxType, text, element, callback);
            });
        }
        (_a = this.messageBox) === null || _a === void 0 ? void 0 : _a.show(messageBoxType, text, element, callback);
    }
    addDatePicker($input, datePickerOptions) {
        const options = datePickerOptions;
        const formats = { D: "DD, MM dd, yy", DDDD: "DD", DDD: "D", MMMM: "MM", MMM: "M", M: "m", MM: "mm", yyyy: "yy" };
        let format = $input.attr("format");
        let pattern;
        for (pattern in formats) {
            const re = new RegExp(`\\b${pattern}\\b`);
            format = format.replace(re, formats[pattern]);
        }
        if (format != undefined)
            if (format != $input.attr("format"))
                options.dateFormat = format;
        options.onSelect = this.pickerSelected;
        $input.datepicker(options);
    }
    pickerSelected() {
        const $row = $(this).closest("tr").parent().closest("tr");
        const $select = $row.find("select");
        if ($select.val() == "") {
            $select.prop("selectedIndex", 1);
        }
    }
    addTimePicker($input) {
        const options = { "zindex": 100000 };
        options.change = this.pickerSelected;
        $input.timepicker(options);
    }
    _configureLinkedControl(control, id, pk) {
        if (this instanceof DbNetGrid) {
            this.configureLinkedControl(control, id, pk);
        }
        if (this instanceof DbNetCombo) {
            this.configureLinkedControl(control, id, pk);
        }
    }
    _getRequest() {
        const request = {
            componentId: this.id,
            connectionString: this.connectionString,
            culture: this.culture,
            parentControlType: this.parentControlType
        };
        return request;
    }
}
DbNetSuite.DBNull = "DBNull";
document.addEventListener("DOMContentLoaded", function () {
    if ($.fn.button && $.fn.button.noConflict !== undefined) {
        $.fn.bootstrapBtn = $.fn.button.noConflict();
    }
});

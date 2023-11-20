"use strict";
class DbNetSuite {
    constructor(id) {
        this.datePickerOptions = {};
        this.element = undefined;
        this.eventHandlers = {};
        this.internalEventHandlers = {};
        this.id = "";
        this.connectionString = "";
        this.culture = "";
        this.linkedControls = [];
        this.parentControlType = "";
        this.parentChildRelationship = null;
        this.initialised = false;
        this.parentControl = null;
        this.dataProvider = null;
        this.quickSearch = false;
        this.quickSearchDelay = 1000;
        this.quickSearchMinChars = 3;
        this.quickSearchToken = "";
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
        this.linkedControls.push(control);
        control.parentControl = this;
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
        var _a, _b;
        (_a = this.loadingPanel) === null || _a === void 0 ? void 0 : _a.addClass("display");
        (_b = this.element) === null || _b === void 0 ? void 0 : _b.css('pointer-events', 'none');
    }
    hideLoader() {
        var _a, _b, _c;
        (_a = this.element) === null || _a === void 0 ? void 0 : _a.removeClass("empty");
        (_b = this.loadingPanel) === null || _b === void 0 ? void 0 : _b.removeClass("display");
        (_c = this.element) === null || _c === void 0 ? void 0 : _c.css('pointer-events', 'all');
    }
    post(action, request, blob = false, page = null) {
        this.showLoader();
        let options = {};
        if (request instanceof FormData) {
            options = {
                method: "POST",
                body: request
            };
        }
        else {
            options = {
                method: "POST",
                headers: {
                    Accept: "application/json",
                    "Content-Type": "application/json;charset=UTF-8",
                },
                body: JSON.stringify(request)
            };
        }
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
        el.width(`${(value.toString().length * 8) + 8}px`);
    }
    configureLinkedControls(id, pk = null, fk = null) {
        this.linkedControls.forEach((control) => {
            this._configureLinkedControl(control, id, pk, fk);
        });
    }
    linkedGridOrEdit() {
        let found = false;
        this.linkedControls.forEach((control) => {
            if (control instanceof DbNetGrid || control instanceof DbNetEdit) {
                found = true;
            }
        });
        return found;
    }
    parentGridOrEdit() {
        return (this.parentControl instanceof DbNetGrid || this.parentControl instanceof DbNetEdit);
    }
    configureParentDeleteButton(disabled) {
        var _a;
        (_a = this.parentControl) === null || _a === void 0 ? void 0 : _a.disable("DeleteBtn", disabled);
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
    _configureLinkedControl(control, id, pk, fk) {
        if (this instanceof DbNetGrid) {
            this.configureLinkedControl(control, id, pk, fk);
        }
        if (this instanceof DbNetCombo) {
            this.configureLinkedControl(control, id, pk);
        }
        if (this instanceof DbNetEdit) {
            this.configureLinkedControl(control, pk);
        }
    }
    _getRequest() {
        const request = {
            componentId: this.id,
            connectionString: this.connectionString,
            culture: this.culture,
            parentControlType: this.parentControlType,
            dataProvider: this.dataProvider
        };
        return request;
    }
    highlight() {
        var _a;
        const className = "highlight-dbnetsuite-container";
        (_a = this.element) === null || _a === void 0 ? void 0 : _a.addClass(className);
        window.setTimeout(() => { var _a; (_a = this.element) === null || _a === void 0 ? void 0 : _a.removeClass(className); }, 3000);
    }
    viewImage(event) {
        const $img = $(event.currentTarget);
        this.openImageViewer($img.attr("src"), $img.data("filename"));
    }
    viewUrl(url, fileName, type = "Image") {
        this.openImageViewer(url, fileName, type);
    }
    openImageViewer(src, fileName, type = "Image") {
        if (this.imageViewer) {
            this.imageViewer.show(src, fileName, type);
            return;
        }
        this.post("image-viewer", this._getRequest(), false, "dbnetsuite")
            .then((response) => {
            var _a;
            if (!this.imageViewer) {
                (_a = this.element) === null || _a === void 0 ? void 0 : _a.append(response.html);
                this.imageViewer = new ImageViewer(`${this.id}_image_viewer`);
                this.imageViewer.show(src, fileName, type);
            }
        });
    }
    quickSearchKeyPress(event) {
        const el = event.target;
        window.clearTimeout(this.quickSearchTimerId);
        if (el.value.length >= this.quickSearchMinChars || el.value.length == 0 || event.key == 'Enter')
            this.quickSearchTimerId = window.setTimeout(() => { this.runQuickSearch(el.value); }, this.quickSearchDelay);
    }
    runQuickSearch(token) {
        this.quickSearchToken = token;
        if (this instanceof DbNetGrid) {
            const grid = this;
            grid.reload();
        }
        else if (this instanceof DbNetEdit) {
            const edit = this;
            edit.currentRow = 1;
            edit.getRows();
        }
        else if (this instanceof DbNetFile) {
            const file = this;
            file.reload();
        }
    }
}
DbNetSuite.DBNull = "DBNull";
document.addEventListener("DOMContentLoaded", function () {
    if ($.fn.button && $.fn.button.noConflict !== undefined) {
        $.fn.bootstrapBtn = $.fn.button.noConflict();
    }
});

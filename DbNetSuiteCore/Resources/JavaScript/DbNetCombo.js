"use strict";
class DbNetCombo extends DbNetSuite {
    constructor(id) {
        super(id);
        this.addEmptyOption = false;
        this.addFilter = false;
        this.currentValue = "";
        this.emptyOptionText = "";
        this.linkedControls = [];
        this.fromPart = "";
        this.valueColumn = "";
        this.textColumn = "";
        this.params = {};
        this.filterDelay = 1000;
        this.filterMinChars = 3;
        this.filterToken = "";
        this.foreignKeyColumn = "";
        this.foreignKeyValue = {};
    }
    initialize() {
        this.callServer("page");
        this.initialised = true;
        this.fireEvent("onInitialized");
    }
    addLinkedControl(control) {
        this.linkedControls.push(control);
    }
    reload() {
        this.callServer("page");
    }
    configureCombo(response) {
        var _a, _b, _c, _d, _e, _f;
        if (response.select) {
            (_a = this.element) === null || _a === void 0 ? void 0 : _a.html(response.select);
            this.$select = (_b = this.element) === null || _b === void 0 ? void 0 : _b.find("select");
            (_c = this.$select) === null || _c === void 0 ? void 0 : _c.html(response.options);
            const selectWidth = (_d = this.$select) === null || _d === void 0 ? void 0 : _d.width();
            const $input = (_e = this.element) === null || _e === void 0 ? void 0 : _e.find("input");
            $input === null || $input === void 0 ? void 0 : $input.width(selectWidth);
            $input === null || $input === void 0 ? void 0 : $input.on("keyup", (event) => this.filterKeyPress(event));
            this.$select.on("change", () => this.optionSelected());
        }
        else {
            (_f = this.$select) === null || _f === void 0 ? void 0 : _f.html(response.options);
        }
        this.fireEvent("onOptionsLoaded");
        this.optionSelected();
    }
    optionSelected() {
        var _a;
        const selectedValue = (_a = this.$select) === null || _a === void 0 ? void 0 : _a.find(":selected").val();
        this.fireEvent("onOptionSelected");
        this.linkedControls.forEach((control) => {
            if (control instanceof DbNetGrid) {
                const grid = control;
                grid.configureLinkedGrid(grid, selectedValue);
            }
            if (control instanceof DbNetCombo) {
                const combo = control;
                combo.configureLinkedCombo(combo, selectedValue);
            }
        });
    }
    configureLinkedCombo(combo, fk) {
        if (combo.connectionString == "") {
            combo.connectionString = this.connectionString;
        }
        combo.foreignKeyValue = fk;
        if (fk) {
            combo.initialised ? combo.reload() : combo.initialize();
        }
        else {
            combo.clear();
        }
    }
    clear() {
        var _a;
        (_a = this.$select) === null || _a === void 0 ? void 0 : _a.find('option').not("value=['']").remove();
        this.optionSelected();
    }
    filterKeyPress(event) {
        const el = event.target;
        window.clearTimeout(this.filterTimerId);
        if (el.value.length >= this.filterMinChars || el.value.length == 0 || event.key == 'Enter') {
            this.filterTimerId = window.setTimeout(() => { this.applyFilter(el.value); }, this.filterDelay);
        }
    }
    applyFilter(filterToken) {
        this.filterToken = filterToken;
        this.callServer("filter");
    }
    callServer(action) {
        this.post(action, this.getRequest())
            .then((response) => {
            if (response.error == false) {
                this.configureCombo(response);
            }
        });
    }
    getRequest() {
        const request = {
            componentId: this.id,
            connectionString: this.connectionString,
            fromPart: this.fromPart,
            valueColumn: this.valueColumn,
            textColumn: this.textColumn,
            params: this.params,
            addEmptyOption: this.addEmptyOption,
            emptyOptionText: this.emptyOptionText,
            addFilter: this.addFilter,
            filterToken: this.filterToken,
            foreignKeyColumn: this.foreignKeyColumn,
            foreignKeyValue: this.foreignKeyValue
        };
        return request;
    }
    post(action, request) {
        this.showLoader();
        const options = {
            method: "POST",
            headers: {
                Accept: "application/json",
                "Content-Type": "application/json;charset=UTF-8",
            },
            body: JSON.stringify(request)
        };
        return fetch(`~/dbnetcombo.dbnetsuite?action=${action}`, options)
            .then(response => {
            this.hideLoader();
            if (!response.ok) {
                throw response;
            }
            return response.json();
        })
            .catch(err => {
            err.text().then((errorMessage) => {
                console.error(errorMessage);
            });
            return Promise.reject();
        });
    }
}

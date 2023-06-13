"use strict";
class DbNetCombo extends DbNetSuite {
    constructor(id) {
        super();
        this.addEmptyOption = false;
        this.addFilter = false;
        this.currentValue = "";
        this.emptyOptionText = "";
        this.linkedControls = [];
        this.sql = "";
        this.params = {};
        this.filterDelay = 1000;
        this.filterMinChars = 3;
        this.filterToken = "";
        this.id = id;
        this.element = $(`#${this.id}`);
        this.element.addClass("dbnetsuite").addClass("cleanslate");
        this.checkStyleSheetLoaded();
        if (this.element.length == 0) {
            //   this.error(`DbNetCombo container element '${this.id}' not found`);
            return;
        }
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
        var _a, _b, _c;
        if (response.select) {
            (_a = this.element) === null || _a === void 0 ? void 0 : _a.html(response.select);
        }
        const $select = (_b = this.element) === null || _b === void 0 ? void 0 : _b.find("select");
        $select === null || $select === void 0 ? void 0 : $select.html(response.options);
        const selectWidth = $select === null || $select === void 0 ? void 0 : $select.width();
        const $input = (_c = this.element) === null || _c === void 0 ? void 0 : _c.find("input");
        $input === null || $input === void 0 ? void 0 : $input.width(selectWidth - 4);
        $input === null || $input === void 0 ? void 0 : $input.on("keyup", (event) => this.filterKeyPress(event));
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
            sql: this.sql,
            params: this.params,
            addEmptyOption: this.addEmptyOption,
            emptyOptionText: this.emptyOptionText,
            addFilter: this.addFilter,
            filterToken: this.filterToken
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

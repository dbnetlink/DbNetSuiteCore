"use strict";
class DbNetCombo extends DbNetSuite {
    constructor(id) {
        super();
        this.id = "";
        this.connectionString = "";
        this.initialised = false;
        this.linkedControls = [];
        this.addEmptyOption = false;
        this.emptyOptionText = "";
        this.sql = "";
        this.params = {};
        this.currentValue = "";
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
        this.getPage();
        this.initialised = true;
        this.fireEvent("onInitialized");
    }
    addLinkedControl(control) {
        this.linkedControls.push(control);
    }
    reload() {
        this.getPage();
    }
    configureCombo(response) {
        var _a;
        (_a = this.element) === null || _a === void 0 ? void 0 : _a.html(response.data);
    }
    getPage() {
        this.post("page", this.getRequest())
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
            emptyOptionText: this.emptyOptionText
        };
        return request;
    }
    post(action, request, blob = false) {
        // this.showLoader();
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
            //  this.hideLoader();
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
                //   this.error(errorMessage.split("\n").shift() as string)
            });
            return Promise.reject();
        });
    }
}

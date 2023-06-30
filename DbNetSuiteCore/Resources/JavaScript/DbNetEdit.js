"use strict";
class DbNetEdit extends DbNetSuite {
    constructor(id) {
        super(id);
        this.currentRow = 1;
        this.toolbarPosition = "Top";
        this.linkedControls = [];
        this.fromPart = "";
        this.search = true;
        this.navigation = true;
        this.quickSearch = true;
        this.columns = [];
    }
    initialize() {
        if (!this.element) {
            return;
        }
        this.element.empty();
        if (this.toolbarPosition == "Top") {
            this.toolbarPanel = this.addPanel("toolbar");
        }
        this.formPanel = this.addPanel("form");
        if (this.toolbarPosition == "Bottom") {
            this.toolbarPanel = this.addPanel("toolbar");
        }
        this.addLoadingPanel();
        this.callServer("initialize");
        this.initialised = true;
        this.fireEvent("onInitialized");
    }
    addLinkedControl(control) {
        this.linkedControls.push(control);
    }
    reload() {
        this.callServer("page");
    }
    configureEdit(response) {
        var _a, _b;
        if (this.toolbarPanel) {
            if (response.toolbar) {
                (_a = this.toolbarPanel) === null || _a === void 0 ? void 0 : _a.html(response.toolbar);
            }
        }
        if (response.form) {
            (_b = this.formPanel) === null || _b === void 0 ? void 0 : _b.html(response.form);
        }
    }
    callServer(action) {
        this.post(action, this.getRequest())
            .then((response) => {
            if (response.error == false) {
                this.configureEdit(response);
            }
        });
    }
    getRequest() {
        const request = {
            componentId: this.id,
            connectionString: this.connectionString,
            fromPart: this.fromPart,
            columns: this.columns.map((column) => { return column; }),
            currentRow: this.currentRow,
            culture: this.culture,
            search: this.search,
            navigation: this.navigation,
            quickSearch: this.quickSearch
        };
        return request;
    }
}

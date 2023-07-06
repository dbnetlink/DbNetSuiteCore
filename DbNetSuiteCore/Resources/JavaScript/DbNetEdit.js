"use strict";
class DbNetEdit extends DbNetGridEdit {
    constructor(id) {
        super(id);
        this.changes = {};
        this.currentRow = 1;
        this.search = true;
        this.totalRows = 0;
        this.primaryKey = "";
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
            this.configureToolbar(response);
        }
        if (response.form) {
            (_b = this.formPanel) === null || _b === void 0 ? void 0 : _b.html(response.form);
        }
        this.updateForm(response);
    }
    updateForm(response) {
        var _a;
        const record = response.record;
        for (const columnName in record) {
            const $input = (_a = this.formPanel) === null || _a === void 0 ? void 0 : _a.find(`input[name='${columnName}']`);
            const value = record[columnName].toString();
            $input.val(value).data("value", value);
        }
        this.primaryKey = response.primaryKey;
        if (response.message) {
            this.message(response.message);
        }
    }
    callServer(action) {
        this.post(action, this.getRequest())
            .then((response) => {
            if (response.error == false) {
                this.updateColumns(response);
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
            quickSearch: this.quickSearch,
            totalRows: this.totalRows,
            primaryKey: this.primaryKey,
            changes: this.changes
        };
        return request;
    }
    configureToolbar(response) {
        if (response.toolbar) {
            const buttons = ["First", "Next", "Previous", "Last", "Cancel", "Apply", "Search"];
            buttons.forEach(btn => this.addEventListener(`${btn}Btn`));
        }
        const $navigationElements = this.controlElement("dbnetedit-toolbar").find(".navigation");
        const $noRecordsCell = this.controlElement("no-records-cell");
        this.setInputElement("Rows", response.totalRows);
        if (response.totalRows == 0) {
            $navigationElements.hide();
            $noRecordsCell.show();
        }
        else {
            $navigationElements.show();
            $noRecordsCell.hide();
            this.controlElement("dbnetgrid-toolbar").find(".navigation").show();
            this.setInputElement("RowNumber", response.currentRow);
            this.setInputElement("RowCount", response.totalRows);
            this.currentRow = response.currentRow;
            this.totalRows = response.totalRows;
            this.disable("FirstBtn", response.currentRow == 1);
            this.disable("PreviousBtn", response.currentRow == 1);
            this.disable("NextBtn", response.currentRow == response.totalRows);
            this.disable("LastBtn", response.currentRow == response.totalRows);
        }
        this.controlElement("QuickSearch").on("keyup", (event) => this.quickSearchKeyPress(event));
    }
    updateColumns(response) {
        var _a;
        this.columns = new Array();
        (_a = response.columns) === null || _a === void 0 ? void 0 : _a.forEach((col) => {
            const properties = {
                columnExpression: col.columnExpression,
                columnName: col.columnName,
                label: col.label,
                format: col.format,
                foreignKey: col.foreignKey
            };
            this.columns.push(new EditColumn(properties));
        });
    }
    addEventListener(id, eventName = "click") {
        this.controlElement(id).on(eventName, (event) => this.handleClick(event));
    }
    handleClick(event) {
        const id = event.target.id;
        switch (id) {
            case this.controlElementId("FirstBtn"):
                this.currentRow = 1;
                break;
            case this.controlElementId("NextBtn"):
                this.currentRow++;
                break;
            case this.controlElementId("PreviousBtn"):
                this.currentRow--;
                break;
            case this.controlElementId("LastBtn"):
                this.currentRow = this.totalRows;
                break;
            case this.controlElementId("SearchBtn"):
            case this.controlElementId("CancelBtn"):
            case this.controlElementId("ApplyBtn"):
                break;
            default:
                return;
        }
        event.preventDefault();
        switch (id) {
            case this.controlElementId("SearchBtn"):
                this.openSearchDialog();
                break;
            case this.controlElementId("ApplyBtn"):
                this.applyChanges();
                break;
            default:
                this.getRecord();
                break;
        }
    }
    openSearchDialog() {
        if (this.searchDialog) {
            this.searchDialog.open();
            return;
        }
        this.post("search-dialog", this.getRequest())
            .then((response) => {
            var _a;
            (_a = this.element) === null || _a === void 0 ? void 0 : _a.append(response.data);
            this.searchDialog = new SearchDialog(`${this.id}_search_dialog`, this);
            this.searchDialog.open();
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
        this.currentRow = 1;
        this.runSearch();
    }
    runSearch() {
        return;
    }
    getRecord() {
        this.callServer("getrecord");
    }
    applyChanges() {
        var _a;
        const changes = {};
        (_a = this.formPanel) === null || _a === void 0 ? void 0 : _a.find(':input.dbnetedit').each(function (index) {
            const $input = $(this);
            if ($input.val() != $input.data("value")) {
                changes[$input.attr("name")] = $input.val();
            }
        });
        if ($.isEmptyObject(changes)) {
            return;
        }
        this.changes = changes;
        this.callServer("applychanges");
    }
    message(msg) {
        var _a;
        (_a = this.formPanel) === null || _a === void 0 ? void 0 : _a.find(".message").html(msg).addClass("highlight");
        setInterval(() => this.clearMessage(), 5000);
    }
    clearMessage() {
        var _a;
        (_a = this.formPanel) === null || _a === void 0 ? void 0 : _a.find(".message").html("&nbsp;").removeClass("highlight");
    }
}

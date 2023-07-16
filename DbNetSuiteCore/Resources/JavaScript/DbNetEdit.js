"use strict";
class DbNetEdit extends DbNetGridEdit {
    constructor(id) {
        super(id);
        this.changes = {};
        this.currentRow = 1;
        this.layoutColumns = 1;
        this.primaryKey = "";
        this.search = true;
        this.totalRows = 0;
        this.isEditDialog = false;
        if (this.toolbarPosition == undefined) {
            this.toolbarPosition = "Bottom";
        }
    }
    initialize(_callback) {
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
        this.post("initialize", this.getRequest())
            .then((response) => {
            if (response.error == false) {
                this.updateColumns(response);
                this.configureEdit(response);
                if (_callback) {
                    _callback();
                }
                this.initialised = true;
                this.fireEvent("onInitialized");
            }
        });
    }
    addLinkedControl(control) {
        this.linkedControls.push(control);
    }
    getRows(callback) {
        this.callServer("search", callback);
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
            this.configureForm();
        }
        this.updateForm(response);
    }
    configureForm() {
        var _a, _b, _c, _d, _e;
        const $inputs = (_a = this.formPanel) === null || _a === void 0 ? void 0 : _a.find(`:input[name]`);
        for (let i = 0; i < $inputs.length; i++) {
            const $input = $($inputs[i]);
            this.fireEvent("onFormElementCreated", { formElement: $input[0], columnName: $input.attr("name") });
        }
        (_b = this.formPanel) === null || _b === void 0 ? void 0 : _b.find("[button-type='calendar']").on("click", (event) => this.selectDate(event));
        (_c = this.formPanel) === null || _c === void 0 ? void 0 : _c.find("[button-type='clock']").on("click", (event) => this.selectTime(event));
        (_d = this.formPanel) === null || _d === void 0 ? void 0 : _d.find("[button-type='lookup']").on("click", (event) => this.editLookup(event));
        (_e = this.formPanel) === null || _e === void 0 ? void 0 : _e.find("input[datatype='DateTime'").get().forEach(e => {
            const $input = $(e);
            this.addDatePicker($input, this.datePickerOptions);
        });
    }
    updateForm(response) {
        var _a, _b, _c;
        if (response.totalRows == 0) {
            (_a = this.formPanel) === null || _a === void 0 ? void 0 : _a.find(':input').val('').not("[primarykey='true']").prop('checked', false).prop('selected', false).prop("disabled", true);
            return;
        }
        (_b = this.formPanel) === null || _b === void 0 ? void 0 : _b.find(':input').not("[primarykey='true']").prop("disabled", false);
        const record = response.record;
        for (const columnName in record) {
            const $input = (_c = this.formPanel) === null || _c === void 0 ? void 0 : _c.find(`:input[name='${columnName}']`);
            const value = record[columnName];
            if ($input.attr("type") == "checkbox") {
                const checked = (value === true);
                $input.prop("checked", checked).data("value", checked);
            }
            else {
                $input.val(value.toString()).data("value", value.toString());
            }
        }
        this.primaryKey = response.primaryKey;
        if (response.message) {
            this.message(response.message);
        }
    }
    callServer(action, callback) {
        this.post(action, this.getRequest())
            .then((response) => {
            if (response.error == false) {
                this.configureToolbar(response);
                this.updateForm(response);
            }
            if (callback) {
                callback(response);
            }
        });
    }
    getRequest() {
        const request = {
            changes: this.changes,
            componentId: this.id,
            connectionString: this.connectionString,
            columns: this.columns.map((column) => { return column; }),
            currentRow: this.currentRow,
            culture: this.culture,
            fromPart: this.fromPart,
            layoutColumns: this.layoutColumns,
            navigation: this.navigation,
            quickSearch: this.quickSearch,
            quickSearchToken: this.quickSearchToken,
            search: this.search,
            searchFilterJoin: this.searchFilterJoin,
            searchParams: this.searchParams,
            totalRows: this.totalRows,
            primaryKey: this.primaryKey,
            isEditDialog: this.isEditDialog
        };
        return request;
    }
    configureToolbar(response) {
        if (response.toolbar) {
            const buttons = this.isEditDialog ? ["Cancel", "Apply"] : ["First", "Next", "Previous", "Last", "Cancel", "Apply", "Search"];
            buttons.forEach(btn => this.addEventListener(`${btn}Btn`));
        }
        const $navigationElements = this.controlElement("dbnetedit-toolbar").find(".navigation");
        const $noRecordsCell = this.controlElement("no-records-cell");
        this.setInputElement("Rows", response.totalRows);
        if (response.totalRows == 0) {
            $navigationElements.hide();
            $noRecordsCell.show();
        }
        else if (this.isEditDialog == false) {
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
                foreignKey: col.foreignKey,
                foreignKeyValue: col.foreignKeyValue,
                lookup: col.lookup,
                style: col.style,
                display: col.display,
                dataType: col.dataType,
                primaryKey: col.primaryKey,
                index: col.index,
                editControlType: col.editControlType,
                pattern: col.pattern
            };
            this.columns.push(new EditColumn(properties));
        });
    }
    addEventListener(id, eventName = "click") {
        this.controlElement(id).on(eventName, (event) => this.handleClick(event));
    }
    handleClick(event) {
        const id = event.target.id;
        /*
        switch (id) {
            case this.controlElementId("FirstBtn"):
            case this.controlElementId("NextBtn"):
            case this.controlElementId("PreviousBtn"):
            case this.controlElementId("LastBtn"):
                if (this.hasUnappliedChanges()) {
                    this.messageBox("There are unapplied changes. Discard ?");

                }
                break;
            default:
                break;
        }
        */
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
                this.openSearchDialog(this.getRequest());
                break;
            case this.controlElementId("ApplyBtn"):
                this.applyChanges();
                break;
            default:
                this.getRecord();
                break;
        }
    }
    getRecord(primaryKey = null) {
        if (primaryKey) {
            this.primaryKey = primaryKey;
        }
        this.callServer("getrecord");
    }
    applyChanges() {
        var _a;
        const changes = {};
        let validForm = true;
        (_a = this.formPanel) === null || _a === void 0 ? void 0 : _a.find(':input.dbnetedit,select.dbnetedit').each(function (index) {
            const $input = $(this);
            const name = $input.attr("name");
            if ($input.attr("type") == "checkbox") {
                if ($input.prop("checked") != $input.data("value")) {
                    changes[name] = $input.prop("checked");
                }
            }
            else {
                if ($input.attr("pattern")) {
                    const valid = $input[0].reportValidity();
                    if (!valid) {
                        validForm = false;
                    }
                }
                if ($input.val() != $input.data("value")) {
                    changes[name] = $input.val();
                }
            }
        });
        if ($.isEmptyObject(changes)) {
            return;
        }
        if (validForm) {
            this.changes = changes;
            this.callServer("applychanges");
        }
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
    selectDate(event) {
        const $button = $(event.target);
        $button.parent().find("input").datepicker("show");
    }
    editLookup(event) {
        const $button = $(event.target);
        const $input = $button.parent().find("input");
        const request = this.getRequest();
        this.lookup($input, request);
    }
    selectTime(event) {
        const $button = $(event.target);
        $button.parent().find("input").timepicker('open');
        event.stopPropagation();
    }
}

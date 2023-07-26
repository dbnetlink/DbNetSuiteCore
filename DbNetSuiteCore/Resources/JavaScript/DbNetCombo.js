"use strict";
class DbNetCombo extends DbNetSuite {
    constructor(id) {
        super(id);
        this.addEmptyOption = false;
        this.addFilter = false;
        this.autoRowSelect = false;
        this.currentValue = "";
        this.dataOnlyColumns = [];
        this.distinct = false;
        this.emptyOptionText = "";
        this.fromPart = "";
        this.filterDelay = 1000;
        this.filterMinChars = 3;
        this.filterToken = "";
        this.foreignKeyColumn = "";
        this.multipleSelect = false;
        this.procedureParams = {};
        this.procedureName = "";
        this.size = 1;
        this.textColumn = "";
        this.valueColumn = "";
    }
    initialize() {
        this.comboPanel = this.addPanel("combo");
        this.addLoadingPanel();
        this.callServer("page");
        this.initialised = true;
        this.fireEvent("onInitialized");
    }
    reload() {
        this.callServer("page");
    }
    selectedValues() {
        return this.getSelectedValues();
    }
    selectedOptions() {
        return this.getSelectedOptions();
    }
    configureCombo(response) {
        var _a, _b, _c, _d, _e, _f, _g, _h, _j, _k;
        if (response.select) {
            (_a = this.comboPanel) === null || _a === void 0 ? void 0 : _a.html(response.select);
            this.$select = (_b = this.comboPanel) === null || _b === void 0 ? void 0 : _b.find("select");
            (_c = this.$select) === null || _c === void 0 ? void 0 : _c.html(response.options);
            const selectWidth = (_d = this.$select) === null || _d === void 0 ? void 0 : _d.width();
            this.$filter = (_e = this.comboPanel) === null || _e === void 0 ? void 0 : _e.find("input");
            (_f = this.$filter) === null || _f === void 0 ? void 0 : _f.width(selectWidth);
            (_g = this.$filter) === null || _g === void 0 ? void 0 : _g.on("keyup", (event) => this.filterKeyPress(event));
            this.$select.on("change", () => this.optionSelected());
        }
        else {
            (_h = this.$select) === null || _h === void 0 ? void 0 : _h.html(response.options);
        }
        if (this.autoRowSelect && this.size > 1 && ((_j = this.$select) === null || _j === void 0 ? void 0 : _j.children().length) > 0) {
            (_k = this.$select) === null || _k === void 0 ? void 0 : _k.prop("selectedIndex", 0);
        }
        this.fireEvent("onOptionsLoaded", { options: this.getOptions() });
        this.optionSelected();
    }
    optionSelected() {
        const selectedValues = this.getSelectedValues();
        const selectedOptions = this.getSelectedOptions();
        const pk = selectedOptions.length ? selectedOptions[0].dataset["pk"] : null;
        this.fireEvent("onOptionSelected", { selectedValues: selectedValues, selectedOptions: selectedOptions });
        this.configureLinkedControls(selectedValues, pk);
    }
    getSelectedValues() {
        var _a;
        const selectedValues = new Array();
        (_a = this.$select) === null || _a === void 0 ? void 0 : _a.find("option:selected").each(function () {
            const s = $(this).val();
            if (s.length > 0) {
                selectedValues.push(s);
            }
        });
        return selectedValues;
    }
    getSelectedOptions() {
        var _a;
        const selectedOptions = new Array();
        (_a = this.$select) === null || _a === void 0 ? void 0 : _a.find("option:selected").each(function () {
            selectedOptions.push($(this)[0]);
        });
        return selectedOptions;
    }
    getOptions() {
        var _a;
        const options = new Array();
        (_a = this.$select) === null || _a === void 0 ? void 0 : _a.find("option").each(function () {
            options.push($(this)[0]);
        });
        return options;
    }
    configureLinkedControl(control, fk, pk) {
        if (control instanceof DbNetCombo) {
            const combo = control;
            if (control.connectionString == "") {
                control.connectionString = this.connectionString;
            }
            combo.foreignKeyValue = fk;
            combo.initialised ? combo.reload() : combo.initialize();
        }
        if (control instanceof DbNetGrid) {
            const grid = control;
            grid.assignForeignKey(grid, fk);
            grid.currentPage = 1;
            grid.initialised ? grid.getPage() : grid.initialize();
        }
        if (control instanceof DbNetEdit) {
            const edit = control;
            edit.currentRow = 1;
            edit.initialised ? edit.getRecord(pk) : edit.initialize(pk);
        }
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
            culture: this.culture,
            dataOnlyColumns: this.dataOnlyColumns,
            parentControlType: this.parentControlType,
            fromPart: this.fromPart,
            valueColumn: this.valueColumn,
            textColumn: this.textColumn,
            procedureParams: this.procedureParams,
            addEmptyOption: this.addEmptyOption,
            emptyOptionText: this.emptyOptionText,
            addFilter: this.addFilter,
            filterToken: this.filterToken,
            foreignKeyColumn: this.foreignKeyColumn,
            foreignKeyValue: this.foreignKeyValue,
            size: this.size,
            multipleSelect: this.multipleSelect,
            procedureName: this.procedureName,
            distinct: this.distinct
        };
        return request;
    }
}

"use strict";
class SearchDialog extends Dialog {
    constructor(id, parent) {
        var _a, _b, _c, _d, _e, _f, _g, _h, _j;
        super(id);
        this.inputBuffer = {};
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.on("dialogopen", (event) => this.dialogOpened(event));
        this.parent = parent;
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("select.search-operator").on("change", (event) => this.configureForOperator(event));
        (_c = this.$dialog) === null || _c === void 0 ? void 0 : _c.find("[button-type='calendar']").on("click", (event) => this.selectDate(event));
        (_d = this.$dialog) === null || _d === void 0 ? void 0 : _d.find("[button-type='lookup']").on("click", (event) => this.lookup(event));
        (_e = this.$dialog) === null || _e === void 0 ? void 0 : _e.find("input").get().forEach(e => {
            const $input = $(e);
            $input.width(300);
            $input.on("keyup", (event) => this.criteriaEntered(event.target));
            if ($input.attr("numeric")) {
                $input.on("keypress", (event) => this.filterNumericKeyPress(event));
            }
        });
        (_f = this.$dialog) === null || _f === void 0 ? void 0 : _f.find("input[datatype='DateTime']").get().forEach(e => {
            const $input = $(e);
            this.addDatePicker($input, DbNetSuite.datePickerOptions);
        });
        (_g = this.$dialog) === null || _g === void 0 ? void 0 : _g.find("[button-type='clear']").on("click", () => this.clear());
        (_h = this.$dialog) === null || _h === void 0 ? void 0 : _h.find("[button-type='apply']").on("click", () => this.apply());
        (_j = this.$dialog) === null || _j === void 0 ? void 0 : _j.find("[button-type='cancel']").on("click", () => this.close());
    }
    dialogOpened(event) {
        var _a, _b;
        const width = ((_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find("table").first().width()) + 20;
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.dialog("option", "width", width);
    }
    configureForOperator(event) {
        const $select = $(event.target);
        if ($select.attr("dataType") == "Boolean") {
            return;
        }
        const $row = $select.closest("tr");
        switch ($select.val()) {
            case "Between":
            case "NotBetween":
                $row.find(".between").show();
                $row.find("input").show().width(102);
                break;
            case "IsNull":
            case "IsNotNull":
                $row.find(".between").hide();
                $row.find("input").hide();
                break;
            default:
                $row.find(".between").hide();
                $row.find("input").show().width(300);
                break;
        }
    }
    filterNumericKeyPress(e) {
        const txt = String.fromCharCode(e.which);
        if (!txt.match(/[0-9,.]/)) {
            window.clearTimeout(this.timerId);
            const columnIndex = $(e.currentTarget).attr("columnIndex");
            this.timerId = window.setTimeout(() => { this.checkInputBuffer(columnIndex); }, 500);
            if (this.inputBuffer[columnIndex] == null) {
                this.inputBuffer[columnIndex] = txt;
            }
            else {
                this.inputBuffer[columnIndex] += txt;
            }
            return false;
        }
    }
    checkInputBuffer(index) {
        var _a;
        if (this.inputBuffer[index] == null) {
            return;
        }
        let op = null;
        switch (this.inputBuffer[index]) {
            case ">":
                op = "GreaterThan";
                break;
            case "<":
                op = "LessThan";
                break;
            case "<=":
                op = "NotGreaterThan";
                break;
            case ">=":
                op = "NotLessThan";
                break;
            case "<>":
            case "!=":
                op = "NotEqualTo";
                break;
        }
        if (op) {
            const $select = (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find(`[name='searchOperator(${index})']`);
            $select.val(op);
        }
        this.inputBuffer = {};
    }
    criteriaEntered(input) {
        const $input = $(input);
        $input.removeClass("highlight");
        const $row = $input.closest("tr").parent().closest("tr");
        const $select = $row.find("select");
        if ($input.val() == "") {
            $select.val("");
        }
        else if ($select.val() == "") {
            $select.prop("selectedIndex", 1);
        }
    }
    selectDate(event) {
        const $button = $(event.target);
        $button.parent().find("input").datepicker("show");
    }
    lookup(event) {
        const $button = $(event.target);
        const $input = $button.parent().find("input");
        let request;
        if (this.parent instanceof DbNetGrid) {
            request = this.parent.getRequest();
        }
        else if (this.parent instanceof DbNetEdit) {
            request = this.parent.getRequest();
        }
        else {
            return;
        }
        this.parent.lookup($input, request);
    }
    selectTime(event) {
        const $button = $(event.target);
        $button.parent().find("input").timepicker('open');
        event.stopPropagation();
    }
    clear() {
        var _a, _b;
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find(".content").find("input").get().forEach(e => {
            const $input = $(e);
            $input.val("");
            $input.removeClass("highlight");
        });
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find(".content").find("select").get().forEach(e => {
            const $select = $(e);
            $select.val("");
        });
    }
    apply() {
        var _a, _b, _c;
        this.parent.searchParams = [];
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find(".content").find("select").get().forEach(e => {
            const $select = $(e);
            const $row = $select.closest("tr");
            const $input1 = $row.find("input:eq(0)");
            const $input2 = $row.find("input:eq(1)");
            if ($select.val() != "" && ($input1.is(":visible") == false || $input1.val() != "")) {
                const searchParam = {
                    searchOperator: $select.val(),
                    columnIndex: $input1.attr("columnIndex"),
                    value1: $input1.val(),
                    value2: $input2.val()
                };
                this.parent.searchParams.push(searchParam);
            }
        });
        if (this.parent instanceof DbNetGrid) {
            const grid = this.parent;
            grid.clearColumnFilters();
            grid.searchFilterJoin = (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("#searchFilterJoin").val();
            grid.currentPage = 1;
            grid.getPage((response) => this.getPageCallback(response));
        }
        else {
            const edit = this.parent;
            edit.searchFilterJoin = (_c = this.$dialog) === null || _c === void 0 ? void 0 : _c.find("#searchFilterJoin").val();
            edit.currentRow = 1;
            edit.getRows((response) => this.getPageCallback(response));
        }
    }
    getPageCallback(response) {
        if (response.searchParams) {
            response.searchParams.forEach(sp => {
                var _a;
                const $row = (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find(`tr[columnIndex='${sp.columnIndex}']`);
                const $input1 = $row === null || $row === void 0 ? void 0 : $row.find("input:nth-of-type(1)");
                const $input2 = $row === null || $row === void 0 ? void 0 : $row.find("input:nth-of-type(2)");
                if (sp.value1Valid == false) {
                    $input1 === null || $input1 === void 0 ? void 0 : $input1.addClass("highlight");
                }
                if (sp.value2Valid == false) {
                    $input2 === null || $input2 === void 0 ? void 0 : $input2.addClass("highlight");
                }
            });
            if (response.message) {
                this.message(response.message);
            }
        }
    }
}

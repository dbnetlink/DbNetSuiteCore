"use strict";
class SearchDialog extends Dialog {
    constructor(id, parent) {
        var _a, _b, _c, _d, _e, _f, _g, _h, _j, _k, _l;
        super(id);
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.on("dialogopen", (event) => this.dialogOpened(event));
        this.parent = parent;
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("select.search-operator").on("change", (event) => this.configureForOperator(event));
        (_c = this.$dialog) === null || _c === void 0 ? void 0 : _c.find("[button-type='calendar']").on("click", (event) => this.selectDate(event));
        (_d = this.$dialog) === null || _d === void 0 ? void 0 : _d.find("[button-type='clock']").on("click", (event) => this.selectTime(event));
        (_e = this.$dialog) === null || _e === void 0 ? void 0 : _e.find("[button-type='lookup']").on("click", (event) => this.lookup(event));
        (_f = this.$dialog) === null || _f === void 0 ? void 0 : _f.find("input").get().forEach(e => {
            const $input = $(e);
            $input.width(240);
            $input.on("keyup", (event) => this.criteriaEntered(event.target));
        });
        (_g = this.$dialog) === null || _g === void 0 ? void 0 : _g.find("input[datatype='DateTime'").get().forEach(e => {
            const $input = $(e);
            this.addDatePicker($input);
        });
        (_h = this.$dialog) === null || _h === void 0 ? void 0 : _h.find("input[datatype='TimeSpan'").get().forEach(e => {
            const $input = $(e);
            this.addTimePicker($input);
        });
        (_j = this.$dialog) === null || _j === void 0 ? void 0 : _j.find("[button-type='clear']").on("click", () => this.clear());
        (_k = this.$dialog) === null || _k === void 0 ? void 0 : _k.find("[button-type='apply']").on("click", () => this.apply());
        (_l = this.$dialog) === null || _l === void 0 ? void 0 : _l.find("[button-type='cancel']").on("click", () => this.close());
    }
    dialogOpened(event) {
        var _a, _b;
        const height = (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find("table").first().height();
        if (height > 400) {
            (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("div.content").height(400);
        }
    }
    configureForOperator(event) {
        const $select = $(event.target);
        const $row = $select.closest("tr");
        switch ($select.val()) {
            case "Between":
            case "NotBetween":
                $row.find(".between").show();
                $row.find("input").width(102);
                break;
            default:
                $row.find(".between").hide();
                $row.find("input").width(240);
                break;
        }
    }
    pickerSelected() {
        const $row = $(this).closest("tr").parent().closest("tr");
        const $select = $row.find("select");
        if ($select.val() == "") {
            $select.prop("selectedIndex", 1);
        }
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
        this.parent.lookup($input);
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
        var _a, _b;
        this.parent.searchParams = [];
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find(".content").find("select").get().forEach(e => {
            const $select = $(e);
            const $row = $select.closest("tr");
            const $input1 = $row.find("input:eq(0)");
            const $input2 = $row.find("input:eq(1)");
            if ($select.val() != "") {
                const searchParam = {
                    searchOperator: $select.val(),
                    columnIndex: $input1.attr("columnIndex"),
                    value1: $input1.val(),
                    value2: $input2.val()
                };
                this.parent.searchParams.push(searchParam);
            }
        });
        this.parent.clearColumnFilters();
        this.parent.searchFilterJoin = (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("#searchFilterJoin").val();
        this.parent.getPage((response) => this.getPageCallback(response));
    }
    getPageCallback(response) {
        if (response.searchParams) {
            response.searchParams.forEach(sp => {
                var _a;
                const $row = (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find(`tr[columnIndex='${sp.columnIndex}']`);
                const $input1 = $row.find("input:nth-of-type(1)");
                const $input2 = $row.find("input:nth-of-type(2)");
                if (sp.value1Valid == false) {
                    $input1.addClass("highlight");
                }
                if (sp.value2Valid == false) {
                    $input2.addClass("highlight");
                }
            });
            if (response.message) {
                this.message(response.message);
            }
        }
    }
    addDatePicker($input) {
        const options = this.parent.datePickerOptions;
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
    addTimePicker($input) {
        const options = { "zindex": 100000 };
        options.change = this.pickerSelected;
        $input.timepicker(options);
    }
}

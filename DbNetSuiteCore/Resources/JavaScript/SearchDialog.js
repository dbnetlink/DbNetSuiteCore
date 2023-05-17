"use strict";
class SearchDialog extends Dialog {
    constructor(id, parent) {
        super(id);
        this.$dialog.on("dialogopen", (event) => this.dialogOpened(event));
        this.parent = parent;
        this.$dialog.find("select.search-operator").on("change", (event) => this.configureForOperator(event));
        this.$dialog.find("[button-type='calendar']").on("click", (event) => this.selectDate(event));
        this.$dialog.find("[button-type='clock']").on("click", (event) => this.selectTime(event));
        this.$dialog.find("[button-type='lookup']").on("click", (event) => this.lookup(event));
        this.$dialog.find("input").get().forEach(e => {
            var $input = $(e);
            $input.width(240);
            $input.on("keyup", (event) => this.criteriaEntered(event.target));
        });
        this.$dialog.find("input[datatype='DateTime'").get().forEach(e => {
            var $input = $(e);
            this.addDatePicker($input);
        });
        this.$dialog.find("input[datatype='TimeSpan'").get().forEach(e => {
            var $input = $(e);
            this.addTimePicker($input);
        });
        this.$dialog.find("[button-type='clear']").on("click", () => this.clear());
        this.$dialog.find("[button-type='apply']").on("click", () => this.apply());
        this.$dialog.find("[button-type='cancel']").on("click", () => this.close());
    }
    dialogOpened(event) {
        var height = this.$dialog.find("table").first().height();
        if (height > 400) {
            this.$dialog.find("div.content").height(400);
        }
    }
    configureForOperator(event) {
        var $select = $(event.target);
        var $row = $select.closest("tr");
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
        var $row = $(this).closest("tr").parent().closest("tr");
        var $select = $row.find("select");
        if ($select.val() == "") {
            $select.prop("selectedIndex", 1);
        }
    }
    criteriaEntered(input) {
        var $input = $(input);
        $input.removeClass("highlight");
        var $row = $input.closest("tr").parent().closest("tr");
        var $select = $row.find("select");
        if ($input.val() == "") {
            $select.val("");
        }
        else if ($select.val() == "") {
            $select.prop("selectedIndex", 1);
        }
    }
    selectDate(event) {
        var $button = $(event.target);
        $button.parent().find("input").datepicker("show");
    }
    lookup(event) {
        var $button = $(event.target);
        var $input = $button.parent().find("input");
        this.parent.lookup($input);
    }
    selectTime(event) {
        var $button = $(event.target);
        $button.parent().find("input").timepicker('open');
        event.stopPropagation();
    }
    clear() {
        this.$dialog.find(".content").find("input").get().forEach(e => {
            var $input = $(e);
            $input.val("");
            $input.removeClass("highlight");
        });
        this.$dialog.find(".content").find("select").get().forEach(e => {
            var $select = $(e);
            $select.val("");
        });
    }
    apply() {
        this.parent.searchParams = [];
        this.$dialog.find(".content").find("select").get().forEach(e => {
            var $select = $(e);
            var $row = $select.closest("tr");
            var $input1 = $row.find("input:eq(0)");
            var $input2 = $row.find("input:eq(1)");
            if ($select.val() != "") {
                let searchParam = {
                    searchOperator: $select.val(),
                    columnIndex: $input1.attr("columnIndex"),
                    value1: $input1.val(),
                    value2: $input2.val()
                };
                this.parent.searchParams.push(searchParam);
            }
        });
        this.parent.clearColumnFilters();
        this.parent.searchFilterJoin = this.$dialog.find("#searchFilterJoin").val();
        this.parent.getPage((response) => this.getPageCallback(response));
    }
    getPageCallback(response) {
        if (response.searchParams) {
            response.searchParams.forEach(sp => {
                var $row = this.$dialog.find(`tr[columnIndex='${sp.columnIndex}']`);
                var $input1 = $row.find("input:nth-of-type(1)");
                var $input2 = $row.find("input:nth-of-type(2)");
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
        let options = this.parent.datePickerOptions;
        var formats = { D: "DD, MM dd, yy", DDDD: "DD", DDD: "D", MMMM: "MM", MMM: "M", M: "m", MM: "mm", yyyy: "yy" };
        let format = $input.attr("format");
        let pattern;
        for (pattern in formats) {
            var re = new RegExp(`\\b${pattern}\\b`);
            format = format.replace(re, formats[pattern]);
        }
        if (format != undefined)
            if (format != $input.attr("format"))
                options.dateFormat = format;
        options.onSelect = this.pickerSelected;
        $input.datepicker(options);
    }
    addTimePicker($input) {
        var options = { "zindex": 100000 };
        options.change = this.pickerSelected;
        $input.timepicker(options);
    }
}

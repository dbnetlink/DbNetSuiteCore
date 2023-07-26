﻿class SearchDialog extends Dialog {
    parent: DbNetGridEdit;
    constructor(id: string, parent: DbNetGridEdit) {
        super(id);

        this.$dialog?.on("dialogopen", (event) => this.dialogOpened(event));
        this.parent = parent;
        this.$dialog?.find("select.search-operator").on("change", (event) => this.configureForOperator(event));
        this.$dialog?.find("[button-type='calendar']").on("click", (event) => this.selectDate(event));
        this.$dialog?.find("[button-type='clock']").on("click", (event) => this.selectTime(event));
        this.$dialog?.find("[button-type='lookup']").on("click", (event) => this.lookup(event));

        this.$dialog?.find("input").get().forEach(e => {
            const $input = $(e as HTMLInputElement);
            $input.width(240);
            $input.on("keyup", (event) => this.criteriaEntered(event.target))
        });
        this.$dialog?.find("input[datatype='DateTime'").get().forEach(e => {
            const $input = $(e as HTMLInputElement);
            this.addDatePicker($input, this.parent.datePickerOptions);
        });
        this.$dialog?.find("input[datatype='TimeSpan'").get().forEach(e => {
            const $input = $(e as HTMLInputElement);
            this.addTimePicker($input);
        });

        this.$dialog?.find("[button-type='clear']").on("click", () => this.clear());
        this.$dialog?.find("[button-type='apply']").on("click", () => this.apply());
        this.$dialog?.find("[button-type='cancel']").on("click", () => this.close());
    }

    private dialogOpened(event: JQuery.TriggeredEvent): void {
        const width = this.$dialog?.find("table").first().width() as number + 20;
        this.$dialog?.dialog("option", "width", width);
    }

    private configureForOperator(event: JQuery.TriggeredEvent): void {
        const $select = $(event.target as HTMLInputElement);
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
                $row.find("input").show().width(240);
                break;
        }
    }

    private criteriaEntered(input: HTMLInputElement): void {
        const $input = $(input);
        $input.removeClass("highlight");
        const $row = $input.closest("tr").parent().closest("tr");
        const $select = $row.find("select");

        if ($input.val() == "") {
            $select.val("");
        }
        else if ($select.val() == "") {
            $select.prop("selectedIndex", 1)
        }
    }

    private selectDate(event: JQuery.TriggeredEvent): void {
        const $button = $(event.target as HTMLInputElement);
        $button.parent().find("input").datepicker("show");
    }

    private lookup(event: JQuery.TriggeredEvent): void {
        const $button = $(event.target as HTMLInputElement);
        const $input = $button.parent().find("input");
        let request: DbNetGridEditRequest;
        if (this.parent instanceof DbNetGrid) {
            request = (this.parent as DbNetGrid).getRequest();
        }
        else if (this.parent instanceof DbNetEdit) {
            request = (this.parent as DbNetEdit).getRequest();
        }
        else {
            return;
        }
        this.parent.lookup($input, request);
    }

    private selectTime(event: JQuery.TriggeredEvent): void {
        const $button = $(event.target as HTMLInputElement);
        $button.parent().find("input").timepicker('open');
        event.stopPropagation();
    }

    public clear(): void {
        this.$dialog?.find(".content").find("input").get().forEach(e => {
            const $input = $(e as HTMLInputElement);
            $input.val("");
            $input.removeClass("highlight");
        });
        this.$dialog?.find(".content").find("select").get().forEach(e => {
            const $select = $(e as HTMLSelectElement);
            $select.val("");
        });
    }

    private apply(): void {
        this.parent.searchParams = [];
        this.$dialog?.find(".content").find("select").get().forEach(e => {
            const $select = $(e as HTMLSelectElement);
            const $row = $select.closest("tr");
            const $input1 = $row.find("input:eq(0)");
            const $input2 = $row.find("input:eq(1)");
            if ($select.val() != "") {
                const searchParam: SearchParam = {
                    searchOperator: $select.val() as string,
                    columnIndex: $input1.attr("columnIndex") as string,
                    value1: $input1.val() as string,
                    value2: $input2.val() as string
                }
                this.parent.searchParams.push(searchParam);
            }
        });

        if (this.parent instanceof DbNetGrid) {
            const grid = this.parent as DbNetGrid;
            grid.clearColumnFilters();
            grid.searchFilterJoin = this.$dialog?.find("#searchFilterJoin").val() as string;
            grid.currentPage = 1;
            grid.getPage((response: DbNetGridResponse) => this.getPageCallback(response));
        }
        else {
            const edit = this.parent as DbNetEdit;
            edit.searchFilterJoin = this.$dialog?.find("#searchFilterJoin").val() as string;
            edit.currentRow = 1;
            edit.getRows((response: DbNetEditResponse) => this.getPageCallback(response));
        }
    }

    private getPageCallback(response: DbNetGridEditResponse) {
        if (response.searchParams) {
            response.searchParams.forEach(sp => {
                const $row = this.$dialog?.find(`tr[columnIndex='${sp.columnIndex}']`)
                const $input1 = $row?.find("input:nth-of-type(1)");
                const $input2 = $row?.find("input:nth-of-type(2)");
                if (sp.value1Valid == false) {
                    $input1?.addClass("highlight");
                }
                if (sp.value2Valid == false) {
                    $input2?.addClass("highlight");
                }
            });
            if (response.message) {
                this.message(response.message);
            }
        }
    }


}
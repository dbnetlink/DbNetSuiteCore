class FileSearchDialog extends Dialog {
    parent: DbNetFile;
    timerId: number | undefined;
    inputBuffer: Dictionary<string> = {};

    constructor(id: string, parent: DbNetFile) {
        super(id);

        this.$dialog?.on("dialogopen", (event) => this.dialogOpened(event));
        this.parent = parent;
        this.$dialog?.find("select.search-operator").on("change", (event) => this.configureForOperator(event));
        this.$dialog?.find("[button-type='calendar']").on("click", (event) => this.selectDate(event));

        this.$dialog?.find("input").get().forEach(e => {
            const $input = $(e as HTMLInputElement);
            $input.width(300);
            $input.on("keyup", (event) => this.criteriaEntered(event.target))

            if ($input.attr("numeric")) {
                $input.on("keypress", (event) => this.filterNumericKeyPress(event))
            }
        });
        this.$dialog?.find("input[datatype='DateTime']").get().forEach(e => {
            const $input = $(e as HTMLInputElement);
            this.addDatePicker($input, this.parent.datePickerOptions);
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
                $row.find("input").show().width(300);
                break;
        }
    }

    private filterNumericKeyPress(e: JQuery.KeyPressEvent) {
        const txt = String.fromCharCode(e.which);
        if (!txt.match(/[0-9,.]/)) {
            window.clearTimeout(this.timerId);
            const columnIndex = $(e.currentTarget).attr("columnIndex") as string
            this.timerId = window.setTimeout(() => { this.checkInputBuffer(columnIndex) }, 500);
            if (this.inputBuffer[columnIndex] == null) {
                this.inputBuffer[columnIndex] = txt;
            }
            else {
                this.inputBuffer[columnIndex] += txt;
            }
            return false;
        }
    }

    private checkInputBuffer(index: string) {
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
            const $select = this.$dialog?.find(`[name='searchOperator(${index})']`) as JQuery<HTMLSelectElement>;
            $select.val(op);
        }
        this.inputBuffer = {};
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
            const $unit1 = $row.find("select:eq(0)");
            const $unit2 = $row.find("select:eq(1)");
            if ($select.val() != "" && ($input1.is(":visible") == false || $input1.val() != "")) {
                const searchParam: SearchParam = {
                    searchOperator: $select.val() as string,
                    columnType: $input1.data("type") as string,
                    value1: $input1.val() as string,
                    value2: $input2.val() as string
                }

                if ($unit1.length) {
                    searchParam.unit1 = $unit1.val() as string;
                    searchParam.unit2 = $unit2.val() as string;
                }
                this.parent.searchParams.push(searchParam);
            }
        });

        const parent = this.parent;
        parent.callServer("validate-search-params", (response: DbNetFileResponse) => this.getPageCallback(response))
    }

    private getPageCallback(response: DbNetGridEditResponse) {
        if (response.error) {
            if (response.searchParams) {
                response.searchParams.forEach(sp => {
                    const $row = this.$dialog?.find(`tr[columntype='${sp.columnType}']`)
                    const $input1 = $row?.find("input:nth-of-type(1)");
                    const $input2 = $row?.find("input:nth-of-type(2)");
                    $input1?.removeClass("highlight");
                    $input2?.removeClass("highlight");
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
        else {
            const searchFilterJoin = this.$dialog?.find("#searchFilterJoin").val() as string;
            const includeSubfolders = this.$dialog?.find("#includeSubfolders").prop("checked");

            this.parent.applySearch(searchFilterJoin, includeSubfolders);
        }
    }
}
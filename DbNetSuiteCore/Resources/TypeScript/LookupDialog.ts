class LookupDialog extends Dialog {
    parent: DbNetGridEdit;
    $input: JQuery<HTMLInputElement> | undefined;
    $select: JQuery<HTMLSelectElement> | undefined;
    columnIndex: number | undefined;
    title: string;

    constructor(id: string, parent: DbNetGridEdit) {
        super(id);
        this.$dialog?.on("dialogopen", (event) => this.dialogOpened(event));
        this.parent = parent;
        this.$dialog?.find("[button-type='apply']").on("click", () => this.apply());
        this.$dialog?.find("[button-type='cancel']").on("click", () => this.close());
        this.title = this.$dialog?.dialog("option", "title");
    }

    private dialogOpened(event: JQuery.TriggeredEvent): void {
        this.$dialog?.dialog("option", "width", 320);
    }

    public update(response: DbNetSuiteResponse, $input: JQuery<HTMLInputElement>): void {
        this.$input = $input;
        this.columnIndex = parseInt($input.attr("columnIndex") as string);
        const dbColumn: DbColumn = this.parent.columns.find((col) => { return col.index! == this.columnIndex }) as DbColumn;
        this.$dialog?.dialog("option", "title", `${dbColumn.label} ${this.title}`);
        this.$dialog?.find("div.select").html(response.html);

        this.$select = this.$dialog?.find("select") as JQuery<HTMLSelectElement>;
        this.$select.val(($input?.val() as string).split(',')).width(300);
        if (this.contextIsEdit()) {
            this.$select.removeAttr("multiple")
        }
        else {
            this.$select.attr("multiple", "multiple");
        }
    }

    private contextIsEdit() {
        return this.$input?.hasClass("dbnetedit");
    }
 
    private apply(): void {
        if (this.contextIsEdit()) {
            if (this.$select?.find(":selected").length) {
                this.$input?.val(this.$select?.val() as string).trigger("keyup");
            }
        }
        else {
            const selectedValues = this.$select?.val() as string[];
            this.$input?.val(selectedValues.join(',')).trigger("keyup");
        }

        this.close();
    }
}
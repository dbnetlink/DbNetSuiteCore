class LookupDialog extends Dialog {
    parent: DbNetGrid;
    $input: JQuery<HTMLInputElement> | undefined;
    $select: JQuery<HTMLSelectElement> | undefined;
    columnIndex: number | undefined;
    title: string;

    constructor(id: string, parent: DbNetGrid) {
        super(id);
        this.$dialog!.on("dialogopen", (event) => this.dialogOpened(event));
        this.parent = parent;
        this.$dialog!.find("[button-type='apply']").on("click", () => this.apply());
        this.$dialog!.find("[button-type='cancel']").on("click", () => this.close());
        this.title = this.$dialog!.dialog("option", "title");
    }

    private dialogOpened(event: JQuery.TriggeredEvent): void {
        this.$dialog!.dialog("option", "width", 320);
    }

    public update(response: DbNetGridResponse, $input:JQuery<HTMLInputElement>): void {
        this.$input = $input;
        this.columnIndex = parseInt($input.attr("columnIndex") as string);
        let gridColumn: GridColumn = this.parent.columns.find((col) => { return col.index! == this.columnIndex }) as GridColumn;
        this.$dialog!.dialog("option", "title", `${gridColumn.label} ${this.title}`);
        this.$dialog!.find("div.select").html(response.data);
        this.$dialog!.find("select").val(($input!.val() as string).split(',')).width(300);
    }
 
    private apply(): void {
        var $select: JQuery<HTMLSelectElement> = this.$dialog!.find("select");
        var selectedValues = $select.val() as string[];
        this.$input!.val(selectedValues.join(',')).trigger("keyup");
        this.close();
    }
}
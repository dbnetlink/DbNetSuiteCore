class EditDialog extends Dialog {
    parent: DbNetGrid;
    constructor(id: string, parent:DbNetGrid) {
        super(id);
        this.parent = parent;

        this.$dialog?.find(".next-btn").on("click", () => this.nextRecord());
        this.$dialog?.find(".previous-btn").on("click", () => this.previousRecord());
        this.$dialog?.find("[button-type='close']").on("click", () => this.close());
    }

    update():void {
        this.open();
        this.setHeight();
    }

    private setHeight() {
        const height = this.$dialog?.find("table").first().height() as number;
        this.$dialog?.find("div.content").height(height > 600 ? 600 : height);
    }

    private nextRecord() {
        $(this.parent.selectedRow()).next().trigger("click")
    }

    private previousRecord() {
        $(this.parent.selectedRow()).prev().trigger("click")
    }
}
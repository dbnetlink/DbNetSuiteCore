class EditDialog extends Dialog {
    parent: DbNetGrid;
    editControl: DbNetEdit;
    constructor(id: string, parent:DbNetGrid, editControl:DbNetEdit) {
        super(id);
        this.parent = parent;
        this.editControl = editControl;
        this.$dialog?.find("[button-type='next']").off().on("click", () => this.nextRecord());
        this.$dialog?.find("[button-type='previous']").off().on("click", () => this.previousRecord());
        this.$dialog?.find("[button-type='close']").on("click", () => this.close());
    }

    update():void {
        this.open();
     //   this.setSize();

        const $row = $(this.parent.selectedRow());
        this.$dialog?.find("[button-type='next']").prop("disabled", $row.next('.data-row').length == 0);
        this.$dialog?.find("[button-type='previous']").prop("disabled", $row.prev('.data-row').length == 0);
    }

    /*
    private setSize() {
        const height = this.$dialog?.find("table").first().height() as number;
        const width = this.$dialog?.find("table").first().width() as number;
        this.$dialog?.find("div.content").height(height > 600 ? 600 : height).width(width > 800 ? 800 : height);
    }
    */

    private nextRecord() {
        $(this.parent.selectedRow()).next().trigger("click")
    }

    private previousRecord() {
        $(this.parent.selectedRow()).prev().trigger("click")
    }
}
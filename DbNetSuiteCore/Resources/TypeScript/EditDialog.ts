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
        const $row = $(this.parent.selectedRow());
        this.$dialog?.find("[button-type='next']").prop("disabled", $row.next('.data-row').length == 0);
        this.$dialog?.find("[button-type='previous']").prop("disabled", $row.prev('.data-row').length == 0);
    }

    private nextRecord() {
        $(this.parent.selectedRow()).next().trigger("click")
    }

    private previousRecord() {
        $(this.parent.selectedRow()).prev().trigger("click")
    }
}
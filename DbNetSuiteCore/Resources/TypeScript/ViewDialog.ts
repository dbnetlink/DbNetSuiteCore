class ViewDialog extends Dialog {
    parent: DbNetGrid;
    dialogWidth = 0;
    constructor(id: string, parent:DbNetGrid) {
        super(id);
        this.parent = parent;

        this.$dialog?.find(".next-btn").on("click", () => this.viewNextRecord());
        this.$dialog?.find(".previous-btn").on("click", () => this.viewPreviousRecord());
        this.$dialog?.find("[button-type='close']").on("click", () => this.close());
    }

    update(response: DbNetGridResponse, $row:JQuery<HTMLTableRowElement>):void {
        this.$dialog?.find("div.content").html(response.data)

        this.$dialog?.find("button.download").get().forEach(e => {
            $(e).on("click", (e) => this.parent.downloadCellData(e.currentTarget, false));
        });

        this.$dialog?.find("img.image").get().forEach(e => {
            this.parent.downloadCellData(e.parentElement!, true);
            $(e).on('load', (e) => this.setSize());
        });

        this.open();

        this.$dialog?.find(".next-btn").prop("disabled", $row.next('.data-row').length == 0)
        this.$dialog?.find(".previous-btn").prop("disabled", $row.prev('.data-row').length == 0)

        const args: ViewRecordSelectedArgs = {
            dialog: this.$dialog,
            record: response.record
        }
        this.setSize();
        this.parent.fireEvent("onViewRecordSelected", args);
    }

    private setSize() {
        const width = this.$dialog?.find("table").first().width() as number + 20;

        if (width > this.dialogWidth) {
            this.$dialog?.dialog("option", "width", width);
        }

        this.dialogWidth = this.$dialog?.dialog("option", "width")
    }

    private viewNextRecord() {
        $(this.parent.selectedRow()).next().trigger("click")
    }

    private viewPreviousRecord() {
        $(this.parent.selectedRow()).prev().trigger("click")
    }
}
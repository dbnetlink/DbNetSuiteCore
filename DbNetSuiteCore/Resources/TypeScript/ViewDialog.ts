class ViewDialog extends Dialog {
    parent: DbNetGrid;
    dialogWidth = 0;
    constructor(id: string, parent:DbNetGrid) {
        super(id);
        this.parent = parent;
        this.$dialog?.on("dialogopen", (event) => this.dialogOpened(event));
        this.$dialog?.find("table").first().css("max-width", this.windowWidth * 0.6 )
        this.$dialog?.find(".next-btn").on("click", () => this.viewNextRecord());
        this.$dialog?.find(".previous-btn").on("click", () => this.viewPreviousRecord());
        this.$dialog?.find("[button-type='close']").on("click", () => this.close());
    }

    update(response: DbNetGridResponse, $row:JQuery<HTMLTableRowElement>):void {
        this.$dialog?.find("div.content").html(response.data)

        this.$dialog?.find("button.download").get().forEach(e => {
            $(e).on("click", (e) => this.parent.downloadBinaryData(e.currentTarget, false));
        });

        this.$dialog?.find("img.image").get().forEach(e => {
            this.parent.downloadBinaryData(e.parentElement!, true);
           // $(e).on('load', (e) => this.setSize());
        });

        this.open();

        this.$dialog?.find(".next-btn").prop("disabled", $row.next('.data-row').length == 0)
        this.$dialog?.find(".previous-btn").prop("disabled", $row.prev('.data-row').length == 0)

        const args: ViewRecordSelectedArgs = {
            dialog: this.$dialog,
            record: response.record
        }
        this.parent.fireEvent("onViewRecordSelected", args);
    }

    private dialogOpened(event: JQuery.TriggeredEvent): void {
        const width = this.$dialog?.find("table").first().width() as number + 20;
        this.$dialog?.dialog("option", "width", width);
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
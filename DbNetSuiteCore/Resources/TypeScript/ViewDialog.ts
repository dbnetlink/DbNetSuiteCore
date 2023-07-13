class ViewDialog extends Dialog {
    parent: DbNetGrid;
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
            $(e).on('load', (e) => this.setHeight());
        });

        this.open();
        this.setHeight();

        this.$dialog?.find(".next-btn").prop("disabled", $row.next('.data-row').length == 0)
        this.$dialog?.find(".previous-btn").prop("disabled", $row.prev('.data-row').length == 0)

        const args: ViewRecordSelectedArgs = {
            dialog: this.$dialog,
            record: response.record
        }

        this.parent.fireEvent("onViewRecordSelected", args);
    }

    private setHeight() {
        const height = this.$dialog?.find("table").first().height() as number;
        if (height > 600) {
            this.$dialog?.find("div.content").height(600);
        }
        else {
            this.$dialog?.find("div.content").height(height);
        }
    }

    private viewNextRecord() {
        $(this.parent.selectedRow()).next().trigger("click")
    }

    private viewPreviousRecord() {
        $(this.parent.selectedRow()).prev().trigger("click")
    }
}
class Dialog {
    $dialog: JQuery<HTMLElement> | undefined;
    width: number = 600;
    maxWidth: number = 800;

    constructor(id:string) {
        this.$dialog = $(`#${id}`);
        this.$dialog.addClass("dialog");
        this.$dialog.dialog({
            autoOpen: false,
            width: this.width,
            maxWidth: this.maxWidth
        });
        this.$dialog!.find(".message").html("&nbsp;")
    }

    open(): void {
        if (this.$dialog) {
            this.$dialog.dialog("open");
        }
    }

    isOpen(): boolean {
        if (this.$dialog) {
            return this.$dialog.dialog("isOpen");
        }
        return false;
    }

    close(): void {
        if (this.$dialog) {
            this.$dialog.dialog("close");
        }
    }

    message(msg: string): void {
        this.$dialog!.find(".message").html(msg).addClass("highlight");
        setInterval(() => this.clearMessage(), 5000);
    }

    clearMessage(): void {
        this.$dialog!.find(".message").html("&nbsp;").removeClass("highlight");
    }
}
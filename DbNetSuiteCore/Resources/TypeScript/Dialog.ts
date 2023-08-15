class Dialog extends DbNetSuite {
    $dialog: JQuery<HTMLElement> | undefined;
    windowWidth = $(window).width() as number;
    windowHeight = $(window).height() as number;

    constructor(id: string) {
        super(null);
        this.$dialog = $(`#${id}`);

        if (this.$dialog.length == 0) {
            alert(`Unable to find dialog with Id => ${id}`)
        }
        this.$dialog.addClass("dialog");
 
        const options = {
            autoOpen: false,
            width: "auto",
            autoResize: true,
            maxWidth: this.windowWidth - 100,
            maxHeight: this.windowHeight - 100

        } as JQueryUI.DialogOptions;

        //options.modal = true;
        this.$dialog.dialog(options);
        this.$dialog?.find(".message").html("&nbsp;")
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
        this.$dialog?.find(".message").html(msg).addClass("highlight");
        setTimeout(() => this.clearMessage(), 5000);
    }

    clearMessage(): void {
        this.$dialog?.find(".message").html("&nbsp;").removeClass("highlight");
    }

    protected button(type: string): JQuery<HTMLElement> {
        return this.$dialog?.find(`.${type}-btn`) as JQuery<HTMLElement>;
    }
}
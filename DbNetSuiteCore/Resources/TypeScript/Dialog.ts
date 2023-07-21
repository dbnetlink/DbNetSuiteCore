﻿class Dialog extends DbNetSuite {
    $dialog: JQuery<HTMLElement> | undefined;
    width = 600;
    maxWidth = 800;

    constructor(id: string) {
        super(null);
        this.$dialog = $(`#${id}`);
        this.$dialog.addClass("dialog");
        const windowWidth = $(window).width() as number;
        const windowHeight = $(window).height() as number;

        const options = {
            autoOpen: false,
            width: "auto",
            autoResize: true,
            maxWidth: windowWidth - 100,
            maxHeight: windowHeight - 100

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
        setInterval(() => this.clearMessage(), 5000);
    }

    clearMessage(): void {
        this.$dialog?.find(".message").html("&nbsp;").removeClass("highlight");
    }
}
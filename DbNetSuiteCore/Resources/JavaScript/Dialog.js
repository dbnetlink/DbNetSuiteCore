"use strict";
class Dialog extends DbNetSuite {
    constructor(id) {
        super(null);
        this.width = 600;
        this.maxWidth = 800;
        this.$dialog = $(`#${id}`);
        this.$dialog.addClass("dialog");
        const options = {
            autoOpen: false,
            width: "auto",
            autoResize: true
            //  width: this.width,
            //   maxWidth: this.maxWidth,
        };
        //options.modal = true;
        this.$dialog.dialog(options);
        this.$dialog.find(".message").html("&nbsp;");
    }
    open() {
        if (this.$dialog) {
            this.$dialog.dialog("open");
        }
    }
    isOpen() {
        if (this.$dialog) {
            return this.$dialog.dialog("isOpen");
        }
        return false;
    }
    close() {
        if (this.$dialog) {
            this.$dialog.dialog("close");
        }
    }
    message(msg) {
        this.$dialog.find(".message").html(msg).addClass("highlight");
        setInterval(() => this.clearMessage(), 5000);
    }
    clearMessage() {
        this.$dialog.find(".message").html("&nbsp;").removeClass("highlight");
    }
}

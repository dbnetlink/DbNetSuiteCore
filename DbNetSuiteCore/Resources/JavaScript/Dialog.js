"use strict";
class Dialog {
    constructor(id) {
        this.width = 600;
        this.maxWidth = 800;
        this.$dialog = $(`#${id}`);
        this.$dialog.addClass("dialog");
        this.$dialog.dialog({
            autoOpen: false,
            width: this.width,
            maxWidth: this.maxWidth
        });
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

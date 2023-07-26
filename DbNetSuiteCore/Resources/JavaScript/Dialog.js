"use strict";
class Dialog extends DbNetSuite {
    constructor(id) {
        var _a;
        super(null);
        this.width = 600;
        this.maxWidth = 800;
        this.$dialog = $(`#${id}`);
        this.$dialog.addClass("dialog");
        const windowWidth = $(window).width();
        const windowHeight = $(window).height();
        const options = {
            autoOpen: false,
            width: "auto",
            autoResize: true,
            maxWidth: windowWidth - 100,
            maxHeight: windowHeight - 100
        };
        //options.modal = true;
        this.$dialog.dialog(options);
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find(".message").html("&nbsp;");
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
        var _a;
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find(".message").html(msg).addClass("highlight");
        setTimeout(() => this.clearMessage(), 5000);
    }
    clearMessage() {
        var _a;
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find(".message").html("&nbsp;").removeClass("highlight");
    }
}

"use strict";
class MessageBox extends Dialog {
    constructor(id) {
        super(id);
    }
    show(text) {
        var _a;
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find(".message-box-text").html(text);
        this.open();
    }
}

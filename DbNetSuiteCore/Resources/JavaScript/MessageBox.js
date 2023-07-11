"use strict";
class MessageBox extends Dialog {
    constructor(id) {
        super(id);
    }
    show(message, type, callback) {
        if (this.messageBox == undefined) {
            this.post("message-box", {})
                .then((response) => {
                var _a;
                (_a = this.element) === null || _a === void 0 ? void 0 : _a.append(response.dialog);
                this.messageBox = new MessageBox(`${this.id}_message_box`);
            });
        }
        this.messageBox.show(message, type);
    }
}

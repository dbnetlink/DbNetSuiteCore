"use strict";
var MessageBoxType;
(function (MessageBoxType) {
    MessageBoxType[MessageBoxType["Error"] = 0] = "Error";
    MessageBoxType[MessageBoxType["Confirm"] = 1] = "Confirm";
    MessageBoxType[MessageBoxType["Info"] = 2] = "Info";
})(MessageBoxType || (MessageBoxType = {}));
var MessageBoxButtonType;
(function (MessageBoxButtonType) {
    MessageBoxButtonType[MessageBoxButtonType["Confirm"] = 0] = "Confirm";
    MessageBoxButtonType[MessageBoxButtonType["Cancel"] = 1] = "Cancel";
    MessageBoxButtonType[MessageBoxButtonType["Ok"] = 2] = "Ok";
})(MessageBoxButtonType || (MessageBoxButtonType = {}));
class MessageBox extends Dialog {
    constructor(id) {
        var _a, _b;
        super(id);
        this.button("ok").on("click", () => this.buttonPressed(MessageBoxButtonType.Ok));
        this.button("confirm").on("click", () => this.buttonPressed(MessageBoxButtonType.Confirm));
        this.button("cancel").on("click", () => this.buttonPressed(MessageBoxButtonType.Cancel));
        this.$text = (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find(".message-box-text");
        this.$icon = (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find(".message-box-icon");
    }
    show(messageBoxType, text, element = null, callback) {
        var _a, _b, _c, _d, _e;
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.dialog("option", "width", "auto");
        if (text.length > 200) {
            (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.dialog("option", "width", 600);
        }
        this.$text.html(text);
        if (element) {
            (_c = this.$dialog) === null || _c === void 0 ? void 0 : _c.dialog("option", "position", { of: element.get(0) });
        }
        this.callback = callback;
        this.hideButtons();
        this.$icon.removeClass("info").removeClass("confirm").removeClass("error");
        switch (messageBoxType) {
            case MessageBoxType.Info:
            case MessageBoxType.Error:
                (_d = this.$dialog) === null || _d === void 0 ? void 0 : _d.dialog("option", "title", messageBoxType == MessageBoxType.Info ? "Information" : "Error");
                this.button("ok").show();
                this.$icon.addClass(messageBoxType == MessageBoxType.Info ? "info" : "error");
                break;
            case MessageBoxType.Confirm:
                (_e = this.$dialog) === null || _e === void 0 ? void 0 : _e.dialog("option", "title", "Confirm");
                this.button("confirm").show();
                this.button("cancel").show();
                this.$icon.addClass("confirm");
                break;
        }
        this.open();
    }
    hideButtons() {
        this.button("confirm").hide();
        this.button("cancel").hide();
        this.button("ok").hide();
    }
    button(type) {
        var _a;
        return (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find(`.${type}-btn`);
    }
    buttonPressed(type) {
        this.close();
        if (this.callback) {
            this.callback(type);
        }
    }
}

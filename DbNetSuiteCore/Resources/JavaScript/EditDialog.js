"use strict";
class EditDialog extends Dialog {
    constructor(id, parent) {
        var _a, _b, _c;
        super(id);
        this.parent = parent;
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find(".next-btn").on("click", () => this.nextRecord());
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find(".previous-btn").on("click", () => this.previousRecord());
        (_c = this.$dialog) === null || _c === void 0 ? void 0 : _c.find("[button-type='close']").on("click", () => this.close());
    }
    update() {
        this.open();
        this.setHeight();
    }
    setHeight() {
        var _a, _b;
        const height = (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find("table").first().height();
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("div.content").height(height > 600 ? 600 : height);
    }
    nextRecord() {
        $(this.parent.selectedRow()).next().trigger("click");
    }
    previousRecord() {
        $(this.parent.selectedRow()).prev().trigger("click");
    }
}

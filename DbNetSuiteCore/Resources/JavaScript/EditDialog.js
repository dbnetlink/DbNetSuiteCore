"use strict";
class EditDialog extends Dialog {
    constructor(id, parent, editControl) {
        var _a, _b, _c;
        super(id);
        this.parent = parent;
        this.editControl = editControl;
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find("[button-type='next']").off().on("click", () => this.nextRecord());
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("[button-type='previous']").off().on("click", () => this.previousRecord());
        (_c = this.$dialog) === null || _c === void 0 ? void 0 : _c.find("[button-type='close']").on("click", () => this.close());
    }
    update() {
        var _a, _b, _c;
        this.open();
        const $row = $(this.parent.selectedRow());
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find("[button-type='next']").prop("disabled", $row.next('.data-row').length == 0);
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("[button-type='previous']").prop("disabled", $row.prev('.data-row').length == 0);
        (_c = this.editControl) === null || _c === void 0 ? void 0 : _c.getRecord($row.data('pk'));
    }
    insert() {
        var _a;
        this.open();
        (_a = this.editControl) === null || _a === void 0 ? void 0 : _a.insertRecord();
    }
    nextRecord() {
        $(this.parent.selectedRow()).next().trigger("click");
    }
    previousRecord() {
        $(this.parent.selectedRow()).prev().trigger("click");
    }
}

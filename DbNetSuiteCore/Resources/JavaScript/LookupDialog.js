"use strict";
class LookupDialog extends Dialog {
    constructor(id, parent) {
        var _a, _b, _c, _d;
        super(id);
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.on("dialogopen", (event) => this.dialogOpened(event));
        this.parent = parent;
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("[button-type='apply']").on("click", () => this.apply());
        (_c = this.$dialog) === null || _c === void 0 ? void 0 : _c.find("[button-type='cancel']").on("click", () => this.close());
        this.title = (_d = this.$dialog) === null || _d === void 0 ? void 0 : _d.dialog("option", "title");
    }
    dialogOpened(event) {
        var _a;
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.dialog("option", "width", 320);
    }
    update(response, $input) {
        var _a, _b, _c;
        this.$input = $input;
        this.columnIndex = parseInt($input.attr("columnIndex"));
        const dbColumn = this.parent.columns.find((col) => { return col.index == this.columnIndex; });
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.dialog("option", "title", `${dbColumn.label} ${this.title}`);
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("div.select").html(response.html);
        (_c = this.$dialog) === null || _c === void 0 ? void 0 : _c.find("select").val(($input === null || $input === void 0 ? void 0 : $input.val()).split(',')).width(300);
    }
    apply() {
        var _a, _b;
        const $select = (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find("select");
        const selectedValues = $select.val();
        (_b = this.$input) === null || _b === void 0 ? void 0 : _b.val(selectedValues.join(',')).trigger("keyup");
        this.close();
    }
}

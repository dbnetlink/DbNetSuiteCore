"use strict";
class ViewDialog extends Dialog {
    constructor(id, parent) {
        var _a, _b, _c, _d, _e;
        super(id);
        this.dialogWidth = 0;
        this.parent = parent;
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.on("dialogopen", (event) => this.dialogOpened(event));
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("table").first().css("max-width", this.windowWidth * 0.6);
        (_c = this.$dialog) === null || _c === void 0 ? void 0 : _c.find(".next-btn").on("click", () => this.viewNextRecord());
        (_d = this.$dialog) === null || _d === void 0 ? void 0 : _d.find(".previous-btn").on("click", () => this.viewPreviousRecord());
        (_e = this.$dialog) === null || _e === void 0 ? void 0 : _e.find("[button-type='close']").on("click", () => this.close());
    }
    update(response, $row) {
        var _a, _b, _c, _d, _e;
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find("div.content").html(response.data);
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("button.download").get().forEach(e => {
            $(e).on("click", (e) => this.parent.downloadBinaryData(e.currentTarget, false));
        });
        (_c = this.$dialog) === null || _c === void 0 ? void 0 : _c.find("img.image").get().forEach(e => {
            this.parent.downloadBinaryData(e, true);
            // $(e).on('load', (e) => this.setSize());
        });
        this.open();
        (_d = this.$dialog) === null || _d === void 0 ? void 0 : _d.find(".next-btn").prop("disabled", $row.next('.data-row').length == 0);
        (_e = this.$dialog) === null || _e === void 0 ? void 0 : _e.find(".previous-btn").prop("disabled", $row.prev('.data-row').length == 0);
        const args = {
            dialog: this.$dialog,
            record: response.record,
            row: this.parent.selectedRow()
        };
        this.parent.fireEvent("onViewRecordSelected", args);
        this.setSize();
    }
    viewElement(columnName) {
        var _a;
        return (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find("table.view-content-table").find(`[data-columnname='${columnName.toLowerCase()}']`).get(0);
    }
    dialogOpened(event) {
        var _a, _b;
        const width = ((_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find("table").first().width()) + 20;
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.dialog("option", "width", width);
    }
    setSize() {
        var _a, _b, _c;
        const width = ((_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find("table").first().width()) + 20;
        if (width > this.dialogWidth) {
            (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.dialog("option", "width", width);
        }
        this.dialogWidth = (_c = this.$dialog) === null || _c === void 0 ? void 0 : _c.dialog("option", "width");
    }
    viewNextRecord() {
        $(this.parent.selectedRow()).next().trigger("click");
    }
    viewPreviousRecord() {
        $(this.parent.selectedRow()).prev().trigger("click");
    }
}

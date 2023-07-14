"use strict";
class ViewDialog extends Dialog {
    constructor(id, parent) {
        var _a, _b, _c;
        super(id);
        this.parent = parent;
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find(".next-btn").on("click", () => this.viewNextRecord());
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find(".previous-btn").on("click", () => this.viewPreviousRecord());
        (_c = this.$dialog) === null || _c === void 0 ? void 0 : _c.find("[button-type='close']").on("click", () => this.close());
    }
    update(response, $row) {
        var _a, _b, _c, _d, _e;
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find("div.content").html(response.data);
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("button.download").get().forEach(e => {
            $(e).on("click", (e) => this.parent.downloadCellData(e.currentTarget, false));
        });
        (_c = this.$dialog) === null || _c === void 0 ? void 0 : _c.find("img.image").get().forEach(e => {
            this.parent.downloadCellData(e.parentElement, true);
            $(e).on('load', (e) => this.setHeight());
        });
        this.open();
        this.setHeight();
        (_d = this.$dialog) === null || _d === void 0 ? void 0 : _d.find(".next-btn").prop("disabled", $row.next('.data-row').length == 0);
        (_e = this.$dialog) === null || _e === void 0 ? void 0 : _e.find(".previous-btn").prop("disabled", $row.prev('.data-row').length == 0);
        const args = {
            dialog: this.$dialog,
            record: response.record
        };
        this.parent.fireEvent("onViewRecordSelected", args);
    }
    setHeight() {
        var _a, _b;
        const height = (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find("table").first().height();
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("div.content").height(height > 600 ? 600 : height);
    }
    viewNextRecord() {
        $(this.parent.selectedRow()).next().trigger("click");
    }
    viewPreviousRecord() {
        $(this.parent.selectedRow()).prev().trigger("click");
    }
}

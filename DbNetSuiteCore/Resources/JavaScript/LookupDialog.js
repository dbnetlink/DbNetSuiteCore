"use strict";
class LookupDialog extends Dialog {
    constructor(id, parent) {
        super(id);
        this.$dialog.on("dialogopen", (event) => this.dialogOpened(event));
        this.parent = parent;
        this.$dialog.find("[button-type='apply']").on("click", () => this.apply());
        this.$dialog.find("[button-type='cancel']").on("click", () => this.close());
        this.title = this.$dialog.dialog("option", "title");
    }
    dialogOpened(event) {
        this.$dialog.dialog("option", "width", 320);
    }
    update(response, $input) {
        this.$input = $input;
        this.columnIndex = parseInt($input.attr("columnIndex"));
        let gridColumn = this.parent.columns.find((col) => { return col.index == this.columnIndex; });
        this.$dialog.dialog("option", "title", `${gridColumn.label} ${this.title}`);
        this.$dialog.find("div.select").html(response.data);
        this.$dialog.find("select").val($input.val().split(',')).width(300);
    }
    apply() {
        var $select = this.$dialog.find("select");
        var selectedValues = $select.val();
        this.$input.val(selectedValues.join(',')).trigger("keyup");
        this.close();
    }
}

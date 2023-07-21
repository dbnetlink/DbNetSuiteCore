"use strict";
class BrowseDialog extends Dialog {
    constructor(id, parent, gridControl) {
        super(id);
        this.parent = parent;
        this.gridControl = gridControl;
    }
    show() {
        this.selectRow(this.parent.currentRow);
        this.open();
    }
    selectRow(currentRow) {
        const selectedRow = this.gridControl.selectedRow();
        if (currentRow != selectedRow.rowIndex) {
            const $tr = $(this.gridControl.table()).find('tr').eq(currentRow);
            this.gridControl.selectRow($tr);
            $tr[0].scrollIntoView(false);
        }
    }
}

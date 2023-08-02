"use strict";
class BrowseDialog extends Dialog {
    constructor(id, gridControl) {
        super(id);
        this.gridControl = gridControl;
    }
    show(currentRow) {
        this.open();
        this.selectRow(currentRow);
    }
    selectRow(currentRow) {
        const selectedRow = this.gridControl.selectedRow();
        if (!selectedRow || currentRow != selectedRow.rowIndex) {
            const $tr = $(this.gridControl.table()).find('tr').eq(currentRow);
            this.gridControl.selectRow($tr);
            $tr[0].scrollIntoView();
        }
    }
}

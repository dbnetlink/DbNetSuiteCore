"use strict";
class BrowseDialog extends Dialog {
    constructor(id, parent, gridControl) {
        super(id);
        this.gridControl = gridControl;
    }
    show(currentRow) {
        this.open();
        this.selectRow(currentRow);
    }
    selectRow(currentRow) {
        const selectedRow = this.gridControl.selectedRow();
        //const topBottom = (selectedRow && currentRow < selectedRow.rowIndex) ? true : false;
        if (!selectedRow || currentRow != selectedRow.rowIndex) {
            const $tr = $(this.gridControl.table()).find('tr').eq(currentRow);
            this.gridControl.selectRow($tr);
            $tr[0].scrollIntoView();
        }
    }
}

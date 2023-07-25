﻿class BrowseDialog extends Dialog {
    gridControl: DbNetGrid;
    constructor(id: string, parent: DbNetEdit, gridControl: DbNetGrid) {
        super(id);
        this.gridControl = gridControl;
    }

    show(currentRow: number): void {
        this.selectRow(currentRow);
        this.open();
    }

    selectRow(currentRow:number): void {
        const selectedRow = this.gridControl.selectedRow() 

        if (!selectedRow || currentRow != selectedRow.rowIndex) {
            const $tr = $(this.gridControl.table()).find('tr').eq(currentRow);
            this.gridControl.selectRow($tr);
            $tr[0].scrollIntoView(false);
        }
    }
}
class BrowseDialog extends Dialog {
    parent: DbNetEdit;
    gridControl: DbNetGrid;
    constructor(id: string, parent: DbNetEdit, gridControl: DbNetGrid) {
        super(id);
        this.parent = parent;
        this.gridControl = gridControl;
    }

    show(): void {
        this.selectRow(this.parent.currentRow);
        this.open();
    }

    selectRow(currentRow:number): void {
        const selectedRow = this.gridControl.selectedRow() 

        if (currentRow != selectedRow.rowIndex) {
            const $tr = $(this.gridControl.table()).find('tr').eq(currentRow);
            this.gridControl.selectRow($tr);
            $tr[0].scrollIntoView(false);
        }
    }
}
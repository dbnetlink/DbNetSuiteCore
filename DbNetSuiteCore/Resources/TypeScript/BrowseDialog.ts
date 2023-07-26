class BrowseDialog extends Dialog {
    gridControl: DbNetGrid;
    constructor(id: string, parent: DbNetEdit, gridControl: DbNetGrid) {
        super(id);
        this.gridControl = gridControl;
    }

    show(currentRow: number): void {
        this.open();
        this.selectRow(currentRow);
    }

    selectRow(currentRow:number): void {
        const selectedRow = this.gridControl.selectedRow() 
        //const topBottom = (selectedRow && currentRow < selectedRow.rowIndex) ? true : false;

        if (!selectedRow || currentRow != selectedRow.rowIndex) {
            const $tr = $(this.gridControl.table()).find('tr').eq(currentRow);
            this.gridControl.selectRow($tr);
            $tr[0].scrollIntoView();
        }
    }
}
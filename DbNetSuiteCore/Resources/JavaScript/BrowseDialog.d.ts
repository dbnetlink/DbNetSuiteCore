declare class BrowseDialog extends Dialog {
    gridControl: DbNetGrid;
    constructor(id: string, parent: DbNetEdit, gridControl: DbNetGrid);
    show(currentRow: number): void;
    selectRow(currentRow: number): void;
}

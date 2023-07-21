declare class BrowseDialog extends Dialog {
    parent: DbNetEdit;
    gridControl: DbNetGrid;
    constructor(id: string, parent: DbNetEdit, gridControl: DbNetGrid);
    show(): void;
    selectRow(currentRow: number): void;
}

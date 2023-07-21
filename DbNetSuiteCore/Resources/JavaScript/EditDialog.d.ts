declare class EditDialog extends Dialog {
    parent: DbNetGrid;
    editControl: DbNetEdit;
    constructor(id: string, parent: DbNetGrid, editControl: DbNetEdit);
    update(): void;
    insert(): void;
    private nextRecord;
    private previousRecord;
}

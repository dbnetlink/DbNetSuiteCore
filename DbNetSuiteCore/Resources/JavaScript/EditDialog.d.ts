declare class EditDialog extends Dialog {
    parent: DbNetGrid;
    constructor(id: string, parent: DbNetGrid);
    update(): void;
    private setHeight;
    private nextRecord;
    private previousRecord;
}

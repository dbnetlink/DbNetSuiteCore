/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class ViewDialog extends Dialog {
    parent: DbNetGrid;
    dialogWidth: number;
    constructor(id: string, parent: DbNetGrid);
    update(response: DbNetGridResponse, $row: JQuery<HTMLTableRowElement>): void;
    viewElement(columnName: string): HTMLElement | undefined;
    private dialogOpened;
    private setSize;
    private viewNextRecord;
    private viewPreviousRecord;
}

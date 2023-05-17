/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class ViewDialog extends Dialog {
    parent: DbNetGrid;
    constructor(id: string, parent: DbNetGrid);
    update(response: DbNetGridResponse, $row: JQuery<HTMLTableRowElement>): void;
    private setHeight;
    private viewNextRecord;
    private viewPreviousRecord;
}

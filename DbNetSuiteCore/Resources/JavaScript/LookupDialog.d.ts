/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class LookupDialog extends Dialog {
    parent: DbNetGrid;
    $input: JQuery<HTMLInputElement> | undefined;
    $select: JQuery<HTMLSelectElement> | undefined;
    columnIndex: number | undefined;
    title: string;
    constructor(id: string, parent: DbNetGrid);
    private dialogOpened;
    update(response: DbNetGridResponse, $input: JQuery<HTMLInputElement>): void;
    private apply;
}

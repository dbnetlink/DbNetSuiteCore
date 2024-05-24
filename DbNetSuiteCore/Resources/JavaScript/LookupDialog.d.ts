/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
declare class LookupDialog extends Dialog {
    parent: DbNetGridEdit;
    $input: JQuery<HTMLInputElement> | undefined;
    $select: JQuery<HTMLSelectElement> | undefined;
    columnIndex: number | undefined;
    title: string;
    constructor(id: string, parent: DbNetGridEdit);
    private dialogOpened;
    update(response: DbNetSuiteResponse, $input: JQuery<HTMLInputElement>): void;
    private contextIsEdit;
    private apply;
}

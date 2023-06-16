/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class DbNetCombo extends DbNetSuite {
    addEmptyOption: boolean;
    addFilter: boolean;
    autoRowSelect: boolean;
    currentValue: string;
    emptyOptionText: string;
    fromPart: string;
    filterDelay: number;
    filterMinChars: number;
    filterTimerId: number | undefined;
    filterToken: string;
    foreignKeyColumn: string;
    foreignKeyValue: Array<string> | undefined;
    linkedControls: Array<DbNetSuite>;
    multipleSelect: boolean;
    params: Dictionary<object>;
    size: number;
    textColumn: string;
    valueColumn: string;
    $select: JQuery<HTMLSelectElement> | undefined;
    constructor(id: string);
    initialize(): void;
    addLinkedControl(control: DbNetSuite): void;
    reload(): void;
    private configureCombo;
    private optionSelected;
    configureLinkedCombo(combo: DbNetCombo, fk: Array<string>): void;
    private filterKeyPress;
    private applyFilter;
    callServer(action: string): void;
    private getRequest;
    private post;
}

/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class DbNetCombo extends DbNetSuite {
    addEmptyOption: boolean;
    addFilter: boolean;
    currentValue: string;
    emptyOptionText: string;
    linkedControls: Array<DbNetSuite>;
    fromPart: string;
    valueColumn: string;
    textColumn: string;
    params: Dictionary<object>;
    filterDelay: number;
    filterMinChars: number;
    filterTimerId: number | undefined;
    filterToken: string;
    foreignKeyColumn: string;
    foreignKeyValue: object;
    $select: JQuery<HTMLSelectElement> | undefined;
    constructor(id: string);
    initialize(): void;
    addLinkedControl(control: DbNetSuite): void;
    reload(): void;
    private configureCombo;
    private optionSelected;
    configureLinkedCombo(combo: DbNetCombo, fk: object): void;
    clear(): void;
    private filterKeyPress;
    private applyFilter;
    callServer(action: string): void;
    private getRequest;
    private post;
}

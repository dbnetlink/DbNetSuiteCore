/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class DbNetCombo extends DbNetSuite {
    addEmptyOption: boolean;
    addFilter: boolean;
    autoRowSelect: boolean;
    currentValue: string;
    dataOnlyColumns: Array<string>;
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
    procedureParams: Dictionary<object>;
    procedureName: string;
    size: number;
    textColumn: string;
    valueColumn: string;
    $select: JQuery<HTMLSelectElement> | undefined;
    $filter: JQuery<HTMLInputElement> | undefined;
    constructor(id: string);
    initialize(): void;
    addLinkedControl(control: DbNetSuite): void;
    reload(): void;
    selectedValues(): string[];
    selectedOptions(): HTMLOptionElement[];
    private configureCombo;
    private optionSelected;
    private getSelectedValues;
    private getSelectedOptions;
    private getOptions;
    configureLinkedCombo(combo: DbNetCombo, fk: Array<string>): void;
    private filterKeyPress;
    private applyFilter;
    callServer(action: string): void;
    private getRequest;
    private post;
}

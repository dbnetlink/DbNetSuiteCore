/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
declare class DbNetCombo extends DbNetSuite {
    addEmptyOption: boolean;
    addFilter: boolean;
    autoRowSelect: boolean;
    comboPanel: JQuery<HTMLElement> | undefined;
    currentValue: string;
    dataOnlyColumns: Array<string>;
    distinct: boolean;
    emptyOptionText: string;
    fromPart: string;
    filterDelay: number;
    filterMinChars: number;
    filterTimerId: number | undefined;
    filterToken: string;
    foreignKeyColumn: string;
    foreignKeyValue: Array<string> | undefined;
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
    reload(): void;
    selectedValues(): string[];
    selectedOptions(): HTMLOptionElement[];
    private configureCombo;
    private optionSelected;
    private getSelectedValues;
    private getSelectedOptions;
    private getOptions;
    configureLinkedControl(control: DbNetSuite, fk: Array<string>, pk: string | null): void;
    configureEditButtons(edit: DbNetEdit): void;
    initialiseEdit(sender: DbNetEdit): void;
    private selectedOption;
    private nextOption;
    private previousOption;
    private filterKeyPress;
    private applyFilter;
    callServer(action: string): void;
    private getRequest;
}

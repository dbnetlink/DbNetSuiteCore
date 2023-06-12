/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class DbNetCombo extends DbNetSuite {
    id: string;
    connectionString: string;
    initialised: boolean;
    element: JQuery<HTMLElement>;
    linkedControls: Array<DbNetSuite>;
    addEmptyOption: boolean;
    emptyOptionText: string;
    sql: string;
    params: Dictionary<object>;
    currentValue: string;
    constructor(id: string);
    initialize(): void;
    addLinkedControl(control: DbNetGrid): void;
    reload(): void;
    private configureCombo;
    getPage(): void;
    private getRequest;
    private post;
}
